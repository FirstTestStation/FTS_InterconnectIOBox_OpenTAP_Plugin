using InterconnectIOBox.Instruments;
using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [Display("DUT", Group: "General", Order: 0, Description: "Reference to the DUT used in this test step")]
        public FTS_DUT Dut { get; set; }  // Assigned once in the test plan

        protected string GetMeta(string key)
        {
            var p = PlanRun?.Parameters.FirstOrDefault(x => x.Name == key);
            return p?.Value?.ToString() ?? "";
        }


        // Temporary queue for results awaiting SerialNumber
        private static readonly List<object> PendingResults = new();

        public override void Run()
        {
            if (Dut == null)
            {
                Log.Error("DUT is not assigned to this step.");
                throw new ArgumentNullException(nameof(Dut), "DUT must be assigned.");
            }

            Log.Info("ResultTestStep.Run() called.");
        }

        public void PublishResult<T>(TestResult<T> result) where T : IConvertible
        {
            if (Results == null)
            {
                Log.Error("Results object is null. Cannot publish result.");
                return;
            }

            // If SerialNumber is not yet known → queue the result
            if (string.IsNullOrWhiteSpace(Dut?.SerialNumber))
            {
                Log.Info($"SerialNumber not assigned yet. Queuing result for '{result.ParamName}'.");
                lock (PendingResults)
                {
                    PendingResults.Add(result);
                }
                return;
            }

            // If SerialNumber is known → first flush old pending results
            if (PendingResults.Count > 0)
                FlushPendingResults();

            // Then publish current result immediately
            PublishToResults(result);
        }
        private void PublishToResults<T>(TestResult<T> result) where T : IConvertible
        {
            const string TableName = "TestResults";

            IConvertible value = result.Value ?? (IConvertible)string.Empty;
            IConvertible lower = result.LowerLimit ?? (IConvertible)string.Empty;
            IConvertible upper = result.UpperLimit ?? (IConvertible)string.Empty;

            // Fixed metadata keys that map to PlanRun.Parameters
            var metaKeys = new[]
            {
        "FixtureName", "FixtureNumber", "FixtureSerial", "ID",
        "ProductName", "ProductNumber", "SerialNumber"
    };

            var columns = new List<string>();
            var values = new List<IConvertible>();

            // Only include metadata columns that were actually published by SetupResultFile
            foreach (var key in metaKeys)
            {
                string meta = GetMeta(key);
                if (meta == "") continue;   // key absent → toggle was off → skip column
                columns.Add(key);
                values.Add(meta);
            }

            // Always include result columns
            columns.AddRange(new[] { "StepName", "Parameter", "Value", "LowerLimit", "UpperLimit", "Units", "Status" });
            values.AddRange(new IConvertible[]
            {
        result.StepName,
        result.ParamName,
        value,
        lower,
        upper,
        result.Units ?? string.Empty,
        result.Verdict.ToUpper()
            });

            Results.Publish(TableName, columns, values.ToArray());

          //  Log.Info($"Published '{result.ParamName}' SN:'{GetMeta("SerialNumber")}'");
        }

        private void FlushPendingResults()
        {
            if (PendingResults.Count == 0)
                return;

            lock (PendingResults)
            {
                Log.Info($"Flushing {PendingResults.Count} pending results now that SerialNumber = {Dut.SerialNumber}");

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
}
