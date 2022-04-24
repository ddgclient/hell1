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

namespace CtvServices
{
    using System;
    using System.Text.RegularExpressions;
    using Prime;
    using Prime.Base.Exceptions;
    using Prime.SharedStorageService;

    /// <summary>
    /// This class is intended to store all utils and common functions.
    /// </summary>
    internal static class DataServices
    {
        /// <summary>
        /// Check if the value is a SharedStorage.
        /// </summary>
        public static readonly Regex SharedStorageCheck = new Regex(@"\b((DUT|LOT|IP)\.)?([\w\.]+)\b", RegexOptions.Compiled);

        /// <summary>
        /// Check if the value is a dff.
        /// </summary>
        public static readonly Regex DffCheck = new Regex(@"\b(\w+)\.(\w+)\.(\w+(\.\w+){0,1})\b", RegexOptions.Compiled);

        /// <summary>
        /// Check if the value is a user token.
        /// </summary>
        public static readonly Regex UsrVarCheck = new Regex(@"\b(\w+::){0,1}\w+\.\w+\b", RegexOptions.Compiled);

        /// <summary>
        /// Check if the value is a double.
        /// </summary>
        public static readonly Regex DoubleCheck = new Regex(@"^\d+(\.\d+){0,1}$", RegexOptions.Compiled);

        /// <summary>
        /// Check if the value is a double.
        /// </summary>
        public static readonly Regex IntCheck = new Regex(@"^\d+$", RegexOptions.Compiled);

        /// <summary>
        /// Check if the value is a binary string.
        /// </summary>
        public static readonly Regex BinaryCheck = new Regex(@"^[01]+$", RegexOptions.Compiled);

        /// <summary>
        /// Check if the value is a valid DataType token.
        /// </summary>
        public static readonly Regex DataTypeRegex = new Regex(@"([IDS]{1}|DFF)\:([\w.:]+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Gets the string value from a SharedStorage, DFF, or UserVar.
        /// </summary>
        /// <param name="token">token of holding the string token to read.</param>
        /// <returns>String from token read.</returns>
        public static string GetStringToken(string token)
        {
            if (SharedStorageCheck.IsMatch(token))
            {
                string key = SharedStorageCheck.Match(token).Groups[3].ToString();
                Context context = (SharedStorageCheck.Match(token).Groups[2].ToString() == "IP") ? Context.IP : ((SharedStorageCheck.Match(token).Groups[2].ToString() == "LOT") ? Context.LOT : Context.DUT);

                // Check if SharedStorage token exists in the corresponding table.
                if (Services.SharedStorageService.KeyExistsInStringTable(key, context))
                {
                    return Services.SharedStorageService.GetStringRowFromTable(key, context);
                }
                else
                {
                    throw new FatalException($"[ERROR] SharedStorage token [{key}], does not exists in [String] table, context [{context}].");
                }
            }
            else if (DffCheck.IsMatch(token))
            {
                string targetDieId = DffCheck.Match(token).Groups[1].ToString();
                string operationType = DffCheck.Match(token).Groups[2].ToString();
                string tokenName = DffCheck.Match(token).Groups[3].ToString();
                return Services.DffService.GetDff(tokenName, operationType, targetDieId);
            }
            else if (UsrVarCheck.IsMatch(token) && !DoubleCheck.IsMatch(token))
            {
                return Services.UserVarService.GetStringValue(token);
            }
            else
            {
                throw new FatalException($"[ERROR] Cannot find a SharedStorage, DFF, or UserVar named [{token}].");
            }
        }

        /// <summary>
        /// Gets the double value from a SharedStorage, DFF, or UserVar.
        /// </summary>
        /// <param name="token">token of holding the string token to read.</param>
        /// <returns>String from token read.</returns>
        public static double GetDoubleToken(string token)
        {
            if (SharedStorageCheck.IsMatch(token))
            {
                string key = SharedStorageCheck.Match(token).Groups[3].ToString();
                Context context = (SharedStorageCheck.Match(token).Groups[2].ToString() == "IP") ? Context.IP : ((SharedStorageCheck.Match(token).Groups[2].ToString() == "LOT") ? Context.LOT : Context.DUT);

                // Check if SharedStorage token exists in the corresponding table.
                if (Services.SharedStorageService.KeyExistsInDoubleTable(key, context))
                {
                    return Services.SharedStorageService.GetDoubleRowFromTable(key, context);
                }
                else
                {
                    throw new FatalException($"[ERROR] SharedStorage token [{key}], does not exists in [Double] table, context [{context}].");
                }
            }
            else if (UsrVarCheck.IsMatch(token) && !DoubleCheck.IsMatch(token))
            {
                return Services.UserVarService.GetDoubleValue(token);
            }
            else
            {
                throw new FatalException($"[ERROR] Cannot find a SharedStorage or UserVar named [{token}].");
            }
        }

        /// <summary>
        /// Gets the int value from a SharedStorage, DFF, or UserVar.
        /// </summary>
        /// <param name="token">token of holding the string token to read.</param>
        /// <returns>String from token read.</returns>
        public static int GetIntToken(string token)
        {
            if (CheckSharedStorageToken(token))
            {
                string key = SharedStorageCheck.Match(token).Groups[3].ToString();
                Context context = (SharedStorageCheck.Match(token).Groups[2].ToString() == "IP") ? Context.IP : ((SharedStorageCheck.Match(token).Groups[2].ToString() == "LOT") ? Context.LOT : Context.DUT);

                // Check if SharedStorage token exists in the corresponding table.
                if (Services.SharedStorageService.KeyExistsInIntegerTable(key, context))
                {
                    return Services.SharedStorageService.GetIntegerRowFromTable(key, context);
                }
                else
                {
                    throw new FatalException($"[ERROR] SharedStorage token [{key}], does not exists in [Integer] table, context [{context}].");
                }
            }
            else if (CheckUserVarToken(token))
            {
                return Services.UserVarService.GetIntValue(token);
            }
            else
            {
                throw new FatalException($"[ERROR] Cannot find a SharedStorage or UserVar named [{token}].");
            }
        }

        /// <summary>
        /// Checks the DFF token string format.
        /// </summary>
        /// <param name="token">token of holding the string token to verified.</param>
        /// <returns>Returns a true if the format matches a DFF.</returns>
        public static bool CheckDFFToken(string token)
        {
            if (DffCheck.IsMatch(token))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks the SharedStorage token string format.
        /// </summary>
        /// <param name="token">token of holding the string token to verified.</param>
        /// <returns>Returns a true if the format matches a SharedStorage.</returns>
        public static bool CheckSharedStorageToken(string token)
        {
            if (SharedStorageCheck.IsMatch(token))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks the UserVar token string format.
        /// </summary>
        /// <param name="token">token of holding the string token to verified.</param>
        /// <returns>Returns a true if the format matches a UserVar.</returns>
        public static bool CheckUserVarToken(string token)
        {
            if (UsrVarCheck.IsMatch(token) && !DoubleCheck.IsMatch(token) && Services.UserVarService.Exists(token))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the string value from a SharedStorage.
        /// </summary>
        /// <param name="token">token of holding the string token to written.</param>
        /// <param name="data">String data to be set in token.</param>
        public static void SetSharedStorage(string token, string data)
        {
            if (SharedStorageCheck.IsMatch(token))
            {
                string key = SharedStorageCheck.Match(token).Groups[3].ToString();
                Context context = (SharedStorageCheck.Match(token).Groups[2].ToString() == "IP") ? Context.IP : ((SharedStorageCheck.Match(token).Groups[2].ToString() == "LOT") ? Context.LOT : Context.DUT);
                Services.SharedStorageService.InsertRowAtTable(key, data, context);
            }
            else
            {
                throw new FatalException($"[ERROR] Expecting a SharedStorage token name, but got [{token}].");
            }
        }

        /// <summary>
        /// Sets the integer value from a SharedStorage.
        /// </summary>
        /// <param name="token">token of holding the string token to written.</param>
        /// <param name="data">Double data to be set in token.</param>
        public static void SetSharedStorage(string token, int data)
        {
            if (SharedStorageCheck.IsMatch(token))
            {
                string key = SharedStorageCheck.Match(token).Groups[3].ToString();
                Context context = (SharedStorageCheck.Match(token).Groups[2].ToString() == "IP") ? Context.IP : ((SharedStorageCheck.Match(token).Groups[2].ToString() == "LOT") ? Context.LOT : Context.DUT);
                Services.SharedStorageService.InsertRowAtTable(key, data, context);
            }
            else
            {
                throw new FatalException($"[ERROR] Expecting a SharedStorage token name, but got [{token}].");
            }
        }

        /// <summary>
        /// Sets the double value from a SharedStorage.
        /// </summary>
        /// <param name="token">token of holding the string token to written.</param>
        /// <param name="data">Double data to be set in token.</param>
        public static void SetSharedStorage(string token, double data)
        {
            if (SharedStorageCheck.IsMatch(token))
            {
                string key = SharedStorageCheck.Match(token).Groups[3].ToString();
                Context context = (SharedStorageCheck.Match(token).Groups[2].ToString() == "IP") ? Context.IP : ((SharedStorageCheck.Match(token).Groups[2].ToString() == "LOT") ? Context.LOT : Context.DUT);
                Services.SharedStorageService.InsertRowAtTable(key, data, context);
            }
            else
            {
                throw new FatalException($"[ERROR] Expecting a SharedStorage token name, but got [{token}].");
            }
        }

        /// <summary>
        /// Sets the string value from a UserVar.
        /// </summary>
        /// <param name="token">token of holding the string token to written.</param>
        /// <param name="data">String data to be set in token.</param>
        public static void SetUserVar(string token, string data)
        {
            if (UsrVarCheck.IsMatch(token) && !DoubleCheck.IsMatch(token))
            {
                try
                {
                    Services.UserVarService.SetValue(token, data);
                }
                catch (Exception e)
                {
                    throw new FatalException($"[ERROR] Failed writing a [string] to a UserVar in token [{token}]. Other Errors\n{e.Message}");
                }
            }
            else
            {
                throw new FatalException($"[ERROR] Expecting a UserVar token in the format of <Collection>::<Name>, but got [{token}].");
            }
        }

        /// <summary>
        /// Sets the double value from a UserVar.
        /// </summary>
        /// <param name="token">token of holding the string token to written.</param>
        /// <param name="data">Double data to be set in token.</param>
        public static void SetUserVar(string token, double data)
        {
            if (UsrVarCheck.IsMatch(token) && !DoubleCheck.IsMatch(token))
            {
                try
                {
                    Services.UserVarService.SetValue(token, data);
                }
                catch (Exception e)
                {
                    throw new FatalException($"[ERROR] Failed writing a [double] to a UserVar in token [{token}]. Other Errors\n{e.Message}");
                }
            }
            else
            {
                throw new FatalException($"[ERROR] Expecting a UserVar token in the format of <Collection>::<Name>, but got [{token}].");
            }
        }

        /// <summary>
        /// Sets the integer value from a UserVar.
        /// </summary>
        /// <param name="token">token of holding the string token to written.</param>
        /// <param name="data">Integer data to be set in token.</param>
        public static void SetUserVar(string token, int data)
        {
            if (UsrVarCheck.IsMatch(token) && !DoubleCheck.IsMatch(token))
            {
                try
                {
                    Services.UserVarService.SetValue(token, data);
                }
                catch (Exception e)
                {
                    throw new FatalException($"[ERROR] Failed writing a [integer] to a UserVar in token [{token}]. Other Errors\n{e.Message}");
                }
            }
            else
            {
                throw new FatalException($"[ERROR] Expecting a UserVar token in the format of <Collection>::<Name>, but got [{token}].");
            }
        }

        /// <summary>
        /// Sets the string value from a DFF.
        /// </summary>
        /// <param name="token">token of holding the string token to written.</param>
        /// <param name="data">String data to be set in token.</param>
        public static void SetDFF(string token, string data)
        {
            if (DffCheck.IsMatch(token))
            {
                string targetDieId = DffCheck.Match(token).Groups[1].ToString();
                string dffName = DffCheck.Match(token).Groups[3].ToString();
                Services.DffService.SetDff(dffName, data, targetDieId);
            }
            else
            {
                throw new FatalException($"[ERROR] Expecting a DFF token in the format <targetDieId>.<operationType>.<tokenName> in the format <targetDieId>.<operationType>.<tokenName>, but received [{token}].");
            }
        }

        /// <summary>
        /// Sets the double value from a DFF.
        /// </summary>
        /// <param name="token">token of holding the string token to written.</param>
        /// <param name="data">Double data to be set in token.</param>
        public static void SetDFF(string token, double data)
        {
            if (DffCheck.IsMatch(token))
            {
                string targetDieId = DffCheck.Match(token).Groups[1].ToString();
                string tokenName = DffCheck.Match(token).Groups[3].ToString();
                Services.DffService.SetDff(tokenName, data.ToString(), targetDieId);
            }
            else
            {
                throw new FatalException($"[ERROR] Expecting a DFF token in the format <targetDieId>.<operationType>.<tokenName>, but received [{token}].");
            }
        }

        /// <summary>
        /// Sets the integer value from a DFF.
        /// </summary>
        /// <param name="token">token of holding the string token to written.</param>
        /// <param name="data">Integer data to be set in token.</param>
        public static void SetDFF(string token, int data)
        {
            if (DffCheck.IsMatch(token))
            {
                string targetDieId = DffCheck.Match(token).Groups[1].ToString();
                string tokenName = DffCheck.Match(token).Groups[3].ToString();
                Services.DffService.SetDff(tokenName, data.ToString(), targetDieId);
            }
            else
            {
                throw new FatalException($"[ERROR] Expecting a DFF token in the format <targetDieId>.<operationType>.<tokenName>, but received [{token}].");
            }
        }

        /// <summary>
        /// Function to return value by key from any storage service.
        /// </summary>
        /// <param name="tokenWithDataType">The key for the stored data.</param>
        /// <returns>Returns the value found in a storage service.</returns>
        public static dynamic GetData(string tokenWithDataType)
        {
            string typePrefix = DataTypeRegex.Match(tokenWithDataType).Groups[1].ToString();
            string token = DataTypeRegex.Match(tokenWithDataType).Groups[2].ToString();

            switch (typePrefix)
            {
                case "i":
                case "I":
                    {
                        return GetIntToken(token);
                    }

                case "d":
                case "D":
                    {
                        return GetDoubleToken(token);
                    }

                case "s":
                case "S":
                case "DFF":
                    {
                        return GetStringToken(token);
                    }

                default:
                    {
                        throw new FatalException($"[ERROR] [GetData] '{typePrefix}' is not a valid prefix in key: {tokenWithDataType}.");
                    }
            }
        }

        /// <summary>
        /// Function to store data to the Shared Storage, UsrVars or DFF.
        /// </summary>
        /// <param name="tokenWithDataType">The key for the stored data.</param>
        /// <param name="data">The data to be programmed.</param>
        public static void SetData(string tokenWithDataType, double data)
        {
            var matchDataTypeRegex = DataTypeRegex.Match(tokenWithDataType);
            string typePrefix = matchDataTypeRegex.Groups[1].ToString();
            string token = matchDataTypeRegex.Groups[2].ToString();

            switch (typePrefix)
            {
                case "i":
                case "I":
                    {
                        // Save as Shared Storage
                        if (CheckSharedStorageToken(token))
                        {
                            SetSharedStorage(token, (int)data);
                        }

                        // Save as User Var
                        else if (CheckUserVarToken(token))
                        {
                            SetUserVar(token, data);
                        }

                        break;
                    }

                case "d":
                case "D":
                    {
                        // Save as Shared Storage
                        if (CheckSharedStorageToken(token))
                        {
                            SetSharedStorage(token, data);
                        }

                        // Save as User Var
                        else if (CheckUserVarToken(token))
                        {
                            SetUserVar(token, data);
                        }

                        // Invalid token case
                        else
                        {
                            Utils.PrintError($"[ERROR] [SetData] Token [{token}] is not a valid SharedStorage or UserVar token.");
                        }

                        break;
                    }

                default:
                    {
                        Utils.PrintError($"[ERROR] [SetData] [{typePrefix}] is not a valid data type prefix for StorageToken [{tokenWithDataType}].");
                        break;
                    }
            }
        }

        /// <summary>
        /// Function to store data to the Shared Storage, UsrVars or DFF.
        /// </summary>
        /// <param name="tokenWithDataType">The key for the stored data.</param>
        /// <param name="data">The data to be programmed.</param>
        public static void SetData(string tokenWithDataType, string data)
        {
            var matchDataTypeRegex = DataTypeRegex.Match(tokenWithDataType);
            string typePrefix = matchDataTypeRegex.Groups[1].ToString();
            string token = matchDataTypeRegex.Groups[2].ToString();

            switch (typePrefix)
            {
                case "s":
                case "S":
                    {
                        // Save as Shared Storage
                        if (CheckSharedStorageToken(token))
                        {
                            SetSharedStorage(token, data);
                        }

                        // Save as User Var
                        else if (CheckUserVarToken(token))
                        {
                            SetUserVar(token, data);
                        }

                        // Invalid token case
                        else
                        {
                            Utils.PrintError($"[ERROR] [SetData] Token [{token}] is not a valid SharedStorage or UserVar token.");
                        }

                        break;
                    }

                case "DFF":
                    {
                        // Save as DFF
                        if (CheckDFFToken(token))
                        {
                            SetDFF(token, data);
                        }

                        // Invalid token case
                        else
                        {
                            Utils.PrintError($"[ERROR] [SetData] Token [{token}] is not a valid DFF. Expected format is <targetDieId>.<operationType>.<tokenName>.");
                        }

                        break;
                    }

                default:
                    {
                        Utils.PrintError($"[ERROR] [SetData] [{typePrefix}] is not a valid data type prefix for StorageToken [{tokenWithDataType}].");
                        break;
                    }
            }
        }
    }
}
