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

namespace DfxTimingTuner
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using DDG;
    using Prime.Base.Exceptions;
    using Prime.Base.Utilities;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PhAttributes;
    using Prime.TestConditionService;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class DfxTimingTuner : TestMethodBase
    {
        private static readonly double EyeSearchFailValue = -9999;

        /// <summary>
        /// Enum to specify which Timing TestConditions to re-resolve on a successful execution.
        /// </summary>
        public enum ResolveMode
        {
            /// <summary>Do not resolve any testconditions.</summary>
            None,

            /// <summary>Resolve the testcondition used in this test instance only.</summary>
            Current,

            /// <summary>Resolve all test conditions.</summary>
            All,
        }

        /// <summary>
        /// Enum to specify which True or False.
        /// </summary>
        public enum MyBool
        {
            /// <summary>False.</summary>
            False,

            /// <summary>True.</summary>
            True,
        }

        /// <summary>
        /// Gets or sets Patlist to execute for the Search.
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
        /// Gets or sets comma separated mask pins for Patlist execution.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString MaskPins { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the Configuration file to load.
        /// </summary>
        public TestMethodsParams.String ConfigFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the Configuration set to use.
        /// </summary>
        public TestMethodsParams.String ConfigSet { get; set; }

        /// <summary>
        /// Gets or sets the starting value of the search.
        /// </summary>
        public TestMethodsParams.String SearchStart { get; set; }

        /// <summary>
        /// Gets or sets the resolution of the search steps.
        /// </summary>
        public TestMethodsParams.String SearchResolution { get; set; }

        /// <summary>
        /// Gets or sets the ending value of the search.
        /// </summary>
        public TestMethodsParams.String SearchEnd { get; set; }

        /// <summary>
        /// Gets or sets the starting value of the search (as an offset from the current value).
        /// </summary>
        public TestMethodsParams.String AdaptiveStart { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resolution of the search steps.
        /// </summary>
        public TestMethodsParams.String AdaptiveResolution { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ending value of the search (as an offset from the current value).
        /// </summary>
        public TestMethodsParams.String AdaptiveEnd { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets which TestConditions to Resolve on a successfull search.
        /// </summary>
        public ResolveMode UpdateTC { get; set; } = ResolveMode.Current;

        /// <summary>
        /// Gets or sets whether or not to Datalog all pins on a successful search.
        /// </summary>
        public MyBool Datalog { get; set; } = MyBool.True;

        /// <summary>
        /// Gets or sets the PrePlist callback to plist execution.
        /// </summary>
        public TestMethodsParams.String PrePlist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        private ICaptureFailureTest FuncTest { get; set; }

        private ConfigurationFile.ConfigSet Configuration { get; set; }

        private List<string> CapturePins { get; set; }

        private List<string> SearchPins { get; set; }

        private List<string> AdjustPins { get; set; }

        private List<string> CtvDecodeOrder { get; set; }

        private Dictionary<string, string> PinToTimingVarMap { get; set; }

        private RangeContainer BaseSearchRange { get; set; }

        private RangeContainer AdaptiveSearchRange { get; set; } = null;

        private bool UseAdaptiveSettings { get; set; } = false;

        private IPatConfigHandle LoopSizePatCfg { get; set; }

        private string LoopSizeDataRegex { get; set; }

        private Dictionary<string, Dictionary<string, string>> InitialHwTimings { get; set; } = null;

        private Dictionary<string, string> InitialUserVarTimings { get; set; } = null;

        private string EdgeAttribute { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;

            // Check for required parameters (the preheader should prevent the TP from loading without these, but do a quick check anyway)
            if (this.Patlist == null || this.TimingsTc == null || this.LevelsTc == null)
            {
                throw new ArgumentException("PatList, TimingsTc and LevelsTc are all required parameters..");
            }

            this.Console?.PrintDebug($"[{this.InstanceName}] PatList=[{this.Patlist}]");
            this.Console?.PrintDebug($"[{this.InstanceName}] TimingsTc=[{this.TimingsTc}]");
            this.Console?.PrintDebug($"[{this.InstanceName}] LevelsTc=[{this.LevelsTc}]");

            if (this.ConfigFile == null || this.ConfigSet == null)
            {
                throw new ArgumentException("ConfigFile and ConfigSet are both required parameters..");
            }

            this.Console?.PrintDebug($"[{this.InstanceName}] ConfigFile=[{this.ConfigFile}]");
            this.Console?.PrintDebug($"[{this.InstanceName}] ConfigSet=[{this.ConfigSet}]");

            if (this.SearchStart == null || this.SearchResolution == null || this.SearchEnd == null)
            {
                throw new ArgumentException("SearchStart, SearchResolution and SearchEnd are all required parameters..");
            }

            // convert the search parameters into double values.
            this.UseAdaptiveSettings = false;
            if (!string.IsNullOrWhiteSpace(this.AdaptiveStart) && !string.IsNullOrWhiteSpace(this.AdaptiveResolution) && !string.IsNullOrWhiteSpace(this.AdaptiveEnd))
            {
                this.AdaptiveSearchRange = new RangeContainer(
                    Convert.ToDecimal(StringUtilities.StringWithUnitsToDouble(this.AdaptiveStart)),
                    Convert.ToDecimal(StringUtilities.StringWithUnitsToDouble(this.AdaptiveEnd)),
                    Convert.ToDecimal(StringUtilities.StringWithUnitsToDouble(this.AdaptiveResolution)));
            }
            else
            {
                this.AdaptiveSearchRange = null;
            }

            // for the base search, the uservar is updated to the SearchStart, so make everything relative to it.
            this.BaseSearchRange = new RangeContainer(
                0,
                Convert.ToDecimal(StringUtilities.StringWithUnitsToDouble(this.SearchEnd)) - Convert.ToDecimal(StringUtilities.StringWithUnitsToDouble(this.SearchStart)),
                Convert.ToDecimal(StringUtilities.StringWithUnitsToDouble(this.SearchResolution)));

            this.Console?.PrintDebug($"[{this.InstanceName}] SearchStart=[{this.SearchStart}] -> [{this.BaseSearchRange.Start}]");
            this.Console?.PrintDebug($"[{this.InstanceName}] SearchEnd=[{this.SearchEnd}] -> [{this.BaseSearchRange.End}]");
            this.Console?.PrintDebug($"[{this.InstanceName}] SearchResolution=[{this.SearchResolution}] -> [{this.BaseSearchRange.Resolution}]");
            this.Console?.PrintDebug($"[{this.InstanceName}] SearchSteps=[{this.BaseSearchRange.NumberOfSteps}]");

            if (this.AdaptiveSearchRange != null)
            {
                this.Console?.PrintDebug($"[{this.InstanceName}] AdaptiveSearchStart=[{this.AdaptiveStart}] -> [{this.AdaptiveSearchRange.Start}]");
                this.Console?.PrintDebug($"[{this.InstanceName}] AdaptiveSearchEnd=[{this.AdaptiveEnd}] -> [{this.AdaptiveSearchRange.End}]");
                this.Console?.PrintDebug($"[{this.InstanceName}] AdaptiveSearchResolution=[{this.AdaptiveResolution}] -> [{this.AdaptiveSearchRange.Resolution}]");
                this.Console?.PrintDebug($"[{this.InstanceName}] AdaptiveSearchSteps=[{this.AdaptiveSearchRange.NumberOfSteps}]");
            }

            // Load the configuration file and validate the configuration set.
            this.VerifyConfiguration(this.ConfigFile, this.ConfigSet);

            // Create the functional test.
            this.Console?.PrintDebug("Creating CaptureCTV test.");
            this.FuncTest = Prime.Services.FunctionalService.CreateCaptureFailureAndCtvPerPinTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.CapturePins, 32000, this.PrePlist);

            // Verify the Mask pins exist.
            if (string.IsNullOrWhiteSpace(this.MaskPins))
            {
                return;
            }

            foreach (var pinName in this.MaskPins.ToList().Where(pinName => !Prime.Services.PinService.Exists(pinName)))
            {
                throw new ArgumentException($"Mask pin=[{pinName}] does not exist in the Testprogram.");
            }
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            // Keeping this in Execute() to avoid Verify ordering dependency with the callbacks.
            if (this.Configuration.SearchType == Globals.EdgeMode.Compare)
            {
                this.FuncTest.SetSoftwareTriggerCallback("IncrementCompareEdge");
                this.EdgeAttribute = "Compare";
            }
            else
            {
                this.FuncTest.SetSoftwareTriggerCallback("IncrementDriveEdge");
                this.EdgeAttribute = "Drive";
            }

            // Run the search
            bool allPinsFoundValidEyes = false;
            Dictionary<string, string> userVarsForRestore = null;
            try
            {
                // Apply the levels once.
                this.FuncTest.ApplyLevelTestCondition();

                // Get the original timing setups (UserVar values, since TestCondtions have not been applied yet).
                userVarsForRestore = this.GetUserVarValuesForPins(this.CtvDecodeOrder);

                if (this.UseAdaptiveSettings && this.AdaptiveSearchRange != null)
                {
                    using (var sw = Prime.Services.PerformanceService?.GetStopWatch("AdaptiveMode"))
                    {
                        // Apply the test settings to hardware.
                        this.FuncTest.ApplyTimingTestCondition();

                        // Get the original HW settings.
                        this.InitialHwTimings = this.GetHwTimingValues(this.SearchPins, this.EdgeAttribute);
                        this.InitialUserVarTimings = userVarsForRestore;

                        this.Console?.PrintDebug($"Running Adaptive Search [{this.AdaptiveSearchRange.Start}] -> [{this.AdaptiveSearchRange.End}]");
                        allPinsFoundValidEyes = this.ExecuteSearch(this.AdaptiveSearchRange, datalogFailure: false);
                    }
                }

                // Run the original base range if the Adaptive range failed or wasn't specified.
                if (!allPinsFoundValidEyes)
                {
                    using (var sw = Prime.Services.PerformanceService?.GetStopWatch("NormalMode"))
                    {
                        // Base range is absolute values of the UserVars. Need to get current values and adjust accordingly.
                        this.InitialUserVarTimings = this.AdjustUserVars(this.CtvDecodeOrder, this.SearchStart);
                        this.ResolveTimings();
                        this.FuncTest.ApplyTimingTestCondition();
                        this.InitialHwTimings = this.GetHwTimingValues(this.SearchPins, this.EdgeAttribute);

                        this.Console?.PrintDebug($"Running Base Search [{this.BaseSearchRange.Start}] -> [{this.BaseSearchRange.End}]");
                        allPinsFoundValidEyes = this.ExecuteSearch(this.BaseSearchRange, datalogFailure: true);
                        this.UseAdaptiveSettings = allPinsFoundValidEyes;
                    }
                }
            }
            catch (Exception e)
            {
                Prime.Services.ConsoleService.PrintError($"Exception thrown when running search - [{e.GetType()}] {e.Message}\n{e.StackTrace}\n");
                allPinsFoundValidEyes = false;
                var ituff = Prime.Services.DatalogService.GetItuffStrgvalWriter();
                ituff.SetTnamePostfix("::ERROR");
                ituff.SetData(e.Message.Replace(" ", "_").Replace("\n", "_"));
                ituff.SetDelimiterCharacterForWrap('_');
                Prime.Services.DatalogService.WriteToItuff(ituff);
            }

            if (!allPinsFoundValidEyes)
            {
                this.AdjustUserVars(userVarsForRestore, 0);
                /* this.AdjustTimings(timingsForRestore, this.EdgeAttribute, 0); */
                this.ResolveTimings();
            }
            else if (this.UpdateTC == ResolveMode.Current)
            {
                /* var tc = Prime.Services.TestConditionService.GetTestCondition(this.TimingsTc);
                tc.Resolve();
                if (Prime.Services.TestConditionService.IsSmartTcEnabled())
                {
                    Prime.Services.TestConditionService.FlushSmartTCCategory(SmartTCCategoryType.TIMING);
                } */
                this.ResolveTimings();
            }
            else if (this.UpdateTC == ResolveMode.All)
            {
                Prime.Services.TestConditionService.ResolveAllTestConditions();
            }

            return allPinsFoundValidEyes ? 1 : 0;
        }

        private bool ExecuteSearch(RangeContainer searchRange, bool datalogFailure)
        {
            // Setup shared storage for the callbacks
            TriggerCallbacks.SetupStorageForCallbacks(Convert.ToDouble(searchRange.Resolution), this.AdjustPins);

            // Execute all the bursts
            var allPinsFoundValidEyes = this.ExecuteAllBursts(searchRange, out var calculatedTimingOffsets);

            // update/log the per-pin timings if all pins found results.
            var datalog = (datalogFailure && !allPinsFoundValidEyes) || (this.Datalog == MyBool.True && allPinsFoundValidEyes);
            if (datalog || allPinsFoundValidEyes)
            {
                var ituff = Prime.Services.DatalogService.GetItuffStrgvalWriter();
                foreach (var pin in this.CtvDecodeOrder)
                {
                    var userVar = this.PinToTimingVarMap[pin];
                    ituff.SetTnamePostfix($"::{userVar}");

                    if (Math.Abs(calculatedTimingOffsets[pin] - EyeSearchFailValue) > double.Epsilon * 2)
                    {
                        var initialTimingValue = this.InitialUserVarTimings[pin].ToDouble();
                        string newTimingValue = ((initialTimingValue + calculatedTimingOffsets[pin]) * 1e9).ToString("N3") + "ns"; // round to the nearest pico second.
                        if (allPinsFoundValidEyes)
                        {
                            this.Console?.PrintDebug($"Updating [{userVar}] Original=[{initialTimingValue}] SearchResult=[{calculatedTimingOffsets[pin]}] NewValue=[{newTimingValue}]");
                            Prime.Services.UserVarService.SetValue(userVar, newTimingValue);
                            this.AdjustTimings(pin, this.InitialHwTimings[pin], this.EdgeAttribute, calculatedTimingOffsets[pin]);
                        }

                        ituff.SetData(newTimingValue);
                    }
                    else
                    {
                        ituff.SetData(EyeSearchFailValue.ToString());
                    }

                    if (datalog)
                    {
                        Prime.Services.DatalogService.WriteToItuff(ituff);
                    }
                }
            }

            return allPinsFoundValidEyes;
        }

        private bool ExecuteAllBursts(RangeContainer searchRange, out Dictionary<string, double> calculatedTimingOffsets)
        {
            var allPinsFoundValidEyes = true;

            // if the range is >4ns, need to break it into multiple bursts.
            var allBurstSetups = searchRange.SplitRange(4E-9m);

            Dictionary<string, BitArray> allResults = null;
            foreach (var burst in allBurstSetups)
            {
                // Set the timing to the initial value.
                this.AdjustTimings(this.InitialHwTimings, this.EdgeAttribute, Convert.ToDouble(burst.Start));

                // Run the burst.
                this.Console?.PrintDebug($"Executing one burst for range: Start=[{burst.Start}] Steps=[{burst.NumberOfSteps}] Resolution=[{burst.Resolution}].");
                var testPassed = this.ExecuteOneBurst(out var perpinTestResults, burst.NumberOfSteps);

                if (perpinTestResults.Count != this.CtvDecodeOrder.Count)
                {
                    throw new TestMethodException($"Expecting pass/fail results for [{this.CtvDecodeOrder.Count}] pins, got [{perpinTestResults.Count}].");
                }

                // combine the results.
                if (allResults == null)
                {
                    allResults = perpinTestResults;
                }
                else
                {
                    foreach (var pin in perpinTestResults.Keys)
                    {
                        allResults[pin] = allResults[pin].Add(perpinTestResults[pin]);
                    }
                }
            }

            // perform the eyesearch on each pin.
            calculatedTimingOffsets = new Dictionary<string, double>(this.CtvDecodeOrder.Count);
            var pinWidth = 20;
            if (this.LogLevel != PrimeLogLevel.DISABLED)
            {
                pinWidth = this.CtvDecodeOrder.Select(s => s.Length).Max();
            }

            foreach (var pin in this.CtvDecodeOrder)
            {
                if (this.LogLevel != PrimeLogLevel.DISABLED)
                {
                    this.Console?.PrintDebug($"Results: [{pin.PadRight(pinWidth)}] = [{string.Join(string.Empty, allResults[pin].Cast<bool>().Select(o => o == EyeSearch.BitArrayBoolForPass ? "*" : "."))}]");
                }

                if (EyeSearch.FindLargestPassingRegion(allResults[pin], out var start, out var width))
                {
                    calculatedTimingOffsets[pin] = Convert.ToDouble(((start + (width / 2)) * searchRange.Resolution) + searchRange.Start);
                    this.Console?.PrintDebug($"\tEyeSearch: PassingStart=[{start}] PassingWidth=[{width}] NewOffset=[{calculatedTimingOffsets[pin]}].");
                }
                else
                {
                    allPinsFoundValidEyes = false;
                    calculatedTimingOffsets[pin] = EyeSearchFailValue;
                }
            }

            return allPinsFoundValidEyes;
        }

        private bool ExecuteOneBurst(out Dictionary<string, BitArray> perpinTestResults, int stepCount)
        {
            perpinTestResults = new Dictionary<string, BitArray>(0); // dummy initialization

            // set the correct number of steps to the pattern
            this.LoopSizePatCfg.SetData(this.LoopSizeDataRegex.Replace("%SIZE%", stepCount.ToString()));
            Prime.Services.PatConfigService.Apply(this.LoopSizePatCfg);

            // set the mask if requested.
            if (!string.IsNullOrWhiteSpace(this.MaskPins))
            {
                this.Console?.PrintDebug($"Setting pin mask to pin=[{this.MaskPins}].");
                this.FuncTest.SetPinMask(this.MaskPins.ToList());
            }

            // execute the test.
            var testPassed = this.FuncTest.Execute();
            this.Console?.PrintDebug($"Test Execution returned [{testPassed}].");

            var count = TriggerCallbacks.GetCallCount();
            this.Console?.PrintDebug($"Recorded [{count}] executions of the trigger callback (TOSTrigger).");

            if (TriggerCallbacks.GetFailureStatus(out var error))
            {
                throw new TestMethodException($"Error during trigger callback. Messge={error}");
            }

            // get the results formatted correctly.
            if (this.FuncTest is ICaptureFailureAndCtvPerPinTest ctvTest)
            {
                var rawCtvData = ctvTest.GetCtvData();
                if (rawCtvData.Count == 0)
                {
                    throw new TestMethodException("ERROR: No CTV data captured, the plist did not run correctly.");
                }

                if (this.LogLevel != PrimeLogLevel.DISABLED)
                {
                    var pinWidth = rawCtvData.Select(s => s.Key.Length).Max();
                    foreach (var item in rawCtvData.OrderBy(i => i.Key))
                    {
                        this.Console?.PrintDebug($"\tCTV Data [{item.Key.PadRight(pinWidth)}] = [{item.Value}]");
                    }
                }

                if (this.Configuration.SearchType == Globals.EdgeMode.Compare)
                {
                    perpinTestResults = EyeSearch.CompareModeMultiCtvToPerPinTestResults(rawCtvData, stepCount);
                }
                else
                {
                    perpinTestResults = EyeSearch.DriveModeSingleCtvToPerPinTestResults(rawCtvData[this.CapturePins[0]], stepCount, this.CtvDecodeOrder);
                }
            }

            return testPassed;
        }

        private void VerifyConfiguration(string configurationFile, string configurationSet)
        {
            ConfigurationFile fullConfiguration = ConfigurationFile.LoadFile(configurationFile);
            var set = fullConfiguration.ConfigSets.Find(c => c.Name == configurationSet);
            if (set == null)
            {
                throw new TestMethodException($"No ConfigSet=[{configurationSet}] found in ConfigurationFile=[{configurationFile}].");
            }

            this.Configuration = set;

            // Initialize the Pin Lists.
            this.SearchPins = this.GetPinGroupFromConfig("search_pins", set.SearchPinGroup, configurationFile, configurationSet, fullConfiguration).Select(o => o.Name).ToList();
            this.AdjustPins = this.SearchPins;
            this.CapturePins = this.GetPinGroupFromConfig("capture_pins", set.CapturePinGroup, configurationFile, configurationSet, fullConfiguration).Select(o => o.Name).ToList();
            List<ConfigurationFile.PinObject> ctvDecoder;
            if (set.SearchType == Globals.EdgeMode.Drive)
            {
                ctvDecoder = this.GetPinGroupFromConfig("capture_ctvorder", set.CtvPinGroup, configurationFile, configurationSet, fullConfiguration);
            }
            else
            {
                ctvDecoder = this.GetPinGroupFromConfig("capture_pins", set.CapturePinGroup, configurationFile, configurationSet, fullConfiguration);
            }

            this.CtvDecodeOrder = ctvDecoder.Select(o => o.Name).ToList();

            // Verify the UserVars for per-pin timing control.
            if (string.IsNullOrWhiteSpace(set.UserVarRegex))
            {
                throw new TestMethodException($"No 'uservar' field found in ConfigSet=[{configurationSet}], ConfigurationFile=[{configurationFile}].");
            }

            this.PinToTimingVarMap = new Dictionary<string, string>(this.CtvDecodeOrder.Count);
            foreach (var pin in ctvDecoder)
            {
                var timingVar = set.UserVarRegex.Replace("%PIN%", pin.Name).Replace("%ALIAS%", pin.Alias);
                this.PinToTimingVarMap[pin.Name] = timingVar;

                if (!Prime.Services.UserVarService.Exists(timingVar))
                {
                    throw new TestMethodException($"No UserVar=[{timingVar}] Found. From Pin=[{pin.Name}] uservar=[{set.UserVarRegex}] in ConfigSet=[{configurationSet}], ConfigurationFile=[{configurationFile}].");
                }
            }

            // Verify the PatConfig for the TosTrigger loop control.
            if (set.LoopControl == null)
            {
                throw new TestMethodException($"No 'loop_size' field found in ConfigSet=[{configurationSet}], ConfigurationFile=[{configurationFile}].");
            }

            if (string.IsNullOrWhiteSpace(set.LoopControl.PatConfig))
            {
                throw new TestMethodException($"No 'config' attribute found in loop_size for ConfigSet=[{configurationSet}], ConfigurationFile=[{configurationFile}].");
            }

            // Get the patconfig handle.
            try
            {
                this.LoopSizePatCfg = Prime.Services.PatConfigService.GetPatConfigHandleWithPlist(set.LoopControl.PatConfig, this.Patlist);
            }
            catch (FatalException e)
            {
                Prime.Services.ConsoleService.PrintError($"Error getting PatConfigHandle for [{set.LoopControl.PatConfig}], plist=[{this.Patlist}].\n{e.Message}\n{e.StackTrace}");
                throw new TestMethodException($"No PatConfig=[{set.LoopControl.PatConfig}] found in ALEPH. referenced from 'loop_size' in ConfigSet=[{configurationSet}], ConfigurationFile=[{configurationFile}].");
            }

            // verify the Patconfig data is the correct format.
            this.LoopSizeDataRegex = set.LoopControl.Data;
            Regex rx = new Regex(@"^(MOV\s+%SIZE%,\s*((R\d+)|_)\s*\|?)+$", RegexOptions.IgnoreCase);
            if (!rx.IsMatch(this.LoopSizeDataRegex))
            {
                throw new TestMethodException($"Invalid 'loop_size' Data=[{set.LoopControl.Data}]. Expecting 'MOV %SIZE%, R\\d+' referenced in ConfigSet=[{configurationSet}], ConfigurationFile=[{configurationFile}].");
            }

            // special support for calling Set*Timing with a pingroup instead of a list of pins.
            if (!string.IsNullOrEmpty(set.PinGroupForAdjust))
            {
                this.AdjustPins = set.PinGroupForAdjust.Split(',').ToList();
                foreach (var pin in this.AdjustPins)
                {
                    if (!Prime.Services.PinService.Exists(pin))
                    {
                        throw new TestMethodException($"Pin/Group=[{pin}] does not exist. Referenced from 'pingroup_for_adjust' field, ConfigSet=[{configurationSet}], ConfigurationFile=[{configurationFile}].");
                    }
                }
            }
        }

        private List<ConfigurationFile.PinObject> GetPinGroupFromConfig(string id, string groupName, string configurationFile, string configurationSet, ConfigurationFile fullConfiguration)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new TestMethodException($"No '{id}' field found in ConfigSet=[{configurationSet}], ConfigurationFile=[{configurationFile}].");
            }

            var group = fullConfiguration.PinGroups.Find(p => p.Name == groupName);
            if (group == null)
            {
                throw new TestMethodException($"No PinGroup=[{groupName}] found in ConfigurationFile=[{configurationFile}]. Referenced in ConfigSet=[{configurationSet}].");
            }

            // verify all the pins exist.
            foreach (var pin in group.Pins)
            {
                if (!Prime.Services.PinService.Exists(pin.Name))
                {
                    throw new TestMethodException($"Pin=[{pin.Name}] does not exist. Referenced from '{id}' field, PinGroup=[{groupName}], ConfigSet=[{configurationSet}], ConfigurationFile=[{configurationFile}].");
                }

                var pinObj = Prime.Services.PinService.Get(pin.Name);
                if (pinObj.IsGroup())
                {
                    throw new TestMethodException($"Pin=[{pin.Name}] is a PinGroup, it must be an individual pin. Referenced from '{id}' field, PinGroup=[{groupName}], ConfigSet=[{configurationSet}], ConfigurationFile=[{configurationFile}].");
                }
            }

            return group.Pins;
        }

        private Dictionary<string, Dictionary<string, string>> GetHwTimingValues(List<string> pins, string edge)
        {
            var attributes = new List<string> { edge };
            var retval = new Dictionary<string, Dictionary<string, string>>(pins.Count);

            foreach (var pin in pins)
            {
                retval[pin] = Prime.Services.PinService.GetPinAttributeValues(pin, attributes);
            }

            return retval;
        }

        private Dictionary<string, string> GetUserVarValuesForPins(List<string> pins)
        {
            var retval = new Dictionary<string, string>(pins.Count);
            foreach (var pin in pins)
            {
                var userVar = this.PinToTimingVarMap[pin];
                retval[pin] = Prime.Services.UserVarService.GetDoubleValue(userVar).ToString();
            }

            return retval;
        }

        private Dictionary<string, string> AdjustUserVars(List<string> pins, string value)
        {
            var result = new Dictionary<string, string>(pins.Count);
            foreach (var pin in pins)
            {
                var userVar = this.PinToTimingVarMap[pin];
                this.Console?.PrintDebug($"\tAdjustUserVars Pin:{pin} UserVar:{userVar} Value={value}");
                result[pin] = value;
                Prime.Services.UserVarService.SetValue(userVar, value);
            }

            return result;
        }

        private void AdjustUserVars(Dictionary<string, string> currentValues, double offset)
        {
            if (currentValues == null)
            {
                return;
            }

            foreach (var pin in currentValues.Keys)
            {
                var userVar = this.PinToTimingVarMap[pin];
                var value = ((currentValues[pin].ToDouble() + offset) * 1e9).ToString("N3") + "ns";
                this.Console?.PrintDebug($"\tAdjustUserVars Pin:{pin} UserVar:{userVar} Value={currentValues[pin]}->{value}");
                Prime.Services.UserVarService.SetValue(userVar, value);
            }
        }

        private void AdjustTimings(Dictionary<string, Dictionary<string, string>> currentTimings, string edge, double offset)
        {
            if (currentTimings == null || currentTimings.Count == 0)
            {
                return;
            }

            foreach (var pin in currentTimings.Keys)
            {
                this.AdjustTimings(pin, currentTimings[pin], edge, offset);
            }
        }

        private void AdjustTimings(string pin, Dictionary<string, string> currentTimingSinglePin, string edge, double offset)
        {
            var newTiming = new Dictionary<string, string>(1);
            newTiming[edge] = (double.Parse(currentTimingSinglePin[edge]) + offset).ToString();
            this.Console?.PrintDebug($"\tAdjustTimings Pin:{pin} CurrentValue:{edge}={currentTimingSinglePin[edge]} UpdatedValue:{newTiming[edge]}");
            Prime.Services.PinService.SetPinAttributeValues(pin, newTiming);
        }

        private void ResolveTimings()
        {
            var tc = Prime.Services.TestConditionService.GetTestCondition(this.TimingsTc);
            tc.Resolve();

            if (Prime.Services.TestConditionService.IsSmartTcEnabled())
            {
                Prime.Services.TestConditionService.FlushSmartTCCategory(SmartTCCategoryType.TIMING);
            }

            /*
            if (this.UpdateTC == ResolveMode.Current)
            {
                var tc = Prime.Services.TestConditionService.GetTestCondition(this.TimingsTc);
                tc.Resolve();

                if (Prime.Services.TestConditionService.IsSmartTcEnabled())
                {
                    Prime.Services.TestConditionService.FlushSmartTCCategory(SmartTCCategoryType.TIMING);
                }
            }
            else
            {
                Prime.Services.TestConditionService.ResolveAllTestConditions();
            } */
        }

        private class RangeContainer
        {
            internal RangeContainer(decimal start, decimal end, decimal stepsize)
            {
                if (stepsize <= 0)
                {
                    throw new ArgumentException($"StepSize=[{stepsize} is invalid. It cannot be negative or 0.", nameof(stepsize));
                }

                if (stepsize > 2.0001E-9m)
                {
                    throw new ArgumentException($"StepSize=[{stepsize} is invalid. It must be <2ns.", nameof(stepsize));
                }

                if (start > end)
                {
                    throw new ArgumentException($"Start=[{start}]  cannot be less then End=[{end}].", nameof(start));
                }

                this.Start = start;
                this.End = end;
                this.Resolution = Math.Abs(stepsize);
                this.NumberOfSteps = (int)((Math.Abs(this.End - this.Start) / this.Resolution) + 1);
                this.NextStep = this.Start + (this.NumberOfSteps * this.Resolution);
            }

            internal decimal Start { get; }

            internal decimal End { get; }

            internal decimal NextStep { get; }

            internal decimal Resolution { get; }

            internal int NumberOfSteps { get; }

            internal List<RangeContainer> SplitRange(decimal maxRange)
            {
                var subRanges = new List<RangeContainer>();
                var start = this.Start;
                var end = this.End;
                while (start < end)
                {
                    var nextRange = new RangeContainer(start, Math.Min(end, start + maxRange), this.Resolution);
                    subRanges.Add(nextRange);
                    start = nextRange.NextStep;
                }

                return subRanges;
            }
        }
    }
}