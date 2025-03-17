
using OpenTap;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using static InterconnectIOBox.Portscfg;
using static InterconnectIOBox.PortsIO;

namespace InterconnectIOBox
{
        [Display(Groups: new[] { "InterconnectIO", "GPIO" }, Name: "GPIO Direction Write/Read", Description: "Configure direction of GPIO as Input or Output. Direction could be set for the designated GPIO or for any GPIO on any Pico device " +
        "Enabling GPIO pin direction is done by checking the enable checkbox and selecting a value(0 for Input, 1 for Output). The read function operates similarly—enable the byte or bit" +
        " to verify and set the value.The read result is then published. Be Careful using the Any Gpio control, it is an advanced mode and could cause malfunction")]

    public class Gpiocfg : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        public InterconnectIO IO_Instrument { get; set; }
        public enum GpFct
        {
            Write_read,
            Write_only,
            read_only,
        }


        [Display("Configuration Action:", Group: "Cfg Control",Order: 0.1, Description: "Action to perform on selected Gpio, read data will be published.")]
        public GpFct Selectedgfct { get; set; }

        public enum GDir
        {
            Input,
            Output,
        }

        [Display("M1_IO0 Direction:", Group: "Dedicated GPIO Direction Read/Write", Order: 2, Collapsed: true, Description: "Write/Read GPIO direction by selecting.(0: Input, 1: Output)")]
        public Enabled<GDir> M1IO0 { get; set; }

        [Display("M1_IO1 Direction:", Group: "Dedicated GPIO Direction Read/Write", Order: 2.1, Collapsed: true, Description: "Write/Read GPIO direction by selecting.(0: Input, 1: Output)")]
        public Enabled<GDir> M1IO1 { get; set; }

        [Display("S1_IO8 Direction:", Group: "Dedicated GPIO Direction Read/Write", Order: 2.2, Collapsed: true, Description: "Write/Read GPIO direction by selecting.(0: Input, 1: Output)")]
        public Enabled<GDir> S1IO8 { get; set; }

        [Display("S1_IO9 Direction:", Group: "Dedicated GPIO Direction Read/Write", Order: 2.3, Collapsed: true, Description: "Write/Read GPIO direction by selecting.(0: Input, 1: Output)")]
        public Enabled<GDir> S1IO9 { get; set; }

        [Display("FLAG Direction:", Group: "Dedicated GPIO Direction Read/Write", Order: 2.4, Collapsed: true, Description: "Write/Read GPIO direction by selecting.(0: Input, 1: Output)")]
        public Enabled<GDir> FLAG { get; set; }

        [Display("CTRL Direction:", Group: "Dedicated GPIO Direction Read/Write", Order: 2.5, Collapsed: true, Description: "Write/Read GPIO direction by selecting.(0: Input, 1: Output)")]
        public Enabled<GDir> CTRL { get; set; }

        private const string GROUPM = "Master0, Any GPIO Direction Read/Write.";
        private const string GPIO_DESC = "Write/Read on any GPIO direction by selecting.(0: Input, 1: Output)";

        [Display("Master_Gpio_0 Direction:", Group : GROUPM, Order : 3.00, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP0 { get; set; }

        [Display("Master_Gpio_1 Direction:", Group : GROUPM, Order : 3.01, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP1 { get; set; }

        [Display("Master_Gpio_2 Direction:", Group : GROUPM, Order : 3.02, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP2 { get; set; }

        [Display("Master_Gpio_3 Direction:", Group : GROUPM, Order : 3.03, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP3 { get; set; }

        [Display("Master_Gpio_4 Direction:", Group : GROUPM, Order : 3.04, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP4 { get; set; }

        [Display("Master_Gpio_5 Direction:", Group : GROUPM, Order : 3.05, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP5 { get; set; }

        [Display("Master_Gpio_6 Direction:", Group : GROUPM, Order : 3.06, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP6 { get; set; }

        [Display("Master_Gpio_7 Direction:", Group : GROUPM, Order : 3.07, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP7 { get; set; }

        [Display("Master_Gpio_8 Direction:", Group : GROUPM, Order : 3.08, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP8 { get; set; }

        [Display("Master_Gpio_9 Direction:", Group : GROUPM, Order : 3.09, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP9 { get; set; }

        [Display("Master_Gpio_10 Direction:", Group : GROUPM, Order : 3.10, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP10 { get; set; }

        [Display("Master_Gpio_11 Direction:", Group : GROUPM, Order : 3.11, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP11 { get; set; }

        [Display("Master_Gpio_12 Direction:", Group : GROUPM, Order : 3.12, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP12 { get; set; }

        [Display("Master_Gpio_13 Direction:", Group : GROUPM, Order : 3.13, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP13 { get; set; }

        [Display("Master_Gpio_14 Direction:", Group : GROUPM, Order : 3.14, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP14 { get; set; }

        [Display("Master_Gpio_15 Direction:", Group : GROUPM, Order : 3.15, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP15 { get; set; }

        [Display("Master_Gpio_16 Direction:", Group : GROUPM, Order : 3.16, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP16 { get; set; }

        [Display("Master_Gpio_17 Direction:", Group : GROUPM, Order : 3.17, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP17 { get; set; }

        [Display("Master_Gpio_18 Direction:", Group : GROUPM, Order : 3.18, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP18 { get; set; }

        [Display("Master_Gpio_19 Direction:", Group : GROUPM, Order : 3.19, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP19 { get; set; }

        [Display("Master_Gpio_20 Direction:", Group : GROUPM, Order : 3.20, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP20 { get; set; }

        [Display("Master_Gpio_21 Direction:", Group : GROUPM, Order : 3.21, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP21 { get; set; }

        [Display("Master_Gpio_22 Direction:", Group : GROUPM, Order : 3.22, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP22 { get; set; }

        [Display("Master_Gpio_23 Direction:", Group : GROUPM, Order : 3.23, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP23 { get; set; }

        [Display("Master_Gpio_24 Direction:", Group : GROUPM, Order : 3.24, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP24 { get; set; }

        [Display("Master_Gpio_25 Direction:", Group : GROUPM, Order : 3.25, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP25 { get; set; }

        [Display("Master_Gpio_26 Direction:", Group : GROUPM, Order : 3.26, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP26 { get; set; }

        [Display("Master_Gpio_27 Direction:", Group : GROUPM, Order : 3.27, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP27 { get; set; }

        [Display("Master_Gpio_28 Direction:", Group : GROUPM, Order : 3.28, Collapsed : true, Description : GPIO_DESC)]
        public Enabled<GDir> M0GP28 { get; set; }

        private const string GROUPS1 = "Slave1, Any GPIO Direction Read/Write.";
    
        [Display("Slave1_Gpio_0 Direction:", Group: GROUPS1, Order: 4.00, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP0 { get; set; }

        [Display("Slave1_Gpio_1 Direction:", Group: GROUPS1, Order: 4.01, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP1 { get; set; }

        [Display("Slave1_Gpio_2 Direction:", Group: GROUPS1, Order: 4.02, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP2 { get; set; }

        [Display("Slave1_Gpio_3 Direction:", Group: GROUPS1, Order: 4.03, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP3 { get; set; }

        [Display("Slave1_Gpio_4 Direction:", Group: GROUPS1, Order: 4.04, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP4 { get; set; }

        [Display("Slave1_Gpio_5 Direction:", Group: GROUPS1, Order: 4.05, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP5 { get; set; }

        [Display("Slave1_Gpio_6 Direction:", Group: GROUPS1, Order: 4.06, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP6 { get; set; }

        [Display("Slave1_Gpio_7 Direction:", Group: GROUPS1, Order: 4.07, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP7 { get; set; }

        [Display("Slave1_Gpio_8 Direction:", Group: GROUPS1, Order: 4.08, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP8 { get; set; }

        [Display("Slave1_Gpio_9 Direction:", Group: GROUPS1, Order: 4.09, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP9 { get; set; }

        [Display("Slave1_Gpio_10 Direction:", Group: GROUPS1, Order: 4.10, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP10 { get; set; }

        [Display("Slave1_Gpio_11 Direction:", Group: GROUPS1, Order: 4.11, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP11 { get; set; }

        [Display("Slave1_Gpio_12 Direction:", Group: GROUPS1, Order: 4.12, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP12 { get; set; }

        [Display("Slave1_Gpio_13 Direction:", Group: GROUPS1, Order: 4.13, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP13 { get; set; }

        [Display("Slave1_Gpio_14 Direction:", Group: GROUPS1, Order: 4.14, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP14 { get; set; }

        [Display("Slave1_Gpio_15 Direction:", Group: GROUPS1, Order: 4.15, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP15 { get; set; }

        [Display("Slave1_Gpio_16 Direction:", Group: GROUPS1, Order: 4.16, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP16 { get; set; }

        [Display("Slave1_Gpio_17 Direction:", Group: GROUPS1, Order: 4.17, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP17 { get; set; }

        [Display("Slave1_Gpio_18 Direction:", Group: GROUPS1, Order: 4.18, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP18 { get; set; }

        [Display("Slave1_Gpio_19 Direction:", Group: GROUPS1, Order: 4.19, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP19 { get; set; }

        [Display("Slave1_Gpio_20 Direction:", Group: GROUPS1, Order: 4.20, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP20 { get; set; }

        [Display("Slave1_Gpio_21 Direction:", Group: GROUPS1, Order: 4.21, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP21 { get; set; }

        [Display("Slave1_Gpio_22 Direction:", Group: GROUPS1, Order: 4.22, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP22 { get; set; }

        [Display("Slave1_Gpio_23 Direction:", Group: GROUPS1, Order: 4.23, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP23 { get; set; }

        [Display("Slave1_Gpio_24 Direction:", Group: GROUPS1, Order: 4.24, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP24 { get; set; }

        [Display("Slave1_Gpio_25 Direction:", Group: GROUPS1, Order: 4.25, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP25 { get; set; }

        [Display("Slave1_Gpio_26 Direction:", Group: GROUPS1, Order: 4.26, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP26 { get; set; }

        [Display("Slave1_Gpio_27 Direction:", Group: GROUPS1, Order: 4.27, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP27 { get; set; }

        [Display("Slave1_Gpio_28 Direction:", Group: GROUPS1, Order: 4.28, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S1GP28 { get; set; }

        private const string GROUPS2 = "Slave2, Any GPIO Direction Read/Write.";

        [Display("Slave2_Gpio_0 Direction:", Group: GROUPS2, Order: 5.00, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP0 { get; set; }

        [Display("Slave2_Gpio_1 Direction:", Group: GROUPS2, Order: 5.01, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP1 { get; set; }

        [Display("Slave2_Gpio_2 Direction:", Group: GROUPS2, Order: 5.02, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP2 { get; set; }

        [Display("Slave2_Gpio_3 Direction:", Group: GROUPS2, Order: 5.03, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP3 { get; set; }

        [Display("Slave2_Gpio_4 Direction:", Group: GROUPS2, Order: 5.04, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP4 { get; set; }

        [Display("Slave2_Gpio_5 Direction:", Group: GROUPS2, Order: 5.05, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP5 { get; set; }

        [Display("Slave2_Gpio_6 Direction:", Group: GROUPS2, Order: 5.06, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP6 { get; set; }

        [Display("Slave2_Gpio_7 Direction:", Group: GROUPS2, Order: 5.07, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP7 { get; set; }

        [Display("Slave2_Gpio_8 Direction:", Group: GROUPS2, Order: 5.08, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP8 { get; set; }

        [Display("Slave2_Gpio_9 Direction:", Group: GROUPS2, Order: 5.09, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP9 { get; set; }

        [Display("Slave2_Gpio_10 Direction:", Group: GROUPS2, Order: 5.10, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP10 { get; set; }

        [Display("Slave2_Gpio_11 Direction:", Group: GROUPS2, Order: 5.11, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP11 { get; set; }

        [Display("Slave2_Gpio_12 Direction:", Group: GROUPS2, Order: 5.12, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP12 { get; set; }

        [Display("Slave2_Gpio_13 Direction:", Group: GROUPS2, Order: 5.13, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP13 { get; set; }

        [Display("Slave2_Gpio_14 Direction:", Group: GROUPS2, Order: 5.14, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP14 { get; set; }

        [Display("Slave2_Gpio_15 Direction:", Group: GROUPS2, Order: 5.15, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP15 { get; set; }

        [Display("Slave2_Gpio_16 Direction:", Group: GROUPS2, Order: 5.16, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP16 { get; set; }

        [Display("Slave2_Gpio_17 Direction:", Group: GROUPS2, Order: 5.17, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP17 { get; set; }

        [Display("Slave2_Gpio_18 Direction:", Group: GROUPS2, Order: 5.18, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP18 { get; set; }

        [Display("Slave2_Gpio_19 Direction:", Group: GROUPS2, Order: 5.19, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP19 { get; set; }

        [Display("Slave2_Gpio_20 Direction:", Group: GROUPS2, Order: 5.20, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP20 { get; set; }

        [Display("Slave2_Gpio_21 Direction:", Group: GROUPS2, Order: 5.21, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP21 { get; set; }

        [Display("Slave2_Gpio_22 Direction:", Group: GROUPS2, Order: 5.22, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP22 { get; set; }

        [Display("Slave2_Gpio_23 Direction:", Group: GROUPS2, Order: 5.23, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP23 { get; set; }

        [Display("Slave2_Gpio_24 Direction:", Group: GROUPS2, Order: 5.24, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP24 { get; set; }

        [Display("Slave2_Gpio_25 Direction:", Group: GROUPS2, Order: 5.25, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP25 { get; set; }

        [Display("Slave2_Gpio_26 Direction:", Group: GROUPS2, Order: 5.26, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP26 { get; set; }

        [Display("Slave2_Gpio_27 Direction:", Group: GROUPS2, Order: 5.27, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP27 { get; set; }

        [Display("Slave2_Gpio_28 Direction:", Group: GROUPS2, Order: 5.28, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S2GP28 { get; set; }

        private const string GROUPS3 = "Slave3, Any GPIO Direction Read/Write.";

        [Display("Slave3_Gpio_0 Direction:", Group: GROUPS3, Order: 6.00, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP0 { get; set; }

        [Display("Slave3_Gpio_1 Direction:", Group: GROUPS3, Order: 6.01, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP1 { get; set; }

        [Display("Slave3_Gpio_2 Direction:", Group: GROUPS3, Order: 6.02, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP2 { get; set; }

        [Display("Slave3_Gpio_3 Direction:", Group: GROUPS3, Order: 6.03, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP3 { get; set; }

        [Display("Slave3_Gpio_4 Direction:", Group: GROUPS3, Order: 6.04, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP4 { get; set; }

        [Display("Slave3_Gpio_5 Direction:", Group: GROUPS3, Order: 6.05, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP5 { get; set; }

        [Display("Slave3_Gpio_6 Direction:", Group: GROUPS3, Order: 6.06, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP6 { get; set; }

        [Display("Slave3_Gpio_7 Direction:", Group: GROUPS3, Order: 6.07, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP7 { get; set; }

        [Display("Slave3_Gpio_8 Direction:", Group: GROUPS3, Order: 6.08, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP8 { get; set; }

        [Display("Slave3_Gpio_9 Direction:", Group: GROUPS3, Order: 6.09, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP9 { get; set; }

        [Display("Slave3_Gpio_10 Direction:", Group: GROUPS3, Order: 6.10, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP10 { get; set; }

        [Display("Slave3_Gpio_11 Direction:", Group: GROUPS3, Order: 6.11, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP11 { get; set; }

        [Display("Slave3_Gpio_12 Direction:", Group: GROUPS3, Order: 6.12, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP12 { get; set; }

        [Display("Slave3_Gpio_13 Direction:", Group: GROUPS3, Order: 6.13, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP13 { get; set; }

        [Display("Slave3_Gpio_14 Direction:", Group: GROUPS3, Order: 6.14, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP14 { get; set; }

        [Display("Slave3_Gpio_15 Direction:", Group: GROUPS3, Order: 6.15, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP15 { get; set; }

        [Display("Slave3_Gpio_16 Direction:", Group: GROUPS3, Order: 6.16, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP16 { get; set; }

        [Display("Slave3_Gpio_17 Direction:", Group: GROUPS3, Order: 6.17, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP17 { get; set; }

        [Display("Slave3_Gpio_18 Direction:", Group: GROUPS3, Order: 6.18, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP18 { get; set; }

        [Display("Slave3_Gpio_19 Direction:", Group: GROUPS3, Order: 6.19, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP19 { get; set; }

        [Display("Slave3_Gpio_20 Direction:", Group: GROUPS3, Order: 6.20, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP20 { get; set; }

        [Display("Slave3_Gpio_21 Direction:", Group: GROUPS3, Order: 6.21, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP21 { get; set; }

        [Display("Slave3_Gpio_22 Direction:", Group: GROUPS3, Order: 6.22, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP22 { get; set; }

        [Display("Slave3_Gpio_23 Direction:", Group: GROUPS3, Order: 6.23, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP23 { get; set; }

        [Display("Slave3_Gpio_24 Direction:", Group: GROUPS3, Order: 6.24, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP24 { get; set; }

        [Display("Slave3_Gpio_25 Direction:", Group: GROUPS3, Order: 6.25, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP25 { get; set; }

        [Display("Slave3_Gpio_26 Direction:", Group: GROUPS3, Order: 6.26, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP26 { get; set; }

        [Display("Slave3_Gpio_27 Direction:", Group: GROUPS3, Order: 6.27, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP27 { get; set; }

        [Display("Slave3_Gpio_28 Direction:", Group: GROUPS3, Order: 6.28, Collapsed: true, Description: GPIO_DESC)]
        public Enabled<GDir> S3GP28 { get; set; }

        public Gpiocfg()
        {
            M1IO0 = new Enabled<GDir> { Value = GDir.Input };
            M1IO1 = new Enabled<GDir> { Value = GDir.Input };
            S1IO8 = new Enabled<GDir> { Value = GDir.Input };
            S1IO9 = new Enabled<GDir> { Value = GDir.Input };
            FLAG = new Enabled<GDir> { Value = GDir.Input };
            CTRL = new Enabled<GDir> { Value = GDir.Input };
            
            M0GP0 = new Enabled<GDir> { Value = GDir.Input };M0GP1 = new Enabled<GDir> { Value = GDir.Input };M0GP2 = new Enabled<GDir> { Value = GDir.Input };M0GP3 = new Enabled<GDir> { Value = GDir.Input };
            M0GP4 = new Enabled<GDir> { Value = GDir.Input };M0GP5 = new Enabled<GDir> { Value = GDir.Input };M0GP6 = new Enabled<GDir> { Value = GDir.Input };M0GP7 = new Enabled<GDir> { Value = GDir.Input };
            M0GP8 = new Enabled<GDir> { Value = GDir.Input }; M0GP9 = new Enabled<GDir> { Value = GDir.Input };M0GP10 = new Enabled<GDir> { Value = GDir.Input };M0GP11 = new Enabled<GDir> { Value = GDir.Input };
            M0GP12 = new Enabled<GDir> { Value = GDir.Input };M0GP13 = new Enabled<GDir> { Value = GDir.Input }; M0GP14 = new Enabled<GDir> { Value = GDir.Input };M0GP15 = new Enabled<GDir> { Value = GDir.Input };
            M0GP16 = new Enabled<GDir> { Value = GDir.Input };M0GP17 = new Enabled<GDir> { Value = GDir.Input };M0GP18 = new Enabled<GDir> { Value = GDir.Input };M0GP19 = new Enabled<GDir> { Value = GDir.Input };
            M0GP20 = new Enabled<GDir> { Value = GDir.Input };M0GP21 = new Enabled<GDir> { Value = GDir.Input };M0GP22 = new Enabled<GDir> { Value = GDir.Input };M0GP23 = new Enabled<GDir> { Value = GDir.Input };
            M0GP24 = new Enabled<GDir> { Value = GDir.Input };M0GP25 = new Enabled<GDir> { Value = GDir.Input };M0GP26 = new Enabled<GDir> { Value = GDir.Input };M0GP27 = new Enabled<GDir> { Value = GDir.Input };
            M0GP28 = new Enabled<GDir> { Value = GDir.Input };

            S1GP0 = new Enabled<GDir> { Value = GDir.Input }; S1GP1 = new Enabled<GDir> { Value = GDir.Input }; S1GP2 = new Enabled<GDir> { Value = GDir.Input }; S1GP3 = new Enabled<GDir> { Value = GDir.Input };
            S1GP4 = new Enabled<GDir> { Value = GDir.Input }; S1GP5 = new Enabled<GDir> { Value = GDir.Input }; S1GP6 = new Enabled<GDir> { Value = GDir.Input }; S1GP7 = new Enabled<GDir> { Value = GDir.Input };
            S1GP8 = new Enabled<GDir> { Value = GDir.Input }; S1GP9 = new Enabled<GDir> { Value = GDir.Input }; S1GP10 = new Enabled<GDir> { Value = GDir.Input }; S1GP11 = new Enabled<GDir> { Value = GDir.Input };
            S1GP12 = new Enabled<GDir> { Value = GDir.Input }; S1GP13 = new Enabled<GDir> { Value = GDir.Input }; S1GP14 = new Enabled<GDir> { Value = GDir.Input }; S1GP15 = new Enabled<GDir> { Value = GDir.Input };
            S1GP16 = new Enabled<GDir> { Value = GDir.Input }; S1GP17 = new Enabled<GDir> { Value = GDir.Input }; S1GP18 = new Enabled<GDir> { Value = GDir.Input }; S1GP19 = new Enabled<GDir> { Value = GDir.Input };
            S1GP20 = new Enabled<GDir> { Value = GDir.Input }; S1GP21 = new Enabled<GDir> { Value = GDir.Input }; S1GP22 = new Enabled<GDir> { Value = GDir.Input }; S1GP23 = new Enabled<GDir> { Value = GDir.Input };
            S1GP24 = new Enabled<GDir> { Value = GDir.Input }; S1GP25 = new Enabled<GDir> { Value = GDir.Input }; S1GP26 = new Enabled<GDir> { Value = GDir.Input }; S1GP27 = new Enabled<GDir> { Value = GDir.Input };
            S1GP28 = new Enabled<GDir> { Value = GDir.Input };

            S2GP0 = new Enabled<GDir> { Value = GDir.Input }; S2GP1 = new Enabled<GDir> { Value = GDir.Input }; S2GP2 = new Enabled<GDir> { Value = GDir.Input }; S2GP3 = new Enabled<GDir> { Value = GDir.Input };
            S2GP4 = new Enabled<GDir> { Value = GDir.Input }; S2GP5 = new Enabled<GDir> { Value = GDir.Input }; S2GP6 = new Enabled<GDir> { Value = GDir.Input }; S2GP7 = new Enabled<GDir> { Value = GDir.Input };
            S2GP8 = new Enabled<GDir> { Value = GDir.Input }; S2GP9 = new Enabled<GDir> { Value = GDir.Input }; S2GP10 = new Enabled<GDir> { Value = GDir.Input }; S2GP11 = new Enabled<GDir> { Value = GDir.Input };
            S2GP12 = new Enabled<GDir> { Value = GDir.Input }; S2GP13 = new Enabled<GDir> { Value = GDir.Input }; S2GP14 = new Enabled<GDir> { Value = GDir.Input }; S2GP15 = new Enabled<GDir> { Value = GDir.Input };
            S2GP16 = new Enabled<GDir> { Value = GDir.Input }; S2GP17 = new Enabled<GDir> { Value = GDir.Input }; S2GP18 = new Enabled<GDir> { Value = GDir.Input }; S2GP19 = new Enabled<GDir> { Value = GDir.Input };
            S2GP20 = new Enabled<GDir> { Value = GDir.Input }; S2GP21 = new Enabled<GDir> { Value = GDir.Input }; S2GP22 = new Enabled<GDir> { Value = GDir.Input }; S2GP23 = new Enabled<GDir> { Value = GDir.Input };
            S2GP24 = new Enabled<GDir> { Value = GDir.Input }; S2GP25 = new Enabled<GDir> { Value = GDir.Input }; S2GP26 = new Enabled<GDir> { Value = GDir.Input }; S2GP27 = new Enabled<GDir> { Value = GDir.Input };
            S2GP28 = new Enabled<GDir> { Value = GDir.Input };

            S3GP0 = new Enabled<GDir> { Value = GDir.Input }; S3GP1 = new Enabled<GDir> { Value = GDir.Input }; S3GP2 = new Enabled<GDir> { Value = GDir.Input }; S3GP3 = new Enabled<GDir> { Value = GDir.Input };
            S3GP4 = new Enabled<GDir> { Value = GDir.Input }; S3GP5 = new Enabled<GDir> { Value = GDir.Input }; S3GP6 = new Enabled<GDir> { Value = GDir.Input }; S3GP7 = new Enabled<GDir> { Value = GDir.Input };
            S3GP8 = new Enabled<GDir> { Value = GDir.Input }; S3GP9 = new Enabled<GDir> { Value = GDir.Input }; S3GP10 = new Enabled<GDir> { Value = GDir.Input }; S3GP11 = new Enabled<GDir> { Value = GDir.Input };
            S3GP12 = new Enabled<GDir> { Value = GDir.Input }; S3GP13 = new Enabled<GDir> { Value = GDir.Input }; S3GP14 = new Enabled<GDir> { Value = GDir.Input }; S3GP15 = new Enabled<GDir> { Value = GDir.Input };
            S3GP16 = new Enabled<GDir> { Value = GDir.Input }; S3GP17 = new Enabled<GDir> { Value = GDir.Input }; S3GP18 = new Enabled<GDir> { Value = GDir.Input }; S3GP19 = new Enabled<GDir> { Value = GDir.Input };
            S3GP20 = new Enabled<GDir> { Value = GDir.Input }; S3GP21 = new Enabled<GDir> { Value = GDir.Input }; S3GP22 = new Enabled<GDir> { Value = GDir.Input }; S3GP23 = new Enabled<GDir> { Value = GDir.Input };
            S3GP24 = new Enabled<GDir> { Value = GDir.Input }; S3GP25 = new Enabled<GDir> { Value = GDir.Input }; S3GP26 = new Enabled<GDir> { Value = GDir.Input }; S3GP27 = new Enabled<GDir> { Value = GDir.Input };
            S3GP28 = new Enabled<GDir> { Value = GDir.Input };

        }
      

        public override void PrePlanRun()
        {
            base.PrePlanRun();
        }



        public override void Run()
        {
       
            if ((M1IO0.IsEnabled == true) || (M1IO1.IsEnabled == true) || (S1IO8.IsEnabled == true) || (S1IO9.IsEnabled == true) || (FLAG.IsEnabled == true) || (CTRL.IsEnabled == true))
            {
                ConfigureDefGpio(Selectedgfct);
            }

            if ((M0GP0.IsEnabled == true) || (M0GP1.IsEnabled == true) || (M0GP2.IsEnabled == true) || (M0GP3.IsEnabled == true) || (M0GP4.IsEnabled == true) || (M0GP5.IsEnabled == true) || (M0GP6.IsEnabled == true) || (M0GP7.IsEnabled == true) || (M0GP8.IsEnabled == true) || (M0GP9.IsEnabled == true) || (M0GP10.IsEnabled == true) || (M0GP11.IsEnabled == true) || (M0GP12.IsEnabled == true) || (M0GP13.IsEnabled == true) || (M0GP14.IsEnabled == true) || (M0GP15.IsEnabled == true) || (M0GP16.IsEnabled == true) || (M0GP17.IsEnabled == true) || (M0GP18.IsEnabled == true) || (M0GP19.IsEnabled == true) || (M0GP20.IsEnabled == true) || (M0GP21.IsEnabled == true) || (M0GP22.IsEnabled == true) || (M0GP23.IsEnabled == true) || (M0GP24.IsEnabled == true) || (M0GP25.IsEnabled == true) || (M0GP26.IsEnabled == true) || (M0GP27.IsEnabled == true) || (M0GP28.IsEnabled == true))
            {
                ConfigureAllGpio(0,Selectedgfct);
            }

            if ((S1GP0.IsEnabled == true) || (S1GP1.IsEnabled == true) || (S1GP2.IsEnabled == true) || (S1GP3.IsEnabled == true) || (S1GP4.IsEnabled == true) || (S1GP5.IsEnabled == true) || (S1GP6.IsEnabled == true) || (S1GP7.IsEnabled == true) || (S1GP8.IsEnabled == true) || (S1GP9.IsEnabled == true) || (S1GP10.IsEnabled == true) || (S1GP11.IsEnabled == true) || (S1GP12.IsEnabled == true) || (S1GP13.IsEnabled == true) || (S1GP14.IsEnabled == true) || (S1GP15.IsEnabled == true) || (S1GP16.IsEnabled == true) || (S1GP17.IsEnabled == true) || (S1GP18.IsEnabled == true) || (S1GP19.IsEnabled == true) || (S1GP20.IsEnabled == true) || (S1GP21.IsEnabled == true) || (S1GP22.IsEnabled == true) || (S1GP23.IsEnabled == true) || (S1GP24.IsEnabled == true) || (S1GP25.IsEnabled == true) || (S1GP26.IsEnabled == true) || (S1GP27.IsEnabled == true) || (S1GP28.IsEnabled == true))
            {
                ConfigureAllGpio(1, Selectedgfct);
            }

            if ((S2GP0.IsEnabled == true) || (S2GP1.IsEnabled == true) || (S2GP2.IsEnabled == true) || (S2GP3.IsEnabled == true) || (S2GP4.IsEnabled == true) || (S2GP5.IsEnabled == true) || (S2GP6.IsEnabled == true) || (S2GP7.IsEnabled == true) || (S2GP8.IsEnabled == true) || (S2GP9.IsEnabled == true) || (S2GP10.IsEnabled == true) || (S2GP11.IsEnabled == true) || (S2GP12.IsEnabled == true) || (S2GP13.IsEnabled == true) || (S2GP14.IsEnabled == true) || (S2GP15.IsEnabled == true) || (S2GP16.IsEnabled == true) || (S2GP17.IsEnabled == true) || (S2GP18.IsEnabled == true) || (S2GP19.IsEnabled == true) || (S2GP20.IsEnabled == true) || (S2GP21.IsEnabled == true) || (S2GP22.IsEnabled == true) || (S2GP23.IsEnabled == true) || (S2GP24.IsEnabled == true) || (S2GP25.IsEnabled == true) || (S2GP26.IsEnabled == true) || (S2GP27.IsEnabled == true) || (S2GP28.IsEnabled == true))
            {
                ConfigureAllGpio(2, Selectedgfct);
            }

            if ((S3GP0.IsEnabled == true) || (S3GP1.IsEnabled == true) || (S3GP2.IsEnabled == true) || (S3GP3.IsEnabled == true) || (S3GP4.IsEnabled == true) || (S3GP5.IsEnabled == true) || (S3GP6.IsEnabled == true) || (S3GP7.IsEnabled == true) || (S3GP8.IsEnabled == true) || (S3GP9.IsEnabled == true) || (S3GP10.IsEnabled == true) || (S3GP11.IsEnabled == true) || (S3GP12.IsEnabled == true) || (S3GP13.IsEnabled == true) || (S3GP14.IsEnabled == true) || (S3GP15.IsEnabled == true) || (S3GP16.IsEnabled == true) || (S3GP17.IsEnabled == true) || (S3GP18.IsEnabled == true) || (S3GP19.IsEnabled == true) || (S3GP20.IsEnabled == true) || (S3GP21.IsEnabled == true) || (S3GP22.IsEnabled == true) || (S3GP23.IsEnabled == true) || (S3GP24.IsEnabled == true) || (S3GP25.IsEnabled == true) || (S3GP26.IsEnabled == true) || (S3GP27.IsEnabled == true) || (S3GP28.IsEnabled == true))
            {
                ConfigureAllGpio(3, Selectedgfct);
            }
        }


        /// <summary>
        /// Configures any Gpio.
        /// </summary>
        /// <param name="device"> Device number (0 for Master, 1 for Slave1, 2 for Slave2 and 3 for Slave3).</param>
        /// <param name="portValue">The byte value for configuration.</param>
        /// <param name="selectedFunction">Selected function for the Gpio.</param>
        private void ConfigureAllGpio(int device, GpFct selectedFunction)
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
                if ((selectedFunction == GpFct.Write_read) || (selectedFunction == GpFct.Write_only))
                {
                    string Command = $"GPIO:DIRECTION:DEVICE{device}:GP{bit} {bitCommand}";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read Command
                if ((selectedFunction == GpFct.read_only) || (selectedFunction == GpFct.Write_read))
                {
                    string Command = $"GPIO:DIRECTION:DEVICE{device}:GP{bit}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

                    double.TryParse(response, out double value);
                    string test = (value == Convert.ToDouble(bitState)) ? "PASS" : "FAIL";

                    if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                    else UpgradeVerdict(Verdict.Fail);

                    // Create test result object
                    TestResult<double> result = new TestResult<double>
                    {
                        ParamName = $"{name} Direction",
                        StepName = Name,
                        Value = value,
                        Verdict = test,
                        Units = "read"
                    };

                    // Add limits for Write_Read function
                    if (selectedFunction == GpFct.Write_read)
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
        /// Configures alread defined GPIO

        /// <param name="selectedFunction">Selected function for the port.</param>
        /// 
        public void ConfigureDefGpio(GpFct selectedFunction)
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
                var config = entry.Value;        // Get the Enabled<GDir> object
                string test = "";

                
                var property = GetType().GetProperty(name);

                var bitConfig = property?.GetValue(this) as Enabled<GDir>;

                if (bitConfig == null || !bitConfig.IsEnabled)
                    continue;
                byte dirgpio = (byte)(bitConfig.Value == GDir.Output ? 1 : 0);

                // Write Command
                if ((selectedFunction == GpFct.Write_read) || (selectedFunction == GpFct.Write_only))
                {
                    string Command = $"GPIO:DIRECTION:DEVICE{device}:GP{pin} {dirgpio}";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read Command
                if ((selectedFunction == GpFct.read_only) || (selectedFunction == GpFct.Write_read))
                {
                    string Command = $"GPIO:DIRECTION:DEVICE{device}:GP{pin}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

                    // Try parsing the response
                    if (!double.TryParse(response, out double value))
                    {
                        Log.Warning($"Invalid SCPI response: {response}, expected: {value}");
                        UpgradeVerdict(Verdict.Fail);
                        test = "FAIL";
                    } else
                    {
                        test = "PASS";
                        UpgradeVerdict(Verdict.Pass);
                    }

                    // Create test result object
                    TestResult<double> result = new TestResult<double>
                    {
                        ParamName = $"{name} Direction",
                        StepName = Name,
                        Value = value,
                        Verdict = test,
                        Units = "read"
                    };

                    // Add limits for Write_Read function
                    if (selectedFunction == GpFct.Write_read)
                    {
                        result.LowerLimit = dirgpio;
                        result.UpperLimit = dirgpio;
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
                var bitConfig = property.GetValue(this) as Enabled<GDir>;
                if (bitConfig?.IsEnabled == true)
                {
                    byte value = (byte)(bitConfig.Value == GDir.Output ? 1 : 0);
                    return (value, propertyName);  // Return both bit value and property name
                }
            }
            return (0, propertyName); // Default to input (0) with property name
        }

        private bool IsBitEnabled(int device, int bit)
        {
            string dev = (device == 0) ? "M0": $"S{device}";
            string propertyName = $"{dev}GP{bit}";
            var property = GetType().GetProperty(propertyName);

            return property?.GetValue(this) is Enabled<GDir> bitConfig && bitConfig.IsEnabled;
        }
        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}
