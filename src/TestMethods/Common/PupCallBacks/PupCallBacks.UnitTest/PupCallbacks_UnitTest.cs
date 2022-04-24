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

namespace PupCallBacks.UnitTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.SharedStorageService;
    using Prime.UserVarService;

    /// <summary>
    /// PupCallback test method's unit test.
    /// </summary>
    [TestClass]
    public class PupCallBacks_UnitTest
    {
        /// <summary>
        /// Vmin Exclude criteria - not eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void VminExcludeCriteria_NotEligibleForTTR()
        {
            var vminBasedExcludeCriteria = "CCPUP1A1AEXVM^1.1|MAX|gt";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("LastFastSearchInstanceStartVoltage", Context.DUT)).Returns("1,2,3,4,5");
            sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("LastFastSearchInstanceStartVoltage", string.Empty, Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(vminBasedExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsFalse(result == "1");
        }

        /// <summary>
        /// Vmin Exclude criteria -  eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void VminExcludeCriteria_EligibleForTTR()
        {
            var vminBasedExcludeCriteria = "CCPUP1A1AEXVM^1.1|MAX|gt";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("LastFastSearchInstanceStartVoltage", Context.DUT)).Returns("1");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(vminBasedExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(result == "1");
        }

        /// <summary>
        /// Vmin Exclude criteria -  eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void VminExcludeCriteria_EligibleForTTR2()
        {
            var vminBasedExcludeCriteria = "CCPUP1A1AEXVM^1.1|MIN|gt";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("LastFastSearchInstanceStartVoltage", Context.DUT)).Returns("1");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(vminBasedExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(result == "1");
        }

        /// <summary>
        /// Vmin Exclude criteria -  eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void VminExcludeCriteria_EligibleForTTR3()
        {
            var vminBasedExcludeCriteria = "CCPUP1A1AEXVM^1.1|AVG|gt";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("LastFastSearchInstanceStartVoltage", Context.DUT)).Returns("1");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(vminBasedExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(result == "1");
        }

        /// <summary>
        /// Vmin Exclude criteria -  eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void VminExcludeCriteria_EligibleForTTR4()
        {
            var vminBasedExcludeCriteria = "CCPUP1A1AEXVM^1.1|MAX|ge";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("LastFastSearchInstanceStartVoltage", Context.DUT)).Returns("1");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(vminBasedExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(result == "1");
        }

        /// <summary>
        /// Vmin Exclude criteria -  eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void VminExcludeCriteria_EligibleForTTR5()
        {
            var vminBasedExcludeCriteria = "CCPUP1A1AEXVM^1.1|RANGE|ge";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("LastFastSearchInstanceStartVoltage", Context.DUT)).Returns("1");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(vminBasedExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(result == "1");
        }

        /// <summary>
        /// Vmin Exclude criteria -  eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void VminExcludeCriteria_EligibleForTTR6()
        {
            var vminBasedExcludeCriteria = "CCPUP1A1AEXVM^0.9|RANGE|le";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("LastFastSearchInstanceStartVoltage", Context.DUT)).Returns("1");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(vminBasedExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(result == "1");
        }

        /// <summary>
        /// Vmin Exclude criteria -  eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void VminExcludeCriteria_EligibleForTTR7()
        {
            var vminBasedExcludeCriteria = "CCPUP1A1AEXVM^0.9|RANGE|lt";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("LastFastSearchInstanceStartVoltage", Context.DUT)).Returns("1");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(vminBasedExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(result == "1");
        }

        /// <summary>
        /// Vmin Exclude criteria -  eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void VminExcludeCriteria_EligibleForTTR_SharedStorage()
        {
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("LastFastSearchInstanceFlow", Context.DUT)).Returns("1");
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("Key", Context.DUT)).Returns("false");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            var vminBasedExcludeCriteria = "SharedStorageKey^dut:Key|MAX|le";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(vminBasedExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(result == "1");
        }

        /// <summary>
        /// Vmin Exclude criteria -  not eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void VminExcludeCriteria_ExcludeValue_SharedStorage()
        {
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("Key", Context.DUT)).Returns("true");
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("LastFastSearchInstanceStartVoltage", Context.DUT)).Returns("1");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            var vminBasedExcludeCriteria = "SharedStorageKey^dut:Key|MAX|le";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(vminBasedExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual("SharedStorageKey:Key", result);
        }

        /// <summary>
        /// Vmin Exclude criteria -  eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void VminExcludeCriteria_EligibleForTTR_Uservar()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            Prime.Services.UserVarService = userVarServiceMock.Object;
            userVarServiceMock.Setup(o => o.Exists("Key")).Returns(true);
            userVarServiceMock.Setup(o => o.GetStringValue("Key")).Returns("false");

            var vminBasedExcludeCriteria = "UserVarName^Key|MAX|eq";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("LastFastSearchInstanceStartVoltage", Context.DUT)).Returns("1");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(vminBasedExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(result == "1");
        }

        /// <summary>
        /// Vmin Exclude criteria -  eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void VminExcludeCriteria_ExcludeValue_Uservar()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            Prime.Services.UserVarService = userVarServiceMock.Object;
            userVarServiceMock.Setup(o => o.Exists("Key")).Returns(true);
            userVarServiceMock.Setup(o => o.GetStringValue("Key")).Returns("true");

            var vminBasedExcludeCriteria = "UserVarName^Key|MAX|eq";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("LastFastSearchInstanceStartVoltage", Context.DUT)).Returns("1");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(vminBasedExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual("UserVarName:Key", result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void VminExcludeCriteria_InvalidFunction_Exception()
        {
            var vminBasedExcludeCriteria = "CCPUP1A1AEXVM^1.1|invalid|ge";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("LastFastSearchInstanceStartVoltage", Context.DUT)).Returns("1");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var ex = Assert.ThrowsException<TestMethodException>(() => PupCallBacks.IsPlistEligibleForTTR(vminBasedExcludeCriteria));
            Assert.AreEqual("Function=[invalid] is an invalid value. Valid values are=[MIN, MAX, RANGE, AVG].", ex.Message);
        }

        /// <summary>
        /// Base number Exclude criteria - not eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void BaseNumberExcludeCriteria_NotEligibleForTTR()
        {
            var baseNumberExcludeCriteria = "CCPUP1A1AEXBN^2210*49*2212";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetIntegerRowFromTable("PUP_BASE_NUMBER_2212", Context.DUT)).Returns(1);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(baseNumberExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsFalse(result == "1");
        }

        /// <summary>
        /// Base number Exclude criteria -  eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void BaseNumberExcludeCriteria_EligibleForTTR()
        {
            var baseNumberExcludeCriteria = "CCPUP1A1AEXBN^2210*49*2212";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;
            Exception exception = new Exception($"GetLoopConfig threw an exception.");

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetIntegerRowFromTable("PUP_BASE_NUMBER_2212", Context.DUT)).Throws(exception);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(baseNumberExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(result == "1");
        }

        /// <summary>
        /// Flow Id Exclude criteria - not eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void FlowIdExcludeCriteria_NotEligibleForTTR()
        {
            var flowIdExcludeCriteria = "CCPUP1A1AEXFN^1";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Loose);
            sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("LastFastSearchInstanceFlow", -55555, Context.DUT));
            sharedStorageServiceMock.Setup(o => o.GetIntegerRowFromTable("LastFastSearchInstanceFlow", Context.DUT)).Returns(2);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(flowIdExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsFalse(result == "1");
        }

        /// <summary>
        /// Flow ID Exclude criteria -  eligible for TTR test.
        /// </summary>
        [TestMethod]
        public void FlowIdExcludeCriteria_EligibleForTTR()
        {
            var flowIdExcludeCriteria = "CCPUP1A1AEXFN^1";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(console => console.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n===="));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.GetIntegerRowFromTable("LastFastSearchInstanceFlow", Context.DUT)).Returns(1);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // [2] Call the method under test.
            var result = PupCallBacks.IsPlistEligibleForTTR(flowIdExcludeCriteria);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.IsTrue(result == "1");
        }
    }
}
