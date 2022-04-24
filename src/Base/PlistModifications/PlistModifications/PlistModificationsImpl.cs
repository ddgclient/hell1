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
    using System.Collections.Concurrent;

    /// <inheritdoc />
    internal class PlistModificationsImpl : IPlistModifications
    {
        private ConcurrentDictionary<string, PlistTree> trees;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlistModificationsImpl"/> class.
        /// </summary>
        public PlistModificationsImpl()
        {
            this.trees = new ConcurrentDictionary<string, PlistTree>();
        }

        /// <inheritdoc />
        public PlistTree BuildPlistTree(string plist)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                if (!this.trees.ContainsKey(plist))
                {
                    var newTree = new PlistTree(plist);
                    this.trees[plist] = newTree;
                }

                return this.trees[plist];
            }
        }

        /// <inheritdoc />
        public void CleanTree(string plist)
        {
            if (string.IsNullOrEmpty(plist))
            {
                this.trees.Clear();
            }
            else if (this.trees.ContainsKey(plist))
            {
                this.trees.TryRemove(plist, out _);
            }
        }

        /// <inheritdoc />
        public void RestoreTree(string plist)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                if (string.IsNullOrEmpty(plist))
                {
                    foreach (var tree in this.trees)
                    {
                        tree.Value.RestorePlist();
                        tree.Value.PlistObject.Resolve();
                    }
                }
                else if (this.trees.ContainsKey(plist))
                {
                    this.trees[plist].RestorePlist();
                    this.trees[plist].PlistObject.Resolve();
                }
            }
        }
    }
}
