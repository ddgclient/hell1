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

namespace PValTC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Prime.ConsoleService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FunctionalService;
    using Prime.PhAttributes;
    using Prime.PlistService;
    using Prime.TestMethods;

    /// <summary>
    /// This class finds test content aggressors.
    /// </summary>
    [PrimeTestMethod]
    public class PValTC : TestMethodBase
    {
        private IFailDataFormat failDataWriter;

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
        /// Gets or sets the PrePlist callback to plist execution.
        /// </summary>
        public TestMethodsParams.String PrePlist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets comma separated mask pins for Patlist execution.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString MaskPins { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the functional test to capture failures.
        /// </summary>
        protected ICaptureFailureTest FunctionalTest { get; set; }

        /// <summary>
        /// Gets or sets the pattern index for Patlist.
        /// </summary>
        protected PlistElement ParentPlist { get; set; }

        /// <summary>
        /// Gets or sets plist interface.
        /// </summary>
        protected IPlistObject Plist { get; set; }

        /// <summary>
        /// Gets or sets list of disabled patterns.
        /// </summary>
        protected HashSet<string> DisabledPatterns { get; set; }

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;
            this.FunctionalTest = Prime.Services.FunctionalService.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 99999, 1, this.PrePlist);
            this.FunctionalTest.SetPinMask(this.MaskPins.ToList());
            this.Plist = Prime.Services.PlistService.GetPlistObject(this.Patlist);
            this.ParentPlist = this.GetPlistElement(this.Patlist, 0, "Plist", 0);
            this.failDataWriter = Prime.Services.DatalogService.GetItuffFailDataWriter();
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Passed.")]
        [Returns(0, PortType.Fail, "Failed.")]
        public override int Execute()
        {
            this.ExecuteTime0();
            this.ExecuteBurstOff();
            this.ExecuteN2(this.ParentPlist);
            this.Plist.EnableAllPatterns();
            return 1;
        }

        private PlistElement GetPlistElement(string name, int position, string type, int burst)
        {
            var result = new PlistElement(name, position, type, burst);
            if (type.ToLower() == "pattern")
            {
                return result;
            }

            var patternsIndex = TOSUserSDK.Plist.Service.GetPatternsAndIndexesInPlist(name, false);
            foreach (var child in patternsIndex.Select(element => this.GetPlistElement(element.Item1, element.Item2, element.Item3, element.Item4)))
            {
                if (result.Children == null)
                {
                    result.Children = new List<PlistElement>();
                }

                result.Children.Add(child);
            }

            return result;
        }

        private void ExecuteTime0()
        {
            this.DisabledPatterns = new HashSet<string>();
            this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Applying test conditions.");
            this.FunctionalTest.ApplyTestConditions();
            this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Enabling all patterns.");
            this.Plist.EnableAllPatterns();
            this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Executing Patlist={this.Patlist}.");
            if (!this.FunctionalTest.Execute())
            {
                Prime.Services.ConsoleService.PrintError($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: {nameof(this.Patlist)}=[{this.Patlist}] failed before executing PVal.");
                var failingPatterns = this.ExtractFailingData("_Time0");
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: {nameof(this.Patlist)}=[{this.Patlist}] removing failing patterns.");
                this.Plist.DisablePatterns(failingPatterns);
                foreach (var p in failingPatterns)
                {
                    this.DisabledPatterns.Add(p);
                }
            }
            else
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Patlist={this.Patlist} passed.");
            }
        }

        private void ExecuteBurstOff()
        {
            this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Setting BurstOffDeep for Patlist={this.Patlist}.");
            this.Plist.SetOption("Burst", "BurstOffDeep");
            this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Executing Patlist={this.Patlist}.");
            if (!this.FunctionalTest.Execute())
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: {nameof(this.Patlist)}=[{this.Patlist}] failed BurstOffDeep. Reading failing patterns:");
                var failingPatterns = this.ExtractFailingData("_BurstOff");
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: {nameof(this.Patlist)}=[{this.Patlist}] removing failing patterns.");
                this.Plist.DisablePatterns(failingPatterns);
                foreach (var p in failingPatterns)
                {
                    this.DisabledPatterns.Add(p);
                }
            }
            else
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Patlist={this.Patlist} passed.");
            }

            this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Removing Burst option for Patlist={this.Patlist}.");
            if (!TOSUserSDK.Plist.Service.RemovePlistOptions(this.Patlist, new List<string> { "BurstOffDeep" }))
            {
                throw new Exception("Unable to set Burst mode on.");
            }
        }

        private void ExecuteN2(PlistElement plistElement)
        {
            if (plistElement.Type == PlistElement.PlistElementType.Pattern)
            {
                return;
            }

            var plistOptions = TOSUserSDK.Plist.Service.GetPlistOptions(plistElement.Name);

            foreach (var element in plistElement.Children)
            {
                if (element.Type == PlistElement.PlistElementType.PList)
                {
                    this.ExecuteN2(element);
                }
                else if (!this.DisabledPatterns.Contains(element.Name))
                {
                    this.SetPrePattern(element.Name, plistElement.Name, plistOptions);
                    this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Executing Patlist={this.Patlist} with PrePattern=[{element.Name}].");
                    if (!this.FunctionalTest.Execute())
                    {
                        var failingPatterns = this.ExtractFailingData("_" + element.Name.Substring(0, Math.Min(element.Name.Length, 16)));
                        this.Console?.PrintDebug($" -- Fail: {nameof(this.Patlist)}=[{this.Patlist}] Aggressor=[{element.Name}] Victim=[{string.Join(",", failingPatterns)}].");
                    }

                    this.RestorePlist(plistElement.Name, plistOptions);
                }
            }
        }

        private void SetPrePattern(string prePattern, string patlist, List<Tuple<string, string>> options)
        {
            var plist = Prime.Services.PlistService.GetPlistObject(patlist);
            var currentPrePattern = options.Find(o => o.Item1 == "PrePattern");
            if (currentPrePattern != null && !string.IsNullOrEmpty(currentPrePattern.Item2))
            {
                var newPrePlist = this.ClonePlist(patlist, "_N2");
                this.EmptyPlist(newPrePlist);
                this.RemovePlistOptions(newPrePlist);
                this.AddPatternsToPlist(newPrePlist, new List<string> { currentPrePattern.Item2, prePattern, currentPrePattern.Item2 });
                this.RemovePrePatternOption(patlist);
                plist.SetOption("PrePList", newPrePlist);
                return;
            }

            var currentPrePlist = options.Find(o => o.Item1 == "PrePList");
            if (currentPrePlist != null && !string.IsNullOrEmpty(currentPrePlist.Item2))
            {
                var newPrePlist = this.ClonePlist(currentPrePlist.Item2, "_N2");
                this.InsertPatternInMiddleOfPrePlist(currentPrePlist.Item2, newPrePlist, prePattern);
                plist.SetOption("PrePList", newPrePlist);
                return;
            }

            plist.SetOption("PrePattern", prePattern);
        }

        private void RestorePlist(string patlist, List<Tuple<string, string>> options)
        {
            if (options.Count == 0)
            {
                return;
            }

            if (!TOSUserSDK.Plist.Service.RemovePlistOptions(patlist, new List<string> { "PrePattern", "PrePList" }))
            {
                throw new Exception($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: failed removing PrePattern option for Patlist=[{patlist}].");
            }

            var nonEmptyOptions = options.Select(o => o).Where(o => !string.IsNullOrEmpty(o.Item2)).ToList();
            if (!TOSUserSDK.Plist.Service.SetPlistOptions(patlist, nonEmptyOptions))
            {
                throw new Exception($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: unable to restore plist options for Patlist=[{patlist}].");
            }

            var emptyOptions = options.Select(o => o).Where(o => string.IsNullOrEmpty(o.Item2)).Select(o => o.Item1).ToList();
            if (emptyOptions.Count > 1 && !TOSUserSDK.Plist.Service.RemovePlistOptions(patlist, emptyOptions))
            {
                throw new Exception($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: failed removing PrePattern option for Patlist=[{patlist}].");
            }

            TOSUserSDK.Plist.Service.ResolvePlist(patlist);
        }

        private HashSet<string> ExtractFailingData(string postFix)
        {
            this.failDataWriter.SetTnamePostfix(postFix);
            var failingPatterns = new HashSet<string>();
            var failCycles = this.FunctionalTest.GetPerCycleFailures();
            foreach (var failCycle in failCycles)
            {
                var pattern = failCycle.GetPatternName();
                var cycle = Convert.ToInt32(failCycle.GetCycle());
                var rma = Convert.ToInt32(failCycle.GetVectorAddress());
                var failingChannels = failCycle.GetFailingPinChannels();
                this.failDataWriter.SetData(pattern, rma, -1, -1, cycle, failingChannels);
                Prime.Services.DatalogService.WriteToItuff(this.failDataWriter);
                failingPatterns.Add(pattern);
            }

            return failingPatterns;
        }

        private void EmptyPlist(string patlist)
        {
            var temporaryPrePlistContents = TOSUserSDK.Plist.Service.GetPatternsAndIndexesInPlist(patlist, false);
            for (var i = 0; i < temporaryPrePlistContents.Count; ++i)
            {
                TOSUserSDK.Plist.Service.RemoveItemFromPList(patlist, 0);
            }

            TOSUserSDK.Plist.Service.ResolvePlist(patlist);
        }

        private void RemovePlistOptions(string patlist)
        {
            var plistOptions = TOSUserSDK.Plist.Service.GetPlistOptions(patlist);
            var optionNames = plistOptions.Select(o => o.Item1).Where(o => o != "Burst").ToList();
            if (!TOSUserSDK.Plist.Service.RemovePlistOptions(patlist, optionNames))
            {
                throw new Exception($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: unabled to remove all plist options for plist=[{patlist}].");
            }

            TOSUserSDK.Plist.Service.ResolvePlist(patlist);
        }

        private string ClonePlist(string patlist, string postFix)
        {
            if (Prime.Services.PlistService.Exists(patlist + postFix))
            {
                return patlist + postFix;
            }

            if (!TOSUserSDK.Plist.Service.CopyPlist(patlist, patlist + postFix))
            {
                throw new Exception($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: failed copying Patlist=[{patlist}].");
            }

            return patlist + postFix;
        }

        private void AddPatternsToPlist(string patlist, List<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                if (!TOSUserSDK.Plist.Service.AddPatternToPList(patlist, pattern))
                {
                    throw new Exception($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: failed adding pattern=[{pattern}] to Patlist=[{patlist}].");
                }
            }

            TOSUserSDK.Plist.Service.ResolvePlist(patlist);
        }

        private void RemovePrePatternOption(string patlist)
        {
            var optionNames = new List<string> { "PrePattern" };
            if (!TOSUserSDK.Plist.Service.RemovePlistOptions(patlist, optionNames))
            {
                throw new Exception($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: unable to remove PrePattern for plist=[{patlist}].");
            }

            TOSUserSDK.Plist.Service.ResolvePlist(patlist);
        }

        private void InsertPatternInMiddleOfPrePlist(string originalPatlist, string newPatlist, string pattern)
        {
            var prePlistPatterns = new List<string>();
            var plistContents = TOSUserSDK.Plist.Service.GetPatternsAndIndexesInPlist(originalPatlist, false);
            this.EmptyPlist(newPatlist);
            prePlistPatterns.AddRange(plistContents.Select(o => o.Item1));
            prePlistPatterns.Add(pattern);
            prePlistPatterns.AddRange(plistContents.Select(o => o.Item1));
            foreach (var p in prePlistPatterns.Where(p => !TOSUserSDK.Plist.Service.AddPatternToPList(newPatlist, p)))
            {
                throw new Exception($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: failed adding pattern=[{p}] to Patlist=[{newPatlist}].");
            }

            TOSUserSDK.Plist.Service.ResolvePlist(newPatlist);
        }
    }
}