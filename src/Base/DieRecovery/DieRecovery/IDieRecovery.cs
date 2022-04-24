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
    using System.Collections;
    using System.Collections.Generic;
    using DieRecoveryBase;

    /// <summary>
    /// Defines the InputType for certain parameters.
    /// </summary>
    public enum InputType
    {
        /// <summary>
        /// Specifies the input value is supplied directly.
        /// </summary>
        Literal,

        /// <summary>
        /// Specifies the input value is supplied from a Evergreen GSDS Token.
        /// Format will be G.{level}.{type}.{variableName} where
        ///    {level} is U for Unit Level or L for Lot Level
        ///    {type} is S for string, I for integer or D for double
        ///    {variableName} is the name of the variable.
        /// </summary>
        Gsds,

        /// <summary>
        /// Specifies the input value is supplied from an HDMT UserVariable.
        /// Format will be {collection}.{variable}
        /// </summary>
        UserVar,

        /// <summary>
        /// Specifies the input value is supplied from Prime SharedStorage construct.
        /// Format will be {context}.{variable} where
        ///     {context} is DUT, LOT or IP.
        /// </summary>
        SharedStorage,
    }

    /// <summary>
    /// Defines the UpdateMode.
    /// </summary>
    public enum UpdateMode
    {
        /// <summary>
        /// Indicates the Update operation should merge the new data with the existing data.
        /// </summary>
        Merge,

        /// <summary>
        /// Indicates the Update operation should use the new data to overwrite the existing data.
        /// </summary>
        OverWrite,
    }

    /// <summary>
    /// Interface for main DieRecovery object.
    /// </summary>
    public interface IDieRecovery
    {
        /// <summary>
        /// Gets the Reset/Initial value associated with this DieRecovery Tracker.
        /// </summary>
        string ResetValue { get; }

        /// <summary>
        /// Returns the Tracking Structure as a BitArray, true elements are ones which should be masked.
        /// </summary>
        /// <returns>The <see cref="BitArray"/> containing the plist mask, true elements should be masked.</returns>
        BitArray GetMaskBits();

        /// <summary>
        /// Converts the given variable as a bit mask. 1 == true == mask.
        /// </summary>
        /// <param name="inputType">The inputType<see cref="InputType"/>.</param>
        /// <param name="inputName">The variable name or value if the type is Literal.</param>
        /// <returns>The <see cref="BitArray"/> containing the plist mask, true elements should be masked.</returns>
        BitArray GetMaskBits(InputType inputType, string inputName);

        /// <summary>
        /// Updates the tracking structure from the given bit array.
        /// </summary>
        /// <param name="value">The value<see cref="BitArray"/> to update the tracking structure with.</param>
        /// <param name="mask">Bit array specifying which slice bits should be ignored. Elements set to true means that bit will not be updated. Default is null (all bits enabled).</param>
        /// <param name="result">Bit array specifying results getting printing to ituff. Default is null (value will be used as result).</param>
        /// <param name="mode">The update mode<see cref="UpdateMode"/> indicating whether the new data should overwrite existing data or be merged (default is UpdateMode.Merge).</param>
        /// <param name="log">Boolean indicating whether to log the update to Ituff or not (default true==PrintToItuff).</param>
        /// <returns>Returns true if the update was successful, false otherwise.</returns>
        bool UpdateTrackingStructure(BitArray value, BitArray mask = null, BitArray result = null, UpdateMode mode = UpdateMode.Merge, bool log = true);

        /// <summary>
        /// Updates the tracking structure from the given GSDS/UserVar/Shared variable or literal value.
        /// </summary>
        /// <param name="inputType">The type of input<see cref="InputType"/>.</param>
        /// <param name="inputName">The name of the variable holding the data to update to the tracking structure, or the value itself.</param>
        /// <param name="mask">Bit array specifying which slice bits should be ignored. Elements set to true means that bit will not be updated. Default is null (all bits enabled).</param>
        /// <param name="result">Bit array result bits to be printed on ituff. Default is null (all bits enabled).</param>
        /// <param name="mode">The update mode<see cref="UpdateMode"/> indicating whether the new data should overwrite existing data or be merged (default is UpdateMode.Merge).</param>
        /// <param name="log">Boolean indicating whether to log the update to Ituff or not (default true==PrintToItuff).</param>
        /// <returns>Returns true if the update was successful, false otherwise.</returns>
        bool UpdateTrackingStructure(InputType inputType, string inputName, BitArray mask = null, BitArray result = null, UpdateMode mode = UpdateMode.Merge, bool log = true);

        /// <summary>
        /// Update the tracking structure from vmin values.
        /// </summary>
        /// <param name="voltages">The list of vmin results.</param>
        /// <param name="mask">Bit array specifying which slice bits should be ignored. Elements set to true means that bit will not be updated. Default is null (all bits enabled).</param>
        /// <param name="result">Bit array result bits to be printed on ituff. Default is null (all bits enabled).</param>
        /// <param name="mode">The update mode<see cref="UpdateMode"/> indicating whether the new data should overwrite existing data or be merged (default is UpdateMode.Merge).</param>
        /// <param name="vminLimitLow">The low limit for determining a pass.</param>
        /// <param name="vminLimitHigh">The upper limit for determining a pass.</param>
        /// <param name="log">Boolean indicating whether to log the update to Ituff or not (default true==PrintToItuff).</param>
        /// <returns>Returns true if the update was successful, false otherwise.</returns>
        bool UpdateTrackingStructure(List<double> voltages, BitArray mask = null, BitArray result = null, UpdateMode mode = UpdateMode.Merge, double vminLimitLow = 0, double vminLimitHigh = 100, bool log = true);

        /// <summary>
        /// Runs the given rule on the given tracking structure.
        /// </summary>
        /// <param name="rule">The name of the rule to run. It must match a rule_group name from the check_rule_settings section of DieRecovery xml input file.</param>
        /// <returns>The list of passing configuration bit vectors.</returns>
        List<DefeatureRule.Rule> RunRule(string rule);

        /// <summary>
        /// Runs the given rule on the given bitarray.
        /// </summary>
        /// <param name="input">Bitmask to run the rule against.</param>
        /// <param name="rule">The name of the rule to run. It must match a rule_group name from the check_rule_settings section of DieRecovery xml input file.</param>
        /// <returns>The list of passing configuration bit vectors.</returns>
        List<DefeatureRule.Rule> RunRule(BitArray input, string rule);

        /// <summary>
        /// Gets list of names for tracker.
        /// </summary>
        /// <returns>List of tracker names.</returns>
        string GetNames();

        /// <summary>
        /// Logs current test data without updating current tracker.
        /// </summary>
        /// <param name="mask">Mask bits.</param>
        /// <param name="result">Result bits.</param>
        void LogTrackingStructure(BitArray mask, BitArray result);
    }
}
