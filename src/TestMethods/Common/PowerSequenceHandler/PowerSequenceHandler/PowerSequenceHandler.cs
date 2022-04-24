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

namespace PowerSequenceHandler
{
    using Prime.Base.Exceptions;
    using Prime.PhAttributes;
    using Prime.TestConditionService;
    using Prime.TestMethods;

    /// <summary>
    /// This TestMethod controls Power-On Test Condition.
    /// </summary>
    [PrimeTestMethod]
    public class PowerSequenceHandler : TestMethodBase
    {
        private ITestCondition powerOnTestCondition;
        private ITestCondition powerDownTestCondition;

        /// <summary>
        /// Forces apply for test condition.
        /// </summary>
        public enum ForceApplyTc
        {
            /// <summary>
            /// Skip Apply Test Condition.
            /// </summary>
            Skip,

            /// <summary>
            /// Force Apply Test Condition.
            /// </summary>
            Always,

            /// <summary>
            /// Apply Test Condition only when Power-ON TC is being changed.
            /// </summary>
            Switch,
        }

        /// <summary>
        /// Enabled or disabled switch.
        /// </summary>
        public enum AlarmModes
        {
            /// <summary>
            /// Enabled.
            /// </summary>
            Enabled,

            /// <summary>
            /// Disabled.
            /// </summary>
            Disabled,
        }

        /// <summary>
        /// Gets or sets ApplyPowerDown while setting a new Power-On TC.
        /// </summary>
        public ForceApplyTc ApplyPowerDown { get; set; } = ForceApplyTc.Skip;

        /// <summary>
        /// Gets or sets ApplyPowerOn.
        /// </summary>
        public ForceApplyTc ApplyPowerOn { get; set; } = ForceApplyTc.Skip;

        /// <summary>
        /// Gets or sets Power-on TC.
        /// </summary>
        public TestMethodsParams.LevelsCondition PowerOnTc { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets Power-down TC.
        /// </summary>
        public TestMethodsParams.LevelsCondition PowerDownTc { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets AlarmMode; when is enabled alarms will be routed to different port.
        /// </summary>
        public AlarmModes AlarmMode { get; set; } = AlarmModes.Disabled;

        /// <inheritdoc />
        public override void Verify()
        {
            if (this.ApplyPowerOn != ForceApplyTc.Skip)
            {
                this.powerOnTestCondition = Prime.Services.TestConditionService.GetTestCondition(this.PowerOnTc);
            }

            if (this.ApplyPowerDown != ForceApplyTc.Skip)
            {
                this.powerDownTestCondition = Prime.Services.TestConditionService.GetTestCondition(this.PowerDownTc);
            }
        }

        /// <inheritdoc />
        [Returns(2, PortType.Fail, "Alarm!")]
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            try
            {
                var originalPowerOn = Prime.Services.TestConditionService.GetPowerUpTCName();
                if (this.ApplyPowerDown == ForceApplyTc.Always ||
                    (this.ApplyPowerDown == ForceApplyTc.Switch && this.PowerOnTc != originalPowerOn))
                {
                    Prime.Services.ConsoleService.PrintDebug($"Applying {nameof(this.PowerDownTc)}={this.PowerDownTc}.");
                    FlushSmartTcCategorySuppressingError(SmartTCCategoryType.LEVELS_POWER_DOWN);
                    this.powerDownTestCondition.Apply(SmartTCCategoryType.LEVELS_POWER_DOWN);
                    FlushSmartTcCategorySuppressingError(SmartTCCategoryType.LEVELS_POWER_ON);
                    FlushSmartTcCategorySuppressingError(SmartTCCategoryType.LEVELS_SETUP);
                }

                if (!string.IsNullOrEmpty(this.PowerOnTc))
                {
                    Prime.Services.ConsoleService.PrintDebug($"Setting {nameof(this.PowerOnTc)}={this.PowerOnTc}.");
                    Prime.Services.TestConditionService.SetPowerUpTCName(this.PowerOnTc.ToString());
                    if (this.PowerOnTc != originalPowerOn)
                    {
                        FlushSmartTcCategorySuppressingError(SmartTCCategoryType.LEVELS_SETUP);
                    }
                }

                if (this.ApplyPowerOn == ForceApplyTc.Always ||
                    (this.ApplyPowerOn == ForceApplyTc.Switch && this.PowerOnTc != originalPowerOn))
                {
                    Prime.Services.ConsoleService.PrintDebug($"Applying {nameof(this.PowerOnTc)}={this.PowerOnTc}.");
                    FlushSmartTcCategorySuppressingError(SmartTCCategoryType.LEVELS_POWER_ON);
                    this.powerOnTestCondition.Apply(SmartTCCategoryType.LEVELS_POWER_ON);
                    FlushSmartTcCategorySuppressingError(SmartTCCategoryType.LEVELS_SETUP);
                }

                return 1;
            }
            catch (AlarmException exception)
            {
                Prime.Services.ConsoleService.PrintError(exception.GetAlarmMessage());
                if (this.AlarmMode == AlarmModes.Enabled)
                {
                    return 2;
                }

                throw;
            }
        }

        private static void FlushSmartTcCategorySuppressingError(SmartTCCategoryType category)
        {
            try
            {
                Prime.Services.TestConditionService.FlushSmartTCCategory(category);
            }
            catch
            {
                // ignored
            }
        }
    }
}