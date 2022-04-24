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

namespace DcPrint.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.DcService;
    using Prime.PinService;
    using Prime.SharedStorageService;
    using Prime.TestConditionService;
    using Prime.TestMethods;
    using Prime.TestMethods.Dc;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class DcPrint_UnitTest
    {
        private static string ituffTname = $"2_tname_TESTNAME";
        private static string ituffVal = $"2_mrsltval";
        private Mock<IDcResults> mockResult;
        private Mock<List<IPinGroupDcResults>> mockDCGroup;
        private Mock<IPinGroupDcResults> mockDCGroupResults;
        private Mock<List<IPinDcResults>> mockDCResults;
        private Mock<IPinDcResults> result;
        private Mock<IDatalogService> datalogServiceMock;
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IMrsltFormat> mrsltFormatMock;

        /// <summary>
        /// Mock the console logger.
        /// </summary>
        [TestInitialize]
        public void SetupCommonMocks()
        {
            // Console
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => Console.WriteLine(s));
            this.consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;
        }

        /// <summary>
        /// Null data test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamEmpty_Pass()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleMock.Setup(o => o.PrintDebug("CustomPostProcessResults: [results] is null, setting exit port to 0."));
            Prime.Services.ConsoleService = consoleMock.Object;

            DcPrint dcPrint = new DcPrint();

            // [2] Call the method under test.
            dcPrint.TestMethodExtension = dcPrint;
            dcPrint.TestMethodExtension.CustomPostProcessResults(null);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            //  Cannot access ExitPort anymore, use a print statement to verify the correct action was taken.
            // TODO: should replace this with a full test of the Execute method where the return value can be tested.
            // Assert.AreEqual(dcPrint.ExitPort, 0);
            consoleMock.Verify(o => o.PrintDebug("CustomPostProcessResults: [results] is null, setting exit port to 0."), Times.Once);
        }

        /// <summary>
        /// Empty data test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamEmpty_Input_Pass()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleMock.Setup(o => o.PrintDebug("CustomPostProcessResults: GetAllPinGroupsDcResults returned null or .Count==0. Setting exit port to 0."));
            Prime.Services.ConsoleService = consoleMock.Object;

            DcPrint dcPrint = new DcPrint();

            this.mockResult = new Mock<IDcResults>(MockBehavior.Strict);
            this.mockDCGroup = new Mock<List<IPinGroupDcResults>>(MockBehavior.Strict);
            this.mockResult.Setup(x => x.GetAllPinGroupsDcResults()).Returns(this.mockDCGroup.Object);

            // [2] Call the method under test.
            dcPrint.TestMethodExtension = dcPrint;
            dcPrint.TestMethodExtension.CustomPostProcessResults(this.mockResult.Object);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            //  Cannot access ExitPort anymore, use a print statement to verify the correct action was taken.
            // TODO: should replace this with a full test of the Execute method where the return value can be tested.
            // Assert.AreEqual(dcPrint.ExitPort, 0);
            consoleMock.Verify(o => o.PrintDebug("CustomPostProcessResults: GetAllPinGroupsDcResults returned null or .Count==0. Setting exit port to 0."), Times.Once);
        }

        /// <summary>
        /// Null data test for pin group data.
        /// </summary>
        [TestMethod]
        public void Verify_ParamEmpty_NullInput_Pass()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleMock.Setup(o => o.PrintDebug("CustomPostProcessResults: GetAllPinGroupsDcResults returned null or .Count==0. Setting exit port to 0."));
            Prime.Services.ConsoleService = consoleMock.Object;

            DcPrint dcPrint = new DcPrint();

            this.mockResult = new Mock<IDcResults>(MockBehavior.Strict);
            this.mockResult.Setup(x => x.GetAllPinGroupsDcResults()).Returns(() => null);

            // [2] Call the method under test.
            dcPrint.TestMethodExtension = dcPrint;
            dcPrint.TestMethodExtension.CustomPostProcessResults(this.mockResult.Object);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            //  Cannot access ExitPort anymore, use a print statement to verify the correct action was taken.
            // TODO: should replace this with a full test of the Execute method where the return value can be tested.
            // Assert.AreEqual(dcPrint.ExitPort, 0);
            consoleMock.Verify(o => o.PrintDebug("CustomPostProcessResults: GetAllPinGroupsDcResults returned null or .Count==0. Setting exit port to 0."), Times.Once);
        }

        /// <summary>
        /// Data data test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamInput_Pass()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            List<double> val = new List<double> { 0.0011, 0.0022 };
            double valAvg = (val[0] + val[1]) / 2;
            string pinName = "Pin1";

            this.SetupDatalog(new List<Tuple<string, double>> { new Tuple<string, double>(pinName, valAvg) });
            this.mockResult = new Mock<IDcResults>(MockBehavior.Strict);
            this.mockDCGroup = new Mock<List<IPinGroupDcResults>>(MockBehavior.Strict);
            this.mockDCGroupResults = new Mock<IPinGroupDcResults>(MockBehavior.Strict);
            this.mockDCResults = new Mock<List<IPinDcResults>>(MockBehavior.Strict);
            this.result = new Mock<IPinDcResults>(MockBehavior.Strict);
            this.result.Setup(x => x.GetPinDcResults()).Returns(() => val); // mock 1 pin
            this.result.Setup(x => x.GetPinName()).Returns(() => pinName);

            this.mockDCGroupResults.Setup(y => y.GetAllPinsDcResults()).Returns(() => new List<IPinDcResults> { this.result.Object });
            this.mockResult.Setup(x => x.GetAllPinGroupsDcResults()).Returns(() => new List<IPinGroupDcResults> { this.mockDCGroupResults.Object });

            // [2] Call the method under test.
            DcPrint dcPrint = new DcPrint();
            dcPrint.TestMethodExtension = dcPrint;
            this.MockAreAllDcResultsWithinLimitsHack(ref dcPrint);
            dcPrint.TestMethodExtension.CustomPostProcessResults(this.mockResult.Object);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            this.datalogServiceMock.Verify();
            this.mrsltFormatMock.VerifyAll();
        }

        /// <summary>
        /// Data data test.
        /// </summary>
        [TestMethod]
        public void Execute_ParamInput_Pass()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            List<double> val = new List<double> { 0.0011, 0.0022 };
            double valAvg = (val[0] + val[1]) / 2;
            string pinName = "Pin1";
            this.SetupDatalog(new List<Tuple<string, double>> { new Tuple<string, double>(pinName, valAvg) });

            var dcTestMock = new Mock<IDcTest>(MockBehavior.Strict);
            var dcServiceMock = new Mock<IDcService>(MockBehavior.Strict);
            dcServiceMock.Setup(o => o.GetLevelDcWithoutSmartTc(new List<string> { "Pin1" }, "SomeLevels", new List<MeasurementType> { MeasurementType.CURRENT })).Returns(dcTestMock.Object);
            Prime.Services.DcService = dcServiceMock.Object;
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            Prime.Services.PinService = pinServiceMock.Object;
            var testConditionService = new Mock<ITestConditionService>(MockBehavior.Strict);
            Prime.Services.TestConditionService = testConditionService.Object;

            DcPrint dcPrint = new DcPrint()
            {
                MeasurementTypes = "Current",
                Pins = "Pin1",
                LevelsTc = "SomeLevels",
                DatalogLevel = PrimeDcTestMethod.DatalogLevels.All,
                HighLimits = "0.1A",
                LowLimits = "0.0A",
            };

            this.mockResult = new Mock<IDcResults>(MockBehavior.Strict);
            this.mockDCGroup = new Mock<List<IPinGroupDcResults>>(MockBehavior.Strict);
            this.mockDCGroupResults = new Mock<IPinGroupDcResults>(MockBehavior.Strict);
            this.mockDCResults = new Mock<List<IPinDcResults>>(MockBehavior.Strict);
            this.result = new Mock<IPinDcResults>(MockBehavior.Strict);
            dcTestMock.Setup(o => o.Execute()).Returns(this.mockResult.Object);

            this.result.Setup(x => x.GetPinDcResults()).Returns(() => val); // mock 1 pin
            this.result.Setup(x => x.GetPinName()).Returns(() => pinName);

            this.mockDCGroupResults.Setup(y => y.GetAllPinsDcResults()).Returns(() => new List<IPinDcResults> { this.result.Object });
            this.mockDCGroupResults.Setup(o => o.GetPinGroupName()).Returns(string.Empty);

            this.mockResult.Setup(x => x.GetAllPinGroupsDcResults()).Returns(() => new List<IPinGroupDcResults> { this.mockDCGroupResults.Object });
            /* this.mockResult.Setup(o => o.PrintToDatalog(DatalogLevel.ALL, new Dictionary<string, Tuple<double, double>> { { "Pin1", new Tuple<double, double>(0.0, 0.1) } })); */

            // [2] Call the method under test.
            dcPrint.TestMethodExtension = dcPrint;
            dcPrint.Verify();
            dcPrint.CustomVerify();
            var port = dcPrint.Execute();
            Assert.AreEqual(1, port);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            this.datalogServiceMock.Verify();
            this.mrsltFormatMock.VerifyAll();
        }

        /// <summary>
        /// Data data test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamNullInput_Pass()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            List<double> val = new List<double> { };
            string pinName = "Pin1";
            this.SetupDatalog(new List<Tuple<string, double>> { new Tuple<string, double>(pinName, -9999) });
            DcPrint dcPrint = new DcPrint();

            this.mockResult = new Mock<IDcResults>(MockBehavior.Strict);
            this.mockDCGroup = new Mock<List<IPinGroupDcResults>>(MockBehavior.Strict);
            this.mockDCGroupResults = new Mock<IPinGroupDcResults>(MockBehavior.Strict);
            this.mockDCResults = new Mock<List<IPinDcResults>>(MockBehavior.Strict);
            this.result = new Mock<IPinDcResults>(MockBehavior.Strict);

            this.result.Setup(x => x.GetPinDcResults()).Returns(() => null); // mock 1 pin
            this.result.Setup(x => x.GetPinName()).Returns(() => pinName);

            this.mockDCGroupResults.Setup(y => y.GetAllPinsDcResults()).Returns(() => new List<IPinDcResults> { this.result.Object });

            this.mockResult.Setup(x => x.GetAllPinGroupsDcResults()).Returns(() => new List<IPinGroupDcResults> { this.mockDCGroupResults.Object });

            // [2] Call the method under test.
            dcPrint.TestMethodExtension = dcPrint;
            dcPrint.TestMethodExtension.CustomPostProcessResults(this.mockResult.Object);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            this.datalogServiceMock.Verify();
            this.mrsltFormatMock.VerifyAll();
        }

        /// <summary>
        /// Data data test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamNoInput_Pass()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            List<double> val = new List<double> { };
            string pinName = "Pin1";
            this.SetupDatalog(new List<Tuple<string, double>> { new Tuple<string, double>(pinName, -9999) });
            DcPrint dcPrint = new DcPrint();

            this.mockResult = new Mock<IDcResults>(MockBehavior.Strict);
            this.mockDCGroup = new Mock<List<IPinGroupDcResults>>(MockBehavior.Strict);
            this.mockDCGroupResults = new Mock<IPinGroupDcResults>(MockBehavior.Strict);
            this.mockDCResults = new Mock<List<IPinDcResults>>(MockBehavior.Strict);
            this.result = new Mock<IPinDcResults>(MockBehavior.Strict);

            this.result.Setup(x => x.GetPinDcResults()).Returns(() => val); // mock 1 pin
            this.result.Setup(x => x.GetPinName()).Returns(() => pinName);

            this.mockDCGroupResults.Setup(y => y.GetAllPinsDcResults()).Returns(() => new List<IPinDcResults> { this.result.Object });

            this.mockResult.Setup(x => x.GetAllPinGroupsDcResults()).Returns(() => new List<IPinGroupDcResults> { this.mockDCGroupResults.Object });

            // [2] Call the method under test.
            dcPrint.TestMethodExtension = dcPrint;
            this.MockAreAllDcResultsWithinLimitsHack(ref dcPrint);
            dcPrint.TestMethodExtension.CustomPostProcessResults(this.mockResult.Object);

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            this.datalogServiceMock.Verify();
            this.mrsltFormatMock.VerifyAll();
        }

        /// <summary>
        /// Data data test.
        /// </summary>
        [TestMethod]
        public void Verify_Catch_Fail()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            DcPrint dcPrint = new DcPrint();

            this.mockResult = new Mock<IDcResults>(MockBehavior.Strict);
            this.mockResult.Setup(x => x.GetAllPinGroupsDcResults()).Throws(new Prime.Base.Exceptions.TestMethodException("Error"));

            // [2] Call the method under test.
            dcPrint.TestMethodExtension = dcPrint;
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => dcPrint.TestMethodExtension.CustomPostProcessResults(this.mockResult.Object));
            Assert.AreEqual("Error", ex.Message);
        }

        private void SetupDatalog(List<Tuple<string, double>> pinResults)
        {
            this.datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            this.mrsltFormatMock = new Mock<IMrsltFormat>(MockBehavior.Strict);
            this.mrsltFormatMock.Setup(o => o.SetPrecision(8));
            foreach (var pair in pinResults)
            {
                this.mrsltFormatMock.Setup(o => o.SetTnamePostfix("_" + pair.Item1));
                this.mrsltFormatMock.Setup(o => o.SetData(pair.Item2));
            }

            this.datalogServiceMock.Setup(o => o.GetItuffMrsltWriter()).Returns(this.mrsltFormatMock.Object);
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.mrsltFormatMock.Object));
            Prime.Services.DatalogService = this.datalogServiceMock.Object;
        }

        private void MockAreAllDcResultsWithinLimitsHack(ref DcPrint underTest)
        {
            underTest.Pins = "Pin1";
            underTest.SamplingCount = "1";
            underTest.MeasurementTypes = "Current";
            underTest.CustomVerify();
        }
    }
}
