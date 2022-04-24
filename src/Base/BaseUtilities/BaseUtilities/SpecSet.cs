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
    using System;

    /// <summary>
    /// Defines the <see cref="SpecSet" />.
    /// </summary>
    public static class SpecSet
    {
        /// <summary>
        /// Checks if a token exists, returns true if it does. Throws an exception
        /// if the token is not in SpecSet format.
        /// </summary>
        /// <param name="token">SpecSet token name of the form S.[TL].Name.</param>
        /// <returns>SpecSet string value.</returns>
        public static string ReadToken(string token)
        {
            string[] splitToken;
            try
            {
                splitToken = SplitAndValidateTokenSize(token);
            }
            catch
            {
                return null;
            }

            var testConditionName = splitToken[1] == "L" ? Prime.Services.TestProgramService.GetCurrentLevels() : Prime.Services.TestProgramService.GetCurrentTimings();
            var testCondition = Prime.Services.TestConditionService.GetTestCondition(testConditionName);
            return testCondition.GetSpecSetValue(splitToken[2]);
        }

        private static string[] SplitAndValidateTokenSize(string token)
        {
            var splitToken = token.Split(new char[] { '.' }, 3);
            if (splitToken.Length != 3 || splitToken[0] != "S" || (splitToken[1] != "T" && splitToken[1] != "L"))
            {
                throw new ArgumentException($"SpecSet token=[{token}] is the wrong format [Does not contain at least 2 periods (.)], expecting S.[TL].name", nameof(token));
            }

            return splitToken;
        }
    }
}
