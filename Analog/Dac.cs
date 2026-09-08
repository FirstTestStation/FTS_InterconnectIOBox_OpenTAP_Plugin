using InterconnectIOBox.Analysis;
using InterconnectIOBox.Instruments;
using OpenTap;
using System.ComponentModel;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
namespace InterconnectIOBox.Analog
{
	[Display(Groups: new[] { "FTS_Interconnect", "Analog" }, Name: "DAC", Description: "Output a voltage between 0 and 3.3V from the Digital-to-Analog Converter (DAC).")]
	public class WriteDac : ResultTestStep
	{
		#region Settings
		// ToDo: Add property here for each parameter the end user should be able to change
		#endregion
		public InterconnectIO IO_Instrument { get; set; }
		public enum Act
		{
			[Display("Write Only", Description: "Write the voltage to the DAC without publishing a result.")]
			Write_only,

			[Display("Write and Publish", Description: "Write the voltage to the DAC and publish the result.")]
			Write_publish
		}

		[Display("Action to Execute:", Group: "DAC Write", Order: 0.1, Description: "Write the DAC value only, or write the value and publish the result.")]
		public Act SelectedAct { get; set; }
		[Display("DAC Set Default Voltage", Group: "Voltage", Order: 1.0, Description: "If checked, save this voltage as the power-up default.")]
		public bool Vdefault { get; set; }
		[Display("DAC Set Output", Group: "Voltage", Order: 1.1,Description: "Voltage value to set on the DAC output (0 to 3.3V).")]
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
			string strname = "DAC SetVolt";

			if (SetVolt < 0)
			{
				Log.Warning("Invalid low voltage value: " + SetVolt + "V. Minimum voltage of 0V will be used instead.");
				SetVolt = 0;
			}
			else if (SetVolt > 3.3)
			{
				Log.Warning("Invalid high voltage value: " + SetVolt + "V. Maximum voltage of 3.3V will be used instead.");
				SetVolt = 3.3;
			}

			IO_Instrument.ScpiCommand("ANA:DAC:VOLT " + SetVolt); // write voltage value on DAC
			Log.Info("DAC set voltage value: " + SetVolt + "V");

			if (Vdefault)
			{
				IO_Instrument.ScpiCommand("ANA:DAC:SAVE " + SetVolt); // save voltage value as power-up default
				Log.Info("DAC saved default voltage value: " + SetVolt + "V");
				strname = "DAC SetDefault";
			}

			UpgradeVerdict(Verdict.Pass);

			if (SelectedAct == Act.Write_publish)
			{
				// Publish final result
				var result = new TestResult<double>
				{
					ParamName = strname,
					StepName = Name,
					Value = SetVolt,
					LowerLimit = SetVolt,
					UpperLimit = SetVolt,
					Verdict = "PASS",
					Units = "Volts"
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