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
namespace MbistVminTC.UnitTest
{
    // ---------------------------------------------------------------
    // Created By Tim Kirkham
    // ---------------------------------------------------------------
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
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PerformanceService;
    using Prime.PinService;
    using Prime.PlistService;
    using Prime.ScoreboardService;
    using Prime.SharedStorageService;
    using Prime.TestConditionService;
    using Prime.TestProgramService;

    /// <summary>
    /// MbistVminTC_UnitTest.
    /// </summary>
    [TestClass]
    public class MbistVminTC_UnitTest : MbistVminTC
    {
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

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// RunScoreBoard_Tid_Pass.
        /// </summary>
        [TestMethod]
        public void RunFullTestCase_Pass()
        {
            string fakeTC = "FakeLevels";
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
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            var plistservice = new Mock<IPlistService>(MockBehavior.Strict);

            var plistobj = new Mock<IPlistObject>(MockBehavior.Strict);
            plistobj.Setup(v => v.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            plistservice.Setup(v => v.GetPlistObject(It.IsAny<string>())).Returns(plistobj.Object);
            Prime.Services.PlistService = plistservice.Object;

            List<IPatConfigHandle> patdata = new List<IPatConfigHandle>();
            var patdataMock = new Mock<IPatConfigHandle>();
            patdataMock.Setup(v => v.SetData("1"));
            patdata.Add(patdataMock.Object);
            var pathandleMock = new Mock<IPatConfigService>(MockBehavior.Loose);

            // pathandleMock.Setup(v => v.Apply(patdata));
            Prime.Services.PatConfigService = pathandleMock.Object;

            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("HRY_RAWSTR_MBIST", "UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU");
            this.sharedStorageValues.Add("SOCRecovery", "10");
            this.sharedStorageValues.Add("BISR_LEG_BISR1_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR1_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_FUSE", string.Empty);
            this.sharedStorageValues.Add("MbistVminPerMem", "-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555");

            // Mock the voltage service.
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Apply(new List<double> { 0.5 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.6 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.7 }));
            voltageMock.Setup(o => o.Restore());
            voltageMock.Setup(o => o.Reset());

            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "VCCSA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(voltageMock.Object);
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("VCCSA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition("FakeLevels")).Returns(testCondition.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.Get("VCCSA")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.SetupSequence(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            // Mock the functional test service.
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist(null)).Returns("Plist_1");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.SetupSequence(o => o.GetFailingPinNames()).Returns(new List<string> { { "TDO" } })
                                                              .Returns(new List<string> { { "TDO" } })
                                                              .Returns(new List<string>());
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("bal", 0, 0));

            // .Returns(new List<string>());
            funcTestMock.SetupSequence(o => o.GetCtvData(It.IsAny<string>())).Returns("001000001000000000000000")
                                                      .Returns("001000001000000000000000")
                                                      .Returns("000000000000000000000000");

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
                .Returns(new List<IFailureData> { failDataMock.Object }) // 0.5
                .Returns(new List<IFailureData> { failDataMock.Object }) // 0.5
                .Returns(new List<IFailureData> { failDataMock.Object }) // 0.6
                .Returns(new List<IFailureData> { failDataMock.Object }) // 0.6
                .Returns(new List<IFailureData>()); // 0.7

            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.DatalogFailure(1));

            // funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false) // 0.5
                .Returns(false) // 0.6
                .Returns(true); // 0.7

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("Plist_1", fakeTC, fakeTC, It.IsAny<List<string>>(), int.MaxValue, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var finalHry = "111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU";

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("Media_F3:1:1.5"));
            strValWriterMock.Setup(o => o.SetData("0.700|0.500|1.000|3"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);

            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            /* ituffMock.Setup(o => o.WriteToItuff($"2_tname_VMIN\n2_strgval_VCCSA:-5555\n")); */
            strValWriterMock.Setup(o => o.SetCustomTname("VMIN"));
            strValWriterMock.Setup(o => o.SetData("VCCSA:-5555"));
            /* ituffMock.Setup(o => o.WriteToItuff($"2_tname_HRY_RAWSTR_MBIST\n2_strgval_{finalHry}\n")); */
            strValWriterMock.Setup(o => o.SetCustomTname("HRY_RAWSTR_MBIST"));
            strValWriterMock.Setup(o => o.SetData(finalHry));
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            var instanceToTest = new MbistVminTC
            {
                VoltageTargets = "VCCSA",
                StepSize = "0.1",
                Patlist = "Plist_1",
                EndVoltageLimits = "1.0",
                StartVoltages = "0.5",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                RecoveryModeDownbin = EnableStates.Enabled,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                VFDMconfig = string.Empty,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                TestMode = TestModes.SingleVmin,
                MbistTestMode = MbistTestModes.HRY,
                ClearVariables = string.Empty,
                CtvPins = "TDO",
                CornerIdentifiers = "Media_F3",

                // RecoveryConfigurationFile = string.Empty,
                RecoveryConfigurationFile = "Recovery_v2.json",
                PrintToItuff = ItuffPrint.Hry_VminPerDomain,
                ForceConfigFileParseState = EnableStates.Enabled,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());
            Assert.AreEqual("111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", this.sharedStorageValues["HRY_RAWSTR_MBIST"]);
            Assert.AreEqual("10", this.sharedStorageValues["SOCRecovery"]);

            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(3));
            ituffMock.VerifyAll();
        }

        /// <summary>
        /// RunScoreBoard_Tid_Pass.
        /// </summary>
        [TestMethod]
        public void RunFullTestCase_Fail()
        {
            string fakeTC = "FakeLevels";
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
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            List<IPatConfigHandle> patdata = new List<IPatConfigHandle>();
            var patdataMock = new Mock<IPatConfigHandle>();
            patdataMock.Setup(v => v.SetData("1"));
            patdata.Add(patdataMock.Object);
            var pathandleMock = new Mock<IPatConfigService>(MockBehavior.Loose);

            // pathandleMock.Setup(v => v.Apply(patdata));
            Prime.Services.PatConfigService = pathandleMock.Object;

            var plistservice = new Mock<IPlistService>(MockBehavior.Strict);

            var plistobj = new Mock<IPlistObject>(MockBehavior.Strict);
            plistobj.Setup(v => v.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            plistservice.Setup(v => v.GetPlistObject(It.IsAny<string>())).Returns(plistobj.Object);
            Prime.Services.PlistService = plistservice.Object;

            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("HRY_RAWSTR_MBIST", "UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU");
            this.sharedStorageValues.Add("SOCRecovery", "10");
            this.sharedStorageValues.Add("BISR_LEG_BISR1_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR1_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_FUSE", string.Empty);
            this.sharedStorageValues.Add("MbistVminPerMem", "-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555");

            // Mock the voltage service.
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Apply(new List<double> { 0.5 }));
            voltageMock.Setup(o => o.Restore());
            voltageMock.Setup(o => o.Reset());

            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "VCCSA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(voltageMock.Object);
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("VCCSA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition("FakeLevels")).Returns(testCondition.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.Get("VCCSA")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            // Mock the functional test service.
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist(null)).Returns("Plist_1");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.SetupSequence(o => o.GetFailingPinNames()).Returns(new List<string> { { "TDO" } });
            funcTestMock.Setup(p => p.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("bal", 0, 0));

            // .Returns(new List<string>());
            funcTestMock.Setup(o => o.GetCtvData(It.IsAny<string>())).Returns("001000001000000000000000");

            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }); // 0.5

            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.DatalogFailure(1));

            // funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("IP_CPU::Plist_1", fakeTC, fakeTC, It.IsAny<List<string>>(), int.MaxValue, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var finalHry = "011UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU";

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("Media_F3:1:1.5"));
            strValWriterMock.Setup(o => o.SetData("-9999|0.500|0.500|1"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);

            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            /* ituffMock.Setup(o => o.WriteToItuff($"2_tname_VMIN\n2_strgval_VCCSA:-5555\n")); */
            strValWriterMock.Setup(o => o.SetCustomTname("VMIN"));
            strValWriterMock.Setup(o => o.SetData("VCCSA:-9999"));
            /* ituffMock.Setup(o => o.WriteToItuff($"2_tname_HRY_RAWSTR_MBIST\n2_strgval_{finalHry}\n")); */
            strValWriterMock.Setup(o => o.SetCustomTname("HRY_RAWSTR_MBIST"));
            strValWriterMock.Setup(o => o.SetData(finalHry));
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            var instanceToTest = new MbistVminTC
            {
                VoltageTargets = "VCCSA",
                StepSize = "0.1",
                Patlist = "IP_CPU::Plist_1",
                EndVoltageLimits = "0.5",
                StartVoltages = "0.5",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                RecoveryModeDownbin = EnableStates.Enabled,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                VFDMconfig = string.Empty,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                TestMode = TestModes.SingleVmin,
                MbistTestMode = MbistTestModes.HRY,
                ClearVariables = string.Empty,
                CtvPins = "TDO",
                CornerIdentifiers = "Media_F3",

                // RecoveryConfigurationFile = string.Empty,
                RecoveryConfigurationFile = string.Empty,
                PrintToItuff = ItuffPrint.Hry_VminPerDomain,
                ForceConfigFileParseState = EnableStates.Enabled,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());
            Assert.AreEqual("011UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", this.sharedStorageValues["HRY_RAWSTR_MBIST"]);
            Assert.AreEqual("10", this.sharedStorageValues["SOCRecovery"]);

            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
            ituffMock.VerifyAll();
        }

        /// <summary>
        /// RunScoreBoard_Tid_Pass.
        /// </summary>
        [TestMethod]
        public void RunFullTestCase_PassNoPerVmin()
        {
            string fakeTC = "FakeLevels";
            var fileservice = new Mock<IFileService>(MockBehavior.Strict);
            fileservice.Setup(f => f.GetFile(It.IsAny<string>())).Returns<string>(s =>
            {
                if (s.Contains("Exampleconfig_WW40_v2_PerPattern.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Exampleconfig_WW40_v2_PerPattern_novmin.json";
                }
                else if (s.Contains("Recovery_v2.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Recovery_v2.json";
                }
                else if (s.Contains("SharedStortoDFFMap.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\SharedStortoDFFMap.json";
                }
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            var plistservice = new Mock<IPlistService>(MockBehavior.Strict);

            var plistobj = new Mock<IPlistObject>(MockBehavior.Strict);
            plistobj.Setup(v => v.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            plistservice.Setup(v => v.GetPlistObject(It.IsAny<string>())).Returns(plistobj.Object);
            Prime.Services.PlistService = plistservice.Object;

            List<IPatConfigHandle> patdata = new List<IPatConfigHandle>();
            var patdataMock = new Mock<IPatConfigHandle>();
            patdataMock.Setup(v => v.SetData("1"));
            patdata.Add(patdataMock.Object);
            var pathandleMock = new Mock<IPatConfigService>(MockBehavior.Loose);

            // pathandleMock.Setup(v => v.Apply(patdata));
            Prime.Services.PatConfigService = pathandleMock.Object;

            // Initialize shared storage values.
            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("HRY_RAWSTR_MBIST", "UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU");
            this.sharedStorageValues.Add("SOCRecovery", "11");
            this.sharedStorageValues.Add("BISR_LEG_BISR1_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR1_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_FUSE", string.Empty);
            this.sharedStorageValues.Add("MbistVminPerMem", "-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555");

            // Mock the voltage service.
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Apply(new List<double> { 0.5 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.6 }));
            voltageMock.Setup(o => o.Apply(new List<double> { 0.7 }));
            voltageMock.Setup(o => o.Restore());
            voltageMock.Setup(o => o.Reset());

            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "VCCSA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(voltageMock.Object);
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("VCCSA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition("FakeLevels")).Returns(testCondition.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.Get("VCCSA")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            // Mock the functional test service.
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist(null)).Returns("Plist_1");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.SetupSequence(o => o.GetFailingPinNames()).Returns(new List<string> { { "TDO" } })
                                                              .Returns(new List<string> { { "TDO" } })
                                                              .Returns(new List<string>());
            funcTestMock.Setup(p => p.SetPinMask(new List<string>()));

            // .Returns(new List<string>());
            funcTestMock.SetupSequence(o => o.GetCtvData(It.IsAny<string>())).Returns("001000001000000000000000")
                                                      .Returns("001000001000000000000000")
                                                      .Returns("000000000000000000000000");

            funcTestMock.SetupSequence(o => o.GetPerCycleFailures())
            .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.5
            .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.5
            .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.6
            .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }) // 0.6
            .Returns(new List<Prime.FunctionalService.IFailureData>()); // 0.7

            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.DatalogFailure(1));
            funcTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("bal", 0, 0));

            // funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false) // 0.5
                .Returns(false) // 0.6
                .Returns(true); // 0.7

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("Plist_1", fakeTC, fakeTC, It.IsAny<List<string>>(), int.MaxValue, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var finalHry = "111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU";

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("Media_F3:1:1.5"));
            strValWriterMock.Setup(o => o.SetData("0.700|0.500|1.000|3"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);

            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            strValWriterMock.Setup(o => o.SetCustomTname("HRY_RAWSTR_MBIST"));
            strValWriterMock.Setup(o => o.SetData(finalHry));
            /* ituffMock.Setup(o => o.WriteToItuff($"2_tname_HRY_RAWSTR_MBIST\n2_strgval_{finalHry}\n")); */
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            var instanceToTest = new MbistVminTC
            {
                VoltageTargets = "VCCSA",
                StepSize = "0.1",
                Patlist = "Plist_1",
                EndVoltageLimits = "1.0",
                StartVoltages = "0.5",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                RecoveryModeDownbin = EnableStates.Enabled,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                VFDMconfig = string.Empty,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                TestMode = TestModes.SingleVmin,
                MappingConfig = "SharedStortoDFFMap.json",
                MbistTestMode = MbistTestModes.HRY,
                ClearVariables = string.Empty,
                CtvPins = "TDO",
                CornerIdentifiers = "Media_F3",

                // RecoveryConfigurationFile = string.Empty,
                RecoveryConfigurationFile = "Recovery_v2.json",
                PrintToItuff = ItuffPrint.Hry,
                ForceConfigFileParseState = EnableStates.Enabled,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());
            Assert.AreEqual("111UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", this.sharedStorageValues["HRY_RAWSTR_MBIST"]);
            Assert.AreEqual("11", this.sharedStorageValues["SOCRecovery"]);

            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(3));
            ituffMock.VerifyAll();
        }

        /// <summary>
        /// RunScoreBoard_Tid_Pass.
        /// </summary>
        [TestMethod]
        public void RunFullTestCase_FailwithRecoveryVFDMEnabled()
        {
            string fakeTC = "FakeLevels";
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
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            List<IPatConfigHandle> patdata = new List<IPatConfigHandle>();
            var patdataMock = new Mock<IPatConfigHandle>();
            patdataMock.Setup(v => v.SetData("1"));
            patdata.Add(patdataMock.Object);
            var pathandleMock = new Mock<IPatConfigService>(MockBehavior.Loose);

            // pathandleMock.Setup(v => v.Apply(patdata));
            Prime.Services.PatConfigService = pathandleMock.Object;

            var plistservice = new Mock<IPlistService>(MockBehavior.Strict);

            var plistobj = new Mock<IPlistObject>(MockBehavior.Strict);
            plistobj.Setup(v => v.Resolve());
            plistobj.Setup(v => v.GetOption("ReturnOn")).Returns("LocalStickyError");
            plistobj.Setup(v => v.SetOption("ReturnOn", "GlobalStickyError"));
            plistobj.Setup(v => v.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            plistservice.Setup(v => v.GetPlistObject(It.IsAny<string>())).Returns(plistobj.Object);
            Prime.Services.PlistService = plistservice.Object;

            this.sharedStorageValues.Add("HRY_RAWSTR_MBIST", "U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U");
            this.sharedStorageValues.Add("SOCRecovery", "10");

            // Mock the voltage service.
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Apply(new List<double> { 0.5 }));
            voltageMock.Setup(o => o.Restore());
            voltageMock.Setup(o => o.Reset());
            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "VCCSA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(voltageMock.Object);
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("VCCSA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition("FakeLevels")).Returns(testCondition.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.Get("VCCSA")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            // Mock the functional test service.
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist(null)).Returns("Plist_1");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { { "TDO" } });
            funcTestMock.Setup(o => o.GetCtvData(It.IsAny<string>())).Returns("001000001000000000000000");
            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }); // 0.5
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.DatalogFailure(1));
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.Execute()).Returns(false); // 0.5
            funcTestMock.Setup(p => p.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("bal", 0, 0));

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("Plist_1", fakeTC, fakeTC, It.IsAny<List<string>>(), int.MaxValue, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var finalHry = "011UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU";

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strValWriterMock.Setup(o => o.SetData("Media_F3:1:1.5"));
            strValWriterMock.Setup(o => o.SetData("0.500|0.500|0.500|1"));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            /* ituffMock.Setup(o => o.WriteToItuff($"2_tname_HRY_RAWSTR_MBIST\n2_strgval_{finalHry}\n")); */
            strValWriterMock.Setup(o => o.SetCustomTname("HRY_RAWSTR_MBIST"));
            strValWriterMock.Setup(o => o.SetData(finalHry));
            /* ituffMock.Setup(o => o.WriteToItuff($"2_tname_VMIN\n2_strgval_-9999,0.5,0.5,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555\n")); */
            strValWriterMock.Setup(o => o.SetCustomTname("VMIN"));
            strValWriterMock.Setup(o => o.SetData("-9999,0.5,0.5,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555"));
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));

            Prime.Services.DatalogService = ituffMock.Object;

            var instanceToTest = new MbistVminTC
            {
                VoltageTargets = "VCCSA",
                StepSize = "0.1",
                Patlist = "Plist_1",
                EndVoltageLimits = "0.5",
                StartVoltages = "0.5",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                RecoveryModeDownbin = EnableStates.Enabled,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
                TestMode = TestModes.SingleVmin,
                VFDMconfig = "colVirtFuse.json",
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                MbistTestMode = MbistTestModes.KS,
                ClearVariables = "vmin,vfdm,bisr,hry",
                CtvPins = "TDO",
                DffOperation = DFFOperation.Disabled,
                CornerIdentifiers = "Media_F3",

                // RecoveryConfigurationFile = string.Empty,
                RecoveryConfigurationFile = "Recovery_v2.json",
                PrintToItuff = ItuffPrint.Hry_VminPerMem,
                ForceConfigFileParseState = EnableStates.Disabled,
                Threads = 4,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(3, instanceToTest.Execute());
            Assert.AreEqual("011UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", this.sharedStorageValues["HRY_RAWSTR_MBIST"]);
            Assert.AreEqual("13", this.sharedStorageValues["SOCRecovery"]);

            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
            ituffMock.VerifyAll();
            plistobj.VerifyAll();
        }

        /// <summary>
        /// Build string.
        /// </summary>
        [TestMethod]
        public void BuildStringQuicksim()
        {
            var temp = new BuildQuickSimString();
            var sttemp = temp.Buildstring("arr_mbist_x_x_tap_all_hry_all_all_parallelallsteps_ks_list", Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\MBIST_HRY_50x.json");
        }

        /// <summary>Bisr FullTestCase.
        /// </summary>
        [TestMethod]
        public void RunFullTestCase_Bisr_Scoarboard()
        {
            string fakeTC = "FakeLevels";
            var fileservice = new Mock<IFileService>(MockBehavior.Strict);
            fileservice.Setup(f => f.GetFile(It.IsAny<string>())).Returns<string>(s =>
            {
                if (s.Contains("Exampleconfig_WW40_v2_PerPattern.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Exampleconfig_WW40_v2_PerPattern_novmin.json";
                }
                else if (s.Contains("Recovery_testcase.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Recovery_testcase.json";
                }
                else if (s.Contains("SharedStortoDFFMap.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\SharedStortoDFFMap.json";
                }
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            var plistservice = new Mock<IPlistService>(MockBehavior.Strict);

            var plistobj = new Mock<IPlistObject>(MockBehavior.Strict);
            plistobj.Setup(v => v.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            plistservice.Setup(v => v.GetPlistObject(It.IsAny<string>())).Returns(plistobj.Object);
            Prime.Services.PlistService = plistservice.Object;

            var patdataMock = new Mock<IPatConfigHandle>();
            var pathandleMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            pathandleMock.Setup(v => v.GetPatConfigHandle(It.IsAny<string>())).Returns(patdataMock.Object);

            pathandleMock.Setup(v => v.Apply(patdataMock.Object));
            Prime.Services.PatConfigService = pathandleMock.Object;

            this.sharedStorageValues.Add("HRY_RAWSTR_MBIST", "U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U");
            this.sharedStorageValues.Add("SOCRecovery", "1");

            // Scoarboard MOCKING
            var scoreBoardServiceMock = new Mock<IScoreboardService>(MockBehavior.Strict);
            var loggerMock = new Mock<IScoreboardLogger>(MockBehavior.Strict);
            loggerMock.Setup(o => o.ProcessFailData(new List<string> { "mb_lehp_aud_csesram_a_ssa_pmovi_x_s_RA1" }));
            scoreBoardServiceMock.Setup(o => o.CreateLogger(9911, "1,2,3,4,5,6,7", 1000)).Returns(loggerMock.Object);
            Prime.Services.ScoreBoardService = scoreBoardServiceMock.Object;

            // Mock the voltage service.
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Restore());
            voltageMock.Setup(o => o.Reset());

            // Vmin search parameters.
            // voltageMock.Setup(o => o.Apply(new List<double> { 1.0 }));
            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "VCCSA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(voltageMock.Object);
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("VCCSA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition("FakeLevels")).Returns(testCondition.Object);

            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.Get("VCCSA")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            // Mock the functional test service.
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist(null)).Returns("Plist_BISR_1");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.SetupSequence(o => o.GetFailingPinNames()).Returns(new List<string> { { "TDO" } });
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("bal", 0, 0));

            // .Returns(new List<string>());
            funcTestMock.Setup(o => o.GetCtvData(It.IsAny<string>())).Returns("000000000100000001000000110000100010000000000000111111");

            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }); // 0.7
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.DatalogFailure(1));

            // funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute()).Returns(false); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("Plist_BISR_1", fakeTC, fakeTC, It.IsAny<List<string>>(), int.MaxValue, 1, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var finalHry = "UUUUUUUUUUUUUUUY1Y0UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU";

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);

            // Needed for Vmin searches
            // strValWriterMock.Setup(o => o.SetData("Media_F3:1:1.500"));
            // strValWriterMock.Setup(o => o.SetData("1.000|1.000|1.000|1"));

            // strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);

            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            /* ituffMock.Setup(o => o.WriteToItuff($"2_tname_HRY_RAWSTR_MBIST\n2_strgval_{finalHry}\n")); */
            strValWriterMock.Setup(o => o.SetCustomTname("HRY_RAWSTR_MBIST"));
            strValWriterMock.Setup(o => o.SetData(finalHry));

            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            var instanceToTest = new MbistVminTC
            {
                VoltageTargets = "VCCSA",
                StepSize = "0.1",
                Patlist = "Plist_BISR_1",
                EndVoltageLimits = "1.0",
                StartVoltages = "1.0",
                TestMode = TestModes.Scoreboard,
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                RecoveryModeDownbin = EnableStates.Enabled,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
                VFDMconfig = string.Empty,
                BisrMode = BisrModes.Compressed,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                MbistTestMode = MbistTestModes.HRY,
                ClearVariables = "all",
                CtvPins = "TDO",
                ForwardingMode = MbistVminTC.ForwardingModes.None,

                // RecoveryConfigurationFile = string.Empty,
                RecoveryConfigurationFile = "Recovery_testcase.json",

                PrintToItuff = ItuffPrint.Hry,
                ForceConfigFileParseState = EnableStates.Enabled,
                PatternNameMap = "1,2,3,4,5,6,7",
                ScoreboardMaxFails = 1000,
                ScoreboardBaseNumberMbist = "9911",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(4, instanceToTest.Execute());
            Assert.AreEqual("100010000000000000111111", this.sharedStorageValues["BISR_BP0_RAW"]);
            Assert.AreEqual("01100010011011111110000", this.sharedStorageValues["BISR_BP0_FUSE"]);
            Assert.AreEqual("UUUUUUUUUUUUUUUY1Y0UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", this.sharedStorageValues["HRY_RAWSTR_MBIST"]);
            Assert.AreEqual("2", this.sharedStorageValues["SOCRecovery"]);

            strValWriterMock.VerifyAll();
            pathandleMock.VerifyAll();
            patdataMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
            ituffMock.VerifyAll();
        }

        /// <summary>Bisr FullTestCase SingleVmin. </summary>
        [TestMethod]
        public void RunFullTestCase_Bisr_SingleVmin()
        {
            string fakeTC = "FakeLevels";
            var fileservice = new Mock<IFileService>(MockBehavior.Strict);
            fileservice.Setup(f => f.GetFile(It.IsAny<string>())).Returns<string>(s =>
            {
                if (s.Contains("Exampleconfig_WW40_v2_PerPattern.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Exampleconfig_WW40_v2_PerPattern_novmin.json";
                }
                else if (s.Contains("Recovery_testcase.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Recovery_testcase.json";
                }
                else if (s.Contains("SharedStortoDFFMap.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\SharedStortoDFFMap.json";
                }
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            var patdataMock = new Mock<IPatConfigHandle>();

            var pathandleMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            pathandleMock.Setup(v => v.GetPatConfigHandle(It.IsAny<string>())).Returns(patdataMock.Object);
            pathandleMock.Setup(v => v.Apply(It.IsAny<IPatConfigHandle>()));

            // pathandleMock.Setup(v => v.Apply(patdataMock.Object));
            Prime.Services.PatConfigService = pathandleMock.Object;

            var plistservice = new Mock<IPlistService>(MockBehavior.Strict);

            var plistobj = new Mock<IPlistObject>(MockBehavior.Strict);
            plistobj.Setup(v => v.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            plistservice.Setup(v => v.GetPlistObject(It.IsAny<string>())).Returns(plistobj.Object);
            Prime.Services.PlistService = plistservice.Object;

            this.sharedStorageValues.Add("HRY_RAWSTR_MBIST", "U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U");
            this.sharedStorageValues.Add("SOCRecovery", "1");

            // Scoarboard MOCKING
            var scoreBoardServiceMock = new Mock<IScoreboardService>(MockBehavior.Strict);
            var loggerMock = new Mock<IScoreboardLogger>(MockBehavior.Strict);
            loggerMock.Setup(o => o.ProcessFailData(new List<string> { "g1234567" }));
            scoreBoardServiceMock.Setup(o => o.CreateLogger(9911, "1,2,3,4,5,6,7", 1000)).Returns(loggerMock.Object);
            Prime.Services.ScoreBoardService = scoreBoardServiceMock.Object;

            // Mock the voltage service.
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Restore());
            voltageMock.Setup(o => o.Reset());

            // Vmin search parameters.
            voltageMock.Setup(o => o.Apply(new List<double> { 1.0 }));
            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "VCCSA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(voltageMock.Object);
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("VCCSA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition("FakeLevels")).Returns(testCondition.Object);

            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.Get("VCCSA")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("g1234567");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            // Mock the functional test service.
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist(null)).Returns("Plist_BISR_1");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.SetupSequence(o => o.GetFailingPinNames()).Returns(new List<string> { { "TDO" } });
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("g1234567", 0, 0));

            // .Returns(new List<string>());
            funcTestMock.Setup(o => o.GetCtvData(It.IsAny<string>())).Returns("000000000100000001000000110000100010000000000000111111");

            funcTestMock.Setup(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }); // 0.7
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.DatalogFailure(1));

            // funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute())
                .Returns(false); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("Plist_BISR_1", fakeTC, fakeTC, It.IsAny<List<string>>(), int.MaxValue, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var finalHry = "UUUUUUUUUUUUUUUY1Y0UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU";

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);

            // Needed for Vmin searches
            // strValWriterMock.Setup(o => o.SetData("Media_F3:1:1.500"));
            strValWriterMock.Setup(o => o.SetData("1.000|1.000|1.000|1"));

            // strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);

            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            /* ituffMock.Setup(o => o.WriteToItuff($"2_tname_HRY_RAWSTR_MBIST\n2_strgval_{finalHry}\n")); */
            strValWriterMock.Setup(o => o.SetCustomTname("HRY_RAWSTR_MBIST"));
            strValWriterMock.Setup(o => o.SetData(finalHry));
            strValWriterMock.Setup(o => o.SetTnamePostfix("_lp"));
            strValWriterMock.Setup(o => o.SetData("1234567"));

            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            var instanceToTest = new MbistVminTC
            {
                VoltageTargets = "VCCSA",
                StepSize = "0.1",
                Patlist = "Plist_BISR_1",
                EndVoltageLimits = "1.0",
                StartVoltages = "1.0",
                TestMode = TestModes.SingleVmin,
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                RecoveryModeDownbin = EnableStates.Enabled,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
                VFDMconfig = string.Empty,
                BisrMode = BisrModes.Compressed,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                MbistTestMode = MbistTestModes.HRY,
                ClearVariables = "all",
                CtvPins = "TDO",
                ForwardingMode = MbistVminTC.ForwardingModes.None,

                // RecoveryConfigurationFile = string.Empty,
                RecoveryConfigurationFile = "Recovery_testcase.json",

                PrintToItuff = ItuffPrint.Hry,
                ForceConfigFileParseState = EnableStates.Enabled,
                PatternNameMap = "1,2,3,4,5,6,7",
                ScoreboardMaxFails = 1000,
                ScoreboardBaseNumberMbist = "9911",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(4, instanceToTest.Execute());
            Assert.AreEqual("100010000000000000111111", this.sharedStorageValues["BISR_BP0_RAW"]);
            Assert.AreEqual("01100010011011111110000", this.sharedStorageValues["BISR_BP0_FUSE"]);
            Assert.AreEqual("UUUUUUUUUUUUUUUY1Y0UUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", this.sharedStorageValues["HRY_RAWSTR_MBIST"]);

            strValWriterMock.VerifyAll();
            pathandleMock.VerifyAll();
            patdataMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
            ituffMock.VerifyAll();
        }

        /// <summary>Bisr FullTestCase.
        /// </summary>
        [TestMethod]
        public void RunFullTestCase_VFDM_Functional()
        {
            string fakeTC = "FakeLevels";
            var fileservice = new Mock<IFileService>(MockBehavior.Strict);
            fileservice.Setup(f => f.GetFile(It.IsAny<string>())).Returns<string>(s =>
            {
                if (s.Contains("Exampleconfig_WW40_v2_PerPattern.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Exampleconfig_WW40_v2_PerPattern_novmin.json";
                }
                else if (s.Contains("Recovery_testcase.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\Recovery_testcase.json";
                }
                else if (s.Contains("SharedStortoDFFMap.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\SharedStortoDFFMap.json";
                }
                else if (s.Contains("colVirtFuse_vfdmtestcase.json"))
                {
                    return Environment.CurrentDirectory + "\\..\\..\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\colVirtFuse_vfdmtestcase.json";
                }
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            var patdataMock = new Mock<IPatConfigHandle>();

            var pathandleMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            pathandleMock.Setup(v => v.GetPatConfigHandle(It.IsAny<string>())).Returns(patdataMock.Object);

            pathandleMock.Setup(v => v.Apply(patdataMock.Object));
            Prime.Services.PatConfigService = pathandleMock.Object;

            var plistservice = new Mock<IPlistService>(MockBehavior.Strict);

            var plistobj = new Mock<IPlistObject>(MockBehavior.Strict);
            plistobj.Setup(v => v.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            plistservice.Setup(v => v.GetPlistObject(It.IsAny<string>())).Returns(plistobj.Object);
            Prime.Services.PlistService = plistservice.Object;

            // Initialize sharedstorage values
            this.sharedStorageValues.Add("HRY_RAWSTR_MBIST", "U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U");
            this.sharedStorageValues.Add("SOCRecovery", "1");

            // Initialization of shared storage values
            // var shstormock = new Mock<ISharedStorageService>(MockBehavior.Loose);
            // shstormock.Setup(v => v.GetStringRowFromTable("HRY_RAWSTR_MBIST", Context.DUT))
            //                .Returns("U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U,U");
            // shstormock.Setup(v => v.GetStringRowFromTable("recoverydata", Context.DUT))
            //                .Returns("1");
            // Prime.Services.SharedStorageService = shstormock.Object;

            // Scoarboard MOCKING
            var scoreBoardServiceMock = new Mock<IScoreboardService>(MockBehavior.Strict);
            var loggerMock = new Mock<IScoreboardLogger>(MockBehavior.Strict);
            loggerMock.Setup(o => o.ProcessFailData(new List<string> { "g1234567" }));
            scoreBoardServiceMock.Setup(o => o.CreateLogger(9911, "1,2,3,4,5,6,7", 1000)).Returns(loggerMock.Object);
            Prime.Services.ScoreBoardService = scoreBoardServiceMock.Object;

            // Mock the voltage service.
            var voltageMock = new Mock<Prime.VoltageService.IVForcePinAttribute>(MockBehavior.Strict);
            voltageMock.Setup(o => o.Restore());
            voltageMock.Setup(o => o.Reset());

            // Vmin search parameters.
            // voltageMock.Setup(o => o.Apply(new List<double> { 1.0 }));
            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "VCCSA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(voltageMock.Object);
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("VCCSA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition("FakeLevels")).Returns(testCondition.Object);

            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.Get("VCCSA")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            // Mock the functional test service.
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist(null)).Returns("Plist_1_VFDM");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { { "TDO" } });
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));

            // .Returns(new List<string>());
            funcTestMock.Setup(o => o.GetCtvData(It.IsAny<string>())).Returns("001000001001000100010000");

            funcTestMock.Setup(o => o.GetPerCycleFailures())
                .Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }); // 0.7
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.DatalogFailure(1));
            funcTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("bal", 0, 0));

            // funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.Execute())
                .Returns(false); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("Plist_1_VFDM", fakeTC, fakeTC, It.IsAny<List<string>>(), int.MaxValue, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var finalHry = "NYYUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU";

            // Mock the datalogger.
            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);

            // Needed for Vmin searches
            // strValWriterMock.Setup(o => o.SetData("Media_F3:1:1.500"));
            // strValWriterMock.Setup(o => o.SetData("1.000|1.000|1.000|1"));

            // strValWriterMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);

            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            /* ituffMock.Setup(o => o.WriteToItuff($"2_tname_HRY_RAWSTR_MBIST\n2_strgval_{finalHry}\n")); */
            strValWriterMock.Setup(o => o.SetCustomTname("HRY_RAWSTR_MBIST"));
            strValWriterMock.Setup(o => o.SetData(finalHry));

            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            var instanceToTest = new MbistVminTC
            {
                VoltageTargets = "VCCSA",
                StepSize = "0.1",
                Patlist = "Plist_1_VFDM",
                EndVoltageLimits = "1.0",
                StartVoltages = "1.0",
                TestMode = TestModes.Functional,
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                RecoveryModeDownbin = EnableStates.Enabled,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
                BisrMode = BisrModes.Compressed,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                MbistTestMode = MbistTestModes.HRY,
                ClearVariables = "all",
                CtvPins = "TDO",
                ForwardingMode = MbistVminTC.ForwardingModes.None,

                // RecoveryConfigurationFile = string.Empty,
                // RecoveryConfigurationFile = "Recovery_testcase.json",
                VFDMconfig = "colVirtFuse_vfdmtestcase.json",
                PrintToItuff = ItuffPrint.Hry,
                ForceConfigFileParseState = EnableStates.Enabled,
                PatternNameMap = "1,2,3,4,5,6,7",
                ScoreboardMaxFails = 1000,
                ScoreboardBaseNumberMbist = "9911",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());

            Assert.AreEqual("0000", this.sharedStorageValues["GLOBALNAME0"]);
            Assert.AreEqual("NYYUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", this.sharedStorageValues["HRY_RAWSTR_MBIST"]);

            strValWriterMock.VerifyAll();
            pathandleMock.VerifyAll();
            patdataMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
            ituffMock.VerifyAll();
        }

        /// <summary>Controller Failure. </summary>
        [TestMethod]
        public void RunFullTestCase_ControllerFailure()
        {
            string fakeTC = "FakeLevels";
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
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            List<IPatConfigHandle> patdata = new List<IPatConfigHandle>();
            var patdataMock = new Mock<IPatConfigHandle>();
            patdataMock.Setup(v => v.SetData("1"));
            patdata.Add(patdataMock.Object);
            var pathandleMock = new Mock<IPatConfigService>(MockBehavior.Loose);

            // pathandleMock.Setup(v => v.Apply(patdata));
            Prime.Services.PatConfigService = pathandleMock.Object;

            var plistservice = new Mock<IPlistService>(MockBehavior.Strict);

            var plistobj = new Mock<IPlistObject>(MockBehavior.Strict);
            plistobj.Setup(v => v.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            plistservice.Setup(v => v.GetPlistObject(It.IsAny<string>())).Returns(plistobj.Object);
            Prime.Services.PlistService = plistservice.Object;

            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("HRY_RAWSTR_MBIST", "UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU");
            this.sharedStorageValues.Add("SOCRecovery", "10");
            this.sharedStorageValues.Add("BISR_LEG_BISR1_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR1_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_FUSE", string.Empty);
            this.sharedStorageValues.Add("MbistVminPerMem", "-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555");

            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "VCCSA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(It.IsAny<Prime.VoltageService.IVForcePinAttribute>());
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("VCCSA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition("FakeLevels")).Returns(testCondition.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.Get("VCCSA")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            // Mock the functional test service.
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist(null)).Returns("Plist_1");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { { "TDO" } });
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("bal", 0, 0));

            // .Returns(new List<string>());
            funcTestMock.Setup(o => o.GetCtvData(It.IsAny<string>())).Returns("011000001000000000000000");

            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }); // 0.5

            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.DatalogFailure(1));

            // funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute()).Returns(false); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("Plist_1", fakeTC, fakeTC, It.IsAny<List<string>>(), int.MaxValue, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var instanceToTest = new MbistVminTC
            {
                VoltageTargets = "VCCSA",
                StepSize = "0.1",
                Patlist = "Plist_1",
                EndVoltageLimits = "0.5",
                StartVoltages = "0.5",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                RecoveryModeDownbin = EnableStates.Enabled,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
                VFDMconfig = string.Empty,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                TestMode = TestModes.Functional,
                MbistTestMode = MbistTestModes.HRY,
                ClearVariables = string.Empty,
                CtvPins = "TDO",
                AdvancedDebug = EnableStates.Enabled,

                RecoveryConfigurationFile = string.Empty,
                PrintToItuff = ItuffPrint.Disabled,
                ForceConfigFileParseState = EnableStates.Enabled,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());
            Assert.AreEqual("888UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", this.sharedStorageValues["HRY_RAWSTR_MBIST"]);

            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
        }

        /// <summary>
        /// RunScoreBoard_Tid_Pass.
        /// </summary>
        [TestMethod]
        public void RunFullTestCase_IncosistantPSFailGOID()
        {
            string fakeTC = "FakeLevels";
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
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            List<IPatConfigHandle> patdata = new List<IPatConfigHandle>();
            var patdataMock = new Mock<IPatConfigHandle>();
            patdataMock.Setup(v => v.SetData("1"));
            patdata.Add(patdataMock.Object);
            var pathandleMock = new Mock<IPatConfigService>(MockBehavior.Loose);

            // pathandleMock.Setup(v => v.Apply(patdata));
            Prime.Services.PatConfigService = pathandleMock.Object;

            var plistservice = new Mock<IPlistService>(MockBehavior.Strict);

            var plistobj = new Mock<IPlistObject>(MockBehavior.Strict);
            plistobj.Setup(v => v.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            plistservice.Setup(v => v.GetPlistObject(It.IsAny<string>())).Returns(plistobj.Object);
            Prime.Services.PlistService = plistservice.Object;

            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("HRY_RAWSTR_MBIST", "UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU");
            this.sharedStorageValues.Add("SOCRecovery", "10");
            this.sharedStorageValues.Add("BISR_LEG_BISR1_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR1_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_FUSE", string.Empty);
            this.sharedStorageValues.Add("MbistVminPerMem", "-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555");

            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "VCCSA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(It.IsAny<Prime.VoltageService.IVForcePinAttribute>());
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("VCCSA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition("FakeLevels")).Returns(testCondition.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.Get("VCCSA")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            // Mock the functional test service.
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist(null)).Returns("Plist_1");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { { "TDO" } });
            funcTestMock.Setup(p => p.SetPinMask(new List<string>()));

            // .Returns(new List<string>());
            funcTestMock.Setup(o => o.GetCtvData(It.IsAny<string>())).Returns("000000001000000000000000");

            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }); // 0.5

            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.DatalogFailure(1));
            funcTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("bal", 0, 0));

            // funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute()).Returns(false); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("Plist_1", fakeTC, fakeTC, It.IsAny<List<string>>(), int.MaxValue, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var instanceToTest = new MbistVminTC
            {
                VoltageTargets = "VCCSA",
                StepSize = "0.1",
                Patlist = "Plist_1",
                EndVoltageLimits = "0.5",
                StartVoltages = "0.5",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                RecoveryModeDownbin = EnableStates.Enabled,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                VFDMconfig = string.Empty,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                TestMode = TestModes.Functional,
                MbistTestMode = MbistTestModes.HRY,
                ClearVariables = string.Empty,
                CtvPins = "TDO",

                RecoveryConfigurationFile = string.Empty,
                PrintToItuff = ItuffPrint.Disabled,
                ForceConfigFileParseState = EnableStates.Enabled,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());
            Assert.AreEqual("666UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", this.sharedStorageValues["HRY_RAWSTR_MBIST"]);

            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
        }

        /// <summary>
        /// RunScoreBoard_Tid_Pass.
        /// </summary>
        [TestMethod]
        public void RunFullTestCase_IncosistantFsPassGOID()
        {
            string fakeTC = "FakeLevels";
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
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            List<IPatConfigHandle> patdata = new List<IPatConfigHandle>();
            var patdataMock = new Mock<IPatConfigHandle>();
            patdataMock.Setup(v => v.SetData("1"));
            patdata.Add(patdataMock.Object);
            var pathandleMock = new Mock<IPatConfigService>(MockBehavior.Loose);

            // pathandleMock.Setup(v => v.Apply(patdata));
            Prime.Services.PatConfigService = pathandleMock.Object;

            var plistservice = new Mock<IPlistService>(MockBehavior.Strict);

            var plistobj = new Mock<IPlistObject>(MockBehavior.Strict);
            plistobj.Setup(v => v.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            plistservice.Setup(v => v.GetPlistObject(It.IsAny<string>())).Returns(plistobj.Object);
            Prime.Services.PlistService = plistservice.Object;

            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("HRY_RAWSTR_MBIST", "UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU");
            this.sharedStorageValues.Add("SOCRecovery", "10");
            this.sharedStorageValues.Add("BISR_LEG_BISR1_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR1_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_FUSE", string.Empty);
            this.sharedStorageValues.Add("MbistVminPerMem", "-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555");

            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "VCCSA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(It.IsAny<Prime.VoltageService.IVForcePinAttribute>());
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("VCCSA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition("FakeLevels")).Returns(testCondition.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.Get("VCCSA")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            // Mock the functional test service.
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist(null)).Returns("Plist_1");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { { "TDO" } });
            funcTestMock.Setup(p => p.SetPinMask(new List<string>()));

            // .Returns(new List<string>());
            funcTestMock.Setup(o => o.GetCtvData(It.IsAny<string>())).Returns("001000000000000000000000");

            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }); // 0.5

            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.DatalogFailure(1));

            // funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute()).Returns(false); // 0.5
            funcTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("bal", 0, 0));

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("Plist_1", fakeTC, fakeTC, It.IsAny<List<string>>(), int.MaxValue, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var instanceToTest = new MbistVminTC
            {
                VoltageTargets = "VCCSA",
                StepSize = "0.1",
                Patlist = "Plist_1",
                EndVoltageLimits = "0.5",
                StartVoltages = "0.5",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                RecoveryModeDownbin = EnableStates.Enabled,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                VFDMconfig = string.Empty,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                TestMode = TestModes.Functional,
                MbistTestMode = MbistTestModes.HRY,
                ClearVariables = string.Empty,
                CtvPins = "TDO",

                RecoveryConfigurationFile = string.Empty,
                PrintToItuff = ItuffPrint.Disabled,
                ForceConfigFileParseState = EnableStates.Enabled,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());
            Assert.AreEqual("555UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", this.sharedStorageValues["HRY_RAWSTR_MBIST"]);

            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
        }

        /// <summary>
        /// RunScoreBoard_Tid_Pass.
        /// </summary>
        [TestMethod]
        public void RunFullTestCase_MisprogramOnlyAlgofail()
        {
            string fakeTC = "FakeLevels";
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
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            var plistservice = new Mock<IPlistService>(MockBehavior.Strict);

            var plistobj = new Mock<IPlistObject>(MockBehavior.Strict);
            plistobj.Setup(v => v.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            plistservice.Setup(v => v.GetPlistObject(It.IsAny<string>())).Returns(plistobj.Object);
            Prime.Services.PlistService = plistservice.Object;

            List<IPatConfigHandle> patdata = new List<IPatConfigHandle>();
            var patdataMock = new Mock<IPatConfigHandle>();
            patdataMock.Setup(v => v.SetData("1"));
            patdata.Add(patdataMock.Object);
            var pathandleMock = new Mock<IPatConfigService>(MockBehavior.Loose);

            // pathandleMock.Setup(v => v.Apply(patdata));
            Prime.Services.PatConfigService = pathandleMock.Object;

            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("HRY_RAWSTR_MBIST", "UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU");
            this.sharedStorageValues.Add("SOCRecovery", "10");
            this.sharedStorageValues.Add("BISR_LEG_BISR1_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR1_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_FUSE", string.Empty);
            this.sharedStorageValues.Add("MbistVminPerMem", "-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555");

            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "VCCSA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(It.IsAny<Prime.VoltageService.IVForcePinAttribute>());
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("VCCSA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition("FakeLevels")).Returns(testCondition.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.Get("VCCSA")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            // Mock the functional test service.
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist(null)).Returns("Plist_1");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { { "TDO" } });
            funcTestMock.Setup(p => p.SetPinMask(new List<string>()));

            // .Returns(new List<string>());
            funcTestMock.Setup(o => o.GetCtvData(It.IsAny<string>())).Returns("000010000000000000000000");

            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }); // 0.5

            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.DatalogFailure(1));
            funcTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("bal", 0, 0));

            // funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute()).Returns(false); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("Plist_1", fakeTC, fakeTC, It.IsAny<List<string>>(), int.MaxValue, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var instanceToTest = new MbistVminTC
            {
                VoltageTargets = "VCCSA",
                StepSize = "0.1",
                Patlist = "Plist_1",
                EndVoltageLimits = "0.5",
                StartVoltages = "0.5",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                RecoveryModeDownbin = EnableStates.Enabled,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                VFDMconfig = string.Empty,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                TestMode = TestModes.Functional,
                MbistTestMode = MbistTestModes.HRY,
                ClearVariables = string.Empty,
                CtvPins = "TDO",

                RecoveryConfigurationFile = string.Empty,
                PrintToItuff = ItuffPrint.Disabled,
                ForceConfigFileParseState = EnableStates.Enabled,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());
            Assert.AreEqual("MMMUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", this.sharedStorageValues["HRY_RAWSTR_MBIST"]);

            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
        }

        /// <summary>
        /// RunScoreBoard_Tid_Pass.
        /// </summary>
        [TestMethod]
        public void RunFullTestCase_MisprogramDoneFailandAlgo()
        {
            string fakeTC = "FakeLevels";
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
                else
                {
                    return null;
                }
            });
            Prime.Services.FileService = fileservice.Object;

            List<IPatConfigHandle> patdata = new List<IPatConfigHandle>();
            var patdataMock = new Mock<IPatConfigHandle>();
            patdataMock.Setup(v => v.SetData("1"));
            patdata.Add(patdataMock.Object);
            var pathandleMock = new Mock<IPatConfigService>(MockBehavior.Loose);

            // pathandleMock.Setup(v => v.Apply(patdata));
            Prime.Services.PatConfigService = pathandleMock.Object;

            var plistservice = new Mock<IPlistService>(MockBehavior.Strict);

            var plistobj = new Mock<IPlistObject>(MockBehavior.Strict);
            plistobj.Setup(v => v.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            plistservice.Setup(v => v.GetPlistObject(It.IsAny<string>())).Returns(plistobj.Object);
            Prime.Services.PlistService = plistservice.Object;

            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("HRY_RAWSTR_MBIST", "UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU");
            this.sharedStorageValues.Add("SOCRecovery", "10");
            this.sharedStorageValues.Add("BISR_LEG_BISR1_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR1_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR2_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_LEG_BISR3_FUSE", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_RAW", string.Empty);
            this.sharedStorageValues.Add("BISR_BP0_FUSE", string.Empty);
            this.sharedStorageValues.Add("MbistVminPerMem", "-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555,-5555");

            var voltageServiceMock = new Mock<Prime.VoltageService.IVoltageService>(MockBehavior.Strict);
            voltageServiceMock.Setup(v => v.CreateVForceForPinAttribute(new List<string> { "VCCSA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(It.IsAny<Prime.VoltageService.IVForcePinAttribute>());
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(t => t.GetPinAttributeValue("VCCSA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition("FakeLevels")).Returns(testCondition.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(p => p.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.Get("VCCSA")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetVectorAddress()).Returns(10);

            // Mock the functional test service.
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ResolvePlist(null)).Returns("Plist_1");
            funcTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { { "TDO" } });
            funcTestMock.Setup(p => p.SetPinMask(new List<string>()));

            // .Returns(new List<string>());
            funcTestMock.Setup(o => o.GetCtvData(It.IsAny<string>())).Returns("000110000000000000000000");

            funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<Prime.FunctionalService.IFailureData> { failDataMock.Object }); // 0.5

            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.Reset());
            funcTestMock.Setup(o => o.DatalogFailure(1));
            funcTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("bal", 0, 0));

            // funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.SetupSequence(o => o.Execute()).Returns(false); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest("Plist_1", fakeTC, fakeTC, It.IsAny<List<string>>(), int.MaxValue, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var instanceToTest = new MbistVminTC
            {
                VoltageTargets = "VCCSA",
                StepSize = "0.1",
                Patlist = "Plist_1",
                EndVoltageLimits = "0.5",
                StartVoltages = "0.5",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                RecoveryModeDownbin = EnableStates.Enabled,
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                VFDMconfig = string.Empty,
                FeatureSwitchSettings = "fivr_mode_off",
                FivrCondition = string.Empty,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                TestMode = TestModes.Functional,
                MbistTestMode = MbistTestModes.HRY,
                ClearVariables = string.Empty,
                CtvPins = "TDO",

                RecoveryConfigurationFile = string.Empty,
                PrintToItuff = ItuffPrint.Disabled,
                ForceConfigFileParseState = EnableStates.Enabled,
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());
            Assert.AreEqual("MMMUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", this.sharedStorageValues["HRY_RAWSTR_MBIST"]);

            testConditionServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
        }
    }
}
