using OpenTap;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static InterconnectIOBox.Bank;
using static InterconnectIOBox.SingleRelay;
using static System.Net.Mime.MediaTypeNames;

namespace InterconnectIOBox
{
    [Display(Groups: new[] { "InterconnectIO", "System" }, Name: "Beeper", Description: "Generate short (100mS) beep pulse")]

    public class Beeper : TestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        public InterconnectIO IO_Instrument { get; set; }

        public Beeper()
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
            string command = $"SYSTEM:BEEPER";

            // Send SCPI command to the instrument
            Log.Info($"Sending SCPI command: {command}");
            IO_Instrument.ScpiCommand(command);



            UpgradeVerdict(Verdict.Pass);
        }
        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }

    [Display(Groups: new[] { "InterconnectIO", "System" }, Name: "Pico Firmware Version", Description: "Read Firmware version for Pico Master, Slave1, Slave2, Slave3 devices ")]

    public class Firmware : ResultTestStep
    {
        #region Settings

        public InterconnectIO IO_Instrument { get; set; }

 
        [Display("Master", Order: 2, Group: "Version Check", Description: "Pico Master Version expected to set verdict = Pass.")]
        public string Master { get; set; }

        [Display("Slave", Order: 2, Group: "Version Check", Description: "Pico Slave Version expected to set verdict = Pass.")]
        public string Slave { get; set; }
        #endregion


        public Firmware()
        {
            Master = "1.1";
            Slave = "1.1";
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        public override void Run()
        {
            string command = "SYSTEM:DEVICE:VERSION?";

            // Send SCPI command to the instrument
            Log.Info($"Sending SCPI command: {command}");

            string response = IO_Instrument.ScpiQuery<string>(command);

            Log.Info($"Firmware version for: Master, Slave1, Slave2, Slave3 - {response}");

            string readVersion = response.Replace("\"", "").Trim();

            // Split and trim each element
            string[] versions = readVersion.Split(',')
                                           .Select(v => v.Trim())
                                           .ToArray();

            if (versions.Length < 4)
            {
                Log.Error("Unexpected firmware version response format.");
                UpgradeVerdict(Verdict.Fail);
                return;
            }

            // Validate Master firmware
            PublishFirmwareVersion("Master", versions[0], Master);

            // Validate Slave firmware (any match among Slave1, Slave2, Slave3)
            string expSlave = $"{Slave}, {Slave}, {Slave }";
            PublishFirmwareVersion("Slave1_2_3", string.Join(", ", versions.Skip(1)), expSlave);
        }

        /// <summary>
        /// Publishes firmware version validation results.
        /// </summary>
        /// <param name="deviceType">Device type (Master/Slave).</param>
        /// <param name="actualVersion">Actual firmware version(s).</param>
        /// <param name="expectedVersion">Expected firmware version.</param>

        public void PublishFirmwareVersion(string deviceType, string actualVersion, string expectedVersion)
        {
            bool match = (actualVersion == expectedVersion);
            if (string.IsNullOrEmpty(expectedVersion))
            {
                Log.Info($"{deviceType} Firmware version not validated: {actualVersion}");
                UpgradeVerdict(Verdict.Pass);
            }
            else if (match)
            {
                Log.Info($"{deviceType} Firmware version validated: {actualVersion}");
                UpgradeVerdict(Verdict.Pass);
            }
            else
            {
                Log.Info($"The {deviceType} Firmware version does not match the expected version: {expectedVersion}. Actual: {actualVersion}");
                UpgradeVerdict(Verdict.Fail);
            }
            // Publish final result
            var result = new TestResult<string>
            {
                ParamName = deviceType,
                StepName = Name,
                Value = actualVersion,
                LowerLimit = expectedVersion,
                Verdict = match ? "PASS":"FAIL",
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

    [Display(Groups: new[] { "InterconnectIO", "System" }, Name: "Error Led", Description: "Set or read the status of the Red Error LED. Each line of the test control is processed sequentially. " +
      "If you first select 'ON' and then 'OFF,' the LED will briefly flash")]

    public class ErrorLed : ResultTestStep
    {
        #region Settings

        // ToDo: Add property here for each parameter the end user should be able to change

        public InterconnectIO IO_Instrument { get; set; }

        [Display("Turn Red LED ON", Order: 1, Group: "LED Control", Description: "Turn ON the Red Error Led.")]
        public bool RedLedOn { get; set; }

        [Display("Turn Red LED OFF", Order: 3, Group: "LED Control", Description: "Turn OFF the Red Error Led.")]
        public bool RedLedOff { get; set; }

        [Display("Red Led ON Test?", Order: 2, Group: "LED Control", Description: "Validate if LED is ON and publish result")]
        public bool LedCheckOn { get; set; }

        [Display("Red Led OFF Test?", Order: 4, Group: "LED Control", Description: "Validate if LED is OFF and publish result")]
        public bool LedCheckOFF { get; set; }
        #endregion


        public ErrorLed()
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
            string command;
            UpgradeVerdict(Verdict.Pass);  // Default Verdict

            if (RedLedOn)
            {
                command = $"SYSTEM:LED:ERROR ON";
                Log.Info("Setting Error Red Led ON...");
                // Send SCPI command to the instrument
                IO_Instrument.ScpiCommand(command);
            }

            if (LedCheckOn) { ValidateLedStatus(1); }  

            if (RedLedOff)
            {
                command = $"SYSTEM:LED:ERROR OFF";
                Log.Info("Setting Error Red Led OFF...");
                // Send SCPI command to the instrument
                IO_Instrument.ScpiCommand(command);
            }

            if (LedCheckOFF) { ValidateLedStatus(0); }
        }

        private void ValidateLedStatus(int ExpStatus)
        {
            // Query LED status
            string cmdq = "SYSTEM:LED:ERROR?";
            Log.Info($"Sending SCPI command: {cmdq}");

            string response = IO_Instrument.ScpiQuery<string>(cmdq)?.Trim();

            int.TryParse(response, out int ledStatus);
            string test = "FAIL";
            string name = ExpStatus == 1 ? "ON" : "OFF";

            // Validate LED state based on expected values
            if ((ledStatus == ExpStatus))
            {
                test = "PASS";
                UpgradeVerdict(Verdict.Pass);
            }
            else
            {
                Log.Error($"Red LED state mismatch: Expected ON= 1, OFF= 0, Actual={response}");
                UpgradeVerdict(Verdict.Fail);
            }

            // Publish final result
            var result = new TestResult<double>
            {
                ParamName = $"Error Led {name}",
                StepName = Name,
                Value = ledStatus,
                LowerLimit = ExpStatus,
                UpperLimit = ExpStatus,
                Verdict = test,
                Units = "bits"
            };

            PublishResult(result);
        }

       

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }

    [Display(Groups: new[] { "InterconnectIO", "System" }, Name: "Pico Slaves", Description: "Enable, Disable (to reset the Pico Slave)  or Read Pico Slave Status.Each line of the test control is processed sequentially." +
        "If you first select 'OFF' and then 'ON,' the Slaves will be reset to default configuration.")]
    public class Slaves : ResultTestStep
    {
        #region Settings

        // ToDo: Add property here for each parameter the end user should be able to change

        public InterconnectIO IO_Instrument { get; set; }

        [Display("Disable Slaves (OFF)", Order: 1, Group: "Slaves Control", Description: "Disable Slaves with RUN = 0.")]
        public bool SlavesOff { get; set; }

        [Display("Enable Slaves (ON)", Order: 3, Group: "Slaves Control", Description: "Enable Slaves with RUN = 1.")]
        public bool SlavesOn { get; set; }

        [Display("Slaves Disabled?", Order: 2, Group: "Slaves Control", Description: "check if Slave are disbaled (RUN = 0).")]
        public bool SlCheckOff { get; set; }

        [Display("Slaves Enabled?", Order: 4, Group: "Slaves Control", Description: "check if Slave are disbaled (RUN = 1).")]
        public bool SlCheckOn { get; set; }
        #endregion

      
        public Slaves()
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
            string command;

            if (SlavesOff)
            {
                command = $"SYSTEM:SLAVES OFF";
                Log.Info("Setting Pico Slaves OFF...");
                // Send SCPI command to the instrument
                IO_Instrument.ScpiCommand(command);
                TapThread.Sleep(100); // Sleep for 100 milliseconds
            }

            if (SlCheckOff) { ValidateLedStatus(0); }

            if (SlavesOn)
            {
                command = $"SYSTEM:SLAVES ON";
                Log.Info("Setting Pico Slaves ON...");
                IO_Instrument.ScpiCommand(command);
            }

            if (SlCheckOn) { ValidateLedStatus(1); }
        }

        private void ValidateLedStatus(int ExpStatus)
        {
            // Query Slaves status
            string cmdq = $"SYSTEM:SLAVES?";
            Log.Info($"Sending SCPI command: {cmdq}");

            string response = IO_Instrument.ScpiQuery<string>(cmdq)?.Trim();

            int.TryParse(response, out int SlaveStatus);
            string test = "FAIL";
            string name = ExpStatus == 1 ? "ON" : "OFF";

            // Validate Slave state based on expected values
            if ((SlaveStatus == ExpStatus))
            {
                test = "PASS";
                UpgradeVerdict(Verdict.Pass);
            }
            else
            {
                Log.Error($"Slaves Status mismatch: Expected Enabled= 1, Disabled= 0, Actual={response}");
                UpgradeVerdict(Verdict.Fail);
            }

            // Publish final result
            var result = new TestResult<double>
            {
                ParamName = $"Slaves {name}",
                StepName = Name,
                Value = SlaveStatus,
                LowerLimit = ExpStatus,
                UpperLimit = ExpStatus,
                Verdict = test,
                Units = "bits"
            };

            PublishResult(result);
        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }

 
    [Display(Groups: new[] { "InterconnectIO", "System" }, Name: "System Version", Description: "Read SCPI version used, Version could be validated and result publish")]

    public class SystemVersion : ResultTestStep
    {
        #region Settings

        public InterconnectIO IO_Instrument { get; set; }


        [Display("System", Order: 1, Group: "Version Check", Description: "System Version expected to set verdict = Pass. Empty Check version will report System version only")]
        public string SystemV { get; set; }

        #endregion


        public SystemVersion()
        {
            SystemV = "1999.0";
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        public override void Run()
        {
            string command = $"SYSTEM:VERSION?";

            // Send SCPI command to the instrument
            Log.Info($"Sending SCPI command: {command}");

            string response = IO_Instrument.ScpiQuery<string>(command);

            string value = response.Trim();
            string Limit = SystemV;  // Set expected string as limit
            string test = "FAIL";

            if (SystemV == "")
            {
                Log.Info($"System Version not validated: {value}");
                UpgradeVerdict(Verdict.Pass);
                Limit = ""; // erase limit string to indicate no limit (read only)
            } 
            else
            if (value == SystemV)
            {
                Log.Info($"Validated System Version: {value}");
                UpgradeVerdict(Verdict.Pass);
                test = "PASS";

            }
            else
            {
                Log.Error($"The System version does not match the expected version: {value}");
                UpgradeVerdict(Verdict.Fail);
            }

            // Publish final result
            var result = new TestResult<string>
            {
                ParamName = "System Version",
                StepName = Name,
                Value = $"'{value}",
                LowerLimit = $"'{Limit}",
                UpperLimit = $"'{Limit}",  // apostrohe added to have excel to kee the number string complete
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
