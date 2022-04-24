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

namespace DynamicFuncTC
{
    /// <summary>
    /// File handling interfaces.
    /// Usage of FileService is preferred.
    /// </summary>
    public interface IFileHandler
    {
        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        /// <param name="filePath">File full path.</param>
        /// <returns>True if file exits.</returns>
        bool Exists(string filePath);

        /// <summary>
        /// Read all file contents and return it content as string.
        /// </summary>
        /// <param name="filePath">File full path.</param>
        /// <returns>File content as string.</returns>
        string ReadAll(string filePath);
    }
}
