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

    /// <summary>
    /// Defines the <see cref="PatConfig" />.
    /// </summary>
    public class PatConfig
    {
        /// <summary>
        /// Executes a list of PatConfigSetPoints.
        /// </summary>
        /// <param name="args">Comma-separated list of PatConfigSetPoints in the format Module:Group:SetPoint.</param>
        public static void ExecutePatConfigSetPoint(string args)
        {
            try
            {
                using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
                {
                    var tuples = args.Split(',').ToList();
                    foreach (var tuple in tuples)
                    {
                        var tokens = tuple.Split(':').ToList();
                        if (tokens.Count < 3)
                        {
                            throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Name} Invalid PatConfig tokens=[{tuple}]. Expected format Module:Group:SetPoint.");
                        }

                        if (tokens.Count == 3)
                        {
                            var patLists = Prime.Services.TestProgramService.GetCurrentPatternLists();
                            foreach (var handle in patLists.Select(patlist => Prime.Services.PatConfigService.GetSetPointHandle(tokens[0], tokens[1], patlist)))
                            {
                                handle.ApplySetPoint(tokens[2]);
                            }
                        }
                        else if (tokens[3].ToLower() == "global")
                        {
                            var handle = Prime.Services.PatConfigService.GetSetPointHandle(tokens[0], tokens[1]);
                            handle.ApplySetPoint(tokens[2]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                Prime.Services.ConsoleService.PrintError($"Exception {MethodBase.GetCurrentMethod()?.Name} - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Executes a list of ExecutePatConfig.
        /// </summary>
        /// <param name="args">Comma-separated list of PatConfigSetPoints in the format Configuration:Data.</param>
        public static void ExecutePatConfig(string args)
        {
            try
            {
                using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
                {
                    var patLists = Prime.Services.TestProgramService.GetCurrentPatternLists();
                    foreach (var handles in patLists.Select(patlist => DDG.PatternModifications.Service.GetPatternConfigHandles(args, patlist)))
                    {
                        DDG.PatternModifications.Service.ApplyPatternConfigHandles(handles);
                    }
                }
            }
            catch (Exception e)
            {
                // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                Prime.Services.ConsoleService.PrintError($"Exception {MethodBase.GetCurrentMethod()?.Name} - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Executes a different PatConfigSetPoint for each bit in a binary string.
        /// BitVectorPatConfigSetPoint --bitvector token --setpoint module:group:setpoint [--msb_first] [--value_map map] [--index_width size].
        /// </summary>
        /// <param name="args">Arguments.</param>
        public static void BitVectorPatConfigSetPoint(string args)
        {
            try
            {
                using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
                {
                    var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                    var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                    var parserResult =
                        Parser.Default.ParseArguments<BitVectorPatConfigSetPointOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    parserResult.WithParsed(options =>
                    {
                        // Create the BitValue to Text map.
                        Dictionary<string, string> valueMap = DecodeValueMap(options.ValueMapString);

                        // Decode the bitvector token into a binary string.
                        string bitvectorValue = string.Empty;
                        foreach (var token in options.BitVectorToken.Split(','))
                        {
                            bitvectorValue += DDG.Gsds.IsTokenFormatAndExists(token)
                                ? DDG.Gsds.ReadToken(token)
                                : token;
                        }

                        console?.PrintDebug($"BitVectorPatConfigSetPoint: Translated bitvector to [{bitvectorValue}].");
                        for (var i = 0; i < bitvectorValue.Length; i++)
                        {
                            var bitIndex = options.MsbFirst ? bitvectorValue.Length - i - 1 : i;
                            var bitIndexPadded = bitIndex.ToString().PadLeft(options.IndexWidth, '0');

                            var bitValueRaw = bitvectorValue.Substring(i, 1);
                            var bitValueMapped =
                                valueMap.ContainsKey(bitValueRaw) ? valueMap[bitValueRaw] : bitValueRaw;

                            var fullSetPointAfterReplacement = options.FullSetPointName
                                .Replace("%INDEX%", bitIndexPadded).Replace("%VALUE%", bitValueMapped);
                            var setPointTokens = fullSetPointAfterReplacement.Split(':').ToList();
                            if (setPointTokens.Count == 2)
                            {
                                console?.PrintDebug($"BitVectorPatConfigSetPoint: Module=[{setPointTokens[0]}] Group=[{setPointTokens[1]}] calling ApplySetPointDefault().");
                                var handle = Prime.Services.PatConfigService.GetSetPointHandle(setPointTokens[0], setPointTokens[1]);
                                handle.ApplySetPointDefault();
                            }
                            else if (setPointTokens.Count == 3)
                            {
                                console?.PrintDebug($"BitVectorPatConfigSetPoint: Module=[{setPointTokens[0]}] Group=[{setPointTokens[1]}] calling ApplySetPoint({setPointTokens[2]}).");
                                var handle = Prime.Services.PatConfigService.GetSetPointHandle(setPointTokens[0], setPointTokens[1]);
                                handle.ApplySetPoint(setPointTokens[2]);
                            }
                            else
                            {
                                throw new Prime.Base.Exceptions.TestMethodException(
                                    $"Error parsing PatConfigSetpoint=[{fullSetPointAfterReplacement}]. Expecting Module:Group:Setpoint or Module:Group format.");
                            }
                        }
                    }).WithNotParsed(e =>
                        throw new ArgumentException(
                            $"{instanceName}.BitVectorPatConfigSetPoint: failed parsing arguments. {string.Join("\n", e)}"));
                }
            }
            catch (Exception e)
            {
                // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                Prime.Services.ConsoleService.PrintError($"Exception in BitVectorPatConfigSetPoint - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        private static Dictionary<string, string> DecodeValueMap(string valueMapString)
        {
            var valueMap = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(valueMapString))
            {
                foreach (var pair in valueMapString.Split(','))
                {
                    var tokens = pair.Split(':');
                    if (tokens.Length != 2)
                    {
                        throw new Prime.Base.Exceptions.TestMethodException($"Error parsing ValueMap=[{pair}]. Expecting a comma-separated list of value:replacement tokens.");
                    }

                    valueMap[tokens[0]] = tokens[1];
                }
            }

            return valueMap;
        }

        private class BitVectorPatConfigSetPointOptions
        {
            [Option("bitvector", Required = true, HelpText = "Comma-separated list of GSDS tokens or binary strings.")]
            public string BitVectorToken { get; set; }

            [Option("setpoint", Required = true, HelpText = "PatConfigSetPoint name of the form MODULE:GROUP:SETPOINT. %INDEX% and %VALUE% will automatically be replaced by the bit index and value for each bit in the bitvector.")]
            public string FullSetPointName { get; set; }

            [Option("value_map", Required = false, HelpText = "Sets the replacement text for the %VALUE% string. Should be '0:value_for_0,1:value_for_1'.")]
            public string ValueMapString { get; set; } = string.Empty;

            [Option("msb_first", Required = false, HelpText = "If set the first (left-most) bit is considered the MSB. Otherwise the first bit is considered to be bit 0.")]
            public bool MsbFirst { get; set; } = false;

            [Option("index_width", Required = false, HelpText = "Sets the minimum text size (left-padded with zeros) for the %INDEX% replacement.")]
            public int IndexWidth { get; set; } = 0;
        }
    }
}
