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

[assembly: InternalsVisibleTo("DieRecoveryBase.UnitTest")]

namespace DieRecoveryBase
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using DDG;
    using Prime;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;

    /// <summary>
    /// Defines the <see cref="PinMapBase" />.
    /// </summary>
    internal class PinMapBase : IPinMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinMapBase"/> class.
        /// </summary>
        /// <param name="name">Name of the PinMapBase.</param>
        public PinMapBase(string name)
        {
            this.Name = name;
            this.Configurations = this.GetConfiguration(name);
            this.FullSize = this.Configurations.Select(o => o.NumberOfTrackerElements).Sum();
            this.Console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
        }

        /// <summary>
        /// Gets or sets the name of this PinMapBase.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the PinMapBase Configuration objects.
        /// </summary>
        protected List<IPinMapDecoder> Configurations { get; set; }

        /// <summary>
        /// Gets or sets the full number of bits associated with this PinMapBase.
        /// </summary>
        protected int FullSize { get; set; }

        /// <summary>
        /// Gets the Prime.Console service or null based on this instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; } = null;

        /// <inheritdoc/>
        public BitArray DecodeFailure(IFunctionalTest functionalTest, int? currentSlice = null)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                var failingPins = new List<string>();

                // Check if the first failure is in the preamble.
                // TODO: GetFailingPinNames throws an exception when there are no failures, not sure what GetPerCycleFailures() does.
                var captureFailureTest = functionalTest as ICaptureFailureTest;
                if (captureFailureTest != null)
                {
                    try
                    {
                        var failures = captureFailureTest.GetPerCycleFailures();
                        if (failures.Count > 0)
                        {
                            failingPins = captureFailureTest.GetFailingPinNames();

                            // need to check the first failure in every domain to see if its in reset.
                            // TODO: Find a faster way to check for reset failures.
                            Dictionary<string, bool> checkedDomains = new Dictionary<string, bool>();
                            foreach (var failure in failures)
                            {
                                var failDomain = failure.GetDomainName();
                                if (!checkedDomains.ContainsKey(failDomain))
                                {
                                    checkedDomains[failDomain] = true;
                                    var failPattern = failure.GetPatternName();
                                    var failPlist = failure.GetParentPlistName();
                                    if (Services.PlistService.GetPlistObject(failPlist).IsPatternAnAmble(failPattern))
                                    {
                                        this.Console?.PrintDebug($"First Failing Pattern=[{failPattern}] is Amble. Marking all domains as failing.");
                                        return new BitArray(this.FullSize, true);
                                    }
                                }
                            }
                        }
                    }
                    catch (FatalException e)
                    {
                        this.Console?.PrintDebug($"Prime failed on Amble check for first failure. Error={e.Message}");
                    }
                }

                var result = new BitArray(this.FullSize);
                var offset = 0;
                var numFailures = 0;
                foreach (var decoder in this.Configurations)
                {
                    var tracker = decoder.GetFailTrackerFromPlistResults(functionalTest, currentSlice);
                    this.Console?.PrintDebug($"Decoder=[{decoder.Name}] Returned Tracker=[{tracker.ToBinaryString()}].");
                    decoder.SaveResultsToSharedStorage(tracker);

                    for (var i = 0; i < tracker.Length; i++)
                    {
                        result[i + offset] = tracker[i];
                        if (tracker[i])
                        {
                            numFailures++;
                        }
                    }

                    offset += decoder.NumberOfTrackerElements;
                }

                // TODO: updated this but still need a better way to handle pins that are not in the individual decoders.
                if (numFailures == 0 && failingPins.Count > 0)
                {
                    this.Console?.PrintDebug("Detected failing pins, but no decoder failed. This could be due to a reset or global failure. Marking all tracker bits as fail.");
                    result.SetAll(true);
                }

                return result;
            }
        }

        /// <inheritdoc/>
        public BitArray FailTrackerToFailVoltageDomains(BitArray trackerBitArray)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                if (trackerBitArray.Length != this.FullSize)
                {
                    throw new TestMethodException($"PinMapBase.FailTrackerToFailVoltageDomains - TrackerBitArray contains [{trackerBitArray.Length}] but PinMapBase=[{this.Name}] expects [{this.FullSize}].");
                }

                var failDomains = new BitArray(this.Configurations.Count, false);
                var trackerIndex = 0;
                for (var domainIndex = 0; domainIndex < this.Configurations.Count; domainIndex++)
                {
                    for (var i = 0; i < this.Configurations[domainIndex].NumberOfTrackerElements; i++)
                    {
                        // TODO: Initial implementation of FailTrackerToFailVoltageDomains assumes 1 PinMapDecoder Object per Voltage Domain. This is currently required by the VMin template but it might change.
                        if (trackerBitArray[trackerIndex++])
                        {
                            failDomains[domainIndex] = true;
                        }
                    }
                }

                return failDomains;
            }
        }

        /// <inheritdoc/>
        public BitArray VoltageDomainsToFailTracker(BitArray voltageDomainBitArray)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                if (voltageDomainBitArray.Length != this.Configurations.Count)
                {
                    throw new TestMethodException($"PinMapBase.VoltageDomainsToFailTracker - TrackerBitArray contains [{voltageDomainBitArray.Length}] bits but PinMapBase=[{this.Name}] expects [{this.Configurations.Count}].");
                }

                // TODO: Initial implementation of VoltageDomainsToFailTracker assumes 1 PinMapDecoder Object per Voltage Domain. This is currently required by the VMin template but it might change.
                var voltageTargets = new BitArray(this.FullSize, false);
                var index = 0;
                for (var domainIndex = 0; domainIndex < this.Configurations.Count; domainIndex++)
                {
                    for (var i = 0; i < this.Configurations[domainIndex].NumberOfTrackerElements; i++)
                    {
                        voltageTargets[index++] = voltageDomainBitArray[domainIndex];
                    }
                }

                return voltageTargets;
            }
        }

        /// <inheritdoc/>
        public void MaskPins(BitArray mask, ref IFunctionalTest plist, List<string> maskedPins)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                var pins = this.GetPinsToMask(mask, ref plist);
                if (maskedPins != null && maskedPins.Count > 0)
                {
                    pins.AddRange(maskedPins);
                }

                this.Console?.PrintDebug($"PinMapBase.MaskPins[{mask.ToBinaryString()}] - setting mask on pins=[{string.Join(",", pins)}].");
                plist.SetPinMask(pins);
            }
        }

        /// <inheritdoc/>
        public List<string> GetMaskPins(BitArray mask, List<string> maskedPins)
        {
            IFunctionalTest dummyFunctionalTest = null;
            var pins = this.GetPinsToMask(mask, ref dummyFunctionalTest);
            if (maskedPins != null && maskedPins.Count > 0)
            {
                pins.AddRange(maskedPins);
            }

            this.Console?.PrintDebug($"PinMapBase.MaskPins[{mask.ToBinaryString()}] - mask on pins=[{string.Join(",", pins)}].");
            return pins;
        }

        /// <inheritdoc />
        public void ModifyPlist(BitArray mask, ref IFunctionalTest plist)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                var offset = 0;
                foreach (var decoder in this.Configurations)
                {
                    var maskForDecoder = new BitArray(decoder.NumberOfTrackerElements);
                    for (var i = 0; i < maskForDecoder.Length; i++)
                    {
                        maskForDecoder[i] = mask[i + offset];
                    }

                    decoder.ApplyPlistSettings(maskForDecoder, ref plist);
                    offset += decoder.NumberOfTrackerElements;
                }
            }
        }

        /// <inheritdoc />
        public void Restore()
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                foreach (var decoder in this.Configurations)
                {
                    decoder.Restore();
                }
            }
        }

        /// <inheritdoc />
        public void Verify(ref IFunctionalTest plist)
        {
            foreach (var decoder in this.Configurations)
            {
                decoder.Verify(ref plist);
            }
        }

        /// <inheritdoc/>
        public void ApplyPatConfig(BitArray iPConfigBits, string plist)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                if (iPConfigBits.Length != this.FullSize)
                {
                    throw new TestMethodException($"PinMapBase=[{this.Name}] Expecting iPConfigBits to have [{this.FullSize}] bits not [{iPConfigBits.Length}]. iPConfigBits=[{iPConfigBits.ToBinaryString()}].");
                }

                var allPatMods = new List<IPatConfigHandle>();
                var offset = 0;
                foreach (var decoder in this.Configurations)
                {
                    // TODO: Create extension methods for dealing with BitArrays (need slice operation)
                    var bitsForDecoder = new BitArray(decoder.NumberOfTrackerElements);
                    for (int i = 0; i < bitsForDecoder.Length; i++)
                    {
                        bitsForDecoder[i] = iPConfigBits[i + offset];
                    }

                    allPatMods.Add(decoder.GetPatConfigForSliceControl(bitsForDecoder, plist));
                    offset += decoder.NumberOfTrackerElements;
                }

                if (allPatMods.Count > 0)
                {
                    this.Console?.PrintDebug($"PinMapBase.ApplyPatConfig - [{this.Name}] is doing patconfig for [{iPConfigBits.ToBinaryString()}].");
                    Services.PatConfigService.Apply(allPatMods);
                }
            }
        }

        /// <inheritdoc/>
        public void ApplyPatConfig(BitArray iPConfigBits)
        {
            this.ApplyPatConfig(iPConfigBits, plist: string.Empty);
        }

        /// <inheritdoc/>
        public IReadOnlyList<IPinMapDecoder> GetConfiguration()
        {
            return this.Configurations;
        }

        private List<string> GetPinsToMask(BitArray mask, ref IFunctionalTest plist)
        {
            if (mask.Length != this.FullSize)
            {
                throw new TestMethodException($"PinMapBase.MaskPins - mask contains [{mask.Length}] but PinMapBase=[{this.Name}] expects [{this.FullSize}].");
            }

            var pinsToMask = new List<string>();
            var offset = 0;
            foreach (var decoder in this.Configurations)
            {
                // TODO: Create extension methods for dealing with BitArrays (need slice operation)
                var maskForDecoder = new BitArray(decoder.NumberOfTrackerElements);
                for (int i = 0; i < maskForDecoder.Length; i++)
                {
                    maskForDecoder[i] = mask[i + offset];
                }

                foreach (var pin in decoder.MaskPlistFromTracker(maskForDecoder, ref plist))
                {
                    if (!pinsToMask.Contains(pin))
                    {
                        pinsToMask.Add(pin);
                    }
                }

                offset += decoder.NumberOfTrackerElements;
            }

            return pinsToMask;
        }

        private List<IPinMapDecoder> GetConfiguration(string name)
        {
            var matchingConfigs = new List<IPinMapDecoder>();
            try
            {
                matchingConfigs.AddRange(name.Split(',').Select(DieRecovery.Utilities.RetrievePinMapDecoder));

                return matchingConfigs;
            }
            catch (Exception e)
            {
                throw new TestMethodException($"Problem extracting PinMapBase Configuration from SharedStorage. DieRecoveryInitTC probably has not been run or PinMap does not exist. Error=${e.Message}]");
            }
        }
    }
}
