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
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Main Class for performing MBIST HRY.
    /// </summary>
    public class MbistHRYAlgorithm
    {
        /// <summary>HRY Table.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1025:Code should not contain multiple whitespace in a row", Justification = "Makes HRY map more readable")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1001:Commas should not be preceded by whitespace", Justification = "Makes HRY map more readable")]
        public static readonly Dictionary<string, char> HryTable = new Dictionary<string, char>
        {
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
        };

        /// <summary>priority order for hry bits (from highest to lowest).</summary>
        public static readonly string[] HryPriority = new string[] { "pattern_issue", "cont_fail", "inconsist_fs_pg", "inconsist_ps_fg", "fail", "pass" };

        /// <summary>
        /// Initializes a new instance of the <see cref="MbistHRYAlgorithm"/> class.
        /// </summary>
        public MbistHRYAlgorithm()
        {
        }

        /// <summary>
        ///  Loads the given JSON config file into the Input structure, returns null on any error.
        /// </summary>
        /// <param name="hryfile">Name of the JSON file to lao.</param>
        /// <returns>MbistHRYInput loaded from the file.</returns>
        public virtual MbistHRYInput LoadInputFile(string hryfile)
        {
            string localFilePath = Prime.Services.FileService.GetFile(hryfile);
            if (string.IsNullOrEmpty(localFilePath))
            {
                Prime.Services.ConsoleService.PrintError($"Error, prime GetFile({hryfile}) returned empty string, file probably doesn't exist.");
                return null;
            }

            try
            {
                var json = JsonConvert.DeserializeObject<MbistHRYInput>(File.ReadAllText(localFilePath));
                var errors = json.Validate();
                if (errors.Count != 0)
                {
                    Prime.Services.ConsoleService.PrintError($"Errors detected while reading HRY JSON input file = [{hryfile}].");
                    foreach (var err in errors)
                    {
                        Prime.Services.ConsoleService.PrintError($"\t{err}");
                    }

                    return null;
                }

                return json;
            }
            catch (JsonException ex)
            {
                Prime.Services.ConsoleService.PrintError($"Error, failed to load hry file=[{hryfile}]. Exception=[{ex.Message}].");
                return null;
            }
        }

        /// <summary>
        ///   Function generates the HRY characters for a single Controller.AlgorithmGroup.
        /// </summary>
        /// <param name="ctvData">The full serialized capture data.</param>
        /// <param name="memGroup">The MbistHRYInput.MbistLookupTable.Controller.AlgorithmGroup definition of the group to get hry data for.</param>
        /// <returns>
        ///   Returns a Dictionary where the keys (int) are the index into the full HRY string
        ///   and the values (char) are the new HRY character.
        /// </returns>
        public virtual Dictionary<int, char> GenerateHRYForGroup(string ctvData, MbistHRYInput.MbistLookupTable.Controller.AlgorithmGroup memGroup)
        {
            // Extract the status and algorithm bits from the captured data
            var statusBits = this.Extract_bits(ctvData, memGroup.StatusBits);
            var algoBits = this.Extract_bits(ctvData, memGroup.AlgorithmSelectBits);

            var perEngineResults = false;
            var expectAllPass = false;
            var expectSomeFail = false;

            var hryByIndex = new Dictionary<int, char>();

            var tempHryVal = HryTable["untested"];
            if (algoBits.Contains("1"))
            {
                tempHryVal = HryTable["pattern_issue"];
            }
            else if (statusBits == "0001")
            {
                tempHryVal = HryTable["pattern_issue"];
            }
            else if (statusBits == "0000")
            {
                perEngineResults = true;  // '10' // per-memory results resolved below, expect all to pass
                expectAllPass = true;
            }
            else if (statusBits == "0010")
            {
                perEngineResults = true; // "11"; // per-memory results resolved below, expect at least one to fail
                expectSomeFail = true;
            }
            else if (statusBits == "000")
            {
                perEngineResults = true;  // "12"; // per-memory results resolved below
            }
            else if (statusBits.Length == 3)
            {
                tempHryVal = HryTable["cont_fail"];
            }
            else
            {
                tempHryVal = HryTable["cont_fail"];
            }

            Prime.Services.ConsoleService.PrintDebug($"GenerateHRYForGroup Chkpt1 Status=[{statusBits}] Algorithm=[{algoBits}] PerEngine=[{perEngineResults}] ExpectFail=[{expectSomeFail}] ExpectPass=[{expectAllPass}] TempHry=[{tempHryVal}]");

            // save the temp hry data, still might get overwritten below.
            memGroup.Memories.ForEach(mem => hryByIndex[mem.HryIndex] = tempHryVal);

            // Based on the status results, need to check each GOID bit
            if (perEngineResults)
            {
                var allMemPass = true;
                foreach (var mem in memGroup.Memories)
                {
                    var goIdBits = this.Extract_bits(ctvData, mem.GoIDBits);
                    if (goIdBits.Contains('1'))
                    {
                        allMemPass = false;
                        hryByIndex[mem.HryIndex] = HryTable["fail"];
                    }
                    else
                    {
                        hryByIndex[mem.HryIndex] = HryTable["pass"];
                    }
                }

                // If we were expecting some to fail based on controller status
                // but none did, override them all to inconsistant
                //   Inconsistant Fail Status, Pass GoID
                if (expectSomeFail && allMemPass)
                {
                    memGroup.Memories.ForEach(mem => hryByIndex[mem.HryIndex] = HryTable["inconsist_fs_pg"]);
                }

                // If we were expecting all to pass based on controller status
                // but there were some failures, override them all to inconsistant
                //   Inconsistant Pass Status, Fail GoID
                else if (expectAllPass && !allMemPass)
                {
                    memGroup.Memories.ForEach(mem => hryByIndex[mem.HryIndex] = HryTable["inconsist_ps_fg"]);
                }

                Prime.Services.ConsoleService.PrintDebug($"GenerateHRYForGroup Chkpt2 AllMemPass=[{allMemPass}] PerMemHry=[{string.Join(", ", hryByIndex.Select(kvp => kvp.Key + ":" + kvp.Value))}]");
            }

            return hryByIndex;
        }

        /// <summary>
        ///   Main function.  Translates capture data into a fresh HRY string.
        ///   This function starts with a fresh (all untested) HRY string and does not
        ///   merge it with any existing HRY data (that is done outside this function).
        /// </summary>
        /// <param name="mbistLookup">The full MbistHRYInput.MbistLookupTable for executed plist.</param>
        /// <param name="serializedCaptureData">The full serialized capture dat.</param>
        /// <param name="hryLength">The full length of the HRY string.</param>
        /// <param name="retest">Whether or not this is post-repair retest run or not.</param>
        /// <returns>The full HRY string for this capture only.</returns>
        public virtual string GenerateHRY(MbistHRYInput.MbistLookupTable mbistLookup, string serializedCaptureData, int hryLength, bool retest = false)
        {
            var currentHryStr = new string('U', hryLength);
            var currentHry = currentHryStr.ToCharArray();

            foreach (var controller in mbistLookup.Controllers)
            {
                var hryPerGroupByIndex = new List<Dictionary<int, char>>();

                foreach (var group in controller.Groups)
                {
                    Prime.Services.ConsoleService.PrintDebug($"GenerateHRY generating group hry for controller=[{controller.Name}]");

                    // create the hry for this group
                    hryPerGroupByIndex.Add(this.GenerateHRYForGroup(serializedCaptureData, group));
                } // end foreach (var group in controller.Groups)

                // Transform the List of Dictionaries<index, hry> to a Dictionary<index, List of hry>
                //    FIXME - I'm sure there's a fancy LINQ way to do it but I can't figure it out...
                Dictionary<int, List<char>> allHryValuesByIndex = new Dictionary<int, List<char>>();
                Prime.Services.ConsoleService.PrintDebug($"GenerateHRY Transposing HryValues, count={hryPerGroupByIndex.Count}");
                foreach (var groupDict in hryPerGroupByIndex)
                {
                    foreach (var indexHryPair in groupDict)
                    {
                        if (!allHryValuesByIndex.ContainsKey(indexHryPair.Key))
                        {
                            allHryValuesByIndex[indexHryPair.Key] = new List<char>();
                        }

                        allHryValuesByIndex[indexHryPair.Key].Add(indexHryPair.Value);
                    }
                }

                // Update current hry data after collapsing all groups to a single bit each.
                foreach (var hryIndex in allHryValuesByIndex.Keys)
                {
                    var hryVal = this.CollapseHryByPriority(allHryValuesByIndex[hryIndex]);
                    var currentVal = currentHry[hryIndex];
                    currentHry[hryIndex] = this.MergeHry(currentVal, hryVal);
                    Prime.Services.ConsoleService.PrintDebug($"GenerateHRY Updating Hrystring for Index={hryIndex} Final={currentHry[hryIndex]} Initial={currentVal} CollapsedHry={hryVal} AllGroupsHry=[{string.Join(",", allHryValuesByIndex[hryIndex])}].");
                }
            } // end foreach (var controller in mbistLookup.Controllers)

            currentHryStr = new string(currentHry);
            return currentHryStr;
        }

        /// <summary>
        /// Given a list of HRY characters, it resolves them to a single char based
        /// on the global HryPriority list.
        /// </summary>
        /// <param name="hryList">List of HRY character.</param>
        /// <returns>A single HRY character.</returns>
        public virtual char CollapseHryByPriority(List<char> hryList)
        {
            // make life easy if there's only one element in the list,
            // nothing to merge.
            if (hryList.Count == 1)
            {
                return hryList[0];
            }

            // FIXME - these are invalid in this function, need to check for them.
            //    { "repairable"        , 'Y' },
            //    { "unrepairable"      , 'N' },
            //    { "pass_retest"       , 'P' },
            //    { "fail_retest"       , 'F' },
            //    { "inconsist_pst_fail", '7' },
            var currentHry = HryTable["untested"];

            // override default (untested) if any of the sections had a higher priority result.
            foreach (var token in HryPriority)
            {
                if (hryList.Contains(HryTable[token]))
                {
                    currentHry = HryTable[token];
                    break;
                }
            }

            return currentHry;
        }

        /// <summary>
        /// Merge the current HRY result with the master/original HRY result.
        /// Wrapper around the char based mergeHry.
        /// </summary>
        /// <param name="original">Full HRY string of the original/base HR.</param>
        /// <param name="currentHry">Full HRY string ofthe new/current HR.</param>
        /// <param name="retest">Specifies if this is a post-repair retest or no.</param>
        /// <returns>Final HRY string.</returns>
        public virtual string MergeHry(string original, string currentHry, bool retest = false)
        {
            if (original.Length != currentHry.Length)
            {
                throw new ArgumentException($"original and currentHry string are different lenght in mergeHry - {original.Length} vs {currentHry.Length}");
            }

            // FIXME check performance of this, might be better to use a StringBuilder or list.
            var retval = string.Empty;
            for (int i = 0; i < original.Length; i++)
            {
                retval += this.MergeHry(original[i], currentHry[i], retest);
            }

            return retval;
        }

        /// <summary>
        /// Merge the current HRY result with the master/original HRY result.
        /// Meant to be called from the string based mergeHry
        ///   Same priority as collapseSectionHry uses, but some added complexity
        ///   if the two differ or the retest flag is set.
        /// </summary>
        /// <param name="original">Origingal/Base HRY cha.</param>
        /// <param name="currentHry">New/Current HRY cha.</param>
        /// <param name="retest">Specifies if this is a post-repair retest or no.</param>
        /// <returns>Final HRY Char.</returns>
        public virtual char MergeHry(char original, char currentHry, bool retest = false)
        {
            var retval = original;

            // Start by handling the straight-up priority ones (pattern, controller or inconsistant)
            if (original == HryTable["pattern_issue"] || currentHry == HryTable["pattern_issue"])
            {
                retval = HryTable["pattern_issue"];
            }
            else if (original == HryTable["cont_fail"] || currentHry == HryTable["cont_fail"])
            {
                retval = HryTable["cont_fail"];
            }
            else if (original == HryTable["inconsist_ps_fg"] || currentHry == HryTable["inconsist_ps_fg"])
            {
                retval = HryTable["inconsist_ps_fg"];
            }
            else if (original == HryTable["inconsist_fs_pg"] || currentHry == HryTable["inconsist_fs_pg"])
            {
                retval = HryTable["inconsist_fs_pg"];
            }

            // now check for ones that switched between pass/fail and repairable
            //   pass to fail switch
            else if (original == HryTable["pass"] && currentHry == HryTable["fail"])
            {
                retval = HryTable["inconsist_pst_fail"];
            }

            // fail to pass switch (NOT flagged as retest)
            else if (original == HryTable["fail"] && currentHry == HryTable["pass"] && !retest)
            {
                retval = HryTable["fail"];
            }

            // fail to pass switch (flagged as retest)
            else if (original == HryTable["fail"] && currentHry == HryTable["pass"] && retest)
            {
                retval = HryTable["pass_retest"];
            }

            // flagged repairable by raster/repair but fails on retest
            else if (original == HryTable["repairable"] && currentHry == HryTable["fail"] && retest)
            {
                retval = HryTable["fail_retest"];
            }

            // flagged repairable by raster/repair and passes on retest
            else if (original == HryTable["repairable"] && currentHry == HryTable["pass"] && retest)
            {
                retval = HryTable["pass_retest"];
            }

            // fails original and retest
            else if (original == HryTable["fail"] && currentHry == HryTable["fail"] && retest)
            {
                retval = HryTable["fail_retest"];
            }

            // no original and fails retest
            else if (original == HryTable["untested"] && currentHry == HryTable["fail"] && retest)
            {
                retval = HryTable["fail_retest"];
            }

            // if the current is untested, stick with the original
            else if (currentHry == HryTable["untested"])
            {
                retval = original;
            }

            // all that's left is untested for original and pass/untested for current.
            else
            {
                retval = currentHry;
            }

            return retval;
        }

        /// <summary>
        ///   Extracts a substring based on a list of bit locations.
        /// </summary>
        /// <param name="basestring">string to extract bits fro.</param>
        /// <param name="bits">list of bit locations to extrac.</param>
        /// <returns>string made up of all the char at the locations specificed in the bits list.</returns>
        public virtual string Extract_bits(string basestring, List<int> bits)
        {
            // FIXME - need to optimize.  for big strings a StringBuilder is better, maybe a list
            string retVal = string.Empty;
            try
            {
                foreach (var bit in bits)
                {
                    retVal += basestring[bit];
                }
            }
            catch (IndexOutOfRangeException)
            {
                Prime.Services.ConsoleService.PrintError($"Problem generating Hry when extracting bits from string.  Bits=[{string.Join(",", bits)}] string=[{basestring}].");
                throw;
            }

            return retVal;
        }

        /// <summary>
        /// Figures out the Exit port based on HRY data.
        /// </summary>
        /// <param name="hrystr">The Current HRY string based only on this run, not the final merged on.</param>
        /// <param name="retest">.</param>
        /// <returns>Specifies if this is a post-repair retest or not.</returns>
        public virtual int CalculateExitPort(string hrystr, bool retest)
        {
            var exitPort = -1;
            if (hrystr.Contains(HryTable["pattern_issue"]) ||
                hrystr.Contains(HryTable["cont_fail"]) ||
                hrystr.Contains(HryTable["inconsist_pst_fail"]) ||
                hrystr.Contains(HryTable["inconsist_fs_pg"]) ||
                hrystr.Contains(HryTable["inconsist_ps_fg"]))
            {
                exitPort = 3;
            }
            else if (hrystr.Contains(HryTable["fail"]) ||
                     hrystr.Contains(HryTable["unrepairable"]) ||
                     hrystr.Contains(HryTable["fail_retest"]))
            {
                exitPort = 2;
            }
            else if (hrystr.Contains(HryTable["pass"]) ||
                     hrystr.Contains(HryTable["untested"]) ||
                     (hrystr.Contains(HryTable["pass_retest"]) && retest))
            {
                exitPort = 1;
            }
            else
            {
                exitPort = 0;
            }

            return exitPort;
        }

        /// <summary>
        /// Generates the Ituff output for a given HRY string.
        /// </summary>
        /// <param name="hrystr">HRY string to log to Ituf.</param>
        /// <param name="chunkSize">Will break the HRY string into chunks of this size and print each chunk to a new line (due to ituff line size limitation of ~4000.</param>
        /// <returns>Ituff string.</returns>
        public virtual string GenerateItuffData(string hrystr, int chunkSize = 3960)
        {
            var token = Prime.Services.TpSettingsService.IsTpFeatureEnabled(Prime.TpSettingsService.Feature.Midas) ? "0" : "2";
            var ituffStr = string.Empty;
            var counter = 1;
            for (int i = 0; i < hrystr.Length; i += chunkSize)
            {
                ituffStr += $"{token}_tname_HRY_RAWSTR_MBIST_{counter++}\n";
                ituffStr += $"{token}_strgval_{hrystr.Substring(i, Math.Min(chunkSize, hrystr.Length - i))}\n2_lsep\n";
            }

            return ituffStr;
        }
    }
}
