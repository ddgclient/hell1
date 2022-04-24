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
    using Prime;
    using Prime.ConsoleService;

    /// <summary>
    /// No recovery mode fails when results are not matching incoming mask.
    /// </summary>
    public class NoRecoveryMode : RecoveryDefaultMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoRecoveryMode"/> class.
        /// </summary>
        /// <param name="console">Console service used to print debug messages. Either Prime.Services.ConsoleService or null.</param>
        public NoRecoveryMode(IConsoleService console)
            : base(console)
        {
        }

        /// <inheritdoc />
        public override int GetPort(SearchResults searchResults)
        {
            if (searchResults.FailedSearch)
            {
                return VminTC.FailPort;
            }

            var xor = new BitArray(searchResults.RulesResultsBits);
            xor = xor.Or(searchResults.IncomingMask);
            xor = xor.Xor(searchResults.IncomingMask);
            var notMatching = xor.Cast<bool>().Contains(true);

            if (notMatching || searchResults.FailedRules)
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.DeclaringType} {MethodBase.GetCurrentMethod()?.Name} --Test Failed. IncomingMask=[{searchResults.IncomingMask.ToBinaryString()}, RulesResults=[{searchResults.RulesResultsBits.ToBinaryString()}]");
                return VminTC.FailRulesPort;
            }

            return VminTC.PassPort;
        }

        /// <inheritdoc />
        public override bool UpdateRecoveryTrackers(SearchResults searchResults, IDieRecovery tracker, bool forceUpdate = false)
        {
            var testResults = searchResults.TestResultsBits.Or();
            if (!forceUpdate)
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
    }
}