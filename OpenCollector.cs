using OpenTap;
using System.Xml.Linq;
using static InterconnectIOBox.Bank;

namespace InterconnectIOBox
{


    #region OC Base Class

    public abstract class OpenCollector : ResultTestStep
    {
        #region Settings

        public InterconnectIO IO_Instrument { get; set; }

        public enum OCCondition
        {
            NoAction, // No SCPI command will be sent
            Close,    // Send command to drive the transistor
            Open      // Send command to open the transistor
        }

        [Display("Set Open Collector #1", Order: 1, Group: "OC Transistor Set", Description: "Set the transistor condition to Open, Close, or NoAction.")]
        public OCCondition OC1 { get; set; }


        [Display("Set Open Collector #2", Order: 1.1, Group: "OC Transistor Set", Description: "Set the transistor condition to Open, Close, or NoAction.")]
        public OCCondition OC2 { get; set; }


        [Display("Set Open Collector #3", Order: 1.2, Group: "OC Transistor Set", Description: "Set the transistor condition to Open, Close, or NoAction.")]
        public OCCondition OC3 { get; set; }

        public enum OCRead
        {
            NoRead, // No SCPI command will be sent
            CheckClose,    // Send command to drive the transistor
            CheckOpen      // Send command to open the transistor
        }

        [Display("Read Open Collector #1", Order: 2, Group: "OC Transistor Validation?", Description: "Validate the state of the transistor condition to Open, Close, or NoAction.")]
        public OCRead OC1r { get; set; }


        [Display("Read Open Collector #2", Order: 2.1, Group: "OC Transistor Validation?", Description: "Validate the state of the transistor condition to Open, Close, or NoAction.")]
        public OCRead OC2r { get; set; }


        [Display("Read Open Collector #3", Order: 2.2, Group: "OC Transistor Validation?", Description: "Validate the state of the transistor condition to Open, Close, or NoAction.")]
        public OCRead OC3r { get; set; }

        #endregion
    }

    #endregion


    [Display(Groups: new[] { "InterconnectIO", "Analog" }, Name: "Open Collector Transistor Control", Description: "SCPI command to control the three open-collector transistors and read back their status.")]

    public class OCAction : OpenCollector
    {
        public OCAction()
        {
            // Set default values
            OC1 = OCCondition.NoAction;
            OC2 = OCCondition.NoAction;
            OC3 = OCCondition.NoAction;
            OC1r = OCRead.NoRead;
            OC2r = OCRead.NoRead;
            OC3r = OCRead.NoRead;

        }

        public override void Run()
        {
            Log.Info("Executing relay actions...");

            HandleOC("OC1", OC1, OC1r);
            HandleOC("OC2", OC2, OC2r);
            HandleOC("OC3", OC3, OC3r);

        }

        private void HandleOC(string OCName, OCCondition condition, OCRead roc)
        {
            string command = "";

            switch (condition)
            {
                case OCCondition.Open:
                    command = $"ROUTE:OPEN:OC {OCName}";
                    Log.Info($"Opening transistor {OCName}");
                    break;

                case OCCondition.Close:
                    command = $"ROUTE:CLOSE:OC {OCName}";
                    Log.Info($"Driving transistor {OCName}");
                    break;

                case OCCondition.NoAction:
                    //Log.Info($"No action for transistor {OCName}");
                    break;
            }

            if (condition != OCCondition.NoAction)
            {
                Log.Info($"Sending SCPI command: {command}");
                IO_Instrument.ScpiCommand(command);
            }


            if (roc == OCRead.NoRead)  // Skip validation if NoRead is selected
            {
                return;
            }

            // read Command
            command = $"ROUTE:STATE:OC? {OCName}";
            string response = IO_Instrument.ScpiQuery<string>(command);
            Log.Info($"Read transistor {OCName} response: {response}");

            string verdictStr = "";
            int Limit = 0;

            // Ensure response is either "0" or "1"
            if (response != "0" && response != "1")
            {
                Log.Error($"Unexpected response from transistor {OCName}: {response}");
                verdictStr = "FAIL";
                UpgradeVerdict(Verdict.Fail);
            }
            else
            {
                // Validate CheckClose condition
                if (roc == OCRead.CheckClose)
                {
                    if (response == "1")
                    {
                        verdictStr = "PASS"; Limit = 1;
                        UpgradeVerdict(Verdict.Pass);
                    }
                    else
                    {
                        Log.Error($"Read transistor {OCName} did not return the expected Close status (1), read: {response}");
                        verdictStr = "FAIL"; Limit = 1;
                        UpgradeVerdict(Verdict.Fail);
                    }
                }
                // Validate CheckOpen condition
                else if (roc == OCRead.CheckOpen)
                {
                    if (response == "0")
                    {
                        verdictStr = "PASS"; Limit = 0;
                        UpgradeVerdict(Verdict.Pass);
                    }
                    else
                    {
                        Log.Error($"Read transistor {OCName} did not return the expected Open status (0), read: {response}");
                        verdictStr = "FAIL"; Limit = 0;
                        UpgradeVerdict(Verdict.Fail);
                    }
                }
            }

            // Ensure response is valid before parsing
            if (!int.TryParse(response, out int responseValue))
            {
                Log.Error($"Invalid response from transistor {OCName}: {response}");
                responseValue = -1; // Assign an invalid value to indicate failure
            }


            // Publish final result
            var result = new TestResult<double>
            {
                ParamName = OCName,
                StepName = Name,
                Value = responseValue, 
                LowerLimit = Limit,
                UpperLimit = Limit,
                Verdict = verdictStr,
                Units = "bit"
            };

            PublishResult(result);
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            Log.Info("Preparing to execute open collector actions...");
        }

        public override void PostPlanRun()
        {
            base.PostPlanRun();
            Log.Info("Open Collector actions completed.");
        }
    }

}
