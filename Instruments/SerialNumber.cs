using InterconnectIOBox.Analysis;
using OpenTap;
using System.Text.RegularExpressions;

namespace InterconnectIOBox.Instruments
{

    // Simple dialog class for entering the Serial Number
    public class EnterSNDialog
    {
        [Display("Serial Number", Description: "Enter or scan the DUT serial number.")]
        public string SerialNumber { get; set; }
    }

    [Display("Get Serial Number", Groups: new[] { "FTS_Interconnect", "Fixture/DUT" },
        Description: "Prompts the operator for the DUT serial number if it is not already set.")]
    public class GetSerialNumberStep : ResultTestStep
    {

        [Display("Regex Validation", Order: 1.2, Group: "SN Validation", Description: "The regular expression pattern used to validate the serial number format (e.g., ^[A-Z]{3}-\\d{1,5}$).")]
        public string ExpectedRegex { get; set; } = @"^[\w\d-]+$";

        public override void Run()
        {
            // Note: RequiresDut defaults to true for ResultTestStep, so PrePlanRun
            // already guarantees Dut is non-null here.
            string SNsource = "Input";

            if (!Dut.Dut1Wire)
            {
                // 1-Wire is disabled → clear serial number, force manual entry below.
                Dut.SerialNumber = string.Empty;
            }
            else if (!string.IsNullOrWhiteSpace(Dut.SerialNumber))
            {
                // 1-Wire is enabled and a serial number was already read from it.
                Log.Info($"Serial Number already provided: {Dut.SerialNumber}");
                SNsource = "Extern";
            }

            if (string.IsNullOrWhiteSpace(Dut.SerialNumber))
            {
                var snDialog = new EnterSNDialog();
                UserInput.Request(snDialog); // Opens GUI dialog or CLI prompt
                Dut.SerialNumber = snDialog.SerialNumber;
            }

            string test;

            if (string.IsNullOrWhiteSpace(Dut.SerialNumber))
            {
                // The user cancelled the dialog or entered an empty string.
                Log.Error("No Serial Number entered.");
                UpgradeVerdict(Verdict.Error);
                test = "ERROR";
            }
            else if (!Regex.IsMatch(Dut.SerialNumber, ExpectedRegex))
            {
                Log.Error($"Serial Number '{Dut.SerialNumber}' does not match expected format: {ExpectedRegex}");
                UpgradeVerdict(Verdict.Fail);
                test = "FAIL";
            }
            else
            {
                Log.Info($"[META]:SerialNumber:{Dut.SerialNumber}");
                PlanRun.Parameters["SerialNumber"] = Dut.SerialNumber;
                UpgradeVerdict(Verdict.Pass);
                test = "PASS";
            }

            // Publish the result record, whether the step passed, failed, or errored,
            // so there's always a trace of what serial number (if any) was captured.
            var result = new TestResult<string>
            {
                ParamName = $"SN Check {SNsource}",
                StepName = Name,
                Value = Dut.SerialNumber ?? "",
                LowerLimit = ExpectedRegex,
                UpperLimit = "",
                Verdict = test,
                Units = "Check"
            };

            PublishResult(result);
        }
    }

    [Display("Clear Serial Number", Groups: new[] { "FTS_Interconnect", "Fixture/DUT" },
        Description: "Clears the DUT serial number to request a new one on the next run.")]
    public class ClearSerialNumberStep : TestStep
    {
        [Display("DUT")]
        public FTS_DUT Dut { get; set; }

        public override void Run()
        {
            if (Dut != null)
            {
                Dut.SerialNumber = null;
                Log.Info("Serial Number cleared.");
            }
            else
            {
                Log.Warning("Clear Serial Number: DUT reference (FTS_DUT) is null. Nothing to clear.");
            }

            UpgradeVerdict(Verdict.Pass);
        }
    }
}