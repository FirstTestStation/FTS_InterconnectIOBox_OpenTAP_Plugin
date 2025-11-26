using InterconnectIOBox;
using OpenTap;
using System;
using System.Data;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace InterconnectIOBox
{
    [Display(Groups: new[] { "InterconnectIO", "Communication" },
             Name: "Data Analyze",
             Description: "Analyze Data from Device to be tested between Low and High limit")]
    public class DataAnalyse : ResultTestStep
    {
        // ===========================================
        // USER INPUTS
        // ===========================================
        public enum Com
        {
            None,
            I2C,
            SPI,
        }

        [Display( "Com Protocol",Order: 0.4,Description: "Select the communication protocol used to read data from the device. The acquired data will be published.")]
        public Com SelectedCom { get; set; }


        private const string Rg = "Read Data";
        [Display("Register Address:", Group: Rg ,Description: "Starting Register to where the data will be read", Order: 1)]
        public int Register { get; set; }

        [Display("Read Length:", Group: Rg, Description: "Starting Register to where the data will be read", Order: 3)]
        public int ReadLength { get; set; } = 1;

        // ---------------- ANALYZE -------------------

        private const string Rt = "Calculate Value";
        [Display("Bit Start",Order: 4.1, Group: Rt, Description: "Index of the first bit inside the raw I2C/SPI data. Bit 0 = LSB of byte[0].")]
        public int BitStart { get; set; } = 0;

        [Display("Bit Length",Order: 4.2, Group: Rt, Description: "Number of bits to extract starting from Bit Start.")]
        public int BitLength { get; set; } = 16;

        [Display("Signed Value",Order: 4.3, Group: Rt, Description: "Enable if the extracted value uses two’s complement format.")]
        public bool Signed { get; set; } = false;

        [Display("Equation (use {R})", Order: 4.4, Group: Rt, Description: "Math expression applied to the raw value. Example: ({R}/16)*0.01")]
        public string Equation { get; set; } = "{R}";

        [Display("Units", Order: 4.5, Group: Rt, Description: "Engineering units for the computed value. ")]
        public string EngineeringUnits { get; set; } = "";


        private const string Rv = "Validate Eng Value";
        [Display("Low Limit", Order: 4.5, Group: Rv, Description: "Minimum allowed engineering value after applying the equation.")]
        public double LowLimit { get; set; }

        [Display("High Limit", Order: 4.6, Group: Rv, Description: "Maximum allowed engineering value after applying the equation.")]
        public double HighLimit { get; set; }

        // ===========================================
        // OUTPUTS
        // ===========================================


        private const string Rdo = "Data Output";
        [Output]
        [Display("Raw Bytes", Order: 10.1, Group: Rdo, Description: "Raw I2C/SPI bytes returned by the device.")]
        public byte[] RawBytes { get; private set; }

        [Output]
        [Display("Raw Value", Order: 10.2, Group: Rdo, Description: "Extracted bitfield converted to integer.")]
        public long RawValue { get; private set; }

        [Output]
        [Display("Engineering Value", Order: 10.3, Group: Rdo, Description: "Value after equation processing.")]
        public double EngValue { get; private set; }


        public InterconnectIO IO_Instrument { get; set; }

        // ===========================================
        // RUN METHOD
        // ===========================================
        public override void Run()
        {
            string response = "";
            RawBytes = Array.Empty<byte>();


            string readCmd = "";

            switch (SelectedCom)
            {
                case Com.I2C:
                    readCmd = $"COM:I2C:READ:LEN{ReadLength}? {Register}";
                    break;

                case Com.SPI:
                    byte wreg = (byte)(Register | 0x80);
                    Log.Info($"SPI register: {Register} become: {wreg} with bit 7 added");
                    readCmd = $"COM:SPI:READ:LEN{ReadLength}? {wreg}";
                    break;

                default:
                    Log.Error("Communication protocol not selected.");
                    UpgradeVerdict(Verdict.Error);
                    return;
            }

            try
            {
                Log.Info($"SCPI → {readCmd}");
                response = IO_Instrument.ScpiQuery<string>(readCmd);

                if (string.IsNullOrWhiteSpace(response))
                {
                    Log.Error("Empty response received from instrument.");
                    UpgradeVerdict(Verdict.Fail);
                    return;
                }

                Log.Info($"SCPI Response = {response}");
            }
            catch (TimeoutException ex)
            {
                Log.Error($"Communication timeout: {ex.Message}");
                UpgradeVerdict(Verdict.Error);
                return;
            }
            catch (Exception ex)
            {
                Log.Error($"Unexpected communication error: {ex.Message}");
                UpgradeVerdict(Verdict.Error);
                return;
            }



            // ---------------- PARSE BYTES ----------------
            RawBytes = ParseResponse(response);
            Log.Info($"Parsed Hexadecimal Bytes → {BitConverter.ToString(RawBytes)}");

            RawValue = ExtractBits(RawBytes, BitStart, BitLength, Signed);
            EngValue = ComputeEquation(RawValue);

            EngValue = Math.Round(EngValue, 4);

            bool ok = EngValue >= LowLimit && EngValue <= HighLimit;
            Log.Info($"Raw={RawValue}, EngValue={EngValue}, Limits[{LowLimit},{HighLimit}] → {(ok ? "PASS" : "FAIL")}");
            UpgradeVerdict(ok ? Verdict.Pass : Verdict.Fail);


            var result = new TestResult<double>
            {
                ParamName = $"Register 0x{Register:X2} Value",
                StepName = Name,
                Value = EngValue,
                Verdict = ok ? "PASS" : "FAIL",
                Units = EngineeringUnits, 
                LowerLimit = LowLimit,
                UpperLimit = HighLimit
            };

            PublishResult(result);
            return;





        }

        // ===========================================
        // UTILITIES
        // ===========================================
     
        private byte[] ParseResponse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return Array.Empty<byte>();

            return s.Split(',', ' ', '\t', '\n', '\r')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x =>
                    {
                        if (x.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                            return Convert.ToByte(x, 16);
                        return byte.Parse(x);
                    })
                    .ToArray();
        }

        private long ExtractBits(byte[] data, int start, int length, bool signed)
        {
            long value = 0;
            for (int i = 0; i < length; i++)
            {
                int bitPos = start + i;
                int byteIndex = bitPos / 8;
                int bitIndex = bitPos % 8;

                if (byteIndex >= data.Length) break;

                int bit = (data[byteIndex] >> bitIndex) & 1;
                value |= ((long)bit << i);
            }

            if (signed)
            {
                long sign = 1L << (length - 1);
                if ((value & sign) != 0) value -= (1L << length);
            }

            return value;
        }

        private double ComputeEquation(long raw)
        {
            try
            {
                string expr = Equation.Replace("{R}", raw.ToString());
                DataTable dt = new DataTable();
                var result = dt.Compute(expr, "");
                return Convert.ToDouble(result);
            }
            catch
            {
                Log.Warning("Failed to compute equation. Using raw value.");
                return raw;
            }
        }
    }
}
