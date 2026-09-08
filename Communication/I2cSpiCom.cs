using InterconnectIOBox.Instruments;
using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;
using static InterconnectIOBox.GPIO.GpioIO;
using static InterconnectIOBox.GPIO.GpioPAD;
using static InterconnectIOBox.Digital.PortsIO;
using static System.Net.Mime.MediaTypeNames;
using InterconnectIOBox.Analysis;

namespace InterconnectIOBox.Communication
{


    [Display(Groups: new[] { "FTS_Interconnect", "Communication" }, Name: "I2C/SPI Data W/R", Description: "Write and/or read data on a register using the selected protocol. The communication protocol must be enabled for operation.")]

    public class I2cSpiCom : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion



        public InterconnectIO IO_Instrument { get; set; }

        // Renamed from "Action" to avoid ambiguity with the built-in System.Action delegate type.
        public enum IsAction
        {
            Write_read,
            Write_only,
            read_only,
            read_test
        }

        [Display("Action to Execute:", Group: "Communication", Order: 0.1,
        Description: "Action to perform on the data.\n" +
                     "Write_read: Write data and read the answer. The answer is compared with the expected read and the result is published.\n" +
                     "Write_only: Write data and exit.\n" +
                     "Read_only: Read data and publish, with no pass/fail comparison.\n" +
                     "Read_test: Read data and compare with expected data. Data is published.")]


        public IsAction ISAct { get; set; }

        public enum Com
        {
            None,
            I2C,
            SPI,
        }

        [Display("Com Protocol:", Order: 0.4, Description: "Select the communication protocol used to read data from the device. The acquired data will be published.")]
        public Com SelectedCom { get; set; }



        private const string GROUPC = "Data Type";
        [Display("Numeric Write/Read:", Order: 0.2, Group: GROUPC, Description: "Check if the data to write/read is numeric.")]
        public bool NumericMode
        {
            get => _NumericMode;
            set
            {
                _NumericMode = value;
                if (value) StringMode = false;
                OnPropertyChanged(nameof(NumericMode));
                OnPropertyChanged(nameof(StringMode));
            }
        }
        private bool _NumericMode;

        [Display("Data String W/R:", Order: 2, Group: GROUPC, Description: "Check if the data to write/read is a string.")]
        public bool StringMode
        {
            get => _StringMode;
            set
            {
                _StringMode = value;
                if (value) NumericMode = false;
                OnPropertyChanged(nameof(NumericMode));
                OnPropertyChanged(nameof(StringMode));
            }
        }
        private bool _StringMode;

        private const string GROUPS = "Number Write Data";
        [Display("Write Register Address:", Group: GROUPS, Order: 2.1, Collapsed: true, Description: "Register address to write data to.")]
        public int WriteRegister { get; set; }

        [Display("Data Write (Numeric):", Group: GROUPS, Order: 2.2, Collapsed: true, Description: "Data to write on the specified register.")]
        [EnabledIf(nameof(ISAct), new object[] { IsAction.Write_read, IsAction.Write_only }, HideIfDisabled = false)]
        [EnabledIf("NumericMode", true, Flags = false)]
        public int WriteData { get; set; }

        private const string GROUPSB = "Number Read Data";
        [Display("Read Register Address:", Group: GROUPSB, Order: 2.3, Collapsed: true, Description: "Register address to read from.")]
        [EnabledIf(nameof(ISAct), new object[] { IsAction.Write_read, IsAction.read_test, IsAction.read_only }, HideIfDisabled = false)]
        public int ReadRegister { get; set; }

        [Display("Data Read Expected (Numeric):", Group: GROUPSB, Order: 2.4, Collapsed: true, Description: "Expected data read from the register. Data will be compared and published.")]
        [EnabledIf(nameof(ISAct), new object[] { IsAction.Write_read, IsAction.read_test, IsAction.read_only }, HideIfDisabled = false)]
        [EnabledIf("NumericMode", true, Flags = false)]
        public double ExpectedRead { get; set; }


        private const string GROUPG = "String Write/Read Generic";

        [Display("Data Write (String):", Group: GROUPG, Order: 3, Collapsed: true, Description: "Data to write over the port as a literal string.")]
        [EnabledIf("StringMode", true, Flags = false)]
        public string WriteDataStr { get; set; }

        [Display("Expected Read (String):", Group: GROUPG, Order: 3.1, Collapsed: true, Description: "Expected string read from the port.")]
        [EnabledIf("StringMode", true, Flags = false)]
        public string ExpectedReadStr { get; set; }

        [Display("Data Read Length:", Group: GROUPG, Order: 3.2, Collapsed: true, Description: "Number of data bytes to read from the port.")]
        [EnabledIf(nameof(ISAct), new object[] { IsAction.Write_read, IsAction.read_test, IsAction.read_only }, HideIfDisabled = false)]
        [EnabledIf("StringMode", true, Flags = false)]
        public int ReadLength { get; set; } = 1;


        [Output]
        [Display("Measure:", Description: "The last value read (numeric or string), shown after the step has run.")]
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
            if (ISAct == IsAction.Write_only || ISAct == IsAction.Write_read)
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

                if (ISAct == IsAction.Write_only)
                {
                    UpgradeVerdict(Verdict.Pass);
                    return;
                }
            }

            // Read command
            if (ISAct != IsAction.Write_only)
            {
                // Use a local variable for the effective read register so that
                // the SPI bit-7 adjustment doesn't permanently mutate the
                // user-configured setting across repeated runs of this step.
                int effectiveReadRegister = ReadRegister;

                if (SelectedCom == Com.SPI)
                {
                    effectiveReadRegister = (byte)(ReadRegister | 0x80);
                    Log.Info($"SPI register: {ReadRegister} becomes: {effectiveReadRegister} with bit 7 added");
                }

                command = $"COM:{SelectedCom}:READ:LEN{ReadLength}? {effectiveReadRegister}";
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

                double readValue = 0;

                if (!string.IsNullOrEmpty(ExpectedReadStr))
                {
                    // String comparison: check if expected string exists in the response
                    test = response.Contains(ExpectedReadStr) ? "PASS" : "FAIL";
                    Measure = response;
                }
                else
                {
                    // Numeric comparison
                    if (!double.TryParse(response.Replace("\"", "").Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out readValue))
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

                // Read_only never affects pass/fail — it's a pure readback.
                if (ISAct == IsAction.read_only)
                {
                    test = "PASS";
                }

                UpgradeVerdict(test == "PASS" ? Verdict.Pass : Verdict.Fail);

                // Publish result
                if (!string.IsNullOrEmpty(ExpectedReadStr))
                {
                    PublishResult(new TestResult<string>
                    {
                        ParamName = $"Reg 0x{effectiveReadRegister:X2} Data (String)",
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
                        ParamName = $"Reg 0x{effectiveReadRegister:X2} Data (Numeric)",
                        StepName = Name,
                        Value = Math.Round(readValue, 4),
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