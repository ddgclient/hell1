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

namespace PTHGetSetGsdsDffTC
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// PTHGetSetGsdsDffTC configuration.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Gets or sets list ALL GSDS variables.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public List<string> GSDSList { get; set; }

        /// <summary>
        /// Gets or sets the Sope.Type name for GSDS.
        /// </summary>
        [JsonProperty]
        public string GSDSScopeType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the DFF name.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string DFF { get; set; }

        /// <summary>
        /// Gets or sets the OPType name for DFF.
        /// </summary>
        [JsonProperty]
        public string DFFOpType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the operation is to convert all the GSDS to DFF.
        /// </summary>
        [JsonProperty]
        public bool GSDS2DFF { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the operation is to convert all the DFF to GSDS.
        /// </summary>
        [JsonProperty]
        public bool DFF2GSDS { get; set; } = false;

        /// <summary>
        /// Gets or sets temperature set point. It can read double or user var value..
        /// </summary>
        [JsonProperty]
        public string Delimiter { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the operation is to convert all the GSDS to DFF.
        /// </summary>
        [JsonProperty]
        public bool PrintDFF { get; set; } = false;

        /// <summary>
        /// Gets or sets list ALL GSDS variables.
        /// </summary>
        [JsonProperty]
        public List<string> GSDS2DFFAllowedList { get; set; } = null;

        /// <summary>
        /// Gets or sets a dictionary of Search/Replace pairs.
        /// </summary>
        [JsonProperty]
        public Dictionary<string, string> SearchReplace { get; set; } = null;
    }
}