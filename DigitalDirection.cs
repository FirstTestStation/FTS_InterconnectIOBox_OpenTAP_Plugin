
using OpenTap;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace InterconnectIOBox
{
        [Display(Groups: new[] { "InterconnectIO", "Digital" }, Name: "Dual Ports 8-bits Direction Write/Read", Description: "Configure direction of Port0 and/or Port1 as Input or Output. Configuration can be specified using a byte value in decimal, hexadecimal, or binary, " +
        "or by setting individual bits for each port.Enabling byte or bit configuration is done by checking the enable checkbox and selecting a value(0 for Input, 1 for Output). The read function operates similarly—enable the byte or bit" +
        " to verify and set the value.The read result is then published.")]

    public class Portscfg : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        public InterconnectIO IO_Instrument { get; set; }
        public enum DigFct
        {
            Write_read,
            Write_only,
            read_only,
        }


        [Display("Configuration Action:", Group: "Cfg Control",Order: 0.1, Description: "Action to perform on selected ports, read data will be published.")]
        public DigFct Selectedfct { get; set; }

        [Display("Port0 I/O Byte Cfg:", Group: "Port I/O Read/Write Cfg using Byte value", Order: 2, Collapsed: true, Description: "Set/Read Port0 direction using decimal, hexadecimal, or binary values.(0: Input, 1: Output)")]
        public Enabled<byte> P0Byte { get; set; }

        [Display("Port1 I/O Byte Cfg:", Group: "Port I/O Read/Write Cfg using Byte value", Order: 2.1, Collapsed: true, Description: "Set/Read Port1 direction using decimal, hexadecimal, or binary values.(0: Input, 1: Output)")]
        public Enabled<byte> P1Byte { get; set; }

        public enum BitDir
        {
            Input,
            Output,
        }

        [Display("Port0 Bit7 Cfg:", Group: "Port0 I/O Settings using Bit value", Order: 2, Collapsed: true, Description: "Set/Read Direction of Port0 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P0b7 { get; set; } 

        [Display("Port0 Bit6 Cfg:", Group: "Port0 I/O Settings using Bit value", Order: 2.1,Collapsed: true, Description: "Set/Read Direction of Port0 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P0b6 { get; set; }

        [Display("Port0 Bit5 Cfg:", Group: "Port0 I/O Settings using Bit value", Order: 2.2, Collapsed: true, Description: "Set/Read Direction of Port0 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P0b5 { get; set; }

        [Display("Port0 Bit4 Cfg:", Group: "Port0 I/O Settings using Bit value", Order: 2.3, Collapsed: true, Description: "Set/Read Direction of Port0 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P0b4 { get; set; }

        [Display("Port0 Bit3 Cfg:", Group: "Port0 I/O Settings using Bit value", Order: 2.4, Collapsed: true, Description: "Set/Read Direction of Port0 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P0b3 { get; set; }

        [Display("Port0 Bit2 Cfg:", Group: "Port0 I/O Settings using Bit value", Order: 2.5, Collapsed: true, Description: "Set/Read Direction of Port0 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P0b2 { get; set; }

        [Display("Port0 Bit1 Cfg:", Group: "Port0 I/O Settings using Bit value", Order: 2.6, Collapsed: true, Description: "Set/Read Direction of Port0 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P0b1 { get; set; }

        [Display("Port0 Bit0 Cfg:", Group: "Port0 I/O Settings using Bit value", Order: 2.7, Collapsed: true, Description: "Set/Read Direction of Port0 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P0b0 { get; set; }

        [Display("Port1 Bit7 Cfg:", Group: "Port1 I/O Settings using Bit value", Order: 3, Collapsed: true, Description: "Set/Read Direction of Port1 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P1b7 { get; set; }

        [Display("Port1 Bit6 Cfg:", Group: "Port1 I/O Settings using Bit value", Order: 3.1, Collapsed: true, Description: "Set/Read Direction ofPort1 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P1b6 { get; set; }

        [Display("Port1 Bit5 Cfg:", Group: "Port1 I/O Settings using Bit value", Order: 3.2, Collapsed: true, Description: "Set/Read Direction of Port1 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P1b5 { get; set; }

        [Display("Port1 Bit4 Cfg:", Group: "Port1 I/O Settings using Bit value", Order: 3.3, Collapsed: true, Description: "Set/Read Direction of Port1 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P1b4 { get; set; }

        [Display("Port1 Bit3 Cfg:", Group: "Port1 I/O Settings using Bit value", Order: 3.4, Collapsed: true, Description: "Set/Read Direction of Port1 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P1b3 { get; set; }

        [Display("Port1 Bit2 Cfg:", Group: "Port1 I/O Settings using Bit value", Order: 3.5, Collapsed: true, Description: "Set/Read Direction of Port1 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P1b2 { get; set; }

        [Display("Port1 Bit1 Cfg:", Group: "Port1 I/O Settings using Bit value", Order: 3.6, Collapsed: true, Description: "Set/Read Direction of Port1 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P1b1 { get; set; }

        [Display("Port1 Bit0 Cfg:", Group: "Port1 I/O Settings using Bit value", Order: 3.7, Collapsed: true, Description: "Set/Read Direction of Port1 by selecting bits(0: Input, 1: Output)")]
        public Enabled<BitDir> P1b0 { get; set; }

        public Portscfg()
        {
            P0Byte = new Enabled<byte>() { IsEnabled = false, Value = 0 };
            P1Byte = new Enabled<byte>() { IsEnabled = false, Value = 0 };
            P0b7 = new Enabled<BitDir> { Value = BitDir.Input };
            P0b6 = new Enabled<BitDir> { Value = BitDir.Input };
            P0b5 = new Enabled<BitDir> { Value = BitDir.Input };
            P0b4 = new Enabled<BitDir> { Value = BitDir.Input };
            P0b3 = new Enabled<BitDir> { Value = BitDir.Input };
            P0b2 = new Enabled<BitDir> { Value = BitDir.Input };
            P0b1 = new Enabled<BitDir> { Value = BitDir.Input };
            P0b0 = new Enabled<BitDir> { Value = BitDir.Input };
            P1b7 = new Enabled<BitDir> { Value = BitDir.Input };
            P1b6 = new Enabled<BitDir> { Value = BitDir.Input };
            P1b5 = new Enabled<BitDir> { Value = BitDir.Input };
            P1b4 = new Enabled<BitDir> { Value = BitDir.Input };
            P1b3 = new Enabled<BitDir> { Value = BitDir.Input };
            P1b2 = new Enabled<BitDir> { Value = BitDir.Input };
            P1b1 = new Enabled<BitDir> { Value = BitDir.Input };
            P1b0 = new Enabled<BitDir> { Value = BitDir.Input };
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }



        public override void Run()
        {
            if ((P0Byte.IsEnabled == true) || (P1Byte.IsEnabled == true))
            {
                // Configure Port0 if enabled
                if (P0Byte.IsEnabled)
                {
                    ConfigurePortByte(0, P0Byte.Value, Selectedfct);
                }

                // Configure Port1 if enabled
                if (P1Byte.IsEnabled)
                {
                    ConfigurePortByte(1, P1Byte.Value, Selectedfct);
                }
            } else
            {
                if ((P0b7.IsEnabled == true) || (P0b6.IsEnabled == true) || (P0b5.IsEnabled == true) || (P0b4.IsEnabled == true) || (P0b3.IsEnabled == true) || (P0b2.IsEnabled == true) || (P0b1.IsEnabled == true) || (P0b0.IsEnabled == true))
                {
                    ConfigurePortBits(0, Selectedfct);
                }
                if ((P1b7.IsEnabled == true) || (P1b6.IsEnabled == true) || (P1b5.IsEnabled == true) || (P1b4.IsEnabled == true) || (P1b3.IsEnabled == true) || (P1b2.IsEnabled == true) || (P1b1.IsEnabled == true) || (P1b0.IsEnabled == true))
                {
                    ConfigurePortBits(1, Selectedfct);
                }
            }
        }


        /// <summary>
        /// Configures a port using a byte value.
        /// </summary>
        /// <param name="portNumber">Port number (0 for Port0, 1 for Port1).</param>
        /// <param name="portValue">The byte value for configuration.</param>
        /// <param name="selectedFunction">Selected function for the port.</param>
        private void ConfigurePortByte(int portNumber, byte portValue, DigFct selectedFunction)
        {
            if ((selectedFunction == DigFct.Write_read) || (selectedFunction == DigFct.Write_only))
            {
                string Command = $"DIGITAL:DIRECTION:PORT{portNumber} {portValue}";
                Log.Info($"Sending SCPI command: {Command}");
                IO_Instrument.ScpiCommand(Command); // Send SCPI command to configure the ports
                UpgradeVerdict(Verdict.Pass);
            }


            if ((selectedFunction == DigFct.read_only) || (selectedFunction == DigFct.Write_read))
            {
                string Command = $"DIGITAL:DIRECTION:PORT{portNumber}?";
                string response = IO_Instrument.ScpiQuery<string>(Command);
                Log.Info($"Sending SCPI command: {Command}, Answer: {response}");
                double.TryParse(response, out double value);
                string test = "FAIL";

                if (value == portValue)
                {
                    Log.Info($"Port{portNumber} Direction configured as {portValue}");
                    UpgradeVerdict(Verdict.Pass);
                    test = "PASS";
                }
                else
                {
                    Log.Error($"Port{portNumber} Direction configuration failed. Expected: {portValue}, Actual: {value}");
                    UpgradeVerdict(Verdict.Fail);
                }

                // Basic publish parameters for read only
                TestResult<double> result = new TestResult<double>
                {
                    ParamName = $"Port{portNumber} Direction",
                    StepName = Name,
                    Value = value,
                    Verdict = test,
                    Units = "read"

                };

                // limit are added only for write_read function
                if (selectedFunction == DigFct.Write_read)
                {
                    result.LowerLimit = portValue;
                    result.UpperLimit = portValue;
                    result.Units= "digcmp";
                }

                PublishResult(result);

            }
           
        }

        // <summary>
        /// Configures a port using a bit value.
        /// </summary>
        /// <param name="portNumber">Port number (0 for Port0, 1 for Port1).</param>
        /// <param name="selectedFunction">Selected function for the port.</param>
        /// 
        public void ConfigurePortBits(int portNumber, DigFct selectedFunction)
        {
            for (int bit = 0; bit < 8; bit++)
            {
                // Skip bits that are not enabled
                if (!IsBitEnabled(portNumber, bit)) continue;

                byte bitValue = GetPortBitValue(portNumber, bit);
                bool bitState = (bitValue == 1);
                string bitCommand = bitState ? "1" : "0";

                // Write Command
                if ((selectedFunction == DigFct.Write_read) || (selectedFunction == DigFct.Write_only))
                {
                    string Command = $"DIGITAL:DIRECTION:PORT{portNumber}:BIT{bit} {bitCommand}";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read Command
                if ((selectedFunction == DigFct.read_only) || (selectedFunction == DigFct.Write_read))
                {
                    string Command = $"DIGITAL:DIRECTION:PORT{portNumber}:BIT{bit}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

                    double.TryParse(response, out double value);
                    string test = (value == Convert.ToDouble(bitState)) ? "PASS" : "FAIL";

                    if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                    else UpgradeVerdict(Verdict.Fail);

                    // Create test result object
                    TestResult<double> result = new TestResult<double>
                    {
                        ParamName = $"Port{portNumber} Bit{bit} Direction",
                        StepName = Name,
                        Value = value,
                        Verdict = test,
                        Units = "read"
                    };

                    // Add limits for Write_Read function
                    if (selectedFunction == DigFct.Write_read)
                    {
                        result.LowerLimit = bitValue;
                        result.UpperLimit = bitValue;
                        result.Units = "digcmp";
                    }

                    PublishResult(result);
                }
            }
        }

        private byte GetPortBitValue(int portNumber, int bit)
        {
            string propertyName = $"P{portNumber}b{bit}";
            var property = GetType().GetProperty(propertyName);

            if (property != null)
            {
                var bitConfig = property.GetValue(this) as Enabled<BitDir>;
                if (bitConfig?.IsEnabled == true)
                {
                    return (byte)(bitConfig.Value == BitDir.Output ? 1 : 0);
                }
            }
            return 0; // Default to input (0)
        }

        private bool IsBitEnabled(int portNumber, int bit)
        {
            string propertyName = $"P{portNumber}b{bit}";
            var property = GetType().GetProperty(propertyName);

            return property?.GetValue(this) is Enabled<BitDir> bitConfig && bitConfig.IsEnabled;
        }



        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}
