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
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Misc tests for checking features I can potentially used. Not directly linked to features being tested in the TC.
    /// </summary>
    [TestClass]
    public class FailInfo_UnitTest
    {
        /// <summary> Make sure we can serialize a fail info into a string, then deserialize it back into its original state.  </summary>
        [TestMethod]
        public void SerializeThenDeserialize()
        {
            var failInfo = new FailInfo("ArrayA", "PatternA");
            failInfo.MBDAddress = new Tuple<int, int, int>(3, 2, 5);

            failInfo.HryIdentifier = "C0";
            failInfo.SliceId = "7";
            failInfo.Module = "3";
            string serializedFail = failInfo.ConvertToString();
            FailInfo newFailInfo = FailInfo.ConvertToObject(serializedFail);

            bool pinListEqual = true;

            Assert.IsTrue(
                failInfo.ArrayName == newFailInfo.ArrayName &&
                failInfo.HryIdentifier == newFailInfo.HryIdentifier &&
                failInfo.Module == newFailInfo.Module &&
                failInfo.Pattern == newFailInfo.Pattern &&
                failInfo.SliceId == newFailInfo.SliceId &&
                pinListEqual);
        }
    }
}