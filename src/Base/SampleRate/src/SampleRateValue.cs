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

    /// <summary>
    /// Class to resolve input sample rate value from the Shared storage string table or Uservar.
    /// </summary>
    public class SampleRateValue
    {
        /// <summary>
        /// check the user input value reside in uservar or shared storage key, if it is not in either places, it will return the same input value.
        /// </summary>
        /// <returns>Value obtained from shared storage key or uservar.</returns>
        /// <param name="userInputDUTSampleRate">Input sample rate value by the user, This function shared by the DUT sampling and wafer rate sampling.</param>
        public uint ResolveSampleRateValue(string userInputDUTSampleRate)
        {
            uint samplingRateValue;

            if (Prime.Services.SharedStorageService.KeyExistsInIntegerTable(userInputDUTSampleRate, Context.LOT))
            {
                samplingRateValue = (uint)Prime.Services.SharedStorageService.GetIntegerRowFromTable(userInputDUTSampleRate, Context.LOT);
                Prime.Services.ConsoleService.PrintDebug($"Sample rate value obtained from shared storage key {userInputDUTSampleRate}=[{samplingRateValue}].\n");
            }
            else if (Prime.Services.UserVarService.Exists(userInputDUTSampleRate))
            {
                samplingRateValue = (uint)Prime.Services.UserVarService.GetIntValue(userInputDUTSampleRate);
                Prime.Services.ConsoleService.PrintDebug($"Sample rate value obtained from uservar {userInputDUTSampleRate}=[{samplingRateValue}].\n");
            }
            else
            {
                Prime.Services.ConsoleService.PrintDebug($"Sample rate value not found in shared storage or uservar, so obtained from user input is=[{userInputDUTSampleRate}].\n");
                if (string.IsNullOrEmpty(userInputDUTSampleRate))
                {
                    throw new Base.Exceptions.TestMethodException($"SamplingRateValue parameter =[{userInputDUTSampleRate}] cannot be empty.\n");
                }

                try
                {
                    samplingRateValue = System.Convert.ToUInt32(userInputDUTSampleRate);
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e.Message);
                    throw new Base.Exceptions.TestMethodException($"Cannot convert the input sample rate value,incorrect value specified for sampling rate value=[{userInputDUTSampleRate}].\n");
                }
            }

            if (samplingRateValue == 0)
            {
                throw new Base.Exceptions.TestMethodException($"Incorrect value specified for sampling rate value=[{userInputDUTSampleRate}].\n");
            }

            return samplingRateValue;
        }
    }
}
