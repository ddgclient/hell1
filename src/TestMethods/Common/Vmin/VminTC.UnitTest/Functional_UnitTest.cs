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
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.BinMatrixService;
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
    using Prime.VoltageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class Functional_UnitTest : VminTC
    {
        private Mock<IFunctionalService> functionalServiceMock;
        private Mock<ICaptureFailureTest> captureFailureTest;
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<ISharedStorageService> sharedStorageMock;
        private Dictionary<string, string> sharedStorageValues;
        private Mock<ITestProgramService> testProgramServiceMock;
        private Mock<IDieRecoveryFactory> dieRecoveryFactoryMock;
        private Mock<IDieRecovery> dieRecoveryMock;
        private Mock<IPinMapFactory> pinMapFactoryMock;
        private Mock<IPinMap> pinMapMock;
        private Mock<IPlistService> plistServiceMock;
        private Mock<IPlistObject> plistObjectMock;
        private Mock<IPerformanceService> performanceServiceMock;

        /// <summary>
        /// Setup mocks.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.TestMode = TestModes.Functional;
            this.FeatureSwitchSettings = string.Empty;
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.FivrCondition = string.Empty;
            this.VoltageTargets = string.Empty;
            this.FailCaptureCount = 1;

            this.captureFailureTest = new Mock<ICaptureFailureTest>(MockBehavior.Default);
            this.functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Default);
            Prime.Services.FunctionalService = this.functionalServiceMock.Object;

            this.plistServiceMock = new Mock<IPlistService>(MockBehavior.Default);
            this.plistObjectMock = new Mock<IPlistObject>(MockBehavior.Default);
            this.plistServiceMock.Setup(o => o.GetPlistObject(It.IsAny<string>())).Returns(this.plistObjectMock.Object);
            Prime.Services.PlistService = this.plistServiceMock.Object;

            this.performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = this.performanceServiceMock.Object;

            this.MockSharedStorage();
            this.MockConsole();
            this.MockVoltage();
            this.MockDieRecovery();
            this.VerifyDts();
        }

        /// <summary>
        /// Validates mocks.
        /// </summary>
        [TestCleanup]
        public void CleanupTestMethod()
        {
            this.captureFailureTest.VerifyAll();
            this.functionalServiceMock.VerifyAll();
        }

        /// <summary>
        /// Basic functional test. Pass.
        /// </summary>
        [TestMethod]
        public void ExecuteFunctionalMode_Basic_Pass()
        {
            // Mock the functional test service.
            this.captureFailureTest.Setup(o => o.EnableStartPatternOnFirstFail());
            this.captureFailureTest.Setup(o => o.ApplyTestConditions());
            this.captureFailureTest.Setup(o => o.Reset());
            this.captureFailureTest.Setup(o => o.SetPinMask(new List<string>()));
            this.captureFailureTest.Setup(o => o.Execute()).Returns(true);
            this.captureFailureTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData>());

            this.functionalServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, this.PrePlist)).Returns(this.captureFailureTest.Object);

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetData("0.500|0.500|1.000|1"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                VoltageTargets = "VccPin",
                StartVoltages = "0.500",
                EndVoltageLimits = "1.0",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 1,

                // Custom Parameters.
                TestMode = VminTC.TestModes.Functional,
                CornerIdentifiers = "CLR_F3",
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Basic functional test. Fail.
        /// </summary>
        [TestMethod]
        public void ExecuteFunctionalMode_Basic_Fail()
        {
            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(5);

            this.captureFailureTest.Setup(o => o.EnableStartPatternOnFirstFail());
            this.captureFailureTest.Setup(o => o.ApplyTestConditions());
            this.captureFailureTest.Setup(o => o.Reset());
            this.captureFailureTest.Setup(o => o.SetPinMask(new List<string>()));
            this.captureFailureTest.Setup(o => o.Execute()).Returns(false);
            this.captureFailureTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object });
            this.captureFailureTest.Setup(o => o.DatalogFailure(1));

            this.functionalServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, this.PrePlist)).Returns(this.captureFailureTest.Object);

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("-9999|0.500|1.000|1"));
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.4)).Returns(0.5);

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                VoltageTargets = "VccPin",
                StartVoltages = "0.400",
                EndVoltageLimits = "1.0",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 1,

                // Custom Parameters.
                TestMode = VminTC.TestModes.Functional,
                CornerIdentifiers = "CLR_F3",
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Basic functional test. Recovery.
        /// </summary>
        [TestMethod]
        public void ExecuteFunctionalMode_Recovery_Port3()
        {
            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("g1234567");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            this.captureFailureTest.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            this.captureFailureTest.Setup(o => o.EnableStartPatternOnFirstFail());
            this.captureFailureTest.Setup(o => o.ApplyTestConditions());
            this.captureFailureTest.Setup(o => o.Reset());
            this.captureFailureTest.Setup(o => o.Execute()).Returns(false);
            this.captureFailureTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object });
            this.captureFailureTest.Setup(o => o.DatalogFailure(1));

            this.functionalServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 0, this.PrePlist)).Returns(this.captureFailureTest.Object);

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("-9999|0.500|1.000|1"));
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");

            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, false));
            this.dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(new[] { true, true, false, false }), new BitArray(4, false), new BitArray(new[] { true, false, false, false }), UpdateMode.Merge, true)).Returns(true);
            this.pinMapMock.Setup(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null)).Returns("1000".ToBitArray());
            var functionalTest = this.captureFailureTest.As<IFunctionalTest>().Object;
            this.pinMapMock.Setup(o => o.MaskPins(new BitArray(4, false), ref functionalTest, new List<string>()));
            this.pinMapMock.Setup(o => o.ModifyPlist(new BitArray(4, false), ref functionalTest));

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                VoltageTargets = "VccPin",
                StartVoltages = "0.500",
                EndVoltageLimits = "1.0",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 0,
                PatternNameMap = string.Empty,
                RecoveryMode = RecoveryModes.RecoveryPort,
                RecoveryTrackingIncoming = "T0,T2,T2,T3",
                RecoveryTrackingOutgoing = "T0,T2,T2,T3",
                PinMap = "P0,P1,P2,P3",
                RecoveryOptions = "0000,0011,1100",
                ScoreboardBaseNumber = string.Empty,
                ScoreboardMaxFails = 0,
                TestMode = VminTC.TestModes.Functional,
                CornerIdentifiers = "CLR_F3",
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(3, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            this.pinMapMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ExecuteFunctionalMode_ScoreboardIncoming1s_Port1()
        {
            // Mock the functional test service.
            this.captureFailureTest.Setup(o => o.EnableStartPatternOnFirstFail());
            this.functionalServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 0, 1, this.PrePlist)).Returns(this.captureFailureTest.Object);

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string>());
            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, true));
            this.dieRecoveryMock.Setup(o => o.LogTrackingStructure(new BitArray(4, true), new BitArray(4, false)));

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                VoltageTargets = "VccPin",
                StartVoltages = "0.500",
                EndVoltageLimits = "1.0",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_off,update_always",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                FailCaptureCount = 0,
                PatternNameMap = string.Empty,
                RecoveryMode = RecoveryModes.RecoveryPort,
                RecoveryTrackingIncoming = "T0,T2,T2,T3",
                RecoveryTrackingOutgoing = "T0,T2,T2,T3",
                PinMap = "P0,P1,P2,P3",
                RecoveryOptions = "0000,0011,1100",
                ScoreboardBaseNumber = string.Empty,
                ScoreboardMaxFails = 0,
                TestMode = VminTC.TestModes.Scoreboard,
                CornerIdentifiers = "CLR_F3",
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            vminCornerMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
            this.testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ExecuteFunctionalMode_ForceRecoveryLoop_Port0()
        {
            // Mock the functional test service.
            this.captureFailureTest.Setup(o => o.EnableStartPatternOnFirstFail());
            this.captureFailureTest.Setup(o => o.ApplyTestConditions());
            this.captureFailureTest.Setup(o => o.Reset());
            var failData = new Mock<IFailureData>(MockBehavior.Strict);
            failData.Setup(o => o.GetPatternName()).Returns("FailingPattern");
            failData.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failData.Setup(o => o.GetParentPlistName()).Returns("FakePlist");
            failData.Setup(o => o.GetVectorAddress()).Returns(2002);
            failData.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "PinA" });
            failData.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 1001 });
            failData.Setup(o => o.GetBurstIndex()).Returns(0);
            this.captureFailureTest.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<IFailureData> { failData.Object })
                .Returns(new List<IFailureData>())
                .Returns(new List<IFailureData>())
                .Returns(new List<IFailureData>());
            this.functionalServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 0, 1, this.PrePlist)).Returns(this.captureFailureTest.Object);

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("-9999|0.500|1.000|2"));
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<Prime.DatalogService.IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string>());
            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(new[] { false, false, true, true }));
            this.dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(new[] { true, false, true, true }), new BitArray(new[] { false, false, true, true }), new BitArray(new[] { true, false, false, false }), UpdateMode.Merge, true)).Returns(true);
            var functionalTest = this.captureFailureTest.As<IFunctionalTest>().Object;
            this.pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { false, false, true, true }), ref functionalTest, new List<string>()));
            this.pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, false, true, true }), ref functionalTest, new List<string>()));
            this.pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { false, false, true, true }), ref functionalTest));
            this.pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, false, true, true }), ref functionalTest));
            this.pinMapMock.Setup(o => o.DecodeFailure(functionalTest, null)).Returns(new BitArray(new[] { true, false, false, false }));
            this.pinMapMock.Setup(o => o.VoltageDomainsToFailTracker(new BitArray(1, false))).Returns(new BitArray(4, false));

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                VoltageTargets = "VccPin",
                StartVoltages = "0.500",
                EndVoltageLimits = "1.0",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_off,recovery_update_always,force_recovery_loop",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                FailCaptureCount = 0,
                PatternNameMap = string.Empty,
                RecoveryMode = RecoveryModes.RecoveryLoop,
                MaxRepetitionCount = 2,
                RecoveryTrackingIncoming = "T0,T2,T2,T3",
                RecoveryTrackingOutgoing = "T0,T2,T2,T3",
                PinMap = "P0,P1,P2,P3",
                RecoveryOptions = "0000,0011,1100",
                ScoreboardBaseNumber = string.Empty,
                ScoreboardMaxFails = 0,
                TestMode = VminTC.TestModes.Scoreboard,
                CornerIdentifiers = "CLR_F3",
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            vminCornerMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
            this.testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Basic functional test. Recovery.
        /// </summary>
        [TestMethod]
        public void ExecuteFunctionalMode_UseTrackingBits_Port0()
        {
            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("g1234567");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            this.captureFailureTest.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            this.captureFailureTest.Setup(o => o.EnableStartPatternOnFirstFail());
            this.captureFailureTest.Setup(o => o.ApplyTestConditions());
            this.captureFailureTest.Setup(o => o.Reset());
            this.captureFailureTest.Setup(o => o.Execute()).Returns(false);
            this.captureFailureTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object });
            this.captureFailureTest.Setup(o => o.DatalogFailure(1));

            this.functionalServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 0, this.PrePlist)).Returns(this.captureFailureTest.Object);

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("-9999|0.500|1.000|1"));
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");

            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns("0001".ToBitArray());
            this.dieRecoveryMock.Setup(o => o.LogTrackingStructure(new BitArray(new[] { false, false, false, true }), new BitArray(new[] { true, false, false, false })));
            var functionalTest = this.captureFailureTest.As<IFunctionalTest>().Object;
            this.pinMapMock.Setup(o => o.DecodeFailure(functionalTest, null)).Returns("1000".ToBitArray());
            this.pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { false, false, false, true }), ref functionalTest, new List<string>()));
            this.pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { false, false, false, true }), ref functionalTest));

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                VoltageTargets = "VccPin",
                StartVoltages = "0.500",
                EndVoltageLimits = "1.0",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 0,
                PatternNameMap = string.Empty,
                RecoveryMode = RecoveryModes.RecoveryPort,
                RecoveryTrackingIncoming = "T0,T2,T2,T3",
                RecoveryTrackingOutgoing = "T0,T2,T2,T3",
                PinMap = "P0,P1,P2,P3",
                RecoveryOptions = "0000,0011,1100",
                ScoreboardBaseNumber = string.Empty,
                ScoreboardMaxFails = 0,
                TestMode = VminTC.TestModes.Functional,
                CornerIdentifiers = "CLR_F3",
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            this.pinMapMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Basic functional test. Recovery.
        /// </summary>
        [TestMethod]
        public void ExecuteFunctionalMode_FailingAmble_Port5()
        {
            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("tgl_pre_");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            this.captureFailureTest.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            this.captureFailureTest.Setup(o => o.EnableStartPatternOnFirstFail());
            this.captureFailureTest.Setup(o => o.ApplyTestConditions());
            this.captureFailureTest.Setup(o => o.Reset());
            this.captureFailureTest.Setup(o => o.Execute()).Returns(false);
            this.captureFailureTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object });
            this.captureFailureTest.Setup(o => o.DatalogFailure(1));

            this.functionalServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 0, this.PrePlist)).Returns(this.captureFailureTest.Object);

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("-9999|0.500|1.000|1"));
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");

            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns("0001".ToBitArray());
            this.dieRecoveryMock.Setup(o => o.LogTrackingStructure(new BitArray(new[] { false, false, false, true }), new BitArray(new[] { true, false, false, false })));
            var functionalTest = this.captureFailureTest.As<IFunctionalTest>().Object;
            this.pinMapMock.Setup(o => o.DecodeFailure(functionalTest, null)).Returns("1000".ToBitArray());
            this.pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { false, false, false, true }), ref functionalTest, new List<string>()));
            this.pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { false, false, false, true }), ref functionalTest));
            this.plistObjectMock.Setup(o => o.IsPatternAnAmble("tgl_pre_")).Returns(true);

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                VoltageTargets = "VccPin",
                StartVoltages = "0.500",
                EndVoltageLimits = "1.0",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 0,
                PatternNameMap = string.Empty,
                RecoveryMode = RecoveryModes.RecoveryPort,
                RecoveryTrackingIncoming = "T0,T2,T2,T3",
                RecoveryTrackingOutgoing = "T0,T2,T2,T3",
                PinMap = "P0,P1,P2,P3",
                RecoveryOptions = "0000,0011,1100",
                ScoreboardBaseNumber = string.Empty,
                ScoreboardMaxFails = 0,
                TestMode = VminTC.TestModes.Functional,
                CornerIdentifiers = "CLR_F3",
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(5, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            this.pinMapMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Multi-pass functional test. Recovery.
        /// </summary>
        [TestMethod]
        public void ExecuteScoreboardMode_MultiPass_Port3()
        {
            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("g1234567");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            this.captureFailureTest.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            this.captureFailureTest.Setup(o => o.EnableStartPatternOnFirstFail());
            this.captureFailureTest.Setup(o => o.ApplyTestConditions());
            this.captureFailureTest.Setup(o => o.Reset());
            this.captureFailureTest.SetupSequence(o => o.Execute())
                .Returns(false)
                .Returns(true);
            this.captureFailureTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object });
            this.captureFailureTest.Setup(o => o.DatalogFailure(1));

            this.functionalServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 0, 1, this.PrePlist)).Returns(this.captureFailureTest.Object);
            this.functionalServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1000, 1, this.PrePlist)).Returns(this.captureFailureTest.Object);

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("-9999|0.500|1.000|2"));
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetData("1234567"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_lp"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");

            var scoreBoardServiceMock = new Mock<IScoreboardService>(MockBehavior.Strict);
            var loggerMock = new Mock<IScoreboardLogger>(MockBehavior.Strict);
            scoreBoardServiceMock.Setup(o => o.CreateLogger(9911, "1,2,3,4,5,6,7", 1000)).Returns(loggerMock.Object);
            Prime.Services.ScoreBoardService = scoreBoardServiceMock.Object;

            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, false));
            this.dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(new[] { true, true, false, false }), new BitArray(4, false), new BitArray(new[] { true, false, false, false }), UpdateMode.Merge, true)).Returns(true);
            var functionalTest = this.captureFailureTest.As<IFunctionalTest>().Object;
            this.pinMapMock.SetupSequence(o => o.DecodeFailure(functionalTest, null))
                .Returns("1000".ToBitArray())
                .Returns("0000".ToBitArray());
            this.pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { false, false, true, true }), ref functionalTest, new List<string>()));
            this.pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, false, false }), ref functionalTest, new List<string>()));
            this.pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { false, false, true, true }), ref functionalTest));
            this.pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, false, false }), ref functionalTest));

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                PrePlist = string.Empty,
                VoltageTargets = "VccPin",
                StartVoltages = "0.500",
                EndVoltageLimits = "1.0",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 0,
                RecoveryMode = RecoveryModes.RecoveryPort,
                RecoveryTrackingIncoming = "T0,T2,T2,T3",
                RecoveryTrackingOutgoing = "T0,T2,T2,T3",
                PinMap = "P0,P1,P2,P3",
                RecoveryOptions = "0000,0011,1100",
                PatternNameMap = "1,2,3,4,5,6,7",
                ExecutionMode = ExecutionModeFlag.SearchWithScoreboard,
                ScoreboardBaseNumber = "9911",
                ScoreboardMaxFails = 1000,
                ScoreboardEdgeTicks = 3,
                MultiPassMasks = "0011,1100",

                // Custom Parameters.
                TestMode = VminTC.TestModes.Scoreboard,
                CornerIdentifiers = "CLR_F3",
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(3, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            scoreBoardServiceMock.VerifyAll();
            loggerMock.VerifyAll();
            this.pinMapMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Recovery functional test. Fail.
        /// </summary>
        [TestMethod]
        public void ApplyPreExecuteSetup_CornerPatConfigSetPoints_Pass()
        {
            this.CornerIdentifiers = "C0,C1,C2,C3";
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.PinMap = "NOA0,NOA1,NO2,NOA3";
            this.RecoveryTrackingIncoming = "CORE0,CORE1,CORE2,CORE3";
            this.RecoveryTrackingOutgoing = "CORE0,CORE1,CORE2,CORE3";
            this.RecoveryOptions = "0000,1100,0011";
            this.VoltageTargets = "Pin0,Pin1,Pin2,Pin3";
            this.EndVoltageLimits = "1,1,1,1";
            this.StartVoltages = "0.5,0.5,0.5,0.5";
            this.FeatureSwitchSettings = string.Empty;
            this.CornerPatConfigSetPoints = "SomeKey";
            this.TestMode = TestModes.Functional;

            var vminForwardingFactoryMock = new Mock<IVminForwardingFactory>(MockBehavior.Strict);
            var corner0 = new Mock<IVminForwardingCorner>(MockBehavior.Strict);
            var corner1 = new Mock<IVminForwardingCorner>(MockBehavior.Strict);
            var corner2 = new Mock<IVminForwardingCorner>(MockBehavior.Strict);
            var corner3 = new Mock<IVminForwardingCorner>(MockBehavior.Strict);
            vminForwardingFactoryMock.Setup(v => v.Get("C0", 1)).Returns(corner0.Object);
            vminForwardingFactoryMock.Setup(v => v.Get("C1", 2)).Returns(corner1.Object);
            vminForwardingFactoryMock.Setup(v => v.Get("C2", 3)).Returns(corner2.Object);
            vminForwardingFactoryMock.Setup(v => v.Get("C3", 4)).Returns(corner3.Object);
            DDG.VminForwarding.Service = vminForwardingFactoryMock.Object;

            var freqSetPointMap = new FreqSetPointMap();
            var patConfigSetPoint = new PatConfigSetPoint
            {
                Module = "FUN",
                Group = "CoreFreq",
                SetPoint = "{BM.CoreFreq}",
            };
            freqSetPointMap.CornerIdentifiers = new Dictionary<string, List<PatConfigSetPoint>>
            {
                { "C0", new List<PatConfigSetPoint> { patConfigSetPoint } },
                { "C1", new List<PatConfigSetPoint> { patConfigSetPoint } },
                { "C2", new List<PatConfigSetPoint> { patConfigSetPoint } },
                { "C3", new List<PatConfigSetPoint> { patConfigSetPoint } },
            };
            this.sharedStorageValues[this.CornerPatConfigSetPoints] = JsonConvert.SerializeObject(freqSetPointMap);

            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            var patConfigSetPointHandle = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            patConfigSetPointHandle.Setup(p => p.ApplySetPoint("1GHz"));
            patConfigSetPointHandle.Setup(p => p.ApplySetPoint("2GHz"));
            patConfigSetPointHandle.Setup(p => p.ApplySetPoint("3GHz"));
            patConfigSetPointHandle.Setup(p => p.ApplySetPoint("4GHz"));
            patConfigServiceMock
                .Setup(p => p.GetSetPointHandle("FUN", "CoreFreq", this.Patlist))
                .Returns(patConfigSetPointHandle.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var binMatrixServiceMock = new Mock<IBinMatrixService>(MockBehavior.Strict);
            var specInfo0 = new Mock<ISpecInfo>(MockBehavior.Strict);
            specInfo0.Setup(s => s.GetData()).Returns("4");
            specInfo0.Setup(s => s.GetUnit()).Returns("GHz");
            var specInfo1 = new Mock<ISpecInfo>(MockBehavior.Strict);
            specInfo1.Setup(s => s.GetData()).Returns("3");
            specInfo1.Setup(s => s.GetUnit()).Returns("GHz");
            var specInfo2 = new Mock<ISpecInfo>(MockBehavior.Strict);
            specInfo2.Setup(s => s.GetData()).Returns("2");
            specInfo2.Setup(s => s.GetUnit()).Returns("GHz");
            var specInfo3 = new Mock<ISpecInfo>(MockBehavior.Strict);
            specInfo3.Setup(s => s.GetData()).Returns("1");
            specInfo3.Setup(s => s.GetUnit()).Returns("GHz");
            binMatrixServiceMock.Setup(b => b.GetSpecInfo(1, "BM.CoreFreq")).Returns(specInfo0.Object);
            binMatrixServiceMock.Setup(b => b.GetSpecInfo(2, "BM.CoreFreq")).Returns(specInfo1.Object);
            binMatrixServiceMock.Setup(b => b.GetSpecInfo(3, "BM.CoreFreq")).Returns(specInfo2.Object);
            binMatrixServiceMock.Setup(b => b.GetSpecInfo(4, "BM.CoreFreq")).Returns(specInfo3.Object);
            Prime.Services.BinMatrixService = binMatrixServiceMock.Object;

            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, false));
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1,2,3,4");

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            ((IVminSearchExtensions)this).ApplyPreExecuteSetup(this.Patlist);

            corner0.VerifyAll();
            corner1.VerifyAll();
            corner2.VerifyAll();
            corner3.VerifyAll();
            vminForwardingFactoryMock.VerifyAll();
            patConfigSetPointHandle.VerifyAll();
            patConfigServiceMock.VerifyAll();
            binMatrixServiceMock.VerifyAll();
            specInfo0.VerifyAll();
            specInfo1.VerifyAll();
            specInfo2.VerifyAll();
            specInfo3.VerifyAll();
            this.testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// ApplySearchVoltage with dlvr converter.
        /// </summary>
        [TestMethod]
        public void ApplySearchVoltage_FunctionalDlvr_Pass()
        {
            this.Patlist = "SomePlist";
            this.FivrCondition = "SomeFivrCondition";
            this.FeatureSwitchSettings = "fivr_mode_on";
            this.TestMode = TestModes.Functional;
            this.VoltageConverter = "--railconfigurations=dlvr pinattributes powerswitch --overrides=1:1.2,2:0.3,3:0.3";
            this.LevelsTc = "SomeLevels";
            this.VoltageTargets = "A";

            var fivrVoltageMock = new Mock<IFivrDomainsAndConditionWithRails>(MockBehavior.Strict);
            fivrVoltageMock.Setup(o => o.ApplyConditionWithOverride(new Dictionary<string, double> { { "1", 1.2 }, { "2", 0.3 }, { "3", 0.3 } }));
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            voltageServiceMock.Setup(o => o.CreateFivrDomainsAndConditionWithRails(new List<string> { "A" }, this.FivrCondition, this.Patlist, new List<string> { "dlvr", "pinattributes", "powerswitch" })).Returns(fivrVoltageMock.Object);
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            var patConfigSetPointHandleMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            patConfigServiceMock.Setup(p => p.GetSetPointHandle("Module", "Group", this.Patlist)).Returns(patConfigSetPointHandleMock.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();

            ((IVminSearchExtensions)this).GetSearchVoltageObject(this.VoltageTargets.ToList(), this.Patlist);
            ((IVminSearchExtensions)this).ApplyPreExecuteSetup(this.Patlist);
            ((IVminSearchExtensions)this).ApplyInitialVoltage(fivrVoltageMock.Object);
            fivrVoltageMock.VerifyAll();
            voltageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Basic functional test using DTS.
        /// </summary>
        [TestMethod]
        public void ExecuteFunctionalMode_DTS_Port4()
        {
            this.InstanceName = "DummyInstance";
            this.Patlist = "FakePlist";
            this.TimingsTc = "FakeTimings";
            this.LevelsTc = "FakeLevels";
            this.VoltageTargets = "VccPin";
            this.StartVoltages = "0.500";
            this.EndVoltageLimits = "1.0";
            this.StepSize = 0.100;
            this.FeatureSwitchSettings = "fivr_mode_off";
            this.FivrCondition = string.Empty;
            this.LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED;
            this.FailCaptureCount = 1;
            this.TestMode = VminTC.TestModes.Functional;
            this.CornerIdentifiers = "CLR_F3";
            this.ForwardingMode = VminTC.ForwardingModes.InputOutput;
            this.PrePlist = string.Empty;
            this.DtsConfiguration = "dts_configuration";

            // Mock the functional test service.
            var ctvTest = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            var ctvData = "000000001000000001";
            ctvTest.Setup(o => o.ResolvePlist(this.InstanceName)).Returns(this.Patlist);
            ctvTest.Setup(o => o.EnableStartPatternOnFirstFail());
            ctvTest.Setup(o => o.ApplyTestConditions());
            ctvTest.Setup(o => o.Reset());
            ctvTest.Setup(o => o.SetPinMask(new List<string>()));
            ctvTest.Setup(o => o.Execute()).Returns(true);
            ctvTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData>());
            ctvTest.SetupSequence(o => o.GetCtvData("TDO")).Returns(ctvData);
            this.functionalServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest(this.Patlist, this.LevelsTc, this.TimingsTc, new List<string> { "TDO" }, this.FailCaptureCount, this.PrePlist)).Returns(ctvTest.Object);

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetData("0.500|0.500|1.000|1"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            this.testProgramServiceMock.Setup(o => o.SetTestInstanceParameter(this.InstanceName, "Patlist", "FakePlist_DTS"));

            // Mock DDG DTS
            var dtsMock = new Mock<IDTS>(MockBehavior.Strict);
            dtsMock.Setup(o => o.GetSettings()).Returns(new DTSBase.Configuration
            {
                IsEnabled = true,
                PinName = "TDO",
                RegisterSize = 9,
                SensorsList = new List<string> { "S0", "S1" },
                Slope = 0.5,
                Offset = -64,
                DatalogValues = true,
                CompressedDatalog = true,
                SetPoint = "100",
                UpperTolerance = "10",
                LowerTolerance = "10",
            });
            dtsMock.Setup(o => o.GetValues(ctvData, ref It.Ref<Dictionary<string, List<double>>>.IsAny));
            dtsMock.Setup(o => o.PrintToDatalog(It.IsAny<Dictionary<string, List<double>>>()));
            dtsMock.Setup(o => o.EvaluateLimits(It.IsAny<Dictionary<string, List<double>>>())).Returns(false);

            var dtsFactoryMock = new Mock<IDTSFactory>(MockBehavior.Strict);
            dtsFactoryMock.Setup(o => o.Get("dts_configuration")).Returns(dtsMock.Object);
            DDG.DTS.Service = dtsFactoryMock.Object;

            // Run Verify/execute.
            this.TestMethodExtension = this;
            this.Verify();
            this.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(4, this.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            dtsMock.VerifyAll();
            dtsFactoryMock.VerifyAll();
        }

        /// <summary>
        /// Basic functional test using DTS.
        /// </summary>
        [TestMethod]
        public void ExecuteFunctionalMode_DTS_Port1()
        {
            this.InstanceName = "DummyInstance";
            this.Patlist = "FakePlist";
            this.TimingsTc = "FakeTimings";
            this.LevelsTc = "FakeLevels";
            this.VoltageTargets = "VccPin";
            this.StartVoltages = "0.500";
            this.EndVoltageLimits = "1.0";
            this.StepSize = 0.100;
            this.FeatureSwitchSettings = "fivr_mode_off";
            this.FivrCondition = string.Empty;
            this.LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED;
            this.FailCaptureCount = 1;
            this.TestMode = VminTC.TestModes.Functional;
            this.CornerIdentifiers = "CLR_F3";
            this.ForwardingMode = VminTC.ForwardingModes.InputOutput;
            this.DtsConfiguration = "dts_configuration";
            this.PrePlist = string.Empty;

            // Mock the functional test service.
            var ctvTest = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            var ctvData = "000000001000000001";
            ctvTest.Setup(o => o.ResolvePlist(this.InstanceName)).Returns(this.Patlist);
            ctvTest.Setup(o => o.EnableStartPatternOnFirstFail());
            ctvTest.Setup(o => o.ApplyTestConditions());
            ctvTest.Setup(o => o.Reset());
            ctvTest.Setup(o => o.SetPinMask(new List<string>()));
            ctvTest.Setup(o => o.Execute()).Returns(true);
            ctvTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData>());
            ctvTest.SetupSequence(o => o.GetCtvData("TDO")).Returns(ctvData);
            this.functionalServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest(this.Patlist, this.LevelsTc, this.TimingsTc, new List<string> { "TDO" }, this.FailCaptureCount, this.PrePlist)).Returns(ctvTest.Object);

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetData("0.500|0.500|1.000|1"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            this.testProgramServiceMock.Setup(o => o.SetTestInstanceParameter(this.InstanceName, "Patlist", "FakePlist_DTS"));

            // Mock DDG DTS
            var dtsMock = new Mock<IDTS>(MockBehavior.Strict);
            dtsMock.Setup(o => o.GetSettings()).Returns(new DTSBase.Configuration
            {
                IsEnabled = true,
                PinName = "TDO",
                RegisterSize = 9,
                SensorsList = new List<string> { "S0", "S1" },
                Slope = 0.5,
                Offset = -64,
                DatalogValues = true,
                CompressedDatalog = true,
                SetPoint = "100",
                UpperTolerance = "10",
                LowerTolerance = "10",
            });
            dtsMock.Setup(o => o.GetValues(ctvData, ref It.Ref<Dictionary<string, List<double>>>.IsAny));
            dtsMock.Setup(o => o.PrintToDatalog(It.IsAny<Dictionary<string, List<double>>>()));
            dtsMock.Setup(o => o.EvaluateLimits(It.IsAny<Dictionary<string, List<double>>>())).Returns(true);

            var dtsFactoryMock = new Mock<IDTSFactory>(MockBehavior.Strict);
            dtsFactoryMock.Setup(o => o.Get("dts_configuration")).Returns(dtsMock.Object);
            DDG.DTS.Service = dtsFactoryMock.Object;

            // Run Verify/execute.
            this.TestMethodExtension = this;
            this.Verify();
            this.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, this.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            dtsMock.VerifyAll();
            dtsFactoryMock.VerifyAll();
        }

        /// <summary>
        /// Basic functional test using DTS.
        /// </summary>
        [TestMethod]
        public void ExecuteFunctionalMode_DTSNoCTV_Port1()
        {
            this.InstanceName = "DummyInstance";
            this.Patlist = "FakePlist";
            this.TimingsTc = "FakeTimings";
            this.LevelsTc = "FakeLevels";
            this.VoltageTargets = "VccPin";
            this.StartVoltages = "0.500";
            this.EndVoltageLimits = "1.0";
            this.StepSize = 0.100;
            this.FeatureSwitchSettings = "fivr_mode_off";
            this.FivrCondition = string.Empty;
            this.LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED;
            this.FailCaptureCount = 1;
            this.TestMode = VminTC.TestModes.Functional;
            this.CornerIdentifiers = "CLR_F3";
            this.ForwardingMode = VminTC.ForwardingModes.InputOutput;
            this.DtsConfiguration = "dts_configuration";
            this.PrePlist = string.Empty;

            // Mock the functional test service.
            var ctvTest = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            ctvTest.Setup(o => o.ResolvePlist(this.InstanceName)).Returns(this.Patlist);
            ctvTest.Setup(o => o.EnableStartPatternOnFirstFail());
            ctvTest.Setup(o => o.ApplyTestConditions());
            ctvTest.Setup(o => o.Reset());
            ctvTest.Setup(o => o.SetPinMask(new List<string>()));
            ctvTest.Setup(o => o.Execute()).Returns(true);
            ctvTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData>());
            ctvTest.SetupSequence(o => o.GetCtvData("TDO")).Throws<Exception>();
            this.functionalServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest(this.Patlist, this.LevelsTc, this.TimingsTc, new List<string> { "TDO" }, this.FailCaptureCount, this.PrePlist)).Returns(ctvTest.Object);

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetData("0.500|0.500|1.000|1"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            this.testProgramServiceMock.Setup(o => o.SetTestInstanceParameter(this.InstanceName, "Patlist", "FakePlist_DTS"));

            // Mock DDG DTS
            var dtsMock = new Mock<IDTS>(MockBehavior.Strict);
            dtsMock.Setup(o => o.GetSettings()).Returns(new DTSBase.Configuration
            {
                IsEnabled = true,
                PinName = "TDO",
                RegisterSize = 9,
                SensorsList = new List<string> { "S0", "S1" },
                Slope = 0.5,
                Offset = -64,
                DatalogValues = true,
                CompressedDatalog = true,
                SetPoint = "100",
                UpperTolerance = "10",
                LowerTolerance = "10",
            });
            dtsMock.Setup(o => o.PrintToDatalog(It.IsAny<Dictionary<string, List<double>>>()));
            dtsMock.Setup(o => o.EvaluateLimits(It.IsAny<Dictionary<string, List<double>>>())).Returns(true);

            var dtsFactoryMock = new Mock<IDTSFactory>(MockBehavior.Strict);
            dtsFactoryMock.Setup(o => o.Get("dts_configuration")).Returns(dtsMock.Object);
            DDG.DTS.Service = dtsFactoryMock.Object;

            // Run Verify/execute.
            this.TestMethodExtension = this;
            this.Verify();
            this.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, this.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            dtsMock.VerifyAll();
            dtsFactoryMock.VerifyAll();
        }

        /// <summary>
        /// Do a full execution of VminTC in Functional mode in ATOM per-core configuration. Failing first pass.
        /// </summary>
        [TestMethod]
        public void FunctionalExecute_AtomPerCore_Port3()
        {
            // Mock the console service.
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
                System.Console.WriteLine($"DEBUG: {msg}"));
            this.consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) => System.Console.WriteLine($"DEBUG: {msg}"));
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            // Mock pin service.
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            var m0Pin = new Mock<IPin>(MockBehavior.Strict);
            m0Pin.Setup(o => o.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var m1Pin = new Mock<IPin>(MockBehavior.Strict);
            m1Pin.Setup(o => o.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            pinServiceMock.Setup(o => o.Get("M0_HC")).Returns(m0Pin.Object);
            pinServiceMock.Setup(o => o.Get("M1_HC")).Returns(m1Pin.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            // Mock test condition service.
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(o => o.GetPinAttributeValue("M0_HC", "FreeDriveTime")).Returns("0.001");
            testCondition.Setup(o => o.GetPinAttributeValue("M1_HC", "FreeDriveTime")).Returns("0.001");
            testConditionServiceMock.Setup(o => o.GetTestCondition("FakeLevels")).Returns(testCondition.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            // Mock the voltage service.
            var vForcePinAttributeMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            var voltageMock = vForcePinAttributeMock.As<IVoltage>();
            voltageMock.Setup(o => o.Restore());
            voltageMock.Setup(o => o.Reset());
            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "M0_HC", "M0_HC", "M0_HC", "M0_HC", "M1_HC", "M1_HC", "M1_HC", "M1_HC" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(vForcePinAttributeMock.Object);
            Prime.Services.VoltageService = voltageServiceMock.Object;

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("g1234567");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetParentPlistName()).Returns("FakePlist");
            failDataMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "TDO" });
            failDataMock.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 1001 });
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(99);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.SetPinMask(new List<string> { "M0_RO" }));
            funcTestMock.Setup(o => o.DatalogFailure(1));
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false)
                .Returns(true);

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData>());
            funcTestMock.Setup(o => o.HasStartPattern()).Returns(false);
            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("-9999_-9999_-9999_-9999_-9999_-9999_-9999_-9999|0.500_0.500_0.500_0.500_0.500_0.500_0.500_0.500|0.520_0.520_0.520_0.520_0.520_0.520_0.520_0.520|2"));
            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            this.dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(new[] { false, false, false, false, false, false, true, true }), new BitArray(8, false), new BitArray(new[] { false, false, false, false, false, false, true, true }), UpdateMode.Merge, true)).Returns(true);
            dieRecoveryServiceMock.Setup(o => o.Get("M0C01_TRACKER,M0C23_TRACKER,M1C01_TRACKER,M1C23_TRACKER")).Returns(this.dieRecoveryMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            this.pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            this.pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            this.pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            this.pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny));
            this.pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapServiceMock.Setup(o => o.Get("ATOM_MAP")).Returns(this.pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string> { { "parameter", "value" } });

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                RecoveryMode = VminTC.RecoveryModes.RecoveryPort,
                VoltageTargets = "M0_HC,M0_HC,M0_HC,M0_HC,M1_HC,M1_HC,M1_HC,M1_HC",
                RecoveryTrackingIncoming = "M0C01_TRACKER,M0C23_TRACKER,M1C01_TRACKER,M1C23_TRACKER",
                RecoveryTrackingOutgoing = "M0C01_TRACKER,M0C23_TRACKER,M1C01_TRACKER,M1C23_TRACKER",
                PinMap = "ATOM_MAP",
                CornerIdentifiers = string.Empty,
                StartVoltages = "0.5,0.5,0.5,0.5,0.5,0.5,0.5,0.5",
                EndVoltageLimits = "0.52,0.52,0.52,0.52,0.52,0.52,0.52,0.52",
                StepSize = 0.01,
                FeatureSwitchSettings = "ignore_masked_results",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                FailCaptureCount = 1,
                MultiPassMasks = "11111100,11110011",
                RecoveryOptions = "00000000,00001100,00000011",
                PatternNameMap = string.Empty,
                TestMode = VminTC.TestModes.Functional,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                VminResult = string.Empty,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(3, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(2));
            this.pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            testCondition.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            vForcePinAttributeMock.VerifyAll();
            m0Pin.VerifyAll();
            m1Pin.VerifyAll();
            failDataMock.VerifyAll();
        }

        private void MockSharedStorage()
        {
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
                .Returns((string key, Type obj, Context context) =>
                    JsonConvert.DeserializeObject(this.sharedStorageValues[key], obj));
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
        }

        private void MockConsole()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
            {
                System.Console.WriteLine($"DEBUG: {msg}");
            });
            this.consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) => { System.Console.WriteLine($"DEBUG: {msg}"); });
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;
        }

        private void MockVoltage()
        {
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Apply(It.IsAny<List<double>>())); // added to deal with rounding errors.
            voltageMock.Setup(o => o.Restore());
            voltageMock.Setup(o => o.Reset());
            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "VccPin" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(voltageMock.Object);
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("VccPin", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition("FakeLevels")).Returns(testCondition.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.Get("VccPin")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;
            this.testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            Prime.Services.TestProgramService = this.testProgramServiceMock.Object;
        }

        private void MockDieRecovery()
        {
            this.dieRecoveryFactoryMock = new Mock<IDieRecoveryFactory>(MockBehavior.Strict);
            this.dieRecoveryMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            this.dieRecoveryFactoryMock.Setup(o => o.Get(It.IsAny<string>())).Returns(this.dieRecoveryMock.Object);
            DDG.DieRecovery.Service = this.dieRecoveryFactoryMock.Object;

            this.pinMapFactoryMock = new Mock<IPinMapFactory>(MockBehavior.Strict);
            this.pinMapMock = new Mock<IPinMap>(MockBehavior.Strict);
            this.pinMapMock.Setup(o => o.Restore());
            this.pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            this.pinMapFactoryMock.Setup(o => o.Get(It.IsAny<string>())).Returns(this.pinMapMock.Object);
            DDG.PinMap.Service = this.pinMapFactoryMock.Object;
        }
    }
}
