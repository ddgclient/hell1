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

namespace PTHCalcTC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using PTHLib;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class PTHCalcTC : TestMethodBase
    {
        /// <summary>
        /// Gets or sets list of GSDS names.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString ListGSDSInputNames { get; set; }

        /// <summary>
        /// Gets or sets name to use for Avg value.
        /// </summary>
        public TestMethodsParams.String GSDSAvgName { get; set; }

        /// <summary>
        /// Gets or sets limits for average BG code.
        /// </summary>
        public TestMethodsParams.Integer CodeLimitMax { get; set; }

        /// <summary>
        /// Gets or sets limits for average BG code.
        /// </summary>
        public TestMethodsParams.Integer CodeLimitMin { get; set; }

        /// <summary>
        /// Gets or sets limit for range.
        /// </summary>
        public TestMethodsParams.Integer RangeLimit { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            if ((this.ListGSDSInputNames == null) | (this.GSDSAvgName == null) |
                (this.CodeLimitMax == null) | (this.CodeLimitMin == null) | (this.RangeLimit == null))
            {
                throw new Prime.Base.Exceptions.FatalException(
                    "Parameters ListGSDSInputNames, GSDSAvgName CodeLimitMax, CodeLimitMin, RangeLimit are all required. " +
                    "They cannot be empty.");
            }
        }

        /// <summary>
        /// Port 1: passes.
        /// Port 0: invalid input data.
        /// Port 3: fails max limit.
        /// Port 4: fails min limit.
        /// Port 5: fails range limit.
        /// </summary>
        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        [Returns(3, PortType.Fail, "Fail!")]
        [Returns(4, PortType.Fail, "Fail!")]
        [Returns(5, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            List<int> listValues = new List<int>();
            int avgValue = -999;
            int maxValue = -999;
            int minValue = -999;
            int rangeValue = -999;
            int gsdsValue;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            long elapsedTime;

            foreach (string gsdsName in this.ListGSDSInputNames.ToList())
            {
                gsdsValue = Prime.Services.SharedStorageService.GetIntegerRowFromTable(gsdsName, Prime.SharedStorageService.Context.DUT);
                if (gsdsValue > 0)
                {
                    listValues.Add(gsdsValue);
                }
                else
                {
                    // If any of the gsds values is 0, update the gsds with -999 and return 0.
                    Prime.Services.ConsoleService.PrintDebug("Issues when processing GSDS input: " + gsdsName + ".\n");
                    Prime.Services.SharedStorageService.InsertRowAtTable(this.GSDSAvgName, avgValue, Prime.SharedStorageService.Context.DUT);

                    stopWatch.Stop();
                    elapsedTime = stopWatch.ElapsedMilliseconds;
                    stopWatch.Reset();
                    Prime.Services.ConsoleService.PrintDebug("Stopwatch elapsed time: " + elapsedTime.ToString() + ".\n");

                    // Write the values to ituff.
                    PTHLib.PrintAvgMaxMinRangeResultsToItuff(avgValue, maxValue, minValue, rangeValue);

                    return 0;
                }
            }

            avgValue = (int)Math.Round(listValues.Average());
            maxValue = listValues.Max();
            minValue = listValues.Min();
            rangeValue = maxValue - minValue;

            Prime.Services.SharedStorageService.InsertRowAtTable(this.GSDSAvgName, avgValue, Prime.SharedStorageService.Context.DUT);

            // Write the values to ituff.
            PTHLib.PrintAvgMaxMinRangeResultsToItuff(avgValue, maxValue, minValue, rangeValue);

            // If max limit is violated, test fails and exits port 3.
            if (avgValue > this.CodeLimitMax)
            {
                Prime.Services.ConsoleService.PrintDebug("Failing max limit. " + avgValue.ToString() + " > " + this.CodeLimitMax.ToString() + ".\n");

                stopWatch.Stop();
                elapsedTime = stopWatch.ElapsedMilliseconds;
                stopWatch.Reset();
                Prime.Services.ConsoleService.PrintDebug("Stopwatch elapsed time: " + elapsedTime.ToString() + ".\n");

                return 3;
            }

            // If min limit is violated, test fails and exits port 4.
            if (avgValue < this.CodeLimitMin)
            {
                Prime.Services.ConsoleService.PrintDebug("Failing min limit. " + avgValue.ToString() + " < " + this.CodeLimitMin.ToString() + ".\n");

                stopWatch.Stop();
                elapsedTime = stopWatch.ElapsedMilliseconds;
                stopWatch.Reset();
                Prime.Services.ConsoleService.PrintDebug("Stopwatch elapsed time: " + elapsedTime.ToString() + ".\n");

                return 4;
            }

            // If range limit is violated, test fails and exits port 5.
            if (rangeValue > this.RangeLimit)
            {
                Prime.Services.ConsoleService.PrintDebug("Failing range limit. " + rangeValue.ToString() + " > " + this.RangeLimit.ToString() + ".\n");

                stopWatch.Stop();
                elapsedTime = stopWatch.ElapsedMilliseconds;
                stopWatch.Reset();
                Prime.Services.ConsoleService.PrintDebug("Stopwatch elapsed time: " + elapsedTime.ToString() + ".\n");

                return 5;
            }

            stopWatch.Stop();
            elapsedTime = stopWatch.ElapsedMilliseconds;
            stopWatch.Reset();
            Prime.Services.ConsoleService.PrintDebug("Stopwatch elapsed time: " + elapsedTime.ToString() + ".\n");

            // Test passes.
            return 1;
        }
    }
}