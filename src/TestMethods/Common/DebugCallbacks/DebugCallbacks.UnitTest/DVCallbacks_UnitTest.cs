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
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.FileService;
    using Prime.UserVarService;

    /// <summary>
    /// Defines the <see cref="DVCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class DVCallbacks_UnitTest
    {
        private Mock<IFileService> fileServiceMock;

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
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("_UserVars", "PrintServiceFileToItuff")).Returns(false);
            Prime.Services.UserVarService = userVarMock.Object;
            this.fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            Prime.Services.FileService = this.fileServiceMock.Object;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void JsonRun_Fail()
        {
            this.fileServiceMock.Setup(o => o.FileExists("./demo.json")).Returns(false);
            this.fileServiceMock.Setup(o => o.GetFile("./demo.json")).Returns("./demo.json");
            var result = Assert.ThrowsException<FileNotFoundException>(() => DV.JsonRun("./demo.json"));
            Assert.AreEqual("Could not load file or assembly 'JsonRunCLR.dll' or one of its dependencies. The specified module could not be found.", result.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void JsonRecorder_Fail()
        {
            var result = Assert.ThrowsException<FileNotFoundException>(() => DV.JsonRecorder(string.Empty));
            Assert.AreEqual("Could not load file or assembly 'JsonRunCLR.dll' or one of its dependencies. The specified module could not be found.", result.Message);
        }
    }
}
