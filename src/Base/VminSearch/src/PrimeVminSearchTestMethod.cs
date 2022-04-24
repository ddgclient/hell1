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

namespace Prime.TestMethods.VminSearch
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Prime.Base.Utilities;
    using Prime.ConsoleService;
    using Prime.DffService;
    using Prime.FunctionalService;
    using Prime.Kernel.TestMethodsExtension;
    using Prime.PhAttributes;
    using Prime.SharedStorageService;
    using Prime.VoltageService;

    /// <inheritdoc cref="TestMethodBase" />
    [PrimeTestMethod]
    public class PrimeVminSearchTestMethod : TestMethodBase,
        IExtendableTestMethod<IVminSearchExtensions, PrimeVminSearchTestMethod>, IVminSearchExtensions
    {
        /// <summary>
        /// String to be printed when no limiting pattern is found.
        /// </summary>
        internal const string NoLimitingPatternToken = "na";

        private IVoltage voltageHandler;
        private IVoltage initialVoltageHandler;
        private SearchPointTest pointTest;
        private ScoreboardPointTest scoreboardPointTest;
        private List<string> voltageTargets;
        private List<string> multiPassMasks;
        private List<BitArray> multiPassBitArrays;
        private List<string> featureSwitchSettings;
        private bool isFivrVoltageMode;
        private PlistExecutionParameters plistExecutionParameters;
        private SearchExecutionFlowStates searchState;

        /// <summary>
        /// Execution Mode Flag , indicating which mode to apply with the search.
        /// </summary>
        public enum ExecutionModeFlag
        {
            /// <summary>
            /// Search without ScoreBoard.
            /// </summary>
            Search,

            /// <summary>
            /// Search with ScoreBoard.
            /// </summary>
            SearchWithScoreboard,
        }

        /// ************ Start Of PH Parameters ************
        /// <summary>
        /// Gets or sets Patlist to execute.
        /// </summary>
        public TestMethodsParams.Plist Patlist { get; set; }

        /// <summary>
        /// Gets or sets level test condition to load.
        /// </summary>
        public TestMethodsParams.LevelsCondition LevelsTc { get; set; }

        /// <summary>
        /// Gets or sets timing test condition to load.
        /// </summary>
        public TestMethodsParams.TimingCondition TimingsTc { get; set; }

        /// <summary>
        /// Gets or sets the PrePlist callback to plist execution.
        /// </summary>
        public TestMethodsParams.String PrePlist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets targets.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString VoltageTargets { get; set; }

        /// <summary>
        /// Gets or sets startVoltageValues.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString StartVoltages { get; set; }

        /// <summary>
        /// Gets or sets EndVoltageLimits.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString EndVoltageLimits { get; set; }

        /// <summary>
        /// Gets or sets lowerStartVoltages for overshoot.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString StartVoltagesForRetry { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets stepSize.
        /// </summary>
        public TestMethodsParams.Double StepSize { get; set; }

        /// <summary>
        /// Gets or sets FeatureSwitchSettings.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString FeatureSwitchSettings { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets fivrCondition name.
        /// </summary>
        public TestMethodsParams.String FivrCondition { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a list containing the mask bit arrays needed for multi pass capability.
        /// Example: "1100,0101" indicates that two multi pass will be executed.
        /// Where elements corresponding to "1" mean that target will be masked or ignored for the corresponding search.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString MultiPassMasks { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets ExtendedFunctions.
        /// </summary>
        public TestMethodsParams.String IfeObject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an integer number value to prefix the generated scoreboard fail counters.
        /// </summary>
        public TestMethodsParams.CommaSeparatedInteger ScoreboardBaseNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of resolution ticks to step down when scoreboard mode is enabled.
        /// </summary>
        public TestMethodsParams.UnsignedInteger ScoreboardEdgeTicks { get; set; } = 0;

        /// <summary>
        /// Gets or sets a comma separated string of integers which map characters in the pattern name to produce a scoreboard counter.
        /// Positive indexes, starting at 0, map characters at the start of the string.
        /// Negative indexes, starting at -1, map characters at the end of the string.
        /// </summary>
        public TestMethodsParams.String PatternNameMap { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum number of fails that can be processed for scoreboard counters.
        /// This parameter is zero by default. If no other positive value is passed, then the maximum possible integer value will be used.
        /// </summary>
        public TestMethodsParams.UnsignedInteger ScoreboardMaxFails { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum number of times a search can be repeated for recovering purposes..
        /// This parameter is zero by default. Meaning, no repetition will be executed for any search.
        /// </summary>
        public TestMethodsParams.UnsignedInteger MaxRepetitionCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets execution mode, default behaviour is Search without scoreboard.
        /// In order to enable ScoreBoard : executionMode = SearchWithScoreboard and ScoreBoardBaseNum > 0.
        /// </summary>
        public ExecutionModeFlag ExecutionMode { get; set; } = ExecutionModeFlag.Search;

        /// <inheritdoc cref="IVminSearchExtensions" />
        public IVminSearchExtensions TestMethodExtension { get; set; }

        /// <inheritdoc cref="IConsoleService" />
        public IConsoleService Console { get; private set; }

        /// <summary>
        /// Gets the current multi pass mask.
        /// </summary>
        public BitArray CurrentMultiPassMask { get; private set; }

        /// <summary>
        /// Gets the voltage values required to be applied for current search point.
        /// </summary>
        public List<double> VoltageValues => this.pointTest.CurrentState.Voltages;

        /// <summary>
        /// Gets an array containing the mask for each target at current search point.
        /// </summary>
        public BitArray SearchMask => this.pointTest.CurrentState.MaskBits;

        /// <summary>
        /// Gets a list containing the point data for the current search point.
        /// </summary>
        public List<SearchPointData> PointData => this.pointTest.CurrentState.PerPointData;

        /// <summary>
        /// Gets an array containing the initial search mask for each target at current search.
        /// </summary>
        public BitArray InitialSearchMask { get; private set; }

        /// <summary>
        /// Gets the search states data.
        /// </summary>
        public SearchExecutionFlowStates SearchStates => this.searchState;

        /// <summary>
        /// local implementation of former StringUtilities.ConvertKeysToDouble.
        /// </summary>
        /// <param name="separatedString">list of tokens.</param>
        /// <param name="expectedSize">Expected size.</param>
        /// <returns>List of key values as doubles.</returns>
        public static List<double> ConvertKeysToDouble(IEnumerable<string> separatedString, int expectedSize)
        {
            var convertedDouble = new List<double>();
            foreach (var str in separatedString)
            {
                try
                {
                    convertedDouble.Add(str.StringWithUnitsToDouble());
                }
                catch (ArgumentException)
                {
                    if (str.StartsWith("DFF."))
                    {
                        var values = GetValuesFromDff(str.Substring(4));
                        convertedDouble.AddRange(values);
                    }
                    else
                    {
                        var value = Prime.Base.ServiceStore<ISharedStorageService>.Service.GetDoubleRowFromTable(str, SharedStorageService.Context.DUT);
                        convertedDouble.Add(value);
                    }
                }
            }

            if (convertedDouble.Count != 1 && convertedDouble.Count != expectedSize)
            {
                throw new ArgumentException("The amount of values converted to double does not match the expected size of values.");
            }

            if (convertedDouble.Count.Equals(1) && expectedSize > 1)
            {
                convertedDouble = new List<double>(Enumerable.Repeat(convertedDouble.ElementAt(0), expectedSize));
            }

            return convertedDouble;
        }

        /// <inheritdoc/>
        public sealed override void Verify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Services.ConsoleService : null;
            this.voltageTargets = this.VoltageTargets.ToList();
            this.multiPassMasks = this.MultiPassMasks.ToList();
            this.featureSwitchSettings = this.FeatureSwitchSettings.ToList();
            this.plistExecutionParameters = new PlistExecutionParameters()
            {
                Patlist = this.Patlist,
                LevelsTc = this.LevelsTc,
                TimingsTc = this.TimingsTc,
                PrePlist = this.PrePlist,
            };

            this.InitializeVoltageObjects();
            this.InitializePointTest();
            this.InitializeScoreboardPointTest();
            this.InitializeMultiPassBitArrays();
        }

        /// <inheritdoc/>
        [Returns(1, PortType.Pass, "PASS PORT")]
        [Returns(0, PortType.Fail, "FAIL PORT")]
        public override int Execute()
        {
            if (this.TestMethodExtension.GetBypassPort() is var bypassPort && bypassPort >= 0)
            {
                return bypassPort;
            }

            var resolvedPlist = this.ResolvePlistToExecute();

            this.ExecuteSetup(resolvedPlist);
            this.searchState.Init();
            var searchResults = new List<SearchResultData>();

            foreach (var currentMultiPassMask in this.multiPassBitArrays)
            {
                this.CurrentMultiPassMask = currentMultiPassMask;
                this.searchState.StartSearch();

                do
                {
                    this.searchState.RepetitionCount++;
                    var executionIdentifier = this.GetExecutionIdentifier(this.searchState.MultiPassCount, this.searchState.RepetitionCount);
                    this.TestMethodExtension.ApplyPreSearchSetup(this.Patlist);
                    var searchIdentifiers = new SearchIdentifiers(executionIdentifier, this.searchState.MultiPassCount, this.searchState.RepetitionCount);
                    if (!this.pointTest.Reset())
                    {
                        searchResults.Add(this.CreateSearchResultData(false, searchIdentifiers));
                        if (!this.TestMethodExtension.HasToContinueToNextSearch(searchResults, this.pointTest.SearchPlist))
                        {
                            break;
                        }

                        continue;
                    }

                    this.InitialSearchMask = new BitArray(this.pointTest.CurrentState.MaskBits);
                    var isLastSearchPointPass = this.ExecuteSingleVminSearch();
                    this.searchState.IsAnySearchPassing |= isLastSearchPointPass;
                    searchResults.Add(this.CreateSearchResultData(isLastSearchPointPass, searchIdentifiers));
                    this.TestMethodExtension.ExecuteScoreboard(executionIdentifier, isLastSearchPointPass);

                    this.searchState.HasToRepeatSearch = this.TestMethodExtension.HasToRepeatSearch(searchResults);
                    this.searchState.HasToAbortSearches = !this.TestMethodExtension.HasToContinueToNextSearch(searchResults, this.pointTest.SearchPlist);
                }
                while (this.searchState.HasToRepeatSearch && this.searchState.RepetitionCount < this.MaxRepetitionCount && !this.searchState.HasToAbortSearches);

                if (this.searchState.HasToAbortSearches)
                {
                    break;
                }
            }

            this.RestoreVoltage();

            var port = this.TestMethodExtension.PostProcessSearchResults(searchResults);
            return this.searchState.IsAnySearchPassing ? port : 0;
        }

        // **************************************************************
        // SECTION START: Default implementation for extendable functions.
        // **************************************************************

        /// <inheritdoc/>
        IVoltage IVminSearchExtensions.GetSearchVoltageObject(List<string> targets, string plistName)
        {
            if (this.isFivrVoltageMode)
            {
                return string.IsNullOrEmpty(this.FivrCondition.ToString()) ?
                    Services.VoltageService.CreateFivrForDomains(targets, this.Patlist) :
                    Services.VoltageService.CreateFivrForDomainsAndCondition(targets, this.FivrCondition.ToString(), this.Patlist);
            }

            var attributes = this.GetVForceAttributesFromLevel();
            return Services.VoltageService.CreateVForceForPinAttribute(targets, attributes);
        }

        /// <inheritdoc/>
        IFunctionalTest IVminSearchExtensions.GetFunctionalTest(string patlist, string levelsTc, string timingsTc, string prePlist)
        {
            return Services.FunctionalService.CreateCaptureFailureTest(patlist, levelsTc, timingsTc, 1, prePlist);
        }

        /// <inheritdoc />
        bool IVminSearchExtensions.IsSinglePointMode() => false;

        /// <inheritdoc />
        bool IVminSearchExtensions.IsCheckOfResultBitsEnabled() => true;

        /// <inheritdoc />
        int IVminSearchExtensions.GetBypassPort() => -1;

        /// <inheritdoc />
        void IVminSearchExtensions.ApplyPreExecuteSetup(string plistName)
        {
            // Method intentionally left empty.
        }

        /// <inheritdoc />
        void IVminSearchExtensions.ApplyInitialVoltage(IVoltage voltageObject)
        {
            if (voltageObject is IFivrCondition voltage)
            {
                voltage.ApplyCondition();
            }
        }

        /// <inheritdoc />
        void IVminSearchExtensions.ApplyPreSearchSetup(string plistName)
        {
            // Method intentionally left empty.
        }

        /// <inheritdoc />
        List<double> IVminSearchExtensions.GetStartVoltageValues(List<string> startVoltageKeys) =>
            ConvertKeysToDouble(startVoltageKeys, this.voltageTargets.Count);

        /// <inheritdoc />
        List<double> IVminSearchExtensions.GetEndVoltageLimitValues(List<string> endVoltageLimitKeys) =>
            ConvertKeysToDouble(endVoltageLimitKeys, this.voltageTargets.Count);

        /// <inheritdoc />
        List<double> IVminSearchExtensions.GetLowerStartVoltageValues(List<string> lowerStartVoltageKeys) =>
            ConvertKeysToDouble(lowerStartVoltageKeys, this.voltageTargets.Count);

        /// <inheritdoc/>
        BitArray IVminSearchExtensions.GetInitialMaskBits() => this.CurrentMultiPassMask;

        /// <inheritdoc />
        void IVminSearchExtensions.ApplySearchVoltage(IVoltage voltageObject, List<double> voltageValues)
        {
            this.Console?.PrintDebug($"VminSearchSP: Search point voltage values=[{string.Join(",", voltageValues)}]\n");
            if (voltageObject is IVForcePinAttribute pinVoltage)
            {
                pinVoltage.Apply(voltageValues);
            }

            if (voltageObject is IFivrDomains domainVoltage)
            {
                domainVoltage.Apply(voltageValues);
            }
        }

        /// <inheritdoc/>
        void IVminSearchExtensions.ApplyMask(BitArray maskBits, IFunctionalTest functionalTest)
        {
            // Method intentionally left empty.
        }

        /// <inheritdoc/>
        BitArray IVminSearchExtensions.ProcessPlistResults(bool plistExecuteResult, IFunctionalTest functionalTest) =>
            new BitArray(this.voltageTargets.Count, !plistExecuteResult);

        /// <inheritdoc/>
        void IVminSearchExtensions.ExecuteScoreboard(string executionIdentifier, bool isLastSearchPointPass)
        {
            if (this.scoreboardPointTest == null)
            {
                return;
            }

            if (this.TestMethodExtension.IsSinglePointMode() && this.pointTest.SearchPlist is ICaptureFailureTest failureTest && failureTest.IsPerPatternCaptureEnabled())
            {
                this.scoreboardPointTest.GenerateScoreboardCounters(executionIdentifier, failureTest);
            }
            else
            {
                this.scoreboardPointTest.Execute(this.pointTest.CurrentState.PerPointData, this.InitialSearchMask, executionIdentifier, isLastSearchPointPass);
            }
        }

        /// <inheritdoc />
        bool IVminSearchExtensions.HasToRepeatSearch(List<SearchResultData> searchResults) => false;

        /// <inheritdoc/>
        bool IVminSearchExtensions.HasToContinueToNextSearch(List<SearchResultData> searchResults, IFunctionalTest functionalTest) => true;

        /// <inheritdoc />
        int IVminSearchExtensions.PostProcessSearchResults(List<SearchResultData> searchResults) => this.BasePostProcessSearchResults(searchResults);

        /// <summary>
        /// Original PortProcessSearchResult implementation.
        /// </summary>
        /// <param name="searchResults">Result from all completed searches.</param>
        /// <returns>A value indicating the exit port.</returns>
        public int BasePostProcessSearchResults(List<SearchResultData> searchResults)
        {
            if (this.TestMethodExtension.IsSinglePointMode())
            {
                return 1;
            }

            var printPerTargetIncrements = this.featureSwitchSettings.Contains("print_per_target_increments");
            var printIndependentSearchResults = this.featureSwitchSettings.Contains("print_results_for_all_searches");
            if (printIndependentSearchResults)
            {
                DataLogger.PrintResultsForAllSearches(searchResults, this.PatternNameMap, printPerTargetIncrements);
            }
            else if (searchResults.Count != 0)
            {
                DataLogger.PrintMergedSearchResults(searchResults, this.PatternNameMap, printPerTargetIncrements);
            }

            return 1;
        }

        private static List<double> GetValuesFromDff(string key)
        {
            var dffValue = Prime.Base.ServiceStore<IDffService>.Service.GetDff(key);
            var valuesAsStrings = dffValue.ToLower().Split('v').ToList();
            return valuesAsStrings.Select(stringValue => double.Parse(stringValue)).ToList();
        }

        // **************************************************************
        // SECTION END: Default implementation for extended functions.
        // **************************************************************
        private static bool MaskContainsOnlyValidValues(string currentMultiPassMask) =>
            currentMultiPassMask.All(mask => mask == '0' || mask == '1');

        private void ExecuteSetup(string patList)
        {
            this.TestMethodExtension.ApplyPreExecuteSetup(patList);
            this.pointTest.ApplyTestConditions();
            this.VoltageObjectsExecuteSetup();
        }

        private string GetExecutionIdentifier(uint multiPassCount, uint repetitionCount)
        {
            var executionIdentifier = this.multiPassBitArrays.Count > 1 ? "M" + multiPassCount : string.Empty;
            executionIdentifier += this.MaxRepetitionCount > 1 ? "R" + repetitionCount : string.Empty;
            this.Console?.PrintDebug($"VminSearchTM: Multi search identifier=[{executionIdentifier}]");
            return executionIdentifier;
        }

        private void VoltageObjectsExecuteSetup()
        {
            this.voltageHandler?.Reset();
            this.initialVoltageHandler?.Reset();
            this.TestMethodExtension.ApplyInitialVoltage(this.initialVoltageHandler);
        }

        private void InitializePointTest()
        {
            if (this.StepSize <= 0)
            {
                throw new Base.Exceptions.TestMethodException("StepSize must be higher than 0");
            }

            this.pointTest = new SearchPointTest(
                this.plistExecutionParameters,
                this.StepSize,
                this.voltageTargets,
                this.TestMethodExtension,
                this.featureSwitchSettings,
                this.Console)
            {
                StartVoltageKeys = this.StartVoltages.ToList(),
                EndVoltageLimitKeys = this.EndVoltageLimits.ToList(),
                LowerStartVoltageKeys = this.StartVoltagesForRetry.ToList(),
            };
        }

        private void InitializeScoreboardPointTest()
        {
            if (this.ExecutionMode != ExecutionModeFlag.SearchWithScoreboard)
            {
                this.scoreboardPointTest = null;
                return;
            }

            var scoreboardBaseNumbers = this.ScoreboardBaseNumber.ToList();

            // Case where the Mode is enabled but no base numbers supplied.
            if (scoreboardBaseNumbers.Count == 0)
            {
                throw new Base.Exceptions.TestMethodException("scoreboardBaseNumbers must not be empty.");
            }

            var scoreboardExecutionParameters = new ScoreboardPointTest.ScoreboardExecutionParameters()
            {
                MaxFails = this.ScoreboardMaxFails,
                EdgeTicks = this.ScoreboardEdgeTicks,
                BaseNumbers = this.ScoreboardBaseNumber,
                PatternNameMap = this.PatternNameMap,
                PrintScoreboardCounters = this.featureSwitchSettings.Contains("print_scoreboard_counters"),
            };

            this.Console?.PrintDebug($"VminSearchTM: Scoreboard is enable with edge ticks=[{this.ScoreboardEdgeTicks}]");
            this.scoreboardPointTest = new ScoreboardPointTest(
                this.plistExecutionParameters,
                scoreboardExecutionParameters,
                this.voltageHandler,
                this.TestMethodExtension,
                this.Console);
        }

        private SearchResultData CreateSearchResultData(bool searchStatus, SearchIdentifiers searchIdentifiers)
        {
            return new SearchResultData(
                    this.pointTest.CurrentState,
                    searchStatus,
                    searchIdentifiers);
        }

        private void InitializeVoltageObjects()
        {
            this.voltageHandler = null;
            this.initialVoltageHandler = null;
            this.isFivrVoltageMode = this.featureSwitchSettings.Contains("fivr_mode_on");

            if (this.voltageTargets.Count.Equals(0))
            {
                throw new Base.Exceptions.TestMethodException("VoltageTargets must not be empty");
            }

            this.voltageHandler = this.TestMethodExtension.GetSearchVoltageObject(this.voltageTargets, this.Patlist);

            if (!string.IsNullOrEmpty(this.FivrCondition.ToString()))
            {
                this.initialVoltageHandler = this.isFivrVoltageMode ? this.voltageHandler :
                    Services.VoltageService.CreateFivrForCondition(this.FivrCondition.ToString(), this.Patlist);
            }
        }

        private bool ExecuteSingleVminSearch()
        {
            bool isLastSearchPointPass;
            do
            {
                this.TestMethodExtension.ApplySearchVoltage(this.voltageHandler, this.VoltageValues);

                var isPlistPass = this.pointTest.Execute();
                isLastSearchPointPass = this.pointTest.ProcessResults(isPlistPass);
            }
            while (!this.pointTest.IsSearchCompleted(isLastSearchPointPass));

            return isLastSearchPointPass;
        }

        private Dictionary<string, Dictionary<string, string>> GetVForceAttributesFromLevel()
        {
            var vForceMandatoryAttributes = new Dictionary<string, Dictionary<string, string>>();
            var level = Services.TestConditionService.GetTestCondition(this.LevelsTc);
            foreach (var voltageTarget in this.voltageTargets.Distinct())
            {
                var pin = Services.PinService.Get(voltageTarget);
                var attributes = pin.GetVforceMandatoryAttributes();
                vForceMandatoryAttributes.Add(voltageTarget, new Dictionary<string, string>());
                foreach (var attribute in attributes)
                {
                    vForceMandatoryAttributes[voltageTarget][attribute] =
                        level.GetPinAttributeValue(voltageTarget, attribute);
                }
            }

            return vForceMandatoryAttributes;
        }

        private void InitializeMultiPassBitArrays()
        {
            if (string.IsNullOrEmpty(this.MultiPassMasks))
            {
                this.multiPassBitArrays = new List<BitArray>() { new BitArray(this.voltageTargets.Count) };
                return;
            }

            this.multiPassBitArrays = new List<BitArray>(this.multiPassMasks.Count);
            foreach (var multiPassMask in this.multiPassMasks)
            {
                this.multiPassBitArrays.Add(this.GetMultiPassBitArray(multiPassMask));
            }
        }

        private BitArray GetMultiPassBitArray(string currentMultiPassMask)
        {
            if (!MaskContainsOnlyValidValues(currentMultiPassMask))
            {
                this.Console?.PrintDebug($"VminSearchTM: Current multi pass mask=[{currentMultiPassMask}] contains at least one invalid value. Only initial mask bits will be used.");
                return new BitArray(this.voltageTargets.Count);
            }

            return new BitArray(currentMultiPassMask.Select(x => x == '1').ToArray());
        }

        private void RestoreVoltage()
        {
            if (this.voltageHandler is IVForcePinAttribute pinAttributeVoltage)
            {
                pinAttributeVoltage.Restore();
            }
        }

        private string ResolvePlistToExecute()
        {
            var resolvedPlist = this.pointTest.ResolvePlist(this.InstanceName);
            this.scoreboardPointTest?.ResolvePlist(this.InstanceName);

            this.Console?.PrintDebug($"VminSearchTM: Resolved Plist name=[{resolvedPlist}]");

            return resolvedPlist;
        }

        /// <summary>
        /// Contains the parameters for the plist execution to create the Functional Service instance.
        /// </summary>
        public struct PlistExecutionParameters
        {
            /// <summary>
            /// Pattern list name.
            /// </summary>
            public string Patlist;

            /// <summary>
            /// Test instance levels test condition.
            /// </summary>
            public string LevelsTc;

            /// <summary>
            /// Test instance timings test condition.
            /// </summary>
            public string TimingsTc;

            /// <summary>
            /// The callback to run before the plist execution.
            /// </summary>
            public string PrePlist;
        }

        /// <summary>
        /// Contains search states.
        /// </summary>
        public struct SearchExecutionFlowStates
        {
            /// <summary>
            /// Multipass iteration count.
            /// </summary>
            public uint MultiPassCount;

            /// <summary>
            /// Gets or sets whether any search is passing.
            /// </summary>
            public bool IsAnySearchPassing;

            /// <summary>
            /// Gets or sets whether the search should abort execution.
            /// </summary>
            public bool HasToAbortSearches;

            /// <summary>
            /// Gets or sets whether the search should repeat an execution.
            /// </summary>
            public bool HasToRepeatSearch;

            /// <summary>
            /// Repetition iteration count.
            /// </summary>
            public uint RepetitionCount;

            /// <summary>
            /// Sets initial states.
            /// </summary>
            public void Init()
            {
                this.MultiPassCount = 0U;
                this.IsAnySearchPassing = false;
                this.HasToAbortSearches = false;
            }

            /// <summary>
            /// Move counters for each search iteration.
            /// </summary>
            public void StartSearch()
            {
                this.MultiPassCount++;
                this.HasToRepeatSearch = false;
                this.RepetitionCount = 0U;
            }
        }
    }
}
