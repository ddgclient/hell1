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

namespace ConcurrentPlistTracer.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.TestMethods.Functional;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class ConcurrentPlistTracer_UnitTest
    {
        /// <summary>
        /// Default initialization.
        /// </summary>
        [TestInitialize]
        public void Initialization()
        {
            // Default Mock for ConsoleService
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) => Console.WriteLine($"DEBUG: {msg}"));
            consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_Pass()
        {
            var funcTestMock = new Mock<ICaptureCtvPerCycleTest>(MockBehavior.Strict);
            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureCtvPerCycleTest("MyPatList", "MyLevels", "MyTiming", new List<string> { "CapturePin" }, string.Empty)).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var patConfigAddCtvMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patConfigAddCtvMock.Setup(o => o.FillData(PatternSymbol.ONE));

            var patConfigRemoveCtvMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patConfigRemoveCtvMock.Setup(o => o.FillData(PatternSymbol.ZERO));

            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.SetupSequence(o => o.GetPatConfigHandleWithPlist("CtvPatConfig", "MyPatList"))
                .Returns(patConfigAddCtvMock.Object)
                .Returns(patConfigRemoveCtvMock.Object);
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var underTest = new ConcurrentPlistTracer
            {
                // base FunctionalTest parameters.
                Patlist = "MyPatList",
                TimingsTc = "MyTiming",
                LevelsTc = "MyLevels",
                CtvCapturePins = "CapturePin",

                // required ConcurrentPlistTracer parameters.
                PatConfigForCtv = "CtvPatConfig",
                CtvCapturePerCycleMode = PrimeFunctionalTestMethod.CtvCycleCaptureMode.ENABLED,
            };

            underTest.Verify();
            underTest.CustomVerify();
            funcServiceMock.VerifyAll();
            funcTestMock.VerifyAll();
            patConfigServiceMock.VerifyAll();
            patConfigAddCtvMock.VerifyAll();
            patConfigRemoveCtvMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_Pass()
        {
            var ctvByCycle1Mock = new Mock<ICtvPerCycle>(MockBehavior.Strict);
            ctvByCycle1Mock.Setup(o => o.GetDomainName()).Returns("LEG");
            ctvByCycle1Mock.Setup(o => o.GetPatternName()).Returns("PatternA");
            ctvByCycle1Mock.Setup(o => o.GetParentPlistName()).Returns("PListA");
            ctvByCycle1Mock.Setup(o => o.GetVectorAddress()).Returns(2);
            ctvByCycle1Mock.Setup(o => o.GetCycle()).Returns(2);
            ctvByCycle1Mock.Setup(o => o.GetTraceLogRegister1()).Returns(0);
            ctvByCycle1Mock.Setup(o => o.GetTraceLogCycle()).Returns(2);
            ctvByCycle1Mock.Setup(o => o.GetBurstIndex()).Returns(0);
            ctvByCycle1Mock.Setup(o => o.GetBurstCycle()).Returns(2);

            var ctvByCycle2Mock = new Mock<ICtvPerCycle>(MockBehavior.Strict);
            ctvByCycle2Mock.Setup(o => o.GetDomainName()).Returns("LEG");
            ctvByCycle2Mock.Setup(o => o.GetPatternName()).Returns("PatternB");
            ctvByCycle2Mock.Setup(o => o.GetParentPlistName()).Returns("PListB");
            ctvByCycle2Mock.Setup(o => o.GetVectorAddress()).Returns(2);
            ctvByCycle2Mock.Setup(o => o.GetCycle()).Returns(2);
            ctvByCycle2Mock.Setup(o => o.GetTraceLogRegister1()).Returns(1);
            ctvByCycle2Mock.Setup(o => o.GetTraceLogCycle()).Returns(2);
            ctvByCycle2Mock.Setup(o => o.GetBurstIndex()).Returns(0);
            ctvByCycle2Mock.Setup(o => o.GetBurstCycle()).Returns(2);

            var ctvByCycle3Mock = new Mock<ICtvPerCycle>(MockBehavior.Strict);
            ctvByCycle3Mock.Setup(o => o.GetDomainName()).Returns("LEG");
            ctvByCycle3Mock.Setup(o => o.GetPatternName()).Returns("PatternA");
            ctvByCycle3Mock.Setup(o => o.GetParentPlistName()).Returns("PListA");
            ctvByCycle3Mock.Setup(o => o.GetVectorAddress()).Returns(2000);
            ctvByCycle3Mock.Setup(o => o.GetCycle()).Returns(40000);
            ctvByCycle3Mock.Setup(o => o.GetTraceLogRegister1()).Returns(0);
            ctvByCycle3Mock.Setup(o => o.GetTraceLogCycle()).Returns(40000);
            ctvByCycle3Mock.Setup(o => o.GetBurstIndex()).Returns(0);
            ctvByCycle3Mock.Setup(o => o.GetBurstCycle()).Returns(40000);

            var funcTestMock = new Mock<ICaptureCtvPerCycleTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.ApplyTestConditions());
            funcTestMock.Setup(o => o.SetPinMask(new List<string>()));
            funcTestMock.Setup(o => o.Execute()).Returns(true);
            funcTestMock.Setup(o => o.GetCtvPerCycle()).Returns(new List<ICtvPerCycle> { ctvByCycle1Mock.Object, ctvByCycle2Mock.Object, ctvByCycle3Mock.Object });

            var funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            funcServiceMock.Setup(o => o.CreateCaptureCtvPerCycleTest("MyPatList", "MyLevels", "MyTiming", new List<string> { "CapturePin" }, string.Empty)).Returns(funcTestMock.Object);
            Prime.Services.FunctionalService = funcServiceMock.Object;

            var patConfigAddCtvMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patConfigAddCtvMock.Setup(o => o.FillData(PatternSymbol.ONE));

            var patConfigRemoveCtvMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patConfigRemoveCtvMock.Setup(o => o.FillData(PatternSymbol.ZERO));

            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock.SetupSequence(o => o.GetPatConfigHandleWithPlist("CtvPatConfig", "MyPatList"))
                .Returns(patConfigAddCtvMock.Object)
                .Returns(patConfigRemoveCtvMock.Object);
            var sequence = new MockSequence();
            patConfigServiceMock.InSequence(sequence).Setup(o => o.Apply(patConfigAddCtvMock.Object));
            patConfigServiceMock.InSequence(sequence).Setup(o => o.Apply(patConfigRemoveCtvMock.Object));
            Prime.Services.PatConfigService = patConfigServiceMock.Object;

            var underTest = new ConcurrentPlistTracer
            {
                // base FunctionalTest parameters.
                Patlist = "MyPatList",
                TimingsTc = "MyTiming",
                LevelsTc = "MyLevels",
                CtvCapturePins = "CapturePin",

                // required ConcurrentPlistTracer parameters.
                PatConfigForCtv = "CtvPatConfig",
                CtvCapturePerCycleMode = PrimeFunctionalTestMethod.CtvCycleCaptureMode.ENABLED,
            };

            underTest.TestMethodExtension = underTest;
            underTest.Verify();
            underTest.CustomVerify();
            Assert.AreEqual(1, underTest.Execute());

            funcServiceMock.VerifyAll();
            funcTestMock.VerifyAll();
            patConfigServiceMock.VerifyAll();
            patConfigServiceMock.Verify(o => o.Apply(patConfigAddCtvMock.Object), Times.Once);
            patConfigServiceMock.Verify(o => o.Apply(patConfigRemoveCtvMock.Object), Times.Once);
            patConfigAddCtvMock.VerifyAll();
            patConfigRemoveCtvMock.VerifyAll();
            ctvByCycle1Mock.VerifyAll();
            ctvByCycle2Mock.VerifyAll();
            ctvByCycle3Mock.VerifyAll();
        }
    }
}
