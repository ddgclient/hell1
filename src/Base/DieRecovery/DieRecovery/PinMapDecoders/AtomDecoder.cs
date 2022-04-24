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
    using System.Text.RegularExpressions;
    using DDG;
    using Newtonsoft.Json;
    using Prime;
    using Prime.FunctionalService;
    using Prime.PatConfigService;

    /// <summary>
    /// Defines the <see cref="ContentType" />.
    /// This is meant to be used to define the ContentType for which AtomDecoder is configured.
    /// </summary>
    internal enum ContentType
    {
        /// <summary>
        /// Array content
        /// </summary>
        Array,

        /// <summary>
        /// Functional content
        /// </summary>
        Func,
    }

    /// <summary>
    /// Defines the <see cref="AtomDecoder" />.
    /// This is meant to be used for Atom decoding, where each module
    /// contains multiple cores and all module output comes out on the same pin.
    /// </summary>
    public class AtomDecoder : PinMapDecoderBase, IPinMapDecoder
    {
        private ContentType contentType;

        /// <inheritdoc cref="PinMapDecoderBase"/>
        public AtomDecoder(string name, string pin, uint module, string content, string patternModifyUniq = null, int numberOfCores = 4, bool reverse = false)
            : base()
        {
            this.Name = name;
            this.Pin = pin;
            this.Module = module;
            this.NumberOfTrackerElements = numberOfCores;
            this.Content = content;
            this.PatternModifyUniq = patternModifyUniq;
            this.Reverse = reverse;
        }

        /// <inheritdoc cref="PinMapDecoderBase"/>
        public AtomDecoder()
            : base()
        {
        }

        /// <summary>
        /// Gets or sets the Pin of this PlistDecoder object.
        /// </summary>
        [JsonProperty("Pin")]
        public string Pin { get; set; }

        /// <summary>
        /// Gets or sets the Module of this PlistDecoder object.
        /// </summary>
        [JsonProperty("Module")]
        public uint Module { get; set; }

        /// <summary>
        /// Gets or sets the content type that is being used with this PlistDecoder object.
        /// </summary>
        [JsonProperty("Content")]
        public string Content
        {
            get
            {
                switch (this.contentType)
                {
                    case ContentType.Array:
                        return "ARRAY";
                    case ContentType.Func:
                        return "FUNC";
                    default:
                        throw new InvalidOperationException($"Content type {this.contentType} is not valid.");
                }
            }

            set
            {
                switch (value.ToUpper())
                {
                    case "ARRAY":
                        this.contentType = ContentType.Array;
                        break;
                    case "FUNC":
                        this.contentType = ContentType.Func;
                        break;
                    default:
                        throw new ArgumentException(value + " is not a valid content type. Valid content types are 'ARRAY' and 'FUNC'.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the string to uniquify patmod names.
        /// </summary>
        [JsonProperty("PatternModifyUniq")]
        public string PatternModifyUniq { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the core to BitArray index mapping is reversed.
        /// </summary>
        [JsonProperty("Reverse")]
        public bool Reverse { get; set; } = false;

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is AtomDecoder decoder &&
                   this.Name == decoder.Name &&
                   this.IpPatternConfigure == decoder.IpPatternConfigure &&
                   this.NumberOfTrackerElements == decoder.NumberOfTrackerElements &&
                   this.contentType == decoder.contentType &&
                   this.Pin == decoder.Pin &&
                   this.Module == decoder.Module &&
                   this.PatternModifyUniq == decoder.PatternModifyUniq &&
                   this.Reverse == decoder.Reverse;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1533541845;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(this.Name);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(this.IpPatternConfigure);
            hashCode = (hashCode * -1521134295) + this.NumberOfTrackerElements.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.contentType.GetHashCode();
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(this.Pin);
            hashCode = (hashCode * -1521134295) + this.Module.GetHashCode();
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(this.PatternModifyUniq);
            hashCode = (hashCode * -1521134295) + this.Reverse.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc />
        public override string GetDecoderType()
        {
            return this.GetType().Name;
        }

        /// <inheritdoc/>
        public override BitArray GetFailTrackerFromPlistResults(IFunctionalTest functionalTest, int? currentSlice = null)
        {
            var result = new BitArray(this.NumberOfTrackerElements, false);
            var captureFailTest = functionalTest as ICaptureFailureTest;
            if (captureFailTest == null)
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: unable to cast IFunctionalTest into ICaptureFailureTest object. Using incorrect input type for this decoder.");
            }

            foreach (IFailureData failure in captureFailTest.GetPerCycleFailures())
            {
                bool allFail = true;
                BitArray currResult = this.GetFailTrackerFromFailData(failure);
                for (int i = 0; i < this.NumberOfTrackerElements; i++)
                {
                    result[i] |= currResult[i];
                    allFail &= result[i];
                }

                if (allFail)
                {
                    return result;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override List<string> MaskPlistFromTracker(BitArray mask, ref IFunctionalTest plist)
        {
            var completeFailure = true;
            var patConfigHandleList = new List<IPatConfigHandle>();

            string content = string.Empty;
            switch (this.contentType)
            {
                case ContentType.Array:
                    content = "array";
                    break;
                case ContentType.Func:
                    content = "func";
                    break;
            }

            string uniquify = string.Empty;
            if (this.PatternModifyUniq != null)
            {
                uniquify = $"_{this.PatternModifyUniq}";
            }

            for (int core = 0; core < mask.Length; core++)
            {
                int bitArrayIndex;
                if (this.Reverse)
                {
                    bitArrayIndex = this.NumberOfTrackerElements - (core + 1);
                }
                else
                {
                    bitArrayIndex = core;
                }

                string patmod;
                if (mask[bitArrayIndex])
                {
                    patmod = $"atom_{content}{uniquify}_m{this.Module}_c{core}_mask";
                }
                else
                {
                    completeFailure = false;
                    patmod = $"atom_{content}{uniquify}_m{this.Module}_c{core}_restore";
                }

                patConfigHandleList.Add(this.GetPatConfigFromCache(patmod, string.Empty));
            }

            if (completeFailure)
            {
                return new List<string> { this.Pin };
            }

            Services.PatConfigService.Apply(patConfigHandleList);
            return new List<string>();
        }

        /// <inheritdoc/>
        public override IPatConfigHandle GetPatConfigForSliceControl(BitArray iPConfigBits, string patlist)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generate fail tracker based off of IFailData.
        /// </summary>
        /// <param name="failure"><see cref="IFailureData"/>.</param>
        /// <returns>A tracker.</returns>
        internal BitArray GetFailTrackerFromFailData(IFailureData failure)
        {
            if (failure.GetFailingPinNames().Contains(this.Pin))
            {
                var label = failure.GetPreviousLabel();
                string pattern = @"(\d+)";
                switch (this.contentType)
                {
                    case ContentType.Array:
                        pattern = @"CORE(\d+)";
                        break;
                    case ContentType.Func:
                        pattern = @"C(\d+)";
                        break;
                }

                var labelMatch = Regex.Match(label, pattern);

                if (labelMatch.Success)
                {
                    this.Console?.PrintDebug("Core failure with label \"" + label + "\". Setting fail state for core " + labelMatch.Groups[1].Value + ".");
                    var result = new BitArray(this.NumberOfTrackerElements, false);
                    if (this.Reverse)
                    {
                        result.Set(this.NumberOfTrackerElements - (int.Parse(labelMatch.Groups[1].Value) + 1), true);
                    }
                    else
                    {
                        result.Set(int.Parse(labelMatch.Groups[1].Value), true);
                    }

                    return result;
                }

                this.Console?.PrintDebug("Non-core failure with label \"" + label + "\". Setting fail state for all cores.");
                return new BitArray(this.NumberOfTrackerElements, true);
            }

            return new BitArray(this.NumberOfTrackerElements, false);
        }
    }
}
