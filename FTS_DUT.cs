using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using OpenTap;

namespace InterconnectIOBox
{

    [Display("DUT Module", Group: "First_TestStation")]
    [Description("DUT who will be tested using  First TestStation")]
    public class FTS_DUT : Dut
    {


        [Display("1-Wire device present on J1", Order: 1, Group: "DUT Information from 1-Wire", Description: "Get DUT information from 1-wire connected on J1.")]
        public bool Enable1WireJ1 { get; set; }


        [Display("1-Wire device present on J2", Order: 2, Group: "DUT Information from 1-Wire", Description: "Get DUT information from 1-wire connected on J2.")]
        public bool Enable1WireJ2 { get; set; }



       // [MetaData(true)]
        [Display("ProductName", Order: 3.1, Group: "DUT Information", Description: "Product Name of the Device Under Test (DUT)")]
        [EnabledIf("Enable1WireJ1", false, Flags = false)]
        [EnabledIf("Enable1WireJ2", false, Flags = false)]
        public string ProductName { get; set; }

       // [MetaData(true)]
        [Display("PartNumber", Order: 3.2, Group: "DUT Information", Description: "PartNumber of the Device Under Test (DUT)")]
        [EnabledIf("Enable1WireJ1", false, Flags = false)]
        [EnabledIf("Enable1WireJ2", false, Flags = false)]
        public string PartNumber { get; set; }

    //    [MetaData(true)]
        [Display("SerialNumber", Order: 3.3, Group: "DUT Information", Description: "SerialNumber of the Device Under Test (DUT)")]
        [EnabledIf("Enable1WireJ1", false, Flags = false)]
        [EnabledIf("Enable1WireJ2", false, Flags = false)]
        public string Serial { get; set; }


        [Display("I2C Address:", Group: "DUT I2C Address ", Order: 4, Description: "I2C address to use to communicate with selftest board. Defined in  DUT")]
        public byte I2CSelftestaddress { get; set; } = 0x20;

        /// <summary>
        /// Initializes a new instance of this DUT class.
        /// </summary>
        public FTS_DUT()
        {
            // ToDo: Set default values for properties / settings.
            Name = "FTS_DUT";
            // Default settings can be configured in the constructor.
            ID = "500-1010"; // Partnumber expected of the DUT
     
        }

        /// <summary>
        /// Opens a connection to the DUT represented by this class
        /// </summary>
        public override void Open()
        {
            Log.Info("Open FTS_DUT.");
            base.Open();
            bool AutoInfo = false;

            // Implement logic to handle 1-Wire interface based on properties
            if (Enable1WireJ1)
            {
                // Code to enable 1-Wire on J1
                Log.Info("1-Wire J1 enabled.");
                AutoInfo = true;
            }
            if (Enable1WireJ2)
            {
                // Code to enable 1-Wire on J2
                Log.Info("1-Wire J2 enabled.");
                AutoInfo = true;
            }

            if (!AutoInfo) // If no 1-Wire interface is enabled, check if required fields are filled in
            {
                bool error = false;
                {

                    Log.Info("No 1-Wire interface enabled. ");

                    if (string.IsNullOrEmpty(ProductName))
                    {
                        Log.Warning("ProductName Field is empty. ProductName is Required");
                        error = true;
                    }


                    if (string.IsNullOrEmpty(PartNumber))
                    {
                        Log.Warning("PartNumber Field is empty. PartNumber is Required");

                    }

                    if (string.IsNullOrEmpty(Serial))
                    {
                        Log.Warning("SerialNumber Field is empty. SerialNumber is Required");

                    }
                }

                if (error)
                {
                    Log.Error("DUT information is missing. Please fill in the required fields.");
                    throw new ArgumentException("DUT information is missing. Please fill in the required fields.");
                }
            }
        }


        /// <summary>
        /// Closes the connection made to the DUT represented by this class
        /// </summary>
        public override void Close()
        {
            // TODO: close connection to DUT
            base.Close();
            Log.Info("Closing FTS_DUT.");
        }

        // Unique to this class.
        public void DoNothing()
        {
            OnActivity();   // Causes the GUI to indicate progress
            Log.Info("FTS_DUT called.");
        }
    }
}

