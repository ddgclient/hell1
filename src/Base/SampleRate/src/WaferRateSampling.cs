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
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.SharedStorageService;
    using Prime.Utilities;

    /// <summary>
    /// WaferRateSampling object.
    /// </summary>
    public class WaferRateSampling : ISampleRate
    {
        /// <summary>
        /// DutSamplingFeature.
        /// </summary>
        private DutSampling dutSamplingFeature;
        private string userInputwaferRateValue;
        private uint currentWaferSampleCount;
        private string currentWaferID;
        private string previousWaferId;
        private uint waferSampleRateValue;
        private SampleRateValue sampleRateValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaferRateSampling"/> class.
        /// </summary>
        /// <param name="userInputwaferRateValue">specifies the rate which the wafer to be processed.</param>
        /// <param name="dutSample">information needed for DUT sampling. </param>
        public WaferRateSampling(string userInputwaferRateValue, DutSampling dutSample)
        {
            this.userInputwaferRateValue = userInputwaferRateValue;
            this.dutSamplingFeature = dutSample;
        }

        /// <summary>
        /// Verify Wafer rate input parameters.
        /// </summary>
        /// <returns>representing the port.</returns>
        public bool Verify()
        {
            this.sampleRateValue = new SampleRateValue();
            this.waferSampleRateValue = this.sampleRateValue.ResolveSampleRateValue(this.userInputwaferRateValue);
            this.currentWaferSampleCount = this.waferSampleRateValue;
            Prime.Services.ConsoleService.PrintDebug($"Wafer sample rate value set to=[{this.waferSampleRateValue}].\n");
            return this.dutSamplingFeature.Verify();
        }

        /// <summary>
        ///  Wafer Rate feature implementation.
        /// </summary>
        /// <returns>int representing the port.</returns>
        public bool Execute()
        {
            Prime.Services.ConsoleService.PrintDebug($"Current wafer sampleCount=[{this.currentWaferSampleCount}].\n");

            this.currentWaferID = Prime.Services.StationControllerService.Get("SC_LOT_WAFER");
            if (string.IsNullOrEmpty(this.currentWaferID))
            {
                throw new Base.Exceptions.TestMethodException($"Wafer sampling is chosen but current wafer Id from SC_LOT_WAFER=[{this.currentWaferID}] is empty.\n");
            }

            Prime.Services.ConsoleService.PrintDebug($"Current wafer Id set to=[{this.currentWaferID}].\n");
            Prime.Services.ConsoleService.PrintDebug($"Previous Wafer Id set to=[{this.previousWaferId}].\n");
            if (string.IsNullOrEmpty(this.previousWaferId))
            {
                this.previousWaferId = string.Copy(this.currentWaferID);
                Prime.Services.ConsoleService.PrintDebug($"Processing the first wafer, wafer Id=[{this.currentWaferID}].\n");
            }
            else
            {
                if (this.currentWaferID != this.previousWaferId)
                {
                    // Processing second or subsequent wafers.
                    this.previousWaferId = this.currentWaferID;
                    Prime.Services.ConsoleService.PrintDebug($"Processing the new wafer, wafer Id=[{this.currentWaferID}].\n");
                    if (this.currentWaferSampleCount == 1)
                    {
                        this.currentWaferSampleCount = this.waferSampleRateValue;
                    }
                    else
                    {
                        --this.currentWaferSampleCount;
                        Prime.Services.ConsoleService.PrintDebug($"Wafer count decremented, current wafer sampleCount=[{this.currentWaferSampleCount}].\n");
                    }
                }
            }

            if (this.currentWaferSampleCount == 1)
            {
                // ensured wafer need to be sampled, now check whether DUT need to be sampled or not. Exit port 1 if need to be sampled or return 2.
                Prime.Services.ConsoleService.PrintDebug("Wafer sampling criteria is successful, now proceeding to DUT Sampling.\n");
                return this.dutSamplingFeature.Execute();
            }
            else
            {
                // No Wafer sampling, exit to port 2 at the end.
                Prime.Services.ConsoleService.PrintDebug("Current wafer not supposed to be sampled, Hence DUT Sampling will not be done.\n");
                return false;
            }
        }
    }
}