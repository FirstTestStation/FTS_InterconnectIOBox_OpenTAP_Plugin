using OpenTap;
using System;
using static InterconnectIOBox.SBank;

namespace InterconnectIOBox
{

    [Display(Groups: new[] { "InterconnectIO", "Route" }, Name: "Relays Banks validation", Description: "Read Bank Relay Status for one or many Bank (1: Close, 0:Open). Optionnal Status validation is available ")]


    public class SBRelay : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion
        public InterconnectIO IO_Instrument { get; set; }

        [Display("StatusBank1", Order: 1.1, Group: "Relay Banks", Description: "Bank selected will read status.")]
        public bool StatusBank1 { get; set; }
        [Display("StatusBank2", Order: 1.2, Group: "Relay Banks", Description: "Bank selected will read status..")]
        public bool StatusBank2 { get; set; }
        [Display("StatusBank3", Order: 1.3, Group: "Relay Banks", Description: "Bank selected will read status.")]
        public bool StatusBank3 { get; set; }
        [Display("StatusBank4", Order: 1.4, Group: "Relay Banks", Description: "Bank selected will read status.")]
        public bool StatusBank4 { get; set; }


        [Display("Bank1", Order: 2.1, Group: "Bank Validation", Description: "Verify Expected Value (dec,hex, bin) of Bank1 Status.")]
        public string testBank1 { get; set; }

        [Display("Bank2", Order: 2.2, Group: "Bank Validation", Description: "Verify Expected Value (dec,hex, bin) of Bank2 Status.")]
        public string testBank2 { get; set; }

        [Display("Bank3", Order: 2.3, Group: "Bank Validation", Description: "Verify Expected Value (dec,hex, bin) of Bank3 Status.")]
        public string testBank3 { get; set; }

        [Display("Bank4", Order: 2.4, Group: "Bank Validation", Description: "Verify Expected Value of (dec,hex, bin) Bank4 Status.")]
        public string testBank4 { get; set; }



        public SBRelay()
        {
            // ToDo: Set default values for properties / settings.
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        /// <summary>
        /// Validates test result and publishes it.
        /// </summary>
        void ValidateAndPublishResult(string measuredValue, string expectedValueStr, string bankName)
        {
            if (double.TryParse(measuredValue, out double measured) &&
                double.TryParse(expectedValueStr, out double expected))
            {
                bool isPass = measured == expected;
                string status = isPass ? "PASS" : "FAIL";
                UpgradeVerdict(isPass ? Verdict.Pass : Verdict.Fail);

                Log.Info($"{status} Verify {bankName} Status: {measured}, expected {expected}");

                var result = new TestResult<double>
                {
                    ParamName = bankName,
                    StepName = Name,
                    Value = measured,
                    LowerLimit = expected,
                    UpperLimit =expected,
                    Verdict = status,
                    Units = "relays"
                };

                PublishResult(result);
            }
            else
            {
                Log.Warning($"Invalid number format for {bankName}. Measured: {measuredValue}, Expected: {expectedValueStr}");
                UpgradeVerdict(Verdict.Error);
            }
        }

        public override void Run()
        {
            string readvalue = "";
            string command = $"ROUTE:BANK:STATE?  ";
            int openlg = command.Length;



            if (StatusBank1) { command += " BANK1,"; }
            if (StatusBank2) { command += " BANK2,"; }
            if (StatusBank3) { command += " BANK3,"; }
            if (StatusBank4) { command += " BANK4,"; }

            int endlg = command.Length;


            if (endlg > openlg)  // if command as at least one Bank
            {
                string Cmd = command.Substring(0, command.Length - 1); // Remove the last comma
                Log.Info($"Sending SCPI command: {command}");
                // Use ScpiQuery to read back from the device.
                readvalue = IO_Instrument.ScpiQuery<string>(Cmd);

                Log.Info($"SCPI Answer: {readvalue}");
                UpgradeVerdict(Verdict.Pass);   // if no validation, declare PASS
            }
            else
            {
                Log.Warning($"SCPI command not complete, no Bank selected on command: {command}");
                UpgradeVerdict(Verdict.NotSet);
            }

            if (readvalue.Length > 0)
            {
                string[] readparts = readvalue.Split(new string[] { "," }, StringSplitOptions.None);

                // Section who validate the results of the test
                if (!string.IsNullOrEmpty(readvalue))
                {
                    
                    int idx = 0;
                    if (idx < readparts.Length && StatusBank1 && testBank1 != null)
                    {
                        ValidateAndPublishResult(readparts[idx++], testBank1, "Bank1");
                    }
                    if (idx < readparts.Length && StatusBank2 && testBank2 != null)
                    {
                        ValidateAndPublishResult(readparts[idx++], testBank2, "Bank2");

                    }
                    if (idx < readparts.Length && StatusBank3 && testBank3 != null)
                    {
                        ValidateAndPublishResult(readparts[idx++], testBank3, "Bank3");
                    }
                    if (idx < readparts.Length && StatusBank4 && testBank4 != null)
                    {
                        ValidateAndPublishResult(readparts[idx++], testBank4, "Bank4");

                    }
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