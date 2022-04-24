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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines the <see cref="UserVar" />.
    /// </summary>
    public static class UserVar
    {
        /// <summary>
        /// Defines the valid Hdmt UserVariable types.
        /// </summary>
        public enum ValidTypes
        {
            /// <summary>Boolean Type.</summary>
            BOOLEAN,

            /// <summary>Boolean Type.</summary>
            DOUBLE,

            /// <summary>Boolean Type.</summary>
            INTEGER,

            /// <summary>Boolean Type.</summary>
            STRING,

            /// <summary>Boolean Type.</summary>
            ARRAYBOOLEAN,

            /// <summary>Boolean Type.</summary>
            ARRAYDOUBLE,

            /// <summary>Boolean Type.</summary>
            ARRAYINTEGER,

            /// <summary>Boolean Type.</summary>
            ARRAYSTRING,
        }

        /// <summary>
        /// Returns true if the given UserVar exists, false otherwise.
        /// </summary>
        /// <param name="uservar">Hdmt UserVariable name of form collection.variable .</param>
        /// <returns>bool.</returns>
        public static bool Exists(string uservar)
        {
            // if it contains too many dots (.) UserVarService.Exists() will throw an exception...
            try
            {
                return Prime.Services.UserVarService.Exists(uservar);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Writes a uservar of the given type.
        /// </summary>
        /// <param name="uservar">Hdmt UserVariable of the form collection.variable .</param>
        /// <param name="type">Type of the Hdmt UserVariable.</param>
        /// <param name="value">String value to write. It will be converted to the correct type, ARRAY types should be a comma-separated string.</param>
        public static void Write(string uservar, ValidTypes type, object value)
        {
            switch (type)
            {
                case ValidTypes.BOOLEAN:
                    Prime.Services.UserVarService.SetValue(uservar, Convert.ToBoolean(value));
                    break;
                case ValidTypes.DOUBLE:
                    Prime.Services.UserVarService.SetValue(uservar, Convert.ToDouble(value));
                    break;
                case ValidTypes.INTEGER:
                    Prime.Services.UserVarService.SetValue(uservar, Convert.ToInt32(value));
                    break;
                case ValidTypes.STRING:
                    Prime.Services.UserVarService.SetValue(uservar, Convert.ToString(value));
                    break;
                case ValidTypes.ARRAYBOOLEAN:
                    List<bool> valueAsBoolList = Convert.ToString(value).Split(',').Select(bool.Parse).ToList();
                    Prime.Services.UserVarService.SetValue(uservar, valueAsBoolList);
                    break;
                case ValidTypes.ARRAYDOUBLE:
                    List<double> valueAsDoubleList = Convert.ToString(value).Split(',').Select(double.Parse).ToList();
                    Prime.Services.UserVarService.SetValue(uservar, valueAsDoubleList);
                    break;
                case ValidTypes.ARRAYINTEGER:
                    List<int> valueAsIntList = Convert.ToString(value).Split(',').Select(int.Parse).ToList();
                    Prime.Services.UserVarService.SetValue(uservar, valueAsIntList);
                    break;
                default: /* ValidTypes.ARRAYSTRING: */
                    List<string> valueAsStringList = Convert.ToString(value).Split(',').ToList();
                    Prime.Services.UserVarService.SetValue(uservar, valueAsStringList);
                    break;
            }
        }

        /// <summary>
        /// Reads the value of the given Hdmt UserVariable.
        /// </summary>
        /// <param name="uservar">Hdmt UserVariable of the form collection.variable .</param>
        /// <param name="type">Type of the Hdmt UserVariable.</param>
        /// <returns>Value as a string. ARRAY types will be a comma-separated string.</returns>
        public static object Read(string uservar, ValidTypes type)
        {
            switch (type)
            {
                case ValidTypes.BOOLEAN:
                    return Prime.Services.UserVarService.GetBoolValue(uservar);
                case ValidTypes.DOUBLE:
                    return Prime.Services.UserVarService.GetDoubleValue(uservar);
                case ValidTypes.INTEGER:
                    return Prime.Services.UserVarService.GetIntValue(uservar);
                case ValidTypes.STRING:
                    return Prime.Services.UserVarService.GetStringValue(uservar);
                case ValidTypes.ARRAYBOOLEAN:
                    return string.Join(",", Prime.Services.UserVarService.GetArrayBoolValue(uservar));
                case ValidTypes.ARRAYDOUBLE:
                    return string.Join(",", Prime.Services.UserVarService.GetArrayDoubleValue(uservar));
                case ValidTypes.ARRAYINTEGER:
                    return string.Join(",", Prime.Services.UserVarService.GetArrayIntValue(uservar));
                default: /* ValidTypes.ARRAYSTRING: */
                    return string.Join(",", Prime.Services.UserVarService.GetArrayStringValue(uservar));
            }
        }

        /// <summary>
        /// Reads the value of the given Hdmt UserVariable without knowning the type.
        /// The parameter "type" will be updated with the actual type that was read.
        /// </summary>
        /// <param name="uservar">Hdmt UserVariable of the form collection.variable .</param>
        /// <param name="type">(output) Type of the Hdmt UserVariable.</param>
        /// <returns>Value as a string. ARRAY types will be a comma-separated string.</returns>
        public static object ReadAndGetType(string uservar, out ValidTypes type)
        {
            // API used for the read has to match the actual type, but there's no way to know the actual type...
            // String seems to work on Array types, so make sure to do that one last.
            var allTypes = new List<ValidTypes>
            {
                ValidTypes.BOOLEAN,
                ValidTypes.INTEGER,
                ValidTypes.DOUBLE,
                ValidTypes.ARRAYBOOLEAN,
                ValidTypes.ARRAYINTEGER,
                ValidTypes.ARRAYDOUBLE,
                ValidTypes.ARRAYSTRING,
                ValidTypes.STRING,
            };

            string lastFailure = string.Empty;
            foreach (var userVarType in allTypes)
            {
                try
                {
                    var retval = DDG.UserVar.Read(uservar, userVarType);
                    type = userVarType;
                    return retval;
                }
                catch (Exception e)
                {
                    lastFailure = $"Failed to read UserVar=[{uservar}]: {e.Message}";
                }
            }

            Prime.Services.ConsoleService.PrintError(lastFailure);
            throw new Prime.Base.Exceptions.TestMethodException(lastFailure);
        }
    }
}
