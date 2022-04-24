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
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Class representing XML property.
    /// </summary>
    [XmlRoot("HSR_HRY_config")]
    public class HryTableConfigXml
    {
        /// <summary> Gets or sets a value indicating whether to reverse Ctv capture data. </summary>
        [XmlElement("ReverseCtvCaptureData")]
        public string ReverseCtvCaptureData { get; set; }

        /// <summary> Gets or sets XML property. </summary>
        [XmlElement("CtvToHryMapping")]
        public CtvToHryMappingContainer CtvToHryMapping { get; set; }

        /// <summary> Gets or sets XML property. </summary>
        [XmlElement("Criterias")]
        public CriteriasContainer Criterias { get; set; }

        /// <summary> Gets or sets XML property. </summary>
        [XmlElement("Algorithms")]
        public AlgorithmsContainer Algorithms { get; set; }

        /// <summary>
        /// Make sure the deserialized config has all necessary elements for ctv decoding.
        /// </summary>
        /// <returns> A value indicating whether this config is valid. </returns>
        public bool ValidateConfig() // Isn't used during verify, need to stick it back in to make sure it gets validated.
        {
            bool isNull = true;

            isNull = SharedFunctions.IsPropertyNull(this.ReverseCtvCaptureData, nameof(this.ReverseCtvCaptureData)) |
                SharedFunctions.IsPropertyNull(this.Criterias, nameof(this.Criterias)) |
                SharedFunctions.IsPropertyNull(this.Algorithms, nameof(this.Algorithms));

            if (isNull)
            {
                return false;
            }

            if (!(this.ReverseCtvCaptureData == "Y" || this.ReverseCtvCaptureData == "N"))
            {
                Prime.Services.ConsoleService.PrintError("ReverseCtvCaptureData field for Hry XML configuration has an invalid value. Must be either \"Y\" or \"N\" ");
                return false;
            }

            bool elementIsNull = false;

            // FIXME: Needs more debug messages to make sure PDEs understand if the configuration is wrong...
            foreach (var criteria in this.Criterias.Criteria)
            {
                isNull = SharedFunctions.IsPropertyNull(criteria.Bypass_Global, nameof(criteria.Bypass_Global)) |
                    SharedFunctions.IsPropertyNull(criteria.Condition, nameof(criteria.Bypass_Global)) |
                    SharedFunctions.IsPropertyNull(criteria.Ctv_Index_Range, nameof(criteria.Ctv_Index_Range)) |
                    SharedFunctions.IsPropertyNull(criteria.Hry_Index, nameof(criteria.Hry_Index)) |
                    SharedFunctions.IsPropertyNull(criteria.Hry_Output_On_Condition_Fail, nameof(criteria.Hry_Output_On_Condition_Fail)) |
                    SharedFunctions.IsPropertyNull(criteria.Pin, nameof(criteria.Pin));

                if (isNull)
                {
                    elementIsNull = isNull;
                }
            }

            if (elementIsNull)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns this HryTableConfigXml list of map objects.
        /// </summary>
        /// <returns> List of <see cref="CriteriaObject"/>. </returns>
        public List<CriteriaObject> GetCriterias()
        {
            try
            {
                return this.Criterias.Criteria;
            }
            catch (NullReferenceException ex)
            {
                Prime.Services.ConsoleService.PrintError("Attempted access to this HryTableConfig Criteria, but it does not exist.");
                throw ex;
            }
            catch
            {
                Prime.Services.ConsoleService.PrintError("Unknown error occured when attempting to access this HryTableConfig Criteria.");
                throw;
            }
        }

        /// <summary>
        /// Returns this HryTableConfigXml list of criteria objects.
        /// </summary>
        /// <returns> List of <see cref="MapObject"/>. </returns>
        public List<MapObject> GetHryCharMapping()
        {
            try
            {
                return this.CtvToHryMapping.Map;
            }
            catch (NullReferenceException ex)
            {
                Prime.Services.ConsoleService.PrintError("Attempted access to this HryTableConfig CtvCharMapping, but it does not exist.");
                throw ex;
            }
            catch
            {
                Prime.Services.ConsoleService.PrintError("Unknown error occured when attempting to access this HryTableConfig CtvCharMapping.");
                throw;
            }
        }

        /// <summary>
        /// Method for returning whether configuration states to reverse capture data.
        /// </summary>
        /// <returns> A boolean value. </returns>
        public bool GetReverseCaptureData()
        {
            if (this.ReverseCtvCaptureData == "Y")
            {
                return true;
            }
            else if (this.ReverseCtvCaptureData == "N")
            {
                return false;
            }
            else
            {
                throw new Prime.Base.Exceptions.TestMethodException("ReverseCaptureData is set to an invalid state, this error was not captured during init.");
            }
        }

        /// <summary> Gets or sets XML property. </summary>
        public class CtvToHryMappingContainer
        {
            /// <summary>
            /// Gets or sets XML property.
            /// </summary>
            [XmlElement("Map")]
            public List<MapObject> Map { get; set; }
        }

        /// <summary>
        /// Class to represent XML element.
        /// </summary>
        public class MapObject
        {
            /// <summary> Gets or sets XML property. </summary>
            [XmlAttribute("ctv_data")]
            public string Ctv_Data { get; set; }

            /// <summary> Gets or sets XML property. </summary>
            [XmlAttribute("hry_data")]
            public string Hry_Data { get; set; }
        }

        /// <summary> Gets or sets XML property. </summary>>
        public class CriteriasContainer
        {
            /// <summary>
            /// Gets or sets XML property.
            /// </summary>
            [XmlElement("Criteria")]
            public List<CriteriaObject> Criteria { get; set; }
        }

        /// <summary>
        /// Class to represent XML element.
        /// </summary>s
        public class CriteriaObject
        {
            /// <summary> Gets or sets XML property. </summary>
            [XmlAttribute("hry_index")]
            public string Hry_Index { get; set; }

            /// <summary> Gets or sets XML property. </summary>
            [XmlAttribute("pin")]
            public string Pin { get; set; }

            /// <summary> Gets or sets XML property. </summary>
            [XmlAttribute("ctv_index_range")]
            public string Ctv_Index_Range { get; set; }

            /// <summary> Gets or sets XML property. </summary>
            [XmlAttribute("condition")]
            public string Condition { get; set; }

            /// <summary> Gets or sets XML property. </summary>
            [XmlAttribute("hry_output_on_condition_fail")]
            public string Hry_Output_On_Condition_Fail { get; set; }

            /// <summary> Gets or sets XML property. </summary>
            [XmlAttribute("bypass_global")]
            public string Bypass_Global { get; set; }
        }

        /// <summary> Gets or sets XML property. </summary>
        public class AlgorithmsContainer
        {
            /// <summary> Gets or sets XML property. </summary>
            [XmlElement("Algorithm")]
            public AlgorithmObject Algorithm { get; set; }

            /// <summary>
            /// Method for retrieving ctvData size.
            /// </summary>
            /// <returns> This Algorithm container's ctvData size. </returns>
            public int GetCtvDataSize()
            {
                return this.Algorithm.Ctv_Size;
            }
        }

        /// <summary>
        /// Class to represent XML element.
        /// </summary>s
        public class AlgorithmObject
        {
            /// <summary> Gets or sets XML property. </summary>
            [XmlAttribute("index")]
            public string Index { get; set; }

            /// <summary> Gets or sets XML property. </summary>
            [XmlAttribute("name")]
            public string Name { get; set; }

            /// <summary> Gets or sets XML property. </summary>
            [XmlAttribute("pat_modify_label")]
            public string Pat_Modify_Label { get; set; }

            /// <summary> Gets or sets XML property. </summary>
            [XmlAttribute("ctv_size")]
            public int Ctv_Size { get; set; }
        }
    }
}
