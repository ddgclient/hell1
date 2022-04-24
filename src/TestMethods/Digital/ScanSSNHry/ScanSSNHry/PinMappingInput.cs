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

namespace ScanSSNHry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Class that represents pinMappingInputFile Json input file structure.
    /// </summary>
    public class PinMappingInput
    {
        /// <summary>
        /// Gets or sets the PinNumbers from the json file.
        /// </summary>
        [JsonProperty("pinNumbers")]
        public ulong PinNumbers { get; set; }

        /// <summary>
        /// Gets or sets the pinsMapping from the json file.
        /// </summary>
        [JsonProperty("pinsMapping")]
        public List<PinMapping> PinsMapping { get; set; }

        /// <summary>
        /// Class that represents pinMapping section in Json input file.
        /// </summary>
        public class PinMapping
        {
            /// <summary>
            /// Gets or sets the ssn_datapth  from the json file.
            /// </summary>
            [JsonProperty("ssn_datapth")]
            public int Ssn_datapth { get; set; }

            /// <summary>
            /// Gets or sets the pinName from the json file.
            /// </summary>
            [JsonProperty("pinName")]
            public string PinName { get; set; }

            /// <inheritdoc/>
            public override string ToString()
            {
                return this.PinName;
            }
        }
    }
}