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

namespace VoltageConverterCallbacks.UnitTest
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.PerformanceService;
    using Prime.PinService;
    using Prime.TestConditionService;
    using Prime.TestProgramService;
    using Prime.VoltageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class VoltageConverterCallbacks_UnitTest
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<ITestProgramService> testProgramServiceMock;
        private Mock<IVoltageService> voltageServiceMock;
        private Mock<ITestConditionService> conditionServiceMock;
        private Mock<IPinService> pinServiceMock;
        private Mock<IPerformanceService> performanceServiceMock;

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

            this.testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            Prime.Services.TestProgramService = this.testProgramServiceMock.Object;

            this.voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = this.voltageServiceMock.Object;

            this.conditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            Prime.Services.TestConditionService = this.conditionServiceMock.Object;

            this.pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            Prime.Services.PinService = this.pinServiceMock.Object;

            this.performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = this.performanceServiceMock.Object;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_FIVR_Pass()
        {
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string> { { "LevelsTc", "SomeLevels" }, { "Patlist", "SomePlist" } });
            var voltageObject = new Mock<IFivrCondition>(MockBehavior.Strict);
            voltageObject.Setup(o => o.ApplyCondition());
            this.voltageServiceMock.Setup(o => o.CreateFivrForCondition("NOM", "SomePlist")).Returns(voltageObject.Object);

            VoltageConverterCallbacks.VoltageConverter("--fivrcondition=NOM --railconfigurations=DlvrConfig");

            this.testProgramServiceMock.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            voltageObject.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_Pin_Pass()
        {
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string> { { "LevelsTc", "SomeLevels" }, { "Patlist", "SomePlist" } });
            var testCondition = new Mock<ITestCondition>(MockBehavior.Strict);
            testCondition.Setup(o => o.GetPinAttributeValue("Pin1", "FreeDriveTime")).Returns("1mS");
            this.conditionServiceMock.Setup(o => o.GetTestCondition("SomeLevels")).Returns(testCondition.Object);
            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(o => o.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            this.pinServiceMock.Setup(o => o.Get("Pin1")).Returns(pinMock.Object);
            var voltageObject = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            voltageObject.Setup(o => o.Apply(new List<double> { 1.2 }));
            this.voltageServiceMock.Setup(o => o.CreateVForceForPinAttribute(new List<string> { "Pin1" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(voltageObject.Object);

            VoltageConverterCallbacks.VoltageConverter("--overrides Pin1:1.2");
            this.testProgramServiceMock.VerifyAll();
            this.conditionServiceMock.VerifyAll();
            this.voltageServiceMock.VerifyAll();
            testCondition.VerifyAll();
        }

        /// <summary>
        /// Cleanup.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
        }
    }
}
