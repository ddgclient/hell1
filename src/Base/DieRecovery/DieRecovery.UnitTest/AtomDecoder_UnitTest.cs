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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DieRecoveryBase;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.TestProgramService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AtomDecoder_UnitTest
    {
        /// <summary>
        /// Setup mocks common to all tests.
        /// </summary>
        [TestInitialize]
        public void SetupCommonMocks()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Loose);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("LogLevel")).Returns("PRIME_DEBUG");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;
        }

        /// <summary>
        /// UnitTest for GetFailTrackerFromFailData method.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromFailureData_ArrayNonCoreFail_Pass()
        {
            var consoleOutput = new List<string>();
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                consoleOutput.Add(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var failData = new Mock<IFailureData>(MockBehavior.Strict);
            failData.Setup(o => o.GetPatternName()).Returns("atom_pattern");
            failData.Setup(o => o.GetPreviousLabel()).Returns("CORE_");
            failData.Setup(o => o.GetDomainName()).Returns("LEG");
            failData.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "STF0" });
            failData.Setup(o => o.GetParentPlistName()).Returns("some_array_list");

            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var result = decoder.GetFailTrackerFromFailData(failData.Object);
            Assert.AreEqual(true, result[0]);
            Assert.AreEqual(true, result[1]);
            Assert.AreEqual(true, result[2]);
            Assert.AreEqual(true, result[3]);
            CollectionAssert.AreEqual(new List<string> { "Non-core failure with label \"CORE_\". Setting fail state for all cores." }, consoleOutput);
        }

        /// <summary>
        /// UnitTest for GetFailTrackerFromFailData method.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromFailureData_DifferentPin_Pass()
        {
            var consoleOutput = new List<string>();
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                consoleOutput.Add(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var failData = new Mock<IFailureData>(MockBehavior.Strict);
            failData.Setup(o => o.GetPatternName()).Returns("atom_pattern");
            failData.Setup(o => o.GetPreviousLabel()).Returns("CORE_");
            failData.Setup(o => o.GetDomainName()).Returns("LEG");
            failData.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "STF1" });
            failData.Setup(o => o.GetParentPlistName()).Returns("some_array_list");

            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var result = decoder.GetFailTrackerFromFailData(failData.Object);
            Assert.AreEqual(false, result[0]);
            Assert.AreEqual(false, result[1]);
            Assert.AreEqual(false, result[2]);
            Assert.AreEqual(false, result[3]);
            CollectionAssert.AreEqual(new List<string>(), consoleOutput);
        }

        /// <summary>
        /// UnitTest for GetFailTrackerFromFailData method.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromFailureData_CoreFail_Pass()
        {
            var consoleOutput = new List<string>();
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                consoleOutput.Add(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var failData = new Mock<IFailureData>(MockBehavior.Strict);
            failData.Setup(o => o.GetPatternName()).Returns("atom_pattern");
            failData.Setup(o => o.GetPreviousLabel()).Returns("CORE2_FAIL");
            failData.Setup(o => o.GetDomainName()).Returns("LEG");
            failData.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "STF0" });
            failData.Setup(o => o.GetParentPlistName()).Returns("some_array_list");

            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var result = decoder.GetFailTrackerFromFailData(failData.Object);
            Assert.AreEqual(false, result[0]);
            Assert.AreEqual(false, result[1]);
            Assert.AreEqual(true, result[2]);
            Assert.AreEqual(false, result[3]);
            CollectionAssert.AreEqual(new List<string> { "Core failure with label \"CORE2_FAIL\". Setting fail state for core 2." }, consoleOutput);
        }

        /// <summary>
        /// UnitTest for GetFailTrackerFromFailData method.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromFailureData_ArrayCoreFailWithPrependLabel_Pass()
        {
            var consoleOutput = new List<string>();
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                consoleOutput.Add(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var failData = new Mock<IFailureData>(MockBehavior.Strict);
            failData.Setup(o => o.GetPatternName()).Returns("atom_pattern");
            failData.Setup(o => o.GetPreviousLabel()).Returns("CPU_FAB_ALL_CORE1_FAIL");
            failData.Setup(o => o.GetDomainName()).Returns("LEG");
            failData.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "STF0" });
            failData.Setup(o => o.GetParentPlistName()).Returns("some_func_list");

            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var result = decoder.GetFailTrackerFromFailData(failData.Object);
            Assert.AreEqual(false, result[0]);
            Assert.AreEqual(true, result[1]);
            Assert.AreEqual(false, result[2]);
            Assert.AreEqual(false, result[3]);
            CollectionAssert.AreEqual(new List<string> { "Core failure with label \"CPU_FAB_ALL_CORE1_FAIL\". Setting fail state for core 1." }, consoleOutput);
        }

        /// <summary>
        /// UnitTest for GetFailTrackerFromFailData method.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromFailureData_FuncCoreFailWithPrependLabel_Pass()
        {
            var consoleOutput = new List<string>();
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                consoleOutput.Add(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var failData = new Mock<IFailureData>(MockBehavior.Strict);
            failData.Setup(o => o.GetPatternName()).Returns("atom_pattern");
            failData.Setup(o => o.GetPreviousLabel()).Returns("CPU_FAB_ALL_C1_FAIL");
            failData.Setup(o => o.GetDomainName()).Returns("LEG");
            failData.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "STF0" });
            failData.Setup(o => o.GetParentPlistName()).Returns("some_array_list");

            var decoder = new AtomDecoder("AM0", "STF0", 0, "FUNC");
            var result = decoder.GetFailTrackerFromFailData(failData.Object);
            Assert.AreEqual(false, result[0]);
            Assert.AreEqual(true, result[1]);
            Assert.AreEqual(false, result[2]);
            Assert.AreEqual(false, result[3]);
            CollectionAssert.AreEqual(new List<string> { "Core failure with label \"CPU_FAB_ALL_C1_FAIL\". Setting fail state for core 1." }, consoleOutput);
        }

        /// <summary>
        /// UnitTest for GetFailTrackerFromPlistResults method.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_NoFail_Pass()
        {
            var failList = new List<IFailureData>();
            var functionalTest = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            functionalTest.Setup(o => o.GetPerCycleFailures()).Returns(failList);
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var result = decoder.GetFailTrackerFromPlistResults(functionalTest.Object);
            Assert.AreEqual(false, result[0]);
            Assert.AreEqual(false, result[1]);
            Assert.AreEqual(false, result[2]);
            Assert.AreEqual(false, result[3]);
        }

        /// <summary>
        /// UnitTest for GetFailTrackerFromPlistResults method.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_NotICaptureFailureTest_Exception()
        {
            var functionalTest = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);

            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            Assert.ThrowsException<System.ArgumentException>(() => decoder.GetFailTrackerFromPlistResults(functionalTest.Object));
        }

        /// <summary>
        /// UnitTest for GetFailTrackerFromPlistResults method.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_2CoreFailure_Pass()
        {
            var consoleOutput = new List<string>();
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                consoleOutput.Add(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var failData = new Mock<IFailureData>(MockBehavior.Strict);
            failData.Setup(o => o.GetPatternName()).Returns("atom_pattern");
            failData.Setup(o => o.GetPreviousLabel()).Returns("CORE1_0_1_1");
            failData.Setup(o => o.GetDomainName()).Returns("LEG");
            failData.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "STF0" });
            failData.Setup(o => o.GetParentPlistName()).Returns("some_array_list");

            var failData2 = new Mock<IFailureData>(MockBehavior.Strict);
            failData2.Setup(o => o.GetPatternName()).Returns("atom_pattern");
            failData2.Setup(o => o.GetPreviousLabel()).Returns("CORE3_0_1_1");
            failData2.Setup(o => o.GetDomainName()).Returns("LEG");
            failData2.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "STF0" });
            failData2.Setup(o => o.GetParentPlistName()).Returns("some_array_list");

            var failList = new List<IFailureData>();
            failList.Add(failData.Object);
            failList.Add(failData2.Object);

            var functionalTest = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            functionalTest.Setup(o => o.GetPerCycleFailures()).Returns(failList);

            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var result = decoder.GetFailTrackerFromPlistResults(functionalTest.Object);
            Assert.AreEqual(false, result[0]);
            Assert.AreEqual(true, result[1]);
            Assert.AreEqual(false, result[2]);
            Assert.AreEqual(true, result[3]);
        }

        /// <summary>
        /// UnitTest for GetFailTrackerFromPlistResults method.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_2CoreFailureReverse_Pass()
        {
            var consoleOutput = new List<string>();
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                consoleOutput.Add(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var failData = new Mock<IFailureData>(MockBehavior.Strict);
            failData.Setup(o => o.GetPatternName()).Returns("atom_pattern");
            failData.Setup(o => o.GetPreviousLabel()).Returns("CORE1_0_1_1");
            failData.Setup(o => o.GetDomainName()).Returns("LEG");
            failData.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "STF0" });
            failData.Setup(o => o.GetParentPlistName()).Returns("some_array_list");

            var failData2 = new Mock<IFailureData>(MockBehavior.Strict);
            failData2.Setup(o => o.GetPatternName()).Returns("atom_pattern");
            failData2.Setup(o => o.GetPreviousLabel()).Returns("CORE3_0_1_1");
            failData2.Setup(o => o.GetDomainName()).Returns("LEG");
            failData2.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "STF0" });
            failData2.Setup(o => o.GetParentPlistName()).Returns("some_array_list");

            var failList = new List<IFailureData>();
            failList.Add(failData.Object);
            failList.Add(failData2.Object);

            var functionalTest = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            functionalTest.Setup(o => o.GetPerCycleFailures()).Returns(failList);

            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", null, 4, true);
            var result = decoder.GetFailTrackerFromPlistResults(functionalTest.Object);
            Assert.AreEqual(true, result[0]);
            Assert.AreEqual(false, result[1]);
            Assert.AreEqual(true, result[2]);
            Assert.AreEqual(false, result[3]);
        }

        /// <summary>
        /// UnitTest for GetFailTrackerFromPlistResults method.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_AllFail_Pass()
        {
            var consoleOutput = new List<string>();
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                consoleOutput.Add(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var failData = new Mock<IFailureData>(MockBehavior.Strict);
            failData.Setup(o => o.GetPatternName()).Returns("atom_pattern");
            failData.Setup(o => o.GetPreviousLabel()).Returns("BUSY");
            failData.Setup(o => o.GetDomainName()).Returns("LEG");
            failData.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "STF0" });
            failData.Setup(o => o.GetParentPlistName()).Returns("some_array_list");

            var failData2 = new Mock<IFailureData>(MockBehavior.Strict);
            failData2.Setup(o => o.GetPatternName()).Returns("atom_pattern");
            failData2.Setup(o => o.GetPreviousLabel()).Returns("CORE3_0_1_1");
            failData2.Setup(o => o.GetDomainName()).Returns("LEG");
            failData2.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "STF0" });
            failData2.Setup(o => o.GetParentPlistName()).Returns("some_array_list");

            var failList = new List<IFailureData>();
            failList.Add(failData.Object);
            failList.Add(failData2.Object);

            var functionalTest = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            functionalTest.Setup(o => o.GetPerCycleFailures()).Returns(failList);

            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var result = decoder.GetFailTrackerFromPlistResults(functionalTest.Object);
            Assert.AreEqual(true, result[0]);
            Assert.AreEqual(true, result[1]);
            Assert.AreEqual(true, result[2]);
            Assert.AreEqual(true, result[3]);
            CollectionAssert.AreEqual(new List<string> { "Non-core failure with label \"BUSY\". Setting fail state for all cores." }, consoleOutput);
        }

        /// <summary>
        /// UnitTest for MaskPlistFromTracker method.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_AllFailTracker_Pass()
        {
            var consoleOutput = new List<string>();
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                consoleOutput.Add(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var patConfigHandleMockM0C0M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C1M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C2M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C3M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_m0_c0_mask")).Returns(patConfigHandleMockM0C0M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_m0_c1_mask")).Returns(patConfigHandleMockM0C1M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_m0_c2_mask")).Returns(patConfigHandleMockM0C2M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_m0_c3_mask")).Returns(patConfigHandleMockM0C3M.Object);
            var patConfigList = new List<IPatConfigHandle>();
            patConfigList.Add(patConfigHandleMockM0C0M.Object);
            patConfigList.Add(patConfigHandleMockM0C1M.Object);
            patConfigList.Add(patConfigHandleMockM0C2M.Object);
            patConfigList.Add(patConfigHandleMockM0C3M.Object);
            patConfigServiceMock.Setup(o => o.Apply(patConfigList));
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var functionalTestForSearch = new Mock<IFunctionalTest>(MockBehavior.Strict).Object;
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            CollectionAssert.AreEqual(new List<string> { "STF0" }, decoder.MaskPlistFromTracker(new BitArray(4, true), ref functionalTestForSearch));
            patConfigServiceMock.Verify(o => o.Apply(It.IsAny<List<IPatConfigHandle>>()), Times.Never);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c0_mask"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c1_mask"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c2_mask"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c3_mask"), Times.Once);
        }

        /// <summary>
        /// UnitTest for MaskPlistFromTracker method.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_ArrayNotAllFailed_Pass()
        {
            var consoleOutput = new List<string>();
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                consoleOutput.Add(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var patConfigHandleMockM0C0M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C1M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C2M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C3R = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_m0_c0_restore")).Returns(patConfigHandleMockM0C0M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_m0_c1_mask")).Returns(patConfigHandleMockM0C1M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_m0_c2_restore")).Returns(patConfigHandleMockM0C2M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_m0_c3_mask")).Returns(patConfigHandleMockM0C3R.Object);
            var patConfigList = new List<IPatConfigHandle>();
            patConfigList.Add(patConfigHandleMockM0C0M.Object);
            patConfigList.Add(patConfigHandleMockM0C1M.Object);
            patConfigList.Add(patConfigHandleMockM0C2M.Object);
            patConfigList.Add(patConfigHandleMockM0C3R.Object);
            patConfigServiceMock.Setup(o => o.Apply(patConfigList));
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var functionalTestForSearch = new Mock<IFunctionalTest>(MockBehavior.Strict).Object;
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", null, 4, true);
            var mask = new BitArray(4);
            mask[0] = true;
            mask[1] = false;
            mask[2] = true;
            mask[3] = false;
            CollectionAssert.AreEqual(new List<string>(), decoder.MaskPlistFromTracker(mask, ref functionalTestForSearch));
            patConfigServiceMock.Verify(o => o.Apply(It.IsAny<List<IPatConfigHandle>>()), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c0_restore"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c1_mask"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c2_restore"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c3_mask"), Times.Once);
        }

        /// <summary>
        /// UnitTest for MaskPlistFromTracker method.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_ArrayNotAllFailed_PatternConfigUniqDefined_Pass()
        {
            var consoleOutput = new List<string>();
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                consoleOutput.Add(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var patConfigHandleMockM0C0M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C1M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C2M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C3R = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_uniq_m0_c0_mask")).Returns(patConfigHandleMockM0C0M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_uniq_m0_c1_mask")).Returns(patConfigHandleMockM0C1M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_uniq_m0_c2_mask")).Returns(patConfigHandleMockM0C2M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_uniq_m0_c3_restore")).Returns(patConfigHandleMockM0C3R.Object);
            var patConfigList = new List<IPatConfigHandle>();
            patConfigList.Add(patConfigHandleMockM0C0M.Object);
            patConfigList.Add(patConfigHandleMockM0C1M.Object);
            patConfigList.Add(patConfigHandleMockM0C2M.Object);
            patConfigList.Add(patConfigHandleMockM0C3R.Object);
            patConfigServiceMock.Setup(o => o.Apply(patConfigList));
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var functionalTestForSearch = new Mock<IFunctionalTest>(MockBehavior.Strict).Object;
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "uniq");
            var mask = new BitArray(4);
            mask[0] = true;
            mask[1] = true;
            mask[2] = true;
            mask[3] = false;
            CollectionAssert.AreEqual(new List<string>(), decoder.MaskPlistFromTracker(mask, ref functionalTestForSearch));
            patConfigServiceMock.Verify(o => o.Apply(It.IsAny<List<IPatConfigHandle>>()), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_uniq_m0_c0_mask"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_uniq_m0_c1_mask"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_uniq_m0_c2_mask"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_uniq_m0_c3_restore"), Times.Once);
        }

        /// <summary>
        /// UnitTest for MaskPlistFromTracker method.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_FuncNotAllFailed_Pass()
        {
            var consoleOutput = new List<string>();
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                consoleOutput.Add(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var patConfigHandleMockM0C0M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C1M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C2M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C3R = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_func_m0_c0_mask")).Returns(patConfigHandleMockM0C0M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_func_m0_c1_mask")).Returns(patConfigHandleMockM0C1M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_func_m0_c2_mask")).Returns(patConfigHandleMockM0C2M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_func_m0_c3_restore")).Returns(patConfigHandleMockM0C3R.Object);
            var patConfigList = new List<IPatConfigHandle>();
            patConfigList.Add(patConfigHandleMockM0C0M.Object);
            patConfigList.Add(patConfigHandleMockM0C1M.Object);
            patConfigList.Add(patConfigHandleMockM0C2M.Object);
            patConfigList.Add(patConfigHandleMockM0C3R.Object);
            patConfigServiceMock.Setup(o => o.Apply(patConfigList));
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var functionalTestForSearch = new Mock<IFunctionalTest>(MockBehavior.Strict).Object;
            var decoder = new AtomDecoder("AM0", "STF0", 0, "FUNC");
            var mask = new BitArray(4);
            mask[0] = true;
            mask[1] = true;
            mask[2] = true;
            mask[3] = false;
            CollectionAssert.AreEqual(new List<string>(), decoder.MaskPlistFromTracker(mask, ref functionalTestForSearch));
            patConfigServiceMock.Verify(o => o.Apply(It.IsAny<List<IPatConfigHandle>>()), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_func_m0_c0_mask"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_func_m0_c1_mask"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_func_m0_c2_mask"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_func_m0_c3_restore"), Times.Once);
        }

        /// <summary>
        /// UnitTest for MaskPlistFromTracker method.
        /// </summary>
        [TestMethod]
        public void MaskPlistFromTracker_B2BExecution_Pass()
        {
            var consoleOutput = new List<string>();
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                consoleOutput.Add(s);
            });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var patConfigHandleMockM0C0M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C1R = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C2M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C3R = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigHandleMockM0C3M = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_m0_c0_mask")).Returns(patConfigHandleMockM0C0M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_m0_c1_restore")).Returns(patConfigHandleMockM0C1R.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_m0_c2_mask")).Returns(patConfigHandleMockM0C2M.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_m0_c3_restore")).Returns(patConfigHandleMockM0C3R.Object);
            patConfigServiceMock.Setup(o => o.GetPatConfigHandle("atom_array_m0_c3_mask")).Returns(patConfigHandleMockM0C3M.Object);
            var patConfigList1 = new List<IPatConfigHandle>();
            patConfigList1.Add(patConfigHandleMockM0C0M.Object);
            patConfigList1.Add(patConfigHandleMockM0C1R.Object);
            patConfigList1.Add(patConfigHandleMockM0C2M.Object);
            patConfigList1.Add(patConfigHandleMockM0C3R.Object);
            patConfigServiceMock.Setup(o => o.Apply(patConfigList1));
            var patConfigList2 = new List<IPatConfigHandle>();
            patConfigList2.Add(patConfigHandleMockM0C0M.Object);
            patConfigList2.Add(patConfigHandleMockM0C1R.Object);
            patConfigList2.Add(patConfigHandleMockM0C2M.Object);
            patConfigList2.Add(patConfigHandleMockM0C3M.Object);
            patConfigServiceMock.Setup(o => o.Apply(patConfigList2));
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var functionalTestForSearch = new Mock<IFunctionalTest>(MockBehavior.Strict).Object;
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var mask = new BitArray(4);
            mask[0] = true;
            mask[1] = false;
            mask[2] = true;
            mask[3] = false;
            CollectionAssert.AreEqual(new List<string>(), decoder.MaskPlistFromTracker(mask, ref functionalTestForSearch));
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c0_mask"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c1_restore"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c2_mask"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c3_restore"), Times.Once);
            patConfigServiceMock.Verify(o => o.Apply(It.IsAny<List<IPatConfigHandle>>()), Times.Once);
            mask[3] = true;
            CollectionAssert.AreEqual(new List<string>(), decoder.MaskPlistFromTracker(mask, ref functionalTestForSearch));
            patConfigServiceMock.Verify(o => o.Apply(It.IsAny<List<IPatConfigHandle>>()), Times.Exactly(2));
            /* Added caching of the handles, so the apply still happens twice, but the config handle is only created once. */
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c0_mask"), Times.Once); // Was Times.Exactly(2)
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c1_restore"), Times.Once); // Was Times.Exactly(2)
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c2_mask"), Times.Once); // Was Times.Exactly(2)
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c3_restore"), Times.Once);
            patConfigServiceMock.Verify(o => o.GetPatConfigHandle("atom_array_m0_c3_mask"), Times.Once);
        }

        /// <summary>
        /// UnitTest for Serialize and Deserialize methods with PatternModifyUniq not defined.
        /// </summary>
        [TestMethod]
        public void SerializeAndDeserialize_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "array", "uniquify");
            string serializedDecoder = JsonConvert.SerializeObject(decoder);
            Assert.AreEqual("{\"Pin\":\"STF0\",\"Module\":0,\"Content\":\"ARRAY\",\"PatternModifyUniq\":\"uniquify\",\"Reverse\":false,\"Name\":\"AM0\",\"PatternModify\":null,\"Size\":4,\"SharedStorageResults\":null}", serializedDecoder);
            var decoder2 = JsonConvert.DeserializeObject<AtomDecoder>("{\"Pin\":\"STF0\",\"Module\":0,\"Content\":\"ARRAY\",\"Name\":\"AM0\",\"PatternModifyUniq\":\"uniquify\",\"PatternModify\":null,\"SharedStorageResults\":null,\"Size\":4,\"Reverse\":false}");
            Assert.AreEqual(decoder2, decoder);
        }

        /// <summary>
        /// UnitTest for Serialize and Deserialize methods with PatternModifyUniq not defined.
        /// </summary>
        [TestMethod]
        public void SerializeAndDeserialize_UndefinedFields_Pass()
        {
            var decoder = JsonConvert.DeserializeObject<AtomDecoder>("{\"Pin\":\"STF0\",\"Module\":0,\"Content\":\"ARRAY\",\"Name\":\"AM0\",\"PatternModify\":null,\"Size\":4}");
            var decoder2 = JsonConvert.DeserializeObject<AtomDecoder>("{\"Pin\":\"STF0\",\"Module\":0,\"Content\":\"ARRAY\",\"Name\":\"AM0\",\"PatternModifyUniq\":null,\"PatternModify\":null,\"Size\":4,\"Reverse\":false}");
            Assert.AreEqual(decoder2, decoder);
        }

        /// <summary>
        /// UnitTest for Equals method.
        /// </summary>
        [TestMethod]
        public void Equals_MiscompareName_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var decoder2 = new AtomDecoder("AM1", "STF0", 0, "ARRAY");
            Assert.AreNotEqual(decoder, decoder2);
        }

        /// <summary>
        /// UnitTest for Equals method.
        /// </summary>
        [TestMethod]
        public void Equals_MiscomparePin_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var decoder2 = new AtomDecoder("AM0", "STF1", 0, "ARRAY");
            Assert.AreNotEqual(decoder, decoder2);
        }

        /// <summary>
        /// UnitTest for Equals method.
        /// </summary>
        [TestMethod]
        public void Equals_MiscompareModule_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var decoder2 = new AtomDecoder("AM0", "STF0", 1, "ARRAY");
            Assert.AreNotEqual(decoder, decoder2);
        }

        /// <summary>
        /// UnitTest for Equals method.
        /// </summary>
        [TestMethod]
        public void Equals_MiscompareSize_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", null, 4);
            var decoder2 = new AtomDecoder("AM0", "STF0", 0, "ARRAY", null, 2);
            Assert.AreNotEqual(decoder, decoder2);
        }

        /// <summary>
        /// UnitTest for Equals method.
        /// </summary>
        [TestMethod]
        public void Equals_MiscomparePatternConfig_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var decoder2 = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            decoder2.IpPatternConfigure = "something_else";
            Assert.AreNotEqual(decoder, decoder2);
        }

        /// <summary>
        /// UnitTest for Equals method.
        /// </summary>
        [TestMethod]
        public void Equals_MiscompareContent_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var decoder2 = new AtomDecoder("AM0", "STF0", 0, "FUNC");
            Assert.AreNotEqual(decoder, decoder2);
        }

        /// <summary>
        /// UnitTest for Equals method.
        /// </summary>
        [TestMethod]
        public void Equals_Match_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var decoder2 = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            Assert.AreEqual(decoder, decoder2);
        }

        /// <summary>
        /// UnitTest for Equals method.
        /// </summary>
        [TestMethod]
        public void Equals_MiscomparePatternModifyUniq_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa");
            var decoder2 = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "dragon");
            Assert.AreNotEqual(decoder, decoder2);
        }

        /// <summary>
        /// UnitTest for Equals method.
        /// </summary>
        [TestMethod]
        public void Equals_MiscompareReverse_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa", 4, true);
            var decoder2 = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa", 4, false);
            Assert.AreNotEqual(decoder, decoder2);
        }

        /// <summary>
        /// UnitTest for Equals method.
        /// </summary>
        [TestMethod]
        public void Equals_WrongTypeObject_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var decoder2 = new PinMapDecoderBase();
            Assert.AreNotEqual(decoder, decoder2);
        }

        /// <summary>
        /// UnitTest for GetHashCode method.
        /// </summary>
        [TestMethod]
        public void GetHashCode_Match_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa");
            var decoder2 = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa");
            Assert.AreEqual(decoder.GetHashCode(), decoder2.GetHashCode());
        }

        /// <summary>
        /// UnitTest for GetHashCode method.
        /// </summary>
        [TestMethod]
        public void GetHashCode_MiscompareName_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa");
            var decoder2 = new AtomDecoder("AM1", "STF0", 0, "ARRAY", "ssa");
            Assert.AreNotEqual(decoder.GetHashCode(), decoder2.GetHashCode());
        }

        /// <summary>
        /// UnitTest for GetHashCode method.
        /// </summary>
        [TestMethod]
        public void GetHashCode_MiscompareModule_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa");
            var decoder2 = new AtomDecoder("AM0", "STF0", 1, "ARRAY", "ssa");
            Assert.AreNotEqual(decoder.GetHashCode(), decoder2.GetHashCode());
        }

        /// <summary>
        /// UnitTest for GetHashCode method.
        /// </summary>
        [TestMethod]
        public void GetHashCode_MiscomparePatternConfig_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa");
            var decoder2 = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa");
            decoder2.IpPatternConfigure = "something_else";
            Assert.AreNotEqual(decoder.GetHashCode(), decoder2.GetHashCode());
        }

        /// <summary>
        /// UnitTest for GetHashCode method.
        /// </summary>
        [TestMethod]
        public void GetHashCode_MiscompareContent_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa");
            var decoder2 = new AtomDecoder("AM0", "STF0", 0, "FUNC", "ssa");
            Assert.AreNotEqual(decoder.GetHashCode(), decoder2.GetHashCode());
        }

        /// <summary>
        /// UnitTest for GetHashCode method.
        /// </summary>
        [TestMethod]
        public void GetHashCode_MiscomparePin_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa");
            var decoder2 = new AtomDecoder("AM0", "STF1", 0, "ARRAY", "ssa");
            Assert.AreNotEqual(decoder.GetHashCode(), decoder2.GetHashCode());
        }

        /// <summary>
        /// UnitTest for GetHashCode method.
        /// </summary>
        [TestMethod]
        public void GetHashCode_MiscomparePatternModifyUniq_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa");
            var decoder2 = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "dragon");
            Assert.AreNotEqual(decoder.GetHashCode(), decoder2.GetHashCode());
        }

        /// <summary>
        /// UnitTest for GetHashCode method.
        /// </summary>
        [TestMethod]
        public void GetHashCode_MiscompareReverse_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa", 4, false);
            var decoder2 = new AtomDecoder("AM0", "STF0", 0, "ARRAY", "ssa", 4, true);
            Assert.AreNotEqual(decoder.GetHashCode(), decoder2.GetHashCode());
        }

        /// <summary>
        /// UnitTest for GetPatConfigForSliceControl method.
        /// </summary>
        [TestMethod]
        public void GetPatConfigForSliceControl_Default_Exception()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            Assert.ThrowsException<System.NotImplementedException>(() => decoder.GetPatConfigForSliceControl(new BitArray(4, false), "doesn't matter"));
        }

        /// <summary>
        /// UnitTest for Content set and get methods.
        /// </summary>
        [TestMethod]
        public void ContentSetGetArray_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "aRrAy");
            Assert.AreEqual(decoder.Content, "ARRAY");
        }

        /// <summary>
        /// UnitTest for Content set and get methods.
        /// </summary>
        [TestMethod]
        public void ContentSetGetFunc_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "fuNC");
            Assert.AreEqual(decoder.Content, "FUNC");
        }

        /// <summary>
        /// UnitTest for Content set method.
        /// </summary>
        [TestMethod]
        public void ContentSet_Exception()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            Assert.ThrowsException<System.ArgumentException>(() => decoder.Content = "scan", "scan is not a valid content type. Valid content types are 'ARRAY' and 'FUNC'.");
        }

        /// <summary>
        /// Refer to test method name..
        /// </summary>
        [TestMethod]
        public void GetDecoderType_Pass()
        {
            var decoder = new AtomDecoder("AM0", "STF0", 0, "ARRAY");
            var result = decoder.GetDecoderType();
            Assert.AreEqual("AtomDecoder", result);
        }
    }
}