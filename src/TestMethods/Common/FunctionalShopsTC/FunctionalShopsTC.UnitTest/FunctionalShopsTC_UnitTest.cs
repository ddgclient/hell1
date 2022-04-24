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

namespace FunctionalShopsTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.PinService;
    using Prime.PlistService;
    using Prime.TestConditionService;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class FunctionalShopsTC_UnitTest : FunctionalShopsTC
    {
        private Mock<IFunctionalService> functionalServiceMock;
        private Mock<ICaptureFailureTest> captureTestMock;
        private Mock<Prime.TestMethods.Functional.IFunctionalExtensions> captureFailuresExtensionsMock;
        private Mock<IPlistObject> plistObjectMock;
        private Mock<IPlistService> plistServiceMock;
        private Mock<IFileService> fileServiceMock;
        private Mock<IPinService> pinServiceMock;
        private Mock<ITestConditionService> testConditionServiceMock;

        /// <summary>
        /// Default initialization.
        /// </summary>
        [TestInitialize]
        public void Initialization()
        {
            this.Patlist = "SomePatlist";
            this.LevelsTc = "SomeLevels";
            this.TimingsTc = "SomeTimings";
            this.FailuresToCaptureTotal = uint.MaxValue;
            this.functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            this.captureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.captureFailuresExtensionsMock = new Mock<Prime.TestMethods.Functional.IFunctionalExtensions>(MockBehavior.Strict);
            this.plistObjectMock = new Mock<IPlistObject>(MockBehavior.Strict);
            this.plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            this.plistServiceMock.Setup(p => p.GetPlistObject(It.IsAny<string>())).Returns(this.plistObjectMock.Object);
            this.fileServiceMock = new Mock<IFileService>();
            this.pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(
                    o => o.PrintDebug(It.IsAny<string>())).
                Callback((string msg) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            Prime.Services.ConsoleService = consoleServiceMock.Object;
            Prime.Services.FunctionalService = this.functionalServiceMock.Object;
            Prime.Services.PlistService = this.plistServiceMock.Object;
            Prime.Services.FileService = this.fileServiceMock.Object;
            Prime.Services.PinService = this.pinServiceMock.Object;

            this.fileServiceMock.Setup(o => o.FileExists(Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json"))).Returns(true);
            this.fileServiceMock.Setup(o => o.GetFile(Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json"))).Returns(Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json"));

            this.fileServiceMock.Setup(o => o.FileExists(Path.Combine(Directory.GetCurrentDirectory(), "schema.json"))).Returns(true);
            this.fileServiceMock.Setup(o => o.GetFile(Path.Combine(Directory.GetCurrentDirectory(), "schema.json"))).Returns(Path.Combine(Directory.GetCurrentDirectory(), "schema.json"));

            // this.captureTestMock.Setup(funcTest => funcTest.ApplyTestConditions());
            // this.captureTestMock.Setup(func => func.Execute()).Returns(true);
            this.captureTestMock.Setup(c => c.GetFailingPinNames()).Returns(new List<string> { "TDO" }).Verifiable();

            // this.captureTestMock.Setup(c => c.DatalogFailure(It.IsAny<uint>()));
            this.captureTestMock.Setup(c => c.DatalogFailure(It.IsAny<uint>())).Verifiable();

            this.functionalServiceMock.Setup(f => f.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.FailuresToCaptureTotal, this.PrePlist)).Returns(this.captureTestMock.Object);
            this.captureFailuresExtensionsMock.Setup(f => f.GetDynamicPinMask()).Returns(new List<string>()).Verifiable();

            this.TestMethodExtension = this.captureFailuresExtensionsMock.Object;
            Prime.Services.FunctionalService = this.functionalServiceMock.Object;

            this.testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            Prime.Services.TestConditionService = this.testConditionServiceMock.Object;
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Verify_GetFileName_Exception()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.fileServiceMock.Setup(o => o.FileExists(string.Empty)).Returns(false);

            // FunctionalShopsTC underTest = new FunctionalShopsTC
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = string.Empty;
            this.SchemaFile = string.Empty;

            this.GetFileName(this.PinConfigFile);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_GetFileName_PASS()
        {
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");

            this.GetFileName(this.PinConfigFile);
            this.GetFileName(this.SchemaFile);

            // Checks wether mocks are called
            this.fileServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_IsValidPinConfig_PASS()
        {
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");

            var pinConfigFile = this.GetFileName(this.PinConfigFile);
            var schemaFile = this.GetFileName(this.SchemaFile);

            // [2] Call the method under test.
            Assert.IsTrue(this.IsValidPinConfig(pinConfigFile, schemaFile));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        public void Verify_IsValidPinConfig_FAIL()
        {
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfigBad.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");

            this.fileServiceMock.Setup(o => o.FileExists(Path.Combine(Directory.GetCurrentDirectory(), "PinConfigBad.json"))).Returns(true);
            this.fileServiceMock.Setup(o => o.GetFile(Path.Combine(Directory.GetCurrentDirectory(), "PinConfigBad.json"))).Returns(Path.Combine(Directory.GetCurrentDirectory(), "PinConfigBad.json"));
            var pinConfigFile = this.GetFileName(this.PinConfigFile);
            var schemaFile = this.GetFileName(this.SchemaFile);

            // [2] Call the method under test.
            Assert.IsFalse(this.IsValidPinConfig(pinConfigFile, schemaFile));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ReadPinConfig_PASS()
        {
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");

            // [2] Call the method under test.
            this.ReadPinConfig();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        public void Verify_ReadPinConfig_FAIL()
        {
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfigBad.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");

            this.fileServiceMock.Setup(o => o.FileExists(Path.Combine(Directory.GetCurrentDirectory(), "PinConfigBad.json"))).Returns(true);
            this.fileServiceMock.Setup(o => o.GetFile(Path.Combine(Directory.GetCurrentDirectory(), "PinConfigBad.json"))).Returns(Path.Combine(Directory.GetCurrentDirectory(), "PinConfigBad.json"));

            // [2] Call the method under test.
            Assert.ThrowsException<FileLoadException>(this.ReadPinConfig);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_ArePinsValid_Exception()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(false);

            this.ReadPinConfig();
            this.ArePinsValid();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ArePinsValid_PASS()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);

            this.ReadPinConfig();
            this.ArePinsValid();

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_Verify_PASS()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
            this.functionalServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        public void Verify_Verify_FAIL()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfigBad.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;

            this.fileServiceMock.Setup(o => o.FileExists(Path.Combine(Directory.GetCurrentDirectory(), "PinConfigBad.json"))).Returns(true);
            this.fileServiceMock.Setup(o => o.GetFile(Path.Combine(Directory.GetCurrentDirectory(), "PinConfigBad.json"))).Returns(Path.Combine(Directory.GetCurrentDirectory(), "PinConfigBad.json"));

            // [2] Call the method under test.
            Assert.ThrowsException<FileLoadException>(this.Verify);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ProcessFails_PASS()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            this.ProcessFails();

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
            this.functionalServiceMock.VerifyAll();

            // this.captureTestMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_UpdateVOXInitial_VOL()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);
            this.VOXOption = VOXOptions.VOL;

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            this.UpdateVOXInitial();

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_UpdateVOXInitial_VOH()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);
            this.VOXOption = VOXOptions.VOH;

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            this.UpdateVOXInitial();

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_UpdateVOXSubsequent()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            this.ProcessFails();

            // how do i check the result for vox?
            this.UpdateVOXSubsequent(0.2);

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
            this.functionalServiceMock.VerifyAll();

            // this.captureTestMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_UpdateVOX()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            this.ProcessFails();

            // how do i check the result for vox?
            this.UpdateVOX(0.2);

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
            this.functionalServiceMock.VerifyAll();

            // this.captureTestMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_SearchPoint_Fail()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);
            this.pinServiceMock.Setup(p => p.SetPinAttributeValues(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Verifiable();

            // plist execution fails
            this.captureTestMock.Setup(c => c.Execute()).Returns(false).Verifiable();

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            Assert.IsFalse(this.SearchPoint(0.1));

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
            this.functionalServiceMock.VerifyAll();

            // this.captureTestMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_SearchPoint_Pass()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);
            this.pinServiceMock.Setup(p => p.SetPinAttributeValues(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Verifiable();

            // plist execution passes
            this.captureTestMock.Setup(c => c.Execute()).Returns(true).Verifiable();

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            Assert.IsTrue(this.SearchPoint(0.1));

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
            this.functionalServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_SearchVOL()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);
            this.pinServiceMock.Setup(p => p.SetPinAttributeValues(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Verifiable();
            this.captureTestMock.Setup(c => c.Execute()).Returns(false).Verifiable();

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            this.SearchVOL();

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
            this.functionalServiceMock.VerifyAll();
            this.captureTestMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_SearchVOH()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);
            this.pinServiceMock.Setup(p => p.SetPinAttributeValues(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Verifiable();
            this.captureTestMock.Setup(c => c.Execute()).Returns(false).Verifiable();

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            this.SearchVOH();

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
            this.functionalServiceMock.VerifyAll();
            this.captureTestMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ApplyTestConditions()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);

            // this.pinServiceMock.Setup(p => p.SetPinAttributeValues(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Verifiable();
            this.captureTestMock.Setup(c => c.Execute()).Returns(false).Verifiable();
            this.captureTestMock.Setup(c => c.ApplyTestConditions()).Verifiable();

            this.captureTestMock.Setup(c => c.SetPinMask(It.IsAny<List<string>>()));

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            this.ApplyTestConditions();

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
            this.functionalServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_Characterization_VOL()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.VOXOption = VOXOptions.VOL;

            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);
            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "FixedDriveState")).Returns("Off");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "TermMode")).Returns("TermVRef");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "TermVRef")).Returns("385e-3");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VCH")).Returns("4");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VCL")).Returns("-1");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VIH")).Returns("770e-3");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VIL")).Returns("0");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VOX")).Returns("0.1");
            this.testConditionServiceMock.Setup(o => o.GetTestCondition(this.LevelsTc)).Returns(testConditionMock.Object);

            // this.pinServiceMock.Setup(p => p.SetPinAttributeValues(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Verifiable();
            this.captureTestMock.Setup(c => c.Execute()).Returns(false).Verifiable();
            this.captureTestMock.Setup(c => c.ApplyTestConditions()).Verifiable();
            var dict = new Dictionary<string, string> { { "VOX", "0.1" } };
            this.pinServiceMock.Setup(p => p.SetPinAttributeValues(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Verifiable();

            this.captureTestMock.Setup(c => c.SetPinMask(It.IsAny<List<string>>()));

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            this.Characterization();

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
            this.functionalServiceMock.VerifyAll();
            this.captureTestMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_Characterization_VOH()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.VOXOption = VOXOptions.VOH;

            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);
            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "FixedDriveState")).Returns("Off");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "TermMode")).Returns("TermVRef");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "TermVRef")).Returns("385e-3");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VCH")).Returns("4");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VCL")).Returns("-1");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VIH")).Returns("770e-3");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VIL")).Returns("0");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VOX")).Returns("0.1");
            this.testConditionServiceMock.Setup(o => o.GetTestCondition(this.LevelsTc)).Returns(testConditionMock.Object);

            // this.pinServiceMock.Setup(p => p.SetPinAttributeValues(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Verifiable();
            this.captureTestMock.Setup(c => c.Execute()).Returns(false).Verifiable();
            this.captureTestMock.Setup(c => c.ApplyTestConditions()).Verifiable();
            var dict = new Dictionary<string, string> { { "VOX", "0.1" } };
            this.pinServiceMock.Setup(p => p.SetPinAttributeValues(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Verifiable();

            this.captureTestMock.Setup(c => c.SetPinMask(It.IsAny<List<string>>()));

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            this.Characterization();

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
            this.functionalServiceMock.VerifyAll();
            this.captureTestMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_Production_Port0()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);

            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "FixedDriveState")).Returns("Off");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "TermMode")).Returns("TermVRef");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "TermVRef")).Returns("385e-3");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VCH")).Returns("4");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VCL")).Returns("-1");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VIH")).Returns("770e-3");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VIL")).Returns("0");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VOX")).Returns("0.1");
            this.testConditionServiceMock.Setup(o => o.GetTestCondition(this.LevelsTc)).Returns(testConditionMock.Object);

            // following call make Plist execution return false
            this.captureTestMock.Setup(c => c.Execute()).Returns(false).Verifiable();
            this.captureTestMock.Setup(c => c.ApplyTestConditions()).Verifiable();

            this.captureTestMock.Setup(c => c.SetPinMask(It.IsAny<List<string>>()));
            this.pinServiceMock.Setup(p => p.SetPinAttributeValues(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Verifiable();

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            Assert.AreEqual(0, this.Production());

            // Checks wether mocks are called
            this.functionalServiceMock.VerifyAll();
            this.captureTestMock.VerifyAll();
            this.captureFailuresExtensionsMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_Production_Port1()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;
            this.pinServiceMock.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);
            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "FixedDriveState")).Returns("Off");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "TermMode")).Returns("TermVRef");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "TermVRef")).Returns("385e-3");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VCH")).Returns("4");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VCL")).Returns("-1");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VIH")).Returns("770e-3");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VIL")).Returns("0");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VOX")).Returns("0.1");
            this.testConditionServiceMock.Setup(o => o.GetTestCondition(this.LevelsTc)).Returns(testConditionMock.Object);

            this.pinServiceMock.Setup(p => p.SetPinAttributeValues(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Verifiable();
            this.captureTestMock.Setup(c => c.Execute()).Returns(true).Verifiable();
            this.captureTestMock.Setup(c => c.ApplyTestConditions()).Verifiable();

            this.captureTestMock.Setup(c => c.SetPinMask(It.IsAny<List<string>>()));

            // [2] Call the method under test.
            this.Verify();
            this.CustomVerify();
            Assert.AreEqual(1, this.Production());

            // Checks wether mocks are called
            this.functionalServiceMock.VerifyAll();
            this.captureFailuresExtensionsMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ApplyVOX()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;

            this.pinServiceMock.Setup(p => p.SetPinAttributeValues(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Verifiable();

            this.ReadPinConfig();
            this.PrepareRequiredPinAttributes();
            this.ApplyVOX();

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_PrepareRequiredPinAttributes()
        {
            // [1] Setup the unit test scenario.
            // Need to mock the fileService as DDG.FileUtilities.GetFile uses it under the hood
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;

            this.ReadPinConfig();
            this.PrepareRequiredPinAttributes();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_StoreAndRestorePinAttributes()
        {
            // var maskPins = string.Empty; // no pins to mask
            this.LogLevel = TestMethodBase.PrimeLogLevel.TEST_METHOD;
            this.PinConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "PinConfig.json");
            this.SchemaFile = Path.Combine(Directory.GetCurrentDirectory(), "schema.json");
            this.TestMode = TestModes.Characterization;

            this.pinServiceMock.Setup(p => p.SetPinAttributeValues(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Verifiable();
            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "FixedDriveState")).Returns("Off");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "TermMode")).Returns("TermVRef");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "TermVRef")).Returns("385e-3");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VCH")).Returns("4");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VCL")).Returns("-1");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VIH")).Returns("770e-3");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VIL")).Returns("0");
            testConditionMock.Setup(o => o.GetPinAttributeValue(It.IsAny<string>(), "VOX")).Returns("0.1");
            this.testConditionServiceMock.Setup(o => o.GetTestCondition(this.LevelsTc)).Returns(testConditionMock.Object);

            this.ReadPinConfig();
            this.PrepareRequiredPinAttributes();
            this.StorePinAttributes();
            this.RestorePinAttributes();

            // Checks wether mocks are called
            this.pinServiceMock.VerifyAll();
        }
    }
}
