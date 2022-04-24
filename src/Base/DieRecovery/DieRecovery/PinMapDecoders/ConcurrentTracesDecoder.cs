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

namespace DieRecoveryBase
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using DDG;
    using Newtonsoft.Json;
    using Prime;
    using Prime.FunctionalService;

    /// <summary>
    /// Implements IConcurrentTraces.
    /// </summary>
    public class ConcurrentTracesDecoder : PinMapDecoderBase, IPinMapDecoder
    {
        private PlistTree plistTree;
        private ICaptureFailureTest captureFailureTest;

        /// <summary>
        /// Gets or sets a description. Optional field.
        /// </summary>
        [ExcludeFromCodeCoverage]
        [JsonProperty(Required = Required.Default)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets mask targets configuration.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public List<MaskTarget> MaskConfigurations { get; set; }

        /// <summary>
        /// Gets or sets list of pattern entries.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public List<ConfigurationEntry> Entries { get; set; }

        /// <inheritdoc/>
        public override BitArray GetFailTrackerFromPlistResults(IFunctionalTest functionalTest, int? currentSlice = null)
        {
            if (!(functionalTest is ICaptureFailureTest captureFailureTest))
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: unable to cast IFunctionalTest into ICaptureFailureTest object. Using incorrect input type for this decoder.");
            }

            var result = new BitArray(this.NumberOfTrackerElements);
            var cycleData = captureFailureTest.GetPerCycleFailures();
            if (cycleData.Count < 1)
            {
                this.Console?.PrintDebug($"CCR: plist=[{functionalTest.GetPlistName()}] passed.");
                for (var i = 0; i < result.Count; i++)
                {
                    result[i] = false;
                }

                return result;
            }

            var matchingEntry = false;

            foreach (var t in cycleData)
            {
                var patternName = t.GetPatternName();
                var burst = t.GetBurstIndex();
                var occurrence = t.GetPatternInstanceId();
                var failingPins = t.GetFailingPinNames();
                foreach (var entry in from entry in this.Entries where patternName == entry.FailFilters.PatternName where burst == entry.FailFilters.Burst where occurrence == entry.FailFilters.PatternOccurrence select entry)
                {
                    if (entry.FailFilters.FailingPins != null && entry.FailFilters.FailingPins.Count > 0)
                    {
                        for (var i = 0; i < entry.FailFilters.FailingPins.Count; i++)
                        {
                            var pins = entry.FailFilters.FailingPins[i].Split(',').Select(o => o.Trim());
                            if (failingPins.Any(o => pins.Contains(o)))
                            {
                                result[entry.FailFilters.TargetPositions[i]] = true;
                                this.Console?.PrintDebug($"CCR: matched entry Burst=[{burst}], Pattern=[{patternName}], Occurrence=[{occurrence}], FailingPins=[{string.Join(",", failingPins)}] with TargetPosition=[{entry.FailFilters.TargetPositions[i]}].");
                                matchingEntry = true;
                            }
                        }
                    }
                    else
                    {
                        foreach (var i in entry.FailFilters.TargetPositions)
                        {
                            result[i] = true;
                            this.Console?.PrintDebug($"CCR: matched entry Burst=[{burst}], Pattern=[{patternName}], Occurrence=[{occurrence}] with TargetPosition=[{i}].");
                            matchingEntry = true;
                        }
                    }
                }
            }

            if (matchingEntry)
            {
                return result;
            }

            throw new Exception($"CCR: PinMapDecoder=[{this.Name}] did not find a matching entry.");
        }

        /// <inheritdoc/>
        public override List<string> MaskPlistFromTracker(BitArray mask, ref IFunctionalTest plist)
        {
            this.BuildPlistTree(plist);
            var plistWideMasking = new List<string>();
            this.captureFailureTest = plist as ICaptureFailureTest;
            foreach (var maskTarget in this.MaskConfigurations.Where(maskTarget => maskTarget.TargetPositions.Any(o => mask[o])))
            {
                if (maskTarget.PatternNames == null || maskTarget.PatternNames.Count < 1)
                {
                    if (maskTarget.Options.ContainsKey("Mask"))
                    {
                        plistWideMasking.AddRange(maskTarget.Options["Mask"].Split(',').Select(o => o.Trim()));
                    }

                    continue;
                }

                this.SetPlistElementOptions(ref this.plistTree, maskTarget);
            }

            return plistWideMasking.Distinct().ToList();
        }

        /// <inheritdoc />
        public override string GetDecoderType()
        {
            return this.GetType().Name;
        }

        /// <inheritdoc />
        public override void ApplyPlistSettings(BitArray mask, ref IFunctionalTest plist)
        {
            if (!(plist is ICaptureFailureTest captureFailureTest))
            {
                throw new Exception($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: incorrect {nameof(IFunctionalTest)}. This decoder requires the use of {nameof(ICaptureFailureTest)}.");
            }

            this.BuildPlistTree(plist);

            try
            {
                if (captureFailureTest.HasStartPattern())
                {
                    var originalStartPattern = captureFailureTest.GetStartPattern();
                    var startPattern = new PatternOccurrence(originalStartPattern.Item2, originalStartPattern.Item1, originalStartPattern.Item3, ulong.MinValue);
                    this.Console?.PrintDebug($"CCR: Previous Fail: Burst=[{startPattern.Burst}], Pattern=[{startPattern.PatternName}], Occurrence=[{startPattern.Occurrence}].");
                    var entry = this.Entries.Find(o =>
                        o.FailFilters.Burst == startPattern.Burst &&
                        o.FailFilters.PatternName == startPattern.PatternName &&
                        o.FailFilters.PatternOccurrence == startPattern.Occurrence);

                    if (entry != null)
                    {
                        this.UpdatePlistElementOptions(entry);
                        this.UpdatePreBurstPList(entry);
                        var entryStartPattern = this.GetStartPattern(entry);
                        startPattern = entryStartPattern ?? startPattern;
                    }

                    this.Console?.PrintDebug($"CCR: Setting start pattern Burst=[{startPattern.Burst}], Pattern=[{startPattern.PatternName}], Occurrence=[{startPattern.Occurrence}].");
                    captureFailureTest.SetStartPattern(startPattern.PatternName, (uint)startPattern.Burst, (uint)startPattern.Occurrence);
                }
            }
            catch (Exception ex)
            {
                Prime.Services.ConsoleService.PrintError(ex.Message);
                this.Restore();
                throw;
            }

            this.plistTree.PlistObject.Resolve();
        }

        /// <inheritdoc />
        public override void Restore()
        {
            if (this.plistTree == null)
            {
                return;
            }

            this.captureFailureTest.Reset();
            this.plistTree.RestorePlist();
            this.plistTree.PlistObject.Resolve();
        }

        private void BuildPlistTree(IFunctionalTest plist)
        {
            if (this.plistTree == null)
            {
                this.plistTree = new PlistTree(plist.GetPlistName());
            }
        }

        private void SetPlistElementOptions(ref PlistTree tree, MaskTarget maskTarget)
        {
            foreach (var item in tree.Contents)
            {
                var index = item.GetPatternIndex();
                if (item.IsPattern() && maskTarget.PatternNames.Any(o => Regex.IsMatch(o, item.GetPlistItemName())))
                {
                    foreach (var option in maskTarget.Options)
                    {
                        tree.UpdateSinglePlistElementOption(index, option);
                    }
                }
                else if (!item.IsPattern())
                {
                    var treeChild = tree.Children[index];
                    this.SetPlistElementOptions(ref treeChild, maskTarget);
                }
            }
        }

        private void UpdatePreBurstPList(ConfigurationEntry entry)
        {
            if (entry.PreBurstPList == null)
            {
                return;
            }

            var tree = this.plistTree.Find(entry.PreBurstPList.Patlist);
            tree.UpdatePreBurstPList(entry.PreBurstPList.PreBurstPList);
        }

        private PatternOccurrence GetStartPattern(ConfigurationEntry entry)
        {
            if (entry.StartPattern != null)
            {
                return new PatternOccurrence(entry.FailFilters.Burst, entry.StartPattern.PatternName, entry.StartPattern.PatternOccurrence, ulong.MinValue);
            }

            return null;
        }

        private void UpdatePlistElementOptions(ConfigurationEntry entry)
        {
            if (entry.PlistElementOptions == null)
            {
                return;
            }

            foreach (var plistElementOption in entry.PlistElementOptions)
            {
                if (plistElementOption.Options == null || plistElementOption.Options.Count <= 0)
                {
                    continue;
                }

                var tree = this.plistTree.Find(plistElementOption.Patlist);
                foreach (var option in plistElementOption.Options)
                {
                    foreach (var index in plistElementOption.Index)
                    {
                        tree.UpdateSinglePlistElementOption(index, option);
                    }
                }
            }
        }

        /// <summary>
        /// Defined plist element options for concurrent traces.
        /// </summary>
        public class PlistElementOption
        {
            /// <summary>
            /// Gets or sets the pattern name.
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public string Patlist { get; set; }

            /// <summary>
            /// Gets or sets the index in current patlist.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public List<uint> Index { get; set; }

            /// <summary>
            /// Gets or sets PlistElementOptions values.
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public Dictionary<string, string> Options { get; set; }
        }

        /// <summary>
        /// Mask configuration entry.
        /// </summary>
        public class MaskTarget
        {
            /// <summary>
            /// Gets or sets some optional comment.
            /// </summary>
            [ExcludeFromCodeCoverage]
            [JsonProperty(Required = Required.Default)]
            public string Comment { get; set; }

            /// <summary>
            /// Gets or sets the failing pattern occurrence.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public List<ushort> TargetPositions { get; set; }

            /// <summary>
            /// Gets or sets a list of regular expressions for pattern masking.
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public List<string> PatternNames { get; set; }

            /// <summary>
            /// Gets or sets PlistElement options.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public Dictionary<string, string> Options { get; set; }
        }

        /// <summary>
        /// ConcurrentTracesConfiguration entry.
        /// </summary>
        public class ConfigurationEntry
        {
            /// <summary>
            /// Gets or sets some optional comment.
            /// </summary>
            [ExcludeFromCodeCoverage]
            [JsonProperty(Required = Required.Default)]
            public string Comment { get; set; }

            /// <summary>
            /// Gets or sets the list of fail filters for a given entry.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public FailFilter FailFilters { get; set; }

            /// <summary>
            /// Gets or sets the start pattern information for a given entry.
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public StartPatternInfo StartPattern { get; set; }

            /// <summary>
            /// Gets or sets the PreBurstPList for a given entry.
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public PreBurstPListInfo PreBurstPList { get; set; }

            /// <summary>
            /// Gets or sets the list of optional plist element options.
            /// </summary>
            [JsonProperty(Required = Required.Default)]

            public List<PlistElementOption> PlistElementOptions { get; set; }

            /// <summary>
            /// Defines fail filter for a given entry.
            /// </summary>
            public class FailFilter
            {
                /// <summary>
                /// Gets or sets the failing burst.
                /// </summary>
                [JsonProperty(Required = Required.Default)]
                public ulong Burst { get; set; } = 0;

                /// <summary>
                /// Gets or sets the failing pattern.
                /// </summary>
                [JsonProperty(Required = Required.Always)]
                public string PatternName { get; set; }

                /// <summary>
                /// Gets or sets the failing pattern occurrence.
                /// </summary>
                [JsonProperty(Required = Required.Always)]
                public ulong PatternOccurrence { get; set; }

                /// <summary>
                /// Gets or sets the list of failing pins by target position.
                /// </summary>
                [JsonProperty(Required = Required.Default)]
                public List<string> FailingPins { get; set; }

                /// <summary>
                /// Gets or sets the failing pattern occurrence.
                /// </summary>
                [JsonProperty(Required = Required.Always)]
                public List<ushort> TargetPositions { get; set; }
            }

            /// <summary>
            /// Defines start pattern information for a given entry.
            /// </summary>
            public class StartPatternInfo
            {
                /// <summary>
                /// Gets or sets the starting pattern.
                /// </summary>
                [JsonProperty(Required = Required.Always)]
                public string PatternName { get; set; }

                /// <summary>
                /// Gets or sets the starting pattern.
                /// </summary>
                [JsonProperty(Required = Required.Always)]
                public ulong PatternOccurrence { get; set; }

                /// <summary>
                /// Gets or sets the starting pattern.
                /// </summary>
                [JsonProperty(Required = Required.Default)]
                public string PreBurstPList { get; set; }
            }

            /// <summary>
            /// Defines PreBurstPList information for a given entry.
            /// </summary>
            public class PreBurstPListInfo
            {
                /// <summary>
                /// Gets or sets the patlist name.
                /// </summary>
                [JsonProperty(Required = Required.Default)]
                public string Patlist { get; set; }

                /// <summary>
                /// Gets or sets the starting pattern.
                /// </summary>
                [JsonProperty(Required = Required.Always)]
                public string PreBurstPList { get; set; }
            }
        }
    }
}