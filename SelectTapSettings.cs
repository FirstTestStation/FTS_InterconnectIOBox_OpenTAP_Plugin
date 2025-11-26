using System.IO;
using OpenTap;

namespace InterconnectIOBox
{
    [OpenTap.Display("TAP Settings Selector", Groups: new[] { "InterconnectIO", "System" },
        Description: "Selects which TAP Settings file to use, so the Production GUI can read it and the operator will not need to select it.")]
    public class SelectSettingsStep : ResultTestStep
    {
        // Initialized to prevent NRE if null is passed during deserialization
        private string _settingsFile = "DefaultSettings.TapSettings";
        private string _AltPath = string.Empty; // Also initialized to string.Empty

        [Display("Settings File", Order: 1, Group: "File Settings Path", Description: "Select the TAP settings file to use (filename only).")]
        [FilePath(FilePathAttribute.BehaviorChoice.Open, "TapSettings")]
        public string SettingsFile
        {
            get => _settingsFile;
            // Simplest possible setter: just assign the value.
            set => _settingsFile = value ?? string.Empty;
        }

        [Display("Alt Folder Path", Order: 1.1, Group: "File Settings Path", Description: "Select an alternative base path (e.g., Development or Production) to look for the TapSettings file.")]
        [DirectoryPath]
        public string AltPath
        {
            get => _AltPath;
            set
            {
                // 1. Assign the value, falling back to empty string if null.
                string path = value ?? string.Empty;

                // 2. Check if the path is non-empty and not already absolute.
                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        path = System.IO.Path.GetFullPath(path);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Warning($"Could not resolve AltPath to an absolute path: '{path}'. Error: {ex.Message}");
                    }
                }

                _AltPath = path;
            }
        }
        
   

        public override void Run()
        {
            // Initialize the local verdict to the lowest possible state.
            OpenTap.Verdict fileCheckVerdict = Verdict.Pass;
            string rawFilePath = SettingsFile;
            string resolvedFileName = string.Empty;
            const string expectedExtension = ".TapSettings";

            // --- 1. CONFIGURATION AND EXTENSION CHECK ---

            // Check if the primary configuration string is empty.
            if (string.IsNullOrEmpty(SettingsFile))
            {
                Log.Error("Configuration Error: Settings File path cannot be empty.");
                UpgradeVerdict(Verdict.Fail);
                fileCheckVerdict = Verdict.Fail;
            }
            else
            {
                // Check the extension on the filename provided in SettingsFile.
                string currentExtension = System.IO.Path.GetExtension(SettingsFile);

                if (!currentExtension.Equals(expectedExtension, System.StringComparison.OrdinalIgnoreCase))
                {
                    Log.Error($"Configuration Error: Incorrect file extension '{currentExtension}' in '{SettingsFile}'. Expected '{expectedExtension}'.");
                    UpgradeVerdict(Verdict.Fail);
                    fileCheckVerdict = Verdict.Fail;
                }
            }


            // --- 2. PATH REPORTING (NO RESOLUTION/FILE SYSTEM ACCESS) ---

            if (fileCheckVerdict == Verdict.Pass)
            {
                // Primary Path Reporting: Report the raw, validated SettingsFile string.
                // We rely on the downstream system (GUI) to handle path resolution.
                resolvedFileName = System.IO.Path.GetFileName(rawFilePath);

                Log.Debug($"Configuration successfully read. Primary settings file (raw string): {resolvedFileName}");

                // Alternative Path Reporting (Check only for non-empty AltPath)
                if (!string.IsNullOrEmpty(AltPath))
                {
                    // Report the combined path using AltPath and the filename part of SettingsFile.
                    // We must use Path.GetFileName to ensure we only get the file name, not subdirectories.
                    string settingsFileName = System.IO.Path.GetFileName(SettingsFile);

                    // We can safely use Path.Combine here since AltPath is a DirectoryPath and SettingsFileName is just a name.
                    string combinedPath = System.IO.Path.Combine(AltPath, settingsFileName);

                    Log.Debug($"Alternative path provided (combined for reference): {combinedPath}");
                }

                // If we reach here, the configuration strings are valid and non-empty.
                Log.Info("Successfully verified TAP Settings configuration strings.");
                UpgradeVerdict(Verdict.Pass);
            }

            // --- 3. FINAL VERDICT AND RESULT PUBLICATION ---

            // Publish the result record containing the final file path (the raw SettingsFile if successful).
            var result = new TestResult<string>
            {
                ParamName = "TapSettings File Path",
                StepName = Name,
                // Publish the raw path string.
                Value = resolvedFileName,
                // Using the expected extension for descriptive purposes
                LowerLimit = expectedExtension,
                UpperLimit = expectedExtension,
                Verdict = fileCheckVerdict.ToString(),
                Units = "Config String Check"
            };

            PublishResult(result);
        }
    }
}
