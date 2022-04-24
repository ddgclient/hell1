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

namespace Prime.TestMethods.VminSearch.Helpers
{
    using System.Collections;
    using System.Linq;

    /// <summary>
    /// BitArrayExtensions.
    /// </summary>
    internal static class BitArrayExtensions
    {
        /// <summary>
        /// Return the number of true values in the BitArray.
        /// </summary>
        /// <param name="bits">bits to operate.</param>
        /// <returns>count of true bits.</returns>
        public static int NumberOfTrue(this BitArray bits)
        {
            return bits.Cast<bool>().Count(value => value);
        }

        /// <summary>
        /// Return the number of false values in the BitArray.
        /// </summary>
        /// <param name="bits">bits to operate.</param>
        /// <returns>count of false bits.</returns>
        public static int NumberOfFalse(this BitArray bits)
        {
            return bits.Count - bits.NumberOfTrue();
        }

        /// <summary>
        /// Return the logical And of all of the entries in the array.
        /// </summary>
        /// <param name="bits">bits to operate.</param>
        /// <returns>Boolean result.</returns>
        public static bool AndAll(this BitArray bits)
        {
            return bits.Cast<bool>().All(value => value);
        }

        /// <summary>
        /// Return the logical Or of all of the entries in the array.
        /// </summary>
        /// <param name="bits">bits to operate.</param>
        /// <returns>Boolean result.</returns>
        public static bool OrAll(this BitArray bits)
        {
            return bits.Cast<bool>().Any(value => value);
        }

        /// <summary>
        /// Return a string showing the BitArray's values.
        /// </summary>
        /// <param name="bits">bits to operate.</param>
        /// <param name="trueValue">value to replace true bits.</param>
        /// <param name="falseValue">value to replace false bits.</param>
        /// <param name="separator">separator between each bit.</param>
        /// <param name="groupSize">sie for bit grouping.</param>
        /// <param name="groupSeparator">separator between each bit group.</param>
        /// <returns>BitArray converted string.</returns>
        public static string ToString(this BitArray bits, string trueValue, string falseValue, string separator = "", int groupSize = int.MaxValue, string groupSeparator = "")
        {
            var result = string.Empty;
            for (var i = 0; i < bits.Length; i++)
            {
                // Add the value and separator.
                if (bits[i])
                {
                    result += separator + trueValue;
                }
                else
                {
                    result += separator + falseValue;
                }

                // Add the group separator if appropriate.
                if ((i + 1) % groupSize == 0)
                {
                    result += groupSeparator;
                }
            }

            // Remove the initial separator.
            if (result.Length > 0)
            {
                result = result.Substring(separator.Length);
            }

            return result;
        }

        /// <summary>
        /// Converts to str.
        /// </summary>
        /// <param name="bits">bits to operate.</param>
        /// <returns>BitArray converted string.</returns>
        public static string ToStr(this BitArray bits)
        {
            return bits.ToString("1", "0");
        }

        /// <summary>
        /// Determines whether two BitArrays are equal in length and bit by bit.
        /// </summary>
        /// <param name="bits">bits to operate.</param>
        /// <param name="bitsToCompare">bits to compare.</param>
        /// <returns>BitArray converted string.</returns>
        public static bool SequenceEqual(this BitArray bits, BitArray bitsToCompare)
        {
            if (bits.Length != bitsToCompare.Length)
            {
                return false;
            }

            for (var i = 0; i < bits.Length; i++)
            {
                if (!bits[i].Equals(bitsToCompare[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
