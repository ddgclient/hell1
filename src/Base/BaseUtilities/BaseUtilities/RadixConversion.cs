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
    /// Defines the <see cref="RadixConversion" />.
    /// </summary>
    public static class RadixConversion
    {
        /// <summary>
        /// String extension method. Convert the given binary string to a Hex string (no size restrictions).
        /// </summary>
        /// <param name="data">Binary string.</param>
        /// <param name="lsbFirst">Bool value, set to true if the binary string is LSB first. (Default is true, which means MSB first).</param>
        /// <returns>Hex string.</returns>
        /// <exception cref="ArgumentNullException"> if the input data is null or empty.</exception>
        public static string BinaryToHex(this string data, bool lsbFirst = false)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("Unable to convert empty string to Hex data.", "data");
            }

            if (lsbFirst)
            {
                data = data.Reverse();
            }

            string hex = string.Empty;
            if (data.Length <= 64)
            {
                // If its <=64 bits, just convert it in one shot, no need to be fancy...
                hex = Convert.ToUInt64(data, 2).ToString("X").ResizeBinary((data.Length + 3) / 4);
            }
            else
            {
                // its bigger than 64 bits so it needs to be split up before converting it in pieces.
                var partLen = 64;
                var paddedWidth = ((data.Length + partLen - 1) / partLen) * partLen; // pad it to the next multiple of 64
                var mod64Bin = data.ResizeBinary(paddedWidth);
                var hexSize = 16 - ((paddedWidth - data.Length) / 4);
                List<string> tmpLst = new List<string>();
                for (var i = 0; i < mod64Bin.Length; i += partLen)
                {
                    var subStr = mod64Bin.Substring(i, partLen);
                    tmpLst.Add(Convert.ToUInt64(subStr, 2).ToString("X").ResizeBinary(hexSize));
                    hexSize = 16; // the first one might have been less than 16 but all the others are full 64 bits (16 hex).
                }

                hex = string.Join(string.Empty, tmpLst);
            }

            return hex;
        }

        /// <summary>
        /// String extension method. Convert the given binary string to decimal (32 bit integer).
        /// </summary>
        /// <param name="data">Binary string.</param>
        /// <param name="lsbFirst">Bool value, set to true if the binary string is LSB first. (Default is true, which means MSB first).</param>
        /// <param name="twosComp">Bool value, if true the binary is treated as a 2s compliment encoding.</param>
        /// <returns>int32 value.</returns>
        public static int BinaryToInteger(this string data, bool lsbFirst = false, bool twosComp = false)
        {
            if (lsbFirst)
            {
                data = data.Reverse();
            }

            if (twosComp)
            {
                return data.TwosComplementToInteger();
            }

            int dec = Convert.ToInt32(data, 2);
            return dec;
        }

        /// <summary>
        /// String extension method. Convert the given binary string (in 2s Complement encoding) to decimal (32 bit integer).
        /// </summary>
        /// <param name="data">Binary string.</param>
        /// <returns>int32 value.</returns>
        public static int TwosComplementToInteger(this string data)
        {
            var signChange = false;
            if (data[0] == '1')
            {
                char[] charArray = new char[data.Length];
                for (var i = 0; i < data.Length; i++)
                {
                    charArray[i] = data[i] == '1' ? '0' : '1';
                }

                signChange = true;
                data = new string(charArray);
            }

            int dec = Convert.ToInt32(data, 2);
            if (signChange)
            {
                dec = (dec + 1) * -1;
            }

            return dec;
        }

        /// <summary>
        /// String extension method. Converts the positive integer value (stored in a string) into a binary value (stored in a string).
        /// </summary>
        /// <param name="data">Integer value in a string.</param>
        /// <param name="size">Optional size to pad the result to.</param>
        /// <returns>binary value in a string.</returns>
        public static string IntegerToBinary(this string data, int size = 0)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException($"In BaseUtilities String.IntegerToBinary(), string is null or empty.", "data");
            }

            try
            {
                var result = Convert.ToString(Convert.ToInt32(data, 10), 2).ResizeBinary(size);
                return result;
            }
            catch (FormatException)
            {
                Prime.Services.ConsoleService.PrintError($"In BaseUtilities String.IntegerToBinary(), String=[{data}] does not contain a valid integer.");
                throw;
            }
        }

        /// <summary>
        /// Int extension method. Converts the positive integer value into a binary value (stored in a string).
        /// </summary>
        /// <param name="data">Integer value.</param>
        /// <param name="size">Optional size to pad the result to.</param>
        /// <returns>binary value in a string.</returns>
        public static string IntegerToBinary(this int data, int size = 0)
        {
            var result = Convert.ToString(data, 2).ResizeBinary(size);
            return result;
        }

        /// <summary>
        /// Char extension method. Converts the Hexidecimal character into a binary value.
        /// </summary>
        /// <param name="data">Hex value in a string.</param>
        /// <param name="size">Optional size to pad the result to.</param>
        /// <returns>binary value in a string.</returns>
        public static string HexToBinary(this char data, int size = 4)
        {
            if (char.IsControl(data))
            {
                var error = $"In BaseUtilities Char.HexToBinary(), Character=[\\U{(int)data:X4}] is not a valid Hex digit.";
                Prime.Services.ConsoleService.PrintError(error);
                throw new ArgumentException(error);
            }

            try
            {
                var dataAsDecimal = Convert.ToInt32(data.ToString(), 16);
                var dataAsBinary = Convert.ToString(dataAsDecimal, 2).ResizeBinary(size);
                return dataAsBinary;
            }
            catch (ArgumentException)
            {
                Prime.Services.ConsoleService.PrintError($"In BaseUtilities Char.HexToBinary(), Character=[{data}] is not valid.");
                throw;
            }
            catch (FormatException)
            {
                Prime.Services.ConsoleService.PrintError($"In BaseUtilities Char.HexToBinary(), Character=[{data}] is not a valid Hex digit.");
                throw;
            }
        }

        /// <summary>
        /// String extension method. Converts the Hexidecimal value into a binary value.
        /// </summary>
        /// <param name="data">Hex value in a string.</param>
        /// <param name="size">Optional size to pad the result to.</param>
        /// <returns>binary value in a string.</returns>
        public static string HexToBinary(this string data, int size = 0)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException($"In BaseUtilities String.HexToBinary(), string is null or empty.", "data");
            }

            var rslt = string.Join(string.Empty, data.Select(c => c.HexToBinary())).ResizeBinary(size);
            return rslt;
        }
    }
}
