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

namespace AnalogFuncCaptureCtv
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This class is intended to store all Ituff Printing functions.
    /// </summary>
    internal static class ItuffFormat
    {
        /// <summary>
        /// Function that creates a dictionary with the Field values in ituff format.
        /// </summary>
        /// <param name="ituffDatabase"> The local database we want to add the field.</param>
        /// <param name="ituffField"> The field we want to concatenate.</param>
        /// /// <param name="postFix"> EDC or FC postfix for Ituff Format.</param>
        internal static void AddToItuff(Dictionary<string, Dictionary<string, string>> ituffDatabase, Field ituffField, string postFix)
        {
            if (ituffField.CheckSettedParam(ituffField.ItuffToken))
            {
                // string token_key = ituffField.ItuffToken + "_" + postFix;
                // JF PTH edit: removing fc or edc in the end
                string token_key = ituffField.ItuffToken;
                string dieName = ituffField.Path.Substring(0, ituffField.Path.IndexOf("."));
                string fieldString;
                if (postFix == "fc")
                {
                    fieldString = ituffField.Path + ":" + ituffField.FieldStrData + "|";
                }
                else
                {
                    fieldString = ituffField.FieldStrData + "|";
                }

                if (ituffDatabase.ContainsKey(dieName))
                {
                    Dictionary<string, string> tokenDict = ituffDatabase[dieName];
                    if (tokenDict.ContainsKey(token_key))
                    {
                        // Appends the field data to this key
                        tokenDict[token_key] = tokenDict[token_key] + fieldString;
                    }
                    else
                    {
                        // Adds the key and field data to the dictionary.
                        tokenDict.Add(token_key, fieldString);
                    }
                }
                else
                {
                    Dictionary<string, string> tokenDict = new Dictionary<string, string> { { token_key, fieldString } };
                    ituffDatabase.Add(dieName, tokenDict);
                }
            }
        }

        /// <summary>
        /// Function that prints a dictionary in ituff format.
        /// </summary>
        /// <param name="ituffDatabase"> The local database to print to ituff. </param>
        internal static void PrintToItuff(Dictionary<string, Dictionary<string, string>> ituffDatabase)
        {
            if (ituffDatabase.Count != 0)
            {
            // Init of ituffWriter and Delimiter
            var ituffWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            ituffWriter.SetDelimiterCharacterForWrap('|');

            // Prints for each Die in Database that contains tokens.
            foreach (KeyValuePair<string, Dictionary<string, string>> die_dict in ituffDatabase)
            {
                // Gets the token dictionary for each die.
                foreach (KeyValuePair<string, string> token_dict in die_dict.Value)
                {
                    // Sets the tname with die_dict.key = pin = TDO
                    // string netname = "_" + die_dict.Key + "_" + token_dict.Key;
                    // JF PTH edit: modify the code to just print the ItuffToken
                    string netname = "_" + token_dict.Key;

                    ituffWriter.SetTnamePostfix(netname);

                    // Sets the strgval
                    string data = token_dict.Value.Remove(token_dict.Value.Length - 1, 1); // Removes last pipe
                    ituffWriter.SetData(data);

                    // Prints to Ituff
                    Prime.Services.DatalogService.WriteToItuff(ituffWriter);
                }
            }
        }
    }

        /// <summary>
        /// Function to compress message and print to Ituff.
        /// </summary>
        /// <param name="message">The string to be compressed and printed.</param>
        [ExcludeFromCodeCoverage]
        internal static void CompressToItuff(string message)
        {
            /* const bool compressionStatus = true; */

            // Init of ituffWriter
            var ituffWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            ituffWriter.SetData(Prime.Base.Utilities.StringUtilities.DeflateCompress32(message));
            /* ituffWriter.SetDataCompression(compressionStatus); */
            Prime.Services.DatalogService.WriteToItuff(ituffWriter);
        }
    }
}
