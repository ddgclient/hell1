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

namespace PTHBgTrimTC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Prime;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.Utilities;
    using PTHLib;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class PTHBgTrimTC : TestMethodBase
    {
        private ICaptureFailureAndCtvPerCycleTest ctvTest;
        private List<string> capturePins;
        private bool ctvTestStatus;

        /// <summary>
        /// Gets or sets Patlist to execute.
        /// </summary>
        public TestMethodsParams.Plist Patlist { get; set; }

        /// <summary>
        /// Gets or sets TimingsTc for plist execution.
        /// </summary>
        public TestMethodsParams.TimingCondition TimingsTc { get; set; }

        /// <summary>
        /// Gets or sets LevelsTc to plist execution.
        /// </summary>
        public TestMethodsParams.LevelsCondition LevelsTc { get; set; }

        /// <summary>
        /// Gets or sets comma separated pins for CTV capture.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString CtvCapturePins { get; set; }

        /// <summary>
        /// Gets or sets pattern name for which to capture and decode CTV.
        /// </summary>
        public TestMethodsParams.String PatternName { get; set; }

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
        public TestMethodsParams.Integer CodeLimitRange { get; set; }

        /// <summary>
        /// Gets or sets EnableStopwatch for test execution time tracking.
        /// </summary>
        public TestMethodsParams.UnsignedInteger EnableStopwatch { get; set; } = 0;

        /// <summary>
        /// Gets or sets preplist.
        /// </summary>
        public TestMethodsParams.String PrePlist { get; set; } = string.Empty;

        /// <inheritdoc />
        public override void Verify()
        {
            this.capturePins = this.CtvCapturePins.ToList();
            if (this.capturePins.Count != 1)
            {
                throw new ArgumentException("Expecting exactly one capture pin - TDO pin with correct name.\n");
            }

            if (this.PatternName == string.Empty | this.GSDSAvgName == string.Empty)
            {
                throw new ArgumentException("PatternName / GSDSAvgName cannot be empty.\n");
            }

            if ((this.CodeLimitMax == null) | (this.CodeLimitMin == null) | (this.CodeLimitRange == null))
            {
                throw new ArgumentException("Parameters CodeLimitMax, CodeLimitMin, RangeLimit are all required. They cannot be empty.\n");
            }

            if (this.CodeLimitMax < this.CodeLimitMin)
            {
                throw new ArgumentException("CodeLimitMax cannot be less than CodeLimitMin.\n");
            }

            this.ctvTest = Prime.Services.FunctionalService.CreateCaptureFailureAndCtvPerCycleTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.capturePins, 1000, 1000, this.PrePlist);
        }

        /// <summary>
        /// Port 1: passes.
        /// Port 0: invalid input data.
        /// Port 3: fails. No data captured.
        /// Port 4: fails. Number of bits captured per pattern does not match expectation.
        /// Port 5: fails. Done or Error bit failed. Done should be 1 while Error should be 0.
        /// Port 6: fails. Max limit is violated.
        /// Port 4: fails. Min limit is violated.
        /// Port 5: fails. Range limit is violated.
        /// </summary>
        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        [Returns(3, PortType.Fail, "Fail!")]
        [Returns(4, PortType.Fail, "Fail!")]
        [Returns(5, PortType.Fail, "Fail!")]
        [Returns(6, PortType.Fail, "Fail!")]
        [Returns(7, PortType.Fail, "Fail!")]
        [Returns(8, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            Stopwatch stopWatch = new Stopwatch();
            long elapsedTime;
            if (this.EnableStopwatch == 1)
            {
                stopWatch.Start();
            }

            string ctvData;
            string pinName = this.capturePins.First();
            int dataLength = 10;
            string doneBit, errorBit, codeStr;
            int trimCode;
            int patIndex = 0;
            bool failDoneOrError = false;
            List<int> trimCodeList = new List<int>();
            int avgValue = -999;
            int maxValue = -999;
            int minValue = -999;
            int rangeValue = -999;

            // CTV test execution.
            this.ctvTest.ApplyTestConditions();
            this.ctvTestStatus = this.ctvTest.Execute();

            if (!this.ctvTestStatus)
            {
                var failData = this.ctvTest.GetPerCycleFailures();
                Prime.Services.ConsoleService.PrintDebug("CTV test execution failed");
                Prime.Services.ConsoleService.PrintDebug(failData.ToString());

                return 0;
            }

            // Process CTV data per pattern.
            List<ICtvPerPattern> ctvDataList = this.ctvTest.GetCtvPerPattern(this.PatternName);

            if (ctvDataList.Count == 0)
            {
                Prime.Services.ConsoleService.PrintDebug("No data captured.\n");
                return 3;
            }

            foreach (var perPatternCTV in ctvDataList)
            {
                ctvData = perPatternCTV.GetCtvData(pinName);
                if (ctvData.Length != dataLength)
                {
                    Prime.Services.ConsoleService.PrintDebug(
                        "Expected length of CTV data for " + this.PatternName + " to be " + dataLength.ToString() +
                        ", captured " + ctvData.Length.ToString() + ".\n");
                    return 4;
                }

                errorBit = Convert.ToString(ctvData[0]);
                doneBit = Convert.ToString(ctvData[1]);
                codeStr = ctvData.Substring(2, 8);
                trimCode = Convert.ToInt32(PTHLib.Reverse(codeStr), 2);
                trimCodeList.Add(trimCode);

                if (errorBit.Equals("1") | doneBit.Equals("0"))
                {
                    failDoneOrError = true;
                }

                Prime.Services.ConsoleService.PrintDebug(
                        "Error: " + errorBit + ", Done: " + doneBit + ", TrimCode: " + trimCode + ".\n");

                PTHLib.PrintListAsPipeSeparatedToItuff(
                    "BGR_" + patIndex.ToString(),
                    new List<string> { errorBit, doneBit, trimCode.ToString() });

                // Normally, each trim pattern is run at least 3 times.
                patIndex++;
            }

            if (failDoneOrError)
            {
                return 5;
            }

            // Post-process captured data (min, max, avg).
            avgValue = (int)Math.Round(trimCodeList.Average());
            maxValue = trimCodeList.Max();
            minValue = trimCodeList.Min();
            rangeValue = maxValue - minValue;

            Prime.Services.SharedStorageService.InsertRowAtTable(this.GSDSAvgName, avgValue, Prime.SharedStorageService.Context.DUT);

            // Write the values to ituff.
            PTHLib.PrintAvgMaxMinRangeResultsToItuff(avgValue, maxValue, minValue, rangeValue);

            // If max limit is violated, test fails and exits port 6.
            if (avgValue > this.CodeLimitMax)
            {
                Prime.Services.ConsoleService.PrintDebug("Failing max limit. " + avgValue.ToString() + " > " + this.CodeLimitMax.ToString() + ".\n");
                return 6;
            }

            // If min limit is violated, test fails and exits port 7.
            if (avgValue < this.CodeLimitMin)
            {
                Prime.Services.ConsoleService.PrintDebug("Failing min limit. " + avgValue.ToString() + " < " + this.CodeLimitMin.ToString() + ".\n");
                return 7;
            }

            // If range limit is violated, test fails and exits port 5.
            if (rangeValue > this.CodeLimitRange)
            {
                Prime.Services.ConsoleService.PrintDebug("Failing range limit. " + rangeValue.ToString() + " > " + this.CodeLimitRange.ToString() + ".\n");
                return 8;
            }

            if (this.EnableStopwatch == 1)
            {
                stopWatch.Stop();
                elapsedTime = stopWatch.ElapsedMilliseconds;
                stopWatch.Reset();
                Prime.Services.ConsoleService.PrintDebug("Stopwatch elapsed time: " + elapsedTime.ToString() + ".\n");
            }

            return 1;
        }
    }
}
