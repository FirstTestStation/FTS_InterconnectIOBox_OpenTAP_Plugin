using OpenTap;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static InterconnectIOBox.Gpiocfg;
using static InterconnectIOBox.GpioIO;
using static InterconnectIOBox.RegCmd;
using static System.Net.Mime.MediaTypeNames;

namespace InterconnectIOBox
{

    [Display(Groups: new[] { "InterconnectIO", "ZModule", "Selftest DUT" }, Name: "Selftest COM command", Description: "Group of I2C command used to communicate with Selftest Board")]

    public class Selftest_com : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change

        public InterconnectIO IO_Instrument { get; set; }


        [Display("Publish Result:", Order: 0.6, Description: "When checked, the result of validation will be published")]
        public bool Validate { get; set; }


        private const string STG = "General Command";

        [Display("DUT Version:", Order: 1,Group:STG, Collapsed: true, Description: "Send I2C command to selftest board to read major and minor version")]
        public Enabled<double> Sversion { get; set; }

        [Display("DUT Status:", Order: 1, Group: STG, Collapsed: true, Description: "Send I2C command to selftest board to read status byte.")]
        public Enabled<byte> Status { get; set; }


        private const string STP = "PWM Configuration";

        [Display("PWM Enable:", Group: STP, Order: 1.2, Collapsed: true, Description: "Send I2C command to selftest board to enable the on board PWM")]

        public bool EnPwm
        {
            get => _EnPwm;
            set
            {
                _EnPwm = value;
                if (value) DiPwm = false; //
                OnPropertyChanged(nameof(EnPwm));
                OnPropertyChanged(nameof(DiPwm));
            }
        }
        private bool _EnPwm;


        [Display("PWM Disable:", Group: STP, Order: 1.3, Collapsed: true, Description: "Send I2C command to selftest board to disable the on board PWM")]
 

        public bool DiPwm
        {
            get => _DiPwm;
            set
            {
                _DiPwm = value;
                if (value) EnPwm = false; //
                OnPropertyChanged(nameof(EnPwm));
                OnPropertyChanged(nameof(DiPwm));
            }
        }
        private bool _DiPwm;


        [Display("PWM Frequency:", Group: STP, Order: 1.4, Collapsed: true, Description: "Send I2C command to selftest board to set the frequency of PWM (1-255)")]
        [Unit("KHz")]
        public Enabled<byte> frequency { get; set; }


        private const string STU = "UART Configuration";

        [Display("Enable Serial Com:", Order: 1, Group: STU, Description: "Check to enable or disable serial communication. ")]

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

        [Display("Disable Serial Com:", Order: 1.1, Group: STU, Description: "Disable Serial. The pins used for serial communication become normal GPIOs set as outputs.")]
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


        public enum Baud : byte
        {
            _19200 = 0,
            _38400 = 1,
            _57600 = 2,
            _115200 = 3,
        }

        [Display("Baudrate:", Group: STU, Collapsed: true, Order: 3, Description: "Set Baudrate speed")]
        [EnabledIf("Enable", true, Flags = false)]
        public Baud SelectB { get; set; } = Baud._38400;


        public enum Parity : byte
        {
            None = 0,
            Even = 1,
            Odd = 2,
        }

        [Display("Parity:", Group: STU, Collapsed: true, Order: 3, Description: "Set Parity value")]
        [EnabledIf("Enable", true, Flags = false)]
        public Parity SelectP { get; set; } = Parity.None;

        public enum Databit : byte
        {
            _8 = 3,
            _7 = 2,
            _6 = 1,
            _5 = 0,
        }

        [Display("Databit:", Group: STU, Collapsed: true, Order: 3.1, Description: "Set Number of Data Bits")]
        [EnabledIf("Enable", true, Flags = false)]
        public Databit SelectD { get; set; } = Databit._8;

        public enum Stopbit : byte
        {
            _1 = 0,
            _2 = 1
        }

        [Display("Stop Bits:", Group: STU, Collapsed: true, Order: 3.2, Description: "Set Number of Stop Bits")]
        [EnabledIf("Enable", true, Flags = false)]
        public Stopbit SelectS { get; set; } = Stopbit._1;

        public enum HandShake : byte
        {
            OFF = 0,
            ON = 1
        }

        [Display("RTS/CTS HandShake:", Group: STU, Collapsed: true, Order: 3.4, Description: "Set or Not the RTS/CTS Handshake")]
        [EnabledIf("Enable", true, Flags = false)]
        public HandShake SelectH { get; set; } = HandShake.OFF;

        [Display("Get Serial Config:", Group: STU, Order: 3.5, Collapsed: true, Description: "Send I2C command to selftest board to read the actual Serial config")]
        public Enabled<byte> GetSer { get; set; }


        private const string SPI = "SPI Configuration";

        [Display("Enable SPI Com:", Order: 4, Group: SPI, Description: "Check to enable or disable SPI communication. ")]

        public bool EnSPI
        {
            get => _EnSPI;
            set
            {
                _EnSPI = value;
                if (value) DiSPI = false; //
                OnPropertyChanged(nameof(EnSPI));
                OnPropertyChanged(nameof(DiSPI));
            }
        }
        private bool _EnSPI;

        [Display("Disable SPI Com:", Order: 4.1, Group: SPI, Description: "DiSPI SPI. The pins used for SPI communication become normal GPIOs set as inputs.")]
        public bool DiSPI
        {
            get => _DiSPI;
            set
            {
                _DiSPI = value;
                if (value) EnSPI = false; //
                OnPropertyChanged(nameof(EnSPI));
                OnPropertyChanged(nameof(DiSPI));
            }
        }
        private bool _DiSPI;

         // Not valid for a slave SPI.  Slave do not control the clock
        public enum Speed : byte
        {
            _100KHz = 1,
            _200KHz = 2,
            _300KHz = 3,
            _400KHz = 4,
            _500KHz = 5,
            _600KHz = 6,
            _700KHz = 7,
        }


        [Display("SPI Speed:", Group: SPI, Order: 4.1, Description: "Set Clock frequency to use on SPI communication")]
        [EnabledIf("EnSPI", true, Flags = false)]
        public Speed SelectSpeed { get; set; } = Speed._100KHz;
   



        public enum Mode : byte
        {
            Mode0_CPOL0_CPHA0 = 0,
            Mode1_CPOL0_CPHA1 = 1,
            Mode2_CPOL1_CPHA0 = 2,
            Mode3_CPOL1_CPHA1 = 3,
        }


        [Display("Set SPI Mode:", Group: SPI, Order: 4.2, Description: "Select the SPI protocol")]
        [EnabledIf("EnSPI", true, Flags = false)]
        public Mode SelectM { get; set; } = Mode.Mode0_CPOL0_CPHA0;

        public enum SpiData : byte
        {
            _8 = 0,
            _16 = 1
        }

        [Display("SPI Databits:", Group: SPI, Collapsed: true, Order: 4.3, Description: "Set the databits number for SPI communication")]
        [EnabledIf("EnSPI", true, Flags = false)]
        public SpiData SelectSD { get; set; } = SpiData._8;


        [Display("Get SPI Config:", Group: SPI, Order: 4.4, Collapsed: true, Description: "Send I2C command to selftest board to read the actual SPI config")]
        public Enabled<byte> GetSpi { get; set; }


        #endregion
        public Selftest_com()
        {
            Validate = false;

            Sversion = new Enabled<double>() { IsEnabled = false, Value = 1.1 };
            Status = new Enabled<byte>() { IsEnabled = false, Value = 0 };
            frequency = new Enabled<byte>() { IsEnabled = false, Value = 10 };
            GetSpi = new Enabled<byte>() { IsEnabled = false, Value = 0 };
            GetSer = new Enabled<byte>() { IsEnabled = false, Value = 0 };
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        public override void Run()
        {
            // public void CommandExecute(string name, byte value, bool check, bool publish)
            if (Sversion.IsEnabled == true)
            {
               CommandExecute("Version", Sversion.Value, true,Validate);
            }

            if (Status.IsEnabled == true)
            {
                CommandExecute("Status", Status.Value, true, Validate);
            }

            if (frequency.IsEnabled == true)
            {
                CommandExecute("Frequency", frequency.Value, false, false);
            }

            if (EnPwm == true)
            {
                CommandExecute("EnPwm",1, true, false);
            }
            if (DiPwm == true)
            {
               CommandExecute("DiPwm", 0, true, false);
            }


            if (Enable == true)  // Set protocol
            {
                byte value = (byte)((byte)SelectB << 6 | (byte)SelectP << 4 | (byte)SelectD << 2 | (byte)SelectS << 1 | (byte)SelectH); // Build protocol value
                CommandExecute("Sercfg", value, true, Validate);
               //GetSer.Value = value;
            }

            if (GetSer.IsEnabled == true)
            {
                CommandExecute("GetSercfg", GetSer.Value, true, Validate);
            }

            // Serial Communication
            if (Enable == true)
            {
                CommandExecute("EnSerial", (byte)SelectH, false, false); // Enable and Set Handshake
            }

            if (Disable == true)
            {
                CommandExecute("DisSerial", 0, false, false);
            }

           

            // SPI Communication, write protocol before enable
            if (EnSPI == true)  // Set SPI protocol
            {
                byte vespi = (EnSPI == true) ? (byte)1 : (byte)0;


                byte value = (byte)((byte)SelectSpeed << 4 | (byte)SelectSD << 3 | (byte)SelectM << 1 | (byte)vespi << 0); // Build protocol value

                CommandExecute("Spicfg", value, true, Validate);
                //GetSpi.Value = value;
            }

            if (EnSPI == true)
            {
                CommandExecute("EnSPI", 1, true, false);
            }
            if (DiSPI == true)
            {
                CommandExecute("DisSPI", 1, true, false);
            }

            if (GetSpi.IsEnabled == true)
            {
                CommandExecute("GetSpicfg", GetSpi.Value, true, Validate);
            }

        }

        /// <summary>
        /// Command execute for configures the selftest board. This method handles different configuration parameters,
        /// </summary>
        /// <param name="name">The name of the configuration parameter to modify, such as "Status" or "Version".</param>
        /// <param name="value">The value to write on gpio number.</param>
        /// <param name="check">A flag indicating whether the validation test should be executed.</param>
        /// <param name="publish">A flag indicating whether the result should be published.</param>
        public void CommandExecute(string name,double value, bool check, bool publish)
        {
            byte   Writereg = 0;
            string WriteCmd = "";
            byte   Readreg = 0;
            string test = "";
            string pname = "";
            double mvalue = 0;
            string unit = "read";

            bool read = false;



            switch (name)
            {
                case "Version":
                    string vrs = Selfreadcmd(01, 0);
                    vrs += "." + Selfreadcmd(02, 0);
                    mvalue = double.Parse(vrs);
                    Log.Info($"Selftest Version: {mvalue}");
                    pname = "Selftest firm version";
                    Readreg = 0;
                    Writereg = 0;
                    read = true;
                    break;

                case "Status":
                    pname = "Error Status";
                    Readreg = 100;
                    Writereg = 0;
                    read = true;
                    break;

                case "EnPwm":
                    pname = "Enable PWM";
                    Readreg = 0;
                    Writereg = 80;
                    mvalue = 1;  //enable
                    read = false;
                    break;

                case "DisPwm":
                    pname = "Disable PWM";
                    Readreg = 0;
                    Writereg = 80;
                    mvalue = 0;  //Disable
                    read = false;
                    break;

                case "Frequency":
                    pname = "PWM frequency";
                    Readreg = 0;
                    Writereg = 81;
                    mvalue = 0;  //Disable
                    read = false;
                    break;

                case "EnSerial":
                    pname = "Enable Serial";
                    Readreg = 0;
                    Writereg = 101;
                    read = false;
                    break;

                case "DisSerial":
                    pname = "Disable Serial";
                    Readreg = 0;
                    Writereg = 102;
                    mvalue = 1;  //Set GPIO as output
                    read = false;
                    break;

                case "Sercfg":
                    pname = "Set Serial Config";
                    Readreg = 0;
                    Writereg = 103;
                    read = false;
                    break;

                case "GetSercfg":
                    pname = "Get Serial Config";
                    Readreg = 105;
                    Writereg = 0;
                    read = true;
                    break; ;

                case "EnSPI":
                    pname = "Enable SPI";
                    Readreg = 0;
                    Writereg = 111;
                    mvalue = 1;  //enable
                    read = false;
                    break;

                case "DisSPI":
                    pname = "Disable SPI";
                    Readreg = 0;
                    Writereg = 112;
                    mvalue = 0;  //Disable
                    read = false;
                    break;

                case "Spicfg":
                    pname = "Set SPI Config";
                    Readreg = 0;
                    Writereg = 113;
                    read = false;
                    break;

                case "GetSpicfg":
                    pname = "Get SPI Config";
                    Readreg = 115;
                    Writereg = 0;
                    read = true;
                    break; ;
            }

            if (Writereg != 0)
            {
                WriteCmd = $"COM:I2C:WRITE {Writereg}, {value}";
                Log.Info($"Sending Write SCPI command: {WriteCmd}");
                IO_Instrument.ScpiCommand(WriteCmd);
                UpgradeVerdict(Verdict.Pass);
            }

            if (Readreg != 0)
            {
               string rsp = Selfreadcmd(Readreg, (byte) value);
               mvalue = byte.Parse(rsp);

            }

            if (check && read == true)
            {
                unit = "digcmp";
                if (mvalue == value) 
                {
                    test = "PASS";
                }
                else
                {
                    Log.Warning($"Invalid SCPI value: {mvalue}, expected:  {value}");
                    test = "FAIL";
                }
                if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                else UpgradeVerdict(Verdict.Fail);


                if (publish) // if publish required
                {
                    // Create test result object
                    TestResult<double> result = new TestResult<double>
                    {
                        ParamName = pname,
                        StepName = Name,
                        Value = mvalue,
                        Verdict = test,
                        LowerLimit = value,
                        UpperLimit = value,
                        Units = unit
                    };

                    PublishResult(result);
                }
            }
        }


        /// <summary>
        /// SCPI query command,
        /// </summary>
        /// <param name="reg">The name of the register to read value.</param>
        /// <param name="value">The value to write on register.</param>
        private string Selfreadcmd(byte reg, byte value)
        {
            string Rd = $"COM:I2C:READ:LEN1? {reg},{value}";
            string response = IO_Instrument.ScpiQuery<string>(Rd);
            Log.Info($"Sending SCPI command: {Rd}, Answer: {response}");
            return response;
        }


        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
 


}
