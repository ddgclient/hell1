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

namespace Prime.TestMethods.VminSearch.UnitTest
{
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FunctionalService;
    using Prime.TestMethods.VminSearch;

    /// <summary>
    /// UnitTest class.
    /// </summary>
    [TestClass]
    public class DataLoggerUnitTest
    {
        /// <summary>
        /// Search passed at first execution.
        /// Pattern name map exist, but there is no data so there's no printing of any limiting pattern.
        /// Print per target increments is enabled but number of increment for every target is zero..
        /// </summary>
        [TestMethod]
        public void DatalogSearchResults_EmptySearchState_NoLimitingPatterns_NoPerTargetIncrements()
        {
            var resultVoltages = new List<double>() { 0.5, 0.5, 0.5 };
            var patternData = new List<SearchPointData>() { new SearchPointData(resultVoltages, new SearchPointData.PatternData("patternName", 1, 1)) };
            var searchIdentifiers = new SearchIdentifiers(string.Empty, 1U, 1U);

            var funcTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);
            var extensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            extensionsMock.Setup(x => x.GetFunctionalTest("patlist", "levels", "timings", "prePlist")).Returns(funcTestMock.Object);
            extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(false);
            var pointTest = new SearchStateValues
            {
                Voltages = new List<double>() { 0.5, 0.5, 0.5 },
                StartVoltages = new List<double>() { 0.5, 0.5, 0.5 },
                EndVoltageLimits = new List<double>() { 1.0, 1.0, 1.0 },
                ExecutionCount = 1U,
                PerTargetIncrements = new List<uint>() { 0, 0, 0 },
                PerPointData = patternData,
                MaskBits = new BitArray(4, true),
            };
            var searchResultData = new SearchResultData(pointTest, true, searchIdentifiers);

            var searchResultList = new List<SearchResultData>() { searchResultData };

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var loggingFormatMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);

            const string outputVoltages = "0.500_0.500_0.500|0.500_0.500_0.500|1.000_1.000_1.000|1";
            const string outputPerTargetIncrements = "0_0_0";
            loggingFormatMock.Setup(writer => writer.SetData(outputVoltages));
            loggingFormatMock.Setup(writer => writer.SetTnamePostfix("_it"));
            loggingFormatMock.Setup(writer => writer.SetData(outputPerTargetIncrements));

            loggingFormatMock.Setup(writer => writer.SetTnamePostfix("_lp"));
            loggingFormatMock.Setup(writer => writer.SetData("att^att^att"));

            datalogServiceMock.Setup(datalogService => datalogService.GetItuffStrgvalWriter()).Returns(loggingFormatMock.Object);
            datalogServiceMock.Setup(datalogService => datalogService.WriteToItuff(loggingFormatMock.Object));
            Services.DatalogService = datalogServiceMock.Object;

            DataLogger.PrintResultsForAllSearches(searchResultList, "1,2,3", true);
            datalogServiceMock.VerifyAll();
            loggingFormatMock.VerifyAll();
        }

        /// <summary>
        /// Complete search with multiple executions. Search contains mask voltages and failing voltages.
        /// Search state identifiers is empty as there is only one multi pass and one repetition.
        /// Pattern name map defined. Printing of the limiting patterns occurs for every target except the ones with mask voltage values.
        /// Printing of the target increments is enabled.
        /// </summary>
        [TestMethod]
        public void DatalogSearchResults_MultipleIterationsWithMaskVoltagesAndFailVoltages_PrintLimitingPatterns_NoPerTargetIncrements()
        {
            var firstPointData = new SearchPointData(new List<double>() { 0.5, 0.5, -8888, 0.5 }, new SearchPointData.PatternData("298746_my_pattern_name_87263470", 1, 1));
            var secondPointData = new SearchPointData(new List<double>() { 0.6, 0.5, -8888, 0.6 }, new SearchPointData.PatternData("685748_my_pattern_name_12456985", 2, 2));
            var thirdPointData = new SearchPointData(new List<double>() { 0.6, 0.5, -8888, 0.6 }, new SearchPointData.PatternData("635435_my_pattern_name_86541276", 3, 3));
            var fourthPointData = new SearchPointData(new List<double>() { 0.7, 0.5, -8888, 0.7 }, new SearchPointData.PatternData("367458_my_pattern_name_17869520", 4, 4));
            var fifthPointData = new SearchPointData(new List<double>() { 0.7, 0.5, -8888, -9999 }, new SearchPointData.PatternData("504625_my_pattern_name_40695750", 5, 5));

            var patternData = new List<SearchPointData>() { firstPointData, secondPointData, thirdPointData, fourthPointData, fifthPointData };
            var searchIdentifiers = new SearchIdentifiers(string.Empty, 1U, 1U);

            var funcTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);
            var extensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            extensionsMock.Setup(x => x.GetFunctionalTest("patlist", "levels", "timings", "prePlist")).Returns(funcTestMock.Object);
            extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(false);
            var pointTest = new SearchStateValues
            {
                Voltages = new List<double>() { 0.7, 0.5, -8888, -9999 },
                StartVoltages = new List<double>() { 0.5, 0.5, 0.5, 0.5 },
                EndVoltageLimits = new List<double>() { 0.8, 0.8, 0.8, 0.8 },
                ExecutionCount = 5U,
                PerTargetIncrements = new List<uint>() { 2, 0, 0, 2 },
                PerPointData = patternData,
                MaskBits = new BitArray(4, true),
            };
            var searchResult = new SearchResultData(pointTest, false, searchIdentifiers);

            var searchResults = new List<SearchResultData> { searchResult };

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var loggingFormatMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);

            const string outputVoltages = "0.700_0.500_-8888_-9999|0.500_0.500_0.500_0.500|0.800_0.800_0.800_0.800|5";
            const string limitingPatterns = "635435^na^na^367458";
            const string outputPerTargetIncrements = "2_0_0_2";
            loggingFormatMock.Setup(writer => writer.SetData(outputVoltages));
            loggingFormatMock.Setup(writer => writer.SetTnamePostfix("_lp"));
            loggingFormatMock.Setup(writer => writer.SetData(limitingPatterns));
            loggingFormatMock.Setup(writer => writer.SetTnamePostfix("_it"));
            loggingFormatMock.Setup(writer => writer.SetData(outputPerTargetIncrements));

            datalogServiceMock.Setup(datalogService => datalogService.GetItuffStrgvalWriter()).Returns(loggingFormatMock.Object);
            datalogServiceMock.Setup(datalogService => datalogService.WriteToItuff(loggingFormatMock.Object));
            Services.DatalogService = datalogServiceMock.Object;

            DataLogger.PrintResultsForAllSearches(searchResults, "0,1,2,3,4,5", true);
            datalogServiceMock.VerifyAll();
            loggingFormatMock.VerifyAll();
        }

        /// <summary>
        /// Logs the individual search results.
        /// Two complete searches due to multi pass. None of this searches contains mask targets or failing targets.
        /// Search state identifiers are not empty as there is more than one multi pass.
        /// Pattern name map defined. Printing of the limiting patterns occurs for every target except the ones with mask voltage values.
        /// Printing of the target increments is enabled.
        /// </summary>
        [TestMethod]
        public void DatalogSearchResults_MultipleSearches_MultipleIterations_NoLimitingPatterns_NoPerTargetIncrements()
        {
            var firstPointData = new SearchPointData(new List<double>() { 0.5, 0.5, 0.5, 0.5 }, new SearchPointData.PatternData("298746_my_pattern_name_87263470", 1, 1));
            var secondPointData = new SearchPointData(new List<double>() { 0.6, 0.5, 0.6, 0.5 }, new SearchPointData.PatternData("685748_my_pattern_name_12456985", 2, 2));
            var thirdPointData = new SearchPointData(new List<double>() { 0.7, 0.6, 0.7, 0.5 }, new SearchPointData.PatternData("635435_my_pattern_name_86541276", 3, 3));
            var fourthPointData = new SearchPointData(new List<double>() { 0.8, 0.7, 0.8, 0.6 }, new SearchPointData.PatternData("367458_my_pattern_name_17869520", 4, 4));

            var otherFirstPointData = new SearchPointData(new List<double>() { 0.5, 0.5, 0.5, 0.5 }, new SearchPointData.PatternData("298746_my_pattern_name_87263470", 1, 1));
            var otherSecondPointData = new SearchPointData(new List<double>() { 0.6, 0.6, 0.6, 0.5 }, new SearchPointData.PatternData("685748_my_pattern_name_12456985", 2, 2));
            var otherThirdPointData = new SearchPointData(new List<double>() { 0.6, 0.7, 0.7, 0.5 }, new SearchPointData.PatternData("635435_my_pattern_name_86541276", 3, 3));
            var otherFourthPointData = new SearchPointData(new List<double>() { 0.6, 0.8, 0.7, 0.6 }, new SearchPointData.PatternData("367458_my_pattern_name_17869520", 4, 4));

            var funcTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);
            var extensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            extensionsMock.Setup(x => x.GetFunctionalTest("patlist", "levels", "timings", "prePlist")).Returns(funcTestMock.Object);
            extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(false);
            var patternData = new List<SearchPointData>() { firstPointData, secondPointData, thirdPointData, fourthPointData };
            var pointTest = new SearchStateValues
            {
                Voltages = new List<double>() { 0.8, 0.7, 0.8, 0.6 },
                StartVoltages = new List<double>() { 0.5, 0.5, 0.5, 0.5 },
                EndVoltageLimits = new List<double>() { 1.0, 1.0, 1.0, 1.0 },
                ExecutionCount = 4U,
                PerPointData = patternData,
                MaskBits = new BitArray(4, true),
                PerTargetIncrements = new List<uint>() { 0, 0, 0, 0 },
            };
            var otherPatternData = new List<SearchPointData>() { otherFirstPointData, otherSecondPointData, otherThirdPointData, otherFourthPointData };
            var otherPointTest = new SearchStateValues
            {
                Voltages = new List<double>() { 0.6, 0.8, 0.7, 0.6 },
                StartVoltages = new List<double>() { 0.5, 0.5, 0.5, 0.5 },
                EndVoltageLimits = new List<double>() { 1.0, 1.0, 1.0, 1.0 },
                ExecutionCount = 4U,
                PerPointData = otherPatternData,
                MaskBits = new BitArray(4, true),
                PerTargetIncrements = new List<uint>() { 0, 0, 0, 0 },
            };

            var searchIdentifiers = new SearchIdentifiers("M1R1", 1U, 1U);
            var searchResult = new SearchResultData(pointTest, false, searchIdentifiers);
            var otherSearchIdentifiers = new SearchIdentifiers("M2R1", 2U, 1U);
            var otherSearchResult = new SearchResultData(otherPointTest, false, otherSearchIdentifiers);

            var searchResultList = new List<SearchResultData> { searchResult, otherSearchResult };

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var loggingFormatMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);

            const string outputVoltages = "0.800_0.700_0.800_0.600|0.500_0.500_0.500_0.500|1.000_1.000_1.000_1.000|4";
            const string otherOutputVoltages = "0.600_0.800_0.700_0.600|0.500_0.500_0.500_0.500|1.000_1.000_1.000_1.000|4";
            loggingFormatMock.Setup(writer => writer.SetTnamePostfix("_" + searchIdentifiers.TnamePostfix));
            loggingFormatMock.Setup(writer => writer.SetData(outputVoltages));
            loggingFormatMock.Setup(writer => writer.SetTnamePostfix("_" + otherSearchIdentifiers.TnamePostfix));
            loggingFormatMock.Setup(writer => writer.SetData(otherOutputVoltages));

            datalogServiceMock.Setup(datalogService => datalogService.GetItuffStrgvalWriter()).Returns(loggingFormatMock.Object);
            datalogServiceMock.Setup(datalogService => datalogService.WriteToItuff(loggingFormatMock.Object));
            Services.DatalogService = datalogServiceMock.Object;

            DataLogger.PrintResultsForAllSearches(searchResultList, string.Empty, false);
            datalogServiceMock.VerifyAll();
            loggingFormatMock.VerifyAll();
        }

        /// <summary>
        /// Logs the joined multi-pass search results with overlapping between failing voltages and passing voltages.
        /// The final execution count is the sum of the execution count for every search result.
        /// Pattern name map defined. Printing of the limiting patterns occurs for every target except the ones with mask voltage values.
        /// Printing of the per target increments is enabled.
        /// </summary>
        [TestMethod]
        public void DatalogMultiPassOverlapFailVoltages_PrintLimitingPatterns_PrintPerTargetIncrements()
        {
            var firstPointData = new SearchPointData(new List<double>() { 0.3, 0.3, 0.5, -8888 }, new SearchPointData.PatternData("pat1", 1, 1));
            var secondPointData = new SearchPointData(new List<double>() { 0.4, 0.4, 0.6, -8888 }, new SearchPointData.PatternData("pat2", 2, 2));
            var thirdPointData = new SearchPointData(new List<double>() { 0.4, 0.4, 0.7, -8888 }, new SearchPointData.PatternData("pat3", 3, 3));
            var fourthPointData = new SearchPointData(new List<double>() { 0.4, 0.5, 0.7, -8888 }, new SearchPointData.PatternData("pat4", 4, 4));

            var otherFirstPointData = new SearchPointData(new List<double>() { -8888, 0.3, 0.5, 0.5 }, new SearchPointData.PatternData("pat5", 5, 5));
            var otherSecondPointData = new SearchPointData(new List<double>() { -8888, 0.4, 0.6, 0.5 }, new SearchPointData.PatternData("pat6", 6, 6));
            var otherThirdPointData = new SearchPointData(new List<double>() { -8888, 0.5, 0.7, 0.6 }, new SearchPointData.PatternData("pat7", 7, 7));
            var otherFourthPointData = new SearchPointData(new List<double>() { -8888, 0.5, -9999, 0.6 }, new SearchPointData.PatternData("pat8", 8, 8));

            var funcTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);
            var extensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            extensionsMock.Setup(x => x.GetFunctionalTest("patlist", "levels", "timings", "prePlist")).Returns(funcTestMock.Object);
            extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(false);
            var patternData = new List<SearchPointData>() { firstPointData, secondPointData, thirdPointData, fourthPointData };
            var pointTest = new SearchStateValues
            {
                Voltages = new List<double>() { 0.4, 0.5, 0.7, -8888 },
                StartVoltages = new List<double>() { 0.3, 0.3, 0.5, 0.5 },
                EndVoltageLimits = new List<double>() { 0.8, 0.8, 0.8, 0.8 },
                ExecutionCount = 4U,
                PerTargetIncrements = new List<uint>() { 1, 2, 2, 0 },
                PerPointData = patternData,
                MaskBits = new BitArray(4, true),
            };
            var otherPatternData = new List<SearchPointData>() { otherFirstPointData, otherSecondPointData, otherThirdPointData, otherFourthPointData };
            var otherPointTest = new SearchStateValues
            {
                Voltages = new List<double>() { -8888, 0.5, -9999, 0.6 },
                StartVoltages = new List<double>() { 0.3, 0.3, 0.5, 0.5 },
                EndVoltageLimits = new List<double>() { 0.8, 0.8, 0.8, 0.8 },
                ExecutionCount = 4U,
                PerTargetIncrements = new List<uint>() { 0, 2, 3, 1 },
                PerPointData = otherPatternData,
                MaskBits = new BitArray(4, true),
            };

            var searchIdentifiers = new SearchIdentifiers("M1R1", 1U, 1U);
            var searchResult = new SearchResultData(pointTest, false, searchIdentifiers);
            var otherSearchIdentifiers = new SearchIdentifiers("M2R1", 2U, 1U);
            var otherSearchResult = new SearchResultData(otherPointTest, false, otherSearchIdentifiers);

            var searchResultList = new List<SearchResultData> { searchResult, otherSearchResult };
            const string patternNameMap = "0,1,2,3";

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var loggingFormatMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);

            const string outputVoltages = "0.400_0.500_-9999_0.600|0.300_0.300_0.500_0.500|0.800_0.800_0.800_0.800|8";
            const string outputLimitingPatterns = "pat1^pat6^pat7^pat6";
            const string outputPerTargetIncrements = "1_2_3_1";
            loggingFormatMock.Setup(writer => writer.SetData(outputVoltages));
            loggingFormatMock.Setup(writer => writer.SetTnamePostfix("_lp"));
            loggingFormatMock.Setup(writer => writer.SetData(outputLimitingPatterns));
            loggingFormatMock.Setup(writer => writer.SetTnamePostfix("_it"));
            loggingFormatMock.Setup(writer => writer.SetData(outputPerTargetIncrements));

            datalogServiceMock.Setup(datalogService => datalogService.GetItuffStrgvalWriter()).Returns(loggingFormatMock.Object);
            datalogServiceMock.Setup(datalogService => datalogService.WriteToItuff(loggingFormatMock.Object));
            Services.DatalogService = datalogServiceMock.Object;

            DataLogger.PrintMergedSearchResults(searchResultList, patternNameMap, true);
            datalogServiceMock.VerifyAll();
            loggingFormatMock.VerifyAll();
        }

        /// <summary>
        /// Logging configuration data for Trace.
        /// </summary>
        [TestMethod]
        public void Datalog_LogForTrace_Pass()
        {
            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var loggingFormatMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);

            loggingFormatMock.Setup(flog => flog.SetTnamePostfix("_vminFwCfg"));

            loggingFormatMock.Setup(flog => flog.SetData("CRCORE0@F6:3:5.500_CRCORE1@F6:3:5.500_CRCORE2@F6:3:5.500"));

            datalogServiceMock.Setup(dlog => dlog.GetItuffStrgvalWriter()).Returns(loggingFormatMock.Object);
            datalogServiceMock.Setup(dlog => dlog.WriteToItuff(loggingFormatMock.Object));

            Prime.Services.DatalogService = datalogServiceMock.Object;

            List<string> inputCorners = new List<string>() { "CR_CORE0@F6", "CR_CORE1@F6", "CR_CORE2@F6" };
            int flowId = 3;
            DataLogger.LogVminForwardingDataForTrace(5500000000, flowId, inputCorners);

            datalogServiceMock.VerifyAll();
            loggingFormatMock.VerifyAll();
        }

        /// <summary>
        /// Logs the search results for the last repetition.
        /// Pattern name map empty. No printing of the limiting patterns occurs.
        /// Printing of the per target increments is disabled.
        /// </summary>
        [TestMethod]
        public void DatalogFinalRepetitionSearchResults_NoLimitingPattern_NoPerTargetIncrements()
        {
            var firstPointData = new SearchPointData(new List<double>() { 0.3, 0.3, 0.5, -8888 }, new SearchPointData.PatternData("pat1", 1, 1));
            var secondPointData = new SearchPointData(new List<double>() { 0.4, 0.4, 0.6, -8888 }, new SearchPointData.PatternData("pat2", 2, 2));
            var thirdPointData = new SearchPointData(new List<double>() { 0.4, 0.4, 0.7, -8888 }, new SearchPointData.PatternData("pat3", 3, 3));
            var fourthPointData = new SearchPointData(new List<double>() { 0.4, 0.5, 0.7, -8888 }, new SearchPointData.PatternData("pat4", 4, 4));

            var otherFirstPointData = new SearchPointData(new List<double>() { -8888, 0.3, 0.5, 0.5 }, new SearchPointData.PatternData("pat5", 5, 5));
            var otherSecondPointData = new SearchPointData(new List<double>() { -8888, 0.4, 0.6, 0.5 }, new SearchPointData.PatternData("pat6", 6, 6));
            var otherThirdPointData = new SearchPointData(new List<double>() { -8888, 0.5, 0.7, 0.6 }, new SearchPointData.PatternData("pat7", 7, 7));
            var otherFourthPointData = new SearchPointData(new List<double>() { -8888, 0.5, -9999, 0.6 }, new SearchPointData.PatternData("pat8", 8, 8));

            var funcTestMock = new Mock<IFunctionalTest>(MockBehavior.Strict);
            var extensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            extensionsMock.Setup(x => x.GetFunctionalTest("patlist", "levels", "timings", "prePlist")).Returns(funcTestMock.Object);
            extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(false);
            var patternData = new List<SearchPointData>() { firstPointData, secondPointData, thirdPointData, fourthPointData };
            var pointTest = new SearchStateValues
            {
                Voltages = new List<double>() { 0.4, 0.5, 0.7, -8888 },
                StartVoltages = new List<double>() { 0.3, 0.3, 0.5, 0.5 },
                EndVoltageLimits = new List<double>() { 0.5, 0.5, 0.7, 0.7 },
                ExecutionCount = 4U,
                PerTargetIncrements = new List<uint>() { 1, 2, 2, 0 },
                PerPointData = patternData,
                MaskBits = new BitArray(4, true),
            };
            var otherPatternData = new List<SearchPointData>() { otherFirstPointData, otherSecondPointData, otherThirdPointData, otherFourthPointData };
            var otherPointTest = new SearchStateValues
            {
                Voltages = new List<double>() { -8888, 0.5, -9999, 0.6 },
                StartVoltages = new List<double>() { 0.3, 0.3, 0.5, 0.5 },
                EndVoltageLimits = new List<double>() { 0.5, 0.5, 0.7, 0.7 },
                ExecutionCount = 4U,
                PerTargetIncrements = new List<uint>() { 0, 2, 3, 1 },
                PerPointData = otherPatternData,
                MaskBits = new BitArray(4, true),
            };

            var searchIdentifiers = new SearchIdentifiers("M1R1", 1U, 1U);
            var searchResult = new SearchResultData(pointTest, false, searchIdentifiers);
            var otherSearchIdentifiers = new SearchIdentifiers("M1R2", 1U, 2U);
            var otherSearchResult = new SearchResultData(otherPointTest, false, otherSearchIdentifiers);

            var searchResultList = new List<SearchResultData> { searchResult, otherSearchResult };

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var loggingFormatMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);

            const string outputVoltages = "0.400_0.500_-9999_0.600|0.300_0.300_0.500_0.500|0.500_0.500_0.700_0.700|8";
            loggingFormatMock.Setup(writer => writer.SetData(outputVoltages));

            datalogServiceMock.Setup(datalogService => datalogService.GetItuffStrgvalWriter()).Returns(loggingFormatMock.Object);
            datalogServiceMock.Setup(datalogService => datalogService.WriteToItuff(loggingFormatMock.Object));
            Services.DatalogService = datalogServiceMock.Object;

            DataLogger.PrintMergedSearchResults(searchResultList, string.Empty, false);
            datalogServiceMock.VerifyAll();
            loggingFormatMock.VerifyAll();
        }
    }
}
