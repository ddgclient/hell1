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

namespace ArrayHRY.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.PinService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;
    using Prime.UserVarService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class ArrayHRY_UnitTest
    {
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
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Verify_FullXml_Pass()
        {
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_AllOptions.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsVerifyOnly("HSR.HRY_Global_1,HSR.HRY_Global_2,HSR.HRY_Global_3,HSR.HRY_Global_4");
            var functionalMocks = this.MockFunctionalTestVerifyOnly("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty);

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_AllOptions.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
            };

            underTest.Verify();
            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Verify_MinimumXml_Pass()
        {
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_MinRequiredOptionsWithReverse.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsVerifyOnly("HSR.HRY_Global_1,HSR.HRY_Global_2,HSR.HRY_Global_3,HSR.HRY_Global_4");
            var functionalMocks = this.MockFunctionalTestVerifyOnly("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty);

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_MinRequiredOptionsWithReverse.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
            };

            underTest.Verify();
            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Verify_FailSchema_Exception()
        {
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("FailingHryXml_FailSchema.xml", ref sharedStorageMock);

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "FailingHryXml_FailSchema.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
            };

            var ex = Assert.ThrowsException<InvalidOperationException>(() => underTest.Verify());
            Assert.IsTrue(ex.Message.Contains("There is an error in XML document"));
            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Try to hit all the non-schema failing cases with one xml.
        /// </summary>
        [TestMethod]
        public void Verify_FailConfigNotSchema_Exception()
        {
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("FailingHryXml_PassSchemaFailOther.xml", ref sharedStorageMock);
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");

            var pin001Mock = new Mock<IPin>(MockBehavior.Strict);
            pin001Mock.Setup(o => o.IsGroup()).Returns(false);
            var pin002Mock = new Mock<IPin>(MockBehavior.Strict);
            pin002Mock.Setup(o => o.IsGroup()).Returns(false);
            var pin004Mock = new Mock<IPin>(MockBehavior.Strict);
            pin004Mock.Setup(o => o.IsGroup()).Returns(true);

            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(o => o.Exists("P001")).Returns(true);
            pinServiceMock.Setup(o => o.Exists("P002")).Returns(true);
            pinServiceMock.Setup(o => o.Exists("P003")).Returns(false);
            pinServiceMock.Setup(o => o.Exists("P004")).Returns(true);
            pinServiceMock.Setup(o => o.Get("P001")).Returns(pin001Mock.Object);
            pinServiceMock.Setup(o => o.Get("P002")).Returns(pin002Mock.Object);
            pinServiceMock.Setup(o => o.Get("P004")).Returns(pin004Mock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("HSR.HRY_Global_1")).Returns(true);
            userVarServiceMock.Setup(o => o.Exists("HSR.HRY_Global_2")).Returns(true);
            userVarServiceMock.Setup(o => o.Exists("HSR.HRY_Global_3")).Returns(true);
            userVarServiceMock.Setup(o => o.Exists("HSR.HRY_Global_4")).Returns(false);
            userVarServiceMock.Setup(o => o.Exists("HSR::HSR.HRY_Global_4")).Returns(false);
            userVarServiceMock.Setup(o => o.Exists("__main__::HSR::HSR.HRY_Global_4")).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "FailingHryXml_PassSchemaFailOther.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
            };

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.Verify());
            System.Console.WriteLine(ex.ToString());
            Assert.IsTrue(ex.Message.Contains("Encountered [9] error(s) reading the xml."), "Failed Error Count.");
            Assert.IsTrue(ex.Message.Contains("Criterias: pin=[P003] is not a valid pin (groups are not allowed)."), "Failed P003 does not exist check.");
            Assert.IsTrue(ex.Message.Contains("Condition: pin=[P004] is not a valid pin (groups are not allowed)."), "Failed P004 is a group check.");
            Assert.IsTrue(ex.Message.Contains("Condition: PassingValue=[0] should have [2] bit(s). From hry_index=[5]."), "Failed condition mismatch range/value check.");
            Assert.IsTrue(ex.Message.Contains("Condition: BypassUserVar=[HSR.HRY_Global_4] does not exist. From hry_index=[9]."), "Failed missing uservar check.");
            Assert.IsTrue(ex.Message.Contains("Condition: FailKey=[key3] does not exist. From hry_index=[2]. Valid Keys=[key1,key2]"), "Failed missing ConditionFailKey check.");
            Assert.IsTrue(ex.Message.Contains("[hry_index] must be zero-based and have consecutive numbers (found [1], expect [0])"), "Failed hry_index=1 check.");
            Assert.IsTrue(ex.Message.Contains("[hry_index] must be zero-based and have consecutive numbers (found [8], expect [7])"), "Failed hry_index=8 check.");
            Assert.IsTrue(ex.Message.Contains("Algorithm=[PMOVI] is missing ctv_size for pin=[P002] (or [default])."), "Failed ctv_size check.");
            Assert.IsTrue(ex.Message.Contains("Algorithm=[March-C] has a non-consecutive index=[3]. Expecting [2]."), "Failed algorithm index check.");

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinServiceMock.VerifyAll();
            pin001Mock.VerifyAll();
            pin002Mock.VerifyAll();
            pin004Mock.VerifyAll();
            userVarServiceMock.VerifyAll();
            testprogramMock.VerifyAll();
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Execute_MissingCapturePin_Exception()
        {
            var userVarGlobals = new Dictionary<string, int>
            {
                { "HSR.HRY_Global_1", 1 },
                { "HSR.HRY_Global_2", 1 },
                { "HSR.HRY_Global_3", 1 },
                { "HSR.HRY_Global_4", 1 },
            };
            var ctvData = new Dictionary<string, string>
            {
                { "P001", "000000000000000000000000000000000000" },
            };
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_MinRequiredOptionsWithReverse.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsForExecute(userVarGlobals);
            var functionalMocks = this.MockFunctionalTestForExecute("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty, string.Empty, ctvData);

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_MinRequiredOptionsWithReverse.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
            };

            underTest.Verify();
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.Execute());
            Assert.AreEqual("Captured CTV is missing Pin=[P002]. CapturedPins=[P001].", ex.Message);

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Execute_CaptureWrongTotalVectors1Alg_Exception()
        {
            var userVarGlobals = new Dictionary<string, int>
            {
                { "HSR.HRY_Global_1", 1 },
                { "HSR.HRY_Global_2", 1 },
            };
            var ctvData = new Dictionary<string, string>
            {
                { "P001", "00" },
                { "P002", "00" },
            };
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_1AlgMultiDomain.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsForExecute(userVarGlobals);
            var functionalMocks = this.MockFunctionalTestForExecute("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty, string.Empty, ctvData);

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_1AlgMultiDomain.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
            };

            underTest.Verify();
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.Execute());
            Assert.AreEqual("Captured data size [2] does not match minimum required size [4] for pin [P002].", ex.Message);

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Execute_CaptureWrongTotalVectors3Alg_Exception()
        {
            var userVarGlobals = new Dictionary<string, int>
            {
                { "HSR.HRY_Global_1", 1 },
                { "HSR.HRY_Global_2", 1 },
                { "HSR.HRY_Global_3", 1 },
                { "HSR.HRY_Global_4", 1 },
            };
            var ctvData = new Dictionary<string, string>
            {
                { "P001", "0000000000000" },
                { "P002", "000100010001000100010001000000000000000100010001000100010001000000000000000100010001000100010001000000000000" },
            };
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_AllOptions.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsForExecute(userVarGlobals);
            var functionalMocks = this.MockFunctionalTestForExecute("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty, string.Empty, ctvData);

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_AllOptions.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
            };

            underTest.Verify();
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.Execute());
            Assert.AreEqual("Captured data size [13] does not match the expected size [108] for pin [P001].", ex.Message);

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Execute_MinimumXmlAllPass_Port1()
        {
            var userVarGlobals = new Dictionary<string, int>
            {
                { "HSR.HRY_Global_1", 1 },
                { "HSR.HRY_Global_2", 1 },
                { "HSR.HRY_Global_3", 1 },
                { "HSR.HRY_Global_4", 1 },
            };
            var ctvData = new Dictionary<string, string>
            {
                { "P001", "000000000000000000000000000000000000" }, // reverse = 000000000000000000000000000000000000
                { "P002", "000000000000100010001000100010001000" }, // reverse = 000100010001000100010001000000000000
            };
            var finalHry = new Dictionary<string, string> { { string.Empty, "000000009" } };
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_MinRequiredOptionsWithReverse.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsForExecute(userVarGlobals);
            var functionalMocks = this.MockFunctionalTestForExecute("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty, string.Empty, ctvData);
            var datalogMocks = this.MockDatalog(finalHry, string.Empty, ref sharedStorageMock);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_MinRequiredOptionsWithReverse.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
            };

            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
            datalogMocks.ForEach(mock => mock.VerifyAll());
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Execute_MultiDomain1Alg_Port1()
        {
            var userVarGlobals = new Dictionary<string, int>
            {
                { "HSR.HRY_Global_1", 1 },
                { "HSR.HRY_Global_2", 1 },
            };
            var ctvData = new Dictionary<string, string>
            {
                { "P001", "00" },
                { "P002", "0000" },
            };
            var finalHry = new Dictionary<string, string> { { string.Empty, "00" } };
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_1AlgMultiDomain.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsForExecute(userVarGlobals);
            var functionalMocks = this.MockFunctionalTestForExecute("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty, string.Empty, ctvData);
            var datalogMocks = this.MockDatalog(finalHry, string.Empty, ref sharedStorageMock);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_1AlgMultiDomain.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
            };

            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
            datalogMocks.ForEach(mock => mock.VerifyAll());
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Execute_MultiDomain3Alg_Port2()
        {
            var userVarGlobals = new Dictionary<string, int>
            {
                { "HSR.HRY_Global_1", 1 },
                { "HSR.HRY_Global_2", -1 },
            };
            var ctvData = new Dictionary<string, string>
            {
                { "P001", "000100" },
                { "P002", "000000000001" },
            };
            var finalHry = new Dictionary<string, string>
            {
                { "AlgA", "00" },
                { "AlgB", "01" },
                { "AlgC", "08" },
            };
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_3AlgMultiDomain.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsForExecute(userVarGlobals);
            var functionalMocks = this.MockFunctionalTestForExecute("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty, string.Empty, ctvData);
            var datalogMocks = this.MockDatalog(finalHry, string.Empty, ref sharedStorageMock);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_3AlgMultiDomain.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
            };

            underTest.Verify();
            Assert.AreEqual(2, underTest.Execute());

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
            datalogMocks.ForEach(mock => mock.VerifyAll());
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Execute_MinimumXmlHryFail_Port2()
        {
            var userVarGlobals = new Dictionary<string, int>
            {
                { "HSR.HRY_Global_1", 1 },
                { "HSR.HRY_Global_2", -1 },
                { "HSR.HRY_Global_3", -1 },
                { "HSR.HRY_Global_4", 1 },
            };
            var ctvData = new Dictionary<string, string>
            {
                { "P001", "000100000000000001000000000000000000" }, // reverse = 000000000000000000100000000000001000
                { "P002", "000000000000100010001000100010001000" }, // reverse = 000100010001000100010001000000000000
            };
            var finalHry = new Dictionary<string, string> { { string.Empty, "000010019" } };
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_MinRequiredOptionsWithReverse.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsForExecute(userVarGlobals);
            var functionalMocks = this.MockFunctionalTestForExecute("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty, string.Empty, ctvData);
            var datalogMocks = this.MockDatalog(finalHry, string.Empty, ref sharedStorageMock);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_MinRequiredOptionsWithReverse.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
            };

            underTest.Verify();
            Assert.AreEqual(2, underTest.Execute());

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
            datalogMocks.ForEach(mock => mock.VerifyAll());
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Execute_MinimumXmlConditionFail_Port3()
        {
            var userVarGlobals = new Dictionary<string, int>
            {
                { "HSR.HRY_Global_1", 1 },
                { "HSR.HRY_Global_2", 1 },
                { "HSR.HRY_Global_3", 1 },
                { "HSR.HRY_Global_4", 1 },
            };
            var ctvData = new Dictionary<string, string>
            {
                { "P001", "000000000000000000000000000000000000" }, // reverse
                { "P002", "000100000000100010001000101110000000" }, // reverse = 000000011101000100010001000000001000
            };
            var finalHry = new Dictionary<string, string> { { string.Empty, "808000008" } };
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_MinRequiredOptionsWithReverse.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsForExecute(userVarGlobals);
            var functionalMocks = this.MockFunctionalTestForExecute("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty, string.Empty, ctvData);
            var datalogMocks = this.MockDatalog(finalHry, string.Empty, ref sharedStorageMock);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_MinRequiredOptionsWithReverse.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
            };

            underTest.Verify();
            Assert.AreEqual(3, underTest.Execute());

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
            datalogMocks.ForEach(mock => mock.VerifyAll());
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Execute_AllOptionsXmlPreMode_Port2()
        {
            var userVarGlobals = new Dictionary<string, int>
            {
                { "HSR.HRY_Global_1", 1 },
                { "HSR.HRY_Global_2", 1 },
                { "HSR.HRY_Global_3", -1 },
                { "HSR.HRY_Global_4", 1 },
            };
            var ctvData = new Dictionary<string, string>
            {
                /*                   11111111112222222222333333               11111111112222222222333333               11111111112222222222333333
                 *         012345678901234567890123456789012345     012345678901234567890123456789012345     012345678901234567890123456789012345*/
                { "P001", "000000000000000000000010000000000000" + "000000000000000000000000000000000000" + "000000000000000000000000000000000001" },
                { "P002", "110101011001100111010001000000000000" + "000100010001000100010001000000000000" + "000000010001000100010001000000001100" },
            };

            var finalHry = new Dictionary<string, string> { { "SCAN", "576881009" }, { "PMOVI", "000000009" }, { "March-C", "400000018" } };
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_AllOptions.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsForExecute(userVarGlobals);
            var functionalMocks = this.MockFunctionalTestForExecute("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty, string.Empty, ctvData);
            var datalogMocks = this.MockDatalog(finalHry, "ForwardedHRY", ref sharedStorageMock);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_AllOptions.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
                RawStringForwardingMode = ArrayHRY.ForwardingMode.PRE,
                SharedStorageKey = "ForwardedHRY",
            };

            underTest.Verify();
            Assert.AreEqual(2, underTest.Execute());

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
            datalogMocks.ForEach(mock => mock.VerifyAll());
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Execute_AllOptionsXmlPostMode_Port2()
        {
            var userVarGlobals = new Dictionary<string, int>
            {
                { "HSR.HRY_Global_1", -1 },
                { "HSR.HRY_Global_2", -1 },
                { "HSR.HRY_Global_3", -1 },
                { "HSR.HRY_Global_4", 1 },
            };
            var ctvData = new Dictionary<string, string>
            {
                /*                   11111111112222222222333333               11111111112222222222333333               11111111112222222222333333
                 *         012345678901234567890123456789012345     012345678901234567890123456789012345     012345678901234567890123456789012345*/
                { "P001", "000000000000001000100010000000000000" + "000000000000000000000000000000000000" + "000000000000000000000000000000000000" },
                { "P002", "000100000001000100010001000000001000" + "010111010001000111010001000000000100" + "000100010001000100010001000000000000" },
            };

            // Note: Evergreen doesn't change bad-to-bad even if the hry character changes. only good-to-bad and bad-to-good get updated.
            var fwdHry = new Dictionary<string, string> { { "SCAN", "146180808" }, { "PMOVI", "750010008" }, { "March-C", "000000009" } };
            var tstHry = new Dictionary<string, string> { { "SCAN", "040111008" }, { "PMOVI", "750080008" }, { "March-C", "000000009" } };
            var fnlHry = new Dictionary<string, string> { { "SCAN", "R4R181R08" }, { "PMOVI", "750010008" }, { "March-C", "000000009" } };

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_AllOptions.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsForExecute(userVarGlobals);
            var functionalMocks = this.MockFunctionalTestForExecute("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty, string.Empty, ctvData);
            var datalogMocks = this.MockDatalog(fnlHry, string.Empty, ref sharedStorageMock);

            foreach (var algorithmName in fwdHry.Keys)
            {
                sharedStorageMock.Setup(o => o.GetStringRowFromTable($"ForwardedHRY_{algorithmName}", Context.DUT)).Returns(fwdHry[algorithmName]);
            }

            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_AllOptions.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
                RawStringForwardingMode = ArrayHRY.ForwardingMode.POST,
                SharedStorageKey = "ForwardedHRY",
            };

            underTest.Verify();
            Assert.AreEqual(2, underTest.Execute());

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
            datalogMocks.ForEach(mock => mock.VerifyAll());
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Execute_PostModeSizeMismatch_Exception()
        {
            var userVarGlobals = new Dictionary<string, int>
            {
                { "HSR.HRY_Global_1", 1 },
                { "HSR.HRY_Global_2", 1 },
                { "HSR.HRY_Global_3", 1 },
                { "HSR.HRY_Global_4", 1 },
            };
            var ctvData = new Dictionary<string, string>
            {
                /*                   11111111112222222222333333               11111111112222222222333333               11111111112222222222333333
                 *         012345678901234567890123456789012345     012345678901234567890123456789012345     012345678901234567890123456789012345*/
                { "P001", "000000000000001000100010000000000000" + "000000000000000000000000000000000000" + "000000000000000000000000000000000000" },
                { "P002", "000100000001000100010001000000001000" + "010111010001000111010001000000000100" + "000100010001000100010001000000000000" },
            };

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_AllOptions.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsForExecute(userVarGlobals);
            var functionalMocks = this.MockFunctionalTestForExecute("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty, string.Empty, ctvData);

            sharedStorageMock.Setup(o => o.GetStringRowFromTable($"ForwardedHRY_SCAN", Context.DUT)).Returns("00000");
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_AllOptions.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
                RawStringForwardingMode = ArrayHRY.ForwardingMode.POST,
                SharedStorageKey = "ForwardedHRY",
            };

            underTest.Verify();
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.Execute());
            Assert.AreEqual("Forwarded HRY has [5] bits while this tests HRY has [9]. SharedStorageKey is probably not correct.", ex.Message);

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Execute_PostModeInvalidForwardChar_Exception()
        {
            var userVarGlobals = new Dictionary<string, int>
            {
                { "HSR.HRY_Global_1", 1 },
                { "HSR.HRY_Global_2", 1 },
                { "HSR.HRY_Global_3", 1 },
                { "HSR.HRY_Global_4", 1 },
            };
            var ctvData = new Dictionary<string, string>
            {
                /*                   11111111112222222222333333               11111111112222222222333333               11111111112222222222333333
                 *         012345678901234567890123456789012345     012345678901234567890123456789012345     012345678901234567890123456789012345*/
                { "P001", "000000000000001000100010000000000000" + "000000000000000000000000000000000000" + "000000000000000000000000000000000000" },
                { "P002", "000100000001000100010001000000001000" + "010111010001000111010001000000000100" + "000100010001000100010001000000000000" },
            };

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_AllOptions.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsForExecute(userVarGlobals);
            var functionalMocks = this.MockFunctionalTestForExecute("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty, string.Empty, ctvData);

            sharedStorageMock.Setup(o => o.GetStringRowFromTable($"ForwardedHRY_SCAN", Context.DUT)).Returns("C00000009");
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_AllOptions.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
                RawStringForwardingMode = ArrayHRY.ForwardingMode.POST,
                SharedStorageKey = "ForwardedHRY",
            };

            underTest.Verify();
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.Execute());
            Assert.AreEqual("Forward HRY char=[C] does not exist in the HryPrePostMapping field.", ex.Message);

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
        }

        /// <summary>
        /// Unit test to check the passing verify case.
        /// </summary>
        [TestMethod]
        public void Execute_PostModeInvalidTestChar_Exception()
        {
            var userVarGlobals = new Dictionary<string, int>
            {
                { "HSR.HRY_Global_1", 1 },
                { "HSR.HRY_Global_2", 1 },
                { "HSR.HRY_Global_3", 1 },
                { "HSR.HRY_Global_4", 1 },
            };
            var ctvData = new Dictionary<string, string>
            {
                /*                   11111111112222222222333333
                 *         012345678901234567890123456789012345*/
                { "P001", "000000000000001000100010000000000000" },
                { "P002", "000100000001000100010001000000001000" },
            };

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            var fileMocks = this.MockFiles("PassingHryXml_MinRequiredOptionsWithReverse.xml", ref sharedStorageMock);
            var pinMocks = this.MockPinsSimplePassingCase("P001,P002");
            var testprogramMock = this.MockTestInstanceName("DummyNameNoIpOrModule");
            var userVarServiceMock = this.MockUserVarsForExecute(userVarGlobals);
            var functionalMocks = this.MockFunctionalTestForExecute("DummyPatlist", "DummyLevels", "DummyTimings", "P001,P002", string.Empty, string.Empty, ctvData);

            sharedStorageMock.Setup(o => o.GetStringRowFromTable($"ForwardedHRY_", Context.DUT)).Returns("000000009");
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            ArrayHRY underTest = new ArrayHRY
            {
                ConfigFile = "PassingHryXml_MinRequiredOptionsWithReverse.xml",
                Patlist = "DummyPatlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
                RawStringForwardingMode = ArrayHRY.ForwardingMode.POST,
                SharedStorageKey = "ForwardedHRY",
            };

            underTest.Verify();
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.Execute());
            Assert.AreEqual("Test HRY char=[0] does not exist in the HryPrePostMapping field.", ex.Message);

            fileMocks.ForEach(mock => mock.VerifyAll());
            sharedStorageMock.VerifyAll();
            pinMocks.ForEach(mock => mock.VerifyAll());
            userVarServiceMock.VerifyAll();
            functionalMocks.ForEach(mock => mock.VerifyAll());
            testprogramMock.VerifyAll();
        }

        private List<Mock> MockPinsSimplePassingCase(string commaSeparatedListOfPins)
        {
            List<Mock> allMocks = new List<Mock>();

            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            foreach (var pin in commaSeparatedListOfPins.Split(','))
            {
                var pinMock = new Mock<IPin>(MockBehavior.Strict);
                pinMock.Setup(o => o.IsGroup()).Returns(false);

                pinServiceMock.Setup(o => o.Exists(pin)).Returns(true);
                pinServiceMock.Setup(o => o.Get(pin)).Returns(pinMock.Object);
                allMocks.Add(pinMock);
            }

            Prime.Services.PinService = pinServiceMock.Object;
            allMocks.Add(pinServiceMock);

            return allMocks;
        }

        private List<Mock> MockFiles(string configFile, ref Mock<ISharedStorageService> sharedStorageMock)
        {
            var outputDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPathToConfig = Path.Combine(outputDirectory, "ArrayHRYInputFiles", configFile);
            var fullPathToSchema = Path.Combine(outputDirectory, "ArrayHRYInputFiles", "ArrayHRY_XML.xsd");

            sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInStringTable($"__ARRAYHRY_FILECACHE__.{configFile}", Context.LOT)).Returns(false);
            sharedStorageMock.Setup(o => o.InsertRowAtTable($"__ARRAYHRY_FILECACHE__.{configFile}", fullPathToConfig, Context.LOT));
            sharedStorageMock.Setup(o => o.KeyExistsInStringTable($"__ARRAYHRY_FILECACHE__.~USER_CODE_DLLS_PATH/ArrayHRY_XML.xsd", Context.LOT)).Returns(false);
            sharedStorageMock.Setup(o => o.InsertRowAtTable($"__ARRAYHRY_FILECACHE__.~USER_CODE_DLLS_PATH/ArrayHRY_XML.xsd", fullPathToSchema, Context.LOT));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.GetNormalizedPath(configFile)).Returns(configFile);
            fileServiceMock.Setup(o => o.GetFile(configFile)).Returns(fullPathToConfig);
            fileServiceMock.Setup(o => o.GetLastModificationTime(fullPathToConfig)).Returns(new DateTime(10));

            fileServiceMock.Setup(o => o.GetNormalizedPath("~USER_CODE_DLLS_PATH/ArrayHRY_XML.xsd")).Returns("~USER_CODE_DLLS_PATH/ArrayHRY_XML.xsd");
            fileServiceMock.Setup(o => o.GetFile("~USER_CODE_DLLS_PATH/ArrayHRY_XML.xsd")).Returns(fullPathToSchema);
            fileServiceMock.Setup(o => o.GetLastModificationTime(fullPathToSchema)).Returns(new DateTime(20));
            Prime.Services.FileService = fileServiceMock.Object;

            return new List<Mock>(1) { fileServiceMock };
        }

        private List<Mock> MockDatalog(Dictionary<string, string> finalHryByAlgorithmName, string sharedStorageKey, ref Mock<ISharedStorageService> sharedStorageMock)
        {
            List<Mock> allMocks = new List<Mock>(finalHryByAlgorithmName.Count);
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var sequence = new MockSequence();

            foreach (var pair in finalHryByAlgorithmName)
            {
                var ituffWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
                var prefix = string.IsNullOrEmpty(pair.Key) ? string.Empty : $"{pair.Key}_";
                ituffWriterMock.Setup(o => o.SetTnamePostfix($"_{prefix}HRY_RAWSTR_1"));
                ituffWriterMock.Setup(o => o.SetData(pair.Value));

                allMocks.Add(ituffWriterMock);
                datalogServiceMock.InSequence(sequence).Setup(o => o.GetItuffStrgvalWriter()).Returns(ituffWriterMock.Object);
                datalogServiceMock.Setup(o => o.WriteToItuff(ituffWriterMock.Object));

                if (!string.IsNullOrEmpty(sharedStorageKey))
                {
                    sharedStorageMock.Setup(o => o.InsertRowAtTable($"{sharedStorageKey}_{pair.Key}", pair.Value, Context.DUT));
                }
            }

            Prime.Services.DatalogService = datalogServiceMock.Object;
            allMocks.Add(datalogServiceMock);

            return allMocks;
        }

        private Mock<ITestProgramService> MockTestInstanceName(string name)
        {
            var testprogramMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramMock.Setup(o => o.GetCurrentTestInstanceName()).Returns(name);
            Prime.Services.TestProgramService = testprogramMock.Object;
            return testprogramMock;
        }

        private Mock<IUserVarService> MockUserVarsVerifyOnly(string commaSeparatedListOfUserVar)
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            foreach (var userVar in commaSeparatedListOfUserVar.Split(','))
            {
                userVarServiceMock.Setup(o => o.Exists(userVar)).Returns(true);
            }

            Prime.Services.UserVarService = userVarServiceMock.Object;
            return userVarServiceMock;
        }

        private List<Mock> MockFunctionalTestVerifyOnly(string plist, string levels, string timings, string pins, string preplist)
        {
            var funcTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            /* functionalServiceMock.Setup(o => o.CreateCaptureCtvPerPinTest(plist, levels, timings, pins.Split(',').ToList(), preplist)).Returns(funcTestMock.Object); */
            functionalServiceMock.Setup(o => o.CreateCaptureCtvPerPinTest(plist, levels, timings, It.Is<List<string>>((List<string> x) => string.Join(",", x) == pins), preplist)).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = functionalServiceMock.Object;
            return new List<Mock> { funcTestMock, functionalServiceMock };
        }

        private Mock<IUserVarService> MockUserVarsForExecute(Dictionary<string, int> userVars)
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            foreach (var pair in userVars)
            {
                userVarServiceMock.Setup(o => o.Exists(pair.Key)).Returns(true);
                userVarServiceMock.Setup(o => o.SetValue(pair.Key, 1)); /* inital bypass set at the beginning of execute. */
                userVarServiceMock.Setup(o => o.SetValue(pair.Key, pair.Value)); /* final bypass set based on hry data. */
            }

            Prime.Services.UserVarService = userVarServiceMock.Object;
            return userVarServiceMock;
        }

        private List<Mock> MockFunctionalTestForExecute(string plist, string levels, string timings, string pins, string preplist, string maskPins, Dictionary<string, string> captCtv)
        {
            var funcTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.Execute()).Returns(true);
            funcTestMock.Setup(o => o.GetCtvData()).Returns(captCtv);
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.SetPinMask(It.Is<List<string>>((List<string> x) => string.Join(",", x) == maskPins)));

            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            functionalServiceMock.Setup(o => o.CreateCaptureCtvPerPinTest(plist, levels, timings, It.Is<List<string>>((List<string> x) => string.Join(",", x) == pins), preplist)).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = functionalServiceMock.Object;
            return new List<Mock> { funcTestMock, functionalServiceMock };
        }
    }
}
