
using OpenTap;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using static InterconnectIOBox.Portscfg;
using static InterconnectIOBox.PortsIO;

namespace InterconnectIOBox
{
        [Display(Groups: new[] { "InterconnectIO", "GPIO" }, Name: "GPIO Input/Output Bit Write/Read", Description: "Write or Read data of GPIO pin on dedicated GPIO or on GPIO of any device." +
        " Enabling GPIO update is done by checking the enable checkbox and selecting a value to write or read(0 or 1). The read function operates similarly—enable the bit" +
        " to verify and set the value.The read result is then published. Be Careful using the Any Gpio control, it is an advanced mode and could cause malfunction")]

    public class GpioIO : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        public InterconnectIO IO_Instrument { get; set; }
        public enum GpIO
        {
            Write_read,
            Write_only,
            read_only,
            read_test
        }


        [Display("Action to Execute:", Group: "Gpio bit Write/Read",Order: 0.1, Description: "Write and/or read bit on selected GPIO, read data will be published.")]
        public GpIO SelectedgAct { get; set; }

        public enum GBit
        {
            Low = 0,
            High = 1
        }

        private const string GROUPD = "Dedicated GPIO Write/Read bit";
        private const string DDESC = "Write/Read on dedicated GPIO bit.(0: Low, 1: High)";

        [Display("M1_IO0 State:", Group: GROUPD, Order: 2, Collapsed: true, Description: DDESC)]
        public Enabled<GBit> M1IO0 { get; set; }

        [Display("M1_IO1 State:", Group: GROUPD, Order: 2.1, Collapsed: true, Description: DDESC)]
        public Enabled<GBit> M1IO1 { get; set; }

        [Display("S1_IO8 State:", Group: GROUPD, Order: 2.2, Collapsed: true, Description: DDESC)]
        public Enabled<GBit> S1IO8 { get; set; }

        [Display("S1_IO9 State:", Group: GROUPD, Order: 2.3, Collapsed: true, Description: DDESC)]
        public Enabled<GBit> S1IO9 { get; set; }

        [Display("FLAG State:", Group: GROUPD, Order: 2.4, Collapsed: true, Description: DDESC)]
        public Enabled<GBit> FLAG { get; set; }

        [Display("CTRL State:", Group: GROUPD, Order: 2.5, Collapsed: true, Description: DDESC)]
        public Enabled<GBit> CTRL { get; set; }

        private const string GROUPM = "Master0, Any GPIO bit Write/Read bit.";
        private const string GPIO_DESC = "Write/Read on any GPIO device bit by selecting.(0: Low, 1: High)";

        [Display("Master_Gpio_0 State:", Group : GROUPM, Order : 3.00, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP0 { get; set; }

        [Display("Master_Gpio_1 State:", Group : GROUPM, Order : 3.01, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP1 { get; set; }

        [Display("Master_Gpio_2 State:", Group : GROUPM, Order : 3.02, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP2 { get; set; }

        [Display("Master_Gpio_3 State:", Group : GROUPM, Order : 3.03, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP3 { get; set; }

        [Display("Master_Gpio_4 State:", Group : GROUPM, Order : 3.04, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP4 { get; set; }

        [Display("Master_Gpio_5 State:", Group : GROUPM, Order : 3.05, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP5 { get; set; }

        [Display("Master_Gpio_6 State:", Group : GROUPM, Order : 3.06, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP6 { get; set; }

        [Display("Master_Gpio_7 State:", Group : GROUPM, Order : 3.07, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP7 { get; set; }

        [Display("Master_Gpio_8 State:", Group : GROUPM, Order : 3.08, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP8 { get; set; }

        [Display("Master_Gpio_9 State:", Group : GROUPM, Order : 3.09, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP9 { get; set; }

        [Display("Master_Gpio_10 State:", Group : GROUPM, Order : 3.10, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP10 { get; set; }

        [Display("Master_Gpio_11 State:", Group : GROUPM, Order : 3.11, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP11 { get; set; }

        [Display("Master_Gpio_12 State:", Group : GROUPM, Order : 3.12, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP12 { get; set; }

        [Display("Master_Gpio_13 State:", Group : GROUPM, Order : 3.13, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP13 { get; set; }

        [Display("Master_Gpio_14 State:", Group : GROUPM, Order : 3.14, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP14 { get; set; }

        [Display("Master_Gpio_15 State:", Group : GROUPM, Order : 3.15, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP15 { get; set; }

        [Display("Master_Gpio_16 State:", Group : GROUPM, Order : 3.16, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP16 { get; set; }

        [Display("Master_Gpio_17 State:", Group : GROUPM, Order : 3.17, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP17 { get; set; }

        [Display("Master_Gpio_18 State:", Group : GROUPM, Order : 3.18, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP18 { get; set; }

        [Display("Master_Gpio_19 State:", Group : GROUPM, Order : 3.19, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP19 { get; set; }

        [Display("Master_Gpio_20 State:", Group : GROUPM, Order : 3.20, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP20 { get; set; }

        [Display("Master_Gpio_21 State:", Group : GROUPM, Order : 3.21, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP21 { get; set; }

        [Display("Master_Gpio_22 State:", Group : GROUPM, Order : 3.22, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP22 { get; set; }

        [Display("Master_Gpio_23 State:", Group : GROUPM, Order : 3.23, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP23 { get; set; }

        [Display("Master_Gpio_24 State:", Group : GROUPM, Order : 3.24, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP24 { get; set; }

        [Display("Master_Gpio_25 State:", Group : GROUPM, Order : 3.25, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP25 { get; set; }

        [Display("Master_Gpio_26 State:", Group : GROUPM, Order : 3.26, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP26 { get; set; }

        [Display("Master_Gpio_27 State:", Group : GROUPM, Order : 3.27, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP27 { get; set; }

        [Display("Master_Gpio_28 State:", Group : GROUPM, Order : 3.28, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GBit> M0GP28 { get; set; }

        private const string GROUPS1 = "Slave1, Any GPIO bit Write/Read bit.";
    
        [Display("Slave1_Gpio_0 State:", Group: GROUPS1, Order: 4.00, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP0 { get; set; }

        [Display("Slave1_Gpio_1 State:", Group: GROUPS1, Order: 4.01, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP1 { get; set; }

        [Display("Slave1_Gpio_2 State:", Group: GROUPS1, Order: 4.02, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP2 { get; set; }

        [Display("Slave1_Gpio_3 State:", Group: GROUPS1, Order: 4.03, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP3 { get; set; }

        [Display("Slave1_Gpio_4 State:", Group: GROUPS1, Order: 4.04, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP4 { get; set; }

        [Display("Slave1_Gpio_5 State:", Group: GROUPS1, Order: 4.05, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP5 { get; set; }

        [Display("Slave1_Gpio_6 State:", Group: GROUPS1, Order: 4.06, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP6 { get; set; }

        [Display("Slave1_Gpio_7 State:", Group: GROUPS1, Order: 4.07, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP7 { get; set; }

        [Display("Slave1_Gpio_8 State:", Group: GROUPS1, Order: 4.08, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP8 { get; set; }

        [Display("Slave1_Gpio_9 State:", Group: GROUPS1, Order: 4.09, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP9 { get; set; }

        [Display("Slave1_Gpio_10 State:", Group: GROUPS1, Order: 4.10, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP10 { get; set; }

        [Display("Slave1_Gpio_11 State:", Group: GROUPS1, Order: 4.11, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP11 { get; set; }

        [Display("Slave1_Gpio_12 State:", Group: GROUPS1, Order: 4.12, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP12 { get; set; }

        [Display("Slave1_Gpio_13 State:", Group: GROUPS1, Order: 4.13, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP13 { get; set; }

        [Display("Slave1_Gpio_14 State:", Group: GROUPS1, Order: 4.14, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP14 { get; set; }

        [Display("Slave1_Gpio_15 State:", Group: GROUPS1, Order: 4.15, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP15 { get; set; }

        [Display("Slave1_Gpio_16 State:", Group: GROUPS1, Order: 4.16, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP16 { get; set; }

        [Display("Slave1_Gpio_17 State:", Group: GROUPS1, Order: 4.17, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP17 { get; set; }

        [Display("Slave1_Gpio_18 State:", Group: GROUPS1, Order: 4.18, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP18 { get; set; }

        [Display("Slave1_Gpio_19 State:", Group: GROUPS1, Order: 4.19, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP19 { get; set; }

        [Display("Slave1_Gpio_20 State:", Group: GROUPS1, Order: 4.20, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP20 { get; set; }

        [Display("Slave1_Gpio_21 State:", Group: GROUPS1, Order: 4.21, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP21 { get; set; }

        [Display("Slave1_Gpio_22 State:", Group: GROUPS1, Order: 4.22, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP22 { get; set; }

        [Display("Slave1_Gpio_23 State:", Group: GROUPS1, Order: 4.23, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP23 { get; set; }

        [Display("Slave1_Gpio_24 State:", Group: GROUPS1, Order: 4.24, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP24 { get; set; }

        [Display("Slave1_Gpio_25 State:", Group: GROUPS1, Order: 4.25, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP25 { get; set; }

        [Display("Slave1_Gpio_26 State:", Group: GROUPS1, Order: 4.26, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP26 { get; set; }

        [Display("Slave1_Gpio_27 State:", Group: GROUPS1, Order: 4.27, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP27 { get; set; }

        [Display("Slave1_Gpio_28 State:", Group: GROUPS1, Order: 4.28, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S1GP28 { get; set; }

        private const string GROUPS2 = "Slave2, Any GPIO bit Write/Read bit.";

        [Display("Slave2_Gpio_0 State:", Group: GROUPS2, Order: 5.00, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP0 { get; set; }

        [Display("Slave2_Gpio_1 State:", Group: GROUPS2, Order: 5.01, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP1 { get; set; }

        [Display("Slave2_Gpio_2 State:", Group: GROUPS2, Order: 5.02, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP2 { get; set; }

        [Display("Slave2_Gpio_3 State:", Group: GROUPS2, Order: 5.03, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP3 { get; set; }

        [Display("Slave2_Gpio_4 State:", Group: GROUPS2, Order: 5.04, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP4 { get; set; }

        [Display("Slave2_Gpio_5 State:", Group: GROUPS2, Order: 5.05, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP5 { get; set; }

        [Display("Slave2_Gpio_6 State:", Group: GROUPS2, Order: 5.06, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP6 { get; set; }

        [Display("Slave2_Gpio_7 State:", Group: GROUPS2, Order: 5.07, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP7 { get; set; }

        [Display("Slave2_Gpio_8 State:", Group: GROUPS2, Order: 5.08, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP8 { get; set; }

        [Display("Slave2_Gpio_9 State:", Group: GROUPS2, Order: 5.09, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP9 { get; set; }

        [Display("Slave2_Gpio_10 State:", Group: GROUPS2, Order: 5.10, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP10 { get; set; }

        [Display("Slave2_Gpio_11 State:", Group: GROUPS2, Order: 5.11, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP11 { get; set; }

        [Display("Slave2_Gpio_12 State:", Group: GROUPS2, Order: 5.12, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP12 { get; set; }

        [Display("Slave2_Gpio_13 State:", Group: GROUPS2, Order: 5.13, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP13 { get; set; }

        [Display("Slave2_Gpio_14 State:", Group: GROUPS2, Order: 5.14, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP14 { get; set; }

        [Display("Slave2_Gpio_15 State:", Group: GROUPS2, Order: 5.15, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP15 { get; set; }

        [Display("Slave2_Gpio_16 State:", Group: GROUPS2, Order: 5.16, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP16 { get; set; }

        [Display("Slave2_Gpio_17 State:", Group: GROUPS2, Order: 5.17, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP17 { get; set; }

        [Display("Slave2_Gpio_18 State:", Group: GROUPS2, Order: 5.18, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP18 { get; set; }

        [Display("Slave2_Gpio_19 State:", Group: GROUPS2, Order: 5.19, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP19 { get; set; }

        [Display("Slave2_Gpio_20 State:", Group: GROUPS2, Order: 5.20, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP20 { get; set; }

        [Display("Slave2_Gpio_21 State:", Group: GROUPS2, Order: 5.21, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP21 { get; set; }

        [Display("Slave2_Gpio_22 State:", Group: GROUPS2, Order: 5.22, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP22 { get; set; }

        [Display("Slave2_Gpio_23 State:", Group: GROUPS2, Order: 5.23, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP23 { get; set; }

        [Display("Slave2_Gpio_24 State:", Group: GROUPS2, Order: 5.24, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP24 { get; set; }

        [Display("Slave2_Gpio_25 State:", Group: GROUPS2, Order: 5.25, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP25 { get; set; }

        [Display("Slave2_Gpio_26 State:", Group: GROUPS2, Order: 5.26, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP26 { get; set; }

        [Display("Slave2_Gpio_27 State:", Group: GROUPS2, Order: 5.27, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP27 { get; set; }

        [Display("Slave2_Gpio_28 State:", Group: GROUPS2, Order: 5.28, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S2GP28 { get; set; }

        private const string GROUPS3 = "Slave3,Any GPIO bit Write/Read bit.";

        [Display("Slave3_Gpio_0 State:", Group: GROUPS3, Order: 6.00, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP0 { get; set; }

        [Display("Slave3_Gpio_1 State:", Group: GROUPS3, Order: 6.01, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP1 { get; set; }

        [Display("Slave3_Gpio_2 State:", Group: GROUPS3, Order: 6.02, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP2 { get; set; }

        [Display("Slave3_Gpio_3 State:", Group: GROUPS3, Order: 6.03, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP3 { get; set; }

        [Display("Slave3_Gpio_4 State:", Group: GROUPS3, Order: 6.04, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP4 { get; set; }

        [Display("Slave3_Gpio_5 State:", Group: GROUPS3, Order: 6.05, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP5 { get; set; }

        [Display("Slave3_Gpio_6 State:", Group: GROUPS3, Order: 6.06, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP6 { get; set; }

        [Display("Slave3_Gpio_7 State:", Group: GROUPS3, Order: 6.07, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP7 { get; set; }

        [Display("Slave3_Gpio_8 State:", Group: GROUPS3, Order: 6.08, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP8 { get; set; }

        [Display("Slave3_Gpio_9 State:", Group: GROUPS3, Order: 6.09, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP9 { get; set; }

        [Display("Slave3_Gpio_10 State:", Group: GROUPS3, Order: 6.10, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP10 { get; set; }

        [Display("Slave3_Gpio_11 State:", Group: GROUPS3, Order: 6.11, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP11 { get; set; }

        [Display("Slave3_Gpio_12 State:", Group: GROUPS3, Order: 6.12, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP12 { get; set; }

        [Display("Slave3_Gpio_13 State:", Group: GROUPS3, Order: 6.13, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP13 { get; set; }

        [Display("Slave3_Gpio_14 State:", Group: GROUPS3, Order: 6.14, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP14 { get; set; }

        [Display("Slave3_Gpio_15 State:", Group: GROUPS3, Order: 6.15, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP15 { get; set; }

        [Display("Slave3_Gpio_16 State:", Group: GROUPS3, Order: 6.16, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP16 { get; set; }

        [Display("Slave3_Gpio_17 State:", Group: GROUPS3, Order: 6.17, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP17 { get; set; }

        [Display("Slave3_Gpio_18 State:", Group: GROUPS3, Order: 6.18, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP18 { get; set; }

        [Display("Slave3_Gpio_19 State:", Group: GROUPS3, Order: 6.19, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP19 { get; set; }

        [Display("Slave3_Gpio_20 State:", Group: GROUPS3, Order: 6.20, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP20 { get; set; }

        [Display("Slave3_Gpio_21 State:", Group: GROUPS3, Order: 6.21, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP21 { get; set; }

        [Display("Slave3_Gpio_22 State:", Group: GROUPS3, Order: 6.22, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP22 { get; set; }

        [Display("Slave3_Gpio_23 State:", Group: GROUPS3, Order: 6.23, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP23 { get; set; }

        [Display("Slave3_Gpio_24 State:", Group: GROUPS3, Order: 6.24, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP24 { get; set; }

        [Display("Slave3_Gpio_25 State:", Group: GROUPS3, Order: 6.25, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP25 { get; set; }

        [Display("Slave3_Gpio_26 State:", Group: GROUPS3, Order: 6.26, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP26 { get; set; }

        [Display("Slave3_Gpio_27 State:", Group: GROUPS3, Order: 6.27, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP27 { get; set; }

        [Display("Slave3_Gpio_28 State:", Group: GROUPS3, Order: 6.28, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GBit> S3GP28 { get; set; }

        public GpioIO()
        {
            M1IO0 = new Enabled<GBit> { Value = GBit.Low };
            M1IO1 = new Enabled<GBit> { Value = GBit.Low };
            S1IO8 = new Enabled<GBit> { Value = GBit.Low };
            S1IO9 = new Enabled<GBit> { Value = GBit.Low };
            FLAG = new Enabled<GBit> { Value = GBit.Low };
            CTRL = new Enabled<GBit> { Value = GBit.Low };
            
            M0GP0 = new Enabled<GBit> { Value = GBit.Low };M0GP1 = new Enabled<GBit> { Value = GBit.Low };M0GP2 = new Enabled<GBit> { Value = GBit.Low };M0GP3 = new Enabled<GBit> { Value = GBit.Low };
            M0GP4 = new Enabled<GBit> { Value = GBit.Low };M0GP5 = new Enabled<GBit> { Value = GBit.Low };M0GP6 = new Enabled<GBit> { Value = GBit.Low };M0GP7 = new Enabled<GBit> { Value = GBit.Low };
            M0GP8 = new Enabled<GBit> { Value = GBit.Low }; M0GP9 = new Enabled<GBit> { Value = GBit.Low };M0GP10 = new Enabled<GBit> { Value = GBit.Low };M0GP11 = new Enabled<GBit> { Value = GBit.Low };
            M0GP12 = new Enabled<GBit> { Value = GBit.Low };M0GP13 = new Enabled<GBit> { Value = GBit.Low }; M0GP14 = new Enabled<GBit> { Value = GBit.Low };M0GP15 = new Enabled<GBit> { Value = GBit.Low };
            M0GP16 = new Enabled<GBit> { Value = GBit.Low };M0GP17 = new Enabled<GBit> { Value = GBit.Low };M0GP18 = new Enabled<GBit> { Value = GBit.Low };M0GP19 = new Enabled<GBit> { Value = GBit.Low };
            M0GP20 = new Enabled<GBit> { Value = GBit.Low };M0GP21 = new Enabled<GBit> { Value = GBit.Low };M0GP22 = new Enabled<GBit> { Value = GBit.Low };M0GP23 = new Enabled<GBit> { Value = GBit.Low };
            M0GP24 = new Enabled<GBit> { Value = GBit.Low };M0GP25 = new Enabled<GBit> { Value = GBit.Low };M0GP26 = new Enabled<GBit> { Value = GBit.Low };M0GP27 = new Enabled<GBit> { Value = GBit.Low };
            M0GP28 = new Enabled<GBit> { Value = GBit.Low };

            S1GP0 = new Enabled<GBit> { Value = GBit.Low }; S1GP1 = new Enabled<GBit> { Value = GBit.Low }; S1GP2 = new Enabled<GBit> { Value = GBit.Low }; S1GP3 = new Enabled<GBit> { Value = GBit.Low };
            S1GP4 = new Enabled<GBit> { Value = GBit.Low }; S1GP5 = new Enabled<GBit> { Value = GBit.Low }; S1GP6 = new Enabled<GBit> { Value = GBit.Low }; S1GP7 = new Enabled<GBit> { Value = GBit.Low };
            S1GP8 = new Enabled<GBit> { Value = GBit.Low }; S1GP9 = new Enabled<GBit> { Value = GBit.Low }; S1GP10 = new Enabled<GBit> { Value = GBit.Low }; S1GP11 = new Enabled<GBit> { Value = GBit.Low };
            S1GP12 = new Enabled<GBit> { Value = GBit.Low }; S1GP13 = new Enabled<GBit> { Value = GBit.Low }; S1GP14 = new Enabled<GBit> { Value = GBit.Low }; S1GP15 = new Enabled<GBit> { Value = GBit.Low };
            S1GP16 = new Enabled<GBit> { Value = GBit.Low }; S1GP17 = new Enabled<GBit> { Value = GBit.Low }; S1GP18 = new Enabled<GBit> { Value = GBit.Low }; S1GP19 = new Enabled<GBit> { Value = GBit.Low };
            S1GP20 = new Enabled<GBit> { Value = GBit.Low }; S1GP21 = new Enabled<GBit> { Value = GBit.Low }; S1GP22 = new Enabled<GBit> { Value = GBit.Low }; S1GP23 = new Enabled<GBit> { Value = GBit.Low };
            S1GP24 = new Enabled<GBit> { Value = GBit.Low }; S1GP25 = new Enabled<GBit> { Value = GBit.Low }; S1GP26 = new Enabled<GBit> { Value = GBit.Low }; S1GP27 = new Enabled<GBit> { Value = GBit.Low };
            S1GP28 = new Enabled<GBit> { Value = GBit.Low };

            S2GP0 = new Enabled<GBit> { Value = GBit.Low }; S2GP1 = new Enabled<GBit> { Value = GBit.Low }; S2GP2 = new Enabled<GBit> { Value = GBit.Low }; S2GP3 = new Enabled<GBit> { Value = GBit.Low };
            S2GP4 = new Enabled<GBit> { Value = GBit.Low }; S2GP5 = new Enabled<GBit> { Value = GBit.Low }; S2GP6 = new Enabled<GBit> { Value = GBit.Low }; S2GP7 = new Enabled<GBit> { Value = GBit.Low };
            S2GP8 = new Enabled<GBit> { Value = GBit.Low }; S2GP9 = new Enabled<GBit> { Value = GBit.Low }; S2GP10 = new Enabled<GBit> { Value = GBit.Low }; S2GP11 = new Enabled<GBit> { Value = GBit.Low };
            S2GP12 = new Enabled<GBit> { Value = GBit.Low }; S2GP13 = new Enabled<GBit> { Value = GBit.Low }; S2GP14 = new Enabled<GBit> { Value = GBit.Low }; S2GP15 = new Enabled<GBit> { Value = GBit.Low };
            S2GP16 = new Enabled<GBit> { Value = GBit.Low }; S2GP17 = new Enabled<GBit> { Value = GBit.Low }; S2GP18 = new Enabled<GBit> { Value = GBit.Low }; S2GP19 = new Enabled<GBit> { Value = GBit.Low };
            S2GP20 = new Enabled<GBit> { Value = GBit.Low }; S2GP21 = new Enabled<GBit> { Value = GBit.Low }; S2GP22 = new Enabled<GBit> { Value = GBit.Low }; S2GP23 = new Enabled<GBit> { Value = GBit.Low };
            S2GP24 = new Enabled<GBit> { Value = GBit.Low }; S2GP25 = new Enabled<GBit> { Value = GBit.Low }; S2GP26 = new Enabled<GBit> { Value = GBit.Low }; S2GP27 = new Enabled<GBit> { Value = GBit.Low };
            S2GP28 = new Enabled<GBit> { Value = GBit.Low };

            S3GP0 = new Enabled<GBit> { Value = GBit.Low }; S3GP1 = new Enabled<GBit> { Value = GBit.Low }; S3GP2 = new Enabled<GBit> { Value = GBit.Low }; S3GP3 = new Enabled<GBit> { Value = GBit.Low };
            S3GP4 = new Enabled<GBit> { Value = GBit.Low }; S3GP5 = new Enabled<GBit> { Value = GBit.Low }; S3GP6 = new Enabled<GBit> { Value = GBit.Low }; S3GP7 = new Enabled<GBit> { Value = GBit.Low };
            S3GP8 = new Enabled<GBit> { Value = GBit.Low }; S3GP9 = new Enabled<GBit> { Value = GBit.Low }; S3GP10 = new Enabled<GBit> { Value = GBit.Low }; S3GP11 = new Enabled<GBit> { Value = GBit.Low };
            S3GP12 = new Enabled<GBit> { Value = GBit.Low }; S3GP13 = new Enabled<GBit> { Value = GBit.Low }; S3GP14 = new Enabled<GBit> { Value = GBit.Low }; S3GP15 = new Enabled<GBit> { Value = GBit.Low };
            S3GP16 = new Enabled<GBit> { Value = GBit.Low }; S3GP17 = new Enabled<GBit> { Value = GBit.Low }; S3GP18 = new Enabled<GBit> { Value = GBit.Low }; S3GP19 = new Enabled<GBit> { Value = GBit.Low };
            S3GP20 = new Enabled<GBit> { Value = GBit.Low }; S3GP21 = new Enabled<GBit> { Value = GBit.Low }; S3GP22 = new Enabled<GBit> { Value = GBit.Low }; S3GP23 = new Enabled<GBit> { Value = GBit.Low };
            S3GP24 = new Enabled<GBit> { Value = GBit.Low }; S3GP25 = new Enabled<GBit> { Value = GBit.Low }; S3GP26 = new Enabled<GBit> { Value = GBit.Low }; S3GP27 = new Enabled<GBit> { Value = GBit.Low };
            S3GP28 = new Enabled<GBit> { Value = GBit.Low };

        }
      

        public override void PrePlanRun()
        {
            base.PrePlanRun();
        }



        public override void Run()
        {
       
            if ((M1IO0.IsEnabled == true) || (M1IO1.IsEnabled == true) || (S1IO8.IsEnabled == true) || (S1IO9.IsEnabled == true) || (FLAG.IsEnabled == true) || (CTRL.IsEnabled == true))
            {
                ConfigureDefGpio(SelectedgAct);
            }

            if ((M0GP0.IsEnabled == true) || (M0GP1.IsEnabled == true) || (M0GP2.IsEnabled == true) || (M0GP3.IsEnabled == true) || (M0GP4.IsEnabled == true) || (M0GP5.IsEnabled == true) || (M0GP6.IsEnabled == true) || (M0GP7.IsEnabled == true) || (M0GP8.IsEnabled == true) || (M0GP9.IsEnabled == true) || (M0GP10.IsEnabled == true) || (M0GP11.IsEnabled == true) || (M0GP12.IsEnabled == true) || (M0GP13.IsEnabled == true) || (M0GP14.IsEnabled == true) || (M0GP15.IsEnabled == true) || (M0GP16.IsEnabled == true) || (M0GP17.IsEnabled == true) || (M0GP18.IsEnabled == true) || (M0GP19.IsEnabled == true) || (M0GP20.IsEnabled == true) || (M0GP21.IsEnabled == true) || (M0GP22.IsEnabled == true) || (M0GP23.IsEnabled == true) || (M0GP24.IsEnabled == true) || (M0GP25.IsEnabled == true) || (M0GP26.IsEnabled == true) || (M0GP27.IsEnabled == true) || (M0GP28.IsEnabled == true))
            {
                ConfigureAllGpio(0,SelectedgAct);
            }

            if ((S1GP0.IsEnabled == true) || (S1GP1.IsEnabled == true) || (S1GP2.IsEnabled == true) || (S1GP3.IsEnabled == true) || (S1GP4.IsEnabled == true) || (S1GP5.IsEnabled == true) || (S1GP6.IsEnabled == true) || (S1GP7.IsEnabled == true) || (S1GP8.IsEnabled == true) || (S1GP9.IsEnabled == true) || (S1GP10.IsEnabled == true) || (S1GP11.IsEnabled == true) || (S1GP12.IsEnabled == true) || (S1GP13.IsEnabled == true) || (S1GP14.IsEnabled == true) || (S1GP15.IsEnabled == true) || (S1GP16.IsEnabled == true) || (S1GP17.IsEnabled == true) || (S1GP18.IsEnabled == true) || (S1GP19.IsEnabled == true) || (S1GP20.IsEnabled == true) || (S1GP21.IsEnabled == true) || (S1GP22.IsEnabled == true) || (S1GP23.IsEnabled == true) || (S1GP24.IsEnabled == true) || (S1GP25.IsEnabled == true) || (S1GP26.IsEnabled == true) || (S1GP27.IsEnabled == true) || (S1GP28.IsEnabled == true))
            {
                ConfigureAllGpio(1, SelectedgAct);
            }

            if ((S2GP0.IsEnabled == true) || (S2GP1.IsEnabled == true) || (S2GP2.IsEnabled == true) || (S2GP3.IsEnabled == true) || (S2GP4.IsEnabled == true) || (S2GP5.IsEnabled == true) || (S2GP6.IsEnabled == true) || (S2GP7.IsEnabled == true) || (S2GP8.IsEnabled == true) || (S2GP9.IsEnabled == true) || (S2GP10.IsEnabled == true) || (S2GP11.IsEnabled == true) || (S2GP12.IsEnabled == true) || (S2GP13.IsEnabled == true) || (S2GP14.IsEnabled == true) || (S2GP15.IsEnabled == true) || (S2GP16.IsEnabled == true) || (S2GP17.IsEnabled == true) || (S2GP18.IsEnabled == true) || (S2GP19.IsEnabled == true) || (S2GP20.IsEnabled == true) || (S2GP21.IsEnabled == true) || (S2GP22.IsEnabled == true) || (S2GP23.IsEnabled == true) || (S2GP24.IsEnabled == true) || (S2GP25.IsEnabled == true) || (S2GP26.IsEnabled == true) || (S2GP27.IsEnabled == true) || (S2GP28.IsEnabled == true))
            {
                ConfigureAllGpio(2, SelectedgAct);
            }

            if ((S3GP0.IsEnabled == true) || (S3GP1.IsEnabled == true) || (S3GP2.IsEnabled == true) || (S3GP3.IsEnabled == true) || (S3GP4.IsEnabled == true) || (S3GP5.IsEnabled == true) || (S3GP6.IsEnabled == true) || (S3GP7.IsEnabled == true) || (S3GP8.IsEnabled == true) || (S3GP9.IsEnabled == true) || (S3GP10.IsEnabled == true) || (S3GP11.IsEnabled == true) || (S3GP12.IsEnabled == true) || (S3GP13.IsEnabled == true) || (S3GP14.IsEnabled == true) || (S3GP15.IsEnabled == true) || (S3GP16.IsEnabled == true) || (S3GP17.IsEnabled == true) || (S3GP18.IsEnabled == true) || (S3GP19.IsEnabled == true) || (S3GP20.IsEnabled == true) || (S3GP21.IsEnabled == true) || (S3GP22.IsEnabled == true) || (S3GP23.IsEnabled == true) || (S3GP24.IsEnabled == true) || (S3GP25.IsEnabled == true) || (S3GP26.IsEnabled == true) || (S3GP27.IsEnabled == true) || (S3GP28.IsEnabled == true))
            {
                ConfigureAllGpio(3, SelectedgAct);
            }
        }


        /// <summary>
        /// Write read on any device any Gpio between 0 and 28.
        /// </summary>
        /// <param name="device"> Device number (0 for Master, 1 for Slave1, 2 for Slave2 and 3 for Slave3).</param>
        /// <param name="portValue">The byte value for configuration.</param>
        /// <param name="selectedFunction">Selected function for the Gpio.</param>
        private void ConfigureAllGpio(int device, GpIO selectedFunction)
        {
            byte bitValue;
            string name;

            for (int bit = 0; bit < 29; bit++)
            {

                // Skip bits that are not enabled
                if (!IsBitEnabled(device, bit)) continue;

                (bitValue, name) = GetPortBitValue(device, bit);
                bool bitState = (bitValue == 1);
                string bitCommand = bitState ? "1" : "0";

                // Write Command
                if ((selectedFunction == GpIO.Write_read) || (selectedFunction == GpIO.Write_only))
                {
                    string Command = $"GPIO:OUT:DEVICE{device}:GP{bit} {bitCommand}";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read Command
                if ((selectedFunction == GpIO.read_only) || (selectedFunction == GpIO.Write_read) || (selectedFunction == GpIO.read_test))
                {
                    string Command = $"GPIO:IN:DEVICE{device}:GP{bit}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

                    double.TryParse(response, out double value);
                    string test = (value == Convert.ToDouble(bitState)) ? "PASS" : "FAIL";


                    if (selectedFunction == GpIO.read_only)
                    {
                        test = "PASS"; // No verdict on read only
                    }

                    // Create test result object
                    TestResult<double> result = new TestResult<double>
                    {
                        ParamName = $"{name} Bit{bit} Data",
                        StepName = Name,
                        Value = value,
                        Verdict = test,
                        Units = "read"
                    };

                    if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                    else UpgradeVerdict(Verdict.Fail);


                    // Add limits for Write_Read function
                    if ((selectedFunction == GpIO.Write_read) || (selectedFunction == GpIO.read_test))
                    {
                        result.LowerLimit = bitValue;
                        result.UpperLimit = bitValue;
                        result.Units = "digcmp";
                    }

                    PublishResult(result);
                }
            }
        }



        // <summary>
        /// Write or Read bit on Dedicted GPIO

        /// <param name="selectedFunction">Selected function for the port.</param>
        /// 
        public void ConfigureDefGpio(GpIO selectedFunction)
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

                var bitConfig = property?.GetValue(this) as Enabled<GBit>;

                if (bitConfig == null || !bitConfig.IsEnabled)
                    continue;
                byte bitgpio = (byte)(bitConfig.Value == GBit.High ? 1 : 0);

                // Write Command
                if ((selectedFunction == GpIO.Write_read) || (selectedFunction == GpIO.Write_only))
                {
                    string Command = $"GPIO:OUT:DEVICE{device}:GP{pin} {bitgpio}";
                    Log.Info($"Sending SCPI command for {name}: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read Command
                if ((selectedFunction == GpIO.read_only) || (selectedFunction == GpIO.Write_read) || (selectedFunction == GpIO.read_test))
                {
                    string Command = $"GPIO:IN:DEVICE{device}:GP{pin}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"Sending SCPI command for {name}: {Command}, Answer: {response}");


                    double.TryParse(response, out double value);
                    test = (value == Convert.ToDouble(bitgpio)) ? "PASS" : "FAIL";


                    if (selectedFunction == GpIO.read_only)
                    {
                        test = "PASS"; // No verdict on read only
                    } else
                    {
                        // Test the answer
                        if (value != bitgpio)
                        {
                            Log.Warning($"Invalid SCPI response: {response}, expected: {bitgpio}");
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
                        ParamName = $"{name} Bit{pin} Data",
                        StepName = Name,
                        Value = value,
                        Verdict = test,
                        Units = "read"
                    };

                    // Add limits for Write_Read function
                    if (selectedFunction == GpIO.Write_read || selectedFunction == GpIO.read_test )
                    {
                        result.LowerLimit = bitgpio;
                        result.UpperLimit = bitgpio;
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
                var bitConfig = property.GetValue(this) as Enabled<GBit>;
                if (bitConfig?.IsEnabled == true)
                {
                    byte value = (byte)(bitConfig.Value == GBit.High ? 1 : 0);
                    return (value, propertyName);  // Return both bit value and property name
                }
            }
            return (0, propertyName); // Default to bit = 0 with property name
        }

        private bool IsBitEnabled(int device, int bit)
        {
            string dev = (device == 0) ? "M0": $"S{device}";
            string propertyName = $"{dev}GP{bit}";
            var property = GetType().GetProperty(propertyName);

            return property?.GetValue(this) is Enabled<GBit> bitConfig && bitConfig.IsEnabled;
        }
        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}
