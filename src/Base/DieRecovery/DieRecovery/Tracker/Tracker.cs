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

namespace DieRecoveryBase
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the <see cref="Tracker" />.
    /// </summary>
    public class Tracker
    {
        /// <summary>
        /// Gets or sets the Name of the Data.
        /// </summary>
        [JsonProperty("Name")]
        [JsonRequired]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the number of bits in this structure.
        /// </summary>
        [JsonProperty("Size")]
        [JsonRequired]
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the initial value for the tracker.
        /// </summary>
        [JsonProperty("InitialValue")]
        public string InitialValue { get; set; }

        /// <summary>
        /// Gets or sets the List of linked-on-disable trackers.
        /// </summary>
        [JsonProperty("LinkDisable")]
        public List<string> LinkOnDisable { get; set; } = new List<string>();

        /// <summary>
        /// Clones current tracker.
        /// </summary>
        /// <param name="newTrackerName">New tracker name.</param>
        /// <returns>New tracker.</returns>
        public Tracker Clone(string newTrackerName)
        {
            return new Tracker
            {
                Name = newTrackerName,
                Size = this.Size,
                InitialValue = this.InitialValue == null ? null : new string(this.InitialValue.ToCharArray()),
                LinkOnDisable = new List<string>(this.LinkOnDisable),
            };
        }
    }
}
