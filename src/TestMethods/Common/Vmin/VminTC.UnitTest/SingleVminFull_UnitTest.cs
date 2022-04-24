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
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FunctionalService;
    using Prime.PerformanceService;
    using Prime.PinService;
    using Prime.PlistService;
    using Prime.SharedStorageService;
    using Prime.TestConditionService;
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="SingleVminFull_UnitTest" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SingleVminFull_UnitTest
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IPlistService> plistServiceMock;
        private Mock<IPlistObject> plistObjectMock;
        private Mock<IPerformanceService> performanceServiceMock;

        /// <summary>
        /// Mock initialization.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            // Mock the console service.
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
                System.Console.WriteLine($"DEBUG: {msg}"));
            this.consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) => System.Console.WriteLine($"DEBUG: {msg}"));
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            this.plistObjectMock = new Mock<IPlistObject>(MockBehavior.Loose);
            this.plistServiceMock = new Mock<IPlistService>(MockBehavior.Loose);
            this.plistServiceMock.Setup(o => o.GetPlistObject(It.IsAny<string>())).Returns(this.plistObjectMock.Object);
            Prime.Services.PlistService = this.plistServiceMock.Object;

            this.performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = this.performanceServiceMock.Object;
        }

        /// <summary>
        /// Do a full execution of VminTC in SingleVmin mode using VminForwarding.
        /// </summary>
        [TestMethod]
        public void SingleVminExecute_Full_Pass()
        {
            // Mock the voltage service.
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Apply(new List<double> { 0.5 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.6 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.7 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.8 })); // fails for rounding error - ([0.79999999999999993]) TODO: ???
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

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(f => f.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(f => f.GetVectorAddress()).Returns(10);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false) // 0.5
                .Returns(false) // 0.6
                .Returns(false) // 0.7
                .Returns(true); // 0.8

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.5
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.6
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.7
                .Returns(new List<Prime.FunctionalService.IFailureData>()); // 0.8

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetData("0.800|0.500|1.000|4"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);
            vminCornerMock.Setup(o => o.GetStartingVoltage(-9999)).Returns(0.5);
            vminCornerMock.Setup(o => o.StoreVminResult(It.IsAny<double>())).Returns(true); // to deal with rounding errors allow any, check with Verify.

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // shared storage mocks for the start/end parameters.
            var sharedStorageMock = new Mock<Prime.SharedStorageService.ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var plistObjectMock = new Mock<IPlistObject>(MockBehavior.Strict);
            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            Prime.Services.PlistService = plistServiceMock.Object;
            plistServiceMock.Setup(o => o.GetPlistObject("FakePlist")).Returns(plistObjectMock.Object);

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
                TestMode = VminTC.TestModes.SingleVmin,
                CornerIdentifiers = "CLR_F3",
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(4));
            failDataMock.VerifyAll();
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            vminCornerMock.Verify(o => o.StoreVminResult(It.Is<double>(actual => Math.Abs(actual - 0.8) < 0.001))); // to deal with rounding errors.
            forwardingServiceMock.VerifyAll();
            testCondition.VerifyAll();
            pinMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void SingleVminExecute_MultipleCorner_Pass()
        {
            // Mock the voltage service.
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Apply(new List<double> { 0.5 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.6 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.7 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.8 })); // fails for rounding error - ([0.79999999999999993]) TODO: ???
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

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false) // 0.5
                .Returns(false) // 0.6
                .Returns(false) // 0.7
                .Returns(true); // 0.8

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.5
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.6
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.7
                .Returns(new List<Prime.FunctionalService.IFailureData>()); // 0.8

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("SA1:1:1.500_SA2:1:1.600"));
            strValWriterMock.Setup(o => o.SetData("0.800|0.500|1.000|4"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock1 = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock1.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);
            vminCornerMock1.Setup(o => o.GetStartingVoltage(-9999)).Returns(0.5);
            vminCornerMock1.Setup(o => o.StoreVminResult(It.IsAny<double>())).Returns(true); // to deal with rounding errors allow any, check with Verify.

            var vminCornerMock2 = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock2.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);
            vminCornerMock2.Setup(o => o.GetStartingVoltage(-9999)).Returns(0.4);
            vminCornerMock2.Setup(o => o.StoreVminResult(It.IsAny<double>())).Returns(true); // to deal with rounding errors allow any, check with Verify.

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("SA1", 1)).Returns(vminCornerMock1.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("SA1", 1)).Returns(1.5e9);
            forwardingServiceMock.Setup(o => o.Get("SA2", 1)).Returns(vminCornerMock2.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("SA2", 1)).Returns(1.6e9);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // shared storage mocks for the start/end parameters.
            var sharedStorageMock = new Mock<Prime.SharedStorageService.ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var plistObjectMock = new Mock<IPlistObject>(MockBehavior.Strict);
            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            Prime.Services.PlistService = plistServiceMock.Object;
            plistServiceMock.Setup(o => o.GetPlistObject("FakePlist")).Returns(plistObjectMock.Object);

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
                TestMode = VminTC.TestModes.SingleVmin,
                CornerIdentifiers = "SA1,SA2",
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(4));
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            vminCornerMock1.Verify(o => o.StoreVminResult(It.Is<double>(actual => Math.Abs(actual - 0.8) < 0.001))); // to deal with rounding errors.
            vminCornerMock2.Verify(o => o.StoreVminResult(It.Is<double>(actual => Math.Abs(actual - 0.8) < 0.001))); // to deal with rounding errors.
            forwardingServiceMock.VerifyAll();
            testCondition.VerifyAll();
            pinMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Do a full execution of VminTC in SingleVmin mode printing per pattern data.
        /// </summary>
        [TestMethod]
        public void SingleVminExecute_PerPatternData_Pass()
        {
            // Mock the voltage service.
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Apply(new List<double> { 0.5 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.6 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.7 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.8 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.9 }));
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

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);
            failDataMock.SetupSequence(o => o.GetPatternName())
                .Returns("gxxxxxx1")
                .Returns("gxxxxxx1")
                .Returns("gxxxxxx0")
                .Returns("gxxxxxx0")
                .Returns("gxxxxxx3")
                .Returns("gxxxxxx3")
                .Returns("gxxxxxx0")
                .Returns("gxxxxxx0");
            failDataMock.SetupSequence(o => o.GetBurstIndex())
                .Returns(1)
                .Returns(1)
                .Returns(1)
                .Returns(1)
                .Returns(2)
                .Returns(2)
                .Returns(2)
                .Returns(2);
            failDataMock.SetupSequence(o => o.GetPatternInstanceId())
                .Returns(1)
                .Returns(1)
                .Returns(1)
                .Returns(1)
                .Returns(2)
                .Returns(2)
                .Returns(1)
                .Returns(1);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false) // 0.5
                .Returns(false) // 0.6
                .Returns(false) // 0.7
                .Returns(false) // 0.8
                .Returns(true); // 0.9
            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object });
            funcTestMock.SetupSequence(o => o.HasStartPattern())
                .Returns(false)
                .Returns(false)
                .Returns(true)
                .Returns(true)
                .Returns(true);
            funcTestMock.SetupSequence(o => o.GetStartPattern())
                .Returns(new Tuple<string, uint, uint>("gxxxxxx0", 1, 1))
                .Returns(new Tuple<string, uint, uint>("gxxxxxx1", 1, 1))
                .Returns(new Tuple<string, uint, uint>("gxxxxxx1", 1, 1))
                .Returns(new Tuple<string, uint, uint>("gxxxxxx3", 2, 2))
                .Returns(new Tuple<string, uint, uint>("gxxxxxx0", 2, 1));
            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("0.900|0.500|1.000|5"));
            strValWriterMock.Setup(o => o.SetData("xxxxxx0"));
            strValWriterMock.Setup(o => o.SetData("xxxxxx1:0.7|xxxxxx2:0.7|xxxxxx3:0.7|xxxxxx4:0.7|xxxxxx5:0.7|xxxxxx3:0.8|xxxxxx4:0.8|xxxxxx7:0.9"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_lp"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_pp"));
            strValWriterMock.Setup(o => o.SetDelimiterCharacterForWrap('^'));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // shared storage mocks for the start/end parameters.
            var sharedStorageMock = new Mock<Prime.SharedStorageService.ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("DummyInstance_pp", "xxxxxx1:0.7|xxxxxx2:0.7|xxxxxx3:0.7|xxxxxx4:0.7|xxxxxx5:0.7|xxxxxx3:0.8|xxxxxx4:0.8|xxxxxx7:0.9", Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var plistContentMock = new Mock<IPlistContent>(MockBehavior.Strict);
            plistContentMock.Setup(o => o.IsPattern()).Returns(true);
            plistContentMock.SetupSequence(o => o.GetBurstIndex())
                .Returns(1)
                .Returns(1)
                .Returns(1)
                .Returns(2)
                .Returns(2)
                .Returns(2)
                .Returns(2)
                .Returns(2)
                .Returns(2)
                .Returns(2);
            plistContentMock.SetupSequence(o => o.GetPatternIndex())
                .Returns(1)
                .Returns(2)
                .Returns(3)
                .Returns(1)
                .Returns(2)
                .Returns(3)
                .Returns(4)
                .Returns(5)
                .Returns(6)
                .Returns(7);
            plistContentMock.SetupSequence(o => o.GetPlistItemName())
                .Returns("gxxxxxx0")
                .Returns("gxxxxxx1")
                .Returns("gxxxxxx2")
                .Returns("gxxxxxx3")
                .Returns("gxxxxxx4")
                .Returns("gxxxxxx5")
                .Returns("gxxxxxx3")
                .Returns("gxxxxxx4")
                .Returns("gxxxxxx0")
                .Returns("gxxxxxx7");

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            Prime.Services.PlistService = plistServiceMock.Object;
            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("FakePlist")).Returns(plistMock.Object);
            plistMock.Setup(o => o.IsPatternAnAmble("gxxxxxx0")).Returns(true);
            plistMock.Setup(o => o.IsPatternAnAmble(It.IsNotIn(new[] { "gxxxxxx0" }))).Returns(false);
            plistMock.Setup(o => o.GetPatternsAndIndexes(true))
                .Returns(new List<IPlistContent> { plistContentMock.Object, plistContentMock.Object, plistContentMock.Object, plistContentMock.Object, plistContentMock.Object, plistContentMock.Object, plistContentMock.Object, plistContentMock.Object, plistContentMock.Object, plistContentMock.Object });

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
                FeatureSwitchSettings = "fivr_mode_off,per_pattern_printing",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 1,
                TestMode = VminTC.TestModes.SingleVmin,
                PatternNameMap = "1,2,3,4,5,6,7",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(5));
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            testCondition.VerifyAll();
            pinMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Do a full execution of VminTC in SingleVmin mode using VminForwarding and DieRecovery.
        /// </summary>
        [TestMethod]
        public void SingleVminExecute_DieRecovery_Pass()
        {
            // Mock the voltage service.
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Apply(new List<double> { 0.5 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.6 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.7 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.8 })); // fails for rounding error - ([0.79999999999999993]) TODO: ???
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

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false) // 0.5
                .Returns(false) // 0.6
                .Returns(false) // 0.7
                .Returns(true); // 0.8

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.5
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.6
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.7
                .Returns(new List<Prime.FunctionalService.IFailureData>()); // 0.8

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetData("0.800|0.500|1.000|4"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);
            vminCornerMock.Setup(o => o.GetStartingVoltage(-9999)).Returns(0.5);
            vminCornerMock.Setup(o => o.StoreVminResult(It.IsAny<double>())).Returns(true); // to deal with rounding errors allow any, check with Verify.

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // shared storage mocks for the start/end parameters.
            var sharedStorageMock = new Mock<Prime.SharedStorageService.ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var dieRecoveryMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, false));
            dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(4, false), new BitArray(new[] { false, false, false, false }), new BitArray(4, false), UpdateMode.Merge, true)).Returns(true);
            var dieRecoveryFactoryMock = new Mock<IDieRecoveryFactory>(MockBehavior.Strict);
            dieRecoveryFactoryMock.Setup(o => o.Get("T0,T1,T2,T3")).Returns(dieRecoveryMock.Object);
            dieRecoveryFactoryMock.Setup(o => o.AreTrackerChangesAllowed()).Returns(true);
            DDG.DieRecovery.Service = dieRecoveryFactoryMock.Object;

            var pinMapMock = new Mock<IPinMap>(MockBehavior.Default);
            pinMapMock.Setup(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null)).Returns(new BitArray(4, false));
            var pinMapFactoryMock = new Mock<IPinMapFactory>(MockBehavior.Strict);
            pinMapFactoryMock.Setup(o => o.Get("P0,P1,P2,P3")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapFactoryMock.Object;

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
                TestMode = VminTC.TestModes.SingleVmin,
                CornerIdentifiers = "CLR_F3",
                PinMap = "P0,P1,P2,P3",
                RecoveryTrackingIncoming = "T0,T1,T2,T3",
                RecoveryTrackingOutgoing = "T0,T1,T2,T3",
                RecoveryOptions = "0000",
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                RecoveryMode = VminTC.RecoveryModes.RecoveryPort,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(4));
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            vminCornerMock.Verify(o => o.StoreVminResult(It.Is<double>(actual => Math.Abs(actual - 0.8) < 0.001))); // to deal with rounding errors.
            forwardingServiceMock.VerifyAll();
            testCondition.VerifyAll();
            pinMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            pinMapMock.VerifyAll();
        }

        /// <summary>
        /// Refer top test name.
        /// </summary>
        [TestMethod]
        public void SingleVminExecute_UpdateOnPass_P0()
        {
            // Mock the voltage service.
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

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.Execute()).Returns(false);
            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object });
            funcTestMock.Setup(o => o.DatalogFailure(1));

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetData("-9999|0.500|1.000|6"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(-9999D)).Returns(0.1);
            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // shared storage mocks for the start/end parameters.
            var sharedStorageMock = new Mock<Prime.SharedStorageService.ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var dieRecoveryMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, false));
            dieRecoveryMock.Setup(o => o.LogTrackingStructure(new BitArray(new[] { false, false, false, false }), new BitArray(new[] { true, true, true, true })));
            var dieRecoveryFactoryMock = new Mock<IDieRecoveryFactory>(MockBehavior.Strict);
            dieRecoveryFactoryMock.Setup(o => o.Get("T0,T1,T2,T3")).Returns(dieRecoveryMock.Object);
            dieRecoveryFactoryMock.Setup(o => o.AreTrackerChangesAllowed()).Returns(true);
            DDG.DieRecovery.Service = dieRecoveryFactoryMock.Object;

            var pinMapMock = new Mock<IPinMap>(MockBehavior.Default);
            var pinMapDecoderMock = new Mock<IPinMapDecoder>(MockBehavior.Strict);
            pinMapDecoderMock.Setup(o => o.NumberOfTrackerElements).Returns(4);
            pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { pinMapDecoderMock.Object });
            pinMapMock.Setup(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null)).Returns(new BitArray(4, true));
            var pinMapFactoryMock = new Mock<IPinMapFactory>(MockBehavior.Strict);
            pinMapFactoryMock.Setup(o => o.Get("P0,P1,P2,P3")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapFactoryMock.Object;

            var plistObjectMock = new Mock<IPlistObject>(MockBehavior.Strict);
            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistObjectMock.Setup(o => o.IsPatternAnAmble("FailingPatternName")).Returns(false);
            Prime.Services.PlistService = plistServiceMock.Object;
            plistServiceMock.Setup(o => o.GetPlistObject("FakePlist")).Returns(plistObjectMock.Object);

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
                FeatureSwitchSettings = "fivr_mode_off,vmin_update_on_pass_only",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 1,

                // Custom Parameters.
                TestMode = VminTC.TestModes.SingleVmin,
                CornerIdentifiers = "CLR_F3",
                PinMap = "P0,P1,P2,P3",
                RecoveryTrackingIncoming = "T0,T1,T2,T3",
                RecoveryTrackingOutgoing = "T0,T1,T2,T3",
                RecoveryOptions = "0000",
                ForwardingMode = VminTC.ForwardingModes.Output,
                RecoveryMode = VminTC.RecoveryModes.Default,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.VerifyAll();
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            vminCornerMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            testCondition.VerifyAll();
            pinMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            pinMapMock.VerifyAll();
        }

        /// <summary>
        /// Refer top test name.
        /// </summary>
        [TestMethod]
        public void SingleVminExecute_UpdateOnPass_P1()
        {
            // Mock the voltage service.
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

            // Mock the functional test service.
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.Execute()).Returns(true);
            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData>());

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

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
            vminCornerMock.Setup(o => o.StoreVminResult(0.5)).Returns(true);
            vminCornerMock.Setup(o => o.GetStartingVoltage(-9999D)).Returns(0.1);
            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // shared storage mocks for the start/end parameters.
            var sharedStorageMock = new Mock<Prime.SharedStorageService.ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var dieRecoveryMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, false));
            dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(4, false), new BitArray(new[] { false, false, false, false }), new BitArray(4, false), UpdateMode.Merge, true)).Returns(true);
            var dieRecoveryFactoryMock = new Mock<IDieRecoveryFactory>(MockBehavior.Strict);
            dieRecoveryFactoryMock.Setup(o => o.Get("T0,T1,T2,T3")).Returns(dieRecoveryMock.Object);
            dieRecoveryFactoryMock.Setup(o => o.AreTrackerChangesAllowed()).Returns(true);
            DDG.DieRecovery.Service = dieRecoveryFactoryMock.Object;

            var pinMapDecoderMock = new Mock<IPinMapDecoder>(MockBehavior.Strict);
            pinMapDecoderMock.Setup(o => o.NumberOfTrackerElements).Returns(4);
            var pinMapMock = new Mock<IPinMap>(MockBehavior.Default);
            pinMapMock.Setup(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null)).Returns(new BitArray(4, false));
            pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { pinMapDecoderMock.Object });
            var pinMapFactoryMock = new Mock<IPinMapFactory>(MockBehavior.Strict);
            pinMapFactoryMock.Setup(o => o.Get("P0,P1,P2,P3")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapFactoryMock.Object;

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
                FeatureSwitchSettings = "fivr_mode_off,reset_pointers,vmin_update_on_pass_only",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                FailCaptureCount = 1,

                // Custom Parameters.
                TestMode = VminTC.TestModes.SingleVmin,
                CornerIdentifiers = "CLR_F3",
                PinMap = "P0,P1,P2,P3",
                RecoveryTrackingIncoming = "T0,T1,T2,T3",
                RecoveryTrackingOutgoing = "T0,T1,T2,T3",
                RecoveryOptions = "0000",
                ForwardingMode = VminTC.ForwardingModes.Output,
                RecoveryMode = VminTC.RecoveryModes.Default,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.VerifyAll();
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            vminCornerMock.VerifyAll();
            forwardingServiceMock.VerifyAll();
            testCondition.VerifyAll();
            pinMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            pinMapMock.VerifyAll();
        }

        /// <summary>
        /// Do a full execution of VminTC in SingleVmin mode using VminForwarding and DieRecovery.
        /// </summary>
        [TestMethod]
        public void SingleVminExecute_RecoveryLoop_Pass()
        {
            // Mock the voltage service.
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Apply(new List<double> { 0.5 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.6 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.7 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.8 })); // fails for rounding error - ([0.79999999999999993]) TODO: ???
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

            // Mock the functional test service.
            var expectedPrint = "PrintFailData --Fail data:  Plist=SomePlist Pattern=FailingPatternName Label=Label1 PinNames=SomePin Channels=1001 Vector=100.";
            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetParentPlistName()).Returns("SomePlist");
            failDataMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "SomePin" });
            failDataMock.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 1001 });
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(100);
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetPreviousLabel()).Returns("Label1");

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.DatalogFailure(1));
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false) // 0.5
                .Returns(false) // 0.6
                .Returns(false) // 0.7
                .Returns(false) // 0.8
                .Returns(false) // 0.5
                .Returns(true); // 0.6

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.5
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.5
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.6
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.6
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.7
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.7
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.8
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.8
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.5
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.5
                .Returns(new List<Prime.FunctionalService.IFailureData>()) // 0.6
                .Returns(new List<Prime.FunctionalService.IFailureData>()); // 0.6

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetData("0.600|0.500|0.800|6"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // Mock DDGs vminforwarding.
            var vminCornerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminCornerMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.5);
            vminCornerMock.Setup(o => o.GetStartingVoltage(-9999)).Returns(0.5);
            vminCornerMock.Setup(o => o.StoreVminResult(It.IsAny<double>())).Returns(true); // to deal with rounding errors allow any, check with Verify.

            var forwardingServiceMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            forwardingServiceMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminCornerMock.Object);
            forwardingServiceMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1.5e9);
            forwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(false);
            DDG.VminForwarding.Service = forwardingServiceMock.Object;

            // shared storage mocks for the start/end parameters.
            var sharedStorageMock = new Mock<Prime.SharedStorageService.ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string>());
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var dieRecoveryMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, false));
            dieRecoveryMock.Setup(o => o.UpdateTrackingStructure(new BitArray(new[] { false, false, true, true }), new BitArray(new[] { false, false, false, false }), new BitArray(new[] { false, false, false, true }), UpdateMode.Merge, true)).Returns(true);
            var dieRecoveryFactoryMock = new Mock<IDieRecoveryFactory>(MockBehavior.Strict);
            dieRecoveryFactoryMock.Setup(o => o.Get("T0,T1,T2,T3")).Returns(dieRecoveryMock.Object);
            dieRecoveryFactoryMock.Setup(o => o.AreTrackerChangesAllowed()).Returns(true);
            DDG.DieRecovery.Service = dieRecoveryFactoryMock.Object;

            var pinMapMock = new Mock<IPinMap>(MockBehavior.Strict);
            var funcTest = funcTestMock.As<IFunctionalTest>().Object;
            pinMapMock.Setup(o => o.MaskPins(It.IsAny<BitArray>(), ref funcTest, new List<string>()));
            pinMapMock.Setup(o => o.ModifyPlist(It.IsAny<BitArray>(), ref funcTest));
            pinMapMock.Setup(o => o.Restore());
            pinMapMock.SetupSequence(o => o.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns("0001".ToBitArray())
                .Returns("0001".ToBitArray())
                .Returns("0001".ToBitArray())
                .Returns("0001".ToBitArray())
                .Returns("0100".ToBitArray())
                .Returns("0000".ToBitArray());
            pinMapMock.Setup(o => o.Verify(ref funcTest));
            var pinMapFactoryMock = new Mock<IPinMapFactory>(MockBehavior.Strict);
            pinMapFactoryMock.Setup(o => o.Get("P0,P1,P2,P3")).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapFactoryMock.Object;

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
                EndVoltageLimits = "0.8",
                StepSize = 0.100,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                FailCaptureCount = 1,

                // Custom Parameters.
                TestMode = VminTC.TestModes.SingleVmin,
                CornerIdentifiers = "CLR_F3",
                PinMap = "P0,P1,P2,P3",
                RecoveryTrackingIncoming = "T0,T1,T2,T3",
                RecoveryTrackingOutgoing = "T0,T1,T2,T3",
                RecoveryOptions = "0000,0011,1100",
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                RecoveryMode = VminTC.RecoveryModes.RecoveryLoop,
                MaxRepetitionCount = 2,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(6));
            strValWriterMock.VerifyAll();
            ituffMock.VerifyAll();
            vminCornerMock.Verify(o => o.StoreVminResult(It.Is<double>(actual => Math.Abs(actual - 0.6) < 0.001))); // to deal with rounding errors.
            forwardingServiceMock.VerifyAll();
            testCondition.VerifyAll();
            pinMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
            dieRecoveryMock.VerifyAll();
            pinMapMock.VerifyAll();
            this.consoleServiceMock.Verify(o => o.PrintDebug(expectedPrint), Times.Exactly(5));
        }

        /// <summary>
        /// Do a full execution of VminTC in SingleVmin mode using VminForwarding.
        /// </summary>
        [TestMethod]
        public void SingleVminExecute_SinglePointMode_Fail()
        {
            // Mocks.
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Apply(new List<double> { 0.5 }));
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

            var vminForwardingMock = new Mock<IVminForwardingCorner>(MockBehavior.Default);
            vminForwardingMock.Setup(o => o.GetStartingVoltage(-9999)).Returns(0.4);
            vminForwardingMock.Setup(o => o.GetStartingVoltage(0.5)).Returns(0.4);
            vminForwardingMock.Setup(o => o.StoreVminResult(It.IsAny<double>()));
            var vminForwardingFactoryMock = new Mock<IVminForwardingFactory>(MockBehavior.Strict);
            vminForwardingFactoryMock.Setup(o => o.IsSinglePointMode()).Returns(true);
            vminForwardingFactoryMock.Setup(o => o.Get("CLR_F3", 1)).Returns(vminForwardingMock.Object);
            vminForwardingFactoryMock.Setup(o => o.GetFrequency("CLR_F3", 1)).Returns(1500000000);
            DDG.VminForwarding.Service = vminForwardingFactoryMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string>());
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);
            failDataMock.Setup(o => o.GetParentPlistName()).Returns("FakePList");
            failDataMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "FakePin3" });
            failDataMock.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 3 });
            failDataMock.Setup(o => o.GetPreviousLabel()).Throws(new Prime.Base.Exceptions.FatalException("Failed to find closest label."));
            var expectedPrint = "PrintFailData --Fail data:  Plist=FakePList Pattern=FailingPatternName Label=<no_label_found> PinNames=FakePin3 Channels=3 Vector=10.";

            var funcTestMock = new Mock<Prime.FunctionalService.ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist("DummyInstance")).Returns("FakePlist");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.Execute()).Returns(false); // 0.5
            funcTestMock.Setup(o => o.DatalogFailure(1));
            funcTestMock.Setup(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureTest("FakePlist", "FakeLevels", "FakeTimings", 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("CLR_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetData("-9999|0.500|1.000|1"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // shared storage mocks for the start/end parameters.
            var sharedStorageMock = new Mock<Prime.SharedStorageService.ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

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
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                FailCaptureCount = 1,
                TestMode = VminTC.TestModes.SingleVmin,
                ForwardingMode = VminTC.ForwardingModes.InputOutput,
                CornerIdentifiers = "CLR_F3",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            // Run Verify/execute.
            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());

            // Use the mocks to check that the plist was run the expected number of times and that the final Voltage was correct.
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
            ituffMock.VerifyAll();
            testCondition.VerifyAll();
            pinMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
            vminForwardingFactoryMock.VerifyAll();
            vminForwardingMock.VerifyAll();
            this.consoleServiceMock.Verify(o => o.PrintDebug(expectedPrint), Times.Once);
        }
    }
}
