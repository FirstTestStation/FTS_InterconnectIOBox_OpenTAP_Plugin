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
        public T Value { get; set; }    // could be int,double or string
        public string Verdict { get; set; }
        public string Units { get; set; }
        public T LowerLimit { get; set; }
        public T UpperLimit { get; set; }
    }


    public abstract class ResultTestStep : TestStep
    {

        [Display("DUT", Group: "General", Order: 0, Description: "Reference to the DUT used in this test step")]
        public FTS_DUT Dut { get; set; }  // Assign once in the test plan

        public override void Run()
        {
            if (Dut == null)
            {
                Log.Error("DUT is not assigned to this step.");
                throw new ArgumentNullException(nameof(Dut), "DUT must be assigned.");
            }

            Log.Info("ResultTestStep.Run() was called.");
        }

        public void PublishResult<T>(TestResult<T> result) where T : IConvertible
        {
            if (Results == null)
            {
                Log.Error("Results object is null. Cannot publish result.");
                return;
            }

            // Prefer DUT info from step, fallback to result if null
            string DUTName = Dut?.ProductName ?? "Unknown";
            string DUTNumber = Dut?.PartNumber ?? "Unknown";
            string DUTSerial = Dut?.SerialNumber ?? "Unknown";
            string FixtName = Dut?.FixtName ?? "Unknown";
            string FixtNumber = Dut?.FixtNumber ?? "Unknown";
            string FixtSerial = Dut?.FixtSerial ?? "Unknown";
            string DutID = Dut?.ID ?? "Unknown";

            string TableName = DUTName + "_" + DUTSerial;

            Results.Publish(
                TableName,
                new List<string> { "FixtureName", "FixtureNumber", "FixtureSerial", "ID", "ProductName", "ProductNumber", "SerialNumber", "StepName", "Parameter", "Value", "LowerLimit", "UpperLimit", "Units", "Status" },
                FixtName,
                FixtNumber,
                FixtSerial,
                DutID,
                DUTName,
                DUTNumber,
                DUTSerial,
                result.StepName,
                result.ParamName,
                result.Value,
                result.LowerLimit,
                result.UpperLimit,
                result.Units ?? "",
                result.Verdict
            );
        }
    }
}

