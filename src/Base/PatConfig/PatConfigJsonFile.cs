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

namespace Prime.TestMethods.PatConfig
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using static Prime.TestMethods.PatConfig.PatConfig;

    /// <summary>
    /// This class represents the data structure of the PinData json input file.
    /// </summary>
    [Serializable]
    public class PatConfigJsonFile
    {
        /// <summary>
        /// Gets or sets SetPoints list.
        /// </summary>
        [JsonProperty("SetPoints", Required = Required.Always)]
        public List<SetPoint> SetPoints { get; set; }

        /// <summary>
        /// This class represents each SetPoint structure contained in the configuration file.
        /// </summary>
        [Serializable]
        public class SetPoint
        {
            /// <summary>
            /// Gets or sets SetPoint name.
            /// </summary>
            [JsonProperty("Name", Required = Required.Always)]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets Configuration list.
            /// </summary>
            [JsonProperty("Configurations", Required = Required.Always)]
            public List<Configuration> Configurations { get; set; }

            /// <summary>
            /// This class represents each Configuration structure contained in the configuration file.
            /// </summary>
            [Serializable]
            public class Configuration
            {
                /// <summary>
                /// Gets or sets Configuration name.
                /// </summary>
                [JsonProperty("Configuration", Required = Required.Always)]
                public string Name { get; set; }

                /// <summary>
                /// Gets or sets Configuration Element.
                /// </summary>
                [JsonProperty("ConfigurationElement")]
                public string SubConfigElement { get; set; } = string.Empty;

                /// <summary>
                /// Gets or sets a value indicating whether gets or sets a value for InPKGApplyToPKGOnly.
                /// </summary>
                [JsonProperty("InPKGApplyToPKGOnly")]
                public bool InPkgApplyToPkgOnly { get; set; } = false;

                /// <summary>
                /// Gets or sets a value indicating whether in the configuration is going to be stored.
                /// </summary>
                [JsonProperty("ToBeStored")]
                public bool ToBeStored { get; set; } = false;

                /// <summary>
                /// Gets or sets data source.
                /// </summary>
                public DataSource Source { get; set; } = DataSource.Raw;

                /// <summary>
                /// Gets or sets Configuration data.
                /// </summary>
                [JsonProperty("Data")]
                public string Data { get; set; } = string.Empty;

                /// <summary>
                /// Sets configuration data.
                /// </summary>
                [JsonProperty("UserVar")]
                private string UserVar
                {
                    set
                    {
                        this.Data = value;
                        this.Source = DataSource.UserVar;
                    }
                }

                /// <summary>
                /// Sets Configuration data.
                /// </summary>
                [JsonProperty("SharedStorage")]
                private string SharedStorage
                {
                    set
                    {
                        this.Data = value;
                        this.Source = DataSource.SharedStorage;
                    }
                }

                /// <summary>
                /// Sets Configuration data.
                /// </summary>
                [JsonProperty("DFF")]
                private string DFF
                {
                    set
                    {
                        this.Data = value;
                        this.Source = DataSource.DFF;
                    }
                }
            }
        }
    }
}
