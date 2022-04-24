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

namespace DDG
{
    using System.Collections.Generic;
    using DTSBase;

    /// <summary>
    /// Wrapper for DTS post-processing.
    /// </summary>
    public class DTSHandler
    {
        private Dictionary<string, List<double>> dtsValues = new Dictionary<string, List<double>>();
        private IDTS dts;
        private Configuration configuration;

        /// <summary>
        /// Sets DTS configuration pointer.
        /// </summary>
        /// <param name="configuration">Configuration name.</param>
        public void SetConfiguration(string configuration)
        {
            this.dts = null;
            this.configuration = null;
            if (!string.IsNullOrEmpty(configuration))
            {
                this.dts = DDG.DTS.Service.Get(configuration);
                this.configuration = this.dts.GetSettings();
            }
        }

        /// <summary>
        /// Indicates isf DTS capture mode is enabled.
        /// </summary>
        /// <returns>true when enabled.</returns>
        public bool IsDtsEnabled()
        {
            return this.configuration != null && this.configuration.IsEnabled;
        }

        /// <summary>
        /// Get Count of CTVs per DTS pattern.
        /// </summary>
        /// <returns>Number of CTVs per pattern.</returns>
        public int GetCtvCount()
        {
            return this.configuration.RegisterSize * this.configuration.SensorsList.Count;
        }

        /// <summary>
        /// Gets CTV capture pin.
        /// </summary>
        /// <returns>Pin name.</returns>
        public string GetCtvPinName()
        {
            return this.configuration.PinName;
        }

        /// <summary>
        /// Resets captured values.
        /// </summary>
        public void Reset()
        {
            this.dtsValues = new Dictionary<string, List<double>>();
        }

        /// <summary>
        /// Process plist settings for DTS modes.
        /// </summary>
        /// <param name="ctvData"> CtvStringToProcess.</param>
        public void ProcessPlistDts(string ctvData)
        {
            if (!this.IsDtsEnabled())
            {
                return;
            }

            this.dts.GetValues(ctvData, ref this.dtsValues);
        }

        /// <summary>
        /// Evaluates DTS limits.
        /// </summary>
        /// <returns>true when passing.</returns>
        public bool EvaluateLimits()
        {
            if (!this.IsDtsEnabled())
            {
                return true;
            }

            this.dts.PrintToDatalog(this.dtsValues);
            return this.dts.EvaluateLimits(this.dtsValues);
        }
    }
}
