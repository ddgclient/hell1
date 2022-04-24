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

namespace ScanSSNHry.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.FunctionalService;
    using Prime.PinService;
    using Prime.PlistService;

    /// <summary>
    /// ScnHRY Unit Tests.
    /// </summary>
    [TestClass]
    public class ScanSSNHry_UnitTest
    {
        private Mock<IPlistService> plistServiceMock;
        private Mock<IPlistObject> plistMock;

        /// <summary>
        /// Setups the mocks.
        /// </summary>
        [TestInitialize]
        public void SetupMockingVerify()
        {
            // Mocking for IPlistService
            this.plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            this.plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            this.plistServiceMock.Setup(mock => mock.GetPlistObject("ValidPlist")).Returns(this.plistMock.Object);
            Prime.Services.PlistService = this.plistServiceMock.Object;
        }

        /// <summary>
        /// This tests verify when CreateCaptureFailureTest throws exception.
        /// </summary>
        [TestMethod]
        public void Verify_CreateCaptureFailureTest_ThrowsException()
        {
            var plistName = "InvalidPlist";
            var lvlName = "InvalidLevels";
            var timName = "InvalidTiming";
            var prePlist = "SomeCallback()";
            uint perPatFailCaptureCount = 99;
            Exception invalidLevelException = new Exception("CreateCaptureFailureTest() Fails.");

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func =>
                    func.CreateCaptureFailureTest(plistName, lvlName, timName, ulong.MaxValue, perPatFailCaptureCount, prePlist))
                .Throws(invalidLevelException);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            ScanSSNHry classUnderTest = new ScanSSNHry
            {
                Patlist = plistName,
                TimingsTc = timName,
                LevelsTc = lvlName,
                PrePlist = prePlist,
                PerPatFailCaptureCount = perPatFailCaptureCount,
                HRYInputFile = string.Empty,
                PinMappingInputFile = string.Empty,
            };

            Assert.ThrowsException<Exception>(() => classUnderTest.Verify());
            funcServiceMock.VerifyAll();
        }

        /// <summary>
        /// This tests verify when HryInput.FromJson throws exception.
        /// </summary>
        [TestMethod]
        public void Verify_HRYInputFromJson_ThrowsException()
        {
            var plistName = "ValidPlist";
            var lvlName = "ValidLevels";
            var timName = "ValidTiming";
            var prePlist = "SomeCallback()";
            uint perPatFailCaptureCount = 99;
            var hryInputFile = "InvalidFile";
            var pinMappingInputFile = "InvalidFile2";
            var jsonExceptionMessage = "InputsProcessed::ProcessData() Fails.";
            Newtonsoft.Json.JsonException exceptionFromJsonParsing = new Newtonsoft.Json.JsonException(jsonExceptionMessage);

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func =>
                    func.CreateCaptureFailureTest(plistName, lvlName, timName, ulong.MaxValue, perPatFailCaptureCount, prePlist))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(expression: file => file.FileExists(hryInputFile)).Returns(true);
            fileServiceMock.Setup(expression: file => file.FileExists(pinMappingInputFile)).Returns(true);
            Prime.Services.FileService = fileServiceMock.Object;

            var hryInputProcessedMock = new Mock<InputsProcessed>(MockBehavior.Strict);
            hryInputProcessedMock.Setup(func => func.ProcessData(hryInputFile, pinMappingInputFile)).Throws(exceptionFromJsonParsing);

            ScanSSNHry classUnderTest = new ScanSSNHry()
            {
                Patlist = plistName,
                TimingsTc = timName,
                LevelsTc = lvlName,
                PrePlist = prePlist,
                PerPatFailCaptureCount = perPatFailCaptureCount,
                HRYInputFile = hryInputFile,
                PinMappingInputFile = pinMappingInputFile,
            };

            var hryInputMockInject = typeof(ScanSSNHry).GetField("hryInputDataProcessed", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            hryInputMockInject.SetValue(classUnderTest, hryInputProcessedMock.Object);

            Assert.ThrowsException<TestMethodException>(() => classUnderTest.Verify());
            funcMock.VerifyAll();
            funcServiceMock.VerifyAll();
            hryInputProcessedMock.VerifyAll();
        }

        /// <summary>
        /// This tests verify when VeryMappingPatterns throws exception.
        /// </summary>
        [TestMethod]
        public void Verify_Patterns_are_Mapped_inJson_ThrowsException()
        {
            var plistName = "ValidPlist";
            var lvlName = "ValidLevels";
            var timName = "ValidTiming";
            var prePlist = "SomeCallback()";
            uint perPatFailCaptureCount = 99;
            var hryInputFile = "HRYFile";
            var pinMappingInputFile = "HRYFile2";
            var exceptionMessage = "VerifyExistsPatterns() fails.";
            TestMethodException verifyPatternException =
                new TestMethodException(exceptionMessage);

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func =>
                    func.CreateCaptureFailureTest(plistName, lvlName, timName, ulong.MaxValue, perPatFailCaptureCount, prePlist))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(expression: file => file.FileExists(hryInputFile)).Returns(true);
            fileServiceMock.Setup(expression: file => file.FileExists(pinMappingInputFile)).Returns(true);
            Prime.Services.FileService = fileServiceMock.Object;

            ScanSSNHry classUnderTest = new ScanSSNHry()
            {
                Patlist = plistName,
                TimingsTc = timName,
                LevelsTc = lvlName,
                PrePlist = prePlist,
                PerPatFailCaptureCount = perPatFailCaptureCount,
                HRYInputFile = hryInputFile,
                PinMappingInputFile = pinMappingInputFile,
            };

            var hryInputProcessedMock = new Mock<InputsProcessed>(MockBehavior.Strict);
            hryInputProcessedMock.Setup(func => func.ProcessData(hryInputFile, pinMappingInputFile));
            hryInputProcessedMock.Setup(func => func.PatternsInJson(plistName)).Throws(verifyPatternException);

            var hryInputMockInject = typeof(ScanSSNHry).GetField("hryInputDataProcessed", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            hryInputMockInject.SetValue(classUnderTest, hryInputProcessedMock.Object);

            Assert.ThrowsException<TestMethodException>(() => classUnderTest.Verify());
            funcServiceMock.VerifyAll();
            hryInputProcessedMock.VerifyAll();
            funcMock.VerifyAll();
        }

        /// <summary>
        /// Verify_PinMaskDoesntExist_ThrowsException.
        /// </summary>
        [TestMethod]
        public void Verify_PinMaskDoesntExist_ThrowsException()
        {
            var plistName = "ValidPlist";
            var lvlName = "ValidLevels";
            var timName = "ValidTiming";
            var prePlist = "SomeCallback()";
            uint perPatFailCaptureCount = 99;
            var hryInputFile = "HRYFile";
            var pinMappingInputFile = "HRYFile2";
            const string failingPin = ":(";
            const string passingPin = ":)";
            var maskPins = passingPin + "," + failingPin; // no pins to mask

            var pinServiceMock = new Mock<IPinService>();
            pinServiceMock.Setup(service => service.Exists(passingPin)).Returns(true);
            pinServiceMock.Setup(service => service.Exists(failingPin)).Returns(false);
            Prime.Services.PinService = pinServiceMock.Object;

            ScanSSNHry classUnderTest = new ScanSSNHry()
            {
                Patlist = plistName,
                TimingsTc = timName,
                LevelsTc = lvlName,
                PrePlist = prePlist,
                MaskPins = maskPins,
                PerPatFailCaptureCount = perPatFailCaptureCount,
                HRYInputFile = hryInputFile,
                PinMappingInputFile = pinMappingInputFile,
            };

            Assert.ThrowsException<TestMethodException>(() => classUnderTest.Verify());
            pinServiceMock.VerifyAll();
        }

        /// <summary>
        /// Verify with correct inputs.
        /// </summary>
        [TestMethod]
        public void Verify_CorrectInputs()
        {
            var plistName = "ValidPlist";
            var lvlName = "ValidLevels";
            var timName = "ValidTiming";
            var prePlist = "SomeCallback()";
            uint perPatFailCaptureCount = 99;
            var hryInputFile = "HRYFile";
            var pinMappingInputFile = "HRYFile2";

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func =>
                    func.CreateCaptureFailureTest(plistName, lvlName, timName, ulong.MaxValue, perPatFailCaptureCount, prePlist))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var path = GetSourcePath();
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(expression: file => file.FileExists(hryInputFile)).Returns(true);
            fileServiceMock.Setup(expression: file => file.GetFile(hryInputFile)).Returns($"{path}\\TemplateHrySCAN.json");
            fileServiceMock.Setup(expression: file => file.FileExists(pinMappingInputFile)).Returns(true);
            fileServiceMock.Setup(expression: file => file.GetFile(pinMappingInputFile)).Returns($"{path}\\PinMapping.json");
            Prime.Services.FileService = fileServiceMock.Object;

            var hryInputProcessedMock = new Mock<InputsProcessed>(MockBehavior.Strict);
            hryInputProcessedMock.Setup(func => func.ProcessData(hryInputFile, pinMappingInputFile));
            hryInputProcessedMock.Setup(func => func.PatternsInJson(plistName));
            hryInputProcessedMock.SetupAllProperties();

            ScanSSNHry classUnderTest = new ScanSSNHry()
            {
                Patlist = plistName,
                TimingsTc = timName,
                LevelsTc = lvlName,
                PrePlist = prePlist,
                PerPatFailCaptureCount = perPatFailCaptureCount,
                HRYInputFile = hryInputFile,
                PinMappingInputFile = pinMappingInputFile,
            };

            var hryInputMockInject = typeof(ScanSSNHry).GetField("hryInputDataProcessed", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            hryInputMockInject.SetValue(classUnderTest, hryInputProcessedMock.Object);
            classUnderTest.Verify();
            funcMock.VerifyAll();
            funcServiceMock.VerifyAll();
            hryInputProcessedMock.VerifyAll();
        }

        /// <summary>
        /// This tests method Execute when ExecuteAndCaptureScanFailuresPerPattern throws exception.
        /// </summary>
        [TestMethod]
        public void Execute_ExecuteAndCaptureScanFailuresPerPattern_ThrowsException()
        {
            var plistName = "ValidPlist";
            var lvlName = "ValidLevels";
            var timName = "ValidTiming";
            uint perPatFailCaptureCount = 99;
            var hryInputFile = "ValidFile";
            var pinMappingInputFile = "ValidFile2";
            var scanFails = new List<IFailureData>();
            var exceptionMessage = "ExecuteAndCaptureScanFailuresPerPattern() fails.";
            Prime.Base.Exceptions.FatalException funcException =
                new Prime.Base.Exceptions.FatalException(exceptionMessage);

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcMock.Setup(funcTest => funcTest.ApplyTestConditions());
            funcMock.Setup(funcTest => funcTest.SetPinMask(It.IsAny<List<string>>()));
            funcMock.Setup(func => func.Execute()).Throws(funcException);

            ScanSSNHry classUnderTest = new ScanSSNHry()
            {
                Patlist = plistName,
                TimingsTc = timName,
                LevelsTc = lvlName,
                PerPatFailCaptureCount = perPatFailCaptureCount,
                HRYInputFile = hryInputFile,
                PinMappingInputFile = pinMappingInputFile,
            };

            var funcTestInjection = typeof(ScanSSNHry).GetField("funcTest", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            funcTestInjection.SetValue(classUnderTest, funcMock.Object);

            Assert.ThrowsException<FatalException>(() => classUnderTest.Execute());
            funcMock.VerifyAll();
        }

        /// <summary>
        /// This tests method Execute when GenerateHRY throws exception.
        /// </summary>
        [TestMethod]
        public void Execute_GenerateHRY_ExitPort0()
        {
            var scanFails = new List<IFailureData>();

            var exceptionMessage = "GenerateHRY() fails.";
            TestMethodException generateHRYException =
                new TestMethodException(exceptionMessage);

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcMock.Setup(funcTest => funcTest.ApplyTestConditions());
            funcMock.Setup(funcTest => funcTest.SetPinMask(It.IsAny<List<string>>()));
            funcMock.Setup(func => func.Execute()).Returns(true);
            funcMock.Setup(func => func.GetPerCycleFailures()).Returns(scanFails);

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(console =>
                console.PrintError(
                    $"Test method has failed with this error message=[{generateHRYException.Message}].",
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var scanHryAlgorithmMock = new Mock<ScanSSNHRYCommonAlgorithm>(MockBehavior.Strict);
            scanHryAlgorithmMock.Setup(func =>
                    func.GenerateHRY(It.IsAny<InputsProcessed>(), It.IsAny<List<IFailureData>>(), It.IsAny<ulong>(), It.IsAny<string>(), ref It.Ref<List<string>>.IsAny, It.IsAny<List<string>>(), ref It.Ref<List<char>>.IsAny))
                    .Throws(generateHRYException);

            ScanSSNHry classUnderTest = new ScanSSNHry();

            var funcTestInjection = typeof(ScanSSNHry).GetField("funcTest", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            funcTestInjection.SetValue(classUnderTest, funcMock.Object);

            var hryAlgorithm = typeof(ScanSSNHry).GetField("hryCommonAlgorithm", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            hryAlgorithm.SetValue(classUnderTest, scanHryAlgorithmMock.Object);

            Assert.AreEqual(0, classUnderTest.Execute());
            funcMock.VerifyAll();
            consoleServiceMock.VerifyAll();
            scanHryAlgorithmMock.VerifyAll();
        }

        /// <summary>
        /// This tests case of no defects.
        /// </summary>
        [TestMethod]
        public void Execute_GenerateHRY_ExitPort1()
        {
            var plistName = "ValidPlist";
            var lvlName = "ValidLevels";
            var timName = "ValidTiming";
            var prePlist = "SomeCallback()";
            uint perPatFailCaptureCount = 99;
            var hryInputFile = "HRYFile";
            var pinMappingInputFile = "HRYFile2";
            int hryLength = 7;
            ulong pinNumbers = 32;
            var scanFails = new List<IFailureData>();
            var unasignedPartitions = new HashSet<int>() { 0, 3 };
            var instanceName = "MyInstance";
            var maskPins = "TDO,NOAB_01,NOAB_02";
            List<string> maskPinsList = new List<string> { "TDO", "NOAB_01", "NOAB_02" };
            List<IFailureData> listOfFailsPins = new List<IFailureData>();
            string expectedHryItuffPrint = "9119111";

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(expression: file => file.FileExists(hryInputFile)).Returns(true);
            fileServiceMock.Setup(expression: file => file.FileExists(pinMappingInputFile)).Returns(true);

            Prime.Services.FileService = fileServiceMock.Object;
            var pinServiceMock = new Mock<IPinService>();
            pinServiceMock.Setup(service => service.Exists("TDO")).Returns(true);
            pinServiceMock.Setup(service => service.Exists("NOAB_01")).Returns(true);
            pinServiceMock.Setup(service => service.Exists("NOAB_02")).Returns(true);
            Prime.Services.PinService = pinServiceMock.Object;

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcMock.Setup(funcTest => funcTest.ApplyTestConditions());
            funcMock.Setup(func => func.SetPinMask(maskPinsList));
            funcMock.Setup(func => func.Execute()).Returns(true);
            funcMock.Setup(func => func.GetPerCycleFailures()).Returns(scanFails);

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func =>
                    func.CreateCaptureFailureTest(plistName, lvlName, timName, ulong.MaxValue, perPatFailCaptureCount, prePlist))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetTnamePostfix("HRY_RAWSTR"));
            writerMock.Setup(o => o.SetData(expectedHryItuffPrint));
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.SetAltInstanceName(instanceName));
            datalogServiceMock.Setup(func => func.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var inputData = new HRYTemplateInput() { HryLength = hryLength };
            var pininputData = new PinMappingInput() { PinNumbers = pinNumbers };
            var processedInput = new InputsProcessed() { HryInputDataProcessed = inputData, UnassignedPartitions = unasignedPartitions, PinMappingProcessed = pininputData };
            var hryInputProcessedMock = new Mock<InputsProcessed>(MockBehavior.Strict);
            hryInputProcessedMock.Setup(func => func.ProcessData(hryInputFile, pinMappingInputFile));
            hryInputProcessedMock.Setup(func => func.PatternsInJson(plistName));
            hryInputProcessedMock.SetupAllProperties();
            hryInputProcessedMock.Object.HryInputDataProcessed = inputData;
            hryInputProcessedMock.Object.PinMappingProcessed = pininputData;
            hryInputProcessedMock.Object.UnassignedPartitions = unasignedPartitions;

            ScanSSNHry classUnderTest = new ScanSSNHry()
            {
                Patlist = plistName,
                TimingsTc = timName,
                LevelsTc = lvlName,
                PrePlist = prePlist,
                MaskPins = maskPins,
                PerPatFailCaptureCount = perPatFailCaptureCount,
                HRYInputFile = hryInputFile,
                InstanceName = instanceName,
                PinMappingInputFile = pinMappingInputFile,
            };

            var funcTestInjection = typeof(ScanSSNHry).GetField("funcTest", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            funcTestInjection.SetValue(classUnderTest, funcMock.Object);

            var inputProcessedtInjection = typeof(ScanSSNHry).GetField("hryInputDataProcessed", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            inputProcessedtInjection.SetValue(classUnderTest, hryInputProcessedMock.Object);

            classUnderTest.Verify();
            Assert.AreEqual(1, classUnderTest.Execute());
            Assert.AreEqual(0, listOfFailsPins.Count);
            funcMock.VerifyAll();
            funcServiceMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            hryInputProcessedMock.VerifyAll();
            pinServiceMock.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// This tests case of defects captured.
        /// </summary>
        [TestMethod]
        public void Execute_GenerateHRY_ExitPort2()
        {
            uint perPatFailCaptureCount = 99;
            string partitionsUnderDebug = "pariiomfmu";

            int hryLength = 4;
            ulong packetSize = 54;
            ulong outputPacketOffset = 700;

            var scanFails = new List<IFailureData>();
            var fail1Mock = new Mock<IFailureData>(MockBehavior.Strict);
            fail1Mock.Setup(func => func.GetCycle()).Returns(2);
            fail1Mock.Setup(func => func.GetPatternName()).Returns("hjhkj_scan_tretd");
            fail1Mock.Setup(func => func.GetFailingPinNames()).Returns(new List<string>() { "xx_PTI_11" });
            scanFails.Add(fail1Mock.Object);
            var fail2Mock = new Mock<IFailureData>(MockBehavior.Strict);
            fail2Mock.Setup(func => func.GetCycle()).Returns(2);
            fail2Mock.Setup(func => func.GetPatternName()).Returns("pariitcotc_ssn_scan_rev1");
            fail2Mock.Setup(func => func.GetFailingPinNames()).Returns(new List<string>() { "xx_PTI_11", "xx_PTI_7", "xx_PTI_24" });
            scanFails.Add(fail2Mock.Object);

            var unasignedPartitions = new HashSet<int>() { };
            var instanceName = "MyInstance";
            string expectedHryItuffPrint = "1101";

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcMock.Setup(funcTest => funcTest.ApplyTestConditions());
            funcMock.Setup(funcTest => funcTest.SetPinMask(It.IsAny<List<string>>()));
            funcMock.Setup(func => func.Execute()).Returns(false);
            funcMock.Setup(func => func.GetPerCycleFailures()).Returns(scanFails);

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetTnamePostfix("HRY_RAWSTR"));
            writerMock.Setup(o => o.SetTnamePostfix("pariiommu"));
            writerMock.Setup(o => o.SetData(expectedHryItuffPrint));
            writerMock.Setup(o => o.SetData("0"));
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.SetAltInstanceName(instanceName));
            datalogServiceMock.Setup(func => func.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var inputData = new HRYTemplateInput() { HryLength = hryLength, };

            var instance1a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 1, HRYPrint = "pariirp" };
            var instance2a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 2, HRYPrint = "pariiommu" };
            var instance3a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 3, HRYPrint = "pariitcotc" };
            var instance1 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance1a };
            var instance2 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance2a };
            var instance3 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance3a };
            var location1a = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "000|001|002|003|004|006|008|009|010|011|012|013|014|015|016", Partitions = instance1 }; // no pins needed, not accessed.
            var location1b = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "017|018|019|020|021|022|023|024|025|026|027|028|029|030|031|032|033|034|035|036|037|038", Partitions = instance2 };
            var location1c = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "039|040|041|042|043|044|045|046|047|048|049|050|051|052|053", Partitions = instance3 };
            var locations1 = new List<HRYTemplateInput.Pattern.Packet>() { location1a, location1b, location1c };
            var pattern1 = new HRYTemplateInput.Pattern() { ContentType = "mainContent", PatternRegex = ".*scan.*", Packets = locations1, PacketSize = packetSize, OutputPacketOffset = outputPacketOffset };
            var instance4b = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 0, HRYPrint = "Reset" };
            var instance4 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance4b };
            var location4a = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = ".*", Partitions = instance4 };
            var locations4 = new List<HRYTemplateInput.Pattern.Packet>() { location4a };
            var pattern2 = new HRYTemplateInput.Pattern() { ContentType = "preamble", PatternRegex = ".*reset.*|.*precat.*", Packets = locations4 };
            inputData.Patterns = new List<HRYTemplateInput.Pattern>() { pattern1, pattern2 };

            var pinMapping = new PinMappingInput() { PinNumbers = 32 };

            var pin1 = new PinMappingInput.PinMapping { Ssn_datapth = 8, PinName = "xx_PTI_11\\b" };
            var pin2 = new PinMappingInput.PinMapping { Ssn_datapth = 7, PinName = "xx_PTI_1\\b" };
            var pin3 = new PinMappingInput.PinMapping { Ssn_datapth = 22, PinName = "xx_PTI_2\\b" };
            var pin4 = new PinMappingInput.PinMapping { Ssn_datapth = 23, PinName = "xx_PTI_22\\b" };
            var pin5 = new PinMappingInput.PinMapping { Ssn_datapth = 1, PinName = "xx_PTI_24\\b" };
            var pin6 = new PinMappingInput.PinMapping { Ssn_datapth = 32, PinName = "xx_PTI_6\\b" };
            var pin7 = new PinMappingInput.PinMapping { Ssn_datapth = 17, PinName = "xx_PTI_7\\b" };
            var pin8 = new PinMappingInput.PinMapping { Ssn_datapth = 27, PinName = "xx_PTI_19\\b" };
            var pin9 = new PinMappingInput.PinMapping { Ssn_datapth = 18, PinName = "xx_PTI_10\\b" };
            pinMapping.PinsMapping = new List<PinMappingInput.PinMapping>() { pin1, pin2, pin3, pin4, pin5, pin6, pin7, pin8, pin9 };

            var processedInput = new InputsProcessed() { HryInputDataProcessed = inputData, PinMappingProcessed = pinMapping, UnassignedPartitions = unasignedPartitions };

            ScanSSNHry classUnderTest = new ScanSSNHry()
            {
                PerPatFailCaptureCount = perPatFailCaptureCount,
                InstanceName = instanceName,
                PartitionsUnderDebug = partitionsUnderDebug,
            };

            var funcTestInjection = typeof(ScanSSNHry).GetField("funcTest", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            funcTestInjection.SetValue(classUnderTest, funcMock.Object);

            var inputProcessedtInjection = typeof(ScanSSNHry).GetField("hryInputDataProcessed", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            inputProcessedtInjection.SetValue(classUnderTest, processedInput);

            Assert.AreEqual(2, classUnderTest.Execute());
            funcMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            fail1Mock.VerifyAll();
            fail2Mock.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// This test verify if exit port 4 when reset fail.
        /// </summary>
        [TestMethod]
        public void Execute_GenerateHRY_ResetFail_ExitPort4()
        {
            uint perPatFailCaptureCount = 99;
            string partitionsUnderDebug = "pariiommu";

            int hryLength = 4;
            ulong packetSize = 54;
            ulong outputPacketOffset = 700;

            var scanFails = new List<IFailureData>();
            var fail1Mock = new Mock<IFailureData>(MockBehavior.Strict);
            fail1Mock.Setup(func => func.GetPatternName()).Returns("hjhkj_reset_tretd");
            fail1Mock.Setup(func => func.GetFailingPinNames()).Returns(new List<string>() { "xx_PTI_11" });
            scanFails.Add(fail1Mock.Object);

            var unasignedPartitions = new HashSet<int>() { };
            var instanceName = "MyInstance";
            string expectedHryItuffPrint = "0111";

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcMock.Setup(funcTest => funcTest.ApplyTestConditions());
            funcMock.Setup(funcTest => funcTest.SetPinMask(It.IsAny<List<string>>()));
            funcMock.Setup(func => func.Execute()).Returns(false);
            funcMock.Setup(func => func.GetPerCycleFailures()).Returns(scanFails);

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetTnamePostfix("HRY_RAWSTR"));
            writerMock.Setup(o => o.SetTnamePostfix("Reset"));
            writerMock.Setup(o => o.SetData(expectedHryItuffPrint));
            writerMock.Setup(o => o.SetData("0"));
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.SetAltInstanceName(instanceName));
            datalogServiceMock.Setup(func => func.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var inputData = new HRYTemplateInput() { HryLength = hryLength, };

            var instance1a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 1, HRYPrint = "pariirp" };
            var instance2a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 2, HRYPrint = "pariiommu" };
            var instance3a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 3, HRYPrint = "pariitcotc" };
            var instance1 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance1a };
            var instance2 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance2a };
            var instance3 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance3a };
            var location1a = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "000|001|002|003|004|006|008|009|010|011|012|013|014|015|016", Partitions = instance1 }; // no pins needed, not accessed.
            var location1b = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "017|018|019|020|021|022|023|024|025|026|027|028|029|030|031|032|033|034|035|036|037|038", Partitions = instance2 };
            var location1c = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "039|040|041|042|043|044|045|046|047|048|049|050|051|052|053", Partitions = instance3 };
            var locations1 = new List<HRYTemplateInput.Pattern.Packet>() { location1a, location1b, location1c };
            var pattern1 = new HRYTemplateInput.Pattern() { ContentType = "mainContent", PatternRegex = ".*scan.*", Packets = locations1, PacketSize = packetSize, OutputPacketOffset = outputPacketOffset };
            var instance4b = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 0, HRYPrint = "Reset" };
            var instance4 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance4b };
            var location4a = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = ".*", Partitions = instance4 };
            var locations4 = new List<HRYTemplateInput.Pattern.Packet>() { location4a };
            var pattern2 = new HRYTemplateInput.Pattern() { ContentType = "preamble", PatternRegex = ".*reset.*|.*precat.*", Packets = locations4 };
            inputData.Patterns = new List<HRYTemplateInput.Pattern>() { pattern1, pattern2 };

            var pinMapping = new PinMappingInput() { PinNumbers = 32 };

            var pin1 = new PinMappingInput.PinMapping { Ssn_datapth = 8, PinName = "xx_PTI_11\\b" };
            var pin2 = new PinMappingInput.PinMapping { Ssn_datapth = 7, PinName = "xx_PTI_1\\b" };
            var pin3 = new PinMappingInput.PinMapping { Ssn_datapth = 22, PinName = "xx_PTI_2\\b" };
            var pin4 = new PinMappingInput.PinMapping { Ssn_datapth = 23, PinName = "xx_PTI_22\\b" };
            var pin5 = new PinMappingInput.PinMapping { Ssn_datapth = 1, PinName = "xx_PTI_24\\b" };
            var pin6 = new PinMappingInput.PinMapping { Ssn_datapth = 32, PinName = "xx_PTI_6\\b" };
            var pin7 = new PinMappingInput.PinMapping { Ssn_datapth = 17, PinName = "xx_PTI_7\\b" };
            var pin8 = new PinMappingInput.PinMapping { Ssn_datapth = 27, PinName = "xx_PTI_19\\b" };
            var pin9 = new PinMappingInput.PinMapping { Ssn_datapth = 18, PinName = "xx_PTI_10\\b" };
            pinMapping.PinsMapping = new List<PinMappingInput.PinMapping>() { pin1, pin2, pin3, pin4, pin5, pin6, pin7, pin8, pin9 };

            var processedInput = new InputsProcessed() { HryInputDataProcessed = inputData, PinMappingProcessed = pinMapping, UnassignedPartitions = unasignedPartitions };

            ScanSSNHry classUnderTest = new ScanSSNHry()
            {
                PerPatFailCaptureCount = perPatFailCaptureCount,
                InstanceName = instanceName,
                PartitionsUnderDebug = partitionsUnderDebug,
            };

            var funcTestInjection = typeof(ScanSSNHry).GetField("funcTest", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            funcTestInjection.SetValue(classUnderTest, funcMock.Object);

            var inputProcessedtInjection = typeof(ScanSSNHry).GetField("hryInputDataProcessed", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            inputProcessedtInjection.SetValue(classUnderTest, processedInput);

            Assert.AreEqual(4, classUnderTest.Execute());
            funcMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            fail1Mock.VerifyAll();
        }

        /// <summary>
        /// This test verify if RAWSTR print 9 when some specific partition is added to be mask under PartitionsUnderDebug parameter.
        /// </summary>
        [TestMethod]
        public void Execute_GenerateHRY_Printing9_RAWSTR_Port1()
        {
            uint perPatFailCaptureCount = 99;
            string partitionsUnderDebug = "pariiommu";

            int hryLength = 4;
            ulong packetSize = 54;
            ulong outputPacketOffset = 700;

            var scanFails = new List<IFailureData>();
            var fail1Mock = new Mock<IFailureData>(MockBehavior.Strict);
            fail1Mock.Setup(func => func.GetPatternName()).Returns("hjhkj_scan_tretd");
            fail1Mock.Setup(func => func.GetFailingPinNames()).Returns(new List<string>() { "xx_PTI_11" });
            fail1Mock.Setup(func => func.GetCycle()).Returns(2);
            scanFails.Add(fail1Mock.Object);

            var unasignedPartitions = new HashSet<int>() { };
            var instanceName = "MyInstance";
            string expectedHryItuffPrint = "1191";

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcMock.Setup(funcTest => funcTest.ApplyTestConditions());
            funcMock.Setup(funcTest => funcTest.SetPinMask(It.IsAny<List<string>>()));
            funcMock.Setup(func => func.Execute()).Returns(false);
            funcMock.Setup(func => func.GetPerCycleFailures()).Returns(scanFails);

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetTnamePostfix("HRY_RAWSTR"));
            writerMock.Setup(o => o.SetData(expectedHryItuffPrint));
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.SetAltInstanceName(instanceName));
            datalogServiceMock.Setup(func => func.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var inputData = new HRYTemplateInput() { HryLength = hryLength, };

            var instance1a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 1, HRYPrint = "pariirp" };
            var instance2a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 2, HRYPrint = "pariiommu" };
            var instance3a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 3, HRYPrint = "pariitcotc" };
            var instance1 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance1a };
            var instance2 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance2a };
            var instance3 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance3a };
            var location1a = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "000|001|002|003|004|006|008|009|010|011|012|013|014|015|016", Partitions = instance1 }; // no pins needed, not accessed.
            var location1b = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "017|018|019|020|021|022|023|024|025|026|027|028|029|030|031|032|033|034|035|036|037|038", Partitions = instance2 };
            var location1c = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "039|040|041|042|043|044|045|046|047|048|049|050|051|052|053", Partitions = instance3 };
            var locations1 = new List<HRYTemplateInput.Pattern.Packet>() { location1a, location1b, location1c };
            var pattern1 = new HRYTemplateInput.Pattern() { ContentType = "mainContent", PatternRegex = ".*scan.*", Packets = locations1, PacketSize = packetSize, OutputPacketOffset = outputPacketOffset };
            var instance4b = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 0, HRYPrint = "Reset" };
            var instance4 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance4b };
            var location4a = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = ".*", Partitions = instance4 };
            var locations4 = new List<HRYTemplateInput.Pattern.Packet>() { location4a };
            var pattern2 = new HRYTemplateInput.Pattern() { ContentType = "preamble", PatternRegex = ".*reset.*|.*precat.*", Packets = locations4 };
            inputData.Patterns = new List<HRYTemplateInput.Pattern>() { pattern1, pattern2 };

            var pinMapping = new PinMappingInput() { PinNumbers = 32 };

            var pin1 = new PinMappingInput.PinMapping { Ssn_datapth = 8, PinName = "xx_PTI_11\\b" };
            var pin2 = new PinMappingInput.PinMapping { Ssn_datapth = 7, PinName = "xx_PTI_1\\b" };
            var pin3 = new PinMappingInput.PinMapping { Ssn_datapth = 22, PinName = "xx_PTI_2\\b" };
            var pin4 = new PinMappingInput.PinMapping { Ssn_datapth = 23, PinName = "xx_PTI_22\\b" };
            var pin5 = new PinMappingInput.PinMapping { Ssn_datapth = 1, PinName = "xx_PTI_24\\b" };
            var pin6 = new PinMappingInput.PinMapping { Ssn_datapth = 32, PinName = "xx_PTI_6\\b" };
            var pin7 = new PinMappingInput.PinMapping { Ssn_datapth = 17, PinName = "xx_PTI_7\\b" };
            var pin8 = new PinMappingInput.PinMapping { Ssn_datapth = 27, PinName = "xx_PTI_19\\b" };
            var pin9 = new PinMappingInput.PinMapping { Ssn_datapth = 18, PinName = "xx_PTI_10\\b" };
            pinMapping.PinsMapping = new List<PinMappingInput.PinMapping>() { pin1, pin2, pin3, pin4, pin5, pin6, pin7, pin8, pin9 };

            var processedInput = new InputsProcessed() { HryInputDataProcessed = inputData, PinMappingProcessed = pinMapping, UnassignedPartitions = unasignedPartitions };

            ScanSSNHry classUnderTest = new ScanSSNHry()
            {
                PerPatFailCaptureCount = perPatFailCaptureCount,
                InstanceName = instanceName,
                PartitionsUnderDebug = partitionsUnderDebug,
            };

            var funcTestInjection = typeof(ScanSSNHry).GetField("funcTest", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            funcTestInjection.SetValue(classUnderTest, funcMock.Object);

            var inputProcessedtInjection = typeof(ScanSSNHry).GetField("hryInputDataProcessed", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            inputProcessedtInjection.SetValue(classUnderTest, processedInput);

            Assert.AreEqual(1, classUnderTest.Execute());
            funcMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            fail1Mock.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// This test verify if RAWSTR print 9 when some specific partition is added to be mask under PartitionsUnderDebug parameter.
        /// </summary>
        [TestMethod]
        public void Execute_GenerateHRY_Printing9_RAWSTR_Port2()
        {
            uint perPatFailCaptureCount = 99;
            string partitionsUnderDebug = "pariiommu";

            int hryLength = 4;
            ulong packetSize = 54;
            ulong outputPacketOffset = 700;

            var scanFails = new List<IFailureData>();
            var fail1Mock = new Mock<IFailureData>(MockBehavior.Strict);
            fail1Mock.Setup(func => func.GetPatternName()).Returns("hjhkj_scan_tretd");
            fail1Mock.Setup(func => func.GetFailingPinNames()).Returns(new List<string>() { "xx_PTI_11" });
            fail1Mock.Setup(func => func.GetCycle()).Returns(2);
            scanFails.Add(fail1Mock.Object);
            var fail2Mock = new Mock<IFailureData>(MockBehavior.Strict);
            fail2Mock.Setup(func => func.GetPatternName()).Returns("hjhkj_scan_tretd");
            fail2Mock.Setup(func => func.GetFailingPinNames()).Returns(new List<string>() { "xx_PTI_11" });
            fail2Mock.Setup(func => func.GetCycle()).Returns(3);
            scanFails.Add(fail2Mock.Object);

            var unasignedPartitions = new HashSet<int>() { };
            var instanceName = "MyInstance";
            string expectedHryItuffPrint = "1091";

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcMock.Setup(funcTest => funcTest.ApplyTestConditions());
            funcMock.Setup(funcTest => funcTest.SetPinMask(It.IsAny<List<string>>()));
            funcMock.Setup(func => func.Execute()).Returns(false);
            funcMock.Setup(func => func.GetPerCycleFailures()).Returns(scanFails);

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetTnamePostfix("HRY_RAWSTR"));
            writerMock.Setup(o => o.SetTnamePostfix("pariirp"));
            writerMock.Setup(o => o.SetData(expectedHryItuffPrint));
            writerMock.Setup(o => o.SetData("0"));
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.SetAltInstanceName(instanceName));
            datalogServiceMock.Setup(func => func.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var inputData = new HRYTemplateInput() { HryLength = hryLength, };

            var instance1a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 1, HRYPrint = "pariirp" };
            var instance2a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 2, HRYPrint = "pariiommu" };
            var instance3a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 3, HRYPrint = "pariitcotc" };
            var instance1 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance1a };
            var instance2 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance2a };
            var instance3 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance3a };
            var location1a = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "000|001|002|003|004|006|008|009|010|011|012|013|014|015|016", Partitions = instance1 }; // no pins needed, not accessed.
            var location1b = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "017|018|019|020|021|022|023|024|025|026|027|028|029|030|031|032|033|034|035|036|037|038", Partitions = instance2 };
            var location1c = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "039|040|041|042|043|044|045|046|047|048|049|050|051|052|053", Partitions = instance3 };
            var locations1 = new List<HRYTemplateInput.Pattern.Packet>() { location1a, location1b, location1c };
            var pattern1 = new HRYTemplateInput.Pattern() { ContentType = "mainContent", PatternRegex = ".*scan.*", Packets = locations1, PacketSize = packetSize, OutputPacketOffset = outputPacketOffset };
            var instance4b = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 0, HRYPrint = "Reset" };
            var instance4 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance4b };
            var location4a = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = ".*", Partitions = instance4 };
            var locations4 = new List<HRYTemplateInput.Pattern.Packet>() { location4a };
            var pattern2 = new HRYTemplateInput.Pattern() { ContentType = "preamble", PatternRegex = ".*reset.*|.*precat.*", Packets = locations4 };
            inputData.Patterns = new List<HRYTemplateInput.Pattern>() { pattern1, pattern2 };

            var pinMapping = new PinMappingInput() { PinNumbers = 32 };

            var pin1 = new PinMappingInput.PinMapping { Ssn_datapth = 8, PinName = "xx_PTI_11\\b" };
            var pin2 = new PinMappingInput.PinMapping { Ssn_datapth = 7, PinName = "xx_PTI_1\\b" };
            var pin3 = new PinMappingInput.PinMapping { Ssn_datapth = 22, PinName = "xx_PTI_2\\b" };
            var pin4 = new PinMappingInput.PinMapping { Ssn_datapth = 23, PinName = "xx_PTI_22\\b" };
            var pin5 = new PinMappingInput.PinMapping { Ssn_datapth = 1, PinName = "xx_PTI_24\\b" };
            var pin6 = new PinMappingInput.PinMapping { Ssn_datapth = 32, PinName = "xx_PTI_6\\b" };
            var pin7 = new PinMappingInput.PinMapping { Ssn_datapth = 17, PinName = "xx_PTI_7\\b" };
            var pin8 = new PinMappingInput.PinMapping { Ssn_datapth = 27, PinName = "xx_PTI_19\\b" };
            var pin9 = new PinMappingInput.PinMapping { Ssn_datapth = 18, PinName = "xx_PTI_10\\b" };
            pinMapping.PinsMapping = new List<PinMappingInput.PinMapping>() { pin1, pin2, pin3, pin4, pin5, pin6, pin7, pin8, pin9 };

            var processedInput = new InputsProcessed() { HryInputDataProcessed = inputData, PinMappingProcessed = pinMapping, UnassignedPartitions = unasignedPartitions };

            ScanSSNHry classUnderTest = new ScanSSNHry()
            {
                PerPatFailCaptureCount = perPatFailCaptureCount,
                InstanceName = instanceName,
                PartitionsUnderDebug = partitionsUnderDebug,
            };

            var funcTestInjection = typeof(ScanSSNHry).GetField("funcTest", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            funcTestInjection.SetValue(classUnderTest, funcMock.Object);

            var inputProcessedtInjection = typeof(ScanSSNHry).GetField("hryInputDataProcessed", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            inputProcessedtInjection.SetValue(classUnderTest, processedInput);

            Assert.AreEqual(2, classUnderTest.Execute());
            funcMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            fail1Mock.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// This test verify if RAWSTR print 9 when some specific partition is added to be mask under PartitionsUnderDebug parameter.
        /// </summary>
        [TestMethod]
        public void Execute_GenerateHRY_Printing99_RAWSTR_Port1()
        {
            uint perPatFailCaptureCount = 99;
            string partitionsUnderDebug = "pariiommu,pariirp";

            int hryLength = 4;
            ulong packetSize = 54;
            ulong outputPacketOffset = 700;

            var scanFails = new List<IFailureData>();
            var fail1Mock = new Mock<IFailureData>(MockBehavior.Strict);
            fail1Mock.Setup(func => func.GetPatternName()).Returns("hjhkj_scan_tretd");
            fail1Mock.Setup(func => func.GetFailingPinNames()).Returns(new List<string>() { "xx_PTI_11" });
            fail1Mock.Setup(func => func.GetCycle()).Returns(2);
            scanFails.Add(fail1Mock.Object);
            var fail2Mock = new Mock<IFailureData>(MockBehavior.Strict);
            fail2Mock.Setup(func => func.GetPatternName()).Returns("hjhkj_scan_tretd");
            fail2Mock.Setup(func => func.GetFailingPinNames()).Returns(new List<string>() { "xx_PTI_11" });
            fail2Mock.Setup(func => func.GetCycle()).Returns(3);
            scanFails.Add(fail2Mock.Object);

            var unasignedPartitions = new HashSet<int>() { };
            var instanceName = "MyInstance";
            string expectedHryItuffPrint = "1991";

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            funcMock.Setup(funcTest => funcTest.ApplyTestConditions());
            funcMock.Setup(funcTest => funcTest.SetPinMask(It.IsAny<List<string>>()));
            funcMock.Setup(func => func.Execute()).Returns(false);
            funcMock.Setup(func => func.GetPerCycleFailures()).Returns(scanFails);

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetTnamePostfix("HRY_RAWSTR"));
            writerMock.Setup(o => o.SetData(expectedHryItuffPrint));
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.SetAltInstanceName(instanceName));
            datalogServiceMock.Setup(func => func.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            var inputData = new HRYTemplateInput() { HryLength = hryLength, };

            var instance1a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 1, HRYPrint = "pariirp" };
            var instance2a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 2, HRYPrint = "pariiommu" };
            var instance3a = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 3, HRYPrint = "pariitcotc" };
            var instance1 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance1a };
            var instance2 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance2a };
            var instance3 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance3a };
            var location1a = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "000|001|002|003|004|006|008|009|010|011|012|013|014|015|016", Partitions = instance1 }; // no pins needed, not accessed.
            var location1b = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "017|018|019|020|021|022|023|024|025|026|027|028|029|030|031|032|033|034|035|036|037|038", Partitions = instance2 };
            var location1c = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = "039|040|041|042|043|044|045|046|047|048|049|050|051|052|053", Partitions = instance3 };
            var locations1 = new List<HRYTemplateInput.Pattern.Packet>() { location1a, location1b, location1c };
            var pattern1 = new HRYTemplateInput.Pattern() { ContentType = "mainContent", PatternRegex = ".*scan.*", Packets = locations1, PacketSize = packetSize, OutputPacketOffset = outputPacketOffset };
            var instance4b = new HRYTemplateInput.Pattern.Packet.Partition() { HRYIndex = 0, HRYPrint = "Reset" };
            var instance4 = new List<HRYTemplateInput.Pattern.Packet.Partition>() { instance4b };
            var location4a = new HRYTemplateInput.Pattern.Packet() { SSNPacketBitPosition = ".*", Partitions = instance4 };
            var locations4 = new List<HRYTemplateInput.Pattern.Packet>() { location4a };
            var pattern2 = new HRYTemplateInput.Pattern() { ContentType = "preamble", PatternRegex = ".*reset.*|.*precat.*", Packets = locations4 };
            inputData.Patterns = new List<HRYTemplateInput.Pattern>() { pattern1, pattern2 };

            var pinMapping = new PinMappingInput() { PinNumbers = 32 };

            var pin1 = new PinMappingInput.PinMapping { Ssn_datapth = 8, PinName = "xx_PTI_11\\b" };
            var pin2 = new PinMappingInput.PinMapping { Ssn_datapth = 7, PinName = "xx_PTI_1\\b" };
            var pin3 = new PinMappingInput.PinMapping { Ssn_datapth = 22, PinName = "xx_PTI_2\\b" };
            var pin4 = new PinMappingInput.PinMapping { Ssn_datapth = 23, PinName = "xx_PTI_22\\b" };
            var pin5 = new PinMappingInput.PinMapping { Ssn_datapth = 1, PinName = "xx_PTI_24\\b" };
            var pin6 = new PinMappingInput.PinMapping { Ssn_datapth = 32, PinName = "xx_PTI_6\\b" };
            var pin7 = new PinMappingInput.PinMapping { Ssn_datapth = 17, PinName = "xx_PTI_7\\b" };
            var pin8 = new PinMappingInput.PinMapping { Ssn_datapth = 27, PinName = "xx_PTI_19\\b" };
            var pin9 = new PinMappingInput.PinMapping { Ssn_datapth = 18, PinName = "xx_PTI_10\\b" };
            pinMapping.PinsMapping = new List<PinMappingInput.PinMapping>() { pin1, pin2, pin3, pin4, pin5, pin6, pin7, pin8, pin9 };

            var processedInput = new InputsProcessed() { HryInputDataProcessed = inputData, PinMappingProcessed = pinMapping, UnassignedPartitions = unasignedPartitions };

            ScanSSNHry classUnderTest = new ScanSSNHry()
            {
                PerPatFailCaptureCount = perPatFailCaptureCount,
                InstanceName = instanceName,
                PartitionsUnderDebug = partitionsUnderDebug,
            };

            var funcTestInjection = typeof(ScanSSNHry).GetField("funcTest", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            funcTestInjection.SetValue(classUnderTest, funcMock.Object);

            var inputProcessedtInjection = typeof(ScanSSNHry).GetField("hryInputDataProcessed", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            inputProcessedtInjection.SetValue(classUnderTest, processedInput);

            Assert.AreEqual(1, classUnderTest.Execute());
            funcMock.VerifyAll();
            datalogServiceMock.VerifyAll();
            fail1Mock.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// This tests case of defects captured.
        /// </summary>
        [TestMethod]
        public void Verify_If_Pattern_Exist_in_Json_UsingRealFiles()
        {
            var plistName = "ValidPlist";
            var lvlName = "ValidLevels";
            var timName = "ValidTiming";
            var prePlist = "SomeCallback()";
            var hryInputFile = @".\TemplateHrySCAN.json";
            var pinMappingInputFile = @".\PinMapping.json";
            uint perPatFailCaptureCount = 99;
            var unasignedPartitions = new HashSet<int>() { };
            var patternsPlist = new HashSet<string>() { "scanfsdfsdfgdafgadfgsdfsdgdf", "hjhkhjk_scan" };

            var path = GetSourcePath();
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(expression: file => file.FileExists(hryInputFile)).Returns(true);
            fileServiceMock.Setup(expression: file => file.GetFile(hryInputFile)).Returns($"{path}\\TemplateHrySCAN.json");
            fileServiceMock.Setup(expression: file => file.FileExists(pinMappingInputFile)).Returns(true);
            fileServiceMock.Setup(expression: file => file.GetFile(pinMappingInputFile)).Returns($"{path}\\PinMapping.json");
            Prime.Services.FileService = fileServiceMock.Object;

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func =>
                    func.CreateCaptureFailureTest(plistName, lvlName, timName, ulong.MaxValue, perPatFailCaptureCount, prePlist))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            this.plistMock.Setup(mock => mock.GetUniquePatternNames()).Returns(patternsPlist);

            ScanSSNHry classUnderTest = new ScanSSNHry()
            {
                Patlist = plistName,
                TimingsTc = timName,
                LevelsTc = lvlName,
                PrePlist = prePlist,
                PerPatFailCaptureCount = perPatFailCaptureCount,
                HRYInputFile = hryInputFile,
                PinMappingInputFile = pinMappingInputFile,
            };
            classUnderTest.Verify();
            funcMock.VerifyAll();
            funcServiceMock.Verify();
            this.plistMock.VerifyAll();
        }

        /// <summary>
        /// This tests case of defects captured.
        /// </summary>
        [TestMethod]
        public void Verify_If_Pattern_Exist_in_Json_UsingRealFiles_ThrowException()
        {
            var plistName = "ValidPlist";
            var lvlName = "ValidLevels";
            var timName = "ValidTiming";
            var prePlist = "SomeCallback()";
            var hryInputFile = @".\TemplateHrySCAN.json";
            var pinMappingInputFile = @".\PinMapping.json";
            uint perPatFailCaptureCount = 99;
            var unasignedPartitions = new HashSet<int>() { };
            var patternsPlist = new HashSet<string>() { "scanfsdfsdfgdafgadfgsdfsdgdf", "hjhkhjk_scavn" };

            var path = GetSourcePath();
            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(expression: file => file.FileExists(hryInputFile)).Returns(true);
            fileServiceMock.Setup(expression: file => file.GetFile(hryInputFile)).Returns($"{path}\\TemplateHrySCAN.json");
            fileServiceMock.Setup(expression: file => file.FileExists(pinMappingInputFile)).Returns(true);
            fileServiceMock.Setup(expression: file => file.GetFile(pinMappingInputFile)).Returns($"{path}\\PinMapping.json");
            Prime.Services.FileService = fileServiceMock.Object;

            var funcMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(func =>
                    func.CreateCaptureFailureTest(plistName, lvlName, timName, ulong.MaxValue, perPatFailCaptureCount, prePlist))
                .Returns(funcMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            this.plistMock.Setup(mock => mock.GetUniquePatternNames()).Returns(patternsPlist);

            ScanSSNHry classUnderTest = new ScanSSNHry()
            {
                Patlist = plistName,
                TimingsTc = timName,
                LevelsTc = lvlName,
                PrePlist = prePlist,
                PerPatFailCaptureCount = perPatFailCaptureCount,
                HRYInputFile = hryInputFile,
                PinMappingInputFile = pinMappingInputFile,
            };

            Assert.ThrowsException<TestMethodException>(() => classUnderTest.Verify());
            funcMock.VerifyAll();
            funcServiceMock.VerifyAll();
            this.plistMock.VerifyAll();
        }

        private static string GetSourcePath([CallerFilePath] string path = null)
        {
            return Path.GetDirectoryName(path);
        }
    }
}
