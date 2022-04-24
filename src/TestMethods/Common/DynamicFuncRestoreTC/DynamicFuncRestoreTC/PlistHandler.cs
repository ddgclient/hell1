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

namespace DynamicFuncRestoreTC
{
    using System.Collections.Generic;
    using Prime.ConsoleService;

    /// <summary>
    /// Multiple Plist handling.
    /// </summary>
    internal class PlistHandler
    {
        private readonly List<Plist> plistItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlistHandler"/> class.
        /// </summary>
        /// <param name="plistNames">plist name.</param>
        /// <param name="console">Console service.</param>
        public PlistHandler(List<string> plistNames, IConsoleService console)
        {
            this.plistItems = new List<Plist>();
            foreach (var plistName in plistNames)
            {
                this.plistItems.Add(new Plist(plistName, console));
            }
        }

        /// <summary>
        /// Update patterns to keep for each plist item with matching given id substring.
        /// </summary>
        /// <param name="id">Id to match.</param>
        /// <param name="start">Substring start position.</param>
        /// <param name="end">Substring length.</param>
        public void AddMatchingSubStringPatternsPerPlist(string id, int start, int end)
        {
            foreach (var plist in this.plistItems)
            {
                plist.AddMatchingSubStringPatterns(id, start, end);
            }
        }

        /// <summary>
        /// Update patterns to keep with matching pattern name.
        /// </summary>
        /// <param name="patternName">Id to match.</param>
        public void AddMatchingPatternsPerPlist(string patternName)
        {
            foreach (var plist in this.plistItems)
            {
                plist.AddMatchingPattern(patternName);
            }
        }

        /// <summary>
        /// Apply stored modifications to each plist item.
        /// </summary>
        /// <returns>Return value to indicate if modifications were applied.</returns>
        public bool ApplyPlistOptionsModifications()
        {
            var isThereAnyModificationApplied = false;
            foreach (var plist in this.plistItems)
            {
                isThereAnyModificationApplied |= plist.EnablePatterns();
            }

            return isThereAnyModificationApplied;
        }

        /// <summary>
        /// Enable all patterns.
        /// </summary>
        public void EnableAllPatterns()
        {
            foreach (var plist in this.plistItems)
            {
                plist.EnableAllPatterns();
            }
        }

        /// <summary>
        /// Disable all patterns.
        /// </summary>
        public void DisableAllPatterns()
        {
            foreach (var plist in this.plistItems)
            {
                plist.DisableAllPatterns();
            }
        }
    }
}
