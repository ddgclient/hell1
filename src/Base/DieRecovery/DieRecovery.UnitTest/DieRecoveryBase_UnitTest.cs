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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using DieRecoveryBase;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.PerformanceService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;
    using Prime.TpSettingsService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DieRecoveryBase_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DieRecoveryBase_UnitTest"/> class.
        /// </summary>
        public DieRecoveryBase_UnitTest()
        {
            this.SharedStorageValues = new Dictionary<string, string>();

            // Mock the test instance name
            var tpMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpMock.Setup(o => o.GetCurrentTestInstanceName()).Returns(this.TestInstanceName);
            Prime.Services.TestProgramService = tpMock.Object;

            // Default Mock for console service.
            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => Console.WriteLine(s));
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;

            // Default mock for TPSettings (enable Midas)
            var tpSettingsMock = new Mock<ITpSettingsService>(MockBehavior.Strict);
            tpSettingsMock.Setup(t => t.IsTpFeatureEnabled(Feature.Midas)).Returns(true);
            Prime.Services.TpSettingsService = tpSettingsMock.Object;

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

            this.SharedStorageMock.Setup(o => o.KeyExistsInIntegerTable(DieRecovery.Globals.DieRecoveryTrackerDownBinsAllowed, DieRecovery.Globals.DieRecoveryTrackerGlobalContext)).Returns(false);

            // update the reset policies
            this.SharedStorageMock.Setup(o => o.OverrideIntegerRowResetPolicy(DieRecovery.Globals.DieRecoveryTrackerDownBinsAllowed, ResetPolicy.NEVER_RESET, DieRecovery.Globals.DieRecoveryTrackerGlobalContext));
            this.SharedStorageMock.Setup(o => o.OverrideStringRowResetPolicy(It.IsAny<string>(), ResetPolicy.NEVER_RESET, DieRecovery.Globals.DieRecoveryTrackerGlobalContext));
            this.SharedStorageMock.Setup(o => o.OverrideObjectRowResetPolicy(It.IsAny<string>(), ResetPolicy.NEVER_RESET, DieRecovery.Globals.DieRecoveryTrackerGlobalContext));

            Prime.Services.SharedStorageService = this.SharedStorageMock.Object;

            // build some dummy pinmaps
            var decoder = (PinToSliceIndexDecoder)JsonConvert.DeserializeObject("{'Name':'NOA_MAP', 'PatternModify':'CORE_DISABLE_ALL', 'Size':8, 'PinToSliceIndexMap':{'NOA0':[0],'NOA1':[1],'NOA2':[2],'NOA0':[3],'NOA4':[4],'NOA5':[5],'NOA6':[6],'NOA7':[7],}}", typeof(PinToSliceIndexDecoder));
            DDG.DieRecovery.Utilities.StorePinMapDecoder(decoder);

            // build some dummy trackers
            Tracker tracker = JsonConvert.DeserializeObject<Tracker>("{'Name':'SliceTracking', 'Size':8}");
            DDG.DieRecovery.Utilities.StoreTrackerDefinition(tracker);

            // build some dummy rules
            BitArray fourCore = new BitArray(new bool[8] { false, false, false, false, true, true, true, true });
            BitArray twoCoreLower = new BitArray(new bool[8] { false, false, true, true, true, true, true, true });
            BitArray twoCoreUpper = new BitArray(new bool[8] { true, true, false, false, true, true, true, true });

            DefeatureRule coreDefeatureRule = new DefeatureRule("CoreDefeaturingVector", new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 });
            coreDefeatureRule.Add(DefeatureRule.RuleMode.ValidCombinations, "4C", 4, DefeatureRule.RuleType.FullyFeatured, new List<BitArray>() { fourCore });
            coreDefeatureRule.Add(DefeatureRule.RuleMode.ValidCombinations, "2C", 2, DefeatureRule.RuleType.Recovery, new List<BitArray>() { twoCoreLower, twoCoreUpper });
            DDG.DieRecovery.Utilities.StoreRule(coreDefeatureRule);

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        private string TestInstanceName { get; set; } = "FakeModule::FakeTest";

        private Dictionary<string, string> SharedStorageValues { get; set; }

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private Mock<ISharedStorageService> SharedStorageMock { get; set; }

        /// <summary>
        /// Test the case where the Tracker doesn't exist.
        /// </summary>
        [TestMethod]
        public void CreateTracker_Fail()
        {
            Assert.ThrowsException<KeyNotFoundException>(() => new DieRecoveryTracker("NotARealTracker"));
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void GetMaskBits_FromTracker_Pass()
        {
            var datalog1 = this.GenerateTrackerDataLog("SliceTracking", "00000000", "00100001", "00000000", "00100001");
            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.WriteToItuff(datalog1.Object));
            datalogMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(datalog1.Object);
            Prime.Services.DatalogService = datalogMock.Object;

            var recovery = new DieRecoveryTracker("SliceTracking");
            var expected = new BitArray(new bool[] { false, false, true, false, false, false, false, true });
            recovery.UpdateTrackingStructure(new BitArray(new bool[8] { false, false, true, false, false, false, false, true }));
            datalogMock.Verify(o => o.WriteToItuff(datalog1.Object), Times.Once);

            var actual = recovery.GetMaskBits();
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void GetMaskBits_Pass()
        {
            var recovery = new DieRecoveryTracker(new List<string> { "SliceTracking" });
            Assert.ThrowsException<ArgumentException>(() => recovery.GetMaskBits(InputType.SharedStorage, "SharedStorageTokenWrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => recovery.GetMaskBits(InputType.SharedStorage, "A.B.SharedStorageTokenWrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => recovery.GetMaskBits(InputType.SharedStorage, "B.SharedStorageTokenWrongFormat"));
            Assert.ThrowsException<NotImplementedException>(() => recovery.GetMaskBits(InputType.UserVar, "collection.uservar"));

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.GetStringRowFromTable("GsdsStringToken", Context.DUT)).Returns("10101010");
            sharedStorageMock.Setup(o => o.GetStringRowFromTable("SharedStorageToken", Context.DUT)).Returns("11111");
            sharedStorageMock.Setup(o => o.GetStringRowFromTable("SharedStorageToken", Context.IP)).Returns("0000");
            sharedStorageMock.Setup(o => o.GetStringRowFromTable("SharedStorageToken", Context.LOT)).Returns("11111100111");
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            Assert.AreEqual("00001111", recovery.GetMaskBits(InputType.Literal, "00001111").ToBinaryString());
            Assert.AreEqual("10101010", recovery.GetMaskBits(InputType.Gsds, "G.U.S.GsdsStringToken").ToBinaryString());
            Assert.AreEqual("11111", recovery.GetMaskBits(InputType.SharedStorage, "DUT.SharedStorageToken").ToBinaryString());
            Assert.AreEqual("0000", recovery.GetMaskBits(InputType.SharedStorage, "IP.SharedStorageToken").ToBinaryString());
            Assert.AreEqual("11111100111", recovery.GetMaskBits(InputType.SharedStorage, "lot.SharedStorageToken").ToBinaryString());
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void CloneTracker_Pass()
        {
            var recovery = new DieRecoveryTracker("SliceTracking");
            DDG.DieRecovery.Service.CloneTracker("SliceTracking", "NewSliceTracking");
            var newTracker = DDG.DieRecovery.Service.Get("NewSliceTracking");
            newTracker.UpdateTrackingStructure("11111000".ToBitArray(), null, null, UpdateMode.Merge, false);
            Assert.AreEqual("11111000", newTracker.GetMaskBits().ToBinaryString());
            DDG.DieRecovery.Service.CloneTracker("NewSliceTracking", "NewSliceTracking");
            Assert.AreEqual("11111000", newTracker.GetMaskBits().ToBinaryString());
            DDG.DieRecovery.Service.CloneTracker("SliceTracking", "NewSliceTracking");
            Assert.IsTrue(newTracker.GetMaskBits().Count == 0);
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void UpdateTrackingStructure_FromBitArrayMergeNoMask_Pass()
        {
            var datalog1 = this.GenerateTrackerDataLog("SliceTracking", "00000000", "11111000", "00000000", "11111000");
            var datalog2 = this.GenerateTrackerDataLog("SliceTracking", "00000000", "00001111", "11111000", "11111111");
            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.WriteToItuff(datalog1.Object));
            datalogMock.Setup(o => o.WriteToItuff(datalog2.Object));
            datalogMock.SetupSequence(o => o.GetItuffStrgvalWriter())
                .Returns(datalog1.Object)
                .Returns(datalog2.Object);
            Prime.Services.DatalogService = datalogMock.Object;

            // var recovery = new DieRecoveryTracker("SliceTracking");
            var recovery = DDG.DieRecovery.Service.Get("SliceTracking");
            var newValue = new BitArray(new bool[] { true, true, true, true, true, false, false, false });
            recovery.UpdateTrackingStructure(newValue, mode: UpdateMode.Merge);
            CollectionAssert.AreEqual(newValue, recovery.GetMaskBits());

            var value2 = new BitArray(new bool[] { false, false, false, false, true, true, true, true });
            recovery.UpdateTrackingStructure(value2, mode: UpdateMode.Merge);
            CollectionAssert.AreEqual(new BitArray(8, true), recovery.GetMaskBits());

            datalogMock.Verify(o => o.WriteToItuff(datalog1.Object), Times.Once);
            datalogMock.Verify(o => o.WriteToItuff(datalog2.Object), Times.Once);
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void UpdateTrackingStructure_FromLiteralOverwriteNoMask_Pass()
        {
            var datalog1 = this.GenerateTrackerDataLog("SliceTracking", "00000000", "11111000", "00000000", "11111000");
            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.WriteToItuff(datalog1.Object));
            datalogMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(datalog1.Object);
            Prime.Services.DatalogService = datalogMock.Object;

            var recovery = new DieRecoveryTracker("SliceTracking");
            recovery.UpdateTrackingStructure(InputType.Literal, "11111000", mode: UpdateMode.OverWrite);
            CollectionAssert.AreEqual(new BitArray(new bool[] { true, true, true, true, true, false, false, false }), recovery.GetMaskBits());
            datalogMock.Verify(o => o.WriteToItuff(datalog1.Object), Times.Once);
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void UpdateTrackingStructure_NoPrint_Pass()
        {
            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            Prime.Services.DatalogService = datalogMock.Object; // need to clear the datalog service or the other tests mocks might make it pass.

            var recovery = new DieRecoveryTracker("SliceTracking");
            recovery.UpdateTrackingStructure(InputType.Literal, "11111000", mode: UpdateMode.OverWrite, log: false);
            CollectionAssert.AreEqual(new BitArray(new bool[] { true, true, true, true, true, false, false, false }), recovery.GetMaskBits());
            datalogMock.VerifyAll();
        }

        /* NO LONGER POSSIBLE.
        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void UpdateTrackingStructure_TruncateName_Pass()
        {
            var testName = "MyModule::ReallyLoooooooongTestName__100Characters_________________________________________________x";
            var trackerName = "ReallyLongTrackerName__100Characters_______________________________________________________________x";
            var datalogTname = testName + "::" + trackerName + "_1";
            var datalotTnameTruncated = datalogTname.Substring(0, 125);

            var trackerValueInitial = "00000000";
            var trackerValueFromTest = "11110000";
            var trackerValueFinal = "11110000";

            var datalogText = $"0_tname_{datalotTnameTruncated}\n";
            datalogText += $"0_strgval_TestResult:b{trackerValueFromTest}|Incoming:b{trackerValueInitial}|Outgoing:b{trackerValueFinal}\n";
            Console.WriteLine($"Expecting Datalog\n{datalogText}\n");

            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.WriteToItuff(datalogText));
            Prime.Services.DatalogService = datalogMock.Object;

            var tpMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            tpMock.Setup(o => o.GetCurrentTestInstanceName()).Returns(testName);
            Prime.Services.TestProgramService = tpMock.Object;

            // build some a trackers
            Tracker tracker = JsonConvert.DeserializeObject<Tracker>($"{{'Name':'{trackerName}', 'Size':8}}");
            DDG.DieRecovery.Utilities.StoreTrackerDefinition(tracker);

            var recovery = new DieRecoveryTracker(trackerName);
            recovery.UpdateTrackingStructure(InputType.Literal, trackerValueFromTest, mode: UpdateMode.Merge);
            Assert.AreEqual(trackerValueFinal, recovery.GetMaskBits().ToBinaryString());
            datalogMock.Verify(o => o.WriteToItuff(datalogText), Times.Once);
        } */

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void UpdateTrackingStructure_FromVminMergeNoMask_Pass()
        {
            var datalog1 = this.GenerateTrackerDataLog("SliceTracking", "00000000", "00011111", "00000000", "00011111");
            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.WriteToItuff(datalog1.Object));
            datalogMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(datalog1.Object);
            Prime.Services.DatalogService = datalogMock.Object;

            var recovery = new DieRecoveryTracker("SliceTracking");
            var vmins = new List<double>() { 1.0, 0.95, 0.85, 1.2, -9999, -9999, -9999, -9999 };
            recovery.UpdateTrackingStructure(vmins, mode: UpdateMode.Merge, vminLimitHigh: 1.1);
            CollectionAssert.AreEqual(new BitArray(new bool[] { false, false, false, true, true, true, true, true }), recovery.GetMaskBits());
            datalogMock.Verify(o => o.WriteToItuff(datalog1.Object), Times.Once);
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void UpdateTrackingStructure_MultipleTimes_Pass()
        {
            // don't care about datalogging for this test.
            var datalogWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Loose);
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Loose);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(datalogWriterMock.Object);
            Prime.Services.DatalogService = datalogServiceMock.Object;

            this.SharedStorageMock.Setup(o => o.KeyExistsInIntegerTable("__DDG_DieRecoveryGlobals__!DownBinsAllowed", DieRecovery.Globals.DieRecoveryTrackerGlobalContext)).Returns(true);
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("__DDG_DieRecoveryGlobals__!DownBinsAllowed", DieRecovery.Globals.DieRecoveryTrackerGlobalContext)).Returns(1);

            var recovery = new DieRecoveryTracker("SliceTracking");
            Assert.AreEqual(true, recovery.UpdateTrackingStructure(InputType.Literal, "11111000", mode: UpdateMode.OverWrite));
            Assert.AreEqual(true, recovery.UpdateTrackingStructure(InputType.Literal, "11111111", mode: UpdateMode.OverWrite));
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void UpdateTrackingStructure_MultipleTimes_Fail()
        {
            // don't care about datalogging for this test.
            var datalogWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Loose);
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Loose);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(datalogWriterMock.Object);
            Prime.Services.DatalogService = datalogServiceMock.Object;

            this.SharedStorageMock.Setup(o => o.KeyExistsInIntegerTable("__DDG_DieRecoveryGlobals__!DownBinsAllowed", DieRecovery.Globals.DieRecoveryTrackerGlobalContext)).Returns(true);
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("__DDG_DieRecoveryGlobals__!DownBinsAllowed", DieRecovery.Globals.DieRecoveryTrackerGlobalContext)).Returns(0);

            var recovery = new DieRecoveryTracker("SliceTracking");
            Assert.AreEqual(true, recovery.UpdateTrackingStructure(InputType.Literal, "11111000", mode: UpdateMode.OverWrite));
            Assert.AreEqual(false, recovery.UpdateTrackingStructure(InputType.Literal, "11111111", mode: UpdateMode.OverWrite));
            Assert.AreEqual(true, recovery.UpdateTrackingStructure(InputType.Literal, "11111000", mode: UpdateMode.OverWrite));
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void UpdateTrackingStructure_DisableLinkingOneTracker_Pass()
        {
            // Create the trackers
            this.AddTracker("{'Name':'MainTracker', 'Size':4, 'LinkDisable': ['SubTracker1', 'SubTracker2' ] }");
            this.AddTracker("{'Name':'SubTracker1', 'Size':2 }");
            this.AddTracker("{'Name':'SubTracker2', 'Size':3 }");

            // Mock the expected datalog writes.
            var datalogMainInitial = this.GenerateTrackerDataLog("MainTracker", "0000", "0000", "0000", "0000");
            var datalogMainWrite0_1_1 = this.GenerateTrackerDataLog("MainTracker", "0000", "0001", "0000", "0001");
            var datalogMainWrite1_E_F = this.GenerateTrackerDataLog("MainTracker", "0000", "1110", "0001", "1111");
            var datalogMainWriteF_F_F = this.GenerateTrackerDataLog("MainTracker", "0000", "1111", "1111", "1111");
            var datalogSubInitial = this.GenerateTrackerDataLog("SubTracker1|SubTracker2", "00000", "00000", "00000", "00000");
            var datalogSub1LinkWrite0_3_3 = this.GenerateTrackerDataLog("SubTracker1", "00", "11", "00", "11");
            var datalogSub2LinkWrite0_7_7 = this.GenerateTrackerDataLog("SubTracker2", "000", "111", "000", "111");

            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.WriteToItuff(datalogMainInitial.Object));
            datalogMock.Setup(o => o.WriteToItuff(datalogMainWrite0_1_1.Object));
            datalogMock.Setup(o => o.WriteToItuff(datalogMainWrite1_E_F.Object));
            datalogMock.Setup(o => o.WriteToItuff(datalogMainWriteF_F_F.Object));
            datalogMock.Setup(o => o.WriteToItuff(datalogSubInitial.Object));
            datalogMock.Setup(o => o.WriteToItuff(datalogSub1LinkWrite0_3_3.Object));
            datalogMock.Setup(o => o.WriteToItuff(datalogSub2LinkWrite0_7_7.Object));
            datalogMock.SetupSequence(o => o.GetItuffStrgvalWriter())
                .Returns(datalogMainInitial.Object)
                .Returns(datalogSubInitial.Object)
                .Returns(datalogMainWrite0_1_1.Object)
                .Returns(datalogMainWrite1_E_F.Object)
                .Returns(datalogSub1LinkWrite0_3_3.Object)
                .Returns(datalogSub2LinkWrite0_7_7.Object)
                .Returns(datalogMainWriteF_F_F.Object);

            Prime.Services.DatalogService = datalogMock.Object;

            // Create recovery objects for each tracker.
            var recoveryMain = new DieRecoveryTracker("MainTracker");
            var recoverySub = new DieRecoveryTracker("SubTracker1,SubTracker2");

            // Initialize all the trackers to 0.
            Assert.IsTrue(recoveryMain.UpdateTrackingStructure("0000".ToBitArray(), mode: UpdateMode.OverWrite));
            Assert.IsTrue(recoverySub.UpdateTrackingStructure("00000".ToBitArray(), mode: UpdateMode.OverWrite));

            // Update the Main tracker with 1 bit set. (no linking should be triggered).
            Assert.IsTrue(recoveryMain.UpdateTrackingStructure("0001".ToBitArray(), mode: UpdateMode.Merge));

            // Update the Main tracker to all fail (linking should be triggered)
            Assert.IsTrue(recoveryMain.UpdateTrackingStructure("1110".ToBitArray(), mode: UpdateMode.Merge));

            // Update the Main tracker to all fail (linking should not be triggered since its already all fail)
            Assert.IsTrue(recoveryMain.UpdateTrackingStructure("1111".ToBitArray(), mode: UpdateMode.Merge));

            datalogMock.Verify(o => o.WriteToItuff(datalogMainInitial.Object), Times.Once);
            datalogMock.Verify(o => o.WriteToItuff(datalogMainWrite0_1_1.Object), Times.Once);
            datalogMock.Verify(o => o.WriteToItuff(datalogMainWrite1_E_F.Object), Times.Once);
            datalogMock.Verify(o => o.WriteToItuff(datalogMainWriteF_F_F.Object), Times.Once);
            datalogMock.Verify(o => o.WriteToItuff(datalogSubInitial.Object), Times.Once);
            datalogMock.Verify(o => o.WriteToItuff(datalogSub1LinkWrite0_3_3.Object), Times.Once);
            datalogMock.Verify(o => o.WriteToItuff(datalogSub2LinkWrite0_7_7.Object), Times.Once);
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void UpdateTrackingStructure_DisableLinkingMultipleTracker_Pass()
        {
            // Create the trackers
            this.AddTracker("{'Name':'MainTracker1', 'Size':1, 'LinkDisable': ['SubTracker1', 'SubTracker2', 'MainTracker2' ] }");
            this.AddTracker("{'Name':'MainTracker2', 'Size':1, 'LinkDisable': ['SubTracker1', 'SubTracker2', 'MainTracker1' ] }");
            this.AddTracker("{'Name':'SubTracker1', 'Size':2 }");
            this.AddTracker("{'Name':'SubTracker2', 'Size':2, 'LinkDisable': ['SubTracker3'] }");
            this.AddTracker("{'Name':'SubTracker3', 'Size':2 }");

            // Mock the expected datalog writes.
            var datalogMainWrite0_1_1 = this.GenerateTrackerDataLog("MainTracker1|MainTracker2", "00", "01", "00", "01");
            var datalogMain1LinkDisable = this.GenerateTrackerDataLog("MainTracker1", "0", "1", "0", "1");
            var datalogSub1LinkDisable = this.GenerateTrackerDataLog("SubTracker1", "00", "11", "00", "11");
            var datalogSub2LinkDisable = this.GenerateTrackerDataLog("SubTracker2", "00", "11", "00", "11");
            var datalogSub3LinkDisable = this.GenerateTrackerDataLog("SubTracker3", "00", "11", "00", "11");

            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.WriteToItuff(datalogMainWrite0_1_1.Object));
            datalogMock.Setup(o => o.WriteToItuff(datalogMain1LinkDisable.Object));
            datalogMock.Setup(o => o.WriteToItuff(datalogSub1LinkDisable.Object));
            datalogMock.Setup(o => o.WriteToItuff(datalogSub2LinkDisable.Object));
            datalogMock.Setup(o => o.WriteToItuff(datalogSub3LinkDisable.Object));
            datalogMock.SetupSequence(o => o.GetItuffStrgvalWriter())
                .Returns(datalogMainWrite0_1_1.Object)
                .Returns(datalogSub1LinkDisable.Object)
                .Returns(datalogSub2LinkDisable.Object)
                .Returns(datalogSub3LinkDisable.Object)
                .Returns(datalogMain1LinkDisable.Object);

            Prime.Services.DatalogService = datalogMock.Object;

            // Write a 1 to MainTracker2 ... it should cascade and disable everything...
            var recoveryMain = new DieRecoveryTracker("MainTracker1,MainTracker2");
            Assert.IsTrue(recoveryMain.UpdateTrackingStructure("01".ToBitArray(), mode: UpdateMode.Merge));

            datalogMock.Verify(o => o.WriteToItuff(datalogMainWrite0_1_1.Object), Times.Once);
            datalogMock.Verify(o => o.WriteToItuff(datalogMain1LinkDisable.Object), Times.Once);
            datalogMock.Verify(o => o.WriteToItuff(datalogSub1LinkDisable.Object), Times.Once);
            datalogMock.Verify(o => o.WriteToItuff(datalogSub2LinkDisable.Object), Times.Once);
            datalogMock.Verify(o => o.WriteToItuff(datalogSub3LinkDisable.Object), Times.Once);
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void RunRule_CoreDefeaturingVector_4C_Pass()
        {
            List<string> expectedBitVectors = new List<string>() { "00001111", "00111111", "11001111" };
            List<string> expectedNames = new List<string>() { "4C", "2C", "2C" };
            List<int> expectedSizes = new List<int>() { 4, 2, 2 };
            List<DefeatureRule.RuleMode> expectedModes = new List<DefeatureRule.RuleMode>() { DefeatureRule.RuleMode.ValidCombinations, DefeatureRule.RuleMode.ValidCombinations, DefeatureRule.RuleMode.ValidCombinations, };
            List<DefeatureRule.RuleType> expectedTypes = new List<DefeatureRule.RuleType>() { DefeatureRule.RuleType.FullyFeatured, DefeatureRule.RuleType.Recovery, DefeatureRule.RuleType.Recovery };

            var recovery = new DieRecoveryTracker("SliceTracking");
            BitArray bits = new BitArray(new bool[8] { false, false, false, false, true, true, true, true });
            var results = recovery.RunRule(bits, "CoreDefeaturingVector");

            Assert.AreEqual(expectedBitVectors.Count, results.Count, "Returned incorrect number of matches.");
            CollectionAssert.AreEqual(expectedBitVectors, results.Select(o => o.BitVector).ToList(), $"Returned incorrect matching bitvectors.");
            CollectionAssert.AreEqual(expectedNames, results.Select(o => o.Name).ToList(), $"Returned incorrect matching names.");
            CollectionAssert.AreEqual(expectedSizes, results.Select(o => o.Size).ToList(), $"Returned incorrect matching sizes.");
            CollectionAssert.AreEqual(expectedModes, results.Select(o => o.Mode).ToList(), $"Returned incorrect matching modes.");
            CollectionAssert.AreEqual(expectedTypes, results.Select(o => o.Type).ToList(), $"Returned incorrect matching types .");
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void RunRule_CoreDefeaturingVector_2CU_Pass()
        {
            List<string> expectedBitVectors = new List<string>() { "11001111" };
            List<string> expectedNames = new List<string>() { "2C" };
            List<int> expectedSizes = new List<int>() { 2 };
            List<DefeatureRule.RuleMode> expectedModes = new List<DefeatureRule.RuleMode>() { DefeatureRule.RuleMode.ValidCombinations, };
            List<DefeatureRule.RuleType> expectedTypes = new List<DefeatureRule.RuleType>() { DefeatureRule.RuleType.Recovery };

            var recovery = new DieRecoveryTracker("SliceTracking");
            BitArray bits = new BitArray(new bool[8] { false, true, false, false, true, true, true, true });

            // CollectionAssert.AreEqual(expected, recovery.RunRule(bits, "CoreDefeaturingVector"));
            var results = recovery.RunRule(bits, "CoreDefeaturingVector");
            Assert.AreEqual(expectedBitVectors.Count, results.Count, "Returned incorrect number of matches.");
            CollectionAssert.AreEqual(expectedBitVectors, results.Select(o => o.BitVector).ToList(), "Returned incorrect matching bitvectors.");
            CollectionAssert.AreEqual(expectedNames, results.Select(o => o.Name).ToList(), "Returned incorrect matching names.");
            CollectionAssert.AreEqual(expectedSizes, results.Select(o => o.Size).ToList(), "Returned incorrect matching sizes.");
            CollectionAssert.AreEqual(expectedModes, results.Select(o => o.Mode).ToList(), "Returned incorrect matching modes.");
            CollectionAssert.AreEqual(expectedTypes, results.Select(o => o.Type).ToList(), "Returned incorrect matching types.");
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void RunRule_CoreDefeaturingVector_2CL_Pass()
        {
            List<string> expectedBitVectors = new List<string>() { "00111111" };
            List<string> expectedNames = new List<string>() { "2C" };
            List<int> expectedSizes = new List<int>() { 2 };
            List<DefeatureRule.RuleMode> expectedModes = new List<DefeatureRule.RuleMode>() { DefeatureRule.RuleMode.ValidCombinations, };
            List<DefeatureRule.RuleType> expectedTypes = new List<DefeatureRule.RuleType>() { DefeatureRule.RuleType.Recovery };

            var recovery = new DieRecoveryTracker("SliceTracking");
            BitArray bits = new BitArray(new bool[8] { false, false, true, true, true, true, true, true });
            var results = recovery.RunRule(bits, "CoreDefeaturingVector");

            // CollectionAssert.AreEqual(expected, results);
            Assert.AreEqual(expectedBitVectors.Count, results.Count, "Returned incorrect number of matches.");
            CollectionAssert.AreEqual(expectedBitVectors, results.Select(o => o.BitVector).ToList(), "Returned incorrect matching bitvectors.");
            CollectionAssert.AreEqual(expectedNames, results.Select(o => o.Name).ToList(), "Returned incorrect matching names.");
            CollectionAssert.AreEqual(expectedSizes, results.Select(o => o.Size).ToList(), "Returned incorrect matching sizes.");
            CollectionAssert.AreEqual(expectedModes, results.Select(o => o.Mode).ToList(), "Returned incorrect matching modes.");
            CollectionAssert.AreEqual(expectedTypes, results.Select(o => o.Type).ToList(), "Returned incorrect matching types.");
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void RunRule_CoreDefeaturingVector_0C_Pass()
        {
            var datalog1 = this.GenerateTrackerDataLog("SliceTracking", "00000000", "01011111", "00000000", "01011111");
            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.WriteToItuff(datalog1.Object));
            datalogMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(datalog1.Object);
            Prime.Services.DatalogService = datalogMock.Object;

            var recovery = new DieRecoveryTracker("SliceTracking");
            BitArray bits = new BitArray(new bool[8] { false, true, false, true, true, true, true, true });
            recovery.UpdateTrackingStructure(bits);

            // CollectionAssert.AreEqual(new List<string>(), recovery.RunRule(bits, "CoreDefeaturingVector"));
            var results = recovery.RunRule("CoreDefeaturingVector");
            Assert.AreEqual(0, results.Count, "Returned incorrect number of matches.");
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void RunRule_InvalidSize_Fail()
        {
            var rule = new DefeatureRule.RuleContainer();
            rule.Mode = DefeatureRule.RuleMode.ValidCombinations;
            rule.Type = DefeatureRule.RuleType.FullyFeatured;
            rule.Name = "SomeName";
            rule.Size = 4;
            rule.FailWhen = true;
            rule.Values = new List<string>() { "0000" };

            List<string> passingBitVectors = new List<string>();

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => rule.IsPass("11".ToBitArray(), ref passingBitVectors));
            Assert.AreEqual("Mismatch between test vector and input vector lengths for Rule=[SomeName] TestVector=[0000] InputVector=[11].", ex.Message);
        }

        /// <summary>
        /// UnitTest of placeholder DieRecoveryBase.
        /// </summary>
        [TestMethod]
        public void RunRule_FailWhenTrue_Pass()
        {
            var rule = new DefeatureRule.RuleContainer();
            rule.Mode = DefeatureRule.RuleMode.ValidCombinations;
            rule.Type = DefeatureRule.RuleType.FullyFeatured;
            rule.Name = "SomeName";
            rule.Size = 4;
            rule.FailWhen = true;
            rule.Values = new List<string>() { "0000", "1111" };

            List<string> passingBitVectors = new List<string>();
            Assert.IsTrue(rule.IsPass("1111".ToBitArray(), ref passingBitVectors));
            Assert.AreEqual(1, passingBitVectors.Count);
            Assert.AreEqual("0000", passingBitVectors[0]);
        }

        /// <summary>
        /// Test the DefeatureRule.GetDefeatureRule fail cases.
        /// </summary>
        [TestMethod]
        public void GetDefeatureRule_Fail()
        {
            Assert.ThrowsException<KeyNotFoundException>(() => DefeatureRule.GetDefeatureRule("NotARealRule"));

            this.SharedStorageValues.Clear();
            Assert.ThrowsException<KeyNotFoundException>(() => DefeatureRule.GetDefeatureRule("CoreDefeaturingVector"));
        }

        private Mock<IStrgvalFormat> GenerateTrackerDataLog(string trackerName, string mask, string result, string current, string outgoing)
        {
            /*var datalogText = $"0_tname_{this.TestInstanceName}::{trackerName}\n";
            datalogText += $"0_strgval_TestResult:b{result}|Incoming:b{current}|Outgoing:b{outgoing}\n";
            Console.WriteLine($"Generated expected datalog\n{datalogText}");
            return datalogText; */

            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetTnamePostfix($"::{trackerName.Replace(",", "|")}"));
            writerMock.Setup(o => o.SetData($"Mask:b{mask}|TestResult:b{result}|Incoming:b{current}|Outgoing:b{outgoing}"));
            return writerMock;
        }

        private void AddTracker(string trackerJson)
        {
            Tracker tracker = JsonConvert.DeserializeObject<Tracker>(trackerJson);
            DDG.DieRecovery.Utilities.StoreTrackerDefinition(tracker);
        }
    }
}
