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

namespace MbistVminTC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DDG;
    using Prime;

    /// <summary>
    /// A class to provide access to data-logging functionality.
    /// </summary>
    public static class ExtendedDataLogger
    {
        private const string TokenValueSeparator = "|";
        private const string TargetValueSeparator = "_";
        private const string ConfigurationSeparator = ":";

        /// <summary>
        /// Logs the resulting voltages from the search as well as the start and end voltage values for all search targets.
        /// </summary>
        /// <param name="cornerIdentifiers">Tuple list with corner_identifiers, flow number and VminForwarding corner interface.</param>
        /// >
        public static void LogVminConfiguration(List<Tuple<string, int, IVminForwardingCorner>> cornerIdentifiers)
        {
            if (cornerIdentifiers == null || !cornerIdentifiers.Any())
            {
                return;
            }

            var configurationWriter = Services.DatalogService.GetItuffStrgvalWriter();
            configurationWriter.SetTnamePostfix("_vminFwCfg");

            var value = string.Empty;
            foreach (var t in cornerIdentifiers)
            {
                value += t.Item1; // this is the corner_identifier name.
                value += ConfigurationSeparator;
                value += t.Item2.ToString(); // this is the flow number.
                value += ConfigurationSeparator;
                value += (DDG.VminForwarding.Service.GetFrequency(t.Item1, t.Item2) / 1e9).ToString("F3"); // Frequency in Ghz with 3 decimal places.
                value += TargetValueSeparator;
            }

            value = value.Remove(value.Length - 1, 1);
            configurationWriter.SetData(value);
            Services.DatalogService.WriteToItuff(configurationWriter);
        }

        /// <summary>
        /// Returns a string containing the characters that are mapped by the provided index map.
        /// </summary>
        /// <param name="stringToMap">The string from which to map the indexed characters.</param>
        /// <param name="indexMap">A string with comma-separated integer indexes.
        /// Each index represents the position of a corresponding character in the string to map.
        /// Positive indexes starting at 0 map to characters at the start of the string to map and forward.
        /// Negative indexes starting at -1 map to characters at the end of the string to map and backwards.</param>
        /// <returns>The string of characters that were mapped with the index map.</returns>
        public static string GetMappedString(string stringToMap, string indexMap)
        {
            var mappingIndexes = indexMap.Split(',').ToList();
            var result = mappingIndexes.Aggregate(string.Empty, (accumulator, currentValue) =>
            {
                var intIndex = int.Parse(currentValue);
                var positiveOnlyIndex = intIndex < 0 ? stringToMap.Length + intIndex : intIndex;
                return accumulator + stringToMap[positiveOnlyIndex];
            });

            return result;
        }
    }
}
