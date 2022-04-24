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
    using DDG;
    using Prime;
    using Prime.SharedStorageService;

    /// <summary>
    /// Defines the <see cref="DataResolver" />.
    /// </summary>
    public static class DataResolver
    {
        /// <summary>
        /// Return the Value stored at inputName.
        /// </summary>
        /// <param name="inputType">Type of variable=<see cref="InputType"/>.</param>
        /// <param name="inputName">Name of variable.</param>
        /// <returns>string value.</returns>
        public static string ResolveInput(InputType inputType, string inputName)
        {
            switch (inputType)
            {
                case InputType.Literal:
                    return inputName;
                case InputType.Gsds:
                    return Convert.ToString(Gsds.ReadToken(inputName));
                case InputType.SharedStorage:
                    return ReadSharedStorageAsString(inputName);
                default:
                    throw new NotImplementedException($"ResolveInput with inputType=[{inputType}] is not implemented.");
            }
        }

        /// <summary>
        /// Reads a shared storage token and returns the value as a string.
        /// </summary>
        /// <param name="token">Shared Storage token name of the the form Context.Name.</param>
        /// <returns>string.</returns>
        public static string ReadSharedStorageAsString(string token)
        {
            var splitToken = token.Split('.');
            if (splitToken.Length != 2)
            {
                throw new ArgumentException($"SharedStorage token=[{token}] is the wrong format [Does not contain exactly 1 period (.)], expecting Context.Name", nameof(token));
            }

            switch (splitToken[0].ToUpper())
            {
                case "DUT":
                    return Services.SharedStorageService.GetStringRowFromTable(splitToken[1], Context.DUT);
                case "LOT":
                    return Services.SharedStorageService.GetStringRowFromTable(splitToken[1], Context.LOT);
                case "IP":
                    return Services.SharedStorageService.GetStringRowFromTable(splitToken[1], Context.IP);
                default:
                    throw new ArgumentException($"SharedStorage token=[{token}] is the wrong format [Context is not DUT, LOT or IP], expecting Context.Name", nameof(token));
            }
        }
    }
}
