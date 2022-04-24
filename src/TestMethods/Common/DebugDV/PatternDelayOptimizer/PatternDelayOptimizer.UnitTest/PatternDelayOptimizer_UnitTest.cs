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

namespace PatternDelayOptimizer.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PatternService;
    using Prime.PinService;
    using Prime.PlistService;
    using Prime.TestProgramService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class PatternDelayOptimizer_UnitTest
    {
        private Mock<IPinService> PinServiceMock { get; set; }

        private Mock<ICaptureFailureTest> FuncTestMock { get; set; }

        private Mock<IFunctionalService> FuncServiceMock { get; set; }

        private Mock<IPlistObject> PlistMock { get; set; }

        private Mock<IPlistService> PlistServiceMock { get; set; }

        private Mock<IFileService> FileServiceMock { get; set; }

        private Mock<IFileSystem> FileSystemMock { get; set; }

        private List<Mock<IPatConfigHandle>> PatConfigHandleMock { get; set; }

        private Mock<IPatConfigService> PatConfigServiceMock { get; set; }

        private Mock<IPatternService> PatternServiceMock { get; set; }

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        /// <summary>
        /// Setup all the standard mocks.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => System.Console.WriteLine(s));
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int i, string s2, string s3) => System.Console.WriteLine(msg));
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_MaskPinDNE_Exception()
        {
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(o => o.Exists("FakePin1")).Returns(false);
            Prime.Services.PinService = pinServiceMock.Object;

            PatternDelayOptimizer underTest = new PatternDelayOptimizer { MaskPins = "FakePin1" };

            // [2] Call the method under test.
            var ex = Assert.ThrowsException<TestMethodException>(() => underTest.Verify());

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual("Mask pin=[FakePin1] does not exist.", ex.Message);
            pinServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamNoEmpty_True()
        {
            this.SetupSimpleMocks(false, preplist: "PrePListCallback");

            PatternDelayOptimizer underTest = new PatternDelayOptimizer
            {
                Patlist = "SomePlist",
                TimingsTc = "SomeTiming",
                LevelsTc = "SomeLevels",
                MaskPins = "SomePin",
                PrePlist = "PrePListCallback",
                SearchValueMin = "50",
                SearchValueResolution = "10",
                SearchMethod = PatternDelayOptimizer.SearchType.LinearHighToLow,

                PatmodConfig = "ConfigName",
                PatmodInputFile = "SearchConfig.json",
                PatmodOutputFile = "Output.patmod.json",
                SummaryOutputFile = "Output.summary.json",
                FileWrapper = this.FileSystemMock.Object,
            };

            // [2] Call the method under test.
            underTest.Verify();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(true);
            this.VerifySimpleMocks(false);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ConfigNotFound_Exception()
        {
            this.FileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            this.FileServiceMock.Setup(o => o.FileExists("SearchConfig.json")).Returns(true);
            this.FileServiceMock.Setup(o => o.GetFile("SearchConfig.json")).Returns("SearchConfig.json");
            Prime.Services.FileService = this.FileServiceMock.Object;

            var config = "{'Configurations': [{'Name': 'ConfigName','ConfigurationElement':[{'Type': 'INSTRUCTION','Domain': 'LEG','StartAddress': 'HVM_TEST_WAIT','StartAddressOffset': 84,'EndAddress': 'HVM_TEST_WAIT','EndAddressOffset': 84,'PatternsRegEx': ['^[gds].*']}]}]}";
            this.FileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
            this.FileSystemMock.Setup(o => o.File.ReadAllText("SearchConfig.json")).Returns(config);

            PatternDelayOptimizer underTest = new PatternDelayOptimizer
            {
                Patlist = "SomePlist",
                TimingsTc = "SomeTiming",
                LevelsTc = "SomeLevels",

                PatmodConfig = "BadConfig",
                PatmodInputFile = "SearchConfig.json",
                FileWrapper = this.FileSystemMock.Object,
            };

            // [2] Call the method under test.
            var ex = Assert.ThrowsException<TestMethodException>(() => underTest.Verify());
            Assert.AreEqual("PatmodFile=[SearchConfig.json] does not contain Configuration=[BadConfig].", ex.Message);
            this.FileServiceMock.VerifyAll();
            this.FileSystemMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_SimpleCaseNoRestore_True()
        {
            this.SetupSimpleMocks(true);

            PatternDelayOptimizer underTest = new PatternDelayOptimizer
            {
                Patlist = "SomePlist",
                TimingsTc = "SomeTiming",
                LevelsTc = "SomeLevels",
                MaskPins = "SomePin",
                PerRunPatternLimit = 2,
                MaxTestpoints = 2,
                GuardbandMultiplier = 0.1,
                RestorePatterns = PatternDelayOptimizer.MyBool.False,

                PatmodConfig = "ConfigName",
                PatmodInputFile = "SearchConfig.json",
                PatmodOutputFile = "Output.patmod.json",
                SummaryOutputFile = "Output.summary.json",
                FileWrapper = this.FileSystemMock.Object,
            };

            // [2] Call the method under test.
            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(true);
            this.VerifySimpleMocks(true);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_SimpleCaseReloadAleph_True()
        {
            this.SetupSimpleMocks(true, reloadAleph: true);

            PatternDelayOptimizer underTest = new PatternDelayOptimizer
            {
                Patlist = "SomePlist",
                TimingsTc = "SomeTiming",
                LevelsTc = "SomeLevels",
                MaskPins = "SomePin",
                PerRunPatternLimit = 2,
                MaxTestpoints = 2,
                GuardbandMultiplier = 0.1,
                RestorePatterns = PatternDelayOptimizer.MyBool.False,
                ReloadPatConfig = PatternDelayOptimizer.MyBool.True,

                PatmodConfig = "ConfigName",
                PatmodInputFile = "SearchConfig.json",
                PatmodOutputFile = "Output.patmod.json",
                SummaryOutputFile = "Output.summary.json",
                FileWrapper = this.FileSystemMock.Object,
            };

            // [2] Call the method under test.
            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(true);
            this.VerifySimpleMocks(true);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_SimpleCaseWithRestore_True()
        {
            this.SetupSimpleMocks(true, restorePatterns: true);

            PatternDelayOptimizer underTest = new PatternDelayOptimizer
            {
                Patlist = "SomePlist",
                TimingsTc = "SomeTiming",
                LevelsTc = "SomeLevels",
                MaskPins = "SomePin",
                PerRunPatternLimit = 2,
                MaxTestpoints = 2,
                GuardbandMultiplier = 0.1,
                RestorePatterns = PatternDelayOptimizer.MyBool.True,

                PatmodConfig = "ConfigName",
                PatmodInputFile = "SearchConfig.json",
                PatmodOutputFile = "Output.patmod.json",
                SummaryOutputFile = "Output.summary.json",
                FileWrapper = this.FileSystemMock.Object,
            };

            // [2] Call the method under test.
            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(true);
            this.VerifySimpleMocks(true);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_SimpleCaseMaxOverride_True()
        {
            this.SetupSimpleMocks(true, patternMax: "2000");

            PatternDelayOptimizer underTest = new PatternDelayOptimizer
            {
                Patlist = "SomePlist",
                TimingsTc = "SomeTiming",
                LevelsTc = "SomeLevels",
                MaskPins = "SomePin",
                PerRunPatternLimit = 2,
                MaxTestpoints = 2,
                GuardbandMultiplier = 0.1,
                RestorePatterns = PatternDelayOptimizer.MyBool.False,
                SearchValueMax = 50000,
                SearchValueMin = 1000,

                PatmodConfig = "ConfigName",
                PatmodInputFile = "SearchConfig.json",
                PatmodOutputFile = "Output.patmod.json",
                SummaryOutputFile = "Output.summary.json",
                FileWrapper = this.FileSystemMock.Object,
            };

            // [2] Call the method under test.
            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(true);
            this.VerifySimpleMocks(true);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void PatternContainer_UnknownLabel_Exception()
        {
            // setup the patmod configuration
            List<PatModConfiguration.ConfigElement> configElements = new List<PatModConfiguration.ConfigElement>
            {
                JsonConvert.DeserializeObject<PatModConfiguration.ConfigElement>("{'Domain': 'LEG', 'StartAddress': 'SearchLabel'}"),
            };

            // setup the Mocks
            var patternServiceMock = new Mock<IPatternService>(MockBehavior.Strict);
            var s = new MockSequence();
            patternServiceMock.InSequence(s).Setup(o => o.GetLabelFromAddress("pat1", "LEG", 0, false)).Throws(new FatalException("Prime Fatal Exception"));
            patternServiceMock.InSequence(s).Setup(o => o.GetLabelFromAddress("pat1", "LEG", 0, false)).Throws(new System.Exception("Unknown Exception"));
            Prime.Services.PatternService = patternServiceMock.Object;

            // run the test
            var pat1 = new PatternContainer("pat1", "CommonPatConfig", 2000, 0, 1000, configElements, Prime.Services.ConsoleService);
            Assert.IsFalse(pat1.Valid);
            this.ConsoleServiceMock.Verify(o => o.PrintDebug("[PrimeError] Cannot find Label=[^SearchLabel$] in Domain=[LEG] Pattern=[pat1]."));

            var pat2 = new PatternContainer("pat1", "CommonPatConfig", 2000, 0, 1000, configElements, Prime.Services.ConsoleService);
            Assert.IsFalse(pat2.Valid);
            this.ConsoleServiceMock.Verify(o => o.PrintDebug("[SystemError] Cannot find Label=[^SearchLabel$] in Domain=[LEG] Pattern=[pat1]."));

            // check the results.
            patternServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void PatternContainer_WrongInstruction_Exception()
        {
            // setup the patmod configuration
            List<PatModConfiguration.ConfigElement> configElements = new List<PatModConfiguration.ConfigElement>
            {
                JsonConvert.DeserializeObject<PatModConfiguration.ConfigElement>("{'Domain': 'LEG', 'StartAddress': 'SearchLabel'}"),
            };

            // setup the Mocks
            var legLabelMock = new Mock<ILabel>(MockBehavior.Strict);
            legLabelMock.Setup(o => o.GetAddress()).Returns(100);
            legLabelMock.Setup(o => o.GetName()).Returns("SearchLabel");

            var patternServiceMock = new Mock<IPatternService>(MockBehavior.Strict);
            patternServiceMock.Setup(o => o.GetLabelFromAddress("pat1", "LEG", 0, false)).Returns(legLabelMock.Object);

            patternServiceMock.Setup(o => o.ReadInstruction("pat1", "LEG", 100)).Returns(new System.Tuple<string, string>("BLAH", "40000, R7"));
            Prime.Services.PatternService = patternServiceMock.Object;

            // run the test
            var pat = new PatternContainer("pat1", "CommonPatConfig", 2000, 0, 1000, configElements, Prime.Services.ConsoleService);
            Assert.IsFalse(pat.Valid);
            this.ConsoleServiceMock.Verify(o => o.PrintDebug("Expecting MOV or RPT instruction, got [BLAH] at Vec=[100] Domain=[LEG] Pattern=[pat1]. Marking pattern as invalid."));

            // check the results.
            patternServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void PatternContainer_BadDomainMult_Exception()
        {
            // setup the patmod configuration
            var configElements = new List<PatModConfiguration.ConfigElement>
            {
                JsonConvert.DeserializeObject<PatModConfiguration.ConfigElement>("{'Domain': 'LEG', 'StartAddress': 'HVM_TEST_WAIT_LEG'}"),
                JsonConvert.DeserializeObject<PatModConfiguration.ConfigElement>("{'Domain': 'DDR', 'StartAddress': 'HVM_TEST_WAIT_DDR'}"),
            };

            // setup the Mocks
            var patConfigHandleMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("CommonPatConfig", "^pat1$")).Returns(patConfigHandleMock.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var legLabelMock = new Mock<ILabel>(MockBehavior.Strict);
            legLabelMock.Setup(o => o.GetAddress()).Returns(100);
            legLabelMock.Setup(o => o.GetName()).Returns("HVM_TEST_WAIT_LEG");

            var ddrLabelMock = new Mock<ILabel>(MockBehavior.Strict);
            ddrLabelMock.Setup(o => o.GetAddress()).Returns(100);
            ddrLabelMock.Setup(o => o.GetName()).Returns("HVM_TEST_WAIT_DDR");

            var patternServiceMock = new Mock<IPatternService>(MockBehavior.Strict);
            patternServiceMock.Setup(o => o.GetLabelFromAddress("pat1", "LEG", 0, false)).Returns(legLabelMock.Object);
            patternServiceMock.Setup(o => o.GetLabelFromAddress("pat1", "DDR", 0, false)).Returns(ddrLabelMock.Object);

            patternServiceMock.Setup(o => o.ReadInstruction("pat1", "LEG", 100)).Returns(new System.Tuple<string, string>("MOV", "0x9c40, R7"));
            patternServiceMock.Setup(o => o.ReadInstruction("pat1", "DDR", 100)).Returns(new System.Tuple<string, string>("MOV", "0xea60, R7"));
            Prime.Services.PatternService = patternServiceMock.Object;

            // run the test
            var ex = Assert.ThrowsException<TestMethodException>(() => new PatternContainer("pat1", "CommonPatConfig", 2000, 0, 1000, configElements, Prime.Services.ConsoleService));
            Assert.AreEqual("Expecting all wait times to be integer multipliers of the first domain. BaseDomain[LEG]=[40000] TargetDomain[DDR]=[60000].", ex.Message);

            // check the results.
            patConfigHandleMock.VerifyAll();
            patConfigServiceMock.VerifyAll();
            patternServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void PatternContainer_Constructor_Pass()
        {
            // setup the patmod configuration
            var configElementsJson = new List<string>
            {
                "{'Type': 'INSTRUCTION', 'Domain': 'LEG', 'StartAddress': 'HVM_TEST_WAIT_LEG', 'StartAddressOffset': 123, 'EndAddress': 'HVM_TEST_WAIT_LEG', 'EndAddressOffset': 123,'PatternsRegEx': ['^[gds].*'], 'ValidationMode': 'ALLOW_LABEL_NO_MATCHING'}",
                "{'Type': 'INSTRUCTION', 'Domain': 'DDR', 'StartAddress': 'HVM_TEST_WAIT_DDR', 'StartAddressOffset':  57, 'EndAddress': 'HVM_TEST_WAIT_DDR', 'EndAddressOffset':  57,'PatternsRegEx': ['^[gds].*'], 'ValidationMode': 'ALLOW_LABEL_NO_MATCHING'}",
            };

            List<PatModConfiguration.ConfigElement> configElements = new List<PatModConfiguration.ConfigElement>(2);
            foreach (var json in configElementsJson)
            {
                configElements.Add(JsonConvert.DeserializeObject<PatModConfiguration.ConfigElement>(json));
            }

            // setup the Mocks
            var patConfigHandleMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("CommonPatConfig", "^pat1$")).Returns(patConfigHandleMock.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var wrongLabelMock = new Mock<ILabel>(MockBehavior.Strict);
            wrongLabelMock.Setup(o => o.GetAddress()).Returns(32);
            wrongLabelMock.Setup(o => o.GetName()).Returns("wronglabelname");

            var legLabelMock = new Mock<ILabel>(MockBehavior.Strict);
            legLabelMock.Setup(o => o.GetAddress()).Returns(100);
            legLabelMock.Setup(o => o.GetName()).Returns("HVM_TEST_WAIT_LEG");

            var ddrLabelMock = new Mock<ILabel>(MockBehavior.Strict);
            ddrLabelMock.Setup(o => o.GetAddress()).Returns(100);
            ddrLabelMock.Setup(o => o.GetName()).Returns("HVM_TEST_WAIT_DDR");

            var patternServiceMock = new Mock<IPatternService>(MockBehavior.Strict);
            var s = new MockSequence();
            patternServiceMock.InSequence(s).Setup(o => o.GetLabelFromAddress("pat1", "LEG", 0, false)).Returns(wrongLabelMock.Object);
            patternServiceMock.InSequence(s).Setup(o => o.GetLabelFromAddress("pat1", "LEG", 33, false)).Returns(legLabelMock.Object);
            patternServiceMock.Setup(o => o.GetLabelFromAddress("pat1", "DDR", 0, false)).Returns(ddrLabelMock.Object);

            patternServiceMock.Setup(o => o.ReadInstruction("pat1", "LEG", 223)).Returns(new System.Tuple<string, string>("MOV", "0x9c40, R7"));
            patternServiceMock.Setup(o => o.ReadInstruction("pat1", "DDR", 157)).Returns(new System.Tuple<string, string>("RPT", "80000"));
            Prime.Services.PatternService = patternServiceMock.Object;

            // run the test
            var pattern = new PatternContainer("pat1", "CommonPatConfig", 2000, 0, 1000, configElements, Prime.Services.ConsoleService);

            // check the results.
            Assert.AreEqual(2000, pattern.BinarySearchLowerValue);
            Assert.AreEqual(1000, pattern.Resolution);
            Assert.AreEqual(40000, pattern.BinarySearchUpperValue);

            Assert.AreEqual(2, pattern.PatModTemplate.Count);
            Assert.AreEqual("MOV %COUNT%, R7", pattern.PatModTemplate[0]);
            Assert.AreEqual("RPT %COUNT%", pattern.PatModTemplate[1]);

            Assert.AreEqual(2, pattern.DomainMultiplier.Count);
            Assert.AreEqual(1, pattern.DomainMultiplier[0]);
            Assert.AreEqual(2, pattern.DomainMultiplier[1]);

            patConfigHandleMock.VerifyAll();
            patConfigServiceMock.VerifyAll();
            patternServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test PatternContainer.SetToFinalValue().
        /// </summary>
        [TestMethod]
        public void SetToLastPassing_AlreaydHasResult_Pass()
        {
            var t = this.MocksForSearchOnly(4000, "pat1", "CommonPatConfig");
            var patConfigHandleMock = t.Item1;
            var elements = t.Item2;

            var s = new MockSequence();
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 0, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 0, R7"));

            var pattern = new PatternContainer("pat1", "CommonPatConfig", 0, 0, 100, elements, Prime.Services.ConsoleService);
            pattern.ResetForInitialSearch(true);

            Assert.IsTrue(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { }, false, true, false)); // testpoint = 0
            Assert.AreEqual("MOV 0, R7", pattern.CurrentPatMod);
            Assert.AreEqual(0, pattern.CurrentSearchValue);
            Assert.AreEqual("0:True", pattern.GetResultsAsString());

            pattern.SetToFinalValue(0);
            Assert.AreEqual("MOV 0, R7", pattern.CurrentPatMod);
            Assert.AreEqual(0, pattern.CurrentSearchValue);
            Assert.AreEqual("0:True", pattern.GetResultsAsString());

            patConfigHandleMock.VerifyAll();
        }

        /// <summary>
        /// Test PatternContainer.SetToFinalValue().
        /// </summary>
        [TestMethod]
        public void SetToLastPassing_HasPassingResult_Pass()
        {
            var t = this.MocksForSearchOnly(5000, "pat1", "CommonPatConfig");
            var patConfigHandleMock = t.Item1;
            var elements = t.Item2;

            var s = new MockSequence();
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 1000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 5000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 3000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 4000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 5000, R7"));

            var pattern = new PatternContainer("pat1", "CommonPatConfig", 1000, 0, 1000, elements, Prime.Services.ConsoleService);
            pattern.ResetForInitialSearch(true);

            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, true, false)); // testpoint = 0/1000
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { }, false, true, false)); // testpoint = 4/5000
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, true, false)); // testpoint = 2/3000

            pattern.SetToFinalValue(0);
            Assert.AreEqual("MOV 5000, R7", pattern.CurrentPatMod);
            Assert.AreEqual(5000, pattern.CurrentSearchValue);
            Assert.AreEqual("1000:False|5000:True|3000:False", pattern.GetResultsAsString());
            patConfigHandleMock.VerifyAll();
        }

        /// <summary>
        /// Test PatternContainer.SetToFinalValue().
        /// </summary>
        [TestMethod]
        public void SetToLastPassing_NoPassingResult_Pass()
        {
            var t = this.MocksForSearchOnly(5000, "pat1", "CommonPatConfig");
            var patConfigHandleMock = t.Item1;
            var elements = t.Item2;

            patConfigHandleMock.Setup(o => o.SetData("MOV 1000, R7"));
            patConfigHandleMock.Setup(o => o.SetData("MOV 5000, R7"));

            var pattern = new PatternContainer("pat1", "CommonPatConfig", 1000, 0, 1000, elements, Prime.Services.ConsoleService);
            pattern.ResetForInitialSearch(true);

            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, true, false)); // testpoint = 0

            pattern.SetToFinalValue(0);
            Assert.AreEqual(string.Empty, pattern.CurrentPatMod);
            Assert.AreEqual("1000:False", pattern.GetResultsAsString());
            patConfigHandleMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void UpdateForNextSearchBinary_FPPxP_Pass()
        {
            var t = this.MocksForSearchOnly(4000, "pat1", "CommonPatConfig");
            var patConfigHandleMock = t.Item1;
            var elements = t.Item2;

            var s = new MockSequence();
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 0, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 4000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 2000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 1000, R7"));

            var pattern = new PatternContainer("pat1", "CommonPatConfig", 0, 0, 1000, elements, Prime.Services.ConsoleService);
            pattern.ResetForInitialSearch(true);

            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, true, false)); // testpoint = 0
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { }, false, true, false)); // testpoint = 4
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { }, false, true, false)); // testpoint = 2
            Assert.IsTrue(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { }, false, true, false)); // testpoint = 1

            Assert.AreEqual("MOV 1000, R7", pattern.CurrentPatMod);
            Assert.AreEqual(1000, pattern.CurrentSearchValue);
            Assert.AreEqual("0:False|4000:True|2000:True|1000:True", pattern.GetResultsAsString());
            patConfigHandleMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void UpdateForNextSearchBinary_FFPxP_Pass()
        {
            var t = this.MocksForSearchOnly(5000, "pat1", "CommonPatConfig");
            var patConfigHandleMock = t.Item1;
            var elements = t.Item2;

            var s = new MockSequence();
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 1000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 5000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 3000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 2000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 3000, R7"));

            var pattern = new PatternContainer("pat1", "CommonPatConfig", 1000, 0, 1000, elements, Prime.Services.ConsoleService);
            pattern.ResetForInitialSearch(true);

            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, true, false)); // testpoint = 0
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { }, false, true, false)); // testpoint = 4
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { }, false, true, false)); // testpoint = 2
            Assert.IsTrue(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, true, false)); // testpoint = 1

            Assert.AreEqual("MOV 3000, R7", pattern.CurrentPatMod);
            Assert.AreEqual(3000, pattern.CurrentSearchValue);
            Assert.AreEqual("1000:False|5000:True|3000:True|2000:False", pattern.GetResultsAsString());
            patConfigHandleMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void UpdateForNextSearchBinary_FxFFP_Pass()
        {
            var t = this.MocksForSearchOnly(5000, "pat1", "CommonPatConfig");
            var patConfigHandleMock = t.Item1;
            var elements = t.Item2;

            var s = new MockSequence();
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 1000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 5000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 3000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 4000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 5000, R7"));

            var pattern = new PatternContainer("pat1", "CommonPatConfig", 1000, 0, 1000, elements, Prime.Services.ConsoleService);
            pattern.ResetForInitialSearch(true);

            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, true, false)); // testpoint = 0
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { }, false, true, false)); // testpoint = 4
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, true, false)); // testpoint = 2
            Assert.IsTrue(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, true, false)); // testpoint = 3

            Assert.AreEqual("MOV 5000, R7", pattern.CurrentPatMod);
            Assert.AreEqual(5000, pattern.CurrentSearchValue);
            Assert.AreEqual("1000:False|5000:True|3000:False|4000:False", pattern.GetResultsAsString());
            patConfigHandleMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void UpdateForNextSearchBinary_FxFPP_Pass()
        {
            var t = this.MocksForSearchOnly(5000, "pat1", "CommonPatConfig");
            var patConfigHandleMock = t.Item1;
            var elements = t.Item2;

            var s = new MockSequence();
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 1000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 5000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 3000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 4000, R7"));

            var pattern = new PatternContainer("pat1", "CommonPatConfig", 1000, 0, 1000, elements, Prime.Services.ConsoleService);
            pattern.ResetForInitialSearch(true);

            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, true, false)); // testpoint = 0
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { }, false, true, false)); // testpoint = 4
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, true, false)); // testpoint = 2
            Assert.IsTrue(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { }, false, true, false)); // testpoint = 3

            Assert.AreEqual("MOV 4000, R7", pattern.CurrentPatMod);
            Assert.AreEqual(4000, pattern.CurrentSearchValue);
            Assert.AreEqual("1000:False|5000:True|3000:False|4000:True", pattern.GetResultsAsString());
            patConfigHandleMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void UpdateForNextSearchBinary_FxxxF_Pass()
        {
            var t = this.MocksForSearchOnly(5000, "pat1", "CommonPatConfig");
            var patConfigHandleMock = t.Item1;
            var elements = t.Item2;

            var s = new MockSequence();
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 1000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 5000, R7"));

            var pattern = new PatternContainer("pat1", "CommonPatConfig", 1000, 0, 1000, elements, Prime.Services.ConsoleService);
            pattern.ResetForInitialSearch(true);

            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, true, false)); // testpoint = 0
            Assert.IsTrue(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, true, false)); // testpoint = 4

            Assert.AreEqual(string.Empty, pattern.CurrentPatMod);
            Assert.AreEqual(5000, pattern.CurrentSearchValue);
            Assert.AreEqual("1000:False|5000:False", pattern.GetResultsAsString());
            patConfigHandleMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void UpdateForNextSearchBinary_Pxxxx_Pass()
        {
            var t = this.MocksForSearchOnly(4000, "pat1", "CommonPatConfig");
            var patConfigHandleMock = t.Item1;
            var elements = t.Item2;

            var s = new MockSequence();
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 0, R7"));

            var pattern = new PatternContainer("pat1", "CommonPatConfig", 0, 0, 100, elements, Prime.Services.ConsoleService);
            pattern.ResetForInitialSearch(true);

            Assert.IsTrue(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { }, false, true, false)); // testpoint = 0
            Assert.AreEqual("MOV 0, R7", pattern.CurrentPatMod);
            Assert.AreEqual(0, pattern.CurrentSearchValue);
            Assert.AreEqual("0:True", pattern.GetResultsAsString());
            patConfigHandleMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void UpdateForNextSearchLinearLowToHigh_FFP_Pass()
        {
            var t = this.MocksForSearchOnly(5000, "pat1", "CommonPatConfig");
            var patConfigHandleMock = t.Item1;
            var elements = t.Item2;

            var s = new MockSequence();
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 1000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 2000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 3000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 3000, R7"));

            var pattern = new PatternContainer("pat1", "CommonPatConfig", 1000, 0, 1000, elements, Prime.Services.ConsoleService);
            pattern.ResetForInitialSearch(true);

            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, false, true));
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, false, true));
            Assert.IsTrue(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string>(), false, false, true));

            Assert.AreEqual("MOV 3000, R7", pattern.CurrentPatMod);
            Assert.AreEqual(3000, pattern.CurrentSearchValue);
            Assert.AreEqual("1000:False|2000:False|3000:True", pattern.GetResultsAsString());

            pattern.SetToFinalValue(0);
            Assert.AreEqual("MOV 3000, R7", pattern.CurrentPatMod);
            Assert.AreEqual(3000, pattern.CurrentSearchValue);

            patConfigHandleMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void UpdateForNextSearchLinearHighToLow_PPF_Pass()
        {
            var t = this.MocksForSearchOnly(5000, "pat1", "CommonPatConfig");
            var patConfigHandleMock = t.Item1;
            var elements = t.Item2;

            var s = new MockSequence();
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 5000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 4000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 3000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 4000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 4000, R7"));

            var pattern = new PatternContainer("pat1", "CommonPatConfig", 1000, 0, 1000, elements, Prime.Services.ConsoleService);
            pattern.ResetForInitialSearch(false);

            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string>(), false, false, false));
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string>(), false, false, false));
            Assert.IsTrue(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, false, false));

            Assert.AreEqual("MOV 4000, R7", pattern.CurrentPatMod);
            Assert.AreEqual(4000, pattern.CurrentSearchValue);
            Assert.AreEqual("5000:True|4000:True|3000:False", pattern.GetResultsAsString());

            pattern.SetToFinalValue(0);
            Assert.AreEqual("MOV 4000, R7", pattern.CurrentPatMod);
            Assert.AreEqual(4000, pattern.CurrentSearchValue);

            patConfigHandleMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void UpdateForNextSearchLinearLowToHigh_FailMax_Pass()
        {
            var t = this.MocksForSearchOnly(5000, "pat1", "CommonPatConfig");
            var patConfigHandleMock = t.Item1;
            var elements = t.Item2;

            var s = new MockSequence();
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 1000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 2000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 3000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 4000, R7"));
            patConfigHandleMock.InSequence(s).Setup(o => o.SetData("MOV 5000, R7"));

            var pattern = new PatternContainer("pat1", "CommonPatConfig", 1000, 0, 1000, elements, Prime.Services.ConsoleService);
            pattern.ResetForInitialSearch(true);

            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, false, true));
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, false, true));
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, false, true));
            Assert.IsFalse(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, false, true));
            Assert.IsTrue(pattern.ReadResultsAndUpdateForNextTestPoint(new HashSet<string> { "pat1" }, false, false, true));

            Assert.AreEqual(string.Empty, pattern.CurrentPatMod);
            Assert.AreEqual(5000, pattern.CurrentSearchValue);
            Assert.AreEqual("1000:False|2000:False|3000:False|4000:False|5000:False", pattern.GetResultsAsString());

            pattern.SetToFinalValue(0);
            Assert.AreEqual(string.Empty, pattern.CurrentPatMod);

            patConfigHandleMock.VerifyAll();
        }

        /// <summary>
        /// Create a complicated patmod output file.
        /// </summary>
        [TestMethod]
        public void PatModOutputConfiguration_BuildOutput_Pass()
        {
            // create a realistic input configuration.
            PatModConfiguration.Config configuration = new PatModConfiguration.Config();
            configuration.Name = "BasePatMod";

            var element1 = new PatModConfiguration.ConfigElement();
            element1.Domain = "LEG";
            element1.StartAddress = "HVM_TEST_WAIT_LEG";
            element1.StartAddressOffset = 123;
            element1.EndAddress = "HVM_TEST_WAIT_LEG";
            element1.EndAddressOffset = 123;

            var element2 = new PatModConfiguration.ConfigElement();
            element2.Domain = "DDR";
            element2.StartAddress = "HVM_TEST_WAIT_DDR";
            element2.StartAddressOffset = 57;
            element2.EndAddress = "HVM_TEST_WAIT_DDR";
            element2.EndAddressOffset = 57;

            configuration.ConfigurationElement = new List<PatModConfiguration.ConfigElement>();
            configuration.ConfigurationElement.Add(element1);
            configuration.ConfigurationElement.Add(element2);

            // create reasonable results.
            var data = new Dictionary<string, List<string>>
            {
                { "RPT  1000|RPT  2000", new List<string> { "Pat1", "Pat2", "Pat3" } },
                { "RPT  5000|RPT 10000", new List<string> { "Pat4" } },
                { "RPT 20000|RPT 40000", new List<string> { "Pat7", "Pat8" } },
            };

            var result = PatModConfiguration.BuildOutput(configuration, data);
            var resultJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            System.Console.WriteLine(resultJson);
            Assert.AreEqual(1, result.Configurations.Count);
            Assert.AreEqual("ImpactStudiesOptimumWaits", result.Configurations[0].Name);
            Assert.AreEqual(6, result.Configurations[0].ConfigurationElement.Count);

            var expectedResults = new List<string>
            {
                "{'Type': 'INSTRUCTION', 'Domain': 'LEG', 'StartAddress': 'HVM_TEST_WAIT_LEG', 'StartAddressOffset': 123, 'EndAddress': 'HVM_TEST_WAIT_LEG', 'EndAddressOffset': 123,'PatternsRegEx': ['^Pat1$','^Pat2$','^Pat3$'], 'ValidationMode': 'ALLOW_LABEL_NO_MATCHING', 'Data': 'RPT  1000'}",
                "{'Type': 'INSTRUCTION', 'Domain': 'DDR', 'StartAddress': 'HVM_TEST_WAIT_DDR', 'StartAddressOffset':  57, 'EndAddress': 'HVM_TEST_WAIT_DDR', 'EndAddressOffset':  57,'PatternsRegEx': ['^Pat1$','^Pat2$','^Pat3$'], 'ValidationMode': 'ALLOW_LABEL_NO_MATCHING', 'Data': 'RPT  2000'}",

                "{'Type': 'INSTRUCTION', 'Domain': 'LEG', 'StartAddress': 'HVM_TEST_WAIT_LEG', 'StartAddressOffset': 123, 'EndAddress': 'HVM_TEST_WAIT_LEG', 'EndAddressOffset': 123,'PatternsRegEx': ['^Pat4$'], 'ValidationMode': 'ALLOW_LABEL_NO_MATCHING', 'Data': 'RPT  5000'}",
                "{'Type': 'INSTRUCTION', 'Domain': 'DDR', 'StartAddress': 'HVM_TEST_WAIT_DDR', 'StartAddressOffset':  57, 'EndAddress': 'HVM_TEST_WAIT_DDR', 'EndAddressOffset':  57,'PatternsRegEx': ['^Pat4$'], 'ValidationMode': 'ALLOW_LABEL_NO_MATCHING', 'Data': 'RPT 10000'}",

                "{'Type': 'INSTRUCTION', 'Domain': 'LEG', 'StartAddress': 'HVM_TEST_WAIT_LEG', 'StartAddressOffset': 123, 'EndAddress': 'HVM_TEST_WAIT_LEG', 'EndAddressOffset': 123,'PatternsRegEx': ['^Pat7$','^Pat8$'], 'ValidationMode': 'ALLOW_LABEL_NO_MATCHING', 'Data': 'RPT 20000'}",
                "{'Type': 'INSTRUCTION', 'Domain': 'DDR', 'StartAddress': 'HVM_TEST_WAIT_DDR', 'StartAddressOffset':  57, 'EndAddress': 'HVM_TEST_WAIT_DDR', 'EndAddressOffset':  57,'PatternsRegEx': ['^Pat7$','^Pat8$'], 'ValidationMode': 'ALLOW_LABEL_NO_MATCHING', 'Data': 'RPT 40000'}",
            };

            foreach (var i in Enumerable.Range(0, 6))
            {
                var obj = JsonConvert.DeserializeObject<PatModConfiguration.ConfigElement>(expectedResults[i]);
                Assert.IsTrue(this.AreEqual(obj, result.Configurations[0].ConfigurationElement[i]), $"Failed for Element {i}");
            }
        }

        private bool AreEqual(PatModConfiguration.ConfigElement elem1, PatModConfiguration.ConfigElement elem2)
        {
            bool notequal = elem1.Type != elem2.Type;
            notequal |= elem1.Domain != elem2.Domain;
            notequal |= elem1.StartAddress != elem2.StartAddress;
            notequal |= elem1.StartAddressOffset != elem2.StartAddressOffset;
            notequal |= elem1.EndAddress != elem2.EndAddress;
            notequal |= elem1.EndAddressOffset != elem2.EndAddressOffset;
            notequal |= elem1.ValidationMode != elem2.ValidationMode;
            notequal |= elem1.Data != elem2.Data;
            notequal |= elem1.Domain != elem2.Domain;
            notequal |= !elem1.PatternsRegEx.SequenceEqual(elem2.PatternsRegEx);

            return !notequal;
        }

        private void VerifySimpleMocks(bool execMode)
        {
            this.PinServiceMock.VerifyAll();
            this.FuncServiceMock.VerifyAll();
            this.FuncTestMock.VerifyAll();
            this.PlistServiceMock.VerifyAll();
            this.PlistMock.VerifyAll();
            this.FileServiceMock.VerifyAll();
            this.FileSystemMock.VerifyAll();
            this.PatConfigServiceMock.VerifyAll();
            this.PatternServiceMock.VerifyAll();
            if (execMode)
            {
                this.PatConfigHandleMock[0].Verify(o => o.SetData("RPT 1000")); // Pattern1 reset/starting point
                this.PatConfigHandleMock[0].Verify(o => o.SetData("RPT 1100")); // Pattern1 final + guardband

                this.PatConfigHandleMock[1].Verify(o => o.SetData("RPT 1000")); // Pattern2 reset/starting point
                this.PatConfigHandleMock[1].Verify(o => o.SetData("RPT 50000")); // Pattern2 ending point
                this.PatConfigHandleMock[1].Verify(o => o.SetData("RPT 25500")); // Pattern2 midpoint
                this.PatConfigHandleMock[1].Verify(o => o.SetData("RPT 55000")); // Pattern2 final + guardband

                this.PatConfigHandleMock[2].Verify(o => o.SetData("RPT 1000")); // Pattern3 reset/starting point
                this.PatConfigHandleMock[2].Verify(o => o.SetData("RPT 50000")); // Pattern3 ending point

                this.PatConfigHandleMock[3].Verify(o => o.SetData("RPT 1000")); // Pattern4 reset/starting point
                this.PatConfigHandleMock[3].Verify(o => o.SetData("RPT 1100")); // Pattern5 final + guardband
            }
        }

        private void SetupSimpleMocks(bool execMode, bool restorePatterns = false, string preplist = "", string patternMax = "50000", bool reloadAleph = false)
        {
            this.PinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            this.PinServiceMock.Setup(o => o.Exists("SomePin")).Returns(true);
            Prime.Services.PinService = this.PinServiceMock.Object;

            this.FuncTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            if (execMode)
            {
                this.FuncTestMock.Setup(o => o.ApplyTestConditions());
                var fail2Mock = new Mock<IFailureData>(MockBehavior.Strict);
                fail2Mock.Setup(o => o.GetPatternName()).Returns("Pattern2");
                var fail3Mock = new Mock<IFailureData>(MockBehavior.Strict);
                fail3Mock.Setup(o => o.GetPatternName()).Returns("Pattern3");

                // TODO: should set the expected number of calls for these.
                this.FuncTestMock.Setup(o => o.SetPinMask(new List<string> { "SomePin" }));
                this.FuncTestMock.Setup(o => o.Execute()).Returns(false);
                this.FuncTestMock.SetupSequence(o => o.GetPerCycleFailures())
                    .Returns(new List<IFailureData> { fail2Mock.Object }) // Group1/Run 1: Pat1 & 2 starting point
                    .Returns(new List<IFailureData> { }) // Group1 Run 2: Pat2 ending point
                    .Returns(new List<IFailureData> { fail3Mock.Object }) // Group2/Run 1: Pat3 & 4 starting point.
                    .Returns(new List<IFailureData> { fail3Mock.Object }); // Group2/Run2: Pat3 ending point.
            }

            this.FuncServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            this.FuncServiceMock.Setup(o => o.CreateCaptureFailureTest("SomePlist", "SomeLevels", "SomeTiming", 99999, 1, preplist)).Returns(this.FuncTestMock.Object);
            Prime.Services.FunctionalService = this.FuncServiceMock.Object;

            this.PlistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            this.PlistMock.Setup(o => o.GetUniquePatternNames()).Returns(new HashSet<string> { "Preamble", "Pattern1", "Pattern2", "Pattern3", "Pattern4", "PatternInvalid1" });
            this.PlistMock.Setup(o => o.IsPatternAnAmble("Preamble")).Returns(true);
            this.PlistMock.Setup(o => o.IsPatternAnAmble(It.Is<string>(p => p.StartsWith("Pattern")))).Returns(false);

            if (execMode)
            {
                this.PlistMock.Setup(o => o.EnableGivenPatternsDisableRest(new HashSet<string> { "Pattern1", "Pattern2" }));
                this.PlistMock.Setup(o => o.DisablePatterns(new HashSet<string> { "Pattern1" }));
                this.PlistMock.Setup(o => o.EnableGivenPatternsDisableRest(new HashSet<string> { "Pattern3", "Pattern4" }));
                this.PlistMock.Setup(o => o.DisablePatterns(new HashSet<string> { "Pattern4" }));
                this.PlistMock.Setup(o => o.DisablePatterns(new HashSet<string> { "Pattern3" }));
                this.PlistMock.Setup(o => o.EnableAllPatterns());
            }

            this.PlistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            this.PlistServiceMock.Setup(o => o.GetPlistObject("SomePlist")).Returns(this.PlistMock.Object);
            Prime.Services.PlistService = this.PlistServiceMock.Object;

            this.FileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            this.FileServiceMock.Setup(o => o.FileExists("SearchConfig.json")).Returns(true);
            this.FileServiceMock.Setup(o => o.GetFile("SearchConfig.json")).Returns("SearchConfig.json");
            Prime.Services.FileService = this.FileServiceMock.Object;

            var config = "{'Configurations': [{'Name': 'ConfigName','ConfigurationElement':[{'Type': 'INSTRUCTION','Domain': 'LEG','StartAddress': 'HVM_TEST_WAIT','StartAddressOffset': 84,'EndAddress': 'HVM_TEST_WAIT','EndAddressOffset': 84,'PatternsRegEx': ['^[gds].*']}]}]}";
            this.FileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
            this.FileSystemMock.Setup(o => o.File.ReadAllText("SearchConfig.json")).Returns(config);

            var testprogramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramServiceMock.Setup(o => o.GetTestPlanPath()).Returns("C:\\mytestplan.tpl");
            Prime.Services.TestProgramService = testprogramServiceMock.Object;

            if (execMode)
            {
                var output = @"{
  ""Configurations"": [
    {
      ""Name"": ""ImpactStudiesOptimumWaits"",
      ""ConfigurationElement"": [
        {
          ""Type"": ""INSTRUCTION"",
          ""Domain"": ""LEG"",
          ""StartAddress"": ""HVM_TEST_WAIT"",
          ""StartAddressOffset"": 84,
          ""EndAddress"": ""HVM_TEST_WAIT"",
          ""EndAddressOffset"": 84,
          ""PatternsRegEx"": [
            ""^Pattern1$"",
            ""^Pattern4$""
          ],
          ""ValidationMode"": ""ALLOW_LABEL_NO_MATCHING"",
          ""Data"": ""RPT 1100""
        },
        {
          ""Type"": ""INSTRUCTION"",
          ""Domain"": ""LEG"",
          ""StartAddress"": ""HVM_TEST_WAIT"",
          ""StartAddressOffset"": 84,
          ""EndAddress"": ""HVM_TEST_WAIT"",
          ""EndAddressOffset"": 84,
          ""PatternsRegEx"": [
            ""^Pattern2$""
          ],
          ""ValidationMode"": ""ALLOW_LABEL_NO_MATCHING"",
          ""Data"": ""RPT 55000""
        }
      ]
    }
  ]
}";
                this.FileSystemMock.Setup(o => o.File.WriteAllText("C:\\Output.patmod.json", output));

                var summary = @"{
  ""ConfigName"": ""ConfigName"",
  ""ValidResults"": {
    ""RPT 1100"": [
      ""Pattern1"",
      ""Pattern4""
    ],
    ""RPT 55000"": [
      ""Pattern2""
    ]
  },
  ""InvalidPatterns"": [
    ""Pattern3""
  ],
  ""SkippedPatterns"": [
    ""Preamble"",
    ""PatternInvalid1""
  ]
}";
                this.FileSystemMock.Setup(o => o.File.WriteAllText("C:\\Output.summary.json", summary));
            }

            this.PatConfigHandleMock = new List<Mock<IPatConfigHandle>>(4);
            this.PatConfigHandleMock.Add(new Mock<IPatConfigHandle>(MockBehavior.Strict));
            this.PatConfigHandleMock.Add(new Mock<IPatConfigHandle>(MockBehavior.Strict));
            this.PatConfigHandleMock.Add(new Mock<IPatConfigHandle>(MockBehavior.Strict));
            this.PatConfigHandleMock.Add(new Mock<IPatConfigHandle>(MockBehavior.Strict));
            if (execMode)
            {
                var s1 = new MockSequence();
                this.PatConfigHandleMock[0].InSequence(s1).Setup(o => o.SetData("RPT 1000")); // Pattern1 reset/starting point
                this.PatConfigHandleMock[0].InSequence(s1).Setup(o => o.SetData("RPT 1100")); // Pattern1 final + guardband

                var s2 = new MockSequence();
                this.PatConfigHandleMock[1].InSequence(s2).Setup(o => o.SetData("RPT 1000")); // Pattern2 reset/starting point
                this.PatConfigHandleMock[1].InSequence(s2).Setup(o => o.SetData("RPT 50000")); // Pattern2 ending point
                this.PatConfigHandleMock[1].InSequence(s2).Setup(o => o.SetData("RPT 25500")); // Pattern2 midpoint
                this.PatConfigHandleMock[1].InSequence(s2).Setup(o => o.SetData("RPT 55000")); // Pattern2 final + guardband

                var s3 = new MockSequence();
                this.PatConfigHandleMock[2].InSequence(s3).Setup(o => o.SetData("RPT 1000")); // Pattern3 reset/starting point
                this.PatConfigHandleMock[2].InSequence(s3).Setup(o => o.SetData("RPT 50000")); // Pattern3 ending point

                var s4 = new MockSequence();
                this.PatConfigHandleMock[3].InSequence(s4).Setup(o => o.SetData("RPT 1000")); // Pattern4 reset/starting point
                this.PatConfigHandleMock[3].InSequence(s4).Setup(o => o.SetData("RPT 1100")); // Pattern4 final + guardband

                if (restorePatterns)
                {
                    this.PatConfigHandleMock[0].InSequence(s1).Setup(o => o.SetData($"RPT {patternMax}"));
                    this.PatConfigHandleMock[1].InSequence(s2).Setup(o => o.SetData($"RPT {patternMax}"));
                    this.PatConfigHandleMock[2].InSequence(s3).Setup(o => o.SetData($"RPT {patternMax}"));
                    this.PatConfigHandleMock[3].InSequence(s4).Setup(o => o.SetData($"RPT {patternMax}"));
                }
            }

            this.PatConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            this.PatConfigServiceMock.Setup(o => o.GetPatConfigHandle("ConfigName", "^Pattern1$")).Returns(this.PatConfigHandleMock[0].Object);
            this.PatConfigServiceMock.Setup(o => o.GetPatConfigHandle("ConfigName", "^Pattern2$")).Returns(this.PatConfigHandleMock[1].Object);
            this.PatConfigServiceMock.Setup(o => o.GetPatConfigHandle("ConfigName", "^Pattern3$")).Returns(this.PatConfigHandleMock[2].Object);
            this.PatConfigServiceMock.Setup(o => o.GetPatConfigHandle("ConfigName", "^Pattern4$")).Returns(this.PatConfigHandleMock[3].Object);
            if (execMode)
            {
                this.PatConfigServiceMock.Setup(o => o.Apply(new List<IPatConfigHandle> { this.PatConfigHandleMock[0].Object, this.PatConfigHandleMock[1].Object }));
                this.PatConfigServiceMock.Setup(o => o.Apply(new List<IPatConfigHandle> { this.PatConfigHandleMock[1].Object }));
                this.PatConfigServiceMock.Setup(o => o.Apply(new List<IPatConfigHandle> { this.PatConfigHandleMock[2].Object, this.PatConfigHandleMock[3].Object }));
                this.PatConfigServiceMock.Setup(o => o.Apply(new List<IPatConfigHandle> { this.PatConfigHandleMock[2].Object }));

                if (restorePatterns)
                {
                    this.PatConfigServiceMock.Setup(o => o.Apply(new List<IPatConfigHandle> { this.PatConfigHandleMock[0].Object, this.PatConfigHandleMock[1].Object, this.PatConfigHandleMock[2].Object, this.PatConfigHandleMock[3].Object }));
                }
            }

            if (reloadAleph)
            {
                this.PatConfigServiceMock.Setup(o => o.InitEngineeringMode(EngineeringMode.ENGINEERING_UNSAFE, It.Is<List<string>>(it => it.Count == 1 && it.Contains("SearchConfig.json"))));
            }

            Prime.Services.PatConfigService = this.PatConfigServiceMock.Object;

            var legLabelMock = new Mock<ILabel>(MockBehavior.Strict);
            legLabelMock.Setup(o => o.GetAddress()).Returns(100);
            legLabelMock.Setup(o => o.GetName()).Returns("HVM_TEST_WAIT");

            this.PatternServiceMock = new Mock<IPatternService>(MockBehavior.Strict);
            this.PatternServiceMock.Setup(o => o.GetLabelFromAddress("Pattern1", "LEG", 0, false)).Returns(legLabelMock.Object);
            this.PatternServiceMock.Setup(o => o.GetLabelFromAddress("Pattern2", "LEG", 0, false)).Returns(legLabelMock.Object);
            this.PatternServiceMock.Setup(o => o.GetLabelFromAddress("Pattern3", "LEG", 0, false)).Returns(legLabelMock.Object);
            this.PatternServiceMock.Setup(o => o.GetLabelFromAddress("Pattern4", "LEG", 0, false)).Returns(legLabelMock.Object);
            this.PatternServiceMock.Setup(o => o.GetLabelFromAddress("PatternInvalid1", "LEG", 0, false)).Throws(new TestMethodException("Label not found"));
            this.PatternServiceMock.Setup(o => o.ReadInstruction("Pattern1", "LEG", 184)).Returns(new System.Tuple<string, string>("RPT", patternMax));
            this.PatternServiceMock.Setup(o => o.ReadInstruction("Pattern2", "LEG", 184)).Returns(new System.Tuple<string, string>("RPT", patternMax));
            this.PatternServiceMock.Setup(o => o.ReadInstruction("Pattern3", "LEG", 184)).Returns(new System.Tuple<string, string>("RPT", patternMax));
            this.PatternServiceMock.Setup(o => o.ReadInstruction("Pattern4", "LEG", 184)).Returns(new System.Tuple<string, string>("RPT", patternMax));
            Prime.Services.PatternService = this.PatternServiceMock.Object;
        }

        private Tuple<Mock<IPatConfigHandle>, List<PatModConfiguration.ConfigElement>> MocksForSearchOnly(int maxValue, string patName, string patConfig)
        {
            // setup the patmod configuration
            List<PatModConfiguration.ConfigElement> configElements = new List<PatModConfiguration.ConfigElement>
            {
                JsonConvert.DeserializeObject<PatModConfiguration.ConfigElement>("{'Domain': 'LEG', 'StartAddress': 'SearchLabel'}"),
            };

            // setup the Mocks
            var patConfigHandleMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle(patConfig, $"^{patName}$")).Returns(patConfigHandleMock.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var legLabelMock = new Mock<ILabel>(MockBehavior.Strict);
            legLabelMock.Setup(o => o.GetAddress()).Returns(100);
            legLabelMock.Setup(o => o.GetName()).Returns("SearchLabel");

            var patternServiceMock = new Mock<IPatternService>(MockBehavior.Strict);
            patternServiceMock.Setup(o => o.GetLabelFromAddress(patName, "LEG", 0, false)).Returns(legLabelMock.Object);
            patternServiceMock.Setup(o => o.ReadInstruction(patName, "LEG", 100)).Returns(new System.Tuple<string, string>("MOV", $"0x{maxValue.IntegerToBinary().BinaryToHex()}, R7"));
            Prime.Services.PatternService = patternServiceMock.Object;

            return new Tuple<Mock<IPatConfigHandle>, List<PatModConfiguration.ConfigElement>>(patConfigHandleMock, configElements);
        }
    }
}
