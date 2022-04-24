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
    using System.Reflection;
    using System.Runtime.Serialization;
    using DDG;
    using Newtonsoft.Json;
    using Prime;
    using Prime.Base.Exceptions;
    using Prime.FunctionalService;

    /// <summary>
    /// Defines the <see cref="PinToSliceIndexDecoder" />.
    /// This is meant to be used for per-core testing like SBFT where each core maps
    /// to a different pin, and any failure on that pin means the corresponding core fails.
    /// It can support mapping multiple pins to a single core or a single pin to one or more cores.
    /// </summary>
    public class PinToSliceIndexDecoder : PinMapDecoderBase, IPinMapDecoder
    {
        /// <summary>
        /// Gets or sets the raw Pin to Slice Bit mapping.
        /// </summary>
        [JsonProperty("PinToSliceIndexMap")]
        public Dictionary<string, List<int>> PinToSliceIndexMap { get; set; }

        /// <summary>
        /// Gets or sets the DOA Pins. PinMasking gets skipped.
        /// </summary>
        [JsonProperty("DoaPins")]
        public List<string> DoaPins { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the Slice Bit to Pin mapping.
        /// TODO: SharedStorage can't handle Dictionaries with Keys=Int ... file a bug.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, List<string>> SliceIndexToPinMap { get; set; }

        /// <inheritdoc />
        public override string GetDecoderType()
        {
            return this.GetType().Name;
        }

        /// <inheritdoc/>
        public override BitArray GetFailTrackerFromPlistResults(IFunctionalTest functionalTest, int? currentSlice = null)
        {
            var captureFailureTest = functionalTest as ICaptureFailureTest;
            if (captureFailureTest == null)
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: unable to cast IFunctionalTest into ICaptureFailureTest object. Using incorrect input type for this decoder.");
            }

            List<string> failingPins;
            try
            {
                // TODO: GetFailingPinNames throws an exception when there are no failures, find a better way to check for this.
                failingPins = captureFailureTest.GetFailingPinNames();
            }
            catch (FatalException)
            {
                failingPins = new List<string>();
            }

            return this.GetFailTrackerFromFailPins(failingPins);
        }

        /// <inheritdoc/>
        public override List<string> MaskPlistFromTracker(BitArray mask, ref IFunctionalTest plist)
        {
            if (mask.Length != this.NumberOfTrackerElements)
            {
                Services.ConsoleService.PrintError($"MaskPlistFromTracker[{this.Name}] - mask contains [{mask.Length}] bits but decoder expects [{this.NumberOfTrackerElements}].");
                throw new ArgumentException($"MaskPlistFromTracker[{this.Name}] - mask contains [{mask.Length}] bits but decoder expects [{this.NumberOfTrackerElements}].", nameof(mask));
            }

            List<string> pinsToMask = new List<string>();
            for (var i = 0; i < mask.Length; i++)
            {
                if (mask[i])
                {
                    string iAsStr = $"{i}";
                    if (this.SliceIndexToPinMap.ContainsKey(iAsStr))
                    {
                        foreach (var pin in this.SliceIndexToPinMap[iAsStr])
                        {
                            if (this.DoaPins.Contains(pin))
                            {
                                continue;
                            }

                            if (!pinsToMask.Contains(pin))
                            {
                                pinsToMask.Add(pin);
                            }
                        }
                    }
                }
            }

            return pinsToMask;
        }

        /// <summary>
        /// Helper function reconstruct non-serialized structures.
        /// </summary>
        /// <param name="context"><see cref="StreamingContext"/> object.</param>
        [OnDeserialized]
        internal void ReBuildAfterDeserialized(StreamingContext context)
        {
            this.SliceIndexToPinMap = new Dictionary<string, List<string>>();
            foreach (var pin in this.PinToSliceIndexMap.Keys)
            {
                foreach (var index in this.PinToSliceIndexMap[pin])
                {
                    string indexAsStr = $"{index}";
                    if (!this.SliceIndexToPinMap.ContainsKey(indexAsStr))
                    {
                        this.SliceIndexToPinMap[indexAsStr] = new List<string>();
                    }

                    if (!this.SliceIndexToPinMap[indexAsStr].Contains(pin))
                    {
                        this.SliceIndexToPinMap[indexAsStr].Add(pin);
                    }
                }
            }
        }

        private BitArray GetFailTrackerFromFailPins(List<string> failingPins)
        {
            var result = new BitArray(this.NumberOfTrackerElements);

            foreach (var pin in failingPins)
            {
                if (this.PinToSliceIndexMap.ContainsKey(pin))
                {
                    foreach (var sliceIndex in this.PinToSliceIndexMap[pin])
                    {
                        result[sliceIndex] = true;
                    }
                }
            }

            return result;
        }
    }
}
