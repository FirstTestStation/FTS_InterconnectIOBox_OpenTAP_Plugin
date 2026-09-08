using InterconnectIOBox.Analysis;
using InterconnectIOBox.Instruments;
using OpenTap;
using System.Collections.Generic;
using System.Linq;

namespace InterconnectIOBox.Relay
{
    [Display(Groups: new[] { "FTS_Interconnect", "Route" }, Name: "Open Single Bank", Description: "Open single relay Bank or all the banks")]
    public class OpenBank : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion
        public InterconnectIO IO_Instrument { get; set; }
        [Display("OpenBank1", Order: 1, Group: "Relay Banks", Description: "Bank selected will open all relays.")]
        public bool OpenBank1 { get; set; }
        [Display("OpenBank2", Order: 2, Group: "Relay Banks", Description: "Bank selected will open all relays.")]
        public bool OpenBank2 { get; set; }
        [Display("OpenBank3", Order: 3, Group: "Relay Banks", Description: "Bank selected will open all relays.")]
        public bool OpenBank3 { get; set; }
        [Display("OpenBank4", Order: 4, Group: "Relay Banks", Description: "Bank selected will open all relays.")]
        public bool OpenBank4 { get; set; }

        [Display("Verify Route State", Order: 5, Group: "Options", Description: "If checked, read back each selected bank state after opening and confirm all relays are open.")]
        public bool VerifyRoute { get; set; } = false;

        [Display("Publish Results", Order: 5.1, Group: "Options", Description: "If checked, publish the read-back verification results. If unchecked, verification still runs and affects verdict, but no result is published. Has no effect if 'Verify Route State' is unchecked.")]
        public bool PublishResults { get; set; } = true;

        public OpenBank()
        {
            // ToDo: Set default values for properties / settings.
        }
        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }
        public override void Run()
        {
            List<string> banks = new List<string>();

            if (OpenBank1) banks.Add("BANK1");
            if (OpenBank2) banks.Add("BANK2");
            if (OpenBank3) banks.Add("BANK3");
            if (OpenBank4) banks.Add("BANK4");

            if (banks.Any())
            {
                string Command = $"ROUTE:OPEN:ALL {string.Join(",", banks)}";
                Log.Info($"Sending SCPI command: {Command}");
                IO_Instrument.ScpiCommand(Command); // Send SCPI command to open the relays
                UpgradeVerdict(Verdict.Pass);

                if (VerifyRoute)
                {
                    VerifyBanksOpen(banks);
                }
            }
            else
            {
                Log.Warning("SCPI command not complete, no Bank selected");
                UpgradeVerdict(Verdict.NotSet);
            }

        }

        /// <summary>
        /// Reads back the state of each selected bank via ROUTe:BANK:STATe? and confirms
        /// all relays are open (value 0).
        /// </summary>
        private void VerifyBanksOpen(List<string> banks)
        {
            foreach (string bank in banks)
            {
                string Command = $"ROUTE:BANK:STATE? {bank}";
                string response = IO_Instrument.ScpiQuery<string>(Command);
                Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

                if (!int.TryParse(response, out int bankValue))
                {
                    Log.Error($"Invalid SCPI response for bank state: {response}");
                    UpgradeVerdict(Verdict.Fail);
                    continue;
                }

                string test = bankValue == 0 ? "PASS" : "FAIL";

                if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                else
                {
                    Log.Error($"{bank} expected fully open (0) but read state: {bankValue}");
                    UpgradeVerdict(Verdict.Fail);
                }

                if (PublishResults)
                {
                    TestResult<double> result = new TestResult<double>
                    {
                        ParamName = $"{bank} State",
                        StepName = Name,
                        Value = bankValue,
                        Verdict = test,
                        Units = "read",
                        LowerLimit = 0,
                        UpperLimit = 0
                    };

                    PublishResult(result);
                }
            }
        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}