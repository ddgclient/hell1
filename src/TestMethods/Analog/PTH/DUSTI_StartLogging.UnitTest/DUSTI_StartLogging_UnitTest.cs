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

namespace DUSTI_StartLogging.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.PinService;
    using Prime.TesterService;
    using Prime.TestProgramService;
    using Prime.UserVarService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class DUSTI_StartLogging_UnitTest
    {
        private Mock<ITesterService> testServiceMock;
        private Mock<ITestProgramService> testProgramServiceMock;
        private Mock<IFileService> fileServiceMock;

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>()))
                .Callback((string message) => System.Console.WriteLine("DEBUG: " + message));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            this.testServiceMock = new Mock<ITesterService>(MockBehavior.Strict);
            Prime.Services.TesterService = this.testServiceMock.Object;
            this.testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            Prime.Services.TestProgramService = this.testProgramServiceMock.Object;

            this.fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            this.fileServiceMock.Setup(o => o.FileExists(It.IsAny<string>())).Returns((string fileName) => System.IO.File.Exists("../../src/TestMethods/Analog/PTH/DUSTI_Configure.UnitTest/" + fileName));
            this.fileServiceMock.Setup(o => o.GetFile(It.IsAny<string>())).Returns((string fileName) => "../../src/TestMethods/Analog/PTH/DUSTI_Configure.UnitTest/" + fileName);
            Prime.Services.FileService = this.fileServiceMock.Object;
        }

        /// <summary>
        /// Verify that the Levels parameters is set.
        /// </summary>
        [TestMethod]
        public void Verify_Levels_Present_Fail()
        {
            DUSTI_StartLogging testing = new DUSTI_StartLogging()
            {
                LevelsOption = string.Empty,
                PinName = "I2C_00",
                ForceFlow = "False",
                AttemptCount = "1",
                AckWaitTime = "10",
                FpgaWaitTime = "10",
            };

            // [2] Call the method under test.
            Assert.ThrowsException<ArgumentException>(() => testing.Verify());
        }

        /// <summary>
        /// Verify that the PinName parameters is set.
        /// </summary>
        [TestMethod]
        public void Verify_PinName_Present_Fail()
        {
            DUSTI_StartLogging testing = new DUSTI_StartLogging()
            {
                LevelsOption = "levels",
                PinName = string.Empty,
                ForceFlow = "False",
                AttemptCount = "1",
                AckWaitTime = "10",
                FpgaWaitTime = "10",
            };

            // [2] Call the method under test.
            Assert.ThrowsException<ArgumentException>(() => testing.Verify());
        }

        /// <summary>
        /// Verify that the Levels parameters is set.
        /// </summary>
        [TestMethod]
        public void Verify_ForceFLow_Present_Fail()
        {
            DUSTI_StartLogging testing = new DUSTI_StartLogging()
            {
                LevelsOption = "levels",
                PinName = "I2C_00",
                ForceFlow = string.Empty,
                AttemptCount = "1",
                AckWaitTime = "10",
                FpgaWaitTime = "10",
            };

            // [2] Call the method under test.
            Assert.ThrowsException<ArgumentException>(() => testing.Verify());
        }

        /// <summary>
        /// Verify that the Levels parameters is set.
        /// </summary>
        [TestMethod]
        public void Verify_Pass()
        {
            DUSTI_StartLogging testing = new DUSTI_StartLogging()
            {
                LevelsOption = "levels",
                PinName = "I2C_00",
                ForceFlow = "False",
                AttemptCount = "1",
                AckWaitTime = "10",
                FpgaWaitTime = "10",
            };

            // [2] Call the method under test.
            testing.Verify();
        }

        /// <summary>
        /// Executing mode 2.
        /// </summary>
        [TestMethod]
        public void Execute_Mode2()
        {
            DUSTI_StartLogging testing = new DUSTI_StartLogging()
            {
                LevelsOption = "Levels",
                PinName = "I2C_00",
                ForceFlow = "False",
                AttemptCount = "1",
                AckWaitTime = "10",
                FpgaWaitTime = "10",
            };

            List<byte> fake = new List<byte>();
            List<byte> fake1 = new List<byte>();
            List<byte> fake2 = new List<byte>();
            fake.Insert(0, Convert.ToByte(0x3));
            fake.Insert(1, Convert.ToByte(0x3));
            fake1.Insert(0, Convert.ToByte(0x1));
            fake1.Insert(1, Convert.ToByte(0x65));
            fake2.Insert(0, Convert.ToByte(0x1));
            fake2.Insert(1, Convert.ToByte(0x65));
            var sequence = new MockSequence();

            // [2] Call the method under test.
            /* this.consoleServiceMock.Setup(console => console.PrintDebug("No XML Input. \n"));*/
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            tpFunctionsMock.Setup(o => o.GetCurrentDutId()).Returns("0");
            Prime.Services.TestProgramService = tpFunctionsMock.Object;
            var userFuncMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userFuncMock.Setup(u => u.GetStringValue(It.IsAny<string>(), It.IsAny<string>())).Returns("12");
            Prime.Services.UserVarService = userFuncMock.Object;
            testing.Verify();
            var i2cServiceMock = new Mock<ITesterService>(MockBehavior.Loose);
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake);
            i2cServiceMock.InSequence(sequence).Setup(v => v.WriteI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<List<byte>>(), It.IsAny<bool>()));
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake);
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake1);
            Prime.Services.TesterService = i2cServiceMock.Object;
            Dictionary<string, string> meh = new Dictionary<string, string>();
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Loose);
            pinServiceMock.Setup(w => w.SetPinAttributeValues("Pin", meh));
            Prime.Services.PinService = pinServiceMock.Object;
            var getDefinedMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            getDefinedMock.Setup(r => r.GetDefinedDutsIndex()).Returns(It.IsAny<List<uint>>);
            var tpConsoleMock = new Mock<IConsoleService>(MockBehavior.Loose);
            tpConsoleMock.Setup(t => t.PrintDebug(" "));

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            int executeResult = testing.Execute();
            Assert.AreEqual(1, executeResult);
        }

        /// <summary>
        /// Executing mode 2 with empty coutnts.
        /// </summary>
        [TestMethod]
        public void Execute_Mode2_Empty_Counts()
        {
            DUSTI_StartLogging testing = new DUSTI_StartLogging()
            {
                LevelsOption = "Levels",
                PinName = "I2C_00",
                ForceFlow = "False",
                AttemptCount = string.Empty,
                AckWaitTime = string.Empty,
                FpgaWaitTime = string.Empty,
            };

            List<byte> fake = new List<byte>();
            List<byte> fake1 = new List<byte>();
            List<byte> fake2 = new List<byte>();
            fake.Insert(0, Convert.ToByte(0x3));
            fake.Insert(1, Convert.ToByte(0x3));
            fake1.Insert(0, Convert.ToByte(0x1));
            fake1.Insert(1, Convert.ToByte(0x65));
            fake2.Insert(0, Convert.ToByte(0x1));
            fake2.Insert(1, Convert.ToByte(0x65));
            var sequence = new MockSequence();

            // [2] Call the method under test.
            /* this.consoleServiceMock.Setup(console => console.PrintDebug("No XML Input. \n"));*/
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            tpFunctionsMock.Setup(o => o.GetCurrentDutId()).Returns("0");
            Prime.Services.TestProgramService = tpFunctionsMock.Object;
            var userFuncMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userFuncMock.Setup(u => u.GetStringValue(It.IsAny<string>(), It.IsAny<string>())).Returns("12");
            Prime.Services.UserVarService = userFuncMock.Object;
            testing.Verify();
            var i2cServiceMock = new Mock<ITesterService>(MockBehavior.Loose);
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake);
            i2cServiceMock.InSequence(sequence).Setup(v => v.WriteI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<List<byte>>(), It.IsAny<bool>()));
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake);
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake1);
            Prime.Services.TesterService = i2cServiceMock.Object;
            Dictionary<string, string> meh = new Dictionary<string, string>();
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Loose);
            pinServiceMock.Setup(w => w.SetPinAttributeValues("Pin", meh));
            Prime.Services.PinService = pinServiceMock.Object;
            var getDefinedMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            getDefinedMock.Setup(r => r.GetDefinedDutsIndex()).Returns(It.IsAny<List<uint>>);
            var tpConsoleMock = new Mock<IConsoleService>(MockBehavior.Loose);
            tpConsoleMock.Setup(t => t.PrintDebug(" "));

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            int executeResult = testing.Execute();
            Assert.AreEqual(1, executeResult);
        }

        /// <summary>
        /// Executing mode 2 with Force Flow.
        /// </summary>
        [TestMethod]
        public void Execute_Mode2_ForceFlow()
        {
            DUSTI_StartLogging testing = new DUSTI_StartLogging()
            {
                LevelsOption = "Levels",
                PinName = "I2C_00",
                ForceFlow = "True",
                AttemptCount = "1",
                AckWaitTime = "10",
                FpgaWaitTime = "10",
            };

            List<byte> fake = new List<byte>();
            fake.Insert(0, Convert.ToByte(0x3));
            fake.Insert(1, Convert.ToByte(0x3));
            var sequence = new MockSequence();

            // [2] Call the method under test.
            /* this.consoleServiceMock.Setup(console => console.PrintDebug("No XML Input. \n"));*/
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            tpFunctionsMock.Setup(o => o.GetCurrentDutId()).Returns("0");
            Prime.Services.TestProgramService = tpFunctionsMock.Object;
            var userFuncMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userFuncMock.Setup(u => u.GetStringValue(It.IsAny<string>(), It.IsAny<string>())).Returns("123");
            Prime.Services.UserVarService = userFuncMock.Object;
            testing.Verify();
            var i2cServiceMock = new Mock<ITesterService>(MockBehavior.Loose);
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake);
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake);
            fake.Insert(0, Convert.ToByte(0x60));
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake);
            Prime.Services.TesterService = i2cServiceMock.Object;
            Dictionary<string, string> meh = new Dictionary<string, string>();
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Loose);
            pinServiceMock.Setup(w => w.SetPinAttributeValues("Pin", meh));
            Prime.Services.PinService = pinServiceMock.Object;
            var getDefinedMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            getDefinedMock.Setup(r => r.GetDefinedDutsIndex()).Returns(It.IsAny<List<uint>>);
            var tpConsoleMock = new Mock<IConsoleService>(MockBehavior.Loose);
            tpConsoleMock.Setup(t => t.PrintDebug(" "));

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            int executeResult = testing.Execute();
            Assert.AreEqual(2, executeResult);
        }

        /// <summary>
        /// Executing mode 2 with force flow enabled for alternate.
        /// </summary>
        [TestMethod]
        public void Execute_Mode2_ForceFlow_Alt()
        {
            DUSTI_StartLogging testing = new DUSTI_StartLogging()
            {
                LevelsOption = "Levels",
                PinName = "I2C_00",
                ForceFlow = "True",
                AttemptCount = "1",
                AckWaitTime = "2",
                FpgaWaitTime = "2",
            };

            List<byte> fake = new List<byte>();
            List<byte> fake1 = new List<byte>();
            List<byte> fake2 = new List<byte>();
            fake.Insert(0, Convert.ToByte(0x3));
            fake.Insert(1, Convert.ToByte(0x3));
            fake1.Insert(0, Convert.ToByte(0x1));
            fake1.Insert(1, Convert.ToByte(0x66));
            fake2.Insert(0, Convert.ToByte(0x1));
            fake2.Insert(1, Convert.ToByte(0x65));
            var sequence = new MockSequence();

            // [2] Call the method under test.
            /* this.consoleServiceMock.Setup(console => console.PrintDebug("No XML Input. \n"));*/
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            tpFunctionsMock.Setup(o => o.GetCurrentDutId()).Returns("0");
            Prime.Services.TestProgramService = tpFunctionsMock.Object;
            var userFuncMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userFuncMock.Setup(u => u.GetStringValue(It.IsAny<string>(), It.IsAny<string>())).Returns("1234");
            Prime.Services.UserVarService = userFuncMock.Object;
            testing.Verify();
            var i2cServiceMock = new Mock<ITesterService>(MockBehavior.Loose);
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake);
            i2cServiceMock.InSequence(sequence).Setup(v => v.WriteI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<List<byte>>(), It.IsAny<bool>()));
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake);
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake1);
            Prime.Services.TesterService = i2cServiceMock.Object;
            Dictionary<string, string> meh = new Dictionary<string, string>();
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Loose);
            pinServiceMock.Setup(w => w.SetPinAttributeValues("Pin", meh));
            Prime.Services.PinService = pinServiceMock.Object;
            var getDefinedMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            getDefinedMock.Setup(r => r.GetDefinedDutsIndex()).Returns(It.IsAny<List<uint>>);
            var tpConsoleMock = new Mock<IConsoleService>(MockBehavior.Loose);
            tpConsoleMock.Setup(t => t.PrintDebug(" "));

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            int executeResult = testing.Execute();
            Assert.AreEqual(2, executeResult);
        }

        /// <summary>
        /// Executing mode 2, failing I2C.
        /// </summary>
        [TestMethod]
        public void Execute_Mode2_No_I2C()
        {
            DUSTI_StartLogging testing = new DUSTI_StartLogging()
            {
                LevelsOption = "Levels",
                PinName = "I2C_00",
                ForceFlow = "False",
                AttemptCount = "1",
                AckWaitTime = "10",
                FpgaWaitTime = "10",
            };

            List<byte> fake = new List<byte>();
            List<byte> fake1 = new List<byte>();
            List<byte> fake2 = new List<byte>();
            fake.Insert(0, Convert.ToByte(0x3));
            fake.Insert(1, Convert.ToByte(0x3));
            fake1.Insert(0, Convert.ToByte(0x1));
            fake1.Insert(1, Convert.ToByte(0x65));
            fake2.Insert(0, Convert.ToByte(0x1));
            fake2.Insert(1, Convert.ToByte(0x65));
            var sequence = new MockSequence();

            // [2] Call the method under test.
            /* this.consoleServiceMock.Setup(console => console.PrintDebug("No XML Input. \n"));*/
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            tpFunctionsMock.Setup(o => o.GetCurrentDutId()).Returns("0");
            Prime.Services.TestProgramService = tpFunctionsMock.Object;
            var userFuncMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userFuncMock.Setup(u => u.GetStringValue(It.IsAny<string>(), It.IsAny<string>())).Returns("12");
            Prime.Services.UserVarService = userFuncMock.Object;
            testing.Verify();
            var i2cServiceMock = new Mock<ITesterService>(MockBehavior.Loose);
            i2cServiceMock.Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Throws(new Prime.Base.Exceptions.FatalException("Failed to contact I2C"));
            Prime.Services.TesterService = i2cServiceMock.Object;

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            int executeResult = testing.Execute();
            Assert.AreEqual(4, executeResult);
        }

        /// <summary>
        /// Executing mode 2.
        /// </summary>
        [TestMethod]
        public void Execute_Mode2_ForceFail()
        {
            DUSTI_StartLogging testing = new DUSTI_StartLogging()
            {
                LevelsOption = "Levels",
                PinName = "I2C_00",
                ForceFlow = "False",
                AttemptCount = "1",
                AckWaitTime = "10",
                FpgaWaitTime = "10",
            };

            List<byte> fake = new List<byte>();
            List<byte> fake1 = new List<byte>();
            List<byte> fake2 = new List<byte>();
            fake.Insert(0, Convert.ToByte(0x3));
            fake.Insert(1, Convert.ToByte(0x3));
            fake1.Insert(0, Convert.ToByte(0x0));
            fake1.Insert(1, Convert.ToByte(0x65));
            fake2.Insert(0, Convert.ToByte(0x2));
            fake2.Insert(1, Convert.ToByte(0x65));
            var sequence = new MockSequence();

            // [2] Call the method under test.
            /* this.consoleServiceMock.Setup(console => console.PrintDebug("No XML Input. \n"));*/
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            tpFunctionsMock.Setup(o => o.GetCurrentDutId()).Returns("0");
            Prime.Services.TestProgramService = tpFunctionsMock.Object;
            var userFuncMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userFuncMock.Setup(u => u.GetStringValue(It.IsAny<string>(), It.IsAny<string>())).Returns("12");
            Prime.Services.UserVarService = userFuncMock.Object;
            testing.Verify();
            var i2cServiceMock = new Mock<ITesterService>(MockBehavior.Loose);
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake);
            i2cServiceMock.InSequence(sequence).Setup(v => v.WriteI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<List<byte>>(), It.IsAny<bool>()));
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake);
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake2);
            Prime.Services.TesterService = i2cServiceMock.Object;
            Dictionary<string, string> meh = new Dictionary<string, string>();
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Loose);
            pinServiceMock.Setup(w => w.SetPinAttributeValues("Pin", meh));
            Prime.Services.PinService = pinServiceMock.Object;
            var getDefinedMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            getDefinedMock.Setup(r => r.GetDefinedDutsIndex()).Returns(It.IsAny<List<uint>>);
            var tpConsoleMock = new Mock<IConsoleService>(MockBehavior.Loose);
            tpConsoleMock.Setup(t => t.PrintDebug(" "));

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            int executeResult = testing.Execute();
            Assert.AreEqual(0, executeResult);
        }

        /// <summary>
        /// Executing mode 2 in Force Flow, passing.
        /// </summary>
        [TestMethod]
        public void Execute_Mode2_ForceFlowPass()
        {
            DUSTI_StartLogging testing = new DUSTI_StartLogging()
            {
                LevelsOption = "Levels",
                PinName = "I2C_00",
                ForceFlow = "True",
                AttemptCount = "1",
                AckWaitTime = "10",
                FpgaWaitTime = "10",
            };

            List<byte> fake = new List<byte>();
            List<byte> fake1 = new List<byte>();
            List<byte> fake2 = new List<byte>();
            fake.Insert(0, Convert.ToByte(0x3));
            fake.Insert(1, Convert.ToByte(0x3));
            fake1.Insert(0, Convert.ToByte(0x1));
            fake1.Insert(1, Convert.ToByte(0x65));
            fake2.Insert(0, Convert.ToByte(0x1));
            fake2.Insert(1, Convert.ToByte(0x65));
            var sequence = new MockSequence();

            // [2] Call the method under test.
            /* this.consoleServiceMock.Setup(console => console.PrintDebug("No XML Input. \n"));*/
            var tpFunctionsMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            tpFunctionsMock.Setup(o => o.GetCurrentDutId()).Returns("0");
            Prime.Services.TestProgramService = tpFunctionsMock.Object;
            var userFuncMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userFuncMock.Setup(u => u.GetStringValue(It.IsAny<string>(), It.IsAny<string>())).Returns("12");
            Prime.Services.UserVarService = userFuncMock.Object;
            testing.Verify();
            var i2cServiceMock = new Mock<ITesterService>(MockBehavior.Loose);
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake);
            i2cServiceMock.InSequence(sequence).Setup(v => v.WriteI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<List<byte>>(), It.IsAny<bool>()));
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake);
            i2cServiceMock.InSequence(sequence).Setup(v => v.ReadI2cData(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(fake1);
            Prime.Services.TesterService = i2cServiceMock.Object;
            Dictionary<string, string> meh = new Dictionary<string, string>();
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Loose);
            pinServiceMock.Setup(w => w.SetPinAttributeValues("Pin", meh));
            Prime.Services.PinService = pinServiceMock.Object;
            var getDefinedMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            getDefinedMock.Setup(r => r.GetDefinedDutsIndex()).Returns(It.IsAny<List<uint>>);
            var tpConsoleMock = new Mock<IConsoleService>(MockBehavior.Loose);
            tpConsoleMock.Setup(t => t.PrintDebug(" "));

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            int executeResult = testing.Execute();
            Assert.AreEqual(1, executeResult);
        }
    }
}