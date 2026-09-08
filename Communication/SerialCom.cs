using OpenTap;
using static InterconnectIOBox.GPIO.GpioIO;
using static InterconnectIOBox.Digital.PortsIO;
using System;
using System.Xml.Linq;
using static InterconnectIOBox.GPIO.GpioPAD;
using System.Collections.Generic;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using InterconnectIOBox.Instruments;
using InterconnectIOBox.Analysis;

namespace InterconnectIOBox.Communication
{
    [Display(Groups: new[] { "FTS_Interconnect", "Communication" }, Name: "Serial Data Write/Read", Description: "Write and/or read data on the serial port." +
    " Serial must be enabled for operation as a serial port.")]

    public class SerialCom : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        [Output]
        [Display("Measure:", Description: "The last string read from the serial port, shown after the step has run.")]
        public string Measure { get; private set; }

        public InterconnectIO IO_Instrument { get; set; }

        // Renamed from "Action" to avoid ambiguity with the built-in System.Action delegate type.
        public enum SerialAction
        {
            Write_read,
            Write_only,
            read_only,
            read_test
        }

        [Display("Action to Execute:", Group: "Serial Communication", Order: 0.1,
            Description: "Action to perform on the serial string.\n" +
                     "Write_read: Write a string and read the answer. The answer is compared with the expected read and the result is published.\n" +
                     "Write_only: Write a string and exit.\n" +
                     "Read_only: Read a string and publish, with no pass/fail comparison.\n" +
                     "Read_test: Read a string and compare with the expected data. The data is published.")]

        public SerialAction SerialAct { get; set; }

        private const string GROUPD = "Serial Communication Transfer";

        [Display("Serial Data Write:", Group: GROUPD, Order: 2, Description: "String to send on the serial port.")]
        [EnabledIf(nameof(SerialAct), new object[] { SerialAction.Write_read, SerialAction.Write_only }, HideIfDisabled = false)]
        public string wdata { get; set; }


        [Display("Serial Data Read:", Group: GROUPD, Order: 3, Description: "The specific substring or pattern expected to be contained within the serial read data. Data will be compared and published.")]
        [EnabledIf(nameof(SerialAct), new object[] { SerialAction.Write_read, SerialAction.read_only, SerialAction.read_test }, HideIfDisabled = false)]
        public string rdata { get; set; }



        public SerialCom()
        {
            // ToDo: Set default values for properties / settings.
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        public override void Run()
        {

            string Command = "";

            // Write only command
            if (SerialAct == SerialAction.Write_only)
            {
                Command = $"COM:SERIAL:WRITE '{wdata}'";
                Log.Info($"Write Only, Sending SCPI command: {Command}");

                IO_Instrument.ScpiCommand(Command);
                UpgradeVerdict(Verdict.Pass);
                return;
            }

            string test = "";
            string response = "";

            if (SerialAct == SerialAction.Write_read)
            {
                Command = $"COM:SERIAL:READ? \"{wdata}\""; // write and read
            }
            else
            {
                Command = $"COM:SERIAL:READ?"; // read_test & read_only
            }


            // Send command
            try
            {

                response = IO_Instrument.ScpiQuery<string>(Command);
                Log.Info($"SCPI query: {Command}, Answer: {response}");
            }
            catch (TimeoutException ex)
            {
                Log.Error($"Timeout exception: {ex.Message}");
                UpgradeVerdict(Verdict.Error);
                return;
            }


            string readP = response.Replace("\"", "").Trim();

            if (SerialAct != SerialAction.read_only)
            {
                if (readP.Contains(rdata)) // compare String
                {
                    test = "PASS";
                }
                else
                {
                    Log.Warning($"Invalid Serial response: {readP}, expected: {rdata}");
                    test = "FAIL";
                }
            }
            else
            {
                test = "PASS"; // Default verdict on read only
            }


            if (test == "PASS") UpgradeVerdict(Verdict.Pass);
            else UpgradeVerdict(Verdict.Fail);

            Measure = readP;

            // Create test result object
            TestResult<string> result = new TestResult<string>
            {
                ParamName = "Serial Data",
                StepName = Name,
                Value = readP,
                Verdict = test,
                Units = "read"
            };

            // Add limits for Write_Read and Read_test functions
            if (SerialAct == SerialAction.Write_read || SerialAct == SerialAction.read_test)
            {
                result.LowerLimit = rdata;
                result.UpperLimit = rdata;
                result.Units = "strcmp";
            }

            PublishResult(result);

        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}