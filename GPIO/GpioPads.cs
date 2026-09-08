using InterconnectIOBox.Analysis;
using InterconnectIOBox.Instruments;
using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace InterconnectIOBox.GPIO
{
    [Display(Groups: new[] { "FTS_Interconnect", "GPIO" }, Name: "GPIO PAD Write/Read", Description:
        "Low-level GPIO PAD configuration register used to control pin behavior.\n" +
        "Bits [7]   : OD        - Output Disable.\n" +
        "Bits [6]   : IE        - Input Enable.\n" +
        "Bits [5:4] : DRIVE    - Drive Strength (00=2mA, 01=4mA, 10=8mA, 11=12mA).\n" +
        "Bit  [3]   : PUE      - Pull-Up Enable.\n" +
        "Bit  [2]   : PDE      - Pull-Down Enable.\n" +
        "Bit  [1]   : SCHMITT  - Schmitt Trigger Enable.\n" +
        "Bit  [0]   : SLEWFAST - Slew Rate Control (1=Fast, 0=Slow).\n" +
        "Bits [31:8]: Reserved.")]
    public class GpioPAD : ResultTestStep
    {
        public InterconnectIO IO_Instrument { get; set; }

        public enum GpPAD
        {
            Write_read,
            Write_only,
            read_only,
            read_test
        }

        // --- ROW CONFIGURATION CLASS TO MANAGE LAYOUT ORDER ---
        public class GpioPinRow
        {
            // Column showing the actual GPIO bit index (0-28).
            // OpenTap's array/table editor numbers rows 1..N by default, which doesn't
            // match the 0-based GP0..GP28 naming — this column removes the ambiguity.
            // Not used by Run() (which relies on the array's loop index) — informational only.
            [Display("GPIO #", Order: 0, Description: "GPIO bit index for this row (0-28). Informational only.")]
            public int Pin { get; set; }

            // Order 1: Places the checkbox on the far left column
            [Display("Enable", Order: 1, Description: "Enable or disable configuration for this pin.")]
            public bool IsEnabled { get; set; } = false;

            // Order 2: Places the byte entry input box immediately to the right
            [Display("PAD Value", Order: 2, Description: "PAD configuration byte value.")]
            public byte Value { get; set; } = 86;
        }

        [Display("Action to Execute:", Group: "Gpio PAD Write/Read", Order: 0.4)]
        public GpPAD SelectedpAct { get; set; }

        [Display("Publish Results", Order: 0.5, Group: "Options")]
        public bool PublishResults { get; set; } = true;

        // --- DEDICATED GPIOS ---
        private const string GROUPD = "Dedicated GPIO PAD Write/Read";
        private const string DDESC = "Write/Read a byte on dedicated GPIO PAD.";

        [Display("M1_IO0 Pad:", Group: GROUPD, Order: 2, Collapsed: true, Description: DDESC)] public Enabled<byte> M1IO0 { get; set; } = new Enabled<byte> { Value = 86 };
        [Display("M1_IO1 Pad:", Group: GROUPD, Order: 2.1, Collapsed: true, Description: DDESC)] public Enabled<byte> M1IO1 { get; set; } = new Enabled<byte> { Value = 86 };
        [Display("S1_IO8 Pad:", Group: GROUPD, Order: 2.2, Collapsed: true, Description: DDESC)] public Enabled<byte> S1IO8 { get; set; } = new Enabled<byte> { Value = 86 };
        [Display("S1_IO9 Pad:", Group: GROUPD, Order: 2.3, Collapsed: true, Description: DDESC)] public Enabled<byte> S1IO9 { get; set; } = new Enabled<byte> { Value = 86 };
        [Display("FLAG Pad:", Group: GROUPD, Order: 2.4, Collapsed: true, Description: DDESC)] public Enabled<byte> FLAG { get; set; } = new Enabled<byte> { Value = 86 };
        [Display("CTRL Pad:", Group: GROUPD, Order: 2.5, Collapsed: true, Description: DDESC)] public Enabled<byte> CTRL { get; set; } = new Enabled<byte> { Value = 86 };

        // --- ARRAY ARRAYS WITH CLEANED-UP MATRIX VIEWS ---
        [Display("Master 0 GPIOs (0-28)", Order: 3, Group: "Master0, Any GPIO PAD Write/Read byte.", Description: "Configure PAD values for Master 0 bank.")]
        public GpioPinRow[] M0_Gpios { get; set; } = CreateGpioArray();

        [Display("Slave 1 GPIOs (0-28)", Order: 3.1, Group: "Slave1, Any GPIO PAD Write/Read byte.", Description: "Configure PAD values for Slave 1 bank.")]
        public GpioPinRow[] S1_Gpios { get; set; } = CreateGpioArray();

        [Display("Slave 2 GPIOs (0-28)", Order: 3.2, Group: "Slave2, Any GPIO PAD Write/Read byte.", Description: "Configure PAD values for Slave 2 bank.")]
        public GpioPinRow[] S2_Gpios { get; set; } = CreateGpioArray();

        [Display("Slave 3 GPIOs (0-28)", Order: 3.3, Group: "Slave3, Any GPIO PAD Write/Read byte.", Description: "Configure PAD values for Slave 3 bank.")]
        public GpioPinRow[] S3_Gpios { get; set; } = CreateGpioArray();

        private static GpioPinRow[] CreateGpioArray()
        {
            var array = new GpioPinRow[29];
            for (int i = 0; i < 29; i++) array[i] = new GpioPinRow { Pin = i };
            return array;
        }

        public GpioPAD() { }

        public override void Run()
        {
            if (M1IO0.IsEnabled || M1IO1.IsEnabled || S1IO8.IsEnabled || S1IO9.IsEnabled || FLAG.IsEnabled || CTRL.IsEnabled)
            {
                ConfigureDefGpio(SelectedpAct);
            }

            ConfigureArrayGpio(0, M0_Gpios, SelectedpAct);
            ConfigureArrayGpio(1, S1_Gpios, SelectedpAct);
            ConfigureArrayGpio(2, S2_Gpios, SelectedpAct);
            ConfigureArrayGpio(3, S3_Gpios, SelectedpAct);
        }

        private void ConfigureArrayGpio(int device, GpioPinRow[] gpioArray, GpPAD selectedFunction)
        {
            string devPrefix = device == 0 ? "M0" : $"S{device}";

            for (int bit = 0; bit < gpioArray.Length; bit++)
            {
                var bitConfig = gpioArray[bit];
                if (bitConfig == null || !bitConfig.IsEnabled) continue;

                byte pvalue = bitConfig.Value;
                string name = $"{devPrefix}GP{bit}";

                // Write Execution
                if (selectedFunction == GpPAD.Write_read || selectedFunction == GpPAD.Write_only)
                {
                    string Command = $"GPIO:SETPAD:DEVICE{device}:GP{bit} {pvalue}";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read/Verify Execution
                if (selectedFunction == GpPAD.read_only || selectedFunction == GpPAD.Write_read || selectedFunction == GpPAD.read_test)
                {
                    string Command = $"GPIO:GETPAD:DEVICE{device}:GP{bit}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

                    double.TryParse(response, out double value);
                    string test = (value == Convert.ToDouble(pvalue) || selectedFunction == GpPAD.read_only) ? "PASS" : "FAIL";

                    UpgradeVerdict(test == "PASS" ? Verdict.Pass : Verdict.Fail);

                    if (PublishResults)
                    {
                        TestResult<double> result = new TestResult<double>
                        {
                            ParamName = $"{name} Bit{bit} Pad",
                            StepName = Name,
                            Value = value,
                            Verdict = test,
                            Units = (selectedFunction == GpPAD.Write_read || selectedFunction == GpPAD.read_test) ? "digcmp" : "read"
                        };

                        if (selectedFunction == GpPAD.Write_read || selectedFunction == GpPAD.read_test)
                        {
                            result.LowerLimit = pvalue;
                            result.UpperLimit = pvalue;
                        }
                        PublishResult(result);
                    }
                }
            }
        }

        public void ConfigureDefGpio(GpPAD selectedFunction)
        {
            var IOConfigs = new Dictionary<string, (byte Device, byte Pin, Enabled<byte> Prop)>
            {
                { "M1IO0", (0, 0, M1IO0) },
                { "M1IO1", (0, 1, M1IO1) },
                { "S1IO8", (1, 8, S1IO8) },
                { "S1IO9", (1, 9, S1IO9) },
                { "FLAG",  (1, 18, FLAG) },
                { "CTRL",  (1, 19, CTRL) }
            };

            foreach (var entry in IOConfigs)
            {
                if (!entry.Value.Prop.IsEnabled) continue;

                string name = entry.Key;
                byte device = entry.Value.Device;
                byte pin = entry.Value.Pin;
                byte padgpio = entry.Value.Prop.Value;

                if (selectedFunction == GpPAD.Write_read || selectedFunction == GpPAD.Write_only)
                {
                    string Command = $"GPIO:SETPAD:DEVICE{device}:GP{pin} {padgpio}";
                    Log.Info($"Sending SCPI command for {name}: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                if (selectedFunction == GpPAD.read_only || selectedFunction == GpPAD.Write_read || selectedFunction == GpPAD.read_test)
                {
                    string Command = $"GPIO:GETPAD:DEVICE{device}:GP{pin}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"Sending SCPI command for {name}: {Command}, Answer: {response}");

                    double.TryParse(response, out double value);
                    string test = "PASS";

                    if (selectedFunction != GpPAD.read_only)
                    {
                        if (value != padgpio)
                        {
                            Log.Warning($"Invalid SCPI response: {response}, expected: {padgpio}");
                            UpgradeVerdict(Verdict.Fail);
                            test = "FAIL";
                        }
                        else
                        {
                            UpgradeVerdict(Verdict.Pass);
                        }
                    }

                    if (PublishResults)
                    {
                        TestResult<double> result = new TestResult<double>
                        {
                            ParamName = $"{name} Bit{pin} Pad",
                            StepName = Name,
                            Value = value,
                            Verdict = test,
                            Units = (selectedFunction == GpPAD.Write_read || selectedFunction == GpPAD.read_test) ? "digcmp" : "read"
                        };

                        if (selectedFunction == GpPAD.Write_read || selectedFunction == GpPAD.read_test)
                        {
                            result.LowerLimit = padgpio;
                            result.UpperLimit = padgpio;
                        }
                        PublishResult(result);
                    }
                }
            }
        }


    }
}
