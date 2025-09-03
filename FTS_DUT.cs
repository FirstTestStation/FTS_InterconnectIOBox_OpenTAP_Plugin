using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using OpenTap;

namespace InterconnectIOBox
{



    [Display("DUT-FIXTURE", Group: "First_TestStation")]
    [Description("DUT who will be tested using  First TestStation")]
    public class FTS_DUT : Dut
    {
        private bool smart1Wire;
        private bool enable1WireJ1;
        private bool enable1WireJ2;

        private const string Fixt = "Fixture Information Options";

        [Display("Fixture use 1-Wire Device?", Group: Fixt, Order: 1, Description: "Checked = 1-Wire device is connected on J1 and/or J2 connectors")]
        public bool Smart1Wire {
            get => smart1Wire;
            set
            {
                smart1Wire = value;

                if (!smart1Wire)
                {
                    Enable1WireJ1 = false;
                    Enable1WireJ2 = false;

                    NotifyPropertyChanged(nameof(Enable1WireJ1));
                    NotifyPropertyChanged(nameof(Enable1WireJ2));
                }

                NotifyPropertyChanged(nameof(Smart1Wire));
            }
        }


        [Display("1-Wire contains DUT information?", Group: Fixt, Order: 1.1, Description: "Unchecked = 1-Wire device Contains fixture information, Checked = 1-Wire device content DUT information")]
        [EnabledIf("Smart1Wire", true)]
        public bool Dut1Wire { get; set; } = false;

        [Display("1-Wire device present on J1?", Order: 2, Group: Fixt, Description: "Get Fixture or DUT information from 1-wire connected on J1.")]
        [EnabledIf(nameof(Smart1Wire), true)]
        public bool Enable1WireJ1
        {
            get => enable1WireJ1;
            set => enable1WireJ1 = value;
        }



        [Display("1-Wire device present on J2", Order: 2.1, Group: Fixt, Description: "Get Fixture or DUT information from 1-wire connected on J2.")]
        [EnabledIf(nameof(Smart1Wire), true)]
        public bool Enable1WireJ2
        {
            get => enable1WireJ2;
            set => enable1WireJ2 = value;
        }

        // Add this event to the FTS_DUT class to support property change notifications
        public new event PropertyChangedEventHandler PropertyChanged;

        [Display("1-Wire String Check ID", Group: Fixt, Order: 2.2, Description: "Expected ID string for 1-Wire device on J1 or/and J2 to validate correct fixture.")]
        [EnabledIf("Smart1Wire", true)]
        public string CheckID { get; set; }

        private const string PFixt = "Fixture Information when no 1-Wire is present";

        [Display("Fixture Name", Group: PFixt, Order: 3.0, Collapsed: true, Description: "Fixture Name to be used when no 1-Wire is present.")]
        [EnabledIf("Smart1Wire", false)]
        public string FixtName { get; set; }

        [Display("Fixture Number", Group: PFixt, Order: 3.1, Collapsed: true, Description: "Fixture Number to be used when no 1-Wire is present.")]
        [EnabledIf("Smart1Wire", false)]
        public string FixtNumber { get; set; }

        [Display("Fixture Serial", Order: 3.3, Group: PFixt, Description: "Optionnal Serial Number of the Fixture ")]
        [EnabledIf("Smart1Wire", false)]
        public string FixtSerial { get; set; }




        private const string DFixt = "DUT Information when is not 1-Wire data";

        [Display("ProductName", Order: 4.1, Group: DFixt, Description: "Product Name of the Device Under Test (DUT)")]
        [EnabledIf("Dut1Wire", false)]
        public string ProductName { get; set; }

        [Display("PartNumber", Order: 4.2, Group: DFixt, Description: "PartNumber of the Device Under Test (DUT)")]
        [EnabledIf("Dut1Wire", false)]
        public string PartNumber { get; set; }

        [Display("SerialNumber", Order: 4.3, Group: DFixt, Description: "SerialNumber of the Device Under Test (DUT)")]
        [EnabledIf("Dut1Wire", false)]
        public string SerialNumber { get; set; }



     
        [Display("I2C Address:", Group: "DUT I2C Address ", Order: 5, Description: "I2C address to use to communicate with DUT (mainly selftest board)")]
        public byte I2CSelftestaddress { get; set; } = 0x20;


        /// <summary>
        /// Initializes a new instance of this DUT class.
        /// </summary>
        public FTS_DUT()
        {
            // ToDo: Set default values for properties / settings.
            Name = "FTS_DUT";
            // Default settings can be configured in the constructor.
           // ID = "500-1010"; // Partnumber expected of the DUT
     
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
                if (Dut1Wire)
                {
                    if (string.IsNullOrEmpty(ProductName))
                    {
                        Log.Warning("ProductName Field is empty. ProductName is Required");
                        error = true;
                    }
                }
                else
                { // Fixture information 
                    if (string.IsNullOrEmpty(FixtName))
                    {
                        Log.Warning("Fixture Name Field is empty. Fixture Name is Required");
                        error = true;
                    }
                }

                if (Dut1Wire)
                {
                    if (string.IsNullOrEmpty(PartNumber))
                    {
                        Log.Warning("PartNumber Field is empty. PartNumber is Required");
                        error = true;
                    }
                }
                else
                { // Fixture information 
                    if (string.IsNullOrEmpty(FixtNumber))
                    {
                        Log.Warning("Fixture Number Field is empty. Fixture Number is Required");
                        error = true;
                    }
                }


                if (error)
                {
                    Log.Error("Some informations are missing. Please fill in the required fields.");
                    throw new ArgumentException("Information are missing. Please fill in the required fields.");
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
        // Add this method to the FTS_DUT class to fix CS0103
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}