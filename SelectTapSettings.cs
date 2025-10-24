using System.IO;
using OpenTap;

namespace InterconnectIOBox
{
    [OpenTap.Display("TAP Settings", Groups: new[] { "InterconnectIO", "System" },
        Description: "Selects which TAP Settings file to use, so the Production GUI can read it and operator will not need to select it.")]
    public class SelectSettingsStep : ResultTestStep
    {
        private string _settingsFile = "DefaultSettings.TapSettings";
        private string _AltPath;


        [Display("Settings File", Order: 1, Group: "File Settings Path", Description: "Select the TAP settings file to use (filename only).")]
        [FilePath(FilePathAttribute.BehaviorChoice.Open, "TapSettings")]
        public string SettingsFile
        {
            get => _settingsFile;
            set => _settingsFile = value;
        }


        [Display("Alternative Path", Order: 1.1, Group: "File Path", Description: "Select an alternative path (Development or Production) to look for the TapSettings")]
        [DirectoryPath]
        public string AltPath
        {
            get => _AltPath;
            set
            {
                // La correction : Normaliser immédiatement le chemin vers sa forme absolue dans le setter.
                // Si la valeur est nulle ou vide, on stocke une chaîne vide, sinon on normalise.
                _AltPath = string.IsNullOrEmpty(value) ? "" : Path.GetFullPath(value);
            }
        }

        public override void Run()
        {
            // 1. Initialise the local verdict to the lowest possible state (Pass)
            OpenTap.Verdict fileCheckVerdict = Verdict.Pass;

            // --- VALIDATION LOGIC (File Extension & Existence) ---

            const string expectedExtension = ".TapSettings";
            string currentExtension = System.IO.Path.GetExtension(SettingsFile);

            // 1. Check file extension
            if (!currentExtension.Equals(expectedExtension, System.StringComparison.OrdinalIgnoreCase))
            {
                Log.Error($"Configuration Error: Incorrect file extension '{currentExtension}'. Expected '{expectedExtension}'.");
                // Upgrade the step's overall verdict
                UpgradeVerdict(Verdict.Fail);
                // Set the local result variable
                fileCheckVerdict = Verdict.Fail;

                // Uncomment the throw line if you want the test plan to halt immediately.
                // throw new System.ArgumentException($"The selected file '{SettingsFile}' must have the '{expectedExtension}' extension.");
            }


            // 2. Check file existence (Only check if the file extension was correct, or check unconditionally)
            if (fileCheckVerdict == Verdict.Pass && !System.IO.File.Exists(SettingsFile))
            {
                Log.Error($"Configuration Error: Settings file not found at path: '{SettingsFile}'.");
                // Upgrade the step's overall verdict
                UpgradeVerdict(Verdict.Fail);
                // Set the local result variable
                fileCheckVerdict = Verdict.Fail;

                // Uncomment the throw line if you want the test plan to halt immediately.
                // throw new System.IO.FileNotFoundException($"Required settings file not found: {SettingsFile}", SettingsFile);
            }

            // --- LOGGING AND RESULT PUBLISHING ---

            // The 'fileCheckVerdict' will be Fail if either check above failed, otherwise it remains Pass.
            if (fileCheckVerdict == Verdict.Pass)
            {
                Log.Info($"Successfully verified TAP Settings file: {SettingsFile}");
                UpgradeVerdict(Verdict.Pass);
            }

            // Define the local variables needed for your TestResult object
            // Note: The 'expected' string is now descriptive, as the true limits are used for the verdict.
            string expectedResultString = fileCheckVerdict == Verdict.Pass ? expectedExtension : "File not valid";

            // Publish the result record
            var result = new TestResult<string>
            {
                ParamName = $"TapSettings Load",
                StepName = Name,
                Value = System.IO.Path.GetFileNameWithoutExtension(SettingsFile),
                LowerLimit = expectedExtension,
                UpperLimit = expectedExtension,
                Verdict = fileCheckVerdict.ToString(),
                Units = "Check"
            };

            // This line publishes the result to OpenTAP's log and database
            PublishResult(result);
        }

    }
}