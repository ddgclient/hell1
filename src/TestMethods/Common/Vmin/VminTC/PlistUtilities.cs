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

namespace VminTC
{
    using System.Collections.Generic;
    using DDG;

    /// <summary>
    /// Plist utilities for Vmin searches.
    /// </summary>
    public static class PlistUtilities
    {
        /// <summary>
        /// Extracts plist contents index converting from position to occurrence.
        /// </summary>
        /// <param name="patlist">Patlist name.</param>
        /// <returns>List of pattern occurrences.</returns>
        public static List<PatternOccurrence> GetPlistContentsIndex(string patlist)
        {
            var plistObject = Prime.Services.PlistService.GetPlistObject(patlist);
            var plistContents = plistObject.GetPatternsAndIndexes(true);
            var plistContentsIndex = new List<PatternOccurrence>();
            foreach (var item in plistContents)
            {
                if (!item.IsPattern())
                {
                    continue;
                }

                ulong burstIndex = (ulong)item.GetBurstIndex();
                string patternName = item.GetPlistItemName();
                ulong index = (ulong)item.GetPatternIndex();
                var found = plistContentsIndex.FindLast(o => o.Burst == burstIndex && o.PatternName == patternName);
                var patternOccurrence = found == null ? new PatternOccurrence(burstIndex, patternName, 1, index) : new PatternOccurrence(burstIndex, patternName, found.Occurrence + 1, index);
                plistContentsIndex.Add(patternOccurrence);
            }

            return plistContentsIndex;
        }
    }
}
