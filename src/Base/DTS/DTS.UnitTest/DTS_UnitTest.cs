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
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DTS_UnitTest
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<ISharedStorageService> sharedStorageMock;
        private Mock<IDatalogService> datalogServiceMock;
        private Mock<IStrgvalFormat> strgValWriterMock;

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
            this.sharedStorageMock.Setup(o => o.KeyExistsInObjectTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                })
                .Returns((string key, Context context) => this.SharedStorageValues.ContainsKey(key));
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

            this.datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            Prime.Services.DatalogService = this.datalogServiceMock.Object;
            this.strgValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);

            var testprogramMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            testprogramMock.Setup(o => o.GetCurrentTestInstanceParameter("LogLevel")).Returns("PRIME_DEBUG");
            Prime.Services.TestProgramService = testprogramMock.Object;
        }

        /// <summary>
        /// Test the DTSImpl constructor fail paths.
        /// </summary>
        [TestMethod]
        public void FactoryGet_InvalidConfigurationTable_Fail()
        {
            this.sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.sharedStorageMock.Setup(o => o.KeyExistsInObjectTable(DDG.DTS.DTSConfigurationTable, Context.DUT)).Returns(false);
            Prime.Services.SharedStorageService = this.sharedStorageMock.Object;

            var ex1 = Assert.ThrowsException<Exception>(() => new DTSImpl("InvalidConfiguration"));
            Assert.AreEqual($"{DDG.DTS.DTSConfigurationTable} does not exist in SharedStorage.", ex1.Message);

            this.sharedStorageMock.Setup(o => o.KeyExistsInObjectTable(DDG.DTS.DTSConfigurationTable, Context.DUT)).Returns(true);
            this.sharedStorageMock.Setup(o => o.GetRowFromTable(DDG.DTS.DTSConfigurationTable, typeof(List<Configuration>), Context.DUT)).Returns(new List<Configuration>());
            Prime.Services.SharedStorageService = this.sharedStorageMock.Object;

            var ex2 = Assert.ThrowsException<Exception>(() => new DTSImpl("InvalidConfiguration"));
            Assert.AreEqual($"{DDG.DTS.DTSConfigurationTable} is invalid.", ex2.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ProcessCapturedData_LastSinglePattern_Pass()
        {
            this.datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.strgValWriterMock.Object);
            this.strgValWriterMock.Setup(o => o.SetTnamePostfix("_DTS"));
            this.strgValWriterMock.Setup(o => o.SetData("SA:100.00|TPCS0:97.50|TPCS1:99.00|IPUS0:98.50|IPUS1:100.00|IPUS2:99.00|C0S0:99.00|C0S1:100.00|C0S2:97.00|C1S0:100.00|C1S1:100.00|C1S2:101.00|C2S0:99.00|C2S1:99.00|C2S2:99.00|C3S0:99.00|C3S1:100.00|C3S2:99.00|GT0S0:99.00|GT0S1:100.00|GT0S2:100.00|GT0S3:99.00|GT1S0:100.00|GT1S1:101.00|GT1S2:98.50|GT1S3:98.50|GT2S0:99.00|GT2S1:99.00|GT2S2:99.00|GT3S0:99.00|GT3S1:100.50|GT3S2:98.00|GT3S3:98.00|GTUS0:100.00|GTUS1:99.00|GTUS2:100.50"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgValWriterMock.Object));
            string source =
                @"[{
""Name"": ""LastPattern1"",
""IsEnabled"": true,
""PinName"": ""TDO"",
""SensorsList"": [""SA"", ""TPCS0"", ""TPCS1"", ""IPUS0"", ""IPUS1"", ""IPUS2"", ""C0S0"", ""C0S1"", ""C0S2"", ""C1S0"", ""C1S1"", ""C1S2"", ""C2S0"", ""C2S1"", ""C2S2"", ""C3S0"", ""C3S1"", ""C3S2"", ""GT0S0"", ""GT0S1"", ""GT0S2"", ""GT0S3"", ""GT1S0"", ""GT1S1"", ""GT1S2"", ""GT1S3"", ""GT2S0"", ""GT2S1"", ""GT2S2"", ""GT3S0"", ""GT3S1"", ""GT3S2"", ""GT3S3"", ""GTUS0"", ""GTUS1"", ""GTUS2""],
""IgnoredSensorsList"": [],
""RegisterSize"": 9,
""Slope"": 0.5,
""Offset"": -64,
""SetPoint"": ""100.0"",
""UpperTolerance"": ""20.0"",
""LowerTolerance"": ""20.0"",
""DatalogValues"": true,
""CompressedDatalog"": true,
""LastPattern"": true
}]";
            var settings = JsonConvert.DeserializeObject<List<Configuration>>(source);
            Prime.Services.SharedStorageService.InsertRowAtTable(DDG.DTS.DTSConfigurationTable, settings, Context.DUT);

            IDTS dts = new DTSImpl("LastPattern1");
            var ctv = "000100101110000101011000101101000101000100101011000101011000101000100101010000101000100101000100101010100101011000101011000101011000101011000101000100101011000101011000101000100101000100101011000101000100101010100101101000101101000101011000101011000101011000101011000101100100101001000101001000101000100101011000101100100101";
            var result = dts.ProcessCapturedData(ctv);
            Assert.IsTrue(result);
            this.datalogServiceMock.VerifyAll();
            this.strgValWriterMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ProcessCapturedData_FailingHighLimit_Fail()
        {
            this.datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.strgValWriterMock.Object);
            this.strgValWriterMock.Setup(o => o.SetTnamePostfix("_DTS"));
            this.strgValWriterMock.Setup(o => o.SetData("SA:100.00|TPCS0:97.50|TPCS1:99.00|IPUS0:98.50|IPUS1:100.00|IPUS2:99.00|C0S0:99.00|C0S1:100.00|C0S2:97.00|C1S0:100.00|C1S1:100.00|C1S2:101.00|C2S0:99.00|C2S1:99.00|C2S2:99.00|C3S0:99.00|C3S1:100.00|C3S2:99.00|GT0S0:99.00|GT0S1:100.00|GT0S2:100.00|GT0S3:99.00|GT1S0:100.00|GT1S1:101.00|GT1S2:98.50|GT1S3:98.50|GT2S0:99.00|GT2S1:99.00|GT2S2:99.00|GT3S0:99.00|GT3S1:100.50|GT3S2:98.00|GT3S3:98.00|GTUS0:100.00|GTUS1:99.00|GTUS2:100.50"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgValWriterMock.Object));
            string source =
                @"[{
""Name"": ""LastPattern1"",
""IsEnabled"": true,
""PinName"": ""TDO"",
""SensorsList"": [""SA"", ""TPCS0"", ""TPCS1"", ""IPUS0"", ""IPUS1"", ""IPUS2"", ""C0S0"", ""C0S1"", ""C0S2"", ""C1S0"", ""C1S1"", ""C1S2"", ""C2S0"", ""C2S1"", ""C2S2"", ""C3S0"", ""C3S1"", ""C3S2"", ""GT0S0"", ""GT0S1"", ""GT0S2"", ""GT0S3"", ""GT1S0"", ""GT1S1"", ""GT1S2"", ""GT1S3"", ""GT2S0"", ""GT2S1"", ""GT2S2"", ""GT3S0"", ""GT3S1"", ""GT3S2"", ""GT3S3"", ""GTUS0"", ""GTUS1"", ""GTUS2""],
""IgnoredSensorsList"": [],
""RegisterSize"": 9,
""Slope"": 0.5,
""Offset"": -64,
""SetPoint"": ""90.0"",
""UpperTolerance"": ""5.0"",
""LowerTolerance"": ""20.0"",
""DatalogValues"": true,
""CompressedDatalog"": true,
""LastPattern"": true
}]";
            var settings = JsonConvert.DeserializeObject<List<Configuration>>(source);
            Prime.Services.SharedStorageService.InsertRowAtTable(DDG.DTS.DTSConfigurationTable, settings, Context.DUT);

            IDTS dts = new DTSImpl("LastPattern1");
            var ctv = "000100101110000101011000101101000101000100101011000101011000101000100101010000101000100101000100101010100101011000101011000101011000101011000101000100101011000101011000101000100101000100101011000101000100101010100101101000101101000101011000101011000101011000101011000101100100101001000101001000101000100101011000101100100101";
            var result = dts.ProcessCapturedData(ctv);
            Assert.IsFalse(result);
            this.datalogServiceMock.VerifyAll();
            this.strgValWriterMock.VerifyAll();
            this.consoleServiceMock.Verify(o => o.PrintDebug(It.IsRegex("Failed.*Max=.*Upper Tolerance Limit=.*")));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ProcessCapturedData_FailingLowLimit_Fail()
        {
            this.datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.strgValWriterMock.Object);
            this.strgValWriterMock.Setup(o => o.SetTnamePostfix("_DTS"));
            this.strgValWriterMock.Setup(o => o.SetData("SA:100.00|TPCS0:97.50|TPCS1:99.00|IPUS0:98.50|IPUS1:100.00|IPUS2:99.00|C0S0:99.00|C0S1:100.00|C0S2:97.00|C1S0:100.00|C1S1:100.00|C1S2:101.00|C2S0:99.00|C2S1:99.00|C2S2:99.00|C3S0:99.00|C3S1:100.00|C3S2:99.00|GT0S0:99.00|GT0S1:100.00|GT0S2:100.00|GT0S3:99.00|GT1S0:100.00|GT1S1:101.00|GT1S2:98.50|GT1S3:98.50|GT2S0:99.00|GT2S1:99.00|GT2S2:99.00|GT3S0:99.00|GT3S1:100.50|GT3S2:98.00|GT3S3:98.00|GTUS0:100.00|GTUS1:99.00|GTUS2:100.50"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgValWriterMock.Object));
            Configuration conf = new Configuration
            {
                PinName = "TDO",
                IsEnabled = true,
                RegisterSize = 9,
                Slope = 0.5,
                Offset = -64,
                SensorsList = new List<string> { "SA", "TPCS0", "TPCS1", "IPUS0", "IPUS1", "IPUS2", "C0S0", "C0S1", "C0S2", "C1S0", "C1S1", "C1S2", "C2S0", "C2S1", "C2S2", "C3S0", "C3S1", "C3S2", "GT0S0", "GT0S1", "GT0S2", "GT0S3", "GT1S0", "GT1S1", "GT1S2", "GT1S3", "GT2S0", "GT2S1", "GT2S2", "GT3S0", "GT3S1", "GT3S2", "GT3S3", "GTUS0", "GTUS1", "GTUS2" },
                IgnoredSensorsList = new List<string>(),
                SetPoint = "100",
                UpperTolerance = "20",
                LowerTolerance = "1",
                DatalogValues = true,
                CompressedDatalog = true,
                LastPattern = true,
            };
            string source =
                @"[{
""Name"": ""LastPattern1"",
""IsEnabled"": true,
""PinName"": ""TDO"",
""SensorsList"": [""SA"", ""TPCS0"", ""TPCS1"", ""IPUS0"", ""IPUS1"", ""IPUS2"", ""C0S0"", ""C0S1"", ""C0S2"", ""C1S0"", ""C1S1"", ""C1S2"", ""C2S0"", ""C2S1"", ""C2S2"", ""C3S0"", ""C3S1"", ""C3S2"", ""GT0S0"", ""GT0S1"", ""GT0S2"", ""GT0S3"", ""GT1S0"", ""GT1S1"", ""GT1S2"", ""GT1S3"", ""GT2S0"", ""GT2S1"", ""GT2S2"", ""GT3S0"", ""GT3S1"", ""GT3S2"", ""GT3S3"", ""GTUS0"", ""GTUS1"", ""GTUS2""],
""IgnoredSensorsList"": [],
""RegisterSize"": 9,
""Slope"": 0.5,
""Offset"": -64,
""SetPoint"": ""100.0"",
""UpperTolerance"": ""20.0"",
""LowerTolerance"": ""1.0"",
""DatalogValues"": true,
""CompressedDatalog"": true,
""LastPattern"": true
}]";
            var settings = JsonConvert.DeserializeObject<List<Configuration>>(source);
            Prime.Services.SharedStorageService.InsertRowAtTable(DDG.DTS.DTSConfigurationTable, settings, Context.DUT);

            IDTS dts = new DTSImpl("LastPattern1");
            var ctv = "000100101110000101011000101101000101000100101011000101011000101000100101010000101000100101000100101010100101011000101011000101011000101011000101000100101011000101011000101000100101000100101011000101000100101010100101101000101101000101011000101011000101011000101011000101100100101001000101001000101000100101011000101100100101";
            var result = dts.ProcessCapturedData(ctv);
            Assert.IsFalse(result);
            this.datalogServiceMock.VerifyAll();
            this.strgValWriterMock.VerifyAll();
            this.consoleServiceMock.Verify(o => o.PrintDebug(It.IsRegex("Failed.*Min=.*Lower Tolerance Limit=.*")));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ProcessCapturedData_IgnoredSensors_Pass()
        {
            this.datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.strgValWriterMock.Object);
            this.strgValWriterMock.Setup(o => o.SetTnamePostfix("_DTS"));
            this.strgValWriterMock.Setup(o => o.SetData("SA:100.00|TPCS0:97.50|TPCS1:99.00|IPUS0:98.50|IPUS1:100.00|IPUS2:99.00|C0S0:99.00|C0S1:100.00|C0S2:97.00|C1S0:100.00|C1S1:100.00|C1S2:101.00|C2S0:99.00|C2S1:99.00|C2S2:99.00|C3S0:99.00|C3S1:100.00|C3S2:99.00|GT0S0:99.00|GT0S1:100.00|GT0S2:100.00|GT0S3:99.00|GT1S0:100.00|GT1S1:101.00|GT1S2:98.50|GT1S3:98.50|GT2S0:99.00|GT2S1:99.00|GT2S2:99.00|GT3S0:99.00|GT3S1:100.50|GT3S2:98.00|GT3S3:98.00|GTUS0:100.00|GTUS1:99.00|GTUS2:100.50"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgValWriterMock.Object));
            string source =
                @"[{
""Name"": ""LastPattern1"",
""IsEnabled"": true,
""PinName"": ""TDO"",
""SensorsList"": [""SA"", ""TPCS0"", ""TPCS1"", ""IPUS0"", ""IPUS1"", ""IPUS2"", ""C0S0"", ""C0S1"", ""C0S2"", ""C1S0"", ""C1S1"", ""C1S2"", ""C2S0"", ""C2S1"", ""C2S2"", ""C3S0"", ""C3S1"", ""C3S2"", ""GT0S0"", ""GT0S1"", ""GT0S2"", ""GT0S3"", ""GT1S0"", ""GT1S1"", ""GT1S2"", ""GT1S3"", ""GT2S0"", ""GT2S1"", ""GT2S2"", ""GT3S0"", ""GT3S1"", ""GT3S2"", ""GT3S3"", ""GTUS0"", ""GTUS1"", ""GTUS2""],
""IgnoredSensorsList"": [""C0S2"", ""TPCS0""],
""RegisterSize"": 9,
""Slope"": 0.5,
""Offset"": -64,
""SetPoint"": ""100.0"",
""UpperTolerance"": ""20.0"",
""LowerTolerance"": ""2.0"",
""DatalogValues"": true,
""CompressedDatalog"": true,
""LastPattern"": true
}]";
            var settings = JsonConvert.DeserializeObject<List<Configuration>>(source);
            Prime.Services.SharedStorageService.InsertRowAtTable(DDG.DTS.DTSConfigurationTable, settings, Context.DUT);

            IDTS dts = new DTSImpl("LastPattern1");
            var ctv = "000100101110000101011000101101000101000100101011000101011000101000100101010000101000100101000100101010100101011000101011000101011000101011000101000100101011000101011000101000100101000100101011000101000100101010100101101000101101000101011000101011000101011000101011000101100100101001000101001000101000100101011000101100100101";
            var result = dts.ProcessCapturedData(ctv);
            Assert.IsTrue(result);
            this.datalogServiceMock.VerifyAll();
            this.strgValWriterMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ProcessCapturedData_AllPatterns_Pass()
        {
            this.datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.strgValWriterMock.Object);
            this.strgValWriterMock.Setup(o => o.SetTnamePostfix("_DTS"));
            this.strgValWriterMock.Setup(o => o.SetData("SA:96.00,98.00,100.00|TPCS0:96.50,97.00,97.50|TPCS1:99.00,99.25,99.50|IPUS0:98.50,98.50,98.50|IPUS1:100.00,100.00,100.00|IPUS2:99.00,99.00,99.00|C0S0:99.00,99.00,99.00|C0S1:100.00,100.00,100.00|C0S2:97.00,97.00,97.00|C1S0:100.00,100.00,100.00|C1S1:100.00,100.00,100.00|C1S2:101.00,101.00,101.00|C2S0:99.00,99.00,99.00|C2S1:99.00,99.00,99.00|C2S2:99.00,99.00,99.00|C3S0:99.00,99.00,99.00|C3S1:100.00,100.00,100.00|C3S2:99.00,99.00,99.00|GT0S0:99.00,99.00,99.00|GT0S1:100.00,100.00,100.00|GT0S2:100.00,100.00,100.00|GT0S3:99.00,99.00,99.00|GT1S0:100.00,100.00,100.00|GT1S1:101.00,101.00,101.00|GT1S2:98.50,98.50,98.50|GT1S3:98.50,98.50,98.50|GT2S0:99.00,99.00,99.00|GT2S1:99.00,99.00,99.00|GT2S2:99.00,99.00,99.00|GT3S0:99.00,99.00,99.00|GT3S1:100.50,100.50,100.50|GT3S2:98.00,98.00,98.00|GT3S3:98.00,98.00,98.00|GTUS0:100.00,100.00,100.00|GTUS1:99.00,99.00,99.00|GTUS2:100.50,100.50,100.50"));
            this.strgValWriterMock.Setup(o => o.SetTnamePostfix("_COMPRESSED_DTS"));
            this.strgValWriterMock.Setup(o => o.SetData(Prime.Base.Utilities.StringUtilities.DeflateCompress32("SA:100.00,96.00|TPCS0:97.50,96.50|TPCS1:99.00,99.50|IPUS0:98.50,98.50|IPUS1:100.00,100.00|IPUS2:99.00,99.00|C0S0:99.00,99.00|C0S1:100.00,100.00|C0S2:97.00,97.00|C1S0:100.00,100.00|C1S1:100.00,100.00|C1S2:101.00,101.00|C2S0:99.00,99.00|C2S1:99.00,99.00|C2S2:99.00,99.00|C3S0:99.00,99.00|C3S1:100.00,100.00|C3S2:99.00,99.00|GT0S0:99.00,99.00|GT0S1:100.00,100.00|GT0S2:100.00,100.00|GT0S3:99.00,99.00|GT1S0:100.00,100.00|GT1S1:101.00,101.00|GT1S2:98.50,98.50|GT1S3:98.50,98.50|GT2S0:99.00,99.00|GT2S1:99.00,99.00|GT2S2:99.00,99.00|GT3S0:99.00,99.00|GT3S1:100.50,100.50|GT3S2:98.00,98.00|GT3S3:98.00,98.00|GTUS0:100.00,100.00|GTUS1:99.00,99.00|GTUS2:100.50,100.50")));
            /* this.strgValWriterMock.Setup(o => o.SetDataCompression(true)); --> replaced by Prime.Base.Utilities.StringUtilities.DeflateCompress32(data)*/
            this.strgValWriterMock.Setup(o => o.SetDelimiterCharacterForWrap('%'));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgValWriterMock.Object));
            string source =
                @"[{
""Name"": ""AllPattern1"",
""IsEnabled"": true,
""PinName"": ""TDO"",
""SensorsList"": [""SA"", ""TPCS0"", ""TPCS1"", ""IPUS0"", ""IPUS1"", ""IPUS2"", ""C0S0"", ""C0S1"", ""C0S2"", ""C1S0"", ""C1S1"", ""C1S2"", ""C2S0"", ""C2S1"", ""C2S2"", ""C3S0"", ""C3S1"", ""C3S2"", ""GT0S0"", ""GT0S1"", ""GT0S2"", ""GT0S3"", ""GT1S0"", ""GT1S1"", ""GT1S2"", ""GT1S3"", ""GT2S0"", ""GT2S1"", ""GT2S2"", ""GT3S0"", ""GT3S1"", ""GT3S2"", ""GT3S3"", ""GTUS0"", ""GTUS1"", ""GTUS2""],
""IgnoredSensorsList"": [],
""RegisterSize"": 9,
""Slope"": 0.5,
""Offset"": -64,
""SetPoint"": ""100.0"",
""UpperTolerance"": ""10.0"",
""LowerTolerance"": ""10.0"",
""DatalogValues"": true,
""CompressedDatalog"": true,
""LastPattern"": false
}]";
            var settings = JsonConvert.DeserializeObject<List<Configuration>>(source);
            Prime.Services.SharedStorageService.InsertRowAtTable(DDG.DTS.DTSConfigurationTable, settings, Context.DUT);

            IDTS dts = new DTSImpl("AllPattern1");
            var ctv = "00000010110000010111100010110100010100010010101100010101100010100010010101000010100010010100010010101010010101100010101100010101100010101100010100010010101100010101100010100010010100010010101100010100010010101010010110100010110100010101100010101100010101100010101100010110010010100100010100100010100010010101100010110010010100010010111000010101100010110100010100010010101100010101100010100010010101000010100010010100010010101010010101100010101100010101100010101100010100010010101100010101100010100010010100010010101100010100010010101010010110100010110100010101100010101100010101100010101100010110010010100100010100100010100010010101100010110010010111";
            var result = dts.ProcessCapturedData(ctv);
            Assert.IsTrue(result);
            this.datalogServiceMock.VerifyAll();
            this.strgValWriterMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ProcessCapturedData_LastPattern_Pass()
        {
            this.datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.strgValWriterMock.Object);
            this.strgValWriterMock.Setup(o => o.SetTnamePostfix("_DTS"));
            this.strgValWriterMock.Setup(o => o.SetData("SA:100.00|TPCS0:97.50|TPCS1:99.00|IPUS0:98.50|IPUS1:100.00|IPUS2:99.00|C0S0:99.00|C0S1:100.00|C0S2:97.00|C1S0:100.00|C1S1:100.00|C1S2:101.00|C2S0:99.00|C2S1:99.00|C2S2:99.00|C3S0:99.00|C3S1:100.00|C3S2:99.00|GT0S0:99.00|GT0S1:100.00|GT0S2:100.00|GT0S3:99.00|GT1S0:100.00|GT1S1:101.00|GT1S2:98.50|GT1S3:98.50|GT2S0:99.00|GT2S1:99.00|GT2S2:99.00|GT3S0:99.00|GT3S1:100.50|GT3S2:98.00|GT3S3:98.00|GTUS0:100.00|GTUS1:99.00|GTUS2:100.50"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgValWriterMock.Object));
            string source =
                @"[{
""Name"": ""LastPattern1"",
""IsEnabled"": true,
""PinName"": ""TDO"",
""SensorsList"": [""SA"", ""TPCS0"", ""TPCS1"", ""IPUS0"", ""IPUS1"", ""IPUS2"", ""C0S0"", ""C0S1"", ""C0S2"", ""C1S0"", ""C1S1"", ""C1S2"", ""C2S0"", ""C2S1"", ""C2S2"", ""C3S0"", ""C3S1"", ""C3S2"", ""GT0S0"", ""GT0S1"", ""GT0S2"", ""GT0S3"", ""GT1S0"", ""GT1S1"", ""GT1S2"", ""GT1S3"", ""GT2S0"", ""GT2S1"", ""GT2S2"", ""GT3S0"", ""GT3S1"", ""GT3S2"", ""GT3S3"", ""GTUS0"", ""GTUS1"", ""GTUS2""],
""IgnoredSensorsList"": [],
""RegisterSize"": 9,
""Slope"": 0.5,
""Offset"": -64,
""SetPoint"": ""100.0"",
""UpperTolerance"": ""20.0"",
""LowerTolerance"": ""20.0"",
""DatalogValues"": true,
""CompressedDatalog"": true,
""LastPattern"": true
}]";
            var settings = JsonConvert.DeserializeObject<List<Configuration>>(source);
            Prime.Services.SharedStorageService.InsertRowAtTable(DDG.DTS.DTSConfigurationTable, settings, Context.DUT);

            IDTS dts = new DTSImpl("LastPattern1");
            var ctv = "000000101100000101111000101101000101000100101011000101011000101000100101010000101000100101000100101010100101011000101011000101011000101011000101000100101011000101011000101000100101000100101011000101000100101010100101101000101101000101011000101011000101011000101011000101100100101001000101001000101000100101011000101100100101000100101110000101011000101101000101000100101011000101011000101000100101010000101000100101000100101010100101011000101011000101011000101011000101000100101011000101011000101000100101000100101011000101000100101010100101101000101101000101011000101011000101011000101011000101100100101001000101001000101000100101011000101100100101";
            var result = dts.ProcessCapturedData(ctv);
            Assert.IsTrue(result);
            this.datalogServiceMock.VerifyAll();
            this.strgValWriterMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void DTSHandler_DTSEnabled_Pass()
        {
            this.datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.strgValWriterMock.Object);
            this.strgValWriterMock.Setup(o => o.SetTnamePostfix("_DTS"));
            this.strgValWriterMock.Setup(o => o.SetData("SA:100.00|TPCS0:97.50|TPCS1:99.00|IPUS0:98.50|IPUS1:100.00|IPUS2:99.00|C0S0:99.00|C0S1:100.00|C0S2:97.00|C1S0:100.00|C1S1:100.00|C1S2:101.00|C2S0:99.00|C2S1:99.00|C2S2:99.00|C3S0:99.00|C3S1:100.00|C3S2:99.00|GT0S0:99.00|GT0S1:100.00|GT0S2:100.00|GT0S3:99.00|GT1S0:100.00|GT1S1:101.00|GT1S2:98.50|GT1S3:98.50|GT2S0:99.00|GT2S1:99.00|GT2S2:99.00|GT3S0:99.00|GT3S1:100.50|GT3S2:98.00|GT3S3:98.00|GTUS0:100.00|GTUS1:99.00|GTUS2:100.50"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgValWriterMock.Object));
            string source =
                @"[{
""Name"": ""LastPattern1"",
""IsEnabled"": true,
""PinName"": ""TDO"",
""SensorsList"": [""SA"", ""TPCS0"", ""TPCS1"", ""IPUS0"", ""IPUS1"", ""IPUS2"", ""C0S0"", ""C0S1"", ""C0S2"", ""C1S0"", ""C1S1"", ""C1S2"", ""C2S0"", ""C2S1"", ""C2S2"", ""C3S0"", ""C3S1"", ""C3S2"", ""GT0S0"", ""GT0S1"", ""GT0S2"", ""GT0S3"", ""GT1S0"", ""GT1S1"", ""GT1S2"", ""GT1S3"", ""GT2S0"", ""GT2S1"", ""GT2S2"", ""GT3S0"", ""GT3S1"", ""GT3S2"", ""GT3S3"", ""GTUS0"", ""GTUS1"", ""GTUS2""],
""IgnoredSensorsList"": [],
""RegisterSize"": 9,
""Slope"": 0.5,
""Offset"": -64,
""SetPoint"": ""100.0"",
""UpperTolerance"": ""20.0"",
""LowerTolerance"": ""20.0"",
""DatalogValues"": true,
""CompressedDatalog"": true,
""LastPattern"": true
}]";
            var settings = JsonConvert.DeserializeObject<List<Configuration>>(source);
            Prime.Services.SharedStorageService.InsertRowAtTable(DDG.DTS.DTSConfigurationTable, settings, Context.DUT);

            var handler = new DTSHandler();
            handler.SetConfiguration("LastPattern1");
            Assert.IsTrue(handler.IsDtsEnabled());
            Assert.AreEqual("TDO", handler.GetCtvPinName());
            Assert.AreEqual(324, handler.GetCtvCount());
            handler.Reset();
            var ctv = "000000101100000101111000101101000101000100101011000101011000101000100101010000101000100101000100101010100101011000101011000101011000101011000101000100101011000101011000101000100101000100101011000101000100101010100101101000101101000101011000101011000101011000101011000101100100101001000101001000101000100101011000101100100101000100101110000101011000101101000101000100101011000101011000101000100101010000101000100101000100101010100101011000101011000101011000101011000101000100101011000101011000101000100101000100101011000101000100101010100101101000101101000101011000101011000101011000101011000101100100101001000101001000101000100101011000101100100101";
            handler.ProcessPlistDts(ctv);
            Assert.IsTrue(handler.EvaluateLimits());

            this.datalogServiceMock.VerifyAll();
            this.strgValWriterMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void DTSHandler_DTSDisabled_Pass()
        {
            string source =
                @"[{
""Name"": ""LastPattern1"",
""IsEnabled"": false,
""PinName"": ""TDO"",
""SensorsList"": [""SA"", ""TPCS0"", ""TPCS1"", ""IPUS0"", ""IPUS1"", ""IPUS2"", ""C0S0"", ""C0S1"", ""C0S2"", ""C1S0"", ""C1S1"", ""C1S2"", ""C2S0"", ""C2S1"", ""C2S2"", ""C3S0"", ""C3S1"", ""C3S2"", ""GT0S0"", ""GT0S1"", ""GT0S2"", ""GT0S3"", ""GT1S0"", ""GT1S1"", ""GT1S2"", ""GT1S3"", ""GT2S0"", ""GT2S1"", ""GT2S2"", ""GT3S0"", ""GT3S1"", ""GT3S2"", ""GT3S3"", ""GTUS0"", ""GTUS1"", ""GTUS2""],
""IgnoredSensorsList"": [],
""RegisterSize"": 9,
""Slope"": 0.5,
""Offset"": -64,
""SetPoint"": ""100.0"",
""UpperTolerance"": ""20.0"",
""LowerTolerance"": ""20.0"",
""DatalogValues"": true,
""CompressedDatalog"": true,
""LastPattern"": true
}]";
            var settings = JsonConvert.DeserializeObject<List<Configuration>>(source);
            Prime.Services.SharedStorageService.InsertRowAtTable(DDG.DTS.DTSConfigurationTable, settings, Context.DUT);

            var handler = new DTSHandler();
            handler.SetConfiguration("LastPattern1");
            Assert.IsFalse(handler.IsDtsEnabled());
            Assert.AreEqual("TDO", handler.GetCtvPinName());
            Assert.AreEqual(324, handler.GetCtvCount());
            handler.Reset();
            var ctv = "000000101100000101111000101101000101000100101011000101011000101000100101010000101000100101000100101010100101011000101011000101011000101011000101000100101011000101011000101000100101000100101011000101000100101010100101101000101101000101011000101011000101011000101011000101100100101001000101001000101000100101011000101100100101000100101110000101011000101101000101000100101011000101011000101000100101010000101000100101000100101010100101011000101011000101011000101011000101000100101011000101011000101000100101000100101011000101000100101010100101101000101101000101011000101011000101011000101011000101100100101001000101001000101000100101011000101100100101";
            handler.ProcessPlistDts(ctv);
            Assert.IsTrue(handler.EvaluateLimits());
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void DTSHandler_MockService_Pass()
        {
            string source =
                @"[{
""Name"": ""LastPattern1"",
""IsEnabled"": false,
""PinName"": ""TDO"",
""SensorsList"": [""SA"", ""TPCS0"", ""TPCS1"", ""IPUS0"", ""IPUS1"", ""IPUS2"", ""C0S0"", ""C0S1"", ""C0S2"", ""C1S0"", ""C1S1"", ""C1S2"", ""C2S0"", ""C2S1"", ""C2S2"", ""C3S0"", ""C3S1"", ""C3S2"", ""GT0S0"", ""GT0S1"", ""GT0S2"", ""GT0S3"", ""GT1S0"", ""GT1S1"", ""GT1S2"", ""GT1S3"", ""GT2S0"", ""GT2S1"", ""GT2S2"", ""GT3S0"", ""GT3S1"", ""GT3S2"", ""GT3S3"", ""GTUS0"", ""GTUS1"", ""GTUS2""],
""IgnoredSensorsList"": [],
""RegisterSize"": 9,
""Slope"": 0.5,
""Offset"": -64,
""SetPoint"": ""100.0"",
""UpperTolerance"": ""20.0"",
""LowerTolerance"": ""20.0"",
""DatalogValues"": true,
""CompressedDatalog"": true,
""LastPattern"": true
}]";
            var settings = JsonConvert.DeserializeObject<List<Configuration>>(source);
            Prime.Services.SharedStorageService.InsertRowAtTable(DDG.DTS.DTSConfigurationTable, settings, Context.DUT);
            var dtsMock = new Mock<IDTS>(MockBehavior.Strict);
            dtsMock.Setup(o => o.GetSettings()).Returns(new Configuration());
            var dtsServiceMock = new Mock<IDTSFactory>(MockBehavior.Strict);
            dtsServiceMock.Setup(o => o.Get("LastPattern1")).Returns(dtsMock.Object);
            DDG.DTS.Service = dtsServiceMock.Object;

            var handler = new DTSHandler();
            handler.SetConfiguration("LastPattern1");
            Assert.IsFalse(handler.IsDtsEnabled());
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ProcessCapturedData_IncompleteCTV_Pass()
        {
            string source =
                @"[{
""Name"": ""LastPattern1"",
""IsEnabled"": true,
""PinName"": ""TDO"",
""SensorsList"": [""SA"", ""TPCS0"", ""TPCS1"", ""IPUS0"", ""IPUS1"", ""IPUS2"", ""C0S0"", ""C0S1"", ""C0S2"", ""C1S0"", ""C1S1"", ""C1S2"", ""C2S0"", ""C2S1"", ""C2S2"", ""C3S0"", ""C3S1"", ""C3S2"", ""GT0S0"", ""GT0S1"", ""GT0S2"", ""GT0S3"", ""GT1S0"", ""GT1S1"", ""GT1S2"", ""GT1S3"", ""GT2S0"", ""GT2S1"", ""GT2S2"", ""GT3S0"", ""GT3S1"", ""GT3S2"", ""GT3S3"", ""GTUS0"", ""GTUS1"", ""GTUS2""],
""IgnoredSensorsList"": [],
""RegisterSize"": 9,
""Slope"": 0.5,
""Offset"": -64,
""SetPoint"": ""100.0"",
""UpperTolerance"": ""20.0"",
""LowerTolerance"": ""20.0"",
""DatalogValues"": true,
""CompressedDatalog"": true,
""LastPattern"": true
}]";
            var settings = JsonConvert.DeserializeObject<List<Configuration>>(source);
            Prime.Services.SharedStorageService.InsertRowAtTable(DDG.DTS.DTSConfigurationTable, settings, Context.DUT);

            IDTS dts = new DTSImpl("LastPattern1");
            var ctv = "0";
            var result = dts.ProcessCapturedData(ctv);
            Assert.IsTrue(result);
            this.datalogServiceMock.VerifyAll();
            this.strgValWriterMock.VerifyAll();
        }
    }
}
