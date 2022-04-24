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
    using Prime.UserVarService;

    /// <summary>
    /// This class is intended to overwrite the members of the $itmeInterface$ interfaces to extend the test method $itmeTestMethod$.
    /// </summary>
    [TestClass]
    public class SIOEDCMain_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SIOEDCMain_UnitTest"/> class.
        /// </summary>
        public SIOEDCMain_UnitTest()
        {
            this.ConsoleOutput = new List<string>();
            this.ErrorOutput = new List<string>();
            this.GSDSValues = new Dictionary<string, string>();
            this.UserVars = new Dictionary<string, string>();

            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                Console.WriteLine(s);
                this.ConsoleOutput.Add(s);
            });
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) =>
                {
                    Console.WriteLine($"ERROR: {msg}");
                    this.ErrorOutput.Add(msg);
                });
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;

            this.DatalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            Prime.Services.DatalogService = this.DatalogServiceMock.Object;

            this.SharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.SharedServiceMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), Context.DUT)).Callback((string key, string value, Context context) =>
            {
                Console.WriteLine($"[GSDS] {key} = {value}");
                this.GSDSValues[key] = value;
            });
            this.SharedServiceMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), Context.DUT))
                .Returns((string key, Context context) => this.GSDSValues[key])
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"[GSDS] Reading {key}.");
                });
            this.SharedServiceMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), Context.LOT)).Callback((string key, string value, Context context) =>
            {
                Console.WriteLine($"[GSDS] {key} = {value}");
                this.GSDSValues[key] = value;
            });
            this.SharedServiceMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), Context.LOT))
                .Returns((string key, Context context) => this.GSDSValues[key])
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"[GSDS] Reading {key}.");
                });
            Prime.Services.SharedStorageService = this.SharedServiceMock.Object;

            this.UserVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            this.UserVarServiceMock.Setup(o => o.GetStringValue(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string collection, string uservar) => this.UserVars[$"{collection}.{uservar}"])
                .Callback((string collection, string uservar) =>
                {
                    Console.WriteLine($"[UserVar] Reading [{collection}.{uservar}]");
                });
            Prime.Services.UserVarService = this.UserVarServiceMock.Object;

            PathToFiles = SIOEDCMain_UnitTest.GetPathToFiles();
        }

        private static string PathToFiles { get; set; }

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private Mock<IDatalogService> DatalogServiceMock { get; set; }

        private Mock<ISharedStorageService> SharedServiceMock { get; set; }

        private Mock<IUserVarService> UserVarServiceMock { get; set; }

        private List<string> ConsoleOutput { get; set; }

        private List<string> ErrorOutput { get; set; }

        private Dictionary<string, string> GSDSValues { get; set; }

        private Dictionary<string, string> UserVars { get; set; }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_ParamEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var underTest = new SIOEDCMainTC { };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamNoEmpty_CaptToken_True()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "PSIO_cnvi_SKEWCALCTLE_calcode.txt", "./CNVI_formatfile.csv" });

            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var underTest = new SIOEDCMainTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "PSIO_cnvi_SKEWCALCTLE_calcode.txt",
                UserToken = "CNVI_LOOP_MAX_1P28_CAPTURE",
            };

            // [2] Call the method under test.
            underTest.Verify();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            fileServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_NoCaptureData()
        {
            var fileServiceMock = this.MockFileService(new List<string> { "PSIO_cnvi_SKEWCALCTLE_calcode.txt", "./CNVI_formatfile.csv" });

            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpFunctionsMock.Setup(o => o.ExecuteTestInstance(It.IsAny<string>())).Returns((string testname) => 1);
            tpFunctionsMock.Setup(o => o.GetTestInstanceParameter(It.IsAny<string>(), "patlist")).Returns("FakePListName");
            Prime.Services.TestProgramService = tpFunctionsMock.Object;

            this.GSDSValues["SIO_GSDS_BINDMEMDATA"] = string.Empty;

            var strgvalWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strgvalWriterMock.Setup(o => o.SetCustomTname("PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE_EDC_CAPTURE_L0_P0"));
            strgvalWriterMock.Setup(o => o.SetData("TOKEN=!Plist=FakePListName!RUN=1!CmpName=NA!TestType=CAPTURE!DataStart##DataEnd!KEY="));
            strgvalWriterMock.Setup(o => o.SetDelimiterCharacterForWrap('!'));

            this.DatalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strgvalWriterMock.Object);
            this.DatalogServiceMock.Setup(o => o.WriteToItuff(strgvalWriterMock.Object));
            /*
            var expectedOutput = new List<string>
            {
                "0_tname_PSIO_CNVI_ALL1::UTILOCAL_TX_CMEM_E_END_1P28_VMAX_OCAL120_SKEWCALCTLE_EDC_CAPTURE_L0_P0\n0_strgval_TOKEN=!Plist=FakePListName!RUN=1!CmpName=NA!TestType=CAPTURE!DataStart##DataEnd!KEY=\n",
            }; */

            var underTest = new SIOEDCMainTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = "PSIO_cnvi_SKEWCALCTLE_calcode.txt",
                UserToken = "CNVI_LOOP_MAX_1P28_CAPTURE",
            };

            underTest.Verify();

            var exitPort = underTest.Execute();
            Assert.AreEqual(1, exitPort, $"Exit port=[{exitPort}] expecting 1.");

            fileServiceMock.VerifyAll();
            this.DatalogServiceMock.VerifyAll();
            strgvalWriterMock.VerifyAll();
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
