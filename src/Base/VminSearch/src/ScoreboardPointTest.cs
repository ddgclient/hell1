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

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Prime.TestMethods.VminSearch.UnitTest")]

namespace Prime.TestMethods.VminSearch
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PlistService;
    using Prime.ScoreboardService;
    using Prime.TestMethods.VminSearch.Helpers;
    using Prime.VoltageService;

    /// <summary>
    /// Class to handle all accumulated data for scoreboard.
    /// </summary>
    internal class ScoreboardPointTest
    {
        private const int MinimalIterations = 2;

        private readonly IConsoleService console;
        private readonly ICaptureFailureTest captureTest;
        private readonly IPlistObject plist;
        private readonly IVoltage voltageTest;
        private readonly IVminSearchExtensions vminSearchExtensions;
        private readonly List<IScoreboardLogger> loggers;
        private readonly List<int> baseNumbers;
        private readonly ulong edgeTicks;
        private readonly bool printScoreboardCounters;
        private List<double> voltages;
        private BitArray maskBits;
        private SearchPointData.PatternData failPatternData;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScoreboardPointTest"/> class.
        /// </summary>
        /// <param name="plistExecutionParameters">Functional object for plist execution options.</param>
        /// <param name="scoreboardExecutionParameters">Scoreboard execution options.</param>
        /// <param name="voltageTest">Object of class which allows to apply the voltages.</param>
        /// <param name="extensions"> Object of class which has all extension methods implemented.</param>
        /// <param name="console">IConsoleService reference.</param>
        public ScoreboardPointTest(
            PrimeVminSearchTestMethod.PlistExecutionParameters plistExecutionParameters,
            ScoreboardExecutionParameters scoreboardExecutionParameters,
            IVoltage voltageTest,
            IVminSearchExtensions extensions,
            IConsoleService console)
        {
            var localMaxFails = scoreboardExecutionParameters.MaxFails == 0 ? ulong.MaxValue : scoreboardExecutionParameters.MaxFails;
            this.captureTest = Services.FunctionalService.CreateCaptureFailureTest(plistExecutionParameters.Patlist, plistExecutionParameters.LevelsTc, plistExecutionParameters.TimingsTc, localMaxFails, 1, plistExecutionParameters.PrePlist);
            this.plist = Services.PlistService.GetPlistObject(plistExecutionParameters.Patlist);
            this.voltageTest = voltageTest;
            this.edgeTicks = scoreboardExecutionParameters.EdgeTicks;
            this.vminSearchExtensions = extensions;
            this.baseNumbers = scoreboardExecutionParameters.BaseNumbers;
            this.loggers = new List<IScoreboardLogger>(this.baseNumbers.Count);
            foreach (var baseNumber in this.baseNumbers)
            {
                this.loggers.Add(Prime.Services.ScoreBoardService.CreateLogger(baseNumber, scoreboardExecutionParameters.PatternNameMap, localMaxFails));
            }

            this.printScoreboardCounters = scoreboardExecutionParameters.PrintScoreboardCounters;
            this.console = console;
        }

        private enum ScoreboardMode
        {
            ScoreboardFailingValues,
            ScoreboardPassingValues,
            SkipScoreboard,
        }

        /// <summary>
        /// Execute sb.
        /// </summary>
        /// <param name="searchPointData">Accumulated results for each iteration in the search.</param>
        /// <param name="initialMasks">Bit array containing the initial mask for each target. True indicates the bit is on and False indicates the bit is off.</param>>
        /// <param name="testNamePostfix">Test instance name postfix.</param>
        /// <param name="searchResult">True if the search passed, false if it failed.</param>
        public void Execute(List<SearchPointData> searchPointData, BitArray initialMasks, string testNamePostfix, bool searchResult)
        {
            this.maskBits = initialMasks;
            if (!this.DefineScoreboardSetup(searchPointData, searchResult))
            {
                return;
            }

            this.console?.PrintDebug($"VminSearchSC: ScoreboardPoint setup:\nVminSearchSC:\tVoltages=[{string.Join(",", this.voltages)}]\n" +
                                     $"VminSearchSC:\tStart pattern=[{(string.IsNullOrEmpty(this.failPatternData.PatternName) ? string.Empty : this.failPatternData.PatternName)}]\n" +
                                     $"VminSearchSC:\tMask=[{this.maskBits.ToStr()}]");

            this.vminSearchExtensions.ApplySearchVoltage(this.voltageTest, this.voltages);
            if (!string.IsNullOrEmpty(this.failPatternData.PatternName) && !this.plist.IsPatternAnAmble(this.failPatternData.PatternName))
            {
                this.captureTest.SetStartPattern(this.failPatternData.PatternName, this.failPatternData.BurstIndex, this.failPatternData.PatternId);
            }
            else
            {
                this.captureTest.Reset();
            }

            this.vminSearchExtensions.ApplyMask(this.maskBits, this.captureTest);
            if (this.captureTest.ExecuteWithInlineRv())
            {
                return;
            }

            this.GenerateScoreboardCounters(testNamePostfix, this.captureTest);
        }

        /// <summary>
        /// Generates and print the Scoreboard Counters.
        /// </summary>
        /// <param name="testNamePostfix">testName postfix for ituff printing.</param>
        /// <param name="captureFailureTest">Functional Failure test instance.</param>
        public void GenerateScoreboardCounters(string testNamePostfix, ICaptureFailureTest captureFailureTest)
        {
            var failData = captureFailureTest.GetPerCycleFailures().Select(x => x.GetPatternName()).ToList();
            this.console?.PrintDebug($"VminSearchSC: Processing Scoreboard data for [{failData.Count}] failures");
            if (failData.Count > 0)
            {
                foreach (var logger in this.loggers)
                {
                    logger.ProcessFailData(failData);

                    if (this.printScoreboardCounters)
                    {
                        logger.PrintCountersToItuff(testNamePostfix);
                    }
                }
            }
        }

        /// <summary>
        /// Resolves plist for execution. If PUP methodology is enabled it might replace the original plist with the slim plist version.
        /// This is done in the functional service.
        /// </summary>
        /// <param name="instanceName">currently executed instance name to check if it is targeted for PUP.</param>
        public void ResolvePlist(string instanceName)
        {
            this.captureTest.ResolvePlist(instanceName);
        }

        private ScoreboardMode GetScoreboardMode()
        {
            var isThereAnyFailVoltage = this.voltages.Any(voltage => voltage.IsEqual(SearchPointTest.VoltageFailValue));
            if (isThereAnyFailVoltage)
            {
                return ScoreboardMode.ScoreboardFailingValues;
            }

            return this.edgeTicks > 0 ? ScoreboardMode.ScoreboardPassingValues : ScoreboardMode.SkipScoreboard;
        }

        private bool DefineScoreboardSetup(List<SearchPointData> searchPointData, bool searchResult)
        {
            if (searchPointData.Count < MinimalIterations || this.baseNumbers.Count == 0)
            {
                return false;
            }

            if (!searchResult)
            {
                var patternData = new SearchPointData.PatternData(string.Empty, 0, 0);
                var failVoltages = new List<double>(Enumerable.Repeat(SearchPointTest.VoltageFailValue, searchPointData.Last().Voltages.Count));
                var pointData = new SearchPointData(failVoltages, patternData);
                searchPointData.Add(pointData);
            }

            this.voltages = new List<double>(searchPointData.Last().Voltages);

            var scoreboardMode = this.GetScoreboardMode();
            if (scoreboardMode != ScoreboardMode.ScoreboardFailingValues)
            {
                return scoreboardMode == ScoreboardMode.ScoreboardPassingValues && this.DefineSetupForPassingValues(searchPointData);
            }

            this.DefineSetupForFailingValues(searchPointData);
            return true;
        }

        private void DefineSetupForFailingValues(IReadOnlyList<SearchPointData> searchPointData)
        {
            var maxVoltage = 0.0;
            var failPatternIteration = searchPointData.Count;
            var lastVoltages = new List<double>(searchPointData.Last().Voltages);
            for (var target = 0; target < this.voltages.Count; target++)
            {
                if (lastVoltages[target].IsEqual(SearchPointTest.VoltageFailValue))
                {
                    for (var iteration = searchPointData.Count - 2; iteration >= 0; iteration--)
                    {
                        if (searchPointData[iteration].Voltages[target] > 0)
                        {
                            this.voltages[target] = searchPointData[iteration].Voltages[target];
                            if (iteration < failPatternIteration)
                            {
                                this.failPatternData = searchPointData[iteration].FailPatternData;
                                failPatternIteration = iteration;
                            }

                            if (this.voltages[target] > maxVoltage)
                            {
                                maxVoltage = this.voltages[target];
                            }

                            break;
                        }
                    }
                }
            }

            this.SetMaxVoltageToPassingValues(lastVoltages, maxVoltage);
        }

        private void SetMaxVoltageToPassingValues(IReadOnlyList<double> lastVoltages, double maxVoltage)
        {
            for (var target = 0; target < this.voltages.Count; target++)
            {
                if (lastVoltages[target] > 0)
                {
                    this.voltages[target] = maxVoltage;
                    this.maskBits[target] = true;
                }
            }
        }

        private bool DefineSetupForPassingValues(IReadOnlyList<SearchPointData> searchPointData)
        {
            var edgeTicksPerTarget = Enumerable.Repeat(this.edgeTicks, this.voltages.Count).ToList();
            var failPatternIterationPerTarget = Enumerable.Repeat(0, this.voltages.Count).ToList();
            for (var target = 0; target < this.voltages.Count; target++)
            {
                for (var iteration = searchPointData.Count - 2; iteration >= 0; iteration--)
                {
                    var isVoltageSwitch = searchPointData[iteration].Voltages[target].IsDifferent(this.voltages[target]);
                    var isEdgeTicksZero = edgeTicksPerTarget[target] == 0;
                    if (isVoltageSwitch && !isEdgeTicksZero)
                    {
                        this.voltages[target] = searchPointData[iteration].Voltages[target];
                        failPatternIterationPerTarget[target] = iteration;
                        edgeTicksPerTarget[target]--;
                    }

                    if (iteration == 0 && edgeTicksPerTarget[target] > 0)
                    {
                        this.maskBits[target] = true;
                        this.voltages[target] = searchPointData.Last().Voltages[target];
                    }
                }
            }

            var targetIteration = searchPointData.Count;
            var isScoreboardPossible = false;
            for (var target = 0; target < this.voltages.Count; target++)
            {
                if (this.maskBits[target])
                {
                    continue;
                }

                isScoreboardPossible = true;
                if (failPatternIterationPerTarget[target] >= targetIteration)
                {
                    continue;
                }

                targetIteration = failPatternIterationPerTarget[target];
                this.failPatternData = searchPointData[targetIteration].FailPatternData;
            }

            return isScoreboardPossible;
        }

        /// <summary>
        /// Scoreboard point test execution settings.
        /// </summary>
        internal struct ScoreboardExecutionParameters
        {
            /// <summary>
            /// Number of fails before the maximum number of fails is reported.
            /// </summary>
            public ulong MaxFails;

            /// <summary>
            /// Number of resolutions to step down.
            /// </summary>
            public ulong EdgeTicks;

            /// <summary>
            /// Integer number value to prefix the generated scoreboard fail counters.
            /// </summary>
            public List<int> BaseNumbers;

            /// <summary>
            /// A comma separated string of integers which map characters in the pattern name to produce a scoreboard counter.
            /// </summary>
            public string PatternNameMap;

            /// <summary>
            /// Indicates whether or not to print the scoreboard counters of the current instance object.
            /// </summary>
            public bool PrintScoreboardCounters;
        }
    }
}