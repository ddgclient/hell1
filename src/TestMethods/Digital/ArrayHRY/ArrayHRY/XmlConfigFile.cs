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

namespace ArrayHRY
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Defines the <see cref="XmlConfigFile" />.
    /// </summary>
    [XmlRoot("HSR_HRY_config")]
    public class XmlConfigFile
    {
        /// <summary>
        /// Gets the remote path to the schema file for this XML object.
        /// </summary>
        [XmlIgnore]
        public static readonly string SchemaPath = "~USER_CODE_DLLS_PATH/ArrayHRY_XML.xsd";

        /// <summary>
        /// Enum to hold (Y)es or (N)o.
        /// </summary>
        public enum EnumYorN
        {
            /// <summary>
            /// Yes.
            /// </summary>
            Y,

            /// <summary>
            /// No.
            /// </summary>
            N,
        }

        /// <summary>
        /// Gets or sets the ReverseCtvCaptureData field.
        /// </summary>
        [XmlElement(ElementName = "ReverseCtvCaptureData")]
        public EnumYorN ReverseCtvCaptureData { get; set; }

        /// <summary>
        /// Gets or sets the BypassGlobalPrefix field.
        /// </summary>
        [XmlElement(ElementName = "BypassGlobalPrefix")]
        public string BypassGlobalPrefix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the CtvToHryMapping field.
        /// </summary>
        [XmlArray("CtvToHryMapping")]
        [XmlArrayItem("Map")]
        public List<CtvDataHryOutputPair> CtvToHryMapping { get; set; }

        /// <summary>
        /// Gets or sets the HryPrePostMapping Name..
        /// </summary>
        [XmlElement(ElementName = "HryPrePostMapping")]
        public HryPrePostMappingBlock HryPrePostMapping { get; set; } = null;

        /// <summary>
        /// Gets or sets a list of Condition FailKeys. Optional value, defaulting to empty list..
        /// </summary>
        [XmlArray("ConditionFailKeys")]
        [XmlArrayItem("ConditionFailKey")]
        public List<ConditionFailKeysBlock> ConditionFailKeysList { get; set; } = new List<ConditionFailKeysBlock>();

        /// <summary>
        /// Gets or sets the Criterias field.
        /// </summary>
        [XmlArrayItem("Criteria")]
        public List<CriteriaBlock> Criterias { get; set; }

        /// <summary>
        /// Gets or sets the Algorithms field.
        /// </summary>
        [XmlArray("Algorithms")]
        [XmlArrayItem("Algorithm")]
        public List<AlgorithmBlock> Algorithms { get; set; }

        /// <summary>
        /// Reads the XML file and creates a new XmlConfigFile object from it.
        /// </summary>
        /// <param name="localConfigXml">Local copy of XML file to load.</param>
        /// <param name="localSchema">Local copy of XSD file to use.</param>
        /// <returns><see cref="XmlConfigFile"/>.</returns>
        public static XmlConfigFile LoadFile(string localConfigXml, string localSchema)
        {
            var readerSettings = new System.Xml.XmlReaderSettings { IgnoreComments = true };
            readerSettings.Schemas.Add(string.Empty, localSchema);
            readerSettings.ValidationType = System.Xml.ValidationType.Schema;

            XmlConfigFile parsedXmlConfig;
            using (var inputFile = System.Xml.XmlReader.Create(localConfigXml, readerSettings))
            {
                var x = new XmlSerializer(typeof(XmlConfigFile));
                parsedXmlConfig = (XmlConfigFile)x.Deserialize(inputFile);
            }

            return parsedXmlConfig;
        }

        /// <summary>
        /// Gets the value of ReverseCtvCaptureData as a bool.
        /// </summary>
        /// <returns>true if ReverseCtvCaptureData is Y.</returns>
        public bool GetReverseCtvCaptureDataAsBool()
        {
            Prime.Services.ConsoleService.PrintDebug($"XML: ReverseCtvCaptureData=[{this.ReverseCtvCaptureData}].");
            return this.ReverseCtvCaptureData == XmlConfigFile.EnumYorN.Y;
        }

        /// <summary>
        /// Parses the CtvToHryMapping to get the CTV to HRY mapping.
        /// </summary>
        /// <returns>Dictionary, Keys(char)=CTV Character, Values(char)=HRY Character.</returns>
        public Dictionary<char, char> GetCtvToHryMapping()
        {
            /* schema guarantees CtvToHryMapping is not null. */
            /* schema guarantees each ctv_data is unique and 0 or 1. */
            /* schema guarantees each hry_data is a valid char. */
            /* set defaults, schema does not guarantee both 0 and 1 are present. */
            var retval = new Dictionary<char, char>()
            {
                { '0', '0' },
                { '1', '1' },
            };
            foreach (var ctvMapping in this.CtvToHryMapping)
            {
                Prime.Services.ConsoleService.PrintDebug($"XML: CtvToHryMapping ctv_data=[{ctvMapping.CtvData}] => hry_data=[{ctvMapping.HryOutput}].");
                retval[char.Parse(ctvMapping.CtvData)] = char.Parse(ctvMapping.HryOutput);
            }

            return retval;
        }

        /// <summary>
        /// Parse the HryPrePostMapping and return the Hry to Status mapping.
        /// </summary>
        /// <param name="postRepairSymbol">[output] the character to be used to indicate that repair was successful.</param>
        /// <returns>Dictionary: Key(char):Hry Character, Value(bool):true means this Hry character represents pass, false means fail.</returns>
        public Dictionary<char, bool> GetPrePostHryMappingAndPostRepairSymbol(out char postRepairSymbol)
        {
            /* HryPrePostMapping is optional, and defaults to null, check for it before doing anything else. */
            var retval = new Dictionary<char, bool>();
            postRepairSymbol = 'R';
            if (this.HryPrePostMapping == null)
            {
                Prime.Services.ConsoleService.PrintDebug("XML: HryPrePostMapping is empty.");
                return retval;
            }

            /* schema guarantees Map is not null if HryPrePostMapping is present. */
            foreach (var pair in this.HryPrePostMapping.Map)
            {
                /* schema guarantees each hry_data is unique and a valid char. */
                /* schema guarantees each status is "pass" or "fail". */
                Prime.Services.ConsoleService.PrintDebug($"XML: HryPrePostMapping hry_data=[{pair.HryData}] => status=[{pair.Status}].");
                retval.Add(char.Parse(pair.HryData), pair.Status.ToUpper() == "PASS" ? true : false);
            }

            /* schema guarantees PostRepairSymbol is a valid char if HryPrePostMapping is present. */
            Prime.Services.ConsoleService.PrintDebug($"XML: HryPrePostMapping PostRepairSymbol=[{this.HryPrePostMapping.PostRepairSymbol.Symbol}]");
            postRepairSymbol = char.Parse(this.HryPrePostMapping.PostRepairSymbol.Symbol);

            return retval;
        }

        /// <summary>
        /// Parses the ConditionFailKeys block and gets a mapping of expected_data to hry_output for each key.
        /// </summary>
        /// <returns>Dictionary: Key(string):Condition fail key, Values(Dictionary):Map of expected_data (string) to hry_output (char).</returns>
        public Dictionary<string, Dictionary<string, char>> GetAllConditionFailKeysMapping()
        {
            var retval = new Dictionary<string, Dictionary<string, char>>();

            /*  ConditionFailKeysList is optional but defaults to an empty list so we don't have to check it first. */
            foreach (var condition in this.ConditionFailKeysList)
            {
                /* schema guarantees each fail key is unique. */
                var key = condition.Name;
                var map = new Dictionary<string, char>();
                foreach (var pair in condition.Map)
                {
                    /* schema guarantees each map contains a valid expected_data and hry_output and that expected_data is unique. */
                    Prime.Services.ConsoleService.PrintDebug($"XML: ConditionFailKeysList key=[{key}] expected_data=[{pair.CtvData}] => hry_output=[{pair.HryOutput}].");
                    map.Add(pair.CtvData, char.Parse(pair.HryOutput));
                }

                retval[key] = map;
            }

            return retval;
        }

        /// <summary>
        /// Defines the <see cref="CtvDataHryOutputPair" />.
        /// </summary>
        public class CtvDataHryOutputPair
        {
            /// <summary>
            /// Gets or sets the ctv_data field..
            /// </summary>
            [XmlAttribute(AttributeName = "ctv_data")]
            public string CtvData { get; set; }

            /// <summary>
            /// Gets or sets the hry_data field..
            /// </summary>
            [XmlAttribute(AttributeName = "hry_data")]
            public string HryOutput { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="HryPrePostMappingBlock" />.
        /// </summary>
        public class HryPrePostMappingBlock
        {
            /// <summary>
            /// Gets or sets the PostRepairSymbol field..
            /// </summary>
            [XmlElement(Type = typeof(PostRepairSymbolContainer), ElementName = "PostRepairSymbol")]
            public PostRepairSymbolContainer PostRepairSymbol { get; set; }

            /// <summary>
            /// Gets or sets the Map field.
            /// </summary>
            [XmlElement(Type = typeof(HryToStatusMap), ElementName = "Map")]
            public List<HryToStatusMap> Map { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="PostRepairSymbolContainer" />.
        /// </summary>
        public class PostRepairSymbolContainer
        {
            /// <summary>
            /// Gets or sets the symbol field..
            /// </summary>
            [XmlAttribute(AttributeName = "symbol")]
            public string Symbol { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="HryToStatusMap" />.
        /// </summary>
        public class HryToStatusMap
        {
            /// <summary>
            /// Gets or sets the hry_data field..
            /// </summary>
            [XmlAttribute(AttributeName = "hry_data")]
            public string HryData { get; set; }

            /// <summary>
            /// Gets or sets the status field..
            /// </summary>
            [XmlAttribute(AttributeName = "status")]
            public string Status { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="ConditionFailKeysBlock" />.
        /// </summary>
        public class ConditionFailKeysBlock
        {
            /// <summary>
            /// Gets or sets the name field..
            /// </summary>
            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the Map field.
            /// </summary>
            [XmlElement(Type = typeof(ConditionalFailCtvHryPairBlock), ElementName = "Map")]
            public List<ConditionalFailCtvHryPairBlock> Map { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="ConditionalFailCtvHryPairBlock" />.
        /// </summary>
        public class ConditionalFailCtvHryPairBlock
        {
            /// <summary>
            /// Gets or sets the expected_data field..
            /// </summary>
            [XmlAttribute(AttributeName = "expected_data")]
            public string CtvData { get; set; }

            /// <summary>
            /// Gets or sets the hry_output field..
            /// </summary>
            [XmlAttribute(AttributeName = "hry_output")]
            public string HryOutput { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="CriteriaBlock" />.
        /// </summary>
        public class CriteriaBlock
        {
            /// <summary>
            /// Gets or sets the hry_index field..
            /// </summary>
            [XmlAttribute(AttributeName = "hry_index")]
            public string HryIndex { get; set; }

            /// <summary>
            /// Gets or sets the pin field..
            /// </summary>
            [XmlAttribute(AttributeName = "pin")]
            public string PinName { get; set; }

            /// <summary>
            /// Gets or sets the ctv_index_range field..
            /// </summary>
            [XmlAttribute(AttributeName = "ctv_index_range")]
            public string CtvIndexRange { get; set; }

            /// <summary>
            /// Gets or sets the condition field..
            /// </summary>
            [XmlAttribute(AttributeName = "condition")]
            public string Condition { get; set; }

            /// <summary>
            /// Gets or sets the hry_output_on_condition_fail field..
            /// </summary>
            [XmlAttribute(AttributeName = "hry_output_on_condition_fail")]
            public string HryOutputOnConditionFail { get; set; }

            /// <summary>
            /// Gets or sets the bypass_global field..
            /// </summary>
            [XmlAttribute(AttributeName = "bypass_global")]
            public string BypassGlobal { get; set; }

            /// <summary>
            /// Gets the hry_index as a uint.
            /// </summary>
            /// <returns>hry_index.</returns>
            public uint GetHryIndexAsUint() => uint.Parse(this.HryIndex); /* schema guarantees hry_index is a non-negative integer */

            /// <summary>
            /// Gets the ctv_index_range as a Tuple where Item1=StartingBit, Item2=Length..
            /// </summary>
            /// <returns>ctv_index_range as a list of uint or empty if NONE.</returns>
            public Tuple<uint, int> GetCtvIndexAsStartLengthTuple() =>
                this.CtvIndexRange.ToUpper() == "NONE" ? new Tuple<uint, int>(0, 0) : Utilities.RangeToStartAndLengthTuple(this.CtvIndexRange); /* schema guarantees this is a valid range or NONE. */

            /// <summary>
            /// Gets the ctv_index_range as a list of uint.
            /// </summary>
            /// <returns>true if ctv_index_range is NONE.</returns>
            public bool IsFixedLengthMode() =>
                this.CtvIndexRange.ToUpper() == "NONE" ? true : false; /* schema guarantees this is a valid range or NONE. */

            /// <summary>
            /// Gets the hry_output_on_condition_fail as a char.
            /// </summary>
            /// <returns>hry_output_on_condition_fail.</returns>
            public char GetHryOutputOnConditionFailAsChar() => char.Parse(this.HryOutputOnConditionFail); /* schema guarantees hry_output_on_condition_fail is a valid char. */

            /// <summary>
            /// Returns the full global names by combining the bypassGlobalPrefix argument with the bypass_global field.
            /// </summary>
            /// <param name="bypassGlobalPrefix">The xml BypassGlobalPrefix field.</param>
            /// <returns>Lists of bypass globals.</returns>
            public List<string> GetFullGlobalNames(string bypassGlobalPrefix)
            {
                /* schema guarantees bypass_global is a comma deliminated string.. */
                var allGlobals = new List<string>();
                foreach (var partialBypassName in this.BypassGlobal.Split(','))
                {
                    allGlobals.Add($"{bypassGlobalPrefix}{partialBypassName}");
                }

                return allGlobals;
            }

            /// <summary>
            /// Parse the Criteria.condition into a usable form.
            /// </summary>
            /// <returns>List of <see cref="ParsedConditionContainer"/>.</returns>
            public List<ParsedConditionContainer> GetConditions()
            {
                var allConditions = new List<ParsedConditionContainer>();
                /* schema guarantees at least one valid condition. */
                foreach (var singleConditionAsString in this.Condition.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    allConditions.Add(new ParsedConditionContainer(singleConditionAsString));
                }

                return allConditions;
            }

            /// <summary>
            /// Defines the <see cref="ParsedConditionContainer" />.
            /// </summary>
            public class ParsedConditionContainer
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="ParsedConditionContainer"/> class.
                /// </summary>
                /// <param name="conditionString">String containing one condition of the form "pin,bit_range,passing_binary_value[,fail_key]".</param>
                internal ParsedConditionContainer(string conditionString)
                {
                    /* schema guarantees each condition has 3 or 4 comma separated blocks. */
                    var conditionSplitByComma = conditionString.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                    this.Pin = conditionSplitByComma[0];
                    this.CtvIndexes = Utilities.RangeToStartAndLengthTuple(conditionSplitByComma[1]);   /* schema guarantees 2nd block is valid range. */
                    this.PassingBinaryValue = conditionSplitByComma[2]; /* schema guarantees 3rd block only contains 0's and 1's. */
                    this.FailKey = conditionSplitByComma.Length == 4 ? conditionSplitByComma[3] : string.Empty;
                }

                /// <summary>
                /// Gets the Pin field.
                /// </summary>
                public string Pin { get; private set; }

                /// <summary>
                /// Gets the CtvIndexes field.
                /// </summary>
                public Tuple<uint, int> CtvIndexes { get; private set; }

                /// <summary>
                /// Gets the PassingBinaryValue field.
                /// </summary>
                public string PassingBinaryValue { get; private set; }

                /// <summary>
                /// Gets the FailKey field.
                /// </summary>
                public string FailKey { get; private set; }
            }
        }

        /// <summary>
        /// Defines the <see cref="AlgorithmBlock" />.
        /// </summary>
        public class AlgorithmBlock
        {
            /// <summary>
            /// The default pin name in the dictionary returned by GetPerPinCtvSize.
            /// </summary>
            public static readonly string DefaultPinName = "default";

            /// <summary>
            /// Gets or sets the index field..
            /// </summary>
            [XmlAttribute(AttributeName = "index")]
            public string Index { get; set; }

            /// <summary>
            /// Gets or sets the name field..
            /// </summary>
            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the pat_modify_label field..
            /// </summary>
            [XmlAttribute(AttributeName = "pat_modify_label")]
            public string PatModifyLabel { get; set; }

            /// <summary>
            /// Gets or sets the ctv_data field..
            /// </summary>
            [XmlAttribute(AttributeName = "ctv_size")]
            public string CtvSize { get; set; }

            /// <summary>
            /// Gets the Index as a uint.
            /// </summary>
            /// <returns>uint.</returns>
            public uint GetIndexAsUint() => uint.Parse(this.Index); /* schema guarantees index is a non-negative integer */

            /// <summary>
            /// Gets the Name of this algorithm (could be string.Empty).
            /// </summary>
            /// <returns>string.</returns>
            public string GetNameAsString() => this.Name; /* schema guarantees name is a string (string.emtpy is acceptable) */

            /// <summary>
            /// Gets the ctv_size as a dictionary where Key=PinName (or default) Value=size (as Uint).
            /// </summary>
            /// <returns>uint.</returns>
            public Dictionary<string, uint> GetPerPinCtvSize()
            {
                /* schema guarantees ctv_size is a non-negative integer OR a pipe separated PinName,size list. */
                var retval = new Dictionary<string, uint>();
                if (uint.TryParse(this.CtvSize, out var simple_size))
                {
                    retval[DefaultPinName] = simple_size;
                    return retval;
                }
                else
                {
                    var elements = this.CtvSize.Split('|');
                    foreach (var pinSizePair in elements)
                    {
                        var pair = pinSizePair.Split(',');
                        var pinName = pair[0].ToLower() == DefaultPinName ? DefaultPinName : pair[0];
                        var size = uint.Parse(pair[1]);
                        retval[pinName] = size;
                    }
                }

                return retval;
            }
        }
    }
}
