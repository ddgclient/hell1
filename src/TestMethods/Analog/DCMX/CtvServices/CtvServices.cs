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
    using global::CtvServices.ConfigurationFile;
    using Microsoft.CSharp.RuntimeBinder;
    using NCalc;
    using Prime;
    using Prime.Base.Exceptions;
    using Prime.TestMethods;

    /// <summary>
    /// This class contains the high level services used for the CtvDecoding with an input file.
    /// </summary>
    public class CtvServices
    {
        private const string ItuffFailingCasesPostFix = "fc";
        private readonly Regex hexRegex = new Regex(@"0x[\dA-F]+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex octalRegex = new Regex(@"0o[0-7]+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex binaryRegex = new Regex(@"0b[0-1]+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex offsetRegex = new Regex(@"^offset\d*\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex stringExpressionRegex = new Regex(@"(?![\d])[\w\.:]+(\()?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex mathRegex = new Regex(@"[\w.]+\(", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex sharedStorageTokenRegex = new Regex(@"(?!\d)\b(?:[*\^/+-])?[IDS]{1}\.\w+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex dataTypeRegex = new Regex(@"([IDS]{1}\:)([\w.]+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private string ctvString;
        private List<string> tssidRename;
        private Dictionary<string, int> pinStringSize;
        private bool exitPortUntouched = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtvServices"/> class.
        /// </summary>
        public CtvServices()
        {
            this.FileHandler = new FileHandler();
        }

        /// <summary>
        /// Gets or sets the IO operations file handler.
        /// </summary>
        public IFileHandler FileHandler { get; set; }

        /// <summary>
        /// Function to initialize the data dictionary in the Verify Method.
        /// </summary>
        /// <param name="configurationFile">The configuration file pointer.</param>
        /// <param name="ctvCapturePins">ctvCapturePins as TestMethodParams.CommaSeparatedString.</param>
        /// <param name="tssidRename">List of strings to rename all CtvCapturePins.</param>
        /// <param name="csvDelimiter">Delimiter for the input csv file.</param>
        /// <returns>Dictionary data structure to store all input file.</returns>
        public Dictionary<string, dynamic> CtvStructureInit(TestMethodsParams.File configurationFile, TestMethodsParams.CommaSeparatedString ctvCapturePins, TestMethodsParams.CommaSeparatedString tssidRename, string csvDelimiter = ",")
        {
            Dictionary<string, dynamic> dataStructure = new Dictionary<string, dynamic>();

            if (!string.IsNullOrEmpty(tssidRename.ToString()) && !ctvCapturePins.ToList().Count.Equals(tssidRename.ToList().Count))
            {
                throw new TestMethodException($"Parameter {nameof(tssidRename)} have different item count from {nameof(ctvCapturePins)}. Expected count: [{ctvCapturePins.ToList().Count}] {nameof(tssidRename)} count: [{tssidRename.ToList().Count}]");
            }

            this.tssidRename = tssidRename.ToList();

            string configFile = Services.FileService.GetFile(configurationFile);
            Utils.PrintDebug($"[INFO] This is the ConfigurationFile [{configurationFile}]");
            if (string.IsNullOrEmpty(configFile))
            {
                throw new TestMethodException($"[ERROR] ConfigurationFile [{configurationFile}] does not exist!");
            }

            // Initialization of this.dataStructure with the csv file from the configFileContent path.
            var configFileContent = this.FileHandler.ReadAllLines(configFile);
            if (configFileContent.Length <= 1)
            {
                Utils.PrintError($"[ERROR] ConfigurationFile [{configurationFile}] is empty!");
            }

            CtvServices ctvServices = new CtvServices();

            // List of headers in csvFile
            string[] header = configFileContent[0].Split(csvDelimiter[0]);

            // Initiates the pinStringSize dictionary
            this.pinStringSize = new Dictionary<string, int>();

            // Column indexes.
            int field_index = Array.IndexOf(header, "Field");
            int size_index = Array.IndexOf(header, "Size");
            int pin_index = Array.IndexOf(header, "Pin");

            // Iterates through rows from CSV file.
            for (int k = 1; k < configFileContent.Length; k++)
            {
                // Initialize empty values of structure variables.
                var fieldParams = new Dictionary<string, string>();
                var rungs = new List<string>();
                var rungDictionary = new Dictionary<string, string>();

                // Field data in row.
                string[] rowData = configFileContent[k].Split(csvDelimiter.ToString()[0]);

                // Fills size dictionary with pins and test sizes.
                string pin = rowData[pin_index];
                int size = int.Parse(rowData[size_index]);

                // Check that CtvCapture pin exists in the csv file
                if (!ctvCapturePins.ToList().Contains(pin))
                {
                    Utils.PrintError($"[ERROR] CSV Pin [{pin}] in row [{pin_index}] does not match with any pin in CtvCapturePins [{ctvCapturePins}]");
                }

                if (this.pinStringSize.ContainsKey(pin))
                {
                    this.pinStringSize[pin] += size;
                }
                else
                {
                    this.pinStringSize.Add(pin, size);
                }

                // Iterates through columns from CSV file.
                for (int i = 0; i < rowData.Length; i++)
                {
                    // Field parameters case.
                    if (i > field_index)
                    {
                        fieldParams.Add(header[i], rowData[i]);
                    }

                    // Add rung to dictionary hierarchy.
                    else
                    {
                        // Change the pin name to die name according to TssidRename.
                        if (i == pin_index)
                        {
                            string renamedPin = ctvServices.GetTssidRename(pin, ctvCapturePins, this.tssidRename);
                            rungs.Add(renamedPin);
                            rungDictionary.Add(header[i], pin);
                            rungDictionary.Add("TssidRename", renamedPin);
                        }
                        else
                        {
                            rungs.Add(rowData[i]);
                            rungDictionary.Add(header[i], rowData[i]);
                        }
                    }
                }

                // Field initialization.
                Field field = new Field(rowData[field_index], fieldParams, rungs, rungDictionary);

                // Check input file violations
                field.CheckViolations();

                // Merges and update the dataStructure with the row information.
                var tree_dict = DictionaryTree(rungs, field);
                dataStructure = DictionaryMerge(dataStructure, tree_dict, k + 1);
            }

            if (dataStructure.Count != ctvCapturePins.ToList().Count)
            {
                Utils.PrintError($"[ERROR] CtvCapturePins [{ctvCapturePins}] do not match CSV pins or TssidRename count [{string.Join(",", dataStructure.Keys)}]");
            }

            return dataStructure;
        }

        /// <summary>
        /// Function to iterate over nested dictionaries.
        /// </summary>
        /// <param name="ctvCapturePins">ctvCapturePins as TestMethodParams.CommaSeparatedString.</param>
        /// <param name="ctvData">Dictionary containing CtvData per pin.</param>
        /// <param name="exitPort">Exit port ofr Test Instance.</param>
        /// <param name="dataStructure">Dictionary data structure to store all input file.</param>
        /// <param name="ituffResults">Dictionary with Ituff data based on csv file ItuffTokens.</param>
        /// <param name="ituffFailFields">Dictionary with failing data to be printed in ituff.</param>
        public void CtvStructureProcessing(TestMethodsParams.CommaSeparatedString ctvCapturePins, Dictionary<string, string> ctvData, ref ushort exitPort, Dictionary<string, dynamic> dataStructure, Dictionary<string, Dictionary<string, Dictionary<string, string>>> ituffResults, Dictionary<string, Dictionary<string, Dictionary<string, string>>> ituffFailFields)
        {
            exitPort = 1;
            ituffResults.Clear();
            ituffFailFields.Clear();

            // Iterates through pins defined in testParams.
            foreach (KeyValuePair<string, string> ctv in ctvData)
            {
                this.ctvString = ctv.Value;
                Utils.PrintDebug($"\n[INFO] {ctv.Key} results: Test size: {this.pinStringSize[ctv.Key]} | CTV size: {this.ctvString.Length}\n");

                // Checks if CTVCapturePin exists.
                if (this.pinStringSize.ContainsKey(ctv.Key))
                {
                    // Check if input string length matches the expected value.
                    if (this.ctvString.Length == this.pinStringSize[ctv.Key])
                    {
                        string renamedPin = this.GetTssidRename(ctv.Key, ctvCapturePins, this.tssidRename);
                        this.NestedDictIteration(dataStructure[renamedPin], ref this.ctvString, ref exitPort, dataStructure, ituffResults, ituffFailFields);
                    }
                    else
                    {
                        Utils.PrintError($"[ERROR] The CTV data size [{this.ctvString.Length}] does not match the expected test input string size [{this.pinStringSize[ctv.Key]}]");
                    }
                }
            }
        }

        /// <summary>
        /// Function that updates the pivot dictionary with the update dictionary data.
        /// </summary>
        /// <param name="pivot_dict">The pivot dictionary we want to update.</param>
        /// <param name="update_dict">Dictionary with the updated data. Appends if common key exists, otherwise adds an item to pivot.</param>
        /// <returns> Returns the updated pivot dictionary.</returns>
        internal static Dictionary<string, dynamic> DictionaryUpdate(Dictionary<string, dynamic> pivot_dict, Dictionary<string, dynamic> update_dict)
        {
            foreach (string key in update_dict.Keys)
            {
                if (pivot_dict.ContainsKey(key))
                {
                    // Appends the flat dictionary, using a common key, to the pivot dictionary.
                    pivot_dict[key] = update_dict[key];
                }
                else
                {
                    // Adds the key and item value to the dictionary.
                    pivot_dict.Add(key, update_dict[key]);
                }
            }

            return pivot_dict;
        }

        /// <summary>
        /// Function that creates a flat dictionary with a Field on tail.
        /// This structure is needed to be merged properly.
        /// </summary>
        /// <param name="rungs">List of rungs in field hierarchy.</param>
        /// <param name="field">The field object.</param>
        /// <returns> Returns the flat dictionary.</returns>
        internal static Dictionary<string, dynamic> DictionaryTree(List<string> rungs, Field field)
        {
            rungs.Reverse();
            var tree_dict = new Dictionary<string, dynamic>();
            for (int i = 0; i < rungs.Count(); i++)
            {
                var tmp_dict = new Dictionary<string, dynamic>();
                if (i == 0)
                {
                    // Appends the field to the tail of flat dictionary.
                    tmp_dict.Add(rungs[i], field);
                }
                else
                {
                    // Appends the hierarchy level to the flat dictionary.
                    tmp_dict.Add(rungs[i], tree_dict);
                }

                tree_dict = tmp_dict;
            }

            return tree_dict;
        }

        /// <summary>
        /// Recursive function that merges the dictionaries.
        /// </summary>
        /// <param name="pivot_dict">The original pivot dictionary.</param>
        /// <param name="merging_dict">The dictionary we want to merge to the pivot dictionary. Has a flat dictionary structure.</param>
        /// <param name="row_index">CSV file row index for the current iterating field.</param>
        /// <returns> Returns the new pivot dictionary with the merged dictionary.</returns>
        internal static Dictionary<string, dynamic> DictionaryMerge(Dictionary<string, dynamic> pivot_dict, Dictionary<string, dynamic> merging_dict, int row_index)
        {
            foreach (string key in pivot_dict.Keys)
            {
                try
                {
                    // Loops recursively, to get to a new item for pivot dictionary.
                    if (merging_dict.ContainsKey(key))
                    {
                        merging_dict[key] = DictionaryMerge(pivot_dict[key], merging_dict[key], row_index);
                    }
                }
                catch (RuntimeBinderException)
                {
                    Utils.PrintError($"[ERROR] Found repeated Field name within same hierarchy in CSV file row {row_index}.");
                }
            }

            pivot_dict = DictionaryUpdate(pivot_dict, merging_dict);
            return pivot_dict;
        }

        /// <summary>
        /// Function to iterate over nested dictionaries.
        /// </summary>
        /// <param name="nestedDict">The dictionary we want to iterate.</param>
        /// <param name="ctvString">String containing the ctv data to be processed.</param>
        /// <param name="exitPort">Exit port ofr Test Instance.</param>
        /// <param name="dataStructure">Dictionary data structure to store all input file.</param>
        /// <param name="ituffResults">Dictionary with Ituff data based on csv file ItuffTokens.</param>
        /// <param name="ituffFailFields">Dictionary with failing data to be printed in ituff.</param>
        internal void NestedDictIteration(Dictionary<string, dynamic> nestedDict, ref string ctvString, ref ushort exitPort, Dictionary<string, dynamic> dataStructure, Dictionary<string, Dictionary<string, Dictionary<string, string>>> ituffResults, Dictionary<string, Dictionary<string, Dictionary<string, string>>> ituffFailFields)
        {
            // const string ItuffEngineeringDataCollectionPostFix = "edc";
            foreach (string key in nestedDict.Keys)
            {
                // Next level is a Field.
                if (nestedDict[key] == null || nestedDict[key].GetType() != nestedDict.GetType())
                {
                    Field field = (Field)nestedDict[key];
                    int size = field.Size;

                    if (field.CheckSetParameter(field.StringData))
                    {
                        if (size != 0)
                        {
                            Utils.PrintError($"{nameof(field.Size)} cannot be different from 0 if {nameof(field.StringData)} is previously defined from Configuration File.");
                        }
                    }
                    else
                    {
                        // Slice input string
                        string slice = Utils.SliceString(ref ctvString, size);

                        // Check if it is an offset field.
                        if (this.offsetRegex.IsMatch(field.Name))
                        {
                            continue;
                        }

                        // Set FieldData according to chopped CTV string.
                        field.SetFieldData(slice);

                        // Equation Case
                        this.SetEquationResult(field, dataStructure);

                        // Compare result to limits or expectedValue.
                        this.SetCompareDataValues(field, dataStructure);
                        field.CompareFieldData();

                        // Save date into the specified storage service.
                        field.StoreFieldData();
                    }

                    // Prints the field name and its parameters info
                    Utils.PrintDebug(field.ToString());

                    // Failport check
                    if (field.FailPort != 1)
                    {
                        // Add to FAILING CASES
                        if (field.FailPort == 0)
                        {
                            ItuffFormat.AddToItuff(ituffFailFields, field, ItuffFailingCasesPostFix);
                        }

                        // Sticky exit port.
                        if (this.exitPortUntouched)
                        {
                            this.exitPortUntouched = false;
                            Utils.PrintDebug("[INFO] Setting the exit port to 0 according to field FailPort");
                            exitPort = field.FailPort;
                        }
                    }

                    ItuffFormat.AddToItuff(ituffResults, field);

                    // Needed to exit field condition
                    continue;
                }

                this.NestedDictIteration((Dictionary<string, dynamic>)nestedDict[key], ref ctvString, ref exitPort, dataStructure, ituffResults, ituffFailFields);
            }
        }

        /// <summary>
        /// Function that renames from pin name to die name.
        /// </summary>
        /// <param name="pin"> The pin name. </param>
        /// <param name="ctvCapturePins">ctvCapturePins as TestMethodParams.CommaSeparatedString.</param>
        /// <param name="tssidRename">List of strings to rename CtvCapturePins.</param>
        /// <returns> Returns the die name. </returns>
        internal string GetTssidRename(string pin, TestMethodsParams.CommaSeparatedString ctvCapturePins, List<string> tssidRename)
        {
            // Empty tssidRename
            if (tssidRename.Count == 0)
            {
                return pin;
            }
            else
            {
                // Gets capturePins list from Test Parameter.
                var capturePins = ctvCapturePins.ToList();
                int pinIndex = capturePins.IndexOf(pin);

                // Gets dice list from Test Parameter
                var dice = tssidRename;

                return dice[pinIndex];
            }
        }

        /// <summary>
        /// Function set the limits and/or expected value from the string within input Field Parameters.
        /// </summary>
        /// <param name="dataField">The field with the data (limits and expected value) expressions.</param>
        /// <param name="dataStructure">Dictionary data structure to store all input file.</param>
        internal void SetCompareDataValues(Field dataField, Dictionary<string, dynamic> dataStructure)
        {
            if (dataField.CheckSetParameter(dataField.HighLimit))
            {
                dataField.HighLimitValue = this.StringParameterExpression(dataField, dataField.HighLimit, dataStructure);
                dataField.HighLimit = dataField.CheckParameterBase("HighLimitValue");
            }

            if (dataField.CheckSetParameter(dataField.LowLimit))
            {
                dataField.LowLimitValue = this.StringParameterExpression(dataField, dataField.LowLimit, dataStructure);
                dataField.LowLimit = dataField.CheckParameterBase("LowLimitValue");
            }

            if (dataField.CheckSetParameter(dataField.ExpectedData))
            {
                dataField.ExpectedDataValue = this.StringParameterExpression(dataField, dataField.ExpectedData, dataStructure);
                dataField.ExpectedData = dataField.CheckParameterBase("ExpectedDataValue");
            }
        }

        /// <summary>
        /// Function set the stringExpression to numbers from the string with field paths.
        /// </summary>
        /// <param name="equation_field">The field with the stringExpression.</param>
        /// <param name="dataStructure">Dictionary data structure to store all input file.</param>
        internal void SetEquationResult(Field equation_field, Dictionary<string, dynamic> dataStructure)
        {
            if (equation_field.CheckSetParameter(equation_field.Equation))
            {
                string equation = equation_field.Equation;

                equation_field.FieldData = this.StringParameterExpression(equation_field, equation, dataStructure);
                equation_field.StringData = equation_field.CheckParameterBase();
                Utils.PrintDebug($"[INFO] Set stringExpression result for field {equation_field.Path}. Result: {equation_field.StringData}.");
            }
        }

        /// <summary>
        /// Function to process an stringExpression expression.
        /// </summary>
        /// <param name="expressionField">The Field being processed.</param>
        /// <param name="stringExpression">The stringExpression to be processed.</param>
        /// <param name="dataStructure">Dictionary data structure to store all input file.</param>
        /// <returns> Returns the value from stringExpression.</returns>
        internal double StringParameterExpression(Field expressionField, string stringExpression, Dictionary<string, dynamic> dataStructure)
        {
            stringExpression = this.BaseStringReplace(stringExpression);

            // Regex pattern that matches field names, not only numbers.
            foreach (Match match in this.stringExpressionRegex.Matches(stringExpression))
            {
                // Ignore if Math operator is called
                if (this.mathRegex.IsMatch(match.Value))
                {
                    continue;
                }

                // Storage Replace
                var matchDataTypeRegex = this.dataTypeRegex.Match(match.Value);
                string token = matchDataTypeRegex.Groups[2].ToString();

                if (DataServices.SharedStorageCheck.IsMatch(token) || DataServices.DffCheck.IsMatch(token) || DataServices.UsrVarCheck.IsMatch(token))
                {
                    var storageValue = DataServices.GetData(match.Value);
                    stringExpression = Utils.ReplaceFirst(stringExpression, match.Value, storageValue.ToString());
                    stringExpression = this.BaseStringReplace(stringExpression);
                }

                // Local Storage (CSV defined field path)
                else
                {
                    string fetch_field_path = match.Value;

                    // Replace "this" to the current field name.
                    Match m = Regex.Match(fetch_field_path, @"this\b");
                    if (m.Success)
                    {
                        fetch_field_path = Utils.ReplaceFirst(fetch_field_path, "this", expressionField.Name);
                        Utils.PrintDebug($"[WARNING] Overwrite FieldData for stringExpression in field: {expressionField.Path}");
                    }

                    // Fetches the field according to the relative path (reference: current field).
                    Field fetched_field = this.GetFieldByPath(fetch_field_path, dataStructure, expressionField.Path);
                    stringExpression = Utils.ReplaceFirst(stringExpression, match.Value, fetched_field.FieldData.ToString());
                }
            }

            // Evaluates the string to get the stringExpression results and updates the Field Data.
            var expression = new Expression(stringExpression);
            var result = expression.Evaluate();

            if (typeof(int) == result.GetType())
            {
                return (double)(int)result;
            }
            else if (typeof(bool) == result.GetType())
            {
                return (bool)result ? 1.0 : 0.0;
            }
            else
            {
                return (double)result;
            }
        }

        /// <summary>
        /// Function to parse based values (hex, oct, bin) to decimal numbers in string format.
        /// </summary>
        /// <param name="baseString">The string that contains the based (0x, 0o, 0b) values.</param>
        /// <returns> Returns the updated string with decimal numbers. </returns>
        internal string BaseStringReplace(string baseString)
        {
            // Replace each hexadecimal match in string
            foreach (Match match in this.hexRegex.Matches(baseString))
            {
                int decValue = Convert.ToInt32(match.Value, 16);
                baseString = Utils.ReplaceFirst(baseString, match.Value, decValue.ToString());
            }

            // Replace each binary match in string
            foreach (Match match in this.binaryRegex.Matches(baseString))
            {
                string binaryValue = match.Value.Substring(2);
                int decValue = Convert.ToInt32(binaryValue, 2);
                baseString = Utils.ReplaceFirst(baseString, match.Value, decValue.ToString());
            }

            // Replace each octal match in string
            foreach (Match match in this.octalRegex.Matches(baseString))
            {
                string octalValue = match.Value.Substring(2);
                int decValue = Convert.ToInt32(octalValue, 8);
                baseString = Utils.ReplaceFirst(baseString, match.Value, decValue.ToString());
            }

            return baseString;
        }

        /// <summary>
        /// Function to get a Field from DataStructure using relative paths.
        /// </summary>
        /// <param name="path">The path of the field we want to return.</param>
        /// <param name="dataStructure">Dictionary data structure to store all input file.</param>
        /// <param name="absolute_path">The absolute path as starting point reference.</param>
        /// <returns> Returns the field fetched by path. </returns>
        internal Field GetFieldByPath(string path, Dictionary<string, dynamic> dataStructure, string absolute_path = "")
        {
            string[] rungs;
            if (absolute_path == string.Empty)
            {
                rungs = path.Split('.').ToArray<string>();
            }
            else
            {
                string[] abs_rungs = absolute_path.Split('.');
                Array.Reverse(abs_rungs);
                string[] fetch_rungs = path.Split('.');
                Array.Reverse(fetch_rungs);

                for (int i = 0; i < fetch_rungs.Length; i++)
                {
                    if (abs_rungs[i] != fetch_rungs[i])
                    {
                        abs_rungs[i] = fetch_rungs[i];
                    }
                }

                Array.Reverse(abs_rungs);
                rungs = abs_rungs.ToArray<string>();
            }

            dynamic next_level = dataStructure;
            int index = 0;
            while (next_level.GetType() == dataStructure.GetType())
            {
                string rung = rungs[index];
                try
                {
                    next_level = next_level[rung];
                }
                catch (System.Collections.Generic.KeyNotFoundException)
                {
                    Utils.PrintError($"[ERROR] Trying to access an invalid key in dictionary: [{rung}].\nIn case you are using TssidRename, you need to take the renamed pin as\nstarting point of an absolute path.");
                }

                index++;
            }

            return next_level;
        }
    }
}