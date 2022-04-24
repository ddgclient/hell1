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
    using System.Linq;
    using System.Text.RegularExpressions;
    using Prime.FunctionalService;

    /// <summary>
    /// Class containing shared functionality between modes of LSARaster.
    /// </summary>
    public class SharedFunctions
    {
        /// <summary>
        /// Try-get pin value from ctvData, throw an exception if not found.
        /// </summary>
        /// <param name="ctvData"> Dict that maps (pin name) -> (pin data). </param>
        /// <param name="pinName"> Name of pin we're trying to access. </param>
        /// <returns> Data from from the key of given pinName. </returns>
        public static string TryGetPinData(Dictionary<string, string> ctvData, string pinName)
        {
            try
            {
                return ctvData[pinName];
            }
            catch (KeyNotFoundException ex)
            {
                Prime.Services.ConsoleService.PrintError($"Could not find pin name [{pinName}] in ctvData.");
                throw ex;
            }
        }

        /// <summary>
        /// Given a filepath, use Prime services to retrieve text from the given file.
        /// </summary>
        /// <param name="filePath"> Filepath to the file to retrieve text from. </param>
        /// <returns> String containing the file's text. </returns>
        public static string RetrieveTextFromFile(string filePath)
        {
            if (!Prime.Services.FileService.FileExists(filePath))
            {
                throw new FileNotFoundException($"File located at: \"{filePath}\" cannot be found");
            }

            try
            {
                string primeFilePath = Prime.Services.FileService.GetFile(filePath);
                string primeFileText = File.ReadAllText(primeFilePath);
                return primeFileText;
            }
            catch (NullReferenceException ex)
            {
                Prime.Services.ConsoleService.PrintError("Error when accessing file " + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Given a string value, parse it and return a range of int values.
        /// </summary>
        /// <param name="ranges"> String representing ranges of indexes. </param>
        /// <returns> List of all indexes to parse. </returns>
        /// <remarks> If the provided string contains only one integer value, it will return the string as an int. </remarks>
        public static List<int> ParseRange(string ranges)
        {
            List<int> returnList = new List<int>();
            string[] rangeFields = ranges.Split(':');

            foreach (string field in rangeFields)
            {
                if (field.Contains('-'))
                {
                    int start;
                    int end;
                    string[] indexes = field.Split('-');
                    try
                    {
                        start = int.Parse(indexes[0]);
                        end = int.Parse(indexes[1]);
                    }
                    catch (ArgumentNullException ex)
                    {
                        Prime.Services.ConsoleService.PrintError($"Given value(s) for a range of string {ranges} returned null during parsing. ");
                        throw ex;
                    }
                    catch (FormatException ex)
                    {
                        Prime.Services.ConsoleService.PrintError($"Could not format a range of string {ranges} to an integer.");
                        throw ex;
                    }
                    catch (OverflowException ex)
                    {
                        Prime.Services.ConsoleService.PrintError($"A given value of string {ranges} is too large for type int.");
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        Prime.Services.ConsoleService.PrintError($"Unhandled exception occurred.");
                        throw ex;
                    }

                    if (start > end)
                    {
                        returnList.AddRange(Enumerable.Range(end, (start - end) + 1).Reverse().ToList<int>());
                    }
                    else
                    {
                        returnList.AddRange(Enumerable.Range(start, (end - start) + 1).ToList<int>());
                    }
                }
                else
                {
                    returnList.Add(int.Parse(field));
                }
            }

            return returnList;
        }

        /// <summary>
        /// Method for extracting defective indexes form FailIo.
        /// </summary>
        /// <param name="currentFailIo"> Failio to extract indexes from. </param>
        /// <returns> List of indexes. </returns>
        public static List<int> ExtractOnesIndexesFromFailIo(string currentFailIo)
        {
            List<int> indexes = new List<int>();

            currentFailIo = SharedFunctions.ReverseString(currentFailIo);

            // Iterate through backwards
            for (int i = 0; i < currentFailIo.Length; i++)
            {
                if (currentFailIo[i] == '1')
                {
                    indexes.Add(i);
                }
            }

            return indexes;
        }

        /// <summary>
        /// Checks if the given label contains a given string.
        /// </summary>
        /// <param name="label"> Label to check. </param>
        /// <param name="containedString"> The substring we check for in the label. </param>
        /// <returns> A value indicating whether the given label is a fail label or not. </returns>
        public static bool CheckLabelContains(string label, Regex containedString)
        {
            Prime.Services.ConsoleService.PrintDebug($"Checking Label {label} contains {containedString.ToString()}");

            MatchCollection matches = containedString.Matches(label);

            if (matches.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method for checking against all given DFM failures for a specific substring within their labels.
        /// </summary>
        /// <param name="faildata"> List of <see cref="IFailureData"/> to check against. </param>
        /// <param name="mismatchFail"> A <see cref="IFailureData"/> object that failed the check. Will be null if all fails contained the string specified. </param>
        /// <param name="containedString"> String to check against every <see cref="IFailureData"/> label. </param>
        /// <returns> A value indicating whether all fails matched against the given string. </returns>
        public static bool CheckAllLabelsContain(List<IFailureData> faildata, out IFailureData mismatchFail, string containedString)
        {
            bool substringMatch = true;
            mismatchFail = null;

            foreach (var fail in faildata)
            {
                var failLabel = Prime.Services.PatternService.GetLabelFromAddress(fail.GetPatternName(), fail.GetDomainName(), (uint)fail.GetVectorAddress(), false);
                substringMatch = CheckLabelContains(failLabel.GetName(), new Regex(containedString, RegexOptions.IgnoreCase));

                if (!substringMatch)
                {
                    mismatchFail = fail;
                    return substringMatch;
                }
            }

            return substringMatch;
        }

        /// <summary>
        /// Method for testing if a given property is null.
        /// </summary>
        /// <param name="property"> Propert that's under test for a null value.</param>
        /// <param name="propertyName"> Name of the property under test.</param>
        /// <returns> A value indicating whether the property is null. </returns>
        public static bool IsPropertyNull(object property, string propertyName)
        {
            try
            {
                if (property == null)
                {
                    Prime.Services.ConsoleService.PrintDebug($"{propertyName} returned null");
                    return true;
                }
            }
            catch (NullReferenceException ex)
            {
                Prime.Services.ConsoleService.PrintDebug($"Attempted access to {propertyName} raised a NullReferenceException");
                Prime.Services.ConsoleService.PrintDebug(ex.Message);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Simple method for taking the hexValue from a PatModifyElement, removing the prefix "0x".
        /// </summary>
        /// <param name="element"> Pat mod element we're taking the value from. </param>
        /// <returns> Hex value converted to int. </returns>
        public static int ExtractHexValue(RasterConfig.PatModifyElement element)
        {
            return int.Parse(element.Value.Substring(2), System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// Method for string reversal.
        /// </summary>
        /// <param name="input"> String to reverse. </param>
        /// <returns> Input reversed. </returns>
        public static string ReverseString(string input)
        {
            return new string(input.ToCharArray().Reverse().ToArray());
        }
    }
}
