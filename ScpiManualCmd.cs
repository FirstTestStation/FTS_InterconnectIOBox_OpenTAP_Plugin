using OpenTap;
using System;
using System.Data;
using System.Threading;
using static InterconnectIOBox.SpiCfg;
using static System.Net.Mime.MediaTypeNames;

namespace InterconnectIOBox
{
    [Display(Groups: new[] { "InterconnectIO", "SCPI " }, Name: "Manual Command", Description: "SCPI command enter manually.")]

    public class Manual : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        public InterconnectIO IO_Instrument { get; set; }


        [Display("Write Command:", Group: "SCPI COMMAND ", Order: 1, Description: "Send SCPI command, not expecting any answer.")]
        public string WCommand { get; set; }

        [Display("Write Query:", Group: "SCPI QUERY ", Order: 2, Description: "Send SCPI query, read the answer.")]
        public string WQuery { get; set; }


        [Display("Validate Numeric Answer:", Group: "SCPI QUERY ", Order: 3, Description: "Check if Answer will be a numeric value")]

        public bool Num
        {
            get => _Num;
            set
            {
                _Num = value;
                if (value) Str = false; //
                OnPropertyChanged(nameof(Num));
                OnPropertyChanged(nameof(Str));
            }
        }
        private bool _Num;

        [Display("Validate String Answer:", Group: "SCPI QUERY ", Order: 3.1, Description: "Check if Answer will be a string")]

        public bool Str
        {
            get => _Str;
            set
            {
                _Str = value;
                if (value) Num = false; //
                OnPropertyChanged(nameof(Num));
                OnPropertyChanged(nameof(Str));
            }
        }
        private bool _Str;

  
        [Display("Expected Answer:", Group: "SCPI QUERY ", Order: 4, Description: "Expected partial or full answer to validate query, result is published.")]
        [EnabledIf("Str", true, Flags = false)]
        public string Eanswer { get; set; }

        [Display(">= Low Limit:", Group: "SCPI QUERY ", Order: 5, Description: "Numeric Answer need to be greater or equal Low Limit.result is published")]
        [EnabledIf("Num", true, Flags = false)]
        public double LowL { get; set; }

        [Display("<= High Limit:", Group: "SCPI QUERY ", Order: 6, Description: "Numeric Answer need to be lower or equal High Limit.result is published")]
        [EnabledIf("Num", true, Flags = false)]
        public double HighL { get; set; }


        public Manual()
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
            if (WCommand != null)
            {
                Log.Info($"Sending SCPI command: {WCommand}");
                IO_Instrument.ScpiCommand(WCommand);
                UpgradeVerdict(Verdict.Pass);
            }

            if (WQuery !=null)
            {
                if (WQuery.Length > 0) // if Wquery is not empty
                {
                    string test = "";
                    string response = "";
                    try
                    {
                        response = IO_Instrument.ScpiQuery(WQuery);
                        Log.Info($"Sending SCPI query: {WQuery}, Response: {response}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"SCPI Error on query: {WQuery}, error: {ex.Message}");
                        UpgradeVerdict(Verdict.Error);
                        return;
                    }

                    if (Num)
                    {
                        double answer = double.Parse(response);

                        if (answer >= LowL && answer <= HighL)
                        {
                            Log.Info($"Response is in range: {LowL} <= {answer} <= {HighL}");
                            UpgradeVerdict(Verdict.Pass);
                            test = "PASS";
                        }
                        else
                        {
                            Log.Error($"Response:{answer} is not in range: {LowL} <= {answer} <= {HighL}");
                            UpgradeVerdict(Verdict.Fail);
                            test = "FAIL";
                        }
                        var result = new TestResult<double>
                        {
                            ParamName = WQuery,
                            StepName = Name,
                            Value = answer,
                            LowerLimit = LowL,
                            UpperLimit = HighL,
                            Verdict = test,
                            Units = "num"
                        };
                        PublishResult(result);
                    }

                    if (Str)
                    {

                        string answer = response.Replace("\"", "").Trim();

                        if (answer.Contains(Eanswer))
                        {
                            Log.Info($"Response contains expected string: {Eanswer}");
                            UpgradeVerdict(Verdict.Pass);
                            test = "PASS";
                        }
                        else
                        {
                            Log.Error($"Response:{answer} does not contain expected string: {Eanswer}");
                            UpgradeVerdict(Verdict.Fail);
                            test = "FAIL";
                        }

                        var result = new TestResult<string>
                        {
                            ParamName = WQuery,
                            StepName = Name,
                            Value = answer,
                            LowerLimit = Eanswer,
                            UpperLimit = Eanswer,
                            Verdict = test,
                            Units = "strcmp"
                        };

                        PublishResult(result);
                    }
                }
            }
        }


        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}
