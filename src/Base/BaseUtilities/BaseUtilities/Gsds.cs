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
    using Prime.SharedStorageService;

    /// <summary>
    /// Defines the <see cref="Gsds" />.
    /// </summary>
    public static class Gsds
    {
        /// <summary>
        /// Checks if a token exists, returns true if it does. Throws an exception
        /// if the token is not in GSDS format.
        /// </summary>
        /// <param name="token">GSDS token name of the form G.[ULI].[SDIO].Name.</param>
        /// <returns>true if the token exists.</returns>
        /// <exception cref="ArgumentException">Thrown when the token is in the wrong format.</exception>
        public static bool TokenExists(string token)
        {
            var splitToken = SplitAndValidateTokenSize(token);
            Context context = StringCharToContext(token, splitToken[1]);
            switch (splitToken[2])
            {
                case "S":
                    return Prime.Services.SharedStorageService.KeyExistsInStringTable(splitToken[3], context);
                case "D":
                    return Prime.Services.SharedStorageService.KeyExistsInDoubleTable(splitToken[3], context);
                case "I":
                    return Prime.Services.SharedStorageService.KeyExistsInIntegerTable(splitToken[3], context);
                case "O":
                    return Prime.Services.SharedStorageService.KeyExistsInObjectTable(splitToken[3], context);
                default:
                    throw new ArgumentException($"GSDS token=[{token}] is the wrong format [Type is not string (S), double (D), integer (I), or object (O)], expecting G.[ULI].[SDIO].name", nameof(token));
            }
        }

        /// <summary>
        /// Checks if a given string is in GSDS format (G.[ULI].[SDIO].Name) and returns
        /// true if it is.
        /// </summary>
        /// <param name="token">GSDS token name.</param>
        /// <returns>true if the token is in G.[ULI].[SDIO].Name format.</returns>
        public static bool IsTokenFormat(string token)
        {
            try
            {
                var splitToken = SplitAndValidateTokenSize(token);
                Context context = StringCharToContext(token, splitToken[1]);
                switch (splitToken[2])
                {
                    case "S":
                    case "D":
                    case "I":
                    case "O":
                        return true;
                    default:
                        return false;
                }
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a given string is in GSDS format (G.[ULI].[SDIO].Name) and returns
        /// true if it is and the token exists. Returns false if the format is wrong
        /// or the token doesn't exist.
        /// </summary>
        /// <param name="token">GSDS token name of the form .</param>
        /// <returns>true if the token exists.</returns>
        public static bool IsTokenFormatAndExists(string token)
        {
            try
            {
                return TokenExists(token);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// Writes a gsds token with the given value.
        /// </summary>
        /// <param name="token">GSDS token name of the form G.[ULI].[SDI].Name.</param>
        /// <param name="value">GSDS value to write.</param>
        /// <exception cref="ArgumentException">Thrown when the token is in the wrong format.</exception>
        public static void WriteToken(string token, object value)
        {
            var splitToken = SplitAndValidateTokenSize(token);
            Context context = StringCharToContext(token, splitToken[1]);
            switch (splitToken[2])
            {
                case "S":
                    Prime.Services.SharedStorageService.InsertRowAtTable(splitToken[3], Convert.ToString(value), context);
                    return;
                case "D":
                    Prime.Services.SharedStorageService.InsertRowAtTable(splitToken[3], Convert.ToDouble(value), context);
                    return;
                case "I":
                    Prime.Services.SharedStorageService.InsertRowAtTable(splitToken[3], Convert.ToInt32(value), context);
                    return;
                /* case "O": // This just causes all sorts of issues...
                    Prime.Services.SharedStorageService.InsertRowAtTable(splitToken[3], (object)value, context);
                    return; */
                default:
                    throw new ArgumentException($"GSDS token=[{token}] is the wrong format [Type is not string (S), double (D), or integer (I)], expecting G.[ULI].[SDI].name", nameof(token));
            }
        }

        /// <summary>
        /// Reads a gsds token and returns the value as a string.
        /// </summary>
        /// <param name="token">GSDS token name of the form G.[ULI].[SDIO].Name.</param>
        /// <returns>string.</returns>
        /// <exception cref="ArgumentException">Thrown when the token is in the wrong format.</exception>
        public static object ReadToken(string token)
        {
            var splitToken = SplitAndValidateTokenSize(token);
            Context context = StringCharToContext(token, splitToken[1]);
            switch (splitToken[2])
            {
                case "S":
                    return Prime.Services.SharedStorageService.GetStringRowFromTable(splitToken[3], context);
                case "D":
                    return Prime.Services.SharedStorageService.GetDoubleRowFromTable(splitToken[3], context);
                case "I":
                    return Prime.Services.SharedStorageService.GetIntegerRowFromTable(splitToken[3], context);
                case "O":
                    return Prime.Services.SharedStorageService.GetRowFromTable(splitToken[3], typeof(object), context).ToString();
                default:
                    throw new ArgumentException($"GSDS token=[{token}] is the wrong format [Type is not string (S), double (D), integer (I), or object (O)], expecting G.[ULI].[SDIO].name", nameof(token));
            }
        }

        private static Context StringCharToContext(string token, string contextString)
        {
            Context context;
            switch (contextString)
            {
                case "U":
                    context = Context.DUT;
                    break;
                case "L":
                    context = Context.LOT;
                    break;
                case "I":
                    context = Context.IP;
                    break;
                default:
                    throw new ArgumentException($"GSDS token=[{token}] is the wrong format. Context=[{contextString}] is not Unit (U), Lot (L), or IP (I), expecting G.[UIL].[SDIO].name", nameof(token));
            }

            return context;
        }

        private static string[] SplitAndValidateTokenSize(string token)
        {
            var splitToken = token.Split(new char[] { '.' }, 4);
            if (splitToken.Length != 4 || splitToken[0] != "G")
            {
                throw new ArgumentException($"GSDS token=[{token}] is the wrong format [Does not contain at least 4 periods (.)], expecting G.[ULI].[SDIO].name", nameof(token));
            }

            return splitToken;
        }
    }
}
