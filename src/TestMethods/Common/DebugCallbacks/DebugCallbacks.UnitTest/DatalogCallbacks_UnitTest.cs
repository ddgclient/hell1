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
    using Prime.PerformanceService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;
    using Prime.UserVarService;
    using TOSUserSDK;

    /// <summary>
    /// Defines the <see cref="DatalogCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class DatalogCallbacks_UnitTest
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
            consoleMock.Setup(o =>
                    o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleMock.Object;

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Test the WriteUser Callback.
        /// </summary>
        [TestMethod]
        public void PrintToItuff_InvalidType_Exception()
        {
            // run the test
            var ex = Assert.ThrowsException<ArgumentException>(() =>
                Datalog.PrintToItuff("--body_type invalid --body_data somestringdata"));
            Assert.IsTrue(ex.Message.StartsWith("Requested value 'INVALID' was not found."));
        }

        /// <summary>
        /// Test the PrintToItuff Callback.
        /// </summary>
        [TestMethod]
        public void PrintToItuff_StrgvalFormatAllTypes_Pass()
        {
            var strgvalWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            strgvalWriterMock.Setup(o => o.SetData("somestringdata_GsdsData_UservarData"));
            strgvalWriterMock.Setup(o => o.SetTnamePostfix(string.Empty));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strgvalWriterMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(strgvalWriterMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInStringTable("Token1", Context.DUT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("Token1", Context.DUT)).Returns("GsdsData");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("SCVars.Token2")).Returns(true);
            userVarServiceMock.Setup(o => o.GetStringValue("SCVars.Token2")).Returns("UservarData");
            Prime.Services.UserVarService = userVarServiceMock.Object;

            Datalog.PrintToItuff("--body_type strgval --body_data somestringdata,_,G.U.S.Token1,_,SCVars.Token2");
            strgvalWriterMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the PrintToItuff Callback.
        /// </summary>
        [TestMethod]
        public void PrintToItuff_MsrltFormatWithPostFix_Pass()
        {
            var mrsltWriterMock = new Mock<IMrsltFormat>(MockBehavior.Strict);
            mrsltWriterMock.Setup(o => o.SetData(3.14159));
            mrsltWriterMock.Setup(o => o.SetTnamePostfix("Post"));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffMrsltWriter()).Returns(mrsltWriterMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(mrsltWriterMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            Datalog.PrintToItuff("--body_type mrslt --body_data 3.14159 --tname_suf Post");
            mrsltWriterMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the PrintToItuff Callback.
        /// </summary>
        [TestMethod]
        public void PrintToItuff_BinaryFormat_Pass()
        {
            var binaryWriterMock = new Mock<IRawbinaryFormat>(MockBehavior.Strict);
            binaryWriterMock.Setup(o => o.SetData("010111", true));
            binaryWriterMock.Setup(o => o.SetTnamePostfix(string.Empty));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffRawbinaryWriter()).Returns(binaryWriterMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(binaryWriterMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInStringTable("BinaryData", Context.DUT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.GetStringRowFromTable("BinaryData", Context.DUT)).Returns("010111");
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            Datalog.PrintToItuff("--body_type rawbinary_msbF --body_data G.U.S.BinaryData");
            binaryWriterMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void SetAltInstanceName_Pass()
        {
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            Prime.Services.DatalogService = datalogServiceMock.Object;
            datalogServiceMock.Setup(o => o.SetAltInstanceName("SomeName"));
            Datalog.SetAltInstanceName("SomeName");

            datalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void SetAltInstanceName_Empty_Pass()
        {
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            Prime.Services.DatalogService = datalogServiceMock.Object;
            var scratchPadMock = new Mock<IScratchPad>(MockBehavior.Strict);
            scratchPadMock.Setup(o => o.GetScratchPadPair("FunctionArguments")).Returns("SomeName");
            TOSUserSDK.ScratchPad.Service = scratchPadMock.Object;

            datalogServiceMock.Setup(o => o.SetAltInstanceName("SomeName"));
            Datalog.SetAltInstanceName(string.Empty);

            datalogServiceMock.VerifyAll();
            scratchPadMock.VerifyAll();
        }
    }
}