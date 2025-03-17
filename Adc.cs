using OpenTap;
using System.ComponentModel;
using System.Reflection;
using static InterconnectIOBox.ADCReadVolt;

namespace InterconnectIOBox
{
    public abstract class ADCReadVolt : ResultTestStep
    {

        #region Settings
        public InterconnectIO IO_Instrument { get; set; }

        public enum AdcSource
        {
            ADC0,
            ADC1,
            VSYS,
            TEMPERATURE
        }

        [Display("ADC Source", Group: "ADC Source Selection", Description: "Choose the ADC source ineternal or external to use.")]
        public AdcSource SelectedAdc { get; set; }


        [Display("Low Limit", Order: 1, Group: "Test Limit",Description: "Set lower and high limit")]
        public double LowerLimit { get; set; }

        [Display("High Limit", Order: 2, Group: "Test Limit")]
        public double UpperLimit { get; set; }
        #endregion

    }

    [Display(Groups: new[] { "InterconnectIO", "Analog" }, Name: "ADC", Description: "Read the voltage from the Pico Analog-to-Digital Converter (ADC) and verify if it falls within the expected range." +
        " The Pico Master has two ADC inputs, ADC0 and ADC1, each supporting a voltage range of 0 to 3V DC. Additionally, VSYS measures the voltage at the Pico Master's VSYS pin, which is expected to be 5V." +
        " The internal temperature sensor provides a reading in Celsius, representing the Pico Master's internal temperature")]

    public class ReadADC : ADCReadVolt
    {
        public ReadADC()
        {
            // ToDo: Set default values for properties / settings.
            LowerLimit = 0;
            UpperLimit = 3;
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        public override void Run()
        {
            string command = "";
            string name = "";
            string unit = "Volt";
            string test = "";

            // Perform actions based on the selected ADC source
            Log.Info($"Selected ADC Source: {SelectedAdc}");

            switch (SelectedAdc)
            {
                case AdcSource.ADC0:
                    command = $"ANA:ADC0:VOLT?";
                    name = "ADC0";
                    Log.Info("Reading from ADC0...");
                    // Add logic for ADC0
                    break;
                case AdcSource.ADC1:
                    command = $"ANA:ADC1:VOLT?";
                    name = "ADC1";
                    Log.Info("Reading from ADC1...");
                    // Add logic for ADC1
                    break;
                case AdcSource.VSYS:
                    command = $"ANA:ADC:VSYS?";
                    name = "VSYS";
                    Log.Info("Reading Master Pico VSYS voltage...");
                    // Add logic for VSYS
                    break;
                case AdcSource.TEMPERATURE:
                    command = $"ANA:ADC:TEMP?";
                    name = "TEMP(C)";
                    unit = "Celcius";
                    Log.Info("Reading Master Pico temperature sensor...");
                    // Add logic for Temp
                    break;
                default:
                    Log.Warning("Unknown ADC source selected.");
                    break;
            }


            Log.Info($"Sending SCPI command: {command}");

            // Use ScpiQuery to read back from the device.
            string value = IO_Instrument.ScpiQuery<string>(command);

            double vvalue = double.Parse(value);

            Log.Info($"{name} read: " + value + " " + unit + ". Limits are set to: > " + LowerLimit + " and < " + UpperLimit);

            if (vvalue >= LowerLimit && vvalue <= UpperLimit)
            {
                UpgradeVerdict(Verdict.Pass);
                test ="PASS";
            }
            else
            {
                UpgradeVerdict(Verdict.Fail);
                test = "FAIL";
            }
            // Publish final result
            var result = new TestResult<double>
            {
                ParamName = name,
                StepName = Name,
                Value = vvalue,
                LowerLimit = LowerLimit,
                UpperLimit = UpperLimit,
                Verdict = test,
                Units = unit
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
