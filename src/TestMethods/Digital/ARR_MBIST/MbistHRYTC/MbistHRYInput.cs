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

namespace MbistHRYTC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ARR_MBIST;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Contains the HRY JSON input data.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "More readable this way.")]
    public class MbistHRYInput
    {
        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("Version", Order = 1)]
        public double Version { get; set; } = 1.0;

        /// <summary>Gets or sets HRY Length (not a JSON element).</summary>
        [JsonIgnore]
        public int HryLength { get; set; }

        /// <summary>Gets or sets HRY String element which contains a list of all memories in the hry string.</summary>
        [JsonProperty("Hry_string", Order = 2)]
        public List<string> HryFullMemList { get; set; }

        /// <summary>Gets or sets JSON Element. Key=PlistName.</summary>
        [JsonProperty("LookupTables", Order = 3)]
        public Dictionary<string, MbistLookupTable> LookupTables { get; set; }

        /// <summary>
        /// Returns the MbistLookupTable struct for the given plist name.
        /// Checks all variations with/without ip scoping.
        /// </summary>
        /// <param name="plist">The name of the PList.</param>
        /// <returns>MbistLookupTable Object.</returns>
        public MbistLookupTable GetLookupTable(string plist)
        {
            if (this.LookupTables.ContainsKey(plist))
            {
                // if the plist exists, use it directly.
                return this.LookupTables[plist];
            }
            else if (plist.Contains("::"))
            {
                // if the given plist contains IP scoping, remove it and look for a match.
                var pair = plist.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                var noIpPlist = pair.Last();
                if (this.LookupTables.ContainsKey(noIpPlist))
                {
                    return this.LookupTables[noIpPlist];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // The given plist does not contain IP scoping, check if any of the existing plists do.
                foreach (var plistFullName in this.LookupTables.Keys)
                {
                    if (plistFullName.Contains("::"))
                    {
                        var pair = plistFullName.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        var noIpPlist = pair.Last();
                        if (noIpPlist == plist)
                        {
                            return this.LookupTables[plistFullName];
                        }
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Valdates the HRY Json struct.
        /// </summary>
        /// <param name="plist">The name of the PList to validate, if empty will validate all plists.</param>
        /// <returns>List of all the errors in string format.</returns>
        public List<string> Validate(string plist = "")
        {
            List<string> errors = new List<string>();
            List<string> plistsToValidate = new List<string>();
            if (plist == string.Empty)
            {
                plistsToValidate = new List<string>(this.LookupTables.Keys);
            }
            else
            {
                plistsToValidate.Add(plist);
            }

            // Validate that all the Memories appear in HryFullMemList.
            // Also populate the main Memories HryIndex field and the main HryLength field.
            if (this.HryFullMemList == null)
            {
                errors.Add("Json Element [Hry_string] is empty.");
            }
            else
            {
                this.HryLength = this.HryFullMemList.Count;

                Dictionary<string, int> hryMap = new Dictionary<string, int>();
                for (var i = 0; i < this.HryFullMemList.Count; i++)
                {
                    hryMap[this.HryFullMemList[i]] = i;
                }

                foreach (var plistName in plistsToValidate)
                {
                    var table = this.GetLookupTable(plistName);
                    if (table == null)
                    {
                        errors.Add($"PList [{plistName}] does not have a [LookupTables] entry.");
                    }
                    else
                    {
                        foreach (var cont in table.Controllers)
                        {
                            foreach (var group in cont.Groups)
                            {
                                foreach (var mem in group.Memories)
                                {
                                    var name = $"{cont.Name}_MEM{mem.MemoryId}";
                                    if (hryMap.ContainsKey(name))
                                    {
                                        mem.HryIndex = hryMap[name];
                                    }
                                    else
                                    {
                                        errors.Add($"Memory=[{name}] in PList=[{plistName}] does not have an entry in Json Element [Hry_string].");
                                    }
                                } // end foreach mem
                            } // end foreach group
                        } // end foreach controller
                    }
                } // end foreach plist
            }

            return errors;
        }

        /// <summary>
        /// Holds the HRY definitions for each PList.
        /// </summary>
        public class MbistLookupTable
        {
            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("CapturePins", Order = 1)]
            public string CapturePins { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("CaptureCount", Order = 2)]
            public int CaptureCount { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("CaptureInterLeaveMode", Order = 3)]
            [JsonConverter(typeof(StringEnumConverter))]
            public Mbist.CaptureInterLeaveType CaptureInterLeaveMode { get; set; } = Mbist.CaptureInterLeaveType.CycleFirst;

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("Controllers", Order = 4)]
            public List<Controller> Controllers { get; set; }

            /// <summary>
            /// Gets the named controller for this plist.
            /// </summary>
            /// <param name="controller">Name of the Controller object to return.</param>
            /// <returns>Controller type.</returns>
            public Controller GetController(string controller)
            {
                var cont = this.Controllers.Find(i => i.Name == controller);
                if (cont == null)
                {
                    throw new InvalidOperationException($"No controller found matching [{controller}] Possibilities=[{string.Join(", ", this.Controllers.Select(o => o.Name).ToList())}]");
                }

                return cont;
            }

            /// <summary>
            /// Gets the index into the HRY string for the given Controller/Memory.
            /// </summary>
            /// <param name="controller">Name of the Controller.</param>
            /// <param name="mem">MemoryID value.</param>
            /// <returns>Index into the HRY string.</returns>
            public int GetHRYIndex(string controller, int mem)
            {
                return this.GetController(controller).Groups.First().Memories.Find(i => i.MemoryId == mem).HryIndex;
            }

            /// <summary>
            /// Controller object contains all the HRY information for a given controller.
            /// </summary>
            public class Controller
            {
                /// <summary>Gets or sets JSON Element.</summary>
                [JsonProperty("Name", Order = 1)]
                public string Name { get; set; }

                /// <summary>Gets or sets JSON Element.</summary>
                [JsonProperty("Execution", Order = 2)]
                public List<AlgorithmGroup> Groups { get; set; }

                /// <summary>
                /// Placeholder so the HRY plist can contain multiple algorithms which map to the same MEM/HRY.
                /// </summary>
                public class AlgorithmGroup
                {
                    /// <summary>Gets or sets JSON Element.</summary>
                    [JsonProperty("STATUS", Order = 1)]
                    public List<int> StatusBits { get; set; }

                    /// <summary>Gets or sets JSON Element.</summary>
                    [JsonProperty("ALGO_SEL", Order = 2)]
                    public List<int> AlgorithmSelectBits { get; set; }

                    /// <summary>Gets or sets JSON Element.</summary>
                    [JsonProperty("Memories", Order = 3)]
                    public List<Memory> Memories { get; set; }

                    /// <summary>
                    /// Represents the HRY information for a single Memory.
                    /// </summary>
                    public class Memory
                    {
                        /// <summary>Gets or sets JSON Element.</summary>
                        [JsonProperty("MEM", Order = 1)]
                        public int MemoryId { get; set; }

                        /// <summary>Gets or sets HryIndex (not a JSON Element).</summary>
                        [JsonIgnore]
                        public int HryIndex { get; set; }

                        /// <summary>Gets or sets JSON Element.</summary>
                        [JsonProperty("GOID", Order = 2)]
                        public List<int> GoIDBits { get; set; }

                        /// <summary>Gets or sets JSON Element.</summary>
                        [JsonProperty("RABits", Order = 3)]
                        public List<int> RABits { get; set; }
                    }
                }

                /// <summary>
                /// Gets a dictionary where Key=MemoryID and Value=HRY Index for
                /// all the memories in this Controller.
                /// </summary>
                /// <returns>Dictionary, Key=MemoryID, Value=HRY Index.</returns>
                public Dictionary<int, int> GetHryIndexByMemory()
                {
                    var retval = new Dictionary<int, int>(); // Key=MemoryID, Value=HryIndex
                    foreach (var group in this.Groups)
                    {
                        foreach (var memory in group.Memories)
                        {
                            if (!retval.ContainsKey(memory.MemoryId))
                            {
                                retval[memory.MemoryId] = memory.HryIndex;
                            }
                        }
                    }

                    return retval;
                }

                /// <summary>
                /// Gets all the memorys in in this controller.
                /// </summary>
                /// <returns>Dictionary where Key=Memory ID, Value=List of Memories which match the ID/Key.</returns>
                public Dictionary<int, List<AlgorithmGroup.Memory>> GetMemoriesByID()
                {
                    var retval = new Dictionary<int, List<AlgorithmGroup.Memory>>();
                    foreach (var group in this.Groups)
                    {
                        foreach (var memory in group.Memories)
                        {
                            if (!retval.ContainsKey(memory.MemoryId))
                            {
                                retval[memory.MemoryId] = new List<AlgorithmGroup.Memory>() { memory };
                            }
                            else
                            {
                                retval[memory.MemoryId].Add(memory);
                            }
                        }
                    }

                    return retval;
                }
            }
        }
    }
}
