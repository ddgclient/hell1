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
    using Prime.UserVarService;
    using SIO;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class SIOShmoo_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SIOShmoo_UnitTest"/> class.
        /// </summary>
        public SIOShmoo_UnitTest()
        {
            this.ConsoleOutput = new List<string>();
            this.ErrorOutput = new List<string>();
            this.ItuffOutput = new List<string>();
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

            this.FileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            this.FileServiceMock.Setup(o => o.GetFile(It.IsAny<string>())).Returns((string s) => Path.IsPathRooted(s) ? s : GetPathToFiles() + Path.GetFileName(s));
            this.FileServiceMock.Setup(o => o.FileExists(It.IsAny<string>())).Returns(true);
            Prime.Services.FileService = this.FileServiceMock.Object;

            this.DatalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            /* this.DatalogServiceMock.Setup(o => o.WriteToItuff(It.IsAny<string>())).Callback((string s) =>
            {
                Console.WriteLine($"[ITUFF]{s.Replace("\n", "\n[ITUFF]")}");
                this.ItuffOutput.Add(s);
            }); */
            Prime.Services.DatalogService = this.DatalogServiceMock.Object;

            this.SharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
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

            PathToFiles = SIOShmoo_UnitTest.GetPathToFiles();
        }

        private static string PathToFiles { get; set; }

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private Mock<IFileService> FileServiceMock { get; set; }

        private Mock<IDatalogService> DatalogServiceMock { get; set; }

        private Mock<ISharedStorageService> SharedServiceMock { get; set; }

        private Mock<IUserVarService> UserVarServiceMock { get; set; }

        private List<string> ConsoleOutput { get; set; }

        private List<string> ErrorOutput { get; set; }

        private List<string> ItuffOutput { get; set; }

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
            SIOShmooTC underTest = new SIOShmooTC { };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamNoEmpty_FuncToken_True()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            SIOShmooTC underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = PathToFiles + "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt",
                UserToken = "DP_LPBK_1620_FUNC",
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamNoEmpty_ShmooToken_True()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            SIOShmooTC underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = PathToFiles + "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt",
                UserToken = "DP_LPBK_5400_SHMOO",
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void RunShmoo_XYZW_Func()
        {
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpFunctionsMock.Setup(o => o.ExecuteTestInstance(It.IsAny<string>())).Returns((string testname) => 0); // FIXME - add some more complicated pass/fail results.
            tpFunctionsMock.Setup(o => o.GetTestInstanceParameter(It.IsAny<string>(), "patlist")).Returns("FakePListName");
            tpFunctionsMock.Setup(o => o.SetTestInstanceParameter(It.IsAny<string>(), "modify_token_dynamic_subset", It.IsAny<string>()));
            Prime.Services.TestProgramService = tpFunctionsMock.Object;

            /* List<string> expectedOutput = new List<string>
            {
                "0_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_1620_FUNC_svtt_SHMOO_GONOGO_L0_P0\n0_strgval_TOKEN=SET_RTERM_SCALAR:1;SET_FSSCALAR:1!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd\n",
                "0_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_1620_FUNC_svtt_SHMOO_GONOGO_L1_P0\n0_strgval_TOKEN=SET_RTERM_SCALAR:1;SET_FSSCALAR:2!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd\n",
                "0_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_1620_FUNC_svtt_SHMOO_GONOGO_L2_P0\n0_strgval_TOKEN=SET_RTERM_SCALAR:2;SET_FSSCALAR:1!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd\n",
                "0_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_1620_FUNC_svtt_SHMOO_GONOGO_L3_P0\n0_strgval_TOKEN=SET_RTERM_SCALAR:2;SET_FSSCALAR:2!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd\n",
                "0_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_1620_FUNC_svtt_SHMOO_GONOGO_L4_P0\n0_strgval_TOKEN=SET_RTERM_SCALAR:3;SET_FSSCALAR:1!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd\n",
                "0_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_1620_FUNC_svtt_SHMOO_GONOGO_L5_P0\n0_strgval_TOKEN=SET_RTERM_SCALAR:3;SET_FSSCALAR:2!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd\n",
            }; */

            var ituffTnameDataPairs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_1620_FUNC_svtt_SHMOO_GONOGO_L0_P0", "TOKEN=SET_RTERM_SCALAR:1;SET_FSSCALAR:1!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd"),
                new Tuple<string, string>("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_1620_FUNC_svtt_SHMOO_GONOGO_L1_P0", "TOKEN=SET_RTERM_SCALAR:1;SET_FSSCALAR:2!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd"),
                new Tuple<string, string>("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_1620_FUNC_svtt_SHMOO_GONOGO_L2_P0", "TOKEN=SET_RTERM_SCALAR:2;SET_FSSCALAR:1!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd"),
                new Tuple<string, string>("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_1620_FUNC_svtt_SHMOO_GONOGO_L3_P0", "TOKEN=SET_RTERM_SCALAR:2;SET_FSSCALAR:2!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd"),
                new Tuple<string, string>("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_1620_FUNC_svtt_SHMOO_GONOGO_L4_P0", "TOKEN=SET_RTERM_SCALAR:3;SET_FSSCALAR:1!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd"),
                new Tuple<string, string>("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_1620_FUNC_svtt_SHMOO_GONOGO_L5_P0", "TOKEN=SET_RTERM_SCALAR:3;SET_FSSCALAR:2!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd"),
            };

            var ituffWriterMock = this.AddItuffStgvalMock(ituffTnameDataPairs);

            var sio = new SIOLib(true);
            var token = "DP_LPBK_1620_FUNC";
            var userFileData = new UserFile(PathToFiles + "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt");

            Assert.IsTrue(userFileData.Valid, "Failed to read UserFile.");

            var shmooSetupValid = sio.ShmooTestSetup(userFileData, token);
            Assert.IsTrue(shmooSetupValid, "Failed to setup shmoo data.");

            userFileData.TokenBlocks[token].ShmooSingleTestPointFunc = sio.RunShmooSinglePoint;
            var result = sio.RunShmoo(userFileData.TokenBlocks[token]);
            Assert.AreEqual(0, result, $"Shmoo had {result} setup failures.");

            // check ituff too..
            ituffWriterMock.VerifyAll();
            this.DatalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void RunShmoo_XY_Shmoo()
        {
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpFunctionsMock.Setup(o => o.ExecuteTestInstance(It.IsAny<string>())).Returns((string testname) => 0); // FIXME - add some more complicated pass/fail results.
            tpFunctionsMock.Setup(o => o.GetTestInstanceParameter(It.IsAny<string>(), "patlist")).Returns("FakePListName");
            tpFunctionsMock.Setup(o => o.SetTestInstanceParameter(It.IsAny<string>(), "modify_token_dynamic_subset", It.IsAny<string>()));
            Prime.Services.TestProgramService = tpFunctionsMock.Object;

            /* List<string> expectedOutput = new List<string>
            {
                "0_comnt_TOKEN=SET_SWING:1;SET_RXODELAYSEL:0;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:1;SET_RXODELAYSEL:1;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:1;SET_RXODELAYSEL:2;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:1;SET_RXODELAYSEL:3;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:1;SET_RXODELAYSEL:7;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:1;SET_RXODELAYSEL:11;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:1;SET_RXODELAYSEL:15;RUN:0\n",

                "0_comnt_TOKEN=SET_SWING:2;SET_RXODELAYSEL:0;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:2;SET_RXODELAYSEL:1;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:2;SET_RXODELAYSEL:2;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:2;SET_RXODELAYSEL:3;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:2;SET_RXODELAYSEL:7;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:2;SET_RXODELAYSEL:11;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:2;SET_RXODELAYSEL:15;RUN:0\n",

                "0_comnt_TOKEN=SET_SWING:3;SET_RXODELAYSEL:0;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:3;SET_RXODELAYSEL:1;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:3;SET_RXODELAYSEL:2;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:3;SET_RXODELAYSEL:3;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:3;SET_RXODELAYSEL:7;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:3;SET_RXODELAYSEL:11;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:3;SET_RXODELAYSEL:15;RUN:0\n",

                "0_comnt_TOKEN=SET_SWING:8;SET_RXODELAYSEL:0;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:8;SET_RXODELAYSEL:1;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:8;SET_RXODELAYSEL:2;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:8;SET_RXODELAYSEL:3;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:8;SET_RXODELAYSEL:7;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:8;SET_RXODELAYSEL:11;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:8;SET_RXODELAYSEL:15;RUN:0\n",

                "0_comnt_TOKEN=SET_SWING:11;SET_RXODELAYSEL:0;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:11;SET_RXODELAYSEL:1;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:11;SET_RXODELAYSEL:2;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:11;SET_RXODELAYSEL:3;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:11;SET_RXODELAYSEL:7;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:11;SET_RXODELAYSEL:11;RUN:0\n",
                "0_comnt_TOKEN=SET_SWING:11;SET_RXODELAYSEL:15;RUN:0\n",
            }; */

            var ituffCommentMock = this.AddItuffComntMock(
                new List<string>
                {
                "TOKEN=SET_SWING:1;SET_RXODELAYSEL:0;RUN:0",
                "TOKEN=SET_SWING:1;SET_RXODELAYSEL:1;RUN:0",
                "TOKEN=SET_SWING:1;SET_RXODELAYSEL:2;RUN:0",
                "TOKEN=SET_SWING:1;SET_RXODELAYSEL:3;RUN:0",
                "TOKEN=SET_SWING:1;SET_RXODELAYSEL:7;RUN:0",
                "TOKEN=SET_SWING:1;SET_RXODELAYSEL:11;RUN:0",
                "TOKEN=SET_SWING:1;SET_RXODELAYSEL:15;RUN:0",

                "TOKEN=SET_SWING:2;SET_RXODELAYSEL:0;RUN:0",
                "TOKEN=SET_SWING:2;SET_RXODELAYSEL:1;RUN:0",
                "TOKEN=SET_SWING:2;SET_RXODELAYSEL:2;RUN:0",
                "TOKEN=SET_SWING:2;SET_RXODELAYSEL:3;RUN:0",
                "TOKEN=SET_SWING:2;SET_RXODELAYSEL:7;RUN:0",
                "TOKEN=SET_SWING:2;SET_RXODELAYSEL:11;RUN:0",
                "TOKEN=SET_SWING:2;SET_RXODELAYSEL:15;RUN:0",

                "TOKEN=SET_SWING:3;SET_RXODELAYSEL:0;RUN:0",
                "TOKEN=SET_SWING:3;SET_RXODELAYSEL:1;RUN:0",
                "TOKEN=SET_SWING:3;SET_RXODELAYSEL:2;RUN:0",
                "TOKEN=SET_SWING:3;SET_RXODELAYSEL:3;RUN:0",
                "TOKEN=SET_SWING:3;SET_RXODELAYSEL:7;RUN:0",
                "TOKEN=SET_SWING:3;SET_RXODELAYSEL:11;RUN:0",
                "TOKEN=SET_SWING:3;SET_RXODELAYSEL:15;RUN:0",

                "TOKEN=SET_SWING:8;SET_RXODELAYSEL:0;RUN:0",
                "TOKEN=SET_SWING:8;SET_RXODELAYSEL:1;RUN:0",
                "TOKEN=SET_SWING:8;SET_RXODELAYSEL:2;RUN:0",
                "TOKEN=SET_SWING:8;SET_RXODELAYSEL:3;RUN:0",
                "TOKEN=SET_SWING:8;SET_RXODELAYSEL:7;RUN:0",
                "TOKEN=SET_SWING:8;SET_RXODELAYSEL:11;RUN:0",
                "TOKEN=SET_SWING:8;SET_RXODELAYSEL:15;RUN:0",

                "TOKEN=SET_SWING:11;SET_RXODELAYSEL:0;RUN:0",
                "TOKEN=SET_SWING:11;SET_RXODELAYSEL:1;RUN:0",
                "TOKEN=SET_SWING:11;SET_RXODELAYSEL:2;RUN:0",
                "TOKEN=SET_SWING:11;SET_RXODELAYSEL:3;RUN:0",
                "TOKEN=SET_SWING:11;SET_RXODELAYSEL:7;RUN:0",
                "TOKEN=SET_SWING:11;SET_RXODELAYSEL:11;RUN:0",
                "TOKEN=SET_SWING:11;SET_RXODELAYSEL:15;RUN:0",
                });

            var sio = new SIOLib(true);
            var token = "DP_LPBK_5400_SHMOO";
            var userFileData = new UserFile(PathToFiles + "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt");

            Assert.IsTrue(userFileData.Valid, "Failed to read UserFile.");

            var shmooSetupValid = sio.ShmooTestSetup(userFileData, token);
            Assert.IsTrue(shmooSetupValid, "Failed to setup shmoo data.");
            userFileData.TokenBlocks[token].TestInstanceIsShmoo = true; // HACK since we can't read parameters yet.
            userFileData.TokenBlocks[token].ShmooSingleTestPointFunc = sio.RunShmooSinglePoint;

            var result = sio.RunShmoo(userFileData.TokenBlocks[token]);
            Assert.AreEqual(0, result, $"Shmoo had {result} setup failures.");

            // check ituff too..
            ituffCommentMock.VerifyAll();
            this.DatalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_XYZW_Func_NoFails()
        {
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpFunctionsMock.Setup(o => o.ExecuteTestInstance(It.IsAny<string>())).Returns((string testname) => 0); // FIXME - add some more complicated pass/fail results.
            tpFunctionsMock.Setup(o => o.GetTestInstanceParameter(It.IsAny<string>(), "patlist")).Returns("FakePListName");
            tpFunctionsMock.Setup(o => o.SetTestInstanceParameter(It.IsAny<string>(), "modify_token_dynamic_subset", It.IsAny<string>()));
            Prime.Services.TestProgramService = tpFunctionsMock.Object;

            /* List<string> expectedOutput = new List<string>
            {
                "0_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_2700_FUNC_svtt_SHMOO_GONOGO_L0_P0\n0_strgval_TOKEN=SET_RTERM_SCALAR:1;SET_FSSCALAR:1!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd\n",
                "0_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_2700_FUNC_svtt_SHMOO_GONOGO_L1_P0\n0_strgval_TOKEN=SET_RTERM_SCALAR:1;SET_FSSCALAR:2!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd\n",
                "0_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_2700_FUNC_svtt_SHMOO_GONOGO_L2_P0\n0_strgval_TOKEN=SET_RTERM_SCALAR:2;SET_FSSCALAR:1!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd\n",
                "0_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_2700_FUNC_svtt_SHMOO_GONOGO_L3_P0\n0_strgval_TOKEN=SET_RTERM_SCALAR:2;SET_FSSCALAR:2!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd\n",
                "0_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_2700_FUNC_svtt_SHMOO_GONOGO_L4_P0\n0_strgval_TOKEN=SET_RTERM_SCALAR:3;SET_FSSCALAR:1!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd\n",
                "0_tname_SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_2700_FUNC_svtt_SHMOO_GONOGO_L5_P0\n0_strgval_TOKEN=SET_RTERM_SCALAR:3;SET_FSSCALAR:2!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd\n",
            }; */

            var ituffTnameDataPairs = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_2700_FUNC_svtt_SHMOO_GONOGO_L0_P0", "TOKEN=SET_RTERM_SCALAR:1;SET_FSSCALAR:1!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd"),
                new Tuple<string, string>("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_2700_FUNC_svtt_SHMOO_GONOGO_L1_P0", "TOKEN=SET_RTERM_SCALAR:1;SET_FSSCALAR:2!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd"),
                new Tuple<string, string>("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_2700_FUNC_svtt_SHMOO_GONOGO_L2_P0", "TOKEN=SET_RTERM_SCALAR:2;SET_FSSCALAR:1!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd"),
                new Tuple<string, string>("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_2700_FUNC_svtt_SHMOO_GONOGO_L3_P0", "TOKEN=SET_RTERM_SCALAR:2;SET_FSSCALAR:2!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd"),
                new Tuple<string, string>("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_2700_FUNC_svtt_SHMOO_GONOGO_L4_P0", "TOKEN=SET_RTERM_SCALAR:3;SET_FSSCALAR:1!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd"),
                new Tuple<string, string>("SIO_DP_LPBK1::DP_X_ANELB_K_END_X_X_X_2700_FUNC_svtt_SHMOO_GONOGO_L5_P0", "TOKEN=SET_RTERM_SCALAR:3;SET_FSSCALAR:2!Plist=FakePListName!RUN=1!CmpName=NA!TestType=GONOGO!NumberID=1!IDValue=GONOGO!YName=SET_SWING!YValue=1;2;3;5;6;8;11!XName=SET_RXODELAYSEL!XValue=0;1;2;3;7;11;15!DataStart#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#0;0;0;0;0;0;0#DataEnd"),
            };

            var ituffWriterMock = this.AddItuffStgvalMock(ituffTnameDataPairs);

            SIOShmooTC underTest = new SIOShmooTC
            {
                InstanceName = "SIO::SomeTestInstanceName",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                UserFile = PathToFiles + "SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt",
                UserToken = "DP_LPBK_2700_FUNC",
            };

            underTest.Verify();

            var exitPort = underTest.Execute();
            Assert.AreEqual(1, exitPort, $"Exit port=[{exitPort}] expecting 1.");

            // check ituff too..
            ituffWriterMock.VerifyAll();
            this.DatalogServiceMock.VerifyAll();
        }

        private static string GetPathToFiles([CallerFilePath] string srcPath = "")
        {
            return Path.GetDirectoryName(srcPath) + "\\input_files\\";
        }

        private Mock<IStrgvalFormat> AddItuffStgvalMock(List<Tuple<string, string>> nameAndData)
        {
            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetDelimiterCharacterForWrap('!'));
            foreach (var pair in nameAndData)
            {
                writerMock.Setup(o => o.SetCustomTname(pair.Item1));
                writerMock.Setup(o => o.SetData(pair.Item2));
            }

            this.DatalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            this.DatalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            return writerMock;
        }

        private Mock<IComntFormat> AddItuffComntMock(List<string> data)
        {
            var writerMock = new Mock<IComntFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.IncludeTnameInPrint(false));
            foreach (var comment in data)
            {
                writerMock.Setup(o => o.SetData(comment));
            }

            this.DatalogServiceMock.Setup(o => o.GetItuffComntWriter()).Returns(writerMock.Object);
            this.DatalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            return writerMock;
        }
    }
}
