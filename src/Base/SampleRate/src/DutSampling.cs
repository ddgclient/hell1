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

namespace Prime.TestMethods.SampleRate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Prime.SharedStorageService;
    using Prime.TestMethods;

    /// <summary>
    /// DUT Sampling.
    /// </summary>
    public class DutSampling : ISampleRate
    {
        /// <summary>
        /// SampleRate value for DUT sampling given by the user, This can be uservar or shared key.
        /// </summary>
        private readonly string userInputDUTSampleRate;

        // Introduced to avoid the casting each time

        /// <summary>
        /// SampleRate Value for DUT sampling.
        /// </summary>
        private uint sampleRateValue;

        /// <summary>
        /// Indicates how many units went through this particular instance.
        /// </summary>
        private uint currentInstanceSampleCount;

        private SampleRateValue sampleRateInputValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DutSampling"/> class.
        /// </summary>
        /// <param name="dutSampleCount">The lot to set.</param>
        public DutSampling(string dutSampleCount)
        {
            this.userInputDUTSampleRate = dutSampleCount;
        }

        /// <summary>
        ///  Verifies the DUT sampling feature.
        /// </summary>
        /// <returns>representing the port.</returns>
        public bool Verify()
        {
            this.sampleRateInputValue = new SampleRateValue();
            this.sampleRateValue = this.sampleRateInputValue.ResolveSampleRateValue(this.userInputDUTSampleRate);
            this.currentInstanceSampleCount = this.sampleRateValue;
            Prime.Services.ConsoleService.PrintDebug($"SamplingRateValue parameter set to=[{this.sampleRateInputValue}].\n");
            return true;
        }

        /// <summary>
        /// DUT Sampling feature to determine whether the unit to be processed or not.
        /// </summary>
        /// <returns>representing the port.</returns>
        public bool Execute()
        {
            if (this.currentInstanceSampleCount == 1)
            {
                Prime.Services.ConsoleService.PrintDebug($"DUT sampling criteria is successful, DUT will be sampled(No skip).\n");

                // reset back to original value
                this.currentInstanceSampleCount = this.sampleRateValue;
                Prime.Services.ConsoleService.PrintDebug($"Current DUT count after reset=[{this.currentInstanceSampleCount}].\n");
                return true;
            }
            else
            {
                Prime.Services.ConsoleService.PrintDebug($"DUT sampling criteria is not successful, DUT will be skipped.\n");
                --this.currentInstanceSampleCount;
                Prime.Services.ConsoleService.PrintDebug($"Current DUT count after decrement=[{this.currentInstanceSampleCount}].\n");
                return false;
            }
        }
    }
}