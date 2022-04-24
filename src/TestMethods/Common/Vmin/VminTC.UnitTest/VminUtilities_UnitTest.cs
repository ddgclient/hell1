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

namespace VminTC.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FunctionalService;
    using Prime.PlistService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class VminUtilities_UnitTest
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<ISharedStorageService> sharedStorageMock;

        /// <summary>
        /// Gets or sets mock shared storage.
        /// </summary>
        public Dictionary<string, string> SharedStorageValues { get; set; }

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

            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            // Default Mock for Shared service.
            this.SharedStorageValues = new Dictionary<string, string>();
            this.sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>()))
                .Callback((string key, object obj, Context context) =>
                {
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, string obj, Context context) =>
                {
                    this.SharedStorageValues[key] = obj;
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<Context>()))
                .Callback((string key, double obj, Context context) =>
                {
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.sharedStorageMock.Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>()))
                .Callback((string key, Type obj, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Type obj, Context context) => JsonConvert.DeserializeObject(this.SharedStorageValues[key], obj));
            this.sharedStorageMock.Setup(o => o.GetDoubleRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => double.Parse(this.SharedStorageValues[key]));
            this.sharedStorageMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => this.SharedStorageValues[key]);
            Prime.Services.SharedStorageService = this.sharedStorageMock.Object;
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void LogVminConfiguration_Pass()
        {
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var strgvalFormatMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strgvalFormatMock.Object);
            Prime.Services.DatalogService = datalogServiceMock.Object;
            strgvalFormatMock.Setup(o => o.SetData("C0:1:1.500_C1:1:1.500_C2:1:1.500_C3:1:1.500"));
            strgvalFormatMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            datalogServiceMock.Setup(o => o.WriteToItuff(strgvalFormatMock.Object));
            var vminForwardingFactoryMock = new Mock<IVminForwardingFactory>(MockBehavior.Strict);
            vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);
            var vminForwardingMock = new Mock<IVminForwardingCorner>(MockBehavior.Strict);
            var tuple0 = new Tuple<string, int, IVminForwardingCorner>("C0", 1, vminForwardingMock.Object);
            var tuple1 = new Tuple<string, int, IVminForwardingCorner>("C1", 1, vminForwardingMock.Object);
            var tuple2 = new Tuple<string, int, IVminForwardingCorner>("C2", 1, vminForwardingMock.Object);
            var tuple3 = new Tuple<string, int, IVminForwardingCorner>("C3", 1, vminForwardingMock.Object);
            var vminForwarding = new List<Tuple<string, int, IVminForwardingCorner>> { tuple0, tuple1, tuple2, tuple3 };
            DDG.VminForwarding.Service = vminForwardingFactoryMock.Object;

            ExtendedDataLogger.LogVminConfiguration(vminForwarding);

            datalogServiceMock.VerifyAll();
            strgvalFormatMock.VerifyAll();
            vminForwardingFactoryMock.VerifyAll();
            vminForwardingMock.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void RunRules_BitVector_Pass()
        {
            var searchResults = default(SearchResults);
            searchResults.TestResultsBits = new List<BitArray> { "0001".ToBitArray() };
            searchResults.RulesResultsBits = searchResults.RunRules(" 0000, 0011 ,1100",  null);
            Assert.AreEqual("0011", searchResults.RulesResultsBits.ToBinaryString());
            Assert.IsFalse(searchResults.FailedRules);
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void RunRules_BitVector_fail()
        {
            var searchResults = default(SearchResults);
            searchResults.TestResultsBits = new List<BitArray> { "0111".ToBitArray() };
            searchResults.RulesResultsBits = searchResults.RunRules("0000,0011,1100",  null);
            Assert.AreEqual("0111", searchResults.RulesResultsBits.ToBinaryString());
            Assert.IsTrue(searchResults.FailedRules);
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void RunRules_NoRecoveryOptions_pass()
        {
            var searchResults = default(SearchResults);
            searchResults.TestResultsBits = new List<BitArray> { "0000".ToBitArray() };
            searchResults.RulesResultsBits = searchResults.RunRules(null,  null);
            Assert.AreEqual("0000", searchResults.RulesResultsBits.ToBinaryString());
            Assert.IsFalse(searchResults.FailedRules);
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void RunRules_NoRecoveryOptions_Fail()
        {
            var searchResults = default(SearchResults);
            searchResults.TestResultsBits = new List<BitArray> { "0111".ToBitArray() };
            searchResults.RulesResultsBits = searchResults.RunRules(null, null);
            Assert.AreEqual("0111", searchResults.RulesResultsBits.ToBinaryString());
            Assert.IsTrue(searchResults.FailedRules);
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void RunRules_WithRule_pass()
        {
            var rule1 = new DieRecoveryBase.DefeatureRule.Rule("RuleName", "0011", 2, DieRecoveryBase.DefeatureRule.RuleType.FullyFeatured, DieRecoveryBase.DefeatureRule.RuleMode.ValidCombinations);
            var passingRules = new List<DieRecoveryBase.DefeatureRule.Rule> { rule1 };
            var dieRecoveryMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.Setup(o => o.RunRule(It.Is<BitArray>(it => it.ToBinaryString() == "0011"), "SomeRuleGroup")).Returns(passingRules);
            dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns("0010".ToBitArray());
            var searchResults = default(SearchResults);
            searchResults.TestResultsBits = new List<BitArray> { "0001".ToBitArray() };
            searchResults.RulesResultsBits = searchResults.RunRules("SomeRuleGroup,2", dieRecoveryMock.Object);
            Assert.AreEqual("0011", searchResults.RulesResultsBits.ToBinaryString());
            Assert.IsFalse(searchResults.FailedRules);
            dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void RunRules_WithRule_FailWrongSize()
        {
            var rule1 = new DieRecoveryBase.DefeatureRule.Rule("RuleName", "0011", 2, DieRecoveryBase.DefeatureRule.RuleType.FullyFeatured, DieRecoveryBase.DefeatureRule.RuleMode.ValidCombinations);
            var passingRules = new List<DieRecoveryBase.DefeatureRule.Rule> { rule1 };
            var dieRecoveryMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.Setup(o => o.RunRule(It.Is<BitArray>(it => it.ToBinaryString() == "0011"), "SomeRuleGroup")).Returns(passingRules);
            dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns("0010".ToBitArray());
            var searchResults = default(SearchResults);
            searchResults.TestResultsBits = new List<BitArray> { "0001".ToBitArray() };
            searchResults.RulesResultsBits = searchResults.RunRules("SomeRuleGroup,4", dieRecoveryMock.Object);
            Assert.AreEqual("0011", searchResults.RulesResultsBits.ToBinaryString());
            Assert.IsTrue(searchResults.FailedRules);
            dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void RunRules_WithRule_FailNoMatch()
        {
            var passingRules = new List<DieRecoveryBase.DefeatureRule.Rule> { };
            var dieRecoveryMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.Setup(o => o.RunRule(It.Is<BitArray>(it => it.ToBinaryString() == "0011"), "SomeRuleGroup")).Returns(passingRules);
            dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns("0010".ToBitArray());
            var searchResults = default(SearchResults);
            searchResults.TestResultsBits = new List<BitArray> { "0001".ToBitArray() };
            searchResults.RulesResultsBits = searchResults.RunRules("SomeRuleGroup,1", dieRecoveryMock.Object);
            Assert.AreEqual("0011", searchResults.RulesResultsBits.ToBinaryString());
            Assert.IsTrue(searchResults.FailedRules);
            dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void RunRules_WithRule_ExceptionBadFormat()
        {
            var dieRecoveryMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns("0010".ToBitArray());
            var searchResults = default(SearchResults);
            searchResults.TestResultsBits = new List<BitArray> { "0001".ToBitArray() };
            Assert.ThrowsException<Exception>(() => searchResults.RunRules("SomeRuleGroup", dieRecoveryMock.Object));
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void GetMappedString_Pass()
        {
            var result = ExtendedDataLogger.GetMappedString("g1234567", "1,2,3,4,5,6,7");
            Assert.AreEqual("1234567", result);
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void GetPlistContentsIndex_Pass()
        {
            var plistContent = new Mock<IPlistContent>(MockBehavior.Strict);
            plistContent.Setup(o => o.IsPattern()).Returns(true);
            plistContent.Setup(o => o.GetBurstIndex()).Returns(1);
            plistContent.SetupSequence(o => o.GetPatternIndex())
                .Returns(1)
                .Returns(2);
            plistContent.Setup(o => o.GetPlistItemName()).Returns("pattern");
            var plistObject = new Mock<IPlistObject>(MockBehavior.Strict);
            plistObject.Setup(o => o.GetPatternsAndIndexes(true)).Returns(new List<IPlistContent> { plistContent.Object, plistContent.Object });
            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            Prime.Services.PlistService = plistServiceMock.Object;
            plistServiceMock.Setup(o => o.GetPlistObject("SomePatlist")).Returns(plistObject.Object);

            var result = PlistUtilities.GetPlistContentsIndex("SomePatlist");
            Assert.AreEqual("pattern", result[0].PatternName);
            Assert.AreEqual(1, (int)result[0].Burst);
            Assert.AreEqual(1, (int)result[0].Index);
            Assert.AreEqual(1, (int)result[0].Occurrence);
            Assert.AreEqual("pattern", result[1].PatternName);
            Assert.AreEqual(1, (int)result[1].Burst);
            Assert.AreEqual(2, (int)result[1].Index);
            Assert.AreEqual(2, (int)result[1].Occurrence);
        }

        /// <summary>
        /// Test the TraceCTVs method.
        /// </summary>
        [TestMethod]
        public void TraceCTVs_Pass()
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

            // this one isn't printed because its the same pattern as the previous one.
            var ctvByCycle3Mock = new Mock<ICtvPerCycle>(MockBehavior.Strict);
            ctvByCycle3Mock.Setup(o => o.GetDomainName()).Returns("LEG");
            ctvByCycle3Mock.Setup(o => o.GetPatternName()).Returns("PatternB");
            ctvByCycle3Mock.Setup(o => o.GetParentPlistName()).Returns("PListB");
            ctvByCycle3Mock.Setup(o => o.GetVectorAddress()).Returns(2000);
            ctvByCycle3Mock.Setup(o => o.GetCycle()).Returns(40000);
            ctvByCycle3Mock.Setup(o => o.GetTraceLogRegister1()).Returns(1);
            ctvByCycle3Mock.Setup(o => o.GetTraceLogCycle()).Returns(40000);
            ctvByCycle3Mock.Setup(o => o.GetBurstIndex()).Returns(0);
            ctvByCycle3Mock.Setup(o => o.GetBurstCycle()).Returns(40000);

            var ctvByCycle4Mock = new Mock<ICtvPerCycle>(MockBehavior.Strict);
            ctvByCycle4Mock.Setup(o => o.GetDomainName()).Returns("LEG");
            ctvByCycle4Mock.Setup(o => o.GetPatternName()).Returns("PatternA");
            ctvByCycle4Mock.Setup(o => o.GetParentPlistName()).Returns("PListA");
            ctvByCycle4Mock.Setup(o => o.GetVectorAddress()).Returns(2000);
            ctvByCycle4Mock.Setup(o => o.GetCycle()).Returns(40000);
            ctvByCycle4Mock.Setup(o => o.GetTraceLogRegister1()).Returns(0);
            ctvByCycle4Mock.Setup(o => o.GetTraceLogCycle()).Returns(41000);
            ctvByCycle4Mock.Setup(o => o.GetBurstIndex()).Returns(1);
            ctvByCycle4Mock.Setup(o => o.GetBurstCycle()).Returns(15000);

            var funcTestMock = new Mock<ICaptureCtvPerCycleTest>(MockBehavior.Strict);
            funcTestMock.Setup(o => o.GetCtvPerCycle()).Returns(new List<ICtvPerCycle> { ctvByCycle1Mock.Object, ctvByCycle2Mock.Object, ctvByCycle3Mock.Object, ctvByCycle4Mock.Object });

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);

            var printSequence = new MockSequence();
            consoleServiceMock.InSequence(printSequence).Setup(o => o.PrintDebug("CTV TRACE BEGIN ============================================================================"));
            consoleServiceMock.InSequence(printSequence).Setup(o => o.PrintDebug("CTV Trace: Domain:[LEG] Pattern:[PatternA] Plist:[PListA] Address:[2] Cycle:[2] TraceReg1:[0] TraceCycle:[2] BusrtIndex:[0] BurstCycle:[2]"));
            consoleServiceMock.InSequence(printSequence).Setup(o => o.PrintDebug("CTV Trace: Domain:[LEG] Pattern:[PatternB] Plist:[PListB] Address:[2] Cycle:[2] TraceReg1:[1] TraceCycle:[2] BusrtIndex:[0] BurstCycle:[2]"));
            consoleServiceMock.InSequence(printSequence).Setup(o => o.PrintDebug("CTV Trace: Domain:[LEG] Pattern:[PatternA] Plist:[PListA] Address:[2000] Cycle:[40000] TraceReg1:[0] TraceCycle:[41000] BusrtIndex:[1] BurstCycle:[15000]"));
            consoleServiceMock.InSequence(printSequence).Setup(o => o.PrintDebug("CTV TRACE END ============================================================================"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            Assert.IsTrue(VminUtilities.TraceCTVs(funcTestMock.Object));
            funcTestMock.VerifyAll();
            ctvByCycle1Mock.VerifyAll();
            ctvByCycle2Mock.VerifyAll();
            ctvByCycle3Mock.VerifyAll();
            ctvByCycle4Mock.VerifyAll();
            consoleServiceMock.VerifyAll();
            consoleServiceMock.Verify(o => o.PrintDebug("CTV TRACE END ============================================================================")); // manually verify the final print since I don't think verifyall works correctly with sequences...
        }
    }
}
