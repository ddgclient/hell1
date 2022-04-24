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

namespace AnalogFuncCaptureCtv
{
    using System.Diagnostics.CodeAnalysis;
    using Prime.SharedStorageService;

    /// <summary>
    /// This class is intended to store all utils and common functions.
    /// </summary>
    internal static class SharedStorage
    {
        /// <summary>
        /// Function to store the value in Shared Storage.
        /// </summary>
        /// <param name="key">The key for the shared storage data.</param>
        /// <param name="value">The value to store in the shared storage.</param>
        public static void SharedStorageSetValue(string key, int value)
        {
            key = key.Substring(2); // Removes the "I." prefix.
            Prime.Services.SharedStorageService.InsertRowAtTable(key, value, Context.DUT);
        }

        /// <summary>
        /// Function to store the value in Shared Storage.
        /// </summary>
        /// <param name="key">The key for the shared storage data.</param>
        /// <param name="value">The value to store in the shared storage.</param>
        [ExcludeFromCodeCoverage]
        public static void SharedStorageSetValue(string key, double value)
        {
            key = key.Substring(2); // Removes the "D." prefix.
            Prime.Services.SharedStorageService.InsertRowAtTable(key, value, Context.DUT);
        }

        /// <summary>
        /// Function to store the value in Shared Storage.
        /// </summary>
        /// <param name="key">The key for the shared storage data.</param>
        /// <param name="value">The value to store in the shared storage.</param>
        [ExcludeFromCodeCoverage]
        public static void SharedStorageSetValue(string key, string value)
        {
            key = key.Substring(2); // Removes the "S." prefix.
            Prime.Services.SharedStorageService.InsertRowAtTable(key, value, Context.DUT);
        }

        /// <summary>
        /// Function to store the value in Shared Storage.
        /// </summary>
        /// <param name="key">The key for the shared storage data.</param>
        /// <param name="value">The value to store in the shared storage.</param>
        [ExcludeFromCodeCoverage]
        public static void SharedStorageSetValue(string key, object value)
        {
            key = key.Substring(2);
            Prime.Services.SharedStorageService.InsertRowAtTable(key, value, Context.DUT);
        }

        /// <summary>
        /// Function to return value by key from Shared Storage.
        /// </summary>
        /// <param name="key">The key for the shared storage data.</param>
        /// <returns>Returns the value found in the shared storage.</returns>
        [ExcludeFromCodeCoverage]
        public static dynamic SharedStorageGetValue(string key)
        {
            string[] keyArray = key.Split(new[] { '.' }, 2);
            string typePrefix = keyArray[0];
            string keyValue = keyArray[1];

            switch (typePrefix)
            {
                case "I":
                    {
                        return SharedStorageIntGetValue(keyValue);
                    }

                case "D":
                    {
                        return SharedStorageDoubleGetValue(keyValue);
                    }

                case "S":
                    {
                        return SharedStorageStrGetValue(keyValue);
                    }

                default:
                    {
                        Utils.PrintError($"[ERROR] '{typePrefix}' is not a valid prefix in Shared Storage key: {key}.");
                        return null; // Needed for compilation, but Error should throw the execution.
                    }
            }
        }

        /// <summary>
        /// Function to return value by key from Shared Storage.
        /// </summary>
        /// <param name="key">The key for the shared storage data.</param>
        /// <returns>Returns the value found in the shared storage.</returns>
        public static int SharedStorageIntGetValue(string key) => Prime.Services.SharedStorageService.GetIntegerRowFromTable(key, Context.DUT);

        /// <summary>
        /// Function to return value by key from Shared Storage.
        /// </summary>
        /// <param name="key">The key for the shared storage data.</param>
        /// <returns>Returns the value found in the shared storage.</returns>
        [ExcludeFromCodeCoverage]
        public static double SharedStorageDoubleGetValue(string key)
        {
            return Prime.Services.SharedStorageService.GetDoubleRowFromTable(key, Context.DUT);
        }

        /// <summary>
        /// Function to return value by key from Shared Storage.
        /// </summary>
        /// <param name="key">The key for the shared storage data.</param>
        /// <returns>Returns the value found in the shared storage.</returns>
        [ExcludeFromCodeCoverage]
        public static string SharedStorageStrGetValue(string key)
        {
            return Prime.Services.SharedStorageService.GetStringRowFromTable(key, Context.DUT);
        }

        /// <summary>
        /// Function to return value by key from Shared Storage.
        /// </summary>
        /// <param name="key">The key for the shared storage data.</param>
        /// <returns>Returns the value found in the shared storage.</returns>
        [ExcludeFromCodeCoverage]
        public static object SharedStorageObjectGetValue(string key)
        {
            return (object)Prime.Services.SharedStorageService.GetRowFromTable(key, typeof(object), Context.DUT);
        }
    }
}
