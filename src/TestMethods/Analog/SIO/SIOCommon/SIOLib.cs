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

namespace SIO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Prime.Base.Exceptions;

    /// <summary>
    /// Enum for messages.
    /// </summary>
    public enum MsgEnum
    {
        /// <summary>SIO_ALWAYS</summary>
        SIO_ALWAYS,

        /// <summary>SIO_ERROR</summary>
        SIO_ERROR,

        /// <summary>SIO_INFO</summary>
        SIO_INFO,

        /// <summary>SIO_DEBUG</summary>
        SIO_DEBUG,
    }

    /// <summary>
    /// Class for most shared SIO code.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "dumb rule.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "dumb rule.")]
    public class SIOLib
    {
        /// <summary>
        /// Container for the Compressed data.
        /// contains the raw base 32 data, the compressed data and the translation key.
        /// </summary>
        public class CompressedData
        {
            /// <summary>Gets the final compressed data.</summary>
            public string CompressedString { get; private set; }

            /// <summary>Gets the uncompressed data.</summary>
            public string UncompressedString { get; private set; }

            /// <summary>Gets the compression translation table.</summary>
            public string TranslationTable { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CompressedData"/> class.
            /// </summary>
            /// <param name="orig">Uncompressed Data.</param>
            /// <param name="compress">Compressed Data.</param>
            /// <param name="table">Translation Table.</param>
            public CompressedData(string orig, string compress, string table)
            {
                this.UncompressedString = orig;
                this.CompressedString = compress;
                this.TranslationTable = table;
            }
        }

        /// <summary>RegEx used to determine if a string is binary or not.</summary>
        public static readonly Regex IsBinaryRegex = new Regex("^[01]+$");

        /// <summary>GSDS Token used to store capture data for EDC Shmoos.</summary>
        public static readonly string SIOGSDSBINDMEMDATA = "SIO_GSDS_BINDMEMDATA"; // Scope="UNT"(unit level)

        // class global

        /// <summary>
        /// Gets or sets the total number of shmoo errors.
        /// </summary>
        internal int ShmooSetupErros { get; set; }

        private bool debugMode { get; set; }

        // globals for compression
        private bool COMPRESS_DEBUG_INNER { get; set; } = false;

        private bool COMPRESS_DEBUG { get; set; } = false;

        private string[] compress_runTypeLst { get; } = { "QUAD", "TRIPLE", "DOUBLE", "TIDY" };

        private char[] compress_keys { get; } =
            {
                '!', '#', '$', '%', '&',
                '(', ')', '-', '.', '/', '`',
                ':', ';', '<', '+', '>', '=',
                '@', '[', ']', '^', '?', '}',
                '~', '\'', '*', '0', '1',
                '8', '9',
            };

        private int compressKeyIndex = -1;

        private Dictionary<string, string[]> compress_container { get; set; } = new Dictionary<string, string[]>();

        private Dictionary<char, string> compress_translation { get; set; } = new Dictionary<char, string>();

        /* Private helper Variables/Constants for shmoo */
        private List<string> ShmooOuterLoopAxisOrder { get; } = new List<string> { "R", "S", "T", "U", "V", "W", "Z", "Y", "X" };

        private List<string> ShmooInnerLoopAxisOrder { get; } = new List<string> { "YShmoo", "XShmoo" };

        /// <summary>
        /// Initializes a new instance of the <see cref="SIOLib"/> class.
        /// </summary>
        /// <param name="debug">True if Debug Mode is enabled.</param>
        public SIOLib(bool debug)
        {
            this.debugMode = debug;
        }

        /// <summary>
        /// simple wrapper function for Primes GetFile.  Might need to add
        /// caching depending on how prime handles things.
        /// </summary>
        /// <param name="remoteFileName">Remote file nam.</param>
        /// <returns>path to the local file.</returns>
        public static string GetFile(string remoteFileName)
        {
            var localFileName = string.Empty;

            // for evg they often put a * instead of a . so replace it here if they do...no idea why they do that.
            remoteFileName = remoteFileName.Replace('*', '.');

            // prime can't handle relative paths unless they start with './' so do some checking here.
            if (remoteFileName.StartsWith("."))
            {
                // relative path, starts with . -> no issues
            }
            else if (Path.IsPathRooted(remoteFileName))
            {
                // absolute/rooted path, no issues.
            }
            else
            {
                // is a relative with no .
                remoteFileName = "./" + remoteFileName;
            }

            try
            {
                localFileName = Prime.Services.FileService.GetFile(remoteFileName);
            }
            catch (Exception e)
            {
                Prime.Services.ConsoleService.PrintError($"GetFile({remoteFileName}) threw an exception.\n{e.Message}");
                return string.Empty;
            }

            if (string.IsNullOrEmpty(localFileName))
            {
                Prime.Services.ConsoleService.PrintError($"GetFile({remoteFileName}) returned an empty string.");
                return string.Empty;
            }

            return localFileName;
        }

        /// <summary>
        /// Splits a string into multiple, equal-length parts.  EmbPython used some fancy
        /// zip() calls to do this.
        /// </summary>
        /// <param name="str">The Base string to spli.</param>
        /// <param name="partLength">The Length of each individual partion.</param>
        /// <param name="offset">Where in the string to start, all characters left of this will be discarded.</param>
        /// <param name="includeRemainder">If true, the final partition might have less than the requested characters,
        /// if false, only partitions with the requested length will be returned, any remaining characters are discarded.</param>
        /// <returns>List of substrings.</returns>
        public static IEnumerable<string> SplitStrInParts(string str, int partLength, int offset, bool includeRemainder = false)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str), "In SplitStrInParts, base string is null.");
            }

            if (partLength <= 0)
            {
                throw new ArgumentException("In SplitStrInParts, Partition length has to be positive.", nameof(partLength));
            }

            if (offset < 0)
            {
                throw new ArgumentException("In SplitStrInParts, offset has to be positive or 0.", nameof(offset));
            }

            var max = includeRemainder ? str.Length : str.Length - partLength + 1;
            for (var i = offset; i < max; i += partLength)
            {
                yield return str.Substring(i, Math.Min(partLength, str.Length - i));
            }
        }

        /// <summary>
        /// Prints the message/error to the HDMT console window.
        /// </summary>
        /// <param name="msgType">MsgEnum setting for this message.  Prime doesn't
        /// support multiple debug modes, so only SIO_ERROR matters.  Everything else
        /// is treated the same.</param>
        /// <param name="message">Message to write to the consol.</param>
        public void MsgToConsole(MsgEnum msgType, string message)
        {
            // Prime only has Debug and Error messages so this isn't an exact translation...
            if (msgType == MsgEnum.SIO_ALWAYS)
            { // hijack printerror
                Prime.Services.ConsoleService.PrintError($"[SIO_ALWAYS] {message}", 0, " ", " ");
            }
            else if (msgType == MsgEnum.SIO_ERROR)
            {
                Prime.Services.ConsoleService.PrintError(message);
            }
            else if (this.debugMode)
            {
                Prime.Services.ConsoleService.PrintDebug($"[{msgType}] {message}");
            }
        }

        /// <summary>
        /// Writes the given name/data to Ituff using tname/strgval tokens.
        /// </summary>
        /// <param name="sDlogName">The testname (tname) to log.</param>
        /// <param name="sDlogData">The data (strgval) to log.</param>
        /// <returns>true on success.</returns>
        public bool ResultStrgValToDatalog(string sDlogName, string sDlogData)
        {
            var nDataLength = sDlogData.Length;
            if (nDataLength <= 0)
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"ResultStrgValToDatalog: Data Length <= 0 [{sDlogData.Length}]");
                return false;
            }

            try
            {
                var writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
                writer.SetCustomTname(sDlogName + "0");
                writer.SetData(sDlogData);
                writer.SetDelimiterCharacterForWrap('!');
                Prime.Services.DatalogService.WriteToItuff(writer);
            }
            catch (Exception e)
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"ResultStrgValToDatalog: Failed writing to ituff\n{e.Message}");
                return false;
            }

            /*
            var nMaxCharacter = 3960; // FIXME - this should probably be a global
            var nDataLength = sDlogData.Length;
            if (nDataLength <= 0)
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"ResultStrgValToDatalog: Data Length <= 0 [{sDlogData.Length}]");
                return false;
            }

            // different formats for sort vs class. (Midas is sort)
            var token = "0";
            var tnamePrefix = string.Empty;
            if (!Prime.Services.TpSettingsService.IsTpFeatureEnabled(Prime.TpSettingsService.Feature.Midas))
            {
                token = "2";
                tnamePrefix = $"{token}_lsep\n";
            }

            // Ituff has line length limits, so split the data into multiple lines if needed.
            var sDatalogStr = string.Empty;
            var nPartition = 0;
            foreach (var output in SplitStrInParts(sDlogData, nMaxCharacter, 0, true))
            {
                sDatalogStr += $"{tnamePrefix}{token}_tname_{sDlogName}{nPartition}\n{token}_strgval_{output}\n";
                nPartition += 1;
            }

            // finally write to ituff and catch any errors.
            try
            {
                Prime.Services.DatalogService.WriteToItuff(sDatalogStr);
            }
            catch (Exception e)
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"ResultStrgValToDatalog: Failed writing to ituff\n{e.Message}");
                return false;
            }

            this.MsgToConsole(MsgEnum.SIO_INFO, $"\n{sDatalogStr}"); */
            return true;
        }

        /// <summary>
        /// Update the given UserData to support Shmoo methods.
        /// </summary>
        /// <param name="userDataFile">UserDataFile object.</param>
        /// <param name="token">Token to index into the UserDataFile.</param>
        /// <returns>true on success.</returns>
        public bool ShmooTestSetup(UserFile userDataFile, string token)
        {
            if (userDataFile == null || userDataFile.TokenBlocks == null)
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"In ShmooTestSetup, invalid UserDataFile.");
                return false;
            }

            if (!userDataFile.TokenBlocks.ContainsKey(token))
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"In ShmooTestSetup UserDataFile does not contain token=[{token}].  Valid Tokens are [{string.Join(", ", userDataFile.TokenBlocks.Keys)}]");
                return false;
            }

            var userData = userDataFile.TokenBlocks[token];

            try
            {
                // try getting patlist based on evg parameter name
                this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Trying to get plist name from [patlist] parameter in test=[{userData.ExecuteTest}].");
                userData.Plist = Prime.Services.TestProgramService.GetTestInstanceParameter(userData.ExecuteTest, "patlist");
            }
            catch
            {
                try
                {
                    // try getting patlist based on Prime parameter name
                    this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Trying to get plist name from [Patlist] parameter in test=[{userData.ExecuteTest}].");
                    userData.Plist = Prime.Services.TestProgramService.GetTestInstanceParameter(userData.ExecuteTest, "Patlist");
                }
                catch
                {
                    this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Failed to get plist name from test=[{userData.ExecuteTest}].");
                    userData.Plist = "NA";
                }
            }

            userData.TestResults = new List<UserFile.TestResult>();
            if (userData.TestType == "PERPORT" || userData.TestType == "PERLANE" || userData.TestType == "BSCAN")
            {
                if (userData.CompareBlock == string.Empty)
                {
                    this.MsgToConsole(MsgEnum.SIO_ERROR, $"ShmooTestSetup: Token=[{token}] TestType=[{userData.TestType}] requires CompareBlock be set.");
                    return false;
                }

                // FIXME - didn't find any examples of Shmoo using a CompareBlock on TGL so not implementing for now
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"ShmooTestSetup: Token=[{token}] TestType=[{userData.TestType}] Compare Block functionality is not implemented yet.");
                return false;
            } // end TestType in ["PERPORT", "PERLANE", "BSCAN"]
            else
            {
                var tmpTestResult = new UserFile.TestResult();
                tmpTestResult.PassCount.Add(0);
                userData.TestResults.Add(tmpTestResult);
            }

            userData.OriginalParameters = new List<UserFile.TestParameter>();
            foreach (var shmooaxis in userData.ShmooAxis.Values)
            {
                if (shmooaxis.Type == "TIMING" || shmooaxis.Type == "LEVEL")
                {
                    // FIXME - didn't find any examples of Shmoo using TIMING/LEVEL on TGL so not implementing for now
                    this.MsgToConsole(MsgEnum.SIO_ERROR, $"ShmooTestSetup: Token=[{token}] TestType=[{userData.TestType}] Shmo Axis=[{shmooaxis.Type}] is not implemented yet.");
                    return false;
                    /* userData.OriginalParameters.Add(new TestParameter(shmooaxis.type, shmooaxis.key)); */
                }
            }

            // make sure all the Key/Value type shmoo axis have valid data.  Create 1 step of dummy data for the ones that don't
            foreach (var axisName in new List<string> { "R", "S", "T", "U", "V", "W", "Z", "Y", "X" })
            {
                if (!userData.ShmooAxis.ContainsKey(axisName))
                {
                    userData.ShmooAxis[axisName] = new UserFile.ShmooAxis(axisName, true);
                }

                if (userData.ShmooAxis[axisName].Key == string.Empty || userData.ShmooAxis[axisName].Values.Count == 0)
                {
                    userData.ShmooAxis[axisName].Key = string.Empty;
                    userData.ShmooAxis[axisName].Values = new List<string> { string.Empty };
                    userData.ShmooAxis[axisName].Valid = false;
                }
                else
                {
                    userData.ShmooAxis[axisName].Valid = true;
                }
            }

            // Now check the XYShmoo type shmoo...if these are invalid then replace them with the XY Key/Value type data.
            // If they're valid, convert start/steps/resolution into the Values list.
            foreach (var axisBase in new List<string> { "X", "Y" })
            {
                var axisName = $"{axisBase}Shmoo";
                if (userData.ShmooAxis.ContainsKey(axisName))
                {
                    if (userData.ShmooAxis[axisName].NumSteps <= 0 || userData.ShmooAxis[axisName].Resolution == 0)
                    {
                        this.MsgToConsole(MsgEnum.SIO_ERROR, $"ShmooTestSetup: Token=[{token}] Axis=[{axisName}] invalid NumSteps=[{userData.ShmooAxis[axisName].NumSteps}] or Resolution=[{userData.ShmooAxis[axisName].Resolution}] both should be > 0.");
                        return false;
                    }

                    var current = userData.ShmooAxis[axisName].StartValue;
                    userData.ShmooAxis[axisBase].Values = new List<string>();
                    for (var i = 0; i < userData.ShmooAxis[axisName].NumSteps; i++)
                    {
                        userData.ShmooAxis[axisBase].Values.Add(current.ToString());
                        current += userData.ShmooAxis[axisName].Resolution;
                    }

                    userData.ShmooAxis[axisName].Valid = true;
                }
                else
                {
                    userData.ShmooAxis[axisName] = new UserFile.ShmooAxis(axisName, true);
                    userData.ShmooAxis[axisName].Key = userData.ShmooAxis[axisBase].Key;
                    userData.ShmooAxis[axisName].Values = userData.ShmooAxis[axisBase].Values;
                    userData.ShmooAxis[axisName].Type = userData.ShmooAxis[axisBase].Type;
                    userData.ShmooAxis[axisName].Valid = true;

                    userData.ShmooAxis[axisBase].Key = string.Empty;
                    userData.ShmooAxis[axisBase].Values = new List<string> { string.Empty };
                    userData.ShmooAxis[axisBase].Valid = false;
                }
            }

            // TestType: TXEQKTI or TXEQIIO sTxEqSum
            // FIXME - TXEQ shmoo types currently not implmented
            if (userData.TestType == "TXEQKTI" || userData.TestType == "TXEQIIO")
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"ShmooTestSetup: Token=[{token}] TestType=[{userData.TestType}] is not implemented yet.");
                return false;
            }

            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"TestInstance:      {userData.ExecuteTest}");
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"PatmodifyInstance: {userData.PatModifyTest}");
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"DlogName:          {userData.DlogName}");
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Plist:             {userData.Plist}");
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"TestType:          {userData.TestType}");
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"NumberOfRun:       {userData.NumberOfRuns}");
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"CompareBlock:      {userData.CompareBlock}");
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"ResultSize:        {userData.TestResults.Count}");

            return true;
        }

        /// <summary>
        /// Perform a shmoo on the given UserData.
        /// </summary>
        /// <param name="userData">SIOUserTokenData containing the object to shmoo.</param>
        /// <returns>Number of setup errors.</returns>
        public int RunShmoo(UserFile.UserData userData)
        {
            userData.PreSetupIteration = 0;
            this.ShmooSetupErros = 0;
            Dictionary<string, string> currentState = new Dictionary<string, string>();
            foreach (var axis in this.ShmooOuterLoopAxisOrder)
            {
                currentState[axis] = string.Empty;
            }

            foreach (var axis in this.ShmooInnerLoopAxisOrder)
            {
                currentState[axis] = string.Empty;
            }

            currentState["RUN"] = string.Empty;
            if (userData.IpoUserVar != string.Empty)
            {
                currentState["IPOUSERVAR"] = userData.IpoUserVar;
            }

            this.ShmooOuterLoopRecursive(userData, currentState, 0);

            /* Restore original parameters. */
            foreach (var param in userData.OriginalParameters)
            {
                // FIXME - didn't find any examples of Shmoo using TIMING/LEVEL on TGL so not implementing for now
                // leave this here so if we ever add code to save OriginalParameters we'll remember to code this too.
                throw new TestMethodException($"Ability to restore original paramters has not been implmented yet.");
            }

            return this.ShmooSetupErros;
        }

        /// <summary>
        /// Convert the shmoos current state to a printable string.
        /// </summary>
        /// <param name="currentState">CurrentState object containing all the shmoo axis current values.</param>
        /// <param name="excludeInnerAxis">If true, the InnerLoop Axis values will not be output.</param>
        /// <returns>String representing the current state of the shmoo.</returns>
        public string ShmooStateToToken(Dictionary<string, string> currentState, bool excludeInnerAxis)
        {
            var tokens = new List<string>();
            foreach (var axis in this.ShmooOuterLoopAxisOrder)
            {
                if (currentState[axis] != string.Empty)
                {
                    tokens.Add(currentState[axis]);
                }
            }

            if (!excludeInnerAxis)
            {
                foreach (var axis in this.ShmooInnerLoopAxisOrder)
                {
                    if (currentState[axis] != string.Empty)
                    {
                        tokens.Add(currentState[axis]);
                    }
                }
            }

            /* data in uservar is stored as follows
             *   test,SKX_PCIEREUT_H::c_pciereut_viominf8000_end_vmcharperportuf_RCOMPOVR_3_5
             *   1,1_2,timing,BASE::lvl_base_sio_min,vcc_class_vio_param,0.5
             *   2,1_4,timing,BASE::tim_d11r12_4x_base_b100_m400,bck_param,1.1e-008
             */
            if (currentState.ContainsKey("IPOUSERVAR") && currentState["IPOUSERVAR"] != string.Empty)
            {
                var tmp = currentState["IPOUSERVAR"].Split('.');
                if (tmp.Length != 2)
                {
                    throw new TestMethodException($"Cannot split uservar into collection/variable pair. IPOUSERVAR=[{currentState["IPOUSERVAR"]}].  Expecting [<collection>.<variable>]");
                }

                var userVarValue = Prime.Services.UserVarService.GetStringValue(tmp[0], tmp[1]);
                if (userVarValue != string.Empty)
                {
                    var ipoTestDetails = userVarValue.Split('\n');
                    for (var i = 1; i < ipoTestDetails.Length; i++)
                    {
                        if (ipoTestDetails[i] != string.Empty)
                        {
                            var testItems = ipoTestDetails[i].Split(',');
                            if (testItems.Length >= 6)
                            {
                                if (testItems[2].ToUpper() == "TIMING" || testItems[2].ToUpper() == "LEVEL")
                                {
                                    tokens.Add($"{testItems[4]}:{testItems[5]}");
                                }
                            }
                        }
                    }
                }
            }

            if (currentState["RUN"] != string.Empty)
            {
                tokens.Add($"RUN:{currentState["RUN"]}");
            }

            return string.Join(";", tokens);
        }

        /// <summary>
        /// Setup the Shmoo testpoing.
        /// </summary>
        /// <param name="key">Parameter name.</param>
        /// <param name="step">Value to write to the parameter.</param>
        /// <param name="type">Type of the Parameter, one of LEVEL, TIMING, PARAMSTR or PATMODIFY.</param>
        /// <param name="test">Test or TestCondition name.</param>
        /// <param name="patModifyTest">Name of the pattern modify test to run if type==PATMODIFY.</param>
        /// <returns>true on success.</returns>
        public bool SetupTestParam(string key, string step, string type, string test, string patModifyTest)
        {
            // this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Running SetupTestParam({key}, {step}, {type}, {test}, {patModifyTest})");
            if (key == string.Empty)
            {
                return true;
            }

            if (type == "LEVEL")
            {
                // FIXME - LEVEL type not supported.
                throw new TestMethodException($"LEVEL type not supported in SetupTestParam.");
            }
            else if (type == "TIMING")
            {
                // FIXME - TIMING type not supported.
                throw new TestMethodException($"TIMING type not supported in SetupTestParam.");
            }
            else if (type == "PARAMSTR")
            {
                // FIXME - PARAMSTR type not supported.
                throw new TestMethodException($"PARAMSTR type not supported in SetupTestParam.");
            }
            else if (type.StartsWith("PATMOD"))
            {
                return this.ExecPatModifyTest(patModifyTest, $"{key}_{step}");
            }
            else
            {
                throw new TestMethodException($"In SetupTestParam: Type=[{type}] is not supported.");
            }
        }

        /// <summary>
        /// Performs the SIOUtil.SetupTest functionality (runs patmod based on token data).
        /// </summary>
        /// <param name="userDataFile">UserDataFile object.</param>
        /// <param name="token">Token to index into the UserDataFile.</param>
        /// <param name="testName">For Timing/Levels updates, the name of the test to modify.</param>
        /// <returns>true on success.</returns>
        public bool RunSetupTest(UserFile userDataFile, string token, string testName = "")
        {
            var rslt = true;
            if (userDataFile == null || userDataFile.TokenBlocks == null)
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"In RunSetup, invalid UserDataFile.");
                return false;
            }

            if (userDataFile.TokenBlocks.ContainsKey(token))
            {
                // its valid User Token, so it should contain MODIFY_TOKEN and MODIFY_VALUE sections.
                var userData = userDataFile.TokenBlocks[token];
                if (userData.ModifyToken == string.Empty || userData.ModifyValue == string.Empty)
                {
                    throw new TestMethodException($"In RunSetupTest: Token=[{token}] Both MODIFY_TOKEN=[{userData.ModifyToken}] and MODIFY_VALUE=[{userData.ModifyValue}] need values.");
                }

                var allModifyTokens = userData.ModifyToken.Split('!');
                var allModifyValues = userData.ModifyValue.Split('!');
                if (allModifyTokens.Length != allModifyValues.Length)
                {
                    throw new TestMethodException($"In RunSetupTest: Token=[{token}] Both MODIFY_TOKEN=[{userData.ModifyToken}] and MODIFY_VALUE=[{userData.ModifyValue}] need the same number of values.");
                }

                for (var i = 0; i < allModifyTokens.Length; i++)
                {
                    var modName = allModifyTokens[i];
                    var modType = "PATMOD";
                    var modValue = allModifyValues[i];
                    if (modName.Contains(":"))
                    {
                        var l = modName.Split(':');
                        modName = l[0].Trim();
                        modType = l[1].Trim().ToUpper();
                    }

                    rslt &= this.SetupTestParam(modName, modValue, modType, testName, userData.PatModifyTest);
                }
            }
            else
            {
                // Its not a user token, its a pat-modify token.  Use the PatModify testname from the global setup.
                if (!userDataFile.TokenBlocks.ContainsKey("GLOBAL_SETUP"))
                {
                    this.MsgToConsole(MsgEnum.SIO_ERROR, $"In RunSetup, UserDataFile does not contain a GLOBAL_SETUP or [{token}] token.");
                    return false;
                }

                var patModTest = userDataFile.TokenBlocks["GLOBAL_SETUP"].PatModifyTest;
                if (patModTest == string.Empty)
                {
                    this.MsgToConsole(MsgEnum.SIO_ERROR, $"In RunSetup, PATMOD_TEST is empty for GLOBAL_SETUP.");
                    return false;
                }

                this.MsgToConsole(MsgEnum.SIO_DEBUG, $"In RunSetup, Setting G.L.S.SIOGSDS_PATMOD_TOKEN to [{token}].");
                Prime.Services.SharedStorageService.InsertRowAtTable("SIOGSDS_PATMOD_TOKEN", token, Prime.SharedStorageService.Context.LOT);
                this.MsgToConsole(MsgEnum.SIO_DEBUG, $"In RunSetup, Test=[{patModTest}] Setting Parameter=[modify_token_dynamic_subset] to [G.L.S.SIOGSDS_PATMOD_TOKEN].");
                Prime.Services.TestProgramService.SetTestInstanceParameter(patModTest, "modify_token_dynamic_subset", "G.L.S.SIOGSDS_PATMOD_TOKEN");
                this.MsgToConsole(MsgEnum.SIO_DEBUG, $"In RunSetup, Executing PatModTest=[{patModTest}].");
                var patModExit = Prime.Services.TestProgramService.ExecuteTestInstance(patModTest);
                if (patModExit == 1)
                {
                    rslt = true;
                }
                else
                {
                    rslt = false;
                    this.MsgToConsole(MsgEnum.SIO_ERROR, $"In RunSetup, PatModTest=[{patModTest}] returned [{patModExit}].");
                }
            }

            return rslt;
        }

        /// <summary>
        /// Perform a Pattern Modify.
        /// </summary>
        /// <param name="patModTest">Pattern Modify test to execute.</param>
        /// <param name="patModToken">Value for PatternModify tests modify_token_dynamic_subset parameter.</param>
        /// <returns>true on success.</returns>
        public bool ExecPatModifyTest(string patModTest, string patModToken)
        {
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Running ExecPatModifyTest({patModTest}, {patModToken})");
            string gsdsKey = "SIOGSDS_PATMOD_TOKEN";
            string gsdsLongKey = $"G.L.S.{gsdsKey}";
            Prime.Services.SharedStorageService.InsertRowAtTable(gsdsKey, patModToken, Prime.SharedStorageService.Context.LOT);

            string tokenParam = "modify_token_dynamic_subset";
            Prime.Services.TestProgramService.SetTestInstanceParameter(patModTest, tokenParam, gsdsLongKey);

            Prime.Services.TestProgramService.ExecuteTestInstance(patModTest);

            return true;
        }

        // -------------------------------------------------------------------------------------------------------------------------
        // Data compression functions
        // FIXME - consider putting these in their own class
        // -------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Wrapper function which converts a binary string into base 32, and then compresses it.
        /// </summary>
        /// <param name="binstringData">Binary string to compres.</param>
        /// <returns>Returns a container with the base32 data, compressed data and compression key.</returns>
        public CompressedData BinToBase32(string binstringData)
        {
            string bin32data = this._BinToBase32(binstringData);
            return this.ProcessPermutations(bin32data);
        }

        /// <summary>
        /// Converts a Binary string into a Base32 string.
        /// </summary>
        /// <param name="binstringData">Binary data to conver.</param>
        /// <returns>Base32 string, or an empty string on a failure to convert.</returns>
        public string _BinToBase32(string binstringData)
        {
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"BinToBase32: compressing binary string to base 32.");

            // FIXME - instead of returning string.Empty, why not throw an exception?
            // Base32 Encoding: 0:A  1:B  2:C  3:D  4:E  5:F  6:G  7:H  8:I  9:J
            //                 10:K 11:L 12:M 13:N 14:O 15:P 16:Q 17:R 18:S 19:T
            //                 20:U 21:V 22:W 23:X 24:Y 25:Z 26:2 27:3 28:4 29:5
            //                 30:6 31:7
            string codeRef = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            int codeSize = 5;  // number of binary bits per coded bit
            int totalBit = binstringData.Length;
            if (totalBit == 0)
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, "BinToBase32: Cannot convert an empty string to base32.");
                return string.Empty;
            }

            if (!SIOLib.IsBinaryRegex.IsMatch(binstringData))
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"BinToBase32: Error Data=[{binstringData}] is not binary.");
                return string.Empty;
            }

            // Pad the string to the correct length.
            int paddingBit = binstringData.Length % codeSize;
            if (paddingBit > 0)
            {
                paddingBit = codeSize - paddingBit;
                binstringData = $"{new string('0', paddingBit)}{binstringData}";
            }

            // Start the string with the encoding for the padding bit.  (FIXME - this matches EmbPython, double check that its correct)
            StringBuilder output = new StringBuilder(codeRef[paddingBit].ToString(), binstringData.Length / codeSize);
            for (int i = 0; i < binstringData.Length; i += codeSize)
            {
                output.Append(codeRef[Convert.ToInt32(binstringData.Substring(i, codeSize), 2)]);
            }

            if (this.debugMode)
            {
                this.MsgToConsole(MsgEnum.SIO_INFO, $"BinToBase32:[{binstringData}]->[{output}].");
            }

            return output.ToString();
        }

        /// <summary>
        /// Given a list of strings, returns a dictionary with the number of times each
        /// string appears in the list, for the top limit elements.
        /// Takes the place of __getCounter from EmbPython.
        /// </summary>
        /// <param name="lst">List of strings to count throug.</param>
        /// <param name="limit">Number of elemnts to return.</param>
        /// <returns>Dictionary where Key=string Value=Number of times the Key appeared in the input list.</returns>
        public Dictionary<string, int> CountListElements(string[] lst, int limit)
        {
            Dictionary<string, int> counter = new Dictionary<string, int>();

            foreach (var item in lst)
            {
                // only count strings which don't contain one of the compression chararcters.
                if (item.IndexOfAny(this.compress_keys) == -1)
                {
                    if (!counter.ContainsKey(item))
                    {
                        counter[item] = 0;
                    }

                    counter[item]++;
                }
            }

            // return the highest counts up to the limit.
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"CountListElements: {string.Join(",", counter)}");

            // if we found less elements than the limit, return them all
            if (counter.Count <= limit)
            {
                return counter;
            }

            // need to find the highest counts for "limit" items.
            Dictionary<string, int> retval = new Dictionary<string, int>(limit);
            foreach (var pair in counter.OrderByDescending(p => p.Value))
            {
                // work with pair.Key and pair.Value
                retval[pair.Key] = pair.Value;
                if (retval.Count >= limit)
                {
                    return retval;
                }
            }

            return retval;
        }

        /// <summary>
        /// For a given run type, figures out the best compression and
        /// updates the compress_container.
        /// </summary>
        /// <param name="strToCompress">Base32 string to compres.</param>
        /// <param name="runType">defines the parition length and number of elements,
        /// should be one of QUAD/TRIPLE/DOUBLE/TID.</param>
        public void GetBestPermutation(string strToCompress, string runType)
        {
            var partLength = 0;
            var maxElems = 0;
            var maxPermSum = 0;

            switch (runType)
            {
                case "QUAD":
                    partLength = 4;
                    maxElems = 5;
                    break;
                case "TRIPLE":
                    partLength = 3;
                    maxElems = 9;
                    break;
                case "DOUBLE":
                    partLength = 2;
                    maxElems = 10;
                    break;
                default:
                    partLength = 2;
                    maxElems = 10;
                    runType = "TIDY";
                    break;
            }

            for (int i = 0; i < partLength; i++)
            {
                var perm = SplitStrInParts(strToCompress, partLength, i).ToArray();
                var bestElem = this.CountListElements(perm, maxElems);
                var curPermContent = bestElem.Keys.ToArray();
                var curPermSum = bestElem.Values.Sum();
                if (curPermSum > maxPermSum)
                {
                    this.compress_container[runType] = curPermContent;
                    maxPermSum = curPermSum;
                }
            }
        }

        /// <summary>
        /// Uncompress the given string using the given translation.
        /// </summary>
        /// <param name="stringToUncompress">Compressed string to uncompress.</param>
        /// <param name="translationstring">Compression Table is a string with | separating
        /// different elements. For each element, the 1st char is the compressed character
        /// and the remainder is the uncompressed string it represents.</param>
        /// <returns>Uncompressed string.</returns>
        public string Uncompress(string stringToUncompress, string translationstring)
        {
            var retval = stringToUncompress;
            var tokens = translationstring.Split('|');
            foreach (var item in tokens)
            {
                this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Full string = {retval}");

                var compressChar = item.Substring(0, 1);
                var uncompressChars = item.Substring(1);
                this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Uncompress: Replacing [{compressChar}] with [{uncompressChars}].");
                retval = retval.Replace(compressChar, uncompressChars);
            }

            return retval;
        }

        /// <summary>
        /// Main function for compressing a base32 string.
        /// </summary>
        /// <param name="strToCompress">Base32 string to compres.</param>
        /// <returns>Compressed data and a compression key.</returns>
        public CompressedData ProcessPermutations(string strToCompress)
        {
            // reset all the global objects for compression
            this.compressKeyIndex = this.compress_keys.Length - 1;
            this.compress_container.Clear();
            this.compress_translation.Clear();

            var originalstring = strToCompress;
            foreach (string runType in this.compress_runTypeLst)
            {
                // updates compress_container[runType]
                // getBestPermutation(originalstring, runType);  // python implementation
                this.GetBestPermutation(strToCompress, runType);

                // Assigns compress_keys to compress_translation using compress_container[runType]
                if (this.compress_container.ContainsKey(runType))
                {
                    this.UpdateTranslation(this.compress_container[runType]);
                }

                // perform the compression based on the current compress_translations.
                strToCompress = this.PerformTranslation(strToCompress);
            }

            return new CompressedData(originalstring, strToCompress, this.GetTranslationAsstring());
        }

        /// <summary>
        /// Converts the translation table global into a string.
        /// Each compression element is the compression key followed by the uncompressed
        /// string it represents.  Elements are joined with | to form the final string.
        /// </summary>
        /// <returns>translation table as a string.</returns>
        public string GetTranslationAsstring()
        {
            List<string> strLst = new List<string>();

            for (int i = this.compress_keys.Length - 1; i >= 0; i--)
            {
                var newChar = this.compress_keys[i];
                if (this.compress_translation.ContainsKey(newChar))
                {
                    strLst.Add($"{newChar}{this.compress_translation[newChar]}");
                }
            }

            string retVal = string.Join("|", strLst);
            return retVal;
        }

        /// <summary>
        /// Runs each point in a shmoo.
        /// </summary>
        /// <param name="userData">UserFile.UserData struct with the shmoo.</param>
        /// <param name="currentState">Dictionary with the current state of the shmoo.</param>
        public void RunShmooSinglePoint(UserFile.UserData userData, Dictionary<string, string> currentState)
        {
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"RunShmooSinglePoint: CurrentState[RUN]=[{currentState["RUN"]}] ExecTest=[{userData.ExecuteTest}]");
            if (userData.TestInstanceIsShmoo)
            {
                var writer = Prime.Services.DatalogService.GetItuffComntWriter();
                writer.IncludeTnameInPrint(false);
                writer.SetData($"TOKEN={this.ShmooStateToToken(currentState, false)}");
                Prime.Services.DatalogService.WriteToItuff(writer);
                /* var lvl = Prime.Services.TpSettingsService.IsTpFeatureEnabled(Prime.TpSettingsService.Feature.Midas) ? 0 : 2;
                Prime.Services.DatalogService.WriteToItuff($"{lvl}_comnt_TOKEN={this.ShmooStateToToken(currentState, false)}\n"); */
            }

            var testRslt = Prime.Services.TestProgramService.ExecuteTestInstance(userData.ExecuteTest);
            if (testRslt < 0)
            {
                this.ShmooSetupErros++;
                testRslt = 0;
            }

            if (userData.TestType == "GONOGO")
            {
                userData.TestResults[0].PassCount[0] += testRslt;
            }
            else if (userData.TestType == "PERPORT" || userData.TestType == "PERLANE")
            {
                throw new TestMethodException($"TestType={userData.TestType} is currently not supported for shmoo.");
            }
            else
            {
                throw new TestMethodException($"TestType={userData.TestType} is currently not supported for shmoo.");
            }
        }

        /// <summary>
        /// This function assigns a compressionKey to each element in the
        /// given container and updates the global compress_translation element.
        /// </summary>
        /// <param name="container">List of substrings to be compresse.</param>
        private void UpdateTranslation(string[] container)
        {
            foreach (var item in container)
            {
                if (!this.compress_translation.ContainsValue(item))
                {
                    if (this.compressKeyIndex < 0)
                    {
                        return;
                    }

                    this.compress_translation[this.compress_keys[this.compressKeyIndex--]] = item;
                }
            }

            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"updateTranslation: TupleList={string.Join(",", container)}");
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"updateTranslation: Translation={string.Join(",", this.compress_translation)}");
        }

        /// <summary>
        /// Compresses the given string based on the current compress_translation table.
        /// </summary>
        /// <param name="stringToCompress">Base32 string to compres.</param>
        /// <returns>Compressed string.</returns>
        private string PerformTranslation(string stringToCompress)
        {
            var retVal = stringToCompress;
            for (int i = this.compress_keys.Length - 1; i >= 0; i--)
            {
                var newChar = this.compress_keys[i];
                if (!this.compress_translation.ContainsKey(newChar))
                {
                    continue;
                }

                var oldStr = this.compress_translation[newChar];
                retVal = retVal.Replace(oldStr, newChar.ToString());
            }

            return retVal;
        }

        /* -------------------------------------------------------------------------------------------------------------------------
         * Private Shmoo functions
         * ------------------------------------------------------------------------------------------------------------------------- */

        private void ShmooOuterLoopRecursive(UserFile.UserData userData, Dictionary<string, string> currentState, int shmooAxisIndex)
        {
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Entered ShmooOuterLoopRecursive with Index={shmooAxisIndex} currentState=[{string.Join(", ", currentState.Select(i => $"{i.Key}: {i.Value}"))}]");
            if (shmooAxisIndex >= this.ShmooOuterLoopAxisOrder.Count)
            {
                /* run the setup before the inner loop */
                userData.ShmooResults.Clear();

                /* run the inner loop */
                this.ShmooInnerLoopRecursive(userData, currentState, 0);

                /* run the cleanup after the inner loop/log the results */
                if (userData.EDCShmooEnabled && !userData.TestInstanceIsShmoo)
                {
                    this.LogShmooResult(userData, currentState);
                }
            }
            else
            {
                var axisName = this.ShmooOuterLoopAxisOrder[shmooAxisIndex];
                var shmooAxis = userData.ShmooAxis[axisName];
                foreach (var value in shmooAxis.Values)
                {
                    if (this.SetupTestParam(shmooAxis.Key, value, shmooAxis.Type, userData.ExecuteTest, userData.PatModifyTest))
                    {
                        currentState[axisName] = shmooAxis.Key != string.Empty ? $"{shmooAxis.Key}:{value}" : string.Empty;

                        // this.MsgToConsole(MsgEnum.SIO_DEBUG, $"ShmooOuterLoopRecursive: Index={shmooAxisIndex} Axis=[{axisName}] CurrentState=[{currentState[axisName]}]");
                        this.ShmooOuterLoopRecursive(userData, currentState, shmooAxisIndex + 1);
                    }
                    else
                    {
                        this.ShmooSetupErros++;
                        continue;
                    }
                }
            }
        } // end ShmooOuterLoopRecursive

        private void ShmooInnerLoopRecursive(UserFile.UserData userData, Dictionary<string, string> currentState, int shmooAxisIndex)
        {
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Entered ShmooInnerLoopRecursive with Index={shmooAxisIndex} currentState=[{string.Join(", ", currentState.Select(i => $"{i.Key}: {i.Value}"))}]");
            if (shmooAxisIndex >= this.ShmooInnerLoopAxisOrder.Count)
            {
                /* Do any Pre-Test setup */
                foreach (var testrslt in userData.TestResults)
                {
                    for (var i = 0; i < testrslt.PassCount.Count; i++)
                    {
                        testrslt.PassCount[i] = 0;
                    }
                }

                /* Run the tests. */
                for (var k = 0; k < userData.NumberOfRuns; k++)
                {
                    this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Test Run:{k} IsShmoo:{userData.TestInstanceIsShmoo}");
                    if (userData.TestInstanceIsShmoo)
                    {
                        currentState["RUN"] = $"{k}";
                    }

                    /*this.RunShmooSinglePoint(userData, currentState); */
                    userData.ShmooSingleTestPointFunc(userData, currentState);
                }

                /* Run any Post-Test Cleanup */
                if (userData.PatModReset != string.Empty)
                {
                    if (!this.ExecPatModifyTest(userData.PatModifyTest, userData.PatModReset))
                    {
                        throw new TestMethodException("Shmoo failed to reset PatModify.  Test=[{userData.patModifyTest}] Token=[{userData.patModReset}].");
                    }
                }

                // save the results.
                for (var n = 0; n < userData.TestResults.Count; n++)
                {
                    userData.ShmooResults[$"{n}_{currentState["YShmoo"]}_{currentState["XShmoo"]}"] = string.Join("-", userData.TestResults[n].PassCount);
                }
            }
            else
            {
                var axisName = this.ShmooInnerLoopAxisOrder[shmooAxisIndex];
                var shmooAxis = userData.ShmooAxis[axisName];
                foreach (var value in shmooAxis.Values)
                {
                    if (this.SetupTestParam(shmooAxis.Key, value, shmooAxis.Type, userData.ExecuteTest, userData.PatModifyTest))
                    {
                        currentState[axisName] = shmooAxis.Key != string.Empty ? $"{shmooAxis.Key}:{value}" : string.Empty;

                        // this.MsgToConsole(MsgEnum.SIO_DEBUG, $"ShmooInnerLoopRecursive: Index={shmooAxisIndex} Axis=[{axisName}] CurrentState=[{currentState[axisName]}]");
                        this.ShmooInnerLoopRecursive(userData, currentState, shmooAxisIndex + 1);
                    }
                    else
                    {
                        this.ShmooSetupErros++;
                        continue;
                    }
                }
            }
        } // end ShmooInnerLoopRecursive

        private void LogShmooResult(UserFile.UserData userData, Dictionary<string, string> currentState)
        {
            for (var n = 0; n < userData.TestResults.Count; n++)
            {
                var dlogTestName = $"{userData.DlogName}_SHMOO_{userData.TestType}_L{userData.PreSetupIteration++}_P";
                var dlogTestDataTokens = new List<string>();
                dlogTestDataTokens.Add($"TOKEN={this.ShmooStateToToken(currentState, true)}");
                dlogTestDataTokens.Add($"Plist={userData.Plist}");
                dlogTestDataTokens.Add($"RUN={userData.NumberOfRuns}");

                dlogTestDataTokens.Add($"CmpName={(userData.TestType == "GONOGO" ? "NA" : userData.TestResults[n].CmpName)}");
                dlogTestDataTokens.Add($"TestType={userData.TestType}");
                dlogTestDataTokens.Add($"NumberID={(userData.TestType == "GONOGO" ? "1" : userData.TestResults[n].CmpID.Count.ToString())}");
                dlogTestDataTokens.Add($"IDValue={(userData.TestType == "GONOGO" ? "GONOGO" : string.Join(";", userData.TestResults[n].CmpID))}");

                /*
                if (userData.TestType == "GONOGO")
                {
                    dlogTestDataTokens.Add($"CmpName=NA");
                    dlogTestDataTokens.Add($"TestType={userData.TestType}");
                    dlogTestDataTokens.Add($"NumberID=1");
                    dlogTestDataTokens.Add($"IDValue=GONOGO");
                }
                else
                {
                    dlogTestDataTokens.Add($"CmpName={userData.TestResults[n].CmpName}");
                    dlogTestDataTokens.Add($"TestType={userData.TestType}");
                    dlogTestDataTokens.Add($"NumberID={userData.TestResults[n].CmpID.Count}");
                    dlogTestDataTokens.Add($"IDValue={string.Join(";", userData.TestResults[n].CmpID)}");
                } */

                dlogTestDataTokens.Add($"YName={(string.IsNullOrWhiteSpace(userData.ShmooAxis["YShmoo"].Key) ? "NA" : userData.ShmooAxis["YShmoo"].Key)}");
                dlogTestDataTokens.Add($"YValue={(string.IsNullOrWhiteSpace(userData.ShmooAxis["YShmoo"].Key) ? "NA" : string.Join(";", userData.ShmooAxis["YShmoo"].Values))}");

                dlogTestDataTokens.Add($"XName={(string.IsNullOrWhiteSpace(userData.ShmooAxis["XShmoo"].Key) ? "NA" : userData.ShmooAxis["XShmoo"].Key)}");
                dlogTestDataTokens.Add($"XValue={(string.IsNullOrWhiteSpace(userData.ShmooAxis["XShmoo"].Key) ? "NA" : string.Join(";", userData.ShmooAxis["XShmoo"].Values))}");

                /*
                if (userData.ShmooAxis["YShmoo"].Key == string.Empty)
                {
                    dlogTestDataTokens.Add($"YName=NA");
                    dlogTestDataTokens.Add($"YValue=NA");
                }
                else
                {
                    dlogTestDataTokens.Add($"YName={userData.ShmooAxis["YShmoo"].Key}");
                    dlogTestDataTokens.Add($"YValue={string.Join(";", userData.ShmooAxis["YShmoo"].Values)}");
                }

                if (userData.ShmooAxis["XShmoo"].Key == string.Empty)
                {
                    dlogTestDataTokens.Add($"XName=NA");
                    dlogTestDataTokens.Add($"XValue=NA");
                }
                else
                {
                    dlogTestDataTokens.Add($"XName={userData.ShmooAxis["XShmoo"].Key}");
                    dlogTestDataTokens.Add($"XValue={string.Join(";", userData.ShmooAxis["XShmoo"].Values)}");
                } */

                var dataStr = "DataStart";
                foreach (var yVal in userData.ShmooAxis["YShmoo"].Values)
                {
                    var dataX = new List<string>();
                    foreach (var xVal in userData.ShmooAxis["XShmoo"].Values)
                    {
                        var dataIndex = $"{n}_{userData.ShmooAxis["YShmoo"].Key}:{yVal}_{userData.ShmooAxis["XShmoo"].Key}:{xVal}";
                        if (userData.ShmooResults.ContainsKey(dataIndex))
                        {
                            dataX.Add(userData.ShmooResults[dataIndex]);
                        }
                        else
                        {
                            // not sure if this should be a fatal error or not..FIXME
                            this.MsgToConsole(MsgEnum.SIO_ERROR, $"No results found for [{dataIndex}] allresults=[{string.Join(", ", userData.ShmooResults.Keys)}]");
                            dataX.Add("?");
                        }
                    }

                    dataStr += $"#{string.Join(";", dataX)}";
                }

                dataStr += "#DataEnd";
                dlogTestDataTokens.Add(dataStr);

                if (!this.ResultStrgValToDatalog(dlogTestName, string.Join("!", dlogTestDataTokens)))
                {
                    this.ShmooSetupErros++;
                }
            } // end n => 0 - userData.TestResults.Count
        }
    }
}