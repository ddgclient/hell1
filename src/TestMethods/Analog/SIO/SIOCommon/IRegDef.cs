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

namespace SIO
{
    /// <summary>
    /// RegDef interface class.
    /// </summary>
    public interface IRegDef
    {
        /// <summary>
        /// Returns true if the RegDef Parser can decode the given port/lane/field.
        /// </summary>
        /// <param name="port">PORT.</param>
        /// <param name="lane">LANE.</param>
        /// <param name="field">FIELD.</param>
        /// <returns>true if this RegDef can decode the given port/lane/field.</returns>
        bool Contains(string port, string lane, string field);

        /// <summary>
        /// Decode the data for the given port/lane/field.
        /// </summary>
        /// <param name="dataHash">Hashed Data struct.</param>
        /// <param name="port">PORT.</param>
        /// <param name="lane">LANE.</param>
        /// <param name="field">FIELD.</param>
        /// <returns>Decoded Data.</returns>
        string GetData(SIOEDC_Util.HashedData dataHash, string port, string lane, string field);
    }

    /// <summary>Factory class for RegDef Parsers.</summary>
    public static class RegDefFactory
    {
        /// <summary>
        /// Gets the RegDef object based on the requested name.
        /// </summary>
        /// <param name="name">Name of the RegDef parser to get.</param>
        /// <param name="utils">SIOEDC_Util object to pass to the RegDef Parser.</param>
        /// <returns>A new RegDef Parser.</returns>
        public static IRegDef GetParser(string name, SIOEDC_Util utils)
        {
            switch (name.ToUpper())
            {
                case "DP":
                    return new RegDef_DP(utils);
                default:
                    return null;
            }
        }
    }
}
