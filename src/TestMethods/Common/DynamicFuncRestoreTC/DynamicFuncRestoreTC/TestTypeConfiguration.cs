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

namespace DynamicFuncRestoreTC
{
    using System.Collections.Generic;

    /// <summary>
    /// This class represents the data structure of the Test Type configurations in json input file.
    /// </summary>
    internal class TestTypeConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestTypeConfiguration"/> class.
        /// </summary>
        /// <param name="plistNames">plistNames.</param>
        /// <param name="bypassUserVar">bypassUserVar.</param>
        /// <param name="testTypeSubStringStart">testTypeSubStringStart.</param>
        /// <param name="testTypeSubStringLength">testTypeSubStringLength.</param>
        /// <param name="testTypeToMatch">testTypeToMatch.</param>
        public TestTypeConfiguration(
            List<string> plistNames,
            string bypassUserVar,
            int testTypeSubStringStart,
            int testTypeSubStringLength,
            string testTypeToMatch)
        {
            this.PlistNames = plistNames;
            this.BypassUserVar = bypassUserVar;
            this.TestTypeSubStringStart = testTypeSubStringStart;
            this.TestTypeSubStringLength = testTypeSubStringLength;
            this.TestTypeToMatch = testTypeToMatch;
        }

        /// <summary>
        /// Gets or sets modify target plist names.
        /// </summary>
        public List<string> PlistNames { get; set; }

        /// <summary>
        /// Gets or sets userVars used for test instance bypassing.
        /// </summary>
        public string BypassUserVar { get; set; }

        /// <summary>
        /// Gets or sets testTypeSubStringStart.
        /// </summary>
        public int TestTypeSubStringStart { get; set; }

        /// <summary>
        /// Gets or sets TestTypeSubStringLength.
        /// </summary>
        public int TestTypeSubStringLength { get; set; }

        /// <summary>
        /// Gets or sets TestTypeToMatch.
        /// </summary>
        public string TestTypeToMatch { get; set; }

        /// <summary>
        /// Gets or sets Plist object.
        /// </summary>
        public PlistHandler Plist { get; set; }
    }
}
