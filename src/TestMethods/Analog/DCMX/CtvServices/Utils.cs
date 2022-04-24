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
    using Prime.Base.Exceptions;

    /// <summary>
    /// This class is intended to store all utils and common functions.
    /// </summary>
    internal class Utils
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
        public static string SliceString(ref string s, int sliceSize)
        {
            if (sliceSize == 0)
            {
                return string.Empty;
            }
            else
            {
                string slice = s.Substring(0, sliceSize);
                s = s.Substring(sliceSize);
                return slice;
            }
        }

        /// <summary>
        /// Function to Replace first appearance in string.
        /// </summary>
        /// <param name="inputString">The string where the replace happens.</param>
        /// <param name="search">String to be replaced.</param>
        /// <param name="replace">Replaces the searched string.</param>
        /// <returns>Returns the string with the replacement done.</returns>
        public static string ReplaceFirst(string inputString, string search, string replace)
        {
            int index = inputString.IndexOf(search);
            if (index < 0)
            {
                return inputString;
            }
            else
            {
                return inputString.Substring(0, index) + replace + inputString.Substring(index + search.Length);
            }
        }

        /// <summary>
        /// Function to calculate OnesComplement.
        /// </summary>
        /// <param name="s">The string to be reversed from OnesComplement.</param>
        /// <returns>Returns the int value of the 1's complement input binary string.</returns>
        public static int OnesComplementToDecimal(string s)
        {
            int value = 0;

            if (s[0] == '0')
            {
                value = -Convert.ToInt32(s, 2);
            }
            else if (s[0] == '1')
            {
                string number = string.Empty;
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '0')
                    {
                        number = number + '1';
                    }
                    else if (s[i] == '1')
                    {
                        number = number + '0';
                    }
                }

                value = Convert.ToInt32(number, 2);
            }

            return value;
        }

        /// <summary>
        /// Function to calculate TwosComplement.
        /// </summary>
        /// <param name="s">The string to be converted to TwosComplement.</param>
        /// <returns>Returns the 2's complement of the input binary string.</returns>
        public static int TwosComplementToDecimal(string s)
        {
            int value = 0;

            if (s[0] == '0')
            {
                value = Convert.ToInt32(s, 2);
            }
            else if (s[0] == '1')
            {
                string number = string.Empty;
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '0')
                    {
                        number = number + '1';
                    }
                    else if (s[i] == '1')
                    {
                        number = number + '0';
                    }
                }

                value = -(Convert.ToInt32(number, 2) + 1);
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
        public static int GrayCodeToDecimal(string inputString)
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
