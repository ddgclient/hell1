// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// INTEL CONFIDENTIAL
// Copyright (2019) (2020) Intel Corporation
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

namespace PrimeValTool_unitTest
{
    using System.Collections.Generic;
    using Wrapper;
    using PrimeValTool;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// UnitTests
    /// </summary>
    [TestClass]
    public class PValMain_UnitTest
    {
        Mock<IEnvironmentVariableHandler> f_envHandler = new Mock<IEnvironmentVariableHandler>();
        Mock<IDirectoryHandler> f_dirHandler = new Mock<IDirectoryHandler>();
        Mock<IInputHandler> f_inpHandler = new Mock<IInputHandler>();

        [TestInitialize]
        public void Init()
        {
            f_envHandler
                .Setup(x => x.RetrievePathFromEnvironmentVariable(It.IsAny<string>()))
                .Returns("C//:FakeDir");
            Handlers.EnvironmentVariableHandler = f_envHandler.Object;

            f_dirHandler
                .Setup(x => x.ReadTextFromPath(It.IsAny<string>()))
                .Returns((string path) =>
                {
                    return System.IO.File.ReadAllText(path);
                });
            Handlers.DirectoryHandler = f_dirHandler.Object;

            PValMain.ToolCollaterals = RuntimeCollaterals.InitializeToolCollaterals();
        }

        /// <summary>
        /// Wipe residual results between tests.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            PValMain.MainTOSVersionToUse = null;
            PValMain.ToolCollaterals = RuntimeCollaterals.InitializeToolCollaterals();
        }

        /// <summary>
        /// Dummy description.
        /// </summary>
        [TestMethod]
        public void GetTOSVersionForExecution_OneArgumentListNotFound_Fail()
        {
            string tosVersion = "C://intel//hdmt//releases//hdmt9999";
            f_dirHandler
                .Setup(x => x.RetrieveListOfDirectories(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new string[1] { tosVersion });
            Handlers.DirectoryHandler = f_dirHandler.Object;

            f_inpHandler
                .Setup(x => x.GetUserInput())
                .Returns("0");
            Handlers.InputHandler = f_inpHandler.Object;

            List<string> availableTosList = new List<string>{ "TosDummy" };
            PValMain.GetTOSVersionForExecution(new string[0]);
        }

        /// <summary>
        /// Successful parse of TOS version that matches with args.
        /// </summary>
        [TestMethod]
        public void GetTOSVersionForExecution_PassedArgs_FoundTosVersion()
        {
            string tosVersion = "C://intel//hdmt//releases//hdmt9999";
            f_dirHandler
                .Setup(x => x.RetrieveListOfDirectories(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new string[1] { tosVersion });
            Handlers.DirectoryHandler = f_dirHandler.Object;
            PValMain.GetTOSVersionForExecution(new string[1] { tosVersion});

            Assert.AreEqual(PValMain.MainTOSVersionToUse, tosVersion);
        }

        /// <summary>
        /// Successful parse of TOS version that matches with args.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void GetTOSVersionForExecution_PassedArgs_NoTOSFound()
        {
            string tosVersion = "C://intel//hdmt//releases//hdmt9999";
            string tosArg = "C://intel//hdmt//releases//hdmt7777";

            f_dirHandler
                .Setup(x => x.RetrieveListOfDirectories(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new string[1] { tosVersion });
            PValMain.GetTOSVersionForExecution(new string[1] { tosArg });
        }

        /// <summary>
        /// Successful parse of TOS version that matches with args.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(System.IO.FileNotFoundException))]
        public void GetTOSVersionForExecution_NoAvailableTOSVersions()
        {
            f_dirHandler
                .Setup(x => x.RetrieveListOfDirectories(It.IsAny<string>()))
                .Returns(new string[0]);

            PValMain.GetTOSVersionForExecution(new string[0]);
        }

        /// <summary>
        /// Dummy description.
        /// </summary>
        [TestMethod]
        public void MatchMainArgsToAvailableVersions_OneArgument_EmptyList_Fail()
        {
            string[] availableTosList = new string[0];
            var read = "1";
            bool returnValue = PValMain.MatchMainArgsToAvailableVersions(read, availableTosList, out string tosVersion);
            Assert.IsFalse(returnValue);
        }

        /// <summary>
        /// Dummy description.
        /// </summary>
        [TestMethod]
        public void ParseTosName_PopulatingTosName_ReturnParsedTosName()
        {
            PValMain.MainTOSVersionToUse = "C:\\TosDummy_3.1";
            string[] availableTosList = new string[] { "C:\\TosDummy_3.1", "C:\\TosDummy1_3.2" };
            PValMain.ParseTosName(availableTosList);
            Assert.AreEqual("TOS31", PValMain.TosName);
        }
    }
}