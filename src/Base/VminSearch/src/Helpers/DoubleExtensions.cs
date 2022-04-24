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

namespace Prime.TestMethods.VminSearch.Helpers
{
    using System;

    /// <summary>
    /// Double extensions helper class to handle mathematical equations among doubles.
    /// </summary>
    internal static class DoubleExtensions
    {
        private const double RequiredPrecision = 0.001;

        /// <summary>
        /// Method to compare two double values to check if they are equal within a precision of three decimals.
        /// </summary>
        /// <param name="value">Value of interest.</param>
        /// <param name="compareTo">Value to compare.</param>
        /// <returns>A bool indicating if the two values are equal.</returns>
        public static bool IsEqual(this double value, double compareTo) =>
            Math.Abs(value - compareTo) < RequiredPrecision;

        /// <summary>
        /// Method to compare two double values to check if they are different within a precision of three decimals.
        /// </summary>
        /// <param name="value">Value of interest.</param>
        /// <param name="compareTo">Value to compare.</param>
        /// <returns>A bool indicating if the two values are different.</returns>
        public static bool IsDifferent(this double value, double compareTo) =>
            Math.Abs(value - compareTo) >= RequiredPrecision;
    }
}