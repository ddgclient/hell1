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
    using System.Threading;
    using CommandLine;
    using DDG;
    using Newtonsoft.Json;
    using Prime.TestConditionService;

    /// <summary>
    /// Defines the <see cref="TestConditions" />.
    /// </summary>
    public class TestConditions
    {
        /// <summary>
        /// Enables SmartTC.
        /// </summary>
        /// <param name="args">Dummy.</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string EnableSmartTC(string args)
        {
            Prime.Services.TestConditionService.EnableSmartTC();
            return DebugCallbacks.PASS;
        }

        /// <summary>
        /// Disables SmartTC.
        /// </summary>
        /// <param name="args">Dummy.</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string DisableSmartTC(string args)
        {
            Prime.Services.TestConditionService.DisableSmartTC();
            return DebugCallbacks.PASS;
        }

        /// <summary>
        /// Applies EndSequence.
        /// </summary>
        /// <param name="args">Dummy.</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string ApplyEndSequence(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                Prime.Services.TestConditionService.ApplyEndSequence();
                return DebugCallbacks.PASS;
            }
        }

        /// <summary>
        /// Sets PowerUpTCName.
        /// </summary>
        /// <param name="powerUpTCName">Test Condition Name.</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string SetPowerUpTCName(string powerUpTCName)
        {
            Prime.Services.TestConditionService.SetPowerUpTCName(powerUpTCName);
            return DebugCallbacks.PASS;
        }

        /// <summary>
        /// Flushes all SmartTCCategories.
        /// </summary>
        /// <param name="args">Dummy.</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string FlushAllSmartTCCategories(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                Prime.Services.TestConditionService.FlushAllSmartTCCategories();
                return DebugCallbacks.PASS;
            }
        }

        /// <summary>
        /// Flushes SmartTCCategory.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <returns>1 for pass, 0 for fail.</returns>
        public static string FlushSmartTCCategory(string category)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                try
                {
                    if (!Enum.TryParse<SmartTCCategoryType>(category, out var result))
                    {
                        return DebugCallbacks.FAIL;
                    }

                    Prime.Services.TestConditionService.FlushSmartTCCategory(result);
                    return DebugCallbacks.PASS;
                }
                catch (Exception e)
                {
                    Prime.Services.ConsoleService.PrintError($"Failed FlushSmartTCCategory. {e.Message}");
                    return DebugCallbacks.FAIL;
                }
            }
        }

        /// <summary>
        /// Validates trigger map.
        /// </summary>
        /// <param name="args">TriggerMap,Plist.</param>
        public static void ValidatePatternTriggerMap(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                var tokens = args.Split(',').Select(it => it.Trim()).ToList();
                TOSUserSDK.TestConditions.Service.ValidatePatternTriggerMap(tokens[0], tokens[1]);
            }
        }

        /// <summary>
        /// Applies trigger map.
        /// </summary>
        /// <param name="args">TriggerMap,Plist.</param>
        public static void ApplyPatternTriggerMap(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                var tokens = args.Split(',').Select(it => it.Trim()).ToList();
                TOSUserSDK.TestConditions.Service.ApplyPatternTriggerMap(tokens[0], tokens[1]);
            }
        }

        /// <summary>
        /// SetPinAttributes Callback.
        /// </summary>
        /// <param name="args">Command-line format: --prepause=1 --postpause=1 --settings=Pin1:Attribute:Value,...,PinN:Attribute:Value.</param>
        public static void SetPinAttributes(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                try
                {
                    var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                    var parserResult = Parser.Default.ParseArguments<SetPinAttributesOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    parserResult.WithParsed(options =>
                        {
                            var pinAttributes = new Dictionary<string, Dictionary<string, string>>();
                            foreach (var value in options.Settings)
                            {
                                var tokens = value.Split(':');
                                if (tokens.Length != 3)
                                {
                                    throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Name} invalid settings value={value}. Expecting 3 tokens separated by ':' Pin:Attribute:Value");
                                }

                                if (pinAttributes.ContainsKey(tokens[0]))
                                {
                                    pinAttributes[tokens[0]][tokens[1]] = tokens[2];
                                }
                                else
                                {
                                    var pin = new Dictionary<string, string> { { tokens[1], tokens[2] } };
                                    pinAttributes.Add(tokens[0], pin);
                                }
                            }

                            if (options.PrePause > 0 && pinAttributes.Count > 0)
                            {
                                console?.PrintDebug($"Applying {nameof(options.PrePause)}={options.PrePause}");
                                Thread.Sleep((int)options.PrePause);
                            }

                            foreach (var pin in pinAttributes)
                            {
                                console?.PrintDebug($"Applying Pin Attributes for Pin={pin.Key} Values={JsonConvert.SerializeObject(pin.Value)}.");
                                Prime.Services.PinService.SetPinAttributeValues(pin.Key, pin.Value);
                            }

                            if (options.PostPause > 0 && pinAttributes.Count > 0)
                            {
                                console?.PrintDebug($"Applying {nameof(options.PostPause)}={options.PostPause}");
                                Thread.Sleep((int)options.PostPause);
                            }
                        })
                        .WithNotParsed(e =>
                            throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Name} Invalid args=[{args}]."));
                }
                catch (Exception e)
                {
                    // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                    Prime.Services.ConsoleService.PrintError($"Exception in {MethodBase.GetCurrentMethod()?.Name} - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                    throw;
                }
            }
        }
    }
}
