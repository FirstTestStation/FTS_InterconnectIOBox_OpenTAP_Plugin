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
 
    [Display(Groups: new[] { "InterconnectIO", "1-Wire" }, Name: "1-Wire Read", Description: "Read and validate the quantity of 1-Wire devices on the DUT. The quantity and contents are both verified.")]
    public class OneWireRead : ResultTestStep
    {


        public InterconnectIO IO_Instrument { get; set; }
        public enum Wmode
        {
            read_only,
            read_test
        }

        [Display("Action to Execute:", Group: "1-Wire Test", Order: 0.1, Description: "Action to perform on 1-wire, in case of Read_test, the result will be published.")]
        public Wmode WAct { get; set; }


        [Display("Number of 1-Wire devices", Group: "1-Wire Read", Order: 1, Description: "Number of 1-Wire devices expected on the Fixture.")]
        public int NbWire { get; set; } = 1;

        [Display("Check String device #1", Order: 1.2)]
        public Enabled<string> ReadStra { get; set; }

        [Display("Check String device #2", Order: 1.3)]
        public Enabled<string> ReadStrb { get; set; }



    /// <summary>
    /// Sends a SCPI command to read 1-Wire devices on the DUT.
    /// </summary>
    /// <param int= qty> Number of 1-Wire to read.</param>
    /// <returns>The data read from the 1-Wire interface.</returns>
    public string ReadOneWires(int qty)
        {

            string command = $"COM:OWIRE:READ? {qty}";
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
        public void CheckOneWireData(string actual, string expected, int index)
        {
            string test = "";
            bool valid = actual.Contains(expected.ToString());
            if (valid)
            {
                Log.Info($"1-Wire data is valid for string: {expected}");
                UpgradeVerdict(Verdict.Pass);
                test = "PASS";
                // Split by spaces or commas (modify as needed)
                string[] parts = actual.Split(new char[] { ']' }, StringSplitOptions.RemoveEmptyEntries);

                // Loop through each part to check for the expected string
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Contains(expected))
                    {
                        string rst = parts[i].Replace("\"", "").Replace("[", "");
                        actual = rst; // Set actual to the result for publishing better information
                        break;
                    }
                }


            }
            else
            {
                Log.Warning($"1-Wire data is not valid on read string: {expected}.");
                UpgradeVerdict(Verdict.Fail);
                test = "FAIL";
            }



            // Publish final result
            var result = new TestResult<string>
            {
                ParamName = $"1-Wire Read data #{index}",
                StepName = Name,
                Value = actual,
                LowerLimit = expected,
                UpperLimit = expected,
                Verdict = test,
                Units = "strcmp"
            };

            PublishResult(result);
        }

    public OneWireRead()
        {
            // ToDo: Set default values for properties / settings.
            ReadStra = new Enabled<string> { Value = "J1" };
            ReadStrb = new Enabled<string> { Value = "J2" };
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();

            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        public override void Run()
        {
  
            string test = "";

            string sdata = ReadOneWires(NbWire);
            int count = Regex.Matches(sdata, "2D").Count;

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
                ParamName = "Number of 1-Wire read",
                StepName = Name,
                Value = count,
                LowerLimit = NbWire,
                UpperLimit = NbWire,
                Verdict = test,
                Units = "digcmp"
            };

            PublishResult(result);

            // From data read on 1-Wire device, validate the contents

            if (ReadStra.IsEnabled)
            {
                CheckOneWireData(sdata, ReadStra.ToString(), 1);
            }

            if (ReadStrb.IsEnabled)
            {
                CheckOneWireData(sdata, ReadStrb.ToString(), 2);
            }

        
        }


        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }

    }


}

