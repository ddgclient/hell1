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
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.PinService;
    using Prime.SharedStorageService;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class PstateTestExecute_UnitTest
    {
        /// <summary>
        /// Fails Execute(): UPS GSDS not successfully read.
        /// </summary>
        [TestMethod]
        public void Execute_FailsToReadGsds_ExceptionExitPortMinusOne()
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

            // Mock GSDS service
            string gsdsToken = "FAST_UPSVFPASSFLOW";

            // string gsdsValue = @"CR:4.900^1.356V1.399V1.286V1.365V-9999V-9999V-9999V-9999%4.200^1.036V1.075V1.034V1.059V-9999V-9999V-9999V-9999%3.400^0.843V0.866V0.827V0.835V-9999V-9999V-9999V-9999%2.200^0.643V0.658V0.636V0.636V-9999V-9999V-9999V-9999%1.200^0.510V0.530V0.510V0.510V-9999V-9999V-9999V-9999%0.400^0.300V0.300V0.300V0.300V-9999V-9999V-9999V-9999_CLR:4.300^1.115%3.600^0.900%3.000^0.809%1.800^0.625%0.800^0.514%0.400^0.470_CRX2:4.900^1.286V1.312V1.272V1.291V-9999V-9999V-9999V-9999%4.200^1.012V1.040V1.005V1.015V-9999V-9999V-9999V-9999%3.400^0.824V0.889V0.814V0.814V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_CRX3:4.800^1.262V1.296V1.240V1.288V-9999V-9999V-9999V-9999%4.200^1.038V1.070V1.022V1.032V-9999V-9999V-9999V-9999%3.400^0.834V0.889V0.824V0.824V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_GTS:1.350^0.911%1.100^0.771%0.900^0.700%0.600^0.630%0.300^0.560_SAQ:3.000^0.840%2.200^0.670%1.100^0.590_CRSSA:4.900^1.113V1.176V1.100V1.132V-9999V-9999V-9999V-9999%4.200^0.926V0.976V0.915V0.928V-9999V-9999V-9999V-9999%3.400^0.781V0.804V0.772V0.774V-9999V-9999V-9999V-9999%2.200^0.638V0.636V0.603V0.603V-9999V-9999V-9999V-9999%1.200^0.536V0.560V0.515V0.525V-9999V-9999V-9999V-9999%0.400^0.470V0.500V0.460V0.470V-9999V-9999V-9999V-9999_CLRSA:4.300^1.025%3.600^0.857%3.000^0.758%1.800^0.600%0.800^0.510%0.400^0.490_SAPS:1.000^0.750%0.200^0.580_SAIS:0.533^0.613%0.400^0.600%0.200^0.580_SAF:0.800^0.670%0.533^0.590_SACD:0.662^0.850%0.562^0.700%0.312^0.580_CDSSA:0.662^0.684%0.562^0.640%0.312^0.580_SA:1.350^1.158%1.100^0.922%0.900^0.790%0.600^0.660%0.300^0.540";
            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(sharedService => sharedService.GetStringRowFromTable(gsdsToken, Context.DUT)).Throws(new Exception(It.IsAny<string>()));
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            // Mock console service
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("Exception of type 'System.Exception' was thrown.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
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
                new List<string>(underTest.CapturePins.ToList()),
                It.IsAny<ulong>(),
                It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // verify test
            underTest.Verify();

            // [2] Call the method under test.
            int exitPort = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(-1, exitPort);
            consoleServiceMock.VerifyAll();
            funcServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            sharedServiceMock.VerifyAll();
        }

        /// <summary>
        /// Fails Execute(): GSDS doesnt contain VID domain.
        /// </summary>
        [TestMethod]
        public void Execute_GsdsDoesntContainVidDomain_ExceptionExitPortMinusOne()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            string domain = "SA";
            string gsdsName = "FAST_UPSVFPASSFLOW";

            // Mock FileService
            // C:\Users\pourso\source\repos\tgl_poc_code\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock GSDS service
            string gsdsToken = "FAST_UPSVFPASSFLOW";

            string gsdsValue = @"CR:4.900^1.356V1.399V1.286V1.365V-9999V-9999V-9999V-9999%4.200^1.036V1.075V1.034V1.059V-9999V-9999V-9999V-9999%3.400^0.843V0.866V0.827V0.835V-9999V-9999V-9999V-9999%2.200^0.643V0.658V0.636V0.636V-9999V-9999V-9999V-9999%1.200^0.510V0.530V0.510V0.510V-9999V-9999V-9999V-9999%0.400^0.300V0.300V0.300V0.300V-9999V-9999V-9999V-9999_CLR:4.300^1.115%3.600^0.900%3.000^0.809%1.800^0.625%0.800^0.514%0.400^0.470_CRX2:4.900^1.286V1.312V1.272V1.291V-9999V-9999V-9999V-9999%4.200^1.012V1.040V1.005V1.015V-9999V-9999V-9999V-9999%3.400^0.824V0.889V0.814V0.814V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_CRX3:4.800^1.262V1.296V1.240V1.288V-9999V-9999V-9999V-9999%4.200^1.038V1.070V1.022V1.032V-9999V-9999V-9999V-9999%3.400^0.834V0.889V0.824V0.824V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_GTS:1.350^0.911%1.100^0.771%0.900^0.700%0.600^0.630%0.300^0.560_SAQ:3.000^0.840%2.200^0.670%1.100^0.590_CRSSA:4.900^1.113V1.176V1.100V1.132V-9999V-9999V-9999V-9999%4.200^0.926V0.976V0.915V0.928V-9999V-9999V-9999V-9999%3.400^0.781V0.804V0.772V0.774V-9999V-9999V-9999V-9999%2.200^0.638V0.636V0.603V0.603V-9999V-9999V-9999V-9999%1.200^0.536V0.560V0.515V0.525V-9999V-9999V-9999V-9999%0.400^0.470V0.500V0.460V0.470V-9999V-9999V-9999V-9999_CLRSA:4.300^1.025%3.600^0.857%3.000^0.758%1.800^0.600%0.800^0.510%0.400^0.490_SAPS:1.000^0.750%0.200^0.580_SAIS:0.533^0.613%0.400^0.600%0.200^0.580_SAF:0.800^0.670%0.533^0.590_SACD:0.662^0.850%0.562^0.700%0.312^0.580_CDSSA:0.662^0.684%0.562^0.640%0.312^0.580_SAX:1.350^1.158%1.100^0.922%0.900^0.790%0.600^0.660%0.300^0.540";
            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(sharedService => sharedService.GetStringRowFromTable(gsdsToken, Context.DUT)).Returns(gsdsValue);
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            // Mock console service
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError($"{gsdsName} doesn't contain domain {domain}.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
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
                MaskPins = string.Empty,
                VidDomain = domain,
                FuncVminGsdsToken = gsdsName,
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
                new List<string>(underTest.CapturePins.ToList()),
                It.IsAny<ulong>(),
                It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            underTest.Verify();

            // [2] Call the method under test.
            int exitPort = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(-1, exitPort);
            funcServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            sharedServiceMock.VerifyAll();
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Fails Execute(): GSDS doesnt contain VID domain.
        /// </summary>
        [TestMethod]
        public void Execute_GsdsCantBeDecoded_ExceptionExitPortMinusOne()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            string domain = "SA";
            string gsdsName = "FAST_UPSVFPASSFLOW";

            // Mock FileService
            // C:\Users\pourso\source\repos\tgl_poc_code\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock GSDS service
            string gsdsToken = "FAST_UPSVFPASSFLOW";

            string gsdsValue = @"_SA:";
            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(sharedService => sharedService.GetStringRowFromTable(gsdsToken, Context.DUT)).Returns(gsdsValue);
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            // Mock console service
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError($"Can't split {gsdsName} into valid VF strings for domain {domain}.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
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
                MaskPins = string.Empty,
                VidDomain = domain,
                FuncVminGsdsToken = gsdsName,
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
                new List<string>(underTest.CapturePins.ToList()),
                It.IsAny<ulong>(),
                It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            underTest.Verify();

            // [2] Call the method under test.
            int exitPort = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(-1, exitPort);
            funcServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            sharedServiceMock.VerifyAll();
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Passes Execute(): Failed capdata limit, fail count > 0.
        /// </summary>
        [TestMethod]
        public void Execute_CapMemLimitFailure_ExitPortThree()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            // C:\Users\pourso\source\repos\tgl_poc_code\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json
            string domain = "SA";
            string gsdsName = "FAST_UPSVFPASSFLOW";
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateDecodeTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock GSDS service
            string gsdsToken = "FAST_UPSVFPASSFLOW";
            string gsdsValue = @"CR:4.900^1.356V1.399V1.286V1.365V-9999V-9999V-9999V-9999%4.200^1.036V1.075V1.034V1.059V-9999V-9999V-9999V-9999%3.400^0.843V0.866V0.827V0.835V-9999V-9999V-9999V-9999%2.200^0.643V0.658V0.636V0.636V-9999V-9999V-9999V-9999%1.200^0.510V0.530V0.510V0.510V-9999V-9999V-9999V-9999%0.400^0.300V0.300V0.300V0.300V-9999V-9999V-9999V-9999_CLR:4.300^1.115%3.600^0.900%3.000^0.809%1.800^0.625%0.800^0.514%0.400^0.470_CRX2:4.900^1.286V1.312V1.272V1.291V-9999V-9999V-9999V-9999%4.200^1.012V1.040V1.005V1.015V-9999V-9999V-9999V-9999%3.400^0.824V0.889V0.814V0.814V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_CRX3:4.800^1.262V1.296V1.240V1.288V-9999V-9999V-9999V-9999%4.200^1.038V1.070V1.022V1.032V-9999V-9999V-9999V-9999%3.400^0.834V0.889V0.824V0.824V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_GTS:1.350^0.911%1.100^0.771%0.900^0.700%0.600^0.630%0.300^0.560_SAQ:3.000^0.840%2.200^0.670%1.100^0.590_CRSSA:4.900^1.113V1.176V1.100V1.132V-9999V-9999V-9999V-9999%4.200^0.926V0.976V0.915V0.928V-9999V-9999V-9999V-9999%3.400^0.781V0.804V0.772V0.774V-9999V-9999V-9999V-9999%2.200^0.638V0.636V0.603V0.603V-9999V-9999V-9999V-9999%1.200^0.536V0.560V0.515V0.525V-9999V-9999V-9999V-9999%0.400^0.470V0.500V0.460V0.470V-9999V-9999V-9999V-9999_CLRSA:4.300^1.025%3.600^0.857%3.000^0.758%1.800^0.600%0.800^0.510%0.400^0.490_SAPS:1.000^0.750%0.200^0.580_SAIS:0.533^0.613%0.400^0.600%0.200^0.580_SAF:0.800^0.670%0.533^0.590_SACD:0.662^0.850%0.562^0.700%0.312^0.580_CDSSA:0.662^0.684%0.562^0.640%0.312^0.580_SA:2.000^1.158%1.500^0.790%1.050^0.660";
            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(sharedService => sharedService.GetStringRowFromTable(gsdsToken, Context.DUT)).Returns(gsdsValue);
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

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
                FuncVminGsdsToken = gsdsName,
                InputFile = inputFile,
                CapDataDef = capFile,
            };

            // Mock functional test
            // 1. mock fail data, cap data empty
            var listOfFailsPins = new List<IFailureData> { };
            var mockIFailData = new Mock<IFailureData>();
            mockIFailData.Setup(failData => failData.GetPatternName()).Returns(string.Empty);

            // listOfFailsPins.Add(mockIFailData.Object);
            var dictOfCapData = new Dictionary<string, string> { };
            dictOfCapData.Add("TDO", "101011100000000000000000000000001010111100000000000000000000000010101111000000000000000000000000101011100000000000000000000000001010111100000000000000000000000010101111000000000000000000000000");

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
                new List<string>(underTest.CapturePins.ToList()),
                It.IsAny<ulong>(),
                It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;
            underTest.Verify();

            // 4. mock datalog service
            var datalogMock = new Mock<IStrgvalFormat>();
            datalogMock.Setup(i => i.SetData(It.IsAny<string>()));
            datalogMock.Setup(i => i.SetTnamePostfix(It.IsAny<string>()));
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(d => d.GetItuffStrgvalWriter()).Returns(datalogMock.Object);
            datalogServiceMock.Setup(e => e.WriteToItuff(datalogMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            // [2] Call the method under test.
            int exitPort = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(3, exitPort);
            funcServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            sharedServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Passes Execute(): fail cap mem data for freqs below curve.
        /// </summary>
        [TestMethod]
        public void Execute_CapMemFreqsBelowCurve_ExitPortThree()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            // C:\Users\pourso\source\repos\tgl_poc_code\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json
            string domain = "SA";
            string gsdsName = "FAST_UPSVFPASSFLOW";
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateDecodeTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock GSDS service
            string gsdsToken = "FAST_UPSVFPASSFLOW";
            string gsdsValue = @"CR:4.900^1.356V1.399V1.286V1.365V-9999V-9999V-9999V-9999%4.200^1.036V1.075V1.034V1.059V-9999V-9999V-9999V-9999%3.400^0.843V0.866V0.827V0.835V-9999V-9999V-9999V-9999%2.200^0.643V0.658V0.636V0.636V-9999V-9999V-9999V-9999%1.200^0.510V0.530V0.510V0.510V-9999V-9999V-9999V-9999%0.400^0.300V0.300V0.300V0.300V-9999V-9999V-9999V-9999_CLR:4.300^1.115%3.600^0.900%3.000^0.809%1.800^0.625%0.800^0.514%0.400^0.470_CRX2:4.900^1.286V1.312V1.272V1.291V-9999V-9999V-9999V-9999%4.200^1.012V1.040V1.005V1.015V-9999V-9999V-9999V-9999%3.400^0.824V0.889V0.814V0.814V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_CRX3:4.800^1.262V1.296V1.240V1.288V-9999V-9999V-9999V-9999%4.200^1.038V1.070V1.022V1.032V-9999V-9999V-9999V-9999%3.400^0.834V0.889V0.824V0.824V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_GTS:1.350^0.911%1.100^0.771%0.900^0.700%0.600^0.630%0.300^0.560_SAQ:3.000^0.840%2.200^0.670%1.100^0.590_CRSSA:4.900^1.113V1.176V1.100V1.132V-9999V-9999V-9999V-9999%4.200^0.926V0.976V0.915V0.928V-9999V-9999V-9999V-9999%3.400^0.781V0.804V0.772V0.774V-9999V-9999V-9999V-9999%2.200^0.638V0.636V0.603V0.603V-9999V-9999V-9999V-9999%1.200^0.536V0.560V0.515V0.525V-9999V-9999V-9999V-9999%0.400^0.470V0.500V0.460V0.470V-9999V-9999V-9999V-9999_CLRSA:4.300^1.025%3.600^0.857%3.000^0.758%1.800^0.600%0.800^0.510%0.400^0.490_SAPS:1.000^0.750%0.200^0.580_SAIS:0.533^0.613%0.400^0.600%0.200^0.580_SAF:0.800^0.670%0.533^0.590_SACD:0.662^0.850%0.562^0.700%0.312^0.580_CDSSA:0.662^0.684%0.562^0.640%0.312^0.580_SA:1.350^1.158%1.100^0.790%0.600^0.660";
            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(sharedService => sharedService.GetStringRowFromTable(gsdsToken, Context.DUT)).Returns(gsdsValue);
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

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
                FuncVminGsdsToken = gsdsName,
                InputFile = inputFile,
                CapDataDef = capFile,
            };

            // Mock functional test
            // 1. mock fail data, cap data empty
            var listOfFailsPins = new List<IFailureData> { };
            var mockIFailData = new Mock<IFailureData>();
            mockIFailData.Setup(failData => failData.GetPatternName()).Returns(string.Empty);

            // listOfFailsPins.Add(mockIFailData.Object);
            var dictOfCapData = new Dictionary<string, string> { };
            dictOfCapData.Add("TDO", "101011100000000000000000000000001010111100000000000000000000000010101111000000000000000000000000101011100000000000000000000000001010111100000000000000000000000010101111000000000000000000000000");

            // 2. mock functional test
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>();
            funcTestMock.Setup(func => func.Execute()).Returns(true);
            funcTestMock.Setup(func => func.GetCtvData()).Returns(dictOfCapData);

            // 3. mock func service
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);

            // funcServiceMock.Setup(func => func.GetFailAndCtvCaptureFuncTestMaskPins(
            funcServiceMock.Setup(func => func.CreateCaptureFailureAndCtvPerPinTest(
                underTest.Patlist,
                underTest.LevelsTc,
                underTest.TimingsTc,
                new List<string>(underTest.CapturePins.ToList()),
                It.IsAny<ulong>(),
                It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // 4. mock datalog service
            var datalogMock = new Mock<IStrgvalFormat>();
            datalogMock.Setup(i => i.SetData(It.IsAny<string>()));
            datalogMock.Setup(i => i.SetTnamePostfix(It.IsAny<string>()));
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(d => d.GetItuffStrgvalWriter()).Returns(datalogMock.Object);
            datalogServiceMock.Setup(e => e.WriteToItuff(datalogMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            underTest.Verify();

            // [2] Call the method under test.
            int exitPort = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(3, exitPort);
            datalogServiceMock.VerifyAll();
            funcServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            sharedServiceMock.VerifyAll();
        }

        /// <summary>
        /// Passes Execute(): Failed pre-amble, fail count > 0.
        /// </summary>
        [TestMethod]
        public void Execute_PreamblePatternFailureFailCountOne_ExitPortTwo()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            // C:\Users\pourso\source\repos\tgl_poc_code\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json
            string domain = "SA";
            string gsdsName = "FAST_UPSVFPASSFLOW";
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock GSDS service
            string gsdsToken = "FAST_UPSVFPASSFLOW";
            string gsdsValue = @"CR:4.900^1.356V1.399V1.286V1.365V-9999V-9999V-9999V-9999%4.200^1.036V1.075V1.034V1.059V-9999V-9999V-9999V-9999%3.400^0.843V0.866V0.827V0.835V-9999V-9999V-9999V-9999%2.200^0.643V0.658V0.636V0.636V-9999V-9999V-9999V-9999%1.200^0.510V0.530V0.510V0.510V-9999V-9999V-9999V-9999%0.400^0.300V0.300V0.300V0.300V-9999V-9999V-9999V-9999_CLR:4.300^1.115%3.600^0.900%3.000^0.809%1.800^0.625%0.800^0.514%0.400^0.470_CRX2:4.900^1.286V1.312V1.272V1.291V-9999V-9999V-9999V-9999%4.200^1.012V1.040V1.005V1.015V-9999V-9999V-9999V-9999%3.400^0.824V0.889V0.814V0.814V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_CRX3:4.800^1.262V1.296V1.240V1.288V-9999V-9999V-9999V-9999%4.200^1.038V1.070V1.022V1.032V-9999V-9999V-9999V-9999%3.400^0.834V0.889V0.824V0.824V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_GTS:1.350^0.911%1.100^0.771%0.900^0.700%0.600^0.630%0.300^0.560_SAQ:3.000^0.840%2.200^0.670%1.100^0.590_CRSSA:4.900^1.113V1.176V1.100V1.132V-9999V-9999V-9999V-9999%4.200^0.926V0.976V0.915V0.928V-9999V-9999V-9999V-9999%3.400^0.781V0.804V0.772V0.774V-9999V-9999V-9999V-9999%2.200^0.638V0.636V0.603V0.603V-9999V-9999V-9999V-9999%1.200^0.536V0.560V0.515V0.525V-9999V-9999V-9999V-9999%0.400^0.470V0.500V0.460V0.470V-9999V-9999V-9999V-9999_CLRSA:4.300^1.025%3.600^0.857%3.000^0.758%1.800^0.600%0.800^0.510%0.400^0.490_SAPS:1.000^0.750%0.200^0.580_SAIS:0.533^0.613%0.400^0.600%0.200^0.580_SAF:0.800^0.670%0.533^0.590_SACD:0.662^0.850%0.562^0.700%0.312^0.580_CDSSA:0.662^0.684%0.562^0.640%0.312^0.580_SA:1.350^1.158%1.100^0.922%0.900^0.790%0.600^0.660%0.300^0.540";
            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(sharedService => sharedService.GetStringRowFromTable(gsdsToken, Context.DUT)).Returns(gsdsValue);
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

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
                FuncVminGsdsToken = gsdsName,
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
            dictOfCapData.Add("TDO", "1010111000000000000000000000000010101111000000000000000000000000101011110000000000000000000000001010111000000000000000000000000010101111000000000000000000000000101011110000000000000000000000001010111000000000000000000000000010101111000000000000000000000000101011110000000000000000000000001010111000000000000000000000000010101111000000000000000000000000101011110000000000000000000000001010111000000000000000000000000010101111000000000000000000000000101011110000000000000000000000001010111000000000000000000000000010101111000000000000000000000000101011110000000000000000000000001010111000000000000000000000000010101111000000000000000000000000101011110000000000000000000000001010111000000000000000000000000010101111000000000000000000000000101011110000000000000000000000001010111000000000000000000000000010101111000000000000000000000000101011110000000000000000000000001010111000000000000000000000000010101111000000000000000000000000101011110000000000000000000000001010111000000000000000000000000010101111000000000000000000000000101011110000000000000000000000001010111000000000000000000000000010101111000000000000000000000000101011110000000000000000000000001010111100000000000000000000000010101111000000000000000000000000");

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
                new List<string>(underTest.CapturePins.ToList()),
                It.IsAny<ulong>(),
                It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // 4. mock datalog service
            var datalogMock = new Mock<IStrgvalFormat>();
            datalogMock.Setup(i => i.SetData(It.IsAny<string>()));
            datalogMock.Setup(i => i.SetTnamePostfix(It.IsAny<string>()));
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(d => d.GetItuffStrgvalWriter()).Returns(datalogMock.Object);
            datalogServiceMock.Setup(e => e.WriteToItuff(datalogMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            underTest.Verify();

            // [2] Call the method under test.
            int exitPort = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(2, exitPort);
            funcServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            sharedServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Passes Execute(): Failed pre-amble, fail count > 0.
        /// </summary>
        [TestMethod]
        public void Execute_PreamblePatternFailureFailCountZero_ExitPortOne()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            // C:\Users\pourso\source\repos\tgl_poc_code\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json
            string domain = "SA";
            string gsdsName = "FAST_UPSVFPASSFLOW";
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock GSDS service
            string gsdsToken = "FAST_UPSVFPASSFLOW";
            string gsdsValue = @"CR:4.900^1.356V1.399V1.286V1.365V-9999V-9999V-9999V-9999%4.200^1.036V1.075V1.034V1.059V-9999V-9999V-9999V-9999%3.400^0.843V0.866V0.827V0.835V-9999V-9999V-9999V-9999%2.200^0.643V0.658V0.636V0.636V-9999V-9999V-9999V-9999%1.200^0.510V0.530V0.510V0.510V-9999V-9999V-9999V-9999%0.400^0.300V0.300V0.300V0.300V-9999V-9999V-9999V-9999_CLR:4.300^1.115%3.600^0.900%3.000^0.809%1.800^0.625%0.800^0.514%0.400^0.470_CRX2:4.900^1.286V1.312V1.272V1.291V-9999V-9999V-9999V-9999%4.200^1.012V1.040V1.005V1.015V-9999V-9999V-9999V-9999%3.400^0.824V0.889V0.814V0.814V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_CRX3:4.800^1.262V1.296V1.240V1.288V-9999V-9999V-9999V-9999%4.200^1.038V1.070V1.022V1.032V-9999V-9999V-9999V-9999%3.400^0.834V0.889V0.824V0.824V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_GTS:1.350^0.911%1.100^0.771%0.900^0.700%0.600^0.630%0.300^0.560_SAQ:3.000^0.840%2.200^0.670%1.100^0.590_CRSSA:4.900^1.113V1.176V1.100V1.132V-9999V-9999V-9999V-9999%4.200^0.926V0.976V0.915V0.928V-9999V-9999V-9999V-9999%3.400^0.781V0.804V0.772V0.774V-9999V-9999V-9999V-9999%2.200^0.638V0.636V0.603V0.603V-9999V-9999V-9999V-9999%1.200^0.536V0.560V0.515V0.525V-9999V-9999V-9999V-9999%0.400^0.470V0.500V0.460V0.470V-9999V-9999V-9999V-9999_CLRSA:4.300^1.025%3.600^0.857%3.000^0.758%1.800^0.600%0.800^0.510%0.400^0.490_SAPS:1.000^0.750%0.200^0.580_SAIS:0.533^0.613%0.400^0.600%0.200^0.580_SAF:0.800^0.670%0.533^0.590_SACD:0.662^0.850%0.562^0.700%0.312^0.580_CDSSA:0.662^0.684%0.562^0.640%0.312^0.580_SA:1.350^1.158%1.100^0.922%0.900^0.790%0.600^0.660%0.300^0.540";
            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(sharedService => sharedService.GetStringRowFromTable(gsdsToken, Context.DUT)).Returns(gsdsValue);
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

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
                FailCount = "0",
                CapturePins = pins,
                MaskPins = string.Empty,
                VidDomain = domain,
                FuncVminGsdsToken = gsdsName,
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
            dictOfCapData.Add("TDO", "1110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111100000000000000000000000011101111000000000000000000000000");

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
                new List<string>(underTest.CapturePins.ToList()),
                It.IsAny<ulong>(),
                It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // 4. mock datalog service
            var datalogMock = new Mock<IStrgvalFormat>();
            datalogMock.Setup(i => i.SetData(It.IsAny<string>()));
            datalogMock.Setup(i => i.SetTnamePostfix(It.IsAny<string>()));
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(d => d.GetItuffStrgvalWriter()).Returns(datalogMock.Object);
            datalogServiceMock.Setup(e => e.WriteToItuff(datalogMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            underTest.Verify();

            // [2] Call the method under test.
            int exitPort = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(1, exitPort);
            funcServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            sharedServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Passes Execute(): Failed pre-amble, fail count > 0.
        /// </summary>
        [TestMethod]
        public void Execute_NoPatternFailureCapDataEmpty_ExitPortMinusOne()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            // C:\Users\pourso\source\repos\tgl_poc_code\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json
            string domain = "SA";
            string gsdsName = "FAST_UPSVFPASSFLOW";
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock GSDS service
            string gsdsToken = "FAST_UPSVFPASSFLOW";
            string gsdsValue = @"CR:4.900^1.356V1.399V1.286V1.365V-9999V-9999V-9999V-9999%4.200^1.036V1.075V1.034V1.059V-9999V-9999V-9999V-9999%3.400^0.843V0.866V0.827V0.835V-9999V-9999V-9999V-9999%2.200^0.643V0.658V0.636V0.636V-9999V-9999V-9999V-9999%1.200^0.510V0.530V0.510V0.510V-9999V-9999V-9999V-9999%0.400^0.300V0.300V0.300V0.300V-9999V-9999V-9999V-9999_CLR:4.300^1.115%3.600^0.900%3.000^0.809%1.800^0.625%0.800^0.514%0.400^0.470_CRX2:4.900^1.286V1.312V1.272V1.291V-9999V-9999V-9999V-9999%4.200^1.012V1.040V1.005V1.015V-9999V-9999V-9999V-9999%3.400^0.824V0.889V0.814V0.814V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_CRX3:4.800^1.262V1.296V1.240V1.288V-9999V-9999V-9999V-9999%4.200^1.038V1.070V1.022V1.032V-9999V-9999V-9999V-9999%3.400^0.834V0.889V0.824V0.824V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_GTS:1.350^0.911%1.100^0.771%0.900^0.700%0.600^0.630%0.300^0.560_SAQ:3.000^0.840%2.200^0.670%1.100^0.590_CRSSA:4.900^1.113V1.176V1.100V1.132V-9999V-9999V-9999V-9999%4.200^0.926V0.976V0.915V0.928V-9999V-9999V-9999V-9999%3.400^0.781V0.804V0.772V0.774V-9999V-9999V-9999V-9999%2.200^0.638V0.636V0.603V0.603V-9999V-9999V-9999V-9999%1.200^0.536V0.560V0.515V0.525V-9999V-9999V-9999V-9999%0.400^0.470V0.500V0.460V0.470V-9999V-9999V-9999V-9999_CLRSA:4.300^1.025%3.600^0.857%3.000^0.758%1.800^0.600%0.800^0.510%0.400^0.490_SAPS:1.000^0.750%0.200^0.580_SAIS:0.533^0.613%0.400^0.600%0.200^0.580_SAF:0.800^0.670%0.533^0.590_SACD:0.662^0.850%0.562^0.700%0.312^0.580_CDSSA:0.662^0.684%0.562^0.640%0.312^0.580_SA:1.350^1.158%1.100^0.922%0.900^0.790%0.600^0.660%0.300^0.540";
            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(sharedService => sharedService.GetStringRowFromTable(gsdsToken, Context.DUT)).Returns(gsdsValue);
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            // Mock pin service
            string pins = "TDO";
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(pin => pin.Exists(pins)).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            // Mock console service
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError($"Capture data is empty.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            PstateTest underTest = new PstateTest
            {
                LevelsTc = "CLK_LJPLL_ALL::CLK_univ_lvl_CLK_nom",
                TimingsTc = "IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
                Patlist = "pstate_soc_list",
                FailCount = "0",
                CapturePins = pins,
                MaskPins = string.Empty,
                VidDomain = domain,
                FuncVminGsdsToken = gsdsName,
                InputFile = inputFile,
                CapDataDef = capFile,
            };

            // Mock functional test
            // 1. mock fail data, cap data empty
            var listOfFailsPins = new List<IFailureData>();
            var mockIFailData = new Mock<IFailureData> { };
            var dictOfCapData = new Dictionary<string, string> { };

            // 2. mock functional test
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>();
            funcTestMock.Setup(func => func.Execute()).Returns(true);
            funcTestMock.Setup(func => func.GetCtvData()).Returns(dictOfCapData);

            // 3. mock func service
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);

            // funcServiceMock.Setup(func => func.GetFailAndCtvCaptureFuncTestMaskPins(
            funcServiceMock.Setup(func => func.CreateCaptureFailureAndCtvPerPinTest(
                underTest.Patlist,
                underTest.LevelsTc,
                underTest.TimingsTc,
                new List<string>(underTest.CapturePins.ToList()),
                It.IsAny<ulong>(),
                It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            underTest.Verify();

            // [2] Call the method under test.
            int exitPort = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(-1, exitPort);
            consoleServiceMock.VerifyAll();
            funcServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            sharedServiceMock.VerifyAll();
        }

        /// <summary>
        /// Passes Execute(): InputFile found.
        /// </summary>
        [TestMethod]
        public void Execute_NonEmptyInputFile_True()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            // C:\Users\pourso\source\repos\tgl_poc_code\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json
            string domain = "SA";
            string gsdsName = "FAST_UPSVFPASSFLOW";
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock GSDS service
            string gsdsToken = "FAST_UPSVFPASSFLOW";
            string gsdsValue = @"CR:4.900^1.356V1.399V1.286V1.365V-9999V-9999V-9999V-9999%4.200^1.036V1.075V1.034V1.059V-9999V-9999V-9999V-9999%3.400^0.843V0.866V0.827V0.835V-9999V-9999V-9999V-9999%2.200^0.643V0.658V0.636V0.636V-9999V-9999V-9999V-9999%1.200^0.510V0.530V0.510V0.510V-9999V-9999V-9999V-9999%0.400^0.300V0.300V0.300V0.300V-9999V-9999V-9999V-9999_CLR:4.300^1.115%3.600^0.900%3.000^0.809%1.800^0.625%0.800^0.514%0.400^0.470_CRX2:4.900^1.286V1.312V1.272V1.291V-9999V-9999V-9999V-9999%4.200^1.012V1.040V1.005V1.015V-9999V-9999V-9999V-9999%3.400^0.824V0.889V0.814V0.814V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_CRX3:4.800^1.262V1.296V1.240V1.288V-9999V-9999V-9999V-9999%4.200^1.038V1.070V1.022V1.032V-9999V-9999V-9999V-9999%3.400^0.834V0.889V0.824V0.824V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_GTS:1.350^0.911%1.100^0.771%0.900^0.700%0.600^0.630%0.300^0.560_SAQ:3.000^0.840%2.200^0.670%1.100^0.590_CRSSA:4.900^1.113V1.176V1.100V1.132V-9999V-9999V-9999V-9999%4.200^0.926V0.976V0.915V0.928V-9999V-9999V-9999V-9999%3.400^0.781V0.804V0.772V0.774V-9999V-9999V-9999V-9999%2.200^0.638V0.636V0.603V0.603V-9999V-9999V-9999V-9999%1.200^0.536V0.560V0.515V0.525V-9999V-9999V-9999V-9999%0.400^0.470V0.500V0.460V0.470V-9999V-9999V-9999V-9999_CLRSA:4.300^1.025%3.600^0.857%3.000^0.758%1.800^0.600%0.800^0.510%0.400^0.490_SAPS:1.000^0.750%0.200^0.580_SAIS:0.533^0.613%0.400^0.600%0.200^0.580_SAF:0.800^0.670%0.533^0.590_SACD:0.662^0.850%0.562^0.700%0.312^0.580_CDSSA:0.662^0.684%0.562^0.640%0.312^0.580_SA:1.350^1.158%1.100^0.922%0.900^0.790%0.600^0.660%0.300^0.540";
            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(sharedService => sharedService.GetStringRowFromTable(gsdsToken, Context.DUT)).Returns(gsdsValue);
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

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
                FailCount = "0",
                CapturePins = pins,
                MaskPins = string.Empty,
                VidDomain = domain,
                FuncVminGsdsToken = gsdsName,
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
            dictOfCapData.Add("TDO", "1110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111000000000000000000000000011101111000000000000000000000000111011110000000000000000000000001110111100000000000000000000000011101111000000000000000000000000");

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
                new List<string>(underTest.CapturePins.ToList()),
                It.IsAny<ulong>(),
                It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            // 4. mock datalog service
            var datalogMock = new Mock<IStrgvalFormat>();
            datalogMock.Setup(i => i.SetData(It.IsAny<string>()));
            datalogMock.Setup(i => i.SetTnamePostfix(It.IsAny<string>()));
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(d => d.GetItuffStrgvalWriter()).Returns(datalogMock.Object);
            datalogServiceMock.Setup(e => e.WriteToItuff(datalogMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            underTest.Verify();

            // [2] Call the method under test.
            int exitPort = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(1, exitPort);
            funcServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            sharedServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Passes Execute(): Passes capdata limit, fail count > 0.
        /// </summary>
        [TestMethod]
        public void Execute_CapMemLimitPassDebugPrint_ExitPortOne()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            // C:\Users\pourso\source\repos\tgl_poc_code\Analog\PstateTest\PstateTest.UnitTest\pstateTestJson.json
            string domain = "SA";
            string gsdsName = "FAST_UPSVFPASSFLOW";
            string inputFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateDecodeTestJson.json";
            string capFile = @"..\..\src\TestMethods\Analog\PstateTest\PstateTest.UnitTest\pstateCapData.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            fileServiceMock.Setup(fileService => fileService.GetFile(capFile)).Returns(capFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock GSDS service
            string gsdsToken = "FAST_UPSVFPASSFLOW";
            string gsdsValue = @"CR:4.900^1.356V1.399V1.286V1.365V-9999V-9999V-9999V-9999%4.200^1.036V1.075V1.034V1.059V-9999V-9999V-9999V-9999%3.400^0.843V0.866V0.827V0.835V-9999V-9999V-9999V-9999%2.200^0.643V0.658V0.636V0.636V-9999V-9999V-9999V-9999%1.200^0.510V0.530V0.510V0.510V-9999V-9999V-9999V-9999%0.400^0.300V0.300V0.300V0.300V-9999V-9999V-9999V-9999_CLR:4.300^1.115%3.600^0.900%3.000^0.809%1.800^0.625%0.800^0.514%0.400^0.470_CRX2:4.900^1.286V1.312V1.272V1.291V-9999V-9999V-9999V-9999%4.200^1.012V1.040V1.005V1.015V-9999V-9999V-9999V-9999%3.400^0.824V0.889V0.814V0.814V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_CRX3:4.800^1.262V1.296V1.240V1.288V-9999V-9999V-9999V-9999%4.200^1.038V1.070V1.022V1.032V-9999V-9999V-9999V-9999%3.400^0.834V0.889V0.824V0.824V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_GTS:1.350^0.911%1.100^0.771%0.900^0.700%0.600^0.630%0.300^0.560_SAQ:3.000^0.840%2.200^0.670%1.100^0.590_CRSSA:4.900^1.113V1.176V1.100V1.132V-9999V-9999V-9999V-9999%4.200^0.926V0.976V0.915V0.928V-9999V-9999V-9999V-9999%3.400^0.781V0.804V0.772V0.774V-9999V-9999V-9999V-9999%2.200^0.638V0.636V0.603V0.603V-9999V-9999V-9999V-9999%1.200^0.536V0.560V0.515V0.525V-9999V-9999V-9999V-9999%0.400^0.470V0.500V0.460V0.470V-9999V-9999V-9999V-9999_CLRSA:4.300^1.025%3.600^0.857%3.000^0.758%1.800^0.600%0.800^0.510%0.400^0.490_SAPS:1.000^0.750%0.200^0.580_SAIS:0.533^0.613%0.400^0.600%0.200^0.580_SAF:0.800^0.670%0.533^0.590_SACD:0.662^0.850%0.562^0.700%0.312^0.580_CDSSA:0.662^0.684%0.562^0.640%0.312^0.580_SA:2.000^1.158%1.500^0.790%1.050^0.660";
            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(sharedService => sharedService.GetStringRowFromTable(gsdsToken, Context.DUT)).Returns(gsdsValue);
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            // Mock console service
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
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
                MaskPins = string.Empty,
                VidDomain = domain,
                FuncVminGsdsToken = gsdsName,
                InputFile = inputFile,
                CapDataDef = capFile,
                LogLevel = TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
            };

            // Mock functional test
            // 1. mock fail data, cap data empty
            var listOfFailsPins = new List<IFailureData> { };
            var mockIFailData = new Mock<IFailureData>();
            mockIFailData.Setup(failData => failData.GetPatternName()).Returns(string.Empty);

            // listOfFailsPins.Add(mockIFailData.Object);
            var dictOfCapData = new Dictionary<string, string> { };
            dictOfCapData.Add("TDO", "111011100000000000000000000000001010111100000000000000000000000010101111000000000000000000000000101011100000000000000000000000001010111100000000000000000000000010101111000000000000000000000000");

            // 2. mock functional test
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>();
            funcTestMock.Setup(func => func.Execute()).Returns(true);
            funcTestMock.Setup(func => func.GetCtvData()).Returns(dictOfCapData);

            // 3. mock func service
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);

            // funcServiceMock.Setup(func => func.GetFailAndCtvCaptureFuncTestMaskPins(
            funcServiceMock.Setup(func => func.CreateCaptureFailureAndCtvPerPinTest(
                underTest.Patlist,
                underTest.LevelsTc,
                underTest.TimingsTc,
                new List<string>(underTest.CapturePins.ToList()),
                It.IsAny<ulong>(),
                It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            underTest.Verify();

            // 4. mock datalog service
            var datalogMock = new Mock<IStrgvalFormat>();
            datalogMock.Setup(i => i.SetData(It.IsAny<string>()));
            datalogMock.Setup(i => i.SetTnamePostfix(It.IsAny<string>()));
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(d => d.GetItuffStrgvalWriter()).Returns(datalogMock.Object);
            datalogServiceMock.Setup(e => e.WriteToItuff(datalogMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            // [2] Call the method under test.
            int exitPort = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(3, exitPort);
            consoleServiceMock.VerifyAll();
            funcServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
            sharedServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }
    }
}
