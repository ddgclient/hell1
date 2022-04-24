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

namespace PatternDelayOptimizer
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.PatConfigService;

    /// <summary>
    /// Internal class to hold the results for all testpoints for a single pattern.
    /// </summary>
    internal class PatternContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatternContainer"/> class.
        /// </summary>
        /// <param name="pattern">Name of the pattern.</param>
        /// <param name="patConfigName">PatConfig name.</param>
        /// <param name="minSearchValue">Minimum search value.</param>
        /// <param name="maxSearchValueOverride">Maximum search value (overrides whats in the pattern if >= 0).</param>
        /// <param name="resolution">Resolution of the search.</param>
        /// <param name="configElements">PatMod Configuration Elements to read the pattern to determine patmod format and max value.</param>
        /// <param name="console">Either Prime.Services.ConsoleService or null.</param>
        internal PatternContainer(string pattern, string patConfigName, int minSearchValue, int maxSearchValueOverride, int resolution, List<PatModConfiguration.ConfigElement> configElements, IConsoleService console)
        {
            this.Console = console;
            this.Pattern = pattern;
            this.Enabled = true;
            this.FoundResult = false;
            this.CurrentPatMod = string.Empty;
            this.AllTestPointStatus = new List<TestPointStatus>();
            this.BinarySearchLowerValue = minSearchValue;
            this.MinimumSearchValue = minSearchValue;
            this.Resolution = resolution;
            this.PatModTemplate = new List<string>(configElements.Count);
            this.DomainMultiplier = new List<int>(configElements.Count);
            this.Valid = true;

            List<int> maxPerDomain = new List<int>(configElements.Count);
            this.Console?.PrintDebug($"Building container for pattern=[{pattern}].");
            foreach (var element in configElements)
            {
                // find the address for the label - TODO: why is it so hard to get the address for a label?
                var label_address = this.GetAddressForLabel(pattern, element.Domain, element.StartAddress);
                if (label_address < 0)
                {
                    this.Console?.PrintDebug($"No Label=[{element.StartAddress}] for Domain=[{element.Domain}]. Marking pattern as invalid.");
                    this.Valid = false;
                    return;
                }

                var address = label_address + element.StartAddressOffset;
                this.Console?.PrintDebug($"Found Label=[{element.StartAddress}] for Domain=[{element.Domain}] at Vec=[{label_address}] + offset=[{element.StartAddressOffset}] => final=[{address}].");

                // Read the instruction vector.
                var instructionTuple = Prime.Services.PatternService.ReadInstruction(this.Pattern, element.Domain, (uint)address);
                this.Console?.PrintDebug($"Read Instruction Opcode=[{instructionTuple.Item1}] Operand=[{instructionTuple.Item2}].");

                // extract the base count and format.
                if (instructionTuple.Item1 == "RPT")
                {
                    this.Console?.PrintDebug($"\tIts a Repeat, using MaxCount=[{instructionTuple.Item2}].");
                    this.PatModTemplate.Add("RPT %COUNT%");
                    maxPerDomain.Add(int.Parse(instructionTuple.Item2));
                }
                else if (instructionTuple.Item1 == "MOV")
                {
                    var opcodes = instructionTuple.Item2.Split(',').ToList().Select(o => o.Trim()).ToList();
                    this.Console?.PrintDebug($"\tIts a MOV, using MaxCount=[{opcodes[0]}].");
                    this.PatModTemplate.Add($"MOV %COUNT%, {opcodes[1]}");
                    maxPerDomain.Add(System.Convert.ToInt32(opcodes[0], 16));
                }
                else
                {
                    this.Console?.PrintDebug($"Expecting MOV or RPT instruction, got [{instructionTuple.Item1}] at Vec=[{address}] Domain=[{element.Domain}] Pattern=[{pattern}]. Marking pattern as invalid.");
                    this.Valid = false;
                    return;
                }
            }

            // Get the patconfig handle once we've determined ths is a valid pattern.
            this.PatConfigHandle = Prime.Services.PatConfigService.GetPatConfigHandle(patConfigName, $"^{pattern}$");

            // now figure out the domain multipliers
            this.BinarySearchUpperValue = maxPerDomain[0];
            this.OriginalRepeatValue = maxPerDomain[0];
            this.MaximumSearchValue = System.Math.Max(maxSearchValueOverride, this.OriginalRepeatValue);
            for (var i = 0; i < maxPerDomain.Count; i++)
            {
                var maxValue = maxPerDomain[i];
                if (maxValue % maxPerDomain[0] != 0)
                {
                    throw new TestMethodException($"Expecting all wait times to be integer multipliers of the first domain. BaseDomain[{configElements[0].Domain}]=[{maxPerDomain[0]}] TargetDomain[{configElements[i].Domain}]=[{maxValue}].");
                }
                else
                {
                    this.DomainMultiplier.Add(maxValue / maxPerDomain[0]);
                }
            }
        }

        /// <summary>
        /// Gets the name of this pattern.
        /// </summary>
        internal string Pattern { get; private set; }

        /// <summary>
        /// Gets the PatConfigHandle specific for this pattern.
        /// </summary>
        internal IPatConfigHandle PatConfigHandle { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this pattern is currently enabled.
        /// </summary>
        internal bool Enabled { get; set; }

        /// <summary>
        /// Gets the list of all the TestpointStatus objects for this pattern.
        /// </summary>
        internal List<TestPointStatus> AllTestPointStatus { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this pattern has completed testing or not.
        /// </summary>
        internal bool FoundResult { get; private set; }

        /// <summary>
        /// Gets the current (final if FoundResult is set) patmod associated with this pattern.
        /// </summary>
        internal string CurrentPatMod { get; private set; }

        /// <summary>
        /// Gets the current LowerValue when in binary search mode.
        /// </summary>
        internal int BinarySearchLowerValue { get; private set; }

        /// <summary>
        /// Gets the absolute minimum wait time to search.
        /// </summary>
        internal int MinimumSearchValue { get; private set; }

        /// <summary>
        /// Gets the absolute maximum wait time to search.
        /// </summary>
        internal int MaximumSearchValue { get; private set; }

        /// <summary>
        /// Gets the patterns original wait time.
        /// </summary>
        internal int OriginalRepeatValue { get; private set; }

        /// <summary>
        /// Gets the current UpperValue when in binary search mode.
        /// </summary>
        internal int BinarySearchUpperValue { get; private set; }

        /// <summary>
        /// Gets the Current search value.
        /// </summary>
        internal int CurrentSearchValue { get; private set; }

        /// <summary>
        /// Gets the Resolution.
        /// </summary>
        internal int Resolution { get; private set; }

        /// <summary>
        /// Gets the template to use for the PatMod Data.
        /// </summary>
        internal List<string> PatModTemplate { get; private set; }

        /// <summary>
        /// Gets the multiplier for each domain wait time.
        /// </summary>
        internal List<int> DomainMultiplier { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this pattern is valid or not.
        /// </summary>
        internal bool Valid { get; private set; }

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        /// <summary>
        /// Reset the pattern to an untested state.
        /// </summary>
        /// <param name="startAtMin">If True, will be reset to Minimum wait time, if false it will be set to the maximum wait time.</param>
        internal void ResetForInitialSearch(bool startAtMin)
        {
            this.Enabled = true;
            this.FoundResult = false;
            this.BinarySearchLowerValue = this.MinimumSearchValue;
            this.BinarySearchUpperValue = this.MaximumSearchValue;
            this.CurrentSearchValue = startAtMin ? this.MinimumSearchValue : this.MaximumSearchValue;

            this.CurrentPatMod = this.BuildPatMod(this.CurrentSearchValue);
            this.PatConfigHandle.SetData(this.CurrentPatMod);
            this.AllTestPointStatus = new List<TestPointStatus>();
        }

        /// <summary>
        /// Update the patmod for the next search or returns true if search is completed.
        /// </summary>
        /// <param name="failingPatterns">List of patterns which failed the current execution.</param>
        /// <param name="ambleFail">Flag indicating if this is an amble failure or not.</param>
        /// <param name="binarySearch">True if running a binary search, false otherwise.</param>
        /// <param name="lowToHighSearch">True if running a linear search from low to high wait times, false otherwise.</param>
        /// <returns>True if the search has completed for this pattern.</returns>
        internal bool ReadResultsAndUpdateForNextTestPoint(HashSet<string> failingPatterns, bool ambleFail, bool binarySearch, bool lowToHighSearch)
        {
            var patternPassed = !failingPatterns.Contains(this.Pattern) && !ambleFail;
            this.AllTestPointStatus.Add(new TestPointStatus(this.CurrentPatMod, this.CurrentSearchValue, patternPassed));

            if (binarySearch)
            {
                return this.UpdateNextTestPointForBinarySearch(patternPassed);
            }
            else
            {
                return this.UpdateNextTestPointForLinearSeach(patternPassed, lowToHighSearch);
            }
        }

        /// <summary>
        /// Updates the PatMod to the best passing configuration and adds in the guardband.
        /// </summary>
        /// <param name="guardband">Multiplies the final value by (1 + guardband).</param>
        internal void SetToFinalValue(double guardband)
        {
            if (this.FoundResult && !string.IsNullOrEmpty(this.CurrentPatMod))
            {
                this.CurrentSearchValue = (int)(this.CurrentSearchValue * (1 + guardband));
                this.Console?.PrintDebug($"SetFinalValue using Result+Guardband=[{this.CurrentSearchValue}]");
                this.CurrentPatMod = this.BuildPatMod(this.CurrentSearchValue);
                this.PatConfigHandle.SetData(this.CurrentPatMod);
                return;
            }

            var lowestPassingTestPoint = this.AllTestPointStatus.OrderBy(testpoint => testpoint.Value).FirstOrDefault(o => o.Passed);
            if (lowestPassingTestPoint != null)
            {
                this.CurrentSearchValue = (int)(lowestPassingTestPoint.Value * (1 + guardband));
                this.Console?.PrintDebug($"SetFinalValue using SmallestPassing+Guardband=[{this.CurrentSearchValue}]");
                this.CurrentPatMod = this.BuildPatMod(this.CurrentSearchValue);
                this.PatConfigHandle.SetData(this.CurrentPatMod);
                this.FoundResult = true;
            }
            else
            {
                this.Console?.PrintDebug($"SetFinalValue has no passing value to use.");
                this.CurrentPatMod = string.Empty;
            }
        }

        /// <summary>
        /// Restores the pattern to its original condition.
        /// </summary>
        internal void RestorePattern()
        {
            this.Console?.PrintDebug($"Restoring the counter for pattern=[{this.Pattern}] to [{this.OriginalRepeatValue}].");
            var patmod = this.BuildPatMod(this.OriginalRepeatValue);
            this.PatConfigHandle.SetData(patmod);
        }

        /// <summary>
        /// Gets a summary of all the testpoints as a string.
        /// </summary>
        /// <returns>string.</returns>
        internal string GetResultsAsString()
        {
            return string.Join("|", this.AllTestPointStatus.Select(s => $"{s.Value}:{s.Passed}"));
        }

        private int GetAddressForLabel(string patName, string domainName, string targetLabelName)
        {
            if (!targetLabelName.StartsWith("^"))
            {
                targetLabelName = "^" + targetLabelName;
            }

            if (!targetLabelName.EndsWith("$"))
            {
                targetLabelName = targetLabelName + "$";
            }

            Regex rgx = new Regex(targetLabelName);

            try
            {
                uint location = 0;
                while (true)
                {
                    var label = Prime.Services.PatternService.GetLabelFromAddress(patName, domainName, location, false);
                    /* if (label.GetName() == targetLabelName) */
                    if (rgx.IsMatch(label.GetName()))
                    {
                        return (int)label.GetAddress();
                    }
                    else
                    {
                        location = label.GetAddress() + 1;
                    }
                }
            }
            catch (FatalException)
            {
                this.Console?.PrintDebug($"[PrimeError] Cannot find Label=[{targetLabelName}] in Domain=[{domainName}] Pattern=[{patName}].");
                return -1;
            }
            catch (System.Exception)
            {
                this.Console?.PrintDebug($"[SystemError] Cannot find Label=[{targetLabelName}] in Domain=[{domainName}] Pattern=[{patName}].");
                return -1;
            }
        }

        private string BuildPatMod(int currentValue)
        {
            List<string> patModPerDomain = new List<string>(this.PatModTemplate.Count);
            for (var i = 0; i < this.PatModTemplate.Count; i++)
            {
                patModPerDomain.Add(this.PatModTemplate[i].Replace("%COUNT%", (this.DomainMultiplier[i] * currentValue).ToString()));
            }

            return string.Join("|", patModPerDomain);
        }

        private bool UpdateNextTestPointForBinarySearch(bool patternPassed)
        {
            if (patternPassed)
            {
                if (this.CurrentSearchValue == this.MinimumSearchValue || (this.CurrentSearchValue - this.BinarySearchLowerValue) <= this.Resolution)
                {
                    // found the lowest passing testpoint for this pattern.
                    this.Enabled = false;
                    this.FoundResult = true;
                    this.Console?.PrintDebug($"Found Final Result=[{this.CurrentSearchValue}] for Pattern=[{this.Pattern}]");
                    return true;
                }
                else
                {
                    this.BinarySearchUpperValue = this.CurrentSearchValue;
                    this.SetTestPointToValue((int)((this.BinarySearchUpperValue - this.BinarySearchLowerValue) / 2) + this.BinarySearchLowerValue);
                    this.Console?.PrintDebug($"Found Passing result=[{this.BinarySearchUpperValue}], next testpoint=[{this.CurrentSearchValue}] for Pattern=[{this.Pattern}]");
                    return false;
                }
            }
            else
            {
                if (this.CurrentSearchValue == this.MaximumSearchValue)
                {
                    // no higher point to check, pattern is invalid.
                    this.SetPatternInvalid();
                    this.Console?.PrintDebug($"Max TestPoint=[{this.MaximumSearchValue}] failed for Pattern=[{this.Pattern}]");
                    return true;
                }
                else if (this.CurrentSearchValue == this.MinimumSearchValue)
                {
                    // 1st testpoint is always the StartingValue, 2nd is always the EndingValue.
                    this.SetTestPointToValue(this.MaximumSearchValue);
                    this.Console?.PrintDebug($"Found Failing result=[{this.MinimumSearchValue}], next testpoint=[{this.CurrentSearchValue}] for Pattern=[{this.Pattern}]");
                    return false;
                }
                else if ((this.BinarySearchUpperValue - this.CurrentSearchValue) <= this.Resolution)
                {
                    // this point failed but we've reached the resolution, use the last passing count.
                    var lastPassing = this.AllTestPointStatus.FindLast(o => o.Passed);
                    this.Console?.PrintDebug($"Found Failing result=[{this.CurrentSearchValue}], using [{lastPassing.Value}] as final result for Pattern=[{this.Pattern}]");
                    this.SetTestPointToValue(lastPassing.Value);
                    this.Enabled = false;
                    this.FoundResult = true;
                    return true;
                }
                else
                {
                    this.BinarySearchLowerValue = this.CurrentSearchValue;
                    this.SetTestPointToValue((int)((this.BinarySearchUpperValue - this.BinarySearchLowerValue) / 2) + this.BinarySearchLowerValue);
                    this.Console?.PrintDebug($"Found Failing result=[{this.BinarySearchLowerValue}], next testpoint=[{this.CurrentSearchValue}] for Pattern=[{this.Pattern}]");
                    return false;
                }
            }
        }

        private bool UpdateNextTestPointForLinearSeach(bool patternPassed, bool searchLowToHigh)
        {
            var doneWithSearch = false;
            var currentResultString = patternPassed ? "Passing" : "Failing";
            var searchDirection = searchLowToHigh ? 1 : -1;
            var nextValue = this.CurrentSearchValue + (searchDirection * this.Resolution);
            var searchComplete = (searchLowToHigh && patternPassed) || (!searchLowToHigh && !patternPassed) || (!searchLowToHigh && nextValue < this.MinimumSearchValue);
            var usePreviousValue = !searchLowToHigh && !patternPassed;

            if ((!searchComplete && nextValue > this.MaximumSearchValue) || (!patternPassed && this.CurrentSearchValue == this.MaximumSearchValue))
            {
                // max testpoint failed.
                this.SetPatternInvalid();
                this.Console?.PrintDebug($"Max TestPoint=[{this.CurrentSearchValue}] failed for Pattern=[{this.Pattern}]");
                doneWithSearch = true;
            }
            else if (!searchComplete)
            {
                // search isn't done, move to the next testpoint.
                this.Console?.PrintDebug($"Found {currentResultString} result=[{this.CurrentSearchValue}], next testpoint=[{nextValue}] for Pattern=[{this.Pattern}]");
                this.SetTestPointToValue(nextValue);
                doneWithSearch = false;
            }
            else if (usePreviousValue)
            {
                // done with search, but need to use the previous value.
                var lastPassing = this.AllTestPointStatus.FindLast(o => o.Passed);
                nextValue = lastPassing.Value;
                this.Console?.PrintDebug($"Found {currentResultString} result=[{this.CurrentSearchValue}], using this as the final result for Pattern=[{this.Pattern}]");
                this.Enabled = false;
                this.FoundResult = true;
                this.SetTestPointToValue(nextValue);
                doneWithSearch = true;
            }
            else
            {
                // done with search and the final value is correct.
                this.Console?.PrintDebug($"Found {currentResultString} result=[{this.CurrentSearchValue}], using this as the final result for Pattern=[{this.Pattern}]");
                this.Enabled = false;
                this.FoundResult = true;
                doneWithSearch = true;
            }

            return doneWithSearch;
        }

        private void SetTestPointToValue(int nextValue)
        {
            this.CurrentSearchValue = nextValue;
            this.CurrentPatMod = this.BuildPatMod(this.CurrentSearchValue);
            this.PatConfigHandle.SetData(this.CurrentPatMod);
        }

        private void SetPatternInvalid()
        {
            this.Enabled = false;
            this.FoundResult = true;
            this.CurrentPatMod = string.Empty;
        }

        /// <summary>
        /// Internal class to hold a single result for a single pattern.
        /// </summary>
        internal class TestPointStatus
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestPointStatus"/> class.
            /// </summary>
            /// <param name="patmod">The PatConfig Data associated with this testpoint.</param>
            /// <param name="value">The searchpoint value.</param>
            /// <param name="testPassed">True if the testpoint passed.</param>
            internal TestPointStatus(string patmod, int value, bool testPassed)
            {
                this.Value = value;
                this.Passed = testPassed;
                this.PatConfigData = patmod;
            }

            /// <summary>
            /// Gets the SearchPoint value.
            /// </summary>
            internal int Value { get; private set; }

            /// <summary>
            /// Gets a value indicating whether this point passed or not.
            /// </summary>
            internal bool Passed { get; private set; }

            /// <summary>
            /// Gets the data for the patconfig associated with this point.
            /// </summary>
            internal string PatConfigData { get; private set; }
        }
    }
}
