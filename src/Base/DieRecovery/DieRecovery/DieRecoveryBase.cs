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
    using System.IO;
    using DDG;
    using Newtonsoft.Json;
    using Prime;
    using Prime.PhAttributes;
    using Prime.SharedStorageService;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class DieRecoveryBase : TestMethodBase
    {
        /// <summary>
        /// Enum to control template execution mode.
        /// </summary>
        public enum ExecuteMode
        {
            /// <summary>Configure Mode, run once during Init to build the Forwarding Table.</summary>
            Configure,

            /// <summary>Print out all the VminForwarding Table information.</summary>
            DumpTables,
        }

        /// <summary>
        /// Simple enum to hold True/False parameter values.
        /// </summary>
        public enum MyBool
        {
            /// <summary>
            /// Enum for True.
            /// </summary>
            True,

            /// <summary>
            /// Enum For false.
            /// </summary>
            False,
        }

        /// <summary>
        /// Gets or sets the DieRecovery Rules file.
        /// </summary>
        public TestMethodsParams.String RulesFile { get; set; } = "./Modules/YBS_UPSS/InputFiles/Recovery.xml";

        /// <summary>
        /// Gets or sets the DieRecovery Tracker configuration file.
        /// </summary>
        public TestMethodsParams.String TrackerFile { get; set; } = "./PrimeConfigs/DieRecoveryTrackers.json";

        /// <summary>
        /// Gets or sets the Templates Execution Mode (Only Configure is valid for a production test program).
        /// </summary>
        public ExecuteMode Mode { get; set; } = ExecuteMode.Configure;

        /// <summary>
        /// Gets or sets the value indicating whether or not DownBins are valid. If false, the Trackers can only be written once and will fail if their value is every changed.
        /// </summary>
        public MyBool AllowDownBins { get; set; } = MyBool.True;

        /// <inheritdoc />
        public override void Verify()
        {
            if (this.Mode == ExecuteMode.Configure && (string.IsNullOrEmpty(this.TrackerFile) || string.IsNullOrEmpty(this.RulesFile)))
            {
                throw new FileLoadException("Error: ConfigFile and RulesFile are required when mode==Configure.", this.TrackerFile);
            }
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            if (this.Mode == ExecuteMode.Configure)
            {
                var downBinsAllowed = this.AllowDownBins == MyBool.True ? 1 : 0;
                Services.SharedStorageService.InsertRowAtTable(DieRecovery.Globals.DieRecoveryTrackerDownBinsAllowed, downBinsAllowed, DieRecovery.Globals.DieRecoveryTrackerGlobalContext);
                Services.SharedStorageService.OverrideIntegerRowResetPolicy(DieRecovery.Globals.DieRecoveryTrackerDownBinsAllowed, ResetPolicy.NEVER_RESET, DieRecovery.Globals.DieRecoveryTrackerGlobalContext);

                return this.BuildDieRecoveryTables() ? 1 : 0;
            }

            if (this.Mode == ExecuteMode.DumpTables)
            {
                OutputAllDieRecoveryTables();
            }

            return 1;
        }

        private static void OutputAllDieRecoveryTables()
        {
            Services.ConsoleService.PrintDebug("Writing out all DieRecovery Tracking data.\n");
            foreach (var trackerName in DieRecovery.Utilities.GetAllTrackerNames())
            {
                try
                {
                    var data = DieRecovery.Utilities.RetrieveTrackerData(trackerName);
                    Services.ConsoleService.PrintDebug($"DieRecovery Tracker=[{trackerName}] Data=[{data}]");
                }
                catch
                {
                    Services.ConsoleService.PrintDebug($"DieRecovery Tracker=[{trackerName}] Data=[None]");
                }
            }

            Services.ConsoleService.PrintDebug("\nWriting out all DieRecovery PinMaps.\n");
            var pinMapNames = DieRecovery.Utilities.GetAllPinMapNames();
            foreach (var name in pinMapNames)
            {
                var pinMap = DieRecovery.Utilities.RetrievePinMapDecoder(name);
                Services.ConsoleService.PrintDebug($"PinMap=[{pinMap.Name}] Size=[{pinMap.NumberOfTrackerElements}] ConfigPatMod=[{pinMap.IpPatternConfigure}].");
                Services.ConsoleService.PrintDebug($"\tData={JsonConvert.SerializeObject(pinMap)}");
            }

            Services.ConsoleService.PrintDebug("\nWriting out all DieRecovery Rules.\n");
            var rules = DieRecovery.Utilities.GetAllRuleNames();
            foreach (var ruleName in rules)
            {
                var rule = DieRecovery.Utilities.RetrieveRule(ruleName);

                Services.ConsoleService.PrintDebug($"RuleCollection=[{rule.Name}].");
                foreach (var subRule in rule.Rules)
                {
                    Services.ConsoleService.PrintDebug($"\tRule=[{subRule.Name}] Mode=[{subRule.Mode}] Size=[{subRule.Size}] Type=[{subRule.Type}].");
                    foreach (var bitVector in subRule.Values)
                    {
                        Services.ConsoleService.PrintDebug($"\t\tBitVector=[{bitVector}].");
                    }
                }
            }

            Services.ConsoleService.PrintDebug("\nDone.");
        }

        private bool BuildDieRecoveryTables()
        {
            if (!DieRecovery.Service.LoadTrackerFile(this.TrackerFile))
            {
                return false;
            }

            if (!DieRecovery.Service.LoadRulesFile(this.RulesFile))
            {
                return false;
            }

            return true;
        }
    }
}