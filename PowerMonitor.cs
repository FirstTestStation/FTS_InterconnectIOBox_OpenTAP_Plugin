using OpenTap;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace InterconnectIOBox
{
    public abstract class PwrMonitor : ResultTestStep
    {


        #region Settings

        public InterconnectIO IO_Instrument { get; set; }

        public enum PwrSource
        {
            Load_Voltage,
            Shunt_Voltage,
            Current_mA,
            Power_mW
        }

        [Display("PWR function", Group: "Power Function", Description: "Choose the PWR function to read.")]
        public PwrSource SelectedPwr { get; set; }


        [Display("Low Limit", Order: 1, Group: "Test Limit")]
        [Description("Set lower and high limit")]
        public double LowerLimit { get; set; }

        [Display("High Limit", Order: 2, Group: "Test Limit")]
        public double UpperLimit { get; set; }

        [Display("Calibrate current ", Order: 3, Group: "Current Calibration", Description: "Calibrate the current on fullrange to get more precision.")]
        public bool Calibration { get; set; }

        [Display("Expected Current (mA) ", Order: 3, Group: "Current Calibration", Description: "Calibrate the current on fullrange to get more precision.")]
        [Unit("mA", UseEngineeringPrefix: false)]
        public double ExpCurrent { get; set; }

        #endregion
    }



    [Display(Groups: new[] { "InterconnectIO", "Analog" }, Name: "PWR Monitor read", Description: "Read Load Voltage or (0.1 Ohm) Shunt voltage or Current (mA) or Power (mW).")]

    public class ReadPWR : PwrMonitor
    {
        [Output]
        [Display("Measure:")]
        public string Measure { get; private set; }
        public ReadPWR()
        {
            // ToDo: Set default values for properties / settings.
            LowerLimit = 0;
            UpperLimit = 0;
            ExpCurrent = 2000;
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
            string lname = "";
            string unit = "Volt";

            // Perform actions based on the selected ADC source
            Log.Info($"Selected PWR Function: {SelectedPwr}");

            switch (SelectedPwr)
            {
                case PwrSource.Load_Voltage:
                    command = $"ANA:PWR:VOLT?";
                    lname = "Load Voltage";
                    name = "Load Voltage";
                    Log.Info("Reading from Power load Voltage");
                    break;
                case PwrSource.Shunt_Voltage:
                    command = $"ANA:PWR:SHUNT?";
                    lname = "0.1 Ohm Shunt Voltage";
                    name = "Shunt Voltage";
                    unit = "mV";
                    Log.Info("Reading from ADC1...");
                    break;
                case PwrSource.Current_mA:
                    command = $"ANA:PWR:Ima?";
                    lname = "0-2000 mA Current read";
                    name = "Current";
                    unit = "mA";
                    Log.Info("Reading current in mA");
                    break;
                case PwrSource.Power_mW:
                    command = $"ANA:PWR:Pmw?";
                    lname = "Power (P=EI) mW";
                    name = "Power";
                    unit = "mW";
                    Log.Info("Reading power in mW");
                    break;
                default:
                    Log.Warning("Unknown PWR source selected.");
                    break;
            }


            Log.Info($"Sending SCPI command: {command}");

            // Use ScpiQuery to read back from the device.
            string value = IO_Instrument.ScpiQuery<string>(command);

            double rvalue = double.Parse(value);

            Log.Info($"{lname} read: " + value + " " + unit + ". Limits are set to: >= " + LowerLimit + " and <= " + UpperLimit);


            // Determine verdict as a string ("PASS" or "FAIL")
            string verdictStr = (rvalue >= LowerLimit && rvalue <= UpperLimit) ? "PASS" : "FAIL";
            UpgradeVerdict(verdictStr == "PASS" ? Verdict.Pass : Verdict.Fail);

            Measure = $"{rvalue:F3} {unit}";  // Output measure with 3 decimal places

            var result = new TestResult<double>
            {
                ParamName = name,
                StepName = Name,
                Value = rvalue,
                LowerLimit = LowerLimit,
                UpperLimit = UpperLimit,
                Verdict = verdictStr,
                Units = unit
            };

            PublishResult(result);

            if (Calibration && SelectedPwr == PwrSource.Current_mA)
            {
                CalibrateCurrent(rvalue, ExpCurrent);
            }
            else if (Calibration)
            {
                Log.Warning("Calibration is only available for Current measurement.");
                UpgradeVerdict(Verdict.Error);
            }

        }

        // <summary>
        /// Current Calibration and publishes the test result.
        /// </summary>
        void CalibrateCurrent(double ActualValue, double ExpectedValue)
        {
            string command = $"ANA:PWR:CAL {ActualValue},{ExpectedValue}";
            Log.Info($"Sending SCPI command: {command}");
            IO_Instrument.ScpiCommand(command); // write calibration value

            // readback current value
            command = $"ANA:PWR:Ima?";
            // Use ScpiQuery to read back from the device.
            string value = IO_Instrument.ScpiQuery<string>(command);

            double rvalue = double.Parse(value);

            double llimit = ExpectedValue * 0.9; // 10% less
            double ulimit = ExpectedValue * 1.1; // 10% more
            string test = "FAIL";
            if (rvalue >= llimit && rvalue <= ulimit)
            {
                UpgradeVerdict(Verdict.Pass);
                test = "PASS";
            }
            else
            {
                UpgradeVerdict(Verdict.Fail);
                Log.Info($"Calibration failed. Expected: {ExpectedValue} mA between Low: {llimit} and High: {ulimit}, Actual: {rvalue} mA");
            }

            Name = $"Current Calibration at {rvalue}";

            var result = new TestResult<double>
            {
                ParamName = "CurrentCAL",
                StepName = Name,
                Value = rvalue,
                LowerLimit = llimit,
                UpperLimit = ulimit,
                Verdict = test,
                Units = "mA"
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
