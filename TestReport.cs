using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace InterconnectIOBox
{
    public class TestResult<T> where T : IConvertible
    {
        public string DUTNumber { get; set; }
        public string DUTName { get; set; }
        public string DUTSerial { get; set; }

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

        [Browsable(false)]
        public FTS_DUT OWire_Dut { get; set; }

        public override void Run()
        {
            // You can either leave it empty or add logging for debugging


            Log.Info("ResultTestStep.Run() was called.");
        }
        public void PublishResult<T>(TestResult<T> result) where T : IConvertible
        {
            if (Results == null)
            {
                Log.Error("Results object is null. Cannot publish result.");
                return;
            }

            string DUTName = OWire_Dut.ProductName;
            string DUTNumber = OWire_Dut.PartNumber;
            string DUTSerial = OWire_Dut.SerialNumber;
            string FixtName = OWire_Dut.FixtName;
            string FixtNumber = OWire_Dut.FixtNumber;
            string FixtSerial = OWire_Dut.FixtSerial;
            string DutID = OWire_Dut.ID;



            string TableName = DUTName + "_" + DUTSerial;


            // Parameter": "Voltage","current", "string","digital","analog"  
            // Status: "Pass", "Fail", "NotSet", "Unknown"

            Results.Publish(
                TableName,
                new List<string> { "FixtureName", "FixtureNumber", "FixtureSerial","ID", "ProductName", "ProductNumber", "SerialNumber","StepName", "Parameter", "Value", "LowerLimit", "UpperLimit", "Units", "Status" },
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

  