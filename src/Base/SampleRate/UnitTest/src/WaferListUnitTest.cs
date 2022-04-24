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
    /// Unit test to test WaferListSampling class.
    /// </summary>
    [TestClass]
    public class WaferListUnitTest
    {
        /// <summary>
        ///  All Wafer List parameter valid. To test WaferListSampling class.
        /// </summary>
        [TestMethod]
        public void Execute_ValidWaferListSamplingParameter_True()
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
            DutSampling dutSample = new DutSampling("2");
            var validValueUnderTest = new WaferListSampling("123", dutSample);
            var verifyStatus = validValueUnderTest.Verify();
            Assert.IsTrue(verifyStatus);
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Both Wafer List and Input parameter is empty .To test WaferListSampling class.
        /// </summary>
        [TestMethod]
        public void Execute_InvalidWaferListSamplingParameter_False()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            consoleServiceMock.Setup(console => console.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var stationControllerServiceMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerServiceMock.Object;
            stationControllerServiceMock.Setup(o => o.Get("SC_WAFER_LIST")).Returns(string.Empty);

            // [2] Creates object
            DutSampling dutSample = new DutSampling("2");
            var validValueUnderTest = new WaferListSampling(string.Empty, dutSample);
            var verifyStatus = validValueUnderTest.Verify();
            Assert.IsFalse(verifyStatus);
            consoleServiceMock.VerifyAll();
            stationControllerServiceMock.VerifyAll();
        }

        /// <summary>
        ///  Empty Lot name  parameter .To test WaferListSampling class.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Base.Exceptions.TestMethodException), "Exception must be thrown.")]
        public void Execute_LotNameEmpty_False()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var stationControllerServiceMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerServiceMock.Object;
            stationControllerServiceMock.Setup(o => o.Get("SC_LOT_WAFER")).Returns(string.Empty);
            stationControllerServiceMock.Setup(o => o.Get("SC_WAFER_LIST")).Returns(string.Empty);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferListSampling("1", dutSample);
            var verifyStatus = validValueUnderTest.Verify();
            Assert.IsTrue(verifyStatus);
            validValueUnderTest.Execute();
            stationControllerServiceMock.VerifyAll();
        }

        /// <summary>
        ///  Wafer Id and Lot Id does not match.To test WaferListSampling class.
        /// </summary>
        [TestMethod]
        public void Execute_WaferListIdAndCurrentLotIdNoMatch_False()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var stationControllerServiceMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerServiceMock.Object;
            stationControllerServiceMock.Setup(o => o.Get("SC_LOT_WAFER")).Returns("345");

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferListSampling("123", dutSample);
            var verifyStatus = validValueUnderTest.Verify();
            Assert.IsTrue(verifyStatus);
            Assert.IsFalse(validValueUnderTest.Execute());
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            stationControllerServiceMock.VerifyAll();
        }

        /// <summary>
        ///  Wafer Id and Lot Id match.To test WaferListSampling class.
        /// </summary>
        [TestMethod]
        public void Execute_WaferIdLotIdMatch_True()
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

            var stationControllerServiceMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerServiceMock.Object;
            stationControllerServiceMock.Setup(o => o.Get("SC_LOT_WAFER")).Returns("345");

            // [2] Creates object
            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferListSampling("345", dutSample);
            var verifyStatus = validValueUnderTest.Verify();
            Assert.IsTrue(verifyStatus);
            Assert.IsTrue(validValueUnderTest.Execute());
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            stationControllerServiceMock.VerifyAll();
        }

        /// <summary>
        ///  User Input is empty, but wafer list is obtained from SC_WAFER_LIST.
        /// </summary>
        [TestMethod]
        public void Execute_WaferListFromSCVariable_True()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var stationControllerServiceMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerServiceMock.Object;
            stationControllerServiceMock.Setup(o => o.Get("SC_WAFER_LIST")).Returns("345");
            stationControllerServiceMock.Setup(o => o.Get("SC_LOT_WAFER")).Returns("345");

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            DutSampling dutSample = new DutSampling("1");
            var validValueUnderTest = new WaferListSampling(string.Empty, dutSample);
            var verifyStatus = validValueUnderTest.Verify();
            Assert.IsTrue(verifyStatus);
            Assert.IsTrue(validValueUnderTest.Execute());
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            stationControllerServiceMock.VerifyAll();
        }
    }
}
