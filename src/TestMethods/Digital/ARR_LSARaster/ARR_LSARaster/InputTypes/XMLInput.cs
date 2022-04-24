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
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Class for XMLInput.
    /// </summary>
    public class XMLInput : IInputFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XMLInput"/> class.
        /// </summary>
        /// <param name="fileText"> Text for this input handler. </param>
        public XMLInput(string fileText)
        {
            this.FileText = fileText;
        }

        /// <inheritdoc/>
        public string FileText { get; set; }

        /// <inheritdoc/>
        public T DeserializeInput<T>()
        {
            var readerSettings = new XmlReaderSettings() { IgnoreComments = true };

            using (var inputFile = XmlReader.Create(new StringReader(this.FileText), readerSettings))
            {
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(T));
                return (T)x.Deserialize(inputFile);
            }
        }

        /// <inheritdoc/>
        /// <remarks> This does not require the use of schema, only checks if the XML is well formed. This is going to be supplemented by a deserialized validation anyways. </remarks>
        /// <remarks> User XML files for HryTableConfigXML are automatically generated, assume that they're valid. </remarks>
        public bool Validate(string schemaText)
        {
            try
            {
                new XmlDocument().LoadXml(this.FileText);
                return true;
            }
            catch (XmlException ex)
            {
                Prime.Services.ConsoleService.PrintError("Error when parsing HryTable XML");
                Prime.Services.ConsoleService.PrintError(ex.Message);
                return false;
            }
        }
    }
}