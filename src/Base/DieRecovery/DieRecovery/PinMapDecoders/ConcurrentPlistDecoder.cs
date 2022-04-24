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
    using DDG;
    using Newtonsoft.Json;
    using Prime;
    using Prime.FunctionalService;

    /// <summary>
    /// Implements IConcurrentTraces.
    /// </summary>
    public class ConcurrentPlistDecoder : PinMapDecoderBase, IPinMapDecoder
    {
        private HashSet<string> plistChildren;

        /// <summary>
        /// Gets or sets a description. Optional field.
        /// </summary>
        [ExcludeFromCodeCoverage]
        [JsonProperty(Required = Required.Default)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a domain name for CTV tracer.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string CtvDomain { get; set; }

        /// <summary>
        /// Gets or sets concurrent plists.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public List<ConcurrentPlist> ConcurrentPlists { get; set; }

        /// <summary>
        /// Gets or sets last executes pattern per plist.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, Tuple<string, uint>> LastPatternsInPlist { get; set; } = new Dictionary<string, Tuple<string, uint>>();

        /// <summary>
        /// Gets or sets last executes pattern per plist.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, Tuple<string, uint>> LastExecutedPatterns { get; set; } = new Dictionary<string, Tuple<string, uint>>();

        /// <inheritdoc/>
        public override BitArray GetFailTrackerFromPlistResults(IFunctionalTest functionalTest, int? currentSlice = null)
        {
            if (!(functionalTest is ICaptureFailureAndCtvPerCycleTest captureFailureTest))
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: unable to cast IFunctionalTest into ICaptureFailureAndCtvPerCycleTest object. Using incorrect input type for this decoder.");
            }

            var plistName = functionalTest.GetPlistName();
            var result = new BitArray(this.NumberOfTrackerElements, false);
            var cycleData = captureFailureTest.GetPerCycleFailures();
            if (cycleData.Count < 1)
            {
                this.Console?.PrintDebug($"CPlist: plist=[{plistName}] passed.");
                return result;
            }

            this.LastExecutedPatterns.Clear();
            foreach (var cycle in cycleData)
            {
                var matchingFilter = false;
                var failingPins = cycle.GetFailingPinNames();
                var failingParentPlist = cycle.GetParentPlistName();
                var failingPattern = cycle.GetPatternName();
                var occurrence = (uint)cycle.GetPatternInstanceId();

                this.LastExecutedPatterns[failingParentPlist] = new Tuple<string, uint>(failingPattern, occurrence);

                foreach (var filter in this.ConcurrentPlists)
                {
                    if (failingParentPlist != filter.PlistName)
                    {
                        continue;
                    }

                    matchingFilter = true;
                    if (filter.FailingPins != null)
                    {
                        for (var i = 0; i < filter.TargetPositions.Count; i++)
                        {
                            var pins = filter.FailingPins[i].Split(',').Select(o => o.Trim());
                            if (failingPins.Any(o => pins.Contains(o)))
                            {
                                result[filter.TargetPositions[i]] = true;
                                this.Console?.PrintDebug($"CPlist: {failingParentPlist} matched FailingPins=[{string.Join(",", failingPins)}] with TargetPosition=[{filter.TargetPositions[i]}].");
                            }
                        }
                    }
                    else
                    {
                        foreach (var t in filter.TargetPositions)
                        {
                            this.Console?.PrintDebug($"CPlist: {failingParentPlist} setting TargetPosition=[{t}].");
                            result[t] = true;
                        }
                    }
                }

                if (!matchingFilter)
                {
                    this.Console?.PrintDebug($"CPlist: PinMapDecoder=[{this.Name}] did not match a filter. Defaulting to set all 1s.");
                    for (var i = 0; i < result.Count; i++)
                    {
                        result[i] = true;
                    }

                    return result;
                }
            }

            var ctvPatterns = captureFailureTest.GetCtvPerPattern();
            ctvPatterns.Reverse();
            if (ctvPatterns == null || ctvPatterns.Count == 0)
            {
                throw new Exception("CPlist: CTV data was captured. Unable to determine last pattern executed for each IP.");
            }

            foreach (var ctvPattern in ctvPatterns)
            {
                var parentPlist = ctvPattern.GetParentPlistName();
                if (this.LastExecutedPatterns.ContainsKey(parentPlist))
                {
                    continue;
                }

                var patternName = ctvPattern.GetName();
                var occurrence = ctvPattern.GetInstanceId();
                this.LastExecutedPatterns[parentPlist] = new Tuple<string, uint>(patternName, (uint)occurrence);

                if (this.plistChildren.All(o => this.LastExecutedPatterns.ContainsKey(o)))
                {
                    break;
                }
            }

            foreach (var plist in this.LastExecutedPatterns)
            {
                this.Console?.PrintDebug($"CPList: Plist=[{plist.Key}] last execution pattern=[{plist.Value}].");
            }

            return result;
        }

        /// <inheritdoc/>
        public override List<string> MaskPlistFromTracker(BitArray mask, ref IFunctionalTest plist)
        {
            var maskPins = new List<string>();
            var captureFailureTest = plist as ICaptureFailureAndCtvPerCycleTest;
            if (captureFailureTest == null)
            {
                throw new Exception($"CPlist: using incorrect capture settings. Please set {typeof(ICaptureFailureAndCtvPerCycleTest)}.");
            }

            Dictionary<string, Tuple<string, uint>> startPatternPerPlist = new Dictionary<string, Tuple<string, uint>>();
            foreach (var childPlist in this.ConcurrentPlists)
            {
                Tuple<string, uint> startPattern = null;
                var maskAllPositions = childPlist.TargetPositions.Aggregate(true, (current, position) => current & mask[position]);
                var atLeastOneDisabledPosition = childPlist.TargetPositions.Aggregate(false, (current, position) => current | mask[position]);
                if (maskAllPositions)
                {
                    this.Console?.PrintDebug($"CPlist: {childPlist.PlistName} has all positions disabled and will skip to last pattern in plist=[{this.LastPatternsInPlist[childPlist.PlistName].Item1}] occurrence=]{this.LastPatternsInPlist[childPlist.PlistName].Item2}].");
                    startPattern = this.LastPatternsInPlist[childPlist.PlistName];
                }
                else
                {
                    if (this.LastExecutedPatterns.ContainsKey(childPlist.PlistName))
                    {
                        startPattern = this.LastExecutedPatterns[childPlist.PlistName];
                    }

                    if (childPlist.FailingPins != null)
                    {
                        for (int i = 0; i < childPlist.TargetPositions.Count; i++)
                        {
                            if (!mask[childPlist.TargetPositions[i]])
                            {
                                continue;
                            }

                            var childListMaskPins = childPlist.FailingPins[i].Split(',').ToList();
                            maskPins.AddRange(childListMaskPins);
                        }
                    }
                    else if (atLeastOneDisabledPosition)
                    {
                        throw new Exception($"CPlist: {childPlist.PlistName} does not contain {nameof(childPlist.FailingPins)} and only partial {nameof(childPlist.TargetPositions)} are disabled. Bits=[{mask.ToBinaryString()}].");
                    }
                }

                if (startPattern != null)
                {
                    startPatternPerPlist.Add(childPlist.PlistName, startPattern);
                }
            }

            if (startPatternPerPlist.Count > 0)
            {
                captureFailureTest.SetStartPatternForConcurrentPlist(startPatternPerPlist);
            }

            return maskPins;
        }

        /// <inheritdoc/>
        public override void Restore()
        {
            this.LastExecutedPatterns.Clear();
        }

        /// <inheritdoc />
        public override string GetDecoderType()
        {
            return this.GetType().Name;
        }

        /// <inheritdoc />
        public override void Verify(ref IFunctionalTest plist)
        {
            this.Restore();
            this.LastPatternsInPlist.Clear();
            this.plistChildren = new HashSet<string>();
            foreach (var o in this.ConcurrentPlists)
            {
                this.plistChildren.Add(o.PlistName);
            }

            foreach (var item in this.plistChildren)
            {
                var plistObject = Prime.Services.PlistService.GetPlistObject(item);
                var plistContents = plistObject.GetPatternsAndIndexes(true);
                for (int i = plistContents.Count; i-- > 0;)
                {
                    if (plistContents[i].IsPattern())
                    {
                        var patternName = plistContents[i].GetPlistItemName();
                        if (i == plistContents.Count - 1)
                        {
                            this.LastPatternsInPlist[item] = new Tuple<string, uint>(patternName, 1);
                        }
                        else if (this.LastPatternsInPlist[item].Item1 == patternName)
                        {
                            var count = this.LastPatternsInPlist[item].Item2 + 1;
                            this.LastPatternsInPlist[item] = new Tuple<string, uint>(patternName, count);
                        }

                        TOSUserSDK.Pattern.Service.SetPVCData(patternName, this.CtvDomain, 1, "CTV", 1);
                    }
                }
            }
        }

        /// <summary>
        /// Concurrent plist configuration for masking.
        /// </summary>
        public class ConcurrentPlist
        {
            /// <summary>
            /// Gets or sets some optional comment.
            /// </summary>
            [ExcludeFromCodeCoverage]
            [JsonProperty(Required = Required.Default)]
            public string Comment { get; set; }

            /// <summary>
            /// Gets or sets the concurrent plist name.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public string PlistName { get; set; }

            /// <summary>
            /// Gets or sets the failing pattern occurrence.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public List<ushort> TargetPositions { get; set; }

            /// <summary>
            /// Gets or sets failingPins.
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public List<string> FailingPins { get; set; }
        }
    }
}