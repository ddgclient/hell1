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

namespace DfxTimingTuner
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Defines the <see cref="ConfigurationFile" />.
    /// </summary>
    [XmlRoot("DfxTimingTuner")]
    public class ConfigurationFile
    {
        /// <summary>
        /// Gets or sets a list of PinGroups.
        /// </summary>
        [XmlElement(Type = typeof(PinGroup), ElementName = "pingroup")]
        public List<PinGroup> PinGroups { get; set; }

        /// <summary>
        /// Gets or sets a list of ConfigSet.
        /// </summary>
        [XmlElement(Type = typeof(ConfigSet), ElementName = "set")]
        public List<ConfigSet> ConfigSets { get; set; }

        /// <summary>
        /// Load the configuration file.
        /// </summary>
        /// <param name="cfgFile">Name of the configuration file to load.</param>
        /// <returns>Configuration object <see cref="ConfigurationFile"/>.</returns>
        public static ConfigurationFile LoadFile(string cfgFile)
        {
            var configFile = DDG.FileUtilities.GetFile(cfgFile);
            var readerSettings = new System.Xml.XmlReaderSettings { IgnoreComments = true };
            using (var inputFile = System.Xml.XmlReader.Create(configFile, readerSettings))
            {
                var x = new XmlSerializer(typeof(ConfigurationFile));
                var configuration = (ConfigurationFile)x.Deserialize(inputFile);
                return configuration;
            }
        }

        /// <summary>
        /// Class to hold pingroups.
        /// </summary>
        public class PinGroup
        {
            /// <summary>
            /// Gets or sets the name of the pingroup.
            /// </summary>
            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the list of pins in this group.
            /// </summary>
            [XmlElement(Type = typeof(PinObject), ElementName = "pin")]
            public List<PinObject> Pins { get; set; }
        }

        /// <summary>
        /// Class to hold a configuration.
        /// </summary>
        public class ConfigSet
        {
            /// <summary>
            /// Gets or sets the Configuration Name.
            /// </summary>
            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the Search mode.
            /// </summary>
            [XmlElement(ElementName = "search_mode")]
            public Globals.EdgeMode SearchType { get; set; }

            /// <summary>
            /// Gets or sets the name of the pingroup to search.
            /// </summary>
            [XmlElement(ElementName = "search_pins")]
            public string SearchPinGroup { get; set; }

            /// <summary>
            /// Gets or sets the name of the pingroup to capture.
            /// </summary>
            [XmlElement(ElementName = "capture_pins")]
            public string CapturePinGroup { get; set; }

            /// <summary>
            /// Gets or sets the name of the pingroup to decode CTV data (Drive Mode only).
            /// </summary>
            [XmlElement(ElementName = "capture_ctvorder")]
            public string CtvPinGroup { get; set; }

            /// <summary>
            /// Gets or sets the uservar regex.
            /// </summary>
            [XmlElement(ElementName = "uservar")]
            public string UserVarRegex { get; set; }

            /// <summary>
            /// Gets or sets the PatternModify information for the Loop size in a TOSTrigger pattern.
            /// </summary>
            [XmlElement(Type = typeof(PatModObject), ElementName = "loop_size")]
            public PatModObject LoopControl { get; set; }

            /// <summary>
            /// Gets or sets the HDMT PinGroup to use for the adjust api call.
            /// </summary>
            [XmlElement(ElementName = "pingroup_for_adjust")]
            public string PinGroupForAdjust { get; set; } = string.Empty;
        }

        /// <summary>
        /// Class to hold a Pin. (name and alias).
        /// </summary>
        public class PinObject
        {
            /// <summary>
            /// Gets or sets the Alias Name.
            /// </summary>
            [XmlAttribute(AttributeName = "alias")]
            public string Alias { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the Pin Name.
            /// </summary>
            [XmlText]
            public string Name { get; set; }
        }

        /// <summary>
        /// Class to hold the information for the PatternMods for TOSTrigger mode (loop size).
        /// </summary>
        public class PatModObject
        {
            /// <summary>
            /// Gets or sets the Alias Name.
            /// </summary>
            [XmlAttribute(AttributeName = "config")]
            public string PatConfig { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the Pin Name.
            /// </summary>
            [XmlText]
            public string Data { get; set; }
        }
    }
}
