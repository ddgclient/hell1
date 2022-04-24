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

namespace PllVidWalkTC
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
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class PllVidWalkTC_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PllVidWalkTC_UnitTest"/> class.
        /// </summary>
        public PllVidWalkTC_UnitTest()
        {
            this.GSDSValues = new Dictionary<string, double>();
            this.GSDSStrValues = new Dictionary<string, string>();
            this.UserVarsInt = new Dictionary<string, int>();

            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => { Console.WriteLine(s); });
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => { Console.WriteLine($"ERROR: {msg}"); });
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;

            this.SharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.SharedServiceMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<double>(), Context.DUT)).Callback((string key, double value, Context context) =>
            {
                Console.WriteLine($"[GSDS] {key} = {value}");
                this.GSDSValues[key] = value;
            });
            this.SharedServiceMock.Setup(o => o.GetDoubleRowFromTable(It.IsAny<string>(), Context.DUT))
                .Returns((string key, Context context) => this.GSDSValues[key])
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"[GSDS] Reading {key}.");
                });
            this.SharedServiceMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), Context.DUT))
                .Returns((string key, Context context) => this.GSDSStrValues[key])
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"[GSDS] Reading {key}.");
                });
            Prime.Services.SharedStorageService = this.SharedServiceMock.Object;

            this.UserVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            this.UserVarServiceMock.Setup(o => o.GetIntValue(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string collection, string uservar) => this.UserVarsInt[$"{collection}.{uservar}"])
                .Callback((string collection, string uservar) =>
                {
                    Console.WriteLine($"[UserVar] Reading [{collection}.{uservar}]");
                });
            Prime.Services.UserVarService = this.UserVarServiceMock.Object;

            this.FileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            this.FileServiceMock.Setup(o => o.GetFile(It.IsAny<string>())).Returns((string s) => s);
            Prime.Services.FileService = this.FileServiceMock.Object;

            PathToFiles = PllVidWalkTC_UnitTest.GetPathToFiles();

            Console.WriteLine("Done with constructor");
        }

        private static string PathToFiles { get; set; }

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private Mock<ISharedStorageService> SharedServiceMock { get; set; }

        private Mock<IFileService> FileServiceMock { get; set; }

        private Mock<IUserVarService> UserVarServiceMock { get; set; }

        private Dictionary<string, double> GSDSValues { get; set; }

        private Dictionary<string, string> GSDSStrValues { get; set; }

        private Dictionary<string, int> UserVarsInt { get; set; }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Verify_ParamEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            PllVidWalkTC underTest = new PllVidWalkTC
            {
                InstanceName = "CLK_ADPLL_ALL::AD_COREPLL_VIDWK_K_END_X_X_X_X_VNOM_100_DCA",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                PllName = string.Empty,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamNoEmpty_True()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // need to force the fusecfg test to pass verify and execute.
            // the VID test should fail
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpFunctionsMock.Setup(o => o.VerifyTestInstance("FakeFuseCfgTest")).Returns(true);
            Prime.Services.TestProgramService = tpFunctionsMock.Object;

            PllVidWalkTC underTest = new PllVidWalkTC
            {
                InstanceName = "CLK_ADPLL_ALL::AD_COREPLL_VIDWK_K_END_X_X_X_X_VNOM_100_DCA",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                InputFile = PathToFiles + "input_file_tgl42uy.csv",
                PllName = "COREPLL",
                MaxVoltage = 1.35,
                MinVoltage = 0.4,
                FuseCfgTest = "FakeFuseCfgTest",
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_AllDefault_Fail()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.UserVarsInt["CLK_ADPLL_ALL::CLK_ADPLL_ALL.VIDWK_EXECUTE_ALL_POINTS"] = 0;
            this.GSDSValues["CLK_ADPLL_VIDWK_LOCKTIME_CORE0"] = 1.23456;
            this.GSDSValues["CLK_ADPLL_VIDWK_LOCKTIME_CORE1"] = 2.3492;
            this.GSDSValues["CLK_ADPLL_VIDWK_LOCKTIME_CORE2"] = 3.1111;
            this.GSDSValues["CLK_ADPLL_VIDWK_LOCKTIME_CORE3"] = 0.9998;

            // need to force the fusecfg test to pass verify and execute.
            // the VID test should fail
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpFunctionsMock.Setup(o => o.VerifyTestInstance("FakeFuseCfgTest")).Returns(true);
            tpFunctionsMock.Setup(o => o.SetTestInstanceParameter("FakeFuseCfgTest", "function_parameter", It.IsAny<string>()));
            tpFunctionsMock.Setup(o => o.ExecuteTestInstance(It.IsAny<string>())).Returns((string testname) => testname == "FakeFuseCfgTest" ? 1 : 0);
            Prime.Services.TestProgramService = tpFunctionsMock.Object;

            var ituffStrgvalMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            ituffStrgvalMock.Setup(o => o.SetTnamePostfix(It.IsAny<string>()));
            ituffStrgvalMock.Setup(o => o.SetData(It.IsAny<string>()));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var ituffSepMock = new Mock<ISeparatorFormat>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffSeparatorFormatWriter()).Returns(ituffSepMock.Object);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(ituffStrgvalMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(ituffSepMock.Object));
            datalogServiceMock.Setup(o => o.WriteToItuff(ituffStrgvalMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            PllVidWalkTC underTest = new PllVidWalkTC
            {
                InstanceName = "CLK_ADPLL_ALL::AD_COREPLL_VIDWK_K_END_X_X_X_X_VNOM_100_DCA",
                InputFile = PathToFiles + "input_file_tgl42uy.csv",
                PllName = "COREPLL",
                MaxVoltage = 1.35,
                MinVoltage = 0.4,
                FuseCfgTest = "FakeFuseCfgTest",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };

            underTest.Verify();

            var exitPort = underTest.Execute();
            Assert.AreEqual(2, exitPort);
            datalogServiceMock.VerifyAll();
            ituffStrgvalMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_AllDefault_Pass()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.UserVarsInt["CLK_ADPLL_ALL::CLK_ADPLL_ALL.VIDWK_EXECUTE_ALL_POINTS"] = 1;

            // FIXME - need to return multiple values
            this.GSDSValues["CLK_ADPLL_VIDWK_LOCKTIME_CORE0"] = 1.23456;
            this.GSDSValues["CLK_ADPLL_VIDWK_LOCKTIME_CORE1"] = 2.3492;
            this.GSDSValues["CLK_ADPLL_VIDWK_LOCKTIME_CORE2"] = 3.1111;
            this.GSDSValues["CLK_ADPLL_VIDWK_LOCKTIME_CORE3"] = 0.9998;

            // need to force the fusecfg test to pass verify and execute.
            // the VID test should fail
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpFunctionsMock.Setup(o => o.VerifyTestInstance("FakeFuseCfgTest")).Returns(true);
            tpFunctionsMock.Setup(o => o.SetTestInstanceParameter("FakeFuseCfgTest", "function_parameter", It.IsAny<string>()));
            tpFunctionsMock.Setup(o => o.ExecuteTestInstance(It.IsAny<string>())).Returns((string testname) => testname == "FakeFuseCfgTest" ? 1 : 1);
            Prime.Services.TestProgramService = tpFunctionsMock.Object;

            var ituffStrgvalMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            ituffStrgvalMock.Setup(o => o.SetTnamePostfix(It.IsAny<string>()));
            ituffStrgvalMock.Setup(o => o.SetData(It.IsAny<string>()));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var ituffSepMock = new Mock<ISeparatorFormat>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffSeparatorFormatWriter()).Returns(ituffSepMock.Object);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(ituffStrgvalMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(ituffSepMock.Object));
            datalogServiceMock.Setup(o => o.WriteToItuff(ituffStrgvalMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            PllVidWalkTC underTest = new PllVidWalkTC
            {
                InstanceName = "CLK_ADPLL_ALL::AD_COREPLL_VIDWK_K_END_X_X_X_X_VNOM_100_DCA",
                InputFile = PathToFiles + "input_file_tgl42uy.csv",
                PllName = "COREPLL",
                MaxVoltage = 1.35,
                MinVoltage = 0.4,
                FuseCfgTest = "FakeFuseCfgTest",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };

            underTest.Verify();

            // FIXME need to check more than just the port
            var exitPort = underTest.Execute();
            Assert.AreEqual(1, exitPort);
            datalogServiceMock.VerifyAll();
            ituffStrgvalMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_Test1_Pass()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.GSDSStrValues["FAST_UPSVFPASSFLOW"] = "CR:4.400^1.067v1.037v1.054v1.043v-9999v-9999v-9999v-9999%4.200^0.985v0.971v0.987v0.968v-9999v-9999v-9999v-9999%3.400^0.807v0.792v0.802v0.802v-9999v-9999v-9999v-9999%2.200^0.615v0.613v0.630v0.633v-9999v-9999v-9999v-9999%1.200^0.540v0.530v0.530v0.530v-9999v-9999v-9999v-9999%0.400^0.500v0.490v0.500v0.520v-9999v-9999v-9999v-9999_CRF:4.400^1.091v1.078v1.086v1.062v-9999v-9999v-9999v-9999%4.200^0.985v0.971v0.987v0.968v-9999v-9999v-9999v-9999%3.400^0.807v0.792v0.802v0.802v-9999v-9999v-9999v-9999%2.200^0.615v0.613v0.630v0.613v-9999v-9999v-9999v-9999%1.200^0.530v0.530v0.520v0.530v-9999v-9999v-9999v-9999%0.400^0.480v0.480v0.480v0.480v-9999v-9999v-9999v-9999_CLR:4.000^0.948%3.600^0.835%3.000^0.725%1.800^0.595%0.800^0.560%0.400^0.550_CRX2:4.300^1.041v1.026v1.035v1.026v-9999v-9999v-9999v-9999%4.200^1.005v0.991v0.997v0.988v-9999v-9999v-9999v-9999%3.400^0.817v0.812v0.822v0.812v-9999v-9999v-9999v-9999%2.200^0.628v0.623v0.630v0.633v-9999v-9999v-9999v-9999%1.200^0.540v0.530v0.530v0.530v-9999v-9999v-9999v-9999%0.400^0.480v0.480v0.490v0.480v-9999v-9999v-9999v-9999_CRX3:4.300^1.085v1.066v1.085v1.066v-9999v-9999v-9999v-9999%4.200^1.045v1.031v1.047v1.028v-9999v-9999v-9999v-9999%3.400^0.847v0.832v0.842v0.822v-9999v-9999v-9999v-9999%2.200^0.645v0.633v0.640v0.633v-9999v-9999v-9999v-9999%1.200^0.540v0.530v0.530v0.530v-9999v-9999v-9999v-9999%0.400^0.480v0.480v0.480v0.480v-9999v-9999v-9999v-9999_GTS:1.300^0.859%1.100^0.753%0.900^0.680%0.600^0.600%0.300^0.580_SAQ:2.700^0.760%2.200^0.670%1.100^0.570_SAPS:1.000^0.830%0.400^0.610%0.200^0.560_SAIS:0.533^0.690%0.200^0.560_SAF:0.800^0.670%0.533^0.570_SACD:0.662^0.744%0.562^0.680%0.312^0.560_GTSM:1.100^0.873%0.900^0.770%0.600^0.650%0.300^0.560";
            this.UserVarsInt["CLK_ADPLL_ALL::CLK_ADPLL_ALL.VIDWK_EXECUTE_ALL_POINTS"] = 0;
            this.GSDSValues["CLK_ADPLL_VIDWK_LOCKTIME_CORE0"] = 1.23456;
            this.GSDSValues["CLK_ADPLL_VIDWK_LOCKTIME_CORE1"] = 2.3492;
            this.GSDSValues["CLK_ADPLL_VIDWK_LOCKTIME_CORE2"] = 3.1111;
            this.GSDSValues["CLK_ADPLL_VIDWK_LOCKTIME_CORE3"] = 0.9998;

            // need to force the fusecfg test to pass verify and execute.
            // the VID test should fail
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpFunctionsMock.Setup(o => o.VerifyTestInstance("FakeFuseCfgTest")).Returns(true);
            tpFunctionsMock.Setup(o => o.SetTestInstanceParameter("FakeFuseCfgTest", "function_parameter", It.IsAny<string>()));
            tpFunctionsMock.Setup(o => o.ExecuteTestInstance(It.IsAny<string>())).Returns((string testname) => testname == "FakeFuseCfgTest" ? 1 : 1);
            Prime.Services.TestProgramService = tpFunctionsMock.Object;

            var ituffStrgvalMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            ituffStrgvalMock.Setup(o => o.SetTnamePostfix(It.IsAny<string>()));
            ituffStrgvalMock.Setup(o => o.SetData(It.IsAny<string>()));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var ituffSepMock = new Mock<ISeparatorFormat>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffSeparatorFormatWriter()).Returns(ituffSepMock.Object);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(ituffStrgvalMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(ituffSepMock.Object));
            datalogServiceMock.Setup(o => o.WriteToItuff(ituffStrgvalMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            PllVidWalkTC underTest = new PllVidWalkTC
            {
                InstanceName = "CLK_ADPLL_ALL::AD_COREPLL_VIDWK_K_END_X_X_X_X_VNOM_100_DCA",
                InputFile = PathToFiles + "input_file_tgl42uy.csv",
                PllName = "COREPLL",
                MaxVoltage = 1.35,
                MinVoltage = 0.4,
                FuseCfgTest = "FakeFuseCfgTest",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };

            underTest.Verify();

            var exitPort = underTest.Execute();
            Assert.AreEqual(1, exitPort);
            datalogServiceMock.VerifyAll();
            ituffStrgvalMock.VerifyAll();
        }

        private static string GetPathToFiles([CallerFilePath] string srcPath = "")
        {
            return Path.GetDirectoryName(srcPath) + "\\InputFiles\\";
        }
    }
}
