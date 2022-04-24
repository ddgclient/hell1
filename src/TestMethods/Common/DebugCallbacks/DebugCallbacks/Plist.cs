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

namespace DebugCallbacks
{
    using System;
    using System.Linq;
    using DDG;

    /// <summary>
    /// Defines the <see cref="Plist" />.
    /// </summary>
    public class Plist
    {
        /// <summary>
        /// Restore plist modifications.
        /// </summary>
        /// <param name="args">List of patlists.</param>
        public static void RestorePlist(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                try
                {
                    var patlists = string.IsNullOrEmpty(args) ? Prime.Services.TestProgramService.GetCurrentPatternLists() : args.Split(',').ToList();
                    foreach (var patlist in patlists)
                    {
                        DDG.PlistModifications.Service.RestoreTree(patlist);
                    }
                }
                catch (Exception e)
                {
                    // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                    Prime.Services.ConsoleService.PrintError($"Exception in ExecuteInstance - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Cleans plist modifications.
        /// </summary>
        /// <param name="args">List of patlists.</param>
        public static void CleanPlist(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                try
                {
                    var patlists = string.IsNullOrEmpty(args) ? Prime.Services.TestProgramService.GetCurrentPatternLists() : args.Split(',').ToList();
                    foreach (var patlist in patlists)
                    {
                        DDG.PlistModifications.Service.CleanTree(patlist);
                    }
                }
                catch (Exception e)
                {
                    // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                    Prime.Services.ConsoleService.PrintError($"Exception in ExecuteInstance - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                    throw;
                }
            }
        }
    }
}
