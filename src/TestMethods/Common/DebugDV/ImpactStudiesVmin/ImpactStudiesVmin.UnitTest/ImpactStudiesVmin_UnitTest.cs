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

namespace ImpactStudiesVmin.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PlistService;
    using Prime.SharedStorageService;
    using Prime.TestMethods.VminSearch;
    using Prime.TestProgramService;
    using Prime.VoltageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class ImpactStudiesVmin_UnitTest
    {
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
        public void VminFactory_Base_Pass()
        {
            var factory = new VminFactory();
            var underTest = factory.CreateInstance();
            Assert.IsNotNull(underTest);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamEmpty_Throw()
        {
            ImpactStudiesVmin underTest = new ImpactStudiesVmin { };
            var ex = Assert.ThrowsException<TestMethodException>(() => underTest.Verify());
            Assert.AreEqual("Failed verify on ConfigurationFile=[].", ex.Message);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ConfigFileMissing_Fail()
        {
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.FileExists("VminConfig.json")).Returns(false);
            Prime.Services.FileService = fileServiceMock.Object;

            ImpactStudiesVmin underTest = new ImpactStudiesVmin
            {
                ConfigurationFile = "VminConfig.json",
            };

            var ex = Assert.ThrowsException<System.IO.FileNotFoundException>(() => underTest.Verify());
            Assert.AreEqual("File=[VminConfig.json] is not found.", ex.Message);
            fileServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ConfigFileNoParameterSection_Fail()
        {
            this.MockConfigFile("VminConfig.json", "{'Tests':[{'Name':'Test1', 'Patlist':'plist1'}]}", out var fileServiceMock, out var fileSystemMock);

            ImpactStudiesVmin underTest = new ImpactStudiesVmin
            {
                ConfigurationFile = "VminConfig.json",
                FileWrapper = fileSystemMock.Object,
            };

            var ex = Assert.ThrowsException<TestMethodException>(() => underTest.Verify());
            Assert.AreEqual("Failed verify on ConfigurationFile=[VminConfig.json].", ex.Message);
            this.ConsoleServiceMock.Verify(o => o.PrintError("Configuration file is missing the [VminParameters] section.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            fileServiceMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ConfigFileNoStartingVoltage_Fail()
        {
            this.MockConfigFile("VminConfig.json", "{'VminParameters':{},'Tests':[{'Name':'Test1', 'Patlist':'plist1'}]}", out var fileServiceMock, out var fileSystemMock);

            ImpactStudiesVmin underTest = new ImpactStudiesVmin
            {
                ConfigurationFile = "VminConfig.json",
                FileWrapper = fileSystemMock.Object,
            };

            var ex = Assert.ThrowsException<TestMethodException>(() => underTest.Verify());
            Assert.AreEqual("Failed verify on ConfigurationFile=[VminConfig.json].", ex.Message);
            this.ConsoleServiceMock.Verify(o => o.PrintError("Configuration file is missing Parameter=[StartVoltages].", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            fileServiceMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ConfigFileInvalidParam_Fail()
        {
            var configData = "{'VminParameters':{'StartVoltages':'1.1','NotAValidVminParam':'NA'},'Tests':[{'Name':'Test1', 'Patlist':'plist1'}]}";
            this.MockConfigFile("VminConfig.json", configData, out var fileServiceMock, out var fileSystemMock);

            ImpactStudiesVmin underTest = new ImpactStudiesVmin
            {
                InstanceName = "DummyInstance",
                ConfigurationFile = "VminConfig.json",
                FileWrapper = fileSystemMock.Object,
            };

            var ex = Assert.ThrowsException<TestMethodException>(() => underTest.Verify());
            Assert.AreEqual("Failed verify on ConfigurationFile=[VminConfig.json].", ex.Message);
            this.ConsoleServiceMock.Verify(o => o.PrintError("VminTC does not contain parameter=[NotAValidVminParam].", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            fileServiceMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ConfigFileInvalidOverride_Fail()
        {
            var configData = "{'VminParameters':{'StartVoltages':'1.1'},'Tests':[{'Name':'Test1', 'Patlist':'plist1', 'Overrides':{'NotAValidOverride':'Value'}}]}";
            this.MockConfigFile("VminConfig.json", configData, out var fileServiceMock, out var fileSystemMock);

            ImpactStudiesVmin underTest = new ImpactStudiesVmin
            {
                InstanceName = "DummyInstance",
                ConfigurationFile = "VminConfig.json",
                FileWrapper = fileSystemMock.Object,
            };

            var ex = Assert.ThrowsException<TestMethodException>(() => underTest.Verify());
            Assert.AreEqual("Failed verify on ConfigurationFile=[VminConfig.json].", ex.Message);
            this.ConsoleServiceMock.Verify(o => o.PrintError("VminTC does not contain parameter=[NotAValidOverride].", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            fileServiceMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ConfigFileNoTest_Fail()
        {
            var configData = "{'VminParameters':{'StartVoltages':'1.1'},'Tests':[]}";
            this.MockConfigFile("VminConfig.json", configData, out var fileServiceMock, out var fileSystemMock);

            ImpactStudiesVmin underTest = new ImpactStudiesVmin
            {
                InstanceName = "DummyInstance",
                ConfigurationFile = "VminConfig.json",
                FileWrapper = fileSystemMock.Object,
            };

            var ex = Assert.ThrowsException<TestMethodException>(() => underTest.Verify());
            Assert.AreEqual("Failed verify on ConfigurationFile=[VminConfig.json].", ex.Message);
            this.ConsoleServiceMock.Verify(o => o.PrintError("Configuration file must contain at least one plist under the [Tests] key.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            fileServiceMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ConfigFile_Pass()
        {
            var configData = "{'VminParameters':{'StartVoltages':'1.1', 'LevelsTc':'myLevels', 'RecoveryMode':'Default', 'StepSize':'0.01'}," +
                "'Tests':[{'Name':'Test1', 'Patlist':'plist1', 'Overrides':{'ScoreboardBaseNumber':'8000'} }]}";
            this.MockConfigFile("VminConfig.json", configData, out var fileServiceMock, out var fileSystemMock);

            // Mock the VminSearch object, no interface.
            var vminSearch = new Mock<VminSearch>(MockBehavior.Strict);
            vminSearch.Setup(o => o.CustomVerify());
            vminSearch.Setup(o => o.VerifyWrapper());

            // Mock the IVminFactory to return our mocked VminSearch.
            var vminSearchFactory = new Mock<IVminFactory>(MockBehavior.Strict);
            vminSearchFactory.Setup(o => o.CreateInstance()).Returns(vminSearch.Object);

            ImpactStudiesVmin underTest = new ImpactStudiesVmin
            {
                InstanceName = "DummyInstance",
                ConfigurationFile = "VminConfig.json",
                FileWrapper = fileSystemMock.Object,
                VminSearchFactory = vminSearchFactory.Object,
            };

            underTest.Verify();
            fileServiceMock.VerifyAll();
            fileSystemMock.VerifyAll();
            vminSearch.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_MockSearch_Pass()
        {
            /* setup mocks for the stuff in verify */
            var testprogramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramServiceMock.Setup(o => o.TriggerCallback("Callback1", "Args1, Args2, Args3")).Returns(string.Empty);
            Prime.Services.TestProgramService = testprogramServiceMock.Object;

            var patSetPointMock1 = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            patSetPointMock1.Setup(o => o.ApplySetPointDefault());
            var patSetPointMock2 = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            patSetPointMock2.Setup(o => o.ApplySetPoint("SomeTestPoint"));

            var configData = "{'VminParameters':{'StartVoltages':'1.1,1.1', 'LevelsTc':'myLevels', 'RecoveryMode':'Default', 'StepSize':'0.01' }" +
                ",'Tests':[{'Name':'Test1', 'Patlist':'plist1','Overrides':{'ScoreboardBaseNumber':'0001'}}, {'Name':'Test2', 'Patlist':'plist2'}, {'Name':'Test3', 'Patlist':'plist3'}]}";
            this.MockConfigFile("VminConfig.json", configData, out var fileServiceMock, out var fileSystemMock);

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            /* Start voltage = 1.1 */
            sharedStorageMock.Setup(o => o.InsertRowAtTable("DummyInstance_Vmin0", 1.1, Context.DUT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("DummyInstance_Vmin1", 1.1, Context.DUT));

            /* Results of Test1: 1.500_-9999 (scoreboard offset = 2 * 0.01*/
            /* Results of Test2: 1.490_-9999 (scoreboard offset = 2 * 0.01*/
            /* Results of Test3: 1.700_1.700 (scoreboard offset = 2 * 0.01*/
            sharedStorageMock.SetupSequence(o => o.GetDoubleRowFromTable("DummyInstance_Vmin0", Context.DUT))
                .Returns(1.5)
                .Returns(1.490)
                .Returns(1.7);
            sharedStorageMock.SetupSequence(o => o.GetDoubleRowFromTable("DummyInstance_Vmin1", Context.DUT))
                .Returns(-9999)
                .Returns(-9999)
                .Returns(1.7);

            var s0Insert = new MockSequence();
            sharedStorageMock.InSequence(s0Insert).Setup(o => o.InsertRowAtTable("DummyInstance_Vmin0", 1.48, Context.DUT));
            sharedStorageMock.InSequence(s0Insert).Setup(o => o.InsertRowAtTable("DummyInstance_Vmin0", 1.48, Context.DUT));
            sharedStorageMock.InSequence(s0Insert).Setup(o => o.InsertRowAtTable("DummyInstance_Vmin0", 1.68, Context.DUT));

            var s1Insert = new MockSequence();
            sharedStorageMock.InSequence(s1Insert).Setup(o => o.InsertRowAtTable("DummyInstance_Vmin1", 1.1, Context.DUT));
            sharedStorageMock.InSequence(s1Insert).Setup(o => o.InsertRowAtTable("DummyInstance_Vmin1", 1.1, Context.DUT));
            sharedStorageMock.InSequence(s1Insert).Setup(o => o.InsertRowAtTable("DummyInstance_Vmin1", 1.68, Context.DUT));

            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // Mock the VminSearch object, no interface.
            var vminSearch = new Mock<VminSearch>(MockBehavior.Strict);
            vminSearch.Setup(o => o.CustomVerify());
            vminSearch.Setup(o => o.VerifyWrapper());
            vminSearch.Setup(o => o.Execute()).Returns(1);

            vminSearch.SetupGet(o => o.PreConfigSetPointsWithData).
                Returns(new List<Tuple<IPatConfigSetPointHandle, string>> { new Tuple<IPatConfigSetPointHandle, string>(patSetPointMock2.Object, "SomeTestPoint") });
            vminSearch.SetupGet(o => o.PreConfigSetPointsWithDefault).Returns(new List<IPatConfigSetPointHandle>());
            vminSearch.SetupGet(o => o.PostConfigSetPointsWithData).Returns(new List<Tuple<IPatConfigSetPointHandle, string>>());
            vminSearch.SetupGet(o => o.PostConfigSetPointsWithDefault).Returns(new List<IPatConfigSetPointHandle> { patSetPointMock1.Object });
            vminSearch.SetupGet(o => o.PreInstanceCallback).Returns((Tuple<string, string>)null);
            vminSearch.SetupGet(o => o.PostInstanceCallback).Returns(new Tuple<string, string>("Callback1", "Args1, Args2, Args3"));

            // Mock the IVminFactory to return our mocked VminSearch.
            var vminSearchFactory = new Mock<IVminFactory>(MockBehavior.Strict);
            vminSearchFactory.Setup(o => o.CreateInstance()).Returns(vminSearch.Object);

            ImpactStudiesVmin underTest = new ImpactStudiesVmin
            {
                InstanceName = "DummyInstance",
                ConfigurationFile = "VminConfig.json",
                VminForwardOffset = 0.02,
                FileWrapper = fileSystemMock.Object,
                VminSearchFactory = vminSearchFactory.Object,
            };

            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            fileServiceMock.VerifyAll();
            fileSystemMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            vminSearch.VerifyAll();
            testprogramServiceMock.VerifyAll();
            patSetPointMock1.VerifyAll();
            patSetPointMock2.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ChildCustomVerify_SetPointsWrongFormat_Exception()
        {
            var vminSearch = new VminSearch();
            vminSearch.SetPointsPreInstance = "SomeValue";
            var ex = Assert.ThrowsException<TestMethodException>(() => vminSearch.ChildCustomVerify(null));
            Assert.AreEqual("Expecting PatConfigSetpoint to be of the form [Module:Group] or [Module:Group:Setpoint], not [SomeValue].", ex.Message);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ChildCustomVerify_SetPointsWrongData_Exception()
        {
            var patSetPointMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);

            var patConfigService = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigService.Setup(o => o.GetSetPointHandle("SomeModule", "SomeGroup", "plist1")).Returns(patSetPointMock.Object);
            patConfigService.Setup(o => o.IsSetPointHandleExist("SomeModule", "SomeGroup", "BadData")).Returns(SetPointHandleCheckerSymbol.SETPOINT_DOESNT_EXIST);
            Prime.Services.PatConfigService = patConfigService.Object;

            var vminSearch = new VminSearch();
            vminSearch.SetPointsPostInstance = "SomeModule:SomeGroup:BadData";
            vminSearch.Patlist = "plist1";

            var ex = Assert.ThrowsException<TestMethodException>(() => vminSearch.ChildCustomVerify(null));
            Assert.AreEqual("PatConfigSetPoint Module=[SomeModule] Group=[SomeGroup] Does not have SetPoint=[BadData].", ex.Message);
            patConfigService.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ChildCustomVerify_CallbackWrongFormat_Exception()
        {
            var vminSearch = new VminSearch();
            vminSearch.PreInstance = "CallbackWrongFormat";

            var ex = Assert.ThrowsException<TestMethodException>(() => vminSearch.ChildCustomVerify(null));
            Assert.AreEqual("Expecting callback to be of the form [func(args)], not [CallbackWrongFormat].", ex.Message);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ChildCustomVerify_CallbacDoesNotExist_Exception()
        {
            var testprogramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramServiceMock.Setup(o => o.DoesCallbackExist("CallbackDNE")).Returns(false);
            Prime.Services.TestProgramService = testprogramServiceMock.Object;

            var vminSearch = new VminSearch();
            vminSearch.PostInstance = "CallbackDNE(args1, args2, args3)";

            var ex = Assert.ThrowsException<TestMethodException>(() => vminSearch.ChildCustomVerify(null));
            Assert.AreEqual("Callback=[CallbackDNE] has not been properly registered.", ex.Message);
            testprogramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ChildCustomVerify_Pass()
        {
            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureTest("plist1", "Levels", "Timings", 99999, 1000, "PrePlistCallback")).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("plist1")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            var testprogramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramServiceMock.Setup(o => o.DoesCallbackExist("callback1")).Returns(true);
            testprogramServiceMock.Setup(o => o.DoesCallbackExist("callback2")).Returns(true);
            Prime.Services.TestProgramService = testprogramServiceMock.Object;

            var patSetPointMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            var patConfigService = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigService.Setup(o => o.GetSetPointHandleWithRegEx("SomeModule", "SomeGroup", "plist1", ".*")).Returns(patSetPointMock.Object);
            patConfigService.Setup(o => o.IsSetPointHandleExist("SomeModule", "SomeGroup", "SomeTestPoint")).Returns(SetPointHandleCheckerSymbol.EXIST);
            Prime.Services.PatConfigService = patConfigService.Object;

            var decoder1Mock = new Mock<IPinMapDecoder>(MockBehavior.Strict);
            decoder1Mock.SetupGet(o => o.NumberOfTrackerElements).Returns(1);
            decoder1Mock.SetupGet(o => o.Name).Returns("CORE1");

            var decoder2Mock = new Mock<IPinMapDecoder>(MockBehavior.Strict);
            decoder2Mock.SetupGet(o => o.NumberOfTrackerElements).Returns(2);
            decoder2Mock.SetupGet(o => o.Name).Returns("CORE2");

            var pinMapMock = new Mock<IPinMap>(MockBehavior.Strict);
            pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { decoder1Mock.Object, decoder2Mock.Object });
            pinMapMock.Setup(o => o.GetMaskPins(It.Is<BitArray>(it => it.ToBinaryString() == "100"), null)).Returns(new List<string> { "PinA", "PinB" });
            pinMapMock.Setup(o => o.GetMaskPins(It.Is<BitArray>(it => it.ToBinaryString() == "010"), null)).Returns(new List<string> { "PinC" });
            pinMapMock.Setup(o => o.GetMaskPins(It.Is<BitArray>(it => it.ToBinaryString() == "001"), null)).Returns(new List<string> { "PinD" });

            var vminSearch = new VminSearch
            {
                SetPointsPreInstance = "SomeModule:SomeGroup:SomeTestPoint",
                SetPointsPostInstance = "SomeModule:SomeGroup",
                Patlist = "plist1",
                LevelsTc = "Levels",
                TimingsTc = "Timings",
                PrePlist = "PrePlistCallback",
                SetPointsRegEx = ".*",
                PreInstance = "callback1()",
                PostInstance = "callback2(args1, args2, args3)",
            };

            vminSearch.ChildCustomVerify(pinMapMock.Object);
            Assert.AreEqual(4, vminSearch.PinToCoreMap.Count);
            Assert.IsTrue(vminSearch.PinToCoreMap.ContainsKey("PinA"), "No PinA in map");
            Assert.IsTrue(vminSearch.PinToCoreMap.ContainsKey("PinB"), "No PinB in map");
            Assert.IsTrue(vminSearch.PinToCoreMap.ContainsKey("PinC"), "No PinC in map");
            Assert.IsTrue(vminSearch.PinToCoreMap.ContainsKey("PinD"), "No PinD in map");
            Assert.AreEqual("CORE1", vminSearch.PinToCoreMap["PinA"]);
            Assert.AreEqual("CORE1", vminSearch.PinToCoreMap["PinB"]);
            Assert.AreEqual("CORE2", vminSearch.PinToCoreMap["PinC"]);
            Assert.AreEqual("CORE2", vminSearch.PinToCoreMap["PinD"]);
            Assert.AreEqual("callback1", vminSearch.PreInstanceCallback.Item1);
            Assert.AreEqual(string.Empty, vminSearch.PreInstanceCallback.Item2);
            Assert.AreEqual("callback2", vminSearch.PostInstanceCallback.Item1);
            Assert.AreEqual("args1, args2, args3", vminSearch.PostInstanceCallback.Item2);

            pinMapMock.VerifyAll();
            decoder1Mock.VerifyAll();
            decoder2Mock.VerifyAll();
            patConfigService.VerifyAll();
            testprogramServiceMock.VerifyAll();
            plistServiceMock.VerifyAll();
            funcServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ExecuteScoreboard_NotEnoughTestPoints_Fail()
        {
            var extensionMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            extensionMock.Setup(o => o.GetStartVoltageValues(new List<string> { "0.5" })).Returns(new List<double> { 0.5 });
            extensionMock.Setup(o => o.GetEndVoltageLimitValues(new List<string> { "1.5" })).Returns(new List<double> { 1.5 });

            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance"));
            writerMock.Setup(o => o.SetData("0.500|0.500|1.500|1"));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var pointData1 = new SearchPointData(new List<double> { 0.5 }, new SearchPointData.PatternData("pat1", 0, 0));
            var underTest = new VminSearch();
            underTest.InstanceName = "MyInstance";
            underTest.TestMethodExtension = extensionMock.Object;
            underTest.StartVoltages = "0.5";
            underTest.EndVoltageLimits = "1.5";

            Assert.IsFalse(underTest.ExecuteCustomScoreboard(new List<SearchPointData> { pointData1 }, new Dictionary<string, string>()));
            datalogServiceMock.VerifyAll();
            writerMock.VerifyAll();
            extensionMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ExecuteScoreboard_AmbleScoreboardPasses_Fail()
        {
            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.Execute()).Returns(true);

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureTest("Plist1", "Levels", "Timings", 99999, 1000, "PrePlistCallback")).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("pat2")).Returns(true);

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("Plist1")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance"));
            writerMock.Setup(o => o.SetData("1.100|0.500|1.500|4"));

            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance|scb"));
            writerMock.Setup(o => o.SetData("0.900|pat2|0|0|0"));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var voltageMock = new Mock<IVoltage>(MockBehavior.Strict);
            var extensionMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            extensionMock.Setup(o => o.GetSearchVoltageObject(new List<string> { "CORE0" }, "Plist1")).Returns(voltageMock.Object);
            extensionMock.Setup(o => o.GetStartVoltageValues(new List<string> { "0.5" })).Returns(new List<double> { 0.5 });
            extensionMock.Setup(o => o.GetEndVoltageLimitValues(new List<string> { "1.5" })).Returns(new List<double> { 1.5 });
            extensionMock.Setup(o => o.ApplySearchVoltage(voltageMock.Object, It.Is<List<double>>(it => Math.Abs(it[0] - 0.9) < 0.01)));
            extensionMock.Setup(o => o.ApplyMask(null, funcTestMock.Object)); // Can't write this.InitialMask_ so it'll be null

            var pointData1 = new SearchPointData(new List<double> { 0.8 }, new SearchPointData.PatternData("pat1", 0, 0));
            var pointData2 = new SearchPointData(new List<double> { 0.9 }, new SearchPointData.PatternData("pat2", 0, 0));
            var pointData3 = new SearchPointData(new List<double> { 1.0 }, new SearchPointData.PatternData("pat3", 0, 0));
            var pointData4 = new SearchPointData(new List<double> { 1.1 }, new SearchPointData.PatternData("pat4", 0, 0));

            var pinMapMock = new Mock<IPinMap>(MockBehavior.Strict);
            pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { });

            var underTest = new VminSearch();
            underTest.TestMethodExtension = extensionMock.Object;
            underTest.InstanceName = "MyInstance";
            underTest.VoltageTargets = "CORE0";
            underTest.Patlist = "Plist1";
            underTest.LevelsTc = "Levels";
            underTest.TimingsTc = "Timings";
            underTest.PrePlist = "PrePlistCallback";
            underTest.StartVoltages = "0.5";
            underTest.EndVoltageLimits = "1.5";
            underTest.StepSize = 0.1;
            underTest.ScoreboardEdgeTicks = 2;

            underTest.ChildCustomVerify(pinMapMock.Object);
            Assert.IsTrue(underTest.ExecuteCustomScoreboard(new List<SearchPointData> { pointData1, pointData2, pointData3, pointData4 }, new Dictionary<string, string>()));

            extensionMock.VerifyAll();
            funcServiceMock.VerifyAll();
            funcTestMock.VerifyAll();
            plistMock.VerifyAll();
            plistServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ExecuteScoreboard_NonAmbleScoreboardGood_Pass()
        {
            var fail1 = new Mock<IFailureData>(MockBehavior.Strict);
            fail1.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "Pin1", "Pin2", "NotInPinMap" });
            fail1.Setup(o => o.GetPatternName()).Returns("d0000001");

            var fail2 = new Mock<IFailureData>(MockBehavior.Strict);
            fail2.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "Pin1" });
            fail2.Setup(o => o.GetPatternName()).Returns("d0000002");

            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.SetStartPattern("pat2", 0, 0));
            funcTestMock.Setup(o => o.Execute()).Returns(false);
            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fail1.Object, fail2.Object });

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureTest("Plist1", "Levels", "Timings", 99999, 1000, "PrePlistCallback")).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("pat2")).Returns(false);

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("Plist1")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance"));
            writerMock.Setup(o => o.SetData("1.100|0.500|1.500|4"));

            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance|scb"));
            writerMock.Setup(o => o.SetData("0.900|pat2|0|0|2"));
            writerMock.Setup(o => o.SetDelimiterCharacterForWrap('|'));

            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance|CORE1"));
            writerMock.Setup(o => o.SetData("80000000001|80000000002"));
            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance|CORE2"));
            writerMock.Setup(o => o.SetData("80000000001"));
            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance|NotInPinMap"));
            writerMock.Setup(o => o.SetData("80000000001"));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var voltageMock = new Mock<IVoltage>(MockBehavior.Strict);
            var extensionMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            extensionMock.Setup(o => o.GetSearchVoltageObject(new List<string> { "CORE0" }, "Plist1")).Returns(voltageMock.Object);
            extensionMock.Setup(o => o.GetStartVoltageValues(new List<string> { "0.5" })).Returns(new List<double> { 0.5 });
            extensionMock.Setup(o => o.GetEndVoltageLimitValues(new List<string> { "1.5" })).Returns(new List<double> { 1.5 });
            extensionMock.Setup(o => o.ApplySearchVoltage(voltageMock.Object, It.Is<List<double>>(it => Math.Abs(it[0] - 0.9) < 0.01)));
            extensionMock.Setup(o => o.ApplyMask(null, funcTestMock.Object)); // Can't write this.InitialMask_ so it'll be null

            var pointData1 = new SearchPointData(new List<double> { 0.8 }, new SearchPointData.PatternData("pat1", 0, 0));
            var pointData2 = new SearchPointData(new List<double> { 0.9 }, new SearchPointData.PatternData("pat2", 0, 0));
            var pointData3 = new SearchPointData(new List<double> { 1.0 }, new SearchPointData.PatternData("pat3", 0, 0));
            var pointData4 = new SearchPointData(new List<double> { 1.1 }, new SearchPointData.PatternData("pat4", 0, 0));

            var pinToCoreMap = new Dictionary<string, string>
            {
                { "Pin1", "CORE1" },
                { "Pin2", "CORE2" },
            };

            var pinMapMock = new Mock<IPinMap>(MockBehavior.Strict);
            pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { });

            var underTest = new VminSearch();
            underTest.TestMethodExtension = extensionMock.Object;
            underTest.InstanceName = "MyInstance";
            underTest.VoltageTargets = "CORE0";
            underTest.Patlist = "Plist1";
            underTest.LevelsTc = "Levels";
            underTest.TimingsTc = "Timings";
            underTest.PrePlist = "PrePlistCallback";
            underTest.StartVoltages = "0.5";
            underTest.EndVoltageLimits = "1.5";
            underTest.StepSize = 0.1;
            underTest.ScoreboardBaseNumber = "8000";
            underTest.PatternNameMap = "1,2,3,4,5,6,7";
            underTest.ScoreboardEdgeTicks = 2;

            underTest.ChildCustomVerify(pinMapMock.Object);
            Assert.IsTrue(underTest.ExecuteCustomScoreboard(new List<SearchPointData> { pointData1, pointData2, pointData3, pointData4 }, pinToCoreMap));

            extensionMock.VerifyAll();
            funcServiceMock.VerifyAll();
            funcTestMock.VerifyAll();
            plistMock.VerifyAll();
            plistServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ExecuteScoreboard_AmbleFailsScoreboard_Pass()
        {
            var fail1 = new Mock<IFailureData>(MockBehavior.Strict);
            fail1.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "Pin1", "Pin2", "NotInPinMap" });
            fail1.Setup(o => o.GetPatternName()).Returns("d0000001");

            var fail2 = new Mock<IFailureData>(MockBehavior.Strict);
            fail2.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "Pin1" });
            fail2.Setup(o => o.GetPatternName()).Returns("d0000002");

            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.SetStartPattern("pat2", 0, 0));
            funcTestMock.Setup(o => o.Execute()).Returns(false);
            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fail1.Object, fail2.Object });

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureTest("Plist1", "Levels", "Timings", 99999, 1000, "PrePlistCallback")).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("pat2")).Returns(false);

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("Plist1")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance"));
            writerMock.Setup(o => o.SetData("1.100|0.500|1.500|4"));

            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance|scb"));
            writerMock.Setup(o => o.SetData("0.900|pat2|0|0|2"));
            writerMock.Setup(o => o.SetDelimiterCharacterForWrap('|'));

            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance|CORE1"));
            writerMock.Setup(o => o.SetData("80000000001|80000000002"));

            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance|CORE2"));
            writerMock.Setup(o => o.SetData("80000000001"));

            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance|NotInPinMap"));
            writerMock.Setup(o => o.SetData("80000000001"));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var voltageMock = new Mock<IVoltage>(MockBehavior.Strict);
            var extensionMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            extensionMock.Setup(o => o.GetSearchVoltageObject(new List<string> { "CORE0" }, "Plist1")).Returns(voltageMock.Object);
            extensionMock.Setup(o => o.GetStartVoltageValues(new List<string> { "0.5" })).Returns(new List<double> { 0.5 });
            extensionMock.Setup(o => o.GetEndVoltageLimitValues(new List<string> { "1.5" })).Returns(new List<double> { 1.5 });
            extensionMock.Setup(o => o.ApplySearchVoltage(voltageMock.Object, It.Is<List<double>>(it => Math.Abs(it[0] - 0.9) < 0.01)));
            extensionMock.Setup(o => o.ApplyMask(null, funcTestMock.Object)); // Can't write this.InitialMask_ so it'll be null

            var pointData1 = new SearchPointData(new List<double> { 0.8 }, new SearchPointData.PatternData("pat1", 0, 0));
            var pointData2 = new SearchPointData(new List<double> { 0.9 }, new SearchPointData.PatternData("pat2", 0, 0));
            var pointData3 = new SearchPointData(new List<double> { 1.0 }, new SearchPointData.PatternData("pat3", 0, 0));
            var pointData4 = new SearchPointData(new List<double> { 1.1 }, new SearchPointData.PatternData("pat4", 0, 0));

            var pinToCoreMap = new Dictionary<string, string>
            {
                { "Pin1", "CORE1" },
                { "Pin2", "CORE2" },
            };

            var pinMapMock = new Mock<IPinMap>(MockBehavior.Strict);
            pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { });

            var underTest = new VminSearch();
            underTest.TestMethodExtension = extensionMock.Object;
            underTest.InstanceName = "MyInstance";
            underTest.VoltageTargets = "CORE0";
            underTest.Patlist = "Plist1";
            underTest.LevelsTc = "Levels";
            underTest.TimingsTc = "Timings";
            underTest.PrePlist = "PrePlistCallback";
            underTest.StartVoltages = "0.5";
            underTest.EndVoltageLimits = "1.5";
            underTest.StepSize = 0.1;
            underTest.ScoreboardBaseNumber = "8000";
            underTest.PatternNameMap = "1,2,3,4,5,6,7";
            underTest.ScoreboardEdgeTicks = 2;

            underTest.ChildCustomVerify(pinMapMock.Object);
            Assert.IsTrue(underTest.ExecuteCustomScoreboard(new List<SearchPointData> { pointData1, pointData2, pointData3, pointData4 }, pinToCoreMap));

            extensionMock.VerifyAll();
            funcServiceMock.VerifyAll();
            funcTestMock.VerifyAll();
            plistMock.VerifyAll();
            plistServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ExecuteScoreboard_SearchFailedPass()
        {
            /* setup the mocks for execute */
            var fail1 = new Mock<IFailureData>(MockBehavior.Strict);
            fail1.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "Pin1", "Pin2", "NotInPinMap" });
            fail1.Setup(o => o.GetPatternName()).Returns("d0000001");

            var fail2 = new Mock<IFailureData>(MockBehavior.Strict);
            fail2.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "Pin1" });
            fail2.Setup(o => o.GetPatternName()).Returns("d0000002");

            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.SetStartPattern("pat4", 0, 0));
            funcTestMock.Setup(o => o.Execute()).Returns(false);
            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { fail1.Object, fail2.Object });

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureTest("Plist1", "Levels", "Timings", 99999, 1000, "PrePlistCallback")).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("pat4")).Returns(false);

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("Plist1")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance"));
            writerMock.Setup(o => o.SetData("-9999|0.500|1.100|4"));

            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance|scb"));
            writerMock.Setup(o => o.SetData("1.100|pat4|0|0|2"));
            writerMock.Setup(o => o.SetDelimiterCharacterForWrap('|'));

            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance|CORE1"));
            writerMock.Setup(o => o.SetData("80000000001|80000000002"));
            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance|CORE2"));
            writerMock.Setup(o => o.SetData("80000000001"));
            writerMock.Setup(o => o.SetTnamePostfix("|MyInstance|NotInPinMap"));
            writerMock.Setup(o => o.SetData("80000000001"));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var voltageMock = new Mock<IVoltage>(MockBehavior.Strict);
            var extensionMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            extensionMock.Setup(o => o.GetSearchVoltageObject(new List<string> { "CORE0" }, "Plist1")).Returns(voltageMock.Object);
            extensionMock.Setup(o => o.GetStartVoltageValues(new List<string> { "0.5" })).Returns(new List<double> { 0.5 });
            extensionMock.Setup(o => o.GetEndVoltageLimitValues(new List<string> { "1.1" })).Returns(new List<double> { 1.1 });
            extensionMock.Setup(o => o.ApplySearchVoltage(voltageMock.Object, It.Is<List<double>>(it => Math.Abs(it[0] - 1.1) < 0.01)));
            extensionMock.Setup(o => o.ApplyMask(null, funcTestMock.Object)); // Can't write this.InitialMask_ so it'll be null

            var pointData1 = new SearchPointData(new List<double> { 0.9 }, new SearchPointData.PatternData("pat2", 0, 0));
            var pointData2 = new SearchPointData(new List<double> { 1.0 }, new SearchPointData.PatternData("pat3", 0, 0));
            var pointData3 = new SearchPointData(new List<double> { 1.1 }, new SearchPointData.PatternData("pat4", 0, 0));
            var pointData4 = new SearchPointData(new List<double> { -9999 }, new SearchPointData.PatternData(string.Empty, 0, 0));

            var pinMapMock = new Mock<IPinMap>(MockBehavior.Strict);
            pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { });

            var pinToCoreMap = new Dictionary<string, string>
            {
                { "Pin1", "CORE1" },
                { "Pin2", "CORE2" },
            };

            var underTest = new VminSearch();
            underTest.TestMethodExtension = extensionMock.Object;
            underTest.InstanceName = "MyInstance";
            underTest.VoltageTargets = "CORE0";
            underTest.Patlist = "Plist1";
            underTest.LevelsTc = "Levels";
            underTest.TimingsTc = "Timings";
            underTest.PrePlist = "PrePlistCallback";
            underTest.StartVoltages = "0.5";
            underTest.EndVoltageLimits = "1.1";
            underTest.StepSize = 0.1;
            underTest.ScoreboardBaseNumber = "8000";
            underTest.PatternNameMap = "1,2,3,4,5,6,7";
            underTest.ScoreboardEdgeTicks = 2;

            underTest.ChildCustomVerify(pinMapMock.Object);
            Assert.IsTrue(underTest.ExecuteCustomScoreboard(new List<SearchPointData> { pointData1, pointData2, pointData3, pointData4 }, pinToCoreMap));

            extensionMock.VerifyAll();
            funcServiceMock.VerifyAll();
            funcTestMock.VerifyAll();
            plistMock.VerifyAll();
            plistServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// Verify all the getters/setters for the vminsearch code.
        /// </summary>
        [TestMethod]
        public void VminSearch_BasicAccessRegex_Pass()
        {
            var pinMapMock = new Mock<IPinMap>(MockBehavior.Strict);
            pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder>());

            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureTest("plist1", "levels", "timings", 99999, 1000, "preplistcallback")).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("plist1")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            var testprogramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramServiceMock.Setup(o => o.DoesCallbackExist("precallback")).Returns(true);
            testprogramServiceMock.Setup(o => o.DoesCallbackExist("postcallback")).Returns(true);
            Prime.Services.TestProgramService = testprogramServiceMock.Object;

            var patSetPointMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            var patConfigService = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigService.Setup(o => o.GetSetPointHandleWithRegEx("MODULE", "GROUP", "plist1", "pat.*")).Returns(patSetPointMock.Object);

            patConfigService.Setup(o => o.IsSetPointHandleExist("MODULE", "GROUP", "pre_sp")).Returns(SetPointHandleCheckerSymbol.EXIST);
            patConfigService.Setup(o => o.IsSetPointHandleExist("MODULE", "GROUP", "post_sp")).Returns(SetPointHandleCheckerSymbol.EXIST);
            Prime.Services.PatConfigService = patConfigService.Object;

            var factory = new VminFactory();
            var vminSearch = factory.CreateInstance();
            vminSearch.PreInstance = "precallback(arg1, arg2)";
            vminSearch.PostInstance = "postcallback()";
            vminSearch.SetPointsPlistParamName = "dummyparam";
            vminSearch.SetPointsRegEx = "pat.*";
            vminSearch.SetPointsPreInstance = "MODULE:GROUP:pre_sp";
            vminSearch.SetPointsPostInstance = "MODULE:GROUP:post_sp";
            vminSearch.Patlist = "plist1";
            vminSearch.TimingsTc = "timings";
            vminSearch.LevelsTc = "levels";
            vminSearch.PrePlist = "preplistcallback";

            vminSearch.ChildCustomVerify(pinMapMock.Object);
            Assert.AreEqual(new Tuple<string, string>("precallback", "arg1, arg2"), vminSearch.PreInstanceCallback);
            Assert.AreEqual(new Tuple<string, string>("postcallback", string.Empty), vminSearch.PostInstanceCallback);
            Assert.AreEqual(1, vminSearch.PreConfigSetPointsWithData.Count);
            Assert.AreEqual(new Tuple<IPatConfigSetPointHandle, string>(patSetPointMock.Object, "pre_sp"), vminSearch.PreConfigSetPointsWithData.First());
            Assert.AreEqual(0, vminSearch.PreConfigSetPointsWithDefault.Count);
            Assert.AreEqual(1, vminSearch.PostConfigSetPointsWithData.Count);
            Assert.AreEqual(new Tuple<IPatConfigSetPointHandle, string>(patSetPointMock.Object, "post_sp"), vminSearch.PostConfigSetPointsWithData.First());
            Assert.AreEqual(0, vminSearch.PostConfigSetPointsWithDefault.Count);
            Assert.AreEqual("dummyparam", vminSearch.SetPointsPlistParamName.ToString()); /* not actually used anywhere */

            funcServiceMock.VerifyAll();
            plistServiceMock.VerifyAll();
            testprogramServiceMock.VerifyAll();
            patConfigService.VerifyAll();
        }

        /// <summary>
        /// Verify all the getters/setters for the vminsearch code.
        /// </summary>
        [TestMethod]
        public void VminSearch_BasicAccessNoRegex_Pass()
        {
            var pinMapMock = new Mock<IPinMap>(MockBehavior.Strict);
            pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder>());

            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureTest("plist1", "levels", "timings", 99999, 1000, "preplistcallback")).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("plist1")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            var testprogramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramServiceMock.Setup(o => o.DoesCallbackExist("precallback")).Returns(true);
            testprogramServiceMock.Setup(o => o.DoesCallbackExist("postcallback")).Returns(true);
            Prime.Services.TestProgramService = testprogramServiceMock.Object;

            var patSetPointMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            var patConfigService = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigService.Setup(o => o.GetSetPointHandle("MODULE", "GROUP", "plist1")).Returns(patSetPointMock.Object);

            patConfigService.Setup(o => o.IsSetPointHandleExist("MODULE", "GROUP", "pre_sp")).Returns(SetPointHandleCheckerSymbol.EXIST);
            patConfigService.Setup(o => o.IsSetPointHandleExist("MODULE", "GROUP", "post_sp")).Returns(SetPointHandleCheckerSymbol.EXIST);
            Prime.Services.PatConfigService = patConfigService.Object;

            var factory = new VminFactory();
            var vminSearch = factory.CreateInstance();
            vminSearch.PreInstance = "precallback(arg1, arg2)";
            vminSearch.PostInstance = "postcallback()";
            vminSearch.SetPointsPlistParamName = "dummyparam";
            vminSearch.SetPointsRegEx = string.Empty;
            vminSearch.SetPointsPreInstance = "MODULE:GROUP:pre_sp";
            vminSearch.SetPointsPostInstance = "MODULE:GROUP:post_sp";
            vminSearch.Patlist = "plist1";
            vminSearch.TimingsTc = "timings";
            vminSearch.LevelsTc = "levels";
            vminSearch.PrePlist = "preplistcallback";

            vminSearch.ChildCustomVerify(pinMapMock.Object);
            Assert.AreEqual(new Tuple<string, string>("precallback", "arg1, arg2"), vminSearch.PreInstanceCallback);
            Assert.AreEqual(new Tuple<string, string>("postcallback", string.Empty), vminSearch.PostInstanceCallback);
            Assert.AreEqual(1, vminSearch.PreConfigSetPointsWithData.Count);
            Assert.AreEqual(new Tuple<IPatConfigSetPointHandle, string>(patSetPointMock.Object, "pre_sp"), vminSearch.PreConfigSetPointsWithData.First());
            Assert.AreEqual(0, vminSearch.PreConfigSetPointsWithDefault.Count);
            Assert.AreEqual(1, vminSearch.PostConfigSetPointsWithData.Count);
            Assert.AreEqual(new Tuple<IPatConfigSetPointHandle, string>(patSetPointMock.Object, "post_sp"), vminSearch.PostConfigSetPointsWithData.First());
            Assert.AreEqual(0, vminSearch.PostConfigSetPointsWithDefault.Count);
            Assert.AreEqual("dummyparam", vminSearch.SetPointsPlistParamName.ToString()); /* not actually used anywhere */
            funcServiceMock.VerifyAll();
            plistServiceMock.VerifyAll();
            testprogramServiceMock.VerifyAll();
            patConfigService.VerifyAll();
        }

        /// <summary>
        /// Verify all the getters/setters for the vminsearch code.
        /// </summary>
        [TestMethod]
        public void VminSearch_BasicAccessDefaultData_Pass()
        {
            var pinMapMock = new Mock<IPinMap>(MockBehavior.Strict);
            pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder>());

            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureTest("plist1", "levels", "timings", 99999, 1000, "preplistcallback")).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject("plist1")).Returns(plistMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;

            var testprogramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramServiceMock.Setup(o => o.DoesCallbackExist("precallback")).Returns(true);
            testprogramServiceMock.Setup(o => o.DoesCallbackExist("postcallback")).Returns(true);
            Prime.Services.TestProgramService = testprogramServiceMock.Object;

            var prePatSetPointMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            var postPatSetPointMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            var patConfigService = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigService.Setup(o => o.GetSetPointHandle("MODULE", "GROUP1", "plist1")).Returns(prePatSetPointMock.Object);
            patConfigService.Setup(o => o.GetSetPointHandle("MODULE", "GROUP2", "plist1")).Returns(postPatSetPointMock.Object);
            Prime.Services.PatConfigService = patConfigService.Object;

            var factory = new VminFactory();
            var vminSearch = factory.CreateInstance();
            vminSearch.PreInstance = "precallback(arg1, arg2)";
            vminSearch.PostInstance = "postcallback()";
            vminSearch.SetPointsPlistParamName = "dummyparam";
            vminSearch.SetPointsRegEx = string.Empty;
            vminSearch.SetPointsPreInstance = "MODULE:GROUP1";
            vminSearch.SetPointsPostInstance = "MODULE:GROUP2";
            vminSearch.Patlist = "plist1";
            vminSearch.TimingsTc = "timings";
            vminSearch.LevelsTc = "levels";
            vminSearch.PrePlist = "preplistcallback";

            vminSearch.ChildCustomVerify(pinMapMock.Object);
            Assert.AreEqual(new Tuple<string, string>("precallback", "arg1, arg2"), vminSearch.PreInstanceCallback);
            Assert.AreEqual(new Tuple<string, string>("postcallback", string.Empty), vminSearch.PostInstanceCallback);
            Assert.AreEqual(0, vminSearch.PreConfigSetPointsWithData.Count);
            Assert.AreEqual(1, vminSearch.PreConfigSetPointsWithDefault.Count);
            Assert.AreEqual(prePatSetPointMock.Object, vminSearch.PreConfigSetPointsWithDefault.First());
            Assert.AreEqual(0, vminSearch.PostConfigSetPointsWithData.Count);
            Assert.AreEqual(1, vminSearch.PostConfigSetPointsWithDefault.Count);
            Assert.AreEqual(postPatSetPointMock.Object, vminSearch.PostConfigSetPointsWithDefault.First());
            Assert.AreEqual("dummyparam", vminSearch.SetPointsPlistParamName.ToString()); /* not actually used anywhere */

            funcServiceMock.VerifyAll();
            plistServiceMock.VerifyAll();
            testprogramServiceMock.VerifyAll();
            patConfigService.VerifyAll();
        }

        private void MockConfigFile(string filename, string filecontents, out Mock<IFileService> fileServiceMock, out Mock<IFileSystem> fileSystemMock)
        {
            fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.FileExists(filename)).Returns(true);
            fileServiceMock.Setup(o => o.GetFile(filename)).Returns(filename);
            Prime.Services.FileService = fileServiceMock.Object;

            fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
            fileSystemMock.Setup(o => o.File.ReadAllText(filename)).Returns(filecontents);
        }
    }
}
