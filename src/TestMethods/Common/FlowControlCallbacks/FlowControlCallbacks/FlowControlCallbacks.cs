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

namespace FlowControlCallbacks
{
    using DDG;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    public class FlowControlCallbacks
    {
        /// <summary>
        /// Gets the default return value to continue testing.
        /// </summary>
        internal static string CONTINUE { get; } = "CONTINUE";

        /// <summary>
        /// Gets the default return value for a failing function.
        /// </summary>
        internal static string FAIL { get; } = "FAIL";

        /// <summary>
        /// Check flow callback to CONTINUE or FAIL on down bin.
        /// </summary>
        /// <param name="domain">The Domain name to check the flow of.</param>
        /// <returns>Return CONTINUE or FAIL.</returns>
        public static string CheckFlow(string domain)
        {
            // "User Callback MUST return only \"CONTINUE\" or \"FAIL\" any other returned value is Invalid and will exit PORT -1."
            if (!DDG.VminForwarding.Service.IsSinglePointMode())
            {
                Prime.Services.ConsoleService.PrintDebug($"VminSinglePointMode=[False] so CheckFlow is disabled. Return=[CONTINUE]\n");
                return CONTINUE;
            }

            var indexValueFromTestInstance = Prime.Services.TestProgramService.GetCurrentFlowNumber();
            var indexValueForDomain = GetCurrentFlowIndexForDomain(domain);

            if (indexValueFromTestInstance == indexValueForDomain)
            {
                Prime.Services.ConsoleService.PrintDebug($"FlowIndex from Instance=[{indexValueFromTestInstance}] and Value From TestProgram Service =[{indexValueForDomain}] for Domain=[{domain}] return=[CONTINUE]\n");
                return CONTINUE;
            }

            Prime.Services.ConsoleService.PrintDebug($"FlowIndex from Instance=[{indexValueFromTestInstance}] and Value From TestProgram Service =[{indexValueForDomain}] for Domain=[{domain}] return=[FAIL]\n");
            return FAIL;
        }

        /// <summary>
        /// Set flow callback to CONTINUE or FAIL on down bin.
        /// </summary>
        /// <param name="args">arguments to print.</param>
        /// <returns>Return CONTINUE or FAIL.</returns>
        public static string SetFlow(string args)
        {
            // "User Callback MUST return only \"CONTINUE\" or \"FAIL\" any other returned value is Invalid and will exit PORT -1."
            var indexValue = Prime.Services.TestProgramService.GetCurrentFlowNumber();
            Prime.Services.TestProgramService.SetDomainCurrentFlow(args, indexValue);

            return CONTINUE;
        }

        // TODO: Move GetCurrentFlowIndexForDomain to DDG TestProgramService extensions.
        private static int GetCurrentFlowIndexForDomain(string domain)
        {
            if (Prime.Services.TestProgramService.IsDffEnableForDomainFlow(domain))
            {
                string dffTokenName = Prime.Services.TestProgramService.GetDffTokenNameForDomainFlow(domain);
                Prime.Services.ConsoleService.PrintDebug($"UseDffToken is set for Domain=[{domain}]. Getting current flow index from DFF Token=[{dffTokenName}]\n");

                string sDffTokenValue = Prime.Services.DffService.GetDff(dffTokenName);
                if (!int.TryParse(sDffTokenValue, out var indexValueForDomain))
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"Unable to convert DFF token [{dffTokenName}] value=[{sDffTokenValue}] to Integer.");
                }

                return indexValueForDomain;
            }
            else
            {
                return Prime.Services.TestProgramService.GetDomainCurrentFlow(domain);
            }
        }
    }
}
