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
    using System.Collections.Generic;
    using Prime.ConsoleService;
    using Prime.PlistService;

    /// <summary>
    /// Single Plist handling.
    /// </summary>
    internal class Plist
    {
        private readonly HashSet<string> patterns;
        private readonly IPlistObject plistObject;
        private readonly string plistName;
        private readonly IConsoleService console;
        private readonly HashSet<string> patternsToKeep;

        /// <summary>
        /// Initializes a new instance of the <see cref="Plist"/> class.
        /// </summary>
        /// <param name="plistName">plist name.</param>
        /// <param name="console">Console service.</param>
        public Plist(string plistName, IConsoleService console)
        {
            this.plistName = plistName;
            this.plistObject = Prime.Services.PlistService.GetPlistObject(plistName);
            this.patterns = this.plistObject.GetUniquePatternNames();
            this.patternsToKeep = new HashSet<string>();
            this.console = console;
            this.console?.PrintDebug($"\tPlistName=[{plistName}] Patterns:\n\t\t{string.Join("\n\t\t", this.patterns)}");
            DynamicFuncTC.AddToRestoreList("PLIST_NAMES_TO_RESTORE", this.plistName);
        }

        /// <summary>
        /// Update patterns to keep with matching given id substring.
        /// </summary>
        /// <param name="id">Id to match.</param>
        /// <param name="start">Substring start position.</param>
        /// <param name="end">Substring length.</param>
        public void AddMatchingSubStringPatterns(string id, int start, int end)
        {
            foreach (var patternName in this.patterns)
            {
                if (patternName.Substring(start, end).Equals(id))
                {
                    this.patternsToKeep.Add(patternName);
                    this.console?.PrintDebug($"\tTarget pattern to keep=[{patternName}] TestId=[{start}|{end}|{id}] Plist=[{this.plistName}]");
                }
            }
        }

        /// <summary>
        /// Update patterns to keep with matching pattern name.
        /// </summary>
        /// <param name="patternName">Id to match.</param>
        public void AddMatchingPattern(string patternName)
        {
            if (this.patterns.Contains(patternName))
            {
                this.patternsToKeep.Add(patternName);
                this.console?.PrintDebug($"\tTarget pattern to keep=[{patternName}]");
            }
        }

        /// <summary>
        /// Wrapper to service methods used for plist modifications.
        /// </summary>
        /// <returns>Return value to indicate if modifications were applied.</returns>
        public bool EnablePatterns()
        {
            if (this.patternsToKeep.Count == 0)
            {
                return false;
            }

            this.console?.PrintDebug($"\tModifying plist=[{this.plistName}]");
            this.plistObject.EnableGivenPatternsDisableRest(this.patternsToKeep);
            this.patternsToKeep.Clear();
            return true;
        }

        /// <summary>
        /// Clear patterns to keep.
        /// </summary>
        public void ClearPatternsToKeep()
        {
            this.console?.PrintDebug($"\tClearing plist=[{this.plistName}]");
            this.patternsToKeep.Clear();
        }

        /// <summary>
        /// Enable all patterns.
        /// </summary>
        public void EnableAllPatterns()
        {
            this.console?.PrintDebug($"\tEnabling all patterns in plist=[{this.plistName}]");
            this.plistObject.EnableAllPatterns();
        }
    }
}
