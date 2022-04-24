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

namespace CtvDecoder.UnitTest
{
    using System;
    using System.Collections.Generic;
    using global::CtvDecoder.UnitTest.TestInputFiles;
    using global::CtvServices.ConfigurationFile;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.FileService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class CtvDecoder_Verify_UnitTest : CtvDecoder
    {
        private const string TestFileGenericPass = "TestFileGenericPass.csv";
        private const string TestFilePerBitCodification = "TestFilePerBitCodification.csv";
        private const string TestFileSameFieldName = "TestFileSameFieldName.csv";
        private const string TestFileEmpty = "TestFileEmpty.csv";
        private const string TestFileBase2SharedStorage = "TestFileBase2SharedStorage.csv";
        private const string TestFilePinFinderFormatFail = "TestFilePinFinderFormatFail.csv";

        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IFileHandler> fileHandlerMock;
        private Mock<IFileService> fileServiceMock;

        /// <summary>
        /// Since the contents of "Verify()" were moved to "CustomVerify()", need to do this setup for everything.
        /// </summary>
        [TestInitialize]
        public void SetupCommonMocks()
        {
            var funcServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Loose);
            var funcTestkMock = new Mock<Prime.FunctionalService.ICaptureCtvPerPinTest>(MockBehavior.Loose);
            funcServiceMock.Setup(o => o.CreateCaptureCtvPerPinTest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), string.Empty))
                .Returns(funcTestkMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_CustomVerify_ParamPinRenameWithDifferentCountFromCtvCapturePins_False()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            Services.ConsoleService = consoleServiceMock.Object;

            var toTest = new CtvDecoder
            {
                CtvCapturePins = "PIN1,PIN2,PIN3",
                TssidRename = "PIN11,PIN12",
                ConfigurationFile = " ",
            };

            var ex = Assert.ThrowsException<TestMethodException>(() => toTest.CustomVerify());
            Assert.AreEqual(
                "Parameter tssidRename have different item count from ctvCapturePins. Expected count: [3] tssidRename count: [2]", ex.Message);
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_CustomVerify_SameFieldName_FNeg1()
        {
            this.Setup(TestFileSameFieldName, "Strict");

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileSameFieldName;

            Assert.ThrowsException<TestMethodException>(() => this.CustomVerify());
            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_CustomVerify_CombinedPerBitWithCodification_FNeg1()
        {
            this.Setup(TestFilePerBitCodification, "Strict");

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFilePerBitCodification;

            Assert.ThrowsException<TestMethodException>(() => this.CustomVerify());
            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_CustomVerify_Base2SharedStorageTypeInt_FNeg1()
        {
            this.Setup(TestFileBase2SharedStorage, "Strict");

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileBase2SharedStorage;

            Assert.ThrowsException<TestMethodException>(() => this.CustomVerify());
            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_CustomVerify_CtvCapturePinMissmatch_FNeg1()
        {
            this.Setup(TestFileGenericPass, "Strict");

            // Configuration
            this.CtvCapturePins = "Testing_Pin";
            this.TssidRename = string.Empty;
            this.ConfigurationFile = TestFileGenericPass;

            Assert.ThrowsException<TestMethodException>(() => this.CustomVerify());
            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_CustomVerify_EmptyFile_FNeg1()
        {
            this.Setup(TestFileEmpty, "Strict");

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = string.Empty;
            this.ConfigurationFile = TestFileEmpty;

            Assert.ThrowsException<TestMethodException>(() => this.CustomVerify());
            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_CustomVerify_PinFinderFormatColumnNotFound_FNeg1()
        {
            this.Setup(TestFilePinFinderFormatFail, "Strict");

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFilePinFinderFormatFail;

            Assert.ThrowsException<TestMethodException>(() => this.CustomVerify());
            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_CustomVerify_ReadConfigurationFile_True()
        {
            // Mocks
            var fileHandlerMock = new Mock<IFileHandler>(MockBehavior.Strict);
            var configurationFileContent = InputFileReader.ReadResourceFile(TestFileGenericPass);
            fileHandlerMock.Setup(x => x.ReadAllLines(TestFileGenericPass)).Returns(configurationFileContent);
            this.CtvServices.FileHandler = fileHandlerMock.Object;

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(x => x.GetFile(TestFileGenericPass))
                .Returns(TestFileGenericPass);
            Services.FileService = fileServiceMock.Object;

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(x => x.PrintDebug("[INFO] This is the ConfigurationFile [TestFileGenericPass.csv]")).Callback((string s) => Console.WriteLine(s));

            Services.ConsoleService = consoleServiceMock.Object;

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50,DDR1_DQ51";
            this.TssidRename = "DDR1_DQ50,DDR1_DQ51";
            this.ConfigurationFile = TestFileGenericPass;

            // Execution & Assert
            this.CustomVerify();

            // Mock Verify
            fileHandlerMock.VerifyAll();
            consoleServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
        }

        private void Setup(string fileName, string consoleMockMode)
        {
            // Mocks
            this.fileHandlerMock = new Mock<IFileHandler>(MockBehavior.Strict);

            var configurationFileContent = InputFileReader.ReadResourceFile(fileName);
            this.fileHandlerMock.Setup(x => x.ReadAllLines(fileName)).Returns(configurationFileContent);
            this.CtvServices.FileHandler = this.fileHandlerMock.Object;

            this.fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            this.fileServiceMock.Setup(x => x.GetFile(fileName))
                .Returns(fileName);
            Services.FileService = this.fileServiceMock.Object;

            if (consoleMockMode == "Loose")
            {
                this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            }
            else if (consoleMockMode == "Strict")
            {
                this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            }

            this.consoleServiceMock.Setup(x => x.PrintDebug($"[INFO] This is the ConfigurationFile [{fileName}]")).Callback((string s) => Console.WriteLine(s));
            Services.ConsoleService = this.consoleServiceMock.Object;
        }
    }
}
