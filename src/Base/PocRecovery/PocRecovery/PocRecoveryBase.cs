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

namespace PocRecoveryBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DDG;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    public class PocRecoveryBase : IPocRecovery
    {
        /// <summary>
        /// constant to hold the value for invalid "unrecoverable" token value setting.
        /// </summary>
        public const int UnrecoverableValue = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="PocRecoveryBase"/> class.
        /// </summary>
        public PocRecoveryBase()
        {
            this.Console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
        }

        /// <summary>
        /// Gets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; }

        /// <inheritdoc />
        public int GetTracking(string token_name)
        {
            /* Initialize return value to -99 to trigger an error if there is any undetected issue with the token read.*/
            int return_value = -99;
            /*Remove whitespaces from each token name and check for any null values in array*/
            if (token_name.Trim() == string.Empty)
            {
                /*Throw exception if token name entered is null before making Prime call*/
                throw new TestMethodException($"Token_name=[{token_name}] does not exist.");
            }

            /*Try to read token name from shared storage database, an error will be triggered if the token does not exist and exit port -1
              Using Get integer row on Prime V4 since HDMT simulator did not like get row                                                   */
            try
            {
                /*Return integer value with context set to DUT so only unit under test has access */
                return_value = Prime.Services.SharedStorageService.GetIntegerRowFromTable(token_name, Context.DUT);
                this.Console?.PrintDebug($"TokenName=[{token_name}], ValueReturned=[{return_value}]");
            }
            catch
            {
                /*Code will execute if shared storage getintegerrow has failed*/
                this.Console?.PrintDebug($"ERROR TokenName=[{token_name}], ValueReturned=[{return_value}]");
                Prime.Services.ConsoleService.PrintError($"Value does not exist in shared storage database");
                throw;
            }

            /*return value for input token as an integer*/
            return return_value;
        }

        /// <inheritdoc />
        public void SetTracking(string token_name, int value)
        {
            /*Remove whitespaces from each token name and check for any null values in user input array*/
            if (token_name.Trim() == string.Empty)
            {
                Prime.Services.ConsoleService.PrintError($"Token_name=[{token_name}] does not exist");
                throw new TestMethodException($"Token_name=[{token_name}] does not exist");
            }

            /* Try to insert row in prime shared storage database as integer.*/
            try
            {
                Prime.Services.SharedStorageService.InsertRowAtTable(token_name, value, Context.DUT);
                this.Console?.PrintDebug($"TokenName=[{token_name}], Writing=[{value}]");
            }
            catch
            {
                Prime.Services.ConsoleService.PrintError($"TokenName=[{token_name}]Value failed to be written to share storage database");
                throw;
            }
        }

        /// <inheritdoc />
        public int SetTrackingList(string name_list, string value_list, bool error_check = false)
        {
            /* Create a list of token names from the user delimited token_name parameter */
            List<string> names = new List<string>(name_list.Split('|'));
            /* Create a list of values to set as tokens from the user delimited value_list parameter */
            var values = value_list.Split('|').Select(int.Parse).ToList();
            /*Error check to ensure that number of token names and values match - should flag in verify as well*/
            if (names.Count != values.Count)
            {
                Prime.Services.ConsoleService.PrintError($"Input lengths are different lengths=[Name inputs = {names.Count}], [Value Inputs = {values.Count}");
                throw new TestMethodException($"Input lengths are different lengths=[Name inputs = {names.Count}], [Value Inputs = {values.Count}");
            }

            /* Optional error check mode to detect case where any valid is attempting to set an unrecoverable token value "1"*/
            /* Only enabled when error_check is true -will prevent user from setting recovery value to 1                     */
            for (int i = 0; i < values.Count; i++)
            {
                if (error_check && values[i] == UnrecoverableValue)
                {
                    throw new TestMethodException($"Error check enabled, error detected: TokenName=[{names[i]}], Writing=[{values[i]}]");
                }

                this.SetTracking(names[i], values[i]);
            }

            /*Return 1 to indicate successful execution */
            return 1;
        }

        /// <inheritdoc />
        public List<int> GetTrackingList(string name_list)
        {
            /* split user entered delimited token list into LIST names */
            List<string> names = new List<string>(name_list.Split('|'));
            /*Create empty List<int> to hold values for names */
            List<int> values = new List<int>();
            /*Iterate through each name entered by user and call getTracking to find the equivelent token.*/
            for (int i = 0; i < names.Count; i++)
            {
                values.Add(this.GetTracking(names[i]));
            }

            /*Return a list of integer values for each token name requested*/
            return values;
        }

        /// <inheritdoc />
        public void SerialSetTracking(string recoveryStringName, List<char> value)
        {
            var storval = string.Join(string.Empty, value);
            this.Console?.PrintDebug($"SharedStorage Write: 'recoveryStringName' : [{storval}]");
            this.Console?.PrintDebug($"SharedStorage Token Names Write: 'RecoveryTokenVariableName' : [{recoveryStringName}]");
            Prime.Services.SharedStorageService.InsertRowAtTable(recoveryStringName, storval, Context.DUT);
        }

        /// <inheritdoc />
        public List<char> ReadSerialSharedStorage(string recoveryStringName)
        {
            List<char> returnVal = new List<char>();
            string retrievedValue = Prime.Services.SharedStorageService.GetStringRowFromTable(recoveryStringName, Context.DUT);
            this.Console?.PrintDebug($"SharedStorage Read: {recoveryStringName} : [{retrievedValue.ToString()}]");
            returnVal = retrievedValue.ToCharArray().ToList<char>();
            return returnVal;
        }

        /// <inheritdoc />
        public void PrintValToItuff(string tname, double value)
        {
            var mtrsltWrite = Prime.Services.DatalogService.GetItuffMrsltWriter();

            // Add tname to token in ituff writer.
            mtrsltWrite.SetTnamePostfix(tname);

            mtrsltWrite.SetPrecision(1);

            // Set data into the iTuff writer.
            mtrsltWrite.SetData(Convert.ToDouble(value));

            // Write data to  iTuff.
            Prime.Services.DatalogService.WriteToItuff(mtrsltWrite);
        }

        /// <inheritdoc />
        public void PrintStringToItuff(string tname, string value)
        {
            var strsltWrite = Prime.Services.DatalogService.GetItuffStrgvalWriter();

            // Add tname to token in ituff writer.
            strsltWrite.SetTnamePrefix(tname);

            // Set data into the iTuff writer.
            strsltWrite.SetData(value);

            // Write data to  iTuff.
            Prime.Services.DatalogService.WriteToItuff(strsltWrite);
        }
    }
}