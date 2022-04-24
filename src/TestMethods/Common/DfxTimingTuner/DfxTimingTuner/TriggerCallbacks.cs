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

namespace DfxTimingTuner
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="TriggerCallbacks" />.
    /// </summary>
    public static class TriggerCallbacks
    {
        /* private static readonly ConcurrentDictionary<string, TriggerContainer> TriggerInformation = new ConcurrentDictionary<string, TriggerContainer>(); */

        /// <summary>
        /// Delagage for TOS Trigger callbacks.
        /// </summary>
        /// <param name="pin">Pin to move.</param>
        /// <param name="increment">Value to increase by.</param>
        public delegate void PrimeSetTimingEdge(string pin, string increment);

        /// <summary>
        /// Gets the name of the shared storage object holding the increment value (in the IP context).
        /// </summary>
        public static string IncrementStorageName { get; } = "__IOTimingTuner__.IncrementValue";

        /// <summary>
        /// Gets the name of the shared storage object holding the Names of the pins to search as List of string (in the IP context).
        /// </summary>
        public static string PinListStorageName { get; } = "__IOTimingTuner__.PinList";

        /// <summary>
        /// Gets the name of the shared storage object holding the number of times the callback was triggered (in the IP context).
        /// </summary>
        public static string CallbackCountStorageName { get; } = "__IOTimingTuner__.TriggeredCount";

        /// <summary>
        /// Gets the name of the shared storage object holding the last failure (in the IP context).
        /// </summary>
        public static string FailStorageName { get; } = "__IOTimingTuner__.FailReason";

        /// <summary>
        /// .
        /// </summary>
        /// <param name="payload">TOSTrigger value.</param>
        public static void IncrementCompareEdge(string payload)
        {
            TriggerCallbacks.IncrementEdge(payload, "IncrementCompareEdge", Prime.Services.PinService.SetCompareTiming);
        }

        /// <summary>
        /// .
        /// </summary>
        /// <param name="payload">TOSTrigger value.</param>
        public static void IncrementDriveEdge(string payload)
        {
            TriggerCallbacks.IncrementEdge(payload, "IncrementDriveEdge", Prime.Services.PinService.SetDriveTiming);
        }

        /*
        /// <summary>
        /// Resets the static container.
        /// </summary>
        public static void ResetStorageForCallbacks()
        {
            var keys = new List<string>(TriggerInformation.Keys);
            for (var i = 0; i < keys.Count; i++)
            {
                TriggerInformation.TryRemove(keys[i], out var temp);
            }
        } */

        /// <summary>
        /// Save Shared storage elements for running trigger based io timing search.
        /// </summary>
        /// <param name="increment">Edge placement incrment value.</param>
        /// <param name="pins">List of pins.</param>
        public static void SetupStorageForCallbacks(double increment, List<string> pins)
        {
            /* string key = GetKey();
            TriggerInformation[key] = new TriggerContainer(increment.ToString(), pins); */
            Prime.Services.SharedStorageService.InsertRowAtTable(IncrementStorageName, increment.ToString(), Prime.SharedStorageService.Context.IP);
            Prime.Services.SharedStorageService.InsertRowAtTable(PinListStorageName, pins, Prime.SharedStorageService.Context.IP);
            Prime.Services.SharedStorageService.InsertRowAtTable(CallbackCountStorageName, 0, Prime.SharedStorageService.Context.IP);
            Prime.Services.SharedStorageService.InsertRowAtTable(FailStorageName, string.Empty, Prime.SharedStorageService.Context.IP);
        }

        /// <summary>
        /// Checks if there was any errors during the Trigger callbacks.
        /// </summary>
        /// <param name="error">String containing the error if there was one.</param>
        /// <returns>true if there was an error, false otherwise.</returns>
        public static bool GetFailureStatus(out string error)
        {
            error = Prime.Services.SharedStorageService.GetStringRowFromTable(FailStorageName, Prime.SharedStorageService.Context.IP);
            return !string.IsNullOrWhiteSpace(error);
        }

        /// <summary>
        /// Checks how many times the callback was executed.
        /// </summary>
        /// <returns>true if there was an error, false otherwise.</returns>
        public static int GetCallCount()
        {
            /* string key = GetKey();
            if (!TriggerInformation.ContainsKey(key))
            {
                throw new Prime.Base.Exceptions.TestMethodException($"TriggerInfo=[{key}] does not exist. TriggerCallbacks.SetupStorageForCallbacks() was probably not called.");
            }
            else
            {
                return TriggerInformation[key].CallCount;
            } */

            if (!Prime.Services.SharedStorageService.KeyExistsInIntegerTable(CallbackCountStorageName, Prime.SharedStorageService.Context.IP))
            {
                throw new Prime.Base.Exceptions.TestMethodException($"SharedStorage=[{CallbackCountStorageName}] does not exist. TriggerCallbacks.SetupStorageForCallbacks() was probably not called.");
            }
            else
            {
                return Prime.Services.SharedStorageService.GetIntegerRowFromTable(CallbackCountStorageName, Prime.SharedStorageService.Context.IP);
            }
        }

        /// <summary>
        /// TosTrigger callback function for incrementing a drive or strobe timing edge.
        /// </summary>
        /// <param name="payload">Payload value from TOSTrigger command.</param>
        /// <param name="caller">Name of the calling function.</param>
        /// <param name="func">Prime Service to use for incrementing the timing edge value.</param>
        public static void IncrementEdge(string payload, string caller, PrimeSetTimingEdge func)
        {
            try
            {
                /* string key = GetKey();
                var increment = TriggerInformation[key].Increment;
                var pins = TriggerInformation[key].Pins; */
                var increment = Prime.Services.SharedStorageService.GetStringRowFromTable(IncrementStorageName, Prime.SharedStorageService.Context.IP);
                var pins = (List<string>)Prime.Services.SharedStorageService.GetRowFromTable(PinListStorageName, typeof(List<string>), Prime.SharedStorageService.Context.IP);

                foreach (var pin in pins)
                {
                    func(pin, increment);
                }

                var count = Prime.Services.SharedStorageService.GetIntegerRowFromTable(CallbackCountStorageName, Prime.SharedStorageService.Context.IP);
                count += 1;
                Prime.Services.SharedStorageService.InsertRowAtTable(CallbackCountStorageName, count, Prime.SharedStorageService.Context.IP);
                /* TriggerInformation[key].CallCount = TriggerInformation[key].CallCount + 1; */
            }
            catch (Prime.Base.Exceptions.BaseException e)
            {
                Prime.Services.ConsoleService.PrintError($"Callback[{caller}] Caught Prime Exception=[{e.Message}].\n{e.StackTrace}");
                Prime.Services.SharedStorageService.InsertRowAtTable(FailStorageName, e.Message, Prime.SharedStorageService.Context.IP);
            }
        }

        /* private static string GetKey()
        {
            var dut = Prime.Services.TestProgramService.GetCurrentDutId();
            var ip = Prime.Services.TestProgramService.GetCurrentIpName();
            return $"DUT{dut}_IP{ip}";
        } */

        /// <summary>
        /// Defines a class which will be used to communicate to the IncrementEdge function <see cref="TriggerContainer" />.
        /// </summary>
        public class TriggerContainer
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TriggerContainer"/> class.
            /// </summary>
            /// <param name="increment">Increment amount as a string.</param>
            /// <param name="pins">List of pins/groups to adjust.</param>
            public TriggerContainer(string increment, IReadOnlyList<string> pins)
            {
                this.Increment = increment;
                this.Pins = pins;
                this.CallCount = 0;
            }

            /// <summary>
            /// Gets the size of the edge adjustment.
            /// </summary>
            public string Increment { get; }

            /// <summary>
            /// Gets the list of pins/pingroups to adjust.
            /// </summary>
            public IReadOnlyList<string> Pins { get; }

            /// <summary>
            /// Gets or sets the number of times the trigger was executed.
            /// </summary>
            public int CallCount { get; set; }
        }
    }
}
