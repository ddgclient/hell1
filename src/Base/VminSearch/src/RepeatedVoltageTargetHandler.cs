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

namespace Prime.TestMethods.VminSearch
{
    using System.Collections.Generic;
    using System.Linq;
    using Prime.TestMethods.VminSearch.Helpers;

    /// <summary>
    /// Class for handling the repeated (ganged) voltage targets feature.
    /// </summary>
    internal sealed class RepeatedVoltageTargetHandler
    {
        private readonly MultiMap<string, int> repeatedVoltageTargets;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatedVoltageTargetHandler"/> class.
        /// </summary>
        /// <param name="targets">Targets name list.</param>
        public RepeatedVoltageTargetHandler(IReadOnlyList<string> targets)
        {
            this.repeatedVoltageTargets = new MultiMap<string, int>();

            // Gets only the repeated/ganged items.
            var repeatedItems = targets.GroupBy(target => target)
                .Where(target => target.Count() > 1).ToDictionary(x => x.Key, y => y.Key);

            // Multimap setup to avoid O(N) target search.
            for (var targetIndex = 0; targetIndex < targets.Count; targetIndex++)
            {
                // Adds only if the item is a repeated one.
                if (repeatedItems.ContainsKey(targets[targetIndex]))
                {
                    this.repeatedVoltageTargets.Add(targets[targetIndex], targetIndex);
                }
            }

            this.AreRepeatedTargets = this.repeatedVoltageTargets.Keys.Any();
        }

        /// <summary>
        /// Gets a value indicating whether at least one target is repeated.
        /// </summary>
        public bool AreRepeatedTargets { get; }

        /// <summary>
        /// Updates the repeated/linked voltage targets with the rules of core linking/ganging.
        /// </summary>
        /// <param name="voltages">Voltage values per target.</param>
        /// <returns>List of voltages with updated values.</returns>
        public List<double> UpdateRepeatedVoltageTargets(List<double> voltages)
        {
            var result = voltages;

            foreach (var voltageTarget in this.repeatedVoltageTargets.Keys)
            {
                var voltageToUpdate = 0D;

                // Find the max voltage.
                this.repeatedVoltageTargets[voltageTarget].ForEach(voltageIndex =>
                {
                    if (voltages[voltageIndex] >= voltageToUpdate)
                    {
                        voltageToUpdate = voltages[voltageIndex];
                    }
                });

                // Apply the max voltage to all non-masked/fail targets.
                this.repeatedVoltageTargets[voltageTarget].ForEach(voltageIndex =>
                {
                    if (voltages[voltageIndex] >= 0 && voltageToUpdate > voltages[voltageIndex])
                    {
                        result[voltageIndex] = voltageToUpdate;
                    }
                });
            }

            return result;
        }
    }
}
