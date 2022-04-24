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

namespace BackgroundPatConfig
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Prime.PatConfigService;
    using Prime.PhAttributes;
    using Prime.SharedStorageService;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class BackgroundPatConfig : TestMethodBase
    {
        /// <summary>
        /// Enable type.
        /// </summary>
        public enum ModeType
        {
            /// <summary>
            /// Start the thread.
            /// </summary>
            Start,

            /// <summary>
            /// Wait for the thread to complete.
            /// </summary>
            Wait,

            /// <summary>
            /// Start the thread and wait for it to complete.
            /// </summary>
            StartAndWait,
        }

        /// <summary>
        /// Gets or sets the list of PatConfigSetpoints to execute.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString PatConfigSetpointList { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets Mode of the template.
        /// </summary>
        public ModeType Mode { get; set; } = ModeType.Start;

        /// <summary>
        /// Gets or sets the Timeout in milli-seconds for Wait mode.
        /// </summary>
        public TestMethodsParams.Integer WaitTimeout { get; set; } = 1000;

        private static ConcurrentDictionary<string, Thread> ThreadTracker { get; set; } = new ConcurrentDictionary<string, Thread>();

        private static ConcurrentDictionary<string, List<Tuple<string, string>>> ResultsTracker { get; set; } = new ConcurrentDictionary<string, List<Tuple<string, string>>>();

        private List<Tuple<IPatConfigSetPointHandle, string>> PatConfigHandlesAndData { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            if ((this.Mode == ModeType.Start || this.Mode == ModeType.StartAndWait) && string.IsNullOrWhiteSpace(this.PatConfigSetpointList))
            {
                throw new Prime.Base.Exceptions.TestMethodException("Parameter=[PatConfigSetpointList] is required when Mode=[Start].");
            }

            if ((this.Mode == ModeType.Wait || this.Mode == ModeType.StartAndWait) && this.WaitTimeout <= 0)
            {
                throw new Prime.Base.Exceptions.TestMethodException("Parameter=[WaitTimeout] is required to be positive when Mode=[Wait].");
            }

            this.PatConfigHandlesAndData = new List<Tuple<IPatConfigSetPointHandle, string>>();
            foreach (var setpointString in this.PatConfigSetpointList.ToList())
            {
                var tokens = setpointString.Split(':').ToList();
                if (tokens.Count != 3)
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"Error in PatConfigSetpoint Format=[{setpointString}]. Expecting [MODULE:GROUP:SETPOINT].");
                }

                Prime.Services.ConsoleService.PrintDebug($"Creating IPatConfigSetPointHandle with Module=[{tokens[0]}] Group=[{tokens[1]}] SetPoint=[{tokens[2]}].");
                this.PatConfigHandlesAndData.Add(new Tuple<IPatConfigSetPointHandle, string>(Prime.Services.PatConfigService.GetSetPointHandle(tokens[0], tokens[1]), tokens[2]));
            }
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            if (this.Mode == ModeType.Start || this.Mode == ModeType.StartAndWait)
            {
                var context = TOSUserSDK.DUTs.Service.GetContext();
                var contextKey = GetKeyForContext(context);
                ResultsTracker[contextKey] = new List<Tuple<string, string>>();
                ThreadTracker[contextKey] = new Thread(() => RunPatModsInNewThread(context, contextKey, this.PatConfigHandlesAndData));
                Prime.Services.ConsoleService.PrintDebug($"Starting new thread=[{contextKey}] to run patmods in background.");
                ThreadTracker[contextKey].Start();
            }

            if (this.Mode == ModeType.Wait || this.Mode == ModeType.StartAndWait)
            {
                var success = this.BackgroundWait(this.WaitTimeout);
                return success ? 1 : 0;
            }

            return 1;
        }

        private static void RunPatModsInNewThread(TOSUserSDK.IDUTs context, string contextKey, List<Tuple<IPatConfigSetPointHandle, string>> allSetPointsAndData)
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
                foreach (var setPointAndData in allSetPointsAndData)
                {
                    setPointAndData.Item1.ApplySetPoint(setPointAndData.Item2);
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

        private bool BackgroundWait(int timeout)
        {
            var context = TOSUserSDK.DUTs.Service.GetContext();
            var contextKey = GetKeyForContext(context);
            if (!ThreadTracker.ContainsKey(contextKey) || !ResultsTracker.ContainsKey(contextKey))
            {
                return true; // Nothing to do, just pass.
            }

            Prime.Services.ConsoleService.PrintDebug($"Waiting on Thread=[{contextKey}] Timeout=[{timeout}ms].");
            var joinPassed = ThreadTracker[contextKey].Join(timeout);
            if (!joinPassed)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"Thread=[{contextKey}] failed to complete in {timeout}mS.");
            }

            var success = true;
            foreach (var result in ResultsTracker[contextKey])
            {
                var strgval = Prime.Services.DatalogService.GetItuffStrgvalWriter();
                strgval.SetTnamePostfix($"|{result.Item1}");
                strgval.SetData(result.Item2);
                Prime.Services.DatalogService.WriteToItuff(strgval);
                if (result.Item1 == "FAILED")
                {
                    success = false;
                }
            }

            ResultsTracker.TryRemove(contextKey, out var dummy1);
            ThreadTracker.TryRemove(contextKey, out var dummy2);
            return success;
        }
    }
}
