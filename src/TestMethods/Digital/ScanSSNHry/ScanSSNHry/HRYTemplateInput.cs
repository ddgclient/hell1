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
    /// Class that represents HRYTemplateInputFile. Json input file structure.
    /// </summary>
    public class HRYTemplateInput
    {
        /// <summary>
        /// Gets or sets the HRYLength  from the json file.
        /// </summary>
        [JsonProperty("HRYLength")]
        public int HryLength { get; set; }

        /// <summary>
        /// Gets or sets the Patterns list objects from the json file.
        /// </summary>
        [JsonProperty("Patterns")]
        public List<Pattern> Patterns { get; set; }

        /// <summary>
        /// Class that represents Pattern object section in Json input file.
        /// </summary>
        public class Pattern
        {
            /// <summary>
            /// Gets or sets the type of content.
            /// </summary>
            [JsonProperty("ContentType")]
            public string ContentType { get; set; }

            /// <summary>
            /// Gets or sets PatternRegex.
            /// </summary>
            [JsonProperty("PatternRegex")]
            public string PatternRegex { get; set; }

            /// <summary>
            /// Gets or sets the partition Packets.
            /// </summary>
            [JsonProperty("Packets")]
            public List<Packet> Packets { get; set; }

            /// <summary>
            /// Gets or sets the PacketSize  from the json file.
            /// </summary>
            [JsonProperty("PacketSize")]
            public ulong PacketSize { get; set; }

            /// <summary>
            /// Gets or sets the OutputPacketOffset value  from the json file.
            /// </summary>
            [JsonProperty("OutputPacketOffset")]
            public ulong OutputPacketOffset { get; set; }

            /// <inheritdoc/>
            public override string ToString()
            {
                return this.PatternRegex;
            }

            /// <summary>
            /// Class that represents Packet section in Json input file.
            /// </summary>
            public class Packet
            {
                /// <summary>
                /// Gets or sets SSNPacketBitPosition per partition.
                /// </summary>
                [JsonProperty("SSNPacketBitPosition")]
                public string SSNPacketBitPosition { get; set; }

                /// <summary>
                /// Gets or sets Partitions.
                /// </summary>
                [JsonProperty("Partitions")]
                public List<Partition> Partitions { get; set; }

                /// <inheritdoc/>
                public override string ToString()
                {
                    return this.SSNPacketBitPosition;
                }

                /// <summary>
                /// Class that represents Partition section in Json input file.
                /// </summary>
                public class Partition
                {
                    /// <summary>
                    /// Gets or sets PinRegex.
                    /// </summary>
                    [JsonProperty("HRYIndex")]
                    public int HRYIndex { get; set; }

                    /// <summary>
                    /// Gets or sets HRYPrint.
                    /// </summary>
                    [JsonProperty("HRYPrint")]
                    public string HRYPrint { get; set; }
                }
            }
        }
    }
}
