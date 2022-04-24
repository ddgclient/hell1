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

namespace FunctionalShopsTC
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Test class used for Pin configuration parsing.
    /// </summary>
    public class PinConfig
    {
        /// <summary>
        /// Gets or sets VOX_LL.
        /// </summary>
        [JsonProperty("VOX_LL")]
        public double VOX_LL { get; set; }

        /// <summary>
        /// Gets or sets VOX_UL.
        /// </summary>
        [JsonProperty("VOX_UL")]
        public double VOX_UL { get; set; }

        /// <summary>
        /// Gets or sets search resolution.
        /// </summary>
        [JsonProperty("Resolution")]
        public double Resolution { get; set; }

        /// <summary>
        /// Gets or sets class that represents PinConfigs Json input file structure.
        /// </summary>
        [JsonProperty("PinConfigs")]
        public List<PinConfiguration> PinConfigs { get; set; }

        /// <summary>
        /// Gets or sets PinConfiguration.
        /// </summary>
        public class PinConfiguration
        {
            /// <summary>
            /// Gets or sets PinName.
            /// </summary>
            [JsonProperty("PinName")]
            public string PinName { get; set; }

            /// <summary>
            /// Gets or sets VOX_UL.
            /// </summary>
            [JsonProperty("VOX")]
            public double VOX { get; set; }
        }
    }
}
