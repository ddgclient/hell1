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
    /// Defines the <see cref="DoubleExtensions" />.
    /// </summary>
    public static class DoubleExtensions
    {
        /// <summary>
        /// Checks if 2 double values are equal to the given number of decimal places.
        /// </summary>
        /// <param name="number1">Double value.</param>
        /// <param name="number2">Double Value.</param>
        /// <param name="decimalPlaces">Number of decimal places to check for equality (0 means exact match).</param>
        /// <returns>true if the values are equivalent.</returns>
        public static bool Equals(this double number1, double number2, uint decimalPlaces)
        {
            if (decimalPlaces == 0)
            {
                return number1 == number2;
            }

            return Math.Abs(number1 - number2) <= Math.Pow(0.1, decimalPlaces);
        }
    }
}
