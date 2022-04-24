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

namespace VminAggregatorTC.UnitTest
{
    using System.IO.Abstractions.TestingHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.DffService;
    using Prime.FileService;
    using Prime.SharedStorageService;
    using Prime.UserVarService;

    /// <summary>
    /// PowerSequenceHandler_UnitTest.
    /// </summary>
    [TestClass]
    public class VminAggregatorTC_UnitTest : VminAggregatorTC
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<ISharedStorageService> sharedStorageMock;
        private Mock<IUserVarService> userVarServiceMock;
        private Mock<IDffService> dffServiceMock;
        private Mock<IDatalogService> datalogServiceMock;
        private Mock<IStrgvalFormat> strgValWriter;

        /// <summary>
        /// TestInitialize.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(
                    o => o.PrintDebug(It.IsAny<string>())).
                Callback((string msg) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });

            Prime.Services.ConsoleService = this.consoleServiceMock.Object;
            this.sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = this.sharedStorageMock.Object;
            this.userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            Prime.Services.UserVarService = this.userVarServiceMock.Object;
            this.dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            Prime.Services.DffService = this.dffServiceMock.Object;
            this.datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            Prime.Services.DatalogService = this.datalogServiceMock.Object;
            this.strgValWriter = new Mock<IStrgvalFormat>(MockBehavior.Strict);
        }

        /// <summary>
        /// Execute_Basic_Pass.
        /// </summary>
        [TestMethod]
        public void Execute_Basic_Pass()
        {
            this.InputFile = "SomeFile";
            const string contents =
@"
[
    {
        'Domain': 'CORE',
        'Corner': 'F1',
        'Frequency': '[Collection.Uservar]',
        'VminExpressions': [
            ['[G.U.D.ARR_Core1]', '[G.U.D.FUN_Core1]', '[undefined]'],
            ['[G.U.D.ARR_Core0]', '[G.U.D.FUN_Core0]']
        ],
        'DffToken': 'COREF1'
    },
    {
        'Domain': 'CCF',
        'Corner': 'F1',
        'Frequency': '\'0.8GHz\'',
        'VminExpressions': [
            ['[G.U.D.ARR_CCF]', '[G.U.D.FUN_CCF]']
        ]
    }
]
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(contents);
            fileSystemMock.AddFile("SomeFile", mockFile);
            this.FileSystem_ = fileSystemMock;
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists(this.InputFile.ToString())).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile");
            Prime.Services.FileService = fileService.Object;

            this.datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.strgValWriter.Object);
            this.sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("ARR_Core1", Context.DUT)).Returns(true);
            this.sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("ARR_Core0", Context.DUT)).Returns(true);
            this.sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("FUN_Core1", Context.DUT)).Returns(true);
            this.sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("FUN_Core0", Context.DUT)).Returns(true);
            this.sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("ARR_CCF", Context.DUT)).Returns(true);
            this.sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("FUN_CCF", Context.DUT)).Returns(true);
            this.sharedStorageMock.Setup(o => o.GetDoubleRowFromTable("ARR_Core1", Context.DUT)).Returns(0.66D);
            this.sharedStorageMock.Setup(o => o.GetDoubleRowFromTable("ARR_Core0", Context.DUT)).Returns(0.77D);
            this.sharedStorageMock.Setup(o => o.GetDoubleRowFromTable("FUN_Core1", Context.DUT)).Returns(-9999D);
            this.sharedStorageMock.Setup(o => o.GetDoubleRowFromTable("FUN_Core0", Context.DUT)).Returns(0.79D);
            this.sharedStorageMock.Setup(o => o.GetDoubleRowFromTable("ARR_CCF", Context.DUT)).Returns(0.75D);
            this.sharedStorageMock.Setup(o => o.GetDoubleRowFromTable("FUN_CCF", Context.DUT)).Returns(0.7D);

            this.userVarServiceMock.Setup(o => o.Exists("Collection.Uservar")).Returns(true);
            this.userVarServiceMock.Setup(o => o.GetStringValue("Collection.Uservar")).Returns("2.3GHz");

            this.strgValWriter.Setup(o => o.SetTnamePostfix("|CORE@F1"));
            this.strgValWriter.Setup(o => o.SetTnamePostfix("|CCF@F1"));
            this.strgValWriter.Setup(o => o.SetData("2.300@-9999|0.790"));
            this.strgValWriter.Setup(o => o.SetData("0.800@0.750"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgValWriter.Object));

            this.dffServiceMock.Setup(o => o.SetDff("COREF1", "2.300@-9999|0.790"));

            this.Verify();
            var executeResult = this.Execute();
            Assert.AreEqual(1, executeResult);

            this.datalogServiceMock.VerifyAll();
            this.strgValWriter.VerifyAll();
            this.sharedStorageMock.VerifyAll();
            this.userVarServiceMock.VerifyAll();
        }
    }
}
