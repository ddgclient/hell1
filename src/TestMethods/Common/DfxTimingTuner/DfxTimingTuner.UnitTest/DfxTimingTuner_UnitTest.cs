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

namespace DfxTimingTuner.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PinService;
    using Prime.SharedStorageService;
    using Prime.TestConditionService;
    using Prime.TestProgramService;
    using Prime.UserVarService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class DfxTimingTuner_UnitTest
    {
        private Mock<ICaptureFailureAndCtvPerPinTest> CtvTestMock { get; set; }

        private Mock<IPinService> PinServiceMock { get; set; }

        private Mock<IFunctionalService> FunctionalServiceMock { get; set; }

        private Mock<IFileService> FileServiceMock { get; set; }

        private Mock<IPatConfigService> PatConfigServiceMock { get; set; }

        private Mock<IUserVarService> UserVarServiceMock { get; set; }

        /// <summary>
        /// Initialize method to setup all common mocks.
        /// </summary>
        [TestInitialize]
        public void TestSetup()
        {
            // Ignore any print messages.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(p => p.PrintDebug(It.IsAny<string>())).Callback((string msg) => Console.WriteLine(msg));
            consoleServiceMock.Setup(p => p.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int i, string s1, string s2) => Console.WriteLine("ERROR:" + msg));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // Mock the testprogram service for the trigger object.
            var testprogramServiceMocks = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramServiceMocks.Setup(o => o.GetCurrentDutId()).Returns("1");
            testprogramServiceMocks.Setup(o => o.GetCurrentIpName()).Returns(string.Empty);
            Prime.Services.TestProgramService = testprogramServiceMocks.Object;
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_Pass()
        {
            this.SetupMocksForVerify();

            var underTest = new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchEnd = "2.000ns",
                SearchResolution = "100ps",
            };

            underTest.Verify();

            this.CtvTestMock.VerifyAll();
            this.FunctionalServiceMock.VerifyAll();
            this.PinServiceMock.VerifyAll();
            this.FileServiceMock.VerifyAll();
            this.UserVarServiceMock.VerifyAll();
            this.PatConfigServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_WithAdjustPinGroup_Pass()
        {
            this.SetupMocksForVerify(file: "ConfigSampleValidWithAdjustPinGroup.xml", adjustPinGroup: "mci_out");

            var underTest = new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValidWithAdjustPinGroup.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchEnd = "2.000ns",
                SearchResolution = "100ps",
            };

            underTest.Verify();

            this.CtvTestMock.VerifyAll();
            this.FunctionalServiceMock.VerifyAll();
            this.PinServiceMock.VerifyAll();
            this.FileServiceMock.VerifyAll();
            this.UserVarServiceMock.VerifyAll();
            this.PatConfigServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_WithPinAlias_Pass()
        {
            this.SetupMocksForVerify(file: "ConfigSampleValidWithAlias.xml", usePinAlias: true);
            var underTest = new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValidWithAlias.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            };

            underTest.Verify();
            this.CtvTestMock.VerifyAll();
            this.FunctionalServiceMock.VerifyAll();
            this.PinServiceMock.VerifyAll();
            this.FileServiceMock.VerifyAll();
            this.UserVarServiceMock.VerifyAll();
            this.PatConfigServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_WithAdaptiveMode_Pass()
        {
            this.SetupMocksForVerify();

            var underTest = new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-4.000ns",
                SearchEnd = "4.000ns",
                SearchResolution = "100ps",
                AdaptiveStart = "2.000ns",
                AdaptiveEnd = "2.000ns",
                AdaptiveResolution = "10ps",
            };

            underTest.Verify();

            this.CtvTestMock.VerifyAll();
            this.FunctionalServiceMock.VerifyAll();
            this.PinServiceMock.VerifyAll();
            this.FileServiceMock.VerifyAll();
            this.UserVarServiceMock.VerifyAll();
            this.PatConfigServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_BadSearchRange_Fail()
        {
            var underTest = new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            };

            underTest.Verify();

            underTest.SearchStart = "10.000ns";
            Assert.ThrowsException<ArgumentException>(() => underTest.Verify());

            underTest.SearchStart = "-2.000ns";
            underTest.SearchResolution = "3.000ns";
            Assert.ThrowsException<ArgumentException>(() => underTest.Verify());
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_BadSearchParams_Fail()
        {
            this.SetupMocksForVerify();

            var underTest = new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            };

            underTest.Verify();

            underTest.SearchStart = "NotANumber";
            Assert.ThrowsException<ArgumentException>(() => underTest.Verify());

            underTest.SearchStart = "-2.000ns";
            underTest.SearchResolution = "NotANumber";
            Assert.ThrowsException<ArgumentException>(() => underTest.Verify());

            underTest.SearchResolution = "0";
            Assert.ThrowsException<ArgumentException>(() => underTest.Verify());
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_BadMaskPin_Fail()
        {
            this.SetupMocksForVerify();
            this.PinServiceMock.Setup(o => o.Exists("MissingPin")).Returns(false);

            var underTest = new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "MissingPin",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            };

            Assert.ThrowsException<ArgumentException>(() => underTest.Verify());
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_MissingParameters_Fail()
        {
            this.SetupMocksForVerify();

            // Missing Patlist
            this.BuildAndVerifyTest(false, new DfxTimingTuner
            {
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            });

            // Missing Levels
            this.BuildAndVerifyTest(false, new DfxTimingTuner
            {
                Patlist = "FakePList",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            });

            // Missing Timing
            this.BuildAndVerifyTest(false, new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            });

            // Missing DfxTunerConfigurationFile
            this.BuildAndVerifyTest(false, new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            });

            // Missing ConfigSet
            this.BuildAndVerifyTest(false, new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValid.xml",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            });

            // Missing SearchStart
            this.BuildAndVerifyTest(false, new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigSet = "SampleCompareMode",
                ConfigFile = "ConfigSampleValid.xml",
                SearchResolution = "100ps",
                SearchEnd = "2ns",
            });

            // Missing SearchResolution
            this.BuildAndVerifyTest(false, new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigSet = "SampleCompareMode",
                ConfigFile = "ConfigSampleValid.xml",
                SearchStart = "-2.000ns",
                SearchEnd = "2.000ns",
            });

            // Missing SearchSteps
            this.BuildAndVerifyTest(false, new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigSet = "SampleCompareMode",
                ConfigFile = "ConfigSampleValid.xml",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
            });

            // Has everything
            this.BuildAndVerifyTest(true, new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigSet = "SampleCompareMode",
                ConfigFile = "ConfigSampleValid.xml",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            });
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_MissingAdjustPinGroup_Fail()
        {
            this.SetupMocksForVerify(file: "ConfigSampleValidWithAdjustPinGroup.xml");
            this.PinServiceMock.Setup(o => o.Exists("mci_out")).Returns(false);

            var underTest = new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValidWithAdjustPinGroup.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchEnd = "2.000ns",
                SearchResolution = "100ps",
            };

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.Verify());
            Assert.IsTrue(ex.Message.StartsWith("Pin/Group=[mci_out] does not exist. Referenced from 'pingroup_for_adjust' field,"));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_DriveModeUpdateCurrent_Pass()
        {
            var maskPins = "PinsToMask";
            var searchPins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var ctvData = new Dictionary<string, string>
            {
                { "DPIN_9_000", string.Concat(System.Linq.Enumerable.Repeat("111", 10)) + string.Concat(System.Linq.Enumerable.Repeat("000", 10)) + string.Concat(System.Linq.Enumerable.Repeat("111", 21)) },
            };

            // setup the functional test service mock.
            var funcMocks = this.MockFunctionalService(ctvData.Keys.ToList(), ctvData, maskPins, "IncrementDriveEdge");

            // Mock the SharedStorage for communicating with the software triggers.
            var sharedStorageMock = this.MockSharedStorageForTriggers(searchPins, 1e-10, 41);
            sharedStorageMock.Setup(s => s.GetStringRowFromTable(TriggerCallbacks.FailStorageName, Context.IP)).Returns(string.Empty);

            // Mock the PinService
            var fullPinList = new List<string>();
            fullPinList.AddRange(ctvData.Keys);
            fullPinList.AddRange(searchPins);
            var pinServiceMock = this.MockPinService(maskPins.Split(',').ToList(), fullPinList, true);

            // Add the timings to the pinServiceMock.
            var tcAttributes = new List<string> { "Drive" };
            var tcAttributeValuesCurrent = new Dictionary<string, string> { { "Drive", "2E-09" } };
            var tcAttributeValuesStart = new Dictionary<string, string> { { "Drive", "2E-09" } };
            var tcAttributeValuesResult = new Dictionary<string, string> { { "Drive", "3.5E-09" } };
            foreach (var pin in searchPins)
            {
                pinServiceMock.Setup(o => o.GetPinAttributeValues(pin, tcAttributes)).Returns(tcAttributeValuesCurrent);
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesStart));
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesResult));
            }

            // Mock the config file.
            var fileServiceMock = this.MockFileService("ConfigSampleValid.xml", true);

            // Mock the patconfig stuff.
            var patCfgMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patCfgMock.Setup(o => o.SetData("MOV 41, R1"));
            var patCfgServiceMock = this.MockPatConfigService("DfxTuneLooopSize", "FakePList", patConfigMock: patCfgMock.Object);

            // Mock the UserVarService so the needed uservars exist.
            var userVars = new List<string> { "TimingCollection1.DPIN_9_003_drv_offset", "TimingCollection1.DPIN_9_005_drv_offset", "TimingCollection1.DPIN_9_007_drv_offset" };
            var userVarServiceMock = this.MockUserVarsExistOnly(userVars, true);
            foreach (var userVar in userVars)
            {
                userVarServiceMock.Setup(o => o.GetDoubleValue(userVar)).Returns(-2e-9);
                userVarServiceMock.Setup(o => o.SetValue(userVar, "-0.500ns"));
                userVarServiceMock.Setup(o => o.SetValue(userVar, "-2.000ns"));
            }

            // Mock the testcondition service
            var tcMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcMock.Setup(o => o.Resolve());

            var tcServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcServiceMock.Setup(o => o.GetTestCondition("FakeTimings")).Returns(tcMock.Object);
            tcServiceMock.Setup(o => o.IsSmartTcEnabled()).Returns(true);
            tcServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.TIMING));
            Prime.Services.TestConditionService = tcServiceMock.Object;

            // mock the datalog/ituff.
            var ituffMocks = this.MockDatalogWrites(new Dictionary<string, string>()
            {
                { "::TimingCollection1.DPIN_9_003_drv_offset", "-0.500ns" },
                { "::TimingCollection1.DPIN_9_005_drv_offset", "-0.500ns" },
                { "::TimingCollection1.DPIN_9_007_drv_offset", "-0.500ns" },
            });

            var underTest = new DfxTimingTuner
            {
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleDriveMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            };

            // Run the Verify/Execute methods
            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            // check that all the mocks were triggered.
            funcMocks.ForEach(o => o.VerifyAll());
            ituffMocks.ForEach(o => o.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            patCfgServiceMock.VerifyAll();
            patCfgMock.VerifyAll();
            tcServiceMock.VerifyAll();
            tcMock.VerifyAll();
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_AdaptiveMode_Pass()
        {
            var maskPins = "PinsToMask";
            var searchPins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var ctvData = new Dictionary<string, string>
            {
                { "DPIN_9_000", string.Concat(System.Linq.Enumerable.Repeat("111", 10)) + string.Concat(System.Linq.Enumerable.Repeat("000", 10)) + string.Concat(System.Linq.Enumerable.Repeat("111", 21)) },
            };

            // setup the functional test service mock.
            var funcMocks = this.MockFunctionalService(ctvData.Keys.ToList(), ctvData, maskPins, "IncrementDriveEdge");

            // Mock the SharedStorage for communicating with the software triggers.
            var sharedStorageMock = this.MockSharedStorageForTriggers(searchPins, 1e-10, 41);
            sharedStorageMock.Setup(s => s.GetStringRowFromTable(TriggerCallbacks.FailStorageName, Context.IP)).Returns(string.Empty);

            // Mock the PinService
            var fullPinList = new List<string>();
            fullPinList.AddRange(ctvData.Keys);
            fullPinList.AddRange(searchPins);
            var pinServiceMock = this.MockPinService(maskPins.Split(',').ToList(), fullPinList, true);

            // Add the timings to the pinServiceMock.
            var tcAttributes = new List<string> { "Drive" };
            var tcAttributeValuesCurrent = new Dictionary<string, string> { { "Drive", "0" } };
            var tcAttributeValuesStart = new Dictionary<string, string> { { "Drive", "0" } };
            var tcAttributeValuesResult = new Dictionary<string, string> { { "Drive", "1.5E-09" } };
            foreach (var pin in searchPins)
            {
                pinServiceMock.Setup(o => o.GetPinAttributeValues(pin, tcAttributes)).Returns(tcAttributeValuesCurrent);
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesStart));
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesResult));
            }

            // Mock the config file.
            var fileServiceMock = this.MockFileService("ConfigSampleValid.xml", true);

            // Mock the patconfig stuff.
            var patCfgMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patCfgMock.Setup(o => o.SetData("MOV 41, R1"));
            var patCfgServiceMock = this.MockPatConfigService("DfxTuneLooopSize", "FakePList", patConfigMock: patCfgMock.Object);

            // Mock the UserVarService so the needed uservars exist.
            var userVars = new List<string> { "TimingCollection1.DPIN_9_003_drv_offset", "TimingCollection1.DPIN_9_005_drv_offset", "TimingCollection1.DPIN_9_007_drv_offset" };
            var userVarServiceMock = this.MockUserVarsExistOnly(userVars, true);
            foreach (var userVar in userVars)
            {
                userVarServiceMock.Setup(o => o.GetDoubleValue(userVar)).Returns(0);
                userVarServiceMock.Setup(o => o.SetValue(userVar, "1.500ns"));
                userVarServiceMock.Setup(o => o.SetValue(userVar, "0.000ns"));
            }

            // Mock the testcondition service
            var tcMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcMock.Setup(o => o.Resolve());

            var tcServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcServiceMock.Setup(o => o.GetTestCondition("FakeTimings")).Returns(tcMock.Object);
            tcServiceMock.Setup(o => o.IsSmartTcEnabled()).Returns(true);
            tcServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.TIMING));
            Prime.Services.TestConditionService = tcServiceMock.Object;

            // mock the datalog/ituff.
            var ituffMocks1 = this.MockDatalogWrites(new Dictionary<string, string>()
            {
                { "::TimingCollection1.DPIN_9_003_drv_offset", "1.500ns" },
                { "::TimingCollection1.DPIN_9_005_drv_offset", "1.500ns" },
                { "::TimingCollection1.DPIN_9_007_drv_offset", "1.500ns" },
            });

            var underTest = new DfxTimingTuner
            {
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleDriveMode",
                SearchStart = "0.000ns",
                SearchResolution = "100ps",
                SearchEnd = "4.000ns",
                AdaptiveStart = "-1ns",
                AdaptiveResolution = "200ps",
                AdaptiveEnd = "1ns",
            };

            // Run the Verify/Execute methods
            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute()); // normal execution

            // check that all the mocks were triggered.
            funcMocks.ForEach(o => o.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            patCfgServiceMock.VerifyAll();
            patCfgMock.VerifyAll();
            tcServiceMock.VerifyAll();
            tcMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            ituffMocks1.ForEach(o => o.VerifyAll());

            // 2nd execution....
            ctvData["DPIN_9_000"] = string.Concat(System.Linq.Enumerable.Repeat("000", 11));
            ((Mock<ICaptureFailureAndCtvPerPinTest>)funcMocks[1]).Setup(o => o.GetCtvData()).Returns(ctvData);

            // Mock the SharedStorage for communicating with the software triggers.
            sharedStorageMock = this.MockSharedStorageForTriggers(searchPins, 2e-10, 11);
            sharedStorageMock.Setup(s => s.GetStringRowFromTable(TriggerCallbacks.FailStorageName, Context.IP)).Returns(string.Empty);

            // Add the timings to the pinServiceMock.
            tcAttributeValuesCurrent = new Dictionary<string, string> { { "Drive", "1.5E-09" } };
            tcAttributeValuesStart = new Dictionary<string, string> { { "Drive", "5E-10" } };
            tcAttributeValuesResult = new Dictionary<string, string> { { "Drive", "1.5E-09" } };
            foreach (var pin in searchPins)
            {
                pinServiceMock.Setup(o => o.GetPinAttributeValues(pin, tcAttributes)).Returns(tcAttributeValuesCurrent);
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesStart));
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesResult));
            }

            // Mock the patconfig stuff.
            patCfgMock.Setup(o => o.SetData("MOV 11, R1"));

            // Mock the UserVarService so the needed uservars exist.
            foreach (var userVar in userVars)
            {
                userVarServiceMock.Setup(o => o.GetDoubleValue(userVar)).Returns(1.5e-9);
                userVarServiceMock.Setup(o => o.SetValue(userVar, "1.500ns"));
            }

            // mock the datalog/ituff.
            var ituffMocks2 = this.MockDatalogWrites(new Dictionary<string, string>()
            {
                { "::TimingCollection1.DPIN_9_003_drv_offset", "1.500ns" },
                { "::TimingCollection1.DPIN_9_005_drv_offset", "1.500ns" },
                { "::TimingCollection1.DPIN_9_007_drv_offset", "1.500ns" },
            });

            Assert.AreEqual(1, underTest.Execute()); // adaptive execution

            // check that all the mocks were triggered.
            funcMocks.ForEach(o => o.VerifyAll());
            ituffMocks2.ForEach(o => o.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            patCfgServiceMock.VerifyAll();
            patCfgMock.VerifyAll();
            tcServiceMock.VerifyAll();
            tcMock.VerifyAll();
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_CompareModeNoUpdate_Pass()
        {
            var capturePins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var ctvData = new Dictionary<string, string>
            {
                { "DPIN_9_003", "00000000011111111111111111111111111111111" },
                { "DPIN_9_005", "11000000000000000000000000000000000000111" },
                { "DPIN_9_007", "11111111111111111111111111111000111111111" },
            };

            // setup the functional test service mock.
            var funcMocks = this.MockFunctionalService(ctvData.Keys.ToList(), ctvData, string.Empty, "IncrementCompareEdge");

            // Mock the SharedStorage for communicating with the software triggers.
            var searchPins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var sharedStorageMock = this.MockSharedStorageForTriggers(searchPins, 1e-10, 41);
            sharedStorageMock.Setup(s => s.GetStringRowFromTable(TriggerCallbacks.FailStorageName, Context.IP)).Returns(string.Empty);

            // Mock the PinService
            var pinServiceMock = this.MockPinService(new List<string>(), searchPins, true);

            // Add the timings to the pinServiceMock.
            var tcAttributes = new List<string> { "Compare" };
            var tcAttributeValuesCurrent = new Dictionary<string, string> { { "Compare", "2E-09" } };
            var tcAttributeValuesStart = new Dictionary<string, string> { { "Compare", "2E-09" } };
            var tcAttributeValuesResult = new Dictionary<string, Dictionary<string, string>>()
            {
                { "DPIN_9_003", new Dictionary<string, string> { { "Compare", "2.4E-09" } } },
                { "DPIN_9_005", new Dictionary<string, string> { { "Compare", "4E-09" } } },
                { "DPIN_9_007", new Dictionary<string, string> { { "Compare", "5E-09" } } },
            };

            foreach (var pin in searchPins)
            {
                pinServiceMock.Setup(o => o.GetPinAttributeValues(pin, tcAttributes)).Returns(tcAttributeValuesCurrent);
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesStart));
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesResult[pin]));
            }

            // Mock the config file.
            var fileServiceMock = this.MockFileService("ConfigSampleValid.xml", true);

            // Mock the patconfig stuff.
            var patCfgMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patCfgMock.Setup(o => o.SetData("MOV 41, _"));
            var patCfgServiceMock = this.MockPatConfigService("DfxTuneLooopSize", "FakePList", patConfigMock: patCfgMock.Object);

            // Mock the UserVarService so the needed uservars exist.
            var userVars = new List<string> { "TimingCollection1.DPIN_9_003_stb_offset", "TimingCollection1.DPIN_9_005_stb_offset", "TimingCollection1.DPIN_9_007_stb_offset" };
            var userVarServiceMock = this.MockUserVarsExistOnly(userVars, true);
            userVarServiceMock.Setup(o => o.SetValue("TimingCollection1.DPIN_9_003_stb_offset", "-1.600ns"));
            userVarServiceMock.Setup(o => o.SetValue("TimingCollection1.DPIN_9_005_stb_offset", "0.000ns"));
            userVarServiceMock.Setup(o => o.SetValue("TimingCollection1.DPIN_9_007_stb_offset", "1.000ns"));

            userVarServiceMock.Setup(o => o.SetValue("TimingCollection1.DPIN_9_003_stb_offset", "-2.000ns"));
            userVarServiceMock.Setup(o => o.SetValue("TimingCollection1.DPIN_9_005_stb_offset", "-2.000ns"));
            userVarServiceMock.Setup(o => o.SetValue("TimingCollection1.DPIN_9_007_stb_offset", "-2.000ns"));

            var tcMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcMock.Setup(o => o.Resolve());

            var tcServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcServiceMock.Setup(o => o.GetTestCondition("FakeTimings")).Returns(tcMock.Object);
            /* tcServiceMock.Setup(o => o.ResolveAllTestConditions()); */
            tcServiceMock.Setup(o => o.IsSmartTcEnabled()).Returns(true);
            tcServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.TIMING));
            Prime.Services.TestConditionService = tcServiceMock.Object;

            foreach (var userVar in userVars)
            {
                userVarServiceMock.Setup(o => o.GetDoubleValue(userVar)).Returns(-2e-9);
            }

            // mock the datalog/ituff.
            var ituffMocks = this.MockDatalogWrites(new Dictionary<string, string>()
            {
                { "::TimingCollection1.DPIN_9_003_stb_offset", "-1.600ns" },
                { "::TimingCollection1.DPIN_9_005_stb_offset", "0.000ns" },
                { "::TimingCollection1.DPIN_9_007_stb_offset", "1.000ns" },
            });

            var underTest = new DfxTimingTuner
            {
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
                UpdateTC = DfxTimingTuner.ResolveMode.None,
            };

            // Run the Verify/Execute methods
            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            // check that all the mocks were triggered.
            funcMocks.ForEach(o => o.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            patCfgServiceMock.VerifyAll();
            patCfgMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            ituffMocks.ForEach(o => o.VerifyAll());
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_DriveModeMultiBurstResolveAll_Pass()
        {
            // setup the functional test service mock.
            var capturePins = new List<string> { "DPIN_9_000" };
            var ctvData1 = new Dictionary<string, string>
            {
                { "DPIN_9_000", string.Concat(System.Linq.Enumerable.Repeat("111", 10)) + string.Concat(System.Linq.Enumerable.Repeat("000", 4)) },
            };
            var ctvData2 = new Dictionary<string, string>
            {
                { "DPIN_9_000", string.Concat(System.Linq.Enumerable.Repeat("000", 6)) + string.Concat(System.Linq.Enumerable.Repeat("111", 8)) },
            };
            var ctvData3 = new Dictionary<string, string>
            {
                { "DPIN_9_000", string.Concat(System.Linq.Enumerable.Repeat("111", 13)) },
            };
            var ctvTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            ctvTestMock.Setup(o => o.SetPinMask(new List<string> { "PinsToMask" }));
            ctvTestMock.Setup(o => o.ApplyLevelTestCondition());
            ctvTestMock.Setup(o => o.ApplyTimingTestCondition());
            ctvTestMock.Setup(o => o.Execute()).Returns(true);
            ctvTestMock.SetupSequence(o => o.GetCtvData())
                .Returns(ctvData1)
                .Returns(ctvData2)
                .Returns(ctvData3);
            ctvTestMock.Setup(o => o.SetSoftwareTriggerCallback("IncrementDriveEdge"));

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("FakePList", "FakeLevels", "FakeTimings", capturePins, 32000, It.IsAny<string>())).Returns(ctvTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // Mock the SharedStorage for communicating with the software triggers.
            var searchPins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var sharedStorageMock = this.MockSharedStorageForTriggers(searchPins, 3e-10, 14);
            sharedStorageMock.Setup(s => s.GetStringRowFromTable(TriggerCallbacks.FailStorageName, Context.IP)).Returns(string.Empty);

            // Mock the PinService
            var fullPinList = new List<string>();
            fullPinList.AddRange(capturePins);
            fullPinList.AddRange(searchPins);
            var pinServiceMock = this.MockPinService(new List<string> { "PinsToMask" }, fullPinList, true);

            // Add the timings to the pinServiceMock.
            var tcAttributes = new List<string> { "Drive" };
            var tcAttributeValuesCurrent = new Dictionary<string, string> { { "Drive", "2E-09" } };
            var tcAttributeValuesStart1 = new Dictionary<string, string> { { "Drive", "2E-09" } };
            var tcAttributeValuesStart2 = new Dictionary<string, string> { { "Drive", "6.2E-09" } };
            var tcAttributeValuesStart3 = new Dictionary<string, string> { { "Drive", "1.04E-08" } };
            var tcAttributeValuesResult = new Dictionary<string, string> { { "Drive", "6.5E-09" } };

            foreach (var pin in searchPins)
            {
                pinServiceMock.Setup(o => o.GetPinAttributeValues(pin, tcAttributes)).Returns(tcAttributeValuesCurrent);
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesStart1));
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesStart2));
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesStart3));
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesResult));
            }

            // Mock the config file.
            var fileServiceMock = this.MockFileService("ConfigSampleValid.xml", true);

            // Mock the patconfig stuff.
            var patCfgMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patCfgMock.Setup(o => o.SetData("MOV 14, R1"));
            patCfgMock.Setup(o => o.SetData("MOV 13, R1"));
            var patCfgServiceMock = this.MockPatConfigService("DfxTuneLooopSize", "FakePList", patConfigMock: patCfgMock.Object);

            // Mock the UserVarService so the needed uservars exist.
            var userVars = new List<string> { "TimingCollection1.DPIN_9_003_drv_offset", "TimingCollection1.DPIN_9_005_drv_offset", "TimingCollection1.DPIN_9_007_drv_offset" };
            var userVarServiceMock = this.MockUserVarsExistOnly(userVars, true);
            foreach (var userVar in userVars)
            {
                userVarServiceMock.Setup(o => o.GetDoubleValue(userVar)).Returns(-2e-9);
                userVarServiceMock.Setup(o => o.SetValue(userVar, "2.500ns"));
                userVarServiceMock.Setup(o => o.SetValue(userVar, "-2.000ns"));
            }

            // Mock the testcondition service
            var tcMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcMock.Setup(o => o.Resolve());

            var tcServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcServiceMock.Setup(o => o.GetTestCondition("FakeTimings")).Returns(tcMock.Object);
            tcServiceMock.Setup(o => o.ResolveAllTestConditions());
            tcServiceMock.Setup(o => o.IsSmartTcEnabled()).Returns(false);
            Prime.Services.TestConditionService = tcServiceMock.Object;

            // mock the datalog/ituff.
            var ituffMocks = this.MockDatalogWrites(new Dictionary<string, string>()
            {
                { "::TimingCollection1.DPIN_9_003_drv_offset", "2.500ns" },
                { "::TimingCollection1.DPIN_9_005_drv_offset", "2.500ns" },
                { "::TimingCollection1.DPIN_9_007_drv_offset", "2.500ns" },
            });

            var underTest = new DfxTimingTuner
            {
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleDriveMode",
                SearchStart = "-2.000ns",
                SearchResolution = "300ps",
                SearchEnd = "10.000ns",
                UpdateTC = DfxTimingTuner.ResolveMode.All,
            };

            // Run the Verify/Execute methods
            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            // check that all the mocks were triggered.
            ituffMocks.ForEach(o => o.VerifyAll());
            ctvTestMock.VerifyAll();
            funcServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            patCfgServiceMock.VerifyAll();
            patCfgMock.VerifyAll();
            patCfgMock.Verify(o => o.SetData("MOV 14, R1"), Times.Exactly(2));
            patCfgMock.Verify(o => o.SetData("MOV 13, R1"), Times.Exactly(1));
            tcServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_CompareMode_FailNoEye()
        {
            // setup the functional test service mock.
            var capturePins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var ctvData = new Dictionary<string, string>
            {
                { "DPIN_9_003", "00000000011111111111111111111111111111111" },
                { "DPIN_9_005", "11111111111111111111111111111111111111111" },
                { "DPIN_9_007", "11111111111111111111111111111000111111111" },
            };
            var ctvTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            ctvTestMock.Setup(o => o.ApplyLevelTestCondition());
            ctvTestMock.Setup(o => o.ApplyTimingTestCondition());
            ctvTestMock.Setup(o => o.Execute()).Returns(true);
            ctvTestMock.Setup(o => o.GetCtvData()).Returns(ctvData);
            ctvTestMock.Setup(o => o.SetSoftwareTriggerCallback("IncrementCompareEdge"));

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("FakePList", "FakeLevels", "FakeTimings", capturePins, 32000, It.IsAny<string>())).Returns(ctvTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // Mock the SharedStorage for communicating with the software triggers.
            var searchPins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var sharedStorageMock = this.MockSharedStorageForTriggers(searchPins, 1e-10, 41);
            sharedStorageMock.Setup(s => s.GetStringRowFromTable(TriggerCallbacks.FailStorageName, Context.IP)).Returns(string.Empty);

            // Mock the PinService
            var pinServiceMock = this.MockPinService(new List<string>(), searchPins, true);

            // Add the timings to the pinServiceMock.
            var tcAttributes = new List<string> { "Compare" };
            var tcAttributeValuesCurrent = new Dictionary<string, string> { { "Compare", "2E-09" } };
            var tcAttributeValuesStart = new Dictionary<string, string> { { "Compare", "2E-09" } };
            foreach (var pin in searchPins)
            {
                pinServiceMock.Setup(o => o.GetPinAttributeValues(pin, tcAttributes)).Returns(tcAttributeValuesCurrent);
                /* pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesStart)); */
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesCurrent));
            }

            // Mock the config file.
            var fileServiceMock = this.MockFileService("ConfigSampleValid.xml", true);

            // Mock the patconfig stuff.
            var patCfgMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patCfgMock.Setup(o => o.SetData("MOV 41, _"));
            var patCfgServiceMock = this.MockPatConfigService("DfxTuneLooopSize", "FakePList", patConfigMock: patCfgMock.Object);

            // mock the uservars.
            var userVars = new List<string> { "TimingCollection1.DPIN_9_003_stb_offset", "TimingCollection1.DPIN_9_005_stb_offset", "TimingCollection1.DPIN_9_007_stb_offset" };
            var userVarServiceMock = this.MockUserVarsExistOnly(userVars, true);
            foreach (var userVar in userVars)
            {
                userVarServiceMock.Setup(o => o.GetDoubleValue(userVar)).Returns(-2e-9);
                userVarServiceMock.Setup(o => o.SetValue(userVar, "-2.000ns"));
            }

            var tcMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcMock.Setup(o => o.Resolve());

            var tcServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcServiceMock.Setup(o => o.GetTestCondition("FakeTimings")).Returns(tcMock.Object);
            tcServiceMock.Setup(o => o.IsSmartTcEnabled()).Returns(true);
            tcServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.TIMING));
            Prime.Services.TestConditionService = tcServiceMock.Object;

            // mock the datalog/ituff.
            var ituffMocks = this.MockDatalogWrites(new Dictionary<string, string>()
            {
                { "::TimingCollection1.DPIN_9_003_stb_offset", "-1.600ns" },
                { "::TimingCollection1.DPIN_9_005_stb_offset", "-9999" },
                { "::TimingCollection1.DPIN_9_007_stb_offset", "1.000ns" },
            });

            var underTest = new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            };

            // Run the Verify/Execute methods
            underTest.Verify();
            Assert.AreEqual(0, underTest.Execute());

            // check that all the mocks were triggered.
            ctvTestMock.VerifyAll();
            funcServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            patCfgServiceMock.VerifyAll();
            patCfgMock.VerifyAll();
            ituffMocks.ForEach(o => o.VerifyAll());
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_CompareMode_FailTrigger()
        {
            // setup the functional test service mock.
            var capturePins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var ctvData = new Dictionary<string, string>
            {
                { "DPIN_9_003", "00000000011111111111111111111111111111111" },
                { "DPIN_9_005", "11111111111111111111111111111111111111111" },
                { "DPIN_9_007", "11111111111111111111111111111000111111111" },
            };
            var ctvTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            ctvTestMock.Setup(o => o.ApplyLevelTestCondition());
            ctvTestMock.Setup(o => o.ApplyTimingTestCondition());
            ctvTestMock.Setup(o => o.Execute()).Returns(true);
            ctvTestMock.Setup(o => o.SetSoftwareTriggerCallback("IncrementCompareEdge"));

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("FakePList", "FakeLevels", "FakeTimings", capturePins, 32000, It.IsAny<string>())).Returns(ctvTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // Mock the SharedStorage for communicating with the software triggers.
            var searchPins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var sharedStorageMock = this.MockSharedStorageForTriggers(searchPins, 1e-10, 41);
            sharedStorageMock.Setup(s => s.GetStringRowFromTable(TriggerCallbacks.FailStorageName, Context.IP)).Returns("SomeError from Trigger.");

            // Mock the PinService
            var pinServiceMock = this.MockPinService(new List<string>(), searchPins, true);

            // Add the timings to the pinServiceMock.
            var tcAttributes = new List<string> { "Compare" };
            var tcAttributeValuesCurrent = new Dictionary<string, string> { { "Compare", "4E-09" } };
            var tcAttributeValuesStart = new Dictionary<string, string> { { "Compare", "2E-09" } };
            foreach (var pin in searchPins)
            {
                pinServiceMock.Setup(o => o.GetPinAttributeValues(pin, tcAttributes)).Returns(tcAttributeValuesCurrent);
                /* pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesStart)); */
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesCurrent));
            }

            // Mock the config file.
            var fileServiceMock = this.MockFileService("ConfigSampleValid.xml", true);

            // Mock the patconfig stuff.
            var patCfgMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patCfgMock.Setup(o => o.SetData("MOV 41, _"));
            var patCfgServiceMock = this.MockPatConfigService("DfxTuneLooopSize", "FakePList", patConfigMock: patCfgMock.Object);

            // Mock the UserVarService so the needed uservars exist.
            // mock the uservars.
            var userVars = new List<string> { "TimingCollection1.DPIN_9_003_stb_offset", "TimingCollection1.DPIN_9_005_stb_offset", "TimingCollection1.DPIN_9_007_stb_offset" };
            var userVarServiceMock = this.MockUserVarsExistOnly(userVars, true);
            foreach (var userVar in userVars)
            {
                userVarServiceMock.Setup(o => o.GetDoubleValue(userVar)).Returns(-2e-9);
                userVarServiceMock.Setup(o => o.SetValue(userVar, "-2.000ns"));
            }

            var tcMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcMock.Setup(o => o.Resolve());

            var tcServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcServiceMock.Setup(o => o.GetTestCondition("FakeTimings")).Returns(tcMock.Object);
            tcServiceMock.Setup(o => o.IsSmartTcEnabled()).Returns(true);
            tcServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.TIMING));
            Prime.Services.TestConditionService = tcServiceMock.Object;

            // mock the datalog.
            var ituffMocks = this.MockDatalogWrites(new Dictionary<string, string> { { "::ERROR", "Error_during_trigger_callback._Messge=SomeError_from_Trigger." } }, wrapChar: '_');

            var underTest = new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            };

            // Run the Verify/Execute methods
            underTest.Verify();
            Assert.AreEqual(0, underTest.Execute());

            // check that all the mocks were triggered.
            ituffMocks.ForEach(o => o.VerifyAll());
            ctvTestMock.VerifyAll();
            funcServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            patCfgServiceMock.VerifyAll();
            patCfgMock.VerifyAll();
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_CompareMode_FailExceptionOnExecute()
        {
            // setup the functional test service mock.
            var capturePins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var ctvData = new Dictionary<string, string>
            {
                { "DPIN_9_003", "00000000011111111111111111111111111111111" },
                { "DPIN_9_005", "11111111111111111111111111111111111111111" },
                { "DPIN_9_007", "11111111111111111111111111111000111111111" },
            };
            var ctvTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            ctvTestMock.Setup(o => o.ApplyLevelTestCondition());
            ctvTestMock.Setup(o => o.ApplyTimingTestCondition());
            ctvTestMock.Setup(o => o.Execute()).Throws(new Prime.Base.Exceptions.FatalException("Error executing plist.\nSome Random Error"));
            ctvTestMock.Setup(o => o.SetSoftwareTriggerCallback("IncrementCompareEdge"));

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("FakePList", "FakeLevels", "FakeTimings", capturePins, 32000, It.IsAny<string>())).Returns(ctvTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // Mock the SharedStorage for communicating with the software triggers.
            var searchPins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var sharedStorageMock = this.MockSharedStorageForTriggers(searchPins, 1e-10, 0);

            // Mock the PinService
            var pinServiceMock = this.MockPinService(new List<string>(), searchPins, true);

            // Add the timings to the pinServiceMock.
            var tcAttributes = new List<string> { "Compare" };
            var tcAttributeValuesCurrent = new Dictionary<string, string> { { "Compare", "4E-09" } };
            var tcAttributeValuesStart = new Dictionary<string, string> { { "Compare", "2E-09" } };
            foreach (var pin in searchPins)
            {
                pinServiceMock.Setup(o => o.GetPinAttributeValues(pin, tcAttributes)).Returns(tcAttributeValuesCurrent);
                /* pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesStart)); */
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesCurrent));
            }

            // mock the uservars.
            var userVars = new List<string> { "TimingCollection1.DPIN_9_003_stb_offset", "TimingCollection1.DPIN_9_005_stb_offset", "TimingCollection1.DPIN_9_007_stb_offset" };
            var userVarServiceMock = this.MockUserVarsExistOnly(userVars, true);
            foreach (var userVar in userVars)
            {
                userVarServiceMock.Setup(o => o.GetDoubleValue(userVar)).Returns(-2e-9);
                userVarServiceMock.Setup(o => o.SetValue(userVar, "-2.000ns"));
            }

            var tcMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcMock.Setup(o => o.Resolve());

            var tcServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcServiceMock.Setup(o => o.GetTestCondition("FakeTimings")).Returns(tcMock.Object);
            tcServiceMock.Setup(o => o.IsSmartTcEnabled()).Returns(true);
            tcServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.TIMING));
            Prime.Services.TestConditionService = tcServiceMock.Object;

            // Mock the config file.
            var fileServiceMock = this.MockFileService("ConfigSampleValid.xml", true);

            // Mock the patconfig stuff.
            var patCfgMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patCfgMock.Setup(o => o.SetData("MOV 41, _"));
            var patCfgServiceMock = this.MockPatConfigService("DfxTuneLooopSize", "FakePList", patConfigMock: patCfgMock.Object);

            // mock the datalog.
            var ituffMocks = this.MockDatalogWrites(new Dictionary<string, string> { { "::ERROR", "Error_executing_plist._Some_Random_Error" } }, wrapChar: '_');

            var underTest = new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            };

            // Run the Verify/Execute methods
            underTest.Verify();
            Assert.AreEqual(0, underTest.Execute());

            // check that all the mocks were triggered.
            ituffMocks.ForEach(o => o.VerifyAll());
            ctvTestMock.VerifyAll();
            funcServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            patCfgServiceMock.VerifyAll();
            patCfgMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_CompareMode_FailExceptionOnApplyTestCondition()
        {
            // setup the functional test service mock.
            var capturePins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var ctvTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            ctvTestMock.Setup(o => o.ApplyLevelTestCondition()).Throws(new Prime.Base.Exceptions.FatalException("Error applying testcondition."));
            ctvTestMock.Setup(o => o.SetSoftwareTriggerCallback("IncrementCompareEdge"));

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("FakePList", "FakeLevels", "FakeTimings", capturePins, 32000, It.IsAny<string>())).Returns(ctvTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // Mock the PinService
            var searchPins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var pinServiceMock = this.MockPinService(new List<string>(), searchPins, true);

            // Mock the config file.
            var fileServiceMock = this.MockFileService("ConfigSampleValid.xml", true);

            // mock the datalog.
            var ituffMocks = this.MockDatalogWrites(new Dictionary<string, string> { { "::ERROR", "Error_applying_testcondition." } }, wrapChar: '_');

            // mock the uservars.
            var userVars = new List<string> { "TimingCollection1.DPIN_9_003_stb_offset", "TimingCollection1.DPIN_9_005_stb_offset", "TimingCollection1.DPIN_9_007_stb_offset" };
            var userVarServiceMock = this.MockUserVarsExistOnly(userVars, true);
            foreach (var userVar in userVars)
            {
                userVarServiceMock.Setup(o => o.GetDoubleValue(userVar)).Returns(-2e-9);
                userVarServiceMock.Setup(o => o.SetValue(userVar, "-2.000ns"));
            }

            // Mock the patconfig stuff.
            var patCfgServiceMock = this.MockPatConfigService("DfxTuneLooopSize", "FakePList");

            // Mock the testcondition stuff
            var tcMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcMock.Setup(o => o.Resolve());

            var tcServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcServiceMock.Setup(o => o.GetTestCondition("FakeTimings")).Returns(tcMock.Object);
            tcServiceMock.Setup(o => o.IsSmartTcEnabled()).Returns(true);
            tcServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.TIMING));
            Prime.Services.TestConditionService = tcServiceMock.Object;

            var underTest = new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            };

            // Run the Verify/Execute methods
            underTest.Verify();
            Assert.AreEqual(0, underTest.Execute());

            // check that all the mocks were triggered.
            ituffMocks.ForEach(o => o.VerifyAll());
            ctvTestMock.VerifyAll();
            funcServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_NoCTVCaptured_Fail()
        {
            var capturePins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var ctvData = new Dictionary<string, string>();

            // setup the functional test service mock.
            var funcMocks = this.MockFunctionalService(capturePins, ctvData, string.Empty, "IncrementCompareEdge");

            // Mock the SharedStorage for communicating with the software triggers.
            var searchPins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var sharedStorageMock = this.MockSharedStorageForTriggers(searchPins, 1e-10, 41);
            sharedStorageMock.Setup(s => s.GetStringRowFromTable(TriggerCallbacks.FailStorageName, Context.IP)).Returns(string.Empty);

            // Mock the PinService
            var pinServiceMock = this.MockPinService(new List<string>(), searchPins, true);

            // Add the timings to the pinServiceMock.
            var tcAttributes = new List<string> { "Compare" };
            var tcAttributeValuesCurrent = new Dictionary<string, string> { { "Compare", "2E-09" } };
            var tcAttributeValuesStart = new Dictionary<string, string> { { "Compare", "2E-09" } };
            var tcAttributeValuesResult = new Dictionary<string, Dictionary<string, string>>()
            {
                { "DPIN_9_003", new Dictionary<string, string> { { "Compare", "2.4E-09" } } },
                { "DPIN_9_005", new Dictionary<string, string> { { "Compare", "4E-09" } } },
                { "DPIN_9_007", new Dictionary<string, string> { { "Compare", "5E-09" } } },
            };

            foreach (var pin in searchPins)
            {
                pinServiceMock.Setup(o => o.GetPinAttributeValues(pin, tcAttributes)).Returns(tcAttributeValuesCurrent);
                pinServiceMock.Setup(o => o.SetPinAttributeValues(pin, tcAttributeValuesStart));
            }

            // Mock the config file.
            var fileServiceMock = this.MockFileService("ConfigSampleValid.xml", true);

            // Mock the patconfig stuff.
            var patCfgMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patCfgMock.Setup(o => o.SetData("MOV 41, _"));
            var patCfgServiceMock = this.MockPatConfigService("DfxTuneLooopSize", "FakePList", patConfigMock: patCfgMock.Object);

            // Mock the UserVarService so the needed uservars exist.
            var userVars = new List<string> { "TimingCollection1.DPIN_9_003_stb_offset", "TimingCollection1.DPIN_9_005_stb_offset", "TimingCollection1.DPIN_9_007_stb_offset" };
            var userVarServiceMock = this.MockUserVarsExistOnly(userVars, true);
            foreach (var userVar in userVars)
            {
                userVarServiceMock.Setup(o => o.GetDoubleValue(userVar)).Returns(-2e-9);
                userVarServiceMock.Setup(o => o.SetValue(userVar, "-2.000ns"));
            }

            var tcMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcMock.Setup(o => o.Resolve());

            var tcServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcServiceMock.Setup(o => o.GetTestCondition("FakeTimings")).Returns(tcMock.Object);
            tcServiceMock.Setup(o => o.ResolveAllTestConditions());
            tcServiceMock.Setup(o => o.IsSmartTcEnabled()).Returns(true);
            tcServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.TIMING));
            Prime.Services.TestConditionService = tcServiceMock.Object;

            // mock the datalog.
            var ituffMocks = this.MockDatalogWrites(new Dictionary<string, string> { { "::ERROR", "ERROR:_No_CTV_data_captured,_the_plist_did_not_run_correctly." } }, wrapChar: '_');

            var underTest = new DfxTimingTuner
            {
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                ConfigFile = "ConfigSampleValid.xml",
                ConfigSet = "SampleCompareMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
                UpdateTC = DfxTimingTuner.ResolveMode.None,
            };

            // Run the Verify/Execute methods
            underTest.Verify();
            Assert.AreEqual(0, underTest.Execute());

            // check that all the mocks were triggered.
            funcMocks.ForEach(o => o.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            patCfgServiceMock.VerifyAll();
            patCfgMock.VerifyAll();
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Verify the correct exceptions are thrown with bad config files.
        /// </summary>
        [TestMethod]
        public void Verify_ConfigFile_Exceptions()
        {
            this.SetupMocksForVerify(mode: "DRIVE");

            // define a default object to be used in these tests and default mocks.
            var underTest = new DfxTimingTuner
            {
                Patlist = "FakePList",
                LevelsTc = "FakeLevels",
                TimingsTc = "FakeTimings",
                MaskPins = "PinsToMask",
                ConfigFile = "ConfigSampleInValidXml.xml",
                ConfigSet = "SampleDriveMode",
                SearchStart = "-2.000ns",
                SearchResolution = "100ps",
                SearchEnd = "2.000ns",
            };

            // Config file does not exist.
            var fileServiceMock = this.MockFileService("ConfigSampleInValidXml.xml", false);
            underTest.ConfigFile = "ConfigSampleInValidXml.xml";
            Assert.ThrowsException<FileNotFoundException>(() => underTest.Verify());

            // Config file has invalid xml format.
            fileServiceMock = this.MockFileService("ConfigSampleInValidXml.xml", true);
            underTest.ConfigFile = "ConfigSampleInValidXml.xml";
            Assert.ThrowsException<InvalidOperationException>(() => underTest.Verify());

            // Config file 'search_pins' field contains an undefined pingroup.
            this.VerifyConfigFileFailures(
                underTest,
                "ConfigSampleMissingSearchPinGroup.xml",
                "SampleDriveMode",
                "No PinGroup=[InvalidSearchPinGroup] found in ConfigurationFile=[ConfigSampleMissingSearchPinGroup.xml]. Referenced in ConfigSet=[SampleDriveMode].");

            // Config file 'capture_pins' field contains an undefined pingroup.
            this.VerifyConfigFileFailures(
                underTest,
                "ConfigSampleMissingCapturePinGroup.xml",
                "SampleDriveMode",
                "No PinGroup=[InvalidCapturePinGroup] found in ConfigurationFile=[ConfigSampleMissingCapturePinGroup.xml]. Referenced in ConfigSet=[SampleDriveMode].");

            // Config file 'capture_ctvorder' field contains an undefined pingroup.
            this.VerifyConfigFileFailures(
                underTest,
                "ConfigSampleMissingCtvOrderPinGroup.xml",
                "SampleDriveMode",
                "No PinGroup=[InvalidCtvPinGroup] found in ConfigurationFile=[ConfigSampleMissingCtvOrderPinGroup.xml]. Referenced in ConfigSet=[SampleDriveMode].");

            // Config file 'search_pins' field is missing from xml.
            this.VerifyConfigFileFailures(
                underTest,
                "ConfigSampleInValidMissingSearchPinsField.xml",
                "SampleDriveMode",
                "No 'search_pins' field found in ConfigSet=[SampleDriveMode], ConfigurationFile=[ConfigSampleInValidMissingSearchPinsField.xml].");

            // Config file is valid, but parameter ConfigSet does not exist in the config file.
            this.VerifyConfigFileFailures(
                underTest,
                "ConfigSampleValid.xml",
                "InvalidSet",
                "No ConfigSet=[InvalidSet] found in ConfigurationFile=[ConfigSampleValid.xml].");

            // Config file 'uservar' field is missing from xml.
            this.SetupMocksForVerify(mode: "COMPARE");
            this.VerifyConfigFileFailures(
                underTest,
                "ConfigSampleInvalidMissingUserVarField.xml",
                "SampleCompareMode",
                "No 'uservar' field found in ConfigSet=[SampleCompareMode], ConfigurationFile=[ConfigSampleInvalidMissingUserVarField.xml].");

            // Config file 'loop_size' field is missing from xml.
            this.VerifyConfigFileFailures(
                underTest,
                "ConfigSampleInvalidLoopField.xml",
                "MissingField",
                "No 'loop_size' field found in ConfigSet=[MissingField], ConfigurationFile=[ConfigSampleInvalidLoopField.xml].");

            // Config file 'loop_size' field is missing the config attribute.
            this.VerifyConfigFileFailures(
                underTest,
                "ConfigSampleInvalidLoopField.xml",
                "MissingPatConfig",
                "No 'config' attribute found in loop_size for ConfigSet=[MissingPatConfig], ConfigurationFile=[ConfigSampleInvalidLoopField.xml].");

            // Config file is valid but loop_size field contains a patconfig which doesn't exist.
            this.PatConfigServiceMock.Setup(o => o.GetPatConfigHandleWithPlist("InvalidPatConfig", "FakePList")).Throws(new Prime.Base.Exceptions.FatalException("ERROR"));
            this.VerifyConfigFileFailures(
                underTest,
                "ConfigSampleInvalidLoopField.xml",
                "BadPatConfig",
                "No PatConfig=[InvalidPatConfig] found in ALEPH. referenced from 'loop_size' in ConfigSet=[BadPatConfig], ConfigurationFile=[ConfigSampleInvalidLoopField.xml].");

            // Config file is valid but loop_size field contains data in wrong format.
            this.VerifyConfigFileFailures(
                underTest,
                "ConfigSampleInvalidLoopField.xml",
                "BadFormat",
                "Invalid 'loop_size' Data=[MOV 50, R1]. Expecting 'MOV %SIZE%, R\\d+' referenced in ConfigSet=[BadFormat], ConfigurationFile=[ConfigSampleInvalidLoopField.xml].");

            // Config file is valid, but the uservar field evaluates to UserVars which don't exist.
            this.UserVarServiceMock.Setup(o => o.Exists("TimingCollection1.DPIN_9_003_stb_offset")).Returns(false);
            this.VerifyConfigFileFailures(
                underTest,
                "ConfigSampleValid.xml",
                "SampleCompareMode",
                "No UserVar=[TimingCollection1.DPIN_9_003_stb_offset] Found. From Pin=[DPIN_9_003] uservar=[TimingCollection1.%PIN%_stb_offset] in ConfigSet=[SampleCompareMode], ConfigurationFile=[ConfigSampleValid.xml].");

            // Config file is valid, but the one of the pins doesn't exist.
            this.UserVarServiceMock.Setup(o => o.Exists("TimingCollection1.DPIN_9_003_stb_offset")).Returns(true);
            this.PinServiceMock.Setup(o => o.Exists("DPIN_9_005")).Returns(false);
            this.VerifyConfigFileFailures(
                underTest,
                "ConfigSampleValid.xml",
                "SampleCompareMode",
                "Pin=[DPIN_9_005] does not exist. Referenced from 'search_pins' field, PinGroup=[MCI_Pins], ConfigSet=[SampleCompareMode], ConfigurationFile=[ConfigSampleValid.xml].");

            // Config file is valid, but the one of the pins is a group.
            this.PinServiceMock.Setup(o => o.Exists("DPIN_9_005")).Returns(true);
            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(o => o.IsGroup()).Returns(true);
            this.PinServiceMock.Setup(o => o.Get("DPIN_9_005")).Returns(pinMock.Object);
            this.VerifyConfigFileFailures(
                underTest,
                "ConfigSampleValid.xml",
                "SampleCompareMode",
                "Pin=[DPIN_9_005] is a PinGroup, it must be an individual pin. Referenced from 'search_pins' field, PinGroup=[MCI_Pins], ConfigSet=[SampleCompareMode], ConfigurationFile=[ConfigSampleValid.xml].");
        }

        private static string GetPathToFiles([CallerFilePath] string srcPath = "")
        {
            return Path.GetDirectoryName(srcPath) + "\\InputFiles\\";
        }

        private List<Mock> MockFunctionalService(List<string> ctvPins, Dictionary<string, string> ctvData, string maskPins, string triggerCallback)
        {
            var ctvTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            ctvTestMock.Setup(o => o.ApplyLevelTestCondition());
            ctvTestMock.Setup(o => o.ApplyTimingTestCondition());
            ctvTestMock.Setup(o => o.Execute()).Returns(true);
            ctvTestMock.Setup(o => o.GetCtvData()).Returns(ctvData);
            ctvTestMock.Setup(o => o.SetSoftwareTriggerCallback(triggerCallback));
            if (!string.IsNullOrWhiteSpace(maskPins))
            {
                var maskPinsList = maskPins.Split(',').ToList();
                ctvTestMock.Setup(o => o.SetPinMask(maskPinsList));
            }

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("FakePList", "FakeLevels", "FakeTimings", ctvPins, 32000, It.IsAny<string>())).Returns(ctvTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            return new List<Mock> { funcServiceMock, ctvTestMock };
        }

        private Mock<IPatConfigService> MockPatConfigService(string patConfigName, string plist, IPatConfigHandle patConfigMock = null)
        {
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);

            if (patConfigMock == null)
            {
                patConfigMock = new Mock<IPatConfigHandle>(MockBehavior.Loose).Object;
            }
            else
            {
                patConfigServiceMock.Setup(o => o.Apply(patConfigMock));
            }

            patConfigServiceMock.Setup(o => o.GetPatConfigHandleWithPlist(patConfigName, plist)).Returns(patConfigMock);

            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            return patConfigServiceMock;
        }

        private Mock<IPinService> MockPinService(List<string> pinsExistOnly, List<string> configPins, bool exists)
        {
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            foreach (var pin in configPins)
            {
                pinServiceMock.Setup(o => o.Exists(pin)).Returns(exists);
                pinServiceMock.Setup(o => o.Get(pin)).Returns(pinMock.Object);
                pinMock.Setup(o => o.IsGroup()).Returns(false);
            }

            foreach (var pin in pinsExistOnly)
            {
                pinServiceMock.Setup(o => o.Exists(pin)).Returns(exists);
            }

            Prime.Services.PinService = pinServiceMock.Object;
            return pinServiceMock;
        }

        private Mock<IFileService> MockFileService(string file, bool exists)
        {
            // Mock for getting files.
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.GetFile(file)).Returns(GetPathToFiles() + file);
            fileServiceMock.Setup(o => o.FileExists(file)).Returns(exists);
            /* fileServiceMock.Setup(o => o.GetFile(It.IsAny<string>())).Returns((string f) => GetPathToFiles() + f);
            fileServiceMock.Setup(o => o.FileExists(It.IsAny<string>())).Returns(true); */
            Prime.Services.FileService = fileServiceMock.Object;

            return fileServiceMock;
        }

        private Mock<IUserVarService> MockUserVarsExistOnly(List<string> userVars, bool exists)
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            foreach (var uservar in userVars)
            {
                userVarServiceMock.Setup(u => u.Exists(uservar)).Returns(exists);
            }

            Prime.Services.UserVarService = userVarServiceMock.Object;
            return userVarServiceMock;
        }

        private Mock<ISharedStorageService> MockSharedStorageForTriggers(List<string> pins, double stepSize, int triggers)
        {
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(s => s.InsertRowAtTable(TriggerCallbacks.IncrementStorageName, stepSize.ToString(), Context.IP));
            sharedStorageMock.Setup(s => s.InsertRowAtTable(TriggerCallbacks.PinListStorageName, pins, Context.IP));
            sharedStorageMock.Setup(s => s.InsertRowAtTable(TriggerCallbacks.CallbackCountStorageName, 0, Context.IP));
            sharedStorageMock.Setup(s => s.InsertRowAtTable(TriggerCallbacks.FailStorageName, string.Empty, Context.IP));

            if (triggers > 0)
            {
                sharedStorageMock.Setup(s => s.KeyExistsInIntegerTable(TriggerCallbacks.CallbackCountStorageName, Context.IP)).Returns(true);
                sharedStorageMock.Setup(s => s.GetIntegerRowFromTable(TriggerCallbacks.CallbackCountStorageName, Context.IP)).Returns(triggers);
            }

            Prime.Services.SharedStorageService = sharedStorageMock.Object;
            return sharedStorageMock;
        }

        private List<Mock> MockDatalogWrites(Dictionary<string, string> namesAndValues, char wrapChar = '\0')
        {
            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            foreach (var item in namesAndValues)
            {
                writerMock.Setup(o => o.SetTnamePostfix(item.Key));
                writerMock.Setup(o => o.SetData(item.Value));
                if (wrapChar != '\0')
                {
                    writerMock.Setup(o => o.SetDelimiterCharacterForWrap(wrapChar));
                }
            }

            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            datalogMock.Setup(o => o.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogMock.Object;

            return new List<Mock> { datalogMock, writerMock };
        }

        private void BuildAndVerifyTest(bool expect, DfxTimingTuner test)
        {
            if (expect)
            {
                test.Verify();
            }
            else
            {
                Assert.ThrowsException<ArgumentException>(test.Verify);
            }
        }

        private void VerifyConfigFileFailures(DfxTimingTuner underTest, string configFile, string configSet, string errorMsg)
        {
            var fileServiceMock = this.MockFileService(configFile, true);
            underTest.ConfigFile = configFile;
            underTest.ConfigSet = configSet;
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.Verify());
            Assert.AreEqual(errorMsg, ex.Message);
            fileServiceMock.VerifyAll();
        }

        private void SetupMocksForVerify(string file = "ConfigSampleValid.xml", bool usePinAlias = false, string mode = "COMPARE", string adjustPinGroup = "")
        {
            // setup the functional test service mock.
            var capturePins = (mode == "DRIVE") ? new List<string> { "DPIN_9_000" } : new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            this.CtvTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);

            this.FunctionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            this.FunctionalServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("FakePList", "FakeLevels", "FakeTimings", capturePins, 32000, It.IsAny<string>())).Returns(this.CtvTestMock.Object);
            Prime.Services.FunctionalService = this.FunctionalServiceMock.Object;

            // Mock the PinService
            var searchPins = new List<string> { "DPIN_9_003", "DPIN_9_005", "DPIN_9_007" };
            var pinsExistOnly = new List<string> { "PinsToMask" };
            var fullPinList = new List<string>();
            fullPinList.AddRange(capturePins);
            fullPinList.AddRange(searchPins);
            if (!string.IsNullOrEmpty(adjustPinGroup))
            {
                pinsExistOnly.AddRange(adjustPinGroup.Split(','));
            }

            this.PinServiceMock = this.MockPinService(pinsExistOnly, fullPinList, true);

            // Mock the config file.
            this.FileServiceMock = this.MockFileService(file, true);

            // Mock the patconfig stuff.
            this.PatConfigServiceMock = this.MockPatConfigService("DfxTuneLooopSize", "FakePList");

            // mock the user vars
            var userVarMode = (mode == "DRIVE") ? "drv" : "stb";
            var userVars = new List<string> { $"TimingCollection1.DPIN_9_003_{userVarMode}_offset", $"TimingCollection1.DPIN_9_005_{userVarMode}_offset", $"TimingCollection1.DPIN_9_007_{userVarMode}_offset" };
            if (usePinAlias)
            {
                userVars = new List<string> { $"TimingCollection1.MCI0_{userVarMode}_offset", $"TimingCollection1.MCI1_{userVarMode}_offset", $"TimingCollection1.MCI2_{userVarMode}_offset" };
            }

            this.UserVarServiceMock = this.MockUserVarsExistOnly(userVars, true);
        }
    }
}
