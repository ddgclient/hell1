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
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;

    /// <summary>
    /// Defines the <see cref="MemDecode_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MemDecode_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemDecode_UnitTest"/> class.
        /// </summary>
        public MemDecode_UnitTest()
        {
            this.ItuffOutput = new List<string>();
            this.ErrorOutput = new List<string>();
            this.ConsoleOutput = new List<string>();

            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                this.ConsoleOutput.Add(s);
                Console.WriteLine(s);
            });
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) =>
                {
                    this.ErrorOutput.Add(msg);
                    Console.WriteLine(msg);
                });
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;

            this.StrgvalFormatMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            this.StrgvalFormatMock.Setup(o => o.SetData(It.IsAny<string>())).Callback((string s) =>
            {
                this.CurrentStrgvalData = s;
            });
            this.StrgvalFormatMock.Setup(o => o.SetTnamePostfix(It.IsAny<string>())).Callback((string s) =>
            {
                this.CurrentStrgvalName = s;
            });

            this.DatalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            this.DatalogServiceMock.Setup(o => o.WriteToItuff(It.IsAny<IItuffFormat>())).Callback((IItuffFormat s) =>
            {
                var txt = "0_tname_TESTNAME";
                if (!string.IsNullOrEmpty(this.CurrentStrgvalName))
                {
                    txt += "_" + this.CurrentStrgvalName;
                    this.CurrentStrgvalName = string.Empty;
                }

                txt += $"\n0_strgval_{this.CurrentStrgvalData}";
                this.CurrentStrgvalData = string.Empty;

                Console.WriteLine($"[ITUFF]{txt.Replace("\n", "\n[ITUFF]")}");
                this.ItuffOutput.Add(txt);
            });

            this.DatalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.StrgvalFormatMock.Object);
            Prime.Services.DatalogService = this.DatalogServiceMock.Object;

            Console.WriteLine("Done with constructor");
        }

        private Mock<IDatalogService> DatalogServiceMock { get; set; }

        private Mock<IStrgvalFormat> StrgvalFormatMock { get; set; }

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private List<string> ErrorOutput { get; set; }

        private List<string> ConsoleOutput { get; set; }

        private List<string> ItuffOutput { get; set; }

        private string CurrentStrgvalData { get; set; }

        private string CurrentStrgvalName { get; set; }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ExtractBits_List_Pass()
        {
            /*                       1111111111
                           01234567890123456789*/
            var testStr = "00010010001101000101";
            Assert.AreEqual("1111111", testStr.ExtractBits(new List<int> { 3, 6, 10, 11, 13, 17, 19 })); // Extract all the 1's
            Assert.AreEqual("0000000000000", testStr.ExtractBits(new List<int> { 0, 1, 2, 4, 5, 7, 8, 9, 12, 14, 15, 16, 18 })); // Extract all the 0's
            Assert.AreEqual("01", testStr.ExtractBits(new List<int> { 0, 19 })); // Extract first and last bits.
            Assert.AreEqual("1", testStr.ExtractBits(new List<int> { 0 }, offset: 3)); // Extract 3rd bit using offset.
        }

        /// <summary>
        /// Verify exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ExtractBits_List_OutOfRange()
        {
            "00010010001101000101".ExtractBits(new List<int> { 38 });
        }

        /// <summary>
        /// Test out ExtractBits function.
        /// </summary>
        [TestMethod]
        public void ExtractBits_Bits_Fail()
        {
            /*                       1111111111
                           01234567890123456789*/
            var testStr = "00010010001101000101";

            var ex1 = Assert.ThrowsException<FormatException>(() => testStr.ExtractBits("1,2,g,6"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("ExtractBits(bits:1,2,g,6, offset:0): Failed to convert item or range to int. Item=[g].", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual("Input string was not in a correct format.", ex1.Message);

            var ex2 = Assert.ThrowsException<IndexOutOfRangeException>(() => testStr.ExtractBits("1,2,60,5"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("ExtractBits(data length:20, bits:1,2,60,5, offset:0): Index out of range on Item=[60].", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual("Index was outside the bounds of the array.", ex2.Message);
        }

        /// <summary>
        /// Test out ExtractBits function.
        /// </summary>
        [TestMethod]
        public void ExtractBits_Bits_Pass()
        {
            /*                       1111111111
                           01234567890123456789*/
            var testStr = "00010010001101000101";
            Assert.AreEqual("0001", testStr.ExtractBits("0-3"));
            Assert.AreEqual("1000", testStr.ExtractBits("3-0"));
            Assert.AreEqual("0001101", testStr.ExtractBits("0-3,17,18,19"));
            Assert.AreEqual("0001101", testStr.ExtractBits("0-3,17-19"));
            Assert.AreEqual("00010100", testStr.ExtractBits("0-3,7-4"));
            Assert.AreEqual("0101", testStr.ExtractBits("0-3", offset: 16));
        }

        /// <summary>
        /// Test out ExtractBits function.
        /// </summary>
        [TestMethod]
        public void ExtractBits_Count_Fail()
        {
            /*                       1111111111
                           01234567890123456789*/
            var testStr = "00010010001101000101";

            var ex1 = Assert.ThrowsException<IndexOutOfRangeException>(() => testStr.ExtractBits(0, 60));
            this.ConsoleServiceMock.Verify(o => o.PrintError("ExtractBits(data length:20, bitStart:0, bitCount:60, countDown:False): Index out of range on bit number 20, Value=[20].", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual("Index was outside the bounds of the array.", ex1.Message);
        }

        /// <summary>
        /// Test out ExtractBits function.
        /// </summary>
        [TestMethod]
        public void ExtractBits_Count_Pass()
        {
            /*                       1111111111
                           01234567890123456789*/
            var testStr = "00010010001101000101";
            Assert.AreEqual("0001", testStr.ExtractBits(0, 4));
            Assert.AreEqual("0010", testStr.ExtractBits(4, 4));
            Assert.AreEqual("0100", testStr.ExtractBits(4, 4, countDown: true));
        }

        /// <summary>
        /// Test out BinaryToInteger function.
        /// </summary>
        [TestMethod]
        public void BinaryToInteger_Pass()
        {
            Assert.AreEqual(8, "1000".BinaryToInteger());
            Assert.AreEqual(1, "1000".BinaryToInteger(lsbFirst: true));
            Assert.AreEqual(-8, "1000".BinaryToInteger(twosComp: true));
            Assert.AreEqual(7, "0111".BinaryToInteger(twosComp: true));
        }

        /// <summary>
        /// Test out BinaryToInteger function.
        /// </summary>
        [TestMethod]
        public void BinaryToHex_Pass()
        {
            Assert.AreEqual("8", "1000".BinaryToHex());
            Assert.AreEqual("1", "1000".BinaryToHex(lsbFirst: true));
            Assert.AreEqual("F", "1111".BinaryToHex());
            Assert.AreEqual("7F", "1111111".BinaryToHex());
            Assert.AreEqual("FFFF0123456789ABCDEF", "11111111111111110000000100100011010001010110011110001001101010111100110111101111".BinaryToHex());
        }

        /// <summary>
        /// Test the passing case of WriteStrgvalToItuff.
        /// </summary>
        [TestMethod]
        public void WriteStrgvalToItuff_Pass()
        {
            // setup the mocks.
            var datalogWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            datalogWriterMock.Setup(o => o.SetTnamePostfix("SomePostFix"));
            datalogWriterMock.Setup(o => o.SetData("SomeData"));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(datalogWriterMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(datalogWriterMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            // run the testcase
            MemDecode.WriteStrgvalToItuff("SomePostFix", "SomeData");

            // verify the mocks.
            datalogWriterMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }
    }
}
