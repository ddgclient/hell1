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

namespace DDGFunctionalTC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PhAttributes;
    using Prime.PlistService;
    using Prime.SharedStorageService;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;

    /// <summary>
    /// Test method responsible for executing different variations of functional test.
    /// </summary>
    [PrimeTestMethod]
    public class DDGFunctionalTC : PrimeFunctionalTestMethod, IFunctionalExtensions
    {
        private IPlistObject plistObject;
        private List<string> pins;
        private List<string> tokens;

        /// <summary>
        /// Enum for On/Off modes.
        /// </summary>
        public enum OnOffMode
        {
            /// <summary>
            /// ENABLED.
            /// </summary>
            ENABLED = 0,

            /// <summary>
            /// DISABLED.
            /// </summary>
            DISABLED = 1,
        }

        /// <summary>
        /// Gets or sets a list of comma-separated SharedStorage tokens to store captured data per pin.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString CapturedDataTokens { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a option to print previous label.
        /// </summary>
        public OnOffMode PrintPreviousLabel { get; set; } = OnOffMode.ENABLED;

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        /// <inheritdoc />
        public override void CustomVerify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;
            this.plistObject = Prime.Services.PlistService.GetPlistObject(this.Patlist);
            if (string.IsNullOrEmpty(this.CtvCapturePins) || string.IsNullOrEmpty(this.CapturedDataTokens))
            {
                return;
            }

            this.pins = this.CtvCapturePins.ToList();
            this.tokens = this.CapturedDataTokens.ToList();
            if (this.pins.Count != this.tokens.Count)
            {
                throw new ArgumentException($"Number of {nameof(this.CapturedDataTokens)} must match number of {nameof(this.CtvCapturePins)}.");
            }
        }

        /// <inheritdoc />
        bool IFunctionalExtensions.ProcessCtvPerPin(Dictionary<string, string> ctvData)
        {
            this.Console?.PrintDebug(ctvData.CtvDataToString());
            if (!string.IsNullOrEmpty(this.CtvCapturePins) && !string.IsNullOrEmpty(this.CapturedDataTokens))
            {
                for (int i = 0; i < this.pins.Count; i++)
                {
                    Prime.Services.SharedStorageService.InsertRowAtTable(this.tokens[i], ctvData[this.pins[i]], Context.DUT);
                }
            }

            return true;
        }

        /// <inheritdoc />
        bool IFunctionalExtensions.ProcessFailures(ICaptureFailureTest captureFailureTest)
        {
            if (this.MaxFailuresToItuff != 0)
            {
                captureFailureTest.DatalogFailure((uint)this.MaxFailuresToItuff, (uint)this.MaxFailuresPerPatternToItuff);
            }

            var failureCycle = captureFailureTest.GetPerCycleFailures();
            if (failureCycle.Count <= 0)
            {
                return true;
            }

            this.ExitPort = 0;
            var failingPattern = failureCycle.First().GetPatternName();
            if (this.plistObject.IsPatternAnAmble(failingPattern))
            {
                this.ExitPort = 2;
            }

            this.Console?.PrintDebug(failureCycle.FailDataToString());
            if (this.PrintPreviousLabel != OnOffMode.ENABLED)
            {
                return true;
            }

            var label = "NA";
            try
            {
                label = failureCycle.First().GetPreviousLabel();
            }
            catch
            {
                // ignore.
            }

            var writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            writer.SetTnamePostfix("_lb");
            writer.SetData(label);
            Prime.Services.DatalogService.WriteToItuff(writer);

            return true;
        }
    }
}