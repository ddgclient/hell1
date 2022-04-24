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
    using Prime.DffService;
    using Prime.PerformanceService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="DffCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class DffCallbacks_UnitTest
    {
        private Mock<IDffService> dffServiceMock;
        private Mock<ISharedStorageService> sharedStorageServiceMock;

        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            var testProgramMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramMock.Setup(o => o.GetCurrentTestInstanceName()).Returns("DummyTestInstance");
            Prime.Services.TestProgramService = testProgramMock.Object;

            var consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleMock.Setup(o => o.PrintDebug(It.IsAny<string>()))
                .Callback((string message) => System.Console.WriteLine("DEBUG " + message));
            consoleMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string message, int line, string member, string file) => System.Console.WriteLine("ERROR " + message));
            Prime.Services.ConsoleService = consoleMock.Object;

            this.dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            Prime.Services.DffService = this.dffServiceMock.Object;

            this.sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = this.sharedStorageServiceMock.Object;

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void WriteDff_Pass()
        {
            // setup mocks.
            this.dffServiceMock.Setup(o => o.SetDff("tokenName1", "tokenValue1"));
            this.dffServiceMock.Setup(o => o.SetDff("tokenName2", "tokenValue2", "die2"));

            // Write all the gsds values.
            Dff.WriteDff("--token tokenName1 --value tokenValue1");
            Dff.WriteDff("--token tokenName2 --value tokenValue2 --targetdie die2");

            this.dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void WriteDff_Fail()
        {
            // setup mocks.
            // Write all the gsds values.
            Assert.ThrowsException<ArgumentException>(() => Dff.WriteDff("--token tokenName1 --invalid tokenValue1"));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void PrintDff_Pass()
        {
            // setup mocks.
            this.dffServiceMock.Setup(o => o.GetDff("tokenName1", true)).Returns("value1");
            this.dffServiceMock.Setup(o => o.GetDff("tokenName2", true)).Returns("value2");
            this.dffServiceMock.Setup(o => o.GetDffByOpType("tokenName1", "optype", true)).Returns("value1");
            this.dffServiceMock.Setup(o => o.GetDffByOpType("tokenName2", "optype", true)).Returns("value2");
            this.dffServiceMock.Setup(o => o.GetDffByDieId("tokenName1", "die", true)).Returns("value1");
            this.dffServiceMock.Setup(o => o.GetDffByDieId("tokenName2", "die", true)).Returns("value2");
            this.dffServiceMock.Setup(o => o.GetDff("tokenName1", "optype", "die", true)).Returns("value1");
            this.dffServiceMock.Setup(o => o.GetDff("tokenName2", "optype", "die", true)).Returns("value2");

            // Write all the gsds values.
            Dff.PrintDff(string.Empty);
            Dff.PrintDff("--tokens tokenName1,tokenName2");
            Dff.PrintDff("--tokens tokenName1,tokenName2 --optype optype");
            Dff.PrintDff("--tokens tokenName1,tokenName2 --targetdie die");
            Dff.PrintDff("--tokens tokenName1,tokenName2 --optype optype --targetdie die");

            this.dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void PrintDff_Fail()
        {
            // setup mocks.
            // Write all the gsds values.
            Assert.ThrowsException<ArgumentException>(() => Dff.PrintDff("--invalid tokenName1,tokenName2"));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void MirrorDff_Pass()
        {
            // setup mocks.
            this.dffServiceMock.Setup(o => o.GetDff("tokenName1", true)).Returns("value1");
            this.dffServiceMock.Setup(o => o.GetDff("tokenName2", true)).Returns("value2");
            this.dffServiceMock.Setup(o => o.GetDffByOpType("tokenName1", "optype", true)).Returns("value1");
            this.dffServiceMock.Setup(o => o.GetDffByOpType("tokenName2", "optype", true)).Returns("value2");
            this.dffServiceMock.Setup(o => o.GetDffByDieId("tokenName1", "die", true)).Returns("value1");
            this.dffServiceMock.Setup(o => o.GetDffByDieId("tokenName2", "die", true)).Returns("value2");
            this.dffServiceMock.Setup(o => o.GetDff("tokenName1", "optype", "die", true)).Returns("value1");
            this.dffServiceMock.Setup(o => o.GetDff("tokenName2", "optype", "die", true)).Returns("value2");

            this.sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("MD_tokenName1__", "value1", Context.DUT));
            this.sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("MD_tokenName2__", "value2", Context.DUT));
            this.sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("MD_tokenName1_optype_", "value1", Context.DUT));
            this.sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("MD_tokenName2_optype_", "value2", Context.DUT));
            this.sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("MD_tokenName1__die", "value1", Context.DUT));
            this.sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("MD_tokenName2__die", "value2", Context.DUT));
            this.sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("MD_tokenName1_optype_die", "value1", Context.DUT));
            this.sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("MD_tokenName2_optype_die", "value2", Context.DUT));

            // Write all the gsds values.
            Dff.MirrorDff(string.Empty);
            Dff.MirrorDff("--tokens tokenName1,tokenName2");
            Dff.MirrorDff("--tokens tokenName1,tokenName2 --optype optype");
            Dff.MirrorDff("--tokens tokenName1,tokenName2 --targetdie die");
            Dff.MirrorDff("--tokens tokenName1,tokenName2 --optype optype --targetdie die");

            this.dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void MirrorDff_Fail()
        {
            // setup mocks.
            // Write all the gsds values.
            Assert.ThrowsException<ArgumentException>(() => Dff.MirrorDff("--invalid tokenName1,tokenName2"));
        }

        /// <summary>
        /// Run the SetCurrentDieId callback.
        /// </summary>
        [TestMethod]
        public void SetCurrentDieID_Pass()
        {
            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(o => o.SetCurrentDieId("newDieID"));
            Prime.Services.DffService = dffServiceMock.Object;

            Assert.AreEqual("1", Dff.SetCurrentDieId("newDieID"));
            dffServiceMock.VerifyAll();
        }
    }
}
