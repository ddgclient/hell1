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

namespace SIOEDCLogTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.SharedStorageService;
    using Prime.TestMethods;
    using Prime.TpSettingsService;
    using Prime.UserVarService;
    using SIO;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class SIOEDCLog_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SIOEDCLog_UnitTest"/> class.
        /// </summary>
        public SIOEDCLog_UnitTest()
        {
            this.ConsoleOutput = new List<string>();
            this.GSDSValues = new Dictionary<string, string>();

            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                Console.WriteLine(s);
                this.ConsoleOutput.Add(s);
            });
            /* this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => Console.WriteLine($"ERROR: {msg}")); */
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;

            this.FileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            this.FileServiceMock.Setup(o => o.FileExists(It.IsAny<string>())).Returns(true);
            this.FileServiceMock.Setup(o => o.GetFile(It.IsAny<string>())).Returns((string s) => Path.IsPathRooted(s) ? s : GetPathToFiles() + Path.GetFileName(s));
            Prime.Services.FileService = this.FileServiceMock.Object;

            this.SharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            /* this.SharedServiceMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), Context.DUT)).Callback((string key, string value, Context context) =>
            {
                Console.WriteLine($"[GSDS] {key} = {value}");
                this.GSDSValues[key] = value;
            }); */
            this.SharedServiceMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), Context.DUT))
                .Returns((string key, Context context) => this.GSDSValues[key])
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"[GSDS] Reading {key}.");
                });
            Prime.Services.SharedStorageService = this.SharedServiceMock.Object;

            this.UserVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            this.UserVarServiceMock.Setup(o => o.Exists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string collection, string uservar) => false);
            Prime.Services.UserVarService = this.UserVarServiceMock.Object;
        }

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private Mock<IFileService> FileServiceMock { get; set; }

        private Mock<ISharedStorageService> SharedServiceMock { get; set; }

        private Mock<IUserVarService> UserVarServiceMock { get; set; }

        private List<string> ConsoleOutput { get; set; }

        private Dictionary<string, string> GSDSValues { get; set; }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void Get_bit_field_EndHigher()
        {
            var utils = new SIOEDC_Util(false);

            // !        222222111111111
            // !        54321098765432109876543210
            // !               ^msb      ^lsb
            var data = "abcdefghijklmnopqrstuvwxyz"; // data, msb first
            Assert.AreEqual("hijklmnopqr", utils.Get_bit_field(data, 8, 18));
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void Get_bit_field_EndLower()
        {
            var utils = new SIOEDC_Util(false);

            // !        222222111111111
            // !        54321098765432109876543210
            // !               ^lsb      ^msb
            var data = "abcdefghijklmnopqrstuvwxyz"; // data, msb first
            Assert.AreEqual("rqponmlkjih", utils.Get_bit_field(data, 18, 8));
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void Parse_SIO_DSI_LPBK1_userFile()
        {
            var sio = new SIOLib(true);
            var rslt = new UserFile("SIO_DSI_LPBK1_dsi_mv_setup.txt");
            Assert.IsTrue(rslt.Valid);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void Parse_SIO_CSI_LPBK1_userFile()
        {
            var sio = new SIOLib(true);
            var rslt = new UserFile("SIO_CSI_LPBK1_csi_mv_setup.txt");
            Assert.IsTrue(rslt.Valid);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void Parse_SIO_DP_LPBK1_userFile()
        {
            var sio = new SIOLib(true);
            var rslt = new UserFile("SIO_DP_LPBK1_tgl_DPLpbk_mv_setup.txt");
            Assert.IsTrue(rslt.Valid);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void Parse_SIO_userFile()
        {
            var sio = new SIOLib(true);
            var rslt = new UserFile("SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt");
            Assert.IsTrue(rslt.Valid);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void Parse_SIO_PCIE_DC1_userFile()
        {
            var sio = new SIOLib(true);
            var rslt = new UserFile("SIO_PCIE_DC1_tgl_pcie_mv_setup.txt");
            Assert.IsTrue(rslt.Valid);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void Parse_SIO_TCSS_ALL1_userFile()
        {
            var sio = new SIOLib(true);
            var rslt = new UserFile("SIO_TCSS_ALL1_mv_setup.txt");
            Assert.IsTrue(rslt.Valid);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void CountListElements_Test()
        {
            string[] testLst = new[]
            {
                "AB", "CD", "EF", "GH", "IJ", "KL", "MN", "OP", "QR", "ST", "UV", "WX", "YZ",
                "AB", "CD", "EF", "GH", "IJ", "IJ", "IJ", "IJ", "IJ", "EF", "EF", "AB",
            };
            Dictionary<string, int> cmpDictFull = new Dictionary<string, int>()
            {
                { "IJ", 6 }, { "EF", 4 }, { "AB", 3 }, { "CD", 2 }, { "GH", 2 }, { "KL", 1 },
                { "MN", 1 }, { "OP", 1 }, { "QR", 1 }, { "ST", 1 }, { "UV", 1 }, { "WX", 1 },
                { "YZ", 1 },
            };
            Dictionary<string, int> cmpDict3 = new Dictionary<string, int>()
            {
                { "IJ", 6 }, { "EF", 4 }, { "AB", 3 },
            };
            var sio = new SIOLib(true);
            var countFull = sio.CountListElements(testLst, 105);
            CollectionAssert.AreEquivalent(cmpDictFull, countFull, "Failed on compare with all elements.");

            var count3 = sio.CountListElements(testLst, 3);
            CollectionAssert.AreEquivalent(cmpDict3, count3, "Failed on compare with 3 elements.");
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void StringSplitInParts_Test()
        {
            string testString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            string[,][] fullCmpArray = new string[5, 4][];
            fullCmpArray[2, 0] = new[] { "AB", "CD", "EF", "GH", "IJ", "KL", "MN", "OP", "QR", "ST", "UV", "WX", "YZ" };
            fullCmpArray[2, 1] = new[] { "BC", "DE", "FG", "HI", "JK", "LM", "NO", "PQ", "RS", "TU", "VW", "XY" };

            fullCmpArray[3, 0] = new[] { "ABC", "DEF", "GHI", "JKL", "MNO", "PQR", "STU", "VWX" };
            fullCmpArray[3, 1] = new[] { "BCD", "EFG", "HIJ", "KLM", "NOP", "QRS", "TUV", "WXY" };
            fullCmpArray[3, 2] = new[] { "CDE", "FGH", "IJK", "LMN", "OPQ", "RST", "UVW", "XYZ" };

            fullCmpArray[4, 0] = new[] { "ABCD", "EFGH", "IJKL", "MNOP", "QRST", "UVWX" };
            fullCmpArray[4, 1] = new[] { "BCDE", "FGHI", "JKLM", "NOPQ", "RSTU", "VWXY" };
            fullCmpArray[4, 2] = new[] { "CDEF", "GHIJ", "KLMN", "OPQR", "STUV", "WXYZ" };
            fullCmpArray[4, 3] = new[] { "DEFG", "HIJK", "LMNO", "PQRS", "TUVW" };

            for (int partLen = 2; partLen < 5; partLen++)
            {
                for (int offset = 0; offset < partLen; offset++)
                {
                    var perm = SIOLib.SplitStrInParts(testString, partLen, offset).ToArray();
                    CollectionAssert.AreEqual(fullCmpArray[partLen, offset], perm, $"Failed on Len=[{partLen}] Offset=[{offset}]. ");
                }
            }
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void StringSplitInParts_TestFail()
        {
            string testString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var perm = SIOLib.SplitStrInParts(testString, 10000, 0, true).ToArray();
            Assert.IsTrue(perm.Count() == 1, $"Count = [{perm.Count()}].");
            Assert.AreEqual(testString, perm[0]);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void BinToBase32_EvenLength()
        {
            var binNum = "11111";
            var sio = new SIOLib(true);
            Assert.AreEqual("A7", sio._BinToBase32(binNum));
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void BinToBase32_Test1()
        {
            var binNum = "111111001100000000000000000000001000000000010011010011011001011000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
            var sio = new SIOLib(true);
            var base32 = sio._BinToBase32(binNum);
            Assert.AreEqual(base32, "A7TAAAAEACNGZMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void BinToBase32_Test2()
        {
            var binNum = "100100101111011000000000111010001110101011000010110110100011110000100110000010000000000010100001010001110000110110010101000101010110000001101000011100101000010000110110011110111101101110111011001101100100100010110100000001101110111110001101111010010110110011111000000000011100010101001110100110100010001000110111111010110011110000111111101111101010111101111110111111111110010000110011001000100011010111101111100101111011100000100101110010001111011011000101001001011100001000101111101010111011111011010101001100110101001100000110111000011110000110111101100001000000010111110001011111110101110111010011111111110111110011111001000110010011011111001010111101110111100110011000010011010110110110100100000001011111011001001001010011001010010011011011001111101101011110110100010000010011111000100101100011011011011011111011101010100100011100110000011100110111100000110011001010110011011111010100011101000001110001001011011001001001111010000011000001100111111111011011";
            var sio = new SIOLib(true);
            var base32 = sio._BinToBase32(binNum);
            Assert.AreEqual("ASL3AB2HKYLNDYJQIACQUODMVCVQGQ4UEGZ55XOZWJC2AN34N5FWPQAOFJ2NCEN7LHQ735L3677SDGIRV56L3QJOI63CSLQRPVO7NKM2TA3Q6DPMEAXYX6XOT756PSGJXZL3XTGCNNWSAL5SJJSSNWPWXWRAT4JMNW352URZQON4DGKZX2R2BYS3ET2BQM763", base32);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void BinToBase32_Test3()
        {
            var binNum = "0011000101111100101001001111100100101000000111010111111111101001110011101101100110100001100010101011111011011000101101001100011110100011000101001001011110110100110111101001000010010100011001110000010111010011100001011110110101100001110011111010001010000000011101010110001100011110001010110010111001001001000001110111000100111110000010111100010011101100001110011001010010100010110111100010101101111001001010000101000110000000001100001111010000111101010100101101010010101010001000001001010000010100010100111000011000011100010011000001110010000010010010100000111110000111010101100110111001101110010110000000010100000111110001000101110001111110010100010110111000010001001111100101010110101010101101000000101011001000101111110101101000001111001011100111001101110001100011011100010000110110000100110001001111001110111011000000001100011011001100101110010101111100010010010000011011000110000110011110011100010010111011110101010001101011000010100001001000110101000101101";
            var sio = new SIOLib(true);
            var base32 = sio._BinToBase32(binNum);
            Assert.AreEqual("EAML4UT4SQHL75HHNTIMKX3MLJR5DCSL3JXUQSRTQLU4F5VQ47IUAOVRR4KZOJEDXCPQLYTWDTFFC3YVXSKCRQAYPIPKS2SVCBFAUKODBYTA4QJFA7B2WNZXFQBIHYROH4ULOCE7FLKVUBLEL6WQPFZZXDDOEGYJRHTXMAMNTFZL4JEDMMGPHCLXVI2YKCI2RN", base32);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void BinToBase32_Test4()
        {
            var binNum = "11111100011010101101010000100001110101100011000110101011111100110110011011000111001110001100111011010011010010101110111010010001011110110110101100000100110010010100000101011101001011111001101100010100010111010101010011010101010000001100010100111101101011000110001000011101110111011001110011001110001011101011110000101010010011111100000110100101110011010000000111011111110110110111011100011011011100101110110011010101111001011011110110100000010100101011111010101011011111010101000011001101010110000100011011011111100110011100100111001000110010010010011000010011010101000011101101111101100110110100100110011111010100010110011111010000110100111100010100111110100000011101101011010101101010001110110010011000011100100100101111011111101000011111000110000001011000100001100010111111111000111000001010100110101111001110011110100010101000101010000001110010100001110101001100010011110111011010001110000011100010111111111001000111001001010001000000110000110010001100110100";
            var sio = new SIOLib(true);
            var base32 = sio._BinToBase32(binNum);
            Assert.AreEqual("DD6GVVBB2YY2X43GY44M5U2K52IXW2YEZFAV2L43CROVJVKAYU62YYQ53WOM4LV4FJH4DJONAHP5W5Y3OLWNLZN5UBJL5K35KDGVQRW7THE4RSJGCNKDW7M3JGPVCZ6Q2PCT5AO22WUOZGDSJPP2D4MBMIML7Y4CU26OPIVCUBZIOUYT3WRYHC76I4SRAMGIZU", base32);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void BinToBase32_Test5()
        {
            var binNum = "011010000111010001110000111111010110011001100100001111111010010100001001111110000100001011001101011011000110010111011011110001100111011000001111011101110110011000000000000000010100001010000010000110110011010110100111100000100001110000001000001110000101000000000101111000011111010101110001111011010001101100101110000100100001110000001000101011001110101101000000001110011101100101011110000100001001111000001011110000010001010100111111110110000000101110010010100001011111100111111100101101111101100010100010110010010010111101101000000111100100110011000001010101001110011100001001001011110101100011010110110101010010100110001010001111011100011101001000110000100010111101110111111011101100100100000111100010110111100110011101010001001011110101000101100000101001010111001101110100110000010010111001111010001010111011100111011100000111010101001000001000110000000111111101101000111111100011010111000100111011011011001101110101101000100011001001101100011101111001010000101";
            var sio = new SIOLib(true);
            var base32 = sio._BinToBase32(binNum);
            Assert.AreEqual("CDIORYP2ZTEH6SQT6CCZVWGLW6GOYHXOZQAAFBIEGZVU6BBYCBYKAC6D5LR5UNS4EQ4BCWOWQBZ3FPBBHQLYEKT7WALSKC7T7FX3CRMSL3IDZGMCVHHBEXVRVWVFGFD3R2IYIXXP3WJA6FXTHKEXVCYFFON2MCLT2FO45YHKSBDAH62H6GXCO3M3VUIZGY54UF", base32);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void BinToBase32_Test6()
        {
            var binNum = "1000001111100100101010101101001010010011010110111101001111101100110010000011101010100110111000010010011111100001000010111011010110010100000000001110111101010001011111000111100011000001111111000011100111000101111010100000100010101000101011100110011001011111000111101000011010000110110110000000100011101100000100110011001011000111100001111100110001011010101100010011010110000000101011010010001100000100101011110011111100001101100111001100010100111001111000001011010011010010010101001100001001010000011001000000111111010010110010110100100011000000010101101100010111011000110110110110101111111110001101111001000100001100100011001110100001001000110000001011011101100010010110001011100101100101110101000101111010100010001100001000011011010001010110101000000011000001001010001101000100011011100110010001110010110010010010101010111111111111001111111000000000111110010111000010100010100001100100010001010011011001010010000010010011101111101011011101001001010000000010010001";
            var sio = new SIOLib(true);
            var base32 = sio._BinToBase32(binNum);
            Assert.AreEqual("BIHZFK2KJVXU7MZA5KNYJH4EF3LFAA55IXY6GB7Q44L2QIVCXGMXY6Q2DNQCHMCMZMPB6MLKYTLAFNEMCK6PYNTTCTTYFU2JKMEUDEB7JMWSGAK3C5RW3L7Y3ZCDEM5BEMBN3CLC4WLVC6UIYINUK2QDASRUI3TEOLESVP747YAPS4FCQZCFGZJASO7LOSKAER", base32);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void Compression_Test1()
        {
            var stringToCompress = "BIQI4GYEAJRCQDLMCFFNL4XH6FEZ4IHSQSMQBO6ZXNVELNUPSKKAFISKRQFSZPDO3RCQ45DF5WX75KW57EOID4DCCLTAQUBZLMLD2QQGRBALJPYLKRY6D5FW7V4JBGH42PQ3R3HUJPAHZCPOGHPY4BK34YXH7ZMJJHRQUPBZRR5RCQ2UBYMDXUB3JHLVMJB7FB";
            var expectedCompressedStr = "9810*L'~}?^][@=>.->+)$#(<4!F5WX75KW57EOID4DCCLTAQU;LMLD2QQGRBAL:YLKRY6D5FW7V4JB&42PQ(3HU:AHZCPO&PY4BK34YXH7Z`/+UP;RR5R<2%YMDX%3/LV`B7FB";
            var expectedTranlation = "9BIQI|84GYE|1AJRC|0QDLM|*CFFN|'4XH|~6FE|}Z4I|?HSQ|^SMQ|]BO6|[ZXN|@VEL|=NUP|>SK|+RQ|<CQ|;BZ|:JP|`MJ|/JH|.KA|-FI|)FS|(3R|&GH|%UB|$ZP|#DO|!5D";

            var sio = new SIOLib(false);
            var data = sio.ProcessPermutations(stringToCompress);
            var compressedString = data.CompressedString;
            var translationString = data.TranslationTable;
            var uncompressedString = sio.Uncompress(compressedString, translationString);

            Console.WriteLine($"CompressedData= {compressedString}");
            Console.WriteLine($"TranslationKey= {translationString}");

            Assert.AreEqual(stringToCompress, uncompressedString, "Failed Compress->Uncompress test");
            Assert.AreEqual(expectedCompressedStr, compressedString);
            Assert.AreEqual(expectedTranlation, translationString);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void Compression_Test2()
        {
            // manually created input to be fully deterministic
            var stringToCompress = "AAAABBBBCCCCBBBBAAAACCCCAAAADDDDBBBBAAAAEEEEDDDDAAAABBBBAAAACCCCBBBBEEEEAAAADDDDCCCCBBBBaaFFFGGGFFFHHHFFFIIIFFFJJJFFFKKKFFFLLLFFFMMMFFFNNNFFFbbbFFFcccFFFdddFFFeeeFFFfffFFFgggFFFhhhFFFiiiGGGHHHGGGIIIGGGJJJGGGKKKGGGLLLGGGMMMGGGNNNGGGjjjGGGkkkGGGlllGGGmmmGGGnnnGGGoooGGGpppHHHIIIHHHJJJHHHKKKHHHLLLHHHMMMHHHNNNHHHqqqHHHrrrHHHsssHHHtttHHHuuuHHHvvvIIIJJJIIIKKKIIILLLIIIMMMIIINNNIIIwwwIIIxxxIIIyyyIIIzzzIII222JJJKKKJJJLLLJJJMMMJJJNNNJJJ333JJJ444JJJ555JJJ666KKKLLLKKKMMMKKKNNNKKK777KKKaaaKKKbbbLLLMMMLLLNNNLLLcccLLLdddMMMNNNMMMaOOPPOOQQOORROOSSOOTTOOUUOOVVOOWWOOXXOOYYOOZIOOZEOOZFOOZGOOZHOObcOObdOObeOObfOObgOObhOOciOOcjOOckOOclOOcmOOcnOOdoOOdpOOdqOOdrOOdsOOdtOOeuOOevOOewPPQQPPRRPPSSPPTTPPUUPPVVPPWWPPXXPPYYPPZIPPZEPPZFPPZGPPZHPPccPPcdPPcePPcfPPcgPPchPPdiPPdjPPdkPPdlPPdmPPdnPPeoPPepPPeqPPerPPesPPetPPfuPPfvQQRRQQSSQQTTQQUUQQVVQQWWQQXXQQYYQQZIQQZEQQZFQQZGQQZHQQdcQQddQQdeQQdfQQdgQQdhQQeiQQejQQekQQelQQemQQenQQfoQQfpQQfqQQfrQQfsQQftQQguRRSSRRTTRRUURRVVRRWWRRXXRRYYRRZIRRZERRZFRRZGRRZHRRecRRedRReeRRefRRegRRehRRfiRRfjRRfkRRflRRfmRRfnRRgoRRgpRRgqRRgrRRgsRRgtSSTTSSUUSSVVSSWWSSXXSSYYSSZISSZESSZFSSZGSSZHSSfcSSfdSSfeSSffSSfgSSfhSSgiSSgjSSgkSSglSSgmSSgnSShoSShpSShqSShrSShsTTUUTTVVTTWWTTXXTTYYTTZITTZETTZFTTZGTTZHTTgcTTgdTTgeTTgfTTggTTghTThiTThjTThkTThlTThmTThnTTioTTipTTiqTTirUUVVUUWWUUXXUUYYUUZIUUZEUUZFUUZGUUZHUUhcUUhdUUheUUhfUUhgUUhhUUiiUUijUUikUUilUUimUUinUUjoUUjpUUjqVVWWVVXXVVYYVVZIVVZEVVZFVVZGVVZHVVicVVidVVieVVifVVigVVihVVjiVVjjVVjkVVjlVVjmVVjnVVkoVVkpWWXXWWYYWWZIWWZEWWZFWWZGWWZHWWjcWWjdWWjeWWjfWWjgWWjhWWkiWWkjWWkkWWklWWkmWWknWWloXXYYXXZIXXZEXXZFXXZGXXZHXXkcXXkdXXkeXXkfXXkgXXkhXXliXXljXXlkXXllXXlmXXlnYYZIYYZEYYZFYYZGYYZHYYlcYYldYYleYYlfYYlgYYlhYYmiYYmjYYmkYYmlYYmmZIZEZIZFZIZGZIZHZImcZImdZImeZImfZImgZImhZIniZInjZInkZInlZEZFZEZGZEZHZEncZEndZEneZEnfZEngZEnhZEoiZEojZEokZFZGZFZHZFocZFodZFoeZFofZFogZFohZFpiZFmjZGZHZGpcZGpdZGpeZGpfZGpgZGphZGqiZHqcZHqdZHqeZHqfZHqgZHqh";
            var expectedCompressedStr = "9818919089*098918*9018aa'~'}'?'^']'['@'='bbb'ccc'ddd'eee'fff'ggg'hhh'iii~}~?~^~]~[~@~=~jjj~kkk~lll~mmm~nnn~ooo~ppp}?}^}]}[}@}=}qqq}rrr}sss}ttt}uuu}vvv?^?]?[?@?=?www?xxx?yyy?zzz?222^]^[^@^=^333^444^555^666][]@]=]777]aaa]bbb[@[=[ccc[ddd@=@a>+><>;>:>`>/>.>->)>(>&>%>$>#>!>bc>bd>be>bf>bg>bh>ci>cj>ck>cl>cm>cn>do>dp>dq>dr>ds>dt>eu>ev>ew+<+;+:+`+/+.+-+)+(+&+%+$+#+!+cc+cd+ce+cf+cg+ch+di+dj+dk+dl+dm+dn+eo+ep+eq+er+es+et+fu+fv<;<:<`</<.<-<)<(<&<%<$<#<!<dc<dd<de<df<dg<dh<ei<ej<ek<el<em<en<fo<fp<fq<fr<fs<ft<gu;:;`;/;.;-;);(;&;%;$;#;!;ec;ed;ee;ef;eg;eh;fi;fj;fk;fl;fm;fn;go;gp;gq;gr;gs;gt:`:/:.:-:):(:&:%:$:#:!:fc:fd:fe:ff:fg:fh:gi:gj:gk:gl:gm:gn:ho:hp:hq:hr:hs`/`.`-`)`(`&`%`$`#`!`gc`gd`ge`gf`gg`gh`hi`hj`hk`hl`hm`hn`io`ip`iq`ir/./-/)/(/&/%/$/#/!/hc/hd/he/hf/hg/hh/ii/ij/ik/il/im/in/jo/jp/jq.-.).(.&.%.$.#.!.ic.id.ie.if.ig.ih.ji.jj.jk.jl.jm.jn.ko.kp-)-(-&-%-$-#-!-jc-jd-je-jf-jg-jh-ki-kj-kk-kl-km-kn-lo)()&)%)$)#)!)kc)kd)ke)kf)kg)kh)li)lj)lk)ll)lm)ln(&(%($(#(!(lc(ld(le(lf(lg(lh(mi(mj(mk(ml(mm&%&$&#&!&mc&md&me&mf&mg&mh&ni&nj&nk&nl%$%#%!%nc%nd%ne%nf%ng%nh%oi%oj%ok$#$!$oc$od$oe$of$og$oh$pi$mj#!#pc#pd#pe#pf#pg#ph#qi!qc!qd!qe!qf!qg!qh";
            var expectedTranlation = "9AAAA|8BBBB|1CCCC|0DDDD|*EEEE|'FFF|~GGG|}HHH|?III|^JJJ|]KKK|[LLL|@MMM|=NNN|>OO|+PP|<QQ|;RR|:SS|`TT|/UU|.VV|-WW|)XX|(YY|&ZI|%ZE|$ZF|#ZG|!ZH";

            var sio = new SIOLib(true);
            var data = sio.ProcessPermutations(stringToCompress);
            var compressedString = data.CompressedString;
            var translationString = data.TranslationTable;
            var uncompressedString = sio.Uncompress(compressedString, translationString);

            Assert.AreEqual(stringToCompress, uncompressedString, "Failed to uncompress to original");
            Assert.AreEqual(expectedTranlation, translationString, "Failed Translation check");
            Assert.AreEqual(expectedCompressedStr, compressedString, "Failed Compression check");
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void Parse_SIO_PCIE_LPBK1_seqFile1()
        {
            var sio = new SIOLib(true);
            var util = new SIOEDC_Util(true);
            var rslt = util.LoadSequenceFile("SIO_PCIE_LPBK1_sequence.csv");
            Assert.IsFalse(rslt.Count() == 0, "LoadSequenceFile returned an empty Dictionary.");
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void Parse_SIO_PCIE_LPBK1_formatFile1()
        {
            var sio = new SIOLib(true);
            var util = new SIOEDC_Util(true);
            var rslt = util.LoadFormatFile("format_SIO_PCIE_LPBK1_merged.csv");
            Assert.IsTrue(rslt.valid, "LoadFormatFile failed to parse file.");
            Assert.IsTrue(rslt.data.Count == 5, $"LoadFormatFile saved [{rslt.data.Count}] formats instead of 5.");
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void GenerateOutput_Test1()
        {
            var sio = new SIOLib(true);
            var util = new SIOEDC_Util(true);
            var seqID = "class_edc_g3";
            var binNum = "111111011100000000000000000000001000000000011111011111011111111010010000000100000000000000000000100100000001000000000000000000001001000000010000000000000000000010010000000100000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000011000000000000000000000000000000110000000000000000000000000000001100000000000000000000000000000011000000000000000000000000000000100100000001000000000000000000001001000000010000000000000000000010010000000100000000000000000000100100000001000000000000000000000000000000000000000000000111000100000000000000000000000000000000000000000000000000000000011100010000000000000000000000000000000000000000000000000000000001110001000000000000000000000000000000000000000000000000000000000111000100000000000000000000000000000000";
            var seqFile = util.LoadSequenceFile("SIO_PCIE_LPBK1_sequence.csv");
            Assert.IsFalse(seqFile.Count() == 0, "LoadSequenceFile returned an empty Dictionary.");
            Assert.IsTrue(seqFile.ContainsKey(seqID), $"Failed to populate key=[{seqID}] from SequenceFile.");

            var formatFile = util.LoadFormatFile("format_SIO_PCIE_LPBK1_merged.csv");
            Assert.IsTrue(formatFile.valid, "LoadFormatFile failed to parse file.");
            Assert.IsTrue(formatFile.data.Count == 5, $"LoadFormatFile saved [{formatFile.data.Count}] formats instead of 5.");

            var seqList = seqFile[seqID];
            var dataHash = util.HashBitStream(seqList, binNum);

            Assert.IsTrue(dataHash.Count() > 0, "Failed to Assign data to sequence.");

            List<string> outputList;
            int exitPort;
            var outputOk = util.GenerateOutput(formatFile, dataHash, string.Empty, out outputList, out exitPort);
            Assert.IsTrue(outputOk, "Failed to generate output.");

            foreach (var item in outputList)
            {
                Console.WriteLine($"OUTPUT:{item}");
            }

            Assert.IsTrue(exitPort == 1, $"ExitPort is {exitPort}");
            Assert.IsTrue(outputList.Count == 5, $"GenerateOutput returned [{outputList.Count}] lines, expected 5.");
            Assert.AreEqual(",pcie,-,1,,,,,,,,,,,,,,,,,,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1", outputList[0], $"Failed output[0]");
            Assert.AreEqual(",pcie,L0,,0,0,1,0,1,1,1,0,0,0,1,2,3,8,8,9,9,,,,,,,,,,,,,,,,,,,,,,,,,,,", outputList[1], $"Failed output[1]");
            Assert.AreEqual(",pcie,L1,,0,0,1,0,1,1,1,0,0,0,1,2,3,8,8,9,9,,,,,,,,,,,,,,,,,,,,,,,,,,,", outputList[2], $"Failed output[2]");
            Assert.AreEqual(",pcie,L2,,0,0,1,0,1,1,1,0,0,0,1,2,3,8,8,9,9,,,,,,,,,,,,,,,,,,,,,,,,,,,", outputList[3], $"Failed output[3]");
            Assert.AreEqual(",pcie,L3,,0,0,1,0,1,1,1,0,0,0,1,2,3,8,8,9,9,,,,,,,,,,,,,,,,,,,,,,,,,,,", outputList[4], $"Failed output[4]");
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void DisplayOutput_Test1()
        {
            var sio = new SIOLib(true);  // needs to be true for output to be displayed
            var util = new SIOEDC_Util(true);  // needs to be true for output to be displayed
            var seqID = "class_edc_g3";
            var binNum = "111111011100000000000000000000001000000000011111011111011111111010010000000100000000000000000000100100000001000000000000000000001001000000010000000000000000000010010000000100000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000011000000000000000000000000000000110000000000000000000000000000001100000000000000000000000000000011000000000000000000000000000000100100000001000000000000000000001001000000010000000000000000000010010000000100000000000000000000100100000001000000000000000000000000000000000000000000000111000100000000000000000000000000000000000000000000000000000000011100010000000000000000000000000000000000000000000000000000000001110001000000000000000000000000000000000000000000000000000000000111000100000000000000000000000000000000";
            var seqFile = util.LoadSequenceFile("SIO_PCIE_LPBK1_sequence.csv");
            Assert.IsFalse(seqFile.Count() == 0, "LoadSequenceFile returned an empty Dictionary.");
            Assert.IsTrue(seqFile.ContainsKey(seqID), $"Failed to populate key=[{seqID}] from SequenceFile.");

            var formatFile = util.LoadFormatFile("format_SIO_PCIE_LPBK1_merged.csv");
            Assert.IsTrue(formatFile.valid, "LoadFormatFile failed to parse file.");
            Assert.IsTrue(formatFile.data.Count == 5, $"LoadFormatFile saved [{formatFile.data.Count}] formats instead of 5.");

            var seqList = seqFile[seqID];
            var dataHash = util.HashBitStream(seqList, binNum);
            Assert.IsTrue(dataHash.Count() > 0, "Failed to Assign data to sequence.");

            List<string> outputList;
            int exitPort;
            var outputOk = util.GenerateOutput(formatFile, dataHash, string.Empty, out outputList, out exitPort);
            Assert.IsTrue(outputOk, "Failed to generate output.");
            Assert.IsTrue(exitPort == 1, $"ExitPort is {exitPort}");

            this.ConsoleOutput.Clear();
            var displayOk = util.DisplayOutput(formatFile.header, outputList);
            Assert.IsTrue(displayOk, "Failed to generate output.");

            var consoleExpect = @"[SIO_INFO] ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
[SIO_INFO] Port      LANE      cfg_cmn_dfx_cl_dfx_status_reg0_b21_b21___CMN_DFX_cl_dfx_status_reg_0__1  cfg_pcs_dfx_cri_errcnt_31_24___PCS_pcs_dword46__1  cfg_pcs_dfx_cri_errcnt___PCS_pcs_dword34__1  cfg_pcs_dfx_cri_lcerxtraincntdone___PCS_pcs_dword34__1  cfg_pcs_dfx_cri_lcetrainactive___PCS_pcs_dword34__1  cfg_pcs_dfx_cri_lcetraindone___PCS_pcs_dword34__1  cfg_pcs_dfx_cri_patchkactive___PCS_pcs_dword34__1  cfg_pcs_dfx_cri_patgenactive___PCS_pcs_dword34__1  cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__1  cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__2  cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__3  cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__1  cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__2  cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__3  cfg_pmd_lane_dsp_ro_rx_seq_ro_b11_b8___PMD_LANE_DSP_RO_DSP_RO_REG_19__1  cfg_pmd_lane_dsp_ro_rx_seq_ro_b11_b8___PMD_LANE_DSP_RO_DSP_RO_REG_19__2  cfg_pmd_lane_dsp_ro_rx_seq_ro_b3_b0___PMD_LANE_DSP_RO_DSP_RO_REG_19__1  cfg_pmd_lane_dsp_ro_rx_seq_ro_b3_b0___PMD_LANE_DSP_RO_DSP_RO_REG_19__2  crireset_l___tap_obs_cfg__3  first_comp_done_pll___tap_obs_cfg__8  o_phy_pmc_pmctrl_corepwr_stable___tap_obs_cfg__2  o_phy_pmc_pmctrl_corepwr_stable___tap_obs_cfg__4  o_phy_pmc_pmctrl_corepwr_stable___tap_obs_cfg__5  o_phy_pmc_pmctrl_corepwr_stable___tap_obs_cfg__6  o_phy_pmc_pmctrl_corepwr_stable___tap_obs_cfg__7  o_phy_pmc_pmctrl_corepwr_stable___tap_obs_cfg__8  oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__1  oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__2  oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__4  oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__5  oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__6  oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__7  oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__8  od_soc_pll1ok_h___tap_obs_cfg__6  od_soc_pll1ok_h___tap_obs_cfg__8  od_soc_pll2ok_h___tap_obs_cfg__7  od_soc_pll2ok_h___tap_obs_cfg__8  phy2pmc_sbpwr_stable_h___tap_obs_cfg__5  phy2pmc_sbpwr_stable_h___tap_obs_cfg__6  phy2pmc_sbpwr_stable_h___tap_obs_cfg__7  phy2pmc_sbpwr_stable_h___tap_obs_cfg__8  pll1_lock___tap_obs_cfg__6  pll1_lock___tap_obs_cfg__8  pll2_lock___tap_obs_cfg__7  pll2_lock___tap_obs_cfg__8
[SIO_INFO] ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
[SIO_INFO] pcie      -         1                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      1                            1                                     1                                                 1                                                 1                                                 1                                                 1                                                 1                                                 1                                           1                                           1                                           1                                           1                                           1                                           1                                           1                                 1                                 1                                 1                                 1                                        1                                        1                                        1                                        1                           1                           1                           1
[SIO_INFO] pcie      L0                                                                                 0                                                  0                                            1                                                       0                                                    1                                                  1                                                  1                                                  0                                                 0                                                 0                                                 1                                               2                                               3                                               8                                                                        8                                                                        9                                                                       9
[SIO_INFO] pcie      L1                                                                                 0                                                  0                                            1                                                       0                                                    1                                                  1                                                  1                                                  0                                                 0                                                 0                                                 1                                               2                                               3                                               8                                                                        8                                                                        9                                                                       9
[SIO_INFO] pcie      L2                                                                                 0                                                  0                                            1                                                       0                                                    1                                                  1                                                  1                                                  0                                                 0                                                 0                                                 1                                               2                                               3                                               8                                                                        8                                                                        9                                                                       9
[SIO_INFO] pcie      L3                                                                                 0                                                  0                                            1                                                       0                                                    1                                                  1                                                  1                                                  0                                                 0                                                 0                                                 1                                               2                                               3                                               8                                                                        8                                                                        9                                                                       9                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      ";
            var consoleExpectLines = consoleExpect.Split('\n');
            Assert.AreEqual(consoleExpectLines.Count() + 1, this.ConsoleOutput.Count(), "different number of lines in output"); /* Extra debug message at the beginning of the function */
            for (int i = 0; i < consoleExpectLines.Count(); i++)
            {
                Assert.AreEqual(consoleExpectLines[i].Trim(), this.ConsoleOutput[i + 1].Trim(), $"Failed at Line {i + 1}");
            }
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void DisplaySequenceData_Test1()
        {
            var sio = new SIOLib(true);  // needs to be true for output to be displayed
            var util = new SIOEDC_Util(true);  // needs to be true for output to be displayed
            var seqID = "class_edc_g3";
            var binNum = "111111011100000000000000000000001000000000011111011111011111111010010000000100000000000000000000100100000001000000000000000000001001000000010000000000000000000010010000000100000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000011000000000000000000000000000000110000000000000000000000000000001100000000000000000000000000000011000000000000000000000000000000100100000001000000000000000000001001000000010000000000000000000010010000000100000000000000000000100100000001000000000000000000000000000000000000000000000111000100000000000000000000000000000000000000000000000000000000011100010000000000000000000000000000000000000000000000000000000001110001000000000000000000000000000000000000000000000000000000000111000100000000000000000000000000000000";
            var seqFile = util.LoadSequenceFile("SIO_PCIE_LPBK1_sequence.csv");
            Assert.IsFalse(seqFile.Count() == 0, "LoadSequenceFile returned an empty Dictionary.");
            Assert.IsTrue(seqFile.ContainsKey(seqID), $"Failed to populate key=[{seqID}] from SequenceFile.");

            var formatFile = util.LoadFormatFile("format_SIO_PCIE_LPBK1_merged.csv");
            Assert.IsTrue(formatFile.valid, "LoadFormatFile failed to parse file.");
            Assert.IsTrue(formatFile.data.Count == 5, $"LoadFormatFile saved [{formatFile.data.Count}] formats instead of 5.");

            var seqList = seqFile[seqID];
            var dataHash = util.HashBitStream(seqList, binNum);
            Assert.IsTrue(dataHash.Count() > 0, "Failed to Assign data to sequence.");

            this.ConsoleOutput.Clear();
            var displayOk = util.DisplaySequenceData(seqList, seqID, dataHash);
            Assert.IsTrue(displayOk, "Failed to generate output.");

            var consoleExpect = @"[SIO_INFO] **
Sequence File Info for sequence id ""class_edc_g3""
PORT           LANE           NUMOFBITS      REGISTER                                          DATA
pcie           -              1              oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__1        1 (0x1)
pcie           -              1              o_phy_pmc_pmctrl_corepwr_stable___tap_obs_cfg__2  1 (0x1)
pcie           -              1              oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__2        1 (0x1)
pcie           -              1              crireset_l___tap_obs_cfg__3                       1 (0x1)
pcie           -              1              o_phy_pmc_pmctrl_corepwr_stable___tap_obs_cfg__4  1 (0x1)
pcie           -              1              oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__4        1 (0x1)
pcie           -              1              reserved___tap_obs_cfg__4                         0 (0x0)
pcie           -              1              phy2pmc_sbpwr_stable_h___tap_obs_cfg__5           1 (0x1)
pcie           -              1              o_phy_pmc_pmctrl_corepwr_stable___tap_obs_cfg__5  1 (0x1)
pcie           -              1              oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__5        1 (0x1)
pcie           -              1              reserved___tap_obs_cfg__5                         0 (0x0)
pcie           -              21             cfg_cmn_dfx_cl_dfx_status_reg0_b20_b0___CMN_DFX_cl_dfx_status_reg_0__1000000000000000000000 (0x0)
pcie           -              1              cfg_cmn_dfx_cl_dfx_status_reg0_b21_b21___CMN_DFX_cl_dfx_status_reg_0__11 (0x1)
pcie           -              10             cfg_cmn_dfx_cl_dfx_status_reg0_b31_b22___CMN_DFX_cl_dfx_status_reg_0__10000000000 (0x0)
pcie           -              1              od_soc_pll1ok_h___tap_obs_cfg__6                  1 (0x1)
pcie           -              1              pll1_lock___tap_obs_cfg__6                        1 (0x1)
pcie           -              1              phy2pmc_sbpwr_stable_h___tap_obs_cfg__6           1 (0x1)
pcie           -              1              o_phy_pmc_pmctrl_corepwr_stable___tap_obs_cfg__6  1 (0x1)
pcie           -              1              oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__6        1 (0x1)
pcie           -              1              reserved___tap_obs_cfg__6                         0 (0x0)
pcie           -              1              od_soc_pll2ok_h___tap_obs_cfg__7                  1 (0x1)
pcie           -              1              pll2_lock___tap_obs_cfg__7                        1 (0x1)
pcie           -              1              phy2pmc_sbpwr_stable_h___tap_obs_cfg__7           1 (0x1)
pcie           -              1              o_phy_pmc_pmctrl_corepwr_stable___tap_obs_cfg__7  1 (0x1)
pcie           -              1              oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__7        1 (0x1)
pcie           -              1              reserved___tap_obs_cfg__7                         0 (0x0)
pcie           -              1              od_soc_pll1ok_h___tap_obs_cfg__8                  1 (0x1)
pcie           -              1              od_soc_pll2ok_h___tap_obs_cfg__8                  1 (0x1)
pcie           -              1              pll1_lock___tap_obs_cfg__8                        1 (0x1)
pcie           -              1              pll2_lock___tap_obs_cfg__8                        1 (0x1)
pcie           -              1              first_comp_done_pll___tap_obs_cfg__8              1 (0x1)
pcie           -              1              phy2pmc_sbpwr_stable_h___tap_obs_cfg__8           1 (0x1)
pcie           -              1              o_phy_pmc_pmctrl_corepwr_stable___tap_obs_cfg__8  1 (0x1)
pcie           -              1              oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__8        1 (0x1)
pcie           -              1              reserved___tap_obs_cfg__8                         0 (0x0)
pcie           L0             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b3_b0___PMD_LANE_DSP_RO_DSP_RO_REG_19__11001 (0x9)
pcie           L0             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b7_b4___PMD_LANE_DSP_RO_DSP_RO_REG_19__10000 (0x0)
pcie           L0             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b11_b8___PMD_LANE_DSP_RO_DSP_RO_REG_19__11000 (0x8)
pcie           L0             20             cfg_pmd_lane_dsp_ro_rx_seq_ro_b31_b12___PMD_LANE_DSP_RO_DSP_RO_REG_19__100000000000000000000 (0x0)
pcie           L3             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b3_b0___PMD_LANE_DSP_RO_DSP_RO_REG_19__11001 (0x9)
pcie           L3             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b7_b4___PMD_LANE_DSP_RO_DSP_RO_REG_19__10000 (0x0)
pcie           L3             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b11_b8___PMD_LANE_DSP_RO_DSP_RO_REG_19__11000 (0x8)
pcie           L3             20             cfg_pmd_lane_dsp_ro_rx_seq_ro_b31_b12___PMD_LANE_DSP_RO_DSP_RO_REG_19__100000000000000000000 (0x0)
pcie           L2             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b3_b0___PMD_LANE_DSP_RO_DSP_RO_REG_19__11001 (0x9)
pcie           L2             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b7_b4___PMD_LANE_DSP_RO_DSP_RO_REG_19__10000 (0x0)
pcie           L2             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b11_b8___PMD_LANE_DSP_RO_DSP_RO_REG_19__11000 (0x8)
pcie           L2             20             cfg_pmd_lane_dsp_ro_rx_seq_ro_b31_b12___PMD_LANE_DSP_RO_DSP_RO_REG_19__100000000000000000000 (0x0)
pcie           L1             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b3_b0___PMD_LANE_DSP_RO_DSP_RO_REG_19__11001 (0x9)
pcie           L1             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b7_b4___PMD_LANE_DSP_RO_DSP_RO_REG_19__10000 (0x0)
pcie           L1             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b11_b8___PMD_LANE_DSP_RO_DSP_RO_REG_19__11000 (0x8)
pcie           L1             20             cfg_pmd_lane_dsp_ro_rx_seq_ro_b31_b12___PMD_LANE_DSP_RO_DSP_RO_REG_19__100000000000000000000 (0x0)
pcie           L0             5              cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__1    00001 (0x1)
pcie           L0             9              cfg_pcs_dfx_status1_b13_b5___PCS_pcs_dword60__1   000000000 (0x0)
pcie           L0             1              cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__1  0 (0x0)
pcie           L0             17             cfg_pcs_dfx_status1_b31_b15___PCS_pcs_dword60__1  00000000000000000 (0x0)
pcie           L3             5              cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__1    00001 (0x1)
pcie           L3             9              cfg_pcs_dfx_status1_b13_b5___PCS_pcs_dword60__1   000000000 (0x0)
pcie           L3             1              cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__1  0 (0x0)
pcie           L3             17             cfg_pcs_dfx_status1_b31_b15___PCS_pcs_dword60__1  00000000000000000 (0x0)
pcie           L2             5              cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__1    00001 (0x1)
pcie           L2             9              cfg_pcs_dfx_status1_b13_b5___PCS_pcs_dword60__1   000000000 (0x0)
pcie           L2             1              cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__1  0 (0x0)
pcie           L2             17             cfg_pcs_dfx_status1_b31_b15___PCS_pcs_dword60__1  00000000000000000 (0x0)
pcie           L1             5              cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__1    00001 (0x1)
pcie           L1             9              cfg_pcs_dfx_status1_b13_b5___PCS_pcs_dword60__1   000000000 (0x0)
pcie           L1             1              cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__1  0 (0x0)
pcie           L1             17             cfg_pcs_dfx_status1_b31_b15___PCS_pcs_dword60__1  00000000000000000 (0x0)
pcie           L0             5              cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__2    00010 (0x2)
pcie           L0             9              cfg_pcs_dfx_status1_b13_b5___PCS_pcs_dword60__2   000000000 (0x0)
pcie           L0             1              cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__2  0 (0x0)
pcie           L0             17             cfg_pcs_dfx_status1_b31_b15___PCS_pcs_dword60__2  00000000000000000 (0x0)
pcie           L1             5              cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__2    00010 (0x2)
pcie           L1             9              cfg_pcs_dfx_status1_b13_b5___PCS_pcs_dword60__2   000000000 (0x0)
pcie           L1             1              cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__2  0 (0x0)
pcie           L1             17             cfg_pcs_dfx_status1_b31_b15___PCS_pcs_dword60__2  00000000000000000 (0x0)
pcie           L2             5              cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__2    00010 (0x2)
pcie           L2             9              cfg_pcs_dfx_status1_b13_b5___PCS_pcs_dword60__2   000000000 (0x0)
pcie           L2             1              cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__2  0 (0x0)
pcie           L2             17             cfg_pcs_dfx_status1_b31_b15___PCS_pcs_dword60__2  00000000000000000 (0x0)
pcie           L3             5              cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__2    00010 (0x2)
pcie           L3             9              cfg_pcs_dfx_status1_b13_b5___PCS_pcs_dword60__2   000000000 (0x0)
pcie           L3             1              cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__2  0 (0x0)
pcie           L3             17             cfg_pcs_dfx_status1_b31_b15___PCS_pcs_dword60__2  00000000000000000 (0x0)
pcie           L0             5              cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__3    00011 (0x3)
pcie           L0             9              cfg_pcs_dfx_status1_b13_b5___PCS_pcs_dword60__3   000000000 (0x0)
pcie           L0             1              cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__3  0 (0x0)
pcie           L0             17             cfg_pcs_dfx_status1_b31_b15___PCS_pcs_dword60__3  00000000000000000 (0x0)
pcie           L3             5              cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__3    00011 (0x3)
pcie           L3             9              cfg_pcs_dfx_status1_b13_b5___PCS_pcs_dword60__3   000000000 (0x0)
pcie           L3             1              cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__3  0 (0x0)
pcie           L3             17             cfg_pcs_dfx_status1_b31_b15___PCS_pcs_dword60__3  00000000000000000 (0x0)
pcie           L2             5              cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__3    00011 (0x3)
pcie           L2             9              cfg_pcs_dfx_status1_b13_b5___PCS_pcs_dword60__3   000000000 (0x0)
pcie           L2             1              cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__3  0 (0x0)
pcie           L2             17             cfg_pcs_dfx_status1_b31_b15___PCS_pcs_dword60__3  00000000000000000 (0x0)
pcie           L1             5              cfg_pcs_dfx_status1_b4_b0___PCS_pcs_dword60__3    00011 (0x3)
pcie           L1             9              cfg_pcs_dfx_status1_b13_b5___PCS_pcs_dword60__3   000000000 (0x0)
pcie           L1             1              cfg_pcs_dfx_status1_b14_b14___PCS_pcs_dword60__3  0 (0x0)
pcie           L1             17             cfg_pcs_dfx_status1_b31_b15___PCS_pcs_dword60__3  00000000000000000 (0x0)
pcie           L0             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b3_b0___PMD_LANE_DSP_RO_DSP_RO_REG_19__21001 (0x9)
pcie           L0             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b7_b4___PMD_LANE_DSP_RO_DSP_RO_REG_19__20000 (0x0)
pcie           L0             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b11_b8___PMD_LANE_DSP_RO_DSP_RO_REG_19__21000 (0x8)
pcie           L0             20             cfg_pmd_lane_dsp_ro_rx_seq_ro_b31_b12___PMD_LANE_DSP_RO_DSP_RO_REG_19__200000000000000000000 (0x0)
pcie           L3             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b3_b0___PMD_LANE_DSP_RO_DSP_RO_REG_19__21001 (0x9)
pcie           L3             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b7_b4___PMD_LANE_DSP_RO_DSP_RO_REG_19__20000 (0x0)
pcie           L3             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b11_b8___PMD_LANE_DSP_RO_DSP_RO_REG_19__21000 (0x8)
pcie           L3             20             cfg_pmd_lane_dsp_ro_rx_seq_ro_b31_b12___PMD_LANE_DSP_RO_DSP_RO_REG_19__200000000000000000000 (0x0)
pcie           L2             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b3_b0___PMD_LANE_DSP_RO_DSP_RO_REG_19__21001 (0x9)
pcie           L2             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b7_b4___PMD_LANE_DSP_RO_DSP_RO_REG_19__20000 (0x0)
pcie           L2             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b11_b8___PMD_LANE_DSP_RO_DSP_RO_REG_19__21000 (0x8)
pcie           L2             20             cfg_pmd_lane_dsp_ro_rx_seq_ro_b31_b12___PMD_LANE_DSP_RO_DSP_RO_REG_19__200000000000000000000 (0x0)
pcie           L1             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b3_b0___PMD_LANE_DSP_RO_DSP_RO_REG_19__21001 (0x9)
pcie           L1             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b7_b4___PMD_LANE_DSP_RO_DSP_RO_REG_19__20000 (0x0)
pcie           L1             4              cfg_pmd_lane_dsp_ro_rx_seq_ro_b11_b8___PMD_LANE_DSP_RO_DSP_RO_REG_19__21000 (0x8)
pcie           L1             20             cfg_pmd_lane_dsp_ro_rx_seq_ro_b31_b12___PMD_LANE_DSP_RO_DSP_RO_REG_19__200000000000000000000 (0x0)
pcie           L0             24             cfg_pcs_dfx_cri_errcnt___PCS_pcs_dword34__1       000000000000000000000000 (0x0)
pcie           L0             1              cfg_pcs_dfx_cri_lcetrainactive___PCS_pcs_dword34__10 (0x0)
pcie           L0             1              cfg_pcs_dfx_cri_lcetraindone___PCS_pcs_dword34__1 1 (0x1)
pcie           L0             1              cfg_pcs_dfx_cri_patgenactive___PCS_pcs_dword34__1 1 (0x1)
pcie           L0             1              cfg_pcs_dfx_cri_patchkactive___PCS_pcs_dword34__1 1 (0x1)
pcie           L0             1              cfg_pcs_dfx_cri_patbufallfail___PCS_pcs_dword34__10 (0x0)
pcie           L0             1              cfg_pcs_reserved___PCS_pcs_dword34__1             0 (0x0)
pcie           L0             1              cfg_pcs_dfx_cri_lcemgnerr___PCS_pcs_dword34__1    0 (0x0)
pcie           L0             1              cfg_pcs_dfx_cri_lcerxtraincntdone___PCS_pcs_dword34__11 (0x1)
pcie           L0             1              cfg_pcs_rx_init_done_rcvd___PCS_pcs_dword46__1    0 (0x0)
pcie           L0             1              cfg_pcs_tx_init_done_rcvd___PCS_pcs_dword46__1    0 (0x0)
pcie           L0             1              cfg_pcs_rx_powerchange_complete___PCS_pcs_dword46__10 (0x0)
pcie           L0             1              cfg_pcs_tx_powerchange_complete___PCS_pcs_dword46__10 (0x0)
pcie           L0             1              cfg_pcs_rxstandbystatus___PCS_pcs_dword46__1      0 (0x0)
pcie           L0             11             cfg_pcs_reserved___PCS_pcs_dword46__1             00000000000 (0x0)
pcie           L0             8              cfg_pcs_dfx_cri_errcnt_31_24___PCS_pcs_dword46__1 00000000 (0x0)
pcie           L0             8              cfg_pcs_i_cri_indx41_1___PCS_pcs_dword46__1       00000000 (0x0)
pcie           L1             24             cfg_pcs_dfx_cri_errcnt___PCS_pcs_dword34__1       000000000000000000000000 (0x0)
pcie           L1             1              cfg_pcs_dfx_cri_lcetrainactive___PCS_pcs_dword34__10 (0x0)
pcie           L1             1              cfg_pcs_dfx_cri_lcetraindone___PCS_pcs_dword34__1 1 (0x1)
pcie           L1             1              cfg_pcs_dfx_cri_patgenactive___PCS_pcs_dword34__1 1 (0x1)
pcie           L1             1              cfg_pcs_dfx_cri_patchkactive___PCS_pcs_dword34__1 1 (0x1)
pcie           L1             1              cfg_pcs_dfx_cri_patbufallfail___PCS_pcs_dword34__10 (0x0)
pcie           L1             1              cfg_pcs_reserved___PCS_pcs_dword34__1             0 (0x0)
pcie           L1             1              cfg_pcs_dfx_cri_lcemgnerr___PCS_pcs_dword34__1    0 (0x0)
pcie           L1             1              cfg_pcs_dfx_cri_lcerxtraincntdone___PCS_pcs_dword34__11 (0x1)
pcie           L1             1              cfg_pcs_rx_init_done_rcvd___PCS_pcs_dword46__1    0 (0x0)
pcie           L1             1              cfg_pcs_tx_init_done_rcvd___PCS_pcs_dword46__1    0 (0x0)
pcie           L1             1              cfg_pcs_rx_powerchange_complete___PCS_pcs_dword46__10 (0x0)
pcie           L1             1              cfg_pcs_tx_powerchange_complete___PCS_pcs_dword46__10 (0x0)
pcie           L1             1              cfg_pcs_rxstandbystatus___PCS_pcs_dword46__1      0 (0x0)
pcie           L1             11             cfg_pcs_reserved___PCS_pcs_dword46__1             00000000000 (0x0)
pcie           L1             8              cfg_pcs_dfx_cri_errcnt_31_24___PCS_pcs_dword46__1 00000000 (0x0)
pcie           L1             8              cfg_pcs_i_cri_indx41_1___PCS_pcs_dword46__1       00000000 (0x0)
pcie           L2             24             cfg_pcs_dfx_cri_errcnt___PCS_pcs_dword34__1       000000000000000000000000 (0x0)
pcie           L2             1              cfg_pcs_dfx_cri_lcetrainactive___PCS_pcs_dword34__10 (0x0)
pcie           L2             1              cfg_pcs_dfx_cri_lcetraindone___PCS_pcs_dword34__1 1 (0x1)
pcie           L2             1              cfg_pcs_dfx_cri_patgenactive___PCS_pcs_dword34__1 1 (0x1)
pcie           L2             1              cfg_pcs_dfx_cri_patchkactive___PCS_pcs_dword34__1 1 (0x1)
pcie           L2             1              cfg_pcs_dfx_cri_patbufallfail___PCS_pcs_dword34__10 (0x0)
pcie           L2             1              cfg_pcs_reserved___PCS_pcs_dword34__1             0 (0x0)
pcie           L2             1              cfg_pcs_dfx_cri_lcemgnerr___PCS_pcs_dword34__1    0 (0x0)
pcie           L2             1              cfg_pcs_dfx_cri_lcerxtraincntdone___PCS_pcs_dword34__11 (0x1)
pcie           L2             1              cfg_pcs_rx_init_done_rcvd___PCS_pcs_dword46__1    0 (0x0)
pcie           L2             1              cfg_pcs_tx_init_done_rcvd___PCS_pcs_dword46__1    0 (0x0)
pcie           L2             1              cfg_pcs_rx_powerchange_complete___PCS_pcs_dword46__10 (0x0)
pcie           L2             1              cfg_pcs_tx_powerchange_complete___PCS_pcs_dword46__10 (0x0)
pcie           L2             1              cfg_pcs_rxstandbystatus___PCS_pcs_dword46__1      0 (0x0)
pcie           L2             11             cfg_pcs_reserved___PCS_pcs_dword46__1             00000000000 (0x0)
pcie           L2             8              cfg_pcs_dfx_cri_errcnt_31_24___PCS_pcs_dword46__1 00000000 (0x0)
pcie           L2             8              cfg_pcs_i_cri_indx41_1___PCS_pcs_dword46__1       00000000 (0x0)
pcie           L3             24             cfg_pcs_dfx_cri_errcnt___PCS_pcs_dword34__1       000000000000000000000000 (0x0)
pcie           L3             1              cfg_pcs_dfx_cri_lcetrainactive___PCS_pcs_dword34__10 (0x0)
pcie           L3             1              cfg_pcs_dfx_cri_lcetraindone___PCS_pcs_dword34__1 1 (0x1)
pcie           L3             1              cfg_pcs_dfx_cri_patgenactive___PCS_pcs_dword34__1 1 (0x1)
pcie           L3             1              cfg_pcs_dfx_cri_patchkactive___PCS_pcs_dword34__1 1 (0x1)
pcie           L3             1              cfg_pcs_dfx_cri_patbufallfail___PCS_pcs_dword34__10 (0x0)
pcie           L3             1              cfg_pcs_reserved___PCS_pcs_dword34__1             0 (0x0)
pcie           L3             1              cfg_pcs_dfx_cri_lcemgnerr___PCS_pcs_dword34__1    0 (0x0)
pcie           L3             1              cfg_pcs_dfx_cri_lcerxtraincntdone___PCS_pcs_dword34__11 (0x1)
pcie           L3             1              cfg_pcs_rx_init_done_rcvd___PCS_pcs_dword46__1    0 (0x0)
pcie           L3             1              cfg_pcs_tx_init_done_rcvd___PCS_pcs_dword46__1    0 (0x0)
pcie           L3             1              cfg_pcs_rx_powerchange_complete___PCS_pcs_dword46__10 (0x0)
pcie           L3             1              cfg_pcs_tx_powerchange_complete___PCS_pcs_dword46__10 (0x0)
pcie           L3             1              cfg_pcs_rxstandbystatus___PCS_pcs_dword46__1      0 (0x0)
pcie           L3             11             cfg_pcs_reserved___PCS_pcs_dword46__1             00000000000 (0x0)
pcie           L3             8              cfg_pcs_dfx_cri_errcnt_31_24___PCS_pcs_dword46__1 00000000 (0x0)
pcie           L3             8              cfg_pcs_i_cri_indx41_1___PCS_pcs_dword46__1       00000000 (0x0)
**";
            var consoleExpectLines = consoleExpect.Split('\n');

            Assert.IsTrue(this.ConsoleOutput.Count() > 1); // the output is printed as a single string
            var consoleActualLines = this.ConsoleOutput[this.ConsoleOutput.Count - 1].Split('\n'); /* Extra debug message at the beginning of the function */

            Assert.AreEqual(consoleExpectLines.Count(), consoleActualLines.Count());
            for (int i = 0; i < consoleExpectLines.Count(); i++)
            {
                Assert.AreEqual(consoleExpectLines[i].Trim(), consoleActualLines[i].Trim(), $"Failed at Line {i + 1}");
            }
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void TestFullSequence1()
        {
            var edc = new SIOEDC("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB", false);
            var captureData = new Dictionary<string, string>
            {
                { "TDO", "111111011100000000000000000000001000000000011111011111011111111010010000000100000000000000000000100100000001000000000000000000001001000000010000000000000000000010010000000100000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000011000000000000000000000000000000110000000000000000000000000000001100000000000000000000000000000011000000000000000000000000000000100100000001000000000000000000001001000000010000000000000000000010010000000100000000000000000000100100000001000000000000000000000000000000000000000000000111000100000000000000000000000000000000000000000000000000000000011100010000000000000000000000000000000000000000000000000000000001110001000000000000000000000000000000000000000000000000000000000111000100000000000000000000000000000000" },
            };

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var ituffWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            ituffWriterMock.Setup(o => o.SetCustomTname("REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB_EDC_CAPTURE_L0_P0"));
            ituffWriterMock.Setup(o => o.SetDelimiterCharacterForWrap('!'));
            ituffWriterMock.Setup(o => o.SetData("TOKEN=RUN:0!Plist=CMEM_pcie_8b_com_bcast_prbs7_dnelb_Gen1__class_list!RUN=1!CmpName=NA!TestType=CAPTURE!DataStart#}9>?^+10*9'~E9<9]9;9]9;9A+9(9:9A`9&9[9/0*9'~@99.Q99A:R99~-99~%9)#DataEnd!KEY=9AAAA|8AAEA|1AAAJ|0AEAA|*ACIB|'SAI|~AAA|}A7X|?D56|^75E|]AAI|[ABQ|@EQC|=AHC|>EA|+AQ|<AB|;AC|:AD|`AY|/AJ|.BY|-HC|)AA|(AE|&AG|%OE"));

            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(ituffWriterMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(ituffWriterMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            edc.SetupEDCLog("SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt", "PCIE_CALCODE_DNELB_GEN1");
            var exitPort = edc.RunEDCLog(captureData, "CMEM_pcie_8b_com_bcast_prbs7_dnelb_Gen1__class_list", "TDO");
            Assert.AreEqual(1, exitPort, $"Failed Exit port=[{exitPort}].");
            datalogServiceMock.VerifyAll();
            ituffWriterMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void Execute_Pass_ReuseSavedMem()
        {
            Console.WriteLine("Now running Execute_Pass_ReuseSavedMem");
            var captureCtvPerPinTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(f => f.CreateCaptureCtvPerPinTest("FakePList", "FakeLevels", "FakeTiming", new List<string> { "TDO" }, It.IsAny<string>())).Returns(captureCtvPerPinTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var ituffWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            ituffWriterMock.Setup(o => o.SetCustomTname("SIO_PCIE_LPBK1::REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB_EDC_CAPTURE_L0_P0"));
            ituffWriterMock.Setup(o => o.SetDelimiterCharacterForWrap('!'));
            ituffWriterMock.Setup(o => o.SetData("TOKEN=RUN:0!Plist=FakePList!RUN=1!CmpName=NA!TestType=CAPTURE!DataStart#}9>?^+10*9'~E9<9]9;9]9;9A+9(9:9A`9&9[9/0*9'~@99.Q99A:R99~-99~%9)#DataEnd!KEY=9AAAA|8AAEA|1AAAJ|0AEAA|*ACIB|'SAI|~AAA|}A7X|?D56|^75E|]AAI|[ABQ|@EQC|=AHC|>EA|+AQ|<AB|;AC|:AD|`AY|/AJ|.BY|-HC|)AA|(AE|&AG|%OE"));

            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(ituffWriterMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(ituffWriterMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            this.GSDSValues["SIO_SAVED_CAPTMEM"] = "111111011100000000000000000000001000000000011111011111011111111010010000000100000000000000000000100100000001000000000000000000001001000000010000000000000000000010010000000100000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000000100000000000000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000000011000000000000000000000000000000110000000000000000000000000000001100000000000000000000000000000011000000000000000000000000000000100100000001000000000000000000001001000000010000000000000000000010010000000100000000000000000000100100000001000000000000000000000000000000000000000000000111000100000000000000000000000000000000000000000000000000000000011100010000000000000000000000000000000000000000000000000000000001110001000000000000000000000000000000000000000000000000000000000111000100000000000000000000000000000000";
            SIOEDCLogTC underTest = new SIOEDCLogTC
            {
                InstanceName = "SIO_PCIE_LPBK1::REUTAFELB_PRBS_CMEM_K_END_X_X_VMAX_2500_DNELB",
                BypassPort = "-1",
                LogLevel = TestMethodBase.PrimeLogLevel.PRIME_DEBUG,
                TimingsTc = "FakeTiming",
                LevelsTc = "FakeLevels",
                Patlist = "FakePList",
                CtvCapturePins = "TDO",
                UserFile = "SIO_PCIE_LPBK1_tgl_pcie_mv_setup.txt",
                UserToken = "PCIE_CALCODE_DNELB_GEN1",
                ReuseCaptMemGlobal = "G.U.S.SIO_SAVED_CAPTMEM",
            };
            underTest.TestMethodExtension = underTest;

            underTest.Verify();
            underTest.CustomVerify(); // FIXME - why isn't this called automatically in the unit test like in real life??

            var exitPort = underTest.Execute();
            Assert.AreEqual(1, exitPort);
            funcServiceMock.VerifyAll();
            captureCtvPerPinTestMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            ituffWriterMock.VerifyAll();
        }

        private static string GetPathToFiles([CallerFilePath] string srcPath = "")
        {
            return Path.GetDirectoryName(srcPath) + "\\input_files\\";
        }
    }
}
