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
    using Prime.UserVarService;

    /// <summary>
    /// Defines the <see cref="AuxiliaryCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class AuxiliaryCallbacks_UnitTest
    {
        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) => Console.WriteLine(msg));
            consoleMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleMock.Object;

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Test the Expression Callback.
        /// </summary>
        [TestMethod]
        public void Expression_MissingArguments_Fail()
        {
            var missingStorageTypeEx = Assert.ThrowsException<ArgumentException>(() => Auxiliary.EvaluateExpression("--expression 5 + 4 --result G.U.S.ResultToken --datatype String"));
            Assert.IsTrue(missingStorageTypeEx.Message.Contains("Failed parsing arguments."));

            var missingDataTypeEx = Assert.ThrowsException<ArgumentException>(() => Auxiliary.EvaluateExpression("--expression 5 + 4 --result G.U.S.ResultToken --storagetype integer"));
            Assert.IsTrue(missingDataTypeEx.Message.Contains("Failed parsing arguments."));

            var missingResultEx = Assert.ThrowsException<ArgumentException>(() => Auxiliary.EvaluateExpression("--expression 5 + 4 --datatype String --storagetype integer"));
            Assert.IsTrue(missingResultEx.Message.Contains("Failed parsing arguments."));

            var missingExpressionEx = Assert.ThrowsException<ArgumentException>(() => Auxiliary.EvaluateExpression("--result G.U.S.ResultToken --datatype String --storagetype integer"));
            Assert.IsTrue(missingExpressionEx.Message.Contains("Failed parsing arguments."));
        }

        /// <summary>
        /// Test the Expression Callback.
        /// </summary>
        [TestMethod]
        public void Expression_StoreGsds_Pass()
        {
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("ResultToken", "5", Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            Auxiliary.EvaluateExpression("--expression 5 --result G.U.S.ResultToken --datatype String --storagetype gsds");
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the Expression Callback.
        /// </summary>
        [TestMethod]
        public void Expression_StoreDff_Pass()
        {
            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(o => o.SetDff("ResultToken", "11"));
            Prime.Services.DffService = dffServiceMock.Object;

            Auxiliary.EvaluateExpression("--expression 5 + 6 --result ResultToken --datatype integer --storagetype dff");
            dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the Expression Callback.
        /// </summary>
        [TestMethod]
        public void Expression_StoreUserVarDouble_Pass()
        {
            var userVarService = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarService.Setup(o => o.SetValue("Coll1.ResultToken", 6.7));
            Prime.Services.UserVarService = userVarService.Object;

            Auxiliary.EvaluateExpression("--result Coll1.ResultToken --datatype double --storagetype uservar --expression -- 11.5 - 4.8 ");
            userVarService.VerifyAll();
        }

        /// <summary>
        /// Test the Expression Callback.
        /// </summary>
        [TestMethod]
        public void Expression_StoreUserVarInteger_Pass()
        {
            var userVarService = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarService.Setup(o => o.SetValue("Coll1.ResultToken", 99));
            Prime.Services.UserVarService = userVarService.Object;

            Auxiliary.EvaluateExpression("--expression 99 --result Coll1.ResultToken --datatype integer --storagetype uservar");
            userVarService.VerifyAll();
        }

        /// <summary>
        /// Test the Expression Callback.
        /// </summary>
        [TestMethod]
        public void Expression_StoreUserVarString_Pass()
        {
            var userVarService = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarService.Setup(o => o.SetValue("Coll1.ResultToken", "11110000"));
            Prime.Services.UserVarService = userVarService.Object;

            Auxiliary.EvaluateExpression("--expression 11110000  --result Coll1.ResultToken --datatype string --storagetype uservar");
            userVarService.VerifyAll();
        }

        /// <summary>
        /// Test the Expression Callback.
        /// </summary>
        [TestMethod]
        public void Expression_GuardBandDouble_Pass()
        {
            var userVarService = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarService.Setup(o => o.Exists("GBVars.SomeGuardBand")).Returns(true);
            userVarService.Setup(o => o.GetDoubleValue("GBVars.SomeGuardBand")).Returns(0.03);
            Prime.Services.UserVarService = userVarService.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInDoubleTable("VminValue", Context.DUT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.GetDoubleRowFromTable("VminValue", Context.DUT)).Returns(0.95);
            sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("VminValueWithGb", It.Is<double>(it => Math.Abs(it - 0.92) < 0.0001), Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            Auxiliary.EvaluateExpression("--result G.U.D.VminValueWithGb --datatype double --storagetype gsds --expression -- [G.U.D.VminValue] - [GBVars.SomeGuardBand]");
            userVarService.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the Expression Callback.
        /// </summary>
        [TestMethod]
        public void Expression_ConcatenateBinaryValueAsString_Pass()
        {
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInStringTable("Token1", Context.DUT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("Token1", Context.DUT)).Returns("0000");
            sharedStorageServiceMock.Setup(o => o.KeyExistsInStringTable("Token2", Context.DUT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("Token2", Context.DUT)).Returns("1111");
            sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("ResultToken", "00001111", Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            Auxiliary.EvaluateExpression("--result G.U.S.ResultToken --datatype string --storagetype gsds --expression [G.U.S.Token1] + [G.U.S.Token2]");
            sharedStorageServiceMock.VerifyAll();
        }
    }
}
