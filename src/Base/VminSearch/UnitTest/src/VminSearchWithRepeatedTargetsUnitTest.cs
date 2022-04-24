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

namespace Prime.TestMethods.VminSearch.UnitTest
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FunctionalService;
    using Prime.PinService;
    using Prime.TestConditionService;
    using Prime.TestMethods.VminSearch;
    using Prime.VoltageService;

    /// <summary>
    /// UnitTest class.
    /// </summary>
    [TestClass]
    public class VminSearchWithRepeatedTargetsUnitTest
    {
        private Mock<IFunctionalService> funcServiceMock;
        private Mock<ICaptureFailureTest> funcTestMock;
        private Mock<IVoltageService> voltageServiceMock;
        private Mock<IVForcePinAttribute> dpsVoltageMock;
        private Mock<IDatalogService> dataLogServiceMock;
        private Mock<IStrgvalFormat> writerMock;
        private Mock<ITestConditionService> testConditionServiceMock;
        private Mock<ITestCondition> testConditionMock;
        private Mock<IPinService> pinServiceMock;
        private Mock<IPin> pinAMock;
        private Mock<IPin> pinBMock;
        private Mock<IPin> pinCMock;
        private Mock<IPin> pinDMock;

        /// <summary>
        /// Setups the mocks.
        /// </summary>
        [TestInitialize]
        public void SetupMockingVerify()
        {
            // Mocking for IFunctionalService
            this.funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            this.funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.funcTestMock.Setup(func => func.EnableStartPatternOnFirstFail());
            this.funcTestMock.Setup(func => func.ResolvePlist("test")).Returns("plist");
            this.funcServiceMock.Setup(func => func.CreateCaptureFailureTest("plist", "level", "timing", 1, "SomeCallback()")).Returns(this.funcTestMock.Object);
            Services.FunctionalService = this.funcServiceMock.Object;

            // Mocking for IPinService
            this.pinServiceMock = new Mock<IPinService>();
            this.pinAMock = new Mock<IPin>();
            this.pinBMock = new Mock<IPin>();
            this.pinCMock = new Mock<IPin>();
            this.pinDMock = new Mock<IPin>();
            var pinAAttributes = new List<string>() { "Attribute1" };
            var pinBAttributes = new List<string>() { "Attribute2" };
            var pinCAttributes = new List<string>() { "Attribute3" };
            var pinDAttributes = new List<string>() { "Attribute4" };
            this.pinAMock.Setup(mock => mock.GetVforceMandatoryAttributes()).Returns(pinAAttributes);
            this.pinBMock.Setup(mock => mock.GetVforceMandatoryAttributes()).Returns(pinBAttributes);
            this.pinCMock.Setup(mock => mock.GetVforceMandatoryAttributes()).Returns(pinCAttributes);
            this.pinDMock.Setup(mock => mock.GetVforceMandatoryAttributes()).Returns(pinDAttributes);
            this.pinServiceMock.Setup(service => service.Get("A")).Returns(this.pinAMock.Object);
            this.pinServiceMock.Setup(service => service.Get("B")).Returns(this.pinBMock.Object);
            this.pinServiceMock.Setup(service => service.Get("C")).Returns(this.pinCMock.Object);
            this.pinServiceMock.Setup(service => service.Get("D")).Returns(this.pinDMock.Object);
            Services.PinService = this.pinServiceMock.Object;

            // Mocking for ITestConditionService
            this.testConditionServiceMock = new Mock<ITestConditionService>();
            this.testConditionMock = new Mock<ITestCondition>();
            this.testConditionServiceMock.Setup(service => service.GetTestCondition("level"))
                .Returns(this.testConditionMock.Object);
            this.testConditionMock.Setup(mock => mock.GetPinAttributeValue("A", "Attribute1")).Returns("Value1");
            this.testConditionMock.Setup(mock => mock.GetPinAttributeValue("B", "Attribute2")).Returns("Value2");
            this.testConditionMock.Setup(mock => mock.GetPinAttributeValue("C", "Attribute3")).Returns("Value3");
            this.testConditionMock.Setup(mock => mock.GetPinAttributeValue("D", "Attribute4")).Returns("Value4");
            Services.TestConditionService = this.testConditionServiceMock.Object;

            // Mocking for IVoltageService
            this.voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            this.dpsVoltageMock = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            this.voltageServiceMock.Setup(service => service.CreateVForceForPinAttribute(new List<string> { "A", "B", "C", "D", "C", "A" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(this.dpsVoltageMock.Object);
            Services.VoltageService = this.voltageServiceMock.Object;

            // Mocking for IDatalogService
            this.dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            this.writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            this.dataLogServiceMock.Setup(dataLog => dataLog.GetItuffStrgvalWriter()).Returns(this.writerMock.Object);
            this.dataLogServiceMock.Setup(dataLog => dataLog.WriteToItuff(this.writerMock.Object));
            Services.DatalogService = this.dataLogServiceMock.Object;
        }

        /// <summary>
        /// Execute simple happy path for plist always failing.
        /// </summary>
        [TestMethod]
        public void Execute_SimpleAllFailCaseWithRepeatedCores_Return0()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.ApplyTestConditions());
            this.funcTestMock.Setup(func => func.Reset());
            this.funcTestMock.Setup(func => func.Execute()).Returns(false);

            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(failureData => failureData.GetPatternName()).Returns("pat1");
            failDataMock.Setup(failureData => failureData.GetBurstIndex()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetPatternInstanceId()).Returns(0);
            failDataMock.Setup(failureData => failureData.GetVectorAddress()).Returns(0);
            this.funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(new List<IFailureData>() { failDataMock.Object });
            this.funcTestMock.Setup(func => func.DatalogFailure(1));

            this.dpsVoltageMock.Setup(voltage => voltage.Reset());
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double> { 0.40, 0.40, 0.42, 0.40, 0.42, 0.40 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double> { 0.41, 0.41, 0.43, 0.41, 0.43, 0.41 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double> { 0.42, 0.42, 0.44, 0.42, 0.44, 0.42 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double> { -9999, -9999, -9999, -9999, -9999, 0.43 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Apply(new List<double> { -9999, -9999, -9999, -9999, -9999, 0.44 }));
            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            this.writerMock.Setup(writer => writer.SetData("-9999_-9999_-9999_-9999_-9999_-9999|0.400_0.400_0.420_0.400_0.400_0.400|0.420_0.420_0.440_0.420_0.440_0.440|5"));

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = "SomeCallback()",
                VoltageTargets = "A,B,C,D,C,A",
                StartVoltages = "0.4,0.4,0.42,0.4,0.4,0.4",
                EndVoltageLimits = "0.42,0.42,0.44,0.42,0.44,0.44",
                StepSize = 0.01,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                MultiPassMasks = string.Empty,
            };
            search.TestMethodExtension = search;
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(0, search.Execute());

            // Verify mocking.
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.dataLogServiceMock.VerifyAll();
            this.writerMock.VerifyAll();
            this.testConditionServiceMock.VerifyAll();
            this.testConditionMock.VerifyAll();
            this.pinServiceMock.VerifyAll();
            this.pinAMock.VerifyAll();
            this.pinBMock.VerifyAll();
            this.pinCMock.VerifyAll();
            this.pinDMock.VerifyAll();
            failDataMock.VerifyAll();
        }
    }
}
