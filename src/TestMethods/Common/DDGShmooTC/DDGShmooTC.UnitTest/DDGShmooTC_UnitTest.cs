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

namespace DDGShmooTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PerformanceService;
    using Prime.TestConditionService;
    using Prime.TestMethods;
    using Prime.TestMethods.Shmoo;
    using Prime.TestProgramService;
    using Prime.UserVarService;
    using Prime.VoltageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class DDGShmooTC_UnitTest
    {
        private Mock<IPerformanceService> performanceServiceMock;

        private string InstanceName { get; set; } = "DummyInstance";

        private DDGShmooTC.ItuffMode ItuffFormat { get; set; } = DDGShmooTC.ItuffMode.SHMOO_HUB;

        private string Plist { get; set; } = "DummyPlist";

        private string Timings { get; set; } = "DummyTimings";

        private string Levels { get; set; } = "DummyLevels";

        private PrimeShmooTestMethod.PowerDownBetweenPointsSettings PowerDownBetweenPoints { get; set; } = PrimeShmooTestMethod.PowerDownBetweenPointsSettings.DISABLED;

        private string VoltageConverterParameter { get; set; } = string.Empty;

        private DDGShmooTC.AxisType YAxisType { get; set; } = DDGShmooTC.AxisType.SpecSetVariable;

        private string YParamName { get; set; } = "DummyLevelsParameter";

        private string YParamOriginalValue { get; set; } = "1";

        private string YParamRange { get; set; } = "0.75:0.05:5";

        private string YParamItuff { get; set; } = "0.75^0.95^0.05";

        private List<string> YParamTestPoints { get; set; } = new List<string> { "0.75", "0.8", "0.85", "0.9", "0.95" };

        private DDGShmooTC.AxisType XAxisType { get; set; } = DDGShmooTC.AxisType.SpecSetVariable;

        private string XParamName { get; set; } = "DummyTimingParameter";

        private string XParamOriginalValue { get; set; } = "1E-08";

        private string XParamRange { get; set; } = "8e-09:1E-09:4";

        /* private string XParamItuff { get; set; } = "0.000000008^0.000000011^0.000000001"; Use "Nano" format */
        private string XParamItuff { get; set; } = "8^11^1";

        private List<string> XParamTestPoints { get; set; } = new List<string> { "8E-09", "9E-09", "1E-08", "1.1E-08" };

        private string ItuffShmooHubFormat { get; set; } = "AAAA_BBBB_CCC*_****_****";

        private List<Tuple<bool, string, string>> TestPointResults { get; set; } = new List<Tuple<bool, string, string>>
        {
            new Tuple<bool, string, string>(false, "A", "pat1:DummyPlist:LEG(0,557,-1,-1):IP_CPU::TDO1,TDO2"), // x=0, y=0
            new Tuple<bool, string, string>(false, "A", "pat1:DummyPlist:LEG(0,557,-1,-1):IP_CPU::TDO1,TDO2"), // x=1, y=0
            new Tuple<bool, string, string>(false, "A", "pat1:DummyPlist:LEG(0,557,-1,-1):IP_CPU::TDO1,TDO2"), // x=2, y=0
            new Tuple<bool, string, string>(false, "A", "pat1:DummyPlist:LEG(0,557,-1,-1):IP_CPU::TDO1,TDO2"), // x=3, y=0
            new Tuple<bool, string, string>(false, "B", "pat2:DummyPlist:LEG(0,561,-1,-1):IP_CPU::TDO1,TDO2"), // x=0, y=1
            new Tuple<bool, string, string>(false, "B", "pat2:DummyPlist:LEG(0,561,-1,-1):IP_CPU::TDO1,TDO2"), // x=1, y=1
            new Tuple<bool, string, string>(false, "B", "pat2:DummyPlist:LEG(0,561,-1,-1):IP_CPU::TDO1,TDO2"), // x=2, y=1
            new Tuple<bool, string, string>(false, "B", "pat2:DummyPlist:LEG(0,561,-1,-1):IP_CPU::TDO1,TDO2"), // x=3, y=1
            new Tuple<bool, string, string>(false, "C", "pat3:DummyPlist:LEG(0,581,-1,-1):IP_CPU::TDO1,TDO2"), // x=0, y=2
            new Tuple<bool, string, string>(false, "C", "pat3:DummyPlist:LEG(0,581,-1,-1):IP_CPU::TDO1,TDO2"), // x=1, y=2
            new Tuple<bool, string, string>(false, "C", "pat3:DummyPlist:LEG(0,581,-1,-1):IP_CPU::TDO1,TDO2"), // x=2, y=2
            new Tuple<bool, string, string>(true, string.Empty, string.Empty), // x=3, y=2
            new Tuple<bool, string, string>(true, string.Empty, string.Empty), // x=0, y=3
            new Tuple<bool, string, string>(true, string.Empty, string.Empty), // x=1, y=3
            new Tuple<bool, string, string>(true, string.Empty, string.Empty), // x=2, y=3
            new Tuple<bool, string, string>(true, string.Empty, string.Empty), // x=3, y=3
            new Tuple<bool, string, string>(true, string.Empty, string.Empty), // x=0, y=4
            new Tuple<bool, string, string>(true, string.Empty, string.Empty), // x=1, y=4
            new Tuple<bool, string, string>(true, string.Empty, string.Empty), // x=2, y=4
            new Tuple<bool, string, string>(true, string.Empty, string.Empty), // x=3, y=4
        };

        // Mocks
        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private Mock<ICaptureFailureTest> FuncTestMock { get; set; }

        private Mock<IFunctionalService> FuncServiceMock { get; set; }

        private Mock<IStrgvalFormat> ItuffStrgvalMock { get; set; }

        private Mock<IComntFormat> ItuffComntMock { get; set; }

        private Mock<IDatalogService> DatalogServiceMock { get; set; }

        private Mock<ITestCondition> LevelsTcMock { get; set; }

        private Mock<ITestCondition> TimingTcMock { get; set; }

        private Mock<IFivrDomains> XParamVoltageMock { get; set; }

        private Mock<IFivrDomains> YParamVoltageMock { get; set; }

        private Mock<IUserVarService> UserVarServiceMock { get; set; }

        private Mock<IPatConfigService> PatConfigServiceMock { get; set; }

        private List<Mock<IPatConfigHandle>> PatConfigMocks { get; set; }

        private List<Mock<IPatConfigSetPointHandle>> PatSetPointMocks { get; set; }

        private Mock<IVoltageService> VoltageServiceMock { get; set; }

        private Mock<ITestConditionService> TestConditionServiceMock { get; set; }

        private string VoltageOverrideString { get; set; } = string.Empty;

        private Dictionary<string, double> VoltageOverrideDict { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Default initialization.
        /// </summary>
        [TestInitialize]
        public void Initialization()
        {
            // Default Mock for ConsoleService
            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) => Console.WriteLine($"DEBUG: {msg}"));
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;

            this.performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = this.performanceServiceMock.Object;
        }

        /// <summary>
        /// Verify base shmoo.
        /// </summary>
        [TestMethod]
        public void Verify_Base_Pass()
        {
            var underTest = this.BuildBaseTest(false);
            this.VerifyBaseShmoo(ref underTest);
            underTest.CustomVerify();
            this.VerifyAll(false);
        }

        /// <summary>
        /// Verify base shmoo.
        /// </summary>
        [TestMethod]
        public void Verify_IllegalSpecSetVar_Fail()
        {
            this.MockFuncTest(false);
            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetSpecSetValue("InvalidParameterName")).Throws(new Prime.Base.Exceptions.FatalException("Parameter does not exist"));

            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(o => o.GetTestCondition(this.Levels)).Returns(testConditionMock.Object);
            testConditionServiceMock.Setup(o => o.GetTestCondition(this.Timings)).Returns(testConditionMock.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var underTest = new DDGShmooTC
            {
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                InstanceName = this.InstanceName,
                Patlist = this.Plist,
                TimingsTc = this.Timings,
                LevelsTc = this.Levels,
                XAxisParamType = PrimeShmooTestMethod.ShmooAxisType.UserDefined,
                YAxisParamType = PrimeShmooTestMethod.ShmooAxisType.UserDefined,

                XAxisType = DDGShmooTC.AxisType.SpecSetVariable,
                YAxisType = DDGShmooTC.AxisType.None,

                XAxisParam = "InvalidParameterName",
                XAxisRange = this.XParamRange,

                YAxisParam = string.Empty,
                YAxisRange = string.Empty,
            };

            underTest.TestMethodExtension = underTest;
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => this.VerifyBaseShmoo(ref underTest));
            Assert.AreEqual("X-Axis Parameter=[InvalidParameterName] is not a valid SpecSet Variable.", ex.Message);
            testConditionMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
        }

        /// <summary>
        /// Verify base shmoo.
        /// </summary>
        [TestMethod]
        public void Verify_SetPointFormat_Fail()
        {
            this.MockFuncTest(false);

            var underTest = new DDGShmooTC
            {
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                InstanceName = this.InstanceName,
                Patlist = this.Plist,
                TimingsTc = this.Timings,
                LevelsTc = this.Levels,
                XAxisParamType = PrimeShmooTestMethod.ShmooAxisType.UserDefined,
                YAxisParamType = PrimeShmooTestMethod.ShmooAxisType.UserDefined,

                XAxisType = DDGShmooTC.AxisType.PatConfigSetPoint,
                YAxisType = DDGShmooTC.AxisType.None,

                XAxisParam = "InvalidSetPointFormat",
                XAxisRange = this.XParamRange,

                YAxisParam = string.Empty,
                YAxisRange = string.Empty,
            };

            underTest.TestMethodExtension = underTest;
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => this.VerifyBaseShmoo(ref underTest));
            Assert.AreEqual("PatConfigSetpoint=[InvalidSetPointFormat] is in the wrong format. Expecting [module:group]", ex.Message);
        }

        /// <summary>
        /// Verify base shmoo.
        /// </summary>
        [TestMethod]
        public void Verify_BadRangeFormat_Fail()
        {
            var underTest = new DDGShmooTC
            {
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                InstanceName = this.InstanceName,
                Patlist = this.Plist,
                TimingsTc = this.Timings,
                LevelsTc = this.Levels,
                XAxisParamType = PrimeShmooTestMethod.ShmooAxisType.UserDefined,
                YAxisParamType = PrimeShmooTestMethod.ShmooAxisType.UserDefined,

                XAxisType = DDGShmooTC.AxisType.SpecSetVariable,
                YAxisType = DDGShmooTC.AxisType.None,

                XAxisParam = this.XParamName,
                XAxisRange = "1::2",

                YAxisParam = string.Empty,
                YAxisRange = string.Empty,
            };

            underTest.TestMethodExtension = underTest;
            this.MockFuncTest(false);
            this.MockTestConditions(false);

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => this.VerifyBaseShmoo(ref underTest));
            Assert.AreEqual("Invalid AxisRange=[1::2]. Expecting [START_POINT:RESOLUTION:NUMBER_OF_POINTS].", ex.Message);
        }

        /// <summary>
        /// Verify base shmoo.
        /// </summary>
        [TestMethod]
        public void Verify_PrePointExecMissingArg_Fail()
        {
            var underTest = this.BuildBaseTest(false);
            underTest.PrePointExecMode = DDGShmooTC.PrePointExecType.OnAnyChange;
            underTest.PrePointExecTest = string.Empty;

            this.VerifyBaseShmoo(ref underTest);
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.CustomVerify());
            Assert.AreEqual("Parameter=[PrePointExecTest] must be specified when Parameter=[PrePointExecMode] is not [Never].", ex.Message);
        }

        /// <summary>
        /// Verify base shmoo.
        /// </summary>
        [TestMethod]
        public void Verify_PrePointExecMissingTest_Fail()
        {
            var underTest = this.BuildBaseTest(false);
            underTest.PrePointExecMode = DDGShmooTC.PrePointExecType.OnAnyChange;
            underTest.PrePointExecTest = "SomeTestA";

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetAllTestInstanceNames()).Returns(new List<string> { "TestA", "TestB", "TestC" });
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            this.VerifyBaseShmoo(ref underTest);
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.CustomVerify());
            Assert.AreEqual("Parameter=[PrePointExecTest] is invalid.  Test=[SomeTestA] does not exist.", ex.Message);
            testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute base shmoo.
        /// </summary>
        [TestMethod]
        public void Execute_Base_Pass()
        {
            var underTest = this.BuildBaseTest(true);
            this.VerifyBaseShmoo(ref underTest);
            underTest.CustomVerify();
            Assert.AreEqual(0, underTest.Execute());
            this.VerifyAll(true);
        }

        /// <summary>
        /// Execute base shmoo.
        /// </summary>
        [TestMethod]
        public void Execute_PrePointExecOnY_Pass()
        {
            var underTest = this.BuildBaseTest(true);
            underTest.PrePointExecMode = DDGShmooTC.PrePointExecType.OnYChange;
            underTest.PrePointExecTest = "SetupTestA";

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetAllTestInstanceNames()).Returns(new List<string> { "TestA", "TestB", "TestC", "SetupTestA" });
            testProgramServiceMock.Setup(o => o.ExecuteTestInstance("SetupTestA")).Returns(1);
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            this.VerifyBaseShmoo(ref underTest);
            underTest.CustomVerify();
            Assert.AreEqual(0, underTest.Execute());
            this.VerifyAll(true);
            testProgramServiceMock.VerifyAll();
            testProgramServiceMock.Verify(o => o.ExecuteTestInstance("SetupTestA"), Times.Exactly(5));
        }

        /// <summary>
        /// Execute base shmoo.
        /// </summary>
        [TestMethod]
        public void Execute_PrePointExecOnXFailAll_Pass()
        {
            this.ItuffShmooHubFormat = "####_####_####_####_####";

            var underTest = this.BuildBaseTest(false);
            this.FuncTestMock.Setup(o => o.ApplyTestConditions());
            this.MockItuffShmooHub();
            this.TimingTcMock.Setup(o => o.SetSpecSetValue(this.XParamName, this.XParamOriginalValue));
            this.LevelsTcMock.Setup(o => o.SetSpecSetValue(this.YParamName, this.YParamOriginalValue));

            underTest.PrePointExecMode = DDGShmooTC.PrePointExecType.OnXChange;
            underTest.PrePointExecTest = "SetupTestA";

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetAllTestInstanceNames()).Returns(new List<string> { "TestA", "TestB", "TestC", "SetupTestA" });
            testProgramServiceMock.Setup(o => o.ExecuteTestInstance("SetupTestA")).Returns(0);
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            this.VerifyBaseShmoo(ref underTest);
            underTest.CustomVerify();
            Assert.AreEqual(1, underTest.Execute());
            this.VerifyAll(false, 0);
            testProgramServiceMock.VerifyAll();
            testProgramServiceMock.Verify(o => o.ExecuteTestInstance("SetupTestA"), Times.Exactly(20));
        }

        /// <summary>
        /// Execute base shmoo.
        /// </summary>
        [TestMethod]
        public void Execute_PinAttributeOverrides_Pass()
        {
            this.VoltageConverterParameter = "--overrides Pin1:1.2";
            this.VoltageOverrideDict = new Dictionary<string, double> { { "Pin1", 1.2 } };
            this.VoltageOverrideString = "Pin1:1.2";

            var underTest = this.BuildBaseTest(true);
            this.VerifyBaseShmoo(ref underTest);
            underTest.CustomVerify();
            Assert.AreEqual(0, underTest.Execute());
            this.VerifyAll(true);
        }

        /// <summary>
        /// Execute base shmoo.
        /// </summary>
        [TestMethod]
        public void Execute_FivrVsTimingPwrdnBetweenPoints_Pass()
        {
            this.YAxisType = DDGShmooTC.AxisType.FIVR;
            this.YParamName = "SA";
            this.YParamOriginalValue = "-1"; /* TODO: Cannot get default/current value for FIVR. */
            this.PowerDownBetweenPoints = PrimeShmooTestMethod.PowerDownBetweenPointsSettings.ENABLED;
            this.VoltageConverterParameter = "--fivrcondition NOM";

            var underTest = this.BuildBaseTest(true);
            this.TestConditionServiceMock.Setup(o => o.ApplyEndSequence());
            var yVoltage = new Mock<IFivrDomainsAndCondition>();
            this.YParamVoltageMock = yVoltage.As<IFivrDomains>();
            this.VoltageServiceMock.Setup(o => o.CreateFivrForDomainsAndCondition(new List<string> { "SA" }, "NOM", "DummyPlist")).Returns(yVoltage.Object);

            this.VerifyBaseShmoo(ref underTest);
            underTest.CustomVerify();
            Assert.AreEqual(0, underTest.Execute());
            this.VerifyAll(true);
            this.TestConditionServiceMock.Verify(o => o.ApplyEndSequence(), Times.Exactly(20));
        }

        /// <summary>
        /// Execute base shmoo.
        /// </summary>
        [TestMethod]
        public void Execute_FivrVsTimingWithVoltageConverter_Pass()
        {
            this.YAxisType = DDGShmooTC.AxisType.FIVR;
            this.YParamName = "SA";
            this.YParamOriginalValue = "-1"; /* TODO: Cannot get default/current value for FIVR. */
            this.VoltageConverterParameter = "--fivrcondition=NOM";

            var underTest = this.BuildBaseTest(true);
            var yVoltage = new Mock<IFivrDomainsAndCondition>();
            this.YParamVoltageMock = yVoltage.As<IFivrDomains>();
            this.VoltageServiceMock.Setup(o => o.CreateFivrForDomainsAndCondition(new List<string> { "SA" }, "NOM", "DummyPlist")).Returns(yVoltage.Object);

            this.VerifyBaseShmoo(ref underTest);
            underTest.CustomVerify();
            Assert.AreEqual(0, underTest.Execute());
            this.VerifyAll(true);
        }

        /// <summary>
        /// Execute base shmoo.
        /// </summary>
        [TestMethod]
        public void Execute_DlvrVsTiming_Pass()
        {
            this.YAxisType = DDGShmooTC.AxisType.FIVR;
            this.YParamName = "CCF";
            this.YParamOriginalValue = "-1"; /* TODO: Cannot get default/current value for DLVR. */
            this.VoltageConverterParameter = "--fivrcondition=NOM --railconfigurations=DlvrConfig";

            var underTest = this.BuildBaseTest(true);
            var yVoltage = new Mock<IFivrDomainsAndConditionWithRails>();
            this.YParamVoltageMock = yVoltage.As<IFivrDomains>();
            this.VoltageServiceMock.Setup(o => o.CreateFivrDomainsAndConditionWithRails(new List<string> { "CCF" }, "NOM", "DummyPlist", new List<string> { "DlvrConfig" })).Returns(yVoltage.Object);

            this.VerifyBaseShmoo(ref underTest);
            underTest.CustomVerify();
            Assert.AreEqual(0, underTest.Execute());
            this.VerifyAll(true);
        }

        /// <summary>
        /// Execute base shmoo.
        /// </summary>
        [TestMethod]
        public void Execute_DlvrVsDlvr_Pass()
        {
            this.XAxisType = DDGShmooTC.AxisType.FIVR;
            this.XParamName = "CORE";
            this.XParamOriginalValue = "-1"; /* TODO: Cannot get default/current value for DLVR. */
            this.XParamRange = "0.75:0.05:4";
            this.XParamItuff = "0.75^0.9^0.05";
            this.XParamTestPoints = new List<string> { "0.75", "0.8", "0.85", "0.9" };

            this.YAxisType = DDGShmooTC.AxisType.FIVR;
            this.YParamName = "CCF";
            this.YParamOriginalValue = "-1"; /* TODO: Cannot get default/current value for DLVR. */

            this.VoltageConverterParameter = "--railconfigurations=DlvrConfig pinattributes powerswitch --fivrcondition=NOM --overrides=domain:1.0";

            var underTest = this.BuildBaseTest(true);

            var xVoltage = new Mock<IFivrDomainsAndConditionWithRails>();
            var yVoltage = new Mock<IFivrDomainsAndConditionWithRails>();
            this.XParamVoltageMock = xVoltage.As<IFivrDomains>();
            this.YParamVoltageMock = yVoltage.As<IFivrDomains>();
            this.VoltageServiceMock.Setup(o => o.CreateFivrDomainsAndConditionWithRails(new List<string> { "CORE" }, "NOM", "DummyPlist", new List<string> { "DlvrConfig", "pinattributes", "powerswitch" })).Returns(xVoltage.Object);
            this.VoltageServiceMock.Setup(o => o.CreateFivrDomainsAndConditionWithRails(new List<string> { "CCF" }, "NOM", "DummyPlist", new List<string> { "DlvrConfig", "pinattributes", "powerswitch" })).Returns(yVoltage.Object);
            this.VerifyBaseShmoo(ref underTest);
            underTest.CustomVerify();
            Assert.AreEqual(0, underTest.Execute());
            this.VerifyAll(true);
        }

        /// <summary>
        /// Execute base shmoo.
        /// </summary>
        [TestMethod]
        public void Execute_UserVarShmoo_Pass()
        {
            this.XAxisType = DDGShmooTC.AxisType.UserVarTiming;
            this.XParamName = "CollectionX.UserVarX";

            this.YAxisType = DDGShmooTC.AxisType.UserVarLevels;
            this.YParamName = "CollectionY.UserVarY";

            var underTest = this.BuildBaseTest(true);
            this.VerifyBaseShmoo(ref underTest);
            underTest.CustomVerify();
            Assert.AreEqual(0, underTest.Execute());
            this.VerifyAll(true);
        }

        /// <summary>
        /// Execute base shmoo.
        /// </summary>
        [TestMethod]
        public void Execute_PatConfig1D_Pass()
        {
            this.XAxisType = DDGShmooTC.AxisType.PatConfig;
            this.XParamOriginalValue = "-1";
            this.XParamName = "PatConfig1,PatConfig2";
            this.XParamRange = "1:1:4";
            this.XParamTestPoints = new List<string> { "001", "010", "011", "100" };
            this.XParamItuff = "1^4^1";

            this.YAxisType = DDGShmooTC.AxisType.None;
            this.YParamName = string.Empty;
            this.YParamRange = string.Empty;
            this.YParamTestPoints = new List<string>();

            this.ItuffShmooHubFormat = "AA**";
            this.TestPointResults = new List<Tuple<bool, string, string>>
            {
                new Tuple<bool, string, string>(false, "A", "pat1|3:DummyPlist:LEG(0,557,-1,-1):IP_CPU::TDO"), // x=0
                new Tuple<bool, string, string>(false, "A", "pat1|3:DummyPlist:LEG(0,557,-1,-1):IP_CPU::TDO"), // x=1
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // x=2
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // x=3

                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
            };

            var underTest = this.BuildBaseTest(true);
            this.VerifyBaseShmoo(ref underTest);
            underTest.CustomVerify();
            Assert.AreEqual(0, underTest.Execute());
            this.VerifyAll(true, 4);
        }

        /// <summary>
        /// Execute base shmoo.
        /// </summary>
        [TestMethod]
        public void Execute_PatSetPoint1D_Pass()
        {
            this.XAxisType = DDGShmooTC.AxisType.PatConfigSetPoint;
            this.XParamOriginalValue = "-1";
            this.XParamName = "FUNC:CoreRatio";
            this.XParamRange = "0.4Ghz,0.8Ghz,1.2Ghz,1.6Ghz";
            this.XParamTestPoints = new List<string> { "0.4Ghz", "0.8Ghz", "1.2Ghz", "1.6Ghz" };
            this.XParamItuff = "0^3^1";

            this.YAxisType = DDGShmooTC.AxisType.None;
            this.YParamName = string.Empty;
            this.YParamRange = string.Empty;
            this.YParamTestPoints = new List<string>();

            this.ItuffShmooHubFormat = "AA**";
            this.TestPointResults = new List<Tuple<bool, string, string>>
            {
                new Tuple<bool, string, string>(false, "A", "pat1:DummyPList:LEG(0,557,-1,-1):IP_CPU::TDO"), // x=0
                new Tuple<bool, string, string>(false, "A", "pat1:DummyPList:LEG(0,557,-1,-1):IP_CPU::TDO"), // x=1
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // x=2
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // x=3

                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
                new Tuple<bool, string, string>(true, string.Empty, string.Empty), // dummy/padding
            };

            var underTest = this.BuildBaseTest(true);
            this.VerifyBaseShmoo(ref underTest);
            underTest.CustomVerify();
            Assert.AreEqual(0, underTest.Execute());
            this.VerifyAll(true, 4);
        }

        /// <summary>
        /// Execute base shmoo.
        /// </summary>
        [TestMethod]
        public void Execute_PatSetPointVsDlvrEcads_Pass()
        {
            this.XAxisType = DDGShmooTC.AxisType.PatConfigSetPoint;
            this.XParamOriginalValue = "-1";
            this.XParamName = "FUNC:CoreRatio";
            this.XParamRange = "0.4Ghz,0.8Ghz,1.2Ghz,1.6Ghz";
            this.XParamTestPoints = new List<string> { "0.4Ghz", "0.8Ghz", "1.2Ghz", "1.6Ghz" };
            this.XParamItuff = "0^3^1";

            this.YAxisType = DDGShmooTC.AxisType.FIVR;
            this.YParamName = "CCF";
            this.YParamOriginalValue = "-1"; /* TODO: Cannot get default/current value for DLVR. */
            this.VoltageConverterParameter = "--fivrcondition=NOM --railconfigurations=DlvrConfig";

            this.ItuffFormat = DDGShmooTC.ItuffMode.ECADS;

            var underTest = this.BuildBaseTest(true);
            var yVoltage = new Mock<IFivrDomainsAndConditionWithRails>();
            this.YParamVoltageMock = yVoltage.As<IFivrDomains>();
            this.VoltageServiceMock.Setup(o => o.CreateFivrDomainsAndConditionWithRails(new List<string> { "CCF" }, "NOM", "DummyPlist", new List<string> { "DlvrConfig" })).Returns(yVoltage.Object);

            this.VerifyBaseShmoo(ref underTest);
            underTest.CustomVerify();
            Assert.AreEqual(0, underTest.Execute());
            this.VerifyAll(true);
        }

        private DDGShmooTC BuildBaseTest(bool execMode)
        {
            var underTest = new DDGShmooTC
            {
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                InstanceName = this.InstanceName,
                Patlist = this.Plist,
                TimingsTc = this.Timings,
                LevelsTc = this.Levels,
                XAxisParamType = this.XAxisType == DDGShmooTC.AxisType.None ? PrimeShmooTestMethod.ShmooAxisType.None : PrimeShmooTestMethod.ShmooAxisType.UserDefined,
                YAxisParamType = this.YAxisType == DDGShmooTC.AxisType.None ? PrimeShmooTestMethod.ShmooAxisType.None : PrimeShmooTestMethod.ShmooAxisType.UserDefined,
                PowerDownBetweenPoints = this.PowerDownBetweenPoints,
                XAxisDatalogPrefix = this.XAxisType == DDGShmooTC.AxisType.SpecSetVariable || this.XAxisType == DDGShmooTC.AxisType.UserVarTiming ? PrimeShmooTestMethod.UnitPrefixForDatalog.Nano : PrimeShmooTestMethod.UnitPrefixForDatalog.Base,

                XAxisType = this.XAxisType,
                YAxisType = this.YAxisType,

                XAxisParam = this.XParamName,
                XAxisRange = this.XParamRange,

                YAxisParam = this.YParamName,
                YAxisRange = this.YParamRange,

                VoltageConverter = this.VoltageConverterParameter,
                DataLogType = this.ItuffFormat,
            };

            underTest.TestMethodExtension = underTest;
            this.MockFuncTest(execMode);
            this.MockTestConditions(execMode);
            this.MockVoltage(execMode);
            this.MockUserVars(execMode);
            this.MockPatConfig(execMode);

            if (execMode)
            {
                if (this.ItuffFormat == DDGShmooTC.ItuffMode.SHMOO_HUB)
                {
                    this.MockItuffShmooHub();
                }
                else
                {
                    this.MockItuffEcads();
                }
            }

            return underTest;
        }

        private void VerifyBaseShmoo(ref DDGShmooTC underTest)
        {
            object instanceInitDataStructure = null;
            underTest.VerifyOffline(ref instanceInitDataStructure);
            underTest.Verify();
        }

        private void MockFuncTest(bool execMode)
        {
            // Mock the functional test.
            this.FuncTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            if (execMode)
            {
                Assert.AreEqual(20, this.TestPointResults.Count, "Unittest has wrong number of testpoint results");
                this.FuncTestMock.Setup(o => o.SetPinMask(new List<string>()));
                this.FuncTestMock.Setup(o => o.ApplyTestConditions());
                if (this.YAxisType == DDGShmooTC.AxisType.SpecSetVariable || this.YAxisType == DDGShmooTC.AxisType.UserVarLevels)
                {
                    this.FuncTestMock.Setup(o => o.ApplyLevelTestCondition());
                }

                if (this.XAxisType == DDGShmooTC.AxisType.SpecSetVariable || this.XAxisType == DDGShmooTC.AxisType.UserVarTiming)
                {
                    this.FuncTestMock.Setup(o => o.ApplyTimingTestCondition());
                }

                this.FuncTestMock.SetupSequence(o => o.Execute())
                    .Returns(this.TestPointResults[0].Item1)
                    .Returns(this.TestPointResults[1].Item1)
                    .Returns(this.TestPointResults[2].Item1)
                    .Returns(this.TestPointResults[3].Item1)
                    .Returns(this.TestPointResults[4].Item1)
                    .Returns(this.TestPointResults[5].Item1)
                    .Returns(this.TestPointResults[6].Item1)
                    .Returns(this.TestPointResults[7].Item1)
                    .Returns(this.TestPointResults[8].Item1)
                    .Returns(this.TestPointResults[9].Item1)
                    .Returns(this.TestPointResults[10].Item1)
                    .Returns(this.TestPointResults[11].Item1)
                    .Returns(this.TestPointResults[12].Item1)
                    .Returns(this.TestPointResults[13].Item1)
                    .Returns(this.TestPointResults[14].Item1)
                    .Returns(this.TestPointResults[15].Item1)
                    .Returns(this.TestPointResults[16].Item1)
                    .Returns(this.TestPointResults[17].Item1)
                    .Returns(this.TestPointResults[18].Item1)
                    .Returns(this.TestPointResults[19].Item1);

                var failMocks = new List<List<IFailureData>>(20);

                var rgx = new Regex(@"^([\d\w\|]+):(\w+):(\w+)\(([-\d]+),([-\d]+),([-\d]+),([-\d]+)\):([,:\w\s]+)$");
                foreach (var i in Enumerable.Range(0, 20))
                {
                    var failMock = new Mock<IFailureData>(MockBehavior.Strict);
                    var failString = this.TestPointResults[i].Item3;
                    if (string.IsNullOrWhiteSpace(failString))
                    {
                        failMocks.Add(new List<IFailureData>());
                    }
                    else
                    {
                        var m = rgx.Match(failString);
                        Assert.IsTrue(m.Success, $"Failed to decode patternfailure in unittest [{failString}].");

                        var patname = m.Groups[1].Value;
                        ulong patInstanceId = 1;
                        if (patname.Contains('|'))
                        {
                            var l = patname.Split('|');
                            patname = l[0];
                            patInstanceId = (ulong)l[1].ToInt();
                        }

                        failMock.Setup(o => o.IsSubroutine()).Returns(false);
                        failMock.Setup(o => o.GetPatternName()).Returns(patname);
                        failMock.Setup(o => o.GetPatternInstanceId()).Returns(patInstanceId);
                        failMock.Setup(o => o.GetParentPlistName()).Returns(m.Groups[2].Value);
                        failMock.Setup(o => o.GetDomainName()).Returns(m.Groups[3].Value);
                        failMock.Setup(o => o.GetCycle()).Returns((ulong)m.Groups[4].Value.ToInt());
                        failMock.Setup(o => o.GetVectorAddress()).Returns((ulong)m.Groups[5].Value.ToInt());
                        failMock.Setup(o => o.GetFailingPinNames()).Returns(m.Groups[8].Value.Split(',').Select(o => o.Trim()).ToList());

                        failMocks.Add(new List<IFailureData> { failMock.Object });
                    }
                }

                // failInfo = $"<{pattern}>:{domain}({cycle},{mainrma},{subrma},{scanrma}):{failingPinsItuff}";
                // <pat2>:LEG(0,561,-1,-1):IP_CPU::TDO
                this.FuncTestMock.SetupSequence(o => o.GetPerCycleFailures())
                    .Returns(failMocks[0])
                    .Returns(failMocks[1])
                    .Returns(failMocks[2])
                    .Returns(failMocks[3])
                    .Returns(failMocks[4])
                    .Returns(failMocks[5])
                    .Returns(failMocks[6])
                    .Returns(failMocks[7])
                    .Returns(failMocks[8])
                    .Returns(failMocks[9])
                    .Returns(failMocks[10])
                    .Returns(failMocks[11])
                    .Returns(failMocks[12])
                    .Returns(failMocks[13])
                    .Returns(failMocks[14])
                    .Returns(failMocks[15])
                    .Returns(failMocks[16])
                    .Returns(failMocks[17])
                    .Returns(failMocks[18])
                    .Returns(failMocks[19]);
            }

            this.FuncServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            this.FuncServiceMock.Setup(o => o.CreateCaptureFailureTest(this.Plist, this.Levels, this.Timings, 1, string.Empty)).Returns(this.FuncTestMock.Object);
            Prime.Services.FunctionalService = this.FuncServiceMock.Object;
        }

        private void MockItuffShmooHub()
        {
            this.ItuffStrgvalMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            this.ItuffStrgvalMock.Setup(o => o.SetTnamePostfix("_SSTP"));

            if (this.YAxisType == DDGShmooTC.AxisType.None)
            {
                this.ItuffStrgvalMock.Setup(o => o.SetData($"X"));
            }
            else
            {
                this.ItuffStrgvalMock.Setup(o => o.SetData($"X_Y"));
            }

            /*
            if (this.XAxisType == DDGShmooTC.AxisType.SpecSetVariable && this.YAxisType == DDGShmooTC.AxisType.SpecSetVariable)
            {
                this.ItuffStrgvalMock.Setup(o => o.SetData($"X_{this.XParamOriginalValue}nS" + $"_Y_{this.YParamOriginalValue}V"));
            }
            else if (this.XAxisType == DDGShmooTC.AxisType.SpecSetVariable)
            {
                this.ItuffStrgvalMock.Setup(o => o.SetData($"X_{this.XParamOriginalValue}nS"));
            }
            else if (this.YAxisType == DDGShmooTC.AxisType.SpecSetVariable)
            {
                this.ItuffStrgvalMock.Setup(o => o.SetData($"Y_{this.YParamOriginalValue}V"));
            } */

            var text = "^" + this.XParamName + "^" + this.XParamItuff;
            if (this.YParamTestPoints.Count > 0)
            {
                text += "^" + this.YParamName + "^" + this.YParamItuff;
            }

            text += "_ShmooHub";

            this.ItuffStrgvalMock.Setup(o => o.SetTnamePostfix(text));
            Console.WriteLine($"Expecting Ituff=[{text}].");
            this.ItuffStrgvalMock.Setup(o => o.SetData(this.ItuffShmooHubFormat));

            foreach (var fail in this.TestPointResults)
            {
                if (!string.IsNullOrWhiteSpace(fail.Item3))
                {
                    this.ItuffStrgvalMock.Setup(o => o.SetTnamePostfix($"^LEGEND^{fail.Item2}"));
                    this.ItuffStrgvalMock.Setup(o => o.SetData(fail.Item3));
                }
            }

            this.DatalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            this.DatalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.ItuffStrgvalMock.Object);
            this.DatalogServiceMock.Setup(o => o.WriteToItuff(this.ItuffStrgvalMock.Object));
            Prime.Services.DatalogService = this.DatalogServiceMock.Object;
        }

        private void MockItuffEcads()
        {
            this.MockItuffShmooHub();
            this.ItuffComntMock = new Mock<IComntFormat>(MockBehavior.Strict);
            this.ItuffComntMock.Setup(o => o.ClearData());
            this.ItuffComntMock.Setup(o => o.IncludeTnameInPrint(false));

            var sequence = new MockSequence();
            this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"Plot3Start_{this.InstanceName}"));
            this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"PLOT_PXName,{this.XParamName}"));
            this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"PLOT_PYName,{this.YParamName}"));
            this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"PLOT_PXStart,{this.XParamItuff.Split('^')[0]}"));
            this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"PLOT_PYStart,{this.YParamItuff.Split('^')[0]}"));
            this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"PLOT_PXStop,{this.XParamItuff.Split('^')[1]}"));
            this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"PLOT_PYStop,{this.YParamItuff.Split('^')[1]}"));
            /* this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"PLOT_PXStep,{this.XParamItuff.Split('^')[2]}")); */
            var xSteps = this.ItuffShmooHubFormat.IndexOf('_');
            var ySteps = this.ItuffShmooHubFormat.Count(f => f == '_') + 1;
            this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"PLOT_PXStep,{xSteps}"));
            /* this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"PLOT_PYStep,{this.YParamItuff.Split('^')[2]}")); */
            this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"PLOT_PYStep,{ySteps}"));
            this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"PLOT_PXValue,{this.XParamOriginalValue}"));
            this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"PLOT_PYValue,{this.YParamOriginalValue}"));

            foreach (var line in this.ItuffShmooHubFormat.Split('_'))
            {
                this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"P3Data_{line}"));
            }

            var alreadyPrinted = new List<string>();
            foreach (var fail in this.TestPointResults)
            {
                if (!string.IsNullOrWhiteSpace(fail.Item3) && !alreadyPrinted.Contains(fail.Item2))
                {
                    this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"P3Legend_{fail.Item2}_{fail.Item3}"));
                    alreadyPrinted.Add(fail.Item2);
                }
            }

            this.ItuffComntMock.InSequence(sequence).Setup(o => o.AddData($"Plot3End_{this.InstanceName}"));
            this.DatalogServiceMock.Setup(o => o.GetItuffComntWriter()).Returns(this.ItuffComntMock.Object);
            this.DatalogServiceMock.Setup(o => o.WriteToItuff(this.ItuffComntMock.Object));
        }

        private void MockUserVars(bool execMode)
        {
            this.UserVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            if (this.XAxisType == DDGShmooTC.AxisType.UserVarLevels || this.XAxisType == DDGShmooTC.AxisType.UserVarTiming)
            {
                this.UserVarServiceMock.Setup(o => o.GetDoubleValue(this.XParamName)).Returns(Convert.ToDouble(this.XParamOriginalValue));
                this.TestConditionServiceMock.Setup(o => o.IsSmartTcEnabled()).Returns(true);
            }

            if (this.YAxisType == DDGShmooTC.AxisType.UserVarLevels || this.YAxisType == DDGShmooTC.AxisType.UserVarTiming)
            {
                this.UserVarServiceMock.Setup(o => o.GetDoubleValue(this.YParamName)).Returns(Convert.ToDouble(this.YParamOriginalValue));
                this.TestConditionServiceMock.Setup(o => o.IsSmartTcEnabled()).Returns(true);
            }

            if (this.XAxisType == DDGShmooTC.AxisType.UserVarLevels || this.YAxisType == DDGShmooTC.AxisType.UserVarLevels)
            {
                this.TestConditionServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.LEVELS_SETUP));
            }

            if (this.XAxisType == DDGShmooTC.AxisType.UserVarTiming || this.YAxisType == DDGShmooTC.AxisType.UserVarTiming)
            {
                this.TestConditionServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.TIMING));
            }

            if (execMode)
            {
                if (this.YAxisType == DDGShmooTC.AxisType.UserVarLevels)
                {
                    foreach (var value in this.YParamTestPoints)
                    {
                        this.UserVarServiceMock.Setup(o => o.SetValue(this.YParamName, Math.Round(Convert.ToDouble(value), 4)));
                    }

                    this.LevelsTcMock.Setup(o => o.Resolve());
                    this.UserVarServiceMock.Setup(o => o.SetValue(this.YParamName, Convert.ToDouble(this.YParamOriginalValue)));
                }

                if (this.XAxisType == DDGShmooTC.AxisType.UserVarTiming)
                {
                    foreach (var value in this.XParamTestPoints)
                    {
                        this.UserVarServiceMock.Setup(o => o.SetValue(this.XParamName, Math.Round(Convert.ToDouble(value), 12)));
                    }

                    this.TimingTcMock.Setup(o => o.Resolve());
                    this.UserVarServiceMock.Setup(o => o.SetValue(this.XParamName, Convert.ToDouble(this.XParamOriginalValue)));
                }
            }

            Prime.Services.UserVarService = this.UserVarServiceMock.Object;
        }

        private void MockTestConditions(bool execMode)
        {
            this.LevelsTcMock = new Mock<ITestCondition>(MockBehavior.Strict);
            this.TimingTcMock = new Mock<ITestCondition>(MockBehavior.Strict);

            if (!string.IsNullOrWhiteSpace(this.YParamName))
            {
                if (this.YAxisType == DDGShmooTC.AxisType.SpecSetVariable)
                {
                    this.LevelsTcMock.Setup(o => o.GetSpecSetValue(this.YParamName)).Returns(this.YParamOriginalValue);
                }
            }

            if (!string.IsNullOrWhiteSpace(this.XParamName))
            {
                if (this.XAxisType == DDGShmooTC.AxisType.SpecSetVariable)
                {
                    this.TimingTcMock.Setup(o => o.GetSpecSetValue(this.XParamName)).Returns(this.XParamOriginalValue);
                    this.LevelsTcMock.Setup(o => o.GetSpecSetValue(this.XParamName)).Throws(new Prime.Base.Exceptions.FatalException("not a levels parameter"));
                }
            }

            if (execMode)
            {
                if (this.YAxisType == DDGShmooTC.AxisType.SpecSetVariable)
                {
                    this.LevelsTcMock.Setup(o => o.SetSpecSetValue(this.YParamName, this.YParamOriginalValue));
                    foreach (var value in this.YParamTestPoints)
                    {
                        this.LevelsTcMock.Setup(o => o.SetSpecSetValue(this.YParamName, value));
                    }
                }

                if (this.XAxisType == DDGShmooTC.AxisType.SpecSetVariable)
                {
                    this.TimingTcMock.Setup(o => o.SetSpecSetValue(this.XParamName, this.XParamOriginalValue));
                    foreach (var value in this.XParamTestPoints)
                    {
                        this.TimingTcMock.Setup(o => o.SetSpecSetValue(this.XParamName, value));
                    }
                }
            }

            this.TestConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            if (this.XAxisType == DDGShmooTC.AxisType.SpecSetVariable || this.YAxisType == DDGShmooTC.AxisType.SpecSetVariable)
            {
                this.TestConditionServiceMock.Setup(o => o.GetTestCondition(this.Levels)).Returns(this.LevelsTcMock.Object);
                this.TestConditionServiceMock.Setup(o => o.GetTestCondition(this.Timings)).Returns(this.TimingTcMock.Object);
            }

            if (this.XAxisType == DDGShmooTC.AxisType.UserVarLevels || this.YAxisType == DDGShmooTC.AxisType.UserVarLevels)
            {
                this.TestConditionServiceMock.Setup(o => o.GetTestCondition(this.Levels)).Returns(this.LevelsTcMock.Object);
            }

            if (this.XAxisType == DDGShmooTC.AxisType.UserVarTiming || this.YAxisType == DDGShmooTC.AxisType.UserVarTiming)
            {
                this.TestConditionServiceMock.Setup(o => o.GetTestCondition(this.Timings)).Returns(this.TimingTcMock.Object);
            }

            Prime.Services.TestConditionService = this.TestConditionServiceMock.Object;
        }

        private void MockVoltage(bool execMode)
        {
            // FIVR mode stuff
            this.VoltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            this.XParamVoltageMock = new Mock<IFivrDomains>(MockBehavior.Strict);
            this.YParamVoltageMock = new Mock<IFivrDomains>(MockBehavior.Strict);

            // VoltageConverter stuff for FIVR/DLVR
            if (this.YAxisType == DDGShmooTC.AxisType.FIVR)
            {
                if (execMode)
                {
                    this.YParamVoltageMock.Setup(o => o.Restore());
                    foreach (var value in this.YParamTestPoints)
                    {
                        if (this.YAxisType == DDGShmooTC.AxisType.FIVR)
                        {
                            this.YParamVoltageMock.Setup(o => o.Apply(new List<double> { Math.Round(Convert.ToDouble(value), 4) }));
                        }
                    }
                }
            }

            if (this.XAxisType == DDGShmooTC.AxisType.FIVR)
            {
                this.VoltageServiceMock.Setup(o => o.CreateFivrForDomains(new List<string> { this.XParamName }, this.Plist)).Returns(this.XParamVoltageMock.Object);

                if (execMode)
                {
                    this.XParamVoltageMock.Setup(o => o.Restore());
                    foreach (var value in this.XParamTestPoints)
                    {
                        if (this.XAxisType == DDGShmooTC.AxisType.FIVR)
                        {
                            this.XParamVoltageMock.Setup(o => o.Apply(new List<double> { Math.Round(Convert.ToDouble(value), 4) }));
                        }
                    }
                }
            }

            Prime.Services.VoltageService = this.VoltageServiceMock.Object;
        }

        private void MockPatConfig(bool execMode)
        {
            this.PatConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            this.PatConfigMocks = new List<Mock<IPatConfigHandle>>();
            this.PatSetPointMocks = new List<Mock<IPatConfigSetPointHandle>>();

            if (this.XAxisType == DDGShmooTC.AxisType.PatConfig)
            {
                var allValues = this.XParamName.Split(',');
                foreach (var configIndex in Enumerable.Range(0, allValues.Length))
                {
                    this.PatConfigMocks.Add(new Mock<IPatConfigHandle>(MockBehavior.Strict));
                    if (execMode)
                    {
                        foreach (var value in this.XParamTestPoints)
                        {
                            this.PatConfigMocks[configIndex].Setup(o => o.SetData(value));
                            this.PatConfigMocks[configIndex].Setup(o => o.GetExpectedDataSize()).Returns((ulong)value.Length);
                        }
                    }

                    this.PatConfigServiceMock.Setup(o => o.GetPatConfigHandleWithPlist(allValues[configIndex], this.Plist)).Returns(this.PatConfigMocks[configIndex].Object);
                }

                if (execMode)
                {
                    // this.PatConfigServiceMock.Setup(o => o.Apply(this.PatConfigMocks.Select(p => p.Object).ToList()));
                    this.PatConfigServiceMock.Setup(o => o.Apply(It.IsAny<List<IPatConfigHandle>>()));
                }
            }
            else if (this.XAxisType == DDGShmooTC.AxisType.PatConfigSetPoint)
            {
                var allValues = this.XParamName.Split(',');
                foreach (var configIndex in Enumerable.Range(0, allValues.Length))
                {
                    this.PatSetPointMocks.Add(new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict));
                    if (execMode)
                    {
                        foreach (var value in this.XParamTestPoints)
                        {
                            this.PatSetPointMocks[configIndex].Setup(o => o.ApplySetPoint(value));
                        }
                    }

                    var moduleGroupPair = allValues[configIndex].Split(':');
                    this.PatConfigServiceMock.Setup(o => o.GetSetPointHandle(moduleGroupPair[0], moduleGroupPair[1], this.Plist)).Returns(this.PatSetPointMocks[configIndex].Object);
                }
            }

            Prime.Services.PatConfigService = this.PatConfigServiceMock.Object;
        }

        private void VerifyAll(bool execMode, int executions = 20)
        {
            this.FuncTestMock.VerifyAll();
            this.FuncServiceMock.VerifyAll();
            this.LevelsTcMock.VerifyAll();
            this.TimingTcMock.VerifyAll();
            this.XParamVoltageMock.VerifyAll();
            this.YParamVoltageMock.VerifyAll();
            this.UserVarServiceMock.VerifyAll();
            this.PatConfigServiceMock.VerifyAll();
            this.PatConfigMocks.ForEach(o => o.VerifyAll());
            this.PatSetPointMocks.ForEach(o => o.VerifyAll());
            this.TestConditionServiceMock.VerifyAll();

            if (execMode)
            {
                this.DatalogServiceMock.VerifyAll();
                this.ItuffStrgvalMock?.VerifyAll();
                this.ItuffComntMock?.VerifyAll();
                this.FuncTestMock.Verify(o => o.Execute(), Times.Exactly(executions));
                this.FuncTestMock.Verify(o => o.GetPerCycleFailures(), Times.Exactly(executions));
            }
        }
    }
}
