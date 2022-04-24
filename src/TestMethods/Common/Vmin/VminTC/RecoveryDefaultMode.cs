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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using DDG;
    using Prime;
    using Prime.ConsoleService;

    /// <summary>
    /// Default mode for Recovery.
    /// </summary>
    public class RecoveryDefaultMode : IRecoveryMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryDefaultMode"/> class.
        /// </summary>
        /// <param name="console">Console service used to print debug messages. Either Prime.Services.ConsoleService or null.</param>
        public RecoveryDefaultMode(IConsoleService console)
        {
            this.Console = console;
        }

        /// <summary>
        /// Gets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; }

        /// <inheritdoc />
        public virtual int GetPort(SearchResults searchResults)
        {
            switch (searchResults.FailedSearch)
            {
                case true when !searchResults.FailedRules:
                    return VminTC.FailRecoveryPort;
                case true when searchResults.FailedRules:
                    return VminTC.FailPort;
                case false when !searchResults.FailedRules:
                    return VminTC.PassPort;
                case false when searchResults.FailedRules:
                    return VminTC.FailRulesPort;
            }

            return VminTC.FailPort;
        }

        /// <inheritdoc />
        public virtual bool HasToRepeatSearch(ref SearchResults searchResults, IPinMap pinMap, IDieRecovery tracker, string recoveryOptions, bool bitsDecodeFromVoltages = true)
        {
            pinMap?.Restore();
            searchResults.UpdateSearchData();
            searchResults.TestResultsBits.Add(this.GetResultBits(searchResults, pinMap, bitsDecodeFromVoltages));
            searchResults.RulesResultsBits = searchResults.RunRules(recoveryOptions, tracker);
            return false;
        }

        /// <inheritdoc />
        public virtual bool UpdateRecoveryTrackers(SearchResults searchResults, IDieRecovery tracker, bool forceUpdate = false)
        {
            var testResults = searchResults.TestResultsBits.Or();
            if (!forceUpdate && (searchResults.FailedRules || searchResults.FailedSearch))
            {
                tracker?.LogTrackingStructure(searchResults.IncomingMask, testResults);
                return true;
            }

            this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.DeclaringType} {MethodBase.GetCurrentMethod()?.Name} --Updating Tracking Structure Bits:{searchResults.RulesResultsBits.ToBinaryString()}");
            if (!tracker.UpdateTrackingStructure(searchResults.RulesResultsBits, searchResults.IncomingMask, testResults))
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.DeclaringType} {MethodBase.GetCurrentMethod()?.Name} --Unable to update DieRecovery trackers.");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public virtual BitArray GetMaskBits(SearchResults searchResults, bool useRulesBits = true)
        {
            if (useRulesBits)
            {
                var result = new BitArray(searchResults.RulesResultsBits);
                return result.Or(searchResults.IncomingMask);
            }

            if (searchResults.TestResultsBits.Count == 0)
            {
                return new BitArray(searchResults.IncomingMask);
            }

            var testResults = searchResults.TestResultsBits.Or();
            return testResults.Or(searchResults.IncomingMask);
        }

        /// <summary>
        /// Gets result bits from voltages or decoded result.
        /// </summary>
        /// <param name="searchResults">Current search results.</param>
        /// <param name="pinMap">DieRecovery pin map interface.</param>
        /// <param name="bitsDecodeFromVoltages">Use voltages to decode bits.</param>
        /// <returns>Result bits.</returns>
        protected virtual BitArray GetResultBits(SearchResults searchResults, IPinMap pinMap, bool bitsDecodeFromVoltages = true)
        {
            if (!bitsDecodeFromVoltages)
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Name} --Result Bits:{searchResults.DecodedResult.ToBinaryString()}");
                return searchResults.DecodedResult;
            }

            var voltages = searchResults.AggregateVoltages();
            var trackingBits = new BitArray(voltages.Count, false);
            for (var i = 0; i < voltages.Count; i++)
            {
                if (voltages[i].Equals(VminUtilities.VoltageFailValue, 3))
                {
                    trackingBits.Set(i, true);
                }
            }

            if (voltages.Count != searchResults.IncomingMask.Count)
            {
                var rightLength = new BitArray(pinMap.VoltageDomainsToFailTracker(trackingBits));
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Name} --Result Bits:{rightLength.ToBinaryString()}");
                return rightLength;
            }

            this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Name} --Result Bits:{trackingBits.ToBinaryString()}");
            return trackingBits;
        }
    }
}