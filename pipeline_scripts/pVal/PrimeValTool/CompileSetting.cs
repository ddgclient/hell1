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
    using System.IO;
    using System.Linq;

    public class CompileSetting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompileSetting"/> class.
        /// This class contain all argument needed to call msbuild and compile solutions and projects.
        /// </summary>
        /// <param name="testPlan">TestPlan has path and names required to define the arguments.</param>
        public CompileSetting(SettingsFile.TestPlanInputs testPlan, string logsFolderPath)
            : this(
                   Path.GetDirectoryName(testPlan.TplFile),
                   Path.GetFullPath(Path.GetDirectoryName(testPlan.TplFile) + @"\UserCode"),
                   Path.Combine(logsFolderPath, "CompilationLogs"),
                   PValMain.GetReleaseOrDebug())
        {
            this.VerifySettings();
        }

        private CompileSetting(string testPlanPath, string libPath, string logPath, string debugOrRelease)
        {
            this.TestPlanPath = testPlanPath;
            this.LibPath = libPath;
            this.LogPath = logPath;
            this.Configuration = debugOrRelease;
            this.ToolsVersion = "Current";
            this.Platform = "x64";
            this.Target = "Restore;Build";
        }

        /// <summary>
        /// Test plant location path.
        /// </summary>
        private readonly string TestPlanPath;

        /// <summary>
        /// Gets and Sets SolutionsPaths directory.
        /// </summary>
        public Dictionary<string, bool> SolutionsPaths { get; private set; }

        /// <summary>
        /// Gets or sets ProjectsPaths.
        /// </summary>
        public Dictionary<string, bool> ProjectsPaths { get; set; }

        /// <summary>
        /// Gets LibPath directory name.
        /// </summary>
        public string LibPath { get; }

        /// <summary>
        /// Gets LogPath directory name.
        /// </summary>
        public string LogPath { get; }

        /// <summary>
        /// Gets ToolsVersion name.
        /// </summary>
        public string ToolsVersion { get; }

        /// <summary>
        /// Gets or set MsBuildTool path.
        /// </summary>
        public string MsBuildTool { get; private set; }

        /// <summary>
        /// Gets Configuration, release or debug.
        /// </summary>
        public string Configuration { get; }

        /// <summary>
        /// Gets Platform.
        /// </summary>
        public string Platform { get; }

        /// <summary>
        /// Gets Target.
        /// </summary>
        public string Target { get; }

        /// <summary>
        /// Gets TestPlan directory path.
        /// </summary>
        /// <returns>Return the path of current testPlan.</returns>
        public string GetTestPlanPath()
        {
            return this.TestPlanPath;
        }

        /// <summary>
        /// Gets Target.
        /// </summary>
        /// <param name="allFiles">List of string to filter, remove all string that contain UnitTest or unit_test.</param>
        /// <returns>List of values filtered.</returns>
        public static List<string> FilterFiles(List<string> allFiles)
        {
            if (allFiles is null)
            {
                throw new ArgumentNullException(nameof(allFiles));
            }

            return allFiles.Where(fileName => !fileName.Contains("UnitTest") && !fileName.Contains("unit_test")).ToList();
        }

        /// <summary>
        /// This method found all project without solution path to be compiling and check if exist the dll file respectively.
        /// </summary>
        /// <param name="pathToFindProjFiles">Path where is found the projects files.</param>
        /// <param name="listNamesOfAllDllFilesFound">List of all dll files to check with the manes of each project.</param>
        /// <returns>All projects without solution file.</returns>
        public Dictionary<string, bool> SearchProjectsPaths(string pathToFindProjFiles, List<string> listNamesOfAllDllFilesFound)
        {
            if (listNamesOfAllDllFilesFound is null)
            {
                return null;
            }

            var allProjFiles = Directory.GetFiles(pathToFindProjFiles, "*.csproj", SearchOption.AllDirectories).ToList();

            var projFiles = FilterFiles(allProjFiles);

            this.SolutionsPaths = SearchSolutionsPaths(pathToFindProjFiles);

            var dllNames = listNamesOfAllDllFilesFound.Aggregate(string.Empty, (current, nameOfDll) => current + (Path.GetFileName(nameOfDll) + " "));

            foreach (var key in this.SolutionsPaths.Keys.ToList())
            {
                string line;
                var file = new StreamReader(key);
                while ((line = file.ReadLine()) != null)
                {
                    if (!line.Contains("Project("))
                    {
                        continue;
                    }

                    for (var index = projFiles.Count - 1; index >= 0; --index)
                    {
                        var projName = Path.GetFileName(projFiles[index]);
                        if (!line.Contains(projName))
                        {
                            continue;
                        }

                        if (!dllNames.Contains(projName.Replace("csproj", "dll")))
                        {
                            this.SolutionsPaths[key] = true;
                        }

                        projFiles.RemoveAt(index);
                    }
                }
            }

            return projFiles.ToDictionary(proj => proj, proj => !dllNames.Contains(Path.GetFileName(proj).Replace("csproj", "dll")));
        }

        private static Dictionary<string, bool> SearchSolutionsPaths(string path)
        {
            var solutionsPaths = new Dictionary<string, bool>();
            var solutionFiles = Directory.GetFiles(path, "*.sln", SearchOption.AllDirectories);

            foreach (var solution in solutionFiles)
            {
                solutionsPaths.Add(solution, false);
            }

            return solutionsPaths;
        }

        private void VerifySettings()
        {
            const string msbuildEnt = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\msbuild.exe";
            const string msbuildPro = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\msbuild.exe";
            if (Directory.Exists(Path.GetDirectoryName(msbuildEnt)))
            {
                this.MsBuildTool = msbuildEnt;
            }
            else if (Directory.Exists(Path.GetDirectoryName(msbuildPro)))
            {
                this.MsBuildTool = msbuildPro;
            }
            else
            {
                var errorMessage = "PrimeValidation.exe Could not locate neither the VS2019";
                errorMessage += " Professional or 2019 Enterprise source bat file";
                errorMessage += $"\n\tPath not found: [{msbuildEnt}]";
                errorMessage += $"\n\tPath not found: [{msbuildPro}]";
                throw new Exception(errorMessage);
            }
        }
    }
}