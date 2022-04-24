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

namespace RunCallback.UnitTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class RunCallback_UnitTest
    {
        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => { Console.WriteLine(s); });
            consoleMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string n, string src) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleMock.Object;
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamEmpty_False()
        {
            var underTest = new RunCallback();

            var ex = Assert.ThrowsException<ArgumentException>(() => underTest.Verify());
            Assert.AreEqual("Callback parameter should not be empty.", ex.Message);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_CallbackDoesNotExist_False()
        {
            var testprogramServiceMock = this.MockTestProgramServiceVerifyFail();

            var underTest = new RunCallback
            {
                Callback = "SomeCallback",
            };

            var ex = Assert.ThrowsException<ArgumentException>(() => underTest.Verify());
            Assert.AreEqual("Callback=[SomeCallback] does not exist.", ex.Message);
            testprogramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_BadResultTokenFormat_False()
        {
            var testprogramServiceMock = this.MockTestProgramServiceVerify("SomeCallback");
            var underTest = new RunCallback
            {
                Callback = "SomeCallback",
                ResultToken = "blah",
            };

            var ex = Assert.ThrowsException<ArgumentException>(() => underTest.Verify());
            Assert.AreEqual("ResultToken=[blah] must be of the form G.[ULI].[SDI].Token", ex.Message);
            testprogramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamNoEmpty_True()
        {
            var testprogramServiceMock = this.MockTestProgramServiceVerify("SomeCallback");
            var underTest = new RunCallback
            {
                Callback = "SomeCallback",
            };

            underTest.Verify();
            testprogramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamWithArgs_True()
        {
            var testprogramServiceMock = this.MockTestProgramServiceVerify("SomeCallback");
            var underTest = new RunCallback
            {
                Callback = "SomeCallback",
                Parameters = "--args1 one --args2 two",
            };

            underTest.Verify();
            testprogramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_NoParams_True()
        {
            var testprogramServiceMock = this.MockTestProgramServiceExecute("SomeCallback", string.Empty, string.Empty);
            var underTest = new RunCallback
            {
                Callback = "SomeCallback",
            };

            underTest.Verify();

            var exitPort = underTest.Execute();
            Assert.AreEqual(1, exitPort);

            testprogramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_WithParams_True()
        {
            var testprogramServiceMock = this.MockTestProgramServiceExecute("SomeCallback", "--args1 one --args2 two", string.Empty);
            var underTest = new RunCallback
            {
                Callback = "SomeCallback",
                Parameters = "--args1 one --args2 two",
            };

            underTest.Verify();

            var exitPort = underTest.Execute();
            Assert.AreEqual(1, exitPort);

            testprogramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_WithResult_True()
        {
            var testprogramServiceMock = this.MockTestProgramServiceExecute("SomeCallback", "--args1 one --args2 two", "somereturnvalue");
            var sharedstorageMock = this.MockSharedStorage(Context.DUT, "SaveReturnToken", "somereturnvalue");

            var underTest = new RunCallback
            {
                Callback = "SomeCallback",
                Parameters = "--args1 one --args2 two",
                ResultToken = "G.U.S.SaveReturnToken",
            };

            underTest.Verify();

            var exitPort = underTest.Execute();
            Assert.AreEqual(1, exitPort);

            testprogramServiceMock.VerifyAll();
            sharedstorageMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_WithExitPort_True()
        {
            var testprogramServiceMock = this.MockTestProgramServiceExecute("SomeCallback", "--args1 one --args2 two", "15");
            var sharedstorageMock = this.MockSharedStorage(Context.DUT, "SaveReturnToken", "15");
            sharedstorageMock.Setup(o => o.KeyExistsInStringTable("SaveReturnToken", Context.DUT)).Returns(true);
            sharedstorageMock.Setup(o => o.GetStringRowFromTable("SaveReturnToken", Context.DUT)).Returns("15");

            var underTest = new RunCallback
            {
                Callback = "SomeCallback",
                Parameters = "--args1 one --args2 two",
                ResultToken = "G.U.S.SaveReturnToken",
                ResultPort = "ToInt32([G.U.S.SaveReturnToken])==15?9:0",
            };

            underTest.Verify();

            var exitPort = underTest.Execute();
            Assert.AreEqual(9, exitPort);

            testprogramServiceMock.VerifyAll();
            sharedstorageMock.VerifyAll();
        }

        private Mock<ITestProgramService> MockTestProgramServiceVerifyFail()
        {
            var mock = new Mock<ITestProgramService>(MockBehavior.Strict);
            mock.Setup(o => o.DoesCallbackExist(It.IsAny<string>())).Returns(false);
            Prime.Services.TestProgramService = mock.Object;
            return mock;
        }

        private Mock<ITestProgramService> MockTestProgramServiceVerify(string callbackName)
        {
            var mock = new Mock<ITestProgramService>(MockBehavior.Strict);
            mock.Setup(o => o.DoesCallbackExist(callbackName)).Returns(true);
            Prime.Services.TestProgramService = mock.Object;
            return mock;
        }

        private Mock<ITestProgramService> MockTestProgramServiceExecute(string callbackName, string args, string result)
        {
            var mock = new Mock<ITestProgramService>(MockBehavior.Strict);
            mock.Setup(o => o.DoesCallbackExist(callbackName)).Returns(true);
            mock.Setup(o => o.TriggerCallback(callbackName, args)).Returns(result);
            Prime.Services.TestProgramService = mock.Object;
            return mock;
        }

        private Mock<ISharedStorageService> MockSharedStorage(Prime.SharedStorageService.Context context, string token, string value)
        {
            var mock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            mock.Setup(o => o.InsertRowAtTable(token, value, context));
            Prime.Services.SharedStorageService = mock.Object;
            return mock;
        }
    }
}
