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

namespace IVCurve
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Prime.Base.Exceptions;
    using Prime.DcService;
    using Prime.PhAttributes;
    using Prime.SharedStorageService;
    using Prime.TestConditionService;
    using Prime.TestMethods;

    /// <summary>
    /// IV Test Class: able to move a force value and take multiple measurements.
    /// </summary>
    [PrimeTestMethod]
    public class IVCurve : TestMethodBase
    {
        private const int NumberOfPorts = 30;

        private readonly List<string> invalidReadAttributes = new List<string>
        {
            "PreMeasurementDelay",
            "StartMeasurement",
            "SamplingCount",
            "SamplingMode",
            "SamplingRatio",
        };

        private DcSetup perPinLimits;
        private List<MeasurementType> measurementType;
        private List<string> pinNames;
        private List<double> lowLimits;
        private List<double> highLimits;
        private Dictionary<string, Dictionary<string, string>> pinAttributes;
        private List<string> vRange;
        private List<string> iRange;
        private List<double> iClampHi;
        private List<double> iClampLo;
        private List<double> freeDriveTime;
        private List<double> freeDriveCurrentHi;
        private List<double> freeDriveCurrentLo;
        private List<double> vSlewStepRatio;
        private List<double> vClamp;
        private List<double> overVoltageLimit;
        private List<double> underVoltageLimit;
        private List<double> samplingRatio;
        private List<int> samplingCount;
        private List<double> preMeasurementDelay;
        private List<double> forceStartValue;
        private List<double> forceStopValue;
        private List<double> forceStepSize;
        private List<string> sharedStorageTokens;
        private List<double> forceSetPoint;
        private DatalogLevel datalogLevel;
        private ITestCondition loadLevels;
        private IDcTest levelDcTest;

        /// <summary>
        /// Test class execution modes.
        /// </summary>
        public enum Modes
        {
            /// <summary>
            /// Skips characterization.
            /// </summary>
            Production,

            /// <summary>
            /// Runs characterization.
            /// </summary>
            Characterization,
        }

        /// <summary>
        /// The datalog level for Ituff printout(ALL, FAIL_ONLY).
        /// </summary>
        public enum DatalogLevels
        {
            /// <summary>
            /// Print fail and pass results.
            /// </summary>
            All,

            /// <summary>
            /// Print fail results only.
            /// </summary>
            FailOnly,
        }

        /// <summary>
        /// Enabled or disabled switch.
        /// </summary>
        public enum AlarmModes
        {
            /// <summary>
            /// Enabled.
            /// </summary>
            Enabled,

            /// <summary>
            /// Disabled.
            /// </summary>
            Disabled,
        }

        /// <summary>
        /// Gets or sets LevelsTc to use.
        /// </summary>
        public TestMethodsParams.LevelsCondition LevelsTc { get; set; }

        /// <summary>
        /// Gets or sets comma separated Pins to get DC results for.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString Pins { get; set; }

        /// <summary>
        /// Gets or sets measurement type(Current, Voltage").
        /// </summary>
        public TestMethodsParams.CommaSeparatedString Type { get; set; }

        /// <summary>
        /// Gets or sets datalog level(All, FailOnly").
        /// </summary>
        public DatalogLevels DatalogLevel { get; set; } = DatalogLevels.FailOnly;

        /// <summary>
        /// Gets or sets comma separated low limits for the measure Pins.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString LowLimits { get; set; }

        /// <summary>
        /// Gets or sets comma separated high limits for the measure Pins.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString HighLimits { get; set; }

        /// <summary>
        /// Gets or sets force start value for characterization mode.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble ForceStartValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets force stop value for characterization mode.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble ForceStopValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets force set point value for production mode.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble ForceSetPoint { get; set; }

        /// <summary>
        /// Gets or sets VRange values. Required for HV pins.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString VRange { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets IRange values. Required for all pins.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString IRange { get; set; }

        /// <summary>
        /// Gets or sets IClampHi values. Required for all pins while using VSIM.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble IClampHi { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets IClampLo values. Required for all pins while using VSIM.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble IClampLo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets FreeDriveTime values. Required for all pins while using  VSIM.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble FreeDriveTime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets FreeDriveCurrentHi values. Required for VLC pins while using VSIM.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble FreeDriveCurrentHi { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets FreeDriveCurrentLo values. Required for VLC pins while using VSIM.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble FreeDriveCurrentLo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets VSlewStepRatio values. Required for HV pins while using VSIM characterization mode.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble VSlewStepRatio { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets VClamp values. Required for VLC pins while using ISVM.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble VClamp { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets OverVoltageLimit values. Required for LC. HC, HV pins while using ISVM.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble OverVoltageLimit { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets UnderVoltageLimit values. Required for LC. HC, HV pins while using ISVM.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble UnderVoltageLimit { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets SamplingRatio values. Required for all pins.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble SamplingRatio { get; set; }

        /// <summary>
        /// Gets or sets SamplingCount values. Required for all pins.
        /// </summary>
        public TestMethodsParams.CommaSeparatedInteger SamplingCount { get; set; }

        /// <summary>
        /// Gets or sets PreMeasurementDelay values. Required for all pins.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble PreMeasurementDelay { get; set; }

        /// <summary>
        /// Gets or sets force step size for characterization mode.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble ForceStepSize { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a list of comma separated names for SharedStorage tokens to store measurement results.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString SharedStorageTokens { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets execution mode.
        /// </summary>
        public Modes Mode { get; set; } = Modes.Production;

        /// <summary>
        /// Gets or sets AlarmMode; when is enabled alarms will be routed to different port.
        /// </summary>
        public AlarmModes AlarmMode { get; set; } = AlarmModes.Disabled;

        /// <inheritdoc />
        public override void Verify()
        {
            this.UpdateMeasurementType();
            this.SetDatalogLevel();
            this.ProcessParameters();
            this.PrepareRequiredPinAttributes();
            this.GetLevelDcTest();
            this.SetLimits();
        }

        /// <inheritdoc />
        [Returns(30, PortType.Fail, "Fail!")]
        [Returns(29, PortType.Fail, "Fail!")]
        [Returns(28, PortType.Fail, "Fail!")]
        [Returns(27, PortType.Fail, "Fail!")]
        [Returns(26, PortType.Fail, "Fail!")]
        [Returns(25, PortType.Fail, "Fail!")]
        [Returns(24, PortType.Fail, "Fail!")]
        [Returns(23, PortType.Fail, "Fail!")]
        [Returns(22, PortType.Fail, "Fail!")]
        [Returns(21, PortType.Fail, "Fail!")]
        [Returns(20, PortType.Fail, "Fail!")]
        [Returns(19, PortType.Fail, "Fail!")]
        [Returns(18, PortType.Fail, "Fail!")]
        [Returns(17, PortType.Fail, "Fail!")]
        [Returns(16, PortType.Fail, "Fail!")]
        [Returns(15, PortType.Fail, "Fail!")]
        [Returns(14, PortType.Fail, "Fail!")]
        [Returns(13, PortType.Fail, "Fail!")]
        [Returns(12, PortType.Fail, "Fail!")]
        [Returns(11, PortType.Fail, "Fail!")]
        [Returns(10, PortType.Fail, "Fail!")]
        [Returns(9, PortType.Fail, "Fail!")]
        [Returns(8, PortType.Fail, "Fail!")]
        [Returns(7, PortType.Fail, "Fail!")]
        [Returns(6, PortType.Fail, "Fail!")]
        [Returns(5, PortType.Fail, "Fail!")]
        [Returns(4, PortType.Fail, "Fail!")]
        [Returns(3, PortType.Fail, "Fail!")]
        [Returns(2, PortType.Fail, "Alarm!")]
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            var port = 1;
            try
            {
                this.loadLevels.ForceApply();
                for (var i = 0; i < this.pinNames.Count; i++)
                {
                    var restoreValues = this.GetRestoreValues(this.pinNames[i], this.loadLevels);
                    var forceAttributeName = this.measurementType[i] == Prime.DcService.MeasurementType.CURRENT
                        ? "VForce"
                        : "IForce";
                    var attributes = this.pinAttributes[this.pinNames[i]];
                    var setPointValue = this.forceSetPoint[i];
                    attributes[forceAttributeName] = setPointValue.ToString();
                    port = this.ExecutionSinglePinProduction(i, attributes, port);
                    this.ExecuteSinglePinCharacterization(i, attributes, forceAttributeName);
                    Prime.Services.PinService.SetPinAttributeValues(this.pinNames[i], restoreValues);
                }
            }
            catch (AlarmException exception)
            {
                Prime.Services.ConsoleService.PrintError(exception.GetAlarmMessage());
                if (this.AlarmMode == AlarmModes.Enabled)
                {
                    port = 2;
                }
                else
                {
                    throw;
                }
            }

            return port;
        }

        private Dictionary<string, string> GetRestoreValues(string pinName, ITestCondition testCondition)
        {
            var restorePinAttributes = new List<string>(this.pinAttributes[pinName].Keys.ToList());
            foreach (var a in this.invalidReadAttributes)
            {
                restorePinAttributes.Remove(a);
            }

            var restoreValues = new Dictionary<string, string>();
            foreach (var a in restorePinAttributes)
            {
                restoreValues[a] = testCondition.GetPinAttributeValue(pinName, a);
            }

            return restoreValues;
        }

        private void UpdateMeasurementType()
        {
            var types = this.Type.ToList();
            if (types.Count == 1)
            {
                types = new List<string>(Enumerable.Repeat(types.First(), this.Pins.ToList().Count).ToList());
            }

            this.measurementType = new List<MeasurementType>();
            foreach (var type in types)
            {
                switch (type.ToLower())
                {
                    case "current":
                        this.measurementType.Add(MeasurementType.CURRENT);
                        break;
                    case "voltage":
                        this.measurementType.Add(MeasurementType.VOLTAGE);
                        break;
                    default:
                        throw new ArgumentException($"Unable to convert {type} to {nameof(Prime.DcService.MeasurementType)}");
                }
            }
        }

        private void SetDatalogLevel()
        {
            this.datalogLevel = (this.DatalogLevel == DatalogLevels.All)
                ? Prime.DcService.DatalogLevel.ALL
                : Prime.DcService.DatalogLevel.FAIL_ONLY;
        }

        private void ProcessParameters()
        {
            switch (this.Mode)
            {
                case Modes.Production when string.IsNullOrEmpty(this.ForceSetPoint):
                    throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name} {nameof(this.ForceSetPoint)} must be set using {nameof(Modes.Production)}.");
                case Modes.Characterization when string.IsNullOrEmpty(this.ForceStepSize) || string.IsNullOrEmpty(this.ForceStartValue) || string.IsNullOrEmpty(this.ForceStopValue):
                    throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name} {nameof(this.ForceStartValue)}, {nameof(this.ForceStopValue)} and {nameof(this.ForceStepSize)} must be set using {nameof(Modes.Characterization)}.");
            }

            this.sharedStorageTokens = this.SharedStorageTokens.ToList();
            this.pinNames = this.Pins.ToList();
            this.forceStartValue = this.ForceStartValue.ToList();
            this.forceStopValue = this.ForceStopValue.ToList();
            this.forceStepSize = this.ForceStepSize.ToList();
            this.forceSetPoint = this.ForceSetPoint.ToList();
            this.vRange = this.VRange.ToList();
            this.iRange = this.IRange.ToList();
            this.iClampHi = this.IClampHi.ToList();
            this.iClampLo = this.IClampLo.ToList();
            this.freeDriveTime = this.FreeDriveTime.ToList();
            this.freeDriveCurrentHi = this.FreeDriveCurrentHi.ToList();
            this.freeDriveCurrentLo = this.FreeDriveCurrentLo.ToList();
            this.vSlewStepRatio = this.VSlewStepRatio.ToList();
            this.vClamp = this.VClamp.ToList();
            this.overVoltageLimit = this.OverVoltageLimit.ToList();
            this.underVoltageLimit = this.UnderVoltageLimit.ToList();
            this.samplingRatio = this.SamplingRatio.ToList();
            this.samplingCount = this.SamplingCount.ToList();
            this.preMeasurementDelay = this.PreMeasurementDelay.ToList();

            if (this.Mode == Modes.Characterization)
            {
                this.CheckParameterSize(nameof(this.ForceStartValue), this.forceStartValue.Count);
                this.CheckParameterSize(nameof(this.ForceStopValue), this.forceStopValue.Count);
                this.CheckParameterSize(nameof(this.ForceStepSize), this.forceStepSize.Count);
            }

            this.CheckParameterSize(nameof(this.SamplingRatio), this.samplingRatio.Count);
            this.CheckParameterSize(nameof(this.SamplingCount), this.samplingCount.Count);
            this.CheckParameterSize(nameof(this.PreMeasurementDelay), this.preMeasurementDelay.Count);
        }

        private void CheckParameterSize(string variableName, int size)
        {
            if (size != this.pinNames.Count)
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name} {variableName} must match number of elements in {nameof(this.Pins)}.");
            }
        }

        /* private Dictionary<string, Tuple<double, double>> PreparePerPinLimits()
        {
            var limitsPairs = this.lowLimits.Zip(this.highLimits, (a, b) => Tuple.Create(a, b));
            return this.pinNames.Zip(limitsPairs, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
        } */
        private DcSetup PreparePerPinLimits()
        {
            var types = this.Type.ToList();
            if (types.Count < this.pinNames.Count)
            {
                types = new List<string>(Enumerable.Repeat(types.First(), this.pinNames.Count).ToList());
            }

            var measurementTypes = new List<Prime.DcService.MeasurementType>(types.Count);
            foreach (var stringType in types)
            {
                measurementTypes.Add(stringType.ToLower() == "current" ? MeasurementType.CURRENT : MeasurementType.VOLTAGE);
            }

            var setup = DcCommon.PreparePerPinDcSetup(this.pinNames, ref this.lowLimits, ref this.highLimits, this.samplingCount, measurementTypes);
            return setup;
        }

        private void PrepareRequiredPinAttributes()
        {
            this.pinAttributes = new Dictionary<string, Dictionary<string, string>>();
            for (var i = 0; i < this.pinNames.Count; i++)
            {
                var pinType = Prime.Services.PinService.Get(this.pinNames[i]).GetResourceType();
                this.CheckParameterSize(nameof(this.PreMeasurementDelay), this.preMeasurementDelay.Count);
                this.CheckParameterSize(nameof(this.IRange), this.iRange.Count);

                var attributes = new Dictionary<string, string>
                {
                    ["IRange"] = this.iRange[i],
                    ["PreMeasurementDelay"] = this.preMeasurementDelay[i].ToString(),
                    ["StartMeasurement"] = "True",
                    ["SamplingRatio"] = "1",
                    ["SamplingMode"] = "Average",
                    ["SamplingCount"] = "1",
                };

                if (this.measurementType[i] == Prime.DcService.MeasurementType.CURRENT)
                {
                    attributes["OPModeCheck"] = "VSIM";
                    attributes["VForce"] = this.Mode == Modes.Production ? this.forceSetPoint[i].ToString() : this.forceStartValue[i].ToString();
                    this.CheckParameterSize(nameof(this.IClampHi), this.iClampHi.Count);
                    attributes["IClampHi"] = this.iClampHi[i].ToString();
                    this.CheckParameterSize(nameof(this.IClampLo), this.iClampLo.Count);
                    attributes["IClampLo"] = this.iClampLo[i].ToString();
                    this.CheckParameterSize(nameof(this.FreeDriveTime), this.freeDriveTime.Count);
                    attributes["FreeDriveTime"] = this.freeDriveTime[i].ToString();

                    if (pinType == "HC")
                    {
                        this.CheckParameterSize(nameof(this.VSlewStepRatio), this.vSlewStepRatio.Count);
                        attributes["VSlewStepRatio"] = this.vSlewStepRatio[i].ToString();
                    }
                    else if (pinType == "HV")
                    {
                        this.CheckParameterSize(nameof(this.VRange), this.vRange.Count);
                        attributes["VRange"] = this.vRange[i];
                        this.CheckParameterSize(nameof(this.VSlewStepRatio), this.vSlewStepRatio.Count);
                        attributes["VSlewStepRatio"] = this.vSlewStepRatio[i].ToString();
                    }
                    else if (pinType == "VLC")
                    {
                        this.CheckParameterSize(nameof(this.FreeDriveCurrentHi), this.freeDriveCurrentHi.Count);
                        attributes["FreeDriveCurrentHi"] = this.freeDriveCurrentHi[i].ToString();
                        this.CheckParameterSize(nameof(this.FreeDriveCurrentLo), this.freeDriveCurrentLo.Count);
                        attributes["FreeDriveCurrentLo"] = this.freeDriveCurrentLo[i].ToString();
                    }
                }
                else
                {
                    attributes["OPModeCheck"] = "ISVM";
                    attributes["IForce"] = this.Mode == Modes.Production ? this.forceSetPoint[i].ToString() : this.forceStartValue[i].ToString();
                    if (pinType == "VLC")
                    {
                        this.CheckParameterSize(nameof(this.VClamp), this.vClamp.Count);
                        attributes["VClamp"] = this.vClamp[i].ToString();
                    }
                    else
                    {
                        this.CheckParameterSize(nameof(this.OverVoltageLimit), this.overVoltageLimit.Count);
                        attributes["OverVoltageLimit"] = this.overVoltageLimit[i].ToString();
                        this.CheckParameterSize(nameof(this.UnderVoltageLimit), this.underVoltageLimit.Count);
                        attributes["UnderVoltageLimit"] = this.underVoltageLimit[i].ToString();
                    }
                }

                this.pinAttributes[this.pinNames[i]] = attributes;
            }
        }

        private void SetLimits()
        {
            var verifyResult = DcCommon.ResolveLimits(this.measurementType, this.LowLimits, this.HighLimits, out this.lowLimits, out this.highLimits);
            if (!verifyResult)
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Invalid limits selection.");
            }

            this.perPinLimits = this.PreparePerPinLimits();
        }

        private void GetLevelDcTest()
        {
            this.loadLevels = Prime.Services.TestConditionService.GetTestCondition(this.LevelsTc);
            this.levelDcTest = Prime.Services.DcService.GetDcTest(this.pinNames, this.measurementType);
        }

        private void ExecuteSinglePinCharacterization(int pinIndex, Dictionary<string, string> attributes, string forceAttributeName)
        {
            if (this.Mode != Modes.Characterization)
            {
                return;
            }

            var currentValue = this.forceStartValue[pinIndex];
            var counter = 0;
            while (currentValue <= this.ForceStopValue.ToList()[pinIndex] + double.Epsilon)
            {
                attributes[forceAttributeName] = currentValue.ToString();
                Prime.Services.PinService.SetPinAttributeValues(this.pinNames[pinIndex], attributes);
                var characterizationResults = this.levelDcTest.Execute();
                var characterizationPinGroupResults = characterizationResults.GetAllPinGroupsDcResults();
                foreach (var results in characterizationPinGroupResults
                    .SelectMany(singlePinGroupResult =>
                        from singlePinResult in singlePinGroupResult.GetAllPinsDcResults()
                        where this.pinNames[pinIndex] == singlePinResult.GetPinName()
                        select singlePinResult.GetPinDcResults()))
                {
                    this.DatalogCharacterizationMode(pinIndex, ref counter, currentValue, results);
                }

                currentValue += this.forceStepSize[pinIndex];
            }
        }

        private void DatalogCharacterizationMode(int pinIndex, ref int counter, double currentValue, List<double> results)
        {
            // var ituff = $"2_tname_{this.InstanceName}_{this.pinNames[pinIndex]}_{counter}\n2_fvalue_{currentValue}\n2_mrslt_{results.First()}\n";
            var mrsltFormatter = Prime.Services.DatalogService.GetItuffMrsltWriter();
            mrsltFormatter.SetTnamePostfix($"_{this.pinNames[pinIndex]}_{counter}");
            mrsltFormatter.SetPrecision(9);
            mrsltFormatter.SetData(results.First());

            var comntFormatter = Prime.Services.DatalogService.GetItuffComntWriter();
            comntFormatter.IncludeTnameInPrint(false);
            comntFormatter.SetData($"fvalue_{currentValue:F9}");

            Prime.Services.DatalogService.WriteToItuff(mrsltFormatter);
            Prime.Services.DatalogService.WriteToItuff(comntFormatter);

            counter++;
        }

        private int ExecutionSinglePinProduction(int pinIndex, Dictionary<string, string> attributes, int port)
        {
            Prime.Services.PinService.SetPinAttributeValues(this.pinNames[pinIndex], attributes);
            var productionResults = this.levelDcTest.Execute();
            if (this.LogLevel != PrimeLogLevel.DISABLED)
            {
                productionResults.PrintToConsole();
            }

            // productionResults.PrintToDatalog(this.datalogLevel, this.perPinLimits);
            productionResults.PrintToDatalog(this.datalogLevel, this.perPinLimits);

            var allPinGroupsResults = productionResults.GetAllPinGroupsDcResults();
            foreach (var results in allPinGroupsResults
                .SelectMany(singlePinGroupResult =>
                    from singlePinResult in singlePinGroupResult.GetAllPinsDcResults()
                    where this.pinNames[pinIndex] == singlePinResult.GetPinName()
                    select singlePinResult.GetPinDcResults()))
            {
                this.StoreResults(pinIndex, results);
                var failedTest = results.Any(result => result <= this.lowLimits[pinIndex] || result >= this.highLimits[pinIndex]);
                port = this.GetPort(pinIndex, port, failedTest);
            }

            return port;
        }

        private void StoreResults(int pinIndex, List<double> results)
        {
            if (this.sharedStorageTokens.Count != this.pinNames.Count)
            {
                return;
            }

            var concatenatedValues = results.Aggregate(string.Empty, (current, values) => current + string.Join(",", results));
            Prime.Services.SharedStorageService.InsertRowAtTable(this.sharedStorageTokens[pinIndex], concatenatedValues, Context.DUT);
        }

        private int GetPort(int pinIndex, int port, bool failedTest)
        {
            if (port == 1)
            {
                if (failedTest && pinIndex <= NumberOfPorts - 2)
                {
                    port = pinIndex + 3;
                }
                else if (failedTest)
                {
                    port = 0; // overflow; ran out of ports.
                }
            }
            else
            {
                port = 0; // failed more than one pin.
            }

            return port;
        }
    }
}