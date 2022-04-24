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

namespace DDG
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using CommandLine;
    using Prime.SharedStorageService;
    using Prime.VoltageService;

    /// <summary>
    /// Implements IVoltageHandler.
    /// </summary>
    public static class VoltageHandler
    {
        /// <summary>
        /// Parses command line and extracts list of configurations.
        /// </summary>
        /// <param name="commandLine">Commandline.</param>
        /// <returns>List of rail configurations.</returns>
        public static List<string> GetRailConfigurations(string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine))
            {
                return null;
            }

            List<string> configurations = null;
            var parseResult = Parser.Default.ParseArguments<VoltageConverterOptions>(commandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            parseResult.WithParsed(
                options =>
                {
                    if (options.RailConfigurations != null)
                    {
                        configurations = new List<string>(options.RailConfigurations.ToList());
                    }
                }).WithNotParsed(e =>
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: failed parsing arguments. {string.Join("\n", e)}"));

            return configurations;
        }

        /// <summary>
        /// Parses command line and extracts all options.
        /// </summary>
        /// <param name="fivrCondition">FIVR condition.</param>
        /// <param name="commandLine">Commandline.</param>
        /// <returns>Voltage converter options. Null if there are no options.</returns>
        public static VoltageConverterOptions ParseCommandLine(string fivrCondition, string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine))
            {
                return null;
            }

            VoltageConverterOptions optionsResult = null;
            var parseResult = Parser.Default.ParseArguments<VoltageConverterOptions>(commandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            parseResult.WithParsed(
                result =>
                {
                    if (string.IsNullOrEmpty(result.FivrCondition))
                    {
                        result.FivrCondition = fivrCondition;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(fivrCondition))
                        {
                            throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: fivrcondition cannot be overwritten using command line. Use instance parameter. {commandLine}");
                        }
                    }

                    optionsResult = result;
                }).WithNotParsed(e =>
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: failed parsing arguments. {string.Join("\n", e)}"));

            return optionsResult;
        }

        /// <summary>
        /// Gets voltage overrides from VoltageConverterOptions.
        /// </summary>
        /// <param name="options">Options parsed object.</param>
        /// <returns>Voltage overrides values.</returns>
        public static Dictionary<string, double> GetVoltageOverrides(VoltageConverterOptions options)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                if (options?.Overrides == null || string.IsNullOrEmpty(options.Overrides))
                {
                    return null;
                }

                Regex rgx = new Regex(@"\s*([\w\\\/]+):([\w\\\/\.@^]+)\s*,*");
                if (!rgx.IsMatch(options.Overrides))
                {
                    throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: invalid {nameof(options.Overrides)}={options.Overrides} format.");
                }

                var overridesResult = new Dictionary<string, double>();

                var vminInterpolator = new VminForwardingBase.VminForwardingPrediction();
                var intermediate = rgx.Matches(options.Overrides).Cast<Match>().ToDictionary(match => match.Groups[1].Value.Trim(), match => match.Groups[2].Value.Trim());
                foreach (var item in intermediate)
                {
                    var value = item.Value.Trim();
                    if (!double.TryParse(value, out var voltage))
                    {
                        if (Prime.Services.SharedStorageService.KeyExistsInDoubleTable(value, Context.DUT))
                        {
                            voltage = Prime.Services.SharedStorageService.GetDoubleRowFromTable(value, Context.DUT);
                        }
                        else if (Prime.Services.UserVarService.Exists(value))
                        {
                            voltage = Prime.Services.UserVarService.GetDoubleValue(value);
                        }
                        else
                        {
                            var forwardingValues = value.Split('^');
                            var fullCornerName = forwardingValues.Last();
                            if (fullCornerName.Count(c => c == '@') != 1)
                            {
                                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: invalid {nameof(value)}={value} format. It doesn't exist in sharedstorage or uservars and isn't a vminCorner.");
                            }

                            var fullCornerNameItems = fullCornerName.Split('@');
                            if (!double.TryParse(fullCornerNameItems[1], out var targetFreqInGhz))
                            {
                                var flow = forwardingValues.Length == 2
                                    ? Prime.Services.TestProgramService.GetDomainCurrentFlow(forwardingValues[0])
                                    : Prime.Services.TestProgramService.GetCurrentFlowNumber();
                                var corner = DDG.VminForwarding.Service.Get(fullCornerName, flow);
                                voltage = corner.GetStartingVoltage(0.000);
                            }
                            else
                            {
                                voltage = vminInterpolator.GetVoltage(fullCornerNameItems[0], targetFreqInGhz);
                            }
                        }
                    }

                    overridesResult[item.Key] = voltage;
                }

                return overridesResult;
            }
        }

        /// <summary>
        /// Gets VForcePinAttribute object for voltage overrides.
        /// </summary>
        /// <param name="domains">Pin names.</param>
        /// <param name="levels">Levels.</param>
        /// <param name="patlist">Patlist required for fivr mode.</param>
        /// <param name="fivrCondition">Fivr condition, empty will default to VForcePinAttribute.</param>
        /// <param name="triggerCondition">Trigger test condition for Vbump mode.</param>
        /// <param name="commandLine">VoltageConverter options..</param>
        /// <param name="options">Voltage converter options.</param>
        /// <returns>Prime IVoltage Object.</returns>
        public static IVoltage GetVoltageObject(List<string> domains, string levels, string patlist, string fivrCondition, string triggerCondition, string commandLine, out VoltageConverterOptions options)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                options = ParseCommandLine(fivrCondition, commandLine);
                if (options != null && string.IsNullOrEmpty(fivrCondition))
                {
                    fivrCondition = options.FivrCondition;
                }

                if (!string.IsNullOrEmpty(fivrCondition))
                {
                    return CreateFivr(domains, levels, patlist, fivrCondition, options);
                }

                return CreateVForce(domains, levels, triggerCondition, options);
            }
        }

        /// <summary>
        /// Gets VForce required attributes from levels.
        /// </summary>
        /// <param name="voltageTargets">Distinct voltage targets.</param>
        /// <param name="levelsTc">LevelsTc.</param>
        /// <returns>Mandatory attributes per target.</returns>
        public static Dictionary<string, Dictionary<string, string>> GetVForceAttributesFromLevel(List<string> voltageTargets, string levelsTc)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                var pinsAttributes = new Dictionary<string, Dictionary<string, string>>();
                var level = Prime.Services.TestConditionService.GetTestCondition(levelsTc);
                foreach (var pinName in voltageTargets)
                {
                    var pin = Prime.Services.PinService.Get(pinName);
                    var pinAttributes = pin.GetVforceMandatoryAttributes();
                    if (pinsAttributes.ContainsKey(pinName))
                    {
                        continue;
                    }

                    pinsAttributes.Add(pinName, new Dictionary<string, string>());
                    foreach (var attribute in pinAttributes)
                    {
                        pinsAttributes[pinName][attribute] = level.GetPinAttributeValue(pinName, attribute);
                    }
                }

                return pinsAttributes;
            }
        }

        /// <summary>
        /// Applies initial voltage using VoltageConverter and/or Prime voltage object.
        /// </summary>
        /// <param name="voltageObject">Prime voltage object.</param>
        /// <param name="levels">Levels.</param>
        /// <param name="overrides">Voltage overrides.</param>
        public static void ApplyInitialVoltage(IVoltage voltageObject, string levels, Dictionary<string, double> overrides)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                if (overrides != null && overrides.Count > 0)
                {
                    if (voltageObject is IFivrCondition fivrConditionVoltage)
                    {
                        fivrConditionVoltage.ApplyConditionWithOverride(overrides);
                    }
                    else if (voltageObject is IVForcePinAttribute vForcePinAttribute)
                    {
                        vForcePinAttribute.Apply(overrides.Values.ToList());
                    }
                }
                else
                {
                    if (voltageObject is IFivrCondition fivrConditionVoltage)
                    {
                        fivrConditionVoltage.ApplyCondition();
                    }
                }
            }
        }

        /// <summary>
        /// Applies search voltage using VoltageConverter, Prime VoltageObject and/or offset.
        /// </summary>
        /// <param name="voltageObject">Prime voltage object.</param>
        /// <param name="voltageValues">Voltage values.</param>
        /// <param name="voltageOffset">Offset. Double, SharedStorage Key or UerVar.</param>
        public static void ApplySearchVoltage(IVoltage voltageObject, List<double> voltageValues, string voltageOffset)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                var localVoltageValues = ApplySearchVoltageOffset(voltageValues, voltageOffset);
                if (voltageObject is IVForcePinAttribute pinVoltage)
                {
                    pinVoltage.Apply(localVoltageValues);
                }
                else if (voltageObject is IFivrDomains domainVoltage)
                {
                    domainVoltage.Apply(localVoltageValues);
                }
            }
        }

        private static List<double> ApplySearchVoltageOffset(List<double> voltageValues, string voltageOffset)
        {
            var localVoltageValues = new List<double>(voltageValues);
            if (string.IsNullOrEmpty(voltageOffset))
            {
                return localVoltageValues;
            }

            var offset = voltageOffset.ToDouble(true);
            if (Math.Abs(offset) > 2 * double.Epsilon)
            {
                for (var i = 0; i < localVoltageValues.Count; i++)
                {
                    localVoltageValues[i] += offset;
                }
            }

            return localVoltageValues;
        }

        private static IVoltage CreateVForce(List<string> domains, string levels, string triggerCondition, VoltageConverterOptions options)
        {
            if (domains == null && options?.Overrides != null && options.Overrides.Any())
            {
                var voltageOverrides = GetVoltageOverrides(options);
                domains = new List<string>(voltageOverrides.Keys);
            }

            if (domains == null)
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: user must enter at least one domain.");
            }

            var pinsAttributes = GetVForceAttributesFromLevel(domains, levels);
            if (options?.RailConfigurations != null && options.RailConfigurations.Any())
            {
                return string.IsNullOrEmpty(triggerCondition)
                    ? Prime.Services.VoltageService.CreateVForceForPinAttributeWithRails(domains, pinsAttributes, options.RailConfigurations.ToList())
                    : Prime.Services.VoltageService.CreateVForceForPinTestCondition(domains, pinsAttributes, triggerCondition);
            }

            return string.IsNullOrEmpty(triggerCondition)
                ? Prime.Services.VoltageService.CreateVForceForPinAttribute(domains, pinsAttributes)
                : Prime.Services.VoltageService.CreateVForceForPinTestCondition(domains, pinsAttributes, triggerCondition);
        }

        private static IVoltage CreateFivr(List<string> domains, string levels, string patlist, string fivrCondition, VoltageConverterOptions options)
        {
            if (domains == null || domains.Count == 0)
            {
                return Prime.Services.VoltageService.CreateFivrForCondition(fivrCondition, patlist);
            }

            var rails = new List<string>();
            if (options?.RailConfigurations != null)
            {
                rails.AddRange(options.RailConfigurations);
            }

            if (options?.DlvrPins != null)
            {
                rails.AddRange(options.DlvrPins);
            }

            if (rails.Count <= 0)
            {
                return Prime.Services.VoltageService.CreateFivrForDomainsAndCondition(domains, fivrCondition, patlist);
            }

            if (options?.DlvrPins == null || !options.DlvrPins.Any())
            {
                return Prime.Services.VoltageService.CreateFivrDomainsAndConditionWithRails(domains, fivrCondition, patlist, rails);
            }

            var pinAttributes = GetVForceAttributesFromLevel(options.DlvrPins.ToList(), levels);
            var voltageObj = Prime.Services.VoltageService.CreateFivrDomainsAndConditionWithRails(domains, fivrCondition, patlist, rails, pinAttributes);
            if (options.OverrideExpressions == null || !options.OverrideExpressions.Any())
            {
                return voltageObj;
            }

            if (options.OverrideExpressions.Count() != options.DlvrPins.Count())
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: number of {nameof(options.DlvrPins)} must match the number of {nameof(options.OverrideExpressions)}.");
            }

            for (var i = 0; i < options.OverrideExpressions.Count(); i++)
            {
                voltageObj.OverrideExpression(RailHandlerType.DLVR, options.OverrideExpressions.ElementAt(i), options.DlvrPins.ElementAt(i));
            }

            return voltageObj;
        }
    }
}
