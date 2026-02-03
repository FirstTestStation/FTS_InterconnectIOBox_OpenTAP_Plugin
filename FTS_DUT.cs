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
        private bool dut1Wire;


        private const string Fixt = "Fixture Information Options";

        [Display("A- Fixture use 1-Wire Device (Smart1Wire) ?", Group: Fixt, Order: 1, Description: "Checked = 1-Wire device is connected on J1 and/or J2 connectors")]
        public bool Smart1Wire {
            get => smart1Wire;
            set
            {
                smart1Wire = value;

                if (!smart1Wire)
                {
                    Dut1Wire = false;
                    Enable1WireJ1 = false;
                    Enable1WireJ2 = false;

                    NotifyPropertyChanged(nameof(Dut1Wire));
                    NotifyPropertyChanged(nameof(Enable1WireJ1));
                    NotifyPropertyChanged(nameof(Enable1WireJ2));
                }
                else
                {
                    // When turning ON Smart1Wire → clear manual fixture info
                    FixtName = string.Empty;
                    FixtNumber = string.Empty;
                    FixtSerial = string.Empty;

                    NotifyPropertyChanged(nameof(FixtName));
                    NotifyPropertyChanged(nameof(FixtNumber));
                    NotifyPropertyChanged(nameof(FixtSerial));
                }

                NotifyPropertyChanged(nameof(Smart1Wire));
                NotifyPropertyChanged(nameof(EnableFixt));
            }
        }


        [Display("B- Fixture contains DUT information (Dut1Wire) ?", Group: Fixt, Order: 1.1, Description: "Unchecked = 1-Wire device Contains fixture information, Checked = 1-Wire device content DUT information: PartNumber and SerialNumber")]
        [EnabledIf("Smart1Wire", true)]
        public bool Dut1Wire
        {
            get => dut1Wire;
            set
            {
                dut1Wire = value;

                if (dut1Wire)
                {
                    // When B is checked → clear manual DUT info
                    ProductName = string.Empty;
                    PartNumber = string.Empty;
                    SerialNumber = string.Empty;

                    NotifyPropertyChanged(nameof(ProductName));
                    NotifyPropertyChanged(nameof(PartNumber));
                    NotifyPropertyChanged(nameof(SerialNumber));
                }

                NotifyPropertyChanged(nameof(Dut1Wire));
                NotifyPropertyChanged(nameof(EnableFixt));
            }
        }



        [Display("C- 1-Wire device present on J1?", Order: 2, Group: Fixt, Description: "Get Fixture or DUT information from 1-wire connected on J1.")]
        [EnabledIf(nameof(Smart1Wire), true)]
        public bool Enable1WireJ1
        {
            get => enable1WireJ1;
            set => enable1WireJ1 = value;
        }



        [Display("D- 1-Wire device present on J2?", Order: 2.1, Group: Fixt, Description: "Get Fixture or DUT information from 1-wire connected on J2.")]
        [EnabledIf(nameof(Smart1Wire), true)]
        public bool Enable1WireJ2
        {
            get => enable1WireJ2;
            set => enable1WireJ2 = value;
        }

        // Add this event to the FTS_DUT class to support property change notifications
        public new event PropertyChangedEventHandler PropertyChanged;

        [Display("E- 1-Wire String Check ID", Group: Fixt, Order: 2.2, Description: "Expected ID string for 1-Wire device on J1 or/and J2 to validate correct fixture.")]
        [EnabledIf("Smart1Wire", true)]
        public string CheckID { get; set; }

        private const string PFixt = "Fixture Information when data is not coming from 1-Wire device";

        [Browsable(false)]
        public bool EnableFixt => !Smart1Wire || Dut1Wire;

        [Display("Fixture Name", Group: PFixt, Order: 3.0, Description: "Fixture Name to be used when no 1-Wire is present.")]
        [EnabledIf("EnableFixt", true)]
        public string FixtName { get; set; }

        [Display("Fixture Number", Group: PFixt, Order: 3.1,  Description: "Fixture Number to be used when no 1-Wire is present.")]
        [EnabledIf("EnableFixt", true)]
        public string FixtNumber { get; set; }

        [Display("Fixture Serial", Order: 3.3, Group: PFixt, Description: "Optionnal Serial Number of the Fixture ")]
        [EnabledIf("EnableFixt", true)]
        public string FixtSerial { get; set; }




        private const string DFixt = "DUT Information when is not 1-Wire data";

        [Display("ProductName", Order: 4.1, Group: DFixt, Description: "Product Name of the Device Under Test (DUT)")]
        [EnabledIf("Dut1Wire", false)]
        public string ProductName { get; set; }

        [Display("PartNumber", Order: 4.2, Group: DFixt, Description: "PartNumber of the Device Under Test (DUT)")]
        [EnabledIf("Dut1Wire", false)]
        public string PartNumber { get; set; }


        [Display("DUT SerialNumber", Order: 5, Group: "Status", Description: "Actual SerialNumber of the Device Under Test (DUT)")]
        [Output]
        public string SerialNumber { get; set; }



     
        [Display("I2C Address:", Group: "DUT I2C Address ", Order: 6, Description: "I2C address to use to communicate with DUT (mainly selftest board)")]
        public byte I2CSelftestaddress { get; set; } = 0x20;


      


        /// <summary>
        /// Initializes a new instance of this DUT class.
        /// </summary>
        public FTS_DUT()
        {
            // 1. Define the desired error message
            const string errorMessage = "The 'Check ID' cannot be empty when '1-Wire contains DUT information?' is set to True.";

            // 2. DEFINE THE LOGIC USING THE OPEN TAP DELEGATE TYPE
            // This explicitly converts the Lambda function into the required delegate type.
            OpenTap.IsValidDelegateDefinition validationLogic = () =>
            {
                // If Dut1Wire is False, validation passes (True).
                if (Dut1Wire == false)
                {
                    return true;
                }

                // If Dut1Wire is True, validation passes (True) ONLY if CheckID is NOT empty.
                return !string.IsNullOrWhiteSpace(CheckID);
            };

            // 3. Add the rule using the Rules.Add overload.
            // The explicit delegate definition should now satisfy the first argument.
            Rules.Add(validationLogic,
                      errorMessage,
                      nameof(Dut1Wire),
                      nameof(CheckID));

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
            bool error = false;

            // --- Enable 1-Wire interfaces ---
            if (Enable1WireJ1)
            {
                Log.Info("1-Wire J1 enabled.");
                AutoInfo = true;
            }

            if (Enable1WireJ2)
            {
                Log.Info("1-Wire J2 enabled.");
                AutoInfo = true;
            }

            // --- Validate Smart1Wire dependencies ---
            if (Smart1Wire)
            {
                // Must have at least one 1-Wire interface enabled
                if (!Enable1WireJ1 && !Enable1WireJ2)
                {
                    Log.Error("Smart1Wire is enabled, but neither 1-Wire J1 nor J2 is enabled. Please enable at least one.");
                    error = true;
                }

                // CheckID must also be defined
                if (string.IsNullOrWhiteSpace(CheckID))
                {
                    Log.Error("1-Wire CheckID field is empty. CheckID is required when Smart1Wire is enabled.");
                    error = true;
                }
            }

            // --- If no 1-Wire automatic info is used ---
            if (!AutoInfo)
            {
                if (Dut1Wire)
                {
                    // DUT information expected from 1-Wire or provided manually
                    if (string.IsNullOrEmpty(ProductName))
                    {
                        Log.Warning("ProductName field is empty. ProductName is required.");
                        error = true;
                    }

                    if (string.IsNullOrEmpty(PartNumber))
                    {
                        Log.Warning("PartNumber field is empty. PartNumber is required.");
                        error = true;
                    }
                }
                else
                {
                    // Fixture information
                    if (string.IsNullOrEmpty(FixtName))
                    {
                        Log.Warning("Fixture Name field is empty. Fixture Name is required.");
                        error = true;
                    }

                    if (string.IsNullOrEmpty(FixtNumber))
                    {
                        Log.Warning("Fixture Number field is empty. Fixture Number is required.");
                        error = true;
                    }
                }
            }

            // --- New Rule ---
            // If Dut1Wire is NOT checked, operator must manually fill ProductName and PartNumber
            if (!Dut1Wire)
            {
                if (string.IsNullOrEmpty(ProductName) || string.IsNullOrEmpty(PartNumber))
                {
                    Log.Error("Dut1Wire is unchecked. You must manually provide ProductName and PartNumber.");
                    error = true;
                }
            }

            // --- Final error check ---
            if (error)
            {
                Log.Error("Some information is missing. Please fill in the required fields.");
                throw new ArgumentException("Information is missing. Please fill in the required fields.");
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