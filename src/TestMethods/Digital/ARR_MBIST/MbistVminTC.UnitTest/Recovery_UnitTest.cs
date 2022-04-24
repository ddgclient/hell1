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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using DDG;
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
    public class Recovery_UnitTest : MbistVminTC
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

            // this.VoltageConfiguration = "DLVR";
            // this.FivrCondition = "FivrCondition";
            this.vminForwardingMock = new Mock<IVminForwardingCorner>(MockBehavior.Strict);
            var vminForwardingFactoryMock = new Mock<IVminForwardingFactory>(MockBehavior.Strict);
            vminForwardingFactoryMock.Setup(f => f.Get(It.IsAny<string>(), It.IsAny<int>())).Returns(this.vminForwardingMock.Object);
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
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Exampleconfig_WW40_v2_PerPattern.json";
                }
                else if (s.Contains("Recovery_v2.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Recovery_v2.json";
                }
                else if (s.Contains("Recovery_v2_DUPLICATES.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Recovery_v2_DUPLICATES.json";
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
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => { System.Console.WriteLine(s); });
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
            Callback<string, int, string, string>((string msg, int line, string n, string src) => { System.Console.WriteLine($"ERROR: {msg}"); });
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;
        }

        /// <summary>
        /// Tests JSON Parser for Recovery.
        /// </summary>
        [TestMethod]
        public void RecoveryJSONParse()
        {
            var fileservicemock = new Mock<IFileService>(MockBehavior.Loose);
            fileservicemock.Setup(f => f.GetFile(It.IsAny<string>())).Returns(Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Recovery.json");
            Prime.Services.FileService = fileservicemock.Object;
            var undertest = new Recovery();
            var temp = undertest.RecoveryJsonParser("TIM");
            Assert.IsNotNull(temp);

            this.RecoveryModeDownbin = EnableStates.Enabled;
            List<string> lookup = new List<string>() { "LEHP_BP34_MEM1", "LEHP_BP35_MEM1", "LEHP_BP35_MEM2", "LEHP_BP36_MEM1", "LEH_BP34_MEM1", "LEH_BP35_MEM1", "LEH_BP36_MEM1" };
            ConcurrentDictionary<int, char> hrystring = new ConcurrentDictionary<int, char>();

            hrystring.TryAdd(0, (char)Hry.ResultNameChar.Pass_retest);
            hrystring.TryAdd(1, (char)Hry.ResultNameChar.Repairable);
            hrystring.TryAdd(2, (char)Hry.ResultNameChar.Pass);
            hrystring.TryAdd(3, (char)Hry.ResultNameChar.Fail);
            hrystring.TryAdd(4, (char)Hry.ResultNameChar.Untested);
            hrystring.TryAdd(5, (char)Hry.ResultNameChar.Pass);
            hrystring.TryAdd(6, (char)Hry.ResultNameChar.Pass);

            var duplicate = temp.BuildIPsRecovery(lookup);
            Assert.IsFalse(duplicate);
            var recoverystring = new List<char>() { '0', '0', '2', '3', '0', '0', '0', '0', '2' };
            var result2 = undertest.ParseResults(hrystring, temp, recoverystring, MbistVminTC.EnableStates.Enabled);

            var testresults = new Recovery.RecoveryResults()
            {
                RecoveryString = new List<char>() { '0', '3', '2', '3', '1', '0', '1', '0', '1' },
                Remove_Mems = new Dictionary<string, List<string>>()
                {
                    { "VPU", new List<string> { "LEHP_BP36_MEM1" } },
                    { "VPU2", new List<string> { "LEHP_BP36_MEM1" } },
                    { "VPU3", new List<string> { "LEHP_BP36_MEM1", "LEHP_BP35_MEM1", "LEHP_BP34_MEM1" } },
                    { "IPU", new List<string> { "LEHP_BP36_MEM1", "LEHP_BP35_MEM1", "LEHP_BP35_MEM2" } },
                    { "IPU3", new List<string> { "LEHP_BP36_MEM1", "LEHP_BP34_MEM1" } },
                },
                RecoveryModes = new List<string> { "Partial", "Partial", "Partial", "Partial", "Partial", "Partial", "Full", "Full", "Full" },
            };

            Assert.AreEqual(result2, testresults);
        }

        /// <summary> Tests Recovery JSON Parser fail. </summary>
        [TestMethod]
        public void RecoveryJsonParserFail()
        {
            var fileservicemock = new Mock<IFileService>(MockBehavior.Loose);
            fileservicemock.Setup(f => f.GetFile(It.IsAny<string>())).Returns(string.Empty);
            Prime.Services.FileService = fileservicemock.Object;
            var undertest = new Recovery();

            Assert.IsNull(undertest.RecoveryfileParse(EnableStates.Disabled, string.Empty, new List<string>()));
        }

        /// <summary> Tests Recovery JSON Parser fail. </summary>
        [TestMethod]
        public void RecoveryJsonParserFail_DueToincorrectInput()
        {
            var undertest = new Recovery();

            Assert.IsNull(undertest.RecoveryfileParse(EnableStates.Disabled, "Recovery_v2_DUPLICATES.json", new List<string>()));
        }

        /// <summary> Tests JSON Write/Read SharedStorage DFF for recovery. </summary>
        [TestMethod]
        public void StorageCheckoutRecovery()
        {
            var recovery = new Recovery();
            var writevalue = new List<char>() { '1', '1', '1', '1' };
            recovery.WriteData(writevalue, true);
            var temp = recovery.ReadData(true);
            Assert.AreEqual(writevalue.ToString(), temp.ToString());
            temp = recovery.ReadData(false);
            Assert.AreEqual(writevalue.ToString(), temp.ToString());
        }

        /// <summary> Tests JSON Write/Read SharedStorage DFF for recovery. </summary>
        [TestMethod]
        public void DuplicateRecoveryIPsFail()
        {
            var hry_string = new List<string>()
            {
                "LEHP_BP34_WBP0_MEM1", "LEHP_BP34_WBP0_MEM2", "LEHP_BP34_WBP0_MEM3", "LEHP_BP35_WBP0_MEM1",
                "LEHP_BP35_WBP0_MEM2", "LEHP_BP35_WBP0_MEM3", "LEHP_BP35_WBP1_MEM1", "LEHP_BP35_WBP1_MEM2",
                "LEHP_BP35_WBP1_MEM3", "LEHP_BP36_WBP0_MEM1", "LEHP_BP36_WBP0_MEM2", "LEHP_BP36_WBP0_MEM3",
                "LEHP_BP36_WBP1_MEM1", "LEHP_BP36_WBP1_MEM2", "LEHP_BP36_WBP1_MEM3", "LEHP_BP37_WBP0_MEM1",
                "LEHP_BP37_WBP0_MEM2", "LEHP_BP37_WBP0_MEM3", "LEHP_BP37_WBP0_MEM4", "LEHP_BP38_WBP0_MEM1",
                "LEHP_BP38_WBP0_MEM2", "LEHP_BP38_WBP0_MEM3", "LEHP_BP38_WBP0_MEM4", "LEHP_BP38_WBP1_MEM1",
                "LEHP_BP38_WBP1_MEM2", "LEHP_BP38_WBP1_MEM3", "LEHP_BP39_WBP0_MEM1", "LEHP_BP39_WBP0_MEM2",
                "LEHP_BP39_WBP1_MEM1", "LEHP_BP39_WBP1_MEM2", "LEHP_BP40_WBP0_MEM10", "LEHP_BP40_WBP0_MEM21",
                "LEHP_BP40_WBP0_MEM33", "LEHP_BP40_WBP1_MEM1", "LEHP_BP40_WBP1_MEM2", "LEHP_BP40_WBP1_MEM3",
                "LEHP_BP42_WBP0_MEM10", "LEHP_BP42_WBP0_MEM21", "LEHP_BP42_WBP0_MEM33", "LEHP_BP43_WBP0_MEM45",
                "LEHP_BP43_WBP0_MEM46", "LEHP_BP43_WBP0_MEM47", "LEHP_BP44_WBP0_MEM12", "LEHP_BP44_WBP0_MEM2",
                "LEHP_BP44_WBP0_MEM21", "LEHP_BP44_WBP0_MEM22", "LEHP_BP44_WBP1_MEM11", "LEHP_BP44_WBP1_MEM12",
                "LEHP_BP44_WBP1_MEM2",
            };
            var fileservicemock = new Mock<IFileService>(MockBehavior.Loose);
            fileservicemock.Setup(f => f.GetFile(It.IsAny<string>())).Returns(Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Recovery.json");
            Prime.Services.FileService = fileservicemock.Object;
            var undertest = new Recovery();

            var testrecoveryjson = undertest.RecoveryfileParse(EnableStates.Disabled, string.Empty, new List<string>());
            testrecoveryjson.Hry_string = null;
            Assert.IsNull(testrecoveryjson.Hry_string);
            Assert.AreEqual(1.0, testrecoveryjson.Version);
            testrecoveryjson.DesignIPs["IPU3"].RequiredForAllmodes = new List<string>() { "LEHP_BP36" };
            Assert.IsTrue(testrecoveryjson.BuildIPsRecovery(hry_string));
            testrecoveryjson.DesignIPs["IPU3"].RecoveryOption["1"] = new List<string>() { "LEHP_BP34", "LEHP_BP34_WBP0_MEM1" };
            Assert.IsTrue(testrecoveryjson.BuildIPsRecovery(hry_string));
        }
    }
}
