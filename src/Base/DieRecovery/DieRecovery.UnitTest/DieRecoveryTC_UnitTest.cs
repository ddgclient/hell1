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

namespace DDG.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;
    using DieRecoveryBase;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.FileService;
    using Prime.SharedStorageService;
    using Prime.TestMethods;

    /// <summary>
    /// Defines the <see cref="DieRecoveryTC_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DieRecoveryTC_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DieRecoveryTC_UnitTest"/> class.
        /// </summary>
        public DieRecoveryTC_UnitTest()
        {
            this.SharedStorageValues = new Dictionary<string, string>();

            // Mock for getting files.
            this.FileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            this.FileServiceMock.Setup(o => o.GetFile("./Modules/YBS_UPSS/InputFiles/Recovery.xml")).Returns(GetPathToFiles() + "Recovery.xml");
            this.FileServiceMock.Setup(o => o.GetFile("./PrimeConfigs/DieRecoveryTrackers.json")).Returns(GetPathToFiles() + "DieRecoveryTrackers.json");
            this.FileServiceMock.Setup(o => o.FileExists("./Modules/YBS_UPSS/InputFiles/Recovery.xml")).Returns(true);
            this.FileServiceMock.Setup(o => o.FileExists("./PrimeConfigs/DieRecoveryTrackers.json")).Returns(true);
            Prime.Services.FileService = this.FileServiceMock.Object;

            // Default Mock for Shared service.
            this.SharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>()))
                .Callback((string key, object obj, Context context) =>
                {
                    this.SharedStorageValues[context + "|" + key] = JsonConvert.SerializeObject(obj);
                });
            this.SharedStorageMock.Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>()))
                .Returns((string key, Type obj, Context context) => JsonConvert.DeserializeObject(this.SharedStorageValues[context + "|" + key], obj));
            this.SharedStorageMock.Setup(o => o.KeyExistsInObjectTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Returns((string key, Context context) => this.SharedStorageValues.ContainsKey(context + "|" + key));

            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, string obj, Context context) =>
                {
                    this.SharedStorageValues[context + "|" + key] = obj;
                });
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Returns((string key, Context context) => this.SharedStorageValues[context + "|" + key]);
            this.SharedStorageMock.Setup(o => o.KeyExistsInStringTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Returns((string key, Context context) => this.SharedStorageValues.ContainsKey(context + "|" + key));

            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Context>()))
                .Callback((string key, int obj, Context context) =>
                {
                    this.SharedStorageValues[context + "|" + key] = obj.ToString();
                });
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Returns((string key, Context context) => int.Parse(this.SharedStorageValues[context + "|" + key]));
            this.SharedStorageMock.Setup(o => o.KeyExistsInIntegerTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Returns((string key, Context context) => this.SharedStorageValues.ContainsKey(context + "|" + key));

            // update the reset policies
            this.SharedStorageMock.Setup(o => o.OverrideIntegerRowResetPolicy(DieRecovery.Globals.DieRecoveryTrackerDownBinsAllowed, ResetPolicy.NEVER_RESET, Context.DUT));
            this.SharedStorageMock.Setup(o => o.OverrideStringRowResetPolicy(It.IsAny<string>(), ResetPolicy.NEVER_RESET, Context.DUT));
            this.SharedStorageMock.Setup(o => o.OverrideObjectRowResetPolicy(It.IsAny<string>(), ResetPolicy.NEVER_RESET, Context.DUT));

            Prime.Services.SharedStorageService = this.SharedStorageMock.Object;
        }

        private Dictionary<string, string> SharedStorageValues { get; set; }

        private Mock<ISharedStorageService> SharedStorageMock { get; set; }

        private Mock<IFileService> FileServiceMock { get; set; }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileLoadException))]
        public void Verify_ParamEmpty_False()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback((string s, int line, string s2, string s3) =>
            {
                Console.WriteLine(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            DieRecoveryBase underTest = new DieRecoveryBase { Mode = DieRecoveryBase.ExecuteMode.Configure };

            // [2] Call the method under test.
            underTest.RulesFile = string.Empty;
            underTest.TrackerFile = "./ValidPath";
            underTest.Verify();

            underTest.RulesFile = "./ValidPath";
            underTest.TrackerFile = string.Empty;
            underTest.Verify();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamNoEmpty_True()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            DieRecoveryBase underTest = new DieRecoveryBase { Mode = DieRecoveryBase.ExecuteMode.DumpTables };

            // [2] Call the method under test.
            underTest.Verify();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(DieRecoveryBase.ExecuteMode.DumpTables, underTest.Mode);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_Configure_True()
        {
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;

            DieRecoveryBase underTest = new DieRecoveryBase { Mode = DieRecoveryBase.ExecuteMode.Configure };

            underTest.Verify();

            Assert.AreEqual(1, underTest.Execute());

            // TODO: Add more checking to make sure the correct data was saved
            Assert.AreEqual("1", this.SharedStorageValues["DUT|__DDG_DieRecoveryGlobals__!DownBinsAllowed"]);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_Configure_BadRecoveryXml_Fail()
        {
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;

            this.FileServiceMock.Setup(o => o.GetFile("BrokenRulesFile.xml")).Returns(GetPathToFiles() + "RecoveryFailBadRule.xml");
            this.FileServiceMock.Setup(o => o.FileExists("BrokenRulesFile.xml")).Returns(true);

            DieRecoveryBase underTest = new DieRecoveryBase
            {
                Mode = DieRecoveryBase.ExecuteMode.Configure,
                RulesFile = "BrokenRulesFile.xml",
            };

            underTest.Verify();

            Assert.AreEqual(0, underTest.Execute());
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_Configure_FailsLinkNotExist()
        {
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;
            this.FileServiceMock.Setup(o => o.GetFile("TestLinkFail.json")).Returns(GetPathToFiles() + "TestLinkFail.json");
            this.FileServiceMock.Setup(o => o.FileExists("TestLinkFail.json")).Returns(true);

            DieRecoveryBase underTest = new DieRecoveryBase
            {
                Mode = DieRecoveryBase.ExecuteMode.Configure,
                TrackerFile = "TestLinkFail.json",
            };

            underTest.Verify();

            Assert.AreEqual(0, underTest.Execute());
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_Configure_FailsLinkWrongFormat()
        {
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;
            this.FileServiceMock.Setup(o => o.GetFile("TestLinkFailFormat.json")).Returns(GetPathToFiles() + "TestLinkFailFormat.json");
            this.FileServiceMock.Setup(o => o.FileExists("TestLinkFailFormat.json")).Returns(true);

            DieRecoveryBase underTest = new DieRecoveryBase
            {
                Mode = DieRecoveryBase.ExecuteMode.Configure,
                TrackerFile = "TestLinkFailFormat.json",
            };

            underTest.Verify();

            Assert.AreEqual(0, underTest.Execute());
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_Configure_AllowDownBinsFalse()
        {
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;
            DieRecoveryBase underTest = new DieRecoveryBase
            {
                Mode = DieRecoveryBase.ExecuteMode.Configure,
                AllowDownBins = DieRecoveryBase.MyBool.False,
            };

            underTest.Verify();

            Assert.AreEqual(1, underTest.Execute());
            Assert.AreEqual("0", this.SharedStorageValues["DUT|__DDG_DieRecoveryGlobals__!DownBinsAllowed"]);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_DumpTables_True()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                Console.WriteLine(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // run the init test
            DieRecoveryBase setupTest = new DieRecoveryBase { Mode = DieRecoveryBase.ExecuteMode.Configure, LogLevel = TestMethodBase.PrimeLogLevel.DISABLED };
            setupTest.Verify();
            Assert.AreEqual(1, setupTest.Execute(), "Failed Execute for Configure.");

            // create the pinmaps
            DDG.DieRecovery.Utilities.StorePinMapDecoder((PinToSliceIndexDecoder)JsonConvert.DeserializeObject("{'Name':'CORE0_NOA', 'PatternModify':'CORE_DISABLE0', 'Size':1, 'PinToSliceIndexMap':{'NOAB_00':[0]}}", typeof(PinToSliceIndexDecoder)));
            DDG.DieRecovery.Utilities.StorePinMapDecoder((PinToSliceIndexDecoder)JsonConvert.DeserializeObject("{'Name':'CORE1_NOA', 'PatternModify':'CORE_DISABLE1', 'Size':1, 'PinToSliceIndexMap':{'NOAB_01':[0]}}", typeof(PinToSliceIndexDecoder)));

            // save some data
            DDG.DieRecovery.Utilities.StoreTrackerData("CORE0", "0");
            DDG.DieRecovery.Utilities.StoreTrackerData("CORE1", "0");
            DDG.DieRecovery.Utilities.StoreTrackerData("CORE2", "0");
            DDG.DieRecovery.Utilities.StoreTrackerData("CORE3", "0");
            DDG.DieRecovery.Utilities.StoreTrackerData("CORE4", "1");
            DDG.DieRecovery.Utilities.StoreTrackerData("CORE5", "1");
            DDG.DieRecovery.Utilities.StoreTrackerData("CORE6", "1");
            DDG.DieRecovery.Utilities.StoreTrackerData("CORE7", "1");

            // Run the instance to dump all the data.
            DieRecoveryBase underTest = new DieRecoveryBase
            {
                Mode = DieRecoveryBase.ExecuteMode.DumpTables,
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
                InstanceName = "DummyInstance",
            };
            underTest.Verify();

            Assert.AreEqual(1, underTest.Execute(), "Failed Execute for DumpTables.");

            // Check that the correct data was printed out.
            consoleServiceMock.Verify(o => o.PrintDebug($"DieRecovery Tracker=[CORE0] Data=[0]"), Times.Once);
            consoleServiceMock.Verify(o => o.PrintDebug($"DieRecovery Tracker=[CORE1] Data=[0]"), Times.Once);
            consoleServiceMock.Verify(o => o.PrintDebug($"DieRecovery Tracker=[CORE2] Data=[0]"), Times.Once);
            consoleServiceMock.Verify(o => o.PrintDebug($"DieRecovery Tracker=[CORE3] Data=[0]"), Times.Once);
            consoleServiceMock.Verify(o => o.PrintDebug($"DieRecovery Tracker=[CORE4] Data=[1]"), Times.Once);
            consoleServiceMock.Verify(o => o.PrintDebug($"DieRecovery Tracker=[CORE5] Data=[1]"), Times.Once);
            consoleServiceMock.Verify(o => o.PrintDebug($"DieRecovery Tracker=[CORE6] Data=[1]"), Times.Once);
            consoleServiceMock.Verify(o => o.PrintDebug($"DieRecovery Tracker=[CORE7] Data=[1]"), Times.Once);
            consoleServiceMock.Verify(o => o.PrintDebug($"DieRecovery Tracker=[CCF] Data=[None]"), Times.Once);
            consoleServiceMock.Verify(o => o.PrintDebug($"DieRecovery Tracker=[GT] Data=[None]"), Times.Once);
        }

        private static string GetPathToFiles([CallerFilePath] string srcPath = "")
        {
            return Path.GetDirectoryName(srcPath) + "\\InputFiles\\";
        }
    }
}
