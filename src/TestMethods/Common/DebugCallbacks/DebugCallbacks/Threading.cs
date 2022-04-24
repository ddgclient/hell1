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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandLine;

    /// <summary>
    /// Defines the <see cref="Threading" />.
    /// </summary>
    public class Threading
    {
        private static ConcurrentDictionary<string, Thread> ThreadTracker { get; set; } = new ConcurrentDictionary<string, Thread>();

        private static ConcurrentDictionary<string, List<Tuple<string, string>>> ResultsTracker { get; set; } = new ConcurrentDictionary<string, List<Tuple<string, string>>>();

        /// <summary>
        /// Executes a series of patconfigs in a separate thread.
        /// </summary>
        /// <param name="args">List of patconfig setpoints to execute in a separate thread.</param>
        /// <returns>dummy.</returns>
        public static string BackgroundPatConfigSetpoint(string args)
        {
            var context = TOSUserSDK.DUTs.Service.GetContext();
            var contextKey = GetKeyForContext(context);
            ResultsTracker[contextKey] = new List<Tuple<string, string>>();
            ThreadTracker[contextKey] = new Thread(() => RunPatModsInNewThread(context, contextKey, args.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()));
            Prime.Services.ConsoleService.PrintDebug($"Starting new thread to run patmods in background - {args}.");
            ThreadTracker[contextKey].Start();

            return string.Empty;
        }

        /// <summary>
        /// Waits for the thread created by running BackgroundExecute().
        /// </summary>
        /// <param name="args">Timeout for the Thread.Join().</param>
        /// <returns>dummy return.</returns>
        public static string BackgroundWait(string args)
        {
            var context = TOSUserSDK.DUTs.Service.GetContext();
            var contextKey = GetKeyForContext(context);
            var exitPort = DebugCallbacks.PASS;
            if (!ThreadTracker.ContainsKey(contextKey) || !ResultsTracker.ContainsKey(contextKey))
            {
                return DebugCallbacks.PASS; // todo: Should this be a Failure?
            }

            int timeout;
            if (!int.TryParse(args, out timeout))
            {
                timeout = 1000;
            }

            var joinPassed = ThreadTracker[contextKey].Join(timeout);
            if (!joinPassed)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"Thread=[{contextKey}] failed to complete in {timeout}mS.");
            }

            foreach (var result in ResultsTracker[contextKey])
            {
                var strgval = Prime.Services.DatalogService.GetItuffStrgvalWriter();
                strgval.SetTnamePostfix($"|{result.Item1}");
                strgval.SetData(result.Item2);
                Prime.Services.DatalogService.WriteToItuff(strgval);
                if (result.Item1 == "FAILED")
                {
                    exitPort = DebugCallbacks.FAIL;
                }
            }

            ResultsTracker.TryRemove(contextKey, out var dummy1);
            ThreadTracker.TryRemove(contextKey, out var dummy2);
            return exitPort;
        }

        /// <summary>
        /// Gets the full list of instances and runs Verify concurrently.
        /// </summary>
        /// <param name="args">No arguments.</param>
        public static void ParallelVerifyAllInstances(string args)
        {
            var context = TOSUserSDK.DUTs.Service.GetContext();
            var contextKey = GetKeyForContext(context);
            var instanceNames = Prime.Services.TestProgramService.GetAllTestInstanceNames();
            Parallel.ForEach(instanceNames, instanceName =>
                {
                    context.Associate();
                    Prime.Services.ConsoleService.PrintDebug($"Running Parallel Verify for ContextKey=[{contextKey}] Instance=[{instanceName}].");
                    if (!Prime.Services.TestProgramService.VerifyTestInstance(instanceName))
                    {
                        throw new Exception($"ParallelVerifyAllInstances failed for instance=[{instanceName}].");
                    }
                });
        }

        private static void RunPatModsInNewThread(TOSUserSDK.IDUTs context, string contextKey, List<string> patMods)
        {
            try
            {
                // If this fails we won't even be able to print to the console or log anything to ituff.
                context.Associate();
            }
            catch
            {
                ResultsTracker[contextKey] = new List<Tuple<string, string>> { new Tuple<string, string>("FAILED", $"Thread=[{contextKey}]_Failed_context.Associate()_thread_did_not_run.") };
                return;
            }

            Prime.Services.ConsoleService.PrintDebug($"Thread=[{contextKey}] Connected.");
            List<Tuple<string, string>> testResults = new List<Tuple<string, string>>();
            try
            {
                foreach (var config in patMods)
                {
                    PatConfig.ExecutePatConfigSetPoint(config);
                    testResults.Add(new Tuple<string, string>(config, "complete"));
                }
            }
            catch (Exception e)
            {
                Prime.Services.ConsoleService.PrintError($"Thread=[{contextKey}] Threw an exception: {e.Message}\n{e.StackTrace}");
                testResults.Add(new Tuple<string, string>("FAILED", $"Exception|{e.Message.Replace(" ", "_").Replace('\n', '|')}"));
            }

            ResultsTracker[contextKey] = testResults;
            return;
        }

        private static string GetKeyForContext(TOSUserSDK.IDUTs context)
        {
            var dutID = context.GetCurrentDutId();
            var dutIP = context.GetCurrentIpName();
            var key = $"{dutID}_{dutIP}";
            return key;
        }

        private class ConcurrentExecuteOptions
        {
            [Option("flow1", Required = true, HelpText = "Comma-separated list of tests.")]
            public string Flow1Tests { get; set; }

            [Option("flow2", Required = true, HelpText = "Comma-separated list of tests.")]
            public string Flow2Tests { get; set; }
        }
    }
}
