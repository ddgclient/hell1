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

namespace Prime.TestMethods.VminSearch.UnitTest
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FunctionalService;
    using Prime.PinService;
    using Prime.TestConditionService;
    using Prime.TestMethods.VminSearch;
    using Prime.VoltageService;

    /// <summary>
    /// UnitTest class.
    /// </summary>
    [TestClass]
    public class VminSearchUnitTest
    {
        private Mock<IFunctionalService> funcServiceMock;
        private Mock<ICaptureFailureTest> funcTestMock;
        private Mock<IVoltageService> voltageServiceMock;
        private Mock<IVForcePinAttribute> dpsVoltageMock;
        private Mock<IDatalogService> dataLogServiceMock;
        private Mock<IStrgvalFormat> writerMock;
        private Mock<ITestConditionService> testConditionServiceMock;
        private Mock<ITestCondition> testConditionMock;
        private Mock<IPinService> pinServiceMock;
        private Mock<IPin> pinAMock;
        private Mock<IPin> pinBMock;
        private string instanceName = "test";
        private string plistName = "plist";

        /// <summary>
        /// Setups the mocks.
        /// </summary>
        [TestInitialize]
        public void SetupMockingVerify()
        {
            // Mocking for IFunctionalService
            this.funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            this.funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.funcTestMock.Setup(func => func.EnableStartPatternOnFirstFail());
            this.funcServiceMock.Setup(func => func.CreateCaptureFailureTest(this.plistName, "level", "timing", 1, "SomeCallback()")).Returns(this.funcTestMock.Object);
            Services.FunctionalService = this.funcServiceMock.Object;

            this.pinServiceMock = new Mock<IPinService>();
            this.pinAMock = new Mock<IPin>();
            this.pinBMock = new Mock<IPin>();
            var pinAAttributes = new List<string>() { "Attribute1" };
            var pinBAttributes = new List<string>() { "Attribute2" };
            this.pinAMock.Setup(mock => mock.GetVforceMandatoryAttributes()).Returns(pinAAttributes);
            this.pinBMock.Setup(mock => mock.GetVforceMandatoryAttributes()).Returns(pinBAttributes);
            this.pinServiceMock.Setup(service => service.Get("A")).Returns(this.pinAMock.Object);
            this.pinServiceMock.Setup(service => service.Get("B")).Returns(this.pinBMock.Object);
            Services.PinService = this.pinServiceMock.Object;

            this.testConditionServiceMock = new Mock<ITestConditionService>();
            this.testConditionMock = new Mock<ITestCondition>();
            this.testConditionServiceMock.Setup(service => service.GetTestCondition("level")).Returns(this.testConditionMock.Object);
            this.testConditionMock.Setup(mock => mock.GetPinAttributeValue("A", "Attribute1")).Returns("Value1");
            this.testConditionMock.Setup(mock => mock.GetPinAttributeValue("B", "Attribute2")).Returns("Value2");
            Services.TestConditionService = this.testConditionServiceMock.Object;

            // Mocking for IVoltageService
            this.voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            this.dpsVoltageMock = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            this.voltageServiceMock.Setup(service => service.CreateVForceForPinAttribute(new List<string>() { "A", "B" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(this.dpsVoltageMock.Object);
            Services.VoltageService = this.voltageServiceMock.Object;

            // Mocking for IDatalogService
            this.dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            this.writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            this.dataLogServiceMock.Setup(dataLog => dataLog.GetItuffStrgvalWriter()).Returns(this.writerMock.Object);
            this.dataLogServiceMock.Setup(dataLog => dataLog.WriteToItuff(this.writerMock.Object));
            Services.DatalogService = this.dataLogServiceMock.Object;
        }

        /// <summary>
        /// Verify test case to catch exception when Voltage targets are empty.
        /// </summary>
        [TestMethod]
        public void Verify_EmptyVoltageTargets_Throw()
        {
            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = string.Empty,
                StartVoltages = "C,D",
                EndVoltageLimits = "E,F",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                TestMethodExtension = null,
                FivrCondition = string.Empty,
            };

            // Test
            Assert.ThrowsException<Base.Exceptions.TestMethodException>(() => search.Verify());
        }

        /// <summary>
        /// Verify test case to catch exception when ScoreBoard enabled but ScoreBoard base numbers are empty.
        /// </summary>
        [TestMethod]
        public void Verify_ScoreBoardModeEnabledNoBaseNumbers_Throw()
        {
            this.SetupMockingVerify();

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "C,D",
                EndVoltageLimits = "E,F",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                TestMethodExtension = null,
                FivrCondition = string.Empty,
                ExecutionMode = PrimeVminSearchTestMethod.ExecutionModeFlag.SearchWithScoreboard,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            Assert.ThrowsException<Base.Exceptions.TestMethodException>(() => search.Verify());
        }

        /// <summary>
        /// Start Voltages does Not Match Expected Count (2 targets vs 3 start voltage keys).
        /// </summary>
        [TestMethod]
        public void Verify_StartVoltagesDoesNotMatchExpectedCount_Throw()
        {
            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "C,D,X",
                EndVoltageLimits = "E,F",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                TestMethodExtension = null,
                FivrCondition = string.Empty,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            Assert.ThrowsException<Base.Exceptions.TestMethodException>(() => search.Verify());
        }

        /// <summary>
        /// End Voltages limits does Not Match Expected Count (2 targets vs 3 start voltage keys).
        /// </summary>
        [TestMethod]
        public void Verify_EndVoltageLimitsDoesNotMatchExpectedCount_Throw()
        {
            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "C,D",
                EndVoltageLimits = "E,F,X",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                TestMethodExtension = null,
                FivrCondition = string.Empty,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            Assert.ThrowsException<Base.Exceptions.TestMethodException>(() => search.Verify());
        }

        /// <summary>
        /// Lower start voltage limits does Not Match Expected Count (2 targets vs 3 start voltage keys).
        /// </summary>
        [TestMethod]
        public void Verify_LowerStartVoltageLimitsDoesNotMatchExpectedCount_Throw()
        {
            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "C,D",
                EndVoltageLimits = "E,F",
                StartVoltagesForRetry = "X,Y,Z",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                TestMethodExtension = null,
                FivrCondition = string.Empty,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            Assert.ThrowsException<Base.Exceptions.TestMethodException>(() => search.Verify());
        }

        /// <summary>
        /// Verify test case to catch exception when StepSize is 0.
        /// </summary>
        [TestMethod]
        public void Verify_StepSizeZero_Throw()
        {
            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "0.4,0.4",
                EndVoltageLimits = "0.6,0.6",
                StepSize = 0,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                MultiPassMasks = string.Empty,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            Assert.ThrowsException<Base.Exceptions.TestMethodException>(() => search.Verify());
            this.voltageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Generic verify pass test case.
        /// </summary>
        [TestMethod]
        public void Verify_AllValidParameters_Pass()
        {
            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "C,D",
                EndVoltageLimits = "E,F",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                MultiPassMasks = string.Empty,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            search.Verify();
            this.voltageServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
        }

        /// <summary>
        /// Execute simple happy path for plist always failing.
        /// </summary>
        [TestMethod]
        public void Execute_SimpleAllFailCase_Return0()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.ApplyTestConditions());
            this.funcTestMock.Setup(func => func.Reset());
            this.funcTestMock.Setup(func => func.ResolvePlist(this.instanceName)).Returns(this.plistName);
            this.funcTestMock.Setup(func => func.Execute()).Returns(false);

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(failureData => failureData.GetPatternName()).Returns("pat1");
            failDataMock.Setup(failureData => failureData.GetBurstIndex()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetPatternInstanceId()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetVectorAddress()).Returns(0);
            this.funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(new List<IFailureData>() { failDataMock.Object });
            this.funcTestMock.Setup(func => func.DatalogFailure(1));

            this.dpsVoltageMock.Setup(voltage => voltage.Reset());
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.4, 0.4 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.5, 0.5 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.6, 0.6 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            this.writerMock.Setup(writer => writer.SetData("-9999_-9999|0.400_0.400|0.600_0.600|3"));

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "0.4,0.4",
                EndVoltageLimits = "0.6,0.6",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                MultiPassMasks = string.Empty,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(0, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.funcTestMock.Verify(x => x.DatalogFailure(1), Times.Once);
            this.writerMock.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Execute simple happy path for plist passing in third execution.
        /// </summary>
        [TestMethod]
        public void Execute_SimpleAllPassCase_Return1()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.ApplyTestConditions());
            this.funcTestMock.Setup(func => func.Reset());
            this.funcTestMock.Setup(func => func.ResolvePlist(this.instanceName)).Returns(this.plistName);
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(false).Returns(false).Returns(true);

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(failureData => failureData.GetPatternName()).Returns("pat1");
            failDataMock.Setup(failureData => failureData.GetBurstIndex()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetPatternInstanceId()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetVectorAddress()).Returns(0);
            this.funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(new List<IFailureData>() { failDataMock.Object });

            this.dpsVoltageMock.Setup(voltage => voltage.Reset());
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.4, 0.4 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.5, 0.5 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.6, 0.6 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            this.writerMock.Setup(writer => writer.SetData("0.600_0.600|0.400_0.400|0.600_0.600|3"));

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "0.4,0.4",
                EndVoltageLimits = "0.6,0.6",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                MultiPassMasks = string.Empty,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.dataLogServiceMock.VerifyAll();
            this.writerMock.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Execute multi pass when none of the multi pass mask is a valid combination.
        /// </summary>
        [TestMethod]
        public void Execute_MultiPassMaskAreNotValid_Return1()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.ApplyTestConditions());
            this.funcTestMock.Setup(func => func.Reset());
            this.funcTestMock.Setup(func => func.ResolvePlist(this.instanceName)).Returns(this.plistName);
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(true).Returns(false).Returns(true);

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(failureData => failureData.GetPatternName()).Returns("pat1");
            failDataMock.Setup(failureData => failureData.GetBurstIndex()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetPatternInstanceId()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetVectorAddress()).Returns(0);
            this.funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(new List<IFailureData>() { failDataMock.Object });

            this.dpsVoltageMock.Setup(voltage => voltage.Reset());
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.5, 0.5 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.6, 0.6 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            this.writerMock.Setup(writer => writer.SetData("0.600_0.600|0.500_0.500|0.700_0.700|3"));

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "0.5,0.5",
                EndVoltageLimits = "0.7,0.7",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                MultiPassMasks = "1100,12",
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.dataLogServiceMock.VerifyAll();
            this.writerMock.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Execute multi pass and join results.
        /// </summary>
        [TestMethod]
        public void Execute_MultiPassJoinResults_Return1()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.ApplyTestConditions());
            this.funcTestMock.Setup(func => func.Reset());
            this.funcTestMock.Setup(func => func.ResolvePlist(this.instanceName)).Returns(this.plistName);
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(true).Returns(false).Returns(false).Returns(false).Returns(false).Returns(true);

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(failureData => failureData.GetPatternName()).Returns("pat1");
            failDataMock.Setup(failureData => failureData.GetBurstIndex()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetPatternInstanceId()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetVectorAddress()).Returns(0);
            this.funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(new List<IFailureData>() { failDataMock.Object });
            this.funcTestMock.Setup(func => func.DatalogFailure(1));

            this.dpsVoltageMock.Setup(voltage => voltage.Reset());

            // First multi pass
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.5, 0.5 }));

            // Second multi pass
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.5, -8888 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.6, -8888 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.7, -8888 }));

            // Third multi pass
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { -8888, 0.5 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { -8888, 0.6 }));

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            this.writerMock.Setup(writer => writer.SetData("-9999_0.600|0.500_0.500|0.700_0.700|6"));

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "0.5,0.5",
                EndVoltageLimits = "0.7,0.7",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                MultiPassMasks = "00,01,10",
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.dataLogServiceMock.VerifyAll();
            this.writerMock.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Execute multi pass and print searches results individually.
        /// </summary>
        [TestMethod]
        public void Execute_MultiPassPrintIndividualSearches_Return1()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.ApplyTestConditions());
            this.funcTestMock.Setup(func => func.Reset());
            this.funcTestMock.Setup(func => func.ResolvePlist(this.instanceName)).Returns(this.plistName);
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(true).Returns(false).Returns(false).Returns(false).Returns(false).Returns(true);

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(failureData => failureData.GetPatternName()).Returns("pat1");
            failDataMock.Setup(failureData => failureData.GetBurstIndex()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetPatternInstanceId()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetVectorAddress()).Returns(0);
            this.funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(new List<IFailureData>() { failDataMock.Object });
            this.funcTestMock.Setup(func => func.DatalogFailure(1));

            this.dpsVoltageMock.Setup(voltage => voltage.Reset());

            // First multi pass
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.5, 0.5 }));

            // Second multi pass
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { -8888, 0.5 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { -8888, 0.6 }));

            // Third multi pass
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.5, -8888 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.6, -8888 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.7, -8888 }));

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            this.writerMock.Setup(writer => writer.SetTnamePostfix("_M1"));
            this.writerMock.Setup(writer => writer.SetTnamePostfix("_M2"));
            this.writerMock.Setup(writer => writer.SetTnamePostfix("_M3"));
            this.writerMock.Setup(writer => writer.SetData("0.500_0.500|0.500_0.500|0.700_0.700|1"));
            this.writerMock.Setup(writer => writer.SetData("-9999_-8888|0.500_0.500|0.700_0.700|3"));
            this.writerMock.Setup(writer => writer.SetData("-8888_0.600|0.500_0.500|0.700_0.700|2"));

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "0.5,0.5",
                EndVoltageLimits = "0.7,0.7",
                StepSize = 0.1,
                FeatureSwitchSettings = "print_results_for_all_searches",
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                MultiPassMasks = "00,01,10",
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.dataLogServiceMock.VerifyAll();
            this.writerMock.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Execute search failing at initial voltages to do overshoot.
        /// </summary>
        [TestMethod]
        public void Execute_SimpleOvershootCase_Return1()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.ApplyTestConditions());
            this.funcTestMock.Setup(func => func.Reset());
            this.funcTestMock.Setup(func => func.ResolvePlist(this.instanceName)).Returns(this.plistName);
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(true).Returns(false).Returns(true);

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(failureData => failureData.GetPatternName()).Returns("pat1");
            failDataMock.Setup(failureData => failureData.GetBurstIndex()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetPatternInstanceId()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetVectorAddress()).Returns(0);
            this.funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(new List<IFailureData>() { failDataMock.Object });

            this.dpsVoltageMock.Setup(voltage => voltage.Reset());
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.5, 0.5 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.3, 0.4 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.4, 0.5 }));

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            this.writerMock.Setup(writer => writer.SetData("0.400_0.500|0.300_0.400|0.800_0.800|3"));
            this.writerMock.Setup(writer => writer.SetTnamePostfix("_it"));
            this.writerMock.Setup(writer => writer.SetData("1_1"));

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "0.5,0.5",
                EndVoltageLimits = "0.8,0.8",
                StartVoltagesForRetry = "0.3,0.4",
                StepSize = 0.1,
                FeatureSwitchSettings = "print_per_target_increments",
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.dataLogServiceMock.VerifyAll();
            this.writerMock.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Overshoot conditions are met two times in a row but is only executed one time.
        /// </summary>
        [TestMethod]
        public void Execute_OvershootConditionsMetTwice_Return1()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.ApplyTestConditions());
            this.funcTestMock.Setup(func => func.Reset());
            this.funcTestMock.Setup(func => func.ResolvePlist(this.instanceName)).Returns(this.plistName);
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(true).Returns(true);

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(failureData => failureData.GetPatternName()).Returns("pat1");
            failDataMock.Setup(failureData => failureData.GetBurstIndex()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetPatternInstanceId()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetVectorAddress()).Returns(0);
            this.funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(new List<IFailureData>() { failDataMock.Object });

            this.dpsVoltageMock.Setup(voltage => voltage.Reset());
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.5, 0.5 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.3, 0.3 }));

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            this.writerMock.Setup(writer => writer.SetData("0.300_0.300|0.300_0.300|0.800_0.800|2"));

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "0.5,0.5",
                EndVoltageLimits = "0.8,0.8",
                StartVoltagesForRetry = "0.3,0.3",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.dataLogServiceMock.VerifyAll();
            this.writerMock.VerifyAll();
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Lower start voltages does not match the targets count.
        /// </summary>
        [TestMethod]
        public void Execute_LowerStartVoltagesDoesNotMatchExpectedCount_Return1()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.ApplyTestConditions());
            this.funcTestMock.Setup(func => func.Reset());
            this.funcTestMock.Setup(func => func.ResolvePlist("test")).Returns("plist");
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(true).Returns(false).Returns(true);

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(failureData => failureData.GetPatternName()).Returns("pat1");
            failDataMock.Setup(failureData => failureData.GetBurstIndex()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetPatternInstanceId()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetVectorAddress()).Returns(0);
            this.funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(new List<IFailureData>() { failDataMock.Object });

            this.dpsVoltageMock.Setup(voltage => voltage.Reset());
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.5, 0.5 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.3, 0.3 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.4, 0.4 }));

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            this.writerMock.Setup(writer => writer.SetData("0.400_0.400|0.300_0.300|0.800_0.800|3"));

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B",
                StartVoltages = "0.5,0.5",
                EndVoltageLimits = "0.8,0.8",
                StartVoltagesForRetry = "0.3",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcTestMock.Verify();
            this.dataLogServiceMock.VerifyAll();
            this.writerMock.VerifyAll();
            failDataMock.VerifyAll();
        }
    }
}
