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

namespace DieRecoveryCallbacks.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using DDG;
    using DieRecoveryBase;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.DffService;
    using Prime.FileService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;
    using Prime.UserVarService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class DieRecoveryCallbacks_UnitTest
    {
        private static string TestInstanceName { get; } = "FakeModule::FakeTest";

        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => Console.WriteLine(s));
            consoleMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string n, string src) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleMock.Object;

            var testprogramMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramMock.Setup(o => o.GetCurrentTestInstanceName()).Returns(TestInstanceName);
            Prime.Services.TestProgramService = testprogramMock.Object;
        }

        /*
        /// <summary>
        /// Initializes a new instance of the <see cref="DieRecoveryCallbacks_UnitTest"/> class.
        /// </summary>
        public DieRecoveryCallbacks_UnitTest()
        {
            this.SharedStorageValues = new Dictionary<string, string>();

            // Default Mock for console service.
            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                Console.WriteLine(s);
            });
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) =>
                {
                    Console.WriteLine($"ERROR: {msg}");
                });
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;

            // Default Mock for Callback service.
            this.TestProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            this.TestProgramServiceMock.Setup(o => o.GetCurrentTestInstanceName()).Returns(TestInstanceName);
            this.TestProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("LogLevel")).Returns("DISABLED");
            Prime.Services.TestProgramService = this.TestProgramServiceMock.Object;

            // Default Mock for Shared service.
            this.SharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), Prime.SharedStorageService.Context.LOT))
                .Callback((string key, object obj, Context context) =>
                {
                    Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                });
            this.SharedStorageMock.Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), Prime.SharedStorageService.Context.LOT))
                .Callback((string key, Type obj, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                })
                .Returns((string key, Type obj, Context context) => JsonConvert.DeserializeObject(this.SharedStorageValues[key], obj));
            Prime.Services.SharedStorageService = this.SharedStorageMock.Object;
        }

        private static string TestInstanceName { get; } = "FakeModule::FakeTest";

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private Mock<ITestProgramService> TestProgramServiceMock { get; set; }

        private Dictionary<string, string> SharedStorageValues { get; set; }

        private Mock<ISharedStorageService> SharedStorageMock { get; set; } */

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Configure_Fail()
        {
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.DisableIP(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.DisableIP("--WrongArgs=1"));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.DisableIP("--pinmap=FakePinMap"));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Configure_plistArg_Pass()
        {
            var trackerName = "TwoSliceTracker";
            var pinMapName = "CORE0_NOA,CORE1_NOA";
            var trackerValue = "10";
            var literalValue = "01";
            var plist = "FakePList";

            var tracker = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            tracker.Setup(o => o.GetMaskBits()).Returns(trackerValue.ToBitArray());

            var recovery = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            recovery.Setup(o => o.Get(trackerName)).Returns(tracker.Object);
            DDG.DieRecovery.Service = recovery.Object;

            var pinMap = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMap.Setup(o => o.ApplyPatConfig(It.IsAny<BitArray>()));
            pinMap.Setup(o => o.ApplyPatConfig(It.IsAny<BitArray>(), plist));

            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            pinMapServiceMock.Setup(o => o.Get(pinMapName)).Returns(pinMap.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            // Call the method under test. (with --value, plist=global)
            DieRecoveryCallbacks.DisableIP($"--pinmap={pinMapName} --value={literalValue} --patlist=global");
            pinMap.Verify(o => o.ApplyPatConfig(It.Is<BitArray>(actual => actual.ToBinaryString() == literalValue)), Times.Once);

            // Call the method under test. (with --tracker and --patlist)
            DieRecoveryCallbacks.DisableIP($"--pinmap={pinMapName} --tracker={trackerName} --patlist={plist}");
            pinMap.Verify(o => o.ApplyPatConfig(It.Is<BitArray>(actual => actual.ToBinaryString() == trackerValue), plist), Times.Once);
            tracker.Verify(o => o.GetMaskBits(), Times.Once);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Configure_globalPlistNotSpecified_Pass()
        {
            var pinMapName = "CORE0_NOA,CORE1_NOA";
            var literalValue = "01";

            var recovery = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            DDG.DieRecovery.Service = recovery.Object;

            var pinMap = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMap.Setup(o => o.ApplyPatConfig(It.IsAny<BitArray>()));

            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            pinMapServiceMock.Setup(o => o.Get(pinMapName)).Returns(pinMap.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceName()).Returns(TestInstanceName);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters())
                .Returns(new Dictionary<string, string> { { "param1", "value1" }, { "param2", "value2" }, { "timing", "localTiming" }, });
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            // Call the method under test. (with --value with no plist arg and no plist parameter in the testinstance)
            DieRecoveryCallbacks.DisableIP($"--pinmap={pinMapName} --value={literalValue}");
            pinMap.Verify(o => o.ApplyPatConfig(It.Is<BitArray>(actual => actual.ToBinaryString() == literalValue)), Times.Once);
            testProgramServiceMock.VerifyAll();
            pinMap.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Configure_localPlistSpecified_Pass()
        {
            var pinMapName = "CORE0_NOA,CORE1_NOA";
            var literalValue = "01";

            var recovery = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            DDG.DieRecovery.Service = recovery.Object;

            var pinMap = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMap.Setup(o => o.ApplyPatConfig(It.IsAny<BitArray>(), "localPlist1"));

            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            pinMapServiceMock.Setup(o => o.Get(pinMapName)).Returns(pinMap.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceName()).Returns(TestInstanceName);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string> { { "Patlist", "localPlist1" } });
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            // Call the method under test. (with --value, plist=local)
            DieRecoveryCallbacks.DisableIP($"--pinmap={pinMapName} --value={literalValue} --patlist=local");
            pinMap.Verify(o => o.ApplyPatConfig(It.Is<BitArray>(actual => actual.ToBinaryString() == literalValue), "localPlist1"), Times.Once);
            testProgramServiceMock.VerifyAll();
            pinMap.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Configure_localPlistNotSpecified_Pass()
        {
            var pinMapName = "CORE0_NOA,CORE1_NOA";
            var literalValue = "01";

            var recovery = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            DDG.DieRecovery.Service = recovery.Object;

            var pinMap = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMap.Setup(o => o.ApplyPatConfig(It.IsAny<BitArray>(), "localPlist2"));
            pinMap.Setup(o => o.ApplyPatConfig(It.IsAny<BitArray>(), "localPlist3"));

            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            pinMapServiceMock.Setup(o => o.Get(pinMapName)).Returns(pinMap.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceName()).Returns(TestInstanceName);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters())
                .Returns(new Dictionary<string, string> { { "patlist", "localPlist2" }, { "reset_patlist", "localPlist3" }, { "timing", "localTiming" }, });
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            // Call the method under test. (with --value with no plist arg)
            DieRecoveryCallbacks.DisableIP($"--pinmap={pinMapName} --value={literalValue}");
            pinMap.Verify(o => o.ApplyPatConfig(It.Is<BitArray>(actual => actual.ToBinaryString() == literalValue), "localPlist2"), Times.Once);
            pinMap.Verify(o => o.ApplyPatConfig(It.Is<BitArray>(actual => actual.ToBinaryString() == literalValue), "localPlist3"), Times.Once);
            testProgramServiceMock.VerifyAll();
            pinMap.VerifyAll();
        }

        /// <summary>
        /// Test the failing cases of MaskIP.
        /// </summary>
        [TestMethod]
        public void MaskIP_Fail()
        {
            var noArgs = Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.MaskIP(string.Empty));
            Assert.AreEqual("MaskIP: failed parsing arguments. CommandLine.MissingRequiredOptionError", noArgs.Message);

            var badArgs = Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.MaskIP("--WrongArgs=1"));
            Assert.AreEqual("MaskIP: failed parsing arguments. CommandLine.UnknownOptionError\nCommandLine.MissingRequiredOptionError", badArgs.Message);

            var noTracker = Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.MaskIP("--pinmap=FakePinMap"));
            Assert.AreEqual("One of [--value, --tracker] is required for MaskIP.", noTracker.Message);
        }

        /// <summary>
        /// Test the passing cases of MaskIP.
        /// </summary>
        [TestMethod]
        public void MaskIP_Tracker_Pass()
        {
            var trackerValue = "1111";
            var trackerName = "FakeTracker";
            var pinMapName = "FakePinMap";
            var pinsToMask = "Pin1,Pin2,Pin3,Pin4";

            // setup the mocks
            var trackerMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            trackerMock.Setup(o => o.GetMaskBits()).Returns(trackerValue.ToBitArray());

            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.Setup(o => o.GetMaskPins(It.Is<BitArray>(actual => actual.ToBinaryString() == trackerValue), null)).Returns(pinsToMask.Split(',').ToList());

            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            pinMapServiceMock.Setup(o => o.Get(pinMapName)).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            var recoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            recoveryServiceMock.Setup(o => o.Get(trackerName)).Returns(trackerMock.Object);
            DDG.DieRecovery.Service = recoveryServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SomeToken", pinsToMask, Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // run the test.
            var retVal = DieRecoveryCallbacks.MaskIP($"--tracker {trackerName} --pinmap {pinMapName} --gsds G.U.S.SomeToken");

            // check the results.
            Assert.AreEqual(pinsToMask, retVal);
            pinMapMock.VerifyAll();
            trackerMock.VerifyAll();
            recoveryServiceMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Test the passing cases of MaskIP.
        /// </summary>
        [TestMethod]
        public void MaskIP_TrackerAndValue_Pass()
        {
            var trackerValue = "1100";
            var maskValue = "0011";
            var trackerName = "FakeTracker";
            var pinMapName = "FakePinMap";
            var pinsToMask = "Pin1,Pin2,Pin3,Pin4";
            var additionalPins = "Pin5,Pin6";

            // setup the mocks
            var trackerMock = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            trackerMock.Setup(o => o.GetMaskBits()).Returns(trackerValue.ToBitArray());

            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.Setup(o => o.GetMaskPins(new BitArray(4, true), new List<string> { "Pin5", "Pin6" })).Returns(pinsToMask.Split(',').ToList());

            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            pinMapServiceMock.Setup(o => o.Get(pinMapName)).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            var recoveryServiceMock = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            recoveryServiceMock.Setup(o => o.Get(trackerName)).Returns(trackerMock.Object);
            DDG.DieRecovery.Service = recoveryServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SomeToken", pinsToMask, Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // run the test.
            var retVal = DieRecoveryCallbacks.MaskIP($"--tracker {trackerName} --value {maskValue} --pinmap {pinMapName} --maskpins {additionalPins} --gsds G.U.S.SomeToken");

            // check the results.
            Assert.AreEqual(pinsToMask, retVal);
            pinMapMock.VerifyAll();
            trackerMock.VerifyAll();
            recoveryServiceMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Test the passing cases of MaskIP.
        /// </summary>
        [TestMethod]
        public void MaskIP_ValueNoGsds_Pass()
        {
            var trackerValue = "0011";
            var pinMapName = "FakePinMap";
            var pinsToMask = "Pin2,Pin3";

            // setup the mocks
            var pinMapMock = new Mock<DDG.IPinMap>(MockBehavior.Strict);
            pinMapMock.Setup(o => o.GetMaskPins(It.Is<BitArray>(actual => actual.ToBinaryString() == trackerValue), null)).Returns(pinsToMask.Split(',').ToList());

            var pinMapServiceMock = new Mock<DDG.IPinMapFactory>(MockBehavior.Strict);
            pinMapServiceMock.Setup(o => o.Get(pinMapName)).Returns(pinMapMock.Object);
            DDG.PinMap.Service = pinMapServiceMock.Object;

            // run the test.
            var retVal = DieRecoveryCallbacks.MaskIP($"--value {trackerValue} --pinmap FakePinMap");

            // check the results.
            Assert.AreEqual(pinsToMask, retVal);
            pinMapMock.VerifyAll();
            pinMapServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void WriteTracker_Fail()
        {
            MockTrackerForUpdate("DummyTracker", true);
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.WriteTracker(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.WriteTracker("--dff=U1:FakeDFF2 --uservar=SampleCollection.SampleVar --gsds=G.L.S.SampleGsds"));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.WriteTracker("--tracker=DummyTracker"));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.WriteTracker("--tracker=DummyTracker --dff=DffWrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.WriteTracker("--tracker=DummyTracker --value=abcd"));

            MockTrackerForUpdate("DummyTracker", false);
            Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => DieRecoveryCallbacks.WriteTracker("--tracker=DummyTracker --value=0011"));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void WriteTracker_FromValue_Pass()
        {
            var trackerMock = MockTrackerForUpdate("DummyTracker");

            // Call the method under test.
            DieRecoveryCallbacks.WriteTracker("--tracker=DummyTracker --value=0011");
            trackerMock.Verify(o => o.UpdateTrackingStructure(It.Is<BitArray>(actual => actual.ToBinaryString() == "0011"), null, null, UpdateMode.OverWrite, true), Times.Once);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void WriteTracker_FromValueNoPrint_Pass()
        {
            var trackerMock = MockTrackerForUpdate("DummyTracker", log: false);

            // Call the method under test.
            DieRecoveryCallbacks.WriteTracker("--tracker=DummyTracker --value=0011 --noprint");
            trackerMock.Verify(o => o.UpdateTrackingStructure(It.Is<BitArray>(actual => actual.ToBinaryString() == "0011"), null, null, UpdateMode.OverWrite, false), Times.Once);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void WriteTracker_FromGsds_Pass()
        {
            var trackerMock = MockTrackerForUpdate("DummyTracker1,DummyTracker2");

            // Mock the GSDS.
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.GetStringRowFromTable("SampleGsds", Context.DUT)).Returns("11010111");
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // Call the method under test.
            DieRecoveryCallbacks.WriteTracker("--tracker=DummyTracker1,DummyTracker2 --gsds=G.U.S.SampleGsds");
            trackerMock.Verify(o => o.UpdateTrackingStructure(It.Is<BitArray>(actual => actual.ToBinaryString() == "11010111"), null, null, UpdateMode.OverWrite, true), Times.Once);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void WriteTracker_FromUserVar_Pass()
        {
            var trackerMock = MockTrackerForUpdate("DummyTracker");

            // Mock the UserVar.
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.GetStringValue("SampleCollection.SampleVar")).Returns("1100");
            Prime.Services.UserVarService = userVarMock.Object;

            // Call the method under test.
            DieRecoveryCallbacks.WriteTracker("--tracker=DummyTracker --uservar=SampleCollection.SampleVar");
            trackerMock.Verify(o => o.UpdateTrackingStructure(It.Is<BitArray>(actual => actual.ToBinaryString() == "1100"), null, null, UpdateMode.OverWrite, true), Times.Once);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void WriteTracker_Reset_Pass()
        {
            var trackerMock = MockTrackerForUpdate("DummyTracker", resetValue: "1111");

            // Call the method under test.
            DieRecoveryCallbacks.WriteTracker("--tracker=DummyTracker --reset");
            trackerMock.Verify(o => o.UpdateTrackingStructure(It.Is<BitArray>(actual => actual.ToBinaryString() == "1111"), null, null, UpdateMode.OverWrite, true), Times.Once);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void WriteTracker_FromDff_Pass()
        {
            var trackerMock = MockTrackerForUpdate("DummyTracker");

            // Mock the DFF.
            var dffMock = new Mock<IDffService>(MockBehavior.Strict);
            dffMock.Setup(o => o.GetDff("FakeDFF", "PBIC", "U1", true)).Returns("1");
            dffMock.Setup(o => o.GetDffByOpType("FakeDFF", "PBIC", true)).Returns("11111");
            Prime.Services.DffService = dffMock.Object;

            // Call the method under test.
            DieRecoveryCallbacks.WriteTracker("--tracker=DummyTracker --dff=U1:PBIC:FakeDFF");
            trackerMock.Verify(o => o.UpdateTrackingStructure(It.Is<BitArray>(actual => actual.ToBinaryString() == "1"), null, null, UpdateMode.OverWrite, true), Times.Once);

            DieRecoveryCallbacks.WriteTracker("--tracker=DummyTracker --dff=PBIC:FakeDFF");
            trackerMock.Verify(o => o.UpdateTrackingStructure(It.Is<BitArray>(actual => actual.ToBinaryString() == "11111"), null, null, UpdateMode.OverWrite, true), Times.Once);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void WriteTracker_FromTrackerMerge_Pass()
        {
            // setup the mocks
            var trackerSrcName = "CORE0,CORE1,CORE2,CORE3";
            var trackerDestName = "SliceTracking4";

            var trackerSrcMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            trackerSrcMock.Setup(o => o.GetMaskBits()).Returns("0011".ToBitArray());

            var trackerDestMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            trackerDestMock.Setup(o => o.UpdateTrackingStructure(It.Is<BitArray>(actual => actual.ToBinaryString() == "0011"), null, null, UpdateMode.Merge, true)).Returns(true);

            var recoveryServiceMock = new Mock<IDieRecoveryFactory>(MockBehavior.Strict);
            recoveryServiceMock.Setup(o => o.Get(trackerSrcName)).Returns(trackerSrcMock.Object);
            recoveryServiceMock.Setup(o => o.Get(trackerDestName)).Returns(trackerDestMock.Object);
            DDG.DieRecovery.Service = recoveryServiceMock.Object;

            // Call the method under test.
            DieRecoveryCallbacks.WriteTracker($"--tracker={trackerDestName} --src_tracker={trackerSrcName} --merge");

            // verify the mocks.
            trackerSrcMock.VerifyAll();
            trackerDestMock.VerifyAll();
            recoveryServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CopyTracker_Fail()
        {
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.CopyTracker(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.CopyTracker("--wrongarg=Idontknow"));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.CopyTracker("--tracker=DummyTracker"));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.CopyTracker("--dff=U1:FakeDFF2 --uservar=SampleCollection.SampleVar --gsds=G.L.S.SampleGsds"));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CopyTracker_Pass()
        {
            MockTrackerForRead("DummyTracker", "10101");

            // Mock the Storage elements.
            var dffMock = new Mock<IDffService>(MockBehavior.Strict);
            dffMock.Setup(o => o.SetDff("FakeDFF2", "10101"));
            dffMock.Setup(o => o.SetDff("FakeDFF", "10101", "U1"));
            Prime.Services.DffService = dffMock.Object;

            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.SetValue("SampleCollection.SampleVar", "10101"));
            Prime.Services.UserVarService = userVarMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("SampleGsds", "10101", Context.LOT));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // Call the method under test.
            DieRecoveryCallbacks.CopyTracker("--tracker=DummyTracker --dff=FakeDFF2 --uservar=SampleCollection.SampleVar --gsds=G.L.S.SampleGsds");

            // Verify everything worked correctly.
            dffMock.Verify(o => o.SetDff("FakeDFF2", "10101"), Times.Once);
            dffMock.Verify(o => o.SetDff("FakeDFF", "10101", "U1"), Times.Never);
            userVarMock.Verify(o => o.SetValue("SampleCollection.SampleVar", "10101"), Times.Once);
            sharedStorageMock.Verify(o => o.InsertRowAtTable("SampleGsds", "10101", Context.LOT), Times.Once);

            // Call the method under test.
            DieRecoveryCallbacks.CopyTracker("--tracker=DummyTracker --dff=U1:FakeDFF");

            // Verify everything worked correctly.
            dffMock.Verify(o => o.SetDff("FakeDFF2", "10101"), Times.Once);
            dffMock.Verify(o => o.SetDff("FakeDFF", "10101", "U1"), Times.Once);
            userVarMock.Verify(o => o.SetValue("SampleCollection.SampleVar", "10101"), Times.Once);
            sharedStorageMock.Verify(o => o.InsertRowAtTable("SampleGsds", "10101", Context.LOT), Times.Once);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CloneTracker_Pass()
        {
            var recovery = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            recovery.Setup(o => o.CloneTracker("SourceTracker", "NewTracker"));
            DDG.DieRecovery.Service = recovery.Object;

            // Call the method under test.
            DieRecoveryCallbacks.CloneTracker("--existing_tracker=SourceTracker --new_tracker=NewTracker");
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CloneTracker_Fail()
        {
            var recovery = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            recovery.Setup(o => o.CloneTracker("SourceTracker", "NewTracker"));
            DDG.DieRecovery.Service = recovery.Object;

            // Call the method under test.
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.CloneTracker("--typo_here=SourceTracker --new_tracker=NewTracker"));
        }

        /// <summary>
        /// Test the LoadPinMap callback (failing cases).
        /// </summary>
        [TestMethod]
        public void LoadPinMap_Fail()
        {
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.LoadPinMapFile(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.LoadPinMapFile("--decoder=somedecoder"));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.LoadPinMapFile("--file=somefile"));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.LoadPinMapFile("--file"));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.LoadPinMapFile("--wrongarg=Idontknow"));

            var fileServicesMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServicesMock.Setup(o => o.FileExists("FakeFile")).Returns(false);
            Prime.Services.FileService = fileServicesMock.Object;
            Assert.ThrowsException<FileNotFoundException>(() => DieRecoveryCallbacks.LoadPinMapFile("--decoder=PinToSliceIndexDecoder --file=FakeFile"));

            fileServicesMock.Setup(o => o.GetFile("SamplePinMapDecoder1_Fail.json")).Returns(GetPathToFiles() + "SamplePinMapDecoder1_Fail.json");
            fileServicesMock.Setup(o => o.FileExists("SamplePinMapDecoder1_Fail.json")).Returns(true);
            Prime.Services.FileService = fileServicesMock.Object;
            Assert.ThrowsException<JsonReaderException>(() => DieRecoveryCallbacks.LoadPinMapFile("--decoder=PinToSliceIndexDecoder --file=SamplePinMapDecoder1_Fail.json"));

            fileServicesMock.Setup(o => o.FileExists("FakeFile")).Returns(true);
            Prime.Services.FileService = fileServicesMock.Object;
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => DieRecoveryCallbacks.LoadPinMapFile("--decoder=InvalidDecoder --file=FakeFile"));
            Assert.IsTrue(ex.Message.StartsWith("Invalid Decoder=[InvalidDecoder]. Valid Decoders are ["));
        }

        /// <summary>
        /// Test the LoadPinMap callback (passing case).
        /// </summary>
        [TestMethod]
        public void LoadPinMap_Pass()
        {
            this.LoadPinMapTest("--decoder=PinToSliceIndexDecoder --file=SamplePinMapDecoder1_Pass.json");
            this.LoadPinMapTest("--decoder PinToSliceIndexDecoder --file SamplePinMapDecoder1_Pass.json");
            this.LoadPinMapTest("--decoder PinToSliceIndexDecoder --file  SamplePinMapDecoder1_Pass.json");
            this.LoadPinMapTest("--decoder  PinToSliceIndexDecoder --file SamplePinMapDecoder1_Pass.json");
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void RunRule_Fail_Exceptions()
        {
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.RunRule(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.RunRule("--wrongarg Idontknow"));
        }

        /// <summary>
        /// Test the case where runrule doesn't match.
        /// </summary>
        [TestMethod]
        public void RunRule_Fail()
        {
            var mocks = MockRules("FakeTracker", "AtLeastOneEnabled", Context.DUT, "PassingRuleName", string.Empty, new List<DefeatureRule.Rule>());
            DieRecoveryCallbacks.RunRule("--tracker FakeTracker --rule AtLeastOneEnabled --gsds G.U.S.PassingRuleName");
            mocks.ForEach(o => o.VerifyAll());
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void RunRule_Pass()
        {
            var rules = new List<DefeatureRule.Rule> { new DefeatureRule.Rule("CORE2Valid", "00100", 1, DefeatureRule.RuleType.Recovery, DefeatureRule.RuleMode.ValidCombinations) };

            var mocks = MockRules("FakeTracker", "AtLeastOneEnabled", Context.DUT, "PassingRuleName", "CORE2Valid", rules);
            DieRecoveryCallbacks.RunRule("--tracker FakeTracker --rule AtLeastOneEnabled --gsds G.U.S.PassingRuleName");
            mocks.ForEach(o => o.VerifyAll());

            mocks = MockRules("FakeTracker", "AtLeastOneEnabled", Context.DUT, "PassingRuleName", "CORE2Valid", rules);
            DieRecoveryCallbacks.RunRule("--tracker FakeTracker --rule AtLeastOneEnabled --gsds G.U.S.PassingRuleName --store_value name");
            mocks.ForEach(o => o.VerifyAll());

            mocks = MockRules("FakeTracker", "AtLeastOneEnabled", Context.DUT, "PassingRuleName", "1", rules);
            DieRecoveryCallbacks.RunRule("--tracker FakeTracker --rule AtLeastOneEnabled --gsds G.U.S.PassingRuleName --store_value size");
            mocks.ForEach(o => o.VerifyAll());

            mocks = MockRules("FakeTracker", "AtLeastOneEnabled", Context.DUT, "PassingRuleName", "00100", rules);
            DieRecoveryCallbacks.RunRule("--tracker FakeTracker --rule AtLeastOneEnabled --gsds G.U.S.PassingRuleName --store_value bitvector");
            mocks.ForEach(o => o.VerifyAll());

            mocks = MockRules("FakeTracker", "AtLeastOneEnabled", Context.DUT, "PassingRuleName", string.Empty, new List<DefeatureRule.Rule>());
            DieRecoveryCallbacks.RunRule("--tracker FakeTracker --rule AtLeastOneEnabled --gsds G.U.S.PassingRuleName");
            mocks.ForEach(o => o.VerifyAll());

            mocks = MockRules("FakeTracker", "AtLeastOneEnabled", Context.DUT, "PassingRuleName", "0", new List<DefeatureRule.Rule>());
            DieRecoveryCallbacks.RunRule("--tracker FakeTracker --rule AtLeastOneEnabled --gsds G.U.S.PassingRuleName --store_value size");
            mocks.ForEach(o => o.VerifyAll());

            MockRules("FakeTracker", "AtLeastOneEnabled", Context.DUT, "PassingRuleName", "00100", rules);
            Assert.ThrowsException<ArgumentException>(() => DieRecoveryCallbacks.RunRule("--tracker FakeTracker --rule AtLeastOneEnabled --gsds G.U.S.PassingRuleName --store_value notvalid"));
        }

        private static List<Mock> MockRules(string trackerName, string ruleName, Context gsdsContext, string gsdsName, string gsdsExpect, List<DefeatureRule.Rule> rulesResult)
        {
            var dieRecoveryTrackerMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            dieRecoveryTrackerMock.Setup(o => o.RunRule(ruleName)).Returns(rulesResult);

            var dieRecoveryServiceMock = new Mock<IDieRecoveryFactory>(MockBehavior.Strict);
            dieRecoveryServiceMock.Setup(o => o.Get(trackerName)).Returns(dieRecoveryTrackerMock.Object);
            DDG.DieRecovery.Service = dieRecoveryServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable(gsdsName, gsdsExpect, gsdsContext));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            return new List<Mock> { dieRecoveryTrackerMock, dieRecoveryServiceMock, sharedStorageMock };
        }

        /// <summary>
        /// The GetPathToFiles.
        /// </summary>
        /// <param name="srcPath">The srcPath<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string GetPathToFiles([CallerFilePath] string srcPath = "")
        {
            return Path.GetDirectoryName(srcPath) + "\\InputFiles\\";
        }

        private static Mock<IDieRecovery> MockTrackerForUpdate(string name, bool updateRslt = true, string resetValue = "0000", bool log = true)
        {
            var tracker = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            tracker.Setup(o => o.UpdateTrackingStructure(It.IsAny<BitArray>(), null, null, UpdateMode.OverWrite, log)).Returns(updateRslt);
            tracker.SetupGet(o => o.ResetValue).Returns(resetValue);

            var recovery = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            recovery.Setup(o => o.Get(name)).Returns(tracker.Object);
            DDG.DieRecovery.Service = recovery.Object;
            return tracker;
        }

        private static void MockTrackerForRead(string name, string value)
        {
            var tracker = new Mock<DDG.IDieRecovery>(MockBehavior.Strict);
            tracker.Setup(o => o.GetMaskBits()).Returns(value.ToBitArray());

            var recovery = new Mock<DDG.IDieRecoveryFactory>(MockBehavior.Strict);
            recovery.Setup(o => o.Get(name)).Returns(tracker.Object);
            DDG.DieRecovery.Service = recovery.Object;
        }

        private static bool AreEqual(PinToSliceIndexDecoder decoder1, string decoder2AsStr)
        {
            var decoder2 = (PinToSliceIndexDecoder)JsonConvert.DeserializeObject(decoder2AsStr, typeof(PinToSliceIndexDecoder));

            if (decoder1.Name != decoder2.Name)
            {
                Console.WriteLine($"AreEqual:Name {decoder1.Name} != {decoder2.Name}");
                return false;
            }

            if (decoder1.NumberOfTrackerElements != decoder2.NumberOfTrackerElements)
            {
                Console.WriteLine($"AreEqual:NumberOfTrackerElements {decoder1.NumberOfTrackerElements} != {decoder2.NumberOfTrackerElements}");
                return false;
            }

            if (decoder1.IpPatternConfigure != decoder2.IpPatternConfigure)
            {
                Console.WriteLine($"AreEqual:IpPatternConfigure {decoder1.IpPatternConfigure} != {decoder2.IpPatternConfigure}");
                return false;
            }

            var decoder1Pins = decoder1.PinToSliceIndexMap.Keys.OrderBy(i => i).ToList();
            var decoder2Pins = decoder2.PinToSliceIndexMap.Keys.OrderBy(i => i).ToList();
            if (!Enumerable.SequenceEqual(decoder1Pins, decoder2Pins))
            {
                Console.WriteLine($"AreEqual:Keys [{string.Join(", ", decoder1Pins)}] != [{string.Join(", ", decoder2Pins)}]");
                return false;
            }

            foreach (var k in decoder1.PinToSliceIndexMap.Keys)
            {
                if (!Enumerable.SequenceEqual(decoder1.PinToSliceIndexMap[k], decoder2.PinToSliceIndexMap[k]))
                {
                    Console.WriteLine($"AreEqual:Map[{k}] [{string.Join(", ", decoder1.PinToSliceIndexMap[k])}] != [{string.Join(", ", decoder2.PinToSliceIndexMap[k])}]");
                    return false;
                }
            }

            return true;
        }

        private void LoadPinMapTest(string args)
        {
            // pinmaps in the file being loaded.
            var pinMap1 = "{'Name':'TestPinMapA','Size':4,'PatternModify':'dummyA','PinToSliceIndexMap':{'Pin1':[0],'Pin2':[1],'Pin3':[2],'Pin4':[3]}}";
            var pinMap2 = "{'Name':'TestPinMapC','Size':4,'PatternModify':'dummyC','PinToSliceIndexMap':{'Pin1':[0],'Pin2':[0],'Pin3':[2],'Pin4':[2]}}";
            var pinMap3 = "{'Name':'TestPinMapB','Size':4,'PatternModify':'dummyB','PinToSliceIndexMap':{'Pin5':[0,1,2,3]}}";

            var fileServicesMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServicesMock.Setup(o => o.GetFile("SamplePinMapDecoder1_Pass.json")).Returns(GetPathToFiles() + "SamplePinMapDecoder1_Pass.json");
            fileServicesMock.Setup(o => o.FileExists("SamplePinMapDecoder1_Pass.json")).Returns(true);
            Prime.Services.FileService = fileServicesMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("__DDG_DieRecoveryPinMapTable__!TestPinMapA", It.IsAny<PinToSliceIndexDecoder>(), Context.DUT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("__DDG_DieRecoveryPinMapTable__!TestPinMapB", It.IsAny<PinToSliceIndexDecoder>(), Context.DUT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("__DDG_DieRecoveryPinMapTable__!TestPinMapC", It.IsAny<PinToSliceIndexDecoder>(), Context.DUT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("__DDG_DieRecoveryPinMapTypeTable__!TestPinMapA", "DieRecoveryBase.PinToSliceIndexDecoder", Context.DUT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("__DDG_DieRecoveryPinMapTypeTable__!TestPinMapB", "DieRecoveryBase.PinToSliceIndexDecoder", Context.DUT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("__DDG_DieRecoveryPinMapTypeTable__!TestPinMapC", "DieRecoveryBase.PinToSliceIndexDecoder", Context.DUT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("__DDG_DieRecoveryPinMapTable__", "TestPinMapA", Context.DUT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("__DDG_DieRecoveryPinMapTable__", "TestPinMapA,TestPinMapC", Context.DUT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("__DDG_DieRecoveryPinMapTable__", "TestPinMapA,TestPinMapC,TestPinMapB", Context.DUT));
            sharedStorageMock.SetupSequence(o => o.KeyExistsInStringTable("__DDG_DieRecoveryPinMapTable__", Context.DUT))
                .Returns(false)
                .Returns(true)
                .Returns(true);
            sharedStorageMock.SetupSequence(o => o.GetStringRowFromTable("__DDG_DieRecoveryPinMapTable__", Context.DUT))
                .Returns("TestPinMapA")
                .Returns("TestPinMapA,TestPinMapC");
            sharedStorageMock.Setup(o => o.OverrideObjectRowResetPolicy("__DDG_DieRecoveryPinMapTable__!TestPinMapA", ResetPolicy.NEVER_RESET, Context.DUT));
            sharedStorageMock.Setup(o => o.OverrideObjectRowResetPolicy("__DDG_DieRecoveryPinMapTable__!TestPinMapB", ResetPolicy.NEVER_RESET, Context.DUT));
            sharedStorageMock.Setup(o => o.OverrideObjectRowResetPolicy("__DDG_DieRecoveryPinMapTable__!TestPinMapC", ResetPolicy.NEVER_RESET, Context.DUT));
            sharedStorageMock.Setup(o => o.OverrideStringRowResetPolicy("__DDG_DieRecoveryPinMapTypeTable__!TestPinMapA", ResetPolicy.NEVER_RESET, Context.DUT));
            sharedStorageMock.Setup(o => o.OverrideStringRowResetPolicy("__DDG_DieRecoveryPinMapTypeTable__!TestPinMapB", ResetPolicy.NEVER_RESET, Context.DUT));
            sharedStorageMock.Setup(o => o.OverrideStringRowResetPolicy("__DDG_DieRecoveryPinMapTypeTable__!TestPinMapC", ResetPolicy.NEVER_RESET, Context.DUT));
            sharedStorageMock.Setup(o => o.OverrideStringRowResetPolicy("__DDG_DieRecoveryPinMapTable__", ResetPolicy.NEVER_RESET, Context.DUT));

            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // Run the test
            DieRecoveryCallbacks.LoadPinMapFile(args);

            // Verify that the correct pinmaps were loaded.
            sharedStorageMock.Verify(o => o.InsertRowAtTable("__DDG_DieRecoveryPinMapTable__!TestPinMapA", It.Is<PinToSliceIndexDecoder>(p => AreEqual(p, pinMap1)), Context.DUT), Times.Once);
            sharedStorageMock.Verify(o => o.InsertRowAtTable("__DDG_DieRecoveryPinMapTable__!TestPinMapB", It.Is<PinToSliceIndexDecoder>(p => AreEqual(p, pinMap3)), Context.DUT), Times.Once);
            sharedStorageMock.Verify(o => o.InsertRowAtTable("__DDG_DieRecoveryPinMapTable__!TestPinMapC", It.Is<PinToSliceIndexDecoder>(p => AreEqual(p, pinMap2)), Context.DUT), Times.Once);
            sharedStorageMock.VerifyAll();

            // check the AreEqual function is correct...
            Assert.IsTrue(AreEqual((PinToSliceIndexDecoder)JsonConvert.DeserializeObject(pinMap1, typeof(PinToSliceIndexDecoder)), pinMap1));

            var pinMap1DiffName = "{'Name':'TestPinMapB','Size':4,'PatternModify':'dummyA','PinToSliceIndexMap':{'Pin1':[0],'Pin2':[1],'Pin3':[2],'Pin4':[3]}}";
            var pinMap1DiffSize = "{'Name':'TestPinMapA','Size':5,'PatternModify':'dummyA','PinToSliceIndexMap':{'Pin1':[0],'Pin2':[1],'Pin3':[2],'Pin4':[3]}}";
            var pinMap1DiffConfig = "{'Name':'TestPinMapA','Size':4,'PatternModify':'dummyB','PinToSliceIndexMap':{'Pin1':[0],'Pin2':[1],'Pin3':[2],'Pin4':[3]}}";
            var pinMap1DiffMap = "{'Name':'TestPinMapA','Size':4,'PatternModify':'dummyA','PinToSliceIndexMap':{'Pin1':[0],'Pin2':[1],'Pin3':[3],'Pin4':[2]}}";
            var pinMap1DiffPins = "{'Name':'TestPinMapA','Size':4,'PatternModify':'dummyA','PinToSliceIndexMap':{'Pin1':[0],'Pin2':[1],'Pin5':[2],'Pin6':[3]}}";
            Assert.IsFalse(AreEqual((PinToSliceIndexDecoder)JsonConvert.DeserializeObject(pinMap1DiffName, typeof(PinToSliceIndexDecoder)), pinMap1));
            Assert.IsFalse(AreEqual((PinToSliceIndexDecoder)JsonConvert.DeserializeObject(pinMap1DiffSize, typeof(PinToSliceIndexDecoder)), pinMap1));
            Assert.IsFalse(AreEqual((PinToSliceIndexDecoder)JsonConvert.DeserializeObject(pinMap1DiffConfig, typeof(PinToSliceIndexDecoder)), pinMap1));
            Assert.IsFalse(AreEqual((PinToSliceIndexDecoder)JsonConvert.DeserializeObject(pinMap1DiffMap, typeof(PinToSliceIndexDecoder)), pinMap1));
            Assert.IsFalse(AreEqual((PinToSliceIndexDecoder)JsonConvert.DeserializeObject(pinMap1DiffPins, typeof(PinToSliceIndexDecoder)), pinMap1));
        }
    }
}
