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

namespace VminForwardingBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DDG;
    using Prime.VminForwardingService;

    /// <summary>
    /// Defines the <see cref="VminForwardingPrediction" />.
    /// </summary>
    public class VminForwardingPrediction
    {
        /// <summary>
        /// Gets the Number of significant bits to use when comparing voltage values.
        /// </summary>
        public static readonly uint VminVoltageInVoltsPrecisionSignificantBits = 3;

        /// <summary>
        /// Gets the number of significant bits to use when comparing frequency (in mhz) values.
        /// </summary>
        public static readonly uint VminMhzFrequencyInMhzPrecisionSignificantBits = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="VminForwardingPrediction"/> class.
        /// </summary>
        public VminForwardingPrediction()
        {
            this.VminTable = null;
        }

        private Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>> VminTable { get; set; }

        /// <summary>
        /// Applies the STC Interpolation algorithm to the Prime Vmin Forwarding table.
        /// </summary>
        /// <param name="domainNames">List of domain names to apply interpolation to.</param>
        /// <param name="cornersToInterpolateFrom">List of corners to use for interpolation, this would be the coners without stc_interpolation="true".</param>
        /// <param name="currentFlow">Flow number of the current/passing flow. (used for logging).</param>
        public static void PrimeSTCInterpolation(List<string> domainNames, List<string> cornersToInterpolateFrom, int currentFlow)
        {
            // Get data from the new export handler.
            var vminPredictor = new VminForwardingPrediction();
            var forwardingExportHandler = Prime.Services.VminForwardingService.CreateExportHandler();
            vminPredictor.VminTable = forwardingExportHandler.GetProcessedCornersData();

            InterpolatorItuffPrint("_search_results", vminPredictor.GetVminItuffStr(domainNames, useSnapShotData: true));
            InterpolatorItuffPrint("_check_results", vminPredictor.GetVminItuffStr(domainNames, useSnapShotData: false));

            foreach (var domain in domainNames)
            {
                var allInstanceNames = DDG.VminForwarding.Service.GetInstanceNamesForDomain(domain);
                foreach (var instance in allInstanceNames)
                {
                    var allCorners = DDG.VminForwarding.Service.GetCornerNamesForDomainInstance(instance).Select(c => c.Split('@').Last()).OrderBy(j => j).ToList();

                    cornersToInterpolateFrom = cornersToInterpolateFrom.OrderBy(j => j).ToList();
                    for (var golCornerIndex = 0; golCornerIndex < cornersToInterpolateFrom.Count - 1; golCornerIndex++)
                    {
                        var lowCorner = cornersToInterpolateFrom[golCornerIndex];
                        var highCorner = cornersToInterpolateFrom[golCornerIndex + 1];
                        var lowCornerIndex = allCorners.IndexOf(lowCorner);
                        var highCornerIndex = allCorners.IndexOf(highCorner);
                        var interpolationCorners = allCorners.GetRange(lowCornerIndex + 1, highCornerIndex - lowCornerIndex - 1);
                        vminPredictor.ApplySTCInterpolationToCornersPrime(domain, instance, $"{instance}@{lowCorner}", $"{instance}@{highCorner}", interpolationCorners.Select(c => $"{instance}@{c}").ToList());
                    }
                }
            }

            // update the active data and relog.
            vminPredictor.VminTable = forwardingExportHandler.GetProcessedCornersData();
            InterpolatorItuffPrint("_interpolation_results", vminPredictor.GetVminItuffStr(domainNames, useSnapShotData: false));
        }

        /// <summary>
        /// Gets the current voltage for the given DomainInstance at the given frequency.
        /// If the frequency does not match a current corner the data will interpolated (using linear fit)
        /// from the nearest 2 corners (by frequency).
        /// </summary>
        /// <param name="domainInstance">Name of the domain instance. Must match a instance from the VminConfiguration file.</param>
        /// <param name="frequencyInGhz">Frequency to get the voltage for in GHz.</param>
        /// <returns>voltage value for the given instance and frequency.</returns>
        public double GetVoltage(string domainInstance, double frequencyInGhz)
        {
            if (this.VminTable == null)
            {
                var forwardingExportHandler = Prime.Services.VminForwardingService.CreateExportHandler();
                this.VminTable = forwardingExportHandler.GetProcessedCornersData();
            }

            if (this.VminTable == null || this.VminTable.Count() == 0)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"VMinForwarding table is empty, cannot get Voltage for DomainInstance=[{domainInstance}].");
            }

            // Get all the active corners for this instance.
            bool instanceIsValid = false;
            List<VminForwardingCornerData> allActiveDataForInstance = new List<VminForwardingCornerData>();
            foreach (var allInstanceDataForDomain in this.VminTable.Values)
            {
                if (allInstanceDataForDomain.ContainsKey(domainInstance))
                {
                    instanceIsValid = true;
                    foreach (var cornerData in allInstanceDataForDomain[domainInstance])
                    {
                        if (cornerData != null && cornerData.ActiveCornerData != null && cornerData.ActiveCornerData.Frequency < 90e9)
                        {
                            allActiveDataForInstance.Add(cornerData.ActiveCornerData);
                        }
                    }
                }
            }

            if (!instanceIsValid)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"DomainInstance=[{domainInstance}] does not exist in VMinForwarding table.");
            }

            if (allActiveDataForInstance.Count == 0)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"DomainInstance=[{domainInstance}] does not have any active data in the VMinForwarding table.");
            }

            if (allActiveDataForInstance.Count == 1)
            {
                var rec = allActiveDataForInstance[0];
                var recFreqInGhz = rec.Frequency / 1e9;
                if (recFreqInGhz.Equals(frequencyInGhz, 3))
                {
                    Prime.Services.ConsoleService.PrintDebug($"GetVoltage({domainInstance}, {frequencyInGhz}): Frequency matches existing record. Using Voltage=[{rec.Voltage}]");
                    return rec.Voltage;
                }
                else
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"DomainInstance=[{domainInstance}] only has one active vmin at [{recFreqInGhz:0.000}Ghz], Cannot interpolate to [{frequencyInGhz:0.000}Ghz].");
                }
            }

            var targetFreqInHz = frequencyInGhz * 1e9;
            var orderedRecords = allActiveDataForInstance.OrderBy(o => Math.Abs(o.Frequency - targetFreqInHz)).ToList();
            if (targetFreqInHz.Equals(orderedRecords[0].Frequency, 3))
            {
                Prime.Services.ConsoleService.PrintDebug($"GetVoltage({domainInstance}, {frequencyInGhz}): Frequency matches existing record. Using Voltage=[{orderedRecords[0].Voltage}]");
                return orderedRecords[0].Voltage;
            }

            var newVoltage = CalculateVoltageFromLinearFit(orderedRecords[0].Voltage, orderedRecords[0].Frequency, orderedRecords[1].Voltage, orderedRecords[1].Frequency, targetFreqInHz);
            Prime.Services.ConsoleService.PrintDebug($"GetVoltage({domainInstance}, {frequencyInGhz})=[{newVoltage}] From Corner1=[{orderedRecords[0].Voltage}, {orderedRecords[0].Frequency}], Corner1=[{orderedRecords[1].Voltage}, {orderedRecords[1].Frequency}]");
            return newVoltage;
        }

        private static double CalculateVoltageFromLinearFit(double voltage1, double frequency1, double voltage2, double frequency2, double newFrequency)
        {
            if (frequency1.Equals(frequency2, VminMhzFrequencyInMhzPrecisionSignificantBits))
            {
                // TODO: [CalculateVoltageFromLinearFit] when frequencies are equal, FAST just returns voltage1, but what if the voltages aren't the same?
                return voltage1;
            }

            double slope = (voltage1 - voltage2) / (frequency1 - frequency2);
            double offset = voltage1 - (slope * frequency1);
            double newVoltage = (newFrequency * slope) + offset;

            return newVoltage;
        }

        private static void InterpolatorItuffPrint(string tnameAppend, string ituffStr)
        {
            var writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            writer.SetTnamePostfix(tnameAppend);
            writer.SetData(ituffStr);
            Prime.Services.DatalogService.WriteToItuff(writer);
        }

        private string GetVminItuffStr(List<string> domains, bool useSnapShotData)
        {
            var ituffPerDomain = new List<string>(domains.Count);

            foreach (var domain in domains)
            {
                var allInstanceNames = DDG.VminForwarding.Service.GetInstanceNamesForDomain(domain);
                var allCorners = DDG.VminForwarding.Service.GetCornerNamesForDomainInstance(allInstanceNames[0]).Select(c => c.Split('@').Last());
                var ituffDataPerCorner = new List<string>(allCorners.Count());

                foreach (var freqCorner in allCorners)
                {
                    List<VminForwardingCornerData> activeRecords;
                    if (useSnapShotData)
                    {
                        activeRecords = allInstanceNames.Select(instance => DDG.VminForwarding.Service.GetVminForwardingSnapshot($"{instance}@{freqCorner}")).ToList();
                    }
                    else
                    {
                        activeRecords = allInstanceNames.Select(instance => this.GetActiveData(domain, instance, $"{instance}@{freqCorner}")).ToList();
                    }

                    var frequencyInGhz = activeRecords.FirstOrDefault(o => o != null)?.Frequency / 1e9;
                    var vmins = activeRecords.Select(o => o == null ? -9999d : o.Voltage);
                    ituffDataPerCorner.Add($"{frequencyInGhz:0.000}^{string.Join("v", vmins)}");
                }

                ituffPerDomain.Add($"{domain}:" + string.Join("%", ituffDataPerCorner));
            }

            return string.Join("_", ituffPerDomain);
        }

        private void ApplySTCInterpolationToCornersPrime(string domain, string instance, string lowCorner, string highCorner, List<string> corners)
        {
            var snapshotDataHiCorner = DDG.VminForwarding.Service.GetVminForwardingSnapshot(highCorner);
            var snapshotDataLoCorner = DDG.VminForwarding.Service.GetVminForwardingSnapshot(lowCorner);
            if (snapshotDataHiCorner == null || snapshotDataLoCorner == null)
            {
                return;
            }

            var vminDataHiCorner = this.GetActiveData(domain, instance, highCorner); // DDG.VminForwarding.Service.Get(highCorner, (int)flow).GetStartingVoltage(-9999);
            var vminDataLoCorner = this.GetActiveData(domain, instance, lowCorner); // DDG.VminForwarding.Service.Get(lowCorner, (int)flow).GetStartingVoltage(-9999);
            if (vminDataHiCorner == null || vminDataLoCorner == null)
            {
                return;
            }

            var highVminDelta = vminDataHiCorner.Voltage - snapshotDataHiCorner.Voltage;
            var highFreq = vminDataHiCorner.Frequency; // DDG.VminForwarding.Service.GetFrequency(highCorner, (int)flow);
            var lowVminDelta = vminDataLoCorner.Voltage - snapshotDataLoCorner.Voltage;
            var lowFreq = vminDataLoCorner.Frequency; // DDG.VminForwarding.Service.GetFrequency(lowCorner, (int)flow);
            var flow = System.Math.Max(vminDataHiCorner.Flow, vminDataLoCorner.Flow);

            foreach (var corner in corners)
            {
                var cornerData = DDG.VminForwarding.Service.GetVminForwardingSnapshot(corner);
                if (cornerData == null)
                {
                    continue;
                }

                var cornerFreq = cornerData.Frequency;
                var deltaVmin = CalculateVoltageFromLinearFit(lowVminDelta, lowFreq, highVminDelta, highFreq, cornerFreq);
                if (deltaVmin > 0)
                {
                    Prime.Services.ConsoleService.PrintDebug($"Applying Interpolation to Corner={corner} Flow={flow} Vmin+={deltaVmin:0.000}");
                    var cornerObj = DDG.VminForwarding.Service.Get(corner, flow);
                    cornerObj.StoreVminResult(new List<double> { cornerData.Voltage + deltaVmin });
                }
            }
        }

        private VminForwardingCornerData GetActiveData(string domainName, string instanceName, string fullCornerName)
        {
            try
            {
                var record = this.VminTable[domainName][instanceName].Find(o => o.Key == fullCornerName);
                return record.ActiveCornerData;
            }
            catch
            {
                return null;
            }
        }
    }
}
