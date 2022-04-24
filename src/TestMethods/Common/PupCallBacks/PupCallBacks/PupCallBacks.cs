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

namespace PupCallBacks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Prime.Base.Exceptions;
    using Prime.SharedStorageService;

    /// <summary>
    /// This class is intended to overwrite the test method PrimeCallbacksRegistrarTestMethod.
    /// </summary>
    public class PupCallBacks
    {
        /// <summary>
        /// This function determines of the plist whose data passed as an argument is eligible for TTR or not.
        /// </summary>
        /// <param name="args">Arguments passed as single string with delimiters.</param>
        /// <returns>result - "1" - eligible for TTR , "0" not eligible for TTR.</returns>
        public static string IsPlistEligibleForTTR(string args)
        {
            Prime.Services.ConsoleService.PrintDebug("\n====\nStart Execution of IsPlistEligibleForTTR callback.\n====");

            var callbackResult = "1";
            var optimizations = ProcessCallBackArguments(args);
            var excludeValue = string.Empty;
            foreach (var optimization in optimizations)
            {
                if (!ResolveOptimization(optimization, ref excludeValue))
                {
                    callbackResult = $"{optimization.GetCriteriaName()}:{excludeValue}";
                    Prime.Services.ConsoleService.PrintDebug($"Result = [{callbackResult}]");
                    break;
                }
            }

            return callbackResult;
        }

        private static bool ResolveOptimization(ExcludeInformation information, ref string excludeValue)
        {
            var name = information.GetCriteriaName();
            var values = information.GetValues();

            // base number check
            if (name.EndsWith("EXBN"))
            {
                return ResolveCriteriaBn(values, ref excludeValue);
            }

            // flow number check
            if (name.EndsWith("EXFN"))
            {
                return ResolveCriteriaFlowNum(values, ref excludeValue);
            }

            // entry Vmin check
            if (name.EndsWith("EXVM"))
            {
                var function = information.GetHelpFunction();
                var comparison = information.GetComparisonOperation().ToLower();
                if (string.IsNullOrEmpty(function))
                {
                    throw new TestMethodException($"{name} - resolving function must be specified in format [ value|Function ].");
                }

                if (values.Count != 1)
                {
                    throw new TestMethodException($"{name} - Expecting single value. Actually - {values.Count} values identified.");
                }

                if (string.IsNullOrEmpty(comparison))
                {
                    throw new TestMethodException($"The comparison operation should be provided when it is {name}.");
                }

                return ResolveCriteriaEntryVmin(values[0], function, comparison, ref excludeValue);
            }

            if (string.Equals(name, "SharedStorageKey"))
            {
                return ResolveCriteriaSharedStorage(values, ref excludeValue);
            }

            if (string.Equals(name, "UserVarName"))
            {
                return ResolveCriteriaUserVar(values, ref excludeValue);
            }

            return true;
        }

        private static double ResolveFunction(string function, List<double> values)
        {
            double resultedValue;
            var lowerCaseFunction = function.ToLower();

            if (string.Equals("min", lowerCaseFunction))
            {
                resultedValue = values.Min();
            }
            else if (string.Equals("max", lowerCaseFunction))
            {
                resultedValue = values.Max();
            }
            else if (string.Equals("range", lowerCaseFunction))
            {
                if (values.Count > 1)
                {
                    resultedValue = values.Max() - values.Min();
                }
                else
                {
                    resultedValue = values[0];
                }
            }
            else if (string.Equals("avg", lowerCaseFunction))
            {
                resultedValue = values.Average();
            }
            else
            {
                throw new TestMethodException($"Function=[{function}] is an invalid value. Valid values are=[MIN, MAX, RANGE, AVG].");
            }

            return resultedValue;
        }

        private static bool ResolveCriteriaSharedStorage(IList<string> values, ref string excludeValue)
        {
            var noMatchingExcludeCriteriaFound = true;

            foreach (var value in values)
            {
                var sharedStorageInformation = value.Split(':').ToList();
                if (sharedStorageInformation.Count == 2)
                {
                    var context = string.Equals(sharedStorageInformation[0].ToLower(), "dut")
                        ? Context.DUT
                        : Context.LOT;
                    var storageKey = sharedStorageInformation[1];
                    var storedValue = string.Empty;
                    try
                    {
                        storedValue = Prime.Services.SharedStorageService.GetStringRowFromTable(storageKey, context);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    if (string.Equals(storedValue.ToLower(), "true"))
                    {
                        excludeValue = storageKey;
                        noMatchingExcludeCriteriaFound = false;
                        break;
                    }
                }
            }

            return noMatchingExcludeCriteriaFound;
        }

        private static bool ResolveCriteriaUserVar(IList<string> values, ref string excludeValue)
        {
            var noMatchingExcludeCriteriaFound = true;

            foreach (var value in values)
            {
                if (Prime.Services.UserVarService.Exists(value))
                {
                    var userVarValue = string.Empty;
                    try
                    {
                        userVarValue = Prime.Services.UserVarService.GetStringValue(value);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    if (string.Equals(userVarValue.ToLower(), "true"))
                    {
                        excludeValue = value;
                        noMatchingExcludeCriteriaFound = false;
                        break;
                    }
                }
            }

            return noMatchingExcludeCriteriaFound;
        }

        private static bool ResolveCriteriaBn(IList<string> values, ref string excludeValue)
        {
            var noMatchingExcludeCriteriaFound = true;
            foreach (var value in values)
            {
                var gsdsName = $"PUP_BASE_NUMBER_{value}";
                try
                {
                    /* Prime.Services.EvergreenService.GetGsdsUnitInt(gsdsName); // TODO: This should probably just use KeyExistsInIntegerTable(). */
                    Prime.Services.SharedStorageService.GetIntegerRowFromTable(gsdsName, Context.DUT);
                    noMatchingExcludeCriteriaFound = false;
                    excludeValue = value;
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return noMatchingExcludeCriteriaFound;
        }

        /// <summary>
        /// This is the comparison order" [referenceValue] [comparisonOperation] [valueToCompare].
        /// </summary>
        /// <param name="referenceValue">Value of reference.</param>
        /// <param name="valueToCompare">Value to compare.</param>
        /// <param name="comparisonOperation">Operation.</param>
        /// <returns>The comparison result between the reference and the comparison value using the comparison operation.</returns>
        private static bool CompareDouble(double referenceValue, double valueToCompare, string comparisonOperation)
        {
            if (string.Equals(comparisonOperation, "gt"))
            {
                return referenceValue > valueToCompare;
            }

            if (string.Equals(comparisonOperation, "lt"))
            {
                return referenceValue < valueToCompare;
            }

            if (string.Equals(comparisonOperation, "ge"))
            {
                return referenceValue >= valueToCompare;
            }

            if (string.Equals(comparisonOperation, "le"))
            {
                return referenceValue <= valueToCompare;
            }

            if (string.Equals(comparisonOperation, "eq"))
            {
                return referenceValue.CompareTo(valueToCompare) == 0;
            }

            return false;
        }

        private static bool ResolveCriteriaEntryVmin(string value, string function, string comparisonOperation, ref string excludeValue)
        {
            var noMatchingExcludeCriteriaFound = true;
            var startVoltageValues = new List<double>();
            try
            {
                /* var lastStartVoltages = Prime.Services.EvergreenService.GetGsdsUnitString("LastFastSearchInstanceStartVoltage").Split(',').ToList(); */
                var lastStartVoltages = Prime.Services.SharedStorageService.GetStringRowFromTable("LastFastSearchInstanceStartVoltage", Context.DUT).Split(',').ToList();
                foreach (var lastStartVoltage in lastStartVoltages)
                {
                    var startVoltage = double.Parse(lastStartVoltage);

                    // skip negative values indicating invalid start voltage
                    if (startVoltage >= 0)
                    {
                        startVoltageValues.Add(double.Parse(lastStartVoltage));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (startVoltageValues.Count > 0)
            {
                var valueToCompare = double.Parse(value);
                var resolvedValue = ResolveFunction(function, startVoltageValues);
                if (CompareDouble(resolvedValue, valueToCompare, comparisonOperation))
                {
                    excludeValue = "Actual-" + resolvedValue.ToString() + "_Criteria-" + value;
                    noMatchingExcludeCriteriaFound = false;
                    /* Prime.Services.EvergreenService.SetGsdsUnit("LastFastSearchInstanceStartVoltage", string.Empty); */
                    Prime.Services.SharedStorageService.InsertRowAtTable("LastFastSearchInstanceStartVoltage", string.Empty, Context.DUT);
                }
                else
                {
                    Prime.Services.ConsoleService.PrintDebug($"Comparison failed - [{resolvedValue} {comparisonOperation} {value}]");
                }
            }

            return noMatchingExcludeCriteriaFound;
        }

        private static bool ResolveCriteriaFlowNum(IList<string> values, ref string excludeValue)
        {
            var noMatchingExcludeCriteriaFound = true;
            var flow = default(int);
            try
            {
                /* flow = Prime.Services.EvergreenService.GetGsdsUnitInt("LastFastSearchInstanceFlow"); */
                flow = Prime.Services.SharedStorageService.GetIntegerRowFromTable("LastFastSearchInstanceFlow", Context.DUT);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            foreach (var value in values)
            {
                if (flow > long.Parse(value))
                {
                    excludeValue = flow.ToString();
                    noMatchingExcludeCriteriaFound = false;
                    Prime.Services.SharedStorageService.InsertRowAtTable("LastFastSearchInstanceFlow", -555555, Context.DUT);
                    break;
                }
            }

            return noMatchingExcludeCriteriaFound;
        }

        private static IList<ExcludeInformation> ProcessCallBackArguments(string args)
        {
            var excludeInformationList = new List<ExcludeInformation>();
            var splitArgs = args.Split('%');
            foreach (var arg in splitArgs)
            {
                var optimization = arg.Split('^');
                if (optimization.Length == 2)
                {
                    IList<string> values = new List<string>();
                    var function = string.Empty;
                    var comparisonOperation = string.Empty;
                    var optimizationName = optimization[0];
                    var optimizationSections = optimization[1].Split('|');
                    var optimizationSectionSize = optimizationSections.Length;
                    if (optimizationSectionSize > 0)
                    {
                        values = optimizationSections[0].Split('*');
                    }

                    if (optimizationSectionSize > 1)
                    {
                        function = optimizationSections[1];
                    }

                    if (optimizationSectionSize > 2)
                    {
                        comparisonOperation = optimizationSections[2];
                    }

                    if (values.Count == 0)
                    {
                        throw new TestMethodException($"Optimization=[{optimizationName}] has empty value.");
                    }

                    var information = new ExcludeInformation(optimizationName, function, comparisonOperation, values);
                    excludeInformationList.Add(information);
                    Prime.Services.ConsoleService.PrintDebug(
                        $"Argument name=[{optimizationName}], function=[{function}], comparison method=[{comparisonOperation}], value=[{optimizationSections[0]}]");
                }
            }

            return excludeInformationList;
        }

        /// <summary>
        /// Struct that represents all the exclude information.
        /// </summary>
        public class ExcludeInformation
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ExcludeInformation"/> class.
            /// </summary>
            /// <param name="name">Name.</param>
            /// <param name="helpFunction">Help function.</param>
            /// <param name="comparisonOperation">Comparison Operation.</param>
            /// <param name="values">Values.</param>
            public ExcludeInformation(string name, string helpFunction, string comparisonOperation, IList<string> values)
            {
                this.ExcludeCriteriaName = name;
                this.HelpFunction = helpFunction;
                this.ComparisonOperation = (comparisonOperation == string.Empty) ? ">" : comparisonOperation;
                this.Values = values;
            }

            /// <summary>
            /// Gets or sets the criteria name.
            /// </summary>
            private string ExcludeCriteriaName { get; set; }

            /// <summary>
            /// Gets or sets the help function: MAX, MIN, AVG and RANGE.
            /// </summary>
            private string HelpFunction { get; set; }

            /// <summary>
            /// Gets or sets the comparison operation.
            /// </summary>
            private string ComparisonOperation { get; set; }

            /// <summary>
            /// Gets or sets the values.
            /// </summary>
            private IList<string> Values { get; set; }

            /// <summary>
            /// Gets the criteria name.
            /// </summary>
            /// <returns>Criteria name.</returns>
            public string GetCriteriaName()
            {
                return this.ExcludeCriteriaName;
            }

            /// <summary>
            /// Gets the help function.
            /// </summary>
            /// <returns>Help Function.</returns>
            public string GetHelpFunction()
            {
                return this.HelpFunction;
            }

            /// <summary>
            /// Gets the comparison operation.
            /// </summary>
            /// <returns>Comparison operation.</returns>
            public string GetComparisonOperation()
            {
                return this.ComparisonOperation;
            }

            /// <summary>
            /// Gets the values.
            /// </summary>
            /// <returns>Values.</returns>
            public IList<string> GetValues()
            {
                return this.Values;
            }
        }
    }
}
