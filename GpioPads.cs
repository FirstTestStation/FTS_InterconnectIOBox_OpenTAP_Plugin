
using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using static InterconnectIOBox.Portscfg;
using static InterconnectIOBox.PortsIO;

namespace InterconnectIOBox
{
    [Display(Groups: new[] { "InterconnectIO", "GPIO" }, Name: "GPIO PAD Write/Read", Description: "GPIO PAD module is a low level to control the differents option available on each GPIO pin." +
    " PAD function are: Bit 7: OD Ouput Disable, Bit 6: IE Input Enable, Bit 5:4 Drive Strength 0x0:2mA, 0x1:4mA, 0x2:8mA, 0x3:12mA, Bit3: PUE Pull-up Enable, Bit 2: PDE Pull-down Enable" +
    " Bit 1: SCHT Enable Schmidt trigger, Bit 0: SLF Slew rate Control (0: Slow, 1: Fast). The write action will update the PAD value and the read will get the PAD value." +
    " Do not forget of the GPIO are protected and not directly available to the DUT. A fonction like Drive Strength will not work dut to the prection devices. ")]

    public class GpioPAD : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion
      
        public InterconnectIO IO_Instrument { get; set; }
        public enum GpPAD
        {
            Write_read,
            Write_only,
            read_only,
            read_test
        }


        [Display("Action to Execute:", Group: "Gpio PAD Write/Read", Order: 0.4, Description: "Write and/or read PAD value on selected GPIO, read data will be published.")]
        public GpPAD SelectedpAct { get; set; }


        private const string GROUPD = "Dedicated GPIO PAD Write/Read";
        private const string DDESC = "Write/Read a byte on dedicated GPIO PAD.";

        [Display("M1_IO0 Pad:", Group: GROUPD, Order: 2, Collapsed: true, Description: DDESC)]
        public Enabled<byte> M1IO0 { get; set; }

        [Display("M1_IO1 Pad:", Group: GROUPD, Order: 2.1, Collapsed: true, Description: DDESC)]
        public Enabled<byte> M1IO1 { get; set; }

        [Display("S1_IO8 Pad:", Group: GROUPD, Order: 2.2, Collapsed: true, Description: DDESC)]
        public Enabled<byte> S1IO8 { get; set; }

        [Display("S1_IO9 Pad:", Group: GROUPD, Order: 2.3, Collapsed: true, Description: DDESC)]
        public Enabled<byte> S1IO9 { get; set; }

        [Display("FLAG Pad:", Group: GROUPD, Order: 2.4, Collapsed: true, Description: DDESC)]
        public Enabled<byte> FLAG { get; set; }

        [Display("CTRL Pad:", Group: GROUPD, Order: 2.5, Collapsed: true, Description: DDESC)]
        public Enabled<byte> CTRL { get; set; }

        private const string GROUPM = "Master0, Any GPIO PAD Write/Read byte.";
        private const string GPIO_DESC = "Write/Read on any GPIO PAD of any device pin.";

        [Display("Master_Gpio_0 Pad:", Group: GROUPM, Order: 3.00, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP0 { get; set; }

        [Display("Master_Gpio_1 Pad:", Group: GROUPM, Order: 3.01, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP1 { get; set; }

        [Display("Master_Gpio_2 Pad:", Group: GROUPM, Order: 3.02, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP2 { get; set; }

        [Display("Master_Gpio_3 Pad:", Group: GROUPM, Order: 3.03, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP3 { get; set; }

        [Display("Master_Gpio_4 Pad:", Group: GROUPM, Order: 3.04, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP4 { get; set; }

        [Display("Master_Gpio_5 Pad:", Group: GROUPM, Order: 3.05, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP5 { get; set; }

        [Display("Master_Gpio_6 Pad:", Group: GROUPM, Order: 3.06, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP6 { get; set; }

        [Display("Master_Gpio_7 Pad:", Group: GROUPM, Order: 3.07, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP7 { get; set; }

        [Display("Master_Gpio_8 Pad:", Group: GROUPM, Order: 3.08, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP8 { get; set; }

        [Display("Master_Gpio_9 Pad:", Group: GROUPM, Order: 3.09, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP9 { get; set; }

        [Display("Master_Gpio_10 Pad:", Group: GROUPM, Order: 3.10, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP10 { get; set; }

        [Display("Master_Gpio_11 Pad:", Group: GROUPM, Order: 3.11, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP11 { get; set; }

        [Display("Master_Gpio_12 Pad:", Group: GROUPM, Order: 3.12, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP12 { get; set; }

        [Display("Master_Gpio_13 Pad:", Group: GROUPM, Order: 3.13, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP13 { get; set; }

        [Display("Master_Gpio_14 Pad:", Group: GROUPM, Order: 3.14, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP14 { get; set; }

        [Display("Master_Gpio_15 Pad:", Group: GROUPM, Order: 3.15, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP15 { get; set; }

        [Display("Master_Gpio_16 Pad:", Group: GROUPM, Order: 3.16, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP16 { get; set; }

        [Display("Master_Gpio_17 Pad:", Group: GROUPM, Order: 3.17, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP17 { get; set; }

        [Display("Master_Gpio_18 Pad:", Group: GROUPM, Order: 3.18, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP18 { get; set; }

        [Display("Master_Gpio_19 Pad:", Group: GROUPM, Order: 3.19, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP19 { get; set; }

        [Display("Master_Gpio_20 Pad:", Group: GROUPM, Order: 3.20, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP20 { get; set; }

        [Display("Master_Gpio_21 Pad:", Group: GROUPM, Order: 3.21, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP21 { get; set; }

        [Display("Master_Gpio_22 Pad:", Group: GROUPM, Order: 3.22, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP22 { get; set; }

        [Display("Master_Gpio_23 Pad:", Group: GROUPM, Order: 3.23, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP23 { get; set; }

        [Display("Master_Gpio_24 Pad:", Group: GROUPM, Order: 3.24, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP24 { get; set; }

        [Display("Master_Gpio_25 Pad:", Group: GROUPM, Order: 3.25, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP25 { get; set; }

        [Display("Master_Gpio_26 Pad:", Group: GROUPM, Order: 3.26, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP26 { get; set; }

        [Display("Master_Gpio_27 Pad:", Group: GROUPM, Order: 3.27, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP27 { get; set; }

        [Display("Master_Gpio_28 Pad:", Group: GROUPM, Order: 3.28, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> M0GP28 { get; set; }

        private const string GROUPS1 = "Slave1, Any GPIO PAD Write/Read byte.";

        [Display("Slave1_Gpio_0 Pad:", Group: GROUPS1, Order: 4.00, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP0 { get; set; }

        [Display("Slave1_Gpio_1 Pad:", Group: GROUPS1, Order: 4.01, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP1 { get; set; }

        [Display("Slave1_Gpio_2 Pad:", Group: GROUPS1, Order: 4.02, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP2 { get; set; }

        [Display("Slave1_Gpio_3 Pad:", Group: GROUPS1, Order: 4.03, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP3 { get; set; }

        [Display("Slave1_Gpio_4 Pad:", Group: GROUPS1, Order: 4.04, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP4 { get; set; }

        [Display("Slave1_Gpio_5 Pad:", Group: GROUPS1, Order: 4.05, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP5 { get; set; }

        [Display("Slave1_Gpio_6 Pad:", Group: GROUPS1, Order: 4.06, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP6 { get; set; }

        [Display("Slave1_Gpio_7 Pad:", Group: GROUPS1, Order: 4.07, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP7 { get; set; }

        [Display("Slave1_Gpio_8 Pad:", Group: GROUPS1, Order: 4.08, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP8 { get; set; }

        [Display("Slave1_Gpio_9 Pad:", Group: GROUPS1, Order: 4.09, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP9 { get; set; }

        [Display("Slave1_Gpio_10 Pad:", Group: GROUPS1, Order: 4.10, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP10 { get; set; }

        [Display("Slave1_Gpio_11 Pad:", Group: GROUPS1, Order: 4.11, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP11 { get; set; }

        [Display("Slave1_Gpio_12 Pad:", Group: GROUPS1, Order: 4.12, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP12 { get; set; }

        [Display("Slave1_Gpio_13 Pad:", Group: GROUPS1, Order: 4.13, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP13 { get; set; }

        [Display("Slave1_Gpio_14 Pad:", Group: GROUPS1, Order: 4.14, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP14 { get; set; }

        [Display("Slave1_Gpio_15 Pad:", Group: GROUPS1, Order: 4.15, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP15 { get; set; }

        [Display("Slave1_Gpio_16 Pad:", Group: GROUPS1, Order: 4.16, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP16 { get; set; }

        [Display("Slave1_Gpio_17 Pad:", Group: GROUPS1, Order: 4.17, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP17 { get; set; }

        [Display("Slave1_Gpio_18 Pad:", Group: GROUPS1, Order: 4.18, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP18 { get; set; }

        [Display("Slave1_Gpio_19 Pad:", Group: GROUPS1, Order: 4.19, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP19 { get; set; }

        [Display("Slave1_Gpio_20 Pad:", Group: GROUPS1, Order: 4.20, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP20 { get; set; }

        [Display("Slave1_Gpio_21 Pad:", Group: GROUPS1, Order: 4.21, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP21 { get; set; }

        [Display("Slave1_Gpio_22 Pad:", Group: GROUPS1, Order: 4.22, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP22 { get; set; }

        [Display("Slave1_Gpio_23 Pad:", Group: GROUPS1, Order: 4.23, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP23 { get; set; }

        [Display("Slave1_Gpio_24 Pad:", Group: GROUPS1, Order: 4.24, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP24 { get; set; }

        [Display("Slave1_Gpio_25 Pad:", Group: GROUPS1, Order: 4.25, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP25 { get; set; }

        [Display("Slave1_Gpio_26 Pad:", Group: GROUPS1, Order: 4.26, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP26 { get; set; }

        [Display("Slave1_Gpio_27 Pad:", Group: GROUPS1, Order: 4.27, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP27 { get; set; }

        [Display("Slave1_Gpio_28 Pad:", Group: GROUPS1, Order: 4.28, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S1GP28 { get; set; }

        private const string GROUPS2 = "Slave2, Any GPIO PAD Write/Read byte.";

        [Display("Slave2_Gpio_0 Pad:", Group: GROUPS2, Order: 5.00, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP0 { get; set; }

        [Display("Slave2_Gpio_1 Pad:", Group: GROUPS2, Order: 5.01, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP1 { get; set; }

        [Display("Slave2_Gpio_2 Pad:", Group: GROUPS2, Order: 5.02, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP2 { get; set; }

        [Display("Slave2_Gpio_3 Pad:", Group: GROUPS2, Order: 5.03, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP3 { get; set; }

        [Display("Slave2_Gpio_4 Pad:", Group: GROUPS2, Order: 5.04, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP4 { get; set; }

        [Display("Slave2_Gpio_5 Pad:", Group: GROUPS2, Order: 5.05, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP5 { get; set; }

        [Display("Slave2_Gpio_6 Pad:", Group: GROUPS2, Order: 5.06, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP6 { get; set; }

        [Display("Slave2_Gpio_7 Pad:", Group: GROUPS2, Order: 5.07, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP7 { get; set; }

        [Display("Slave2_Gpio_8 Pad:", Group: GROUPS2, Order: 5.08, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP8 { get; set; }

        [Display("Slave2_Gpio_9 Pad:", Group: GROUPS2, Order: 5.09, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP9 { get; set; }

        [Display("Slave2_Gpio_10 Pad:", Group: GROUPS2, Order: 5.10, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP10 { get; set; }

        [Display("Slave2_Gpio_11 Pad:", Group: GROUPS2, Order: 5.11, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP11 { get; set; }

        [Display("Slave2_Gpio_12 Pad:", Group: GROUPS2, Order: 5.12, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP12 { get; set; }

        [Display("Slave2_Gpio_13 Pad:", Group: GROUPS2, Order: 5.13, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP13 { get; set; }

        [Display("Slave2_Gpio_14 Pad:", Group: GROUPS2, Order: 5.14, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP14 { get; set; }

        [Display("Slave2_Gpio_15 Pad:", Group: GROUPS2, Order: 5.15, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP15 { get; set; }

        [Display("Slave2_Gpio_16 Pad:", Group: GROUPS2, Order: 5.16, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP16 { get; set; }

        [Display("Slave2_Gpio_17 Pad:", Group: GROUPS2, Order: 5.17, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP17 { get; set; }

        [Display("Slave2_Gpio_18 Pad:", Group: GROUPS2, Order: 5.18, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP18 { get; set; }

        [Display("Slave2_Gpio_19 Pad:", Group: GROUPS2, Order: 5.19, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP19 { get; set; }

        [Display("Slave2_Gpio_20 Pad:", Group: GROUPS2, Order: 5.20, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP20 { get; set; }

        [Display("Slave2_Gpio_21 Pad:", Group: GROUPS2, Order: 5.21, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP21 { get; set; }

        [Display("Slave2_Gpio_22 Pad:", Group: GROUPS2, Order: 5.22, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP22 { get; set; }

        [Display("Slave2_Gpio_23 Pad:", Group: GROUPS2, Order: 5.23, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP23 { get; set; }

        [Display("Slave2_Gpio_24 Pad:", Group: GROUPS2, Order: 5.24, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP24 { get; set; }

        [Display("Slave2_Gpio_25 Pad:", Group: GROUPS2, Order: 5.25, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP25 { get; set; }

        [Display("Slave2_Gpio_26 Pad:", Group: GROUPS2, Order: 5.26, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP26 { get; set; }

        [Display("Slave2_Gpio_27 Pad:", Group: GROUPS2, Order: 5.27, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP27 { get; set; }

        [Display("Slave2_Gpio_28 Pad:", Group: GROUPS2, Order: 5.28, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S2GP28 { get; set; }

        private const string GROUPS3 = "Slave3,Any GPIO PAD Write/Read byte.";

        [Display("Slave3_Gpio_0 Pad:", Group: GROUPS3, Order: 6.00, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP0 { get; set; }

        [Display("Slave3_Gpio_1 Pad:", Group: GROUPS3, Order: 6.01, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP1 { get; set; }

        [Display("Slave3_Gpio_2 Pad:", Group: GROUPS3, Order: 6.02, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP2 { get; set; }

        [Display("Slave3_Gpio_3 Pad:", Group: GROUPS3, Order: 6.03, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP3 { get; set; }

        [Display("Slave3_Gpio_4 Pad:", Group: GROUPS3, Order: 6.04, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP4 { get; set; }

        [Display("Slave3_Gpio_5 Pad:", Group: GROUPS3, Order: 6.05, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP5 { get; set; }

        [Display("Slave3_Gpio_6 Pad:", Group: GROUPS3, Order: 6.06, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP6 { get; set; }

        [Display("Slave3_Gpio_7 Pad:", Group: GROUPS3, Order: 6.07, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP7 { get; set; }

        [Display("Slave3_Gpio_8 Pad:", Group: GROUPS3, Order: 6.08, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP8 { get; set; }

        [Display("Slave3_Gpio_9 Pad:", Group: GROUPS3, Order: 6.09, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP9 { get; set; }

        [Display("Slave3_Gpio_10 Pad:", Group: GROUPS3, Order: 6.10, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP10 { get; set; }

        [Display("Slave3_Gpio_11 Pad:", Group: GROUPS3, Order: 6.11, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP11 { get; set; }

        [Display("Slave3_Gpio_12 Pad:", Group: GROUPS3, Order: 6.12, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP12 { get; set; }

        [Display("Slave3_Gpio_13 Pad:", Group: GROUPS3, Order: 6.13, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP13 { get; set; }

        [Display("Slave3_Gpio_14 Pad:", Group: GROUPS3, Order: 6.14, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP14 { get; set; }

        [Display("Slave3_Gpio_15 Pad:", Group: GROUPS3, Order: 6.15, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP15 { get; set; }

        [Display("Slave3_Gpio_16 Pad:", Group: GROUPS3, Order: 6.16, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP16 { get; set; }

        [Display("Slave3_Gpio_17 Pad:", Group: GROUPS3, Order: 6.17, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP17 { get; set; }

        [Display("Slave3_Gpio_18 Pad:", Group: GROUPS3, Order: 6.18, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP18 { get; set; }

        [Display("Slave3_Gpio_19 Pad:", Group: GROUPS3, Order: 6.19, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP19 { get; set; }

        [Display("Slave3_Gpio_20 Pad:", Group: GROUPS3, Order: 6.20, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP20 { get; set; }

        [Display("Slave3_Gpio_21 Pad:", Group: GROUPS3, Order: 6.21, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP21 { get; set; }

        [Display("Slave3_Gpio_22 Pad:", Group: GROUPS3, Order: 6.22, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP22 { get; set; }

        [Display("Slave3_Gpio_23 Pad:", Group: GROUPS3, Order: 6.23, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP23 { get; set; }

        [Display("Slave3_Gpio_24 Pad:", Group: GROUPS3, Order: 6.24, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP24 { get; set; }

        [Display("Slave3_Gpio_25 Pad:", Group: GROUPS3, Order: 6.25, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP25 { get; set; }

        [Display("Slave3_Gpio_26 Pad:", Group: GROUPS3, Order: 6.26, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP26 { get; set; }

        [Display("Slave3_Gpio_27 Pad:", Group: GROUPS3, Order: 6.27, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP27 { get; set; }

        [Display("Slave3_Gpio_28 Pad:", Group: GROUPS3, Order: 6.28, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<byte> S3GP28 { get; set; }

        public GpioPAD()
        {
            M1IO0 = new Enabled<byte> { Value = 86 };
            M1IO1 = new Enabled<byte> { Value = 86 };
            S1IO8 = new Enabled<byte> { Value = 86 };
            S1IO9 = new Enabled<byte> { Value = 86 };
            FLAG = new Enabled<byte> { Value = 86 };
            CTRL = new Enabled<byte> { Value = 86 };

            M0GP0 = new Enabled<byte> { Value = 86 }; M0GP1 = new Enabled<byte> { Value = 86 }; M0GP2 = new Enabled<byte> { Value = 86 }; M0GP3 = new Enabled<byte> { Value = 86 };
            M0GP4 = new Enabled<byte> { Value = 86 }; M0GP5 = new Enabled<byte> { Value = 86 }; M0GP6 = new Enabled<byte> { Value = 86 }; M0GP7 = new Enabled<byte> { Value = 86 };
            M0GP8 = new Enabled<byte> { Value = 86 }; M0GP9 = new Enabled<byte> { Value = 86 }; M0GP10 = new Enabled<byte> { Value = 86 }; M0GP11 = new Enabled<byte> { Value = 86 };
            M0GP12 = new Enabled<byte> { Value = 86 }; M0GP13 = new Enabled<byte> { Value = 86 }; M0GP14 = new Enabled<byte> { Value = 86 }; M0GP15 = new Enabled<byte> { Value = 86 };
            M0GP16 = new Enabled<byte> { Value = 86 }; M0GP17 = new Enabled<byte> { Value = 86 }; M0GP18 = new Enabled<byte> { Value = 86 }; M0GP19 = new Enabled<byte> { Value = 86 };
            M0GP20 = new Enabled<byte> { Value = 86 }; M0GP21 = new Enabled<byte> { Value = 86 }; M0GP22 = new Enabled<byte> { Value = 86 }; M0GP23 = new Enabled<byte> { Value = 86 };
            M0GP24 = new Enabled<byte> { Value = 86 }; M0GP25 = new Enabled<byte> { Value = 86 }; M0GP26 = new Enabled<byte> { Value = 86 }; M0GP27 = new Enabled<byte> { Value = 86 };
            M0GP28 = new Enabled<byte> { Value = 86 };

            S1GP0 = new Enabled<byte> { Value = 86 }; S1GP1 = new Enabled<byte> { Value = 86 }; S1GP2 = new Enabled<byte> { Value = 86 }; S1GP3 = new Enabled<byte> { Value = 86 };
            S1GP4 = new Enabled<byte> { Value = 86 }; S1GP5 = new Enabled<byte> { Value = 86 }; S1GP6 = new Enabled<byte> { Value = 86 }; S1GP7 = new Enabled<byte> { Value = 86 };
            S1GP8 = new Enabled<byte> { Value = 86 }; S1GP9 = new Enabled<byte> { Value = 86 }; S1GP10 = new Enabled<byte> { Value = 86 }; S1GP11 = new Enabled<byte> { Value = 86 };
            S1GP12 = new Enabled<byte> { Value = 86 }; S1GP13 = new Enabled<byte> { Value = 86 }; S1GP14 = new Enabled<byte> { Value = 86 }; S1GP15 = new Enabled<byte> { Value = 86 };
            S1GP16 = new Enabled<byte> { Value = 86 }; S1GP17 = new Enabled<byte> { Value = 86 }; S1GP18 = new Enabled<byte> { Value = 86 }; S1GP19 = new Enabled<byte> { Value = 86 };
            S1GP20 = new Enabled<byte> { Value = 86 }; S1GP21 = new Enabled<byte> { Value = 86 }; S1GP22 = new Enabled<byte> { Value = 86 }; S1GP23 = new Enabled<byte> { Value = 86 };
            S1GP24 = new Enabled<byte> { Value = 86 }; S1GP25 = new Enabled<byte> { Value = 86 }; S1GP26 = new Enabled<byte> { Value = 86 }; S1GP27 = new Enabled<byte> { Value = 86 };
            S1GP28 = new Enabled<byte> { Value = 86 };

            S2GP0 = new Enabled<byte> { Value = 86 }; S2GP1 = new Enabled<byte> { Value = 86 }; S2GP2 = new Enabled<byte> { Value = 86 }; S2GP3 = new Enabled<byte> { Value = 86 };
            S2GP4 = new Enabled<byte> { Value = 86 }; S2GP5 = new Enabled<byte> { Value = 86 }; S2GP6 = new Enabled<byte> { Value = 86 }; S2GP7 = new Enabled<byte> { Value = 86 };
            S2GP8 = new Enabled<byte> { Value = 86 }; S2GP9 = new Enabled<byte> { Value = 86 }; S2GP10 = new Enabled<byte> { Value = 86 }; S2GP11 = new Enabled<byte> { Value = 86 };
            S2GP12 = new Enabled<byte> { Value = 86 }; S2GP13 = new Enabled<byte> { Value = 86 }; S2GP14 = new Enabled<byte> { Value = 86 }; S2GP15 = new Enabled<byte> { Value = 86 };
            S2GP16 = new Enabled<byte> { Value = 86 }; S2GP17 = new Enabled<byte> { Value = 86 }; S2GP18 = new Enabled<byte> { Value = 86 }; S2GP19 = new Enabled<byte> { Value = 86 };
            S2GP20 = new Enabled<byte> { Value = 86 }; S2GP21 = new Enabled<byte> { Value = 86 }; S2GP22 = new Enabled<byte> { Value = 86 }; S2GP23 = new Enabled<byte> { Value = 86 };
            S2GP24 = new Enabled<byte> { Value = 86 }; S2GP25 = new Enabled<byte> { Value = 86 }; S2GP26 = new Enabled<byte> { Value = 86 }; S2GP27 = new Enabled<byte> { Value = 86 };
            S2GP28 = new Enabled<byte> { Value = 86 };

            S3GP0 = new Enabled<byte> { Value = 86 }; S3GP1 = new Enabled<byte> { Value = 86 }; S3GP2 = new Enabled<byte> { Value = 86 }; S3GP3 = new Enabled<byte> { Value = 86 };
            S3GP4 = new Enabled<byte> { Value = 86 }; S3GP5 = new Enabled<byte> { Value = 86 }; S3GP6 = new Enabled<byte> { Value = 86 }; S3GP7 = new Enabled<byte> { Value = 86 };
            S3GP8 = new Enabled<byte> { Value = 86 }; S3GP9 = new Enabled<byte> { Value = 86 }; S3GP10 = new Enabled<byte> { Value = 86 }; S3GP11 = new Enabled<byte> { Value = 86 };
            S3GP12 = new Enabled<byte> { Value = 86 }; S3GP13 = new Enabled<byte> { Value = 86 }; S3GP14 = new Enabled<byte> { Value = 86 }; S3GP15 = new Enabled<byte> { Value = 86 };
            S3GP16 = new Enabled<byte> { Value = 86 }; S3GP17 = new Enabled<byte> { Value = 86 }; S3GP18 = new Enabled<byte> { Value = 86 }; S3GP19 = new Enabled<byte> { Value = 86 };
            S3GP20 = new Enabled<byte> { Value = 86 }; S3GP21 = new Enabled<byte> { Value = 86 }; S3GP22 = new Enabled<byte> { Value = 86 }; S3GP23 = new Enabled<byte> { Value = 86 };
            S3GP24 = new Enabled<byte> { Value = 86 }; S3GP25 = new Enabled<byte> { Value = 86 }; S3GP26 = new Enabled<byte> { Value = 86 }; S3GP27 = new Enabled<byte> { Value = 86 };
            S3GP28 = new Enabled<byte> { Value = 86 };

        }


        public override void PrePlanRun()
        {
            base.PrePlanRun();
        }

     
        /// <summary>
        /// Instantiate an OpenTAP picture with some default picture
        /// These can be controlled by other test step properties if they should be configurable, or they can be hardcoded values
        /// </summary>
        public Picture Picture { get; } = new Picture()
        {
            Source = "PicoPad.jpg",
            Description = "A Picture of the GPIO PAD to help configuration."
        };

        /// <summary>
        /// Control the source of the picture with a regular test step property
        /// </summary>
        [Display("Source", "The source of the picture. This can be a URL or a file path.", "GPIO PAD Picture", Order: 0.1, Collapsed: true)]
        [FilePath(FilePathAttribute.BehaviorChoice.Open)]
        public string PictureSource
        {
            get => Picture.Source;
            set => Picture.Source = value;
        }

     
        public override void Run()
        {
       
            if ((M1IO0.IsEnabled == true) || (M1IO1.IsEnabled == true) || (S1IO8.IsEnabled == true) || (S1IO9.IsEnabled == true) || (FLAG.IsEnabled == true) || (CTRL.IsEnabled == true))
            {
                ConfigureDefGpio(SelectedpAct);
            }

            if ((M0GP0.IsEnabled == true) || (M0GP1.IsEnabled == true) || (M0GP2.IsEnabled == true) || (M0GP3.IsEnabled == true) || (M0GP4.IsEnabled == true) || (M0GP5.IsEnabled == true) || (M0GP6.IsEnabled == true) || (M0GP7.IsEnabled == true) || (M0GP8.IsEnabled == true) || (M0GP9.IsEnabled == true) || (M0GP10.IsEnabled == true) || (M0GP11.IsEnabled == true) || (M0GP12.IsEnabled == true) || (M0GP13.IsEnabled == true) || (M0GP14.IsEnabled == true) || (M0GP15.IsEnabled == true) || (M0GP16.IsEnabled == true) || (M0GP17.IsEnabled == true) || (M0GP18.IsEnabled == true) || (M0GP19.IsEnabled == true) || (M0GP20.IsEnabled == true) || (M0GP21.IsEnabled == true) || (M0GP22.IsEnabled == true) || (M0GP23.IsEnabled == true) || (M0GP24.IsEnabled == true) || (M0GP25.IsEnabled == true) || (M0GP26.IsEnabled == true) || (M0GP27.IsEnabled == true) || (M0GP28.IsEnabled == true))
            {
                ConfigureAllGpio(0, SelectedpAct);
            }

            if ((S1GP0.IsEnabled == true) || (S1GP1.IsEnabled == true) || (S1GP2.IsEnabled == true) || (S1GP3.IsEnabled == true) || (S1GP4.IsEnabled == true) || (S1GP5.IsEnabled == true) || (S1GP6.IsEnabled == true) || (S1GP7.IsEnabled == true) || (S1GP8.IsEnabled == true) || (S1GP9.IsEnabled == true) || (S1GP10.IsEnabled == true) || (S1GP11.IsEnabled == true) || (S1GP12.IsEnabled == true) || (S1GP13.IsEnabled == true) || (S1GP14.IsEnabled == true) || (S1GP15.IsEnabled == true) || (S1GP16.IsEnabled == true) || (S1GP17.IsEnabled == true) || (S1GP18.IsEnabled == true) || (S1GP19.IsEnabled == true) || (S1GP20.IsEnabled == true) || (S1GP21.IsEnabled == true) || (S1GP22.IsEnabled == true) || (S1GP23.IsEnabled == true) || (S1GP24.IsEnabled == true) || (S1GP25.IsEnabled == true) || (S1GP26.IsEnabled == true) || (S1GP27.IsEnabled == true) || (S1GP28.IsEnabled == true))
            {
                ConfigureAllGpio(1, SelectedpAct);
            }

            if ((S2GP0.IsEnabled == true) || (S2GP1.IsEnabled == true) || (S2GP2.IsEnabled == true) || (S2GP3.IsEnabled == true) || (S2GP4.IsEnabled == true) || (S2GP5.IsEnabled == true) || (S2GP6.IsEnabled == true) || (S2GP7.IsEnabled == true) || (S2GP8.IsEnabled == true) || (S2GP9.IsEnabled == true) || (S2GP10.IsEnabled == true) || (S2GP11.IsEnabled == true) || (S2GP12.IsEnabled == true) || (S2GP13.IsEnabled == true) || (S2GP14.IsEnabled == true) || (S2GP15.IsEnabled == true) || (S2GP16.IsEnabled == true) || (S2GP17.IsEnabled == true) || (S2GP18.IsEnabled == true) || (S2GP19.IsEnabled == true) || (S2GP20.IsEnabled == true) || (S2GP21.IsEnabled == true) || (S2GP22.IsEnabled == true) || (S2GP23.IsEnabled == true) || (S2GP24.IsEnabled == true) || (S2GP25.IsEnabled == true) || (S2GP26.IsEnabled == true) || (S2GP27.IsEnabled == true) || (S2GP28.IsEnabled == true))
            {
                ConfigureAllGpio(2, SelectedpAct);
            }

            if ((S3GP0.IsEnabled == true) || (S3GP1.IsEnabled == true) || (S3GP2.IsEnabled == true) || (S3GP3.IsEnabled == true) || (S3GP4.IsEnabled == true) || (S3GP5.IsEnabled == true) || (S3GP6.IsEnabled == true) || (S3GP7.IsEnabled == true) || (S3GP8.IsEnabled == true) || (S3GP9.IsEnabled == true) || (S3GP10.IsEnabled == true) || (S3GP11.IsEnabled == true) || (S3GP12.IsEnabled == true) || (S3GP13.IsEnabled == true) || (S3GP14.IsEnabled == true) || (S3GP15.IsEnabled == true) || (S3GP16.IsEnabled == true) || (S3GP17.IsEnabled == true) || (S3GP18.IsEnabled == true) || (S3GP19.IsEnabled == true) || (S3GP20.IsEnabled == true) || (S3GP21.IsEnabled == true) || (S3GP22.IsEnabled == true) || (S3GP23.IsEnabled == true) || (S3GP24.IsEnabled == true) || (S3GP25.IsEnabled == true) || (S3GP26.IsEnabled == true) || (S3GP27.IsEnabled == true) || (S3GP28.IsEnabled == true))
            {
                ConfigureAllGpio(3, SelectedpAct);
            }
        }


        /// <summary>
        /// Write read on any device any Gpio between 0 and 28.
        /// </summary>
        /// <param name="device"> Device number (0 for Master, 1 for Slave1, 2 for Slave2 and 3 for Slave3).</param>
        /// <param name="portValue">The byte value for configuration.</param>
        /// <param name="selectedFunction">Selected function for the Gpio.</param>
        private void ConfigureAllGpio(int device, GpPAD selectedFunction)
        {
            byte pvalue;
            string name;

            for (int bit = 0; bit < 29; bit++)
            {

                // Skip bits that are not enabled
                if (!IsBitEnabled(device, bit)) continue;

                (pvalue, name) = GetPortBitValue(device, bit);



                // Write Command
                if ((selectedFunction == GpPAD.Write_read) || (selectedFunction == GpPAD.Write_only))
                {
                    string Command = $"GPIO:SETPAD:DEVICE{device}:GP{bit} {pvalue}";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read Command
                if ((selectedFunction == GpPAD.read_only) || (selectedFunction == GpPAD.Write_read) || (selectedFunction == GpPAD.read_test))
                {
                    string Command = $"GPIO:GETPAD:DEVICE{device}:GP{bit}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

                    double.TryParse(response, out double value);
                    string test = (value == Convert.ToDouble(pvalue) ? "PASS" : "FAIL");


                    if (selectedFunction == GpPAD.read_only)
                    {
                        test = "PASS"; // No verdict on read only
                    }

                    // Create test result object
                    TestResult<double> result = new TestResult<double>
                    {
                        ParamName = $"{name} Bit{bit} Pad",
                        StepName = Name,
                        Value = value,
                        Verdict = test,
                        Units = "read"
                    };

                    if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                    else UpgradeVerdict(Verdict.Fail);


                    // Add limits for Write_Read function
                    if ((selectedFunction == GpPAD.Write_read) || (selectedFunction == GpPAD.read_test))
                    {
                        result.LowerLimit = pvalue;
                        result.UpperLimit = pvalue;
                        result.Units = "digcmp";
                    }

                    PublishResult(result);
                }
            }
        }



        // <summary>
        /// Write or Read PAD byte on Dedicated GPIO

        /// <param name="selectedFunction">Selected function for action.</param>
        /// 
        public void ConfigureDefGpio(GpPAD selectedFunction)
        {
            Dictionary<string, (byte Device, byte Pin)> IOConfigs = new Dictionary<string, (byte Device, byte Pin)>
            {
                { "M1IO0", (0, 0) },
                { "M1IO1", (0, 1) },
                { "S1IO8", (1, 8) },
                { "S1IO9", (1, 9) },
                { "FLAG", (1, 18) },
                { "CTRL", (1, 19) }
            };

            foreach (var entry in IOConfigs)
            {
                string name = entry.Key;                   // Extract name (key)
                byte device = entry.Value.Device;          // Extract device number
                byte pin = entry.Value.Pin;                // Extract pin number
                var config = entry.Value;        // Get the Enabled<GpIO> object
                string test = "";


                var property = GetType().GetProperty(name);

                var bitConfig = property?.GetValue(this) as Enabled<byte>;

                if (bitConfig == null || !bitConfig.IsEnabled)
                    continue;
                byte padgpio = (byte)(bitConfig.Value);

                // Write Command
                if ((selectedFunction == GpPAD.Write_read) || (selectedFunction == GpPAD.Write_only))
                {
                    string Command = $"GPIO:SETPAD:DEVICE{device}:GP{pin} {padgpio}";
                    Log.Info($"Sending SCPI command for {name}: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read Command
                if ((selectedFunction == GpPAD.read_only) || (selectedFunction == GpPAD.Write_read) || (selectedFunction == GpPAD.read_test))
                {
                    string Command = $"GPIO:GETPAD:DEVICE{device}:GP{pin}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"Sending SCPI command for {name}: {Command}, Answer: {response}");


                    double.TryParse(response, out double value);
                    test = (value == Convert.ToDouble(padgpio)) ? "PASS" : "FAIL";


                    if (selectedFunction == GpPAD.read_only)
                    {
                        test = "PASS"; // No verdict on read only
                    }
                    else
                    {
                        // Test the answer
                        if (value != padgpio)
                        {
                            Log.Warning($"Invalid SCPI response: {response}, expected: {padgpio}");
                            UpgradeVerdict(Verdict.Fail);
                            test = "FAIL";
                        }
                        else
                        {
                            test = "PASS";
                            UpgradeVerdict(Verdict.Pass);
                        }

                    }


                    // Create test result object
                    TestResult<double> result = new TestResult<double>
                    {
                        ParamName = $"{name} Bit{pin} Pad",
                        StepName = Name,
                        Value = value,
                        Verdict = test,
                        Units = "read"
                    };

                    // Add limits for Write_Read function
                    if (selectedFunction == GpPAD.Write_read || selectedFunction == GpPAD.read_test)
                    {
                        result.LowerLimit = padgpio;
                        result.UpperLimit = padgpio;
                        result.Units = "digcmp";
                    }

                    PublishResult(result);
                }
            }
        }


        private (byte, string) GetPortBitValue(int device, int bit)
        {
            string dev = (device == 0) ? "M0" : $"S{device}";
            string propertyName = $"{dev}GP{bit}";
            var property = GetType().GetProperty(propertyName);

            if (property != null)
            {
                var bitConfig = property.GetValue(this) as Enabled<byte>;
                if (bitConfig?.IsEnabled == true)
                {
                    byte value = (byte)(bitConfig.Value);
                    return (value, propertyName);  // Return both bit value and property name
                }
            }
            return (0, propertyName); // Default to bit = 0 with property name
        }

        private bool IsBitEnabled(int device, int bit)
        {
            string dev = (device == 0) ? "M0" : $"S{device}";
            string propertyName = $"{dev}GP{bit}";
            var property = GetType().GetProperty(propertyName);

            return property?.GetValue(this) is Enabled<byte> bitConfig && bitConfig.IsEnabled;
        }
        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }

    }
    [Display("Confirm instrument configuration")]
    class PictureDialogRequest
    {
        public PictureDialogRequest()
        {
        }

        /// <summary>
        /// The layout of a picture can be controlled using the Layout attribute, just like other UserInput members
        /// </summary>
        [Layout(LayoutMode.FullRow)]
        [Display("PAD Picture to help", Order: 0.01)]
        public Picture Picture { get; set; }

        [Layout(LayoutMode.FullRow, rowHeight: 2)]
        [Browsable(true)]
        [Display("Message", Order: 0.02)]
        public string Question { get; }
    }

}
