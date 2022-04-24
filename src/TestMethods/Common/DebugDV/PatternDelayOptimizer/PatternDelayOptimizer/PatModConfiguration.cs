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

namespace PatternDelayOptimizer
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines the <see cref="PatModConfiguration" />.
    /// </summary>
    public class PatModConfiguration
    {
        /// <summary>
        /// Gets or sets the Configurations.
        /// </summary>
        public List<Config> Configurations { get; set; }

        /// <summary>
        /// Creates a Prime .patmod.json contents using the input configuraiton a nd pattern status results.
        /// </summary>
        /// <param name="configuration">PatModConfiguration struct.</param>
        /// <param name="results">Struct containing the results. Key=PatMod String, Value=List of patterns.</param>
        /// <returns>Json string in prime .patmod.json format.</returns>
        internal static PatModConfiguration BuildOutput(Config configuration, Dictionary<string, List<string>> results)
        {
            var obj = new PatModConfiguration();
            obj.Configurations = new List<Config>(1);
            obj.Configurations.Add(new Config());
            obj.Configurations[0].Name = "ImpactStudiesOptimumWaits"; // TODO: Hardcode output patmod name or let the user pass it in?
            obj.Configurations[0].ConfigurationElement = new List<ConfigElement>(configuration.ConfigurationElement.Count);

            foreach (var searchItem in results.OrderBy(o => o.Key))
            {
                var patterns = searchItem.Value;
                var patmods = new List<string>(searchItem.Key.Split('|'));
                foreach (var element in configuration.ConfigurationElement)
                {
                    var patmodElement = new ConfigElement();
                    patmodElement.Type = "INSTRUCTION";
                    patmodElement.Domain = element.Domain;
                    patmodElement.StartAddress = element.StartAddress;
                    patmodElement.StartAddressOffset = element.StartAddressOffset;
                    patmodElement.EndAddress = element.EndAddress;
                    patmodElement.EndAddressOffset = element.EndAddressOffset;
                    patmodElement.PatternsRegEx = patterns.Select(s => $"^{s}$").ToList();
                    patmodElement.Data = patmods[0];
                    obj.Configurations[0].ConfigurationElement.Add(patmodElement);

                    if (patmods.Count > 1)
                    {
                        patmods.RemoveAt(0);
                    }
                }
            }

            return obj;
        }

        /// <summary>
        /// Defines the <see cref="Config" />.
        /// </summary>
        public class Config
        {
            /// <summary>
            /// Gets or sets the Name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the ConfigurationElement.
            /// </summary>
            public List<ConfigElement> ConfigurationElement { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="ConfigElement" />.
        /// </summary>
        public class ConfigElement
        {
            /// <summary>
            /// Gets or sets the Type.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Gets or sets the Domain.
            /// </summary>
            public string Domain { get; set; }

            /// <summary>
            /// Gets or sets the StartAddress.
            /// </summary>
            public string StartAddress { get; set; }

            /// <summary>
            /// Gets or sets the StartAddressOffset.
            /// </summary>
            public int StartAddressOffset { get; set; } = 0;

            /// <summary>
            /// Gets or sets the EndAddress.
            /// </summary>
            public string EndAddress { get; set; }

            /// <summary>
            /// Gets or sets the EndAddressOffset.
            /// </summary>
            public int EndAddressOffset { get; set; } = 0;

            /// <summary>
            /// Gets or sets the PatternsRegEx.
            /// </summary>
            public List<string> PatternsRegEx { get; set; }

            /// <summary>
            /// Gets the ValidationMode.
            /// </summary>
            public string ValidationMode { get; } = "ALLOW_LABEL_NO_MATCHING";

            /// <summary>
            /// Gets or sets the Data.
            /// </summary>
            public string Data { get; set; }
        }
    }
}
