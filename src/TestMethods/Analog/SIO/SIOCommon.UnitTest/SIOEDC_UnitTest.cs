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
    using System.IO;
    using System.Runtime.CompilerServices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;
    using SIO;

    /// <summary>
    /// Defines the <see cref="SIOEDC_UnitTest" />.
    /// </summary>
    [TestClass]
    public class SIOEDC_UnitTest
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

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void SetupEDCLog_Pass()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt", "./format_SIO_PCIE_LPBK1_merged.csv", "./SIO_PCIE_LPBK1_sequence.csv" });

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            edc.SetupEDCLog("SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt", "PCIE_CALCODE_DNELB_GEN1");
            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void SetupEDCLog_MissingFiles_Pass()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "TestUserFile.txt" });
            fileServiceMock.Setup(o => o.GetFile("./format_Missing.csv")).Throws(new Prime.Base.Exceptions.FatalException("Missing File"));
            fileServiceMock.Setup(o => o.GetFile("./missing_sequence.csv")).Throws(new Prime.Base.Exceptions.FatalException("Missing File"));

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", debug: true);
            edc.SetupEDCLog("TestUserFile.txt", "MISSING_FILES", allowMissingFiles: true);
            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void SetupEDCLog_InvalidUserFile_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "FailUserFile_CompareBlockNoName.txt" });

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            var ex = Assert.ThrowsException<Exception>(() => edc.SetupEDCLog("FailUserFile_CompareBlockNoName.txt", "token"));
            Assert.AreEqual("SIOEDC.SetupEDCLog failed to construct UserFileData.", ex.Message);
            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void SetupEDCLog_InvalidToken_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt" });

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            var ex = Assert.ThrowsException<Exception>(() => edc.SetupEDCLog("SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt", "BadToken"));
            Assert.AreEqual("SIOEDC.Setup failed, token=[BadToken] does not exist in UserFileData.", ex.Message);
            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void SetupEDCLog_MissingFormatFile_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "TestUserFile.txt" });
            fileServiceMock.Setup(o => o.GetFile("./format_Missing.csv")).Throws(new Prime.Base.Exceptions.FatalException("Missing File"));

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            var ex = Assert.ThrowsException<Exception>(() => edc.SetupEDCLog("TestUserFile.txt", "MISSING_FORMATFILE"));
            Assert.AreEqual("SIOEDC.Setup failed to construct FormatFileData.", ex.Message);
            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void SetupEDCLog_MissingSequenceFile_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "TestUserFile.txt", "./format_SIO_PCIE_LPBK1_merged.csv" });
            fileServiceMock.Setup(o => o.GetFile("./missing_sequence.csv")).Throws(new Prime.Base.Exceptions.FatalException("Missing File"));

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            var ex = Assert.ThrowsException<Exception>(() => edc.SetupEDCLog("TestUserFile.txt", "MISSING_SEQUENCEFILE"));
            Assert.AreEqual("SIOEDC.Setup failed to construct SequenceFileData.", ex.Message);
            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void SetupEDCLog_MissingSequenceId_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "TestUserFile.txt", "./format_SIO_PCIE_LPBK1_merged.csv", "./SIO_PCIE_LPBK1_sequence.csv" });

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            var ex = Assert.ThrowsException<Exception>(() => edc.SetupEDCLog("TestUserFile.txt", "MISSING_SEQUENCEID"));
            Assert.AreEqual("SIOEDC.Setup failed, seqid=[missing_id] does not exist in SequenceFileData.", ex.Message);
            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunEDCLog_SetupFailed_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "TestUserFile.txt", "./format_SIO_PCIE_LPBK1_merged.csv", "./SIO_PCIE_LPBK1_sequence.csv" });

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            var ex = Assert.ThrowsException<Exception>(() => edc.SetupEDCLog("TestUserFile.txt", "MISSING_SEQUENCEID"));
            Assert.AreEqual("SIOEDC.Setup failed, seqid=[missing_id] does not exist in SequenceFileData.", ex.Message);
            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunEDCLog_BadPin_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt", "./format_SIO_PCIE_LPBK1_merged.csv", "./SIO_PCIE_LPBK1_sequence.csv" });

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            edc.SetupEDCLog("SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt", "PCIE_CALCODE_DNELB_GEN1");
            var ex = Assert.ThrowsException<ArgumentException>(() => edc.RunEDCLog(new Dictionary<string, string>(), string.Empty, "TDO"));
            Assert.AreEqual("CTV data does not contain data for pin=[TDO]", ex.Message);

            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunEDCLog_BadData_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt", "./format_SIO_PCIE_LPBK1_merged.csv", "./SIO_PCIE_LPBK1_sequence.csv" });

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            edc.SetupEDCLog("SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt", "PCIE_CALCODE_DNELB_GEN1");
            var ex = Assert.ThrowsException<ArgumentException>(() => edc.RunEDCLog(new Dictionary<string, string>() { { "TDO", string.Empty } }, string.Empty, "TDO"));
            Assert.AreEqual("Failed to compress ctv data", ex.Message);

            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunEDCLog_Ituff_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "TestUserFile.txt", "./format_SIO_PCIE_LPBK1_merged.csv", "./SIO_PCIE_LPBK1_sequence.csv" });

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            /* datalogServiceMock.Setup(o => o.WriteToItuff(It.IsAny<string>())).Throws(new Prime.Base.Exceptions.FatalException("Cannot datalog")); */
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Throws(new Prime.Base.Exceptions.FatalException("Cannot datalog"));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            edc.SetupEDCLog("TestUserFile.txt", "TEST_DLOGNAME");
            Assert.AreEqual(0, edc.RunEDCLog(new Dictionary<string, string>() { { "TDO", "0000" } }, string.Empty, "TDO"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("Datalogging failed", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            fileServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunEDCLog_Hash_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "TestUserFile.txt", "./format_SIO_PCIE_LPBK1_merged.csv", "./SIO_PCIE_LPBK1_sequence.csv" });

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var looseItuffWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Loose);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(looseItuffWriterMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(looseItuffWriterMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            edc.SetupEDCLog("TestUserFile.txt", "TEST_DLOGNAME");
            Assert.AreEqual(0, edc.RunEDCLog(new Dictionary<string, string>() { { "TDO", "0000" } }, string.Empty, "TDO"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("Failed to assign data from raw ctv to struct.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            fileServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunEDCLog_GenerateOutput_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "TestUserFile.txt", "./fourbit_sequence.csv" });
            fileServiceMock.Setup(o => o.GetFile("./format_Missing.csv")).Throws(new Prime.Base.Exceptions.FatalException("Missing File"));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var looseItuffWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Loose);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(looseItuffWriterMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(looseItuffWriterMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            edc.SetupEDCLog("TestUserFile.txt", "TEST_OUTPUTFAIL", allowMissingFiles: true);
            Assert.AreEqual(0, edc.RunEDCLog(new Dictionary<string, string>() { { "TDO", "0000" } }, string.Empty, "TDO"));
            this.ConsoleServiceMock.Verify(o => o.PrintError("Failed to generate output correctly.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            fileServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunEDCLog_NoOutput_Pass()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "TestUserFile.txt", "./fourbit_sequence.csv" });
            fileServiceMock.Setup(o => o.GetFile("./format_Missing.csv")).Throws(new Prime.Base.Exceptions.FatalException("Missing File"));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var looseItuffWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Loose);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(looseItuffWriterMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(looseItuffWriterMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            edc.SetupEDCLog("TestUserFile.txt", "TEST_NOOUTPUT", allowMissingFiles: true);
            Assert.AreEqual(1, edc.RunEDCLog(new Dictionary<string, string>() { { "TDO", "0000" } }, string.Empty, "TDO"));

            fileServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void SetupEDCMain_InvalidShmoo_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "TestUserFile.txt" });

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            Assert.IsFalse(edc.SetupEDCMain("TestUserFile.txt", "SHMOO_SETUP_FAIL", true, false));

            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunShmooSinglePointEDC_NoOutput_Pass()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "TestUserFile.txt", "./fourbit_sequence.csv" });
            fileServiceMock.Setup(o => o.GetFile("./format_Missing.csv")).Throws(new Prime.Base.Exceptions.FatalException("Missing File"));

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.ExecuteTestInstance("sometesttoexecute")).Returns(1);
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var sharedStorageService = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageService.Setup(o => o.GetStringRowFromTable(SIOLib.SIOGSDSBINDMEMDATA, Context.DUT)).Returns("0000");
            Prime.Services.SharedStorageService = sharedStorageService.Object;

            var userData = new UserFile.UserData("token");
            userData.ExecuteTest = "sometesttoexecute";
            userData.SeqId = "class_edc_g3";
            userData.RegDef = string.Empty;
            userData.EDCLogEnabled = false;

            var state = new Dictionary<string, string> { { "RUN", "0" } };
            foreach (var axis in new List<string> { "R", "S", "T", "U", "V", "W", "Z", "Y", "X", "YShmoo", "XShmoo" })
            {
                state[axis] = string.Empty;
            }

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            edc.SetupEDCLog("TestUserFile.txt", "TEST_OUTPUTFAIL", allowMissingFiles: true);
            edc.RunShmooSinglePointEDC(userData, state);

            testProgramServiceMock.VerifyAll();
            sharedStorageService.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void RunShmooSinglePointEDC_SimpleOutput_Pass()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "TestUserFile.txt", "./fourbit_sequence.csv", "./fourbit_format.csv" });

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.ExecuteTestInstance("sometesttoexecute")).Returns(1);
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var sharedStorageService = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageService.Setup(o => o.GetStringRowFromTable(SIOLib.SIOGSDSBINDMEMDATA, Context.DUT)).Returns("0000");
            Prime.Services.SharedStorageService = sharedStorageService.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Throws(new Prime.Base.Exceptions.FatalException("Failed to datalog"));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var userData = new UserFile.UserData("token");
            userData.TestResults = new List<UserFile.TestResult> { new UserFile.TestResult() };
            userData.TestResults[0].PassCount = new List<int> { 0 };
            userData.ExecuteTest = "sometesttoexecute";
            userData.SeqId = "class_edc_g3";
            userData.RegDef = string.Empty;
            userData.EDCLogEnabled = true;

            var state = new Dictionary<string, string> { { "RUN", "0" } };
            foreach (var axis in new List<string> { "R", "S", "T", "U", "V", "W", "Z", "Y", "X", "YShmoo", "XShmoo" })
            {
                state[axis] = string.Empty;
            }

            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", true);
            Assert.IsTrue(edc.SetupEDCMain("TestUserFile.txt", "TEST_SIMPLEOUTPUT", true, true));
            /* Assert.IsTrue(edc.SetupEDCLog("TestUserFile.txt", "TEST_SIMPLEOUTPUT", allowMissingFiles: true)); */
            edc.RunShmooSinglePointEDC(userData, state);

            testProgramServiceMock.VerifyAll();
            sharedStorageService.VerifyAll();
            datalogServiceMock.Verify();
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
    }
}
