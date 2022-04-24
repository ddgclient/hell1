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

namespace AnalogFuncCaptureCtv.UnitTest
{
    using System;
    using global::AnalogFuncCaptureCtv.ConfigurationFile;
    using global::AnalogFuncCaptureCtv.UnitTest.TestInputFiles;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime;
    using Prime.ConsoleService;
    using Prime.FileService;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class AnalogFuncCaptureCtv_Verify_UnitTest : AnalogFuncCaptureCtv
    {
        private const string TestFileGenericPass = "TestFileGenericPass.csv";

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AnalogFuncCaptureCtv_CustomVerify_ParamPinRenameWithDifferentCountFromCtvCapturePins_False()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(x => x.PrintDebug("Parameter PinRename have different item count from CtvCapturePins. Expected count: [3] PinRename count: [2]"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var toTest = new AnalogFuncCaptureCtv
            {
                CtvCapturePins = "PIN1,PIN2,PIN3",
                Kill = AnalogFuncCaptureCtv.EnabledDisabled.DISABLED,
                PinRename = "PIN11,PIN12",
                ConfigurationFile = " ",
            };

            toTest.CustomVerify();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void AnalogFuncCaptureCtv_CustomVerify_ReadConfigurationFile_True()
        {
            // Mocks
            var fileHandlerMock = new Mock<IFileHandler>(MockBehavior.Strict);
            var configurationFileContent = InputFileReader.ReadResourceFile(TestFileGenericPass);
            fileHandlerMock.Setup(x => x.ReadAllLines(TestFileGenericPass)).Returns(configurationFileContent);
            this.FileHandler = fileHandlerMock.Object;

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(x => x.GetFile(TestFileGenericPass))
                .Returns(TestFileGenericPass);
            Prime.Services.FileService = fileServiceMock.Object;

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(x => x.PrintDebug("[INFO] This is the InputFile: " + TestFileGenericPass + " " + TestFileGenericPass));

            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50,DDR1_DQ51";
            this.Kill = AnalogFuncCaptureCtv.EnabledDisabled.DISABLED;
            this.PinRename = "DDR1_DQ50,DDR1_DQ51";
            this.ConfigurationFile = TestFileGenericPass;

            // Execution & Assert
            this.CustomVerify();

            // Mock Verify
            fileHandlerMock.VerifyAll();
            consoleServiceMock.VerifyAll();
            fileServiceMock.VerifyAll();
        }
    }
}
