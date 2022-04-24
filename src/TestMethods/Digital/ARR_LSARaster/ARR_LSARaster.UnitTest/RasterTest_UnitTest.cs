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
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PatternService;
    using Prime.PlistService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;
    using Prime.UserVarService;
    using Prime.VoltageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class RasterTest_UnitTest
    {
        private Mock<IConsoleService> mockConsole = new Mock<IConsoleService>();
        private Mock<ISharedStorageService> mockSharedStorage = new Mock<ISharedStorageService>();
        private Mock<IUserVarService> mockUserVar = new Mock<IUserVarService>();
        private Mock<ITestProgramService> mockTp = new Mock<ITestProgramService>();
        private Mock<IPatConfigService> mockPatConfig = new Mock<IPatConfigService>();
        private Mock<IDatalogService> mockDatalog = new Mock<IDatalogService>();
        private Mock<IVoltageService> mockVoltage = new Mock<IVoltageService>();
        private Mock<IFunctionalService> mockFunctional = new Mock<IFunctionalService>();

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
            this.mockConsole.Setup(x => x.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => { Console.WriteLine($"ERROR: {msg}"); });
            this.mockConsole.Setup(x => x.PrintDebug(It.IsAny<string>()))
                .Callback<string>((string msg) => { Console.WriteLine($"{msg}"); });
            Prime.Services.ConsoleService = this.mockConsole.Object;

            this.mockDatalog.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()))
                .Verifiable();
            this.mockDatalog.Setup(x => x.GetItuffStrgvalWriter())
                .Returns(new FakeStrgvalFormat());
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
        /// Exit out of port -1 if Prescreen detected no failing arrays to raster if prescreen_map_name is defined.
        /// </summary>
        [ExpectedException(typeof(InvalidOperationException))]
        public void NoFailingArrays()
        {
            var test = new RasterTest(new MetadataConfig(), new RasterConfig(), "fake levels", "fake timings", "fake pinMappingSet", string.Empty, string.Empty, "default", string.Empty);
            test.Execute();

            Assert.IsTrue(test.ExitPort == -1);
        }

        /// <summary>
        /// Method for decoding ctvData into a defect object.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void DecodeCtvData_MisalignedException()
        {
            Dictionary<string, string> ctvData = new Dictionary<string, string>()
            {
                { "NOAB_00", "00111100001010001111100110000011000000" },
            };

            var test = new RasterTest(this.deserializedMetadata, this.deserializedRaster, "fake levels", "fake timings", "4_CORES", string.Empty, string.Empty, string.Empty, string.Empty);

            RasterTest.DecodeCtvData(ctvData, this.deserializedMetadata.GetPinMappingSet("4_CORES"), this.deserializedMetadata.GetCaptureSet("LSA_RASTER_CAPTURE_DECODING_SET_CORE"), "ARRAY_A", null);
        }

        /// <summary>
        /// Method for decoding ctvData into a defect object.
        /// </summary>
        [TestMethod]
        public void DecodeCtvData()
        {
            // Not realistic that we find ctvData from more than one pin per given slice, but it's nice to have the functionality
            Dictionary<string, string> ctvData = new Dictionary<string, string>()
            {
                { "NOAB_00", "0000000000000000000000000000000000000000000000000000000000000000000000000000000000" },
                { "NOAB_01", "0000000000000000000000000000000000000001110010010100000000000000000000000000001010" },
            };

            BigCoreDefect defectZero = new BigCoreDefect(0, 0, "000000000000", "00000000000000000000000000000000");
            defectZero.Array = "ARRAY_A";
            defectZero.Slice = "0";
            BigCoreDefect defectOne = new BigCoreDefect(4, 10, "111000000000", "01010000000000000000000000000000");
            defectOne.Array = "ARRAY_A";
            defectOne.Slice = "1";

            var defects = RasterTest.DecodeCtvData(ctvData, this.deserializedMetadata.GetPinMappingSet("4_CORES"), this.deserializedMetadata.GetCaptureSet("LSA_RASTER_CAPTURE_DECODING_SET_CORE"), "ARRAY_A", null);

            Assert.IsTrue(defects[0].CreateTFileString() == defectZero.CreateTFileString() && defects[1].CreateTFileString() == defectOne.CreateTFileString());
        }

        /// <summary>
        /// Verify method works when more than one "chunk" is present from a pin.
        /// </summary>
        [TestMethod]
        public void DecodeCtvData_MultipleChunks()
        {
            // Not realistic that we find ctvData from more than one pin per given slice, but it's nice for testing (and to have the functionality)
            Dictionary<string, string> ctvData = new Dictionary<string, string>()
            {
                { "NOAB_00", "0000000000000000000000000000000000000000000000000000000000000000000000000000000000" },
                { "NOAB_01", "00000000000000000000000000000000000000011100100101000000000000000000000000000010100000000000000000000000000000000000000001110010010100000000000000000000000000001010" },
            };

            BigCoreDefect defectZero = new BigCoreDefect(0, 0, "000000000000", "00000000000000000000000000000000");
            defectZero.Array = "ARRAY_A";
            defectZero.Slice = "0";
            BigCoreDefect defectOne = new BigCoreDefect(4, 10, "111000000000", "01010000000000000000000000000000");
            defectOne.Array = "ARRAY_A";
            defectOne.Slice = "1";

            var defects = RasterTest.DecodeCtvData(ctvData, this.deserializedMetadata.GetPinMappingSet("4_CORES"), this.deserializedMetadata.GetCaptureSet("LSA_RASTER_CAPTURE_DECODING_SET_CORE"), "ARRAY_A", null);

            Assert.IsTrue(defects[0].CreateTFileString() == defectZero.CreateTFileString() && defects[1].CreateTFileString() == defectOne.CreateTFileString() && defects[2].CreateTFileString() == defectOne.CreateTFileString());
        }

        /// <summary>
        /// Check that TC can create an internal DB using a string passed in by the user.
        /// </summary>
        [TestMethod]
        public void SimulateDatabase()
        {
            string simulationDB = "bpu_trel,1,0|1|0;bpu_bme,1,0|2|0";
            DBContainer comparisonContainer = DBContainer.CreateDBFromString(simulationDB, MetadataConfig.ArrayType.BIGCORE);
            var rasterMap = comparisonContainer.CreateRasterMap(MetadataConfig.ArrayType.BIGCORE);
            bool isRasterMapValid = Equals(rasterMap["bpu_trel"]["1"][0], new Tuple<int, int, int>(0, 1, 0)) && Equals(rasterMap["bpu_bme"]["1"][0], new Tuple<int, int, int>(0, 2, 0));
            Assert.IsTrue(isRasterMapValid);
        }

        /// <summary>
        /// Generate a TFile string for all rastered defects in DB.
        /// </summary>
        [TestMethod]
        public void GenerateTFileStrings()
        {
            var test = new RasterTest(this.deserializedMetadata, this.deserializedRaster, "fake levels", "fake timings", "fake pinMappingSet", string.Empty, string.Empty, string.Empty, string.Empty);

            AtomDefect defect = new AtomDefect(0, 0, "failaddr", new Dictionary<string, string>());
            defect.Array = "ic_btb";
            defect.Bank = 0;
            defect.CoreToFailIO = new Dictionary<string, string>
            {
                { "0", "0101100000000000010101100111000000000000000000000000010110000000" },
            };
            defect.Dword = 0;
            defect.FailAddress = "110000000000";
            defect.Module = "0";
            defect.SendToRepair = true;

            var database = new Dictionary<string, List<IDefect>> { };
            defect.AddToInternalDatabase(ref database);
            var outcome = test.GenerateTFileStrings(database);

            Assert.IsTrue(outcome == "DUT 0, 0\nTest: \nArray: ic_btb\nModule: 0\n0,0,0,0xC00,0x5800567000000580\n");
        }

        /// <summary>
        /// Generate a TFile string for all rastered defects in DB.
        /// </summary>
        [TestMethod]
        public void GenerateTFileStrings_DutCollection()
        {
            this.mockUserVar.Setup(x => x.Exists("fake_collection", "x"))
                .Returns(true);
            this.mockUserVar.Setup(x => x.Exists("fake_collection", "y"))
                .Returns(true);
            this.mockUserVar.Setup(x => x.GetStringValue("fake_collection", "x"))
                .Returns("dutX");
            this.mockUserVar.Setup(x => x.GetStringValue("fake_collection", "x"))
                .Returns("dutY");
            Prime.Services.UserVarService = this.mockUserVar.Object;

            var test = new RasterTest(this.deserializedMetadata, this.deserializedRaster, "fake levels", "fake timings", "fake pinMappingSet", string.Empty, string.Empty, string.Empty, string.Empty);
            test.DutCollection = "fake_collection";
            test.DutXGlobal = "x";
            test.DutYGlobal = "y";

            AtomDefect defect = new AtomDefect(0, 0, "failaddr", new Dictionary<string, string>());
            defect.Array = "ic_btb";
            defect.Bank = 0;
            defect.CoreToFailIO = new Dictionary<string, string>
            {
                { "0", "0101100000000000010101100111000000000000000000000000010110000000" },
            };
            defect.Dword = 0;
            defect.FailAddress = "110000000000";
            defect.Module = "0";
            defect.SendToRepair = true;

            var database = new Dictionary<string, List<IDefect>> { };
            defect.AddToInternalDatabase(ref database);
            var outcome = test.GenerateTFileStrings(database);
            string compareResult = "DUT dutY, \nTest: \nArray: ic_btb\nModule: 0\n0,0,0,0xC00,0x5800567000000580\n";
            Assert.IsTrue(outcome == compareResult);
        }

        /// <summary>
        /// Ensure we can print the RasterMap successfully.
        /// </summary>
        [TestMethod]
        public void PrintRasterMap()
        {
            var arrayType = MetadataConfig.ArrayType.BIGCORE;
            DBContainer container = DBContainer.CreateDBFromString(
                "bpu_trel,1,0|1|0;bpu_bme,1,0|2|0;bpu_bme,1,0|0|0;bpu_bme,1,1|0|0;bpu_trol,1,0|0|0",
                arrayType);
            RasterTest.PrintRasterMap(
                container.CreateRasterMap(arrayType),
                MetadataConfig.ConvertArrayTypeToDBKeyName(arrayType));

            string expectedHeader = "Performing raster on following arrays at specified locations:\n";
            string expectedOutput = "Array: bpu_trel Slice: 1 MBD: [010]\nArray: bpu_bme Slice: 1 MBD: [020][000][100]\nArray: bpu_trol Slice: 1 MBD: [000]\n";
            this.mockConsole.Verify(x => x.PrintDebug(expectedHeader));
            this.mockConsole.Verify(x => x.PrintDebug(expectedOutput));
        }

        /// <summary>
        /// Ensure we achieve proper behaviour when performing parallel raster.
        /// </summary>
        /// <remarks> This is gonna be one of the hardest to setup. </remarks>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void Execute_RasterDatabaseParallel_NoMatchingDefectFound()
        {
            Mock<IPatConfigHandle> fakeHandle = new Mock<IPatConfigHandle>();
            fakeHandle.Setup(x => x.GetExpectedDataSize())
                .Returns(2);
            fakeHandle.Setup(x => x.SetData(It.IsAny<string>())).Verifiable();
            this.mockPatConfig
                .Setup(x => x.GetPatConfigHandleWithPlist(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fakeHandle.Object);
            Prime.Services.PatConfigService = this.mockPatConfig.Object;

            RasterTest test = new RasterTest(
                this.deserializedMetadata,
                this.deserializedRaster,
                "fake",
                "fake",
                "4_CORES",
                "fake",
                string.Empty,
                string.Empty,
                "true",
                "bpu_bme,1,0|1|0");

            test.Execute();
        }

        /// <summary>
        /// Ensure we achieve proper behaviour when performing parallel raster.
        /// </summary>
        /// <remarks> This is gonna be one of the hardest to setup. </remarks>
        [TestMethod]
        public void Execute_RasterDatabaseParallel()
        {
            string compareString = "DUT 0, 0\nTest: \nArray: bpu_bme\nSlice: 1\n0,0,0x000,0x00000000\n";
            Mock<IPatConfigHandle> fakeHandle = new Mock<IPatConfigHandle>();
            fakeHandle.Setup(x => x.GetExpectedDataSize())
                .Returns(2);
            fakeHandle.Setup(x => x.SetData(It.IsAny<string>())).Verifiable();
            this.mockPatConfig
                .Setup(x => x.GetPatConfigHandleWithPlist(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fakeHandle.Object);
            Prime.Services.PatConfigService = this.mockPatConfig.Object;

            this.mockTest.Setup(x => x.GetCtvData()).Returns(new Dictionary<string, string>()
            {
                { "NOAB_01", "0000000000000000000000000000000000000000000000000000000000000000000000000000000000" },
            });

            this.mockTest.Setup(x => x.GetPerCycleFailures()).Returns(new List<IFailureData>());
            this.mockFunctional
                .Setup(x => x.CreateCaptureFailureAndCtvPerPinTest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .Returns(this.mockTest.Object);
            Prime.Services.FunctionalService = this.mockFunctional.Object;

            RasterTest test = new RasterTest(
                this.deserializedMetadata,
                this.deserializedRaster,
                "fake",
                "fake",
                "4_CORES",
                "fake",
                string.Empty,
                string.Empty,
                "true",
                "bpu_bme,1,0|0|0");

            test.Execute();
            Assert.AreEqual(test.TFile, compareString);
        }

        /// <summary>
        /// Ensure we achieve proper behaviour when performing parallel raster.
        /// </summary>
        /// <remarks> This is gonna be one of the hardest to setup. </remarks>
        [TestMethod]
        public void Execute_RasterDatabaseSerially()
        {
            string compareString = "DUT 0, 0\nTest: \nArray: bpu_bme\nSlice: 1\n0,0,0x000,0x00000000\n";
            Mock<IPatConfigHandle> fakeHandle = new Mock<IPatConfigHandle>();
            fakeHandle.Setup(x => x.GetExpectedDataSize())
                .Returns(2);
            fakeHandle.Setup(x => x.SetData(It.IsAny<string>())).Verifiable();
            this.mockPatConfig
                .Setup(x => x.GetPatConfigHandleWithPlist(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fakeHandle.Object);
            Prime.Services.PatConfigService = this.mockPatConfig.Object;

            this.mockTest.Setup(x => x.GetCtvData()).Returns(new Dictionary<string, string>()
            {
                { "NOAB_01", "0000000000000000000000000000000000000000000000000000000000000000000000000000000000" },
            });

            this.mockTest.Setup(x => x.GetPerCycleFailures()).Returns(new List<IFailureData>());
            this.mockFunctional
                .Setup(x => x.CreateCaptureFailureAndCtvPerPinTest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .Returns(this.mockTest.Object);
            Prime.Services.FunctionalService = this.mockFunctional.Object;

            RasterTest test = new RasterTest(
                this.deserializedMetadata,
                this.deserializedRasterNP,
                "fake",
                "fake",
                "4_CORES",
                "fake",
                string.Empty,
                string.Empty,
                "true",
                "bpu_bme,1,0|0|0");

            test.Execute();
            Assert.AreEqual(test.TFile, compareString);
        }

        /// <summary>
        /// Ensure we achieve proper behaviour when performing parallel raster.
        /// </summary>
        /// <remarks> This is gonna be one of the hardest to setup. </remarks>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void Execute_RasterDatabaseSerially_NoMatchingDefectFound()
        {
            Mock<IPatConfigHandle> fakeHandle = new Mock<IPatConfigHandle>();
            fakeHandle.Setup(x => x.GetExpectedDataSize())
                .Returns(2);
            fakeHandle.Setup(x => x.SetData(It.IsAny<string>())).Verifiable();
            this.mockPatConfig
                .Setup(x => x.GetPatConfigHandleWithPlist(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fakeHandle.Object);
            Prime.Services.PatConfigService = this.mockPatConfig.Object;

            this.mockTest.Setup(x => x.GetCtvData()).Returns(new Dictionary<string, string>()
            {
                { "NOAB_01", "0000000000000000000000000000000000000000000000000000000000000000000000000000000000" },
            });

            this.mockTest.Setup(x => x.GetPerCycleFailures()).Returns(new List<IFailureData>());
            this.mockFunctional
                .Setup(x => x.CreateCaptureFailureAndCtvPerPinTest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .Returns(this.mockTest.Object);
            Prime.Services.FunctionalService = this.mockFunctional.Object;

            RasterTest test = new RasterTest(
                this.deserializedMetadata,
                this.deserializedRasterNP,
                "fake",
                "fake",
                "4_CORES",
                "fake",
                string.Empty,
                string.Empty,
                "true",
                "bpu_bme,1,0|1|0");

            test.Execute();
        }

        /// <summary>
        /// Database reductions can be logged once created by the internal DB.
        /// </summary>
        [TestMethod]
        public void LogDatabaseReductions()
        {
            string simulationDB =
                "bpu_trel,1,0|1|0;bpu_bme,1,0|2|0;bpu_bme,1,0|0|0;bpu_bme,1,0|2|0;bpu_bme,1,1|0|0;bpu_bme,1,1|0|0;bpu_trol,1,0|0|0;"
                + "bpu_trel,2,0|1|0;bpu_bme,2,0|0|0;bpu_trol,2,0|0|0;bpu_bmo,2,0|0|0;"
                + "bpu_gll,5,0|0|0;bpu_gll,6,0|0|0;bpu_gll,7,0|0|0";
            DBContainer container = DBContainer.CreateDBFromString(
                simulationDB,
                MetadataConfig.ArrayType.BIGCORE,
                this.deserializedRasterRC.GetReductionConfigSet("Example_Set"));

            container.CreateRasterMap(MetadataConfig.ArrayType.BIGCORE);
            RasterTest.LogDatabaseReductions(container);
            this.mockConsole.Verify(x => x.PrintDebug("array_bpu_gll_slice_5_1_CORES_MAX_COUNT_REDUCTION"));
            this.mockConsole.Verify(x => x.PrintDebug("array_bpu_bme_slice_1_mbd_0_2_0_mbd_0_0_0_MBDS_REDUCTION"));
            this.mockConsole.Verify(x => x.PrintDebug("array_1_slice_bpu_trel_1_ARRAY_MAX_COUNT_REDUCTION"));
            this.mockConsole.Verify(x => x.PrintDebug("slice_2_array_bpu_trel_mbds_1_array_bpu_bme_mbds_1_array_bpu_trol_mbds_1_array_bpu_bmo_mbds_1_ARRAY_MAF_MAX_REDUCTION"));
        }

        /// <summary>
        /// Ensure we get the proper plist to execute when multicore mode is enabled.
        /// </summary>
        [TestMethod]
        public void GetMulticorePlistToExecute()
        {
            JsonInput multicoreMetadata = new JsonInput(File.ReadAllText(@"./TestInput/Metadata_Multicore.json"));
            JsonInput multicoreRaster = new JsonInput(File.ReadAllText(@"./TestInput/RasterConfig_Multicore.json"));
            var plistName = RasterTest.GetMulticorePlistToExecute(
                multicoreMetadata.DeserializeInput<MetadataConfig>().GetPinMappingSet("Multicore"),
                multicoreRaster.DeserializeInput<RasterConfig>().GetLdatArray("Multicore"),
                "1");
            Assert.IsTrue(plistName == "example_s1_plist");
        }

        /// <summary>
        /// Throw exception when no plist matching HryName turns up in search.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void GetMulticorePlistToExecute_NoPlistMatchingHryName()
        {
            JsonInput multicoreMetadata = new JsonInput(File.ReadAllText(@"./TestInput/Metadata_Multicore.json"));
            JsonInput multicoreRaster = new JsonInput(File.ReadAllText(@"./TestInput/RasterConfig_Multicore.json"));
            RasterTest.GetMulticorePlistToExecute(
                multicoreMetadata.DeserializeInput<MetadataConfig>().GetPinMappingSet("Multicore"),
                multicoreRaster.DeserializeInput<RasterConfig>().GetLdatArray("Multicore_noMatches"),
                "1");
            Assert.IsTrue(false, "Should have thrown an exception when no plist is found.");
        }

        /// <summary>
        /// Throw exception when there is a mismatch in number of plist to number of hry names.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void GetMulticorePlistToExecute_HryNameToNumOfPlistMismatch()
        {
            JsonInput multicoreMetadata = new JsonInput(File.ReadAllText(@"./TestInput/Metadata_Multicore.json"));
            JsonInput multicoreRaster = new JsonInput(File.ReadAllText(@"./TestInput/RasterConfig_Multicore.json"));
            RasterTest.GetMulticorePlistToExecute(
                multicoreMetadata.DeserializeInput<MetadataConfig>().GetPinMappingSet("Multicore"),
                multicoreRaster.DeserializeInput<RasterConfig>().GetLdatArray("Multicore_hryMismatch"),
                "1");
            Assert.IsTrue(false, "Should have thrown an exception when there is a mismatch of # of plist to hry names.");
        }

        /// <summary>
        /// DFM fails are properly checked and report if they end on non-complete label or not.
        /// </summary>
        [TestMethod]
        public void CheckDFMFails_EndOnNonCompleteLabel()
        {
            FakeFailureData fail1 = new FakeFailureData("pattern1", "domain1", 1000);
            FakeFailureData fail2 = new FakeFailureData("pattern2", "domain2", 2000);
            Mock<ILabel> fakeLabel = new Mock<ILabel>();
            fakeLabel.Setup(x => x.GetName())
                .Returns("helloAgain");
            Mock<IPatternService> mockPattern = new Mock<IPatternService>();
            mockPattern.Setup(x => x.GetLabelFromAddress("pattern1", "domain1", 1000, false))
                .Returns(fakeLabel.Object);

            Prime.Services.PatternService = mockPattern.Object;

            Mock<IPlistObject> mockPlistObj = new Mock<IPlistObject>();
            mockPlistObj.Setup(x => x.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            Mock<IPlistService> mockPlistService = new Mock<IPlistService>();
            mockPlistService.Setup(x => x.GetPlistObject("fake")).Returns(mockPlistObj.Object);

            Prime.Services.PlistService = mockPlistService.Object;

            Assert.AreEqual(RasterTest.CheckDFMFails(new List<IFailureData>() { fail1 }, "fake"), RasterTest.ExecutionStates.FailOnNonCompleteLabel);
        }

        /// <summary>
        /// DFM fails are properly checked and report if they end on non-complete label or not.
        /// </summary>
        [TestMethod]
        public void CheckDFMFails_FailOnAmble()
        {
            FakeFailureData fail1 = new FakeFailureData("pattern1", "domain1", 1000);
            FakeFailureData fail2 = new FakeFailureData("pattern2", "domain2", 2000);
            Mock<ILabel> fakeLabel = new Mock<ILabel>();
            fakeLabel.Setup(x => x.GetName())
                .Returns("helloAgain");
            Mock<IPatternService> mockPattern = new Mock<IPatternService>();
            mockPattern.Setup(x => x.GetLabelFromAddress("pattern1", "domain1", 1000, false))
                .Returns(fakeLabel.Object);

            Prime.Services.PatternService = mockPattern.Object;

            Mock<IPlistObject> mockPlistObj = new Mock<IPlistObject>();
            mockPlistObj.Setup(x => x.IsPatternAnAmble("pattern1")).Returns(true);
            Mock<IPlistService> mockPlistService = new Mock<IPlistService>();
            mockPlistService.Setup(x => x.GetPlistObject("fake")).Returns(mockPlistObj.Object);

            Prime.Services.PlistService = mockPlistService.Object;

            Assert.AreEqual(RasterTest.CheckDFMFails(new List<IFailureData>() { fail1 }, "fake"), RasterTest.ExecutionStates.FailOnAmble);
        }

        /// <summary>
        /// DFM fails are properly checked and report if they end on non-complete label or not.
        /// </summary>
        [TestMethod]
        public void CheckDFMFails_EndOnCompleteLabel()
        {
            FakeFailureData fail1 = new FakeFailureData("pattern1", "domain1", 1000);
            FakeFailureData fail2 = new FakeFailureData("pattern2", "domain2", 2000);
            Mock<ILabel> fakeLabel = new Mock<ILabel>();
            fakeLabel.Setup(x => x.GetName())
                .Returns("Complete");

            Mock<IPatternService> mockPattern = new Mock<IPatternService>();
            mockPattern.Setup(x => x.GetLabelFromAddress("pattern1", "domain1", 1000, false))
                .Returns(fakeLabel.Object);
            Prime.Services.PatternService = mockPattern.Object;

            Assert.AreEqual(RasterTest.CheckDFMFails(new List<IFailureData>() { fail1 }, "fake"), RasterTest.ExecutionStates.Success);
        }

        /// <summary>
        /// Determine plist to execute when given a non-multicore pinmapping.
        /// </summary>
        [TestMethod]
        public void GetPlistToExecute_NonMulticore()
        {
            string plist = RasterTest.GetPlistToExecute(
                this.deserializedRaster.GetLdatArray("bpu_bme"),
                this.deserializedMetadata.GetPinMappingSet("4_CORES"),
                "0");

            Assert.AreEqual(plist, "arr_pbist_mclk_x_mcis_core_raster_lsa_bpu_bme_indirect_list");
        }

        /// <summary>
        /// Determine plist to execute when given a multicore pinmapping.
        /// </summary>
        [TestMethod]
        public void GetPlistToExecute_Multicore()
        {
            var serializedMulticoreMeta = new JsonInput(File.ReadAllText(@".\TestInput\Metadata_Multicore.json"));
            var deserializedMulticoreMeta = serializedMulticoreMeta.DeserializeInput<MetadataConfig>();
            JsonInput serializedMulticoreRaster = new JsonInput(File.ReadAllText(@"./TestInput/RasterConfig_Multicore.json"));
            var deserializedmulticoreRaster = serializedMulticoreRaster.DeserializeInput<RasterConfig>();
            string plist0 = RasterTest.GetPlistToExecute(
                deserializedmulticoreRaster.GetLdatArray("Multicore"),
                deserializedMulticoreMeta.GetPinMappingSet("Multicore"),
                "0");
            string plist1 = RasterTest.GetPlistToExecute(
                deserializedmulticoreRaster.GetLdatArray("Multicore"),
                deserializedMulticoreMeta.GetPinMappingSet("Multicore"),
                "1");

            Assert.IsTrue(plist0 == "example_s0_plist" && plist1 == "example_s1_plist");
        }

        /// <summary>
        /// Throw an exception when pinmapping has nothing in its array.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetPlistToExecute_Exception()
        {
            JsonInput missingPlist = new JsonInput(File.ReadAllText(@"./TestInput/RasterConfig_MissingPlist.json"));
            var deserializedConfig = missingPlist.DeserializeInput<RasterConfig>();

            RasterTest.GetPlistToExecute(
                deserializedConfig.GetLdatArray("missing_plist"),
                this.deserializedMetadata.GetPinMappingSet("4_CORES"),
                "0");
        }

        /// <summary>
        /// Verify calls on Prime services when targeting a particular MBD address.
        /// </summary>
        [TestMethod]
        public void ExecuteTestOnMBDAddress()
        {
            var ctvData = RasterTest.ExecuteTestOnMBDAddress(
                new List<string>() { "hello" },
                new List<string>(),
                new List<IPatConfigHandle>(),
                this.deserializedMetadata.GetPinMappingSet("4_CORES"),
                "fake_plist",
                "0",
                "fake_condition",
                "fake_levels",
                "fake_timings",
                out var failData);

            Assert.IsTrue(ctvData.Count == 0);
        }

        /// <summary>
        /// Verify calls on Prime services when targeting a particular MBD address.
        /// </summary>
        [TestMethod]
        public void ExecuteTestOnMBDAddress_HasPatternPerSliceId()
        {
            Mock<IPlistObject> mockPlistObject = new Mock<IPlistObject>();
            mockPlistObject
                .Setup(x => x.EnableGivenPatternsDisableRest(It.Is<HashSet<string>>(hashset => hashset.Contains("pattern0"))))
                .Verifiable();
            Mock<IPlistService> mockPlist = new Mock<IPlistService>();
            mockPlist.Setup(x => x.GetPlistObject("fake_plist")).Returns(mockPlistObject.Object);
            Prime.Services.PlistService = mockPlist.Object;

            var serializedPatternPerSlice = new JsonInput(File.ReadAllText(@".\TestInput\Metadata_PatternPerSlice.json"));
            var deserializedPatternPerSlice = serializedPatternPerSlice.DeserializeInput<MetadataConfig>();
            var ctvData = RasterTest.ExecuteTestOnMBDAddress(
                new List<string>() { "hello" },
                new List<string>(),
                new List<IPatConfigHandle>(),
                deserializedPatternPerSlice.GetPinMappingSet("PATTERN_PER_SLICE"),
                "fake_plist",
                "0",
                "fake_condition",
                "fake_levels",
                "fake_timings",
                out var failData);

            Assert.IsTrue(ctvData.Count == 0);
        }

        /// <summary>
        /// Verify calls on Prime services when targeting a particular MBD address.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void ExecuteTestOnMBDAddress_HasPatternPerSliceId_ParallelSlice()
        {
            Mock<IPlistObject> mockPlistObject = new Mock<IPlistObject>();
            mockPlistObject
                .Setup(x => x.EnableGivenPatternsDisableRest(It.Is<HashSet<string>>(hashset => hashset.Contains("pattern0"))))
                .Verifiable();
            Mock<IPlistService> mockPlist = new Mock<IPlistService>();
            mockPlist.Setup(x => x.GetPlistObject("fake_plist")).Returns(mockPlistObject.Object);
            Prime.Services.PlistService = mockPlist.Object;

            var serializedPatternPerSlice = new JsonInput(File.ReadAllText(@".\TestInput\Metadata_PatternPerSlice.json"));
            var deserializedPatternPerSlice = serializedPatternPerSlice.DeserializeInput<MetadataConfig>();
            var ctvData = RasterTest.ExecuteTestOnMBDAddress(
                new List<string>() { "hello" },
                new List<string>(),
                new List<IPatConfigHandle>(),
                deserializedPatternPerSlice.GetPinMappingSet("PATTERN_PER_SLICE"),
                "fake_plist",
                "PARALLEL",
                "fake_condition",
                "fake_levels",
                "fake_timings",
                out var failData);

            Assert.IsTrue(ctvData.Count == 0);
        }

        /// <summary>
        /// Assert proper remapping of db.
        /// </summary>
        [TestMethod]
        public void GetSlicesForMBDAddress()
        {
            var tupleKey = new Tuple<int, int, int>(0, 0, 0);
            Dictionary<string, List<Tuple<int, int, int>>> db = new Dictionary<string, List<Tuple<int, int, int>>>();

            db.Add("1", new List<Tuple<int, int, int>>() { new Tuple<int, int, int>(0, 0, 0) });
            db.Add("2", new List<Tuple<int, int, int>>() { new Tuple<int, int, int>(0, 0, 0) });
            db.Add("3", new List<Tuple<int, int, int>>() { new Tuple<int, int, int>(0, 0, 0) });

            var convertedDB = RasterTest.GetSlicesForMBDAddress(db);
            Assert.IsTrue(
                convertedDB[tupleKey].Contains("1")
                && convertedDB[tupleKey].Contains("2")
                && convertedDB[tupleKey].Contains("3"));
        }
    }
}