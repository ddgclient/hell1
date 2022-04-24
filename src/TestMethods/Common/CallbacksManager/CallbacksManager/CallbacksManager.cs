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

namespace CallbacksManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Prime.PhAttributes;
    using Prime.TestMethods.CallbacksRegistrar;

    /// <summary>
    /// Callback to run multiple pre-registered callbacks.
    /// </summary>
    [PrimeTestMethod]
    public class CallbacksManager : PrimeCallbacksRegistrarTestMethod
    {
        /// <summary>
        /// Functional callback to call multiple pre-registered callbacks.
        /// </summary>
        /// <param name="args">List is callbacks. Format: Function1(arg1) | ... |  FunctionN(argN).</param>
        /// <returns>Nothing.</returns>
        public static string Call(string args)
        {
            var pass = true;
            var rgx = new Regex(@"(\w+)\((.*?)\)\s*\|*");
            if (!rgx.IsMatch(args))
            {
                throw new ArgumentException(
                    $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: invalid {nameof(args)}={args} format.");
            }

            foreach (Match match in rgx.Matches(args))
            {
                var function = match.Groups[1].Value;
                var argument = match.Groups[2].Value;
                if (!Prime.Services.TestProgramService.DoesCallbackExist(function))
                {
                    throw new ArgumentException(
                        $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: callback {function} has not been registered.");
                }

                Prime.Services.ConsoleService.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: function=[{function}] argument=[{argument}]");
                var result = Prime.Services.TestProgramService.TriggerCallback(function, argument).ToLower();
                pass &= result != "fail" && result != "0";
            }

            return pass ? "pass" : "fail";
        }

        /// <summary>
        /// Function meant to be called from the TOS Shmoo GuiCallback dimension.
        /// Supports 2 argument formats.
        ///    1. arguments|function|PreInstance .
        ///    2. function(arguments)|testname|PreInstance .
        ///    in both cases the final "PreInstance" is ignored, I have no idea what that is.
        ///    *note* exceptions aren't being printed, so using PrintError to make sure everything goes to the console.
        /// </summary>
        /// <param name="args">Function/Arguments to call.</param>
        /// <returns>Return value from the callback.</returns>
        public static string GuiCall(string args)
        {
            var argsSplit = args.Split('|').ToList();
            if (argsSplit.Count < 3)
            {
                Prime.Services.ConsoleService.PrintError($"GuiCallback is expecting at least 3 pipe-separated elements. Actual=[{args}].");
                throw new ArgumentException($"GuiCallback is expecting at least 3 pipe-separated elements. Actual=[{args}].");
            }

            // Get the last 2 elements in the list and then combine all the others.
            // The function arguments might contain | but the name/PreInstance never will.
            var lastArgIgnore = argsSplit.Last();
            argsSplit.RemoveAt(argsSplit.Count - 1);
            var name = argsSplit.Last();
            argsSplit.RemoveAt(argsSplit.Count - 1);
            var functionArgs = string.Join("|", argsSplit);

            // Check if name is a function name for a test name.
            var functionName = name;
            if (!Prime.Services.TestProgramService.DoesCallbackExist(functionName))
            {
                var rgx = new Regex(@"^\s*([^\(\s]*)\s*\((.*?)\)\s*$");
                if (!rgx.IsMatch(functionArgs))
                {
                    Prime.Services.ConsoleService.PrintError($"Callback=[{functionName}] does not exist and Argument=[{functionArgs}] is not in Function(Argument) format.");
                    throw new ArgumentException($"Callback=[{functionName}] does not exist and Argument=[{functionArgs}] is not in Function(Argument) format.");
                }

                var match = rgx.Match(functionArgs);
                functionName = match.Groups[1].Value;
                functionArgs = match.Groups[2].Value;
                if (!Prime.Services.TestProgramService.DoesCallbackExist(functionName))
                {
                    Prime.Services.ConsoleService.PrintError($"Callback=[{functionName}] does not exist.");
                    throw new ArgumentException($"Callback=[{functionName}] does not exist.");
                }
            }

            try
            {
                Prime.Services.ConsoleService.PrintDebug($"GuiCallback: Function=[{functionName}] Arguments=[{functionArgs}].");
                var retval = Prime.Services.TestProgramService.TriggerCallback(functionName, functionArgs);
                return retval;
            }
            catch (Exception e)
            {
                // exceptions aren't getting printed so do it manually.
                Prime.Services.ConsoleService.PrintError($"{e.GetType()} {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        /// <inheritdoc />
        public override void RegisterCallbacks()
        {
            /* ================================================================================== */
            // Add your callback one of these lists depending on if it returns void or a string.
            // Also add a reference to the project its defined in.
            var stringReturnFunctions = new List<Prime.TestProgramService.CallbackDelegate>
            {
                CallbacksManager.Call,
                CallbacksManager.GuiCall,
                DebugCallbacks.SharedStorage.WriteSharedStorage,
                DebugCallbacks.SharedStorage.PrintSharedStorage,
                DebugCallbacks.TestConditions.ApplyEndSequence,
                DebugCallbacks.TestConditions.DisableSmartTC,
                DebugCallbacks.TestConditions.EnableSmartTC,
                DebugCallbacks.TestConditions.SetPowerUpTCName,
                DebugCallbacks.TestConditions.FlushAllSmartTCCategories,
                DebugCallbacks.TestConditions.FlushSmartTCCategory,
                DebugCallbacks.TestProgram.ExecuteInstance,
                DebugCallbacks.Dff.MirrorDff,
                DebugCallbacks.Dff.PrintDff,
                DebugCallbacks.Dff.WriteDff,
                DebugCallbacks.Dff.SetCurrentDieId,
                DebugCallbacks.DV.JsonRun,
                DebugCallbacks.DV.JsonRecorder,
                TosTriggersCallbacks.TosTriggersCallbacks.TosTriggersCallbackSetup,
                TosTriggersCallbacks.TosTriggersCallbacks.TosTriggersCallbackExecute,
                UserCodeCallbacks.UserCodeCallbacks.CompileUserCode,
                UserCodeCallbacks.UserCodeCallbacks.RunUserCode,
                FlowControlCallbacks.FlowControlCallbacks.CheckFlow,
                FlowControlCallbacks.FlowControlCallbacks.SetFlow,
                DieRecoveryCallbacks.DieRecoveryCallbacks.MaskIP,
                DebugCallbacks.Threading.BackgroundWait,
                DebugCallbacks.Threading.BackgroundPatConfigSetpoint,
                DebugCallbacks.UserVar.WriteUserVar,
                DebugCallbacks.Datalog.PrintToItuff,
                PupCallBacks.PupCallBacks.IsPlistEligibleForTTR,
                DebugCallbacks.Auxiliary.EvaluateExpression,
                DebugCallbacks.Functional.ExecuteNoCapturePlist,
                DebugCallbacks.TestProgram.VerifyAllPrimeInstances,
            };

            var voidReturnFunctions = new List<VoidCallbackDelegate>
            {
                DfxTimingTuner.TriggerCallbacks.IncrementCompareEdge,
                DfxTimingTuner.TriggerCallbacks.IncrementDriveEdge,
                DieRecoveryCallbacks.DieRecoveryCallbacks.ConfigureIpForRecovery,
                DieRecoveryCallbacks.DieRecoveryCallbacks.DisableIP,
                DieRecoveryCallbacks.DieRecoveryCallbacks.WriteTracker,
                DieRecoveryCallbacks.DieRecoveryCallbacks.CopyTracker,
                DieRecoveryCallbacks.DieRecoveryCallbacks.CloneTracker,
                DieRecoveryCallbacks.DieRecoveryCallbacks.LoadPinMapFile,
                DieRecoveryCallbacks.DieRecoveryCallbacks.RunRule,
                SocRecoveryCallbacks.SocRecoveryCallbacks.InitialiseSOCRecovery,
                SocRecoveryCallbacks.SocRecoveryCallbacks.SetSOCRecoveryToken,
                SocRecoveryCallbacks.SocRecoveryCallbacks.PrintTokenToItuffDLCP,
                VminForwardingCallbacks.VminForwardingCallbacks.VminSearchStore,
                VminForwardingCallbacks.VminForwardingCallbacks.VminInterpolation,
                VminForwardingCallbacks.VminForwardingCallbacks.LoadVminFromDFF,
                VoltageConverterCallbacks.VoltageConverterCallbacks.VoltageConverter,
                DebugCallbacks.PatConfig.ExecutePatConfig,
                DebugCallbacks.PatConfig.ExecutePatConfigSetPoint,
                DebugCallbacks.PatConfig.BitVectorPatConfigSetPoint,
                DebugCallbacks.TestProgram.Sleep,
                DebugCallbacks.TestProgram.LockThread,
                DebugCallbacks.TestProgram.ReleaseThread,
                DebugCallbacks.TestConditions.SetPinAttributes,
                DebugCallbacks.TestConditions.ValidatePatternTriggerMap,
                DebugCallbacks.TestConditions.ApplyPatternTriggerMap,
                DebugCallbacks.OnlyMe.AcquireOnlyMeLock,
                DebugCallbacks.OnlyMe.ReleaseOnlyMeLock,
                DebugCallbacks.Plist.CleanPlist,
                DebugCallbacks.Plist.RestorePlist,
                DebugCallbacks.Threading.ParallelVerifyAllInstances,
                DebugCallbacks.Datalog.SetAltInstanceName,
            };
            /* ================================================================================== */

            Prime.Services.ConsoleService.PrintDebug($"[{this.InstanceName}] Registering any unregistered callback functions.");
            foreach (var f in stringReturnFunctions)
            {
                if (!Prime.Services.TestProgramService.DoesCallbackExist(f.GetMethodInfo().Name))
                {
                    Prime.Services.ConsoleService.PrintDebug($"[{this.InstanceName}] Registering callback=[{f.GetMethodInfo().Name}].");
                    this.RegisterCallback(f);
                }
            }

            foreach (var f in voidReturnFunctions)
            {
                if (!Prime.Services.TestProgramService.DoesCallbackExist(f.GetMethodInfo().Name))
                {
                    Prime.Services.ConsoleService.PrintDebug($"[{this.InstanceName}] Registering callback=[{f.GetMethodInfo().Name}].");
                    this.RegisterCallback(f);
                }
            }

            // Register GuiCall with the GUI
            Prime.Services.TestProgramService.RegisterGuiCallback("GuiCall");

            Prime.Services.ConsoleService.PrintDebug($"[{this.InstanceName}] Done.");
        }
    }
}
