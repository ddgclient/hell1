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

namespace CtvServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using NCalc;

    /// <summary>
    /// This class contains the basic structure of a Field.
    /// </summary>
    internal class Field
    {
        // UnSet condition for the parameters.
        private readonly string[] unSetCondition = { "-", string.Empty, null };

        // Regex pattern for the data types.
        private readonly string pattern = @"<\w+?>";

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// Constructor for the Field class.
        /// </summary>
        /// <param name="name">The field's name.</param>
        /// <param name="configFileParameters">The parameters for each field .</param>
        /// <param name="rungs">The hierarchy levels of the field.</param>
        /// <param name="rungDictionary">The hierarchy levels in dictionary format.</param>
        public Field(string name, Dictionary<string, string> configFileParameters, List<string> rungs, Dictionary<string, string> rungDictionary)
        {
            this.Name = name;
            this.ConfigFileParameters = configFileParameters;
            this.Path = string.Join(".", rungs);
            this.Hierarchy = rungDictionary;
            Type fieldType = this.GetType();

            // Parameters
            foreach (KeyValuePair<string, string> parameter in this.ConfigFileParameters)
            {
                // Get the property.
                var property = fieldType.GetProperty(parameter.Key);

                if (property == null)
                {
                    Utils.PrintError($"[ERROR] Parameter [{parameter.Key}] is not a valid column in CSV file.");
                }

                // Convert the value to the property type.
                var parameterValue = parameter.Value;
                var convertedValue = Convert.ChangeType(parameterValue, property.PropertyType);
                property.SetValue(this, convertedValue);

                // Overwrite the String Typed Parameters with information of <var> from CSV hierarchy information.
                if (property.PropertyType == typeof(string))
                {
                    if (this.CheckSetParameter(parameterValue) && Regex.IsMatch(parameterValue, this.pattern))
                    {
                        foreach (Match match in Regex.Matches(parameterValue, this.pattern))
                        {
                            // Removes the <> brackets.
                            string hierarchyReplaceKey = match.Value.Substring(1, match.Value.Length - 2);

                            // Checks for <Path> keyword.
                            if (hierarchyReplaceKey == "Path")
                            {
                                // Replace value in property.
                                parameterValue = Utils.ReplaceFirst(parameterValue, match.Value, this.Path);
                                property.SetValue(this, parameterValue);
                            }

                            // Checks if defined in hierarchy.
                            else if (this.Hierarchy.ContainsKey(hierarchyReplaceKey))
                            {
                                string rungValue = this.Hierarchy[hierarchyReplaceKey];

                                // Replace value in property.
                                parameterValue = Utils.ReplaceFirst(parameterValue, match.Value, rungValue);
                                property.SetValue(this, parameterValue);
                            }
                            else
                            {
                                if (hierarchyReplaceKey == "TssidRename")
                                {
                                    Utils.PrintError($"[ERROR] [{hierarchyReplaceKey}] was not defined in the TestMethod parameters, but\nit is being called in the CSV file.");
                                }
                                else
                                {
                                    Utils.PrintError($"[ERROR] The Key [<{hierarchyReplaceKey}>] was not found in Field Hierarchy.\nCheck for an existing CSV column header.");
                                }
                            }
                        }
                    }
                }
            }

            // Checks the format conditions for the ItuffDescriptor.
            if (this.CheckSetParameter(this.ItuffDescriptor))
            {
                if (this.ItuffDescriptor.Length > 16)
                {
                    Utils.PrintError($"[ERROR] {this.ItuffDescriptor} max support size is 16. Current size: {this.ItuffDescriptor.Length}");
                }

                if (this.ItuffDescriptor.Contains("_"))
                {
                    Utils.PrintError($"[ERROR] {this.ItuffDescriptor} cannot contain character \"_\"");
                }
            }

            // Sets the exit port as short, if user defined.
            if (this.CheckSetParameter(this.ExitPort))
            {
                this.ExitPortShort = ushort.Parse(this.ExitPort);
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
        /// Gets or sets the Expected Value of a Field.
        /// </summary>
        public double ExpectedDataValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the String Data of a Field.
        /// </summary>
        public string StringData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the decimal Data of a Field.
        /// </summary>
        public double FieldData { get; set; }

        /// <summary>
        /// Gets or sets the Size of a Field.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the FailPort at which the test will exit the instance.
        /// </summary>
        public ushort FailPort { get; set; } = 1;

        /// <summary>
        /// Gets or sets the user defined ExitPort at which the test will exit the instance.
        /// </summary>
        public string ExitPort { get; set; }

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
        /// Gets or sets the LowLimit parameter of a Field.
        /// </summary>
        public double LowLimitValue { get; set; }

        /// <summary>
        /// Gets or sets the HighLimit parameter of a Field.
        /// </summary>
        public double HighLimitValue { get; set; }

        /// <summary>
        /// Gets or sets the Equation string.
        /// </summary>
        public string Equation { get; set; }

        /// <summary>
        /// Gets or sets the token the storage service.
        /// </summary>
        public string StorageToken { get; set; }

        /// <summary>
        /// Gets or sets the codification type for the field.
        /// </summary>
        public string Codification { get; set; }

        /// <summary>
        /// Gets or sets the base for the field DataService.
        /// </summary>
        public string Base { get; set; }

        /// <summary>
        /// Gets or sets the ItuffDescriptor for the field DataService.
        /// </summary>
        public string ItuffDescriptor { get; set; }

        /// <summary>
        /// Gets or sets the Reverse for the field DataService.
        /// </summary>
        public string Reverse { get; set; }

        /// <summary>
        /// Gets or sets the Reverse for the field DataService.
        /// </summary>
        public string Append { get; set; }

        /// <summary>
        /// Gets or sets the list of PerBit fail cases.
        /// </summary>
        public List<int> PerBitFailures { get; set; }

        /// <summary>
        /// Gets or sets the parameters of a Field.
        /// </summary>
        public Dictionary<string, string> ConfigFileParameters { get; set; }

        /// <summary>
        /// Gets or sets as ushort the ExitPort  at which the test will exit the instance.
        /// </summary>
        private ushort ExitPortShort { get; set; } = 0;

        /// <summary>
        /// Function to SetFieldData to apply the data into the Field based on CTV DataService.
        /// </summary>
        /// <param name="input_string">The string chopped from the CTV DataService.</param>
        public void SetFieldData(string input_string)
        {
            if (input_string != string.Empty)
            {
                if (this.PerBit == "1")
                {
                    this.StringData = input_string;
                }
                else
                {
                    string fieldBinaryData = Utils.Reverse(input_string);

                    if (this.CheckSetParameter(this.Codification))
                    {
                        // Checks the type of codification and returns the value
                        this.FieldData = Convert.ToDouble(this.CheckCodification(fieldBinaryData));
                    }
                    else
                    {
                        this.FieldData = Convert.ToDouble(Convert.ToInt32(fieldBinaryData, 2));
                    }

                    // this.StringData = this.FieldDataService.ToString();
                    this.StringData = this.CheckParameterBase();
                }
            }
        }

        /// <summary>
        /// Function to store the field data to the Shared Storage.
        /// </summary>
        public void StoreFieldData()
        {
            if (this.CheckSetParameter(this.StorageToken))
            {
                if (this.Base != "-" && this.Base != "10")
                {
                    DataServices.SetData(this.StorageToken, this.StringData.Substring(2));
                }
                else
                {
                    DataServices.SetData(this.StorageToken, this.FieldData);
                }
            }
        }

        /// <summary>
        /// Compares the FieldData to expected value or limits.
        /// </summary>
        public void CompareFieldData()
        {
            // Sets boolean cases for Set values.
            var setExpectedData = this.CheckSetParameter(this.ExpectedData);
            var setLowLimit = this.CheckSetParameter(this.LowLimit);
            var setHighLimit = this.CheckSetParameter(this.HighLimit);

            // Utils.PrintDebug(string.Format("[INFO] Set values: {0} {1} {2}", SetExpectedData, SetLowLimit, SetHighLimit));

            // Compares only if exists expectedValue or Limits.
            if (!setExpectedData && !setLowLimit && !setHighLimit)
            {
                return;
            }

            if (this.PerBit == "1")
            {
                if (this.ExpectedData.Length > 1)
                {
                    Utils.PrintError($"[ERROR] {nameof(this.ExpectedData)} size should be [1] at {this.Path}, got [{this.ExpectedData.Length}] characters.");
                    this.FailPort = this.ExitPortShort;
                    return;
                }

                this.PerBitFailures = new List<int>(this.StringData.Length);
                for (var i = 0; i < this.StringData.Length; i++)
                {
                    if (setExpectedData && this.ExpectedData != this.StringData[i].ToString())
                    {
                        // Stores the index of failing bit.
                        Utils.PrintDebugError($"[ERROR] Failing bit at {this.Path}: {i}");
                        this.PerBitFailures.Add(i);
                        if (this.CheckSetParameter(this.PinFinderFormat))
                        {
                            this.PrintPinfinderToItuff(i);
                        }

                        this.FailPort = this.ExitPortShort;
                    }
                }
            }
            else
            {
                if (setExpectedData && setLowLimit && setHighLimit)
                {
                    Utils.PrintError($"[ERROR] Field contains expected value and limits at the same time, Field: {this.Path}");
                    this.FailPort = this.ExitPortShort;
                    return;
                }

                // Compare with ExpectedData.
                if (setExpectedData)
                {
                    double expectedDecimal = this.GetParameterDecimal("ExpectedData");

                    // double ExpectedDecimal = double.Parse(this.ExpectedData);
                    if (expectedDecimal != this.FieldData)
                    {
                        var error_msg = $"[ERROR] The tested value is different to the expected value at {this.Path}\n" + $"[ERROR] The tested value is: {this.StringData} and the expected value is: {this.ExpectedData}";
                        Utils.PrintDebugError(error_msg);
                        this.FailPort = this.ExitPortShort;
                        return;
                    }

                    // Utils.PrintDebug(string.Format("[INFO] The tested value corresponds to the expected value at {0}", this.Path));
                }

                // Compare with limits.
                if (!setLowLimit && !setHighLimit)
                {
                     return;
                }

                if (setLowLimit && this.FieldData < this.LowLimitValue)
                {
                    Utils.PrintDebugError($"[ERROR] Value is lower than Low Limit. Value: {this.StringData}, Limit: {this.LowLimit}, at {this.Path}");
                    this.FailPort = this.ExitPortShort;
                    return;
                }

                if (setHighLimit && this.FieldData > this.HighLimitValue)
                {
                    Utils.PrintDebugError($"[ERROR] Value is higher than High Limit. Value: {this.StringData}, Limit: {this.HighLimit}, at {this.Path}");
                    this.FailPort = this.ExitPortShort;
                    return;
                }
            }
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
            const string pattern = @"{[,|\w]+?}";
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
            const string bit_pattern = @"(?i)\$[bit+\-\d]+?\$";
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
        /// Function that checks the input csv data for violations during verify.
        /// </summary>
        public void CheckViolations()
        {
            if (this.PerBit == "1" && this.CheckSetParameter(this.Codification))
            {
                Utils.PrintError($"[ERROR] The field [{this.Path}] contains PerBit Flag and Codification at the same time");
            }

            if (this.CheckSetParameter(this.Base) && this.CheckSetParameter(this.StorageToken) && this.Base != "10" && this.Base != "-" && !(this.StorageToken.Substring(0, 2) == "S:" || this.StorageToken.Substring(0, 4) == "DFF:"))
            {
                Utils.PrintError($"[ERROR] The field [{this.Path}] contains a Base different to decimal and the Shared Storage Token is not type String nor DFF");
            }
        }

        /// <summary>
        /// Function that checks the base for a Field parameter.
        /// </summary>
        /// <param name="parameter">Parameter to change to decimal.</param>
        /// <returns> Returns the Field parameter data as a decimal number. </returns>
        public double GetParameterDecimal(string parameter = "StringData")
        {
            var property = this.GetType().GetProperty(parameter);
            string paramValue = Convert.ToString(property.GetValue(this, null));

            // Regex pattern that contains only numbers.
            const string pattern = @"^-?\d+$";
            if (Regex.IsMatch(paramValue, pattern))
            {
                return double.Parse(paramValue);
            }
            else
            {
                string basePrefix = paramValue.Substring(0, 2);
                paramValue = paramValue.Substring(2);
                switch (basePrefix)
                {
                    case "0b":
                        {
                            return (double)Convert.ToInt32(paramValue, 2);
                        }

                    case "0o":
                        {
                            return (double)Convert.ToInt32(paramValue, 8);
                        }

                    case "0x":
                        {
                            return (double)Convert.ToInt32(paramValue, 16);
                        }

                    default:
                        {
                            Utils.PrintError($"The base used for {parameter} is not supported.");
                            return 0;
                        }
                }
            }
        }

        /// <summary>
        /// Function that checks the base for the Field DataService.
        /// </summary>
        /// <param name="parameter">Parameter to change to Base format.</param>
        /// <returns> Returns the Field Data in the requested base. </returns>
        public string CheckParameterBase(string parameter = "FieldData")
        {
            var property = this.GetType().GetProperty(parameter);
            double paramValue = Convert.ToDouble(property.GetValue(this, null));

            if (this.CheckSetParameter(this.Base))
            {
                switch (this.Base)
                {
                    case "2":
                        {
                            // dec to bin
                            return "0b" + Convert.ToString(Convert.ToInt32(paramValue), 2).PadLeft(this.Size, '0');
                        }

                    case "8":
                        {
                            // dec to bin
                            return "0o" + Convert.ToString(Convert.ToInt32(paramValue), 8).PadLeft((int)Math.Ceiling(this.Size / 3.0), '0');
                        }

                    case "10":
                        {
                            // dec to dec
                            return string.Format("{0:0.####}", paramValue);
                        }

                    case "16":
                        {
                            // dec to hex
                            return "0x" + Convert.ToInt32(paramValue).ToString("X").PadLeft((int)Math.Ceiling(this.Size / 4.0), '0');
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
                return string.Format("{0:0.####}", paramValue);
            }
        }

        /// <summary>
        /// Function that checks the type of codification for the field.
        /// </summary>
        /// <param name="inputString"> The data to be converted. </param>
        /// <returns> Returns the Field Data in the requested codification. </returns>
        public int CheckCodification(string inputString)
        {
            const string OnesComplementToDecimal = "1Comp";
            const string TwosComplementToDecimal = "2Comp";
            const string GrayToDecimal = "Gray";
            const string ReverseToDecimal = "Reverse";

            switch (this.Codification)
            {
                case OnesComplementToDecimal:
                    {
                        return Utils.OnesComplementToDecimal(inputString);
                    }

                case TwosComplementToDecimal:
                    {
                        return Utils.TwosComplementToDecimal(inputString);
                    }

                case GrayToDecimal:
                    {
                        return Utils.GrayCodeToDecimal(inputString);
                    }

                case ReverseToDecimal:
                    {
                        return Convert.ToInt32(Utils.Reverse(inputString), 2);
                    }

                default:
                    {
                        Utils.PrintError($"The type of codification used for field [{this.Path}] is not supported.");
                        return 0;
                    }
            }
        }

        /// <summary>
        /// Checks if the there is codification prefix and return the str data.
        /// </summary>
        /// <returns> Returns the FieldData without base prefix.</returns>
        public string FieldDataRemoveRadixPrefix()
        {
            if (this.CheckSetParameter(this.Base) && this.Base != "10")
            {
                return this.StringData.Substring(2);
            }
            else
            {
                return this.StringData;
            }
        }

        /// <summary>
        /// Checks if the Parameter Value is set.
        /// </summary>
        /// <param name="parameter">The string of the parameter to check if set.</param>
        /// <returns> Returns the boolean if it's contained.</returns>
        public bool CheckSetParameter(string parameter)
        {
            return !this.unSetCondition.Contains(parameter);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string paramsInfo = $"Field {nameof(this.Name)}: {this.Name}; {nameof(this.Path)}: {this.Path}; ";

            if (this.CheckSetParameter(this.StringData))
            {
                paramsInfo += $"{nameof(this.FieldData)}: {this.FieldData}; {nameof(this.StringData)}: {this.StringData}; ";
            }

            // Parameters
            foreach (KeyValuePair<string, string> param in this.ConfigFileParameters)
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
