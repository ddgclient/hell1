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
    using System.Collections.Generic;
    using System.Linq;
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="TestProgramServiceExtensions" />.
    /// </summary>
    public static class TestProgramServiceExtensions
    {
        /// <summary>
        /// Returns current instance pattern lists name.
        /// </summary>
        /// <param name="service">Service interface.</param>
        /// <returns>Pattern list name.</returns>
        public static List<string> GetCurrentPatternLists(this ITestProgramService service)
        {
            var result = new List<string>();
            var possibleParameters = new List<string>
            {
                "patlist",
                "reset_patlist",
                "retest_patlist",
                "arbiter_patlist",
                "primary_patlist",
                "secondary_patlist",
                "leak_high_patlist",
                "leak_low_patlist",
                "prescreen_patlist",
                "second_patlist",
                "search_reset_patlist",
                "Patlist",
            };
            var parameters = service.GetCurrentTestInstanceParameters();
            foreach (var p in possibleParameters)
            {
                if (parameters.ContainsKey(p))
                {
                    result.Add(parameters[p]);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns current instance timings.
        /// </summary>
        /// <param name="service">Service interface.</param>
        /// <returns>Pattern list name.</returns>
        public static string GetCurrentTimings(this ITestProgramService service)
        {
            var result = new List<string>();
            var possibleParameters = new List<string>
            {
                "timing",
                "timings",
                "TimingsTc",
                "TimingTc",
            };
            var parameters = service.GetCurrentTestInstanceParameters();
            return (from p in possibleParameters where parameters.ContainsKey(p) select parameters[p]).FirstOrDefault();
        }

        /// <summary>
        /// Returns current instance levels.
        /// </summary>
        /// <param name="service">Service interface.</param>
        /// <returns>Pattern list name.</returns>
        public static string GetCurrentLevels(this ITestProgramService service)
        {
            var result = new List<string>();
            var possibleParameters = new List<string>
            {
                "level",
                "levels",
                "LevelTc",
                "LevelsTc",
            };
            var parameters = service.GetCurrentTestInstanceParameters();
            return (from p in possibleParameters where parameters.ContainsKey(p) select parameters[p]).FirstOrDefault();
        }

        /// <summary>
        /// Returns current instance flow number.
        /// </summary>
        /// <param name="service">Service interface.</param>
        /// <returns>Flow number.</returns>
        public static int GetCurrentFlowNumber(this ITestProgramService service)
        {
            var parameters = service.GetCurrentTestInstanceParameters();
            foreach (var p in parameters)
            {
                if (p.Key == "FlowNumber" || p.Key == "FlowIndex")
                {
                    return p.Value.ToInt();
                }

                if (p.Key == "setting_values")
                {
                    var values = p.Value.Split(',');
                    foreach (var value in values)
                    {
                        var tokens = value.Split(':');
                        if (tokens.Length == 2)
                        {
                            if (tokens[0].Trim() == "flow")
                            {
                                return tokens[1].Trim().ToInt();
                            }
                        }
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the value of the LogLevel parameter for a Callback/Userfunction.
        /// Will be "DISABLED" if called from Evergreen code.
        /// </summary>
        /// <param name="service">Service interface.</param>
        /// <returns>Value of LogLevel, DISABLED, TEST_METHOD, or PRIME_DEBUG.</returns>
        public static string GetCurrentLogLevel(this ITestProgramService service)
        {
            try
            {
                var value = service.GetCurrentTestInstanceParameter("LogLevel");
                return string.IsNullOrWhiteSpace(value) ? "DISABLED" : value;
            }
            catch
            {
                return "DISABLED";
            }
        }
    }
}