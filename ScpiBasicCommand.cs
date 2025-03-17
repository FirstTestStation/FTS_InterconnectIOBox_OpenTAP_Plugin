using OpenTap;
using System;
using System.Data;
using System.Threading;
using static InterconnectIOBox.SpiCfg;

namespace InterconnectIOBox
{
    [Display(Groups: new[] { "InterconnectIO", "SCPI " }, Name: "Basic Command", Description: "Required SCPI Basic command.")]

    public class Basic : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        public InterconnectIO IO_Instrument { get; set; }


        private const string GROUPD = "*IDN? (Identification String)";

        [Display("Sent Query *IDN?", Order: 1, Group: GROUPD, Collapsed: true, Description: "Send command *IDN? to the instrument.")]
        public bool EnableIdn { get; set; }

        [Display("Test and Publish", Order: 1.2, Group: GROUPD, Collapsed: true, Description: "Validate answer and publish is available.")]
        public bool IDNp { get; set; }

        [Display("Validation subString:", Order: 1.3, Group: GROUPD, Collapsed: true, Description: "partial string expected to be part of the response to the command *IDN?.")]
        [EnabledIf(nameof(IDNp), true, Flags = false)]
        public string ExpectedIdn { get; set; } = "InterconnectIO";

        private const string GROUPE = "*TST? (Internal Basic Selftest)";

        [Display("Sent Query *TST?", Order: 2, Group: GROUPE, Collapsed: true, Description: "Send command *TST? to the instrument to run an internal selftest.")]
        public bool EnableTst { get; set; }

        [Display("Test and Publish", Order: 2.1, Group: GROUPE, Collapsed: true, Description: "Validate answer and publish is available.")]
        public bool TSTp { get; set; }

        [Display("Validation subString:", Order: 2.2, Group: GROUPE, Collapsed: true, Description: "partial string expected to be part of the response to the command *IDN?.")]
        [EnabledIf(nameof(TSTp), true, Flags = false)]
        public string ExpectedTst { get; set; } = "OK";


        private const string GROUPF = "*OPC and *OPC? (Operation Complete)";

        [Display("Sent Command *OPC", Order: 2, Group: GROUPF, Collapsed: true, Description: "Send command *OPC to know of all previous commad have finished executing.")]
        public bool OpcCmd { get; set; }

        [Display("Sent Query *OPC?", Order: 2, Group: GROUPF, Collapsed: true, Description: "Send command *OPC? to wait the previous command finish.")]
        public bool OpcQuery { get; set; }

 


        private const string GROUPG = "Single Function Command";

        [Display("Sent Command *RST", Order: 3, Group: GROUPG, Collapsed: true, Description: "Send command *RST (Reset) to the instrument.")]
        public bool EnableRST { get; set; }

        [Display("Sent Command *CLS", Order: 3.1, Group: GROUPG, Collapsed: true, Description: "Send command *CLS (Clear Status) to the instrument.")]
        public bool EnableCLS { get; set; }

        [Display("Sent Command *WAI", Order: 3.1, Group: GROUPG, Collapsed: true, Description: "Send command *WAI (Wait) wait for all pending operation to complete.")]
        public bool EnableWai { get; set; }


        public Basic()
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

            UpgradeVerdict(Verdict.Pass); // PASS by default
            if (EnableIdn) { TstCommand("*IDN?", IDNp, ExpectedIdn);}
            if (EnableTst) { TstCommand("*TST?", TSTp, ExpectedTst); }
            if (OpcCmd)    { SendCommand("*OPC"); }
            if (OpcQuery) { TstCommand("*OPC?", false, "1"); }

            if (EnableCLS) { SendCommand("*CLS"); }
            if (EnableWai) { SendCommand("*WAI"); }

            if (EnableRST) 
            {
                //SendCommand("*RST");
                ResetAndReconnect();
            }



        }

        public void ResetAndReconnect()
        {
            try
            {
                IO_Instrument.ScpiCommand("*RST");
                Log.Info("Instrument reset. Waiting for reboot...");

                // Wait some time for the instrument to reboot
             //   Thread.Sleep(5000); // 5 seconds, adjust as needed

            //    Log.Info("Reconnecting to instrument...");
            //    IO_Instrument.Close();
           //     Thread.Sleep(1000); // small pause before reopen
           //     IO_Instrument.Open();

                Log.Info("Reconnection successful.");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to reset and reconnect: {ex.Message}");
            }
        }


        /// <summary>
        /// Sent SCPI command.
        /// </summary>
        /// <param name="command">SCPI command to send.</param>
        public void SendCommand(string command)
        {
            Log.Info($"Sending SCPI command: {command}");
            IO_Instrument.ScpiCommand(command);
            UpgradeVerdict(Verdict.Pass);
        }


        /// <summary>
        /// Write and test the response from an SCPI query.
        /// </summary>
        /// <param name="command">SCPI command to send.</param>
        /// <param name="publish">Selected function for action ("selectedAct").</param>
        /// <param name="expectedstr">Partial string expected to be part of the response.</param>
        public void TstCommand(string command, bool publish, string expectedstr)
        {
            string test = "";
            Log.Info($"Sending SCPI command: {command}");
            string response = IO_Instrument.ScpiQuery(command);
            Log.Info($"Response: {response}");
            UpgradeVerdict(Verdict.Pass); // PASS by default
 
            string answer = response.Replace("\"", "").Trim();

            if (publish)
            {
                if (answer.Contains(expectedstr))
                {
                    Log.Info($"Response contains expected string: {expectedstr}");
                    UpgradeVerdict(Verdict.Pass);
                    test="PASS";
                }
                else
                {
                    Log.Error($"Response:{answer} does not contain expected string: {expectedstr}");
                    UpgradeVerdict(Verdict.Fail);
                    test = "FAIL";
                }

                var result = new TestResult<string>
                {
                    ParamName = command,
                    StepName = Name,
                    Value = answer,
                    LowerLimit = expectedstr,
                    UpperLimit = expectedstr,
                    Verdict = test,
                    Units = "strcmp"
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
