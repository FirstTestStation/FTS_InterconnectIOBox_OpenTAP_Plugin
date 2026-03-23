using OpenTap;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace InterconnectIOBox.Instruments
{
    [Display("Setup Results Data", Groups: new[] { "InterconnectIO", "Fixture/DUT" },
    Description: "Defines additional columns in CSV result tables and configures the file name and save location. " +
"Settings are processed by FTS OperatorGUI. " +
"This step must be placed first in the Test Plan.")]
    public class SetupResultFile : TestStep
    {
        #region DUT
        [Display("DUT", Group: "General", Order: 0)]
        public FTS_DUT Dut { get; set; }
        #endregion

        #region Metadata Toggles
        private const string MetaGroup = "Columns to add on CSV";

        [Browsable(false)]
        public bool AddSerialNumber { get; set; } = true;

        [Display("Product Name", Group: MetaGroup, Order: 2, Description: "Product Name defined in FTS_DUT.")]
        public bool AddProductName { get; set; } = true;
        [Display("Product Number", Group: MetaGroup, Order: 3, Description: "Product Number defined in FTS_DUT.")]
        public bool AddProductNumber { get; set; } = true;
        [Display("Fixture Name", Group: MetaGroup, Order: 4, Description: "Fixture Name defined in FTS_DUT.")]
        public bool AddFixtureName { get; set; } = true;
        [Display("Fixture Number", Group: MetaGroup, Order: 5, Description: "Fixture Number defined in FTS_DUT.")]
        public bool AddFixtureNumber { get; set; } = true;
        [Display("Fixture Serial", Group: MetaGroup, Order: 6, Description: "Fixture Serial Number defined in FTS_DUT.")]
        public bool AddFixtureSerial { get; set; } = true;
        #endregion

        #region CSV File Name
        private const string CsvNameGroup = "CSV File Name";

        public enum CsvFileNameMode
        {
            Fixed,
            SerialNumber,
            ProductName,
            Id,
            ProductNumber,
            ProductName_SerialNumber,
            ProductNumber_SerialNumber,
            ProductName_ProductNumber_SerialNumber,
        }

        [Display("File Name Mode", Group: CsvNameGroup, Order: 10,
            Description: "Defines the <ResultName> part of: <ResultName>-<Date>-<Verdict>.csv.  ResultName will be replaced by new selected name." )]
        public CsvFileNameMode FileNameMode { get; set; } = CsvFileNameMode.ProductName_SerialNumber;

        [Display("Fixed Name", Group: CsvNameGroup, Order: 11,
            Description: "Used only when File Name Mode is set to Fixed.")]
        [EnabledIf(nameof(FileNameMode), CsvFileNameMode.Fixed)]
        public string FixedFileName { get; set; } = "TestResults";
        #endregion

        #region CSV Save Location
        private const string CsvSaveGroup = "CSV Save Location";

        public enum SaveLocationMode
        {
            Default,
            Local,
            Network,
            LocalAndNetwork
        }


        [Display("Save Mode", Group: CsvSaveGroup, Order: 20,
            Description: "Where FTS OperatorGUI will save the CSV result files using selected CSV file Name.")]
        public SaveLocationMode SaveMode { get; set; } = SaveLocationMode.Default;

        [Display("Local Folder", Group: CsvSaveGroup, Order: 21,
            Description: "Local folder where CSV result files will be saved. If network selected, local folder will be used as temporary location if network down.")]
        [EnabledIf(nameof(SaveMode), SaveLocationMode.Local, SaveLocationMode.LocalAndNetwork)]
        [DirectoryPath]
        public string LocalFolder { get; set; } = @"C:\TestResults";

        [Display("Network Folder", Group: CsvSaveGroup, Order: 22,
            Description: "Network folder where OperatorGUI will copy the CSV using new CSV File Name after each run.")]
        [EnabledIf(nameof(SaveMode), SaveLocationMode.Network, SaveLocationMode.LocalAndNetwork)]
        [DirectoryPath]
        public string NetworkFolder { get; set; } = @"\\Server\TestResults";
        #endregion

        #region Helpers
        private void SetPlanParam(string key, string value)
        {
            if (value == null) return;
            PlanRun.Parameters[key] = value;
        }

        private string BuildFileNameTemplate()
        {
            return FileNameMode switch
            {
                CsvFileNameMode.Fixed => FixedFileName,
                CsvFileNameMode.SerialNumber => "{SerialNumber}",
                CsvFileNameMode.ProductName => "{ProductName}",
                CsvFileNameMode.Id => "{ID}",
                CsvFileNameMode.ProductNumber => "{PartNumber}",
                CsvFileNameMode.ProductName_SerialNumber => "{ProductName}-{SerialNumber}",
                CsvFileNameMode.ProductNumber_SerialNumber => "{PartNumber}-{SerialNumber}",
                CsvFileNameMode.ProductName_ProductNumber_SerialNumber => "{ProductName}-{PartNumber}-{SerialNumber}",
                _ => throw new InvalidOperationException($"Unhandled FileNameMode: {FileNameMode}")
            };
        }
        #endregion

        public override void Run()
        {
            if (Dut == null)
            {
                Log.Error("[Publish Metadata] DUT is not assigned.");
                UpgradeVerdict(Verdict.Error);
                return;
            }

            // --- Push metadata into PlanRun.Parameters ---
            if (AddSerialNumber) SetPlanParam("SerialNumber", Dut.SerialNumber ?? "");
            if (AddProductName) SetPlanParam("ProductName", Dut.ProductName ?? "");
            if (AddProductNumber) SetPlanParam("ProductNumber", Dut.PartNumber ?? "");
            if (AddFixtureName) SetPlanParam("FixtureName", Dut.FixtName ?? "");
            if (AddFixtureNumber) SetPlanParam("FixtureNumber", Dut.FixtNumber ?? "");
            if (AddFixtureSerial) SetPlanParam("FixtureSerial", Dut.FixtSerial ?? "");
            SetPlanParam("ID", Dut.ID ?? "");

            // --- Pass all info to OperatorGUI via log ---
            // DUT info
            Log.Info($"[META]:SerialNumber:{Dut.SerialNumber ?? ""}");
            Log.Info($"[META]:ProductName:{Dut.ProductName ?? ""}");
            Log.Info($"[META]:PartNumber:{Dut.PartNumber ?? ""}");
            Log.Info($"[META]:FixtureName:{Dut.FixtName ?? ""}");
            Log.Info($"[META]:FixtureNumber:{Dut.FixtNumber ?? ""}");
            Log.Info($"[META]:FixtureSerial:{Dut.FixtSerial ?? ""}");
            Log.Info($"[META]:ID:{Dut.ID ?? ""}");

            Log.Info($"[META]:CsvName:{BuildFileNameTemplate()}");
            Log.Info($"[META]:SaveMode:{SaveMode}");

            if (SaveMode != SaveLocationMode.Default)
            {
                Log.Info($"[META]:LocalFolder:{LocalFolder}");

                if (SaveMode != SaveLocationMode.Local)
                    Log.Info($"[META]:NetworkFolder:{NetworkFolder}");
            }
            UpgradeVerdict(Verdict.Pass);
        }
    }
}