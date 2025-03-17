using OpenTap;
using System.Xml.Linq;

namespace InterconnectIOBox
{

    #region Relay Base Class

    public abstract class SingleRelay : ResultTestStep
    {
        #region Settings

        public InterconnectIO IO_Instrument { get; set; }

        public enum RelayCondition
        {
            NoAction, // No SCPI command will be sent
            Close_And_Read, // Send command to close the relay and read the relay state
            Open_And_Read, // Send command to open the relay and read the relay state
            Read_Only,      // Send command to read the relay state
            Close_Only,    // Send command to close the relay
            Open_Only,      // Send command to open the relay

        }

        [Display("Relay LPR1", Order: 1, Group: "Relays Control", Description: "Set the relay state to Open, Close, or NoAction." +
        " You can also read the relay state, close the relay and verify if it is closed, or open the relay and verify if it is open.")]
        public RelayCondition RelayLPR1 { get; set; }

        [Display("Relay LPR2", Order: 2, Group: "Relays Control", Description: "Set the relay state to Open, Close, or NoAction." +
        " You can also read the relay state, close the relay and verify if it is closed, or open the relay and verify if it is open.")]
        public RelayCondition RelayLPR2 { get; set; }

        [Display("Relay HPR1", Order: 3, Group: "Relays Control", Description: "Set the relay state to Open, Close, or NoAction." +
          " You can also read the relay state, close the relay and verify if it is closed, or open the relay and verify if it is open.")]
        public RelayCondition RelayHPR1 { get; set; }

        [Display("Relay SSR1", Order: 4, Group: "Relays Control", Description: "Set the relay state to Open, Close, or NoAction." +
        " You can also read the relay state, close the relay and verify if it is closed, or open the relay and verify if it is open.")]
        public RelayCondition RelaySSR1 { get; set; }
        [Display("Relay 5V_PWR", Order: 5, Group: "Relays Control", Description: "Set the relay 5V_PWR to DUT condition to Open, Close, or NoAction." +
        " You can also read the relay state, close the relay and verify if it is closed, or open the relay and verify if it is open.")]
        public RelayCondition Relay5VDUT { get; set; }

        #endregion
    }

    #endregion
    [Display(Groups: new[] { "InterconnectIO", "Analog" }, Name: "Isolated Relay Control", Description: "SCPI Command to set state of relays: LPR1, LPR2, HPR1, HPR2 and 5V_DUT." +
        " You can also read the relay state, close the relay and verify if it is closed, or open the relay and verify if it is open.\")")]
    public class RelayAction : SingleRelay
    {
        public RelayAction()
        {
            // Set default values
            RelayLPR1 = RelayCondition.NoAction;
            RelayLPR2 = RelayCondition.NoAction;
            RelayHPR1 = RelayCondition.NoAction;
            RelaySSR1 = RelayCondition.NoAction;
            Relay5VDUT = RelayCondition.NoAction;
        }

        public override void Run()
        {
            Log.Info("Executing relay actions...");

            HandleRelay("LPR1", RelayLPR1);
            HandleRelay("LPR2", RelayLPR2);
            HandleRelay("HPR1", RelayHPR1);
            HandleRelay("SSR1", RelaySSR1);
            HandleRelay("5V_DUT", Relay5VDUT);

        }

        private void HandleRelay(string relayName, RelayCondition condition)
        {
            string command = "";

            switch (condition)
            {
                case RelayCondition.Open_Only:
                case RelayCondition.Open_And_Read:
                    command = $"ROUTE:OPEN:PWR {relayName}";
                    if (relayName == "5V_DUT")
                    {
                        command = $"SYSTEM:OUTPUT OFF";
                    }
                    Log.Info($"Opening relay {relayName}");
                    break;

                case RelayCondition.Close_Only:
                case RelayCondition.Close_And_Read:
                    command = $"ROUTE:CLOSE:PWR {relayName}";
                    if (relayName == "5V_DUT")
                    {
                        command = $"SYSTEM:OUTPUT ON";
                    }
                    Log.Info($"Closing relay {relayName}");
                    break;

                case RelayCondition.NoAction:
                    //Log.Info($"No action for relay {relayName}");
                    return; // Skip SCPI command
            }

            if (command.Length > 6) // if command exists
            { 
                // Send SCPI command to the instrument
                Log.Info($"Sending SCPI command: {command}");
                IO_Instrument.ScpiCommand(command);  // Write SCPI command
            }

            // If no validation is required, pass the test immediately
            if (condition == RelayCondition.NoAction || condition == RelayCondition.Open_Only || condition == RelayCondition.Close_Only)
            {
                UpgradeVerdict(Verdict.Pass);
                return;
            }

            // Determine the SCPI query command based on the relay name
            command = relayName == "5V_DUT" ? "SYSTEM:OUTPUT?" : $"ROUTE:STATE:PWR? {relayName}";

            // Query the relay state
            string response = IO_Instrument.ScpiQuery<string>(command);
            Log.Info($"Relay {relayName} responded with: {response}");

            // Convert response to boolean states
            bool isClosed = response == "1";
            bool isOpen = response == "0";

            // Determine the expected state for validation
            bool expectedStateMet =
                (isClosed && condition == RelayCondition.Close_And_Read) ||
                (isOpen && condition == RelayCondition.Open_And_Read) ||
                (condition == RelayCondition.Read_Only);


            if (expectedStateMet)
            {
                UpgradeVerdict(Verdict.Pass); // Relay condition validated
            }
            else
            {
                string expectedState = condition == RelayCondition.Close_And_Read ? "Closed" : "Open";
                Log.Warning($"Relay {relayName} state mismatch. Expected: {expectedState}, Received: {response}");
                UpgradeVerdict(Verdict.Fail); // Relay condition not validated
            }

            double Limit = 0;
            if (condition == RelayCondition.Close_And_Read)
            {
                Limit = 1; // Set the limit to 1 for closed relay
            }

            bool isValid = double.TryParse(response, out double responseValue);

            if (condition == RelayCondition.Read_Only && isValid)
            {
                Limit = responseValue; // Set the limit to the response value in read only mode
            }


            // Publish final result
            var result = new TestResult<double>
            {
                ParamName = relayName,
                StepName = Name,
                Value = responseValue,
                LowerLimit = Limit,
                UpperLimit = Limit,
                Verdict = expectedStateMet ? "Pass" : "Fail",
                Units = "bit"
            };

            PublishResult(result);

        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            Log.Info("Preparing to execute relay actions...");
        }

        public override void PostPlanRun()
        {
            base.PostPlanRun();
            Log.Info("Relay actions completed.");
        }
    }

}
