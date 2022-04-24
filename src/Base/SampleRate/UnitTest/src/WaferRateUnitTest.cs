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
    using Prime.StationControllerService;
    using Prime.TestMethods;
    using Prime.TestMethods.SampleRate;
    using Prime.UserVarService;

    /// <summary>
    /// Unit test to test WaferRateSampling class.
    /// </summary>
    [TestClass]
    public class WaferRateUnitTest
    {
        /// <summary>
        ///  To test invalid Wafer sampling parameter. Exeception will be thrown. To test DutSampling class.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Base.Exceptions.TestMethodException), "Exception must be thrown.")]
        public void Execute_InvalidWaferSamplingParameter_false()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferRateSampling("^", dutSample);
            validValueUnderTest.Verify();
        }

        /// <summary>
        ///  Valid Wafer Rate parameter. To test WaferRateSampling class.
        /// </summary>
        [TestMethod]
        public void Execute_ValidWaferRateSamplingParameter_True()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // DUT sampling and wafer sampling is true.
            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferRateSampling("1", dutSample);
            var verifyStatus = validValueUnderTest.Verify();
            Assert.IsTrue(verifyStatus);
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        ///  Empty Wafer Rate parameter, To test WaferRateSampling class.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Base.Exceptions.TestMethodException), "Exception must be thrown.")]
        public void Execute_EmptyWaferRateSamplingParameter_False()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            consoleServiceMock.Setup(console => console.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferRateSampling(string.Empty, dutSample);
            validValueUnderTest.Verify();
        }

        /// <summary>
        ///  Invalid Wafer Rate parameter, 0 is not valid. To test WaferRateSampling class.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Base.Exceptions.TestMethodException), "Exception must be thrown.")]
        public void Execute_InValidWaferRateSamplingParameter_False()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            consoleServiceMock.Setup(console => console.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferRateSampling("0", dutSample);
            validValueUnderTest.Verify();
        }

        /// <summary>
        ///  Lot Wafernameis empty . To test WaferRateSampling class.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Base.Exceptions.TestMethodException), "Exception must be thrown.")]
        public void Execute_CurrentWaferIdEmpty_False()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            consoleServiceMock.Setup(console =>
                console.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var stationControllerServiceMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerServiceMock.Object;
            stationControllerServiceMock.Setup(o => o.Get("SC_LOT_WAFER")).Returns(string.Empty);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            // [2] Creates object
            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferRateSampling("1", dutSample);
            var verifyStatus = validValueUnderTest.Verify();
            Assert.IsTrue(verifyStatus);
            validValueUnderTest.Execute();
            stationControllerServiceMock.VerifyAll();
        }

        /// <summary>
        ///  Process First wafer, Current Wafer Id and Previous Id is not equal. Wafer Count and DUT count 1. To test WaferRateSampling class.
        /// </summary>
        [TestMethod]
        public void Execute_ProcessFirstWaferAndWaferRate1_True()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var stationControllerServiceMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerServiceMock.Object;
            stationControllerServiceMock.Setup(o => o.Get("SC_LOT_WAFER")).Returns("123");

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferRateSampling("1", dutSample);
            Assert.IsTrue(validValueUnderTest.Verify());
            Assert.IsTrue(validValueUnderTest.Execute());
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            stationControllerServiceMock.VerifyAll();
        }

        /// <summary>
        ///  Process First wafer, Current Wafer Id and Previous Id is not equal. Wafer Count 2 and DUT count 1. To test WaferRateSampling class.
        /// </summary>
        [TestMethod]
        public void Execute_ProcessFirstWaferAndWaferRatet2_false()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var stationControllerMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerMock.Object;
            stationControllerMock.Setup(o => o.Get("SC_LOT_WAFER")).Returns("123");

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferRateSampling("2", dutSample);
            Assert.IsTrue(validValueUnderTest.Verify());
            Assert.IsFalse(validValueUnderTest.Execute());
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            stationControllerMock.VerifyAll();
        }

        /// <summary>
        ///  Current wafer and previous wafer not same, . Wafer Count 2 and DUT count 1. To test WaferRateSampling class.
        /// </summary>
        [TestMethod]
        public void Execute_CurrentWaferPreviousWaferNotSame_false()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var stationControllerMock1 = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerMock1.Object;
            stationControllerMock1.Setup(o => o.Get("SC_LOT_WAFER")).Returns("123");

            var userVarServiceMock1 = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock1.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock1.Object;

            // [2] First wafer
            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferRateSampling("3", dutSample);
            Assert.IsTrue(validValueUnderTest.Verify());
            Assert.IsFalse(validValueUnderTest.Execute());

            // Set current wafer id 234, previous wafer id 123, both not same.
            var stationControllerMock2 = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerMock2.Object;
            stationControllerMock2.Setup(o => o.Get("SC_LOT_WAFER")).Returns("234");

            // Wafersample Count goes to 2.Current wafer count should be 2 after executing below statement.
            Assert.IsFalse(validValueUnderTest.Execute());

            // Set current wafer id 456, previous wafer id 234.
            var stationControllerMock3 = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerMock3.Object;
            stationControllerMock3.Setup(o => o.Get("SC_LOT_WAFER")).Returns("456");

            // Wafersample Count goes to 2
            Assert.IsTrue(validValueUnderTest.Execute());

            // Set current wafer id 456, previous wafer id 234.
            var stationControllerMock4 = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerMock4.Object;
            stationControllerMock4.Setup(o => o.Get("SC_LOT_WAFER")).Returns("678");

            // Current and previous wafer count is different. Current sample count goes to 1, new wafer rate will be assigned here.
            Assert.IsFalse(validValueUnderTest.Execute());
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        ///  Get Wafer sampling rate value from shared storage key.
        /// </summary>
        [TestMethod]
        public void Execute_WaferSampleValueFromSharedStorage_True()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable("sampleRateValue", Prime.SharedStorageService.Context.LOT)).Returns(true);
            sharedStorageServiceMock.Setup(service => service.GetIntegerRowFromTable("sampleRateValue", Prime.SharedStorageService.Context.LOT)).Returns(1);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferRateSampling("sampleRateValue", dutSample);
            Assert.IsTrue(validValueUnderTest.Verify());
            consoleServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        ///  Get Wafer sampling rate value from userVar.
        /// </summary>
        [TestMethod]
        public void Execute_WaferSampleValueFromUserVar_True()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            userVarServiceMock.Setup(uservar => uservar.Exists("_UserVars.SampleRateValue")).Returns(true);

            userVarServiceMock.Setup(uservar => uservar.GetIntValue("_UserVars.SampleRateValue")).Returns(1);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferRateSampling("_UserVars.SampleRateValue", dutSample);
            Assert.IsTrue(validValueUnderTest.Verify());

            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
        }
    }
}