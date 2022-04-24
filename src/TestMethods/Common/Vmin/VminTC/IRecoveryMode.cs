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
    using DDG;

    /// <summary>
    /// Implements interface for different Recovery modes.
    /// </summary>
    public interface IRecoveryMode
    {
        /// <summary>
        /// Evaluates results and determines final port.
        /// </summary>
        /// <param name="searchResults">Accumulated test results.</param>
        /// <returns>Final port number.</returns>
        int GetPort(SearchResults searchResults);

        /// <summary>
        /// Determines if search has to be repeated.
        /// </summary>
        /// <param name="searchResults">Accumulated test results.</param>
        /// <param name="pinMap">PinMap interface.</param>
        /// <param name="tracker">DieRecovery tracker interface.</param>
        /// <param name="recoveryOptions">List of recovery options.</param>
        /// <param name="bitsDecodeFromVoltages">Decode result bits from voltages.</param>
        /// <returns>True if search is being repeated.</returns>
        bool HasToRepeatSearch(ref SearchResults searchResults, IPinMap pinMap, IDieRecovery tracker, string recoveryOptions, bool bitsDecodeFromVoltages = true);

        /// <summary>
        /// Update trackers based on results and modes.
        /// </summary>
        /// <param name="searchResults">Accumulated test results.</param>
        /// <param name="tracker">DieRecovery outgoing tracker.</param>
        /// <param name="forceUpdate">Force UpdateAlways.</param>
        /// <returns>False when failed to update tracker.</returns>
        bool UpdateRecoveryTrackers(SearchResults searchResults, IDieRecovery tracker, bool forceUpdate = false);

        /// <summary>
        /// Gets the mask bits based on current results.
        /// </summary>
        /// <param name="searchResults">Accumulated test results.</param>
        /// <param name="useRulesBits">Use DieRecovery Rules bits for evaluation.</param>
        /// <returns>mask bits.</returns>
        BitArray GetMaskBits(SearchResults searchResults,  bool useRulesBits = true);
    }
}