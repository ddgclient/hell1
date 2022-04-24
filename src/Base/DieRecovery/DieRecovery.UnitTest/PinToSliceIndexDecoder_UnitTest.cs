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

    /// <summary>
    /// Defines the <see cref="PinToSliceIndexDecoder_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PinToSliceIndexDecoder_UnitTest
    {
        /// <summary>
        /// Test decoder GetFailTrackerFromPlistResults fail paths.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_Fail()
        {
            var decoderJson = "{'Name':'CORE0_NOA', 'PatternModify':'CORE_DISABLE0', 'Size':1, 'PinToSliceIndexMap':{'NOAB_00':[0],'NOAB_08':[0]}}";
            var decoder = (PinToSliceIndexDecoder)JsonConvert.DeserializeObject(decoderJson, typeof(PinToSliceIndexDecoder));

            Assert.ThrowsException<ArgumentException>(() => decoder.GetFailTrackerFromPlistResults(null));
            Assert.ThrowsException<ArgumentException>(() => decoder.GetFailTrackerFromPlistResults(new Mock<INoCaptureTest>(MockBehavior.Strict).Object));

            var funcTest = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcTest.Setup(o => o.GetFailingPinNames()).Throws(new Prime.Base.Exceptions.FatalException("exception"));
            Assert.AreEqual("0", decoder.GetFailTrackerFromPlistResults(funcTest.Object).ToBinaryString());
        }

        /// <summary>
        /// Test decoder GetFailTrackerFromPlistResults passing paths.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_Pass()
        {
            this.TestGetFailTrackerFromPlistResults("{'Name':'CORE0_NOA', 'PatternModify':'CORE_DISABLE0', 'Size':1, 'PinToSliceIndexMap':{'NOAB_00':[0],'NOAB_08':[0]}}", "NOAB_00", "1");
            this.TestGetFailTrackerFromPlistResults("{'Name':'CORE0_NOA', 'PatternModify':'CORE_DISABLE0', 'Size':1, 'PinToSliceIndexMap':{'NOAB_00':[0],'NOAB_08':[0]}}", "NOAB_08", "1");
            this.TestGetFailTrackerFromPlistResults("{'Name':'CORE0_NOA', 'PatternModify':'CORE_DISABLE0', 'Size':1, 'PinToSliceIndexMap':{'NOAB_00':[0],'NOAB_08':[0]}}", "NOAB_00,NOAB_08", "1");
            this.TestGetFailTrackerFromPlistResults("{'Name':'CORE0_NOA', 'PatternModify':'CORE_DISABLE0', 'Size':1, 'PinToSliceIndexMap':{'NOAB_00':[0],'NOAB_08':[0]}}", "differentpin", "0");
        }

        /// <summary>
        /// Test decoder GetFailTrackerFromPlistResults passing paths.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_Fail()
        {
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleMock.Setup(o => o.PrintError("MaskPlistFromTracker[CORE0_NOA] - mask contains [12] bits but decoder expects [1].", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleMock.Object;

            var decoder = (PinToSliceIndexDecoder)JsonConvert.DeserializeObject("{'Name':'CORE0_NOA', 'PatternModify':'CORE_DISABLE0', 'Size':1, 'PinToSliceIndexMap':{'NOAB_00':[0],'NOAB_08':[0]}}", typeof(PinToSliceIndexDecoder));
            var funcTest = new Mock<IFunctionalTest>(MockBehavior.Strict).Object;
            Assert.ThrowsException<ArgumentException>(() => decoder.MaskPlistFromTracker("111111111111".ToBitArray(), ref funcTest));
            consoleMock.VerifyAll();
        }

        /// <summary>
        /// Test decoder GetFailTrackerFromPlistResults passing paths.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_Pass()
        {
            var decoder = (PinToSliceIndexDecoder)JsonConvert.DeserializeObject("{'Name':'CORE0_NOA', 'PatternModify':'CORE_DISABLE0', 'Size':1, 'PinToSliceIndexMap':{'NOAB_00':[0],'NOAB_08':[0]}}", typeof(PinToSliceIndexDecoder));
            var funcTest = new Mock<IFunctionalTest>(MockBehavior.Strict).Object;
            CollectionAssert.AreEqual(new List<string>(), decoder.MaskPlistFromTracker("0".ToBitArray(), ref funcTest));
            CollectionAssert.AreEqual(new List<string> { "NOAB_00", "NOAB_08" }, decoder.MaskPlistFromTracker("1".ToBitArray(), ref funcTest));
        }

        /// <summary>
        /// Refer to test method name.
        /// </summary>
        [TestMethod]
        public void GetDecoderType_Pass()
        {
            var decoder = (PinToSliceIndexDecoder)JsonConvert.DeserializeObject("{'Name':'CORE0_NOA', 'PatternModify':'CORE_DISABLE0', 'Size':1, 'PinToSliceIndexMap':{'NOAB_00':[0],'NOAB_08':[0]}}", typeof(PinToSliceIndexDecoder));
            var result = decoder.GetDecoderType();
            Assert.AreEqual("PinToSliceIndexDecoder", result);
        }

        private void TestGetFailTrackerFromPlistResults(string jsonDecoder, string pinList, string result, int? currentSlice = null)
        {
            var decoder = (PinToSliceIndexDecoder)JsonConvert.DeserializeObject(jsonDecoder, typeof(PinToSliceIndexDecoder));
            var funcTest = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcTest.Setup(o => o.GetFailingPinNames()).Returns(pinList.Split(',').ToList());
            Assert.AreEqual(result, decoder.GetFailTrackerFromPlistResults(funcTest.Object, currentSlice).ToBinaryString());
        }
    }
}