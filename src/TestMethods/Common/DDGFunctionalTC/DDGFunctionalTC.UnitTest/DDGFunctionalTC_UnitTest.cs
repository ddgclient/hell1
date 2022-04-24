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

namespace DDGFunctionalTC.UnitTest
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
    using Prime.PlistService;
    using Prime.SharedStorageService;
    using Prime.TestConditionService;
    using Prime.TestMethods.Functional;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DDGFunctionalTC_UnitTest : DDGFunctionalTC
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IFunctionalService> functionalService;
        private Mock<ITestConditionService> testConditionService;
        private Mock<IDatalogService> datalogService;
        private Mock<IStrgvalFormat> ituffWriter;
        private Mock<IPlistService> plistService;
        private Mock<IPlistObject> plistObjectMock;
        private Mock<ITestCondition> testCondition;
        private Mock<ISharedStorageService> sharedStorageServiceMock;

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
            this.consoleServiceMock.Setup(o =>
                    o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            this.functionalService = new Mock<IFunctionalService>(MockBehavior.Strict);
            Prime.Services.FunctionalService = this.functionalService.Object;

            this.testConditionService = new Mock<ITestConditionService>(MockBehavior.Strict);
            Prime.Services.TestConditionService = this.testConditionService.Object;

            this.testCondition = new Mock<ITestCondition>(MockBehavior.Strict);

            this.plistService = new Mock<IPlistService>(MockBehavior.Strict);
            Prime.Services.PlistService = this.plistService.Object;

            this.plistObjectMock = new Mock<IPlistObject>(MockBehavior.Strict);
            this.plistService.Setup(o => o.GetPlistObject(It.IsAny<string>()))
                .Returns(this.plistObjectMock.Object);

            this.ituffWriter = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            this.datalogService = new Mock<IDatalogService>(MockBehavior.Strict);
            this.datalogService.Setup(d => d.GetItuffStrgvalWriter()).Returns(this.ituffWriter.Object);
            Prime.Services.DatalogService = this.datalogService.Object;

            this.sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = this.sharedStorageServiceMock.Object;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Full_PrintPreviousLabelDisabled_Port1()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;
            this.PrintPreviousLabel = OnOffMode.DISABLED;
            this.FailuresToCaptureTotal = 1;

            this.TestMethodExtension = (IFunctionalExtensions)this;
            var failureTest = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.functionalService
                .Setup(o => o.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 1, this.PrePlist))
                .Returns(failureTest.Object);
            failureTest.Setup(o => o.ApplyTestConditions());
            failureTest.Setup(o => o.Execute()).Returns(true);
            failureTest.Setup(o => o.SetPinMask(new List<string>()));
            failureTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData>());

            this.Verify();
            this.CustomVerify();
            var result = this.Execute();
            Assert.AreEqual(1, result);
            Assert.AreEqual(1, this.ExitPort);
            this.functionalService.VerifyAll();
            failureTest.VerifyAll();
            this.testConditionService.VerifyAll();
            this.testCondition.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Full_CaptureDataTokens_Port1()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;
            this.PrintPreviousLabel = OnOffMode.DISABLED;
            this.FailuresToCaptureTotal = 0;
            this.CtvCapturePins = "TDO";
            this.CapturedDataTokens = "Token1";

            this.TestMethodExtension = (IFunctionalExtensions)this;
            var ctvTest = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            this.functionalService
                .Setup(o => o.CreateCaptureCtvPerPinTest(this.Patlist, this.LevelsTc, this.TimingsTc, new List<string> { "TDO" }, this.PrePlist))
                .Returns(ctvTest.Object);
            ctvTest.Setup(o => o.ApplyTestConditions());
            ctvTest.Setup(o => o.Execute()).Returns(true);
            ctvTest.Setup(o => o.SetPinMask(new List<string>()));
            ctvTest.Setup(o => o.GetCtvData()).Returns(new Dictionary<string, string> { { "TDO", "1100" } });
            this.sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("Token1", "1100", Context.DUT));

            this.Verify();
            this.CustomVerify();
            var result = this.Execute();
            Assert.AreEqual(1, result);
            Assert.AreEqual(1, this.ExitPort);
            this.functionalService.VerifyAll();
            ctvTest.VerifyAll();
            this.testConditionService.VerifyAll();
            this.testCondition.VerifyAll();
            this.sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Full_CaptureDataTokensNotMatching_Port0()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;
            this.PrintPreviousLabel = OnOffMode.DISABLED;
            this.FailuresToCaptureTotal = 0;
            this.CtvCapturePins = "TDO";
            this.CapturedDataTokens = "Token1,Token2";

            this.TestMethodExtension = (IFunctionalExtensions)this;
            var ctvTest = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            this.functionalService
                .Setup(o => o.CreateCaptureCtvPerPinTest(this.Patlist, this.LevelsTc, this.TimingsTc, new List<string> { "TDO" }, this.PrePlist))
                .Returns(ctvTest.Object);

            this.Verify();
            var ex = Assert.ThrowsException<ArgumentException>(this.CustomVerify);
            Assert.AreEqual("Number of CapturedDataTokens must match number of CtvCapturePins.", ex.Message);

            this.functionalService.VerifyAll();
            ctvTest.VerifyAll();
            this.testConditionService.VerifyAll();
            this.testCondition.VerifyAll();
            this.sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Full_PrintPreviousLabelDisabled_Port0()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;
            this.PrintPreviousLabel = OnOffMode.DISABLED;
            this.FailuresToCaptureTotal = 1;

            this.TestMethodExtension = (IFunctionalExtensions)this;
            var failureTest = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.functionalService
                .Setup(o => o.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 1, this.PrePlist))
                .Returns(failureTest.Object);
            failureTest.Setup(o => o.ApplyTestConditions());
            failureTest.Setup(o => o.Execute()).Returns(true);
            failureTest.Setup(o => o.SetPinMask(new List<string>()));
            var failData = new Mock<IFailureData>(MockBehavior.Strict);
            failData.Setup(o => o.GetPatternName()).Returns("FailingPattern");
            failData.Setup(o => o.GetDomainName()).Returns("Domain");
            failData.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "FailingPin" });
            failData.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 1001 });
            failData.Setup(o => o.GetVectorAddress()).Returns(2002);
            failData.Setup(o => o.GetCycle()).Returns(4004);
            this.plistObjectMock.Setup(o => o.IsPatternAnAmble("FailingPattern")).Returns(false);
            failureTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failData.Object });

            this.Verify();
            this.CustomVerify();
            var result = this.Execute();
            Assert.AreEqual(0, result);
            Assert.AreEqual(0, this.ExitPort);
            this.functionalService.VerifyAll();
            failureTest.VerifyAll();
            this.testConditionService.VerifyAll();
            this.testCondition.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Full_PrintPreviousLabelEnabled_Port0()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;
            this.PrintPreviousLabel = OnOffMode.ENABLED;
            this.FailuresToCaptureTotal = 1;
            this.MaxFailuresToItuff = 1;

            this.TestMethodExtension = (IFunctionalExtensions)this;
            var failureTest = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.functionalService
                .Setup(o => o.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 1, this.PrePlist))
                .Returns(failureTest.Object);
            failureTest.Setup(o => o.ApplyTestConditions());
            failureTest.Setup(o => o.Execute()).Returns(true);
            failureTest.Setup(o => o.SetPinMask(new List<string>()));
            failureTest.Setup(o => o.DatalogFailure(1, 0));
            var failData = new Mock<IFailureData>(MockBehavior.Strict);
            failData.Setup(o => o.GetPatternName()).Returns("FailingPattern");
            failData.Setup(o => o.GetDomainName()).Returns("Domain");
            failData.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "FailingPin" });
            failData.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 1001 });
            failData.Setup(o => o.GetVectorAddress()).Returns(2002);
            failData.Setup(o => o.GetCycle()).Returns(4004);
            failData.Setup(o => o.GetPreviousLabel()).Returns("SomeLabel");
            this.plistObjectMock.Setup(o => o.IsPatternAnAmble("FailingPattern")).Returns(false);
            failureTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failData.Object });
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_lb"));
            this.ituffWriter.Setup(o => o.SetData("SomeLabel"));
            this.datalogService.Setup(o => o.WriteToItuff(this.ituffWriter.Object));

            this.Verify();
            this.CustomVerify();
            var result = this.Execute();
            Assert.AreEqual(0, result);
            Assert.AreEqual(0, this.ExitPort);
            this.functionalService.VerifyAll();
            failureTest.VerifyAll();
            this.testConditionService.VerifyAll();
            this.testCondition.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Full_PrintPreviousLabelEnabledNA_Port0()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;
            this.PrintPreviousLabel = OnOffMode.ENABLED;
            this.FailuresToCaptureTotal = 1;
            this.MaxFailuresToItuff = 1;

            this.TestMethodExtension = (IFunctionalExtensions)this;
            var failureTest = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.functionalService
                .Setup(o => o.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 1, this.PrePlist))
                .Returns(failureTest.Object);
            failureTest.Setup(o => o.ApplyTestConditions());
            failureTest.Setup(o => o.Execute()).Returns(true);
            failureTest.Setup(o => o.SetPinMask(new List<string>()));
            failureTest.Setup(o => o.DatalogFailure(1, 0));
            var failData = new Mock<IFailureData>(MockBehavior.Strict);
            failData.Setup(o => o.GetPatternName()).Returns("FailingPattern");
            failData.Setup(o => o.GetDomainName()).Returns("Domain");
            failData.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "FailingPin" });
            failData.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 1001 });
            failData.Setup(o => o.GetVectorAddress()).Returns(2002);
            failData.Setup(o => o.GetCycle()).Returns(4004);
            this.plistObjectMock.Setup(o => o.IsPatternAnAmble("FailingPattern")).Returns(false);
            failureTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failData.Object });
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_lb"));
            this.ituffWriter.Setup(o => o.SetData("NA"));
            this.datalogService.Setup(o => o.WriteToItuff(this.ituffWriter.Object));

            this.Verify();
            this.CustomVerify();
            var result = this.Execute();
            Assert.AreEqual(0, result);
            Assert.AreEqual(0, this.ExitPort);
            this.functionalService.VerifyAll();
            failureTest.VerifyAll();
            this.testConditionService.VerifyAll();
            this.testCondition.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Full_FailedAmble_Port2()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;
            this.PrintPreviousLabel = OnOffMode.DISABLED;
            this.FailuresToCaptureTotal = 1;

            this.TestMethodExtension = (IFunctionalExtensions)this;
            var failureTest = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.functionalService
                .Setup(o => o.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 1, this.PrePlist))
                .Returns(failureTest.Object);
            failureTest.Setup(o => o.ApplyTestConditions());
            failureTest.Setup(o => o.Execute()).Returns(true);
            failureTest.Setup(o => o.SetPinMask(new List<string>()));
            var failData = new Mock<IFailureData>(MockBehavior.Strict);
            failData.Setup(o => o.GetPatternName()).Returns("FailingPattern");
            failData.Setup(o => o.GetDomainName()).Returns("Domain");
            failData.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "FailingPin" });
            failData.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 1001 });
            failData.Setup(o => o.GetVectorAddress()).Returns(2002);
            failData.Setup(o => o.GetCycle()).Returns(4004);
            this.plistObjectMock.Setup(o => o.IsPatternAnAmble("FailingPattern")).Returns(true);
            failureTest.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failData.Object });

            this.Verify();
            this.CustomVerify();
            var result = this.Execute();
            Assert.AreEqual(2, result);
            Assert.AreEqual(2, this.ExitPort);
            this.functionalService.VerifyAll();
            failureTest.VerifyAll();
            this.testConditionService.VerifyAll();
            this.testCondition.VerifyAll();
        }
    }
}