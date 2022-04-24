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

namespace ARR_MBIST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Prime.Base.Exceptions;

    /// <summary>
    /// Static helper functions/enums/stuff for MBIST code.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "More explicit this way.")]
    public static class Mbist
    {
        /// <summary>Enum for repair types.</summary>
        public enum RepairType
        {
            /// <summary>Row Repair.</summary>
            ROW,

            /// <summary>Column Repair.</summary>
            COL,

            /// <summary>Word Repair.</summary>
            WORD,

            /// <summary>Both Row and Column Repair.</summary>
            BOTH,

            /// <summary>No Repair.</summary>
            NR,

            /// <summary>No Repair.</summary>
            NONE,
        }

        /// <summary>Enum for repair status.</summary>
        public enum RepairStatus
        {
            /// <summary>Repair Element is Available.</summary>
            AVAIL,

            /// <summary>Repair Element is Used.</summary>
            USED,
        }

        /// <summary>Enum for how to serialize capture data from multiple pins.</summary>
        public enum CaptureInterLeaveType
        {
            /// <summary>All bits from Pin[0] before Pin[1].</summary>
            PinFirst,

            /// <summary>First element of each pin, followed by the 2nd element of each pin, etc.</summary>
            CycleFirst,
        }

        /// <summary>
        /// Gets the order to iterate through repair types. This is to match the Python implentation.
        /// </summary>
        public static List<RepairType> RepairPriority { get; } = new List<RepairType> { RepairType.COL, RepairType.ROW, RepairType.WORD };

        private static string GlobalForHRY { get; } = "HRY_RAWSTR_MBIST";

        /// <summary>
        /// This functions takes a CtvCapturePins from HRY or Repair template and maps it to the
        /// CapturePins field from the Json input file. This is so that the json doesn't need IP
        /// scoping.  For sort/non-intradut this is mostly redundant.
        /// </summary>
        /// <param name="capturePins">CtvCapturePins from template (with ip scoping for intradut).</param>
        /// <param name="requiredPins">CapturePins field from JSON (does not require ip scoping ever).</param>
        /// <returns>IP Scoped list of capture pins in the correct order from the JSON input file.</returns>
        public static List<string> ResolveCapturePins(List<string> capturePins, List<string> requiredPins)
        {
            var retLst = new List<string>();
            if (requiredPins.Count > capturePins.Count)
            {
                Prime.Services.ConsoleService.PrintError($"Error, Parameter CtvCapturePins=[{string.Join(", ", capturePins)}] does not match RequiredPinList=[{string.Join(", ", requiredPins)}] *** Required PinList has less elements.");
                return new List<string>();
            }

            for (int i = 0; i < requiredPins.Count; i++)
            {
                if (requiredPins[i] == capturePins[i])
                {
                    retLst.Add(capturePins[i]);
                }
                else if (capturePins[i].Contains("::") && !requiredPins[i].Contains("::"))
                {
                    var l = capturePins[i].Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (l[l.Count - 1] == requiredPins[i])
                    {
                        // they match, but one has ip scoping, add that one.
                        retLst.Add(capturePins[i]);
                    }
                    else
                    {
                        Prime.Services.ConsoleService.PrintError($"Error, Parameter CtvCapturePins=[{string.Join(", ", capturePins)}] does not match RequiredPinList=[{string.Join(", ", requiredPins)}]");
                        return new List<string>();
                    }
                }
                else if (!capturePins[i].Contains("::") && requiredPins[i].Contains("::"))
                {
                    var l = requiredPins[i].Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (l[l.Count - 1] == capturePins[i])
                    {
                        // they match, but one has ip scoping, add that one.
                        retLst.Add(requiredPins[i]);
                    }
                    else
                    {
                        Prime.Services.ConsoleService.PrintError($"Error, Parameter CtvCapturePins=[{string.Join(", ", capturePins)}] does not match RequiredPinList=[{string.Join(", ", requiredPins)}]");
                        return new List<string>();
                    }
                }
            }

            return retLst;
        }

        /// <summary>
        /// Function to convert the per-pin capture data into a single serialized string.
        /// Supports two methods, PinFirst and CycleFirst
        ///      PinFirst:  Data = { Pin1Data[0-x], Pin2Data[0-x], ... }
        ///      CycleFirst: Data = { Pin1Data[0], Pin2Data[0], ..., Pin1Data[1], Pin2Data[1], ... }.
        /// </summary>
        /// <param name="ctvData">Capture data from Prime, Dictionary where Key=PinName, Value=string of capture dat.</param>
        /// <param name="pinlist">List of pins to extract capture data for, must be a subset of the pins in ctvDat.</param>
        /// <param name="interleaving">How to interleave the pindata, either PinFirst or CycleFirs.</param>
        /// <returns>Single string containing all the serialized capture data.</returns>
        public static string SerializeCaptureData(Dictionary<string, string> ctvData, List<string> pinlist, CaptureInterLeaveType interleaving)
        {
            string serializedData = string.Empty;
            if (pinlist.Count == 1)
            {
                // simple case just return the data from the selected pin.
                serializedData = ctvData[pinlist[0]];
            }
            else if (interleaving == CaptureInterLeaveType.PinFirst)
            {
                foreach (var pin in pinlist)
                {
                    serializedData += ctvData[pin];
                }
            }
            else
            {
                var dataLen = ctvData.First().Value.Length;
                var sb = new StringBuilder(dataLen * pinlist.Count);
                for (var i = 0; i < dataLen; i++)
                {
                    foreach (var pin in pinlist)
                    {
                        sb.Append(ctvData[pin][i]);
                    }
                }

                serializedData = sb.ToString();
            }

            return serializedData;
        }

        /// <summary>
        /// Simply reverses a string.  This is the quickest way I could find to do it.
        /// </summary>
        /// <param name="s">String to reverse.</param>
        /// <returns>Reversed String.</returns>
        public static string StringReverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Function to adjust a range by a given offset.
        /// </summary>
        /// <param name="range">Range value.</param>
        /// <param name="offset">Amount to shift range.</param>
        /// <returns>New Range.</returns>
        public static string AdjustRange(string range, int offset)
        {
            // should this be here or at the caller?
            if (range == MbistRasterInput.BitRanges.NA)
            {
                return MbistRasterInput.BitRanges.NA;
            }

            if (offset == 0)
            {
                return range;
            }

            var rangeDelim = new char[] { '-' };
            List<string> retval = new List<string>();
            foreach (var item in range.Split(','))
            {
                if (item.Contains('-'))
                {
                    var rangePair = item.Split(rangeDelim, 2);
                    var r1 = int.Parse(rangePair[0]);
                    var r2 = int.Parse(rangePair[1]);
                    retval.Add($"{r1 + offset}-{r2 + offset}");
                }
                else
                {
                    retval.Add($"{int.Parse(item) + offset}");
                }
            }

            return string.Join(",", retval);
        }

        /// <summary>
        /// Function to convert a string range into a list of integers.
        /// Example ranges: "3,6-8" becomes { 3, 6, 7, 8 }.
        /// </summary>
        /// <param name="range">Range to convert.</param>
        /// <returns>List of indexes.</returns>
        public static List<int> RangeToList(string range)
        {
            List<int> retval = new List<int>();

            if (range == MbistRasterInput.BitRanges.NA)
            {
                return retval;
            }

            var rangeDelim = new char[] { '-' };
            foreach (var item in range.Split(','))
            {
                if (item.Contains('-'))
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
        /// Gets the number of bits in a range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>Number of bits in the range.</returns>
        public static int RangeSize(string range)
        {
            return RangeToList(range).Count;
        }

        /// <summary>
        /// Replaces a substring at the given indexes in the full string.
        /// </summary>
        /// <param name="range">Range value giving the index positions in the full string.</param>
        /// <param name="subDataStr">SubString to write.</param>
        /// <param name="fullDataStr">Full String to update.</param>
        /// <returns>Updated full string.</returns>
        public static string SetSubData(string range, string subDataStr, string fullDataStr)
        {
            // FIXME really should do some error checking
            var subData = subDataStr.ToCharArray();
            var fullData = fullDataStr.ToCharArray();
            var indexes = RangeToList(range);
            uint subIndex = 0;
            foreach (int i in indexes)
            {
                fullData[i] = subData[subIndex++];
            }

            return new string(fullData);
        }

        /// <summary>
        /// Get a substring based on a range (bit locations separated by commas or bitranges using -).
        /// </summary>
        /// <param name="range">Indexes of the data to return. ie "1,3,5,9-11,15".</param>
        /// <param name="data">Full string.</param>
        /// <returns>SubString.</returns>
        public static string GetSubData(string range, string data)
        {
            // should this be here or at the caller?
            if (range == MbistRasterInput.BitRanges.NA)
            {
                return string.Empty;
            }

            /* FIXME - PERFORMANCE issues with this function.*/
            var rangeDelim = new char[] { '-' };
            string retval = string.Empty;
            foreach (var item in range.Split(','))
            {
                if (item.Contains('-'))
                {
                    var rangePair = item.Split(rangeDelim, 2);
                    var r1 = int.Parse(rangePair[0]);
                    var r2 = int.Parse(rangePair[1]);
                    if (r1 < r2)
                    {
                        retval += data.Substring(r1, r2 - r1 + 1);
                    }
                    else
                    {
                        retval += Mbist.StringReverse(data.Substring(r2, r1 - r2 + 1));
                    }
                }
                else
                {
                    retval += data.Substring(int.Parse(item), 1);
                }
            }

            return retval;
        }

        /// <summary>
        /// Get the current MBIST HRY string.
        /// </summary>
        /// <returns>full HRY string.</returns>
        public static string GetMbistHRYData()
        {
            string hry;
            try
            {
                hry = Prime.Services.SharedStorageService.GetStringRowFromTable(GlobalForHRY, Prime.SharedStorageService.Context.DUT);
            }
            catch (FatalException e)
            {
                Prime.Services.ConsoleService.PrintDebug($"Prime failed to get {GlobalForHRY} fromS haredStorage - {e.GetType()}: {e.Message}");
                hry = string.Empty;
            }

            Prime.Services.ConsoleService.PrintDebug($"Read {GlobalForHRY}=[{hry}].");
            return hry;
        }

        /// <summary>
        /// Writes the given string to the MBIST HRY Global.
        /// </summary>
        /// <param name="hry">HRY data to write.</param>
        public static void SetMbistHRYData(string hry)
        {
            Prime.Services.ConsoleService.PrintDebug($"Writing {hry} to SharedStorage Row {GlobalForHRY}.");
            Prime.Services.SharedStorageService.InsertRowAtTable(GlobalForHRY, hry, Prime.SharedStorageService.Context.DUT);
        }
    }
}
