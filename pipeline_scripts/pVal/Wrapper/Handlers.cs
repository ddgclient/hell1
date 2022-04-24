// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// INTEL CONFIDENTIAL
// Copyright (2019) (2020) Intel Corporation
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

namespace Wrapper
{
    /// <summary>
    /// Class that instantiates classes containing common methods for use across PVAL.
    /// </summary>
    /// <remarks> Main purpose is to be used as a wrapper around hard to test System methods (allowing for use of Mocks). </remarks>
    public static class Handlers
    {
        static Handlers()
        {
            EnvironmentVariableHandler = new EnvironmentVariableHandler();
            DirectoryHandler = new DirectoryHandler();
            InputHandler = new InputHandler();
            LoggerHandler = new LoggerHandler();
        }

        /// <summary>
        /// Handles all methods related to Environment variables.
        /// </summary>
        public static IEnvironmentVariableHandler EnvironmentVariableHandler { get; set; }

        /// <summary>
        /// Handles all methods related to parsing/retrieving directories.
        /// </summary>
        public static IDirectoryHandler DirectoryHandler { get; set; }

        /// <summary>
        /// Handles all methods related to user input.
        /// </summary>
        public static IInputHandler InputHandler { get; set; }

        /// <summary>
        /// Handles all methods related to logging.
        /// </summary>
        public static ILoggerHandler LoggerHandler { get; set; }
    }
}
