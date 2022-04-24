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

namespace DDG.UnitTest
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines the <see cref="DoubleExtensions_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DoubleExtensions_UnitTest
    {
        /// <summary>
        /// Test the double extension method IsEqual.
        /// </summary>
        [TestMethod]
        public void DoubleIsEqual_Pass()
        {
            double d = 9.123456789;

            Assert.IsTrue(d.Equals(9.123456789, decimalPlaces: 0));
            Assert.IsTrue(d.Equals(9.123456789, decimalPlaces: 1));
            Assert.IsTrue(d.Equals(9.14, decimalPlaces: 1));
            Assert.IsTrue(d.Equals(9.123499999, decimalPlaces: 4));

            Assert.IsFalse(d.Equals(0, decimalPlaces: 0));
            Assert.IsFalse(d.Equals(9.12345678, decimalPlaces: 0));
            Assert.IsFalse(d.Equals(9.14, decimalPlaces: 2));
            Assert.IsFalse(d.Equals(9.123499999, decimalPlaces: 5));
        }
    }
}
