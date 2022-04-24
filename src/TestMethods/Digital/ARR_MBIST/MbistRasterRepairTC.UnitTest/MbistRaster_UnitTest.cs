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

namespace MbistUnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using ARR_MBIST;
    using MbistRasterRepairTC;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.SharedStorageService;
    using Prime.TestMethods;
    using Prime.UserVarService;

    /// <summary>Unit test class for MBIST Raster.</summary>
    [TestClass]
    public class MbistRaster_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MbistRaster_UnitTest"/> class.
        /// </summary>
        public MbistRaster_UnitTest()
        {
            this.ConsoleOutput = new List<string>();
            this.ErrorOutput = new List<string>();
            this.TFileOutput = new List<string>();

            this.ConsoleServiceMock = new Mock<Prime.ConsoleService.IConsoleService>(MockBehavior.Strict);
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

            this.DatalogServiceMock = new Mock<Prime.DatalogService.IDatalogService>(MockBehavior.Strict);
            this.DatalogServiceMock.Setup(o => o.WriteToTFile(It.IsAny<string>())).Callback((string s) =>
            {
                this.TFileOutput.Add(s);
            });
            Prime.Services.DatalogService = this.DatalogServiceMock.Object;
        }

        private Mock<Prime.ConsoleService.IConsoleService> ConsoleServiceMock { get; set; }

        private Mock<Prime.DatalogService.IDatalogService> DatalogServiceMock { get; set; }

        private List<string> ConsoleOutput { get; set; }

        private List<string> ErrorOutput { get; set; }

        private List<string> TFileOutput { get; set; }

        /// <summary>Unit test for MBIST Raster.</summary>
        [TestMethod]
        public void Bin2dec()
        {
            Assert.AreEqual(0, MbistRasterAlgorithm.Bin2dec("00000000000000000000000000000000"));
            Assert.AreEqual(1, MbistRasterAlgorithm.Bin2dec("000001"));
            Assert.AreEqual(5, MbistRasterAlgorithm.Bin2dec("000101"));
            Assert.AreEqual(2, MbistRasterAlgorithm.Bin2dec("01000000000000000000", reverse: true));
        }

        /// <summary>Unit test for MBIST Raster.</summary>
        [TestMethod]
        public void Bin2hex()
        {
            Assert.AreEqual("002", MbistRasterAlgorithm.Bin2hex("00000000010"));
            Assert.AreEqual("001", MbistRasterAlgorithm.Bin2hex("000000000001"));
            Assert.AreEqual("0001", MbistRasterAlgorithm.Bin2hex("0000000000001"));
            Assert.AreEqual("A", MbistRasterAlgorithm.Bin2hex("1010"));
            Assert.AreEqual("5", MbistRasterAlgorithm.Bin2hex("1010", reverse: true));
            Assert.AreEqual("DEAD", MbistRasterAlgorithm.Bin2hex("1101111010101101"));
            Assert.AreEqual("12'h001", MbistRasterAlgorithm.Bin2hex("000000000001", prefix: true));

            Assert.AreEqual("68'h00000000000000001", MbistRasterAlgorithm.Bin2hex("00000000000000000000000000000000000000000000000000000000000000000001", prefix: true));
            Assert.AreEqual("67'h00000000000000001", MbistRasterAlgorithm.Bin2hex("0000000000000000000000000000000000000000000000000000000000000000001", prefix: true));
            Assert.AreEqual("66'h00000000000000001", MbistRasterAlgorithm.Bin2hex("000000000000000000000000000000000000000000000000000000000000000001", prefix: true));
            Assert.AreEqual("65'h00000000000000001", MbistRasterAlgorithm.Bin2hex("00000000000000000000000000000000000000000000000000000000000000001", prefix: true));
            Assert.AreEqual("64'h0000000000000001", MbistRasterAlgorithm.Bin2hex("0000000000000000000000000000000000000000000000000000000000000001", prefix: true));
        }

        /// <summary>Unit test for MBIST Raster.</summary>
        [TestMethod]
        public void ExtractData()
        {
            var alg = new MbistRasterAlgorithm(new MbistRasterInput());

            // !                     1111111111222222
            // !           01234567890123456789012345
            var testStr = "ABCDEFGHIJKLMNOPQRSTUVQXYZ";

            Assert.AreEqual("A", Mbist.GetSubData("0", testStr));
            Assert.AreEqual("Z", Mbist.GetSubData("25", testStr));
            Assert.AreEqual("BZ", Mbist.GetSubData("1,25", testStr));
            Assert.AreEqual("EFG", Mbist.GetSubData("4-6", testStr));
            Assert.AreEqual("ABCT", Mbist.GetSubData("0-2,19", testStr));
            Assert.AreEqual("XKJIH", Mbist.GetSubData("23,10-7", testStr));
            Assert.AreEqual("CKPTA", Mbist.GetSubData("2,10-10,15,19,0", testStr));
        }

        /// <summary>Unit test for MBIST Raster.</summary>
        [TestMethod]
        public void RangeToBitList()
        {
            var range = "7,8-8,10-12,3-1,57";
            var expectList = new List<int> { 7, 8, 10, 11, 12, 3, 2, 1, 57 };
            var rslt = Mbist.RangeToList(range);
            CollectionAssert.AreEqual(expectList, rslt);
        }

        /// <summary>Unit test for MBIST Raster.</summary>
        [TestMethod]
        public void MustRepair_ColRepair()
        {
            var config = new MbistRasterInput();
            config.RepairMap = new Dictionary<string, Dictionary<string, List<MbistRasterInput.MemRepairMap>>>();
            config.RepairMap["tdk0_BP0_WBP0"] = new Dictionary<string, List<MbistRasterInput.MemRepairMap>>();
            config.RepairMap["tdk0_BP0_WBP0"]["STEP0"] = new List<MbistRasterInput.MemRepairMap> { new MbistRasterInput.MemRepairMap("1-2", "SDKL0R_BP0WBP0MEM12") };

            config.RepairGroups = new Dictionary<string, MbistRasterInput.RepairGroup>();
            config.RepairGroups["SDKL0R_BP0WBP0MEM12"] = new MbistRasterInput.RepairGroup(
                "SDKL0R",
                "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                new List<MbistRasterInput.RepairGroup.RepairElement>
                {
                    new MbistRasterInput.RepairGroup.RepairElement(Mbist.RepairType.ROW, "22-28"),
                    new MbistRasterInput.RepairGroup.RepairElement(Mbist.RepairType.ROW, "29-35"),
                    new MbistRasterInput.RepairGroup.RepairElement(Mbist.RepairType.COL, "36-39"),
                });

            var alg = new MbistRasterAlgorithm(config);
            alg.FailDatabase.Add("tdk0_BP0_WBP0", 0, 1, 0, 1, 0, 5);
            alg.FailDatabase.Add("tdk0_BP0_WBP0", 0, 1, 0, 2, 0, 5);
            alg.FailDatabase.Add("tdk0_BP0_WBP0", 0, 1, 0, 3, 0, 5);
            alg.FailDatabase.Add("tdk0_BP0_WBP0", 0, 1, 0, 4, 0, 5);
            alg.FailDatabase.Add("tdk0_BP0_WBP0", 0, 1, 0, 5, 0, 5);
            alg.FailDatabase.Add("tdk0_BP0_WBP0", 0, 1, 0, 6, 0, 5);

            Assert.IsTrue(alg.CheckMustRepair());
            Assert.IsFalse(alg.FailDatabase.RepairsNeeded());
            Assert.AreEqual("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX0101", config.RepairGroups["SDKL0R_BP0WBP0MEM12"].GlobalValue);
        }

        /// <summary>Unit test for MBIST Raster.</summary>
        [TestMethod]
        public void MustRepair_RowRepair()
        {
            var config = new MbistRasterInput();
            config.RepairMap = new Dictionary<string, Dictionary<string, List<MbistRasterInput.MemRepairMap>>>();
            config.RepairMap["tdk0_BP0_WBP0"] = new Dictionary<string, List<MbistRasterInput.MemRepairMap>>();
            config.RepairMap["tdk0_BP0_WBP0"]["STEP0"] = new List<MbistRasterInput.MemRepairMap> { new MbistRasterInput.MemRepairMap("1-2", "SDKL0R_BP0WBP0MEM12") };
            config.RepairMap["tdk0_BP0_WBP0"]["STEP1"] = new List<MbistRasterInput.MemRepairMap> { new MbistRasterInput.MemRepairMap("3-4", "SDKL0R_BP0WBP0MEM34") };

            config.RepairGroups = new Dictionary<string, MbistRasterInput.RepairGroup>();
            config.RepairGroups["SDKL0R_BP0WBP0MEM12"] = new MbistRasterInput.RepairGroup(
                "SDKL0R",
                "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                new List<MbistRasterInput.RepairGroup.RepairElement>
                {
                    new MbistRasterInput.RepairGroup.RepairElement(Mbist.RepairType.ROW, "22-28"),
                    new MbistRasterInput.RepairGroup.RepairElement(Mbist.RepairType.ROW, "29-35"),
                    new MbistRasterInput.RepairGroup.RepairElement(Mbist.RepairType.COL, "36-39"),
                });
            config.RepairGroups["SDKL0R_BP0WBP0MEM34"] = new MbistRasterInput.RepairGroup(
                "SDKL0R",
                "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                new List<MbistRasterInput.RepairGroup.RepairElement>
                {
                    new MbistRasterInput.RepairGroup.RepairElement(Mbist.RepairType.ROW, "0-7"),
                    new MbistRasterInput.RepairGroup.RepairElement(Mbist.RepairType.ROW, "8-15"),
                    new MbistRasterInput.RepairGroup.RepairElement(Mbist.RepairType.COL, "16-21"),
                });

            var alg = new MbistRasterAlgorithm(config);
            alg.FailDatabase.Add("tdk0_BP0_WBP0", 0, 1, 0, 1, 0, 0);
            alg.FailDatabase.Add("tdk0_BP0_WBP0", 0, 2, 0, 1, 0, 1);
            alg.FailDatabase.Add("tdk0_BP0_WBP0", 1, 3, 0, 2, 0, 8);
            alg.FailDatabase.Add("tdk0_BP0_WBP0", 1, 4, 0, 2, 0, 7);

            Assert.IsTrue(alg.CheckMustRepair());
            Assert.IsFalse(alg.FailDatabase.RepairsNeeded());
            Assert.AreEqual("XXXXXXXXXXXXXXXXXXXXXX0000001XXXXXXXXXXX", config.RepairGroups["SDKL0R_BP0WBP0MEM12"].GlobalValue);
            Assert.AreEqual("00000010XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", config.RepairGroups["SDKL0R_BP0WBP0MEM34"].GlobalValue);
        }

        /// <summary>Unit test for MBIST Raster.</summary>
        [TestMethod]
        public void UpdateRasterLog()
        {
            var userVarServiceMock = new Mock<Prime.UserVarService.IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.GetStringValue("SCVars", "SC_WAFERX")).Returns("0");
            userVarServiceMock.Setup(o => o.GetStringValue("SCVars", "SC_WAFERY")).Returns("1");
            userVarServiceMock.Setup(o => o.Exists("SCVars", "SC_WAFERX")).Returns(true);
            userVarServiceMock.Setup(o => o.Exists("SCVars", "SC_WAFERY")).Returns(true);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var alg = new MbistRasterAlgorithm(new MbistRasterInput());

            alg.FailDatabase.Add("TAP1_WTAP1_CONT1", 0, 0, 0, 0, 0, 45);
            alg.FailDatabase.Add("TAP1_WTAP1_CONT1", 0, 0, 0, 0, 0, 67);
            alg.FailDatabase.Add("TAP1_WTAP1_CONT1", 0, 0, 0, 1, 0, 45);
            alg.FailDatabase.Add("TAP1_WTAP1_CONT1", 0, 0, 0, 1, 2, 45);
            alg.FailDatabase.Add("TAP1_WTAP1_CONT1", 0, 0, 1, 17, 0, 3);
            alg.FailDatabase.Add("TAP1_WTAP1_CONT1", 0, 1, 0, 1, 0, 45);
            alg.FailDatabase.Add("TAP1_WTAP1_CONT1", 1, 0, 0, 1, 0, 45);
            alg.FailDatabase.Add("TAP1_WTAP1_CONT2", 0, 0, 0, 0, 0, 9);

            alg.WriteRasterLog("fakeTest");

            var expected = string.Empty;
            expected += $"DUT 0,1\n";
            expected += $"Test: mbist2_fakeTest\n";
            expected += $"Array: TAP1.WTAP1.CONT1.STEP0.MEM0\n";
            expected += $"0,0,0,45\n";
            expected += $"0,0,0,67\n";
            expected += $"0,1,0,45\n";
            expected += $"0,1,2,45\n";
            expected += $"1,17,0,3\n";

            expected += $"Array: TAP1.WTAP1.CONT1.STEP0.MEM1\n";
            expected += $"0,1,0,45\n";

            expected += $"DUT 0,1\n";
            expected += $"Test: mbist2_fakeTest\n";
            expected += $"Array: TAP1.WTAP1.CONT1.STEP1.MEM0\n";
            expected += $"0,1,0,45\n";

            expected += $"DUT 0,1\n";
            expected += $"Test: mbist2_fakeTest\n";
            expected += $"Array: TAP1.WTAP1.CONT2.STEP0.MEM0\n";
            expected += $"0,0,0,9\n";

            Assert.AreEqual(expected, string.Join("\n", this.TFileOutput));

            /* var tfileOutput = this.ConsoleOutput.Where(msg => msg.StartsWith("[TFILE]"));
            var tFileOutputStr = string.Join("\n", tfileOutput).Replace("[TFILE]", string.Empty);

            Assert.AreEqual(expected, tFileOutputStr); */
        }

        /// <summary>Unit test for MBIST Raster.</summary>
        [TestMethod]
        public void UpdateHRY()
        {
            // create a fake decoder with hry information
            var decoder = new MbistRasterInput.BitRanges();
            decoder.GoIDs = new List<MbistRasterInput.BitRanges.Memory>
            {
                new MbistRasterInput.BitRanges.Memory(0, 0, "15"),
                new MbistRasterInput.BitRanges.Memory(1, 1, "16"),
                new MbistRasterInput.BitRanges.Memory(2, 2, "17"),
            };

            var rasterConfig = new MbistRasterInput();
            rasterConfig.CaptureDecoders = new Dictionary<string, Dictionary<string, MbistRasterInput.BitRanges>>();
            rasterConfig.CaptureDecoders["TAP1_WTAP1_CONT1"] = new Dictionary<string, MbistRasterInput.BitRanges>();
            rasterConfig.CaptureDecoders["TAP1_WTAP1_CONT1"]["TAP1_WTAP1_CONT1"] = decoder;

            MbistRasterInput.PList plist = new MbistRasterInput.PList();
            plist.CaptureGroups = new List<string> { "TAP1_WTAP1_CONT1" };

            // create the raster data
            var alg = new MbistRasterAlgorithm(rasterConfig, repairMode: false);
            var controllers = new List<string> { "TAP1_WTAP1_CONT1" };

            alg.Initialize(mafFlag: true, contFail: false);
            Assert.AreEqual("HHHUU", alg.UpdateHRY(plist, "UUUUU"), "Expecting MAF");

            alg.Initialize(mafFlag: false, contFail: true);
            Assert.AreEqual("888UU", alg.UpdateHRY(plist, "UUUUU"), "Expecting ControllerStatusFailure");

            alg = new MbistRasterAlgorithm(rasterConfig, repairMode: true);
            alg.Initialize(mafFlag: false, contFail: false, repaired: false);
            alg.FailDatabase.Add("TAP1_WTAP1_CONT1", 0, 0, 0, 0, 0, 45);
            alg.FailDatabase.Add("TAP1_WTAP1_CONT1", 0, 0, 0, 1, 0, 67);
            alg.FailDatabase.Add("TAP1_WTAP1_CONT1", 0, 0, 0, 2, 0, 45);
            alg.FailDatabase.Add("TAP1_WTAP1_CONT1", 0, 0, 1, 3, 2, 45);
            alg.FailDatabase.Add("TAP1_WTAP1_CONT1", 0, 0, 1, 17, 0, 3);
            alg.FailDatabase.Add("TAP1_WTAP1_CONT1", 0, 1, 0, 1, 0, 45);

            Assert.AreEqual("KK1UU", alg.UpdateHRY(plist, "111UU"));
            Assert.AreEqual("EDJUU", alg.UpdateHRY(plist, "000UU"));
        }

        /// <summary>Unit test for MBIST Raster.</summary>
        [TestMethod]
        public void DecodeSingleControllerCapture()
        {
            var decoder = new MbistRasterInput.BitRanges();
            decoder.Status = "0,1,2";
            decoder.ErrorCnt = "3-4";
            decoder.LoopCounter = "5-8";
            decoder.AddrZ = "9";
            decoder.AddrX = "10-12";
            decoder.AddrY = "13-15";
            decoder.Instruction = "16-19";
            decoder.Algorithm = "20,21,22,23";
            decoder.GoIDs = new List<MbistRasterInput.BitRanges.Memory>
            {
                new MbistRasterInput.BitRanges.Memory(0, 0, "24-27"),
                new MbistRasterInput.BitRanges.Memory(1, 1, "28-31"),
                new MbistRasterInput.BitRanges.Memory(2, 2, "32-35"),
            };

            var alg = new MbistRasterAlgorithm(new MbistRasterInput());

            // DecodeSingleControllerCapture(string controllerName, int captureGroupNum, MbistRasterInput.BitRanges decoder, string ctvData)
            // RasterContainer.Add(controllerName, bin2dec(stepBin), memID, addrZ, addrX, addrY, io)

            // !            status   err     loop     Z      X       Y      instr    algo     mem0     mem1     mem2
            string ctvData = "000" + "00" + "0001" + "1" + "101" + "011" + "0101" + "0000" + "0001" + "0000" + "1111";
            alg.DecodeSingleControllerCapture("TAP1_WTAP1_CONT1", 0, decoder, ctvData);

            // check the raster container
            Assert.AreEqual(1, alg.FailDatabase.FailingRows("TAP1_WTAP1_CONT1", 0), "MEM0 Failing Rows");
            Assert.AreEqual(0, alg.FailDatabase.FailingRows("TAP1_WTAP1_CONT1", 1), "MEM1 Failing Rows");
            Assert.AreEqual(0, alg.FailDatabase.FailingRows("TAP1_WTAP1_CONT1", 2), "MEM2 Failing Rows");  // MAF

            var fail_mem0 = alg.FailDatabase.GetAllFailAddresses("TAP1_WTAP1_CONT1", 0, 0).ToList();

            Assert.AreEqual(1, fail_mem0.Count, "MEM0 Failing Addresses");
            Assert.AreEqual(1, fail_mem0[0].Bits.Count, "MEM0 Failing IOs (count)");
            Assert.AreEqual(3, fail_mem0[0].Bits[0], "MEM0 Failing IO");
            Assert.AreEqual(1, fail_mem0[0].Bank, "MEM0 Failing Bank");
            Assert.AreEqual(5, fail_mem0[0].Row, "MEM0 Failing Row");
            Assert.AreEqual(3, fail_mem0[0].Col, "MEM0 Failing Col");
            Assert.AreEqual("TAP1_WTAP1_CONT1", fail_mem0[0].Controller, "MEM0 Failing Controller");
            Assert.AreEqual(0, fail_mem0[0].Mem, "MEM0 Failing Mem");
            Assert.AreEqual(0, fail_mem0[0].Step, "MEM0 Failing Step");

            Assert.AreEqual(true, alg.MafFlag, "MAF");
            Assert.AreEqual(false, alg.ControllerStatusFailure, "Controller Status Failure Flag");

            // check the fafi output too
        }

        /// <summary>Unit test for MBIST Raster.</summary>
        [TestMethod]
        public void RasterContainer_FailingRows()
        {
            var alg = new MbistRasterAlgorithm(new MbistRasterInput());

            alg.FailDatabase.Add("ctrl1", 0, 0, 0, 0, 0, 45);
            alg.FailDatabase.Add("ctrl1", 0, 0, 0, 0, 0, 67);
            alg.FailDatabase.Add("ctrl1", 0, 0, 0, 1, 0, 45);
            alg.FailDatabase.Add("ctrl1", 0, 1, 0, 1, 0, 45);
            alg.FailDatabase.Add("ctrl1", 0, 0, 0, 1, 2, 45);
            alg.FailDatabase.Add("ctrl1", 0, 0, 1, 17, 0, 3);
            alg.FailDatabase.Add("ctrl2", 0, 0, 0, 0, 0, 9);

            Assert.AreEqual(3, alg.FailDatabase.FailingRows("ctrl1", 0));
            Assert.AreEqual(1, alg.FailDatabase.FailingRows("ctrl2", 0));
            Assert.AreEqual(0, alg.FailDatabase.FailingRows("ctrl3", 0));
        }

        /// <summary>Unit test for MBIST Raster.</summary>
        [TestMethod]
        public void GetControllerSizeStartAt0()
        {
            var controller = new MbistRasterInput.BitRanges();
            controller.Status = "0,1,2";
            controller.Step = MbistRasterInput.BitRanges.NA;
            controller.ErrorCnt = "4-19";
            controller.LoopCounter = "150-151";
            controller.AddrZ = "152";
            controller.AddrX = "153-160";
            controller.AddrY = "161-162";
            controller.Instruction = "163-167";
            controller.Algorithm = "168-174";
            controller.GoIDs = new List<MbistRasterInput.BitRanges.Memory>
            {
                new MbistRasterInput.BitRanges.Memory(1, 0, "20-45"),
                new MbistRasterInput.BitRanges.Memory(2, 1, "46-71"),
                new MbistRasterInput.BitRanges.Memory(3, 2, "72-110"),
                new MbistRasterInput.BitRanges.Memory(4, 2, "111-149"),
            };

            Assert.AreEqual(175, controller.Size());
        }

        /// <summary>Unit test for MBIST Raster.</summary>
        [TestMethod]
        public void GetControllerSizeStartAt4()
        {
            var controller = new MbistRasterInput.BitRanges();
            controller.Status = "175,176,177";
            controller.Step = MbistRasterInput.BitRanges.NA;
            controller.ErrorCnt = "4-19";
            controller.LoopCounter = "150-151";
            controller.AddrZ = "152";
            controller.AddrX = "153-160";
            controller.AddrY = "161-162";
            controller.Instruction = "163-167";
            controller.Algorithm = "168-174";
            controller.GoIDs = new List<MbistRasterInput.BitRanges.Memory>
            {
                new MbistRasterInput.BitRanges.Memory(1, 0, "20-45"),
                new MbistRasterInput.BitRanges.Memory(2, 1, "46-71"),
                new MbistRasterInput.BitRanges.Memory(3, 2, "72-110"),
                new MbistRasterInput.BitRanges.Memory(4, 2, "111-149"),
            };

            Assert.AreEqual(174, controller.Size());
        }

        /// <summary>Unit test for MBIST Raster.</summary>
        [TestMethod]
        public void Struct2JsonRasterOnly()
        {
            var decoder = new MbistRasterInput.BitRanges();
            decoder.Status = "0,1,2";
            decoder.ErrorCnt = "3-4";
            decoder.LoopCounter = "5-8";
            decoder.AddrZ = "9";
            decoder.AddrX = "10-12";
            decoder.AddrY = "13-15";
            decoder.Instruction = "16-19";
            decoder.Algorithm = "20,21,22,23";
            decoder.GoIDs = new List<MbistRasterInput.BitRanges.Memory>
            {
                new MbistRasterInput.BitRanges.Memory(1, 0, "24-27"),
                new MbistRasterInput.BitRanges.Memory(2, 1, "28-31"),
                new MbistRasterInput.BitRanges.Memory(3, 2, "32-35"),
            };

            var plist = new MbistRasterInput.PList();
            plist.CapturePins = "TDO";
            plist.CaptureGroups = new List<string> { "TAP1_WTAP1_CONT1_Group" };
            var rasterConfig = new MbistRasterInput();

            rasterConfig.PLists = new Dictionary<string, MbistRasterInput.PList>();
            rasterConfig.PLists["arr_mbist_fake_list"] = plist;

            rasterConfig.CaptureDecoders = new Dictionary<string, Dictionary<string, MbistRasterInput.BitRanges>>();
            rasterConfig.CaptureDecoders["TAP1_WTAP1_CONT1_Group"] = new Dictionary<string, MbistRasterInput.BitRanges>();
            rasterConfig.CaptureDecoders["TAP1_WTAP1_CONT1_Group"]["TAP1_WTAP1_CONT1"] = decoder;

            rasterConfig.HryFullMemList = new List<string> { "TAP1_WTAP1_CONT1_MEM1", "TAP1_WTAP1_CONT1_MEM2", "TAP1_WTAP1_CONT1_MEM3", };

            var alg = new MbistRasterAlgorithm(rasterConfig);

            // var expectedJson = "{\"Version\":1.0,\"HryLength\":57,\"PLists\":{\"arr_mbist_fake_list\":{\"CapturePins\":\"TDO\",\"CaptureInterLeaveMode\":\"CycleFirst\",\"Captures\":[\"TAP1_WTAP1_CONT1\"]}},\"CaptureGroups\":{\"TAP1_WTAP1_CONT1_Group\":{\"TAP1_WTAP1_CONT1\":{\"STATUS\":\"0,1,2\",\"STEP\":\"NA\",\"ERROR_CNT\":\"3-4\",\"GOID\":[{\"MEM\":1,\"HRYINDEX\":0,\"BITS\":\"24-27\"},{\"MEM\":2,\"HRYINDEX\":1,\"BITS\":\"28-31\"},{\"MEM\":3,\"HRYINDEX\":2,\"BITS\":\"32-35\"}],\"LOOP_COUNTER\":\"5-8\",\"ADDR_Z\":\"9\",\"ADDR_X\":\"10-12\",\"ADDR_Y\":\"13-15\",\"INSTRUCTION\":\"16-19\",\"ALGO_SEL\":\"20,21,22,23\"}}},\"REPAIRMAP\":null,\"REPAIRGROUPS\":null}";
            var expectedJson = @"{""Version"":1.0,""Hry_string"":[""TAP1_WTAP1_CONT1_MEM1"",""TAP1_WTAP1_CONT1_MEM2"",""TAP1_WTAP1_CONT1_MEM3""],""PLists"":{""arr_mbist_fake_list"":{""CapturePins"":""TDO"",""CaptureInterLeaveMode"":""CycleFirst"",""Captures"":[""TAP1_WTAP1_CONT1_Group""]}},""CaptureGroups"":{""TAP1_WTAP1_CONT1_Group"":{""TAP1_WTAP1_CONT1"":{""STATUS"":""0,1,2"",""STEP"":""NA"",""ERROR_CNT"":""3-4"",""GOID"":[{""MEM"":1,""BITS"":""24-27""},{""MEM"":2,""BITS"":""28-31""},{""MEM"":3,""BITS"":""32-35""}],""LOOP_COUNTER"":""5-8"",""ADDR_Z"":""9"",""ADDR_X"":""10-12"",""ADDR_Y"":""13-15"",""INSTRUCTION"":""16-19"",""ALGO_SEL"":""20,21,22,23""}}},""REPAIRMAP"":null,""REPAIRGROUPS"":null}";
            var jsonTxt = JsonConvert.SerializeObject(rasterConfig);
            Console.WriteLine(jsonTxt);

            Assert.AreEqual(expectedJson, jsonTxt, "Failed Struct to JSON conversion");

            // reconvert it to make sure we get the original. (FIXME - build a comparator)
            var rebuiltStruct = JsonConvert.DeserializeObject<MbistRasterInput>(jsonTxt);

            var jsonTxtRebuilt = JsonConvert.SerializeObject(rebuiltStruct);
            Console.WriteLine(jsonTxtRebuilt);

            Assert.AreEqual(expectedJson, jsonTxtRebuilt, "Failed Struct to JSON reconversion");
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void Validate_Fail_MemNotInFullList()
        {
            var inputJson = @"{""Version"":1.0,""Hry_string"":[""TAP1_WTAP1_CONT1_MEM1"",""TAP1_WTAP1_CONT1_MEM2"",""TAP1_WTAP1_CONT1_MEM3""],""PLists"":{""arr_mbist_fake_list"":{""CapturePins"":""TDO"",""CaptureInterLeaveMode"":""CycleFirst"",""Captures"":[""TAP1_WTAP1_CONT1_Group""]}},""CaptureGroups"":{""TAP1_WTAP1_CONT1_Group"":{""TAP1_WTAP1_CONT1"":{""STATUS"":""0,1,2"",""STEP"":""NA"",""ERROR_CNT"":""3-4"",""GOID"":[{""MEM"":12,""BITS"":""24-27""},{""MEM"":2,""BITS"":""28-31""},{""MEM"":3,""BITS"":""32-35""}],""LOOP_COUNTER"":""5-8"",""ADDR_Z"":""9"",""ADDR_X"":""10-12"",""ADDR_Y"":""13-15"",""INSTRUCTION"":""16-19"",""ALGO_SEL"":""20,21,22,23""}}},""REPAIRMAP"":null,""REPAIRGROUPS"":null}";
            var inputObj = JsonConvert.DeserializeObject<MbistRasterInput>(inputJson);
            var errors = inputObj.Validate();

            Assert.IsTrue(errors.Count == 1, $"Raster Input file had [{errors.Count}] expecting [1]");
            Assert.AreEqual("Memory=[TAP1_WTAP1_CONT1_MEM12] in CaptureGroup=[TAP1_WTAP1_CONT1_Group]/PList=[arr_mbist_fake_list] does not have an entry in Json Element [Hry_string].", errors[0]);
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void Validate_Pass()
        {
            var inputJson = @"{""Version"":1.0,""Hry_string"":[""TAP1_WTAP1_CONT1_MEM1"",""TAP1_WTAP1_CONT1_MEM2"",""TAP1_WTAP1_CONT1_MEM3""],""PLists"":{""arr_mbist_fake_list"":{""CapturePins"":""TDO"",""CaptureInterLeaveMode"":""CycleFirst"",""Captures"":[""TAP1_WTAP1_CONT1_Group""]}},""CaptureGroups"":{""TAP1_WTAP1_CONT1_Group"":{""TAP1_WTAP1_CONT1"":{""STATUS"":""0,1,2"",""STEP"":""NA"",""ERROR_CNT"":""3-4"",""GOID"":[{""MEM"":1,""BITS"":""24-27""},{""MEM"":2,""BITS"":""28-31""},{""MEM"":3,""BITS"":""32-35""}],""LOOP_COUNTER"":""5-8"",""ADDR_Z"":""9"",""ADDR_X"":""10-12"",""ADDR_Y"":""13-15"",""INSTRUCTION"":""16-19"",""ALGO_SEL"":""20,21,22,23""}}},""REPAIRMAP"":null,""REPAIRGROUPS"":null}";
            var inputObj = JsonConvert.DeserializeObject<MbistRasterInput>(inputJson);
            var errors = inputObj.Validate();

            Assert.IsTrue(errors.Count == 0, $"Raster Input file had [{errors.Count}] expecting [0]");
            Assert.AreEqual(3, inputObj.HryLength, "HRY Length failure");
            Assert.AreEqual(0, inputObj.CaptureDecoders["TAP1_WTAP1_CONT1_Group"]["TAP1_WTAP1_CONT1"].GoIDs[0].HryIndex, "Failed on HryIndex for Mem1");
            Assert.AreEqual(1, inputObj.CaptureDecoders["TAP1_WTAP1_CONT1_Group"]["TAP1_WTAP1_CONT1"].GoIDs[1].HryIndex, "Failed on HryIndex for Mem2");
            Assert.AreEqual(2, inputObj.CaptureDecoders["TAP1_WTAP1_CONT1_Group"]["TAP1_WTAP1_CONT1"].GoIDs[2].HryIndex, "Failed on HryIndex for Mem3");
        }

        /// <summary>Full test of Raster/Repair with a failure in 2 rows both in the same column.</summary>
        [TestMethod]
        public void MbistRepair_2defectColRepair()
        {
            /* This CTV data represents 2 failures
                [TFILE]Array: tim1.BP0.WBP1.STEP0.MEM6
                [TFILE]0,63,2,7
                [TFILE]0,63,1,7
             * This is expected to be fixed with a single io repair.
             */
            var pin = "TDO";
            var ctv = new Dictionary<string, string> { { pin, "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001000000000000000000000000000000000000111111100010001100110000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000010000000000000000000000000000000000001111111000100011001100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000000000000000000000000100011111101001010110011000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001000000000000000000000000000000010000111111100010001100110000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000010000000000000000000000000000000100001111110100100011001100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000000000000000000000001100011111101001010110011001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001100110110000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000011001101100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000110011011000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001100110110000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000011001101100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000110011011000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001100110110000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000011001101100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000110011011000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001100110110000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000011001101100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000110011011000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001100110110000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000011001101100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000110011" } };
            var fakeTC = "FakeTC";
            var plist = "IP_CPU::arr_mbist_x_x_tap_all_rasterautoinc_ssa_tim1_all_regularallsteps_list";
            var cfgFile = GetPathToFiles() + "mbist_raster_sds13R.json";
            var initialFuseValue = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
            var finalFuseValue = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX000111";

            var uservarsMock = new Mock<IUserVarService>(MockBehavior.Strict);
            uservarsMock.Setup(o => o.Exists("SCVars", "SC_WAFERX")).Returns(true);
            uservarsMock.Setup(o => o.Exists("SCVars", "SC_WAFERY")).Returns(true);
            uservarsMock.Setup(o => o.Exists("TestTimeLog", "PrimeUserCode")).Returns(false);
            uservarsMock.Setup(o => o.GetStringValue("SCVars", "SC_WAFERX")).Returns("1");
            uservarsMock.Setup(o => o.GetStringValue("SCVars", "SC_WAFERY")).Returns("2");
            Prime.Services.UserVarService = uservarsMock.Object;

            var evgMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            evgMock.Setup(o => o.GetStringRowFromTable("STBTF2R", Context.DUT)).Returns(initialFuseValue);
            evgMock.Setup(o => o.InsertRowAtTable("STBTF2R", finalFuseValue, Context.DUT));
            evgMock.Setup(o => o.GetStringRowFromTable("HRY_RAWSTR_MBIST", Context.DUT)).Returns("UUUUUUUUUUUUUUUUUUUUUUUUUUUU11UU11111111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU111111UUUUUUUUUUUUUUUUUUUUUU111111UUUUUUUUUUUUUUUUUU11111111UUUUUUUUUUUUUUUUUU111111111U1U11UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU1111111111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU1UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU11U11UU1UUUUUUUUUUU1UUUUUUUUUUUUUUUUUUUUU1U1111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU11UU111111UUUUUUUUUU1111111111111111U111111U111110UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU");
            evgMock.Setup(o => o.InsertRowAtTable("HRY_RAWSTR_MBIST", "UUUUUUUUUUUUUUUUUUUUUUUUUUUU11UU11111111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU111111UUUUUUUUUUUUUUUUUUUUUU111111UUUUUUUUUUUUUUUUUU11111111UUUUUUUUUUUUUUUUUU111111111U1U11UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU1111111111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU1UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU11U11UU1UUUUUUUUUUU1UUUUUUUUUUUUUUUUUUUUU1U1111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU11UU111111UUUUUUUUUU1111111111111111U111111U11111YUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", Context.DUT));
            Prime.Services.SharedStorageService = evgMock.Object;

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.FileExists(cfgFile)).Returns(true);
            fileServiceMock.Setup(o => o.GetFile(cfgFile)).Returns(cfgFile);
            Prime.Services.FileService = fileServiceMock.Object;

            var funcTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Execute()).Returns(false);
            funcTestMock.Setup(o => o.GetCtvData()).Returns(ctv);
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureCtvPerPinTest(plist, fakeTC, fakeTC, new List<string> { pin }, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var template = new MbistRasterRepairTC
            {
                EnableRepair = MbistRasterRepairTC.MyBoolean.TRUE,
                EnableFAFI = MbistRasterRepairTC.MyBoolean.TRUE,
                RasterInputFile = cfgFile,

                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                InstanceName = "FakeInstance",
                Patlist = plist,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                CtvCapturePins = pin,
            };

            template.TestMethodExtension = template; // without this the this.TestMethodExtension.CustomPostProcessCtvData(ctvData) call fails.

            template.Verify();
            template.CustomVerify();

            Assert.AreEqual(1, template.Execute(), "Failed Execute.");
            evgMock.Verify(o => o.InsertRowAtTable("STBTF2R", finalFuseValue, Context.DUT), Times.Once);

            // check the TFile output
            var expected = string.Empty;
            expected += $"DUT 1,2\n";
            expected += $"Test: mbist2_FakeInstance\n";
            expected += $"Array: tim1.BP0.WBP1.STEP0.MEM6\n";
            expected += $"0,63,2,7\n";
            expected += $"0,63,1,7\n";
            Assert.AreEqual(expected, string.Join("\n", this.TFileOutput), "Failed TFile comparison.");
        }

        private static string GetPathToFiles([CallerFilePath] string srcPath = "")
        {
            return Path.GetDirectoryName(srcPath) + "\\InputFiles\\";
        }
    }
}
