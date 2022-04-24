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
    /// <summary>
    /// Class to store the identifiers for each search.
    /// </summary>
    public class SearchIdentifiers
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchIdentifiers"/> class.
        /// </summary>
        /// <param name="tnamePostfix">String to be used as tname postfix to identify the current search.</param>
        /// <param name="multiPassCount">Current multi pass count.</param>
        /// <param name="repetitionCount">Current repetition count.</param>
        public SearchIdentifiers(string tnamePostfix, uint multiPassCount, uint repetitionCount)
        {
            this.TnamePostfix = tnamePostfix;
            this.MultiPassCount = multiPassCount;
            this.RepetitionCount = repetitionCount;
        }

        /// <summary>
        /// Gets a string of the form MxRy. Where x is the multi pass execution count and y is the repetition count.
        /// </summary>
        public string TnamePostfix { get; }

        /// <summary>
        /// Gets an unsigned integer identifying the number of multi pass execution.
        /// </summary>
        public uint MultiPassCount { get; }

        /// <summary>
        /// Gets an unsigned integer identifying the number of the repetition count.
        /// </summary>
        public uint RepetitionCount { get; }
    }
}
