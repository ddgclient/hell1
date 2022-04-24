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
    /// <summary>
    /// Defines the <see cref="FileUtilities" />.
    /// </summary>
    public static class FileUtilities
    {
        /// <summary>
        /// Wrapper function for Prime GetFile which actually prints out the name of the file if it isn't found
        /// or GetFile fails.
        /// </summary>
        /// <param name="fileName">Filename to create a local copy of.</param>
        /// <returns>Local file name.</returns>
        /// <exception cref="System.IO.FileNotFoundException"> if file is not found.</exception>
        public static string GetFile(string fileName)
        {
            string localFile = string.Empty;
            if (!Prime.Services.FileService.FileExists(fileName))
            {
                throw new System.IO.FileNotFoundException($"File=[{fileName}] is not found.");
            }

            try
            {
                localFile = Prime.Services.FileService.GetFile(fileName);
            }
            catch (System.IO.FileNotFoundException)
            {
                Prime.Services.ConsoleService.PrintError($"Call to Prime.Services.FileService.GetFile({fileName}) threw an IO exception.");
                throw;
            }
            catch (Prime.Base.Exceptions.BaseException)
            {
                Prime.Services.ConsoleService.PrintError($"Call to Prime.Services.FileService.GetFile({fileName}) threw a Prime exception.");
                throw;
            }

            if (string.IsNullOrEmpty(localFile))
            {
                throw new System.IO.FileNotFoundException($"Prime GetFile({fileName}) returned empty string, file probably doesn't exist.");
            }

            return localFile;
        }
    }
}
