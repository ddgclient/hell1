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

namespace LSARasterTC.UnitTest
{
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Misc tests for checking features I can potentially used. Not directly linked to features being tested in the TC.
    /// </summary>
    [TestClass]
    public class MiscTests_UnitTest
    {
        /// <summary> Testing a method of getting core num from label. </summary>
        [TestMethod]
        public void RetrieveCoreNumber()
        {
            string coreLabelRegex = "CORE([0-9]+)";
            string coreLabel = "CORE4_EPBIST_ENGINE_STATUS_FAIL_SMBD_X_7_0_0_1";
            var coreNumRegex = new Regex(coreLabelRegex);

            var matches = coreNumRegex.Match(coreLabel);
            Assert.IsTrue(matches.Groups[1].ToString() == "4");
        }

        /// <summary> Testing a method of getting core num from label. </summary>
        [TestMethod]
        public void FailRetrieveCoreNum()
        {
            string coreLabelRegex = "CORE([0-9]+)";
            string coreLabel = "COREB_EPBIST_ENGINE_STATUS_FAIL_SMBD_X_7_0_0_1";
            var coreNumRegex = new Regex(coreLabelRegex);

            var match = coreNumRegex.Match(coreLabel);
            Assert.IsTrue(!match.Success);
        }
    }
}
