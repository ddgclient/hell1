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
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using global::LSARasterTC;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.PatternService;
    using Prime.PlistService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;
    using Prime.UserVarService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class PrescreenTest_UnitTest
    {
        private Mock<IConsoleService> mockConsole = new Mock<IConsoleService>();
        private Mock<IDatalogService> mockDatalog = new Mock<IDatalogService>();
        private Mock<IStrgvalFormat> mockStrgval = new Mock<IStrgvalFormat>();
        private Mock<IMrsltFormat> mockMrslt = new Mock<IMrsltFormat>();
        private Mock<IFileService> mockFileservice = new Mock<IFileService>();
        private Mock<ISharedStorageService> mockSharedstorage = new Mock<ISharedStorageService>();
        private Mock<IUserVarService> mockUserVar = new Mock<IUserVarService>();
        private Mock<ITestProgramService> mockTpService = new Mock<ITestProgramService>();

        private string validStringMetadata = "{ \"Setup\":{ \"PatModConfigSets\":{ \"LSA_RASTER_PAT_MODIFY_SET_CORE\":[\"MaxDefectsCount\",\"Multiport\",\"Dword\",\"IOMask\"], \"LSA_RASTER_PAT_MODIFY_SET_CBO\":[\"MaxDefectsCount\",\"Multiport\",\"Bank\",\"Dword\",\"IOMask\"], \"LSA_RASTER_PAT_MODIFY_SET_SA\":[\"MaxDefectsCount\",\"Multiport\",\"Bank\",\"Dword\",\"IOMask\"] }, \"CaptureConfigSets\":{ \"LSA_RASTER_CAPTURE_DECODING_SET_CORE\":{ \"Length\":82, \"DecodingElements\":{ \"FailAddress\":{ \"Start\":30, \"End\":41 }, \"Dword\":{ \"Start\":42, \"End\":45 }, \"Bank\":{ \"Start\":46, \"End\":49 }, \"FailIO\":{ \"Start\":50, \"End\":81 } } }, \"LSA_RASTER_CAPTURE_DECODING_SET_CBO\":{ \"Length\":52, \"DecodingElements\":{ \"FailAddress\":{ \"Start\":0, \"End\":11 }, \"Dword\":{ \"Start\":12, \"End\":15 }, \"Bank\":{ \"Start\":16, \"End\":19 }, \"FailIO\": { \"Start\":20, \"End\":51 } } }, \"LSA_RASTER_CAPTURE_DECODING_SET_SA\":{ \"Length\":52, \"DecodingElements\":{ \"FailAddress\":{ \"Start\":0, \"End\":11 }, \"Dword\":{ \"Start\":12, \"End\":15 }, \"Bank\":{ \"Start\":16, \"End\":19 }, \"FailIO\":{ \"Start\":20, \"End\":51 } } } }, \"SlicePinMapping\":{ \"4_CORES\":{ \"MulticorePatternEnabled\":false, \"Configurations\":{ \"ArrayNameRegex\":\"[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_([a-zA-Z0-9]_[a-zA-Z0-9]_?[a-zA-Z0-9]*)_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]\", \"IsRasterModeSupported\":true, \"IsGetDwordFromFailIoIndex\":false, \"PreScreenLabelRegex\":\"_MBD_([0-9])_([0-9])_([0-9])\", \"LabelRegExTokens\":[ \"MULTIPORT\", \"BANK\", \"DWORD\" ] }, \"PinMappings\":[ { \"SliceId\":\"0\", \"PinName\":\"NOAB_00\", \"HryName\":\"C0\" }, { \"SliceId\":\"1\", \"PinName\":\"NOAB_01\", \"HryName\":\"C1\" }, { \"SliceId\":\"2\", \"PinName\":\"NOAB_02\", \"HryName\":\"C2\" }, { \"SliceId\":\"3\", \"PinName\":\"NOAB_03\", \"HryName\":\"C3\" } ] }, \"CCF_SLICE_0\":{ \"MulticorePatternEnabled\":false, \"Configurations\":{ \"ArrayNameRegex\":\"[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_([a-zA-Z0-9]_[a-zA-Z0-9]_?[a-zA-Z0-9]*)_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]\", \"IsRasterModeSupported\":true, \"IsGetDwordFromFailIoIndex\":false, \"PreScreenLabelRegex\":\"_S?MBD_?X?_([0-7])_([0-9])_([0-9])_([0-9])\", \"LabelRegExTokens\":[ \"SLICE\", \"MULTIPORT\", \"BANK\", \"DWORD\" ] }, \"PinMappings\":[ { \"SliceId\":\"0\", \"PinName\":\"TDO\", \"HryName\":\"C0\" } ] }, \"CCF_SLICE_1\":{ \"MulticorePatternEnabled\":false, \"Configurations\":{ \"ArrayNameRegex\":\"[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_([a-zA-Z0-9]_[a-zA-Z0-9]_?[a-zA-Z0-9]*)_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]\", \"IsRasterModeSupported\":true, \"IsGetDwordFromFailIoIndex\":false, \"PreScreenLabelRegex\":\"_S?MBD_?X?_([0-7])_([0-9])_([0-9])_([0-9])\", \"LabelRegExTokens\":[ \"SLICE\", \"MULTIPORT\", \"BANK\", \"DWORD\" ] }, \"PinMappings\":[ { \"SliceId\":\"1\", \"PinName\":\"TDO\", \"HryName\":\"C1\" } ] }, \"CCF_SLICE_2\":{ \"MulticorePatternEnabled\":false, \"Configurations\":{ \"ArrayNameRegex\":\"[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_([a-zA-Z0-9]_[a-zA-Z0-9]_?[a-zA-Z0-9]*)_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]\", \"IsRasterModeSupported\":true, \"IsGetDwordFromFailIoIndex\":false, \"PreScreenLabelRegex\":\"_S?MBD_?X?_([0-7])_([0-9])_([0-9])_([0-9])\", \"LabelRegExTokens\":[ \"SLICE\", \"MULTIPORT\", \"BANK\", \"DWORD\" ] }, \"PinMappings\":[ { \"SliceId\":\"2\", \"PinName\":\"TDO\", \"HryName\":\"C2\" } ] }, \"CCF_SLICE_3\":{ \"MulticorePatternEnabled\":false, \"Configurations\":{ \"ArrayNameRegex\":\"[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_([a-zA-Z0-9]_[a-zA-Z0-9]_?[a-zA-Z0-9]*)_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]\", \"IsRasterModeSupported\":true, \"IsGetDwordFromFailIoIndex\":false, \"PreScreenLabelRegex\":\"_S?MBD_?X?_([0-7])_([0-9])_([0-9])_([0-9])\", \"LabelRegExTokens\":[ \"SLICE\", \"MULTIPORT\", \"BANK\", \"DWORD\" ] }, \"PinMappings\":[ { \"SliceId\":\"3\", \"PinName\":\"TDO\", \"HryName\":\"C3\" } ] }, \"4_SLICES_CBO\":{ \"MulticorePatternEnabled\":false, \"Configurations\":{ \"ArrayNameRegex\":\"[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_([a-zA-Z0-9]_[a-zA-Z0-9])_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]_[a-zA-Z0-9]\", \"IsRasterModeSupported\":true, \"IsGetDwordFromFailIoIndex\":false, \"PreScreenLabelRegex\":\"_S?MBD_?X?_([0-7])_([0-9])_([0-9])_([0-9])\", \"LabelRegExTokens\":[ \"SLICE\", \"MULTIPORT\", \"BANK\", \"DWORD\" ] }, \"PinMappings\":[ { \"SliceId\":\"0\", \"PinName\":\"NOAB_00\", \"HryName\":\"C0\" }, { \"SliceId\":\"1\", \"PinName\":\"NOAB_01\", \"HryName\":\"C1\" }, { \"SliceId\":\"2\", \"PinName\":\"NOAB_02\", \"HryName\":\"C2\" }, { \"SliceId\":\"3\", \"PinName\":\"NOAB_03\", \"HryName\":\"C3\" } ] } } } }";

        private MetadataConfig deserializedMetadata;

        /// <summary>
        /// Dummy summary.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            // Mocking all Prime services needed for testing
            this.mockConsole.Setup(x => x.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, int, string, string>((string msg, int line, string n, string src) => { Console.WriteLine($"ERROR: {msg}"); });
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

            this.mockFileservice.Setup(x => x.FileExists("Nonexistent")).Returns(false);
            this.mockFileservice.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
            Prime.Services.FileService = this.mockFileservice.Object;

            this.mockSharedstorage.Setup(x => x.InsertRowAtTable(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<Context>())).Verifiable();
            Prime.Services.SharedStorageService = this.mockSharedstorage.Object;

            this.mockUserVar.Setup(x => x.GetAllNamesFromCollection(It.IsAny<string>())).Throws(new Exception());
            Prime.Services.UserVarService = this.mockUserVar.Object;

            this.mockTpService.Setup(x => x.IsClassTestSocket()).Returns(true);
            Prime.Services.TestProgramService = this.mockTpService.Object;

            // Initializing common objects needed for prescreen testing
            var inputHandler = InputFactory.CreateConfigHandler(this.validStringMetadata, InputFactory.FileType.JSON);
            this.deserializedMetadata = inputHandler.DeserializeInput<MetadataConfig>();
        }

        /// <summary>
        /// Verify we can extract MBD info from label.
        /// </summary>
        [TestMethod]
        public void ExtractValuesFromLabel_OnlyMBD() // FIXME: BAD TEST, labelregextokens determines if we get slice from label or not
        {
            var pinMappingSet = this.deserializedMetadata.GetPinMappingSet("4_CORES");
            pinMappingSet.ValidateAndSetupItems();

            PrescreenTest test = new PrescreenTest(pinMappingSet, null, null, null, null, LSARasterTC.PrintMode.FAILMODE, "fake");

            BigCoreArray fail = new BigCoreArray("fakeArray");
            string label = "EPBIST_FAIL_MBD_0_0_0_1";

            Mock<ILabel> labelObject = new Mock<ILabel>();
            labelObject.Setup(x => x.GetName()).Returns(label);
            labelObject.Setup(x => x.GetAddress()).Returns(243);

            fail.ExtractValuesFromLabel(labelObject.Object, pinMappingSet);

            var mbdValues = fail.MBDAddress;

            Assert.IsTrue(mbdValues.Item1 == 0 &&
                mbdValues.Item2 == 0 &&
                mbdValues.Item3 == 0 &&
                fail.SliceId.Count == 0);
        }

        /// <summary>
        /// Verify we can extract slice Id from label.
        /// </summary>
        [TestMethod]
        public void ExtractValuesFromLabel_SliceId()
        {
            string labelRegex = "_S?MBD_?X?_([0-7]+)_([0-9]+)_([0-9]+)_([0-9]+)";

            var serializedInput = new JsonInput(this.validStringMetadata);
            var metadata = serializedInput.DeserializeInput<MetadataConfig>();
            var pinMappingSet = this.deserializedMetadata.GetPinMappingSet("4_SLICES_CBO");
            pinMappingSet.ValidateAndSetupItems();
            pinMappingSet.Configurations.PrescreenLabelRegex = labelRegex;

            PrescreenTest test = new PrescreenTest(pinMappingSet, null, null, null, null, LSARasterTC.PrintMode.FAILMODE, "fake");

            BigCoreArray fail = new BigCoreArray("fake");
            string label = "EPBIST_FAIL_SMBD_X_7_0_0_1";
            Mock<ILabel> labelObject = new Mock<ILabel>();
            labelObject.Setup(x => x.GetName()).Returns(label);
            labelObject.Setup(x => x.GetAddress()).Returns(243);

            Regex prescreenLabelRegex = new Regex(labelRegex);
            fail.ExtractValuesFromLabel(labelObject.Object, pinMappingSet);

            var mbdValues = fail.MBDAddress;

            Assert.IsTrue(mbdValues.Item1 == 0 &&
                mbdValues.Item2 == 0 &&
                mbdValues.Item3 == 1 &&
                fail.SliceId.Contains("7"));
        }

        /// <summary>
        /// Verify that unit exits with port 1 when no fails are detected.
        /// </summary>
        [TestMethod]
        public void Execute_NoFailures_FailMode()
        {
            var prescreen = new PrescreenTest(new MetadataConfig.PinMappingSet(), "Fake", "Fake", 0, "Fake", LSARasterTC.PrintMode.CTVMODE, "fake");
            prescreen.Execute(true, null);
            Assert.IsTrue(prescreen.ExitPort == 1);
        }

        /// <summary>
        /// Verify that function returns port 1 and logs ctvData when in CTV_MODE.
        /// </summary>
        [TestMethod]
        public void Execute_NoFailures_CTVMode()
        {
            var prescreen = new PrescreenTest(new MetadataConfig.PinMappingSet(), "Fake", "Fake", 0, "Fake", LSARasterTC.PrintMode.CTVMODE, "fake");
            prescreen.Execute(true, null);
            Assert.IsTrue(prescreen.ExitPort == 1);
        }

        /// <summary>
        /// Verify that function captures and logs failure data when fails are detected; exits out of port 3.
        /// </summary>
        [TestMethod]
        public void Execute_Failures_FailMode()
        {
            string domain = "Fake domain";
            string patternName = "0_0_0_0_0_0_0_1_1_1_0_0_0_0";
            uint address = 100;
            string label = "EPBIST_FAIL_MBD_0_0_1";
            List<string> pinNames = new List<string>() { "NOAB_00" };

            Mock<IFailureData> failMock = new Mock<IFailureData>();

            failMock.Setup(x => x.GetDomainName()).Returns(domain);
            failMock.Setup(x => x.GetVectorAddress()).Returns(address);
            failMock.Setup(x => x.GetPatternName()).Returns(patternName);
            failMock.Setup(x => x.GetFailingPinNames()).Returns(pinNames);

            var failData = new List<IFailureData>() { failMock.Object };

            Mock<IPatternService> psMock = new Mock<IPatternService>();
            Mock<ILabel> labelMock = new Mock<ILabel>();
            labelMock.Setup(x => x.GetName()).Returns(label);
            psMock.Setup(x => x.GetLabelFromAddress(patternName, domain, address, false)).Returns(labelMock.Object);

            Prime.Services.PatternService = psMock.Object;

            var pinMappingSet = this.deserializedMetadata.GetPinMappingSet("4_CORES");
            pinMappingSet.ValidateAndSetupItems();

            var prescreen = new PrescreenTest(pinMappingSet, "FAKE", "FAKE", 0, "key", LSARasterTC.PrintMode.CTVMODE, "fake");
            prescreen.Execute(false, failData);

            prescreen.PrintInternalDbToConsole();
            Assert.IsTrue(prescreen.ExitPort == 3);
        }

        /// <summary>
        /// Verify that function captures and logs more than one failing array in multiple different slices when DFM failures are detected.
        /// </summary>
        [TestMethod]
        public void Execute_Failures_MultipleFailInfo()
        {
            string domain = "Fake domain";
            string patternName = "0_0_0_0_0_0_0_1_1_1_0_0_0_0";
            uint address = 100;
            string label = "EPBIST_FAIL_MBD_7_0_0_1";

            List<string> pinNames = new List<string>() { "NOAB_00", "NOAB_01" };

            Mock<IFailureData> failMock = new Mock<IFailureData>();

            failMock.Setup(x => x.GetDomainName()).Returns(domain);
            failMock.Setup(x => x.GetVectorAddress()).Returns(address);
            failMock.Setup(x => x.GetPatternName()).Returns(patternName);
            failMock.Setup(x => x.GetFailingPinNames()).Returns(pinNames);

            var failData = new List<IFailureData>() { failMock.Object, failMock.Object, failMock.Object };

            Mock<IPatternService> psMock = new Mock<IPatternService>();
            Mock<ILabel> labelMock = new Mock<ILabel>();
            labelMock.Setup(x => x.GetName()).Returns(label);
            psMock.Setup(x => x.GetLabelFromAddress(patternName, domain, address, false)).Returns(labelMock.Object);

            Prime.Services.PatternService = psMock.Object;

            var pinMappingSet = this.deserializedMetadata.GetPinMappingSet("4_SLICES_CBO");
            pinMappingSet.ValidateAndSetupItems();

            var prescreen = new PrescreenTest(pinMappingSet, "FAKE", "FAKE", 0, "key", LSARasterTC.PrintMode.CTVMODE, "fake");
            prescreen.Execute(false, failData);

            prescreen.PrintInternalDbToConsole();
            Assert.IsTrue(prescreen.ExitPort == 3);
        }

        /// <summary>
        /// Method for testing if we can map based on label.
        /// </summary>
        [TestMethod]
        public void Execute_Failures_MapFromLabel()
        {
            string domain = "Fake domain";
            string patternName = "0_0_0_0_0_0_0_1_1_1_0_0_0_0";
            uint address = 100;
            string label = "EPBIST_FAIL_SMBD_X_2_0_0_1";

            List<string> pinNames = new List<string>() { "NOAB_00", "NOAB_01" };

            Mock<IFailureData> failMock = new Mock<IFailureData>();

            failMock.Setup(x => x.GetDomainName()).Returns(domain);
            failMock.Setup(x => x.GetVectorAddress()).Returns(address);
            failMock.Setup(x => x.GetPatternName()).Returns(patternName);
            failMock.Setup(x => x.GetFailingPinNames()).Returns(pinNames);

            var failData = new List<IFailureData>() { failMock.Object };

            Mock<IPatternService> psMock = new Mock<IPatternService>();
            Mock<ILabel> labelMock = new Mock<ILabel>();
            labelMock.Setup(x => x.GetName()).Returns(label);
            psMock.Setup(x => x.GetLabelFromAddress(patternName, domain, address, false)).Returns(labelMock.Object);

            Prime.Services.PatternService = psMock.Object;

            var pinMappingSet = this.deserializedMetadata.GetPinMappingSet("4_SLICES_CBO");
            pinMappingSet.ValidateAndSetupItems();

            var prescreen = new PrescreenTest(pinMappingSet, "FAKE", "FAKE", 0, "key", LSARasterTC.PrintMode.FAILMODE, "fake");
            prescreen.Execute(false, failData);

            prescreen.PrintInternalDbToConsole();
            var failInfo = prescreen.ReturnInternalDB();
            Assert.IsTrue(failInfo["2"]["1_1"].Count == 1);
        }

        /// <summary>
        /// Ensure we can attach module info to fail array info.
        /// </summary>
        [TestMethod]
        public void Execute_Failures_ModuleInfo()
        {
            string domain = "Fake domain";
            string patternName = "0_0_0_0_0_0_0_1_1_1_0_0_0_0";
            uint address = 100;
            string label = "EPBIST_FAIL_SMBD_X_7_0_0_1";

            List<string> pinNames = new List<string>() { "NOAB_00", "NOAB_01" };

            Mock<IFailureData> failMock = new Mock<IFailureData>();

            failMock.Setup(x => x.GetDomainName()).Returns(domain);
            failMock.Setup(x => x.GetVectorAddress()).Returns(address);
            failMock.Setup(x => x.GetPatternName()).Returns(patternName);
            failMock.Setup(x => x.GetFailingPinNames()).Returns(pinNames);

            var failData = new List<IFailureData>() { failMock.Object };

            Mock<IPatternService> psMock = new Mock<IPatternService>();
            Mock<ILabel> labelMock = new Mock<ILabel>();
            labelMock.Setup(x => x.GetName()).Returns(label);
            psMock.Setup(x => x.GetLabelFromAddress(patternName, domain, address, false)).Returns(labelMock.Object);

            Prime.Services.PatternService = psMock.Object;

            string moduleMetadata = "{ \"Setup\":{ \"PatModConfigSets\":{ \"LSA_RASTER_PAT_MODIFY_SET_CORE\":[\"MaxDefectsCount\",\"Multiport\",\"Dword\",\"IOMask\"], \"LSA_RASTER_PAT_MODIFY_SET_CBO\":[\"MaxDefectsCount\",\"Multiport\",\"Bank\",\"Dword\",\"IOMask\"], \"LSA_RASTER_PAT_MODIFY_SET_SA\":[\"MaxDefectsCount\",\"Multiport\",\"Bank\",\"Dword\",\"IOMask\"] }, \"CaptureConfigSets\":{ \"LSA_RASTER_CAPTURE_DECODING_SET_CORE\":{ \"Length\":82, \"DecodingElements\":{ \"FailAddress\":{ \"Start\":30, \"End\":41 }, \"Dword\":{ \"Start\":42, \"End\":45 }, \"Bank\":{ \"Start\":46, \"End\":49 }, \"FailIO\":{ \"Start\":50, \"End\":81 } } }, \"LSA_RASTER_CAPTURE_DECODING_SET_CBO\":{ \"Length\":52, \"DecodingElements\":{ \"FailAddress\":{ \"Start\":0, \"End\":11 }, \"Dword\":{ \"Start\":12, \"End\":15 }, \"Bank\":{ \"Start\":16, \"End\":19 }, \"FailIO\":{ \"Start\":20, \"End\":51 } } }, \"LSA_RASTER_CAPTURE_DECODING_SET_SA\":{ \"Length\":52, \"DecodingElements\":{ \"FailAddress\":{ \"Start\":0, \"End\":11 }, \"Dword\":{ \"Start\":12, \"End\":15 }, \"Bank\":{ \"Start\":16, \"End\":19 }, \"FailIO\":{ \"Start\":20, \"End\":51 } } } }, \"SlicePinMapping\":{ \"MODULE_EXAMPLE\":{ \"MulticorePatternEnabled\":false, \"Configurations\":{ \"ArrayNameRegex\":\"[a-zA-Z0-9]+_[a-zA-Z0-9]+_[a-zA-Z0-9]+_[a-zA-Z0-9]+_[a-zA-Z0-9]+_[a-zA-Z0-9]+_[a-zA-Z0-9]+_([a-zA-Z0-9]+_[a-zA-Z0-9]+_?[a-zA-Z0-9]*)_[a-zA-Z0-9]+_[a-zA-Z0-9]+_[a-zA-Z0-9]+_[a-zA-Z0-9]+\", \"IsRasterModeSupported\":true, \"IsGetDwordFromFailIoIndex\":false, \"PreScreenLabelRegex\":\"_S?MBD_?X?_([0-7]+)_([0-9]+)_([0-9]+)_([0-9]+)\", \"LabelRegExTokens\":[ \"SLICE\", \"MULTIPORT\", \"BANK\", \"DWORD\" ] }, \"PinMappings\":[ { \"Module\":\"0\", \"PinName\":\"NOAB_00\", \"HryName\":\"C0\" }, { \"Module\":\"1\", \"PinName\":\"NOAB_01\", \"HryName\":\"C1\" }, { \"Module\":\"2\", \"PinName\":\"NOAB_02\", \"HryName\":\"C2\" }, { \"Module\":\"3\", \"PinName\":\"NOAB_03\", \"HryName\":\"C3\" } ] } } } }";

            var serializedInput = new JsonInput(moduleMetadata);
            var deserializedInput = serializedInput.DeserializeInput<MetadataConfig>();
            var pinMappingSet = deserializedInput.GetPinMappingSet("MODULE_EXAMPLE");
            pinMappingSet.ValidateAndSetupItems();

            var prescreen = new PrescreenTest(pinMappingSet, "FAKE", "FAKE", 0, "key", LSARasterTC.PrintMode.CTVMODE, "fake");
            prescreen.Execute(false, failData);
            prescreen.PrintInternalDbToConsole();
            var database = prescreen.ReturnInternalDB();
            Assert.IsTrue(database.ContainsKey("0"));
        }

        /// <summary>
        /// Verify that function captures and logs failure data when fails are detected; exits out of port 3.
        /// </summary>
        [TestMethod]
        public void Execute_Failures_CTVMode()
        {
            string domain = "Fake domain";
            string patternName = "0_0_0_0_0_0_0_1_1_1_0_0_0_0";
            uint address = 100;
            string label = "FAIL_MBD_0_0_1";
            List<string> pinNames = new List<string>() { "NOAB_00" };

            Mock<IFailureData> failMock = new Mock<IFailureData>();

            failMock.Setup(x => x.GetDomainName()).Returns(domain);
            failMock.Setup(x => x.GetVectorAddress()).Returns(address);
            failMock.Setup(x => x.GetPatternName()).Returns(patternName);
            failMock.Setup(x => x.GetFailingPinNames()).Returns(pinNames);

            var failData = new List<IFailureData>() { failMock.Object };

            Mock<IPatternService> psMock = new Mock<IPatternService>();
            Mock<ILabel> labelMock = new Mock<ILabel>();
            labelMock.Setup(x => x.GetName()).Returns(label);
            psMock.Setup(x => x.GetLabelFromAddress(patternName, domain, address, false)).Returns(labelMock.Object);

            Prime.Services.PatternService = psMock.Object;

            string validHryTableInputXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?> <HSR_HRY_config>  <ReverseCtvCaptureData>true</ReverseCtvCaptureData>  <CtvToHryMapping> 	<Map ctv_data=\"0\" hry_data=\"0\" /> 	<Map ctv_data=\"1\" hry_data=\"1\" /> </CtvToHryMapping>  <Criterias> 	<Criteria hry_index=\"0\"  pin=\"P001\" ctv_index_range=\"2\"  condition=\"P002,0-1,00|P002,3,1\"    hry_output_on_condition_fail=\"8\" bypass_global=\"HSR.HRY_Global_1\" /> 	<Criteria hry_index=\"1\"  pin=\"P001\" ctv_index_range=\"6\"  condition=\"P002,4-5,00|P002,7,1\"    hry_output_on_condition_fail=\"8\" bypass_global=\"HSR.HRY_Global_1\" /> </Criterias>  <Algorithms> 	<Algorithm index=\"0\" name=\"SCAN\"    pat_modify_label=\"\" ctv_size=\"36\" /> 	<Algorithm index=\"1\" name=\"PMOVI\"   pat_modify_label=\"\" ctv_size=\"36\" /> 	<Algorithm index=\"2\" name=\"March-C\" pat_modify_label=\"\" ctv_size=\"36\" /> </Algorithms>  </HSR_HRY_config>";
            var input = InputFactory.CreateConfigHandler(validHryTableInputXML, InputFactory.FileType.XML);
            var deserializedHryTable = input.DeserializeInput<HryTableConfigXml>();

            // Output of HRY string should all be 1's given this data
            Dictionary<string, string> ctvData = new Dictionary<string, string>()
            {
                { "P001", "001100110000000000000000000000000000" },
                { "P002", "00X100X10000000000000000000000000000" },
                { "NOA1", "011100000000000000000000000000000000" },
            };

            var pinMappingSet = this.deserializedMetadata.GetPinMappingSet("4_CORES");
            pinMappingSet.ValidateAndSetupItems();

            var prescreen = new PrescreenTest(pinMappingSet, "FAKE", "FAKE", 0, "key", LSARasterTC.PrintMode.CTVMODE, "fake");
            prescreen.Execute(false, failData, ctvData, deserializedHryTable);

            prescreen.PrintInternalDbToConsole();
            Assert.IsTrue(prescreen.ExitPort == 3);
        }

        /// <summary>
        /// Ensure we end on a port 2 if we detect a failure on a complete label.
        /// </summary>
        [TestMethod]
        public void Execute_Failures_CompleteLabel()
        {
            string domain = "Fake domain";
            string patternName = "0_0_0_0_0_0_0_1_1_1_0_0_0_0";
            uint address = 100;
            string label = "Complete_MBD_0_0_1";
            List<string> pinNames = new List<string>() { "NOAB_00" };

            Mock<IFailureData> failMock = new Mock<IFailureData>();

            failMock.Setup(x => x.GetDomainName()).Returns(domain);
            failMock.Setup(x => x.GetVectorAddress()).Returns(address);
            failMock.Setup(x => x.GetPatternName()).Returns(patternName);
            failMock.Setup(x => x.GetFailingPinNames()).Returns(pinNames);

            var failData = new List<IFailureData>() { failMock.Object };

            Mock<ILabel> labelMock = new Mock<ILabel>();
            labelMock.Setup(x => x.GetName()).Returns(label);
            labelMock.Setup(x => x.GetAddress()).Returns(address);

            Mock<IPatternService> psMock = new Mock<IPatternService>();
            psMock.Setup(x => x.GetLabelFromAddress(patternName, domain, address, false)).Returns(labelMock.Object);

            Prime.Services.PatternService = psMock.Object;

            var pinMappingSet = this.deserializedMetadata.GetPinMappingSet("4_CORES");
            pinMappingSet.ValidateAndSetupItems();

            var prescreen = new PrescreenTest(pinMappingSet, "FAKE", "FAKE", 0, "key", LSARasterTC.PrintMode.CTVMODE, "fake");
            prescreen.Execute(false, failData);

            Assert.IsTrue(prescreen.ExitPort == 2);
        }

        /// <summary>
        /// Ensure we end on a port 0 if we detect a failures during preamble.
        /// </summary>
        [TestMethod]
        public void Execute_Failures_Preamble()
        {
            string domain = "Fake domain";
            string patternName = "preamblePattern";
            uint address = 100;
            string label = "Complete_MBD_0_0_1";
            List<string> pinNames = new List<string>() { "NOAB_00" };

            Mock<IFailureData> failMock = new Mock<IFailureData>();

            failMock.Setup(x => x.GetDomainName()).Returns(domain);
            failMock.Setup(x => x.GetVectorAddress()).Returns(address);
            failMock.Setup(x => x.GetPatternName()).Returns(patternName);
            failMock.Setup(x => x.GetFailingPinNames()).Returns(pinNames);

            var failData = new List<IFailureData>() { failMock.Object };

            Mock<ILabel> labelMock = new Mock<ILabel>();
            labelMock.Setup(x => x.GetName()).Returns(label);
            labelMock.Setup(x => x.GetAddress()).Returns(address);

            Mock<IPatternService> psMock = new Mock<IPatternService>();
            psMock.Setup(x => x.GetLabelFromAddress(patternName, domain, address, false)).Returns(labelMock.Object);

            Prime.Services.PatternService = psMock.Object;

            Mock<IPlistObject> mockPlistObj = new Mock<IPlistObject>();
            mockPlistObj.Setup(x => x.IsPatternAnAmble("preamblePattern")).Returns(true);
            Mock<IPlistService> mockPlistService = new Mock<IPlistService>();
            mockPlistService.Setup(x => x.GetPlistObject(It.IsAny<string>())).Returns(mockPlistObj.Object);

            Prime.Services.PlistService = mockPlistService.Object;

            var pinMappingSet = this.deserializedMetadata.GetPinMappingSet("4_CORES");
            pinMappingSet.ValidateAndSetupItems();

            var prescreen = new PrescreenTest(pinMappingSet, "FAKE", "FAKE", 0, "key", LSARasterTC.PrintMode.CTVMODE, "fake");
            prescreen.Execute(false, failData);

            Assert.IsTrue(prescreen.ExitPort == 0);
        }

        /// <summary>
        /// Ensure we end on a port 0 if we detect a failures during preamble and TOS/Prime fails to detect as a preamble.
        /// </summary>
        [TestMethod]
        public void Execute_Failures_Preamble_TOSReturnsFalse()
        {
            string domain = "Fake domain";
            string patternName = "preamblePattern";
            uint address = 100;
            string label = "Complete_MBD_0_0_1";
            string parentPlist = "resetplb_fake";
            List<string> pinNames = new List<string>() { "NOAB_00" };

            Mock<IFailureData> failMock = new Mock<IFailureData>();

            failMock.Setup(x => x.GetDomainName()).Returns(domain);
            failMock.Setup(x => x.GetVectorAddress()).Returns(address);
            failMock.Setup(x => x.GetPatternName()).Returns(patternName);
            failMock.Setup(x => x.GetFailingPinNames()).Returns(pinNames);
            failMock.Setup(x => x.GetParentPlistName()).Returns(parentPlist);

            var failData = new List<IFailureData>() { failMock.Object };

            Mock<ILabel> labelMock = new Mock<ILabel>();
            labelMock.Setup(x => x.GetName()).Returns(label);
            labelMock.Setup(x => x.GetAddress()).Returns(address);

            Mock<IPatternService> psMock = new Mock<IPatternService>();
            psMock.Setup(x => x.GetLabelFromAddress(patternName, domain, address, false)).Returns(labelMock.Object);

            Prime.Services.PatternService = psMock.Object;

            Mock<IPlistObject> mockPlistObj = new Mock<IPlistObject>();
            mockPlistObj.Setup(x => x.IsPatternAnAmble("preamblePattern")).Returns(false);
            Mock<IPlistService> mockPlistService = new Mock<IPlistService>();
            mockPlistService.Setup(x => x.GetPlistObject(It.IsAny<string>())).Returns(mockPlistObj.Object);

            Prime.Services.PlistService = mockPlistService.Object;

            var pinMappingSet = this.deserializedMetadata.GetPinMappingSet("4_CORES");
            pinMappingSet.ValidateAndSetupItems();

            var prescreen = new PrescreenTest(pinMappingSet, "FAKE", "FAKE", 0, "key", LSARasterTC.PrintMode.CTVMODE, "fake");
            prescreen.Execute(false, failData);

            Assert.IsTrue(prescreen.ExitPort == 0);
        }

        /// <summary>
        /// If a failure occurs on a pattern that contains no name, and is not a preamble, error out.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void Execute_Failures_NoArrayInPattern()
        {
            string domain = "Fake domain";
            string patternName = "preamblePattern";
            uint address = 100;
            string label = "Complete_MBD_0_0_1";
            string parentPlist = "not_preamble";
            List<string> pinNames = new List<string>() { "NOAB_00" };

            Mock<IFailureData> failMock = new Mock<IFailureData>();

            failMock.Setup(x => x.GetDomainName()).Returns(domain);
            failMock.Setup(x => x.GetVectorAddress()).Returns(address);
            failMock.Setup(x => x.GetPatternName()).Returns(patternName);
            failMock.Setup(x => x.GetFailingPinNames()).Returns(pinNames);
            failMock.Setup(x => x.GetParentPlistName()).Returns(parentPlist);

            var failData = new List<IFailureData>() { failMock.Object };

            Mock<ILabel> labelMock = new Mock<ILabel>();
            labelMock.Setup(x => x.GetName()).Returns(label);
            labelMock.Setup(x => x.GetAddress()).Returns(address);

            Mock<IPatternService> psMock = new Mock<IPatternService>();
            psMock.Setup(x => x.GetLabelFromAddress(patternName, domain, address, false)).Returns(labelMock.Object);

            Prime.Services.PatternService = psMock.Object;

            Mock<IPlistObject> mockPlistObj = new Mock<IPlistObject>();
            mockPlistObj.Setup(x => x.IsPatternAnAmble("preamblePattern")).Returns(false);
            Mock<IPlistService> mockPlistService = new Mock<IPlistService>();
            mockPlistService.Setup(x => x.GetPlistObject(It.IsAny<string>())).Returns(mockPlistObj.Object);

            Prime.Services.PlistService = mockPlistService.Object;

            var pinMappingSet = this.deserializedMetadata.GetPinMappingSet("4_CORES");
            pinMappingSet.ValidateAndSetupItems();

            var prescreen = new PrescreenTest(pinMappingSet, "FAKE", "FAKE", 0, "key", LSARasterTC.PrintMode.CTVMODE, "fake");
            prescreen.Execute(false, failData);
        }

        /// <summary>
        /// Check that TC fails during Verify when missing required params.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void Check_Params_Fail()
        {
            var test = new LSARasterTC
            {
                PrescreenHryFlowToken = "Fake",
                PrescreenHryFreqToken = "Fake",
                ExecutionMode = LSARasterTC.TestInstanceMode.PRESCREEN,
                PrescreenPrintMode = LSARasterTC.PrintMode.CTVMODE,
            };

            test.Verify();
            Assert.IsTrue(false, "No exceptions detected during verify when one was expected.");
        }
    }
}
