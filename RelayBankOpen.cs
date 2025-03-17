using OpenTap;
using System.ComponentModel;



namespace InterconnectIOBox
{

    [Display(Groups: new[] { "InterconnectIO", "Route" }, Name: "Open Single Bank", Description: "Open single relay Bank or all the banks")]
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
            string opencommand = $"ROUTE:OPEN:ALL  ";
            int openlg = opencommand.Length;

            if (OpenBank1) { opencommand += " BANK1,"; }
            if (OpenBank2) { opencommand += " BANK2,"; }
            if (OpenBank3) { opencommand += " BANK3,"; }
            if (OpenBank4) { opencommand += " BANK4,"; }

            int endlg = opencommand.Length;

            if (endlg > openlg)  // if command as at least one Bank
            {
                string Command = opencommand.Substring(0, opencommand.Length - 1); // Remove the last comma
                Log.Info($"Sending SCPI command: {Command}");
                IO_Instrument.ScpiCommand(Command); // Send SCPI command to close the relay
                UpgradeVerdict(Verdict.Pass);
            }
            else
            {
                Log.Warning($"SCPI command not complete, no Bank selected on command: {opencommand}");
                UpgradeVerdict(Verdict.NotSet);

            }

         
        }



        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}


