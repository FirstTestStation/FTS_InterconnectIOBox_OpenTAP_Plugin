using OpenTap;
using static InterconnectIOBox.GPIO.GpioIO;
using static InterconnectIOBox.Digital.PortsIO;
using System;
using System.Xml.Linq;
using static InterconnectIOBox.GPIO.GpioPAD;
using System.Collections.Generic;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using static InterconnectIOBox.Communication.SerialCfg;
using InterconnectIOBox.Instruments;
using InterconnectIOBox.Analysis;

namespace InterconnectIOBox.Communication
{
    [Display(Groups: new[] { "FTS_Interconnect", "Communication" }, Name: "I2C Config", Description: "Enable and configure I2C communication, including baud rate, address, and data bits." +
    " I2C must be enabled for operation as an I2C port. When disabled, the I2C pins function as normal GPIOs.")]


    public class ItcCfg : ResultTestStep
    {
        #region Settings

        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion


        public InterconnectIO IO_Instrument { get; set; }

        // Renamed from "Action" to avoid ambiguity with System.Action (the built-in delegate type).
        public enum ItcAction
        {
            Write_read,
            Write_only,
            read_only,
            read_test
        }

        [Display("Action to Execute:", Group: "I2C Configuration", Order: 0.1, Description: "Action to perform on the data; the result will be published.")]
        public ItcAction ItcAct { get; set; }

        [Display("Publish Results", Order: 0.2, Group: "Options", Description: "If checked, publish the read-back verification results. If unchecked, verification still runs and affects verdict, but no result is published.")]
        public bool PublishResults { get; set; } = false;


        [Display("Enable I2C Com:", Order: 1, Group: "I2C Port Enable/Disable", Description: "Check to enable I2C communication on this port.")]

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

        [Display("Disable I2C Com:", Order: 1.1, Group: "I2C Port Enable/Disable", Description: "Check to disable I2C. The pins used for I2C communication become normal GPIOs set as inputs.")]
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

        [Display("I2C Speed:", Group: GROUPD, Order: 2, Description: "Set the clock frequency to use for I2C communication.")]
        [EnabledIf("Enable", true, Flags = false)]
        [Unit("Hz", UseEngineeringPrefix: true)]
        public double Baud { get; set; } = 100000;


        [Display("I2C Address:", Group: GROUPD, Order: 3.1, Description: "Set the I2C address of the device to read/write.")]
        [EnabledIf("Enable", true, Flags = false)]
        public byte Address { get; set; }

        public enum Databit : byte
        {
            _8 = 8,
            _16 = 16,
        }

        [Display("I2C Databits:", Group: GROUPD, Order: 3.2, Description: "Set the size of the data to read or write.")]
        [EnabledIf("Enable", true, Flags = false)]
        public Databit SelectD { get; set; } = Databit._8;


        public ItcCfg()
        {
            // ToDo: Set default values for properties / settings.
            // NOTE: Dut is not assigned yet at construction time (it's set by
            // the test plan after this object is created), so it cannot be
            // read here. The default address is now set in PrePlanRun instead.
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();

            if (Address == 0 && Dut != null)
            {
                Address = Dut.I2CSelftestaddress; // Get default I2C address from DUT
            }
        }

        public override void Run()
        {
            string test = "";
            if (Enable)
            {
                Log.Info("I2C Communication Enabled");

                // Write command
                if (ItcAct == ItcAction.Write_read || ItcAct == ItcAction.Write_only)
                {
                    string Command = $"COM:INITIALIZE:ENABLE I2C";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }
            }
            else if (Disable)
            {
                if (ItcAct == ItcAction.Write_read || ItcAct == ItcAction.Write_only)
                {
                    string Command = $"COM:INITIALIZE:DISABLE I2C";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }
            }

            // Read Command
            if (ItcAct == ItcAction.read_only || ItcAct == ItcAction.Write_read || ItcAct == ItcAction.read_test)
            {
                string Command = $"COM:INITIALIZE:STATUS? I2C";
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
                    Log.Warning($"Could not parse I2C status response: '{response}'.");
                    test = "FAIL";
                }
                else
                {
                    test = value == expected ? "PASS" : "FAIL";
                }

                if (ItcAct == ItcAction.read_only)
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
                    Log.Warning($"I2C Communication Status Error, expected: {expected}, Answer: {response}");
                }

                if (ItcAct != ItcAction.read_only && PublishResults) // if publish required
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
                    if (ItcAct == ItcAction.Write_read || ItcAct == ItcAction.read_test)
                    {
                        result.LowerLimit = expected;
                        result.UpperLimit = expected;
                        result.Units = "digcmp";
                    }

                    PublishResult(result);
                }

            }

            // Process Itc Parameters
            if (Enable)
            {
                Log.Info("I2C Communication Parameters");
                ConfigureItcParam("Baudrate", ItcAct);
                ConfigureItcParam("Databits", ItcAct);
                ConfigureItcParam("Address", ItcAct);
            }
        }


        /// <summary>
        /// Writes and/or reads back an I2C configuration parameter.
        /// </summary>
        /// <param name="name">Name of the parameter to configure ("Baudrate", "Databits", or "Address").</param>
        /// <param name="selectedAct">Selected action (write, read, or both) to perform.</param>
        public void ConfigureItcParam(string name, ItcAction selectedAct)
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
            if (selectedAct == ItcAction.Write_read || selectedAct == ItcAction.Write_only)
            {
                Log.Info($"Sending SCPI command: {WriteCmd}");
                IO_Instrument.ScpiCommand(WriteCmd);
                UpgradeVerdict(Verdict.Pass);
            }

            // Read Command
            if (selectedAct == ItcAction.read_only || selectedAct == ItcAction.Write_read || selectedAct == ItcAction.read_test)
            {
                string response = IO_Instrument.ScpiQuery<string>(ReadCmd);
                Log.Info($"SCPI query: {ReadCmd}, Answer: {response}");

                // Sanitize before parsing — see note in Run(): an unparsed response must
                // not silently pass as a coincidental 0/false match.
                string cleaned = response?.Trim();
                bool parsed = double.TryParse(cleaned, out double value);

                if (!parsed)
                {
                    Log.Warning($"Could not parse I2C {name} response: '{response}'.");
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

                if (selectedAct == ItcAction.read_only)
                {
                    test = "PASS"; // No verdict on read only
                }

                if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                else UpgradeVerdict(Verdict.Fail);


                if (selectedAct != ItcAction.read_only && PublishResults) // if publish required
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
                    if (selectedAct == ItcAction.Write_read || selectedAct == ItcAction.read_test)
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
