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

namespace ArrayHRY
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines the <see cref="Utilities" />.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Reverse the given string, return a new string.
        /// </summary>
        /// <param name="baseString">String to reverse.</param>
        /// <returns>Reversed string.</returns>
        public static string StringReverse(string baseString)
        {
            char[] charArray = baseString.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Gets a substring uint a tuple from RangeToStartAndLengthTuple.
        /// </summary>
        /// <param name="baseString">Base string.</param>
        /// <param name="startAndLength">Tuple with start index and length fields.</param>
        /// <returns>string.</returns>
        public static string GetSubStringWithStartAndLengthTuple(string baseString, Tuple<uint, int> startAndLength)
        {
            var subString = baseString.Substring((int)startAndLength.Item1, Math.Abs(startAndLength.Item2));
            if (startAndLength.Item2 < 0)
            {
                return Utilities.StringReverse(subString);
            }
            else
            {
                return subString;
            }
        }

        /// <summary>
        /// Function to convert a string range of X-Y into a Tuple containing the starting bit and length.
        /// Length will be negative if start is greater then the end.
        /// The starting bit will always be the smaller value of the range.
        /// </summary>
        /// <param name="range">Range to convert.</param>
        /// <returns>List of indexes.</returns>
        public static Tuple<uint, int> RangeToStartAndLengthTuple(string range)
        {
            if (string.IsNullOrWhiteSpace(range))
            {
                throw new ArgumentException($"Range cannot be null or empty.");
            }

            if (range.Contains("-"))
            {
                var rangePair = range.Split(new char[] { '-' }, 2);
                if (!uint.TryParse(rangePair[0], out var rStart) || !uint.TryParse(rangePair[1], out var rEnd))
                {
                    throw new ArgumentException($"Cannot convert R0=[{rangePair[0]}] and R1=[{rangePair[1]}] into unsigned integers from range=[{range}]");
                }

                int direction = (rStart <= rEnd) ? 1 : -1;
                uint startIndex = (uint)Math.Min(rStart, rEnd);
                int length = Math.Abs((int)rStart - (int)rEnd) + 1;
                return new Tuple<uint, int>(startIndex, length * direction);
            }
            else
            {
                if (!uint.TryParse(range, out var rangeAsUint))
                {
                    throw new ArgumentException($"Cannot convert range=[{range}] into an unsigned integer.");
                }

                return new Tuple<uint, int>(rangeAsUint, 1);
            }
        }

        /// <summary>
        /// Verify that the pin name is a valid pin but not a group.
        /// </summary>
        /// <param name="pinName">Name of the pin to check.</param>
        /// <returns>true if the pin exists and is not a group, false otherwise.</returns>
        public static bool IsPinNotGroup(string pinName)
        {
            return Prime.Services.PinService.Exists(pinName) && !Prime.Services.PinService.Get(pinName).IsGroup();
        }

        /// <summary>
        /// Looks for a pin in package and IP scope and returns the one it finds first.
        /// </summary>
        /// <param name="ipScope">IP scope to look for the pin if it cannot be found in PKG scope.</param>
        /// <param name="pin">(ref) Pin name to look for, will be modified if needed.</param>
        /// <returns>True if the pin can be found.</returns>
        public static bool ResolvePinScope(string ipScope, ref string pin)
        {
            // check for pin as-is.
            if (Utilities.IsPinNotGroup(pin))
            {
                return true;
            }

            // check for pin with IP scoping matching this test.
            if (!string.IsNullOrWhiteSpace(ipScope) && Utilities.IsPinNotGroup($"{ipScope}::{pin}"))
            {
                pin = $"{ipScope}::{pin}";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to find the HDMT User Variable and return its fully-qualified name.
        /// TODO: ResolveUserVar only exists for backwards compatibility with iCHSRTest. The XML file should have the full UserVar name.
        /// Searches in this order:
        ///   1. As-is.
        ///   2. With IP/PKG + Module Scoping.
        ///   3. With IP scoping in the same module as the collection.
        ///   4. In the PKG scope in the same module as the collection.
        /// </summary>
        /// <param name="ipScope">The IP scope to check.</param>
        /// <param name="module">The Module of the current test.</param>
        /// <param name="global">(ref) HDMT UserVar name of the format "collection.uservar" with optional IP + Module scoping.</param>
        /// <returns>true if it can find the UserVar.</returns>
        public static bool ResolveUserVar(string ipScope, string module, ref string global)
        {
            // Check global as-is.
            if (Prime.Services.UserVarService.Exists(global))
            {
                return true;
            }

            // build shortcuts for the IP and Module scope.
            var ipPrefix = string.IsNullOrEmpty(ipScope) ? string.Empty : $"{ipScope}::";
            var modulePrefix = string.IsNullOrEmpty(module) ? string.Empty : $"{module}::";

            // Check global with IP + Module scoping.
            // If global already contains :: then assume it has the module, and just add IP.
            var globalWithModuleIp = global.Contains("::") ? $"{ipPrefix}{global}" : $"{ipPrefix}{modulePrefix}{global}";
            if (Prime.Services.UserVarService.Exists(globalWithModuleIp))
            {
                global = globalWithModuleIp;
                return true;
            }

            // Check for global with IP scope and Module = Collection.
            var collection = global.Split('.')[0];
            if (Prime.Services.UserVarService.Exists($"{ipPrefix}{collection}::{global}"))
            {
                global = $"{ipScope}::{collection}::{global}";
                return true;
            }

            // Check for global with PKG scope and Module = Collection.
            if (Prime.Services.UserVarService.Exists($"__main__::{collection}::{global}"))
            {
                global = $"__main__::{collection}::{global}";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the Module and IP context from the current test instance name.
        /// TODO: Could use GetCurrentIpName() name but still need to get the module name from the test name.
        /// </summary>
        /// <param name="ip">Name of the IP scope.</param>
        /// <param name="module">Name of the Module scope.</param>
        public static void GetModuleAndIp(out string ip, out string module)
        {
            var testName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
            var testNameSplit = testName.Split(new string[] { "::" }, 3, System.StringSplitOptions.None);
            if (testNameSplit.Length == 1)
            {
                // no Module or IP.
                ip = string.Empty;
                module = string.Empty;
            }
            else if (testNameSplit.Length == 2)
            {
                // Module but no IP.
                ip = string.Empty;
                module = testNameSplit[0];
            }
            else
            {
                // Module and IP
                ip = testNameSplit[0];
                module = testNameSplit[1];
            }
        }

        /// <summary>
        /// Write the HRY to ITUFF as STRGVAL data.
        /// </summary>
        /// <param name="algorithmName">Name of algorithm for this data. Will be used as part of the tname postfix.</param>
        /// <param name="hryListData">HRY data as a list of char.</param>
        /// <param name="lineLimit">Character limit for each strgval.</param>
        public static void DatalogHryStrgval(string algorithmName, List<char> hryListData, int lineLimit)
        {
            for (var logIndex = 0; logIndex * lineLimit < hryListData.Count; logIndex++)
            {
                var writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
                var postfix = string.IsNullOrWhiteSpace(algorithmName) ? $"_HRY_RAWSTR_{logIndex + 1}" : $"_{algorithmName}_HRY_RAWSTR_{logIndex + 1}";
                writer.SetTnamePostfix(postfix);
                writer.SetData(string.Join(string.Empty, hryListData.GetRange(logIndex * lineLimit, Math.Min(lineLimit, hryListData.Count - (logIndex * lineLimit)))));
                Prime.Services.DatalogService.WriteToItuff(writer);
            }
        }
    }
}
