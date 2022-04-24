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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using DDG;
    using Prime;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;

    /// <summary>
    /// Defines the <see cref="DefeatureRule" />.
    /// </summary>
    public class DefeatureRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefeatureRule"/> class. For SharedStorage.
        /// </summary>
        public DefeatureRule()
        {
            this.Name = string.Empty;
            this.Index = new List<int>();
            this.Rules = new List<RuleContainer>();
            this.Console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefeatureRule"/> class.
        /// </summary>
        /// <param name="name">Name of the Deafeature Rule group.</param>
        /// <param name="index">Indexes input the full slice tracking to be passed to the Rules. (TODO: why is this here?).</param>
        public DefeatureRule(string name, List<int> index)
        {
            this.Name = name;
            this.Index = index;
            this.Rules = new List<RuleContainer>();
        }

        /// <summary>
        /// Holds the valid Rule Modes. Currently only ValidCombinations is supported.
        /// </summary>
        public enum RuleMode
        {
            /// <summary>ValidCombinations Mode.</summary>
            ValidCombinations,
        }

        /// <summary>
        /// Holds the valid rule Types.
        /// </summary>
        public enum RuleType
        {
            /// <summary>Fully Featured Die.</summary>
            FullyFeatured,

            /// <summary>Valid for de-feature and fusing.</summary>
            Recovery,

            /// <summary>Valid only for fusing and for creating a fusedown string .</summary>
            FuseDown,

            /// <summary>Valid only for fusing.</summary>
            FuseOnly,
        }

        /// <summary>
        /// Gets or sets the Name of the DefeatureRule Group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets this DefeatureRule index property.
        /// </summary>
        public List<int> Index { get; set; }

        /// <summary>
        /// Gets or sets the list of Rules associated with this DefeatureRule object.
        /// </summary>
        public List<RuleContainer> Rules { get; set; }

        /// <summary>
        /// Gets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel parameter.
        /// </summary>
        protected IConsoleService Console { get; }

        /// <summary>
        /// Pull the named defeature rule out of shared storage and return it.
        /// </summary>
        /// <param name="name">Name of the rule.</param>
        /// <returns>Rule object.</returns>
        public static DefeatureRule GetDefeatureRule(string name)
        {
            try
            {
                return DieRecovery.Utilities.RetrieveRule(name);
            }
            catch (Exception e)
            {
                Services.ConsoleService.PrintError($"No DieRecovery Rule=[{name}] found. Error=${e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Add a rule to this group.
        /// </summary>
        /// <param name="mode">Rule Mode=<see cref="RuleMode"/>.</param>
        /// <param name="name">Name of this rule.</param>
        /// <param name="size">Minimum number of non-defeatured slices required by this rule.</param>
        /// <param name="type">Rule Type=<see cref="RuleType"/>.</param>
        /// <param name="values">BitVector values for the rule (when RuleMode=ValidCombinations).</param>
        /// <param name="failWhen">(optional)Indicates when this rule should fail, defaults to 'false'.</param>
        public void Add(RuleMode mode, string name, int size, RuleType type, List<BitArray> values, bool failWhen = false)
        {
            var rule = new RuleContainer();
            rule.Name = name;
            rule.Mode = mode;
            rule.Size = size;
            rule.Type = type;
            rule.FailWhen = failWhen;
            this.Rules.Add(rule);

            // rule.Values = values;
            rule.Values = new List<string>();
            foreach (var value in values)
            {
                var sb = new StringBuilder();
                for (var i = 0; i < value.Length; i++)
                {
                    sb.Append(value.Get(i) ? "1" : "0");
                }

                rule.Values.Add(sb.ToString());
            }
        }

        /// <summary>
        /// Returns the passing rules for the given input vector.
        /// </summary>
        /// <param name="inputVector">BitArray input.</param>
        /// <returns>List of <see cref="Rule"/>.</returns>
        public List<Rule> GetPassingRules(BitArray inputVector)
        {
            // TODO: should use the rules.index here to slice the input vector, but that could be done by the caller too...
            var passingRules = new List<Rule>();
            foreach (var rule in this.Rules)
            {
                List<string> bitVectors = null;
                if (rule.IsPass(inputVector, ref bitVectors))
                {
                    foreach (var bitvector in bitVectors)
                    {
                        passingRules.Add(new Rule(rule.Name, bitvector, rule.Size, rule.Type, rule.Mode));
                    }
                }
            }

            if (passingRules.Count > 0)
            {
                this.Console?.PrintDebug($"RuleGroup=[{this.Name}] Input=[{inputVector.ToBinaryString()}] Passed Rule=[{passingRules[0].Name}] Size=[{passingRules[0].Size}] BitVector=[{passingRules[0].BitVector}].");
            }
            else
            {
                this.Console?.PrintDebug($"RuleGroup=[{this.Name}] Input=[{inputVector.ToBinaryString()}] Failed to find any passing rules.");
            }

            return passingRules;
        }

        /// <summary>
        /// Struct to hold the contents of a Passing Rule.
        /// This is only used for passing rule values back to the user.
        /// </summary>
        public struct Rule
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Rule"/> struct.
            /// </summary>
            /// <param name="name">Name property.</param>
            /// <param name="bitvector">BitVector property.</param>
            /// <param name="size">Size property.</param>
            /// <param name="type">Type property.</param>
            /// <param name="mode">Mode property.</param>
            public Rule(string name, string bitvector, int size, RuleType type, RuleMode mode)
            {
                this.Name = name;
                this.BitVector = bitvector;
                this.Size = size;
                this.Type = type;
                this.Mode = mode;
            }

            /// <summary>
            /// Gets the Name of this Rule. Equivalent to .xml rule.name attribute.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the BitVector associated with this rule. Equivalent to the .xml rule.bitvector field.
            /// </summary>
            public string BitVector { get; }

            /// <summary>
            /// Gets the Size of this rule. Equivalent the .xml rule.size attribute.
            /// </summary>
            public int Size { get; }

            /// <summary>
            /// Gets the Type of this field. Equivalent the .xml rule.type attribute.
            /// </summary>
            public RuleType Type { get; }

            /// <summary>
            /// Gets the Mode of this field. Equivalent the .xml rule.mode attribute.
            /// </summary>
            public RuleMode Mode { get; }
        }

        /// <summary>
        /// Defines the <see cref="RuleContainer" />.
        /// </summary>
        public class RuleContainer
        {
            /// <summary>
            /// Gets or sets this rules Mode.
            /// </summary>
            public RuleMode Mode { get; set; }

            /// <summary>
            /// Gets or sets this rules Type.
            /// </summary>
            public RuleType Type { get; set; }

            /// <summary>
            /// Gets or sets this rules Name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the minimum number of non-defeatured slices.
            /// </summary>
            public int Size { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this rule fails
            /// when it resolves to true or false. (defaults to false).
            /// </summary>
            public bool FailWhen { get; set; } = false;

            /// <summary>
            /// Gets or sets the Values for this rule.
            /// </summary>
            // TODO: can't store BitArray in shared storage, need to replace this with something less sucky.
            // public List<BitArray> Values { get; set; }
            public List<string> Values { get; set; }

            /// <summary>
            /// Executes this rule on the given SliceTracking mask. Returns true if the rule passes.
            /// </summary>
            /// <param name="input">BitArray contining the SliceTracking.</param>
            /// <param name="passingBitVectors">A pass-by-reference list of BitArrays representing the passing configurations.</param>
            /// <returns>true if the rule passes, false otherwise.</returns>
            public bool IsPass(BitArray input, ref List<string> passingBitVectors)
            {
                var matchingBitVectors = new List<string>();
                var misMatchingBitVectors = new List<string>();
                switch (this.Mode)
                {
                    case RuleMode.ValidCombinations:
                        int passingValues = 0;
                        int failingValues = 0;
                        foreach (var testVector in this.Values)
                        {
                            if (testVector.Length != input.Length)
                            {
                                throw new TestMethodException($"Mismatch between test vector and input vector lengths for Rule=[{this.Name}] TestVector=[{testVector}] InputVector=[{input.ToBinaryString()}].");
                            }

                            // My original plan was to OR the BitArrays and see if that was equal to the testVector.
                            // If there's no "1"s in the input that are not in the testVector, then it passes.
                            // But, there's no built-in way to compare two BitArray contents (.Equals compares the references,
                            // not the contents) ... so I have to loop through each element...
                            // TODO: Find a better way to compare two BitArray objects.
                            bool isMatch = true;
                            for (int i = 0; i < testVector.Length; i++)
                            {
                                if (testVector[i] == '0' && input[i])
                                {
                                    // input has a slice disabled that the testVector does not.
                                    isMatch = false;
                                    break;
                                }
                            }

                            if (isMatch)
                            {
                                // The rule is a pass if there's no 1's in the input that aren't in the test vector.
                                passingValues += 1;
                                matchingBitVectors.Add(testVector);
                            }
                            else
                            {
                                failingValues += 1;
                                misMatchingBitVectors.Add(testVector);
                            }
                        }

                        if (this.FailWhen == false)
                        {
                            passingBitVectors = matchingBitVectors;
                            return passingValues > 0;
                        }
                        else
                        {
                            passingBitVectors = misMatchingBitVectors;
                            return failingValues > 0;
                        }

                    default:
                        throw new TestMethodException($"Invalid Mode=[{this.Mode}] for Rule=[{this.Name}].");
                }
            }
        }
    }
}
