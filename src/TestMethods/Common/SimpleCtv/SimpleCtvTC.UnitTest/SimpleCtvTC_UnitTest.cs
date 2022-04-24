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

namespace SimpleCtvTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FunctionalService;
    using Prime.TestConditionService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SimpleCtvTC_UnitTest
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IFunctionalService> functionalService;
        private Mock<ITestConditionService> testConditionService;
        private Mock<IDatalogService> datalogService;
        private Mock<IMrsltFormat> ituffWriter;

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(
                o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
            {
                System.Console.WriteLine($"DEBUG: {msg}");
            });
            this.consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            this.functionalService = new Mock<IFunctionalService>(MockBehavior.Strict);
            Prime.Services.FunctionalService = this.functionalService.Object;

            this.testConditionService = new Mock<ITestConditionService>(MockBehavior.Strict);
            Prime.Services.TestConditionService = this.testConditionService.Object;

            this.ituffWriter = new Mock<IMrsltFormat>(MockBehavior.Strict);
            this.datalogService = new Mock<IDatalogService>(MockBehavior.Strict);
            this.datalogService.Setup(d => d.GetItuffMrsltWriter()).Returns(this.ituffWriter.Object);
            Prime.Services.DatalogService = this.datalogService.Object;
        }

        /// <summary>
        /// SimplePin test. Print only.
        /// </summary>
        [TestMethod]
        public void SinglePin_NoLimitsPrint_Pass()
        {
            var underTest = new SimpleCtvTC
            {
                Patlist = "SomePatlist",
                LevelsTc = "SomeLevels",
                TimingsTc = "SomeTimings",
                CtvCapturePins = "SomePin",
                Registers = "--registers reg1:11-0 reg2:12,13,14",
                Print = "--registers reg1 reg2",
            };

            var captureData = new Dictionary<string, string> { { "SomePin", "111010101100110" } };
            var functionalTest = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            functionalTest.Setup(f => f.ApplyTestConditions());
            functionalTest.Setup(f => f.GetCtvData()).Returns(captureData);
            functionalTest.Setup(f => f.Execute()).Returns(true);
            functionalTest.Setup(f => f.SetPinMask(new List<string>()));
            this.functionalService.Setup(f => f.CreateCaptureCtvPerPinTest(underTest.Patlist, underTest.LevelsTc, underTest.TimingsTc, new List<string> { "SomePin" }, underTest.PrePlist)).Returns(functionalTest.Object);

            this.ituffWriter.Setup(i => i.SetTnamePostfix("_reg1"));
            this.ituffWriter.Setup(i => i.SetTnamePostfix("_reg2"));
            this.ituffWriter.Setup(i => i.SetData(855, -1, -1));
            this.ituffWriter.Setup(i => i.SetData(6, -1, -1));
            this.datalogService.Setup(d => d.WriteToItuff(this.ituffWriter.Object));

            underTest.Verify();
            underTest.CustomVerify();
            underTest.TestMethodExtension = underTest;
            var executeResult = underTest.Execute();
            Assert.AreEqual(1, executeResult);

            functionalTest.VerifyAll();
            this.functionalService.VerifyAll();
            this.ituffWriter.VerifyAll();
            this.datalogService.VerifyAll();
        }

        /// <summary>
        /// SimplePin test. Failed not equal.
        /// </summary>
        [TestMethod]
        public void SinglePin_FailedNotEqual_Port4()
        {
            var underTest = new SimpleCtvTC
            {
                Patlist = "SomePatlist",
                LevelsTc = "SomeLevels",
                TimingsTc = "SomeTimings",
                CtvCapturePins = "SomePin",
                Registers = "--registers reg1:11-0 reg2:12,13,14",
                Print = "--registers reg1 reg2",
                Limits = "--ne reg1:2 reg2:10",
            };

            var captureData = new Dictionary<string, string> { { "SomePin", "111010101100110" } };
            var functionalTest = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            functionalTest.Setup(f => f.ApplyTestConditions());
            functionalTest.Setup(f => f.GetCtvData()).Returns(captureData);
            functionalTest.Setup(f => f.Execute()).Returns(true);
            functionalTest.Setup(o => o.SetPinMask(new List<string>()));
            this.functionalService.Setup(f => f.CreateCaptureCtvPerPinTest(underTest.Patlist, underTest.LevelsTc, underTest.TimingsTc, new List<string> { "SomePin" }, underTest.PrePlist)).Returns(functionalTest.Object);

            this.ituffWriter.Setup(i => i.SetTnamePostfix("_reg1"));
            this.ituffWriter.Setup(i => i.SetTnamePostfix("_reg2"));
            this.ituffWriter.Setup(i => i.SetData(855, 2, 2));
            this.ituffWriter.Setup(i => i.SetData(6, 10, 10));
            this.datalogService.Setup(d => d.WriteToItuff(this.ituffWriter.Object));

            underTest.Verify();
            underTest.CustomVerify();
            underTest.TestMethodExtension = underTest;
            var executeResult = underTest.Execute();
            Assert.AreEqual(4, executeResult);

            functionalTest.VerifyAll();
            this.functionalService.VerifyAll();
            this.ituffWriter.VerifyAll();
            this.datalogService.VerifyAll();
        }

        /// <summary>
        /// SimplePin test. Failed high and low.
        /// </summary>
        [TestMethod]
        public void SinglePin_FailedHighAndLow_Port4()
        {
            var underTest = new SimpleCtvTC
            {
                Patlist = "SomePatlist",
                LevelsTc = "SomeLevels",
                TimingsTc = "SomeTimings",
                CtvCapturePins = "SomePin",
                Registers = "--registers reg1:11-0 reg2:12,13,14",
                Print = "--registers reg1 reg2",
                Limits = "--high reg1:2 reg2:10 --low reg1:0 reg2:7",
            };

            var captureData = new Dictionary<string, string> { { "SomePin", "111010101100110" } };
            var functionalTest = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            functionalTest.Setup(f => f.ApplyTestConditions());
            functionalTest.Setup(f => f.GetCtvData()).Returns(captureData);
            functionalTest.Setup(f => f.Execute()).Returns(true);
            functionalTest.Setup(f => f.SetPinMask(new List<string>()));
            this.functionalService.Setup(f => f.CreateCaptureCtvPerPinTest(underTest.Patlist, underTest.LevelsTc, underTest.TimingsTc, new List<string> { "SomePin" }, underTest.PrePlist)).Returns(functionalTest.Object);

            this.ituffWriter.Setup(i => i.SetTnamePostfix("_reg1"));
            this.ituffWriter.Setup(i => i.SetTnamePostfix("_reg2"));
            this.ituffWriter.Setup(i => i.SetData(855, 0, 2));
            this.ituffWriter.Setup(i => i.SetData(6, 7, 10));
            this.datalogService.Setup(d => d.WriteToItuff(this.ituffWriter.Object));

            underTest.Verify();
            underTest.CustomVerify();
            underTest.TestMethodExtension = underTest;
            var executeResult = underTest.Execute();
            Assert.AreEqual(4, executeResult);

            functionalTest.VerifyAll();
            this.functionalService.VerifyAll();
            this.ituffWriter.VerifyAll();
            this.datalogService.VerifyAll();
        }

        /// <summary>
        /// SimplePin test. Failed Low.
        /// </summary>
        [TestMethod]
        public void SinglePin_FailedLow_Port3()
        {
            var underTest = new SimpleCtvTC
            {
                Patlist = "SomePatlist",
                LevelsTc = "SomeLevels",
                TimingsTc = "SomeTimings",
                CtvCapturePins = "SomePin",
                Registers = "--registers reg1:11-0 reg2:12,13,14",
                Print = string.Empty,
                Limits = "--high reg2:10 --low reg1:856",
            };

            var captureData = new Dictionary<string, string> { { "SomePin", "111010101100110" } };
            var functionalTest = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            functionalTest.Setup(f => f.ApplyTestConditions());
            functionalTest.Setup(f => f.GetCtvData()).Returns(captureData);
            functionalTest.Setup(f => f.Execute()).Returns(true);
            functionalTest.Setup(f => f.SetPinMask(new List<string>()));
            this.functionalService.Setup(f => f.CreateCaptureCtvPerPinTest(underTest.Patlist, underTest.LevelsTc, underTest.TimingsTc, new List<string> { "SomePin" }, underTest.PrePlist)).Returns(functionalTest.Object);

            underTest.Verify();
            underTest.CustomVerify();
            underTest.TestMethodExtension = underTest;
            var executeResult = underTest.Execute();
            Assert.AreEqual(3, executeResult);

            functionalTest.VerifyAll();
            this.functionalService.VerifyAll();
        }

        /// <summary>
        /// Invalid register.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SinglePin_ParseError1_Fail()
        {
            var underTest = new SimpleCtvTC
            {
                Patlist = "SomePatlist",
                LevelsTc = "SomeLevels",
                TimingsTc = "SomeTimings",
                CtvCapturePins = "SomePin",
                Registers = "--registers reg1:11-0:99 reg2:12,13,14",
                Print = string.Empty,
                Limits = "--high reg2:10 --low reg1:856",
            };

            var captureData = new Dictionary<string, string> { { "SomePin", "111010101100110" } };
            var functionalTest = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            functionalTest.Setup(f => f.ApplyTestConditions());
            functionalTest.Setup(f => f.GetCtvData()).Returns(captureData);
            functionalTest.Setup(f => f.Execute()).Returns(true);
            this.functionalService.Setup(f => f.CreateCaptureCtvPerPinTest(underTest.Patlist, underTest.LevelsTc, underTest.TimingsTc, new List<string> { "SomePin" }, underTest.PrePlist)).Returns(functionalTest.Object);

            underTest.Verify();
            underTest.CustomVerify();
        }

        /// <summary>
        /// Invalid command line.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SinglePin_ParseError2_Fail()
        {
            var underTest = new SimpleCtvTC
            {
                Patlist = "SomePatlist",
                LevelsTc = "SomeLevels",
                TimingsTc = "SomeTimings",
                CtvCapturePins = "SomePin",
                Registers = "--registers reg1:11-0 reg2:12,13,14",
                Print = string.Empty,
                Limits = "--blah reg2:10 --blah reg1:856",
            };

            var captureData = new Dictionary<string, string> { { "SomePin", "111010101100110" } };
            var functionalTest = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            functionalTest.Setup(f => f.ApplyTestConditions());
            functionalTest.Setup(f => f.GetCtvData()).Returns(captureData);
            functionalTest.Setup(f => f.Execute()).Returns(true);
            this.functionalService.Setup(f => f.CreateCaptureCtvPerPinTest(underTest.Patlist, underTest.LevelsTc, underTest.TimingsTc, new List<string> { "SomePin" }, underTest.PrePlist)).Returns(functionalTest.Object);

            underTest.Verify();
            underTest.CustomVerify();
        }

        /// <summary>
        /// Invalid command line.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SinglePin_ParseError3_Fail()
        {
            var underTest = new SimpleCtvTC
            {
                Patlist = "SomePatlist",
                LevelsTc = "SomeLevels",
                TimingsTc = "SomeTimings",
                CtvCapturePins = "SomePin",
                Registers = "--registers reg1:11-0 reg2:12,13,14",
                Print = string.Empty,
                Limits = "--high reg2:10:9 --low reg1:856:1",
            };

            var captureData = new Dictionary<string, string> { { "SomePin", "111010101100110" } };
            var functionalTest = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            functionalTest.Setup(f => f.ApplyTestConditions());
            functionalTest.Setup(f => f.GetCtvData()).Returns(captureData);
            functionalTest.Setup(f => f.Execute()).Returns(true);
            this.functionalService.Setup(f => f.CreateCaptureCtvPerPinTest(underTest.Patlist, underTest.LevelsTc, underTest.TimingsTc, new List<string> { "SomePin" }, underTest.PrePlist)).Returns(functionalTest.Object);

            underTest.Verify();
            underTest.CustomVerify();
        }

        /// <summary>
        /// Invalid command line.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SinglePin_ParseError4_Fail()
        {
            var underTest = new SimpleCtvTC
            {
                Patlist = "SomePatlist",
                LevelsTc = "SomeLevels",
                TimingsTc = "SomeTimings",
                CtvCapturePins = "SomePin",
                Registers = "--registr reg1:11-0 reg2:12,13,14",
                Print = string.Empty,
                Limits = "--high reg2:10 --low reg1:856",
            };

            var captureData = new Dictionary<string, string> { { "SomePin", "111010101100110" } };
            var functionalTest = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            functionalTest.Setup(f => f.ApplyTestConditions());
            functionalTest.Setup(f => f.GetCtvData()).Returns(captureData);
            functionalTest.Setup(f => f.Execute()).Returns(true);
            this.functionalService.Setup(f => f.CreateCaptureCtvPerPinTest(underTest.Patlist, underTest.LevelsTc, underTest.TimingsTc, new List<string> { "SomePin" }, underTest.PrePlist)).Returns(functionalTest.Object);

            underTest.Verify();
            underTest.CustomVerify();
        }

        /// <summary>
        /// Invalid command line.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SinglePin_ParseError5_Fail()
        {
            var underTest = new SimpleCtvTC
            {
                Patlist = "SomePatlist",
                LevelsTc = "SomeLevels",
                TimingsTc = "SomeTimings",
                CtvCapturePins = "SomePin",
                Registers = "--registers reg1:11-0 reg2:12,13,14",
                Print = "invalid value",
                Limits = "--high reg2:10 --low reg1:856",
            };

            var captureData = new Dictionary<string, string> { { "SomePin", "111010101100110" } };
            var functionalTest = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            functionalTest.Setup(f => f.ApplyTestConditions());
            functionalTest.Setup(f => f.GetCtvData()).Returns(captureData);
            functionalTest.Setup(f => f.Execute()).Returns(true);
            this.functionalService.Setup(f => f.CreateCaptureCtvPerPinTest(underTest.Patlist, underTest.LevelsTc, underTest.TimingsTc, new List<string> { "SomePin" }, underTest.PrePlist)).Returns(functionalTest.Object);

            underTest.Verify();
            underTest.CustomVerify();
        }
    }
}