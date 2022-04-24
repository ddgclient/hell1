// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// INTEL CONFIDENTIAL
// Copyright (2019) (2020) Intel Corporation
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

namespace PrimeValTool
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Wrapper;

    /// <summary>
    /// This Class provides tos related commands to be run on tester ex: startLot.
    /// </summary>
    public class TOSControlCommander // FIXME: make this class one to instantiate. every method requires the same references, just make it easier...
    {
        /// <summary>
        /// All possible commands to pass to TOS
        /// </summary>
        private enum TOSCommands
        {
            GetAllTestInstances,
            GetTestInstanceResult,
            GetTosStatus,
            SiteStatus,
            StartTOS,
            StopTOS,
            RestartTOS,
            SwitchTOS,
            StopConsolidatedLogging,
            IsTPLoaded,
            UnloadTP,
            Init,
            StartLot,
            TestUnit,
            EndLot,
        }

        /// <summary>
        /// Mapping of all possible commands to their string equivalent for submission to TOS console.
        /// </summary>
        private Dictionary<TOSCommands, string> commandConversion = new Dictionary<TOSCommands, string>()
        {
            { TOSCommands.GetAllTestInstances, "getAllTestInstances" },
            { TOSCommands.GetTestInstanceResult, "getTestInstanceResult" },
            { TOSCommands.GetTosStatus, "getTosStatus" },
            { TOSCommands.SiteStatus, "sitestatus" },
            { TOSCommands.StartTOS, "starttos" },
            { TOSCommands.StopTOS, "stoptos" },
            { TOSCommands.RestartTOS, "restarttos" },
            { TOSCommands.SwitchTOS, "switchtos" },
            { TOSCommands.StopConsolidatedLogging, "stopConsolidatedLogging" },
            { TOSCommands.IsTPLoaded, "isTPLoaded" },
            { TOSCommands.UnloadTP, "unloadTP" },
            { TOSCommands.Init, "init" },
            { TOSCommands.StartLot, "startLot" },
            { TOSCommands.TestUnit, "testUnit" },
            { TOSCommands.EndLot, "endLot" },

        };

        public TOSControlCommander(string commandPath)
        {
            this.CommandPath = commandPath;
        }

        public string CommandPath { get; set; }

        /// <summary>
        /// This method is being used by all tos related commands to run.
        /// </summary>
        /// <param name="commandPath"> Path of hdmttosctrl.exe for switching/stopping tos or singlescriptcmd path for load and init.</param>
        /// <param name="argument"> Argument needed for hdmttosctrl.exe or singlescriptcmd.exe.</param>
        /// <returns>Return true if TOS is working.</returns>
        protected internal static bool ProcessTosRelatedCommands(string commandPath, string argument, string logsFolderPath)
        {
            var myList = new List<string>();

            try
            {
                var process = new Process();
                var isTpLoaded = true;
                var isTosActivated = false;
                var isTosWorking = true;
                Directory.CreateDirectory(logsFolderPath);
                var tosOutStream = Path.Combine(logsFolderPath, @"Tosoutput.log");
                process.StartInfo.FileName = commandPath;
                process.StartInfo.Arguments = argument;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine();

                    if (line == null)
                    {
                        continue;
                    }

                    if (!argument.Contains("stopConsolidatedLogging") && line.Contains("TOS Not Running"))
                    {
                        isTosWorking = false;
                    }

                    if (line.Contains("Exception rethrown at"))
                    {
                        isTosWorking = false;
                    }

                    if (line.Contains("No TP loaded"))
                    {
                        isTpLoaded = false;
                    }

                    if (argument.Contains("getAllTestInstances"))
                    {
                        if (line.Contains("GET_ALL_TEST_INSTANCES"))
                        {
                            myList.Clear();
                        }

                        if (!string.IsNullOrEmpty(line))
                        {
                            myList.Add(line);
                        }
                    }

                    if (argument.Contains("getTestInstanceResult"))
                    {
                        if (line.Contains("Output Port"))
                        {
                            var port = line.Split('=');
                            PValMain.InstancesPort = port[1];
                        }

                        if (line.Contains("PassFailStatus"))
                        {
                            var port = line.Split('=');
                            PValMain.InstancesStatus = port[1];
                        }
                    }

                    if (argument.Contains("getTosStatus"))
                    {
                        if (line.Contains("OPERATION MODE:"))
                        {
                            var mode = line.Split(':');
                            PValMain.TosMode = mode[1];
                        }
                    }

                    if (argument.Contains("sitestatus"))
                    {
                        if (line.Contains("TosActivated"))
                        {
                            isTosActivated = true;
                        }
                    }

                    using (var file = new StreamWriter(tosOutStream, true))
                    {
                        file.WriteLine(line);
                    }
                }

                process.WaitForExit();

                if (!isTosWorking)
                {
                    throw new Exception("TOS is not running.");
                }

                switch (argument)
                {
                    case "getAllTestInstances":
                        PValMain.TestInstancesFromTester = myList;
                        break;
                    case "isTPLoaded" when isTpLoaded:
                        return true;
                    case "isTPLoaded":
                    case "sitestatus" when !isTosActivated:
                        return false;
                    case "sitestatus":
                        Handlers.LoggerHandler.PrintLine("Tos is activated.", PrintType.LOGGER_ONLY);
                        return true;
                }
            }
            catch (Exception e)
            {
                var message = $"Tos process command fail with commandPath=[{commandPath}] and argument=[{argument}].\n\t{e.Message}\n";
                throw new Exception(message);
            }

            return true;
        }

        /// <summary>
        /// Changes current TOS version being used to one specified by <see cref="PValMain.MainTosVersionToUse"/> if not the one currently in use.
        /// </summary>
        protected internal static void SwitchToMainTOSVersion(RuntimeCollaterals toolCollaterals)
        {
            if (ProcessTosRelatedCommands(toolCollaterals.TOSControlPathForCurrentTos, "sitestatus", toolCollaterals.TOSLogsPath))
            {
                Handlers.LoggerHandler.PrintLine($"Stopping TOS.", Wrapper.PrintType.DEFAULT);
                ProcessTosRelatedCommands(toolCollaterals.TOSControlPathForCurrentTos, "stoptos", toolCollaterals.TOSLogsPath);
                System.Threading.Thread.Sleep(5000);
                if (ProcessTosRelatedCommands(toolCollaterals.TOSControlPathForCurrentTos, "sitestatus", toolCollaterals.TOSLogsPath))
                {
                    throw new Exception("Failed to stop TOS.");
                }
            }

            if (PValMain.MainTOSVersionToUse == toolCollaterals.CurrentTOSReleaseDirectory)
            {
                Handlers.LoggerHandler.PrintLine($"No Tos Switch Needed = {PValMain.MainTOSVersionToUse}.", Wrapper.PrintType.DEFAULT);
            }
            else
            {
                var newTosControlPath = Path.GetFullPath(
                    Path.Combine(PValMain.MainTOSVersionToUse, @"Runtime\Release\HdmtSupervisorService\hdmttosctrl.exe"));
                ProcessTosRelatedCommands(newTosControlPath, "switchtos" + " " + PValMain.MainTOSVersionToUse, toolCollaterals.TOSLogsPath);
                Handlers.LoggerHandler.PrintLine($"TOS switched succesfully ={PValMain.MainTOSVersionToUse}", Wrapper.PrintType.DEFAULT);
                System.Threading.Thread.Sleep(5000);
                /* ProcessTosRelatedCommands(newTosControlPath, "starttos", toolCollaterals.TOSLogsPath);
                Handlers.LoggerHandler.PrintLine($"TOS started successfully = {PValMain.MainTOSVersionToUse}.", Wrapper.PrintType.DEFAULT); */
            }
        }

        /// <summary>
        /// Examine TOS status; restart TOS in case it is down.
        /// </summary>
        /// <param name="isForcedRestartNeeded"> If TOS requires a forced restart. </param>
        protected internal static void TOSRestarter(string logsFolderPath, bool isForcedRestartNeeded = false)
        {
            var tosControlPath = Path.GetFullPath(
                Path.Combine(PValMain.MainTOSVersionToUse, @"Runtime\Release\HdmtSupervisorService\hdmttosctrl.exe"));

            if (isForcedRestartNeeded)
            {
                if (PValMain.IsHdmtLogsRequired)
                {
                    Handlers.LoggerHandler.PrintLine("Stopping TOS...", Wrapper.PrintType.DEFAULT);
                    ProcessTosRelatedCommands(tosControlPath, "stoptos", logsFolderPath);
                    Directory.Delete(PValMain.HdmtLogPath, recursive: true);
                    Handlers.LoggerHandler.PrintLine("Deleted current HDMT logs...", Wrapper.PrintType.DEBUG);
                    Handlers.LoggerHandler.PrintLine("Starting TOS...", Wrapper.PrintType.DEFAULT);
                    ProcessTosRelatedCommands(tosControlPath, "starttos", logsFolderPath);
                }
                else
                {
                    Handlers.LoggerHandler.PrintLine("Restarting TOS...", Wrapper.PrintType.DEFAULT);
                    ProcessTosRelatedCommands(tosControlPath, "restarttos", logsFolderPath);
                }
            }

            if (!ProcessTosRelatedCommands(tosControlPath, "sitestatus", logsFolderPath))
            {
                Handlers.LoggerHandler.PrintLine("Restarting TOS...", Wrapper.PrintType.DEFAULT);
                ProcessTosRelatedCommands(tosControlPath, "restarttos", logsFolderPath);

                if (!ProcessTosRelatedCommands(tosControlPath, "sitestatus", logsFolderPath))
                {
                    throw new Exception("TOS was not activated after being given the restart command.");
                }
            }
        }

        /// <summary>
        /// This method start console log on tester using singlescriptcommand.
        /// </summary>
        /// <param name="singleScriptPath">Path of singlescriptcmd.exe.</param>
        /// <param name="automationDirectory">Path of the directory of the tp.</param>
        /// <param name="tosSpecificPath">Path where the console log to stored.</param>
        protected internal static void StartConsolidatedLogging(string logsFolderPath, string singleScriptPath, string automationDirectory, string tosSpecificPath)
        {
            const string consoleLogName = "console.log";
            var consoleLogPathName = $"{tosSpecificPath}\\{consoleLogName}";
            var consoleLogFullPath = "startConsolidatedLogging" + " " +
                             Path.Combine(automationDirectory, consoleLogPathName);
            try
            {
                ProcessTosRelatedCommands(singleScriptPath, consoleLogFullPath, logsFolderPath);
            }
            catch
            {
                TOSRestarter(logsFolderPath, true);
                ProcessTosRelatedCommands(singleScriptPath, consoleLogFullPath, logsFolderPath);
            }
        }

        /// <summary>
        /// This method runs Test program related commands ex:startLot, Init.
        /// </summary>
        /// <param name="singleScriptPath">Path of singlescriptcmd.exe.</param>
        /// <param name="plans">List of plans selected by user.</param>
        protected internal static void LoadingTpOnTester(string logsFolderPath, string singleScriptPath, SettingsFile.TestPlanInputs plans)
        {
            if (ProcessTosRelatedCommands(singleScriptPath, "isTPLoaded", logsFolderPath))
            {
                Handlers.LoggerHandler.PrintLine("Unloading previous TP.", Wrapper.PrintType.DEFAULT);
                ProcessTosRelatedCommands(singleScriptPath, "unloadTP", logsFolderPath);
            }

            var tpLoadList = $"loadTP ";
            if (PValMain.TosVerMajorRev >= 3 && PValMain.TosVerMinorRev >= 10)
            {
                // TOS 3.10+ added the directory as the 2nd argument.
                tpLoadList += $"{Directory.GetCurrentDirectory()} ";
            }

            tpLoadList += $"{plans.TplFile} {plans.StplFile} {plans.PlistFile} {plans.SocFile} {plans.EnvFile}";
            Handlers.LoggerHandler.PrintLine($"TP load cmd=[{tpLoadList}].", Wrapper.PrintType.DEFAULT);

            Handlers.LoggerHandler.PrintLine($"tpl file ={plans.TplFile}, stpl file ={plans.StplFile}, Plist file= {plans.PlistFile}, soc file ={plans.SocFile},env file = {plans.EnvFile}", Wrapper.PrintType.LOGGER_ONLY);
            ProcessTosRelatedCommands(singleScriptPath, tpLoadList, logsFolderPath);
            if (!ProcessTosRelatedCommands(singleScriptPath, "isTPLoaded", logsFolderPath))
            {
                Handlers.LoggerHandler.PrintLine("Failed to load TP.", Wrapper.PrintType.ERROR);
                TOSRestarter(logsFolderPath,true);
                Handlers.LoggerHandler.PrintLine("Tos Restarted and loading TP again...");
                ProcessTosRelatedCommands(singleScriptPath, tpLoadList, logsFolderPath);
                if (!ProcessTosRelatedCommands(singleScriptPath, "isTPLoaded", logsFolderPath))
                {
                    throw new Exception("Failed to load TP.");
                }
            }

            Handlers.LoggerHandler.PrintLine("TP loaded successfully.", Wrapper.PrintType.DEFAULT);
        }

        /// <summary>
        /// Process the Results from test unit command.
        /// </summary>
        /// <param name="singleScriptPath">Path of singlescriptcmd.exe.</param>
        /// <param name="listOfInstances">Instances executed during test.</param>
        /// <param name="planName">Name of the Plan/Tp run on tester.</param>
        /// <param name="testInstanceResultList">List of instances names along with their ports and status.</param>
        protected internal static void GetTestInstanceResults(
            string logsFolderPath,
            string singleScriptPath,
            List<string> listOfInstances,
            string planName,
            Dictionary<string, TestProgramResults> testInstanceResultList)
        {
            if (planName is null)
            {
                throw new ArgumentNullException(nameof(planName));
            }

            if (testInstanceResultList is null)
            {
                throw new ArgumentNullException(nameof(testInstanceResultList));
            }

            if (string.IsNullOrEmpty(singleScriptPath))
            {
                throw new ArgumentException($"'{nameof(singleScriptPath)}' cannot be null or empty", nameof(singleScriptPath));
            }

            if (listOfInstances is null)
            {
                throw new ArgumentNullException(nameof(listOfInstances));
            }

            if (listOfInstances.Count == 0)
            {
                return;
            }

            for (var i = 3; i < listOfInstances.Count; i++)
            {
                PValMain.InstancesPort = string.Empty;
                PValMain.InstancesStatus = string.Empty;
                var testInstanceResults = new TestProgramResults
                {
                    TestInstanceName = listOfInstances[i],
                    TpRunName = planName,
                };
                var testName = listOfInstances[i];
                ProcessTosRelatedCommands(singleScriptPath, "getTestInstanceResult" + " " + testName, logsFolderPath);
                testInstanceResults.TesterPort = PValMain.InstancesPort;
                testInstanceResults.TestInstanceStatus = PValMain.InstancesStatus;
                testInstanceResultList.Add(testName, testInstanceResults);
            }
        }
    }
}
