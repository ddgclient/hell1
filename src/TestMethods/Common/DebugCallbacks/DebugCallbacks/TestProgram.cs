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
    using System.Threading;
    using CommandLine;
    using DDG;
    using Prime.ConsoleService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Defines the <see cref="TestProgram" />.
    /// </summary>
    public class TestProgram
    {
        /// <summary>
        /// Execute the testinstance(s).
        /// </summary>
        /// <param name="args">Argument string of the form --test comma-separated-list-of-tests [--exception_on_fail].</param>
        /// <returns>The exit port of the executed tests as a comma-separated string.</returns>
        public static string ExecuteInstance(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                try
                {
                    var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                    var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                    var parserResult = Parser.Default.ParseArguments<ExecuteInstanceOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    var returnValue = string.Empty;
                    parserResult.WithParsed(options =>
                    {
                        var tests = options.TestsToRun.Split(',');
                        var results = new List<string>(tests.Length);
                        foreach (var test in tests)
                        {
                            var exitPort = Prime.Services.TestProgramService.ExecuteTestInstance(test);
                            console?.PrintDebug($"[{instanceName}] Test=[{test}] Exited Port=[{exitPort}].");
                            if (options.ExceptionOnFail && exitPort < 1)
                            {
                                throw new Prime.Base.Exceptions.TestMethodException($"[{instanceName}] Test=[{test}] failed. Exit Port=[{exitPort}].");
                            }

                            results.Add(exitPort.ToString());
                        }

                        returnValue = string.Join(",", results);
                        if (!string.IsNullOrWhiteSpace(options.GsdsExitPort))
                        {
                            DDG.Gsds.WriteToken(options.GsdsExitPort, returnValue);
                        }
                    }).
                        WithNotParsed(e => throw new ArgumentException($"{instanceName}.ExecuteInstance: failed parsing arguments. {string.Join("\n", e)}"));
                    return returnValue;
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
        /// Sleep callback in mS.
        /// </summary>
        /// <param name="args">Time in mS.</param>
        public static void Sleep(string args)
        {
            using (var sw = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                var time = args.ToInt();
                Prime.Services.ConsoleService.PrintDebug($"Sleep for {time} mS");
                Thread.Sleep(time);
            }
        }

        /// <summary>
        /// Gets a thread lock for serial test execution.
        /// </summary>
        /// <param name="args">Time-out in mS.</param>
        public static void LockThread(string args)
        {
            const string key = "_DDG_Active_Lock";
            var currentDutId = Prime.Services.TestProgramService.GetCurrentDutId();
            var currentDutIndex = Prime.Services.TestProgramService.GetCurrentDutIndex();

            var startTime = DateTime.Now.Millisecond;
            var timeOut = string.IsNullOrEmpty(args) ? 30000 : args.ToInt();

            while (true)
            {
                var activeLock = "true";
                try
                {
                    activeLock = Prime.Services.SharedStorageService.GetStringRowFromTable(key, Context.LOT);
                }
                catch
                {
                    activeLock = "false";
                }

                if (activeLock == "false")
                {
                    Prime.Services.ConsoleService.PrintDebug($"No thread lock is active. Activating thread lock and continuing execution on DutIndex=[{currentDutIndex}], DutId=[{currentDutId}].");
                    Prime.Services.SharedStorageService.InsertRowAtTable(key, "true", Context.LOT);
                    Prime.Services.SharedStorageService.OverrideStringRowResetPolicy(key, ResetPolicy.NEVER_RESET, Context.LOT);
                    break;
                }

                Prime.Services.ConsoleService.PrintDebug("Sleeping 10ms while thread lock gets released.");
                Thread.Sleep(10);

                if (DateTime.Now.Millisecond - startTime > timeOut)
                {
                    throw new Exception($"ThreadLock as exceed time-out=[{timeOut}].");
                }
            }
        }

        /// <summary>
        /// Releases thread lock for serial test execution.
        /// </summary>
        /// <param name="args">Time-out in mS.</param>
        public static void ReleaseThread(string args)
        {
            const string key = "_DDG_Active_Lock";
            var currentDutId = Prime.Services.TestProgramService.GetCurrentDutId();
            var currentDutIndex = Prime.Services.TestProgramService.GetCurrentDutIndex();

            Prime.Services.ConsoleService.PrintDebug($"Releasing thread lock on DutIndex=[{currentDutIndex}], DutId=[{currentDutId}].");
            Prime.Services.SharedStorageService.InsertRowAtTable(key, "false", Context.LOT);
            Prime.Services.SharedStorageService.OverrideStringRowResetPolicy(key, ResetPolicy.NEVER_RESET, Context.LOT);
        }

        /// <summary>
        /// UserFunction to run verify on all the Prime instances.
        /// Also disables the LogLevel.
        /// </summary>
        /// <param name="args">Nothing.</param>
        /// <returns>PASS.</returns>
        public static string VerifyAllPrimeInstances(string args)
        {
            // get the name of this instance so we don't verify it by mistake.
            var currentInstance = Prime.Services.TestProgramService.GetCurrentTestInstanceName();

            // TODO: to speed up printing (optimize later based on current instances log level)
            var currentParams = Prime.Services.TestProgramService.GetTestInstanceParameters(currentInstance);
            IConsoleService console = Prime.Services.ConsoleService;
            if (!currentParams.ContainsKey("LogLevel") || (currentParams["LogLevel"] == "DISABLED"))
            {
                console = null;
            }

            // loop through every instance.
            var failingInstances = new List<string>();
            var allInstances = Prime.Services.TestProgramService.GetAllTestInstanceNames();
            foreach (var instance in allInstances)
            {
                // skip if its the current instance.
                if (instance == currentInstance)
                {
                    continue;
                }

                // check if its a prime instance by looking at the parameters.
                var parameters = Prime.Services.TestProgramService.GetTestInstanceParameters(instance);
                if (parameters.ContainsKey("LogLevel"))
                {
                    // its a Prime instance. Make sure the logging is disabled.
                    if (parameters["LogLevel"] != "DISABLED")
                    {
                        Prime.Services.TestProgramService.SetTestInstanceParameter(instance, "LogLevel", "DISABLED");
                    }

                    console?.PrintDebug($"Verifying {instance}");
                    if (!Prime.Services.TestProgramService.VerifyTestInstance(instance))
                    {
                        failingInstances.Add(instance);
                    }
                }
                else
                {
                    // not a prime instance
                    console?.PrintDebug($"Skipping {instance}");
                }
            }

            if (failingInstances.Count > 0)
            {
                Prime.Services.ConsoleService.PrintError($"Failed Verify for the following [{failingInstances.Count}] instances:\n {string.Join("\n", failingInstances)}\n\n");
                throw new Prime.Base.Exceptions.TestMethodException("Some instances failed Verify.");
            }

            return DebugCallbacks.PASS;
        }

        private class ExecuteInstanceOptions
        {
            [Option("test", Required = true, HelpText = "Comma-separated list of tests to execute.")]
            public string TestsToRun { get; set; }

            [Option("save_exit_port", Required = false, HelpText = "GSDS (of the form G.U.S.Token) to save the exit ports to (as a comma-separated string).")]
            public string GsdsExitPort { get; set; } = string.Empty;

            [Option("exception_on_fail", Required = false, HelpText = "Causes an exception to be thrown (which will force an exit on port -1) if any of the executed tests fail.")]
            public bool ExceptionOnFail { get; set; } = false;
        }
    }
}
