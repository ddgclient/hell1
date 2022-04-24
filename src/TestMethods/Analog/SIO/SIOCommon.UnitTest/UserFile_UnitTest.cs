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
    using System.IO;
    using System.Runtime.CompilerServices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.FileService;
    using SIO;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class UserFile_UnitTest
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
        public void UserFile_ReadAll_Pass()
        {
            var fileServiceMock = MockFileService("sample_user_file_all1.txt", true);

            var newUserFile = new UserFile("sample_user_file_all1.txt");
            Assert.IsNotNull(newUserFile);
            Assert.IsTrue(newUserFile.Valid);

            fileServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the UserFile parsing.
        /// </summary>
        [TestMethod]
        public void UserFile_BadFormats_Fail()
        {
            this.TestBadFileFormat("FailUserFile_CompareBlockNoName.txt", "Invalid START_COMPARE_BLOCK on Line=[START_COMPARE_BLOCK ] at lineNum=[2] File=[FailUserFile_CompareBlockNoName.txt].");
            this.TestBadFileFormat("FailUserFile_CompareBlockNoMissingPortCompareValue.txt", "Invalid Line=[PORTCOMPARE PortToCompare ] at lineNum=[3] File=[FailUserFile_CompareBlockNoMissingPortCompareValue.txt] while reading COMPARE_BLOCK data.");
            this.TestBadFileFormat("FailUserFile_InvalidGlobal.txt", "Invalid Line=[GLOBAL_SETUP      INVALID_GLOBAL_ID  	SOME_MODULE::GLOBAL_PATMOD_TEST] at lineNum=[2] File=[FailUserFile_InvalidGlobal.txt] only PATMODIFY_TEST and PYTHON_TEST are valid for GLOBAL_SETUP, got [INVALID_GLOBAL_ID].");
            this.TestBadFileFormat("FailUserFile_NumTestsRunNotANumber.txt", "Invalid Line=[LOCAL_OPTIONS_SETUP 				NUMOFTESTRUN	      	FAIL_NOT_A_NUMBER] at lineNum=[1] File=[FailUserFile_NumTestsRunNotANumber.txt] Expecting a number for NUMOFTESTRUN, not [FAIL_NOT_A_NUMBER]");
            this.TestBadFileFormat("FailUserFile_BadToken.txt", "Invalid Line=[LOCAL_OPTIONS_SETUP					MptARealToken			MODE!MODE!MODE] at lineNum=[1] File=[FailUserFile_BadToken.txt] Invalid Key=[MptARealToken].");
            this.TestBadFileFormat("FailUserFile_MissingVAlue.txt", "Invalid Line=[LOCAL_OPTIONS_SETUP           		REGDEF] at lineNum=[1] File=[FailUserFile_MissingVAlue.txt] NOT reading COMPARE_BLOCK data and numTokens=[2] Expected 3.");
            this.TestBadFileFormat("FailUserFile_ShmooKeyToManyFields.txt", "Invalid Line=[LOCAL_OPTIONS_SETUP      	RKEY                   KeyValueR:SomeType:ToManyFields] at lineNum=[1] File=[FailUserFile_ShmooKeyToManyFields.txt] Expecting 2 items after split, got [3].");
            this.TestBadFileFormat("FailUserFile_ShmooParamToManyFields.txt", "Invalid Line=[LOCAL_OPTIONS_SETUP			SHMOO_YPARAM_NAME			YparamName:SomeType:toomanyfields] at lineNum=[1] File=[FailUserFile_ShmooParamToManyFields.txt] Expecting 2 items after split, got [3].");
        }

        private static Mock<IFileService> MockFileService(string filename, bool exists)
        {
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.FileExists(filename)).Returns(exists);
            if (exists)
            {
                fileServiceMock.Setup(o => o.GetFile(filename)).Returns((string s) => Path.IsPathRooted(s) ? s : GetPathToFiles() + Path.GetFileName(s));
            }

            Prime.Services.FileService = fileServiceMock.Object;
            return fileServiceMock;
        }

        private static string GetPathToFiles([CallerFilePath] string srcPath = "")
        {
            return Path.GetDirectoryName(srcPath) + "\\input_files\\";
        }

        private void TestBadFileFormat(string filename, string errorMsg)
        {
            var fileServiceMock = MockFileService(filename, true);
            var newUserFile = new UserFile(filename);
            Assert.IsNotNull(newUserFile);
            Assert.IsFalse(newUserFile.Valid);
            this.ConsoleServiceMock.Verify(o => o.PrintError(errorMsg, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
