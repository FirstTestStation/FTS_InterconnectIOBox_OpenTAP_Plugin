using System.IO;
using InterconnectIOBox.Analysis;
using OpenTap;

namespace InterconnectIOBox.Configuration
{
    [Display("TAP Settings Selector", Groups: new[] { "FTS_Interconnect", "System" },
        Description: "Selects which TAP settings file to use, so the production GUI can read it and the operator does not need to select it.")]
    public class SelectSettingsStep : ResultTestStep
    {
        // This step is independent of the DUT — it doesn't need one assigned to run.
        // Its result is still published through the normal SerialNumber-gated
        // pipeline, so it lands in the same single report as every other step.
        protected override bool RequiresDut => false;

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

                // 2. Resolve to an absolute path, if non-empty.
                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        path = Path.GetFullPath(path);
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
            Verdict fileCheckVerdict = Verdict.Pass;
            string rawFilePath = SettingsFile;
            const string expectedExtension = ".TapSettings";

            // --- 1. CONFIGURATION AND EXTENSION CHECK ---

            // Check if the primary configuration string is empty.
            if (string.IsNullOrEmpty(SettingsFile))
            {
                Log.Error("Configuration Error: Settings File path cannot be empty.");
                fileCheckVerdict = Verdict.Fail;
            }
            else
            {
                // Check the extension on the filename provided in SettingsFile.
                string currentExtension = Path.GetExtension(SettingsFile);

                if (!currentExtension.Equals(expectedExtension, System.StringComparison.OrdinalIgnoreCase))
                {
                    Log.Error($"Configuration Error: Incorrect file extension '{currentExtension}' in '{SettingsFile}'. Expected '{expectedExtension}'.");
                    fileCheckVerdict = Verdict.Fail;
                }
            }

            // Always report the raw file name for the result, whether or not validation passed,
            // so a failed run still shows what was actually configured.
            string reportedFileName = Path.GetFileName(rawFilePath);

            // --- 2. PATH REPORTING (NO RESOLUTION/FILE SYSTEM ACCESS) ---

            if (fileCheckVerdict == Verdict.Pass)
            {
                // Primary Path Reporting: Report the raw, validated SettingsFile string.
                // We rely on the downstream system (GUI) to handle path resolution.
                Log.Debug($"Configuration successfully read. Primary settings file (raw string): {reportedFileName}");

                // Alternative Path Reporting (Check only for non-empty AltPath)
                if (!string.IsNullOrEmpty(AltPath))
                {
                    // Report the combined path using AltPath and the filename part of SettingsFile.
                    // We must use Path.GetFileName to ensure we only get the file name, not subdirectories.
                    string settingsFileName = Path.GetFileName(SettingsFile);

                    // We can safely use Path.Combine here since AltPath is a DirectoryPath and settingsFileName is just a name.
                    string combinedPath = Path.Combine(AltPath, settingsFileName);

                    Log.Debug($"Alternative path provided (combined for reference): {combinedPath}");
                }

                // If we reach here, the configuration strings are valid and non-empty.
                Log.Info("Successfully verified TAP settings configuration strings.");
            }

            UpgradeVerdict(fileCheckVerdict);

            // --- 3. RESULT PUBLICATION ---
            // Published through ResultTestStep.PublishResult: if the SerialNumber
            // isn't known yet (or Dut isn't assigned), this result is queued and
            // flushed alongside every other step's result once it is — ending up
            // in a single, unified report.
            var result = new TestResult<string>
            {
                ParamName = "TapSettings File Path",
                StepName = Name,
                // Publish the raw file name.
                Value = reportedFileName,
                // Using the expected extension for descriptive purposes
                LowerLimit = expectedExtension,
                UpperLimit = expectedExtension,
                Verdict = fileCheckVerdict == Verdict.Pass ? "PASS" : "FAIL",
                Units = "Config String Check"
            };

            PublishResult(result);
        }
    }
}