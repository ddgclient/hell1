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

namespace TosTriggersCallbacks
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using CommandLine;
    using Newtonsoft.Json;
    using Prime.SharedStorageService;

    /// <summary>
    /// This test methods registers TOSTriggers callbacks.
    /// </summary>
    public class TosTriggersCallbacks
    {
        private const string SharedStorageKey = "DDG_TosTriggersCallbacks";

        /// <summary>
        /// Callback function to setup apply set pin attribute based using command line parameter.
        /// Function should run as pre-instance.
        /// </summary>
        /// <param name="args">Argument String.</param>
        /// <returns>Nothing.</returns>
        public static string TosTriggersCallbackSetup(string args)
        {
            var parserResult = Parser.Default.ParseArguments<TosTriggersCallbacksOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            parserResult.WithParsed(
                options =>
                {
                    if (options.Type.ToLower() != "setpinattributes")
                    {
                        throw new ArgumentException($"Unsupported callback type={options.Type}");
                    }

                    Prime.Services.SharedStorageService.InsertRowAtTable(SharedStorageKey, options, Context.IP);
                }).
                WithNotParsed(e => throw new ArgumentException($"TosTriggersCallbacks: failed parsing arguments. {string.Join("\n", e)}"));

            return string.Empty;
        }

        /// <summary>
        /// Callback function to apply set pin attribute based on index..
        /// Requires to run TosTriggersCallbackSetup ahead.
        /// </summary>
        /// <param name="args">Argument String.</param>
        /// <returns>Nothing.</returns>
        public static string TosTriggersCallbackExecute(string args)
        {
            if (!(Prime.Services.SharedStorageService.GetRowFromTable(SharedStorageKey, typeof(TosTriggersCallbacksOptions), Context.IP) is TosTriggersCallbacksOptions options))
            {
                throw new Exception($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: TosTriggersCallbackSetup must run before executing TOSTrigger.");
            }

            var pinAttributes = new Dictionary<string, Dictionary<string, string>>();
            foreach (var value in options.Settings)
            {
                var tokens = value.Split(':');
                if (tokens.Length != 4)
                {
                    throw new ArgumentException($"TosTriggersCallbacks: SetPinAttributes: invalid value={value}. Expecting 4 tokens separated by :. Index:Pin:Attribute:Value");
                }

                if (tokens[0] != args)
                {
                    continue;
                }

                if (pinAttributes.ContainsKey(tokens[1]))
                {
                    pinAttributes[tokens[1]][tokens[2]] = tokens[3];
                }
                else
                {
                    var pin = new Dictionary<string, string> { { tokens[2], tokens[3] } };
                    pinAttributes.Add(tokens[1], pin);
                }
            }

            if (options.PrePause > 0 && pinAttributes.Count > 0)
            {
                Prime.Services.ConsoleService.PrintDebug($"Applying {nameof(options.PrePause)}={options.PrePause}");
                Thread.Sleep((int)options.PrePause);
            }

            foreach (var pin in pinAttributes)
            {
                Prime.Services.ConsoleService.PrintDebug($"Applying Pin Attributes for Pin={pin.Key} Values={JsonConvert.SerializeObject(pin.Value)}.");
                Prime.Services.PinService.SetPinAttributeValues(pin.Key, pin.Value);
            }

            if (options.PostPause > 0 && pinAttributes.Count > 0)
            {
                Prime.Services.ConsoleService.PrintDebug($"Applying {nameof(options.PostPause)}={options.PostPause}");
                Thread.Sleep((int)options.PostPause);
            }

            return string.Empty;
        }
    }
}
