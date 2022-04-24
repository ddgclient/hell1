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

namespace DDG
{
    using System.Collections.Generic;
    using CommandLine;

    /// <summary>
    /// Defines the <see cref="VoltageConverterOptions" />.
    /// </summary>
    public class VoltageConverterOptions
    {
        /// <summary>
        /// Gets or sets the fivr condition name.
        /// </summary>
        [Option("fivrcondition", Required = false, HelpText = "FIVR condition name. Defined in ALEPH configuration file.")]
        public string FivrCondition { get; set; }

        /// <summary>
        /// Gets or sets the list of overrides using specific syntax.
        /// </summary>
        [Option("overrides", Required = false, HelpText = "Voltage overrides.Format: domain:value,...,domainN:valueN")]
        public string Overrides { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of rail configurations.
        /// </summary>
        [Option("railconfigurations", Required = false, HelpText = "List of rail configurations to set.")]
        public IEnumerable<string> RailConfigurations { get; set; }

        /// <summary>
        /// Gets or sets the list of dlvr pins.
        /// </summary>
        [Option("dlvrpins", Required = false, HelpText = "List of dlvr pins to set.")]
        public IEnumerable<string> DlvrPins { get; set; }

        /// <summary>
        /// Gets or sets the list of override dlvr expressions.
        /// </summary>
        [Option("expressions", Required = false, HelpText = "List of dlvr override expressions. Number of items has to match number of dlvrpins.")]
        public IEnumerable<string> OverrideExpressions { get; set; }
    }
}
