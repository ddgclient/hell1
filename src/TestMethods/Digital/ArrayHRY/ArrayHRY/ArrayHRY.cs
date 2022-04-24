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

namespace ArrayHRY
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PhAttributes;
    using Prime.SharedStorageService;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class ArrayHRY : TestMethodBase
    {
        /// <summary>
        /// Controls whether the raw string should be stored (PRE) or retrieved (POST) when using raw string forwarding mode.
        /// </summary>
        public enum ForwardingMode
        {
            /// <summary>
            /// PRE Mode. HRY string is stored in shared storage.
            /// </summary>
            PRE,

            /// <summary>
            /// POST Mode. HRY string is retrieved from shared storage and combined with the test results.
            /// </summary>
            POST,
        }

        /// <summary>
        /// Gets or sets the Configuration File.
        /// </summary>
        public TestMethodsParams.String ConfigFile { get; set; }

        /// <summary>
        /// Gets or sets Patlist to execute.
        /// </summary>
        public TestMethodsParams.Plist Patlist { get; set; }

        /// <summary>
        /// Gets or sets TimingsTc for plist execution.
        /// </summary>
        public TestMethodsParams.TimingCondition TimingsTc { get; set; }

        /// <summary>
        /// Gets or sets LevelsTc for plist execution.
        /// </summary>
        public TestMethodsParams.LevelsCondition LevelsTc { get; set; }

        /// <summary>
        /// Gets or sets the PrePlist callback to plist execution.
        /// </summary>
        public TestMethodsParams.String PrePlist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets comma separated pins for mask.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString MaskPins { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Raw String forwarding mode.
        /// </summary>
        public ForwardingMode RawStringForwardingMode { get; set; } = ForwardingMode.PRE;

        /// <summary>
        /// Gets or sets the Key to be used with Raw STring Forwarding mode. If empty the mode is disabled.
        /// </summary>
        public TestMethodsParams.String SharedStorageKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        private HryConfiguration Configuration { get; set; }

        private ICaptureCtvPerPinTest CaptureCtvTest { get; set; }

        private bool CtvCapturedNonZeroBit { get; set; }

        private bool ConditionFailed { get; set; }

        private Dictionary<string, int> BypassGlobalsValueMap { get; set; }

        private FileHelper XmlConfigurationFileHandler { get; } = new FileHelper();

        private FileHelper XmlSchemaFileHandler { get; } = new FileHelper();

        private XmlConfigFile XmlConfiguration { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;

            // only reread the XML file if something has changed or we're in debug mode.
            var schemaChanged = this.XmlSchemaFileHandler.HasFileChanged(XmlConfigFile.SchemaPath, out var localSchemaPath);
            var xmlConfigFileChanged = this.XmlConfigurationFileHandler.HasFileChanged(this.ConfigFile, out var xmlConfigurationFile);

            if (this.XmlConfiguration == null || schemaChanged || xmlConfigFileChanged || this.LogLevel != PrimeLogLevel.DISABLED)
            {
                this.XmlConfiguration = XmlConfigFile.LoadFile(xmlConfigurationFile, localSchemaPath);
                this.Configuration = new HryConfiguration(this.XmlConfiguration);
                this.BypassGlobalsValueMap = this.Configuration.BypassGlobalsToInit.ToDictionary(x => x, y => 1);
            }

            // always re-create the Test.
            this.CaptureCtvTest = Prime.Services.FunctionalService.CreateCaptureCtvPerPinTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.Configuration.PinsToCapture, this.PrePlist);
        }

        /// <inheritdoc />
        [Returns(3, PortType.Pass, "HRY ACCEPTANCE CONDITION FAIL")]
        [Returns(2, PortType.Pass, "HRY CTV FAIL")]
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            // Reset runtime variables.
            this.CtvCapturedNonZeroBit = false;
            this.ConditionFailed = false;

            // Reset all bypass globals to 1.
            foreach (var bypassGlobal in this.Configuration.BypassGlobalsToInit)
            {
                this.BypassGlobalsValueMap[bypassGlobal] = 1;
                this.Console?.PrintDebug($"Setting userVar name=[{bypassGlobal}] value=[{this.BypassGlobalsValueMap[bypassGlobal]}].");
                Prime.Services.UserVarService.SetValue(bypassGlobal, this.BypassGlobalsValueMap[bypassGlobal]);
            }

            // Execute Patlist
            this.CaptureCtvTest.ApplyTestConditions();
            this.CaptureCtvTest.SetPinMask(this.MaskPins);
            this.CaptureCtvTest.Execute();
            var ctv = this.CaptureCtvTest.GetCtvData();

            // validate the Total CTV capture size.
            this.Configuration.VerifyCaptureData(ctv);

            // Decode the CTV for each algorithm.
            var ctvPerAlgorithm = this.GetCtvPerAlgorithmPerPin(ctv);
            foreach (var algorithm in this.Configuration.Algorithms)
            {
                // Build the raw HRY string.
                var hryAsList = this.DecodeCtvForOneAlgorithm(ctvPerAlgorithm[algorithm.Name]);

                // If the "POST" mode is enabled, combine the raw HRY string with the "PRE" HRY string.
                this.Console?.PrintDebug($"Test HRY   : Algorithm=[{algorithm.Name}] HRY=[{string.Join(string.Empty, hryAsList)}]");
                List<char> finalHryAsList;
                if (!string.IsNullOrWhiteSpace(this.SharedStorageKey) && this.RawStringForwardingMode == ForwardingMode.POST)
                {
                    var key = $"{this.SharedStorageKey}_{algorithm.Name}";
                    var forwardedHry = Prime.Services.SharedStorageService.GetStringRowFromTable(key, Context.DUT);
                    this.Console?.PrintDebug($"Forward HRY: Algorithm=[{algorithm.Name}] HRY=[{forwardedHry}]");

                    finalHryAsList = this.CombinePreAndPostHryData(forwardedHry, hryAsList);
                    this.Console?.PrintDebug($"Final HRY  : Algorithm=[{algorithm.Name}] HRY=[{string.Join(string.Empty, finalHryAsList)}]");
                }
                else
                {
                    finalHryAsList = hryAsList;
                }

                // Output the final HRY string.
                Utilities.DatalogHryStrgval(algorithm.Name, finalHryAsList, 3900);

                // If the "PRE" mode is enabled, save the HRY string.
                if (!string.IsNullOrWhiteSpace(this.SharedStorageKey) && this.RawStringForwardingMode == ForwardingMode.PRE)
                {
                    var hryAsString = string.Join(string.Empty, finalHryAsList);
                    var key = $"{this.SharedStorageKey}_{algorithm.Name}";
                    this.Console?.PrintDebug($"RawStringForwarding PRE Mode: Saving HRY=[{hryAsString}] to SharedStorage=[{key}]");
                    Prime.Services.SharedStorageService.InsertRowAtTable(key, hryAsString, Context.DUT);
                }
            }

            // Update any bypass globals with an HRY bit=1.
            foreach (var globalValuePair in this.BypassGlobalsValueMap)
            {
                if (globalValuePair.Value != 1)
                {
                    this.Console?.PrintDebug($"Setting userVar name=[{globalValuePair.Key}] value=[{globalValuePair.Value}].");
                    Prime.Services.UserVarService.SetValue(globalValuePair.Key, globalValuePair.Value);
                }
            }

            // calculate the return value.
            if (this.CtvCapturedNonZeroBit)
            {
                return 2;
            }
            else if (this.ConditionFailed)
            {
                return 3;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Convert CTV to HRY for one algorithm.
        /// </summary>
        /// <param name="ctv">CTV for one algorithm. Keys=Pin Names, Values=CTV for this algorithm only.</param>
        /// <returns>HRY as list of char.</returns>
        private List<char> DecodeCtvForOneAlgorithm(Dictionary<string, string> ctv)
        {
            List<char> hryAsList = new List<char>(this.Configuration.Criterias.Count);
            foreach (var criteria in this.Configuration.Criterias)
            {
                // Check the condition.
                var failCondition = this.IsFailCondition(criteria, ctv, out var hryChar);

                if (!failCondition)
                {
                    if (criteria.FixedLengthMode)
                    {
                        hryChar = HryConfiguration.FixedLengthPassChar;
                    }
                    else
                    {
                        hryChar = this.MapCtvToHryAndUpdateGlobalsForPassingCondition(criteria, ctv);
                    }
                }

                hryAsList.Add(hryChar);
            }

            return hryAsList;
        }

        /// <summary>
        /// Checks if the given critera has a failing condition for the given ctv and updates the HRY character
        /// and ConditionFailed global if it does.
        /// </summary>
        /// <param name="criteria">critera object.</param>
        /// <param name="ctv">CTV data for one algorithm.</param>
        /// <param name="hryChar">(output) HRY character if there was a condition fail.</param>
        /// <returns>Returns true if the condition failed, also updates hryChar and ConditionFailed global.</returns>
        private bool IsFailCondition(HryConfiguration.Criteria criteria, Dictionary<string, string> ctv, out char hryChar)
        {
            foreach (var condition in criteria.Conditions)
            {
                var ctvDataForCondition = Utilities.GetSubStringWithStartAndLengthTuple(ctv[condition.Pin], condition.CtvIndexes);
                if (ctvDataForCondition != condition.PassingBinaryValue)
                {
                    this.ConditionFailed = true;
                    if (!string.IsNullOrEmpty(condition.FailKey) && this.Configuration.ConditionFailKeyMap[condition.FailKey].ContainsKey(ctvDataForCondition))
                    {
                        hryChar = this.Configuration.ConditionFailKeyMap[condition.FailKey][ctvDataForCondition];
                    }
                    else
                    {
                        hryChar = criteria.HryOutputOnConditionFail;
                    }

                    return true;
                }
            }

            hryChar = '-';
            return false;
        }

        /// <summary>
        /// Maps the CTV to HRY for the given critera, assuming all conditions passed. Also updates the
        /// CtvCapturedNonZeroBit and BypassGlobalsValueMap globals if there was an HRY fail.
        /// </summary>
        /// <param name="criteria">critera object.</param>
        /// <param name="ctv">CTV data for one algorithm.</param>
        /// <returns>HRY character, plus CtvCapturedNonZeroBit and BypassGlobalsValueMap globals are updated.</returns>
        private char MapCtvToHryAndUpdateGlobalsForPassingCondition(HryConfiguration.Criteria criteria, Dictionary<string, string> ctv)
        {
            foreach (var ctvBit in Utilities.GetSubStringWithStartAndLengthTuple(ctv[criteria.Pin], criteria.CtvIndexes))
            {
                if (ctvBit == HryConfiguration.CtvCharForFail)
                {
                    this.CtvCapturedNonZeroBit = true;
                    foreach (var global in criteria.BypassGlobals)
                    {
                        this.BypassGlobalsValueMap[global] = -1;
                    }

                    return this.Configuration.CtvToHryMapping[ctvBit];
                }
            }

            return this.Configuration.CtvToHryMapping[HryConfiguration.CtvCharForPass];
        }

        /// <summary>
        /// Split the CTV data per-algorithm.
        /// </summary>
        /// <param name="ctvPerPin">Dictionary of CTV data per-pin.</param>
        /// <returns>Dictionary of CTV data per-algorithm/per-pin.</returns>
        private Dictionary<string, Dictionary<string, string>> GetCtvPerAlgorithmPerPin(Dictionary<string, string> ctvPerPin)
        {
            var retval = new Dictionary<string, Dictionary<string, string>>(this.Configuration.Algorithms.Count);
            foreach (var algorithm in this.Configuration.Algorithms)
            {
                retval.Add(algorithm.Name, new Dictionary<string, string>(ctvPerPin.Count));
            }

            foreach (var pin in ctvPerPin.Keys)
            {
                var algorithmOffset = 0;
                var pinData = this.Configuration.ReverseCtvCaptureData ? Utilities.StringReverse(ctvPerPin[pin]) : ctvPerPin[pin];
                foreach (var algorithm in this.Configuration.Algorithms)
                {
                    var data = this.Configuration.Algorithms.Count == 1 ? pinData : pinData.Substring(algorithmOffset, (int)algorithm.PerPinCtvSize[pin]);
                    this.Console?.PrintDebug($"Capture data from pin=[{pin}] for algorithm=[{algorithm.Name}] is:\n\t\t[{data}]");
                    retval[algorithm.Name].Add(pin, data);
                    algorithmOffset += (int)algorithm.PerPinCtvSize[pin];
                }
            }

            return retval;
        }

        /// <summary>
        /// Combine the PRE and POST (current) HRY strings.
        /// </summary>
        /// <param name="forwardedHry">HRY from PRE instance (as a string).</param>
        /// <param name="hryFromCurrentTest">HRY from POST (current) instance (as a list of char).</param>
        /// <returns>List of char containing the combined HRY data.</returns>
        private List<char> CombinePreAndPostHryData(string forwardedHry, List<char> hryFromCurrentTest)
        {
            if (hryFromCurrentTest.Count != forwardedHry.Length)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"Forwarded HRY has [{forwardedHry.Length}] bits while this tests HRY has [{hryFromCurrentTest.Count}]. SharedStorageKey is probably not correct.");
            }

            var finalHryAsList = new List<char>(forwardedHry);
            for (var i = 0; i < hryFromCurrentTest.Count; i++)
            {
                if (!this.Configuration.HryCharIsPassStatus.ContainsKey(hryFromCurrentTest[i]))
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"Test HRY char=[{hryFromCurrentTest[i]}] does not exist in the HryPrePostMapping field.");
                }

                if (!this.Configuration.HryCharIsPassStatus.ContainsKey(finalHryAsList[i]))
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"Forward HRY char=[{finalHryAsList[i]}] does not exist in the HryPrePostMapping field.");
                }

                // Pass->Fail = use Fail character.
                // Fail->Pass = use Repair character.
                // Fail->Fail or Pass->Pass use forwarded character. (which is the initialized value)
                if (this.Configuration.HryCharIsPassStatus[finalHryAsList[i]] && !this.Configuration.HryCharIsPassStatus[hryFromCurrentTest[i]])
                {
                    finalHryAsList[i] = hryFromCurrentTest[i];
                }
                else if (!this.Configuration.HryCharIsPassStatus[finalHryAsList[i]] && this.Configuration.HryCharIsPassStatus[hryFromCurrentTest[i]])
                {
                    finalHryAsList[i] = this.Configuration.PostRepairSymbol;
                }
            }

            return finalHryAsList;
        }
    }
}