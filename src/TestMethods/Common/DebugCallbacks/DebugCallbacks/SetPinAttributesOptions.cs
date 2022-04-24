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

namespace DebugCallbacks
{
    using System.Collections.Generic;
    using CommandLine;

    /// <summary>
    /// Defines SetPinAttributesOptions.
    /// </summary>
    public class SetPinAttributesOptions
    {
        /// <summary>
        /// Gets or sets pause before executing code.
        /// </summary>
        [Option("prepause", Required = false, HelpText = "Sleep before executing code. In miliseconds.")]
        public double PrePause { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets pause after executing code.
        /// </summary>
        [Option("postpause", Required = false, HelpText = "Sleep after executing code. In miliseconds.")]
        public double PostPause { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets values to be set.
        /// </summary>
        [Option("settings", Required = true, HelpText = "<pin1>:<attribute1>:<value1> <pinN>:<attributeN>:<valueN>")]
        public IEnumerable<string> Settings { get; set; }
    }
}
