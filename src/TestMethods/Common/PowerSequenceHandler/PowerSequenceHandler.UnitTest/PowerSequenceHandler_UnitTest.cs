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

namespace PowerSequenceHandler.UnitTest
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.TestConditionService;

    /// <summary>
    /// PowerSequenceHandler_UnitTest.
    /// </summary>
    [TestClass]
    public class PowerSequenceHandler_UnitTest
    {
        /// <summary>
        /// TestInitialize.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(
                    o => o.PrintDebug(It.IsAny<string>())).
                Callback((string msg) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            Prime.Services.ConsoleService = consoleServiceMock.Object;
        }

        /// <summary>
        /// Verify skipping apply TC.
        /// </summary>
        [TestMethod]
        public void Verify_SkipApply_Pass()
        {
            var underTest = new PowerSequenceHandler
            {
                PowerOnTc = "SomeLevel",
                ApplyPowerOn = PowerSequenceHandler.ForceApplyTc.Skip,
            };
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            underTest.Verify();
            testConditionServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute skipping apply TC.
        /// </summary>
        [TestMethod]
        public void Execute_SwitchModeSkipApply_Pass()
        {
            var underTest = new PowerSequenceHandler
            {
                PowerOnTc = "SomeLevel",
                ApplyPowerOn = PowerSequenceHandler.ForceApplyTc.Switch,
            };
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition(underTest.PowerOnTc.ToString()))
                .Returns(testConditionMock.Object);
            testConditionServiceMock.Setup(t => t.GetPowerUpTCName()).Returns("SomeLevel");
            testConditionServiceMock.Setup(t => t.SetPowerUpTCName(underTest.PowerOnTc.ToString()));
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(1, executeResult);
            testConditionServiceMock.VerifyAll();
        }

        /// <summary>
        /// Verify applying Power-on and Power-down TCs.
        /// </summary>
        [TestMethod]
        public void Verify_ForceModeApply_Pass()
        {
            var underTest = new PowerSequenceHandler
            {
                PowerOnTc = "SomePowerOn",
                PowerDownTc = "SomePowerDown",
                ApplyPowerOn = PowerSequenceHandler.ForceApplyTc.Always,
                ApplyPowerDown = PowerSequenceHandler.ForceApplyTc.Always,
            };
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionServiceMock.Setup(t => t.GetTestCondition(underTest.PowerOnTc.ToString()))
                .Returns(testConditionMock.Object);
            testConditionServiceMock.Setup(t => t.GetTestCondition(underTest.PowerDownTc.ToString()))
                .Returns(testConditionMock.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            underTest.Verify();
            testConditionServiceMock.VerifyAll();
        }

        /// <summary>
        /// Verify applying Power-on and Power-down TCs.
        /// </summary>
        [TestMethod]
        public void Execute_ModeSwitchApply_Pass()
        {
            var underTest = new PowerSequenceHandler
            {
                PowerOnTc = "SomePowerOn",
                PowerDownTc = "SomePowerDown",
                ApplyPowerOn = PowerSequenceHandler.ForceApplyTc.Switch,
                ApplyPowerDown = PowerSequenceHandler.ForceApplyTc.Switch,
            };
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            var powerOnMock = new Mock<ITestCondition>(MockBehavior.Strict);
            powerOnMock.Setup(p => p.Apply(SmartTCCategoryType.LEVELS_POWER_ON));
            var powerDownMock = new Mock<ITestCondition>(MockBehavior.Strict);
            powerDownMock.Setup(p => p.Apply(SmartTCCategoryType.LEVELS_POWER_DOWN));
            testConditionServiceMock.Setup(t => t.GetTestCondition(underTest.PowerOnTc.ToString()))
                .Returns(powerOnMock.Object);
            testConditionServiceMock.Setup(t => t.GetTestCondition(underTest.PowerDownTc.ToString()))
                .Returns(powerDownMock.Object);
            testConditionServiceMock.SetupSequence(t => t.GetPowerUpTCName())
                .Returns("OriginalPowerOn")
                .Returns(underTest.PowerOnTc.ToString());
            testConditionServiceMock.Setup(t => t.SetPowerUpTCName(underTest.PowerOnTc.ToString()));
            testConditionServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.LEVELS_POWER_DOWN));
            testConditionServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.LEVELS_POWER_ON));
            testConditionServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.LEVELS_SETUP));
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(1, executeResult);
            executeResult = underTest.Execute();
            Assert.AreEqual(1, executeResult);
            testConditionServiceMock.VerifyAll();
            powerOnMock.Verify(p => p.Apply(SmartTCCategoryType.LEVELS_POWER_ON), Times.Once);
            powerDownMock.Verify(p => p.Apply(SmartTCCategoryType.LEVELS_POWER_DOWN), Times.Once);
        }

        /// <summary>
        /// refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_AlarmMode_P2()
        {
            var underTest = new PowerSequenceHandler
            {
                PowerOnTc = "SomePowerOn",
                PowerDownTc = "SomePowerDown",
                ApplyPowerOn = PowerSequenceHandler.ForceApplyTc.Always,
                ApplyPowerDown = PowerSequenceHandler.ForceApplyTc.Always,
                AlarmMode = PowerSequenceHandler.AlarmModes.Enabled,
            };
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            var powerOnMock = new Mock<ITestCondition>(MockBehavior.Strict);
            powerOnMock.Setup(p => p.Apply(SmartTCCategoryType.LEVELS_POWER_ON))
                .Throws(new AlarmException("Fl", "Fn", 1, "An alarm has occurred.", new List<AlarmException.AlarmInfo>()));
            var powerDownMock = new Mock<ITestCondition>(MockBehavior.Strict);
            powerDownMock.Setup(p => p.Apply(SmartTCCategoryType.LEVELS_POWER_DOWN));
            testConditionServiceMock.Setup(t => t.GetTestCondition(underTest.PowerOnTc.ToString()))
                .Returns(powerOnMock.Object);
            testConditionServiceMock.Setup(t => t.GetTestCondition(underTest.PowerDownTc.ToString()))
                .Returns(powerDownMock.Object);
            testConditionServiceMock.SetupSequence(t => t.GetPowerUpTCName())
                .Returns("OriginalPowerOn")
                .Returns(underTest.PowerOnTc.ToString());
            testConditionServiceMock.Setup(t => t.SetPowerUpTCName(underTest.PowerOnTc.ToString()));
            testConditionServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.LEVELS_POWER_DOWN));
            testConditionServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.LEVELS_POWER_ON));
            testConditionServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.LEVELS_SETUP));
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(2, executeResult);
            testConditionServiceMock.VerifyAll();
            powerOnMock.VerifyAll();
            powerDownMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_ModeAlwaysApply_Pass()
        {
            var underTest = new PowerSequenceHandler
            {
                PowerOnTc = "SomePowerOn",
                PowerDownTc = "SomePowerDown",
                ApplyPowerOn = PowerSequenceHandler.ForceApplyTc.Always,
                ApplyPowerDown = PowerSequenceHandler.ForceApplyTc.Always,
            };
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            var powerOnMock = new Mock<ITestCondition>(MockBehavior.Strict);
            powerOnMock.Setup(p => p.Apply(SmartTCCategoryType.LEVELS_POWER_ON));
            var powerDownMock = new Mock<ITestCondition>(MockBehavior.Strict);
            powerDownMock.Setup(p => p.Apply(SmartTCCategoryType.LEVELS_POWER_DOWN));
            testConditionServiceMock.Setup(t => t.GetTestCondition(underTest.PowerOnTc.ToString()))
                .Returns(powerOnMock.Object);
            testConditionServiceMock.Setup(t => t.GetTestCondition(underTest.PowerDownTc.ToString()))
                .Returns(powerDownMock.Object);
            testConditionServiceMock.SetupSequence(t => t.GetPowerUpTCName())
                .Returns("OriginalPowerOn")
                .Returns(underTest.PowerOnTc.ToString());
            testConditionServiceMock.Setup(t => t.SetPowerUpTCName(underTest.PowerOnTc.ToString()));
            testConditionServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.LEVELS_POWER_DOWN));
            testConditionServiceMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.LEVELS_POWER_ON));
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            underTest.Verify();
            var executeResult = underTest.Execute();
            Assert.AreEqual(1, executeResult);
            executeResult = underTest.Execute();
            Assert.AreEqual(1, executeResult);
            testConditionServiceMock.VerifyAll();
            powerOnMock.Verify(p => p.Apply(SmartTCCategoryType.LEVELS_POWER_ON), Times.Exactly(2));
            powerDownMock.Verify(p => p.Apply(SmartTCCategoryType.LEVELS_POWER_DOWN), Times.Exactly(2));
        }
    }
}
