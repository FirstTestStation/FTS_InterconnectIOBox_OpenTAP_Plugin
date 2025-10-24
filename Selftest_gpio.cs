using OpenTap;
using System;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using static InterconnectIOBox.Gpiocfg;
using static InterconnectIOBox.GpioIO;

namespace InterconnectIOBox
{

    [Display(Groups: new[] { "InterconnectIO", "ZModule", "Selftest DUT" }, Name: "Selftest GPIO command", Description: "Group of I2C command used to communicate with Selftest Board")]

    public class Selftest_gpio : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change

        public InterconnectIO IO_Instrument { get; set; }

      


        [Display("I2C Address:", Order: 0.5, Description: "I2C address to use to communicate with selftest board. When Checked, the new I2C Address will be used for the TestStep duration.")]
        public Enabled<byte> I2Caddress { get; set; }

        [Display("Publish Result:", Order: 0.6, Description: "When checked, the result of validation will be published")]
        public bool Validate { get; set; }

        public enum GpioPins
        {
            None = -1,
            GP0 = 0,
            GP1 = 1,
            GP2 = 2,
            GP3 = 3,
            GP4 = 4,
            GP5 = 5,
            GP6 = 6,
            GP7 = 7,
            GP8 = 8,
            GP9 = 9,
            GP10 = 10,
            GP11 = 11,
            GP12 = 12,
            GP13 = 13,
            GP14 = 14,
            GP15 = 15,
            GP16 = 16,
            GP17 = 17,
            GP18 = 18,
            GP19 = 19,
            GP20 = 20,
            GP21 = 21,
            GP22 = 22,
            GP23 = 23,
            GP24 = 24,
            GP25 = 25,
            GP26 = 26,
            GP27 = 27,
            GP28 = 28
        }


        [Display("GPIO Number:", Order: 1, Collapsed: true, Description: "Send I2C command to selftest board to configure the selected GPIO")]
        public GpioPins  Selectgp { get; set; } = GpioPins.None;


        private const string GPCFG = "Selftest GPIO Basic Configuration";

        public enum GpioDir
        {
            Input =0,
            Output =1
        }

        [Display("GPIO Direction:", Group: GPCFG, Order: 1.2, Collapsed: true, Description: "Send I2C command to selftest board to configure direction of GPIO")]
        public Enabled<GpioDir> SelectDir { get; set; }

        [Display("Validate GPIO Direction:", Group: GPCFG, Order: 1.3, Collapsed: true, Description: "Send I2C command to selftest board to read the direction of the GPIO")]
        public bool EnableDir { get; set; }

        public enum GpioStr
        {
            mA_2 = 0,
            mA_4 = 1,
            mA_8 = 2,
            mA_12 = 3
        }

        [Display("GPIO Output Strength:", Group: GPCFG, Order: 1.4, Collapsed: true, Description: "Send I2C command to selftest board to configure strength of the GPIO output")]
        public Enabled<GpioStr> SelectStr { get; set; }

        [Display("Validate GPIO Strength:", Group: GPCFG, Order: 1.5, Collapsed: true, Description: "Send I2C command to selftest board to read the strength of the GPIO output")]
        public bool EnableStr { get; set; }

        public enum Gpiopd
        {
            none = 0,
            Pullup = 1,
            PullDown = 2,
        }

        [Display("GPIO Pull Direction:", Group: GPCFG, Order: 1.6, Collapsed: true, Description: "Send I2C command to selftest board to configure pull direction of the GPIO")]
        public Enabled<Gpiopd> Selectpull { get; set; }

        [Display("Validate GPIO Pull:", Group: GPCFG, Order: 1.7, Collapsed: true, Description: "Send I2C command to selftest board to read the pull direction of the GPIO")]
        public bool Enablepull { get; set; }

        public enum Setgpio
        {
            Low = 0,
            High = 1
        }

        [Display("GPIO Output Value:", Group: GPCFG, Order: 1.8, Collapsed: true, Description: "Send I2C command to selftest board to set the GPIO output")]
        public Enabled<Setgpio> Setgpout { get; set; }


        [Display("Validate GPIO Value:", Group: GPCFG, Order: 1.9, Collapsed: true, Description: "Send I2C command to selftest board to read the GPIO value")]
        public Enabled<Setgpio> Getgpin { get; set; }


        private const string GPDCFG = "Selftest GPIO Direct Configuration";



        [Display("GPIO Set Pads State:", Group: GPDCFG, Order: 2.1, Collapsed: true, Description: "Send I2C command to selftest board to Set the Pads State of selected GPIO")]
        public Enabled<byte> SetPads { get; set; }

        [Display("GPIO Get Pads State:", Group: GPDCFG, Order: 2.2, Collapsed: true, Description: "Send I2C command to selftest board to Get the Pads State of selected GPIO")]
        public Enabled<byte> GetPads { get; set; }

        [Display("GPIO Get Pads function:", Group: "Selftest GPIO Function", Order: 3.0, Collapsed: true, Description: "Send I2C command to selftest board to Get the function of selected GPIO:" +
            "0:XIP, 1:SPI, 2:UART, 3:I2C, 4:PWM, 5:SIO, 6:PIO0, 7:PIO1, 8:GPCK, 9:USB, 31:NULL")]
        public Enabled<byte> Getfct { get; set; }

        #endregion
        public Selftest_gpio()
        {
            Validate = false;
            SelectDir = new Enabled<GpioDir> { Value = GpioDir.Input };
            SelectStr = new Enabled<GpioStr> { Value = GpioStr.mA_4 };
            Selectpull = new Enabled<Gpiopd> { Value = Gpiopd.none };
            SetPads = new Enabled<byte>() { IsEnabled = false, Value = 86 };
            GetPads = new Enabled<byte>() { IsEnabled = false, Value = 86 };
            Getfct = new Enabled<byte>() { IsEnabled = false, Value = 5 };
            Setgpout = new Enabled<Setgpio> { IsEnabled = false, Value = Setgpio.Low };
            Getgpin = new Enabled<Setgpio> { IsEnabled = false, Value = Setgpio.Low };
            I2Caddress = new Enabled<byte>() { IsEnabled = false, Value = Dut.I2CSelftestaddress };  // Get I2C address from DUT

        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        public override void Run()
        {

            if (Selectgp == GpioPins.None)
            {
                Log.Error("No GPIO selected, Select A GPIO");
                UpgradeVerdict(Verdict.Error);
                return;
            }


            // Check if the I2C address is enabled and valid
            if ((I2Caddress.IsEnabled == true) && I2Caddress.Value != 0)
            {
                SetAddress(I2Caddress.Value);
            }

            // ConfigureBasicGpio(string name, byte gpio, byte value, bool check, bool publish)
            if (Selectgp != GpioPins.None) // If Action need to be performed on GPIO on Basic Configuration
            {
                if (SelectDir.IsEnabled == true)
                {
                    ConfigureBasicGpio("Direction", (byte)Selectgp, (byte)SelectDir.Value, EnableDir, Validate);
                }

                if (SelectStr.IsEnabled == true)
                {
                    ConfigureBasicGpio("Strength", (byte)Selectgp, (byte)SelectStr.Value, EnableStr, Validate);
                }

                if (Selectpull.IsEnabled == true)
                {
                    ConfigureBasicGpio("Pull", (byte)Selectgp, (byte)Selectpull.Value, Enablepull, Validate);
                }

                if (Setgpout.IsEnabled == true)
                {
                    ConfigureBasicGpio("Output", (byte)Selectgp, (byte)Setgpout.Value, true, Validate);
                }

                if (Getgpin.IsEnabled == true)
                {
                    ConfigureBasicGpio("Input", (byte)Selectgp, (byte)Getgpin.Value, true, Validate);
                }

                if (SetPads.IsEnabled == true)
                {
                    ConfigureBasicGpio("SetPad", (byte)Selectgp, (byte)SetPads.Value, false, Validate);
                    ConfigureBasicGpio("SetPadgp", (byte)Selectgp, (byte)SetPads.Value, true, Validate);
                }

                if (GetPads.IsEnabled == true)
                {
                    ConfigureBasicGpio("GetPad", (byte)Selectgp, (byte)GetPads.Value, true, Validate);
                }

                if (Getfct.IsEnabled == true)
                {
                    ConfigureBasicGpio("Getfunction", (byte)Selectgp, (byte)Getfct.Value, true, Validate);
                }

            }

            // Check if the I2C address is enabled and valid
            if ((I2Caddress.IsEnabled == true) && I2Caddress.Value != 0)
            {
                SetAddress(Dut.I2CSelftestaddress); // replace the address to the original one
            }


        }

        // <summary>
        /// Change I2C address in configuration 
        ///  <param name="<"Address to use">
        /// 
        public void SetAddress(byte Address)
        {

            string WriteCmd = $"COM:I2C:ADDRESS {Address}";
            Log.Info($"Sending SCPI command: {WriteCmd}");
            IO_Instrument.ScpiCommand(WriteCmd);

            // verify if the address has been set

            string ReadCmd = "COM:I2C:ADDRESS?";
            Log.Info($"Sending SCPI command: {ReadCmd}");
            byte response = IO_Instrument.ScpiQuery<byte>(ReadCmd);
            //Log.Info($"Response: {response}");

            if (response != Address)
            {
                Log.Error($"Failed to set the I2C address at {Address}");
                UpgradeVerdict(Verdict.Fail);
            }
            else
            {
                Log.Info($"I2C address has been set to {Address}");
                UpgradeVerdict(Verdict.Pass);
            }
        }

        /// <summary>
        /// Configures the self-test GPIO based on the provided test name. This method handles different configuration parameters,
        /// </summary>
        /// <param name="name">The name of the configuration parameter to modify, such as "Direction" or "Strength".</param>
        /// <param name="gpio">The gpio number to configure.</param>
        /// <param name="value">The value to write on gpio number.</param>
        /// <param name="check">A flag indicating whether the validation test should be executed.</param>
        /// <param name="publish">A flag indicating whether the configuration change should be published.</param>
        public void ConfigureBasicGpio(string name,byte gpio,byte value, bool check, bool publish)
        {
            byte   Writereg = 0;
            string WriteCmd;
            string ReadCmd;
            byte   Readreg = 0;
            string test;
            string pname = "";
            byte mvalue = 0;
            byte mask = 0xff;  
            string unit;
            byte response;



            switch (name)
            {
                case "Direction":
                    if (value == 0)
                    {
                        Writereg = 21; 
                        pname = "Set Dir Input";
                    }
                    else
                    {
                        Writereg = 20;
                        pname = "Set Dir Output";
                    }
                    Readreg = 25;
                    break;

                case "Output":
                    if (value == 0)
                    {
                        Writereg = 10;
                        pname = "Set Output Low";
                    }
                    else
                    {
                        Writereg = 11;
                        pname = "Set Output High";
                    }
                    Readreg = 15;
                    break;

                case "Input":

                    Writereg = 0;
                    pname = "Read Input";
                    Readreg = 15;
                    break;


                case "Strength":
                    switch (value)
                    {
                        case 0:
                            Writereg = 30;
                            pname = "Set Strength 2mA";
                            break;
                        case 1:
                            Writereg = 31;
                            pname = "Set Strength 4mA";
                            break;
                        case 2:
                            Writereg = 32;
                            pname = "Set Strength 8mA";
                            break;
                        case 3:
                            Writereg = 33;
                            pname = "Set Strength 12mA";
                            break;
                    }
                    Readreg = 35;
                    break;

                case "Pull":
                    switch (value)
                    {
                        case 0:
                            Writereg = 50;
                            pname = "Set Pull:None";
                            Readreg = 65;        // Command to read the pads state byte
                            mask = 0b00001100;   // Mask for bits 2 (pull-up) and 3 (pull-down)
                            break;
                        case 1:
                            Writereg = 41;
                            pname = "Set Pull:Up";
                            Readreg = 45;
                            value = 1;  // expected value
                            break;
                        case 2:
                            Writereg = 51;
                            pname = "Set Pull:Down";
                            Readreg = 55;
                            value = 1; // expected value
                            break;

                    }
                    break;

                case "SetPad":      // Setpas is perform in 2 steps, first one set the pads value, second one set the gpio to pads value
                    Writereg = 60;
                    pname = "Set Pads Value";
                    gpio = value;    // assign the value to gpio
                    Readreg = 0;
                    break;

                case "SetPadgp":
                    Writereg = 61;
                    pname = "Set Gpio Pads";
                    Readreg = 0;
                    break;

                case "GetPad":
                    Writereg = 0;
                    pname = "Read Gpio Pads";
                    Readreg = 65;
                    break;

                case "Getfunction":
                    Writereg = 0;
                    pname = "Read Gpio Fct";
                    Readreg = 75;
                    break;

            }

            if (Writereg != 0)
            {
                WriteCmd = $"COM:I2C:WRITE {Writereg},{gpio}";
                Log.Info($"Sending Write SCPI command: {WriteCmd}");
                IO_Instrument.ScpiCommand(WriteCmd);
                UpgradeVerdict(Verdict.Pass);
            }

            if (Readreg != 0)
            {
                ReadCmd = $"COM:I2C:READ:LEN1? {Readreg},{gpio}";
                Log.Info($"Sending Read SCPI command: {ReadCmd}");
                response = IO_Instrument.ScpiQuery<byte>(ReadCmd);
                Log.Info($"Sending SCPI command: {ReadCmd}, Answer: {response} ,expected:  {value} ");
                mvalue = (byte)(response & mask);
                test = "PASS";

            }

            if (check && Readreg != 0)
            {
                unit = "digcmp";
                if (mvalue == value)
                {
                    test = "PASS";
                }
                else
                {
                    Log.Warning($"Invalid SCPI masked value: {mvalue}, expected:  {value}");
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



        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
 


}
