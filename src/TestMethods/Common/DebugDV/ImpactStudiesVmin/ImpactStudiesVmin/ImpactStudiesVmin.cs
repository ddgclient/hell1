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

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ImpactStudiesVmin.UnitTest")]

namespace ImpactStudiesVmin
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using DDG;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.PatConfigService;
    using Prime.PhAttributes;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class ImpactStudiesVmin : TestMethodBase
    {
        /// <summary>
        /// Gets or sets the name of the Configuration File.
        /// </summary>
        public TestMethodsParams.File ConfigurationFile { get; set; }

        /// <summary>
        /// Gets or sets the value to subtract from the Vmin Result before forwarding to the next test.
        /// </summary>
        public TestMethodsParams.Double VminForwardOffset { get; set; } = 0.03;

        /// <summary>
        /// Gets or sets a IFileSystem for Mocking.
        /// </summary>
        internal IFileSystem FileWrapper { get; set; } = new FileSystem();

        /// <summary>
        /// Gets or sets the IVminFactory for Mocking.
        /// </summary>
        internal IVminFactory VminSearchFactory { get; set; } = new VminFactory();

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        private List<VminSearch> SearchTests { get; set; }

        private List<string> SharedStorageForVminResult { get; set; }

        private List<double> StartingVoltages { get; set; }

        private Configuration ConfigurationData { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;
            this.Console?.PrintDebug("Starting Verify...");
            if (this.ConfigurationFile == null || !this.ReadConfiguration(this.ConfigurationFile))
            {
                throw new Prime.Base.Exceptions.TestMethodException($"Failed verify on ConfigurationFile=[{this.ConfigurationFile}].");
            }

            this.SearchTests = new List<VminSearch>(this.ConfigurationData.Tests.Count);
            foreach (var testStruct in this.ConfigurationData.Tests)
            {
                this.Console?.PrintDebug($"\n*************** Creating new test {testStruct.Name} ***************");
                var test = this.CreateNewVminTest(testStruct);
                this.SearchTests.Add(test);

                this.Console?.PrintDebug($"\n*************** Running Verify on {test.InstanceName} ***************");
                test.VerifyWrapper();

                this.Console?.PrintDebug($"\n*************** Running CustomVerify on {test.InstanceName} ***************");
                test.CustomVerify();
                this.Console?.PrintDebug($"test {testStruct.Name} has been created and verified.");
            }
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            // reset the vmin forwarding
            // var lastValidVmin = new List<double>(this.SharedStorageForVminResult.Count);
            var lastValidStartVoltage = new List<double>(this.SharedStorageForVminResult.Count);
            for (var i = 0; i < this.StartingVoltages.Count(); i++)
            {
                this.Console?.PrintDebug($"Setting SharedStorage=[{this.SharedStorageForVminResult[i]}] with StartVoltage=[{this.StartingVoltages[i]}]");
                Prime.Services.SharedStorageService.InsertRowAtTable(this.SharedStorageForVminResult[i], this.StartingVoltages[i], Prime.SharedStorageService.Context.DUT);
                lastValidStartVoltage.Add(this.StartingVoltages[i]);
            }

            // run the tests.
            for (var testIndex = 0; testIndex < this.SearchTests.Count; testIndex++)
            {
                var test = this.SearchTests[testIndex];
                this.TriggerCallback(test.PreInstanceCallback, $"\n*************** Executing PreInstance for {test.InstanceName} ***************");
                this.ExecuteSetPointPatConfig(test.PreConfigSetPointsWithData, test.PreConfigSetPointsWithDefault, $"\n*************** Executing SetPointsPreInstance for {test.InstanceName} ***************");

                this.Console?.PrintDebug($"\n*************** Running Execute on {test.InstanceName} ***************");
                var result = test.Execute();
                this.Console?.PrintDebug($"*************** Returned {result} ***************");

                this.ExecuteSetPointPatConfig(test.PostConfigSetPointsWithData, test.PostConfigSetPointsWithDefault, $"\n*************** Executing SetPointsPostInstance for {test.InstanceName} ***************");
                this.TriggerCallback(test.PostInstanceCallback, $"\n*************** Executing PostInstance for {test.InstanceName} ***************");

                // reset the starting voltage to be (VminResult - (ScoreboardEdgeTicks * StepSize))
                // use the next tests parameters to determine the offset (if its the last test we could skip this, or just use the current tests parameters).
                // var nextScoreboardEdgeTicks = testIndex < (this.SearchTests.Count - 1) ? this.SearchTests[testIndex + 1].ScoreboardEdgeTicks : this.SearchTests[testIndex].ScoreboardEdgeTicks;
                // var nextStepSize = testIndex < (this.SearchTests.Count - 1) ? this.SearchTests[testIndex + 1].StepSize : this.SearchTests[testIndex].StepSize;
                // var forwardOffset = nextScoreboardEdgeTicks * nextStepSize;
                var forwardOffset = Math.Abs(this.VminForwardOffset);

                for (var i = 0; i < this.StartingVoltages.Count(); i++)
                {
                    var latestVmin = Prime.Services.SharedStorageService.GetDoubleRowFromTable(this.SharedStorageForVminResult[i], Prime.SharedStorageService.Context.DUT);
                    if ((latestVmin - forwardOffset) > lastValidStartVoltage[i])
                    {
                        lastValidStartVoltage[i] = latestVmin - forwardOffset;
                    }

                    Prime.Services.SharedStorageService.InsertRowAtTable(this.SharedStorageForVminResult[i], lastValidStartVoltage[i], Prime.SharedStorageService.Context.DUT);
                }
            }

            return 1;
        }

        private void ExecuteSetPointPatConfig(List<Tuple<IPatConfigSetPointHandle, string>> handlesWithData, List<IPatConfigSetPointHandle> handlesWithDefault, string commentString)
        {
            this.Console?.PrintDebug(commentString);
            handlesWithData.ForEach(t => t.Item1.ApplySetPoint(t.Item2));
            handlesWithDefault.ForEach(h => h.ApplySetPointDefault());
        }

        private void TriggerCallback(Tuple<string, string> callbackTuple, string commentString)
        {
            if (callbackTuple != null)
            {
                this.Console?.PrintDebug(commentString);
                Prime.Services.TestProgramService.TriggerCallback(callbackTuple.Item1, callbackTuple.Item2);
            }
        }

        private VminSearch CreateNewVminTest(Configuration.VminTest testobj)
        {
            this.Console?.PrintDebug($"Creating new VminSearch instance for Test=[{testobj.Name}].");
            var test = this.VminSearchFactory.CreateInstance();
            test.InstanceName = testobj.Name;
            test.LogLevel = this.LogLevel;
            test.BypassPort = -1;

            // Setup defaults just in case the user doesn't set these.
            this.Console?.PrintDebug($"<setting default parameters from code.>");
            this.UpdateParameter(ref test, "PatternNameMap", "1,2,3,4,5,6,7");
            this.UpdateParameter(ref test, "ScoreboardBaseNumber", "0000");
            this.UpdateParameter(ref test, "ScoreboardEdgeTicks", "2");

            this.Console?.PrintDebug($"<setting default parameters from user config file.>");
            foreach (var paramPair in this.ConfigurationData.VminParameters)
            {
                this.UpdateParameter(ref test, paramPair.Key, paramPair.Value);
            }

            this.Console?.PrintDebug($"<setting override parameters from user config file.>");
            foreach (var paramPair in testobj.Overrides)
            {
                this.UpdateParameter(ref test, paramPair.Key, paramPair.Value);
            }

            // Overwrite any user selections with this stuff.
            this.Console?.PrintDebug($"<setting override parameters from code.>");
            this.UpdateParameter(ref test, "Patlist", testobj.Patlist);
            this.UpdateParameter(ref test, "StartVoltages", string.Join(",", this.SharedStorageForVminResult));
            this.UpdateParameter(ref test, "VminResult", string.Join(",", this.SharedStorageForVminResult));
            this.UpdateParameter(ref test, "ForwardingMode", "None");
            this.UpdateParameter(ref test, "RecoveryMode", "Default");
            this.UpdateParameter(ref test, "TestMode", this.SharedStorageForVminResult.Count == 1 ? "SingleVmin" : "MultiVmin");

            test.TestMethodExtension = test;
            return test;
        }

        private void UpdateParameter(ref VminSearch test, string param, string value)
        {
            var property = typeof(VminSearch).GetProperty(param);
            if (property.PropertyType.IsEnum)
            {
                this.Console?.PrintDebug($"[{test.InstanceName}] Setting Enum Parameter [{param}] to [{value}].");
                property.SetValue(test, System.Enum.Parse(property.PropertyType, value));
            }
            else
            {
                this.Console?.PrintDebug($"[{test.InstanceName}] Setting Object Parameter [{param}] to [{value}].");
                property.SetValue(test, System.Activator.CreateInstance(property.PropertyType, new object[] { value }));
            }
        }

        private bool ReadConfiguration(string configFile)
        {
            var localFileName = FileUtilities.GetFile(configFile);
            this.ConfigurationData = JsonConvert.DeserializeObject<Configuration>(this.FileWrapper.File.ReadAllText(localFileName));

            var invalidParameterFound = false;
            if (this.ConfigurationData.VminParameters == null)
            {
                Prime.Services.ConsoleService.PrintError($"Configuration file is missing the [VminParameters] section.");
                return false;
            }

            /* // check for common parameters.
            var commonParams = new List<string> { "PreInstance", "PostInstance" }; */

            // check for parameters that we're going to use differently.
            if (this.ConfigurationData.VminParameters.ContainsKey("StartVoltages"))
            {
                this.StartingVoltages = this.ConfigurationData.VminParameters["StartVoltages"].Split(',').Select(s => s.Trim().EvaluateExpression()).ToList();
                this.ConfigurationData.VminParameters.Remove("StartVoltages");
                this.SharedStorageForVminResult = new List<string>(this.StartingVoltages.Count());
                for (var i = 0; i < this.StartingVoltages.Count(); i++)
                {
                    var name = $"{this.InstanceName.Replace(":", "_")}_Vmin{i}";
                    this.SharedStorageForVminResult.Add(name);
                }
            }
            else
            {
                Prime.Services.ConsoleService.PrintError($"Configuration file is missing Parameter=[StartVoltages].");
                invalidParameterFound = true;
            }

            /* // check for parameters that we're going to override.
            var overrideParams = new List<string> { "VminResult" }; */

            // verify that the parameters are correct.
            foreach (var param in this.ConfigurationData.VminParameters.Keys)
            {
                invalidParameterFound |= !this.ParamIsValid(param);
            }

            // verify that there is at least one plist to run.
            if (this.ConfigurationData.Tests == null || this.ConfigurationData.Tests.Count < 1)
            {
                Prime.Services.ConsoleService.PrintError($"Configuration file must contain at least one plist under the [Tests] key.");
                invalidParameterFound = true;
            }

            // verify the override parameters are correct.
            foreach (var test in this.ConfigurationData.Tests)
            {
                foreach (var param in test.Overrides.Keys)
                {
                    invalidParameterFound |= !this.ParamIsValid(param);
                }
            }

            return !invalidParameterFound;
        }

        private bool ParamIsValid(string param)
        {
            var property = typeof(VminSearch).GetProperty(param);
            if (property == null)
            {
                Prime.Services.ConsoleService.PrintError($"VminTC does not contain parameter=[{param}].");
                return false;
            }

            return true;
        }
    }
}