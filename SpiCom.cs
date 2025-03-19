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


    [Display(Groups: new[] { "InterconnectIO", "Communication", "SPI" }, Name: "SPI Data Write/Read", Description: "Write/read data on SPI port.SPI must be enabled for operation as a serial port.")]
  
    public class SpiCom : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        [Output]
        [Display("Measure:")]
        public string Measure { get; private set; }


        public InterconnectIO IO_Instrument { get; set; }
        public enum Action
        {
            Write_read,
            Write_only,
            read_only,
            read_test
        }

        [Display(
        "Action to Execute:", Group: "SPI Communication", Order: 0.1,
        Description: "Action to perform on the SPI Data.\n" +
                     "Write_read: Write data and read answer. Answer is compared with expected read and result is published.\n" +
                     "Write_only: Write data and exit.\n" +
                     "Read_only: Read data and publish.\n" +
                     "Read_test: Read data and compare with expected data. Data is published")]

        public Action SPIAct { get; set; }


        [Display("SPI Number Write/Read:", Order: 0.2, Group: "SPI Communication", Description: "Define if SPI COM will use number or string to Write/Read data")]
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

        [Display("SPI string W/R:", Order: 2, Group: "SPI Communication", Description: "Define if SPI COM will use number or string to Write/Read data")]
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

        private const string GROUPS = "SPI number Write/Read Register";

        [Display("SPI Data Write/Read Register:", Group: GROUPS, Order: 2.1, Collapsed: true, Description: "Which register will be used to read or write. In case of Read the bit 7 of register number will be set to 1 to address register before perform the read.")]
        [EnabledIf("Enum", true, Flags = false)]
        public int rwdata { get; set; }

        [Display("SPI Data Write on Register:", Group: GROUPS, Order: 2.2, Collapsed: true, Description: " Data to write on specified register")]
        [EnabledIf(nameof(SPIAct), new object[] { Action.Write_read, Action.Write_only }, HideIfDisabled = false)]
        [EnabledIf("Enum", true, Flags = false)]
        public int rgdata { get; set; }


        [Display("SPI Data Read Register Expected:", Group: GROUPS, Order: 2.3, Collapsed: true, Description: "Expected Data read from register. Data will be compared and published")]
        [EnabledIf(nameof(SPIAct), new object[] { Action.Write_read, Action.read_test }, HideIfDisabled = false)]
        [EnabledIf("Enum", true, Flags = false)]
        public double rrdata { get; set; }

        [Display("SPI Data Read Length:", Group: GROUPS, Order: 2.4, Collapsed: true, Description: "Set number of Data to read on SPI Port. Will be byte if Byte if Databits = 8 and Word if Databits = 16")]
        [EnabledIf(nameof(SPIAct), new object[] { Action.Write_read, Action.read_test, Action.read_only }, HideIfDisabled = false)]
        [EnabledIf("Enum", true, Flags = false)]
        public int lgdata { get; set; } = 1;

        private const string GROUPG = "SPI string Write/Read Generic ";

        [Display("SPI Data Write:", Group: GROUPG, Order: 3, Collapsed: true, Description: "Send data over the SPI port using a literal string. Bit 7 is not set for write operations.")]
        [EnabledIf("Estr", true, Flags = false)]
        public string wdata { get; set; }

        [Display("SPI Read Data:", Group: GROUPG, Order: 3.1, Collapsed: true, Description: "Read Data on SPI Port using specified data length.")]
        [EnabledIf("Estr", true, Flags = false)]
        public string rdata { get; set; }

        [Display("SPI Data Read Length:", Group: GROUPG, Order: 3.2, Collapsed: true, Description: "Set number of Data to read on SPI Port. Will be byte if Byte if Databits = 8 and Word if Databits = 16")]
        [EnabledIf(nameof(SPIAct), new object[] { Action.Write_read, Action.read_test, Action.read_only }, HideIfDisabled = false)]
        [EnabledIf("Estr", true, Flags = false)]
        public int ldata { get; set; } = 1;

        public SpiCom()
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

            Log.Info("SPI Communication Transfer");


            // Write only command
            if (SPIAct == Action.Write_only)
            {
                if (Enum)
                {
                    Command = $"COM:SPI:WRITE {rwdata},{rgdata}";
                }
                else
                {
                    Command = $"COM:SPI:WRITE {wdata}";
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
                    byte wreg = (byte)(rwdata | 0x80);
                    Command = $"COM:SPI:READ:LEN{lgdata}? {wreg}"; // write on which register we want to read
                    Log.Info($"register: {rwdata} become: {wreg} with bit 7 added");                                        // 
                // }
            }
            else
                {
                    Command = $"COM:SPI:READ:LEN{ldata}? {wdata}"; // write
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

            // In case of SPI string
            if (Estr)
            // if string
            {
                if (SPIAct != Action.read_only)
                {
                    if (response == rdata) // compare 
                    {
                        test = "PASS";
                    }
                    else
                    {
                        Log.Warning($"Invalid SPI string response: {response}, expected: {rdata}");
                        test = "FAIL";
                    }
                }
                else
                {
                    test = "PASS"; // Default verdict on read only
                }
                if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                else UpgradeVerdict(Verdict.Fail);

                Measure = response;

                if (SPIAct != Action.read_only) // if publish required
                {
                    // Create test result object
                    TestResult<string> result = new TestResult<string>
                    {
                        ParamName = $"SPI String Data:",
                        StepName = Name,
                        Value = response,
                        Verdict = test,
                        Units = "read"
                    };

                    // Add limits for Write_Read function
                    if (SPIAct == Action.Write_read || SPIAct == Action.read_test)
                    {
                        result.LowerLimit = rdata;
                        result.UpperLimit = rdata;
                        result.Units = "strcmp";
                    }

                    PublishResult(result);
                    return;
                }
            }

            // If Numeric SPI
            double readP = double.Parse(response.Replace("\"", "").Trim(), CultureInfo.InvariantCulture);

            if (SPIAct != Action.read_only)
            {
                if (readP == rrdata) // compare 
                {
                    test = "PASS";
                }
                else
                {
                    Log.Warning($"Invalid SPI Number response: {readP}, expected: {rrdata}");
                    test = "FAIL";
                }
            }
            else
            {
                test = "PASS"; // Default verdict on read only
            }


            if (test == "PASS") UpgradeVerdict(Verdict.Pass);
            else UpgradeVerdict(Verdict.Fail);

            Measure = readP.ToString();

            if (SPIAct != Action.read_only) // if publish required
            {
                // Create test result object
                TestResult<double> result = new TestResult<double>
                {
                    ParamName = $"SPI Data:",
                    StepName = Name,
                    Value = readP,
                    Verdict = test,
                    Units = "read"
                };

                // Add limits for Write_Read function
                if (SPIAct == Action.Write_read || SPIAct == Action.read_test)
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
