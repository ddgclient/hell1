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

namespace VminTC
{
    using System.Linq;
    using System.Reflection;
    using Prime;

    /// <summary>
    /// Aggregates and stores multiple SearchResults.
    /// </summary>
    public class FeatureSwitches
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureSwitches"/> class.
        /// </summary>
        /// <param name="argument">List of comma-separated switches.</param>
        public FeatureSwitches(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                return;
            }

            var featureList = argument.Split(',');
            this.DisableMaskedTargets = featureList.Contains("disable_masked_targets") || featureList.Contains("disable_pairs") || featureList.Contains("disable_quadruplets");
            this.DisablePairs = featureList.Contains("disable_pairs");
            this.DisableQuadruplets = featureList.Contains("disable_quadruplets");
            this.IsStartOnFirstFail = !featureList.Contains("start_on_first_fail_off");
            this.IsOvershootModeEnabled = featureList.Contains("overshoot_enabled");
            this.IgnoreMaskedResults = featureList.Contains("ignore_masked_results");
            this.IsPerPatternPrintingEnabled = featureList.Contains("per_pattern_printing");
            this.PrintPerTargetIncrements = featureList.Contains("print_per_target_increments");
            this.PrintIndependentSearchResults = featureList.Contains("print_results_for_all_searches");
            this.ResetPointers = featureList.Contains("reset_pointers");
            this.RecoveryUpdateAlways = featureList.Contains("recovery_update_always");
            this.VminUpdateOnPassOnly = featureList.Contains("vmin_update_on_pass_only");
            this.TraceCtv = featureList.Contains("trace_ctv_on");
            this.CtvPerCycle = featureList.Contains("ctv_per_cycle");
            this.ReturnOnGlobalStickyError = featureList.Contains("return_on_global_sticky_error");
            this.ForceRecoveryLoop = featureList.Contains("force_recovery_loop");

            var message = "--FeatureSwitchSettings disable_masked_targets:";
            message += this.DisableMaskedTargets ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings disable_pairs:";
            message += this.DisablePairs ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings disable_quadruplets:";
            message += this.DisableQuadruplets ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings start_on_first_fail:";
            message += this.IsStartOnFirstFail ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings overshoot_enabled:";
            message += this.IsOvershootModeEnabled ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings ignore_masked_results:";
            message += this.IgnoreMaskedResults ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings pre_pattern_printing:";
            message += this.IsPerPatternPrintingEnabled ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings print_per_target_increments:";
            message += this.PrintPerTargetIncrements ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings print_results_for_all_searches:";
            message += this.PrintIndependentSearchResults ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings reset_pointers:";
            message += this.ResetPointers ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings recovery_update_always:";
            message += this.RecoveryUpdateAlways ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings vmin_update_on_pass_only:";
            message += this.VminUpdateOnPassOnly ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings trace_ctv:";
            message += this.TraceCtv ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings ctv_per_cycle:";
            message += this.CtvPerCycle ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings return_on_global_sticky_error:";
            message += this.ReturnOnGlobalStickyError ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings force_recovery_loop:";
            message += this.ForceRecoveryLoop ? "ON" : "OFF";
            Services.ConsoleService.PrintDebug($"{MethodBase.GetCurrentMethod()?.DeclaringType} {MethodBase.GetCurrentMethod()?.Name}:\n" + message);
        }

        /// <summary>
        /// Gets or sets a value indicating whether disable incoming masked items.
        /// </summary>
        public bool DisableMaskedTargets { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether disable pairs feature is enabled.
        /// </summary>
        public bool DisablePairs { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether disable quadruplets feature is enabled.
        /// </summary>
        public bool DisableQuadruplets { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether incremental search mode is enabled.
        /// </summary>
        public bool IsStartOnFirstFail { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether overshoot mode is enabled.
        /// </summary>
        public bool IsOvershootModeEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether per pattern vmin printing is enabled.
        /// </summary>
        public bool IsPerPatternPrintingEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether decoded results for masked elements should be ignored.
        /// </summary>
        public bool IgnoreMaskedResults { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether per target increments should be printed to ituff.
        /// </summary>
        public bool PrintPerTargetIncrements { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether independent search results should be printed to ituff.
        /// </summary>
        public bool PrintIndependentSearchResults { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether pointers should be reset.
        /// </summary>
        public bool ResetPointers { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether die recovery tracker should be updated regardless of pass/fail conditions.
        /// </summary>
        public bool RecoveryUpdateAlways { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether vmin forwarding should be updated only for passing voltages.
        /// </summary>
        public bool VminUpdateOnPassOnly { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the TraceCtv function should be called after every test execution.
        /// </summary>
        public bool TraceCtv { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether CTV per cycle is enabled.
        /// </summary>
        public bool CtvPerCycle { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the ReturnOn GlobalStickyError should be set.
        /// </summary>
        public bool ReturnOnGlobalStickyError { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the recovery loop should be force when search fail, regardless of rules validation result.
        /// </summary>
        public bool ForceRecoveryLoop { get; set; } = false;
    }
}
