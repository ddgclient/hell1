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

namespace AuxiliaryTC.UnitTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.DffService;
    using Prime.SharedStorageService;
    using Prime.UserVarService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class AuxiliaryTC_UnitTest : AuxiliaryTC
    {
        private Mock<ISharedStorageService> sharedStorageMock;
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IUserVarService> userVarServiceMock;
        private Mock<IDffService> dffServiceMock;
        private Mock<IDatalogService> datalogServiceMock;

        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;
            this.consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>()))
                .Callback((string s) => { System.Console.WriteLine(s); });
            this.consoleServiceMock
                .Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string n, string src) => System.Console.WriteLine($"ERROR: {msg}"));
            this.sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = this.sharedStorageMock.Object;
            this.userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            Prime.Services.UserVarService = this.userVarServiceMock.Object;
            this.dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            Prime.Services.DffService = this.dffServiceMock.Object;
            this.datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            Prime.Services.DatalogService = this.datalogServiceMock.Object;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Verify_NoArgs_Exception()
        {
            this.ResultPort = string.Empty;
            this.ResultToken = string.Empty;

            var ex = Assert.ThrowsException<ArgumentException>(() => this.Verify());
            Assert.IsTrue(ex.Message.Contains("user must specify at least one option ResultToken and/or ResultPort."));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_InvalidToken_Fail()
        {
            this.ResultToken = "G.U.I.Token1";
            this.ResultPort = "[R]==1?2:0";
            this.Expression = "[G.U.I.invalid]";
            this.DataType = ValueType.Integer;
            this.Storage = StorageType.SharedStorage;
            this.sharedStorageMock.Setup(o => o.KeyExistsInIntegerTable("invalid", Context.DUT)).Returns(false);
            this.userVarServiceMock.Setup(o => o.Exists("invalid")).Returns(false);

            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(0, result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StoreAsIntegerSharedStorage_Pass()
        {
            this.ResultToken = "G.U.I.Token1";
            this.Expression = "1";
            this.DataType = ValueType.Integer;
            this.Storage = StorageType.SharedStorage;

            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 1, Context.DUT));
            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(1, result);

            this.sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StoreAsDoubleSharedStorage_Pass()
        {
            this.ResultToken = "G.U.D.Token1";
            this.Expression = "ToDouble(1)";
            this.DataType = ValueType.Double;
            this.Storage = StorageType.SharedStorage;

            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 1D, Context.DUT));
            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(1, result);

            this.sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StoreAsStringSharedStorage_Pass()
        {
            this.ResultToken = "G.U.S.Token1";
            this.Expression = "1";
            this.DataType = ValueType.String;
            this.Storage = StorageType.SharedStorage;

            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", "1", Context.DUT));
            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(1, result);

            this.sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StoreAsIntegerUservar_Pass()
        {
            this.ResultToken = "Token1";
            this.ResultPort = "[R]==1?2:0";
            this.Expression = "1";
            this.DataType = ValueType.Integer;
            this.Storage = StorageType.UserVar;

            this.userVarServiceMock.Setup(o => o.SetValue("Token1", 1));
            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(2, result);

            this.userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StoreAsDoubleUservar_Pass()
        {
            this.ResultToken = "Token1";
            this.Expression = "ToDouble(1)";
            this.ResultPort = "[R]>0?2:0";
            this.DataType = ValueType.Double;
            this.Storage = StorageType.UserVar;

            this.userVarServiceMock.Setup(o => o.SetValue("Token1", 1D));
            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(2, result);

            this.userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StoreAsStringUservar_Pass()
        {
            this.ResultToken = "Token1";
            this.Expression = "1";
            this.ResultPort = "[R]==\"1\"?2:0";
            this.DataType = ValueType.String;
            this.Storage = StorageType.UserVar;

            this.userVarServiceMock.Setup(o => o.SetValue("Token1", "1"));
            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(2, result);

            this.userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StoreAsIntegerDff_Pass()
        {
            this.ResultToken = "Token1";
            this.ResultPort = "[R]==1?2:0";
            this.Expression = "1";
            this.DataType = ValueType.Integer;
            this.Storage = StorageType.DFF;
            this.Datalog = EnableType.Enabled;

            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetData("1"));
            this.datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            this.datalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            this.dffServiceMock.Setup(o => o.SetDff("Token1", "1"));
            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(2, result);

            this.dffServiceMock.VerifyAll();
            this.datalogServiceMock.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StoreAsDoubleDff_Pass()
        {
            this.ResultToken = "Token1";
            this.Expression = "ToDouble(1)";
            this.ResultPort = "[R]>0?2:0";
            this.DataType = ValueType.Double;
            this.Storage = StorageType.DFF;
            this.Datalog = EnableType.Enabled;

            var writerMock = new Mock<IMrsltFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetData(It.IsAny<double>()));
            this.datalogServiceMock.Setup(o => o.GetItuffMrsltWriter()).Returns(writerMock.Object);
            this.datalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            this.dffServiceMock.Setup(o => o.SetDff("Token1", "1"));
            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(2, result);

            this.dffServiceMock.VerifyAll();
            this.datalogServiceMock.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_StoreAsStringDff_Pass()
        {
            this.ResultToken = "Token1";
            this.Expression = "1";
            this.ResultPort = "[R]==\"1\"?2:0";
            this.DataType = ValueType.String;
            this.Storage = StorageType.DFF;
            this.Datalog = EnableType.Enabled;

            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetData("1"));
            this.datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            this.datalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            this.dffServiceMock.Setup(o => o.SetDff("Token1", "1"));
            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(2, result);

            this.dffServiceMock.VerifyAll();
            this.datalogServiceMock.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_IntegerSharedStorage_Pass()
        {
            this.ResultToken = "G.U.I.Token1";
            this.ResultPort = "[R]>1?2:0";
            this.Expression = "[G.U.I.Source]+1";
            this.DataType = ValueType.Integer;
            this.Storage = StorageType.SharedStorage;

            this.sharedStorageMock.Setup(o => o.KeyExistsInIntegerTable("Source", Context.DUT)).Returns(true);
            this.sharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Source", Context.DUT)).Returns(3);
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 4, Context.DUT));
            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(2, result);

            this.sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_IntegerFromDoubleSharedStorage_Pass()
        {
            this.ResultToken = "G.U.I.Token1";
            this.ResultPort = "[R]>1?2:0";
            this.Expression = "ToInt32([G.U.D.Source])+1";
            this.DataType = ValueType.Integer;
            this.Storage = StorageType.SharedStorage;

            this.sharedStorageMock.Setup(o => o.KeyExistsInDoubleTable("Source", Context.DUT)).Returns(true);
            this.sharedStorageMock.Setup(o => o.GetDoubleRowFromTable("Source", Context.DUT)).Returns(3.3);
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 4, Context.DUT));
            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(2, result);

            this.sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_IntegerFromStringSharedStorage_Pass()
        {
            this.ResultToken = "G.U.I.Token1";
            this.ResultPort = "[R]>1?2:0";
            this.Expression = "ToInt32([G.U.S.Source])+1";
            this.DataType = ValueType.Integer;
            this.Storage = StorageType.SharedStorage;

            this.sharedStorageMock.Setup(o => o.KeyExistsInStringTable("Source", Context.DUT)).Returns(true);
            this.sharedStorageMock.Setup(o => o.GetStringRowFromTable("Source", Context.DUT)).Returns("3");
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 4, Context.DUT));
            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(2, result);

            this.sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_UserVars_Pass()
        {
            this.ResultToken = "G.U.I.Token1";
            this.ResultPort = "[R]>1?2:0";
            this.Expression = "ToInt32([G.U.S.Source1])+[Source2]+ToInt32([Source3])+ToInt32(ToDouble([Source4]))";
            this.DataType = ValueType.Integer;
            this.Storage = StorageType.SharedStorage;

            this.sharedStorageMock.Setup(o => o.KeyExistsInStringTable("Source1", Context.DUT)).Returns(true);
            this.sharedStorageMock.Setup(o => o.GetStringRowFromTable("Source1", Context.DUT)).Returns("3");
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 12, Context.DUT));
            this.userVarServiceMock.Setup(o => o.Exists("Source2")).Returns(true);
            this.userVarServiceMock.Setup(o => o.GetIntValue("Source2")).Returns(2);
            this.userVarServiceMock.Setup(o => o.Exists("Source3")).Returns(true);
            this.userVarServiceMock.Setup(o => o.GetDoubleValue("Source3")).Returns(3.3);
            this.userVarServiceMock.Setup(o => o.Exists("Source4")).Returns(true);
            this.userVarServiceMock.Setup(o => o.GetStringValue("Source4")).Returns("4.4");

            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(2, result);

            this.sharedStorageMock.VerifyAll();
            this.userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_UnableToFindUservar_Fail()
        {
            this.ResultToken = "G.U.I.Token1";
            this.ResultPort = "[R]>1?2:0";
            this.Expression = "ToInt32([G.U.S.Source1])+[Source2]+ToInt32([Source3])+ToInt32(ToDouble([Source4]))";
            this.DataType = ValueType.Integer;
            this.Storage = StorageType.SharedStorage;

            this.sharedStorageMock.Setup(o => o.KeyExistsInStringTable("Source1", Context.DUT)).Returns(true);
            this.sharedStorageMock.Setup(o => o.GetStringRowFromTable("Source1", Context.DUT)).Returns("3");
            this.userVarServiceMock.Setup(o => o.Exists("Source2")).Returns(true);

            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(0, result);

            this.sharedStorageMock.VerifyAll();
            this.userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_DFF_Pass()
        {
            this.ResultToken = "G.U.I.Token1";
            this.ResultPort = "[R]>1?2:0";
            this.Expression = "ToInt32([G.U.S.Source1])+ToInt32([Source2])";
            this.DataType = ValueType.Integer;
            this.Storage = StorageType.SharedStorage;

            this.sharedStorageMock.Setup(o => o.KeyExistsInStringTable("Source1", Context.DUT)).Returns(true);
            this.sharedStorageMock.Setup(o => o.GetStringRowFromTable("Source1", Context.DUT)).Returns("3");
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 5, Context.DUT));
            this.userVarServiceMock.Setup(o => o.Exists("Source2")).Returns(false);
            this.dffServiceMock.Setup(o => o.GetDff("Source2", true)).Returns("2");

            this.Verify();
            var result = this.Execute();
            Assert.AreEqual(2, result);

            this.sharedStorageMock.VerifyAll();
            this.userVarServiceMock.VerifyAll();
            this.dffServiceMock.VerifyAll();
        }
    }
}
