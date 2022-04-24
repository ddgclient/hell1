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

namespace SIOEDCMainTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.SharedStorageService;
    using Prime.TestMethods;
    using Prime.TestProgramService;
    using Prime.TpSettingsService;

    /// <summary>
    /// Defines the <see cref="SIOEDCMainTC_UnitTest" />.
    /// </summary>
    [TestClass]
    public class SIOEDCMainTC_UnitTest
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
        /// Test the verify function.
        /// </summary>
        [TestMethod]
        public void Verify_MissingFileParam_Fail()
        {
            var underTest = new SIOEDCMainTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserToken = "CNVI_LOOP_MAX_1P28_CAPTURE",
            };

            var ex = Assert.ThrowsException<ArgumentException>(() => underTest.Verify());
            Assert.IsTrue(ex.Message.Contains("[SIO::SomeTestInstanceName] UserFile is a required argument."));
        }

        /// <summary>
        /// Test the verify function.
        /// </summary>
        [TestMethod]
        public void Verify_MissingTokenParam_Fail()
        {
            var underTest = new SIOEDCMainTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "PSIO_cnvi_SKEWCALCTLE_calcode.txt",
            };

            var ex = Assert.ThrowsException<ArgumentException>(() => underTest.Verify());
            Assert.IsTrue(ex.Message.Contains("[SIO::SomeTestInstanceName] UserToken is a required argument."));
        }

        /// <summary>
        /// Test the verify function.
        /// </summary>
        [TestMethod]
        public void Verify_MissingFile_Fail()
        {
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.FileExists("PSIO_cnvi_SKEWCALCTLE_calcode.txt")).Returns(false);
            Prime.Services.FileService = fileServiceMock.Object;

            var underTest = new SIOEDCMainTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "PSIO_cnvi_SKEWCALCTLE_calcode.txt",
                UserToken = "CNVI_LOOP_MAX_1P28_CAPTURE",
            };

            Assert.ThrowsException<FileNotFoundException>(() => underTest.Verify());
            fileServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_PrePostTokensEmptyCapture_Pass()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "PSIO_cnvi_SKEWCALCTLE_calcode.txt", "./CNVI_formatfile.csv" });

            var ituffWriterMock = this.MockDataLog(
                "PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE_EDC_CAPTURE_L0_P0",
                "TOKEN=!Plist=FakePListName!RUN=1!CmpName=NA!TestType=CAPTURE!DataStart##DataEnd!KEY=",
                out var datalogServiceMock);
            /* this.MockDataLog(
                true,
                $"0_tname_PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE_EDC_CAPTURE_L0_P0\n0_strgval_TOKEN=!Plist=FakePListName!RUN=1!CmpName=NA!TestType=CAPTURE!DataStart##DataEnd!KEY=\n",
                out var tpSettingsServiceMock,
                out var datalogServiceMock); */

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", "PRE_RESET_PMOD", Context.LOT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", "POST_RESET_PMOD", Context.LOT));
            sharedStorageMock.Setup(o => o.GetStringRowFromTable("SIO_GSDS_BINDMEMDATA", Context.DUT)).Returns(string.Empty);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var tpService = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpService.Setup(o => o.SetTestInstanceParameter("PSIO_CNVI_ALL1::UTIL_X_PMOD_X_END_X_X_MAX_X_SKEWCALCTLE", "modify_token_dynamic_subset", "G.L.S.SIOGSDS_PATMOD_TOKEN"));
            tpService.Setup(o => o.GetTestInstanceParameter("PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE", "patlist")).Returns("FakePListName");
            tpService.Setup(o => o.ExecuteTestInstance("PSIO_CNVI_ALL1::UTIL_X_PMOD_X_END_X_X_MAX_X_SKEWCALCTLE")).Returns(1);
            tpService.Setup(o => o.ExecuteTestInstance("PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE")).Returns(1);
            Prime.Services.TestProgramService = tpService.Object;

            var underTest = new SIOEDCMainTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "PSIO_cnvi_SKEWCALCTLE_calcode.txt",
                UserToken = "CNVI_LOOP_MAX_1P28_CAPTURE",
                PreTestTokens = "PRE_RESET_PMOD",
                PostTestTokens = "POST_RESET_PMOD",
            };

            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            fileServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            ituffWriterMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            tpService.VerifyAll();
            tpService.Verify(o => o.SetTestInstanceParameter("PSIO_CNVI_ALL1::UTIL_X_PMOD_X_END_X_X_MAX_X_SKEWCALCTLE", "modify_token_dynamic_subset", "G.L.S.SIOGSDS_PATMOD_TOKEN"), Times.Exactly(2));
            tpService.Verify(o => o.ExecuteTestInstance("PSIO_CNVI_ALL1::UTIL_X_PMOD_X_END_X_X_MAX_X_SKEWCALCTLE"), Times.Exactly(2));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_PreTokenFail_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "PSIO_cnvi_SKEWCALCTLE_calcode.txt", "./CNVI_formatfile.csv" });

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", "PRE_RESET_PMOD", Context.LOT));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var tpService = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpService.Setup(o => o.SetTestInstanceParameter("PSIO_CNVI_ALL1::UTIL_X_PMOD_X_END_X_X_MAX_X_SKEWCALCTLE", "modify_token_dynamic_subset", "G.L.S.SIOGSDS_PATMOD_TOKEN"));
            tpService.Setup(o => o.ExecuteTestInstance("PSIO_CNVI_ALL1::UTIL_X_PMOD_X_END_X_X_MAX_X_SKEWCALCTLE")).Returns(0);
            Prime.Services.TestProgramService = tpService.Object;

            var underTest = new SIOEDCMainTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "PSIO_cnvi_SKEWCALCTLE_calcode.txt",
                UserToken = "CNVI_LOOP_MAX_1P28_CAPTURE",
                PreTestTokens = "PRE_RESET_PMOD",
                PostTestTokens = "POST_RESET_PMOD",
            };

            underTest.Verify();
            Assert.AreEqual(0, underTest.Execute());
            this.ConsoleServiceMock.Verify(o => o.PrintError("[SIO::SomeTestInstanceName] Failed to run PreTest Tokens.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            fileServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            tpService.VerifyAll();
            tpService.Verify(o => o.SetTestInstanceParameter("PSIO_CNVI_ALL1::UTIL_X_PMOD_X_END_X_X_MAX_X_SKEWCALCTLE", "modify_token_dynamic_subset", "G.L.S.SIOGSDS_PATMOD_TOKEN"), Times.Exactly(1));
            tpService.Verify(o => o.ExecuteTestInstance("PSIO_CNVI_ALL1::UTIL_X_PMOD_X_END_X_X_MAX_X_SKEWCALCTLE"), Times.Exactly(1));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_PostTokenFail_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "PSIO_cnvi_SKEWCALCTLE_calcode.txt", "./CNVI_formatfile.csv" });

            var ituffWriterMock = this.MockDataLog(
                "PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE_EDC_CAPTURE_L0_P0",
                "TOKEN=!Plist=FakePListName!RUN=1!CmpName=NA!TestType=CAPTURE!DataStart##DataEnd!KEY=",
                out var datalogServiceMock);
            /* this.MockDataLog(
                true,
                $"0_tname_PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE_EDC_CAPTURE_L0_P0\n0_strgval_TOKEN=!Plist=FakePListName!RUN=1!CmpName=NA!TestType=CAPTURE!DataStart##DataEnd!KEY=\n",
                out var tpSettingsServiceMock,
                out var datalogServiceMock); */

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", "PRE_RESET_PMOD", Context.LOT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", "POST_RESET_PMOD", Context.LOT));
            sharedStorageMock.Setup(o => o.GetStringRowFromTable("SIO_GSDS_BINDMEMDATA", Context.DUT)).Returns(string.Empty);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var tpService = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpService.Setup(o => o.SetTestInstanceParameter("PSIO_CNVI_ALL1::UTIL_X_PMOD_X_END_X_X_MAX_X_SKEWCALCTLE", "modify_token_dynamic_subset", "G.L.S.SIOGSDS_PATMOD_TOKEN"));
            tpService.Setup(o => o.GetTestInstanceParameter("PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE", "patlist")).Returns("FakePListName");
            tpService.SetupSequence(o => o.ExecuteTestInstance("PSIO_CNVI_ALL1::UTIL_X_PMOD_X_END_X_X_MAX_X_SKEWCALCTLE"))
                .Returns(1)
                .Returns(0);
            tpService.Setup(o => o.ExecuteTestInstance("PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE")).Returns(1);
            Prime.Services.TestProgramService = tpService.Object;

            var underTest = new SIOEDCMainTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "PSIO_cnvi_SKEWCALCTLE_calcode.txt",
                UserToken = "CNVI_LOOP_MAX_1P28_CAPTURE",
                PreTestTokens = "PRE_RESET_PMOD",
                PostTestTokens = "POST_RESET_PMOD",
            };

            underTest.Verify();
            Assert.AreEqual(0, underTest.Execute());
            this.ConsoleServiceMock.Verify(o => o.PrintError("[SIO::SomeTestInstanceName] Failed to run PostTest Tokens.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            fileServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            ituffWriterMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            tpService.VerifyAll();
            tpService.Verify(o => o.SetTestInstanceParameter("PSIO_CNVI_ALL1::UTIL_X_PMOD_X_END_X_X_MAX_X_SKEWCALCTLE", "modify_token_dynamic_subset", "G.L.S.SIOGSDS_PATMOD_TOKEN"), Times.Exactly(2));
            tpService.Verify(o => o.ExecuteTestInstance("PSIO_CNVI_ALL1::UTIL_X_PMOD_X_END_X_X_MAX_X_SKEWCALCTLE"), Times.Exactly(2));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_BadShmoo_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "PSIO_cnvi_SKEWCALCTLE_calcode.txt", "./CNVI_formatfile.csv" });

            var ituffWriterMock = this.MockDataLog(
                "PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE_EDC_CAPTURE_L0_P0",
                "TOKEN=!Plist=FakePListName!RUN=1!CmpName=NA!TestType=CAPTURE!DataStart##DataEnd!KEY=",
                out var datalogServiceMock);
            /* this.MockDataLog(
                true,
                $"0_tname_PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE_EDC_CAPTURE_L0_P0\n0_strgval_TOKEN=!Plist=FakePListName!RUN=1!CmpName=NA!TestType=CAPTURE!DataStart##DataEnd!KEY=\n",
                out var tpSettingsServiceMock,
                out var datalogServiceMock); */

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.GetStringRowFromTable("SIO_GSDS_BINDMEMDATA", Context.DUT)).Returns(string.Empty);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var tpService = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpService.Setup(o => o.GetTestInstanceParameter("PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE", "patlist")).Returns("FakePListName");
            tpService.Setup(o => o.ExecuteTestInstance("PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE")).Returns(-2);
            Prime.Services.TestProgramService = tpService.Object;

            var underTest = new SIOEDCMainTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "PSIO_cnvi_SKEWCALCTLE_calcode.txt",
                UserToken = "CNVI_LOOP_MAX_1P28_CAPTURE",
            };

            underTest.Verify();
            Assert.AreEqual(0, underTest.Execute());
            this.ConsoleServiceMock.Verify(o => o.PrintError("[SIO::SomeTestInstanceName] Detected 1 error(s) while running shmoo.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            fileServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            ituffWriterMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            tpService.VerifyAll();
        }

        private static string GetPathToFiles([CallerFilePath] string srcPath = "")
        {
            return Path.GetDirectoryName(srcPath) + "\\input_files\\";
        }

        private Mock<IFileService> MockFileService(List<string> filenames)
        {
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.FileExists(It.IsAny<string>())).Returns(true); // TODO: add fileexists checking to the sequence and format files.
            foreach (var filename in filenames)
            {
                fileServiceMock.Setup(o => o.GetFile(filename)).Returns((string s) => Path.IsPathRooted(s) ? s : GetPathToFiles() + Path.GetFileName(s));
            }

            Prime.Services.FileService = fileServiceMock.Object;
            return fileServiceMock;
        }

        private Mock<IStrgvalFormat> MockDataLog(string tname, string data, out Mock<IDatalogService> datalogServiceMock)
        {
            datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var strgvalWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strgvalWriterMock.Setup(o => o.SetCustomTname("PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE_EDC_CAPTURE_L0_P0"));
            strgvalWriterMock.Setup(o => o.SetData("TOKEN=!Plist=FakePListName!RUN=1!CmpName=NA!TestType=CAPTURE!DataStart##DataEnd!KEY="));
            strgvalWriterMock.Setup(o => o.SetDelimiterCharacterForWrap('!'));

            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strgvalWriterMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(strgvalWriterMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;
            return strgvalWriterMock;
        }
    }
}
