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

namespace BackgroundPatConfig.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.PatConfigService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class BackgroundPatConfig_UnitTest
    {
        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>()))
                .Callback((string s) => Console.WriteLine(s));
            consoleServiceMock
                .Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string n, string src) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Verify_BadArgs_Exception()
        {
            this.TestVerifyException(mode: BackgroundPatConfig.ModeType.Start, setpoints: string.Empty, -1, "Parameter=[PatConfigSetpointList] is required when Mode=[Start].");
            this.TestVerifyException(mode: BackgroundPatConfig.ModeType.StartAndWait, setpoints: string.Empty, -1, "Parameter=[PatConfigSetpointList] is required when Mode=[Start].");
            this.TestVerifyException(mode: BackgroundPatConfig.ModeType.Wait, setpoints: "blah:blah:blah", -1, "Parameter=[WaitTimeout] is required to be positive when Mode=[Wait].");
            this.TestVerifyException(mode: BackgroundPatConfig.ModeType.StartAndWait, setpoints: "blah:blah:blah", -1, "Parameter=[WaitTimeout] is required to be positive when Mode=[Wait].");
            this.TestVerifyException(mode: BackgroundPatConfig.ModeType.Start, setpoints: "blah:blah", 2000, "Error in PatConfigSetpoint Format=[blah:blah]. Expecting [MODULE:GROUP:SETPOINT].");
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Verify_Pass()
        {
            var dummyHandleMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            var patconfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patconfigServiceMock.Setup(o => o.GetSetPointHandle("mod1", "group1")).Returns(dummyHandleMock.Object);
            patconfigServiceMock.Setup(o => o.GetSetPointHandle("mod2", "group2")).Returns(dummyHandleMock.Object);
            Prime.Services.PatConfigService = patconfigServiceMock.Object;

            var undertest = new BackgroundPatConfig
            {
                Mode = BackgroundPatConfig.ModeType.Start,
                PatConfigSetpointList = "mod1:group1:sp1,mod2:group2:sp2",
            };

            undertest.Verify();
            patconfigServiceMock.VerifyAll();
            dummyHandleMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_WaitForNothing_Pass()
        {
            var patconfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            Prime.Services.PatConfigService = patconfigServiceMock.Object;
            var contextMocks = this.MockContext("dut1", "ip1", associate: false);

            var undertest = new BackgroundPatConfig
            {
                Mode = BackgroundPatConfig.ModeType.Wait,
                WaitTimeout = 3000,
            };

            undertest.Verify();
            Assert.AreEqual(1, undertest.Execute());

            patconfigServiceMock.VerifyAll();
            contextMocks.ForEach(m => m.VerifyAll());
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StartAndWait_Pass()
        {
            var sp1 = new Tuple<string, string, string>("mod1", "group1", "sp1");
            var sp2 = new Tuple<string, string, string>("mod2", "group2", "sp2");
            var patconfigMocks = this.MockPatConfigHandles(new List<Tuple<string, string, string>> { sp1, sp2 });
            var contextMocks = this.MockContext("dut1", "ip1");

            var undertest = new BackgroundPatConfig
            {
                Mode = BackgroundPatConfig.ModeType.StartAndWait,
                PatConfigSetpointList = "mod1:group1:sp1,mod2:group2:sp2",
                WaitTimeout = 3000,
            };

            undertest.Verify();
            Assert.AreEqual(1, undertest.Execute());

            patconfigMocks.ForEach(m => m.VerifyAll());
            contextMocks.ForEach(m => m.VerifyAll());
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StartAndWait_FailToStop()
        {
            var sp1 = new Tuple<string, string, string>("mod1", "group1", "sp1");
            var sp2 = new Tuple<string, string, string>("mod2", "group2", "sp2");
            var patconfigMocks = this.MockPatConfigHandles(new List<Tuple<string, string, string>> { sp1, sp2 }, delay: 100);
            var contextMocks = this.MockContext("dut1", "ip1");

            var undertest = new BackgroundPatConfig
            {
                Mode = BackgroundPatConfig.ModeType.StartAndWait,
                PatConfigSetpointList = "mod1:group1:sp1,mod2:group2:sp2",
                WaitTimeout = 6,
            };

            undertest.Verify();
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => undertest.Execute());
            Assert.AreEqual("Thread=[dut1_ip1] failed to complete in 6mS.", ex.Message);
            System.Threading.Thread.Sleep(200);
            patconfigMocks.ForEach(m => m.VerifyAll());
            contextMocks.ForEach(m => m.VerifyAll());
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StartAndWait_FailAssociate()
        {
            var dummyHandleMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            var patconfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patconfigServiceMock.Setup(o => o.GetSetPointHandle("mod1", "group1")).Returns(dummyHandleMock.Object);
            patconfigServiceMock.Setup(o => o.GetSetPointHandle("mod2", "group2")).Returns(dummyHandleMock.Object);
            Prime.Services.PatConfigService = patconfigServiceMock.Object;
            var ituffMocks = this.MockDatalog("|FAILED", "Thread=[dut1_ip1]_Failed_context.Associate()_thread_did_not_run.");

            var dutMock = new Mock<TOSUserSDK.IDUTs>(MockBehavior.Strict);
            dutMock.Setup(o => o.Associate()).Throws(new Exception("error"));
            dutMock.Setup(o => o.GetCurrentDutId()).Returns("dut1");
            dutMock.Setup(o => o.GetCurrentIpName()).Returns("ip1");

            var dutServiceMock = new Mock<TOSUserSDK.IDUTService>(MockBehavior.Strict);
            dutServiceMock.Setup(o => o.GetContext()).Returns(dutMock.Object);
            TOSUserSDK.DUTs.Service = dutServiceMock.Object;

            var undertest = new BackgroundPatConfig
            {
                Mode = BackgroundPatConfig.ModeType.StartAndWait,
                PatConfigSetpointList = "mod1:group1:sp1,mod2:group2:sp2",
                WaitTimeout = 3000,
            };

            undertest.Verify();
            Assert.AreEqual(0, undertest.Execute());

            ituffMocks.ForEach(m => m.VerifyAll());
            dummyHandleMock.VerifyAll();
            patconfigServiceMock.VerifyAll();
            dutMock.VerifyAll();
            dutServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StartAndWait_FailPatConfig()
        {
            var contextMocks = this.MockContext("dut1", "ip1");
            var ituffMocks = this.MockDatalog("|FAILED", "Exception|something_went_wrong|error_details");

            var spHandleMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            spHandleMock.Setup(o => o.ApplySetPoint("sp1")).Throws(new Exception("something went wrong\nerror details"));

            var patconfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patconfigServiceMock.Setup(o => o.GetSetPointHandle("mod1", "group1")).Returns(spHandleMock.Object);
            Prime.Services.PatConfigService = patconfigServiceMock.Object;

            var undertest = new BackgroundPatConfig
            {
                Mode = BackgroundPatConfig.ModeType.StartAndWait,
                PatConfigSetpointList = "mod1:group1:sp1",
                WaitTimeout = 3000,
            };

            undertest.Verify();
            Assert.AreEqual(0, undertest.Execute());

            ituffMocks.ForEach(m => m.VerifyAll());
            patconfigServiceMock.VerifyAll();
            spHandleMock.VerifyAll();
            contextMocks.ForEach(m => m.VerifyAll());
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StartThenWait_Pass()
        {
            var sp1 = new Tuple<string, string, string>("mod1", "group1", "sp1");
            var sp2 = new Tuple<string, string, string>("mod2", "group2", "sp2");
            var patconfigMocks = this.MockPatConfigHandles(new List<Tuple<string, string, string>> { sp1, sp2 });
            var contextMocks = this.MockContext("dut1", "ip1");

            var undertestStart = new BackgroundPatConfig
            {
                Mode = BackgroundPatConfig.ModeType.Start,
                PatConfigSetpointList = "mod1:group1:sp1,mod2:group2:sp2",
            };

            undertestStart.Verify();
            Assert.AreEqual(1, undertestStart.Execute());

            var undertestWait = new BackgroundPatConfig
            {
                Mode = BackgroundPatConfig.ModeType.Wait,
                WaitTimeout = 3000,
            };

            undertestWait.Verify();
            Assert.AreEqual(1, undertestWait.Execute());

            patconfigMocks.ForEach(m => m.VerifyAll());
            contextMocks.ForEach(m => m.VerifyAll());
        }

        private void TestVerifyException(BackgroundPatConfig.ModeType mode, string setpoints, int waittime, string errormessage)
        {
            var undertest = new BackgroundPatConfig
            {
                Mode = mode,
                PatConfigSetpointList = setpoints,
                WaitTimeout = waittime,
            };

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => undertest.Verify());
            Assert.AreEqual(errormessage, ex.Message);
        }

        private List<Mock> MockPatConfigHandles(List<Tuple<string, string, string>> moduleGroupSetpoints, int delay = 0)
        {
            var patconfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            var allmocks = new List<Mock> { patconfigServiceMock };
            foreach (var moduleGroupSetpoint in moduleGroupSetpoints)
            {
                var spHandleMock = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
                spHandleMock.Setup(o => o.ApplySetPoint(moduleGroupSetpoint.Item3)).Callback(() => System.Threading.Thread.Sleep(delay));
                patconfigServiceMock.Setup(o => o.GetSetPointHandle(moduleGroupSetpoint.Item1, moduleGroupSetpoint.Item2)).Returns(spHandleMock.Object);
                allmocks.Add(spHandleMock);
            }

            Prime.Services.PatConfigService = patconfigServiceMock.Object;
            return allmocks;
        }

        private List<Mock> MockContext(string dut, string ip, bool associate = true)
        {
            var dutMock = new Mock<TOSUserSDK.IDUTs>(MockBehavior.Strict);
            dutMock.Setup(o => o.GetCurrentDutId()).Returns("dut1");
            dutMock.Setup(o => o.GetCurrentIpName()).Returns("ip1");
            if (associate)
            {
                dutMock.Setup(o => o.Associate());
            }

            var dutServiceMock = new Mock<TOSUserSDK.IDUTService>(MockBehavior.Strict);
            dutServiceMock.Setup(o => o.GetContext()).Returns(dutMock.Object);
            TOSUserSDK.DUTs.Service = dutServiceMock.Object;

            return new List<Mock> { dutMock, dutServiceMock };
        }

        private List<Mock> MockDatalog(string postfix, string data)
        {
            var writer = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writer.Setup(o => o.SetTnamePostfix(postfix));
            writer.Setup(o => o.SetData(data));

            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writer.Object);
            datalogMock.Setup(o => o.WriteToItuff(writer.Object));
            Prime.Services.DatalogService = datalogMock.Object;

            return new List<Mock> { writer, datalogMock };
        }
    }
}
