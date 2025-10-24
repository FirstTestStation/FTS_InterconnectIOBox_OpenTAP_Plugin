using OpenTap;
using OpenTap.Diagnostic;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Security;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace InterconnectIOBox // DUT Validation
{
    public abstract class OneWireReadWrite : ResultTestStep
    {
        #region Settings
        public InterconnectIO IO_Instrument { get; set; }

      
        #endregion


        /// <summary>
        /// Check on Selftest_DUT if J1 and J2 has been checked.
        /// </summary>
        /// <returns int = nbWire >The number of 1-Wire on the DUT.
        /// <returns bool = j1 > True is 1-Wire on connector  J1 is enabled.
        /// <returns bool = j2 > True is 1-Wire on connector  J2 is enabled.</returns>
        protected (int nbWire, bool j1, bool j2) NumberofOneWires()
        {
            int nbWire = 0;
            bool j1 = false;
            bool j2 = false;

         //   string OwireID = OWire_Dut.ID;  // Get 1-Wire ID from DUT

            // Check if Enable1WireJ1 is true, then increment nbWire
            if (Dut.Enable1WireJ1) { nbWire++; j1 = true; }

            // Check if Enable1WireJ1 is true, then increment nbWire
            if (Dut.Enable1WireJ2) { nbWire++; j2 = true; }
            // Common logic
            return (nbWire, j1, j2);
        }
    }
    [Display(Groups: new[] { "InterconnectIO", "1-Wire" }, Name: "1-Wire validation", Description: "Validate the part number of the Fixture or DUT by reading data from its 1-Wire " +
        "device. If the test FAILS, the Testplan must be aborted to prevent potential damage to an unknown Fixture .In the TestStep," +
        "enable the break condition when the TestStep encounters an error or failure.")]
    public class OneWireDUTRead : OneWireReadWrite
    {
        public OneWireDUTRead()
        {
            // ToDo: Set default values for properties / settings.
        }

        /// <summary>
        /// Sends a SCPI command to read 1-Wire devices on the DUT.
        /// </summary>
        /// <param int= qty> Number of 1-Wire to read.</param>
        /// <returns>The data read from the 1-Wire interface.</returns>
        public string ReadOneWires(int qty)
        {

            string command = $"COM:OWIRE:READ? {qty}";
            Log.Info($"Sending SCPI command: {command}");

            // Use ScpiQuery to read back from the device.
            string response = IO_Instrument.ScpiQuery<string>(command);

            Log.Info($"1-Wire received response: {response}");
            return response;
        }
        public override void PrePlanRun()
        {
            base.PrePlanRun();

            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        public override void Run()
        {
            string sdata;
            string scid = "";  // contains connector id
            string test = "FAIL"; // default test result
            bool valid, vconn;
            bool verdict1, verdict2;
            string WireSerialNumber = null;
            string WireName = null;
            string WirePartNumber = null;
            string ReportValue = null;

            var dutwire = NumberofOneWires();
            int nbWire = dutwire.nbWire;
            bool j1 = dutwire.j1;
            bool j2 = dutwire.j2;

            string OwireID = Dut.CheckID;  // Get 1-Wire ID from DUT

            if (j1) { scid = "J1"; }

            if (nbWire > 0) // Check if there is 1-Wire device to check
            {
                sdata = ReadOneWires(nbWire); // read 1-Wires value
                string[] field = sdata.Split(new string[] { "," }, StringSplitOptions.None);
                WireSerialNumber = field[3].Trim(); // get serial number from 1-Wire data
                WirePartNumber = field[2].Trim(); // get PartNumber from 1-Wire data
                WireName = field[1].Trim(); // get Name from 1-Wire data


                Log.Info("1-Wire Data: " + sdata);

                if (sdata.Length < 16) // if answer is not valid
                {
                    sdata = ReadOneWires(1); // retry read with a single 1-Wire value
                    if (sdata.Length < 16)   // if no 1-wire detected
                    {
                        Log.Info("No 1-Wire device was detected. Check DUT setup");
                        UpgradeVerdict(Verdict.Error);
                    }
                    else
                    {
                        Log.Info("1-Wire data is too short, retry with number of 1-Wire = 1, value: " + sdata);
                    }

                }

                if (nbWire == 1)    // if only single 1-wire device
                {
                    if (j1) { scid = "J1"; } // if 1-Wire on J1 is enabled
                    if (j2) { scid = "J2"; }
                    valid = sdata.Contains(OwireID); // check if 1-Wire ID is valid
                    vconn = sdata.Contains(scid);// check if connector  ID is valid
                    if (valid && vconn)
                    {
                        Log.Info("1-Wire Fixture partnumber is valid for: " + scid);
                        UpgradeVerdict(Verdict.Pass);
                        test = "PASS";
                    }
                    else
                    {
                        if (!vconn)  // if connector ID not valid
                        {
                            Log.Error("1-Wire connector ID is not valid. Expected: " + scid);

                        }
                        if (!valid)  // if 1-wire board ID not valid
                        {
                            Log.Error("1-Wire Fixture partnumber is not valid for: " + scid + " - does not contains ID: " + OwireID);

                        }
                        UpgradeVerdict(Verdict.Fail);
                        test = "FAIL";

                    }
                }

                if (nbWire == 2)    // if double 1-wire device
                {
                    // Split string by "] ["
                    string[] parts = sdata.Split(new string[] { "] [" }, StringSplitOptions.None);
                    valid = false; verdict1 = false; verdict2 = false;

                    scid = "J1";
                    vconn = parts[0].Contains(scid); // check if 1-Wire ID is valid
                    if (vconn)
                    {   // if J1 found
                        valid = parts[0].Contains(OwireID); // check if 1-Wire ID is vali
                        if (valid)
                        {
                            Log.Info("1-Wire partnumber and connector ID is valid for:" + scid);
                            verdict1 = true;
                        }
                        else
                        {
                            Log.Error("1-Wire PartNumber is not valid for:" + scid + " - does not contains ID: " + OwireID);
                            verdict1 = false;
                        }
                    }
                    else
                    {
                        scid = "J2";
                        vconn = parts[0].Contains(scid); // check if 1-Wire ID is valid
                        if (vconn)
                        {   // if J2 found
                            valid = parts[0].Contains(OwireID); // check if 1-Wire ID is vali
                            if (valid)
                            {
                                Log.Info("1-Wire PartNumber and connector ID is valid for:" + scid);
                                verdict1 = true;
                            }
                            else
                            {
                                Log.Error("1-Wire PartNumber is not valid for:" + scid + " - does not contains ID: " + OwireID);
                                verdict1 = false;
                            }
                        }
                    }
                    scid = "J1";
                    vconn = parts[1].Contains(scid); // check if 1-Wire ID is valid
                    if (vconn)
                    {   // if J1 found
                        valid = parts[1].Contains(OwireID); // check if 1-Wire ID is valid
                        if (valid)
                        {
                            Log.Info("1-Wire PartNumber is valid for: " + scid);
                            verdict2 = true;
                        }
                        else
                        {
                            Log.Error("1-Wire PartNumber is not valid for: " + scid + " - does not contains ID: " + OwireID);
                            verdict2 = false;
                        }
                    }
                    else
                    {
                        scid = "J2";
                        vconn = parts[1].Contains(scid); // check if 1-Wire ID is valid
                        if (vconn)
                        {   // if J2 found
                            valid = parts[1].Contains(OwireID); // check if 1-Wire ID is vali
                            if (valid)
                            {
                                Log.Info("1-Wire PartNumber is valid for: " + scid);
                                verdict2 = true;
                            }
                            else
                            {
                                Log.Error("1-Wire PartNumber is not valid for: " + scid + " - does not contains ID: " + OwireID);
                                verdict2 = false;
                            }

                        }
                        else
                        {
                            Log.Error("1-Wire connector ID not found for: " + scid);
                        }
                    }


                    if (verdict1 && verdict2)
                    {
                        UpgradeVerdict(Verdict.Pass);
                        test = "PASS";
                        scid = "J1-J2";

                    }
                    else
                    {
                        UpgradeVerdict(Verdict.Fail);
                        test = "FAIL";
                        scid = "J1-J2";

                    }
                }
            }

            else // if nb_wires ==0 
            {
                Log.Warning("No 1-Wire device to check.");
                UpgradeVerdict(Verdict.Fail);
                nbWire = 0;
            }

            if (nbWire > 0) // if 1-Wire device present, set DUT or Fixture information from 1-Wire data
            {
                if (Dut.Dut1Wire)
                {
                    Dut.SerialNumber = WireSerialNumber;  // Set SerialNumber from 1-Wire ID from DUT
                    Dut.PartNumber = WirePartNumber;
                    Dut.ProductName = WireName;
                    ReportValue = Dut.PartNumber;
                    Log.Info($"[KEY] DUTSerialNumber: {Dut.SerialNumber}");
                }
                else
                {
                    Dut.FixtSerial = WireSerialNumber;  // Set SerialNumber from 1-Wire ID from DUT
                    Dut.FixtNumber = WirePartNumber;
                    Dut.FixtName = WireName;
                    ReportValue = Dut.FixtNumber;
                }
            }

            // Publish final result
            var result = new TestResult<string>
            {
                ParamName = "1-Wire Check",
                StepName = Name,
                Value = ReportValue,
                LowerLimit = scid,
                UpperLimit = OwireID,
                Verdict = test,
                Units = nbWire.ToString()
            };

            PublishResult(result);

        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }

    }


    [Display(Groups: new[] { "InterconnectIO", "1-Wire" }, Name: "1-Wire Write Data", Description: "Write data (partnumber,serialnumber and connector refdes) on 1-Wire DUT device")]

    public class OneWireDUTWrite : OneWireReadWrite
    {

        #region Settings
        [Display("Write Data on 1-Wire J1", Order: 1, Group: "1-Wire Device", Description: "Write Data on 1-wire connected to J1.")]
        public bool EWireJ1
        {
            get => _eWireJ1;
            set
            {
                _eWireJ1 = value;
                if (value) EWireJ2 = false; // Disable J2 when J1 is selected
                OnPropertyChanged(nameof(EWireJ1));
                OnPropertyChanged(nameof(EWireJ2));
            }
        }
        private bool _eWireJ1;

        [Display("Write Data on 1-Wire J2", Order: 2, Group: "1-Wire Device", Description: "Write Data on 1-wire connected to J2.")]
        public bool EWireJ2
        {
            get => _eWireJ2;
            set
            {
                _eWireJ2 = value;
                if (value) EWireJ1 = false; // Disable J1 when J2 is selected
                OnPropertyChanged(nameof(EWireJ1));
                OnPropertyChanged(nameof(EWireJ2));
            }
        }
        private bool _eWireJ2;



        [Display("Fixture Name", Group: "Fixture Data Write Field:")]
        [Description("ProductName to write to the 1-Wire device, used during Fixture validation")]
        public string ProductName { get; set; }


        [Display("Fixture PartNumber", Group: "Fixture Data Write Field:")]
        [Description("Fixture part number to write to the 1-Wire device, used during Fixture validation")]
        public string PartNumber { get; set; }

        [Display("Fixture SerialNumber", Group: "Fixture Data Write Field:")]
        [Description("Fixture serial number  to write to the 1-Wire device, read during Fixture validation")]
        public string SerialNumber { get; set; }

        #endregion


        public OneWireDUTWrite()
        {
            // ToDo: Set default values for properties / settings.
            ProductName = "FTS";
            PartNumber = "610-1010-020";
            // Default settings can be configured in the constructor.
            SerialNumber = "";
            EWireJ1 = true;  // Default to J1
            EWireJ2 = false; // Ensure J2 is off by default

        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        /// <summary>
        /// Sends a SCPI command to check 1-Wire devices on the DUT (read 1-Wire Identification number).
        /// </summary>
        /// <param int= qty> Number of 1-Wire to read.</param>
        /// <returns>The data read from the 1-Wire interface.</returns>
        public string CheckOneWires(int qty)
        {

            string command = $"COM:OWIRE:CHECK? {qty}";
            Log.Info($"Sending SCPI command: {command}");

            // Use ScpiQuery to read back from the device.
            string response = IO_Instrument.ScpiQuery<string>(command);

            Log.Info($"1-Wire Received response: {response}");
            return response;
        }

        public override void Run()
        {
            string scid = "";

            string sdata;
            sdata = CheckOneWires(1); // read 1-Wires value
            int count = sdata.Count(c => c == 'O');

            if (count == 0)
            {
                Log.Warning("No 1-Wire device detected on the Fixture. Verify that a single 1-Wire device is present on J1 or J2 of the Fixture");
                UpgradeVerdict(Verdict.Error);
                return;
            }

            if (count > 1)
            {
                Log.Warning("Only a single 1-Wire device can be programmed at a time. Number of 1-Wire devices detected: " + count);
                UpgradeVerdict(Verdict.Fail);
                return;
            }


            if (sdata.Length > 64)
            {

                Log.Error("1-Wire Read Info is too long. expected 64 chars maximum, counted:  " + sdata.Length);
                UpgradeVerdict(Verdict.Error);

            }

            // Split string by ":"
            string[] parts = sdata.Split(new string[] { ":" }, StringSplitOptions.None);

            string owid = parts[1].Replace(" ", ""); // remove space before owid number
            string owide = owid.Replace("\"", ""); // remove /" at the end of owid number

            int owl = owide.Length;  // get length to verify number

            if (owl != 16) // if not valid OWID
            {
                Log.Info("1-Wire ID is wrong, expect 16 digits, number of digit read: " + owl);
                UpgradeVerdict(Verdict.Error);
                return;

            }

            if (EWireJ1) { scid = "J1"; } // if J1 1-wire device is present
            if (EWireJ2) { scid = "J2"; } // if J2 1-wire device is present


            string wdata = "\"" + owide + ", " + ProductName + ", " + PartNumber + ", " + SerialNumber + ", " + scid + "\"";
            Log.Info("1-Wire Write string: " + wdata);

            int lw = wdata.Length;
            if (lw > 64) // if string is too long
            {
                Log.Error("1-Wire Write Info is too long. expected 64 chars maximum, counted:  " + lw);
                UpgradeVerdict(Verdict.Error);
            }

            IO_Instrument.ScpiCommand("COM:OWIRE:WRITE " + wdata); // write data on 1-wire

            string rdata = IO_Instrument.ScpiQuery<string>("COM:OWIRE:READ? 1");  // readback data from 1-wire
            Log.Info("1-Wire readback: " + rdata);
            string rdatas = rdata.Replace("[ ", "").Replace(" ]", "");   // remove bracket from answer

            if (wdata.Equals(rdatas, StringComparison.Ordinal)) // check if write = readback
            {
                UpgradeVerdict(Verdict.Pass);
                return;
            }
            else
            {
                Log.Error("1-Wire Write Info and Readback Info are different: WRITE: " + wdata + " , ====> READ: " + rdatas);
                UpgradeVerdict(Verdict.Error);
            }
        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}

