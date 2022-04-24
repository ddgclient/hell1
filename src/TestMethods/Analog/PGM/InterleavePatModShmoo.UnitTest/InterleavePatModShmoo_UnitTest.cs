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

namespace InterleavePatModShmoo.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.TestConditionService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class InterleavePatModShmoo_UnitTest
    {
        /// <summary>
        /// Test console print debug message with X axis parameter not found in level or timing specset.
        /// </summary>
        [TestMethod]
        public void Verify_PrintConsoleDebugWithUnidentifiedXAxisParams_True()
        {
            // Elements to mock
            var plistName = "myValidPlist";
            var lvlName = "myValidLevel";
            var timName = "myValidTiming";

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func => func.CreateCaptureFailureTest(plistName, lvlName, timName, 1, string.Empty))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var tcLvlMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcLvlMock.Setup(lvl => lvl.GetSpecSetValue("A")).Throws(new FatalException("fatal"));
            var tcTimMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcTimMock.Setup(tim => tim.GetSpecSetValue("A")).Throws(new FatalException("fatal"));

            var tcMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcMock.Setup(tc => tc.GetTestCondition(lvlName)).Returns(tcLvlMock.Object);
            tcMock.Setup(tc => tc.GetTestCondition(timName)).Returns(tcTimMock.Object);
            Prime.Services.TestConditionService = tcMock.Object;

            var classUnderTest = new InterleavePatModShmoo()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                XAxisParam = "A",
                XAxisRange = "0.1:0.1:2",
                YAxisParam = string.Empty,
                YAxisRange = string.Empty,
                ConfigList = "SIO:TXEQ",
                ConfigSetPoints = "TXEQ1, TXEQ2",
            };

            var ex = Assert.ThrowsException<TestMethodException>(() => classUnderTest.Verify());
            Assert.AreEqual(ex.Message, "XAxisParameter value=[A] is not a valid TC.\n");
            funcMock.VerifyAll();
            tcLvlMock.VerifyAll();
            tcTimMock.VerifyAll();
            tcMock.VerifyAll();
        }

        /// <summary>
        /// Test console print test method exception when test condition input value is empty.
        /// </summary>
        [TestMethod]
        public void Verify_PrintTcExceptionWithEmptyTcValue_ThrowsException()
        {
            // Elements to mock
            var plistName = "myValidPlist";
            var lvlName = string.Empty;
            var timName = "myValidTiming";

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func => func.CreateCaptureFailureTest(plistName, lvlName, timName, 1, string.Empty))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var tcMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcMock.Setup(tc => tc.GetTestCondition(string.Empty)).Throws(
               new TestMethodException("Prime.Services.TestConditionService.GetTestCondition(this.LevelsTc)"));
            Prime.Services.TestConditionService = tcMock.Object;

            var classUnderTest = new InterleavePatModShmoo()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                XAxisParam = "A",
                YAxisParam = string.Empty,
                ConfigList = "SIO:TXEQ",
                ConfigSetPoints = "TXEQ1, TXEQ2",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };

            var ex = Assert.ThrowsException<TestMethodException>(() => classUnderTest.Verify());
            Assert.AreEqual(ex.Message, "Prime.Services.TestConditionService.GetTestCondition(this.LevelsTc)");
            funcMock.VerifyAll();
            tcMock.VerifyAll();
        }

        /// <summary>
        /// Test X range value with incorrect format, expect to throw exception.
        /// </summary>
        [TestMethod]
        public void Verify_IncorrectXRangeValue_ThrowsException()
        {
            // Elements to mock
            var plistName = "myValidPlist";
            var lvlName = "myValidLevel";
            var timName = "myValidTiming";

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func => func.CreateCaptureFailureTest(plistName, lvlName, timName, 1, string.Empty))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var tcLvlMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcLvlMock.Setup(lv => lv.GetSpecSetValue("A")).Returns(string.Empty);
            tcLvlMock.Setup(lv => lv.GetSpecSetValue("A")).Returns("0.5");

            var tcTmgMock = new Mock<ITestCondition>(MockBehavior.Strict);

            var tcMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcMock.Setup(tc => tc.GetTestCondition(lvlName)).Returns(tcLvlMock.Object);
            tcMock.Setup(tc => tc.GetTestCondition(timName)).Returns(tcTmgMock.Object);
            Prime.Services.TestConditionService = tcMock.Object;

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug("OriSpecSet for X=[0.5]\n"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var classUnderTest = new InterleavePatModShmoo()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                XAxisParam = "A",
                XAxisRange = "0.1:0.1,2",
                YAxisParam = string.Empty,
                YAxisRange = string.Empty,
                ConfigList = "SIO:TXEQ",
                ConfigSetPoints = "TXEQ1, TXEQ2",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };

            var ex = Assert.ThrowsException<TestMethodException>(() => classUnderTest.Verify());
            Assert.AreEqual(ex.Message, "No axis is being defined or incorrect format.");
            funcMock.VerifyAll();
            tcLvlMock.VerifyAll();
            tcTmgMock.VerifyAll();
            tcMock.VerifyAll();
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test ConfigList with incorrect format, expect to throw exception.
        /// </summary>
        [TestMethod]
        public void Verify_IncorrectConfigListRangeValue_ThrowsException()
        {
            // Elements to mock
            var plistName = "myValidPlist";
            var lvlName = "myValidLevel";
            var timName = "myValidTiming";

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func => func.CreateCaptureFailureTest(plistName, lvlName, timName, 1, string.Empty))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var tcLvlMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcLvlMock.Setup(lv => lv.GetSpecSetValue("A")).Returns(string.Empty);
            tcLvlMock.Setup(lv => lv.GetSpecSetValue("A")).Returns("0.5");

            var tcTmgMock = new Mock<ITestCondition>(MockBehavior.Strict);

            var tcMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcMock.Setup(tc => tc.GetTestCondition(lvlName)).Returns(tcLvlMock.Object);
            tcMock.Setup(tc => tc.GetTestCondition(timName)).Returns(tcTmgMock.Object);
            Prime.Services.TestConditionService = tcMock.Object;

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug("OriSpecSet for X=[0.5]\n"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var classUnderTest = new InterleavePatModShmoo()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                XAxisParam = "A",
                XAxisRange = "0.1:0.1:2",
                YAxisParam = string.Empty,
                YAxisRange = string.Empty,
                ConfigList = "SIO TXEQ",
                ConfigSetPoints = "TXEQ1, TXEQ2",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };

            var ex = Assert.ThrowsException<TestMethodException>(() => classUnderTest.Verify());
            Assert.AreEqual(ex.Message, "Incorrect format for ConfigList. Format should be <Module>:<Group> as specified in PatConfigSetPoint.json file.");
            funcMock.VerifyAll();
            tcLvlMock.VerifyAll();
            tcTmgMock.VerifyAll();
            tcMock.VerifyAll();
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test verify return true if all parameters are valid.
        /// </summary>
        [TestMethod]
        public void Verify_AllValidParameters_True()
        {
            // Elements to mock
            var plistName = "myValidPlist";
            var lvlName = "myValidLevel";
            var timName = "myValidTiming";

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func => func.CreateCaptureFailureTest(plistName, lvlName, timName, 1, string.Empty))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var tcLvlMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcLvlMock.Setup(lvl => lvl.GetSpecSetValue("A")).Returns("0.5");
            tcLvlMock.Setup(lvl => lvl.GetSpecSetValue("B")).Throws(new FatalException("fatal"));

            var tcTmgMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcTmgMock.Setup(tmg => tmg.GetSpecSetValue("B")).Returns("0.7");

            var tcMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcMock.Setup(tc => tc.GetTestCondition(lvlName)).Returns(tcLvlMock.Object);
            tcMock.Setup(tc => tc.GetTestCondition(timName)).Returns(tcTmgMock.Object);
            Prime.Services.TestConditionService = tcMock.Object;

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug("OriSpecSet for X=[0.5]\n"));
            consoleServiceMock.Setup(console => console.PrintDebug("OriSpecSet for Y=[0.7]\n"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var classUnderTest = new InterleavePatModShmoo()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                XAxisParam = "A",
                XAxisRange = "0.02:0.50:3",
                YAxisParam = "B",
                YAxisRange = "0.04:0.70:4",
                ConfigList = "SIO:TXEQ",
                ConfigSetPoints = "TXEQ1, TXEQ2",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };

            classUnderTest.Verify();
            funcMock.VerifyAll();
            tcLvlMock.VerifyAll();
            tcTmgMock.VerifyAll();
            tcMock.VerifyAll();
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Check xy point execution passing, execution expect to exit on port 1. For only 1 configsetpoint.
        /// </summary>
        [TestMethod]
        public void Execute_AllPass_OneconfigSetPoint_Port1()
        {
            // Elements to mock
            var plistName = "myValidPlist";
            var lvlName = "myValidLevel";
            var timName = "myValidTiming";
            List<IFailureData> listOfFailsPins = new List<IFailureData>();

            // mock all test points pass
            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcMock.Setup(func => func.Execute()).Returns(true);
            funcMock.Setup(func => func.GetPerCycleFailures()).Returns(listOfFailsPins);

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func => func.CreateCaptureFailureTest(plistName, lvlName, timName, 1, string.Empty))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // get the original specset value
            var tcLvlMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcLvlMock.Setup(lvl => lvl.GetSpecSetValue("A")).Returns("0.5");
            tcLvlMock.Setup(lvl => lvl.GetSpecSetValue("B")).Throws(new FatalException("fatal")); // this makes X axis level tc, and Y axis timing

            var tcTimMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.7"));
            tcTimMock.Setup(tim => tim.GetSpecSetValue("B")).Returns("0.7");

            var tcMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcMock.Setup(tc => tc.GetTestCondition(lvlName)).Returns(tcLvlMock.Object);
            tcMock.Setup(tc => tc.GetTestCondition(timName)).Returns(tcTimMock.Object);
            Prime.Services.TestConditionService = tcMock.Object;

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug("OriSpecSet for X=[0.5]\n"));
            consoleServiceMock.Setup(console => console.PrintDebug("OriSpecSet for Y=[0.7]\n"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // setup patconfigservice handle
            var handleMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);

            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(mock => mock.GetSetPointHandle("SIO", "TXEQ")).Returns(handleMock.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            handleMock.Setup(mock => mock.ApplySetPoint("TXEQ1"));
            consoleServiceMock.Setup(console => console.PrintDebug("Applying setpoint=[TXEQ1]\n"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // setup the specset based on input param x and y axis ranges
            tcLvlMock.Setup(lvl => lvl.SetSpecSetValue("A", "0.1"));
            tcLvlMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting x-axis spec value=[0.1]"));
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.1"));
            tcTimMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting y-axis spec value=[0.1]"));

            tcLvlMock.Setup(lvl => lvl.SetSpecSetValue("A", "0.2"));
            tcLvlMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting x-axis spec value=[0.2]"));
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.2"));
            tcTimMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting y-axis spec value=[0.2]"));

            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.3"));
            tcTimMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting y-axis spec value=[0.3]"));

            // expected console output
            consoleServiceMock.Setup(func => func.PrintDebug("  Y\\X |   0.1 |   0.2 \n  0.1 |     *       *  \n  0.2 |     *       *  \n  0.3 |     *       *  "));

            // expected ituff output
            var ituffMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            ituffMock.Setup(ituff => ituff.SetTnamePostfix("_IPMShmoo_TXEQ1_SSTP"));
            ituffMock.Setup(ituff => ituff.SetData("X_0.5V_Y_0.7nS")); // expect X is level, Y is timing
            ituffMock.Setup(ituff => ituff.SetTnamePostfix("_IPMShmoo_TXEQ1^A^0.1^0.2^0.1_B^0.1^0.3^0.1"));
            ituffMock.Setup(ituff => ituff.SetData("**_**_**"));

            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(ituffMock.Object);
            datalogMock.Setup(datalog => datalog.WriteToItuff(ituffMock.Object));
            Prime.Services.DatalogService = datalogMock.Object;

            // restoring back to default
            handleMock.Setup(mock => mock.ApplySetPointDefault());
            consoleServiceMock.Setup(console => console.PrintDebug("Successfully apply default set point.\n"));
            tcLvlMock.Setup(lvl => lvl.SetSpecSetValue("A", "0.5"));
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.7"));

            var classUnderTest = new InterleavePatModShmoo()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                XAxisRange = "0.1:0.1:2",
                YAxisRange = "0.1:0.1:3",
                XAxisParam = "A",
                YAxisParam = "B",
                ConfigList = "SIO:TXEQ",
                ConfigSetPoints = "TXEQ1",
                IfeObject = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };

            classUnderTest.Verify();
            Assert.AreEqual(1, classUnderTest.Execute());
            Assert.AreEqual(0, listOfFailsPins.Count);
            funcMock.VerifyAll();
            tcLvlMock.VerifyAll();
            tcTimMock.VerifyAll();
            tcMock.VerifyAll();
            consoleServiceMock.VerifyAll();
            ituffMock.VerifyAll();
            datalogMock.VerifyAll();
        }

        /// <summary>
        /// Check xy point execution passing, execution expect to exit on port 1. For 2 configsetpoint.
        /// </summary>
        [TestMethod]
        public void Execute_AllPass_TwoconfigSetPoint_Port1()
        {
            // Elements to mock
            var plistName = "myValidPlist";
            var lvlName = "myValidLevel";
            var timName = "myValidTiming";
            List<IFailureData> listOfFailsPins = new List<IFailureData>();

            // mock all test points pass
            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcMock.Setup(func => func.Execute()).Returns(true);
            funcMock.Setup(func => func.GetPerCycleFailures()).Returns(listOfFailsPins);

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func => func.CreateCaptureFailureTest(plistName, lvlName, timName, 1, string.Empty))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // get the original specset value
            var tcLvlMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcLvlMock.Setup(lvl => lvl.GetSpecSetValue("A")).Returns("0.5");
            tcLvlMock.Setup(lvl => lvl.GetSpecSetValue("B")).Throws(new FatalException("fatal")); // this makes X axis level tc, and Y axis timing

            var tcTimMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.7"));
            tcTimMock.Setup(tim => tim.GetSpecSetValue("B")).Returns("0.7");

            var tcMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcMock.Setup(tc => tc.GetTestCondition(lvlName)).Returns(tcLvlMock.Object);
            tcMock.Setup(tc => tc.GetTestCondition(timName)).Returns(tcTimMock.Object);
            Prime.Services.TestConditionService = tcMock.Object;

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug("OriSpecSet for X=[0.5]\n"));
            consoleServiceMock.Setup(console => console.PrintDebug("OriSpecSet for Y=[0.7]\n"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // setup patconfigservice handle
            var handleMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);

            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(mock => mock.GetSetPointHandle("SIO", "TXEQ")).Returns(handleMock.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            handleMock.Setup(mock => mock.ApplySetPoint("TXEQ1"));
            consoleServiceMock.Setup(console => console.PrintDebug("Applying setpoint=[TXEQ1]\n"));
            handleMock.Setup(mock => mock.ApplySetPoint("TXEQ2"));
            consoleServiceMock.Setup(console => console.PrintDebug("Applying setpoint=[TXEQ2]\n"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // setup the specset based on input param x and y axis ranges
            tcLvlMock.Setup(lvl => lvl.SetSpecSetValue("A", "0.1"));
            tcLvlMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting x-axis spec value=[0.1]"));
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.1"));
            tcTimMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting y-axis spec value=[0.1]"));

            tcLvlMock.Setup(lvl => lvl.SetSpecSetValue("A", "0.2"));
            tcLvlMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting x-axis spec value=[0.2]"));
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.2"));
            tcTimMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting y-axis spec value=[0.2]"));

            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.3"));
            tcTimMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting y-axis spec value=[0.3]"));

            // expected console output
            consoleServiceMock.Setup(func => func.PrintDebug("  Y\\X |   0.1 |   0.2 \n  0.1 |     *       *  \n  0.2 |     *       *  \n  0.3 |     *       *  "));

            // expected ituff output
            var ituffMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            ituffMock.Setup(ituff => ituff.SetTnamePostfix("_IPMShmoo_TXEQ1_SSTP"));
            ituffMock.Setup(ituff => ituff.SetTnamePostfix("_IPMShmoo_TXEQ2_SSTP"));
            ituffMock.Setup(ituff => ituff.SetData("X_0.5V_Y_0.7nS")); // expect X is level, Y is timing
            ituffMock.Setup(ituff => ituff.SetTnamePostfix("_IPMShmoo_TXEQ1^A^0.1^0.2^0.1_B^0.1^0.3^0.1"));
            ituffMock.Setup(ituff => ituff.SetTnamePostfix("_IPMShmoo_TXEQ2^A^0.1^0.2^0.1_B^0.1^0.3^0.1"));
            ituffMock.Setup(ituff => ituff.SetData("**_**_**"));

            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(ituffMock.Object);
            datalogMock.Setup(datalog => datalog.WriteToItuff(ituffMock.Object));
            Prime.Services.DatalogService = datalogMock.Object;

            // restoring back to default
            handleMock.Setup(mock => mock.ApplySetPointDefault());
            consoleServiceMock.Setup(console => console.PrintDebug("Successfully apply default set point.\n"));
            tcLvlMock.Setup(lvl => lvl.SetSpecSetValue("A", "0.5"));
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.7"));

            var classUnderTest = new InterleavePatModShmoo()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                XAxisRange = "0.1:0.1:2",
                YAxisRange = "0.1:0.1:3",
                XAxisParam = "A",
                YAxisParam = "B",
                ConfigList = "SIO:TXEQ",
                ConfigSetPoints = "TXEQ1, TXEQ2",
                IfeObject = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };

            classUnderTest.Verify();
            Assert.AreEqual(1, classUnderTest.Execute());
            Assert.AreEqual(0, listOfFailsPins.Count);
            funcMock.VerifyAll();
            tcLvlMock.VerifyAll();
            tcTimMock.VerifyAll();
            tcMock.VerifyAll();
            consoleServiceMock.VerifyAll();
            ituffMock.VerifyAll();
            datalogMock.VerifyAll();
        }

        /// <summary>
        ///  Check xy point execution with fail shmoo test, port stil expect to exit port 1 with shmoo plot fail characters.
        /// </summary>
        [TestMethod]
        public void Execute_FailingShmoo_Port1()
        {
            // Elements to mock
            var plistName = "myValidPlist";
            var lvlName = "myValidLevel";
            var timName = "myValidTiming";

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            List<IFailureData> listOfFailData = new List<IFailureData>();
            failDataMock.Setup(failData => failData.GetFailingPinNames()).Returns(new List<string>() { "P001", "P002" });
            failDataMock.Setup(failData => failData.GetPatternName()).Returns("pattern_001");
            failDataMock.Setup(failData => failData.GetVectorAddress()).Returns(1);
            failDataMock.Setup(failData => failData.GetCycle()).Returns(10);
            failDataMock.Setup(failData => failData.GetDomainName()).Returns("DDR");
            listOfFailData.Add(failDataMock.Object);

            // mock all test points pass
            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcMock.Setup(func => func.Execute()).Returns(false);
            funcMock.Setup(func => func.GetPerCycleFailures()).Returns(listOfFailData);

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func => func.CreateCaptureFailureTest(plistName, lvlName, timName, 1, string.Empty))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // get the original specset value
            var tcLvlMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcLvlMock.Setup(lvl => lvl.GetSpecSetValue("A")).Returns("0.5");
            tcLvlMock.Setup(lvl => lvl.GetSpecSetValue("B")).Throws(new FatalException("fatal")); // this makes X axis level tc, and Y axis timing

            var tcTimMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.7"));
            tcTimMock.Setup(tim => tim.GetSpecSetValue("B")).Returns("0.7");

            var tcMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcMock.Setup(tc => tc.GetTestCondition(lvlName)).Returns(tcLvlMock.Object);
            tcMock.Setup(tc => tc.GetTestCondition(timName)).Returns(tcTimMock.Object);
            Prime.Services.TestConditionService = tcMock.Object;

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug("OriSpecSet for X=[0.5]\n"));
            consoleServiceMock.Setup(console => console.PrintDebug("OriSpecSet for Y=[0.7]\n"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // setup patconfigservice handle
            var handleMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);

            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(mock => mock.GetSetPointHandle("SIO", "TXEQ")).Returns(handleMock.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            handleMock.Setup(mock => mock.ApplySetPoint("TXEQ1"));
            consoleServiceMock.Setup(console => console.PrintDebug("Applying setpoint=[TXEQ1]\n"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // setup the specset based on input param x and y axis ranges
            tcLvlMock.Setup(lvl => lvl.SetSpecSetValue("A", "0.1"));
            tcLvlMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting x-axis spec value=[0.1]"));
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.1"));
            tcTimMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting y-axis spec value=[0.1]"));

            tcLvlMock.Setup(lvl => lvl.SetSpecSetValue("A", "0.2"));
            tcLvlMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting x-axis spec value=[0.2]"));
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.2"));
            tcTimMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting y-axis spec value=[0.2]"));

            // expected console output
            consoleServiceMock.Setup(console => console.PrintDebug("  Y\\X |   0.1 |   0.2 \n  0.1 |     a       a  \n  0.2 |     a       a  "));
            consoleServiceMock.Setup(console => console.PrintDebug("Legend : [a] | Failure information: <pattern_001>:DDR(10,1,-1,-1):P001, P002"));

            // expected ituff output
            var ituffMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            ituffMock.Setup(ituff => ituff.SetTnamePostfix("_IPMShmoo_TXEQ1_SSTP"));
            ituffMock.Setup(ituff => ituff.SetData("X_0.5V_Y_0.7nS")); // expect X is level, Y is timing
            ituffMock.Setup(ituff => ituff.SetTnamePostfix("_IPMShmoo_TXEQ1^A^0.1^0.2^0.1_B^0.1^0.2^0.1"));
            ituffMock.Setup(ituff => ituff.SetData("aa_aa"));
            ituffMock.Setup(ituff => ituff.SetTnamePostfix("_IPMShmoo_TXEQ1^LEGEND^a"));
            ituffMock.Setup(ituff => ituff.SetData("<pattern_001>:DDR(10,1,-1,-1):P001, P002"));

            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(ituffMock.Object);
            datalogMock.Setup(datalog => datalog.WriteToItuff(ituffMock.Object));
            Prime.Services.DatalogService = datalogMock.Object;

            // restoring back to default
            handleMock.Setup(mock => mock.ApplySetPointDefault());
            consoleServiceMock.Setup(console => console.PrintDebug("Successfully apply default set point.\n"));
            tcLvlMock.Setup(lvl => lvl.SetSpecSetValue("A", "0.5"));
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.7"));

            var classUnderTest = new InterleavePatModShmoo()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                XAxisRange = "0.1:0.1:2",
                YAxisRange = "0.1:0.1:2",
                XAxisParam = "A",
                YAxisParam = "B",
                ConfigList = "SIO:TXEQ",
                ConfigSetPoints = "TXEQ1",
                IfeObject = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };

            classUnderTest.Verify();
            Assert.AreEqual(1, classUnderTest.Execute());
            Assert.AreEqual(1, listOfFailData.Count);
            funcMock.VerifyAll();
            tcLvlMock.VerifyAll();
            tcTimMock.VerifyAll();
            tcMock.VerifyAll();
            consoleServiceMock.VerifyAll();
            ituffMock.VerifyAll();
            datalogMock.VerifyAll();
        }

        /// <summary>
        ///  Check xy point execution with passfail shmoo points, port stil expect to exit port 1 with a mixed of shmoo plot pass/fail characters.
        /// </summary>
        [TestMethod]
        public void Execute_PassFailShmooPoints_Port1()
        {
            // Elements to mock
            var plistName = "myValidPlist";
            var lvlName = "myValidLevel";
            var timName = "myValidTiming";

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            List<IFailureData> listOfFailData = new List<IFailureData>();
            failDataMock.SetupSequence(failData => failData.GetFailingPinNames())
                .Returns(new List<string>() { "P001", "P002" })
                .Returns(new List<string>() { "P001" });
            failDataMock.SetupSequence(failData => failData.GetPatternName())
                .Returns("pattern_001")
                .Returns("pattern_002");
            failDataMock.SetupSequence(failData => failData.GetVectorAddress())
                .Returns(1)
                .Returns(1);
            failDataMock.SetupSequence(failData => failData.GetCycle())
                .Returns(10)
                .Returns(20);
            failDataMock.SetupSequence(failData => failData.GetDomainName())
                .Returns("DDR")
                .Returns("LEG");
            listOfFailData.Add(failDataMock.Object);

            // mock all test points pass
            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcMock.SetupSequence(func => func.Execute())
                .Returns(true)
                .Returns(false)
                .Returns(true)
                .Returns(false);
            funcMock.SetupSequence(func => func.GetPerCycleFailures())
                .Returns(new List<IFailureData>())
                .Returns(listOfFailData)
                .Returns(new List<IFailureData>())
                .Returns(listOfFailData);

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func => func.CreateCaptureFailureTest(plistName, lvlName, timName, 1, string.Empty))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // get the original specset value
            var tcLvlMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcLvlMock.Setup(lvl => lvl.GetSpecSetValue("A")).Returns("0.5");
            tcLvlMock.Setup(lvl => lvl.GetSpecSetValue("B")).Throws(new FatalException("fatal")); // this makes X axis level tc, and Y axis timing

            var tcTimMock = new Mock<ITestCondition>(MockBehavior.Strict);
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.7"));
            tcTimMock.Setup(tim => tim.GetSpecSetValue("B")).Returns("0.7");

            var tcMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            tcMock.Setup(tc => tc.GetTestCondition(lvlName)).Returns(tcLvlMock.Object);
            tcMock.Setup(tc => tc.GetTestCondition(timName)).Returns(tcTimMock.Object);
            Prime.Services.TestConditionService = tcMock.Object;

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug("OriSpecSet for X=[0.5]\n"));
            consoleServiceMock.Setup(console => console.PrintDebug("OriSpecSet for Y=[0.7]\n"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // setup patconfigservice handle
            var handleMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);

            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(mock => mock.GetSetPointHandle("SIO", "TXEQ")).Returns(handleMock.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            handleMock.Setup(mock => mock.ApplySetPoint("TXEQ1"));
            consoleServiceMock.Setup(console => console.PrintDebug("Applying setpoint=[TXEQ1]\n"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // setup the specset based on input param x and y axis ranges
            tcLvlMock.Setup(lvl => lvl.SetSpecSetValue("A", "0.1"));
            tcLvlMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting x-axis spec value=[0.1]"));
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.1"));
            tcTimMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting y-axis spec value=[0.1]"));

            tcLvlMock.Setup(lvl => lvl.SetSpecSetValue("A", "0.2"));
            tcLvlMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting x-axis spec value=[0.2]"));
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.2"));
            tcTimMock.Setup(lvl => lvl.ForceApply());
            consoleServiceMock.Setup(console => console.PrintDebug($"Shmoo setting y-axis spec value=[0.2]"));

            // expected console output
            consoleServiceMock.Setup(console => console.PrintDebug("  Y\\X |   0.1 |   0.2 \n  0.1 |     *       a  \n  0.2 |     *       b  "));
            consoleServiceMock.Setup(console => console.PrintDebug("Legend : [a] | Failure information: <pattern_001>:DDR(10,1,-1,-1):P001, P002"));
            consoleServiceMock.Setup(console => console.PrintDebug("Legend : [b] | Failure information: <pattern_002>:LEG(20,1,-1,-1):P001"));

            // expected ituff output
            var ituffMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            ituffMock.Setup(ituff => ituff.SetTnamePostfix("_IPMShmoo_TXEQ1_SSTP"));
            ituffMock.Setup(ituff => ituff.SetData("X_0.5V_Y_0.7nS")); // expect X is level, Y is timing
            ituffMock.Setup(ituff => ituff.SetTnamePostfix("_IPMShmoo_TXEQ1^A^0.1^0.2^0.1_B^0.1^0.2^0.1"));
            ituffMock.Setup(ituff => ituff.SetData("*a_*b"));
            ituffMock.Setup(ituff => ituff.SetTnamePostfix("_IPMShmoo_TXEQ1^LEGEND^a"));
            ituffMock.Setup(ituff => ituff.SetTnamePostfix("_IPMShmoo_TXEQ1^LEGEND^b"));
            ituffMock.Setup(ituff => ituff.SetData("<pattern_001>:DDR(10,1,-1,-1):P001, P002"));
            ituffMock.Setup(ituff => ituff.SetData("<pattern_002>:LEG(20,1,-1,-1):P001"));

            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(ituffMock.Object);
            datalogMock.Setup(datalog => datalog.WriteToItuff(ituffMock.Object));
            Prime.Services.DatalogService = datalogMock.Object;

            // restoring back to default
            handleMock.Setup(mock => mock.ApplySetPointDefault());
            consoleServiceMock.Setup(console => console.PrintDebug("Successfully apply default set point.\n"));
            tcLvlMock.Setup(lvl => lvl.SetSpecSetValue("A", "0.5"));
            tcTimMock.Setup(tmg => tmg.SetSpecSetValue("B", "0.7"));

            var classUnderTest = new InterleavePatModShmoo()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                XAxisRange = "0.1:0.1:2",
                YAxisRange = "0.1:0.1:2",
                XAxisParam = "A",
                YAxisParam = "B",
                ConfigList = "SIO:TXEQ",
                ConfigSetPoints = "TXEQ1",
                IfeObject = string.Empty,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };

            classUnderTest.Verify();
            Assert.AreEqual(1, classUnderTest.Execute());
            Assert.AreEqual(1, listOfFailData.Count);
            funcMock.VerifyAll();
            tcLvlMock.VerifyAll();
            tcTimMock.VerifyAll();
            tcMock.VerifyAll();
            consoleServiceMock.VerifyAll();
            ituffMock.VerifyAll();
            datalogMock.VerifyAll();
        }
    }
}
