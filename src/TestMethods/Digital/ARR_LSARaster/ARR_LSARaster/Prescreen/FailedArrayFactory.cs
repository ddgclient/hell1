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

namespace LSARasterTC
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using Prime.FunctionalService;
    using Prime.PatternService;
    using Prime.TestMethods;

    /// <summary>
    /// Class responsible for creating objects that implement <see cref="FailedArray"/> during prescreen execution.
    /// </summary>
    public class FailedArrayFactory
    {
        /// <summary>
        /// Method for creating object that represents a failed array.
        /// </summary>
        /// <param name="arrayType"> Type of array determined by the MetadataConfig. </param>
        /// <param name="arrayName"> DFM failure to create the FailedArray from. </param>
        /// <returns> An object that implements <see cref="FailedArray"/>. </returns>
        public static FailedArray CreateFailedArray(MetadataConfig.ArrayType arrayType, string arrayName)
        {
            switch (arrayType)
            {
                case MetadataConfig.ArrayType.ATOM:
                    return new AtomArray(arrayName);
                case MetadataConfig.ArrayType.BIGCORE:
                    return new BigCoreArray(arrayName);
                default:
                    throw new Prime.Base.Exceptions.TestMethodException($"Could not create a failed array object for the given array type: {arrayType}");
            }
        }
    }
}
