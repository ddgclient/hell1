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
    using DTSBase;

    /// <summary>
    /// Singleton for DTS service.
    /// </summary>
    public static class DTS
    {
        /// <summary>
        /// Gets key for DTS configuration table in shared storage.
        /// </summary>
        public static readonly string DTSConfigurationTable = "__DDG_DTSConfigurationTable__";

        /// <summary>
        /// Gets key for DTS plist clones.
        /// </summary>
        public static readonly string DTSPlistClones = "__DDG_DTSPlistClones__";

        private static readonly Lazy<IDTSFactory> DTSLazy = new Lazy<IDTSFactory>(() => new DTSFactory());

        /// <summary>
        /// Gets or sets Dlvr service implementation.
        /// </summary>
        public static IDTSFactory Service { get; set; } = DTSLazy.Value;
    }
}
