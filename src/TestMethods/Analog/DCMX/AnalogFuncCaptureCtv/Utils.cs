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
    using System;
    using System.Collections.Generic;
    using Prime.Base.Exceptions;

    /// <summary>
    /// This class is intended to store all utils and common functions.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Function to print debug messages in console.
        /// </summary>
        /// <param name="message">The message to be printed.</param>
        public static void PrintDebug(string message)
        {
            // message = string.Format("[DEBUG]{0}", message);
            Prime.Services.ConsoleService.PrintDebug(message);
        }

        /// <summary>
        /// Function to print error messages in console.
        /// </summary>
        /// <param name="message">The string to be reversed.</param>
        public static void PrintDebugError(string message)
        {
            message = $"\n################################ ERROR ################################\n" +
                          message +
                       "\n#######################################################################\n";
            Prime.Services.ConsoleService.PrintDebug(message);
        }

        /// <summary>
        /// Function to Slice a string in two parts based on the sliceSize.
        /// </summary>
        /// <param name="s">The string to be reversed.</param>
        /// <param name="sliceSize">The size of the slice to be cut.</param>
        /// <returns>Returns a list with the data split.</returns>
        public static List<string> SliceString(string s, int sliceSize)
        {
            if (sliceSize == 0)
            {
                List<string> list = new List<string> { string.Empty, s };
                return list;
            }
            else
            {
                List<string> list = new List<string> { s.Substring(0, sliceSize), s.Substring(sliceSize) };
                return list;
            }
        }

        /// <summary>
        /// Function to calculate OnesComplement.
        /// </summary>
        /// <param name="s">The string to be converted to OnesComplement.</param>
        /// <returns>Returns the 1's complement of the input binary string.</returns>
        public static int OnesComplement(string s)
        {
            int value = ~int.Parse(Convert.ToInt32(s, 2).ToString());
            return value;
        }

        /// <summary>
        /// Function to calculate TwosComplement.
        /// </summary>
        /// <param name="s">The string to be converted to TwosComplement.</param>
        /// <returns>Returns the 2's complement of the input binary string.</returns>
        public static int TwosComplement(string s)
        {
            int numberBits = s.Length;
            int value = int.Parse(Convert.ToInt32(s, 2).ToString());

            if ((value & (1 << (numberBits - 1))) != 0)
            {
                value = value - (1 << numberBits);
            }

            return value;
        }

        /// <summary>
        /// Function to reverse a string.
        /// </summary>
        /// <param name="s">The string to be reversed.</param>
        /// <returns>Returns the reversed string.</returns>
        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Function to print error messages in console.
        /// </summary>
        /// <param name="message">The string to be printed.</param>
        public static void PrintError(string message)
        {
            // ItuffFormat.CompressToItuff(message);
            message = $"\n################################ ERROR ################################\n" +
                          message +
                       "\n#######################################################################\n";

            throw new TestMethodException(message);
        }

        /// <summary>
        /// Function that turns gray code into decimal.
        /// </summary>
        /// <param name="inputString">The string to turned into decimal.</param>
        /// /// <returns>Returns the decimal value.</returns>
        public static int GrayToDec(string inputString)
        {
            int num = Convert.ToInt32(inputString, 2);
            int dec = 0;

            // Taking xor until n becomes zero
            for (; num != 0; num = num >> 1)
            {
                dec ^= num;
            }

            return dec;
        }
    }
}
