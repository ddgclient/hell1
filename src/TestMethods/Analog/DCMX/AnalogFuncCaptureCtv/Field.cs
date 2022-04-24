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

namespace AnalogFuncCaptureCtv
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text.RegularExpressions;
    using NCalc;

    /// <summary>
    /// This class contains the basic structure of a Field.
    /// </summary>
    internal class Field
    {
        /// <summary>
        /// Unset condition for the parameters.
        /// </summary>
        private readonly string[] unsetCondition = { "-", string.Empty, null };

        /// <summary>
        /// Propierties of field to apply string typed operations.
        /// </summary>
        private readonly string[] stringTypedParameters = { "ItuffToken", "PinFinderFormat", "SharedStorageToken" };

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// Constructor for the Field class.
        /// </summary>
        /// <param name="name">The field's name.</param>
        /// <param name="fieldParams">The parameters for each field .</param>
        /// <param name="rungs">The hierarchy levels of the field.</param>
        /// <param name="rungDictionary">The hierarchy levels in dictionary format.</param>
        public Field(string name, Dictionary<string, string> fieldParams, List<string> rungs, Dictionary<string, string> rungDictionary)
        {
            this.Name = name;
            this.Params = fieldParams;
            this.FailPort = 0;
            this.Path = string.Join(".", rungs);
            this.Hierarchy = rungDictionary;
            Type fieldType = this.GetType();

            // Parameters
            foreach (KeyValuePair<string, string> param in fieldParams)
            {
                // Get the property.
                var property = fieldType.GetProperty(param.Key);

                // Convert the value to the property type.
                var convertedValue = Convert.ChangeType(param.Value, property.PropertyType);
                property.SetValue(this, convertedValue);
            }

            // Overwrite the String Typed Parameters with information of <var> from CSV hierarchy information.
            foreach (string propertyName in this.stringTypedParameters)
            {
                // Gets the property.
                var property = fieldType.GetProperty(propertyName);
                var parameterObject = property.GetValue(this, null);

                if (parameterObject != null)
                {
                    // Parse the parameter value to string.
                    var parameterValue = parameterObject.ToString();

                    if (this.CheckSettedParam(parameterValue))
                    {
                        // <var> Regex
                        string pattern = @"<\w+?>";

                        foreach (Match match in Regex.Matches(parameterValue, pattern))
                        {
                            // Removes the <> brackets.
                            string hierarchyReplaceKey = match.Value.Substring(1, match.Value.Length - 2);

                            // Checks if defined in hierarchy.
                            if (this.Hierarchy.ContainsKey(hierarchyReplaceKey))
                            {
                                string rungValue = this.Hierarchy[hierarchyReplaceKey];

                                // Replace value in property.
                                parameterValue = parameterValue.Replace(match.Value, rungValue);
                                property.SetValue(this, parameterValue);
                            }
                            else
                            {
                                Utils.PrintError($"[ERROR] The Key \"{hierarchyReplaceKey}\" was not found in Field Hierarchy. (CSV column headers).");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the Name of a Field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Path of a Field.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the Path of a Field.
        /// </summary>
        public Dictionary<string, string> Hierarchy { get; set; }

        /// <summary>
        /// Gets or sets the Expected Value of a Field.
        /// </summary>
        public string ExpectedData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the String Data of a Field.
        /// </summary>
        public string FieldStrData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the decimanl Data of a Field.
        /// </summary>
        public int FieldData { get; set; }

        /// <summary>
        /// Gets or sets the Size of a Field.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the FailPort at which the test will exit the instance.
        /// </summary>
        public ushort FailPort { get; set; }

        /// <summary>
        /// Gets or sets the ITUFF Token number of a field.
        /// </summary>
        public string ItuffToken { get; set; }

        /// <summary>
        /// Gets or sets the PinFinderFormat string.
        /// </summary>
        public string PinFinderFormat { get; set; }

        /// <summary>
        /// Gets or sets the PerBit parameter of a Field.
        /// </summary>
        public string PerBit { get; set; }

        /// <summary>
        /// Gets or sets the LowLimit parameter of a Field.
        /// </summary>
        public string LowLimit { get; set; }

        /// <summary>
        /// Gets or sets the HighLimit parameter of a Field.
        /// </summary>
        public string HighLimit { get; set; }

        /// <summary>
        /// Gets or sets the Equation string.
        /// </summary>
        public string Equation { get; set; }

        /// <summary>
        /// Gets or sets the token for Shared Storage.
        /// </summary>
        public string SharedStorageToken { get; set; }

        /// <summary>
        /// Gets or sets the codification type for the field.
        /// </summary>
        public string Codif { get; set; }

        /// <summary>
        /// Gets or sets the base for the field data.
        /// </summary>
        public string Base { get; set; }

        /// <summary>
        /// Gets or sets the list of PerBit fail cases.
        /// </summary>
        public List<int> PerBitFailures { get; set; }

        /// <summary>
        /// Gets or sets the parameters of a Field.
        /// </summary>
        public Dictionary<string, string> Params { get; set; }

        /// <summary>
        /// Function to SetFieldData to apply the data into the Field based on CTV data.
        /// </summary>
        /// <param name="input_string">The string chopped from the CTV data.</param>
        public void SetFieldData(string input_string)
        {
            if (input_string != string.Empty)
            {
                if (this.PerBit == "1")
                {
                    if (this.CheckSettedParam(this.Codif))
                    {
                        Utils.PrintError($"The field {this.Path} contains PerBit Flag and Codification at the same time");
                    }
                    else
                    {
                        this.FieldStrData = input_string;
                    }
                }
                else
                {
                    string fieldBinaryData = Utils.Reverse(input_string);
                    if (this.CheckSettedParam(this.Codif))
                    {
                        // Checks the type of codification and returns the value
                        this.FieldData = this.CheckCodif(fieldBinaryData);
                    }
                    else
                    {
                        this.FieldData = Convert.ToInt32(fieldBinaryData, 2);
                    }

                    // this.FieldStrData = this.FieldData.ToString();
                    this.FieldStrData = this.CheckBase();
                }
            }
        }

        /// <summary>
        /// Function to store the field data to the Shared Storage.
        /// </summary>
        public void SaveToSharedStorage()
        {
            if (this.CheckSettedParam(this.SharedStorageToken))
            {
                string token = this.SharedStorageToken + "_" + this.Path.Replace(".", "_");
                Utils.PrintDebug($"[INFO] Storing {nameof(this.FieldData)} [{this.FieldData}] with key: {token}");
                SharedStorage.SharedStorageSetValue(token, this.FieldData);
            }
        }

        /// <summary>
        /// Compares the FieldData to expected value or limits.
        /// </summary>
        /// <returns> Returns the Field Data in the requested base. </returns>
        public ushort CompareFieldData()
        {
            // Sets boolean cases for setted values.
            var settedExpectedData = this.CheckSettedParam(this.ExpectedData);
            var settedLowLimit = this.CheckSettedParam(this.LowLimit);
            var settedHighLimit = this.CheckSettedParam(this.HighLimit);
            var failInfo = Convert.ToUInt16(1);

            // Utils.PrintDebug(string.Format("[INFO] Setted values: {0} {1} {2}", settedExpectedData, settedLowLimit, settedHighLimit));

            // Compares only if exists expectedValue or Limits.
            // if (!settedExpectedData && !settedLowLimit && !settedHighLimit)
            // {
            //     failInfo = 1;
            // }
            if (this.PerBit == "1")
            {
                this.PerBitFailures = new List<int>(this.FieldStrData.Length);
                for (var i = 0; i < this.FieldStrData.Length; i++)
                {
                    if (settedExpectedData && this.ExpectedData != this.FieldStrData[i].ToString())
                    {
                        // Stores the index of failing bit.
                        Utils.PrintDebugError($"[ERROR] Failing bit at {this.Path}: {i}");
                        this.PerBitFailures.Add(i);
                        if (this.CheckSettedParam(this.PinFinderFormat))
                        {
                            this.PrintPinfinderToItuff(i);
                        }
                    }
                }
            }
            else
            {
                if (settedExpectedData && (settedLowLimit || settedHighLimit))
                {
                    Utils.PrintError($"[ERROR] Field contains expected value and limits at the same time, Field: {this.Path}");

                    // this.FailPort = 0;
                    failInfo = 0;
                }

                // Compare with ExpectedData.
                if (settedExpectedData && this.ExpectedData != this.FieldStrData)
                {
                    var error_msg = $"[ERROR] The tested value is different to the expected value at {this.Path}\n" +
                                       $"[ERROR] The tested value is: {this.FieldStrData} and the expected value is: {this.ExpectedData}";
                    Utils.PrintDebugError(error_msg);

                    // this.FailPort = 0;
                    failInfo = this.FailPort;

                    // Utils.PrintDebug(string.Format("[INFO] The tested value corresponds to the expected value at {0}", this.Path));
                }

                // Compare with limits.
                // if (!settedLowLimit && !settedHighLimit)
                // {
                //     failInfo = 1;
                // }
                if (settedLowLimit && this.FieldData < int.Parse(this.LowLimit))
                {
                    Utils.PrintDebugError($"[ERROR] Value is lower than Low Limit. Value: {this.FieldStrData}, Limit: {this.LowLimit}, at {this.Path}");

                    // this.FailPort = 0;
                    failInfo = this.FailPort;
                }

                if (settedHighLimit && this.FieldData > int.Parse(this.HighLimit))
                {
                    Utils.PrintDebugError($"[ERROR] Value is higher than High Limit. Value: {this.FieldStrData}, Limit: {this.HighLimit}, at {this.Path}");

                    // this.FailPort = 0;
                    failInfo = this.FailPort;
                }
            }

            return failInfo;
        }

        /// <summary>
        /// Prints the PerBitFailures to Ituff.
        /// </summary>
        /// <param name="bitIndex">Bit to be printed in PinFinderFormat.</param>
        public void PrintPinfinderToItuff(int bitIndex)
        {
            // Sets a list of strings to store the updated strings
            var pinfinderStrings = new List<string>
            {
                this.PinFinderFormat,
            };

            // Regex pattern that contains the format {x|y} and {x,y}.
            string pattern = @"{[,|\w]+?}";
            int match_count = Regex.Matches(this.PinFinderFormat, pattern).Count;

            // For the amount of matches to be resolved from {} to info.
            for (int i = 0; i < match_count; i++)
            {
                var new_strings = new List<string>();
                foreach (string pinfinderString in pinfinderStrings)
                {
                    Match match = Regex.Match(pinfinderString, pattern);

                    // Splits the '|' case
                    string[] values = match.Value.Substring(1, match.Value.Length - 2).Split('|');
                    foreach (string value in values)
                    {
                        // Create a new string for each iteration.
                        new_strings.Add(pinfinderString.Replace(match.Value, value));
                    }
                }

                // Update the pinfinderStrings if needed to be rerun for another resolution for {}.
                pinfinderStrings = new_strings;
            }

            // BitIndex printToItuff for each string.
            string bit_pattern = @"(?i)\$[bit+\-\d]+?\$";
            foreach (string pinfinderString in pinfinderStrings)
            {
                Match bit_match = Regex.Match(pinfinderString, bit_pattern);
                if (bit_match.Success)
                {
                    // Changes the $bit$ for the bitIndex
                    string bit_string = bit_match.Value.Substring(1, bit_match.Value.Length - 2); // Removes $ $
                    bit_string = bit_string.Replace("bit", bitIndex.ToString());
                    var expression = new Expression(bit_string);
                    int bit = (int)expression.Evaluate();

                    // Sets the netname
                    string netname = "|" + pinfinderString.Replace(bit_match.Value, bit.ToString()) + "_failbump";

                    // Print to Ituff
                    var ituffWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();
                    ituffWriter.SetTnamePostfix(netname);
                    ituffWriter.SetData("1");
                    Prime.Services.DatalogService.WriteToItuff(ituffWriter);
                }
            }
        }

        /// <summary>
        /// Function that checks the base for the Field Data.
        /// </summary>
        /// <returns> Returns the Field Data in the requested base. </returns>
        public string CheckBase()
        {
            if (this.CheckSettedParam(this.Base))
            {
                switch (this.Base)
                {
                    case "2":
                        {
                            // dec to bin
                            return Convert.ToString(this.FieldData, 2).PadLeft(this.Size, '0');
                        }

                    case "8":
                        {
                            // dec to bin
                            return Convert.ToString(this.FieldData, 8).PadLeft((this.Size / 3) + 1, '0');
                        }

                    case "10":
                        {
                            // dec to dec
                            return this.FieldData.ToString();
                        }

                    case "16":
                        {
                            // dec to hex
                            return this.FieldData.ToString("X").PadLeft((this.Size / 4) + 1, '0');
                        }

                    default:
                        {
                            Utils.PrintError($"The base used for field {this.Path} is not supported.");
                            return null;
                        }
                }
            }
            else
            {
                return this.FieldData.ToString();
            }
        }

        /// <summary>
        /// Function that checks the type of codification for the field.
        /// </summary>
        /// <param name="inputString"> The data to be converted. </param>
        /// <returns> Returns the Field Data in the requested codification. </returns>
        public int CheckCodif(string inputString)
        {
            switch (this.Codif)
            {
                case "1Comp":
                    {
                        return Utils.OnesComplement(inputString);
                    }

                case "2Comp":
                    {
                        return Utils.TwosComplement(inputString);
                    }

                case "Gray":
                    {
                        return Utils.GrayToDec(inputString);
                    }

                default:
                    {
                        Utils.PrintError($"The type of codification used for field {this.Path} is not supported.");
                        return 0;
                    }
            }
        }

        /// <summary>
        /// Checks if the Parameter Value is setted.
        /// </summary>
        /// <param name="parameter">The string of the parameter to check if setted.</param>
        /// <returns> Returns the boolean if it's contained.</returns>
        public bool CheckSettedParam(string parameter)
        {
            return !this.unsetCondition.Contains(parameter);
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            string paramsInfo = $"Field {nameof(this.Name)}: {this.Name}; {nameof(this.Path)}: {this.Path}; ";

            if (this.CheckSettedParam(this.FieldStrData))
            {
                paramsInfo += $"{nameof(this.FieldData)}: {this.FieldData}; {nameof(this.FieldStrData)}: {this.FieldStrData}; ";
            }

            // Parameters
            foreach (KeyValuePair<string, string> param in this.Params)
            {
                // Get the property
                var property = this.GetType().GetProperty(param.Key);
                string paramValue = property.GetValue(this, null).ToString();
                paramsInfo += $"{param.Key}: {paramValue}; ";
            }

            // Add failport
            paramsInfo += $"{nameof(this.FailPort)}: {this.FailPort}.";

            return paramsInfo;
        }
    }
}
