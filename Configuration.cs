using InterconnectIOBox;
using OpenTap;
using System.ComponentModel;
using static InterconnectIOBox.PwrMonitor;

namespace InterconnectIOBox
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

        [Display("Parameter", Group: "EEprom Parameters"), Description("Choose the parameter to read or write.")]
        public Parameter SelectedParameter { get; set; }


        #endregion
    }


    [Display(Groups: new[] { "InterconnectIO", "Config" }, Name: "Read Full Configuration", Description: "System power-up configuration is saved in an EEPROM. This SCPI command (CFG:READ:EEPROM:FULL?) " +
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
            // ToDo: Add test case code here
            string command = "CFG:Read:Eeprom:Full?";
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
}



[Display(Groups: new[] { "InterconnectIO", "Config" }, Name: "Write Default Configuration", Description: "System power-up configuration is saved in an EEPROM. This SCPI command (CFG:WRITE:EEPROM:DEFAULT) " +
      "write all default parameters in the EEPROM.")]
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
        // ToDo: Add test case code here
        string command = "CFG:Write:Eeprom:Default";
        Log.Info($"Sending SCPI command: {command}");

        IO_Instrument.ScpiCommand(command); // write default parameter

        UpgradeVerdict(Verdict.Pass);
    }

    public override void PostPlanRun()
    {
        // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
        base.PostPlanRun();
    }
}



[Display(Groups: new[] { "InterconnectIO", "Config" }, Name: "Write Configuration parameter ", Description: "System power-up configuration is saved in an EEPROM. This SCPI command (CFG:WRITE:EEPROM:STR) " +
       " write a single parameter to the EEPROM. Run TestSTep Read Configuration to get the list of valids parameters")]
public class Wcfgp : Config
{
    #region Settings

    [Display("Value (string)", Order: 2, Group: "EEprom Parameters"), Description("Parameter value to update on Configuration")]
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
        // ToDo: Add test case code here

        // Paramater selected
        Log.Info($"Selected Parameter: {SelectedParameter}, Value: {Pvalue} ");

        string Ptvalue = Pvalue.Trim();
        if (string.IsNullOrWhiteSpace(Ptvalue))
        {
            Log.Warning("Value string is empty. Please provide a valid parameter to update the configuration.");
            UpgradeVerdict(Verdict.Fail); // Mark the test step as failed.
            return; // Exit the method or test step if Parameter or value  is invalid.
        }

        string command = $"CFG:WRITE:EEPROM:STR {SelectedParameter} , '{Ptvalue}'";
        Log.Info($"Sending SCPI command: {command}");
        IO_Instrument.ScpiCommand(command); // write parameter
        UpgradeVerdict(Verdict.Pass); // Mark the test step as failed.
    }

    public override void PostPlanRun()
    {
        // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
        base.PostPlanRun();
    }

}




[Display(Groups: new[] { "InterconnectIO", "Config" }, Name: "Read Configuration parameter ", Description: "System power-up configuration is saved in an EEPROM. This SCPI command (CFG:READ:EEPROM:STR?) " +
       " Read single parameter to the EEPROM. Validation of Value read is possible")]
public class Rcfgp : Config
{

    [Display("Expected Value", Order: 1, Group: "Parameter Validation")]
    [Description("if not empty, Check Value read against Expected Value")]
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
        // ToDo: Add test case code here

        // Parameter selected
        Log.Info($"Selected Parameter: {SelectedParameter}");

        string command = $"CFG:READ:EEPROM:STR? {SelectedParameter}";
        Log.Info($"Sending SCPI command: {command}");
        // Use ScpiQuery to read back from the device.
        string value = IO_Instrument.ScpiQuery<string>(command);
        Log.Info($"Value Read: {value}");

        string Evalue = ExpValue.Trim().ToUpper();
        string test = "";

        if (value == Evalue || ExpValue.Length == 0)
        {
            UpgradeVerdict(Verdict.Pass); // Mark the test step as failed.
            test = "Pass";
        }
        else
        {
            Log.Info($"Value read do not match with Expected Value: {value} , {Evalue}");
            UpgradeVerdict(Verdict.Fail); // Mark the test step as failed.
            test = "Fail";
        }
        // Publish final result
        var result = new TestResult<string>
        {
            ParamName = $"{SelectedParameter}",
            StepName = Name,
            Value = value,
            LowerLimit = Evalue,
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