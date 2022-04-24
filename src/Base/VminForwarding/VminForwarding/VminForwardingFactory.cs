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

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VminForwardingCallbacks.UnitTest")]

namespace VminForwardingBase
{
    using System.Collections.Generic;
    using System.Linq;
    using DDG;
    using Prime;
    using Prime.VminForwardingService;

    /// <summary>
    /// Defines the <see cref="VminForwardingFactory" />.
    /// </summary>
    internal class VminForwardingFactory : IVminForwardingFactory
    {
        /// <inheritdoc/>
        public IVminForwardingCorner Get(string corner, int flow)
        {
            return new VminForwardingTable(corner, flow);
        }

        /// <inheritdoc/>
        public IVminForwardingCorner Get(string domain, string freqCorner, int flow)
        {
            return new VminForwardingTable(domain, freqCorner, flow);
        }

        /// <inheritdoc/>
        public List<string> GetAllDomainNames() => Services.VminForwardingService.CreateConfigurationHandler().GetDomainNames();

        /// <inheritdoc/>
        /// <exception cref="Prime.Base.Exceptions.TestMethodException"> Thrown if domain is not valid.</exception>
        public List<string> GetInstanceNamesForDomain(string domainName) => Services.VminForwardingService.CreateConfigurationHandler().GetInstanceNames(domainName);

        /// <inheritdoc/>
        /// <exception cref="Prime.Base.Exceptions.TestMethodException"> Thrown if instance is not valid.</exception>
        public List<string> GetCornerNamesForDomainInstance(string instanceName) => Services.VminForwardingService.CreateConfigurationHandler().GetsCornerNames(instanceName);

        /// <inheritdoc/>
        /// <exception cref="System.ArgumentException"> Thrown if domain or corner is not valid.</exception>
        public double GetFrequency(string fullCornerName, int flow) => Services.VminForwardingService.CreateHandler(new List<string> { fullCornerName }, flow).GetFrequencySourceValue(fullCornerName);

        /// <inheritdoc/>
        public Dictionary<string, string> GetDffTokenMap() =>
            (Dictionary<string, string>)Services.SharedStorageService.GetRowFromTable(
                VminForwarding.Globals.VminForwardingDffMap,
                typeof(Dictionary<string, string>),
                VminForwarding.Globals.VminForwardingDffMapContext);

        /// <inheritdoc/>
        public bool IsSinglePointMode() =>
            Services.SharedStorageService.KeyExistsInIntegerTable(VminForwarding.Globals.VminForwardingSinglePointMode, VminForwarding.Globals.VminForwardingFlagContext)
            ? Services.SharedStorageService.GetIntegerRowFromTable(VminForwarding.Globals.VminForwardingSinglePointMode, VminForwarding.Globals.VminForwardingFlagContext) == 1
            : false;

        /// <inheritdoc/>
        public bool IsSearchGuardbandEnabled() =>
            Services.SharedStorageService.KeyExistsInIntegerTable(VminForwarding.Globals.VminForwardingSearchGuardbandEnable, VminForwarding.Globals.VminForwardingFlagContext)
            ? Services.SharedStorageService.GetIntegerRowFromTable(VminForwarding.Globals.VminForwardingSearchGuardbandEnable, VminForwarding.Globals.VminForwardingFlagContext) == 1
            : false;

        /// <inheritdoc/>
        public VminForwardingCornerData GetVminForwardingSnapshot(string cornerName)
        {
            var name = DDG.VminForwarding.Globals.VminForwardingSnapshot + DDG.VminForwarding.Globals.NameSeparator + cornerName;
            var context = DDG.VminForwarding.Globals.VminForwardingSnapshotContext;
            if (Prime.Services.SharedStorageService.KeyExistsInObjectTable(name, context))
            {
                return (VminForwardingCornerData)Prime.Services.SharedStorageService.GetRowFromTable(name, typeof(VminForwardingCornerData), context);
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="Prime.Base.Exceptions.TestMethodException"> Thrown if domain is not valid.</exception>
        public void SaveVminForwardingSnapshot(List<string> domainNames)
        {
            // Get data from the new export handler.
            var forwardingExportHandler = Prime.Services.VminForwardingService.CreateExportHandler();
            var dataSnapshot = forwardingExportHandler.GetProcessedCornersData();

            foreach (var domainName in domainNames)
            {
                if (!dataSnapshot.ContainsKey(domainName))
                {
                    var str = $"No Domain=[{domainName}] found in Prime ExportHandler. Contents=[{string.Join(",", dataSnapshot.Keys.OrderBy(k => k))}]";
                    Prime.Services.ConsoleService.PrintError(str);
                    throw new Prime.Base.Exceptions.TestMethodException(str);
                }

                Prime.Services.ConsoleService.PrintDebug($"ExportHandler: Domain={domainName}");
                foreach (var instance in dataSnapshot[domainName].Keys)
                {
                    Prime.Services.ConsoleService.PrintDebug($"\tInstance={instance}");
                    foreach (var rec in dataSnapshot[domainName][instance])
                    {
                        Prime.Services.ConsoleService.PrintDebug($"\t\tRecord={rec.Key} Active Data=[{rec.ActiveCornerData?.Voltage}] Flow=[{rec.ActiveCornerData?.Flow}]");
                        if (rec.ActiveCornerData != null)
                        {
                            var name = DDG.VminForwarding.Globals.VminForwardingSnapshot + DDG.VminForwarding.Globals.NameSeparator + rec.Key;
                            var context = DDG.VminForwarding.Globals.VminForwardingSnapshotContext;
                            Prime.Services.SharedStorageService.InsertRowAtTable(name, rec.ActiveCornerData, context);
                        }
                    }
                }
            }
        }
    }
}
