using OpenTap;
using static InterconnectIOBox.GpioIO;
using static InterconnectIOBox.PortsIO;
using System.Windows.Input;
using System;
using System.Xml.Linq;
using static InterconnectIOBox.GpioPAD;
using System.Collections.Generic;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace InterconnectIOBox
{
    [Display(Groups: new[] { "InterconnectIO", "Communication", "Serial" }, Name: "Serial Data Write/Read", Description: "Write/read data on Serial port." +
"Serial must be enabled for operation as a serial port.")]

    public class SerialCom : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        [Output]
        [Display("Serial_Data_Read:")]
        public string SerReadData { get; private set; }

        public InterconnectIO IO_Instrument { get; set; }
        public enum Action
        {
            Write_read,
            Write_only,
            read_only,
            read_test
        }

        [Display("Action to Execute:", Group: "Serial Communication", Order: 0.1,
            Description: "Action to perform on the Serial string.\n" +
                     "Write_read: Write string and read answer. Answer is compared with expected read and result is published.\n" +
                     "Write_only: Write string and exit.\n" +
                     "Read_only: Read string and publish.\n" +
                     "Read_test: Read string and compare with expected data. Data is published")]
     
        public Action SerialAct { get; set; }

        public enum Eol // End of line character
        {
            LF,
            CR,
            LFCR
        }

        [Display("End-Of-Line Character:", Group: "Serial string EOL", Order: 0.2, Description: "Set the end character to be added on string to write.")]
        public Eol Lchar { get; set; }


        private const string GROUPD = "Serial Communication Transfer";

        [Display("Serial Data Write:", Group: GROUPD, Order: 2, Description: "Send String on serial Port")]
        [EnabledIf(nameof(SerialAct), new object[] { Action.Write_read, Action.Write_only }, HideIfDisabled = false)]
        public string wdata { get; set; }


        [Display("Serial Data Read:", Group: GROUPD, Order: 3, Description: "Expected Data String on read serial Port. Data will be compared and published")]
        [EnabledIf(nameof(SerialAct), new object[] { Action.Write_read, Action.read_only, Action.Write_read }, HideIfDisabled = false)]
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
            if (SerialAct == Action.Write_only)
            {
                Command = $"COM:SERIAL:WRITE \'{wdata}\'";
                Log.Info($"Write Only, Sending SCPI command: {Command}");

                IO_Instrument.ScpiCommand(Command);
                UpgradeVerdict(Verdict.Pass);
                return;
            }

            string test = "";
            string response = "";  

            if (SerialAct == Action.Write_read)
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
                Log.Info($"Sending SCPI command: {Command}, Answer: {response}");
            }
            catch (TimeoutException ex)
            {
                Log.Error($"Timeout exception: {ex.Message}");
                UpgradeVerdict(Verdict.Error);
                return;
            }


            string readP = response.Replace("\"", "").Trim();

            if (SerialAct != Action.read_only)
            {
                if (readP == rdata) // compare String
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

            SerReadData = readP;

            if (SerialAct != Action.read_only) // if publish required
            {
                // Create test result object
                TestResult<string> result = new TestResult<string>
                {
                    ParamName = $"Serial Data:",
                    StepName = Name,
                    Value = readP,
                    Verdict = test,
                    Units = "read"
                };

                // Add limits for Write_Read function
                if (SerialAct == Action.Write_read || SerialAct == Action.read_test)
                {
                    result.LowerLimit = rdata;
                    result.UpperLimit = rdata;
                    result.Units = "strcmp";
                }

                PublishResult(result);
            }

        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}
