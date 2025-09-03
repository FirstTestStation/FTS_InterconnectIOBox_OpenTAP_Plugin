using System;
using OpenTap;

namespace InterconnectIOBox
{
    // Simple dialog class for entering Serial Number
    public class EnterSNDialog
    {
        [Display("Serial Number", Description: "Enter or scan the DUT Serial Number.")]
        public string SerialNumber { get; set; }
    }

    [Display("Get Serial Number", Groups: new[] { "InterconnectIO", "Fixture/DUT" },
        Description: "Prompts operator for DUT Serial Number if not already set.")]
    public class GetSerialNumberStep : TestStep
    {
        [Display("DUT")]
        public FTS_DUT Dut { get; set; }

        [Display("Timeout (s)", Description: "Time in seconds before the dialog automatically closes. 0 = wait indefinitely.")]
        public double Timeout { get; set; } = 0;

        public override void Run()
        {
            // 1️⃣ If the Serial Number is already set (GUI injected), skip prompt
            if (Dut != null && !string.IsNullOrWhiteSpace(Dut.SerialNumber))
            {
                Log.Info($"Serial Number already provided by GUI: {Dut.SerialNumber}");
                return;
            }

            // 2️⃣ Otherwise, ask operator using UserInput dialog
            var snDialog = new EnterSNDialog();
            TimeSpan ts = Timeout > 0 ? TimeSpan.FromSeconds(Timeout) : TimeSpan.Zero;

            try
            {
                if (Timeout > 0)
                    UserInput.Request(snDialog, ts); // Opens GUI dialog or CLI prompt
                else
                    UserInput.Request(snDialog); // Wait indefinitely
            }
            catch (TimeoutException)
            {
                throw new Exception("User did not enter a Serial Number before timeout.");
            }

            // 3️⃣ Store the Serial Number in DUT
            if (!string.IsNullOrWhiteSpace(snDialog.SerialNumber))
            {
                if (Dut != null)
                {
                    Dut.SerialNumber = snDialog.SerialNumber;
                    Log.Info($"Serial Number set: {Dut.SerialNumber}");
                }
            }
            else
            {
                throw new Exception("No Serial Number entered.");
            }
        }
    }

    [Display("Clear Serial Number", Groups: new[] { "InterconnectIO", "Fixture/DUT" },
        Description: "Clears DUT Serial Number to request a new one in the next run.")]
    public class ClearSerialNumberStep : TestStep
    {
        [Display("DUT")]
        public FTS_DUT Dut { get; set; }

        public override void Run()
        {
            if (Dut != null)
            {
                Dut.SerialNumber = null;
                Log.Info("Serial Number cleared. Next DUT will ask for a new one.");
            }
        }
    }
}
