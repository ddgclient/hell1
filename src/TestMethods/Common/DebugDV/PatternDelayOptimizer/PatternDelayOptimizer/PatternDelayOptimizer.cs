// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// INTEL CONFIDENTIAL
// Copyright (2019) (2022) Intel Corporation
//
// The source code contained or described herein and all documents related to the source code ("Material") are
// owned by Intel Corporation or its suppliers or licensors. Title to the Material remains with Intel Corporation
// or its suppliers and licensors. The Material contains trade secrets and proprietary and confidential
// information of Intel Corporation or its suppliers and licensors. The Material is protected by worldwide copyright
// and trade secret laws and treaty provisions. No part of the Material may be used, copied, reproduced, modified,
// published, uploaded, posted, transmitted, distributed, or disclosed in any way without Intel Corporation's prior express
// written permission.
//
// No license under any patent, copyright, trade secret or other intellectual property right is granted to or
// conferred upon you by disclosure or delivery of the Materials, either expressly, by implication, inducement,
// estoppel or otherwise. Any license under such intellectual property rights must be express and approved by
// Intel in writing.
// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PatternDelayOptimizer.UnitTest")]

namespace PatternDelayOptimizer
{
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using DDG;
    using Newtonsoft.Json;
    using Prime;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PhAttributes;
    using Prime.PlistService;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class PatternDelayOptimizer : TestMethodBase
    {
        /// <summary>
        /// Boolean for parameters.
        /// </summary>
        public enum MyBool
        {
            /// <summary>
            /// False Value.
            /// </summary>
            False,

            /// <summary>
            /// True Value.
            /// </summary>
            True,
        }

        /// <summary>
        /// Enum to hold the search type parameter.
        /// </summary>
        public enum SearchType
        {
            /// <summary>
            /// Binary Search.
            /// </summary>
            Binary,

            /// <summary>
            /// Linear search starting at the minimum wait time and increasing.
            /// Will stop when test passes.
            /// </summary>
            LinearLowToHigh,

            /// <summary>
            /// Linear search starting at the maximum wait time and decreasing.
            /// Will stop when test fails and report the last passing point.
            /// </summary>
            LinearHighToLow,
        }

        /// <summary>
        /// Gets or sets Patlist to execute.
        /// </summary>
        public TestMethodsParams.Plist Patlist { get; set; }

        /// <summary>
        /// Gets or sets TimingsTc for plist execution.
        /// </summary>
        public TestMethodsParams.TimingCondition TimingsTc { get; set; }

        /// <summary>
        /// Gets or sets LevelsTc to plist execution.
        /// </summary>
        public TestMethodsParams.LevelsCondition LevelsTc { get; set; }

        /// <summary>
        /// Gets or sets the PrePlist callback to plist execution.
        /// </summary>
        public TestMethodsParams.String PrePlist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets comma separated mask pins for Patlist execution.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString MaskPins { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a limit of how many patterns to run at a time. 0 means run all patterns.
        /// </summary>
        public TestMethodsParams.Integer PerRunPatternLimit { get; set; } = 0;

        /// <summary>
        /// Gets or sets the name of the PatConfig Name (must exist in PatmodInputFile).
        /// </summary>
        public TestMethodsParams.String PatmodConfig { get; set; }

        /// <summary>
        /// Gets or sets the name of the input/base Prime .patmod.json file.
        /// </summary>
        public TestMethodsParams.File PatmodInputFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the output Prime .patmod.json file.
        /// </summary>
        public TestMethodsParams.File PatmodOutputFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the output json file which summarizes the results including the list of invalid patterns.
        /// </summary>
        public TestMethodsParams.File SummaryOutputFile { get; set; }

        /// <summary>
        /// Gets or sets the Minimum wait value to use.
        /// </summary>
        public TestMethodsParams.Integer SearchValueMin { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the Maximum wait value to use. If less-than-or-equal-to 0, the existing wait time in the pattern is used.
        /// </summary>
        public TestMethodsParams.Integer SearchValueMax { get; set; } = 0;

        /// <summary>
        /// Gets or sets the Resolution of the Search.
        /// </summary>
        public TestMethodsParams.Integer SearchValueResolution { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the maximum number of wait times to check.
        /// </summary>
        public TestMethodsParams.Integer MaxTestpoints { get; set; } = 100;

        /// <summary>
        /// Gets or sets a guardband for the final value. The saved value will be multiplied by (1 + GuardbandMultiplier).
        /// </summary>
        public TestMethodsParams.Double GuardbandMultiplier { get; set; } = 0.1;

        /// <summary>
        /// Gets or sets the Type of search to do. Either Binary, LinearLowToHigh or LinearHighToLow.
        /// </summary>
        public SearchType SearchMethod { get; set; } = SearchType.Binary;

        /// <summary>
        /// Gets or sets a value indicating whether the patterns should be restored to their original state when the intance finishes.
        /// </summary>
        public MyBool RestorePatterns { get; set; } = MyBool.True;

        /// <summary>
        /// Gets or sets a value indicating whether the ALEPH/PatConfig should be reloaded before running (might fail if Prime Ticket 23312 hasn't been fixed).
        /// </summary>
        public MyBool ReloadPatConfig { get; set; } = MyBool.False;

        /// <summary>
        /// Gets or sets a IFileSystem for Mocking.
        /// </summary>
        internal IFileSystem FileWrapper { get; set; } = new FileSystem();

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        private List<PatternContainer> AllPatterns { get; set; }

        private List<HashSet<string>> PatternsInTestGroups { get; set; }

        private ICaptureFailureTest FuncTest { get; set; }

        private IPlistObject Plist { get; set; }

        private List<string> PinsToMask { get; set; }

        private PatModConfiguration ConfigData { get; set; }

        private List<string> SkippedPatterns { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;

            // Check that all the pins for masking exist.
            this.PinsToMask = this.MaskPins;
            foreach (string pinName in this.PinsToMask)
            {
                this.Console?.PrintDebug("Checking that all pins to mask exist.");
                if (!Services.PinService.Exists(pinName))
                {
                    throw new TestMethodException($"Mask pin=[{pinName}] does not exist.");
                }
            }

            // load the config file.
            var localFileName = FileUtilities.GetFile(this.PatmodInputFile);
            if (this.ReloadPatConfig == MyBool.True)
            {
                this.Console?.PrintDebug($"Overriding ALEPH with configuration file=[{this.PatmodInputFile}].");
                Services.PatConfigService.InitEngineeringMode(EngineeringMode.ENGINEERING_UNSAFE, new List<string> { localFileName });
            }

            this.Console?.PrintDebug("Reading PatMod configuration file.");
            var configData = JsonConvert.DeserializeObject<PatModConfiguration>(this.FileWrapper.File.ReadAllText(localFileName));
            var match = configData.Configurations.FindIndex(o => o.Name == this.PatmodConfig);
            if (match == -1)
            {
                throw new TestMethodException($"PatmodFile=[{this.PatmodInputFile}] does not contain Configuration=[{this.PatmodConfig}].");
            }

            this.ConfigData = new PatModConfiguration();
            this.ConfigData.Configurations = new List<PatModConfiguration.Config> { configData.Configurations[match] };

            // build the test
            this.Console?.PrintDebug("Building the functional test.");
            this.FuncTest = Prime.Services.FunctionalService.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 99999, 1, this.PrePlist);

            // Get the plist and all the patterns to test.
            this.Plist = Prime.Services.PlistService.GetPlistObject(this.Patlist);

            // TODO: this takes a really long time, like a minute on a big list...
            // var allPatterns = new HashSet<string>(this.Plist.GetPatternNames().Where(pat => !this.Plist.IsPatternAnAmble(pat)));
            var allPatNamesRaw = this.Plist.GetUniquePatternNames();
            this.Console?.PrintDebug($"Found {allPatNamesRaw.Count} patterns to check.");

            // Build all the pattern structures.
            this.AllPatterns = new List<PatternContainer>(allPatNamesRaw.Count);
            this.SkippedPatterns = new List<string>();
            var allPatNamesValid = new HashSet<string>();
            foreach (var patname in allPatNamesRaw)
            {
                if (this.Plist.IsPatternAnAmble(patname))
                {
                    this.Console?.PrintDebug($"Skipping Amble pattern=[{patname}].");
                    this.SkippedPatterns.Add(patname);
                }
                else
                {
                    var pattern = new PatternContainer(patname, this.PatmodConfig, this.SearchValueMin, this.SearchValueMax, this.SearchValueResolution, this.ConfigData.Configurations.First().ConfigurationElement, this.Console);
                    if (pattern.Valid)
                    {
                        this.AllPatterns.Add(pattern);
                        allPatNamesValid.Add(patname);
                    }
                    else
                    {
                        this.SkippedPatterns.Add(patname);
                    }
                }
            }

            // group the patterns if requested to limit the number of patterns per burst/execute.
            this.PatternsInTestGroups = new List<HashSet<string>>();
            if (this.PerRunPatternLimit <= 0)
            {
                this.PatternsInTestGroups.Add(allPatNamesValid);
            }
            else
            {
                var source = new HashSet<string>(allPatNamesValid);
                while (source.Any())
                {
                    this.PatternsInTestGroups.Add(new HashSet<string>(source.Take(this.PerRunPatternLimit)));
                    source = new HashSet<string>(source.Skip(this.PerRunPatternLimit));
                }
            }

            // update the output paths if needed.
            if (!System.IO.Path.IsPathRooted(this.PatmodOutputFile))
            {
                var basepath = Prime.Services.TestProgramService.GetTestPlanPath();
                this.PatmodOutputFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(basepath), this.PatmodOutputFile);
            }

            if (!System.IO.Path.IsPathRooted(this.SummaryOutputFile))
            {
                var basepath = Prime.Services.TestProgramService.GetTestPlanPath();
                this.SummaryOutputFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(basepath), this.SummaryOutputFile);
            }
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            this.FuncTest.ApplyTestConditions();
            foreach (var patternGroup in this.PatternsInTestGroups)
            {
                this.RunSearch(patternGroup, this.MaxTestpoints);
            }

            this.AllPatterns.ForEach(o => o.SetToFinalValue(this.GuardbandMultiplier));
            this.Plist.EnableAllPatterns();

            var customOutput = this.CreateCustomOutput();
            var customOutputJson = JsonConvert.SerializeObject(customOutput, Formatting.Indented);
            this.Console?.PrintDebug($"\nFinalOutput:\n{customOutputJson}\n");
            this.FileWrapper.File.WriteAllText(this.SummaryOutputFile, customOutputJson);

            var patmodOutput = PatModConfiguration.BuildOutput(this.ConfigData.Configurations.Find(o => o.Name == this.PatmodConfig), customOutput.PatternsByPatMod);
            var patmodOutputJson = JsonConvert.SerializeObject(patmodOutput, Formatting.Indented);
            this.Console?.PrintDebug($"\nPrime PatMod:\n{patmodOutputJson}\n");
            this.FileWrapper.File.WriteAllText(this.PatmodOutputFile, patmodOutputJson);

            // restore all patterns to their original state.
            if (this.RestorePatterns == MyBool.True)
            {
                this.AllPatterns.ForEach(o => o.RestorePattern());
                var allPatConfigs = this.AllPatterns.Select(o => o.PatConfigHandle).ToList();
                Prime.Services.PatConfigService.Apply(allPatConfigs);
            }

            return 1;
        }

        private void RunSearch(HashSet<string> patternsToTest, int timeout)
        {
            this.Console?.PrintDebug("Running the search....");

            this.AllPatterns.ForEach(o => o.Enabled = false);
            this.AllPatterns.Where(item => patternsToTest.Contains(item.Pattern)).ToList().ForEach(o => o.ResetForInitialSearch(this.SearchMethod != SearchType.LinearHighToLow));
            this.Plist.EnableGivenPatternsDisableRest(patternsToTest);

            int count = 0;
            while (this.AllPatterns.Count(o => o.Enabled) > 0 && ++count <= timeout)
            {
                this.Console?.PrintDebug($"\n\n*************************************************\nRunning testpoint {count}\n\n");
                var patternsToDisable = this.RunSingleTestPoint();
                if (patternsToDisable.Count > 0)
                {
                    this.Plist.DisablePatterns(patternsToDisable);
                }
            }
        }

        private HashSet<string> RunSingleTestPoint()
        {
            var patternsUnderTest = this.AllPatterns.Where(item => item.Enabled && !item.FoundResult).ToList();

            // do all the patconfigs.
            this.Console?.PrintDebug("Setting up all patterns...");
            var allPatConfigs = patternsUnderTest.Select(o => o.PatConfigHandle).ToList();
            Prime.Services.PatConfigService.Apply(allPatConfigs);

            // execute the test.
            this.Console?.PrintDebug("Running the test...");
            this.FuncTest.SetPinMask(this.PinsToMask);
            var testResult = this.FuncTest.Execute();

            // read the results, setup the next testpoint.
            this.Console?.PrintDebug("Checking the results...");
            var allFails = this.FuncTest.GetPerCycleFailures();
            var failingPatterns = new HashSet<string>(allFails.Select(o => o.GetPatternName()));
            var ambleFail = allFails.Count() > 0 ? this.Plist.IsPatternAnAmble(allFails.First().GetPatternName()) : false;
            this.Console?.PrintDebug($"Failing Patterns:\n    {string.Join("\n    ", failingPatterns)}");

            HashSet<string> completedPatterns = new HashSet<string>();
            foreach (var pattern in patternsUnderTest)
            {
                if (pattern.ReadResultsAndUpdateForNextTestPoint(failingPatterns, ambleFail, this.SearchMethod == SearchType.Binary, this.SearchMethod == SearchType.LinearLowToHigh))
                {
                    completedPatterns.Add(pattern.Pattern);
                }
            }

            return completedPatterns;
        }

        private CustomOutput CreateCustomOutput()
        {
            this.Console?.PrintDebug($"\nResults:");
            var results = new CustomOutput();
            results.ConfigName = this.PatmodConfig;
            results.PatternsByPatMod = new Dictionary<string, List<string>>();
            results.InvalidPatterns = new List<string>();
            results.SkippedPatterns = this.SkippedPatterns;

            foreach (var pattern in this.AllPatterns.OrderBy(o => $"{o.CurrentPatMod}_{o.Pattern}"))
            {
                this.Console?.PrintDebug($"{pattern.CurrentPatMod,-30} ; {pattern.GetResultsAsString(),-40} ; {pattern.Pattern}");

                if (string.IsNullOrEmpty(pattern.CurrentPatMod))
                {
                    results.InvalidPatterns.Add(pattern.Pattern);
                }
                else if (!results.PatternsByPatMod.ContainsKey(pattern.CurrentPatMod))
                {
                    results.PatternsByPatMod[pattern.CurrentPatMod] = new List<string> { pattern.Pattern };
                }
                else
                {
                    results.PatternsByPatMod[pattern.CurrentPatMod].Add(pattern.Pattern);
                }
            }

            return results;
        }

        /// <summary>
        /// Container to hold the custom results format.
        /// </summary>
        internal class CustomOutput
        {
            /// <summary>
            /// Gets or sets the PatConfig Name used with this data.
            /// </summary>
            [JsonProperty("ConfigName")]
            [JsonRequired]
            internal string ConfigName { get; set; }

            /// <summary>
            /// Gets or sets the List of patterns valid for each patconfig data set..
            /// </summary>
            [JsonProperty("ValidResults")]
            [JsonRequired]
            internal Dictionary<string, List<string>> PatternsByPatMod { get; set; }

            /// <summary>
            /// Gets or sets the list of invalid or failing patterns.
            /// </summary>
            [JsonProperty("InvalidPatterns")]
            [JsonRequired]
            internal List<string> InvalidPatterns { get; set; }

            /// <summary>
            /// Gets or sets the list of skippedpatterns.
            /// </summary>
            [JsonProperty("SkippedPatterns")]
            [JsonRequired]
            internal List<string> SkippedPatterns { get; set; }
        }
    }
}