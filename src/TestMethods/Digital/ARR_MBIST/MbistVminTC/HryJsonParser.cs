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

// ---------------------------------------------------------------
// Created By Tim Kirkham
// ---------------------------------------------------------------
namespace MbistVminTC
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Prime;

    /// <summary> HRY JSON Parser.</summary>
    public class HryJsonParser
    {
        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("Version")]
        public double Version { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("Hry_string")]
        public List<string> HryStringRef { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("Voltage_String", Required = Required.Default)]
        public List<string> VoltageStringRef { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("BISR_CHAIN", Required = Required.Default)]
        public Dictionary<string, BisrClasses> Bisrdata { get; set; }

        // /// <summary>Gets or sets all Indexes of running IPs.</summary>
        // [JsonProperty(Required = Required.DisallowNull)]
        // public HashSet<int> RunningIndexs { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty(Required = Required.Default)]
        public List<string> Bisrpats { get; set; }

        /// <summary>Gets or sets a value indicating whether gets or sets JSON Element.</summary>
        [JsonProperty(Required = Required.Default)]
        public bool Bisrexists { get; set; } = false;

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("Plists")]
        public Dictionary<string, List<string>> Plists { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty(Required = Required.Default)]
        public Dictionary<string, HashSet<int>> Indexperpattern { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty(Required = Required.Default)]
        public int PlistCTVCount { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty(Required = Required.Default)]
        public Dictionary<string, HashSet<int>> Indexperplist { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("Patterns")]
        public Dictionary<string, PatternBlocks> Patterns { get; set; }

        /// <summary>Remove Non MBIST patterns from Plists.</summary>
        public void RemoveNonMBistPats()
        {
            var updatedPlist = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, List<string>> plist in this.Plists)
            {
                updatedPlist.Add(plist.Key, new List<string>());
                foreach (var pattern in plist.Value)
                {
                    if (this.Patterns.ContainsKey(pattern))
                    {
                        updatedPlist[plist.Key].Add(pattern);
                    }
                }
            }

            this.Plists = updatedPlist;
        }

        /// <summary>Build HRY Index for each memory.</summary>
        public void BuildIndex()
        {
            this.PlistCTVCount = 0;

            // this.RunningIndexs = new HashSet<int>();
            this.Indexperpattern = new Dictionary<string, HashSet<int>>();
            foreach (KeyValuePair<string, PatternBlocks> pattern in this.Patterns)
            {
                this.Indexperpattern[pattern.Key] = new HashSet<int>();
                foreach (KeyValuePair<string, PatternBlocks.ControllersBlock> cont in pattern.Value.Controllers)
                {
                    if (cont.Value.Type != Hry.AllowedTypes.BISR)
                    {
                        cont.Value.ControllerIndexs = new Dictionary<string, int>();
                        foreach (var execution in cont.Value.Executions)
                        {
                            var temp = execution.BuildIndex(cont.Key, this.HryStringRef);
                            foreach (KeyValuePair<string, int> loop in temp)
                            {
                                if (cont.Value.ControllerIndexs.ContainsKey(loop.Key))
                                {
                                    cont.Value.ControllerIndexs[loop.Key] = loop.Value;
                                }
                                else
                                {
                                    cont.Value.ControllerIndexs.Add(loop.Key, loop.Value);
                                }
                            }

                            // this.RunningIndexs.UnionWith(temp.Values);
                            this.Indexperpattern[pattern.Key].UnionWith(temp.Values);
                        }
                    }
                }
            }

            this.Indexperplist = new Dictionary<string, HashSet<int>>();
            foreach (KeyValuePair<string, List<string>> plist in this.Plists)
            {
                this.Indexperplist[plist.Key] = new HashSet<int>();
                var patternfound = new List<string>();

                // Prime.Services.ConsoleService.PrintDebug($"[HRYJSONPARSE] Indexing in PLIST.");
                foreach (var pattern in plist.Value)
                {
                    this.PlistCTVCount += this.Patterns[pattern].CaptureCount;

                    // Prime.Services.ConsoleService.PrintDebug($"[HRYJSONPARSE] pattern: {pattern}.");
                    if (!patternfound.Contains(pattern))
                    {
                        this.Indexperplist[plist.Key].UnionWith(this.Indexperpattern[pattern]);
                        patternfound.Add(pattern);
                    }
                }
            }
        }

        /// <summary> Builds Bisr specific .</summary>
        /// <param name = "plist" > Plist to run.</param>
        /// <param name = "runbuildbisr" > run build and bisr.</param>
        /// <returns>Hry lookup for just this plist.</returns>
        public HryJsonParser MakePerPlistLookup(string plist, bool runbuildbisr)
        {
            if (runbuildbisr == true)
            {
            this.BuildIndex();
            this.BuildBisr(plist);
            }

            var copyhrylookup = new HryJsonParser();
            copyhrylookup.Version = this.Version;
            copyhrylookup.HryStringRef = this.HryStringRef;
            copyhrylookup.VoltageStringRef = this.VoltageStringRef;
            copyhrylookup.Bisrdata = this.Bisrdata;
            copyhrylookup.Bisrpats = this.Bisrpats;
            copyhrylookup.Bisrexists = this.Bisrexists;
            copyhrylookup.Plists = new Dictionary<string, List<string>>();
            copyhrylookup.Plists.Add(plist, this.Plists[plist]);
            if (runbuildbisr == true)
            {
                copyhrylookup.Indexperplist = new Dictionary<string, HashSet<int>>();
                copyhrylookup.Indexperplist.Add(plist, this.Indexperplist[plist]);

                copyhrylookup.Indexperpattern = new Dictionary<string, HashSet<int>>();
            }

            copyhrylookup.Patterns = new Dictionary<string, PatternBlocks>();
            var patternfound = new List<string>();
            foreach (var pattern in this.Plists[plist])
            {
                if (!patternfound.Contains(pattern))
                {
                    if (runbuildbisr == true)
                    {
                        copyhrylookup.Indexperpattern.Add(pattern, this.Indexperpattern[pattern]);
                    }

                    copyhrylookup.Patterns.Add(pattern, this.Patterns[pattern]);
                    patternfound.Add(pattern);
                }
            }

            return copyhrylookup;
        }

        /// <summary> Builds Bisr specific .</summary>
        /// /// <param name = "plist" > Plist to run.</param>
        public void BuildBisr(string plist)
        {
            var birafound = false;
            this.Bisrexists = false;
            this.Bisrpats = new List<string>();
            List<string> bisrpats = new List<string>();
            foreach (var pattern in this.Plists[plist])
            {
                foreach (KeyValuePair<string, HryJsonParser.PatternBlocks.ControllersBlock> cont in this.Patterns[pattern].Controllers)
                {
                    if (cont.Value.Type == Hry.AllowedTypes.BISR)
                    {
                        bisrpats.Add(pattern);
                    }
                    else if (cont.Value.Type == Hry.AllowedTypes.RABITS)
                    {
                        birafound = true;
                    }
                }
            }

            if (birafound == true && bisrpats.Count > 0)
            {
                this.Bisrpats = bisrpats;
                this.Bisrexists = true;
            }
        }

        /// <summary> Class to parse Patterns section.</summary>
        public class PatternBlocks
        {
            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("CaptureCount")]
            public int CaptureCount { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("Controllers")]
            public Dictionary<string, ControllersBlock> Controllers { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            public class ControllersBlock
            {
                /// <summary>Gets or sets JSON Element.</summary>
                [JsonProperty("TYPE")]
                public Hry.AllowedTypes Type { get; set; }

                /// <summary>Gets or sets JSON Element.</summary>
                [JsonProperty("Execution", Required = Required.Default)]
                public List<ExecutionBlocks> Executions { get; set; }

                /// <summary>Gets or sets all Indexes of running IPs.</summary>
                [JsonProperty(Required = Required.Default)]
                public Dictionary<string, int> ControllerIndexs { get; set; }

                /// <summary>Gets or sets JSON Element.</summary>
                [JsonProperty("CAPTURE_DATA", Required = Required.Default)]
                public string CaptureData { get; set; }

                /// <summary>Gets or sets JSON Element.</summary>
                [JsonProperty("CAPTURE_COMMENTS", Required = Required.Default)]
                public string CaptureComments { get; set; }

                /// <summary>Gets or sets JSON Element.</summary>
                public class ExecutionBlocks
                {
                    /// <summary>Gets or sets JSON Element.</summary>
                    [JsonProperty("STATUS")]
                    public List<int> Status { get; set; }

                    /// <summary>Gets or sets JSON Element.</summary>
                    [JsonProperty("ALGO_SEL")]
                    public string Algo_sel { get; set; }

                    /// <summary>Gets or sets JSON Element.</summary>
                    [JsonProperty("PALGO_SEL")]
                    public string PAlgo_sel { get; set; }

                    /// <summary>Gets or sets JSON Element.</summary>
                    [JsonProperty("Memories", Required = Required.Default)]
                    public Dictionary<string, string> Memories { get; set; }

                    /// <summary>Gets or sets JSON Element.</summary>
                    [JsonProperty(Required = Required.Default)]
                    public Dictionary<string, int> HryIndex { get; set; }

                    /// <summary>Build HRY Index for each memory.</summary>
                    /// <param name = "controller" > Name of the Controller.</param>
                    /// <param name = "hryreference" > Refrence String of memories.</param>
                    /// <returns>list of indexes running.</returns>
                    public Dictionary<string, int> BuildIndex(string controller, List<string> hryreference)
                    {
                        var templist = new List<int>();
                        this.HryIndex = new Dictionary<string, int>();
                        if (this.Memories != null)
                        {
                            foreach (KeyValuePair<string, string> memory in this.Memories)
                            {
                                var temp = hryreference.IndexOf(controller.ToUpper() + "_MEM" + memory.Key);
                                if (!this.HryIndex.ContainsKey(memory.Key))
                                {
                                    templist.Add(temp);
                                    this.HryIndex.Add(memory.Key, temp);
                                }
                            }
                        }
                        else
                        {
                            var found = true;
                            var idx = 1;
                            while (found == true)
                            {
                                var mem = controller.ToUpper() + "_MEM" + idx;
                                if (hryreference.Contains(mem))
                                {
                                    var intostr = idx.ToString();
                                    var temp = hryreference.IndexOf(mem);
                                    if (!this.HryIndex.ContainsKey(intostr))
                                    {
                                        this.HryIndex.Add(intostr, temp);
                                    }
                                }
                                else
                                {
                                    found = false;
                                }

                                idx += 1;
                            }
                        }

                        return this.HryIndex;
                    }
                }
            }
        }

        /// <summary> Class to parse BISR section.</summary>
        public class BisrClasses
        {
            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("buffer_size")]
            public int Buffer_size { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("zero_counter_bits")]
            public int ZeroCountBits { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("fuse_box_size")]
            public int FuseBoxSize { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("totallength")]
            public int Totallength { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("fuseboxAddress")]
            public int FuseboxAddress { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("max_fuse_box_programming_sessions")]
            public int MaxSessions { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("chains")]
            public List<int> Chains { get; set; }
        }
    }
}
