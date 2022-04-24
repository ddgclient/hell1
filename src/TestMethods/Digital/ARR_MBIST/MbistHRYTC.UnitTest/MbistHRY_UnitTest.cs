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
    using System.Runtime.CompilerServices;
    using ARR_MBIST;
    using MbistHRYTC;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.SharedStorageService;
    using Prime.TestMethods;

    /// <summary>Unit test class for HRY.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1025:Code should not contain multiple whitespace in a row", Justification = "Makes Tables more readable")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1001:Commas should not be preceded by whitespace", Justification = "Makes Tables more readable")]
    [TestClass]
    public class MbistHRY_UnitTest
    {
        private Mock<IConsoleService> consoleServiceMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="MbistHRY_UnitTest"/> class.
        /// </summary>
        public MbistHRY_UnitTest()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => { Console.WriteLine(s); });
            this.consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => { Console.WriteLine($"ERROR: {msg}"); });
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            Console.WriteLine("Done with constructor");
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void Validate_Fail_MemNotInFullList()
        {
            var inputJson = "{\"LookupTables\":{\"PList1\":{\"CapturePins\":\"TDO\",\"CaptureCount\":12,\"Controllers\":[{\"Name\":\"Controller1\",\"Execution\":[{\"STATUS\":[0,1,2,3],\"ALGO_SEL\":[4,5],\"Memories\":[{\"MEM\":1,\"GOID\":[6,7,8]},{\"MEM\":12,\"GOID\":[9,10,11]}]}]}]}},\"Version\":1.1,\"Hry_string\":[\"Controller1_MEM1\",\"Controller1_MEM2\",\"Controller1_MEM3\",\"Controller1_MEM4\",\"Controller1_MEM5\",\"Controller1_MEM6\",\"Controller1_MEM7\",\"Controller1_MEM8\",\"Controller1_MEM9\",\"Controller1_MEM10\"]}";
            var inputObj = JsonConvert.DeserializeObject<MbistHRYInput>(inputJson);
            var errors = inputObj.Validate();

            Assert.IsTrue(errors.Count == 1, $"HRY Input file had [{errors.Count}] expecting [1]");
            Assert.AreEqual("Memory=[Controller1_MEM12] in PList=[PList1] does not have an entry in Json Element [Hry_string].", errors[0]);
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void GenerateHrystring()
        {
            var inputJson = "{\"LookupTables\":{\"PList1\":{\"CapturePins\":\"TDO\",\"CaptureCount\":12,\"Controllers\":[{\"Name\":\"Controller1\",\"Execution\":[{\"STATUS\":[0,1,2,3],\"ALGO_SEL\":[4,5],\"Memories\":[{\"MEM\":1,\"GOID\":[6,7,8]},{\"MEM\":2,\"GOID\":[9,10,11]}]}]}]}},\"Version\":1.1,\"Hry_string\":[\"Controller1_MEM1\",\"Controller1_MEM2\",\"Controller1_MEM3\",\"Controller1_MEM4\",\"Controller1_MEM5\",\"Controller1_MEM6\",\"Controller1_MEM7\",\"Controller1_MEM8\",\"Controller1_MEM9\",\"Controller1_MEM10\"]}";
            var inputObj = JsonConvert.DeserializeObject<MbistHRYInput>(inputJson);
            var errors = inputObj.Validate();
            Assert.IsTrue(errors.Count == 0, $"HRY Input file had errors\n{string.Join("\n", errors)}");

            var hryAlg = new MbistHRYAlgorithm();

            // !          status    alg    mem1    mem2
            // !                                    11
            // !           0123     45     678     901
            var ctvData = "0000" + "00" + "000" + "000";
            var hryExpect = "11UUUUUUUU";
            var hryDataStr = hryAlg.GenerateHRY(inputObj.LookupTables["PList1"], ctvData, inputObj.HryLength, false);
            Assert.AreEqual(hryExpect, hryDataStr);
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void Struct2Json()
        {
            // create a raw input struct.
            var hryInput = new MbistHRYInput();
            hryInput.Version = 1.1;
            hryInput.HryLength = 10;
            hryInput.HryFullMemList = new List<string> { "Controller1_MEM1", "Controller1_MEM2", "Controller1_MEM3", "Controller1_MEM4", "Controller1_MEM5", "Controller1_MEM6", "Controller1_MEM7", "Controller1_MEM8", "Controller1_MEM9", "Controller1_MEM10", };
            hryInput.LookupTables = new Dictionary<string, MbistHRYInput.MbistLookupTable>();

            hryInput.LookupTables["PList1"] = new MbistHRYInput.MbistLookupTable();
            hryInput.LookupTables["PList1"].CapturePins = "TDO";
            hryInput.LookupTables["PList1"].CaptureCount = 20;
            hryInput.LookupTables["PList1"].CaptureInterLeaveMode = Mbist.CaptureInterLeaveType.CycleFirst;

            hryInput.LookupTables["PList1"].Controllers = new List<MbistHRYInput.MbistLookupTable.Controller>();

            hryInput.LookupTables["PList1"].Controllers.Add(new MbistHRYInput.MbistLookupTable.Controller());
            hryInput.LookupTables["PList1"].Controllers[0].Name = "Controller1";

            hryInput.LookupTables["PList1"].Controllers[0].Groups = new List<MbistHRYInput.MbistLookupTable.Controller.AlgorithmGroup>();
            hryInput.LookupTables["PList1"].Controllers[0].Groups.Add(new MbistHRYInput.MbistLookupTable.Controller.AlgorithmGroup());
            hryInput.LookupTables["PList1"].Controllers[0].Groups[0].StatusBits = new List<int>() { 0, 1, 2 };
            hryInput.LookupTables["PList1"].Controllers[0].Groups[0].AlgorithmSelectBits = new List<int>() { 4, 5, 6 };

            hryInput.LookupTables["PList1"].Controllers[0].Groups[0].Memories = new List<MbistHRYInput.MbistLookupTable.Controller.AlgorithmGroup.Memory>();

            hryInput.LookupTables["PList1"].Controllers[0].Groups[0].Memories.Add(new MbistHRYInput.MbistLookupTable.Controller.AlgorithmGroup.Memory());
            hryInput.LookupTables["PList1"].Controllers[0].Groups[0].Memories[0].MemoryId = 1;
            hryInput.LookupTables["PList1"].Controllers[0].Groups[0].Memories[0].HryIndex = 0;
            hryInput.LookupTables["PList1"].Controllers[0].Groups[0].Memories[0].GoIDBits = new List<int>() { 7, 8, 9 };

            hryInput.LookupTables["PList1"].Controllers[0].Groups[0].Memories.Add(new MbistHRYInput.MbistLookupTable.Controller.AlgorithmGroup.Memory());
            hryInput.LookupTables["PList1"].Controllers[0].Groups[0].Memories[1].MemoryId = 2;
            hryInput.LookupTables["PList1"].Controllers[0].Groups[0].Memories[1].HryIndex = 1;
            hryInput.LookupTables["PList1"].Controllers[0].Groups[0].Memories[1].GoIDBits = new List<int>() { 10, 11, 12 };

            // json string for the above strct
            // var expectedJson = "{\"Version\":1.1,\"HryLength\":10,\"LookupTables\":{\"PList1\":{\"CapturePins\":\"TDO\",\"CaptureCount\":20,\"CaptureInterLeaveMode\":\"CycleFirst\",\"Controllers\":[{\"Name\":\"Controller1\",\"Groups\":[{\"StatusBits\":[0,1,2],\"AlgorithmSelectBits\":[4,5,6],\"Memories\":[{\"MemoryId\":1,\"HryIndex\":0,\"GoIDBits\":[7,8,9]},{\"MemoryId\":2,\"HryIndex\":1,\"GoIDBits\":[10,11,12]}]}]}]}}}";
            var expectedJson = "{\"Version\":1.1,\"Hry_string\":[\"Controller1_MEM1\",\"Controller1_MEM2\",\"Controller1_MEM3\",\"Controller1_MEM4\",\"Controller1_MEM5\",\"Controller1_MEM6\",\"Controller1_MEM7\",\"Controller1_MEM8\",\"Controller1_MEM9\",\"Controller1_MEM10\"],\"LookupTables\":{\"PList1\":{\"CapturePins\":\"TDO\",\"CaptureCount\":20,\"CaptureInterLeaveMode\":\"CycleFirst\",\"Controllers\":[{\"Name\":\"Controller1\",\"Execution\":[{\"STATUS\":[0,1,2],\"ALGO_SEL\":[4,5,6],\"Memories\":[{\"MEM\":1,\"GOID\":[7,8,9]},{\"MEM\":2,\"GOID\":[10,11,12]}]}]}]}}}";

            // serialize it
            var jsonTxt = JsonConvert.SerializeObject(hryInput, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Console.WriteLine(jsonTxt);

            Assert.AreEqual(expectedJson, jsonTxt, "Failed Struct to JSON conversion");

            // reconvert it to make sure we get the original. (FIXME - build a comparator)
            var rebuiltStruct = JsonConvert.DeserializeObject<MbistHRYInput>(jsonTxt);

            // Assert.AreEqual(hryInput, rebuiltStruct, "Failed JSON to Struct conversion");
            var jsonTxtRebuilt = JsonConvert.SerializeObject(rebuiltStruct, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Console.WriteLine(jsonTxtRebuilt);

            Assert.AreEqual(expectedJson, jsonTxtRebuilt, "Failed Struct to JSON reconversion");
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void SerializeCaptureData_SinglePinByPin()
        {
            Dictionary<string, string> ctvData = new Dictionary<string, string>();
            ctvData["PIN1"] = "000011110000111100001111";

            var serialData = Mbist.SerializeCaptureData(ctvData, new List<string>() { "PIN1" }, Mbist.CaptureInterLeaveType.PinFirst);
            Assert.AreEqual(ctvData["PIN1"], serialData);
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void SerializeCaptureData_SinglePinByVec()
        {
            Dictionary<string, string> ctvData = new Dictionary<string, string>();
            ctvData["PIN1"] = "1100";

            var serialData = Mbist.SerializeCaptureData(ctvData, new List<string>() { "PIN1" }, Mbist.CaptureInterLeaveType.CycleFirst);
            Assert.AreEqual(ctvData["PIN1"], serialData);
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void SerializeCaptureData_MultiPinByPin()
        {
            Dictionary<string, string> ctvData = new Dictionary<string, string>();
            ctvData["PIN1"] = "000011110000111100001111";
            ctvData["PIN2"] = "111111111111111111111111";
            ctvData["PIN3"] = "000000000000000000000000";
            var expectedData = ctvData["PIN1"] + ctvData["PIN3"] + ctvData["PIN2"];

            var serialData = Mbist.SerializeCaptureData(ctvData, new List<string>() { "PIN1", "PIN3", "PIN2" }, Mbist.CaptureInterLeaveType.PinFirst);
            Assert.AreEqual(expectedData, serialData);
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void SerializeCaptureData_MultiPinByVec()
        {
            Dictionary<string, string> ctvData = new Dictionary<string, string>();
            ctvData["PIN1"] = "000011110000111100001111";
            ctvData["PIN2"] = "111111111111111111111111";
            ctvData["PIN3"] = "000000000000000000000000";
            var expectedData = "010010010010110110110110010010010010110110110110010010010010110110110110";

            var serialData = Mbist.SerializeCaptureData(ctvData, new List<string>() { "PIN1", "PIN2", "PIN3" }, Mbist.CaptureInterLeaveType.CycleFirst);
            Assert.AreEqual(expectedData, serialData);
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void CalculateExitPort_All()
        {
            var hryAlg = new MbistHRYAlgorithm();
            var teststrings = new Dictionary<int, List<string>>()
            {
                {
                    3,
                    new List<string>()
                    {
                        "UUP00F00PPPPPPNPP00000UUUUUYYYYPIPPPPUUU", // I=pattern_issue
                        "UUP00F00PPPI8PNPP00000UUUUUYYYYPPPPPPUUU", // I & 8
                        "UUP00F00P888PPNPP00000UUUUUYYYYPPPPPPUUU", // 8=cont_fail
                        "UUP00F00PPPPPPNPP06000UUUUUYYYYPPPPPPUUU", // 6=inconsist_ps_fg
                        "UUP00F05PPPPPPNPP00000UUUUUYYYYPPPPPPUUU", // 5=inconsist_fs_pg
                        "UUP00F00PPPPPPNPP00000UUUUUYYYYPPPPPPUU7", // 7=inconsist_pst_fail
                        "IUP00F00PPPPPPNPP00000UUUUUYYYYPPPPPPUU7", // I & 7
                    }
                },
                {
                    2,
                    new List<string>()
                    {
                        "000000000000000000000000000", // 0=fail
                        "UUUUUUUU0UUU1111YYYYPPPPPUU", // 0=fail
                        "UUUUUUUUUUUU1111YYYYPPNPPUU", // N=unrepairable
                        "UUUUFUUUUUUU1111YYYYPPPPPUU", // F=fail_retest
                    }
                },
                {
                    1,
                    new List<string>()
                    {
                        "UUUUUUUUUUUUUUUUUUUUUUUUUU", // U=untested
                        "11111111111111111111111111", // 1=pass
                        "UUUUUUUUUUUUUUU1UUUUUUUUUU", // U & 1
                    }
                },
                {
                    0,
                    new List<string>()
                    {
                        "YYYYYYYYYYYYYYYYYYYYYYYYYY", // not valid, only raster/repair can set things as repairable
                    }
                },
            };
            var port1stringsWithRetest = new List<string>()
            {
                "PPPPPPPPPPPPPPPPPPPPPPPPPP", // P=pass_retest
            };
            var port0stringsNoRetest = new List<string>()
            {
                "PPPPPPPPPPPPPPPPPPPPPPPPPP", // P=pass_retest
            };

            int testsrun = 0;
            foreach (var retest in new List<bool>() { true, false })
            {
                foreach (var testpair in teststrings)
                {
                    foreach (var teststring in testpair.Value)
                    {
                        Assert.AreEqual(testpair.Key, hryAlg.CalculateExitPort(teststring, retest), $"Failed Port{testpair.Key} with retest={retest} test={teststring}");
                        testsrun++;
                    }
                }
            }

            foreach (var teststring in port1stringsWithRetest)
            {
                Assert.AreEqual(1, hryAlg.CalculateExitPort(teststring, true), $"Failed 'pass_retest' Port1 with retest={true} test={teststring}");
                testsrun++;
            }

            foreach (var teststring in port0stringsNoRetest)
            {
                Assert.AreEqual(0, hryAlg.CalculateExitPort(teststring, false), $"Failed 'pass_retest' Port0 with retest={false} test={teststring}");
                testsrun++;
            }

            Assert.IsTrue(testsrun > 0, "None of the tests ran");
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void MergeHry_char_All()
        {
            var hryAlg = new MbistHRYAlgorithm();

            // FIXME - Some of these cases are not possible (like the HRY algorithm never sets repairable/unrepairable).
            //         Should they cause exceptions?
            var testMatrix = new List<List<string>>();

            // !                               original                new                   expected             expected(retest)
            testMatrix.Add(new List<string> { "cont_fail", "cont_fail", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "cont_fail", "fail", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "cont_fail", "fail_retest", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "cont_fail", "inconsist_fs_pg", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "cont_fail", "inconsist_ps_fg", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "cont_fail", "inconsist_pst_fail", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "cont_fail", "pass", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "cont_fail", "pass_retest", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "cont_fail", "pattern_issue", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "cont_fail", "repairable", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "cont_fail", "unrepairable", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "cont_fail", "untested", "cont_fail", "cont_fail" });

            testMatrix.Add(new List<string> { "fail", "cont_fail", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "fail", "fail", "fail", "fail_retest" });
            testMatrix.Add(new List<string> { "fail", "fail_retest", "fail_retest", "fail_retest" });
            testMatrix.Add(new List<string> { "fail", "inconsist_fs_pg", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "fail", "inconsist_ps_fg", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "fail", "inconsist_pst_fail", "inconsist_pst_fail", "inconsist_pst_fail" });
            testMatrix.Add(new List<string> { "fail", "pass", "fail", "pass_retest" });
            testMatrix.Add(new List<string> { "fail", "pass_retest", "pass_retest", "pass_retest" });
            testMatrix.Add(new List<string> { "fail", "pattern_issue", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "fail", "repairable", "repairable", "repairable" });
            testMatrix.Add(new List<string> { "fail", "unrepairable", "unrepairable", "unrepairable" });
            testMatrix.Add(new List<string> { "fail", "untested", "fail", "fail" });

            testMatrix.Add(new List<string> { "fail_retest", "cont_fail", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "fail_retest", "fail", "fail", "fail" });
            testMatrix.Add(new List<string> { "fail_retest", "fail_retest", "fail_retest", "fail_retest" });
            testMatrix.Add(new List<string> { "fail_retest", "inconsist_fs_pg", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "fail_retest", "inconsist_ps_fg", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "fail_retest", "inconsist_pst_fail", "inconsist_pst_fail", "inconsist_pst_fail" });
            testMatrix.Add(new List<string> { "fail_retest", "pass", "pass", "pass" });
            testMatrix.Add(new List<string> { "fail_retest", "pass_retest", "pass_retest", "pass_retest" });
            testMatrix.Add(new List<string> { "fail_retest", "pattern_issue", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "fail_retest", "repairable", "repairable", "repairable" });
            testMatrix.Add(new List<string> { "fail_retest", "unrepairable", "unrepairable", "unrepairable" });
            testMatrix.Add(new List<string> { "fail_retest", "untested", "fail_retest", "fail_retest" });

            testMatrix.Add(new List<string> { "inconsist_fs_pg", "cont_fail", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "inconsist_fs_pg", "fail", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "inconsist_fs_pg", "fail_retest", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "inconsist_fs_pg", "inconsist_fs_pg", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "inconsist_fs_pg", "inconsist_ps_fg", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "inconsist_fs_pg", "inconsist_pst_fail", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "inconsist_fs_pg", "pass", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "inconsist_fs_pg", "pass_retest", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "inconsist_fs_pg", "pattern_issue", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "inconsist_fs_pg", "repairable", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "inconsist_fs_pg", "unrepairable", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "inconsist_fs_pg", "untested", "inconsist_fs_pg", "inconsist_fs_pg" });

            testMatrix.Add(new List<string> { "inconsist_ps_fg", "cont_fail", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "inconsist_ps_fg", "fail", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "inconsist_ps_fg", "fail_retest", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "inconsist_ps_fg", "inconsist_fs_pg", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "inconsist_ps_fg", "inconsist_ps_fg", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "inconsist_ps_fg", "inconsist_pst_fail", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "inconsist_ps_fg", "pass", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "inconsist_ps_fg", "pass_retest", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "inconsist_ps_fg", "pattern_issue", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "inconsist_ps_fg", "repairable", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "inconsist_ps_fg", "unrepairable", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "inconsist_ps_fg", "untested", "inconsist_ps_fg", "inconsist_ps_fg" });

            testMatrix.Add(new List<string> { "inconsist_pst_fail", "cont_fail", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "inconsist_pst_fail", "fail", "fail", "fail" });
            testMatrix.Add(new List<string> { "inconsist_pst_fail", "fail_retest", "fail_retest", "fail_retest" });
            testMatrix.Add(new List<string> { "inconsist_pst_fail", "inconsist_fs_pg", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "inconsist_pst_fail", "inconsist_ps_fg", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "inconsist_pst_fail", "inconsist_pst_fail", "inconsist_pst_fail", "inconsist_pst_fail" });
            testMatrix.Add(new List<string> { "inconsist_pst_fail", "pass", "pass", "pass" });
            testMatrix.Add(new List<string> { "inconsist_pst_fail", "pass_retest", "pass_retest", "pass_retest" });
            testMatrix.Add(new List<string> { "inconsist_pst_fail", "pattern_issue", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "inconsist_pst_fail", "repairable", "repairable", "repairable" });
            testMatrix.Add(new List<string> { "inconsist_pst_fail", "unrepairable", "unrepairable", "unrepairable" });
            testMatrix.Add(new List<string> { "inconsist_pst_fail", "untested", "inconsist_pst_fail", "inconsist_pst_fail" });

            testMatrix.Add(new List<string> { "pass", "cont_fail", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "pass", "fail", "inconsist_pst_fail", "inconsist_pst_fail" });
            testMatrix.Add(new List<string> { "pass", "fail_retest", "fail_retest", "fail_retest" });
            testMatrix.Add(new List<string> { "pass", "inconsist_fs_pg", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "pass", "inconsist_ps_fg", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "pass", "inconsist_pst_fail", "inconsist_pst_fail", "inconsist_pst_fail" });
            testMatrix.Add(new List<string> { "pass", "pass", "pass", "pass" });
            testMatrix.Add(new List<string> { "pass", "pass_retest", "pass_retest", "pass_retest" });
            testMatrix.Add(new List<string> { "pass", "pattern_issue", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "pass", "repairable", "repairable", "repairable" });
            testMatrix.Add(new List<string> { "pass", "unrepairable", "unrepairable", "unrepairable" });
            testMatrix.Add(new List<string> { "pass", "untested", "pass", "pass" });

            testMatrix.Add(new List<string> { "pass_retest", "cont_fail", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "pass_retest", "fail", "fail", "fail" });
            testMatrix.Add(new List<string> { "pass_retest", "fail_retest", "fail_retest", "fail_retest" });
            testMatrix.Add(new List<string> { "pass_retest", "inconsist_fs_pg", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "pass_retest", "inconsist_ps_fg", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "pass_retest", "inconsist_pst_fail", "inconsist_pst_fail", "inconsist_pst_fail" });
            testMatrix.Add(new List<string> { "pass_retest", "pass", "pass", "pass" });
            testMatrix.Add(new List<string> { "pass_retest", "pass_retest", "pass_retest", "pass_retest" });
            testMatrix.Add(new List<string> { "pass_retest", "pattern_issue", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "pass_retest", "repairable", "repairable", "repairable" });
            testMatrix.Add(new List<string> { "pass_retest", "unrepairable", "unrepairable", "unrepairable" });
            testMatrix.Add(new List<string> { "pass_retest", "untested", "pass_retest", "pass_retest" });

            testMatrix.Add(new List<string> { "pattern_issue", "cont_fail", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "pattern_issue", "fail", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "pattern_issue", "fail_retest", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "pattern_issue", "inconsist_fs_pg", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "pattern_issue", "inconsist_ps_fg", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "pattern_issue", "inconsist_pst_fail", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "pattern_issue", "pass", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "pattern_issue", "pass_retest", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "pattern_issue", "pattern_issue", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "pattern_issue", "repairable", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "pattern_issue", "unrepairable", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "pattern_issue", "untested", "pattern_issue", "pattern_issue" });

            testMatrix.Add(new List<string> { "repairable", "cont_fail", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "repairable", "fail", "fail", "fail_retest" });
            testMatrix.Add(new List<string> { "repairable", "fail_retest", "fail_retest", "fail_retest" });
            testMatrix.Add(new List<string> { "repairable", "inconsist_fs_pg", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "repairable", "inconsist_ps_fg", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "repairable", "inconsist_pst_fail", "inconsist_pst_fail", "inconsist_pst_fail" });
            testMatrix.Add(new List<string> { "repairable", "pass", "pass", "pass_retest" });
            testMatrix.Add(new List<string> { "repairable", "pass_retest", "pass_retest", "pass_retest" });
            testMatrix.Add(new List<string> { "repairable", "pattern_issue", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "repairable", "repairable", "repairable", "repairable" });
            testMatrix.Add(new List<string> { "repairable", "unrepairable", "unrepairable", "unrepairable" });
            testMatrix.Add(new List<string> { "repairable", "untested", "repairable", "repairable" });

            testMatrix.Add(new List<string> { "unrepairable", "cont_fail", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "unrepairable", "fail", "fail", "fail" });
            testMatrix.Add(new List<string> { "unrepairable", "fail_retest", "fail_retest", "fail_retest" });
            testMatrix.Add(new List<string> { "unrepairable", "inconsist_fs_pg", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "unrepairable", "inconsist_ps_fg", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "unrepairable", "inconsist_pst_fail", "inconsist_pst_fail", "inconsist_pst_fail" });
            testMatrix.Add(new List<string> { "unrepairable", "pass", "pass", "pass" });
            testMatrix.Add(new List<string> { "unrepairable", "pass_retest", "pass_retest", "pass_retest" });
            testMatrix.Add(new List<string> { "unrepairable", "pattern_issue", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "unrepairable", "repairable", "repairable", "repairable" });
            testMatrix.Add(new List<string> { "unrepairable", "unrepairable", "unrepairable", "unrepairable" });
            testMatrix.Add(new List<string> { "unrepairable", "untested", "unrepairable", "unrepairable" });

            testMatrix.Add(new List<string> { "untested", "cont_fail", "cont_fail", "cont_fail" });
            testMatrix.Add(new List<string> { "untested", "fail", "fail", "fail_retest" });
            testMatrix.Add(new List<string> { "untested", "fail_retest", "fail_retest", "fail_retest" });
            testMatrix.Add(new List<string> { "untested", "inconsist_fs_pg", "inconsist_fs_pg", "inconsist_fs_pg" });
            testMatrix.Add(new List<string> { "untested", "inconsist_ps_fg", "inconsist_ps_fg", "inconsist_ps_fg" });
            testMatrix.Add(new List<string> { "untested", "inconsist_pst_fail", "inconsist_pst_fail", "inconsist_pst_fail" });
            testMatrix.Add(new List<string> { "untested", "pass", "pass", "pass" });
            testMatrix.Add(new List<string> { "untested", "pass_retest", "pass_retest", "pass_retest" });
            testMatrix.Add(new List<string> { "untested", "pattern_issue", "pattern_issue", "pattern_issue" });
            testMatrix.Add(new List<string> { "untested", "repairable", "repairable", "repairable" });
            testMatrix.Add(new List<string> { "untested", "unrepairable", "unrepairable", "unrepairable" });
            testMatrix.Add(new List<string> { "untested", "untested", "untested", "untested" });

            foreach (var test in testMatrix)
            {
                foreach (var idx in new List<int>() { 2, 3 })
                {
                    var retest = idx == 3;
                    try
                    {
                        var rslt = hryAlg.MergeHry(MbistHRYAlgorithm.HryTable[test[0]], MbistHRYAlgorithm.HryTable[test[1]], retest);
                        Assert.AreEqual(MbistHRYAlgorithm.HryTable[test[idx]], rslt, $"mergeHry({test[0]}, {test[1]}, {retest}) failed");
                    }
                    catch (Exception e) when (!(e is AssertFailedException))
                    {
                        Assert.Fail($"mergeHry({test[0]}, {test[1]}, {retest}) threw exception=[{e.Message}]");
                    }
                }
            }
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void MergeHry_string_ExceptionDifferentLengths()
        {
            var hryAlg = new MbistHRYAlgorithm();
            var original = "UUUUUUUUUU"; // 10
            var current = "PPPPPPPPP"; // 9

            try
            {
                var rslt = hryAlg.MergeHry(original, current, true);
                Assert.Fail("No exception thrown");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("original and currentHry string are different lenght in mergeHry - 10 vs 9", e.Message);
            }
            catch (Exception e) when (!(e is AssertFailedException))
            {
                Assert.Fail($"Wrong Exceptiong thrown - {e.Message}");
            }
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void MergeHry_string_All()
        {
            var hryAlg = new MbistHRYAlgorithm();
            var original = "PPFUUUUUUUUUUUUU";
            var current = "UUUFFPP888II7UU6";
            var expect = "PPFFFPP888II7UU6";

            var rslt = hryAlg.MergeHry(original, current, true);
            Assert.AreEqual(expect, rslt);
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void GenerateItuff_short()
        {
            var tpSettingsServiceMock = new Mock<Prime.TpSettingsService.ITpSettingsService>(MockBehavior.Strict);
            tpSettingsServiceMock.Setup(o => o.IsTpFeatureEnabled(Prime.TpSettingsService.Feature.Midas)).Returns(false); // mimic class, token is 2
            Prime.Services.TpSettingsService = tpSettingsServiceMock.Object;

            var hryAlg = new MbistHRYAlgorithm();
            var hryStr = "PPPPPPPPPPPPPPPPPPPPPPFPPPPPPPPPPP";
            var expect = "2_tname_HRY_RAWSTR_MBIST_1\n2_strgval_PPPPPPPPPPPPPPPPPPPPPPFPPPPPPPPPPP\n2_lsep\n";

            Assert.AreEqual(expect, hryAlg.GenerateItuffData(hryStr));
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void GenerateItuff_longOddLen()
        {
            var tpSettingsServiceMock = new Mock<Prime.TpSettingsService.ITpSettingsService>(MockBehavior.Strict);
            tpSettingsServiceMock.Setup(o => o.IsTpFeatureEnabled(Prime.TpSettingsService.Feature.Midas)).Returns(true); // mimic sort, token is 0
            Prime.Services.TpSettingsService = tpSettingsServiceMock.Object;

            var hryAlg = new MbistHRYAlgorithm();
            var hryStr = "PPPPPPPPPPPPPPPPPPPPPPFPPPPPPPPPPP";
            var expect = "0_tname_HRY_RAWSTR_MBIST_1\n0_strgval_PPPPPPPPPP\n2_lsep\n";
            expect += "0_tname_HRY_RAWSTR_MBIST_2\n0_strgval_PPPPPPPPPP\n2_lsep\n";
            expect += "0_tname_HRY_RAWSTR_MBIST_3\n0_strgval_PPFPPPPPPP\n2_lsep\n";
            expect += "0_tname_HRY_RAWSTR_MBIST_4\n0_strgval_PPPP\n2_lsep\n";

            Assert.AreEqual(expect, hryAlg.GenerateItuffData(hryStr, 10));
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void GenerateItuff_longMatchLen()
        {
            var tpSettingsServiceMock = new Mock<Prime.TpSettingsService.ITpSettingsService>(MockBehavior.Strict);
            tpSettingsServiceMock.Setup(o => o.IsTpFeatureEnabled(Prime.TpSettingsService.Feature.Midas)).Returns(true); // mimic sort, token is 0
            Prime.Services.TpSettingsService = tpSettingsServiceMock.Object;

            var hryAlg = new MbistHRYAlgorithm();
            var hryStr = "PPPPPPPPPPPPPPPPPPPPPPFPPPPPPPPPPPUUUUUU";
            var expect = "0_tname_HRY_RAWSTR_MBIST_1\n0_strgval_PPPPPPPPPP\n2_lsep\n";
            expect += "0_tname_HRY_RAWSTR_MBIST_2\n0_strgval_PPPPPPPPPP\n2_lsep\n";
            expect += "0_tname_HRY_RAWSTR_MBIST_3\n0_strgval_PPFPPPPPPP\n2_lsep\n";
            expect += "0_tname_HRY_RAWSTR_MBIST_4\n0_strgval_PPPPUUUUUU\n2_lsep\n";

            Assert.AreEqual(expect, hryAlg.GenerateItuffData(hryStr, 10));
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void CheckHryPriorities_All()
        {
            var hryAlg = new MbistHRYAlgorithm();

            // static public char collapseSectionHry(List<char> sectionHrys)
            Assert.AreEqual('I', hryAlg.CollapseHryByPriority(new List<char> { 'U', '1', '0', '6', '5', '8', 'I' }));
            Assert.AreEqual('8', hryAlg.CollapseHryByPriority(new List<char> { 'U', '1', '0', '6', '5', '8', '0' }));
            Assert.AreEqual('5', hryAlg.CollapseHryByPriority(new List<char> { 'U', '1', '0', '6', '5', '1', '0' }));
            Assert.AreEqual('6', hryAlg.CollapseHryByPriority(new List<char> { 'U', '1', '0', '6', 'U', '1', '0' }));
            Assert.AreEqual('0', hryAlg.CollapseHryByPriority(new List<char> { 'U', '1', '0', 'U', 'U', '1', '0' }));
            Assert.AreEqual('1', hryAlg.CollapseHryByPriority(new List<char> { 'U', '1', 'U', 'U', 'U', '1', 'U' }));
            Assert.AreEqual('U', hryAlg.CollapseHryByPriority(new List<char> { 'U', 'U', 'U', 'U', 'U', 'U', 'U' }));
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void ExtractBits_All()
        {
            var hryAlg = new MbistHRYAlgorithm();

            // static public string extract_bits(string basestring, List<int> bits)
            //             0123456789
            var testStr = "1234567890";

            Assert.AreEqual("1", hryAlg.Extract_bits(testStr, new List<int> { 0 }));
            Assert.AreEqual("0", hryAlg.Extract_bits(testStr, new List<int> { 9 }));
            Assert.AreEqual("105", hryAlg.Extract_bits(testStr, new List<int> { 0, 9, 4 }));
            Assert.AreEqual("567", hryAlg.Extract_bits(testStr, new List<int> { 4, 5, 6 }));
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void GenerateHryTest1Group()
        {
            Console.WriteLine("Running GenerateHryTest1Group");

            var mem1 = new MbistHRYInput.MbistLookupTable.Controller.AlgorithmGroup.Memory();
            mem1.GoIDBits = new List<int>() { 12, 13, 14, 15 };
            mem1.MemoryId = 1;
            mem1.HryIndex = 0;

            var mem2 = new MbistHRYInput.MbistLookupTable.Controller.AlgorithmGroup.Memory();
            mem2.GoIDBits = new List<int>() { 16, 17, 18, 19 };
            mem2.MemoryId = 2;
            mem2.HryIndex = 1;

            var group1 = new MbistHRYInput.MbistLookupTable.Controller.AlgorithmGroup();
            group1.StatusBits = new List<int>() { 0, 1, 2, 3 };
            group1.AlgorithmSelectBits = new List<int>() { 4, 5, 6, 7 };
            group1.Memories = new List<MbistHRYInput.MbistLookupTable.Controller.AlgorithmGroup.Memory> { mem1, mem2 };

            var testCases = new List<List<string>>();

            // !                                         1111111111       Mem1                Mem2
            // !                               01234567890123456789      Result              Result
            testCases.Add(new List<string> { "00001111000000000000", "pattern_issue", "pattern_issue" });  // alg_bits!=0 => pattern_issue
            testCases.Add(new List<string> { "00001000000000000000", "pattern_issue", "pattern_issue" });  // alg_bits!=0 => pattern_issue
            testCases.Add(new List<string> { "00000100000000000000", "pattern_issue", "pattern_issue" });  // alg_bits!=0 => pattern_issue
            testCases.Add(new List<string> { "00000010000000000000", "pattern_issue", "pattern_issue" });  // alg_bits!=0 => pattern_issue
            testCases.Add(new List<string> { "00000001000000000000", "pattern_issue", "pattern_issue" });  // alg_bits!=0 => pattern_issue
            testCases.Add(new List<string> { "00010000000000000000", "pattern_issue", "pattern_issue" });  // bit='0001 => pattern_issue
            testCases.Add(new List<string> { "00000000000000000000", "pass", "pass" });  // bit='0000, mem1_goid='0000 => pass, mem2_goid='0000 => pass
            testCases.Add(new List<string> { "00100000000001000000", "fail", "pass" });  // bit='0010, mem1_goid='0100 => fail, mem2_goid='0000 => pass
            testCases.Add(new List<string> { "00100000000000000010", "pass", "fail" });  // bit='0010, mem1_goid='0000 => pass, mem2_goid='0010 => fail
            testCases.Add(new List<string> { "00100000000011000001", "fail", "fail" });  // bit='0010, mem1_goid='1100 => fail, mem2_goid='0001 => fail
            testCases.Add(new List<string> { "00000000000000000010", "inconsist_ps_fg", "inconsist_ps_fg" });  // bit='0000, mem1_goid='0000 => inconsist_ps_fg, mem2_goid='0010 => inconsist_ps_fg
            testCases.Add(new List<string> { "00100000000000000000", "inconsist_fs_pg", "inconsist_fs_pg" });  // bit='0010, mem1_goid='0000 => inconsist_fs_pg, mem2_goid='0000 => inconsist_fs_pg
            testCases.Add(new List<string> { "11110000000000000000", "cont_fail", "cont_fail" });  // bit='1111 => cont_fail

            var hryAlg = new MbistHRYAlgorithm();
            foreach (var testcase in testCases)
            {
                Console.WriteLine("Calling hryAlg.GenerateHRYForGroup");

                var rslt = hryAlg.GenerateHRYForGroup(testcase[0], group1);
                Assert.IsTrue(rslt.ContainsKey(0), $"No result for Mem1 - Data={testcase[0]}");
                Assert.IsTrue(rslt.ContainsKey(1), $"No result for Mem2 - Data={testcase[0]}");
                Assert.AreEqual(MbistHRYAlgorithm.HryTable[testcase[1]], rslt[0], $"Failed Mem1 Data={testcase[0]}");
                Assert.AreEqual(MbistHRYAlgorithm.HryTable[testcase[2]], rslt[1], $"Failed Mem2 Data={testcase[0]}");
            }
        }

        /// <summary>Unit test for HRY.</summary>
        [TestMethod]
        public void ExecuteFull_pass1()
        {
            var pin = "TDO";
            var ctv = new Dictionary<string, string> { { pin, "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" } };
            var fakeTC = "FakeTC";
            var plist = "arr_mbist_x_x_tap_all_hry_ssa_all_parallelallsteps_list";
            var cfgFile = GetPathToFiles() + "mbist_hry_sds13R.json";
            var initialHry = "111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111Y11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111";
            var finalHry = "111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111P11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111";
            var expectedItuff = $"2_tname_HRY_RAWSTR_MBIST_1\n2_strgval_{finalHry}\n2_lsep\n";

            var tpSettingsServiceMock = new Mock<Prime.TpSettingsService.ITpSettingsService>(MockBehavior.Strict);
            tpSettingsServiceMock.Setup(o => o.IsTpFeatureEnabled(Prime.TpSettingsService.Feature.Midas)).Returns(false);
            Prime.Services.TpSettingsService = tpSettingsServiceMock.Object;

            var evgMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            evgMock.Setup(o => o.GetStringRowFromTable("HRY_RAWSTR_MBIST", Context.DUT)).Returns(initialHry);
            evgMock.Setup(o => o.InsertRowAtTable("HRY_RAWSTR_MBIST", finalHry, Context.DUT));
            Prime.Services.SharedStorageService = evgMock.Object;

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.FileExists(cfgFile)).Returns(true);
            fileServiceMock.Setup(o => o.GetFile(cfgFile)).Returns(cfgFile);
            Prime.Services.FileService = fileServiceMock.Object;

            var ituffServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetCustomTname("HRY_RAWSTR_MBIST"));
            strValWriterMock.Setup(o => o.SetData(finalHry));

            ituffServiceMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            ituffServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            Prime.Services.DatalogService = ituffServiceMock.Object;

            var funcTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Execute()).Returns(false);
            funcTestMock.Setup(o => o.GetCtvData()).Returns(ctv);
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureCtvPerPinTest(plist, fakeTC, fakeTC, new List<string> { pin }, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var template = new MbistHRYTC
            {
                HRYInputFile = cfgFile,
                RetestMode = MbistHRYTC.MyBoolean.TRUE,
                LogToItuff = MbistHRYTC.MyBoolean.TRUE,

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
            ituffServiceMock.Verify(o => o.WriteToItuff(strValWriterMock.Object), Times.Once);
        }

        private static string GetPathToFiles([CallerFilePath] string srcPath = "")
        {
            return Path.GetDirectoryName(srcPath) + "\\InputFiles\\";
        }
    }
}
