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

namespace LSARasterTC
{
    /// <summary>
    /// Interface that allows different file formats to provide input for LSARaster.
    /// </summary>
    public interface IInputFile
    {
        /// <summary>
        /// Gets or sets string containing file text.
        /// </summary>
        string FileText { get; set; }

        /// <summary>
        /// Generic method for deserializing passed in input of type T.
        /// </summary>
        /// <typeparam name="T"> Type of which to deserialize the input file into. </typeparam>
        /// <returns> An object of type T.</returns>
        T DeserializeInput<T>();

        /// <summary>
        /// Initial validation of file; prevents passing in of malformed inputs.
        /// </summary>
        /// <param name="schemaFile"> String containing schema for file validation. </param>
        /// <returns> Boolean value representing if passed file is valid. </returns>
        bool Validate(string schemaFile);
    }
}
