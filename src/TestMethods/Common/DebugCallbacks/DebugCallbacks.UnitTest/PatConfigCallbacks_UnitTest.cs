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

namespace DebugCallbacks.UnitTest
{
    using System;
    using System.Collections.Generic;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.PatConfigService;
    using Prime.PerformanceService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="PatConfigCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class PatConfigCallbacks_UnitTest
    {
        private Mock<ITestProgramService> testProgramMock;
        private Mock<IPatternModification> patternModifications;

        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => { Console.WriteLine(s); });
            consoleMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string n, string src) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleMock.Object;

            this.testProgramMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            this.testProgramMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string> { { "Patlist", "SomePatlist" } });
            Prime.Services.TestProgramService = this.testProgramMock.Object;

            this.patternModifications = new Mock<IPatternModification>(MockBehavior.Strict);
            DDG.PatternModifications.Service = this.patternModifications.Object;

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ExecutePatConfig_Pass()
        {
            var handle = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var handles = new List<IPatConfigHandle> { handle.Object };
            this.patternModifications.Setup(o => o.GetPatternConfigHandles("Configuration:0x7F", "SomePatlist")).Returns(handles);
            this.patternModifications.Setup(o => o.ApplyPatternConfigHandles(handles));
            PatConfig.ExecutePatConfig("Configuration:0x7F");
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ExecutePatConfig_Exception_Fail()
        {
            var handle = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var handles = new List<IPatConfigHandle> { handle.Object };
            this.patternModifications.Setup(o => o.GetPatternConfigHandles("Configuration:0x7F", "SomePatlist")).Returns(handles);
            this.patternModifications.Setup(o => o.ApplyPatternConfigHandles(handles)).Throws<Exception>();
            PatConfig.ExecutePatConfig("Configuration:0x7F");
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ExecutePatConfigSetPoint_Local_Pass()
        {
            var handle = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            var patConfigService = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigService.Setup(o => o.GetSetPointHandle("Module", "Group", "SomePatlist")).Returns(handle.Object);
            Prime.Services.PatConfigService = patConfigService.Object;

            handle.Setup(o => o.ApplySetPoint("EnableAllCores"));
            PatConfig.ExecutePatConfigSetPoint("Module:Group:EnableAllCores");

            patConfigService.VerifyAll();
            handle.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ExecutePatConfigSetPoint_MixedModel_Pass()
        {
            var handle = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            var patConfigService = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigService.Setup(o => o.GetSetPointHandle("Module", "EnableCores", "SomePatlist")).Returns(handle.Object);
            patConfigService.Setup(o => o.GetSetPointHandle("Module", "Subr")).Returns(handle.Object);
            Prime.Services.PatConfigService = patConfigService.Object;

            handle.Setup(o => o.ApplySetPoint("All"));
            handle.Setup(o => o.ApplySetPoint("SetRatio"));
            PatConfig.ExecutePatConfigSetPoint("Module:EnableCores:All,Module:Subr:SetRatio:Global");

            patConfigService.VerifyAll();
            handle.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ExecutePatConfigSetPoint_Global_Pass()
        {
            var handle = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            var patConfigService = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigService.Setup(o => o.GetSetPointHandle("Module", "Group")).Returns(handle.Object);
            Prime.Services.PatConfigService = patConfigService.Object;

            handle.Setup(o => o.ApplySetPoint("EnableAllCores"));
            PatConfig.ExecutePatConfigSetPoint("Module:Group:EnableAllCores:Global");

            patConfigService.VerifyAll();
            handle.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExecutePatConfigSetPoint_InvalidInput_Fail()
        {
            var handle = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            var patConfigService = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigService.Setup(o => o.GetSetPointHandle("Module", "Group", "SomePatlist")).Returns(handle.Object);
            Prime.Services.PatConfigService = patConfigService.Object;

            handle.Setup(o => o.ApplySetPoint("EnableAllCores"));
            PatConfig.ExecutePatConfigSetPoint("Module:Group");
        }

        /// <summary>
        /// Test fail case of BitVectorPatConfigSetPoint.
        /// </summary>
        [TestMethod]
        public void BitVectorPatConfigSetPoint_InvalidArgs_Fail()
        {
            // Setup the mocks
            var allMocks = this.SetupMocksForBitVectorPatConfigSetPoint();

            var noArgs = Assert.ThrowsException<ArgumentException>(() => PatConfig.BitVectorPatConfigSetPoint(string.Empty));
            Assert.AreEqual("DummyTestInstance.BitVectorPatConfigSetPoint: failed parsing arguments. CommandLine.MissingRequiredOptionError\nCommandLine.MissingRequiredOptionError", noArgs.Message);

            var badArgs = Assert.ThrowsException<ArgumentException>(() => PatConfig.BitVectorPatConfigSetPoint("--WrongArgs=1"));
            Assert.AreEqual("DummyTestInstance.BitVectorPatConfigSetPoint: failed parsing arguments. CommandLine.UnknownOptionError\nCommandLine.MissingRequiredOptionError\nCommandLine.MissingRequiredOptionError", badArgs.Message);

            var invalidSetPointFormat1 = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => PatConfig.BitVectorPatConfigSetPoint("--bitvector=1 --setpoint=A:B:C:D:E"));
            Assert.AreEqual("Error parsing PatConfigSetpoint=[A:B:C:D:E]. Expecting Module:Group:Setpoint or Module:Group format.", invalidSetPointFormat1.Message);

            var invalidSetPointFormat2 = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => PatConfig.BitVectorPatConfigSetPoint("--bitvector=1 --setpoint=A"));
            Assert.AreEqual("Error parsing PatConfigSetpoint=[A]. Expecting Module:Group:Setpoint or Module:Group format.", invalidSetPointFormat2.Message);

            var invalidValueMap1 = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => PatConfig.BitVectorPatConfigSetPoint("--bitvector=1 --setpoint=A:B --value_map blah"));
            Assert.AreEqual("Error parsing ValueMap=[blah]. Expecting a comma-separated list of value:replacement tokens.", invalidValueMap1.Message);

            var invalidValueMap2 = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => PatConfig.BitVectorPatConfigSetPoint("--bitvector=1 --setpoint=A:B --value_map blah:blah:blah"));
            Assert.AreEqual("Error parsing ValueMap=[blah:blah:blah]. Expecting a comma-separated list of value:replacement tokens.", invalidValueMap2.Message);

            // Verify all the mocks.
            allMocks.ForEach(it => it.VerifyAll());
        }

        /// <summary>
        /// Test fail case of BitVectorPatConfigSetPoint.
        /// </summary>
        [TestMethod]
        public void BitVectorPatConfigSetPoint_SetPointDoesNotExist_Fail()
        {
            // Setup the mocks
            var allMocks = this.SetupMocksForBitVectorPatConfigSetPoint();

            // Run the test with missing module/group
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(o => o.GetSetPointHandle("FUNC", "Group")).Throws(new Prime.Base.Exceptions.FatalException("Module or Group does not exist."));
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var badModuleOrGroup = Assert.ThrowsException<Prime.Base.Exceptions.FatalException>(() => PatConfig.BitVectorPatConfigSetPoint("--bitvector 1011 --setpoint FUNC:Group:SetPoint%INDEX%_%VALUE%"));
            Assert.AreEqual("Module or Group does not exist.", badModuleOrGroup.Message);

            // Run the test with missing setpoint
            var setPointMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            setPointMock.Setup(o => o.ApplySetPoint("SetPoint0_1")).Throws(new Prime.Base.Exceptions.FatalException("SetPoint does not exist."));
            patConfigServiceMock.Setup(o => o.GetSetPointHandle("FUNC", "Group")).Returns(setPointMock.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var badSetPoint = Assert.ThrowsException<Prime.Base.Exceptions.FatalException>(() => PatConfig.BitVectorPatConfigSetPoint("--bitvector 1011 --setpoint FUNC:Group:SetPoint%INDEX%_%VALUE%"));
            Assert.AreEqual("SetPoint does not exist.", badSetPoint.Message);
        }

        /// <summary>
        /// Test pass case of BitVectorPatConfigSetPoint.
        /// </summary>
        [TestMethod]
        public void BitVectorPatConfigSetPoint_NoOptionalArgs_Pass()
        {
            // Setup the mocks
            List<string> setPointsToMock = new List<string> { "FUNC:Group:SetPoint0_1", "FUNC:Group:SetPoint1_0", "FUNC:Group:SetPoint2_1", "FUNC:Group:SetPoint3_1" };
            var allMocks = this.SetupMocksForBitVectorPatConfigSetPoint(setPointsToMock);

            // Run the test
            PatConfig.BitVectorPatConfigSetPoint("--bitvector 1011 --setpoint FUNC:Group:SetPoint%INDEX%_%VALUE%");

            // Verify all the mocks.
            allMocks.ForEach(it => it.VerifyAll());
        }

        /// <summary>
        /// Test pass case of BitVectorPatConfigSetPoint.
        /// </summary>
        [TestMethod]
        public void BitVectorPatConfigSetPoint_NoOptionalArgsDefault_Pass()
        {
            // Setup the mocks
            List<string> setPointsToMock = new List<string> { "FUNC:Group0_1", "FUNC:Group1_0", "FUNC:Group2_1", "FUNC:Group3_1" };
            var allMocks = this.SetupMocksForBitVectorPatConfigSetPoint(setPointsToMock);

            // Run the test
            PatConfig.BitVectorPatConfigSetPoint("--bitvector 1011 --setpoint FUNC:Group%INDEX%_%VALUE%");

            // Verify all the mocks.
            allMocks.ForEach(it => it.VerifyAll());
        }

        /// <summary>
        /// Test pass case of BitVectorPatConfigSetPoint.
        /// </summary>
        [TestMethod]
        public void BitVectorPatConfigSetPoint_MsbFirstValueMap_Pass()
        {
            // Setup the mocks
            List<string> setPointsToMock = new List<string> { "FUNC:Group:SetPoint003_MASK", "FUNC:Group:SetPoint002_STROBE", "FUNC:Group:SetPoint001_MASK", "FUNC:Group:SetPoint000_MASK" };
            var allMocks = this.SetupMocksForBitVectorPatConfigSetPoint(setPointsToMock);

            // Run the test
            PatConfig.BitVectorPatConfigSetPoint("--bitvector 10,11 --setpoint FUNC:Group:SetPoint%INDEX%_%VALUE% --msb_first --value_map 0:STROBE,1:MASK --index_width 3");

            // Verify all the mocks.
            allMocks.ForEach(it => it.VerifyAll());
        }

        /// <summary>
        /// Test pass case of BitVectorPatConfigSetPoint.
        /// </summary>
        [TestMethod]
        public void BitVectorPatConfigSetPoint_Gsds_Pass()
        {
            // Setup the mocks
            List<string> setPointsToMock = new List<string> { "FUNC:Group:MASK0", "FUNC:Group:STROBE1", "FUNC:Group:MASK2", "FUNC:Group:MASK3" };
            var allMocks = this.SetupMocksForBitVectorPatConfigSetPoint(setPointsToMock);

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInStringTable("DUMMY", Context.DUT)).Returns(true);
            sharedStorageMock.Setup(o => o.GetStringRowFromTable("DUMMY", Context.DUT)).Returns("1011");
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // Run the test
            PatConfig.BitVectorPatConfigSetPoint("--bitvector G.U.S.DUMMY --setpoint FUNC:Group:%VALUE%%INDEX% --value_map 0:STROBE,1:MASK");

            // Verify all the mocks.
            allMocks.ForEach(it => it.VerifyAll());
            sharedStorageMock.VerifyAll();
        }

        private List<Mock> SetupMocksForBitVectorPatConfigSetPoint(List<string> setPointsToMock = null)
        {
            var allMocks = new List<Mock>();

            var testprogramMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramMock.Setup(o => o.GetCurrentTestInstanceName()).Returns("DummyTestInstance");
            Prime.Services.TestProgramService = testprogramMock.Object;
            allMocks.Add(testprogramMock);

            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            if (setPointsToMock != null)
            {
                var definedHandles = new Dictionary<string, Mock<IPatConfigSetPointHandle>>();
                foreach (var setpointName in setPointsToMock)
                {
                    var setpointSplit = setpointName.Split(':');
                    var setPointMock = definedHandles.ContainsKey($"{setpointSplit[0]}:{setpointSplit[1]}") ?
                        definedHandles[$"{setpointSplit[0]}:{setpointSplit[1]}"] : new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
                    if (setpointSplit.Length == 2)
                    {
                        Console.WriteLine($"Mocking PatConfigSetPoint Module=[{setpointSplit[0]}] Group=[{setpointSplit[1]}] ApplySetPointDefault ");
                        setPointMock.Setup(o => o.ApplySetPointDefault());
                    }
                    else
                    {
                        Console.WriteLine($"Mocking PatConfigSetPoint Module=[{setpointSplit[0]}] Group=[{setpointSplit[1]}] ApplySetPoint({setpointSplit[2]})");
                        setPointMock.Setup(o => o.ApplySetPoint(setpointSplit[2]));
                    }

                    patConfigServiceMock.Setup(o => o.GetSetPointHandle(setpointSplit[0], setpointSplit[1])).Returns(setPointMock.Object);
                    allMocks.Add(setPointMock);
                    definedHandles[$"{setpointSplit[0]}:{setpointSplit[1]}"] = setPointMock;
                }

                Prime.Services.PatConfigService = patConfigServiceMock.Object;
                allMocks.Add(patConfigServiceMock);
            }

            return allMocks;
        }
    }
}
