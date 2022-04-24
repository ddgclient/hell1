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
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PlistService;
    using Prime.ScoreboardService;
    using Prime.VoltageService;

    /// <summary>
    /// UnitTest class.
    /// </summary>
    [TestClass]
    public class ScoreboardPointTestUnitTest
    {
        private Mock<IPlistService> plistServiceMock;
        private Mock<IPlistObject> plistMock;

        /// <summary>
        /// Setups the mocks.
        /// </summary>
        [TestInitialize]
        public void SetupMockingVerify()
        {
            // Mocking for IPlistService
            this.plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            this.plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            this.plistServiceMock.Setup(mock => mock.GetPlistObject("plistName")).Returns(this.plistMock.Object);
            Services.PlistService = this.plistServiceMock.Object;
        }

        /// <summary>
        /// List of SearchPointData does not contain enough elements to do scoreboard.
        /// </summary>
        [TestMethod]
        public void Execute_SearchPointDataDoesNotContainEnoughElements_Void()
        {
            var searchPointData = new List<SearchPointData>();
            var mask = new BitArray(4, true);
            var pointData = new SearchPointData(new List<double>() { 0.5, 0.6, 0.7, 0.8 }, new SearchPointData.PatternData("myPattern", 2, 3));
            searchPointData.Add(pointData);

            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            Services.FunctionalService = functionalServiceMock.Object;
            functionalServiceMock.Setup(func => func.CreateCaptureFailureTest("plistName", "level", "timing", ulong.MaxValue, 1, string.Empty)).Returns(funcTestMock.Object);
            var voltageMock = new Mock<IVoltage>(MockBehavior.Strict);
            var searchExtensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);

            var scoreBoardLoggerMock = new Mock<IScoreboardLogger>(MockBehavior.Strict);
            var scoreboardServiceMock = new Mock<IScoreboardService>(MockBehavior.Strict);
            scoreboardServiceMock.Setup(x => x.CreateLogger(1255, "PatternNameMap", ulong.MaxValue)).Returns(scoreBoardLoggerMock.Object);
            Services.ScoreBoardService = scoreboardServiceMock.Object;

            var plistExecutionParameters = new PrimeVminSearchTestMethod.PlistExecutionParameters()
            {
                Patlist = "plistName",
                LevelsTc = "level",
                TimingsTc = "timing",
                PrePlist = string.Empty,
            };

            var scoreboardExecutionParameters = new ScoreboardPointTest.ScoreboardExecutionParameters()
            {
                MaxFails = 0,
                EdgeTicks = 2,
                BaseNumbers = new List<int> { 1255 },
                PatternNameMap = "PatternNameMap",
                PrintScoreboardCounters = false,
            };

            var pointTest = new ScoreboardPointTest(plistExecutionParameters, scoreboardExecutionParameters, voltageMock.Object, searchExtensionsMock.Object, consoleServiceMock.Object);
            pointTest.Execute(searchPointData, mask, string.Empty, true);

            funcTestMock.VerifyAll();
            functionalServiceMock.VerifyAll();
            voltageMock.VerifyAll();
            searchExtensionsMock.VerifyAll();
            scoreboardServiceMock.VerifyAll();
            scoreBoardLoggerMock.VerifyAll();
        }

        /// <summary>
        /// List of SearchPointData does not contain fail voltage values nor mask voltage values.
        /// </summary>
        [TestMethod]
        public void Execute_SearchPointDataDoesNotContainMaskTargets_Void()
        {
            var maskBits = new BitArray(4, false);
            var searchPointData = new List<SearchPointData>();
            var firstPointData = new SearchPointData(new List<double>() { 0.5, 0.5, 0.5, 0.5 }, new SearchPointData.PatternData("P1", 1, 1));
            var secondPointData = new SearchPointData(new List<double>() { 0.5, 0.6, 0.5, 0.5 }, new SearchPointData.PatternData("P2", 2, 2));
            var thirdPointData = new SearchPointData(new List<double>() { 0.6, 0.6, 0.5, 0.5 }, new SearchPointData.PatternData("P3", 3, 3));
            var fourthPointData = new SearchPointData(new List<double>() { 0.6, 0.7, 0.6, 0.6 }, new SearchPointData.PatternData("P4", 4, 4));
            var fifthPointData = new SearchPointData(new List<double>() { 0.7, 0.8, 0.7, 0.7 }, new SearchPointData.PatternData("P5", 5, 5));
            var sixthPointData = new SearchPointData(new List<double>() { 0.7, 0.8, 0.8, 0.7 }, new SearchPointData.PatternData("P6", 6, 6));
            var seventhPointData = new SearchPointData(new List<double>() { 0.7, 0.8, 0.9, 0.8 }, new SearchPointData.PatternData("P7", 7, 7));
            searchPointData.Add(firstPointData);
            searchPointData.Add(secondPointData);
            searchPointData.Add(thirdPointData);
            searchPointData.Add(fourthPointData);
            searchPointData.Add(fifthPointData);
            searchPointData.Add(sixthPointData);
            searchPointData.Add(seventhPointData);

            var voltageMock = new Mock<IVoltage>(MockBehavior.Strict);
            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            Services.FunctionalService = functionalServiceMock.Object;
            functionalServiceMock.Setup(func => func.CreateCaptureFailureTest("plistName", "level", "timing", ulong.MaxValue, 1, string.Empty)).Returns(funcTestMock.Object);
            this.plistMock.Setup(mock => mock.IsPatternAnAmble("P2")).Returns(false);
            funcTestMock.Setup(mock => mock.SetStartPattern("P2", 2, 2));
            funcTestMock.Setup(mock => mock.ExecuteWithInlineRv()).Returns(false);
            var failureDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failureDataMock.Setup(mock => mock.GetPatternName()).Returns("pattern1");
            funcTestMock.Setup(mock => mock.GetPerCycleFailures()).Returns(new List<IFailureData>() { failureDataMock.Object });
            var searchExtensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            searchExtensionsMock.Setup(func => func.ApplyMask(maskBits, funcTestMock.Object));
            searchExtensionsMock.Setup(x => x.ApplySearchVoltage(voltageMock.Object, new List<double>() { 0.5, 0.6, 0.7, 0.6 }));
            var consoleServiceMock = new Mock<IConsoleService>();
            Services.ConsoleService = consoleServiceMock.Object;
            consoleServiceMock.Setup(func => func.PrintDebug(It.IsAny<string>()));

            var scoreBoardLoggerMock = new Mock<IScoreboardLogger>(MockBehavior.Strict);
            scoreBoardLoggerMock.Setup(x => x.ProcessFailData(It.IsAny<List<string>>()));
            scoreBoardLoggerMock.Setup(x => x.PrintCountersToItuff(string.Empty));
            var scoreboardServiceMock = new Mock<IScoreboardService>(MockBehavior.Strict);
            scoreboardServiceMock.Setup(x => x.CreateLogger(1255, "PatternNameMap", ulong.MaxValue)).Returns(scoreBoardLoggerMock.Object);
            Services.ScoreBoardService = scoreboardServiceMock.Object;

            var plistExecutionParameters = new PrimeVminSearchTestMethod.PlistExecutionParameters()
            {
                Patlist = "plistName",
                LevelsTc = "level",
                TimingsTc = "timing",
                PrePlist = string.Empty,
            };

            var scoreboardExecutionParameters = new ScoreboardPointTest.ScoreboardExecutionParameters()
            {
                MaxFails = 0,
                EdgeTicks = 2,
                BaseNumbers = new List<int> { 1255 },
                PatternNameMap = "PatternNameMap",
                PrintScoreboardCounters = true,
            };

            var pointTest = new ScoreboardPointTest(plistExecutionParameters, scoreboardExecutionParameters, voltageMock.Object, searchExtensionsMock.Object, consoleServiceMock.Object);
            pointTest.Execute(searchPointData, maskBits, string.Empty, true);

            voltageMock.VerifyAll();
            funcTestMock.VerifyAll();
            functionalServiceMock.VerifyAll();
            searchExtensionsMock.VerifyAll();
            consoleServiceMock.VerifyAll();
            failureDataMock.VerifyAll();
            scoreboardServiceMock.VerifyAll();
            scoreBoardLoggerMock.VerifyAll();
        }

        /// <summary>
        /// List of SearchPointData contain one mask voltage value.
        /// </summary>
        [TestMethod]
        public void Execute_SearchPointDataHasOneMaskVoltageValue_Void()
        {
            var mask = new[] { false, false, false, true };
            var maskBits = new BitArray(mask);
            var searchPointData = new List<SearchPointData>();
            var firstPointData = new SearchPointData(new List<double>() { 0.5, 0.5, 0.5, -8888 }, new SearchPointData.PatternData("P1", 1, 1));
            var secondPointData = new SearchPointData(new List<double>() { 0.5, 0.6, 0.5, -8888 }, new SearchPointData.PatternData("P2", 2, 2));
            var thirdPointData = new SearchPointData(new List<double>() { 0.6, 0.6, 0.5, -8888 }, new SearchPointData.PatternData("P3", 3, 3));
            var fourthPointData = new SearchPointData(new List<double>() { 0.6, 0.7, 0.6, -8888 }, new SearchPointData.PatternData("P4", 4, 4));
            var fifthPointData = new SearchPointData(new List<double>() { 0.7, 0.8, 0.7, -8888 }, new SearchPointData.PatternData("P5", 5, 5));
            var sixthPointData = new SearchPointData(new List<double>() { 0.7, 0.8, 0.8, -8888 }, new SearchPointData.PatternData("P6", 6, 6));
            var seventhPointData = new SearchPointData(new List<double>() { 0.7, 0.8, 0.9, -8888 }, new SearchPointData.PatternData("P7", 7, 7));
            searchPointData.Add(firstPointData);
            searchPointData.Add(secondPointData);
            searchPointData.Add(thirdPointData);
            searchPointData.Add(fourthPointData);
            searchPointData.Add(fifthPointData);
            searchPointData.Add(sixthPointData);
            searchPointData.Add(seventhPointData);

            var voltageMock = new Mock<IVoltage>(MockBehavior.Strict);
            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            Services.FunctionalService = functionalServiceMock.Object;
            functionalServiceMock.Setup(func => func.CreateCaptureFailureTest("plistName", "level", "timing", ulong.MaxValue, 1, string.Empty)).Returns(funcTestMock.Object);
            this.plistMock.Setup(mock => mock.IsPatternAnAmble("P2")).Returns(false);
            funcTestMock.Setup(mock => mock.SetStartPattern("P2", 2, 2));
            funcTestMock.Setup(mock => mock.ExecuteWithInlineRv()).Returns(false);
            funcTestMock.Setup(mock => mock.GetPerCycleFailures()).Returns(new List<IFailureData>());
            var searchExtensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            searchExtensionsMock.Setup(func => func.ApplyMask(maskBits, funcTestMock.Object));
            searchExtensionsMock.Setup(x => x.ApplySearchVoltage(voltageMock.Object, new List<double>() { 0.5, 0.6, 0.7, -8888 }));
            var consoleServiceMock = new Mock<IConsoleService>();
            Services.ConsoleService = consoleServiceMock.Object;
            consoleServiceMock.Setup(func => func.PrintDebug(It.IsAny<string>()));

            var scoreBoardLoggerMock = new Mock<IScoreboardLogger>(MockBehavior.Strict);
            var scoreboardServiceMock = new Mock<IScoreboardService>(MockBehavior.Strict);
            scoreboardServiceMock.Setup(x => x.CreateLogger(1255, "PatternNameMap", ulong.MaxValue)).Returns(scoreBoardLoggerMock.Object);
            Services.ScoreBoardService = scoreboardServiceMock.Object;

            var plistExecutionParameters = new PrimeVminSearchTestMethod.PlistExecutionParameters()
            {
                Patlist = "plistName",
                LevelsTc = "level",
                TimingsTc = "timing",
                PrePlist = string.Empty,
            };

            var scoreboardExecutionParameters = new ScoreboardPointTest.ScoreboardExecutionParameters()
            {
                MaxFails = 0,
                EdgeTicks = 2,
                BaseNumbers = new List<int> { 1255 },
                PatternNameMap = "PatternNameMap",
                PrintScoreboardCounters = false,
            };

            var pointTest = new ScoreboardPointTest(plistExecutionParameters, scoreboardExecutionParameters, voltageMock.Object, searchExtensionsMock.Object, consoleServiceMock.Object);
            pointTest.Execute(searchPointData, maskBits, string.Empty, true);

            voltageMock.VerifyAll();
            funcTestMock.VerifyAll();
            functionalServiceMock.VerifyAll();
            searchExtensionsMock.VerifyAll();
            consoleServiceMock.VerifyAll();
            scoreboardServiceMock.VerifyAll();
            scoreBoardLoggerMock.VerifyAll();
        }

        /// <summary>
        /// List of SearchPointData contains two fail voltage values and one mask voltage value.
        /// Skip of SetStartPattern because it is an amble.
        /// </summary>
        [TestMethod]
        public void Execute_SearchPointDataContainsMaskTargets_Void()
        {
            var mask = new[] { false, false, false, true };
            var initialMask = new BitArray(mask);
            var searchPointData = new List<SearchPointData>();
            var firstPointData = new SearchPointData(new List<double>() { 0.5, 0.55, 0.5, -8888 }, new SearchPointData.PatternData("P1", 1, 1));
            var secondPointData = new SearchPointData(new List<double>() { 0.6, 0.66, 0.5, -8888 }, new SearchPointData.PatternData("P2", 2, 2));
            var thirdPointData = new SearchPointData(new List<double>() { 0.7, 0.75, 0.5, -8888 }, new SearchPointData.PatternData("P3", 3, 3));
            var fourthPointData = new SearchPointData(new List<double>() { 0.8, 0.85, 0.6, -8888 }, new SearchPointData.PatternData("P4", 4, 4));
            var fifthPointData = new SearchPointData(new List<double>() { 0.9, 0.95, 0.7, -8888 }, new SearchPointData.PatternData("P5", 5, 5));
            var sixthPointData = new SearchPointData(new List<double>() { 1.0, -9999, 0.8, -8888 }, new SearchPointData.PatternData("P6", 6, 6));
            var seventhPointData = new SearchPointData(new List<double>() { -9999, -9999, 0.9, -8888 }, new SearchPointData.PatternData("P7", 7, 7));
            searchPointData.Add(firstPointData);
            searchPointData.Add(secondPointData);
            searchPointData.Add(thirdPointData);
            searchPointData.Add(fourthPointData);
            searchPointData.Add(fifthPointData);
            searchPointData.Add(sixthPointData);
            searchPointData.Add(seventhPointData);

            var finalMask = new[] { false, false, true, true };
            var maskBits = new BitArray(finalMask);
            var voltageMock = new Mock<IVoltage>(MockBehavior.Strict);
            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            Services.FunctionalService = functionalServiceMock.Object;
            functionalServiceMock.Setup(func => func.CreateCaptureFailureTest("plistName", "level", "timing", ulong.MaxValue, 1, string.Empty)).Returns(funcTestMock.Object);
            this.plistMock.Setup(mock => mock.IsPatternAnAmble("P5")).Returns(true);
            funcTestMock.Setup(mock => mock.Reset());
            funcTestMock.Setup(mock => mock.ExecuteWithInlineRv()).Returns(false);
            funcTestMock.Setup(mock => mock.GetPerCycleFailures()).Returns(new List<IFailureData>());
            var searchExtensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            searchExtensionsMock.Setup(func => func.ApplyMask(maskBits, funcTestMock.Object));
            searchExtensionsMock.Setup(x => x.ApplySearchVoltage(voltageMock.Object, new List<double>() { 1.0, 0.95, 1.0, -8888 }));
            var consoleServiceMock = new Mock<IConsoleService>();
            Services.ConsoleService = consoleServiceMock.Object;
            consoleServiceMock.Setup(func => func.PrintDebug(It.IsAny<string>()));
            var scoreBoardLoggerMock = new Mock<IScoreboardLogger>(MockBehavior.Strict);
            var scoreboardServiceMock = new Mock<IScoreboardService>(MockBehavior.Strict);
            scoreboardServiceMock.Setup(x => x.CreateLogger(1255, "PatternNameMap", ulong.MaxValue)).Returns(scoreBoardLoggerMock.Object);
            Services.ScoreBoardService = scoreboardServiceMock.Object;

            var plistExecutionParameters = new PrimeVminSearchTestMethod.PlistExecutionParameters()
            {
                Patlist = "plistName",
                LevelsTc = "level",
                TimingsTc = "timing",
                PrePlist = string.Empty,
            };

            var scoreboardExecutionParameters = new ScoreboardPointTest.ScoreboardExecutionParameters()
            {
                MaxFails = 0,
                EdgeTicks = 2,
                BaseNumbers = new List<int> { 1255 },
                PatternNameMap = "PatternNameMap",
                PrintScoreboardCounters = false,
            };

            var pointTest = new ScoreboardPointTest(plistExecutionParameters, scoreboardExecutionParameters, voltageMock.Object, searchExtensionsMock.Object, consoleServiceMock.Object);
            pointTest.Execute(searchPointData, initialMask, string.Empty, true);

            voltageMock.VerifyAll();
            funcTestMock.VerifyAll();
            functionalServiceMock.VerifyAll();
            searchExtensionsMock.VerifyAll();
            consoleServiceMock.VerifyAll();
            scoreboardServiceMock.VerifyAll();
            scoreBoardLoggerMock.VerifyAll();
        }

        /// <summary>
        /// List of SearchPointData contains only mask targets.
        /// </summary>
        [TestMethod]
        public void Execute_SearchPointDataContainsOnlyMaskTargets_Void()
        {
            var maskBits = new BitArray(4, true);
            var searchPointData = new List<SearchPointData>();
            var firstPointData = new SearchPointData(new List<double>() { 0.5, 0.5, 0.5, 0.5 }, new SearchPointData.PatternData("P1", 1, 1));
            var secondPointData = new SearchPointData(new List<double>() { 0.5, 0.6, 0.5, 0.5 }, new SearchPointData.PatternData("P2", 2, 2));
            var thirdPointData = new SearchPointData(new List<double>() { 0.6, 0.6, 0.5, 0.5 }, new SearchPointData.PatternData("P3", 3, 3));
            var fourthPointData = new SearchPointData(new List<double>() { 0.6, 0.6, 0.6, 0.5 }, new SearchPointData.PatternData("P4", 4, 4));
            var fifthPointData = new SearchPointData(new List<double>() { 0.7, 0.6, 0.7, 0.6 }, new SearchPointData.PatternData("P5", 5, 5));
            searchPointData.Add(firstPointData);
            searchPointData.Add(secondPointData);
            searchPointData.Add(thirdPointData);
            searchPointData.Add(fourthPointData);
            searchPointData.Add(fifthPointData);

            var voltageMock = new Mock<IVoltage>(MockBehavior.Strict);
            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            Services.FunctionalService = functionalServiceMock.Object;
            functionalServiceMock.Setup(func => func.CreateCaptureFailureTest("plistName", "level", "timing", ulong.MaxValue, 1, string.Empty)).Returns(funcTestMock.Object);
            var searchExtensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);

            var scoreBoardLoggerMock = new Mock<IScoreboardLogger>(MockBehavior.Strict);
            var scoreboardServiceMock = new Mock<IScoreboardService>(MockBehavior.Strict);
            scoreboardServiceMock.Setup(x => x.CreateLogger(0, "PatternNameMap", ulong.MaxValue)).Returns(scoreBoardLoggerMock.Object);
            Services.ScoreBoardService = scoreboardServiceMock.Object;

            var plistExecutionParameters = new PrimeVminSearchTestMethod.PlistExecutionParameters()
            {
                Patlist = "plistName",
                LevelsTc = "level",
                TimingsTc = "timing",
                PrePlist = string.Empty,
            };

            var scoreboardExecutionParameters = new ScoreboardPointTest.ScoreboardExecutionParameters()
            {
                MaxFails = 0,
                EdgeTicks = 3,
                BaseNumbers = new List<int> { 0 },
                PatternNameMap = "PatternNameMap",
                PrintScoreboardCounters = false,
            };

            var pointTest = new ScoreboardPointTest(plistExecutionParameters, scoreboardExecutionParameters, voltageMock.Object, searchExtensionsMock.Object, consoleServiceMock.Object);
            pointTest.Execute(searchPointData, maskBits, string.Empty, true);

            voltageMock.VerifyAll();
            funcTestMock.VerifyAll();
            functionalServiceMock.VerifyAll();
            searchExtensionsMock.VerifyAll();
            scoreboardServiceMock.VerifyAll();
            scoreBoardLoggerMock.VerifyAll();
        }

        /// <summary>
        /// The parameter ScoreboardMaxFails is not used with the default value.
        /// </summary>
        [TestMethod]
        public void Execute_TotalCaptureCountIsNotDefaultValue_Void()
        {
            var maskBits = new BitArray(4, true);
            var searchPointData = new List<SearchPointData>();
            var firstPointData = new SearchPointData(new List<double>() { 0.5, 0.5, 0.5, 0.5 }, new SearchPointData.PatternData("P1", 1, 1));
            var secondPointData = new SearchPointData(new List<double>() { 0.5, 0.6, 0.5, 0.5 }, new SearchPointData.PatternData("P2", 2, 2));
            var thirdPointData = new SearchPointData(new List<double>() { 0.6, 0.6, 0.5, 0.5 }, new SearchPointData.PatternData("P3", 3, 3));
            var fourthPointData = new SearchPointData(new List<double>() { 0.6, 0.6, 0.6, 0.5 }, new SearchPointData.PatternData("P4", 4, 4));
            var fifthPointData = new SearchPointData(new List<double>() { 0.7, 0.6, 0.7, 0.6 }, new SearchPointData.PatternData("P5", 5, 5));
            searchPointData.Add(firstPointData);
            searchPointData.Add(secondPointData);
            searchPointData.Add(thirdPointData);
            searchPointData.Add(fourthPointData);
            searchPointData.Add(fifthPointData);

            var voltageMock = new Mock<IVoltage>(MockBehavior.Strict);
            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            Services.FunctionalService = functionalServiceMock.Object;
            functionalServiceMock.Setup(func => func.CreateCaptureFailureTest("plistName", "level", "timing", 5, 1, string.Empty)).Returns(funcTestMock.Object);
            var searchExtensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);

            var scoreBoardLoggerMock = new Mock<IScoreboardLogger>(MockBehavior.Strict);
            var scoreboardServiceMock = new Mock<IScoreboardService>(MockBehavior.Strict);
            scoreboardServiceMock.Setup(x => x.CreateLogger(0, "PatternNameMap", 5)).Returns(scoreBoardLoggerMock.Object);
            Services.ScoreBoardService = scoreboardServiceMock.Object;

            var plistExecutionParameters = new PrimeVminSearchTestMethod.PlistExecutionParameters()
            {
                Patlist = "plistName",
                LevelsTc = "level",
                TimingsTc = "timing",
                PrePlist = string.Empty,
            };

            var scoreboardExecutionParameters = new ScoreboardPointTest.ScoreboardExecutionParameters()
            {
                MaxFails = 5,
                EdgeTicks = 3,
                BaseNumbers = new List<int> { 0 },
                PatternNameMap = "PatternNameMap",
                PrintScoreboardCounters = false,
            };

            var pointTest = new ScoreboardPointTest(plistExecutionParameters, scoreboardExecutionParameters, voltageMock.Object, searchExtensionsMock.Object, consoleServiceMock.Object);
            pointTest.Execute(searchPointData, maskBits, string.Empty, true);

            voltageMock.VerifyAll();
            funcTestMock.VerifyAll();
            functionalServiceMock.VerifyAll();
            searchExtensionsMock.VerifyAll();
            scoreboardServiceMock.VerifyAll();
            scoreBoardLoggerMock.VerifyAll();
        }

        /// <summary>
        /// List of SearchPointData contains enough elements but no base number is passed.
        /// </summary>
        [TestMethod]
        public void Execute_NoBaseNumberIsPassed_Void()
        {
            var maskBits = new BitArray(4, false);
            var searchPointData = new List<SearchPointData>();
            var firstPointData = new SearchPointData(new List<double>() { 0.5, 0.5, 0.5, 0.5 }, new SearchPointData.PatternData("P1", 1, 1));
            var secondPointData = new SearchPointData(new List<double>() { 0.5, 0.6, 0.5, 0.5 }, new SearchPointData.PatternData("P2", 2, 2));
            var thirdPointData = new SearchPointData(new List<double>() { 0.6, 0.6, 0.5, 0.5 }, new SearchPointData.PatternData("P3", 3, 3));
            var fourthPointData = new SearchPointData(new List<double>() { 0.6, 0.7, 0.6, 0.6 }, new SearchPointData.PatternData("P4", 4, 4));
            var fifthPointData = new SearchPointData(new List<double>() { 0.7, 0.8, 0.7, 0.7 }, new SearchPointData.PatternData("P5", 5, 5));
            var sixthPointData = new SearchPointData(new List<double>() { 0.7, 0.8, 0.8, 0.7 }, new SearchPointData.PatternData("P6", 6, 6));
            var seventhPointData = new SearchPointData(new List<double>() { 0.7, 0.8, 0.9, 0.8 }, new SearchPointData.PatternData("P7", 7, 7));
            searchPointData.Add(firstPointData);
            searchPointData.Add(secondPointData);
            searchPointData.Add(thirdPointData);
            searchPointData.Add(fourthPointData);
            searchPointData.Add(fifthPointData);
            searchPointData.Add(sixthPointData);
            searchPointData.Add(seventhPointData);

            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            Services.FunctionalService = functionalServiceMock.Object;
            functionalServiceMock.Setup(func => func.CreateCaptureFailureTest("plistName", "level", "timing", ulong.MaxValue, 1, string.Empty)).Returns(funcTestMock.Object);
            var voltageMock = new Mock<IVoltage>(MockBehavior.Strict);
            var searchExtensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);

            var scoreBoardLoggerMock = new Mock<IScoreboardLogger>(MockBehavior.Strict);
            var scoreboardServiceMock = new Mock<IScoreboardService>(MockBehavior.Strict);
            Services.ScoreBoardService = scoreboardServiceMock.Object;

            var plistExecutionParameters = new PrimeVminSearchTestMethod.PlistExecutionParameters()
            {
                Patlist = "plistName",
                LevelsTc = "level",
                TimingsTc = "timing",
                PrePlist = string.Empty,
            };

            var scoreboardExecutionParameters = new ScoreboardPointTest.ScoreboardExecutionParameters()
            {
                MaxFails = ulong.MaxValue,
                EdgeTicks = 2,
                BaseNumbers = new List<int>(),
                PatternNameMap = "PatternNameMap",
                PrintScoreboardCounters = false,
            };

            var pointTest = new ScoreboardPointTest(plistExecutionParameters, scoreboardExecutionParameters, voltageMock.Object, searchExtensionsMock.Object, consoleServiceMock.Object);
            pointTest.Execute(searchPointData, maskBits, string.Empty, true);

            funcTestMock.VerifyAll();
            functionalServiceMock.VerifyAll();
            voltageMock.VerifyAll();
            searchExtensionsMock.VerifyAll();
            scoreboardServiceMock.VerifyAll();
            scoreBoardLoggerMock.VerifyAll();
        }
    }
}