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

namespace SIOCommon.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.FileService;
    using SIO;

    /// <summary>
    /// Defines the <see cref="SIOEDC_Util_UnitTest" />.
    /// </summary>
    [TestClass]
    public class SIOEDC_Util_UnitTest
    {
        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        /// <summary>
        /// Setup the common mocks for all the tests.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => Console.WriteLine(s));
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => Console.WriteLine(msg));
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;
        }

        /// <summary>
        /// simple unit test.
        /// </summary>
        [TestMethod]
        public void HashedData_Get_Exceptions()
        {
            var data = new SIOEDC_Util.HashedData();
            Assert.ThrowsException<KeyNotFoundException>(() => data.Get("port", "lane", "signal"));
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void MessageAlways_Pass()
        {
            var utils = new SIOEDC_Util(true);
            utils.MsgToConsole(MsgEnum.SIO_ALWAYS, "SomeMessage");
            this.ConsoleServiceMock.Verify(o => o.PrintError("[SIO_ALWAYS] SomeMessage", 0, " ", " "), Times.Once);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void LoadFormatFile_BadFormat_Fail()
        {
            this.TestInvalidFormatFile("./format_invalidnumofelements.csv", "Invalid number of elements in Line=[pcie,-] at lineNum=[3] File=[./format_invalidnumofelements.csv], Expected at least 3, got [2].");
            this.TestInvalidFormatFile("./format_invalidbitrange.csv", "Invalid Line at lineNum=[2] File=[./format_invalidbitrange.csv] Expecting Integers in bitrange got [NAN-0]");
            this.TestInvalidFormatFile("./format_invalidexitport.csv", "Invalid Line at lineNum=[2] File=[./format_invalidexitport.csv] Expecting an integer for exit_port, got [NAN]");
            this.TestInvalidFormatFile("./format_invalidrangeforbin.csv", "Invalid Line at lineNum=[2] File=[./format_invalidrangeforbin.csv] Data format cannot be bin or hex when upper/lower limits are specified.");
            this.TestInvalidFormatFile("./format_invalidrange.csv", "Invalid Line at lineNum=[2] File=[./format_invalidrange.csv] Expecting an integer for lowlimit,highlimit and exit_port, got [3,NAN,0]");
            this.TestInvalidFormatFile("./format_toomanyfields.csv", "WARNING: Invalid Line at lineNum=[2] File=[./format_toomanyfields.csv] Expecting 3, 5, or 6 elements, got [11] in [oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__1:0-0:dec:3:0:x:y:z:t:u:v]", true);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void LoadFormatFile_Pass()
        {
            var fileServiceMock = this.MockFileService("./testfileparser_format.csv");

            var utils = new SIOEDC_Util(true);
            var formats = utils.LoadFormatFile("./testfileparser_format.csv");
            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void LoadSequenceFile_BadFormat_Fail()
        {
            this.TestInvalidSeqenceFile("./sequence_toomanyelements.csv", "Invalid number of elements in Line=[class_edc_g3,pcie,-,-,4,oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__1,7,8] at lineNum=[2] File=[./sequence_toomanyelements.csv], Expected 6, got [8].");
            this.TestInvalidSeqenceFile("./sequence_invalidformatbitsnotinteger.csv", "Invalid Line=[class_edc_g3,pcie,-,-,NAN,oa_phy_pmc_phy_pwr_stable___tap_obs_cfg__1] at lineNum=[2] File=[./sequence_invalidformatbitsnotinteger.csv] Expecting a number for Bits, not [NAN]");
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void LoadSequenceFile_Pass()
        {
            var fileServiceMock = this.MockFileService("./sequence_testcomments.csv");

            var utils = new SIOEDC_Util(true);
            var sequence = utils.LoadSequenceFile("./sequence_testcomments.csv");
            Assert.AreNotEqual(0, sequence.Count);
            fileServiceMock.VerifyAll();
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void HashBitStream_WrongSize_Fail()
        {
            var utils = new SIOEDC_Util(true);
            var hash = utils.HashBitStream(new List<SIOEDC_Util.SIOSequence>(), "0000");
            this.ConsoleServiceMock.Verify(o => o.PrintError("Size of Captured data [4] is not equal to size [0] defined in sequence file. Exit out of port 0", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual(0, hash.Count());
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void GenerateOutput_TestAllFormats_Pass()
        {
            var format = new SIOEDC_Util.SIOFormat("Port1", "Lane1");
            format.fields = new List<SIOEDC_Util.SIOFormatField>();
            foreach (var dataformat in new List<string> { "dec", "2cdec", "gray2dec", "mcd14_gray2dec", "sgray2dec", "sdec", "hex", "bin", "gray2bin", string.Empty })
            {
                var field = new SIOEDC_Util.SIOFormatField("signal1");
                field.port = "Port1";
                field.lane = "Lane1";
                field.dataFormat = dataformat;
                format.fields.Add(field);
            }

            var formats = new SIOEDC_Util.SIOFormatFile();
            formats.valid = true;
            formats.header = "someheader";
            formats.data = new List<SIOEDC_Util.SIOFormat> { format };

            var dataHash = new SIOEDC_Util.HashedData();
            dataHash.Add("Port1", "Lane1", "signal1", "10000000");

            var utils = new SIOEDC_Util(true);
            Assert.IsTrue(utils.GenerateOutput(formats, dataHash, string.Empty, out var outputList, out var exitPort));
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void GenerateOutput_ExpectLowHigh_Pass()
        {
            var format = new SIOEDC_Util.SIOFormat("Port1", "Lane1");
            format.fields = new List<SIOEDC_Util.SIOFormatField>();
            var field = new SIOEDC_Util.SIOFormatField("signal1");
            field.port = "Port1";
            field.lane = "Lane1";
            field.dataFormat = "dec";
            field.expectLowLimit = 1;
            field.expectHighLimit = 5;
            field.exitPort = -2;
            format.fields.Add(field);

            var formats = new SIOEDC_Util.SIOFormatFile();
            formats.valid = true;
            formats.header = "someheader";
            formats.data = new List<SIOEDC_Util.SIOFormat> { format };

            var dataHash = new SIOEDC_Util.HashedData();
            dataHash.Add("Port1", "Lane1", "signal1", "0000");

            var utils = new SIOEDC_Util(true);
            Assert.IsTrue(utils.GenerateOutput(formats, dataHash, string.Empty, out var outputList, out var exitPort));
            Assert.AreEqual(-2, exitPort);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void GenerateOutput_ExpecExact_Pass()
        {
            var format = new SIOEDC_Util.SIOFormat("Port1", "Lane1");
            format.fields = new List<SIOEDC_Util.SIOFormatField>();
            var field = new SIOEDC_Util.SIOFormatField("signal1");
            field.port = "Port1";
            field.lane = "Lane1";
            field.dataFormat = "dec";
            field.expectExactVal = "1";
            field.exitPort = -2;
            format.fields.Add(field);

            var formats = new SIOEDC_Util.SIOFormatFile();
            formats.valid = true;
            formats.header = "someheader";
            formats.data = new List<SIOEDC_Util.SIOFormat> { format };

            var dataHash = new SIOEDC_Util.HashedData();
            dataHash.Add("Port1", "Lane1", "signal1", "0000");

            var utils = new SIOEDC_Util(true);
            Assert.IsTrue(utils.GenerateOutput(formats, dataHash, string.Empty, out var outputList, out var exitPort));
            Assert.AreEqual(-2, exitPort);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void GenerateOutput_ExpecExactNaN_Fail()
        {
            var format = new SIOEDC_Util.SIOFormat("Port1", "Lane1");
            format.fields = new List<SIOEDC_Util.SIOFormatField>();
            var field = new SIOEDC_Util.SIOFormatField("signal1");
            field.port = "Port1";
            field.lane = "Lane1";
            field.dataFormat = "dec";
            field.expectExactVal = "NotANumber";
            field.exitPort = -2;
            format.fields.Add(field);

            var formats = new SIOEDC_Util.SIOFormatFile();
            formats.valid = true;
            formats.header = "someheader";
            formats.data = new List<SIOEDC_Util.SIOFormat> { format };

            var dataHash = new SIOEDC_Util.HashedData();
            dataHash.Add("Port1", "Lane1", "signal1", "0000");

            var utils = new SIOEDC_Util(true);
            Assert.IsTrue(utils.GenerateOutput(formats, dataHash, string.Empty, out var outputList, out var exitPort));
            this.ConsoleServiceMock.Verify(o => o.PrintError("Error in format file. Value=[NotANumber] to match must be integer for PORT:\"Port1\", Lane:\"Lane1\", FieldName:\"signal1\". Exiting out of ExitPort \"-2\" ", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual(-2, exitPort);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void GenerateOutput_ExpecExactNonInt_Pass()
        {
            var format = new SIOEDC_Util.SIOFormat("Port1", "Lane1");
            format.fields = new List<SIOEDC_Util.SIOFormatField>();
            var field = new SIOEDC_Util.SIOFormatField("signal1");
            field.port = "Port1";
            field.lane = "Lane1";
            field.dataFormat = "bin";
            field.expectExactVal = "0001";
            field.exitPort = -2;
            format.fields.Add(field);

            var formats = new SIOEDC_Util.SIOFormatFile();
            formats.valid = true;
            formats.header = "someheader";
            formats.data = new List<SIOEDC_Util.SIOFormat> { format };

            var dataHash = new SIOEDC_Util.HashedData();
            dataHash.Add("Port1", "Lane1", "signal1", "0000");

            var utils = new SIOEDC_Util(true);
            Assert.IsTrue(utils.GenerateOutput(formats, dataHash, string.Empty, out var outputList, out var exitPort));
            Assert.AreEqual(-2, exitPort);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void GenerateOutput_InvalidFormats_Pass()
        {
            var format = new SIOEDC_Util.SIOFormat("Port1", "Lane1");
            format.fields = new List<SIOEDC_Util.SIOFormatField>();
            var field = new SIOEDC_Util.SIOFormatField("signal1");
            field.port = "Port1";
            field.lane = "Lane1";
            field.dataFormat = "bin";
            field.expectExactVal = "0001";
            field.exitPort = -2;
            format.fields.Add(field);

            var formats = new SIOEDC_Util.SIOFormatFile();
            formats.valid = true;
            formats.header = "someheader";
            formats.data = new List<SIOEDC_Util.SIOFormat> { format };

            var dataHash = new SIOEDC_Util.HashedData();

            dataHash.Add("Port1", "Lane1", "signal1", "xxxx");
            this.TestGenerateOutputFailCase(formats, dataHash, "GenerateOutput: Error Data=[xxxx] is not binary for Format Field [Port1, Lane1, signal1] in sequence data.");

            dataHash.Add("Port1", "Lane1", "signal1", "000000000000000000000000000000000000000000000000000000000000");
            this.TestGenerateOutputFailCase(formats, dataHash, "GenerateOutput: Error Data=[000000000000000000000000000000000000000000000000000000000000] is too many bits to convert for for Format Field [Port1, Lane1, signal1] in sequence data.");

            dataHash.Add("Port1", "Lane1", "signal1", "0000");
            format.fields[0].dataFormat = "blah";
            this.TestGenerateOutputFailCase(formats, dataHash, "Output data format [blah] is not valid.");

            format.fields[0].dataFormat = string.Empty;
            dataHash.Add("Port1", "Lane1", "signal1", "1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111");
            this.TestGenerateOutputFailCase(formats, dataHash, "Raw format only supports up to 64 bits.");
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void GenerateOutput_DPRegParser_Pass()
        {
            var format = new SIOEDC_Util.SIOFormat("Port1", "Lane1");
            format.fields = new List<SIOEDC_Util.SIOFormatField>();
            var field = new SIOEDC_Util.SIOFormatField("DP_LCE_TEST_STATUS");
            field.port = "Port1";
            field.lane = "Lane1";
            field.dataFormat = "bin";
            field.expectExactVal = "0001";
            field.exitPort = -2;
            format.fields.Add(field);

            var field2 = new SIOEDC_Util.SIOFormatField("DP_LCE_TEST_STATUS_PERLANE");
            field2.port = "Port1";
            field2.lane = "Lane1";
            field2.dataFormat = "bin";
            field2.expectLowLimit = 5;
            field2.expectHighLimit = 15;
            field2.exitPort = -2;
            format.fields.Add(field2);

            var formats = new SIOEDC_Util.SIOFormatFile();
            formats.valid = true;
            formats.header = "someheader";
            formats.data = new List<SIOEDC_Util.SIOFormat> { format };

            var dataHash = new SIOEDC_Util.HashedData();
            dataHash.Add("Port1", "Lane1", "signal1", "0000");
            dataHash.Add("Port1", "Lane1", "LCECAP", "0000000000111000000000000000000000000000".Reverse());

            var utils = new SIOEDC_Util(true);
            Assert.IsTrue(utils.GenerateOutput(formats, dataHash, "DP", out var outputList, out var exitPort));
            Assert.AreEqual(-2, exitPort);
        }

        /// <summary>Unit test for SIO.</summary>
        [TestMethod]
        public void DisaplayOutput_Simple_Pass()
        {
            var header = "One,Two,,Four";
            var outputList = new List<string> { ",A,B,C,D,E,F,G" };

            var utils = new SIOEDC_Util(true);
            Assert.IsTrue(utils.DisplayOutput(header, outputList));

            this.ConsoleServiceMock.Verify(o => o.PrintDebug("[SIO_INFO] ------------------------------"), Times.Exactly(2));
            this.ConsoleServiceMock.Verify(o => o.PrintDebug("[SIO_INFO] One       Two       Four      "), Times.Once);
            this.ConsoleServiceMock.Verify(o => o.PrintDebug("[SIO_INFO] A         B         C         D E F G "), Times.Once);
        }

        private static string GetPathToFiles([CallerFilePath] string srcPath = "")
        {
            return Path.GetDirectoryName(srcPath) + "\\input_files\\";
        }

        private void TestInvalidFormatFile(string filename, string failmsg, bool status = false)
        {
            var fileServiceMock = this.MockFileService(filename);
            var utils = new SIOEDC_Util(true);
            var formats = utils.LoadFormatFile(filename);
            this.ConsoleServiceMock.Verify(o => o.PrintError(failmsg, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual(status, formats.valid);
            fileServiceMock.VerifyAll();
        }

        private void TestInvalidSeqenceFile(string filename, string failmsg)
        {
            var fileServiceMock = this.MockFileService(filename);
            var utils = new SIOEDC_Util(true);
            var sequence = utils.LoadSequenceFile(filename);
            this.ConsoleServiceMock.Verify(o => o.PrintError(failmsg, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual(0, sequence.Count);
            fileServiceMock.VerifyAll();
        }

        private void TestGenerateOutputFailCase(SIOEDC_Util.SIOFormatFile formats, SIOEDC_Util.HashedData data, string expectMessage)
        {
            var utils = new SIOEDC_Util(true);
            Assert.IsFalse(utils.GenerateOutput(formats, data, string.Empty, out var outputList, out var exitPort));
            this.ConsoleServiceMock.Verify(o => o.PrintError(expectMessage, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private Mock<IFileService> MockFileService(string filename)
        {
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.GetFile(filename)).Returns((string s) => Path.IsPathRooted(s) ? s : GetPathToFiles() + Path.GetFileName(s));
            Prime.Services.FileService = fileServiceMock.Object;
            return fileServiceMock;
        }
    }
}
