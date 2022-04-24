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
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="TestProgramCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class TestProgramCallbacks_UnitTest
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

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ExecuteInstance_Fail()
        {
            // Setup the mocks
            var testprogramMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramMock.Setup(o => o.GetCurrentTestInstanceName()).Returns("DummyTestInstance");
            Prime.Services.TestProgramService = testprogramMock.Object;

            var noArgs = Assert.ThrowsException<ArgumentException>(() => TestProgram.ExecuteInstance(string.Empty));
            Assert.AreEqual("DummyTestInstance.ExecuteInstance: failed parsing arguments. CommandLine.MissingRequiredOptionError", noArgs.Message);

            var badArgs = Assert.ThrowsException<ArgumentException>(() => TestProgram.ExecuteInstance("--WrongArgs=1"));
            Assert.AreEqual("DummyTestInstance.ExecuteInstance: failed parsing arguments. CommandLine.UnknownOptionError\nCommandLine.MissingRequiredOptionError", badArgs.Message);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ExecuteTestInstance_Pass()
        {
            // Setup the mocks
            var testprogramMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramMock.Setup(o => o.GetCurrentTestInstanceName()).Returns("DummyTestInstance");
            testprogramMock.Setup(o => o.ExecuteTestInstance("test1")).Returns(1);
            testprogramMock.Setup(o => o.ExecuteTestInstance("test2")).Returns(0);
            Prime.Services.TestProgramService = testprogramMock.Object;

            var sharedStorageMock = new Mock<Prime.SharedStorageService.ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("ExitPorts", "1,0", Prime.SharedStorageService.Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // Call the method under test.
            var results = TestProgram.ExecuteInstance("--test test1,test2 --save_exit_port G.U.S.ExitPorts");

            // Verify the results.
            Assert.AreEqual("1,0", results);
            testprogramMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void ExecuteTestInstance_ExceptionOnFail_Pass()
        {
            // Setup the mocks
            var testprogramMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramMock.Setup(o => o.GetCurrentTestInstanceName()).Returns("DummyTestInstance");
            testprogramMock.Setup(o => o.ExecuteTestInstance("test1")).Returns(1);
            testprogramMock.Setup(o => o.ExecuteTestInstance("test2")).Returns(0);
            Prime.Services.TestProgramService = testprogramMock.Object;

            // Call the method under test.
            var testFail = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => TestProgram.ExecuteInstance("--test test1,test2,test3 --exception_on_fail"));
            Assert.AreEqual("[DummyTestInstance] Test=[test2] failed. Exit Port=[0].", testFail.Message);

            // Verify the results.
            testprogramMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Sleep_Pass()
        {
            TestProgram.Sleep("1");
        }

        /// <summary>
        /// Test VerifyAllPrimeInstances.
        /// </summary>
        [TestMethod]
        public void VerifyAllPrimeInstances_Pass()
        {
            var allInstances = new List<string> { "Instance1", "BaseInstance", "NotAPrimeInstance", "Instance2" };
            var paramsBaseInstance = new Dictionary<string, string> { { "LogLevel", "TEST_METHOD" }, { "param2", "value2" }, { "param3", "value3" } };
            var paramsInstance1 = new Dictionary<string, string> { { "LogLevel", "DISABLED" }, { "param2", "value2" }, { "param3", "value3" } };
            var paramsInstance2 = new Dictionary<string, string> { { "LogLevel", "PRIME_DEBUG" }, { "param2", "value2" }, { "param3", "value3" } };
            var paramsNotAPrimeInstance = new Dictionary<string, string> { { "param2", "value2" }, { "param3", "value3" } };

            var testprogramMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramMock.Setup(o => o.GetCurrentTestInstanceName()).Returns("BaseInstance");
            testprogramMock.Setup(o => o.GetAllTestInstanceNames()).Returns(allInstances);
            testprogramMock.Setup(o => o.GetTestInstanceParameters("BaseInstance")).Returns(paramsBaseInstance);
            testprogramMock.Setup(o => o.GetTestInstanceParameters("Instance1")).Returns(paramsInstance1);
            testprogramMock.Setup(o => o.GetTestInstanceParameters("Instance2")).Returns(paramsInstance2);
            testprogramMock.Setup(o => o.GetTestInstanceParameters("NotAPrimeInstance")).Returns(paramsNotAPrimeInstance);
            testprogramMock.Setup(o => o.SetTestInstanceParameter("Instance2", "LogLevel", "DISABLED"));
            testprogramMock.Setup(o => o.VerifyTestInstance("Instance1")).Returns(true);
            testprogramMock.Setup(o => o.VerifyTestInstance("Instance2")).Returns(true);
            Prime.Services.TestProgramService = testprogramMock.Object;

            Assert.AreEqual("1", TestProgram.VerifyAllPrimeInstances(string.Empty));
            testprogramMock.VerifyAll();
        }

        /// <summary>
        /// Test VerifyAllPrimeInstances.
        /// </summary>
        [TestMethod]
        public void VerifyAllPrimeInstances_Fail()
        {
            var allInstances = new List<string> { "Instance1" };
            var paramsBaseInstance = new Dictionary<string, string> { { "LogLevel", "DISABLED" }, { "param2", "value2" }, { "param3", "value3" } };
            var paramsInstance1 = new Dictionary<string, string> { { "LogLevel", "DISABLED" } };

            var testprogramMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramMock.Setup(o => o.GetCurrentTestInstanceName()).Returns("BaseInstance");
            testprogramMock.Setup(o => o.GetAllTestInstanceNames()).Returns(allInstances);
            testprogramMock.Setup(o => o.GetTestInstanceParameters("BaseInstance")).Returns(paramsBaseInstance);
            testprogramMock.Setup(o => o.GetTestInstanceParameters("Instance1")).Returns(paramsInstance1);
            testprogramMock.Setup(o => o.VerifyTestInstance("Instance1")).Returns(false);
            Prime.Services.TestProgramService = testprogramMock.Object;

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => TestProgram.VerifyAllPrimeInstances(string.Empty));
            Assert.AreEqual("Some instances failed Verify.", ex.Message);
            testprogramMock.VerifyAll();
        }
    }
}
