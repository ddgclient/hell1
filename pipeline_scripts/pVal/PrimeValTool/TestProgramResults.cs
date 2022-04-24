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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Wrapper;

    /// <summary>
    /// This class process the test instances and their results for report generation.
    /// </summary>
    public class TestProgramResults
    {
        /// <summary>
        /// Gets or Sets name of Instance.
        /// </summary>
        public string TestInstanceName;

        /// <summary>
        /// Gets or Sets status of instance.
        /// </summary>
        public string TestInstanceStatus;

        /// <summary>
        /// Gets or sets port of instance.
        /// </summary>
        public List<string> TestInstancePort;

        /// <summary>
        /// Gets or sets Tpl name.
        /// </summary>
        public string TpRunName;

        /// <summary>
        /// Gets or sets module name.
        /// </summary>
        public string TpModuleName;

        /// <summary>
        /// Gets or sets Tester Port.
        /// </summary>
        public string TesterPort;

        /// <summary>
        /// Gets or sets the expected port.
        /// </summary>
        public List<string> ExpectedPort;

        /// <summary>
        /// Gets or sets the pass fail info of instance.
        /// </summary>
        public string PassFailStatus;

        /// <summary>
        /// Gets or sets ConsoleFail funcition.
        /// </summary>
        public string ConsoleFailInfo;

        /// <summary>
        /// Gets or sets failing instance name from console.
        /// </summary>
        public string ConsoleFailInstance;

        public TestProgramResults()
        {
            this.ExpectedPort = new List<string>();
            this.TestInstancePort = new List<string>();
        }

        /// <summary>
        /// This class creates an object for the tester results for report generation.
        /// </summary>
        /// <param name="testInstResultsList">Previously populated list of instances with names, ports and status.</param>
        /// <param name="folder">folder is the location of console.log file.</param>
        public static void GenerateTpResultStruct(Dictionary<string, TestProgramResults> testInstResultsList, string automationDirectory, string folder, ref List<TestProgramResults> finalTpResultsList)
        {
            var repeatedTest = new List<string>();

            // FIXME: Figure out what this is doing, then fix it... breaks unit tests when i comment it out...
            foreach (var instResult in testInstResultsList)
            {
                IdentifyPortsInTestName(instResult, repeatedTest);
            }

            const string consoleLogName = "console.log";
            var consoleLogPathName = $"{folder}\\{consoleLogName}";
            var consolePath = Path.Combine(automationDirectory, consoleLogPathName);

            GetPortsFromConsoleFile(testInstResultsList, consolePath, repeatedTest);

            foreach (var instResult in testInstResultsList)
            {
                var finalTpResultStruct = new TestProgramResults();
                var splitTestName = instResult.Value.TestInstanceName.Split(':');
                finalTpResultStruct.TpModuleName = splitTestName.Length > 1 ? splitTestName.First() : "N/A";
                finalTpResultStruct.TestInstanceName = splitTestName.Last();

                if (instResult.Value.ExpectedPort.Count == 1)
                {
                    instResult.Value.TestInstancePort.Add(instResult.Value.TesterPort.Replace(" ", string.Empty));
                }

                if (instResult.Value.TestInstanceStatus.ToUpper().Trim() == "BYPASS" || instResult.Value.TestInstanceStatus.ToUpper().Trim() == "PASSFAILUNDEFINED")
                {
                    finalTpResultStruct.TestInstanceStatus = instResult.Value.TestInstanceStatus.ToUpper().Trim();
                }

                finalTpResultStruct.TesterPort = instResult.Value.TesterPort;
                finalTpResultStruct.ExpectedPort = instResult.Value.ExpectedPort;
                finalTpResultStruct.TestInstancePort = instResult.Value.TestInstancePort;
                finalTpResultStruct.PassFailStatus = finalTpResultStruct.ValidateTestPorts();
                finalTpResultsList.Add(finalTpResultStruct);
            }
        }

        private static void GetPortsFromConsoleFile(Dictionary<string, TestProgramResults> testInstResultsList, string consolePath, List<string> repeatedTest)
        {
            if (!File.Exists(consolePath)) //FIXME: huh??????? you serious?
            {
                return;
            }

            using var reader = new StreamReader(consolePath);
            string line;
            var lastPort = string.Empty;
            while ((line = reader.ReadLine()) != null)
            {
                foreach (var test in repeatedTest)
                {
                    if (line.Contains($"Instance=[{test}] TestClass=[") &&
                        line.Contains("]::execute()") &&
                        line.Contains("xit on Port=["))
                    {
                        var indexOf = line.IndexOf("Port=[", StringComparison.Ordinal) + 6;
                        var lastIndexOf = line.LastIndexOf(']');
                        var portValue = line.Substring(indexOf, lastIndexOf - indexOf);
                        testInstResultsList[test].TestInstancePort.Add(portValue);
                        continue;
                    }

                    if (line.Contains("StopTest ") && line.Contains(test) && lastPort != string.Empty)
                    {
                        testInstResultsList[test].TestInstancePort.Add(lastPort);
                        continue;
                    }

                    if (!line.Contains("Exiting to port")) continue;

                    var splittedLine = line.Split(' ');
                    lastPort = splittedLine.Last().Replace(".", string.Empty);
                }
            }
        }

        /// <summary>
        /// Print out the cause for failure for each failing test instance.
        /// </summary>
        /// <param name="instResult"> Results of each test instance. </param>
        /// <param name="repeatedTest"> Not sure...</param>
        /// <remarks> This method doesn't look like it's actually printing out the cause of each failure, need to change that... also the logic is a little hard to follow </remarks>
        private static void IdentifyPortsInTestName(KeyValuePair<string, TestProgramResults> instResult, List<string> repeatedTest)
        {
            var portInfo = instResult.Value.TestInstanceName.Split('_').Last();
            verifyPortInfo:
            while (portInfo != string.Empty)
            {
                var portInfoChar = portInfo.ToCharArray();
                switch (char.ToUpper(portInfoChar[0]))
                {
                    case 'P':
                        if (GetValue(instResult, portInfoChar, ref portInfo))
                        {
                            goto verifyPortInfo;
                        }

                        goto default;
                    case 'F':
                        if (portInfo.Length > 4)
                        {
                            if (portInfo.Substring(0, 5) == "FNEG1" || portInfo.Substring(0, 5) == "FNeg1")
                            {
                                instResult.Value.ExpectedPort.Add("-1");
                                portInfo = portInfo.Remove(0, 5);
                                goto verifyPortInfo;
                            }

                            if (portInfo.Substring(0, 5) == "FNEG2" || portInfo.Substring(0, 5) == "FNeg2")
                            {
                                instResult.Value.ExpectedPort.Add("-2");
                                portInfo = portInfo.Remove(0, 5);
                                goto verifyPortInfo;
                            }
                        }

                        if (GetValue(instResult, portInfoChar, ref portInfo))
                        {
                            goto verifyPortInfo;
                        }

                        goto default;
                    case 'V':
                        if (portInfo.Length > 4)
                        {
                            if (portInfo.Substring(0, 5) == "VNEG1")
                            {
                                instResult.Value.ExpectedPort.Add("-1");
                                portInfo = portInfo.Remove(0, 5);
                                goto verifyPortInfo;
                            }

                            if (portInfo.Substring(0, 5) == "VNEG2")
                            {
                                instResult.Value.ExpectedPort.Add("-2");
                                portInfo = portInfo.Remove(0, 5);
                                goto verifyPortInfo;
                            }
                        }

                        goto default;

                    default:
                        if (instResult.Value.TestInstanceStatus.ToUpper().Trim() == "BYPASS" || instResult.Value.TestInstanceStatus.ToUpper().Trim() == "PASSFAILUNDEFINED")
                        {
                            Handlers.LoggerHandler.PrintLine(
                                $"Test Instance =[{instResult.Key}] doesn't follow the naming convention.",
                                PrintType.WARNING_LOGGER);
                            portInfo = string.Empty;
                            break;
                        }

                        Handlers.LoggerHandler.PrintLine(
                            $"Test Instance =[{instResult.Key}] doesn't follow the naming convention.",
                            PrintType.ERROR);
                        portInfo = string.Empty;
                        instResult.Value.PassFailStatus = "FAIL";
                        break;
                }
            }

            if (instResult.Value.ExpectedPort.Count <= 1)
            {
                return;
            }

            var portsString = instResult.Value.TestInstanceName.Split('_').Last();
            instResult.Value.TestInstanceName = instResult.Value.TestInstanceName.Replace(portsString, "***");
            repeatedTest.Add(instResult.Key);
        }

        private static bool GetValue(KeyValuePair<string, TestProgramResults> instResult, char[] portInfoChar, ref string portInfo)
        {
            try //FIXME: why is this failing...???
            {
                for (var i = 0; i < 10; ++i)
                {
                    // Logic here is that this is checking for the 
                    if (i == Convert.ToInt16(portInfoChar[1]) - 48) // FIXME: Failing here for some reason...
                    {
                        instResult.Value.ExpectedPort.Add(Convert.ToString(i));
                        portInfo = portInfo.Remove(0, 2);
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        /// Parses console logs for fail function information.
        /// </summary>
        /// <param name="planName">name of the tp/plan name.</param>
        /// <param name="path">Path of console logs to parse.</param>
        public static void ParseConsoleLog(string planName, string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var filename in Directory.EnumerateFiles(path, "*console.log"))
                {
                    if (path.Contains(planName))
                    {
                        PValMain.FinalConsolePath = filename;
                        var consoleLines = File.ReadAllLines(filename);
                        foreach (var line in consoleLines)
                        {
                            if (!line.Contains("Error in:"))
                            {
                                continue;
                            }

                            var consoleInstStruct = new TestProgramResults();
                            var lineElements = line.Split(' ');
                            foreach (var i in lineElements)
                            {
                                if (i.Contains("function="))
                                {
                                    consoleInstStruct.ConsoleFailInfo = i;
                                }
                                else if (i.Contains("instance"))
                                {
                                    consoleInstStruct.ConsoleFailInstance = i;
                                }
                            }

                            PValMain.ConsoleTpResultList.Add(consoleInstStruct);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a Csv file and result structure for html report.
        /// </summary>
        /// <param name="planName">Tp/Plan name.</param>
        /// <param name="csvReportPath">Path to create the csv file.</param>
        /// <param name="htmlPath">html file path.</param>
        /// <param name="tplReport">Report for Html format.</param>
        public static void GenerateCsvFile(string planName, string csvReportPath, string htmlPath, GenerateHtmlReports tplReport, ref List<TestProgramResults> finalTpResultsList)
        {
            var missingInstancesCounts = 0;
            var notExpectedPortCounts = 0;
            var passInstancesCount = 0;
            var failInstancesCount = 0;
            var byPassedInstancesCount = 0;
            var failInstances = new List<string>();
            var missingInstances = new List<string>();
            var notExpectedPort = new List<string>();
            using (var file = new StreamWriter(csvReportPath, true))
            {
                file.WriteLine("{0}_{1}:{2}", planName, PValMain.TosName, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                file.WriteLine("ModuleName" + "," + "InstanceName" + "," + "Testerport" + "," + "Expectedport" + "," + "PassFailstatus" + "," + "InstanceStatus");
                foreach (var tpResults in finalTpResultsList)
                {
                    if (tpResults.TestInstancePort[0] == "N/A" && tpResults.PassFailStatus.Contains("No Expected Port"))
                    {
                        ++notExpectedPortCounts;
                        notExpectedPort.Add(tpResults.TestInstanceName);
                        continue;
                    }

                    if (tpResults.TestInstancePort.Count > 0 &&
                        int.TryParse(tpResults.TestInstancePort[0], out int value) &&
                        value < -2)
                    {
                        tpResults.TestInstancePort = new List<string> { "N/A" };
                        tpResults.TesterPort = tpResults.TestInstancePort[0];
                        tpResults.PassFailStatus = "Not Executed";
                        missingInstancesCounts++;
                        missingInstances.Add(tpResults.TestInstanceName);
                    }
                    else
                    {
                        switch (tpResults.PassFailStatus)
                        {
                            case "PASS":
                                passInstancesCount++;
                                break;
                            case "FAIL":
                                failInstancesCount++;
                                failInstances.Add(string.Concat(tpResults.TestInstanceName, "_", tpResults.TesterPort));
                                break;
                            default:
                                break;
                        }
                    }

                    if (tpResults.TestInstanceStatus == "BYPASS")
                    {
                        byPassedInstancesCount++;
                    }

                    foreach (var consoleResult in PValMain.ConsoleTpResultList.Where(consoleResult => consoleResult.ConsoleFailInstance.Contains(tpResults.TestInstanceName)))
                    {
                        tpResults.ConsoleFailInfo = consoleResult.ConsoleFailInfo;
                    }

                    file.WriteLine(tpResults.TpModuleName + "," + tpResults.TestInstanceName + "," + tpResults.TesterPort + "," + tpResults.ExpectedPort + "," + tpResults.PassFailStatus + "," + tpResults.TestInstanceStatus);
                }
            }

            tplReport.TplName = planName;
            tplReport.TotalInstances = finalTpResultsList.Count;
            tplReport.MissingTests = missingInstancesCounts;
            tplReport.NotExpectedPort = notExpectedPortCounts;
            tplReport.BypassInstances = byPassedInstancesCount;
            tplReport.ActualExecutedTests = tplReport.TotalInstances - tplReport.MissingTests - tplReport.BypassInstances;
            tplReport.Reportfile =
                $"<a href = \"{htmlPath}\" target=\"_blank\"> Summary</a>,<a href = \"{PValMain.FinalConsolePath}\" target=\"_blank\" > Console</a>";
            tplReport.PassInstances = passInstancesCount - byPassedInstancesCount;
            tplReport.FailInstances = failInstancesCount;
            if (!PValMain.FailInstanceDict.ContainsKey(planName))
            {
                PValMain.FailInstanceDict.Add(planName, failInstances);
            }

            if (!PValMain.MissingInstanceDict.ContainsKey(planName))
            {
                PValMain.MissingInstanceDict.Add(planName, missingInstances);
            }

            if (!PValMain.NotExpectedPort.ContainsKey(planName))
            {
                PValMain.NotExpectedPort.Add(planName, notExpectedPort);
            }
        }

        /// <summary>
        /// Compares golden and generated ituff which is copied from TOS folder.
        /// </summary>
        /// <param name="tplReport">Report for Html format.</param>
        /// <param name="tosPath">Tos path for copying the ituff generated.</param>
        /// <param name="planInputs">Plant input used to get the ituff tokens and golden ituff path.</param>
        public static void CompareItuffResult(GenerateHtmlReports tplReport, string tosPath, string logsFolderPath, SettingsFile.TestPlanInputs planInputs) //FIXME: method is waaaaay too large; break it up
        {
            var ituffTokensToIgnore = new List<string>();
            Handlers.LoggerHandler.PrintLine("Ituff tokens to be ignored:", PrintType.LOGGER_SEPARATOR);
            foreach (var tokens in planInputs.ItuffTokensToIgnore)
            {
                ituffTokensToIgnore.Add(tokens);
                Handlers.LoggerHandler.PrintLine(tokens, PrintType.LOGGER_ONLY);
            }

            var tnamesContentToIgnore = new List<string>();
            Handlers.LoggerHandler.PrintLine("Ituff tnames content to be ignored:", PrintType.LOGGER_SEPARATOR);
            foreach (var tokens in planInputs.TnamesContentToIgnore)
            {
                tnamesContentToIgnore.Add(tokens);
                Handlers.LoggerHandler.PrintLine(tokens, PrintType.LOGGER_ONLY);
            }

            var destPath = string.Empty;
            Handlers.LoggerHandler.PrintLine("------------------------ITUFF_Compare------------------------", PrintType.LOGGER_ONLY);
            Handlers.LoggerHandler.PrintLine("Comparing ITUFF");
            var sourcePath = tosPath;
            var files = Directory.GetFiles(sourcePath);
            foreach (var s in files)
            {
                if (!s.Contains("1A") && Path.GetFileName(s) != "A_1")
                {
                    continue;
                }

                var destFileName = "ituff.txt";
                destPath = Path.Combine(logsFolderPath, destFileName);
                File.Copy(s, destPath, true);
                File.Delete(s);
            }

            var testerItuffPath = destPath;
            var goldenItuffPath = planInputs.GoldenItuffs;
            var isCompare = false;
            if (File.Exists(testerItuffPath) && File.Exists(goldenItuffPath))
            {
                if (File.Exists(goldenItuffPath))
                {
                    var lines1 = File.ReadAllLines(testerItuffPath);
                    lines1 = lines1.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                    var lines2 = File.ReadAllLines(goldenItuffPath);
                    lines2 = lines2.Where(y => !string.IsNullOrWhiteSpace(y)).ToArray();
                    if (lines1.Length == lines2.Length)
                    {
                        bool hasToCompareTnameContent = true;
                        for (var itr = 0; itr < lines1.Length; itr++)
                        {
                            if (!hasToCompareTnameContent)
                            {
                                hasToCompareTnameContent = true;
                            }
                            else
                            {
                                foreach (var token in tnamesContentToIgnore.Where(token =>
                                    lines1[itr].Contains(token) && lines2[itr].Contains(token) &&
                                    (lines1[itr].StartsWith("2_tname_") && lines2[itr].StartsWith("2_tname_") ||
                                     lines1[itr].StartsWith("0_tname_") && lines2[itr].StartsWith("0_tname_"))))
                                {
                                    hasToCompareTnameContent = false;
                                }

                                if (lines1[itr] == lines2[itr])
                                {
                                    continue;
                                }

                                isCompare = true;
                                foreach (var token in ituffTokensToIgnore.Where(token => lines1[itr].Contains(token) && lines2[itr].Contains(token)))
                                {
                                    isCompare = false;
                                }

                                if (!isCompare)
                                {
                                    continue;
                                }

                                // Handlers.LoggerHandler.PrintLine("Generated ituff lines vrs Golden ituff lines", Wrapper.PrintType.PVAL_LOGGER_ONLY);
                                Handlers.LoggerHandler.PrintLine($"*************************************************************************.", PrintType.DEFAULT);
                                Handlers.LoggerHandler.PrintLine($"ITUFF do not match. Line #{itr} is different.", PrintType.DEFAULT);
                                Handlers.LoggerHandler.PrintLine($"Actual: {lines1[itr]}", PrintType.DEFAULT);
                                Handlers.LoggerHandler.PrintLine($"Expect: {lines2[itr]}\n", PrintType.DEFAULT);
                                tplReport.Itufffails = "Do not match";
                                break;
                            }
                        }
                    }
                    else
                    {
                        Handlers.LoggerHandler.PrintLine($"*************************************************************************.", PrintType.DEFAULT);
                        Handlers.LoggerHandler.PrintLine($"ITUFF do not match - Different number of lines. ({lines1.Length} vs {lines2.Length})", PrintType.DEFAULT);
                        tplReport.Itufffails = "Do not match";
                        isCompare = true;
                        for (var itr = 0; itr < Math.Min(lines1.Length, lines2.Length); itr++)
                        {
                            if (lines1[itr] == lines2[itr])
                            {
                                continue;
                            }

                            Handlers.LoggerHandler.PrintLine($"Actual: {lines1[itr]}", PrintType.LOGGER_ONLY);
                            Handlers.LoggerHandler.PrintLine($"Expect: {lines2[itr]}\n", PrintType.LOGGER_ONLY);
                        }
                    }

                    if (isCompare)
                    {
                        return;
                    }

                    Handlers.LoggerHandler.PrintLine("ITUFF Matches", PrintType.LOGGER_SEPARATOR);
                    tplReport.Itufffails = "Matches";
                }
                else
                {
                    Handlers.LoggerHandler.PrintLine($"No Golden Ituff for plan/TP = {planInputs.TestPlanName}.", PrintType.WARNING);
                    tplReport.Itufffails = "No Golden Ituff.";
                }
            }
            else
            {
                Handlers.LoggerHandler.PrintLine($"No TOS ituff for plan/TP = {planInputs.TestPlanName}.", PrintType.WARNING);
                tplReport.Itufffails = "No TOS ituff.";
            }
        }

        private string ValidateTestPorts()
        {
            var doesPortsMatching = true;
            if (this.TestInstancePort.Count == 0)
            {
                this.TestInstancePort = new List<string> { "N/A" };
                this.ExpectedPort = new List<string> { "N/A" };
                return "No Expected Port";
            }

            if (this.ExpectedPort.Count == this.TestInstancePort.Count)
            {
                var failPortNumber = 0;
                var expectedPortCount = this.ExpectedPort.Count;
                for (var i = 0; expectedPortCount > i; ++i)
                {
                    if (this.ExpectedPort[i] == this.TestInstancePort[i])
                    {
                        continue;
                    }

                    ++failPortNumber;
                    doesPortsMatching = false;
                    Handlers.LoggerHandler.PrintLine(
                        expectedPortCount > 1
                            ? $"Test=[{this.TestInstanceName}] fail in iteration=[{i}].\n\tExpected=[{this.ExpectedPort[i]}] Actual=[{this.TestInstancePort[i]}]"
                            : $"Test=[{this.TestInstanceName}] fail.\n\tExpected=[{this.ExpectedPort[i]}] Actual=[{this.TestInstancePort[i]}]",
                        PrintType.WARNING_LOGGER);
                }

                if (expectedPortCount <= 1 || PValMain.IsFullConsoleRequired)
                {
                    return doesPortsMatching ? "PASS" : "FAIL";
                }

                this.ExpectedPort = new List<string> { $"Expected Iterations = {expectedPortCount}" };
                this.TestInstancePort = doesPortsMatching ? new List<string> { $"All match." } : new List<string> { $"Total Fails = {failPortNumber}" };
                return doesPortsMatching ? "PASS" : "FAIL";
            }

            this.ExpectedPort = new List<string> { $"Expected Iterations = {this.ExpectedPort.Count}" };
            this.TestInstancePort = new List<string> { $"Iterations = {this.TestInstancePort.Count}" };
            return "FAIL";
        }
    }
}
