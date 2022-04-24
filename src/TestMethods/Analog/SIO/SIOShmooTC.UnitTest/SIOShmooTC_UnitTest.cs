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

namespace SIOShmooTC.UnitTest
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
    /// Defines the <see cref="SIOShmooTC_UnitTest" />.
    /// </summary>
    [TestClass]
    public class SIOShmooTC_UnitTest
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
            var underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserToken = "DP_LPBK_5400_SHMOO",
            };

            var ex = Assert.ThrowsException<ArgumentException>(() => underTest.Verify());
            Assert.AreEqual("[SIO::SomeTestInstanceName] UserFile is a required argument.", ex.Message);
        }

        /// <summary>
        /// Test the verify function.
        /// </summary>
        [TestMethod]
        public void Verify_MissingTokenParam_Fail()
        {
            var underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt",
            };

            var ex = Assert.ThrowsException<ArgumentException>(() => underTest.Verify());
            Assert.AreEqual("[SIO::SomeTestInstanceName] UserToken is a required argument.", ex.Message);
        }

        /// <summary>
        /// Test the verify function.
        /// </summary>
        [TestMethod]
        public void Verify_MissingFile_Fail()
        {
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.FileExists("SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt")).Returns(false);
            Prime.Services.FileService = fileServiceMock.Object;

            var underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt",
                UserToken = "DP_LPBK_5400_SHMOO",
            };

            var ex = Assert.ThrowsException<FileNotFoundException>(() => underTest.Verify());
            Assert.AreEqual("File=[SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt] is not found.", ex.Message);
            fileServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the verify function.
        /// </summary>
        [TestMethod]
        public void Verify_InvalidFile_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "FailUserFile_ShmooKeyBadValue.txt" });

            var underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "FailUserFile_ShmooKeyBadValue.txt",
                UserToken = "DP_LPBK_5400_SHMOO",
            };

            var ex = Assert.ThrowsException<FileLoadException>(() => underTest.Verify());
            Assert.AreEqual("[SIO::SomeTestInstanceName] Failed to read UserDataFile=[FailUserFile_ShmooKeyBadValue.txt] correctly.", ex.Message);
            fileServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the verify function.
        /// </summary>
        [TestMethod]
        public void Verify_TokenNotInFile_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt" });

            var underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt",
                UserToken = "TOKEN_DOES_NOT_EXIST",
            };

            var ex = Assert.ThrowsException<FileLoadException>(() => underTest.Verify());
            Assert.AreEqual("[SIO::SomeTestInstanceName] Token=[TOKEN_DOES_NOT_EXIST] Not found in UserDataFile=[SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt].", ex.Message);
            fileServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the verify function.
        /// </summary>
        [TestMethod]
        public void Verify_BadTestType_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt" });

            var underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt",
                UserToken = "DP_LPBK_5400_SHMOO_BAD",
            };

            var ex = Assert.ThrowsException<FileLoadException>(() => underTest.Verify());
            Assert.AreEqual("[SIO::SomeTestInstanceName] Token=[DP_LPBK_5400_SHMOO_BAD] Failed to setup UserData for shmoo.", ex.Message);
            fileServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the verify function.
        /// </summary>
        [TestMethod]
        public void Verify_MismatchShmooTestType_Fail()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt" });
            var tpService = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpService.Setup(o => o.GetTestInstanceParameter("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO", "xaxis_parameter")).Returns("bclkper_spec");
            Prime.Services.TestProgramService = tpService.Object;

            var underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt",
                UserToken = "DP_LPBK_5400_SHMOO_WRONGTYPE",
            };

            var ex = Assert.ThrowsException<ArgumentException>(() => underTest.Verify());
            Assert.AreEqual("[SIO::SomeTestInstanceName] Token=[DP_LPBK_5400_SHMOO_WRONGTYPE] Invalid Test Type=[CAPTURE] for iCShmoo test, must be GONOGO.", ex.Message);
            fileServiceMock.VerifyAll();
            tpService.VerifyAll();
        }

        /// <summary>
        /// Test the verify function.
        /// </summary>
        [TestMethod]
        public void Execute_PrePostInstances_Pass()
        {
            var yPoints = new List<string> { "SET_SWING:1", "SET_SWING:2", "SET_SWING:3", "SET_SWING:8", "SET_SWING:11" };
            var xPoints = new List<string> { "SET_RXODELAYSEL:0", "SET_RXODELAYSEL:1", "SET_RXODELAYSEL:2", "SET_RXODELAYSEL:3", "SET_RXODELAYSEL:7", "SET_RXODELAYSEL:11", "SET_RXODELAYSEL:15" };
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt" });

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", "PRE_RESET_PMOD", Context.LOT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", "POST_RESET_PMOD", Context.LOT));

            foreach (var point in yPoints)
            {
                sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", point.Replace(":", "_"), Context.LOT));
            }

            foreach (var point in xPoints)
            {
                sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", point.Replace(":", "_"), Context.LOT));
            }

            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var tpService = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpService.Setup(o => o.SetTestInstanceParameter("SIO_DP_LPBK1::REUAFELB_X_PMOD_E_END_X_X_X_X_GLOBAL", "modify_token_dynamic_subset", "G.L.S.SIOGSDS_PATMOD_TOKEN"));
            tpService.Setup(o => o.GetTestInstanceParameter("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO", "patlist")).Returns("FakePListName");
            tpService.Setup(o => o.ExecuteTestInstance("SIO_DP_LPBK1::REUAFELB_X_PMOD_E_END_X_X_X_X_GLOBAL")).Returns(1);
            tpService.Setup(o => o.ExecuteTestInstance("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO")).Returns(1);
            tpService.Setup(o => o.GetTestInstanceParameter("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO", "xaxis_parameter")).Returns("bclkper_spec");
            Prime.Services.TestProgramService = tpService.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var writerMock = new Mock<IComntFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.IncludeTnameInPrint(false));
            writerMock.Setup(o => o.SetData("tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO_svtt_SHMOO_LVLTIM_START"));
            /* datalogServiceMock.Setup(o => o.WriteToItuff("0_comnt_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO_svtt_SHMOO_LVLTIM_START\n")); */
            foreach (var y in yPoints)
            {
                foreach (var x in xPoints)
                {
                    writerMock.Setup(o => o.SetData($"TOKEN={y};{x};RUN:0"));
                    /* datalogServiceMock.Setup(o => o.WriteToItuff($"0_comnt_TOKEN={y};{x};RUN:0\n")); */
                }
            }

            writerMock.Setup(o => o.SetData("tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO_svtt_SHMOO_LVLTIM_END"));
            /* datalogServiceMock.Setup(o => o.WriteToItuff("0_comnt_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO_svtt_SHMOO_LVLTIM_END\n")); */
            datalogServiceMock.Setup(o => o.GetItuffComntWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt",
                UserToken = "DP_LPBK_5400_SHMOO",
                PreTestTokens = "PRE_RESET_PMOD",
                PostTestTokens = "POST_RESET_PMOD",
            };

            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            fileServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            tpService.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// Test the verify function.
        /// </summary>
        [TestMethod]
        public void Execute_PreTokenFail_Fail()
        {
            var yPoints = new List<string> { "SET_SWING:1", "SET_SWING:2", "SET_SWING:3", "SET_SWING:8", "SET_SWING:11" };
            var xPoints = new List<string> { "SET_RXODELAYSEL:0", "SET_RXODELAYSEL:1", "SET_RXODELAYSEL:2", "SET_RXODELAYSEL:3", "SET_RXODELAYSEL:7", "SET_RXODELAYSEL:11", "SET_RXODELAYSEL:15" };
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt" });

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", "PRE_RESET_PMOD", Context.LOT));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var tpService = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpService.Setup(o => o.SetTestInstanceParameter("SIO_DP_LPBK1::REUAFELB_X_PMOD_E_END_X_X_X_X_GLOBAL", "modify_token_dynamic_subset", "G.L.S.SIOGSDS_PATMOD_TOKEN"));
            tpService.Setup(o => o.ExecuteTestInstance("SIO_DP_LPBK1::REUAFELB_X_PMOD_E_END_X_X_X_X_GLOBAL")).Returns(0);
            tpService.Setup(o => o.GetTestInstanceParameter("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO", "xaxis_parameter")).Returns("bclkper_spec");
            Prime.Services.TestProgramService = tpService.Object;

            var underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt",
                UserToken = "DP_LPBK_5400_SHMOO",
                PreTestTokens = "PRE_RESET_PMOD",
                PostTestTokens = "POST_RESET_PMOD",
            };

            underTest.Verify();
            Assert.AreEqual(0, underTest.Execute());
            this.ConsoleServiceMock.Verify(o => o.PrintError("[SIO::SomeTestInstanceName] Failed to run PreTest Tokens.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            fileServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            tpService.VerifyAll();
        }

        /// <summary>
        /// Test the verify function.
        /// </summary>
        [TestMethod]
        public void Execute_PostTokenFail()
        {
            var yPoints = new List<string> { "SET_SWING:1", "SET_SWING:2", "SET_SWING:3", "SET_SWING:8", "SET_SWING:11" };
            var xPoints = new List<string> { "SET_RXODELAYSEL:0", "SET_RXODELAYSEL:1", "SET_RXODELAYSEL:2", "SET_RXODELAYSEL:3", "SET_RXODELAYSEL:7", "SET_RXODELAYSEL:11", "SET_RXODELAYSEL:15" };
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt" });

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", "PRE_RESET_PMOD", Context.LOT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", "POST_RESET_PMOD", Context.LOT));

            foreach (var point in yPoints)
            {
                sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", point.Replace(":", "_"), Context.LOT));
            }

            foreach (var point in xPoints)
            {
                sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", point.Replace(":", "_"), Context.LOT));
            }

            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var tpService = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpService.Setup(o => o.SetTestInstanceParameter("SIO_DP_LPBK1::REUAFELB_X_PMOD_E_END_X_X_X_X_GLOBAL", "modify_token_dynamic_subset", "G.L.S.SIOGSDS_PATMOD_TOKEN"));
            tpService.Setup(o => o.GetTestInstanceParameter("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO", "patlist")).Returns("FakePListName");
            tpService.SetupSequence(o => o.ExecuteTestInstance("SIO_DP_LPBK1::REUAFELB_X_PMOD_E_END_X_X_X_X_GLOBAL"))
                .Returns(1)
                .Returns(0);
            tpService.Setup(o => o.ExecuteTestInstance("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO")).Returns(1);
            tpService.Setup(o => o.GetTestInstanceParameter("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO", "xaxis_parameter")).Returns("bclkper_spec");
            Prime.Services.TestProgramService = tpService.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var writerMock = new Mock<IComntFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.IncludeTnameInPrint(false));
            writerMock.Setup(o => o.SetData("tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO_svtt_SHMOO_LVLTIM_START"));
            /* datalogServiceMock.Setup(o => o.WriteToItuff("0_comnt_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO_svtt_SHMOO_LVLTIM_START\n")); */
            foreach (var y in yPoints)
            {
                foreach (var x in xPoints)
                {
                    writerMock.Setup(o => o.SetData($"TOKEN={y};{x};RUN:0"));
                    /* datalogServiceMock.Setup(o => o.WriteToItuff($"0_comnt_TOKEN={y};{x};RUN:0\n")); */
                }
            }

            writerMock.Setup(o => o.SetData("tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO_svtt_SHMOO_LVLTIM_END"));
            /* datalogServiceMock.Setup(o => o.WriteToItuff("0_comnt_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO_svtt_SHMOO_LVLTIM_END\n")); */
            datalogServiceMock.Setup(o => o.GetItuffComntWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt",
                UserToken = "DP_LPBK_5400_SHMOO",
                PreTestTokens = "PRE_RESET_PMOD",
                PostTestTokens = "POST_RESET_PMOD",
            };

            underTest.Verify();
            Assert.AreEqual(0, underTest.Execute());
            this.ConsoleServiceMock.Verify(o => o.PrintError("[SIO::SomeTestInstanceName] Failed to run PostTest Tokens.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            fileServiceMock.VerifyAll();
            writerMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            tpService.VerifyAll();
        }

        /// <summary>
        /// Test the verify function.
        /// </summary>
        [TestMethod]
        public void Execute_ShmooError_Fail()
        {
            var yPoints = new List<string> { "SET_SWING:1", "SET_SWING:2", "SET_SWING:3", "SET_SWING:8", "SET_SWING:11" };
            var xPoints = new List<string> { "SET_RXODELAYSEL:0", "SET_RXODELAYSEL:1", "SET_RXODELAYSEL:2", "SET_RXODELAYSEL:3", "SET_RXODELAYSEL:7", "SET_RXODELAYSEL:11", "SET_RXODELAYSEL:15" };
            var fileServiceMock = this.MockFileService(new List<string> { "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt" });

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", "PRE_RESET_PMOD", Context.LOT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", "POST_RESET_PMOD", Context.LOT));

            foreach (var point in yPoints)
            {
                sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", point.Replace(":", "_"), Context.LOT));
            }

            foreach (var point in xPoints)
            {
                sharedStorageMock.Setup(o => o.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", point.Replace(":", "_"), Context.LOT));
            }

            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var tpService = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpService.Setup(o => o.SetTestInstanceParameter("SIO_DP_LPBK1::REUAFELB_X_PMOD_E_END_X_X_X_X_GLOBAL", "modify_token_dynamic_subset", "G.L.S.SIOGSDS_PATMOD_TOKEN"));
            tpService.Setup(o => o.GetTestInstanceParameter("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO", "patlist")).Returns("FakePListName");
            tpService.Setup(o => o.ExecuteTestInstance("SIO_DP_LPBK1::REUAFELB_X_PMOD_E_END_X_X_X_X_GLOBAL")).Returns(1);
            tpService.Setup(o => o.ExecuteTestInstance("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO")).Returns(-2);
            tpService.Setup(o => o.GetTestInstanceParameter("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO", "xaxis_parameter")).Returns("bclkper_spec");
            Prime.Services.TestProgramService = tpService.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var writerMock = new Mock<IComntFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.IncludeTnameInPrint(false));
            writerMock.Setup(o => o.SetData("tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO_svtt_SHMOO_LVLTIM_START"));
            /* datalogServiceMock.Setup(o => o.WriteToItuff("0_comnt_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO_svtt_SHMOO_LVLTIM_START\n")); */
            foreach (var y in yPoints)
            {
                foreach (var x in xPoints)
                {
                    writerMock.Setup(o => o.SetData($"TOKEN={y};{x};RUN:0"));
                    /* datalogServiceMock.Setup(o => o.WriteToItuff($"0_comnt_TOKEN={y};{x};RUN:0\n")); */
                }
            }

            writerMock.Setup(o => o.SetData("tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO_svtt_SHMOO_LVLTIM_END"));
            /* datalogServiceMock.Setup(o => o.WriteToItuff("0_comnt_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_5400_SHMOO_svtt_SHMOO_LVLTIM_END\n")); */
            datalogServiceMock.Setup(o => o.GetItuffComntWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt",
                UserToken = "DP_LPBK_5400_SHMOO",
                PreTestTokens = "PRE_RESET_PMOD",
                PostTestTokens = "POST_RESET_PMOD",
            };

            underTest.Verify();
            Assert.AreEqual(0, underTest.Execute());
            this.ConsoleServiceMock.Verify(o => o.PrintError("[SIO::SomeTestInstanceName] Detected 35 errors while running shmoo.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            fileServiceMock.VerifyAll();
            writerMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
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
    }
}
