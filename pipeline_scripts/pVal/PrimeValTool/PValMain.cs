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
    using System.Linq;
    using Wrapper;

    /// <summary>
    /// Main Class for Prime.
    /// </summary>
    public class PValMain
    {
        public static RuntimeCollaterals ToolCollaterals;

        /// <summary>
        /// Stores modified results of all the instances.
        /// </summary>
        private static List<TestProgramResults> FinalTpResultsList = new List<TestProgramResults>();

        /// <summary>
        /// Console log results (name of function failed along with test instance).
        /// </summary>
        public static List<TestProgramResults> ConsoleTpResultList = new List<TestProgramResults>();

        /// <summary>
        /// Dictionary for failing instances and corresponding Tp/Plan name.
        /// </summary>
        public static Dictionary<string, List<string>> FailInstanceDict = new Dictionary<string, List<string>>();

        /// <summary>
        /// Dictionary for Missing instances and corresponding Tp/Plan name.
        /// </summary>
        public static Dictionary<string, List<string>> MissingInstanceDict = new Dictionary<string, List<string>>();

        /// <summary>
        /// Dictionary for instances with not expected port and corresponding Tp/Plan name.
        /// </summary>
        public static Dictionary<string, List<string>> NotExpectedPort = new Dictionary<string, List<string>>();

        /// <summary>
        /// Report for all the Tp/plans ran on tester.
        /// </summary>
        public static List<GenerateHtmlReports> Tplreportlist = new List<GenerateHtmlReports>();

        /// <summary>
        /// List of instances ran on tester.
        /// </summary>
        public static List<string> TestInstancesFromTester;

        /// <summary>
        /// The first version of TOS to use during pVal startup.
        /// </summary>
        public static string MainTOSVersionToUse; //FIXME: we can use the default TOS provided by env var instead of being provided one in args...

        /// <summary>
        /// Mode of tos.
        /// </summary>
        public static string TosMode;

        /// <summary>
        /// Test instance port from tester.
        /// </summary>
        public static string InstancesPort;

        /// <summary>
        /// Pass/fail status of instance.
        /// </summary>
        public static string InstancesStatus;

        /// <summary>
        /// name of Tos (TOS34 or TOS36).
        /// </summary>
        public static string TosName;

        /// <summary>
        /// Console log path to add to report.
        /// </summary>
        public static string FinalConsolePath;

        /// <summary>
        /// boolean to force the user code compilation.
        /// </summary>
        private static bool IsCompileRequired;

        /// <summary>
        /// boolean to compile and run in debug mode.
        /// </summary>
        public static bool IsDebugModeRequired;

        /// <summary>
        /// boolean to get the logs files from HDMT.
        /// </summary>
        public static bool IsHdmtLogsRequired;

        /// <summary>
        /// boolean to get all console printing from pVal.
        /// </summary>
        public static bool IsFullConsoleRequired;

        /// <summary>
        /// boolean to define the needed to lauch the HTML files.
        /// </summary>
        public static bool IsSkipNeeded = false;

        /// <summary>
        /// boolean to set only the load test Program.
        /// </summary>
        public static bool IsOnlyLoadRequired;

        /// <summary>
        /// string to get Hdmt log path.
        /// </summary>
        public static string HdmtLogPath = @"C:\HDMT3\logs";

        /// <summary>
        /// Major TOS version (eg. 3)
        /// </summary>
        public static int TosVerMajorRev = 0;

        /// <summary>
        /// Minor TOS version (eg. 9 or 10 for tos3.9 or tos3.10
        /// </summary>
        public static int TosVerMinorRev = 0;

        /// <summary>
        /// Stores result of instances run on tester.
        /// </summary>
        private static readonly List<TestProgramResults> TestInstanceResultlist = new List<TestProgramResults>();

        /// <summary>
        /// Stores full plan list.
        /// </summary>
        private static List<SettingsFile.TestPlanInputs> pValRunPlanList = new List<SettingsFile.TestPlanInputs>();

        /// <summary>
        /// Stores Selected Plan List.
        /// </summary>
        private static List<SettingsFile.TestPlanInputs> pValSelectedPlanList = new List<SettingsFile.TestPlanInputs>();

        /// <summary>
        /// Gets return the string Debug or Release if is required.
        /// </summary>
        /// <returns>Return String "Debug" or "Release".</returns>
        public static string GetReleaseOrDebug()
        {
            return IsDebugModeRequired ? "Debug" : "Release";
        }

        /// <summary>
        /// Main program.
        /// </summary>
        /// <param name="args">User input args. First arg is the initial version of TOS to use. Second arg is whether </param>
        /// <returns>Returns 1 or 0.</returns>
        public static int Main(string[] args)
        {
            bool error = false;

            try
            {
                ToolCollaterals = RuntimeCollaterals.InitializeToolCollaterals();

                if (args.Length > 3)
                {
                    Handlers.LoggerHandler.PrintLine("Invalid number of arguments detected; number should be three or less.");
                    Handlers.LoggerHandler.PrintLine("First arg: Path to TOS release folder");
                    Handlers.LoggerHandler.PrintLine("Second arg(opt): Whether to execute whole test plan list");
                    Handlers.LoggerHandler.PrintLine("Third arg(opt): Codes for additional modes");
                    Handlers.LoggerHandler.PrintLine("Examples:");
                    Handlers.LoggerHandler.PrintLine("     ");
                    Handlers.LoggerHandler.PrintLine("     C:\\Intel\\hdmt\\hdmtOS_1.2.3.4_Release");
                    Handlers.LoggerHandler.PrintLine("     C:\\Intel\\hdmt\\hdmtOS_5.6.7.8_Release full");
                    Handlers.LoggerHandler.PrintLine("     C:\\Intel\\hdmt\\hdmtOS_9.10.11.12_Release full -cdz");
                    Console.ReadKey();
                    ConsoleExit();
                    return 1;
                }

                ParseMainArguments(ref args); // FIXME: determine TOS to execute here instead of later method...
                StopAllCurrentTOSInstances(); // FIXME: analyze if this needs to be called here. all params need to be good to go before deciding to call TOS...
                Handlers.LoggerHandler.InitializeLogFile(ToolCollaterals.TOSLogsPath); // FIXME: initialize all these log files an directories before execution
                GetTOSVersionForExecution(args);

                Handlers.LoggerHandler.PrintLine("====================================================================\n");

                SelectRunPlanForExecution(args);
                TOSControlCommander.SwitchToMainTOSVersion(ToolCollaterals);

                Handlers.LoggerHandler.PrintLine("Selected Plans:");
                foreach (var planNames in pValSelectedPlanList)
                {
                    Handlers.LoggerHandler.PrintLine(planNames.TestPlanName);
                }

                Handlers.LoggerHandler.PrintLine("====================================================================");
                Handlers.LoggerHandler.PrintLine("Verifying inputs.");
                System.Threading.Thread.Sleep(5000); // why sleep for 5 seconds?

                // whenever we switch TOS, the env var should change as well... the logic in RuntimeCollats should cover this.
                var singleScriptCmdPath = ToolCollaterals.SingleScriptForCurrentTOS;
                var watch = new Stopwatch();

                foreach (var plans in pValSelectedPlanList)
                {
                    plans.FixPaths(); // Once we know the TOS we can decide how to format the files for loading the testprogram.

                    var planLogsFolderPath = Path.Combine(ToolCollaterals.FinalReportLogsPath, TosName);
                    planLogsFolderPath = Path.Combine(planLogsFolderPath, plans.TestPlanName);
                    planLogsFolderPath = Path.Combine(planLogsFolderPath, GetReleaseOrDebug());
                    TOSControlCommander.TOSRestarter(planLogsFolderPath, true);

                    if (Directory.Exists(planLogsFolderPath))
                    {
                        Directory.Delete(planLogsFolderPath, recursive: true);
                    }

                    if (IsCompileRequired)
                    {
                        UserCodeCompiler.CheckSlnInTp(planLogsFolderPath, plans);
                    }

                    var testPlanReport = new GenerateHtmlReports();
                    watch.Start();
                    TOSControlCommander.StartConsolidatedLogging(planLogsFolderPath, singleScriptCmdPath, RuntimeCollaterals.AutomationDirectory, planLogsFolderPath);
                    Handlers.LoggerHandler.PrintLine($"Loading TP for {plans.TestPlanName} on {TosName}.", PrintType.DEFAULT);
                    TOSControlCommander.LoadingTpOnTester(planLogsFolderPath, singleScriptCmdPath, plans);
                    TOSControlCommander.ProcessTosRelatedCommands(singleScriptCmdPath, "getTosStatus", planLogsFolderPath);

                    if (TosMode != null && TosMode.Contains("OfflineLSM"))
                    {
                        if (string.IsNullOrEmpty(plans.QuickSimFile))
                        {
                            throw new InvalidOperationException($"No QuickSim file was found with name [{plans.QuickSimFile}] defined by current plan.");
                        }

                        Handlers.LoggerHandler.PrintLine($"loading QuickSim for offline val for {TosName},{plans.TestPlanName}", PrintType.DEFAULT);
                        TOSControlCommander.ProcessTosRelatedCommands(
                            singleScriptCmdPath,
                            "LoadQuickSimResponseFile" + " " + plans.QuickSimFile,
                            planLogsFolderPath);
                    }

                    var testResultList = new Dictionary<string, TestProgramResults>();
                    if (!IsOnlyLoadRequired)
                    {
                        testResultList = RunTestProgram(plans, planLogsFolderPath, singleScriptCmdPath, watch);
                    }

                    Handlers.LoggerHandler.PrintLine($"Stopping {TosName} for {plans.TestPlanName}", PrintType.DEFAULT);
                    TOSControlCommander.ProcessTosRelatedCommands(singleScriptCmdPath, "stopConsolidatedLogging", planLogsFolderPath);
                    /* Handlers.LoggerHandler.PrintLine($"Unloading {TosName} for {plans.TestPlanName}", PrintType.DEFAULT);
                    TOSControlCommander.ProcessTosRelatedCommands(singleScriptCmdPath, "unloadTP", planLogsFolderPath); */
                    TOSControlCommander.ProcessTosRelatedCommands(ToolCollaterals.TOSControlPathForCurrentTos, "stoptos", planLogsFolderPath);
                    System.Threading.Thread.Sleep(5000);

                    if (!IsOnlyLoadRequired)
                    {
                        ProcessTestResultList(testResultList, plans, planLogsFolderPath, RuntimeCollaterals.AutomationDirectory, testPlanReport, watch, ref FinalTpResultsList);
                    }

                    watch.Reset();

                    if (IsHdmtLogsRequired)
                    {
                        Dictionary<string, string> temp = new Dictionary<string, string>();
                        temp.Add("HdmtApp.log", HdmtLogPath);
                        temp.Add("HdmtApiRecorded.py", HdmtLogPath);
                        temp.Add("hdmtOScommon.log", Path.Combine(HdmtLogPath, "commonhdmt"));

                        foreach (var file in temp)
                        {
                            Handlers.LoggerHandler.PrintLine($"Copying file from [{Path.Combine(file.Value, file.Key)}] to [{Path.Combine(planLogsFolderPath, file.Key)}]]", PrintType.DEBUG);
                            File.Copy(Path.Combine(file.Value, file.Key), Path.Combine(planLogsFolderPath, file.Key));
                        }
                    }
                }

                if (!IsOnlyLoadRequired)
                {
                    GenerateReports(ToolCollaterals.FinalReportLogsPath);
                }

                // FIXME: Change how the program fails. Just detect failing instances and report them...
                foreach (var entry in FailInstanceDict.Where(entry => entry.Value.Count > 0))
                {
                    Handlers.LoggerHandler.PrintLine($"Tests failed = {entry.Value.Count} in TP = {entry.Key}.", PrintType.REDLINE_ERROR);
                    error = true;
                }

                foreach (var entry in MissingInstanceDict.Where(entry => entry.Value.Count > 0))
                {
                    Handlers.LoggerHandler.PrintLine($"Tests Missing = {entry.Value.Count} in TP = {entry.Key}.", PrintType.ERROR);
                    error = true;
                }

                foreach (var entry in NotExpectedPort.Where(entry => entry.Value.Count > 0))
                {
                    Handlers.LoggerHandler.PrintLine($"Tests without expected port = {entry.Value.Count} in TP = {entry.Key}.", PrintType.ERROR);
                    error = true;
                }

                foreach (var plan in Tplreportlist)
                {
                    if (plan.Itufffails.ToUpper().Contains("DO NOT MATCH"))
                    {
                        Handlers.LoggerHandler.PrintLine($"ituff failed for plan/TP = {plan.TplName}.", PrintType.REDLINE_ERROR);
                        error = true;
                    }

                    if (plan.Itufffails.ToUpper().Contains("No Golden Ituff.") || plan.Itufffails.ToUpper().Contains("No TOS ituff."))
                    {
                        Handlers.LoggerHandler.PrintLine($"{plan.Itufffails} file is not found for plan/TP = {plan.TplName}.", PrintType.ERROR);
                        error = true;
                    }
                }
            }
            catch (Exception e)
            {
                Handlers.LoggerHandler.PrintLine("\nPrimeValidaton.exe was stopped by an error during execution.", PrintType.REDLINE_ERROR);
                Handlers.LoggerHandler.PrintLine(e.Message, PrintType.ERROR);
                Handlers.LoggerHandler.PrintLine($"Source:\n\t{e.Source}", PrintType.DEBUG);
                Handlers.LoggerHandler.PrintLine($"StackTrace:\n\t{e.StackTrace}", PrintType.DEBUG);
                Console.ReadKey();
                return 1;
            }

            if (error)
            {
                Handlers.LoggerHandler.PrintLine("Failing tests detected during test plan execution.", PrintType.REDLINE_ERROR);
                Handlers.LoggerHandler.PrintLine("\n");
                return 1;
            }

            Handlers.LoggerHandler.PrintLine("All tests passed.", PrintType.GREEN_CONSOLE);
            Handlers.LoggerHandler.PrintLine("\n");
            return 0;
        }

        private static void SelectRunPlanForExecution(string[] args)
        {
            pValRunPlanList = ToolCollaterals.SettingsFile.ReturnTestPlanMatchingWithTOSName(TosName);

            if (pValRunPlanList.Count == 0)
            {
                Handlers.LoggerHandler.PrintLine("No test plans to run are found.", PrintType.ERROR);
                ConsoleExit(); //FIXME: make this throw an exception and not exit gracefully, we shouldn't be executing if no plans were found matching to desired ver.
            }

            if (args.Length == 2) 
            {
                if (args[1].Trim(' ').ToUpper() == "FULL") // FIXME: this arg shouldn't force the codes to be in the third position...
                {
                    pValSelectedPlanList = pValRunPlanList;
                    Handlers.LoggerHandler.PrintLine("All Plans are selected.");
                }
                else
                {
                    Handlers.LoggerHandler.PrintLine($"Invalid argument = {args[1]} \n accepted argument FULL/full", PrintType.ERROR_CONSOLE);
                }
            }
            else
            {
                //FIXME: forced non-silent use when not executing all plans in a runplan (when FULL is not provided as arg)
                ToolCollaterals.SettingsFile.SelectTplPlansToRun(pValRunPlanList, pValSelectedPlanList);
            }

            if (pValSelectedPlanList.Count == 0)
            {
                Handlers.LoggerHandler.PrintLine("No plans were selected.", PrintType.ERROR_CONSOLE);
                ConsoleExit(); //FIXME: make this throw an exception, we shouldn't be executing if no plans were found matching to desired ver.
            }

            return;
        }

        /// <summary>
        /// Restart all current TOS instances
        /// </summary>
        /// <param name="toolCollaterals"></param>
        private static void StopAllCurrentTOSInstances()
        {
            try
            {
                TOSControlCommander.ProcessTosRelatedCommands(ToolCollaterals.SingleScriptForCurrentTOS, "stopConsolidatedLogging", ToolCollaterals.TOSLogsPath);
            }
            catch
            {
                // FIXME: must be a better way of doing this w/o the try-catch block...
                MainTOSVersionToUse = ToolCollaterals.CurrentTOSReleaseDirectory;
                TOSControlCommander.TOSRestarter(ToolCollaterals.TOSLogsPath, true);
                TOSControlCommander.ProcessTosRelatedCommands(ToolCollaterals.SingleScriptForCurrentTOS, "stopConsolidatedLogging", ToolCollaterals.TOSLogsPath);
            }

            return;
        }

        private static void GenerateReports(string mainLogsFolderPath)
        {
            GenerateHtmlReports.GenerateLoadedTpsReport(mainLogsFolderPath);
            Handlers.LoggerHandler.PrintLine("Validation run successfully.\n", PrintType.DEFAULT);
            if (Directory.Exists(Path.Combine(mainLogsFolderPath, TosName)) && !IsSkipNeeded)
            {
                foreach (string filename in Directory.EnumerateFiles(Path.Combine(mainLogsFolderPath, TosName), "*.HTML"))
                {
                    Handlers.LoggerHandler.PrintLine($"Launch {filename}.", PrintType.DEBUG);
                    Process.Start(@"cmd.exe ", @$"/c {filename}");
                    // Process.Start(filename);
                }
            }

            return;
        }

        private static void ProcessTestResultList(Dictionary<string, TestProgramResults> testResultList,
                                                  SettingsFile.TestPlanInputs plans,
                                                  string logsFolderPath,
                                                  string automationDirectory,
                                                  GenerateHtmlReports testPlanReport,
                                                  Stopwatch watch,
                                                  ref List<TestProgramResults> finalTpResultsList)
        {
            TestProgramResults.GenerateTpResultStruct(testResultList, automationDirectory, logsFolderPath, ref finalTpResultsList);
            Handlers.LoggerHandler.PrintLine($"Generating Results for {TosName}_{plans.TestPlanName}", PrintType.DEFAULT);
            TestProgramResults.ParseConsoleLog(plans.TestPlanName, logsFolderPath);
            string resultCsvFile = Path.Combine(logsFolderPath, "Results.csv");
            string resultsHtmlFile = Path.Combine(logsFolderPath, "Results.HTML");
            TestProgramResults.GenerateCsvFile(plans.TestPlanName, resultCsvFile, resultsHtmlFile, testPlanReport, ref finalTpResultsList);
            GenerateHtmlReports.GenerateTpBasedReport(plans.TestPlanName, logsFolderPath, ref finalTpResultsList);
            TestProgramResults.CompareItuffResult(testPlanReport, MainTOSVersionToUse, logsFolderPath, plans);
            testPlanReport.TotalRuntime = watch.Elapsed.Duration().ToString("g");
            Tplreportlist.Add(testPlanReport);
            TestInstanceResultlist.Clear();
            FinalTpResultsList.Clear();

            return;
        }

        /// <summary>
        /// Calls TOS to run the TP loaded onto the tester.
        /// </summary>
        /// <param name="plans"> Test plan to run for current loaded TP. </param>
        /// <param name="singleScriptCmdPath"> Path to the TOS command line. </param>
        /// <param name="watch"> Stopwatch keeping track of time for TP to execute. </param>
        /// <returns> Results of TP execution. </returns>
        private static Dictionary<string, TestProgramResults> RunTestProgram(SettingsFile.TestPlanInputs plans, string logsFolderPath, string singleScriptCmdPath, Stopwatch watch)
        {
            Handlers.LoggerHandler.PrintLine($"Running {plans.TestPlanName}.");
            TOSControlCommander.ProcessTosRelatedCommands(singleScriptCmdPath, "init", logsFolderPath); // FIXME: only execute init and test unit..
            TOSControlCommander.ProcessTosRelatedCommands(singleScriptCmdPath, "startLot", logsFolderPath);
            TOSControlCommander.ProcessTosRelatedCommands(singleScriptCmdPath, "testUnit", logsFolderPath);
            TOSControlCommander.ProcessTosRelatedCommands(singleScriptCmdPath, "endLot", logsFolderPath);

            watch.Stop();
            TOSControlCommander.ProcessTosRelatedCommands(singleScriptCmdPath, "getAllTestInstances", logsFolderPath);
            Handlers.LoggerHandler.PrintLine("Printing all testInstances loaded on tester.", PrintType.LOGGER_ONLY);
            foreach (var instName in TestInstancesFromTester)
            {
                Handlers.LoggerHandler.PrintLine(instName, PrintType.LOGGER_ONLY);
            }

            var testResultList = new Dictionary<string, TestProgramResults>();
            TOSControlCommander.GetTestInstanceResults(logsFolderPath, singleScriptCmdPath, TestInstancesFromTester, plans.TestPlanName, testResultList);
            if (testResultList.Count == 0)
            {
                TOSControlCommander.TOSRestarter(logsFolderPath, true);
                throw new Exception($"pVal didn't get any result from tester executing TestPlan=[{plans.TestPlanName}].");
            }

            TestInstancesFromTester.Clear();
            return testResultList;
        }

        public static void GetTOSVersionForExecution(string[] args)
        {
            var availableTosVersions = Handlers.DirectoryHandler.RetrieveListOfDirectories(ToolCollaterals.AllTOSReleasesDirectory, "hdmtOS*");
            string tosToUse = string.Empty;

            if (availableTosVersions.Length == 0)
            {
                throw new FileNotFoundException($"No installed TOS versions were found in directory [{ToolCollaterals.AllTOSReleasesDirectory}].");
            }

            if (args.Length == 0)
            {
                List<string> formattedTosList = new List<string>();

                foreach (var availTos in availableTosVersions)
                {
                    formattedTosList.Add(string.Concat(Array.IndexOf(availableTosVersions, availTos), ":", " ", availTos));
                }

                Handlers.LoggerHandler.PrintLine("Available Tos Versions:");
                foreach (var tos in formattedTosList)
                {
                    Handlers.LoggerHandler.PrintLine(tos, PrintType.CYAN_CONSOLE);
                }

                Handlers.LoggerHandler.PrintLine("Choose a TOS version and press enter.");
                bool isTosFound = false;
                while (!isTosFound)
                {
                    isTosFound = ParseUserInputForTosVersion(formattedTosList, Handlers.InputHandler.GetUserInput(), out tosToUse);
                }
            }
            else
            {
                bool isTOSVersionFound = MatchMainArgsToAvailableVersions(args[0], availableTosVersions, out tosToUse);

                if (!isTOSVersionFound)
                {
                    throw new ArgumentException($"No TOS version found that matches to the argument [{args[0]}] passed into program.");
                }
            }

            if (string.IsNullOrWhiteSpace(tosToUse))
            {
                throw new InvalidOperationException("Unexpected error when attempting to find a TOS version to execute.");
            }

            MainTOSVersionToUse = tosToUse;
            Handlers.LoggerHandler.PrintLine($"Selected Tos version = {MainTOSVersionToUse}");
            ParseTosName(availableTosVersions);

            return;
        }

        /// <summary>
        /// what is this doing...?
        /// </summary>
        /// <param name="availableTosVersions"></param>
        public static void ParseTosName(string[] availableTosVersions)
        {
            try
            {
                TosName = MainTOSVersionToUse.Split('_')[1];
                var tosVersionNumbers = TosName.Split('.');
                TosVerMajorRev = int.Parse(tosVersionNumbers[0]);
                TosVerMinorRev = int.Parse(tosVersionNumbers[1]);
                TosName = string.Concat("TOS", TosName.Replace(".", string.Empty));
                Handlers.LoggerHandler.PrintLine($"TosName={TosName}. Major=[{TosVerMajorRev}] Minor=[{TosVerMinorRev}]", PrintType.DEFAULT);

            }
            catch
            {
                Handlers.LoggerHandler.PrintLine("TOS release name abnormality; does not follow expected convention. Using full name instead...", PrintType.WARNING);
                TosName = MainTOSVersionToUse;
            }

            return;
        }

        /// <summary>
        /// Parses given arguments for a TOS version.
        /// </summary>
        /// <param name="desiredTOS">Argument passed by user as to what TOS version to use for startup.</param>
        /// <param name="availableTosVer">List of installed TOS versions available on current machine.</param>
        /// <returns>True/False</returns>
        public static bool MatchMainArgsToAvailableVersions(string desiredTOS, string[] availableTosVer, out string matchingTOS)
        {
            matchingTOS = null;

            foreach (var tos in availableTosVer)
            {
                if (desiredTOS.Trim(' ').ToUpper() == tos.ToUpper())
                {
                    matchingTOS = tos;
                }
            }

            if (string.IsNullOrEmpty(matchingTOS))
            {
                Handlers.LoggerHandler.PrintLine($"Given TOS version = {desiredTOS} is not installed.\n");
                return false;
            }

            Handlers.LoggerHandler.PrintLine($"Selected TOS version to run = {matchingTOS}.\n", PrintType.DEFAULT);

            return true;
        }

        /// <summary>
        /// Reads user input for a given TOS version to run for TP run; recursively calls if user input does not match any versions available on machine.
        /// </summary>
        /// <param name="tosVerList">Formatted tos ver list parsed previously which is displayed during exe.</param>
        private static bool ParseUserInputForTosVersion(List<string> tosVerList, string readKey, out string tosVersion)
        {
            tosVersion = string.Empty;

            if (!string.IsNullOrEmpty(readKey))
            {
                foreach (var tos in tosVerList)
                {
                    if (readKey.Trim(' ') == tos.Split(' ').First().Trim(':'))
                    {
                        tosVersion = tos.Split(' ')[1];
                        return true;
                    }
                }

                Handlers.LoggerHandler.PrintLine("Selected number does not match those provided in the list above. Please select again.", PrintType.DEFAULT);
            }
            else
            {
                Handlers.LoggerHandler.PrintLine("Input was empty; please select from above TOS versions.", PrintType.DEFAULT);
            }

            return false;
        }

        /// <summary>
        /// Parse arguments passed to main for program runtime settings.
        /// </summary>
        /// <param name="args"> Arguments passed to main for execution. </param>
        private static void ParseMainArguments(ref string[] args)
        {
            var argsFiltered = new List<string>();
            foreach (var arg in args)
            {
                if (arg.Contains("-"))
                {
                    if (arg.ToUpper().Contains("L"))
                    {
                        IsOnlyLoadRequired = true;
                    }

                    if (arg.ToUpper().Contains("C"))
                    {
                        IsCompileRequired = true;
                    }

                    if (arg.ToUpper().Contains("D"))
                    {
                        IsDebugModeRequired = true;
                    }

                    if (arg.ToUpper().Contains("Z"))
                    {
                        IsHdmtLogsRequired = true;
                    }

                    if (arg.ToUpper().Contains("K"))
                    {
                        IsFullConsoleRequired = true;
                    }

                    if (arg.ToUpper().Contains("S"))
                    {
                        IsSkipNeeded = true;
                    }
                }
                else
                {
                    argsFiltered.Add(arg);
                }
            }

            args = argsFiltered.ToArray();
        }

        /// <summary>
        /// Exits out of console based on user input.
        /// </summary>
        private static void ConsoleExit()
        {
            Handlers.LoggerHandler.PrintLine("Press a Key to exit.");
            if (!string.IsNullOrEmpty(Console.ReadKey().ToString()))
            {
                Environment.Exit(0);
            }
        }
    }
}
