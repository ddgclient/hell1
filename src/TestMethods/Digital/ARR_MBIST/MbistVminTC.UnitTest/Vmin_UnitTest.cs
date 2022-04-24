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
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.DffService;
    using Prime.FileService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;

    /// <summary>
    /// MbistVminTC_UnitTest.
    /// </summary>
    [TestClass]
    public class Vmin_UnitTest : MbistVminTC
    {
        // private string testConfigFile = Directory.GetFiles("../../src/TestMethods/Digital/ARR_MBIST/MbistVminTC/InputFiles", "Exampleconfig_WW40_v2_PerPattern.json")[0];
        // private string testConfigFileKS = Directory.GetFiles("../../src/TestMethods/Digital/ARR_MBIST/MbistVminTC/InputFiles", "MBIST_HRY_KS.json")[0];
        private Mock<IPinMap> pinMapMock;
        private Mock<IVminForwardingCorner> vminForwardingMock;

        // private Mock<IVoltageConverter> voltageConverterMock;
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
            this.MbistTestMode = MbistVminTC.MbistTestModes.HRY;
            this.FeatureSwitchSettings = string.Empty;
            this.LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json";
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;

            // this.PatternModifications = string.Empty;
            this.CornerIdentifiers = string.Empty;
            this.FlowNumber = "-99";

            this.Threads = 2;
            this.Patlist = "Plist_BISR_1";
            this.LevelsTc = "SomeLevels";
            this.VoltageTargets = "Domain";
            this.ForceConfigFileParseState = EnableStates.Enabled;
            this.LogLevel = PrimeLogLevel.TEST_METHOD;

            // Default Mock for Shared service.
            this.sharedStorageValues = new Dictionary<string, string>();
            this.sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.sharedStorageMock.Setup(o =>
                    o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>()))
                .Callback((string key, object obj, Context context) =>
                {
                    System.Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.sharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                });
            this.sharedStorageMock.Setup(o =>
                    o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, string obj, Context context) =>
                {
                    System.Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.sharedStorageValues[key] = obj;
                });
            this.sharedStorageMock
                .Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>()))
                .Callback((string key, Type obj, Context context) =>
                {
                    System.Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.sharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Type obj, Context context) =>
                    JsonConvert.DeserializeObject(this.sharedStorageValues[key], obj));
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

            // this.VoltageConfiguration = "DLVR";
            // this.FivrCondition = "FivrCondition";
            this.vminForwardingMock = new Mock<IVminForwardingCorner>(MockBehavior.Strict);
            var vminForwardingFactoryMock = new Mock<IVminForwardingFactory>(MockBehavior.Strict);
            vminForwardingFactoryMock.Setup(f => f.Get(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(this.vminForwardingMock.Object);
            vminForwardingFactoryMock.Setup(o => o.IsSinglePointMode()).Returns(false);

            this.pinMapMock = new Mock<IPinMap>(MockBehavior.Strict);
            var pinMapFactoryMock = new Mock<IPinMapFactory>(MockBehavior.Strict);
            pinMapFactoryMock.Setup(p => p.Get(It.IsAny<string>())).Returns(this.pinMapMock.Object);

            // this.voltageConverterMock = new Mock<IVoltageConverter>(MockBehavior.Strict);
            // var voltageConverterFactoryMock = new Mock<IVoltageConverterFactory>(MockBehavior.Strict);
            // voltageConverterFactoryMock.Setup(v => v.Get(this.VoltageConfiguration, this.LevelsTc)).Returns(this.voltageConverterMock.Object);
            var fileservice = new Mock<IFileService>(MockBehavior.Strict);
            fileservice.Setup(f => f.GetFile(It.IsAny<string>())).Returns<string>(s =>
            {
                if (s.Contains("Exampleconfig_WW40_v2_PerPattern.json"))
                {
                    return Environment.CurrentDirectory +
                           "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Exampleconfig_WW40_v2_PerPattern.json";
                }
                else if (s.Contains("Recovery_v2.json"))
                {
                    return Environment.CurrentDirectory +
                           "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Recovery_v2.json";
                }
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            DDG.VminForwarding.Service = vminForwardingFactoryMock.Object;
            DDG.PinMap.Service = pinMapFactoryMock.Object;

            // DDG.VoltageConverter.Service = voltageConverterFactoryMock.Object;
            // Default Mock for Console Service
            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>()))
                .Callback((string s) => { System.Console.WriteLine(s); });
            this.ConsoleServiceMock
                .Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, int, string, string>((string msg, int line, string n, string src) =>
                {
                    System.Console.WriteLine($"ERROR: {msg}");
                });
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;
        }

        /// <summary> Tests JSON Write/Read SharedStorage DFF for VMIN. </summary>
        [TestMethod]
        public void StorageCheckoutVmin_incremental()
        {
            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var strgvalWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strgvalWriterMock.Setup(o => o.SetCustomTname("MbistVminPerMem"));
            strgvalWriterMock.Setup(o => o.SetData("VCCSA:0.7,VCCIO:0.8"));
            strgvalWriterMock.Setup(o => o.SetData("0.7,0.7,0.7,0.8"));
            ituffMock.Setup(o => o.WriteToItuff(strgvalWriterMock.Object));
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strgvalWriterMock.Object);
            /* ituffMock.Setup(o => o.WriteToItuff($"2_tname_MbistVminPerMem\n2_strgval_VCCSA:0.7,VCCIO:0.8\n"));
            ituffMock.Setup(o => o.WriteToItuff($"2_tname_MbistVminPerMem\n2_strgval_0.7,0.7,0.7,0.8\n")); */
            Prime.Services.DatalogService = ituffMock.Object;

            var hryref = new List<string>() { "T", "S", "R", "U" };
            var vminlookup = new List<string>() { "VCCSA", "VCCSA", "VCCIO", "VCCIO" };
            var vmintargets = new List<string>() { "VCCIO", "VCCSA" };
            var vmin = new Vmin(hryref, vminlookup, vmintargets);
            vmin.Setvmin(0, .7);
            vmin.Setvmin(1, .7);

            vmin.Setvmin(2, .7);
            vmin.Setvmin(3, .8);
            vmin.VminWriteSharedStorage();
            vmin.VminReadSharedStorage();

            Assert.AreEqual(.7, vmin.Grabvmin(0));
            Assert.AreEqual(.7, vmin.Grabvmin(1));

            Assert.AreEqual(.7, vmin.Grabvmin(2));
            Assert.AreEqual(.8, vmin.Grabvmin(3));

            vmin.PrintDataToItuffPerArrayVmin();
            vmin.PrintDataToItuffPerDomain();
        }

        /// <summary> Tests JSON Write/Read SharedStorage DFF for VMIN. </summary>
        [TestMethod]
        public void StorageCheckoutVminNoVoltageFound()
        {
            var ituffMock = new Mock<Prime.DatalogService.IDatalogService>(MockBehavior.Strict);
            var strgvalWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strgvalWriterMock.Setup(o => o.SetCustomTname("MbistVminPerMem"));
            strgvalWriterMock.Setup(o => o.SetData("VCCSA:-9999,VCCIO:-5555"));
            strgvalWriterMock.Setup(o => o.SetData("0.7,-9999,-5555,0.8"));
            ituffMock.Setup(o => o.WriteToItuff(strgvalWriterMock.Object));
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strgvalWriterMock.Object);
            /* ituffMock.Setup(o => o.WriteToItuff($"2_tname_MbistVminPerMem\n2_strgval_VCCSA:-9999,VCCIO:-5555\n"));
            ituffMock.Setup(o => o.WriteToItuff($"2_tname_MbistVminPerMem\n2_strgval_0.7,-9999,-5555,0.8\n")); */
            Prime.Services.DatalogService = ituffMock.Object;

            var hryref = new List<string>() { "T", "S", "R", "U" };
            var vminlookup = new List<string>() { "VCCSA", "VCCSA", "VCCIO", "VCCIO" };
            var vmintargets = new List<string>() { "VCCIO", "VCCSA" };
            var vmin = new Vmin(hryref, vminlookup, vmintargets);
            vmin.Setvmin(0, .7);
            vmin.Setvmin(1, -9999);
            vmin.Setvmin(3, .8);
            vmin.VminWriteSharedStorage();
            vmin.VminReadSharedStorage();

            Assert.AreEqual(-9999, vmin.Grabvmin(1));
            Assert.AreEqual(.7, vmin.Grabvmin(0));
            Assert.AreEqual(-5555, vmin.Grabvmin(2));
            Assert.AreEqual(.8, vmin.Grabvmin(3));

            vmin.PrintDataToItuffPerArrayVmin();
            vmin.PrintDataToItuffPerDomain();
        }
    }
}
