﻿// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.SharedStorageService;
    using Prime.TestMethods.Functional;
    using Prime.TpSettingsService;

    /// <summary>
    /// Unit test class.
    /// </summary>
    [TestClass]
    public class CtvDecoder_Execute_SharedStorage_UnitTest : CtvDecoder
    {
        private const string TestFileSharedStoragePass = "TestFileSharedStoragePass.csv";
        private const string TestFileSharedStoragePassAllDataTypes = "TestFileSharedStoragePassAllDataTypes.csv";
        private const string TestFileSharedStorageFail = "TestFileSharedStorageFail.csv";

        private Mock<IConsoleService> consoleServiceMock;

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
        /// Execute unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_Execute_SharedStorage_Pass()
        {
            this.Setup(TestFileSharedStoragePass);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileSharedStoragePass;
            this.CsvDelimiter = ";";

            // input setup
            const string dataPinDDR1DQ50 = "00100100101010111101011011";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Shared storage Mocks
            var sharedStorageServiceMock1 = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock1.Setup(x => x.InsertRowAtTable("CtvDecoder_DDR1_DQ50.V0.training.subseq1.val1", 36, Context.DUT));
            sharedStorageServiceMock1.Setup(x => x.KeyExistsInIntegerTable("CtvDecoder_DDR1_DQ50.V0.training.subseq1.val1", Context.DUT)).Returns(true);
            sharedStorageServiceMock1.Setup(x => x.GetIntegerRowFromTable("CtvDecoder_DDR1_DQ50.V0.training.subseq1.val1", Context.DUT)).Returns(36);
            sharedStorageServiceMock1.Setup(x => x.InsertRowAtTable("CtvDecoder_DDR1_DQ50.V0.training.subseq1.val6", "F", Context.DUT));
            sharedStorageServiceMock1.Setup(x => x.InsertRowAtTable("CtvDecoder_DDR1_DQ50.V0.training.subseq1.val7", "12", Context.DUT));
            sharedStorageServiceMock1.Setup(x => x.InsertRowAtTable("CtvDecoder_DDR1_DQ50.V0.training.subseq1.val8", "1101", Context.DUT));
            sharedStorageServiceMock1.Setup(x => x.KeyExistsInIntegerTable("CtvDecoder_DDR1_DQ50.V0.training.subseq1.val1", Context.DUT)).Returns(true);
            sharedStorageServiceMock1.Setup(x => x.GetIntegerRowFromTable("CtvDecoder_DDR1_DQ50.V0.training.subseq1.val1", Context.DUT)).Returns(36);

            Services.SharedStorageService = sharedStorageServiceMock1.Object;

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));
            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1"));
            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("36|41|41|10|1255|F|12|1101"));
            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Services.DatalogService = dataLogServiceMock.Object;

            // Execution & Assert
            this.CustomVerify();
            var toExecute = this as IFunctionalExtensions;
            Assert.AreEqual(true, toExecute.ProcessCtvPerPin(inputCtvData));
            sharedStorageServiceMock1.VerifyAll();
            istrgvalFormat.VerifyAll();
            dataLogServiceMock.VerifyAll();
        }

        private void Setup(string fileName)
        {
            // Mocks
            var fileHandlerMock = new Mock<IFileHandler>(MockBehavior.Strict);
            var configurationFileContent = InputFileReader.ReadResourceFile(fileName);
            fileHandlerMock.Setup(x => x.ReadAllLines(fileName)).Returns(configurationFileContent);
            this.CtvServices.FileHandler = fileHandlerMock.Object;

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(x => x.GetFile(fileName))
                .Returns(fileName);
            Services.FileService = fileServiceMock.Object;

            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(x => x.PrintDebug($"[INFO] This is the ConfigurationFile [{fileName}]")).Callback((string s) => Console.WriteLine(s));
            Services.ConsoleService = this.consoleServiceMock.Object;
        }
    }
}
