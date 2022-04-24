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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;

    /// <summary>
    /// Recovery JSON Parser.
    /// </summary>
    public class RecoveryJsonParser
    {
        /// <summary>Gets or sets jSON Element.</summary>
        [JsonProperty("Version", Required = Required.Always)]
        public double Version { get; set;  }

        /// <summary>Gets or sets jSON Element.</summary>
        [JsonProperty("Hry_string", Required = Required.Default)]
        public List<string> Hry_string { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("RecoveryToken")]
        public List<string> RecoveryToken { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("ValidOptions", Required = Required.DisallowNull)]
        public List<string> ValidOptions { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("RecoveryPriority", Required = Required.DisallowNull)]
        public List<string> RecoveryPriority { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("ErrorValue")]
        public char ErrorValue { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty(Required = Required.DisallowNull)]
        public List<int> TotalForAllmodesbyIndex { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty(Required = Required.DisallowNull)]
        public List<string> TotalForAllmodesbymem { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("FailValue", Order = 4)]
        public char FailValue { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("PassValue", Order = 5)]
        public char PassValue { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("DesignIps", Order = 6)]
        public Dictionary<string, IPs> DesignIPs { get; set; }

        /// <summary>Clears recovery token.</summary>
        /// <returns> Returns list of all passing characters.</returns>
        public List<char> InitializeRecovery()
        {
            var temp = new string(this.PassValue, this.RecoveryToken.Count());
            return temp.ToList();
        }

        /// <summary>Builds the recovery option and populates the variables in the class.</summary>
        /// <param name = "hrylocation" > HryNames per location in .</param>
        /// <returns> Returns True if the recovery flow runs dupliactes.</returns>
        public bool BuildIPsRecovery(List<string> hrylocation)
        {
            this.TotalForAllmodesbyIndex = new List<int>();
            this.TotalForAllmodesbymem = new List<string>();
            bool duplicates = false;
            foreach (KeyValuePair<string, RecoveryJsonParser.IPs> listmatches in this.DesignIPs)
            {
                var duplicaterun = listmatches.Value.BuildRecovery(hrylocation, listmatches.Key);
                if (duplicaterun == true)
                {
                    duplicates = duplicaterun;
                }

                this.TotalForAllmodesbyIndex.AddRange(listmatches.Value.Extractallidxs());
                this.TotalForAllmodesbymem.AddRange(listmatches.Value.Extractallmems());
            }

            return duplicates;
        }

        /// <summary>Gets or sets JSON Element.</summary>
        [Serializable]
        public class IPs
        {
            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("Type")]
            public string RecoveryType { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("Required")]
            public List<string> RequiredForAllmodes { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty(Required = Required.DisallowNull)]
            public List<int> RequiredForAllmodesbyIndex { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty(Required = Required.DisallowNull)]
            public List<string> RequiredForAllmodesbymem { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("RecoveryOptions")]
            public Dictionary<string, List<string>> RecoveryOption { get; set; }

            /// <summary>Gets or sets Contains a list of controllers by the names found in the hrystring.</summary>
            [JsonProperty(Required = Required.DisallowNull)]
            public Dictionary<string, List<string>> SkusbyName { get; set; }

            /// <summary>Gets or sets Contains a list of controllers by the index.</summary>
            [JsonProperty(Required = Required.DisallowNull)]
            public Dictionary<string, List<int>> SkusbyIndex { get; set; }

            /// <summary>Builds the recovery option and populates the variables in the class.</summary>
            /// <param name = "hrylocation" > HryNames per location in .</param>
            /// <param name = "iprunning" > Name of the IP running to allow the print statement to be clear.</param>
            /// <returns> Returns True if the recovery flow runs dupliactes.</returns>
            public bool BuildRecovery(List<string> hrylocation, string iprunning)
            {
                bool duplicates = false;
                this.SkusbyName = new Dictionary<string, List<string>>();
                this.SkusbyIndex = new Dictionary<string, List<int>>();
                if (this.RequiredForAllmodesbyIndex == null)
                {
                    this.RequiredForAllmodesbyIndex = new List<int>();
                    this.RequiredForAllmodesbymem = new List<string>();
                }

                var capture = new List<string>();
                foreach (var name in this.RequiredForAllmodes)
                {
                    capture = hrylocation.Where(x => Regex.IsMatch(x, name, RegexOptions.IgnoreCase) == true).ToList();

                    foreach (var mem in capture)
                    {
                        this.RequiredForAllmodesbymem.Add(mem);
                        this.RequiredForAllmodesbyIndex.Add(hrylocation.IndexOf(mem));
                    }
                }

                foreach (KeyValuePair<string, List<string>> listmatches in this.RecoveryOption)
                {
                    var capcontrollers = new List<string>();
                    foreach (var name in listmatches.Value)
                    {
                        capcontrollers.AddRange(hrylocation.Where(x => Regex.IsMatch(x, name, RegexOptions.IgnoreCase) == true).ToList());
                    }

                    var query = capcontrollers.GroupBy(x => x)
                            .Where(g => g.Count() > 1)
                            .Select(y => y.Key)
                            .ToList();

                    if (query.Count > 0)
                    {
                        Prime.Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Recovery Config has duplicates for [{iprunning}],option [{listmatches.Key}] with duplicates on [{query}].");
                        duplicates = true;
                    }

                    this.SkusbyName.Add(listmatches.Key.ToString(), capcontrollers);
                    var indexes = new List<int>();
                    foreach (var name in capcontrollers)
                    {
                        indexes.Add(hrylocation.IndexOf(name));
                    }

                    this.SkusbyIndex.Add(listmatches.Key.ToString(), indexes);

                    foreach (var mem in capture)
                    {
                        if (capcontrollers.Contains(mem))
                        {
                            duplicates = true;
                        }
                    }
                }

                return duplicates;
            }

            /// <summary>Extract all memories by name.</summary>
            /// <returns> Returns List of memories used for this recovery option.</returns>
            public List<string> Extractallmems()
            {
                var totallist = new List<string>();
                foreach (KeyValuePair<string, List<string>> option in this.SkusbyName)
                {
                    totallist.AddRange(option.Value);
                }

                totallist.AddRange(this.RequiredForAllmodesbymem);
                return totallist;
            }

            /// <summary>Extract all memories by idx.</summary>
            /// <returns> Returns List of memories used for this recovery option.</returns>
            public List<int> Extractallidxs()
            {
                var totallist = new List<int>();
                foreach (KeyValuePair<string, List<int>> option in this.SkusbyIndex)
                {
                    totallist.AddRange(option.Value);
                }

                totallist.AddRange(this.RequiredForAllmodesbyIndex);

                return totallist;
            }
        }
    }
}
