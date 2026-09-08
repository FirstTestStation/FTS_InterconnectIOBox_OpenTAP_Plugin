using InterconnectIOBox.Instruments;
using OpenTap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InterconnectIOBox.Analysis
{
    public class TestResult<T> where T : IConvertible
    {
        public string StepName { get; set; }
        public string ParamName { get; set; }
        public T Value { get; set; }    // Could be int, double, or string
        public string Verdict { get; set; }
        public string Units { get; set; }
        public T LowerLimit { get; set; }
        public T UpperLimit { get; set; }
    }

    public abstract class ResultTestStep : TestStep
    {
        [Display("DUT", Group: "General", Order: -100, Description: "Reference to the DUT used in this test step.")]
        public FTS_DUT Dut { get; set; }  // Assigned once in the test plan

        // Steps that don't need a DUT reference (e.g. utility/config steps not
        // tied to a specific unit under test) can override this to false to
        // skip the mandatory-DUT check in PrePlanRun, while still using the
        // same SerialNumber-gated publishing pipeline for a single, unified report.
        protected virtual bool RequiresDut => true;

        protected string GetMeta(string key)
        {
            var p = PlanRun?.Parameters.FirstOrDefault(x => x.Name == key);
            return p?.Value?.ToString() ?? "";
        }


        // Temporary queue for results awaiting SerialNumber
        private static readonly List<object> PendingResults = new();

        // Tracks which plan run the pending queue belongs to, so stale
        // results from a previous run are never carried into a new one.
        private static TestPlanRun lastPlanRun;

        public override void PrePlanRun()
        {
            base.PrePlanRun();

            if (RequiresDut && Dut == null)
            {
                Log.Error("DUT is not assigned to this step.");
                throw new ArgumentNullException(nameof(Dut), "DUT must be assigned.");
            }
        }

        public void PublishResult<T>(TestResult<T> result) where T : IConvertible
        {
            if (Results == null)
            {
                Log.Error("Results object is null. Cannot publish result.");
                return;
            }

            lock (PendingResults)
            {
                // A new plan run started since the last publish → discard any
                // leftover pending results from the previous run.
                if (!ReferenceEquals(PlanRun, lastPlanRun))
                {
                    if (PendingResults.Count > 0)
                        Log.Warning($"Discarding {PendingResults.Count} pending result(s) from a previous test plan run.");

                    PendingResults.Clear();
                    lastPlanRun = PlanRun;
                }

                // If SerialNumber is not yet known → queue the result
                // (works even if Dut itself is null, thanks to the null-conditional below).
                if (string.IsNullOrWhiteSpace(Dut?.SerialNumber))
                {
                    Log.Info($"SerialNumber not assigned yet. Queuing result for '{result.ParamName}'.");
                    PendingResults.Add(result);
                    return;
                }

                // If SerialNumber is known → first flush old pending results
                if (PendingResults.Count > 0)
                    FlushPendingResults();
            }

            // Then publish current result immediately
            PublishToResults(result);
        }

        private void PublishToResults<T>(TestResult<T> result) where T : IConvertible
        {
            const string TableName = "TestResults";

            IConvertible value = result.Value ?? (IConvertible)string.Empty;
            IConvertible lower = result.LowerLimit ?? (IConvertible)string.Empty;
            IConvertible upper = result.UpperLimit ?? (IConvertible)string.Empty;

            // Metadata published BEFORE the result columns
            var leadingMetaKeys = new[] { "ID", "ProductName", "ProductNumber", "SerialNumber" };

            // Metadata published AFTER the result columns
            var trailingMetaKeys = new[] { "FixtureName", "FixtureNumber", "FixtureSerial" };

            var columns = new List<string>();
            var values = new List<IConvertible>();

            // 1. Leading metadata (ID, ProductName, ProductNumber, SerialNumber)
            foreach (var key in leadingMetaKeys)
            {
                string meta = GetMeta(key);
                if (meta == "") continue;   // key absent → toggle was off → skip column
                columns.Add(key);
                values.Add(meta);
            }

            // 2. Result columns
            columns.AddRange(new[] { "StepName", "Parameter", "Value", "LowerLimit", "UpperLimit", "Units", "Status" });
            values.AddRange(new IConvertible[]
            {
        result.StepName,
        result.ParamName,
        value,
        lower,
        upper,
        result.Units ?? string.Empty,
        result.Verdict?.ToUpper() ?? "UNKNOWN"
            });

            // 3. Trailing metadata (Fixture*)
            foreach (var key in trailingMetaKeys)
            {
                string meta = GetMeta(key);
                if (meta == "") continue;
                columns.Add(key);
                values.Add(meta);
            }

            Results.Publish(TableName, columns, values.ToArray());
        }

        private void FlushPendingResults()
        {
            // Note: caller already holds the lock on PendingResults.
            if (PendingResults.Count == 0)
                return;

            Log.Info($"Flushing {PendingResults.Count} pending results now that SerialNumber = {Dut?.SerialNumber}");

            foreach (var res in PendingResults)
            {
                switch (res)
                {
                    case TestResult<string> strRes:
                        PublishToResults(strRes);
                        break;
                    case TestResult<int> intRes:
                        PublishToResults(intRes);
                        break;
                    case TestResult<double> dblRes:
                        PublishToResults(dblRes);
                        break;
                    default:
                        Log.Warning($"Unsupported result type: {res.GetType()} — cannot publish.");
                        break;
                }
            }

            PendingResults.Clear();
        }
    }
}