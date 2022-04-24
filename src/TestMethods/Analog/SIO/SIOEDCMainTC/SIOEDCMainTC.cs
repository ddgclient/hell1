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

namespace SIOEDCMainTC
{
    using System;
    using System.Collections.Generic;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using SIO;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class SIOEDCMainTC : TestMethodBase
    {
        /// <summary>Enum to hold bool value for Parameters.</summary>
        public enum MyBool
        {
            /// <summary>true.</summary>
            True,

            /// <summary>false.</summary>
            False,
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
        /// Gets or sets a value indicating wheter EDC Shmoo Mode is enabled.
        /// </summary>
        public MyBool EDCShmooEnabled { get; set; } = MyBool.False;

        /// <summary>
        /// Gets or sets a value indicating wheter EDC Logging is enabled.
        /// </summary>
        public MyBool EDCLogEnabled { get; set; } = MyBool.True;

        /// <summary>
        /// Gets or sets comma separated list of tokens to run before the main test.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString PreTestTokens { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets comma separated list of tokens to run after the main test.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString PostTestTokens { get; set; } = string.Empty;

        private SIOEDC EdcLib { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            Prime.Services.ConsoleService.PrintDebug($"UserFile=[{this.UserFile}]");
            Prime.Services.ConsoleService.PrintDebug($"UserToken=[{this.UserToken}]");

            if (this.UserFile == null || this.UserFile == string.Empty)
            {
                throw new ArgumentException($"[{this.InstanceName}] UserFile is a required argument.");
            }

            if (this.UserToken == null || this.UserToken == string.Empty)
            {
                throw new ArgumentException($"[{this.InstanceName}] UserToken is a required argument.");
            }

            this.EdcLib = new SIOEDC(this.InstanceName, this.LogLevel != PrimeLogLevel.DISABLED);

            if (!this.EdcLib.SetupEDCMain(this.UserFile, this.UserToken, this.EDCShmooEnabled == MyBool.True, this.EDCLogEnabled == MyBool.True))
            {
                throw new ArgumentException($"[{this.InstanceName}] failed SetupEDCMain.");
            }

            Prime.Services.ConsoleService.PrintDebug($"Verify completed successfully on TestInstance=[{this.InstanceName}]");
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            // Run any setup.
            if (this.PreTestTokens != string.Empty)
            {
                if (!this.RunSetup(this.PreTestTokens.ToList()))
                {
                    Prime.Services.ConsoleService.PrintError($"[{this.InstanceName}] Failed to run PreTest Tokens.");
                    return 0;
                }
            }

            // Run the Shmoo/Test.
            Prime.Services.ConsoleService.PrintDebug($"[{this.InstanceName}] Running Shmoo.");
            var numErrors = this.EdcLib.RunEDCMain();
            Prime.Services.ConsoleService.PrintDebug($"[{this.InstanceName}] Shmoo returned {numErrors} setup errors.");

            // Run any cleanup.
            if (this.PostTestTokens != string.Empty)
            {
                if (!this.RunSetup(this.PostTestTokens.ToList()))
                {
                    Prime.Services.ConsoleService.PrintError($"[{this.InstanceName}] Failed to run PostTest Tokens.");
                    return 0;
                }
            }

            // Exit based on setup errors during the shmoo
            if (numErrors > 0)
            {
                Prime.Services.ConsoleService.PrintError($"[{this.InstanceName}] Detected {numErrors} error(s) while running shmoo.");
                return 0;
            }
            else
            {
                return 1;
            }
        }

        private bool RunSetup(List<string> tokens)
        {
            var success = true;
            foreach (var token in tokens)
            {
                success &= this.EdcLib.SioLib.RunSetupTest(this.EdcLib.UserFileData, token);
            }

            return success;
        }
    }
}
