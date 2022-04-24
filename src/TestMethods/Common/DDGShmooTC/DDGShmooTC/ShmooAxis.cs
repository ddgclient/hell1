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

namespace DDGShmooTC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using DDG;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.TestConditionService;
    using Prime.VoltageService;

    /// <summary>
    /// Defines the <see cref="ShmooAxis" />.
    /// </summary>
    /// TODO: should really make ShmooAxis a base class and create child classes for each type.
    internal class ShmooAxis
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShmooAxis"/> class.
        /// </summary>
        /// <param name="id">The Axis Identifier (X or Y) of type <see cref="DDGShmooTC.AxisID"/>.</param>
        /// <param name="axisType">The Axis parameter type <see cref="DDGShmooTC.AxisType"/>.</param>
        /// <param name="axisParam">The Axis parameter name.</param>
        /// <param name="timingsTc">The name of the Timings TestCondition for this instance.</param>
        /// <param name="levelsTc">The name of the Leves TestCondition for this instance.</param>
        /// <param name="patList">The name of the pattern list for this instance.</param>
        /// <param name="voltageConverterCommandLine">Voltage converter command line.</param>
        /// <param name="console">Either Prime.Services.ConsoleService or null.</param>
        internal ShmooAxis(DDGShmooTC.AxisID id, DDGShmooTC.AxisType axisType, string axisParam, string timingsTc, string levelsTc, string patList, string voltageConverterCommandLine, IConsoleService console)
        {
            this.ID = id;
            this.Type = axisType;
            this.Parameter = axisParam;
            this.IsValid = !string.IsNullOrWhiteSpace(axisParam) && axisType != DDGShmooTC.AxisType.None;
            this.IsTestConditionType = this.Type == DDGShmooTC.AxisType.SpecSetVariable || this.Type == DDGShmooTC.AxisType.UserVarLevels || this.Type == DDGShmooTC.AxisType.UserVarTiming;
            this.Console = console;

            if (!this.IsValid)
            {
                this.Type = DDGShmooTC.AxisType.None;
            }

            switch (axisType)
            {
                case DDGShmooTC.AxisType.None:
                    break;

                case DDGShmooTC.AxisType.FIVR:
                    // Setup the Prime Voltage object related to the Axis.
                    var paramLst = axisParam.Split(',').Select(o => o.Trim()).ToList();
                    this.VoltageObj = DDG.VoltageHandler.GetVoltageObject(paramLst, levelsTc, patList, null, null, voltageConverterCommandLine, out var options);
                    this.VoltageConverterOptions = options;
                    /* TODO: how to get the default/current FIVR value? */
                    break;

                case DDGShmooTC.AxisType.SpecSetVariable:
                    this.TestCondition = this.RetrieveTcParam(axisParam, levelsTc, timingsTc);
                    if (this.TestCondition == null)
                    {
                        throw new TestMethodException($"{id}-Axis Parameter=[{axisParam}] is not a valid SpecSet Variable.");
                    }

                    this.OriginalValue = new List<double> { Convert.ToDouble(this.TestCondition.GetSpecSetValue(axisParam)) };
                    break;

                case DDGShmooTC.AxisType.UserVarLevels:
                    this.TestCondition = Prime.Services.TestConditionService.GetTestCondition(levelsTc);
                    var userVarLvlParamLst = axisParam.Split(',').Select(o => o.Trim()).ToList();
                    this.OriginalValue = new List<double>(userVarLvlParamLst.Count);
                    foreach (var userVar in userVarLvlParamLst)
                    {
                        this.OriginalValue.Add(Prime.Services.UserVarService.GetDoubleValue(userVar));
                    }

                    this.TcType = TestConditionType.LEVELS;
                    break;

                case DDGShmooTC.AxisType.UserVarTiming:
                    this.TestCondition = Prime.Services.TestConditionService.GetTestCondition(timingsTc);
                    var userVarTimParamLst = axisParam.Split(',').Select(o => o.Trim()).ToList();
                    this.OriginalValue = new List<double>(userVarTimParamLst.Count);
                    foreach (var userVar in userVarTimParamLst)
                    {
                        this.OriginalValue.Add(Prime.Services.UserVarService.GetDoubleValue(userVar));
                    }

                    this.TcType = TestConditionType.TIMING;
                    break;

                case DDGShmooTC.AxisType.PatConfig:
                    /* TODO: how to get the current PatConfig value? */
                    this.PatConfigs = new List<IPatConfigHandle>();
                    foreach (var param in axisParam.Split(','))
                    {
                        this.PatConfigs.Add(Prime.Services.PatConfigService.GetPatConfigHandleWithPlist(param, patList));
                    }

                    break;

                case DDGShmooTC.AxisType.PatConfigSetPoint:
                    /* TODO: how to get the current SetPoint value? */
                    this.PatConfigSetPoints = new List<IPatConfigSetPointHandle>();
                    foreach (var param in axisParam.Split(','))
                    {
                        var setPointPair = param.Split(':');
                        if (setPointPair.Length != 2)
                        {
                            throw new TestMethodException($"PatConfigSetpoint=[{param}] is in the wrong format. Expecting [module:group]");
                        }

                        this.PatConfigSetPoints.Add(Prime.Services.PatConfigService.GetSetPointHandle(setPointPair[0], setPointPair[1], patList));
                    }

                    break;

                default:
                    throw new TestMethodException($"Axis=[{id}] does not support type=[{axisType}].");
            }
        }

        /// <summary>
        /// Gets the Axis ID (X or Y).
        /// </summary>
        internal DDGShmooTC.AxisID ID { get; }

        /// <summary>
        /// Gets a value indicating whether this axis is valid or not.
        /// </summary>
        internal bool IsValid { get; }

        /// <summary>
        /// Gets a value indicating whether the TestPoints list should be used as
        /// an index into TestPointStringList to get the real testpoint values.
        /// </summary>
        internal bool UseTestPointsAsIndex { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this axis is tied to a testcondition.
        /// </summary>
        internal bool IsTestConditionType { get; }

        /// <summary>
        /// Gets the name of the Parameter for this axis.
        /// </summary>
        internal string Parameter { get; }

        /// <summary>
        /// Gets the Axis Type.
        /// </summary>
        internal DDGShmooTC.AxisType Type { get; }

        /// <summary>
        /// Gets the testcondition associated with this axis.
        /// </summary>
        internal ITestCondition TestCondition { get; private set; } = null;

        /// <summary>
        /// Gets the Prime IVoltage associated with this axis.
        /// </summary>
        internal IVoltage VoltageObj { get; private set; } = null;

        /// <summary>
        /// Gets or sets voltage converter options.
        /// </summary>
        internal VoltageConverterOptions VoltageConverterOptions { get; set; } = null;

        /// <summary>
        /// Gets the list of PatConfigs associated with this axis.
        /// </summary>
        internal List<IPatConfigHandle> PatConfigs { get; private set; } = new List<IPatConfigHandle>();

        /// <summary>
        /// Gets the list of PatConfig SetPoints associated with this axis.
        /// </summary>
        internal List<IPatConfigSetPointHandle> PatConfigSetPoints { get; private set; } = new List<IPatConfigSetPointHandle>();

        /// <summary>
        /// Gets the original value of this Axis.
        /// </summary>
        internal List<double> OriginalValue { get; private set; } = new List<double> { -1 };

        /// <summary>
        /// Gets all the testpoints for this axis.
        /// </summary>
        internal List<double> TestPoints { get; private set; }

        /// <summary>
        /// Gets all the testpoints as strings.
        /// </summary>
        internal List<string> TestPointStringList { get; private set; }

        /// <summary>
        /// Gets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; }

        private TestConditionType TcType { get; set; } = TestConditionType.ALL;

        /// <summary>
        /// Updates the TestCondition or Hardware for this Axis based on the value given.
        /// </summary>
        /// <param name="testPoint">TestPoint value or index.</param>
        /// <param name="funcTest">IFunctionalTest to use for the update (TestCondition Types only).</param>
        internal void UpdateAndApply(double testPoint, IFunctionalTest funcTest = null)
        {
            var axisValue = this.UseTestPointsAsIndex ? this.TestPointStringList[(int)testPoint] : testPoint.ToString();
            switch (this.Type)
            {
                case DDGShmooTC.AxisType.None:
                    break;

                case DDGShmooTC.AxisType.SpecSetVariable:
                    this.Console?.PrintDebug($"Setting {this.ID}-Axis Parameter=[{this.Parameter}] Value=[{axisValue}]");
                    this.TestCondition.SetSpecSetValue(this.Parameter, axisValue);
                    this.ApplyTestCondition(funcTest);
                    break;

                case DDGShmooTC.AxisType.UserVarTiming:
                case DDGShmooTC.AxisType.UserVarLevels:
                    this.Console?.PrintDebug($"Setting {this.ID}-Axis UserVar=[{this.Parameter}] Value=[{axisValue}]");
                    foreach (var param in this.Parameter.Split(','))
                    {
                        Prime.Services.UserVarService.SetValue(param, Convert.ToDouble(axisValue));
                    }

                    this.TestCondition.Resolve();
                    if (Prime.Services.TestConditionService.IsSmartTcEnabled())
                    {
                        Prime.Services.TestConditionService.FlushSmartTCCategory(this.TcType == TestConditionType.LEVELS ? SmartTCCategoryType.LEVELS_SETUP : SmartTCCategoryType.TIMING);
                    }

                    this.ApplyTestCondition(funcTest);
                    break;

                case DDGShmooTC.AxisType.FIVR:
                    var overrides = DDG.VoltageHandler.GetVoltageOverrides(this.VoltageConverterOptions);
                    DDG.VoltageHandler.ApplyInitialVoltage(this.VoltageObj, null, overrides);
                    this.Console?.PrintDebug($"Setting {this.ID}-Axis FIVR=[{this.Parameter}] Value=[{axisValue}]");
                    var fivrValue = Enumerable.Repeat(Convert.ToDouble(axisValue), this.Parameter.Split(',').Length).ToList();
                    DDG.VoltageHandler.ApplySearchVoltage(this.VoltageObj, fivrValue, null);
                    break;

                case DDGShmooTC.AxisType.PatConfig:
                    this.Console?.PrintDebug($"Setting {this.ID}-Axis PatConfig=[{this.Parameter}] Value=[{axisValue}]");
                    var data = this.PatConfigs.Select(o => axisValue.IntegerToBinary((int)o.GetExpectedDataSize())).ToList();

                    // this.AxisDetails[id].PatConfigs.ForEach(o => o.SetData(axisValue));
                    for (var i = 0; i < data.Count; i++)
                    {
                        this.PatConfigs[i].SetData(data[i]);
                    }

                    Prime.Services.PatConfigService.Apply(this.PatConfigs);
                    break;

                case DDGShmooTC.AxisType.PatConfigSetPoint:
                    this.Console?.PrintDebug($"Setting {this.ID}-Axis PatConfigSetpoint=[{this.Parameter}] Value=[{axisValue}]");
                    this.PatConfigSetPoints.ForEach(o => o.ApplySetPoint(axisValue));
                    break;

                default:
                    throw new TestMethodException($"Axis=[{this.ID}] does not support type=[{this.Type}].");
            }
        }

        /// <summary>
        /// Reset the axis back to its original value if possible.
        /// </summary>
        internal void ResetOriginalValue()
        {
            switch (this.Type)
            {
                case DDGShmooTC.AxisType.None:
                    break;

                case DDGShmooTC.AxisType.SpecSetVariable:
                    this.Console?.PrintDebug($"Restoring {this.ID}-Axis Parameter=[{this.Parameter}] to original Value=[{this.OriginalValue.FirstOrDefault()}]");
                    this.TestCondition.SetSpecSetValue(this.Parameter, this.OriginalValue.FirstOrDefault().ToString());
                    break;

                case DDGShmooTC.AxisType.UserVarTiming:
                case DDGShmooTC.AxisType.UserVarLevels:
                    this.Console?.PrintDebug($"Restoring {this.ID}-Axis UserVar=[{this.Parameter}] to original Value=[{string.Join(",", this.OriginalValue)}]");
                    var paramList = this.Parameter.Split(',');
                    for (var i = 0; i < paramList.Length; i++)
                    {
                        Prime.Services.UserVarService.SetValue(paramList[i], this.OriginalValue[i]);
                    }

                    this.TestCondition.Resolve();
                    if (Prime.Services.TestConditionService.IsSmartTcEnabled())
                    {
                        Prime.Services.TestConditionService.FlushSmartTCCategory(this.TcType == TestConditionType.LEVELS ? SmartTCCategoryType.LEVELS_SETUP : SmartTCCategoryType.TIMING);
                    }

                    break;

                case DDGShmooTC.AxisType.FIVR:
                    this.Console?.PrintDebug($"Restoring {this.ID}-Axis Parameter=[{this.Parameter}] using IVoltage.Restore().");
                    this.VoltageObj?.Restore();
                    break;

                default:
                    this.Console?.PrintDebug($"Cannot restore {this.ID}-Axis Paremeter=[{this.Parameter}] of type [{this.Type}]");
                    break;
            }
        }

        /// <summary>
        /// Updates the Axis TestPoints/TestPointStringList from the given range equation.
        /// </summary>
        /// <param name="rangeParameter">Range equation. Must match either "start:resolution:numsteps" or "value1,value2,...,valueN".</param>
        /// <returns>The testpoints or testpoint indexes as list of double.</returns>
        internal List<double> DecodeRange(string rangeParameter)
        {
            List<double> points;
            this.UseTestPointsAsIndex = false;

            // TODO: differentiating Range types (comma separated or start:resolution:count) needs to be more robust.
            if (!this.IsValid)
            {
                points = new List<double> { double.NaN };
            }
            else if (rangeParameter.Contains(',') || rangeParameter.Count(c => c == ':') != 2)
            {
                this.TestPointStringList = rangeParameter.Split(',').Select(o => o.Trim()).ToList();
                this.UseTestPointsAsIndex = true;
                points = Enumerable.Range(0, this.TestPointStringList.Count).Select(i => (double)i).ToList();
            }
            else
            {
                points = this.ParsePointsRange(rangeParameter);
            }

            this.TestPoints = points;
            return points;
        }

        // TODO: duplicate code, base class method is inaccessible.
        private List<double> ParsePointsRange(string pointsRangeExpression)
        {
            // Tokenize the points range as "START_POINT:RESOLUTION:NUMBER_OF_POINTS".
            var rangeFields = this.ExtractRangeFields(pointsRangeExpression);

            // conversion to double guaranteed by verify
            return this.GeneratePointsSequence(
                Convert.ToDouble(rangeFields.Start),
                Convert.ToDouble(rangeFields.Resolution),
                Convert.ToDouble(rangeFields.NumberOfPoints));
        }

        // TODO: duplicate code, base class method is inaccessible.
        private List<double> GeneratePointsSequence(double start, double resolution, double numberOfPoints)
        {
            var points = new List<double>();
            for (var pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
            {
                points.Add(start + (pointIndex * resolution));
            }

            return points;
        }

        // function used internally for getting x and y params test conditions and updating member variables.
        // TODO: duplicate code, base class method is inaccessible.
        private ITestCondition RetrieveTcParam(string specParam, string levelsTc, string timingsTc)
        {
            var lvlTC = Prime.Services.TestConditionService.GetTestCondition(levelsTc) ??
                        throw new TestMethodException(
                            "Prime.Services.TestConditionService.GetTestCondition(this.LevelsTc)");

            var timTC = Prime.Services.TestConditionService.GetTestCondition(timingsTc) ??
                        throw new TestMethodException(
                            "Prime.Services.TestConditionService.GetTestCondition(this.TimingsTc)");

            try
            {
                lvlTC.GetSpecSetValue(specParam);
                this.TcType = TestConditionType.LEVELS;
                return lvlTC;
            }
            catch (FatalException)
            {
            }

            try
            {
                timTC.GetSpecSetValue(specParam);
                this.TcType = TestConditionType.TIMING;
                return timTC;
            }
            catch (FatalException)
            {
            }

            return null;
        }

        // TODO: duplicate code, base class method is inaccessible.
        private DDGShmooTC.ExtractedRangeFields ExtractRangeFields(string xAxisRange)
        {
            var r = new Regex(@"^\s*(.+):(.+):(.+)\s*$", RegexOptions.IgnoreCase);
            var match = r.Match(xAxisRange);
            if (match.Success)
            {
                DDGShmooTC.ExtractedRangeFields fieldsValues;
                fieldsValues.Start = match.Groups[1].Value;
                fieldsValues.Resolution = match.Groups[2].Value;
                fieldsValues.NumberOfPoints = match.Groups[3].Value;
                return fieldsValues;
            }

            // ERROR
            throw new Prime.Base.Exceptions.TestMethodException($"Invalid AxisRange=[{xAxisRange}]. Expecting [START_POINT:RESOLUTION:NUMBER_OF_POINTS].");
        }

        private void ApplyTestCondition(IFunctionalTest funcTest)
        {
            if (this.TcType == TestConditionType.LEVELS)
            {
                funcTest.ApplyLevelTestCondition();
            }
            else
            {
                funcTest.ApplyTimingTestCondition();
            }
        }
    }
}
