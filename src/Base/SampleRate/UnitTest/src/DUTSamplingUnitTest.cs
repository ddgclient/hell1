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
namespace Prime.TestMethods.SampleRate.UnitTest.Src
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.SharedStorageService;
    using Prime.TestMethods;
    using Prime.TestMethods.SampleRate;
    using Prime.UserVarService;

    /// <summary>
    /// Invalid DUT Sampling parameter .
    /// </summary>
    [TestClass]
    public class DUTSamplingUnitTest
    {
        /// <summary>
        ///  To test invalid DUT sampling parameter. Exception will be thrown. To test DutSampling class.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Base.Exceptions.TestMethodException), "Exception must be thrown.")]
        public void Execute_InvalidDUTSamplingParameter_false()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            var validValueUnderTest = new DutSampling("^");

            // Exception will be thrown.
            validValueUnderTest.Verify();
        }

        /// <summary>
        /// DUTSamplingParameterCannotBeZero, To test DutSampling class, throws exeception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Base.Exceptions.TestMethodException), "Exception must be thrown.")]
        public void Execute_DUTSamplingParameterCannotBeZero_False()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            consoleServiceMock.Setup(console => console.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            var validValueUnderTest = new DutSampling("0");
            validValueUnderTest.Verify();
        }

        /// <summary>
        ///  To Test DutSampling class, All Valid DUT Sampling Parameter. To test DutSampling class.
        /// </summary>
        [TestMethod]
        public void Execute_ValidDUTSamplingParameter_True()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var validValueUnderTest = new DutSampling("2");
            var verifyStatus = validValueUnderTest.Verify();
            Assert.IsTrue(verifyStatus);

            // Sampling is not done
            Assert.IsFalse(validValueUnderTest.Execute());
            Assert.IsTrue(validValueUnderTest.Execute());
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        ///  Empty DUT sampling Parameter. To test DutSampling class.
        /// </summary>
        [ExpectedException(typeof(Base.Exceptions.TestMethodException), "Exception must be thrown.")]
        [TestMethod]
        public void Execute_EmptyDUTSamplingParameter_False()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            consoleServiceMock.Setup(console => console.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            var validValueUnderTest = new DutSampling(string.Empty);
            validValueUnderTest.Verify();
        }

        /// <summary>
        ///  DUT sampling Parameter set 1. To test DutSampling class.
        /// </summary>
        [TestMethod]
        public void Execute_DUTSampleCountSet1_true()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var validValueUnderTest = new DutSampling("1");
            var verifyStatus = validValueUnderTest.Verify();

            verifyStatus = validValueUnderTest.Execute();
            Assert.IsTrue(verifyStatus);
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        ///  DUT sampling Parameter set 2, To exercise fasle condition. To test DutSampling class.
        /// </summary>
        [TestMethod]
        public void Execute_DUTSampleCountSet2_false()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            var validValueUnderTest = new DutSampling("2");
            var verifyStatus = validValueUnderTest.Verify();
            Assert.IsTrue(verifyStatus);

            verifyStatus = validValueUnderTest.Execute();
            Assert.IsFalse(verifyStatus);
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        ///  Get DUT sampling rate value from shared storage key. .
        /// </summary>
        [TestMethod]
        public void Execute_DUTSampleValueFromSharedStorage_True()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(true);
            sharedStorageServiceMock.Setup(service => service.GetIntegerRowFromTable("sampleRateValue", Prime.SharedStorageService.Context.LOT)).Returns(1);

            var validValueUnderTest = new DutSampling("sampleRateValue");
            Assert.IsTrue(validValueUnderTest.Verify());
            consoleServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        ///  Get DUT sampling rate value from userVar.
        /// </summary>
        [TestMethod]
        public void Execute_DUTSampleValueFromUserVar_True()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists("_UserVars.SampleRateValue")).Returns(true);
            userVarServiceMock.Setup(uservar => uservar.GetIntValue("_UserVars.SampleRateValue")).Returns(1);

            Prime.Services.UserVarService = userVarServiceMock.Object;

            var validValueUnderTest = new DutSampling("_UserVars.SampleRateValue");
            Assert.IsTrue(validValueUnderTest.Verify());

            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
        }
    }
}
