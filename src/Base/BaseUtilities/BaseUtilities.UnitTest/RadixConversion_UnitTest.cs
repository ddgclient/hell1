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
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;

    /// <summary>
    /// Defines the <see cref="RadixConversion_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RadixConversion_UnitTest
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
        /// Test out BinaryToHex function.
        /// </summary>
        [TestMethod]
        public void String_BinaryToHex_Exception_Fail()
        {
            Assert.ThrowsException<ArgumentNullException>(() => string.Empty.BinaryToHex());
            string x = null;
            Assert.ThrowsException<ArgumentNullException>(() => x.BinaryToHex());
        }

        /// <summary>
        /// Test out BinaryToHex function.
        /// </summary>
        [TestMethod]
        public void String_BinaryToHex_Pass()
        {
            Assert.AreEqual("8", "1000".BinaryToHex());
            Assert.AreEqual("1", "1000".BinaryToHex(lsbFirst: true));
            Assert.AreEqual("1", "0001".BinaryToHex());
            Assert.AreEqual("1", "001".BinaryToHex());
            Assert.AreEqual("1", "1".BinaryToHex());
            Assert.AreEqual("F", "1111".BinaryToHex());
            Assert.AreEqual("7F", "1111111".BinaryToHex());
            Assert.AreEqual("00", "0000000".BinaryToHex());
            Assert.AreEqual("FFFF0123456789ABCDEF", "11111111111111110000000100100011010001010110011110001001101010111100110111101111".BinaryToHex());
        }

        /// <summary>
        /// Test out BinaryToInteger function.
        /// </summary>
        [TestMethod]
        public void String_BinaryToInteger_Pass()
        {
            Assert.AreEqual(8, "1000".BinaryToInteger());
            Assert.AreEqual(1, "1000".BinaryToInteger(lsbFirst: true));
            Assert.AreEqual(-8, "1000".BinaryToInteger(twosComp: true));
            Assert.AreEqual(7, "0111".BinaryToInteger(twosComp: true));
            Assert.AreEqual(1, "0001".BinaryToInteger());
            Assert.AreEqual(0, "0000".BinaryToInteger());
            Assert.AreEqual(255, "11111111".BinaryToInteger());
        }

        /// <summary>
        /// Test out TwosComplementToInteger function.
        /// </summary>
        [TestMethod]
        public void String_TwosComplementToInteger_Pass()
        {
            Assert.AreEqual(-8, "1000".TwosComplementToInteger());
            Assert.AreEqual(7, "0111".TwosComplementToInteger());
        }

        /// <summary>
        /// Test out HexToBinary function.
        /// </summary>
        [TestMethod]
        public void Char_HexToBinary_Exception_Fail()
        {
            Assert.ThrowsException<ArgumentException>(() => '\0'.HexToBinary());
            Assert.ThrowsException<ArgumentException>(() => '-'.HexToBinary());
            Assert.ThrowsException<FormatException>(() => 'z'.HexToBinary());
        }

        /// <summary>
        /// Test out HexToBinary function.
        /// </summary>
        [TestMethod]
        public void String_HexToBinary_Exception_Fail()
        {
            Assert.ThrowsException<ArgumentNullException>(() => string.Empty.HexToBinary());
            string x = null;
            Assert.ThrowsException<ArgumentNullException>(() => x.HexToBinary());
        }

        /// <summary>
        /// Test out HexToBinary function.
        /// </summary>
        [TestMethod]
        public void String_HexToBinary_Pass()
        {
            Assert.AreEqual("0000", "0".HexToBinary());
            Assert.AreEqual("0001", "1".HexToBinary());
            Assert.AreEqual("10100101", "A5".HexToBinary());
            Assert.AreEqual("1010101111001101111011110000000100100011010001010110011110001001", "abcdef0123456789".HexToBinary());
        }

        /// <summary>
        /// Test out IntegerToBinary function.
        /// </summary>
        [TestMethod]
        public void Integer_IntegerToBinary_Pass()
        {
            Assert.AreEqual("0", 0.IntegerToBinary());
            Assert.AreEqual("1", 1.IntegerToBinary());
            Assert.AreEqual("0001", 1.IntegerToBinary(size: 4));
            Assert.AreEqual("10101", 21.IntegerToBinary());

            int negInt = -37;
            Assert.AreEqual("11011011", negInt.IntegerToBinary(size: 8));
        }

        /// <summary>
        /// Test out IntegerToBinary function.
        /// </summary>
        [TestMethod]
        public void String_IntegerToBinary_Exception_Fail()
        {
            Assert.ThrowsException<FormatException>(() => "abcd".IntegerToBinary());
            Assert.ThrowsException<ArgumentNullException>(() => string.Empty.IntegerToBinary());
        }

        /// <summary>
        /// Test out IntegerToBinary function.
        /// </summary>
        [TestMethod]
        public void String_IntegerToBinary_Pass()
        {
            Assert.AreEqual("0", "0".IntegerToBinary());
            Assert.AreEqual("1", "1".IntegerToBinary());
            Assert.AreEqual("0001", "1".IntegerToBinary(size: 4));
            Assert.AreEqual("10101", "21".IntegerToBinary());
            Assert.AreEqual("11011011", "-37".IntegerToBinary(size: 8));
        }
    }
}
