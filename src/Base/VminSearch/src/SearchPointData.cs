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

    /// <summary>
    /// A class to accumulate multi-pass results.
    /// </summary>
    public class SearchPointData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchPointData"/> class.
        /// </summary>
        /// <param name="voltages">A list containing the search point voltages.</param>
        /// <param name="failPatternData">Structure containing the data of the fail pattern.</param>>
        public SearchPointData(List<double> voltages, PatternData failPatternData)
        {
            this.Voltages = voltages;
            this.FailPatternData = failPatternData;
        }

        /// <summary>
        /// Gets a list containing the resulting search voltages.
        /// </summary>
        public List<double> Voltages { get; }

        /// <summary>
        /// Gets the fail pattern data.
        /// </summary>
        public PatternData FailPatternData { get; }

        /// <summary>
        /// Struct containing pattern name, burst index and pattern Id to identify the fail pattern in each search.
        /// </summary>
        public readonly struct PatternData
        {
            /// <summary>
            /// String containing the pattern name.
            /// </summary>
            public readonly string PatternName;

            /// <summary>
            /// Unsigned number containing the burst index.
            /// </summary>
            public readonly uint BurstIndex;

            /// <summary>
            /// Unsigned number containing the the pattern ID.
            /// </summary>
            public readonly uint PatternId;

            /// <summary>
            /// Unsigned number containing the the fail vector address.
            /// </summary>
            public readonly ulong FailVector;

            /// <summary>
            /// Initializes a new instance of the <see cref="PatternData"/> struct.
            /// </summary>
            /// <param name="patternName">Pattern name.</param>
            /// <param name="burstIndex">Burst index.</param>
            /// <param name="patternId">Pattern Id.</param>
            /// <param name="vector">Fail vector address.</param>
            public PatternData(string patternName, uint burstIndex, uint patternId, ulong vector = 0)
            {
                this.PatternName = patternName;
                this.BurstIndex = burstIndex;
                this.PatternId = patternId;
                this.FailVector = vector;
            }
        }
    }
}
