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
    [Display(Groups: new[] { "InterconnectIO", "Communication"}, Name: "SPI Config", Description: "Enable and configure SPI communication, including baudrate, Mode, ChipSelect and Databits." +
"SPI must be enabled for operation as a SPI port. When disabled, the SPI pins function as normal GPIOs.")]


    public class SpiCfg : ResultTestStep
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

        [Display("Action to Execute:", Group: "SPI Configuration", Order: 0.1, Description: "Action to perform on the Data, result will be published.")]
        public Action SpiAct { get; set; }


        [Display("Enable SPI Com:", Order: 1, Group: "SPI Port Enable/Disable", Description: "Check to enable or disable SPI communication. ")]

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

        [Display("Disable SPI Com:", Order: 1.1, Group: "SPI Port Enable/Disable", Description: "Disable SPI. The pins used for SPI communication become normal GPIOs set as inputs.")]
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



        private const string GROUPD = "SPI Communication Config";

        [Display("SPI Speed:", Group: GROUPD, Order: 2, Description: "Set Clock frequency to use on SPI communication")]
        [EnabledIf("Enable", true, Flags = false)]
        [Unit("Hz", UseEngineeringPrefix: true)]
        public double Baud { get; set; } = 100000;


        public enum Mode : byte
        {
            Mode0_CS0_CPOL0_CPHA0 =0,
            Mode1_CS0_CPOL0_CPHA1 =1,
            Mode2_CS0_CPOL1_CPHA0 =2,
            Mode3_CS0_CPOL1_CPHA1 =3,
            Mode4_CS1_CPOL0_CPHA0 =4,
            Mode5_CS1_CPOL0_CPHA1 =5,
            Mode6_CS1_CPOL1_CPHA0 =6,
            Mode7_CS1_CPOL1_CPHA1 =7
        }


        [Display("SPI Mode:", Group: GROUPD, Order: 3, Description: "Select the SPI mode. Modes 0-3: Chip Select (CS) remains low during the entire transfer. Modes 4-7: Chip Select (CS) returns high between each byte.")]
        [EnabledIf("Enable", true, Flags = false)]
        public Mode SelectM { get; set; } = Mode.Mode0_CS0_CPOL0_CPHA0;

        public enum Cs : byte
        {
            _0 =0,
            _1 = 1,
            _5 = 5,
            _6 = 6,
            _7 = 7,
            _12 = 12,
            _13 = 13,
            _14 = 14,
            _15 = 15,
            _16 = 16,
            _17 = 17

        }

        [Display("SPI ChipSelect:", Group: GROUPD, Order: 3.1, Description: "Set ChipSelect pin, SPIO_CS (Gpio5) is CS default")]
        [EnabledIf("Enable", true, Flags = false)]
        public Cs SelectCs { get; set; } = Cs._5;

        public enum Databit :byte
        {
            _8 = 8,
            _16 = 16,
        }

        [Display("SPI Databits:", Group: GROUPD, Order: 3.2, Description: "Set Size of data to read or write")]
        [EnabledIf("Enable", true, Flags = false)]
        public Databit SelectD { get; set; } = Databit._8;

        public SpiCfg()
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
            // Process Spi Parameters before enabling 
            if (Enable)
            {
                Log.Info("SPI Communication Parameters");
                ConfigureSpiParam("Baudrate", SpiAct);
                ConfigureSpiParam("Mode", SpiAct);
                ConfigureSpiParam("Databits", SpiAct);
                ConfigureSpiParam("ChipSelect", SpiAct);

            }

            string test = "";
            if (Enable)
            {
                Log.Info("SPI Communication Enabled");

                // Write command
                if ((SpiAct == Action.Write_read) || (SpiAct == Action.Write_only))
                {
                    string Command = $"COM:INITIALIZE:ENABLE SPI";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }
            }
            else if (Disable)
            {
                if ((SpiAct == Action.Write_read) || (SpiAct == Action.Write_only))
                {
                    string Command = $"COM:INITIALIZE:DISABLE SPI";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }
            }

            // Read Command
            if ((SpiAct == Action.read_only) || (SpiAct == Action.Write_read) || (SpiAct == Action.read_test))
            {
                string Command = $"COM:INITIALIZE:STATUS? SPI";
                string response = IO_Instrument.ScpiQuery<string>(Command);
                Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

                double expected = Enable ? 1 : 0;

                double.TryParse(response, out double value);
                test = (value == expected) ? "PASS" : "FAIL";

                if (SpiAct == Action.read_only)
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
                    Log.Warning($"SPI Communication Status Error, expected: {expected}, Answer: {response}");
                }

                if (SpiAct != Action.read_only) // if publish required
                {
                    // Create test result object
                    TestResult<double> result = new TestResult<double>
                    {
                        ParamName = $"SPI Status",
                        StepName = Name,
                        Value = value,
                        Verdict = test,
                        Units = "read"
                    };

                    // Add limits for Write_Read function
                    if ((SpiAct == Action.Write_read) || (SpiAct == Action.read_test))
                    {
                        result.LowerLimit = expected;
                        result.UpperLimit = expected;
                        result.Units = "digcmp";
                    }

                    PublishResult(result);
                }

            }

        }


        // <summary>
        /// Write or Read Spi parameter
        ///  <param name="<"name of the parameter">
        /// <param Action=<"selectedAct">Selected function for action.</param>
        /// 
        public void ConfigureSpiParam(string name,Action selectedAct)
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
                    WriteCmd = $"COM:SPI:BAUDRATE {Baud}";
                    LowerLimit = Baud;
                    UpperLimit = Baud;
                    ReadCmd = "COM:SPI:BAUDRATE?";
                    SUnits = "digcmp";
                    break;

                case "Mode":
                    mode = (byte)SelectM; // Extract the number from the enum
                    WriteCmd = $"COM:SPI:MODE {mode}";
                    LowerLimit = mode; // Expect to read the sane value
                    UpperLimit = mode; 
                    ReadCmd = "COM:SPI:MODE?";
                    SUnits = "digcmp";
                    break;

                case "Databits":
                    mode = (byte)SelectD; // Extract the number from the enum
                    WriteCmd = $"COM:SPI:DATABITS {mode}";
                    LowerLimit = mode; // Expect to read the same value
                    UpperLimit = mode;
                    ReadCmd = "COM:SPI:DATABITS?";
                    SUnits = "digcmp";
                    break;

                case "ChipSelect":
                    mode = (byte)SelectCs; // Extract the number from the enum
                    WriteCmd = $"COM:SPI:CS {mode}";
                    LowerLimit = mode;
                    UpperLimit = mode;
                    ReadCmd = "COM:SPI:CS?";
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
                            ParamName = $"SPI {name}",
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
