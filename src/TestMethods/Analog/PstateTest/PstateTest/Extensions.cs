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

namespace PstateTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class implements extension string methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Reverse a string.
        /// </summary>
        /// <param name="s">String to be reversed.</param>
        /// <returns>String.</returns>
        public static string ReverseString(this string s)
        {
            return new string(s.ToCharArray().Reverse().ToArray());
        }

        /// <summary>
        /// Interpret string as operator.
        /// </summary>
        /// <param name="logic">string containing operator.</param>
        /// <param name="val">value to assess.</param>
        /// <param name="lim">limit to apply.</param>
        /// <param name="result">bool indicating pass/fail status.</param>
        /// <returns>bool indicating operation successful.</returns>
        public static bool Operator(this string logic, int val, int lim, out bool result)
        {
            switch (logic)
            {
                case ">":
                    result = val > lim;
                    return true;
                case ">=":
                    result = val >= lim;
                    return true;
                case "<":
                    result = val < lim;
                    return true;
                case "<=":
                    result = val <= lim;
                    return true;
                case "==":
                    result = val == lim;
                    return true;
                default:
                    result = false;
                    throw new Exception("Invalid operator.");
            }
        }
    }
}
