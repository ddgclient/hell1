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
    using System.Text.RegularExpressions;

    /// <summary>
    /// Main container for the User File data.
    /// </summary>
    public class UserFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserFile"/> class.
        /// </summary>
        public UserFile()
        {
            this.CompareBlocks = new Dictionary<string, CompareBlock>();
            this.TokenBlocks = new Dictionary<string, UserData>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFile"/> class.
        /// </summary>
        /// <param name="filename">Name of the file.</param>
        public UserFile(string filename)
        {
            this.CompareBlocks = new Dictionary<string, CompareBlock>();
            this.TokenBlocks = new Dictionary<string, UserData>();
            this.ReadUserFile(filename);
        }

        /// <summary>
        /// Gets or sets a value indicating whether Valid.
        /// </summary>
        public bool Valid { get; set; } = false;

        /// <summary>
        /// Gets or sets the CompareBlocks.
        /// </summary>
        public Dictionary<string, CompareBlock> CompareBlocks { get; set; }

        /// <summary>
        /// Gets or sets the TokenBlocks.
        /// </summary>
        public Dictionary<string, UserData> TokenBlocks { get; set; }

        /// <summary>
        /// Gets or sets the LocalFileName.
        /// </summary>
        public string LocalFileName { get; set; }

        /// <summary>
        /// Gets or sets the RemoteFilename.
        /// </summary>
        public string RemoteFilename { get; set; }

        private static string ParseFileToken(string remoteFileName, string value)
        {
            if (!File.Exists(value) && File.Exists(Path.Combine(remoteFileName, value)))
            {
                value = Path.Combine(remoteFileName, value);
            }

            return value;
        }

        /// <summary>
        /// Main function for reading the User File and converting it to a structure.
        /// Combines the functionality of the EmbPython GetUserFile/GetUserData/GetShmooTestSetup.
        /// </summary>
        /// <param name="remoteFileName">Name of the User File.</param>
        private void ReadUserFile(string remoteFileName)
        {
            Prime.Services.ConsoleService.PrintDebug($"Starting ReadUserFile File=[{remoteFileName}]");
            this.Valid = false;
            this.RemoteFilename = remoteFileName;

            // create a local copy of the file.
            this.LocalFileName = DDG.FileUtilities.GetFile(remoteFileName);

            var shmooRegexKeyValueType = new Regex("^([RSTUVWZXY])(KEY|VALUE)$");
            var shmooRegexParamType = new Regex("^SHMOO_([XY])PARAM_(NAME|START_VALUE|RESOLUTION|STEPS)$");

            string globalPatModifyTest = string.Empty;
            string globalPythonTest = string.Empty;

            // read the file
            using (StreamReader sr = new StreamReader(this.LocalFileName))
            {
                string line;
                int lineNumber = 0;
                bool readingCompareBlock = false;
                string compareBlockName = string.Empty;

                while ((line = sr.ReadLine()) != null)
                {
                    lineNumber++;

                    // Remove any comments and skip blank lines.
                    if (line.StartsWith("#"))
                    {
                        continue;
                    }

                    if (line.Contains("#"))
                    {
                        line = line.Split('#')[0];
                    }

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    // MsgToConsole(MsgEnum.SIO_DEBUG, $"Read line [{line}]");
                    var tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    var numTokens = tokens.Length;
                    var testId = tokens[0];

                    if (testId == "START_COMPARE_BLOCK")
                    {
                        if (numTokens < 2)
                        {
                            Prime.Services.ConsoleService.PrintError($"Invalid START_COMPARE_BLOCK on Line=[{line}] at lineNum=[{lineNumber}] File=[{this.RemoteFilename}].");
                            return;
                        }

                        readingCompareBlock = true;
                        compareBlockName = tokens[1];
                        this.CompareBlocks[compareBlockName] = new CompareBlock(compareBlockName);
                    }
                    else if (readingCompareBlock)
                    {
                        if (testId == "END_COMPARE_BLOCK")
                        {
                            compareBlockName = string.Empty;
                            readingCompareBlock = false;
                        }
                        else if (!this.CompareBlocks[compareBlockName].AssignValue(testId, tokens))
                        {
                            Prime.Services.ConsoleService.PrintError($"Invalid Line=[{line}] at lineNum=[{lineNumber}] File=[{this.RemoteFilename}] while reading COMPARE_BLOCK data.");
                            return;
                        }
                    } // end if (readingCompareBlock)
                    else if (numTokens == 3)
                    {
                        if (!this.TokenBlocks.ContainsKey(testId))
                        {
                            this.TokenBlocks[testId] = new UserData(testId);
                            this.TokenBlocks[testId].ShmooSingleTestPointFunc = null; /* TODO: make sure all users of ShmooSingleTestPointFunc set it manually. */
                        }

                        var item = tokens[1];
                        var value = tokens[2];

                        if (testId == "GLOBAL_SETUP")
                        {
                            if (item == "PATMODIFY_TEST")
                            {
                                this.TokenBlocks[testId].PatModifyTest = value;
                                globalPatModifyTest = value;
                            }
                            else if (item == "PYTHON_TEST")
                            {
                                this.TokenBlocks[testId].PythonTest = value;
                                globalPythonTest = value;
                            }
                            else
                            {
                                Prime.Services.ConsoleService.PrintError($"Invalid Line=[{line}] at lineNum=[{lineNumber}] File=[{this.RemoteFilename}] only PATMODIFY_TEST and PYTHON_TEST are valid for GLOBAL_SETUP, got [{item}].");
                                return;
                            }
                        }
                        else if (UserData.SimplePropertyTokenMap.ContainsKey(item))
                        {
                            // If its a simple assignment use the TOKEN to Property map to do it here.
                            var propertyInfo = this.TokenBlocks[testId].GetType().GetProperty(UserData.SimplePropertyTokenMap[item]);
                            if (propertyInfo.PropertyType == typeof(int))
                            {
                                try
                                {
                                    propertyInfo.SetValue(this.TokenBlocks[testId], int.Parse(value), null);
                                }
                                catch (FormatException)
                                {
                                    Prime.Services.ConsoleService.PrintError($"Invalid Line=[{line}] at lineNum=[{lineNumber}] File=[{this.RemoteFilename}] Expecting a number for {item}, not [{value}]");
                                    return;
                                }
                            }
                            else
                            {
                                propertyInfo.SetValue(this.TokenBlocks[testId], value, null);
                            }
                        }
                        else if (item == "SEQ_FILE")
                        {
                            this.TokenBlocks[testId].SeqFile = ParseFileToken(this.RemoteFilename, value);
                        }
                        else if (item == "FORMAT_FILE")
                        {
                            this.TokenBlocks[testId].FormatFile = ParseFileToken(this.RemoteFilename, value);
                        }
                        else if (shmooRegexKeyValueType.IsMatch(item))
                        {
                            // check if its a shmoo key or value
                            var match = shmooRegexKeyValueType.Match(item);
                            var shmooAxisName = match.Groups[1].Value;
                            var shmooAxisType = match.Groups[2].Value;
                            if (!this.TokenBlocks[testId].ShmooAxis.ContainsKey(shmooAxisName))
                            {
                                this.TokenBlocks[testId].ShmooAxis[shmooAxisName] = new ShmooAxis(shmooAxisName, true);
                            }

                            if (shmooAxisType == "KEY")
                            {
                                if (value.Contains(':'))
                                {
                                    var tmp = value.Split(':');
                                    if (tmp.Length != 2)
                                    {
                                        Prime.Services.ConsoleService.PrintError($"Invalid Line=[{line}] at lineNum=[{lineNumber}] File=[{this.RemoteFilename}] Expecting 2 items after split, got [{tmp.Length}].");
                                        return;
                                    }

                                    this.TokenBlocks[testId].ShmooAxis[shmooAxisName].Key = tmp[0];
                                    this.TokenBlocks[testId].ShmooAxis[shmooAxisName].Type = tmp[1].ToUpper();
                                }
                                else
                                {
                                    this.TokenBlocks[testId].ShmooAxis[shmooAxisName].Key = value;
                                    this.TokenBlocks[testId].ShmooAxis[shmooAxisName].Type = "PATMODIFY";
                                }
                            }
                            else if (shmooAxisType == "VALUE")
                            {
                                this.TokenBlocks[testId].ShmooAxis[shmooAxisName].Values = new List<string>(value.Split('!'));
                            }
                        } // end if (shmooRegexKeyValueType.IsMatch(item))
                        else if (shmooRegexParamType.IsMatch(item))
                        {
                            // check if its a shmoo parameter
                            var match = shmooRegexParamType.Match(item);
                            var shmooAxisName = match.Groups[1].Value + "Shmoo";
                            var shmooAxisType = match.Groups[2].Value;
                            if (!this.TokenBlocks[testId].ShmooAxis.ContainsKey(shmooAxisName))
                            {
                                this.TokenBlocks[testId].ShmooAxis[shmooAxisName] = new ShmooAxis(shmooAxisName, false);
                            }

                            if (shmooAxisType == "NAME")
                            {
                                if (value.Contains(':'))
                                {
                                    var tmp = value.Split(':');
                                    if (tmp.Length != 2)
                                    {
                                        Prime.Services.ConsoleService.PrintError($"Invalid Line=[{line}] at lineNum=[{lineNumber}] File=[{this.RemoteFilename}] Expecting 2 items after split, got [{tmp.Length}].");
                                        return;
                                    }

                                    this.TokenBlocks[testId].ShmooAxis[shmooAxisName].Key = tmp[0];
                                    this.TokenBlocks[testId].ShmooAxis[shmooAxisName].Type = tmp[1].ToUpper();
                                }
                                else
                                {
                                    this.TokenBlocks[testId].ShmooAxis[shmooAxisName].Key = value;
                                    this.TokenBlocks[testId].ShmooAxis[shmooAxisName].Type = "PATMODIFY";
                                }
                            }
                            else if (shmooAxisType == "START_VALUE")
                            {
                                this.TokenBlocks[testId].ShmooAxis[shmooAxisName].StartValue = double.Parse(value);
                            }
                            else if (shmooAxisType == "RESOLUTION")
                            {
                                this.TokenBlocks[testId].ShmooAxis[shmooAxisName].Resolution = double.Parse(value);
                            }
                            else if (shmooAxisType == "STEPS")
                            {
                                this.TokenBlocks[testId].ShmooAxis[shmooAxisName].NumSteps = int.Parse(value);
                            }
                        } // end if (shmooRegexParamType.IsMatch(item))
                        else if (item == "STEPKEY")
                        {
                            this.TokenBlocks[testId].StepKey = value;
                            this.TokenBlocks[testId].StepType = "PATMODIFY";
                        }
                        else if (item == "PEAKKEY")
                        {
                            this.TokenBlocks[testId].PeakKey = value;
                            this.TokenBlocks[testId].PeakType = "PATMODIFY";
                        }
                        else
                        {
                            Prime.Services.ConsoleService.PrintError($"Invalid Line=[{line}] at lineNum=[{lineNumber}] File=[{this.RemoteFilename}] Invalid Key=[{item}].");
                            return;
                        }
                    }
                    else
                    {
                        Prime.Services.ConsoleService.PrintError($"Invalid Line=[{line}] at lineNum=[{lineNumber}] File=[{this.RemoteFilename}] NOT reading COMPARE_BLOCK data and numTokens=[{numTokens}] Expected 3.");
                        return;
                    }
                } // end while ((line = sr.ReadLine()) != null)

                // fill in the python/patmodify test names from global if not specified
                if (!string.IsNullOrEmpty(globalPatModifyTest) || !string.IsNullOrEmpty(globalPythonTest))
                {
                    foreach (var item in this.TokenBlocks)
                    {
                        if (string.IsNullOrEmpty(item.Value.PatModifyTest))
                        {
                            item.Value.PatModifyTest = globalPatModifyTest;
                        }

                        if (string.IsNullOrEmpty(item.Value.PythonTest))
                        {
                            item.Value.PythonTest = globalPythonTest;
                        }
                    }
                }

                // FIXME...do any final cleanup needed (EDCLog didn't need anything)
            } // end using (StreamReader sr = new StreamReader(localFileName))

            this.Valid = true;
            return;
        }

        /// <summary>
        /// Contains a single Tokens worth of data from a User File.
        /// </summary>
        public class UserData
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserData"/> class.
            /// </summary>
            /// <param name="id">TOKEN name or id.</param>
            public UserData(string id)
            {
                this.Name = id;
                this.ShmooAxis = new Dictionary<string, ShmooAxis>();
            }

            /// <summary>
            /// Delegate to hold a function to run for each point in a shmoo.
            /// </summary>
            /// <param name="userData">SIOLib.SIOUserTokenData struct with the shmoo.</param>
            /// <param name="currentState">Dictionary with the current state of the shmoo.</param>
            public delegate void RunShmooTestPointDelegate(UserFile.UserData userData, Dictionary<string, string> currentState);

            /// <summary>
            /// Gets or sets the Name.
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the Plist.
            /// </summary>
            public string Plist { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the PatModifyTest.
            /// </summary>
            public string PatModifyTest { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the PythonTest.
            /// </summary>
            public string PythonTest { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the ModifyToken.
            /// </summary>
            public string ModifyToken { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the ModifyValue.
            /// </summary>
            public string ModifyValue { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the ExecuteTest.
            /// </summary>
            public string ExecuteTest { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the DlogName.
            /// </summary>
            public string DlogName { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the NumberOfRuns.
            /// </summary>
            public int NumberOfRuns { get; set; } = 0;

            /// <summary>
            /// Gets or sets the TestType.
            /// </summary>
            public string TestType { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the TestMode.
            /// </summary>
            public string TestMode { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the CompareBlock.
            /// </summary>
            public string CompareBlock { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the SeqFile.
            /// </summary>
            public string SeqFile { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the FormatFile.
            /// </summary>
            public string FormatFile { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the SeqId.
            /// </summary>
            public string SeqId { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the ShmooAxis.
            /// </summary>
            public Dictionary<string, ShmooAxis> ShmooAxis { get; set; }

            /// <summary>
            /// Gets or sets the StepKey.
            /// </summary>
            public string StepKey { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the StepType.
            /// </summary>
            public string StepType { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the StepStartValue.
            /// </summary>
            public string StepStartValue { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the NumberOfSteps.
            /// </summary>
            public int NumberOfSteps { get; set; } = 0;

            /// <summary>
            /// Gets or sets the StepsStartInterval.
            /// </summary>
            public string StepsStartInterval { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the PeakKey.
            /// </summary>
            public string PeakKey { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the PeakType.
            /// </summary>
            public string PeakType { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the PeakStartValue.
            /// </summary>
            public string PeakStartValue { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the NumberOfPeak.
            /// </summary>
            public int NumberOfPeak { get; set; } = 0;

            /// <summary>
            /// Gets or sets the PeakIncrementInterval.
            /// </summary>
            public string PeakIncrementInterval { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the DataRate.
            /// </summary>
            public string DataRate { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the TxEqSum.
            /// </summary>
            public string TxEqSum { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the TxEqStart.
            /// </summary>
            public string TxEqStart { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the TxEqStop.
            /// </summary>
            public string TxEqStop { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the TxEqResolution.
            /// </summary>
            public string TxEqResolution { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the TxEqPatModifyTest.
            /// </summary>
            public string TxEqPatModifyTest { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the PatModReset.
            /// </summary>
            public string PatModReset { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the TestPin.
            /// </summary>
            public string TestPin { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the ForceValue.
            /// </summary>
            public string ForceValue { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the IpoUserVar.
            /// </summary>
            public string IpoUserVar { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the RegDef.
            /// </summary>
            public string RegDef { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the LogType.
            /// </summary>
            public string LogType { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the AmGsds.
            /// </summary>
            public string AmGsds { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the AmFunc.
            /// </summary>
            public string AmFunc { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the ShmooSingleTestPointFunc.
            /// </summary>
            public RunShmooTestPointDelegate ShmooSingleTestPointFunc { get; set; }

            /// <summary>
            /// Gets or sets the TestResults.
            /// </summary>
            public List<TestResult> TestResults { get; set; } = new List<TestResult>();

            /// <summary>
            /// Gets or sets the OriginalParameters.
            /// </summary>
            public List<TestParameter> OriginalParameters { get; set; } = new List<TestParameter>();

            /// <summary>
            /// Gets or sets a value indicating whether TestInstanceIsShmoo.
            /// </summary>
            public bool TestInstanceIsShmoo { get; set; } = false;

            /// <summary>
            /// Gets or sets a value indicating whether EDCShmooEnabled.
            /// </summary>
            public bool EDCShmooEnabled { get; set; } = true;

            /// <summary>
            /// Gets or sets a value indicating whether EDCLogEnabled.
            /// </summary>
            public bool EDCLogEnabled { get; set; } = true;

            /// <summary>
            /// Gets or sets the ShmooResults.
            /// </summary>
            public Dictionary<string, string> ShmooResults { get; set; } = new Dictionary<string, string>();

            /// <summary>
            /// Gets or sets the PreSetupIteration.
            /// </summary>
            public int PreSetupIteration { get; set; } = 0;

            /// <summary>
            /// Gets or sets the PreEDCSetupIteration.
            /// </summary>
            public int PreEDCSetupIteration { get; set; } = 0;

            /// <summary>
            /// Gets the PropertyTokens
            /// Gets a map of property names to therr user file token string..
            /// </summary>
            internal static Dictionary<string, string> SimplePropertyTokenMap { get; } = new Dictionary<string, string>
            {
                { "PATMODIFY_TEST", nameof(PatModifyTest) },
                /* { "PYTHON_TEST", nameof(PythonTest) }, -- Not implmented in Prime TODO: is the PYTHON_TEST token needed? */
                { "MODIFY_TOKEN", nameof(ModifyToken) },
                { "MODIFY_VALUE", nameof(ModifyValue) },
                { "EXECUTE_TEST", nameof(ExecuteTest) },
                { "NUMOFTESTRUN", nameof(NumberOfRuns) },
                { "DLOG_NAME", nameof(DlogName) },
                { "TESTTYPE", nameof(TestType) },
                /* { "TESTMODE", nameof(TestMode) }, -- TODO: Is TESTMODE token needed? Couldn't find it used anywhere. */
                { "COMPAREBLOCK", nameof(CompareBlock) },
                { "SEQ_ID", nameof(SeqId) },
                { "STEPSTARTVALUE", nameof(StepStartValue) },
                { "STEPSTARTINTERVAL", nameof(StepsStartInterval) },
                { "NUMOFSTEP", nameof(NumberOfSteps) },
                { "PEAKSTARTVALUE", nameof(PeakStartValue) },
                { "PEAKINCREMENTINTERVAL", nameof(PeakIncrementInterval) },
                { "NUMOFPEAK", nameof(NumberOfPeak) },
                { "DATARATE", nameof(DataRate) },
                { "TXEQSUM", nameof(TxEqSum) },
                { "TXEQSTART", nameof(TxEqStart) },
                { "TXEQSTOP", nameof(TxEqStop) },
                { "TXEQRESOLUTION", nameof(TxEqResolution) },
                { "TXEQPATMODIFYTEST", nameof(TxEqPatModifyTest) },
                { "PATMODRESET", nameof(PatModReset) },
                { "TESTPIN", nameof(TestPin) },
                { "FORCEVALUE", nameof(ForceValue) },
                { "IPO_USERVAR", nameof(IpoUserVar) },
                { "REGDEF", nameof(RegDef) },
                { "LOGTYPE", nameof(LogType) },
                { "AM_GSDS", nameof(AmGsds) },
                { "AM_FUNC", nameof(AmFunc) },
            };
        }

        /// <summary>
        /// Container for a single shmoo axis.
        /// </summary>
        public class ShmooAxis
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ShmooAxis"/> class.
            /// </summary>
            /// <param name="name">Name of the Axis, should be one of R,S,T,U,V,W,Z,X,Y, XShmoo or YShmoo.</param>
            /// <param name="keyValueType">true if this shmoo is defined with a KEY/VALUE format.</param>
            public ShmooAxis(string name, bool keyValueType)
            {
                this.KeyValueType = keyValueType;
                this.Valid = false;
                this.Axis = name;
                this.Key = string.Empty;
                this.Values = new List<string>();
                this.Type = string.Empty;
                this.StartValue = 0;
                this.Resolution = 0;
                this.NumSteps = 0;
            }

            /// <summary>
            /// Gets or sets a value indicating whether Valid.
            /// </summary>
            public bool Valid { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether KeyValueType.
            /// </summary>
            public bool KeyValueType { get; set; }

            /// <summary>
            /// Gets or sets the Axis.
            /// </summary>
            public string Axis { get; set; }

            /// <summary>
            /// Gets or sets the Key.
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the Values.
            /// </summary>
            public List<string> Values { get; set; }

            /// <summary>
            /// Gets or sets the Type.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Gets or sets the StartValue.
            /// </summary>
            public double StartValue { get; set; }

            /// <summary>
            /// Gets or sets the Resolution.
            /// </summary>
            public double Resolution { get; set; }

            /// <summary>
            /// Gets or sets the NumSteps.
            /// </summary>
            public int NumSteps { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="TestParameter" />.
        /// </summary>
        public class TestParameter
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestParameter"/> class.
            /// </summary>
            /// <param name="key">Type of Parameter - TIMING or LEVEL.</param>
            /// <param name="name">Name of the Paramter.</param>
            public TestParameter(string key, string name)
            {
                this.Type = key;
                this.Name = name;
                this.Value = 0;
            }

            /// <summary>
            /// Gets or sets the Type.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Gets or sets the Name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            public double Value { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="TestResult" />.
        /// </summary>
        public class TestResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestResult"/> class.
            /// </summary>
            public TestResult()
            {
                this.CmpName = string.Empty;
                this.CmpID = new List<string>();
                this.CmpValue = new List<string>();
                this.PassCount = new List<int>();
            }

            /// <summary>
            /// Gets or sets the CmpName.
            /// </summary>
            public string CmpName { get; set; }

            /// <summary>
            /// Gets or sets the CmpID.
            /// </summary>
            public List<string> CmpID { get; set; }

            /// <summary>
            /// Gets or sets the CmpValue.
            /// </summary>
            public List<string> CmpValue { get; set; }

            /// <summary>
            /// Gets or sets the PassCount.
            /// </summary>
            public List<int> PassCount { get; set; }
        }

        /// <summary>
        /// Contains the User File data denoted by START_COMPARE_BLOCK ... END_COMPARE_BLOCK.
        /// FIXME - this isn't used for EDCLog so its not fully implemented.
        /// </summary>
        public class CompareBlock
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CompareBlock"/> class.
            /// </summary>
            /// <param name="id">Compare Block ID or NAME.</param>
            public CompareBlock(string id)
            {
                this.Name = id;
                this.MaskPins = new Dictionary<string, string>();
                this.PortCmp = new Dictionary<string, string>();
                this.LaneCmp = new Dictionary<string, string>();
                this.TestDefs = new Dictionary<string, string>();
                this.PinDefs = new Dictionary<string, string>();
            }

            /// <summary>
            /// Gets or sets the Name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the MaskPins.
            /// </summary>
            public Dictionary<string, string> MaskPins { get; set; }

            /// <summary>
            /// Gets or sets the PortCmp.
            /// </summary>
            public Dictionary<string, string> PortCmp { get; set; }

            /// <summary>
            /// Gets or sets the LaneCmp.
            /// </summary>
            public Dictionary<string, string> LaneCmp { get; set; }

            /// <summary>
            /// Gets or sets the TestDefs.
            /// </summary>
            public Dictionary<string, string> TestDefs { get; set; }

            /// <summary>
            /// Gets or sets the PinDefs.
            /// </summary>
            public Dictionary<string, string> PinDefs { get; set; }

            /// <summary>
            /// Updates value from token in file.
            /// </summary>
            /// <param name="tokenId">TokenName from file (ie PORTCOMPARE, LANECOMPARE, PINDEF, ...).</param>
            /// <param name="tokens">Values of token from file.</param>
            /// <returns>true on success, false otherwise.</returns>
            internal bool AssignValue(string tokenId, string[] tokens)
            {
                var numTokens = tokens.Length;
                if (tokenId == "PORTCOMPARE" && numTokens > 2)
                {
                    this.PortCmp[tokens[1]] = tokens[2];
                }
                else if (tokenId == "LANECOMPARE" && numTokens > 2)
                {
                    this.LaneCmp[tokens[1]] = tokens[2];
                }
                else if (tokenId == "PINDEF" && numTokens > 2)
                {
                    this.PinDefs[tokens[1]] = tokens[2];
                }
                else if (tokenId == "TESTDEF" && numTokens > 2)
                {
                    this.TestDefs[tokens[1]] = tokens[2];
                }
                else if (tokenId == "MASKPIN" && numTokens > 2)
                {
                    this.MaskPins[tokens[1]] = tokens[2];
                }
                else if (tokenId == "NAME" && numTokens > 1)
                {
                    this.Name = tokens[1];
                }
                else
                {
                    return false;
                }

                return true;
            }
        }
    }
}
