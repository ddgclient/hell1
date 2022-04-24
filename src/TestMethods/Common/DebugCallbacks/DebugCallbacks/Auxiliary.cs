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
    using System.Reflection;
    using CommandLine;
    using DDG;

    /// <summary>
    /// Defines the <see cref="EvaluateExpression" />.
    /// </summary>
    public class Auxiliary
    {
        private static readonly Dictionary<ValueType, DDG.UserVar.ValidTypes> UserVarTypeMap = new Dictionary<ValueType, DDG.UserVar.ValidTypes>
        {
            { ValueType.INTEGER, DDG.UserVar.ValidTypes.INTEGER },
            { ValueType.DOUBLE, DDG.UserVar.ValidTypes.DOUBLE },
            { ValueType.STRING, DDG.UserVar.ValidTypes.STRING },
        };

        private enum ValueType
        {
            INTEGER,
            DOUBLE,
            STRING,
        }

        private enum StorageType
        {
            USERVAR,
            GSDS,
            DFF,
        }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="args">String expression".</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string EvaluateExpression(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                string retval = DebugCallbacks.FAIL;
                try
                {
                    var parser = new Parser(with =>
                    {
                        with.EnableDashDash = true;
                        with.IgnoreUnknownArguments = false;
                    });

                    var parserResult = parser.ParseArguments<ExpressionOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    parserResult.WithParsed(options =>
                    {
                        var expression = string.Join(" ", options.Expression);
                        console?.PrintDebug($"Expression={expression}");
                        console?.PrintDebug($"ResultVar={options.ResultVar}");
                        console?.PrintDebug($"DataType={options.DataType}");
                        console?.PrintDebug($"StorageType={options.StorageType}");

                        var storageType = (StorageType)Enum.Parse(typeof(StorageType), options.StorageType.ToUpper());
                        var valueType = (ValueType)Enum.Parse(typeof(ValueType), options.DataType.ToUpper());

                        var expressionObj = new DDG.HdmtExpression(expression);
                        var result = expressionObj.Evaluate();
                        retval = Convert.ToString(result);

                        switch (storageType)
                        {
                            case StorageType.USERVAR:
                                DDG.UserVar.Write(options.ResultVar, UserVarTypeMap[valueType], result);
                                break;
                            case StorageType.DFF:
                                Prime.Services.DffService.SetDff(options.ResultVar, Convert.ToString(result));
                                break;
                            case StorageType.GSDS:
                            default:
                                DDG.Gsds.WriteToken(options.ResultVar, result);
                                break;
                        }
                    }).
                        WithNotParsed(e => throw new ArgumentException($"Failed parsing arguments. {string.Join("\n", e)}"));
                }
                catch (Exception e)
                {
                    // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                    Prime.Services.ConsoleService.PrintError($"Exception in {MethodBase.GetCurrentMethod().Name} - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                    throw;
                }

                return retval;
            }
        }

        private class ExpressionOptions
        {
            [Option("result", Required = true, HelpText = "The uservar/gsds/dff token to write.")]
            public string ResultVar { get; set; }

            [Option("expression", Required = true, HelpText = "Expression to evaluate.")]
            public IEnumerable<string> Expression { get; set; }

            [Option("datatype", Required = true, HelpText = "The data type (integer, double, string) of the result token.")]
            public string DataType { get; set; }

            [Option("storagetype", Required = true, HelpText = "The type of token to write (uservar, gsds, dff).")]
            public string StorageType { get; set; }
        }
    }
}
