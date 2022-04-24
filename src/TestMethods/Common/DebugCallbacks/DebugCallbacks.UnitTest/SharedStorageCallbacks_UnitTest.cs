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
    using Prime.PerformanceService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;
    using Prime.UserVarService;

    /// <summary>
    /// Defines the <see cref="SharedStorageCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class SharedStorageCallbacks_UnitTest
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

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Test the WriteSharedStorage functionality, all passing cases. No need to exhaustively test the combinations since that's done in the BaseUtilities.
        /// </summary>
        [TestMethod]
        public void WriteSharedStorage_FromLiteral_Pass()
        {
            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            gsdsMock.Setup(o => o.InsertRowAtTable("StringToken", "LotValue", Context.LOT));
            gsdsMock.Setup(o => o.InsertRowAtTable("IntegerToken", 4, Context.DUT));
            gsdsMock.Setup(o => o.InsertRowAtTable("StringToken2", "blah.blah.blah", Context.DUT));
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("LotValue")).Returns(false);
            userVarMock.Setup(o => o.Exists("4")).Returns(false);
            userVarMock.Setup(o => o.Exists("blah.blah.blah")).Throws(new Prime.Base.Exceptions.FatalException("The full user var name=[blah.blah.blah] has more than 1 separator=[.]. Expected format=[<collection>.<name>], with the collection optional (Taking the _UserVar collection)."));
            Prime.Services.UserVarService = userVarMock.Object;

            // ignore any console messages.
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;

            // Write all the gsds values.
            SharedStorage.WriteSharedStorage("--token G.L.S.StringToken --value LotValue");
            SharedStorage.WriteSharedStorage("--token G.U.I.IntegerToken --value 4");
            SharedStorage.WriteSharedStorage("--token G.U.S.StringToken2 --value blah.blah.blah");

            // verify the mock
            gsdsMock.VerifyAll();
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteSharedStorage functionality, all passing cases. No need to exhaustively test the combinations since that's done in the BaseUtilities.
        /// </summary>
        [TestMethod]
        public void WriteSharedStorage_FromeGsds_Pass()
        {
            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            gsdsMock.Setup(o => o.InsertRowAtTable("StringToken", "SomeValue", Context.LOT));
            gsdsMock.Setup(o => o.KeyExistsInStringTable("Token1", Context.IP)).Returns(true);
            gsdsMock.Setup(o => o.GetStringRowFromTable("Token1", Context.IP)).Returns("SomeValue");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            // ignore any console messages.
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;

            // Write all the gsds values.
            SharedStorage.WriteSharedStorage("--token G.L.S.StringToken --value G.I.S.Token1");

            // verify the mock
            gsdsMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteSharedStorage functionality, all passing cases. No need to exhaustively test the combinations since that's done in the BaseUtilities.
        /// </summary>
        [TestMethod]
        public void WriteSharedStorage_FromeUserVarString_Pass()
        {
            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            gsdsMock.Setup(o => o.InsertRowAtTable("StringToken", "SomeValue", Context.LOT));
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("Col1.Var1")).Returns(true);
            userVarMock.Setup(o => o.GetStringValue("Col1.Var1")).Returns("SomeValue");
            Prime.Services.UserVarService = userVarMock.Object;

            // ignore any console messages.
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;

            // Write all the gsds values.
            SharedStorage.WriteSharedStorage("--token G.L.S.StringToken --value Col1.Var1");

            // verify the mock
            gsdsMock.VerifyAll();
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteSharedStorage functionality, all passing cases. No need to exhaustively test the combinations since that's done in the BaseUtilities.
        /// </summary>
        [TestMethod]
        public void WriteSharedStorage_FromeUserVarDouble_Pass()
        {
            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            gsdsMock.Setup(o => o.InsertRowAtTable("StringToken", "1.2345", Context.LOT));
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("Col1.Var1")).Returns(true);
            userVarMock.Setup(o => o.GetDoubleValue("Col1.Var1")).Returns(1.2345);
            Prime.Services.UserVarService = userVarMock.Object;

            // ignore any console messages.
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;

            // Write all the gsds values.
            SharedStorage.WriteSharedStorage("--token G.L.S.StringToken --value Col1.Var1");

            // verify the mock
            gsdsMock.VerifyAll();
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteSharedStorage functionality, all passing cases. No need to exhaustively test the combinations since that's done in the BaseUtilities.
        /// </summary>
        [TestMethod]
        public void WriteSharedStorage_FromeUserVarInt_Pass()
        {
            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            gsdsMock.Setup(o => o.InsertRowAtTable("StringToken", "57", Context.LOT));
            Prime.Services.SharedStorageService = gsdsMock.Object;

            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("Col1.Var1")).Returns(true);
            userVarMock.Setup(o => o.GetIntValue("Col1.Var1")).Returns(57);
            Prime.Services.UserVarService = userVarMock.Object;

            // ignore any console messages.
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;

            // Write all the gsds values.
            SharedStorage.WriteSharedStorage("--token G.L.S.StringToken --value Col1.Var1");

            // verify the mock
            gsdsMock.VerifyAll();
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteSharedStorage failing/exception cases.
        /// </summary>
        [TestMethod]
        public void WriteSharedStorage_Fail()
        {
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("LotValue")).Returns(false);
            Prime.Services.UserVarService = userVarMock.Object;

            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = gsdsMock.Object;

            // ignore any console messages.
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;

            Assert.ThrowsException<ArgumentException>(() => SharedStorage.WriteSharedStorage(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => SharedStorage.WriteSharedStorage("--token blah"));
            Assert.ThrowsException<ArgumentException>(() => SharedStorage.WriteSharedStorage("--value fake"));
            Assert.ThrowsException<ArgumentException>(() => SharedStorage.WriteSharedStorage("--token G.L.S.StringToken --value SomeValue -extraArg"));
            Assert.ThrowsException<ArgumentException>(() => SharedStorage.WriteSharedStorage("--token G.L.X.StringToken --value LotValue"));

            userVarMock.VerifyAll();
            gsdsMock.VerifyAll();
        }

        /// <summary>
        /// Test the WriteSharedStorage functionality, all passing cases. No need to exhaustively test the combinations since that's done in the BaseUtilities.
        /// </summary>
        [TestMethod]
        public void WriteSharedStorage_FromeUserVarX_Fail()
        {
            var userVarMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarMock.Setup(o => o.Exists("Col1.Var1")).Returns(true);
            userVarMock.Setup(o => o.GetIntValue("Col1.Var1")).Throws(new Prime.Base.Exceptions.FatalException("Actual uservar Type=[Unknown] does not match expected=[Int]"));
            Prime.Services.UserVarService = userVarMock.Object;

            // ignore any console messages.
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;

            // Write all the gsds values.
            Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => SharedStorage.WriteSharedStorage("--token G.L.S.StringToken --value Col1.Var1"));

            // verify the mock
            userVarMock.VerifyAll();
        }

        /// <summary>
        /// Test the PrintSharedStorage functionality, all passing cases. No need to exhaustively test the combinations since that's done in the BaseUtilities.
        /// </summary>
        [TestMethod]
        public void PrintSharedStorage_NoArgs_Pass()
        {
            // ignore any console messages.
            var consoleService = new Mock<IConsoleService>(MockBehavior.Strict);
            Prime.Services.ConsoleService = consoleService.Object;

            // mock the shared storage.
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.DumpAllTablesToConsole());
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // Print all the gsds values.
            SharedStorage.PrintSharedStorage(string.Empty);

            // verify the mock
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Test the PrintSharedStorage functionality, all passing cases. No need to exhaustively test the combinations since that's done in the BaseUtilities.
        /// </summary>
        [TestMethod]
        public void PrintSharedStorage_Pass()
        {
            // setup the sharedstorage/gsds mock.
            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            gsdsMock.Setup(o => o.KeyExistsInStringTable("StringToken", Context.LOT)).Returns(true);
            gsdsMock.Setup(o => o.KeyExistsInStringTable("StringToken2", Context.LOT)).Returns(true);
            gsdsMock.Setup(o => o.KeyExistsInIntegerTable("IntegerToken1", Context.DUT)).Returns(true);
            gsdsMock.Setup(o => o.KeyExistsInDoubleTable("DoubleToken1", Context.DUT)).Returns(true);
            gsdsMock.Setup(o => o.KeyExistsInIntegerTable("MissingToken", Context.DUT)).Returns(false);

            gsdsMock.Setup(o => o.GetStringRowFromTable("StringToken", Context.LOT)).Returns("StringLotValue");
            gsdsMock.Setup(o => o.GetStringRowFromTable("StringToken2", Context.LOT)).Returns("StringLotValue2");
            gsdsMock.Setup(o => o.GetIntegerRowFromTable("IntegerToken1", Context.DUT)).Returns(5);
            gsdsMock.Setup(o => o.GetDoubleRowFromTable("DoubleToken1", Context.DUT)).Returns(0.6);
            Prime.Services.SharedStorageService = gsdsMock.Object;

            // mock all the console messages.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug("[SharedStorage] G.L.S.StringToken=StringLotValue"));
            consoleServiceMock.Setup(o => o.PrintDebug("[SharedStorage] G.U.I.MissingToken=<undefined>"));
            consoleServiceMock.Setup(o => o.PrintDebug("[SharedStorage] G.U.I.IntegerToken1=5"));
            consoleServiceMock.Setup(o => o.PrintDebug("[SharedStorage] G.L.S.StringToken2=StringLotValue2"));
            consoleServiceMock.Setup(o => o.PrintDebug("[SharedStorage] G.U.D.DoubleToken1=0.6"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // Print all the gsds values.
            SharedStorage.PrintSharedStorage("--token G.L.S.StringToken");
            SharedStorage.PrintSharedStorage("--token G.U.I.MissingToken");
            SharedStorage.PrintSharedStorage("--token G.U.I.IntegerToken1,G.L.S.StringToken2,G.U.D.DoubleToken1");

            // verify the mock
            gsdsMock.VerifyAll();
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the PrintSharedStorage failing/exception cases.
        /// </summary>
        [TestMethod]
        public void PrintSharedStorage_Fail()
        {
            // ignore any console messages.
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;

            Assert.ThrowsException<ArgumentException>(() => SharedStorage.PrintSharedStorage("--token blah"));
            Assert.ThrowsException<ArgumentException>(() => SharedStorage.PrintSharedStorage("--token G.L.S.StringToken --value LotValue -extraArg"));
            Assert.ThrowsException<ArgumentException>(() => SharedStorage.PrintSharedStorage("--token G.L.X.StringToken"));
        }
    }
}
