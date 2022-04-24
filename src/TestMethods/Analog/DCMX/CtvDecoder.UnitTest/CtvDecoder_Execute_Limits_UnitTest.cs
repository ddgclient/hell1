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

namespace CtvDecoder.UnitTest
{
    using System;
    using System.Collections.Generic;
    using global::CtvDecoder.UnitTest.TestInputFiles;
    using global::CtvServices.ConfigurationFile;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.TestMethods.Functional;
    using Prime.TpSettingsService;

    /// <summary>
    /// Unit test class.
    /// </summary>
    [TestClass]
    public class CtvDecoder_Execute_Limits_UnitTest : CtvDecoder
    {
        private const string TestFileEquationLimits = "TestFileEquationLimits.csv";
        private const string TestFileBaseExpectedData = "TestFileBaseExpectedData.csv";
        private const string TestFileLowLimits = "TestFileLowLimits.csv";
        private const string TestFileHighLimits = "TestFileHighLimits.csv";
        private const string TestFileExitPort = "TestFileExitPort.csv";

        private Mock<IConsoleService> consoleServiceMock;

        /// <summary>
        /// Since the contents of "Verify()" were moved to "CustomVerify()", need to do this setup for everything.
        /// </summary>
        [TestInitialize]
        public void SetupCommonMocks()
        {
            var funcServiceMock = new Mock<Prime.FunctionalService.IFunctionalService>(MockBehavior.Loose);
            var funcTestkMock = new Mock<Prime.FunctionalService.ICaptureCtvPerPinTest>(MockBehavior.Loose);
            funcServiceMock.Setup(o => o.CreateCaptureCtvPerPinTest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), string.Empty))
                .Returns(funcTestkMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;
        }

        /// <summary>
        /// Execute unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_Execute_EquationLimits_Pass()
        {
            this.Setup(TestFileEquationLimits);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileEquationLimits;

            // input setup
            const string dataPinDDR1DQ50 = "001001110";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("36|3|108"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Services.DatalogService = dataLogServiceMock.Object;

            // Execution & Assert
            this.CustomVerify();
            var toExecute = this as IFunctionalExtensions;
            Assert.AreEqual(true, toExecute.ProcessCtvPerPin(inputCtvData));

            istrgvalFormat.VerifyAll();
            dataLogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_Execute_EquationLimits_Fail()
        {
            this.Setup(TestFileEquationLimits);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileEquationLimits;

            // input setup
            const string dataPinDDR1DQ50 = "001000110";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_fc"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("4|3|12"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("DDR1_DQ50.V0.training.subseq2.result:12"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Services.DatalogService = dataLogServiceMock.Object;

            // Execution & Assert
            this.CustomVerify();
            var toExecute = this as IFunctionalExtensions;
            Assert.AreEqual(false, toExecute.ProcessCtvPerPin(inputCtvData));

            istrgvalFormat.VerifyAll();
            dataLogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_Execute_LowLimits_Pass()
        {
            this.Setup(TestFileLowLimits);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileLowLimits;

            // input setup
            const string dataPinDDR1DQ50 = "0010111111001";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("20|15|9"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Services.DatalogService = dataLogServiceMock.Object;

            // Execution & Assert
            this.CustomVerify();
            var toExecute = this as IFunctionalExtensions;
            Assert.AreEqual(true, toExecute.ProcessCtvPerPin(inputCtvData));

            istrgvalFormat.VerifyAll();
            dataLogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_Execute_LowLimits_Fail()
        {
            this.Setup(TestFileLowLimits);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileLowLimits;

            // input setup
            const string dataPinDDR1DQ50 = "0010111111010";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_fc"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("20|15|5"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("DDR1_DQ50.V0.training.subseq3.val3:5"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Services.DatalogService = dataLogServiceMock.Object;

            // Execution & Assert
            this.CustomVerify();
            var toExecute = this as IFunctionalExtensions;
            Assert.AreEqual(false, toExecute.ProcessCtvPerPin(inputCtvData));

            istrgvalFormat.VerifyAll();
            dataLogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_Execute_HighLimits_Pass()
        {
            this.Setup(TestFileHighLimits);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileHighLimits;

            // input setup
            const string dataPinDDR1DQ50 = "0010111111001";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("20|15|9"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Services.DatalogService = dataLogServiceMock.Object;

            // Execution & Assert
            this.CustomVerify();
            var toExecute = this as IFunctionalExtensions;
            Assert.AreEqual(true, toExecute.ProcessCtvPerPin(inputCtvData));

            istrgvalFormat.VerifyAll();
            dataLogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_Execute_HighLimits_Fail()
        {
            this.Setup(TestFileHighLimits);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileHighLimits;

            // input setup
            const string dataPinDDR1DQ50 = "0111111111010";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_fc"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("30|15|5"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("DDR1_DQ50.V0.training.subseq1.val1:30"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Services.DatalogService = dataLogServiceMock.Object;

            // Execution & Assert
            this.CustomVerify();
            var toExecute = this as IFunctionalExtensions;
            Assert.AreEqual(false, toExecute.ProcessCtvPerPin(inputCtvData));

            istrgvalFormat.VerifyAll();
            dataLogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_Execute_BaseLimits_Pass()
        {
            this.Setup(TestFileBaseExpectedData);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileBaseExpectedData;

            // input setup
            const string dataPinDDR1DQ50 = "110100001100";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("0B|10|001"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Services.DatalogService = dataLogServiceMock.Object;

            // Execution & Assert
            this.CustomVerify();
            var toExecute = this as IFunctionalExtensions;
            Assert.AreEqual(true, toExecute.ProcessCtvPerPin(inputCtvData));

            istrgvalFormat.VerifyAll();
            dataLogServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_Execute_ExitPort_Pass()
        {
            this.Setup(TestFileExitPort);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileExitPort;

            // input setup
            const string dataPinDDR1DQ50 = "10111010000011";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("|MDFI_DDR1_DQ50_Field0_DQ[1]_failbump"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_fc"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1011|5|0|6"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("DDR1_DQ50.IP1.Field1:5|DDR1_DQ50.IP2.Field0:0"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Services.DatalogService = dataLogServiceMock.Object;

            // Execution & Assert
            this.CustomVerify();
            var toExecute = this as IFunctionalExtensions;
            Assert.AreEqual(false, toExecute.ProcessCtvPerPin(inputCtvData));

            istrgvalFormat.VerifyAll();
            dataLogServiceMock.VerifyAll();
        }

        private void Setup(string fileName)
        {
            // Mocks
            var fileHandlerMock = new Mock<IFileHandler>(MockBehavior.Strict);
            var configurationFileContent = InputFileReader.ReadResourceFile(fileName);
            fileHandlerMock.Setup(x => x.ReadAllLines(fileName)).Returns(configurationFileContent);
            this.CtvServices.FileHandler = fileHandlerMock.Object;

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(x => x.GetFile(fileName))
                .Returns(fileName);
            Services.FileService = fileServiceMock.Object;

            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(x => x.PrintDebug($"[INFO] This is the ConfigurationFile [{fileName}]")).Callback((string s) => Console.WriteLine(s));
            Services.ConsoleService = this.consoleServiceMock.Object;
        }
    }
}
