using InterconnectIOBox.Analysis;
using InterconnectIOBox.Instruments;
using OpenTap;
using System;
using System.Collections.Generic;

namespace InterconnectIOBox.GPIO
{
    [Display(Groups: new[] { "FTS_Interconnect", "GPIO" }, Name: "GPIO Direction Write/Read", Description: "Configure the direction of a GPIO as input or output, either a dedicated GPIO or any GPIO on any Pico device." +
    " Enable a pin by checking its checkbox and selecting a value (0 for Input, 1 for Output). The read function operates similarly — enable the bit" +
    " to verify and set the value. The read result is then published. Be careful using the 'Any GPIO' control — it is an advanced mode and could cause malfunction.")]

    public class Gpiocfg : ResultTestStep
    {
        #region Settings

        public InterconnectIO IO_Instrument { get; set; }

        public enum GpFct
        {
            Write_read,
            Write_only,
            read_only,
        }

        public enum GDir
        {
            Input,
            Output,
        }

        [Display("Configuration Action:", Group: "Cfg Control", Order: 0.1, Description: "Action to perform on the selected GPIO; read data will be published.")]
        public GpFct Selectedgfct { get; set; }

        [Display("Publish Results", Order: 0.2, Group: "Options", Description: "If checked, publish the read-back verification results. If unchecked, verification still runs and affects verdict, but no result is published.")]
        public bool PublishResults { get; set; } = true;

        // --- ROW CONFIGURATION CLASS TO MANAGE LAYOUT ORDER ---
        public class GpioDirRow
        {
            // Column showing the actual GPIO bit index (0-28).
            // OpenTap's array/table editor numbers rows 1..N by default, which doesn't
            // match the 0-based GP0..GP28 naming — this column removes the ambiguity.
            // Not used by Run() (which relies on the array's loop index) — informational only.
            [Display("GPIO #", Order: 0, Description: "GPIO bit index for this row (0-28). Informational only.")]
            public int Pin { get; set; }

            [Display("Enable", Order: 1, Description: "Enable write/read of direction for this GPIO bit.")]
            public bool IsEnabled { get; set; } = false;

            [Display("Direction", Order: 2, Description: "Direction to write and/or the expected direction to read (0: Input, 1: Output).")]
            public GDir Value { get; set; } = GDir.Input;
        }

        // --- DEDICATED GPIOS ---
        private const string GROUPD = "Dedicated GPIO Direction Read/Write";
        private const string DDESC = "Write/Read GPIO direction by selecting. (0: Input, 1: Output)";

        [Display("M1_IO0 Direction:", Group: GROUPD, Order: 2, Collapsed: true, Description: DDESC)] public Enabled<GDir> M1IO0 { get; set; } = new Enabled<GDir> { Value = GDir.Input };
        [Display("M1_IO1 Direction:", Group: GROUPD, Order: 2.1, Collapsed: true, Description: DDESC)] public Enabled<GDir> M1IO1 { get; set; } = new Enabled<GDir> { Value = GDir.Input };
        [Display("S1_IO8 Direction:", Group: GROUPD, Order: 2.2, Collapsed: true, Description: DDESC)] public Enabled<GDir> S1IO8 { get; set; } = new Enabled<GDir> { Value = GDir.Input };
        [Display("S1_IO9 Direction:", Group: GROUPD, Order: 2.3, Collapsed: true, Description: DDESC)] public Enabled<GDir> S1IO9 { get; set; } = new Enabled<GDir> { Value = GDir.Input };
        [Display("FLAG Direction:", Group: GROUPD, Order: 2.4, Collapsed: true, Description: DDESC)] public Enabled<GDir> FLAG { get; set; } = new Enabled<GDir> { Value = GDir.Input };
        [Display("CTRL Direction:", Group: GROUPD, Order: 2.5, Collapsed: true, Description: DDESC)] public Enabled<GDir> CTRL { get; set; } = new Enabled<GDir> { Value = GDir.Input };

        // --- ANY GPIO ARRAYS (replaces 116 individual properties) ---
        private const string ANYDESC = "Write/Read on any GPIO direction by selecting. (0: Input, 1: Output)";

        [Display("Master 0 GPIOs (0-28)", Order: 3, Group: "Master0, Any GPIO Direction Read/Write.", Description: ANYDESC)]
        public GpioDirRow[] M0_Gpios { get; set; } = CreateGpioArray();

        [Display("Slave 1 GPIOs (0-28)", Order: 3.1, Group: "Slave1, Any GPIO Direction Read/Write.", Description: ANYDESC)]
        public GpioDirRow[] S1_Gpios { get; set; } = CreateGpioArray();

        [Display("Slave 2 GPIOs (0-28)", Order: 3.2, Group: "Slave2, Any GPIO Direction Read/Write.", Description: ANYDESC)]
        public GpioDirRow[] S2_Gpios { get; set; } = CreateGpioArray();

        [Display("Slave 3 GPIOs (0-28)", Order: 3.3, Group: "Slave3, Any GPIO Direction Read/Write.", Description: ANYDESC)]
        public GpioDirRow[] S3_Gpios { get; set; } = CreateGpioArray();

        private static GpioDirRow[] CreateGpioArray()
        {
            var array = new GpioDirRow[29];
            for (int i = 0; i < 29; i++) array[i] = new GpioDirRow { Pin = i };
            return array;
        }

        #endregion

        public Gpiocfg() { }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
        }

        public override void Run()
        {
            if (M1IO0.IsEnabled || M1IO1.IsEnabled || S1IO8.IsEnabled || S1IO9.IsEnabled || FLAG.IsEnabled || CTRL.IsEnabled)
            {
                ConfigureDefGpio(Selectedgfct);
            }

            ConfigureArrayGpio(0, M0_Gpios, Selectedgfct);
            ConfigureArrayGpio(1, S1_Gpios, Selectedgfct);
            ConfigureArrayGpio(2, S2_Gpios, Selectedgfct);
            ConfigureArrayGpio(3, S3_Gpios, Selectedgfct);
        }

        /// <summary>
        /// Writes and/or reads the direction of any GPIO bit (0-28) on the given device.
        /// </summary>
        /// <param name="device">Device number (0 = Master, 1 = Slave1, 2 = Slave2, 3 = Slave3).</param>
        /// <param name="gpioArray">The array of direction rows configured for this device.</param>
        /// <param name="selectedFunction">Selected action (write, read, or both) to perform.</param>
        private void ConfigureArrayGpio(int device, GpioDirRow[] gpioArray, GpFct selectedFunction)
        {
            string devPrefix = device == 0 ? "M0" : $"S{device}";

            for (int bit = 0; bit < gpioArray.Length; bit++)
            {
                var bitConfig = gpioArray[bit];
                if (bitConfig == null || !bitConfig.IsEnabled) continue;

                byte bitValue = (byte)(bitConfig.Value == GDir.Output ? 1 : 0);
                string name = $"{devPrefix}GP{bit}";

                // Write Command
                if (selectedFunction == GpFct.Write_read || selectedFunction == GpFct.Write_only)
                {
                    string Command = $"GPIO:DIRECTION:DEVICE{device}:GP{bit} {bitValue}";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read Command
                if (selectedFunction == GpFct.read_only || selectedFunction == GpFct.Write_read)
                {
                    string Command = $"GPIO:DIRECTION:DEVICE{device}:GP{bit}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"SCPI query: {Command}, Answer: {response}");

                    double.TryParse(response, out double value);
                    string test = value == bitValue ? "PASS" : "FAIL";

                    if (test == "PASS")
                    {
                        UpgradeVerdict(Verdict.Pass);
                    }
                    else
                    {
                        Log.Warning($"Invalid SCPI response for {name}: {response}, expected: {bitValue}");
                        UpgradeVerdict(Verdict.Fail);
                    }

                    if (PublishResults)
                    {
                        var result = new TestResult<double>
                        {
                            ParamName = $"{name} Direction",
                            StepName = Name,
                            Value = value,
                            Verdict = test,
                            Units = "read"
                        };

                        if (selectedFunction == GpFct.Write_read)
                        {
                            result.LowerLimit = bitValue;
                            result.UpperLimit = bitValue;
                            result.Units = "digcmp";
                        }

                        PublishResult(result);
                    }
                }
            }
        }

        /// <summary>
        /// Writes and/or reads the direction of the dedicated GPIOs (M1_IO0, M1_IO1, S1_IO8, S1_IO9, FLAG, CTRL).
        /// </summary>
        /// <param name="selectedFunction">Selected action (write, read, or both) to perform.</param>
        public void ConfigureDefGpio(GpFct selectedFunction)
        {
            var IOConfigs = new Dictionary<string, (byte Device, byte Pin, Enabled<GDir> Prop)>
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
                byte dirgpio = (byte)(entry.Value.Prop.Value == GDir.Output ? 1 : 0);

                // Write Command
                if (selectedFunction == GpFct.Write_read || selectedFunction == GpFct.Write_only)
                {
                    string Command = $"GPIO:DIRECTION:DEVICE{device}:GP{pin} {dirgpio}";
                    Log.Info($"Sending SCPI command for {name}: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read Command
                if (selectedFunction == GpFct.read_only || selectedFunction == GpFct.Write_read)
                {
                    string Command = $"GPIO:DIRECTION:DEVICE{device}:GP{pin}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"SCPI query for {name}: {Command}, Answer: {response}");

                    double.TryParse(response, out double value);
                    string test = value == dirgpio ? "PASS" : "FAIL";

                    if (test == "PASS")
                    {
                        UpgradeVerdict(Verdict.Pass);
                    }
                    else
                    {
                        Log.Warning($"Invalid SCPI response: {response}, expected: {dirgpio}");
                        UpgradeVerdict(Verdict.Fail);
                    }

                    if (PublishResults)
                    {
                        var result = new TestResult<double>
                        {
                            ParamName = $"{name} Direction",
                            StepName = Name,
                            Value = value,
                            Verdict = test,
                            Units = "read"
                        };

                        if (selectedFunction == GpFct.Write_read)
                        {
                            result.LowerLimit = dirgpio;
                            result.UpperLimit = dirgpio;
                            result.Units = "digcmp";
                        }

                        PublishResult(result);
                    }
                }
            }
        }

        public override void PostPlanRun()
        {
            base.PostPlanRun();
        }
    }
}
