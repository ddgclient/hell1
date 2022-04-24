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
    using CommandLine;
    using DDG;

    /// <summary>
    /// Defines the <see cref="SharedStorage" />.
    /// </summary>
    public class SharedStorage
    {
        /// <summary>
        /// Writes a sharedstorage or GSDS token.
        /// </summary>
        /// <param name="args">String of the form "--token=G.[ULI].[SDIO].token --value=valueToWrite".</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string WriteSharedStorage(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                try
                {
                    var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                    var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                    var parserResult = Parser.Default.ParseArguments<WriteGsdsOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    parserResult.WithParsed(options =>
                    {
                        var realValue = options.Value;
                        if (DDG.Gsds.IsTokenFormatAndExists(options.Value))
                        {
                            realValue = Convert.ToString(DDG.Gsds.ReadToken(options.Value));
                            console?.PrintDebug($"Argument=[{options.Value}] is a GSDS. Using Value=[{realValue}]");
                        }
                        else if (DDG.UserVar.Exists(options.Value))
                        {
                            realValue = Convert.ToString(DDG.UserVar.ReadAndGetType(options.Value, out var userVarType));
                            console?.PrintDebug($"Argument=[{options.Value}] is UserVar. Using Value=[{realValue}]");
                        }

                        DDG.Gsds.WriteToken(options.FullTokenName, realValue);
                    }).
                        WithNotParsed(e => throw new ArgumentException($"[{instanceName}]: failed parsing arguments. {string.Join("\n", e)}"));
                }
                catch (Exception e)
                {
                    // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                    Prime.Services.ConsoleService.PrintError($"Exception in WriteSharedStorage - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                    throw;
                }

                return DebugCallbacks.PASS;
            }
        }

        /// <summary>
        /// Print a sharedstorage or GSDS token.
        /// </summary>
        /// <param name="args">String of the form "--token=G.[ULI].[SDIO].token". Multiple tokens can be supplied as a comma separated string.</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string PrintSharedStorage(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                try
                {
                    var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                    var parserResult = Parser.Default.ParseArguments<PrintGsdsOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    parserResult.WithParsed(options =>
                    {
                        if (string.IsNullOrWhiteSpace(options.FullTokenName))
                        {
                            Prime.Services.SharedStorageService.DumpAllTablesToConsole();
                        }
                        else
                        {
                            foreach (var token in options.FullTokenName.Split(','))
                            {
                                if (DDG.Gsds.TokenExists(token))
                                {
                                    var value = DDG.Gsds.ReadToken(token);
                                    Prime.Services.ConsoleService.PrintDebug($"[SharedStorage] {token}={value}");
                                }
                                else
                                {
                                    Prime.Services.ConsoleService.PrintDebug($"[SharedStorage] {token}=<undefined>");
                                }
                            }
                        }
                    }).
                        WithNotParsed(e => throw new ArgumentException($"[{instanceName}]: failed parsing arguments. {string.Join("\n", e)}"));
                }
                catch (Exception e)
                {
                    // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                    Prime.Services.ConsoleService.PrintError($"Exception in PrintSharedStorage - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                    throw;
                }

                return DebugCallbacks.PASS;
            }
        }

        private class WriteGsdsOptions
        {
            [Option("token", Required = true, HelpText = "The SharedStorage/GSDS token to write, in GSDS format - G.scope.type.tokenname - where [scope = U for unit/dut, L for lot, or I for Ip], and [type = S for string, D for double, or I for integer].")]
            public string FullTokenName { get; set; }

            [Option("value", Required = true, HelpText = "The value to write to the SharedStorage/GSDS token.")]
            public string Value { get; set; }
        }

        private class PrintGsdsOptions
        {
            [Option("token", Required = false, HelpText = "The SharedStorage/GSDS token to read, in GSDS format - G.scope.type.tokenname - where [scope = U for unit/dut, L for lot, or I for Ip], and [type = S for string, D for double, or I for integer].")]
            public string FullTokenName { get; set; } = string.Empty;
        }
    }
}
