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

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VminForwardingBase.UnitTest")]

namespace VminForwardingBase
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Prime.PhAttributes;
    using Prime.SharedStorageService;
    using Prime.TestMethods;
    using Prime.VminForwardingService;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class VminForwardingBase : TestMethodBase
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
        /// Gets or sets the Templates Execution mode (Either Configur or DumpTables).
        /// </summary>
        public ExecuteMode Mode { get; set; } = ExecuteMode.Configure;

        /// <summary>
        /// Gets or sets the operation mode to force VminTC to only run a single test point.
        /// </summary>
        public MyBool VminSinglePointMode { get; set; } = MyBool.False;

        /// <summary>
        /// Gets or sets the operation mode to enable VminTC SearchGuradband mode.
        /// </summary>
        public MyBool SearchGuardbandEnable { get; set; } = MyBool.False;

        /// <summary>
        /// Gets or sets the operation mode flag OperationModeFlag.UseLimitCheckAsSource.
        /// </summary>
        public MyBool UseDffAsSource { get; set; } = MyBool.False;

        /// <summary>
        /// Gets or sets the operation mode flag OperationModeFlag.UseLimitCheck.
        /// </summary>
        public MyBool UseLimitCheck { get; set; } = MyBool.False;

        /// <summary>
        /// Gets or sets the operation mode flag OperationModeFlag.UseVoltagesSources.
        /// </summary>
        public MyBool UseVoltagesSources { get; set; } = MyBool.True;

        /// <summary>
        /// Gets or sets the operation mode flag OperationModeFlag.StoreVoltages.
        /// </summary>
        public MyBool StoreVoltages { get; set; } = MyBool.True;

        /// <summary>
        /// Gets or sets the name of the DFF-to-GSDS mapping file.
        /// </summary>
        public TestMethodsParams.String DffMappingFile { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the Set within the DffMappingFile to use.
        /// </summary>
        public TestMethodsParams.String DffMappingSet { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the location to read the DFF data from.
        /// </summary>
        public TestMethodsParams.String DffMappingOptype { get; set; } = string.Empty;

        /// <inheritdoc />
        public override void Verify()
        {
            int errors = 0;
            if (this.Mode == ExecuteMode.Configure && (this.UseDffAsSource == MyBool.True || this.UseLimitCheck == MyBool.True) && (string.IsNullOrWhiteSpace(this.DffMappingFile) || string.IsNullOrWhiteSpace(this.DffMappingSet) || string.IsNullOrWhiteSpace(this.DffMappingOptype)))
            {
                Prime.Services.ConsoleService.PrintError("Parameters [DffMappingFile, DffMappingSet, DffMappingOptype] are required when Mode=[Configure] and UseDffAsSource=[True] or UseLimitCheck=[True].");
                errors++;
            }

            if (this.Mode == ExecuteMode.Configure && this.UseDffAsSource == MyBool.True && this.UseVoltagesSources == MyBool.True)
            {
                Prime.Services.ConsoleService.PrintError("Parameters [UseDffAsSource] and [UseVoltagesSources] cannot both be True.");
                errors++;
            }

            if (errors == 0)
            {
                return;
            }

            throw new Exception($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Number of {nameof(errors)}=[{errors}].");
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            if (this.Mode == ExecuteMode.Configure)
            {
                Prime.Services.ConsoleService.PrintDebug($"Setting Mode VminSinglePointMode to [{this.VminSinglePointMode}].");
                Prime.Services.SharedStorageService.InsertRowAtTable(
                    DDG.VminForwarding.Globals.VminForwardingSinglePointMode,
                    this.VminSinglePointMode == MyBool.True ? 1 : 0,
                    DDG.VminForwarding.Globals.VminForwardingFlagContext);
                Prime.Services.SharedStorageService.OverrideIntegerRowResetPolicy(DDG.VminForwarding.Globals.VminForwardingSinglePointMode, ResetPolicy.NEVER_RESET, DDG.VminForwarding.Globals.VminForwardingFlagContext);

                Prime.Services.ConsoleService.PrintDebug($"Setting Mode SearchGuardbandEnable to [{this.SearchGuardbandEnable}].");
                Prime.Services.SharedStorageService.InsertRowAtTable(
                    DDG.VminForwarding.Globals.VminForwardingSearchGuardbandEnable,
                    this.SearchGuardbandEnable == MyBool.True ? 1 : 0,
                    DDG.VminForwarding.Globals.VminForwardingFlagContext);
                Prime.Services.SharedStorageService.OverrideIntegerRowResetPolicy(DDG.VminForwarding.Globals.VminForwardingSearchGuardbandEnable, ResetPolicy.NEVER_RESET, DDG.VminForwarding.Globals.VminForwardingFlagContext);

                Prime.Services.ConsoleService.PrintDebug($"Setting OperationModeFlag.UseLimitCheckAsSource to [{this.UseDffAsSource}].");
                Prime.Services.VminForwardingService.SetOperationModeFlag(OperationMode.UseLimitCheckAsSource, this.UseDffAsSource == MyBool.True);
                this.StoreFlag(DDG.VminForwarding.Globals.VminForwardingUseLimitCheckAsSourceEnable, this.UseDffAsSource);

                Prime.Services.ConsoleService.PrintDebug($"Setting OperationModeFlag.UseVoltagesSources to [{this.UseVoltagesSources}].");
                Prime.Services.VminForwardingService.SetOperationModeFlag(OperationMode.UseVoltagesSources, this.UseVoltagesSources == MyBool.True);
                this.StoreFlag(DDG.VminForwarding.Globals.VminForwardingUseVoltagesSourcesEnable, this.UseVoltagesSources);

                Prime.Services.ConsoleService.PrintDebug($"Setting OperationModeFlag.UseLimitCheck to [{this.UseLimitCheck}].");
                Prime.Services.VminForwardingService.SetOperationModeFlag(OperationMode.UseLimitCheck, this.UseLimitCheck == MyBool.True);
                this.StoreFlag(DDG.VminForwarding.Globals.VminForwardingUseLimitCheckEnable, this.UseLimitCheck);

                Prime.Services.ConsoleService.PrintDebug($"Setting OperationModeFlag.StoreVoltages to [{this.StoreVoltages}].");
                Prime.Services.VminForwardingService.SetOperationModeFlag(OperationMode.StoreVoltages, this.StoreVoltages == MyBool.True);
                this.StoreFlag(DDG.VminForwarding.Globals.VminForwardingStoreVoltagesEnable, this.StoreVoltages);

                var tokenMap = new Dictionary<string, string>();
                if (this.UseDffAsSource == MyBool.True || this.UseLimitCheck == MyBool.True)
                {
                    tokenMap = this.ParseDffMappingFile();
                }

                Prime.Services.SharedStorageService.InsertRowAtTable(DDG.VminForwarding.Globals.VminForwardingDffMap, tokenMap, DDG.VminForwarding.Globals.VminForwardingDffMapContext);
                Prime.Services.SharedStorageService.OverrideObjectRowResetPolicy(DDG.VminForwarding.Globals.VminForwardingDffMap, ResetPolicy.NEVER_RESET, DDG.VminForwarding.Globals.VminForwardingDffMapContext);
                Prime.Services.ConsoleService.PrintDebug($"Done.");
            }
            else if (this.Mode == ExecuteMode.DumpTables)
            {
                // Get data from the new export handler.
                var forwardingExportHandler = Prime.Services.VminForwardingService.CreateExportHandler();
                var dataSnapshot = forwardingExportHandler.GetProcessedCornersData();
                foreach (var domainName in dataSnapshot.Keys.OrderBy(k => k))
                {
                    Prime.Services.ConsoleService.PrintDebug($"ExportHandler: Domain={domainName}");
                    foreach (var instance in dataSnapshot[domainName].Keys.OrderBy(k => k))
                    {
                        Prime.Services.ConsoleService.PrintDebug($"\tInstance={instance}");
                        foreach (var rec in dataSnapshot[domainName][instance])
                        {
                            Prime.Services.ConsoleService.PrintDebug($"\t\tRecord={rec.Key} Active Data: Freq=[{rec.ActiveCornerData?.Frequency}] Voltage=[{rec.ActiveCornerData?.Voltage}] Flow=[{rec.ActiveCornerData?.Flow}]");
                        }
                    }
                }

                return 1;
            }

            return 1;
        }

        private void StoreFlag(string token, MyBool value)
        {
            Prime.Services.SharedStorageService.InsertRowAtTable(token, value == MyBool.True ? 1 : 0, DDG.VminForwarding.Globals.VminForwardingFlagContext);
            Prime.Services.SharedStorageService.OverrideIntegerRowResetPolicy(token, ResetPolicy.NEVER_RESET, DDG.VminForwarding.Globals.VminForwardingFlagContext);
        }

        private Dictionary<string, string> ParseDffMappingFile()
        {
            Prime.Services.ConsoleService.PrintDebug($"Loading DFF Mapping file {this.DffMappingFile}.");
            var localTrackerFile = DDG.FileUtilities.GetFile(this.DffMappingFile);
            var table = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(File.ReadAllText(localTrackerFile));
            if (!table.ContainsKey("UpsDffMap"))
            {
                throw new Prime.Base.Exceptions.TestMethodException($"DFF Mapping file=[{this.DffMappingFile}] is missing the Top-Level Token=[UpsDffMap].");
            }

            if (!table["UpsDffMap"].ContainsKey(this.DffMappingSet))
            {
                throw new Prime.Base.Exceptions.TestMethodException($"DFF Mapping file=[{this.DffMappingFile}] does not contain Set=[{this.DffMappingSet}].");
            }

            // var map = table["UpsDffMap"][this.DffMappingSet];
            var map = new Dictionary<string, string>(table["UpsDffMap"][this.DffMappingSet].Count);
            foreach (var item in table["UpsDffMap"][this.DffMappingSet])
            {
                map[item.Key] = string.IsNullOrWhiteSpace(this.DffMappingOptype) ? item.Value : $"{this.DffMappingOptype}:{item.Value}";
            }

            return map;
            /*
            var domains = DDG.VminForwarding.Service.GetAllDomainNames();
            var config = Prime.Services.VminForwardingService.CreateConfigurationHandler();
            var tokenMap = new Dictionary<string, string>();
            var errors = false;

            Prime.Services.ConsoleService.PrintDebug($"Creating DFF Token to SharedStorage Mapping.");
            foreach (var domain in domains)
            {
                var instances = DDG.VminForwarding.Service.GetInstanceNamesForDomain(domain);
                var corners = DDG.VminForwarding.Service.GetCornerNamesForDomainInstance(instances.First());
                foreach (var fullDomainCornerName in corners)
                {
                    var corner = fullDomainCornerName.Split('@').Last();
                    var cornerName = $"{domain}@{corner}";

                    if (!map.ContainsKey(cornerName))
                    {
                        Prime.Services.ConsoleService.PrintError($"Corner=[{cornerName}] not found in DffMappingFile=[{this.DffMappingFile}] DffMappingSet=[{this.DffMappingSet}].");
                        errors = true;
                    }
                    else
                    {
                        var dffToken = $"{this.DffMappingOptype}.{map[cornerName]}";
                        var sharedStorage = config.GetSharedStorageLimitCheck(cornerName);
                        if (string.IsNullOrWhiteSpace(sharedStorage))
                        {
                            Prime.Services.ConsoleService.PrintError($"Corner=[{cornerName}] does not have a SharedStorageLimitCheck field in the prime vminforwarding configuration file.");
                            errors = true;
                        }
                        else
                        {
                            tokenMap[dffToken] = sharedStorage;
                            Prime.Services.ConsoleService.PrintDebug($"\t{cornerName}:{dffToken} => {tokenMap[dffToken]}");
                        }
                    }
                }
            }

            if (errors)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"Errors mapping VminCorners to DFF/GSDS using ConfigFile=[{this.DffMappingFile}]");
            }

            return tokenMap; */
        }
    }
}