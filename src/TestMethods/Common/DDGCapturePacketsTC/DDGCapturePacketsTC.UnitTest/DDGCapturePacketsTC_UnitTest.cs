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

namespace DDGCapturePacketsTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PinService;
    using Prime.PlistService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class DDGCapturePacketsTC_UnitTest
    {
        /// <summary>
        /// Default initialization.
        /// </summary>
        [TestInitialize]
        public void Initialization()
        {
            // Default Mock for ConsoleService
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) => Console.WriteLine(msg));
            consoleMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleMock.Object;
        }

        /// <summary>
        /// Test exception is thrown if no CtvCapturePins parameter is supplied.
        /// </summary>
        [TestMethod]
        public void Verify_InvalidOutputGsds_Fail()
        {
            var underTest = new DDGCapturePacketsTC
            {
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                Patlist = "FakePlist",
                ExecutionMode = DDGCapturePacketsTC.Mode.PER_PIN,
                DataPins = "TDO",
                OutputGsds = "blah",
            };

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.Verify());
            Assert.AreEqual("Parameter=[OutputGsds] must be a gsds of string type. Expected format=[G.[LUI].S.name]", ex.Message);
        }

        /// <summary>
        /// Test exception is thrown if no CtvCapturePins parameter is supplied.
        /// </summary>
        [TestMethod]
        public void Verify_InvalidMaskPin_Fail()
        {
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(o => o.Exists("FakePin")).Returns(false);
            Prime.Services.PinService = pinServiceMock.Object;

            var underTest = new DDGCapturePacketsTC
            {
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                Patlist = "FakePlist",
                ExecutionMode = DDGCapturePacketsTC.Mode.PER_PIN,
                DataPins = "TDO",
                OutputGsds = "G.U.S.SomeToken",
                MaskPins = "FakePin",
            };

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.Verify());
            Assert.AreEqual("Mask pin=[FakePin] does not exist.", ex.Message);
            pinServiceMock.VerifyAll();
        }

        /// <summary>
        /// SimplePin test. Print only.
        /// </summary>
        [TestMethod]
        public void Verify_WithPinMask_Pass()
        {
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            var funcServiceMock = this.MockFunctionalService(funcTestMock.Object, "FakePlist", "FakeLevels", "FakeTimings", "TDO");
            var plistServiceMock = this.MockPlistService(plistMock.Object, "FakePlist");

            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(o => o.Exists("FakePin")).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            var underTest = new DDGCapturePacketsTC
            {
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                Patlist = "FakePlist",
                ExecutionMode = DDGCapturePacketsTC.Mode.PER_PIN,
                DataPins = "TDO",
                OutputGsds = "G.U.S.SomeToken",
                MaskPins = "FakePin",
            };

            underTest.Verify();

            funcTestMock.VerifyAll();
            funcServiceMock.VerifyAll();
            plistMock.VerifyAll();
            plistServiceMock.VerifyAll();
            pinServiceMock.VerifyAll();
        }

        /// <summary>
        /// SimplePin test. Print only.
        /// </summary>
        [TestMethod]
        public void Execute_AmbleFail_Port2()
        {
            var ambleFailureMock = new Mock<IFailureData>(MockBehavior.Strict);
            ambleFailureMock.Setup(o => o.GetPatternName()).Returns("somepattern");

            var ctvData = new Dictionary<string, string> { { "Pin1", "01011" }, { "Pin2", "11100" } };
            var failData = new List<IFailureData> { ambleFailureMock.Object };
            var funcTestMock = this.MockFunctionalTest(false, ctvData, failData);
            funcTestMock.Setup(o => o.DatalogFailure(1));

            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("somepattern")).Returns(true);

            var funcServiceMock = this.MockFunctionalService(funcTestMock.Object, "FakePlist", "FakeLevels", "FakeTimings", "Pin1,Pin2");
            var plistServiceMock = this.MockPlistService(plistMock.Object, "FakePlist");
            var sharedStorageService = this.MockSharedStorageWrite("SomeToken", "0101111100", Context.DUT);

            var underTest = new DDGCapturePacketsTC
            {
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                Patlist = "FakePlist",
                ExecutionMode = DDGCapturePacketsTC.Mode.PER_PIN,
                DataPins = "Pin1, Pin2",
                OutputGsds = "G.U.S.SomeToken",
            };

            underTest.Verify();
            Assert.AreEqual(2, underTest.Execute());

            funcTestMock.VerifyAll();
            funcServiceMock.VerifyAll();
            plistMock.VerifyAll();
            plistServiceMock.VerifyAll();
            sharedStorageService.VerifyAll();
            ambleFailureMock.VerifyAll();
        }

        /// <summary>
        /// SimplePin test. Print only.
        /// </summary>
        [TestMethod]
        public void Execute_CtvAllZero_Port2()
        {
            var ctvData = new Dictionary<string, string> { { "Pin1", "0000" }, { "Pin2", "0000" } };
            var failData = new List<IFailureData> { };
            var funcTestMock = this.MockFunctionalTest(true, ctvData, failData);
            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            var sharedStorageService = this.MockSharedStorageWrite("SomeToken", "00000000", Context.DUT);
            var funcServiceMock = this.MockFunctionalService(funcTestMock.Object, "FakePlist", "FakeLevels", "FakeTimings", "Pin1,Pin2");
            var plistServiceMock = this.MockPlistService(plistMock.Object, "FakePlist");

            var underTest = new DDGCapturePacketsTC
            {
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                Patlist = "FakePlist",
                ExecutionMode = DDGCapturePacketsTC.Mode.PER_PIN,
                DataPins = "Pin1, Pin2",
                OutputGsds = "G.U.S.SomeToken",
            };

            underTest.Verify();
            Assert.AreEqual(2, underTest.Execute());

            funcTestMock.VerifyAll();
            funcServiceMock.VerifyAll();
            plistMock.VerifyAll();
            plistServiceMock.VerifyAll();
            sharedStorageService.VerifyAll();
        }

        /// <summary>
        /// SimplePin test. Print only.
        /// </summary>
        [TestMethod]
        public void Execute_LengthMismatch_Port0()
        {
            var ctvData = new Dictionary<string, string> { { "Pin1", "01011" }, { "Pin2", "11100" } };
            var failData = new List<IFailureData> { };
            var funcTestMock = this.MockFunctionalTest(true, ctvData, failData);
            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            var sharedStorageService = this.MockSharedStorageWrite("SomeToken", string.Empty, Context.DUT);
            var funcServiceMock = this.MockFunctionalService(funcTestMock.Object, "FakePlist", "FakeLevels", "FakeTimings", "Pin1,Pin2");
            var plistServiceMock = this.MockPlistService(plistMock.Object, "FakePlist");

            var underTest = new DDGCapturePacketsTC
            {
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                Patlist = "FakePlist",
                ExecutionMode = DDGCapturePacketsTC.Mode.PER_PIN,
                DataPins = "Pin1, Pin2",
                OutputGsds = "G.U.S.SomeToken",
                TotalSize = 56,
            };

            underTest.Verify();
            Assert.AreEqual(0, underTest.Execute());

            funcTestMock.VerifyAll();
            funcServiceMock.VerifyAll();
            plistMock.VerifyAll();
            plistServiceMock.VerifyAll();
            sharedStorageService.VerifyAll();
        }

        /// <summary>
        /// SimplePin test. Print only.
        /// </summary>
        [TestMethod]
        public void Execute_NoCtvCaptured_Port0()
        {
            var ctvData = new Dictionary<string, string> { { "Pin1", string.Empty }, { "Pin2", string.Empty } };
            var failData = new List<IFailureData> { };
            var funcTestMock = this.MockFunctionalTest(true, ctvData, failData);
            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            var sharedStorageService = this.MockSharedStorageWrite("SomeToken", string.Empty, Context.DUT);
            var funcServiceMock = this.MockFunctionalService(funcTestMock.Object, "FakePlist", "FakeLevels", "FakeTimings", "Pin1,Pin2");
            var plistServiceMock = this.MockPlistService(plistMock.Object, "FakePlist");

            var underTest = new DDGCapturePacketsTC
            {
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                Patlist = "FakePlist",
                ExecutionMode = DDGCapturePacketsTC.Mode.PER_PIN,
                DataPins = "Pin1, Pin2",
                OutputGsds = "G.U.S.SomeToken",
                TotalSize = 56,
            };

            underTest.Verify();
            Assert.AreEqual(0, underTest.Execute());

            funcTestMock.VerifyAll();
            funcServiceMock.VerifyAll();
            plistMock.VerifyAll();
            plistServiceMock.VerifyAll();
            sharedStorageService.VerifyAll();
        }

        /// <summary>
        /// SimplePin test. Print only.
        /// </summary>
        [TestMethod]
        public void Execute_Pass_Port1()
        {
            var ctvData = new Dictionary<string, string> { { "Pin1", "01011" }, { "Pin2", "11100" } };
            var failData = new List<IFailureData> { };
            var funcTestMock = this.MockFunctionalTest(true, ctvData, failData);
            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            var sharedStorageService = this.MockSharedStorageWrite("SomeToken", "0101111100", Context.DUT);
            var funcServiceMock = this.MockFunctionalService(funcTestMock.Object, "FakePlist", "FakeLevels", "FakeTimings", "Pin1,Pin2");
            var plistServiceMock = this.MockPlistService(plistMock.Object, "FakePlist");

            var underTest = new DDGCapturePacketsTC
            {
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                Patlist = "FakePlist",
                ExecutionMode = DDGCapturePacketsTC.Mode.PER_PIN,
                DataPins = "Pin1, Pin2",
                OutputGsds = "G.U.S.SomeToken",
            };

            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            funcTestMock.VerifyAll();
            funcServiceMock.VerifyAll();
            plistMock.VerifyAll();
            plistServiceMock.VerifyAll();
            sharedStorageService.VerifyAll();
        }

        /// <summary>
        /// SimplePin test. Print only.
        /// </summary>
        [TestMethod]
        public void Execute_TestFailedButGoodCtv_Port1()
        {
            var notAmbleFailureMock = new Mock<IFailureData>(MockBehavior.Strict);
            notAmbleFailureMock.Setup(o => o.GetPatternName()).Returns("somepattern");

            var ctvData = new Dictionary<string, string> { { "Pin1", "01011" }, { "Pin2", "11100" } };
            var failData = new List<IFailureData> { notAmbleFailureMock.Object };
            var funcTestMock = this.MockFunctionalTest(false, ctvData, failData);
            funcTestMock.Setup(o => o.DatalogFailure(1));
            var plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistMock.Setup(o => o.IsPatternAnAmble("somepattern")).Returns(false);

            var funcServiceMock = this.MockFunctionalService(funcTestMock.Object, "FakePlist", "FakeLevels", "FakeTimings", "Pin1,Pin2");
            var plistServiceMock = this.MockPlistService(plistMock.Object, "FakePlist");
            var sharedStorageService = this.MockSharedStorageWrite("SomeToken", "0101111100", Context.DUT);

            var underTest = new DDGCapturePacketsTC
            {
                TimingsTc = "FakeTimings",
                LevelsTc = "FakeLevels",
                Patlist = "FakePlist",
                ExecutionMode = DDGCapturePacketsTC.Mode.PER_PIN,
                DataPins = "Pin1, Pin2",
                OutputGsds = "G.U.S.SomeToken",
            };

            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            funcTestMock.VerifyAll();
            funcServiceMock.VerifyAll();
            plistMock.VerifyAll();
            plistServiceMock.VerifyAll();
            sharedStorageService.VerifyAll();
            notAmbleFailureMock.VerifyAll();
        }

        private Mock<ISharedStorageService> MockSharedStorageWrite(string token, string value, Context context)
        {
            var sharedStorageService = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageService.Setup(o => o.InsertRowAtTable(token, value, context));
            Prime.Services.SharedStorageService = sharedStorageService.Object;
            return sharedStorageService;
        }

        private Mock<ICaptureFailureAndCtvPerPinTest> MockFunctionalTest(bool testResult, Dictionary<string, string> ctvData, List<IFailureData> failData)
        {
            var funcTestMock = new Mock<ICaptureFailureAndCtvPerPinTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.SetPinMask(new List<string> { }));
            funcTestMock.Setup(o => o.Execute()).Returns(testResult);
            funcTestMock.Setup(o => o.GetCtvData()).Returns(ctvData);
            if (!testResult)
            {
                funcTestMock.Setup(o => o.GetPerCycleFailures()).Returns(failData);
            }

            return funcTestMock;
        }

        private Mock<IFunctionalService> MockFunctionalService(ICaptureFailureAndCtvPerPinTest test, string plist, string levels, string timings, string ctvPins)
        {
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureFailureAndCtvPerPinTest(plist, levels, timings, It.Is<List<string>>(it => string.Join(",", it) == ctvPins), 1, 1, string.Empty))
                .Returns(test);
            Prime.Services.FunctionalService = funcServiceMock.Object;
            return funcServiceMock;
        }

        private Mock<IPlistService> MockPlistService(IPlistObject obj, string plist)
        {
            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            plistServiceMock.Setup(o => o.GetPlistObject(plist)).Returns(obj);
            Prime.Services.PlistService = plistServiceMock.Object;
            return plistServiceMock;
        }
    }
}