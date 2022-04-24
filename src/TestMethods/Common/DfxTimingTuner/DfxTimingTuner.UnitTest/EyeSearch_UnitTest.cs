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

namespace DfxTimingTuner.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;

    /// <summary>
    /// Defines the <see cref="EyeSearch_UnitTest" />.
    /// </summary>
    [TestClass]
    public class EyeSearch_UnitTest
    {
        /// <summary>
        /// Initialize method to setup all common mocks.
        /// </summary>
        [TestInitialize]
        public void TestSetup()
        {
            // Ignore any print messages.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(p => p.PrintDebug(It.IsAny<string>())).Callback((string msg) => Console.WriteLine(msg));
            Prime.Services.ConsoleService = consoleServiceMock.Object;
        }

        /// <summary>
        /// Test the EyeSearch find passing region.
        /// </summary>
        [TestMethod]
        public void FindLargestPassingRegion_Pass()
        {
            this.SetupAndTestFindLargestPassingRegion("000111000".ToBitArray(), 3, 3);
            this.SetupAndTestFindLargestPassingRegion("000111100".ToBitArray(), 3, 4);
            this.SetupAndTestFindLargestPassingRegion("000".ToBitArray(), -1, -1);
            this.SetupAndTestFindLargestPassingRegion("111111100".ToBitArray(), 0, 7);
            this.SetupAndTestFindLargestPassingRegion("1110000".ToBitArray(), 0, 3);
            this.SetupAndTestFindLargestPassingRegion("0111".ToBitArray(), 1, 3);
            this.SetupAndTestFindLargestPassingRegion("1111".ToBitArray(), 0, 4);
            this.SetupAndTestFindLargestPassingRegion("101111100".ToBitArray(), 2, 5);
        }

        /// <summary>
        /// Test the EyeSearch Decode CTV logic for Drive/Input mode.
        /// </summary>
        [TestMethod]
        public void DecodeCTV_DriveMode_Pass()
        {
            var p = EyeSearch.CtvCharForPass;
            var f = EyeSearch.CtvCharForFail;

            var expectedResultPin1 = new BitArray(new bool[] { !EyeSearch.BitArrayBoolForPass, EyeSearch.BitArrayBoolForPass, !EyeSearch.BitArrayBoolForPass });
            var expectedResultPin2 = new BitArray(new bool[] { !EyeSearch.BitArrayBoolForPass, !EyeSearch.BitArrayBoolForPass, EyeSearch.BitArrayBoolForPass });
            var pinOrder = new List<string> { "Pin1", "Pin2" };
            var ctvData = $"{f}{f}{f}{p}" + $"{p}{p}{p}{f}" + $"{f}{p}{p}{p}";

            var rslt = EyeSearch.DriveModeSingleCtvToPerPinTestResults(ctvData, 3, pinOrder);
            Assert.AreEqual(2, rslt.Keys.Count, "Wrong number of Pins in result.");
            Assert.IsTrue(rslt.ContainsKey("Pin1"), "Result does not include 'Pin1'.");
            Assert.IsTrue(rslt.ContainsKey("Pin2"), "Result does not include 'Pin2'.");
            Assert.AreEqual(expectedResultPin1.ToBinaryString(), rslt["Pin1"].ToBinaryString(), "Results for 'Pin1' do not match.");
            Assert.AreEqual(expectedResultPin2.ToBinaryString(), rslt["Pin2"].ToBinaryString(), "Results for 'Pin2' do not match.");
        }

        /// <summary>
        /// Test the EyeSearch Decode CTV logic for Drive/Input mode.
        /// </summary>
        [TestMethod]
        public void DecodeCTV_DriveMode_Fail()
        {
            var ex1 = Assert.ThrowsException<TestMethodException>(() => EyeSearch.DriveModeSingleCtvToPerPinTestResults("0000", 3, new List<string> { "Pin1", "Pin2" }));
            Assert.IsTrue(ex1.Message.Equals("CTVLength=[4] is not a multiple of NumberOfTestpoints=[3]."));

            var ex2 = Assert.ThrowsException<TestMethodException>(() => EyeSearch.DriveModeSingleCtvToPerPinTestResults("000", 3, new List<string> { "Pin1", "Pin2" }));
            Assert.IsTrue(ex2.Message.Equals("BitsPerTestpoint=[1] (calculated from CTVLength=[3] / NumberOfTestpoints=[3]) is not a multiple of DecodePins=[2]."));
        }

        /// <summary>
        /// Test the EyeSearch Decode CTV logic for Strobe/Compare mode.
        /// </summary>
        [TestMethod]
        public void DecodeCTV_CompareMode_Pass()
        {
            var p = EyeSearch.CtvCharForPass;
            var f = EyeSearch.CtvCharForFail;

            var expectedResultPin1 = new BitArray(new bool[] { !EyeSearch.BitArrayBoolForPass, EyeSearch.BitArrayBoolForPass, !EyeSearch.BitArrayBoolForPass });
            var expectedResultPin2 = new BitArray(new bool[] { !EyeSearch.BitArrayBoolForPass, !EyeSearch.BitArrayBoolForPass, EyeSearch.BitArrayBoolForPass });
            var ctvData = new Dictionary<string, string>
            {
                { "Pin1", $"{f}{f}" + $"{p}{p}" + $"{f}{p}" },
                { "Pin2", $"{f}{p}" + $"{p}{f}" + $"{p}{p}" },
            };

            var rslt = EyeSearch.CompareModeMultiCtvToPerPinTestResults(ctvData, 3);
            Assert.AreEqual(2, rslt.Keys.Count, "Wrong number of Pins in result.");
            Assert.IsTrue(rslt.ContainsKey("Pin1"), "Result does not include 'Pin1'.");
            Assert.IsTrue(rslt.ContainsKey("Pin2"), "Result does not include 'Pin2'.");
            Assert.AreEqual(expectedResultPin1.ToBinaryString(), rslt["Pin1"].ToBinaryString(), "Results for 'Pin1' do not match.");
            Assert.AreEqual(expectedResultPin2.ToBinaryString(), rslt["Pin2"].ToBinaryString(), "Results for 'Pin2' do not match.");
        }

        /// <summary>
        /// Test the EyeSearch Decode CTV logic for Strobe/Compare mode.
        /// </summary>
        [TestMethod]
        public void DecodeCTV_CompareMode_Fail()
        {
            var p = EyeSearch.CtvCharForPass;
            var f = EyeSearch.CtvCharForFail;

            var ctvData = new Dictionary<string, string>
            {
                { "Pin1", $"{f}{f}" + $"{p}{p}" },
                { "Pin2", $"{f}{p}" + $"{p}{f}" },
            };

            var ex1 = Assert.ThrowsException<TestMethodException>(() => EyeSearch.CompareModeMultiCtvToPerPinTestResults(ctvData, 3));
            Assert.IsTrue(ex1.Message.Equals("CTVLength=[4] is not a multiple of NumberOfTestpoints=[3]."));
        }

        private void SetupAndTestFindLargestPassingRegion(BitArray testResults, int expectedStart, int expectedWidth)
        {
            System.Console.WriteLine($"Running SetupAndTestFindLargestPassingRegion({testResults.ToBinaryString()}, {expectedStart}, {expectedWidth})");
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            Prime.Services.ConsoleService = consoleMock.Object;

            var retval = EyeSearch.FindLargestPassingRegion(testResults, out var start, out var width);
            Assert.AreEqual(expectedWidth > 0, retval, "Failed for pass/fail result.");
            Assert.AreEqual(expectedStart, start, "Failed for the starting index.");
            Assert.AreEqual(expectedWidth, width, "Failed for the width.");
        }
    }
}
