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
    using Prime.SharedStorageService;

    /// <summary>
    /// Defines the <see cref="Dff" />.
    /// </summary>
    public class Dff
    {
        /// <summary>
        /// Sets the current DieID for DFF.
        /// </summary>
        /// <param name="dieId">die ID to set.</param>
        /// <returns>1 on success.</returns>
        public static string SetCurrentDieId(string dieId)
        {
            Prime.Services.DffService.SetCurrentDieId(dieId);
            return DebugCallbacks.PASS;
        }

        /// <summary>
        /// Writes a DFF token.
        /// </summary>
        /// <param name="args">String of the form "--token=token --value=valueToWrite --targetdie=dieID".</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string WriteDff(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                try
                {
                    var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                    var parserResult = Parser.Default.ParseArguments<WriteDffOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    parserResult.WithParsed(options =>
                    {
                        if (string.IsNullOrEmpty(options.TargetDie))
                        {
                            Prime.Services.DffService.SetDff(options.Token, options.Value);
                        }
                        else
                        {
                            Prime.Services.DffService.SetDff(options.Token, options.Value, options.TargetDie);
                        }
                    }).
                        WithNotParsed(e => throw new ArgumentException($"[{instanceName}]: failed parsing arguments. {string.Join("\n", e)}"));
                }
                catch (Exception e)
                {
                    // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                    Prime.Services.ConsoleService.PrintError($"Exception in WriteDff - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                    throw;
                }

                return DebugCallbacks.PASS;
            }
        }

        /// <summary>
        /// Prints a DFF token.
        /// </summary>
        /// <param name="args">String of the form "--tokens=token1,token2 --optype=optype --targetdie=dieId".</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string PrintDff(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                try
                {
                    var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                    var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                    var parserResult = Parser.Default.ParseArguments<PrintDffOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    parserResult.WithParsed(options =>
                        {
                            if (string.IsNullOrWhiteSpace(options.Tokens))
                            {
                                return;
                            }

                            foreach (var token in options.Tokens.Split(','))
                            {
                                string value;
                                var opType = string.IsNullOrEmpty(options.OpType) ? string.Empty : options.OpType;
                                var targetDie = string.IsNullOrEmpty(options.TargetDie) ? string.Empty : options.TargetDie;
                                if (string.IsNullOrEmpty(targetDie))
                                {
                                    value = string.IsNullOrEmpty(opType) ? Prime.Services.DffService.GetDff(token) : Prime.Services.DffService.GetDffByOpType(token, opType);
                                }
                                else
                                {
                                    value = string.IsNullOrEmpty(opType) ? Prime.Services.DffService.GetDffByDieId(token, targetDie) : Prime.Services.DffService.GetDff(token, opType, targetDie);
                                }

                                console?.PrintDebug($"token=[{token}] optype=[{opType}] targetdie=[{targetDie}] value=[{value}].\n");
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

        /// <summary>
        /// Mirror a DFF token to into ShareStorage token using MD_token_optype_targetdie as key.
        /// </summary>
        /// <param name="args">String of the form "--tokens=token1,token2 --optype=optype --targetdie=dieId".</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string MirrorDff(string args)
        {
            try
            {
                var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                var parserResult = Parser.Default.ParseArguments<PrintDffOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                parserResult.WithParsed(options =>
                {
                    if (!string.IsNullOrWhiteSpace(options.Tokens))
                    {
                        foreach (var token in options.Tokens.Split(','))
                        {
                            string value;
                            var opType = string.IsNullOrEmpty(options.OpType) ? string.Empty : options.OpType;
                            var targetDie = string.IsNullOrEmpty(options.TargetDie) ? string.Empty : options.TargetDie;
                            if (string.IsNullOrEmpty(targetDie))
                            {
                                value = string.IsNullOrEmpty(opType) ? Prime.Services.DffService.GetDff(token) : Prime.Services.DffService.GetDffByOpType(token, opType);
                            }
                            else
                            {
                                value = string.IsNullOrEmpty(opType) ? Prime.Services.DffService.GetDffByDieId(token, targetDie) : Prime.Services.DffService.GetDff(token, opType, targetDie);
                            }

                            Prime.Services.SharedStorageService.InsertRowAtTable($"MD_{token}_{opType}_{targetDie}", value, Context.DUT);
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

        private class WriteDffOptions
        {
            [Option("token", Required = true, HelpText = "The DFF token to write.")]
            public string Token { get; set; }

            [Option("value", Required = true, HelpText = "The value to write to the DFF token.")]
            public string Value { get; set; }

            [Option("targetdie", Required = false, HelpText = "Optional target die Id.")]
            public string TargetDie { get; set; }
        }

        private class PrintDffOptions
        {
            [Option("tokens", Required = false, HelpText = "The DFF tokens to write.")]
            public string Tokens { get; set; }

            [Option("optype", Required = false, HelpText = "Optional operation type.")]
            public string OpType { get; set; }

            [Option("targetdie", Required = false, HelpText = "Optional target die Id.")]
            public string TargetDie { get; set; }
        }
    }
}
