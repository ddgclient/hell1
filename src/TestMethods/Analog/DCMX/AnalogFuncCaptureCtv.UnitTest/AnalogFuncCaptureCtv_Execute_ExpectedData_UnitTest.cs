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

namespace AnalogFuncCaptureCtv.UnitTest
{
    using System;
    using System.Collections.Generic;
    using global::AnalogFuncCaptureCtv.ConfigurationFile;
    using global::AnalogFuncCaptureCtv.UnitTest.TestInputFiles;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FileService;
    using Prime.SharedStorageService;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;

    /// <summary>
    /// Unit test class.
    /// </summary>
    [TestClass]
    public class AnalogFuncCaptureCtv_Execute_ExpectedData_UnitTest : AnalogFuncCaptureCtv
    {
        private const string TestFileGenericPass = "TestFileGenericPass.csv";
        private const string TestFileEquationExpectedData = "TestFileEquationExpectedData.csv";
        private const string TestFileCodification = "TestFileCodification.csv";
        private const string TestFilePerBit = "TestFilePerBit.csv";
        private const string TestFileBaseExpectedData = "TestFileBaseExpectedData.csv";
        private const string TestFileBaseExpectedDataFailPort = "TestFileBaseExpectedDataFailPort.csv";
        private const string TestFileBaseExpectedDataFailPort6 = "TestFileBaseExpectedDataFailPort6.csv";

        private Mock<IConsoleService> consoleServiceMock;

        /// <summary>
        /// Execute unit test.
        /// </summary>
        [TestMethod]
        public void AnalogFuncCaptureCtv_Execute_Pass()
        {
            this.Setup(TestFileGenericPass);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50,DDR1_DQ51";
            this.Kill = AnalogFuncCaptureCtv.EnabledDisabled.DISABLED;
            this.PinRename = "DDR1_DQ50,DDR1_DQ51";
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

            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_edc"));
            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token2_edc"));
            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token3_edc"));
            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token4_edc"));
            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ51_FSM_Status_edc"));
            // JF PTH edit
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token2"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token3"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token4"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_FSM_Status"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("63|7|1|1023|1|1|1|1|0|0|1|1023|1023|1022|511|481|111111111111111111111111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111|1|0|0|1|255|63|1023|127|127|127|127|1|1|1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1|63|7|1|1023|1|1|1|1|0|0|1|1023|1023|511|511|111111111111111111111111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111|1|0|0|1|255|63|1023|127|127|127|127|1|1|1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1|63|7|1|1023|1|1|1|1|0|0|1|1023|1023|511|511|111111111111111111111111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111|1|0|0|1|255|63|1023|127|127|127|127|1|1|1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1|63|7|1|1023|1|1|1|1|0|0|1|1023|1023|511|511|111111111111111111111111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111|111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111111111111|1111111111111111111111111111111111111111111111111111111111111111111111111|1|0|0|1|255|63|1023|127|127|127|127|1|1|1|1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1|0|0|1"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Prime.Services.DatalogService = dataLogServiceMock.Object;

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
        public void AnalogFuncCaptureCtv_Execute_EquationExpectedData_Pass()
        {
            this.Setup(TestFileEquationExpectedData);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.Kill = AnalogFuncCaptureCtv.EnabledDisabled.DISABLED;
            this.PinRename = "DDR1_DQ50";
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

            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_edc"));
            // JF PTH edit
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("36|3|108"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Prime.Services.DatalogService = dataLogServiceMock.Object;

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
        public void AnalogFuncCaptureCtv_Execute_EquationExpectedData_Fail()
        {
            this.Setup(TestFileEquationExpectedData);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.Kill = AnalogFuncCaptureCtv.EnabledDisabled.DISABLED;
            this.PinRename = "DDR1_DQ50";
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

            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_edc"));
            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_fc"));
            // JF PTH edit
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("4|3|12"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("DDR1_DQ50.V0.training.subseq2.result:12"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Prime.Services.DatalogService = dataLogServiceMock.Object;

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
        public void AnalogFuncCaptureCtv_Execute_CodificationGray_Pass()
        {
            this.Setup(TestFileCodification);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.Kill = AnalogFuncCaptureCtv.EnabledDisabled.DISABLED;
            this.PinRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileCodification;

            // input setup
            const string dataPinDDR1DQ50 = "001001001100100";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();

            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_edc"));
            // JF PTH edit
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("56|7|-2|1"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Prime.Services.DatalogService = dataLogServiceMock.Object;

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
        public void AnalogFuncCaptureCtv_Execute_PerBit_Pass()
        {
            this.Setup(TestFilePerBit);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.Kill = AnalogFuncCaptureCtv.EnabledDisabled.DISABLED;
            this.PinRename = "DDR1_DQ50";
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

            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_edc"));
            // JF PTH edit
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("000000|111"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Prime.Services.DatalogService = dataLogServiceMock.Object;

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
        public void AnalogFuncCaptureCtv_Execute_PerBit_Fail()
        {
            this.Setup(TestFilePerBit);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.Kill = AnalogFuncCaptureCtv.EnabledDisabled.DISABLED;
            this.PinRename = "DDR1_DQ50";
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

            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_edc"));
            // JF PTH edit
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("010000|111"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Prime.Services.DatalogService = dataLogServiceMock.Object;

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
        public void AnalogFuncCaptureCtv_Execute_BaseExpectedData_Pass()
        {
            this.Setup(TestFileBaseExpectedData);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.Kill = AnalogFuncCaptureCtv.EnabledDisabled.DISABLED;
            this.PinRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileBaseExpectedData;

            // input setup
            const string dataPinDDR1DQ50 = "110100001100100";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();

            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_edc"));
            // JF PTH edit
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("0B|10|001|1"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Prime.Services.DatalogService = dataLogServiceMock.Object;

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
        public void AnalogFuncCaptureCtv_Execute_BaseExpectedData_Fail()
        {
            this.Setup(TestFileBaseExpectedData);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.Kill = AnalogFuncCaptureCtv.EnabledDisabled.DISABLED;
            this.PinRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileBaseExpectedData;

            // input setup
            const string dataPinDDR1DQ50 = "110110001100100";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();

            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_edc"));
            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_fc"));
            // JF PTH edit
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1B|10|001|1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("DDR1_DQ50.V0.training.subseq1.val1:1B"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Prime.Services.DatalogService = dataLogServiceMock.Object;

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
        public void AnalogFuncCaptureCtv_Execute_BaseExpectedDataFailPort_Fail()
        {
            this.Setup(TestFileBaseExpectedDataFailPort);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.Kill = AnalogFuncCaptureCtv.EnabledDisabled.DISABLED;
            this.PinRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileBaseExpectedDataFailPort;

            // input setup
            const string dataPinDDR1DQ50 = "110110001100100";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();

            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_edc"));
            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_fc"));
            // JF PTH edit
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1B|10|001|1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("DDR1_DQ50.V0.training.subseq1.val1:1B"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("DDR1_DQ50.V0.training.subseq1.val4:1"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Prime.Services.DatalogService = dataLogServiceMock.Object;

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
        public void AnalogFuncCaptureCtv_Execute_BaseExpectedDataFailPort6_Fail()
        {
            this.Setup(TestFileBaseExpectedDataFailPort6);

            // Configuration
            this.CtvCapturePins = "DDR1_DQ50";
            this.Kill = AnalogFuncCaptureCtv.EnabledDisabled.DISABLED;
            this.PinRename = "DDR1_DQ50";
            this.ConfigurationFile = TestFileBaseExpectedDataFailPort6;

            // input setup
            const string dataPinDDR1DQ50 = "110110001100100";
            var inputCtvData = new Dictionary<string, string>(1)
            {
                { "DDR1_DQ50", dataPinDDR1DQ50 },
            };

            // Ituff Mocks
            var istrgvalFormat = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            istrgvalFormat.Setup(x => x.SetDelimiterCharacterForWrap('|'));

            var setTnamePostfixSequence = new MockSequence();

            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_edc"));
            // istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_DDR1_DQ50_Token1_fc"));
            // JF PTH edit
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));
            istrgvalFormat.InSequence(setTnamePostfixSequence).Setup(x => x.SetTnamePostfix("_Token1"));

            var setDataSequence = new MockSequence();
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("1B|10|001|1"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("DDR1_DQ50.V0.training.subseq1.val1:1B"));
            istrgvalFormat.InSequence(setDataSequence).Setup(x => x.SetData("DDR1_DQ50.V0.training.subseq1.val4:1"));

            var dataLogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            dataLogServiceMock.Setup(x => x.WriteToItuff(It.IsAny<IStrgvalFormat>()));
            dataLogServiceMock.Setup(x => x.GetItuffStrgvalWriter()).Returns(istrgvalFormat.Object);

            Prime.Services.DatalogService = dataLogServiceMock.Object;

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
            this.FileHandler = fileHandlerMock.Object;

            var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            fileServiceMock.Setup(x => x.GetFile(fileName))
                .Returns(fileName);
            Prime.Services.FileService = fileServiceMock.Object;

            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(x => x.PrintDebug("[INFO] This is the InputFile: " + fileName + " " + fileName));
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;
        }
    }
}
