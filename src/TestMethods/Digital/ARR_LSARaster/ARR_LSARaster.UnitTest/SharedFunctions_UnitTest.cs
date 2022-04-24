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
    using System.Text.RegularExpressions;
    using global::LSARasterTC;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;

    /// <summary> Dummy description of this test method's unit test.</summary>
    [TestClass]
    public class SharedFunctions_UnitTest
    {
        private Mock<IConsoleService> mockConsole = new Mock<IConsoleService>();

        /// <summary> Dummy description of this test method's unit test.</summary>
        [TestInitialize]
        public void Init()
        {
            this.mockConsole.Setup(x => x.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => { Console.WriteLine($"ERROR: {msg}"); });

            Prime.Services.ConsoleService = this.mockConsole.Object;
        }

        /// <summary> Ensure we can find a failing label if given one. </summary>
        [TestMethod]
        public void HexToBinaryTest()
        {
            string binary1 = "0101";
            string hex1 = DDG.RadixConversion.BinaryToHex("0101");
            string binary2 = DDG.RadixConversion.HexToBinary(hex1);

            System.Console.WriteLine($"Binary [{binary1}]");
            System.Console.WriteLine($"Converted to Hex [{hex1}]");
            System.Console.WriteLine($"Converted back [{binary2}]");
        }

        /// <summary> Ensure we can find a failing label if given one + fail if label does not exist. </summary>
        [TestMethod]
        public void CheckLabelContains_PassAndFail()
        {
            string label1 = "EPBIST_ENGINE_STATUS_FAIL_MBD_0_0_1";
            string label2 = "EPBIST_ENGINE_STATUS_BUSY_2_0";

            Assert.IsTrue(SharedFunctions.CheckLabelContains(label1, new Regex("FAIL")) && !SharedFunctions.CheckLabelContains(label2, new Regex("FAIL")));
        }

        /// <summary> See if we can find a label containing a sliceId. </summary>
        [TestMethod]
        public void CheckLabelContains_FindSubstringInSingleLabel()
        {
            string sliceLabelIdentifier = "SMBD";
            string sliceLabel = "EPBIST_ENGINE_STATUS_FAIL_SMBD_X_7_0_0_1";
            Assert.IsTrue(SharedFunctions.CheckLabelContains(sliceLabel, new Regex(sliceLabelIdentifier)));
        }

        /// <summary>
        /// Given a list of faildata, find the failing label in one of them.
        /// </summary>
        public void CheckLabelContains_FindSubstringInFailDataList()
        {
            Assert.IsTrue(false);
        }

        /// <summary>
        /// When all labels contain the same substring, this will return true.
        /// </summary>
        public void CheckAllLabelsContain_AllLabelsContainSubstring()
        {
            Assert.IsTrue(false);
        }

        /// <summary>
        /// Not all labels contain the same substring; will return false.
        /// </summary>
        public void CheckAllLabelsContain_NotAllLablesContainSubstring()
        {
            Assert.IsTrue(false);
        }

        /// <summary>
        /// Return all failing indexes from a failIO.
        /// </summary>
        public void ExtractOnesIndexesFromFailIo()
        {
            Assert.IsTrue(false);
        }

        /// <summary> See if we can find a label containing a sliceId. </summary>
        [TestMethod]
        public void RetrieveCoreNumber() // This test doesn't really test for anything, was used for figuring out how regexes works
        {
            string coreLabelRegex = "CORE([0-9]+)";
            string coreLabel = "CORE4_EPBIST_ENGINE_STATUS_FAIL_SMBD_X_7_0_0_1";
            var coreNumRegex = new Regex(coreLabelRegex);

            var matches = coreNumRegex.Match(coreLabel);
            Assert.IsTrue(matches.Groups[1].ToString() == "4");
        }

        /// <summary>
        /// Check that a null property is found while not throwing an exception.
        /// </summary>
        [TestMethod]
        public void IsPropertyNull()
        {
            LSARasterTC fakeTest = new LSARasterTC();

            Assert.IsTrue(SharedFunctions.IsPropertyNull(fakeTest.MetadataConfigPath, nameof(fakeTest.MetadataConfigPath)));
        }

        /// <summary>
        /// Check if we can catch a null propety exception and return true.
        /// </summary>
        [TestMethod]
        public void IsPropertyNull_CatchNullProperty()
        {
            LSARasterTC fakeTest = new LSARasterTC();
            Assert.IsTrue(SharedFunctions.IsPropertyNull(fakeTest.MetadataConfigSchemaPath, nameof(fakeTest.MetadataConfigSchemaPath)));
        }
    }
}
