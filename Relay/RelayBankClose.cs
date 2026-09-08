using InterconnectIOBox.Analysis;
using InterconnectIOBox.Instruments;
using OpenTap;
using System;
using System.Collections.Generic;
using System.Linq;
using static InterconnectIOBox.Relay.Bank;

namespace InterconnectIOBox.Relay

{
    public abstract class Bank : ResultTestStep
    {
        #region Settings
        public InterconnectIO IO_Instrument { get; set; }

        public enum BankSelection
        {
            BANK1 = 10,
            BANK2 = 20,
            BANK3 = 30,
            BANK4 = 40
        }
        [Display("Bank", Order: 1, Group: "Relay Bank", Description: "Select on which Relay bank the action will be performed")]
        public BankSelection SelectedBank { get; set; }

        public enum RelayState
        {
            _,
            Close,    // Send command to close the relay
            Open      // Send command to open the relay
        }

        [Display("CH0", Order: 2.0, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH0 { get; set; } = RelayState._;

        [Display("CH1", Order: 2.1, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH1 { get; set; } = RelayState._;

        [Display("CH2", Order: 2.2, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH2 { get; set; } = RelayState._;

        [Display("CH3", Order: 2.3, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH3 { get; set; } = RelayState._;

        [Display("CH4", Order: 2.4, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH4 { get; set; } = RelayState._;

        [Display("CH5", Order: 2.5, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH5 { get; set; } = RelayState._;

        [Display("CH6", Order: 2.6, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH6 { get; set; } = RelayState._;

        [Display("CH7", Order: 2.7, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH7 { get; set; } = RelayState._;

        public enum RelayCom
        {
            _,
            Normal,    // Send command to close the relay
            Reverse      // Send command to open the relay
        }

        [Display("COM", Order: 3, Group: "Channels", Description: "Set the relay COM condition to No Change (_), Normal or Reverse.")]
        public RelayCom COM { get; set; } = RelayCom._;

        [Display("Verify Route State", Order: 4, Group: "Options", Description: "If checked, read back the channel and/or reverse relay state after writing and compare against the requested configuration.")]
        public bool VerifyRoute { get; set; } = false;

        [Display("Publish Results", Order: 4.1, Group: "Options", Description: "If checked, publish the read-back verification results. If unchecked, verification still runs and affects verdict, but no result is published. Has no effect if 'Verify Route State' is unchecked.")]
        public bool PublishResults { get; set; } = true;

        #endregion
    }

    [Display(Groups: new[] { "FTS_Interconnect", "Route" }, Name: "Single Bank Multiple Relay Close or Open", Description: "Open or close one or multiple routes to a single Bank")]
    public class RBank : Bank
    {
        #region Settings

        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        // Loop through properties and build SCPI commands
        public (string openCommand, string closeCommand) GetSCPICommands()
        {
            List<int> openRelays = new List<int>();
            List<int> closeRelays = new List<int>();

            // Get all CHx properties dynamically
            var properties = GetType().GetProperties()
                                .Where(p => p.Name.StartsWith("CH") && p.PropertyType == typeof(RelayState));

            foreach (var prop in properties)
            {
                int channelNumber = int.Parse(prop.Name.Substring(2));  // Extract the number from CHx
                int relayNumber = (int)SelectedBank + channelNumber;

                RelayState state = (RelayState)prop.GetValue(this);

                if (state == RelayState.Open)
                    openRelays.Add(relayNumber);
                else if (state == RelayState.Close)
                    closeRelays.Add(relayNumber);
            }

            string openSCPI = openRelays.Any() ? $"ROUTE:OPEN (@{string.Join(",", openRelays)})" : "";
            string closeSCPI = closeRelays.Any() ? $"ROUTE:CLOSE (@{string.Join(",", closeRelays)})" : "";

            return (openSCPI, closeSCPI);
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        public override void Run()
        {
            Log.Info("Executing relay actions...");
            string comCommand = "";

            // Get SCPI commands (the method returns a tuple of two strings)
            var commands = GetSCPICommands();
            string openCommand = commands.openCommand;
            string closeCommand = commands.closeCommand;

            if (SelectedBank == 0)
            {
                Log.Error($"Error: No Relay Bank has been selected");
            }
            else
            {
                if (!string.IsNullOrEmpty(openCommand))
                {
                    Log.Info($"Sending SCPI command: {openCommand}");
                    IO_Instrument.ScpiCommand(openCommand); // Send SCPI command to open the relay
                }

                if (!string.IsNullOrEmpty(closeCommand))
                {
                    Log.Info($"Sending SCPI command: {closeCommand}");
                    IO_Instrument.ScpiCommand(closeCommand); // Send SCPI command to close the relay
                }

                // Send command related to relay COM
                if (COM != RelayCom._)
                {
                    if (COM == RelayCom.Normal) { comCommand = $"ROUTE:OPEN:REV {SelectedBank}"; }
                    if (COM == RelayCom.Reverse) { comCommand = $"ROUTE:CLOSE:REV {SelectedBank}"; }
                    Log.Info($"Sending SCPI command: {comCommand}");
                    IO_Instrument.ScpiCommand(comCommand); // Send SCPI command to COM relay
                }

                UpgradeVerdict(Verdict.Pass);

                // Verify the resulting relay states against the requested configuration
                if (VerifyRoute)
                {
                    VerifyChannelStates();

                    if (COM != RelayCom._)
                    {
                        VerifyReverseState();
                    }
                }
            }
        }

        /// <summary>
        /// Reads back the state of each explicitly configured channel (Close/Open) via
        /// ROUTe:CHANnel:STATe? and compares it against the requested state.
        /// Channels left at "_" (no change) are skipped since their expected state is unknown.
        /// </summary>
        private void VerifyChannelStates()
        {
            var properties = GetType().GetProperties()
                                .Where(p => p.Name.StartsWith("CH") && p.PropertyType == typeof(RelayState));

            // Build the list of configured channels (relay number, expected state, property name)
            List<(int RelayNumber, int ExpectedState, string PropName)> configured = new List<(int, int, string)>();

            foreach (var prop in properties)
            {
                RelayState state = (RelayState)prop.GetValue(this);

                // Skip channels left at "No Change"; their expected state is unknown
                if (state == RelayState._) continue;

                int channelNumber = int.Parse(prop.Name.Substring(2));
                int relayNumber = (int)SelectedBank + channelNumber;
                int expectedState = state == RelayState.Close ? 1 : 0;

                configured.Add((relayNumber, expectedState, prop.Name));
            }

            if (!configured.Any()) return;

            string chList = string.Join(",", configured.Select(c => c.RelayNumber));
            string Command = $"ROUTE:CHANNEL:STATE? (@{chList})";
            string response = IO_Instrument.ScpiQuery<string>(Command);
            Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

            string[] values = response.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (values.Length != configured.Count)
            {
                Log.Error($"Invalid SCPI response for channel state: {response}. Expected {configured.Count} values, got {values.Length}");
                UpgradeVerdict(Verdict.Fail);
                return;
            }

            for (int i = 0; i < configured.Count; i++)
            {
                var (relayNumber, expectedState, propName) = configured[i];

                if (!int.TryParse(values[i], out int actualState))
                {
                    Log.Error($"Invalid SCPI response value for {propName}: {values[i]}");
                    UpgradeVerdict(Verdict.Fail);
                    continue;
                }

                string test = actualState == expectedState ? "PASS" : "FAIL";

                if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                else
                {
                    Log.Error($"{SelectedBank} {propName} state mismatch. Expected: {expectedState}, Actual: {actualState}");
                    UpgradeVerdict(Verdict.Fail);
                }

                if (PublishResults)
                {
                    TestResult<double> result = new TestResult<double>
                    {
                        ParamName = $"{SelectedBank} {propName} State",
                        StepName = Name,
                        Value = actualState,
                        Verdict = test,
                        Units = "read",
                        LowerLimit = expectedState,
                        UpperLimit = expectedState
                    };

                    PublishResult(result);
                }
            }
        }

        /// <summary>
        /// Reads back the contact side of the reverse (COM) relay via ROUTe:REV:STATe? and
        /// compares it against the requested COM configuration.
        /// Normal expects LOW side (0), Reverse expects HIGH side (1).
        /// </summary>
        private void VerifyReverseState()
        {
            string Command = $"ROUTE:REV:STATE? {SelectedBank}";
            string response = IO_Instrument.ScpiQuery<string>(Command);
            Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

            if (!int.TryParse(response, out int actualState))
            {
                Log.Error($"Invalid SCPI response for reverse relay state: {response}");
                UpgradeVerdict(Verdict.Fail);
                return;
            }

            int expectedState = COM == RelayCom.Reverse ? 1 : 0;
            string test = actualState == expectedState ? "PASS" : "FAIL";

            if (test == "PASS") UpgradeVerdict(Verdict.Pass);
            else
            {
                Log.Error($"{SelectedBank} COM state mismatch. Expected: {expectedState}, Actual: {actualState}");
                UpgradeVerdict(Verdict.Fail);
            }

            if (PublishResults)
            {
                TestResult<double> result = new TestResult<double>
                {
                    ParamName = $"{SelectedBank} COM State",
                    StepName = Name,
                    Value = actualState,
                    Verdict = test,
                    Units = "read",
                    LowerLimit = expectedState,
                    UpperLimit = expectedState
                };

                PublishResult(result);
            }
        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }

}