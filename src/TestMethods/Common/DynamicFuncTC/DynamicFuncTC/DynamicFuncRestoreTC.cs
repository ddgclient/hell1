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

namespace DynamicFuncTC
{
    using System.Collections.Generic;
    using Prime;
    using Prime.ConsoleService;
    using Prime.PhAttributes;
    using Prime.SharedStorageService;
    using Prime.TestMethods;

    /// <summary>
    /// Plist modification type options.
    /// </summary>
    public enum RestoreTypeEnum
    {
        /// <summary>
        /// All patterns in all pLists and bypass UserVars are initialized to skip.
        /// </summary>
        SkipAll,

        /// <summary>
        /// All patterns in all plists and bypass UserVars are expected to be enabled at loading time.
        /// </summary>
        EnableAll,
    }

    /// <summary>
    /// Boolean enum.
    /// </summary>
    public enum BooleanEnum
    {
        /// <summary>
        /// True.
        /// </summary>
        True,

        /// <summary>
        /// False.
        /// </summary>
        False,
    }

    /// <summary>
    /// Dummy summary.
    /// </summary>
    [PrimeTestMethod]
    public class DynamicFuncRestoreTC : TestMethodBase
    {
        private IConsoleService console;

        /// <summary>
        /// Gets or sets desired plist modification type.
        /// </summary>
        public RestoreTypeEnum RestoreMode { get; set; } = RestoreTypeEnum.EnableAll;

        /// <summary>
        /// Gets or sets desired plist modification type.
        /// </summary>
        public BooleanEnum CleanRestoreVariables { get; set; } = BooleanEnum.False;

        /// <inheritdoc />
        public override void Verify()
        {
            this.console = this.LogLevel != PrimeLogLevel.DISABLED ? Services.ConsoleService : null;
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "PASS PORT")]
        [Returns(0, PortType.Fail, "FAIL PORT")]
        public override int Execute()
        {
            if (Services.SharedStorageService.KeyExistsInObjectTable("PLIST_NAMES_TO_RESTORE", Context.LOT))
            {
                var plistNamesToRestore = (List<string>)Services.SharedStorageService.GetRowFromTable("PLIST_NAMES_TO_RESTORE", typeof(List<string>), Context.LOT);
                foreach (var plist in plistNamesToRestore)
                {
                    var plistObject = Services.PlistService.GetPlistObject(plist);
                    if (this.RestoreMode == RestoreTypeEnum.EnableAll)
                    {
                        this.console?.PrintDebug($"Modifying plist=[{plist}] to all enabled");
                        plistObject.EnableAllPatterns();
                    }
                    else if (this.RestoreMode == RestoreTypeEnum.SkipAll)
                    {
                        this.console?.PrintDebug($"Modifying plist=[{plist}] to all disabled");
                        plistObject.DisableAllPatterns();
                    }
                }

                if (this.CleanRestoreVariables == BooleanEnum.True)
                {
                    this.console?.PrintDebug("Removing all plist names from restore list");
                    Services.SharedStorageService.InsertRowAtTable("PLIST_NAMES_TO_RESTORE", new List<string>(), Context.LOT);
                    Services.SharedStorageService.OverrideObjectRowResetPolicy("PLIST_NAMES_TO_RESTORE", ResetPolicy.NEVER_RESET, Context.LOT);
                }
            }

            if (Services.SharedStorageService.KeyExistsInObjectTable("BYPASS_VARIABLES_TO_RESTORE", Context.LOT))
            {
                var instanceNamesToRestore = (List<string>)Services.SharedStorageService.GetRowFromTable("BYPASS_VARIABLES_TO_RESTORE", typeof(List<string>), Context.LOT);
                foreach (var bypassUserVar in instanceNamesToRestore)
                {
                    var userVarValue = this.RestoreMode == RestoreTypeEnum.EnableAll ? -1 : 1;
                    this.console?.PrintDebug($"Setting BypassPort UserVar=[{bypassUserVar}] to value=[{userVarValue}]");
                    Services.UserVarService.SetValue(bypassUserVar, userVarValue);
                }
            }

            if (this.CleanRestoreVariables == BooleanEnum.True)
            {
                this.console?.PrintDebug("Removing all BypassPort UserVar names from restore list");
                Services.SharedStorageService.InsertRowAtTable("BYPASS_VARIABLES_TO_RESTORE", new List<string>(), Context.LOT);
                Services.SharedStorageService.OverrideObjectRowResetPolicy("BYPASS_VARIABLES_TO_RESTORE", ResetPolicy.NEVER_RESET, Context.LOT);
            }

            return 1;
        }
    }
}