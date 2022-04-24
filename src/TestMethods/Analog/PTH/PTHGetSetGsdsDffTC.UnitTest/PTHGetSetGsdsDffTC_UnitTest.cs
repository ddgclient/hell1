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

namespace PTHGetSetGsdsDffTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions.TestingHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.DffService;
    using Prime.FileService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class PTHGetSetGsdsDffTC_UnitTest : PTHGetSetGsdsDffTC
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IFileService> fileServiceMock;
        private MockFileSystem fileSystemMock;

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) => System.Console.WriteLine(msg));
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            this.fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            this.fileServiceMock.Setup(o => o.FileExists(It.IsAny<string>())).Returns(true);
            this.fileServiceMock.Setup(o => o.GetFile(It.IsAny<string>())).Returns((string fileName) => fileName);
            Prime.Services.FileService = this.fileServiceMock.Object;
            this.fileSystemMock = new MockFileSystem();
            this.FileSystem_ = this.fileSystemMock;
            this.LogLevel = PrimeLogLevel.TEST_METHOD;
        }

        /// <summary>
        /// Reading all possible configuration options.
        /// </summary>
        [TestMethod]
        public void Verify_ReadingFile_Pass()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": true
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.Verify();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Verify_InvalidFileTYPO_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFTYPOOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            Assert.ThrowsException<ArgumentException>(() => this.Verify());
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]

        // [ExpectedException(typeof(ArgumentException))]
        public void Verify_InvalidFileAllTrue_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            var ex = Assert.ThrowsException<ArgumentException>(() => this.Verify());
            Assert.IsTrue(ex.Message.Contains(":Invalid condition GSDS2DFF == DFF2GSDS True\n Invalid condition GSDS2DFF == DFF2GSDS:True"));

            // this.Verify();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_InvalidFileAllTrue_Fail1()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            // var ex = Assert.ThrowsException<ArgumentException>(() => this.Verify());
            // Assert.AreEqual(ex.Message, "PTHGetSetGsdsDffTC.dll.<Verify>b__10_0:Invalid condition GSDS2DFF == DFF2GSDS True\n Invalid condition GSDS2DFF == DFF2GSDS:True");
            this.Verify();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_InvalidFileDelimiter_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": """",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.Verify();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_InvalidFileDelimiter_Fail1()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.Verify();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_InvalidFileDFF_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0,TSDT1"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.Verify();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_InvalidFileDFFOpType_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT,PBIC_DAB"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.Verify();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_InvalidFileDFFOpType4_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S,U.D,U.I"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.Verify();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_InvalidFileDFFOpType1_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""US"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.Verify();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_InvalidFileDFFOpType2_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""A.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.Verify();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_InvalidFileDFFOpType3_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.A"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.Verify();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_InvalidFileDFF_Fail1()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""||"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.Verify();
        }

        /// <summary>
        /// Reading all possible configuration options.
        /// </summary>
        [TestMethod]
        public void Verify_InvalidGsdsFormatNoGSDSScopeTypeGsds2Dff_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP""],
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            var ex = Assert.ThrowsException<ArgumentException>(() => this.Verify());
            Assert.IsTrue(ex.Message.Contains("GSDSScopeType is empty and Token=[sSORT_TSAP] is not in GSDS format or in GSDS2DFFAllowedList."));
        }

        /// <summary>
        /// Reading all possible configuration options.
        /// </summary>
        [TestMethod]
        public void Verify_InvalidGsdsFormatNoGSDSScopeTypeDff2Gsds_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP""],
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFFAllowedList"": [""sSORT_TSAP""],
""DFF2GSDS"": true,
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            var ex = Assert.ThrowsException<ArgumentException>(() => this.Verify());
            Assert.IsTrue(ex.Message.Contains("GSDSScopeType is empty and Token=[sSORT_TSAP] is not in GSDS format or in GSDS2DFFAllowedList."));
        }

        /// <summary>
        /// Reading all possible configuration options.
        /// </summary>
        [TestMethod]
        public void Verify_NoGSDSScopeTypeGsdsAllowedList_Pass()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""sSORT_TSAP""],
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFFAllowedList"": [""sSORT_TSAP""],
""GSDS2DFF"": true,
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.Verify();
        }

        /// <summary>
        /// Reading all possible configuration options.
        /// </summary>
        [TestMethod]
        public void Verify_NoDffOptype_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            string source =
@"[{
""GSDSList"": [""G.U.S.sSORT_TSAP""],
""DFF"": ""TSDT0"",
""GSDS2DFF"": true,
}]";

            // [2] Verifies the input file of the Method under test, and that any mock setup on [1].
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            var ex = Assert.ThrowsException<ArgumentException>(() => this.Verify());
            Assert.IsTrue(ex.Message.Contains("DFFOpType is required or DFF must be optype.token format."));
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_DFF2GSDS_Pass()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|460|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TTPCS0P", "460");
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_DFF2GSDS_Pass1()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            string source =
@"[{
""GSDSList"": ["" sSORT_TSAP""],
""GSDSScopeType"": ""U.S"",
""DFF"": ""TSDT0 "",
""DFFOpType"": ""SORT "",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450");
            Prime.Services.DffService = dffServiceMock.Object;

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_DFF2GSDS_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.consoleServiceMock.Setup(o => o.PrintError("The GSDSList count does not matches the DFF split count. \n GSDSList count=[3]. \n GSDSList count=[2]", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|460");
            Prime.Services.DffService = dffServiceMock.Object;

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = gsdsMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(2, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
            dffServiceMock.VerifyAll();
            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_DFF2GSDS_Fail1()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.consoleServiceMock.Setup(o => o.PrintError("The GSDSList count does not matches the DFF split count. \n GSDSList count=[2]. \n GSDSList count=[3]", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|460|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = gsdsMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(2, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
            dffServiceMock.VerifyAll();
            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_DFF2GSDS_Fail2()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.consoleServiceMock.Setup(o => o.PrintError("The GSDS token = [G.U.S.sSORT_TTPCS0P] already exists.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|460|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            gsdsMock.Setup(o => o.KeyExistsInStringTable("sSORT_TTPCS0P", Context.DUT)).Returns(true);
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(2, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
            dffServiceMock.VerifyAll();
            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_DFF2GSDS_OPType_Pass()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.DFF2GSDS;
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|460|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TTPCS0P", "460");
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
            dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_DFF2GSDS_OPType_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            Prime.Services.DffService = dffServiceMock.Object;

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = gsdsMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(4, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
            dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_DFF2GSDS_AllowedList_Pass()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false,
""GSDS2DFFAllowedList"": [""sSORT_TTPCS0P""]
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|460|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TTPCS0P", "460");
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
            dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_DFF2GSDS_AllowedList_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""-999"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""G.U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": false,
""DFF2GSDS"": true,
""PrintDFF"": false,
""GSDS2DFFAllowedList"": [""-999""]
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|460|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsWriteTokenMock(ref gsdsMock, "-999", "460");
            AddGsdsWriteTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
            dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_GSDS2DFF_Pass()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP "", ""sSORT_TTPCS0P "", ""sSORT_TTPCS1P ""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0 "",
""DFFOpType"": ""SORT "",
""GSDS2DFF"": true,
""DFF2GSDS"": false,
""PrintDFF"": false
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TTPCS0P", "460");
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.SetDff("TSDT0", "450|460|470"));
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|460|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
            dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_GSDS2DFF_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
""DFF2GSDS"": false,
""PrintDFF"": false
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.consoleServiceMock.Setup(o => o.PrintError("The GSDS token = [G.U.S.sSORT_TTPCS0P] does not exist nor it is in the GSDS2DFFAllowedList.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            gsdsMock.Setup(o => o.KeyExistsInStringTable("sSORT_TTPCS0P", Context.DUT)).Returns(false);
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.SetDff("TSDT0", "450|470"));
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(3, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
            dffServiceMock.VerifyAll();
            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_GSDS2DFF_AllowedList_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
""DFF2GSDS"": false,
""PrintDFF"": false,
""GSDS2DFFAllowedList"": [""1""]
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);
            this.consoleServiceMock.Setup(o => o.PrintError("The GSDS token = [G.U.S.sSORT_TTPCS0P] does not exist nor it is in the GSDS2DFFAllowedList.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            gsdsMock.Setup(o => o.KeyExistsInStringTable("sSORT_TTPCS0P", Context.DUT)).Returns(false);
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.SetDff("TSDT0", "450|470"));
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(3, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
            dffServiceMock.VerifyAll();
            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_GSDS2DFF_AllowedList_Pass()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
""DFF2GSDS"": false,
""PrintDFF"": false,
""GSDS2DFFAllowedList"": [""sSORT_TTPCS0P""]
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.SetDff("TSDT0", "450|sSORT_TTPCS0P|470"));
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|sSORT_TTPCS0P|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
            dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_GSDS2DFF_AllowedList_Pass1()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""1"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
""DFF2GSDS"": false,
""PrintDFF"": false,
""GSDS2DFFAllowedList"": [1]
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.SetDff("TSDT0", "450|1|470"));
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|1|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            dffServiceMock.VerifyAll();
            gsdsMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_GSDS2DFF_AllowedList_Pass2()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", -999, ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
""DFF2GSDS"": false,
""PrintDFF"": false,
""GSDS2DFFAllowedList"": [-999]
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.SetDff("TSDT0", "450|-999|470"));
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|-999|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            dffServiceMock.VerifyAll();
            gsdsMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_GSDS2DFF_AllowedList_Pass3()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""0x7|1011|5"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
""DFF2GSDS"": false,
""PrintDFF"": false,
""GSDS2DFFAllowedList"": [""0x7|1011|5""]
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.SetDff("TSDT0", "450|0x7|1011|5|470"));
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|0x7|1011|5|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            dffServiceMock.VerifyAll();
            dffServiceMock.VerifyAll();
            gsdsMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_GSDS2DFF_AllowedList_Pass4()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", 0x7, ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
""DFF2GSDS"": false,
""PrintDFF"": false,
""GSDS2DFFAllowedList"": [0x7]
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.SetDff("TSDT0", "450|0x7|470"));
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|0x7|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            dffServiceMock.VerifyAll();
            gsdsMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_GSDS2DFF_AllowedList_Pass5()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", 1011, ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
""DFF2GSDS"": false,
""PrintDFF"": false,
""GSDS2DFFAllowedList"": [1011]
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.SetDff("TSDT0", "450|1011|470"));
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|1011|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            dffServiceMock.VerifyAll();
            gsdsMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_GSDS2DFF_AllowedList_Pass6()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [-999],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
""DFF2GSDS"": false,
""PrintDFF"": false,
""GSDS2DFFAllowedList"": [-999]
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.SetDff("TSDT0", "-999"));
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("-999");
            Prime.Services.DffService = dffServiceMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            dffServiceMock.VerifyAll();
            gsdsMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteToItuff during Gsds2Dff.
        /// </summary>
        [TestMethod]
        public void Excute_GSDS2DFF_WriteToItuff_Pass()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"" ],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
""DFF2GSDS"": false,
""PrintDFF"": true,
""GSDS2DFFAllowedList"": []
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.SetDff("TSDT0", "450"));
            dffServiceMock.Setup(o => o.GetDffByOpType("TSDT0", "SORT", true)).Returns("450");
            Prime.Services.DffService = dffServiceMock.Object;

            var ituffWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            ituffWriterMock.Setup(o => o.SetTnamePostfix("_TSDT0"));
            ituffWriterMock.Setup(o => o.SetData("450"));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(ituffWriterMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(ituffWriterMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            dffServiceMock.VerifyAll();
            gsdsMock.VerifyAll();
            ituffWriterMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_GSDS2DFF_OPType_Fail()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";

            // this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [""sSORT_TSAP"", ""sSORT_TTPCS0P"", ""sSORT_TTPCS1P""],
""GSDSScopeType"": ""U.S"",
""Delimiter"": ""|"",
""DFF"": ""TSDT0"",
""DFFOpType"": ""SORT"",
""GSDS2DFF"": true,
""DFF2GSDS"": false,
""PrintDFF"": false
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            Prime.Services.DffService = dffServiceMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(4, this.Execute());

            // verify the mock
            dffServiceMock.VerifyAll();
            gsdsMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_DFF2GSDS_ShortFormSearchReplace_Pass()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            string source =
@"[{
'GSDSList': ['G.U.S.VminA'],
'DFF': 'SORT:FwdVminA',
'DFF2GSDS': true,
'SearchReplace': { 'v': ',', '^\\|': '-8.888|' }
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.GetDffByOpType("FwdVminA", "SORT", true)).Returns("|1.0v1.1v1.2|0.9|1.5v0.9v-9999");
            Prime.Services.DffService = dffServiceMock.Object;

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            gsdsMock.Setup(o => o.KeyExistsInStringTable("VminA", Context.DUT)).Returns(false);
            gsdsMock.Setup(o => o.InsertRowAtTable("VminA", "-8.888|1.0,1.1,1.2|0.9|1.5,0.9,-9999", Context.DUT));
            Prime.Services.SharedStorageService = gsdsMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
            dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        public void Excute_GSDS2DFF_ShortFormSearchReplace_Pass()
        {
            // [1] Setup the ConfigurationFile required for unit test.
            this.ConfigurationFile = "PTHGetSetGsdsDffTC.json";
            this.OPType = GSDSDFFOP.GSDS2DFF;
            string source =
@"[{
""GSDSList"": [""G.U.S.sSORT_TSAP "", ""G.U.S.sSORT_TTPCS0P "", ""G.U.S.sSORT_TTPCS1P ""],
""Delimiter"": ""|"",
""DFF"": ""SORT:TSDT0 "",
""GSDS2DFF"": true,
""SearchReplace"": { ""460"": ""blah"" },
}]";

            // [2] Setting up the mock.
            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("PTHGetSetGsdsDffTC.json", mockFile);

            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TSAP", "450");
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TTPCS0P", "460");
            AddGsdsReadTokenMock(ref gsdsMock, "sSORT_TTPCS1P", "470");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(service => service.SetDff("TSDT0", "450|blah|470"));
            dffServiceMock.Setup(service => service.GetDffByOpType("TSDT0", "SORT", true)).Returns("450|blah|470");
            Prime.Services.DffService = dffServiceMock.Object;

            // [3] Verifies the input file of the Method under test, and that any mock setup on [2].
            this.Verify();
            Assert.AreEqual(1, this.Execute());

            // verify the mock
            gsdsMock.VerifyAll();
            dffServiceMock.VerifyAll();
        }

        private static void AddGsdsReadTokenMock(ref Mock<ISharedStorageService> mock, string name, string value)
        {
            mock.Setup(o => o.KeyExistsInStringTable(name, Context.DUT)).Returns(true);
            mock.Setup(o => o.GetStringRowFromTable(name, Context.DUT)).Returns(value);
        }

        private static void AddGsdsWriteTokenMock(ref Mock<ISharedStorageService> mock, string name, string value)
        {
            mock.Setup(o => o.KeyExistsInStringTable(name, Context.DUT)).Returns(false);
            mock.Setup(o => o.InsertRowAtTable(name, value, Context.DUT));
        }
    }
}
