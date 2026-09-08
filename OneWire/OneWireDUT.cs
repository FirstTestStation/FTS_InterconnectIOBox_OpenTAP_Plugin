using InterconnectIOBox.Analysis;
using InterconnectIOBox.Instruments;
using OpenTap;
using System;
using System.Linq;

namespace InterconnectIOBox.OneWire // DUT Validation
{
    public abstract class OneWireReadWrite : ResultTestStep
    {
        #region Settings
        public InterconnectIO IO_Instrument { get; set; }
        #endregion


        /// <summary>
        /// Checks on the DUT whether 1-Wire devices on connectors J1 and/or J2 are enabled.
        /// </summary>
        /// <returns>
        /// A tuple containing: the number of 1-Wire devices enabled on the DUT (nbWire),
        /// whether 1-Wire on connector J1 is enabled (j1), and whether 1-Wire on connector J2 is enabled (j2).
        /// </returns>
        protected (int nbWire, bool j1, bool j2) NumberofOneWires()
        {
            int nbWire = 0;
            bool j1 = false;
            bool j2 = false;

            if (Dut.Enable1WireJ1) { nbWire++; j1 = true; }
            if (Dut.Enable1WireJ2) { nbWire++; j2 = true; }

            return (nbWire, j1, j2);
        }
    }

    [Display(Groups: new[] { "FTS_Interconnect", "1-Wire" }, Name: "1-Wire Validation", Description: "Validates the part number of the fixture or DUT by reading data from its 1-Wire " +
        "device. The compare value is defined on the DUT (Step E). If the test FAILS, the test plan must be aborted to prevent potential damage to an unknown fixture. " +
        "In the test step, enable the break condition for when the test step encounters an error or failure.")]
    public class OneWireDUTRead : OneWireReadWrite
    {
        public OneWireDUTRead()
        {
            // ToDo: Set default values for properties / settings.
        }

        /// <summary>
        /// Sends a SCPI command to read the 1-Wire devices on the DUT.
        /// </summary>
        /// <param name="qty">Number of 1-Wire devices to read.</param>
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

            if (nbWire > 0) // Check if there is a 1-Wire device to check
            {
                sdata = ReadOneWires(nbWire); // read 1-Wire value(s)
                string[] field = sdata.Split(new string[] { "," }, StringSplitOptions.None);
                WireSerialNumber = field[3].Trim(); // get serial number from 1-Wire data
                WirePartNumber = field[2].Trim(); // get part number from 1-Wire data
                WireName = field[1].Trim(); // get name from 1-Wire data


                Log.Info("1-Wire Data: " + sdata);

                if (sdata.Length < 16) // if answer is not valid
                {
                    sdata = ReadOneWires(1); // retry read with a single 1-Wire value
                    if (sdata.Length < 16)   // if no 1-wire detected
                    {
                        Log.Info("No 1-Wire device was detected. Check DUT setup.");
                        UpgradeVerdict(Verdict.Error);
                    }
                    else
                    {
                        Log.Info("1-Wire data is too short, retrying with number of 1-Wire = 1, value: " + sdata);
                    }
                }

                if (nbWire == 1)    // if only a single 1-wire device
                {
                    if (j1) { scid = "J1"; } // if 1-Wire on J1 is enabled
                    if (j2) { scid = "J2"; }
                    valid = sdata.Contains(OwireID); // check if 1-Wire ID is valid
                    vconn = sdata.Contains(scid); // check if connector ID is valid
                    if (valid && vconn)
                    {
                        Log.Info("1-Wire fixture part number is valid for: " + scid);
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
                            Log.Error("1-Wire fixture part number is not valid for: " + scid + " - does not contain ID: " + OwireID);
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
                    vconn = parts[0].Contains(scid);
                    if (vconn)
                    {   // if J1 found
                        valid = parts[0].Contains(OwireID);
                        if (valid)
                        {
                            Log.Info("1-Wire part number and connector ID is valid for: " + scid);
                            verdict1 = true;
                        }
                        else
                        {
                            Log.Error("1-Wire part number is not valid for: " + scid + " - does not contain ID: " + OwireID);
                            verdict1 = false;
                        }
                    }
                    else
                    {
                        scid = "J2";
                        vconn = parts[0].Contains(scid);
                        if (vconn)
                        {   // if J2 found
                            valid = parts[0].Contains(OwireID);
                            if (valid)
                            {
                                Log.Info("1-Wire part number and connector ID is valid for: " + scid);
                                verdict1 = true;
                            }
                            else
                            {
                                Log.Error("1-Wire part number is not valid for: " + scid + " - does not contain ID: " + OwireID);
                                verdict1 = false;
                            }
                        }
                        else
                        {
                            Log.Error("1-Wire connector ID not found in first segment (expected J1 or J2).");
                        }
                    }

                    scid = "J1";
                    vconn = parts[1].Contains(scid);
                    if (vconn)
                    {   // if J1 found
                        valid = parts[1].Contains(OwireID);
                        if (valid)
                        {
                            Log.Info("1-Wire part number is valid for: " + scid);
                            verdict2 = true;
                        }
                        else
                        {
                            Log.Error("1-Wire part number is not valid for: " + scid + " - does not contain ID: " + OwireID);
                            verdict2 = false;
                        }
                    }
                    else
                    {
                        scid = "J2";
                        vconn = parts[1].Contains(scid);
                        if (vconn)
                        {   // if J2 found
                            valid = parts[1].Contains(OwireID);
                            if (valid)
                            {
                                Log.Info("1-Wire part number is valid for: " + scid);
                                verdict2 = true;
                            }
                            else
                            {
                                Log.Error("1-Wire part number is not valid for: " + scid + " - does not contain ID: " + OwireID);
                                verdict2 = false;
                            }
                        }
                        else
                        {
                            Log.Error("1-Wire connector ID not found in second segment (expected J1 or J2).");
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
            else // if nbWire == 0
            {
                Log.Warning("No 1-Wire device to check.");
                UpgradeVerdict(Verdict.Fail);
                nbWire = 0;
            }

            if (nbWire > 0) // if a 1-Wire device is present, set DUT or fixture information from 1-Wire data
            {
                if (Dut.Dut1Wire)
                {
                    Dut.SerialNumber = WireSerialNumber;  // Set SerialNumber from 1-Wire ID on DUT
                    Dut.PartNumber = WirePartNumber;
                    Dut.ProductName = WireName;
                    ReportValue = Dut.PartNumber;
                    Log.Info($"[META]:SerialNumber:{Dut.SerialNumber ?? ""}");
                    Log.Info($"[META]:ProductName:{Dut.ProductName ?? ""}");
                    Log.Info($"[META]:PartNumber:{Dut.PartNumber ?? ""}");
                }
                else
                {
                    Dut.FixtSerial = WireSerialNumber;  // Set SerialNumber from 1-Wire ID on fixture
                    Dut.FixtNumber = WirePartNumber;
                    Dut.FixtName = WireName;
                    ReportValue = Dut.FixtNumber;
                    Log.Info($"[META]:FixtureName:{Dut.FixtName ?? ""}");
                    Log.Info($"[META]:FixtureNumber:{Dut.FixtNumber ?? ""}");
                    Log.Info($"[META]:FixtureSerial:{Dut.FixtSerial ?? ""}");
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


    [Display(Groups: new[] { "FTS_Interconnect", "1-Wire" }, Name: "1-Wire Write Data", Description: "Writes data (part number, serial number, and connector reference designator) to a 1-Wire DUT device.")]

    public class OneWireDUTWrite : OneWireReadWrite
    {

        #region Settings
        [Display("Write Data on 1-Wire J1", Order: 1, Group: "1-Wire Device", Description: "Write data on the 1-Wire device connected to J1.")]
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

        [Display("Write Data on 1-Wire J2", Order: 2, Group: "1-Wire Device", Description: "Write data on the 1-Wire device connected to J2.")]
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



        [Display("Fixture Name", Group: "Fixture Data Write Field", Description: "Product name to write to the 1-Wire device, used during fixture validation.")]
        public string ProductName { get; set; }

        [Display("Fixture Part Number", Group: "Fixture Data Write Field", Description: "Fixture part number to write to the 1-Wire device, used during fixture validation.")]
        public string PartNumber { get; set; }

        [Display("Fixture Serial Number", Group: "Fixture Data Write Field", Description: "Fixture serial number to write to the 1-Wire device, read during fixture validation.")]
        public string SerialNumber { get; set; }

        #endregion


        public OneWireDUTWrite()
        {
            // ToDo: Set default values for properties / settings.
            ProductName = "FTS";
            PartNumber = "550-XXXX-XXX";
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
        /// Sends a SCPI command to check the 1-Wire devices on the fixture (reads the 1-Wire identification number).
        /// </summary>
        /// <param name="qty">Number of 1-Wire devices to read.</param>
        /// <returns>The data read from the 1-Wire interface.</returns>
        public string CheckOneWires(int qty)
        {
            string command = $"COM:OWIRE:CHECK? {qty}";
            Log.Info($"Sending SCPI command: {command}");

            // Use ScpiQuery to read back from the device.
            string response = IO_Instrument.ScpiQuery<string>(command);

            Log.Info($"1-Wire received response: {response}");
            return response;
        }

        public override void Run()
        {
            string scid;
            string test;

            string sdata = CheckOneWires(1); // read 1-Wire value
            int count = sdata.Count(c => c == 'O');

            if (count == 0)
            {
                Log.Warning("No 1-Wire device detected on the fixture. Verify that a single 1-Wire device is present on J1 or J2.");
                UpgradeVerdict(Verdict.Error);
                return;
            }

            if (count > 1)
            {
                Log.Warning("Only a single 1-Wire device can be programmed at a time. Detected: " + count);
                UpgradeVerdict(Verdict.Fail);
                return;
            }

            if (sdata.Length > 64)
            {
                Log.Error("1-Wire read info is too long. Max 64 chars, got: " + sdata.Length);
                UpgradeVerdict(Verdict.Error);
                return;
            }

            // Extract OWID
            string[] parts = sdata.Split(new[] { ":" }, StringSplitOptions.None);
            string owid = parts[1].Trim().Replace("\"", "");

            if (owid.Length != 16)
            {
                Log.Info("Invalid 1-Wire ID length (expected 16 digits): " + owid.Length);
                UpgradeVerdict(Verdict.Error);
                return;
            }

            scid = EWireJ1 ? "J1" : "J2";

            // Build write string
            string wdata = "\"" + owid + ", " + ProductName + ", " + PartNumber + ", " + SerialNumber + ", " + scid + "\"";
            Log.Info("1-Wire write string: " + wdata);

            if (wdata.Length > 64)
            {
                Log.Error("1-Wire write info too long. Max 64 chars, got: " + wdata.Length);
                UpgradeVerdict(Verdict.Error);
                return;
            }

            // Send write
            IO_Instrument.ScpiCommand("COM:OWIRE:WRITE " + wdata);

            // Read back
            string rdata = IO_Instrument.ScpiQuery<string>("COM:OWIRE:READ? 1");
            Log.Info("1-Wire raw readback: " + rdata);

            // Normalize readback
            string rdatas = rdata
               .Replace("[", "")
               .Replace("]", "")
               .Replace("\"", "")
               .Replace("\r", "")
               .Replace("\n", "")
               .Replace("\0", "")
               .Trim();

            // Normalize write
            string wdatas = wdata
                .Replace("\"", "")
                .Replace("\0", "")
                .Trim();

            // Compare
            if (string.Equals(wdatas, rdatas, StringComparison.Ordinal))
            {
                Log.Info("1-Wire write/read match.");
                UpgradeVerdict(Verdict.Pass);
                test = "PASS";
            }
            else
            {
                Log.Error($"1-Wire mismatch: WRITE: \"{wdatas}\" !== READ: \"{rdatas}\"");

                // Show hex to see all characters, including invisible ones
                Log.Info($"WRITE data hex: {BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(wdatas))}");
                Log.Info($"READ  data hex: {BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(rdatas))}");

                UpgradeVerdict(Verdict.Fail);
                test = "FAIL";
            }

            // Publish final result
            var result = new TestResult<string>
            {
                ParamName = "1-Wire Write/Read",
                StepName = Name,
                Value = rdatas,
                LowerLimit = wdatas,
                UpperLimit = wdatas,
                Verdict = test,
                Units = "strcmp"
            };

            PublishResult(result);
        }


        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}