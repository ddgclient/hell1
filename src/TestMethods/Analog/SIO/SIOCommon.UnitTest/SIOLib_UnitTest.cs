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

namespace SIOCommon.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.TestProgramService;
    using Prime.TpSettingsService;
    using Prime.UserVarService;
    using SIO;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class SIOLib_UnitTest
    {
        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        /// <summary>
        /// Setup the common mocks for all the tests.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => Console.WriteLine(s));
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => Console.WriteLine(msg));
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;
        }

        /// <summary>
        /// Test the UserFile parsing.
        /// </summary>
        [TestMethod]
        public void GetFile_Pass()
        {
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.GetFile("./dummyfilename.txt")).Returns("dummyfilename.txt");
            Prime.Services.FileService = fileServiceMock.Object;

            var localFileName = SIO.SIOLib.GetFile("./dummyfilename.txt");
            Assert.AreEqual("dummyfilename.txt", localFileName);
            fileServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the UserFile parsing.
        /// </summary>
        [TestMethod]
        public void GetFile_Exception()
        {
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.GetFile("./dummyfilename.txt")).Throws(new Prime.Base.Exceptions.FatalException("Some Exception Message"));
            Prime.Services.FileService = fileServiceMock.Object;

            var localFileName = SIO.SIOLib.GetFile("dummyfilename.txt");
            Assert.AreEqual(string.Empty, localFileName);
            this.ConsoleServiceMock.Verify(o => o.PrintError("GetFile(./dummyfilename.txt) threw an exception.\nSome Exception Message", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            fileServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the UserFile parsing.
        /// </summary>
        [TestMethod]
        public void GetFile_Fail()
        {
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.GetFile("C:/Full/PathToFile/dummyfilename.txt")).Returns(string.Empty);
            Prime.Services.FileService = fileServiceMock.Object;

            var localFileName = SIO.SIOLib.GetFile("C:/Full/PathToFile/dummyfilename.txt");
            Assert.AreEqual(string.Empty, localFileName);
            this.ConsoleServiceMock.Verify(o => o.PrintError("GetFile(C:/Full/PathToFile/dummyfilename.txt) returned an empty string.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void BinToBase32_WithPadding_Pass()
        {
            var binNum = "0";
            var sio = new SIOLib(true);
            var base32 = sio._BinToBase32(binNum);
            Assert.AreEqual("EA", base32);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void BinToBase32_EmptyString_Fail()
        {
            var binNum = string.Empty;
            var sio = new SIOLib(true);
            var base32 = sio._BinToBase32(binNum);
            Assert.AreEqual(string.Empty, base32);
            this.ConsoleServiceMock.Verify(o => o.PrintError("BinToBase32: Cannot convert an empty string to base32.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void BinToBase32_IllegalChar_Fail()
        {
            var binNum = "i";
            var sio = new SIOLib(true);
            var base32 = sio._BinToBase32(binNum);
            Assert.AreEqual(string.Empty, base32);
            this.ConsoleServiceMock.Verify(o => o.PrintError("BinToBase32: Error Data=[i] is not binary.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void SplitStrInParts_Exceptions()
        {
            var ex1 = Assert.ThrowsException<ArgumentNullException>(() => SIOLib.SplitStrInParts(null, 1, 1).ToList());
            Assert.AreEqual("In SplitStrInParts, base string is null.\r\nParameter name: str", ex1.Message);

            var ex2 = Assert.ThrowsException<ArgumentException>(() => SIOLib.SplitStrInParts("AAAAAA", -1, 1).ToList());
            Assert.AreEqual("In SplitStrInParts, Partition length has to be positive.\r\nParameter name: partLength", ex2.Message);

            var ex3 = Assert.ThrowsException<ArgumentException>(() => SIOLib.SplitStrInParts("AAAAAA", 10, -1).ToList());
            Assert.AreEqual("In SplitStrInParts, offset has to be positive or 0.\r\nParameter name: offset", ex3.Message);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void MessageAlways_Pass()
        {
            var sio = new SIOLib(true);
            sio.MsgToConsole(MsgEnum.SIO_ALWAYS, "SomeMessage");
            this.ConsoleServiceMock.Verify(o => o.PrintError("[SIO_ALWAYS] SomeMessage", 0, " ", " "), Times.Once);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void ResultStrgValToDatalog_Empty_Fail()
        {
            var sio = new SIOLib(true);
            Assert.IsFalse(sio.ResultStrgValToDatalog("somename", string.Empty));
            this.ConsoleServiceMock.Verify(o => o.PrintError("ResultStrgValToDatalog: Data Length <= 0 [0]", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void ResultStrgValToDatalog_Exception()
        {
            var datalogService = new Mock<IDatalogService>(MockBehavior.Strict);
            var strgvalWriter = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strgvalWriter.Setup(o => o.SetCustomTname("somename0"));
            strgvalWriter.Setup(o => o.SetData("somedata"));
            strgvalWriter.Setup(o => o.SetDelimiterCharacterForWrap('!'));
            datalogService.Setup(o => o.GetItuffStrgvalWriter()).Returns(strgvalWriter.Object);
            datalogService.Setup(o => o.WriteToItuff(strgvalWriter.Object)).Throws(new Prime.Base.Exceptions.FatalException("Some Prime Message"));
            /* datalogService.Setup(o => o.WriteToItuff("2_lsep\n2_tname_somename0\n2_strgval_somedata\n")).Throws(new Prime.Base.Exceptions.FatalException("Some Prime Message")); */
            Prime.Services.DatalogService = datalogService.Object;

            var sio = new SIOLib(true);
            Assert.IsFalse(sio.ResultStrgValToDatalog("somename", "somedata"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("ResultStrgValToDatalog: Failed writing to ituff\nSome Prime Message", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            strgvalWriter.VerifyAll();
            datalogService.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void ShmooTestSetup_NoUserFile_Fail()
        {
            var sio = new SIOLib(true);
            Assert.IsFalse(sio.ShmooTestSetup(null, "token"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("In ShmooTestSetup, invalid UserDataFile.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void ShmooTestSetup_MissingToken_Fail()
        {
            var sio = new SIOLib(true);
            Assert.IsFalse(sio.ShmooTestSetup(new UserFile(), "token"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("In ShmooTestSetup UserDataFile does not contain token=[token].  Valid Tokens are []", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void ShmooTestSetup_NoCompareBlock_Fail()
        {
            var testprogramService = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramService.Setup(o => o.GetTestInstanceParameter("SomePrimeTest", "patlist")).Throws(new Prime.Base.Exceptions.FatalException("No matching parameter"));
            testprogramService.Setup(o => o.GetTestInstanceParameter("SomePrimeTest", "Patlist")).Returns("SomePList");
            Prime.Services.TestProgramService = testprogramService.Object;

            var sio = new SIOLib(true);
            var userFile = new UserFile();
            var tokenData = new UserFile.UserData("token");
            tokenData.TestType = "PERPORT";
            tokenData.ExecuteTest = "SomePrimeTest";
            userFile.TokenBlocks.Add("token", tokenData);

            Assert.IsFalse(sio.ShmooTestSetup(userFile, "token"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("ShmooTestSetup: Token=[token] TestType=[PERPORT] requires CompareBlock be set.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            testprogramService.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void ShmooTestSetup_CompareBlockNotImplemented_Fail()
        {
            var testprogramService = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramService.Setup(o => o.GetTestInstanceParameter("SomePrimeTest", "patlist")).Throws(new Prime.Base.Exceptions.FatalException("No matching parameter"));
            testprogramService.Setup(o => o.GetTestInstanceParameter("SomePrimeTest", "Patlist")).Returns("SomePList");
            Prime.Services.TestProgramService = testprogramService.Object;

            var sio = new SIOLib(true);
            var userFile = new UserFile();
            var tokenData = new UserFile.UserData("token");
            tokenData.TestType = "PERPORT";
            tokenData.ExecuteTest = "SomePrimeTest";
            tokenData.CompareBlock = "SomeCompareBlock";
            userFile.TokenBlocks.Add("token", tokenData);

            Assert.IsFalse(sio.ShmooTestSetup(userFile, "token"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("ShmooTestSetup: Token=[token] TestType=[PERPORT] Compare Block functionality is not implemented yet.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            testprogramService.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void ShmooTestSetup_ShmooAxisTiming_Fail()
        {
            var testprogramService = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramService.Setup(o => o.GetTestInstanceParameter("SomePrimeTest", "patlist")).Throws(new Prime.Base.Exceptions.FatalException("No matching parameter"));
            testprogramService.Setup(o => o.GetTestInstanceParameter("SomePrimeTest", "Patlist")).Returns("SomePList");
            Prime.Services.TestProgramService = testprogramService.Object;

            var sio = new SIOLib(true);
            var userFile = new UserFile();

            var shmooAxis = new UserFile.ShmooAxis("XShmoo", true);
            shmooAxis.Type = "TIMING";

            var tokenData = new UserFile.UserData("token");
            tokenData.TestType = "GONOGO";
            tokenData.ExecuteTest = "SomePrimeTest";
            tokenData.ShmooAxis.Add("XShmoo", shmooAxis);
            userFile.TokenBlocks.Add("token", tokenData);

            Assert.IsFalse(sio.ShmooTestSetup(userFile, "token"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("ShmooTestSetup: Token=[token] TestType=[GONOGO] Shmo Axis=[TIMING] is not implemented yet.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            testprogramService.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void ShmooTestSetup_NoSteps_Fail()
        {
            var testprogramService = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramService.Setup(o => o.GetTestInstanceParameter("SomePrimeTest", "patlist")).Throws(new Prime.Base.Exceptions.FatalException("No matching parameter"));
            testprogramService.Setup(o => o.GetTestInstanceParameter("SomePrimeTest", "Patlist")).Returns("SomePList");
            Prime.Services.TestProgramService = testprogramService.Object;

            var sio = new SIOLib(true);
            var userFile = new UserFile();

            var shmooAxis = new UserFile.ShmooAxis("XShmoo", true);
            shmooAxis.Type = "PATMOD";
            shmooAxis.NumSteps = -1;
            shmooAxis.Resolution = 0;

            var tokenData = new UserFile.UserData("token");
            tokenData.TestType = "GONOGO";
            tokenData.ExecuteTest = "SomePrimeTest";
            tokenData.ShmooAxis.Add("XShmoo", shmooAxis);
            userFile.TokenBlocks.Add("token", tokenData);

            Assert.IsFalse(sio.ShmooTestSetup(userFile, "token"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("ShmooTestSetup: Token=[token] Axis=[XShmoo] invalid NumSteps=[-1] or Resolution=[0] both should be > 0.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            testprogramService.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void ShmooTestSetup_Pass()
        {
            var testprogramService = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramService.Setup(o => o.GetTestInstanceParameter("SomePrimeTest", "patlist")).Throws(new Prime.Base.Exceptions.FatalException("No matching parameter"));
            testprogramService.Setup(o => o.GetTestInstanceParameter("SomePrimeTest", "Patlist")).Returns("SomePList");
            Prime.Services.TestProgramService = testprogramService.Object;

            var sio = new SIOLib(true);
            var userFile = new UserFile();

            var shmooAxis = new UserFile.ShmooAxis("XShmoo", true);
            shmooAxis.Type = "PATMOD";
            shmooAxis.NumSteps = 1;
            shmooAxis.Resolution = 1;

            var tokenData = new UserFile.UserData("token");
            tokenData.TestType = "GONOGO";
            tokenData.ExecuteTest = "SomePrimeTest";
            tokenData.ShmooAxis.Add("XShmoo", shmooAxis);
            userFile.TokenBlocks.Add("token", tokenData);

            Assert.IsTrue(sio.ShmooTestSetup(userFile, "token"));

            testprogramService.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void ShmooStateToToken_IPOUSERVARbadformat_Fail()
        {
            // ShmooStateToToken(Dictionary<string, string> currentState, bool excludeInnerAxis)
            var sio = new SIOLib(true);
            var currentState = new Dictionary<string, string>() { { "IPOUSERVAR", "MalformedText" }, { "X", "GoodState" } };
            foreach (var axis in new List<string> { "R", "S", "T", "U", "V", "W", "Z", "Y" })
            {
                currentState[axis] = string.Empty;
            }

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => sio.ShmooStateToToken(currentState, true));
            Assert.AreEqual("Cannot split uservar into collection/variable pair. IPOUSERVAR=[MalformedText].  Expecting [<collection>.<variable>]", ex.Message);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void ShmooStateToToken_Pass()
        {
            var userVarService = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarService.Setup(o => o.GetStringValue("Collection", "Name")).Returns("A\n0,1,timing,3,4,5,6,7,8");
            Prime.Services.UserVarService = userVarService.Object;

            var sio = new SIOLib(true);
            var currentState = new Dictionary<string, string>() { { "IPOUSERVAR", "Collection.Name" }, { "X", "GoodState" }, { "RUN", string.Empty } };
            foreach (var axis in new List<string> { "R", "S", "T", "U", "V", "W", "Z", "Y" })
            {
                currentState[axis] = string.Empty;
            }

            Assert.AreEqual("GoodState;4:5", sio.ShmooStateToToken(currentState, true));
            userVarService.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void SetupTestParam_Exceptions()
        {
            // public bool SetupTestParam(string key, string step, string type, string test, string patModifyTest)
            var sio = new SIOLib(true);
            var ex1 = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => sio.SetupTestParam("Key", "step", "LEVEL", "test", "patmod"));
            Assert.AreEqual("LEVEL type not supported in SetupTestParam.", ex1.Message);

            var ex2 = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => sio.SetupTestParam("Key", "step", "TIMING", "test", "patmod"));
            Assert.AreEqual("TIMING type not supported in SetupTestParam.", ex2.Message);

            var ex3 = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => sio.SetupTestParam("Key", "step", "PARAMSTR", "test", "patmod"));
            Assert.AreEqual("PARAMSTR type not supported in SetupTestParam.", ex3.Message);

            var ex4 = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => sio.SetupTestParam("Key", "step", "NOTAREALTYPE", "test", "patmod"));
            Assert.AreEqual("In SetupTestParam: Type=[NOTAREALTYPE] is not supported.", ex4.Message);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunSetupTest_NoUserFile_Fail()
        {
            var sio = new SIOLib(true);
            Assert.IsFalse(sio.RunSetupTest(null, "token"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("In RunSetup, invalid UserDataFile.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunSetupTest_MissingModifyToken_Fail()
        {
            var sio = new SIOLib(true);
            var userData = new UserFile.UserData("token");

            var userFile = new UserFile();
            userFile.TokenBlocks.Add("token", userData);

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => sio.RunSetupTest(userFile, "token"));
            Assert.AreEqual("In RunSetupTest: Token=[token] Both MODIFY_TOKEN=[] and MODIFY_VALUE=[] need values.", ex.Message);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunSetupTest_MismatchValuesAndTokens_Fail()
        {
            var sio = new SIOLib(true);
            var userData = new UserFile.UserData("token");
            userData.ModifyToken = "token1";
            userData.ModifyValue = "value1!value2";

            var userFile = new UserFile();
            userFile.TokenBlocks.Add("token", userData);

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => sio.RunSetupTest(userFile, "token"));
            Assert.AreEqual("In RunSetupTest: Token=[token] Both MODIFY_TOKEN=[token1] and MODIFY_VALUE=[value1!value2] need the same number of values.", ex.Message);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunSetupTest_UserToken_Pass()
        {
            var sio = new SIOLib(true);
            var userData = new UserFile.UserData("token");
            userData.ModifyToken = ":token1";
            userData.ModifyValue = "value1";

            var userFile = new UserFile();
            userFile.TokenBlocks.Add("token", userData);

            Assert.AreEqual(true, sio.RunSetupTest(userFile, "token"));
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunSetupTest_NoGlobal_Fail()
        {
            var sio = new SIOLib(true);
            var userFile = new UserFile();
            Assert.AreEqual(false, sio.RunSetupTest(userFile, "token"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("In RunSetup, UserDataFile does not contain a GLOBAL_SETUP or [token] token.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunSetupTest_NoPatMod_Fail()
        {
            var sio = new SIOLib(true);
            var userData = new UserFile.UserData("GLOBAL_SETUP");

            var userFile = new UserFile();
            userFile.TokenBlocks.Add("GLOBAL_SETUP", userData);
            Assert.AreEqual(false, sio.RunSetupTest(userFile, "token"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("In RunSetup, PATMOD_TEST is empty for GLOBAL_SETUP.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void BinToBase32_Pass()
        {
            var binNum = "111111001100000000000000000000001000000000010011010011011001011000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
            var sio = new SIOLib(true);
            var data = sio.BinToBase32(binNum);

            Assert.IsNotNull(data);
            Assert.AreNotEqual(string.Empty, data.UncompressedString);
            Assert.AreNotEqual(string.Empty, data.CompressedString);
            Assert.AreNotEqual(string.Empty, data.TranslationTable);

            var uncompressedString = sio.Uncompress(data.CompressedString, data.TranslationTable);
            Assert.AreEqual(data.UncompressedString, uncompressedString);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void CountListElements_Pass()
        {
            string[] testLst = new[]
            {
                "AB", "CD", "EF", "GH", "IJ", "KL", "MN", "OP", "QR", "ST", "UV", "WX", "YZ",
                "AB", "CD", "EF", "GH", "IJ", "IJ", "IJ", "IJ", "IJ", "EF", "EF", "AB",
            };
            Dictionary<string, int> cmpDictFull = new Dictionary<string, int>()
            {
                { "IJ", 6 }, { "EF", 4 }, { "AB", 3 }, { "CD", 2 }, { "GH", 2 }, { "KL", 1 },
                { "MN", 1 }, { "OP", 1 }, { "QR", 1 }, { "ST", 1 }, { "UV", 1 }, { "WX", 1 },
                { "YZ", 1 },
            };
            Dictionary<string, int> cmpDict3 = new Dictionary<string, int>()
            {
                { "IJ", 6 }, { "EF", 4 }, { "AB", 3 },
            };
            var sio = new SIOLib(true);
            var countFull = sio.CountListElements(testLst, 105);
            CollectionAssert.AreEquivalent(cmpDictFull, countFull, "Failed on compare with all elements.");

            var count3 = sio.CountListElements(testLst, 3);
            CollectionAssert.AreEquivalent(cmpDict3, count3, "Failed on compare with 3 elements.");
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunShmooSinglePoint_Exception()
        {
            var testprogramService = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramService.Setup(o => o.ExecuteTestInstance("SomeTest")).Returns(1);
            Prime.Services.TestProgramService = testprogramService.Object;

            var sio = new SIOLib(true);
            var userData = new UserFile.UserData("token");
            userData.ExecuteTest = "SomeTest";
            userData.TestInstanceIsShmoo = false;
            var currentState = new Dictionary<string, string> { { "RUN", "STATE" } };

            userData.TestType = "PERPORT";
            var ex1 = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => sio.RunShmooSinglePoint(userData, currentState));
            Assert.AreEqual("TestType=PERPORT is currently not supported for shmoo.", ex1.Message);

            userData.TestType = "NOTVALID";
            var ex2 = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => sio.RunShmooSinglePoint(userData, currentState));
            Assert.AreEqual("TestType=NOTVALID is currently not supported for shmoo.", ex2.Message);
        }
    }
}