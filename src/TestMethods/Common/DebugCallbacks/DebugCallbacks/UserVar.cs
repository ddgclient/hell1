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
    using System.Reflection;
    using CommandLine;
    using DDG;

    /// <summary>
    /// Defines the <see cref="UserVar" />.
    /// </summary>
    public class UserVar
    {
        /// <summary>
        /// Writes a sharedstorage or GSDS token.
        /// </summary>
        /// <param name="args">String of the form "--uservar=collection.uservar --value=valueToWrite --type type".</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string WriteUserVar(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                try
                {
                    var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                    var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                    var parserResult = Parser.Default.ParseArguments<WriteUserVarOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    parserResult.WithParsed(options =>
                    {
                        var userVarType = (DDG.UserVar.ValidTypes)Enum.Parse(typeof(DDG.UserVar.ValidTypes), options.UserVarType.ToUpper());
                        var realValue = options.Value;
                        if (DDG.Gsds.IsTokenFormatAndExists(options.Value))
                        {
                            realValue = Convert.ToString(DDG.Gsds.ReadToken(options.Value));
                            console?.PrintDebug($"Argument=[{options.Value}] is a GSDS. Using Value=[{realValue}]");
                        }
                        else if (DDG.UserVar.Exists(options.Value))
                        {
                            realValue = Convert.ToString(DDG.UserVar.Read(options.Value, userVarType));
                            console?.PrintDebug($"Argument=[{options.Value}] is UserVar. Using Value=[{realValue}]");
                        }
                        else
                        {
                            console?.PrintDebug($"Argument=[{options.Value}] is a Literal value. Using Value=[{realValue}]");
                        }

                        DDG.UserVar.Write(options.FullUserVarName, userVarType, realValue);
                    }).
                        WithNotParsed(e => throw new ArgumentException($"[{instanceName}]: failed parsing arguments. {string.Join("\n", e)}"));
                }
                catch (Exception e)
                {
                    // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                    Prime.Services.ConsoleService.PrintError($"Exception in {MethodBase.GetCurrentMethod().Name} - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                    throw;
                }

                return DebugCallbacks.PASS;
            }
        }

        private class WriteUserVarOptions
        {
            [Option("uservar", Required = true, HelpText = "The Hdmt UserVariable to write. Format = Collection.Variable")]
            public string FullUserVarName { get; set; }

            [Option("value", Required = true, HelpText = "The value to write to the SharedStorage/GSDS token.")]
            public string Value { get; set; }

            [Option("type", Required = true, HelpText = "The type of UserVar to be written.")]
            public string UserVarType { get; set; }
        }
    }
}
