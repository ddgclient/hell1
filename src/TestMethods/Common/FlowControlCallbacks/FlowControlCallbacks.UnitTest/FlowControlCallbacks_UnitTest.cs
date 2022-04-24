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

namespace FlowControlCallbacks.UnitTest
{
    using System;
    using System.Collections.Generic;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DffService;
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="FlowControlCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class FlowControlCallbacks_UnitTest
    {
        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => Console.WriteLine(s));
            consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string n, string src) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void SetFlow_Continue()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string> { { "FlowNumber", "1" } });
            testProgramServiceMock.Setup(o => o.SetDomainCurrentFlow("domain", 1));
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            FlowControlCallbacks.SetFlow("domain");
            testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void CheckFlow_NotSinglePointMode_Continue()
        {
            var vminForwardingServiceMock = MockIsSinglePointMode(false);

            var result = FlowControlCallbacks.CheckFlow("domain");
            Assert.AreEqual("CONTINUE", result);
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void CheckFlow_Continue()
        {
            var vminForwardingServiceMock = MockIsSinglePointMode(true);
            var testProgramServiceMock = MockTestProgramFlowIsStandard("domain", "1", 1);

            var result = FlowControlCallbacks.CheckFlow("domain");
            Assert.AreEqual("CONTINUE", result);
            vminForwardingServiceMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void CheckFlow_UseDFF_Continue()
        {
            var vminForwardingServiceMock = MockIsSinglePointMode(true);
            var dffServiceMock = MockDffValue("DMN1", "1");
            var testProgramServiceMock = MockTestProgramFlowIsDff("domain", "1", "DMN1");

            var result = FlowControlCallbacks.CheckFlow("domain");
            Assert.AreEqual("CONTINUE", result);
            vminForwardingServiceMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
            dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void CheckFlow_UseDFF_Exception()
        {
            var vminForwardingServiceMock = MockIsSinglePointMode(true);
            var dffServiceMock = MockDffValue("DMN1", "NotANumber");
            var testProgramServiceMock = MockTestProgramFlowIsDff("domain", "1", "DMN1");

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => FlowControlCallbacks.CheckFlow("domain"));
            Assert.AreEqual("Unable to convert DFF token [DMN1] value=[NotANumber] to Integer.", ex.Message);
            vminForwardingServiceMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
            dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void CheckFlow_Fail()
        {
            var vminForwardingServiceMock = MockIsSinglePointMode(true);
            var testProgramServiceMock = MockTestProgramFlowIsStandard("domain", "2", 3);

            var result = FlowControlCallbacks.CheckFlow("domain");
            Assert.AreEqual("FAIL", result);
            vminForwardingServiceMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
        }

        private static Mock<IVminForwardingFactory> MockIsSinglePointMode(bool mode)
        {
            var vminForwardingServiceMock = new Mock<IVminForwardingFactory>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.IsSinglePointMode()).Returns(mode);
            VminForwarding.Service = vminForwardingServiceMock.Object;
            return vminForwardingServiceMock;
        }

        private static Mock<IDffService> MockDffValue(string token, string value)
        {
            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(o => o.GetDff(token, true)).Returns(value);
            Prime.Services.DffService = dffServiceMock.Object;
            return dffServiceMock;
        }

        private static Mock<ITestProgramService> MockTestProgramFlowIsDff(string domain, string testInstanceFlow, string dffToken)
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string> { { "FlowNumber", testInstanceFlow } });
            testProgramServiceMock.Setup(o => o.IsDffEnableForDomainFlow(domain)).Returns(true);
            testProgramServiceMock.Setup(o => o.GetDffTokenNameForDomainFlow(domain)).Returns(dffToken);
            Prime.Services.TestProgramService = testProgramServiceMock.Object;
            return testProgramServiceMock;
        }

        private static Mock<ITestProgramService> MockTestProgramFlowIsStandard(string domain, string testInstanceFlow, int currentFlow)
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string> { { "FlowNumber", testInstanceFlow } });
            testProgramServiceMock.Setup(o => o.IsDffEnableForDomainFlow(domain)).Returns(false);
            testProgramServiceMock.Setup(o => o.GetDomainCurrentFlow(domain)).Returns(currentFlow);
            Prime.Services.TestProgramService = testProgramServiceMock.Object;
            return testProgramServiceMock;
        }
    }
}
