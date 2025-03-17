
using OpenTap;
using System.Xml.Linq;

namespace InterconnectIOBox
{

    [Display(Groups: new[] { "InterconnectIO", "System" }, Name: "System Error", Description: "read number of Errors. Read List Errors. Empty List of Errors. Publish is value Expected")]

    public class SysErr : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion
        public InterconnectIO IO_Instrument { get; set; }

        [Output]
        [Display("System Error Found:")]
        public string SysError { get; private set; }

        private const string GROUPD = "System Error Count";

        [Display("System Error Count?", Order: 1, Group: GROUPD, Collapsed: true, Description: "Read number of system error on the instruments.")]
        public bool EnableCnt { get; set; }

        [Display("Expected Errors Count:", Group: GROUPD, Order: 2, Collapsed: true,Description: "Expected number of Errors. Result is published")]
        [EnabledIf(nameof(EnableCnt), true, Flags = false)]
        public double expectedCount { get; set; } = 0;


        private const string GROUPE = "System Error List";

        [Display("List Last Error?", Order: 3, Group: GROUPE, Collapsed: true, Description: "Read the last errors in the fifo.Result will be placed on output")]
        public bool EnableLast { get; set; }

        [Display("List All Errors?", Order: 4, Group: GROUPE, Collapsed: true, Description: "Read All the errors in the fifo. Result will be placed on output")]
        public bool EnableAll { get; set; }


        public SysErr()
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
            string test = "";
            string value = "";
            double vvalue;

            if (EnableCnt) {
                string command = "SYST:ERR:COUN?";
                
                // Use ScpiQuery to read back from the device.
                value = IO_Instrument.ScpiQuery<string>(command);
                Log.Info($"Sending SCPI command: {command} ,response: {value}  ");

                vvalue = double.Parse(value);
                if (expectedCount == vvalue)
                {
                    UpgradeVerdict(Verdict.Pass);
                    test = "PASS";
                    Log.Info($"System Error Match Count: {vvalue}, expected: {expectedCount}");
                }
                else
                {
                    UpgradeVerdict(Verdict.Fail);
                    test = "FAIL";
                    Log.Warning($"Invalid System Error Count: {vvalue}, expected: {expectedCount}");
                }

                // Publish final result
                var result = new TestResult<double>
                {
                    ParamName = "Syst Err Count",
                    StepName = Name,
                    Value = vvalue,
                    LowerLimit = expectedCount,
                    UpperLimit = expectedCount,
                    Verdict = test,
                    Units = "digcmp"
                };

                PublishResult(result);

            }

            if (EnableLast)
            {
                string command = "SYST:ERR?";
                // Use ScpiQuery to read back from the device.
                value = IO_Instrument.ScpiQuery<string>(command);
                Log.Warning($"Sending SCPI command: {command} ,response: {value}  ");
                SysError = value;

            }

            if (EnableAll)
            {
                bool bt;
                do
                {
                    string command = "SYST:ERR:NEXT?";
                    value = IO_Instrument.ScpiQuery<string>(command);
                    Log.Warning($"SCPI command: {command}, response: {value}");
                    bt = value.Contains("0,");
                }
                while (!bt);
            }

            // if test is not defined , set the verdict
            if (test.Length == 0)
            {
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
