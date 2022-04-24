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

namespace MbistRasterRepairTC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using ARR_MBIST;
    using Newtonsoft.Json;

    /// <summary>
    /// Main class for MBIST Raster/Repair.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "More readable this way.")]
    public class MbistRasterAlgorithm
    {
        /// <summary>
        /// Helper class containing fail information.
        /// </summary>
        public class FailContainer
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FailContainer"/> class.
            /// </summary>
            /// <param name="input">Object containing the JSON configuration file.</param>
            public FailContainer(MbistRasterInput input)
            {
                this.RasterConfig = input;
                this.Contents = new Dictionary<string, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, FailData>>>>>>();
                this.Failrows = new Dictionary<string, Dictionary<int, List<int>>>();
                this.RepairFails = new Dictionary<string, Dictionary<Mbist.RepairType, Dictionary<int, Dictionary<int, Mbist.RepairType>>>>();
                this.RepairedDefects = new Dictionary<string, Dictionary<Mbist.RepairType, List<int>>>();
                this.Count = 0;
            }

            /// <summary>Gets the Number of Defects in this FailContainer object.</summary>
            public int Count { get; private set; }

            /// <summary>Gets or sets Base container for the failure.</summary>
            private Dictionary<string,             // controller Name
                Dictionary<int,                    // Step
                    Dictionary<int,                // Mem
                        Dictionary<int,            // Bank
                            Dictionary<int,        // Row
                                Dictionary<int,    // Col
                                    FailData>>>>>> Contents { get; set; }

            /// <summary>Gets or sets container to keep track of just the failing rows.</summary>
            private Dictionary<string, // controller Name
                Dictionary<int,        // Mem
                    List<int>>> Failrows { get; set; }

            /// <summary>Gets or sets container to track unrepaired defects.</summary>
            private Dictionary<string,                                // RepairGroup Name
                Dictionary<Mbist.RepairType,                          // RepairType (ROW, COL, WORD)
                    Dictionary<int,                                   // row, io or wordv
                        Dictionary<int,                               // io, row or 0.
                            Mbist.RepairType>>>> RepairFails { get; set; }

            /// <summary>Gets or sets container for possible repairs.</summary>
            private Dictionary<string,                       // RepairGroup Name
                Dictionary<Mbist.RepairType,                 // RepairType (ROW, COL, WORD)
                   List<int>>> RepairedDefects { get; set; } // address of defect (row, io or wordv based on type)

            /// <summary>Gets or sets pointer to the JSON Raster file.</summary>
            private MbistRasterInput RasterConfig { get; set; }

            /// <summary>Remove everything from this container.</summary>
            public void Clear()
            {
                this.Count = 0;
                this.Contents.Clear();
                this.Failrows.Clear();
                this.RepairFails.Clear();
                this.RepairedDefects.Clear();
            }

            /// <summary>
            /// Returns true if there are repairs needed, otherwise False.
            /// </summary>
            /// <returns>True if repairs are needed.</returns>
            public bool RepairsNeeded()
            {
                return this.RepairFails.Keys.Count > 0;
            }

            /// <summary>
            /// Returns true if the specified RepairGroup has any defects which need repairs.
            /// </summary>
            /// <param name="group">Repair Group name.</param>
            /// <returns>True if repairs are needed.</returns>
            public bool RepairsNeeded(string group)
            {
                if (!this.RepairFails.ContainsKey(group))
                {
                    return false;
                }

                return this.RepairFails[group].Keys.Count > 0;
            }

            /// <summary>
            /// Returns true if the specified RepairGroup has any defects which need repairs of the specified type.
            /// </summary>
            /// <param name="group">Repair Group name.</param>
            /// <param name="type">Repair Type.</param>
            /// <returns>True if repairs are needed.</returns>
            public bool RepairsNeeded(string group, Mbist.RepairType type)
            {
                if (!this.RepairFails.ContainsKey(group))
                {
                    return false;
                }

                if (!this.RepairFails[group].ContainsKey(type))
                {
                    return false;
                }

                return this.RepairFails[group][type].Keys.Count > 0;
            }

            /// <summary>
            /// Gets all the Defects for the specified RepairGroup and Type.
            /// </summary>
            /// <param name="group">Name of the RepairGroup.</param>
            /// <param name="type">Defect Type.</param>
            /// <returns>List of defects.</returns>
            public List<int> FailAddresses(string group, Mbist.RepairType type)
            {
                if (!this.RepairFails.ContainsKey(group))
                {
                    return new List<int>();
                }

                if (!this.RepairFails[group].ContainsKey(type))
                {
                    return new List<int>();
                }

                return this.RepairFails[group][type].Keys.ToList();
            }

            /// <summary>
            /// Assigns defects for repair in the given RepairGroup/Type.
            /// </summary>
            /// <param name="group">Repair Group.</param>
            /// <param name="type">Repair Type.</param>
            /// <returns>true on success.</returns>
            public bool MarkForRepair(string group, Mbist.RepairType type)
            {
                if (!this.RepairFails.ContainsKey(group))
                {
                    throw new ArgumentException($"MarkForRepair:No RepairGroup=[{group}] in Fail container. Valid groups are [{string.Join(",", this.RepairFails.Keys)}].", "group");
                }

                if (!this.RepairFails[group].ContainsKey(type))
                {
                    throw new ArgumentException($"MarkForRepair:No Type=[{type}] for RepairGroup=[{group}] in Fail container. Valid types are [{string.Join(",", this.RepairFails[group].Keys)}].", "type");
                }

                var retval = true;
                var addresses = this.RepairFails[group][type].Keys.ToList(); // Since MarkForRepair updates RepairFails, need to separate this out.
                foreach (var addr in addresses)
                {
                    if (this.RepairFails.ContainsKey(group) && this.RepairFails[group].ContainsKey(type) && this.RepairFails[group][type].ContainsKey(addr))
                    {
                        retval &= this.MarkForRepair(group, type, addr);
                    }
                }

                return retval;
            }

            /// <summary>
            /// Assigns the given defect a Repair element.
            /// </summary>
            /// <param name="group">Name of the Repair Group.</param>
            /// <param name="type">Type of the Repair.</param>
            /// <param name="address">Address of the defect.</param>
            /// <returns>true on succes.</returns>
            public bool MarkForRepair(string group, Mbist.RepairType type, int address)
            {
                if (!this.RepairFails.ContainsKey(group))
                {
                    throw new ArgumentException($"MarkForRepair:No RepairGroup=[{group}] in Fail container. Valid groups are [{string.Join(",", this.RepairFails.Keys)}].", "group");
                }

                if (!this.RepairFails[group].ContainsKey(type))
                {
                    throw new ArgumentException($"MarkForRepair:No Type=[{type}] for RepairGroup=[{group}] in Fail container. Valid types are [{string.Join(",", this.RepairFails[group].Keys)}].", "type");
                }

                if (!this.RepairFails[group][type].ContainsKey(address))
                {
                    throw new ArgumentException($"MarkForRepair:No Address=[{address} for Type=[{type}]/RepairGroup=[{group}] in Fail container. Valid addresses are [{string.Join(",", this.RepairFails[group][type].Keys)}].", "address");
                }

                // remove this defect.
                if (type == Mbist.RepairType.ROW)
                {
                    // remove the corresponding failures from the COL type
                    this.RemoveDefectFromRepair(group, Mbist.RepairType.COL, this.RepairFails[group][type][address].Keys.ToList());
                }
                else if (type == Mbist.RepairType.COL)
                {
                    // remove the corresponding failures from the ROW type
                    this.RemoveDefectFromRepair(group, Mbist.RepairType.ROW, this.RepairFails[group][type][address].Keys.ToList());
                }

                this.RemoveDefectFromRepair(group, type, address);

                // update the repaired container
                if (!this.RepairedDefects.ContainsKey(group))
                {
                    this.RepairedDefects[group] = new Dictionary<Mbist.RepairType, List<int>>();
                }

                if (!this.RepairedDefects[group].ContainsKey(type))
                {
                    this.RepairedDefects[group][type] = new List<int>();
                }

                this.RepairedDefects[group][type].Add(address);

                // Find an open slot for this defect.
                var foundOpenFuse = false;
                foreach (var repair in this.RasterConfig.RepairGroups[group].Repairs)
                {
                    if (repair.Status == Mbist.RepairStatus.AVAIL && repair.Type == type)
                    {
                        foundOpenFuse = true;
                        var fuseValue = address;
                        if (repair.LogSize != "NA")
                        {
                            // defect = df % int(repairGroups[grp]['LOG_SIZE'][idx])
                            fuseValue = address % int.Parse(repair.LogSize);
                        }

                        var fuseStr = this.RasterConfig.RepairGroups[group].GlobalValue;
                        var fuseValueStr = Convert.ToString(fuseValue, 2).PadLeft(Mbist.RangeSize(repair.FusePos), '0');

                        this.RasterConfig.RepairGroups[group].GlobalValue = Mbist.SetSubData(repair.FusePos, fuseValueStr, fuseStr);
                        repair.Status = Mbist.RepairStatus.USED;

                        break;
                    }
                }

                return foundOpenFuse;
            }

            /// <summary>
            /// Gets the names of all the RepairGroups that have defects.
            /// </summary>
            /// <returns>List of RepairGroup names.</returns>
            public List<string> GetRepairGroups()
            {
                return this.RepairFails.Keys.ToList();
            }

            /// <summary>
            /// Counts the number of repairs needed in the given defect.
            /// </summary>
            /// <param name="group">RepairGroup name.</param>
            /// <param name="type">Type of repair.</param>
            /// <param name="address">Address of the defect.</param>
            /// <returns>the number of repairs needed.</returns>
            public int CountNeededRepairs(string group, Mbist.RepairType type, int address)
            {
                if (!this.RepairFails.ContainsKey(group))
                {
                    return 0;
                }

                if (!this.RepairFails[group].ContainsKey(type))
                {
                    return 0;
                }

                if (!this.RepairFails[group][type].ContainsKey(address))
                {
                    return 0;
                }

                return this.RepairFails[group][type][address].Keys.Count;
            }

            /// <summary>
            /// Counts the number of repairs needed in the given defect.
            /// </summary>
            /// <param name="group">RepairGroup name.</param>
            /// <param name="type">Type of repair.</param>
            /// <returns>the number of repairs needed.</returns>
            public int CountNeededRepairs(string group, Mbist.RepairType type)
            {
                if (!this.RepairFails.ContainsKey(group))
                {
                    return 0;
                }

                if (!this.RepairFails[group].ContainsKey(type))
                {
                    return 0;
                }

                return this.RepairFails[group][type].Keys.Count;
            }

            /// <summary>
            /// Gets the number of failing rows for the given memory.
            /// </summary>
            /// <param name="controller">Name of the Controller.</param>
            /// <param name="mem">Memory ID.</param>
            /// <returns>Number of failing rows.</returns>
            public int FailingRows(string controller, int mem)
            {
                try
                {
                    return this.Failrows[controller][mem].Count;
                }
                catch (KeyNotFoundException)
                {
                    return 0;
                }
            }

            /// <summary>
            /// Gets a list of all the Controllers.
            /// </summary>
            /// <returns>List of controller names.</returns>
            public IEnumerable<string> GetAllControllers()
            {
                foreach (var cont in this.Contents.Keys)
                {
                    yield return cont;
                }
            }

            /// <summary>
            /// Gets all the valid STEP values for the given controller.
            /// </summary>
            /// <param name="controller">Controller Name.</param>
            /// <returns>List of valid STEP values.</returns>
            public IEnumerable<int> GetAllSteps(string controller)
            {
                foreach (var step in this.Contents[controller].Keys)
                {
                    yield return step;
                }
            }

            /// <summary>
            /// Gets all the valid MemoryIDs for the given controller and step.
            /// </summary>
            /// <param name="controller">Controller Name.</param>
            /// <param name="step">STEP value.</param>
            /// <returns>List of MemoryIDs.</returns>
            public IEnumerable<int> GetAllMems(string controller, int step)
            {
                foreach (var mem in this.Contents[controller][step].Keys)
                {
                    yield return mem;
                }
            }

            /// <summary>
            /// Get all the addresses of failures.
            /// </summary>
            /// <param name="controller">Controller Name.</param>
            /// <param name="step">STEP value.</param>
            /// <param name="mem">MemoryID.</param>
            /// <returns>List of FailData.</returns>
            public IEnumerable<FailData> GetAllFailAddresses(string controller, int step, int mem)
            {
                foreach (var bank in this.Contents[controller][step][mem].Keys)
                {
                    foreach (var row in this.Contents[controller][step][mem][bank].Keys)
                    {
                        foreach (var col in this.Contents[controller][step][mem][bank][row].Keys)
                        {
                            yield return this.Contents[controller][step][mem][bank][row][col];
                        }
                    }
                }
            }

            /// <summary>
            /// Get all the saved fail addresses.
            /// </summary>
            /// <returns>List of FailData.</returns>
            public IEnumerable<FailData> GetAllFailAddresses()
            {
                foreach (var cont in this.GetAllControllers())
                {
                    foreach (var step in this.GetAllSteps(cont))
                    {
                        foreach (var mem in this.GetAllMems(cont, step))
                        {
                            foreach (var bank in this.Contents[cont][step][mem].Keys)
                            {
                                foreach (var row in this.Contents[cont][step][mem][bank].Keys)
                                {
                                    foreach (var col in this.Contents[cont][step][mem][bank][row].Keys)
                                    {
                                        yield return this.Contents[cont][step][mem][bank][row][col];
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Records a defect at the given.
            /// </summary>
            /// <param name="cont">Controller Name.</param>
            /// <param name="step">STEP Value.</param>
            /// <param name="mem">MemoryID.</param>
            /// <param name="bank">BANK Value (Z address).</param>
            /// <param name="row">ROW Value (X address).</param>
            /// <param name="col">COLUMN Value (Yaddress).</param>
            /// <param name="bit">Failing Bit/IO location.</param>
            /// <returns>true on success.</returns>
            public bool Add(string cont, int step, int mem, int bank, int row, int col, int bit)
            {
                if (!this.Contents.ContainsKey(cont))
                {
                    this.Contents[cont] = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, FailData>>>>>();
                }

                if (!this.Contents[cont].ContainsKey(step))
                {
                    this.Contents[cont][step] = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, FailData>>>>();
                }

                if (!this.Contents[cont][step].ContainsKey(mem))
                {
                    this.Contents[cont][step][mem] = new Dictionary<int, Dictionary<int, Dictionary<int, FailData>>>();
                }

                if (!this.Contents[cont][step][mem].ContainsKey(bank))
                {
                    this.Contents[cont][step][mem][bank] = new Dictionary<int, Dictionary<int, FailData>>();
                }

                if (!this.Contents[cont][step][mem][bank].ContainsKey(row))
                {
                    this.Contents[cont][step][mem][bank][row] = new Dictionary<int, FailData>();
                }

                if (!this.Contents[cont][step][mem][bank][row].ContainsKey(col))
                {
                    this.Contents[cont][step][mem][bank][row][col] = new FailData(cont, step, mem, bank, row, col);
                }

                var newBit = this.Contents[cont][step][mem][bank][row][col].Add(bit);
                if (newBit)
                {
                    this.Count++;
                }

                if (!this.Failrows.ContainsKey(cont))
                {
                    this.Failrows[cont] = new Dictionary<int, List<int>>();
                }

                if (!this.Failrows[cont].ContainsKey(mem))
                {
                    this.Failrows[cont][mem] = new List<int>();
                }

                if (!this.Failrows[cont][mem].Contains(row))
                {
                    this.Failrows[cont][mem].Add(row);
                }

                // update the repair struct.
                // this controller might not have repair so an exception isn't fatal
                // FIXME: Performance of FindRepairGroup and this whole try/catch block. Using HasRepair to speed it up...may need to add step/mem as well.
                var repGroup = string.Empty;
                if (this.RasterConfig.HasRepair(cont))
                {
                    try
                    {
                        repGroup = this.RasterConfig.FindRepairGroup(cont, step, mem);
                    }
                    catch (ArgumentException e)
                    {
                        Prime.Services.ConsoleService.PrintError($"non-fatal error, failed to add defect to repair container: {e.Message}");
                    }
                }

                if (!string.IsNullOrEmpty(repGroup))
                {
                    var repType = this.RasterConfig.GetRepairType(repGroup);
                    if (!this.RepairFails.ContainsKey(repGroup))
                    {
                        this.RepairFails[repGroup] = new Dictionary<Mbist.RepairType, Dictionary<int, Dictionary<int, Mbist.RepairType>>>();
                    }

                    if (repType == Mbist.RepairType.WORD)
                    {
                        var lengths = this.RasterConfig.RepairGroups[repGroup].Width;
                        var x = Convert.ToString(row, 2).PadLeft(lengths[0], '0');
                        var y = Convert.ToString(col, 2).PadLeft(lengths[1], '0');
                        var z = Convert.ToString(bank, 2).PadLeft(lengths[2], '0');
                        var wordvStr = $"{x}{y}{z}";
                        var wordv = Bin2dec(wordvStr);
                        if (!this.RepairFails[repGroup].ContainsKey(Mbist.RepairType.WORD))
                        {
                            this.RepairFails[repGroup][Mbist.RepairType.WORD] = new Dictionary<int, Dictionary<int, Mbist.RepairType>>();
                        }

                        if (!this.RepairFails[repGroup][Mbist.RepairType.WORD].ContainsKey(wordv))
                        {
                            this.RepairFails[repGroup][Mbist.RepairType.WORD][wordv] = new Dictionary<int, Mbist.RepairType>();
                        }

                        this.RepairFails[repGroup][Mbist.RepairType.WORD][wordv][0] = Mbist.RepairType.NONE;
                        Prime.Services.ConsoleService.PrintDebug($"<debug> Added WORD repair Controller=[{cont}] Step=[{step}] Mem=[{mem}] Row=[{row}] Col=[{col}] Bank=[{bank}] WordV=[{wordv}]");
                    }

                    if (repType == Mbist.RepairType.ROW || repType == Mbist.RepairType.BOTH)
                    {
                        if (!this.RepairFails[repGroup].ContainsKey(Mbist.RepairType.ROW))
                        {
                            this.RepairFails[repGroup][Mbist.RepairType.ROW] = new Dictionary<int, Dictionary<int, Mbist.RepairType>>();
                        }

                        if (!this.RepairFails[repGroup][Mbist.RepairType.ROW].ContainsKey(row))
                        {
                            this.RepairFails[repGroup][Mbist.RepairType.ROW][row] = new Dictionary<int, Mbist.RepairType>();
                        }

                        // point the failing row to the failing col/bit
                        this.RepairFails[repGroup][Mbist.RepairType.ROW][row][bit] = Mbist.RepairType.COL;
                        Prime.Services.ConsoleService.PrintDebug($"<debug> Added ROW repair Group=[{repGroup}] Row=[{row}] IO=[{bit}]");
                    }

                    if (repType == Mbist.RepairType.COL || repType == Mbist.RepairType.BOTH)
                    {
                        if (!this.RepairFails[repGroup].ContainsKey(Mbist.RepairType.COL))
                        {
                            this.RepairFails[repGroup][Mbist.RepairType.COL] = new Dictionary<int, Dictionary<int, Mbist.RepairType>>();
                        }

                        if (!this.RepairFails[repGroup][Mbist.RepairType.COL].ContainsKey(bit))
                        {
                            this.RepairFails[repGroup][Mbist.RepairType.COL][bit] = new Dictionary<int, Mbist.RepairType>();
                        }

                        // point the failing col to the failing row
                        this.RepairFails[repGroup][Mbist.RepairType.COL][bit][row] = Mbist.RepairType.ROW;
                        Prime.Services.ConsoleService.PrintDebug($"<debug> Added COL repair Group=[{repGroup}] IO=[{bit}] Row=[{row}]");
                    }
                }

                return newBit;
            }

            private void RemoveDefectFromRepair(string group, Mbist.RepairType type, int addr)
            {
                if (!this.RepairFails.ContainsKey(group) || !this.RepairFails[group].ContainsKey(type) || !this.RepairFails[group][type].ContainsKey(addr))
                {
                    return;
                }

                this.RepairFails[group][type].Remove(addr);
                if (this.RepairFails[group][type].Count == 0)
                {
                    this.RepairFails[group].Remove(type);
                }

                if (this.RepairFails[group].Count == 0)
                {
                    this.RepairFails.Remove(group);
                }
            }

            private void RemoveDefectFromRepair(string group, Mbist.RepairType type, List<int> addresses)
            {
                if (this.RepairFails.ContainsKey(group) && this.RepairFails[group].ContainsKey(type))
                {
                    foreach (var addr in addresses)
                    {
                        if (this.RepairFails[group][type].ContainsKey(addr))
                        {
                            this.RemoveDefectFromRepair(group, type, addr);
                        }
                    }
                }
            }

            /// <summary>
            /// Base container to hold fail data.
            /// </summary>
            public class FailData
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="FailData"/> class.
                /// </summary>
                /// <param name="cont">Controller Name.</param>
                /// <param name="step">STEP Value.</param>
                /// <param name="mem">Memory ID.</param>
                /// <param name="bank">Bank Value (Z Address).</param>
                /// <param name="row">Row Value (X Address).</param>
                /// <param name="col">Column Value (Y Address).</param>
                public FailData(string cont, int step, int mem, int bank, int row, int col)
                {
                    this.Controller = cont;
                    this.Step = step;
                    this.Mem = mem;
                    this.Bank = bank;
                    this.Row = row;
                    this.Col = col;
                    this.Bits = new List<int>();
                }

                /// <summary>Gets the Controller name for this failure.</summary>
                public string Controller { get; private set; }

                /// <summary>Gets the STEP value for this failure.</summary>
                public int Step { get; private set; }

                /// <summary>Gets the MemoryID for this failure.</summary>
                public int Mem { get; private set; }

                /// <summary>Gets the Bank Value (Z address) for this failure.</summary>
                public int Bank { get; private set; }

                /// <summary>Gets the Row Value (X address) for this failure.</summary>
                public int Row { get; private set; }

                /// <summary>Gets the Column Value (Yaddress) for this failure.</summary>
                public int Col { get; private set; }

                /// <summary>Gets the Controller name for this failure.</summary>
                public List<int> Bits { get; private set; }

                /// <summary>
                /// Adds a new failure.
                /// </summary>
                /// <param name="failBit">Failing Bit/IO.</param>
                /// <returns>true on success.</returns>
                public bool Add(int failBit)
                {
                    if (!this.Bits.Contains(failBit))
                    {
                        this.Bits.Add(failBit);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                /// <summary>
                /// Gets all the failing Bits/IOs at this address.
                /// </summary>
                /// <returns>List of failing Bits/IOs.</returns>
                public IEnumerable<int> GetAllBits()
                {
                    this.Bits.Sort();
                    foreach (var it in this.Bits)
                    {
                        yield return it;
                    }
                }
            }
        }

        /// <summary>Gets a value indicating whether the algorithm should include repair.</summary>
        public bool RepairMode { get; private set; }

        /// <summary>Gets the FailDatabase.</summary>
        public FailContainer FailDatabase { get; private set; }

        /// <summary>Gets or sets the uservar collection name for the X,Y identifiers for this DUT (for the TFile).</summary>
        public string DutCollection { get; set; } = "SCVars"; // FIXME - this only works with sort I think. need to make this more robust

        /// <summary>Gets or sets the uservar variable name for the X identifiers for this DUT (for the TFile).</summary>
        public string DutXGlobal { get; set; } = "SC_WAFERX"; // FIXME - this only works with sort I think. need to make this more robust

        /// <summary>Gets or sets the uservar variable name for the Y identifiers for this DUT (for the TFile).</summary>
        public string DutYGlobal { get; set; } = "SC_WAFERY"; // FIXME - this only works with sort I think. need to make this more robust

        /// <summary>Gets a value indicating whether the Controller Status has failed.</summary>
        public bool ControllerStatusFailure { get; private set; } = false;

        /// <summary>Gets a value indicating whether an MAF (massive failure) has occurred.</summary>
        public bool MafFlag { get; private set; } = false;

        /// <summary>Gets a value indicating whether Repair has been successful.</summary>
        public bool Repaired { get; private set; } = false;

        private bool SAR_RUN_ALL_LOOP_OVERRIDE { get; set; } = false;  // global, not used - FIXME

        private MbistRasterInput RasterConfig { get; set; }

        private bool FAFIMode { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1025:Code should not contain multiple whitespace in a row", Justification = "Makes HRY map more readable")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1001:Commas should not be preceded by whitespace", Justification = "Makes HRY map more readable")]
        private static readonly Dictionary<string, char> HryTable = new Dictionary<string, char>
        {
            // from HRY code (FIXME: find some way to share)
            { "untested"          , 'U' },
            { "pass"              , '1' },
            { "fail"              , '0' },
            { "repairable"        , 'Y' },
            { "unrepairable"      , 'N' },
            { "pass_retest"       , 'P' },
            { "fail_retest"       , 'F' },
            { "inconsist_ps_fg"   , '6' },
            { "inconsist_fs_pg"   , '5' },
            { "inconsist_pst_fail", '7' },
            { "cont_fail"         , '8' },
            { "pattern_issue"     , 'I' },

            // Raster/Repair specific
            { "rep_ifnoshare"     , 'D' },
            { "unrep_noshare5to8" , 'E' },
            { "unrep_noshare9to12", 'G' },
            { "mass_failure"      , 'H' },
            { "no_rasterdet"      , 'J' },
            { "inconsist_rast"    , 'K' },
            { "rep_notapplied"    , 'L' },
        };

        /// <summary>
        /// Convert the given binary string to hex.
        /// </summary>
        /// <param name="sbin">Binary String.</param>
        /// <param name="reverse">True if the string should be reversed befor converting to hex.</param>
        /// <param name="prefix">If true, hex value will be len'hvalue format.  eg 9'h003.</param>
        /// <returns>Hex string.</returns>
        public static string Bin2hex(string sbin, bool reverse = false, bool prefix = false)
        {
            /*if (sbin.Length > 64)
            {
                throw new ArgumentException($"Bin2hex: binary must be <=64 bits, got [{sbin}] (len={sbin.Length}).");
            } */

            if (reverse)
            {
                sbin = Mbist.StringReverse(sbin);
            }

            // need to break the binary string into 64 bit chunks in order to handle really big strings.
            /*             var max = includeRemainder ? str.Length : str.Length - partLength + 1;
            for (var i = offset; i < max; i += partLength)
            {
                yield return str.Substring(i, Math.Min(partLength, str.Length - i));
            }
*/
            string hex = string.Empty;
            if (sbin.Length <= 64)
            {
                // If its <=64 bits, just convert it in one shot, no need to be fancy...
                hex = Convert.ToUInt64(sbin, 2).ToString("X").PadLeft((sbin.Length + 3) / 4, '0');
            }
            else
            {
                // its bigger than 64 bits so it needs to be split up before converting it in pieces.
                var partLen = 64;
                var paddedWidth = ((sbin.Length + partLen - 1) / partLen) * partLen; // pad it to the next multiple of 64
                var mod64Bin = sbin.PadLeft(paddedWidth, '0');
                var hexSize = 16 - ((paddedWidth - sbin.Length) / 4);
                List<string> tmpLst = new List<string>();
                for (var i = 0; i < mod64Bin.Length; i += partLen)
                {
                    var subStr = mod64Bin.Substring(i, partLen);
                    tmpLst.Add(Convert.ToUInt64(subStr, 2).ToString("X").PadLeft(hexSize, '0'));
                    hexSize = 16; // the first one might have been less than 16 but all the others are full 64 bits (16 hex).
                }

                hex = string.Join(string.Empty, tmpLst);
            }

            if (prefix)
            {
                return $"{sbin.Length}'h{hex}";
            }
            else
            {
                return hex;
            }
        }

        /// <summary>
        /// Convert the given binary string to Decimal.
        /// </summary>
        /// <param name="sbin">Binary String.</param>
        /// <param name="reverse">True if the string should be reversed befor converting.</param>
        /// <returns>Int32.</returns>
        public static int Bin2dec(string sbin, bool reverse = false)
        {
            if (sbin.Length > 33)
            {
                throw new ArgumentException($"Bin2dec: binary must be <=32 bits, got [{sbin}] (len={sbin.Length}).");
            }

            if (reverse)
            {
                sbin = Mbist.StringReverse(sbin);
            }

            int dec = Convert.ToInt32(sbin, 2);
            return dec;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MbistRasterAlgorithm"/> class.
        /// </summary>
        /// <param name="input">Raster JSON input structure.</param>
        /// <param name="fafiMode">True if this should run in FAFI mode.</param>
        /// <param name="repairMode">True if Repair should be attempted.</param>
        public MbistRasterAlgorithm(MbistRasterInput input = null,  bool fafiMode = true, bool repairMode = false)
        {
            this.FAFIMode = fafiMode;
            this.RepairMode = repairMode;
            this.RasterConfig = input;
            this.FailDatabase = new FailContainer(this.RasterConfig);

            this.ControllerStatusFailure = false;
            this.MafFlag = false;
            this.Repaired = false;
        }

        /// <summary>
        /// Clears out any previously stored data.
        /// </summary>
        /// <param name="contFail">Value for ControllerStatus Flag (defaults to false).</param>
        /// <param name="mafFlag">Value for MAF Flag (defaults to false).</param>
        /// <param name="repaired">Value for Repaired flag (defaults to false).</param>
        public void Initialize(bool contFail = false,  bool mafFlag = false, bool repaired = false)
        {
            this.ControllerStatusFailure = contFail;
            this.MafFlag = mafFlag;
            this.Repaired = repaired;
            this.FailDatabase = new FailContainer(this.RasterConfig);
        }

        /// <summary>
        /// Loads the given JSON config file into the Input structure, returns null on any error.
        /// </summary>
        /// <param name="rasterfile">Name of the JSON file to lao.</param>
        /// <returns>MbistRasterInput loaded from the file.</returns>
        public virtual MbistRasterInput LoadInputFile(string rasterfile)
        {
            string localFilePath = DDG.FileUtilities.GetFile(rasterfile);

            try
            {
                this.RasterConfig = JsonConvert.DeserializeObject<MbistRasterInput>(File.ReadAllText(localFilePath));
                var errors = this.RasterConfig.Validate();
                if (errors.Count != 0)
                {
                    Prime.Services.ConsoleService.PrintError($"Errors detected while reading Raster/Repair JSON input file = [{rasterfile}].");
                    foreach (var err in errors)
                    {
                        Prime.Services.ConsoleService.PrintError($"\t{err}");
                    }

                    return null;
                }
            }
            catch (JsonException ex)
            {
                Prime.Services.ConsoleService.PrintError($"Error, failed to load raster file=[{rasterfile}]. Exception=[{ex.Message}].");
                return null;
            }

            return this.RasterConfig;
        }

        /// <summary>
        /// Writes the current FailDatabase to the Raster log (TFile).
        /// </summary>
        /// <param name="instanceName">Instance name to use in the log.</param>
        /// <returns>true on success.</returns>
        public bool WriteRasterLog(string instanceName)
        {
            // don't print on an MAF or STATUS failure
            if (this.MafFlag || this.ControllerStatusFailure)
            {
                return true;
            }

            // FIXME - error checking
            var dutX = "0";
            var dutY = "0";
            if (Prime.Services.UserVarService.Exists(this.DutCollection, this.DutXGlobal) &&
                Prime.Services.UserVarService.Exists(this.DutCollection, this.DutYGlobal))
            {
                dutX = Prime.Services.UserVarService.GetStringValue(this.DutCollection, this.DutXGlobal);
                dutY = Prime.Services.UserVarService.GetStringValue(this.DutCollection, this.DutYGlobal);
                Prime.Services.ConsoleService.PrintDebug($"Assigned DUT X/Y from {this.DutCollection}.{this.DutXGlobal} and {this.DutCollection}.{this.DutYGlobal}");
            }
            else if (Prime.Services.UserVarService.Exists(this.DutCollection, "TP_UNIT_ID"))
            {
                var unit = Prime.Services.UserVarService.GetStringValue(this.DutCollection, "TP_UNIT_ID");
                var l = unit.Split('_');
                if (l.Length > 3)
                {
                    dutX = l[2];
                    dutY = l[3];
                    Prime.Services.ConsoleService.PrintDebug($"Assigned DUT X/Y from {this.DutCollection}.TP_UNIT_ID=[{unit}].");
                }
                else
                {
                    Prime.Services.ConsoleService.PrintDebug($"{this.DutCollection}.TP_UNIT_ID exists but is invalid, using default X/Y values.");
                }
            }
            else
            {
                Prime.Services.ConsoleService.PrintDebug($"Cannot find UserVars for X/Y values, using defaults.");
            }

            var dutHeader = $"DUT {dutX},{dutY}\nTest: mbist2_{instanceName}\n";
            var tfileBuffer = new StringBuilder();

            foreach (var cont in this.FailDatabase.GetAllControllers())
            {
                var contItems = cont.Split('_');

                // FIXME - error checking
                var tap = contItems[0];
                var wtap = contItems[1];
                var controller = contItems[2];

                foreach (var step in this.FailDatabase.GetAllSteps(cont))
                {
                    tfileBuffer.Append(dutHeader);
                    foreach (var mem in this.FailDatabase.GetAllMems(cont, step))
                    {
                        tfileBuffer.Append($"Array: {tap}.{wtap}.{controller}.STEP{step}.MEM{mem}\n");
                        foreach (var failure in this.FailDatabase.GetAllFailAddresses(cont, step, mem))
                        {
                            foreach (var bit in failure.GetAllBits())
                            {
                                tfileBuffer.Append($"{failure.Bank},{failure.Row},{failure.Col},{bit}\n");
                            }
                        }
                    }
                }
            }

            // output the Tfile
            var tfileStr = tfileBuffer.ToString();
            if (!string.IsNullOrEmpty(tfileStr))
            {
                Prime.Services.DatalogService.WriteToTFile(tfileStr);
                Prime.Services.ConsoleService.PrintDebug($"[TFILE]{tfileStr.Replace("\n", "\n[TFILE]")}");
            }
            else
            {
                Prime.Services.ConsoleService.PrintDebug("No defects to write to Raster Logfile.");
            }

            return true;
        }

        /// <summary>
        /// Gets the values for the repair fuses for the given RepairGroups.
        /// </summary>
        /// <param name="repairGroups">Names of the Repair Groups.</param>
        /// <returns>Dictionary, Key=RepairGroup Name, Value=Current Fuse Value.</returns>
        public Dictionary<string, string> GetRepairGlobalValues(List<string> repairGroups)
        {
            Prime.Services.ConsoleService.PrintDebug($"[GetRepairGlobalValues] Getting Repair globals from Prime SharedStorage.");
            var retval = new Dictionary<string, string>();
            foreach (var groupName in repairGroups)
            {
                var global = this.RasterConfig.GetGlobalForRepairGroup(groupName);
                string globalValue = Prime.Services.SharedStorageService.GetStringRowFromTable(global, Prime.SharedStorageService.Context.DUT);

                retval[groupName] = globalValue;
                Prime.Services.ConsoleService.PrintDebug($"<debug> GetRepairGlobalValues: RepairGroup=[{groupName}] Global=[{global}] Value=[{globalValue}].");
            }

            return retval;
        }

        /// <summary>
        /// Attempts to combine the fuse values.
        /// </summary>
        /// <param name="values">List of fuse values.</param>
        /// <returns>Single merged fuse value.</returns>
        public string MergeFuseValues(List<string> values)
        {
            if (values.Count == 1)
            {
                return values[0];
            }

            var retval = values[0].ToCharArray();
            for (var i = 1; i < values.Count; i++)
            {
                var nextVal = values[i].ToCharArray();
                for (var c = 0; c < nextVal.Length; c++)
                {
                    if (nextVal[c] != 'X' && retval[c] == 'X')
                    {
                        retval[c] = nextVal[c];
                    }
                    else if (nextVal[c] != retval[c] && nextVal[c] != 'X' && retval[c] != 'X')
                    {
                        throw new ArgumentException($"MergeFuseValues: Cannot merge [{new string(retval)}] with [{values[i]}] bit=[{i}]", "values");
                    }
                }
            }

            return new string(retval);
        }

        /// <summary>
        /// Saves the fuse values for the given repair groups.
        /// </summary>
        /// <param name="repairGroups">Repair Group name.</param>
        /// <returns>true on success.</returns>
        public bool SetRepairGlobalValues(List<string> repairGroups)
        {
            var fuseValues = new Dictionary<string, List<string>>();
            foreach (var groupName in repairGroups)
            {
                var global = this.RasterConfig.GetGlobalForRepairGroup(groupName);
                if (!fuseValues.ContainsKey(global))
                {
                    fuseValues[global] = new List<string>();
                }

                fuseValues[global].Add(this.RasterConfig.RepairGroups[groupName].GlobalValue);
            }

            Prime.Services.ConsoleService.PrintDebug($"[SetRepairGlobalValues] Writing Repair globals to Prime SharedStorage.");
            foreach (var global in fuseValues.Keys)
            {
                var finalValue = this.MergeFuseValues(fuseValues[global]);
                Prime.Services.ConsoleService.PrintDebug($"<debug> Writing [{finalValue}] to [{global}]");
                Prime.Services.SharedStorageService.InsertRowAtTable(global, finalValue, Prime.SharedStorageService.Context.DUT);
            }

            return true;
        }

        /// <summary>
        /// Updates the RepairStatus fields in the global RasterConfig based
        /// on the given fuse values.
        /// </summary>
        /// <param name="repairFuseValues">Fuse Values, Key=RepairGroup Name, Value=Fuse Value.</param>
        /// <returns>true on success.</returns>
        public bool UpdateRepairStatusFromFuses(Dictionary<string, string> repairFuseValues)
        {
            var repairAvailable = false;
            foreach (var repairGroup in repairFuseValues.Keys)
            {
                var repairFuse = repairFuseValues[repairGroup];
                this.RasterConfig.RepairGroups[repairGroup].GlobalValue = repairFuse;
                foreach (var repair in this.RasterConfig.RepairGroups[repairGroup].Repairs)
                {
                    var fuse = Mbist.GetSubData(repair.FusePos, repairFuse);
                    if (fuse.Contains("X"))
                    {
                        // this position is available
                        repairAvailable = true;
                        repair.Status = Mbist.RepairStatus.AVAIL;
                    }
                    else
                    {
                        repair.Status = Mbist.RepairStatus.USED;
                    }

                    Prime.Services.ConsoleService.PrintDebug($"UpdateRepairStatus: Type:{repair.Type} Pos:{repair.FusePos} Value:{fuse} Status:{repair.Status}");
                }
            }

            return repairAvailable;
        }

        /// <summary>
        /// Returns the available repair types for the given repair group.
        /// </summary>
        /// <param name="repairGroup">Repair Group name.</param>
        /// <returns>Dictionary, Key=Repair Type, Value=Number of available repairs of that type.</returns>
        public Dictionary<Mbist.RepairType, int> AvailableRepairTypes(string repairGroup)
        {
            Dictionary<Mbist.RepairType, int> availRepairs = new Dictionary<Mbist.RepairType, int>
            {
                { Mbist.RepairType.ROW, 0 },
                { Mbist.RepairType.COL, 0 },
                { Mbist.RepairType.WORD, 0 },
            };
            foreach (var repair in this.RasterConfig.RepairGroups[repairGroup].Repairs)
            {
                if (repair.Status == Mbist.RepairStatus.AVAIL)
                {
                    availRepairs[repair.Type]++;
                }
            }

            return availRepairs;
        }

        /// <summary>
        /// Perform the Must Repair Algorithm.
        /// </summary>
        /// <returns>false if repair was not possible.</returns>
        public bool CheckMustRepair()
        {
            foreach (var group in this.FailDatabase.GetRepairGroups())
            {
                var repairType = this.RasterConfig.GetRepairType(group);
                Prime.Services.ConsoleService.PrintDebug($"<debug> CheckMustRepair: Group=[{group}] RepairType=[{repairType}]");

                var availableRepairs = this.AvailableRepairTypes(group);
                if (repairType != Mbist.RepairType.BOTH)
                {
                    if (availableRepairs[repairType] >= this.FailDatabase.CountNeededRepairs(group, repairType))
                    {
                        Prime.Services.ConsoleService.PrintDebug($"<debug> CheckMustRepair: Repairing with {repairType}. Available=[{availableRepairs[repairType]}] Needed=[{this.FailDatabase.CountNeededRepairs(group, repairType)}]");
                        if (!this.FailDatabase.MarkForRepair(group, repairType))
                        {
                            Prime.Services.ConsoleService.PrintError($"CheckMustRepair: Failed for {repairType} repairs");
                            return false;
                        }
                    }
                    else if (repairType == Mbist.RepairType.COL)
                    {
                        // FIXME: this matches the EmbPython implementation but why?
                        Prime.Services.ConsoleService.PrintError($"CheckMustRepair: Not enough {repairType} repairs.  Available=[{availableRepairs[repairType]}] Needed=[{this.FailDatabase.CountNeededRepairs(group, repairType)}]");
                        return false;
                    }
                }
                else
                {
                    // this group supports both rows and columns, need to pick the correct ones.
                    Prime.Services.ConsoleService.PrintDebug($"<debug> CheckMustRepair: AvailableRows=[{availableRepairs[Mbist.RepairType.ROW]}] NeededRows=[{this.FailDatabase.CountNeededRepairs(group, Mbist.RepairType.ROW)}]");
                    Prime.Services.ConsoleService.PrintDebug($"<debug> CheckMustRepair: AvailableCols=[{availableRepairs[Mbist.RepairType.COL]}] NeededCols=[{this.FailDatabase.CountNeededRepairs(group, Mbist.RepairType.COL)}]");

                    // check each row defect to see if it can be repaired with a col
                    foreach (var row in this.FailDatabase.FailAddresses(group, Mbist.RepairType.ROW))
                    {
                        if (availableRepairs[Mbist.RepairType.COL] < this.FailDatabase.CountNeededRepairs(group, Mbist.RepairType.ROW, row))
                        {
                            Prime.Services.ConsoleService.PrintDebug($"<debug> CheckMustRepair: Row=[{row}] Defects=[{this.FailDatabase.CountNeededRepairs(group, Mbist.RepairType.ROW, row)}]");

                            // need a row repair for this one.
                            if (availableRepairs[Mbist.RepairType.ROW] == 0)
                            {
                                Prime.Services.ConsoleService.PrintError($"CheckMustRepair: Ran out of ROW repairs on Group=[{group}] Row=[{row}].");
                                return false;
                            }
                            else
                            {
                                Prime.Services.ConsoleService.PrintDebug($"\t<debug> repairing row=[{row}] with ROW repair.");
                                if (!this.FailDatabase.MarkForRepair(group, Mbist.RepairType.ROW, row))
                                {
                                    Prime.Services.ConsoleService.PrintError($"CheckMustRepair: Failed for {repairType} repairs (ROW type)");
                                    return false;
                                }

                                availableRepairs[Mbist.RepairType.ROW] -= 1;
                            }
                        }
                    }

                    // check each col defect to see if it can be repaired with a row
                    foreach (var col in this.FailDatabase.FailAddresses(group, Mbist.RepairType.COL))
                    {
                        Prime.Services.ConsoleService.PrintDebug($"<debug> CheckMustRepair: Col=[{col}] Defects=[{this.FailDatabase.CountNeededRepairs(group, Mbist.RepairType.COL, col)}]");
                        if (availableRepairs[Mbist.RepairType.ROW] <= this.FailDatabase.CountNeededRepairs(group, Mbist.RepairType.COL, col))
                        {
                            // need a col repair for this one.
                            if (availableRepairs[Mbist.RepairType.COL] == 0)
                            {
                                Prime.Services.ConsoleService.PrintError($"CheckMustRepair: Ran out of COL repairs on Group=[{group}] Col=[{col}]");
                                return false;
                            }
                            else
                            {
                                Prime.Services.ConsoleService.PrintDebug($"\t<debug> repairing col=[{col}] with COL repair.");
                                if (!this.FailDatabase.MarkForRepair(group, Mbist.RepairType.COL, col))
                                {
                                    Prime.Services.ConsoleService.PrintError($"CheckMustRepair: Failed for {repairType} repairs (COL type)");
                                    return false;
                                }

                                availableRepairs[Mbist.RepairType.COL] -= 1;
                            }
                        }
                    }
                } // else if (repairType == Mbist.RepairType.BOTH)
            } // foreach (var group in this.FailDatabase.GetRepairGroups())

            return true;
        }

        /// <summary>
        /// Main wrapper function for performing repair.
        /// </summary>
        /// <returns>true if repair was successful.</returns>
        public bool PerformRepair()
        {
            // If its an MAF or status failure, then can't repair
            if (this.MafFlag || this.ControllerStatusFailure)
            {
                return false;
            }

            var currentRepairFuses = this.GetRepairGlobalValues(this.FailDatabase.GetRepairGroups());
            if (!this.UpdateRepairStatusFromFuses(currentRepairFuses))
            {
                // no more available fuses, can't repair anything.
                Prime.Services.ConsoleService.PrintError($"PerformRepair: No available fuses, can't repair anything.");
                return false;
            }

            if (!this.CheckMustRepair())
            {
                return false;
            }

            if (this.FailDatabase.RepairsNeeded())
            {
                Prime.Services.ConsoleService.PrintDebug($"<debug> PerformRepair: More repairs needed after CheckMustRepair.");

                foreach (var group in this.FailDatabase.GetRepairGroups())
                {
                    var availableRepairs = this.AvailableRepairTypes(group);
                    /* foreach (var repairType in availableRepairs.Keys) -- this doesn't match the python ordering. */
                    foreach (var repairType in Mbist.RepairPriority)
                    {
                        if (!availableRepairs.ContainsKey(repairType))
                        {
                            continue;
                        }

                        if (this.FailDatabase.RepairsNeeded(group, repairType))
                        {
                            Prime.Services.ConsoleService.PrintDebug($"<debug> PerformRepair: Group=[{group}] RepairType=[{repairType}].");

                            if (availableRepairs[repairType] >= this.FailDatabase.CountNeededRepairs(group, repairType))
                            {
                                if (!this.FailDatabase.MarkForRepair(group, repairType))
                                {
                                    Prime.Services.ConsoleService.PrintError($"PerformRepair: Failed for {repairType} repairs");
                                    return false;
                                }
                            }
                            else if (repairType == Mbist.RepairType.COL)
                            {
                                // FIXME: this matches the EmbPython implementation but why?
                                Prime.Services.ConsoleService.PrintError($"PerformRepair: Not enough {repairType} repairs.  Available=[{availableRepairs[repairType]}] Needed=[{this.FailDatabase.CountNeededRepairs(group, repairType)}]");
                                return false;
                            }
                        } // end if (this.FailDatabase.CountNeededRepairs(group, repairType) > 0)
                    } // end foreach (var repairType in availableRepairs.Keys)

                    if (this.FailDatabase.RepairsNeeded(group))
                    {
                        Prime.Services.ConsoleService.PrintDebug($"<debug> PerformRepair: Failed to repair everything  group=[{group}].");
                        return false;
                    }
                } // end foreach (var group in this.FailDatabase.GetRepairGroups())
            }

            Prime.Services.ConsoleService.PrintDebug($"<debug> PerformRepair: Completed all repairs.");
            this.SetRepairGlobalValues(currentRepairFuses.Keys.ToList());
            this.Repaired = true;
            return true;
        }

        /// <summary>
        /// Updates the HRY data for the given controllers.
        /// </summary>
        /// <param name="plist">PList object to get the controllers from.</param>
        /// <param name="hrystring">Current HRY string.</param>
        /// <returns>A New/Updated HRY string.</returns>
        public string UpdateHRY(MbistRasterInput.PList plist, string hrystring)
        {
            if (plist == null)
            {
                throw new ArgumentNullException("UpdateHRY:HRY PList Object to UpdateHRY is null.", "plist");
            }

            if (hrystring == null)
            {
                throw new ArgumentNullException("UpdateHRY:HRY string list to UpdateHRY is null.", "hrystring");
            }

            var hryList = hrystring.ToCharArray();
            foreach (var captureGroup in plist.CaptureGroups)
            {
                if (!this.RasterConfig.CaptureDecoders.ContainsKey(captureGroup))
                {
                    throw new ArgumentException($"UpdateHRY: No definition for CaptureGroup=[{captureGroup}] found.", "plist");
                }

                foreach (var controller in this.RasterConfig.CaptureDecoders[captureGroup].Keys)
                {
                    var decoder = this.RasterConfig.CaptureDecoders[captureGroup][controller];
                    Prime.Services.ConsoleService.PrintDebug($"<UpdateHRY> Checking Controller=[{controller}] MAF=[{this.MafFlag}] StatusFail=[{this.ControllerStatusFailure}]");

                    var hryMap = decoder.GetHryIndexByMemory();
                    foreach (var pair in hryMap)
                    {
                        var mem = pair.Key;
                        var hryIndex = pair.Value;
                        if (hryList.Length <= hryIndex)
                        {
                            throw new ArgumentException($"UpdateHRY:HRY String is too short. Length=[{hryList.Length}] but Controller=[{controller}] MemID=[{mem}] HRYIndex=[{hryIndex}].", "hrystring");
                        }

                        var currentHRY = hryList[hryIndex];
                        var failRows = this.FailDatabase.FailingRows(controller, mem);
                        var unrepairable = this.RepairMode & !this.Repaired;

                        if (this.MafFlag)
                        {
                            hryList[hryIndex] = HryTable["mass_failure"];
                        }
                        else if (this.ControllerStatusFailure)
                        {
                            hryList[hryIndex] = HryTable["cont_fail"];
                        }
                        else if (currentHRY == HryTable["pass"] && failRows > 0)
                        {
                            hryList[hryIndex] = HryTable["inconsist_rast"];
                        }
                        else if (currentHRY == HryTable["fail"] && failRows == 0)
                        {
                            hryList[hryIndex] = HryTable["no_rasterdet"];
                        }
                        else if (unrepairable && currentHRY == HryTable["fail"] && failRows > 0)
                        {
                            if (failRows <= 4)
                            {
                                hryList[hryIndex] = HryTable["rep_ifnoshare"];
                            }
                            else if (failRows >= 5 && failRows <= 8)
                            {
                                hryList[hryIndex] = HryTable["unrep_noshare5to8"];
                            }
                            else if (failRows >= 9 && failRows <= 12)
                            {
                                hryList[hryIndex] = HryTable["unrep_noshare9to12"];
                            }
                            else if (failRows > 12)
                            {
                                hryList[hryIndex] = HryTable["mass_failure"];
                            }
                        }
                        else if (this.Repaired && currentHRY == HryTable["fail"])
                        {
                            hryList[hryIndex] = HryTable["repairable"];
                        }

                        Prime.Services.ConsoleService.PrintDebug($"<UpdateHRY> Mem=[{mem}] HRYIndex=[{hryIndex}] CurrentHRY=[{currentHRY}] Fails=[{failRows}] NewHRY=[{hryList[hryIndex]}]");
                    }
                }
            }

            return new string(hryList);
        }

        /// <summary>
        /// Main function for Decoding Raster capture memory.
        /// </summary>
        /// <param name="plist">JSON input struct for the Plist.</param>
        /// <param name="ctvData">Serialized capture data.</param>
        /// <returns>The number of captures successfully processed.</returns>
        public int DecodeAllCaptures(MbistRasterInput.PList plist, string ctvData)
        {
            var cachedSize = new Dictionary<string, int>();
            int captureCount = 0;
            int ctvStart = 0;

            // Figure out how many bits are required for one capture step of this PList.
            // Also validate that all the CaptureGroups in this plist are defined.
            // And builds the size cache for each group.
            int sizeForOneFullCaptureStep = 0;
            foreach (var captureGroup in plist.CaptureGroups)
            {
                if (!this.RasterConfig.CaptureDecoders.ContainsKey(captureGroup))
                {
                    throw new ArgumentException($"DecodeAllCaptures: No definition for CaptureGroup=[{captureGroup}] found.", "plist");
                }

                cachedSize[captureGroup] = this.RasterConfig.GetSize(this.RasterConfig.CaptureDecoders[captureGroup]);
                sizeForOneFullCaptureStep += cachedSize[captureGroup];
            }

            // calculate the repeat count for each capture group.
            if ((ctvData.Length % sizeForOneFullCaptureStep) != 0)
            {
                throw new ArgumentException($"DecodeAllCaptures: Number of CTV bits=[{ctvData.Length}] is not a whole number of captures. 1 CaptureStep=[{sizeForOneFullCaptureStep}] Bits.", "ctvData");
            }

            int captureRptCount = ctvData.Length / sizeForOneFullCaptureStep;

            foreach (var captureGroup in plist.CaptureGroups)
            {
                var captSize = cachedSize[captureGroup];
                var decoders = this.RasterConfig.CaptureDecoders[captureGroup];
                for (var i = 0; i < captureRptCount; i++)
                {
                    if (ctvData.Length < (ctvStart + captSize))
                    {
                        Prime.Services.ConsoleService.PrintError($"CTVData (len={ctvData.Length}) does not contain enough data for iteration={captureCount} Controller=[{captureGroup}]. Expecting {ctvStart + captSize} bits.");
                        return captureCount; // what error/return? FIXME.
                    }

                    foreach (var controller in decoders.Keys)
                    {
                        Prime.Services.ConsoleService.PrintDebug($"Now Decoding CaptureGroup=[{captureGroup}] Controller=[{controller}] Iteration=[{i}]");
                        var decoder = decoders[controller];
                        var captureData = ctvData.Substring(ctvStart, captSize);

                        this.DecodeSingleControllerCapture(controller, i, decoder, captureData, offset: ctvStart);
                        captureCount++;
                    }

                    ctvStart += captSize;
                }
            }

            return captureCount;
        }

        /// <summary>
        /// Decodes a single captures worth of raster data.
        /// </summary>
        /// <param name="controllerName">Name of the controller that the data belongs to.</param>
        /// <param name="captureGroupNum">Globally, which capture this is (for FAFI logging).</param>
        /// <param name="decoder">RasterConfig JSON decoder for this data.</param>
        /// <param name="ctvData">Capture data.</param>
        /// <param name="offset">Location of bit 0 within the full ctv string (only used for FAFI printing).</param>
        public void DecodeSingleControllerCapture(string controllerName, int captureGroupNum, MbistRasterInput.BitRanges decoder, string ctvData, int offset = 0)
        {
            var statusBin = Mbist.GetSubData(decoder.Status, ctvData);
            if (statusBin == "100" || statusBin == "010" || statusBin == "110")
            {
                this.ControllerStatusFailure = true;  // FIXME - continue or exit out?
            }
            else if (statusBin == "001" && !this.SAR_RUN_ALL_LOOP_OVERRIDE)
            {
                Prime.Services.ConsoleService.PrintDebug($"processFailures::NO MORE FAILURES CAPTURED! Status==001 & SAR_RUN_ALL_LOOP_OVERRIDE==False");
                return; // FIXME - set status??
            }

            var stepBin = Mbist.GetSubData(decoder.Step, ctvData);
            if (stepBin == string.Empty)
            {
                stepBin = "0";
            }

            var errorCntBin = Mbist.GetSubData(decoder.ErrorCnt, ctvData);
            var addrXBin = Mbist.GetSubData(decoder.AddrX, ctvData);
            var addrYBin = Mbist.GetSubData(decoder.AddrY, ctvData);
            var addrZBin = Mbist.GetSubData(decoder.AddrZ, ctvData);
            var loopCntBin = Mbist.GetSubData(decoder.LoopCounter, ctvData);
            var instrBin = Mbist.GetSubData(decoder.Instruction, ctvData);
            var algoBin = Mbist.GetSubData(decoder.Algorithm, ctvData);

            var addrX = Bin2dec(addrXBin);
            var addrY = (decoder.AddrY == MbistRasterInput.BitRanges.NA) ? 0 : Bin2dec(addrYBin);
            var addrZ = (decoder.AddrZ == MbistRasterInput.BitRanges.NA) ? 0 : Bin2dec(addrZBin);

            if (this.FAFIMode)
            {
                Prime.Services.ConsoleService.PrintDebug("\n#########################################################################################################");
                Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::{0,14} {1,-20}", "CONTROLLER", controllerName));
                Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::{0,14} {1,-20}", "EXECUTION", captureGroupNum));
                Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::{0,14} {1,-20}", "STEP", stepBin));
                Prime.Services.ConsoleService.PrintDebug("=========================================================================================================");
                Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::{0,-8} {1,-13} {2,-50} {3,-20}", "FIELD", "CTVs(LSB-MSB)", "VALUE", "BITS(MSB-LSB)"));
                Prime.Services.ConsoleService.PrintDebug("=========================================================================================================");
                Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::{0,-10} {1,-10} {2,-50} {3,-20}", "status", Mbist.AdjustRange(decoder.Status, offset), Bin2dec(statusBin), statusBin));
                if (decoder.ErrorCnt != MbistRasterInput.BitRanges.NA)
                {
                    Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::{0,-10} {1,-10} {2,-50} {3,-20}", "errorcnt", Mbist.AdjustRange(decoder.ErrorCnt, offset), Bin2dec(errorCntBin), errorCntBin));
                }

                Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::{0,-10} {1,-10} {2,-50} {3,-20}", "addr_x", Mbist.AdjustRange(decoder.AddrX, offset), addrX, addrXBin));
                if (decoder.AddrY != MbistRasterInput.BitRanges.NA)
                {
                    Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::{0,-10} {1,-10} {2,-50} {3,-20}", "addr_y", Mbist.AdjustRange(decoder.AddrY, offset), addrY, addrYBin));
                }

                if (decoder.AddrZ != MbistRasterInput.BitRanges.NA)
                {
                    Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::{0,-10} {1,-10} {2,-50} {3,-20}", "addr_z", Mbist.AdjustRange(decoder.AddrZ, offset), addrZ, addrZBin));
                }

                Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::{0,-10} {1,-10} {2,-50} {3,-20}", "loopCnt", Mbist.AdjustRange(decoder.LoopCounter, offset), Bin2dec(loopCntBin), loopCntBin));
                Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::{0,-10} {1,-10} {2,-50} {3,-20}", "inst", Mbist.AdjustRange(decoder.Instruction, offset), Bin2dec(instrBin), instrBin));
                Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::{0,-10} {1,-10} {2,-50} {3,-20}", "algo", Mbist.AdjustRange(decoder.Algorithm, offset), Bin2hex(algoBin, prefix: true), algoBin));
            } // end if (this.FAFIMode)

            foreach (var mem in decoder.GoIDs)
            {
                var memID = mem.MemoryId;
                var memVal = Mbist.GetSubData(mem.GoIDBits, ctvData);

                var numGoid = memVal.Length;
                /* var numFails = memVal.Count(f => f == '1');  // FIXME: performance */
                /* This for loop is about 2x as fast as the .Count line above... */
                var numFails = 0;
                for (var iobit = 0; iobit < memVal.Length; iobit++)
                {
                    if (memVal[iobit] == '1')
                    {
                        numFails++;
                    }
                }

                if (this.FAFIMode)
                {
                    var hexVal = Bin2hex(memVal, reverse: true, prefix: true);
                    var memValMsbFirst = Mbist.StringReverse(memVal);
                    Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::MEM{0,-7} {1,-10} {2,-50} {3}'b{4,-20}", memID, Mbist.AdjustRange(mem.GoIDBits, offset), hexVal, numGoid, memValMsbFirst));
                }

                if (numGoid / 4 <= numFails && addrX == 0 && addrY == 0 && addrZ == 0)
                { // isn't this redundant?? FIXME
                    this.MafFlag = true;
                }

                if (numFails > 0 && numGoid / 4 >= numFails)
                {
                    for (int io = numGoid - 1; io >= 0; io--)
                    {
                        if (memVal[io] == '1')
                        {
                            Prime.Services.ConsoleService.PrintDebug($"<debug> Adding Defect {controllerName} {Bin2dec(stepBin)} {memID} {addrZ} {addrX} {addrY} {io}");
                            this.FailDatabase.Add(controllerName, Bin2dec(stepBin), memID, addrZ, addrX, addrY, io);
                        }
                    }
                }
                else if (numFails > 0 && numGoid / 4 < numFails)
                {
                    Prime.Services.ConsoleService.PrintDebug($"<debug> Setting MAF flag - NumFails=[{numFails}] NumGoID=[{numGoid}].");
                    this.MafFlag = true;
                }
            } // end foreach (var memPair in decoder.GoIDs)

            if (this.FAFIMode)
            {
                var loopCnt = Bin2dec(loopCntBin);
                var instCnt = Bin2dec(instrBin);
                var stuckAt = "N/A";
                if ((loopCnt == 1 || loopCnt == 3) && instCnt == 5)
                {
                    stuckAt = "1";
                }
                else if ((loopCnt == 1 || loopCnt == 3) && instCnt == 4)
                {
                    stuckAt = "0";
                }
                else if ((loopCnt == 0 || loopCnt == 2) && instCnt == 5)
                {
                    stuckAt = "0";
                }
                else if ((loopCnt == 0 || loopCnt == 2) && instCnt == 4)
                {
                    stuckAt = "1";
                }

                Prime.Services.ConsoleService.PrintDebug(string.Format("FAFI::{0,-10} {1,-10}", "Stuckat", stuckAt));
                Prime.Services.ConsoleService.PrintDebug("#########################################################################################################");
            }
        } // end of DecodeSingleControllerCapture
    }
}
