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

namespace ExitPortFromGsds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Stores GSDS token name and exit ports to use.
    /// </summary>
    public class JsonData
    {
        /// <summary>
        /// Gets or sets list of pairs of GSDS name and value.
        /// </summary>
        [JsonProperty("ExitPorts")]
        public Dictionary<string, int> ExitPorts { get; set; }

        /// <summary>
        /// Gets or sets GSDS name containing ExitPorts key.
        /// </summary>
        [JsonProperty("GSDSName")]
        public string GsdsName { get; set; }

        /// <summary>
        /// Gets or sets UserVar name containing ExitPorts key.
        /// </summary>
        [JsonProperty("UserVarName")]
        public string UserVarName { get; set; }
    }
}
