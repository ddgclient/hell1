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

namespace PValTC.UnitTest
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
    using Prime.TestConditionService;
    using TOSUserSDK;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class PValTC_UnitTest : PValTC
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IFunctionalService> functionalService;
        private Mock<ITestConditionService> testConditionService;
        private Mock<IDatalogService> datalogService;
        private Mock<IFailDataFormat> ituffWriter;
        private Mock<ICaptureFailureTest> functionalTestMock;
        private Mock<IPlistService> plistServiceMock;
        private Mock<IPlistObject> plistMock;
        private Mock<IPlist> tosUserSdkPlistMock;

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
            this.functionalTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.functionalTestMock.Setup(o => o.ApplyTestConditions());
            this.functionalTestMock.Setup(o => o.SetPinMask(It.IsAny<List<string>>()));
            this.functionalService.Setup(o => o.CreateCaptureFailureTest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ulong>(), 1, It.IsAny<string>())).Returns(this.functionalTestMock.Object);
            Prime.Services.FunctionalService = this.functionalService.Object;

            this.testConditionService = new Mock<ITestConditionService>(MockBehavior.Strict);
            Prime.Services.TestConditionService = this.testConditionService.Object;

            this.ituffWriter = new Mock<IFailDataFormat>(MockBehavior.Strict);
            this.datalogService = new Mock<IDatalogService>(MockBehavior.Strict);
            this.datalogService.Setup(d => d.GetItuffFailDataWriter()).Returns(this.ituffWriter.Object);
            Prime.Services.DatalogService = this.datalogService.Object;

            this.plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            this.plistMock = new Mock<IPlistObject>(MockBehavior.Strict);
            this.plistMock.Setup(o => o.EnableAllPatterns());
            this.plistServiceMock.Setup(o => o.GetPlistObject("SomePatlist")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist0")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist1")).Returns(this.plistMock.Object);
            Prime.Services.PlistService = this.plistServiceMock.Object;

            var tuple0 = new Tuple<string, int, string, int>("Pattern0", 1, "Pattern", 1);
            var tuple1 = new Tuple<string, int, string, int>("Pattern1", 2, "Pattern", 1);
            var tuple2 = new Tuple<string, int, string, int>("Pattern2", 3, "Pattern", 1);
            var tuple3 = new Tuple<string, int, string, int>("Pattern3", 4, "Pattern", 1);
            var tuple4 = new Tuple<string, int, string, int>("Pattern4", 5, "Pattern", 1);
            var tuple5 = new Tuple<string, int, string, int>("Pattern5", 6, "Pattern", 1);
            var plist0 = new Tuple<string, int, string, int>("Plist0", 7, "Plist", 1);
            var tuple6 = new Tuple<string, int, string, int>("Pattern6", 8, "Pattern", 1);
            var plist1 = new Tuple<string, int, string, int>("Plist1", 9, "Plist", 1);
            var tuple7 = new Tuple<string, int, string, int>("Pattern7", 1, "Pattern", 1);
            var tuple8 = new Tuple<string, int, string, int>("Pattern8", 2, "Pattern", 1);
            var tuple9 = new Tuple<string, int, string, int>("Pattern9", 3, "Pattern", 1);
            var tuple10 = new Tuple<string, int, string, int>("Pattern10", 1, "Pattern", 1);
            var topIndex = new List<Tuple<string, int, string, int>> { tuple0, tuple1, tuple2, tuple3, tuple4, tuple5, plist0, tuple6, plist1 };
            var index0 = new List<Tuple<string, int, string, int>> { tuple7, tuple8, tuple9 };
            var index1 = new List<Tuple<string, int, string, int>> { tuple10 };
            this.tosUserSdkPlistMock = new Mock<IPlist>(MockBehavior.Strict);
            this.tosUserSdkPlistMock.Setup(o => o.GetPatternsAndIndexesInPlist("SomePatlist", false)).Returns(topIndex);
            this.tosUserSdkPlistMock.Setup(o => o.GetPatternsAndIndexesInPlist("Plist0", false)).Returns(index0);
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("Plist0")).Returns(new List<Tuple<string, string>>());
            this.tosUserSdkPlistMock.Setup(o => o.GetPatternsAndIndexesInPlist("Plist1", false)).Returns(index1);
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("Plist1")).Returns(new List<Tuple<string, string>>());
            TOSUserSDK.Plist.Service = this.tosUserSdkPlistMock.Object;
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void Execute_PassAllExecutions_Pass()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.PrePlist = string.Empty;
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;

            this.functionalTestMock.SetupSequence(o => o.Execute())
                .Returns(true) // Time0
                .Returns(true) // Burst-off
                .Returns(true) // Pattern0
                .Returns(true) // Pattern1
                .Returns(true) // Pattern2
                .Returns(true) // Pattern3
                .Returns(true) // Pattern4
                .Returns(true) // Pattern5
                .Returns(true) // Pattern7
                .Returns(true) // Pattern8
                .Returns(true) // Pattern9
                .Returns(true) // Pattern6
                .Returns(true); // Pattern10
            this.plistMock.Setup(o => o.SetOption("Burst", "BurstOffDeep"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern0"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern1"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern2"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern3"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern4"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern5"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern6"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern7"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern8"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern9"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern10"));
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist0")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist1")).Returns(this.plistMock.Object);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "BurstOffDeep" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("SomePatlist")).Returns(new List<Tuple<string, string>>());
            this.Verify();
            Assert.AreEqual(1, this.Execute());
            this.functionalTestMock.VerifyAll();
            this.plistMock.VerifyAll();
            this.tosUserSdkPlistMock.VerifyAll();
            this.ituffWriter.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void Execute_ExecuteN2_Pass()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.PrePlist = string.Empty;
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;

            this.functionalTestMock.SetupSequence(o => o.Execute())
                .Returns(true) // Time0
                .Returns(true) // Burst-off
                .Returns(false) // A:Pattern0, V:Pattern2
                .Returns(false) // A:Pattern1, V:Pattern3
                .Returns(true) // Pattern2
                .Returns(false) // A: Pattern3, V:Pattern7
                .Returns(true) // Pattern4
                .Returns(true) // Pattern5
                .Returns(true) // Pattern7
                .Returns(false) // A: Pattern8, V:Pattern9
                .Returns(true) // Pattern9
                .Returns(true) // Pattern6
                .Returns(true); // Pattern10
            this.plistMock.Setup(o => o.SetOption("Burst", "BurstOffDeep"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern0"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern1"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern2"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern3"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern4"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern5"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern6"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern7"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern8"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern9"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern10"));
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist0")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist1")).Returns(this.plistMock.Object);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "BurstOffDeep" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("SomePatlist")).Returns(new List<Tuple<string, string>>());

            var failureData = new Mock<IFailureData>(MockBehavior.Strict);
            failureData.SetupSequence(o => o.GetPatternName())
                .Returns("Pattern2")
                .Returns("Pattern3")
                .Returns("Pattern7")
                .Returns("Pattern9");
            failureData.Setup(o => o.GetCycle()).Returns(100);
            failureData.Setup(o => o.GetVectorAddress()).Returns(10);
            failureData.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 1001, 1002 });
            this.functionalTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failureData.Object });
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Pattern0"));
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Pattern1"));
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Pattern3"));
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Pattern8"));
            this.ituffWriter.Setup(o => o.SetData("Pattern2", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.ituffWriter.Setup(o => o.SetData("Pattern3", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.ituffWriter.Setup(o => o.SetData("Pattern7", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.ituffWriter.Setup(o => o.SetData("Pattern9", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.datalogService.Setup(o => o.WriteToItuff(It.IsAny<IItuffFormat>()));
            this.Verify();
            Assert.AreEqual(1, this.Execute());
            this.functionalTestMock.VerifyAll();
            this.plistMock.VerifyAll();
            this.tosUserSdkPlistMock.VerifyAll();
            this.ituffWriter.VerifyAll();
            failureData.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void Execute_ExecuteN2_RemovePlistOptionsFails()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.PrePlist = string.Empty;
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;

            this.functionalTestMock.SetupSequence(o => o.Execute())
                .Returns(true) // Time0
                .Returns(true); // Burst-off
            this.plistMock.Setup(o => o.SetOption("Burst", "BurstOffDeep"));
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist0")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist1")).Returns(this.plistMock.Object);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "BurstOffDeep" })).Returns(false);
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("SomePatlist")).Returns(new List<Tuple<string, string>>());

            this.Verify();
            var ex = Assert.ThrowsException<Exception>(() => this.Execute());
            Assert.AreEqual("Unable to set Burst mode on.", ex.Message);
            this.functionalTestMock.VerifyAll();
            this.plistMock.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void Execute_ExecuteN2PrePlist_Pass()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.PrePlist = string.Empty;
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;

            this.functionalTestMock.SetupSequence(o => o.Execute())
                .Returns(true) // Time0
                .Returns(true) // Burst-off
                .Returns(false) // A:Pattern0, V:Pattern2
                .Returns(false) // A:Pattern1, V:Pattern3
                .Returns(true) // Pattern2
                .Returns(false) // A: Pattern3, V:Pattern7
                .Returns(true) // Pattern4
                .Returns(true) // Pattern5
                .Returns(true) // Pattern7
                .Returns(false) // A: Pattern8, V:Pattern9
                .Returns(true) // Pattern9
                .Returns(true) // Pattern6
                .Returns(true); // Pattern10
            this.plistMock.Setup(o => o.SetOption("Burst", "BurstOffDeep"));
            this.plistMock.Setup(o => o.SetOption("PrePList", "SomePrePlist_N2"));
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist0")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist1")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.Exists("SomePrePlist_N2")).Returns(false);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "BurstOffDeep" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "PrePattern", "PrePList" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("Plist0", new List<string> { "PrePattern", "PrePList" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("Plist1", new List<string> { "PrePattern", "PrePList" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("SomePatlist")).Returns(new List<Tuple<string, string>> { new Tuple<string, string>("PrePList", "SomePrePlist") });
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("Plist0")).Returns(new List<Tuple<string, string>> { new Tuple<string, string>("PrePList", "SomePrePlist") });
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("Plist1")).Returns(new List<Tuple<string, string>> { new Tuple<string, string>("PrePList", "SomePrePlist") });
            this.tosUserSdkPlistMock.Setup(o => o.SetPlistOptions("SomePatlist", new List<Tuple<string, string>> { new Tuple<string, string>("PrePList", "SomePrePlist") })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.SetPlistOptions("Plist0", new List<Tuple<string, string>> { new Tuple<string, string>("PrePList", "SomePrePlist") })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.SetPlistOptions("Plist1", new List<Tuple<string, string>> { new Tuple<string, string>("PrePList", "SomePrePlist") })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.CopyPlist("SomePrePlist", "SomePrePlist_N2")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.ResolvePlist("SomePatlist")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.ResolvePlist("Plist0")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.ResolvePlist("Plist1")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.ResolvePlist("SomePrePlist_N2")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.GetPatternsAndIndexesInPlist("SomePrePlist_N2", false))
                .Returns(new List<Tuple<string, int, string, int>> { new Tuple<string, int, string, int>("SomePrePattern", 0, "Pattern", 0) });
            this.tosUserSdkPlistMock.Setup(o => o.GetPatternsAndIndexesInPlist("SomePrePlist", false))
                .Returns(new List<Tuple<string, int, string, int>> { new Tuple<string, int, string, int>("SomePrePattern", 0, "Pattern", 0) });
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern0")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern1")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern2")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern3")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern4")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern5")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern6")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern7")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern8")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern9")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern10")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "SomePrePattern")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemoveItemFromPList("SomePrePlist_N2", 0)).Returns(true);

            var failureData = new Mock<IFailureData>(MockBehavior.Strict);
            failureData.SetupSequence(o => o.GetPatternName())
                .Returns("Pattern2")
                .Returns("Pattern3")
                .Returns("Pattern7")
                .Returns("Pattern9");
            failureData.Setup(o => o.GetCycle()).Returns(100);
            failureData.Setup(o => o.GetVectorAddress()).Returns(10);
            failureData.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 1001, 1002 });
            this.functionalTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failureData.Object });
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Pattern0"));
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Pattern1"));
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Pattern3"));
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Pattern8"));
            this.ituffWriter.Setup(o => o.SetData("Pattern2", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.ituffWriter.Setup(o => o.SetData("Pattern3", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.ituffWriter.Setup(o => o.SetData("Pattern7", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.ituffWriter.Setup(o => o.SetData("Pattern9", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.datalogService.Setup(o => o.WriteToItuff(It.IsAny<IItuffFormat>()));
            this.Verify();
            Assert.AreEqual(1, this.Execute());
            this.functionalTestMock.VerifyAll();
            this.plistMock.VerifyAll();
            this.tosUserSdkPlistMock.VerifyAll();
            this.ituffWriter.VerifyAll();
            failureData.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void Execute_ExecuteN2PrePlist_RestoreFails()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.PrePlist = string.Empty;
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;

            this.functionalTestMock.SetupSequence(o => o.Execute())
                .Returns(true) // Time0
                .Returns(true) // Burst-off
                .Returns(false) // A:Pattern0, V:Pattern2
                .Returns(false) // A:Pattern1, V:Pattern3
                .Returns(true) // Pattern2
                .Returns(false) // A: Pattern3, V:Pattern7
                .Returns(true) // Pattern4
                .Returns(true) // Pattern5
                .Returns(true) // Pattern7
                .Returns(false) // A: Pattern8, V:Pattern9
                .Returns(true) // Pattern9
                .Returns(true) // Pattern6
                .Returns(true); // Pattern10
            this.plistMock.Setup(o => o.SetOption("Burst", "BurstOffDeep"));
            this.plistMock.Setup(o => o.SetOption("PrePList", "SomePrePlist_N2"));
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist0")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist1")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.Exists("SomePrePlist_N2")).Returns(false);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "BurstOffDeep" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "PrePattern", "PrePList" })).Returns(false);
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("SomePatlist")).Returns(new List<Tuple<string, string>> { new Tuple<string, string>("PrePList", "SomePrePlist") });
            this.tosUserSdkPlistMock.Setup(o => o.CopyPlist("SomePrePlist", "SomePrePlist_N2")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.ResolvePlist("SomePrePlist_N2")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.GetPatternsAndIndexesInPlist("SomePrePlist_N2", false))
                .Returns(new List<Tuple<string, int, string, int>> { new Tuple<string, int, string, int>("SomePrePattern", 0, "Pattern", 0) });
            this.tosUserSdkPlistMock.Setup(o => o.GetPatternsAndIndexesInPlist("SomePrePlist", false))
                .Returns(new List<Tuple<string, int, string, int>> { new Tuple<string, int, string, int>("SomePrePattern", 0, "Pattern", 0) });
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern0")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "SomePrePattern")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemoveItemFromPList("SomePrePlist_N2", 0)).Returns(true);

            var failureData = new Mock<IFailureData>(MockBehavior.Strict);
            failureData.SetupSequence(o => o.GetPatternName())
                .Returns("Pattern2")
                .Returns("Pattern3")
                .Returns("Pattern7")
                .Returns("Pattern9");
            failureData.Setup(o => o.GetCycle()).Returns(100);
            failureData.Setup(o => o.GetVectorAddress()).Returns(10);
            failureData.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 1001, 1002 });
            this.functionalTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failureData.Object });
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Pattern0"));
            this.ituffWriter.Setup(o => o.SetData("Pattern2", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.datalogService.Setup(o => o.WriteToItuff(It.IsAny<IItuffFormat>()));
            this.Verify();
            var ex = Assert.ThrowsException<Exception>(() => this.Execute());
            Assert.AreEqual("PValTC.dll.RestorePlist: failed removing PrePattern option for Patlist=[SomePatlist].", ex.Message);
            this.functionalTestMock.VerifyAll();
            this.plistMock.VerifyAll();
            this.ituffWriter.VerifyAll();
            failureData.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void Execute_ExecuteN2PrePlist_CopyPlistFails()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.PrePlist = string.Empty;
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;

            this.functionalTestMock.SetupSequence(o => o.Execute())
                .Returns(true) // Time0
                .Returns(true) // Burst-off
                .Returns(false) // A:Pattern0, V:Pattern2
                .Returns(false) // A:Pattern1, V:Pattern3
                .Returns(true) // Pattern2
                .Returns(false) // A: Pattern3, V:Pattern7
                .Returns(true) // Pattern4
                .Returns(true) // Pattern5
                .Returns(true) // Pattern7
                .Returns(false) // A: Pattern8, V:Pattern9
                .Returns(true) // Pattern9
                .Returns(true) // Pattern6
                .Returns(true); // Pattern10
            this.plistMock.Setup(o => o.SetOption("Burst", "BurstOffDeep"));
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist0")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist1")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.Exists("SomePrePlist_N2")).Returns(false);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "BurstOffDeep" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "PrePattern", "PrePList" })).Returns(false);
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("SomePatlist")).Returns(new List<Tuple<string, string>> { new Tuple<string, string>("PrePList", "SomePrePlist") });
            this.tosUserSdkPlistMock.Setup(o => o.CopyPlist("SomePrePlist", "SomePrePlist_N2")).Returns(false);
            this.tosUserSdkPlistMock.Setup(o => o.ResolvePlist("SomePrePlist_N2")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.GetPatternsAndIndexesInPlist("SomePrePlist_N2", false))
                .Returns(new List<Tuple<string, int, string, int>> { new Tuple<string, int, string, int>("SomePrePattern", 0, "Pattern", 0) });
            this.tosUserSdkPlistMock.Setup(o => o.GetPatternsAndIndexesInPlist("SomePrePlist", false))
                .Returns(new List<Tuple<string, int, string, int>> { new Tuple<string, int, string, int>("SomePrePattern", 0, "Pattern", 0) });
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern0")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "SomePrePattern")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemoveItemFromPList("SomePrePlist_N2", 0)).Returns(true);

            this.Verify();
            var ex = Assert.ThrowsException<Exception>(() => this.Execute());
            Assert.AreEqual("PValTC.dll.ClonePlist: failed copying Patlist=[SomePrePlist].", ex.Message);
            this.functionalTestMock.VerifyAll();
            this.plistMock.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void Execute_ExecuteN2PrePattern_Pass()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.PrePlist = string.Empty;
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;
            this.MaskPins = "SomeMaskedPin";

            this.functionalTestMock.SetupSequence(o => o.Execute())
                .Returns(true) // Time0
                .Returns(true) // Burst-off
                .Returns(false) // A:Pattern0, V:Pattern2
                .Returns(false) // A:Pattern1, V:Pattern3
                .Returns(true) // Pattern2
                .Returns(false) // A: Pattern3, V:Pattern7
                .Returns(true) // Pattern4
                .Returns(true) // Pattern5
                .Returns(true) // Pattern7
                .Returns(false) // A: Pattern8, V:Pattern9
                .Returns(true) // Pattern9
                .Returns(true) // Pattern6
                .Returns(true); // Pattern10
            this.plistMock.Setup(o => o.SetOption("Burst", "BurstOffDeep"));
            this.plistMock.Setup(o => o.SetOption("PrePList", "SomePrePlist_N2"));
            this.plistMock.Setup(o => o.SetOption("PrePList", "SomePatlist_N2"));
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist0")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist1")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.Exists("SomePrePlist_N2")).Returns(false);
            this.plistServiceMock.Setup(o => o.Exists("SomePatlist_N2")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "BurstOffDeep" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "PrePattern", "PrePList" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "PrePattern" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist_N2", new List<string> { "PrePattern" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("Plist0", new List<string> { "PrePattern", "PrePList" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("Plist1", new List<string> { "PrePattern", "PrePList" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("SomePatlist")).Returns(new List<Tuple<string, string>> { new Tuple<string, string>("PrePattern", "SomePrePattern") });
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("SomePatlist_N2")).Returns(new List<Tuple<string, string>> { new Tuple<string, string>("PrePattern", "SomePrePattern") });
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("Plist0")).Returns(new List<Tuple<string, string>> { new Tuple<string, string>("PrePList", "SomePrePlist") });
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("Plist1")).Returns(new List<Tuple<string, string>> { new Tuple<string, string>("PrePList", "SomePrePlist") });
            this.tosUserSdkPlistMock.Setup(o => o.SetPlistOptions("SomePatlist", new List<Tuple<string, string>> { new Tuple<string, string>("PrePattern", "SomePrePattern") })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.SetPlistOptions("Plist0", new List<Tuple<string, string>> { new Tuple<string, string>("PrePList", "SomePrePlist") })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.SetPlistOptions("Plist1", new List<Tuple<string, string>> { new Tuple<string, string>("PrePList", "SomePrePlist") })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.CopyPlist("SomePrePlist", "SomePrePlist_N2")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.ResolvePlist("SomePatlist")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.ResolvePlist("Plist0")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.ResolvePlist("Plist1")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.ResolvePlist("SomePrePlist_N2")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.ResolvePlist("SomePatlist_N2")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.GetPatternsAndIndexesInPlist("SomePrePlist_N2", false))
                .Returns(new List<Tuple<string, int, string, int>> { new Tuple<string, int, string, int>("SomePrePattern", 0, "Pattern", 0) });
            this.tosUserSdkPlistMock.Setup(o => o.GetPatternsAndIndexesInPlist("SomePrePlist", false))
                .Returns(new List<Tuple<string, int, string, int>> { new Tuple<string, int, string, int>("SomePrePattern", 0, "Pattern", 0) });
            this.tosUserSdkPlistMock.Setup(o => o.GetPatternsAndIndexesInPlist("SomePatlist_N2", false))
                .Returns(new List<Tuple<string, int, string, int>> { new Tuple<string, int, string, int>("Pattern0", 0, "Pattern", 0) });
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePatlist_N2", "SomePrePattern")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePatlist_N2", "Pattern0")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePatlist_N2", "Pattern1")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePatlist_N2", "Pattern2")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePatlist_N2", "Pattern3")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePatlist_N2", "Pattern4")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePatlist_N2", "Pattern5")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePatlist_N2", "Pattern6")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "SomePrePattern")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern7")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern8")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern9")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.AddPatternToPList("SomePrePlist_N2", "Pattern10")).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemoveItemFromPList("SomePrePlist_N2", 0)).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.RemoveItemFromPList("SomePatlist_N2", 0)).Returns(true);

            var failureData = new Mock<IFailureData>(MockBehavior.Strict);
            failureData.SetupSequence(o => o.GetPatternName())
                .Returns("Pattern2")
                .Returns("Pattern3")
                .Returns("Pattern7")
                .Returns("Pattern9");
            failureData.Setup(o => o.GetCycle()).Returns(100);
            failureData.Setup(o => o.GetVectorAddress()).Returns(10);
            failureData.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 1001, 1002 });
            this.functionalTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failureData.Object });
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Pattern0"));
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Pattern1"));
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Pattern3"));
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Pattern8"));
            this.ituffWriter.Setup(o => o.SetData("Pattern2", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.ituffWriter.Setup(o => o.SetData("Pattern3", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.ituffWriter.Setup(o => o.SetData("Pattern7", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.ituffWriter.Setup(o => o.SetData("Pattern9", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.datalogService.Setup(o => o.WriteToItuff(It.IsAny<IItuffFormat>()));
            this.Verify();
            Assert.AreEqual(1, this.Execute());
            this.functionalTestMock.VerifyAll();
            this.plistMock.VerifyAll();
            this.tosUserSdkPlistMock.VerifyAll();
            this.ituffWriter.VerifyAll();
            failureData.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void Execute_FailingTime0_Pass()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.PrePlist = string.Empty;
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;

            this.functionalTestMock.SetupSequence(o => o.Execute())
                .Returns(false) // Time0
                .Returns(true) // Burst-off
                .Returns(true) // Pattern0
                .Returns(true) // Pattern1
                .Returns(true) // Pattern3
                .Returns(true) // Pattern4
                .Returns(true) // Pattern5
                .Returns(true) // Pattern7
                .Returns(true) // Pattern8
                .Returns(true) // Pattern9
                .Returns(true) // Pattern6
                .Returns(true); // Pattern10
            this.plistMock.Setup(o => o.SetOption("Burst", "BurstOffDeep"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern0"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern1"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern3"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern4"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern5"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern6"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern7"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern8"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern9"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern10"));
            this.plistMock.Setup(o => o.DisablePatterns(new HashSet<string> { "Pattern2" }));
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist0")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist1")).Returns(this.plistMock.Object);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "BurstOffDeep" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("SomePatlist")).Returns(new List<Tuple<string, string>>());
            var failureData = new Mock<IFailureData>(MockBehavior.Strict);
            failureData.Setup(o => o.GetPatternName()).Returns("Pattern2");
            failureData.Setup(o => o.GetCycle()).Returns(100);
            failureData.Setup(o => o.GetVectorAddress()).Returns(10);
            failureData.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 1001, 1002 });
            this.functionalTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failureData.Object });
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_Time0"));
            this.ituffWriter.Setup(o => o.SetData("Pattern2", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.datalogService.Setup(o => o.WriteToItuff(It.IsAny<IItuffFormat>()));
            this.Verify();
            Assert.AreEqual(1, this.Execute());
            this.functionalTestMock.VerifyAll();
            this.plistMock.VerifyAll();
            this.tosUserSdkPlistMock.VerifyAll();
            this.ituffWriter.VerifyAll();
            failureData.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void Execute_FailingBurstOff_Pass()
        {
            this.Patlist = "SomePatlist";
            this.TimingsTc = "SomeTimings";
            this.LevelsTc = "SomeLevels";
            this.PrePlist = string.Empty;
            this.LogLevel = PrimeLogLevel.PRIME_DEBUG;

            this.functionalTestMock.SetupSequence(o => o.Execute())
                .Returns(true) // Time0
                .Returns(false) // Burst-off
                .Returns(true) // Pattern0
                .Returns(true) // Pattern1
                .Returns(true) // Pattern3
                .Returns(true) // Pattern4
                .Returns(true) // Pattern5
                .Returns(true) // Pattern7
                .Returns(true) // Pattern8
                .Returns(true) // Pattern9
                .Returns(true) // Pattern6
                .Returns(true); // Pattern10
            this.plistMock.Setup(o => o.SetOption("Burst", "BurstOffDeep"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern0"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern1"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern3"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern4"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern5"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern6"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern7"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern8"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern9"));
            this.plistMock.Setup(o => o.SetOption("PrePattern", "Pattern10"));
            this.plistMock.Setup(o => o.DisablePatterns(new HashSet<string> { "Pattern2" }));
            this.plistServiceMock.Setup(o => o.GetPlistObject("Plist0")).Returns(this.plistMock.Object);
            this.plistServiceMock.Setup(o => o.Exists("SomePatlist_N2")).Returns(false);
            this.tosUserSdkPlistMock.Setup(o => o.RemovePlistOptions("SomePatlist", new List<string> { "BurstOffDeep" })).Returns(true);
            this.tosUserSdkPlistMock.Setup(o => o.GetPlistOptions("SomePatlist")).Returns(new List<Tuple<string, string>>());
            var failureData = new Mock<IFailureData>(MockBehavior.Strict);
            failureData.Setup(o => o.GetPatternName()).Returns("Pattern2");
            failureData.Setup(o => o.GetCycle()).Returns(100);
            failureData.Setup(o => o.GetVectorAddress()).Returns(10);
            failureData.Setup(o => o.GetFailingPinChannels()).Returns(new List<uint> { 1001, 1002 });
            this.functionalTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failureData.Object });
            this.ituffWriter.Setup(o => o.SetTnamePostfix("_BurstOff"));
            this.ituffWriter.Setup(o => o.SetData("Pattern2", 10, -1, -1, 100, new List<uint> { 1001, 1002 }));
            this.datalogService.Setup(o => o.WriteToItuff(It.IsAny<IItuffFormat>()));
            this.Verify();
            Assert.AreEqual(1, this.Execute());
            this.functionalTestMock.VerifyAll();
            this.plistMock.VerifyAll();
            this.tosUserSdkPlistMock.VerifyAll();
            this.ituffWriter.VerifyAll();
            failureData.VerifyAll();
        }
    }
}