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

namespace DieRecoveryBase.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PatternService;
    using Prime.PlistService;
    using Prime.SharedStorageService;
    using TOSUserSDK;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConcurrentPlistDecoder_UnitTest
    {
        private string configurationJson;
        private IPinMapDecoder decoder;

        private Dictionary<string, string> SharedStorageValues { get; set; }

        private Mock<ISharedStorageService> SharedStorageMock { get; set; }

        /// <summary>
        /// Initializes all tests.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
            {
                System.Console.WriteLine($"DEBUG: {msg}");
            });

            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // Default Mock for Shared service.
            this.SharedStorageValues = new Dictionary<string, string>();
            this.SharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.SharedStorageMock.Setup(o =>
                    o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>()))
                .Callback((string key, object obj, Context context) =>
                {
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.SharedStorageMock.Setup(o =>
                    o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, string obj, Context context) =>
                {
                    this.SharedStorageValues[key] = obj;
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.SharedStorageMock.Setup(o =>
                    o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<Context>()))
                .Callback((string key, double obj, Context context) =>
                {
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.SharedStorageMock
                .Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>()))
                .Callback((string key, Type obj, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Type obj, Context context) =>
                    JsonConvert.DeserializeObject(this.SharedStorageValues[key], obj));
            this.SharedStorageMock.Setup(o => o.GetDoubleRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => double.Parse(this.SharedStorageValues[key]));
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => this.SharedStorageValues[key]);
            this.SharedStorageMock.Setup(o => o.KeyExistsInObjectTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Returns((string key, Context c) => this.SharedStorageValues.ContainsKey(key));
            Prime.Services.SharedStorageService = this.SharedStorageMock.Object;

            this.configurationJson =
@"
{
    'Description': 'TargetPositions: CORE1,CORE0,SLICE3,SLICE2,SLICE1,SLICE0',
    'CtvDomain': 'CPU_FAB_ALL',
    'ConcurrentPlists': [
        {
            'Comment': 'CORE',
            'TargetPositions': [0,1],
            'PlistName': 'arr_cdie_pbist_core_ccr_r1',
            'FailingPins': ['YY_TEST_PORT_OUT_C2S_01','YY_TEST_PORT_OUT_C2S_00']
        },
        {
            'Comment': 'CCF',
            'TargetPositions': [2,3,4,5],
            'PlistName': 'arr_cdie_pbist_ccf_ccr_r1'
        }
    ]
}
";
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        public void ReadInputFile()
        {
            this.decoder = (IPinMapDecoder)JsonConvert.DeserializeObject(this.configurationJson, typeof(ConcurrentPlistDecoder));
            this.decoder.NumberOfTrackerElements = 6;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_InvalidPlistObject_Fail()
        {
            this.ReadInputFile();
            var result = Assert.ThrowsException<ArgumentException>(() => this.decoder.GetFailTrackerFromPlistResults(null, null));
            Assert.AreEqual("DieRecoveryBase.dll.GetFailTrackerFromPlistResults: unable to cast IFunctionalTest into ICaptureFailureAndCtvPerCycleTest object. Using incorrect input type for this decoder.", result.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_NoFails_Pass()
        {
            this.ReadInputFile();
            var test = new Mock<ICaptureFailureAndCtvPerCycleTest>(MockBehavior.Strict);
            test.Setup(o => o.GetPlistName()).Returns("MAIN");
            test.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData>());
            var result = this.decoder.GetFailTrackerFromPlistResults(test.Object);
            Assert.AreEqual("000000", result.ToBinaryString());
            test.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_NotMatchingFilter_Fail()
        {
            this.ReadInputFile();
            MockingPlistContents();
            var test = new Mock<ICaptureFailureAndCtvPerCycleTest>(MockBehavior.Strict);
            test.Setup(o => o.GetPlistName()).Returns("MAIN");
            var fail = new Mock<IFailureData>(MockBehavior.Strict);
            fail.Setup(o => o.GetParentPlistName()).Returns("UNKNOWN_PLIST");
            fail.Setup(o => o.GetFailingPinNames()).Returns(new List<string>());
            fail.Setup(o => o.GetPatternName()).Returns("CORE_PATTERN");
            fail.Setup(o => o.GetPatternInstanceId()).Returns(1);
            test.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fail.Object });

            var functionalTest = test.As<IFunctionalTest>().Object;
            this.decoder.Verify(ref functionalTest);
            var result = this.decoder.GetFailTrackerFromPlistResults(functionalTest);
            Assert.AreEqual("111111", result.ToBinaryString());

            test.VerifyAll();
            fail.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_FailingPin_Pass()
        {
            this.ReadInputFile();
            MockingPlistContents();
            var test = new Mock<ICaptureFailureAndCtvPerCycleTest>(MockBehavior.Strict);
            test.Setup(o => o.GetPlistName()).Returns("MAIN");
            var fail = new Mock<IFailureData>(MockBehavior.Strict);
            fail.Setup(o => o.GetParentPlistName()).Returns("arr_cdie_pbist_core_ccr_r1");
            fail.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "YY_TEST_PORT_OUT_C2S_01" });
            fail.Setup(o => o.GetPatternName()).Returns("CORE_PATTERN");
            fail.Setup(o => o.GetPatternInstanceId()).Returns(1);
            test.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fail.Object });
            var ctvPattern = new Mock<ICtvPerPattern>(MockBehavior.Strict);
            ctvPattern.Setup(o => o.GetInstanceId()).Returns(1);
            ctvPattern.SetupSequence(o => o.GetName())
                .Returns("CCF_PATTERN")
                .Returns("CORE_PATTERN");
            ctvPattern.SetupSequence(o => o.GetParentPlistName())
                .Returns("arr_cdie_pbist_ccf_ccr_r1")
                .Returns("arr_cdie_pbist_core_ccr_r1");
            test.Setup(o => o.GetCtvPerPattern()).Returns(new List<ICtvPerPattern> { ctvPattern.Object, ctvPattern.Object });

            var functionalTest = test.As<IFunctionalTest>().Object;
            this.decoder.Verify(ref functionalTest);
            var result = this.decoder.GetFailTrackerFromPlistResults(functionalTest);
            Assert.AreEqual("100000", result.ToBinaryString());

            test.VerifyAll();
            fail.VerifyAll();
            ctvPattern.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_NoCtvData_Pass()
        {
            this.ReadInputFile();
            MockingPlistContents();
            var test = new Mock<ICaptureFailureAndCtvPerCycleTest>(MockBehavior.Strict);
            test.Setup(o => o.GetPlistName()).Returns("MAIN");
            var fail = new Mock<IFailureData>(MockBehavior.Strict);
            fail.Setup(o => o.GetParentPlistName()).Returns("arr_cdie_pbist_core_ccr_r1");
            fail.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "YY_TEST_PORT_OUT_C2S_01" });
            test.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fail.Object });
            test.Setup(o => o.GetCtvPerPattern()).Returns(new List<ICtvPerPattern>());
            fail.Setup(o => o.GetPatternName()).Returns("CORE_PATTERN");
            fail.Setup(o => o.GetPatternInstanceId()).Returns(1);
            var ex = Assert.ThrowsException<Exception>(() => this.decoder.GetFailTrackerFromPlistResults(test.Object));
            Assert.AreEqual("CPlist: CTV data was captured. Unable to determine last pattern executed for each IP.", ex.Message);

            test.VerifyAll();
            fail.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_NoFailingPin_Pass()
        {
            this.ReadInputFile();
            MockingPlistContents();
            var test = new Mock<ICaptureFailureAndCtvPerCycleTest>(MockBehavior.Strict);
            test.Setup(o => o.GetPlistName()).Returns("MAIN");
            var fail = new Mock<IFailureData>(MockBehavior.Strict);
            fail.Setup(o => o.GetParentPlistName()).Returns("arr_cdie_pbist_ccf_ccr_r1");
            fail.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "UNKNOWN" });
            fail.Setup(o => o.GetPatternName()).Returns("CORE_PATTERN");
            fail.Setup(o => o.GetPatternInstanceId()).Returns(1);
            test.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fail.Object });
            var ctvPattern = new Mock<ICtvPerPattern>(MockBehavior.Strict);
            ctvPattern.Setup(o => o.GetInstanceId()).Returns(1);
            ctvPattern.SetupSequence(o => o.GetName())
                .Returns("CCF_PATTERN")
                .Returns("CORE_PATTERN");
            ctvPattern.SetupSequence(o => o.GetParentPlistName())
                .Returns("arr_cdie_pbist_ccf_ccr_r1")
                .Returns("arr_cdie_pbist_core_ccr_r1");
            test.Setup(o => o.GetCtvPerPattern()).Returns(new List<ICtvPerPattern> { ctvPattern.Object, ctvPattern.Object });

            var functionalTest = test.As<IFunctionalTest>().Object;
            this.decoder.Verify(ref functionalTest);
            var result = this.decoder.GetFailTrackerFromPlistResults(functionalTest);
            Assert.AreEqual("001111", result.ToBinaryString());

            test.VerifyAll();
            fail.VerifyAll();
            ctvPattern.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Verify_Pass()
        {
            this.ReadInputFile();
            MockingPlistContents();
            var test = new Mock<ICaptureFailureAndCtvPerCycleTest>(MockBehavior.Strict);
            var captureFailureAndCtvPerCycleTest = test.Object as IFunctionalTest;
            this.decoder.Verify(ref captureFailureAndCtvPerCycleTest);

            test.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_AllZeros_Pass()
        {
            this.ReadInputFile();
            MockingPlistContents();
            var test = new Mock<ICaptureFailureAndCtvPerCycleTest>(MockBehavior.Strict);
            var captureFailureAndCtvPerCycleTest = test.As<IFunctionalTest>();
            var ob = captureFailureAndCtvPerCycleTest.Object;
            this.decoder.Verify(ref ob);
            var result = this.decoder.MaskPlistFromTracker(new BitArray(new[] { false, false, false, false, false, false }), ref ob);

            Assert.IsTrue(result.Count == 0);
            test.VerifyAll();
            captureFailureAndCtvPerCycleTest.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_LastExecutedPatterns_Pass()
        {
            this.ReadInputFile();
            MockingPlistContents();
            var test = new Mock<ICaptureFailureAndCtvPerCycleTest>(MockBehavior.Strict);
            test.Setup(o => o.SetStartPatternForConcurrentPlist(new Dictionary<string, Tuple<string, uint>>
            {
                { "arr_cdie_pbist_core_ccr_r1", new Tuple<string, uint>("CORE_PATTERN", 1) },
                { "arr_cdie_pbist_ccf_ccr_r1", new Tuple<string, uint>("CCF_PATTERN", 1) },
            }));
            var captureFailureAndCtvPerCycleTest = test.As<IFunctionalTest>();
            var plistService = new Mock<IPlistService>(MockBehavior.Strict);

            var ctvPattern = new Mock<ICtvPerPattern>(MockBehavior.Strict);
            ctvPattern.Setup(o => o.GetInstanceId()).Returns(1);
            ctvPattern.SetupSequence(o => o.GetName())
                .Returns("CORE_PATTERN");
            ctvPattern.SetupSequence(o => o.GetParentPlistName())
                .Returns("arr_cdie_pbist_core_ccr_r1");
            test.Setup(o => o.GetPlistName()).Returns("MAIN");
            test.Setup(o => o.GetCtvPerPattern()).Returns(new List<ICtvPerPattern> { ctvPattern.Object, ctvPattern.Object });
            var fail = new Mock<IFailureData>(MockBehavior.Strict);
            fail.Setup(o => o.GetParentPlistName()).Returns("arr_cdie_pbist_ccf_ccr_r1");
            fail.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "UNKNOWN" });
            fail.Setup(o => o.GetPatternName()).Returns("CCF_PATTERN");
            fail.Setup(o => o.GetPatternInstanceId()).Returns(1);
            test.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fail.Object });

            var ob = captureFailureAndCtvPerCycleTest.Object;
            this.decoder.Verify(ref ob);
            this.decoder.MaskPlistFromTracker(new BitArray(new[] { false, false, false, false, false, false }), ref ob);
            var result = this.decoder.GetFailTrackerFromPlistResults(ob);
            Assert.AreEqual("001111", result.ToBinaryString());
            this.decoder.MaskPlistFromTracker(new BitArray(new[] { false, false, false, false, false, false }), ref ob);

            test.VerifyAll();
            captureFailureAndCtvPerCycleTest.VerifyAll();
            plistService.VerifyAll();
            ctvPattern.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_SkippingOneIP_Pass()
        {
            this.ReadInputFile();
            MockingPlistContents();
            var test = new Mock<ICaptureFailureAndCtvPerCycleTest>(MockBehavior.Strict);
            test.Setup(o => o.SetStartPatternForConcurrentPlist(new Dictionary<string, Tuple<string, uint>>
            {
                { "arr_cdie_pbist_core_ccr_r1", new Tuple<string, uint>("CORE_PATTERN", 1) },
            }));
            var captureFailureAndCtvPerCycleTest = test.As<IFunctionalTest>();

            var ob = captureFailureAndCtvPerCycleTest.Object;
            this.decoder.Verify(ref ob);
            var result = this.decoder.MaskPlistFromTracker(new BitArray(new[] { true, true, false, false, false, false }), ref ob);

            Assert.IsTrue(result.Count == 0);
            test.VerifyAll();
            captureFailureAndCtvPerCycleTest.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_MissingFailingPinsConfiguration_Fail()
        {
            this.ReadInputFile();
            MockingPlistContents();
            var test = new Mock<ICaptureFailureAndCtvPerCycleTest>(MockBehavior.Strict);
            var captureFailureAndCtvPerCycleTest = test.As<IFunctionalTest>();

            var ob = captureFailureAndCtvPerCycleTest.Object;
            this.decoder.Verify(ref ob);
            var ex = Assert.ThrowsException<Exception>(() => this.decoder.MaskPlistFromTracker(new BitArray(new[] { false, false, true, false, false, false }), ref ob));
            Assert.AreEqual("CPlist: arr_cdie_pbist_ccf_ccr_r1 does not contain FailingPins and only partial TargetPositions are disabled. Bits=[001000].", ex.Message);

            test.VerifyAll();
            captureFailureAndCtvPerCycleTest.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_IncorrectCaptureSettings_Fail()
        {
            this.ReadInputFile();
            MockingPlistContents();
            var test = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var captureFailureAndCtvPerCycleTest = test.As<IFunctionalTest>();

            var ob = captureFailureAndCtvPerCycleTest.Object;
            this.decoder.Verify(ref ob);
            var ex = Assert.ThrowsException<Exception>(() => this.decoder.MaskPlistFromTracker(new BitArray(new[] { false, false, true, false, false, false }), ref ob));
            Assert.AreEqual("CPlist: using incorrect capture settings. Please set Prime.FunctionalService.ICaptureFailureAndCtvPerCycleTest.", ex.Message);

            test.VerifyAll();
            captureFailureAndCtvPerCycleTest.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_MaskingPins_Pass()
        {
            this.ReadInputFile();
            MockingPlistContents();

            var test = new Mock<ICaptureFailureAndCtvPerCycleTest>(MockBehavior.Strict);
            var captureFailureAndCtvPerCycleTest = test.As<IFunctionalTest>();

            var ob = captureFailureAndCtvPerCycleTest.Object;
            this.decoder.Verify(ref ob);
            var result = this.decoder.MaskPlistFromTracker(new BitArray(new[] { true, false, false, false, false, false }), ref ob);

            Assert.AreEqual("YY_TEST_PORT_OUT_C2S_01", result[0]);
            test.VerifyAll();
            captureFailureAndCtvPerCycleTest.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetDecoderType_Pass()
        {
            this.ReadInputFile();
            var result = this.decoder.GetDecoderType();
            Assert.AreEqual("ConcurrentPlistDecoder", result);
        }

        private static void MockingPlistContents()
        {
            var plistService = new Mock<IPlistService>(MockBehavior.Strict);
            Prime.Services.PlistService = plistService.Object;
            var plist = new Mock<IPlistObject>(MockBehavior.Strict);
            plistService.Setup(o => o.GetPlistObject("arr_cdie_pbist_core_ccr_r1")).Returns(plist.Object);
            plistService.Setup(o => o.GetPlistObject("arr_cdie_pbist_ccf_ccr_r1")).Returns(plist.Object);
            var plistContentA = new Mock<IPlistContent>(MockBehavior.Strict);
            plistContentA.Setup(o => o.IsPattern()).Returns(true);
            plistContentA.Setup(o => o.GetPlistItemName()).Returns("CCF_PATTERN");
            var plistContentB = new Mock<IPlistContent>(MockBehavior.Strict);
            plistContentB.Setup(o => o.IsPattern()).Returns(true);
            plistContentB.Setup(o => o.GetPlistItemName()).Returns("CORE_PATTERN");
            plist.SetupSequence(o => o.GetPatternsAndIndexes(true))
                .Returns(new List<IPlistContent> { plistContentB.Object })
                .Returns(new List<IPlistContent> { plistContentA.Object });
            var tosUserSdkPattern = new Mock<IPattern>(MockBehavior.Strict);
            TOSUserSDK.Pattern.Service = tosUserSdkPattern.Object;
            var tosUserSdkPlist = new Mock<IPlist>(MockBehavior.Strict);
            tosUserSdkPattern.Setup(o => o.SetPVCData("CCF_PATTERN", "CPU_FAB_ALL", 1, "CTV", 1));
            tosUserSdkPattern.Setup(o => o.SetPVCData("CORE_PATTERN", "CPU_FAB_ALL", 1, "CTV", 1));
        }
    }
}
