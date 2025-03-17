using OpenTap;
using System.ComponentModel;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace InterconnectIOBox
{

    [Display(Groups: new[] { "InterconnectIO", "Analog" }, Name: "DAC", Description: "Output voltage 0 - 3.3V from Digital Analog Converter (DAC).")]

    public class WriteDac : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion
        public InterconnectIO IO_Instrument { get; set; }

        [Display("DAC Set default Voltage", Group: "Voltage")]
        [Description("Set voltage value as power up default")]
        public bool Vdefault { get; set; }

        [Display("DAC Set Output", Group: "Voltage")]
        [Description("Set voltage value")]
        [Unit("V")]
        public double SetVolt { get; set; }
        public WriteDac()
        {
            // ToDo: Set default values for properties / settings.
            SetVolt = 2.5;
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        public override void Run()
        {

            if (SetVolt < 0)
            {
                Log.Warning("Invalid low voltage value: " + SetVolt + " Minimum voltage of 0V is used");
                SetVolt = 0;
            }
            else if (SetVolt > 3.3)
            {
                Log.Warning("Invalid high voltage value: " + SetVolt + " Maximum voltage of 3.3V is used");
                SetVolt = 3.3;
            }
            else
            {
                IO_Instrument.ScpiCommand("ANA:DAC:VOLT " + SetVolt); // write voltage value on DAC
                Log.Info("DAC Set voltage value: " + SetVolt + "V");
            }

            if (Vdefault)
            {
                IO_Instrument.ScpiCommand("ANA:DAC:SAVE " + SetVolt); // write voltage value on DAC
                Log.Info("DAC Save default voltage value: " + SetVolt + "V");
            }

            UpgradeVerdict(Verdict.Pass);
            // Publish final result
            var result = new TestResult<double>
            {
                ParamName = "DAC SetVolt:",
                StepName = Name,
                Value = SetVolt,
                LowerLimit = SetVolt,
                UpperLimit = SetVolt,
                Verdict ="PASS",
                Units = "Volts"
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
