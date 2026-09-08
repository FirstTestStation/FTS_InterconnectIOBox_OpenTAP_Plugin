using InterconnectIOBox.Analysis;
using InterconnectIOBox.Instruments;
using OpenTap;

namespace InterconnectIOBox.Configuration
{
    public abstract class Config : ResultTestStep
    {
        #region Settings
        public InterconnectIO IO_Instrument { get; set; }

        public enum Parameter
        {
            PARTNUMBER,
            SERIALNUMBER,
            MOD_OPTION,
            COM_SER_SPEED,
            COM_SER_ECHO,
            PICO_SLAVES_RUN,
            TESTBOARD_NUM,
            PARAMETER1,
            PARAMETER2,
            PARAMETER3,
            PARAMETER4,
            PARAMETER5,
            TEST
        }

        [Display("Parameter", Group: "EEprom Parameters", Order:1,Description: "Choose the parameter to read or write.")]
        public Parameter SelectedParameter { get; set; }

        #endregion
    }


    [Display(Groups: new[] { "FTS_Interconnect", "Config" }, Order: 2, Name: "Read Full Configuration", Description: "The system power-up configuration is saved in an EEPROM. This SCPI command (CFG:READ:EEPROM:FULL?) " +
        "retrieves all parameters stored in the EEPROM.")]
    public class Rcfgf : TestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion
        public InterconnectIO IO_Instrument { get; set; }
        public Rcfgf()
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
            string command = "CFG:READ:EEPROM:FULL?";
            Log.Info($"Sending SCPI command: {command}");

            // Use ScpiQuery to read back from the device.
            string value = IO_Instrument.ScpiQuery<string>(command);

            Log.Info($"Actual Configuration: {value}");

            UpgradeVerdict(Verdict.Pass);
        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }


    [Display(Groups: new[] { "FTS_Interconnect", "Config" }, Order: 2,Name: "Write Default Configuration", Description: "The system power-up configuration is saved in an EEPROM. This SCPI command (CFG:WRITE:EEPROM:DEFAULT) " +
          "writes all default parameters to the EEPROM.")]
    public class Wcfgf : TestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion
        public InterconnectIO IO_Instrument { get; set; }
        public Wcfgf()
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
            string command = "CFG:WRITE:EEPROM:DEFAULT";
            Log.Info($"Sending SCPI command: {command}");

            IO_Instrument.ScpiCommand(command); // write default parameters

            UpgradeVerdict(Verdict.Pass);
        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }


    [Display(Groups: new[] { "FTS_Interconnect", "Config" }, Order: 2, Name: "Write Configuration Parameter", Description: "The system power-up configuration is saved in an EEPROM. This SCPI command (CFG:WRITE:EEPROM:STR) " +
           "writes a single parameter to the EEPROM. Run the 'Read Configuration' test step to get the list of valid parameters.")]
    public class Wcfgp : Config
    {
        #region Settings

        [Display("Value (string)", Order: 3, Group: "EEprom Parameters", Description: "Parameter value to write to the configuration.")]
        public string Pvalue { get; set; }
        #endregion

        public Wcfgp()
        {
            Pvalue = "";
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
        }


        public override void Run()
        {
            // Parameter selected
            Log.Info($"Selected Parameter: {SelectedParameter}, Value: {Pvalue}");

            string Ptvalue = Pvalue.Trim();
            if (string.IsNullOrWhiteSpace(Ptvalue))
            {
                Log.Warning("Value string is empty. Please provide a valid value to update the configuration.");
                UpgradeVerdict(Verdict.Fail); // Mark the test step as failed.
                return; // Exit the method if the value is invalid.
            }

            string command = $"CFG:WRITE:EEPROM:STR {SelectedParameter}, '{Ptvalue}'";
            Log.Info($"Sending SCPI command: {command}");
            IO_Instrument.ScpiCommand(command); // write parameter
            UpgradeVerdict(Verdict.Pass); // Mark the test step as passed.
        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }

    }




    [Display(Groups: new[] { "FTS_Interconnect", "Config" }, Order: 2,Name: "Read Configuration Parameter", Description: "The system power-up configuration is saved in an EEPROM. This SCPI command (CFG:READ:EEPROM:STR?) " +
           "reads a single parameter from the EEPROM. Validation of the value read is optional.")]
    public class Rcfgp : Config
    {

        [Display("Expected Value", Order: 3, Group: "Parameter Validation", Description: "If not empty, the value read will be checked against this expected value (case-insensitive).")]
        public string ExpValue { get; set; }

        public Rcfgp()
        {
            ExpValue = "";
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
        }

        public override void Run()
        {
            // Parameter selected
            Log.Info($"Selected Parameter: {SelectedParameter}");

            string command = $"CFG:READ:EEPROM:STR? {SelectedParameter}";
            Log.Info($"Sending SCPI command: {command}");
            // Use ScpiQuery to read back from the device.
            string value = IO_Instrument.ScpiQuery<string>(command);
            Log.Info($"Value Read: {value}");

            string readValue = value?.Trim() ?? "";
            string expected = ExpValue.Trim();
            string test;

            if (expected.Length == 0 || string.Equals(readValue, expected, System.StringComparison.OrdinalIgnoreCase))
            {
                UpgradeVerdict(Verdict.Pass);
                test = "Pass";
            }
            else
            {
                Log.Info($"Value read does not match the expected value: {readValue}, {expected}");
                UpgradeVerdict(Verdict.Fail);
                test = "Fail";
            }

            // Publish final result
            var result = new TestResult<string>
            {
                ParamName = $"{SelectedParameter}",
                StepName = Name,
                Value = readValue,
                LowerLimit = expected,
                UpperLimit = expected,
                Verdict = test,
                Units = "string"
            };

            PublishResult(result);

        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }

    }
}