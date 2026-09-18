using InterconnectIOBox.Analysis;
using InterconnectIOBox.Instruments;
using OpenTap;
using System;

namespace InterconnectIOBox.GPIO
{
    [Display(Groups: new[] { "FTS_Interconnect", "GPIO" }, Name: "GPIO Input/Output Bit Write/Read", Description: "Write and/or read the state of a GPIO pin, either a dedicated GPIO or any GPIO on any device." +
    " Enable a pin by checking its checkbox and selecting a value to write and/or an expected value to read (0 or 1). The read result is then published." +
    " Be careful using the 'Any GPIO' control — it is an advanced mode and could cause malfunction.")]

    public class GpioIO : ResultTestStep
    {
        #region Settings

        public InterconnectIO IO_Instrument { get; set; }

        public enum GpIO
        {
            Write_read,
            Write_only,
            read_only,
            read_test
        }

        public enum GBit
        {
            Low = 0,
            High = 1
        }

        [Display("Action to Execute:", Group: "Gpio bit Write/Read", Order: 0.1, Description: "Write and/or read bit on selected GPIO, read data will be published.")]
        public GpIO SelectedgAct { get; set; }

        [Display("Publish Results", Order: 0.2, Group: "Options", Description: "If checked, publish the read-back verification results. If unchecked, verification still runs and affects verdict, but no result is published.")]
        public bool PublishResults { get; set; } = true;

        // --- ROW CONFIGURATION CLASS TO MANAGE LAYOUT ORDER ---
        public class GpioBitRow
        {
            // Column showing the actual GPIO bit index (0-28).
            // OpenTap's array/table editor numbers rows 1..N by default, which doesn't
            // match the 0-based GP0..GP28 naming — this column removes the ambiguity.
            // Not marked ReadOnly: OpenTap's table editor hides ReadOnly columns entirely
            // rather than showing them disabled. This value isn't used by Run() (which
            // relies on the array's loop index), so editing it here has no effect.
            [Display("GPIO #", Order: 0, Description: "GPIO bit index for this row (0-28). Informational only.")]
            public int Pin { get; set; }

            [Display("Enable", Order: 1, Description: "Enable write/read for this GPIO bit.")]
            public bool IsEnabled { get; set; } = false;

            [Display("State", Order: 2, Description: "Bit state to write and/or the expected state to read (0: Low, 1: High).")]
            public GBit Value { get; set; } = GBit.Low;
        }

        // --- DEDICATED GPIOS ---
        private const string GROUPD = "Dedicated GPIO Write/Read bit";
        private const string DDESC = "Write/Read on dedicated GPIO bit. (0: Low, 1: High)";

        [Display("M1_IO0 State:", Group: GROUPD, Order: 2, Collapsed: true, Description: DDESC)] public Enabled<GBit> M1IO0 { get; set; } = new Enabled<GBit> { Value = GBit.Low };
        [Display("M1_IO1 State:", Group: GROUPD, Order: 2.1, Collapsed: true, Description: DDESC)] public Enabled<GBit> M1IO1 { get; set; } = new Enabled<GBit> { Value = GBit.Low };
        [Display("S1_IO8 State:", Group: GROUPD, Order: 2.2, Collapsed: true, Description: DDESC)] public Enabled<GBit> S1IO8 { get; set; } = new Enabled<GBit> { Value = GBit.Low };
        [Display("S1_IO9 State:", Group: GROUPD, Order: 2.3, Collapsed: true, Description: DDESC)] public Enabled<GBit> S1IO9 { get; set; } = new Enabled<GBit> { Value = GBit.Low };
        [Display("FLAG State:", Group: GROUPD, Order: 2.4, Collapsed: true, Description: DDESC)] public Enabled<GBit> FLAG { get; set; } = new Enabled<GBit> { Value = GBit.Low };
        [Display("CTRL State:", Group: GROUPD, Order: 2.5, Collapsed: true, Description: DDESC)] public Enabled<GBit> CTRL { get; set; } = new Enabled<GBit> { Value = GBit.Low };

        // --- ANY GPIO ARRAYS (replaces 116 individual properties) ---
        private const string ANYDESC = "Write/Read on any GPIO device bit by selecting. (0: Low, 1: High)";

        [Display("Master 0 GPIOs (0-28)", Order: 3, Group: "Master0, Any GPIO bit Write/Read.", Description: ANYDESC)]
        public GpioBitRow[] M0_Gpios { get; set; } = CreateGpioArray();

        [Display("Slave 1 GPIOs (0-28)", Order: 3.1, Group: "Slave1, Any GPIO bit Write/Read.", Description: ANYDESC)]
        public GpioBitRow[] S1_Gpios { get; set; } = CreateGpioArray();

        [Display("Slave 2 GPIOs (0-28)", Order: 3.2, Group: "Slave2, Any GPIO bit Write/Read.", Description: ANYDESC)]
        public GpioBitRow[] S2_Gpios { get; set; } = CreateGpioArray();

        [Display("Slave 3 GPIOs (0-28)", Order: 3.3, Group: "Slave3, Any GPIO bit Write/Read.", Description: ANYDESC)]
        public GpioBitRow[] S3_Gpios { get; set; } = CreateGpioArray();

        private static GpioBitRow[] CreateGpioArray()
        {
            var array = new GpioBitRow[29];
            for (int i = 0; i < 29; i++) array[i] = new GpioBitRow { Pin = i };
            return array;
        }

        #endregion

        public GpioIO() { }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
        }

        public override void Run()
        {
            UpgradeVerdict(Verdict.Pass);
            if (M1IO0.IsEnabled || M1IO1.IsEnabled || S1IO8.IsEnabled || S1IO9.IsEnabled || FLAG.IsEnabled || CTRL.IsEnabled)
            {
                ConfigureDefGpio(SelectedgAct);
            }

            ConfigureArrayGpio(0, M0_Gpios, SelectedgAct);
            ConfigureArrayGpio(1, S1_Gpios, SelectedgAct);
            ConfigureArrayGpio(2, S2_Gpios, SelectedgAct);
            ConfigureArrayGpio(3, S3_Gpios, SelectedgAct);
        }

        /// <summary>
        /// Writes and/or reads any GPIO bit (0-28) on the given device.
        /// </summary>
        /// <param name="device">Device number (0 = Master, 1 = Slave1, 2 = Slave2, 3 = Slave3).</param>
        /// <param name="gpioArray">The array of bit rows configured for this device.</param>
        /// <param name="selectedFunction">Selected action (write, read, or both) to perform.</param>
        private void ConfigureArrayGpio(int device, GpioBitRow[] gpioArray, GpIO selectedFunction)
        {
            string devPrefix = device == 0 ? "M0" : $"S{device}";

            for (int bit = 0; bit < gpioArray.Length; bit++)
            {
                var bitConfig = gpioArray[bit];
                if (bitConfig == null || !bitConfig.IsEnabled) continue;

                byte bitValue = (byte)(bitConfig.Value == GBit.High ? 1 : 0);
                string name = $"{devPrefix}GP{bit}";

                // Write Command
                if (selectedFunction == GpIO.Write_read || selectedFunction == GpIO.Write_only)
                {
                    string Command = $"GPIO:OUT:DEVICE{device}:GP{bit} {bitValue}";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read Command
                if (selectedFunction == GpIO.read_only || selectedFunction == GpIO.Write_read || selectedFunction == GpIO.read_test)
                {
                    string Command = $"GPIO:IN:DEVICE{device}:GP{bit}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"SCPI query: {Command}, Answer: {response}");

                    double.TryParse(response, out double value);
                    string test = (value == bitValue || selectedFunction == GpIO.read_only) ? "PASS" : "FAIL";

                    if (test != "PASS" && selectedFunction != GpIO.read_only)
                        Log.Warning($"Invalid SCPI response for {name}: {response}, expected: {bitValue}");

                    UpgradeVerdict(test == "PASS" ? Verdict.Pass : Verdict.Fail);

                    if (PublishResults)
                    {
                        var result = new TestResult<double>
                        {
                            ParamName = $"{name} Bit{bit} Data",
                            StepName = Name,
                            Value = value,
                            Verdict = test,
                            Units = "read"
                        };

                        if (selectedFunction == GpIO.Write_read || selectedFunction == GpIO.read_test)
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
        /// Writes and/or reads a bit on the dedicated GPIOs (M1_IO0, M1_IO1, S1_IO8, S1_IO9, FLAG, CTRL).
        /// </summary>
        /// <param name="selectedFunction">Selected action (write, read, or both) to perform.</param>
        public void ConfigureDefGpio(GpIO selectedFunction)
        {
            var IOConfigs = new System.Collections.Generic.Dictionary<string, (byte Device, byte Pin, Enabled<GBit> Prop)>
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
                byte bitgpio = (byte)(entry.Value.Prop.Value == GBit.High ? 1 : 0);

                // Write Command
                if (selectedFunction == GpIO.Write_read || selectedFunction == GpIO.Write_only)
                {
                    string Command = $"GPIO:OUT:DEVICE{device}:GP{pin} {bitgpio}";
                    Log.Info($"Sending SCPI command for {name}: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read Command
                if (selectedFunction == GpIO.read_only || selectedFunction == GpIO.Write_read || selectedFunction == GpIO.read_test)
                {
                    string Command = $"GPIO:IN:DEVICE{device}:GP{pin}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"SCPI query for {name}: {Command}, Answer: {response}");

                    double.TryParse(response, out double value);
                    string test = "PASS";

                    if (selectedFunction != GpIO.read_only)
                    {
                        if (value != bitgpio)
                        {
                            Log.Warning($"Invalid SCPI response: {response}, expected: {bitgpio}");
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
                        var result = new TestResult<double>
                        {
                            ParamName = $"{name} Bit{pin} Data",
                            StepName = Name,
                            Value = value,
                            Verdict = test,
                            Units = "read"
                        };

                        if (selectedFunction == GpIO.Write_read || selectedFunction == GpIO.read_test)
                        {
                            result.LowerLimit = bitgpio;
                            result.UpperLimit = bitgpio;
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
