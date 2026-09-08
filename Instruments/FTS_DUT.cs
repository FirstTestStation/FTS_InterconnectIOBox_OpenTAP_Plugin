using System;
using System.ComponentModel;
using OpenTap;

namespace InterconnectIOBox.Instruments
{
    [Display("DUT-FIXTURE", Group: "First_TestStation", Description: "DUT to be tested using the First TestStation.")]
    public class FTS_DUT : Dut
    {
        private bool smart1Wire;
        private bool enable1WireJ1;
        private bool enable1WireJ2;
        private bool dut1Wire;


        private const string Fixt = "Fixture Information Options";

        [Display("A- Fixture Uses a 1-Wire Device (Smart1Wire)?", Group: Fixt, Order: 1, Description: "Checked = a 1-Wire device is connected on the J1 and/or J2 connectors.")]
        public bool Smart1Wire
        {
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


        [Display("B- Fixture Contains DUT Information (Dut1Wire)?", Group: Fixt, Order: 1.1, Description: "Unchecked = the 1-Wire device contains fixture information. Checked = the 1-Wire device contains DUT information (product name, part number, and serial number).")]
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



        [Display("C- 1-Wire Device Present on J1?", Order: 2, Group: Fixt, Description: "Get fixture or DUT information from the 1-Wire device connected on J1.")]
        [EnabledIf(nameof(Smart1Wire), true)]
        public bool Enable1WireJ1
        {
            get => enable1WireJ1;
            set => enable1WireJ1 = value;
        }



        [Display("D- 1-Wire Device Present on J2?", Order: 2.1, Group: Fixt, Description: "Get fixture or DUT information from the 1-Wire device connected on J2.")]
        [EnabledIf(nameof(Smart1Wire), true)]
        public bool Enable1WireJ2
        {
            get => enable1WireJ2;
            set => enable1WireJ2 = value;
        }

        // NOTE: this redeclares (hides) the base class's PropertyChanged event rather than
        // reusing it. See the note above this class: the GUI may not receive live
        // notifications raised through NotifyPropertyChanged() if it subscribed to the
        // base class's event via INotifyPropertyChanged. Verify this behaves as expected
        // in the plan editor; if not, look for the correct protected notifier exposed by
        // the Dut/ValidatingObject base class instead of hiding the event here.
        public new event PropertyChangedEventHandler PropertyChanged;

        [Display("E- 1-Wire String Check ID", Group: Fixt, Order: 2.2, Description: "Expected ID string for the 1-Wire device on J1 and/or J2, used to validate the correct fixture.")]
        [EnabledIf("Smart1Wire", true)]
        public string CheckID { get; set; }

        private const string PFixt = "Fixture Information (when data does not come from a 1-Wire device)";

        [Browsable(false)]
        public bool EnableFixt => !Smart1Wire || Dut1Wire;

        [Display("Fixture Name", Group: PFixt, Order: 3.0, Description: "Fixture name to use when no 1-Wire device is present.")]
        [EnabledIf("EnableFixt", true)]
        public string FixtName { get; set; }

        [Display("Fixture Number", Group: PFixt, Order: 3.1, Description: "Fixture number to use when no 1-Wire device is present.")]
        [EnabledIf("EnableFixt", true)]
        public string FixtNumber { get; set; }

        [Display("Fixture Serial", Order: 3.3, Group: PFixt, Description: "Optional serial number of the fixture.")]
        [EnabledIf("EnableFixt", true)]
        public string FixtSerial { get; set; }




        private const string DFixt = "DUT Information (when not from 1-Wire data)";

        [Display("Product Name", Order: 4.1, Group: DFixt, Description: "Product name of the Device Under Test (DUT).")]
        [EnabledIf("Dut1Wire", false)]
        public string ProductName { get; set; }

        [Display("Part Number", Order: 4.2, Group: DFixt, Description: "Part number of the Device Under Test (DUT).")]
        [EnabledIf("Dut1Wire", false)]
        public string PartNumber { get; set; }


        [Display("DUT Serial Number", Order: 5, Group: "Status", Description: "Actual serial number of the Device Under Test (DUT).")]
        [Output]
        public string SerialNumber { get; set; }



        [Display("I2C Address:", Group: "DUT I2C Address", Order: 6, Description: "I2C address used to communicate with the DUT (mainly the selftest board).")]
        public byte I2CSelftestaddress { get; set; } = 0x20;


        /// <summary>
        /// Initializes a new instance of this DUT class.
        /// </summary>
        public FTS_DUT()
        {
            const string errorMessage = "The 'Check ID' field cannot be empty when 'Fixture contains DUT information?' is set to true.";

            IsValidDelegateDefinition validationLogic = () =>
            {
                // If Dut1Wire is false, validation passes.
                if (Dut1Wire == false)
                {
                    return true;
                }

                // If Dut1Wire is true, validation passes only if CheckID is not empty.
                return !string.IsNullOrWhiteSpace(CheckID);
            };

            Rules.Add(validationLogic,
                      errorMessage,
                      nameof(Dut1Wire),
                      nameof(CheckID));

            // ToDo: Set default values for properties / settings.
            Name = "FTS_DUT";
        }

        /// <summary>
        /// Opens a connection to the DUT represented by this class.
        /// </summary>
        public override void Open()
        {
            Log.Info("Open FTS_DUT.");
            base.Open();

            bool autoInfo = false;
            bool error = false;

            // --- Enable 1-Wire interfaces ---
            if (Enable1WireJ1)
            {
                Log.Info("1-Wire J1 enabled.");
                autoInfo = true;
            }

            if (Enable1WireJ2)
            {
                Log.Info("1-Wire J2 enabled.");
                autoInfo = true;
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
                    Log.Error("The 1-Wire CheckID field is empty. CheckID is required when Smart1Wire is enabled.");
                    error = true;
                }
            }

            // --- If no automatic 1-Wire info is used, require manual fallback fields ---
            if (!autoInfo)
            {
                if (Dut1Wire)
                {
                    // DUT information would normally come from 1-Wire; since no wire is enabled, require it manually.
                    if (string.IsNullOrEmpty(ProductName))
                    {
                        Log.Error("The Product Name field is empty. Product Name is required.");
                        error = true;
                    }

                    if (string.IsNullOrEmpty(PartNumber))
                    {
                        Log.Error("The Part Number field is empty. Part Number is required.");
                        error = true;
                    }
                }
                else
                {
                    // Fixture information would normally come from 1-Wire; since no wire is enabled, require it manually.
                    if (string.IsNullOrEmpty(FixtName))
                    {
                        Log.Error("The Fixture Name field is empty. Fixture Name is required.");
                        error = true;
                    }

                    if (string.IsNullOrEmpty(FixtNumber))
                    {
                        Log.Error("The Fixture Number field is empty. Fixture Number is required.");
                        error = true;
                    }
                }
            }

            // --- DUT product info is always required manually when the 1-Wire device
            // is not carrying DUT information (i.e. it's carrying fixture info instead,
            // or Smart1Wire is off entirely). ---
            if (!Dut1Wire)
            {
                if (string.IsNullOrEmpty(ProductName) || string.IsNullOrEmpty(PartNumber))
                {
                    Log.Error("Dut1Wire is unchecked. You must manually provide Product Name and Part Number.");
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
        /// Closes the connection made to the DUT represented by this class.
        /// </summary>
        public override void Close()
        {
            base.Close();
            Log.Info("Closing FTS_DUT.");
        }

        // Unique to this class.
        public void DoNothing()
        {
            OnActivity();   // Causes the GUI to indicate progress
            Log.Info("FTS_DUT called.");
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}