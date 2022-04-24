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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using DDG;
    using Prime;
    using Prime.Base.Exceptions;

    /// <summary>
    /// Main DieRecovery Class.
    /// </summary>
    public class DieRecoveryTracker : IDieRecovery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DieRecoveryTracker"/> class.
        /// </summary>
        /// <param name="trackingStructureName">Name of the Tracker to load.</param>
        public DieRecoveryTracker(string trackingStructureName)
            : this(trackingStructureName.Split(',').ToList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DieRecoveryTracker"/> class.
        /// </summary>
        /// <param name="trackingStructureNames">List of names of the Trackers to load.</param>
        public DieRecoveryTracker(List<string> trackingStructureNames)
        {
            this.TrackerDefinitions = new List<Tracker>(trackingStructureNames.Count);
            this.Size = 0;
            this.Names = string.Join(",", trackingStructureNames);
            this.ResetValue = string.Empty;

            try
            {
                foreach (var trackerName in trackingStructureNames)
                {
                    var tracker = DieRecovery.Utilities.RetrieveTrackerDefinition(trackerName);
                    this.TrackerDefinitions.Add(tracker);
                    this.Size += tracker.Size;
                    this.ResetValue += tracker.InitialValue;
                }
            }
            catch (KeyNotFoundException e)
            {
                Services.ConsoleService.PrintError($"Error, Some trackers in [{string.Join(",", trackingStructureNames)} do not exists.\n{e.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public string ResetValue { get; }

        private List<Tracker> TrackerDefinitions { get; }

        private int Size { get; }

        private string Names { get; }

        /// <inheritdoc/>
        public BitArray GetMaskBits()
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                return this.GetTrackerData().ToBitArray();
            }
        }

        /// <inheritdoc/>
        public BitArray GetMaskBits(InputType inputType, string inputName)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                var strMask = DataResolver.ResolveInput(inputType, inputName);
                BitArray bitMask = new BitArray(strMask.Length, false);
                for (var i = 0; i < strMask.Length; i++)
                {
                    if (strMask[i] == '1')
                    {
                        bitMask[i] = true;
                    }
                }

                return bitMask;
            }
        }

        /// <inheritdoc/>
        public string GetNames()
        {
            return this.Names.Replace(",", "|");
        }

        /// <inheritdoc/>
        public List<DefeatureRule.Rule> RunRule(string rule)
        {
            return this.RunRule(this.GetTrackerData().ToBitArray(), rule);
        }

        /// <inheritdoc/>
        public List<DefeatureRule.Rule> RunRule(BitArray bitmask, string rule)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                // pull the rule out of storage.
                var defeatureRule = DefeatureRule.GetDefeatureRule(rule);

                // execute the rule and return the passing configurations.
                return defeatureRule.GetPassingRules(bitmask);
            }
        }

        /// <inheritdoc/>
        public bool UpdateTrackingStructure(BitArray value, BitArray mask = null, BitArray result = null, UpdateMode mode = UpdateMode.Merge, bool log = true)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                BitArray currentValue;
                bool currentValueValid;

                try
                {
                    currentValue = this.GetTrackerData().ToBitArray();
                    currentValueValid = true;
                }
                catch
                {
                    currentValue = new BitArray(this.Size, false);
                    currentValueValid = false;
                }

                if (mask == null)
                {
                    mask = new BitArray(currentValue.Length, false);
                }

                var valueToWrite = new BitArray(currentValue.Length, false);
                for (var i = 0; i < currentValue.Length; i++)
                {
                    valueToWrite[i] = mask[i] ? currentValue[i] : value[i] | (currentValue[i] & mode == UpdateMode.Merge);
                }

                // Log the update
                if (log)
                {
                    var testResult = result ?? value;
                    this.LogTrackingStructure(mask, testResult, currentValue, valueToWrite);
                }

                // Verify the update is needed and allowed.
                List<Tracker> disabledTrackers;
                if (!currentValueValid)
                {
                    disabledTrackers = this.SetTrackerData(valueToWrite, currentValue);
                }
                else if (currentValue.ToBinaryString() != valueToWrite.ToBinaryString())
                {
                    if (DieRecovery.Service.AreTrackerChangesAllowed())
                    {
                        disabledTrackers = this.SetTrackerData(valueToWrite, currentValue);
                    }
                    else
                    {
                        Services.ConsoleService.PrintError($"DieRecovery.UpdateTrackingStructure AllowDownBinFlag=[false] currentValue=[{currentValue.ToBinaryString()}] newValue=[{valueToWrite.ToBinaryString()}]");
                        return false;
                    }
                }
                else
                {
                    // values are the same, no need to update.
                    return true;
                }

                // For any trackers that were newly disabled, check them for Linked trackers to also disable.
                var disabledTrackerNames = disabledTrackers.Select(o => o.Name).ToList();
                foreach (var tracker in disabledTrackers)
                {
                    foreach (var linkedTrackerName in tracker.LinkOnDisable)
                    {
                        if (disabledTrackerNames.Contains(linkedTrackerName))
                        {
                            continue;
                        }

                        var linkObj = new DieRecoveryTracker(linkedTrackerName);
                        var alreadyDisabled = DieRecovery.Utilities.HasTrackerData(linkedTrackerName) && !linkObj.GetMaskBits().Cast<bool>().Contains(false);
                        if (!alreadyDisabled && !linkObj.UpdateTrackingStructure(new BitArray(linkObj.Size, true), mode: UpdateMode.OverWrite))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        /// <inheritdoc/>
        public bool UpdateTrackingStructure(InputType inputType, string inputName, BitArray mask = null, BitArray result = null, UpdateMode mode = UpdateMode.Merge, bool log = true)
        {
            var input = this.GetMaskBits(inputType, inputName);
            return this.UpdateTrackingStructure(input, mask, result, mode, log);
        }

        /// <inheritdoc/>
        public bool UpdateTrackingStructure(List<double> voltages, BitArray mask = null, BitArray result = null, UpdateMode mode = UpdateMode.Merge, double vminLimitLow = 0, double vminLimitHigh = 100, bool log = true)
        {
            BitArray input = new BitArray(voltages.Count, true);
            for (var i = 0; i < voltages.Count; i++)
            {
                if (vminLimitLow <= voltages[i] && voltages[i] <= vminLimitHigh)
                {
                    input[i] = false;
                }
            }

            return this.UpdateTrackingStructure(input, mask, result, mode, log);
        }

        /// <inheritdoc/>
        public void LogTrackingStructure(BitArray mask, BitArray result)
        {
            var currentValue = this.GetTrackerData().ToBitArray();
            this.LogTrackingStructure(mask, result, currentValue, currentValue);
        }

        private void LogTrackingStructure(BitArray mask, BitArray result, BitArray incoming, BitArray outgoing)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                var writer = Services.DatalogService.GetItuffStrgvalWriter();
                writer.SetTnamePostfix($"::{this.GetNames()}");
                writer.SetData($"Mask:b{mask.ToBinaryString()}|TestResult:b{result.ToBinaryString()}|Incoming:b{incoming.ToBinaryString()}|Outgoing:b{outgoing.ToBinaryString()}");
                Services.DatalogService.WriteToItuff(writer);
            }
        }

        private string GetTrackerData()
        {
            try
            {
                var data = this.TrackerDefinitions.Select(tracker => DieRecovery.Utilities.RetrieveTrackerData(tracker.Name)).ToList();
                return string.Join(string.Empty, data);
            }
            catch (Exception)
            {
                throw new TestMethodException($"ERROR: Tracker=[{this.Names}] has not been initialized yet.");
            }
        }

        private List<Tracker> SetTrackerData(BitArray data, BitArray previousValue)
        {
            if (data.Length != this.Size)
            {
                throw new ArgumentException($"Wrong size. Expected Bits=[{this.Size}] Actual=[{data.Length}].", nameof(data));
            }

            var offset = 0;
            var disabledTrackers = new List<Tracker>();
            foreach (var tracker in this.TrackerDefinitions)
            {
                var size = tracker.Size;
                var newData = data.Slice(offset, size);
                var newlyDisabled = previousValue.Slice(offset, size).Cast<bool>().Contains(false) && !newData.Cast<bool>().Contains(false);
                DieRecovery.Utilities.StoreTrackerData(tracker.Name, newData.ToBinaryString());
                offset += size;

                if (newlyDisabled)
                {
                    disabledTrackers.Add(tracker);
                }
            }

            return disabledTrackers;
        }
    }
}
