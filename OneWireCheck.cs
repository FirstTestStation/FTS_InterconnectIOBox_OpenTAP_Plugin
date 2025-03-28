using OpenTap;
using OpenTap.Diagnostic;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Security;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using static InterconnectIOBox.Selftest_com;
using static System.Net.Mime.MediaTypeNames;

namespace InterconnectIOBox // DUT Validation
{
 
    [Display(Groups: new[] { "InterconnectIO", "1-Wire" }, Name: "1-Wire Check", Description: "Check and validate the quantity of 1-Wire devices on the DUT. The quantity and contents are both verified.")]
    public class OneWireGen : ResultTestStep
    {


        public InterconnectIO IO_Instrument { get; set; }
        public enum Wmode
        {
            read_only,
            read_test
        }

        [Display("Action to Execute:", Group: "1-Wire Test", Order: 0.1, Description: "Action to perform on 1-wire, in case of Read_test, the result will be published.")]
        public Wmode WAct { get; set; }


        [Display("Number of 1-Wire devices", Group: "1-Wire Test", Order: 1, Description: "Number of 1-Wire devices expected on the DUT.")]
        public int NbWire { get; set; } = 1;

        [Display("Check String #1", Order: 1.2)]
        public Enabled<string> CheckStra { get; set; }

        [Display("Check String #2", Order: 1.3)]
        public Enabled<string> CheckStrb { get; set; }



    /// <summary>
    /// Sends a SCPI command to read 1-Wire devices on the DUT.
    /// </summary>
    /// <param int= qty> Number of 1-Wire to read.</param>
    /// <returns>The data read from the 1-Wire interface.</returns>
    public string ReadOneWires(int qty)
        {

            string command = $"COM:OWIRE:CHECK? {qty}";
            Log.Info($"Sending SCPI command: {command}");

            // Use ScpiQuery to read back from the device.
            string response = IO_Instrument.ScpiQuery<string>(command);

            Log.Info($"1-Wire received response: {response}");
            return response;
        }

        /// <summary>
        /// Compares actual and expected SCPI command responses for 1-Wire devices on the DUT.
        /// </summary>
        /// <param name="actual">The actual data read from the 1-Wire interface.</param>
        /// <param name="expected">The expected data to compare against.</param>
        /// <param name="index">A number to add on ParamName.</param>
        public void CheckOneWireData(string actual, string expected, int idx)
        {
            string test = "";
            bool valid = actual.Contains(expected.ToString());
            if (valid)
            {
                Log.Info($"1-Wire data is valid for string: {expected}");
                UpgradeVerdict(Verdict.Pass);
                test = "PASS";
            }
            else
            {
                Log.Error($"1-Wire data is not valid on check string: {expected}.");
                UpgradeVerdict(Verdict.Fail);
                test = "FAIL";
            }

            // Publish final result
            var result = new TestResult<string>
            {
                ParamName = $"1-Wire Check data #{idx}",
                StepName = Name,
                Value = actual,
                LowerLimit = expected,
                UpperLimit = expected,
                Verdict = test,
                Units = "strcmp"
            };

            PublishResult(result);
        }

    public OneWireGen()
        {
            // ToDo: Set default values for properties / settings.
            CheckStra = new Enabled<string> { Value = "VALID" };
            CheckStrb = new Enabled<string> { Value = "NEXT" };
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();

            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        public override void Run()
        {
  
            string test = "FAIL";

            string sdata = ReadOneWires(NbWire);
            int count = Regex.Matches(sdata, "OWID").Count;

            if (WAct == Wmode.read_only)
            {
                Log.Info("Read 1-Wire data only.");
                UpgradeVerdict(Verdict.Pass);
                return;
            }

            if (count == NbWire)
            {
                Log.Info($"Number of 1-Wire devices detected is valid, count: {count}");
                UpgradeVerdict(Verdict.Pass);
                test = "PASS";
            }
            else
            {
                Log.Warning($"Number of 1-Wire devices detected is wrong, expected: {NbWire}, count: {count}");
                UpgradeVerdict(Verdict.Fail);
                test = "FAIL";
            }


            // Publish final result
            var result = new TestResult<double>
            {
                ParamName = "Number of 1-Wire Check",
                StepName = Name,
                Value = count,
                LowerLimit = NbWire,
                UpperLimit = NbWire,
                Verdict = test,
                Units = "digcmp"
            };

            PublishResult(result);

            // From data read on 1-Wire device, validate the contents

            if (CheckStra.IsEnabled)
            {
                CheckOneWireData(sdata, CheckStra.ToString(), 1);
            }

            if (CheckStrb.IsEnabled)
            {
                CheckOneWireData(sdata, CheckStrb.ToString(),2);
            }

        
        }


        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }

    }


}

