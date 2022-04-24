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
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Prime;
    using Prime.ConsoleService;
    using Prime.SharedStorageService;

    /// <summary>hry Class.</summary>
    public class Hry
    {
        /// <summary> The characters that will be provided for the HRY string have been mapped with priority values.</summary>
        public static readonly Dictionary<ResultNameChar, int> ResultsReferenceTable = new Dictionary<ResultNameChar, int>()
        {
            { ResultNameChar.Untested, 0 }, // per memory
            { ResultNameChar.Pass, 1 }, // per memory
            { ResultNameChar.Fail, 2 }, // per memory
            { ResultNameChar.Repairable, 3 }, // per memory
            { ResultNameChar.Unrepairable, 4 }, // per memory
            { ResultNameChar.Pass_retest, 5 }, // Post Repair mode ONLY, per memory : y - > 1 = passRetest
            { ResultNameChar.Fail_retest, 6 }, // Post Repair mode ONLY, per memory : y - > 0 = failRetest
            { ResultNameChar.Inconsist_pst_fail, 7 }, // Post Repair mode ONLY, per memory : 1 -> 0 = inconsistent
            { ResultNameChar.Inconsist_ps_fg, 8 },   // applies to all memories for the execution, requires mem level
            { ResultNameChar.Inconsist_fs_pg, 9 },   // applies to all memories for the execution, requires mem level
            { ResultNameChar.Cont_fail, 14 },   // applies to all memories for all executions
            { ResultNameChar.Pattern_misprogram, 13 },   // applies to all memories for all executions
            { ResultNameChar.Pattern_bad_wait, 11 },   // applies to all memories for all executions
            { ResultNameChar.Fail_OnOtherPins, 12 },   // applies to all memories for all executions

            // Raster/Repair specific
            { ResultNameChar.Rep_ifnoshare, 14 },
            { ResultNameChar.Unrep_noshare5to8, 14 },
            { ResultNameChar.Unrep_noshare9to12, 14 },
            { ResultNameChar.Mass_failure, 14 },
            { ResultNameChar.No_rasterdet, 14 },
            { ResultNameChar.Inconsist_rast, 14 },
            { ResultNameChar.Rep_notapplied, 14 },
        };

        /// <summary>Enum of allowed types for controller execution. </summary>
        public enum AllowedTypes
        {
            /// <summary>BIRA mode.</summary>
            RABITS,

            /// <summary>HRY mode.</summary>
            GOID,

            /// <summary>BISR Chain data.</summary>
            BISR,

            /// <summary>VFDM Column only repair.</summary>
            VFDM,
        }

        /// <summary>name to char enum.</summary>
        public enum ResultNameChar
        {
            /// <summary>Untested/ char represent U in HRY.</summary>
            Untested = 'U',

            /// <summary>Pass/ char represent 1 in HRY.</summary>
            Pass = '1',

            /// <summary>Fail/ char represent 0 in HRY.</summary>
            Fail = '0',

            /// <summary>Repairable/ char represent Y in HRY.</summary>
            Repairable = 'Y',

            /// <summary>UnRepairable/ char represent N in HRY.</summary>
            Unrepairable = 'N',

            /// <summary>Pass_retest/ char represent P in HRY.</summary>
            Pass_retest = 'P',

            /// <summary>Fail_retest/ char represent F in HRY.</summary>
            Fail_retest = 'F',

            /// <summary>Inconsist_pst_fail/ char represent 7 in HRY.</summary>
            Inconsist_pst_fail = '7',

            /// <summary>inconsist_ps_fg/ char represent 6 in HRY.</summary>
            Inconsist_ps_fg = '6',

            /// <summary>inconsist_ps_fg/ char represent 5 in HRY.</summary>
            Inconsist_fs_pg = '5',

            /// <summary>Cont_fail/ char represent 8 in HRY.</summary>
            Cont_fail = '8',

            /// <summary>Pattern_misprogram/ char represent M in HRY.</summary>
            Pattern_misprogram = 'M',

            /// <summary>pattern_bad_wait/ char represent W in HRY.</summary>
            Pattern_bad_wait = 'W',

            /// <summary>pattern_bad_wait/ char represent W in HRY.</summary>
            Fail_OnOtherPins = 'B',

            /// Raster only variables.
            /// <summary>Rep_ifnoshare/ char represent D in HRY.</summary>
            Rep_ifnoshare = 'D',

            /// <summary>Unrep_noshare5to8/ char represent E in HRY.</summary>
            Unrep_noshare5to8 = 'E',

            /// <summary>unrep_noshare9to12/ char represent G in HRY.</summary>
            Unrep_noshare9to12 = 'G',

            /// <summary>Mass_failure/ char represent H in HRY.</summary>
            Mass_failure = 'H',

            /// <summary>No_rasterdet/ char represent J in HRY.</summary>
            No_rasterdet = 'J',

            /// <summary>Inconsist_rast/ char represent K in HRY.</summary>
            Inconsist_rast = 'K',

            /// <summary>Rep_notapplied/ char represent L in HRY.</summary>
            Rep_notapplied = 'L',
        }

        /// <summary>Gets or sets Debug Printouts for each thread for debug.</summary>
        public ConcurrentDictionary<long, List<string>> DebugCaptures { get; set; }

        /// <summary>Gets or sets Debug Printouts for each thread for debug.</summary>
        public MbistVminTC.PrimeLogLevel Loglevel { get; set; }

        /// <summary>Gets or sets current Storage of your HRY string.</summary>
        public ConcurrentDictionary<int, char> CurrentHryString { get; set; }

        /// <summary>Gets or sets a value indicating whether scoreboard functions should be enabled.</summary>
        public bool ScoreboardEnabled { get; set; } = false;

        /// <summary>Gets or sets Hrylookup from config.</summary>
        public HryJsonParser HryLookupTable { get; set; }

        /// <summary>Gets or sets name for hrystring data to print to shared storage.</summary>
        public string HrystringNameSS { get; set; }

        /// <summary>Gets or sets name for hrystring data to print to shared storage.</summary>
        public string HrystringNameITUFF { get; set; }

        /// <summary>Gets or sets name for hrystring data to print to shared storage.</summary>
        public ConcurrentDictionary<string, HashSet<int>> ScoreboardFails { get; set; } = new ConcurrentDictionary<string, HashSet<int>>();

        /// <summary>Gets or sets for concurrentfailures for each pattern running.</summary>
        public ConcurrentDictionary<string, ConcurrentDictionary<string, ResultNameChar>> ConcurrentFails { get; set; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, ResultNameChar>>();

        /// <summary>Gets or sets enabling concurrentfailure captures or not.</summary>
        public MbistVminTC.EnableStates AdvanceDebug { get; set; }

        /// <summary>Gets or sets a value indicating whether gets or sets threading enabled.</summary>
        public int Threads { get; set; }

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        public IConsoleService Console { get; set; } = null;

        /// <summary>Will Create RecoveryLookup and add to shared storage for later use if not exist.</summary>
        /// <param name = "forceConfigFileParseState" > Force the configs to be repulled.</param>
        /// <param name = "hryconfigfile" > Name of hry configfile.</param>
        /// <param name = "patlist" > Name of plist under execution.</param>
        public void HryfileParse(MbistVminTC.EnableStates forceConfigFileParseState, string hryconfigfile, string patlist)
        {
            var hryLookupTable = new HryJsonParser();

            // Creates RecoveryReference if recovery mode enabled
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Hry Check.");
            if (forceConfigFileParseState == MbistVminTC.EnableStates.Enabled)
            {
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Force pull recovery JSON from HRY {hryconfigfile}");
                hryLookupTable = this.HryJsonParser(Prime.Services.FileService.GetFile(hryconfigfile));
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Building Indexs to simplify run time");
                hryLookupTable.RemoveNonMBistPats();

                foreach (var plist in hryLookupTable.Plists.Keys)
                {
                    Prime.Services.SharedStorageService.InsertRowAtTable(plist, hryLookupTable.MakePerPlistLookup(plist, false), Context.LOT);
                }

                this.HryLookupTable = hryLookupTable.MakePerPlistLookup(patlist, true);
            }
            else
            {
                try
                {
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Pulling recovery JSON from Shared storage");
                    this.HryLookupTable = (HryJsonParser)Prime.Services.SharedStorageService.GetRowFromTable(patlist, typeof(HryJsonParser), Context.LOT);
                    this.HryLookupTable.BuildIndex();
                    this.HryLookupTable.BuildBisr(patlist);
                }
                catch
                {
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Failed to pull recovery JSON from Shared storage");
                    hryLookupTable = this.HryJsonParser(Prime.Services.FileService.GetFile(hryconfigfile));
                    hryLookupTable.RemoveNonMBistPats();

                    foreach (var plist in hryLookupTable.Plists.Keys)
                    {
                        Prime.Services.SharedStorageService.InsertRowAtTable(plist, hryLookupTable.MakePerPlistLookup(plist, false), Context.LOT);
                    }

                    this.HryLookupTable = hryLookupTable.MakePerPlistLookup(patlist, true);
                }
            }
        }

        /// <summary>Will be called in the.</summary>
        /// <param name = "patterns" > Failing patterns list.</param>
        /// <param name = "patlist" > Patlist running.</param>
        /// <returns>return limiting failing tuple.</returns>
        public virtual List<string> FindlimitingPattern(List<string> patterns, string patlist)
        {
            var patternordered = new List<string>();
            foreach (var pattern in this.HryLookupTable.Plists[patlist])
            {
                if (patterns.Contains(pattern))
                {
                    patternordered.Add(pattern);
                }
            }

            return patternordered;
        }

        /// <summary>Will be called in the.</summary>
        /// <param name = "jsonfile" > Name of the JSON file to lao.</param>
        /// <returns>bool for whether file was found or errored.</returns>
        public virtual HryJsonParser HryJsonParser(string jsonfile)
        {
            if (string.IsNullOrEmpty(jsonfile))
            {
                Prime.Services.ConsoleService.PrintError($"[{MethodBase.GetCurrentMethod().Name}]] Error, prime GetFile({jsonfile}) returned empty string, file probably doesn't exist.");
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<HryJsonParser>(File.ReadAllText(jsonfile));
            }
            catch (JsonException ex)
            {
                Prime.Services.ConsoleService.PrintError($"[{MethodBase.GetCurrentMethod().Name}]] Error, failed to load hry file=[{jsonfile}]. Exception=[{ex.Message}].");
                return null;
            }
        }

        /// <summary> Parses the provided this.CtvString for the specific controller result bits. </summary>
        /// <param name="plist"> Plist to parse.</param>
        /// <param name="ctvcapture">list of ctvs captured for the execution.</param>
        /// <param name="vfdmlookup"> VFDM lookup from.</param>
        /// <param name="startpattern"> start pattern.</param>
        /// <param name="dtsperpatternmode"> PerPatternMode capture .</param>
        /// <param name="dtsctvperpattern"> Number of CTVs per DTS pattern.</param>
        /// <returns>the ctvs used for DTS. </returns>
        public virtual (string, Dictionary<string, string>, bool) RunAllCTVPerPlist(string plist, string ctvcapture, Virtualfuse vfdmlookup, string startpattern = "",  bool dtsperpatternmode = false, int dtsctvperpattern = 0)
        {
            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Started ---- ");
            var patctvs = new Dictionary<long, Tuple<string, string>>();
            this.CurrentHryString = new ConcurrentDictionary<int, char>();
            this.DebugCaptures = new ConcurrentDictionary<long, List<string>>();
            var dtsctvs = string.Empty;
            var bisrctvs = new Dictionary<string, string>();

            long idx = 0;
            int location = 0;
            var startpatternidx = this.HryLookupTable.Plists[plist].IndexOf(startpattern);
            var breakout = false;

            foreach (string patternname in this.HryLookupTable.Plists[plist])
            {
                string subctvstring;
                if (idx >= startpatternidx)
                {
                    try
                    {
                        subctvstring = ctvcapture.Substring(location, this.HryLookupTable.Patterns[patternname].CaptureCount);
                    }
                    catch
                    {
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]]: {location}:{ctvcapture.Length}");
                        subctvstring = "0";
                        if (location != ctvcapture.Length)
                        {
                            breakout = true;
                        }

                        break;
                    }

                    location += this.HryLookupTable.Patterns[patternname].CaptureCount;
                }
                else
                {
                    subctvstring = new string('0', this.HryLookupTable.Patterns[patternname].CaptureCount);
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]] Pattern is being padded with 0s {patternname} @ index {idx}: {subctvstring.Length}");
                }

                if (!this.HryLookupTable.Bisrpats.Contains(patternname))
                {
                    if (this.Threads > 1)
                    {
                        patctvs.Add(idx, new Tuple<string, string>(patternname, subctvstring));
                    }
                }
                else
                {
                    bisrctvs.Add(patternname, subctvstring);
                }

                // this.Console?.PrintDebug($"[RunAllCTVPerPlist] CTV capture string for {patternname} = [{subctvstring}]");
                this.DebugCaptures.AddOrUpdate(idx, new List<string>(), (key, existingVal) => new List<string>());

                if (this.Loglevel == Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG)
                {
                    this.DebugCaptures[idx].Add($"[RunAllCTVPerPlist] CTV capture string for {patternname} = [{subctvstring}]");
                }

                if (!this.HryLookupTable.Bisrpats.Contains(patternname))
                {
                    this.ProcessPatternCTVs(patternname, subctvstring, vfdmlookup, idx);
                }

                // Removing per PATTERN DTS code since the same pattern can't be repeated to capture the data.
/*                if (dtsctvperpattern > 0 && dtsperpatternmode == true)
            {
                dtsctvs += ctvcapture.Substring(location, dtsctvperpattern);
                location += dtsctvperpattern;
            }*/

                idx += 1;
            }

            if (dtsctvperpattern > 0 && dtsperpatternmode == false)
            {
                dtsctvs += ctvcapture.Substring(location, dtsctvperpattern);
            }

            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Stopped ---- ");
            return (dtsctvs, bisrctvs, breakout);
        }

        /// <summary> Parses the provided this.CtvString for the specific controller result bits. </summary>
        /// <param name="pattern">PatternBlock being debugged.</param>
        /// <param name="ctvcapture">list of ctvs captured for the execution.</param>
        /// <param name="vfdmlookup"> VFDM lookup from.</param>
        /// <param name="threadnum"> Thread number to uniquify results.</param>
        public virtual void ProcessPatternCTVs(string pattern, string ctvcapture, Virtualfuse vfdmlookup, long threadnum)
        {
            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- [{pattern}] -> [{ctvcapture}] ----Start ");
            var failingindex = new HashSet<int>();
            if (this.Contains1(ctvcapture))
            {
                foreach (KeyValuePair<string, HryJsonParser.PatternBlocks.ControllersBlock> controller in this.HryLookupTable.Patterns[pattern].Controllers)
                {
                    foreach (HryJsonParser.PatternBlocks.ControllersBlock.ExecutionBlocks execution in controller.Value.Executions)
                    {
                        var advancepattern = pattern;
                        if (this.AdvanceDebug == MbistVminTC.EnableStates.Disabled)
                        {
                            advancepattern = string.Empty;
                        }

                        // this.Console?.PrintDebug("[ProcessPatternCTVs]-------------------------------------------------------------------------------------------------------");
                        // this.Console?.PrintDebug($"[ProcessPatternCTVs] Running Controller: {controller.Key} Execution: {idx}");
                        var controllerfailure = this.ExtractExecution(execution, controller.Value.Type, ctvcapture, vfdmlookup, threadnum, advancepattern);

                        if (controllerfailure.Item1 == true)
                        {
                            this.UpdateAllMems(ResultNameChar.Cont_fail, controller.Value.ControllerIndexs, pattern);
                            failingindex.UnionWith(new HashSet<int>(controller.Value.ControllerIndexs.Values));
                            break;
                        }
                        else
                        {
                            failingindex.UnionWith(controllerfailure.Item2);
                        }
                    }
                }

                if (this.ScoreboardEnabled == true)
                {
                    this.ScoreboardFails.AddOrUpdate(pattern, failingindex, (key, existingVal) => this.Hashunion(existingVal, failingindex));
                }
            }
            else
            {
                this.UpdateAllMems(ResultNameChar.Pass, this.HryLookupTable.Indexperpattern[pattern]);
            }

            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Started ---- ");
        }

        /// <summary> Contains1 function Since it should be faster then contains. </summary>
        /// <param name="existing"> Existing hash set.</param>
        /// <param name="newvalues"> Incoming values.</param>
        /// <returns> True if value is contained.</returns>
        public HashSet<int> Hashunion(HashSet<int> existing, HashSet<int> newvalues)
        {
            foreach (var intger in newvalues)
            {
                existing.Add(intger);
            }

            return existing;
        }

        /// <summary> Contains1 function Since it should be faster then contains. </summary>
        /// <param name="searchstring"> String to search in.</param>
        /// <returns> True if value is contained.</returns>
        public virtual bool Contains1(string searchstring)
        {
            HashSet<char> compress = new HashSet<char>();
            foreach (var character in searchstring)
            {
                compress.Add(character);
            }

            if (compress.Count == 1 && !compress.Contains('1'))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary> Parses the provided this.CtvString for the specific controller result bits. </summary>
        /// <param name="executionblock">The controller to check.</param>
        /// <param name="type"> This is they type to debug.</param>
        /// <param name="ctvcapture">list of ctvs captured for the execution.</param>
        /// <param name="vfdmlookup"> VFDM lookup from.</param>
        /// <param name="threadnum"> Threadnum executing.</param>
        /// <param name="pattern"> Patternname.</param>
        /// <returns>Whether its a global controller failure and set all memories in that controller.  All will apply to memories under current execution only.</returns>
        public virtual (bool, HashSet<int>) ExtractExecution(HryJsonParser.PatternBlocks.ControllersBlock.ExecutionBlocks executionblock, AllowedTypes type, string ctvcapture, Virtualfuse vfdmlookup, long threadnum, string pattern)
        {
            var failindex = new HashSet<int>();
            var statussplit = this.ExtractStatus(ctvcapture, executionblock.Status);

            if (this.Loglevel != Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED)
            {
                this.DebugCaptures[threadnum].Add($"[{MethodBase.GetCurrentMethod().Name}]\tstatus bits positions:\t{string.Join(",", executionblock.Status)}, \tCaptured status bits:\t{string.Join(string.Empty, statussplit)}");
            }

            // this.Console?.PrintDebug($"[ExtractExecution]\tstatus bits positions:\t{string.Join(",", executionblock.Status)}, \tCaptured status bits:\t{string.Join(string.Empty, statussplit)}");
            if (this.Contains1(statussplit[0]))
            {
                if (this.Loglevel != Prime.TestMethods.TestMethodBase.PrimeLogLevel.DISABLED)
                {
                    this.DebugCaptures[threadnum].Add($"[{MethodBase.GetCurrentMethod().Name}]\t\t-Controller Failure");
                }

                // this.Console?.PrintDebug("[ExtractExecution]\t\t-Controller Failure");
                return (true, failindex);
            }
            else
            {
                var palgo = this.ExtractOther(ctvcapture, executionblock.PAlgo_sel);
                var algo = this.ExtractOther(ctvcapture, executionblock.Algo_sel);

                // this.Console?.PrintDebug($"[ExtractExecution]\tpalgo_sel bits positions:\t[{string.Join(",", executionblock.PAlgo_sel)}], \tCaptured palgo_sel bits:\t{palgo}");
                // this.Console?.PrintDebug($"[ExtractExecution]\talgo_sel bits positions:\t[{string.Join(",", executionblock.Algo_sel)}], \tCaptured algo_ bits:\t{algo}");
                var (skippermem, statuspass) = this.GlobalFieldsCheck(algo + palgo, statussplit[1], executionblock.HryIndex, pattern);

                if (this.Loglevel == Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG)
                {
                    this.DebugCaptures[threadnum].Add($"[{MethodBase.GetCurrentMethod().Name}]\tpalgo_sel bits positions:\t[{string.Join(",", executionblock.PAlgo_sel)}], \tCaptured palgo_sel bits:\t{palgo}");
                    this.DebugCaptures[threadnum].Add($"[{MethodBase.GetCurrentMethod().Name}]\talgo_sel bits positions:\t[{string.Join(",", executionblock.Algo_sel)}], \tCaptured algo_ bits:\t{algo}");
                    this.DebugCaptures[threadnum].Add($"[{MethodBase.GetCurrentMethod().Name}]\tstatuspass:\t[{statuspass}], skippermem:\t[{skippermem}]");
                }

                // this.Console?.PrintDebug($"[ExtractExecution]\tstatuspass:\t[{statuspass}], skippermem:\t[{skippermem}]");
                if (executionblock.Memories == null && skippermem == false)
                {
                    if (statuspass == true)
                    {
                        this.UpdateAllMems(ResultNameChar.Pass, executionblock.HryIndex);
                    }
                    else
                    {
                        this.UpdateAllMems(ResultNameChar.Fail, executionblock.HryIndex);
                    }
                }
                else if (skippermem == false)
                {
                    var (memresults, failinmem) = this.ExtractMemCtvs(ctvcapture, executionblock.Memories);

                    if (this.Loglevel == Prime.TestMethods.TestMethodBase.PrimeLogLevel.PRIME_DEBUG || skippermem == false)
                    {
                        foreach (KeyValuePair<string, string> mem in memresults)
                        {
                            this.DebugCaptures[threadnum].Add($"[{MethodBase.GetCurrentMethod().Name}] MEMORY {mem.Key}, \tCaptured datat: [{mem.Value}]");

                            // this.Console?.PrintDebug($"[ExtractMemCtvs] MEMORY {mem.Key}, \tCaptured datat: [{mem.Value}]");
                        }
                    }

                    switch (type)
                    {
                        case AllowedTypes.GOID:
                            if (failinmem == true && statuspass == false)
                            {
                                failindex = this.SetMemoryDataGOID(memresults, executionblock.HryIndex, pattern);
                            }
                            else if (failinmem == false && statuspass == true)
                            {
                                this.UpdateAllMems(ResultNameChar.Pass, executionblock.HryIndex);
                            }
                            else if (failinmem == true && statuspass == true && statussplit[1].Length == 1)
                            {
                                failindex = this.SetMemoryDataGOID(memresults, executionblock.HryIndex, pattern);
                            }
                            else if (failinmem == true && statuspass == true)
                            {
                                this.UpdateAllMems(ResultNameChar.Inconsist_ps_fg, executionblock.HryIndex, pattern);
                                failindex = new HashSet<int>(executionblock.HryIndex.Values);
                            }
                            else if (failinmem == false && statuspass == false)
                            {
                                this.UpdateAllMems(ResultNameChar.Inconsist_fs_pg, executionblock.HryIndex, pattern);
                                failindex = new HashSet<int>(executionblock.HryIndex.Values);
                            }
                            else
                            {
                                Prime.Services.ConsoleService.PrintError($"[{MethodBase.GetCurrentMethod().Name}]\tHit an invalid option in GOID parsing");
                            }

                            break;

                        case AllowedTypes.RABITS:
                            if (failinmem == true)
                            {
                                this.SetMemoryDataBIRA(statuspass, memresults, executionblock.HryIndex);
                            }
                            else if (failinmem == false && statuspass == true)
                            {
                                this.UpdateAllMems(ResultNameChar.Pass, executionblock.HryIndex);
                            }
                            else if (failinmem == false && statuspass == false)
                            {
                                this.UpdateAllMems(ResultNameChar.Inconsist_fs_pg, executionblock.HryIndex);
                            }
                            else
                            {
                                Prime.Services.ConsoleService.PrintError($"[{MethodBase.GetCurrentMethod().Name}]\tHit an invalid option in BIRA parsing");
                            }

                            break;

                        case AllowedTypes.VFDM:
                            if (failinmem == true && statuspass == false)
                            {
                                this.SetMemoryDataVFDM(memresults, executionblock.HryIndex, vfdmlookup);
                            }
                            else if (failinmem == false && statuspass == true)
                            {
                                this.UpdateAllMems(ResultNameChar.Pass, executionblock.HryIndex);
                            }
                            else if (failinmem == false && statuspass == false)
                            {
                                this.UpdateAllMems(ResultNameChar.Inconsist_fs_pg, executionblock.HryIndex);
                            }
                            else
                            {
                                Prime.Services.ConsoleService.PrintError($"[{MethodBase.GetCurrentMethod().Name}]\tHit an invalid option in VFDM parsing");
                            }

                            break;
                    }
                }

                return (false, failindex);
            }
        }

        /// <summary> Sets memory statuses for each memory for BIRA.</summary>
        /// <param name="memdata"> CTV data for each memory.</param>
        /// <param name="memidx"> Memory to index in the HRY string.</param>
        /// <param name="vfdmlookup"> VFDM lookup from.</param>
        public void SetMemoryDataVFDM(Dictionary<string, string> memdata, Dictionary<string, int> memidx, Virtualfuse vfdmlookup)
        {
            foreach (KeyValuePair<string, int> mem in memidx)
            {
                if (this.Contains1(memdata[mem.Key]))
                {
                    var reverseddata = memdata[mem.Key].ToCharArray();
                    Array.Reverse(reverseddata);
                    var revString = new string(reverseddata);
                    List<int> foundcol = new List<int>();
                    int i = 0;
                    while ((i = revString.IndexOf('1', i)) != -1)
                    {
                        foundcol.Add(i);
                        i++;
                    }

                    var foundinfuse = false;

                    // this.Console?.PrintDebug($"ColumnFailures: [{string.Join(",", foundcol)}]");
                    foreach (KeyValuePair<string, VirtFuseColOnlyJsonParser.VFDMBLOCKS> temp in vfdmlookup.VfdmLookupTable.GSDSNAMEs)
                    {
                        var idx = 0;
                        foreach (var fuse in temp.Value.FullFusemapIdx)
                        {
                            if (fuse.Contains(memidx[mem.Key]))
                            {
                                foundinfuse = true;
                                if (vfdmlookup.Virtualfusedata[temp.Key].Fuses[idx].Available == true)
                                {
                                    vfdmlookup.Virtualfusedata[temp.Key].Fuses[idx].Available = false;
                                    var firstindex = foundcol[0];
                                    foundcol.Remove(firstindex);
                                    var tempstring = Convert.ToString(firstindex, 2);
                                    var start = temp.Value.FusePositions[idx].Split('-').Select(int.Parse).ToList();
                                    var length = Math.Abs(start[0] - start[1]) + 1;
                                    vfdmlookup.Virtualfusedata[temp.Key].Fuses[idx].FuseValue = tempstring.PadLeft(length, '0');
                                }
                                else
                                {
                                    var valuefuse = int.Parse(vfdmlookup.Virtualfusedata[temp.Key].Fuses[idx].FuseValue);
                                    if (foundcol.Contains(valuefuse))
                                    {
                                        foundcol.Remove(foundcol.IndexOf(valuefuse));
                                    }
                                }
                            }

                            idx += 1;

                            if (foundcol.Count() == 0)
                            {
                                break;
                            }
                        }
                    }

                    if (foundcol.Count() > 0 && foundinfuse == false)
                    {
                        this.CurrentHryString.AddOrUpdate(mem.Value, (char)ResultNameChar.Fail, (key, existingVal) => this.ChoosePriorityResult(existingVal, ResultNameChar.Fail));
                    }
                    else if (foundcol.Count() > 0 && foundinfuse == true)
                    {
                        this.CurrentHryString.AddOrUpdate(mem.Value, (char)ResultNameChar.Unrepairable, (key, existingVal) => this.ChoosePriorityResult(existingVal, ResultNameChar.Fail));
                    }
                    else
                    {
                        this.CurrentHryString.AddOrUpdate(mem.Value, (char)ResultNameChar.Repairable, (key, existingVal) => this.ChoosePriorityResult(existingVal, ResultNameChar.Repairable));
                    }
                }
                else
                {
                    this.CurrentHryString.AddOrUpdate(mem.Value, (char)ResultNameChar.Pass, (key, existingVal) => this.ChoosePriorityResult(existingVal, ResultNameChar.Pass));
                }
            }
        }

        /// <summary> Sets memory statuses for each memory for BIRA.</summary>
        /// <param name="statuspass"> Expecting the test to fully pass.</param>
        /// <param name="memdata"> CTV data for each memory.</param>
        /// <param name="memidx"> Memory to index in the HRY string.</param>
        public void SetMemoryDataBIRA(bool statuspass, Dictionary<string, string> memdata, Dictionary<string, int> memidx)
        {
            ResultNameChar globalerror = ResultNameChar.Mass_failure;
            var exitloop = false;
            foreach (KeyValuePair<string, int> mem in memidx)
            {
                if (memdata[mem.Key].Length == 1)
                {
                    switch (memdata[mem.Key])
                    {
                        case "0":
                            this.CurrentHryString.AddOrUpdate(mem.Value, (char)ResultNameChar.Pass, (key, existingVal) => this.ChoosePriorityResult(existingVal, ResultNameChar.Pass));
                            break;
                        case "1":
                            if (statuspass == true)
                            {
                                globalerror = ResultNameChar.Inconsist_ps_fg;
                                exitloop = true;
                            }
                            else
                            {
                                this.CurrentHryString.AddOrUpdate(mem.Value, (char)ResultNameChar.Fail, (key, existingVal) => this.ChoosePriorityResult(existingVal, ResultNameChar.Fail));
                            }

                            break;
                    }
                }
                else
                {
                    switch (memdata[mem.Key])
                    {
                        case "00":
                            this.CurrentHryString.AddOrUpdate(mem.Value, (char)ResultNameChar.Pass, (key, existingVal) => this.ChoosePriorityResult(existingVal, ResultNameChar.Pass));
                            break;
                        case "01":
                            this.CurrentHryString.AddOrUpdate(mem.Value, (char)ResultNameChar.Repairable, (key, existingVal) => this.ChoosePriorityResult(existingVal, ResultNameChar.Repairable));
                            break;
                        default:
                            if (statuspass == true)
                            {
                                globalerror = ResultNameChar.Inconsist_ps_fg;
                                exitloop = true;
                            }
                            else
                            {
                                this.CurrentHryString.AddOrUpdate(mem.Value, (char)ResultNameChar.Unrepairable, (key, existingVal) => this.ChoosePriorityResult(existingVal, ResultNameChar.Unrepairable));
                            }

                            break;
                    }
                }

                if (exitloop == true)
                {
                    break;
                }
            }

            if (globalerror != ResultNameChar.Mass_failure)
            {
                this.UpdateAllMems(globalerror, memidx);
            }
        }

        /// <summary> Returns the value to update used in addorupdate dirctionary function.</summary>
        /// <param name="existingVal"> All mems combined into a single string.</param>
        /// <param name="updateval"> Ctvs caputred for this execution.</param>
        /// <returns> Charvalue to updateto.</returns>
        public char ChoosePriorityResult(char existingVal, dynamic updateval)
        {
            if (updateval is char)
            {
                if (ResultsReferenceTable[(ResultNameChar)existingVal] < ResultsReferenceTable[(ResultNameChar)updateval])
                {
                    return (char)updateval;
                }
                else
                {
                    return existingVal;
                }
            }
            else
            {
                if (ResultsReferenceTable[(ResultNameChar)existingVal] < ResultsReferenceTable[updateval])
                {
                    return (char)updateval;
                }
                else
                {
                    return existingVal;
                }
            }
        }

        /// <summary> Returns the value to update used in addorupdate dirctionary function.</summary>
        /// <param name="existingVal"> All mems combined into a single string.</param>
        /// <param name="updateval"> Ctvs caputred for this execution.</param>
        /// <returns> Charvalue to updateto.</returns>
        public ResultNameChar ChoosePriorityResult(ResultNameChar existingVal, dynamic updateval)
        {
            if (updateval is char)
            {
                if (ResultsReferenceTable[existingVal] < ResultsReferenceTable[(ResultNameChar)updateval])
                {
                    return updateval;
                }
                else
                {
                    return existingVal;
                }
            }
            else
            {
                if (ResultsReferenceTable[existingVal] < ResultsReferenceTable[updateval])
                {
                    return updateval;
                }
                else
                {
                    return existingVal;
                }
            }
        }

        /// <summary> Extracts all memory for each memory found in this execution.</summary>
        /// <param name="ctvdata"> Ctvs caputred for this execution.</param>
        /// <param name="memories">Specific memories in execution.</param>
        /// <returns> Returns a tuple dictionary of mems and captured data and all mems passed.</returns>
        public (Dictionary<string, string> capturemems, bool failure) ExtractMemCtvs(string ctvdata, Dictionary<string, string> memories)
        {
            var foundone = false;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (memories.Count() > 0)
            {
                foreach (KeyValuePair<string, string> mem in memories)
                {
                    var tempstring = this.ExtractOther(ctvdata, mem.Value);
                    results.Add(mem.Key, tempstring);
                    if (this.Contains1(tempstring))
                    {
                        foundone = true;
                    }

                    // this.Console?.PrintDebug($"[ExtractMemCtvs] MEMORY {mem.Key}, \tCaptured datat: [{tempstring}]");
                }
            }

            return (results, foundone);
        }

        /// <summary> Sets memory statuses for each memory.</summary>
        /// <param name="memdata"> CTV data for each memory.</param>
        /// <param name="memidx"> Memory to index in the HRY string.</param>
        /// <param name="pattern"> Pattern Name.</param>
        /// <returns> Set of indexs that fail.</returns>
        public HashSet<int> SetMemoryDataGOID(Dictionary<string, string> memdata, Dictionary<string, int> memidx, string pattern = "")
        {
            var failidx = new HashSet<int>();

            foreach (KeyValuePair<string, int> mem in memidx)
            {
                if (this.Contains1(memdata[mem.Key]))
                {
                    this.CurrentHryString.AddOrUpdate(mem.Value, (char)ResultNameChar.Fail, (key, existingVal) => this.ChoosePriorityResult(existingVal, ResultNameChar.Fail));
                    failidx.Add(mem.Value);
                    if (pattern != string.Empty)
                    {
                        if (!this.ConcurrentFails.ContainsKey(pattern))
                        {
                            this.ConcurrentFails.GetOrAdd(pattern, s => new ConcurrentDictionary<string, ResultNameChar>());
                        }

                        this.ConcurrentFails[pattern].AddOrUpdate(this.HryLookupTable.HryStringRef[mem.Value], ResultNameChar.Fail, (key, existingVal) => this.ChoosePriorityResult(existingVal, ResultNameChar.Fail));
                    }
                }
                else
                {
                    this.CurrentHryString.AddOrUpdate(mem.Value, (char)ResultNameChar.Pass, (key, existingVal) => this.ChoosePriorityResult(existingVal, ResultNameChar.Pass));
                }
            }

            return failidx;
        }

        /// <summary> Updates all the memories. </summary>
        /// <param name="algobits">String of algo bits.</param>
        /// <param name="status"> Last 2 or 1 bits of the status.</param>
        /// <param name="memlocations"> mem, location to be updated.</param>
        /// <param name="pattern"> Pattern name.</param>
        /// <returns> Returns boolean for whether you need to run memlevel. </returns>
        public (bool skippermem, bool statuspass) GlobalFieldsCheck(string algobits, string status, Dictionary<string, int> memlocations, string pattern = "")
        {
            var statuspass = false;
            var skipmemlevel = false;
            bool algofail = this.Contains1(algobits);

            switch (status)
            {
                case "00":
                    if (algofail == true)
                    {
                        this.UpdateAllMems(ResultNameChar.Pattern_misprogram, memlocations, pattern);
                        skipmemlevel = true;
                    }
                    else
                    {
                        statuspass = true;
                    }

                    break;
                case "01":
                    if (algofail == false)
                    {
                        this.UpdateAllMems(ResultNameChar.Pattern_bad_wait, memlocations, pattern);
                    }
                    else
                    {
                        this.UpdateAllMems(ResultNameChar.Pattern_misprogram, memlocations, pattern);
                    }

                    skipmemlevel = true;
                    break;
                case "10":
                    if (algofail == true)
                    {
                        this.UpdateAllMems(ResultNameChar.Pattern_misprogram, memlocations, pattern);
                        skipmemlevel = true;
                    }
                    else
                    {
                        statuspass = false;
                    }

                    break;
                case "11":
                    this.UpdateAllMems(ResultNameChar.Pattern_misprogram, memlocations, pattern);
                    skipmemlevel = true;
                    break;
                case "0":
                    statuspass = true;
                    break;
                case "1":
                    this.UpdateAllMems(ResultNameChar.Pattern_bad_wait, memlocations, pattern);
                    skipmemlevel = true;
                    break;
                default:
                    Prime.Services.ConsoleService.PrintError($"\t\t[{MethodBase.GetCurrentMethod().Name}] Something is wrong Done not compelete and state not found");
                    break;
            }

            return (skipmemlevel, statuspass);
        }

        /// <summary> Updates all the memories. </summary>
        /// <param name="globalapply">Character to update all results.</param>
        /// <param name="memlocations"> mem, location to be updated.</param>
        /// <param name="pattern"> Pattern name.</param>
        public void UpdateAllMems(ResultNameChar globalapply, Dictionary<string, int> memlocations, string pattern = "")
        {
            foreach (var mem in memlocations)
            {
                this.CurrentHryString.AddOrUpdate(mem.Value, (char)globalapply, (key, existingVal) => this.ChoosePriorityResult(existingVal, globalapply));
                if (pattern != string.Empty)
                {
                    if (globalapply != ResultNameChar.Pass)
                    {
                        if (!this.ConcurrentFails.ContainsKey(pattern))
                        {
                            this.ConcurrentFails.GetOrAdd(pattern, s => new ConcurrentDictionary<string, ResultNameChar>());
                        }

                        this.ConcurrentFails[pattern].AddOrUpdate(this.HryLookupTable.HryStringRef[mem.Value], globalapply, (key, existingVal) => this.ChoosePriorityResult(existingVal, globalapply));
                    }
                }
            }
        }

        /// <summary> Updates all the memories. </summary>
        /// <param name="globalapply">Character to update all results.</param>
        /// <param name="memlocations"> mem, location to be updated.</param>
        public void UpdateAllMems(ResultNameChar globalapply, HashSet<int> memlocations)
        {
            foreach (var mem in memlocations)
            {
                this.CurrentHryString.AddOrUpdate(mem, (char)globalapply, (key, existingVal) => this.ChoosePriorityResult(existingVal, globalapply));
            }
        }

        /// <summary>extracts raw data from ctv string.</summary>
        /// <param name = "ctvdata" > Raw captured CTV data.</param>
        /// <param name = "extractbits" > Extractedbits for the field needed.</param>
        /// <returns>string data.</returns>
        public string ExtractOther(string ctvdata, string extractbits)
        {
            if (extractbits != string.Empty)
            {
                if (!extractbits.Contains('-'))
                {
                    return ctvdata[int.Parse(extractbits)].ToString();
                }
                else
                {
                    var reverse = false;
                    var stfin = extractbits.Split('-').Select(int.Parse).ToList();
                    int length = Math.Abs(stfin[0] - stfin[1]) + 1;
                    if (stfin[0] > stfin[1])
                    {
                        reverse = true;
                    }

                    if (reverse == true)
                    {
                        var charA = ctvdata.Substring(stfin[1], length).ToCharArray();
                        Array.Reverse(charA);
                        return new string(charA);
                    }
                    else
                    {
                        return ctvdata.Substring(stfin[0], length);
                    }
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>Clears HRY string.</summary>
        public void ClearCurHryString()
        {
            this.CurrentHryString = new ConcurrentDictionary<int, char>();
        }

        /// <summary>Extract status.</summary>
        /// <param name = "ctvdata" > Raw captured CTV data.</param>
        /// <param name = "extractbits" > Extractedbits for the field needed.</param>
        /// <returns>List of string of captured data of status split.</returns>
        public List<string> ExtractStatus(string ctvdata, List<int> extractbits)
        {
            List<string> statussplit = new List<string>();
            statussplit.Add(string.Empty);
            string captureddata = string.Empty;
            var splitloc = 0;
            if (extractbits.Count == 2)
            {
                splitloc = 1;
            }
            else
            {
                splitloc = 2;
            }

            var count = 0;
            foreach (var bit in extractbits)
            {
                if (count == splitloc)
                {
                    statussplit.Add(string.Empty);
                }

                statussplit[statussplit.Count - 1] += ctvdata[bit];
                count += 1;
            }

            return statussplit;
        }

        /// <summary>Writes Hrystring to Shared Storage.</summary>
        /// <param name = "hrystring" > Data to write to shared storage.</param>
        public virtual void HryWriteSharedStorage(List<char> hrystring)
        {
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] SharedStorage Write: {this.HrystringNameSS} : [{string.Join(string.Empty, hrystring)}]");
            Services.SharedStorageService.InsertRowAtTable(this.HrystringNameSS, string.Join(string.Empty, hrystring), Context.DUT);
        }

        /// <summary>Read HRY string from shared storage.</summary>
        /// <returns>Returns shared storage value.</returns>
        public List<char> HryReadSharedStorage()
        {
            var retrievedValue = (string)Prime.Services.SharedStorageService.GetStringRowFromTable(this.HrystringNameSS, Context.DUT);
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] SharedStorage Read:  {this.HrystringNameSS} : [{retrievedValue}].");
            return retrievedValue.ToList<char>();
        }

        /// <summary> Prints specified data to ituff. </summary>
        /// <param name="data">Data to print to ituff.</param>
        public void HryPrintDataToItuff(string data)
        {
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] SharedStorage Write:  {this.HrystringNameITUFF}.");
            /* Prime.Services.DatalogService.WriteToItuff($"2_tname_{this.HrystringNameITUFF}\n2_strgval_{data}\n"); */
            MbistVminTC.WriteStrgvalToItuff(this.HrystringNameITUFF, string.Join(",", data));
        }
    }
}
