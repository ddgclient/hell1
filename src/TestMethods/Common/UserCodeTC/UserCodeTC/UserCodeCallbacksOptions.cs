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

namespace UserCodeCallbacks
{
    using CommandLine;

    /// <summary>
    /// Defines the <see cref="T:DDG.VoltageConverterOptions" />.
    /// </summary>
    public class UserCodeCallbacksOptions
    {
        /// <summary>
        /// Gets or sets input file.
        /// </summary>
        [Option("file", HelpText = "Source code input file.", Required = true)]
        public string File { get; set; }

        /// <summary>
        /// Gets or sets code namespace and class.
        /// </summary>
        [Option("namespace.class", HelpText = "Code namespace.class", Required = true)]
        public string NamespaceClass { get; set; }

        /// <summary>
        /// Gets or sets method to invoke.
        /// </summary>
        [Option("method", HelpText = "Code method to invoke.", Required = true)]
        public string Method { get; set; }
    }
}