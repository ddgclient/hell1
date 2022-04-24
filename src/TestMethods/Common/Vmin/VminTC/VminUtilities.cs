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

namespace VminTC
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using DDG;
    using Prime.FunctionalService;
    using Prime.PlistService;
    using Prime.TestMethods.VminSearch;

    /// <summary>
    /// Static utility functions.
    /// </summary>
    public static class VminUtilities
    {
        /// <summary>
        /// VoltageMaskValue.
        /// </summary>
        public const double VoltageMaskValue = -8888D;

        /// <summary>
        /// VoltageFailValue.
        /// </summary>
        public const double VoltageFailValue = -9999D;

        /// <summary>
        /// Evaluates DieRecovery rules.
        /// </summary>
        /// <param name="searchResults">Accumulated search results.</param>
        /// <param name="recoveryOptions">RecoveryOptions parameter.</param>
        /// <param name="tracker">DieRecovery outgoing tracker.</param>
        /// <returns>Result bits.</returns>
        public static BitArray RunRules(ref this SearchResults searchResults, string recoveryOptions, IDieRecovery tracker)
        {
            var resultBits = searchResults.TestResultsBits.Or();
            if (string.IsNullOrEmpty(recoveryOptions))
            {
                searchResults.FailedRules = resultBits.Cast<bool>().Contains(true);
                return new BitArray(resultBits);
            }

            if (tracker != null)
            {
                resultBits.Or(tracker.GetMaskBits());
            }

            recoveryOptions = recoveryOptions.Replace(" ", string.Empty);
            var recoveryOptionsListRegx = new Regex(@"^[01]+(,[01]+)*$");
            if (recoveryOptionsListRegx.IsMatch(recoveryOptions))
            {
                var options = recoveryOptions.Split(',');
                foreach (var option in options)
                {
                    var combination = option.ToBitArray();
                    var intermediate = new BitArray(combination);
                    intermediate.Or(resultBits);
                    var passing = intermediate.Xor(combination).OfType<bool>().All(e => !e);
                    if (!passing)
                    {
                        continue;
                    }

                    searchResults.FailedRules = false;
                    return combination;
                }
            }
            else
            {
                var options = recoveryOptions.Split(',');
                if (options.Length != 2)
                {
                    throw new Exception($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Invalid RecoveryOptions={recoveryOptions}. Please enter list of bit vectors or DieRecovery GroupName,Size.");
                }

                var groupName = options[0].Trim();
                var ruleSize = options[1].Trim().ToInt();

                if (tracker != null)
                {
                    var defeatureRulesList = tracker.RunRule(resultBits, groupName);
                    if (defeatureRulesList.Count <= 0)
                    {
                        searchResults.FailedRules = true;
                        return resultBits;
                    }

                    var firstPassing = defeatureRulesList.First();
                    if (firstPassing.Size >= ruleSize)
                    {
                        searchResults.FailedRules = false;
                        return defeatureRulesList.First().BitVector.ToBitArray();
                    }
                }
            }

            searchResults.FailedRules = resultBits.Cast<bool>().Contains(true);
            return resultBits;
        }

        /// <summary>
        /// Aggregate voltage results.
        /// </summary>
        /// <param name="searchResults">List of results.</param>
        /// <returns>Final voltages.</returns>
        public static List<double> AggregateVoltages(this SearchResults searchResults)
        {
            List<double> voltages = null;
            var currentMultiPassCount = searchResults.SearchResultData.Last().MultiPassCount + 1;

            foreach (var searchResult in searchResults.SearchResultData.ToArray().Reverse())
            {
                if (voltages == null)
                {
                    voltages = new List<double>(searchResult.Voltages);
                }
                else if (searchResult.MultiPassCount < currentMultiPassCount)
                {
                    for (int targetIndex = 0; targetIndex < voltages.Count; targetIndex++)
                    {
                        if (!searchResult.Voltages[targetIndex].Equals(VminUtilities.VoltageMaskValue, 3) &&
                            !voltages[targetIndex].Equals(VminUtilities.VoltageFailValue, 3))
                        {
                            UpdateSingleVoltage(searchResult.Voltages, targetIndex, voltages);
                        }
                    }
                }
                else
                {
                    for (int targetIndex = 0; targetIndex < voltages.Count; targetIndex++)
                    {
                        if (!searchResult.Voltages[targetIndex].Equals(VminUtilities.VoltageMaskValue)
                            && voltages[targetIndex].Equals(VminUtilities.VoltageMaskValue, 3))
                        {
                            UpdateSingleVoltage(searchResult.Voltages, targetIndex, voltages);
                        }
                    }
                }

                currentMultiPassCount = searchResult.MultiPassCount;
            }

            return voltages == null || voltages.Count <= 0
                ? Enumerable.Repeat(VoltageMaskValue, searchResults.SearchResultData.First().StartVoltages.Count).ToList()
                : voltages;
        }

        /// <summary>
        /// Traverses search results and finds if search is passing.
        /// </summary>
        /// <param name="searchResults">Search results.</param>
        public static void UpdateSearchData(ref this SearchResults searchResults)
        {
            var lastSearchResultData = searchResults.SearchResultData.Last();
            var result = lastSearchResultData.IsPass;
            var currentMultiPassCount = lastSearchResultData.MultiPassCount + 1;

            foreach (var searchResult in searchResults.SearchResultData.ToArray().Reverse())
            {
                if (searchResult.MultiPassCount >= currentMultiPassCount)
                {
                    continue;
                }

                result &= searchResult.IsPass || searchResult.ExecutionCount == 0;
                currentMultiPassCount = searchResult.MultiPassCount;
            }

            searchResults.FailedSearch = !result;
        }

        /// <summary>
        /// Finds if you failed search due to an amble fail.
        /// </summary>
        /// <param name="searchResults">Search results.</param>
        /// <param name="plistObject">Plist interface.</param>
        /// <returns>true when amble failed.</returns>
        public static bool FailedAmble(IEnumerable<SearchResultData> searchResults, IPlistObject plistObject)
        {
            var currentMultiPassCount = searchResults.Last().MultiPassCount + 1;

            foreach (var searchResult in searchResults.Reverse())
            {
                if (searchResult.MultiPassCount >= currentMultiPassCount)
                {
                    continue;
                }

                currentMultiPassCount = searchResult.MultiPassCount;
                if (!searchResult.IsPass && searchResult.VoltageLimitingPatterns.Any(plistObject.IsPatternAnAmble))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This function mimics the Evergreen TraceCTVs UserFunction.
        /// It takes an ICaptureCtvPerCycleTest and writes each cycle captured.
        /// It's meant to be used as a way to trace the execution/interleaving of a ConcurrentPList.
        /// </summary>
        /// <param name="captureCtvPerCycleTest">ICaptureCtvPerCycleTest containing the CTVs.</param>
        /// <returns>true.</returns>
        public static bool TraceCTVs(ICaptureCtvPerCycleTest captureCtvPerCycleTest)
        {
            Prime.Services.ConsoleService.PrintDebug($"CTV TRACE BEGIN ============================================================================");
            string lastPoint = string.Empty;
            foreach (var cycleData in captureCtvPerCycleTest.GetCtvPerCycle())
            {
                var domain = cycleData.GetDomainName();
                var pattern = cycleData.GetPatternName();
                var plist = cycleData.GetParentPlistName();
                var address = cycleData.GetVectorAddress();
                var cycle = cycleData.GetCycle();
                var traceRegister = cycleData.GetTraceLogRegister1();
                var traceCycle = cycleData.GetTraceLogCycle();
                var burstIndex = cycleData.GetBurstIndex();
                var burstCycle = cycleData.GetBurstCycle();

                var currentPoint = $"{plist} {pattern} {domain}";
                if (currentPoint != lastPoint)
                {
                    Prime.Services.ConsoleService.PrintDebug($"CTV Trace: Domain:[{domain}] Pattern:[{pattern}] Plist:[{plist}] Address:[{address}] Cycle:[{cycle}] TraceReg1:[{traceRegister}] TraceCycle:[{traceCycle}] BusrtIndex:[{burstIndex}] BurstCycle:[{burstCycle}]");
                }

                lastPoint = currentPoint;
            }

            Prime.Services.ConsoleService.PrintDebug($"CTV TRACE END ============================================================================");
            return true;
        }

        /// <summary>
        /// Bitwise OR all elements in a list.
        /// </summary>
        /// <param name="elements">Elements.</param>
        /// <returns>Result.</returns>
        public static BitArray Or(this List<BitArray> elements)
        {
            if (elements == null || elements.Count <= 0)
            {
                return null;
            }

            var result = new BitArray(elements.First());
            foreach (var t in elements)
            {
                result.Or(t);
            }

            return result;
        }

        private static void UpdateSingleVoltage(List<double> searchResult, int targetIndex, List<double> updatedResult)
        {
            if (!searchResult[targetIndex].Equals(VminUtilities.VoltageFailValue) && !(searchResult[targetIndex] > updatedResult[targetIndex]))
            {
                return;
            }

            updatedResult[targetIndex] = searchResult[targetIndex];
        }
    }
}
