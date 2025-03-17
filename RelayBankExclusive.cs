using OpenTap;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using static InterconnectIOBox.Bank;
using static InterconnectIOBox.SingleRelay;

namespace InterconnectIOBox     
{


    public abstract class EBank : TestStep
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

        [Display("Channel", Order: 1, Group: "RELAY BANK1", Collapsed:true, Description: "Select Relay to close on BANK1 (CH8-CH15 will close the Reverse Relay)")]
        public RelaySelection CBank1 { get; set; } = RelaySelection._;

        [Display("Channel", Order: 2, Group: "RELAY BANK2", Collapsed: true, Description: "Select Relay on BANK2 (CH8-CH15 will close the Reverse Relay)")]
        public RelaySelection CBank2 { get; set; } = RelaySelection._;
   
        [Display("Channel", Order: 3, Group: "RELAY BANK3", Collapsed: true, Description: "Select Relay on BANK3 (CH8-CH15 will close the Reverse Relay)")]
        public RelaySelection CBank3 { get; set; } = RelaySelection._;

        [Display("Channel", Order: 4, Group: "RELAY BANK4", Collapsed: true, Description: "Select Relay on BANK4 (CH8-CH15 will close the Reverse Relay)")]
        public RelaySelection CBank4 { get; set; } = RelaySelection._;

        #endregion
    }


    [Display(Groups: new[] { "InterconnectIO", "Route" }, Name: "Exclusive Mode: Close Relay by Bank", Description: "Closes one relay per bank in Exclusive mode. " +
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
            int relay;
            string closeCommand = $"ROUTE:CLOSE:EXCLUSIVE (@";

            if ((int)CBank1 >= 0)
            {
                relay = 100 + (int)CBank1;
                closeCommand += relay.ToString()+ ",";
            }

            if ((int)CBank2 >= 0)
            {
                relay = 200 + (int)CBank2;
                closeCommand += relay.ToString() + ",";
            }

            if ((int)CBank3 >= 0)
            {
                relay = 300 + (int)CBank3;
                closeCommand += relay.ToString() + ",";
            }

            if ((int)CBank4 >= 0)
            {
                relay = 400 + (int)CBank4;
                closeCommand += relay.ToString() + ",";
            }

            string Command = closeCommand.Substring(0, closeCommand.Length - 1) + ")";
            Log.Info($"Sending SCPI command: {Command}");
            IO_Instrument.ScpiCommand(Command); // Send SCPI command to close the relay
           
            UpgradeVerdict(Verdict.Pass);
        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}
