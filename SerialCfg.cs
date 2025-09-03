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
using static InterconnectIOBox.SpiCfg;

namespace InterconnectIOBox
{
    [Display(Groups: new[] { "InterconnectIO", "Communication", "Serial" }, Name: "Serial Config", Description: "Enable and configure serial communication, including baud rate, protocol, handshake, and timeout." +
"Serial must be enabled for operation as a serial port. When disabled, the serial pins function as normal GPIOs.")]


    public class SerialCfg : ResultTestStep
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

        [Display("Action to Execute:", Group: "Serial Configuration", Order: 0.1, Description: "Action to perform on the Data, result will be published.")]
        public Action SerialAct { get; set; }


        [Display("Enable Serial Com:", Order: 1, Group: "Serial Port Enable/Disable", Description: "Check to enable or disable serial communication. ")]

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

        [Display("Disable Serial Com:", Order: 1.1, Group: "Serial Port Enable/Disable", Description: "Disable Serial. The pins used for serial communication become normal GPIOs set as inputs.")]
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



        private const string GROUPD = "Serial Communication Config";

        [Display("Baudrate:", Group: GROUPD, Order: 2, Description: "Set Baudrate value")]
        [EnabledIf("Enable", true, Flags = false)]
        public double Baud { get; set; } = 115200;

        [Display("Timeout:", Group: GROUPD, Order: 2, Description: "Set Timeout in mS")]
        [EnabledIf("Enable", true, Flags = false)]
        [Unit("mS", UseEngineeringPrefix: false)]
        public double Stimeout { get; set; } = 2000;

        public enum Parity : byte
        {
            None = (byte) 'N',
            Even = (byte) 'E',
            Odd = (byte) 'O',
        }

        [Display("Parity:", Group: GROUPD, Order: 3, Description: "Set Parity value")]
        [EnabledIf("Enable", true, Flags = false)]
        public Parity SelectP { get; set; } = Parity.None;

        public enum Databit : byte
        {
            _8 = 8,
            _7 = 7,
            _6 = 6,
            _5 = 5,
        }

        [Display("Databit:", Group: GROUPD, Order: 3.1, Description: "Set Number of Data Bits")]
        [EnabledIf("Enable", true, Flags = false)]
        public Databit SelectD { get; set; } = Databit._8;

        public enum Stopbit : byte
        {
            _1 = 1,
            _2 = 2
        }

        [Display("Stop Bits:", Group: GROUPD, Order: 3.2, Description: "Set Number of Stop Bits")]
        [EnabledIf("Enable", true, Flags = false)]
        public Stopbit SelectS { get; set; } = Stopbit._1;


        public enum Eol : byte
        {
            None = 0,
            LF=1,
            CR = 2,
            LFCR = 3,
            CRLF = 4
        }

        [Display("EOL Sequence:", Group: GROUPD, Order: 3.3, Description: "Define the EOL characters sequence to use for write and detect during read")]
        [EnabledIf("Enable", true, Flags = false)]
        public Eol SelectE{ get; set; } = Eol.LF;


        public enum HandShake : byte
        {
            OFF = 0,
            ON = 1
        }

        [Display("RTS/CTS HandShake:", Group: GROUPD, Order: 3.4, Description: "Set or Not the RTS/CTS Handshake")]
        [EnabledIf("Enable", true, Flags = false)]
        public HandShake SelectH { get; set; } = HandShake.OFF;


        



        public SerialCfg()
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
            string test;
            if (Enable)
            {
                Log.Info("Serial Communication Enabled");

                // Write command
                if ((SerialAct == Action.Write_read) || (SerialAct == Action.Write_only))
                {
                    string Command = $"COM:INITIALIZE:ENABLE SERIAL";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }
            }
            else if (Disable)
            {
                if ((SerialAct == Action.Write_read) || (SerialAct == Action.Write_only))
                {
                    string Command = $"COM:INITIALIZE:DISABLE SERIAL";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }
            }

            // Read Command
            if ((SerialAct == Action.read_only) || (SerialAct == Action.Write_read) || (SerialAct == Action.read_test))
            {
                string Command = $"COM:INITIALIZE:STATUS? SERIAL";
                string response = IO_Instrument.ScpiQuery<string>(Command);
                Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

                double expected = Enable ? 1 : 0;

                double.TryParse(response, out double value);
                test = (value == expected) ? "PASS" : "FAIL";

                if (SerialAct == Action.read_only)
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
                    Log.Warning($"Serial Communication Status Error, expected: {expected}, Answer: {response}");
                }

                if (SerialAct != Action.read_only) // if publish required
                {
                    // Create test result object
                    TestResult<double> result = new TestResult<double>
                    {
                        ParamName = $"Serial Status",
                        StepName = Name,
                        Value = value,
                        Verdict = test,
                        Units = "read"
                    };

                    // Add limits for Write_Read function
                    if ((SerialAct == Action.Write_read) || (SerialAct == Action.read_test))
                    {
                        result.LowerLimit = expected;
                        result.UpperLimit = expected;
                        result.Units = "digcmp";
                    }

                    PublishResult(result);
                }

            }

            // Process Serial Parameters
            if(Enable)
            {
                Log.Info("Serial Communication Parameters");
                ConfigureSerialParam("Baudrate", SerialAct);
                ConfigureSerialProtocol(SerialAct);
                ConfigureSerialParam("Eol", SerialAct);
                ConfigureSerialParam("HandShake", SerialAct);
                ConfigureSerialParam("Timeout", SerialAct);
            }
        }


        // <summary>
        /// Write or Read Serial parameter
        ///  <param name="<"name of the parameter">
        /// <param Action=<"selectedAct">Selected function for action.</param>
        /// 
        public void ConfigureSerialParam(string name,Action selectedAct)
        {
            string WriteCmd = "";
            string ReadCmd = "";
            string test;
            double LowerLimit = 0;
            double UpperLimit = 1;
            string SUnits = "";

            switch (name)
            {
                case "Baudrate":
                    WriteCmd = $"COM:SERIAL:BAUDRATE {Baud}";
                    LowerLimit = Baud * 0.98; // True Speed readback can be 2% lower
                    UpperLimit = Baud * 1.02; // True Speed readback can be 2% higher
                    ReadCmd = "COM:SERIAL:BAUDRATE?";
                    SUnits = "numeric";
                    break;

                case "Timeout":
                    WriteCmd = $"COM:SERIAL:TIMEOUT {Stimeout}";
                    LowerLimit = Stimeout; // Expect to read the sane value
                    UpperLimit = Stimeout; 
                    ReadCmd = "COM:SERIAL:TIMEOUT?";
                    SUnits = "numeric";
                    break;

                case "Eol":
                    byte eolValue = (byte)SelectE;
                    WriteCmd = $"COM:SERIAL:EOL {eolValue}";
                    LowerLimit = eolValue; // Expect to read the sane value
                    UpperLimit = eolValue;
                    ReadCmd = "COM:SERIAL:EOL?";
                    SUnits = "cmp";
                    break;


                case "HandShake":
                    byte Setval = (SelectH == HandShake.ON ? (byte)1 : (byte)0);
                    WriteCmd = $"COM:SERIAL:Handshake {Setval}";
                    LowerLimit = Setval; // Expect to read the same value
                    UpperLimit = Setval;
                    ReadCmd = "COM:SERIAL:Handshake?";
                    SUnits = "cmp";
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
                            ParamName = $"Serial {name}",
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

        // <summary>
        /// Write or Read Protocol parameter
        /// <param Action=<"selectedAct">Selected function for action.</param>
        /// 
        public void ConfigureSerialProtocol(Action selectedAct)
        {
            string Wp;
            string test;

            Wp = ((byte)SelectD).ToString(); // Extract the databit number from the enum
            Wp += (char)SelectP; // add parity
            Wp += ((byte)SelectS).ToString(); // Extract the stopbits number from the enum

            // Write Command
            if ((selectedAct == Action.Write_read) || (selectedAct == Action.Write_only))
            {
                string WriteCmd = $"COM:SERIAL:PROTOCOL {Wp}";
                Log.Info($"Sending SCPI command: {WriteCmd}");
                IO_Instrument.ScpiCommand(WriteCmd);
                UpgradeVerdict(Verdict.Pass);
            }

            // Read Command
            if ((selectedAct == Action.read_only) || (selectedAct == Action.Write_read) || (selectedAct == Action.read_test))
            {
                string ReadCmd = "COM:SERIAL:PROTOCOL?";
                string response = IO_Instrument.ScpiQuery<string>(ReadCmd);
                Log.Info($"Sending SCPI command: {ReadCmd}, Answer: {response}");

                string readP = response.Replace("\"", "").Trim();


                if (readP == Wp)
                {
                    test = "PASS";
                }
                else
                {
                    Log.Warning($"Invalid SCPI response: {readP}, expected: {Wp}");
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
                    TestResult<string> result = new TestResult<string>
                    {
                        ParamName = $"Serial Protocol:",
                        StepName = Name,
                        Value = readP,
                        Verdict = test,
                        Units = "read"
                    };

                    // Add limits for Write_Read function
                    if (selectedAct == Action.Write_read || selectedAct == Action.read_test)
                    {
                        result.LowerLimit = Wp;
                        result.UpperLimit = Wp;
                        result.Units = "strcmp";
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
