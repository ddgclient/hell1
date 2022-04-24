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
    using Newtonsoft.Json;
    using Prime;
    using Prime.ConsoleService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Recovery Class.
    /// </summary>
    public class Recovery
    {
        /// <summary>Static name of the recovery name used in shared storage. </summary>
        private static string recoveryLookupName = "recoveryLookupTable";

        /// <summary>Gets or sets vfdm lookup table. </summary>
        public string SSRecoveryName { get; set; } = "recoverydata";

        /// <summary>Gets or sets vfdm lookup table. </summary>
        public string DFFRecoveryName { get; set; } = "recoverydata";

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        public IConsoleService Console { get; set; } = null;

        /// <summary>Writes Recovery String to Shared Storage.</summary>
        /// <param name = "recoverydata" > Data to write to shared storage.</param>
        public virtual void RecoveryWriteSharedStorage(List<char> recoverydata)
        {
            var storval = string.Join(string.Empty, recoverydata);
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] : {this.SSRecoveryName} : [{storval}]");
            Services.SharedStorageService.InsertRowAtTable(this.SSRecoveryName, storval, Context.DUT);
        }

        /// <summary> Write DFF.</summary>
        /// <param name = "recoverydata" > Data to write to shared storage.</param>
        public void WriteDff(List<char> recoverydata)
        {
            Services.DffService.SetDff(this.DFFRecoveryName, string.Join(string.Empty, recoverydata));
        }

        /// <summary> GetBisrData.</summary>
        /// <param name = "dff" > Whether to read from DFF or Shared Storage.</param>
        /// <returns> Returns recovery string.</returns>
        public List<char> ReadData(bool dff = false)
        {
            if (dff == true)
            {
                return this.RecoveryReadDff();
            }
            else
            {
                return this.RecoveryReadSharedStorage();
            }
        }

        /// <summary> WriteBisrData.</summary>
        /// <param name = "recoverydata" > Data to write to shared storage.</param>
        /// <param name="dff"> Whether to read from DFF or Shared Storage.</param>
        public void WriteData(List<char> recoverydata, bool dff = false)
        {
            this.RecoveryWriteSharedStorage(recoverydata);
            if (dff == true)
            {
                this.WriteDff(recoverydata);
            }
        }

        /// <summary> ReadDFF.</summary>
        /// <returns>Returns value from DFF.</returns>
        public List<char> RecoveryReadDff()
        {
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Retrieving from shared for key : [{this.DFFRecoveryName}].");
            var retrievedValue = (string)Services.DffService.GetDff(this.DFFRecoveryName);
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Retrieving value from DFF: [{retrievedValue}].");
            var returnVal = retrievedValue.ToCharArray().ToList<char>();
            return returnVal;
        }

        /// <summary>Writes Recovery String to Shared Storage.</summary>
        /// <returns>Shared Storage .</returns>
        public virtual List<char> RecoveryReadSharedStorage()
        {
            List<char> returnVal = new List<char>();
            string retrievedValue = Prime.Services.SharedStorageService.GetStringRowFromTable(this.SSRecoveryName, Context.DUT);
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] : {this.SSRecoveryName} : [{retrievedValue.ToString()}]");
            returnVal = retrievedValue.ToCharArray().ToList<char>();
            return returnVal;
        }

        /// <summary>Will be called in the.</summary>
        /// <param name = "jsonfile" > Name of the JSON file to lao.</param>
        /// <returns>recoveryJsonParser class or null on fail.</returns>
        public virtual RecoveryJsonParser RecoveryJsonParser(string jsonfile)
        {
            string localFilePath = Prime.Services.FileService.GetFile(jsonfile);
            if (string.IsNullOrEmpty(localFilePath))
            {
                Prime.Services.ConsoleService.PrintError($"[RecoveryJsonParser] Error, prime GetFile({jsonfile}) returned empty string, file probably doesn't exist.");
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<RecoveryJsonParser>(File.ReadAllText(localFilePath));
            }
            catch (JsonException ex)
            {
                Prime.Services.ConsoleService.PrintError($"[RecoveryJsonParser] Error, failed to load hry file=[{jsonfile}]. Exception=[{ex.Message}].");
                return null;
            }
        }

        /// <summary>Will Create RecoveryLookup and add to shared storage for later use if not exist.</summary>
        /// <param name = "forceConfigFileParseState" > Force the configs to be repulled.</param>
        /// <param name = "recoveryConfigurationFile" > Name of recovery configfile.</param>
        /// <param name = "hryStringReference" > Reference used to build the proper controllers for recovery.</param>
        /// <returns>recoveryJsonParser class or null on fail.</returns>
        public RecoveryJsonParser RecoveryfileParse(MbistVminTC.EnableStates forceConfigFileParseState, string recoveryConfigurationFile, List<string> hryStringReference)
        {
            RecoveryJsonParser recoveryLookupTable = new RecoveryJsonParser();

            // Creates RecoveryReference if recovery mode enabled
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Recovery Check.");
            if (forceConfigFileParseState == MbistVminTC.EnableStates.Enabled)
            {
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Force pull recovery JSON from RecoveryFile {recoveryConfigurationFile}");
                recoveryLookupTable = this.RecoveryJsonParser(Prime.Services.FileService.GetFile(recoveryConfigurationFile));
                if (recoveryLookupTable.BuildIPsRecovery(hryStringReference) == true)
                {
                    Services.ConsoleService.PrintError($"[{MethodBase.GetCurrentMethod().Name}] During parsing of the recovery options duplicates were found in a given option and you must resolve for this to work");
                }
                else
                {
                    Prime.Services.SharedStorageService.InsertRowAtTable(recoveryLookupName, recoveryLookupTable, Context.LOT);
                }
            }
            else
            {
                try
                {
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Pulling recovery JSON from Shared storage");
                    recoveryLookupTable = (RecoveryJsonParser)Prime.Services.SharedStorageService.GetRowFromTable(recoveryLookupName, typeof(RecoveryJsonParser), Context.LOT);
                }
                catch
                {
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Failed to pull recovery JSON from Shared storage");
                    recoveryLookupTable = this.RecoveryJsonParser(Prime.Services.FileService.GetFile(recoveryConfigurationFile));
                    if (recoveryLookupTable != null)
                    {
                        if (recoveryLookupTable.BuildIPsRecovery(hryStringReference) == true)
                        {
                            Services.ConsoleService.PrintError($"[{MethodBase.GetCurrentMethod().Name}] During parsing of the recovery options duplicates were found in a given option and you must resolve for this to work");
                        }
                        else
                        {
                            Prime.Services.SharedStorageService.InsertRowAtTable(recoveryLookupName, recoveryLookupTable, Context.LOT);
                        }
                    }
                    else
                    {
                        Services.ConsoleService.PrintError($"[{MethodBase.GetCurrentMethod().Name}] During parsing of the recovery file {recoveryConfigurationFile} an issue was found and was unable to parse");
                    }
                }
            }

            return recoveryLookupTable;
        }

        /// <summary>.</summary>
        /// <param name = "hrystring" > HRY string results.</param>
        /// <param name = "recoveryjson" > revoeryblock.</param>
        /// <param name = "currentrecoverystring" > Incomming recovery string.</param>
        /// <param name = "recdownbin" > Whether downbin is allowed or not.</param>
        /// <returns> Returns recovery results class with results from the check.</returns>
        public RecoveryResults ParseResults(ConcurrentDictionary<int, char> hrystring, RecoveryJsonParser recoveryjson, List<char> currentrecoverystring, MbistVminTC.EnableStates recdownbin)
        {
            var recoveryres = new RecoveryResults();
            recoveryres.Remove_Mems = new Dictionary<string, List<string>>();
            recoveryres.RecoveryModes = new List<string>();
            recoveryres.RecoveryString = Enumerable.Repeat('!', recoveryjson.RecoveryToken.Count).ToList();

            /* TODO This should not be set as base should initialize this value
            if (currentrecoverystring == null)
            {
                currentrecoverystring = Enumerable.Repeat(recoveryjson.PassValue, recoveryjson.RecoveryToken.Count).ToList();
            }
            else if (currentrecoverystring.Count() == 0)
            {
                currentrecoverystring = Enumerable.Repeat(recoveryjson.PassValue, recoveryjson.RecoveryToken.Count).ToList();
            }
            */

            foreach (var item in recoveryjson.RecoveryToken)
            {
                var idxrecovtoken = recoveryjson.RecoveryToken.IndexOf(item);
                var result = new Dictionary<char, List<bool>>();

                var fullchar = currentrecoverystring[idxrecovtoken];
                recoveryres.RecoveryString[idxrecovtoken] = fullchar;
                recoveryres.RecoveryModes.Add(recoveryjson.DesignIPs[item].RecoveryType);

                if (recoveryres.RecoveryString[idxrecovtoken] != recoveryjson.ErrorValue && recoveryres.RecoveryString[idxrecovtoken] != recoveryjson.FailValue)
                {
                    foreach (var opt in recoveryjson.DesignIPs[item].SkusbyIndex)
                    {
                        var temp = this.Checkpasslist(hrystring, opt.Value);
                        if (temp == null)
                        {
                            Prime.Services.ConsoleService.PrintError($"[ParseResults] Error, Controllers not defined in option:({opt})");
                        }
                        else
                        {
                            result[opt.Key.ToCharArray()[0]] = temp;
                        }
                    }

                    if (recoveryjson.DesignIPs[item].RequiredForAllmodes.Count > 0)
                    {
                        var required = this.Checkpasslist(hrystring, recoveryjson.DesignIPs[item].RequiredForAllmodesbyIndex);
                        this.MustPassfunct(required, recoveryjson.FailValue, recoveryjson.FailValue, fullchar, recoveryjson.ErrorValue, item, ref recoveryres, recoveryjson.DesignIPs[item].RequiredForAllmodesbymem, idxrecovtoken);
                    }

                    if (recoveryres.RecoveryString[idxrecovtoken] != recoveryjson.ErrorValue || recoveryres.RecoveryString[idxrecovtoken] != recoveryjson.FailValue)
                    {
                        if (recoveryjson.DesignIPs[item].RecoveryType == "Full")
                        {
                            if (result.Count != 1)
                            {
                                Prime.Services.ConsoleService.PrintError($"[ParseResults] Error, Recovery options for ({item}) has too many options since mode is Full");
                                return null;
                            }
                            else
                            {
                                foreach (var each in result)
                                {
                                    this.MustPassfunct(each.Value, recoveryjson.FailValue, each.Key, fullchar, recoveryjson.ErrorValue, item, ref recoveryres, recoveryjson.DesignIPs[item].SkusbyName[each.Key.ToString()], idxrecovtoken);
                                    if (recoveryjson.DesignIPs[item].RequiredForAllmodesbymem.Count > 0 && recoveryres.RecoveryString[idxrecovtoken] == each.Key)
                                    {
                                        if (recoveryres.Remove_Mems[item].Contains(recoveryjson.DesignIPs[item].RequiredForAllmodesbymem[0]) == false)
                                        {
                                            recoveryres.Remove_Mems[item].AddRange(recoveryjson.DesignIPs[item].RequiredForAllmodesbymem);
                                        }
                                    }
                                }
                            }
                        }
                        else if (recoveryjson.DesignIPs[item].RecoveryType == "Partial")
                        {
                            this.PartialPassfunct(ref result, ref recoveryjson, fullchar, item, ref recoveryres, idxrecovtoken);
                        }
                    }
                }
            }

            recoveryres.CheckRecoveryOptions(recoveryjson.ValidOptions, recoveryjson.RecoveryPriority, recoveryjson.ErrorValue, currentrecoverystring, recdownbin);

            return recoveryres;
        }

        /// <summary>.</summary>
        /// <param name = "result" > List All Mems Tested [0] and Memories all pass [1] .</param>
        /// <param name = "recoveryjson" > This is the variable holding the JSON informains.</param>
        /// <param name = "currentvalue" > fullchar option.</param>
        /// <param name = "item" > Name of the recovery option.</param>
        /// <param name = "recoveryres" > Ref to the recovery options to allow updating within this function.</param>
        /// <param name = "indextoupdate" > Index to update data.</param>
        public void PartialPassfunct(ref Dictionary<char, List<bool>> result, ref RecoveryJsonParser recoveryjson, char currentvalue, string item, ref RecoveryResults recoveryres, int indextoupdate)
        {
            if (recoveryres.RecoveryString[indextoupdate] != recoveryjson.ErrorValue)
            {
                var truecount = 0;
                foreach (var opt in result)
                {
                    if (recoveryres.RecoveryString[indextoupdate] != recoveryjson.ErrorValue)
                    {
                        if (opt.Value[2] != true)
                        {
                            if (opt.Value[0] == false)
                            {
                                Prime.Services.ConsoleService.PrintError($"[PartialPassfunct] Error, Recovery options for ({item}) found memories listed that were untested in option ({opt.Key})");

                                // recoveryres.RecoveryString[indextoupdate] = recoveryjson.ErrorValue;
                            }
                            else
                            {
                                if (opt.Value[1] == true)
                                {
                                    if (recoveryres.RecoveryString[indextoupdate] != recoveryjson.FailValue)
                                    {
                                        if (opt.Key == currentvalue || currentvalue == recoveryjson.PassValue)
                                        {
                                            truecount += 1;
                                            recoveryres.RecoveryString[indextoupdate] = opt.Key;
                                        }
                                        else
                                        {
                                            this.AddToMem(ref recoveryres, item, recoveryjson.DesignIPs[item].SkusbyName[opt.Key.ToString()]);
                                        }
                                    }
                                    else
                                    {
                                        this.AddToMem(ref recoveryres, item, recoveryjson.DesignIPs[item].SkusbyName[opt.Key.ToString()]);
                                    }
                                }
                                else
                                {
                                    this.AddToMem(ref recoveryres, item, recoveryjson.DesignIPs[item].SkusbyName[opt.Key.ToString()]);
                                }
                            }
                        }
                    }
                }

                if (truecount == result.Count)
                {
                    recoveryres.RecoveryString[indextoupdate] = recoveryjson.PassValue;
                }
                else if (truecount == 0 && recoveryres.RecoveryString[indextoupdate] != recoveryjson.ErrorValue)
                {
                    recoveryres.RecoveryString[indextoupdate] = recoveryjson.FailValue;
                }
            }
        }

        /// <summary>.</summary>
        /// <param name = "hrystring" > HRY string results.</param>
        /// <param name = "listindexs" > List of memory/controller indexs to check.</param>
        /// <returns> Returns List k.</returns>
        public List<bool> Checkpasslist(ConcurrentDictionary<int, char> hrystring, List<int> listindexs)
        {
            if (listindexs.Count > 0)
            {
                bool allmems = true;
                bool allpass = true;
                bool nonerun = true;

                foreach (var indexs in listindexs)
                {
                    if (!hrystring.ContainsKey(indexs))
                    {
                        allmems = false;
                    }
                    else if (hrystring[indexs] != (char)Hry.ResultNameChar.Pass && hrystring[indexs] != (char)Hry.ResultNameChar.Pass_retest && hrystring[indexs] != (char)Hry.ResultNameChar.Repairable)
                    {
                        allpass = false;
                    }

                    if (hrystring.ContainsKey(indexs))
                    {
                        if (hrystring[indexs] != (char)Hry.ResultNameChar.Untested)
                        {
                            nonerun = false;
                        }
                    }
                }

                return new List<bool>() { allmems, allpass, nonerun };
            }

            return null;
        }

        /// <summary>.</summary>
        /// <param name = "recoveryres" > Ref to the recovery options to allow updating within this function.</param>
        /// <param name = "item" > Name of the recovery option.</param>
        /// <param name = "memstoRemove" > List of memory/controller indexs to remove.</param>
        public void AddToMem(ref RecoveryResults recoveryres, string item, List<string> memstoRemove)
        {
            if (recoveryres.Remove_Mems.ContainsKey(item) == false)
            {
                recoveryres.Remove_Mems[item] = new List<string>();
            }

            if (recoveryres.Remove_Mems.ContainsKey(item))
            {
                recoveryres.Remove_Mems[item].AddRange(memstoRemove);
            }
            else
            {
                recoveryres.Remove_Mems[item] = memstoRemove;
            }
        }

        /// <summary>.</summary>
        /// <param name = "values" > List All Mems Tested [0] and Memories all pass [1] .</param>
        /// <param name = "norecoverychar" > norecoverychar.</param>
        /// <param name = "currentchar" > currentchar.</param>
        /// <param name = "fullchar" > fullchar option.</param>
        /// <param name = "errorchar" > charactor if some type of issue is occuring.</param>
        /// <param name = "item" > Name of the recovery option.</param>
        /// <param name = "recoveryres" > Ref to the recovery options to allow updating within this function.</param>
        /// <param name = "memstodisable" > List of memories/controllers to disable if it fails.</param>
        /// <param name = "indextoupdate" > Index to update data.</param>
        public void MustPassfunct(List<bool> values, char norecoverychar, char currentchar, char fullchar, char errorchar, string item, ref RecoveryResults recoveryres, List<string> memstodisable, int indextoupdate)
        {
            if (recoveryres.RecoveryString[indextoupdate] != errorchar)
            {
                if (values[2] != true)
                {
                    if (values[0] == true)
                    {
                        if (values[1] == true && recoveryres.RecoveryString[indextoupdate] != norecoverychar)
                        {
                            recoveryres.RecoveryString[indextoupdate] = fullchar;
                        }
                        else
                        {
                            recoveryres.RecoveryString[indextoupdate] = currentchar;
                            this.AddToMem(ref recoveryres, item, memstodisable);
                        }
                    }
                    else
                    {
                        this.Console?.PrintDebug($"[MustPassfunct] Recovery options for ({item}) found memories listed that were untested");

                        // recoveryres.RecoveryString[indextoupdate] = errorchar;
                    }
                }
                else
                {
                    Prime.Services.ConsoleService.PrintError($"[MustPassfunct] Recovery options for ({item}) found memories listed that were untested");

                    // recoveryres.RecoveryString[indextoupdate] = errorchar;
                }
            }
            else
            {
                this.AddToMem(ref recoveryres, item, memstodisable);
            }
        }

        /// <summary>
        /// Recovery Return class.  Contains a list of disabled memories for BISR optimization and the Recovery String.
        /// </summary>
        public class RecoveryResults
        {
            /// <summary> Gets or sets Removable memories.</summary>
            public virtual Dictionary<string, List<string>> Remove_Mems { get; set; }

            /// <summary> Gets or sets Updated Recovery String.</summary>
            public virtual List<char> RecoveryString { get; set; }

            /// <summary> Gets or sets Priority of recovery.</summary>
            public virtual int Priority { get; set; } = 100;

            /// <summary> Gets or sets Mode type for figuring if recovery is needed.</summary>
            public virtual List<string> RecoveryModes { get; set; }

            /// <summary> Gets or sets a value indicating whether gets or sets whethervalid option.</summary>
            public virtual bool ValidRecovery { get; set; } = false;

            /// <summary>Check results.</summary>
            /// <param name = "validoptions" > List of valid recovery skews.</param>
            /// <param name = "priority" > List of priorities for validoptions.</param>
            /// <param name = "errorchar" > Character representing an error has occured.</param>
            /// <param name = "priorrecovery" > Prior recovery value.</param>
            /// <param name = "recdownbin" > Allow recovery downbin.</param>
            public void CheckRecoveryOptions(List<string> validoptions, List<string> priority, char errorchar, List<char> priorrecovery, MbistVminTC.EnableStates recdownbin)
            {
                if (!this.RecoveryString.Contains(errorchar))
                {
                    if (validoptions.Count > 0)
                    {
                        var recidx = 0;
                        foreach (string recoveryoption in validoptions)
                        {
                            var validrecovery = true;
                            List<string> tempcomp = recoveryoption.Split(',').ToList();

                            var idx = 0;
                            foreach (char recoveryval in this.RecoveryString)
                            {
                                if (!tempcomp[idx].Contains(recoveryval))
                                {
                                    validrecovery = false;
                                    break;
                                }

                                idx += 1;
                            }

                            if (validrecovery == true)
                            {
                                this.ValidRecovery = true;
                                if (priority.Count > 0)
                                {
                                    if (priority.Count() - 1 >= recidx)
                                    {
                                        this.Priority = Convert.ToInt32(priority[recidx]);
                                    }
                                }
                            }

                            recidx += 1;
                        }
                    }
                    else
                    {
                        this.ValidRecovery = true;
                        this.Priority = 1;
                    }
                }

                if (recdownbin == MbistVminTC.EnableStates.Disabled && priorrecovery != this.RecoveryString)
                {
                    this.ValidRecovery = false;
                }
            }

            /// <summary>.</summary>
            /// <param name = "o" > Object to compare.</param>
            /// <returns> Returns List k.</returns>
            public override bool Equals(object o)
            {
                var project = o as RecoveryResults;
                if (project != null)
                {
                    var i = 0;

                    var result = this.RecoveryString[i].Equals(project.RecoveryString[i]);
                    foreach (var item in project.Remove_Mems)
                    {
                        if (this.Remove_Mems.ContainsKey(item.Key) == true)
                        {
                            result &= project.Remove_Mems[item.Key].SequenceEqual(this.Remove_Mems[item.Key]);
                        }
                        else
                        {
                            return false;
                        }
                    }

                    return result;
                }

                return false;
            }

            /// <summary>.</summary>
            /// <returns> Returns List k.</returns>
            public override int GetHashCode()
            {
                var hashCode = this.RecoveryString.GetHashCode();

                hashCode ^= this.Remove_Mems.GetHashCode();
                return hashCode;
            }
        }
    }
}
