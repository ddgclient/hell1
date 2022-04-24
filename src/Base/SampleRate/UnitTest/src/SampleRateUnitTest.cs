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

namespace Prime.TestMethods.SampleRate.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.SharedStorageService;
    using Prime.StationControllerService;
    using Prime.TestMethods;
    using Prime.TestMethods.SampleRate;
    using Prime.UserVarService;

    /// <summary>
    /// Unit test to test PrimeSampleRateTestMethod class.
    /// </summary>
    [TestClass]
    public class SampleRateUnitTest
    {
        /// <summary>
        ///  Unit Test function to validate DUT sampling.
        /// </summary>
        [TestMethod]
        public void Execute_DUTSampling_True()
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
            var validValueUnderTest = new PrimeSampleRateTestMethod
            {
                SamplingRateValue = "2",
                SampleOption = PrimeSampleRateTestMethod.SampleOptions.DUT_SAMPLING,
            };
            validValueUnderTest.Verify();

            // Sampling is not done
            var samplingStatus = validValueUnderTest.Execute();
            Assert.AreEqual(2, samplingStatus);

            // sampling is done
            samplingStatus = validValueUnderTest.Execute();
            Assert.AreEqual(1, samplingStatus);
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Unit test function to validate ituff logging part.
        /// </summary>
        [TestMethod]
        public void Execute_ituffLogging_true()
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
            var validValueUnderTest = new PrimeSampleRateTestMethod
            {
                SamplingRateValue = "1",
                PrintItuffExitPort = ItuffEnabled.True,
                SampleOption = PrimeSampleRateTestMethod.SampleOptions.DUT_SAMPLING,
            };
            validValueUnderTest.Verify();

            var ituffMrsltMock = new Mock<IMrsltFormat>(MockBehavior.Strict);
            ituffMrsltMock.Setup(mrslt => mrslt.SetData(It.IsAny<double>()));
            ituffMrsltMock.Setup(mrslt => mrslt.SetPrecision(It.IsAny<uint>()));
            ituffMrsltMock.Setup(mrslt => mrslt.SetTnamePostfix(It.IsAny<string>()));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(dlog => dlog.GetItuffMrsltWriter()).Returns(ituffMrsltMock.Object);
            datalogServiceMock.Setup(dlog => dlog.WriteToItuff(ituffMrsltMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            // Sampling is done
            var samplingStatus = validValueUnderTest.Execute();
            Assert.AreEqual(1, samplingStatus);
            sharedStorageServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            consoleServiceMock.VerifyAll();
            ituffMrsltMock.VerifyAll();
            datalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_AllSampleRateParametersValid_True()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [1] Verifies the valid value
            var validValueUnderTest = new PrimeSampleRateTestMethod
            {
                SamplingRateValue = "5",
                PrintItuffExitPort = ItuffEnabled.True,
                SampleOption = PrimeSampleRateTestMethod.SampleOptions.DUT_SAMPLING,
            };

            validValueUnderTest.Verify();
            consoleServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Sampling rate parameter is empty, throws exception.
        /// </summary>
        [TestMethod]
        public void Verify_SampleRateValueParameterEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Verifies the empty value
            var emptyValueUnderTest = new PrimeSampleRateTestMethod
            {
                SamplingRateValue = string.Empty,
                SampleOption = PrimeSampleRateTestMethod.SampleOptions.DUT_SAMPLING,
            };
            TestMethodException ex = Assert.ThrowsException<TestMethodException>(() => emptyValueUnderTest.Verify());
            Assert.AreEqual("SamplingRateValue parameter =[] cannot be empty.\n", ex.Message);
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Wafer Rate Sampling option but dont sample current Wafer.
        /// </summary>
        [TestMethod]
        public void Execute_WaferRateNoSample_true()
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

            var stationControllerMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerMock.Object;
            stationControllerMock.Setup(o => o.Get("SC_LOT_WAFER")).Returns("wafer3");

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            var waferRateSampling = new PrimeSampleRateTestMethod
            {
                WaferSampleRateValue = "2",
                SamplingRateValue = "1",
                SampleOption = PrimeSampleRateTestMethod.SampleOptions.WAFER_SAMPLE_RATE,
                PrintItuffExitPort = ItuffEnabled.False,
            };
            waferRateSampling.Verify();

            var waferSamplingStataus = waferRateSampling.Execute();
            Assert.AreEqual(2, waferSamplingStataus);
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            stationControllerMock.VerifyAll();
        }

        /// <summary>
        /// Wafer Sampling option is Wafer List is processed.
        /// </summary>
        [TestMethod]
        public void Execute_WaferListBasedOnUserInput_true()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var stationControllerMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerMock.Object;
            stationControllerMock.Setup(o => o.Get("SC_LOT_WAFER")).Returns("wafer3");

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            var validValueUnderTest = new PrimeSampleRateTestMethod
            {
                SamplingRateValue = "1",
                WaferSampleList = "wafer1,wafer2,wafer3",
                SampleOption = PrimeSampleRateTestMethod.SampleOptions.WAFER_LIST,
            };
            validValueUnderTest.Verify();

            var waferSamplingStataus = validValueUnderTest.Execute();
            Assert.AreEqual(1, waferSamplingStataus);
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            stationControllerMock.VerifyAll();
        }

        /// <summary>
        /// Wafer Sampling option is Wafer List is based on the station controller variable.
        /// </summary>
        [TestMethod]
        public void Execute_WaferListFromSCVarList_true()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var stationControllerMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerMock.Object;
            stationControllerMock.Setup(o => o.Get("SC_LOT_WAFER")).Returns("wafer3");
            stationControllerMock.Setup(o => o.Get("SC_WAFER_LIST")).Returns("wafer1,wafer2,wafer3");

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            var validValueUnderTest = new PrimeSampleRateTestMethod
            {
                SamplingRateValue = "1",
                WaferSampleList = string.Empty,
                SampleOption = PrimeSampleRateTestMethod.SampleOptions.WAFER_LIST,
            };

            validValueUnderTest.Verify();
            var waferSamplingStataus = validValueUnderTest.Execute();
            Assert.AreEqual(1, waferSamplingStataus);
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            stationControllerMock.VerifyAll();
        }

        /// <summary>
        /// Wafer Sampling option is Wafer List and current wafer doesnt match, so sampling done.
        /// </summary>
        [TestMethod]
        public void Execute_CurrentWaferDoesntMatchWaferList_true()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);

            // Here Current wafer ID is "wafer4" not in the Wafer List
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var stationControllerMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerMock.Object;
            stationControllerMock.Setup(o => o.Get("SC_LOT_WAFER")).Returns("wafer4");
            stationControllerMock.Setup(o => o.Get("SC_WAFER_LIST")).Returns("wafer1,wafer2,wafer3");

            // [2] Creates object
            var validValueUnderTest = new PrimeSampleRateTestMethod
            {
                SamplingRateValue = "1",
                WaferSampleList = string.Empty,
                SampleOption = PrimeSampleRateTestMethod.SampleOptions.WAFER_LIST,
            };

            validValueUnderTest.Verify();
            var waferSamplingStatus = validValueUnderTest.Execute();
            Assert.AreEqual(2, waferSamplingStatus);
            consoleServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            stationControllerMock.VerifyAll();
        }

        /// <summary>
        /// Wafer Sampling option is specified, user input wafer List is empty and SC_WAFER_LIST is empty.
        /// </summary>
        [TestMethod]
        public void Execute_WaferListEmpty_false()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            consoleServiceMock.Setup(console => console.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var stationControllerMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerMock.Object;
            stationControllerMock.Setup(o => o.Get("SC_WAFER_LIST")).Returns(string.Empty);

            // [2] Creates object
            var validValueUnderTest = new PrimeSampleRateTestMethod
            {
                SamplingRateValue = "1",
                WaferSampleList = string.Empty,
                SampleOption = PrimeSampleRateTestMethod.SampleOptions.WAFER_LIST,
            };
            TestMethodException ex = Assert.ThrowsException<TestMethodException>(() => validValueUnderTest.Verify());
            Assert.AreEqual("Failed sampleRateFeature Verify.", ex.Message);
            consoleServiceMock.VerifyAll();
            stationControllerMock.VerifyAll();
        }

        /// <summary>
        ///  Test the verify section of catch block.
        /// </summary>
        [TestMethod]
        public void Execute_VerifyTryCatchBlockCoverage_false()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            var validValueUnderTest = new PrimeSampleRateTestMethod
            {
                SamplingRateValue = "^",
                SampleOption = PrimeSampleRateTestMethod.SampleOptions.DUT_SAMPLING,
            };

            TestMethodException ex = Assert.ThrowsException<TestMethodException>(() => validValueUnderTest.Verify());

            // Exception will be thrown.
            Assert.AreEqual("Cannot convert the input sample rate value,incorrect value specified for sampling rate value=[^].\n", ex.Message);
            consoleServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        ///  Test the execute section of catch block.
        /// </summary>
        [TestMethod]
        public void Execute_TryCatchBlockCoverage_false()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console => console.PrintDebug(It.IsAny<string>()));
            consoleServiceMock.Setup(console => console.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(service => service.KeyExistsInIntegerTable(It.IsAny<string>(), Prime.SharedStorageService.Context.LOT)).Returns(false);

            var stationControllerMock = new Mock<IStationControllerService>(MockBehavior.Strict);
            Prime.Services.StationControllerService = stationControllerMock.Object;
            stationControllerMock.Setup(o => o.Get("SC_LOT_WAFER")).Returns(string.Empty);

            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(uservar => uservar.Exists(It.IsAny<string>())).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            // [2] Creates object
            var validValueUnderTest = new PrimeSampleRateTestMethod
            {
                SamplingRateValue = "1",
                WaferSampleList = "123",
                SampleOption = PrimeSampleRateTestMethod.SampleOptions.WAFER_LIST,
            };

            validValueUnderTest.Verify();

            // Exception will be thrown.
            validValueUnderTest.Execute();
            consoleServiceMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            stationControllerMock.VerifyAll();
        }
    }
 }
