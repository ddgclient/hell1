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
    using DTSBase;

    /// <summary>
    /// Interface for VoltageConverter methods extending PRIME capabilities.
    /// </summary>
    public interface IDTS
    {
        /// <summary>
        /// Gets configuration.
        /// </summary>
        /// <returns>Configuration object.</returns>
        Configuration GetSettings();

        /// <summary>
        /// Converts CTV data into DTS measurements and process using configuration.
        /// </summary>
        /// <param name="ctv">Binary string.</param>
        /// <returns>Pass status.</returns>
        bool ProcessCapturedData(string ctv);

        /// <summary>
        /// Decodes CTV data into DTS readings.
        /// </summary>
        /// <param name="ctv">CTV binary data (LSB-MSB).</param>
        /// <param name="values">Output values.</param>
        void GetValues(string ctv, ref Dictionary<string, List<double>> values);

        /// <summary>
        /// Evaluates DTS limits for captured data.
        /// </summary>
        /// <param name="values">DTS values by sensor.</param>
        /// <returns>Pass status.</returns>
        bool EvaluateLimits(Dictionary<string, List<double>> values);

        /// <summary>
        /// Prints data to console according to user settings.
        /// </summary>
        /// <param name="values">DTS captured data.</param>
        void PrintToDatalog(Dictionary<string, List<double>> values);
    }
}
