
using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace InterconnectIOBox
{
        [Display(Groups: new[] { "InterconnectIO", "Digital" }, Name: "Dual Ports 8-bits  Write/Read", Description: "Write or Read data of Port0 and/or Port1 configured as Input or Output. Set or Read data can be specified using a byte value in decimal, hexadecimal, or binary, " +
        "or by setting individual bits for each port. Enabling byte or bit Write/Read is done by checking the enable checkbox and selecting a binary value. The read function operates similarly—enable the byte or bit" +
        " to verify and set the value.The read result is then published.")]

    public class PortsIO : ResultTestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        [Output]
        [Display("Data Read_only")]
        public byte ReadData { get; private set; }

        public InterconnectIO IO_Instrument { get; set; }
        public enum DigAct
        {
            Write_read,
            Write_only,
            read_only,
            read_test
        }


        [Display("Action to Execute:", Group: "Ports byte Write/Read ",Order: 0.5, Description: "Write and/or read byte on selected ports, read data will be published.")]
        public DigAct SelectedAct { get; set; }

        [Display("Port0 Byte:", Group: "Port I/O Write/Read using Byte value", Order: 2, Collapsed: true, Description: "Write/Read Port0 byte value using decimal, hexadecimal, or binary values.(0: Input, 1: Output)")]
        public Enabled<byte> P0byte { get; set; }

        [Display("Port1 Byte:", Group: "Port I/O Write/Read using Byte value", Order: 2.1, Collapsed: true, Description: "Write/Read Port0 byte value using decimal, hexadecimal, or binary values.(0: Input, 1: Output)")]
        public Enabled<byte> P1byte { get; set; }

        public enum Bit
        {
            Low = 0,
            High = 1
        }

        [Display("Port0 Bit7 State:", Group: "Port0 I/O using Bit value", Order: 2, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P0B7 { get; set; } 

        [Display("Port0 Bit6 State:", Group: "Port0 I/O using Bit value", Order: 2.1,Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P0B6 { get; set; }

        [Display("Port0 Bit5 State:", Group: "Port0 I/O using Bit value", Order: 2.2, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P0B5 { get; set; }

        [Display("Port0 Bit4 State:", Group: "Port0 I/O using Bit value", Order: 2.3, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P0B4 { get; set; }

        [Display("Port0 Bit3 State:", Group: "Port0 I/O using Bit value", Order: 2.4, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P0B3 { get; set; }

        [Display("Port0 Bit2 State:", Group: "Port0 I/O using Bit value", Order: 2.5, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P0B2 { get; set; }

        [Display("Port0 Bit1 State:", Group: "Port0 I/O using Bit value", Order: 2.6, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P0B1 { get; set; }

        [Display("Port0 Bit0 State:", Group: "Port0 I/O using Bit value", Order: 2.7, Collapsed: true, Description: "Write / Read of Port0 by selecting bits(0:Low, 1: High)")]
        public Enabled<Bit> P0B0 { get; set; }

        [Display("Port1 Bit7 State:", Group: "Port1 I/O using Bit value", Order: 3, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P1B7 { get; set; }

        [Display("Port1 Bit6 State:", Group: "Port1 I/O using Bit value", Order: 3.1, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P1B6 { get; set; }

        [Display("Port1 Bit5 State:", Group: "Port1 I/O using Bit value", Order: 3.2, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P1B5 { get; set; }

        [Display("Port1 Bit4 State:", Group: "Port1 I/O using Bit value", Order: 3.3, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P1B4 { get; set; }

        [Display("Port1 Bit3 State:", Group: "Port1 I/O using Bit value", Order: 3.4, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P1B3 { get; set; }

        [Display("Port1 Bit2 State:", Group: "Port1 I/O using Bit value", Order: 3.5, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P1B2 { get; set; }

        [Display("Port1 Bit1 State:", Group: "Port1 I/O using Bit value", Order: 3.6, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P1B1 { get; set; }

        [Display("Port1 Bit0 State:", Group: "Port1 I/O using Bit value", Order: 3.7, Collapsed: true, Description: "Write/Read of Port0 by selecting bits (0:Low, 1: High)")]
        public Enabled<Bit> P1B0 { get; set; }

        public PortsIO()
        {
            P0byte = new Enabled<byte>() { IsEnabled = false, Value = 0 };
            P1byte = new Enabled<byte>() { IsEnabled = false, Value = 0 };
            P0B7 = new Enabled<Bit> { Value = Bit.Low };
            P0B6 = new Enabled<Bit> { Value = Bit.Low };
            P0B5 = new Enabled<Bit> { Value = Bit.Low };
            P0B4 = new Enabled<Bit> { Value = Bit.Low };
            P0B3 = new Enabled<Bit> { Value = Bit.Low };
            P0B2 = new Enabled<Bit> { Value = Bit.Low };
            P0B1 = new Enabled<Bit> { Value = Bit.Low };
            P0B0 = new Enabled<Bit> { Value = Bit.Low };
            P1B7 = new Enabled<Bit> { Value = Bit.Low };
            P1B6 = new Enabled<Bit> { Value = Bit.Low };
            P1B5 = new Enabled<Bit> { Value = Bit.Low };
            P1B4 = new Enabled<Bit> { Value = Bit.Low };
            P1B3 = new Enabled<Bit> { Value = Bit.Low };
            P1B2 = new Enabled<Bit> { Value = Bit.Low };
            P1B1 = new Enabled<Bit> { Value = Bit.Low };
            P1B0 = new Enabled<Bit> { Value = Bit.Low };
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }



        public override void Run()
        {
            if ((P0byte.IsEnabled == true) || (P1byte.IsEnabled == true))
            {
                // Check Port0 if enabled
                if (P0byte.IsEnabled)
                {
                    WriteReadPortByte(0, P0byte.Value, SelectedAct);
                }

                // Configure Port1 if enabled
                if (P1byte.IsEnabled)
                {
                    WriteReadPortByte(1, P1byte.Value, SelectedAct);
                }
            } else
            {
                if ((P0B7.IsEnabled == true) || (P0B6.IsEnabled == true) || (P0B5.IsEnabled == true) || (P0B4.IsEnabled == true) || (P0B3.IsEnabled == true) || (P0B2.IsEnabled == true) || (P0B1.IsEnabled == true) || (P0B0.IsEnabled == true))
                {
                    WriteReadPortBits(0, SelectedAct);
                }
                if ((P1B7.IsEnabled == true) || (P1B6.IsEnabled == true) || (P1B5.IsEnabled == true) || (P1B4.IsEnabled == true) || (P1B3.IsEnabled == true) || (P1B2.IsEnabled == true) || (P1B1.IsEnabled == true) || (P1B0.IsEnabled == true))
                {
                    WriteReadPortBits(1, SelectedAct);
                }
            }
        }


        /// <summary>
        /// Write or Read a port using a byte value.
        /// </summary>
        /// <param name="portNumber">Port number (0 for Port0, 1 for Port1).</param>
        /// <param name="portValue">The byte value for configuration.</param>
        /// <param name="selectedFunction">Selected function for the port.</param>
        private void WriteReadPortByte(int portNumber, byte portValue, DigAct selectedFunction)
        {
            if ((selectedFunction == DigAct.Write_read) || (selectedFunction == DigAct.Write_only))
            {
                string Command = $"DIGITAL:OUT:PORT{portNumber} {portValue}";
                Log.Info($"Sending SCPI command: {Command}");
                IO_Instrument.ScpiCommand(Command); // Send SCPI command to configure the ports
                UpgradeVerdict(Verdict.Pass);
            }


            if ((selectedFunction == DigAct.read_only) || (selectedFunction == DigAct.Write_read) || (selectedFunction == DigAct.read_test))
            {
                string Command = $"DIGITAL:IN:PORT{portNumber}?";
                string response = IO_Instrument.ScpiQuery<string>(Command);
                Log.Info($"Sending SCPI command: {Command}, Answer: {response}");
                double.TryParse(response, out double value);
                string test = "FAIL";

                if ((selectedFunction == DigAct.read_test) || (selectedFunction == DigAct.Write_read))
                { 
                    if (value == portValue)
                    {
                        Log.Info($"Port{portNumber} Input value match as {portValue}");
                        UpgradeVerdict(Verdict.Pass);
                        test = "PASS";
                    }
                    else
                    {
                        Log.Error($"Port{portNumber} Input failed. Expected: {portValue}, Actual: {value}");
                        UpgradeVerdict(Verdict.Fail);
                    }
                }
                else
                {
                    UpgradeVerdict(Verdict.Pass); // read_only
                    test = "PASS";
                    ReadData = (byte)value;

                }

                // Basic publish parameters for read only
                TestResult<double> result = new TestResult<double>
                {
                    ParamName = $"Port{portNumber} Data",
                    StepName = Name,
                    Value = value,
                    Verdict = test,
                    Units = "read"

                };

                // limit are added only for write_read function
                if ((selectedFunction == DigAct.Write_read) || (selectedFunction == DigAct.read_test))
                {
                    result.LowerLimit = portValue;
                    result.UpperLimit = portValue;
                    result.Units= "cmp";
                }

                PublishResult(result);

            }
           
        }

        // <summary>
        /// Write and/or read a port using a bit value.
        /// </summary>
        /// <param name="portNumber">Port number (0 for Port0, 1 for Port1).</param>
        /// <param name="selectedFunction">Selected function for the port.</param>
        /// 
        public void WriteReadPortBits(int portNumber, DigAct selectedFunction)
        {
            for (int bit = 0; bit < 8; bit++)
            {
                // Skip bits that are not enabled
                if (!IsBitEnabled(portNumber, bit)) continue;

                byte bitValue = GetPortBitValue(portNumber, bit);
                bool bitState = (bitValue == 1);
                string bitCommand = bitState ? "1" : "0";

                // Write Command
                if ((selectedFunction == DigAct.Write_read) || (selectedFunction == DigAct.Write_only))
                {
                    string Command = $"DIGITAL:OUT:PORT{portNumber}:BIT{bit} {bitCommand}";
                    Log.Info($"Sending SCPI command: {Command}");
                    IO_Instrument.ScpiCommand(Command);
                    UpgradeVerdict(Verdict.Pass);
                }

                // Read Command
                if ((selectedFunction == DigAct.read_only) || (selectedFunction == DigAct.Write_read) || (selectedFunction == DigAct.read_test))
                {
                    string Command = $"DIGITAL:IN:PORT{portNumber}:BIT{bit}?";
                    string response = IO_Instrument.ScpiQuery<string>(Command);
                    Log.Info($"Sending SCPI command: {Command}, Answer: {response}");

                    double.TryParse(response, out double value);
                    string test = (value == Convert.ToDouble(bitState)) ? "PASS" : "FAIL";


                    if (selectedFunction == DigAct.read_only)
                    {
                        test = "PASS"; // No verdict on read only
                    }

                   // Create test result object
                   TestResult<double> result = new TestResult<double>
                     {
                       ParamName = $"Port{portNumber} Bit{bit} Data",
                       StepName = Name,
                       Value = value,
                       Verdict = test,
                       Units= "read"
                     };

                    if (test == "PASS") UpgradeVerdict(Verdict.Pass);
                    else UpgradeVerdict(Verdict.Fail);


                    // Add limits for Write_Read function
                    if ((selectedFunction == DigAct.Write_read) || (selectedFunction == DigAct.read_test))
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
            string propertyName = $"P{portNumber}B{bit}";
            var property = GetType().GetProperty(propertyName);

            if (property != null)
            {
                var bitConfig = property.GetValue(this) as Enabled<Bit>;
                if (bitConfig?.IsEnabled == true)
                {
                    return (byte)(bitConfig.Value == Bit.High ? 1 : 0);
                }
            }
            return 0; // Default to input (0)
        }

        private bool IsBitEnabled(int portNumber, int bit)
        {
            string propertyName = $"P{portNumber}B{bit}";
            var property = GetType().GetProperty(propertyName);

            return property?.GetValue(this) is Enabled<Bit> bitConfig && bitConfig.IsEnabled;
        }



        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}
