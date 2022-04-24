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

namespace SocRecoveryCallbacks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DDG;
    using Prime.Base.Exceptions;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    public class SocRecoveryCallbacks
    {
        /// <summary>
        /// Callback function to Configure an IP for Recovery. Token for recovery in list will be set to 0.
        /// </summary>
        /// <param name="args">Argument String as delimited string of token names to initialise to 0 using either " " or "|".</param>
        public static void InitialiseSOCRecovery(string args)
        {
            /* Simple debug info to show user inputs */
            /* Parse arguments is a simple function to parse string entered by user calling the callback */
            var input_string = ParseArguments(args);
            var input_values = new List<int>();
            /* Create a 0 entry for each user input token to set into Prime shared storage database */
            for (var i = 0; i < input_string.Count; i++)
            {
                input_values.Add(0);
            }

            /* Make inputs into format taken in by POC recovery interface */
            var name_list = string.Join("|", input_string);
            var value_list = string.Join("|", input_values);
            /* Make sure none of the input strings are null */
            if (!string.IsNullOrEmpty(name_list))
            {
                // Call the set tracking list function.
                DDG.PocRecovery.Service.SetTrackingList(name_list, value_list, false);
            }
        }

        /// <summary>
        /// Callback function to set an IP token for Recovery to any value. format for token entry is Token1=3|Toke2=2.
        /// </summary>
        /// <param name="args">Argument String as delimited string of token names to initialise to 0 using either " " or "|".</param>
        public static void SetSOCRecoveryToken(string args)
        {
            /* Parse arguments is a simple function to parse string entered by user calling the callback */
            var input_string = ParseArguments(args);
            var name_holder = new List<string>();
            var input_values = new List<int>();
            /* Create a value entry for each user input token to set into Prime shared storage database */
            for (var i = 0; i < input_string.Count; i++)
            {
                /* Quick check to determine if arguments were correctly entered */
                if (!input_string[i].Contains('=') || input_string[i] == string.Empty)
                {
                    throw new TestMethodException("Argument is not entered properly format TOKENNAME=integervalue");
                }

                input_values.Add(input_string[i].Split('=')[1].ToInt());
                name_holder.Add(input_string[i].Split('=')[0]);
            }

            /* Make inputs into format taken in by POC recovery interface */
            var name_list = string.Join("|", name_holder);
            var value_list = string.Join("|", input_values);
            /* Make sure none of the input strings are null */
            if (!string.IsNullOrEmpty(name_list))
            {
                // Call the set tracking list function.
                DDG.PocRecovery.Service.SetTrackingList(name_list, value_list, false);
            }
        }

        /// <summary>
        /// Print out recovery token to ituff in DiePar format.
        /// </summary>
        /// <param name="args">Argument string containing serial recovery token name to print values to ituff in mrslt format.</param>
        public static void PrintTokenToItuffDLCP(string args)
        {
            /* Simple debug info to show user inputs */
            /* Parse arguments is a simple function to parse string entered by user calling the callback */
            var input_string = ParseArguments(args);
            /* Make sure none of the input strings are null */
            if (!string.IsNullOrEmpty(input_string[0]))
            {
                // Call the set tracking list function.
                var value_holder = DDG.PocRecovery.Service.ReadSerialSharedStorage(input_string[0]);
                for (var i = 0; i < value_holder.Count; i++)
                {
                    var temp_string = value_holder[i].ToString();
                    DDG.PocRecovery.Service.PrintValToItuff(input_string[0] + "_" + i, temp_string.ToInt());
                }
            }
        }

        // TODO: make this a base function.
        private static List<string> ParseArguments(string argStr)
        {
            if (string.IsNullOrEmpty(argStr))
            {
                throw new TestMethodException("Argument is empty or null.");
            }

            var variable = new List<string>();
            var delims = new[] { ' ', '|' };
            var splitArgs = argStr.Trim().Split(delims, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < splitArgs.Length; i++)
            {
                Prime.Services.ConsoleService.PrintDebug($"[{splitArgs[i]}]");
                variable.Add(splitArgs[i]);
            }

            return variable;
        }
    }
}
