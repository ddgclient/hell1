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
    using System.Linq;
    using DDG;
    using DieRecoveryBase;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="AnyFailSingleSliceDecoder_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AnyFailSingleSliceDecoder_UnitTest
    {
        /// <summary>
        /// Test decoder GetFailTrackerFromPlistResults fail paths.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_Fail()
        {
            var decoderJson = "{'Name':'SLICE5_SCAN', 'PatternModify':'SLICE_DISABLE5', 'Size':1}";
            var decoder = (AnyFailSingleSliceDecoder)JsonConvert.DeserializeObject(decoderJson, typeof(AnyFailSingleSliceDecoder));

            Assert.ThrowsException<ArgumentException>(() => decoder.GetFailTrackerFromPlistResults(null));
            Assert.ThrowsException<ArgumentException>(() => decoder.GetFailTrackerFromPlistResults(new Mock<INoCaptureTest>(MockBehavior.Strict).Object));

            var funcTest = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcTest.Setup(o => o.GetFailingPinNames()).Throws(new Prime.Base.Exceptions.FatalException("exception"));
            Assert.AreEqual("0", decoder.GetFailTrackerFromPlistResults(funcTest.Object).ToBinaryString());

            var decoderJsonMultBit = "{'Name':'SLICE5_SCAN', 'PatternModify':'SLICE_DISABLE5', 'Size':10}";
            var decoderMultiBit = (AnyFailSingleSliceDecoder)JsonConvert.DeserializeObject(decoderJsonMultBit, typeof(AnyFailSingleSliceDecoder));
            funcTest.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "somepin" });
            Assert.ThrowsException<ArgumentException>(() => decoderMultiBit.GetFailTrackerFromPlistResults(funcTest.Object));

            Assert.ThrowsException<ArgumentException>(() => decoderMultiBit.GetFailTrackerFromPlistResults(funcTest.Object, currentSlice: 15));
        }

        /// <summary>
        /// Test decoder GetFailTrackerFromPlistResults passing paths.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_Pass()
        {
            this.TestGetFailTrackerFromPlistResults("{'Name':'SLICE5_SCAN', 'PatternModify':'SLICE_DISABLE5', 'Size':1}", "somepin", "1");
            this.TestGetFailTrackerFromPlistResults("{'Name':'SLICE5_SCAN', 'PatternModify':'SLICE_DISABLE5', 'Size':1, 'PinList':['somepin']}", "somepin", "1");
            this.TestGetFailTrackerFromPlistResults("{'Name':'SLICE5_SCAN', 'PatternModify':'SLICE_DISABLE5', 'Size':1, 'PinList':['somepin']}", "differentpin", "0");
        }

        /// <summary>
        /// Test decoder GetFailTrackerFromPlistResults passing paths.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_Pass()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("LogLevel")).Returns("PRIME_DEBUG");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleMock.Setup(o => o.PrintDebug("MaskPlistFromTracker - Decoder=[SLICE5_SCAN] Type=[DieRecoveryBase.AnyFailSingleSliceDecoder] does not support masking pins."));
            Prime.Services.ConsoleService = consoleMock.Object;

            var decoder = (AnyFailSingleSliceDecoder)JsonConvert.DeserializeObject("{'Name':'SLICE5_SCAN', 'PatternModify':'SLICE_DISABLE5', 'Size':1}", typeof(AnyFailSingleSliceDecoder));
            var funcTest = new Mock<IFunctionalTest>(MockBehavior.Strict).Object;
            CollectionAssert.AreEqual(new List<string>(), decoder.MaskPlistFromTracker("000".ToBitArray(), ref funcTest));
            consoleMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// refer to test method name.
        /// </summary>
        [TestMethod]
        public void GetDecoderType_Pass()
        {
            var decoder = (AnyFailSingleSliceDecoder)JsonConvert.DeserializeObject("{'Name':'SLICE5_SCAN', 'PatternModify':'SLICE_DISABLE5', 'Size':1}", typeof(AnyFailSingleSliceDecoder));
            var result = decoder.GetDecoderType();
            Assert.AreEqual("AnyFailSingleSliceDecoder", result);
        }

        private void TestGetFailTrackerFromPlistResults(string jsonDecoder, string pinList, string result, int? currentSlice = null)
        {
            var decoder = (AnyFailSingleSliceDecoder)JsonConvert.DeserializeObject(jsonDecoder, typeof(AnyFailSingleSliceDecoder));
            var funcTest = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcTest.Setup(o => o.GetFailingPinNames()).Returns(pinList.Split(',').ToList());
            Assert.AreEqual(result, decoder.GetFailTrackerFromPlistResults(funcTest.Object, currentSlice).ToBinaryString());
        }
    }
}
