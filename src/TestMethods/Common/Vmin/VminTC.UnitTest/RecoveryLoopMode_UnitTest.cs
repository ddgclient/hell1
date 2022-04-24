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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.TestMethods.VminSearch;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RecoveryLoopMode_UnitTest : RecoveryLoopMode
    {
        private Mock<IConsoleService> consoleServiceMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryLoopMode_UnitTest"/> class.
        /// </summary>
        public RecoveryLoopMode_UnitTest()
            : base(Prime.Services.ConsoleService)
        {
        }

        /// <summary>
        /// Setup mocks.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(
                    o => o.PrintDebug(It.IsAny<string>())).
                Callback((string msg) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            this.consoleServiceMock.Setup(
                    o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string member, string path) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;
        }

        /// <summary>
        /// Validates mocks.
        /// </summary>
        [TestCleanup]
        public void CleanupTestMethod()
        {
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void HasToRepeatSearch_True()
        {
            var searchPointData = new SearchPointData(new List<double>(), new SearchPointData.PatternData("PatternName", 1, 1));
            var searchStateValues = new SearchStateValues
            {
                Voltages = new List<double> { -9999D, 0.5, 0.5, 0.5 },
                StartVoltages = new List<double>(),
                EndVoltageLimits = new List<double>(),
                ExecutionCount = 1,
                MaskBits = new BitArray(4),
                FailReason = string.Empty,
                PerPointData = new List<SearchPointData> { searchPointData },
                PerTargetIncrements = new List<uint>(),
            };

            var searchIdentifiers = new SearchIdentifiers(string.Empty, 1, 1);
            var searchResultData = new SearchResultData(searchStateValues, false, searchIdentifiers);
            var testBits = "1000".ToBitArray();
            var searchResults = new SearchResults
            {
                FailedRules = false,
                FailedSearch = true,
                TestResultsBits = new List<BitArray> { testBits },
                RulesResultsBits = "1000".ToBitArray(),
                SearchResultData = new List<SearchResultData> { searchResultData },
                MaxRepetitionCount = 2,
                IncomingMask = new BitArray(4, false),
            };

            var result = this.HasToRepeatSearch(ref searchResults, null, null, "0000,1100,0011");
            Assert.AreEqual("1100", searchResults.RulesResultsBits.ToBinaryString());
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void HasToRepeatSearch_MaxRepetition_False()
        {
            var searchPointData = new SearchPointData(new List<double>(), new SearchPointData.PatternData("PatternName", 1, 1));
            var searchStateValues = new SearchStateValues
            {
                Voltages = new List<double> { -9999D, 0.5, 0.5, 0.5 },
                StartVoltages = new List<double>(),
                EndVoltageLimits = new List<double>(),
                ExecutionCount = 1,
                MaskBits = new BitArray(4),
                FailReason = string.Empty,
                PerPointData = new List<SearchPointData> { searchPointData },
                PerTargetIncrements = new List<uint>(),
            };

            var searchIdentifiers = new SearchIdentifiers(string.Empty, 1, 2);
            var searchResultData = new SearchResultData(searchStateValues, false, searchIdentifiers);
            var testBits = "1000".ToBitArray();
            var searchResults = new SearchResults
            {
                FailedRules = false,
                FailedSearch = true,
                TestResultsBits = new List<BitArray> { testBits },
                RulesResultsBits = "1000".ToBitArray(),
                SearchResultData = new List<SearchResultData> { searchResultData },
                MaxRepetitionCount = 2,
                IncomingMask = new BitArray(4, false),
            };

            var result = this.HasToRepeatSearch(ref searchResults, null, null, "0000");
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void HasToRepeatSearch_FailedRules_False()
        {
            var searchPointData = new SearchPointData(new List<double>(), new SearchPointData.PatternData("PatternName", 1, 1));
            var searchStateValues = new SearchStateValues
            {
                Voltages = new List<double> { -9999D, 0.5, 0.5, 0.5 },
                StartVoltages = new List<double>(),
                EndVoltageLimits = new List<double>(),
                ExecutionCount = 1,
                MaskBits = new BitArray(4),
                FailReason = string.Empty,
                PerPointData = new List<SearchPointData> { searchPointData },
                PerTargetIncrements = new List<uint>(),
            };

            var searchIdentifiers = new SearchIdentifiers(string.Empty, 1, 1);
            var searchResultData = new SearchResultData(searchStateValues, false, searchIdentifiers);
            var testBits = "1000".ToBitArray();
            var searchResults = new SearchResults
            {
                FailedRules = true,
                FailedSearch = true,
                TestResultsBits = new List<BitArray> { testBits },
                RulesResultsBits = "1000".ToBitArray(),
                SearchResultData = new List<SearchResultData> { searchResultData },
                MaxRepetitionCount = 2,
                IncomingMask = new BitArray(4, false),
            };

            var result = this.HasToRepeatSearch(ref searchResults, null, null, "0000");
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void UpdateRecoveryTrackers_Passing_Pass()
        {
            var testBits = "1000".ToBitArray();
            var searchResults = new SearchResults
            {
                FailedRules = false,
                FailedSearch = false,
                IncomingMask = "1100".ToBitArray(),
                TestResultsBits = new List<BitArray> { testBits },
                RulesResultsBits = "1000".ToBitArray(),
            };

            var tracker = new Mock<IDieRecovery>(MockBehavior.Default);
            tracker.Setup(o => o.UpdateTrackingStructure(searchResults.RulesResultsBits, searchResults.IncomingMask, testBits, UpdateMode.Merge, true))
                .Returns(true);

            var result = this.UpdateRecoveryTrackers(searchResults, tracker.Object, false);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void UpdateRecoveryTrackers_Force_Pass()
        {
            var testBits = "1000".ToBitArray();
            var searchResults = new SearchResults
            {
                FailedRules = true,
                FailedSearch = true,
                IncomingMask = "1100".ToBitArray(),
                TestResultsBits = new List<BitArray> { testBits },
                RulesResultsBits = "1000".ToBitArray(),
            };

            var tracker = new Mock<IDieRecovery>(MockBehavior.Default);
            tracker.Setup(o => o.UpdateTrackingStructure(searchResults.RulesResultsBits, searchResults.IncomingMask, testBits, UpdateMode.Merge, true))
                .Returns(true);

            var result = this.UpdateRecoveryTrackers(searchResults, tracker.Object, true);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void UpdateRecoveryTrackers_FailedUpdate_Exception()
        {
            var testBits = "1000".ToBitArray();
            var searchResults = new SearchResults
            {
                FailedRules = true,
                FailedSearch = true,
                IncomingMask = "1100".ToBitArray(),
                TestResultsBits = new List<BitArray> { testBits },
                RulesResultsBits = "1000".ToBitArray(),
            };

            var tracker = new Mock<IDieRecovery>(MockBehavior.Default);
            tracker.Setup(o => o.UpdateTrackingStructure(searchResults.RulesResultsBits, searchResults.IncomingMask, testBits, UpdateMode.Merge, true))
                .Returns(false);

            var result = this.UpdateRecoveryTrackers(searchResults, tracker.Object, true);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void UpdateRecoveryTrackers_FailedRules_Skip()
        {
            var testBits = "1000".ToBitArray();
            var searchResults = new SearchResults
            {
                FailedRules = true,
                FailedSearch = false,
                IncomingMask = "1100".ToBitArray(),
                TestResultsBits = new List<BitArray> { testBits },
                RulesResultsBits = "1000".ToBitArray(),
            };

            var tracker = new Mock<IDieRecovery>(MockBehavior.Default);
            var result = this.UpdateRecoveryTrackers(searchResults, tracker.Object, false);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void UpdateRecoveryTrackers_FailedTest_Skip()
        {
            var testBits = "1000".ToBitArray();
            var searchResults = new SearchResults
            {
                FailedRules = false,
                FailedSearch = true,
                IncomingMask = "1100".ToBitArray(),
                TestResultsBits = new List<BitArray> { testBits },
                RulesResultsBits = "1000".ToBitArray(),
            };

            var tracker = new Mock<IDieRecovery>(MockBehavior.Default);
            var result = this.UpdateRecoveryTrackers(searchResults, tracker.Object, false);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetPort_PassedSearchPassedRules_Port1()
        {
            var testBits = "1000".ToBitArray();
            var searchResults = new SearchResults
            {
                FailedRules = false,
                FailedSearch = false,
                IncomingMask = "1100".ToBitArray(),
                TestResultsBits = new List<BitArray> { testBits },
                RulesResultsBits = "1000".ToBitArray(),
            };

            var result = this.GetPort(searchResults);
            Assert.AreEqual(1, result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetPort_FailedSearch_Port3()
        {
            var testBits = "1100".ToBitArray();
            var searchResults = new SearchResults
            {
                FailedRules = false,
                FailedSearch = true,
                IncomingMask = "1000".ToBitArray(),
                TestResultsBits = new List<BitArray> { testBits },
                RulesResultsBits = "1100".ToBitArray(),
            };

            var result = this.GetPort(searchResults);
            Assert.AreEqual(3, result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetPort_FailedSearch_Port0()
        {
            var testBits = "1111".ToBitArray();
            var searchResults = new SearchResults
            {
                FailedRules = true,
                FailedSearch = true,
                IncomingMask = "1000".ToBitArray(),
                TestResultsBits = new List<BitArray> { testBits },
                RulesResultsBits = "1111".ToBitArray(),
            };

            var result = this.GetPort(searchResults);
            Assert.AreEqual(0, result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetPort_FailedRules_Port2()
        {
            var testBits = "1100".ToBitArray();
            var searchResults = new SearchResults
            {
                FailedRules = true,
                FailedSearch = false,
                IncomingMask = "1000".ToBitArray(),
                TestResultsBits = new List<BitArray> { testBits },
                RulesResultsBits = "1100".ToBitArray(),
            };

            var result = this.GetPort(searchResults);
            Assert.AreEqual(2, result);
        }
    }
}