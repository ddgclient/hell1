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

namespace Prime.TestMethods.VminSearch.UnitTest
{
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.FunctionalService;
    using Prime.TestMethods.VminSearch;

    /// <summary>
    /// UnitTest class.
    /// </summary>
    [TestClass]
    public class SearchPointTestUnitTest
    {
        /// <summary>
        /// Normal Execute Run.
        /// </summary>
        [TestMethod]
        public void Execute_PassPlistExecution_ReturnTrue()
        {
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(func => func.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(test => test.Execute()).Returns(true);
            Services.FunctionalService = funcServiceMock.Object;
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(service => service.PrintDebug(It.IsAny<string>()));
            Services.ConsoleService = consoleServiceMock.Object;

            var searchExtensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            searchExtensionsMock.Setup(point => point.GetFunctionalTest("plistName", "level", "timing", string.Empty)).Returns(funcTestMock.Object);
            searchExtensionsMock.Setup(extensions => extensions.ApplyMask(It.IsAny<BitArray>(), funcTestMock.Object));
            searchExtensionsMock.Setup(point => point.IsSinglePointMode()).Returns(false);
            searchExtensionsMock.Setup(point => point.IsCheckOfResultBitsEnabled()).Returns(true);

            // Test setup
            var plistExecutionParameters = new PrimeVminSearchTestMethod.PlistExecutionParameters()
            {
                Patlist = "plistName",
                LevelsTc = "level",
                TimingsTc = "timing",
                PrePlist = string.Empty,
            };

            var pointTest = new SearchPointTest(plistExecutionParameters, 0.1, new List<string> { "A", "B" }, searchExtensionsMock.Object, new List<string>(), consoleServiceMock.Object);

            // Test
            Assert.IsTrue(pointTest.Execute());
            funcServiceMock.VerifyAll();
            funcTestMock.VerifyAll();
            searchExtensionsMock.VerifyAll();
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// No need to process results because plist passes.
        /// </summary>
        [TestMethod]
        public void ProcessResults_ValidResultBits_ReturnTrue()
        {
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(func => func.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(new List<IFailureData>());
            Services.FunctionalService = funcServiceMock.Object;

            var searchExtensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            searchExtensionsMock.Setup(point => point.GetFunctionalTest("plistName", "level", "timing", string.Empty)).Returns(funcTestMock.Object);
            searchExtensionsMock.Setup(extensions => extensions.ProcessPlistResults(true, funcTestMock.Object)).Returns(new BitArray(2, false));
            searchExtensionsMock.Setup(point => point.IsSinglePointMode()).Returns(false);
            searchExtensionsMock.Setup(point => point.IsCheckOfResultBitsEnabled()).Returns(true);

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(service => service.PrintDebug(It.IsAny<string>()));

            // Test setup
            var plistExecutionParameters = new PrimeVminSearchTestMethod.PlistExecutionParameters()
            {
                Patlist = "plistName",
                LevelsTc = "level",
                TimingsTc = "timing",
                PrePlist = string.Empty,
            };

            var pointTest = new SearchPointTest(plistExecutionParameters, 0.1, new List<string> { "A", "B" }, searchExtensionsMock.Object, new List<string>(), consoleServiceMock.Object);

            // Test
            Assert.IsTrue(pointTest.ProcessResults(true));
            funcServiceMock.VerifyAll();
            funcTestMock.VerifyAll();
            searchExtensionsMock.VerifyAll();
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// plist Results number of bits does not match the expected number of targets.
        /// </summary>
        [TestMethod]
        public void ProcessResults_ResultBitsDoesNotMatchExpectedTargetCount_ReturnFalse()
        {
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(func => func.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(new List<IFailureData>());
            Services.FunctionalService = funcServiceMock.Object;

            var searchExtensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            searchExtensionsMock.Setup(point => point.GetFunctionalTest("plistName", "level", "timing", string.Empty)).Returns(funcTestMock.Object);
            searchExtensionsMock.Setup(extensions => extensions.ProcessPlistResults(false, funcTestMock.Object)).Returns(new BitArray(1, true));
            searchExtensionsMock.Setup(point => point.IsSinglePointMode()).Returns(false);
            searchExtensionsMock.Setup(point => point.IsCheckOfResultBitsEnabled()).Returns(true);

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(service => service.PrintDebug(It.IsAny<string>()));
            Services.ConsoleService = consoleServiceMock.Object;

            // Test setup
            var plistExecutionParameters = new PrimeVminSearchTestMethod.PlistExecutionParameters()
            {
                Patlist = "plistName",
                LevelsTc = "level",
                TimingsTc = "timing",
                PrePlist = string.Empty,
            };

            var pointTest = new SearchPointTest(plistExecutionParameters, 0.1, new List<string> { "A", "B" }, searchExtensionsMock.Object, new List<string>(), consoleServiceMock.Object);

            // Test
            Assert.IsFalse(pointTest.ProcessResults(false));
            funcServiceMock.VerifyAll();
            funcTestMock.VerifyAll();
            searchExtensionsMock.VerifyAll();
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// End Voltages Does Not Match Expected Count.
        /// </summary>
        [TestMethod]
        public void Reset_StartVoltageHigherThanEndVoltage_ReturnFalse()
        {
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var funcTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcTestMock.Setup(func => func.EnableStartPatternOnFirstFail());
            funcTestMock.Setup(func => func.GetPerCycleFailures()).Returns(new List<IFailureData>());
            funcTestMock.Setup(test => test.Reset());
            Services.FunctionalService = funcServiceMock.Object;
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(service => service.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Services.ConsoleService = consoleServiceMock.Object;

            var searchExtensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            searchExtensionsMock.Setup(point => point.GetFunctionalTest("plistName", "level", "timing", string.Empty)).Returns(funcTestMock.Object);
            searchExtensionsMock.Setup(point => point.GetStartVoltageValues(new List<string>() { "0.5", "1.2" })).Returns(new List<double>() { 0.5, 1.2 });
            searchExtensionsMock.Setup(extensions => extensions.GetEndVoltageLimitValues(new List<string>() { "1.0", "1.0" })).Returns(new List<double>() { 1.0, 1.0 });
            searchExtensionsMock.Setup(point => point.IsSinglePointMode()).Returns(false);
            searchExtensionsMock.Setup(point => point.IsCheckOfResultBitsEnabled()).Returns(true);

            // Test setup
            var plistExecutionParameters = new PrimeVminSearchTestMethod.PlistExecutionParameters()
            {
                Patlist = "plistName",
                LevelsTc = "level",
                TimingsTc = "timing",
                PrePlist = string.Empty,
            };

            var pointTest = new SearchPointTest(plistExecutionParameters, 0.1, new List<string>() { "A", "B" }, searchExtensionsMock.Object, new List<string>(), consoleServiceMock.Object)
            {
                StartVoltageKeys = new List<string> { "0.5", "1.2" },
                EndVoltageLimitKeys = new List<string> { "1.0", "1.0" },
            };

            // Test
            Assert.IsFalse(pointTest.Reset());
            funcServiceMock.VerifyAll();
            searchExtensionsMock.VerifyAll();
            consoleServiceMock.VerifyAll();
        }
    }
}
