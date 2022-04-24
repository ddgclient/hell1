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

namespace PTHLib
{
    using System;
    using System.Collections.Generic;
    using Prime;

    /// <summary>
    /// Class of methods to be shared across PTH TC.
    /// </summary>
    public class PTHLib
    {
        /// <summary>
        /// Writes statistics values (average, max, min and range) values to ITUFF.
        /// </summary>
        /// <param name="avgValue">The Average value to print to ituff.</param>
        /// <param name="maxValue">The Max value to print to ituff.</param>
        /// <param name="minValue">The Min value to print to ituff.</param>
        /// <param name="rangeValue">The Range value to print to ituff.</param>
        public static void PrintAvgMaxMinRangeResultsToItuff(int avgValue, int maxValue, int minValue, int rangeValue)
        {
            var strgvalWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();

            strgvalWriter.SetTnamePostfix("_AVG");
            strgvalWriter.SetData(avgValue.ToString());
            Prime.Services.DatalogService.WriteToItuff(strgvalWriter);

            strgvalWriter.SetTnamePostfix("_MAX");
            strgvalWriter.SetData(maxValue.ToString());
            Prime.Services.DatalogService.WriteToItuff(strgvalWriter);

            strgvalWriter.SetTnamePostfix("_MIN");
            strgvalWriter.SetData(minValue.ToString());
            Prime.Services.DatalogService.WriteToItuff(strgvalWriter);

            strgvalWriter.SetTnamePostfix("_RANGE");
            strgvalWriter.SetData(rangeValue.ToString());
            Prime.Services.DatalogService.WriteToItuff(strgvalWriter);
        }

        /// <summary>
        /// Method that takes in a list of strings and prints it as pipe separated string to Ituff.
        /// </summary>
        /// <param name="postFix">Post fix name to add to Ituff name.</param>
        /// <param name="strToItuff">List of strings to print to Ituff as pipe separated string.</param>
        public static void PrintListAsPipeSeparatedToItuff(string postFix, List<string> strToItuff)
        {
            var strgvalWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            string pipeSeparatedStr = string.Join("|", strToItuff);

            if (!postFix.Equals(string.Empty))
            {
                strgvalWriter.SetTnamePostfix("_" + postFix);
            }

            strgvalWriter.SetData(pipeSeparatedStr);
            Prime.Services.DatalogService.WriteToItuff(strgvalWriter);
        }

        /// <summary>
        /// Reverse the string provided as input.
        /// </summary>
        /// <param name="sCode">Input string.</param>
        /// <returns>"Reverse string of input".</returns>
        public static string Reverse(string sCode)
        {
            char[] cArray = sCode.ToCharArray();
            string reverse = string.Empty;
            for (int i = cArray.Length - 1; i > -1; i--)
            {
                reverse += cArray[i];
            }

            return reverse;
        }
    }
}