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

namespace DebugCallbacks.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.PerformanceService;
    using Prime.PinService;
    using Prime.TestConditionService;
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="TestConditionCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class TestConditionCallbacks_UnitTest
    {
        private Mock<ITestProgramService> testprogramMock;
        private Mock<ITestConditionService> testConditionMock;

        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            this.testprogramMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            this.testprogramMock.Setup(o => o.GetCurrentTestInstanceName()).Returns("DummyTestInstance");
            Prime.Services.TestProgramService = this.testprogramMock.Object;
            this.testConditionMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            Prime.Services.TestConditionService = this.testConditionMock.Object;
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => { Console.WriteLine(s); });
            consoleMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string n, string src) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleMock.Object;
            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void EnableSmartTC_Pass()
        {
            this.testConditionMock.Setup(o => o.EnableSmartTC());
            var result = TestConditions.EnableSmartTC("dummy");
            Assert.AreEqual("1", result);
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void DisableSmartTC_Pass()
        {
            this.testConditionMock.Setup(o => o.DisableSmartTC());
            var result = TestConditions.DisableSmartTC("dummy");
            Assert.AreEqual("1", result);
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void ApplyEndSequence_Pass()
        {
            this.testConditionMock.Setup(o => o.ApplyEndSequence());
            var result = TestConditions.ApplyEndSequence("dummy");
            Assert.AreEqual("1", result);
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void FlushAllSmartTCCategories_Pass()
        {
            this.testConditionMock.Setup(o => o.FlushAllSmartTCCategories());
            var result = TestConditions.FlushAllSmartTCCategories("dummy");
            Assert.AreEqual("1", result);
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void SetPowerUpTCName_Pass()
        {
            this.testConditionMock.Setup(o => o.SetPowerUpTCName("Value"));
            var result = TestConditions.SetPowerUpTCName("Value");
            Assert.AreEqual("1", result);
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void FlushSmartTCCategory_Pass()
        {
            this.testConditionMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.LEVELS_SETUP));
            var result = TestConditions.FlushSmartTCCategory("LEVELS_SETUP");
            Assert.AreEqual("1", result);
            this.testConditionMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void FlushSmartTCCategory_InvalidType_Fail()
        {
            this.testConditionMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.LEVELS_SETUP));
            var result = TestConditions.FlushSmartTCCategory("ERROR_HERE");
            Assert.AreEqual("0", result);
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void FlushSmartTCCategory_ErrorOnFlush_Fail()
        {
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleMock.Setup(o => o.PrintError("Failed FlushSmartTCCategory. Invalid SmartTC Type 'LEVELS_SETUP'", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleMock.Object;

            var testConditionMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.FlushSmartTCCategory(SmartTCCategoryType.LEVELS_SETUP)).Throws(new Prime.Base.Exceptions.FatalException("Invalid SmartTC Type 'LEVELS_SETUP'"));
            Prime.Services.TestConditionService = testConditionMock.Object;

            var result = TestConditions.FlushSmartTCCategory("LEVELS_SETUP");
            Assert.AreEqual("0", result);

            consoleMock.VerifyAll();
            testConditionMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void SetPinAttributes_Pauses_Pass()
        {
            var pinService = new Mock<IPinService>(MockBehavior.Strict);
            pinService.Setup(o => o.SetPinAttributeValues("PinA", new Dictionary<string, string> { { "AttributeName1", "AttributeValue1" }, { "AttributeName2", "AttributeValue2" } }));
            Prime.Services.PinService = pinService.Object;
            TestConditions.SetPinAttributes("--prepause 10 --postpause 11 --settings PinA:AttributeName1:AttributeValue1 PinA:AttributeName2:AttributeValue2");
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void SetPinAttributes_NoPauses_Pass()
        {
            var pinService = new Mock<IPinService>(MockBehavior.Strict);
            pinService.Setup(o => o.SetPinAttributeValues("PinA", new Dictionary<string, string> { { "AttributeName1", "AttributeValue1" }, { "AttributeName2", "AttributeValue2" } }));
            Prime.Services.PinService = pinService.Object;
            TestConditions.SetPinAttributes("--settings PinA:AttributeName1:AttributeValue1 PinA:AttributeName2:AttributeValue2");
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetPinAttributes_InvalidFormat_Fail()
        {
            TestConditions.SetPinAttributes("--settings PinA:AttributeName1:AttributeValue1 PinA:AttributeName2:AttributeValue2:TypoHERE");
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void ValidatePatternTriggerMap_Pass()
        {
            var testConditionMock = new Mock<TOSUserSDK.ITestConditions>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.ValidatePatternTriggerMap("triggerName", "plistName")).Returns(true);
            TOSUserSDK.TestConditions.Service = testConditionMock.Object;

            TestConditions.ValidatePatternTriggerMap("triggerName, plistName");
            testConditionMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void ApplyPatternTriggerMap_Pass()
        {
            var testConditionMock = new Mock<TOSUserSDK.ITestConditions>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.ApplyPatternTriggerMap("triggerName", "plistName")).Returns(true);
            TOSUserSDK.TestConditions.Service = testConditionMock.Object;

            TestConditions.ApplyPatternTriggerMap("triggerName ,plistName");
            testConditionMock.VerifyAll();
        }
    }
}
