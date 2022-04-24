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
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Defines the <see cref="HryConfiguration" />.
    /// </summary>
    public class HryConfiguration
    {
        /// <summary>
        /// Gets the Passing HRY character for Fixed Length Mode.
        /// </summary>
        public static readonly char FixedLengthPassChar = '9';

        /// <summary>
        /// Gets the CTV character which represents a fail.
        /// </summary>
        public static readonly char CtvCharForFail = '1';

        /// <summary>
        /// Gets the CTV character which represents a pass.
        /// </summary>
        public static readonly char CtvCharForPass = '0';

        /// <summary>
        /// Initializes a new instance of the <see cref="HryConfiguration"/> class.
        /// </summary>
        /// <param name="xmlConfig">Structure holding the XML configuration file.</param>
        public HryConfiguration(XmlConfigFile xmlConfig)
        {
            Utilities.GetModuleAndIp(out var ip, out var module);
            this.IpName = ip;
            this.ModuleName = module;

            this.ErrorCount = 0;
            this.ErrorMessages = new List<string>();

            this.ReverseCtvCaptureData = xmlConfig.GetReverseCtvCaptureDataAsBool();
            this.CtvToHryMapping = xmlConfig.GetCtvToHryMapping();
            this.HryCharIsPassStatus = xmlConfig.GetPrePostHryMappingAndPostRepairSymbol(out var tempSymbol);
            this.HryCharIsPassStatus[FixedLengthPassChar] = true; /* Make sure the hard-coded pass for fixed length mode is included in the mapping. */
            this.PostRepairSymbol = tempSymbol;
            this.ConditionFailKeyMap = xmlConfig.GetAllConditionFailKeysMapping();
            var sortedFailKeys = this.ConditionFailKeyMap.Keys.ToList();
            sortedFailKeys.Sort();

            this.Criterias = this.ParseCriteria(xmlConfig.Criterias, xmlConfig.BypassGlobalPrefix, sortedFailKeys);
            this.BuildCapturePinsList();
            this.Algorithms = this.ParseAlgorithmBlock(xmlConfig.Algorithms, this.PinsToCapture);

            if (this.ErrorCount > 0)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"Encountered [{this.ErrorCount}] error(s) reading the xml.\n * {string.Join("\n * ", this.ErrorMessages)}");
            }

            // Setup the user visible helper stuff.
            this.BuildBypassGlobalList();
            this.RequireExactSizeMatch = this.Algorithms.Count > 1 || this.ReverseCtvCaptureData;
            this.TotalCaptureSizePerPin = new Dictionary<string, uint>(this.PinsToCapture.Count);
            if (this.RequireExactSizeMatch)
            {
                foreach (var pin in this.PinsToCapture)
                {
                    this.TotalCaptureSizePerPin.Add(pin, (uint)this.Algorithms.Sum(o => o.PerPinCtvSize[pin]));
                }
            }
            else
            {
                foreach (var pin in this.PinsToCapture)
                {
                    uint bitsRequired = 0;
                    foreach (var criteria in this.Criterias)
                    {
                        if (criteria.Pin == pin)
                        {
                            bitsRequired = (uint)Math.Max(bitsRequired, criteria.CtvIndexes.Item1 + Math.Abs(criteria.CtvIndexes.Item2));
                        }

                        foreach (var condition in criteria.Conditions)
                        {
                            if (condition.Pin == pin)
                            {
                                bitsRequired = (uint)Math.Max(bitsRequired, condition.CtvIndexes.Item1 + Math.Abs(condition.CtvIndexes.Item2));
                            }
                        }
                    }

                    this.TotalCaptureSizePerPin.Add(pin, bitsRequired);
                }
            }
        }

        /// <summary>
        /// Gets the list of pins to capture when executing the Plist.
        /// </summary>
        public List<string> PinsToCapture { get; private set; }

        /// <summary>
        /// Gets the list of bypass globals to be set before executing the plist.
        /// </summary>
        public List<string> BypassGlobalsToInit { get; private set; }

        /// <summary>
        /// Gets the Algorithm objects.
        /// </summary>
        public List<Algorithm> Algorithms { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the number of captures must match exactly (true)
        /// or if more captures are allowed (false).
        /// </summary>
        public bool RequireExactSizeMatch { get; private set; }

        /// <summary>
        /// Gets the map of Pin name to expected number of captures.
        /// </summary>
        public Dictionary<string, uint> TotalCaptureSizePerPin { get; private set; }

        /// <summary>
        /// Gets the list of Criterias for parsing the CTV data.
        /// </summary>
        public List<Criteria> Criterias { get; private set; }

        /// <summary>
        /// Gets the ConditionFailKeyMap.
        /// </summary>
        public Dictionary<string, Dictionary<string, char>> ConditionFailKeyMap { get; private set; }

        /// <summary>
        /// Gets the CtvToHryMapping.
        /// </summary>
        public Dictionary<char, char> CtvToHryMapping { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the captured CTV data should be reversed before parsing.
        /// </summary>
        public bool ReverseCtvCaptureData { get; private set; }

        /// <summary>
        /// Gets the PostRepairSymbol.
        /// </summary>
        public char PostRepairSymbol { get; private set; }

        /// <summary>
        /// Gets the Map to convert the HRY character to pass (true) or fail (false).
        /// </summary>
        public Dictionary<char, bool> HryCharIsPassStatus { get; private set; }

        private int ErrorCount { get; set; }

        private List<string> ErrorMessages { get; set; }

        private string IpName { get; set; }

        private string ModuleName { get; set; }

        /// <summary>
        /// Verify the minimum amount of CTV data has been captured. Throws TestMethodException if not.
        /// </summary>
        /// <param name="ctv">CTV data captured as dictionary of Keys=PinName, Values=Captured data as string.</param>
        public void VerifyCaptureData(Dictionary<string, string> ctv)
        {
            foreach (var requiredPinAndSize in this.TotalCaptureSizePerPin)
            {
                if (!ctv.ContainsKey(requiredPinAndSize.Key))
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"Captured CTV is missing Pin=[{requiredPinAndSize.Key}]. CapturedPins=[{string.Join(",", ctv.Keys)}].");
                }

                if (this.RequireExactSizeMatch)
                {
                    if (requiredPinAndSize.Value != ctv[requiredPinAndSize.Key].Length)
                    {
                        Prime.Services.ConsoleService.PrintError($"Captured data size [{ctv[requiredPinAndSize.Key].Length}] does not match the expected size [{requiredPinAndSize.Value}] for pin [{requiredPinAndSize.Key}].");
                        Prime.Services.ConsoleService.PrintError("Note that the expected size is the sum of ctv_size's from all algorithms (see config file).");
                        throw new Prime.Base.Exceptions.TestMethodException($"Captured data size [{ctv[requiredPinAndSize.Key].Length}] does not match the expected size [{requiredPinAndSize.Value}] for pin [{requiredPinAndSize.Key}].");
                    }
                }
                else
                {
                    if (requiredPinAndSize.Value > ctv[requiredPinAndSize.Key].Length)
                    {
                        Prime.Services.ConsoleService.PrintError($"Captured data size [{ctv[requiredPinAndSize.Key].Length}] does not match minimum required size [{requiredPinAndSize.Value}] for pin [{requiredPinAndSize.Key}].");
                        Prime.Services.ConsoleService.PrintError("Note that the minimum required size is the largest bit referenced. (see config file).");
                        throw new Prime.Base.Exceptions.TestMethodException($"Captured data size [{ctv[requiredPinAndSize.Key].Length}] does not match minimum required size [{requiredPinAndSize.Value}] for pin [{requiredPinAndSize.Key}].");
                    }
                }
            }
        }

        private void AddError(string msg)
        {
            this.ErrorCount++;
            this.ErrorMessages.Add(msg);
        }

        private void BuildCapturePinsList()
        {
            var tempPinSet = new HashSet<string>();
            foreach (var critera in this.Criterias)
            {
                tempPinSet.Add(critera.Pin);
                foreach (var condition in critera.Conditions)
                {
                    tempPinSet.Add(condition.Pin);
                }
            }

            this.PinsToCapture = new List<string>(tempPinSet);
            this.PinsToCapture.Sort();
        }

        private void BuildBypassGlobalList()
        {
            var bypassGlobalsSet = new HashSet<string>();
            foreach (var criteria in this.Criterias)
            {
                bypassGlobalsSet.UnionWith(criteria.BypassGlobals);
            }

            this.BypassGlobalsToInit = new List<string>(bypassGlobalsSet);
            this.BypassGlobalsToInit.Sort();
        }

        private List<Algorithm> ParseAlgorithmBlock(List<XmlConfigFile.AlgorithmBlock> xmlAlgorithms, List<string> capturePins)
        {
            // The Algorithms section is fully checked by the schema.
            var retval = new List<Algorithm>();
            uint expectedIndex = 0;
            /* schema guarantees Algorithms is not null */
            foreach (var xmlAlgorithm in xmlAlgorithms)
            {
                var algorithm = new Algorithm();
                algorithm.Index = xmlAlgorithm.GetIndexAsUint();
                algorithm.Name = xmlAlgorithm.GetNameAsString();
                if (algorithm.Index != expectedIndex)
                {
                    this.AddError($"Algorithm=[{algorithm.Name}] has a non-consecutive index=[{algorithm.Index}]. Expecting [{expectedIndex}].");
                }

                expectedIndex = algorithm.Index + 1;
                var rawPerPinSizes = xmlAlgorithm.GetPerPinCtvSize();
                algorithm.PerPinCtvSize = new Dictionary<string, uint>(capturePins.Count);
                foreach (var pin in capturePins)
                {
                    if (rawPerPinSizes.ContainsKey(pin))
                    {
                        algorithm.PerPinCtvSize[pin] = rawPerPinSizes[pin];
                    }
                    else if (rawPerPinSizes.ContainsKey(XmlConfigFile.AlgorithmBlock.DefaultPinName))
                    {
                        algorithm.PerPinCtvSize[pin] = rawPerPinSizes[XmlConfigFile.AlgorithmBlock.DefaultPinName];
                    }
                    else
                    {
                        algorithm.PerPinCtvSize[pin] = 0;
                        this.AddError($"Algorithm=[{algorithm.Name}] is missing ctv_size for pin=[{pin}] (or [{XmlConfigFile.AlgorithmBlock.DefaultPinName}]).");
                    }
                }

                retval.Add(algorithm);
                Prime.Services.ConsoleService.PrintDebug($"XML: Algorithms index=[{algorithm.Index}] name=[{algorithm.Name}] ctv_size=[{string.Join(",", algorithm.PerPinCtvSize.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}].");
            }

            return retval;
        }

        private List<Criteria> ParseCriteria(List<XmlConfigFile.CriteriaBlock> xmlCriterias, string bypassGlobalPrefix, List<string> validKeys)
        {
            var retval = new List<Criteria>();
            uint expectedHryIndex = 0;
            /* schema guarantees Criterias is not null */
            foreach (var xmlCriteriaBlock in xmlCriterias)
            {
                var criteria = new Criteria();
                criteria.HryIndex = xmlCriteriaBlock.GetHryIndexAsUint();

                /* Check that hry_index are 0 based and consecutive. (to match the evg code) */
                if (criteria.HryIndex != expectedHryIndex)
                {
                    this.AddError($"[hry_index] must be zero-based and have consecutive numbers (found [{criteria.HryIndex}], expect [{expectedHryIndex}])");
                }

                expectedHryIndex = criteria.HryIndex + 1;
                var criteriaPin = xmlCriteriaBlock.PinName; /* schema guarantees pin is a string, still need to check its a real pin and not a group. */
                if (Utilities.ResolvePinScope(this.IpName, ref criteriaPin))
                {
                    criteria.Pin = criteriaPin;
                }
                else
                {
                    criteria.Pin = xmlCriteriaBlock.PinName;
                    this.AddError($"Criterias: pin=[{xmlCriteriaBlock.PinName}] is not a valid pin (groups are not allowed).");
                }

                criteria.FixedLengthMode = xmlCriteriaBlock.IsFixedLengthMode();
                criteria.CtvIndexes = xmlCriteriaBlock.GetCtvIndexAsStartLengthTuple();
                Prime.Services.ConsoleService.PrintDebug($"XML: Criterias hry_index=[{criteria.HryIndex}] pin=[{criteria.Pin}] fixed_length_mode=[{criteria.FixedLengthMode}] ctv_indexes=[{string.Join(",", criteria.CtvIndexes)}].");

                var allXmlConditions = xmlCriteriaBlock.GetConditions();
                criteria.Conditions = new List<Condition>();
                foreach (var xmlCondition in allXmlConditions)
                {
                    var condition = new Condition(xmlCondition.Pin, xmlCondition.CtvIndexes, xmlCondition.PassingBinaryValue, xmlCondition.FailKey);
                    criteria.Conditions.Add(condition);

                    var conditionPin = condition.Pin;
                    Prime.Services.ConsoleService.PrintDebug($"XML: \tCriterias.Conditions pin=[{xmlCondition.Pin}] ctv_indexes=[{string.Join(",", xmlCondition.CtvIndexes)}] passing_binary_value=[{xmlCondition.PassingBinaryValue}] fail_key=[{xmlCondition.FailKey}].");

                    if (Utilities.ResolvePinScope(this.IpName, ref conditionPin))
                    {
                        condition.Pin = conditionPin;
                    }
                    else
                    {
                        this.AddError($"Condition: pin=[{condition.Pin}] is not a valid pin (groups are not allowed).");
                    }

                    if (condition.PassingBinaryValue.Length != Math.Abs(condition.CtvIndexes.Item2))
                    {
                        this.AddError($"Condition: PassingValue=[{condition.PassingBinaryValue}] should have [{Math.Abs(condition.CtvIndexes.Item2)}] bit(s). From hry_index=[{xmlCriteriaBlock.HryIndex}].");
                    }

                    if (!string.IsNullOrEmpty(condition.FailKey) && !validKeys.Contains(condition.FailKey))
                    {
                        this.AddError($"Condition: FailKey=[{condition.FailKey}] does not exist. From hry_index=[{xmlCriteriaBlock.HryIndex}]. Valid Keys=[{string.Join(",", validKeys)}]");
                    }
                }

                criteria.HryOutputOnConditionFail = xmlCriteriaBlock.GetHryOutputOnConditionFailAsChar();
                criteria.BypassGlobals = xmlCriteriaBlock.GetFullGlobalNames(bypassGlobalPrefix);
                for (var i = 0; i < criteria.BypassGlobals.Count; i++)
                {
                    var global = criteria.BypassGlobals[i];
                    if (Utilities.ResolveUserVar(this.IpName, this.ModuleName, ref global))
                    {
                        criteria.BypassGlobals[i] = global;
                    }
                    else
                    {
                        this.AddError($"Condition: BypassUserVar=[{global}] does not exist. From hry_index=[{xmlCriteriaBlock.HryIndex}].");
                    }
                }

                Prime.Services.ConsoleService.PrintDebug($"XML: \tCriterias hry_output_on_condition_fail=[{criteria.HryOutputOnConditionFail}] bypass_globals=[{string.Join(",", criteria.BypassGlobals)}].");
                retval.Add(criteria);
            }

            return retval;
        }

        /// <summary>
        /// Defines the <see cref="Criteria" />.
        /// </summary>
        public class Criteria
        {
            /// <summary>
            /// Gets or sets the HryIndex.
            /// </summary>
            public uint HryIndex { get; set; }

            /// <summary>
            /// Gets or sets the Pin.
            /// </summary>
            public string Pin { get; set; }

            /// <summary>
            /// Gets or sets the CtvIndexes as a Tuple of start_index and length.
            /// </summary>
            public Tuple<uint, int> CtvIndexes { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether FixedLengthMode is enabled.
            /// </summary>
            public bool FixedLengthMode { get; set; }

            /// <summary>
            /// Gets or sets the Conditions.
            /// </summary>
            public List<Condition> Conditions { get; set; }

            /// <summary>
            /// Gets or sets the hry_output_on_condition_fail.
            /// </summary>
            public char HryOutputOnConditionFail { get; set; }

            /// <summary>
            /// Gets or sets the bypass_global.
            /// </summary>
            public List<string> BypassGlobals { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="Condition" />.
        /// </summary>
        public class Condition
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Condition"/> class.
            /// </summary>
            /// <param name="pin">Pin name.</param>
            /// <param name="ctvIndexs">List of CTV locations.</param>
            /// <param name="passingValue">Passing value in binary. (order must match the ctvIndexes).</param>
            /// <param name="failKey">FailKey name.</param>
            public Condition(string pin, Tuple<uint, int> ctvIndexs, string passingValue, string failKey)
            {
                this.Pin = pin;
                this.CtvIndexes = ctvIndexs;
                this.PassingBinaryValue = passingValue;
                this.FailKey = failKey ?? string.Empty;
            }

            /// <summary>
            /// Gets or sets the Pin.
            /// </summary>
            public string Pin { get; set; }

            /// <summary>
            /// Gets or sets the CtvIndexes.
            /// </summary>
            public Tuple<uint, int> CtvIndexes { get; set; }

            /// <summary>
            /// Gets or sets the PassingValue in binary.
            /// </summary>
            public string PassingBinaryValue { get; set; }

            /// <summary>
            /// Gets or sets the FailKey.
            /// </summary>
            public string FailKey { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="Algorithm" />.
        /// </summary>
        public class Algorithm
        {
            /// <summary>
            /// Gets or sets the Index.
            /// </summary>
            public uint Index { get; set; }

            /// <summary>
            /// Gets or sets the Name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the CtvSize.
            /// </summary>
            public Dictionary<string, uint> PerPinCtvSize { get; set; }
        }
    }
}
