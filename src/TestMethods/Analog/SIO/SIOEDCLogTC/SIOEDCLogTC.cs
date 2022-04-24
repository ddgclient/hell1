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

namespace SIOEDCLogTC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;
    using SIO;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class SIOEDCLogTC : PrimeFunctionalTestMethod, IFunctionalExtensions
    {
        private enum GlobalType
        {
            NONE,
            GSDS,
            USERVAR,
         }

        /// <summary>
        /// Gets or sets the User File name.
        /// </summary>
        public TestMethodsParams.String UserFile { get; set; }

        /// <summary>
        /// Gets or sets the User Token.
        /// </summary>
        public TestMethodsParams.String UserToken { get; set; }

        /// <summary>
        /// Gets or sets the ReuseCaptMemGlobal. If set, the plist will not be executed, instead
        /// the capture memory will be read from this global. The global can be GSDS, UserVar or SharedStorage.
        /// Formats for Globals:
        ///   UserVar:         collection.uservar
        ///   GSDS:            G.[UL].S.token
        ///   SharedStorage:   context:variable
        ///   .
        /// </summary>
        public TestMethodsParams.String ReuseCaptMemGlobal { get; set; } = string.Empty;

        private SIOEDC SioEdcLib { get; set; }

        private bool RunPlist { get; set; }

        private GlobalType CaptMemGlobalType { get; set; } = GlobalType.NONE;

        /// <inheritdoc/>
        bool IFunctionalExtensions.ProcessCtvPerPin(Dictionary<string, string> ctvData)
        {
            Prime.Services.ConsoleService.PrintDebug($"[SIOEDCLogTC] {this.InstanceName} running ProcessCtvPerPin.");
            var exitPort = this.SioEdcLib.RunEDCLog(ctvData, this.Patlist, this.CtvCapturePins);
            Prime.Services.ConsoleService.PrintDebug($"[SIOEDCLogTC] {this.InstanceName} RunEDCLog returned [{exitPort}].");

            return exitPort == 1;
        }

        /// <summary>
        /// Override the PrimeFuncCaptureCtvTestMethod.Execute method to not execute plist if ReuseCaptMemGlobal is set.
        /// </summary>
        /// <returns>exit port.</returns>
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        [Returns(2, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            if (this.RunPlist)
            {
                // Normal execution, just use the base execute.
                Prime.Services.ConsoleService.PrintDebug($"[SIOEDCLogTC] {this.InstanceName} running Execute().");
                return base.Execute();
            }
            else
            {
                Prime.Services.ConsoleService.PrintDebug($"[SIOEDCLogTC] {this.InstanceName} Skipping Execute() since ReuseCaptMemGlobal is set.");
                Dictionary<string, string> ctvData = new Dictionary<string, string>();
                if (this.CaptMemGlobalType == GlobalType.USERVAR)
                {
                    ctvData[this.CtvCapturePins] = Prime.Services.UserVarService.GetStringValue(this.ReuseCaptMemGlobal);
                }
                else if (this.CaptMemGlobalType == GlobalType.GSDS)
                {
                    ctvData[this.CtvCapturePins] = Convert.ToString(DDG.Gsds.ReadToken(this.ReuseCaptMemGlobal));
                }

                return this.TestMethodExtension.ProcessCtvPerPin(ctvData) ? 1 : 2;
            }
        }

        /// <inheritdoc />
        public override void CustomVerify()
        {
            Prime.Services.ConsoleService.PrintDebug($"[SIOEDCLogTC] {this.InstanceName} running CustomVerify.");

            if (this.ReuseCaptMemGlobal == string.Empty)
            {
                this.RunPlist = true;
                this.CaptMemGlobalType = GlobalType.NONE;
                Prime.Services.ConsoleService.PrintDebug($"[SIOEDCLogTC] {this.InstanceName} - Parameter ReuseCaptMemGlobal is empty, running instance normally.");
            }
            else
            {
                this.RunPlist = false;
                Prime.Services.ConsoleService.PrintDebug($"[SIOEDCLogTC] {this.InstanceName} - Parameter ReuseCaptMemGlobal=[{this.ReuseCaptMemGlobal}] ... will skip pattern execution.");

                var numPeriods = this.ReuseCaptMemGlobal.ToString().Count(ch => ch == '.');
                var numColons = this.ReuseCaptMemGlobal.ToString().Count(ch => ch == ':');

                if (numPeriods == 1)
                {
                    // format = <collection>.<variable> -> its a Uservar.
                    this.CaptMemGlobalType = GlobalType.USERVAR;
                }
                else if (numPeriods == 3)
                {
                    // format = G.[UL].S.variable -> its a GSDS
                    this.CaptMemGlobalType = GlobalType.GSDS;
                }
                else
                {
                    // format = unknown
                    throw new ArgumentException($"Error: ReuseCaptMemGlobal=[{this.ReuseCaptMemGlobal}] is in an unknown format, should be UserVar(Collection.uservar) or GSDS (G.U.S.Token).");
                }
            }

            this.SioEdcLib = new SIOEDC(this.InstanceName, this.LogLevel != PrimeLogLevel.DISABLED);
            this.SioEdcLib.SetupEDCLog(this.UserFile, this.UserToken);

            Prime.Services.ConsoleService.PrintDebug($"[SIOEDCLogTC] {this.InstanceName} done with CustomVerify.");
        }
    }
}