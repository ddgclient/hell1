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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CommandLine;
    using DDG;
    using Prime.DatalogService.DatalogSpec;

    /// <summary>
    /// Defines the <see cref="Datalog" />.
    /// </summary>
    public class Datalog
    {
        /// <summary>
        /// Defines the ValidTypes.
        /// </summary>
        private enum ValidTypes
        {
            MRSLT,
            RAWBINARY_MSBF,
            STRGVAL,
        }

        /// <summary>
        /// Writes a sharedstorage or GSDS token.
        /// </summary>
        /// <param name="args">String of the form "--token=G.[ULI].[SDIO].token --value=valueToWrite".</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string PrintToItuff(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                try
                {
                    var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                    var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                    var parserResult = Parser.Default.ParseArguments<PrintToItuffOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    parserResult.WithParsed(options =>
                    {
                        var datalogType = (ValidTypes)Enum.Parse(typeof(ValidTypes), options.Type.ToUpper());
                        var postfix = string.Join(string.Empty, ResolveListOfTokens(options.PostTname.Split(',').ToList(), console));
                        var allValues = ResolveListOfTokens(options.Data.Split(',').ToList(), console);

                        IItuffFormat writer;
                        switch (datalogType)
                        {
                            case ValidTypes.MRSLT:
                                writer = Prime.Services.DatalogService.GetItuffMrsltWriter();
                                (writer as IMrsltFormat).SetData(allValues.Select(o => Convert.ToDouble(o)).Sum());
                                break;
                            case ValidTypes.RAWBINARY_MSBF:
                                writer = Prime.Services.DatalogService.GetItuffRawbinaryWriter();
                                (writer as IRawbinaryFormat).SetData(string.Join(string.Empty, allValues), true);
                                break;
                            default: /* ValidTypes.STRGVAL */
                                writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
                                (writer as IStrgvalFormat).SetData(string.Join(string.Empty, allValues));
                                break;
                        }

                        ExecuteDatalog(writer, postfix);
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

        /// <summary>
        /// Set alternative instance name for the upcoming tname datalogs.
        /// </summary>
        /// <param name="args">Instance name.</param>
        public static void SetAltInstanceName(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                args = TOSUserSDK.ScratchPad.Service.GetScratchPadPair("FunctionArguments");
            }

            Prime.Services.DatalogService.SetAltInstanceName(args);
        }

        private static void ExecuteDatalog(IItuffFormat writer, string postfix)
        {
            writer.SetTnamePostfix(postfix);
            Prime.Services.DatalogService.WriteToItuff(writer);
        }

        private static List<string> ResolveListOfTokens(List<string> tokens, Prime.ConsoleService.IConsoleService console)
        {
            var retval = new List<string>();
            foreach (var token in tokens)
            {
                var realValue = token;
                if (DDG.Gsds.IsTokenFormatAndExists(token))
                {
                    realValue = Convert.ToString(DDG.Gsds.ReadToken(token));
                    console?.PrintDebug($"Argument=[{token}] is a GSDS. Using Value=[{realValue}]");
                }
                else if (DDG.UserVar.Exists(token))
                {
                    realValue = Convert.ToString(DDG.UserVar.ReadAndGetType(token, out var userVarType));
                    console?.PrintDebug($"Argument=[{token}] is UserVar. Using Value=[{realValue}]");
                }
                else
                {
                    console?.PrintDebug($"Argument=[{token}] is a Literal value. Using Value=[{realValue}]");
                }

                retval.Add(realValue);
            }

            return retval;
        }

        /// <summary>
        /// Defines the <see cref="PrintToItuffOptions" />.
        /// </summary>
        private class PrintToItuffOptions
        {
            /// <summary>
            /// Gets or sets the FullUserVarName.
            /// </summary>
            [Option("body_type", Required = true, HelpText = "The Ituff format type. One of mrslt, strgval or rawbinary_msbF")]
            public string Type { get; set; }

            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            [Option("tname_suf", Required = false, HelpText = "The suffix to be evaluated and printed after the instance name.")]
            public string PostTname { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the UserVarType.
            /// </summary>
            [Option("body_data", Required = true, HelpText = "The data to be evaluated and printed to ituff.")]
            public string Data { get; set; }
        }
    }
}
