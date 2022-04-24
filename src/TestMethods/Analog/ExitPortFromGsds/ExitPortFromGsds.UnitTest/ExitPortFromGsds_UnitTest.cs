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

namespace ExitPortFromGsds.UnitTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime;
    using Prime.ConsoleService;
    using Prime.FileService;
    using Prime.SharedStorageService;
    using Prime.TestMethods;
    using Prime.UserVarService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class ExitPortFromGsds_UnitTest
    {
        /// <summary>
        /// Fails Verify(): empty InputFile instance param.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_ParamEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("InputFile must be a valid JSON file.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            ExitPortFromGsds underTest = new ExitPortFromGsds { InputFile = string.Empty };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Passes Verify(): InputFile found.
        /// </summary>
        [TestMethod]
        public void Verify_NonEmptyInputFile_True()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            string inputFile = @"..\..\src\TestMethods\Analog\ExitPortFromGsds\ExitPortFromGsds.UnitTest\gsdsExitPorts.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            Prime.Services.FileService = fileServiceMock.Object;

            ExitPortFromGsds underTest = new ExitPortFromGsds { InputFile = inputFile };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Fails Verify(): InputFile missing GsdsName and UserVarName.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_NullGsdsUserVarInputFile_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            string inputFile = @"..\..\src\TestMethods\Analog\ExitPortFromGsds\ExitPortFromGsds.UnitTest\nullExitPorts.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock ConsoleService
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("GsdsName and UserVarName can't both be null!", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            ExitPortFromGsds underTest = new ExitPortFromGsds { InputFile = inputFile };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Fail Verify(): InputFile contains both GsdsName and UserVarName.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_BothGsdsUserVarInputFile_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            string inputFile = @"..\..\src\TestMethods\Analog\ExitPortFromGsds\ExitPortFromGsds.UnitTest\bothExitPorts.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock ConsoleService
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("GsdsName and UserVarName can't both be used!", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            ExitPortFromGsds underTest = new ExitPortFromGsds { InputFile = inputFile };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Fail Verify(): InputFile contains UserVarName but UserVar doesn't exist.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_UserVarDoesntExist_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            string inputFile = @"..\..\src\TestMethods\Analog\ExitPortFromGsds\ExitPortFromGsds.UnitTest\uservarExitPorts.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock ConsoleService
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("UserVarName doesn't exist!", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // Mock UserVarService
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(userVarService => userVarService.Exists("CLK_LJPLL_ALL.ENGR_DATA")).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            ExitPortFromGsds underTest = new ExitPortFromGsds { InputFile = inputFile };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Passes Execute(): GSDS contains "LONGMV".
        /// ExitPorts contains ("LONGMV", 3).
        /// </summary>
        [TestMethod]
        public void Execute_GsdsContainsLongMV_Return3()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            string inputFile = @"..\..\src\TestMethods\Analog\ExitPortFromGsds\ExitPortFromGsds.UnitTest\gsdsExitPorts.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock SharedService to get GSDS
            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(sharedService => sharedService.GetStringRowFromTable(It.IsAny<string>(), Context.DUT)).Returns("LongMV");
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            ExitPortFromGsds underTest = new ExitPortFromGsds { InputFile = inputFile };

            // [2] Call the method under test.
            underTest.Verify();
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(3, executeResult);
        }

        /// <summary>
        /// Passes Verify(): InputFile contains UserVarName and UserVar exists.
        /// </summary>
        [TestMethod]
        public void Verify_UserVarExists_True()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            string inputFile = @"..\..\src\TestMethods\Analog\ExitPortFromGsds\ExitPortFromGsds.UnitTest\uservarExitPorts.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock ConsoleService
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError("UserVarName doesn't exist!", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // Mock UserVarService
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(userVarService => userVarService.Exists("CLK_LJPLL_ALL.ENGR_DATA")).Returns(true);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            ExitPortFromGsds underTest = new ExitPortFromGsds { InputFile = inputFile };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Fails Execute(): ExitPorts key not found.
        /// </summary>
        [TestMethod]
        public void Execute_FlowNotFound_Return0()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            string inputFile = @"..\..\src\TestMethods\Analog\ExitPortFromGsds\ExitPortFromGsds.UnitTest\gsdsExitPorts.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // Mock SharedService to get GSDS
            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(sharedService => sharedService.GetStringRowFromTable(It.IsAny<string>(), Context.DUT)).Returns("Bunk");
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            // Mock ConsoleService to return error message
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintError($"Module flow Bunk doesn't exist in ExitPorts dict!", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            ExitPortFromGsds underTest = new ExitPortFromGsds { InputFile = inputFile };

            // [2] Call the method under test.
            underTest.Verify();
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(0, executeResult);
        }

        /// <summary>
        /// Passes Execute(): UserVar contains "HVM".
        /// ExitPorts contains ("LONGMV", 3).
        /// </summary>
        [TestMethod]
        public void Execute_UserVarContainsHVM_Return3()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // Mock FileService
            string inputFile = @"..\..\src\TestMethods\Analog\ExitPortFromGsds\ExitPortFromGsds.UnitTest\uservarExitPorts.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(inputFile);
            Prime.Services.FileService = fileServiceMock.Object;

            // need to figure out how to use relative paths for inputFile, below doesnt account for gitlab repo?
            /*string inputFile = @".\uservarExitPorts.json";
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(fileService => fileService.FileExists(It.IsAny<string>())).Returns(true);
            fileServiceMock.Setup(fileService => fileService.GetFile(inputFile)).Returns(Environment.CurrentDirectory + "\\..\\..\\ExitPortFromGsds\\ExitPortFromGsds.UnitTest\\" + inputFile);
            Prime.Services.FileService = fileServiceMock.Object;*/

            // Mock UserVarService
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(userVarService => userVarService.Exists("CLK_LJPLL_ALL.ENGR_DATA")).Returns(true);
            userVarServiceMock.Setup(userVarService => userVarService.GetStringValue("CLK_LJPLL_ALL.ENGR_DATA")).Returns("HVM");
            Prime.Services.UserVarService = userVarServiceMock.Object;

            ExitPortFromGsds underTest = new ExitPortFromGsds { InputFile = inputFile };

            // [2] Call the method under test.
            underTest.Verify();
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(4, executeResult);
        }
    }
}
