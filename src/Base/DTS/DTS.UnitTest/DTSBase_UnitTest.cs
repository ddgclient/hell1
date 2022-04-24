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

namespace DTSBase.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions.TestingHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.FileService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DTSBase_UnitTest : DTSBase
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<ISharedStorageService> sharedStorageMock;
        private Mock<IFileService> fileServiceMock;
        private MockFileSystem fileSystemMock;

        /// <summary>
        /// Gets or sets mock shared storage.
        /// </summary>
        public Dictionary<string, string> SharedStorageValues { get; set; }

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(
                o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
            {
                System.Console.WriteLine($"DEBUG: {msg}");
            });

            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            // Default Mock for Shared service.
            this.SharedStorageValues = new Dictionary<string, string>();
            this.sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>()))
                .Callback((string key, object obj, Context context) =>
                {
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, string obj, Context context) =>
                {
                    this.SharedStorageValues[key] = obj;
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<Context>()))
                .Callback((string key, double obj, Context context) =>
                {
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.sharedStorageMock.Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>()))
                .Callback((string key, Type obj, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Type obj, Context context) => JsonConvert.DeserializeObject(this.SharedStorageValues[key], obj));
            this.sharedStorageMock.Setup(o => o.GetDoubleRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => double.Parse(this.SharedStorageValues[key]));
            this.sharedStorageMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => this.SharedStorageValues[key]);
            Prime.Services.SharedStorageService = this.sharedStorageMock.Object;

            this.fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            this.fileServiceMock.Setup(o => o.FileExists(It.IsAny<string>())).Returns(true);
            this.fileServiceMock.Setup(o => o.GetFile(It.IsAny<string>())).Returns((string fileName) => fileName);
            Prime.Services.FileService = this.fileServiceMock.Object;
            this.fileSystemMock = new MockFileSystem();
            this.FileSystem_ = this.fileSystemMock;
        }

        /// <summary>
        /// Reading all possible configuration options.
        /// </summary>
        [TestMethod]
        public void ReadingFile_Pass()
        {
            this.ConfigurationFile = "dts_config.json";

            string source =
@"[{
""Name"": ""ConfigurationName"",
""IsEnabled"": true,
""PinName"": ""TDO"",
""SensorsList"": [""sensor1"",""sensor2"",""sensor3""],
""RegisterSize"": 9,
""Slope"": 0.5,
""Offset"": -64,
""IgnoredSensorsList"": [""sensor2""],
""SetPoint"": ""100.0"",
""UpperTolerance"": ""110.0"",
""LowerTolerance"": ""90.0"",
""DatalogValues"": true,
""CompressedDatalog"": true,
""LastPattern"": true,
}]";

            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("dts_config.json", mockFile);
            this.sharedStorageMock.Setup(o => o.OverrideObjectRowResetPolicy(DDG.DTS.DTSConfigurationTable, ResetPolicy.NEVER_RESET, Context.DUT));
            this.sharedStorageMock.Setup(o => o.OverrideObjectRowResetPolicy(DDG.DTS.DTSPlistClones, ResetPolicy.NEVER_RESET, Context.DUT));

            this.Verify();
            Assert.AreEqual(1, this.Execute());
        }

        /// <summary>
        /// Reading invalid configuration.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(JsonSerializationException))]
        public void InvalidFile_Fail()
        {
            this.ConfigurationFile = "dts_config.json";

            string source =
@"[{
""Name"": ""Configuration"",
""IsEnabled"": true,
""PinName"": ""TDO"",
""SeTYPOnsorsList"": [""sensor1"",""sensor2"",""sensor3""],
""RegisterSize"": 9,
""Slope"": 0.5,
""Offset"": -64,
""IgnoredSensorsList"": [""sensor2""],
""SetPoint"": ""100.0"",
""UpperTolerance"": ""110.0"",
""LowerTolerance"": ""90.0"",
""DatalogValues"": true,
""CompressedDatalog"": true
}]";

            var mockFile = new MockFileData(source);
            this.fileSystemMock.AddFile("dts_config.json", mockFile);

            this.Verify();
        }
    }
}
