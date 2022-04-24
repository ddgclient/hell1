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
    using System.Collections;
    using System.Linq;
    using System.Reflection;
    using DDG;
    using Prime.ConsoleService;

    /// <summary>
    /// Default mode for Recovery.
    /// </summary>
    public class RecoveryFailRetestMode : RecoveryDefaultMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryFailRetestMode"/> class.
        /// </summary>
        /// <param name="console">Console service used to print debug messages. Either Prime.Services.ConsoleService or null.</param>
        public RecoveryFailRetestMode(IConsoleService console)
            : base(console)
        {
        }

        /// <inheritdoc />
        public override bool HasToRepeatSearch(ref SearchResults searchResults, IPinMap pinMap, IDieRecovery tracker, string recoveryOptions, bool bitsDecodeFromVoltages = true)
        {
            pinMap?.Restore();
            searchResults.UpdateSearchData();
            searchResults.TestResultsBits.Add(this.GetResultBits(searchResults, pinMap, bitsDecodeFromVoltages));
            searchResults.RulesResultsBits = searchResults.RunRules(recoveryOptions, tracker);

            var lastSearchResultData = searchResults.SearchResultData.Last();
            if (!searchResults.FailedSearch && searchResults.FailedRules && lastSearchResultData.RepetitionCount < searchResults.MaxRepetitionCount)
            {
                var resultBits = searchResults.TestResultsBits.Or();
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.DeclaringType} {MethodBase.GetCurrentMethod()?.Name} --Repeating search. M{lastSearchResultData.MultiPassCount}R{lastSearchResultData.RepetitionCount}. ResultBits=[{resultBits.ToBinaryString()}] RulesBits=[{searchResults.RulesResultsBits.ToBinaryString()}]");
                searchResults.FailedRules = false;
                searchResults.FailedSearch = false;
                searchResults.TestResultsBits.RemoveAt(searchResults.TestResultsBits.Count - 1);
                return true;
            }

            if (searchResults.FailedSearch && !searchResults.FailedRules && lastSearchResultData.RepetitionCount < searchResults.MaxRepetitionCount)
            {
                var resultBits = searchResults.TestResultsBits.Or();
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.DeclaringType} {MethodBase.GetCurrentMethod()?.Name} --Repeating search. M{lastSearchResultData.MultiPassCount}R{lastSearchResultData.RepetitionCount}. ResultBits=[{resultBits.ToBinaryString()}] RulesBits=[{searchResults.RulesResultsBits.ToBinaryString()}]");
                searchResults.FailedRules = false;
                searchResults.FailedSearch = false;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override BitArray GetMaskBits(SearchResults searchResults, bool useRulesBits = true)
        {
            var result = new BitArray(useRulesBits ? searchResults.RulesResultsBits : searchResults.TestResultsBits.Or());
            if (searchResults.SearchFlowStates.RepetitionCount > 1)
            {
                result = result.Not();
            }

            return result.Or(searchResults.IncomingMask);
        }
    }
}