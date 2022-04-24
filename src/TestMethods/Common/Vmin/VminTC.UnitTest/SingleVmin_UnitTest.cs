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

namespace VminTC.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PerformanceService;
    using Prime.PinService;
    using Prime.PlistService;
    using Prime.ScoreboardService;
    using Prime.SharedStorageService;
    using Prime.TestConditionService;
    using Prime.TestMethods.VminSearch;
    using Prime.TestProgramService;
    using Prime.UserVarService;
    using Prime.VoltageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SingleVmin_UnitTest : VminTC
    {
        private Mock<IPinMap> pinMapMock;
        private Mock<IDieRecovery> dieRecoveryMock;
        private Mock<IVminForwardingCorner> vminForwardingMock;
        private Mock<IVminForwardingFactory> vminForwardingFactoryMock;
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<ISharedStorageService> sharedStorageMock;
        private Dictionary<string, string> sharedStorageValues;
        private Mock<ITestProgramService> testProgramServiceMock;
        private Mock<IDieRecoveryFactory> dieRecoveryFactoryMock;
        private Mock<IScoreboardLogger> scoreBoardLoggerMock;
        private Mock<IDatalogService> datalogServiceMock;
        private Mock<IStrgvalFormat> strgvalFormatMock;
        private Mock<IPlistService> plistServiceMock;
        private Mock<IPlistObject> plistObjectMock;
        private Mock<IPerformanceService> performanceServiceMock;

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            // Default Mock for Shared service.
            this.sharedStorageValues = new Dictionary<string, string>();
            this.sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>()))
                .Callback((string key, object obj, Context context) =>
                {
                    System.Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.sharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                });
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, string obj, Context context) =>
                {
                    System.Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.sharedStorageValues[key] = obj;
                });
            this.sharedStorageMock.Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>()))
                .Callback((string key, Type obj, Context context) =>
                {
                    System.Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.sharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Type obj, Context context) => JsonConvert.DeserializeObject(this.sharedStorageValues[key], obj));
            this.sharedStorageMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    System.Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.sharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => this.sharedStorageValues[key]);
            this.sharedStorageMock.Setup(o => o.KeyExistsInObjectTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    if (this.sharedStorageValues.ContainsKey(key))
                    {
                        System.Console.WriteLine($"SharedStorage Key={key} exists in table.");
                    }
                    else
                    {
                        System.Console.WriteLine($"SharedStorage Key={key} exists in table.");
                    }
                })
                .Returns((string key, Context context) => this.sharedStorageValues.ContainsKey(key));
            this.sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    if (this.sharedStorageValues.ContainsKey(key))
                    {
                        System.Console.WriteLine($"SharedStorage Key={key} exists in table.");
                    }
                    else
                    {
                        System.Console.WriteLine($"SharedStorage Key={key} exists in table.");
                    }
                })
                .Returns((string key, Context context) => this.sharedStorageValues.ContainsKey(key));
            Prime.Services.SharedStorageService = this.sharedStorageMock.Object;

            this.performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = this.performanceServiceMock.Object;

            this.FeatureSwitchSettings = string.Empty;
            this.PinMap = string.Empty;
            this.CornerIdentifiers = string.Empty;
            this.InitialMaskBits = string.Empty;
            this.Patlist = string.Empty;
            this.VoltageTargets = "PinName";
            this.FivrCondition = "FivrCondition";
            this.TestMode = TestModes.SingleVmin;
            this.LevelsTc = "SomeLevels";

            this.vminForwardingMock = new Mock<IVminForwardingCorner>(MockBehavior.Default);
            this.vminForwardingFactoryMock = new Mock<IVminForwardingFactory>(MockBehavior.Strict);
            this.vminForwardingFactoryMock.Setup(f => f.Get(It.IsAny<string>(), It.IsAny<int>())).Returns(this.vminForwardingMock.Object);
            this.vminForwardingFactoryMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            this.pinMapMock = new Mock<IPinMap>(MockBehavior.Default);
            var pinMapFactoryMock = new Mock<IPinMapFactory>(MockBehavior.Strict);
            pinMapFactoryMock.Setup(p => p.Get(It.IsAny<string>())).Returns(this.pinMapMock.Object);
            this.dieRecoveryMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            this.dieRecoveryFactoryMock = new Mock<IDieRecoveryFactory>(MockBehavior.Strict);
            this.dieRecoveryFactoryMock.Setup(d => d.Get(It.IsAny<string>())).Returns(this.dieRecoveryMock.Object);
            DDG.VminForwarding.Service = this.vminForwardingFactoryMock.Object;
            DDG.PinMap.Service = pinMapFactoryMock.Object;
            DDG.DieRecovery.Service = this.dieRecoveryFactoryMock.Object;

            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).
                Callback((string msg) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            this.consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string member, string path) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            this.testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            Prime.Services.TestProgramService = this.testProgramServiceMock.Object;

            this.scoreBoardLoggerMock = new Mock<IScoreboardLogger>(MockBehavior.Strict);
            var scoreBoardServiceMock = new Mock<IScoreboardService>(MockBehavior.Strict);
            scoreBoardServiceMock.Setup(o => o.CreateLogger(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ulong>())).Returns(this.scoreBoardLoggerMock.Object);
            Prime.Services.ScoreBoardService = scoreBoardServiceMock.Object;

            this.datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            this.strgvalFormatMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            this.datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.strgvalFormatMock.Object);
            Prime.Services.DatalogService = this.datalogServiceMock.Object;

            this.plistServiceMock = new Mock<IPlistService>(MockBehavior.Default);
            this.plistObjectMock = new Mock<IPlistObject>(MockBehavior.Default);
            this.plistServiceMock.Setup(o => o.GetPlistObject(It.IsAny<string>())).Returns(this.plistObjectMock.Object);
            Prime.Services.PlistService = this.plistServiceMock.Object;

            this.VerifyDts();
        }

        /// <summary>
        /// Null ForwardingMode should skip checks.
        /// </summary>
        [TestMethod]
        public void VerifyForwarding_ForwardingModeNone_True()
        {
            this.FeatureSwitchSettings = string.Empty;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
        }

        /// <summary>
        /// Empty ForwardingMode should skip checks.
        /// </summary>
        [TestMethod]
        public void VerifyForwarding_EmptyForwardingMode_True()
        {
            this.FeatureSwitchSettings = string.Empty;
            this.ForwardingMode = ForwardingModes.None;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
        }

        /// <summary>
        /// Valid forwarding mode and corner identifiers.
        /// </summary>
        [TestMethod]
        public void VerifyForwarding_ValidForwardingMode_Pass()
        {
            this.FeatureSwitchSettings = string.Empty;
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.CornerIdentifiers = "id1,id2";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void VerifySingleVminMode_NumberOfVoltageTargets_Fail()
        {
            this.FeatureSwitchSettings = string.Empty;
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.CornerIdentifiers = "id1,id2";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.VoltageTargets = "1,2";
            this.VerifyFeatureSwitchSettings();
            var ex = Assert.ThrowsException<ArgumentException>(this.CustomVerify);
            Assert.AreEqual("VminTC.dll.VerifySingleVminMode: supports a single VoltageTargets", ex.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void VerifySingleVminMode_MultiPassMasksIsNotSupported_Fail()
        {
            this.FeatureSwitchSettings = string.Empty;
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.CornerIdentifiers = "id1,id2";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.VoltageTargets = "1";
            this.MultiPassMasks = "01,10";
            this.VerifyFeatureSwitchSettings();
            Assert.ThrowsException<ArgumentException>(() => this.CustomVerify());
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void VerifySingleVminMode_MaxRepetition_Fail()
        {
            this.FeatureSwitchSettings = string.Empty;
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.CornerIdentifiers = "id1,id2";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.VoltageTargets = "1";
            this.RecoveryMode = RecoveryModes.Default;
            this.RecoveryOptions = "0000";
            this.PinMap = "PinMap";
            this.MaxRepetitionCount = 2;
            this.VerifyFeatureSwitchSettings();
            var ex = Assert.ThrowsException<ArgumentException>(this.CustomVerify);
            Assert.AreEqual("VminTC.dll.VerifyRecoveryMode: use of MaxRepetitionCount greater than 1 requires to set RecoveryLoop or RecoveryFailRetest.", ex.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void VerifySingleVminMode_RecoveryPortModeMissingPinMap_Fail()
        {
            this.FeatureSwitchSettings = string.Empty;
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.CornerIdentifiers = "id1,id2";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.VoltageTargets = "1";
            this.RecoveryMode = RecoveryModes.RecoveryPort;
            this.RecoveryOptions = "0000";
            this.PinMap = string.Empty;
            this.VerifyFeatureSwitchSettings();
            var ex = Assert.ThrowsException<ArgumentException>(this.CustomVerify);
            Assert.AreEqual("VminTC.dll.VerifyRecoveryMode: use of RecoveryPort requires to set PinMap.", ex.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void VerifySingleVminMode_RecoveryLoopModeMissingPinMap_Fail()
        {
            this.FeatureSwitchSettings = string.Empty;
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.CornerIdentifiers = "id1,id2";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.VoltageTargets = "1";
            this.RecoveryMode = RecoveryModes.RecoveryLoop;
            this.RecoveryOptions = "0000";
            this.PinMap = string.Empty;
            this.VerifyFeatureSwitchSettings();
            var ex = Assert.ThrowsException<ArgumentException>(this.CustomVerify);
            Assert.AreEqual("VminTC.dll.VerifyRecoveryMode: use of RecoveryLoop requires to set PinMap.", ex.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void VerifySingleVminMode_RecoveryFailRetestMissingPinMap_Fail()
        {
            this.FeatureSwitchSettings = string.Empty;
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.CornerIdentifiers = "id1,id2";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.VoltageTargets = "1";
            this.RecoveryMode = RecoveryModes.RecoveryFailRetest;
            this.RecoveryOptions = "0000";
            this.PinMap = string.Empty;
            this.VerifyFeatureSwitchSettings();
            var ex = Assert.ThrowsException<ArgumentException>(this.CustomVerify);
            Assert.AreEqual("VminTC.dll.VerifyRecoveryMode: use of RecoveryFailRetest requires to set PinMap.", ex.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void VerifyDieRecovery_MissingPinMap1_Fail()
        {
            this.FeatureSwitchSettings = string.Empty;
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.CornerIdentifiers = "id1,id2";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.VoltageTargets = "1";
            this.RecoveryTrackingIncoming = "0000";
            this.RecoveryTrackingOutgoing = "0000";
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            var ex = Assert.ThrowsException<ArgumentException>(this.SetDieRecoveryTrackers);
            Assert.AreEqual("SetDieRecoveryTrackers: use of RecoveryTrackingOutgoing requires to specify a valid PinMap.", ex.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void VerifyDieRecovery_MissingPinMap2_Fail()
        {
            this.FeatureSwitchSettings = string.Empty;
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.CornerIdentifiers = "id1,id2";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.VoltageTargets = "1";
            this.RecoveryTrackingIncoming = "0000";
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            var ex = Assert.ThrowsException<ArgumentException>(this.SetDieRecoveryTrackers);
            Assert.AreEqual("SetDieRecoveryTrackers: use of RecoveryTrackingIncoming requires to specify a valid PinMap.", ex.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void VerifyDieRecovery_MissingPinMap3_Fail()
        {
            this.FeatureSwitchSettings = string.Empty;
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.CornerIdentifiers = "id1,id2";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.VoltageTargets = "1";
            this.InitialMaskBits = "0000";
            var ex = Assert.ThrowsException<ArgumentException>(this.CustomVerify);
            Assert.AreEqual("VerifyDieRecovery: use of InitialMaskBits requires to specify a valid PinMap.", ex.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void VerifyPerPatternPrinting_MissingPatternMap_Fail()
        {
            this.Patlist = "SomePatlist";
            this.LevelsTc = "SomeLevels";
            this.TimingsTc = "SomeTimings";
            this.PrePlist = string.Empty;
            this.FeatureSwitchSettings = "per_pattern_printing";

            this.plistObjectMock.Setup(o => o.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            this.plistObjectMock.Setup(o => o.GetPatternsAndIndexes(true)).Returns(new List<IPlistContent>());
            this.plistServiceMock.Setup(o => o.GetPlistObject(this.Patlist)).Returns(this.plistObjectMock.Object);
            Prime.Services.PlistService = this.plistServiceMock.Object;
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var captureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            functionalServiceMock
                .Setup(o => o.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 1, string.Empty))
                .Returns(captureTestMock.Object);
            Prime.Services.FunctionalService = functionalServiceMock.Object;

            ((IVminSearchExtensions)this).GetFunctionalTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.PrePlist);
            var ex = Assert.ThrowsException<ArgumentException>(this.CustomVerify);
            Assert.AreEqual("VminTC.dll.VerifyPerPatternPrinting: use of per_pattern_printing feature requires to set PatternNameMap", ex.Message);
        }

        /// <summary>
        /// Initial mask bits from parameter.
        /// </summary>
        [TestMethod]
        public void SetInitialMask_FromParameter_DefaultValue()
        {
            bool[] expectedBooleans = { true, false, true, false, true, true, true, true };
            var expected = new BitArray(expectedBooleans);
            this.InitialMaskBits = "10101111";
            this.StartVoltages = "0.5";
            this.InitializeVoltageTargets();
            this.VerifyFeatureSwitchSettings();
            this.VerifyRecoveryMode();
            this.SetIncomingMask();
            this.SetInitialMask();

            Assert.IsTrue(expected.Xor(this.InitialSearchMask_).OfType<bool>().All(e => !e));
        }

        /// <summary>
        /// Initial mask bits from parameter in InputOutput mode.
        /// </summary>
        [TestMethod]
        public void SetInitialMask_InputOutput_Pass()
        {
            bool[] expectedBooleans = { true, true, true, false, true, true, true, true };
            var expected = new BitArray(expectedBooleans);
            this.InitialMaskBits = "10101111";
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.StartVoltages = "0.5";
            bool[] fakeBooleans = { false, true, false, false, false, false, false, false };
            var fakeDieRecoveryBitArray = new BitArray(fakeBooleans);
            this.dieRecoveryMock.Setup(d => d.GetMaskBits()).Returns(fakeDieRecoveryBitArray);
            this.DieRecoveryIncoming_ = this.dieRecoveryMock.Object;
            this.InitializeVoltageTargets();
            this.VerifyFeatureSwitchSettings();
            this.VerifyRecoveryMode();
            this.SetIncomingMask();
            this.SetInitialMask();

            Assert.IsTrue(expected.Xor(this.InitialSearchMask_).OfType<bool>().All(e => !e));
            this.dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Initial mask bits from parameter in Input mode.
        /// </summary>
        [TestMethod]
        public void SetInitialMask_Input_Pass()
        {
            bool[] expectedBooleans = { true, false, true, true, true, true, true, true };
            var expected = new BitArray(expectedBooleans);
            this.InitialMaskBits = "10101111";
            this.ForwardingMode = ForwardingModes.Input;
            this.VoltageTargets = "FivrDomain";
            this.StartVoltages = "0.5";
            bool[] fakeBooleans = { false, false, false, true, false, false, false, false };
            var fakeDieRecoveryBitArray = new BitArray(fakeBooleans);
            this.dieRecoveryMock.Setup(d => d.GetMaskBits()).Returns(fakeDieRecoveryBitArray);
            this.DieRecoveryIncoming_ = this.dieRecoveryMock.Object;
            this.InitializeVoltageTargets();
            this.VerifyFeatureSwitchSettings();
            this.VerifyRecoveryMode();
            this.SetIncomingMask();
            this.SetInitialMask();

            Assert.IsTrue(expected.Xor(this.InitialSearchMask_).OfType<bool>().All(e => !e));
            this.dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Empty initial mask bits from parameter in InputOutput mode.
        /// </summary>
        [TestMethod]
        public void SetInitialMask_InputOutputEmptyInitial_Pass()
        {
            bool[] expectedBooleans = { true, true, true, false, true, true, true, true };
            var expected = new BitArray(expectedBooleans);
            this.InitialMaskBits = string.Empty;
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.StartVoltages = "0.5";
            this.dieRecoveryMock.Setup(d => d.GetMaskBits()).Returns(expected);
            this.DieRecoveryIncoming_ = this.dieRecoveryMock.Object;
            this.InitializeVoltageTargets();
            this.VerifyFeatureSwitchSettings();
            this.VerifyRecoveryMode();
            this.SetIncomingMask();
            this.SetInitialMask();

            Assert.IsTrue(expected.Xor(this.InitialSearchMask_).OfType<bool>().All(e => !e));
            this.dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Empty initial mask bits from parameter in Input mode.
        /// </summary>
        [TestMethod]
        public void SetInitialMask_InputEmptyInitial_Pass()
        {
            bool[] expectedBooleans = { true, false, true, true, true, true, true, true };
            var expected = new BitArray(expectedBooleans);
            this.InitialMaskBits = string.Empty;
            this.ForwardingMode = ForwardingModes.Input;
            this.StartVoltages = "0.5";
            this.dieRecoveryMock.Setup(d => d.GetMaskBits()).Returns(expected);
            this.DieRecoveryIncoming_ = this.dieRecoveryMock.Object;
            this.InitializeVoltageTargets();
            this.VerifyFeatureSwitchSettings();
            this.VerifyRecoveryMode();
            this.SetIncomingMask();
            this.SetInitialMask();

            Assert.IsTrue(expected.Xor(this.InitialSearchMask_).OfType<bool>().All(e => !e));
            this.dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Empty initial mask bits from parameter in Output mode.
        /// </summary>
        [TestMethod]
        public void SetInitialMask_OutputEmptyInitial_Pass()
        {
            bool[] expectedBooleans = { false, false, false, false, false, false, false, false };
            var expected = new BitArray(expectedBooleans);
            this.InitialMaskBits = string.Empty;
            this.VoltageTargets = "1,2,3,4,5,6,7,8";
            this.ForwardingMode = ForwardingModes.Output;
            this.TestMode = TestModes.MultiVmin;
            this.PinMap = "P1,P2,P3,P4,P5,P6,P7,P8";
            this.RecoveryTrackingIncoming = "T1,T2,T3,T4,T5,T6,T7,T8";
            this.RecoveryTrackingOutgoing = "T1,T2,T3,T4,T5,T6,T7,T8";
            this.StartVoltages = "0.5";
            var decoder = new Mock<IPinMapDecoder>(MockBehavior.Strict);
            decoder.Setup(o => o.NumberOfTrackerElements).Returns(8);
            this.pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { decoder.Object });

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetIncomingMask();
            this.SetInitialMask();
            Assert.IsTrue(expected.Xor(this.InitialSearchMask_).OfType<bool>().All(e => !e));
        }

        /// <summary>
        /// Apply Mask from mask.
        /// </summary>
        [TestMethod]
        public void ApplyMask_MaskSomePins_Pass()
        {
            bool[] expectedBooleans = { true, false, true, false, true, true, true, true };
            var bitArray = new BitArray(expectedBooleans);
            var plistMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.PinMap = "SomeMap";
            this.InitialMaskBits = "10101111";
            this.StartVoltages = "0.5";
            this.pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, false, true, false, true, true, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            this.pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, false, true, false, true, true, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            this.PinMap_ = this.pinMapMock.Object;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetIncomingMask();
            this.SetInitialMask();

            var temp = (IVminSearchExtensions)this;
            temp.ApplyMask(bitArray, plistMock.Object);
            this.pinMapMock.VerifyAll();
            plistMock.VerifyAll();
            this.pinMapMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Apply Mask from mask.
        /// </summary>
        [TestMethod]
        public void ApplyMask_ParameterAndPinMap_Pass()
        {
            this.MaskPins = "Pin1";
            bool[] expectedBooleans = { true, false, true, false, true, true, true, true };
            var bitArray = new BitArray(expectedBooleans);
            var plistMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.PinMap = "SomeMap";
            this.InitialMaskBits = "10101111";
            this.StartVoltages = "0.5";
            this.pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, false, true, false, true, true, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string> { "Pin1" }));
            this.pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, false, true, false, true, true, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            this.PinMap_ = this.pinMapMock.Object;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetIncomingMask();
            this.SetInitialMask();

            var temp = (IVminSearchExtensions)this;
            temp.ApplyMask(bitArray, plistMock.Object);
            this.pinMapMock.VerifyAll();
            plistMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
            this.pinMapMock.VerifyAll();
        }

        /// <summary>
        /// Covering different switches.
        /// </summary>
        [TestMethod]
        public void VerifyFeatureSwitchSettings_Pass()
        {
            this.FeatureSwitchSettings = "DUMMY";
            this.FivrCondition = string.Empty;
            this.Patlist = string.Empty;
            this.LogLevel = PrimeLogLevel.DISABLED;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            Assert.IsFalse(this.Switches_.DisableMaskedTargets);
            Assert.IsFalse(this.Switches_.DisablePairs);
            this.FeatureSwitchSettings = "disable_masked_targets,fivr_mode_on,disable_pairs";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;

            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Loose);
            var voltageMock = new Mock<IFivrCondition>(MockBehavior.Loose);
            voltageServiceMock.Setup(v => v.CreateFivrForCondition(It.IsAny<string>(), It.IsAny<string>())).Returns(voltageMock.Object);
            Services.VoltageService = voltageServiceMock.Object;
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(u => u.GetStringValue("DLVR", "pins")).Returns("DLVR_IN");
            Prime.Services.UserVarService = userVarServiceMock.Object;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            Assert.IsTrue(this.Switches_.DisableMaskedTargets);
            Assert.IsTrue(this.Switches_.DisablePairs);
            voltageMock.VerifyAll();
        }

        /// <summary>
        /// Disabling cores.
        /// </summary>
        [TestMethod]
        public void ApplyPreSearchSetup_DisableCores_Pass()
        {
            this.Patlist = "SomePatlist";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.ForwardingMode = ForwardingModes.None;
            this.InitialMaskBits = "01";
            this.VoltageTargets = "CORE1,CORE2";
            this.FeatureSwitchSettings = "disable_masked_targets";
            this.PinMap = "CORE1_NOA,CORE2_NOA";
            this.TestMode = TestModes.MultiVmin;
            this.StartVoltages = "0.5";
            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            var expectedBitArray = new BitArray(new[] { false, true });
            this.pinMapMock.Setup(p => p.ApplyPatConfig(It.Is<BitArray>(b => b.Xor(expectedBitArray).OfType<bool>().All(e => !e)), this.Patlist));
            this.PinMap_ = this.pinMapMock.Object;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetIncomingMask();
            ((IVminSearchExtensions)this).ApplyPreSearchSetup(this.Patlist);
            this.pinMapMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Disabling cores and slices using pairs.
        /// </summary>
        [TestMethod]
        public void ApplyPreSearchSetup_DisablePairs_Pass()
        {
            this.Patlist = "SomePatlist";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.ForwardingMode = ForwardingModes.None;
            this.InitialMaskBits = "1000";
            this.VoltageTargets = "CCF";
            this.FeatureSwitchSettings = "disable_masked_targets,disable_pairs";
            this.PinMap = "CORE0_NOA,CORE1_NOA,CORE2_NOA,CORE3_NOA";
            this.StartVoltages = "0.5";
            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            var expectedBitArray = new BitArray(new[] { true, true, false, false });
            this.pinMapMock.Setup(p => p.ApplyPatConfig(It.Is<BitArray>(b => b.Xor(expectedBitArray).OfType<bool>().All(e => !e)), this.Patlist));
            this.PinMap_ = this.pinMapMock.Object;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetIncomingMask();
            ((IVminSearchExtensions)this).ApplyPreSearchSetup(this.Patlist);
            this.pinMapMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Disabling cores and slices using quadruplets.
        /// </summary>
        [TestMethod]
        public void ApplyPreSearchSetup_DisableQuadruples_Pass()
        {
            this.Patlist = "SomePatlist";
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
            this.ForwardingMode = ForwardingModes.None;
            this.InitialMaskBits = "10000000";
            this.VoltageTargets = "CCF";
            this.FeatureSwitchSettings = "disable_masked_targets,disable_quadruplets";
            this.PinMap = "CORE0_NOA,CORE1_NOA,CORE2_NOA,CORE3_NOA,CORE4_NOA,CORE5_NOA,CORE6_NOA,CORE7_NOA";
            this.StartVoltages = "0.5";
            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            var expectedBitArray = new BitArray(new[] { true, true, true, true, false, false, false, false });
            this.pinMapMock.Setup(p => p.ApplyPatConfig(It.Is<BitArray>(b => b.Xor(expectedBitArray).OfType<bool>().All(e => !e)), this.Patlist));
            this.PinMap_ = this.pinMapMock.Object;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetIncomingMask();
            ((IVminSearchExtensions)this).ApplyPreSearchSetup(this.Patlist);
            this.pinMapMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// GetStartVoltageValues no forwarding.
        /// </summary>
        [TestMethod]
        public void GetStartVoltageValues_NoForwarding_Pass()
        {
            this.Patlist = "SomePatlist";
            this.LogLevel = PrimeLogLevel.DISABLED;
            this.ForwardingMode = ForwardingModes.None;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            var voltageKeys = new List<string> { "1.2", "1.0" };
            var result = this.CalculateStartVoltageValues(voltageKeys);
            var expected = new List<double> { 1.2 };
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.IsTrue(Math.Abs(expected[i] - result[i]) <= double.Epsilon);
            }
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetLowerStartVoltageValues_Pass()
        {
            this.Patlist = "SomePatlist";
            this.LogLevel = PrimeLogLevel.DISABLED;
            this.ForwardingMode = ForwardingModes.None;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            var voltageKeys = new List<string> { "1.2", "1.0" };
            var result = ((IVminSearchExtensions)this).GetLowerStartVoltageValues(voltageKeys);
            var expected = new List<double> { 1.2 };
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.IsTrue(Math.Abs(expected[i] - result[i]) <= double.Epsilon);
            }
        }

        /// <summary>
        /// GetStartVoltageValues no forwarding.
        /// </summary>
        [TestMethod]
        public void GetStartVoltageValues_Forwarding_Pass()
        {
            this.Patlist = "SomePatlist";
            this.LogLevel = PrimeLogLevel.DISABLED;
            this.ForwardingMode = ForwardingModes.Input;
            this.CornerIdentifiers = "C1,C2,C3";

            var voltageKeys = new List<string> { "1.2", "1.0", "0.5" };
            var startingVoltages = voltageKeys.Select(v => v.ToDouble()).ToList();
            var forwardingKeys = new List<string> { "1.3", "1.0", "0.6" };
            this.vminForwardingMock.Setup(v => v.GetStartingVoltage(startingVoltages[0])).Returns(1.3);
            this.vminForwardingMock.Setup(v => v.GetStartingVoltage(startingVoltages[1])).Returns(1.0);
            this.vminForwardingMock.Setup(v => v.GetStartingVoltage(startingVoltages[2])).Returns(0.6);

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();

            var result = this.CalculateStartVoltageValues(voltageKeys);
            Assert.AreEqual(1.3, result[0]);
            this.vminForwardingMock.VerifyAll();
        }

        /// <summary>
        /// ApplySearchVoltage no dlvr converter.
        /// </summary>
        [TestMethod]
        public void ApplySearchVoltage_NoDlvr_Pass()
        {
            var voltages = new List<double> { 1.2, 1.0, 0.5 };
            this.VoltagesOffset = "0.02";
            var expectedVoltages = new List<double> { 1.22, 1.02, 0.520 };
            var voltageMock = new Mock<IFivrDomains>(MockBehavior.Strict);
            voltageMock.Setup(v => v.Apply(expectedVoltages));

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            ((IVminSearchExtensions)this).ApplySearchVoltage(voltageMock.Object, voltages);
            voltageMock.VerifyAll();
        }

        /// <summary>
        /// ApplySearchVoltage no dlvr converter.
        /// </summary>
        [TestMethod]
        public void ApplySearchVoltage_OffsetFromUserVar_Pass()
        {
            var voltages = new List<double> { 1.2, 1.0, 0.5 };
            this.VoltagesOffset = "SomeUserVar";
            var expectedVoltages = new List<double> { 1.23, 1.03, 0.530 };
            var voltageMock = new Mock<IFivrDomains>(MockBehavior.Strict);
            voltageMock.Setup(v => v.Apply(expectedVoltages));
            var useVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            useVarServiceMock.Setup(u => u.GetDoubleValue(this.VoltagesOffset.ToString())).Returns(0.03);
            useVarServiceMock.Setup(u => u.Exists(this.VoltagesOffset.ToString())).Returns(true);
            Prime.Services.UserVarService = useVarServiceMock.Object;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            ((IVminSearchExtensions)this).ApplySearchVoltage(voltageMock.Object, voltages);
            voltageMock.VerifyAll();
        }

        /// <summary>
        /// ApplySearchVoltage with dlvr converter.
        /// </summary>
        [TestMethod]
        public void ApplySearchVoltage_Dlvr_Pass()
        {
            this.Patlist = "SomePlist";
            this.FivrCondition = "SomeFivrCondition";
            this.FeatureSwitchSettings = "fivr_mode_on";
            this.VoltageConverter = "--railconfigurations dlvr pinattributes powerswitch --overrides=1:1.2,2:0.3,3:0.3";
            this.LevelsTc = "SomeLevels";
            this.VoltageTargets = "SA";
            this.FivrCondition = "NOM";

            var voltageMock = new Mock<IFivrDomainsAndConditionWithRails>(MockBehavior.Strict);
            voltageMock.Setup(o => o.ApplyConditionWithOverride(new Dictionary<string, double> { { "1", 1.2 }, { "2", 0.3 }, { "3", 0.3 } }));
            voltageMock.Setup(o => o.Apply(new List<double> { 1 }));
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            voltageServiceMock.Setup(o => o.CreateFivrDomainsAndConditionWithRails(new List<string> { "SA" }, "NOM", this.Patlist, new List<string> { "dlvr", "pinattributes", "powerswitch" })).Returns(voltageMock.Object);
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            var patConfigSetPointHandleMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            patConfigServiceMock.Setup(p => p.GetSetPointHandle("Module", "Group", this.Patlist)).Returns(patConfigSetPointHandleMock.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            ((IVminSearchExtensions)this).GetSearchVoltageObject(this.VoltageTargets.ToList(), this.Patlist);
            ((IVminSearchExtensions)this).ApplyPreExecuteSetup(this.Patlist);
            ((IVminSearchExtensions)this).ApplyInitialVoltage(voltageMock.Object);
            ((IVminSearchExtensions)this).ApplySearchVoltage(voltageMock.Object, new List<double> { 1.0 });
            voltageMock.VerifyAll();
            voltageServiceMock.VerifyAll();
        }

        /// <summary>
        /// PostProcessSearchPassResult.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_Output_Pass()
        {
            this.ForwardingMode = ForwardingModes.Output;
            this.CornerIdentifiers = "C1";
            this.StartVoltages = "0.5";
            this.EndVoltageLimits = "1.8";

            var voltage = new List<double> { 1.7 }; // expected 0.5 offset.
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());

            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchStateValues = new SearchStateValues
            {
                Voltages = voltage,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(1, false),
                ExecutionCount = 1,
                PerTargetIncrements = new List<uint> { 1 },
                PerPointData = new List<SearchPointData> { new SearchPointData(new List<double> { 0.5 }, new SearchPointData.PatternData("pattern", 1, 1)) },
            };

            var searchPoints = new List<SearchResultData> { new SearchResultData(searchStateValues, true, searchIdentifiers) };

            this.vminForwardingMock.Setup(v => v.StoreVminResult(voltage[0])).Returns(true);
            var tuple0 = new Tuple<string, int, IVminForwardingCorner>("C0", 1, this.vminForwardingMock.Object);
            this.VminForwarding_ = new List<Tuple<string, int, IVminForwardingCorner>> { tuple0 };
            this.vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);
            this.scoreBoardLoggerMock.Setup(o => o.ProcessFailData(It.IsAny<List<string>>()));
            this.strgvalFormatMock.Setup(o => o.SetData("1.700|0.500|1.800|1"));
            this.strgvalFormatMock.Setup(o => o.SetData("C1:1:1.500"));
            this.strgvalFormatMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgvalFormatMock.Object));
            var functionalTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            ((IVminSearchExtensions)this).GetBypassPort();
            ((IVminSearchExtensions)this).ApplyPreExecuteSetup(this.Patlist);
            ((IVminSearchExtensions)this).ProcessPlistResults(true, functionalTestMock.Object);
            ((IVminSearchExtensions)this).HasToRepeatSearch(searchPoints);
            var result = ((IVminSearchExtensions)this).PostProcessSearchResults(searchPoints);
            Assert.AreEqual(1, result);
            this.vminForwardingMock.VerifyAll();
            this.strgvalFormatMock.VerifyAll();
        }

        /// <summary>
        /// PostProcessSearchFailResult.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_Output_Fail()
        {
            this.ForwardingMode = ForwardingModes.Output;
            this.CornerIdentifiers = "C1";
            this.StartVoltages = "0.5";
            this.EndVoltageLimits = "1.8";
            this.Patlist = "SomePatlist";
            this.LevelsTc = "SomeLevels";
            this.TimingsTc = "SomeTimings";
            this.PrePlist = string.Empty;

            this.plistObjectMock.Setup(o => o.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            this.plistServiceMock.Setup(o => o.GetPlistObject(this.Patlist)).Returns(this.plistObjectMock.Object);
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var captureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            functionalServiceMock
                .Setup(o => o.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 1, string.Empty))
                .Returns(captureTestMock.Object);
            Prime.Services.FunctionalService = functionalServiceMock.Object;

            var voltage = new List<double> { 1.7 };
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());
            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchStateValues = new SearchStateValues
            {
                Voltages = voltage,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(1, false),
                ExecutionCount = 1,
                PerTargetIncrements = new List<uint> { 1 },
                PerPointData = new List<SearchPointData> { new SearchPointData(new List<double> { 0.5 }, new SearchPointData.PatternData("pattern", 1, 1)) },
            };

            var searchPoints = new List<SearchResultData> { new SearchResultData(searchStateValues, false, searchIdentifiers) };

            this.vminForwardingMock.Setup(v => v.StoreVminResult(voltage[0]));
            var tuple0 = new Tuple<string, int, IVminForwardingCorner>("C0", 1, this.vminForwardingMock.Object);
            this.VminForwarding_ = new List<Tuple<string, int, IVminForwardingCorner>> { tuple0 };
            this.vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);
            this.scoreBoardLoggerMock.Setup(o => o.ProcessFailData(It.IsAny<List<string>>()));
            this.strgvalFormatMock.Setup(o => o.SetData("1.700|0.500|1.800|1"));
            this.strgvalFormatMock.Setup(o => o.SetData("C1:1:1.500"));
            this.strgvalFormatMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgvalFormatMock.Object));
            var functionalTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);

            ((IVminSearchExtensions)this).GetFunctionalTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.PrePlist);
            this.CustomVerify();
            ((IVminSearchExtensions)this).GetBypassPort();
            ((IVminSearchExtensions)this).ApplyPreExecuteSetup(this.Patlist);
            ((IVminSearchExtensions)this).ProcessPlistResults(false, functionalTestMock.Object);
            ((IVminSearchExtensions)this).HasToRepeatSearch(searchPoints);
            var result = ((IVminSearchExtensions)this).PostProcessSearchResults(searchPoints);
            Assert.AreEqual(0, result);
            this.vminForwardingMock.VerifyAll();
            this.strgvalFormatMock.VerifyAll();
        }

        /// <summary>
        /// PostProcessSearchResults. Passing search and rules.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_OutputMultipleCorners_Pass()
        {
            this.ForwardingMode = ForwardingModes.Output;
            this.CornerIdentifiers = "C1,C2";
            this.StartVoltages = "0.5";
            this.EndVoltageLimits = "1.8";
            this.RecoveryOptions = "0000";
            this.RecoveryTrackingIncoming = "SomeTracking";
            this.RecoveryTrackingOutgoing = "SomeTracking";
            this.PinMap = "C0_MAP,C1_MAP,C2_MAP,C3_MAP";
            this.InitialMaskBits = "0000";

            var functionalTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);
            var voltage = new List<double> { 1.7 };
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());

            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchStateValues = new SearchStateValues
            {
                Voltages = voltage,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(1, false),
                ExecutionCount = 1,
                PerTargetIncrements = new List<uint> { 1 },
                PerPointData = new List<SearchPointData> { new SearchPointData(new List<double> { 0.5 }, new SearchPointData.PatternData("pattern", 1, 1)) },
            };

            var searchPoints = new List<SearchResultData> { new SearchResultData(searchStateValues, true, searchIdentifiers) };

            this.dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(4, false), null, new BitArray(4, false), UpdateMode.Merge, true)).Returns(true);
            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns("0000".ToBitArray());
            this.vminForwardingMock.Setup(v => v.StoreVminResult(voltage[0])).Returns(true);
            this.pinMapMock.Setup(p => p.DecodeFailure(functionalTestMock.Object, null)).Returns(new BitArray(new[] { false, false, false, false }));
            this.vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);
            this.scoreBoardLoggerMock.Setup(o => o.ProcessFailData(It.IsAny<List<string>>()));
            this.strgvalFormatMock.Setup(o => o.SetData("1.700|0.500|1.800|1"));
            this.strgvalFormatMock.Setup(o => o.SetData("C1:1:1.500_C2:1:1.500"));
            this.strgvalFormatMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgvalFormatMock.Object));

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetIncomingMask();
            ((IVminSearchExtensions)this).ProcessPlistResults(true, functionalTestMock.Object);
            ((IVminSearchExtensions)this).HasToRepeatSearch(searchPoints);
            ((IVminSearchExtensions)this).HasToContinueToNextSearch(searchPoints, functionalTestMock.Object);
            var processSearchResults = ((IVminSearchExtensions)this).PostProcessSearchResults(searchPoints);
            Assert.AreEqual(1, processSearchResults);
            this.pinMapMock.VerifyAll();
            this.vminForwardingMock.VerifyAll();
            this.strgvalFormatMock.VerifyAll();
        }

        /// <summary>
        /// PostProcessSearchResults. RecoveryPort: Failing search and passing rules.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_RecoveryPort_Port3()
        {
            this.ForwardingMode = ForwardingModes.Output;
            this.CornerIdentifiers = "CCF";
            this.StartVoltages = "0.5";
            this.EndVoltageLimits = "1.8";
            this.RecoveryOptions = "0000,1100,0011";
            this.RecoveryTrackingIncoming = "SomeTracking";
            this.RecoveryTrackingOutgoing = "SomeTracking";
            this.PinMap = "C0_MAP,C1_MAP,C2_MAP,C3_MAP";
            this.InitialMaskBits = "0000";
            this.RecoveryMode = RecoveryModes.RecoveryPort;
            this.Patlist = "SomePatlist";
            this.LevelsTc = "SomeLevels";
            this.TimingsTc = "SomeTimings";
            this.PrePlist = string.Empty;

            this.plistObjectMock.Setup(o => o.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            this.plistServiceMock.Setup(o => o.GetPlistObject(this.Patlist)).Returns(this.plistObjectMock.Object);
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var captureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            functionalServiceMock
                .Setup(o => o.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 1, string.Empty))
                .Returns(captureTestMock.Object);
            Prime.Services.FunctionalService = functionalServiceMock.Object;

            var functionalTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);
            var voltage = new List<double> { 1.7 };
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());

            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchStateValues = new SearchStateValues
            {
                Voltages = voltage,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(1, false),
                ExecutionCount = 1,
                PerTargetIncrements = new List<uint> { 1 },
                PerPointData = new List<SearchPointData> { new SearchPointData(new List<double> { 0.5 }, new SearchPointData.PatternData("pattern", 1, 1)) },
            };

            var searchPoints = new List<SearchResultData> { new SearchResultData(searchStateValues, false, searchIdentifiers) };

            this.pinMapMock.Setup(p => p.DecodeFailure(functionalTestMock.Object, null)).Returns(new BitArray(new[] { true, false, false, false }));
            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, false));
            this.dieRecoveryMock.Setup(d => d.UpdateTrackingStructure(new BitArray(new[] { true, true, false, false }), new BitArray(new[] { false, false, false, false }), new BitArray(new[] { true, false, false, false }), UpdateMode.Merge, true)).Returns(true);
            this.vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);
            this.scoreBoardLoggerMock.Setup(o => o.ProcessFailData(It.IsAny<List<string>>()));
            this.strgvalFormatMock.Setup(o => o.SetData("1.700|0.500|1.800|1"));
            this.strgvalFormatMock.Setup(o => o.SetData("CCF:1:1.500"));
            this.strgvalFormatMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgvalFormatMock.Object));

            ((IVminSearchExtensions)this).GetFunctionalTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.PrePlist);
            this.CustomVerify();
            this.SetDieRecoveryTrackers();
            this.SetIncomingMask();
            ((IVminSearchExtensions)this).ProcessPlistResults(false, functionalTestMock.Object);
            ((IVminSearchExtensions)this).HasToRepeatSearch(searchPoints);
            ((IVminSearchExtensions)this).HasToContinueToNextSearch(searchPoints, functionalTestMock.Object);
            var processSearchResults = ((IVminSearchExtensions)this).PostProcessSearchResults(searchPoints);
            Assert.AreEqual(3, processSearchResults);
            this.pinMapMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
            this.strgvalFormatMock.VerifyAll();
        }

        /// <summary>
        /// PostProcessSearchResults. Default: Failing search and passing rules.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_Default_Port0()
        {
            this.ForwardingMode = ForwardingModes.Output;
            this.CornerIdentifiers = "CCF";
            this.StartVoltages = "0.5";
            this.EndVoltageLimits = "1.8";
            this.RecoveryOptions = "0000";
            this.RecoveryTrackingIncoming = "SomeTracking";
            this.RecoveryTrackingOutgoing = "SomeTracking";
            this.PinMap = "C0_MAP,C1_MAP,C2_MAP,C3_MAP";
            this.InitialMaskBits = "0000";
            this.RecoveryMode = RecoveryModes.Default;
            this.Patlist = "SomePatlist";
            this.LevelsTc = "SomeLevels";
            this.TimingsTc = "SomeTimings";
            this.PrePlist = string.Empty;

            this.plistObjectMock.Setup(o => o.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            this.plistServiceMock.Setup(o => o.GetPlistObject(this.Patlist)).Returns(this.plistObjectMock.Object);
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var captureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            functionalServiceMock
                .Setup(o => o.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 1, string.Empty))
                .Returns(captureTestMock.Object);
            Prime.Services.FunctionalService = functionalServiceMock.Object;

            var voltage = new List<double> { -9999 };
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());

            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchStateValues = new SearchStateValues
            {
                Voltages = voltage,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(1, false),
                ExecutionCount = 1,
                PerTargetIncrements = new List<uint> { 1 },
                PerPointData = new List<SearchPointData> { new SearchPointData(new List<double> { 0.5 }, new SearchPointData.PatternData("pattern", 1, 1)) },
            };

            var searchPoints = new List<SearchResultData> { new SearchResultData(searchStateValues, false, searchIdentifiers) };

            var functionalTestMock = captureTestMock.As<IFunctionalTest>();
            this.vminForwardingMock.Setup(v => v.StoreVminResult(It.IsAny<double>()));
            this.pinMapMock.Setup(p => p.DecodeFailure(functionalTestMock.Object, null)).Returns(new BitArray(new[] { true, false, false, false }));
            this.vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);
            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns("0000".ToBitArray());

            this.scoreBoardLoggerMock.Setup(o => o.ProcessFailData(It.IsAny<List<string>>()));
            this.strgvalFormatMock.Setup(o => o.SetData("-9999|0.500|1.800|1"));
            this.strgvalFormatMock.Setup(o => o.SetData("CCF:1:1.500"));
            this.strgvalFormatMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgvalFormatMock.Object));

            ((IVminSearchExtensions)this).GetFunctionalTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.PrePlist);
            this.CustomVerify();
            this.SetIncomingMask();
            ((IVminSearchExtensions)this).ProcessPlistResults(false, functionalTestMock.Object);
            ((IVminSearchExtensions)this).HasToRepeatSearch(searchPoints);
            ((IVminSearchExtensions)this).HasToContinueToNextSearch(searchPoints, functionalTestMock.Object);
            var processSearchResults = ((IVminSearchExtensions)this).PostProcessSearchResults(searchPoints);
            Assert.AreEqual(0, processSearchResults);
            this.pinMapMock.VerifyAll();
            this.vminForwardingMock.VerifyAll();
            this.strgvalFormatMock.VerifyAll();
        }

        /// <summary>
        /// PostInstance_Empty_pass.
        /// </summary>
        [TestMethod]
        public void PostInstance_Empty_pass()
        {
            this.ExecutePostInstance();
        }

        /// <summary>
        /// PostInstance_PostPlist_Pass.
        /// </summary>
        [TestMethod]
        public void PostInstance_PostPlist_Pass()
        {
            this.PostPlist = "AnotherPlist";
            this.LevelsTc = "SomeLevels";
            this.TimingsTc = "SomeTiming";
            this.Patlist = "SomePatlist";
            this.FeatureSwitchSettings = string.Empty;

            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var test = new Mock<INoCaptureTest>(MockBehavior.Strict);
            test.Setup(t => t.Execute()).Returns(true);
            functionalServiceMock.Setup(f => f.CreateNoCaptureTest(this.PostPlist, this.LevelsTc, this.TimingsTc, string.Empty)).Returns(test.Object);
            Prime.Services.FunctionalService = functionalServiceMock.Object;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.ExecutePostInstance();
        }

        /// <summary>
        /// GetFunctionalTest_StopFirstFail_Pass.
        /// </summary>
        [TestMethod]
        public void GetFunctionalTest_StopFirstFail_Pass()
        {
            this.LevelsTc = "SomeLevels";
            this.TimingsTc = "SomeTiming";
            this.Patlist = "SomePatlist";
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var captureFailureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureFailureTestMock.Setup(c => c.EnableStartPatternOnFirstFail());
            functionalServiceMock.Setup(f => f.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.FailCaptureCount, this.PrePlist)).Returns(captureFailureTestMock.Object);
            Prime.Services.FunctionalService = functionalServiceMock.Object;

            var temp = (IVminSearchExtensions)this;
            temp.GetFunctionalTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.PrePlist);
            functionalServiceMock.VerifyAll();
            captureFailureTestMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetFunctionalTest_ReturnOnStickyError_Pass()
        {
            this.LevelsTc = "SomeLevels";
            this.TimingsTc = "SomeTiming";
            this.Patlist = "SomePatlist";
            this.FeatureSwitchSettings = "return_on_global_sticky_error";
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var captureFailureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureFailureTestMock.Setup(c => c.EnableStartPatternOnFirstFail());
            functionalServiceMock.Setup(f => f.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 9999, this.PrePlist)).Returns(captureFailureTestMock.Object);
            Prime.Services.FunctionalService = functionalServiceMock.Object;
            this.plistObjectMock.Setup(o => o.SetOption("ReturnOn", "GlobalStickyError"));

            var temp = (IVminSearchExtensions)this;
            temp.GetFunctionalTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.PrePlist);
            functionalServiceMock.VerifyAll();
            captureFailureTestMock.VerifyAll();
            this.plistObjectMock.VerifyAll();
            this.plistObjectMock.VerifyAll();
        }

        /// <summary>
        /// GetFunctionalTest_MaskPins_Pass.
        /// </summary>
        [TestMethod]
        public void GetFunctionalTest_MaskPins_Pass()
        {
            this.LevelsTc = "SomeLevels";
            this.TimingsTc = "SomeTiming";
            this.Patlist = "SomePatlist";
            this.MaskPins = "Pin1,Pin2";
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var captureFailureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureFailureTestMock.Setup(c => c.EnableStartPatternOnFirstFail());
            captureFailureTestMock.Setup(c => c.SetPinMask(new List<string> { "Pin1", "Pin2" }));
            functionalServiceMock.Setup(f => f.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.FailCaptureCount, this.PrePlist)).Returns(captureFailureTestMock.Object);
            Prime.Services.FunctionalService = functionalServiceMock.Object;

            var temp = (IVminSearchExtensions)this;
            temp.GetFunctionalTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.PrePlist);
            functionalServiceMock.VerifyAll();
            captureFailureTestMock.VerifyAll();
        }

        /// <summary>
        /// GetFunctionalTest_TriggerMap_Pass.
        /// </summary>
        [TestMethod]
        public void GetFunctionalTest_TriggerMap_Pass()
        {
            this.LevelsTc = "SomeLevels";
            this.TimingsTc = "SomeTiming";
            this.Patlist = "SomePatlist";
            this.TriggerMap = "TriggerMap";
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var captureFailureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureFailureTestMock.Setup(c => c.EnableStartPatternOnFirstFail());
            captureFailureTestMock.Setup(c => c.SetTriggerMap("TriggerMap"));
            functionalServiceMock.Setup(f => f.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.FailCaptureCount, this.PrePlist)).Returns(captureFailureTestMock.Object);
            Prime.Services.FunctionalService = functionalServiceMock.Object;

            var temp = (IVminSearchExtensions)this;
            temp.GetFunctionalTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.PrePlist);
            functionalServiceMock.VerifyAll();
            captureFailureTestMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetSearchVoltageObject_MissingTriggerMap_Fail()
        {
            this.LevelsTc = "SomeLevels";
            this.TimingsTc = "SomeTiming";
            this.Patlist = "SomePatlist";
            this.TriggerLevelsCondition = "SomeVbumpLevels";
            this.TriggerMap = string.Empty;
            this.VoltageTargets = "PinA";

            var ex = Assert.ThrowsException<ArgumentException>(() => ((IVminSearchExtensions)this).GetSearchVoltageObject(new List<string> { "PinA" }, this.Patlist));
            Assert.AreEqual("VminTC.dll.Prime.TestMethods.VminSearch.IVminSearchExtensions.GetSearchVoltageObject: use of TriggerLevelsCondition requires to use TriggerMap", ex.Message);
        }

        /// <summary>
        /// GetFunctionalTest_StopFirstFail_Pass.
        /// </summary>
        [TestMethod]
        public void GetFunctionalTest_CtvPins_Pass()
        {
            this.LevelsTc = "SomeLevels";
            this.TimingsTc = "SomeTiming";
            this.Patlist = "SomePatlist";
            this.CtvPins = "TDO";
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var captureFailureTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            captureFailureTestMock.Setup(c => c.EnableStartPatternOnFirstFail());
            functionalServiceMock.Setup(f => f.CreateCaptureFailureAndCtvPerPinTest(this.Patlist, this.LevelsTc, this.TimingsTc, new List<string> { this.CtvPins }, this.FailCaptureCount, this.PrePlist)).Returns(captureFailureTestMock.Object);
            Prime.Services.FunctionalService = functionalServiceMock.Object;

            var temp = (IVminSearchExtensions)this;
            temp.GetFunctionalTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.PrePlist);
            functionalServiceMock.VerifyAll();
            captureFailureTestMock.VerifyAll();
        }

        /// <summary>
        /// GetSearchVoltageObject_FivrMode_Pass.
        /// </summary>
        [TestMethod]
        public void GetSearchVoltageObject_FivrMode_Pass()
        {
            this.FeatureSwitchSettings = "fivr_mode_on";
            this.VoltageTargets = "Fivr1,Fivr2";
            this.Patlist = "somePatlist";

            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            var voltageObject = new Mock<IFivrDomainsAndCondition>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateFivrForDomainsAndCondition(new List<string> { "Fivr1", "Fivr2" }, "FivrCondition", "somePatlist")).Returns(voltageObject.Object);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            ((IVminSearchExtensions)this).GetSearchVoltageObject(this.VoltageTargets.ToList(), this.Patlist);
            voltageServiceMock.VerifyAll();
        }

        /// <summary>
        /// GetSearchVoltageObject_DPS_Pass.
        /// </summary>
        [TestMethod]
        public void GetSearchVoltageObject_DPS_Pass()
        {
            this.FeatureSwitchSettings = "fivr_mode_off";
            this.VoltageTargets = "Pin1,Pin2";
            this.Patlist = "somePatlist";
            this.LevelsTc = "someLevel";

            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("Pin1", "SomeMandatoryAttribute")).Returns("SomeValue");
            testCondition.Setup(t => t.GetPinAttributeValue("Pin2", "SomeMandatoryAttribute")).Returns("SomeValue");
            testConditionServiceMock.Setup(t => t.GetTestCondition(this.LevelsTc)).Returns(testCondition.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            var pin = new Mock<IPin>(MockBehavior.Strict);
            pin.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "SomeMandatoryAttribute" });
            pinServiceMock.Setup(p => p.Get("Pin1")).Returns(pin.Object);
            pinServiceMock.Setup(p => p.Get("Pin2")).Returns(pin.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            var voltageObject = new Mock<IVForcePinAttribute>(MockBehavior.Strict);

            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "Pin1", "Pin2" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(voltageObject.Object);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            ((IVminSearchExtensions)this).GetSearchVoltageObject(this.VoltageTargets.ToList(), this.Patlist);
            voltageServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
            testCondition.VerifyAll();
            pin.VerifyAll();
        }

        /// <summary>
        /// GetSearchVoltageObject_Trigger_Pass.
        /// </summary>
        [TestMethod]
        public void GetSearchVoltageObject_Trigger_Pass()
        {
            this.FeatureSwitchSettings = "fivr_mode_off";
            this.VoltageTargets = "Pin1,Pin2";
            this.Patlist = "somePatlist";
            this.LevelsTc = "someLevel";
            this.TriggerLevelsCondition = "someOtherLevel";
            this.TriggerMap = "someTriggerMap";

            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("Pin1", "FreeDriveTime")).Returns("1mS");
            testCondition.Setup(t => t.GetPinAttributeValue("Pin2", "FreeDriveTime")).Returns("1mS");
            testConditionServiceMock.Setup(t => t.GetTestCondition("someLevel")).Returns(testCondition.Object);
            testConditionServiceMock.Setup(t => t.GetTestCondition("someOtherLevel")).Returns(testCondition.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(o => o.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            Prime.Services.PinService = pinServiceMock.Object;
            pinServiceMock.Setup(o => o.Get("Pin1")).Returns(pinMock.Object);
            pinServiceMock.Setup(o => o.Get("Pin2")).Returns(pinMock.Object);

            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            var voltageObject = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinTestCondition(new List<string> { "Pin1", "Pin2" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>(), "someOtherLevel")).Returns(voltageObject.Object);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            ((IVminSearchExtensions)this).GetSearchVoltageObject(this.VoltageTargets.ToList(), this.Patlist);
            voltageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the code to enable trace_ctv mode and execute it.
        /// </summary>
        [TestMethod]
        public void TraceCtvMode_Disabled_Pass()
        {
            this.FeatureSwitchSettings = "DUMMY";
            this.LogLevel = PrimeLogLevel.DISABLED;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            Assert.IsFalse(this.Switches_.TraceCtv);
        }

        /// <summary>
        /// Test the code to enable trace_ctv mode and execute it.
        /// </summary>
        [TestMethod]
        public void TraceCtvMode_Enabled_NoCtvPinException()
        {
            this.FeatureSwitchSettings = "trace_ctv_on";
            this.LogLevel = PrimeLogLevel.DISABLED;
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => this.VerifyFeatureSwitchSettings());
            Assert.AreEqual("Parameter=[CtvPins] must be specified when FeatureSwitchSettings=[trace_ctv_on].", ex.Message);
        }

        /// <summary>
        /// Test the code to enable trace_ctv mode and execute it.
        /// </summary>
        [TestMethod]
        public void TraceCtvMode_FunctionalMode_Pass()
        {
            this.TestMode = TestModes.Functional;
            this.TraceCtvMode_Enabled_Base();
        }

        /// <summary>
        /// Test the code to enable trace_ctv mode and execute it.
        /// </summary>
        [TestMethod]
        public void TraceCtvMode_ScoreboardMode_Pass()
        {
            this.TestMode = TestModes.Scoreboard;
            this.TraceCtvMode_Enabled_Base();
        }

        /// <summary>
        /// Test the code to enable trace_ctv mode and execute it.
        /// </summary>
        private void TraceCtvMode_Enabled_Base()
        {
            this.FeatureSwitchSettings = "trace_ctv_on";
            this.CtvPins = "TDO";
            this.LogLevel = PrimeLogLevel.DISABLED;
            this.Patlist = "dummy_plist";
            this.LevelsTc = "dummy_levels";
            this.TimingsTc = "dummy_timings";
            this.FailCaptureCount = 100;
            this.PrePlist = string.Empty;

            var ctvByCycle1Mock = new Mock<ICtvPerCycle>(MockBehavior.Strict);
            ctvByCycle1Mock.Setup(o => o.GetDomainName()).Returns("LEG");
            ctvByCycle1Mock.Setup(o => o.GetPatternName()).Returns("PatternA");
            ctvByCycle1Mock.Setup(o => o.GetParentPlistName()).Returns("PListA");
            ctvByCycle1Mock.Setup(o => o.GetVectorAddress()).Returns(2);
            ctvByCycle1Mock.Setup(o => o.GetCycle()).Returns(2);
            ctvByCycle1Mock.Setup(o => o.GetTraceLogRegister1()).Returns(0);
            ctvByCycle1Mock.Setup(o => o.GetTraceLogCycle()).Returns(41000);
            ctvByCycle1Mock.Setup(o => o.GetBurstIndex()).Returns(1);
            ctvByCycle1Mock.Setup(o => o.GetBurstCycle()).Returns(15000);

            var functionalTestMock = new Mock<ICaptureFailureAndCtvPerCycleTest>(MockBehavior.Strict);
            functionalTestMock.Setup(o => o.GetCtvPerCycle()).Returns(new List<ICtvPerCycle> { ctvByCycle1Mock.Object }); // we're not trying to test the trace functionality, just its integration into vmin.
            functionalTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            if (this.TestMode == TestModes.Scoreboard)
            {
                funcServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerCycleTest(this.Patlist, this.LevelsTc, this.TimingsTc, It.Is<List<string>>(it => it.Count() == 1 && it[0] == "TDO"), this.FailCaptureCount, 1, this.PrePlist))
                    .Returns(functionalTestMock.Object);
            }
            else
            {
                funcServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerCycleTest(this.Patlist, this.LevelsTc, this.TimingsTc, It.Is<List<string>>(it => it.Count() == 1 && it[0] == "TDO"), this.FailCaptureCount, this.PrePlist))
                    .Returns(functionalTestMock.Object);
            }

            Prime.Services.FunctionalService = funcServiceMock.Object;

            var functionalTest = ((IVminSearchExtensions)this).GetFunctionalTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.PrePlist);
            this.CustomVerify();
            Assert.IsTrue(this.Switches_.TraceCtv);

            ((IVminSearchExtensions)this).ProcessPlistResults(true, functionalTest);
            funcServiceMock.VerifyAll();
            functionalTestMock.VerifyAll();
            ctvByCycle1Mock.VerifyAll();
        }
    }
}
