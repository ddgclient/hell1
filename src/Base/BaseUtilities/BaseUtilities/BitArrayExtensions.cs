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
    using System.Collections;
    using System.Text;

    /// <summary>
    /// Extension functions dealing with BitArray objects.
    /// </summary>
    public static class BitArrayExtensions
    {
        /// <summary>
        /// String extension method. Turns a string of 1s and 0s into a BitArray.
        /// A 1 in the string is a true in the BitArray, 0 (or any other character) is false.
        /// </summary>
        /// <param name="data">String data to convert.</param>
        /// <returns>BitArray.</returns>
        /// <exception cref="ArgumentNullException"> if input string is null.</exception>
        public static BitArray ToBitArray(this string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException($"In BaseUtilities String.ToBitArray(), string is null", "data");
            }

            var retval = new BitArray(data.Length, false);
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] == '1')
                {
                    retval[i] = true;
                }
            }

            return retval;
        }

        /// <summary>
        /// BitArray extension method. Turns a BitArray into a string of 1s and 0s.
        /// The BitArray true maps to 1, while false maps to 0.
        /// </summary>
        /// <param name="data">BitArray to convert.</param>
        /// <returns>string.</returns>
        /// <exception cref="ArgumentNullException"> if input BitArray is null.</exception>
        public static string ToBinaryString(this BitArray data)
        {
            if (data == null)
            {
                throw new ArgumentNullException($"In BaseUtilities BitArray.ToBinString(), BitArray is null", "data");
            }

            var sb = new StringBuilder(data.Length);
            for (var i = 0; i < data.Length; i++)
            {
                sb.Append(data.Get(i) ? '1' : '0');
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a slice of a BitArray.
        /// </summary>
        /// <param name="bitArray">BitArray source.</param>
        /// <param name="start">Offset of the first element.</param>
        /// <param name="count">NUmber of elements to get.</param>
        /// <returns>A subset of the Bitarray.</returns>
        /// <exception cref="ArgumentNullException"> if input BitArray is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"> if start + size is larger than the size of the bit array.</exception>
        public static BitArray Slice(this BitArray bitArray, int start, int count)
        {
            if (bitArray == null)
            {
                throw new ArgumentNullException($"In BaseUtilities BitArray.Slice(), BitArray is null", "bitArray");
            }

            if (start == 0 && count == bitArray.Count)
            {
                return bitArray;
            }

            if (start + count > bitArray.Count)
            {
                throw new ArgumentOutOfRangeException($"Start + Count is larger than BitArray Size=[{bitArray.Count}]. start=[{start}] size=[{count}].");
            }

            var retval = new BitArray(count, false);
            for (var i = 0; i < count; i++)
            {
                retval.Set(i, bitArray.Get(i + start));
            }

            return retval;
        }

        /// <summary>
        /// Appends one BitArray to another.
        /// </summary>
        /// <param name="bitArray1">Base BitArray.</param>
        /// <param name="bitArray2">BitArray to add.</param>
        /// <returns>new BitArray containing the contents of both BitArrays.</returns>
        /// <exception cref="ArgumentNullException"> if both BitArrays are null.</exception>
        public static BitArray Add(this BitArray bitArray1, BitArray bitArray2)
        {
            if (bitArray1 == null && bitArray2 == null)
            {
                throw new ArgumentNullException($"In BaseUtilities BitArray.Add(), both BitArray arguments are null.");
            }

            if (bitArray1 == null)
            {
                return bitArray2;
            }

            if (bitArray2 == null)
            {
                return bitArray1;
            }

            var retval = new BitArray(bitArray1.Count + bitArray2.Count, false);
            for (var i = 0; i < bitArray1.Count; i++)
            {
                retval.Set(i, bitArray1.Get(i));
            }

            for (var i = 0; i < bitArray2.Count; i++)
            {
                retval.Set(i + bitArray1.Count, bitArray2.Get(i));
            }

            return retval;
        }
    }
}
