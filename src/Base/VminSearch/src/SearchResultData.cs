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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Prime.TestMethods.VminSearch.Helpers;

    /// <summary>
    /// A class to accumulate multi-pass results.
    /// </summary>
    public class SearchResultData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultData"/> class.
        /// </summary>
        /// <param name="pointTest">Object to provide search point test information.</param>>
        /// <param name="searchIdentifiers">A struct containing the current search identifiers.</param>
        /// <param name="isPass">A bool indicating whether the search passed or not.</param>>
        public SearchResultData(SearchStateValues pointTest, bool isPass, SearchIdentifiers searchIdentifiers)
        {
            this.Voltages = new List<double>(pointTest.Voltages);
            this.StartVoltages = new List<double>(pointTest.StartVoltages);
            this.EndVoltageLimits = new List<double>(pointTest.EndVoltageLimits);
            this.IsPass = isPass;
            this.FailReason = pointTest.FailReason;
            this.TnamePostfix = searchIdentifiers.TnamePostfix;
            this.MultiPassCount = searchIdentifiers.MultiPassCount;
            this.RepetitionCount = searchIdentifiers.RepetitionCount;
            this.MaskBits = new BitArray(pointTest.MaskBits);
            this.ExecutionCount = pointTest.ExecutionCount;
            this.PerTargetIncrements = new List<uint>(pointTest.PerTargetIncrements);
            this.VoltageLimitingPatterns = Enumerable.Repeat(PrimeVminSearchTestMethod.NoLimitingPatternToken, this.Voltages.Count).ToList();
            this.GetLimitingPatterns(pointTest.PerPointData);
        }

        /// <summary>
        /// Gets a list containing the resulting voltages.
        /// </summary>>
        public List<double> Voltages { get; }

        /// <summary>
        /// Gets a list containing the starting voltage values.
        /// </summary>
        public List<double> StartVoltages { get; }

        /// <summary>
        /// Gets a list containing the end voltage values.
        /// </summary>
        public List<double> EndVoltageLimits { get; }

        /// <summary>
        /// Gets a value indicating whether the multi-pass failed or passed.
        /// </summary>
        public bool IsPass { get; }

        /// <summary>
        /// Gets an array of bits values.
        /// </summary>
        public BitArray MaskBits { get; }

        /// <summary>
        /// Gets a list of strings containing the failing patterns per target.
        /// </summary>
        public List<string> VoltageLimitingPatterns { get; }

        /// <summary>
        /// Gets a value indicating the execution count.
        /// </summary>
        public uint ExecutionCount { get; }

        /// <summary>
        /// Gets a list containing the iterations per target.
        /// </summary>
        public List<uint> PerTargetIncrements { get; }

        /// <summary>
        /// Gets a string of the form MxRy. Where x is the multi pass execution count and y is the repetition count.
        /// </summary>
        public string TnamePostfix { get; }

        /// <summary>
        /// Gets an unsigned integer identifying the number of multi pass execution.
        /// </summary>
        public uint MultiPassCount { get; }

        /// <summary>
        /// Gets an unsigned integer identifying the number of the repetition count.
        /// </summary>
        public uint RepetitionCount { get; }

        /// <summary>
        /// Gets a failing reason (if any) for logging purposes.
        /// </summary>
        public string FailReason { get; }

        /// <summary>
        /// Selects the limiting pattern for each search target.
        /// </summary>
        /// <param name="searchPointData">List containing the voltages, mask bits and fail pattern per point data.</param>
        private void GetLimitingPatterns(IReadOnlyList<SearchPointData> searchPointData)
        {
            if (searchPointData.Count == 1)
            {
                for (var target = 0; target < this.Voltages.Count; target++)
                {
                    this.VoltageLimitingPatterns[target] = searchPointData[0].FailPatternData.PatternName;
                }

                return;
            }

            var lastVoltages = new List<double>(searchPointData.Last().Voltages);
            for (var target = 0; target < this.Voltages.Count; target++)
            {
                if (lastVoltages[target] > 0)
                {
                    for (var iteration = searchPointData.Count - 2; iteration >= 0; iteration--)
                    {
                        if (searchPointData[iteration].Voltages[target].IsDifferent(lastVoltages[target]))
                        {
                            this.VoltageLimitingPatterns[target] = searchPointData[iteration].FailPatternData.PatternName;
                            break;
                        }
                    }
                }
                else if (lastVoltages[target].IsEqual(SearchPointTest.VoltageFailValue))
                {
                    for (var iteration = searchPointData.Count - 2; iteration >= 0; iteration--)
                    {
                        if (searchPointData[iteration].Voltages[target] > 0)
                        {
                            this.VoltageLimitingPatterns[target] = searchPointData[iteration].FailPatternData.PatternName;
                            break;
                        }
                    }
                }
            }
        }
    }
}
