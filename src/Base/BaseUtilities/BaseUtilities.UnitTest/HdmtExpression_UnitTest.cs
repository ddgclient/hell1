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

namespace DDG.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DffService;
    using Prime.SharedStorageService;
    using Prime.TestConditionService;
    using Prime.TestProgramService;
    using Prime.UserVarService;

    /// <summary>
    /// Defines the <see cref="HdmtExpression_UnitTest" />.
    /// </summary>
    [TestClass]
    public class HdmtExpression_UnitTest
    {
        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>()))
                .Callback((string s) => { Console.WriteLine(s); });
            consoleServiceMock
                .Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string n, string src) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("LogLevel")).Returns("TEST_METHOD");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;
        }

        /// <summary>
        /// dummy test of ncalc expression.
        /// </summary>
        [TestMethod]
        public void NcalcExpressionTests()
        {
            Assert.AreEqual(0, new NCalc.Expression("5 - 5").Evaluate());
            Assert.IsTrue((bool)new NCalc.Expression("5 == 5").Evaluate());
            Assert.AreEqual("S_4.5", new NCalc.Expression("'S' + '_' + 4.5").Evaluate());
            Assert.AreEqual("4.5_S", new NCalc.Expression("'4.5' + '_' + 'S'").Evaluate()); // doesn't like "4.5 + '_' + 'S'" --> first thing sets the int/double/string mode
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void HdmtExpression_InvalidToken_Exception()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("invalid")).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var underTest = new DDG.HdmtExpression("[invalid]");
            Assert.ThrowsException<ArgumentException>(() => underTest.Evaluate());
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void HdmtExpression_IntegerExpression_Pass()
        {
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInIntegerTable("Token1", Context.LOT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInStringTable("Token2", Context.DUT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.GetIntegerRowFromTable("Token1", Context.LOT)).Returns(5);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("Token2", Context.DUT)).Returns("4");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("Coll1.Var1")).Returns(true);
            userVarServiceMock.Setup(o => o.GetIntValue("Coll1.Var1")).Returns(10);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(o => o.GetDff("Dff1", true)).Returns("100");
            Prime.Services.DffService = dffServiceMock.Object;

            var underTest = new DDG.HdmtExpression("ToInt32([G.U.S.Token2]) + 1000 + [Coll1.Var1] + ToInt32([Dff1]) + [G.L.I.Token1]");
            Assert.AreEqual(1119, underTest.Evaluate());
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            dffServiceMock.VerifyAll();

            Assert.AreEqual(9, new HdmtExpression("ToInt32(9.0)").Evaluate());
            Assert.AreEqual(9, new HdmtExpression("ToInt32(9.3)").Evaluate());
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void HdmtExpression_DoubleExpression_Pass()
        {
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInDoubleTable("Token1", Context.LOT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInStringTable("Token2", Context.DUT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.GetDoubleRowFromTable("Token1", Context.LOT)).Returns(0.1);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("Token2", Context.DUT)).Returns("4");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("Coll1.Var1")).Returns(true);
            userVarServiceMock.Setup(o => o.GetDoubleValue("Coll1.Var1")).Returns(10.05);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(o => o.GetDff("Dff1", true)).Returns("100.009");
            Prime.Services.DffService = dffServiceMock.Object;

            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string> { { "TimingsTc", "SomeTimings" } });
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetSpecSetValue("Spec")).Returns("9nS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(o => o.GetTestCondition("SomeTimings")).Returns(testConditionMock.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            var underTest = new DDG.HdmtExpression("ToDouble([G.U.S.Token2]) + 1000 + [Coll1.Var1] + ToDouble([Dff1]) + [G.L.D.Token1] + ToDouble([S.T.Spec])*1E08");
            var result = (double)underTest.Evaluate();
            Assert.IsTrue(Math.Abs(1115.059 - result) < 0.00001);
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            dffServiceMock.VerifyAll();
            testConditionMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
            testProgramServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void HdmtExpression_StringExpression_Pass()
        {
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInIntegerTable("Token1", Context.LOT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInStringTable("Token2", Context.DUT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.GetIntegerRowFromTable("Token1", Context.LOT)).Returns(5);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("Token2", Context.DUT)).Returns("Prefix");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("Coll1.Var1")).Returns(true);
            userVarServiceMock.Setup(o => o.GetStringValue("Coll1.Var1")).Returns("blah");
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            dffServiceMock.Setup(o => o.GetDff("Dff1", true)).Returns("SomeDff");
            Prime.Services.DffService = dffServiceMock.Object;

            var underTest = new DDG.HdmtExpression("Substring([G.U.S.Token2],1,3) + '_' + [Coll1.Var1] + '_' + [Dff1] + '_' + [G.L.I.Token1]");
            Assert.AreEqual("ref_blah_SomeDff_5", underTest.Evaluate());
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            dffServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void HdmtExpression_Random_Pass()
        {
            var underTest = new DDG.HdmtExpression("Random()");
            Assert.IsTrue((double)underTest.Evaluate() > 0);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void HdmtExpression_Bin2Dec_Pass()
        {
            Assert.AreEqual(1, new HdmtExpression("Bin2Dec('00001')").Evaluate());
            Assert.AreEqual(9, new HdmtExpression("Bin2Dec('01001')").Evaluate());
            Assert.ThrowsException<FormatException>(() => new HdmtExpression("Bin2Dec('notabinarynumber')").Evaluate());

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInStringTable("Token", Context.DUT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("Token", Context.DUT)).Returns("0101");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            Assert.AreEqual(5, new HdmtExpression("Bin2Dec([G.U.S.Token])").Evaluate());
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void HdmtExpression_Dec2Bin_Pass()
        {
            Assert.AreEqual("000001", new HdmtExpression("Dec2Bin(1, 6)").Evaluate());
            Assert.AreEqual("1", new HdmtExpression("Dec2Bin(1,1)").Evaluate());
            Assert.AreEqual("10011", new HdmtExpression("Dec2Bin(19, 5)").Evaluate());
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void HdmtExpression_Reverse_Pass()
        {
            Assert.AreEqual("000001", new HdmtExpression("Reverse('100000')").Evaluate());
            Assert.AreEqual("100000", new HdmtExpression("Reverse('000001')").Evaluate());
            Assert.AreEqual("1", new HdmtExpression("Reverse(1)").Evaluate());

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInStringTable("Token", Context.DUT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("Token", Context.DUT)).Returns("abcdefg");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            Assert.AreEqual("gfedcba", new HdmtExpression("Reverse([G.U.S.Token])").Evaluate());
        }

        /// <summary>
        /// Make sure if we evaluate the expression multiple times, the parameters get re-evaluated each time.
        /// </summary>
        [TestMethod]
        public void HdmtExpression_CheckCaching_Pass()
        {
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInIntegerTable("Token1", Context.LOT)).Returns(true);
            sharedStorageServiceMock.SetupSequence(o => o.GetIntegerRowFromTable("Token1", Context.LOT))
                .Returns(5)
                .Returns(50)
                .Returns(100);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            var underTest = new DDG.HdmtExpression("[G.L.I.Token1] + 5");
            Assert.AreEqual(10, underTest.Evaluate());
            Assert.AreEqual(55, underTest.Evaluate());
            Assert.AreEqual(105, underTest.Evaluate());
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void HdmtExpression_GetPatSymbolString_Pass()
        {
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInStringTable("Token", Context.DUT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("Token", Context.DUT)).Returns("1100");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("Coll1.Var1")).Returns(true);
            userVarServiceMock.Setup(o => o.GetStringValue("Coll1.Var1")).Returns("1010");
            Prime.Services.UserVarService = userVarServiceMock.Object;

            Assert.AreEqual("000101", new HdmtExpression("GetPatSymbolString('0b101',6)").Evaluate());
            Assert.AreEqual("LHL", new HdmtExpression("GetPatSymbolString('LHL',3)").Evaluate());
            Assert.AreEqual("101000", new HdmtExpression("Reverse(GetPatSymbolString('101',6))").Evaluate());
            Assert.AreEqual("001000", new HdmtExpression("GetPatSymbolString('0d8',6)").Evaluate());
            Assert.AreEqual("11111111", new HdmtExpression("GetPatSymbolString('0xFF',8)").Evaluate());
            Assert.AreEqual("00001100", new HdmtExpression("GetPatSymbolString([G.U.S.Token],8)").Evaluate());
            Assert.AreEqual("001010", new HdmtExpression("GetPatSymbolString([Coll1.Var1],6)").Evaluate());
        }
    }
}
