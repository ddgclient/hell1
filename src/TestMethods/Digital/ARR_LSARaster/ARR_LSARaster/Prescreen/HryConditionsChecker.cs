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
    using System.IO;
    using System.Text;

    /// <summary>
    /// Class that parses conditions found within the HryTableConfig classes.
    /// </summary>
    public class HryConditionsChecker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HryConditionsChecker"/> class.
        /// </summary>
        /// <param name="ctvData"> CtvData to use when parsing data for pin and checking for conditions. </param>
        /// <param name="conditions">String representing the conditions to use when parsing pin data for information. </param>
        public HryConditionsChecker(Dictionary<string, string> ctvData, string conditions)
        {
            this.Conditions = conditions;
            this.CtvData = ctvData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HryConditionsChecker"/> class.
        /// </summary>
        /// <param name="criterias"> Criterias after deserializing HryMap. </param>
        public HryConditionsChecker(List<HryTableConfigXml.CriteriaObject> criterias)
        {
            this.Criterias = criterias;
        }

        /// <summary>
        /// Gets or sets ctvData to use when checking conditions or pin data.
        /// </summary>
        public Dictionary<string, string> CtvData { get; set; }

        /// <summary>
        /// Gets or sets ctvData to use when checking conditions or pin data.
        /// </summary>
        public List<HryTableConfigXml.CriteriaObject> Criterias { get; set; }

        /// <summary>
        /// Gets or sets string representing the condition to use when querying ctvData.
        /// </summary>
        public string Conditions { get; set; }

        /// <summary>ra
        /// Checks if condition has failed for this instance of PinDataParser.
        /// </summary>
        /// <returns> Bool representing if pin data has failed the condition. </returns>
        public bool CheckIfConditionPassed()
        {
            string[] deserializedConditions = this.Conditions.Split('|');
            foreach (var condition in deserializedConditions)
            {
                string[] conditionElements = condition.Split(',');

                // Condition must be written as (Pin, Range to check, Criteria)
                if (conditionElements.Length != 3)
                {
                    throw new ArgumentException();
                }

                string conditionData = string.Empty;

                try
                {
                    conditionData = this.CtvData[conditionElements[0]];
                }
                catch (KeyNotFoundException ex)
                {
                    Prime.Services.ConsoleService.PrintError($"Could not find pin {conditionElements[0]} in ctvData.");
                    throw ex;
                }

                var indexesToCheck = SharedFunctions.ParseRange(conditionElements[1]);
                StringBuilder sb = new StringBuilder();

                foreach (int index in indexesToCheck)
                {
                    try
                    {
                        sb.Append(conditionData[index]);
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Prime.Services.ConsoleService.PrintError($"pin {conditionElements[0]} is trying to go out of bounds (is it empty?)");
                        throw ex;
                    }
                }

                string dataToCheck = sb.ToString();

                // Length of range we're checking and criteria must be equal
                if (dataToCheck.Length != conditionElements[2].Length)
                {
                    throw new InvalidDataException();
                }

                if (!dataToCheck.Equals(conditionElements[2]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns all the pins that CTV's need to be captured from. Parses "Pin" and "Condition" attributes or HryMap.Json.
        /// </summary>
        /// <returns> List of strings representing list of pins that CTV's need to be captured. </returns>
        public HashSet<string> GetListofPinsToMonitor()
        {
            var pinlist = new HashSet<string>();

            foreach (var criteria in this.Criterias)
            {
                pinlist.Add(criteria.Pin);
                string[] splitPipe = criteria.Condition.Split('|');
                foreach (var splitPipeVariable in splitPipe)
                {
                    pinlist.Add(splitPipeVariable.Split(',')[0]);
                }
            }

            return pinlist;
        }
    }
}
