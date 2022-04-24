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

namespace DTSBase
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// DTS decode configuration.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Gets or sets the configuration name.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DTS decoding is enabled for the entire program.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether DTS decoding is enabled for the entire program.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool LastPattern { get; set; } = true;

        /// <summary>
        /// Gets or sets captured pin name.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string PinName { get; set; }

        /// <summary>
        /// Gets or sets list ALL DTS sensors.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public List<string> SensorsList { get; set; }

        /// <summary>
        /// Gets or sets sensor register size (number of bits).
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public int RegisterSize { get; set; }

        /// <summary>
        /// Gets or sets the slope for DTS decoding.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public double Slope { get; set; }

        /// <summary>
        /// Gets or sets the offset for DTS decoding.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public double Offset { get; set; }

        /// <summary>
        /// Gets or sets list of ignored DTS sensors.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public List<string> IgnoredSensorsList { get; set; }

        /// <summary>
        /// Gets or sets temperature set point. It can read double or user var value..
        /// </summary>
        [JsonProperty]
        public string SetPoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets upper tolerance limit to fail for worst sensor.
        /// </summary>
        [JsonProperty]
        public string UpperTolerance { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets lower tolerance limit to fail for worst sensor.
        /// </summary>
        [JsonProperty]
        public string LowerTolerance { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether stats for sensor values should be printed to ituff.
        /// </summary>
        [JsonProperty]
        public bool DatalogValues { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether DEFLATE32 compressed format should be printed to ituff.
        /// </summary>
        [JsonProperty]
        public bool CompressedDatalog { get; set; } = false;
    }
}