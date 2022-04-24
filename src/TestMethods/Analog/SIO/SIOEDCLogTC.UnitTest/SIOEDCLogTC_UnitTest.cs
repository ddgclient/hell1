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

namespace SIOEDCLogTC.UnitTest
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
    using Prime.FunctionalService;
    using Prime.SharedStorageService;
    using Prime.TestMethods;
    using Prime.TpSettingsService;
    using Prime.UserVarService;

    /// <summary>
    /// Defines the <see cref="SIOEDCLogTC_UnitTest" />.
    /// </summary>
    [TestClass]
    public class SIOEDCLogTC_UnitTest
    {
        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private string SimpleCaptureData { get; set; } = "111111011100000000000000000000001000000000011111011111011111111010010000000100000000000000000000100100000001000000000000000000001001000000010000000000000000000010010000000100000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000011000000000000000000000000000000110000000000000000000000000000001100000000000000000000000000000011000000000000000000000000000000100100000001000000000000000000001001000000010000000000000000000010010000000100000000000000000000100100000001000000000000000000000000000000000000000000000111000100000000000000000000000000000000000000000000000000000000011100010000000000000000000000000000000000000000000000000000000001110001000000000000000000000000000000000000000000000000000000000111000100000000000000000000000000000000";

        private string SimpleDataLog { get; set; } = "TOKEN=RUN:0!Plist=FakePList!RUN=1!CmpName=NA!TestType=CAPTURE!DataStart#}9>?^+10*9'~E9<9]9;9]9;9A+9(9:9A`9&9[9/0*9'~@99.Q99A:R99~-99~%9)#DataEnd!KEY=9AAAA|8AAEA|1AAAJ|0AEAA|*ACIB|'SAI|~AAA|}A7X|?D56|^75E|]AAI|[ABQ|@EQC|=AHC|>EA|+AQ|<AB|;AC|:AD|`AY|/AJ|.BY|-HC|)AA|(AE|&AG|%OE";

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
        /// Test the templates verify with reusemem mode.
        /// </summary>
        [TestMethod]
        public void Verify_BadReuseMemFormat_Fail()
        {
            // setup the mocks
            var captureCtvPerPinTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(f => f.CreateCaptureCtvPerPinTest("FakePList", "FakeLevels", "FakeTiming", new List<string> { "TDO" }, It.IsAny<string>())).Returns(captureCtvPerPinTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // creat the test.
            SIOEDCLogTC underTest = new SIOEDCLogTC
            {
                InstanceName = "SIO_PCIE_LPBK1::REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB",
                BypassPort = "-1",
                LogLevel = TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                TimingsTc = "FakeTiming",
                LevelsTc = "FakeLevels",
                Patlist = "FakePList",
                CtvCapturePins = "TDO",
                UserFile = "SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt",
                UserToken = "PCIE_CALCODE_DNELB_GEN1",
                ReuseCaptMemGlobal = "MyCollection.BAD.SIO_SAVED_CAPTMEM",
            };
            underTest.TestMethodExtension = underTest;

            underTest.Verify();
            var ex = Assert.ThrowsException<ArgumentException>(() => underTest.CustomVerify());
            Assert.IsTrue(ex.Message.Contains("Error: ReuseCaptMemGlobal=[MyCollection.BAD.SIO_SAVED_CAPTMEM] is in an unknown format, should be UserVar(Collection.uservar) or GSDS (G.U.S.Token)."));
            funcServiceMock.VerifyAll();
            captureCtvPerPinTestMock.VerifyAll();
        }

        /// <summary>
        /// Test the templates verify with reusemem mode.
        /// </summary>
        [TestMethod]
        public void Verify_MissingFile_Fail()
        {
            // setup the mocks
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.FileExists("SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt")).Returns(false);
            Prime.Services.FileService = fileServiceMock.Object;

            var captureCtvPerPinTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(f => f.CreateCaptureCtvPerPinTest("FakePList", "FakeLevels", "FakeTiming", new List<string> { "TDO" }, It.IsAny<string>())).Returns(captureCtvPerPinTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // creat the test.
            SIOEDCLogTC underTest = new SIOEDCLogTC
            {
                InstanceName = "SIO_PCIE_LPBK1::REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB",
                BypassPort = "-1",
                LogLevel = TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                TimingsTc = "FakeTiming",
                LevelsTc = "FakeLevels",
                Patlist = "FakePList",
                CtvCapturePins = "TDO",
                UserFile = "SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt",
                UserToken = "PCIE_CALCODE_DNELB_GEN1",
                ReuseCaptMemGlobal = "G.U.S.SIO_SAVED_CAPTMEM",
            };
            underTest.TestMethodExtension = underTest;

            // Run Verify.
            underTest.Verify();
            Assert.ThrowsException<FileNotFoundException>(() => underTest.CustomVerify());

            // check all the mocks.
            funcServiceMock.VerifyAll();
            captureCtvPerPinTestMock.VerifyAll();
            fileServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the templates verify with reusemem mode.
        /// </summary>
        [TestMethod]
        public void Execute_ReuseMemGsds_Pass()
        {
            // setup the mocks
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt", "./format_SIO_PCIE_LPBK1_merged.csv", "./SIO_PCIE_LPBK1_sequence.csv" });
            var ituffWriterMock = this.MockDataLog(
                "SIO_PCIE_LPBK1::REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB_EDC_CAPTURE_L0_P0",
                this.SimpleDataLog,
                out var datalogServiceMock);
            /* this.MockDataLog(
                true,
                $"0_tname_SIO_PCIE_LPBK1::REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB_EDC_CAPTURE_L0_P0\n0_strgval_{this.SimpleDataLog}\n",
                out var tpSettingsServiceMock,
                out var datalogServiceMock); */

            var captureCtvPerPinTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(f => f.CreateCaptureCtvPerPinTest("FakePList", "FakeLevels", "FakeTiming", new List<string> { "TDO" }, It.IsAny<string>())).Returns(captureCtvPerPinTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.GetStringRowFromTable("SIO_SAVED_CAPTMEM", Context.DUT))
                .Returns(this.SimpleCaptureData);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // creat the test.
            SIOEDCLogTC underTest = new SIOEDCLogTC
            {
                InstanceName = "SIO_PCIE_LPBK1::REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB",
                BypassPort = "-1",
                LogLevel = TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                TimingsTc = "FakeTiming",
                LevelsTc = "FakeLevels",
                Patlist = "FakePList",
                CtvCapturePins = "TDO",
                UserFile = "SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt",
                UserToken = "PCIE_CALCODE_DNELB_GEN1",
                ReuseCaptMemGlobal = "G.U.S.SIO_SAVED_CAPTMEM",
            };
            underTest.TestMethodExtension = underTest;

            // Run Verify.
            underTest.Verify();
            underTest.CustomVerify(); // FIXME - why isn't this called automatically in the unit test like in real life??

            // run execute
            var exitPort = underTest.Execute();
            Assert.AreEqual(1, exitPort);

            // check all the mocks.
            funcServiceMock.VerifyAll();
            captureCtvPerPinTestMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            fileServiceMock.VerifyAll();
            ituffWriterMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the templates verify with reusemem mode.
        /// </summary>
        [TestMethod]
        public void Execute_ReuseMemUserVar_Pass()
        {
            // setup the mocks
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt", "./format_SIO_PCIE_LPBK1_merged.csv", "./SIO_PCIE_LPBK1_sequence.csv" });
            var ituffWriterMock = this.MockDataLog(
                "SIO_PCIE_LPBK1::REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB_EDC_CAPTURE_L0_P0",
                this.SimpleDataLog,
                out var datalogServiceMock);
            /* this.MockDataLog(
                true,
                $"0_tname_SIO_PCIE_LPBK1::REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB_EDC_CAPTURE_L0_P0\n0_strgval_{this.SimpleDataLog}\n",
                out var tpSettingsServiceMock,
                out var datalogServiceMock); */

            var captureCtvPerPinTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(f => f.CreateCaptureCtvPerPinTest("FakePList", "FakeLevels", "FakeTiming", new List<string> { "TDO" }, It.IsAny<string>())).Returns(captureCtvPerPinTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.GetStringValue("MyCollection.SIO_SAVED_CAPTMEM")).Returns(this.SimpleCaptureData);

            Prime.Services.UserVarService = userVarMock.Object;

            // creat the test.
            SIOEDCLogTC underTest = new SIOEDCLogTC
            {
                InstanceName = "SIO_PCIE_LPBK1::REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB",
                BypassPort = "-1",
                LogLevel = TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                TimingsTc = "FakeTiming",
                LevelsTc = "FakeLevels",
                Patlist = "FakePList",
                CtvCapturePins = "TDO",
                UserFile = "SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt",
                UserToken = "PCIE_CALCODE_DNELB_GEN1",
                ReuseCaptMemGlobal = "MyCollection.SIO_SAVED_CAPTMEM",
            };
            underTest.TestMethodExtension = underTest;

            // Run Verify.
            underTest.Verify();
            underTest.CustomVerify(); // FIXME - why isn't this called automatically in the unit test like in real life??

            // run execute
            var exitPort = underTest.Execute();
            Assert.AreEqual(1, exitPort);

            // check all the mocks.
            funcServiceMock.VerifyAll();
            captureCtvPerPinTestMock.VerifyAll();
            userVarMock.VerifyAll();
            fileServiceMock.VerifyAll();
            ituffWriterMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the templates verify with reusemem mode.
        /// </summary>
        [TestMethod]
        public void Execute_FullExec_Pass()
        {
            // setup the mocks
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt", "./format_SIO_PCIE_LPBK1_merged.csv", "./SIO_PCIE_LPBK1_sequence.csv" });
            var ituffWriterMock = this.MockDataLog(
                "SIO_PCIE_LPBK1::REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB_EDC_CAPTURE_L0_P0",
                this.SimpleDataLog,
                out var datalogServiceMock);
            /* this.MockDataLog(
                false,
                $"2_lsep\n2_tname_SIO_PCIE_LPBK1::REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB_EDC_CAPTURE_L0_P0\n2_strgval_{this.SimpleDataLog}\n",
                out var tpSettingsServiceMock,
                out var datalogServiceMock); */

            var captureCtvPerPinTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            captureCtvPerPinTestMock.Setup(t => t.ApplyTestConditions());
            captureCtvPerPinTestMock.Setup(t => t.Execute()).Returns(true);
            captureCtvPerPinTestMock.Setup(t => t.SetPinMask(new List<string>()));
            captureCtvPerPinTestMock.Setup(t => t.GetCtvData()).Returns(new Dictionary<string, string> { { "TDO", this.SimpleCaptureData } });

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(f => f.CreateCaptureCtvPerPinTest("FakePList", "FakeLevels", "FakeTiming", new List<string> { "TDO" }, It.IsAny<string>())).Returns(captureCtvPerPinTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // creat the test.
            SIOEDCLogTC underTest = new SIOEDCLogTC
            {
                InstanceName = "SIO_PCIE_LPBK1::REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB",
                BypassPort = "-1",
                LogLevel = TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                TimingsTc = "FakeTiming",
                LevelsTc = "FakeLevels",
                Patlist = "FakePList",
                CtvCapturePins = "TDO",
                UserFile = "SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt",
                UserToken = "PCIE_CALCODE_DNELB_GEN1",
            };
            underTest.TestMethodExtension = underTest;

            // Run Verify.
            underTest.Verify();
            underTest.CustomVerify(); // FIXME - why isn't this called automatically in the unit test like in real life??

            // run execute
            var exitPort = underTest.Execute();
            Assert.AreEqual(1, exitPort);

            // check all the mocks.
            funcServiceMock.VerifyAll();
            captureCtvPerPinTestMock.VerifyAll();
            fileServiceMock.VerifyAll();
            ituffWriterMock.VerifyAll();
            datalogServiceMock.VerifyAll();
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
            strgvalWriterMock.Setup(o => o.SetCustomTname(tname));
            strgvalWriterMock.Setup(o => o.SetData(data));
            strgvalWriterMock.Setup(o => o.SetDelimiterCharacterForWrap('!'));

            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strgvalWriterMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(strgvalWriterMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;
            return strgvalWriterMock;
        }
    }
}
