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

namespace PatConfigTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions.TestingHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.FileService;
    using Prime.PatConfigService;
    using Prime.SharedStorageService;
    using Prime.UserVarService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class PatConfigTC_UnitTest : PatConfigTC
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IFileService> fileService;
        private Mock<IPatConfigService> patConfigService;
        private Mock<ISharedStorageService> sharedStorage;
        private Mock<IUserVarService> userVarService;

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(
                o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
            {
                System.Console.WriteLine($"DEBUG: {msg}");
            });
            this.consoleServiceMock.Setup(o =>
                    o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            this.fileService = new Mock<IFileService>(MockBehavior.Strict);
            Prime.Services.FileService = this.fileService.Object;

            this.patConfigService = new Mock<IPatConfigService>(MockBehavior.Strict);
            Prime.Services.PatConfigService = this.patConfigService.Object;

            this.sharedStorage = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = this.sharedStorage.Object;

            this.userVarService = new Mock<IUserVarService>(MockBehavior.Strict);
            Prime.Services.UserVarService = this.userVarService.Object;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Passing_Test()
        {
            const string source =
@"
[
    {
        'Tag': 'Tag1',
        'PatConfig': 'conf1',
        'Data': 'GetPatSymbolString(\'0d11\',8)'
    },
    {
        'Tag': 'Tag2',
        'PatConfig': 'conf2',
        'Data': 'Reverse(GetPatSymbolString(\'0xC\', 4))'
    },
    {
        'Tag': 'Tag3',
        'PatConfig': 'conf3',
        'Data': '\'01100\''
    },
    {
        'Tag': 'Tag4',
        'PatConfig': 'conf4',
        'Data': '\'11100\''
    }    
]
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile", mockFile);
            this.FileWrapper = fileSystemMock;
            this.fileService.Setup(o => o.FileExists("SomeFile")).Returns(true);
            this.fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile");

            this.sharedStorage.Setup(o => o.KeyExistsInStringTable("SharedStorageToken", Context.DUT)).Returns(true);
            this.sharedStorage.Setup(o => o.GetStringRowFromTable("SharedStorageToken", Context.DUT)).Returns("Tag2");

            this.userVarService.Setup(o => o.Exists("Collection.Uservar")).Returns(true);
            this.userVarService.Setup(o => o.GetStringValue("Collection.Uservar")).Returns("Tag3");

            var handle1 = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            handle1.Setup(o => o.GetExpectedDataSize()).Returns(8);
            handle1.Setup(o => o.SetData("00001011"));
            var handle2 = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            handle2.Setup(o => o.GetExpectedDataSize()).Returns(8);
            handle2.Setup(o => o.SetData("00000011"));
            var handle3 = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            handle3.Setup(o => o.GetExpectedDataSize()).Returns(8);
            handle3.Setup(o => o.SetData("00001100"));
            var handle4 = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            this.patConfigService.Setup(o => o.GetPatConfigHandle("conf1")).Returns(handle1.Object);
            this.patConfigService.Setup(o => o.GetPatConfigHandle("conf2")).Returns(handle2.Object);
            this.patConfigService.Setup(o => o.GetPatConfigHandle("conf3")).Returns(handle3.Object);
            this.patConfigService.Setup(o => o.GetPatConfigHandle("conf4")).Returns(handle3.Object);
            this.patConfigService.Setup(o => o.Apply(It.IsAny<List<IPatConfigHandle>>()));

            this.InputFile = "SomeFile";
            this.Tags = "'Tag1',[G.U.S.SharedStorageToken],[Collection.Uservar]";
            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(1, result);

            handle1.VerifyAll();
            handle2.VerifyAll();
            handle3.VerifyAll();
            handle4.VerifyAll();
            this.patConfigService.VerifyAll();
            this.sharedStorage.VerifyAll();
            this.userVarService.VerifyAll();
            this.fileService.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Failing_Test()
        {
            const string source =
@"
[
    {
        'Tag': 'Tag1',
        'PatConfig': 'conf1',
        'Data': 'GetPatSymbolString(\'0d11\',8)'
    }    
]
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile", mockFile);
            this.FileWrapper = fileSystemMock;
            this.fileService.Setup(o => o.FileExists("SomeFile")).Returns(true);
            this.fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile");

            var handle1 = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            handle1.Setup(o => o.GetExpectedDataSize()).Returns(1);
            handle1.Setup(o => o.GetConfigurationName()).Returns("conf1");
            this.patConfigService.Setup(o => o.GetPatConfigHandle("conf1", "SomePlist")).Returns(handle1.Object);

            this.InputFile = "SomeFile";
            this.Tags = "'Tag1'";
            this.PlistRegEx = "SomePlist";
            this.Verify();
            var ex = Assert.ThrowsException<Exception>(() => this.Execute());
            Assert.AreEqual("Invalid size. Data=[00001011] is not matching expected size=[1] for configuration=[conf1].", ex.Message);

            handle1.VerifyAll();
            this.patConfigService.VerifyAll();
            this.fileService.VerifyAll();
        }
    }
}