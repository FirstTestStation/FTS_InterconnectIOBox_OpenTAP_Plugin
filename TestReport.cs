using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace InterconnectIOBox
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
            string DUTName = Dut?.ProductName ?? "Unknown";
            string DUTNumber = Dut?.PartNumber ?? "Unknown";
            string DUTSerial = Dut?.SerialNumber ?? "Unknown";
            string FixtName = Dut?.FixtName ?? "Unknown";
            string FixtNumber = Dut?.FixtNumber ?? "Unknown";
            string FixtSerial = Dut?.FixtSerial ?? "Unknown";
            string DutID = Dut?.ID ?? "Unknown";

            string TableName = $"{DUTName}_{DUTSerial}";

            // Safely cast generics to IConvertible or fallback to empty string
            IConvertible value = result.Value != null ? result.Value : (IConvertible)string.Empty;
            IConvertible lower = result.LowerLimit != null ? result.LowerLimit : (IConvertible)string.Empty;
            IConvertible upper = result.UpperLimit != null ? result.UpperLimit : (IConvertible)string.Empty;

            Results.Publish(
                TableName,
                new List<string>
                {
                    "FixtureName", "FixtureNumber", "FixtureSerial", "ID",
                    "ProductName", "ProductNumber", "SerialNumber",
                    "StepName", "Parameter", "Value", "LowerLimit", "UpperLimit", "Units", "Status"
                },
                FixtName,
                FixtNumber,
                FixtSerial,
                DutID,
                DUTName,
                DUTNumber,
                DUTSerial,
                result.StepName,
                result.ParamName,
                value,
                lower,
                upper,
                result.Units ?? string.Empty,
                result.Verdict.ToUpper()
            );

            Log.Info($"Published result for '{result.ParamName}' under SerialNumber '{DUTSerial}'.");
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
