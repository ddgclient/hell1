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

// ---------------------------------------------------------------
// Created By Tim Kirkham
// ---------------------------------------------------------------
namespace MbistVminTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.DffService;
    using Prime.FileService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;

    /// <summary>
    /// MbistVminTC_UnitTest.
    /// </summary>
    [TestClass]
    public class Mapping_UnitTest : MbistVminTC
    {
        private Mock<ISharedStorageService> sharedStorageMock;
        private Dictionary<string, string> sharedStorageValues;
        private Dictionary<string, string> dffdata;
        private Mock<IDffService> dffStorageMock;

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private Mock<ITestProgramService> TestProgramServiceMock { get; set; }

        private Mock<ITestProgramService> TestProgramServiceMockString { get; set; }

        private Hry Testhry { get; set; }

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            // Default Mock for Shared service.
            this.sharedStorageValues = new Dictionary<string, string>();
            this.sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>()))
                .Callback((string key, object obj, Context context) =>
                {
                    System.Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.sharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                });
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, string obj, Context context) =>
                {
                    System.Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.sharedStorageValues[key] = obj;
                });
            this.sharedStorageMock.Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>()))
                .Callback((string key, Type obj, Context context) =>
                {
                    System.Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.sharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Type obj, Context context) => JsonConvert.DeserializeObject(this.sharedStorageValues[key], obj));
            this.sharedStorageMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    System.Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.sharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => this.sharedStorageValues[key]);
            this.sharedStorageMock.Setup(o => o.KeyExistsInObjectTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    if (this.sharedStorageValues.ContainsKey(key))
                    {
                        System.Console.WriteLine($"SharedStorage Key={key} exists in table.");
                    }
                    else
                    {
                        System.Console.WriteLine($"SharedStorage Key={key} exists in table.");
                    }
                })
                .Returns((string key, Context context) => this.sharedStorageValues.ContainsKey(key));
            Prime.Services.SharedStorageService = this.sharedStorageMock.Object;

            // Default Mock for Shared service.
            this.dffdata = new Dictionary<string, string>();
            this.dffStorageMock = new Mock<IDffService>(MockBehavior.Strict);
            this.dffStorageMock.Setup(o => o.SetDff(It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string key, string value) =>
                {
                    System.Console.WriteLine($"Saving DFF Key={key}, Value written {value}");
                    if (!this.dffdata.ContainsKey(key))
                    {
                        this.dffdata.Add(key, value);
                    }
                    else
                    {
                        this.dffdata[key] = value;
                    }
                });
            this.dffStorageMock.Setup(o => o.GetDff(It.IsAny<string>(), true))
                .Callback((string key, bool log) =>
                {
                    System.Console.WriteLine($"Reading DFF Key={key}, Value expected {this.dffdata[key]}");
                }).Returns((string key, bool log) => this.dffdata[key]);
            Prime.Services.DffService = this.dffStorageMock.Object;

            // this.voltageConverterMock = new Mock<IVoltageConverter>(MockBehavior.Strict);
            // var voltageConverterFactoryMock = new Mock<IVoltageConverterFactory>(MockBehavior.Strict);
            // voltageConverterFactoryMock.Setup(v => v.Get(this.VoltageConfiguration, this.LevelsTc)).Returns(this.voltageConverterMock.Object);
            var fileservice = new Mock<IFileService>(MockBehavior.Strict);
            fileservice.Setup(f => f.GetFile(It.IsAny<string>())).Returns<string>(s =>
            {
                if (s.Contains("Exampleconfig_WW40_v2_PerPattern.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Exampleconfig_WW40_v2_PerPattern.json";
                }
                else if (s.Contains("Recovery_v2.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Recovery_v2.json";
                }
                else if (s.Contains("SharedStortoDFFMap.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\SharedStortoDFFMap.json";
                }
                else if (s.Contains("colVirtFuse.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\colVirtFuse.json";
                }
                else if (s.Contains("SharedStortoDFFMap_bad.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\SharedStortoDFFMap_bad.json";
                }
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            // DDG.VoltageConverter.Service = voltageConverterFactoryMock.Object;
            // Default Mock for Console Service
            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => { System.Console.WriteLine(s); });
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
            Callback<string, int, string, string>((string msg, int line, string n, string src) => { System.Console.WriteLine($"ERROR: {msg}"); });
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;
        }

        /// <summary> Tests JSON Parser for Mapping. </summary>
        [TestMethod]
        public void MappingJsonParser()
        {
            var undertest = new Mapping();

            undertest.LoadMappingConfig(EnableStates.Enabled, "SharedStortoDFFMap.json");
            Assert.IsNotNull(undertest.Map);
            Assert.AreEqual(1.0, undertest.Map.Version);
            Assert.AreEqual(string.Empty, undertest.Map.TokenHRYName(global::MbistVminTC.MappingJsonParser.TPTypes.DFF, "KS"));
            Assert.AreEqual(string.Empty, undertest.Map.TokenHRYName(global::MbistVminTC.MappingJsonParser.TPTypes.ITUFF, "KS"));
            Assert.AreEqual(string.Empty, undertest.Map.TokenHRYName(global::MbistVminTC.MappingJsonParser.TPTypes.SharedStorage, "KS"));
            Assert.AreEqual(string.Empty, undertest.Getname(global::MbistVminTC.MappingJsonParser.Fields.Hry, global::MbistVminTC.MappingJsonParser.TPTypes.DFF));
            Assert.AreEqual(true, undertest.Map.DieToPin["TDO"].Voltages.ContainsKey("VCCSA"));
            Assert.AreEqual("VCCSA", undertest.Map.DieToPin["TDO"].Voltages["VCCSA"]);
            Assert.AreEqual(string.Empty, undertest.Map.DieToPin["TDO"].Shortname);
        }

        /// <summary> Tests JSON Parser for Mapping fail. </summary>
        [TestMethod]
        public void MappingJsonParserFails()
        {
            var undertest = new Mapping();
            undertest.MapConfig("SharedStortoDFFMap_bad.json");
            Assert.IsNull(undertest.Map);
            undertest.LoadMappingConfig(EnableStates.Disabled, string.Empty);
            Assert.IsNull(undertest.Map);
            undertest.LoadMappingConfig(EnableStates.Disabled, "SharedStortoDFFMap.json");
            Assert.IsNotNull(undertest.Map);
        }
    }
}
