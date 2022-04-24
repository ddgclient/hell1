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

    /// <summary>
    /// Defines the <see cref="OnlyMeCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class OnlyMeCallbacks_UnitTest
    {
        private Mock<Prime.ThreadAlignmentService.IThreadAlignmentService> onlymeServiceMock;

        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleMock.Setup(o => o.PrintDebug(It.IsAny<string>()))
                .Callback((string message) => System.Console.WriteLine("DEBUG " + message));
            consoleMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string message, int line, string member, string file) => System.Console.WriteLine("ERROR " + message));
            Prime.Services.ConsoleService = consoleMock.Object;

            this.onlymeServiceMock = new Mock<Prime.ThreadAlignmentService.IThreadAlignmentService>(MockBehavior.Strict);
            Prime.Services.ThreadAlignmentService = this.onlymeServiceMock.Object;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void AcquireOnlyMeLock_Pass()
        {
            // setup mocks.
            this.onlymeServiceMock.Setup(o => o.AcquireOnlyMeLock(1000));

            // compare results.
            OnlyMe.AcquireOnlyMeLock("1000");

            this.onlymeServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void AcquireOnlyMeLock_NotANumber_Fail()
        {
            // setup mocks.
            // check throw exception message matching with production code.
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => OnlyMe.AcquireOnlyMeLock("This is not a number"));
            Assert.AreEqual("Argument must be a number.", ex.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void AcquireOnlyMeLock_Empty_Fail()
        {
            // setup mocks.
            // check throw exception message matching with production code.
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => OnlyMe.AcquireOnlyMeLock(string.Empty));
            Assert.AreEqual("Argument should not be empty.", ex.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ReleaseOnlyMeLock_Pass()
        {
            // setup mocks.
            this.onlymeServiceMock.Setup(o => o.ReleaseOnlyMeLock());

            // compare results.
            OnlyMe.ReleaseOnlyMeLock(string.Empty);

            this.onlymeServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ReleaseOnlyMeLock_Fail()
        {
            // setup mocks.
            this.onlymeServiceMock.Setup(o => o.ReleaseOnlyMeLock()).Throws<Exception>();

            // check throw exception message matching with production code.
            var ex = Assert.ThrowsException<Exception>(() => OnlyMe.ReleaseOnlyMeLock(string.Empty));
            Assert.AreEqual("Exception of type 'System.Exception' was thrown.", ex.Message);

            this.onlymeServiceMock.VerifyAll();
        }
    }
}
