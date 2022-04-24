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
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class BitArrayExtensions_UnitTest
    {
        /// <summary>
        /// Setup generic mocks.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) =>
                {
                    Console.WriteLine($"ERROR: {msg}");
                });
            Prime.Services.ConsoleService = consoleServiceMock.Object;
        }

        /// <summary>
        /// Test the string extension method ToBitArray.
        /// </summary>
        [TestMethod]
        public void StringToBitArray_ArgNullException_Fail()
        {
            string x = null;
            Assert.ThrowsException<ArgumentNullException>(() => x.ToBitArray());
        }

        /// <summary>
        /// Test the string extension method ToBitArray.
        /// </summary>
        [TestMethod]
        public void StringToBitArray_Pass()
        {
            CollectionAssert.AreEqual(new BitArray(new bool[0] { }), string.Empty.ToBitArray());
            CollectionAssert.AreEqual(new BitArray(new bool[1] { true }), "1".ToBitArray());
            CollectionAssert.AreEqual(new BitArray(new bool[1] { false }), "0".ToBitArray());
            CollectionAssert.AreEqual(new BitArray(new bool[2] { false, false }), "c2".ToBitArray()); // this is an odd one, but any non-binary is false.
            CollectionAssert.AreEqual(new BitArray(new bool[2] { true, false }), "10".ToBitArray());
            CollectionAssert.AreEqual(new BitArray(new bool[2] { false, true }), "01".ToBitArray());
            CollectionAssert.AreEqual(new BitArray(new bool[10] { false, true, true, false, false, true, true, true, false, false }), "0110011100".ToBitArray());
        }

        /// <summary>
        /// Test the BitArray extension method ToBinaryString.
        /// </summary>
        [TestMethod]
        public void BitArrayToString_ArgNullException_Fail()
        {
            BitArray x = null;
            Assert.ThrowsException<ArgumentNullException>(() => x.ToBinaryString());
        }

        /// <summary>
        /// Test the BitArray extension method ToBinaryString.
        /// </summary>
        [TestMethod]
        public void BitArrayToString_Pass()
        {
            Assert.AreEqual(string.Empty, new BitArray(new bool[0] { }).ToBinaryString());
            Assert.AreEqual("1", new BitArray(new bool[1] { true }).ToBinaryString());
            Assert.AreEqual("0", new BitArray(new bool[1] { false }).ToBinaryString());
            Assert.AreEqual("010110111", new BitArray(new bool[9] { false, true, false, true, true, false, true, true, true }).ToBinaryString());
        }

        /// <summary>
        /// Test the BitArray extension method Slice.
        /// </summary>
        [TestMethod]
        public void BitArraySlice_Exception_Fail()
        {
            BitArray x = null;
            Assert.ThrowsException<ArgumentNullException>(() => x.Slice(0, 0));

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new BitArray(new bool[1] { true }).Slice(0, 2));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new BitArray(new bool[1] { true }).Slice(2, 1));
        }

        /// <summary>
        /// Test the BitArray extension method Slice.
        /// </summary>
        [TestMethod]
        public void BitArraySlice_Pass()
        {
            CollectionAssert.AreEqual(new BitArray(new bool[] { }), new BitArray(new bool[2] { true, false }).Slice(0, 0));

            CollectionAssert.AreEqual(new BitArray(new bool[1] { true }), new BitArray(new bool[2] { true, false }).Slice(0, 1));
            CollectionAssert.AreEqual(new BitArray(new bool[1] { false }), new BitArray(new bool[2] { true, false }).Slice(1, 1));

            CollectionAssert.AreEqual(new BitArray(new bool[3] { true, true, true }), new BitArray(new bool[7] { false, false, true, true, true, false, false }).Slice(2, 3));

            CollectionAssert.AreEqual(new BitArray(new bool[] { true, false }), new BitArray(new bool[2] { true, false }).Slice(0, 2));
        }

        /// <summary>
        /// Test the BitArray extension method Slice.
        /// </summary>
        [TestMethod]
        public void BitArrayAdd_Exception_Fail()
        {
            BitArray x1 = null;
            BitArray x2 = null;
            Assert.ThrowsException<ArgumentNullException>(() => x1.Add(x2));
        }

        /// <summary>
        /// Test the BitArray extension method Slice.
        /// </summary>
        [TestMethod]
        public void BitArrayAdd_Pass()
        {
            BitArray nullBitArray = null;

            Assert.AreEqual("010", "010".ToBitArray().Add(nullBitArray).ToBinaryString());
            Assert.AreEqual("010", nullBitArray.Add("010".ToBitArray()).ToBinaryString());

            Assert.AreEqual("010", "010".ToBitArray().Add(string.Empty.ToBitArray()).ToBinaryString());
            Assert.AreEqual("010", string.Empty.ToBitArray().Add("010".ToBitArray()).ToBinaryString());

            Assert.AreEqual("0101111", "010".ToBitArray().Add("1111".ToBitArray()).ToBinaryString());
        }
    }
}
