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
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.Utilities;

    /// <summary>
    /// WaferListSampling Rate Interface.
    /// </summary>
    public class WaferListSampling : ISampleRate
    {
        /// <summary>
        /// DutSamplingFeature.
        /// </summary>
        private readonly DutSampling dutSamplingFeature;
        private readonly string userInputWaferList;
        private List<string> allWaferNameList;
        private string currentWaferId;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaferListSampling"/> class.
        /// </summary>
        /// <param name="waferList">The lot to set.</param>
        /// <param name="dutSample">information needed for DUT sampling. </param>
        public WaferListSampling(string waferList, DutSampling dutSample)
        {
            this.userInputWaferList = waferList;
            this.dutSamplingFeature = dutSample;
        }

        /// <summary>
        /// Verifies theWafer list feature input parameter.
        /// </summary>
        /// <returns>representing the port.</returns>
        public bool Verify()
        {
            if (string.IsNullOrEmpty(this.userInputWaferList))
            {
                Prime.Services.ConsoleService.PrintDebug("User input is empty, checking SC_WAFER_LIST has valid values.\n");
                string scWaferList = Prime.Services.StationControllerService.Get("SC_WAFER_LIST");
                if (string.IsNullOrEmpty(scWaferList))
                {
                    Prime.Services.ConsoleService.PrintError("Wafer sample list option specified, but no wafer list provided and SC_WAFER_LIST is empty.\n");
                    return false;
                }

                this.allWaferNameList = scWaferList.Split(',').ToList();
            }
            else
            {
                this.allWaferNameList = this.userInputWaferList.Split(',').ToList();
            }

            return this.dutSamplingFeature.Verify();
        }

        /// <summary>
        /// Wafer list feature.
        /// </summary>
        /// <returns>int representing the port.</returns>
        public bool Execute()
        {
            this.currentWaferId = Prime.Services.StationControllerService.Get("SC_LOT_WAFER");
            if (string.IsNullOrEmpty(this.currentWaferId))
            {
                throw new Base.Exceptions.TestMethodException(
                    $"Wafer sampling is chosen but current Wafer Id from SC_LOT_WAFER=[{this.currentWaferId}] is empty.\n");
            }

            Prime.Services.ConsoleService.PrintDebug($"Current wafer Id from SC_LOT_WAFER set to=[{this.currentWaferId}].\n");

            if (this.allWaferNameList.Contains(this.currentWaferId))
            {
                Prime.Services.ConsoleService.PrintDebug(
                    $"Wafer sampling criteria is successful now proceeding to DUT sampling.\n");
                return this.dutSamplingFeature.Execute();
            }
            else
            {
                Prime.Services.ConsoleService.PrintDebug(
                    $"Current wafer Id=[{this.currentWaferId}] is not matching with user input waferList or SC_WAFER_LIST.\n");
                return false;
            }
        }
    }
}