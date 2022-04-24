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

namespace DDGCapturePacketsTC
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PhAttributes;
    using Prime.PlistService;
    using Prime.TestMethods;

    /// <summary>
    /// This class is intended to overwrite the test method PrimeFuncCaptureCtvTestMethod.
    /// </summary>
    [PrimeTestMethod]
    public class DDGCapturePacketsTC : TestMethodBase
    {
        /// <summary>
        /// Enum holding the test method execution mode.
        /// </summary>
        public enum Mode
        {
            /// <summary>PER_PIN execution mode.</summary>
            PER_PIN,
        }

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
        /// Gets or sets  comma separated pins for mask.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString MaskPins { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets comma separated pins for CTV capture.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString DataPins { get; set; }

        /// <summary>
        /// Gets or sets the GSDS tokens where the decoded packets will be saved.
        /// </summary>
        public Mode ExecutionMode { get; set; } = Mode.PER_PIN;

        /// <summary>
        /// Gets or sets the GSDS tokens where the decoded packets will be saved.
        /// </summary>
        public TestMethodsParams.String OutputGsds { get; set; }

        /// <summary>
        /// Gets or sets the length of all the valid data captured.
        /// If this amount mismatches the length of the valid captured data,
        /// the instance fails. This parameter is optional; if empty, the size is not checked.
        /// </summary>
        public TestMethodsParams.Integer TotalSize { get; set; } = 0;

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        private IPlistObject PlistObject { get; set; }

        private ICaptureFailureAndCtvPerPinTest FunctionalTest { get; set; }

        private List<string> PinsToMask { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;
            if (!Regex.IsMatch(this.OutputGsds, @"^G.[LUI].S.\S+$"))
            {
                throw new Prime.Base.Exceptions.TestMethodException($"Parameter=[OutputGsds] must be a gsds of string type. Expected format=[G.[LUI].S.name]");
            }

            this.PinsToMask = string.IsNullOrWhiteSpace(this.MaskPins) ? new List<string>() : this.MaskPins.ToList();
            foreach (string pinName in this.PinsToMask)
            {
                if (!Prime.Services.PinService.Exists(pinName))
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"Mask pin=[{pinName}] does not exist.");
                }
            }

            this.PlistObject = Prime.Services.PlistService.GetPlistObject(this.Patlist);
            this.FunctionalTest = Prime.Services.FunctionalService.CreateCaptureFailureAndCtvPerPinTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.DataPins, 1, 1, this.PrePlist);
        }

        /// <inheritdoc />
        [Returns(2, PortType.Fail, "FAIL PORT")]
        [Returns(1, PortType.Pass, "PASS PORT")]
        [Returns(0, PortType.Fail, "FAIL PORT")]
        public override int Execute()
        {
            this.FunctionalTest.ApplyTestConditions();
            this.FunctionalTest.SetPinMask(this.PinsToMask);

            var executionResult = this.FunctionalTest.Execute();
            var ambleFailed = false;
            if (!executionResult)
            {
                this.FunctionalTest.DatalogFailure(1);
                ambleFailed = this.DidAmbleFail(this.FunctionalTest);
            }

            var ctvData = this.ProcessCtvPerPin(this.FunctionalTest.GetCtvData());
            var ctvIsAllZero = !ctvData.Contains('1');
            var ctvIsInvalidLength = this.TotalSize > 0 && this.TotalSize != ctvData.Length;

            this.Console?.PrintDebug($"Functional Test Passed=[{executionResult}].");
            this.Console?.PrintDebug($"Amble Failure=[{ambleFailed}].");
            this.Console?.PrintDebug($"Data Length=[{ctvData.Length}].");
            this.Console?.PrintDebug($"Data Is All Zero=[{ctvIsAllZero}].");
            this.Console?.PrintDebug($"Failed Size Check=[{ctvIsInvalidLength}].");
            this.Console?.PrintDebug($"Captured Data=[{ctvData}].");

            if (ctvIsInvalidLength || ctvData.Length == 0)
            {
                this.Console?.PrintDebug($"Data failed size check. Clearing [{this.OutputGsds}] and Exiting Port 0");
                DDG.Gsds.WriteToken(this.OutputGsds, string.Empty);
                return 0;
            }
            else if (ctvIsAllZero || ambleFailed)
            {
                this.Console?.PrintDebug($"Data is all Zero or Amble failed. Writing Captured Data to [{this.OutputGsds}] and Exiting Port 2");
                DDG.Gsds.WriteToken(this.OutputGsds, ctvData);
                return 2;
            }
            else
            {
                this.Console?.PrintDebug($"Writing Captured Data to [{this.OutputGsds}] and Exiting Port 1");
                DDG.Gsds.WriteToken(this.OutputGsds, ctvData);
                return 1;
            }
        }

        private bool DidAmbleFail(ICaptureFailureTest captureFailureTest)
        {
            foreach (var failure in captureFailureTest.GetPerCycleFailures())
            {
                if (this.PlistObject.IsPatternAnAmble(failure.GetPatternName()))
                {
                    return true;
                }
            }

            return false;
        }

        private string ProcessCtvPerPin(Dictionary<string, string> ctvData)
        {
            switch (this.ExecutionMode)
            {
                default: /* Mode.PER_PIN: */
                    var perPinData = new System.Text.StringBuilder();
                    foreach (var ctvPin in this.DataPins.ToList())
                    {
                        perPinData.Append(ctvData[ctvPin]);
                    }

                    return perPinData.ToString();
            }
        }
    }
}