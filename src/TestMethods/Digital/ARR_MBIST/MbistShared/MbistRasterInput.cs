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

namespace ARR_MBIST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>MBIST Raster JSON configuration.</summary>
    public class MbistRasterInput
    {
        /// <summary>Gets or sets Version.</summary>
        [JsonProperty("Version", Order = 1)]
        public double Version { get; set; } = 1.0;

        /// <summary>Gets or sets HRY Length (not a JSON element).</summary>
        [JsonIgnore]
        public int HryLength { get; set; }

        /// <summary>Gets or sets HRY String element which contains a list of all memories in the hry string.</summary>
        [JsonProperty("Hry_string", Order = 2)]
        public List<string> HryFullMemList { get; set; }

        /// <summary>Gets or sets PLists.</summary>
        [JsonProperty("PLists", Order = 3)]
        public Dictionary<string, PList> PLists { get; set; }

        /// <summary>Gets or sets CaptureDecoders.  Key=Capture Name, Value={Dict Key=Controller, Value=Decoder}.</summary>
        [JsonProperty("CaptureGroups", Order = 4)]
        /* public Dictionary<string, BitRanges> CaptureDecoders { get; set; } */
        public Dictionary<string, Dictionary<string, BitRanges>> CaptureDecoders { get; set; }

        /// <summary>
        /// Gets or sets RepairMap, Key=Controller Name, Value=(Dictionary) Key=Step(format="STEP[01]"), Value=List of MemRepairMap.
        /// </summary>
        [JsonProperty("REPAIRMAP", Order = 5)]
        public Dictionary<string, Dictionary<string, List<MemRepairMap>>> RepairMap { get; set; }

        /// <summary>Gets or sets REPAIRGROUPS. Key=RepairGroup Name, Value=RepairGroup object.</summary>
        [JsonProperty("REPAIRGROUPS", Order = 6)]
        public Dictionary<string, RepairGroup> RepairGroups { get; set; }

        /// <summary>
        /// Validates the MbistRasterInput instance.
        /// </summary>
        /// <param name="plist">The name of the PList to validate, if empty will validate all plists.</param>
        /// <returns>List of all the errors in string format.</returns>
        public List<string> Validate(string plist = "")
        {
            List<string> errors = new List<string>();
            List<string> plistsToValidate = new List<string>();
            if (plist == string.Empty)
            {
                plistsToValidate = new List<string>(this.PLists.Keys);
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
                    var plistObj = this.GetPListStruct(plistName);
                    if (plistObj == null)
                    {
                        errors.Add($"PList [{plistName}] does not have a [PLists] entry.");
                    }
                    else
                    {
                        foreach (var captGroup in plistObj.CaptureGroups)
                        {
                            if (!this.CaptureDecoders.ContainsKey(captGroup))
                            {
                                errors.Add($"CaptureGroup=[{captGroup}] referenced in PList=[{plistName}] does not exist.");
                            }
                            else
                            {
                                foreach (var controller in this.CaptureDecoders[captGroup].Keys)
                                {
                                    foreach (var mem in this.CaptureDecoders[captGroup][controller].GoIDs)
                                    {
                                        var name = $"{controller}_MEM{mem.MemoryId}";
                                        if (hryMap.ContainsKey(name))
                                        {
                                            mem.HryIndex = hryMap[name];
                                        }
                                        else
                                        {
                                            errors.Add($"Memory=[{name}] in CaptureGroup=[{captGroup}]/PList=[{plistName}] does not have an entry in Json Element [Hry_string].");
                                        }
                                    } // end foreach mem
                                } // end foreach controller
                            }
                        } // end foreach capture group
                    }
                } // end foreach plist
            }

            return errors;
        }

        /// <summary>
        /// Returns the PList struct for the given plist name.
        /// Checks all variations with/without ip scoping.
        /// </summary>
        /// <param name="plist">The name of the PList.</param>
        /// <returns>PList Object.</returns>
        public PList GetPListStruct(string plist)
        {
            if (this.PLists.ContainsKey(plist))
            {
                // if the plist exists, us it directly.
                return this.PLists[plist];
            }
            else if (plist.Contains("::"))
            {
                // if the given plist contains IP scoping, remove it and look for a match.
                var pair = plist.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                var noIpPlist = pair.Last();
                if (this.PLists.ContainsKey(noIpPlist))
                {
                    return this.PLists[noIpPlist];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // The given plist does not contain IP scoping, check if any of the existing plists do.
                foreach (var plistFullName in this.PLists.Keys)
                {
                    if (plistFullName.Contains("::"))
                    {
                        var pair = plistFullName.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        var noIpPlist = pair.Last();
                        if (noIpPlist == plist)
                        {
                            return this.PLists[plistFullName];
                        }
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the number of capture bits in the given capture group.
        /// </summary>
        /// <param name="captureGroup">capture group.</param>
        /// <returns>Number of capture bits in the capture group.</returns>
        public int GetSize(Dictionary<string, BitRanges> captureGroup)
        {
            var min = -1;
            var max = -1;
            foreach (var decoder in captureGroup.Values)
            {
                var rslts = decoder.GetMinMax();
                if (rslts.Item1 < min || min == -1)
                {
                    min = rslts.Item1;
                }

                if (rslts.Item2 > max || max == -1)
                {
                    max = rslts.Item2;
                }
            }

            var size = max - min + 1;
            return size;
        }

        /// <summary>
        /// Given a repair group name, returns the possible repair types.
        /// </summary>
        /// <param name="repairGroup">.</param>
        /// <returns>repair type.</returns>
        public Mbist.RepairType GetRepairType(string repairGroup)
        {
            if (!this.RepairGroups.ContainsKey(repairGroup))
            {
                throw new ArgumentException($"GetRepairType:No match for group=[{repairGroup}]. Valid groups are [{string.Join(",", this.RepairGroups.Keys)}]. Check input file.", "repairGroup");
            }

            bool foundRow = false;
            bool foundCol = false;

            foreach (var r in this.RepairGroups[repairGroup].Repairs)
            {
                if (r.Type == Mbist.RepairType.WORD)
                {
                    return Mbist.RepairType.WORD;
                }
                else if (r.Type == Mbist.RepairType.ROW)
                {
                    foundRow = true;
                }
                else if (r.Type == Mbist.RepairType.COL)
                {
                    foundCol = true;
                }

                if (foundRow && foundCol)
                {
                    return Mbist.RepairType.BOTH;
                }
            }

            if (foundRow)
            {
                return Mbist.RepairType.ROW;
            }

            if (foundCol)
            {
                return Mbist.RepairType.COL;
            }

            return Mbist.RepairType.NONE;

            // FIXME: look for ways to speed this up, it shows up on profiling
            // var allRepairTypes = RepairGroups[repairGroup].Repairs.Select(o => o.Type);
            // if (allRepairTypes.Contains(Mbist.RepairType.ROW) && allRepairTypes.Contains(Mbist.RepairType.COL))
            //     return Mbist.RepairType.BOTH;
            // else if (allRepairTypes.Contains(Mbist.RepairType.ROW))
            //     return Mbist.RepairType.ROW;
            // else if (allRepairTypes.Contains(Mbist.RepairType.COL))
            //     return Mbist.RepairType.COL;
            // else if (allRepairTypes.Contains(Mbist.RepairType.WORD))
            //     return Mbist.RepairType.WORD;
            // else
            //     return Mbist.RepairType.NONE;
        }

        /// <summary>
        /// Gets the Global name which holds the fuse values for the given Repair Group.
        /// </summary>
        /// <param name="group">Repair Group name.</param>
        /// <returns>Global name.</returns>
        public string GetGlobalForRepairGroup(string group)
        {
            if (!this.RepairGroups.ContainsKey(group))
            {
                throw new ArgumentException($"GetGlobalForRepairGroup:No RepairGroup=[{group}]. Valid groups are [{string.Join(",", this.RepairGroups.Keys)}]. Check input file.", "group");
            }

            return this.RepairGroups[group].GlobalName;
        }

        /// <summary>
        /// Checks if this Controller has a RepairGroup associated with it.
        /// </summary>
        /// <param name="controller">Contoller Name.</param>
        /// <returns>True if there exists a repair group for this controller.</returns>
        public bool HasRepair(string controller)
        {
            if (this.RepairMap == null)
            {
                return false;
            }

            return this.RepairMap.ContainsKey(controller);
        }

        /// <summary>
        /// Returns the repair group name for the given controller, step and mem.
        /// Raises an exeption if one does not exist.
        /// </summary>
        /// <param name="controller">Controller name.</param>
        /// <param name="step">Step Index (integer.</param>
        /// <param name="mem">Mem ID value (integer.</param>
        /// <returns>Repair Group Name.</returns>
        public string FindRepairGroup(string controller, int step, int mem)
        {
            if (this.RepairMap == null)
            {
                throw new ArgumentException($"FindRepairGroup:No RepairMap defined. Check input file.", "controller");
            }

            if (!this.RepairMap.ContainsKey(controller))
            {
                throw new ArgumentException($"FindRepairGroup:No RepairMap for controller=[{controller}]. Valid controllers are [{string.Join(",", this.RepairMap.Keys)}]. Check input file.", "controller");
            }

            var stepStr = $"STEP{step}";
            if (!this.RepairMap[controller].ContainsKey(stepStr))
            {
                throw new ArgumentException($"FindRepairGroup:No Step=[{stepStr}] for controller=[{controller}]. Valid Steps are [{string.Join(",", this.RepairMap[controller].Keys)}]. Check input file.", "step");
            }

            var validMems = new List<int>();
            foreach (var memRepairMap in this.RepairMap[controller][stepStr])
            {
                var memList = Mbist.RangeToList(memRepairMap.MemRange);
                if (memList.Contains(mem))
                {
                    return memRepairMap.RepairGroup;
                }

                validMems.AddRange(memList);
            }

            throw new ArgumentException($"FindRepairGroup:No Mem=[{mem}] for controller=[{controller}] Step=[{stepStr}]. Valid Steps are [{string.Join(",", validMems)}]. Check input file.", "mem");
        }

        /*
        /// <summary>
        /// Gets the list of all Controller names in a PList.
        /// </summary>
        /// <param name="plist">PList object.</param>
        /// <returns>List of Controller Names.</returns>
        public List<string> GetControllerNamesInPList(PList plist)
        {
            var retVal = new List<string>();
            foreach (var captGroup in plist.CaptureGroups)
            {
                if (!this.CaptureDecoders.ContainsKey(captGroup))
                {
                    throw new ArgumentException($"GetControllerNamesInPList: No definition for CaptureGroup=[{captGroup}] found.", "plist");
                }

                retVal.AddRange(this.CaptureDecoders[captGroup].Keys);
            }

            retVal.Sort();
            return retVal;
        } */

        /// <summary>Represents the capture data from a single PList.</summary>
        public class PList
        {
            /// <summary>Gets or sets CapturePins.</summary>
            [JsonProperty("CapturePins", Order = 1)]
            public string CapturePins { get; set; }

            /*/// <summary>Gets or sets CaptureCount.</summary>
            [JsonProperty("CaptureCount", Order = 2)]
            public int CaptureCount { get; set; } */

            /// <summary>Gets or sets CaptureInterLeaveMode.</summary>
            [JsonProperty("CaptureInterLeaveMode", Order = 3)]
            [JsonConverter(typeof(StringEnumConverter))]
            public Mbist.CaptureInterLeaveType CaptureInterLeaveMode { get; set; } = Mbist.CaptureInterLeaveType.CycleFirst;

            /// <summary>Gets or sets Captures.</summary>
            [JsonProperty("Captures", Order = 4)]
            public List<string> CaptureGroups { get; set; }

            /*
            /// <summary>
            /// Gets the list of all Controller names in this PList.
            /// </summary>
            /// <returns>List of controller names.</returns>
            public List<string> GetControllerNames()
            {
                var retVal = new List<string>();
                foreach (var captGroup in this.CaptureGroups)
                {
                    if (!this.CaptureGroups)
                    //        public Dictionary<string, Dictionary<string, BitRanges>> CaptureDecoders { get; set; }

                    temp[capture.Name] = capture.Name;
                }

                Dictionary<string, string> temp = new Dictionary<string, string>();
                foreach (var capture in this.CaptureGroups)
                {
                    temp[capture.Name] = capture.Name;
                }

                var retVal = temp.Keys.ToList();
                retVal.Sort();
                return retVal;
            }
            */
            /*
            /// <summary>Represents a capture element int a plist (capture name + repeat count).</summary>
            public class CaptureType
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="CaptureType"/> class.
                /// </summary>
                /// <param name="name">Name of the capture instance.</param>
                /// <param name="count">Repeat count of the capture.</param>
                public CaptureType(string name, int count)
                {
                    this.Name = name;
                    this.Count = count;
                }

                /// <summary>Gets or sets Name.</summary>
                [JsonProperty("Name", Order = 1)]
                public string Name { get; set; }

                /// <summary>Gets or sets Count.</summary>
                [JsonProperty("Count", Order = 2)]
                public int Count { get; set; } = 1;
            } */
        }

        /// <summary>Holds the information to decode a single capture for a single controller.</summary>
        public class BitRanges
        {
            /// <summary>Constant used as default for optional ranges.</summary>
            [JsonIgnore]
            public static readonly string NA = "NA";

            /// <summary>Gets or sets STATUS.</summary>
            [JsonProperty("STATUS", Order = 1)]
            public string Status { get; set; }

            /// <summary>Gets or sets STEP.</summary>
            [JsonProperty("STEP", Order = 2)]
            public string Step { get; set; } = NA;

            /// <summary>Gets or sets ERROR_CNT.</summary>
            [JsonProperty("ERROR_CNT", Order = 3)]
            public string ErrorCnt { get; set; } = NA;

            /// <summary>Gets or sets GOID.</summary>
            [JsonProperty("GOID", Order = 4)]
            public List<Memory> GoIDs { get; set; }

            /// <summary>Gets or sets LOOP_COUNTER.</summary>
            [JsonProperty("LOOP_COUNTER", Order = 5)]
            public string LoopCounter { get; set; }

            /// <summary>Gets or sets ADDR_Z.</summary>
            [JsonProperty("ADDR_Z", Order = 6)]
            public string AddrZ { get; set; } = NA;

            /// <summary>Gets or sets ADDR_X.</summary>
            [JsonProperty("ADDR_X", Order = 7)]
            public string AddrX { get; set; }

            /// <summary>Gets or sets ADDR_Y.</summary>
            [JsonProperty("ADDR_Y", Order = 8)]
            public string AddrY { get; set; } = NA;

            /// <summary>Gets or sets INSTRUCTION.</summary>
            [JsonProperty("INSTRUCTION", Order = 9)]
            public string Instruction { get; set; }

            /// <summary>Gets or sets ALGO_SEL.</summary>
            [JsonProperty("ALGO_SEL", Order = 10)]
            public string Algorithm { get; set; }

            /// <summary>
            /// Returns the min/max bit locations for this capture elemnt.
            /// </summary>
            /// <returns>Tuple Item1=Minimum index, Item2=Maximum location.</returns>
            public Tuple<int, int> GetMinMax()
            {
                var min = -1;
                var max = -1;

                this.GetMinMaxInRange(this.Status, ref min, ref max);
                this.GetMinMaxInRange(this.Step, ref min, ref max);
                this.GetMinMaxInRange(this.ErrorCnt, ref min, ref max);
                this.GetMinMaxInRange(this.LoopCounter, ref min, ref max);
                this.GetMinMaxInRange(this.AddrZ, ref min, ref max);
                this.GetMinMaxInRange(this.AddrX, ref min, ref max);
                this.GetMinMaxInRange(this.AddrY, ref min, ref max);
                this.GetMinMaxInRange(this.Instruction, ref min, ref max);
                this.GetMinMaxInRange(this.Algorithm, ref min, ref max);
                foreach (var goidRange in this.GoIDs.Select(o => o.GoIDBits))
                {
                    this.GetMinMaxInRange(goidRange, ref min, ref max);
                }

                return new Tuple<int, int>(min, max);
            }

            /// <summary>
            /// Gets the Size of this capture.
            /// </summary>
            /// <returns>size.</returns>
            public int Size()
            {
                var rslts = this.GetMinMax();
                var size = rslts.Item2 - rslts.Item1 + 1;
                return size;
            }

            /// <summary>
            /// Gets the HRY index for every Memory.
            /// </summary>
            /// <returns>Dictionary, Key=MemoryID, Value=HRY index.</returns>
            public Dictionary<int, int> GetHryIndexByMemory()
            {
                var retval = new Dictionary<int, int>(); // Key=MemoryID, Value=HryIndex
                foreach (var memory in this.GoIDs)
                {
                    if (!retval.ContainsKey(memory.MemoryId))
                    {
                        retval[memory.MemoryId] = memory.HryIndex;
                    }
                }

                return retval;
            }

            private bool GetMinMaxInRange(string range, ref int min, ref int max)
            {
                if (range == MbistRasterInput.BitRanges.NA)
                {
                    return false;
                }

                var allDelim = new char[] { ',', '-' };
                foreach (var item in range.Split(allDelim))
                {
                    try
                    {
                        var value = int.Parse(item);
                        if (value < min || min == -1)
                        {
                            min = value;
                        }

                        if (value > max || max == -1)
                        {
                            max = value;
                        }
                    }
                    catch (Exception)
                    {
                        // just ignore for now.
                    }
                }

                return true;
            }

            /// <summary>Holds the information for decoding a single Memorys capture.</summary>
            public class Memory
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="Memory"/> class.
                /// </summary>
                /// <param name="id">MemoryID.</param>
                /// <param name="hry">HRY Index.</param>
                /// <param name="range">Bit locations for the memoies GOID bits.</param>
                public Memory(int id, int hry, string range)
                {
                    this.MemoryId = id;
                    this.HryIndex = hry;
                    this.GoIDBits = range;
                }

                /// <summary>Gets or sets MEM.</summary>
                [JsonProperty("MEM", Order = 1)]
                public int MemoryId { get; set; }

                /// <summary>Gets or sets HryIndex (not a JSON Element).</summary>
                [JsonIgnore]
                public int HryIndex { get; set; }

                /// <summary>Gets or sets BITS.</summary>
                [JsonProperty("BITS", Order = 2)]
                public string GoIDBits { get; set; }
            }
        }

        // ===========================================================================================
        // Repair Mapping

        /// <summary>Holds the repair map from the JSON Raster config file.</summary>
        public class MemRepairMap
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MemRepairMap"/> class.
            /// </summary>
            /// <param name="mem">MemoryID.</param>
            /// <param name="group">RepairGroup name.</param>
            public MemRepairMap(string mem, string group)
            {
                this.MemRange = mem;
                this.RepairGroup = group;
            }

            /// <summary>Gets or sets MEM.</summary>
            [JsonProperty("MEM", Order = 1)]
            public string MemRange { get; set; }

            /// <summary>Gets or sets REPAIRGROUP.</summary>
            [JsonProperty("REPAIRGROUP", Order = 2)]
            public string RepairGroup { get; set; }
        }

        /// <summary>Repair Group Class.</summary>
        public class RepairGroup
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RepairGroup"/> class.
            /// </summary>
            /// <param name="name">Global which holds the fuse value for this group.</param>
            /// <param name="value">Current Value of the Fuses.</param>
            /// <param name="contents">List of the reair elements in this group.</param>
            public RepairGroup(string name, string value, List<RepairElement> contents)
            {
                this.GlobalName = name;
                this.Repairs = contents;
                this.GlobalValue = value;
            }

            /// <summary>Gets or sets GLOBALNAME.</summary>
            [JsonProperty("GLOBALNAME", Order = 1)]
            public string GlobalName { get; set; }

            /// <summary>Gets or sets GlobalValue (not part of JSON), used to hold the fuse value.</summary>
            [JsonIgnore]
            public string GlobalValue { get; set; }

            /// <summary>Gets or sets REPAIRS.</summary>
            [JsonProperty("REPAIRS", Order = 2)]
            public List<RepairElement> Repairs { get; set; }

            /// <summary>Gets or sets WIDTH for WORD repair.</summary>
            [JsonProperty("WIDTH", Order = 3)]
            public List<int> Width { get; set; } = new List<int> { 0, 0, 0 };

            /// <summary>Class representing a single repair element.</summary>
            public class RepairElement
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="RepairElement"/> class.
                /// </summary>
                /// <param name="type">Repair Type.</param>
                /// <param name="fuse">Fuse Bits.</param>
                public RepairElement(Mbist.RepairType type, string fuse)
                {
                    this.Type = type;
                    this.FusePos = fuse;
                }

                /// <summary>Gets or sets TYPE.</summary>
                [JsonProperty("TYPE", Order = 1)]
                [JsonConverter(typeof(StringEnumConverter))]
                public Mbist.RepairType Type { get; set; }

                /// <summary>Gets or sets LOG_SIZE.</summary>
                [JsonProperty("LOG_SIZE", Order = 2)]
                public string LogSize { get; set; } = "NA";

                /// <summary>Gets or sets FUSE_POS.</summary>
                [JsonProperty("FUSE_POS", Order = 3)]
                public string FusePos { get; set; }

                /// <summary>Gets or sets STATUS.</summary>
                [JsonProperty("STATUS", Order = 4)]
                [JsonConverter(typeof(StringEnumConverter))]
                public Mbist.RepairStatus Status { get; set; } = Mbist.RepairStatus.AVAIL;
            }
        }
    }
}
