using OpenTap;
using static InterconnectIOBox.Bank;
using System.Collections.Generic;
using System.Linq;
using static InterconnectIOBox.SBank;
using System.Xml.Linq;
using System;

namespace InterconnectIOBox
{
    public abstract class SBank : ResultTestStep
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

        [Display("Bank", Order: 1, Group: "Relay Bank Status", Description: "Select on which Relay bank the state need to be validated")]
        public BankSelection SelectedBank { get; set; }


        public enum RelayStatus
        {
            _,
            VerifyIsOpen,   // Check if the relay is open
            VerifyIsClosed  // Check if the relay is close
        }

        [Display("CH0", Order: 2, Group: "Channels to read Status", Description: "Select relay channel to read.")]
        public RelayStatus CH0 { get; set; } = RelayStatus._;

        [Display("CH1", Order: 2, Group: "Channels to read Status", Description: "Select relay channel to read.")]
        public RelayStatus CH1 { get; set; } = RelayStatus._;

        [Display("CH2", Order: 2, Group: "Channels to read Status", Description: "Select relay channel to read.")]
        public RelayStatus CH2 { get; set; } = RelayStatus._;

        [Display("CH3", Order: 2, Group: "Channels to read Status", Description: "Select relay channel to read.")]
        public RelayStatus CH3 { get; set; } = RelayStatus._;

        [Display("CH4", Order: 2, Group: "Channels to read Status", Description: "Select relay channel to read.")]
        public RelayStatus CH4 { get; set; } = RelayStatus._;

        [Display("CH5", Order: 2, Group: "Channels to read Status", Description: "Select relay channel to read.")]
        public RelayStatus CH5 { get; set; } = RelayStatus._;

        [Display("CH6", Order: 2, Group: "Channels to read Status", Description: "Select relay channel to read.")]
        public RelayStatus CH6 { get; set; } = RelayStatus._;

        [Display("CH7", Order: 2, Group: "Channels to read Status", Description: "Select relay channel to read.")]
        public RelayStatus CH7 { get; set; } = RelayStatus._;

        [Display("COM", Order: 3, Group: "Channels to read Status", Description: "Read Reverse relay status.")]
        public RelayStatus COM { get; set; } = RelayStatus._;


        #endregion



    }




    [Display(Groups: new[] { "InterconnectIO", "Route" }, Name: "Single Bank Multiple Relay Validation", Description: "Read Relay Channel state (1: Close, 0:Open)")]

    public class Cstatus : SBank
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        public Cstatus()
        {
            // ToDo: Set default values for properties / settings.
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        /// <summary>
        /// Validates and publishes the test result.
        /// </summary>
        void ValidateAndPublishResult(string measuredValue,string expected, BankSelection bank, string channel)
        {
            //Log.Info($"Validating result for {bank}-{channel}: {measuredValue}");

            var result = new TestResult<string>
            {
                ParamName = $"{bank}-{channel}",
                StepName = Name,
                Value = measuredValue,
                LowerLimit = expected,
                UpperLimit = expected,
                Verdict = (measuredValue == expected) ? "PASS" : "FAIL",
                Units = "relays"
            };

            PublishResult(result);
        }

        public override void Run()
        {
            List<int> StatusRelays = new List<int>();

            // Get all CHx properties dynamically
            var properties = GetType().GetProperties()
                                .Where(p => p.Name.StartsWith("CH") && p.PropertyType == typeof(RelayStatus));

            foreach (var prop in properties)
            {
                int channelNumber = int.Parse(prop.Name.Substring(2));  // Extract the number from CHx
                int relayNumber = (int)SelectedBank + channelNumber;

                RelayStatus state = (RelayStatus)prop.GetValue(this);

                if (state != RelayStatus._)
                    StatusRelays.Add(relayNumber);
            }

            if (StatusRelays.Count > 0) // if any relay selected
            {
                string command = StatusRelays.Any() ? $"ROUTE:CHANNEL:STATE? (@{string.Join(",", StatusRelays)})" : "";
                Log.Info($"Sending SCPI command: {command}");
                // Use ScpiQuery to read back from the device.
                string status = IO_Instrument.ScpiQuery<string>(command);
                // Split string by ","
                string[] parts = status.Split(new string[] { "," }, StringSplitOptions.None);


                int i = 0;
                string channel = "";
                foreach (var prop in properties)
                {
                    RelayStatus state = (RelayStatus)prop.GetValue(this);
                    channel = prop.Name;

                    switch (state)
                    {
                        case RelayStatus.VerifyIsOpen:
                            bool isOpen = (parts[i] == "0");
                            ValidateAndPublishResult(parts[i],"0", SelectedBank, channel);
                            Log.Info($"{(isOpen ? "Pass" : "Fail")} Verify Open Status for {SelectedBank}-{channel}: {parts[i]}");
                            UpgradeVerdict(isOpen ? Verdict.Pass : Verdict.Fail);
                            i++;
                            break;

                        case RelayStatus.VerifyIsClosed:
                            bool isClosed = (parts[i] == "1");
                            ValidateAndPublishResult(parts[i],"1",SelectedBank, channel);
                            Log.Info($"{(isClosed ? "Pass" : "Fail")} Verify Closed Status for {SelectedBank}-{channel}: {parts[i]}");
                            UpgradeVerdict(isClosed ? Verdict.Pass : Verdict.Fail);
                            i++;
                            break;
                    }   
                }
            }

            // Check Status of Reverse Relays
            if (COM != RelayStatus._)
            {
                string comand = $"ROUTE:REV:STATE? {SelectedBank}";
                Log.Info($"Sending SCPI command: {comand}");
                // Use ScpiQuery to read back from the device.
                string statusCOM = IO_Instrument.ScpiQuery<string>(comand);

                switch (COM)
                {
             
                    case RelayStatus.VerifyIsOpen:
                        bool isOpen = (statusCOM == "0");
                        ValidateAndPublishResult(statusCOM,"1", SelectedBank, "COM");
                        Log.Info($"{(statusCOM == "0" ? "Pass" : "Fail")} Verify Open Status for {SelectedBank}-COM: {statusCOM}");
                        UpgradeVerdict(statusCOM == "0" ? Verdict.Pass : Verdict.Fail);
                        break;

                    case RelayStatus.VerifyIsClosed:
                        bool isClosed = (statusCOM == "1");
                        ValidateAndPublishResult(statusCOM,"1", SelectedBank, "COM");
                        Log.Info($"{(statusCOM == "1" ? "Pass" : "Fail")} Verify Closed Status for {SelectedBank}-COM: {statusCOM}");
                        UpgradeVerdict(statusCOM == "1" ? Verdict.Pass : Verdict.Fail);
                        break;
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
