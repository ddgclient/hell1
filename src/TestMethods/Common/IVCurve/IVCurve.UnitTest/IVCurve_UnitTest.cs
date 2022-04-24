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

namespace IVCurve.UnitTest
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.DcService;
    using Prime.PinService;
    using Prime.SharedStorageService;
    using Prime.TestConditionService;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class IVCurve_UnitTest
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IDcTest> dcTest;
        private Mock<IDcResults> dcResults;
        private Mock<IPinGroupDcResults> pinGroupDcResults;
        private Mock<IPinDcResults> pinDcResults;
        private Mock<IDcService> dcServiceMock;
        private Mock<ISharedStorageService> sharedStorageService;
        private Mock<IPinService> pinServiceMock;
        private Mock<IPin> pinMock;
        private Mock<IDatalogService> datalogServiceMock;
        private Mock<IMrsltFormat> datalogWritereMock;
        private Mock<ITestConditionService> testConditionServiceMock;
        private Mock<ITestCondition> testConditionMock;

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(
                    o => o.PrintDebug(It.IsAny<string>())).
                Callback((string msg) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            this.consoleServiceMock.Setup(
                    o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string member, string path) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;
            this.pinDcResults = new Mock<IPinDcResults>(MockBehavior.Strict);
            this.pinGroupDcResults = new Mock<IPinGroupDcResults>(MockBehavior.Strict);
            this.pinGroupDcResults.Setup(o => o.GetAllPinsDcResults()).Returns(new List<IPinDcResults> { this.pinDcResults.Object });
            this.dcResults = new Mock<IDcResults>(MockBehavior.Strict);
            this.dcResults.Setup(o => o.GetAllPinGroupsDcResults()).Returns(new List<IPinGroupDcResults> { this.pinGroupDcResults.Object });
            this.dcResults.Setup(o => o.PrintToDatalog(It.IsAny<DatalogLevel>(), It.IsAny<DcSetup>()));
            this.dcTest = new Mock<IDcTest>(MockBehavior.Strict);
            this.dcTest.Setup(o => o.Execute()).Returns(this.dcResults.Object);
            this.dcServiceMock = new Mock<IDcService>(MockBehavior.Strict);
            this.dcServiceMock.Setup(o => o.GetDcTest(It.IsAny<List<string>>(), It.IsAny<List<MeasurementType>>())).Returns(this.dcTest.Object);
            Prime.Services.DcService = this.dcServiceMock.Object;
            this.sharedStorageService = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = this.sharedStorageService.Object;
            this.pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            Prime.Services.PinService = this.pinServiceMock.Object;
            this.datalogWritereMock = new Mock<IMrsltFormat>(MockBehavior.Strict);
            this.datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            this.datalogServiceMock.Setup(o => o.GetItuffMrsltWriter()).Returns(this.datalogWritereMock.Object);
            Prime.Services.DatalogService = this.datalogServiceMock.Object;
            this.testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            Prime.Services.TestConditionService = this.testConditionServiceMock.Object;
            this.testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            this.testConditionMock.Setup(o => o.ForceApply());
            this.testConditionServiceMock.Setup(o => o.GetTestCondition(It.IsAny<string>())).Returns(this.testConditionMock.Object);
            this.pinMock = new Mock<IPin>(MockBehavior.Strict);
        }

        /// <summary>
        /// ProductionMode_SinglePin_Pass.
        /// </summary>
        [TestMethod]
        public void ProductionMode_SinglePin_Pass()
        {
            this.pinDcResults.Setup(o => o.GetPinName()).Returns("Pin1");
            this.pinDcResults.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 0.5 });
            this.sharedStorageService.Setup(o => o.InsertRowAtTable("Token", "0.5", Context.DUT));
            this.pinServiceMock.Setup(o => o.Get("Pin1")).Returns(this.pinMock.Object);
            this.pinMock.Setup(o => o.GetResourceType()).Returns("HC");
            var restoreValues = new Dictionary<string, string>
            {
                { "IRange", "19A" },
                { "PreMeasurementDelay", "0.001" },
                { "StartMeasurement", "False" },
                { "SamplingRatio", "1" },
                { "SamplingMode", "Average" },
                { "SamplingCount", "1" },
                { "VForce", "0" },
                { "IClampHi", "19" },
                { "IClampLo", "0" },
                { "FreeDriveTime", "0.001" },
                { "VSlewStepRatio", "120" },
                { "OPModeCheck", "VSIM" },
            };

            this.testConditionMock.Setup(o => o.GetPinAttributeValue("Pin1", It.IsAny<string>())).Returns((string pin, string key) => restoreValues[key]);
            this.pinServiceMock.Setup(o => o.GetPinAttributeValues("Pin1", It.IsAny<List<string>>())).Returns(restoreValues);
            this.pinServiceMock.Setup(o => o.SetPinAttributeValues("Pin1", It.IsAny<Dictionary<string, string>>()));
            var underTest = new IVCurve
            {
                InstanceName = "InstanceName",
                Mode = IVCurve.Modes.Production,
                Pins = "Pin1",
                LevelsTc = "SomeLevels",
                LowLimits = "0.1A",
                HighLimits = "0.8A",
                ForceSetPoint = "1",
                IRange = "19A",
                IClampHi = "19",
                IClampLo = "0",
                FreeDriveTime = "0.001",
                SamplingRatio = "1",
                SamplingCount = "1",
                PreMeasurementDelay = "0.001",
                Type = "Current",
                SharedStorageTokens = "Token",
                VSlewStepRatio = "1",
            };
            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(1, executeResult);
            this.dcResults.VerifyAll();
            this.dcTest.VerifyAll();
            this.pinDcResults.VerifyAll();
            this.pinGroupDcResults.VerifyAll();
            this.sharedStorageService.VerifyAll();
            this.testConditionServiceMock.VerifyAll();
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ProductionMode_AlarmMode_P2()
        {
            this.pinServiceMock.Setup(o => o.Get("Pin1")).Returns(this.pinMock.Object);
            this.pinMock.Setup(o => o.GetResourceType()).Returns("HC");
            var restoreValues = new Dictionary<string, string>
            {
                { "IRange", "19A" },
                { "PreMeasurementDelay", "0.001" },
                { "StartMeasurement", "False" },
                { "SamplingRatio", "1" },
                { "SamplingMode", "Average" },
                { "SamplingCount", "1" },
                { "VForce", "0" },
                { "IClampHi", "19" },
                { "IClampLo", "0" },
                { "FreeDriveTime", "0.001" },
                { "VSlewStepRatio", "120" },
                { "OPModeCheck", "VSIM" },
            };

            this.testConditionMock.Setup(o => o.GetPinAttributeValue("Pin1", It.IsAny<string>())).Returns((string pin, string key) => restoreValues[key]);
            this.pinServiceMock.Setup(o => o.GetPinAttributeValues("Pin1", It.IsAny<List<string>>())).Returns(restoreValues);
            this.pinServiceMock.Setup(o => o.SetPinAttributeValues("Pin1", It.IsAny<Dictionary<string, string>>()))
                .Throws(new AlarmException("Fl", "Fn", 1, "An alarm has occurred.", new List<AlarmException.AlarmInfo>()));
            var underTest = new IVCurve
            {
                InstanceName = "InstanceName",
                Mode = IVCurve.Modes.Production,
                Pins = "Pin1",
                LevelsTc = "SomeLevels",
                LowLimits = "0.1A",
                HighLimits = "0.8A",
                ForceSetPoint = "1",
                IRange = "19A",
                IClampHi = "19",
                IClampLo = "0",
                FreeDriveTime = "0.001",
                SamplingRatio = "1",
                SamplingCount = "1",
                PreMeasurementDelay = "0.001",
                Type = "Current",
                SharedStorageTokens = "Token",
                VSlewStepRatio = "1",
                AlarmMode = IVCurve.AlarmModes.Enabled,
            };
            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(2, executeResult);
            this.sharedStorageService.VerifyAll();
            this.testConditionServiceMock.VerifyAll();
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// CharacterizationMode_SinglePin_Pass.
        /// </summary>
        [TestMethod]
        public void CharacterizationMode_SinglePin_Pass()
        {
            this.pinDcResults.Setup(o => o.GetPinName()).Returns("Pin1");
            this.pinDcResults.SetupSequence(o => o.GetPinDcResults())
                .Returns(new List<double> { 0.5 }) // production
                .Returns(new List<double> { 0.2 }) // characterization
                .Returns(new List<double> { 0.3 })
                .Returns(new List<double> { 0.4 })
                .Returns(new List<double> { 0.5 })
                .Returns(new List<double> { 0.6 });
            this.sharedStorageService.Setup(o => o.InsertRowAtTable("Token", "0.5", Context.DUT));
            this.pinServiceMock.Setup(o => o.Get("Pin1")).Returns(this.pinMock.Object);
            this.pinMock.Setup(o => o.GetResourceType()).Returns("HC");
            var restoreValues = new Dictionary<string, string>
            {
                { "IRange", "19A" },
                { "PreMeasurementDelay", "0.001" },
                { "StartMeasurement", "False" },
                { "SamplingRatio", "1" },
                { "SamplingMode", "Average" },
                { "SamplingCount", "1" },
                { "VForce", "0" },
                { "IClampHi", "19" },
                { "IClampLo", "0" },
                { "FreeDriveTime", "0.001" },
                { "VSlewStepRatio", "120" },
                { "OPModeCheck", "VSIM" },
            };

            this.testConditionMock.Setup(o => o.GetPinAttributeValue("Pin1", It.IsAny<string>())).Returns((string pin, string key) => restoreValues[key]);
            this.pinServiceMock.Setup(o => o.GetPinAttributeValues("Pin1", It.IsAny<List<string>>())).Returns(restoreValues);
            this.pinServiceMock.Setup(o => o.SetPinAttributeValues("Pin1", It.IsAny<Dictionary<string, string>>()));
            this.datalogWritereMock.Setup(o => o.SetPrecision(9));
            this.datalogWritereMock.Setup(o => o.SetTnamePostfix("_Pin1_0"));
            this.datalogWritereMock.Setup(o => o.SetTnamePostfix("_Pin1_1"));
            this.datalogWritereMock.Setup(o => o.SetTnamePostfix("_Pin1_2"));
            this.datalogWritereMock.Setup(o => o.SetTnamePostfix("_Pin1_3"));
            this.datalogWritereMock.Setup(o => o.SetTnamePostfix("_Pin1_4"));
            this.datalogWritereMock.Setup(o => o.SetData(0.2));
            this.datalogWritereMock.Setup(o => o.SetData(0.3));
            this.datalogWritereMock.Setup(o => o.SetData(0.4));
            this.datalogWritereMock.Setup(o => o.SetData(0.5));
            this.datalogWritereMock.Setup(o => o.SetData(0.6));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.datalogWritereMock.Object));

            var comntWriterMock = new Mock<IComntFormat>(MockBehavior.Strict);
            comntWriterMock.Setup(o => o.IncludeTnameInPrint(false));
            comntWriterMock.Setup(o => o.SetData("fvalue_0.100000000"));
            comntWriterMock.Setup(o => o.SetData("fvalue_0.300000000"));
            comntWriterMock.Setup(o => o.SetData("fvalue_0.500000000"));
            comntWriterMock.Setup(o => o.SetData("fvalue_0.700000000"));
            comntWriterMock.Setup(o => o.SetData("fvalue_0.900000000"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(comntWriterMock.Object));
            this.datalogServiceMock.Setup(o => o.GetItuffComntWriter()).Returns(comntWriterMock.Object);

            var underTest = new IVCurve
            {
                InstanceName = "InstanceName",
                Mode = IVCurve.Modes.Characterization,
                Pins = "Pin1",
                LevelsTc = "SomeLevels",
                LowLimits = "0.1A",
                HighLimits = "0.8A",
                ForceSetPoint = "1",
                ForceStartValue = "0.1",
                ForceStopValue = "1",
                ForceStepSize = "0.2",
                IRange = "19A",
                IClampHi = "19",
                IClampLo = "0",
                FreeDriveTime = "0.001",
                SamplingRatio = "1",
                SamplingCount = "1",
                PreMeasurementDelay = "0.001",
                Type = "Current",
                SharedStorageTokens = "Token",
                VSlewStepRatio = "1",
            };
            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(1, executeResult);
            this.dcResults.VerifyAll();
            this.dcTest.VerifyAll();
            this.pinDcResults.VerifyAll();
            this.pinGroupDcResults.VerifyAll();
            this.datalogServiceMock.VerifyAll();
            this.datalogWritereMock.VerifyAll();
            comntWriterMock.VerifyAll();
            this.sharedStorageService.VerifyAll();
            this.testConditionServiceMock.VerifyAll();
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// ProductionMode_VlcPin_Pass.
        /// </summary>
        [TestMethod]
        public void ProductionMode_VlcPin_Pass()
        {
            this.dcResults.Setup(o => o.PrintToConsole());
            this.pinDcResults.Setup(o => o.GetPinName()).Returns("Pin1");
            this.pinDcResults.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 0.5 });
            this.sharedStorageService.Setup(o => o.InsertRowAtTable("Token", "0.5", Context.DUT));
            this.pinServiceMock.Setup(o => o.Get("Pin1")).Returns(this.pinMock.Object);
            this.pinMock.Setup(o => o.GetResourceType()).Returns("VLC");
            var restoreValues = new Dictionary<string, string>
            {
                { "IRange", "19A" },
                { "PreMeasurementDelay", "0.001" },
                { "StartMeasurement", "False" },
                { "SamplingRatio", "1" },
                { "SamplingMode", "Average" },
                { "SamplingCount", "1" },
                { "VForce", "0" },
                { "IClampHi", "19" },
                { "IClampLo", "0" },
                { "FreeDriveTime", "0.001" },
                { "FreeDriveCurrentHi", "0.001" },
                { "FreeDriveCurrentLo", "0.001" },
                { "OPModeCheck", "VSIM" },
            };

            this.testConditionMock.Setup(o => o.GetPinAttributeValue("Pin1", It.IsAny<string>())).Returns((string pin, string key) => restoreValues[key]);
            this.pinServiceMock.Setup(o => o.GetPinAttributeValues("Pin1", It.IsAny<List<string>>())).Returns(restoreValues);
            this.pinServiceMock.Setup(o => o.SetPinAttributeValues("Pin1", It.IsAny<Dictionary<string, string>>()));
            var underTest = new IVCurve
            {
                InstanceName = "InstanceName",
                Mode = IVCurve.Modes.Production,
                Pins = "Pin1",
                LevelsTc = "SomeLevels",
                LowLimits = "0.1A",
                HighLimits = "0.8A",
                ForceSetPoint = "1",
                IRange = "19A",
                IClampHi = "19",
                IClampLo = "0",
                FreeDriveTime = "0.001",
                SamplingRatio = "1",
                SamplingCount = "1",
                PreMeasurementDelay = "0.001",
                FreeDriveCurrentHi = "0.0001",
                FreeDriveCurrentLo = "0.00001",
                Type = "Current",
                SharedStorageTokens = "Token",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };
            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(1, executeResult);
            this.dcResults.VerifyAll();
            this.dcTest.VerifyAll();
            this.pinDcResults.VerifyAll();
            this.pinGroupDcResults.VerifyAll();
            this.sharedStorageService.VerifyAll();
            this.testConditionServiceMock.VerifyAll();
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// ProductionMode_HvPin_Pass.
        /// </summary>
        [TestMethod]
        public void ProductionMode_HvPin_Pass()
        {
            this.dcResults.Setup(o => o.PrintToConsole());
            this.pinDcResults.Setup(o => o.GetPinName()).Returns("Pin1");
            this.pinDcResults.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 0.5 });
            this.sharedStorageService.Setup(o => o.InsertRowAtTable("Token", "0.5", Context.DUT));
            this.pinServiceMock.Setup(o => o.Get("Pin1")).Returns(this.pinMock.Object);
            this.pinMock.Setup(o => o.GetResourceType()).Returns("HV");
            var restoreValues = new Dictionary<string, string>
            {
                { "IRange", "19A" },
                { "VRange", "1.8V" },
                { "PreMeasurementDelay", "0.001" },
                { "StartMeasurement", "False" },
                { "SamplingRatio", "1" },
                { "SamplingMode", "Average" },
                { "SamplingCount", "1" },
                { "VForce", "0" },
                { "IClampHi", "19" },
                { "IClampLo", "0" },
                { "FreeDriveTime", "0.001" },
                { "VSlewStepRatio", "1" },
                { "OPModeCheck", "VSIM" },
            };

            this.testConditionMock.Setup(o => o.GetPinAttributeValue("Pin1", It.IsAny<string>())).Returns((string pin, string key) => restoreValues[key]);
            this.pinServiceMock.Setup(o => o.GetPinAttributeValues("Pin1", It.IsAny<List<string>>())).Returns(restoreValues);
            this.pinServiceMock.Setup(o => o.SetPinAttributeValues("Pin1", It.IsAny<Dictionary<string, string>>()));
            var underTest = new IVCurve
            {
                InstanceName = "InstanceName",
                Mode = IVCurve.Modes.Production,
                Pins = "Pin1",
                LevelsTc = "SomeLevels",
                LowLimits = "0.1A",
                HighLimits = "0.8A",
                ForceSetPoint = "1",
                IRange = "19A",
                VRange = "1.8V",
                IClampHi = "19",
                IClampLo = "0",
                FreeDriveTime = "0.001",
                SamplingRatio = "1",
                SamplingCount = "1",
                PreMeasurementDelay = "0.001",
                VSlewStepRatio = "1",
                Type = "Current",
                SharedStorageTokens = "Token",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };
            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(1, executeResult);
            this.dcResults.VerifyAll();
            this.dcTest.VerifyAll();
            this.pinDcResults.VerifyAll();
            this.pinGroupDcResults.VerifyAll();
            this.sharedStorageService.VerifyAll();
            this.testConditionServiceMock.VerifyAll();
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// ProductionMode_VlcISVM_Pass.
        /// </summary>
        [TestMethod]
        public void ProductionMode_VlcISVM_Pass()
        {
            this.dcResults.Setup(o => o.PrintToConsole());
            this.pinDcResults.Setup(o => o.GetPinName()).Returns("Pin1");
            this.pinDcResults.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 0.5 });
            this.sharedStorageService.Setup(o => o.InsertRowAtTable("Token", "0.5", Context.DUT));
            this.pinServiceMock.Setup(o => o.Get("Pin1")).Returns(this.pinMock.Object);
            this.pinMock.Setup(o => o.GetResourceType()).Returns("VLC");
            var restoreValues = new Dictionary<string, string>
            {
                { "IRange", "19A" },
                { "PreMeasurementDelay", "0.001" },
                { "StartMeasurement", "False" },
                { "SamplingRatio", "1" },
                { "SamplingMode", "Average" },
                { "SamplingCount", "1" },
                { "IForce", "0" },
                { "VClamp", "1.8" },
                { "FreeDriveTime", "0.001" },
                { "FreeDriveCurrentHi", "0.001" },
                { "FreeDriveCurrentLo", "0.001" },
                { "OPModeCheck", "ISVM" },
            };

            this.testConditionMock.Setup(o => o.GetPinAttributeValue("Pin1", It.IsAny<string>())).Returns((string pin, string key) => restoreValues[key]);
            this.pinServiceMock.Setup(o => o.GetPinAttributeValues("Pin1", It.IsAny<List<string>>())).Returns(restoreValues);
            this.pinServiceMock.Setup(o => o.SetPinAttributeValues("Pin1", It.IsAny<Dictionary<string, string>>()));
            var underTest = new IVCurve
            {
                InstanceName = "InstanceName",
                Mode = IVCurve.Modes.Production,
                Pins = "Pin1",
                LevelsTc = "SomeLevels",
                LowLimits = "0.1V",
                HighLimits = "0.8V",
                ForceSetPoint = "1",
                IRange = "19A",
                VClamp = "1.8",
                SamplingRatio = "1",
                SamplingCount = "1",
                PreMeasurementDelay = "0.001",
                FreeDriveCurrentHi = "0.0001",
                FreeDriveCurrentLo = "0.00001",
                Type = "Voltage",
                SharedStorageTokens = "Token",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };
            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(1, executeResult);
            this.dcResults.VerifyAll();
            this.dcTest.VerifyAll();
            this.pinDcResults.VerifyAll();
            this.pinGroupDcResults.VerifyAll();
            this.sharedStorageService.VerifyAll();
            this.testConditionServiceMock.VerifyAll();
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// ProductionMode_VlcISVM_Pass.
        /// </summary>
        [TestMethod]
        public void ProductionMode_ISVM_Pass()
        {
            this.dcResults.Setup(o => o.PrintToConsole());
            this.pinDcResults.Setup(o => o.GetPinName()).Returns("Pin1");
            this.pinDcResults.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 0.5 });
            this.sharedStorageService.Setup(o => o.InsertRowAtTable("Token", "0.5", Context.DUT));
            this.pinServiceMock.Setup(o => o.Get("Pin1")).Returns(this.pinMock.Object);
            this.pinMock.Setup(o => o.GetResourceType()).Returns("HC");
            var restoreValues = new Dictionary<string, string>
            {
                { "IRange", "19A" },
                { "PreMeasurementDelay", "0.001" },
                { "StartMeasurement", "False" },
                { "SamplingRatio", "1" },
                { "SamplingMode", "Average" },
                { "SamplingCount", "1" },
                { "IForce", "0" },
                { "IClampHi", "19" },
                { "IClampLo", "0" },
                { "FreeDriveTime", "0.001" },
                { "OverVoltageLimit", "0.001" },
                { "UnderVoltageLimit", "0.001" },
                { "VSlewStepRatio", "120" },
                { "OPModeCheck", "ISVM" },
            };

            this.testConditionMock.Setup(o => o.GetPinAttributeValue("Pin1", It.IsAny<string>())).Returns((string pin, string key) => restoreValues[key]);
            this.pinServiceMock.Setup(o => o.GetPinAttributeValues("Pin1", It.IsAny<List<string>>())).Returns(restoreValues);
            this.pinServiceMock.Setup(o => o.SetPinAttributeValues("Pin1", It.IsAny<Dictionary<string, string>>()));
            var underTest = new IVCurve
            {
                InstanceName = "InstanceName",
                Mode = IVCurve.Modes.Production,
                Pins = "Pin1",
                LevelsTc = "SomeLevels",
                LowLimits = "0.1V",
                HighLimits = "0.8V",
                ForceSetPoint = "1",
                IRange = "19A",
                SamplingRatio = "1",
                SamplingCount = "1",
                PreMeasurementDelay = "0.001",
                OverVoltageLimit = "0.0001",
                UnderVoltageLimit = "0.00001",
                Type = "Voltage",
                SharedStorageTokens = "Token",
                LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };
            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(1, executeResult);
            this.dcResults.VerifyAll();
            this.dcTest.VerifyAll();
            this.pinDcResults.VerifyAll();
            this.pinGroupDcResults.VerifyAll();
            this.sharedStorageService.VerifyAll();
            this.testConditionServiceMock.VerifyAll();
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// ProductionMode_MultiplePins_Pass.
        /// </summary>
        [TestMethod]
        public void ProductionMode_MultiplePins_Pass()
        {
            this.pinDcResults.SetupSequence(o => o.GetPinName())
                .Returns("Pin1")
                .Returns("Pin2");
            this.pinDcResults.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 0.5 });
            this.sharedStorageService.Setup(o => o.InsertRowAtTable("Token1", "0.5", Context.DUT));
            this.sharedStorageService.Setup(o => o.InsertRowAtTable("Token2", "0.5", Context.DUT));
            this.pinServiceMock.Setup(o => o.Get("Pin1")).Returns(this.pinMock.Object);
            this.pinMock.Setup(o => o.GetResourceType()).Returns("HC");
            this.pinServiceMock.Setup(o => o.Get("Pin2")).Returns(this.pinMock.Object);
            this.pinMock.Setup(o => o.GetResourceType()).Returns("VLC");
            var restoreValues = new Dictionary<string, string>
            {
                { "IRange", "19A" },
                { "PreMeasurementDelay", "0.001" },
                { "StartMeasurement", "False" },
                { "SamplingRatio", "1" },
                { "SamplingMode", "Average" },
                { "SamplingCount", "1" },
                { "VForce", "0" },
                { "IClampHi", "19" },
                { "IClampLo", "0" },
                { "FreeDriveTime", "0.001" },
                { "FreeDriveCurrentHi", "0.001" },
                { "FreeDriveCurrentLo", "0.000" },
                { "VSlewStepRatio", "120" },
                { "OPModeCheck", "VSIM" },
            };

            this.testConditionMock.Setup(o => o.GetPinAttributeValue("Pin1", It.IsAny<string>())).Returns((string pin, string key) => restoreValues[key]);
            this.testConditionMock.Setup(o => o.GetPinAttributeValue("Pin2", It.IsAny<string>())).Returns((string pin, string key) => restoreValues[key]);
            this.pinServiceMock.Setup(o => o.GetPinAttributeValues("Pin1", It.IsAny<List<string>>())).Returns(restoreValues);
            this.pinServiceMock.Setup(o => o.GetPinAttributeValues("Pin2", It.IsAny<List<string>>())).Returns(restoreValues);
            this.pinServiceMock.Setup(o => o.SetPinAttributeValues("Pin1", It.IsAny<Dictionary<string, string>>()));
            this.pinServiceMock.Setup(o => o.SetPinAttributeValues("Pin2", It.IsAny<Dictionary<string, string>>()));
            var underTest = new IVCurve
            {
                InstanceName = "InstanceName",
                Mode = IVCurve.Modes.Production,
                Pins = "Pin1,Pin2",
                LevelsTc = "SomeLevels",
                LowLimits = "0.1A,0.2A",
                HighLimits = "0.8A,0.9A",
                ForceSetPoint = "1,0.9",
                IRange = "19A,19A",
                IClampHi = "19,19",
                IClampLo = "0,0",
                FreeDriveTime = "0.001,0.002",
                SamplingRatio = "1,1",
                SamplingCount = "1,1",
                PreMeasurementDelay = "0.001,0.002",
                Type = "Current",
                SharedStorageTokens = "Token1,Token2",
                VSlewStepRatio = "1,1",
                FreeDriveCurrentHi = "-99,1",
                FreeDriveCurrentLo = "-99,1",
            };
            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(1, executeResult);
            this.dcResults.VerifyAll();
            this.dcTest.VerifyAll();
            this.pinDcResults.VerifyAll();
            this.pinGroupDcResults.VerifyAll();
            this.sharedStorageService.VerifyAll();
            this.testConditionServiceMock.VerifyAll();
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// ProductionMode_MultiplePinsFailing_Port2.
        /// </summary>
        [TestMethod]
        public void ProductionMode_MultiplePinsFailing_Port3()
        {
            this.pinDcResults.SetupSequence(o => o.GetPinName())
                .Returns("Pin1")
                .Returns("Pin2");
            this.pinDcResults.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 0.5 });
            this.pinServiceMock.Setup(o => o.Get("Pin1")).Returns(this.pinMock.Object);
            this.pinMock.Setup(o => o.GetResourceType()).Returns("HC");
            this.pinServiceMock.Setup(o => o.Get("Pin2")).Returns(this.pinMock.Object);
            this.pinMock.Setup(o => o.GetResourceType()).Returns("LC");
            var restoreValues = new Dictionary<string, string>
            {
                { "IRange", "19A" },
                { "PreMeasurementDelay", "0.001" },
                { "StartMeasurement", "False" },
                { "SamplingRatio", "1" },
                { "SamplingMode", "Average" },
                { "SamplingCount", "1" },
                { "VForce", "0" },
                { "IClampHi", "19" },
                { "IClampLo", "0" },
                { "FreeDriveTime", "0.001" },
                { "VSlewStepRatio", "120" },
                { "OPModeCheck", "VSIM" },
            };

            this.testConditionMock.Setup(o => o.GetPinAttributeValue("Pin1", It.IsAny<string>())).Returns((string pin, string key) => restoreValues[key]);
            this.testConditionMock.Setup(o => o.GetPinAttributeValue("Pin2", It.IsAny<string>())).Returns((string pin, string key) => restoreValues[key]);
            this.pinServiceMock.Setup(o => o.GetPinAttributeValues("Pin1", It.IsAny<List<string>>())).Returns(restoreValues);
            this.pinServiceMock.Setup(o => o.GetPinAttributeValues("Pin2", It.IsAny<List<string>>())).Returns(restoreValues);
            this.pinServiceMock.Setup(o => o.SetPinAttributeValues("Pin1", It.IsAny<Dictionary<string, string>>()));
            this.pinServiceMock.Setup(o => o.SetPinAttributeValues("Pin2", It.IsAny<Dictionary<string, string>>()));
            var underTest = new IVCurve
            {
                InstanceName = "InstanceName",
                Mode = IVCurve.Modes.Production,
                Pins = "Pin1,Pin2",
                LevelsTc = "SomeLevels",
                LowLimits = "0.1A,0.01A",
                HighLimits = "0.8A,0.1A",
                ForceSetPoint = "1,0.9",
                IRange = "19A,19A",
                IClampHi = "19,19",
                IClampLo = "0,0",
                FreeDriveTime = "0.001,0.002",
                SamplingRatio = "1,1",
                SamplingCount = "1,1",
                PreMeasurementDelay = "0.001,0.002",
                Type = "Current",
                DatalogLevel = IVCurve.DatalogLevels.All,
                VSlewStepRatio = "1,1",
            };
            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(4, executeResult);
            this.dcResults.VerifyAll();
            this.dcTest.VerifyAll();
            this.pinDcResults.VerifyAll();
            this.pinGroupDcResults.VerifyAll();
            this.sharedStorageService.VerifyAll();
            this.testConditionServiceMock.VerifyAll();
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// ProductionMode_FailingMoreThanOnePin_Port0.
        /// </summary>
        [TestMethod]
        public void ProductionMode_FailingMoreThanOnePin_Port0()
        {
            this.pinDcResults.SetupSequence(o => o.GetPinName())
                .Returns("Pin1")
                .Returns("Pin2");
            this.pinDcResults.Setup(o => o.GetPinDcResults()).Returns(new List<double> { 0.5 });
            this.sharedStorageService.Setup(o => o.InsertRowAtTable("Token1", "0.5", Context.DUT));
            this.sharedStorageService.Setup(o => o.InsertRowAtTable("Token2", "0.5", Context.DUT));
            this.pinServiceMock.Setup(o => o.Get("Pin1")).Returns(this.pinMock.Object);
            this.pinMock.Setup(o => o.GetResourceType()).Returns("HC");
            this.pinServiceMock.Setup(o => o.Get("Pin2")).Returns(this.pinMock.Object);
            this.pinMock.Setup(o => o.GetResourceType()).Returns("LC");
            var restoreValues = new Dictionary<string, string>
            {
                { "IRange", "19A" },
                { "PreMeasurementDelay", "0.001" },
                { "StartMeasurement", "False" },
                { "SamplingRatio", "1" },
                { "SamplingMode", "Average" },
                { "SamplingCount", "1" },
                { "VForce", "0" },
                { "IClampHi", "19" },
                { "IClampLo", "0" },
                { "FreeDriveTime", "0.001" },
                { "VSlewStepRatio", "120" },
                { "OPModeCheck", "VSIM" },
            };

            this.testConditionMock.Setup(o => o.GetPinAttributeValue("Pin1", It.IsAny<string>())).Returns((string pin, string key) => restoreValues[key]);
            this.testConditionMock.Setup(o => o.GetPinAttributeValue("Pin2", It.IsAny<string>())).Returns((string pin, string key) => restoreValues[key]);

            this.pinServiceMock.Setup(o => o.GetPinAttributeValues("Pin1", It.IsAny<List<string>>())).Returns(restoreValues);
            this.pinServiceMock.Setup(o => o.GetPinAttributeValues("Pin2", It.IsAny<List<string>>())).Returns(restoreValues);
            this.pinServiceMock.Setup(o => o.SetPinAttributeValues("Pin1", It.IsAny<Dictionary<string, string>>()));
            this.pinServiceMock.Setup(o => o.SetPinAttributeValues("Pin2", It.IsAny<Dictionary<string, string>>()));
            var underTest = new IVCurve
            {
                InstanceName = "InstanceName",
                Mode = IVCurve.Modes.Production,
                Pins = "Pin1,Pin2",
                LevelsTc = "SomeLevels",
                LowLimits = "0.1A,0.01A",
                HighLimits = "0.2A,0.1A",
                ForceSetPoint = "1,0.9",
                IRange = "19A,19A",
                IClampHi = "19,19",
                IClampLo = "0,0",
                FreeDriveTime = "0.001,0.002",
                SamplingRatio = "1,1",
                SamplingCount = "1,1",
                PreMeasurementDelay = "0.001,0.002",
                Type = "Current",
                SharedStorageTokens = "Token1,Token2",
                VSlewStepRatio = "1,1",
            };
            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(0, executeResult);
            this.dcResults.VerifyAll();
            this.dcTest.VerifyAll();
            this.pinDcResults.VerifyAll();
            this.pinGroupDcResults.VerifyAll();
            this.sharedStorageService.VerifyAll();
            this.testConditionServiceMock.VerifyAll();
            this.testConditionMock.VerifyAll();
        }
    }
}
