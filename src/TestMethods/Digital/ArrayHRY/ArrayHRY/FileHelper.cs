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

namespace ArrayHRY
{
    using System;

    /// <summary>
    /// Defines the <see cref="FileHelper" />.
    /// This gives a way to cache local files and share them between test instances.
    /// Really only useful for the XML Schema, no need to create 1 copy per instance for those.
    /// Create an Empty FileHelper() for each file. Then call HasFileChanged() with the remote file
    /// path to create a new local version if needed.
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileHelper"/> class.
        /// </summary>
        public FileHelper()
        {
            this.RemoteFileName = string.Empty;
            this.LocalFileName = string.Empty;
            this.LocalFileAccessTime = new DateTime(1);
        }

        private string RemoteFileName { get; set; }

        private string LocalFileName { get; set; }

        private DateTime LocalFileAccessTime { get; set; }

        /// <summary>
        /// Checks if the remote file has changed. Uses shared storage as a cache so only one local copy of the file
        /// will exist per test program.
        /// </summary>
        /// <param name="remoteFile">Remote path to the file. Will be run through Prime FileService.GetNormalizedPath().</param>
        /// <param name="localFile">(output) Local path to the file.</param>
        /// <returns>True if the local file has changed since the last time this was called.</returns>
        public bool HasFileChanged(string remoteFile, out string localFile)
        {
            var remoteFileNormalizedPath = Prime.Services.FileService.GetNormalizedPath(remoteFile);

            // see if there is a cached version of this file.
            var cachedCopyIsUpToDate = this.GetCachedLocalFile(remoteFileNormalizedPath, out localFile);

            // get a new copy (and update the cache) if no cached copy exists or it has changed.
            if (!cachedCopyIsUpToDate)
            {
                localFile = Prime.Services.FileService.GetFile(remoteFileNormalizedPath);
                if (string.IsNullOrEmpty(localFile))
                {
                    throw new System.IO.FileNotFoundException($"GetFile({remoteFileNormalizedPath}) returned an empty path.", remoteFile);
                }

                Prime.Services.SharedStorageService.InsertRowAtTable(this.GetCacheKey(remoteFileNormalizedPath), localFile, Prime.SharedStorageService.Context.LOT);
                Prime.Services.ConsoleService.PrintDebug($"   Using new local file=[{localFile}].");
            }

            // if the remote or local copy has changed
            var localFileAccessTime = Prime.Services.FileService.GetLastModificationTime(localFile);
            var fileHasChanged = remoteFile != this.RemoteFileName || localFile != this.LocalFileName || this.LocalFileAccessTime != localFileAccessTime;

            // update the object to with all the latest info.
            this.RemoteFileName = remoteFile;
            this.LocalFileName = localFile;
            this.LocalFileAccessTime = localFileAccessTime;

            return fileHasChanged;
        }

        private string GetCacheKey(string remoteFileNormalizedPath)
        {
            return $"__ARRAYHRY_FILECACHE__.{remoteFileNormalizedPath}";
        }

        private bool GetCachedLocalFile(string remoteFileNormalizedPath, out string localFile)
        {
            var cacheKey = this.GetCacheKey(remoteFileNormalizedPath);
            if (Prime.Services.SharedStorageService.KeyExistsInStringTable(cacheKey, Prime.SharedStorageService.Context.LOT))
            {
                var cachedLocalFile = Prime.Services.SharedStorageService.GetStringRowFromTable(cacheKey, Prime.SharedStorageService.Context.LOT);
                /* this gives a way to force a reload of the file, just write the shared storage to this value in the test program. */
                if (cachedLocalFile.ToUpper() != "RELOAD")
                {
                    var remoteFileTimeStamp = Prime.Services.FileService.GetLastModificationTime(remoteFileNormalizedPath);
                    var cachedLocalFileTimeStamp = Prime.Services.FileService.GetLastModificationTime(cachedLocalFile);

                    if (remoteFileTimeStamp == cachedLocalFileTimeStamp)
                    {
                        Prime.Services.ConsoleService.PrintDebug($"   Using cached local file=[{cachedLocalFile}].");
                        localFile = cachedLocalFile;
                        return true;
                    }
                }
            }

            localFile = string.Empty;
            return false;
        }
    }
}
