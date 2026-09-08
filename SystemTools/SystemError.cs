using InterconnectIOBox.Analysis;
using InterconnectIOBox.Instruments;
using OpenTap;
using System.Text;

namespace InterconnectIOBox.SystemTools
{

    [Display(Groups: new[] { "FTS_Interconnect", "System" }, Name: "System Error", Description: "Reads the number of system errors, reads the error list, and/or clears the error list. The error count can optionally be validated and published.")]

    public class SysErr : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion
        public InterconnectIO IO_Instrument { get; set; }

        [Output]
        [Display("System Error Found:", Description: "The last error and/or full error list read, shown after the step has run.")]
        public string SysError { get; private set; }

        private const string GROUPD = "System Error Count";

        [Display("System Error Count?", Order: 1, Group: GROUPD, Collapsed: true, Description: "Read the number of system errors on the instrument.")]
        public bool EnableCnt { get; set; }

        [Display("Expected Errors Count:", Group: GROUPD, Order: 2, Collapsed: true, Description: "Expected number of errors. The result is published.")]
        [EnabledIf(nameof(EnableCnt), true, Flags = false)]
        public double ExpectedCount { get; set; } = 0;


        private const string GROUPE = "System Error List";

        [Display("List Last Error?", Order: 3, Group: GROUPE, Collapsed: true, Description: "Read the last error in the FIFO. The result is placed in the output.")]
        public bool EnableLast { get; set; }

        [Display("List All Errors?", Order: 4, Group: GROUPE, Collapsed: true, Description: "Read all errors in the FIFO, until empty. The result is placed in the output.")]
        public bool EnableAll { get; set; }

        // Safety limit to avoid an infinite loop if the instrument never
        // reports an empty error queue (e.g. malfunctioning FIFO).
        private const int MaxErrorReads = 100;


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
            var outputLines = new StringBuilder();

            if (EnableCnt)
            {
                string command = "SYST:ERR:COUN?";

                // Use ScpiQuery to read back from the device.
                string value = IO_Instrument.ScpiQuery<string>(command);
                Log.Info($"Sending SCPI command: {command}, response: {value}");

                double vvalue = double.Parse(value);
                if (ExpectedCount == vvalue)
                {
                    UpgradeVerdict(Verdict.Pass);
                    test = "PASS";
                    Log.Info($"System Error Count matches: {vvalue}, expected: {ExpectedCount}");
                }
                else
                {
                    UpgradeVerdict(Verdict.Fail);
                    test = "FAIL";
                    Log.Warning($"Invalid System Error Count: {vvalue}, expected: {ExpectedCount}");
                }

                // Publish final result
                var result = new TestResult<double>
                {
                    ParamName = "Syst Err Count",
                    StepName = Name,
                    Value = vvalue,
                    LowerLimit = ExpectedCount,
                    UpperLimit = ExpectedCount,
                    Verdict = test,
                    Units = "digcmp"
                };

                PublishResult(result);
            }

            if (EnableLast)
            {
                string command = "SYST:ERR?";
                // Use ScpiQuery to read back from the device.
                string value = IO_Instrument.ScpiQuery<string>(command);
                Log.Info($"Sending SCPI command: {command}, response: {value}");
                outputLines.AppendLine(value);
            }

            if (EnableAll)
            {
                bool empty;
                int reads = 0;

                do
                {
                    string command = "SYST:ERR:NEXT?";
                    string value = IO_Instrument.ScpiQuery<string>(command);
                    empty = value.Contains("0,");

                    if (!empty)
                    {
                        Log.Warning($"SCPI command: {command}, response: {value}");
                        outputLines.AppendLine(value);
                    }

                    reads++;
                }
                while (!empty && reads < MaxErrorReads);

                if (!empty)
                {
                    Log.Error($"Stopped reading the error FIFO after {MaxErrorReads} reads without reaching an empty queue. There may be more errors left unread.");
                }
            }

            if (outputLines.Length > 0)
            {
                SysError = outputLines.ToString().TrimEnd();
            }

            // if test is not defined, set the verdict
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