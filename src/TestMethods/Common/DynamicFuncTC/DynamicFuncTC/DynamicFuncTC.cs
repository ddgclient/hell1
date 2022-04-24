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

namespace DynamicFuncTC
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Prime;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PhAttributes;
    using Prime.SharedStorageService;
    using Prime.TestMethods;

    /// <summary>
    /// Plist modification type options.
    /// </summary>
    public enum UnhandledCaseActionEnum
    {
        /// <summary>
        /// Exit through port 2 without modifying any plist or UserVar.
        /// </summary>
        ExitPort2,

        /// <summary>
        /// All patterns in all pLists and bypass UserVars are enabled.
        /// </summary>
        EnableAll,
    }

    /// <summary>
    /// Dummy summary.
    /// </summary>
    [PrimeTestMethod]
    public class DynamicFuncTC : TestMethodBase
    {
        private readonly IFileHandler fileHandler = new FileHandler();
        private IConsoleService console;
        private ulong maxNumberOfFails;
        private int testIdSubStringStart;
        private int testIdSubStringLength;
        private bool isTestIdFilterInUse;
        private int failPatternLogSubStringStart;
        private int failPatternLogSubStringLength;
        private bool isFailPatternLogInUse;
        private Dictionary<string, TestTypeConfiguration> testTypeMapping;

        /// <summary>
        /// Gets or sets Patlist to execute.
        /// </summary>
        public TestMethodsParams.Plist Patlist { get; set; }

        /// <summary>
        /// Gets or sets TimingsTc for plist execution.
        /// </summary>
        public TestMethodsParams.TimingCondition TimingsTc { get; set; }

        /// <summary>
        /// Gets or sets LevelsTc to plist execution.
        /// </summary>
        public TestMethodsParams.LevelsCondition LevelsTc { get; set; }

        /// <summary>
        /// Gets or sets comma separated mask pins for Patlist execution.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString MaskPins { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the PrePlist callback to plist execution.
        /// </summary>
        public TestMethodsParams.String PrePlist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets max number of patternNames to process.
        /// </summary>
        public TestMethodsParams.UnsignedInteger MaxNumberOfFails { get; set; } = 0;

        /// <summary>
        /// Gets or sets test id positions from pattern name to match in target plist instead of full pattern name.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString TestIdSubStringFilter { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets fail pattern positions from pattern name for dataLog.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString FailPatternLogSubString { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets name of input test mapping configuration file.
        /// </summary>
        public TestMethodsParams.File TestTypeMappingFile { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets unhandled case selection, port 2 or enable all.
        /// </summary>
        public UnhandledCaseActionEnum UnhandledCaseAction { get; set; } = UnhandledCaseActionEnum.ExitPort2;

        /// <summary>
        /// Gets or sets the functional test to capture failures.
        /// </summary>
        private ICaptureFailureTest FunctionalTest { get; set; }

        /// <summary>
        /// Method to store plist name or user var value for restoring.
        /// </summary>
        /// <param name="storeKey">store key.</param>
        /// <param name="valueToStore">value to store.</param>
        public static void AddToRestoreList(string storeKey, string valueToStore)
        {
            var storeValues = new List<string>();
            if (Services.SharedStorageService.KeyExistsInObjectTable(storeKey, Context.LOT))
            {
                storeValues = (List<string>)Services.SharedStorageService.GetRowFromTable(storeKey, typeof(List<string>), Context.LOT);
            }

            if (!storeValues.Contains(valueToStore))
            {
                storeValues.Add(valueToStore);
            }

            Services.SharedStorageService.InsertRowAtTable(storeKey, storeValues, Context.LOT);
            Services.SharedStorageService.OverrideObjectRowResetPolicy(storeKey, ResetPolicy.NEVER_RESET, Context.LOT);
        }

        /// <inheritdoc />
        public override void Verify()
        {
            this.console = this.LogLevel != PrimeLogLevel.DISABLED ? Services.ConsoleService : null;
            this.maxNumberOfFails = this.MaxNumberOfFails == 0 ? ulong.MaxValue : (ulong)this.MaxNumberOfFails;
            this.FunctionalTest = Services.FunctionalService.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.maxNumberOfFails, 1, this.PrePlist);
            this.FunctionalTest.SetPinMask(this.MaskPins.ToList());

            this.isTestIdFilterInUse = false;
            var testIdArguments = this.TestIdSubStringFilter.ToList();
            if (testIdArguments.Count == 2)
            {
                this.testIdSubStringStart = int.Parse(testIdArguments[0]);
                this.testIdSubStringLength = int.Parse(testIdArguments[1]);
                this.isTestIdFilterInUse = true;
            }

            this.isFailPatternLogInUse = false;
            var failPatternLogArguments = this.FailPatternLogSubString.ToList();
            if (failPatternLogArguments.Count == 2)
            {
                this.failPatternLogSubStringStart = int.Parse(failPatternLogArguments[0]);
                this.failPatternLogSubStringLength = int.Parse(failPatternLogArguments[1]);
                this.isFailPatternLogInUse = true;
            }

            this.testTypeMapping = new Dictionary<string, TestTypeConfiguration>();
            if (!this.fileHandler.Exists(this.TestTypeMappingFile))
            {
                throw new TestMethodException($"File {this.TestTypeMappingFile} doesn't exist.");
            }

            var configurationFileContent = this.fileHandler.ReadAll(this.TestTypeMappingFile);
            this.testTypeMapping = JsonConvert.DeserializeObject<Dictionary<string, TestTypeConfiguration>>(configurationFileContent);
            foreach (var configuration in this.testTypeMapping)
            {
                this.console?.PrintDebug($"Configuration=[{configuration.Key}]");
                configuration.Value.Plist = new PlistHandler(configuration.Value.PlistNames, this.console);
                this.console?.PrintDebug($"\tBypassUserVar=[{configuration.Value.BypassUserVar}] verify value=[{Services.UserVarService.GetIntValue(configuration.Value.BypassUserVar)}]");
                AddToRestoreList("BYPASS_VARIABLES_TO_RESTORE", configuration.Value.BypassUserVar);
            }
        }

        /// <inheritdoc />
        [Returns(2, PortType.Pass, "FAIL PORT FOR UNHANDLED CASE")]
        [Returns(1, PortType.Pass, "PASS PORT")]
        [Returns(0, PortType.Fail, "FAIL PORT")]
        public override int Execute()
        {
            foreach (var configuration in this.testTypeMapping)
            {
                this.console?.PrintDebug($"Cleared patternsToKeep for configuration=[{configuration.Key}]");
                configuration.Value.Plist.ClearKeepPatternsPerPlist();
                this.SetBypassUserVar(configuration.Value.BypassUserVar, true);
            }

            this.FunctionalTest.ApplyTestConditions();
            if (this.FunctionalTest.Execute())
            {
                this.console?.PrintDebug("Functional test execution passed");
                this.ApplyModificationsForPassCase();
                return 1;
            }

            this.console?.PrintDebug("Functional test execution failed");
            this.FunctionalTest.DatalogFailure(1);
            var failData = this.FunctionalTest.GetPerCycleFailures();
            this.DataLogAllFailPatterns(failData);
            return this.ApplyModificationsForFailCase(failData);
        }

        private void DataLogAllFailPatterns(List<IFailureData> failData)
        {
            if (!this.isFailPatternLogInUse)
            {
                return;
            }

            var writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            var failList = new HashSet<string>();
            foreach (var fail in failData)
            {
                var patternName = fail.GetPatternName();
                failList.Add(patternName.Substring(this.failPatternLogSubStringStart, this.failPatternLogSubStringLength));
            }

            writer.SetData(string.Join(",", failList));
            Prime.Services.DatalogService.WriteToItuff(writer);
        }

        private void ApplyModificationsForPassCase()
        {
            foreach (var configuration in this.testTypeMapping)
            {
                this.console?.PrintDebug($"Applying changes for configuration=[{configuration.Key}]");
                this.SetBypassUserVar(configuration.Value.BypassUserVar, true);
            }
        }

        private void SetBypassUserVar(string userVar, bool isBypass)
        {
            var bypassValue = isBypass ? 1 : -1;
            this.console?.PrintDebug($"\tSetting bypassPort UserVar=[{userVar}] to value=[{bypassValue}]");
            Services.UserVarService.SetValue(userVar, bypassValue);
        }

        private int ApplyModificationsForFailCase(List<IFailureData> failData)
        {
            if ((uint)failData.Count >= this.maxNumberOfFails)
            {
                return this.ExecuteUnhandledCaseAction();
            }

            if (this.isTestIdFilterInUse)
            {
                if (!this.GetKeepFailTestIdsPatternsPerPlist(failData))
                {
                    return this.ExecuteUnhandledCaseAction();
                }
            }
            else
            {
                if (!this.GetFailPatternNamesPerPlist(failData))
                {
                    return this.ExecuteUnhandledCaseAction();
                }
            }

            var isThereAnyModificationApplied = false;
            foreach (var configuration in this.testTypeMapping)
            {
                this.console?.PrintDebug($"Applying changes for configuration=[{configuration.Key}]");
                if (configuration.Value.Plist.ApplyPlistOptionsModifications())
                {
                    isThereAnyModificationApplied = true;
                    this.SetBypassUserVar(configuration.Value.BypassUserVar, false);
                }
                else
                {
                    this.SetBypassUserVar(configuration.Value.BypassUserVar, true);
                }
            }

            return isThereAnyModificationApplied ? 0 : this.ExecuteUnhandledCaseAction();
        }

        private int ExecuteUnhandledCaseAction()
        {
            if (this.UnhandledCaseAction == UnhandledCaseActionEnum.ExitPort2)
            {
                return 2;
            }

            if (this.UnhandledCaseAction == UnhandledCaseActionEnum.EnableAll)
            {
                foreach (var configuration in this.testTypeMapping)
                {
                    this.console?.PrintDebug($"Applying changes for configuration=[{configuration.Key}]");
                    this.SetBypassUserVar(configuration.Value.BypassUserVar, false);
                    configuration.Value.Plist.EnableAllPatterns();
                }
            }

            return 0;
        }

        private bool GetFailPatternNamesPerPlist(List<IFailureData> failData)
        {
            foreach (var fail in failData)
            {
                var isThereAnyMatch = false;
                var patternName = fail.GetPatternName();
                foreach (var configuration in this.testTypeMapping)
                {
                    var testType = patternName.Substring(configuration.Value.TestTypeSubStringStart, configuration.Value.TestTypeSubStringLength);
                    if (testType == configuration.Value.TestTypeToMatch)
                    {
                        this.console?.PrintDebug($"Keeping match Pattern=[{patternName}] TestType=[{configuration.Value.TestTypeSubStringStart}|{configuration.Value.TestTypeSubStringLength}|{configuration.Value.TestTypeToMatch}] Configuration={configuration.Key}");
                        configuration.Value.Plist.AddMatchingPatternsPerPlist(patternName);
                        isThereAnyMatch = true;
                    }
                }

                if (!isThereAnyMatch)
                {
                    return false;
                }
            }

            return true;
        }

        private bool GetKeepFailTestIdsPatternsPerPlist(List<IFailureData> failData)
        {
            foreach (var fail in failData)
            {
                var isThereAnyMatch = false;
                var patternName = fail.GetPatternName();
                foreach (var configuration in this.testTypeMapping)
                {
                    var testType = patternName.Substring(configuration.Value.TestTypeSubStringStart, configuration.Value.TestTypeSubStringLength);
                    if (testType == configuration.Value.TestTypeToMatch)
                    {
                        var testId = patternName.Substring(this.testIdSubStringStart, this.testIdSubStringLength);
                        this.console?.PrintDebug($"Keeping match fail pattern=[{patternName}] TestType=[{configuration.Value.TestTypeSubStringStart}|{configuration.Value.TestTypeSubStringLength}|{configuration.Value.TestTypeToMatch}] Configuration=[{configuration.Key}]");
                        configuration.Value.Plist.AddMatchingSubStringPatternsPerPlist(testId, this.testIdSubStringStart, this.testIdSubStringLength);
                        isThereAnyMatch = true;
                    }
                }

                if (!isThereAnyMatch)
                {
                    return false;
                }
            }

            return true;
        }
    }
}