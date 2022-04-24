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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Prime.TestMethods.VminSearch.Helpers;

    /// <summary>
    /// A class to provide access to data-logging functionality.
    /// </summary>
    public static class DataLogger
    {
        private const string TokenValueSeparator = "|";
        private const string TargetValueSeparator = "_";
        private const string LimitingPatternsLogSeparator = "^";

        // used for Trace logging to Ituff
        private const string TraceConfigurationSeparator = ":";
        private const string TraceConfigurationSuffix = "_vminFwCfg";
        private const int TraceSkipSearchValue = 9999;

        /// <summary>
        /// Print Vmin forwarding configuration data to ituff.
        /// This printout consumed by Trace to configure Vmin Forwarding.
        /// Since search is executed for instances at the same corner , one frequency input value expected.
        /// e.g tname_VminTestName_vminFwCfg
        ///     strgval_CRCORE7@F6:4:5.500_CRCORE6@F6:4:5.500_CRCORE5@F6:4:5.500_CRCORE4@F6:4:5.500.
        ///             [domain][instanceName]@[CornerValue]:[FlowId]:[FrequencyValue].
        /// </summary>
        /// <param name="frequencyRawValue">Frequency value for the corner.</param>
        /// <param name="flowId">current flow id data.</param>
        /// <param name="vminFwCornerIdentifiers">corner identifiers value matching the format : [domain]_[instanceName]@[cornerID] e.g:CR_CORE7@F6.</param>
        public static void LogVminForwardingDataForTrace(double frequencyRawValue, int flowId, List<string> vminFwCornerIdentifiers)
        {
            if (vminFwCornerIdentifiers == null || vminFwCornerIdentifiers.Count == 0)
            {
                return;
            }

            var vminFwCornerIdsTraceFormat = vminFwCornerIdentifiers.Select(cornerId => cornerId.Replace("_", string.Empty)).ToList();

            // frequency value calculation in Ghz units, 3 decimal places.
            var frequencyLoggingValue = (frequencyRawValue / 1e9).ToString("F3");

            var dataLogWriter = Services.DatalogService.GetItuffStrgvalWriter();
            dataLogWriter.SetTnamePostfix(TraceConfigurationSuffix);

            StringBuilder fwConfigurationData = new StringBuilder();

            // iterating over all cornerIds:
            foreach (var cornderIdentifier in vminFwCornerIdsTraceFormat)
            {
                fwConfigurationData.Append(cornderIdentifier);
                fwConfigurationData.Append(TraceConfigurationSeparator);
                fwConfigurationData.Append(flowId);
                fwConfigurationData.Append(TraceConfigurationSeparator);
                fwConfigurationData.Append(frequencyLoggingValue);
                fwConfigurationData.Append(TargetValueSeparator);
            }

            // remove the last '_' sign.
            fwConfigurationData.Length--;

            dataLogWriter.SetData(fwConfigurationData.ToString());
            Services.DatalogService.WriteToItuff(dataLogWriter);
        }

        /// <summary>
        /// Logs the resulting voltages from the search as well as the start and end voltage values for all search targets.
        /// Logs the voltage limiting patterns when the patternNameMap is not empty.
        /// Logs the voltage increments per target when the option is enabled through the FeatureSwitchSettings parameter.
        /// </summary>
        /// <param name="searchResults"> A list containing the accumulated data.</param>
        /// <param name="patternNameMap">Gets or sets a comma separated string of integers which map characters in the pattern name to produce a scoreboard counter.</param>>
        /// <param name="printPerTargetIncrements">Bool that indicates whether or not to print the increments per target.</param>>
        public static void PrintResultsForAllSearches(IEnumerable<SearchResultData> searchResults, string patternNameMap, bool printPerTargetIncrements)
        {
            foreach (var searchResult in searchResults)
            {
                if (searchResult.ExecutionCount != 0)
                {
                    var postfix = searchResult.TnamePostfix;

                    WriteSearchVoltageResultsToItuff(
                        searchResult.Voltages.ToArray(),
                        searchResult.StartVoltages.ToArray(),
                        searchResult.EndVoltageLimits.ToArray(),
                        searchResult.ExecutionCount,
                        postfix);
                    if (!string.IsNullOrEmpty(patternNameMap))
                    {
                        WriteLimitingPatternsToItuff(
                            searchResult.VoltageLimitingPatterns.ToArray(),
                            patternNameMap,
                            postfix);
                    }

                    if (printPerTargetIncrements)
                    {
                        WritePerTargetIncrementsToItuff(searchResult.PerTargetIncrements.ToArray(), postfix);
                    }
                }
                else if (!string.IsNullOrEmpty(searchResult.FailReason))
                {
                    LogSkipSearch(searchResult.StartVoltages, searchResult.EndVoltageLimits, Enumerable.Repeat(TraceSkipSearchValue, searchResult.StartVoltages.Count).ToList());
                }
            }
        }

        /// <summary>
        /// Logs the joined resulting voltages from the multi-pass search as well as the start and end voltage values for all search targets.
        /// Logs the voltage limiting patterns when the patternNameMap is not empty.
        /// Logs the voltage increments per target when the option is enabled through the FeatureSwitchSettings parameter.
        /// </summary>
        /// <param name="searchResults"> A list containing the accumulated data.</param>
        /// <param name="patternNameMap">A comma separated string of integers which map characters in the pattern name to produce a scoreboard counter.</param>
        /// <param name="printPerTargetIncrements">Bool that indicates whether or not to print the increments per target.</param>
        public static void PrintMergedSearchResults(IEnumerable<SearchResultData> searchResults, string patternNameMap, bool printPerTargetIncrements)
        {
            var mergedSearchResultData = MergeSearchResults(searchResults, !string.IsNullOrEmpty(patternNameMap), printPerTargetIncrements);

            if (mergedSearchResultData.ExecutionCount != 0)
            {
                WriteSearchVoltageResultsToItuff(mergedSearchResultData.ResultVoltages, mergedSearchResultData.StartVoltages, mergedSearchResultData.EndVoltages, mergedSearchResultData.ExecutionCount);
                if (!string.IsNullOrEmpty(patternNameMap))
                {
                    WriteLimitingPatternsToItuff(mergedSearchResultData.LimitingPatterns, patternNameMap);
                }

                if (printPerTargetIncrements)
                {
                    WritePerTargetIncrementsToItuff(mergedSearchResultData.PerTargetIncrements);
                }
            }
            else if (!string.IsNullOrEmpty(mergedSearchResultData.FailReason))
            {
                LogSkipSearch(new List<double>(mergedSearchResultData.StartVoltages), new List<double>(mergedSearchResultData.EndVoltages), Enumerable.Repeat(TraceSkipSearchValue, mergedSearchResultData.StartVoltages.Length).ToList());
            }
        }

        /// <summary>
        /// Logs a skipped search with its fail reason.
        /// </summary>
        /// <param name="startVoltage">Start voltage list.</param>
        /// <param name="endVoltage">End voltage list.</param>
        /// <param name="searchInvalidValues">Invalid voltages values list.</param>
        public static void LogSkipSearch(List<double> startVoltage, List<double> endVoltage, List<int> searchInvalidValues)
        {
            var startValues = string.Join(TargetValueSeparator, startVoltage.Select(i => $"{i:F3}"));
            var endValues = string.Join(TargetValueSeparator, endVoltage.Select(i => $"{i:F3}"));
            var searchValues = string.Join(TargetValueSeparator, searchInvalidValues.Select(i => i));
            var ituffOutput = searchValues + TokenValueSeparator + startValues + TokenValueSeparator + endValues;

            var outputWriter = Services.DatalogService.GetItuffStrgvalWriter();
            outputWriter.SetData(ituffOutput);
            Services.DatalogService.WriteToItuff(outputWriter);
        }

        /// <summary>
        /// Merges the resulting voltages from the multi-pass search as well as the start and end voltage values
        /// of all search targets for data-logging purposes.
        /// </summary>
        /// <param name="searchResults"> A list containing the results data to merge.</param>
        /// <param name="includeLimitingPatterns">Bool that indicates whether or not to include Limiting Patterns data.</param>
        /// <param name="includePerTargetIncrements">Bool that indicates whether or not to include the increments per target.</param>
        /// <returns>The MergedSearchResultsData object containing all merged data.</returns>
        private static MergedSearchResultData MergeSearchResults(IEnumerable<SearchResultData> searchResults, bool includeLimitingPatterns, bool includePerTargetIncrements)
        {
            var mergedSearchResultsData = default(MergedSearchResultData);
            var searchResultData = searchResults as SearchResultData[] ?? searchResults.ToArray();
            var currentMultiPassCount = searchResultData.Last().MultiPassCount + 1;

            foreach (var searchResult in searchResultData.Reverse())
            {
                if (mergedSearchResultsData.ExecutionCount == 0U)
                {
                    mergedSearchResultsData.ResultVoltages = searchResult.Voltages.ToArray();
                    mergedSearchResultsData.StartVoltages = searchResult.StartVoltages.ToArray();
                    mergedSearchResultsData.EndVoltages = searchResult.EndVoltageLimits.ToArray();
                    mergedSearchResultsData.LimitingPatterns = searchResult.VoltageLimitingPatterns.ToArray();
                    mergedSearchResultsData.PerTargetIncrements = searchResult.PerTargetIncrements.ToArray();
                    mergedSearchResultsData.ExecutionCount = 0U;
                    mergedSearchResultsData.FailReason = searchResult.FailReason;
                }
                else if (searchResult.MultiPassCount < currentMultiPassCount)
                {
                    mergedSearchResultsData.FailReason = searchResult.FailReason;
                    for (var targetIndex = 0; targetIndex < searchResult.Voltages.Count; targetIndex++)
                    {
                        if (!searchResult.Voltages[targetIndex].IsEqual(SearchPointTest.VoltageMaskValue)
                            && !mergedSearchResultsData.ResultVoltages[targetIndex].IsEqual(SearchPointTest.VoltageFailValue))
                        {
                            mergedSearchResultsData.UpdateMergedSearchResults(includeLimitingPatterns, includePerTargetIncrements, searchResult, targetIndex);
                        }
                    }
                }
                else
                {
                    for (var targetIndex = 0; targetIndex < searchResult.Voltages.Count; targetIndex++)
                    {
                        if (!searchResult.Voltages[targetIndex].IsEqual(SearchPointTest.VoltageMaskValue)
                            && mergedSearchResultsData.ResultVoltages[targetIndex].IsEqual(SearchPointTest.VoltageMaskValue))
                        {
                            mergedSearchResultsData.UpdateMergedSearchResults(includeLimitingPatterns, includePerTargetIncrements, searchResult, targetIndex);
                        }
                    }
                }

                mergedSearchResultsData.ExecutionCount += searchResult.ExecutionCount;
                currentMultiPassCount = searchResult.MultiPassCount;
            }

            return mergedSearchResultsData;
        }

        /// <summary>
        /// Updates the merged results for the specified target index with the given search result data.
        /// </summary>
        /// <param name="mergedSearchResultsData">The merged search result data to update.</param>
        /// <param name="includeLimitingPatterns">Bool that indicates whether or not to include Limiting Patterns data.</param>
        /// <param name="includePerTargetIncrements">Bool that indicates whether or not to include the increments per target.</param>
        /// <param name="searchResult">The search result data object from which to update values.</param>
        /// <param name="targetIndex">The index of the current target being updated.</param>
        private static void UpdateMergedSearchResults(
            ref this MergedSearchResultData mergedSearchResultsData,
            bool includeLimitingPatterns,
            bool includePerTargetIncrements,
            SearchResultData searchResult,
            int targetIndex)
        {
            var needToUpdateVoltage = searchResult.Voltages[targetIndex].IsEqual(SearchPointTest.VoltageFailValue) ||
                                      searchResult.Voltages[targetIndex] > mergedSearchResultsData.ResultVoltages[targetIndex];
            if (needToUpdateVoltage)
            {
                mergedSearchResultsData.ResultVoltages[targetIndex] = searchResult.Voltages[targetIndex];
                mergedSearchResultsData.StartVoltages[targetIndex] = searchResult.StartVoltages[targetIndex];
                mergedSearchResultsData.EndVoltages[targetIndex] = searchResult.EndVoltageLimits[targetIndex];
                if (includeLimitingPatterns)
                {
                    mergedSearchResultsData.LimitingPatterns[targetIndex] = searchResult.VoltageLimitingPatterns[targetIndex];
                }

                if (includePerTargetIncrements)
                {
                    mergedSearchResultsData.PerTargetIncrements[targetIndex] = searchResult.PerTargetIncrements[targetIndex];
                }
            }
        }

        /// <summary>
        /// Returns a string containing the characters that are mapped by the provided index map.
        /// </summary>
        /// <param name="stringToMap">The string from which to map the indexed characters.</param>
        /// <param name="indexMap">A string with comma-separated integer indexes.
        /// Each index represents the position of a corresponding character in the string to map.
        /// Positive indexes starting at 0 map to characters at the start of the string to map and forward.
        /// Negative indexes starting at -1 map to characters at the end of the string to map and backwards.</param>
        /// <returns>The string of characters that were mapped with the index map.</returns>
        private static string GetMappedString(string stringToMap, string indexMap)
        {
            var mappingIndexes = indexMap.Split(',').ToList();
            var result = mappingIndexes.Aggregate(string.Empty, (accumulator, currentValue) =>
            {
                var intIndex = int.Parse(currentValue);
                var positiveOnlyIndex = (intIndex < 0) ? stringToMap.Length + intIndex : intIndex;
                return accumulator + stringToMap[positiveOnlyIndex];
            });

            return result;
        }

        /// <summary>
        /// Prints to the ituff file a format string containing the search result voltages, start voltages and end voltages.
        /// </summary>
        /// <param name="resultVoltages">Result voltages for the current search.</param>
        /// <param name="startVoltages">Start voltages for the current search.</param>
        /// <param name="endVoltages">End voltages for the current search.</param>
        /// <param name="executionCount">Execution count for the current search.</param>
        /// <param name="postfix">Instance name postfix to be set.</param>
        private static void WriteSearchVoltageResultsToItuff(IEnumerable<double> resultVoltages, IEnumerable<double> startVoltages, IEnumerable<double> endVoltages, uint executionCount, string postfix = "")
        {
            var outputSearchVoltages = string.Join(TargetValueSeparator, resultVoltages.Select(GetFormattedVoltageValue));
            var outputStartVoltages = string.Join(TargetValueSeparator, startVoltages.Select(GetFormattedVoltageValue));
            var outputEndVoltages = string.Join(TargetValueSeparator, endVoltages.Select(GetFormattedVoltageValue));
            var outputVoltages = outputSearchVoltages + TokenValueSeparator + outputStartVoltages +
                                 TokenValueSeparator + outputEndVoltages + TokenValueSeparator + executionCount;

            var writer = Services.DatalogService.GetItuffStrgvalWriter();
            if (!string.IsNullOrEmpty(postfix))
            {
                writer.SetTnamePostfix(TargetValueSeparator + postfix);
            }

            writer.SetData(outputVoltages);
            Services.DatalogService.WriteToItuff(writer);
        }

        private static string GetFormattedVoltageValue(double value)
        {
            var formattedString = $"{value:F3}";
            return formattedString.Length <= 5 ? formattedString : formattedString.Substring(0, 5);
        }

        /// <summary>
        /// Prints to the ituff file a format string containing the limiting patterns.
        /// </summary>
        /// <param name="limitingPatterns">Limiting pattern counter for each search target.</param>
        /// <param name="patternNameMap">A comma separated string of integers which map characters in the pattern name to produce a scoreboard counter.</param>
        /// <param name="postfix">Instance name postfix to be set.</param>
        private static void WriteLimitingPatternsToItuff(string[] limitingPatterns, string patternNameMap, string postfix = "")
        {
            var printLimitingPatterns = false;
            var limitingPatternsString = Enumerable.Repeat(PrimeVminSearchTestMethod.NoLimitingPatternToken, limitingPatterns.Length).ToArray();
            for (var targetIndex = 0; targetIndex < limitingPatterns.Length; targetIndex++)
            {
                if (limitingPatterns[targetIndex] != PrimeVminSearchTestMethod.NoLimitingPatternToken)
                {
                    printLimitingPatterns = true;
                    limitingPatternsString[targetIndex] = GetMappedString(limitingPatterns[targetIndex], patternNameMap);
                }
            }

            if (printLimitingPatterns)
            {
                var outputLimitingPatterns = string.Join(LimitingPatternsLogSeparator, limitingPatternsString);
                var strgvalWriter = Services.DatalogService.GetItuffStrgvalWriter();
                var tnamePostfix = string.IsNullOrEmpty(postfix)
                    ? TargetValueSeparator + "lp"
                    : TargetValueSeparator + postfix + TargetValueSeparator + "lp";
                strgvalWriter.SetTnamePostfix(tnamePostfix);
                strgvalWriter.SetData(outputLimitingPatterns);
                Services.DatalogService.WriteToItuff(strgvalWriter);
            }
        }

        /// <summary>
        /// Prints the voltage increments per target to the ituff.
        /// </summary>
        /// <param name="perTargetIncrements">A list containing the voltage increments per target.</param>
        /// <param name="postfix">Instance name postfix to be set.</param>>
        private static void WritePerTargetIncrementsToItuff(uint[] perTargetIncrements, string postfix = "")
        {
            var outPerTargetIncrements = string.Join(TargetValueSeparator, perTargetIncrements);
            var strgvalWriter = Services.DatalogService.GetItuffStrgvalWriter();
            var tnamePostfix = string.IsNullOrEmpty(postfix)
                ? TargetValueSeparator + "it"
                : TargetValueSeparator + postfix + TargetValueSeparator + "it";
            strgvalWriter.SetTnamePostfix(tnamePostfix);
            strgvalWriter.SetData(outPerTargetIncrements);
            Services.DatalogService.WriteToItuff(strgvalWriter);
        }

        /// <summary>
        /// A struct to hold merged results data for data-logging purposes.
        /// </summary>
        private struct MergedSearchResultData
        {
            public double[] ResultVoltages { get; set; }

            public double[] StartVoltages { get; set; }

            public double[] EndVoltages { get; set; }

            public uint ExecutionCount { get; set; }

            public string[] LimitingPatterns { get; set; }

            public uint[] PerTargetIncrements { get; set; }

            public string FailReason { get; set; }
        }
    }
}
