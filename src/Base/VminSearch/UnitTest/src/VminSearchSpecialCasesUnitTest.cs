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
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FunctionalService;
    using Prime.PlistService;
    using Prime.ScoreboardService;
    using Prime.SharedStorageService;
    using Prime.TestMethods.VminSearch;
    using Prime.VoltageService;

    /// <summary>
    /// UnitTest class.
    /// </summary>
    [TestClass]
    public class VminSearchSpecialCasesUnitTest
    {
        private Mock<IFunctionalService> funcServiceMock;
        private Mock<ICaptureFailureTest> funcTestMock;
        private Mock<ICaptureFailureTest> funcScoreboardTestMock;
        private Mock<IVoltageService> voltageServiceMock;
        private Mock<IFivrDomainsAndCondition> fivrVoltageMock;
        private Mock<IDatalogService> dataLogServiceMock;
        private Mock<IScoreboardLogger> scoreboardLoggerMock;
        private Mock<IScoreboardService> scoreboardServiceMock;
        private Mock<IStrgvalFormat> writerMock;
        private Mock<IConsoleService> consoleMock;
        private Mock<IPlistService> plistServiceMock;
        private Mock<IPlistObject> plistMock;

        /// <summary>
        /// Setups the mocks.
        /// </summary>
        [TestInitialize]
        public void SetupMockingVerify()
        {
            // Mocking for consoleMock
            this.consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.consoleMock.Setup(mock => mock.PrintDebug(It.IsAny<string>()));
            Services.ConsoleService = this.consoleMock.Object;
            Prime.Base.ServiceStore<IConsoleService>.Service = this.consoleMock.Object;

            // Mocking for IFunctionalService
            this.funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            this.funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.funcTestMock.Setup(func => func.EnableStartPatternOnFirstFail());
            this.funcTestMock.Setup(func => func.ResolvePlist("test")).Returns("plist");

            this.funcServiceMock.Setup(func => func.CreateCaptureFailureTest("plist", "level", "timing", 1, "SomeCallback()")).Returns(this.funcTestMock.Object);
            this.funcScoreboardTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.funcServiceMock.Setup(func => func.CreateCaptureFailureTest("plist", "level", "timing", 50, 1, "SomeCallback()")).Returns(this.funcScoreboardTestMock.Object);
            this.funcScoreboardTestMock.Setup(func => func.ResolvePlist("test")).Returns("plist");
            Services.FunctionalService = this.funcServiceMock.Object;

            // Mocking for IVoltageService
            this.voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            this.fivrVoltageMock = new Mock<IFivrDomainsAndCondition>(MockBehavior.Strict);
            this.fivrVoltageMock.Setup(voltage => voltage.Reset());
            this.voltageServiceMock.Setup(service => service.CreateFivrForDomainsAndCondition(new List<string>() { "A", "B", "B", "A" }, "fivrCondition", "plist")).Returns(this.fivrVoltageMock.Object);
            Services.VoltageService = this.voltageServiceMock.Object;

            // Mocking for IDatalogService
            this.dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            this.writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            this.dataLogServiceMock.Setup(dataLog => dataLog.GetItuffStrgvalWriter()).Returns(this.writerMock.Object);
            this.dataLogServiceMock.Setup(dataLog => dataLog.WriteToItuff(this.writerMock.Object));
            Services.DatalogService = this.dataLogServiceMock.Object;

            this.scoreboardServiceMock = new Mock<IScoreboardService>(MockBehavior.Strict);
            this.scoreboardLoggerMock = new Mock<IScoreboardLogger>(MockBehavior.Strict);
            this.scoreboardServiceMock.Setup(mock => mock.CreateLogger(120, "1,3", 50)).Returns(this.scoreboardLoggerMock.Object);
            Services.ScoreBoardService = this.scoreboardServiceMock.Object;

            // Mocking for IPlistService
            this.plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            this.plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            this.plistServiceMock.Setup(mock => mock.GetPlistObject("plist")).Returns(this.plistMock.Object);
            Services.PlistService = this.plistServiceMock.Object;
        }

        /// <summary>
        /// Execute happy path with the purpose of cover a few conditions:
        /// 1. Multi target with single Start and End Limit key Voltages provided (shared storage).
        /// 2. MultiPass - 2 passes (states), first pass and second fail due to invalid range.
        /// 3. Scoreboard mode enabled but scoreboard plists execution is passing.
        /// </summary>
        [TestMethod]
        public void Execute_SpecialScenarios1_Return1()
        {
            // Mock Setup
            this.consoleMock.Setup(mock => mock.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.SetupSequence(sharedStorage => sharedStorage.GetDoubleRowFromTable("StartValue", Context.DUT))
                .Returns(-9999).Returns(0.4).Returns(0.4).Returns(0.4)
                .Returns(0.6).Returns(0.6).Returns(0.6).Returns(0.6);
            sharedStorageServiceMock
                .SetupSequence(sharedStorage => sharedStorage.GetDoubleRowFromTable("EndValue", Context.DUT))
                .Returns(0.6).Returns(0.4);
            Base.ServiceStore<ISharedStorageService>.Service = sharedStorageServiceMock.Object;

            this.funcTestMock.Setup(func => func.ApplyTestConditions());
            this.funcTestMock.Setup(func => func.Reset());
            this.funcTestMock.Setup(func => func.ResolvePlist("test")).Returns("plist");
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(false).Returns(false).Returns(true);

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(failureData => failureData.GetPatternName()).Returns("pat1");
            failDataMock.Setup(failureData => failureData.GetBurstIndex()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetPatternInstanceId()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetVectorAddress()).Returns(0);
            this.funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(new List<IFailureData>() { failDataMock.Object });

            this.funcScoreboardTestMock.SetupSequence(func => func.ExecuteWithInlineRv()).Returns(true);
            this.plistMock.Setup(mock => mock.IsPatternAnAmble("pat1")).Returns(false);
            this.funcScoreboardTestMock.Setup(func => func.ResolvePlist("test")).Returns("plist");
            this.funcScoreboardTestMock.Setup(func => func.SetStartPattern("pat1", 0, 0));

            this.fivrVoltageMock.Setup(voltage => voltage.ApplyCondition());
            this.fivrVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { -8888, 0.4, 0.4, 0.4 }));
            this.fivrVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { -8888, 0.5, 0.5, 0.5 }));
            this.fivrVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { -8888, 0.6, 0.6, 0.6 }));

            this.writerMock.Setup(vminWriter => vminWriter.SetTnamePostfix("_M1"));
            this.writerMock.Setup(vminWriter => vminWriter.SetData("-8888_0.600_0.600_0.600|-9999_0.400_0.400_0.400|0.600_0.600_0.600_0.600|3"));
            this.writerMock.Setup(vminWriter => vminWriter.SetTnamePostfix("_M1_lp"));
            this.writerMock.Setup(vminWriter => vminWriter.SetData("na^a1^a1^a1"));
            this.writerMock.Setup(vminWriter => vminWriter.SetData("9999_9999_9999_9999|0.600_0.600_0.600_0.600|0.400_0.400_0.400_0.400"));

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B,B,A",
                StartVoltages = "StartValue,StartValue,StartValue,StartValue",
                EndVoltageLimits = "EndValue",
                StepSize = 0.1,
                FeatureSwitchSettings = "fivr_mode_on,print_results_for_all_searches",
                IfeObject = "IfeObjectParameterForGetCoverage",
                FivrCondition = "fivrCondition",
                MultiPassMasks = "0000,1101",
                ScoreboardBaseNumber = "120",
                ScoreboardMaxFails = 50,
                ScoreboardEdgeTicks = 2,
                PatternNameMap = "1,3",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                ExecutionMode = PrimeVminSearchTestMethod.ExecutionModeFlag.SearchWithScoreboard,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            Assert.AreEqual("IfeObjectParameterForGetCoverage", search.IfeObject.ToString());
            this.consoleMock.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            this.fivrVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.funcScoreboardTestMock.VerifyAll();
            this.scoreboardServiceMock.VerifyAll();
            this.scoreboardLoggerMock.VerifyAll();
            this.dataLogServiceMock.VerifyAll();
            this.writerMock.VerifyAll();
            this.plistServiceMock.VerifyAll();
            this.plistMock.VerifyAll();
            failDataMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute happy path with the purpose of cover a few scoreboard scenarios executing multiple times back to back:
        /// 1. Scoreboard on edge (ScoreboardEdgeTicks != 0) is executed for a passing search case (edge ticks used to set voltages).
        /// 2. Scoreboard only for fail range cases (ScoreboardEdgeTicks = 0) is executed for a fail search case (set last voltages before fail).
        /// 3. Scoreboard is disabled (ScoreboardBaseNumber = 0).
        /// </summary>
        [TestMethod]
        public void Execute_SpecialScenarios2_Return1()
        {
            // Mock Setup
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.SetupSequence(sharedStorage => sharedStorage.GetDoubleRowFromTable("StartValue", Context.DUT))
                .Returns(0.4).Returns(0.4).Returns(0.4);
            sharedStorageServiceMock.SetupSequence(sharedStorage => sharedStorage.GetDoubleRowFromTable("EndValue", Context.DUT))
                .Returns(0.5).Returns(0.5).Returns(0.5);
            Base.ServiceStore<ISharedStorageService>.Service = sharedStorageServiceMock.Object;

            this.funcTestMock.Setup(func => func.ApplyTestConditions());
            this.funcTestMock.Setup(func => func.Reset());
            this.funcTestMock.Setup(func => func.ResolvePlist("test")).Returns("plist");
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(false).Returns(true).Returns(false).Returns(false).Returns(false).Returns(false);
            this.funcTestMock.SetupSequence(func => func.DatalogFailure(1));

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.SetupSequence(failureData => failureData.GetPatternName()).Returns("pat1").Returns("pat2").Returns("pat2").Returns("pat3").Returns("pat3");
            failDataMock.Setup(failureData => failureData.GetBurstIndex()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetPatternInstanceId()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetVectorAddress()).Returns(0);
            var failData = new List<IFailureData>() { failDataMock.Object };
            var emptyFailData = new List<IFailureData>();
            this.funcTestMock.SetupSequence(func => func.GetPerCycleFailures()).Returns(failData).Returns(emptyFailData).Returns(failData).Returns(failData).Returns(failData).Returns(failData);

            this.funcScoreboardTestMock.SetupSequence(func => func.ExecuteWithInlineRv()).Returns(true).Returns(true);
            this.plistMock.Setup(mock => mock.IsPatternAnAmble("pat1")).Returns(false);
            this.funcScoreboardTestMock.Setup(func => func.SetStartPattern("pat1", 0, 0));
            this.plistMock.Setup(mock => mock.IsPatternAnAmble("pat2")).Returns(false);
            this.funcScoreboardTestMock.Setup(func => func.SetStartPattern("pat2", 0, 0));
            this.funcScoreboardTestMock.Setup(func => func.ResolvePlist("test")).Returns("plist");

            this.fivrVoltageMock.Setup(voltage => voltage.ApplyCondition());
            this.fivrVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.4, 0.4, 0.4, 0.4 }));
            this.fivrVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.5, 0.5, 0.5, 0.5 }));

            this.writerMock.Setup(vminWriter => vminWriter.SetData("0.500_0.500_0.500_0.500|0.400_0.400_0.400_0.400|0.500_0.500_0.500_0.500|2"));
            this.writerMock.Setup(vminWriter => vminWriter.SetData("-9999_-9999_-9999_-9999|0.400_0.400_0.400_0.400|0.500_0.500_0.500_0.500|2"));
            this.writerMock.Setup(vminWriter => vminWriter.SetTnamePostfix("_lp"));
            this.writerMock.Setup(vminWriter => vminWriter.SetData("a1^a1^a1^a1"));
            this.writerMock.Setup(vminWriter => vminWriter.SetData("a2^a2^a2^a2"));
            this.writerMock.Setup(vminWriter => vminWriter.SetData("a3^a3^a3^a3"));

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B,B,A",
                StartVoltages = "StartValue",
                EndVoltageLimits = "EndValue",
                StepSize = 0.1,
                FeatureSwitchSettings = "fivr_mode_on",
                IfeObject = "IfeObjectParameterForGetCoverage",
                FivrCondition = "fivrCondition",
                ScoreboardBaseNumber = "120",
                ScoreboardMaxFails = 50,
                ScoreboardEdgeTicks = 1,
                PatternNameMap = "1,3",
                ExecutionMode = PrimeVminSearchTestMethod.ExecutionModeFlag.SearchWithScoreboard,
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            Assert.AreEqual("IfeObjectParameterForGetCoverage", search.IfeObject.ToString());

            search.ScoreboardEdgeTicks = 0;
            search.Verify();
            Assert.AreEqual(0, search.Execute());

            search.ExecutionMode = PrimeVminSearchTestMethod.ExecutionModeFlag.Search;
            search.ScoreboardBaseNumber = string.Empty;
            search.Verify();
            Assert.AreEqual(0, search.Execute());

            this.consoleMock.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            this.fivrVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.funcScoreboardTestMock.VerifyAll();
            this.scoreboardServiceMock.VerifyAll();
            this.scoreboardLoggerMock.VerifyAll();
            this.dataLogServiceMock.VerifyAll();
            this.writerMock.VerifyAll();
            this.plistServiceMock.VerifyAll();
            this.plistMock.VerifyAll();
            failDataMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute overshoot case when scoreboard is enabled.
        /// </summary>
        [TestMethod]
        public void Execute_SpecialScenarios3_Return1()
        {
            // Mock Setup
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(sharedStorage => sharedStorage.GetDoubleRowFromTable("StartValue", Context.DUT))
                .Returns(0.4);
            sharedStorageServiceMock.SetupSequence(sharedStorage => sharedStorage.GetDoubleRowFromTable("EndValue", Context.DUT))
                .Returns(0.5).Returns(0.5);
            sharedStorageServiceMock.Setup(sharedStorage => sharedStorage.GetDoubleRowFromTable("LowerStartValue", Context.DUT))
                .Returns(0.3);
            Base.ServiceStore<ISharedStorageService>.Service = sharedStorageServiceMock.Object;

            this.funcTestMock.Setup(func => func.ApplyTestConditions());
            this.funcTestMock.Setup(func => func.Reset());
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(true).Returns(true);

            this.funcScoreboardTestMock.Setup(func => func.ResolvePlist("test")).Returns("plist");

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);

            failDataMock.SetupSequence(failureData => failureData.GetPatternName()).Returns("pat1").Returns("pat0");
            failDataMock.Setup(failureData => failureData.GetBurstIndex()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetPatternInstanceId()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetVectorAddress()).Returns(0);
            var failData = new List<IFailureData>() { failDataMock.Object };

            this.funcTestMock.SetupSequence(func => func.GetPerCycleFailures()).Returns(failData).Returns(failData);
            this.fivrVoltageMock.Setup(voltage => voltage.ApplyCondition());
            this.fivrVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.4, 0.4, 0.4, 0.4 }));
            this.fivrVoltageMock.Setup(voltage => voltage.Apply(new List<double>() { 0.3, 0.3, 0.3, 0.3 }));

            var outputVoltages = "0.300_0.300_0.300_0.300|0.300_0.300_0.300_0.300|0.500_0.500_0.500_0.500|2";
            this.writerMock.Setup(vminWriter => vminWriter.SetData(outputVoltages));

            this.writerMock.Setup(strgvalWriter => strgvalWriter.SetData(outputVoltages));
            this.writerMock.Setup(strgvalWriter => strgvalWriter.SetTnamePostfix("_lp"));
            this.writerMock.Setup(strgvalWriter => strgvalWriter.SetData("a0^a0^a0^a0"));

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B,B,A",
                StartVoltages = "StartValue",
                EndVoltageLimits = "EndValue",
                StartVoltagesForRetry = "LowerStartValue",
                StepSize = 0.1,
                FeatureSwitchSettings = "fivr_mode_on",
                IfeObject = string.Empty,
                FivrCondition = "fivrCondition",
                ScoreboardBaseNumber = "120",
                ScoreboardMaxFails = 50,
                ScoreboardEdgeTicks = 1,
                PatternNameMap = "1,3",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                ExecutionMode = PrimeVminSearchTestMethod.ExecutionModeFlag.SearchWithScoreboard,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());

            this.consoleMock.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            this.fivrVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.funcScoreboardTestMock.VerifyAll();
            this.scoreboardServiceMock.VerifyAll();
            this.scoreboardLoggerMock.VerifyAll();
            this.dataLogServiceMock.VerifyAll();
            this.writerMock.VerifyAll();
            this.plistServiceMock.VerifyAll();
            this.plistMock.VerifyAll();
            failDataMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
        }
    }
}
