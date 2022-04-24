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

namespace SIOShmooTC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using SIO;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class SIOShmooTC : TestMethodBase
    {
        /// <summary>
        /// Gets or sets the User File name.
        /// </summary>
        public TestMethodsParams.String UserFile { get; set; }

        /// <summary>
        /// Gets or sets the User Token.
        /// </summary>
        public TestMethodsParams.String UserToken { get; set; }

        /// <summary>
        /// Gets or sets comma separated list of tokens to run before the main test.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString PreTestTokens { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets comma separated list of tokens to run after the main test.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString PostTestTokens { get; set; } = string.Empty;

        private SIOLib Lib { get; set; }

        private UserFile.UserData UserData { get; set; }

        private UserFile UserFileData { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            Prime.Services.ConsoleService.PrintDebug($"Running Verify on TestInstance=[{this.InstanceName}]");
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

            this.Lib = new SIOLib(this.LogLevel != PrimeLogLevel.DISABLED);
            this.UserFileData = new SIO.UserFile(this.UserFile);
            if (!this.UserFileData.Valid)
            {
                throw new FileLoadException($"[{this.InstanceName}] Failed to read UserDataFile=[{this.UserFile}] correctly.");
            }

            if (!this.UserFileData.TokenBlocks.ContainsKey(this.UserToken))
            {
                throw new FileLoadException($"[{this.InstanceName}] Token=[{this.UserToken}] Not found in UserDataFile=[{this.UserFile}].");
            }

            if (!this.Lib.ShmooTestSetup(this.UserFileData, this.UserToken))
            {
                throw new FileLoadException($"[{this.InstanceName}] Token=[{this.UserToken}] Failed to setup UserData for shmoo.");
            }

            this.UserData = this.UserFileData.TokenBlocks[this.UserToken];
            this.UserData.ShmooSingleTestPointFunc = this.Lib.RunShmooSinglePoint;

            Prime.Services.ConsoleService.PrintDebug($"[{this.InstanceName}] reading test parameters from executable=[{this.UserData.ExecuteTest}]");

            try
            {
                // if it has an xaxis_parameter then its a shmoo. FIXME -this matches python but it better to ask for a prime GetTemplateName service.
                var xparam = Prime.Services.TestProgramService.GetTestInstanceParameter(this.UserData.ExecuteTest, "xaxis_parameter");
                this.UserData.TestInstanceIsShmoo = true;
            }
            catch
            {
                this.UserData.TestInstanceIsShmoo = false;
            }

            Prime.Services.ConsoleService.PrintDebug($"[{this.InstanceName}] Determined IsShmoo=[{this.UserData.TestInstanceIsShmoo}] PatList=[{this.UserData.Plist}]");

            // final verification of parameters
            if (this.UserData.TestInstanceIsShmoo && this.UserData.TestType != "GONOGO")
            {
                throw new ArgumentException($"[{this.InstanceName}] Token=[{this.UserToken}] Invalid Test Type=[{this.UserData.TestType}] for iCShmoo test, must be GONOGO.");
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

            // do special logging if this is an iCShmoo test.
            if (this.UserData.TestInstanceIsShmoo)
            {
                var writer = Prime.Services.DatalogService.GetItuffComntWriter();
                writer.IncludeTnameInPrint(false);
                writer.SetData($"tname_{this.UserData.DlogName}_SHMOO_LVLTIM_START");
                Prime.Services.DatalogService.WriteToItuff(writer);
                /* Prime.Services.DatalogService.WriteToItuff($"{ituffToken}_comnt_tname_{this.UserData.DlogName}_SHMOO_LVLTIM_START\n"); */
            }

            // Run the Shmoo.
            Prime.Services.ConsoleService.PrintDebug($"[{this.InstanceName}] Running Shmoo.");
            var numErrors = this.Lib.RunShmoo(this.UserData);
            Prime.Services.ConsoleService.PrintDebug($"[{this.InstanceName}] Shmoo returned {numErrors} setup errors.");

            // do special logging if this is an iCShmoo test.
            if (this.UserData.TestInstanceIsShmoo)
            {
                var writer = Prime.Services.DatalogService.GetItuffComntWriter();
                writer.IncludeTnameInPrint(false);
                writer.SetData($"tname_{this.UserData.DlogName}_SHMOO_LVLTIM_END");
                Prime.Services.DatalogService.WriteToItuff(writer);
                /* Prime.Services.DatalogService.WriteToItuff($"{ituffToken}_comnt_tname_{this.UserData.DlogName}_SHMOO_LVLTIM_END\n"); */
            }

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
                Prime.Services.ConsoleService.PrintError($"[{this.InstanceName}] Detected {numErrors} errors while running shmoo.");
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
                success &= this.Lib.RunSetupTest(this.UserFileData, token);
            }

            return success;
        }
    }
}
