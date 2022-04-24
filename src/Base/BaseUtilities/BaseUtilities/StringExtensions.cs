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
    using System.Text.RegularExpressions;
    using NCalc;
    using Prime.SharedStorageService;

    /// <summary>
    /// Defines the <see cref="StringExtensions" />.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Simply reverses a string. This is the fastest way I could find to do it.
        /// </summary>
        /// <param name="data">String to reverse.</param>
        /// <returns>Reversed String.</returns>
        /// <exception cref="ArgumentNullException"> if the string is null.</exception>
        public static string Reverse(this string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException($"In BaseUtilities String.Reverse(), string is null", "data");
            }

            char[] charArray = data.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Resizes the binary number. It will either pad (to the left) with 0s, or cut (from the left) extra bits.)
        /// </summary>
        /// <param name="data">Binary data.</param>
        /// <param name="size">Expected size (0 means return as is).</param>
        /// <returns>A resized binary string.</returns>
        public static string ResizeBinary(this string data, int size)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException($"In BaseUtilities String.ResizeBinary(), string is null or empty", "data");
            }

            if (size <= 0 || size == data.Length)
            {
                return data;
            }

            if (size > data.Length)
            {
                return data.PadLeft(size, '0');
            }

            return data.Substring(data.Length - size);
        }

        /// <summary>
        /// Function to convert a string range into a list of integers.
        /// Example ranges: "3,6-8" becomes { 3, 6, 7, 8 }.
        /// </summary>
        /// <param name="range">Range to convert.</param>
        /// <returns>List of indexes.</returns>
        public static List<int> RangeToList(this string range)
        {
            List<int> retval = new List<int>();
            if (string.IsNullOrEmpty(range))
            {
                return retval;
            }

            var rangeDelim = new char[] { '-' };
            foreach (var item in range.Split(','))
            {
                if (item.Contains("-"))
                {
                    var rangePair = item.Split(rangeDelim, 2);
                    var r1 = int.Parse(rangePair[0]);
                    var r2 = int.Parse(rangePair[1]);
                    var step = (r1 < r2) ? 1 : -1;
                    for (int i = r1; i != r2 + step; i += step)
                    {
                        retval.Add(i);
                    }
                }
                else
                {
                    retval.Add(int.Parse(item));
                }
            }

            return retval;
        }

        /// <summary>
        /// Converts string with or without units to double value.
        /// Able to handle units multipliers and scientific notation.
        /// </summary>
        /// <param name="value">string value.</param>
        /// <param name="useSharedStorage">Evaluates value from SharedStorage.</param>
        /// <returns>double value.</returns>
        public static double ToDouble(this string value, bool useSharedStorage = false)
        {
            value = value.Trim();
            if (double.TryParse(value, out var result))
            {
                return result;
            }

            if (useSharedStorage && Prime.Services.SharedStorageService.KeyExistsInDoubleTable(value, Context.DUT))
            {
                return Prime.Services.SharedStorageService.GetDoubleRowFromTable(value, Context.DUT);
            }

            if (useSharedStorage && Prime.Services.UserVarService.Exists(value))
            {
                return Prime.Services.UserVarService.GetDoubleValue(value);
            }

            var multipliers = new Dictionary<string, double>
            {
                { "d", 1e-1 },
                { "c", 1e-2 },
                { "m", 1e-3 },
                { "u", 1e-6 },
                { "n", 1e-9 },
                { "p", 1e-12 },
                { "h", 1e2 },
                { "k", 1e3 },
                { "M", 1e6 },
                { "G", 1e12 },
            };

            Regex rgx = new Regex(@"^\s*(-?\d+\.?\d*(e-?\d+)?)([dcmunphkMG])?([A-z]+)*\s*$");
            var m = rgx.Match(value);
            if (m.Success)
            {
                var scalar = m.Groups[1].Value;
                var multiplier = m.Groups[3].Value;
                if (!string.IsNullOrEmpty(multiplier))
                {
                    return System.Convert.ToDouble(scalar) * multipliers[multiplier];
                }

                return System.Convert.ToDouble(scalar);
            }

            throw new ArgumentException($"Unable to convert String=[{value}] into a double.");
        }

        /// <summary>
        /// Evaluates a string expression to a double value.
        /// </summary>
        /// <param name="value">Expression. Using $ as token delimiter.</param>
        /// <returns>Converted double value.</returns>
        public static double EvaluateExpression(this string value)
        {
            try
            {
                return value.ToDouble(true);
            }
            catch
            {
                var parameters = new Dictionary<string, double>();
                var regex = new Regex(@"\[(.+)\]");
                var m = regex.Match(value);
                if (m.Success)
                {
                    for (var i = 1; i < m.Groups.Count; i++)
                    {
                        parameters[$"{m.Groups[i].Value}"] = m.Groups[i].Value.ToDouble(true);
                    }
                }

                var expression = new Expression(value)
                {
                    Parameters = parameters.ToDictionary<KeyValuePair<string, double>, string, object>(token => token.Key, token => token.Value),
                };

                return (double)expression.Evaluate();
            }
        }

        /// <summary>
        /// Converts string to int32.
        /// </summary>
        /// <param name="value">string value.</param>
        /// <returns>integer result.</returns>
        public static int ToInt(this string value)
        {
            return System.Convert.ToInt32(value);
        }

        /// <summary>
        /// Gets string data into PatSymbol. Able to process binary, integer and hex representation.
        /// </summary>
        /// <param name="data">data string.</param>
        /// <param name="expectedSize">data size.</param>
        /// <returns>Array of PatternSymbol.</returns>
        public static string GetPatSymbolString(this string data, int expectedSize)
        {
            if (data == string.Empty)
            {
                throw new ArgumentException("Unable to convert empty string to pattern data.");
            }

            // Format examples: 0xFFF, 0b0101, 0d7; 0xFFF'r, 0b0101'r, 7'r
            var reverse = false;
            if (data.EndsWith("'r"))
            {
                reverse = true;
                data = data.Substring(0, data.Length - 2);
            }

            var result = data;
            if (data.StartsWith("0x"))
            {
                result = Convert.ToString(data.Substring(2).HexToBinary());
            }
            else if (data.StartsWith("0d"))
            {
                result = Convert.ToString(Convert.ToInt64(data.Substring(2), 10), 2);
            }
            else if (data.StartsWith("0b"))
            {
                result = data.Substring(2);
            }

            if (result.Length < expectedSize)
            {
                result = result.PadLeft(expectedSize, '0');
            }
            else if (result.Length > expectedSize)
            {
                throw new ArgumentException($"Data=[{data}] exceeding expected size=[{expectedSize}].");
            }

            if (reverse)
            {
                char[] arr = result.ToCharArray();
                Array.Reverse(arr);
                result = new string(arr);
            }

            return result;
        }
    }
}
