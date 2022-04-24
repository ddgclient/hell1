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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.DffService;
    using Prime.FunctionalService;
    using Prime.PerformanceService;
    using Prime.PinService;
    using Prime.PlistService;
    using Prime.SharedStorageService;
    using Prime.TestConditionService;
    using Prime.TestProgramService;
    using Prime.VoltageService;

    /// <summary>
    /// Defines the <see cref="MultiVminFull_UnitTest" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MultiVminFull_UnitTest
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IFivrDomainsAndCondition> voltageMock;
        private Mock<IVoltageService> voltageServiceMock;
        private Mock<IDatalogService> ituffMock;
        private Mock<ISharedStorageService> sharedStorageMock;
        private Mock<ITestProgramService> testProgramServiceMock;
        private Mock<IPlistService> plistServiceMock;
        private Mock<IPlistObject> plistObjectMock;
        private Mock<IPerformanceService> performanceServiceMock;

        /// <summary>
        /// Initialize generic mocks.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Mock the console service.
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
                System.Console.WriteLine($"DEBUG: {msg}"));
            this.consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) => System.Console.WriteLine($"DEBUG: {msg}"));
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            // Mock the voltage service.
            this.voltageMock = new Mock<Prime.VoltageService.IFivrDomainsAndCondition>(MockBehavior.Strict);
            this.voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = this.voltageServiceMock.Object;

            // Mock datalog.
            this.ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            Prime.Services.DatalogService = this.ituffMock.Object;

            // shared storage mocks for the start/end parameters.
            this.sharedStorageMock = new Mock<Prime.SharedStorageService.ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = this.sharedStorageMock.Object;

            // test program.
            this.testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string> { { "SomeParameter", "SomeValue" } });
            Prime.Services.TestProgramService = this.testProgramServiceMock.Object;

            this.performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = this.performanceServiceMock.Object;

            // TODO - Generic mock for plist service.
            this.plistServiceMock = new Mock<IPlistService>(MockBehavior.Default);
            this.plistObjectMock = new Mock<IPlistObject>(MockBehavior.Default);
            this.plistServiceMock.Setup(o => o.GetPlistObject(It.IsAny<string>())).Returns(this.plistObjectMock.Object);
            Prime.Services.PlistService = this.plistServiceMock.Object;
        }

        /// <summary>
        /// Do a full execution of VminTC in MultiVmin mode using VminForwarding.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_Full_Pass()
        {
            // Mock the voltage service.
            this.voltageMock.Setup(o => o.Apply(new List<double> { 0.5, 0.6, 0.5, 0.7 }));
            this.voltageMock.Setup(o => o.Apply(new List<double> { 0.6, 0.6, 0.5, 0.7 }));
            this.voltageMock.Setup(o => o.Apply(It.IsAny<List<double>>())); // added to deal with rounding errors.
            this.voltageMock.Setup(o => o.ApplyCondition());
            this.voltageMock.Setup(o => o.Reset());
            this.voltageServiceMock.Setup(v => v.CreateFivrForDomainsAndCondition(new List<string> { "CORE0", "CORE1", "CORE2", "CORE3" }, "NOM", "FakePlist")).Returns(this.voltageMock.Object);

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetParentPlistName()).Returns("ParentPlistName");
            failDataMock.Setup(o => o.GetPatternName()).Returns("g1234567");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.SetPinMask(new List<string> { "NOAB_02", "NOAB_03" }));
            funcTestMock.Setup(o => o.SetPinMask(new List<string> { "NOAB_00", "NOAB_01" }));
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false)
                .Returns(true)
                .Returns(true);

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object });

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("CR0@F1:1:0.800_CR1@F1:1:0.800_CR2@F1:1:0.800_CR3@F1:1:0.800"));
            strValWriterMock.Setup(o => o.SetData("0.600_0.600_0.500_0.700|0.500_0.600_0.500_0.700|0.900_0.900_0.900_0.900|3"));
            strValWriterMock.Setup(o => o.SetData("1234567^na^na^na"));
            strValWriterMock.Setup(o => o.SetData("1_0_0_0"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_it"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_lp"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            this.ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            this.ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.6)).Returns(0.6);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.7)).Returns(0.6);
            vminCornerMock.Setup(o => o.GetStartingVoltage(-9999)).Returns(0.5);
            vminCornerMock.Setup(o => o.StoreVminResult(It.IsAny<double>())).Returns(true); // to deal with rounding errors allow any, check with Verify.

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CR0@F1", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.Get("CR1@F1", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.Get("CR2@F1", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.Get("CR3@F1", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CR0@F1", 1)).Returns(0.8e9);
            forwardingServiceMock.Setup(o => o.GetFrequency("CR1@F1", 1)).Returns(0.8e9);
            forwardingServiceMock.Setup(o => o.GetFrequency("CR2@F1", 1)).Returns(0.8e9);
            forwardingServiceMock.Setup(o => o.GetFrequency("CR3@F1", 1)).Returns(0.8e9);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            var dieRecoveryMockIncoming = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMockIncoming.SetupSequence(o => o.GetMaskBits())
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));
            var dieRecoveryMockOutgoing = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMockOutgoing.Setup(o => o.UpdateTrackingStructure(new BitArray(4, false), new BitArray(4, false), new BitArray(4, false), UpdateMode.Merge, true)).Returns(true);
            dieRecoveryServiceMock.Setup(o => o.Get("TCORE0,TCORE1,TCORE2,TCORE3")).Returns(dieRecoveryMockIncoming.Object);
            dieRecoveryServiceMock.Setup(o => o.Get("SCORE0,SCORE1,SCORE2,SCORE3")).Returns(dieRecoveryMockOutgoing.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(new BitArray(new[] { true, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));
            var funcTest = funcTestMock.As<IFunctionalTest>().Object;
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.Restore());
            pinMapServiceMock.Setup(o => o.Get("PCORE0,PCORE1,PCORE2,PCORE3")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                RecoveryMode = VminTC.RecoveryModes.Default,
                VoltageTargets = "CORE0,CORE1,CORE2,CORE3",
                RecoveryTrackingIncoming = "TCORE0,TCORE1,TCORE2,TCORE3",
                RecoveryTrackingOutgoing = "SCORE0,SCORE1,SCORE2,SCORE3",
                PinMap = "PCORE0,PCORE1,PCORE2,PCORE3",
                CornerIdentifiers = "CR0@F1,CR1@F1,CR2@F1, CR3@F1",
                StartVoltages = "0.5,0.6,0.5,0.7",
                EndVoltageLimits = "0.9,0.9,0.9,0.9",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_on,print_per_target_increments",
                FivrCondition = "NOM",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 1,
                MultiPassMasks = "0011,1100",
                PatternNameMap = "1,2,3,4,5,6,7",
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(3));
            funcTestMock.Verify(o => o.GetPerCycleFailures(), Times.Exactly(3));
            strValWriterMock.VerifyAll();
            this.ituffMock.VerifyAll();
            vminCornerMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            dieRecoveryMockIncoming.VerifyAll();
            dieRecoveryMockOutgoing.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            this.sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Do a execution of VminTC in MultiVmin mode using VminForwarding with no DieRecovery and negative start voltages.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_NegativeStartVoltage_Pass()
        {
            // Mock the voltage service.
            this.voltageMock.Setup(o => o.Apply(It.IsAny<List<double>>())); // added to deal with rounding errors.
            this.voltageMock.Setup(o => o.ApplyCondition());
            this.voltageMock.Setup(o => o.Reset());
            this.voltageServiceMock.Setup(v => v.CreateFivrForDomainsAndCondition(new List<string> { "CORE0", "CORE1", "CORE2", "CORE3" }, "NOM", "FakePlist")).Returns(this.voltageMock.Object);

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetParentPlistName()).Returns("ParentPlistName");
            failDataMock.Setup(o => o.GetPatternName()).Returns("g1234567");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false)
                .Returns(true);

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData>());

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("0.600_0.600_0.500_-8888|0.500_0.600_0.500_-9999|0.900_0.900_0.900_0.900|2"));
            strValWriterMock.Setup(o => o.SetData("1234567^na^na^na"));
            strValWriterMock.Setup(o => o.SetData("1_0_0_0"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_it"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_lp"));

            this.ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            this.ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));

            // Mock DDGs DieRecovery
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(new BitArray(new[] { true, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));
            var pinMapDecoder = new Mock<IPinMapDecoder>(MockBehavior.Strict);
            pinMapDecoder.Setup(o => o.NumberOfTrackerElements).Returns(4);
            pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { pinMapDecoder.Object });
            var funcTest = funcTestMock.As<IFunctionalTest>().Object;
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { false, false, false, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { false, false, false, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.Restore());
            pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            pinMapServiceMock.Setup(o => o.Get("PCORE0,PCORE1,PCORE2,PCORE3")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                VoltageTargets = "CORE0,CORE1,CORE2,CORE3",
                PinMap = "PCORE0,PCORE1,PCORE2,PCORE3",
                StartVoltages = "0.5,0.6,0.5,-9999",
                EndVoltageLimits = "0.9,0.9,0.9,0.9",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_on,print_per_target_increments",
                FivrCondition = "NOM",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
                FailCaptureCount = 1,
                PatternNameMap = "1,2,3,4,5,6,7",
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.None,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(2));
            funcTestMock.Verify(o => o.GetPerCycleFailures(), Times.Exactly(3));
            funcTestMock.VerifyAll();
            strValWriterMock.VerifyAll();
            this.ituffMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            this.sharedStorageMock.VerifyAll();
            this.voltageMock.VerifyAll();
        }

        /// <summary>
        /// Do a full execution of VminTC in MultiVmin mode using DFF.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_FullDFF_Pass()
        {
            // Mock the voltage service.
            this.voltageMock.Setup(o => o.Apply(new List<double> { 0.5, 0.6, 0.5, 0.7 }));
            this.voltageMock.Setup(o => o.Apply(new List<double> { 0.6, 0.6, 0.5, 0.7 }));
            this.voltageMock.Setup(o => o.Apply(It.IsAny<List<double>>())); // added to deal with rounding errors.
            this.voltageMock.Setup(o => o.ApplyCondition());
            this.voltageMock.Setup(o => o.Reset());
            this.voltageServiceMock.Setup(v => v.CreateFivrForDomainsAndCondition(new List<string> { "CORE0", "CORE1", "CORE2", "CORE3" }, "NOM", "FakePlist")).Returns(this.voltageMock.Object);

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetParentPlistName()).Returns("ParentPlistName");
            failDataMock.Setup(o => o.GetPatternName()).Returns("g1234567");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.SetPinMask(new List<string> { "NOAB_02", "NOAB_03" }));
            funcTestMock.Setup(o => o.SetPinMask(new List<string> { "NOAB_00", "NOAB_01" }));
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false)
                .Returns(true)
                .Returns(true);

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object });

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("CR0@F1:1:0.800_CR1@F1:1:0.800_CR2@F1:1:0.800_CR3@F1:1:0.800"));
            strValWriterMock.Setup(o => o.SetData("0.600_0.600_0.500_0.700|0.500_0.600_0.500_0.700|0.900_0.900_0.900_0.900|3"));
            strValWriterMock.Setup(o => o.SetData("1234567^na^na^na"));
            strValWriterMock.Setup(o => o.SetData("1_0_0_0"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_it"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_lp"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DFF service.
            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(o => o.GetDff("START_VOLTAGE", true)).Returns("0.5v0.6v0.5v0.7");
            dffServiceMock.Setup(o => o.GetDff("END_VOLTAGE", true)).Returns("0.9");
            dffServiceMock.Setup(o => o.SetDff("VMIN_RESULT", "0.600v0.600v0.500v0.700"));
            Prime.Services.DffService = dffServiceMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.6)).Returns(0.6);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.7)).Returns(0.6);
            vminCornerMock.Setup(o => o.GetStartingVoltage(-9999)).Returns(0.5);
            vminCornerMock.Setup(o => o.StoreVminResult(It.IsAny<double>())).Returns(true); // to deal with rounding errors allow any, check with Verify.

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CR0@F1", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.Get("CR1@F1", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.Get("CR2@F1", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.Get("CR3@F1", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CR0@F1", 1)).Returns(0.8e9);
            forwardingServiceMock.Setup(o => o.GetFrequency("CR1@F1", 1)).Returns(0.8e9);
            forwardingServiceMock.Setup(o => o.GetFrequency("CR2@F1", 1)).Returns(0.8e9);
            forwardingServiceMock.Setup(o => o.GetFrequency("CR3@F1", 1)).Returns(0.8e9);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            var dieRecoveryMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.SetupSequence(o => o.GetMaskBits())
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));
            dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(4, false), new BitArray(4, false), new BitArray(4, false), UpdateMode.Merge, true)).Returns(true);
            dieRecoveryServiceMock.Setup(o => o.Get("TCORE0,TCORE1,TCORE2,TCORE3")).Returns(dieRecoveryMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(new BitArray(new[] { true, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));
            var funcTest = funcTestMock.As<IFunctionalTest>().Object;
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.Restore());
            pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            pinMapServiceMock.Setup(o => o.Get("PCORE0,PCORE1,PCORE2,PCORE3")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            // shared storage mocks for the start/end parameters.
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                RecoveryMode = VminTC.RecoveryModes.Default,
                VoltageTargets = "CORE0,CORE1,CORE2,CORE3",
                RecoveryTrackingIncoming = "TCORE0,TCORE1,TCORE2,TCORE3",
                RecoveryTrackingOutgoing = "TCORE0,TCORE1,TCORE2,TCORE3",
                PinMap = "PCORE0,PCORE1,PCORE2,PCORE3",
                CornerIdentifiers = "CR0@F1,CR1@F1,CR2@F1, CR3@F1",
                StartVoltages = "D.START_VOLTAGE",
                EndVoltageLimits = "D.END_VOLTAGE",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_on,print_per_target_increments",
                FivrCondition = "NOM",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 1,
                MultiPassMasks = "0011,1100",
                PatternNameMap = "1,2,3,4,5,6,7",
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                VminResult = "D.VMIN_RESULT",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(3));
            funcTestMock.Verify(o => o.GetPerCycleFailures(), Times.Exactly(3));
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            vminCornerMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            this.sharedStorageMock.VerifyAll();
            dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_LimitGuardband_P0()
        {
            // Mock the voltage service.
            this.voltageMock.Setup(o => o.Apply(new List<double> { 0.5, 0.6, 0.5, 0.7 }));
            this.voltageMock.Setup(o => o.Apply(new List<double> { 0.6, 0.6, 0.5, 0.7 }));
            this.voltageMock.Setup(o => o.Apply(It.IsAny<List<double>>())); // added to deal with rounding errors.
            this.voltageMock.Setup(o => o.ApplyCondition());
            this.voltageMock.Setup(o => o.Reset());
            this.voltageServiceMock.Setup(v => v.CreateFivrForDomainsAndCondition(new List<string> { "CORE0", "CORE1", "CORE2", "CORE3" }, "NOM", "FakePlist")).Returns(this.voltageMock.Object);

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetParentPlistName()).Returns("ParentPlistName");
            failDataMock.Setup(o => o.GetPatternName()).Returns("g1234567");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.SetPinMask(new List<string> { "NOAB_02", "NOAB_03" }));
            funcTestMock.Setup(o => o.SetPinMask(new List<string> { "NOAB_00", "NOAB_01" }));
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false)
                .Returns(true)
                .Returns(true);

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object });

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("CR0@F1:1:0.800_CR1@F1:1:0.800_CR2@F1:1:0.800_CR3@F1:1:0.800"));
            strValWriterMock.Setup(o => o.SetData("0.600_0.600_0.500_0.700|0.500_0.600_0.500_0.700|0.900_0.900_0.900_0.900|3"));
            strValWriterMock.Setup(o => o.SetData("1234567^na^na^na"));
            strValWriterMock.Setup(o => o.SetData("1_0_0_0"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_it"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_lp"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DFF service.
            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(o => o.GetDff("START_VOLTAGE", true)).Returns("0.5v0.6v0.5v0.7");
            dffServiceMock.Setup(o => o.GetDff("END_VOLTAGE", true)).Returns("0.9");
            dffServiceMock.Setup(o => o.SetDff("VMIN_RESULT", "0.600v0.600v0.500v0.700"));
            Prime.Services.DffService = dffServiceMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.6)).Returns(0.6);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.7)).Returns(0.6);
            vminCornerMock.Setup(o => o.GetStartingVoltage(-9999)).Returns(0.5);
            vminCornerMock.Setup(o => o.StoreVminResult(It.IsAny<double>())).Returns(true); // to deal with rounding errors allow any, check with Verify.

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CR0@F1", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.Get("CR1@F1", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.Get("CR2@F1", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.Get("CR3@F1", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CR0@F1", 1)).Returns(0.8e9);
            forwardingServiceMock.Setup(o => o.GetFrequency("CR1@F1", 1)).Returns(0.8e9);
            forwardingServiceMock.Setup(o => o.GetFrequency("CR2@F1", 1)).Returns(0.8e9);
            forwardingServiceMock.Setup(o => o.GetFrequency("CR3@F1", 1)).Returns(0.8e9);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            forwardingServiceMock.Setup(o => o.IsSearchGuardbandEnabled()).Returns(true);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            var dieRecoveryMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.SetupSequence(o => o.GetMaskBits())
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));
            dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(4, false), new BitArray(4, false), new BitArray(4, false), UpdateMode.Merge, true)).Returns(true);
            dieRecoveryServiceMock.Setup(o => o.Get("TCORE0,TCORE1,TCORE2,TCORE3")).Returns(dieRecoveryMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(new BitArray(new[] { true, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));
            var funcTest = funcTestMock.As<IFunctionalTest>().Object;
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.Restore());
            pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            pinMapServiceMock.Setup(o => o.Get("PCORE0,PCORE1,PCORE2,PCORE3")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            // shared storage mocks for the start/end parameters.
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                RecoveryMode = VminTC.RecoveryModes.Default,
                VoltageTargets = "CORE0,CORE1,CORE2,CORE3",
                RecoveryTrackingIncoming = "TCORE0,TCORE1,TCORE2,TCORE3",
                RecoveryTrackingOutgoing = "TCORE0,TCORE1,TCORE2,TCORE3",
                PinMap = "PCORE0,PCORE1,PCORE2,PCORE3",
                CornerIdentifiers = "CR0@F1,CR1@F1,CR2@F1, CR3@F1",
                StartVoltages = "D.START_VOLTAGE",
                EndVoltageLimits = "D.END_VOLTAGE",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_on,print_per_target_increments",
                FivrCondition = "NOM",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 1,
                MultiPassMasks = "0011,1100",
                PatternNameMap = "1,2,3,4,5,6,7",
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                VminResult = "D.VMIN_RESULT",
                LimitGuardband = "0.05",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(3));
            funcTestMock.Verify(o => o.GetPerCycleFailures(), Times.Exactly(3));
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            vminCornerMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            this.sharedStorageMock.VerifyAll();
            dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Do a full execution of VminTC in MultiVmin mode with no forwarding.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_NoVminForwarding_Pass()
        {
            // Mock the voltage service.
            this.voltageMock.Setup(o => o.Apply(new List<double> { 0.5, 0.6, 0.5, 0.7 }));
            this.voltageMock.Setup(o => o.Apply(new List<double> { 0.6, 0.6, 0.5, 0.7 }));
            this.voltageMock.Setup(o => o.Apply(It.IsAny<List<double>>())); // added to deal with rounding errors.
            this.voltageMock.Setup(o => o.ApplyCondition());
            this.voltageMock.Setup(o => o.Reset());
            this.voltageServiceMock.Setup(v => v.CreateFivrForDomainsAndCondition(new List<string> { "CORE0", "CORE1", "CORE2", "CORE3" }, "NOM", "FakePlist")).Returns(this.voltageMock.Object);

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("g1234567");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.SetPinMask(new List<string> { "NOAB_02", "NOAB_03" }));
            funcTestMock.Setup(o => o.SetPinMask(new List<string> { "NOAB_00", "NOAB_01" }));
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false)
                .Returns(true)
                .Returns(true);

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData>());

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("0.600_0.600_0.500_0.700|0.500_0.600_0.500_0.700|0.900_0.900_0.900_0.900|3"));
            strValWriterMock.Setup(o => o.SetData("1234567^na^na^na"));
            strValWriterMock.Setup(o => o.SetData("1_0_0_0"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_it"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_lp"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            var dieRecoveryMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.SetupSequence(o => o.GetMaskBits())
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));
            dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(4, false), new BitArray(4, false), new BitArray(4, false), UpdateMode.Merge, true)).Returns(true);
            dieRecoveryServiceMock.Setup(o => o.Get("TCORE0,TCORE1,TCORE2,TCORE3")).Returns(dieRecoveryMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(new BitArray(new[] { true, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.Restore());
            pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            pinMapServiceMock.Setup(o => o.Get("PCORE0,PCORE1,PCORE2,PCORE3")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            // shared storage mocks for the start/end parameters.
            var sharedStorageMock = new Mock<Prime.SharedStorageService.ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("Highest", 0.7, Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                RecoveryMode = VminTC.RecoveryModes.Default,
                VoltageTargets = "CORE0,CORE1,CORE2,CORE3",
                RecoveryTrackingIncoming = "TCORE0,TCORE1,TCORE2,TCORE3",
                RecoveryTrackingOutgoing = "TCORE0,TCORE1,TCORE2,TCORE3",
                PinMap = "PCORE0,PCORE1,PCORE2,PCORE3",
                CornerIdentifiers = string.Empty,
                StartVoltages = "0.5,0.6,0.5,0.7",
                EndVoltageLimits = "0.9,0.9,0.9,0.9",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_on,print_per_target_increments",
                FivrCondition = "NOM",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 1,
                MultiPassMasks = "0011,1100",
                PatternNameMap = "1,2,3,4,5,6,7",
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                VminResult = "Highest",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(3));
            ituffMock.VerifyAll();
            strValWriterMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_AtomModule_Pass()
        {
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
            var vForcePinaAttributeMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            vForcePinaAttributeMock.Setup(o => o.Apply(new List<double> { -9999, 0.6 }));
            vForcePinaAttributeMock.Setup(o => o.Apply(new List<double> { 0.5, 0.6 }));
            vForcePinaAttributeMock.Setup(o => o.Apply(new List<double> { 0.7, 0.6 }));
            vForcePinaAttributeMock.Setup(o => o.Apply(new List<double> { 0.9, 0.6 }));
            vForcePinaAttributeMock.Setup(o => o.Restore());
            vForcePinaAttributeMock.Setup(o => o.Reset());
            this.voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "M0_HC", "M1_HC" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(vForcePinaAttributeMock.Object);

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("g1234567");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.SetPinMask(new List<string> { "M0_RO" }));
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false)
                .Returns(false)
                .Returns(true)
                .Returns(true)
                .Returns(true);

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
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
            strValWriterMock.Setup(o => o.SetData("-9999_0.600|0.500_0.600|0.900_0.900|5"));
            strValWriterMock.Setup(o => o.SetData("1234567^na"));
            strValWriterMock.Setup(o => o.SetData("3_0"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_it"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_lp"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            var dieRecoveryMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.SetupSequence(o => o.GetMaskBits())
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(new[] { true, true, true, true, false, false, false, false }), new BitArray(8, false), new BitArray(new[] { true, true, true, true, false, false, false, false }), UpdateMode.Merge, true)).Returns(true);
            dieRecoveryServiceMock.Setup(o => o.Get("M0_TRACKER,M1_TRACKER")).Returns(dieRecoveryMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(new BitArray(new[] { true, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { true, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { true, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            pinMapMock.SetupSequence(o => o.FailTrackerToFailVoltageDomains(It.IsAny<BitArray>()))
                .Returns(new BitArray(new[] { true, false }))
                .Returns(new BitArray(new[] { true, false }))
                .Returns(new BitArray(new[] { true, false }))
                .Returns(new BitArray(new[] { false, false }))
                .Returns(new BitArray(new[] { false, false }));
            var pinMapDecoder = new Mock<IPinMapDecoder>(MockBehavior.Strict);
            pinMapDecoder.Setup(o => o.NumberOfTrackerElements).Returns(4);
            pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { pinMapDecoder.Object });
            pinMapMock.Setup(o => o.VoltageDomainsToFailTracker(new BitArray(new[] { true, false })))
                .Returns(new BitArray(new[] { true, true, true, true, false, false, false, false }));
            pinMapMock.Setup(o => o.VoltageDomainsToFailTracker(new BitArray(new[] { false, false })))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { false, false, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { false, false, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.Restore());
            pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            pinMapServiceMock.Setup(o => o.Get("M0_PINMAP,M1_PINMAP")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("M0_Result", -9999D, Context.DUT));
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("M1_Result", 0.6, Context.DUT));

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                RecoveryMode = VminTC.RecoveryModes.RecoveryPort,
                VoltageTargets = "M0_HC,M1_HC",
                RecoveryTrackingIncoming = "M0_TRACKER,M1_TRACKER",
                RecoveryTrackingOutgoing = "M0_TRACKER,M1_TRACKER",
                PinMap = "M0_PINMAP,M1_PINMAP",
                CornerIdentifiers = string.Empty,
                StartVoltages = "0.5,0.6",
                EndVoltageLimits = "0.9,0.9",
                StepSize = 0.200,
                FeatureSwitchSettings = "print_per_target_increments",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 1,
                MultiPassMasks = "00110011,11001100",
                RecoveryOptions = "00000000,11110000",
                PatternNameMap = "1,2,3,4,5,6,7",
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                VminResult = "M0_Result,M1_Result",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(5));
            ituffMock.VerifyAll();
            strValWriterMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            this.sharedStorageMock.VerifyAll();
            testCondition.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            pinMapDecoder.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            vForcePinaAttributeMock.VerifyAll();
            m0Pin.VerifyAll();
            m1Pin.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_Tracker0sAllNegativeVoltages_Pass()
        {
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
            var vForcePinaAttributeMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            this.voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "M0_HC", "M1_HC" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(vForcePinaAttributeMock.Object);

            // Mock the functional test service.
            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock DDGs vminforwarding.
            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            var dieRecoveryMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            dieRecoveryMock.Setup(o => o.LogTrackingStructure(new BitArray(8, true), new BitArray(8, false)));
            dieRecoveryServiceMock.Setup(o => o.Get("M0_TRACKER,M1_TRACKER")).Returns(dieRecoveryMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            pinMapServiceMock.Setup(o => o.Get("M0_PINMAP,M1_PINMAP")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("M0_Result", -8888D, Context.DUT));
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("M1_Result", -8888D, Context.DUT));

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                RecoveryMode = VminTC.RecoveryModes.RecoveryPort,
                VoltageTargets = "M0_HC,M1_HC",
                RecoveryTrackingIncoming = "M0_TRACKER,M1_TRACKER",
                RecoveryTrackingOutgoing = "M0_TRACKER,M1_TRACKER",
                PinMap = "M0_PINMAP,M1_PINMAP",
                CornerIdentifiers = string.Empty,
                StartVoltages = "-9999,-8888",
                EndVoltageLimits = "0.9,0.9",
                StepSize = 0.200,
                FeatureSwitchSettings = "print_per_target_increments",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                FailCaptureCount = 1,
                MultiPassMasks = "00110011,11001100",
                RecoveryOptions = "00000000,11110000",
                PatternNameMap = "1,2,3,4,5,6,7",
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                VminResult = "M0_Result,M1_Result",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            this.sharedStorageMock.VerifyAll();
            testCondition.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            vForcePinaAttributeMock.VerifyAll();
            m0Pin.VerifyAll();
            m1Pin.VerifyAll();
        }

        /// <summary>
        /// Do a full execution of VminTC in MultiVmin mode in ATOM module configuration.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_AtomModuleNoRecocovery_Fail()
        {
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
            var vForcePinaAttributeMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            vForcePinaAttributeMock.Setup(o => o.Apply(new List<double> { -9999, 0.6 }));
            vForcePinaAttributeMock.Setup(o => o.Apply(new List<double> { 0.5, 0.6 }));
            vForcePinaAttributeMock.Setup(o => o.Apply(new List<double> { 0.7, 0.6 }));
            vForcePinaAttributeMock.Setup(o => o.Apply(new List<double> { 0.9, 0.6 }));
            vForcePinaAttributeMock.Setup(o => o.Restore());
            vForcePinaAttributeMock.Setup(o => o.Reset());
            this.voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "M0_HC", "M1_HC" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(vForcePinaAttributeMock.Object);

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("g1234567");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.SetPinMask(new List<string> { "M0_RO" }));
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false)
                .Returns(false)
                .Returns(true)
                .Returns(true)
                .Returns(true);

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
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
            strValWriterMock.Setup(o => o.SetData("-9999_0.600|0.500_0.600|0.900_0.900|5"));
            strValWriterMock.Setup(o => o.SetData("1234567^na"));
            strValWriterMock.Setup(o => o.SetData("3_0"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_it"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_lp"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            var dieRecoveryMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.SetupSequence(o => o.GetMaskBits())
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            dieRecoveryMock.Setup(o => o.LogTrackingStructure(new BitArray(new[] { false, false, false, false, false, false, false, false }), new BitArray(new[] { true, true, true, true, false, false, false, false })));
            dieRecoveryServiceMock.Setup(o => o.Get("M0_TRACKER,M1_TRACKER")).Returns(dieRecoveryMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var asFunctional = funcTestMock.As<IFunctionalTest>().Object;
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(new BitArray(new[] { true, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { true, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { true, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            pinMapMock.SetupSequence(o => o.FailTrackerToFailVoltageDomains(It.IsAny<BitArray>()))
                .Returns(new BitArray(new[] { true, false }))
                .Returns(new BitArray(new[] { true, false }))
                .Returns(new BitArray(new[] { true, false }))
                .Returns(new BitArray(new[] { false, false }))
                .Returns(new BitArray(new[] { false, false }));
            var pinMapDecoder = new Mock<IPinMapDecoder>(MockBehavior.Strict);
            pinMapDecoder.Setup(o => o.NumberOfTrackerElements).Returns(4);
            pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { pinMapDecoder.Object });
            pinMapMock.Setup(o => o.VoltageDomainsToFailTracker(new BitArray(new[] { true, false })))
                .Returns(new BitArray(new[] { true, true, true, true, false, false, false, false }));
            pinMapMock.Setup(o => o.VoltageDomainsToFailTracker(new BitArray(new[] { false, false })))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { false, false, true, true, false, false, true, true }), ref asFunctional, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { false, false, true, true, false, false, true, true }), ref asFunctional));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, false, false, true, true }), ref asFunctional, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, false, false, true, true }), ref asFunctional));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref asFunctional, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref asFunctional));
            pinMapMock.Setup(o => o.Restore());
            pinMapMock.Setup(o => o.Verify(ref asFunctional));
            pinMapServiceMock.Setup(o => o.Get("M0_PINMAP,M1_PINMAP")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("M0_Result", -9999D, Context.DUT));
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("M1_Result", 0.6, Context.DUT));

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                RecoveryMode = VminTC.RecoveryModes.NoRecovery,
                VoltageTargets = "M0_HC,M1_HC",
                RecoveryTrackingIncoming = "M0_TRACKER,M1_TRACKER",
                RecoveryTrackingOutgoing = "M0_TRACKER,M1_TRACKER",
                PinMap = "M0_PINMAP,M1_PINMAP",
                CornerIdentifiers = string.Empty,
                StartVoltages = "0.5,0.6",
                EndVoltageLimits = "0.9,0.9",
                StepSize = 0.200,
                FeatureSwitchSettings = "print_per_target_increments",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 1,
                MultiPassMasks = "00110011,11001100",
                RecoveryOptions = "00000000,11110000",
                PatternNameMap = "1,2,3,4,5,6,7",
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                VminResult = "M0_Result,M1_Result",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(2, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(5));
            ituffMock.VerifyAll();
            strValWriterMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            this.sharedStorageMock.VerifyAll();
            testCondition.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            pinMapDecoder.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            vForcePinaAttributeMock.VerifyAll();
            m0Pin.VerifyAll();
            m1Pin.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Do a full execution of VminTC in MultiVmin mode in ATOM module configuration.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_AtomModule_AllMasked()
        {
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
            this.voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "M0_HC", "M1_HC" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(vForcePinAttributeMock.Object);

            // Mock the functional test service.
            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock DDGs vminforwarding.
            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            var dieRecoveryMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(new[] { true, true, true, true, true, true, true, true }));
            dieRecoveryMock.Setup(o => o.LogTrackingStructure(new BitArray(8, true), new BitArray(8, false)));
            dieRecoveryServiceMock.Setup(o => o.Get("M0_TRACKER,M1_TRACKER")).Returns(dieRecoveryMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            var asFunctional = funcTestMock.As<IFunctionalTest>().Object;
            pinMapMock.Setup(o => o.Verify(ref asFunctional));
            var pinMapDecoder = new Mock<IPinMapDecoder>(MockBehavior.Strict);
            pinMapServiceMock.Setup(o => o.Get("M0_PINMAP,M1_PINMAP")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("M0_Result", -8888D, Context.DUT));
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("M1_Result", -8888D, Context.DUT));

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                RecoveryMode = VminTC.RecoveryModes.RecoveryPort,
                VoltageTargets = "M0_HC,M1_HC",
                RecoveryTrackingIncoming = "M0_TRACKER,M1_TRACKER",
                RecoveryTrackingOutgoing = "M0_TRACKER,M1_TRACKER",
                PinMap = "M0_PINMAP,M1_PINMAP",
                CornerIdentifiers = string.Empty,
                StartVoltages = "0.5,0.6",
                EndVoltageLimits = "0.9,0.9",
                StepSize = 0.200,
                FeatureSwitchSettings = "print_per_target_increments",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                FailCaptureCount = 1,
                MultiPassMasks = "00110011,11001100",
                RecoveryOptions = "00000000,11110000",
                PatternNameMap = "1,2,3,4,5,6,7",
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                VminResult = "M0_Result,M1_Result",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            this.sharedStorageMock.VerifyAll();
            testCondition.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            pinMapDecoder.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            vForcePinAttributeMock.VerifyAll();
            m0Pin.VerifyAll();
            m1Pin.VerifyAll();
        }

        /// <summary>
        /// Do a full execution of VminTC in MultiVmin mode in ATOM per-core configuration.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_AtomPerCore_Pass()
        {
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
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { 0.5, 0.5, -8888, -8888, 0.6, 0.6, -8888, -8888 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { 0.7, 0.7, -8888, -8888, 0.6, 0.6, -8888, -8888 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { 0.9, 0.9, -8888, -8888, 0.6, 0.6, -8888, -8888 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -9999, 0.9, -8888, -8888, 0.6, 0.6, -8888, -8888 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, -8888, -8888, 0.6, 0.6 }));
            vForcePinAttributeMock.Setup(o => o.Restore());
            vForcePinAttributeMock.Setup(o => o.Reset());
            this.voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "M0_HC", "M0_HC", "M0_HC", "M0_HC", "M1_HC", "M1_HC", "M1_HC", "M1_HC" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(vForcePinAttributeMock.Object);

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("g1234567");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.SetPinMask(new List<string> { "M0_RO" }));
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false)
                .Returns(false)
                .Returns(true)
                .Returns(true)
                .Returns(true);

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData>());

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("-9999_0.900_-8888_-8888_0.600_0.600_0.600_0.600|0.500_0.500_0.500_0.500_0.600_0.600_0.600_0.600|0.900_0.900_0.900_0.900_0.900_0.900_0.900_0.900|5"));
            strValWriterMock.Setup(o => o.SetData("1234567^1234567^na^na^na^na^na^na"));
            strValWriterMock.Setup(o => o.SetData("3_0_0_0_0_0_0_0"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_it"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_lp"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            var dieRecoveryMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.SetupSequence(o => o.GetMaskBits())
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(new[] { true, true, true, true, false, false, false, false }), new BitArray(8, false), new BitArray(new[] { true, false, false, false, false, false, false, false }), UpdateMode.Merge, true)).Returns(true);
            dieRecoveryServiceMock.Setup(o => o.Get("M0_TRACKER,M1_TRACKER")).Returns(dieRecoveryMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(new BitArray(new[] { true, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { true, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { true, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            var funcTest = funcTestMock.As<IFunctionalTest>().Object;
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { false, false, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { false, false, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, false, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, false, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.Restore());
            pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            pinMapServiceMock.Setup(o => o.Get("M0_PINMAP,M1_PINMAP")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

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
                RecoveryTrackingIncoming = "M0_TRACKER,M1_TRACKER",
                RecoveryTrackingOutgoing = "M0_TRACKER,M1_TRACKER",
                PinMap = "M0_PINMAP,M1_PINMAP",
                CornerIdentifiers = string.Empty,
                StartVoltages = "0.5,0.5,0.5,0.5,0.6,0.6,0.6,0.6",
                EndVoltageLimits = "0.9,0.9,0.9,0.9,0.9,0.9,0.9,0.9",
                StepSize = 0.200,
                FeatureSwitchSettings = "print_per_target_increments",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 1,
                MultiPassMasks = "00110011,11001100",
                RecoveryOptions = "00000000,11110000",
                PatternNameMap = "1,2,3,4,5,6,7",
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                VminResult = string.Empty,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(5));
            ituffMock.VerifyAll();
            strValWriterMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            testCondition.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            vForcePinAttributeMock.VerifyAll();
            m0Pin.VerifyAll();
            m1Pin.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Do a full execution of VminTC in MultiVmin mode in ATOM per-core configuration. Failing first pass.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_AtomPerCore_Port3()
        {
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
            var vForcePinAttribute = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            vForcePinAttribute.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, -8888, -8888, 0.5, 0.5 }));
            vForcePinAttribute.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, -8888, -8888, 0.51, 0.51 }));
            vForcePinAttribute.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, -8888, -8888, 0.52, 0.52 }));
            vForcePinAttribute.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, 0.5, 0.5, -8888, -8888 }));
            vForcePinAttribute.Setup(o => o.Restore());
            vForcePinAttribute.Setup(o => o.Reset());
            this.voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "M0_HC", "M0_HC", "M0_HC", "M0_HC", "M1_HC", "M1_HC", "M1_HC", "M1_HC" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(vForcePinAttribute.Object);

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
                .Returns(false)
                .Returns(true)
                .Returns(true);

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData>());

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("-8888_-8888_-8888_-8888_0.500_0.500_-9999_-9999|0.500_0.500_0.500_0.500_0.500_0.500_0.500_0.500|0.520_0.520_0.520_0.520_0.520_0.520_0.520_0.520|4"));
            strValWriterMock.Setup(o => o.SetData("0_0_0_0_0_0_3_3"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_it"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            var dieRecoveryMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.SetupSequence(o => o.GetMaskBits())
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(new[] { false, false, false, false, false, false, true, true }), new BitArray(8, false), new BitArray(new[] { false, false, false, false, false, false, true, true }), UpdateMode.Merge, true)).Returns(true);
            dieRecoveryServiceMock.Setup(o => o.Get("M0C01_TRACKER,M0C23_TRACKER,M1C01_TRACKER,M1C23_TRACKER")).Returns(dieRecoveryMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            var funcTest = funcTestMock.As<IFunctionalTest>().Object;
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.Restore());
            pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            pinMapServiceMock.Setup(o => o.Get("ATOM_MAP")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

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
                FeatureSwitchSettings = "print_per_target_increments",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                FailCaptureCount = 1,
                MultiPassMasks = "11111100,11110011",
                RecoveryOptions = "00000000,00001100,00000011",
                PatternNameMap = string.Empty,
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                VminResult = string.Empty,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(3, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(4));
            ituffMock.VerifyAll();
            strValWriterMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            testCondition.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            vForcePinAttribute.VerifyAll();
            m0Pin.VerifyAll();
            m1Pin.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Do a full execution of VminTC in MultiVmin mode in ATOM per-core configuration. Failing first pass.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_AtomPerCoreUpdateAlways_Port0()
        {
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
            var vForcePinAttribute = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            vForcePinAttribute.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, -8888, -8888, 0.5, 0.5 }));
            vForcePinAttribute.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, -8888, -8888, 0.51, 0.51 }));
            vForcePinAttribute.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, -8888, -8888, 0.52, 0.52 }));
            vForcePinAttribute.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, 0.5, 0.5, -8888, -8888 }));
            vForcePinAttribute.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, 0.51, 0.51, -8888, -8888 }));
            vForcePinAttribute.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, 0.52, 0.52, -8888, -8888 }));
            vForcePinAttribute.Setup(o => o.Restore());
            vForcePinAttribute.Setup(o => o.Reset());
            this.voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "M0_HC", "M0_HC", "M0_HC", "M0_HC", "M1_HC", "M1_HC", "M1_HC", "M1_HC" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(vForcePinAttribute.Object);

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
            funcTestMock.Setup(o => o.Execute()).Returns(false);

            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object });

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("-8888_-8888_-8888_-8888_-9999_-9999_-9999_-9999|0.500_0.500_0.500_0.500_0.500_0.500_0.500_0.500|0.520_0.520_0.520_0.520_0.520_0.520_0.520_0.520|6"));
            strValWriterMock.Setup(o => o.SetData("0_0_0_0_3_3_3_3"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_it"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            var dieRecoveryMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.Setup(o => o.GetMaskBits())
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(new[] { false, false, false, false, true, true, true, true }), new BitArray(8, false), new BitArray(new[] { false, false, false, false, true, true, true, true }), UpdateMode.Merge, true)).Returns(true);
            dieRecoveryServiceMock.Setup(o => o.Get("M0C01_TRACKER,M0C23_TRACKER,M1C01_TRACKER,M1C23_TRACKER")).Returns(dieRecoveryMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, false, false, true, true, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, true, true, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, true, true, false, false }));
            var funcTest = funcTestMock.As<IFunctionalTest>().Object;
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.Restore());
            pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            pinMapServiceMock.Setup(o => o.Get("ATOM_MAP")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                RecoveryMode = VminTC.RecoveryModes.RecoveryLoop,
                VoltageTargets = "M0_HC,M0_HC,M0_HC,M0_HC,M1_HC,M1_HC,M1_HC,M1_HC",
                RecoveryTrackingIncoming = "M0C01_TRACKER,M0C23_TRACKER,M1C01_TRACKER,M1C23_TRACKER",
                RecoveryTrackingOutgoing = "M0C01_TRACKER,M0C23_TRACKER,M1C01_TRACKER,M1C23_TRACKER",
                PinMap = "ATOM_MAP",
                CornerIdentifiers = string.Empty,
                StartVoltages = "0.5,0.5,0.5,0.5,0.5,0.5,0.5,0.5",
                EndVoltageLimits = "0.52,0.52,0.52,0.52,0.52,0.52,0.52,0.52",
                StepSize = 0.01,
                FeatureSwitchSettings = "print_per_target_increments,recovery_update_always",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                FailCaptureCount = 1,
                MultiPassMasks = "11111100,11110011",
                RecoveryOptions = "00000000,00001100,00000011",
                PatternNameMap = string.Empty,
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                VminResult = string.Empty,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(6));
            ituffMock.VerifyAll();
            strValWriterMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            testCondition.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            vForcePinAttribute.VerifyAll();
            m0Pin.VerifyAll();
            m1Pin.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_MultiPassRecoveryLoop_Pass()
        {
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
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, -8888, -8888, 0.5, 0.5 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, -8888, -8888, 0.51, 0.51 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, -8888, -8888, 0.52, 0.52 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, 0.5, 0.5, -8888, -8888 }));
            vForcePinAttributeMock.Setup(o => o.Restore());
            vForcePinAttributeMock.Setup(o => o.Reset());
            this.voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "M0_HC", "M0_HC", "M0_HC", "M0_HC", "M1_HC", "M1_HC", "M1_HC", "M1_HC" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(vForcePinAttributeMock.Object);

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
                .Returns(false)
                .Returns(false)
                .Returns(true)
                .Returns(true);

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object })
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData>())
                .Returns(new List<Prime.FunctionalService.IFailureData>());

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("-8888_-8888_-8888_-8888_0.500_0.500_-9999_-9999|0.500_0.500_0.500_0.500_0.500_0.500_0.500_0.500|0.520_0.520_0.520_0.520_0.520_0.520_0.520_0.520|4"));
            strValWriterMock.Setup(o => o.SetData("0_0_0_0_0_0_3_3"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_it"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            var dieRecoveryMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.SetupSequence(o => o.GetMaskBits())
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(new[] { false, false, false, false, false, false, true, true }), new BitArray(8, false), new BitArray(new[] { false, false, false, false, false, false, true, true }), UpdateMode.Merge, true)).Returns(true);
            dieRecoveryServiceMock.Setup(o => o.Get("M0C01_TRACKER,M0C23_TRACKER,M1C01_TRACKER,M1C23_TRACKER")).Returns(dieRecoveryMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.Restore());
            pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            pinMapServiceMock.Setup(o => o.Get("ATOM_MAP")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                RecoveryMode = VminTC.RecoveryModes.RecoveryLoop,
                VoltageTargets = "M0_HC,M0_HC,M0_HC,M0_HC,M1_HC,M1_HC,M1_HC,M1_HC",
                RecoveryTrackingIncoming = "M0C01_TRACKER,M0C23_TRACKER,M1C01_TRACKER,M1C23_TRACKER",
                RecoveryTrackingOutgoing = "M0C01_TRACKER,M0C23_TRACKER,M1C01_TRACKER,M1C23_TRACKER",
                PinMap = "ATOM_MAP",
                CornerIdentifiers = string.Empty,
                StartVoltages = "0.5,0.5,0.5,0.5,0.5,0.5,0.5,0.5",
                EndVoltageLimits = "0.52,0.52,0.52,0.52,0.52,0.52,0.52,0.52",
                StepSize = 0.01,
                FeatureSwitchSettings = "print_per_target_increments",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                FailCaptureCount = 1,
                MultiPassMasks = "11111100,11110011",
                RecoveryOptions = "00000000,00001100,00000011",
                PatternNameMap = string.Empty,
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                VminResult = string.Empty,
                MaxRepetitionCount = 2,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(4));
            ituffMock.VerifyAll();
            strValWriterMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            testCondition.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            vForcePinAttributeMock.VerifyAll();
            m0Pin.VerifyAll();
            m1Pin.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void MultiVminExecute_MultiPassRecoveryFailRetest_Pass()
        {
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
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -8888, 0.5, 0.5, 0.5, -8888, -8888, 0.5, 0.5 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -8888, 0.51, 0.51, 0.51, -8888, -8888, 0.51, 0.51 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -8888, 0.51, 0.51, 0.51, -8888, -8888, 0.52, 0.52 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -8888, 0.51, 0.51, 0.51, -8888, -8888, -9999, -9999 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -8888, -8888, -8888, -8888, -8888, -8888, 0.5, 0.5 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -8888, -8888, 0.5, 0.5, 0.5, 0.5, -8888, -8888 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -8888, -8888, 0.5, 0.5, 0.51, 0.51, -8888, -8888 }));
            vForcePinAttributeMock.Setup(o => o.Apply(new List<double> { -8888, -8888, 0.5, 0.5, 0.52, 0.52, -8888, -8888 }));
            vForcePinAttributeMock.Setup(o => o.Restore());
            vForcePinAttributeMock.Setup(o => o.Reset());
            this.voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "M0_HC", "M0_HC", "M0_HC", "M0_HC", "M1_HC", "M1_HC", "M1_HC", "M1_HC" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(vForcePinAttributeMock.Object);

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
                .Returns(false)
                .Returns(false)
                .Returns(true)
                .Returns(true)
                .Returns(false)
                .Returns(false)
                .Returns(true);

            funcTestMock.Setup(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object });

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("-8888_0.510_0.500_0.500_0.520_0.520_0.500_0.500|0.500_0.500_0.500_0.500_0.500_0.500_0.500_0.500|0.520_0.520_0.520_0.520_0.520_0.520_0.520_0.520|8"));
            strValWriterMock.Setup(o => o.SetData("0_1_0_0_0_2_0_0"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_it"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // Mock DDGs DieRecovery
            var dieRecoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            var dieRecoveryMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.Setup(o => o.GetMaskBits())
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(new[] { false, false, false, false, false, false, false, false }), new BitArray(new[] { true, false, false, false, false, false, false, false }), new BitArray(new[] { false, false, false, false, false, false, false, false }), UpdateMode.Merge, true)).Returns(true);
            dieRecoveryServiceMock.Setup(o => o.Get("M0C01_TRACKER,M0C23_TRACKER,M1C01_TRACKER,M1C23_TRACKER")).Returns(dieRecoveryMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;
            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(new BitArray(new[] { false, true, false, false, false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, true, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, true, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false, false, false, false, false }));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, false, false, false, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, false, false, false, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, false, false, false, true, true, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, false, false, false, true, true, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, true, true, true, true, false, false }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, true, false, false, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, true, false, false, false, false, true, true }), ref It.Ref<IFunctionalTest>.IsAny));
            pinMapMock.Setup(o => o.Restore());
            pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            pinMapServiceMock.Setup(o => o.Get("ATOM_MAP")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            // Build the Vmin Instance to test.
            var instanceToTest = new VminTC
            {
                // Base Parameters.
                InstanceName = "DummyInstance",
                Patlist = "FakePlist",
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                RecoveryMode = VminTC.RecoveryModes.RecoveryFailRetest,
                VoltageTargets = "M0_HC,M0_HC,M0_HC,M0_HC,M1_HC,M1_HC,M1_HC,M1_HC",
                RecoveryTrackingIncoming = "M0C01_TRACKER,M0C23_TRACKER,M1C01_TRACKER,M1C23_TRACKER",
                RecoveryTrackingOutgoing = "M0C01_TRACKER,M0C23_TRACKER,M1C01_TRACKER,M1C23_TRACKER",
                PinMap = "ATOM_MAP",
                CornerIdentifiers = string.Empty,
                StartVoltages = "0.5,0.5,0.5,0.5,0.5,0.5,0.5,0.5",
                EndVoltageLimits = "0.52,0.52,0.52,0.52,0.52,0.52,0.52,0.52",
                StepSize = 0.01,
                FeatureSwitchSettings = "print_per_target_increments",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                FailCaptureCount = 1,
                MultiPassMasks = "00001100,11000011",
                RecoveryOptions = "00000000",
                InitialMaskBits = "10000000",
                PatternNameMap = string.Empty,
                TestMode = VminTC.TestModes.MultiVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                VminResult = string.Empty,
                MaxRepetitionCount = 2,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(8));
            ituffMock.VerifyAll();
            strValWriterMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            dieRecoveryServiceMock.VerifyAll();
            testCondition.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            vForcePinAttributeMock.VerifyAll();
            m0Pin.VerifyAll();
            m1Pin.VerifyAll();
            failDataMock.VerifyAll();
        }
    }
}
