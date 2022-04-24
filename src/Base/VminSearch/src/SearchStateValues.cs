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
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Class to store the identifiers for each search.
    /// </summary>
    public class SearchStateValues
    {
        /// <summary>
        /// Gets or sets voltages to be applied for each target at current search point.
        /// </summary>
        public List<double> Voltages { get; set; }

        /// <summary>
        /// Gets or sets an array containing the mask for each target at current search point.
        /// </summary>
        public BitArray MaskBits { get; set; }

        /// <summary>
        /// Gets or sets a list of the start voltages per target.
        /// </summary>
        public List<double> StartVoltages { get; set; }

        /// <summary>
        /// Gets or sets a list of the end voltage limit per target.
        /// </summary>
        public List<double> EndVoltageLimits { get; set; }

        /// <summary>
        /// Gets or sets the execution count.
        /// </summary>
        public uint ExecutionCount { get; set; }

        /// <summary>
        /// Gets or sets a list of the iterations per target.
        /// </summary>
        public List<uint> PerTargetIncrements { get; set; }

        /// <summary>
        /// Gets or sets a list of the accumulated per point results.
        /// </summary>
        public List<SearchPointData> PerPointData { get; set; } = new List<SearchPointData>();

        /// <summary>
        /// Gets or sets a failing reason (if any) for logging purposes.
        /// </summary>
        public string FailReason { get; set; } = string.Empty;
    }
}
