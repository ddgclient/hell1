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
    using System.Security;
    using Wrapper;

    /// <summary>
    /// CompileTool read solution file and compile each project.
    /// </summary>
    public static class UserCodeCompiler
    {
        public static void CheckSlnInTp(string logsFolderPath, SettingsFile.TestPlanInputs testPlan)
        {
            if (CompileSln(logsFolderPath, testPlan))
            {
                throw new Exception("PVal tool could not compile the solutions in selected test plan");
            }
        }

        private static bool CompileSln(
            string logsFolderPath,
            SettingsFile.TestPlanInputs testPlan)
        {
            var compileSettings = new CompileSetting(testPlan, logsFolderPath);

            var dllList = SearchUserCodeDlls(compileSettings);

            compileSettings.ProjectsPaths = compileSettings.SearchProjectsPaths(compileSettings.GetTestPlanPath(), dllList);

            return CompileProjects(compileSettings);
        }

        private static bool CompileProjects(CompileSetting setting)
        {
            if (setting.SolutionsPaths == null)
            {
                return false;
            }

            var error = false;

            var filesToCompile = setting.SolutionsPaths;
            foreach (var addProjectsPath in setting.ProjectsPaths)
            {
                filesToCompile.Add(addProjectsPath.Key, addProjectsPath.Value);
            }

            foreach (var solutionsPath in setting.SolutionsPaths)
            {
                if (solutionsPath.Value)
                {
                    try
                    {
                        var fileName = Path.GetFileName(solutionsPath.Key);
                        if (fileName.Contains("sln"))
                        {
                            fileName = fileName.Replace("sln", "log");
                        }
                        else if (fileName.Contains("csproj"))
                        {
                            Handlers.LoggerHandler.PrintLine($"Project file is not defined in any solution for this plan/TP.\n\tFile = {solutionsPath.Key}", Wrapper.PrintType.WARNING);
                            continue;
                        }
                        else
                        {
                            throw new Exception($"pVal try to compile unknown file [{fileName}].");
                        }

                        var loggerFile = Path.Combine(setting.LogPath, fileName);
                        Directory.CreateDirectory(setting.LogPath);
                        var logger = new StreamWriter(loggerFile);
                        var msBuildProcess = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = setting.MsBuildTool,
                                Arguments = $"{solutionsPath.Key} -p:Configuration={setting.Configuration} -p:Platform={setting.Platform} -toolsVersion:{setting.ToolsVersion} /t:{setting.Target}",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true,
                            },
                        };

                        msBuildProcess.Start();
                        var isCompilationSuccess = false;
                        while (!msBuildProcess.StandardOutput.EndOfStream)
                        {
                            var line = msBuildProcess.StandardOutput.ReadLine();
                            logger.WriteLine(line);
                            if (line.Contains("Build succeeded"))
                            {
                                isCompilationSuccess = true;
                            }
                        }

                        if (msBuildProcess.ExitCode == 0 && isCompilationSuccess)
                        {
                            Handlers.LoggerHandler.PrintLine($"{Path.GetFileName(solutionsPath.Key)}: Build succeeded.", Wrapper.PrintType.GREEN_CONSOLE);
                        }
                        else
                        {
                            Handlers.LoggerHandler.PrintLine($"{Path.GetFileName(solutionsPath.Key)}: Build FAILED.", Wrapper.PrintType.REDLINE_ERROR);
                            Handlers.LoggerHandler.PrintLine($"\t- log file  =>  {Path.GetFullPath(loggerFile)}", Wrapper.PrintType.CYAN_CONSOLE);
                            error = true;
                        }

                        logger.Close();
                        msBuildProcess.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        if
                        (
                            ex is UnauthorizedAccessException
                            || ex is ArgumentNullException
                            || ex is PathTooLongException
                            || ex is DirectoryNotFoundException
                            || ex is NotSupportedException
                            || ex is ArgumentException
                            || ex is SecurityException
                            || ex is IOException)
                        {
                            Handlers.LoggerHandler.PrintLine("\tFailed to compile solutions.", Wrapper.PrintType.REDLINE_ERROR);
                            Handlers.LoggerHandler.PrintLine("\tMessage:");
                            Handlers.LoggerHandler.PrintLine($"\t\t{ex.Message}", Wrapper.PrintType.CYAN_CONSOLE);
                            Handlers.LoggerHandler.PrintLine($"Source:\n\t\t{ex.Source}", Wrapper.PrintType.DEBUG);
                            Handlers.LoggerHandler.PrintLine($"StackTrace:\n\t\t{ex.StackTrace}", Wrapper.PrintType.DEBUG);
                            Handlers.LoggerHandler.PrintLine($"HelpLink:\n\t\t{ex.HelpLink}", Wrapper.PrintType.DEBUG);
                            error = true;
                        }
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            return error;
        }

        private static List<string> SearchUserCodeDlls(CompileSetting compileSetting)
        {
            if (!Directory.Exists(compileSetting.LibPath))
            {
                return null;
            }

            var directories = Directory.GetDirectories(
                compileSetting.LibPath,
                PValMain.GetReleaseOrDebug(),
                SearchOption.AllDirectories);
            var foldersWithDlls = CompileSetting.FilterFiles(directories.ToList());

            var files = new List<string>();
            foreach (var allFiles in foldersWithDlls.Select(folder => Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories)))
            {
                files.AddRange(CompileSetting.FilterFiles(allFiles.ToList()));
            }

            return files;
        }
    }
}