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
    using System.Collections.Generic;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.PerformanceService;
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="PlistCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class PlistCallbacks_UnitTest
    {
        private Mock<IPlistModifications> plistModifications;
        private Mock<ITestProgramService> testProgramMock;

        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            this.plistModifications = new Mock<IPlistModifications>(MockBehavior.Strict);
            DDG.PlistModifications.Service = this.plistModifications.Object;

            this.testProgramMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            Prime.Services.TestProgramService = this.testProgramMock.Object;

            var consoleMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) => Console.WriteLine(msg));
            consoleMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleMock.Object;

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void RestorePlist_Args()
        {
            this.plistModifications.Setup(o => o.RestoreTree("SomePatlist"));
            Plist.RestorePlist("SomePatlist");
            this.plistModifications.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void CleanPlist_Args()
        {
            this.plistModifications.Setup(o => o.CleanTree("SomePatlist"));
            Plist.CleanPlist("SomePatlist");
            this.plistModifications.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void RestorePlist_NoArgs()
        {
            this.plistModifications.Setup(o => o.RestoreTree("SomePatlist"));
            this.testProgramMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string> { { "Patlist", "SomePatlist" } });
            Plist.RestorePlist(string.Empty);
            this.plistModifications.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void CleanPlist_NoArgs()
        {
            this.plistModifications.Setup(o => o.CleanTree("SomePatlist"));
            this.testProgramMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string> { { "Patlist", "SomePatlist" } });
            Plist.CleanPlist(string.Empty);
            this.plistModifications.VerifyAll();
        }
    }
}