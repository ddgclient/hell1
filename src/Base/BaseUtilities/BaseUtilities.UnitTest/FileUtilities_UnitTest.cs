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
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.FileService;

    /// <summary>
    /// Defines the <see cref="FileUtilities_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FileUtilities_UnitTest
    {
        /// <summary>
        /// Test the Fail paths of FileUtilities.GetFile() - Prime failes .FileExists() check.
        /// </summary>
        [TestMethod]
        public void GetFile_FileExistsFalse()
        {
            var fileMock = new Mock<IFileService>(MockBehavior.Strict);
            fileMock.Setup(o => o.FileExists("TestFileName")).Returns(false);
            Prime.Services.FileService = fileMock.Object;

            Assert.ThrowsException<System.IO.FileNotFoundException>(() => DDG.FileUtilities.GetFile("TestFileName"));
            fileMock.VerifyAll();
        }

        /// <summary>
        /// Test the Fail paths of FileUtilities.GetFile().
        /// </summary>
        [TestMethod]
        public void GetFile_FailsFileNotFoundException()
        {
            var fileName = "TestFileName";
            var fileMock = new Mock<IFileService>(MockBehavior.Strict);
            fileMock.Setup(o => o.FileExists(fileName)).Returns(true);
            fileMock.Setup(o => o.GetFile(fileName)).Throws(new System.IO.FileNotFoundException("File Not Found"));
            Prime.Services.FileService = fileMock.Object;

            Assert.ThrowsException<System.IO.FileNotFoundException>(() => DDG.FileUtilities.GetFile(fileName));
            fileMock.VerifyAll();
        }

        /// <summary>
        /// Test the Fail paths of FileUtilities.GetFile().
        /// </summary>
        [TestMethod]
        public void GetFile_FailsPrimeException()
        {
            var fileName = "TestFileName";
            var fileMock = new Mock<IFileService>(MockBehavior.Strict);
            fileMock.Setup(o => o.FileExists(fileName)).Returns(true);
            fileMock.Setup(o => o.GetFile(fileName)).Throws(new Prime.Base.Exceptions.FatalException("File Not Found"));
            Prime.Services.FileService = fileMock.Object;

            Assert.ThrowsException<Prime.Base.Exceptions.FatalException>(() => DDG.FileUtilities.GetFile(fileName));
            fileMock.VerifyAll();
        }

        /// <summary>
        /// Test the Fail paths of FileUtilities.GetFile().
        /// </summary>
        [TestMethod]
        public void GetFile_FailsReturnsNull()
        {
            var fileName = "TestFileName";
            var fileMock = new Mock<IFileService>(MockBehavior.Strict);
            fileMock.Setup(o => o.FileExists(fileName)).Returns(true);
            fileMock.Setup(o => o.GetFile(fileName)).Returns(string.Empty);
            Prime.Services.FileService = fileMock.Object;

            Assert.ThrowsException<System.IO.FileNotFoundException>(() => DDG.FileUtilities.GetFile(fileName));
            fileMock.VerifyAll();
        }

        /// <summary>
        /// Test the Passing path of FileUtilities.GetFile().
        /// </summary>
        [TestMethod]
        public void GetFile_Pass()
        {
            var fileName = "TestFileName";
            var fileMock = new Mock<IFileService>(MockBehavior.Strict);
            fileMock.Setup(o => o.FileExists(fileName)).Returns(true);
            fileMock.Setup(o => o.GetFile(fileName)).Returns(fileName);
            Prime.Services.FileService = fileMock.Object;

            Assert.AreEqual(fileName, DDG.FileUtilities.GetFile(fileName));
            fileMock.VerifyAll();
        }
    }
}
