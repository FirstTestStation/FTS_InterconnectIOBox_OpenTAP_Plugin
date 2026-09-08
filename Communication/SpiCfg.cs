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
    [Display(Groups: new[] { "FTS_Interconnect", "Communication" }, Name: "SPI Config", Description: "Enable and configure SPI communication, including baud rate, mode, chip select, and data bits." +
    " SPI must be enabled for operation as an SPI port. When disabled, the SPI pins function as normal GPIOs.")]


    public class SpiCfg : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        public InterconnectIO IO_Instrument { get; set; }

        // Renamed from "Action" to avoid ambiguity with the built-in System.Action delegate type.
        public enum SpiAction
        {
            Write_read,
            Write_only,
            read_only,
            read_test
        }

        [Display("Action to Execute:", Group: "SPI Configuration", Order: 0.1, Description: "Action to perform on the data; the result will be published.")]
        public SpiAction SpiAct { get; set; }

        [Display("Publish Results", Order: 0.2, Group: "Options", Description: "If checked, publish the read-back verification results. If unchecked, verification still runs and affects verdict, but no result is published.")]
        public bool PublishResults { get; set; } = false;


        [Display("Enable SPI Com:", Order: 1, Group: "SPI Port Enable/Disable", Description: "Check to enable SPI communication on this port.")]

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

        [Display("Disable SPI Com:", Order: 1.1, Group: "SPI Port Enable/Disable", Description: "Check to disable SPI. The pins used for SPI communication become normal GPIOs set as inputs.")]
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

        [Display("SPI Speed:", Group: GROUPD, Order: 2, Description: "Set the clock frequency to use for SPI communication.")]
        [EnabledIf("Enable", true, Flags = false)]
        [Unit("Hz", UseEngineeringPrefix: true)]
        public double Baud { get; set; } = 100000;


        public enum Mode : byte
        {
            Mode0_CS0_CPOL0_CPHA0 = 0,
            Mode1_CS0_CPOL0_CPHA1 = 1,
            Mode2_CS0_CPOL1_CPHA0 = 2,
            Mode3_CS0_CPOL1_CPHA1 = 3,
            Mode4_CS1_CPOL0_CPHA0 = 4,
            Mode5_CS1_CPOL0_CPHA1 = 5,
            Mode6_CS1_CPOL1_CPHA0 = 6,
            Mode7_CS1_CPOL1_CPHA1 = 7
        }


        [Display("SPI Mode:", Group: GROUPD, Order: 3, Description: "Select the SPI mode. Modes 0-3: Chip Select (CS) remains low during the entire transfer. Modes 4-7: Chip Select (CS) returns high between each byte.")]
        [EnabledIf("Enable", true, Flags = false)]
        public Mode SelectM { get; set; } = Mode.Mode0_CS0_CPOL0_CPHA0;

        public enum Cs : byte
        {
            _0 = 0,
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

        [Display("SPI Chip Select:", Group: GROUPD, Order: 3.1, Description: "Set the Chip Select pin. SPI0_CS (GPIO5) is the default.")]
        [EnabledIf("Enable", true, Flags = false)]
        public Cs SelectCs { get; set; } = Cs._5;

        public enum Databit : byte
        {
            _8 = 8,
            _16 = 16,
        }

        [Display("SPI Databits:", Group: GROUPD, Order: 3.2, Description: "Set the size of the data to read or write.")]
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
            string test = "";
            if (Enable)
            {
                Log.Info("SPI Communication Enabled");

                // Write command
                if (SpiAct == SpiAction.Write_read || SpiAct == SpiAction.Write_only)
                {
                    string Command = $"COM:INITIALIZE:ENABLE SPI";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }
            }
            else if (Disable)
            {
                if (SpiAct == SpiAction.Write_read || SpiAct == SpiAction.Write_only)
                {
                    string Command = $"COM:INITIALIZE:DISABLE SPI";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }
            }

            // Read Command
            if (SpiAct == SpiAction.read_only || SpiAct == SpiAction.Write_read || SpiAct == SpiAction.read_test)
            {
                string Command = $"COM:INITIALIZE:STATUS? SPI";
                string response = IO_Instrument.ScpiQuery<string>(Command);
                Log.Info($"SCPI query: {Command}, Answer: {response}");

                double expected = Enable ? 1 : 0;

                // Sanitize before parsing: without Trim(), a trailing newline/whitespace or
                // stray quotes from the instrument can make TryParse silently fail, leaving
                // value at its default 0 — which would coincidentally match expected==0
                // (Disable case) and report a false PASS even though the real status differs.
                string cleaned = response?.Trim();
                bool parsed = double.TryParse(cleaned, out double value);

                if (!parsed)
                {
                    Log.Warning($"Could not parse SPI status response: '{response}'.");
                    test = "FAIL";
                }
                else
                {
                    test = value == expected ? "PASS" : "FAIL";
                }

                if (SpiAct == SpiAction.read_only)
                {
                    test = "PASS"; // No verdict on read only
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

                if (SpiAct != SpiAction.read_only && PublishResults) // if publish required
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
                    if (SpiAct == SpiAction.Write_read || SpiAct == SpiAction.read_test)
                    {
                        result.LowerLimit = expected;
                        result.UpperLimit = expected;
                        result.Units = "digcmp";
                    }

                    PublishResult(result);
                }

            }

            // Process SPI Parameters
            if (Enable)
            {
                Log.Info("SPI Communication Parameters");
                ConfigureSpiParam("Baudrate", SpiAct);
                ConfigureSpiParam("Mode", SpiAct);
                ConfigureSpiParam("Databits", SpiAct);
                ConfigureSpiParam("ChipSelect", SpiAct);
            }

        }


        /// <summary>
        /// Writes and/or reads back an SPI configuration parameter.
        /// </summary>
        /// <param name="name">Name of the parameter to configure ("Baudrate", "Mode", "Databits", or "ChipSelect").</param>
        /// <param name="selectedAct">Selected action (write, read, or both) to perform.</param>
        public void ConfigureSpiParam(string name, SpiAction selectedAct)
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
                    LowerLimit = mode; // Expect to read the same value
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
            if (selectedAct == SpiAction.Write_read || selectedAct == SpiAction.Write_only)
            {
                Log.Info($"Sending SCPI command: {WriteCmd}");
                IO_Instrument.ScpiCommand(WriteCmd);
                UpgradeVerdict(Verdict.Pass);
            }

            // Read Command
            if (selectedAct == SpiAction.read_only || selectedAct == SpiAction.Write_read || selectedAct == SpiAction.read_test)
            {
                string response = IO_Instrument.ScpiQuery<string>(ReadCmd);
                Log.Info($"SCPI query: {ReadCmd}, Answer: {response}");

                // Sanitize before parsing — see note in Run(): an unparsed response must
                // not silently pass as a coincidental in-range match.
                string cleaned = response?.Trim();
                bool parsed = double.TryParse(cleaned, out double value);

                if (!parsed)
                {
                    Log.Warning($"Could not parse SPI {name} response: '{response}'.");
                    test = "FAIL";
                }
                else if (value >= LowerLimit && value <= UpperLimit)
                {
                    test = "PASS";
                }
                else
                {
                    Log.Warning($"Invalid SCPI response: {response}, expected between: {LowerLimit} and {UpperLimit}");
                    test = "FAIL";
                }

                if (selectedAct == SpiAction.read_only)
                {
                    test = "PASS"; // No verdict on read only
                }

                if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                else UpgradeVerdict(Verdict.Fail);


                if (selectedAct != SpiAction.read_only && PublishResults) // if publish required
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
                    if (selectedAct == SpiAction.Write_read || selectedAct == SpiAction.read_test)
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
