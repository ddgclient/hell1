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

namespace LSARasterTC
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using Prime.FunctionalService;
    using Prime.PatternService;
    using Prime.TestMethods;

    /// <summary> Class for executing and holding information of a PrescreenMode run. </summary>
    public class PrescreenTest
    {
        /// <summary> Character representing 1 in the bitstream. Since ctvData will always be 1 or 0, this is probably unnecessary. </summary>
        public const char PositiveChar = '1';

        /// <summary> Character representing 0 in the bitstream. Since ctvData will always be 1 or 0, this is probably unnecessary. </summary>
        public const char NegativeChar = '0';

        /// <summary> String that matches to a substring of a given fail label. </summary>
        private Regex failLabelIdentifier = new Regex("(FAIL|IO_MBD|STROBE_ME_MBD)", RegexOptions.IgnoreCase);

        private MetadataConfig.PinMappingSet pinMappingSet;
        private TestMethodsParams.String prescreenHryFlowToken;
        private TestMethodsParams.String prescreenHryFreqToken;
        private TestMethodsParams.UnsignedInteger prescreenMafLimit;
        private LSARasterTC.PrintMode prescreenPrintMode;
        private List<Tuple<string, string>> ituffArrayInfo = new List<Tuple<string, string>>();
        private MetadataConfig.ArrayType arrayType;
        private string plist;

        /// <summary> Map(SliceId -> Map(Array Name -> List of fail info)). </summary>
        private DBContainer failArrayDB;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrescreenTest"/> class.
        /// </summary>
        /// <param name="pinMappingSet"> Instance of <see cref="MetadataConfig"/> to provide metadata for execution. </param>
        /// <param name="prescreenHryFlowToken"> Prescreen HRY flow token for ituff. </param>
        /// <param name="prescreenHryFreqToken"> Prescreen HRY frequency token for ituff. </param>
        /// <param name="prescreenMAFLimit"> Limit the amount of errors printed out to ituff. </param>
        /// <param name="prescreenMapName"> Key to use to store fail DB to shared storage. </param>
        /// <param name="prescreenPrintMode"> Print mode used to determine what info is printed to ituff. </param>
        /// <param name="plist"> Name of the plist used to execute this PrescreenTest. </param>
        public PrescreenTest(MetadataConfig.PinMappingSet pinMappingSet, TestMethodsParams.String prescreenHryFlowToken, TestMethodsParams.String prescreenHryFreqToken, TestMethodsParams.UnsignedInteger prescreenMAFLimit, TestMethodsParams.String prescreenMapName, LSARasterTC.PrintMode prescreenPrintMode, string plist)
        {
            this.pinMappingSet = pinMappingSet;
            this.prescreenHryFlowToken = prescreenHryFlowToken;
            this.prescreenHryFreqToken = prescreenHryFreqToken;
            this.prescreenMafLimit = prescreenMAFLimit;
            this.prescreenPrintMode = prescreenPrintMode;
            this.plist = plist;
        }

        /// <summary>
        /// Values representing the final status of the algorithm's execution.
        /// </summary>
        public enum ExecutionStates
        {
            /// <summary>
            /// The unit failed on amble pattern.
            /// </summary>
            FailedOnAmble = 0,

            /// <summary>
            /// No errors during functional test.
            /// </summary>
            FunctionalTestPassed = 1,

            /// <summary>
            /// The unit failed on a cycle that did not contain a fail label.
            /// </summary>
            NonFailingLabelDetected = 2,

            /// <summary>
            /// Successfully parsed functional errors retrieved from functional test.
            /// </summary>
            SuccessfulAlgorithm = 3,
        }

        /// <summary> Gets or sets exit port for this test instance.</summary>
        public int ExitPort { get; set; }

        /// <summary>
        /// Prints CTVs to console, iterates through pins.
        /// </summary>
        /// <param name="ctvData">Self explanatory.</param>
        public static void PrintCtvsToConsole(Dictionary<string, string> ctvData)
        {
            Prime.Services.ConsoleService.PrintDebug("CtvData to taken from functional test:\n");
            foreach (KeyValuePair<string, string> pinToCtv in ctvData)
            {
                Prime.Services.ConsoleService.PrintDebug($"Pin: {pinToCtv.Key}\nCtv: {pinToCtv.Value}\n\n");
            }
        }

        /// <summary>
        /// PrescreenMode when in FAIL mode.
        /// </summary>
        /// <param name="testPass"> Value indicating if functional test passed with no failures. </param>
        /// <param name="failData"> DFM failures to parse and map to failing arrays. </param>
        public void Execute(bool testPass, List<IFailureData> failData)
        {
            Prime.Services.ConsoleService.PrintDebug("Beginning start of prescreen execute...");
            ExecutionStates finalState = ExecutionStates.FunctionalTestPassed;

            if (testPass)
            {
                Prime.Services.ConsoleService.PrintDebug("Functional test passed; no errors to parse");
            }
            else
            {
                this.arrayType = this.pinMappingSet.GetArrayType();
                this.failArrayDB = new DBContainer();
                finalState = this.MapDFMFailures(failData);
            }

            this.ExitPort = (int)finalState;
            this.GenerateArrayItuffData();
        }

        /// <summary>
        /// Method for printing internalDB to console.
        /// </summary>
        public void PrintInternalDbToConsole()
        {
            this.failArrayDB.PrintInternalDbToConsole(this.arrayType);
        }

        /// <summary>
        /// Return the generic structure containing all FailInfo that will be submitted to internalDB.
        /// </summary>
        /// <returns> Generic structure containing results from Prescren execute.</returns>
        /// <remarks> This should not be used in the main body of the test class functions. Used for unit test debugging...</remarks>
        public Dictionary<string, Dictionary<string, HashSet<Tuple<int, int, int>>>> ReturnInternalDB()
        {
            return this.failArrayDB.PrescreenDatabase;
        }

        /// <summary>
        /// PrescreenMode when in CTVMODE. Parses ctvData for HryString printing.
        /// </summary>
        /// <param name="testPass"> Value indicating if functional test passed with no failures. </param>
        /// <param name="failData"> DFM failures to parse and map to failing arrays. </param>
        /// <param name="ctvData"> Dictionary holding CTV data. </param>
        /// <param name="deserializedHryTable"> Deserialized HryTable for ctv decoding. </param>
        public void Execute(bool testPass, List<IFailureData> failData, Dictionary<string, string> ctvData, HryTableConfigXml deserializedHryTable)
        {
            // Execute the main algorithm first before parsing ctvData
            this.Execute(testPass, failData);

            switch (this.ExitPort)
            {
                    /* case 1:   //this code should only be needed for quicksim where CTVdata = "" if no mismatches.  on live tester it shouldn't be broken.
                    Prime.Services.ConsoleService.PrintDebug("Prescreen set to CTVMODE, port 1 = no CTV data, writing iTUFF with HRY string of all 1's...");
                    string hryString_pass = new string('1', deserializedHryTable.GetCriterias().Count);
                    DatalogToItuff(hryString_pass);
                    return;*/
                case 2:
                    Prime.Services.ConsoleService.PrintDebug("Prescreen set to CTVMODE, but PBIST engine detected failure on cycle containing non-failing label. CtvData will not be decoded...");
                    break;
                case 0:
                    Prime.Services.ConsoleService.PrintDebug("Prescreen set to CTVMODE, but PBIST engine detected failures during preamble. CtvData will not be decoded... (preamble fail, rasterconfig missing definition for array, or exception for domain with no labels)");
                    return;
            }

            Prime.Services.ConsoleService.PrintDebug("Beginning CTV decoding...");
            IsMultipleOfSize(ctvData, deserializedHryTable);
            PrintCtvsToConsole(ctvData);
            string hryString = DecodeCtvData(ctvData, deserializedHryTable);
            DatalogToItuff(hryString);
        }

        /// <summary>
        /// Method for mapping DFM failures to their respective arrays.
        /// </summary>
        /// <param name="failData"> List of <see cref="IFailureData"/> to map to array failures. </param>
        /// <returns> A enum representing the algorithm's final state. </returns>
        public ExecutionStates MapDFMFailures(List<IFailureData> failData)
        {
            Prime.Services.ConsoleService.PrintDebug("Errors detected when executing functional test. Beginning mapping process...\n");

            // Begin DFM mapping process
            foreach (var failObject in failData)
            {
                List<string> failedPins = failObject.GetFailingPinNames();
                string arrayName;

                try
                {
                    arrayName = this.GetArrayNameFromPattern(failObject);
                }
                catch (InvalidOperationException ex)
                {
                    Prime.Services.ConsoleService.PrintError(ex.Message);
                    return ExecutionStates.FailedOnAmble;
                }

                FailedArray failInfo = FailedArrayFactory.CreateFailedArray(this.arrayType, arrayName);
                Prime.Services.ConsoleService.PrintDebug($"Array fail info created for [{failInfo.ArrayName}]");

                // Only care about whether the label is failing or not if RasterMode is supported.
                if (this.pinMappingSet.IsRasterModeSupported())
                {
                    ILabel failLabelObject = null;
                    try
                    {
                        failLabelObject = Prime.Services.PatternService.GetLabelFromAddress(failObject.GetPatternName(), failObject.GetDomainName(), (uint)failObject.GetVectorAddress(), false);
                    }
                    catch
                    {
                        Prime.Services.ConsoleService.PrintError($"GetLabelFromAddress failed for pattern [{failObject.GetPatternName()}] within the domain [{failObject.GetDomainName()}], at address [{failObject.GetVectorAddress()}].  Is this a domain you were expecting failing strobes, because it needs to have labels for this to work.");
                        return ExecutionStates.FailedOnAmble;
                    }

                    bool isFailLabel = SharedFunctions.CheckLabelContains(failLabelObject.GetName(), this.failLabelIdentifier);

                    // Only labels ending in fail are valid. All others are PBIST engine errors.
                    if (!isFailLabel)
                    {
                        Prime.Services.ConsoleService.PrintError($"PBIST engine failed on a non-failing label [{failLabelObject.GetName()}] within the plist [{failObject.GetParentPlistName()}], on the pattern [{failObject.GetPatternName()}], at address [{failLabelObject.GetAddress()}].");
                        return ExecutionStates.NonFailingLabelDetected;
                    }

                    failInfo.ExtractValuesFromLabel(failLabelObject, this.pinMappingSet);
                }
                else
                {
                    Prime.Services.ConsoleService.PrintDebug("Raster mode not supported. Initializing MBD to illegal values.");
                    failInfo.MBDAddress = new Tuple<int, int, int>(-1, -1, -1);
                }

                failInfo.ExtractValuesFromPins(failedPins, this.pinMappingSet);
                failInfo.MapToInternalDB(ref this.failArrayDB);
            }

            Prime.Services.ConsoleService.PrintDebug("DFM failure parsing complete!\n");
            return ExecutionStates.SuccessfulAlgorithm;
        }

        /// <summary>
        /// Submits internal DB to shared storage using given key.
        /// </summary>
        /// <param name="prescreenMapKey"> Key to use to submit DB to shared storage. </param>
        public void SubmitDBToSharedStorage(string prescreenMapKey)
        {
            this.failArrayDB.StoreDBInStorage(prescreenMapKey);
        }

        /// <summary>
        /// Using RegEx define in MetadataConfig, get the name of the failing array from the pattern that's defined in <see cref="IFailureData"/>.
        /// </summary>
        /// <param name="failObject"> Instance of object that implements<see cref="IFailureData"/>. </param>
        /// <returns> String representing the array name if the match was successful. </returns>
        /// <remarks> This method has a conditional statement checking the string representing the name of plist. This is a temporary work around due to service not behaving as expected. </remarks>
        public string GetArrayNameFromPattern(IFailureData failObject)
        {
            var arrayNameRegex = this.pinMappingSet.GetArrayNameRegex();
            string patternName = failObject.GetPatternName();
            Match arrayMatch = arrayNameRegex.Match(patternName);

            if (!arrayMatch.Success)
            {
                // This will impact test time, but the rationale is that TT savings doesn't matter when a failure like this occurs.
                var parentPlistObj = Prime.Services.PlistService.GetPlistObject(this.plist);
                string directPlist = failObject.GetParentPlistName();

                if (parentPlistObj.IsPatternAnAmble(patternName))
                {
                    throw new InvalidOperationException($"Attempted to get an array name from pattern [{patternName}] from plist [{this.plist}], but TOS detected it as a preamble pattern. ");
                }
                else if (directPlist.ToLower().Contains("resetplb"))
                {
                    throw new InvalidOperationException($"Attempted to get an array name from pattern [{patternName}] from plist [{this.plist}], but it contains \"resetplb\"; recognized as preamble pattern. ");
                }
                else
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"Unable to determine pattern name from pattern [{patternName}] from plist [{this.plist}].\nRegex used to match to array names: [{arrayNameRegex}]");
                }
            }

            string arrayName = arrayMatch.Groups[1].Value;
            return arrayName;
        }

        private static void DatalogToItuff(string hryString)
        {
            var ituffWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            ituffWriter.SetTnamePostfix("_HRY_RAWSTR");
            ituffWriter.SetDelimiterCharacterForWrap('|');
            ituffWriter.SetData(hryString);

            Prime.Services.DatalogService.WriteToItuff(ituffWriter);
        }

        private static string DecodeCtvData(Dictionary<string, string> ctvData, HryTableConfigXml deserializedHryTable)
        {
            Prime.Services.ConsoleService.PrintDebug("Beginning decoding...");
            var criterias = deserializedHryTable.GetCriterias();
            StringBuilder hryBuilder = new StringBuilder();
            var hryCharMapping = deserializedHryTable.GetHryCharMapping();

            for (int i = 0; i < criterias.Count; i++)
            {
                var currentCriteria = criterias[i];
                HryConditionsChecker checker = new HryConditionsChecker(ctvData, currentCriteria.Condition);
                bool didPass = checker.CheckIfConditionPassed();

                if (didPass)
                {
                    char hryChar = DetermineCharForCtvData(currentCriteria, ctvData);

                    // if (hryChar.Equals(PositiveChar))
                    // {
                    //    try
                    //    {
                    //        Prime.Services.ConsoleService.PrintDebug($"Fail detected for {currentCriteria.Bypass_Global} at HRY index {currentCriteria.Hry_Index}, bypass_global = -1\n");
                    //        Prime.Services.UserVarService.SetValue(currentCriteria.Bypass_Global, "-1");
                    //    }
                    //    catch (Prime.Base.Exceptions.FatalException ex)
                    //    {
                    //        Prime.Services.ConsoleService.PrintError("Something is wrong with call to UserVar SetValue.  Are you sure your uservar collection/module name is defined?");
                    //        Prime.Services.ConsoleService.PrintError(ex.Message);
                    //    }
                    //    catch
                    //    {
                    //        Prime.Services.ConsoleService.PrintError("Something is wrong with XML config Bypass global setting.  Is it defined in xml?");
                    //    }
                    // }

                    // PDEs can map 1 or 0 to different characters if they wish
                    if (hryCharMapping != null)
                    {
                        hryChar = MapCtvUsingMapping(hryChar, hryCharMapping);
                    }

                    hryBuilder.Append(hryChar);
                }
                else
                {
                    hryBuilder.Append(currentCriteria.Hry_Output_On_Condition_Fail);
                }
            }

            return hryBuilder.ToString();
        }

        private static void IsMultipleOfSize(Dictionary<string, string> ctvData, HryTableConfigXml deserializedHryTable)
        {
            int ctvDataSize = deserializedHryTable.Algorithms.GetCtvDataSize();

            foreach (var pinToCtvData in ctvData)
            {
                Prime.Services.ConsoleService.PrintDebug($"CTV Data: {pinToCtvData.Key}{pinToCtvData.Value}");
                if (pinToCtvData.Value.Length % ctvDataSize != 0)
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"CtvData out of pin [{pinToCtvData.Key}] is not a multiple of [{ctvDataSize}]");
                }
                else if (pinToCtvData.Value.Length == 0)
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"CtvData out of pin [{pinToCtvData.Key}] is zero length");
                }
            }
        }

        private static char MapCtvUsingMapping(char hryChar, List<HryTableConfigXml.MapObject> hryCharMapping)
        {
            foreach (var map in hryCharMapping)
            {
                if (map.Ctv_Data == hryChar.ToString())
                {
                    return char.Parse(map.Hry_Data);
                }
            }

            throw new ArgumentNullException($"HryTable does not contain map for the char {hryChar}");
        }

        private static char DetermineCharForCtvData(HryTableConfigXml.CriteriaObject currentCriteria, Dictionary<string, string> ctvData)
        {
            var indexesToCheck = SharedFunctions.ParseRange(currentCriteria.Ctv_Index_Range);
            string pinData = SharedFunctions.TryGetPinData(ctvData, currentCriteria.Pin);
            char hryChar = NegativeChar;

            // OR of the indexes we're checking
            foreach (int index in indexesToCheck)
            {
                if (pinData[index].Equals(PositiveChar))
                {
                    hryChar = PositiveChar;
                    break;
                }
            }

            return hryChar;
        }

        private void GenerateArrayItuffData()
        {
            Prime.Services.ConsoleService.PrintDebug("Generating Ituff data...");

            var mrsltBuilder = Prime.Services.DatalogService.GetItuffMrsltWriter();
            string flowTokens = $"_{this.prescreenHryFlowToken}_{this.prescreenHryFreqToken}_exitPort";
            mrsltBuilder.SetTnamePostfix(flowTokens);
            mrsltBuilder.SetPrecision(0);

            if (this.ExitPort == 1)
            {
                mrsltBuilder.SetData(1);
            }
            else
            {
                mrsltBuilder.SetData(0);
            }

            Prime.Services.DatalogService.WriteToItuff(mrsltBuilder);

            if (this.prescreenPrintMode == LSARasterTC.PrintMode.PASSMODE || this.ExitPort == 1)
            {
                return;
            }

            flowTokens = $"{this.prescreenHryFlowToken}_{this.prescreenHryFreqToken}";

            var tempBuilder = Prime.Services.DatalogService.GetItuffMrsltWriter();
            foreach (var rootKeyToArrayMap in this.failArrayDB.PrescreenDatabase)
            {
                uint count = 0;
                uint arrayCount = (uint)rootKeyToArrayMap.Value.Count;
                uint mrsltValue = this.prescreenMafLimit < arrayCount ? (uint)this.prescreenMafLimit : 0;
                string currentKey = rootKeyToArrayMap.Key;

                tempBuilder.SetTnamePostfix($"_{flowTokens}_{currentKey}_MAF");
                tempBuilder.SetPrecision(0);
                tempBuilder.SetData(mrsltValue);

                foreach (var arrayNameToMBDAddress in rootKeyToArrayMap.Value)
                {
                    string arrayName = arrayNameToMBDAddress.Key;
                    tempBuilder.SetTnamePostfix($"_{arrayName}_{flowTokens}_{currentKey}_MAF");
                    tempBuilder.SetData(mrsltValue);
                    Prime.Services.DatalogService.WriteToItuff(tempBuilder);
                    count++;

                    if (count == this.prescreenMafLimit)
                    {
                        return;
                    }
                }
            }
        }
    }
}
