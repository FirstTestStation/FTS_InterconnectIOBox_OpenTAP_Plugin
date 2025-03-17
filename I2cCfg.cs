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
using static InterconnectIOBox.SerialCfg;

namespace InterconnectIOBox
{
    [Display(Groups: new[] { "InterconnectIO", "Communication", "I2C" }, Name: "I2C Config", Description: "Enable and configure I2C communication, including baudrate, Mode, ChipSelect and Databits." +
"I2C must be enabled for operation as a I2C port. When disabled, the I2C pins function as normal GPIOs.")]


    public class ItcCfg : ResultTestStep
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

        [Display("Action to Execute:", Group: "I2C Configuration", Order: 0.1, Description: "Action to perform on the Data, result will be published.")]
        public Action ItcAct { get; set; }


        [Display("Enable I2C Com:", Order: 1, Group: "I2C Port Enable/Disable", Description: "Check to enable or disable I2C communication. ")]

        public bool Enable
        {
            get => _Enable;
            set
            {
                _Enable = value;
                if (value) Disable = false; //
                OnPropertyChanged(nameof(Enable));
                OnPropertyChanged(nameof(Disable));
            }
        }
        private bool _Enable;

        [Display("Disable I2C Com:", Order: 1.1, Group: "I2C Port Enable/Disable", Description: "Disable I2C. The pins used for I2C communication become normal GPIOs set as inputs.")]
        public bool Disable
        {
            get => _Disable;
            set
            {
                _Disable = value;
                if (value) Enable = false; //
                OnPropertyChanged(nameof(Enable));
                OnPropertyChanged(nameof(Disable));
            }
        }
        private bool _Disable;

        private const string GROUPD = "I2C Communication Config";

        [Display("I2C Speed:", Group: GROUPD, Order: 2, Description: "Set Clock frequency to use on I2C communication")]
        [EnabledIf("Enable", true, Flags = false)]
        [Unit("Hz", UseEngineeringPrefix: true)]
        public double Baud { get; set; } = 100000;


        [Display("I2C Address:", Group: GROUPD, Order: 3.1, Description: "Set I2C Address of the device to Read/Write")]
        [EnabledIf("Enable", true, Flags = false)]
        public byte Address { get; set; }

        public enum Databit :byte
        {
            _8 = 8,
            _16 = 16,
        }

        [Display("I2C Databits:", Group: GROUPD, Order: 3.2, Description: "Set Size of data to read or write")]
        [EnabledIf("Enable", true, Flags = false)]
        public Databit SelectD { get; set; } = Databit._8;


        public ItcCfg()
        {
            // ToDo: Set default values for properties / settings.
            Address = OWire_Dut.I2CSelftestaddress;  // Get I2C address from DUT
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts

        }

        public override void Run()
        {
            string test = "";
            if (Enable)
            {
                Log.Info("I2C Communication Enabled");

                // Write command
                if ((ItcAct == Action.Write_read) || (ItcAct == Action.Write_only))
                {
                    string Command = $"COM:INITIALIZE:ENABLE I2C";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }
            }
            else if (Disable)
            {
                if ((ItcAct == Action.Write_read) || (ItcAct == Action.Write_only))
                {
                    string Command = $"COM:INITIALIZE:DISABLE I2C";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }
            }

            // Read Command
            if ((ItcAct == Action.read_only) || (ItcAct == Action.Write_read) || (ItcAct == Action.read_test))
            {
                string Command = $"COM:INITIALIZE:STATUS? I2C";
                string response = IO_Instrument.ScpiQuery<string>(Command);
                Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

                double expected = Enable ? 1 : 0;

                double.TryParse(response, out double value);
                test = (value == expected) ? "PASS" : "FAIL";

                if (ItcAct == Action.read_only)
                {
                    test = "PASS"; // No verdict on read only }
                }

                if (test == "PASS")
                {
                    UpgradeVerdict(Verdict.Pass);
                }
                else
                {
                    UpgradeVerdict(Verdict.Fail);
                    Log.Warning($"I2C Communication Status Error, expected: {expected}, Answer: {response}");
                }

                if (ItcAct != Action.read_only) // if publish required
                {
                    // Create test result object
                    TestResult<double> result = new TestResult<double>
                    {
                        ParamName = $"I2C Status",
                        StepName = Name,
                        Value = value,
                        Verdict = test,
                        Units = "read"
                    };

                    // Add limits for Write_Read function
                    if ((ItcAct == Action.Write_read) || (ItcAct == Action.read_test))
                    {
                        result.LowerLimit = expected;
                        result.UpperLimit = expected;
                        result.Units = "digcmp";
                    }

                    PublishResult(result);
                }

            }

            // Process Itc Parameters
            if(Enable)
            {
                Log.Info("I2C Communication Parameters");
                ConfigureItcParam("Baudrate", ItcAct);
                ConfigureItcParam("Databits", ItcAct);
                ConfigureItcParam("Address", ItcAct);
            }
        }


        // <summary>
        /// Write or Read Itc parameter
        ///  <param name="<"name of the parameter">
        /// <param Action=<"selectedAct">Selected function for action.</param>
        /// 
        public void ConfigureItcParam(string name,Action selectedAct)
        {
            string WriteCmd = "";
            string ReadCmd = "";
            string test = "";
            double LowerLimit = 0;
            double UpperLimit = 1;
            string SUnits = "";
            byte mode = 0;

            switch (name)
            {
                case "Baudrate":
                    WriteCmd = $"COM:I2C:BAUDRATE {Baud}";
                    LowerLimit = Baud;
                    UpperLimit = Baud;
                    ReadCmd = "COM:I2C:BAUDRATE?";
                    SUnits = "digcmp";
                    break;

                case "Databits":
                    mode = (byte)SelectD; // Extract the number from the enum
                    WriteCmd = $"COM:I2C:DATABITS {mode}";
                    LowerLimit = mode; // Expect to read the same value
                    UpperLimit = mode;
                    ReadCmd = "COM:I2C:DATABITS?";
                    SUnits = "digcmp";
                    break;

                case "Address":
                    WriteCmd = $"COM:I2C:ADDRESS {Address}";
                    LowerLimit = Address;
                    UpperLimit = Address;
                    ReadCmd = "COM:I2C:ADDRESS?";
                    SUnits = "digcmp";
                    break;
            }
                // Write Command
                if ((selectedAct == Action.Write_read) || (selectedAct == Action.Write_only))
                {
                    Log.Info($"Sending SCPI command: {WriteCmd}");
                    IO_Instrument.ScpiCommand(WriteCmd);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read Command
                if ((selectedAct == Action.read_only) || (selectedAct == Action.Write_read) || (selectedAct == Action.read_test))
                {
                    string response = IO_Instrument.ScpiQuery<string>(ReadCmd);
                    Log.Info($"Sending SCPI command: {ReadCmd}, Answer: {response}");


                    double.TryParse(response, out double value);

                    if (value >= LowerLimit && value <= UpperLimit)
                    {
                        test = "PASS";
                    }
                    else
                    {
                        Log.Warning($"Invalid SCPI response: {response}, expected between: {LowerLimit} and {UpperLimit}");
                        test = "FAIL";
                    }

                    if (selectedAct == Action.read_only)
                    {
                        test = "PASS"; // No verdict on read only
                    }

                    if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                    else UpgradeVerdict(Verdict.Fail);


                    if (selectedAct != Action.read_only) // if publish required
                    {
                        // Create test result object
                        TestResult<double> result = new TestResult<double>
                        {
                            ParamName = $"I2C {name}",
                            StepName = Name,
                            Value = value,
                            Verdict = test,
                            Units = "read"
                        };

                        // Add limits for Write_Read function
                        if (selectedAct == Action.Write_read || selectedAct == Action.read_test)
                        {
                            result.LowerLimit = LowerLimit;
                            result.UpperLimit = UpperLimit;
                            result.Units = SUnits;
                        }

                        PublishResult(result);
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
