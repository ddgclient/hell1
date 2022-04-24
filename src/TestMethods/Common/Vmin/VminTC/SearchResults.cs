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
    using Prime.TestMethods.VminSearch;

    /// <summary>
    /// Aggregates and stores multiple SearchResults.
    /// </summary>
    public struct SearchResults
    {
        /// <summary>
        /// Gets or sets a value indicating whether any search iteration failed.
        /// </summary>
        public bool FailedSearch;

        /// <summary>
        /// Gets or sets a value indicating whether the rules failed.
        /// </summary>
        public bool FailedRules;

        /// <summary>
        /// Gets or sets a value indicating whether one or more search executions failed amble.
        /// </summary>
        public bool FailedAmble;

        /// <summary>
        /// Gets or sets incoming mask bits.
        /// </summary>
        public BitArray IncomingMask;

        /// <summary>
        /// Gets or sets the latest decoded test result.
        /// </summary>
        public BitArray DecodedResult;

        /// <summary>
        /// Gets or sets the rules results bits.
        /// </summary>
        public BitArray RulesResultsBits;

        /// <summary>
        /// Gets or sets the search test results bits.
        /// </summary>
        public List<BitArray> TestResultsBits;

        /// <summary>
        /// Gets or sets a reference to Prime search result data.
        /// </summary>
        public List<SearchResultData> SearchResultData;

        /// <summary>
        /// gets or sets a reference to MaxRepetitionCount parameter value.
        /// </summary>
        public uint MaxRepetitionCount;

        /// <summary>
        /// Gets or sets a reference to Prime execution flow states.
        /// </summary>
        public PrimeVminSearchTestMethod.SearchExecutionFlowStates SearchFlowStates;
    }
}
