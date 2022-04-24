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

namespace PTHGetSetGsdsDffTC
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using DDG;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.PhAttributes;
    using Prime.SharedStorageService;
    using Prime.TestMethods;

    /// <summary>
    /// PTHGetSetGsdsDffTC Test Class. Run during Main to convert GSDS2DFF or DFF2GSDS.
    /// </summary>
    [PrimeTestMethod]
    public class PTHGetSetGsdsDffTC : TestMethodBase
    {
        private List<Configuration> settings;

        /// <summary>
        /// List if valid GSDSDFFOP.
        /// </summary>
        public enum GSDSDFFOP
        {
            /// <summary>
            /// Sets DFF2GSDS.
            /// </summary>
            DFF2GSDS,

            /// <summary>
            /// Sets GSDS2DFF.
            /// </summary>
            GSDS2DFF,
        }

        /// <summary>
        /// Gets or sets the GSDS2DFF or DFF2GSDS OPType.
        /// </summary>
        public GSDSDFFOP OPType { get; set; }

        /// <summary>
        /// Gets or sets the voltage converter ActiveConfiguration file.
        /// </summary>
        public TestMethodsParams.File ConfigurationFile { get; set; }

        /// <summary>
        /// Gets or sets the file system interface.
        /// </summary>
        protected IFileSystem FileSystem_ { get; set; } = new FileSystem();

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;
            var localFileName = FileUtilities.GetFile(this.ConfigurationFile);
            var fileContents = this.FileSystem_.File.ReadAllText(localFileName);
            this.Console?.PrintDebug($"File Contents=[{fileContents}]");
            this.settings = JsonConvert.DeserializeObject<List<Configuration>>(fileContents);

            // verifying for each setting in json file.
            for (var i = 0; i < this.settings.Count; i++)
            {
                // clean all the inputs.
                this.settings[i].GSDS2DFFAllowedList = this.settings[i].GSDS2DFFAllowedList ?? new List<string>(); // switch to x ??= new List<string>(); in C# 8.0
                this.settings[i].SearchReplace = this.settings[i].SearchReplace ?? new Dictionary<string, string>(); // switch to x ??= new Dictionary<string, string>(); in C# 8.0
                this.settings[i].DFF = this.settings[i].DFF.Replace(" ", string.Empty);
                this.settings[i].DFFOpType = this.settings[i].DFFOpType.Replace(" ", string.Empty);
                for (var j = 0; j < this.settings[i].GSDSList.Count; j++)
                {
                    this.settings[i].GSDSList[j] = this.settings[i].GSDSList[j].Replace(" ", string.Empty);
                }

                var setting = this.settings[i];

                // GSDS2DFF == DFF2GSDS
                if (setting.GSDS2DFF == setting.DFF2GSDS)
                {
                    throw new ArgumentException(
                            $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}:Invalid condition GSDS2DFF == DFF2GSDS {string.Join("\n Invalid condition GSDS2DFF == DFF2GSDS:", setting.GSDS2DFF, setting.DFF2GSDS)}");
                }

                // Demiliter check if GSDSList count is >1
                if (string.IsNullOrEmpty(setting.Delimiter) && (setting.GSDSList.Count > 1))
                {
                    throw new ArgumentException(
                            $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}:Missing Delimiter as GSDS count is greater than 1 {string.Join("\n Invalid Delimiter parameters:", setting.Delimiter)}");
                }

                // Demiliter length check == 1
                if (!string.IsNullOrEmpty(setting.Delimiter) && (setting.Delimiter.Length != 1))
                {
                    throw new ArgumentException(
                            $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Delimiter length !=1 and supports 1 parameter only {string.Join("\n Invalid Delimiter parameters:", setting.Delimiter)}");
                }

                // Count of GSDSScopeType,DFF,DFFOpType,Delimiter is 1
                if (setting.DFF.Contains(",") || setting.DFFOpType.Contains(",") || setting.GSDSScopeType.Contains(","))
                {
                    throw new ArgumentException(
                            $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: DFF,DFFOpType,GSDSScopeType supports 1 parameter only. {string.Join("\n Invalid DFF,DFFOpType,GSDSScopeType parameters:", setting.DFF, setting.DFFOpType, setting.GSDSScopeType)}");
                }

                if (string.IsNullOrWhiteSpace(setting.DFFOpType))
                {
                    if (setting.DFF.Contains(':'))
                    {
                        var l = setting.DFF.Split(new char[] { ':' }, 2);
                        setting.DFFOpType = l[0];
                        setting.DFF = l[1];
                    }
                    else
                    {
                        throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: DFFOpType is required or DFF must be optype.token format.");
                    }
                }

                // Verify everything in the GSDSList is a valid GSDS token name or in the GSDS2DFFAllowedList list.
                var gsdsPrefix = string.Empty;
                if (!string.IsNullOrWhiteSpace(setting.GSDSScopeType))
                {
                    // GSDSScopeType length == 3
                    // scope = U for unit/dut, L for lot
                    // type = S for string, D for double, or I for integer.
                    var hasGsdsPrefixRegEx = new Regex("^(G.)?[ULI].[SDI]$");
                    if (!hasGsdsPrefixRegEx.IsMatch(setting.GSDSScopeType))
                    {
                        throw new ArgumentException(
                                $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: GSDSScopeType supports scope = U for unit/dut, L for lot; type = S for string, D for double, or I for integer. {string.Join("\n Invalid GSDSScopeType format", setting.GSDSScopeType)}");
                    }

                    // Create the G.U.S. prefix to prepend to all the gsds tokens.
                    gsdsPrefix = $"{(setting.GSDSScopeType.StartsWith("G.") ? string.Empty : "G.")}{setting.GSDSScopeType}.";
                }

                for (var j = 0; j < setting.GSDSList.Count; j++)
                {
                    // Check if the token is in G.U.S.token format ... or if its in the GSDS2DFFAllowedList (meaning its not a gsds token).
                    if ((setting.DFF2GSDS || !setting.GSDS2DFFAllowedList.Contains(setting.GSDSList[j])) && !DDG.Gsds.IsTokenFormat(setting.GSDSList[j]))
                    {
                        if (string.IsNullOrEmpty(gsdsPrefix))
                        {
                            throw new ArgumentException(
                                $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: GSDSScopeType is empty and Token=[{setting.GSDSList[j]}] is not in GSDS format or in GSDS2DFFAllowedList.");
                        }
                        else
                        {
                            this.settings[i].GSDSList[j] = gsdsPrefix + this.settings[i].GSDSList[j];
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        [Returns(2, PortType.Fail, "Failed to Convert DFF to GSDS!")]
        [Returns(3, PortType.Fail, "Failed to Convert GSDS to DFF!")]
        [Returns(4, PortType.Fail, "No valid operation. Check OPType!")]
        public override int Execute()
        {
            // var failedDff2Gsds = false;
            // var failedGsds2Dff = false;
            List<bool> failedDff2Gsds = new List<bool>();
            List<bool> failedGsds2Dff = new List<bool>();

            this.settings.ForEach(setting =>
            {
                // Dff2Gsds
                if (setting.DFF2GSDS && (this.OPType == GSDSDFFOP.DFF2GSDS))
                {
                    failedDff2Gsds.Add(this.Dff2GsdsFunc(setting));
                }

                // Gsds2Dff
                if (setting.GSDS2DFF && (this.OPType == GSDSDFFOP.GSDS2DFF))
                {
                    failedGsds2Dff.Add(this.Gsds2DffFunc(setting));
                }
            });

            this.Console?.PrintDebug($"Printing failedDff2Gsds [{failedDff2Gsds}]. \n Printing failedGsds2Dff =[{failedGsds2Dff}].\n");

            if (failedDff2Gsds.Contains(true))
            {
                return 2;
            }
            else if (failedGsds2Dff.Contains(true))
            {
                return 3;
            }
            else if (failedDff2Gsds.Count + failedGsds2Dff.Count == 0)
            {
                return 4;
            }
            else
            {
                return 1;
            }
        }

        private static void WriteToItuff(Configuration settingFunc, string dffvalue)
        {
            if (settingFunc.PrintDFF)
            {
                var ituffWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();
                ituffWriter.SetTnamePostfix($"_{settingFunc.DFF}");
                ituffWriter.SetData(dffvalue);
                Prime.Services.DatalogService.WriteToItuff(ituffWriter);
            }
        }

        private static string DoSearchReplace(Dictionary<string, string> searchReplacePair, string sourceString)
        {
            var finalString = sourceString;
            foreach (var pair in searchReplacePair)
            {
                finalString = Regex.Replace(finalString, pair.Key, pair.Value);
            }

            return finalString;
        }

        private bool Dff2GsdsFunc(Configuration settingFunc)
        {
            var failDff = false;
            this.Console?.PrintDebug($"Pulling token=[{settingFunc.DFF}] Dff data from current die id with opType=[{settingFunc.DFFOpType}].\n");

            // Get the DFF using GetDffByOpType Prime Service
            var dffValue = Prime.Services.DffService.GetDffByOpType(settingFunc.DFF, settingFunc.DFFOpType);
            dffValue = DoSearchReplace(settingFunc.SearchReplace, dffValue);

            // printing the dff in iTuff
            WriteToItuff(settingFunc, dffValue);

            // Splitting the DFF string to string array with the delimiter
            string[] dffStrList;
            if (settingFunc.Delimiter.Length == 1)
            {
                this.Console?.PrintDebug($"Delimiter token used =[{settingFunc.Delimiter.ToCharArray()[0]}].\n");
                dffStrList = dffValue.Split(settingFunc.Delimiter.ToCharArray()[0]);
            }
            else
            {
                List<string> templst = new List<string>();
                templst.Add(dffValue);
                dffStrList = templst.ToArray();
            }

            // check if the GSDSList count matches the DFF split count
            if (dffStrList.Count() == settingFunc.GSDSList.Count)
            {
                // foreach (var gSDSListFunc in settingFunc.GSDSList)
                for (int i = 0; i < settingFunc.GSDSList.Count; i++)
                {
                    var token = settingFunc.GSDSList[i];

                    // check if the GSDS already exist and then print
                    if (DDG.Gsds.IsTokenFormatAndExists(token))
                    {
                        failDff = true;
                        Prime.Services.ConsoleService.PrintError($"The GSDS token = [{token}] already exists.");
                    }
                    else
                    {
                        // Setting the GSDS
                        this.Console?.PrintDebug($"Setting GSDS token=[{token}] set to value from DFF=[{dffStrList[i]}].\n");
                        DDG.Gsds.WriteToken(token, dffStrList[i].Replace(" ", string.Empty));
                    }
                }
            }
            else
            {
                failDff = true;
                Prime.Services.ConsoleService.PrintError($"The GSDSList count does not matches the DFF split count. \n GSDSList count=[{settingFunc.GSDSList.Count}]. \n GSDSList count=[{dffStrList.Count()}]");
            }

            this.Console?.PrintDebug($"The failGsds token = [{failDff}].\n");
            return failDff;
        }

        private bool Gsds2DffFunc(Configuration settingFunc)
        {
            var failGsds = false;
            this.Console?.PrintDebug($"Pulling token=[{settingFunc.DFF}] Dff data from current die id with opType=[{settingFunc.DFFOpType}].\n");

            var allGsdsValues = new List<string>();
            foreach (var token in settingFunc.GSDSList)
            {
                // check if the GSDS exist
                if (settingFunc.GSDS2DFFAllowedList.Contains(token))
                {
                    allGsdsValues.Add(token);
                }
                else if (DDG.Gsds.IsTokenFormatAndExists(token))
                {
                    var value = Convert.ToString(DDG.Gsds.ReadToken(token)).Replace(" ", string.Empty);
                    allGsdsValues.Add(value);
                }
                else
                {
                    failGsds = true;
                    Prime.Services.ConsoleService.PrintError($"The GSDS token = [{token}] does not exist nor it is in the GSDS2DFFAllowedList.");
                }
            }

            // Printing the final string before getting the DFF
            var dffStrCum = string.Join(settingFunc.Delimiter, allGsdsValues);
            dffStrCum = DoSearchReplace(settingFunc.SearchReplace, dffStrCum);
            this.Console?.PrintDebug($"Final dffStrCum token=[{dffStrCum}].\n");

            // Setting the DFF
            Prime.Services.DffService.SetDff(settingFunc.DFF, dffStrCum);

            // Get the DFF using GetDffByOpType Prime Service. Validation that the DFF is set correctly
            var dffValue = Prime.Services.DffService.GetDffByOpType(settingFunc.DFF, settingFunc.DFFOpType);

            // printing the dff in iTuff
            WriteToItuff(settingFunc, dffValue);
            this.Console?.PrintDebug($"The DFF token = [{settingFunc.DFF}] is set [{dffValue}].\n");
            this.Console?.PrintDebug($"The failGsds token = [{failGsds}].\n");
            return failGsds;
        }
    }
}
