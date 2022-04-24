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
    using Prime.Base.Exceptions;
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
    public class CtvDecoder_Execute_ExpectedData_UnitTest : CtvDecoder
    {
        private const string TestFileGenericPass = "TestFileGenericPass.csv";
        private const string TestFileEquationExpectedData = "TestFileEquationExpectedData.csv";
        private const string TestFileCodification = "TestFileCodification.csv";
        private const string TestFilePerBit = "TestFilePerBit.csv";
        private const string TestFileBaseExpectedData = "TestFileBaseExpectedData.csv";
        private const string TestFileOffsetField = "TestFileOffsetField.csv";
        private const string TestFileFieldParametersPrint = "TestFileFieldParametersPrint.csv";
        private const string TestFilePinFinderFormatPass = "TestFilePinFinderFormatPass.csv";
        private const string TestFilePinRenameCheck = "TestFilePinRenameCheck.csv";
        private const string TestFileOnesComplement = "TestFileOnesComplement.csv";
        private const string TestFileTwosComplement = "TestFileTwosComplement.csv";
        private const string TestFileCodificationFail = "TestFileCodificationFail.csv";
        private const string TestFileTssidMsunitCheck = "TestFileTssidMsunitCheck.csv";
        private const string TestFileMultipleTssidMsunitCheck = "TestFileMultipleTssidMsunitCheck.csv";
        private const string TestFileStringData = "TestFileStringData.csv";
        private const string TestFileStringDataFail = "TestFileStringDataFail.csv";

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
        public void CtvDecoder_Execute_Pass()
        {
            this.Setup(TestFileGenericPass);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50,DDR1_DQ51";
            this.TssidRename = "DDR1_DQ50,DDR1_DQ51";
            this.ConfigurationFile = TestFileGenericPass;

            // input setup
            const string dataPinDDR1DQ50 = "11111111111111111111111100111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111100111111111111111111111111111111111111111111111111111111111111111111111111111111111001111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111001111111111111111111111111111111111111111111111111111111111111111111111111111111110011111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111110011111111111111111111111111111111111111111111111111111111111111111111111111111111100111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111100111111111111111111111111111111111111111111111111111111111";
            const string dataPinDDR1DQ51 = "11111111111111111111111100111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111100111111111111111111111111111111111111111111111111111111111111111111111111111111111001111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111001111111111111111111111111111111111111111111111111111111111111111111111111111111110011111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111110011111111111111111111111111111111111111111111111111111111111111111111111111111111100111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111100111111111111111111111111111111111111111111111111111111111";
            var inputCtvData = new Dictionary<string, string>(2)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
                { "DDR1_DQ51", dataPinDDR1DQ51 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token2"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token3"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token4"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ51_FSM_Status"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("63|7|1|1023|1|1|1|1|0|0|1|1023|1023|1022|511|481|111111111111111111111111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111|1|0|0|1|255|63|1023|127|127|127|127|1|1|1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1|63|7|1|1023|1|1|1|1|0|0|1|1023|1023|511|511|111111111111111111111111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111|1|0|0|1|255|63|1023|127|127|127|127|1|1|1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1|63|7|1|1023|1|1|1|1|0|0|1|1023|1023|511|511|111111111111111111111111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111|1|0|0|1|255|63|1023|127|127|127|127|1|1|1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1|63|7|1|1023|1|1|1|1|0|0|1|1023|1023|511|511|111111111111111111111111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111|1|0|0|1|255|63|1023|127|127|127|127|1|1|1|1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1|0|0|1"));

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
        public void CtvDecoder_Execute_EquationExpectedData_Pass()
        {
            this.Setup(TestFileEquationExpectedData);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileEquationExpectedData;

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
        public void CtvDecoder_Execute_EquationExpectedData_Fail()
        {
            this.Setup(TestFileEquationExpectedData);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileEquationExpectedData;

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
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("DDR1_DQ50.V0.training.subseq2.result:12|DDR1_DQ50.V0.training.subseq2.result2:0|DDR1_DQ50.V0.training.subseq2.result6:0|DDR1_DQ50.V0.training.subseq2.result7:0|DDR1_DQ50.V0.training.subseq2.result8:1"));

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
        public void CtvDecoder_Execute_CodificationGray_Pass()
        {
            this.Setup(TestFileCodification);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileCodification;

            // input setup
            const string dataPinDDR1DQ50 = "001001001";
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
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("56|7"));

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
        public void CtvDecoder_Execute_OnesComplement_Pass()
        {
            this.Setup(TestFileOnesComplement);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileOnesComplement;

            // input setup
            const string dataPinDDR1DQ50 = "00101010000101000000101110101";
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
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("-4|-5|-8|-1|7|1|5"));

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
        public void CtvDecoder_Execute_TwosComplement_Pass()
        {
            this.Setup(TestFileTwosComplement);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileTwosComplement;

            // input setup
            const string dataPinDDR1DQ50 = "0101000001101111000100001011111100101111";
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
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("10|-10|8|-3|-12"));

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
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_Execute_CodificationNotSupported_Fail()
        {
            this.Setup(TestFileCodificationFail);

            // input setup
            const string dataPinDDR1DQ50 = "1111";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileCodificationFail;

            // Execution & Assert
            this.CustomVerify();
            var toExecute = this as IFunctionalExtensions;
            Assert.ThrowsException<TestMethodException>(() => toExecute.ProcessCtvPerPin(inputCtvData));
            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_Execute_OffsetField_Pass()
        {
            this.Setup(TestFileOffsetField);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileOffsetField;

            // input setup
            const string dataPinDDR1DQ50 = "000100010001";
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
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("8|8|8"));

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
        public void CtvDecoder_Execute_PerBit_Pass()
        {
            this.Setup(TestFilePerBit);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFilePerBit;

            // input setup
            const string dataPinDDR1DQ50 = "000000111";
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
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("000000|111"));

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
        public void CtvDecoder_Execute_PerBit_Fail()
        {
            this.Setup(TestFilePerBit);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFilePerBit;

            // input setup
            const string dataPinDDR1DQ50 = "010000111";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("|Pin1_failbump"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("010000|111"));

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
        public void CtvDecoder_Execute_BaseExpectedData_Pass()
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
        public void CtvDecoder_Execute_BaseExpectedData_Fail()
        {
            this.Setup(TestFileBaseExpectedData);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileBaseExpectedData;

            // input setup
            const string dataPinDDR1DQ50 = "110110001100";
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
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1B|10|001"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("DDR1_DQ50.V0.training.subseq1.val1:1B"));

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
        public void CtvDecoder_Execute_PinFinderFormatCheck_Pass()
        {
            this.Setup(TestFilePinFinderFormatPass);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFilePinFinderFormatPass;

            // input setup
            const string dataPinDDR1DQ50 = "011111111111111111111111111111111111111111111111111111111111111111110111111111111111111111111111111111111111";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("|TX_MDFI_DDR1_DQ50_C1_DQ[0]_failbump"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("|RX_MDFI_DDR1_DQ50_C1_DQ[0]_failbump"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("|TX_MDFI_DDR1_DQ50_C2_DQ[14]_failbump"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("|RX_MDFI_DDR1_DQ50_C2_DQ[14]_failbump"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("011111111111111111111111111111111111111111111111111111|111111111111110111111111111111111111111111111111111111"));

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
        public void CtvDecoder_Execute_StringData_Pass()
        {
            this.Setup(TestFileStringData);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.ConfigurationFile = TestFileStringData;

            // input setup
            const string dataPinDDR1DQ50 = "101";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("Test|5|IP1_Field2|Field0"));

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
        public void CtvDecoder_Execute_StringData_Fail()
        {
            this.Setup(TestFileStringDataFail);

            // input setup
            const string dataPinDDR1DQ50 = "00101";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.ConfigurationFile = TestFileStringDataFail;

            // Execution & Assert
            this.CustomVerify();
            var toExecute = this as IFunctionalExtensions;
            Assert.ThrowsException<TestMethodException>(() => toExecute.ProcessCtvPerPin(inputCtvData));
            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_Execute_FieldParametersPrint_Pass()
        {
            this.Setup(TestFileFieldParametersPrint);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileFieldParametersPrint;

            // input setup
            const string dataPinDDR1DQ50 = "010101";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            this.consoleServiceMock.Setup(x => x.PrintDebug("\n[INFO] DDR1_DQ50 results: Test size: 6 | CTV size: 6\n")).Callback((string s) => Console.WriteLine(s));
            this.consoleServiceMock.Setup(x => x.PrintDebug("Field Name: val1; Path: DDR1_DQ50.V0.training.subseq1.val1; FieldData: 42; StringData: 42; Size: 6; ExpectedData: 42; FailPort: 1.")).Callback((string s) => Console.WriteLine(s));
            Services.ConsoleService = this.consoleServiceMock.Object;

            // Execution & Assert
            this.CustomVerify();
            var toExecute = this as IFunctionalExtensions;
            Assert.AreEqual(true, toExecute.ProcessCtvPerPin(inputCtvData));

            this.consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Execute unit test.
        /// </summary>
        [TestMethod]
        public void CtvDecoder_Execute_TssidMsunitCheck_Pass()
        {
            this.Setup(TestFileTssidMsunitCheck);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.TssidRename = "U1";
            this.ConfigurationFile = TestFileTssidMsunitCheck;

            // input setup
            const string dataPinDDR1DQ50 = "11001100";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));
            /* istrgvalFormat.Setup(x => x.SetMsUnitAttributes(It.IsAny<Unit>())); TODO: Fixme */
            istrgvalFormat.Setup(x => x.SetTssidAttributes("U1"));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_U1_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("3|3"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            /* dataLogServiceMock.Setup(x => x.WriteToItuff("2_msunit_SPMV1://[!MNEMONIC]|//\n2_tssid_U1\n")); */

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
        public void CtvDecoder_Execute_MultipleTssidMsunitCheck_Pass()
        {
            this.Setup(TestFileMultipleTssidMsunitCheck);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50,DDR1_DQ51";
            this.TssidRename = "U1,U2.U3";
            this.ConfigurationFile = TestFileMultipleTssidMsunitCheck;

            // input setup
            const string dataPinDDR1DQ50 = "11001100";
            const string dataPinDDR1DQ51 = "00100010";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
                { "DDR1_DQ51", dataPinDDR1DQ51 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_U1_Token1"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_U2.U3_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("3|3"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("4|4"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            /* istrgvalFormat.Setup(x => x.SetMsUnitAttributes(It.IsAny<Unit>()));  TODO: Fixme */
            istrgvalFormat.Setup(x => x.SetTssidAttributes("U1"));
            istrgvalFormat.Setup(x => x.SetTssidAttributes("U2.U3"));

            /* dataLogServiceMock.Setup(x => x.WriteToItuff("2_msunit_SPMV1://[!MNEMONIC1]|//\n2_tssid_U1\n"));
            dataLogServiceMock.Setup(x => x.WriteToItuff("2_msunit_SPMV1://[!MNEMONIC2]|//\n2_tssid_U2.U3\n")); */

            Services.DatalogService = dataLogServiceMock.Object;

            // Execution & Assert
            this.CustomVerify();
            var toExecute = this as IFunctionalExtensions;
            Assert.AreEqual(true, toExecute.ProcessCtvPerPin(inputCtvData));

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
