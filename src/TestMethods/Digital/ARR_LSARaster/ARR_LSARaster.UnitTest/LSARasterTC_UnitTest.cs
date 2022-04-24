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

namespace LSARasterTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using global::LSARasterTC;
    using global::LSARasterTC.UnitTest.MockObjects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.EvergreenService;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PatternService;
    using Prime.PlistService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;
    using Prime.UserVarService;
    using Prime.VoltageService;

    /// <summary>
    /// Unit test class for <see cref="LSARasterTC"/>.
    /// </summary>
    [TestClass]
    public class LSARasterTC_UnitTest
    {
        private Mock<IConsoleService> mockConsole = new Mock<IConsoleService>();
        private Mock<IFileService> mockFile = new Mock<IFileService>();
        private Mock<ISharedStorageService> mockSharedStorage = new Mock<ISharedStorageService>();
        private Mock<IUserVarService> mockUserVar = new Mock<IUserVarService>();
        private Mock<ITestProgramService> mockTp = new Mock<ITestProgramService>();
        private Mock<IPatConfigService> mockPatConfig = new Mock<IPatConfigService>();
        private Mock<IDatalogService> mockDatalog = new Mock<IDatalogService>();
        private Mock<IVoltageService> mockVoltage = new Mock<IVoltageService>();
        private Mock<IFunctionalService> mockFunctional = new Mock<IFunctionalService>();
        private Mock<IStrgvalFormat> mockStrgval = new Mock<IStrgvalFormat>();
        private Mock<IMrsltFormat> mockMrslt = new Mock<IMrsltFormat>();
        private Mock<ICaptureFailureAndCtvPerPinTest> mockTest = new Mock<ICaptureFailureAndCtvPerPinTest>();

        private MetadataConfig deserializedMetadata;
        private RasterConfig deserializedRaster;
        private RasterConfig deserializedRasterRC;
        private RasterConfig deserializedRasterNP;

        private string validInputWithReductionConfig = File.ReadAllText(".\\TestInput\\RasterConfig_ReductionConfigSet.json");

        /// <summary>
        /// Dummy summary.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            this.mockFile.Setup(x => x.FileExists(It.IsAny<string>()))
          .Returns((string filepath) => { return System.IO.File.Exists(filepath); });
            this.mockFile.Setup(x => x.GetFile(It.IsAny<string>()))
                .Returns((string filepath) => { return filepath; });

            Prime.Services.FileService = this.mockFile.Object;

            this.mockConsole.Setup(x => x.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => { Console.WriteLine($"ERROR: {msg}"); });
            this.mockConsole.Setup(x => x.PrintDebug(It.IsAny<string>()))
                .Callback<string>((string msg) => { Console.WriteLine($"{msg}"); });
            Prime.Services.ConsoleService = this.mockConsole.Object;

            this.mockStrgval.Setup(x => x.SetTnamePostfix(It.IsAny<string>())).Verifiable();
            this.mockStrgval.Setup(x => x.SetDelimiterCharacterForWrap(It.IsAny<char>())).Verifiable();
            this.mockStrgval.Setup(x => x.SetData(It.IsAny<string>())).Verifiable();
            this.mockStrgval.Setup(x => x.ToString()).Returns("fake ituff string");

            this.mockMrslt.Setup(x => x.SetTnamePostfix(It.IsAny<string>())).Verifiable();
            this.mockMrslt.Setup(x => x.SetData(It.IsAny<int>())).Verifiable();

            this.mockDatalog.Setup(x => x.WriteToItuff(It.IsAny<IMrsltFormat>())).Verifiable();
            this.mockDatalog.Setup(x => x.GetItuffStrgvalWriter()).Returns(this.mockStrgval.Object);
            this.mockDatalog.Setup(x => x.GetItuffMrsltWriter()).Returns(this.mockMrslt.Object);

            Prime.Services.DatalogService = this.mockDatalog.Object;

            Mock<IFivrCondition> mockFivrCondition = new Mock<IFivrCondition>();
            mockFivrCondition.Setup(x => x.ApplyCondition()).Verifiable();
            this.mockVoltage.Setup(x => x.CreateFivrForCondition(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(mockFivrCondition.Object);
            Prime.Services.VoltageService = this.mockVoltage.Object;

            this.mockTest.Setup(x => x.SetPinMask(It.IsAny<List<string>>())).Verifiable();
            this.mockTest.Setup(x => x.ApplyTestConditions()).Verifiable();
            this.mockTest.Setup(x => x.Execute()).Verifiable();
            this.mockTest.Setup(x => x.GetCtvData()).Returns(new Dictionary<string, string>());
            this.mockTest.Setup(x => x.GetPerCycleFailures()).Returns(new List<IFailureData>());
            this.mockTest.Setup(x => x.Reset()).Verifiable();

            this.mockFunctional
                .Setup(x => x.CreateCaptureFailureAndCtvPerPinTest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .Returns(this.mockTest.Object);
            this.mockFunctional
                .Setup(x => x.CreateCaptureFailureTest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .Returns(this.mockTest.Object);
            Prime.Services.FunctionalService = this.mockFunctional.Object;

            this.mockPatConfig.Setup(x => x.Apply(It.IsAny<List<IPatConfigHandle>>())).Verifiable();
            Prime.Services.PatConfigService = this.mockPatConfig.Object;

            var serializedMetadata = new JsonInput(File.ReadAllText(@".\TestInput\Metadata.json"));
            this.deserializedMetadata = serializedMetadata.DeserializeInput<MetadataConfig>();

            var serializedRaster = new JsonInput(File.ReadAllText(@".\TestInput\RasterConfig.json"));
            this.deserializedRaster = serializedRaster.DeserializeInput<RasterConfig>();

            var serializedRasterRC = new JsonInput(this.validInputWithReductionConfig);
            this.deserializedRasterRC = serializedRasterRC.DeserializeInput<RasterConfig>();

            var serializedRasterNP = new JsonInput(File.ReadAllText(@".\TestInput\RasterConfig_NonParallel.json"));
            this.deserializedRasterNP = serializedRasterNP.DeserializeInput<RasterConfig>();
            var mylist = new List<string>() { "0,0,0" };
            var mydictionary = new Dictionary<string, List<string>>()
            {
                { "ic_btb", mylist },
            };
            object fakeStorage = new Dictionary<string, Dictionary<string, List<string>>>()
            {
                { "0", new Dictionary<string, List<string>>(mydictionary) },
            };

            this.mockSharedStorage.Setup(x => x.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>())).Returns(fakeStorage);

            Prime.Services.UserVarService = this.mockUserVar.Object;
            Prime.Services.TestProgramService = this.mockTp.Object;
            Prime.Services.SharedStorageService = this.mockSharedStorage.Object;
        }

        /// <summary>
        /// Ensure the string sent to iCRepair is formatted as expected.
        /// </summary>
        [TestMethod]
        public void ExportDefectsToRepair()
        {
            string gsdsTag = string.Empty;
            /* Mock<IEvergreenService> evergreen = new Mock<IEvergreenService>();
            evergreen.Setup(x => x.SetGsdsUnit(It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string key, string value) => { gsdsTag = value; });

            Prime.Services.EvergreenService = evergreen.Object; */
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("fakeTag", "fake_1,ATOM_1,ATOM_1,0,0,1,0", Context.DUT))
                .Callback((string key, string value, Context context) => { gsdsTag = value; });
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            Dictionary<string, List<IDefect>> exportDefects = new Dictionary<string, List<IDefect>>();
            Dictionary<string, string> failIO = new Dictionary<string, string>()
            {
                { "ATOM_1", "000000001" },
            };
            AtomDefect defect = new AtomDefect(0, 0, "00000001", failIO);
            defect.Array = "fake_1";
            defect.Module = "ATOM_1";
            exportDefects.Add("fake_1", new List<IDefect>() { defect });
            LSARasterTC.ExportDefectsToRepair("fakeTag", exportDefects);
            Assert.IsTrue(gsdsTag == "fake_1,ATOM_1,ATOM_1,0,0,1,0");
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Verify passes and deserialized all configurations properly.
        /// </summary>
        [TestMethod]
        public void Verify_PrescreenCtvMode()
        {
            LSARasterTC verifyTest = new LSARasterTC();
            verifyTest.Patlist = "fake";
            verifyTest.TimingsTc = "fake";
            verifyTest.LevelsTc = "fake";
            verifyTest.HryMapPath = @".\TestInput\HSR_HRY_config_file.xml";
            verifyTest.MetadataConfigPath = @".\TestInput\Metadata_ModuleSupport.json";
            verifyTest.MetadataConfigSchemaPath = @".\TestInput\Metadata_Schema.json";
            verifyTest.PinMappingSetName = "MODULE_EXAMPLE";
            verifyTest.PrescreenPrintMode = LSARasterTC.PrintMode.CTVMODE;
            verifyTest.PrescreenHryFlowToken = "fake";
            verifyTest.PrescreenHryFreqToken = "fake";
            verifyTest.FivrCondition = "fake";

            verifyTest.Verify();
            Assert.IsTrue(true, "Exceptions detected when none were expected.");
        }

        /// <summary>
        /// Verify passes and deserialized all configurations properly.
        /// </summary>
        [TestMethod]
        public void Verify_PrescreenPassMode()
        {
            LSARasterTC verifyTest = new LSARasterTC();
            verifyTest.Patlist = "fake";
            verifyTest.TimingsTc = "fake";
            verifyTest.LevelsTc = "fake";
            verifyTest.HryMapPath = @".\TestInput\HSR_HRY_config_file.xml";
            verifyTest.MetadataConfigPath = @".\TestInput\Metadata_ModuleSupport.json";
            verifyTest.MetadataConfigSchemaPath = @".\TestInput\Metadata_Schema.json";
            verifyTest.PinMappingSetName = "MODULE_EXAMPLE";
            verifyTest.PrescreenHryFlowToken = "fake";
            verifyTest.PrescreenHryFreqToken = "fake";
            verifyTest.FivrCondition = "fake";

            verifyTest.Verify();
            Assert.IsTrue(true, "Exceptions detected when none were expected.");
        }

        /// <summary>
        /// Verify passes and deserialized all configurations properly.
        /// </summary>
        [TestMethod]
        public void Verify_Raster()
        {
            LSARasterTC verifyTest = new LSARasterTC();
            verifyTest.Patlist = "fake";
            verifyTest.TimingsTc = "fake";
            verifyTest.LevelsTc = "fake";
            verifyTest.ExecutionMode = LSARasterTC.TestInstanceMode.RASTER;
            verifyTest.HryMapPath = @".\TestInput\HSR_HRY_config_file.xml";
            verifyTest.MetadataConfigPath = @".\TestInput\Metadata_ModuleSupport.json";
            verifyTest.MetadataConfigSchemaPath = @".\TestInput\Metadata_Schema.json";
            verifyTest.RasterConfigPath = @".\TestInput\RasterConfig_ReductionConfigSet.json";
            verifyTest.RasterConfigSchemaPath = @".\TestInput\RasterConfig_Schema.json";
            verifyTest.PinMappingSetName = "MODULE_EXAMPLE";
            verifyTest.PrescreenHryFlowToken = "fake";
            verifyTest.PrescreenHryFreqToken = "fake";
            verifyTest.FivrCondition = "fake";

            verifyTest.Verify();
            Assert.IsTrue(true, "Exceptions detected when none were expected.");
        }

        /// <summary>
        /// Make sure we throw an exception when raster is not supported for a pinmapping during raster.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void Verify_Raster_RasterModeNotSupported()
        {
            LSARasterTC verifyTest = new LSARasterTC();
            verifyTest.Patlist = "fake";
            verifyTest.TimingsTc = "fake";
            verifyTest.LevelsTc = "fake";
            verifyTest.ExecutionMode = LSARasterTC.TestInstanceMode.RASTER;
            verifyTest.HryMapPath = @".\TestInput\HSR_HRY_config_file.xml";
            verifyTest.MetadataConfigPath = @".\TestInput\Metadata_ModuleSupport.json";
            verifyTest.MetadataConfigSchemaPath = @".\TestInput\Metadata_Schema.json";
            verifyTest.RasterConfigPath = @".\TestInput\RasterConfig_ReductionConfigSet.json";
            verifyTest.RasterConfigSchemaPath = @".\TestInput\RasterConfig_Schema.json";
            verifyTest.PinMappingSetName = "RASTER_NotSupported";
            verifyTest.PrescreenHryFlowToken = "fake";
            verifyTest.PrescreenHryFreqToken = "fake";
            verifyTest.FivrCondition = "fake";

            verifyTest.Verify();
            Assert.IsTrue(false, "No exceptions detected when one was expected.");
        }

        /// <summary>
        /// Verify + Execute Raster test and ensure returns port 1.
        /// </summary>
        [TestMethod]
        public void Execute()
        {
            Mock<IPatConfigHandle> fakeHandle = new Mock<IPatConfigHandle>();
            fakeHandle.Setup(x => x.GetExpectedDataSize())
                .Returns(1);
            fakeHandle.Setup(x => x.SetData(It.IsAny<string>())).Verifiable();
            this.mockPatConfig
                .Setup(x => x.GetPatConfigHandleWithPlist(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fakeHandle.Object);
            Prime.Services.PatConfigService = this.mockPatConfig.Object;

            this.mockTest.Setup(x => x.GetCtvData()).Returns(new Dictionary<string, string>()
            {
                { "YY_TAP_TDO_C2S", "00000000000000110000000000000000110100000000000000000000000001110011010100000000000011010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" },
            });

            this.mockTest.Setup(x => x.GetPerCycleFailures()).Returns(new List<IFailureData>());
            this.mockFunctional
                .Setup(x => x.CreateCaptureFailureAndCtvPerPinTest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .Returns(this.mockTest.Object);
            Prime.Services.FunctionalService = this.mockFunctional.Object;

            LSARasterTC verifyTest = new LSARasterTC();
            verifyTest.Patlist = "arr_atom_m0_raster_doe_list";
            verifyTest.TimingsTc = "fake";
            verifyTest.LevelsTc = "fake";
            verifyTest.ExecutionMode = LSARasterTC.TestInstanceMode.RASTER;
            verifyTest.HryMapPath = @".\TestInput\HSR_HRY_config_file.xml";
            verifyTest.MetadataConfigPath = @".\TestInput\MTL_PrimeMetaDataATOM.json";
            verifyTest.MetadataConfigSchemaPath = @".\TestInput\Metadata_Schema.json";
            verifyTest.RasterConfigPath = @".\TestInput\MTL_RasterConfigATOM.json";
            verifyTest.RasterConfigSchemaPath = @".\TestInput\RasterConfig_Schema.json";
            verifyTest.PinMappingSetName = "ATOM_MOD0";
            verifyTest.PrescreenHryFlowToken = "CORE_NOM";
            verifyTest.PrescreenHryFreqToken = "800MHz";
            verifyTest.FivrCondition = string.Empty;
            verifyTest.Verify();
            Assert.IsTrue(verifyTest.Execute() == 1);
        }

        /// <summary>
        /// Make sure we throw an exception when raster is not supported for a pinmapping during raster.
        /// </summary>
        [TestMethod]
        public void Execute_Prescreen()
        {
            Mock<IPatConfigHandle> fakeHandle = new Mock<IPatConfigHandle>();
            fakeHandle.Setup(x => x.GetExpectedDataSize())
                .Returns(1);
            fakeHandle.Setup(x => x.SetData(It.IsAny<string>())).Verifiable();
            this.mockPatConfig
                .Setup(x => x.GetPatConfigHandleWithPlist(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fakeHandle.Object);
            Prime.Services.PatConfigService = this.mockPatConfig.Object;

            this.mockTest.Setup(x => x.GetCtvData()).Returns(new Dictionary<string, string>()
            {
                { "YY_TAP_TDO_C2S", "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100001000000000000" },
            });

            this.mockTest.Setup(x => x.GetPerCycleFailures()).Returns(new List<IFailureData>());
            this.mockFunctional
                .Setup(x => x.CreateCaptureFailureAndCtvPerPinTest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .Returns(this.mockTest.Object);
            Prime.Services.FunctionalService = this.mockFunctional.Object;

            LSARasterTC verifyTest = new LSARasterTC();
            verifyTest.Patlist = "arr_atom_m0_raster_doe_list";
            verifyTest.TimingsTc = "fake";
            verifyTest.LevelsTc = "fake";
            verifyTest.ExecutionMode = LSARasterTC.TestInstanceMode.PRESCREEN;
            verifyTest.HryMapPath = @".\TestInput\array_pbist_hry_atom_m0R_indirect_lsa_repairable_ic_list.xml";
            verifyTest.MetadataConfigPath = @".\TestInput\MTL_PrimeMetaDataATOM.json";
            verifyTest.MetadataConfigSchemaPath = @".\TestInput\Metadata_Schema.json";
            verifyTest.RasterConfigPath = @".\TestInput\MTL_RasterConfigATOM.json";
            verifyTest.RasterConfigSchemaPath = @".\TestInput\RasterConfig_Schema.json";
            verifyTest.PinMappingSetName = "ATOM_MOD0";
            verifyTest.PrescreenHryFlowToken = "CORE_NOM";
            verifyTest.PrescreenHryFreqToken = "800MHz";
            verifyTest.PrescreenPrintMode = LSARasterTC.PrintMode.CTVMODE;
            verifyTest.FivrCondition = string.Empty;
            verifyTest.Verify();
            Assert.IsNotNull(verifyTest.Execute());
        }

        /// <summary>
        /// Make sure we fail when amble patterns are detected.
        /// </summary>
        [TestMethod]
        public void Execute_PrescreenPASSMODE_AmbleFails()
        {
            Mock<IPatConfigHandle> fakeHandle = new Mock<IPatConfigHandle>();
            fakeHandle.Setup(x => x.GetExpectedDataSize())
                .Returns(1);
            fakeHandle.Setup(x => x.SetData(It.IsAny<string>())).Verifiable();
            this.mockPatConfig
                .Setup(x => x.GetPatConfigHandleWithPlist(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fakeHandle.Object);
            Prime.Services.PatConfigService = this.mockPatConfig.Object;

            this.mockTest.Setup(x => x.GetCtvData()).Returns(new Dictionary<string, string>()
            {
                { "YY_TAP_TDO_C2S", "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100001000000000000" },
            });

            List<IFailureData> fakeCycleFails = new List<IFailureData>();
            FakeFailureData ambleFail = new FakeFailureData("amblePattern", "fake", 299);
            ambleFail.PlistName = "AmblePlist";
            fakeCycleFails.Add(ambleFail);

            this.mockTest.Setup(x => x.GetPerCycleFailures()).Returns(fakeCycleFails);
            this.mockFunctional
                .Setup(x => x.CreateCaptureFailureAndCtvPerPinTest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .Returns(this.mockTest.Object);
            Prime.Services.FunctionalService = this.mockFunctional.Object;

            Mock<IPlistObject> mockPlistObj = new Mock<IPlistObject>();
            mockPlistObj.Setup(x => x.IsPatternAnAmble("amblePattern")).Returns(true);

            Mock<IPlistService> mockPlist = new Mock<IPlistService>();
            mockPlist.Setup(x => x.GetPlistObject("arr_atom_m0_raster_doe_list")).Returns(mockPlistObj.Object);
            Prime.Services.PlistService = mockPlist.Object;

            LSARasterTC verifyTest = new LSARasterTC();
            verifyTest.Patlist = "arr_atom_m0_raster_doe_list";
            verifyTest.TimingsTc = "fake";
            verifyTest.LevelsTc = "fake";
            verifyTest.ExecutionMode = LSARasterTC.TestInstanceMode.PRESCREEN;
            verifyTest.MetadataConfigPath = @".\TestInput\MTL_PrimeMetaDataATOM.json";
            verifyTest.MetadataConfigSchemaPath = @".\TestInput\Metadata_Schema.json";
            verifyTest.RasterConfigPath = @".\TestInput\MTL_RasterConfigATOM.json";
            verifyTest.RasterConfigSchemaPath = @".\TestInput\RasterConfig_Schema.json";
            verifyTest.PinMappingSetName = "ATOM_MOD0";
            verifyTest.PrescreenHryFlowToken = "CORE_NOM";
            verifyTest.PrescreenHryFreqToken = "800MHz";
            verifyTest.PrescreenPrintMode = LSARasterTC.PrintMode.PASSMODE;
            verifyTest.FivrCondition = string.Empty;
            verifyTest.Verify();
            Assert.AreEqual(0, verifyTest.Execute());
        }

        /// <summary>
        /// Make sure we throw an exception when raster is not supported for a pinmapping during raster.
        /// </summary>
        [TestMethod]
        public void Execute_PrescreenCTVMODE_AmbleFails()
        {
            Mock<IPatConfigHandle> fakeHandle = new Mock<IPatConfigHandle>();
            fakeHandle.Setup(x => x.GetExpectedDataSize())
                .Returns(1);
            fakeHandle.Setup(x => x.SetData(It.IsAny<string>())).Verifiable();
            this.mockPatConfig
                .Setup(x => x.GetPatConfigHandleWithPlist(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fakeHandle.Object);
            Prime.Services.PatConfigService = this.mockPatConfig.Object;

            this.mockTest.Setup(x => x.GetCtvData()).Returns(new Dictionary<string, string>()
            {
                { "YY_TAP_TDO_C2S", "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100001000000000000" },
            });

            List<IFailureData> fakeCycleFails = new List<IFailureData>();
            FakeFailureData ambleFail = new FakeFailureData("amblePattern", "fake", 299);
            ambleFail.PlistName = "AmblePlist";
            fakeCycleFails.Add(ambleFail);

            this.mockTest.Setup(x => x.GetPerCycleFailures()).Returns(fakeCycleFails);
            this.mockFunctional
                .Setup(x => x.CreateCaptureFailureAndCtvPerPinTest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .Returns(this.mockTest.Object);
            Prime.Services.FunctionalService = this.mockFunctional.Object;

            Mock<IPlistObject> mockPlistObj = new Mock<IPlistObject>();
            mockPlistObj.Setup(x => x.IsPatternAnAmble("amblePattern")).Returns(true);

            Mock<IPlistService> mockPlist = new Mock<IPlistService>();
            mockPlist.Setup(x => x.GetPlistObject("arr_atom_m0_raster_doe_list")).Returns(mockPlistObj.Object);
            Prime.Services.PlistService = mockPlist.Object;

            LSARasterTC verifyTest = new LSARasterTC();
            verifyTest.Patlist = "arr_atom_m0_raster_doe_list";
            verifyTest.TimingsTc = "fake";
            verifyTest.LevelsTc = "fake";
            verifyTest.ExecutionMode = LSARasterTC.TestInstanceMode.PRESCREEN;
            verifyTest.HryMapPath = @".\TestInput\array_pbist_hry_atom_m0R_indirect_lsa_repairable_ic_list.xml";
            verifyTest.MetadataConfigPath = @".\TestInput\MTL_PrimeMetaDataATOM.json";
            verifyTest.MetadataConfigSchemaPath = @".\TestInput\Metadata_Schema.json";
            verifyTest.RasterConfigPath = @".\TestInput\MTL_RasterConfigATOM.json";
            verifyTest.RasterConfigSchemaPath = @".\TestInput\RasterConfig_Schema.json";
            verifyTest.PinMappingSetName = "ATOM_MOD0";
            verifyTest.PrescreenHryFlowToken = "CORE_NOM";
            verifyTest.PrescreenHryFreqToken = "800MHz";
            verifyTest.PrescreenPrintMode = LSARasterTC.PrintMode.CTVMODE;
            verifyTest.FivrCondition = string.Empty;
            verifyTest.Verify();
            Assert.AreEqual(0, verifyTest.Execute());
        }

        /// <summary>
        /// Make sure we throw an exception when raster is not supported for a pinmapping during raster.
        /// </summary>
        [TestMethod]
        public void Execute_Prescreen2()
        {
            Mock<IPatConfigHandle> fakeHandle = new Mock<IPatConfigHandle>();
            fakeHandle.Setup(x => x.GetExpectedDataSize())
                .Returns(1);
            fakeHandle.Setup(x => x.SetData(It.IsAny<string>())).Verifiable();
            this.mockPatConfig
                .Setup(x => x.GetPatConfigHandleWithPlist(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fakeHandle.Object);
            Prime.Services.PatConfigService = this.mockPatConfig.Object;

            this.mockTest.Setup(x => x.GetCtvData()).Returns(new Dictionary<string, string>()
            {
                { "YY_TAP_TDO_C2S", "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100001000000000000" },
            });

            this.mockTest.Setup(x => x.GetPerCycleFailures()).Returns(new List<IFailureData>());
            this.mockFunctional
                .Setup(x => x.CreateCaptureFailureAndCtvPerPinTest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .Returns(this.mockTest.Object);
            Prime.Services.FunctionalService = this.mockFunctional.Object;

            LSARasterTC verifyTest = new LSARasterTC();
            verifyTest.Patlist = "arr_atom_m0_raster_doe_list";
            verifyTest.TimingsTc = "fake";
            verifyTest.LevelsTc = "fake";
            verifyTest.ExecutionMode = LSARasterTC.TestInstanceMode.PRESCREEN;
            verifyTest.HryMapPath = @".\TestInput\array_pbist_hry_atom_m0R_indirect_lsa_repairable_ic_list.xml";
            verifyTest.MetadataConfigPath = @".\TestInput\MTL_PrimeMetaDataATOM.json";
            verifyTest.MetadataConfigSchemaPath = @".\TestInput\Metadata_Schema.json";
            verifyTest.RasterConfigPath = @".\TestInput\MTL_RasterConfigATOM.json";
            verifyTest.RasterConfigSchemaPath = @".\TestInput\RasterConfig_Schema.json";
            verifyTest.PinMappingSetName = "ATOM_MOD0";
            verifyTest.PrescreenHryFlowToken = "CORE_NOM";
            verifyTest.PrescreenHryFreqToken = "800MHz";
            verifyTest.FivrCondition = string.Empty;
            verifyTest.Verify();
            Assert.IsNotNull(verifyTest.Execute());
        }
    }
}
