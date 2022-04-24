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

namespace DynamicFuncRestoreTC
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Prime;
    using Prime.Base.Exceptions;
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
        private readonly IFileHandler fileHandler = new FileHandler();
        private IConsoleService console;
        private Dictionary<string, TestTypeConfiguration> testTypeMapping;

        /// <summary>
        /// Gets or sets name of input test mapping configuration file.
        /// </summary>
        public TestMethodsParams.File TestTypeMappingFile { get; set; } = string.Empty;

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

            this.testTypeMapping = new Dictionary<string, TestTypeConfiguration>();
            if (!this.fileHandler.Exists(this.TestTypeMappingFile))
            {
                throw new TestMethodException($"File {this.TestTypeMappingFile} doesn't exist.");
            }

            var configurationFileContent = this.fileHandler.ReadAll(this.TestTypeMappingFile);
            this.testTypeMapping = JsonConvert.DeserializeObject<Dictionary<string, TestTypeConfiguration>>(configurationFileContent);
            foreach (var configuration in this.testTypeMapping)
            {
                this.console?.PrintDebug($"Configuration=[{configuration.Key}]");
                configuration.Value.Plist = new PlistHandler(configuration.Value.PlistNames, this.console);
                this.console?.PrintDebug($"\tBypassUserVar=[{configuration.Value.BypassUserVar}] verify value=[{Services.UserVarService.GetIntValue(configuration.Value.BypassUserVar)}]");
            }
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "PASS PORT")]
        [Returns(0, PortType.Fail, "FAIL PORT")]
        public override int Execute()
        {
            if (this.RestoreMode == RestoreTypeEnum.EnableAll)
            {
                foreach (var configuration in this.testTypeMapping)
                {
                    this.console?.PrintDebug($"Applying changes for configuration=[{configuration.Key}]");
                    this.SetBypassUserVar(configuration.Value.BypassUserVar, false);
                    configuration.Value.Plist.EnableAllPatterns();
                }
            }
            else if (this.RestoreMode == RestoreTypeEnum.SkipAll)
            {
                foreach (var configuration in this.testTypeMapping)
                {
                    this.console?.PrintDebug($"Applying changes for configuration=[{configuration.Key}]");
                    this.SetBypassUserVar(configuration.Value.BypassUserVar, true);
                    configuration.Value.Plist.DisableAllPatterns();
                }
            }

            if (this.CleanRestoreVariables == BooleanEnum.True)
            {
                if (Services.SharedStorageService.KeyExistsInObjectTable("PLIST_NAMES_TO_RESTORE", Context.LOT))
                {
                    this.console?.PrintDebug("Removing all plist names from restore list");
                    Services.SharedStorageService.InsertRowAtTable("PLIST_NAMES_TO_RESTORE", new List<string>(), Context.LOT);
                    Services.SharedStorageService.OverrideObjectRowResetPolicy("PLIST_NAMES_TO_RESTORE", ResetPolicy.NEVER_RESET, Context.LOT);
                }

                if (Services.SharedStorageService.KeyExistsInObjectTable("BYPASS_VARIABLES_TO_RESTORE", Context.LOT))
                {
                    this.console?.PrintDebug("Removing all BypassPort UserVar names from restore list");
                    Services.SharedStorageService.InsertRowAtTable("BYPASS_VARIABLES_TO_RESTORE", new List<string>(), Context.LOT);
                    Services.SharedStorageService.OverrideObjectRowResetPolicy("BYPASS_VARIABLES_TO_RESTORE", ResetPolicy.NEVER_RESET, Context.LOT);
                }
            }

            return 1;
        }

        private void SetBypassUserVar(string userVar, bool isBypass)
        {
            var bypassValue = isBypass ? 1 : -1;
            this.console?.PrintDebug($"\tSetting bypassPort UserVar=[{userVar}] to value=[{bypassValue}]");
            Services.UserVarService.SetValue(userVar, bypassValue);
        }
    }
}