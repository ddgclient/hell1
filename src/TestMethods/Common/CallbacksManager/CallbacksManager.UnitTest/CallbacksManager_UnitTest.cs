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

namespace CallbacksManager.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.TestProgramService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class CallbacksManager_UnitTest
    {
        private Mock<IConsoleService> consoleServiceMock;

        private List<string> RegisteredFunctions { get; set; }

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(
                    o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) => System.Console.WriteLine(msg));
            this.consoleServiceMock.Setup(
                    o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string member, string path) => System.Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;
        }

        /// <summary>
        /// Invalid argument value.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Call_InvalidArgument_Fail()
        {
            CallbacksManager.Call("someInvalidFormat");
        }

        /// <summary>
        /// Valid Format. All functions pass.
        /// </summary>
        [TestMethod]
        public void Call_Pass()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("Function1")).Returns(true);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("Fu_Ab2")).Returns(true);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("n")).Returns(true);
            testProgramServiceMock.Setup(t => t.TriggerCallback("Function1", "some argument")).Returns(string.Empty);
            testProgramServiceMock.Setup(t => t.TriggerCallback("Fu_Ab2", "1")).Returns("Pass");
            testProgramServiceMock.Setup(t => t.TriggerCallback("n", string.Empty)).Returns("1");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var result = CallbacksManager.Call("Function1(some argument) | Fu_Ab2(1)|n()");
            Assert.AreEqual("pass", result);
        }

        /// <summary>
        /// Valid Format. All functions pass.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Call_FunctionDoesNotExist_Fail()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("Function1")).Returns(true);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("Fu_Ab2")).Returns(true);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("n")).Returns(false);
            testProgramServiceMock.Setup(t => t.TriggerCallback("Function1", "some argument")).Returns(string.Empty);
            testProgramServiceMock.Setup(t => t.TriggerCallback("Fu_Ab2", "1")).Returns("Pass");
            testProgramServiceMock.Setup(t => t.TriggerCallback("n", string.Empty)).Returns("1");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            CallbacksManager.Call("Function1(some argument) | Fu_Ab2(1)|n()");
            Assert.Fail();
        }

        /// <summary>
        /// Test the failing case of GuiCall.
        /// </summary>
        [TestMethod]
        public void GuiCall_InvalidArgument_Fail()
        {
            var ex = Assert.ThrowsException<ArgumentException>(() => CallbacksManager.GuiCall("badargs"));
            Assert.AreEqual("GuiCallback is expecting at least 3 pipe-separated elements. Actual=[badargs].", ex.Message);
        }

        /// <summary>
        /// Test the failing case of GuiCall.
        /// </summary>
        [TestMethod]
        public void GuiCall_FunctionDoesNotExist1_Fail()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("Function1")).Returns(false);
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var ex = Assert.ThrowsException<ArgumentException>(() => CallbacksManager.GuiCall("Value|Function1|Garbage"));
            Assert.AreEqual("Callback=[Function1] does not exist and Argument=[Value] is not in Function(Argument) format.", ex.Message);
            testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the failing case of GuiCall.
        /// </summary>
        [TestMethod]
        public void GuiCall_FunctionDoesNotExist2_Fail()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("TestName")).Returns(false);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("Function1")).Returns(false);
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var ex = Assert.ThrowsException<ArgumentException>(() => CallbacksManager.GuiCall("Function1(Value)|TestName|Garbage"));
            Assert.AreEqual("Callback=[Function1] does not exist.", ex.Message);
            testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the passing case of GuiCall.
        /// </summary>
        [TestMethod]
        public void GuiCall_FunctionException_Fail()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("Function1")).Returns(true);
            testProgramServiceMock.Setup(t => t.TriggerCallback("Function1", "ValueA")).Throws(new TestMethodException("Some Error Message"));
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var ex = Assert.ThrowsException<TestMethodException>(() => CallbacksManager.GuiCall("ValueA|Function1|Garbage"));
            Assert.AreEqual("Some Error Message", ex.Message);
            testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the passing case of GuiCall.
        /// </summary>
        [TestMethod]
        public void GuiCall_FunctionArgsFormat_Pass()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("Function1")).Returns(true);
            testProgramServiceMock.Setup(t => t.TriggerCallback("Function1", "ValueA ValueB ValueC")).Returns("blah");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            Assert.AreEqual("blah", CallbacksManager.GuiCall("ValueA ValueB ValueC|Function1|Garbage"));
            testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the passing case of GuiCall.
        /// </summary>
        [TestMethod]
        public void GuiCall_TestNameFormat_Pass()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("TestName")).Returns(false);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("Function1")).Returns(true);
            testProgramServiceMock.Setup(t => t.TriggerCallback("Function1", "param1=Args1 param2=Args2")).Returns("blah");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            Assert.AreEqual("blah", CallbacksManager.GuiCall("Function1(param1=Args1 param2=Args2)|TestName|Garbage"));
            testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the passing case of GuiCall.
        /// </summary>
        [TestMethod]
        public void GuiCall_Call_Pass()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("TestName")).Returns(false);
            testProgramServiceMock.Setup(t => t.DoesCallbackExist("Call")).Returns(true);
            testProgramServiceMock.Setup(t => t.TriggerCallback("Call", "Func1(argsA argsB) | Func2(args) | Func3(param1=argsD argsF)")).Returns("blah");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            Assert.AreEqual("blah", CallbacksManager.GuiCall("Call(Func1(argsA argsB) | Func2(args) | Func3(param1=argsD argsF))|TestName|Garbage"));
            testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the RegisterCallbacks function.
        /// </summary>
        [TestMethod]
        public void RegisterCallbacks_NewFunction_Pass()
        {
            // Setup the mocks.
            this.RegisteredFunctions = new List<string>();
            var testprogramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramServiceMock.Setup(o => o.DoesCallbackExist(It.IsAny<string>())).Returns(false);
            testprogramServiceMock.As<Prime.TestProgramService.Internal.ITestProgramService>().
                Setup(o => o.RegisterCallback(It.IsAny<string>(), It.IsAny<CallbackDelegate>())).
                    Callback((string name, CallbackDelegate f) =>
                    {
                        System.Console.WriteLine($"DEBUG: Registering {name}");
                        if (this.RegisteredFunctions.Contains(name))
                        {
                            throw new ArgumentException($"Duplicate Callback: Function [{name}] is defined twice.");
                        }

                        this.RegisteredFunctions.Add(name);
                    });

            testprogramServiceMock.Setup(o => o.RegisterGuiCallback("GuiCall"));
            Prime.Services.TestProgramService = testprogramServiceMock.Object;

            // Setup the instance.
            var underTest = new CallbacksManager
            {
                InstanceName = "MyInstance",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.TEST_METHOD,
            };

            underTest.TestMethodExtension = underTest;

            // Run the test and verify the mocks.
            underTest.Verify();
            testprogramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the RegisterCallbacks function when the callbacks are already registered.
        /// </summary>
        [TestMethod]
        public void RegisterCallbacks_AlreadyExist_Pass()
        {
            // Setup the mocks.
            var testprogramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramServiceMock.Setup(o => o.DoesCallbackExist(It.IsAny<string>())).Returns(true);
            testprogramServiceMock.Setup(o => o.RegisterGuiCallback("GuiCall"));
            Prime.Services.TestProgramService = testprogramServiceMock.Object;

            // Setup the instance.
            var underTest = new CallbacksManager
            {
                InstanceName = "MyInstance",
                LogLevel = Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED,
            };

            underTest.TestMethodExtension = underTest;

            // Run the test and verify the mocks.
            underTest.Verify();
            testprogramServiceMock.VerifyAll();
        }
    }
}
