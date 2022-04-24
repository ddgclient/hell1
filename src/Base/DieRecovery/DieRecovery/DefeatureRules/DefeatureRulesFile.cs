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
    using System.Xml.Serialization;

    /// <summary>
    /// Defines the <see cref="DefeatureRulesFile" />.
    /// </summary>
    [XmlRoot(ElementName = "recovery")]
    public class DefeatureRulesFile
    {
        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        [XmlElement("defeaturing_rules")]
        public List<DefeatureRulesContainer> DefeatureRules { get; set; }

        /// <summary>
        /// Defines the <see cref="DefeatureRulesContainer" />.
        /// </summary>
        public class DefeatureRulesContainer
        {
            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            [XmlElement("rules")]
            public List<RulesContainer> Rules { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="RulesContainer" />.
        /// </summary>
        public class RulesContainer
        {
            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            [XmlAttribute("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            [XmlAttribute("index")]
            public string Index { get; set; }

            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            [XmlElement("rule")]
            public List<Rule> Rules { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="Rule" />.
        /// </summary>
        public class Rule
        {
            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            [XmlAttribute("mode")]
            public string Mode { get; set; }

            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            [XmlAttribute("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            [XmlAttribute("type")]
            public string Type { get; set; }

            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            [XmlAttribute("size")]
            public string Size { get; set; }

            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            [XmlAttribute("fail_when")]
            public string FailWhen { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            [XmlElement("bitvector")]
            public List<BitVector> BitVectors { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="BitVector" />.
        /// </summary>
        public class BitVector
        {
            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            [XmlAttribute("value")]
            public string Value { get; set; }
        }
    }
}
