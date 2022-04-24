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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DDG;
    using DieRecoveryBase;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PerformanceService;
    using Prime.PlistService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="PinMap_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PinMap_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinMap_UnitTest"/> class.
        /// </summary>
        public PinMap_UnitTest()
        {
            this.SharedStorageValues = new Dictionary<string, string>();

            // Default Mock for console service.
            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => Console.WriteLine(s));
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;

            // Default Mock for Shared service.
            this.SharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>()))
                .Callback((string key, object obj, Context context) =>
                {
                    this.SharedStorageValues[context + "|" + key] = JsonConvert.SerializeObject(obj);
                });
            this.SharedStorageMock.Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>()))
                .Returns((string key, Type obj, Context context) => JsonConvert.DeserializeObject(this.SharedStorageValues[context + "|" + key], obj));
            this.SharedStorageMock.Setup(o => o.KeyExistsInObjectTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Returns((string key, Context context) => this.SharedStorageValues.ContainsKey(context + "|" + key));

            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, string obj, Context context) =>
                {
                    this.SharedStorageValues[context + "|" + key] = obj;
                });
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Returns((string key, Context context) => this.SharedStorageValues[context + "|" + key]);
            this.SharedStorageMock.Setup(o => o.KeyExistsInStringTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Returns((string key, Context context) => this.SharedStorageValues.ContainsKey(context + "|" + key));

            // update the reset policies
            this.SharedStorageMock.Setup(o => o.OverrideStringRowResetPolicy(It.IsAny<string>(), ResetPolicy.NEVER_RESET, Context.DUT));
            this.SharedStorageMock.Setup(o => o.OverrideObjectRowResetPolicy(It.IsAny<string>(), ResetPolicy.NEVER_RESET, Context.DUT));

            Prime.Services.SharedStorageService = this.SharedStorageMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("LogLevel")).Returns("PRIME_DEBUG");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private Dictionary<string, string> SharedStorageValues { get; set; }

        private Mock<ISharedStorageService> SharedStorageMock { get; set; }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void PinMapDecoderBase_TestUnimplemented_Fail()
        {
            var pinMapDecoder = JsonConvert.DeserializeObject<PinMapDecoderBase>(@"{'Name':'TestPinMap', 'Size':4, 'PatternModify':'dummy'}");
            var funcTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);
            var funcTestAsRef = funcTestMock.Object;
            Assert.ThrowsException<System.NotImplementedException>(() => pinMapDecoder.GetFailTrackerFromPlistResults(funcTestMock.Object, null));
            Assert.ThrowsException<System.NotImplementedException>(() => pinMapDecoder.MaskPlistFromTracker("000".ToBitArray(), ref funcTestAsRef));
            Assert.ThrowsException<System.NotImplementedException>(() => pinMapDecoder.GetDecoderType());
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void PinMapConstructor_Fail()
        {
            Assert.ThrowsException<TestMethodException>(() => new PinMapBase("NotARealPinMap"));
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void DecodeFailure_FailAmbleCheck_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));

            // Mock a failure.
            var fakeFailure = new Mock<IFailureData>(MockBehavior.Strict);
            fakeFailure.Setup(o => o.GetDomainName()).Returns("FakeDomain");
            fakeFailure.Setup(o => o.GetParentPlistName()).Returns("FakePList");
            fakeFailure.Setup(o => o.GetPatternName()).Returns("FakePattern");

            // Mock the plist object to return 2 failing pins.
            var captureFailureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureFailureTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string>() { "Pin2", "Pin3" });
            captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Throws(new Prime.Base.Exceptions.FatalException("Failed to get Failures from Prime"));
            var plist = captureFailureTestMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("FakePattern")).Returns(false);

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("FakePList")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            // Create the PinMapBase object and test the decodefailure method
            PinMapBase pinMap = new PinMapBase("TestPinMap");
            CollectionAssert.AreEqual(new BitArray(new bool[4] { false, true, true, false }), pinMap.DecodeFailure(plist));
            captureFailureTestMock.VerifyAll();
            this.ConsoleServiceMock.Verify(o => o.PrintDebug($"Prime failed on Amble check for first failure. Error=Failed to get Failures from Prime"), Times.Once);
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void DecodeFailure_1to1Map_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));

            // Mock a failure.
            var fakeFailure = new Mock<IFailureData>(MockBehavior.Strict);
            fakeFailure.Setup(o => o.GetDomainName()).Returns("FakeDomain");
            fakeFailure.Setup(o => o.GetParentPlistName()).Returns("FakePList");
            fakeFailure.Setup(o => o.GetPatternName()).Returns("FakePattern");

            // Mock the plist object to return 2 failing pins.
            var captureFailureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureFailureTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string>() { "Pin2", "Pin3" });
            captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fakeFailure.Object });
            var plist = captureFailureTestMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("FakePattern")).Returns(false);

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("FakePList")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            // Create the PinMapBase object and test the decodefailure method
            PinMapBase pinMap = new PinMapBase("TestPinMap");
            CollectionAssert.AreEqual(new BitArray(new bool[4] { false, true, true, false }), pinMap.DecodeFailure(plist));
            captureFailureTestMock.VerifyAll();
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void DecodeFailure_ExtraPin_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));

            // Mock a failure.
            var fakeFailure = new Mock<IFailureData>(MockBehavior.Strict);
            fakeFailure.Setup(o => o.GetDomainName()).Returns("FakeDomain");
            fakeFailure.Setup(o => o.GetParentPlistName()).Returns("FakePList");
            fakeFailure.Setup(o => o.GetPatternName()).Returns("FakePattern");

            // Mock the plist object to return 2 failing pins.
            var captureFailureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureFailureTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string>() { "TDO" });
            captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fakeFailure.Object });
            var plist = captureFailureTestMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("FakePattern")).Returns(false);

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("FakePList")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            // Create the PinMapBase object and test the decodefailure method
            PinMapBase pinMap = new PinMapBase("TestPinMap");

            CollectionAssert.AreEqual(new BitArray(new bool[4] { true, true, true, true }), pinMap.DecodeFailure(plist));
            captureFailureTestMock.VerifyAll();
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void DecodeFailure_Preamble_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));

            // Mock a failure.
            var fakeFailure = new Mock<IFailureData>(MockBehavior.Strict);
            fakeFailure.Setup(o => o.GetDomainName()).Returns("FakeDomain");
            fakeFailure.Setup(o => o.GetParentPlistName()).Returns("FakePList");
            fakeFailure.Setup(o => o.GetPatternName()).Returns("FakeAmblePattern");

            // Mock the plist object to return 2 failing pins.
            var captureFailureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureFailureTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string>() { "Pin1" });
            captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fakeFailure.Object });
            var plist = captureFailureTestMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("FakeAmblePattern")).Returns(true);

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("FakePList")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            // Create the PinMapBase object and test the decodefailure method
            PinMapBase pinMap = new PinMapBase("TestPinMap");

            CollectionAssert.AreEqual(new BitArray(new bool[4] { true, true, true, true }), pinMap.DecodeFailure(plist));
            captureFailureTestMock.VerifyAll();
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void DecodeFailure_Manyto1Map_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2a':[1], 'Pin2b':[1], 'Pin2c':[1], 'Pin3':[2], 'Pin4':[3]}}"));

            // Mock a failure.
            var fakeFailure = new Mock<IFailureData>(MockBehavior.Strict);
            fakeFailure.Setup(o => o.GetDomainName()).Returns("FakeDomain");
            fakeFailure.Setup(o => o.GetParentPlistName()).Returns("FakePList");
            fakeFailure.Setup(o => o.GetPatternName()).Returns("FakePattern");

            // Mock the plist object to return 2 failing pins.
            var captureFailureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureFailureTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string>() { "Pin2a", "Pin4", "Pin2c" });
            captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fakeFailure.Object });
            var plist = captureFailureTestMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("FakePattern")).Returns(false);

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("FakePList")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            // Create the PinMapBase object and test the decodefailure method
            PinMapBase pinMap = new PinMapBase("TestPinMap");

            CollectionAssert.AreEqual(new BitArray(new bool[4] { false, true, false, true }), pinMap.DecodeFailure(plist));
            captureFailureTestMock.VerifyAll();
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void DecodeFailure_1toManyMap_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1,2,3]}}"));

            // Mock a failure.
            var fakeFailure = new Mock<IFailureData>(MockBehavior.Strict);
            fakeFailure.Setup(o => o.GetDomainName()).Returns("FakeDomain");
            fakeFailure.Setup(o => o.GetParentPlistName()).Returns("FakePList");
            fakeFailure.Setup(o => o.GetPatternName()).Returns("FakePattern");

            // Mock the plist object to return 2 failing pins.
            var captureFailureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureFailureTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string>() { "Pin2" });
            captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fakeFailure.Object });
            var plist = captureFailureTestMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("FakePattern")).Returns(false);

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("FakePList")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            // Create the PinMapBase object and test the decode failure method
            PinMapBase pinMap = new PinMapBase("TestPinMap");

            CollectionAssert.AreEqual(new BitArray(new bool[4] { false, true, true, true }), pinMap.DecodeFailure(plist));
            captureFailureTestMock.VerifyAll();
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void DecodeFailure_MultiPinMap_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap1', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap2', 'Size':3, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin2':[0], 'Pin6':[1], 'Pin7':[2]}}"));

            // Mock a failure.
            var fakeFailure = new Mock<IFailureData>(MockBehavior.Strict);
            fakeFailure.Setup(o => o.GetDomainName()).Returns("FakeDomain");
            fakeFailure.Setup(o => o.GetParentPlistName()).Returns("FakePList");
            fakeFailure.Setup(o => o.GetPatternName()).Returns("FakePattern");

            // Mock the plist object to return 3 failing pins.
            var captureFailureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureFailureTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string>() { "Pin2", "Pin3", "Pin7" });
            captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fakeFailure.Object });
            var plist = captureFailureTestMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("FakePattern")).Returns(false);

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("FakePList")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            // Create the PinMapBase object and test the decodefailure method
            PinMapBase pinMap = new PinMapBase("TestPinMap1,TestPinMap2");

            CollectionAssert.AreEqual(new BitArray(new bool[7] { false, true, true, false, true, false, true }), pinMap.DecodeFailure(plist));
            captureFailureTestMock.VerifyAll();
        }

        /// <summary>
        /// Test that sending the wrong size bitarray fails.
        /// </summary>
        [TestMethod]
        public void MaskPins_WrongSize_Fail()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));

            // Create the fake functional test.
            var functionalTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);
            var plist = functionalTestMock.Object;

            // Create the PinMapBase object and test the mask pins method
            var pinMap = DDG.PinMap.Service.Get("TestPinMap");

            // Assert.IsFalse(pinMap.MaskPins(new BitArray(new bool[2] { false, true }), ref plist));
            Assert.ThrowsException<TestMethodException>(() => pinMap.MaskPins(new BitArray(new bool[2] { false, true }), ref plist, null));

            // Assert.IsFalse(pinMap.MaskPins(new BitArray(new bool[6] { false, true, false, true, false, true }), ref plist));
            Assert.ThrowsException<TestMethodException>(() => pinMap.MaskPins(new BitArray(new bool[6] { false, true, false, true, false, true }), ref plist, null));

            functionalTestMock.VerifyAll();
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void MaskPins_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));

            // Mock the plist object for pinmasking.
            List<string> maskPins = new List<string> { "Pin4" };
            var functionalTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);
            functionalTestMock.Setup(f => f.SetPinMask(maskPins));
            var plist = functionalTestMock.Object;

            // Create the PinMapBase object and test the mask pins method
            // PinMapBase pinMap = new PinMapBase("TestPinMap");
            var factory = DDG.PinMap.Service;
            var pinMap = factory.Get("TestPinMap");

            pinMap.MaskPins(new BitArray(new bool[4] { false, false, false, true }), ref plist, null);

            functionalTestMock.VerifyAll();
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void MaskPins_Doa_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}, 'DoaPins':['Pin5']}"));

            // Mock the plist object for pinmasking.
            List<string> maskPins = new List<string> { "Pin4", "Pin5" };
            var functionalTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);
            functionalTestMock.Setup(f => f.SetPinMask(new List<string> { "Pin4" }));
            var plist = functionalTestMock.Object;

            // Create the PinMapBase object and test the mask pins method
            // PinMapBase pinMap = new PinMapBase("TestPinMap");
            var factory = DDG.PinMap.Service;
            var pinMap = factory.Get("TestPinMap");

            pinMap.MaskPins(new BitArray(new bool[4] { false, false, false, true }), ref plist, null);

            functionalTestMock.VerifyAll();
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void MaskPins_MultiPinMap_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap1', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap2', 'Size':5, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin2':[0], 'Pin6':[1], 'Pin7':[2], 'Pin8':[3], 'Pin9':[4]}}"));

            // Mock the plist object for pinmasking.
            // List<string> maskPins = new List<string> { "Pin4", "Pin8" };
            var functionalTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);
            functionalTestMock.Setup(f => f.SetPinMask(new List<string> { "Pin4", "Pin8" }));
            var plist = functionalTestMock.Object;

            // Create the PinMapBase object and test the mask pins method
            IPinMap pinMap = new PinMapBase("TestPinMap1,TestPinMap2");

            pinMap.MaskPins(new BitArray(new bool[9] { false, false, false, true, false, false, false, true, false }), ref plist, null);

            functionalTestMock.VerifyAll();
        }

/*
        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void GetMaskPins_MultiPinMap_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap1', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap2', 'Size':5, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin2':[0], 'Pin6':[1], 'Pin7':[2], 'Pin8':[3], 'Pin9':[4]}}"));

            // Create the PinMapBase object and test the mask pins method
            IPinMap pinMap = new PinMapBase("TestPinMap1,TestPinMap2");

            var pinsToMask = pinMap.GetMaskPins(new BitArray(new bool[9] { false, false, false, true, false, false, false, true, false }));
            var expectMaskPins = new List<string>() { "Pin4", "Pin8" };
            CollectionAssert.AreEqual(expectMaskPins, pinsToMask);
        }
*/

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void MaskPlist_MultiPinMap_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap1', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap2', 'Size':5, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin2':[0], 'Pin6':[1], 'Pin7':[2], 'Pin8':[3], 'Pin9':[4]}}"));

            // Create the PinMapBase object and test the mask pins method
            IPinMap pinMap = new PinMapBase("TestPinMap1,TestPinMap2");
            var functionalTest = new Mock<IFunctionalTest>(MockBehavior.Strict).Object;

            pinMap.ModifyPlist(new BitArray(new bool[9] { false, false, false, true, false, false, false, true, false }), ref functionalTest);
            pinMap.Restore();
            var configurations = pinMap.GetConfiguration();
            foreach (var configuration in configurations)
            {
                Assert.AreEqual("PinToSliceIndexDecoder", configuration.GetDecoderType());
            }
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void DecodeFailure_Scan_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<AnyFailSingleSliceDecoder>(
                @"{'Name':'ScanPinMap', 'Size':4, 'PatternModify':'dummy', 'PinList':['Pin1','Pin2','Pin3','Pin4']}"));

            // Mock a failure.
            var fakeFailure = new Mock<IFailureData>(MockBehavior.Strict);
            fakeFailure.Setup(o => o.GetDomainName()).Returns("FakeDomain");
            fakeFailure.Setup(o => o.GetParentPlistName()).Returns("FakePList");
            fakeFailure.Setup(o => o.GetPatternName()).Returns("FakePattern");

            // Mock the plist object to return 2 failing pins.
            var captureFailureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureFailureTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string>() { "Pin2", "Pin3" });
            captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fakeFailure.Object });
            var plist = captureFailureTestMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("FakePattern")).Returns(false);

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("FakePList")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            // Create the PinMapBase object and test the decodefailure method
            PinMapBase pinMap = new PinMapBase("ScanPinMap");

            CollectionAssert.AreEqual(new BitArray(new bool[4] { false, false, true, false }), pinMap.DecodeFailure(plist, currentSlice: 2));
            captureFailureTestMock.VerifyAll();
        }

        /// <summary>
        /// Test the PinMapBase.ApplyPatConfig functionality.
        /// </summary>
        [TestMethod]
        public void ApplyPatConfig_WrongSize_Fail()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'CORE0_NOA', 'Size':1, 'PatternModify':'CORE0_DISABLE', 'PinToSliceIndexMap':{'Pin0':[0]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'CORE1_NOA', 'Size':1, 'PatternModify':'CORE1_DISABLE', 'PinToSliceIndexMap':{'Pin1':[0]}}"));

            var pinMap = DDG.PinMap.Service.Get("CORE0_NOA,CORE1_NOA");
            var ex = Assert.ThrowsException<TestMethodException>(() => pinMap.ApplyPatConfig("0100".ToBitArray()));
            Assert.AreEqual($"PinMapBase=[CORE0_NOA,CORE1_NOA] Expecting iPConfigBits to have [2] bits not [4]. iPConfigBits=[0100].", ex.Message);
        }

        /// <summary>
        /// Test the PinMapBase.ApplyPatConfig functionality.
        /// </summary>
        [TestMethod]
        public void ApplyPatConfig_NoPList_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'CORE0_NOA', 'Size':1, 'PatternModify':'CORE0_DISABLE', 'PinToSliceIndexMap':{'Pin0':[0]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'CORE1_NOA', 'Size':1, 'PatternModify':'CORE1_DISABLE', 'PinToSliceIndexMap':{'Pin1':[0]}}"));

            // Setup the patconfig mocks
            var patConfigHandle0Mock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patConfigHandle0Mock.Setup(o => o.SetData("0"));

            var patConfigHandle1Mock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patConfigHandle1Mock.Setup(o => o.SetData("1"));

            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock
                .Setup(o => o.GetPatConfigHandle("CORE0_DISABLE"))
                .Returns(patConfigHandle0Mock.Object);
            patConfigServiceMock
                .Setup(o => o.GetPatConfigHandle("CORE1_DISABLE"))
                .Returns(patConfigHandle1Mock.Object);
            patConfigServiceMock.Setup(o => o.Apply(new List<IPatConfigHandle> { patConfigHandle0Mock.Object, patConfigHandle1Mock.Object }));
            Services.PatConfigService = patConfigServiceMock.Object;

            // Create the PinMapBase object.
            var pinMap = DDG.PinMap.Service.Get("CORE0_NOA,CORE1_NOA");

            // Configure the IPs.
            pinMap.ApplyPatConfig("01".ToBitArray());

            patConfigServiceMock.Verify(o => o.Apply(It.IsAny<List<IPatConfigHandle>>()), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("CORE0_DISABLE"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("CORE1_DISABLE"), Times.Once);
            patConfigHandle0Mock.Verify(o => o.SetData("0"), Times.Once);
            patConfigHandle1Mock.Verify(o => o.SetData("1"), Times.Once);

            patConfigHandle0Mock.VerifyAll();
            patConfigHandle1Mock.VerifyAll();
            patConfigServiceMock.VerifyAll();

            // do it again to verify the caching.
            pinMap.ApplyPatConfig("01".ToBitArray());

            patConfigServiceMock.Verify(o => o.Apply(It.IsAny<List<IPatConfigHandle>>()), Times.Exactly(2));
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("CORE0_DISABLE"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("CORE1_DISABLE"), Times.Once);
            patConfigHandle0Mock.Verify(o => o.SetData("0"), Times.Exactly(2));
            patConfigHandle1Mock.Verify(o => o.SetData("1"), Times.Exactly(2));

            patConfigHandle0Mock.VerifyAll();
            patConfigHandle1Mock.VerifyAll();
            patConfigServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the PinMapBase.ApplyPatConfig functionality.
        /// </summary>
        [TestMethod]
        public void ApplyPatConfig_WithPList_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'CORE0_NOA', 'Size':1, 'PatternModify':'CORE0_DISABLE', 'PinToSliceIndexMap':{'Pin0':[0]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'CORE1_NOA', 'Size':1, 'PatternModify':'CORE1_DISABLE', 'PinToSliceIndexMap':{'Pin1':[0]}}"));

            // Setup the patconfig mocks
            var patConfigHandle0Mock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patConfigHandle0Mock.Setup(o => o.SetData("0"));

            var patConfigHandle1Mock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patConfigHandle1Mock.Setup(o => o.SetData("1"));

            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock
                .Setup(o => o.GetPatConfigHandleWithPlist("CORE0_DISABLE", "FakePList"))
                .Returns(patConfigHandle0Mock.Object);
            patConfigServiceMock
                .Setup(o => o.GetPatConfigHandleWithPlist("CORE1_DISABLE", "FakePList"))
                .Returns(patConfigHandle1Mock.Object);
            patConfigServiceMock.Setup(o => o.Apply(new List<IPatConfigHandle> { patConfigHandle0Mock.Object, patConfigHandle1Mock.Object }));
            Services.PatConfigService = patConfigServiceMock.Object;

            // Create the PinMapBase object.
            var pinMap = DDG.PinMap.Service.Get("CORE0_NOA,CORE1_NOA");

            // Configure the IPs.
            pinMap.ApplyPatConfig("01".ToBitArray(), "FakePList");

            patConfigServiceMock.Verify(o => o.Apply(It.IsAny<List<IPatConfigHandle>>()), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandleWithPlist("CORE0_DISABLE", "FakePList"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandleWithPlist("CORE1_DISABLE", "FakePList"), Times.Once);
            patConfigHandle0Mock.Verify(o => o.SetData("0"), Times.Once);
            patConfigHandle1Mock.Verify(o => o.SetData("1"), Times.Once);
            patConfigHandle0Mock.VerifyAll();
            patConfigHandle1Mock.VerifyAll();
            patConfigServiceMock.VerifyAll();

            // do it again to verify the caching.
            pinMap.ApplyPatConfig("01".ToBitArray(), "FakePList");

            patConfigServiceMock.Verify(o => o.Apply(It.IsAny<List<IPatConfigHandle>>()), Times.Exactly(2));
            patConfigServiceMock.Verify(o => o.GetPatConfigHandleWithPlist("CORE0_DISABLE", "FakePList"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandleWithPlist("CORE1_DISABLE", "FakePList"), Times.Once);
            patConfigHandle0Mock.Verify(o => o.SetData("0"), Times.Exactly(2));
            patConfigHandle1Mock.Verify(o => o.SetData("1"), Times.Exactly(2));
            patConfigHandle0Mock.VerifyAll();
            patConfigHandle1Mock.VerifyAll();
            patConfigServiceMock.VerifyAll();
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void FailTrackerToFailVoltageDomains_WrongSize_Fail()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap1', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap2', 'Size':5, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin2':[0], 'Pin6':[1], 'Pin7':[2], 'Pin8':[3], 'Pin9':[4]}}"));

            // Create the PinMapBase object and test the mask pins method
            IPinMap pinMap = new PinMapBase("TestPinMap1,TestPinMap2");

            Assert.ThrowsException<TestMethodException>(() => pinMap.FailTrackerToFailVoltageDomains("01".ToBitArray()));
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void FailTrackerToFailVoltageDomains_MultiBitTracker_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap1', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap2', 'Size':5, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin2':[0], 'Pin6':[1], 'Pin7':[2], 'Pin8':[3], 'Pin9':[4]}}"));

            // Create the PinMapBase object and test the mask pins method
            IPinMap pinMap = new PinMapBase("TestPinMap1,TestPinMap2");

            Assert.AreEqual("00", pinMap.FailTrackerToFailVoltageDomains("000000000".ToBitArray()).ToBinaryString());
            Assert.AreEqual("10", pinMap.FailTrackerToFailVoltageDomains("100000000".ToBitArray()).ToBinaryString());
            Assert.AreEqual("10", pinMap.FailTrackerToFailVoltageDomains("010000000".ToBitArray()).ToBinaryString());
            Assert.AreEqual("10", pinMap.FailTrackerToFailVoltageDomains("001000000".ToBitArray()).ToBinaryString());
            Assert.AreEqual("10", pinMap.FailTrackerToFailVoltageDomains("000100000".ToBitArray()).ToBinaryString());
            Assert.AreEqual("01", pinMap.FailTrackerToFailVoltageDomains("000010000".ToBitArray()).ToBinaryString());
            Assert.AreEqual("01", pinMap.FailTrackerToFailVoltageDomains("000001000".ToBitArray()).ToBinaryString());
            Assert.AreEqual("01", pinMap.FailTrackerToFailVoltageDomains("000000100".ToBitArray()).ToBinaryString());
            Assert.AreEqual("01", pinMap.FailTrackerToFailVoltageDomains("000000010".ToBitArray()).ToBinaryString());
            Assert.AreEqual("01", pinMap.FailTrackerToFailVoltageDomains("000000001".ToBitArray()).ToBinaryString());
            Assert.AreEqual("11", pinMap.FailTrackerToFailVoltageDomains("100000001".ToBitArray()).ToBinaryString());
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void FailTrackerToFailVoltageDomains_SingleBitTracker_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'Core0', 'Size':1, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'Core1', 'Size':1, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin2':[0]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'Core2', 'Size':1, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin3':[0]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'Core3', 'Size':1, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin4':[0]}}"));

            // Create the PinMapBase object and test the mask pins method
            IPinMap pinMap = new PinMapBase("Core0,Core1,Core2,Core3");

            Assert.AreEqual("0000", pinMap.FailTrackerToFailVoltageDomains("0000".ToBitArray()).ToBinaryString());
            Assert.AreEqual("1000", pinMap.FailTrackerToFailVoltageDomains("1000".ToBitArray()).ToBinaryString());
            Assert.AreEqual("0100", pinMap.FailTrackerToFailVoltageDomains("0100".ToBitArray()).ToBinaryString());
            Assert.AreEqual("0010", pinMap.FailTrackerToFailVoltageDomains("0010".ToBitArray()).ToBinaryString());
            Assert.AreEqual("0001", pinMap.FailTrackerToFailVoltageDomains("0001".ToBitArray()).ToBinaryString());
            Assert.AreEqual("1100", pinMap.FailTrackerToFailVoltageDomains("1100".ToBitArray()).ToBinaryString());
            Assert.AreEqual("0011", pinMap.FailTrackerToFailVoltageDomains("0011".ToBitArray()).ToBinaryString());
            Assert.AreEqual("1111", pinMap.FailTrackerToFailVoltageDomains("1111".ToBitArray()).ToBinaryString());
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void VoltageDomainsToFailTracker_WrongSize_Fail()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap1', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap2', 'Size':5, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin2':[0], 'Pin6':[1], 'Pin7':[2], 'Pin8':[3], 'Pin9':[4]}}"));

            // Create the PinMapBase object and test the mask pins method
            IPinMap pinMap = new PinMapBase("TestPinMap1,TestPinMap2");

            Assert.ThrowsException<TestMethodException>(() => pinMap.VoltageDomainsToFailTracker("0".ToBitArray()));
            Assert.ThrowsException<TestMethodException>(() => pinMap.VoltageDomainsToFailTracker("111".ToBitArray()));
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void VoltageDomainsToFailTracker_MultiBitTracker_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap1', 'Size':4, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0], 'Pin2':[1], 'Pin3':[2], 'Pin4':[3]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'TestPinMap2', 'Size':5, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin2':[0], 'Pin6':[1], 'Pin7':[2], 'Pin8':[3], 'Pin9':[4]}}"));

            // Create the PinMapBase object and test the mask pins method
            IPinMap pinMap = new PinMapBase("TestPinMap1,TestPinMap2");

            Assert.AreEqual("000000000", pinMap.VoltageDomainsToFailTracker("00".ToBitArray()).ToBinaryString());
            Assert.AreEqual("111100000", pinMap.VoltageDomainsToFailTracker("10".ToBitArray()).ToBinaryString());
            Assert.AreEqual("000011111", pinMap.VoltageDomainsToFailTracker("01".ToBitArray()).ToBinaryString());
            Assert.AreEqual("111111111", pinMap.VoltageDomainsToFailTracker("11".ToBitArray()).ToBinaryString());
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void VoltageDomainsToFailTracker_SingleBitTracker_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'Core0', 'Size':1, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'Core1', 'Size':1, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin2':[0]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'Core2', 'Size':1, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin3':[0]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'Core3', 'Size':1, 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin4':[0]}}"));

            // Create the PinMapBase object and test the mask pins method
            IPinMap pinMap = new PinMapBase("Core0,Core1,Core2,Core3");

            Assert.AreEqual("0000", pinMap.VoltageDomainsToFailTracker("0000".ToBitArray()).ToBinaryString());
            Assert.AreEqual("1000", pinMap.VoltageDomainsToFailTracker("1000".ToBitArray()).ToBinaryString());
            Assert.AreEqual("0100", pinMap.VoltageDomainsToFailTracker("0100".ToBitArray()).ToBinaryString());
            Assert.AreEqual("0010", pinMap.VoltageDomainsToFailTracker("0010".ToBitArray()).ToBinaryString());
            Assert.AreEqual("0001", pinMap.VoltageDomainsToFailTracker("0001".ToBitArray()).ToBinaryString());
            Assert.AreEqual("1100", pinMap.VoltageDomainsToFailTracker("1100".ToBitArray()).ToBinaryString());
            Assert.AreEqual("0011", pinMap.VoltageDomainsToFailTracker("0011".ToBitArray()).ToBinaryString());
            Assert.AreEqual("1111", pinMap.VoltageDomainsToFailTracker("1111".ToBitArray()).ToBinaryString());
        }

        /// <summary>
        /// Test the PinMapBase.ApplyPatConfig functionality.
        /// </summary>
        [TestMethod]
        public void GetConfiguration_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'CORE0_NOA', 'Size':1, 'PatternModify':'CORE0_DISABLE', 'PinToSliceIndexMap':{'Pin0':[0]}}"));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'CORE1_NOA', 'Size':1, 'PatternModify':'CORE1_DISABLE', 'PinToSliceIndexMap':{'Pin1':[0]}}"));

            var pinMap = DDG.PinMap.Service.Get("CORE0_NOA,CORE1_NOA");
            var decoders = pinMap.GetConfiguration();
            Assert.AreEqual(2, decoders.Count);
            Assert.AreEqual("CORE0_NOA", decoders[0].Name);
            Assert.AreEqual("CORE1_NOA", decoders[1].Name);
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void DecodeFailure_SharedStorageResults_Pass()
        {
            // Create a dummy pinmap configuration.
            DDG.DieRecovery.Utilities.StorePinMapDecoder(JsonConvert.DeserializeObject<PinToSliceIndexDecoder>(
                @"{'Name':'Core0', 'Size':1, 'SharedStorageResults':'token', 'PatternModify':'dummy', 'PinToSliceIndexMap':{'Pin1':[0]}}"));

            // Create the PinMapBase object and test the mask pins method
            IPinMap pinMap = new PinMapBase("Core0");

            var test = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            test.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData>());
            test.Setup(o => o.GetFailingPinNames()).Returns(new List<string>());
            pinMap.DecodeFailure(test.Object, null);
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("token", "0", Context.DUT));
        }
    }
}
