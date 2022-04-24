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

namespace CtvServices
{
    using System.Collections.Generic;
    using Prime;
    using Prime.TpSettingsService;

    /// <summary>
    /// This class is intended to store all Ituff Printing functions.
    /// </summary>
    public static class ItuffFormat
    {
        /// <summary>
        /// Function that prints a dictionary in ituff format.
        /// </summary>
        /// <param name="ituffDatabase"> The local database to print to ituff. </param>
        public static void PrintToItuff(Dictionary<string, Dictionary<string, Dictionary<string, string>>> ituffDatabase)
        {
            if (ituffDatabase.Count != 0)
            {
                // Init of ituffWriter and Delimiter
                var ituffWriter = Services.DatalogService.GetItuffStrgvalWriter();
                ituffWriter.SetDelimiterCharacterForWrap('|');

                // Gets the token dictionary for each die.
                foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, string>>> tssidDictionary in ituffDatabase)
                {
                    foreach (KeyValuePair<string, Dictionary<string, string>> ituffTokenDictionary in tssidDictionary.Value)
                    {
                        // Contains strgval and msunit
                        var ituffData = ituffTokenDictionary.Value;

                        // Sets the tnamePostfix
                        string tnamePostfix = "_" + ituffTokenDictionary.Key;
                        ituffWriter.SetTnamePostfix(tnamePostfix);

                        // Sets the strgval
                        string data = ituffData["strgval"];
                        string strgval = data.Remove(data.Length - 1, 1); // Removes last pipe
                        ituffWriter.SetData(strgval);

                        // msunit
                        if (ituffData.ContainsKey("msunit"))
                        {
                            string msunit = ituffData["msunit"];
                            /* var msunitAsEnum = (Prime.DatalogService.Unit)System.Enum.Parse(typeof(Prime.DatalogService.Unit), msunit); TODO: Fixme */
                            /* ituffWriter.SetMsUnitAttributes(msunitAsEnum); */
                        }

                        // tssid
                        if (tssidDictionary.Key != "-")
                        {
                            string tssid = tssidDictionary.Key;
                            ituffWriter.SetTssidAttributes(tssid);
                        }

                        // Prints to Ituff
                        Services.DatalogService.WriteToItuff(ituffWriter);

                        /* // Sets database prefix for msunit and tssid print
                        string iTuffLevel = string.Empty;
                        if (Services.TpSettingsService.IsTpFeatureEnabled(Feature.Midas))
                        {
                            iTuffLevel = "0";
                        }
                        else
                        {
                            iTuffLevel = "2";
                        }

                        // Empty var to store tssid and msunit
                        var ituffDescription = string.Empty;

                        // msunit
                        if (ituffData.ContainsKey("msunit"))
                        {
                            string msunit = ituffData["msunit"];
                            ituffDescription += $"{iTuffLevel}_msunit_SPMV1://[!{msunit}]|//\n";
                        }

                        // tssid
                        if (tssidDictionary.Key != "-")
                        {
                            string tssid = tssidDictionary.Key;
                            ituffDescription += $"{iTuffLevel}_tssid_{tssid}\n";
                        }

                        // Prints to ituff if there is msunit or tssid.
                        if (ituffDescription != string.Empty)
                        {
                            Services.DatalogService.WriteToItuff(ituffDescription);
                        } */
                    }
                }
            }
        }

        /// <summary>
        /// Function that creates a dictionary with the Field values in ituff format.
        /// </summary>
        /// <param name="ituffDatabase"> The local database we want to add the field.</param>
        /// <param name="ituffField"> The field we want to concatenate.</param>
        /// /// <param name="postFix"> EDC or FC postfix for Ituff Format.</param>
        internal static void AddToItuff(Dictionary<string, Dictionary<string, Dictionary<string, string>>> ituffDatabase, Field ituffField, string postFix = "")
        {
            if (ituffField.CheckSetParameter(ituffField.ItuffToken) || postFix != string.Empty)
            {
                string ituffToken = string.Empty;

                if (ituffField.CheckSetParameter(ituffField.ItuffToken) && postFix == string.Empty)
                {
                    ituffToken = ituffField.ItuffToken;
                }
                else
                {
                    ituffToken = postFix;
                }

                // Check if defined Tssid Token
                var tssidToken = ituffField.Hierarchy["TssidRename"];
                if (tssidToken == ituffField.Hierarchy["Pin"])
                {
                    tssidToken = "-";
                }

                // Check if Tssid Token already exists in ItuffDatabase dictionary.
                var tssidDataDictionary = new Dictionary<string, Dictionary<string, string>>();
                if (ituffDatabase.ContainsKey(tssidToken))
                {
                    tssidDataDictionary = ituffDatabase[tssidToken];
                }

                // Check if Ituff Token already exists in TssidData dictionary.
                var ituffDataDictionary = new Dictionary<string, string>();
                if (tssidDataDictionary.ContainsKey(ituffToken))
                {
                    ituffDataDictionary = tssidDataDictionary[ituffToken];
                }

                // Strgval value
                string fieldValueString;
                if (postFix == "fc")
                {
                    fieldValueString = ituffField.Path + ":" + ituffField.FieldDataRemoveRadixPrefix() + "|";
                }
                else
                {
                    fieldValueString = ituffField.FieldDataRemoveRadixPrefix() + "|";
                }

                if (ituffDataDictionary.ContainsKey("strgval"))
                {
                    // Appends the field data to this key
                    ituffDataDictionary["strgval"] = ituffDataDictionary["strgval"] + fieldValueString;
                }
                else
                {
                    // Adds the key and field data to the dictionary.
                    ituffDataDictionary.Add("strgval", fieldValueString);
                }

                // Msunit token
                if (ituffField.CheckSetParameter(ituffField.ItuffDescriptor))
                {
                    if (ituffDataDictionary.ContainsKey("msunit"))
                    {
                        // Appends the field data to this key
                        var msunit = ituffDataDictionary["msunit"];
                        if (msunit != ituffField.ItuffDescriptor)
                        {
                            Utils.PrintError($"[ERROR] Field {ituffField.Path} is trying to set {nameof(ituffField.ItuffDescriptor)} as {ituffField.ItuffDescriptor}. It was previously defined as {msunit}.");
                        }
                    }
                    else
                    {
                        // Adds the key and field data to the dictionary.
                        ituffDataDictionary.Add("msunit", ituffField.ItuffDescriptor);
                    }
                }

                // ItuffToken Key
                if (tssidDataDictionary.ContainsKey(ituffToken))
                {
                    // Appends the field data to this key
                    tssidDataDictionary[ituffToken] = ituffDataDictionary;
                }
                else
                {
                    // Adds the key and field data to the dictionary.
                    tssidDataDictionary.Add(ituffToken, ituffDataDictionary);
                }

                // Tssid Key
                if (ituffDatabase.ContainsKey(tssidToken))
                {
                    // Appends the field data to this key
                    ituffDatabase[tssidToken] = tssidDataDictionary;
                }
                else
                {
                    // Adds the key and field data to the dictionary.
                    ituffDatabase.Add(tssidToken, tssidDataDictionary);
                }
            }
            else if (ituffField.CheckSetParameter(ituffField.ItuffDescriptor))
            {
                Utils.PrintDebugError($"[WARNING] Field {ituffField.Path} contains {nameof(ituffField.ItuffDescriptor)} as {ituffField.ItuffDescriptor} when {nameof(ituffField.ItuffToken)} is not set.");
            }
        }

        /// <summary>
        /// Function to compress message and print to Ituff.
        /// </summary>
        /// <param name="message">The string to be compressed and printed.</param>
        internal static void CompressToItuff(string message)
        {
            /* const bool compressionStatus = true; */

            // Init of ituffWriter
            var ituffWriter = Services.DatalogService.GetItuffStrgvalWriter();
            ituffWriter.SetData(Prime.Base.Utilities.StringUtilities.DeflateCompress32(message));
            /* ituffWriter.SetDataCompression(compressionStatus); */
            Services.DatalogService.WriteToItuff(ituffWriter);
        }

        /// <summary>
        /// Function that updates the pivot dictionary with the update dictionary data.
        /// </summary>
        /// <param name="pivot_dict">The pivot dictionary we want to update.</param>
        /// <param name="update_dict">Dictionary with the updated data. Appends if common key exists, otherwise adds an item to pivot.</param>
        /// <returns> Returns the updated pivot dictionary.</returns>
        internal static Dictionary<string, dynamic> DictionaryUpdate(Dictionary<string, dynamic> pivot_dict, Dictionary<string, dynamic> update_dict)
        {
            foreach (string key in update_dict.Keys)
            {
                if (pivot_dict.ContainsKey(key))
                {
                    // Appends the flat dictionary, using a common key, to the pivot dictionary.
                    pivot_dict[key] = update_dict[key];
                }
                else
                {
                    // Adds the key and item value to the dictionary.
                    pivot_dict.Add(key, update_dict[key]);
                }
            }

            return pivot_dict;
        }

        /// <summary>
        /// Recursive function that merges the dictionaries.
        /// </summary>
        /// <param name="pivot_dict">The original pivot dictionary.</param>
        /// <param name="merging_dict">The dictionary we want to merge to the pivot dictionary. Has a flat dictionary structure.</param>
        /// <returns> Returns the new pivot dictionary with the merged dictionary.</returns>
        internal static Dictionary<string, dynamic> DictionaryMerge(Dictionary<string, dynamic> pivot_dict, Dictionary<string, dynamic> merging_dict)
        {
            foreach (string key in pivot_dict.Keys)
            {
                // Loops recursively, to get to a new item for pivot dictionary.
                if (merging_dict.ContainsKey(key))
                {
                    merging_dict[key] = DictionaryMerge(pivot_dict[key], merging_dict[key]);
                }
            }

            pivot_dict = DictionaryUpdate(pivot_dict, merging_dict);
            return pivot_dict;
        }
    }
}
