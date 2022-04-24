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
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using global::AnalogFuncCaptureCtv.ConfigurationFile;
    using NCalc;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;

    /// <summary>
    /// This class is intended to overwrite the members of the IFunctionalExtensions interfaces to extend the test method PrimeFuncCaptureCtvTestMethod.
    /// </summary>
    [PrimeTestMethod]
    public class AnalogFuncCaptureCtv : PrimeFunctionalTestMethod, IFunctionalExtensions
    {
        private readonly Regex offsetRegex = new Regex(@"^offset\d*\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex equationRegex = new Regex(@"(?!\d)[\w\.]+(\()?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex mathRegex = new Regex(@"[\w]+\(", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex sharedStorageTokenRegex = new Regex(@"(?!\d)\b(?:[*\^/+-])?[IDS]{1}\.\w+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Dictionary<string, Dictionary<string, string>> ituffResults = new Dictionary<string, Dictionary<string, string>>();
        private readonly Dictionary<string, Dictionary<string, string>> ituffFailFields = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, dynamic> dataStructure;
        private Dictionary<string, int> testSize;
        private ushort exitPort;
        private List<ushort> exitPortLst;
        private string ctvString;
        private List<string> pinRename;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogFuncCaptureCtv"/> class.
        /// </summary>
        public AnalogFuncCaptureCtv()
        {
            this.FileHandler = new FileHandler();
        }

        /// <summary>
        /// List of available EnabledDisabled states.
        /// </summary>
        public enum EnabledDisabled
        {
            /// <summary>
            /// ENABLED.
            /// </summary>
            ENABLED,

            /// <summary>
            /// DISABLED.
            /// </summary>
            DISABLED,
        }

        /// <summary>
        /// List of available ItuffDataBases.
        /// </summary>
        public enum ItuffDataBases
        {
            /// <summary>
            /// MIDAS.
            /// </summary>
            MIDAS,

            /// <summary>
            /// ARIES.
            /// </summary>
            ARIES,
        }

        /// <summary>
        /// Gets or sets CSV file with dataStructure parameters.
        /// </summary>
        public TestMethodsParams.File ConfigurationFile { get; set; }

        /// <summary>
        /// Gets or sets the PinRename.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString PinRename { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the PinRename.
        /// </summary>
        public TestMethodsParams.String CsvDelimiter { get; set; } = ",";

        /// <summary>
        /// Gets or sets Kill, should be either ENABLED or DISABLED.
        /// </summary>
        public EnabledDisabled Kill { get; set; }

        /// <summary>
        /// Gets or sets the IO operations file handler.
        /// </summary>
        protected IFileHandler FileHandler { get; set; }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Passed.")]
        [Returns(0, PortType.Fail, "Failed Port0")]
        [Returns(2, PortType.Fail, "Failed Port2")]
        [Returns(3, PortType.Fail, "Failed Port3")]
        [Returns(4, PortType.Fail, "Failed Port4")]
        [Returns(5, PortType.Fail, "Failed Port5")]
        [Returns(6, PortType.Fail, "Failed Port6")]
        public override int Execute()
        {
            base.Execute();
            return this.exitPort;
        }

        /// <inheritdoc />
        public override void CustomVerify()
        {
            if (!string.IsNullOrEmpty(this.PinRename.ToString()) && !this.CtvCapturePins.ToList().Count.Equals(this.PinRename.ToList().Count))
            {
                throw new ArgumentException($"Parameter {nameof(this.PinRename)} have different item count from {nameof(this.CtvCapturePins)}. Expected count: [{this.CtvCapturePins.ToList().Count}] {nameof(this.PinRename)} count: [{this.PinRename.ToList().Count}]");
            }

            this.pinRename = this.PinRename.ToList();

            string configFile = Prime.Services.FileService.GetFile(this.ConfigurationFile);
            Utils.PrintDebug("[INFO] This is the InputFile: " + configFile + " " + this.ConfigurationFile);
            if (string.IsNullOrEmpty(configFile))
            {
                throw new FileNotFoundException("[ERROR] ConfigurationFile: " + this.ConfigurationFile + " does not exist!", this.ConfigurationFile);
            }

            // Initialization of this.dataStructure with the csv file from the configFileContent path.
            var fileContents = this.FileHandler.ReadAllLines(configFile);
            this.DictionaryInit(fileContents);
        }

        /// <inheritdoc />
        bool IFunctionalExtensions.ProcessCtvPerPin(Dictionary<string, string> ctvData)
        {
            this.exitPort = 1;
            this.exitPortLst = new List<ushort>();

            // Iterates through pins defined in testParams.
            foreach (KeyValuePair<string, string> ctv in ctvData)
            {
                this.ctvString = ctv.Value;
                Utils.PrintDebug($"\n[INFO] {ctv.Key} results: Test size: {this.testSize[ctv.Key]} | CTV size: {this.ctvString.Length}\n");

                // Checks if CTVCapturePin exists.
                if (this.testSize.ContainsKey(ctv.Key))
                {
                    // Check if input string length matches the expected value.
                    if (this.ctvString.Length == this.testSize[ctv.Key])
                    {
                        string die = this.PinToDieName(ctv.Key);
                        this.NestedDictIteration(this.dataStructure[die]);
                    }
                    else
                    {
                        Utils.PrintError($"[ERROR] The CTV data size [{this.ctvString.Length}] doesnt match the expected test input string size [{this.testSize[ctv.Key]}]");
                    }
                }
                else
                {
                    Utils.PrintError("[ERROR] The CTVCapturePin is not found in the CSV input file.");
                }
            }

            ItuffFormat.PrintToItuff(this.ituffResults);
            ItuffFormat.PrintToItuff(this.ituffFailFields);
            this.ituffResults.Clear();
            this.ituffFailFields.Clear();
            this.exitPort = this.SetExitPort(this.exitPortLst);
            return this.exitPort == 1;
        }

        /// <summary>
        /// Function that updates the pivot dictionary with the update dictionary data.
        /// </summary>
        /// <param name="pivot_dict">The pivot dictionary we want to update.</param>
        /// <param name="update_dict">Dictionary with the updated data. Appends if common key exists, otherwise adds an item to pivot.</param>
        /// <returns> Returns the updated pivot dictionary.</returns>
        private static Dictionary<string, dynamic> DictionaryUpdate(Dictionary<string, dynamic> pivot_dict, Dictionary<string, dynamic> update_dict)
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
        /// Function to initialize the data dictionary in the Verify Method.
        /// </summary>
        /// <param name="configFileContent">The file's content.</param>
        private void DictionaryInit(string[] configFileContent)
        {
            // Initialize empty values of class params.
            this.dataStructure = new Dictionary<string, dynamic>();
            this.testSize = new Dictionary<string, int>();

            // List of headers in csvFile
            string[] header = configFileContent[0].Split(this.CsvDelimiter.ToString()[0]);

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
                string[] rowData = configFileContent[k].Split(this.CsvDelimiter.ToString()[0]);

                // Fills size dictionary with pins and test sizes.
                string pin = rowData[pin_index];
                int size = int.Parse(rowData[size_index]);

                if (this.testSize.ContainsKey(pin))
                {
                    this.testSize[pin] += size;
                }
                else
                {
                    this.testSize.Add(pin, size);
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
                        // Change the pin name to die name according to PinRename.
                        if (i == pin_index && this.pinRename.Count > 0)
                        {
                            string die = this.PinToDieName(pin);
                            rungs.Add(die);
                            rungDictionary.Add(header[i], pin);
                            rungDictionary.Add("Die", die);
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

                // Merges and update the dataStructure with the row information.
                var tree_dict = this.DictionaryTree(rungs, field);
                this.dataStructure = this.DictionaryMerge(this.dataStructure, tree_dict, k + 1);
            }
        }

        /// <summary>
        /// Function that creates a flat dictionary with a Field on tail.
        /// This structure is needed to be merged properly.
        /// </summary>
        /// <param name="rungs">List of rungs in field hierarchy.</param>
        /// <param name="field">The field object.</param>
        /// <returns> Returns the flat dictionary.</returns>
        private Dictionary<string, dynamic> DictionaryTree(List<string> rungs, Field field)
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
        /// <param name="row_index">Todo.</param>
        /// <returns> Returns the new pivot dictionary with the merged dictionary.</returns>
        private Dictionary<string, dynamic> DictionaryMerge(Dictionary<string, dynamic> pivot_dict, Dictionary<string, dynamic> merging_dict, int row_index)
        {
            foreach (string key in pivot_dict.Keys)
            {
                try
                {
                    // Loops recursively, to get to a new item for pivot dictionary.
                    if (merging_dict.ContainsKey(key))
                    {
                        merging_dict[key] = this.DictionaryMerge(pivot_dict[key], merging_dict[key], row_index);
                    }
                }
                catch
                {
                    Utils.PrintError($"Found repeated Field name within same hierarchy in CSV file row {row_index}.");
                }
            }

            pivot_dict = DictionaryUpdate(pivot_dict, merging_dict);
            return pivot_dict;
        }

        /// <summary>
        /// Function to iterate over nested dictionaries.
        /// </summary>
        /// <param name="nestedDict">The dictionary we want to iterate.</param>
        private void NestedDictIteration(Dictionary<string, dynamic> nestedDict)
        {
            foreach (string key in nestedDict.Keys)
            {
                // Next level is a Field.
                if (nestedDict[key] == null || nestedDict[key].GetType() != nestedDict.GetType())
                {
                    Field field = (Field)nestedDict[key];
                    int size = field.Size;

                    // Slice input string
                    var slices = Utils.SliceString(this.ctvString, size);
                    this.ctvString = slices[1];

                    // Check if it is an offset field.
                    if (this.offsetRegex.IsMatch(field.Name))
                    {
                        continue;
                    }

                    // Set FieldData according to chopped CTV string.
                    field.SetFieldData(slices[0]);

                    // Equation Case
                    this.SetEquationResult(field);

                    // Save to shared storage
                    field.SaveToSharedStorage();

                    // Compare result to limits or expectedValue.
                    this.exitPortLst.Add(field.CompareFieldData());

                    // Prints the field name and its parameters info
                    // Utils.PrintDebug(field.ToString());
                    if (this.exitPort != 1)
                    {
                        // Add to FAILING CASES
                        ItuffFormat.AddToItuff(this.ituffFailFields, field, "fc");
                        Utils.PrintDebug("[INFO] Setting the exit port to 0 according to field FailPort");

                        // this.exitPort = 0;
                    }

                    ItuffFormat.AddToItuff(this.ituffResults, field, "edc");

                    // Needed to exit field condition
                    continue;
                }

                this.NestedDictIteration((Dictionary<string, dynamic>)nestedDict[key]);
            }
        }

        /// <summary>
        /// Function set the equation to numbers from the string with field paths.
        /// </summary>
        /// <param name="equation_field">The field with the equation.</param>
        private void SetEquationResult(Field equation_field)
        {
            if (equation_field.CheckSettedParam(equation_field.Equation))
            {
                string equation = equation_field.Equation;

                equation_field.FieldData = this.IntEquationExpression(equation_field, equation);
                equation_field.FieldStrData = equation_field.CheckBase();
                Utils.PrintDebug($"[INFO] Setted equation result for field {equation_field.Path}. Result: {equation_field.FieldStrData}.");
            }
        }

        /// <summary>
        /// Function to process an equation expression.
        /// </summary>
        /// <param name="equationField">The Field being processed.</param>
        /// <param name="equation">The equation to be processed.</param>
        /// <returns> Returns the value from equation.</returns>
        private int IntEquationExpression(Field equationField, string equation)
        {
            // Regex pattern that matches field names, not only numbers.
            foreach (Match match in this.equationRegex.Matches(equation))
            {
                // Ignore if Math operator is called
                if (this.mathRegex.IsMatch(match.Value))
                {
                    continue;
                }

                // Shared Storage Replace
                if (this.sharedStorageTokenRegex.IsMatch(match.Value))
                {
                    var sharedStorageValue = SharedStorage.SharedStorageGetValue(match.Value);
                    equation = equation.Replace(match.Value, sharedStorageValue.ToString());
                }

                // Common field path
                else
                {
                    string fetch_field_path = match.Value;

                    // Replace "this" to the current field name.
                    Match m = Regex.Match(fetch_field_path, @"this\b");
                    if (m.Success)
                    {
                        fetch_field_path = fetch_field_path.Replace("this", equationField.Name);
                        Utils.PrintDebug($"[WARNING] Overwrite FieldData for equation in field: {equationField.Path}");
                    }

                    // Fetches the field according to the relative path (reference: current field).
                    Field fetched_field = this.GetFieldByPath(fetch_field_path, equationField.Path);
                    equation = equation.Replace(match.Value, fetched_field.FieldData.ToString());
                }
            }

            // Evaluates the string to get the equation results and updates the Field Data.
            var expression = new Expression(equation);
            var result = expression.Evaluate();

            if (typeof(int) == result.GetType())
            {
                return (int)result;
            }
            else
            {
                return (int)(double)result;
            }
        }

        /// <summary>
        /// Function to get a Field from DataStructure using relative paths.
        /// </summary>
        /// <param name="path">The path of the field we want to return.</param>
        /// <param name="absolute_path">The absolute path as starting point reference.</param>
        /// <returns> Returns the field fetched by path. </returns>
        private Field GetFieldByPath(string path, string absolute_path = "")
        {
            List<string> rungs;
            if (absolute_path == string.Empty)
            {
                rungs = path.Split('.').ToList<string>();
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
                rungs = abs_rungs.ToList<string>();
            }

            dynamic next_level = this.dataStructure;
            int index = 0;
            while (next_level.GetType() == this.dataStructure.GetType())
            {
                string rung = rungs[index];
                next_level = next_level[rung];
                index++;
            }

            return next_level;
        }

        /// <summary>
        /// Function that renames from pin name to die name.
        /// </summary>
        /// <param name="pin"> The pin name. </param>
        /// <returns> Returns the die name. </returns>
        private string PinToDieName(string pin)
        {
            // Gets capturePins list from Test Paramater.
            var capturePins = this.CtvCapturePins.ToList();
            int pinIndex = capturePins.IndexOf(pin);

            // Gets dice list from Test Paramater
            var dice = this.pinRename;

            if (pinIndex >= dice.Count())
            {
                Utils.PrintDebug($"[WARNING] The Pin {pin} [index:{pinIndex}] does not have a Pin Rename.");
                return pin;
            }

            return dice[pinIndex];
        }

        /// <summary>
        /// Compares the list of  Exit ports and returns the higher prority Exit port.
        /// </summary>
        /// <returns> Returns the Exit port based on the prioirty.</returns>
        private ushort SetExitPort(List<ushort> exitPortList)
        {
            List<ushort> passLst = new List<ushort>();
            List<ushort> failLst = new List<ushort>();
            exitPortList = exitPortList.OrderBy(x => x).ToList();
            foreach (var exitvar in exitPortList)
            {
                if (exitvar == 1)
                {
                    passLst.Add(exitvar);
                }
                else
                {
                    failLst.Add(exitvar);
                }
            }

            if (failLst.Count == 0 && passLst.Count == exitPortList.Count)
            {
                return 1;
            }
            else
            {
                if (failLst[0] <= 6)
                {
                    return failLst[0];
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
