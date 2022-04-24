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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.SharedStorageService;
    using Prime.UserVarService;

    /// <summary>
    /// Defines the <see cref="StringExtensions_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StringExtensions_UnitTest
    {
        /// <summary>
        /// Test the string extension method Reverse.
        /// </summary>
        [TestMethod]
        public void StringReverse_ArgNullException_Fail()
        {
            string x = null;
            Assert.ThrowsException<ArgumentNullException>(() => x.Reverse());
        }

        /// <summary>
        /// Test the string extension method Reverse.
        /// </summary>
        [TestMethod]
        public void StringReverse_Pass()
        {
            Assert.AreEqual(string.Empty, string.Empty.Reverse());

            Assert.AreEqual("1", "1".Reverse());

            Assert.AreEqual("321GFEdcba", "abcdEFG123".Reverse());
        }

        /// <summary>
        /// Test the string extension method ResizeBinary.
        /// </summary>
        [TestMethod]
        public void ResizeBinary_ArgNullException_Fail()
        {
            string x = null;
            Assert.ThrowsException<ArgumentNullException>(() => x.ResizeBinary(0));
            Assert.ThrowsException<ArgumentNullException>(() => string.Empty.ResizeBinary(37));
        }

        /// <summary>
        /// Test the string extension method ResizeBinary.
        /// </summary>
        [TestMethod]
        public void ResizeBinary_Pass()
        {
            Assert.AreEqual("1", "1".ResizeBinary(0));
            Assert.AreEqual("1", "1".ResizeBinary(1));
            Assert.AreEqual("11", "11".ResizeBinary(2));
            Assert.AreEqual("111", "111111111".ResizeBinary(3));
            Assert.AreEqual("0111111111", "111111111".ResizeBinary(10));
        }

        /// <summary>
        /// Test the string extension method ToDouble.
        /// </summary>
        [TestMethod]
        public void ToDouble_RegExFail()
        {
            Assert.ThrowsException<ArgumentException>(() => "notadouble".ToDouble());
        }

        /// <summary>
        /// Test the string extension method ToDouble.
        /// </summary>
        [TestMethod]
        public void ToDouble_SharedStorage()
        {
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("Token", Context.DUT)).Returns(true);
            sharedStorageMock.Setup(s => s.GetDoubleRowFromTable("Token", Context.DUT)).Returns(0.1);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;
            Assert.AreEqual(0.1, "Token".ToDouble(true));
        }

        /// <summary>
        /// Test the string extension method ToDouble.
        /// </summary>
        [TestMethod]
        public void ToDouble_UserVar()
        {
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("Collection.Token", Context.DUT)).Returns(false);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("Collection.Token")).Returns(true);
            userVarMock.Setup(o => o.GetDoubleValue("Collection.Token")).Returns(0.1);
            Prime.Services.UserVarService = userVarMock.Object;

            Assert.AreEqual(0.1, "Collection.Token".ToDouble(true));
        }

        /// <summary>
        /// Test the string extension method ToDouble.
        /// </summary>
        [TestMethod]
        public void ToDouble_Pass()
        {
            Assert.AreEqual(0.01, "10mV".ToDouble());
            Assert.AreEqual(1e-9, "1e-9".ToDouble());
            Assert.AreEqual(-7, "-7".ToDouble());
            Assert.AreEqual(-7, "-7A".ToDouble());
            Assert.AreEqual(20.3e12, "20.3GHz".ToDouble());
            Assert.AreEqual(-8000, "-8kW".ToDouble());

            // check that they still work with shared storage enabled.
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("10mV", Context.DUT)).Returns(false);
            sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("1e-9", Context.DUT)).Returns(false);
            sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("-7", Context.DUT)).Returns(false);
            sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("-7A", Context.DUT)).Returns(false);
            sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("20.3GHz", Context.DUT)).Returns(false);
            sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("-8kW", Context.DUT)).Returns(false);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarMock.Object;

            Assert.AreEqual(0.01, "10mV".ToDouble(true));
            Assert.AreEqual(1e-9, "1e-9".ToDouble(true));
            Assert.AreEqual(-7, "-7".ToDouble(true));
            Assert.AreEqual(-7, "-7A".ToDouble(true));
            Assert.AreEqual(20.3e12, "20.3GHz".ToDouble(true));
            Assert.AreEqual(-8000, "-8kW".ToDouble(true));

            // check that getting the value from shared storage works.
            sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("GSDS1", Context.DUT)).Returns(true);
            sharedStorageMock.Setup(o => o.GetDoubleRowFromTable("GSDS1", Context.DUT)).Returns(1.4);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            Assert.AreEqual(1.4, "GSDS1".ToDouble(true));
            Assert.AreEqual(1.4, "GSDS1 ".ToDouble(true));
            Assert.AreEqual(1.4, " GSDS1 ".ToDouble(true));
            Assert.AreEqual(1.4, " GSDS1".ToDouble(true));
        }

        /// <summary>
        /// Test the string extension method ToInt.
        /// </summary>
        [TestMethod]
        public void ToInt_Fail()
        {
            Assert.ThrowsException<FormatException>(() => "notanint".ToInt());
            Assert.ThrowsException<FormatException>(() => "5.5".ToInt());
        }

        /// <summary>
        /// Test the string extension method ToInt.
        /// </summary>
        [TestMethod]
        public void ToInt_Pass()
        {
            Assert.AreEqual(1, "1".ToInt());
            Assert.AreEqual(100, "100".ToInt());
            Assert.AreEqual(-2, "-2".ToInt());
        }

        /// <summary>
        /// Test the string extension method ToInt.
        /// </summary>
        [TestMethod]
        public void RangeToList_Pass()
        {
            CollectionAssert.AreEqual(new List<int>(), string.Empty.RangeToList());
            CollectionAssert.AreEqual(new List<int>() { 1, 2, 3, 5, 7, 6, 5, 4, 3, 2 }, "1-3,5,7-2".RangeToList());
        }

        /// <summary>
        /// Test the string extension method ToDouble.
        /// </summary>
        [TestMethod]
        public void EvaluateExpression_UsingSharedStorage()
        {
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("Token", Context.DUT)).Returns(true);
            sharedStorageMock.Setup(s => s.GetDoubleRowFromTable("Token", Context.DUT)).Returns(0.1);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;
            Assert.AreEqual(0.35, "[Token]+0.5/2".EvaluateExpression());
        }

        /// <summary>
        /// Test the string extension method ToDouble.
        /// </summary>
        [TestMethod]
        public void EvaluateExpression_Simple()
        {
            Assert.AreEqual(2.25, "2+0.5/2".EvaluateExpression());
        }
    }
}
