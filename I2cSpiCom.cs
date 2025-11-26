using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Input;
using System.Xml.Linq;
using static InterconnectIOBox.GpioIO;
using static InterconnectIOBox.GpioPAD;
using static InterconnectIOBox.PortsIO;
using static System.Net.Mime.MediaTypeNames;

namespace InterconnectIOBox
{


    [Display(Groups: new[] { "InterconnectIO", "Communication",}, Name: "I2C/SPI Data W/R", Description: "Write/read data on register using selected protocol. Communication Protocol must be enabled for operation.")]

    public class I2cSpiCom : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion



        public InterconnectIO IO_Instrument { get; set; }
        public enum Action
        {
            Write_read,
            Write_only,
            read_only,
            read_test
        }

        [Display("Action to Execute:", Group: "Communication", Order: 0.1,
        Description: "Action to perform on the Data.\n" +
                     "Write_read: Write data and read answer. Answer is compared with expected read and result is published.\n" +
                     "Write_only: Write data and exit.\n" +
                     "Read_only: Read data and publish.\n" +
                     "Read_test: Read data and compare with expected data. Data is published")]


        public Action ISAct { get; set; }

        public enum Com
        {
            None,
            I2C,
            SPI,
        }

        [Display("Com Protocol:", Order: 0.4, Description: "Select the communication protocol used to read data from the device. The acquired data will be published.")]
        public Com SelectedCom { get; set; }



        private const string com = "Data Type";
        [Display("Number Write/Read:", Order: 0.2, Group: com, Description: "Define if Data will use number or string to Write/Read data")]
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

        [Display("Data String W/R:", Order: 2, Group: com, Description: "Define if Data is number or string to Write/Read data")]
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

        private const string GROUPS = "Number Write Data";
        [Display("Write Register Address:", Group: GROUPS, Order: 2.1, Collapsed: true, Description: "Which register will be used to write data.")]
        [EnabledIf("Enum", true, Flags = false)]

        public int WriteRegister { get; set; }

        [Display("Data Write (Numeric):", Group: GROUPS, Order: 2.2, Collapsed: true, Description: " Data to write on specified register")]
        [EnabledIf(nameof(ISAct), new object[] { Action.Write_read, Action.Write_only }, HideIfDisabled = false)]
        [EnabledIf("Enum", true, Flags = false)]
        public int WriteData { get; set; }

        private const string GROUPSB = "Number Read Data";
        [Display("Read Register Address:", Group: GROUPSB, Order: 2.3, Collapsed: true, Description: "Address of Register to read.")]
        [EnabledIf(nameof(ISAct), new object[] { Action.Write_read, Action.read_test, Action.read_only }, HideIfDisabled = false)]
        [EnabledIf("Enum", true, Flags = false)]
        public int ReadRegister { get; set; }

        [Display("Data Read Expected (Numeric):", Group: GROUPSB, Order: 2.4, Collapsed: true, Description: "Expected Data read from register. Data will be compared and published")]
        [EnabledIf(nameof(ISAct), new object[] { Action.Write_read, Action.read_test, Action.read_only }, HideIfDisabled = false)]
        [EnabledIf("Enum", true, Flags = false)]
        public double ExpectedRead { get; set; }


        private const string GROUPG = "String Write/Read Generic ";

        [Display("Data Write (String):", Group: GROUPG, Order: 3, Collapsed: true, Description: "Send data over the port using a literal string.")]
        [EnabledIf("Estr", true, Flags = false)]
        public string WriteDataStr { get; set; }

        [Display("Expected Read (String):", Group: GROUPG, Order: 3.1, Collapsed: true, Description: "Read Data on Port using specified data length.")]
        [EnabledIf("Estr", true, Flags = false)]
        public string ExpectedReadStr { get; set; }

        [Display("Data Read Length:", Group: GROUPG, Order: 3.2, Collapsed: true, Description: "Set number of Data to read on Port.")]
        [EnabledIf(nameof(ISAct), new object[] { Action.Write_read, Action.read_test, Action.read_only }, HideIfDisabled = false)]
        [EnabledIf("Estr", true, Flags = false)]
        public int ReadLength { get; set; } = 1;


        [Output]
        [Display("Measure:")]
        public string Measure { get; private set; }

        public I2cSpiCom()
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
            if (SelectedCom == Com.None)
            {
                Log.Error("No communication protocol selected.");
                UpgradeVerdict(Verdict.Error);
                return;
            }

            string command = "";
            string response = "";
            string test = "";

            // Build SCPI command based on protocol
            if (ISAct == Action.Write_only || ISAct == Action.Write_read)
            {
                if (!string.IsNullOrEmpty(WriteDataStr))
                {
                    command = $"COM:{SelectedCom}:WRITE {WriteRegister},{WriteDataStr}";
                }
                else
                {
                    command = $"COM:{SelectedCom}:WRITE {WriteRegister},{WriteData}";
                }

                Log.Info($"Write Command: {command}");
                IO_Instrument.ScpiCommand(command);

                if (ISAct == Action.Write_only)
                {
                    UpgradeVerdict(Verdict.Pass);
                    return;
                }
            }

            // Read command
            if (ISAct != Action.Write_only)
            {
                if (SelectedCom == Com.SPI)
                {
                    byte wreg = (byte)(ReadRegister | 0x80);
                    Log.Info($"SPI register: {ReadRegister} become: {wreg} with bit 7 added");
                    ReadRegister = wreg;
                }

                command = $"COM:{SelectedCom}:READ:LEN{ReadLength}? {ReadRegister}";
                Log.Info($"Read Command: {command}");

                try
                {
                    response = IO_Instrument.ScpiQuery<string>(command);
                }
                catch (TimeoutException ex)
                {
                    Log.Error($"Timeout: {ex.Message}");
                    UpgradeVerdict(Verdict.Error);
                    return;
                }

                Log.Info($"Response: {response}");

                if (!string.IsNullOrEmpty(ExpectedReadStr))
                {
                    // String comparison: check if expected string exists in the response
                    test = response.Contains(ExpectedReadStr) ? "PASS" : "FAIL";
                    Measure = response;
                }
                else
                {
                    // Numeric comparison
                    if (!double.TryParse(response.Replace("\"", "").Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double readValue))
                    {
                        Log.Error("Failed to parse response as numeric.");
                        UpgradeVerdict(Verdict.Error);
                        return;
                    }

                    const double tolerance = 1e-6; // very small tolerance for floating point comparison
                    bool ok = Math.Abs(readValue - ExpectedRead) < tolerance;
                    test = ok ? "PASS" : "FAIL";

                    // Limit output to 4 digits after decimal
                    Measure = readValue.ToString("F4", CultureInfo.InvariantCulture);
                }



                UpgradeVerdict(test == "PASS" ? Verdict.Pass : Verdict.Fail);

                // Publish result
                if (!string.IsNullOrEmpty(ExpectedReadStr))
                {
                    PublishResult(new TestResult<string>
                    {
                        ParamName = $"Reg 0x{ReadRegister:X2} Data (String)",
                        StepName = Name,
                        Value = response,
                        Verdict = test,
                        Units = "strcmp",
                        LowerLimit = ExpectedReadStr,
                        UpperLimit = ExpectedReadStr
                    });
                }
                else
                {
                    PublishResult(new TestResult<double>
                    {
                        ParamName = $"Reg 0x{ReadRegister:X2} Data (Numeric)",
                        StepName = Name,
                        Value = Math.Round(double.Parse(response, CultureInfo.InvariantCulture), 4),
                        Verdict = test,
                        Units = "read",
                        LowerLimit = ExpectedRead,
                        UpperLimit = ExpectedRead
                    });
                }
            }
        }
    
           

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}
