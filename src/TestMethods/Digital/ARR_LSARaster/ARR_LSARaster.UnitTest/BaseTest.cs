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

namespace LSARasterTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;
    using Prime.UserVarService;

    /// <summary>
    /// Base test class that implements some of the basic Prime services needed across most other unit tests.
    /// </summary>
    [TestClass]
    public class BaseTest
    {
        private static object fakeStorage;
        private static Mock<IConsoleService> mockConsole = new Mock<IConsoleService>();
        private static Mock<ISharedStorageService> mockSharedStorage = new Mock<ISharedStorageService>();
        private static Mock<IFileService> mockFile = new Mock<IFileService>();
        private static Mock<ITestProgramService> mockTestProgram = new Mock<ITestProgramService>();

        /// <summary>
        /// Dummy summary.
        /// </summary>
        [TestInitialize]
        public virtual void Init()
        {
            mockFile.Setup(x => x.FileExists(It.IsAny<string>()))
                 .Returns((string filepath) => { return System.IO.File.Exists(filepath); });
            mockFile.Setup(x => x.GetFile(It.IsAny<string>()))
                .Returns((string filepath) => { return filepath; });
            Prime.Services.FileService = mockFile.Object;

            mockConsole.Setup(x => x.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => { Console.WriteLine($"ERROR: {msg}"); });
            mockConsole.Setup(x => x.PrintDebug(It.IsAny<string>()))
                .Callback<string>((string msg) => { Console.WriteLine($"{msg}"); });
            Prime.Services.ConsoleService = mockConsole.Object;

            mockSharedStorage.Setup(x => x.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>()))
                .Callback((string key, object item, Context context) => { fakeStorage = item; });
            mockSharedStorage.Setup(x => x.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>())).Returns(fakeStorage);
            Prime.Services.SharedStorageService = mockSharedStorage.Object;

            mockTestProgram.Setup(x => x.IsClassTestSocket()).Returns(false);
            Prime.Services.TestProgramService = mockTestProgram.Object;
        }

        /// <summary>
        /// Cleanup after every single test.
        /// </summary>
        [TestCleanup]
        public virtual void CleanUp()
        {
            fakeStorage = null;
            Prime.Services.FileService = mockFile.Object;
            Prime.Services.ConsoleService = mockConsole.Object;
            Prime.Services.SharedStorageService = mockSharedStorage.Object;
        }
    }
}
