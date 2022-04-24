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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.PatConfigService;
    using Prime.PerformanceService;

    /// <summary>
    /// Defines the <see cref="Threading_UnitTest" />.
    /// </summary>
    [TestClass]
    public class Threading_UnitTest
    {
        /// <summary>
        /// Setup common mocks.
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
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void BackgroundPatConfigSetpoint_StartAndWait_Pass()
        {
            var patConfigMock1 = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            patConfigMock1.Setup(o => o.ApplySetPoint("SP1"));
            var patConfigMock2 = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            patConfigMock2.Setup(o => o.ApplySetPoint("SP2"));

            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(o => o.GetSetPointHandle("MODULE1", "GROUP1")).Returns(patConfigMock1.Object);
            patConfigServiceMock.Setup(o => o.GetSetPointHandle("MODULE2", "GROUP2")).Returns(patConfigMock2.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var strgvalWriterMock1 = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strgvalWriterMock1.SetupSequence(o => o.SetTnamePostfix("|MODULE1:GROUP1:SP1:global"));
            strgvalWriterMock1.SetupSequence(o => o.SetData("complete"));

            var strgvalWriterMock2 = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strgvalWriterMock2.SetupSequence(o => o.SetTnamePostfix("|MODULE2:GROUP2:SP2:global"));
            strgvalWriterMock2.SetupSequence(o => o.SetData("complete"));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.SetupSequence(o => o.GetItuffStrgvalWriter())
                .Returns(strgvalWriterMock1.Object)
                .Returns(strgvalWriterMock2.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(strgvalWriterMock1.Object));
            datalogServiceMock.Setup(o => o.WriteToItuff(strgvalWriterMock2.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var contextMock = new Mock<TOSUserSDK.IDUTs>(MockBehavior.Strict);
            contextMock.Setup(o => o.Associate());
            contextMock.Setup(o => o.GetCurrentDutId()).Returns("DUT1");
            contextMock.Setup(o => o.GetCurrentIpName()).Returns(string.Empty);

            var dutServiceMock = new Mock<TOSUserSDK.IDUTService>(MockBehavior.Strict);
            dutServiceMock.Setup(o => o.GetContext()).Returns(contextMock.Object);
            TOSUserSDK.DUTs.Service = dutServiceMock.Object;

            Threading.BackgroundPatConfigSetpoint("MODULE1:GROUP1:SP1:global,MODULE2:GROUP2:SP2:global");
            Assert.AreEqual("1", Threading.BackgroundWait(string.Empty));

            contextMock.VerifyAll();
            dutServiceMock.VerifyAll();
            patConfigMock1.VerifyAll();
            patConfigMock2.VerifyAll();
            patConfigServiceMock.VerifyAll();
            strgvalWriterMock1.VerifyAll();
            strgvalWriterMock2.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void BackgroundPatConfigSetpoint_BadAssociate_Fail()
        {
            var strgvalWriterMock1 = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strgvalWriterMock1.SetupSequence(o => o.SetTnamePostfix("|FAILED"));
            strgvalWriterMock1.SetupSequence(o => o.SetData("Thread=[DUT1_]_Failed_context.Associate()_thread_did_not_run."));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strgvalWriterMock1.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(strgvalWriterMock1.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var contextMock = new Mock<TOSUserSDK.IDUTs>(MockBehavior.Strict);
            contextMock.Setup(o => o.Associate()).Throws(new Exception("blah"));
            contextMock.Setup(o => o.GetCurrentDutId()).Returns("DUT1");
            contextMock.Setup(o => o.GetCurrentIpName()).Returns(string.Empty);

            var dutServiceMock = new Mock<TOSUserSDK.IDUTService>(MockBehavior.Strict);
            dutServiceMock.Setup(o => o.GetContext()).Returns(contextMock.Object);
            TOSUserSDK.DUTs.Service = dutServiceMock.Object;

            Threading.BackgroundPatConfigSetpoint("MODULE1:GROUP1:SP1");
            Assert.AreEqual("0", Threading.BackgroundWait(string.Empty));

            contextMock.VerifyAll();
            dutServiceMock.VerifyAll();
            strgvalWriterMock1.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void BackgroundPatConfigSetpoint_SetPointError_Fail()
        {
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(o => o.GetSetPointHandle("MODULE1", "GROUP1")).Throws(new Prime.Base.Exceptions.FatalException("blah"));
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var strgvalWriterMock1 = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strgvalWriterMock1.SetupSequence(o => o.SetTnamePostfix("|FAILED"));
            strgvalWriterMock1.SetupSequence(o => o.SetData("Exception|blah"));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strgvalWriterMock1.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(strgvalWriterMock1.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var contextMock = new Mock<TOSUserSDK.IDUTs>(MockBehavior.Strict);
            contextMock.Setup(o => o.Associate());
            contextMock.Setup(o => o.GetCurrentDutId()).Returns("DUT1");
            contextMock.Setup(o => o.GetCurrentIpName()).Returns(string.Empty);

            var dutServiceMock = new Mock<TOSUserSDK.IDUTService>(MockBehavior.Strict);
            dutServiceMock.Setup(o => o.GetContext()).Returns(contextMock.Object);
            TOSUserSDK.DUTs.Service = dutServiceMock.Object;

            Threading.BackgroundPatConfigSetpoint("MODULE1:GROUP1:SP1:global");
            Assert.AreEqual("0", Threading.BackgroundWait(string.Empty));

            contextMock.VerifyAll();
            dutServiceMock.VerifyAll();
            patConfigServiceMock.VerifyAll();
            strgvalWriterMock1.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void BackgroundPatConfigSetpoint_JoinTimout_Fail()
        {
            var patConfigMock1 = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            patConfigMock1.Setup(o => o.ApplySetPoint("SP1")).Callback(() => { System.Threading.Thread.Sleep(1000); });

            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(o => o.GetSetPointHandle("MODULE1", "GROUP1")).Returns(patConfigMock1.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var contextMock = new Mock<TOSUserSDK.IDUTs>(MockBehavior.Strict);
            contextMock.Setup(o => o.Associate());
            contextMock.Setup(o => o.GetCurrentDutId()).Returns("DUT1");
            contextMock.Setup(o => o.GetCurrentIpName()).Returns(string.Empty);

            var dutServiceMock = new Mock<TOSUserSDK.IDUTService>(MockBehavior.Strict);
            dutServiceMock.Setup(o => o.GetContext()).Returns(contextMock.Object);
            TOSUserSDK.DUTs.Service = dutServiceMock.Object;

            Threading.BackgroundPatConfigSetpoint("MODULE1:GROUP1:SP1:global");
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => Threading.BackgroundWait("1"));
            Assert.AreEqual("Thread=[DUT1_] failed to complete in 1mS.", ex.Message);

            contextMock.VerifyAll();
            dutServiceMock.VerifyAll();
            patConfigMock1.VerifyAll();
            patConfigServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void BackgroundWait_NoThread_Pass()
        {
            var contextMock = new Mock<TOSUserSDK.IDUTs>(MockBehavior.Strict);
            contextMock.Setup(o => o.GetCurrentDutId()).Returns("DUTNONE");
            contextMock.Setup(o => o.GetCurrentIpName()).Returns(string.Empty);

            var dutServiceMock = new Mock<TOSUserSDK.IDUTService>(MockBehavior.Strict);
            dutServiceMock.Setup(o => o.GetContext()).Returns(contextMock.Object);
            TOSUserSDK.DUTs.Service = dutServiceMock.Object;

            Assert.AreEqual("1", Threading.BackgroundWait(string.Empty));

            contextMock.VerifyAll();
            dutServiceMock.VerifyAll();
        }
    }
}
