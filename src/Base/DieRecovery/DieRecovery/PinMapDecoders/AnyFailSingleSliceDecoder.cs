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
    using DDG;
    using Newtonsoft.Json;
    using Prime;
    using Prime.Base.Exceptions;
    using Prime.FunctionalService;

    /// <summary>
    /// Defines the <see cref="AnyFailSingleSliceDecoder" />.
    /// This is meant to be used for SCAN decoding, where each core is tested
    /// separately and any failure on a set of pins means that core (and only
    /// that core) should be marked as a failure.
    /// </summary>
    public class AnyFailSingleSliceDecoder : PinMapDecoderBase, IPinMapDecoder
    {
        /// <summary>
        /// Gets or sets the raw list of pins in this PinMap.
        /// </summary>
        [JsonProperty("PinList")]
        public List<string> PinList { get; set; }

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

            return this.GetFailTrackerFromFailPins(failingPins, currentSlice);
        }

        /// <inheritdoc/>
        public override List<string> MaskPlistFromTracker(BitArray mask, ref IFunctionalTest plist)
        {
            // this function isn't supported for this type of decoder (yet?)
            this.Console?.PrintDebug($"MaskPlistFromTracker - Decoder=[{this.Name}] Type=[{this.GetType()}] does not support masking pins.");
            return new List<string>();
        }

        private BitArray GetFailTrackerFromFailPins(List<string> failingPins, int? currentSlice = null)
        {
            if (currentSlice == null)
            {
                if (this.NumberOfTrackerElements == 1)
                {
                    currentSlice = 0;
                }
                else
                {
                    throw new ArgumentException($"PinMapDecoder=[{this.Name}] requires the currentSlice argument for function=[GetFailTrackerFromPlistResults].", nameof(currentSlice));
                }
            }

            int slice = (int)currentSlice;
            if (slice >= this.NumberOfTrackerElements)
            {
                throw new ArgumentException($"CurrentSlice=[{slice}] is invalid. PinMapDecoder=[{this.Name}] only supports [{this.NumberOfTrackerElements}] elements for function=[GetFailTrackerFromPlistResults].", nameof(currentSlice));
            }

            var result = new BitArray(this.NumberOfTrackerElements);

            if (this.PinList == null || this.PinList.Count == 0)
            {
                result[slice] = failingPins.Count > 0;
                return result;
            }

            foreach (var pin in failingPins)
            {
                if (this.PinList.Contains(pin))
                {
                    result[slice] = true;
                    return result;
                }
            }

            return result;
        }
    }
}
