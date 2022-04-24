
namespace PrimeValTool
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Wrapper;

    public class SettingsFile
    {
        /// <summary>
        /// Gets or sets class that represents pValRunList Json input file structure.
        /// </summary>
        [JsonProperty("pValRunList")]
        public List<TosTestPlanList> PValRunList { get; set; }

        /// <summary>
        /// Gets or sets class that represents TOSTestPlanList Json input file structure.
        /// </summary>
        public class TosTestPlanList
        {
            /// <summary>
            /// Gets or sets TOSVersion.
            /// </summary>
            [JsonProperty("TOSVersion")]
            public string TOSVersion { get; set; }

            /// <summary>
            /// Gets or sets area to log files to for this plan list. Optional.
            /// </summary>
            [JsonProperty("LogArea")]
            public string LogArea { get; set; }

            /// <summary>
            /// Gets or sets TestPlan.
            /// </summary>
            [JsonProperty("TestPlan")]
            public List<TestPlanInputs> TestPlan { get; set; }
        }

        public class EnvFiles
        {
            /// <summary>
            /// Gets or sets release Environment file.
            /// </summary>
            [JsonProperty("Release")]
            public string Release { get; set; }

            /// <summary>
            /// Gets or sets Debug Environment file.
            /// </summary>
            [JsonProperty("Debug")]
            public string Debug { get; set; }
        }

        /// <summary>
        /// Gets or sets class that represents TestPlanInputs Json input file structure.
        /// </summary>
        public class TestPlanInputs
        {
            /// <summary>
            /// Gets or sets TestPlanName.
            /// </summary>
            [JsonProperty("TestPlanName")]
            public string TestPlanName { get; set; }

            /// <summary>
            /// Gets or sets tplFile to store the tpl file path of the test program.
            /// </summary>
            [JsonProperty("TplFile")]
            public string TplFile { get; set; }

            /// <summary>
            /// Gets or sets stplFile to store the stpl file path of the test program.
            /// </summary>
            [JsonProperty("StplFile")]
            public string StplFile { get; set; }

            /// <summary>
            /// Gets or sets PlistFile.
            /// </summary>
            [JsonProperty("PlistFile")]
            public string PlistFile { get; set; }

            /// <summary>
            /// Gets or sets socFile to store the soc file path of the test program.
            /// </summary>
            [JsonProperty("SocFile")]
            public string SocFile { get; set; }

            /// <summary>
            /// Gets or sets EnvFile.
            /// </summary>
            [JsonProperty("EnvFiles")]
            public EnvFiles EnvFiles { get; set; }

            /// <summary>
            /// Gets or sets GoldenItuffs.
            /// </summary>
            [JsonProperty("GoldenItuffs")]
            public string GoldenItuffs { get; set; }

            /// <summary>
            /// Gets or sets QuickSimFile.
            /// </summary>
            [JsonProperty("QuickSimFile")]
            public string QuickSimFile { get; set; }

            /// <summary>
            /// Gets or sets ItuffTokens.
            /// </summary>
            [JsonProperty("ItuffTokensToIgnore")]
            public List<string> ItuffTokensToIgnore { get; set; }

            /// <summary>
            /// Gets or sets ItuffTokens.
            /// </summary>
            [JsonProperty("TnamesContentToIgnore")]
            public List<string> TnamesContentToIgnore { get; set; } = new List<string>();

            /// <summary>
            /// Gets or sets envFile.
            /// </summary>
            public string EnvFile { get; set; }

            internal void FixPaths()
            {
                this.QuickSimFile = ConvertToAbsolute(this.QuickSimFile, false);
                this.GoldenItuffs = ConvertToAbsolute(this.GoldenItuffs, false);
                if (PValMain.TosVerMajorRev < 3 || PValMain.TosVerMinorRev < 10)
                {
                    // TOS 3.10+ uses relative paths for the testprogram files.
                    this.TplFile = ConvertToAbsolute(this.TplFile);
                    this.StplFile = ConvertToAbsolute(this.StplFile);
                    this.SocFile = ConvertToAbsolute(this.SocFile);
                    this.PlistFile = ConvertToAbsolute(this.PlistFile);
                    this.EnvFiles.Release = ConvertToAbsolute(this.EnvFiles.Release, false);
                    this.EnvFiles.Debug = ConvertToAbsolute(this.EnvFiles.Debug, false);
                    this.EnvFile = ConvertToAbsolute(this.EnvFile, false);
                }
            }
        }

        /// <summary>
        /// This Method parses the Plan list files stored in the pVal folder of test section of prime repo.
        /// </summary>
        /// <param name="tosName">Provides current Tos version ex:TOS3613 to run the validation.</param>
        public List<TestPlanInputs> ReturnTestPlanMatchingWithTOSName(string tosName)
        {
            foreach (var tosTestPlanList in this.PValRunList)
            {
                if (tosTestPlanList.TOSVersion.Contains(tosName))
                {
                    Handlers.LoggerHandler.PrintLine("Successfully parsed pValRunList.json file", PrintType.DEFAULT);
                    Handlers.LoggerHandler.PrintLine($"Total number of plans: {tosTestPlanList.TestPlan.Count}", PrintType.LOGGER_ONLY);
                    return tosTestPlanList.TestPlan;
                }
            }

            throw new Exception($"No found test plans found in user defined .json file for TOS version [{tosName}]");
        }

        /// <summary>
        /// This method populates user selected Test programs based on Full test programs available for that tos.
        /// </summary>
        /// <param name="runPlanList">Full List of Plans/Tps populated previously.</param>
        /// <param name="outSelectedPlanList">User selected Plans/TPs.</param>
        /// <returns>List of selected Plans.</returns>
        public List<TestPlanInputs> SelectTplPlansToRun(List<TestPlanInputs> runPlanList, List<SettingsFile.TestPlanInputs> outSelectedPlanList)
        {
            List<string> planNames = new List<string>();
            foreach (var plnName in runPlanList)
            {
                planNames.Add(plnName.TestPlanName);
            }

            for (int i = 0; i < planNames.Count; i++)
            {
                planNames[i] = string.Concat(planNames.IndexOf(planNames[i]), ":", " ", planNames[i]);
            }

            Handlers.LoggerHandler.PrintLine("Accepted formats with usage examples:");
            Handlers.LoggerHandler.PrintLine("\tfull: Runs all Plans.");
            Handlers.LoggerHandler.PrintLine("\t1-4: Runs 1 to 4 plans.");
            Handlers.LoggerHandler.PrintLine("\t1 4: Runs plans 1 and 4.");
            Handlers.LoggerHandler.PrintLine("\tsampletp: runs the specific plan.");
            Handlers.LoggerHandler.PrintLine("============ ******* ============\n");
            Handlers.LoggerHandler.PrintLine($"TPs available for tos version = {PValMain.TosName}.");

            foreach (var testPlans in planNames)
            {
                Handlers.LoggerHandler.PrintLine(testPlans, PrintType.CYAN_CONSOLE);
            }

            Handlers.LoggerHandler.PrintLine("Select the Plans to run:");
            string readKey = Console.ReadLine();
            if (!string.IsNullOrEmpty(readKey))
            {
                if (readKey.Split(' ').Length.ToString() == "1" && readKey.ToUpper() == "FULL")
                {
                    outSelectedPlanList = runPlanList;
                }
                else if (readKey.Split('-').Length.ToString() == "2" && readKey.Contains("-"))
                {
                    string startPlan = readKey.Split('-')[0];
                    string endPlan = readKey.Split('-')[1];
                    for (int itr = int.Parse(startPlan); itr <= int.Parse(endPlan); itr++)
                    {
                        foreach (var plans in runPlanList)
                        {
                            if (itr == runPlanList.IndexOf(plans))
                            {
                                outSelectedPlanList.Add(plans);
                            }
                        }
                    }
                }
                else
                {
                    var tokens = readKey.Split(' ');
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        foreach (var eachPlan in runPlanList)
                        {
                            if (tokens[i].ToUpper().Trim(' ') == runPlanList.IndexOf(eachPlan).ToString())
                            {
                                outSelectedPlanList.Add(eachPlan);
                            }
                            else if (tokens[i].ToUpper().Trim(' ') == eachPlan.TestPlanName.ToUpper())
                            {
                                outSelectedPlanList.Add(eachPlan);
                            }
                        }
                    }

                    return outSelectedPlanList;
                }
            }

            return outSelectedPlanList;
        }

        /// <summary>
        /// Modify fields passed by user in .json file for easier use later in program execution.
        /// </summary>
        internal void PostProcessFields()
        {
            foreach (var runList in this.PValRunList)
            {
                runList.LogArea = ConvertToAbsolute(runList.LogArea, false);

                foreach (var testPlan in runList.TestPlan)
                {
                    /*testPlan.QuickSimFile = ConvertToAbsolute(testPlan.QuickSimFile, false);
                    testPlan.GoldenItuffs = ConvertToAbsolute(testPlan.GoldenItuffs, false);
                    if (PValMain.TosVerMajorRev < 3 || PValMain.TosVerMinorRev < 10)
                    {
                        // TOS 3.10+ uses relative paths for the testprogram files.
                        Handlers.LoggerHandler.PrintLine($"TosVersion=[{PValMain.TosVerMajorRev}.{PValMain.TosVerMinorRev}] Using full paths to tp collateral.", Wrapper.PrintType.DEFAULT);
                        testPlan.TplFile = ConvertToAbsolute(testPlan.TplFile);
                        testPlan.StplFile = ConvertToAbsolute(testPlan.StplFile);
                        testPlan.SocFile = ConvertToAbsolute(testPlan.SocFile);
                        testPlan.PlistFile = ConvertToAbsolute(testPlan.PlistFile);
                        testPlan.EnvFiles.Release = ConvertToAbsolute(testPlan.EnvFiles.Release, false);
                        testPlan.EnvFiles.Debug = ConvertToAbsolute(testPlan.EnvFiles.Debug, false);
                    }
                    else
                    {
                        Handlers.LoggerHandler.PrintLine($"TosVersion=[{PValMain.TosVerMajorRev}.{PValMain.TosVerMinorRev}] Using relative paths to tp collateral.", Wrapper.PrintType.DEFAULT);
                    } */

                    if (PValMain.IsDebugModeRequired)
                    {
                        if (testPlan.EnvFiles.Debug == string.Empty)
                        {
                            throw new Exception($"Environment file for debug mode is not defined in pValRunList.json when " +
                                $" debug mode was set for PVal execution.");
                        }

                        testPlan.EnvFile = testPlan.EnvFiles.Debug;
                    }
                    else
                    {
                        testPlan.EnvFile = testPlan.EnvFiles.Release;

                        if(string.IsNullOrEmpty(testPlan.EnvFile))
                        {
                            throw new Exception($"Environment file for release mode is not defined in pValRunList.json.");
                        }
                    }
                }
            }
        }

        private static string ConvertToAbsolute(string input, bool isRequired = true)
        {
            if (string.IsNullOrEmpty(input))
            {
                if(isRequired)
                {
                    throw new Exception("User input in configuration was empty when it was required for execution.\n" +
                        "Required elements are: Tpl, Stpl, Soc, Plist, Env(Release or Debug). Please check these for proper values.");
                }
                return input;
            }


            bool isRelative = input.Contains(@".\") || input.Contains(@"..\");

            // May break if current working directory is ever changed before this line of code...
            string newPath = isRelative ? Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), input)) : input;
            if (!Directory.Exists(newPath) && !File.Exists(newPath))
            {
                throw new Exception($"File [{newPath}] defined in pValRunList.json was not found.");
            }

            return newPath;
        }
    }
}
