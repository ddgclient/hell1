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
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary> Recovery JSON Parser. </summary>
    public class MappingJsonParser
    {
        /// <summary>Enum of allowed types SharedStorage or DFF. </summary>
        public enum TPTypes
        {
            /// <summary>SharedStorage.</summary>
            SharedStorage,

            /// <summary>DFF.</summary>
            DFF,

            /// <summary>ITUFF.</summary>
            ITUFF,
        }

        /// <summary>Enum of allowed types Fields. </summary>
        public enum Fields
        {
            /// <summary>VMIN.</summary>
            Vmin,

            /// <summary>Recovery.</summary>
            Recovery,

            /// <summary>DFF.</summary>
            Bisr_Chains,

            /// <summary>DFF.</summary>
            Hry,
        }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("Version")]
        public double Version { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("MULTIDIE", Required = Required.Default)]
        public Dictionary<string, MultiDie> DieToPin { get; set; } = new Dictionary<string, MultiDie>();

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("MULTICORE", Required = Required.Default)]
        public Dictionary<string, string> IpToPin { get; set; } = new Dictionary<string, string>();

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("HRY", Required = Required.DisallowNull)]
        public Dictionary<string, Mappings> Hry { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("BISR_CHAINS", Required = Required.DisallowNull)]
        public Dictionary<string, Mappings> Bisr_Chains { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("Recovery", Required = Required.Default)]
        public Mappings Recovery { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("Vmin", Required = Required.Default)]
        public Mappings Vmin { get; set; }

        /// <summary> Returns Value needed to write to for sharedstorage/dff.</summary>
        /// <param name = "nametype" > The name to grab.</param>
        /// <param name = "name" > Name of the HRY type KS/HRY.</param>
        /// <returns>String value to run.</returns>
        public string TokenHRYName(TPTypes nametype, string name)
        {
            return this.Hry[name].GetTPValue(nametype);
        }

        /// <summary> Returns Value needed to write bisr to for sharedstorage/dff.</summary>
        /// <param name = "nametype" > The name to grab.</param>
        /// <param name = "name" > Name of the HRY type KS/HRY.</param>
        /// <returns>String value to run.</returns>
        public string TokenBISRName(TPTypes nametype, string name)
        {
            return this.Bisr_Chains[name].GetTPValue(nametype);
        }

        /// <summary> Multidie Parser. </summary>
        public class MultiDie
        {
            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("VOLTAGE", Required = Required.DisallowNull)]
            public Dictionary<string, string> Voltages { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("SHORTNAME", Required = Required.DisallowNull)]
            public string Shortname { get; set; } = string.Empty;
        }

        /// <summary> Mappings Parser. </summary>
        public class Mappings
        {
            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("SHAREDSTORAGE", Required = Required.DisallowNull)]
            public string SharedStorage { get; set; } = string.Empty;

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("DFF", Required = Required.DisallowNull)]
            public string Dff { get; set; } = string.Empty;

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("ITUFF", Required = Required.DisallowNull)]
            public string Ituff { get; set; } = string.Empty;

            /// <summary> Returns Value needed to write to for sharedstorage/dff.</summary>
            /// <param name = "nametype" > The name to grab.</param>
            /// <returns>String value to run.</returns>
            public string GetTPValue(TPTypes nametype)
            {
                if (nametype == TPTypes.DFF)
                {
                    if (string.IsNullOrEmpty(this.Dff))
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return this.Dff;
                    }
                }
                else if (nametype == TPTypes.SharedStorage)
                {
                    if (string.IsNullOrEmpty(this.SharedStorage))
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return this.SharedStorage;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(this.Ituff))
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return this.Ituff;
                    }
                }
            }
        }
    }
}
