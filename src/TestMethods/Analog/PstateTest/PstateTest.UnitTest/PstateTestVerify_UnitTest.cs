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

namespace PstateTest.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime;
    using Prime.ConsoleService;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.PinService;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class PstateTestVerify_UnitTest
    {
        /// <summary>
        /// Verify() fails due to empty LevelsTc param.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_LevelsParamEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("LevelsTc must be a valid string.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            PstateTest underTest = new PstateTest { LevelsTc = string.Empty };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Verify() fails due to empty TimingsTc param.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_TimingsParamEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("TimingsTc must be a valid string.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            PstateTest underTest = new PstateTest { LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom", TimingsTc = string.Empty };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Verify() fails due to empty TimingsTc param.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_PListParamEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("Patlist must be a valid string.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = string.Empty,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Verify() fails due to empty FailCount param.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_FailCountParamEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("FailCount must be a valid string.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = string.Empty,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Verify() fails due to empty InputFile param.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_FailCountLessThanZero_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("FailCount must be >= 0. Disabled: 0. Enabled: 1.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "-1",
                VidDomain = string.Empty,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Verify() fails due to empty CapturePins param.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_CapturePinsParamEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("CapturePins must be a valid comma-separated string.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "1",
                FuncVminGsdsToken = "FAST_UPSVFPASSFLOW",
                VidDomain = "SA",
                CapturePins = string.Empty,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Verify() fails due to empty InputFile param.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_VidDomainParamEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("VidDomain must be a valid string.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // Mock pin service
            string pins = "TDO";
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(pin => pin.Exists(pins)).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "1",
                CapturePins = pins,
                FuncVminGsdsToken = "FAST_UPSVFPASSFLOW",
                VidDomain = string.Empty,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Verify() fails due to empty FuncVminGsdsToken param.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_FuncVminGsdsTokenParamEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("FuncVminGsdsToken must be a valid string.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // Mock pin service
            string pins = "TDO";
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);

            // pinServiceMock.Setup(pin => pin.Exists(pins)).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "1",
                CapturePins = pins,
                VidDomain = "SA",
                FuncVminGsdsToken = string.Empty,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Verify() fails due to empty InputFile param.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_InputFileParamEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("InputFile must be a valid JSON file.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // Mock pin service
            string pins = "TDO";
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(pin => pin.Exists(pins)).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "1",
                CapturePins = pins,
                VidDomain = "SA",
                FuncVminGsdsToken = "FAST_UPSVFPASSFLOW",
                InputFile = string.Empty,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Verify() fails due to empty CapDataDef param.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_CapDataDefParamEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("CapDataDef must be a valid JSON file.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // mock file service
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);

            // fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock pin service
            string pins = "TDO";
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(pin => pin.Exists(pins)).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "1",
                CapturePins = pins,
                VidDomain = "SA",
                FuncVminGsdsToken = "FAST_UPSVFPASSFLOW",
                InputFile = inputFile,
                CapDataDef = string.Empty,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Verify() fails due to Domains dict missing VidDomain key.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_DomainsKeyMissing_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            string domain = "Core";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError($"InputFile Domain dict must contain key {domain}.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // mock file service
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);

            // fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock pin service
            string pins = "TDO";
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(pin => pin.Exists(pins)).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "1",
                CapturePins = pins,
                VidDomain = domain,
                FuncVminGsdsToken = "FAST_UPSVFPASSFLOW",
                InputFile = inputFile,
                CapDataDef = capFile,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Verify() fails due to more than one Domains dict key.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_MoreThanOneDomainsKey_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            string domain = "Core";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError($"InputFile Domain dict must contain only 1 domain: {domain}.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // mock file service
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\twoDomainsTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);

            // fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock pin service
            string pins = "TDO";
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(pin => pin.Exists(pins)).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "1",
                CapturePins = pins,
                VidDomain = domain,
                FuncVminGsdsToken = "FAST_UPSVFPASSFLOW",
                InputFile = inputFile,
                CapDataDef = capFile,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Verify() fails due to minV > maxV.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_CantConvertMinVToFloat_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            string domain = "SA";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("Can't convert number to float.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // mock file service
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\notFloatTestJson.json";

            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);

            // fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock pin service
            string pins = "TDO";
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(pin => pin.Exists(pins)).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "1",
                CapturePins = pins,
                VidDomain = domain,
                FuncVminGsdsToken = "FAST_UPSVFPASSFLOW",
                InputFile = inputFile,
                CapDataDef = capFile,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Verify() fails due to minV > maxV.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_MinMaxVoltagesSwapped_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            string domain = "SA";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError($"minV can't be larger than maxV: 1.2 > 0.5.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // mock file service
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\voltSwapTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);

            // fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock pin service
            string pins = "TDO";
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(pin => pin.Exists(pins)).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "1",
                CapturePins = pins,
                VidDomain = domain,
                FuncVminGsdsToken = "FAST_UPSVFPASSFLOW",
                InputFile = inputFile,
                CapDataDef = capFile,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Passes Verify(): InputFile found.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_TestPointGsdsTokenCountMismatch_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            // C:\Users\pourso\source\repos\tgl_poc_code\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json
            string domain = "SA";
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\testPointGsdsTokenMismatchJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);

            // fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            string inst = "VPU";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError($"Instance {inst} must have same number of freq points 3 as domain corners 4.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // Mock pin service
            string pins = "TDO";
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(pin => pin.Exists(pins)).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "1",
                CapturePins = pins,
                VidDomain = domain,
                FuncVminGsdsToken = "FAST_UPSVFPASSFLOW",
                InputFile = inputFile,
                CapDataDef = capFile,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Passes Verify(): CapData file missing position.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_CapDataFileMissingPosition_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            // C:\Users\pourso\source\repos\tgl_poc_code\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json
            string domain = "SA";
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\brokenCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock pin service
            string pins = "TDO";
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(pin => pin.Exists(pins)).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            // mock console service
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError($"Field: unlock for NoC must have bit positions.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "1",
                CapturePins = pins,
                MaskPins = string.Empty,
                VidDomain = domain,
                FuncVminGsdsToken = "FAST_UPSVFPASSFLOW",
                InputFile = inputFile,
                CapDataDef = capFile,
            };

            // Mock functional test
            // 1. mock fail data, cap data empty
            var listOfFailsPins = new List<IFailureData>();
            var mockIFailData = new Mock<IFailureData>();
            mockIFailData.Setup(failData => failData.GetPatternName()).Returns("tgl_pre_pat");
            listOfFailsPins.Add(mockIFailData.Object);
            var dictOfCapData = new Dictionary<string, string> { };

            // 2. mock functional test
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>();
            funcTestMock.Setup(func => func.Execute()).Returns(false);
            funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(listOfFailsPins);
            funcTestMock.Setup(func => func.GetCtvData()).Returns(dictOfCapData);

            // 3. mock func service
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);

            // funcServiceMock.Setup(func => func.GetFailAndCtvCaptureFuncTestMaskPins(
            funcServiceMock.Setup(func => func.CreateCaptureFailureAndCtvPerPinTest(
                underTest.Patlist,
                underTest.LevelsTc,
                underTest.TimingsTc,
                underTest.CapturePins,
                underTest.FailCountUlong,
                It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Passes Verify(): InputFile found.
        /// </summary>
        [TestMethod]
        public void Verify_NonEmptyInputFile_True()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            // C:\Users\pourso\source\repos\tgl_poc_code\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json
            string domain = "SA";
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock pin service
            string pins = "TDO";
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(pin => pin.Exists(pins)).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "1",
                CapturePins = pins,
                MaskPins = string.Empty,
                VidDomain = domain,
                FuncVminGsdsToken = "FAST_UPSVFPASSFLOW",
                InputFile = inputFile,
                CapDataDef = capFile,
            };

            // Mock functional test
            // 1. mock fail data, cap data empty
            var listOfFailsPins = new List<IFailureData>();
            var mockIFailData = new Mock<IFailureData>();
            mockIFailData.Setup(failData => failData.GetPatternName()).Returns("tgl_pre_pat");
            listOfFailsPins.Add(mockIFailData.Object);
            var dictOfCapData = new Dictionary<string, string> { };

            // 2. mock functional test
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>();
            funcTestMock.Setup(func => func.Execute()).Returns(false);
            funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(listOfFailsPins);
            funcTestMock.Setup(func => func.GetCtvData()).Returns(dictOfCapData);

            // 3. mock func service
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);

            // funcServiceMock.Setup(func => func.GetFailAndCtvCaptureFuncTestMaskPins(
            funcServiceMock.Setup(func => func.CreateCaptureFailureAndCtvPerPinTest(
                underTest.Patlist.ToString(),
                underTest.LevelsTc.ToString(),
                underTest.TimingsTc.ToString(),
                new List<string>(underTest.CapturePins.ToList()),
                1,
                It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // [2] Call the method under test.
            underTest.Verify();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            fileServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            funcServiceMock.VerifyAll();
        }
    }
}
