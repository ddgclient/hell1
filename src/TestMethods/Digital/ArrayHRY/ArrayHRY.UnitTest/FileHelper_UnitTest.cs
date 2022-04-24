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

namespace ArrayHRY.UnitTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime;
    using Prime.ConsoleService;
    using Prime.FileService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Defines the <see cref="FileHelper_UnitTest" />.
    /// </summary>
    [TestClass]
    public class FileHelper_UnitTest
    {
        /// <summary>
        /// Initialize method to setup all common mocks.
        /// </summary>
        [TestInitialize]
        public void TestSetup()
        {
            // Ignore any print messages.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(p => p.PrintDebug(It.IsAny<string>())).Callback((string msg) => Console.WriteLine(msg));
            consoleServiceMock.Setup(p => p.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int i, string s1, string s2) => Console.WriteLine("ERROR:" + msg));
            Prime.Services.ConsoleService = consoleServiceMock.Object;
        }

        /// <summary>
        /// Check the Exception thrown when Prime cannot find the file.
        /// </summary>
        [TestMethod]
        public void HasFileChanged_FileNotFound_Exception()
        {
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.GetNormalizedPath("missing_file")).Returns("normalized_missing_file");
            fileServiceMock.Setup(o => o.GetFile("normalized_missing_file")).Returns(string.Empty);
            Prime.Services.FileService = fileServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInStringTable(It.IsAny<string>(), Context.LOT)).Returns(false);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var file = new FileHelper();

            var ex = Assert.ThrowsException<System.IO.FileNotFoundException>(() => file.HasFileChanged("missing_file", out var temp));
            Assert.AreEqual("GetFile(normalized_missing_file) returned an empty path.", ex.Message);
            Assert.AreEqual("missing_file", ex.FileName);

            fileServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Check the case where the file is not in the cache.
        /// </summary>
        [TestMethod]
        public void HasFileChanged_NotInCache_True()
        {
            var fileName = "SomeFileName";
            var normalizedFileName = "NormalizedPathToFile";
            var localFileName = "LocalFileName";

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.GetNormalizedPath(fileName)).Returns(normalizedFileName);
            fileServiceMock.Setup(o => o.GetFile(normalizedFileName)).Returns(localFileName);
            fileServiceMock.Setup(o => o.GetLastModificationTime(localFileName)).Returns(new DateTime(1));
            Prime.Services.FileService = fileServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInStringTable($"__ARRAYHRY_FILECACHE__.{normalizedFileName}", Context.LOT)).Returns(false);
            sharedStorageMock.Setup(o => o.InsertRowAtTable($"__ARRAYHRY_FILECACHE__.{normalizedFileName}", localFileName, Context.LOT));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var file = new FileHelper();
            var result = file.HasFileChanged(fileName, out var returnedLocalFile);
            Assert.IsTrue(result);
            Assert.AreEqual(localFileName, returnedLocalFile);

            fileServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Check the case where the cache is set to force a the file to be re-pulled.
        /// </summary>
        [TestMethod]
        public void HasFileChanged_InCacheForceReload_True()
        {
            var fileName = "SomeFileName";
            var normalizedFileName = "NormalizedPathToFile";
            var localFileName = "LocalFileName";

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.GetNormalizedPath(fileName)).Returns(normalizedFileName);
            fileServiceMock.Setup(o => o.GetFile(normalizedFileName)).Returns(localFileName);
            fileServiceMock.Setup(o => o.GetLastModificationTime(localFileName)).Returns(new DateTime(1));
            Prime.Services.FileService = fileServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInStringTable($"__ARRAYHRY_FILECACHE__.{normalizedFileName}", Context.LOT)).Returns(true);
            sharedStorageMock.Setup(o => o.GetStringRowFromTable($"__ARRAYHRY_FILECACHE__.{normalizedFileName}", Context.LOT)).Returns("RELOAD");
            sharedStorageMock.Setup(o => o.InsertRowAtTable($"__ARRAYHRY_FILECACHE__.{normalizedFileName}", localFileName, Context.LOT));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var file = new FileHelper();
            var result = file.HasFileChanged(fileName, out var returnedLocalFile);
            Assert.IsTrue(result);
            Assert.AreEqual(localFileName, returnedLocalFile);

            fileServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Check the case where the file is in the cache, but is out of date.
        /// </summary>
        [TestMethod]
        public void HasFileChanged_InCacheOutOfDate_True()
        {
            var fileName = "SomeFileName";
            var normalizedFileName = "NormalizedPathToFile";
            var localFileName = "LocalFileName";
            var outOfDateLocalFileName = "OutOfDateLocalFileName";

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.GetNormalizedPath(fileName)).Returns(normalizedFileName);
            fileServiceMock.Setup(o => o.GetFile(normalizedFileName)).Returns(localFileName);
            fileServiceMock.Setup(o => o.GetLastModificationTime(outOfDateLocalFileName)).Returns(new DateTime(1));
            fileServiceMock.Setup(o => o.GetLastModificationTime(normalizedFileName)).Returns(new DateTime(100));
            fileServiceMock.Setup(o => o.GetLastModificationTime(localFileName)).Returns(new DateTime(1000));
            Prime.Services.FileService = fileServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInStringTable($"__ARRAYHRY_FILECACHE__.{normalizedFileName}", Context.LOT)).Returns(true);
            sharedStorageMock.Setup(o => o.GetStringRowFromTable($"__ARRAYHRY_FILECACHE__.{normalizedFileName}", Context.LOT)).Returns(outOfDateLocalFileName);
            sharedStorageMock.Setup(o => o.InsertRowAtTable($"__ARRAYHRY_FILECACHE__.{normalizedFileName}", localFileName, Context.LOT));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var file = new FileHelper();
            var result = file.HasFileChanged(fileName, out var returnedLocalFile);
            Assert.IsTrue(result);
            Assert.AreEqual(localFileName, returnedLocalFile);

            fileServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Check the case where the file is in the cache and up-to-date, but the current object is not.
        /// </summary>
        [TestMethod]
        public void HasFileChanged_InCacheUpToDate_True()
        {
            var fileName = "SomeFileName";
            var normalizedFileName = "NormalizedPathToFile";
            var localFileName = "LocalFileName";

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.GetNormalizedPath(fileName)).Returns(normalizedFileName);
            fileServiceMock.Setup(o => o.GetLastModificationTime(normalizedFileName)).Returns(new DateTime(100));
            fileServiceMock.Setup(o => o.GetLastModificationTime(localFileName)).Returns(new DateTime(100));
            Prime.Services.FileService = fileServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInStringTable($"__ARRAYHRY_FILECACHE__.{normalizedFileName}", Context.LOT)).Returns(true);
            sharedStorageMock.Setup(o => o.GetStringRowFromTable($"__ARRAYHRY_FILECACHE__.{normalizedFileName}", Context.LOT)).Returns(localFileName);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var file = new FileHelper();
            var result = file.HasFileChanged(fileName, out var returnedLocalFile);
            Assert.IsTrue(result);
            Assert.AreEqual(localFileName, returnedLocalFile);

            fileServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Check the case where both the cache and object are up to date.
        /// </summary>
        [TestMethod]
        public void HasFileChanged_InCacheUpToDate_False()
        {
            var fileName = "SomeFileName";
            var normalizedFileName = "NormalizedPathToFile";
            var localFileName = "LocalFileName";

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(o => o.GetNormalizedPath(fileName)).Returns(normalizedFileName);
            fileServiceMock.Setup(o => o.GetLastModificationTime(normalizedFileName)).Returns(new DateTime(100));
            fileServiceMock.Setup(o => o.GetLastModificationTime(localFileName)).Returns(new DateTime(100));
            Prime.Services.FileService = fileServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInStringTable($"__ARRAYHRY_FILECACHE__.{normalizedFileName}", Context.LOT)).Returns(true);
            sharedStorageMock.Setup(o => o.GetStringRowFromTable($"__ARRAYHRY_FILECACHE__.{normalizedFileName}", Context.LOT)).Returns(localFileName);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var file = new FileHelper();

            // call it once to get the object up to date.
            var result1 = file.HasFileChanged(fileName, out var returnedLocalFile1);
            Assert.IsTrue(result1);
            Assert.AreEqual(localFileName, returnedLocalFile1);

            // 2nd call, object and cache are correct.
            var result2 = file.HasFileChanged(fileName, out var returnedLocalFile2);
            Assert.IsFalse(result2);

            fileServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }
    }
}
