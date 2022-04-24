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
namespace BisrRepairCheckTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.PinService;
    using Prime.PlistService;
    using Prime.SharedStorageService;
    using Prime.TestConditionService;
    using Prime.TestMethods.Functional;
    using Prime.VoltageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class BisrRepairCheckTC_UnitTest : BisrRepairCheckTC
    {
        private Mock<ISharedStorageService> sharedStorageMock;
        private Dictionary<string, string> sharedStorageValues;

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json";
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;

            // this.PatternModifications = string.Empty;
            this.Patlist = "Plist_BISR_1";
            this.LevelsTc = "SomeLevels";
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

            // DDG.VoltageConverter.Service = voltageConverterFactoryMock.Object;
            // Default Mock for Console Service
            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => { System.Console.WriteLine(s); });
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
            Callback<string, int, string, string>((string msg, int line, string n, string src) => { System.Console.WriteLine($"ERROR: {msg}"); });
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;
        }

        /// <summary>
        /// No fuse check.
        /// </summary>
        [TestMethod]
        public void Nofusing_check()
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

            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("BISR_BP0_RAW", "000000000000000000000000");

            var testCondition = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            testCondition.Setup(t => t.Apply(new List<double>() { 0.5 }));
            testCondition.Setup(t => t.Restore());
            testCondition.Setup(t => t.Reset());

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);

            var funcTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.SetPinMask(It.IsAny<List<string>>()));
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.GetCtvData())
                .Returns(new Dictionary<string, string>() { { "TDO", "000000000000000000000000" } });
            funcTestMock.Setup(o => o.Execute()).Returns(true); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureCtvPerPinTest("BISR_1", fakeTC, fakeTC, new List<string>() { "TDO" }, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var instanceToTest = new BisrRepairCheckTC
            {
                Patlist = "BISR_1",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                CtvCapturePins = "TDO",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(3, instanceToTest.Execute());

            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
        }

        /// <summary>
        /// No fuse check.
        /// </summary>
        [TestMethod]
        public void Refusing_not_allowed()
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

            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("BISR_BP0_RAW", "010000000000000000000000");

            var testCondition = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            testCondition.Setup(t => t.Apply(new List<double>() { 0.5 }));
            testCondition.Setup(t => t.Restore());
            testCondition.Setup(t => t.Reset());

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);

            var funcTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.SetPinMask(It.IsAny<List<string>>()));
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.GetCtvData())
                .Returns(new Dictionary<string, string>() { { "TDO", "000000000001000000000000" } });
            funcTestMock.Setup(o => o.Execute()).Returns(true); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureCtvPerPinTest("BISR_1", fakeTC, fakeTC, new List<string>() { "TDO" }, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var instanceToTest = new BisrRepairCheckTC
            {
                Patlist = "BISR_1",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                CtvCapturePins = "TDO",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(0, instanceToTest.Execute());

            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
        }

        /// <summary>
        /// No fuse check.
        /// </summary>
        [TestMethod]
        public void Refusing()
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

            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("BISR_BP0_RAW", "010000000000000000000000");

            var testCondition = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            testCondition.Setup(t => t.Apply(new List<double>() { 0.5 }));
            testCondition.Setup(t => t.Restore());
            testCondition.Setup(t => t.Reset());

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);

            var funcTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.SetPinMask(It.IsAny<List<string>>()));
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.GetCtvData())
                .Returns(new Dictionary<string, string>() { { "TDO", "010000000001000000000000" } });
            funcTestMock.Setup(o => o.Execute()).Returns(true); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureCtvPerPinTest("BISR_1", fakeTC, fakeTC, new List<string>() { "TDO" }, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var instanceToTest = new BisrRepairCheckTC
            {
                Patlist = "BISR_1",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                AllowRefuse = MbistVminTC.MbistVminTC.EnableStates.Enabled,
                CtvCapturePins = "TDO",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(1, instanceToTest.Execute());

            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
        }

        /// <summary>
        /// No fuse check.
        /// </summary>
        [TestMethod]
        public void Fusing_no_priorfuse()
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

            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("BISR_BP0_RAW", "100000000000000000000000");

            var testCondition = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            testCondition.Setup(t => t.Apply(new List<double>() { 0.5 }));
            testCondition.Setup(t => t.Restore());
            testCondition.Setup(t => t.Reset());

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);

            var funcTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.SetPinMask(It.IsAny<List<string>>()));
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.GetCtvData())
                .Returns(new Dictionary<string, string>() { { "TDO", "000000000000000000000000" } });
            funcTestMock.Setup(o => o.Execute()).Returns(true); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureCtvPerPinTest("BISR_1", fakeTC, fakeTC, new List<string>() { "TDO" }, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var instanceToTest = new BisrRepairCheckTC
            {
                Patlist = "BISR_1",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                CtvCapturePins = "TDO",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(4, instanceToTest.Execute());

            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
        }

        /// <summary>
        /// No fuse check.
        /// </summary>
        [TestMethod]
        public void Nofusing_has_occured()
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

            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("BISR_BP0_RAW", "000000000000000000000000");

            var testCondition = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            testCondition.Setup(t => t.Apply(new List<double>() { 0.5 }));
            testCondition.Setup(t => t.Restore());
            testCondition.Setup(t => t.Reset());

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);

            var funcTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.SetPinMask(It.IsAny<List<string>>()));
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.GetCtvData())
                .Returns(new Dictionary<string, string>() { { "TDO", "000000000000000000000000" } });
            funcTestMock.Setup(o => o.Execute()).Returns(true); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureCtvPerPinTest("BISR_1", fakeTC, fakeTC, new List<string>() { "TDO" }, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var instanceToTest = new BisrRepairCheckTC
            {
                Patlist = "BISR_1",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                CtvCapturePins = "TDO",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(3, instanceToTest.Execute());

            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
        }

        /// <summary>
        /// No fuse check.
        /// </summary>
        [TestMethod]
        public void Bisr_prepostmatch()
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

            // Initialize shared storage values (if clearvariables is not enabled).
            this.sharedStorageValues.Add("BISR_BP0_RAW", "001000000000000000000000");

            var testCondition = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            testCondition.Setup(t => t.Apply(new List<double>() { 0.5 }));
            testCondition.Setup(t => t.Restore());
            testCondition.Setup(t => t.Reset());

            var failDataMock = new Mock<Prime.FunctionalService.IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FailingPatternName");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);

            var funcTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.SetPinMask(It.IsAny<List<string>>()));
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.GetCtvData())
                .Returns(new Dictionary<string, string>() { { "TDO", "001000000000000000000000" } });
            funcTestMock.Setup(o => o.Execute()).Returns(true); // 0.5

            var funcTestServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Strict);
            funcTestServiceMock.Setup(o => o.CreateCaptureCtvPerPinTest("BISR_1", fakeTC, fakeTC, new List<string>() { "TDO" }, It.IsAny<string>())).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcTestServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);

            var instanceToTest = new BisrRepairCheckTC
            {
                Patlist = "BISR_1",
                LookupTableConfigurationFile = "Exampleconfig_WW40_v2_PerPattern.json",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
                TimingsTc = fakeTC,
                LevelsTc = fakeTC,
                MappingConfig = "SharedStortoDFFMap.json",
                CtvCapturePins = "TDO",
            };

            instanceToTest.TestMethodExtension = instanceToTest;

            instanceToTest.Verify();
            instanceToTest.CustomVerify(); // need to call this manually, not sure why.
            Assert.AreEqual(2, instanceToTest.Execute());

            funcTestMock.Verify(o => o.Execute(), Times.Exactly(1));
        }
    }
}