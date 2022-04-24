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
    using System.Linq;
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
    public class Hry_UnitTest : MbistVminTC
    {
        /// <summary>
        /// Hry tests for Plist_1 from the test config file.
        /// </summary>
        private readonly Dictionary<string, CTVExampleTest> hryCtvTests = new Dictionary<string, CTVExampleTest>()
        {
            { "HryPass", new CTVExampleTest("Plist_1", "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },
            { "HryFail_Execution1", new CTVExampleTest("Plist_2", "00100010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "UUU011111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },
            { "HryFail_Execution2", new CTVExampleTest("Plist_2", "00000000000000000010001000000000000000000000000000000000000000000000000000000000000000000000000000000", "UUU110111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },

            // Palgo || Algo failure == misprogrammed
            { "HryPatternMisprogram", new CTVExampleTest("Plist_1", "0010100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "MMMUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },
            { "HryPatternMisprogram_Pattern2", new CTVExampleTest("Plist_2", "0000000000000000000000000000000000000000000000000010000000000000010000000000000000000000000000000000", "UUU111MMMUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },

            // When PostDone in Status bit is a failure (0000, the LSB, far right), but all passes in the mems == Bad WaitTime
            { "HryBadWaitTime", new CTVExampleTest("Plist_1", "00010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "WWWUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },
            { "HryBadWaitTime_Execution2", new CTVExampleTest("Plist_1", "00000000000000000001000000000000000000000000000000000000000000000000000000000000000000000000000000000", "111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },
            { "HryControllerIssue_1", new CTVExampleTest("Plist_1", "10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "888UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },
            { "HryControllerIssue_2", new CTVExampleTest("Plist_1", "01000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "888UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },

            // if Status==ALL_PASSthenMEM==ALL_PASSelseinconsistent
            { "HryInconsistentPS_FGOID_1", new CTVExampleTest("Plist_1", "00000000000001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "666UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },
            { "HryInconsistentPS_FGOID_2", new CTVExampleTest("Plist_2", "00001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "UUU661111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },

            // if Status==ALL_PASSthenStatus==FAILthen1ormoreMEMs==FAILelseinconsistent
            { "HryInconsistentFS_PGOID_1", new CTVExampleTest("Plist_1", "0010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "555UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },
            { "HryInconsistentFS_PGOID_2", new CTVExampleTest("Plist_2", "0000000000000000000000000000000000000000000000000010000000000000000000000000000000000000000000000000", "UUU111555UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },

            // BIRA tests for Plist_4 from the test config file.
            // General Pass/Fail
            { "BiraPassNoRepair", new CTVExampleTest("Plist_4", "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "UUUUUUUUUUUUUUU1111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },
            { "BiraPassRepair", new CTVExampleTest("Plist_4", "00100000010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "UUUUUUUUUUUUUUUY111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },
            { "BiraFailUnrepairable1", new CTVExampleTest("Plist_4", "00100000100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "UUUUUUUUUUUUUUUN111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },
            { "BiraFailUnrepairable2", new CTVExampleTest("Plist_4", "00100000001000000100000110000000000000000000000000000000000000000000000000000000000000000000000000000", "UUUUUUUUUUUUUUU10N1UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },

            // Global failures
            { "BiraControllerIssue", new CTVExampleTest("Plist_4", "01000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "UUUUUUUUUUUUUUU8888UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },

            // update                                                                                                                                                                     WW possible have 1 execution pass and other fail.
            { "BiraBadWaitTime", new CTVExampleTest("Plist_4", "00010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "UUUUUUUUUUUUUUUWW11UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },

            // update                                                                                                                                                                         MM possible have miss programming in ine execution and not in other.
            { "BiraPatternMisprogram", new CTVExampleTest("Plist_4", "00000000000000000100010000000000000000000000000000000000000000000000000000000000000000000000000000000", "UUUUUUUUUUUUUUU11MMUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") },

            // Repairable Tests
            { "BiraInconsistentFS_PGOID", new CTVExampleTest("Plist_4", "00100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "UUUUUUUUUUUUUUU5511UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") }, // if Status==ALL_PASSthenMEM==ALL_PASelseinconsistent
            { "BiraInconsistentPS_FGOID", new CTVExampleTest("Plist_4", "00000000000000000000000110000000000000000000000000000000000000000000000000000000000000000000000000000", "UUUUUUUUUUUUUUU1166UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU") }, // if Status==ALL_PASSthenMEM==ALL_PASelseinconsistent

            // Reparabletests
            { "PreRepair", new CTVExampleTest("Plist_7",   "00100110000000000010010000000001000000000000000000000000000000000000001000000110000001000000100000001000000100000001000000100000000000", "UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU010WWWY0Y1Y1Y") },
            { "PostRepair", new CTVExampleTest("Plist_7r", "00100110000000000010010000000001000000000000000000000000000000000000001000000010000000100001100000000000000000000000000000000000000000", "UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU010WWWP0F7P1P") },
        };

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

        // Hry Tests ==================================================================================================================================================

        /// <summary>
        /// Verifies a successful passing ctv returns generates the correct passing HryString.
        /// </summary>
        [TestMethod]
        public void HryPass()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "HryPass";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Verifies a pattern miss programming condition is successfully found and categorized.
        /// </summary>
        [TestMethod]
        public void HryFail_Execution1()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "HryFail_Execution1";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Verifies a pattern miss programming condition is successfully found and categorized. This failure should only be found in the second execution.
        /// </summary>
        [TestMethod]
        public void HryFail_Execution2()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "HryFail_Execution2";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Verifies a pattern miss programming condition is successfully found and it overrides previous execution memory locations in the hry string as this
        /// is a controller level global status.
        /// </summary>
        [TestMethod]
        public void HryPatternMisprogram()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "HryPatternMisprogram";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Verifies a pattern miss programming condition is successfully found and categorized.
        /// </summary>
        [TestMethod]
        public void HryPatternMisprogram_Pattern2()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "HryPatternMisprogram_Pattern2";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Verifies a pattern bad wait time condition is successfully found and categorized.
        /// </summary>
        [TestMethod]
        public void HryBadWaitTime()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "HryBadWaitTime";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Verifies a controller failure is successfully found.
        /// </summary>
        [TestMethod]
        public void HryControllerIssue_1()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "HryControllerIssue_1";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Verifies a controller failure is successfully found and it overrides previous execution memory locations in the hry string as this
        /// is a controller level global status.
        /// </summary>
        [TestMethod]
        public void HryControllerIssue_2()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "HryControllerIssue_2";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Verifies a the Inconsistent Pass Status Fail Go condition is met and the hrystring is updated accurately.
        /// </summary>
        [TestMethod]
        public void HryInconsistentPS_FGOID_1()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "HryInconsistentPS_FGOID_1";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Verifies a the Inconsistent Pass Status Fail Go condition is met and it only a execution global override for its contained memories. This should
        /// not effect other executions.
        /// </summary>
        [TestMethod]
        public void HryInconsistentPS_FGOID_2()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "HryInconsistentPS_FGOID_2";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Verifies a the Inconsistent Fail Status Pass Go bits condition is met and it only a execution global override for its contained memories. This should
        /// not effect other executions.
        /// </summary>
        [TestMethod]
        public void HryInconsistentFS_PGOID_1()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "HryInconsistentFS_PGOID_1";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Verifies a the Inconsistent Fail Status Pass Go bits condition is met and it only a execution global override for its contained memories. This should
        /// not effect other executions.
        /// </summary>
        [TestMethod]
        public void HryInconsistentFS_PGOID_2()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "HryInconsistentFS_PGOID_2";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Verifies the Shared Storage Gsds HryString updates with new values when running multiple plist
        /// back to back. After one plist's results are categorized the results are stored in shared storage. After
        /// starting on the second plist, the Gsds Hry String gets retrieved then updated with the new results in the
        /// correct controller memory positions based on the lookuptable.
        /// </summary>
        [TestMethod]
        public void HryMultiPlist_HryStringUpdate()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "HryPatternMisprogram";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);

            plist = "HryInconsistentPS_FGOID_2";
            this.Patlist = this.hryCtvTests[plist].Plist;
            this.Testhry.RunAllCTVPerPlist(this.Patlist, this.hryCtvTests[plist].CtvString, vfdmlookup);
            var resulthry = this.CheckHryStringPriority(this.Testhry.CurrentHryString, globalhry);
            System.Console.WriteLine($"hryStringResults: {string.Join(",", globalhry)}");
            Assert.AreEqual("MMM661111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", string.Join(string.Empty, globalhry));
        }

        // Bira Tests ==================================================================================================================================================

        /// <summary>
        /// Bira Plist: Verifies a the Pass condition is met and categorized accurately.
        /// </summary>
        [TestMethod]
        public void BiraPassNoRepair()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "BiraPassNoRepair";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Bira Plist: Verifies a the Pass Repairable condition is met and categorized accurately.
        /// </summary>
        [TestMethod]
        public void BiraPassRepair()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "BiraPassRepair";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Bira Plist: Verifies a the Pattern Miss program condition is met and categorized accurately (2 memory bits: 10).
        /// </summary>
        [TestMethod]
        public void BiraFailUnrepairable1()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "BiraFailUnrepairable1";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Bira Plist: Verifies a the Fail Unrepairable condition is met and categorized accurately for another failure condition (2 memory bits: 11).
        /// </summary>
        [TestMethod]
        public void BiraFailUnrepairable2()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "BiraFailUnrepairable2";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Bira Plist: Verifies a the Controller condition is met and categorized accurately.
        /// </summary>
        [TestMethod]
        public void BiraControllerIssue()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "BiraControllerIssue";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Bira Plist: Verifies a the Bad Wait Time condition is met and categorized accurately.
        /// </summary>
        [TestMethod]
        public void BiraBadWaitTime()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "BiraBadWaitTime";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Bira Plist: Verifies a the Pattern Miss program condition is met and categorized accurately.
        /// </summary>
        [TestMethod]
        public void BiraPatternMisprogram()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "BiraPatternMisprogram";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Bira Plist: Verifies a the Inconsistent Fail Status Pass Go bits condition is met and it only a execution global override for its contained memories. This should
        /// not effect other executions.
        /// </summary>
        [TestMethod]
        public void BiraInconsistentFS_PGOID()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "BiraInconsistentFS_PGOID";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Bira Plist: Verifies a the Inconsistent Pass Status Fail Go bits condition is met and it only a execution global override for its contained memories. This should
        /// not effect other executions.
        /// </summary>
        [TestMethod]
        public void BiraInconsistentPS_FGOID()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "BiraInconsistentPS_FGOID";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>
        /// Verifies the Shared Storage Gsds HryString updates with new values when running multiple plist
        /// back to back. After one plist's results are categorized the results are stored in shared storage. After
        /// starting on the second plist, the Gsds Hry String gets retrieved then updated with the new results in the
        /// correct controller memory positions based on the lookuptable.
        /// </summary>
        [TestMethod]
        public void BiraPostRepairFlow()
        {
            var vfdmlookup = new Virtualfuse();
            var plist = "PreRepair";
            var globalhry = new List<char>();
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);

            this.MbistTestMode = MbistVminTC.MbistTestModes.PostRepair;
            plist = "PostRepair";
            globalhry = this.RunHRYTest(globalhry, plist, vfdmlookup);
        }

        /// <summary>Tests JSON Parser for Hry.</summary>
        [TestMethod]
        public void HRYperPatternParse()
        {
            var patlist = "Plist_BISR_1";
            var undertest = new Hry();
            undertest.HryfileParse(EnableStates.Enabled, "Exampleconfig_WW40_v2_PerPattern.json", patlist);

            Assert.IsNotNull(undertest.HryLookupTable);
        }

        /// <summary>Tests JSON Parser for Hry.</summary>
        [TestMethod]
        public void HRYFailParse_stringempty()
        {
            var fileservicemock = new Mock<IFileService>(MockBehavior.Loose);
            fileservicemock.Setup(f => f.GetFile(It.IsAny<string>())).Returns(string.Empty);
            Prime.Services.FileService = fileservicemock.Object;
            var undertest = new Hry();
            undertest.HryJsonParser(string.Empty);

            Assert.IsNull(undertest.HryLookupTable);
        }

        /// <summary>TestFunctionality of GlobablFieldCheck Status 11.</summary>
        [TestMethod]
        public void GlobalFieldsCheck_STATUS11()
        {
            var undertest = new Hry();
            var dict = new Dictionary<string, int>();
            dict.Add("1", 1);
            undertest.CurrentHryString = new ConcurrentDictionary<int, char>();
            var result = undertest.GlobalFieldsCheck("0", "11", dict);
            Assert.IsTrue(result.Item1);
            Assert.IsFalse(result.Item2);
        }

        /// <summary>TestFunctionality of GlobablFieldCheck Status 11.</summary>
        [TestMethod]
        public void GlobalFieldsCheck_STATUS0()
        {
            var undertest = new Hry();
            var dict = new Dictionary<string, int>();
            dict.Add("1", 1);
            var result = undertest.GlobalFieldsCheck("0", "0", dict);
            Assert.IsFalse(result.Item1);
            Assert.IsTrue(result.Item2);
        }

        /// <summary>TestFunctionality of GlobablFieldCheck Status 11.</summary>
        [TestMethod]
        public void GlobalFieldsCheck_STATUS1()
        {
            var undertest = new Hry();
            var dict = new Dictionary<string, int>();
            dict.Add("1", 1);
            var result = undertest.GlobalFieldsCheck("0", "0", dict);
            Assert.IsFalse(result.Item1);
            Assert.IsTrue(result.Item2);
        }

        /// <summary>TestFunctionality of GlobablFieldCheck Status 11.</summary>
        [TestMethod]
        public void GlobalFieldsCheck_STATUSOUTOFRANGE()
        {
            var undertest = new Hry();
            var dict = new Dictionary<string, int>();
            dict.Add("1", 1);
            var result = undertest.GlobalFieldsCheck("0", "111", dict);
        }

        /// <summary>TestFunctionality of GlobablFieldCheck Status 11.</summary>
        [TestMethod]
        public void ExtractOther_NonDash()
        {
            var undertest = new Hry();
            Assert.AreEqual("101", undertest.ExtractOther("00001010", "4-6"));
            Assert.AreEqual("101", undertest.ExtractOther("00001010", "6-4"));
        }

        /// <summary>Tests JSON Parser for Hry.</summary>
        [TestMethod]
        public void HRYFailParse_badfile()
        {
            var undertest = new Hry();
            undertest.HryJsonParser(Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\SharedStortoDFFMap_bad.json");

            Assert.IsNull(undertest.HryLookupTable);
        }

        /// <summary> Takes HRY from current run and aggregates it to global by priority. </summary>
        /// <param name="currentHryString"> If set later skips recovery.</param>
        /// <param name="globalHrystring"> This contains the HRY string.</param>
        /// <returns>Final HRY string.</returns>
        public List<char> CheckHryStringPriority(ConcurrentDictionary<int, char> currentHryString, List<char> globalHrystring)
        {
            foreach (KeyValuePair<int, char> updatehry in currentHryString)
            {
                switch (this.MbistTestMode)
                {
                    case MbistTestModes.PostRepair:

                        // post repair update flow
                        if (globalHrystring[updatehry.Key] == (char)Hry.ResultNameChar.Repairable)
                        {
                            if (updatehry.Value == (char)Hry.ResultNameChar.Fail)
                            {
                                globalHrystring[updatehry.Key] = (char)Hry.ResultNameChar.Fail_retest;
                            }
                            else if (updatehry.Value == (char)Hry.ResultNameChar.Pass)
                            {
                                globalHrystring[updatehry.Key] = (char)Hry.ResultNameChar.Pass_retest;
                            }
                            else
                            {
                                globalHrystring[updatehry.Key] = updatehry.Value;
                            }
                        }
                        else
                        {
                            if (globalHrystring[updatehry.Key] == (char)Hry.ResultNameChar.Pass && updatehry.Value == (char)Hry.ResultNameChar.Fail)
                            {
                                globalHrystring[updatehry.Key] = (char)Hry.ResultNameChar.Inconsist_pst_fail;
                            }
                            else
                            {
                                globalHrystring[updatehry.Key] = this.Testhry.ChoosePriorityResult(globalHrystring[updatehry.Key], currentHryString[updatehry.Key]);
                            }
                        }

                        break;
                    default:
                        globalHrystring[updatehry.Key] = this.Testhry.ChoosePriorityResult(globalHrystring[updatehry.Key], currentHryString[updatehry.Key]);
                        break;
                }
            }

            System.Console.WriteLine($"Updated sharedStorage value  : [{string.Join(string.Empty, globalHrystring)}]");
            return globalHrystring;
        }

        /// <summary> Runs the HRY test. </summary>
        /// <param name="globalhry"> Global HRY value.</param>
        /// <param name="plist"> Plist you are running.</param>
        /// <param name="vfdmlookup"> VFDMlookup if running VFDM mode.</param>
        /// <returns>Final HRY string.</returns>
        public List<char> RunHRYTest(List<char> globalhry, string plist, Virtualfuse vfdmlookup)
        {
            var mbistlookup = new HryJsonParser();
            this.Patlist = this.hryCtvTests[plist].Plist;
            this.Testhry = new Hry();
            mbistlookup = this.Testhry.HryJsonParser(Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Exampleconfig_WW40_v2_PerPattern.json");
            mbistlookup.BuildIndex();
            mbistlookup.BuildBisr(this.Patlist);
            this.Testhry.HryLookupTable = mbistlookup;
            if (globalhry.Count() == 0)
            {
                globalhry = this.Testhry.HryLookupTable.HryStringRef.Select(u => 'U').ToList<char>();
            }

            this.Testhry.CurrentHryString = new ConcurrentDictionary<int, char>();
            this.Testhry.RunAllCTVPerPlist(this.Patlist, this.hryCtvTests[plist].CtvString, vfdmlookup);
            var resulthry = this.CheckHryStringPriority(this.Testhry.CurrentHryString, globalhry);
            System.Console.WriteLine($"hryStringResults: {string.Join(",", resulthry)}");
            Assert.AreEqual(this.hryCtvTests[plist].ExpectedResult, string.Join(string.Empty, resulthry));
            return resulthry;
        }

        private class CTVExampleTest
        {
            public CTVExampleTest(string plist, string ctv, string result)
            {
                this.Plist = plist;
                this.CtvString = ctv;
                this.ExpectedResult = result;
            }

            public string Plist { get; set; }

            public string CtvString { get; set; }

            public string ExpectedResult { get; set; }
        }
    }
}
