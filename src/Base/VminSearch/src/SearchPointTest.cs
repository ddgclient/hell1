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

namespace Prime.TestMethods.VminSearch
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.TestMethods.VminSearch.Helpers;

    /// <summary>
    /// Class to provide all behaviour to a search point object.
    /// </summary>
    internal class SearchPointTest
    {
        /// <summary>
        /// Value assigned to failing voltages.
        /// </summary>
        internal const double VoltageFailValue = -9999.0;

        /// <summary>
        /// Value assigned to masked targets.
        /// </summary>
        internal const double VoltageMaskValue = -8888.0;

        private const int DoubleRoundingDecimals = 3;

        private readonly IConsoleService console;
        private readonly int targetsCount;
        private readonly double stepSize;
        private readonly IVminSearchExtensions vminSearchExtensions;
        private readonly bool isRecoveryMaskEnabled;
        private readonly bool isStartOnFirstFailEnabled;
        private readonly bool isSinglePointMode;
        private readonly bool isCheckOfResultBitsEnabled;
        private readonly bool isHighToLowSearch;
        private readonly RepeatedVoltageTargetHandler repeatedVoltageHandler;

        private bool isOvershootEnabled;
        private BitArray plistResultBits;
        private List<string> startVoltageKeys;
        private List<string> endVoltageLimitKeys;
        private List<string> lowerStartVoltageKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchPointTest"/> class.
        /// </summary>
        /// <param name="plistExecutionParameters">Values to create the functional object for plist execution.</param>
        /// <param name="stepSize">Voltage value to increase if conditions are not met for search completion.</param>
        /// <param name="targets">List of voltage targets, used for validation purposes.</param>
        /// <param name="extensions"> Object of class which has all extension methods implemented.</param>
        /// <param name="featureSwitches">List of switches used to enable or disable specific features.</param>
        /// <param name="console">IConsoleService reference.</param>
        public SearchPointTest(
            PrimeVminSearchTestMethod.PlistExecutionParameters plistExecutionParameters,
            double stepSize,
            IReadOnlyList<string> targets,
            IVminSearchExtensions extensions,
            ICollection<string> featureSwitches,
            IConsoleService console)
        {
            this.CurrentState = new SearchStateValues();
            this.stepSize = stepSize;
            this.targetsCount = targets.Count;
            this.CurrentState.Voltages = new List<double>(this.targetsCount);
            this.vminSearchExtensions = extensions;
            this.SearchPlist = extensions.GetFunctionalTest(plistExecutionParameters.Patlist, plistExecutionParameters.LevelsTc, plistExecutionParameters.TimingsTc, plistExecutionParameters.PrePlist);
            this.isRecoveryMaskEnabled = !featureSwitches.Contains("recovery_mask_off");
            this.isHighToLowSearch = featureSwitches.Contains("high_to_low_search");
            this.isStartOnFirstFailEnabled = false;
            if (!featureSwitches.Contains("start_on_first_fail_off") && this.SearchPlist is ICaptureFailureTest test)
            {
                test.EnableStartPatternOnFirstFail();
                this.isStartOnFirstFailEnabled = true;
            }

            this.CurrentState.MaskBits = new BitArray(this.targetsCount, false);
            this.isSinglePointMode = extensions.IsSinglePointMode();
            this.isCheckOfResultBitsEnabled = extensions.IsCheckOfResultBitsEnabled();
            this.repeatedVoltageHandler = new RepeatedVoltageTargetHandler(targets);
            this.console = console;
        }

        /// <summary>
        /// Gets object which contains all current state search values.
        /// </summary>
        public SearchStateValues CurrentState { get; }

        /// <summary>
        /// Gets the functional test object with plist execution settings.
        /// </summary>
        public IFunctionalTest SearchPlist { get; }

        /// <summary>
        /// Sets startVoltageKeys.
        /// </summary>
        public List<string> StartVoltageKeys
        {
            private get => this.startVoltageKeys;
            set
            {
                if (!value.Count.Equals(1) && this.targetsCount != value.Count)
                {
                    throw new Base.Exceptions.TestMethodException("Start voltages count does not match target count");
                }

                this.startVoltageKeys = value;
            }
        }

        /// <summary>
        /// Sets endVoltageLimitKeys.
        /// </summary>
        public List<string> EndVoltageLimitKeys
        {
            private get => this.endVoltageLimitKeys;
            set
            {
                if (!value.Count.Equals(1) && this.targetsCount != value.Count)
                {
                    throw new Base.Exceptions.TestMethodException("End voltage limits count does not match target count");
                }

                this.endVoltageLimitKeys = value;
            }
        }

        /// <summary>
        /// Sets lowerStartVoltageKeys.
        /// </summary>
        public List<string> LowerStartVoltageKeys
        {
            private get => this.lowerStartVoltageKeys;
            set
            {
                if (value.Any())
                {
                    if (!value.Count.Equals(1) && this.targetsCount != value.Count)
                    {
                        throw new Base.Exceptions.TestMethodException("Lower start voltages count does not match the target count");
                    }

                    this.lowerStartVoltageKeys = value;
                    this.isOvershootEnabled = true;
                }
            }
        }

        /// <summary>
        /// Resolves plist for execution. If PUP methodology is enabled it might replace the original plist with the slim plist version.
        /// This is done in the functional service.
        /// </summary>
        /// <param name="instanceName">currently executed instance name to check if it is targeted for PUP.</param>
        /// <returns>Returns resolved plist.</returns>
        public string ResolvePlist(string instanceName)
        {
            return this.SearchPlist.ResolvePlist(instanceName);
        }

        /// <summary>
        /// Resetting of all dynamic fields if applicable.
        /// </summary>
        /// <returns>Returns if reset was successful.</returns>
        public bool Reset()
        {
            this.CurrentState.PerTargetIncrements = Enumerable.Repeat(0U, this.targetsCount).ToList();
            this.CurrentState.PerPointData.Clear();
            this.CurrentState.FailReason = string.Empty;

            if (this.SearchPlist is ICaptureFailureTest test)
            {
                test.Reset();
            }

            this.StartVoltageSetup();
            this.EndVoltageSetup();
            if (!this.AreVoltageValuesValid())
            {
                this.DisableAllTargets();
                this.SetPointData();
                return false;
            }

            this.CurrentState.Voltages = new List<double>(this.CurrentState.StartVoltages);
            if (this.repeatedVoltageHandler.AreRepeatedTargets)
            {
                this.CurrentState.Voltages = this.repeatedVoltageHandler.UpdateRepeatedVoltageTargets(this.CurrentState.Voltages);
            }

            this.InitialMaskSetup();
            if (this.CurrentState.MaskBits.AndAll())
            {
                this.console?.PrintError($"VminSearchSP: There are no enabled bits in mask=[{this.CurrentState.MaskBits.ToStr()}], this search is not executed");
                this.DisableAllTargets();
                this.SetPointData();
                this.CurrentState.FailReason = "InvalidInitialMask";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Execute Search Plist Test Point.
        /// </summary>
        /// <returns>The status of the plist execution.</returns>
        public bool Execute()
        {
            this.CurrentState.ExecutionCount++;
            this.console?.PrintDebug($"VminSearchSP: Search point plist execution iteration#=[{this.CurrentState.ExecutionCount}]\n" +
                                     $"VminSearchSP: Search point mask values=[{this.CurrentState.MaskBits.ToStr()}]");
            this.vminSearchExtensions.ApplyMask(this.CurrentState.MaskBits, this.SearchPlist);
            return this.SearchPlist.Execute();
        }

        /// <summary>
        /// ProcessResults.
        /// </summary>
        /// <param name="plistExecutionStatus">The status of the current search point.</param>
        /// <returns>Represents a pass/fail result, pass=true only if all bits from PrecessPlistResults call are false.</returns>
        public bool ProcessResults(bool plistExecutionStatus)
        {
            this.SetPointData();
            var resultsBits = this.vminSearchExtensions.ProcessPlistResults(plistExecutionStatus, this.SearchPlist);
            if (this.IsResultBitsInvalid(plistExecutionStatus, resultsBits))
            {
                this.console?.PrintDebug("VminSearchSP: Replacing result from ProcessPlistResults extension by all fail bits due to invalid condition");
                resultsBits = new BitArray(this.targetsCount, true);
            }

            this.console?.PrintDebug($"VminSearchSP: PList result decoded bits=[{resultsBits.ToStr()}].");
            this.plistResultBits = resultsBits;

            return !resultsBits.OrAll();
        }

        /// <summary>
        /// Returns boolean field that is true only when the search algorithm has determined search is completed.
        /// </summary>
        /// <param name="isProcessResultsPass">The status of the current search point.</param>
        /// <returns>isSearchCompleted.</returns>
        public bool IsSearchCompleted(bool isProcessResultsPass)
        {
            if (isProcessResultsPass)
            {
                this.console?.PrintDebug("VminSearchSP: Search completed with PASS result.");
                if (this.CurrentState.ExecutionCount == 1 && this.isOvershootEnabled)
                {
                    this.console?.PrintDebug("VminSearchSP: Search passed at first try. Lowering start voltages to redo the search.");
                    this.Reset();
                    return false;
                }
            }
            else
            {
                this.DefineVoltagesForNextSearchPoint();
                if (this.isSinglePointMode || this.IsRecoveryNotPossible())
                {
                    this.console?.PrintDebug("VminSearchSP: Search completed with FAIL result.");
                    this.DisableAllTargets();
                    if (this.SearchPlist is ICaptureFailureTest test)
                    {
                        test.DatalogFailure(1);
                    }
                }
                else
                {
                    return false;
                }
            }

            this.console?.PrintDebug($"VminSearchSP: Search result voltage(s)=[{string.Join(",", this.CurrentState.Voltages)}].");
            return true;
        }

        /// <summary>
        /// Applies the test conditions of the related to the plist execution.
        /// </summary>
        public void ApplyTestConditions()
        {
            this.SearchPlist.ApplyTestConditions();
        }

        private bool IsResultBitsInvalid(bool plistExecutionStatus, BitArray resultsBits)
        {
            if (resultsBits.Length != this.targetsCount)
            {
                this.console?.PrintDebug($"VminSearchSP: Invalid result bits=[{resultsBits.ToStr()}], does not match expected size=[{this.targetsCount}]");
                return true;
            }

            if (!this.isCheckOfResultBitsEnabled)
            {
                return false;
            }

            if (!plistExecutionStatus && !resultsBits.OrAll())
            {
                this.console?.PrintDebug($"VminSearchSP: Invalid result bits=[{resultsBits.ToStr()}], no failures reported for a PList fail condition");
                return true;
            }

            if (this.IsMaskFailing(resultsBits))
            {
                this.console?.PrintDebug($"VminSearchSP: Invalid result bits=[{resultsBits.ToStr()}], fail check for mask=[{this.CurrentState.MaskBits.ToStr()}]");
                return true;
            }

            return false;
        }

        private void DefineVoltagesForNextSearchPoint()
        {
            var isAnyVoltageUpdated = false;

            for (var index = 0; index < this.targetsCount; ++index)
            {
                if (this.CurrentState.Voltages[index] >= 0.0 && this.plistResultBits[index])
                {
                    this.UpdateVoltageToNextStep(index);
                    this.CurrentState.PerTargetIncrements[index]++;
                    isAnyVoltageUpdated = true;
                }

                if (this.IsBeforeEndLimit(this.CurrentState.Voltages[index], index))
                {
                    continue;
                }

                this.DisableTarget(index);
                isAnyVoltageUpdated = true;
            }

            if (!isAnyVoltageUpdated)
            {
                this.console?.PrintDebug("VminSearchSP: Replacing result from ProcessPlistResults extension by all fail bits because none voltage was updated for next point");
                this.plistResultBits = new BitArray(this.targetsCount, true);
                this.DefineVoltagesForNextSearchPoint();
            }

            if (this.repeatedVoltageHandler.AreRepeatedTargets)
            {
                this.CurrentState.Voltages = this.repeatedVoltageHandler.UpdateRepeatedVoltageTargets(this.CurrentState.Voltages);
            }
        }

        private void UpdateVoltageToNextStep(int index)
        {
            this.CurrentState.Voltages[index] = this.isHighToLowSearch
                ? Math.Round(this.CurrentState.Voltages[index] - this.stepSize, DoubleRoundingDecimals)
                : Math.Round(this.CurrentState.Voltages[index] + this.stepSize, DoubleRoundingDecimals);
        }

        private bool IsBeforeEndLimit(double valueToCompare, int index)
        {
            return (!this.isHighToLowSearch && valueToCompare <= this.CurrentState.EndVoltageLimits[index]) ||
                   (this.isHighToLowSearch && valueToCompare >= this.CurrentState.EndVoltageLimits[index]);
        }

        private void DisableTarget(int index)
        {
            this.CurrentState.Voltages[index] = VoltageFailValue;
            this.CurrentState.MaskBits[index] = true;
        }

        private void DisableAllTargets()
        {
            for (var index = 0; index < this.CurrentState.Voltages.Count; ++index)
            {
                if (!this.CurrentState.Voltages[index].Equals(VoltageMaskValue))
                {
                    this.DisableTarget(index);
                }
            }
        }

        private bool IsRecoveryNotPossible() => this.isRecoveryMaskEnabled
            ? this.CurrentState.MaskBits.AndAll()
            : this.CurrentState.Voltages.Contains(VoltageFailValue);

        private void StartVoltageSetup()
        {
            if (this.CurrentState.ExecutionCount == 1 && this.isOvershootEnabled)
            {
                this.CurrentState.StartVoltages = this.vminSearchExtensions.GetLowerStartVoltageValues(this.LowerStartVoltageKeys);
            }
            else
            {
                this.CurrentState.ExecutionCount = 0;
                this.CurrentState.StartVoltages = this.vminSearchExtensions.GetStartVoltageValues(this.StartVoltageKeys);
            }

            if (this.CurrentState.StartVoltages.Count != this.targetsCount)
            {
                throw new Base.Exceptions.TestMethodException(
                    $"List count=[{this.CurrentState.StartVoltages.Count}] returned from GetStartVoltageValues " +
                    $"extension does not match target count=[{this.targetsCount}]");
            }
        }

        private void EndVoltageSetup()
        {
            this.CurrentState.EndVoltageLimits = this.vminSearchExtensions.GetEndVoltageLimitValues(this.EndVoltageLimitKeys);
            if (this.CurrentState.EndVoltageLimits.Count != this.targetsCount)
            {
                throw new Base.Exceptions.TestMethodException(
                    $"List count=[{this.CurrentState.EndVoltageLimits.Count}] returned from GetEndVoltageLimitValues " +
                    $"extension does not match target count=[{this.targetsCount}]");
            }
        }

        private void InitialMaskSetup()
        {
            this.CurrentState.MaskBits = this.vminSearchExtensions.GetInitialMaskBits();
            if (this.CurrentState.MaskBits.Length != this.targetsCount)
            {
                this.console?.PrintError($"VminSearchSP: Initial mask bits=[{this.CurrentState.MaskBits.ToStr()}] does not match the target count=[{this.targetsCount}], all enable bits will be used instead.");
                this.CurrentState.MaskBits = new BitArray(this.targetsCount, false);
            }

            for (var index = 0; index < this.targetsCount; ++index)
            {
                if (this.CurrentState.Voltages[index] < 0)
                {
                    this.CurrentState.MaskBits[index] = true;
                }

                if (this.CurrentState.MaskBits[index])
                {
                    this.CurrentState.Voltages[index] = VoltageMaskValue;
                }
            }

            this.console?.PrintDebug($"VminSearchSP: Initial mask bits=[{this.CurrentState.MaskBits.ToStr()}].");
        }

        private bool IsMaskFailing(BitArray resultBits)
        {
            var localBits = new BitArray(this.CurrentState.MaskBits);
            return localBits.And(resultBits).OrAll();
        }

        private void SetPointData()
        {
            var patternData = new SearchPointData.PatternData(PrimeVminSearchTestMethod.NoLimitingPatternToken, 0, 0);
            if (this.SearchPlist is ICaptureFailureTest test && this.isStartOnFirstFailEnabled)
            {
                var failPatternData = test.GetPerCycleFailures();
                if (failPatternData.Count > 0)
                {
                    var patternName = failPatternData[0].GetPatternName();
                    var burstIndex = Convert.ToUInt32(failPatternData[0].GetBurstIndex());
                    var patternId = Convert.ToUInt32(failPatternData[0].GetPatternInstanceId());
                    var failVector = failPatternData[0].GetVectorAddress();
                    patternData = new SearchPointData.PatternData(patternName, burstIndex, patternId, failVector);
                }
            }

            this.CurrentState.PerPointData.Add(new SearchPointData(new List<double>(this.CurrentState.Voltages), patternData));
        }

        private bool AreVoltageValuesValid()
        {
            var isThereAnyPositiveStartVoltage = false;
            for (var i = 0; i < this.CurrentState.StartVoltages.Count; i++)
            {
                if (!isThereAnyPositiveStartVoltage && this.CurrentState.StartVoltages[i] >= 0.0)
                {
                    isThereAnyPositiveStartVoltage = true;
                }

                if (this.IsBeforeEndLimit(this.CurrentState.StartVoltages[i], i))
                {
                    continue;
                }

                this.console?.PrintError($"VminSearchSP: Start voltage=[{this.CurrentState.StartVoltages[i]}] can't be {(this.isHighToLowSearch ? "lower" : "higher")} than End voltage=[{this.CurrentState.EndVoltageLimits[i]}]");
                this.CurrentState.FailReason = "InvalidRange";
                return false;
            }

            if (!isThereAnyPositiveStartVoltage)
            {
                this.console?.PrintError("VminSearchSP: There are no valid start voltage values, this search is not executed");
                this.CurrentState.FailReason = "InvalidStartVoltage";
                return false;
            }

            return true;
        }
    }
}
