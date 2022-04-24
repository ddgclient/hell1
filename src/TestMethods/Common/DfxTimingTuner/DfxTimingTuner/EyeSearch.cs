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

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DfxTimingTuner.UnitTest")]

namespace DfxTimingTuner
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines the <see cref="EyeSearch" />.
    /// </summary>
    internal static class EyeSearch
    {
        /// <summary>
        /// Gets the CTV Character (0 or 1) which represents a Pass value.
        /// </summary>
        internal static string CtvCharForPass { get; } = "0";

        /// <summary>
        /// Gets the CTV Character (0 or 1) which represents a Fail value.
        /// </summary>
        internal static string CtvCharForFail { get; } = "1";

        /// <summary>
        /// Gets a value indicating whether the test passed or failed. Used in BitArrays.
        /// </summary>
        internal static bool BitArrayBoolForPass { get; } = true;

        /// <summary>
        /// Find the center of the passing region.
        /// </summary>
        /// <param name="testResults">BitArray containing test results. True means pass, False means fail.</param>
        /// <param name="passingStartIndex">Output: Index where the largest starting region begins.</param>
        /// <param name="passingWidth">Output: Width of passing region.</param>
        /// <returns>True if search found a passing region, false otherwise.</returns>
        internal static bool FindLargestPassingRegion(BitArray testResults, out int passingStartIndex, out int passingWidth)
        {
            passingStartIndex = -1;
            passingWidth = -1;
            var currentPassingStart = -1;
            var currentPassingWidth = -1;

            for (var i = 0; i < testResults.Count; i++)
            {
                if (testResults.Get(i) == BitArrayBoolForPass)
                {
                    if (currentPassingStart < 0)
                    {
                        currentPassingStart = i;
                        currentPassingWidth = 1;
                    }
                    else
                    {
                        currentPassingWidth += 1;
                    }

                    if (currentPassingWidth > passingWidth)
                    {
                        passingWidth = currentPassingWidth;
                        passingStartIndex = currentPassingStart;
                    }
                }
                else
                {
                    currentPassingStart = -1;
                    currentPassingWidth = -1;
                }
            }

            return passingWidth > 0 ? true : false;
        }

        /// <summary>
        /// Converts the capture data from a DriveMode/TosTrigger test to per-pin results.
        /// </summary>
        /// <param name="ctvData">Single string containing CTV data.</param>
        /// <param name="testpoints">Number of testpoints.</param>
        /// <param name="decodePins">Mapping order of ctv bit locations to pin names.</param>
        /// <returns>Dictionary, Keys=PinNames, Values=BitArray of pass/fail results per testpoint.</returns>
        internal static Dictionary<string, BitArray> DriveModeSingleCtvToPerPinTestResults(string ctvData, int testpoints, List<string> decodePins)
        {
            if (ctvData.Length == 0 || ctvData.Length % testpoints != 0)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"CTVLength=[{ctvData.Length}] is not a multiple of NumberOfTestpoints=[{testpoints}].");
            }

            int bitsPerTestpoint = ctvData.Length / testpoints;
            if (bitsPerTestpoint % decodePins.Count != 0)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"BitsPerTestpoint=[{bitsPerTestpoint}] (calculated from CTVLength=[{ctvData.Length}] / NumberOfTestpoints=[{testpoints}]) is not a multiple of DecodePins=[{decodePins.Count}].");
            }

            // for each pin, combine all the results for each testpoint into a single value (fail if any results fails).
            Dictionary<string, BitArray> perpinTestResults = new Dictionary<string, BitArray>(decodePins.Count);
            for (var pinIndex = 0; pinIndex < decodePins.Count; pinIndex++)
            {
                var pinName = decodePins[pinIndex];
                perpinTestResults[pinName] = new BitArray(testpoints, BitArrayBoolForPass);
                for (var testpointIndex = 0; testpointIndex < testpoints; testpointIndex++)
                {
                    var ctv = ctvData.Substring(testpointIndex * bitsPerTestpoint, bitsPerTestpoint);
                    for (var ctvIndex = pinIndex; ctvIndex < ctv.Length; ctvIndex += decodePins.Count)
                    {
                        if (ctv.Substring(ctvIndex, 1) == CtvCharForFail)
                        {
                            perpinTestResults[pinName].Set(testpointIndex, !BitArrayBoolForPass);
                            break;
                        }
                    }
                }
            }

            return perpinTestResults;
        }

        /// <summary>
        /// Converts the capture data from a CompareMode/TosTrigger test to per-pin results.
        /// </summary>
        /// <param name="combinedCtvData">PerPin capture data combined for all testpoints.</param>
        /// <param name="testpoints">Number of testpoints.</param>
        /// <returns>Dictionary, Keys=PinNames, Values=BitArray of pass/fail results per testpoint.</returns>
        internal static Dictionary<string, BitArray> CompareModeMultiCtvToPerPinTestResults(Dictionary<string, string> combinedCtvData, int testpoints)
        {
            // Figure out how many bits are in each testpoint ... expect all to be the same so just use the first pin.
            var firstPinData = combinedCtvData.Values.First<string>();
            if (firstPinData.Length == 0 || firstPinData.Length % testpoints != 0)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"CTVLength=[{firstPinData.Length}] is not a multiple of NumberOfTestpoints=[{testpoints}].");
            }

            int bitsPerTestpoint = firstPinData.Length / testpoints;
            var perpinTestResults = new Dictionary<string, BitArray>(combinedCtvData.Count);
            foreach (var pinName in combinedCtvData.Keys)
            {
                perpinTestResults[pinName] = new BitArray(testpoints, BitArrayBoolForPass);
                for (var testPointIndex = 0; testPointIndex < testpoints; testPointIndex++)
                {
                    for (var ctvOffset = 0; ctvOffset < bitsPerTestpoint; ctvOffset++)
                    {
                        if (combinedCtvData[pinName].Substring((testPointIndex * bitsPerTestpoint) + ctvOffset, 1) == CtvCharForFail)
                        {
                            perpinTestResults[pinName].Set(testPointIndex, !BitArrayBoolForPass);
                            break;
                        }
                    }
                }
            }

            return perpinTestResults;
        }
    }
}
