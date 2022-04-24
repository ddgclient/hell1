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

namespace ImpactStudiesVmin
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using DDG;
    using Prime;
    using Prime.Base.Exceptions;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PlistService;
    using Prime.TestMethods;
    using Prime.TestMethods.CommonParams;
    using Prime.TestMethods.VminSearch;
    using VminTC;

    /// <summary>
    /// Defines the <see cref="VminSearch" />.
    /// </summary>
    public class VminSearch : VminTC, IVminSearchExtensions, IPreInstanceCommonParam, IPostInstanceCommonParam, IPatConfigSetPointsCommonParam
    {
        /// <inheritdoc />
        public TestMethodsParams.String PreInstance { get; set; } = string.Empty;

        /// <inheritdoc />
        public TestMethodsParams.String PostInstance { get; set; } = string.Empty;

        /// <inheritdoc />
        public TestMethodsParams.String SetPointsPlistParamName { get; set; } = string.Empty;

        /// <inheritdoc />
        public PlistMode SetPointsPlistMode { get; set; } = PlistMode.Local;

        /// <inheritdoc />
        public TestMethodsParams.String SetPointsRegEx { get; set; } = string.Empty;

        /// <inheritdoc />
        public TestMethodsParams.String SetPointsPreInstance { get; set; } = string.Empty;

        /// <inheritdoc />
        public TestMethodsParams.String SetPointsPostInstance { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sests a Map of pin names to IP. Made internal for easer unit testing.
        /// </summary>
        public Dictionary<string, string> PinToCoreMap { get; private set; } = null;

        /// <summary>
        /// Gets the PatConfigSetPointHandles for executing PreTest with supplied data.
        /// </summary>
        public virtual List<Tuple<IPatConfigSetPointHandle, string>> PreConfigSetPointsWithData { get; private set; } = new List<Tuple<IPatConfigSetPointHandle, string>>();

        /// <summary>
        /// Gets the PatConfigSetPointHandles for executing PreTest with default data.
        /// </summary>
        public virtual List<IPatConfigSetPointHandle> PreConfigSetPointsWithDefault { get; private set; } = new List<IPatConfigSetPointHandle>();

        /// <summary>
        /// Gets the PatConfigSetPointHandles for executing PostTest with supplied data.
        /// </summary>
        public virtual List<Tuple<IPatConfigSetPointHandle, string>> PostConfigSetPointsWithData { get; private set; } = new List<Tuple<IPatConfigSetPointHandle, string>>();

        /// <summary>
        /// Gets the PatConfigSetPointHandles for executing PostTest with default data.
        /// </summary>
        public virtual List<IPatConfigSetPointHandle> PostConfigSetPointsWithDefault { get; private set; } = new List<IPatConfigSetPointHandle>();

        /// <summary>
        /// Gets or sets the PreInstanceCallback function and arguments.
        /// </summary>
        public virtual Tuple<string, string> PreInstanceCallback { get; set; } = null;

        /// <summary>
        /// Gets or sets the PostInstanceCallback function and arguments.
        /// </summary>
        public virtual Tuple<string, string> PostInstanceCallback { get; set; } = null;

        private ICaptureFailureTest ScoreboardTest { get; set; }

        private IPlistObject ScoreboardPlist { get; set; }

        /// <summary>
        /// Mockable wrapper around Verify().
        /// </summary>
        public virtual void VerifyWrapper()
        {
            this.Verify();
        }

        /// <inheritdoc />
        public override void CustomVerify()
        {
            // TODO: Figure out how to get unit test coverage on CustomVerify without having to run VminTC.CustomVerify.
            // run the bases verify
            base.CustomVerify();

            // call this childs classes custom verify, broke it out like this to make unit testing easier.
            this.ChildCustomVerify(this.PinMap_);
        }

        /// <inheritdoc/>
        // TODO: Figure out how to get unit test coverage on IVminSearchExtensions.ExecuteScoreboard without executing the full VminTC.Execute.
        void IVminSearchExtensions.ExecuteScoreboard(string executionIdentifier, bool isLastSearchPointPass)
            => this.ExecuteCustomScoreboard(this.PointData, this.PinToCoreMap);

        /// <summary>
        /// CustomVerify for the child class only.
        /// </summary>
        /// <param name="pinmap">Pinmap object to use.</param>
        internal void ChildCustomVerify(IPinMap pinmap)
        {
            // verify the common parameters. since this class special we can't rely on the prime base class to do it.
            this.VerifyCommonParameters();

            // get a map of pins to core (pinMap).
            this.PinToCoreMap = this.BuildPinToCoreMap(pinmap);

            // setup the scoreboard test
            this.ScoreboardTest = Prime.Services.FunctionalService.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 99999, 1000, this.PrePlist);
            this.ScoreboardPlist = Prime.Services.PlistService.GetPlistObject(this.Patlist);
        }

        /// <summary>
        /// Runs a scoreboard test using SearchPointData from the search to calculate which voltages and start pattern to use.
        /// Ideally this would be private, but that would mean having to UnitTest the full "Execute()" which is just too annoying.
        /// </summary>
        /// <param name="searchData">All the search point data.</param>
        /// <param name="pinToCoreMap">Dictionary mapping pins to ip.</param>
        /// <returns>true if scoreboard was run.</returns>
        internal bool ExecuteCustomScoreboard(List<SearchPointData> searchData, Dictionary<string, string> pinToCoreMap)
        {
            // Log the VMin results first. Need to do this so the TestName shows up in the log.
            var startingVoltages = this.TestMethodExtension.GetStartVoltageValues(this.StartVoltages.ToList());
            var endingVoltages = this.TestMethodExtension.GetEndVoltageLimitValues(this.EndVoltageLimits.ToList());
            var finalVoltages = searchData.Last().Voltages;
            this.LogSearchVoltageResultsToItuff(finalVoltages, startingVoltages, endingVoltages, (uint)searchData.Count, this.InstanceName);

            if (searchData == null || searchData.Count < 2)
            {
                this.Console?.PrintDebug($"Search passed on the first test point, no need to run scoreboard.");
                return false;
            }

            this.Console?.PrintDebug($"\n ************ In Custom ExecuteScoreboard ************");
            for (var searchPointIndex = 0; searchPointIndex < searchData.Count; searchPointIndex++)
            {
                this.Console?.PrintDebug($"TestPoint=[{searchPointIndex,2}] Results=[{string.Join(",", searchData[searchPointIndex].Voltages)}] Pattern=[{searchData[searchPointIndex].FailPatternData.PatternName}].");
            }

            // setup the correct voltages for scoreboard.
            var voltageTargets = this.TestMethodExtension.GetSearchVoltageObject(this.VoltageTargets.ToList(), this.Patlist);
            var scoreboardVoltages = this.CalculateScoreboardVoltagesAndStartPattern(searchData, out var startPatternData);
            this.TestMethodExtension.ApplySearchVoltage(voltageTargets, scoreboardVoltages);

            // run the test.
            this.Console?.PrintDebug($"Running scoreboard at Vcc=[{string.Join(",", scoreboardVoltages)}] with Pattern=[{startPatternData.BurstIndex}/{startPatternData.PatternId}/{startPatternData.PatternName}]");
            var allFailingCycles = this.RunScoreboardTest(startPatternData);

            // Datalog the scoreboard test details.
            this.Console?.PrintDebug($"Found {allFailingCycles.Count} failing cycles.");
            var strgvalWriter = Services.DatalogService.GetItuffStrgvalWriter();
            strgvalWriter.SetTnamePostfix($"|{this.InstanceName}|scb");
            strgvalWriter.SetData($"{string.Join("_", scoreboardVoltages.Select(i => $"{i:F3}"))}|{startPatternData.PatternName}|{startPatternData.BurstIndex}|{startPatternData.PatternId}|{allFailingCycles.Count}");
            Services.DatalogService.WriteToItuff(strgvalWriter);

            Dictionary<string, HashSet<string>> failingPatternPerPin = this.GetFailingPatternPerIP(allFailingCycles, pinToCoreMap);
            this.LogFailingPatternPerIP(failingPatternPerPin);

            return true;
        }

        private void VerifyCommonParameters()
        {
            this.VerifyPatConfigSetpoint(this.SetPointsPreInstance, out var handlesWithDefaultPre, out var handlesWithDataPre);
            this.PreConfigSetPointsWithData = handlesWithDataPre;
            this.PreConfigSetPointsWithDefault = handlesWithDefaultPre;

            this.VerifyPatConfigSetpoint(this.SetPointsPostInstance, out var handlesWithDefaultPost, out var handlesWithDataPost);
            this.PostConfigSetPointsWithData = handlesWithDataPost;
            this.PostConfigSetPointsWithDefault = handlesWithDefaultPost;

            this.PreInstanceCallback = this.VerifyCallback(this.PreInstance);
            this.PostInstanceCallback = this.VerifyCallback(this.PostInstance);
        }

        private Tuple<string, string> VerifyCallback(string callback)
        {
            if (string.IsNullOrWhiteSpace(callback))
            {
                return null;
            }

            // format should be [function(args)] .. but args could contain ().
            callback = callback.Trim();
            var firstParen = callback.IndexOf('(');
            var lastParen = callback.LastIndexOf(')');
            if (firstParen < 0 || lastParen < 0)
            {
                throw new TestMethodException($"Expecting callback to be of the form [func(args)], not [{callback}].");
            }

            var func = callback.Substring(0, firstParen).Trim();
            var args = firstParen == (lastParen - 1) ? string.Empty : callback.Substring(firstParen + 1, lastParen - firstParen - 1).Trim();
            this.Console?.PrintDebug($"Translated Callback=[{callback}] into Function=[{func}] and Args=[{args}].");

            if (!Prime.Services.TestProgramService.DoesCallbackExist(func))
            {
                throw new TestMethodException($"Callback=[{func}] has not been properly registered.");
            }

            return new Tuple<string, string>(func, args);
        }

        private void VerifyPatConfigSetpoint(string setpointList, out List<IPatConfigSetPointHandle> handlesWithDefault, out List<Tuple<IPatConfigSetPointHandle, string>> handlesWithData)
        {
            handlesWithData = new List<Tuple<IPatConfigSetPointHandle, string>>();
            handlesWithDefault = new List<IPatConfigSetPointHandle>();

            if (string.IsNullOrWhiteSpace(setpointList))
            {
                return;
            }

            /* setpointList is a comma separated string of Module:Group[:data] */
            foreach (var setpoint in setpointList.Split(','))
            {
                var c = setpoint.Split(':');
                if (c.Length == 2)
                {
                    // use default data
                    var setPointWithDefault = this.GetPatConfigSetPointHandle(c[0], c[1], this.Patlist, this.SetPointsRegEx);
                    handlesWithDefault.Add(setPointWithDefault);
                }
                else if (c.Length == 3)
                {
                    // use supplied data
                    var setPointWithData = this.GetPatConfigSetPointHandle(c[0], c[1], this.Patlist, this.SetPointsRegEx);
                    handlesWithData.Add(new Tuple<IPatConfigSetPointHandle, string>(setPointWithData, c[2]));
                    if (Prime.Services.PatConfigService.IsSetPointHandleExist(c[0], c[1], c[2]) != SetPointHandleCheckerSymbol.EXIST)
                    {
                        throw new TestMethodException($"PatConfigSetPoint Module=[{c[0]}] Group=[{c[1]}] Does not have SetPoint=[{c[2]}].");
                    }
                }
                else
                {
                    throw new TestMethodException($"Expecting PatConfigSetpoint to be of the form [Module:Group] or [Module:Group:Setpoint], not [{setpoint}].");
                }
            }
        }

        private IPatConfigSetPointHandle GetPatConfigSetPointHandle(string module, string group, string patlist, string regex)
        {
            if (string.IsNullOrWhiteSpace(regex))
            {
                return Prime.Services.PatConfigService.GetSetPointHandle(module, group, patlist);
            }
            else
            {
                return Prime.Services.PatConfigService.GetSetPointHandleWithRegEx(module, group, patlist, regex);
            }
        }

        private Dictionary<string, string> BuildPinToCoreMap(IPinMap pinMap)
        {
            var map = new Dictionary<string, string>();
            var decoders = pinMap.GetConfiguration();
            var fullSize = decoders.Select(o => o.NumberOfTrackerElements).Sum();
            var currentIndex = 0;
            foreach (var decoder in decoders)
            {
                for (var element = 0; element < decoder.NumberOfTrackerElements; element++)
                {
                    var maskBits = new BitArray(fullSize, false);
                    maskBits.Set(currentIndex++, true);
                    var pins = pinMap.GetMaskPins(maskBits, null);
                    foreach (var pin in pins)
                    {
                        map[pin] = decoder.Name;
                    }
                }
            }

            return map;
        }

        private List<double> CalculateScoreboardVoltagesAndStartPattern(List<SearchPointData> searchData, out SearchPointData.PatternData startPatternData)
        {
            var startingVoltages = this.TestMethodExtension.GetStartVoltageValues(this.StartVoltages.ToList());
            var endingVoltages = this.TestMethodExtension.GetEndVoltageLimitValues(this.EndVoltageLimits.ToList());
            var finalVoltages = searchData.Last().Voltages;
            var scoreboardVoltages = new List<double>(finalVoltages.Count);
            var testpointFailPatternIndex = searchData.Count;

            for (int targetIndex = 0; targetIndex < finalVoltages.Count; targetIndex++)
            {
                if (finalVoltages[targetIndex] < 0)
                {
                    scoreboardVoltages.Add(endingVoltages[targetIndex]);
                }
                else
                {
                    var voltage = Math.Max(startingVoltages[targetIndex], finalVoltages[targetIndex] - (this.StepSize * this.ScoreboardEdgeTicks));
                    scoreboardVoltages.Add(voltage);
                }

                // find the last time this target ran at this voltage, that's the pattern we want to use as the starting point.
                var testPointIndex = searchData.FindLastIndex(data => data.Voltages[targetIndex].Equals(scoreboardVoltages.Last(), 3));
                this.Console?.PrintDebug($"Target=[{targetIndex}] ScoreboardVoltage=[{scoreboardVoltages.Last()}] IndexForPattern=[{testPointIndex}]");
                if (testPointIndex >= 0 && testPointIndex < testpointFailPatternIndex)
                {
                    testpointFailPatternIndex = testPointIndex;
                }
            }

            startPatternData = searchData[testpointFailPatternIndex].FailPatternData;
            return scoreboardVoltages;
        }

        private List<IFailureData> RunScoreboardTest(SearchPointData.PatternData startPatternData)
        {
            this.TestMethodExtension.ApplyMask(this.InitialSearchMask_, this.ScoreboardTest);
            if (!string.IsNullOrEmpty(startPatternData.PatternName) && startPatternData.PatternName != "na" && !this.ScoreboardPlist.IsPatternAnAmble(startPatternData.PatternName))
            {
                this.ScoreboardTest.SetStartPattern(startPatternData.PatternName, startPatternData.BurstIndex, startPatternData.PatternId);
            }
            else
            {
                this.ScoreboardTest.Reset();
            }

            if (this.ScoreboardTest.Execute())
            {
                return new List<IFailureData>(); // scoreboard test passed, no data
            }
            else
            {
                return this.ScoreboardTest.GetPerCycleFailures();
            }
        }

        private string PatternToScBrdNum(string patName, string patMap, int baseNum)
        {
            string retval = baseNum.ToString().PadLeft(4, '0');
            foreach (var index in patMap.Split(','))
            {
                retval += patName[int.Parse(index)];
            }

            return retval;
        }

        private void LogFailingPatternPerIP(Dictionary<string, HashSet<string>> failingPatternPerIp)
        {
            foreach (var item in failingPatternPerIp.OrderBy(p => p.Key))
            {
                // var patternData = string.Join("_", item.Value.Select(p => p.Substring(1, 7)));
                // var patternData = item.Key == "AMBLE" ? string.Join("|", item.Value) : string.Join("|", item.Value.Select(p => this.PatternToScBrdNum(p, this.PatternNameMap, (int)(uint)this.ScoreboardBaseNumber)));
                var patternData = string.Join("|", item.Value.Select(p => this.PatternToScBrdNum(p, this.PatternNameMap, Convert.ToInt32(this.ScoreboardBaseNumber))));
                var pin = item.Key;

                this.Console?.PrintDebug($"{item.Key} = {patternData}");
                var strgvalWriter = Services.DatalogService.GetItuffStrgvalWriter();
                strgvalWriter.SetTnamePostfix($"|{this.InstanceName}|{item.Key}");
                strgvalWriter.SetData(patternData);
                strgvalWriter.SetDelimiterCharacterForWrap('|');
                Services.DatalogService.WriteToItuff(strgvalWriter);
            }
        }

        private void LogSearchVoltageResultsToItuff(IEnumerable<double> resultVoltages, IEnumerable<double> startVoltages, IEnumerable<double> endVoltages, uint executionCount, string tname)
        {
            var targetValueSeparator = "_";
            var tokenValueSeparator = "|";

            var outputSearchVoltages = string.Join(targetValueSeparator, resultVoltages.Select(i => i < 0 ? $"{(int)i}" : $"{i:F3}"));
            var outputStartVoltages = string.Join(targetValueSeparator, startVoltages.Select(i => i < 0 ? $"{(int)i}" : $"{i:F3}"));
            var outputEndVoltages = string.Join(targetValueSeparator, endVoltages.Select(i => i < 0 ? $"{(int)i}" : $"{i:F3}"));
            var outputVoltages = outputSearchVoltages + tokenValueSeparator + outputStartVoltages +
                                 tokenValueSeparator + outputEndVoltages + tokenValueSeparator + executionCount;

            var strgvalWriter = Services.DatalogService.GetItuffStrgvalWriter();
            strgvalWriter.SetTnamePostfix(tokenValueSeparator + tname);

            strgvalWriter.SetData(outputVoltages);
            Services.DatalogService.WriteToItuff(strgvalWriter);
        }

        private Dictionary<string, HashSet<string>> GetFailingPatternPerIP(List<IFailureData> allFailingCycles, Dictionary<string, string> pinToIpMap)
        {
            var failingPatternPerIP = new Dictionary<string, HashSet<string>>();
            foreach (var cycle in allFailingCycles)
            {
                var failingPins = cycle.GetFailingPinNames();
                var patName = cycle.GetPatternName();
                /* this takes an incredible long time...just log it by pin name.
                if (this.ScoreboardPlist.IsPatternAnAmble(patName))
                {
                    return new Dictionary<string, HashSet<string>> { { "AMBLE", new HashSet<string> { patName } } };
                } */

                foreach (var pin in failingPins)
                {
                    var ip = pinToIpMap.ContainsKey(pin) ? pinToIpMap[pin] : pin.Split(':').Last();

                    if (!failingPatternPerIP.ContainsKey(ip))
                    {
                        failingPatternPerIP[ip] = new HashSet<string>();
                    }

                    failingPatternPerIP[ip].Add(patName);
                }
            }

            return failingPatternPerIP;
        }
    }
}
