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
    using System;

    /// <summary>
    /// Class for handling different inputs.
    /// </summary>
    public class InputFactory
    {
        /// <summary>
        /// Acceptable file types for this TC.
        /// </summary>
        public enum FileType
        {
            /// <summary>
            /// JSON FileType
            /// </summary>
            JSON,

            /// <summary>
            /// XML FileType
            /// </summary>
            XML,
        }

        /// <summary>
        /// Creates an object that implements the IInputFile interface.
        /// </summary>
        /// <param name="inputPath"> Path to the input file we're validating. </param>
        /// <returns> An object that implements <see cref="IInputFile"/>. </returns>
        public static IInputFile CreateConfigHandler(string inputPath) // FIXME: Duplicate methods in this class. Determine which one to keep...
        {
            IInputFile inputObject = null;

            string inputText = SharedFunctions.RetrieveTextFromFile(inputPath);

            if (inputPath.EndsWith(".xml"))
            {
                inputObject = new XMLInput(inputText);
            }
            else if (inputPath.EndsWith(".json"))
            {
                inputObject = new JsonInput(inputText);
            }
            else
            {
                throw new ArgumentException($"File at \"{inputPath}\" does not contain recognizable file type. Accepted inputs are JSON and XML, make sure the file ends with one of these types");
            }

            return inputObject;
        }

        /// <summary>
        /// Validate given input by using an object that implements IInputFile.
        /// </summary>
        /// <param name="text"> Text to be added in config handler. </param>
        /// <param name="type"> Type of text file used for config handler. </param>
        /// <returns> An object that implements <see cref="IInputFile"/>. </returns>
        public static IInputFile CreateConfigHandler(string text, FileType type)
        {
            switch (type)
            {
                case FileType.JSON:
                    return new JsonInput(text);
                case FileType.XML:
                    return new XMLInput(text);
                default:
                    throw new Prime.Base.Exceptions.FatalException("Invalid state when creating a handler for file.");
            }
        }
    }
}
