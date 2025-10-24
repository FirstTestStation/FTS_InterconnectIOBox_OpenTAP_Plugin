using OpenTap;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace InterconnectIOBox
{
  
    // Simple dialog class for entering Serial Number
    public class EnterSNDialog
    {

        [Display("Serial Number", Description: "Enter or scan the DUT Serial Number.")]
        public string SerialNumber { get; set; }

    }

    [Display("Get Serial Number", Groups: new[] { "InterconnectIO", "Fixture/DUT" },
        Description: "Prompts operator for DUT Serial Number if not already set.")]
    public class GetSerialNumberStep : ResultTestStep
    {
       

        [Display("Regex Validation", Order: 1.2, Group: "SN Validation", Description: "The regular expression pattern used to validate the serial number format (e.g., ^[A-Z]{3}-\\d{1,5}$).")]
        public string ExpectedRegex { get; set; } = @"^[\w\d-]+$"; // Corrected the string literal for Regex

        public override void Run()
        {

            string SNsource = "Input";

            if (Dut != null)
            {
                if (!Dut.Dut1Wire)
                {
                    // 1-Wire is disabled → clear serial number
                    Dut.SerialNumber = string.Empty;
                }
                else if (!string.IsNullOrWhiteSpace(Dut.SerialNumber))
                {
                    // 1-Wire is enabled and serial number exists
                    Log.Info($"Serial Number already provided: {Dut.SerialNumber}");
                    SNsource = "Extern";
                    UpgradeVerdict(Verdict.Pass);

                }
            }


            if (string.IsNullOrWhiteSpace(Dut.SerialNumber))
            {

                var snDialog = new EnterSNDialog();
                UserInput.Request(snDialog); // Opens GUI dialog or CLI prompt
                Dut.SerialNumber = snDialog.SerialNumber;
            }

            if (!string.IsNullOrWhiteSpace(Dut.SerialNumber))
            {
                // Optional: Add Regex validation here before storing
                 if (!System.Text.RegularExpressions.Regex.IsMatch(Dut.SerialNumber, ExpectedRegex))
                {
                     UpgradeVerdict(Verdict.Fail);
                    throw new Exception($"Serial Number '{Dut.SerialNumber}' does not match expected format: {ExpectedRegex}");
                }

                if (Dut != null)
                {
                    Log.Info($"Serial Number set: {Dut.SerialNumber}");
                    UpgradeVerdict(Verdict.Pass);
                }
                else
                {
                    // This case handles if a SN was entered but the DUT reference is null
                    Log.Warning("Serial Number entered but DUT is null. Cannot store SN.");
                    UpgradeVerdict(Verdict.Inconclusive);
                }
            }
            else
            {
                // This happens if the user clicks 'Cancel' or enters an empty string.
                UpgradeVerdict(Verdict.Error);
                throw new Exception("No Serial Number entered.");
            }

            string currentVerdictString = this.Verdict.ToString();

            // Publish the result record
            var result = new TestResult<string>
            {
                ParamName = $"SN Check {SNsource}" ,
                StepName = Name,
                Value = Dut.SerialNumber,
                LowerLimit = ExpectedRegex,
                UpperLimit = "",
                Verdict = currentVerdictString,
                Units = "Check"
            };


            // This line publishes the result to OpenTAP's log and database
            PublishResult(result);
        }
    }
    [Display("Clear Serial Number", Groups: new[] { "InterconnectIO", "Fixture/DUT" },
        Description: "Clears DUT Serial Number to request a new one in the next run.")]
    public class ClearSerialNumberStep : TestStep
    {
        [Display("DUT")]
        public FTS_DUT Dut { get; set; }

        public override void Run()
        {
            if (Dut != null)
            {
                Dut.SerialNumber = null;
                Log.Info("Serial Number cleared. Next DUT will ask for a new one.");
            }
            else
            {
                Log.Warning("Clear Serial Number: DUT reference (FTS_DUT) is null. Nothing to clear.");
            }


            UpgradeVerdict(Verdict.Pass);
        }
    }


}