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
    using Wrapper;
    using System;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is meant for populating various folder paths and reading files for the main method.
    /// </summary>
    public class RuntimeCollaterals
    {
        private RuntimeCollaterals()
        {
        }

        /// <summary>
        /// Gets or sets path to where PVAL currently resides.
        /// </summary>
        /// <remarks> Will return invalid values if current directory is ever changed. </remarks>
        public static string AutomationDirectory 
        { 
            get { return Directory.GetCurrentDirectory(); } 
        }

        /// <summary>
        /// Gets or sets path to main directory.
        /// </summary>
        public string FinalReportLogsPath { get; private set; }

        /// <summary>
        /// Gets or sets location of logs directory.
        /// </summary>
        public string TOSLogsPath { get; private set; }

        /// <summary>
        /// Gets or sets directory containing current TOS release.
        /// </summary>
        public string CurrentTOSReleaseDirectory 
        { 
            get
            {
                return Handlers.EnvironmentVariableHandler.RetrievePathFromEnvironmentVariable(CurrentTOSReleaseVar);
            }
        }

        /// <summary>
        /// Gets or sets location of directory containing all TOS releases.
        /// </summary>
        public string AllTOSReleasesDirectory 
        { 
            get
            {
                return Handlers.EnvironmentVariableHandler.RetrievePathFromEnvironmentVariable(AllTOSReleasesVar);
            }
        }

        /// <summary>
        /// Gets or sets location of TOS single script tool.
        /// </summary>
        /// <remarks> Changes everytime TOS is switched with a new verison. </remarks>
        public string SingleScriptForCurrentTOS
        {
            get
            {
                return Path.GetFullPath(
                    Path.Combine(this.CurrentTOSReleaseDirectory, @"Runtime\Release\SingleScriptCmd.exe"));
            }
        }

        public string TOSControlPathForCurrentTos
        {
            get
            {
                return Path.GetFullPath(
                    Path.Combine(this.CurrentTOSReleaseDirectory, @"Runtime\Release\HdmtSupervisorService\hdmttosctrl.exe"));
            }
        }

        /// <summary>
        /// Parallelized version of user generated .json settings file.
        /// </summary>
        public SettingsFile SettingsFile { get; private set; }

        /// <summary>
        /// Name of the directory that contains the logs for pVal execution.
        /// </summary>
        public const string PValLogsFolderName = "pVal";

        /// <summary>
        /// Name of JSON file that contains the settings for pVal execution.
        /// </summary>
        public const string JsonFileName = "pValRunList.json";

        private const string AllTOSReleasesVar = "TOSROOT";

        /// <summary>
        /// Env token to retrieve HDMTOS filepath
        /// </summary>
        private const string CurrentTOSReleaseVar = "HDMTTOS";

        /// <summary>
        /// Initialize a new instance of <see cref="RuntimeCollaterals"/> from a JSON file in the current working directory.
        /// </summary>
        /// <returns> Instance of <see cref="RuntimeCollaterals"/> initialized from pValRunList.json file in local dir.</returns>
        public static RuntimeCollaterals InitializeToolCollaterals()
        {
            RuntimeCollaterals newCollats = new RuntimeCollaterals();
            newCollats.SettingsFile = ParallelizeSettings();
            newCollats.InitializeFilePaths();
            return newCollats;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeCollaterals"/> class.
        /// This constructor parse and process the data given in the .json file.
        /// The purpose is parse test plants files.
        /// </summary>
        /// <param name="autoDirectory"> path of UserSDK.</param>
        private static SettingsFile ParallelizeSettings()
        {
            SettingsFile pValRunList;
            string jsonText;

            //string jsonFilePath = Path.Combine(AutomationDirectory, JsonFileName);
            string jsonFilePath = Path.Combine("C:\\runDir", JsonFileName);

            jsonText = Handlers.DirectoryHandler.ReadTextFromPath(jsonFilePath);

            Handlers.LoggerHandler.PrintLine($"pValRunList file path = {Convert.ToString(jsonFilePath)}\n", PrintType.DEFAULT);
            pValRunList = JsonConvert.DeserializeObject<SettingsFile>(jsonText);
            pValRunList.PostProcessFields();
            return pValRunList;
        }

        private void InitializeFilePaths()
        {
            if(string.IsNullOrEmpty(this.SettingsFile.PValRunList[0].LogArea))
            {
                this.FinalReportLogsPath = Path.Combine(AutomationDirectory, PValLogsFolderName);
            }
            else
            {
                this.FinalReportLogsPath = this.SettingsFile.PValRunList[0].LogArea; // FIXME: Not optimal... make it pull from the base object...
            }

            this.TOSLogsPath = FinalReportLogsPath;
        }
    }
}