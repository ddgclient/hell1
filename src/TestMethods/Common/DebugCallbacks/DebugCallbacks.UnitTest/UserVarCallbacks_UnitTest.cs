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
    using Prime.SharedStorageService;
    using Prime.TestProgramService;
    using Prime.UserVarService;

    /// <summary>
    /// Defines the <see cref="UserVarCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class UserVarCallbacks_UnitTest
    {
        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            var testprogramMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramMock.Setup(o => o.GetCurrentTestInstanceName()).Returns("DummyTestInstance");
            Prime.Services.TestProgramService = testprogramMock.Object;

            var consoleMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) => Console.WriteLine(msg));
            consoleMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleMock.Object;

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Test the WriteUser Callback.
        /// </summary>
        [TestMethod]
        public void WriteUserVar_InvalidType_Exception()
        {
            // run the test
            var ex = Assert.ThrowsException<ArgumentException>(() =>
                UserVar.WriteUserVar("--uservar SC.ValueDest --value SomeValue --type NotAValidType"));
            Assert.IsTrue(ex.Message.StartsWith("Requested value 'NOTAVALIDTYPE' was not found."));
        }

        /// <summary>
        /// Test the WriteUser Callback.
        /// </summary>
        [TestMethod]
        public void WriteUserVar_FromLiteralStringType_Pass()
        {
            // setup the mocks.
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("SomeValue")).Returns(false);
            userVarMock.Setup(o => o.SetValue("SC.ValueDest", "SomeValue"));
            Prime.Services.UserVarService = userVarMock.Object;

            // run the test
            UserVar.WriteUserVar("--uservar SC.ValueDest --value SomeValue --type String");

            // verify the mock
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteUser Callback.
        /// </summary>
        [TestMethod]
        public void WriteUserVar_FromLiteralTestUserVarExceptionPath_Pass()
        {
            // setup the mocks.
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("Lots.Of.Dots")).Throws(new Prime.Base.Exceptions.FatalException("too many dots..."));
            userVarMock.Setup(o => o.SetValue("SC.ValueDest", "Lots.Of.Dots"));
            Prime.Services.UserVarService = userVarMock.Object;

            // run the test
            UserVar.WriteUserVar("--uservar SC.ValueDest --value Lots.Of.Dots --type String");

            // verify the mock
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteUser Callback.
        /// </summary>
        [TestMethod]
        public void WriteUserVar_FromGsdsStringType_Pass()
        {
            // setup the mocks.
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.SetValue("SC.ValueDest", "SomeValue"));
            Prime.Services.UserVarService = userVarMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInStringTable("SomeToken", Context.DUT)).Returns(true);
            sharedStorageMock.Setup(o => o.GetStringRowFromTable("SomeToken", Context.DUT)).Returns("SomeValue");
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // run the test
            UserVar.WriteUserVar("--uservar SC.ValueDest --value G.U.S.SomeToken --type String");

            // verify the mock
            userVarMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteUser Callback.
        /// </summary>
        [TestMethod]
        public void WriteUserVar_FromUserVarBoolType_Pass()
        {
            // setup the mocks.
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("SC.ValueSrc")).Returns(true);
            userVarMock.Setup(o => o.GetBoolValue("SC.ValueSrc")).Returns(false);
            userVarMock.Setup(o => o.SetValue("SC.ValueDest", false));
            Prime.Services.UserVarService = userVarMock.Object;

            // run the test
            UserVar.WriteUserVar("--uservar SC.ValueDest --value SC.ValueSrc --type Boolean");

            // verify the mock
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteUser Callback.
        /// </summary>
        [TestMethod]
        public void WriteUserVar_FromUserVarDoubleType_Pass()
        {
            // setup the mocks.
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("SC.ValueSrc")).Returns(true);
            userVarMock.Setup(o => o.GetDoubleValue("SC.ValueSrc")).Returns(3.14156);
            userVarMock.Setup(o => o.SetValue("SC.ValueDest", 3.14156));
            Prime.Services.UserVarService = userVarMock.Object;

            // run the test
            UserVar.WriteUserVar("--uservar SC.ValueDest --value SC.ValueSrc --type Double");

            // verify the mock
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteUser Callback.
        /// </summary>
        [TestMethod]
        public void WriteUserVar_FromUserVarIntegerType_Pass()
        {
            // setup the mocks.
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("SC.ValueSrc")).Returns(true);
            userVarMock.Setup(o => o.GetIntValue("SC.ValueSrc")).Returns(42);
            userVarMock.Setup(o => o.SetValue("SC.ValueDest", 42));
            Prime.Services.UserVarService = userVarMock.Object;

            // run the test
            UserVar.WriteUserVar("--uservar SC.ValueDest --value SC.ValueSrc --type Integer");

            // verify the mock
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteUser Callback.
        /// </summary>
        [TestMethod]
        public void WriteUserVar_FromUserVarStringType_Pass()
        {
            // setup the mocks.
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("SC.ValueSrc")).Returns(true);
            userVarMock.Setup(o => o.GetStringValue("SC.ValueSrc")).Returns("blah");
            userVarMock.Setup(o => o.SetValue("SC.ValueDest", "blah"));
            Prime.Services.UserVarService = userVarMock.Object;

            // run the test
            UserVar.WriteUserVar("--uservar SC.ValueDest --value SC.ValueSrc --type String");

            // verify the mock
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteUser Callback.
        /// </summary>
        [TestMethod]
        public void WriteUserVar_FromUserVarArrayBoolType_Pass()
        {
            // setup the mocks.
            var values = new List<bool> { true, true, false, true };
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("SC.ValueSrc")).Returns(true);
            userVarMock.Setup(o => o.GetArrayBoolValue("SC.ValueSrc")).Returns(values);
            userVarMock.Setup(o => o.SetValue("SC.ValueDest", values));
            Prime.Services.UserVarService = userVarMock.Object;

            // run the test
            UserVar.WriteUserVar("--uservar SC.ValueDest --value SC.ValueSrc --type ArrayBoolean");

            // verify the mock
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteUser Callback.
        /// </summary>
        [TestMethod]
        public void WriteUserVar_FromUserVarArrayDoubleType_Pass()
        {
            // setup the mocks.
            var values = new List<double> { 1.5, 3.7 };
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("SC.ValueSrc")).Returns(true);
            userVarMock.Setup(o => o.GetArrayDoubleValue("SC.ValueSrc")).Returns(values);
            userVarMock.Setup(o => o.SetValue("SC.ValueDest", values));
            Prime.Services.UserVarService = userVarMock.Object;

            // run the test
            UserVar.WriteUserVar("--uservar SC.ValueDest --value SC.ValueSrc --type ArrayDouble");

            // verify the mock
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteUser Callback.
        /// </summary>
        [TestMethod]
        public void WriteUserVar_FromUserVarArrayIntegerType_Pass()
        {
            // setup the mocks.
            var values = new List<int> { 1, 1, 2, 3, 5, 8, 13 };
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("SC.ValueSrc")).Returns(true);
            userVarMock.Setup(o => o.GetArrayIntValue("SC.ValueSrc")).Returns(values);
            userVarMock.Setup(o => o.SetValue("SC.ValueDest", values));
            Prime.Services.UserVarService = userVarMock.Object;

            // run the test
            UserVar.WriteUserVar("--uservar SC.ValueDest --value SC.ValueSrc --type ArrayInteger");

            // verify the mock
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteUser Callback.
        /// </summary>
        [TestMethod]
        public void WriteUserVar_FromUserVarArrayStringType_Pass()
        {
            // setup the mocks.
            var values = new List<string> { "Apple", "Betty", "Car" };
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("SC.ValueSrc")).Returns(true);
            userVarMock.Setup(o => o.GetArrayStringValue("SC.ValueSrc")).Returns(values);
            userVarMock.Setup(o => o.SetValue("SC.ValueDest", values));
            Prime.Services.UserVarService = userVarMock.Object;

            // run the test
            UserVar.WriteUserVar("--uservar SC.ValueDest --value SC.ValueSrc --type ArrayString");

            // verify the mock
            userVarMock.VerifyAll();
        }
    }
}