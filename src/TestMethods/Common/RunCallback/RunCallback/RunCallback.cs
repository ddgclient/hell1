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

namespace RunCallback
{
    using System;
    using System.Collections.Generic;
    using Prime.ConsoleService;
    using Prime.PhAttributes;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class RunCallback : TestMethodBase
    {
        private DDG.HdmtExpression portExpressionObj;

        /// <summary>
        /// Gets or sets the name of the callback to execute.
        /// </summary>
        public TestMethodsParams.String Callback { get; set; }

        /// <summary>
        /// Gets or sets the parameters to send to the callback.
        /// </summary>
        public TestMethodsParams.String Parameters { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the GSDS token (of the form G.U.S.TokenName) to write the callbacks return value to.
        /// </summary>
        public TestMethodsParams.String ResultToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an expression to set exit port based on the callback return value.
        /// </summary>
        public TestMethodsParams.String ResultPort { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;
            if (this.Callback == null || string.IsNullOrWhiteSpace(this.Callback))
            {
                throw new ArgumentException("Callback parameter should not be empty.");
            }

            if (!Prime.Services.TestProgramService.DoesCallbackExist(this.Callback))
            {
                throw new ArgumentException($"Callback=[{this.Callback}] does not exist.");
            }

            if (!string.IsNullOrWhiteSpace(this.ResultToken) && !DDG.Gsds.IsTokenFormat(this.ResultToken))
            {
                throw new ArgumentException($"ResultToken=[{this.ResultToken}] must be of the form G.[ULI].[SDI].Token");
            }

            this.portExpressionObj = string.IsNullOrEmpty(this.ResultPort) ? null : new DDG.HdmtExpression(this.ResultPort);
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            this.Console?.PrintDebug($"Executing {this.Callback}({this.Parameters})");
            var retval = Prime.Services.TestProgramService.TriggerCallback(this.Callback, this.Parameters);
            this.Console?.PrintDebug($"Callback returned [{retval}]");

            if (!string.IsNullOrWhiteSpace(this.ResultToken))
            {
                DDG.Gsds.WriteToken(this.ResultToken, retval);
            }

            if (string.IsNullOrEmpty(this.ResultPort))
            {
                return 1;
            }

            var resultToken = new Dictionary<string, object>() { { "R", retval } };
            this.portExpressionObj.Parameters = resultToken;
            return (int)this.portExpressionObj.Evaluate();
        }
    }
}