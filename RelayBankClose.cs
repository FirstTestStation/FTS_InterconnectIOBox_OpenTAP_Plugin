using OpenTap;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using static InterconnectIOBox.Bank;
using static InterconnectIOBox.SingleRelay;

namespace InterconnectIOBox

{
    public abstract class Bank : TestStep
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

        [Display("CH0", Order: 2, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH0 { get; set; } = RelayState._;

        [Display("CH1", Order: 2, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH1 { get; set; } = RelayState._;

        [Display("CH2", Order: 2, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH2 { get; set; } = RelayState._;

        [Display("CH3", Order: 2, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH3 { get; set; } = RelayState._;

        [Display("CH4", Order: 2, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH4 { get; set; } = RelayState._;

        [Display("CH5", Order: 2, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH5 { get; set; } = RelayState._;

        [Display("CH6", Order: 2, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH6 { get; set; } = RelayState._;

        [Display("CH7", Order: 2, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayState CH7 { get; set; } = RelayState._;

        public enum RelayCom
        {
            _,
            Normal,    // Send command to close the relay
            Reverse      // Send command to open the relay
        }

        [Display("COM", Order: 3, Group: "Channels", Description: "Set the relay condition to No Change (_), Close or Open.")]
        public RelayCom COM { get; set; } = RelayCom._;

        #endregion
    }

    [Display(Groups: new[] { "InterconnectIO", "Route" }, Name: "Single Bank Multiple Relay Close or Open", Description: "Open or close one or multiple routes to a single Bank")]
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
            }
        }

        

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }

}
