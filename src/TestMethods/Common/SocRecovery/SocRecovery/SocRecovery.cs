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

namespace SocRecovery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Prime;
    using Prime.Base.Exceptions;
    using Prime.PhAttributes;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class SocRecovery : TestMethodBase
    {
        /// <summary>
        /// Enumerate for setting different functions in test program.
        /// </summary>
        public enum Modes
        {
            /// <summary>
            /// Inits one specified rcovery token, containing a character for each recovery IP to a value of '0'.
            /// </summary>
            SerialMode_Init,

            /// <summary>
            /// Inits one specified rcovery token, containing a character for each recovery IP to a value of '0'.
            /// </summary>
            SerialMode_FlowControl,

            /// <summary>
            /// Sets tokens in serial mode. SerialModeInit must be run first. ValueList must be set, with length specified when SerialMode init was run. X denotes no change to previous value,0-9 characters can be set according.
            /// </summary>
            SerialMode_SetToken,

            /// <summary>
            /// Inits all tokens to 0 value so they are available in the prime shared storage database.
            /// </summary>
            Init,

            /// <summary>
            /// Mode to read in recovery tokens and set output port based on their values.
            /// </summary>
            FlowControl,

            /// <summary>
            /// Set the recovery token values specified with an error check mode to prevent invalid cases for partial recovery.
            /// </summary>
            SetToken_ErrorCheck,

            /// <summary>
            /// Set the recovery token to specified value without error check mode.
            /// </summary>
            SetToken,

            /// <summary>
            /// Mode to output Binary version of recovery token.
            /// </summary>
            BinaryConversion,

            /// <summary>
            /// Token Print to iTuff
            /// </summary>
            Token_Print,

            /// <summary>
            /// Set token mode for 2 bit recovery to prevent certain error conditions with 2 bit wide partial recovery.
            /// </summary>
            SerialMode_SetToken_ErrorCheck,
        }

        /// <summary>
        /// Gets or sets recovery test mode.
        /// </summary>
        public Modes RecoveryMode { get; set; } = Modes.Init;

        /// <summary>
        /// Gets or sets list of recovery tokens delimited with | or spaces.
        /// Required for all modes.
        /// </summary>
        public TestMethodsParams.String TokenNames { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets list of recovery values to set tokens with, number of values must match number of tokens.
        /// Required for SetToken, SetToken_ErrorCheck.
        /// </summary>
        public TestMethodsParams.String ValueList { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets length of serial token during init,Required for Serial mode init mode. Specifies length of serial recovery string.
        /// </summary>
        public TestMethodsParams.Integer SerialModeLength { get; set; } = null;

        /// <inheritdoc />
        public override void Verify()
        {
            /*Variable to track verify status.*/
            /* Verify Checks required for all modes */
            var verify_status = this.VerifyCheck_TokenNames();
            /* Unique verify function for SetToken modes */
            if (this.RecoveryMode == Modes.SetToken || this.RecoveryMode == Modes.SetToken_ErrorCheck)
            {
                verify_status &= this.VerifyCheck_ValueList();
            }

            if (this.RecoveryMode == Modes.SerialMode_Init)
            {
                verify_status &= this.VerifyCheck_LengthCheck();
            }

            if (this.RecoveryMode == Modes.SerialMode_SetToken || this.RecoveryMode == Modes.SerialMode_FlowControl)
            {
                verify_status &= this.VerifyCheck_SerialMode_SetToken();
            }

            if (this.RecoveryMode == Modes.BinaryConversion)
            {
                verify_status &= this.VerifyCheck_BinaryMode_SetToken();
            }

            if (!verify_status)
            {
                throw new Exception($"{this.InstanceName} failed verification");
            }
        }

        /// <summary>
        /// Method for verifying token names have been set.
        /// 1.Check token_name list is not empty.
        /// </summary>
        /// <returns>bool value to set pass/fail.</returns>
        public bool VerifyCheck_TokenNames()
        {
            if (this.TokenNames == string.Empty)
            {
                Services.ConsoleService.PrintDebug("Null string for TokenName/s");
                return false;
            }

            return true;
        }

        /// <summary>
        /// VerifyCheck_ValueList checks set token modes during verify for.
        /// 1. value_list parameter has been set.
        /// 2. value_list count and token_name count match.
        /// 3. value_list values are positive and less than 10.
        /// </summary>
        /// <returns>bool value to set pass/fail for verify.</returns>
        public bool VerifyCheck_ValueList()
        {
            /*Variables for functions */
            var valueListString = this.ValueList.ToString();
            var nameListString = this.TokenNames.ToString();
            /*Check value list entered by user is not empty */
            if (valueListString == string.Empty)
            {
                Services.ConsoleService.PrintDebug("Null string for ValueList");
                return false;
            }

            var token_value_list = valueListString.Split('|').Select(int.Parse).ToList(); // Convert user input value list into int list .
            var token_name_list = nameListString.Split('|').ToList();                  // Convert user input name list into string list .

            /* Check all values for each user input value are positive and less than max port limit */
            for (var i = 0; i < token_value_list.Count; i++)
            {
                if (token_value_list[i] < 0 || token_value_list[i] > 10)
                {
                    Services.ConsoleService.PrintDebug("Token Value falls outside of valid ranges");
                    Services.ConsoleService.PrintDebug("Token should be positive and less than 10");
                    return false;
                }
            }

            /* Check that value count matches number of token names */
            if (token_value_list.Count != token_name_list.Count)
            {
                Services.ConsoleService.PrintDebug("Token name count does not match Value Count");
                Services.ConsoleService.PrintDebug($"User input errors: Name inputs {token_value_list.Count}, Value Inputs: [{token_name_list.Count}] ");
                return false;
            }

            return true;
        }

        /// <summary>
        /// VerifyCheck_LengthCheck checks length is set for the serial mode init.
        /// </summary>
        /// <returns>bool value to set pass/fail for verify.</returns>
        public bool VerifyCheck_LengthCheck()
        {
            /*Variables for functions */
            if (this.SerialModeLength == null)
            {
                Services.ConsoleService.PrintDebug("Null input for SerialModeLength");
                return false;
            }

            if (this.SerialModeLength == -1 && this.ValueList == string.Empty)
            {
                Prime.Services.ConsoleService.PrintDebug("Setting length parameter to -1 will overide init token with value list.");
                Prime.Services.ConsoleService.PrintDebug("Value list is empty.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// VerifyCheck_SerialMode_SetToken checks set token modes during verify for.
        /// 1. value_list parameter has been set.
        /// </summary>
        /// <returns>bool value to set pass/fail for verify.</returns>
        public bool VerifyCheck_SerialMode_SetToken()
        {
            /*Variables for functions */
            var valueListString = this.ValueList.ToString();
            /*Check value list entered by user is not empty */
            if (valueListString == string.Empty)
            {
                Services.ConsoleService.PrintDebug("Null string for ValueList");
                return false;
            }

            return true;
        }

        /// <summary>
        /// VerifyCheck_binary mode checks set BinaryConversion modes during verify for.
        /// 1. value_list parameter has been set.
        /// 2. No bit width specified exceed 3 (would require binary representation > single char.
        /// </summary>
        /// <returns>bool value to set pass/fail for verify.</returns>
        public bool VerifyCheck_BinaryMode_SetToken()
        {
            /*Variables for functions */
            var token_name_split = new List<string>();
            var valueListString = this.ValueList.ToString();
            var integerList = new List<int>();
            /*Check value list entered by user is not empty */
            if (valueListString == string.Empty)
            {
                Services.ConsoleService.PrintDebug("Null string for ValueList");
                return false;
            }

            var name_length_split = valueListString.Split('|').ToList();
            foreach (var sp in name_length_split)
            {
                Services.ConsoleService.PrintDebug($"Test: {sp.Split('_')[0]}");
                Services.ConsoleService.PrintDebug($"Test: {sp.Split('_')[1]}");
                var split_temps = sp.Split('_')[0];
                token_name_split.Add(split_temps);
                split_temps = sp.Split('_')[1];
                integerList.Add(int.Parse(split_temps));
            }

            // integerList = valueListString.Split('|').Select(int.Parse).ToList();
            if (integerList.Min() < 1 || integerList.Max() > 3)
            {
                Services.ConsoleService.PrintDebug("Values entered in BinaryConversion mode need to represent bit width for single character: 1,2 or 3 bits wide only");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        [Returns(9, PortType.Fail, "Fail PORT. Flow Routing for recovery.")]
        [Returns(8, PortType.Pass, "PASS PORT. Flow Routing for recovery.")]
        [Returns(7, PortType.Pass, "PASS PORT. Flow Routing for recovery.")]
        [Returns(6, PortType.Pass, "PASS PORT. Flow Routing for recovery.")]
        [Returns(5, PortType.Pass, "PASS PORT. Flow Routing for recovery.")]
        [Returns(4, PortType.Pass, "PASS PORT. Flow Routing for recovery.")]
        [Returns(3, PortType.Pass, "PASS PORT. Flow Routing for recovery.")]
        [Returns(2, PortType.Pass, "PASS PORT. Flow Routing for recovery.")]
        [Returns(1, PortType.Pass, "PASS PORT")]
        [Returns(0, PortType.Fail, "PASS PORT. Flow routing for recovery.")]

        public override int Execute()
        {
            /* User Input Mode sets execution mode */
            /* Init mode initialises token list to "0" */
            if (this.RecoveryMode == Modes.Init)
            {
                return this.ExecuteInitMode();
            }

            if (this.RecoveryMode == Modes.FlowControl)
            {
                /* Flow control mode reads in current token values and sets port based on values*/
                return this.ExecuteFlowControlMode();
            }

            if (this.RecoveryMode == Modes.SetToken)
            {
                /*SetToken mode sets token names to token values*/
                return this.ExecuteSetTokenMode();
            }

            if (this.RecoveryMode == Modes.SetToken_ErrorCheck)
            {
                /*SetToken_ErrorCheck mode sets token names to token values
                 Additional Error checking to read in past value to prevent tokens from being reset to Valid.
                 If any token is set to 1, error out.
                 If next value and past value are >2 they must be the same.
                 If next value is less than 1 and past value is >2 error out to prevent recovering die.
                */
                return this.ExecuteSetTokenModeErrorCheck();
            }

            if (this.RecoveryMode == Modes.SerialMode_Init)
            {
                /*Initialises single Recovery token with N '0' characters, where n is specified by length user input parameter.
                */
                return this.ExecuteSerialInitMode();
            }

            if (this.RecoveryMode == Modes.SerialMode_SetToken)
            {
                /*Set Token in serial modes. User enters in value parameter a string containing values to set to string. X in character position will skip setting token. '1' -> '9' can be used to set token value.
                */
                return this.ExecuteSerialSetTokenMode();
            }

            if (this.RecoveryMode == Modes.SerialMode_SetToken_ErrorCheck)
            {
                /*TODO: Add support for 3 bit wide recovery. Set Token in serial modes. User enters in value parameter a string containing values to set to string. X in character position will skip setting token. '1' -> '9' can be used to set token value.
                  Detects Error condition to prevent losing recovery information. old value >0 cannot be set to 1; old value of 2 setting 3 will resolve to 1; old value of 3 setting 2 will resolve to 1;*/
                return this.ExecuteSerialSetTokenMode_ErrorCheck();
            }

            if (this.RecoveryMode == Modes.SerialMode_FlowControl)
            {
                /*Set Token in serial modes. User enters in value parameter a string containing values to set to string. X in character position will skip setting token. '1' -> '9' can be used to set token value.
                */
                return this.ExecuteSerialFlowControlMode();
            }

            if (this.RecoveryMode == Modes.BinaryConversion)
            {
                return this.BinaryConversion();
            }

            if (this.RecoveryMode == Modes.Token_Print)
            {
                return this.PrintTokenToItuff();
            }

            /* Error invalid mode was set */
            return 0;
        }

        /// <summary>
        /// Method for Init Mode.
        /// Sets all entered tokens to 0. Should be run only once per die execution.
        /// </summary>
        /// <returns>bool value to set pass/fail.</returns>
        public int ExecuteInitMode()
        {
            Services.ConsoleService.PrintDebug("Init Mode execution Active");
            /* Initialise variables */
            var token_name_holder = this.TokenNames.ToString();
            var token_name_list = new List<string>(token_name_holder.Split('|'));
            var input_values = new List<int>();
            Prime.Services.ConsoleService.PrintDebug($"[TokenNames={this.TokenNames}].");
            /* Create a 0 value for each token name */
            for (var i = 0; i < token_name_list.Count; i++)
            {
                input_values.Add(0);
            }

            Prime.Services.ConsoleService.PrintDebug($"[TokenNames={input_values}].");
            /* Put value list into format required for Recovery set list functions */
            var value_list = string.Join("|", input_values);
            /* call the POC recovery service to set token values to 1 in the Prime shared storage database */
            DDG.PocRecovery.Service.SetTrackingList(token_name_holder, value_list, false);

            Services.ConsoleService.PrintDebug($"Token List will be initialised with a value of 0 [{this.TokenNames}]");
            /* return out port 1 if everything has passed */
            return 1;
        }

        /// <summary>
        /// Method for Serial Init Mode.
        /// Sets all entered tokens to 0 and stores them in one token. To be used in the init flow.
        /// </summary>
        /// <returns>bool value to set pass/fail.</returns>
        public int ExecuteSerialInitMode()
        {
            /* Initialise variables */
            var token_name_holder = this.TokenNames.ToString();
            var override_init = this.ValueList.ToString();
            var input_values = new List<char>();

            // Give user the ability to override init values, only works if value_list parameter is set and length parameter is set to 0.
            if (override_init.Length > 0 && this.SerialModeLength == -1)
            {
                Prime.Services.ConsoleService.PrintDebug("length set to -1, overriding init values with value_list.");
                input_values = override_init.ToList();
                /* Store values in the shared storage data base under one Recovery variable */
                DDG.PocRecovery.Service.SerialSetTracking(token_name_holder, input_values);
                return 1;
            }

            /* Create a 0 value for each token name */
            for (var i = 0; i < this.SerialModeLength; i++)
            {
                input_values.Add('0');
            }

            /* Store values in the shared storage data base under one Recovery variable */
            DDG.PocRecovery.Service.SerialSetTracking(token_name_holder, input_values);

            /* return out port 1 if everything has passed */
            return 1;
        }

        /// <summary>
        /// Method for executing set token mode.
        /// Method sets list of tokens with user specified values.
        /// </summary>
        /// <returns>bool value to set pass/fail.</returns>
        public int ExecuteSetTokenMode()
        {
            Services.ConsoleService.PrintDebug("Set Token Mode execution");
            /* Initialise variables */
            var token_name_holder = this.TokenNames.ToString();
            var value_list_holder = this.ValueList.ToString();
            new List<string>(token_name_holder.Split('|'));
            /* Call the set tracking list */
            return DDG.PocRecovery.Service.SetTrackingList(token_name_holder, value_list_holder, false);
        }

        /// <summary>
        /// Method for settng bits in recovery character string.
        /// Method sets each character as specified in value_list. All recovery bits must be set, according to token length set in SerialModeInit.X value will skip bit position.
        /// </summary>
        /// <returns>int value to set pass/fail.</returns>
        public int ExecuteSerialSetTokenMode()
        {
            Services.ConsoleService.PrintDebug("Serial Mode Set Token Mode execution");
            /* Initialise variables */
            var token_name_holder = this.TokenNames.ToString();
            var value_list_holder = this.ValueList.ToString();
            var input_values = value_list_holder.Split('|').Select(char.Parse).ToList();
            /* Call the set tracking list */
            var past_values = DDG.PocRecovery.Service.ReadSerialSharedStorage(token_name_holder);

            if (past_values.Count != input_values.Count)
            {
                Prime.Services.ConsoleService.PrintError($"Recovery Token String does not match input length specified. Check Number of bits in input string is {past_values}");
                throw new TestMethodException("Invalid length of input variable");
            }

            for (var i = 0; i < input_values.Count; i++)
            {
                if (input_values[i] == 'X')
                {
                    Services.ConsoleService.PrintDebug($"X (Do not care) detected in char position {i}, past value [{past_values[i]}] is not being updated");
                    input_values[i] = past_values[i];
                }
            }

            // Do not write to shared storage service if its not required (previous token matches new token to set.
            if (past_values != input_values)
            {
                Services.ConsoleService.PrintDebug($"Setting {token_name_holder}, to {input_values}");
                DDG.PocRecovery.Service.SerialSetTracking(token_name_holder, input_values);
            }
            else
            {
                Prime.Services.ConsoleService.PrintError("Recovery Token String was not written as no change was detected.");
            }

            return 1;
        }

        /// <summary>
        /// Method for settng bits in recovery character string.
        /// Method sets each character as specified in value_list. All recovery bits must be set, according to token length set in SerialModeInit.X value will skip bit position.
        /// Method adds some additional error checks to ensure certain error cases are not triggered.
        /// Past value > 0, New value can not set 0.
        /// </summary>
        /// <returns>int value to set pass/fail.</returns>
        public int ExecuteSerialSetTokenMode_ErrorCheck()
        {
            Services.ConsoleService.PrintDebug("Serial Mode Set Token Error Check Mode execution");
            /* Initialise variables */
            var token_name_holder = this.TokenNames.ToString();
            var value_list_holder = this.ValueList.ToString();
            var input_values = value_list_holder.Split('|').Select(char.Parse).ToList();
            /* Call the set tracking list */
            var past_values = DDG.PocRecovery.Service.ReadSerialSharedStorage(token_name_holder);

            if (past_values.Count != input_values.Count)
            {
                Prime.Services.ConsoleService.PrintError($"Recovery Token String does not match input length specified. Check Number of bits in input string is {past_values}");
                throw new TestMethodException("Invalid length of input variable");
            }

            for (var i = 0; i < input_values.Count; i++)
            {
                Services.ConsoleService.PrintDebug($"Char position {i}, past value [{past_values[i]}] Requested Value: {input_values[i]} ");
                if (input_values[i] == 'X')
                {
                    Services.ConsoleService.PrintDebug($"X (Do not care) detected in char position {i}, past value [{past_values[i]}] is not being updated");
                    input_values[i] = past_values[i];
                    continue;
                }

                var past_value = int.Parse(past_values[i].ToString());
                var next_value = int.Parse(input_values[i].ToString());
                if (next_value > 1 && past_value == 1)
                {
                    Services.ConsoleService.PrintDebug($"Past token value was {past_values[i]}, New value requested is {input_values[i]} ");
                    Services.ConsoleService.PrintDebug("Error check mode prevents user from setting IP from 1 (fully defeatured) to partial defeatured state ");
                    Services.ConsoleService.PrintDebug("Past token value will not be changed from 1 ");
                    input_values[i] = past_values[i];
                }
                else if (next_value == 0 && past_value > 0)
                {
                    Services.ConsoleService.PrintDebug($"Past token value was {past_values[i]}, New value requested is {input_values[i]} ");
                    Services.ConsoleService.PrintDebug("Error check mode prevents user from setting IP back to fully functional state '0' ");
                    Services.ConsoleService.PrintDebug("Past token value will not be changed ");
                    input_values[i] = past_values[i];
                }
                else if ((next_value == 2 && past_value == 3) || (next_value == 3 && past_value == 2))
                {
                    Services.ConsoleService.PrintDebug($"Past token value was {past_values[i]}, New value requested is {input_values[i]} ");
                    Services.ConsoleService.PrintDebug("Error check mode setting new value to 1 or fully defeatured ");
                    input_values[i] = '1';
                }
            }

            // Do not write to shared storage service if its not required (previous token matches new token to set).
            if (past_values != input_values)
            {
                Services.ConsoleService.PrintDebug($"Setting {token_name_holder}, to {input_values}");
                DDG.PocRecovery.Service.SerialSetTracking(token_name_holder, input_values);
            }
            else
            {
                Prime.Services.ConsoleService.PrintError("Recovery Token String was not written as no change was detected.");
            }

            return 1;
        }

        /// <summary>
        /// Method for executing set token mode when set with an in built error check mode.
        /// Reads back past token value. If the past token value is >1 and new value to set is >1 checks to see if they are equal.
        /// If they are, token will be set, if they are not exit out port 0.
        /// </summary>
        /// <returns>bool value to set pass/fail.</returns>
        public int ExecuteSetTokenModeErrorCheck()
        {
            Services.ConsoleService.PrintDebug("Set Token Mode execution with error checking enabled");
            /* Initialise variables */
            var token_name_holder = this.TokenNames.ToString();
            var value_list_holder = this.ValueList.ToString();
            var token_name_list = new List<string>(token_name_holder.Split('|'));
            var input_values = value_list_holder.Split('|').Select(int.Parse).ToList();
            /* Get list of values tokens were set to previously */
            var past_input_values = DDG.PocRecovery.Service.GetTrackingList(token_name_holder);
            /* Cycle through each token, to compare value to set and previous token value */
            for (var i = 0; i < past_input_values.Count; i++)
            {
                /* Invalid Case: If token was flagged for recovery but is trying to reset token to pass value */
                if ((past_input_values[i] > 1 && input_values[i] <= 1) && (past_input_values[i] != input_values[i]))
                {
                    Prime.Services.ConsoleService.PrintDebug("[FAILING CASE - MOVING FROM VALID RECOVERY SCENARIO TO INVALID SCENARIO]");
                    Prime.Services.ConsoleService.PrintDebug($"[Previous Value for token {token_name_list[i]} ={past_input_values[i]}].");
                    Prime.Services.ConsoleService.PrintDebug($"[Requested Value for token {token_name_list[i]} ={input_values[i]}].");
                    return 0;
                } /* Invalid Case: If token was set to recovery scenario >1 and is being set to a different recovery. Triggers error to prevent case where content triggers different recovery sscenarios which combine to create invalid.*/

                if ((past_input_values[i] > 1 && input_values[i] > 1) && (past_input_values[i] != input_values[i]))
                {
                    Prime.Services.ConsoleService.PrintDebug("[FAILING CASE - TOKENS ARE DIFFERENT AND BOTH GREATER THAN 1]");
                    Prime.Services.ConsoleService.PrintDebug($"[Previous Value for token {token_name_list[i]} ={past_input_values[i]}].");
                    Prime.Services.ConsoleService.PrintDebug($"[Requested Value for token {token_name_list[i]} ={input_values[i]}].");
                    return 0;
                } /* Valid Case: If token was flagged for recovery but is trying to reset token to pass value */

                if ((past_input_values[i] < 1 || input_values[i] > 1) && (past_input_values[i] != input_values[i]))
                {
                    Prime.Services.ConsoleService.PrintDebug("[VALID RECOVERY CASE - TOKEN IS BEING SET TO A RECOVERY STATE]");
                    Prime.Services.ConsoleService.PrintDebug($"[Previous Value for token {token_name_list[i]} ={past_input_values[i]}].");
                    Prime.Services.ConsoleService.PrintDebug($"[Requested Value for token {token_name_list[i]} ={input_values[i]}].");
                }
            }

            Prime.Services.ConsoleService.PrintDebug("Setting tokens, no invalid cases detected");
            /*Error checks have passed and tracking list will be set.*/
            DDG.PocRecovery.Service.SetTrackingList(token_name_holder, value_list_holder, false);
            return 1;
        }

        /// <summary>
        /// Method for setting flow based on value of specified tokens.
        /// If tokns are all set to 1 exit out port 1.
        /// If any token is 0 set port to 0.
        /// If any token is >1 then set the output port to the max token value specified.
        /// </summary>
        /// <returns>integer to set the return port.</returns>
        public int ExecuteFlowControlMode()
        {
            Services.ConsoleService.PrintDebug("Flow Control Mode execution");
            var token_name_holder = this.TokenNames.ToString();
            var token_name_list = new List<string>(token_name_holder.Split('|'));
            /* Read back values tokens are set to. */
            var return_values = DDG.PocRecovery.Service.GetTrackingList(token_name_holder);
            /* Check if count of 1 values is greater than 1, if it is then exit out port 0 */
            if (return_values.Count(n => n == 1) > 0)
            {
                return 0;
            }

            /* Check if any value is greater than 1 and return out port corresponding to the maximum value */
            if (return_values.Count(n => n > 1) > 0)
            {
                return return_values.Max();
            }

            /* Otherwise all values in the tokens should be set to 0. */
            if (return_values.Count(n => n == 0) == return_values.Count)
            {
                return 1;
            }

            Services.ConsoleService.PrintError($"Invalid case detected, exiting out port -1.Tokens = {token_name_list} Values = {return_values}");
            return -1;
        }

        /// <summary>
        /// Serial mode implementation for flow control.
        /// Reads back prime recovery token character string. User inputs value_string to select which bits to use in the value comparison. X skips bit,.
        /// If any token is >1 then set the output port to the max token value specified.
        /// Full token list is read from the prime shared storage database. Value list is a required input - 1 indicates token is used in flow control logic, X indicates token is not used.
        /// </summary>
        /// <returns>integer to set the return port.</returns>
        public int ExecuteSerialFlowControlMode()
        {
            Services.ConsoleService.PrintDebug("Serial Flow Control Mode execution");
            var token_name_holder = this.TokenNames.ToString();
            var token_value_holder = this.ValueList.ToString();
            var int_lookup = new HashSet<char> { '0', '1', '2', '3', '4', '5', '6', '7' };

            /* Read back token stored in prime shared storage database */
            var return_values = DDG.PocRecovery.Service.ReadSerialSharedStorage(token_name_holder);
            /* Read in the user input token specifier string */
            var token_selector = token_value_holder.Split('|').Select(char.Parse).ToList();
            /* int List intended to hold only token values integer value. */
            var integer_token_converter = new List<int>();
            /* iterate through each bit in the recovery string */

            // Check to ensure user input selector string matches the value list read back.
            if (token_selector.Count != return_values.Count)
            {
                Prime.Services.ConsoleService.PrintError(" Invalid case detected, token selector string entered does not match recovery string length");
                return -1;
            }

            for (var i = 0; i < token_selector.Count; i++)
            {
                // If the user input token selector string equals X skip this element.
                if (token_selector[i] == 'X')
                {
                    Prime.Services.ConsoleService.PrintDebug($"[Skipping index {i} = X value detected.");
                }
                else if (int_lookup.Contains(return_values[i]))
                {
                    // Otherwise check if prime shared storage values are 0 - 9 using lookup table check.
                    // Code contains valid character case - convert to int and set port accordingly.
                    integer_token_converter.Add((int)char.GetNumericValue(return_values[i]));
                }
                else
                {
                    // value is not X or 0 - 9 - invalid case exit out.
                    Prime.Services.ConsoleService.PrintError($"[Index {i} contains error in input string. X or 0 - 9 accepted");
                    return -1;
                }
            }

            // Check to make sure the list of integers collected is not empty.
            if (integer_token_converter.Count == 0)
            {
                Prime.Services.ConsoleService.PrintError("[something went wrong, value string must contain at least one '1'");
                return -1;
            }

            // Can only exit out one port - select the max of all token values read back.
            var max_token_value = integer_token_converter.Max();

            // All values are 0, exits out port 1 since recovery convention uses 1 to represent failing case.
            if (integer_token_converter.Count(n => n == 0) == integer_token_converter.Count)
            {
                return 1;
            }

            // Any values are 1, exits out port 0 since recovery convention uses 1 to represent failing case.
            if (integer_token_converter.Contains('1'))
            {
                return 0;
            }

            // Any values are 9, exits out port -1 since recovery convention uses 9 to represent software fail case.
            if (integer_token_converter.Contains('9'))
            {
                return -1;
            }

            // Max token value of 1, exits out port 0 since recovery convention uses 1 to represent failing case.
            return max_token_value == 1 ? 0 : max_token_value;
        }

        /// <summary>
        /// Method to convert token to binary.
        /// User enters recovery tracking structure name, and value list containing bit length (1,2 or 3) for each bit in recovery string.
        /// new token will be created in binary string format.
        /// </summary>
        /// <returns>integer to set the return port.</returns>
        public int BinaryConversion()
        {
            Services.ConsoleService.PrintDebug("Converting token string to binary structure.");
            var token_name_holder = this.TokenNames.ToString();
            var token_value_holder = this.ValueList.ToString();
            var binary_string = string.Empty;
            var token_name_split = new List<string>();
            var token_selector = new List<int>();
            /* Read back token stored in prime shared storage database */
            var return_values = DDG.PocRecovery.Service.ReadSerialSharedStorage(token_name_holder);
            var name_length_split = token_value_holder.Split('|').ToList();

            // Check to ensure user input selector string matches the value list read back.
            if (name_length_split.Count != return_values.Count)
            {
                Prime.Services.ConsoleService.PrintError(" Invalid case detected, bit length string entered does not match recovery string length");
                return -1;
            }

            for (var i = 0; i < name_length_split.Count; i++)
            {
                token_name_split.Add(name_length_split[i].Split('_')[0]);
                token_selector.Add(Convert.ToInt32(name_length_split[i].Split('_')[1]));
            }

            for (var i = 0; i < token_selector.Count; i++)
            {
                var integer_value = (int)char.GetNumericValue(return_values[i]);
                integer_value = this.TableRemap(integer_value, token_selector[i]);
                var single_binary = this.ToBinary(integer_value);

                if (token_selector[i] < 1 || token_selector[i] > 3)
                {
                    Prime.Services.ConsoleService.PrintError(" Error , token selector value is to slow");
                    return -1;
                }

                Prime.Services.ConsoleService.PrintDebug($"Padding with = {token_name_split[i]}");
                single_binary = single_binary.PadLeft(token_selector[i], '0');
                Prime.Services.SharedStorageService.InsertRowAtTable("Binary_" + token_name_holder + "_" + token_name_split[i], single_binary, Prime.SharedStorageService.Context.DUT);
                binary_string = binary_string + single_binary;
            }

            // Prime.Services.ConsoleService.PrintDebug($"Binary_Conversion = {binary_string}");
            // Prime.Services.ConsoleService.PrintDebug($"Storing in shared storage database as Binary_{token_name_holder} ");
            Prime.Services.SharedStorageService.InsertRowAtTable("Binary_" + token_name_holder, binary_string, Prime.SharedStorageService.Context.DUT);
            return 1;
        }

        /// <summary>
        /// Method to convert token to binary.
        /// User enters recovery tracking structure name, and value list containing bit length (1,2 or 3) for each bit in recovery string.
        /// new token will be created in binary string format.
        /// </summary>
        /// <param name="num"> number to convert to binary.</param>
        /// <returns>binary string converted.</returns>
        public string ToBinary(int num)
        {
            if (num < 2)
            {
                // Prime.Services.ConsoleService.PrintDebug($"single digit {num}");
                return num.ToString();
            }

            var divisor = num / 2;
            var remainder = num % 2;

            // Prime.Services.ConsoleService.PrintDebug($"divisor is = {divisor} remainder is {remainder}");
            return this.ToBinary(divisor) + remainder;
        }

        /// <summary>
        /// Method to remap integer value to facilitate binary conversion to represent char table.
        /// User enters integer value to remap, and bit length required for binary conversion.
        /// return value can be used in binary conversion to map to the correct value.
        /// token length of 2 bits will remap '1'(01) to 3(11) and '3'(11) to '1' (01).
        /// token length of 3 bits will remap '1' to 7 (111) and '7(111) to 1 (001).
        /// </summary>
        /// <param name="num"> number to remap based on bit_length.</param>
        /// <param name="bit_length"> Specifies the user input bit length to return when converting token to binary.</param>
        /// <returns>integer used to convert binary.</returns>
        public int TableRemap(int num, int bit_length)
        {
            // No need to do anything with 1 bit representation
            if (bit_length == 1)
            {
                return num;
            }

            // Bit length of 2 will require swapping 1 and 3 (11 needs to be in '1' slot)  values for binary conversion
            if (bit_length == 2)
            {
                if (num == 3)
                {
                    return 1;
                }

                return num == 1 ? 3 : num;
            }

            // Bit length of 3 will require swapping 1 and 7 (112 needs to be in '1' slot)  values for binary conversion
            if (num == 7)
            {
                return 1;
            }

            return num == 1 ? 7 : num;
        }

        /// <summary>
        /// Function to dump prime shared storage to console and print token value character by character in ituff.
        /// </summary>
        /// <returns> Returns pass/fail status of test.</returns>
        public int PrintTokenToItuff()
        {
            // var logLevel = Prime.Services.TestProgramService.GetCurrentTestInstanceParameter("LogLevel");
            /* Simple debug info to show user inputs */

            // Prime.Services.ConsoleService.PrintDebug($"[{instanceName}][LogLevel={logLevel}] Running SetSocToken with Args=[{args}].");

            /*Pull in user input specifier for token name */
            var input_string = this.TokenNames.ToString();
            var token_override_string = this.ValueList.ToString();
            var token_override = new List<string>();

            if (token_override_string != string.Empty && token_override_string.Contains("|"))
            {
                token_override = token_override_string.Split('|').ToList();
            }

            /* Make sure none of the input strings are null */
            if (!string.IsNullOrEmpty(input_string))
            {
                // Call the set tracking list function.
                var value_holder = DDG.PocRecovery.Service.ReadSerialSharedStorage(input_string);
                if (token_override.Count != value_holder.Count)
                {
                    Prime.Services.ConsoleService.PrintDebug("Value List entered has been parsed and does not match number of chars in recovery string");
                    Prime.Services.ConsoleService.PrintDebug("Value List inputs will not be used to override the token name");
                    for (var i = 0; i < value_holder.Count; i++)
                    {
                        Prime.Services.ConsoleService.PrintDebug($"Printing token pos {i} with value:[{double.Parse(value_holder[i].ToString())}");
                        DDG.PocRecovery.Service.PrintValToItuff("_" + input_string + i, double.Parse(value_holder[i].ToString()));
                    }

                    return 1;
                }

                Prime.Services.ConsoleService.PrintDebug("Value List entered has been parsed and values entered will be used to override token name");
                for (var i = 0; i < value_holder.Count; i++)
                {
                    Prime.Services.ConsoleService.PrintDebug($"Printing token pos {i} with value:[{double.Parse(value_holder[i].ToString())}");
                    Prime.Services.ConsoleService.PrintDebug($"Overriding {i} with name:[{token_override[i]}");
                    DDG.PocRecovery.Service.PrintValToItuff("_" + token_override[i], double.Parse(value_holder[i].ToString()));
                }

                return 1;
            }

            return 0;
        }
    }
}