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
    using DDG;
    using Newtonsoft.Json;
    using Prime;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Defines the <see cref="PinMapDecoderBase" />.
    /// </summary>
    public class PinMapDecoderBase : IPinMapDecoder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinMapDecoderBase"/> class.
        /// </summary>
        public PinMapDecoderBase()
        {
            this.Console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
        }

        /// <inheritdoc/>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <inheritdoc/>
        [JsonProperty("PatternModify")]
        public string IpPatternConfigure { get; set; }

        /// <inheritdoc/>
        [JsonProperty("Size")]
        public int NumberOfTrackerElements { get; set; }

        /// <inheritdoc/>
        [JsonProperty("SharedStorageResults")]
        public string SharedStorageResults { get; set; }

        /// <summary>
        /// Gets a variable to hold the Prime.Console service or null based on this instances LogLevel.
        /// </summary>
        [JsonIgnore]
        protected IConsoleService Console { get; }

        [JsonIgnore]
        private Dictionary<string, Dictionary<string, IPatConfigHandle>> PatConfigCache { get; set; }

        /// <inheritdoc />
        public virtual string GetDecoderType()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual BitArray GetFailTrackerFromPlistResults(IFunctionalTest functionalTest, int? currentSlice = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual List<string> MaskPlistFromTracker(BitArray mask, ref IFunctionalTest plist)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IPatConfigHandle GetPatConfigForSliceControl(BitArray iPConfigBits, string patlist)
        {
            var data = iPConfigBits.ToBinaryString();
            var handle = this.GetPatConfigFromCache(this.IpPatternConfigure, patlist);
            handle.SetData(data);
            return handle;
        }

        /// <inheritdoc />
        public virtual void ApplyPlistSettings(BitArray mask, ref IFunctionalTest plist)
        {
        }

        /// <inheritdoc />
        public virtual void Restore()
        {
        }

        /// <inheritdoc />
        public virtual void SaveResultsToSharedStorage(BitArray results)
        {
            if (!string.IsNullOrEmpty(this.SharedStorageResults))
            {
                Prime.Services.SharedStorageService.InsertRowAtTable(this.SharedStorageResults, results.ToBinaryString(), Context.DUT);
            }
        }

        /// <inheritdoc />
        public virtual void Verify(ref IFunctionalTest plist)
        {
        }

        /// <summary>
        /// Gets a PatConfigHandle for the given PatConfig and Patlist, either creates a new copy or returns a previously cached copy.
        /// </summary>
        /// <param name="config">PatConfig name.</param>
        /// <param name="patlist">Plist - can be null.</param>
        /// <returns>IPatConfigHandle for the given config and patlist.</returns>
        protected IPatConfigHandle GetPatConfigFromCache(string config, string patlist)
        {
            if (this.PatConfigCache != null && this.PatConfigCache.ContainsKey(config) && this.PatConfigCache[config] != null && this.PatConfigCache[config].ContainsKey(patlist))
            {
                return this.PatConfigCache[config][patlist];
            }

            var patConfigHandle = string.IsNullOrWhiteSpace(patlist) ? Services.PatConfigService.GetPatConfigHandle(config) : Services.PatConfigService.GetPatConfigHandleWithPlist(config, patlist);

            if (this.PatConfigCache == null)
            {
                this.PatConfigCache = new Dictionary<string, Dictionary<string, IPatConfigHandle>>(1);
            }

            if (!this.PatConfigCache.ContainsKey(config) || this.PatConfigCache[config] == null)
            {
                this.PatConfigCache[config] = new Dictionary<string, IPatConfigHandle>(1);
            }

            this.PatConfigCache[config][patlist] = patConfigHandle;
            return patConfigHandle;
        }
    }
}
