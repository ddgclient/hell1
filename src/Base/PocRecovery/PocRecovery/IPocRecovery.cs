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

namespace DDG
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for main PocRecovery object.
    /// </summary>
    public interface IPocRecovery
    {
        /// <summary>
        /// Function to serialise and store as a list in the shared token database.
        /// </summary>
        /// <param name="recoveryStringName">token name string used to store joined list of recovery tokens.</param>
        /// <param name="value">character value to set token value.</param>
        void SerialSetTracking(string recoveryStringName, List<char> value);

        /// <summary>
        /// Read back stored recovery variables.
        /// </summary>
        /// <param name="recoveryStringName">name of variable used to store joined list of recovery tokes.</param>
        /// <returns>list of recovery variables as a character string.</returns>
        List<char> ReadSerialSharedStorage(string recoveryStringName);

        /// <summary>
        /// Set a single integer as a token name.
        /// </summary>
        /// <param name="token_name">token name as string to set in prime shared storage database.</param>
        /// <param name="value">integer value to set token value.</param>
        void SetTracking(string token_name, int value);

        /// <summary>
        /// Get integer value from the shared storage database specified by token_name.
        /// </summary>
        /// <param name="token_name">Token name as string to retrieve from shared sorage database.</param>
        /// <returns>Value of token from the shares storage database. -99 is returned if value can not be found in shared storage database.</returns>
        int GetTracking(string token_name);

        /// <summary>
        /// Set a list of tokens with token names:name_list delimited by | with integer values:value_list in prime shared storage database.
        /// </summary>
        /// <param name="name_list">input a delimited list of token names to set within the shared storage database.</param>
        /// <param name="value_list">input list of int values as string delimited with | to set to corresponding token in shred storage database.</param>
        /// <param name="error_check">optional bool value to activate error check mode, returns zero if any token is being set to 0 (error case).</param>
        /// <returns>Return pass/fail status.</returns>
        int SetTrackingList(string name_list, string value_list, bool error_check = false);

        /// <summary>
        /// Method to read back a list of tokens from prime shared storage and store them in List Int format. Tokens must be initialised in prime shared storage db.
        /// User input list of token names delimited with "|".
        /// </summary>
        /// <param name="name_list">retrieve a list of tokens from prime shared storage database, string delimited with |.</param>
        /// <returns>List Int of values set for specified token names.</returns>
        List<int> GetTrackingList(string name_list);

        /// <summary>
        /// Writes values to ITUFF in mrslt format required for DLCP intercept.
        /// </summary>
        /// <param name="tname">Token name to print to ituff. </param>
        /// <param name="value"> Value to write to mrslt. </param>
        void PrintValToItuff(string tname, double value);

        /// <summary>
        /// Writes values to ITUFF in mrslt format required for UPS Fusing intercept.
        /// </summary>
        /// <param name="tname"> Token name in print to ituff. </param>
        /// <param name="value"> Value to write to ituff. </param>
        void PrintStringToItuff(string tname, string value);
    }
}
