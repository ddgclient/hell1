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
    /// Contains static helper functions for decoding capture memory.
    /// </summary>
    public static class MemDecode
    {
        /// <summary>
        /// String extension method. Extract the given bits from the given binary string.
        /// </summary>
        /// <param name="data">Binary Data string. Bit 0 is the first (leftmost) bit in the string.</param>
        /// <param name="bits">List specifying which bits to extract.</param>
        /// <param name="offset">Optional offset to add to each element in the bits list.</param>
        /// <returns>Substring based on the requested bits.</returns>
        public static string ExtractBits(this string data, List<int> bits, int offset = 0)
        {
            char[] charArray = new char[bits.Count];
            int index = 0;
            foreach (var bit in bits)
            {
                try
                {
                    charArray[index++] = data[bit + offset];
                }
                catch (IndexOutOfRangeException)
                {
                    Prime.Services.ConsoleService.PrintError($"ExtractBits(data length:{data.Length}, bits:[{string.Join(", ", bits)}], offset:{offset}): Index out of range on Bit=[{bit}].");
                    throw;
                }
            }

            return new string(charArray);
        }

        /// <summary>
        /// String extension method. Extract the given bits from the given binary string.
        /// </summary>
        /// <param name="data">Binary Data string. Bit 0 is the first (leftmost) bit in the string.</param>
        /// <param name="bits">Range specifying which bits to extract. Should be a string with comma separated bits, or - to specify a range.  ie.  0,5-7,9 will extract bits 0, 5, 6, 7 and 9.</param>
        /// <param name="offset">Optional offset to add to each element in the bits range.</param>
        /// <returns>Substring based on the requested bits.</returns>
        public static string ExtractBits(this string data, string bits, int offset = 0)
        {
            var rangeDelim = new char[] { '-' };
            string retval = string.Empty;
            foreach (var item in bits.Split(','))
            {
                try
                {
                    if (item.Contains('-'))
                    {
                        var rangePair = item.Split(rangeDelim, 2);
                        var r1 = int.Parse(rangePair[0]);
                        var r2 = int.Parse(rangePair[1]);
                        if (r1 < r2)
                        {
                            retval += data.ExtractBits(r1 + offset, r2 - r1 + 1, countDown: false);
                        }
                        else
                        {
                            retval += data.ExtractBits(r1 + offset, r1 - r2 + 1, countDown: true);
                        }
                    }
                    else
                    {
                        retval += data[int.Parse(item) + offset];
                    }
                }
                catch (FormatException)
                {
                    Prime.Services.ConsoleService.PrintError($"ExtractBits(bits:{bits}, offset:{offset}): Failed to convert item or range to int. Item=[{item}].");
                    throw;
                }
                catch (IndexOutOfRangeException)
                {
                    Prime.Services.ConsoleService.PrintError($"ExtractBits(data length:{data.Length}, bits:{bits}, offset:{offset}): Index out of range on Item=[{item}].");
                    throw;
                }
            }

            return retval;
        }

        /// <summary>
        /// String extension method. Extract the given bits from the given binary string.
        /// </summary>
        /// <param name="data">Binary Data string. Bit 0 is the first (leftmost) bit in the string.</param>
        /// <param name="bitStart">First bit to extract.</param>
        /// <param name="bitCount">Number of consecutive bits to extract.</param>
        /// <param name="countDown">Specifies the order bits are extracted. ie if given bitStart=4, bitCount=3, countDown=false(default) extracts bits 4,5,6, but countDown=true extracts bits 4,3,2.</param>
        /// <returns>Substring based on the requested bits.</returns>
        public static string ExtractBits(this string data, int bitStart, int bitCount, bool countDown = false)
        {
            char[] charArray = new char[bitCount];
            int order = countDown ? -1 : 1;
            for (var i = 0; i < bitCount; i++)
            {
                try
                {
                    charArray[i] = data[bitStart + (order * i)];
                }
                catch (IndexOutOfRangeException)
                {
                    Prime.Services.ConsoleService.PrintError($"ExtractBits(data length:{data.Length}, bitStart:{bitStart}, bitCount:{bitCount}, countDown:{countDown}): Index out of range on bit number {i}, Value=[{bitStart + (order * i)}].");
                    throw;
                }
            }

            return new string(charArray);
        }

        /// <summary>
        /// Writes data to Ituff using tname/strgval tokens.
        /// </summary>
        /// <param name="tnamePostFix">Output tname will be TestInstanceName_tnamePostFix. If empty, only the Test Instance name will be used.</param>
        /// <param name="strgval">strgval data to write.</param>
        public static void WriteStrgvalToItuff(string tnamePostFix, string strgval)
        {
            Prime.Services.ConsoleService.PrintDebug($"[WriteStrgvalToItuff] with tnamePostfix=[{tnamePostFix}] Data=[{strgval}]");
            var writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            if (tnamePostFix != string.Empty)
            {
                writer.SetTnamePostfix(tnamePostFix);
            }

            writer.SetData(strgval);
            Prime.Services.DatalogService.WriteToItuff(writer);
        }
    }
}
