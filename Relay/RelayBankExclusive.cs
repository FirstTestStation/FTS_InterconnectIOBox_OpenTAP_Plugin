using InterconnectIOBox.Analysis;
using InterconnectIOBox.Instruments;
using OpenTap;
using System;
using System.Collections.Generic;
using System.Linq;
using static InterconnectIOBox.Relay.Bank;

namespace InterconnectIOBox.Relay
{
    public abstract class EBank : ResultTestStep
    {
        #region Settings
        public InterconnectIO IO_Instrument { get; set; }
        public enum RelaySelection
        {
            _ = -1,
            CH0 = 0,
            CH1 = 1,
            CH2 = 2,
            CH3 = 3,
            CH4 = 4,
            CH5 = 5,
            CH6 = 6,
            CH7 = 7,
            CH8 = 8,
            CH9 = 9,
            CH10 = 10,
            CH11 = 11,
            CH12 = 12,
            CH13 = 13,
            CH14 = 14,
            CH15 = 15,
        }
        [Display("Channel", Order: 1, Group: "RELAY BANK1", Collapsed: true, Description: "Select Relay to close on BANK1 (CH8-CH15 will close the Reverse Relay)")]
        public RelaySelection CBank1 { get; set; } = RelaySelection._;
        [Display("Channel", Order: 2, Group: "RELAY BANK2", Collapsed: true, Description: "Select Relay on BANK2 (CH8-CH15 will close the Reverse Relay)")]
        public RelaySelection CBank2 { get; set; } = RelaySelection._;

        [Display("Channel", Order: 3, Group: "RELAY BANK3", Collapsed: true, Description: "Select Relay on BANK3 (CH8-CH15 will close the Reverse Relay)")]
        public RelaySelection CBank3 { get; set; } = RelaySelection._;
        [Display("Channel", Order: 4, Group: "RELAY BANK4", Collapsed: true, Description: "Select Relay on BANK4 (CH8-CH15 will close the Reverse Relay)")]
        public RelaySelection CBank4 { get; set; } = RelaySelection._;

        [Display("Verify Route State", Order: 5, Group: "Options", Description: "If checked, read back the closed relay state after writing and compare against the requested channel selection.")]
        public bool VerifyRoute { get; set; } = false;

        [Display("Publish Results", Order: 5.1, Group: "Options", Description: "If checked, publish the read-back verification results. If unchecked, verification still runs and affects verdict, but no result is published. Has no effect if 'Verify Route State' is unchecked.")]
        public bool PublishResults { get; set; } = true;
        #endregion
    }
    [Display(Groups: new[] { "FTS_Interconnect", "Route" }, Name: "Exclusive Mode: Close Relay by Bank", Description: "Closes one relay per bank in Exclusive mode. " +
      "Any previously closed relay on the selected bank will be opened before closing the new relay. " +
       "Selecting CH8-CH15 activates the Reverse Relay, connecting the Low Side input to COM High Side.")]
    public class ExBank : EBank
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion
        public ExBank()
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
            List<int> relays = new List<int>();

            if ((int)CBank1 >= 0) relays.Add(100 + (int)CBank1);
            if ((int)CBank2 >= 0) relays.Add(200 + (int)CBank2);
            if ((int)CBank3 >= 0) relays.Add(300 + (int)CBank3);
            if ((int)CBank4 >= 0) relays.Add(400 + (int)CBank4);

            if (!relays.Any())
            {
                Log.Error("Error: No channel has been selected on any bank");
                return;
            }

            string Command = $"ROUTE:CLOSE:EXCLUSIVE (@{string.Join(",", relays)})";
            Log.Info($"Sending SCPI command: {Command}");
            IO_Instrument.ScpiCommand(Command); // Send SCPI command to close the relay

            UpgradeVerdict(Verdict.Pass);

            if (VerifyRoute)
            {
                VerifyClosedRelays(relays);
            }
        }

        /// <summary>
        /// Reads back the state of each closed relay via ROUTe:CHANnel:STATe? and confirms
        /// it is closed (1). Compares each relay individually against the requested selection.
        /// </summary>
        private void VerifyClosedRelays(List<int> relays)
        {
            string Command = $"ROUTE:CHANNEL:STATE? (@{string.Join(",", relays)})";
            string response = IO_Instrument.ScpiQuery<string>(Command);
            Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

            string[] values = response.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (values.Length != relays.Count)
            {
                Log.Error($"Invalid SCPI response for channel state: {response}. Expected {relays.Count} values, got {values.Length}");
                UpgradeVerdict(Verdict.Fail);
                return;
            }

            for (int i = 0; i < relays.Count; i++)
            {
                int relayNumber = relays[i];

                if (!int.TryParse(values[i], out int actualState))
                {
                    Log.Error($"Invalid SCPI response value for relay {relayNumber}: {values[i]}");
                    UpgradeVerdict(Verdict.Fail);
                    continue;
                }

                string test = actualState == 1 ? "PASS" : "FAIL";

                if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                else
                {
                    Log.Error($"Relay {relayNumber} expected to be closed but read state: {actualState}");
                    UpgradeVerdict(Verdict.Fail);
                }

                if (PublishResults)
                {
                    TestResult<double> result = new TestResult<double>
                    {
                        ParamName = $"Relay {relayNumber} State",
                        StepName = Name,
                        Value = actualState,
                        Verdict = test,
                        Units = "read",
                        LowerLimit = 1,
                        UpperLimit = 1
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