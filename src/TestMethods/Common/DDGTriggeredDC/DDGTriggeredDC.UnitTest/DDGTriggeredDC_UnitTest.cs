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

namespace DDGTriggeredDC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DcService;
    using Prime.FunctionalService;
    using Prime.SharedStorageService;
    using Prime.TestMethods.TriggeredDc;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class DDGTriggeredDC_UnitTest
    {
        /// <summary>
        /// Setup any common/loose mocks.
        /// </summary>
        [TestInitialize]
        public void SetupCommon()
        {
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) => Console.WriteLine(msg));
            consoleMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) => Console.WriteLine($"ERROR:{msg}"));
            Prime.Services.ConsoleService = consoleMock.Object;
        }

        /// <summary>
        /// Test the full functionality. This is overkill, remove once we can call PrintToDatalog from the extension.
        /// </summary>
        [TestMethod]
        public void Execute_FullRun_Pass()
        {
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.ClearLevelSixItuffHeader("dummy"));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var pinResultsMock = new Mock<IPinDcResults>(MockBehavior.Strict);
            pinResultsMock.Setup(o => o.GetPinName()).Returns("VccIn");
            pinResultsMock.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 1.4 });

            var pinGroupResultsMock = new Mock<IPinGroupDcResults>(MockBehavior.Strict);
            pinGroupResultsMock.Setup(o => o.GetPinGroupName()).Returns(string.Empty);
            pinGroupResultsMock.Setup(o => o.GetAllPinsDcResults()).Returns(new List<IPinDcResults> { pinResultsMock.Object });

            var dcResultsMock = new Mock<IDcResults>(MockBehavior.Strict);
            dcResultsMock.Setup(o => o.GetAllPinGroupsDcResults()).Returns(new List<IPinGroupDcResults> { pinGroupResultsMock.Object });
            dcResultsMock.Setup(o => o.PrintToDatalog(DatalogLevel.FAIL_ONLY, It.IsAny<DcSetup>()));

            var dcTestMock = new Mock<IDcTest>(MockBehavior.Strict);
            dcTestMock.Setup(o => o.Execute()).Returns(dcResultsMock.Object);

            var dcServiceMock = new Mock<IDcService>(MockBehavior.Strict);
            dcServiceMock.Setup(o => o.GetDcTest(new List<string> { "VccIn" }, new List<MeasurementType> { MeasurementType.CURRENT })).Returns(dcTestMock.Object);
            Prime.Services.DcService = dcServiceMock.Object;

            var funcTestMock = new Mock<INoCaptureTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.SetTriggerMap("DummyTriggerMap"));
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.Execute()).Returns(true);

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateNoCaptureTest("DummyPlist", "DummyLevels", "DummyTimings", string.Empty)).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var sharedStorageService = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageService.Setup(o => o.InsertRowAtTable("VccInCurrent1", 1.4, Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageService.Object;

            var underTest = new DDGTriggeredDC
            {
                InstanceName = "dummy",
                Patlist = "DummyPlist",
                TimingsTc = "DummyTimings",
                LevelsTc = "DummyLevels",
                MeasurementTypes = "Current",
                Pins = "VccIn",
                LowLimits = "0A",
                HighLimits = "2.0A",
                TriggerMapName = "DummyTriggerMap",
                SaveResults = "G.U.D.VccInCurrent1",
            };

            underTest.TestMethodExtension = underTest;
            underTest.Verify();
            underTest.CustomVerify();
            Assert.AreEqual(1, underTest.Execute());

            datalogServiceMock.VerifyAll();
            dcServiceMock.VerifyAll();
            dcTestMock.VerifyAll();
            dcResultsMock.VerifyAll();
            pinGroupResultsMock.VerifyAll();
            pinResultsMock.VerifyAll();
            funcServiceMock.VerifyAll();
            funcTestMock.VerifyAll();
            sharedStorageService.VerifyAll();
        }

        /// <summary>
        /// Test the SaveResultsToGsds post-process code.
        /// </summary>
        [TestMethod]
        public void SaveResultsToGsds_SinglePinSingleMeasurement_Pass()
        {
            var pinResultsMock = new Mock<IPinDcResults>(MockBehavior.Strict);
            pinResultsMock.Setup(o => o.GetPinName()).Returns("VccIn");
            pinResultsMock.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 1.4 });

            var pinGroupResultsMock = new Mock<IPinGroupDcResults>(MockBehavior.Strict);
            pinGroupResultsMock.Setup(o => o.GetAllPinsDcResults()).Returns(new List<IPinDcResults> { pinResultsMock.Object });

            var dcResultsMock = new Mock<IDcResults>(MockBehavior.Strict);
            dcResultsMock.Setup(o => o.GetAllPinGroupsDcResults()).Returns(new List<IPinGroupDcResults> { pinGroupResultsMock.Object });
            dcResultsMock.Setup(o => o.PrintToDatalog(DatalogLevel.ALL, It.IsAny<DcSetup>()));

            var sharedStorageService = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageService.Setup(o => o.InsertRowAtTable("VccInCurrent1", 1.4, Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageService.Object;

            var underTest = new DDGTriggeredDC
            {
                Pins = "VccIn",
                SaveResults = "G.U.D.VccInCurrent1",
                DatalogLevel = PrimeTriggeredDcTestMethod.DatalogLevels.All,
            };

            underTest.TestMethodExtension = underTest;
            (underTest as ITriggeredDcExtensions).CustomPostProcessResults(dcResultsMock.Object);
            pinResultsMock.VerifyAll();
            pinGroupResultsMock.VerifyAll();
            dcResultsMock.VerifyAll();
            sharedStorageService.VerifyAll();
        }

        /// <summary>
        /// Test the SaveResultsToGsds post-process code.
        /// </summary>
        [TestMethod]
        public void SaveResultsToGsds_SinglePinMultipleMeasurement_Pass()
        {
            var pinResultsMock = new Mock<IPinDcResults>(MockBehavior.Strict);
            pinResultsMock.Setup(o => o.GetPinName()).Returns("VccIn");
            pinResultsMock.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 1.4, 1.5, 1.6 });

            var pinGroupResultsMock = new Mock<IPinGroupDcResults>(MockBehavior.Strict);
            pinGroupResultsMock.Setup(o => o.GetAllPinsDcResults()).Returns(new List<IPinDcResults> { pinResultsMock.Object });

            var dcResultsMock = new Mock<IDcResults>(MockBehavior.Strict);
            dcResultsMock.Setup(o => o.GetAllPinGroupsDcResults()).Returns(new List<IPinGroupDcResults> { pinGroupResultsMock.Object });
            dcResultsMock.Setup(o => o.PrintToDatalog(DatalogLevel.ALL, It.IsAny<DcSetup>()));

            var sharedStorageService = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageService.Setup(o => o.InsertRowAtTable("VccInCurrent1", 1.4, Context.DUT));
            sharedStorageService.Setup(o => o.InsertRowAtTable("VccInCurrent2", 1.5, Context.DUT));
            sharedStorageService.Setup(o => o.InsertRowAtTable("VccInCurrent3", 1.6, Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageService.Object;

            var underTest = new DDGTriggeredDC
            {
                Pins = "VccIn",
                SaveResults = "G.U.D.VccInCurrent1,G.U.D.VccInCurrent2,G.U.D.VccInCurrent3",
                DatalogLevel = PrimeTriggeredDcTestMethod.DatalogLevels.All,
            };

            underTest.TestMethodExtension = underTest;
            (underTest as ITriggeredDcExtensions).CustomPostProcessResults(dcResultsMock.Object);
            pinResultsMock.VerifyAll();
            pinGroupResultsMock.VerifyAll();
            dcResultsMock.VerifyAll();
            sharedStorageService.VerifyAll();
        }

        /// <summary>
        /// Test the SaveResultsToGsds post-process code.
        /// </summary>
        [TestMethod]
        public void SaveResultsToGsds_MultiplePinMultipleMeasurement_Pass()
        {
            var pinResultsMock1 = new Mock<IPinDcResults>(MockBehavior.Strict);
            pinResultsMock1.Setup(o => o.GetPinName()).Returns("Core1In");
            pinResultsMock1.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 1.4, 1.5, 1.6 });

            var pinResultsMock2 = new Mock<IPinDcResults>(MockBehavior.Strict);
            pinResultsMock2.Setup(o => o.GetPinName()).Returns("Core0In");
            pinResultsMock2.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 2.4, 2.5, 2.6 });

            var pinGroupResultsMock = new Mock<IPinGroupDcResults>(MockBehavior.Strict);
            pinGroupResultsMock.Setup(o => o.GetAllPinsDcResults()).Returns(new List<IPinDcResults> { pinResultsMock1.Object, pinResultsMock2.Object });

            var dcResultsMock = new Mock<IDcResults>(MockBehavior.Strict);
            dcResultsMock.Setup(o => o.GetAllPinGroupsDcResults()).Returns(new List<IPinGroupDcResults> { pinGroupResultsMock.Object });
            dcResultsMock.Setup(o => o.PrintToDatalog(DatalogLevel.ALL, It.IsAny<DcSetup>()));

            var sharedStorageService = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageService.Setup(o => o.InsertRowAtTable("Core1a", 1.4, Context.DUT));
            sharedStorageService.Setup(o => o.InsertRowAtTable("Core1b", 1.5, Context.DUT));
            sharedStorageService.Setup(o => o.InsertRowAtTable("Core1c", 1.6, Context.DUT));
            sharedStorageService.Setup(o => o.InsertRowAtTable("Core0a", "2.4", Context.DUT));
            sharedStorageService.Setup(o => o.InsertRowAtTable("Core0b", "2.5", Context.DUT));
            sharedStorageService.Setup(o => o.InsertRowAtTable("Core0c", "2.6", Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageService.Object;

            var underTest = new DDGTriggeredDC
            {
                Pins = "Core1In,Core0In",
                SaveResults = "G.U.D.Core1a,G.U.D.Core1b,G.U.D.Core1c,G.U.S.Core0a,G.U.S.Core0b,G.U.S.Core0c",
                DatalogLevel = PrimeTriggeredDcTestMethod.DatalogLevels.All,
            };

            underTest.TestMethodExtension = underTest;
            (underTest as ITriggeredDcExtensions).CustomPostProcessResults(dcResultsMock.Object);
            pinResultsMock1.VerifyAll();
            pinResultsMock2.VerifyAll();
            pinGroupResultsMock.VerifyAll();
            dcResultsMock.VerifyAll();
            sharedStorageService.VerifyAll();
        }

        /// <summary>
        /// Test the SaveResultsToGsds post-process code.
        /// </summary>
        [TestMethod]
        public void SaveResultsToGsds_WrongGsdsCount_Fail()
        {
            var pinResultsMock1 = new Mock<IPinDcResults>(MockBehavior.Strict);
            pinResultsMock1.Setup(o => o.GetPinName()).Returns("Core1In");
            pinResultsMock1.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 1.4, 1.5, 1.6 });

            var pinGroupResultsMock = new Mock<IPinGroupDcResults>(MockBehavior.Strict);
            pinGroupResultsMock.Setup(o => o.GetAllPinsDcResults()).Returns(new List<IPinDcResults> { pinResultsMock1.Object });

            var dcResultsMock = new Mock<IDcResults>(MockBehavior.Strict);
            dcResultsMock.Setup(o => o.GetAllPinGroupsDcResults()).Returns(new List<IPinGroupDcResults> { pinGroupResultsMock.Object });
            dcResultsMock.Setup(o => o.PrintToDatalog(DatalogLevel.ALL, It.IsAny<DcSetup>()));

            var underTest = new DDGTriggeredDC
            {
                Pins = "Core1In",
                SaveResults = "G.U.D.Core1a,G.U.D.Core1b,G.U.D.Core1c,G.U.S.Core0a,G.U.S.Core0b,G.U.S.Core0c",
                DatalogLevel = PrimeTriggeredDcTestMethod.DatalogLevels.All,
            };

            underTest.TestMethodExtension = underTest;
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => (underTest as ITriggeredDcExtensions).CustomPostProcessResults(dcResultsMock.Object));
            Assert.AreEqual("6 GSDS tokens supplied, but 3 DC results measured. There must be exactly one GSDS listed for each result.", ex.Message);

            pinResultsMock1.VerifyAll();
            pinGroupResultsMock.VerifyAll();
            dcResultsMock.VerifyAll();
        }

        /// <summary>
        /// Test the SaveResultsToGsds post-process code.
        /// </summary>
        [TestMethod]
        public void SaveResultsToGsds_PinNotFoundInResults_Fail()
        {
            var pinResultsMock1 = new Mock<IPinDcResults>(MockBehavior.Strict);
            pinResultsMock1.Setup(o => o.GetPinName()).Returns("Core0In");
            pinResultsMock1.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 1.4, 1.5, 1.6 });

            var pinGroupResultsMock = new Mock<IPinGroupDcResults>(MockBehavior.Strict);
            pinGroupResultsMock.Setup(o => o.GetAllPinsDcResults()).Returns(new List<IPinDcResults> { pinResultsMock1.Object });

            var dcResultsMock = new Mock<IDcResults>(MockBehavior.Strict);
            dcResultsMock.Setup(o => o.GetAllPinGroupsDcResults()).Returns(new List<IPinGroupDcResults> { pinGroupResultsMock.Object });
            dcResultsMock.Setup(o => o.PrintToDatalog(DatalogLevel.ALL, It.IsAny<DcSetup>()));

            var underTest = new DDGTriggeredDC
            {
                Pins = "Core1In,Core0In",
                SaveResults = "G.U.D.Core1a,G.U.D.Core1b,G.U.D.Core1c",
                DatalogLevel = PrimeTriggeredDcTestMethod.DatalogLevels.All,
            };

            underTest.TestMethodExtension = underTest;
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => (underTest as ITriggeredDcExtensions).CustomPostProcessResults(dcResultsMock.Object));
            Assert.AreEqual("Pin from Pins Parameter=[Core1In] does not have any DC results.", ex.Message);

            pinResultsMock1.VerifyAll();
            pinGroupResultsMock.VerifyAll();
            dcResultsMock.VerifyAll();
        }
    }
}
