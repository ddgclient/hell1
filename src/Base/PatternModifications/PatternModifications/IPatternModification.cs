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
    using Prime.PatConfigService;

    /// <summary>
    /// Interface for pattern modification. Helper for Prime PatConfig.
    /// </summary>
    public interface IPatternModification
    {
        /// <summary>
        /// Returns dictionary with lists of pat config handles.
        /// </summary>
        /// <param name="input">input text. Comma-separated: [ConfigurationName]![value],...,[ConfigurationName]![value] .</param>
        /// <param name="patList">Pattern list.</param>
        /// <returns>Dictionary where key is the modification type and value is the list of pat config handles.</returns>
        List<IPatConfigHandle> GetPatternConfigHandles(string input, string patList);

        /// <summary>
        /// Wrapper for application of pattern config handles. It will skip apply if null or empty.
        /// </summary>
        /// <param name="handles">List of IPatConfigHandle.</param>
        void ApplyPatternConfigHandles(List<IPatConfigHandle> handles);
    }
}
