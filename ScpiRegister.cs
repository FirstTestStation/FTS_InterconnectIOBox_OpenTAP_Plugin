using OpenTap;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace InterconnectIOBox
{

    [Display(Groups: new[] { "InterconnectIO", "SCPI " }, Name: "Register Command", Description: "Group of command used to read or SCPI Register.")]


    public class RegCmd : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion


        public InterconnectIO IO_Instrument { get; set; }
        public enum Action
        {
            write_read,
            write_only,
            read_only,
            read_test
        }

        [Display("Action to Execute:", Group: "SCPI Register", Order: 0.3,
        Description: "Action to perform on the Register.\n" +
                 "Write_read: Write Data and read answer. Answer is compared with expected read and result is published.\n" +
                 "Write_only: Write Data and exit.\n" +
                 "Read_only: Read Data and publish.\n" +
                 "Read_test: Read Data and compare with expected data. Data is published")]
        public Action RegAct { get; set; }

        public enum Reg // SCPI Register
        {
            ESR_Event_Status_Register,
            SRE_Service_Request_Enable,
            STB_Status_Byte_Register,
            ESE_Event_Status_Enable,
            Status_Questionnable_condition,
            Status_Questionnable_Event,
            Status_Preset,
            Status_Questionnable_Enable,
            Status_Operation_condition,
            Status_Operation_Event,
            Status_Operation_Enable
        }


        [Display("Register List", Group: "SCPI Register Command", Order: 0.5, Description: "Action to perform on the SCPI Register")]
        public Reg SelectReg { get; set; }

        private const string GROUPD = "SCPI Register Data";

        [Display("Register Data Write:", Group: GROUPD, Order: 2, Description: "Data to write on selected Register")]
        [EnabledIf(nameof(RegAct), new object[] { Action.write_read, Action.write_only }, HideIfDisabled = false)]
        public byte wdata { get; set; }

        [Display("Register Expected Data:", Group: GROUPD, Order: 2, Description: "Expected data to compare with the data read")]
        [EnabledIf(nameof(RegAct), new object[] { Action.read_only, Action.read_test, Action.write_read }, HideIfDisabled = false)]
        public byte edata { get; set; }

        public RegCmd()
        {
            // ToDo: Set default values for properties / settings
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        /// <summary>
        /// Instantiate an OpenTAP picture with some default picture
        /// These can be controlled by other test step properties if they should be configurable, or they can be hardcoded values
        /// </summary>
        public Picture Picture { get; } = new Picture()
        {
            Source = "Register.jpg",
            Description = "A Diagram of the SCPI Register to help understand."
        };

        /// <summary>
        /// Control the source of the picture with a regular test step property
        /// </summary>
        [Display("Source", "The source of the picture. This can be a URL or a file path.", "Register Diagram Picture", Order: 0.1, Collapsed: true)]
        [FilePath(FilePathAttribute.BehaviorChoice.Open)]
        public string PictureSource
        {
            get => Picture.Source;
            set => Picture.Source = value;
        }


        public override void Run()
        {
            string WriteCmd = "";
            string ReadCmd = "";
            string regname = "";
            string wvalue = wdata.ToString(); // transform data to write to string


            switch (SelectReg)
            {
                case Reg.ESR_Event_Status_Register:
                    regname = "ESR_Event_Status_Register";
                    WriteCmd = "";
                    ReadCmd = "*ESR?";
                    break;

                case Reg.SRE_Service_Request_Enable:
                    regname = "SRE_Service_Request_Enable";
                    WriteCmd = "*SRE";
                    ReadCmd = "*SRE?";
                    break;

                case Reg.STB_Status_Byte_Register:
                    regname = "STB_Status_Byte_Register";
                    WriteCmd = "";
                    ReadCmd = "*STB?";
                    break;

                case Reg.ESE_Event_Status_Enable:
                    regname = "ESE_Event_Status_Enable";
                    WriteCmd = "*ESE";
                    ReadCmd = "*ESE?";
                    break;

                case Reg.Status_Questionnable_condition:
                    regname = "Status_Questionnable_condition";
                    WriteCmd = "";
                    ReadCmd = "STATus:QUEStionable:CONDition?";
                    break;

                case Reg.Status_Questionnable_Event:
                    regname = "Status_Questionnable_Event";
                    WriteCmd = "";
                    ReadCmd = "STATus:QUEStionable:EVENt?";
                    break;

                case Reg.Status_Preset:
                    regname = "Status_Preset";
                    WriteCmd = "STATus:PRESet";
                    ReadCmd = "STATus:QUEStionable?";
                    wvalue = "";  // no paramater allowed for this command

                    break;

                case Reg.Status_Questionnable_Enable:
                    regname = "Status_Questionnable_Enable";
                    WriteCmd = "STATus:QUEStionable:ENABle";
                    ReadCmd = "STATus:QUEStionable:ENABle?";
                    break;

                case Reg.Status_Operation_condition:
                    regname = "Status_Operation_condition";
                    WriteCmd = "";
                    ReadCmd = "STATus:OPERation:CONDition?";
                    break;

                case Reg.Status_Operation_Event:
                    regname = "Status_Operation_Event";
                    WriteCmd = "";
                    ReadCmd = "STATus:OPERation:EVENt?";
                    break;

                case Reg.Status_Operation_Enable:
                    regname = "Status_Operation_Enable";
                    WriteCmd = "STATus:OPERation:ENABle";
                    ReadCmd = "STATus:OPERation:ENABle?";
                    break;
            }

            if (RegAct == Action.write_read || RegAct == Action.write_only)
            {
                if (WriteCmd != "") // if command exist
                {
                    string cmd = WriteCmd + " " + wvalue;
                    Log.Info($"Sending write SCPI command: {cmd}");
                    IO_Instrument.ScpiCommand(cmd);
                }
                else
                {
                    Log.Info("Write command not available for this register");
                }
                UpgradeVerdict(Verdict.Pass);
            }

            if (RegAct == Action.write_read || RegAct == Action.read_only || RegAct == Action.read_test)
            {

                Log.Info($"Sending read SCPI command: {ReadCmd}");
                string response = IO_Instrument.ScpiQuery<string>(ReadCmd);
                Log.Info($"{regname} read: " + response);
                if (RegAct == Action.read_only)
                {
                    UpgradeVerdict(Verdict.Pass);
                    return;
                }

                string test = "";
                double value = double.Parse(response);
                if (value == edata)
                {
                    UpgradeVerdict(Verdict.Pass);
                    test = "PASS";
                }
                else
                {
                    UpgradeVerdict(Verdict.Fail);
                    test = "FAIL";
                    Log.Info($"{regname} read: {value}, Expected: {edata}");
                }


                // Create test result object
                TestResult<double> result = new TestResult<double>
                {
                    ParamName = regname,
                    StepName = Name,
                    Value = value,
                    Verdict = test,
                    Units = "read"
                };

                // Add limits for Write_Read function
                if (RegAct == Action.write_read || RegAct == Action.read_test)
                {
                    result.LowerLimit = edata;
                    result.UpperLimit = edata;
                    result.Units = "digcmp";
                }

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
