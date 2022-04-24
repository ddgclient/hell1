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

namespace LSARasterTC.UnitTest
{
    using System;
    using global::LSARasterTC;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;

    /// <summary> Dummy description of this test method's unit test.</summary>
    [TestClass]
    public class HryTableConfig_UnitTest
    {
        private string hryTableSchema;
        private string validHryTableInput;
        private string malformedInput;
        private string missingElementsInput;
        private string validHryTableInputXML;

        private Mock<IConsoleService> mockConsole = new Mock<IConsoleService>();

        /// <summary>
        /// Initialize for this test class.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            this.hryTableSchema = "{ \"$schema\":\"http://json-schema.org/draft-07/schema\", \"$id\":\"http://example.com/example.json\", \"type\":\"object\", \"title\":\"Therootschema\", \"description\":\"TherootschemacomprisestheentireJSONdocument.\", \"required\":[ \"HSR_HRY_config\" ], \"properties\":{ \"HSR_HRY_config\":{ \"$id\":\"#/properties/HSR_HRY_config\", \"type\":\"object\", \"title\":\"TheHSR_HRY_configschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\", \"required\":[ \"ReverseCtvCaptureData\", \"CtvToHryMapping\", \"Criterias\", \"Algorithms\" ], \"properties\":{ \"ReverseCtvCaptureData\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/ReverseCtvCaptureData\", \"type\":\"boolean\", \"title\":\"TheReverseCtvCaptureDataschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\", \"default\":false }, \"CtvToHryMapping\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/CtvToHryMapping\", \"type\":\"array\", \"title\":\"TheCtvToHryMappingschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\", \"additionalItems\":true, \"items\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/CtvToHryMapping/items\", \"anyOf\":[ { \"$id\":\"#/properties/HSR_HRY_config/properties/CtvToHryMapping/items/anyOf/0\", \"type\":\"object\", \"title\":\"ThefirstanyOfschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\", \"default\":{}, \"required\":[ \"Ctv_Data\", \"Hry_Data\" ], \"properties\":{ \"Ctv_Data\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/CtvToHryMapping/items/anyOf/0/properties/Ctv_Data\", \"type\":\"string\", \"title\":\"TheCtv_Dataschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\", \"default\":\"\" }, \"Hry_Data\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/CtvToHryMapping/items/anyOf/0/properties/Hry_Data\", \"type\":\"string\", \"title\":\"TheHry_Dataschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\", \"default\":\"\" } }, \"additionalProperties\":true } ] } }, \"Criterias\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Criterias\", \"type\":\"array\", \"title\":\"TheCriteriasschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\", \"additionalItems\":true, \"items\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Criterias/items\", \"anyOf\":[ { \"$id\":\"#/properties/HSR_HRY_config/properties/Criterias/items/anyOf/0\", \"type\":\"object\", \"title\":\"ThefirstanyOfschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\", \"required\":[ \"Hry_Index\", \"Pin\", \"Ctv_Index_Range\", \"Condition\", \"Hry_Output_On_Condition_Fail\", \"Bypass_Global\" ], \"properties\":{ \"Hry_Index\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Criterias/items/anyOf/0/properties/Hry_Index\", \"type\":\"string\", \"title\":\"TheHry_Indexschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\" }, \"Pin\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Criterias/items/anyOf/0/properties/Pin\", \"type\":\"string\", \"title\":\"ThePinschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\" }, \"Ctv_Index_Range\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Criterias/items/anyOf/0/properties/Ctv_Index_Range\", \"type\":\"string\", \"title\":\"TheCtv_Index_Rangeschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\" }, \"Condition\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Criterias/items/anyOf/0/properties/Condition\", \"type\":\"string\", \"title\":\"TheConditionschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\" }, \"Hry_Output_On_Condition_Fail\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Criterias/items/anyOf/0/properties/Hry_Output_On_Condition_Fail\", \"type\":\"string\", \"title\":\"TheHry_Output_On_Condition_Failschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\" }, \"Bypass_Global\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Criterias/items/anyOf/0/properties/Bypass_Global\", \"type\":\"string\", \"title\":\"TheBypass_Globalschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\" } }, \"additionalProperties\":true } ] } }, \"Algorithms\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Algorithms\", \"type\":\"array\", \"title\":\"TheAlgorithmsschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\", \"additionalItems\":true, \"items\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Algorithms/items\", \"anyOf\":[ { \"$id\":\"#/properties/HSR_HRY_config/properties/Algorithms/items/anyOf/0\", \"type\":\"object\", \"title\":\"ThefirstanyOfschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\", \"required\":[ \"Index\", \"Name\", \"Pat_Modify_Label\", \"Ctv_Size\" ], \"properties\":{ \"Index\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Algorithms/items/anyOf/0/properties/Index\", \"type\":\"string\", \"title\":\"TheIndexschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\", 										\"default\":\"\" }, \"Name\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Algorithms/items/anyOf/0/properties/Name\", \"type\":\"string\", \"title\":\"TheNameschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\", 										\"default\":\"\" }, \"Pat_Modify_Label\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Algorithms/items/anyOf/0/properties/Pat_Modify_Label\", \"type\":\"string\", \"title\":\"ThePat_Modify_Labelschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\", 										\"default\":\"\" }, \"Ctv_Size\":{ \"$id\":\"#/properties/HSR_HRY_config/properties/Algorithms/items/anyOf/0/properties/Ctv_Size\", \"type\":\"string\", \"title\":\"TheCtv_Sizeschema\", \"description\":\"Anexplanationaboutthepurposeofthisinstance.\",\"default\":\"\" } }, \"additionalProperties\":true } ] } } }, \"additionalProperties\":true } }, \"additionalProperties\":true }";
            this.validHryTableInput = "{ \"HSR_HRY_config\":{\"ReverseCtvCaptureData\":false,\"CtvToHryMapping\":[{\"Ctv_Data\":\"0\",\"Hry_Data\":\"0\"},{\"Ctv_Data\":\"1\",\"Hry_Data\":\"1\"}],\"Criterias\":[{\"Hry_Index\":\"0\",\"Pin\":\"P001\",\"Ctv_Index_Range\":\"2\",\"Condition\":\"P002,0-1,00|P002,3,1\",\"Hry_Output_On_Condition_Fail\":\"8\",\"Bypass_Global\":\"HSR.HRY_Global_1\"},{\"Hry_Index\":\"1\",\"Pin\":\"P001\",\"Ctv_Index_Range\":\"6\",\"Condition\":\"P002,4-5,00|P002,7,1\",\"Hry_Output_On_Condition_Fail\":\"8\",\"Bypass_Global\":\"HSR.HRY_Global_1\"}],\"Algorithms\":[{\"Index\":\"0\",\"Name\":\"SCAN\",\"Pat_Modify_Label\":\"\",\"Ctv_Size\":\"36\"},{\"Index\":\"1\",\"Name\":\"PMOVI\",\"Pat_Modify_Label\":\"\",\"Ctv_Size\":\"36\"},{\"Index\":\"2\",\"Name\":\"March-C\",\"Pat_Modify_Label\":\"\",\"Ctv_Size\":\"36\"}]}}";
            this.malformedInput = "{ \"$schema\":\"http://json-schema.org/draft-07/schema#\",\"$id\":\"http://example.com/product.schema.json\",\"title\":\"Product\",\"description\":\"AproductfromAcme'scatalog\",\"type\":\"object\",\"properties\":{\"productId\":{\"description\":\"Theuniqueidentifierforaproduct\",\"type\":\"integer\"}},\"required\":[\"productId\"]}";
            this.missingElementsInput = "{ \"HSR_HRY_config\": { \"ReverseCtvCaptureData\":false, \"CtvToHryMapping\":[ { \"Ctv_Data\":\"0\", \"Hry_Data\":\"0\" }, { \"Ctv_Data\":\"1\", \"Hry_Data\":\"1\" } ], \"Criterias\":[ { \"Hry_Index\":\"0\", \"Ctv_Index_Range\":\"2\", \"Condition\":\"P002,0-1,00|P002,3,1\", \"Hry_Output_On_Condition_Fail\":\"8\", \"Bypass_Global\":\"HSR.HRY_Global_1\" }, { \"Hry_Index\":\"1\", \"Pin\":\"P001\", \"Condition\":\"P002,4-5,00|P002,7,1\", \"Hry_Output_On_Condition_Fail\":\"8\", \"Bypass_Global\":\"HSR.HRY_Global_1\" } ], \"Algorithms\":[ { \"Index\":\"0\", \"Name\":\"SCAN\", \"Pat_Modify_Label\":\"\", \"Ctv_Size\":\"36\" }, { \"Index\":\"1\", \"Name\":\"PMOVI\", \"Pat_Modify_Label\":\"\", \"Ctv_Size\":\"36\" }, { \"Index\":\"2\", \"Name\":\"March-C\", \"Pat_Modify_Label\":\"\", \"Ctv_Size\":\"36\" } ] } }";
            this.validHryTableInputXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?> <HSR_HRY_config>  <ReverseCtvCaptureData>N</ReverseCtvCaptureData>  <CtvToHryMapping> 	<Map ctv_data=\"0\" hry_data=\"0\" /> 	<Map ctv_data=\"1\" hry_data=\"1\" /> </CtvToHryMapping>  <Criterias> 	<Criteria hry_index=\"0\"  pin=\"P001\" ctv_index_range=\"2\"  condition=\"P002,0-1,00|P002,3,1\"    hry_output_on_condition_fail=\"8\" bypass_global=\"HSR.HRY_Global_1\" /> 	<Criteria hry_index=\"1\"  pin=\"P001\" ctv_index_range=\"6\"  condition=\"P002,4-5,00|P002,7,1\"    hry_output_on_condition_fail=\"8\" bypass_global=\"HSR.HRY_Global_1\" /> </Criterias>  <Algorithms> 	<Algorithm index=\"0\" name=\"SCAN\"    pat_modify_label=\"\" ctv_size=\"36\" /> 	<Algorithm index=\"1\" name=\"PMOVI\"   pat_modify_label=\"\" ctv_size=\"36\" /> 	<Algorithm index=\"2\" name=\"March-C\" pat_modify_label=\"\" ctv_size=\"36\" /> </Algorithms>  </HSR_HRY_config>";

            this.mockConsole.Setup(x => x.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
    Callback<string, int, string, string>((string msg, int line, string n, string src) => { Console.WriteLine($"ERROR: {msg}"); });
            Prime.Services.ConsoleService = this.mockConsole.Object;
        }

        /// <summary>
        /// Check that file is invalidated when malformed.
        /// </summary>
        public void MalformedValidateInput()
        {
            var malformedJson = new JsonInput(this.malformedInput);

            Assert.IsFalse(malformedJson.Validate(this.hryTableSchema));
        }

        /// <summary>
        /// Check that file is invalidated when missing critical properties.
        /// </summary>
        public void MissingElementsValidateInput()
        {
            var missingElementsJson = new JsonInput(this.missingElementsInput);
            Assert.IsFalse(missingElementsJson.Validate(this.hryTableSchema));
        }

        /// <summary>
        /// Check that file is validated when all schema criteria is met.
        /// </summary>
        [TestMethod]
        public void SuccessValidateInput()
        {
            var validJson = new JsonInput(this.validHryTableInput);
            Assert.IsTrue(validJson.Validate(this.hryTableSchema));
        }

        /// <summary>
        /// Check that file is deserialized successfully when given as an XML.
        /// </summary>
        [TestMethod]
        public void DeserializeInput_XML()
        {
            var input = InputFactory.CreateConfigHandler(this.validHryTableInputXML, InputFactory.FileType.XML);
            var deserializedHryTable = input.DeserializeInput<HryTableConfigXml>();

            Assert.IsTrue(deserializedHryTable.ValidateConfig());
        }

        /// <summary>
        /// Check that deserializedconfig fails when a given element is null.
        /// </summary>
        [TestMethod]
        public void DeserializeInput_XML_ElementNull()
        {
            var input = InputFactory.CreateConfigHandler(this.validHryTableInputXML, InputFactory.FileType.XML);
            var deserializedHryTable = input.DeserializeInput<HryTableConfigXml>();
            deserializedHryTable.Criterias = null;

            Assert.IsFalse(deserializedHryTable.ValidateConfig());
        }
    }
}
