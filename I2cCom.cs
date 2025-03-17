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
using System.Globalization;

namespace InterconnectIOBox
{


    [Display(Groups: new[] { "InterconnectIO", "Communication", "I2C" }, Name: "I2C Data Write/Read", Description: "Write/read data on I2C port. I2C must be enabled for operation.")]

    public class ItcCom : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        [Output]
        [Display("I2C_Data_Read:")]
        public string ItcReadData { get; private set; }


        public InterconnectIO IO_Instrument { get; set; }
        public enum Action
        {
            Write_read,
            Write_only,
            read_only,
            read_test
        }

        [Display(
        "Action to Execute:", Group: "I2C Communication", Order: 0.1,
        Description: "Action to perform on the I2C Data.\n" +
                     "Write_read: Write data and read answer. Answer is compared with expected read and result is published.\n" +
                     "Write_only: Write data and exit.\n" +
                     "Read_only: Read data and publish.\n" +
                     "Read_test: Read data and compare with expected data. Data is published")]

        
        public Action I2CAct { get; set; }


        [Display("I2C Number Write/Read:", Order: 0.2, Group: "I2C Communication", Description: "Define if I2C COM will use number or string to Write/Read data")]
        public bool Enum
        {
            get => _Enum;
            set
            {
                _Enum = value;
                if (value) Estr = false;
                OnPropertyChanged(nameof(Enum));
                OnPropertyChanged(nameof(Estr));
            }
        }
        private bool _Enum;

        [Display("I2C string W/R:", Order: 2, Group: "I2C Communication", Description: "Define if I2C COM will use number or string to Write/Read data")]
        public bool Estr
        {
            get => _Estr;
            set
            {
                _Estr = value;
                if (value) Enum = false;
                OnPropertyChanged(nameof(Enum));
                OnPropertyChanged(nameof(Estr));
            }
        }
        private bool _Estr;

        private const string GROUPS = "I2C number Write/Read Register";

        [Display("I2C Data Write/Read Register:", Group: GROUPS, Order: 2.1, Collapsed: true, Description: "Which register will be used to read or write.")]
        [EnabledIf("Enum", true, Flags = false)]
        public int rwdata { get; set; }

        [Display("I2C Data Write on Register:", Group: GROUPS, Order: 2.2, Collapsed: true, Description: " Data to write on specified register")]
        [EnabledIf(nameof(I2CAct), new object[] { Action.Write_read, Action.Write_only }, HideIfDisabled = false)]
        [EnabledIf("Enum", true, Flags = false)]
        public int rgdata { get; set; }


        [Display("I2C Data Read Register Expected:", Group: GROUPS, Order: 2.3, Collapsed: true, Description: "Expected Data read from register. Data will be compared and published")]
        [EnabledIf(nameof(I2CAct), new object[] { Action.Write_read, Action.read_test }, HideIfDisabled = false)]
        [EnabledIf("Enum", true, Flags = false)]
        public double rrdata { get; set; }

        [Display("I2C Data Read Length:", Group: GROUPS, Order: 2.4, Collapsed: true, Description: "Set number of Data to read on I2C Port. Will be byte if Byte if Databits = 8 and Word if Databits = 16")]
        [EnabledIf(nameof(I2CAct), new object[] { Action.Write_read, Action.read_test, Action.read_only }, HideIfDisabled = false)]
        [EnabledIf("Enum", true, Flags = false)]
        public int lgdata { get; set; } = 1;

        private const string GROUPG = "I2C string Write/Read Generic ";

        [Display("I2C Data Write:", Group: GROUPG, Order: 3, Collapsed: true, Description: "Send data over the I2C port using a literal string.")]
        [EnabledIf("Estr", true, Flags = false)]
        public string wdata { get; set; }

        [Display("I2C Read Data:", Group: GROUPG, Order: 3.1, Collapsed: true, Description: "Read Data on I2C Port using specified data length.")]
        [EnabledIf("Estr", true, Flags = false)]
        public string rdata { get; set; }

        [Display("I2C Data Read Length:", Group: GROUPG, Order: 3.2, Collapsed: true, Description: "Set number of Data to read on I2C Port. Will be byte if Byte if Databits = 8 and Word if Databits = 16")]
        [EnabledIf(nameof(I2CAct), new object[] { Action.Write_read, Action.read_test, Action.read_only }, HideIfDisabled = false)]
        [EnabledIf("Estr", true, Flags = false)]
        public int ldata { get; set; } = 1;

        public ItcCom()
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

            Log.Info("I2C Communication Transfer");


            // Write only command
            if (I2CAct == Action.Write_only)
            {
                if (Enum)
                {
                    Command = $"COM:I2C:WRITE {rwdata},{rgdata}";
                }
                else
                {
                    Command = $"COM:I2C:WRITE {wdata}";
                }
                Log.Info($"Write Only, Sending SCPI command: {Command}");

                IO_Instrument.ScpiCommand(Command);
                UpgradeVerdict(Verdict.Pass);
                return;
            }

            // READ Command

            string test = "";
            string response = "";

            if (Enum)
            {
       
                Command = $"COM:I2C:READ:LEN{lgdata}? {rwdata}"; // write on which register we want to read               }
            }
            else
            {
                Command = $"COM:I2C:READ:LEN{ldata}? {wdata}"; // write
            }

            // Send command
            try
            {
                response = IO_Instrument.ScpiQuery<string>(Command);
            }
            catch (TimeoutException ex)
            {
                Log.Error($"Timeout exception: {ex.Message}");
                UpgradeVerdict(Verdict.Error);
                return;
            }

            Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

            // In case of I2C string
            if (Estr)
            // if string
            {
                if (I2CAct != Action.read_only)
                {
                    if (response == rdata) // compare 
                    {
                        test = "PASS";
                    }
                    else
                    {
                        Log.Warning($"Invalid I2C response: {response}, expected: {rdata}");
                        test = "FAIL";
                    }
                }
                else
                {
                    test = "PASS"; // Default verdict on read only
                }
                if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                else UpgradeVerdict(Verdict.Fail);

                ItcReadData = response;

                if (I2CAct != Action.read_only) // if publish required
                {
                    // Create test result object
                    TestResult<string> result = new TestResult<string>
                    {
                        ParamName = $"I2C String Data:",
                        StepName = Name,
                        Value = response,
                        Verdict = test,
                        Units = "read"
                    };

                    // Add limits for Write_Read function
                    if (I2CAct == Action.Write_read || I2CAct == Action.read_test)
                    {
                        result.LowerLimit = rdata;
                        result.UpperLimit = rdata;
                        result.Units = "strcmp";
                    }

                    PublishResult(result);
                    return;
                }
            }

            // If Numeric I2C
            double readP = double.Parse(response.Replace("\"", "").Trim(), CultureInfo.InvariantCulture);

            if (I2CAct != Action.read_only)
            {
                if (readP == rrdata) // compare 
                {
                    test = "PASS";
                }
                else
                {
                    Log.Warning($"Invalid I2C response: {readP}, expected: {rrdata}");
                    test = "FAIL";
                }
            }
            else
            {
                test = "PASS"; // Default verdict on read only
            }


            if (test == "PASS") UpgradeVerdict(Verdict.Pass);
            else UpgradeVerdict(Verdict.Fail);

            ItcReadData = readP.ToString();

            if (I2CAct != Action.read_only) // if publish required
            {
                // Create test result object
                TestResult<double> result = new TestResult<double>
                {
                    ParamName = $"I2C Data:",
                    StepName = Name,
                    Value = readP,
                    Verdict = test,
                    Units = "read"
                };

                // Add limits for Write_Read function
                if (I2CAct == Action.Write_read || I2CAct == Action.read_test)
                {
                    result.LowerLimit = rrdata;
                    result.UpperLimit = rrdata;
                    result.Units = "digcmp";
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
