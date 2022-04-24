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

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Prime.TestMethods.Shmoo.UnitTest")]

namespace Prime.TestMethods.Shmoo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Prime.Base.Exceptions;
    using Prime.FunctionalService;
    using Prime.Kernel.TestMethodsExtension;
    using Prime.PhAttributes;
    using Prime.TestConditionService;

    /// <summary>
    /// Test method responsible for executing Shmoo.
    /// </summary>
    [PrimeTestMethod]
    public partial class PrimeShmooTestMethod : TestMethodBase, IExtendableTestMethod<IShmooExtensions, PrimeShmooTestMethod>, IShmooExtensions
    {
        // functional test object to be used in run-time to execute plist
        private IFunctionalTest functionalTestForShmooExecute;

        // Test method default post point processing can be used only if default functional test used.
        private bool isDefaultFuncTestUsed;

        // X and Y axis defined...
        private bool isXAxisDefined;
        private bool isYAxisDefined;

        /// <summary>
        /// Enum for X/Y IShmooAxis Type parameters.
        /// </summary>
        public enum ShmooAxisType
        {
            /// <summary>
            /// ShmooAxis is unused.
            /// </summary>
            None,

            /// <summary>
            /// ShmooAxis is a Prime LevelsTestCondtion Type.
            /// </summary>
            LevelsTestCondition,

            /// <summary>
            /// ShmooAxis is a Prime LevelsTestCondtion Type.
            /// </summary>
            TimingTestCondition,

            /// <summary>
            /// ShmooAxis is a list of Prime PatConfigs.
            /// </summary>
            PatConfig,

            /// <summary>
            /// ShmooAxis is a list of Prime PatConfigSetPoints.
            /// </summary>
            PatConfigSetPoint,

            /// <summary>
            /// ShmooAxis is user defined. By default shmoo execution functions will do nothing. Functions can be overwritten using extensions.
            /// </summary>
            UserDefined,
        }

        /// <summary>
        /// Scale for a given axis.
        /// </summary>
        public enum UnitPrefixForDatalog
        {
            /// <summary>
            /// Axis should be printed to datalog in base units.
            /// </summary>
            Base,

            /// <summary>
            /// Axis should be printed to datalog in milli units.
            /// </summary>
            Milli,

            /// <summary>
            /// Axis should be printed to datalog in micro units.
            /// </summary>
            Micro,

            /// <summary>
            /// Axis should be printed to datalog in nano units.
            /// </summary>
            Nano,
        }

        /// <summary>
        /// Enum for format to print to ituff. Currently, only ShmooHub is supported.
        /// </summary>
        public enum ShmooPrintFormat
        {
            /// <summary>
            /// Shmoohub print in format:  .
            /// </summary>
            ShmooHub,

            /// <summary>
            /// ECADS print in format:  .
            /// </summary>
            ECADS,

            /// <summary>
            /// ARIES print in format:  .
            /// </summary>
            ARIES,
        }

        /// <summary>
        /// Settings to enable or disable PowerDownBetweenPoints.
        /// </summary>
        public enum PowerDownBetweenPointsSettings
        {
            /// <summary>
            /// Enable power down between points.
            /// </summary>
            ENABLED,

            /// <summary>
            /// Disable power down between points.
            /// </summary>
            DISABLED,
        }

        /// <summary>
        /// Shmoo plot mode.
        /// </summary>
        public enum ShmooPlotMode
        {
            /// <summary>
            /// Normal mode where symbol and key in the plot is automatically chosen, but user control the legend fail string.
            /// </summary>
            NORMAL,

            /// <summary>
            /// Cusom mode where user control all the plot attributes, symbol key and legend fail string.
            /// </summary>
            CUSTOM,
        }

        /// <summary>
        /// Gets or sets Patlist to execute.
        /// </summary>
        public TestMethodsParams.Plist Patlist { get; set; }

        /// <summary>
        /// Gets or sets TimingsTc for plist execution.
        /// </summary>
        public TestMethodsParams.TimingCondition TimingsTc { get; set; }

        /// <summary>
        /// Gets or sets LevelsTc to plist execution.
        /// </summary>
        public TestMethodsParams.LevelsCondition LevelsTc { get; set; }

        /// <summary>
        /// Gets or sets the PrePlist callback to plist execution.
        /// </summary>
        public TestMethodsParams.String PrePlist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets comma separated pins for mask.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString MaskPins { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the XAxisParameter. Empty by default.
        /// </summary>
        public TestMethodsParams.String XAxisParam { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the XAxisParamType. None by Default.
        /// </summary>
        public ShmooAxisType XAxisParamType { get; set; } = ShmooAxisType.None;

        /// <summary>
        /// Gets or sets the unit prefix for scaling the X axis. (Base, Milli, Micro, Nano).
        /// </summary>
        public UnitPrefixForDatalog XAxisDatalogPrefix { get; set; } = UnitPrefixForDatalog.Base;

        /// <summary>
        /// Gets or sets the name used during datalog prints for X axis. Uses XAxisParam by default.
        /// </summary>
        public TestMethodsParams.String XAxisDatalogName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the XAxisRange. Default format: "Start: Resolution: NumberOfPoints".
        /// </summary>
        public TestMethodsParams.String XAxisRange { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the YAxisParameter. Empty by default.
        /// </summary>
        public TestMethodsParams.String YAxisParam { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the YAxisParamType. None by default.
        /// </summary>
        public ShmooAxisType YAxisParamType { get; set; } = ShmooAxisType.None;

        /// <summary>
        /// Gets or sets the YAxisRange. Default format: "Start: Resolution: NumberOfPoints".
        /// </summary>
        public TestMethodsParams.String YAxisRange { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unit prefix for scaling the Y axis. (Base, Milli, Micro, Nano).
        /// </summary>
        public UnitPrefixForDatalog YAxisDatalogPrefix { get; set; } = UnitPrefixForDatalog.Base;

        /// <summary>
        /// Gets or sets the name used during datalog prints for Y Axis. Uses YAxisParam by default.
        /// </summary>
        public TestMethodsParams.String YAxisDatalogName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the RegionalKillLimits. Format is: "XMin, XMax, YMin, YMax".
        /// </summary>
        public TestMethodsParams.String RegionalKillLimits { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the print format for shmoo ituffs.
        /// </summary>
        public ShmooPrintFormat PrintFormat { get; set; } = ShmooPrintFormat.ShmooHub;

        /// <summary>
        ///  Gets or sets power_down_between_point. This will trigger EndSequence before ApplyTestCondition in PrePointExecute.
        /// </summary>
        public PowerDownBetweenPointsSettings PowerDownBetweenPoints { get; set; } = PowerDownBetweenPointsSettings.DISABLED;

        /// <summary>
        /// Gets or sets PlotMode.
        /// </summary>
        public ShmooPlotMode PlotMode { get; set; } = ShmooPlotMode.NORMAL;

        /// <summary>
        /// Gets or sets ExtendedFunctions.
        /// </summary>
        public TestMethodsParams.String IfeObject { get; set; } = string.Empty;

        /// <inheritdoc cref="IShmooExtensions" />
        public IShmooExtensions TestMethodExtension { get; set; }

        /// <summary>
        /// Gets or sets the verify data structure.
        /// These are all the TM members that are populated by VerifyOffline grouped together.
        /// Making it internal to provide access to UTs.
        /// </summary>
        internal VerifyDataStructure VerifyDataStructure { get; set; } = new VerifyDataStructure();

        /// <inheritdoc />
        public override sealed void Verify()
        {
            // replace the IOfflineReady calls
            this.VerifyDataStructure = new VerifyDataStructure();
            this.VerifyMaskPins();
            this.SetupPlotLegend();
            this.VerifyDataStructure.ShmooPlot = this.GenerateShmooPlot();
            this.VerifyDataStructure.Printer = this.GetPrinter();
            this.VerifyDataStructure.RegionalKillLimits = this.GetRegionalKillLimits(this.RegionalKillLimits);
            /* end IOfflineReady calls */

            this.ClearVerifyRuntimeData();
            this.SetupFunctionalTest();
            this.VerifyDataStructure.ShmooPlot.Verify();
        }

        /* /// <inheritdoc/>
        public void VerifyOffline(ref object instanceInitDataStructure)
        {
            this.VerifyMaskPins();
            this.SetupPlotLegend();
            this.VerifyDataStructure.ShmooPlot = this.GenerateShmooPlot();
            this.VerifyDataStructure.Printer = this.GetPrinter();
            this.VerifyDataStructure.RegionalKillLimits = this.GetRegionalKillLimits(this.RegionalKillLimits);

            instanceInitDataStructure = this.VerifyDataStructure;
        }

        /// <inheritdoc/>
        public void Init(object instanceDataStructure)
        {
            this.VerifyDataStructure = (VerifyDataStructure)instanceDataStructure;
        } */

        /// <inheritdoc />
        [Returns(3, PortType.Pass, "PASS PORT")]
        [Returns(2, PortType.Fail, "FAIL PORT")]
        [Returns(1, PortType.Pass, "PASS PORT")]
        [Returns(0, PortType.Fail, "FAIL PORT")]
        public override int Execute()
        {
            this.ClearExecuteRuntimeData();
            this.TestMethodExtension.PreExecute(this.functionalTestForShmooExecute);

            bool failureFound = false;
            bool regionalFailureFound = false;

            foreach (var point in this.VerifyDataStructure.ShmooPlot.ShmooLoopPoints)
            {
                if (this.PowerDownBetweenPoints == PowerDownBetweenPointsSettings.ENABLED)
                {
                    Prime.Services.TestConditionService.ApplyEndSequence();
                }

                if (!this.TestMethodExtension.PrePointExecute(point, this.functionalTestForShmooExecute))
                {
                    if (this.PlotMode == ShmooPlotMode.NORMAL)
                    {
                        this.GetPlotLegend().FlagSkippedPoint(point);
                    }

                    continue;
                }

                this.SetPinMask();

                if (!this.TestMethodExtension.PointExecute(point))
                {
                    regionalFailureFound = this.IsPointWithinKillLimits(point) || regionalFailureFound;
                    failureFound = true;
                }

                this.TestMethodExtension.PostPointExecute(point, this.functionalTestForShmooExecute);
            }

            this.VerifyDataStructure.Printer.PrintPlotToItuff(this.VerifyDataStructure.PlotLegend, this.VerifyDataStructure.ShmooPlot);

            this.TestMethodExtension.PostExecute(this.functionalTestForShmooExecute);
            var exitPort = failureFound ? 0 : 1;
            if (this.VerifyDataStructure.RegionalKillLimits.AreLimitsUsed)
            {
                exitPort = regionalFailureFound ? 2 : 3;
            }

            return exitPort;
        }

        /// <summary>
        /// Returns normal plot legend.
        /// </summary>
        /// <returns>normal plot legend.</returns>
        public PlotLegend GetPlotLegend()
        {
            if (this.PlotMode != ShmooPlotMode.NORMAL)
            {
                throw new TestMethodException("Cannot use GetPlotLegend(...) when 'PlotMode' parameter is not 'NORMAL'.");
            }

            return (PlotLegend)this.VerifyDataStructure.PlotLegend;
        }

        /// <summary>
        /// Returns custom plot legend.
        /// </summary>
        /// <returns>custom plot legend.</returns>
        public PlotLegendCustom GetCustomPlotLegend()
        {
            if (this.PlotMode != ShmooPlotMode.CUSTOM)
            {
                throw new TestMethodException("Cannot use GetNormalPlotLegend(...) when 'PlotMode' parameter is not 'CUSTOM'.");
            }

            return (PlotLegendCustom)this.VerifyDataStructure.PlotLegend;
        }

        /// <summary>
        /// This is a default implementation of the function. User can choose to override it.
        /// Expected format-> startPoint:resolution:numOfPoints. Each value is a double.
        /// </summary>
        /// <param name="pointValue">String in this format: startPoint:resolution:numOfPoints. </param>
        /// <returns>List of double for X points.</returns>
        List<double> IShmooExtensions.GetXAxisPoints(string pointValue)
        {
            var points = this.ParsePointsRange(pointValue, out var xAxisStringPoints);
            this.VerifyDataStructure.XAxisStringPoints = xAxisStringPoints;
            return points;
        }

        /// <summary>
        /// This is a default implementation of the function. User can choose to override it.
        /// Expected format-> startPoint:resolution:numOfPoints. Each value is a double.
        /// </summary>
        /// <param name="pointValue">String in this format: startPoint:resolution:numOfPoints. </param>
        /// <returns>List of double for Y points.</returns>
        List<double> IShmooExtensions.GetYAxisPoints(string pointValue)
        {
            var points = this.ParsePointsRange(pointValue, out var yAxisStringPoints);
            this.VerifyDataStructure.YAxisStringPoints = yAxisStringPoints;
            return points;
        }

        /// <summary>
        /// This is a default implementation of the function. User can choose to override it.
        /// In this implementation DFM + first fail per plist settings is created.
        /// </summary>
        /// <param name="patlist">plist.</param>
        /// <param name="levelsTc">levels.</param>
        /// <param name="timingsTc">timings.</param>
        /// <param name="prePlist">The callback to run before the plist execution.</param>
        /// <returns>The created IFunctionalTest.</returns>
        IFunctionalTest IShmooExtensions.GetFunctionalTest(string patlist, string levelsTc, string timingsTc, string prePlist)
        {
            this.isDefaultFuncTestUsed = true;
            return Prime.Services.FunctionalService.CreateCaptureFailureTest(patlist, levelsTc, timingsTc, 1, prePlist);
        }

        /// <summary>
        /// This is a default implementation of the function. User can choose to override it.
        /// In this default implementation, we set the TC spec value of X and Y if they are defined.
        /// </summary>
        /// <param name="point">current point.</param>
        /// <param name="functionalTest">funcTest of the executed plist.</param>
        /// <returns>false if this point should be skipped, true otherwise.</returns>
        bool IShmooExtensions.PrePointExecute(ShmooPoint point, IFunctionalTest functionalTest)
        {
            return this.VerifyDataStructure.ShmooPlot.PrePointExecute(point, functionalTest);
        }

        /// <summary>
        /// This is a default implementation of the function. User can choose to override it.
        /// </summary>
        /// <param name="functionalTest">funcTest of the executed plist.</param>
        void IShmooExtensions.PreExecute(IFunctionalTest functionalTest)
        {
            this.VerifyDataStructure.ShmooPlot.PreExecute();
        }

        /// <summary>
        /// This is a default implementation of the function. User can choose to override it.
        /// </summary>
        /// <param name="functionalTest">funcTest of the executed plist.</param>
        void IShmooExtensions.PostExecute(IFunctionalTest functionalTest)
        {
            this.VerifyDataStructure.ShmooPlot.PostExecute();
        }

        /// <summary>
        /// Set the list of pins that will be masked when plist is executed.
        /// </summary>
        public void SetPinMask()
        {
            List<string> pinMasks = this.TestMethodExtension.GetDynamicPinMask();
            if (this.VerifyDataStructure.MaskPins != null && this.VerifyDataStructure.MaskPins.Count != 0)
            {
                pinMasks = pinMasks.Union(this.VerifyDataStructure.MaskPins).ToList();
            }

            this.functionalTestForShmooExecute.SetPinMask(pinMasks);
        }

        /// <summary>
        /// This is default implementation of plist execute. User can choose to override it.
        /// </summary>
        /// <param name="point">Current shmoo point.</param>
        /// <returns>Return a true if plist execute pass, false otherwise.</returns>
        bool IShmooExtensions.PointExecute(ShmooPoint point)
        {
            return this.functionalTestForShmooExecute.Execute();
        }

        /// <summary>
        /// This is a default implementation of the function. User can choose to override it.
        /// It returns false if multiple defects captured for point. true otherwise.
        /// </summary>
        /// <param name="point">current point.</param>
        /// <param name="functionalTest">functionalTest of the executed plist.</param>
        void IShmooExtensions.PostPointExecute(ShmooPoint point, IFunctionalTest functionalTest)
        {
            if (!this.isDefaultFuncTestUsed)
            {
                throw new TestMethodException($"User should implement extension method PostPointExecute() when GetFunctionalTest() is extended.");
            }

            ICaptureFailureTest dfmFuncTest = (ICaptureFailureTest)functionalTest;
            List<IFailureData> fails = dfmFuncTest.GetPerCycleFailures();

            if (fails.Count != 0)
            {
                //////////////////////////////////////
                // Fail format is dependent on type of printer being used
                //////////////////////////////////////
                var failString = this.VerifyDataStructure.Printer.GetFailString(fails);
                this.GetPlotLegend().AddData(point, failString);
            }
        }

        /// <inheritdoc />
        List<string> IShmooExtensions.GetDynamicPinMask()
        {
            ///////////////////////////////////
            //  To be overridden by user code
            ///////////////////////////////////
            return new List<string>();
        }

        /// <summary>
        /// function to set plot legend based on the parameter.
        /// </summary>
        internal void SetupPlotLegend()
        {
            if (this.PlotMode == ShmooPlotMode.NORMAL)
            {
                this.VerifyDataStructure.PlotLegend = new PlotLegend();
            }
            else
            {
                this.VerifyDataStructure.PlotLegend = new PlotLegendCustom();
            }
        }

        /// <summary>
        /// Creates a new ShmooPlot object.
        /// </summary>
        /// <returns>new Shmoo Plot object.</returns>
        internal ShmooPlot GenerateShmooPlot()
        {
            this.isXAxisDefined = !string.IsNullOrEmpty(this.XAxisParam);
            this.isYAxisDefined = !string.IsNullOrEmpty(this.YAxisParam);

            var xAxis = this.GetShmooAxisCategory(this.XAxisParam, this.XAxisParamType, this.XAxisDatalogPrefix, "X", this.XAxisDatalogName);
            var yAxis = this.GetShmooAxisCategory(this.YAxisParam, this.YAxisParamType, this.YAxisDatalogPrefix, "Y", this.YAxisDatalogName);

            var xAxisPoints = this.TestMethodExtension.GetXAxisPoints(this.XAxisRange);
            var yAxisPoints = this.TestMethodExtension.GetYAxisPoints(this.YAxisRange);

            var xAxisData = new ShmooPlot.AxisData(xAxis, xAxisPoints, this.VerifyDataStructure.XAxisStringPoints);
            var yAxisData = new ShmooPlot.AxisData(yAxis, yAxisPoints, this.VerifyDataStructure.YAxisStringPoints);

            return new ShmooPlot(
                xAxisData,
                yAxisData);
        }

        private ShmooPrinter GetPrinter()
        {
            switch (this.PrintFormat)
            {
                case ShmooPrintFormat.ShmooHub:
                    return new ShmooHubPrinter();
                case ShmooPrintFormat.ECADS:
                    return new EcadsPrinter(this.InstanceName);
                case ShmooPrintFormat.ARIES:
                    return new AriesPrinter();
                default:
                    throw new TestMethodException(
                        "Unsupported Print format was selected");
            }
        }

        // This function only returns test condition type Shmoo axis.
        private IShmooAxis GetShmooAxisCategory(string axisParameter, ShmooAxisType axisType, UnitPrefixForDatalog unitPrefixForDatalog, string axisName, string axisDatalogName)
        {
            if (string.IsNullOrEmpty(axisDatalogName))
            {
                axisDatalogName = axisParameter;
            }

            switch (axisType)
            {
                case ShmooAxisType.UserDefined:
                    return new UserDefinedAxis(axisParameter, unitPrefixForDatalog, axisDatalogName);
                case ShmooAxisType.LevelsTestCondition:
                    return new LevelAxis(axisParameter, this.LevelsTc, unitPrefixForDatalog, axisDatalogName);
                case ShmooAxisType.TimingTestCondition:
                    return new TimingAxis(axisParameter, this.TimingsTc, unitPrefixForDatalog, axisDatalogName);
                case ShmooAxisType.PatConfig:
                    return new PatConfigAxis(axisParameter, this.Patlist, axisDatalogName);
                case ShmooAxisType.PatConfigSetPoint:
                    return new PatConfigSetPointAxis(axisParameter, this.Patlist, axisDatalogName);
                case ShmooAxisType.None:
                    return new EmptyShmooAxis();
                default:
                    throw new TestMethodException(
                        $"{axisName}Axis type was not found");
            }
        }

        private List<double> ParsePointsRange(string pointsRangeExpression, out List<string> axisStringPoints)
        {
            axisStringPoints = new List<string>();
            var points = new List<double>();

            if (!string.IsNullOrEmpty(pointsRangeExpression))
            {
                if (pointsRangeExpression.Contains(',') || pointsRangeExpression.Count(c => c == ':') != 2)
                {
                    axisStringPoints = pointsRangeExpression.Split(',').Select(o => o.Trim()).ToList();
                    points = Enumerable.Range(0, axisStringPoints.Count).Select(i => (double)i).ToList();
                }
                else
                {
                    points = this.ParsePointsAsDoubles(pointsRangeExpression);
                }
            }

            return points;
        }

        private List<double> ParsePointsAsDoubles(string pointsRangeExpression)
        {
            // Tokenize the points range as "START_POINT:RESOLUTION:NUMBER_OF_POINTS".
            ExtractedRangeFields rangeFields = this.ExtractRangeFields(pointsRangeExpression);

            // conversion to double guaranteed by verify
            return this.GeneratePointsSequence(
                Convert.ToDouble(rangeFields.Start),
                Convert.ToDouble(rangeFields.Resolution),
                Convert.ToDouble(rangeFields.NumberOfPoints));
        }

        // Verifies mask pins
        private void VerifyMaskPins()
        {
            this.VerifyDataStructure.MaskPins = this.MaskPins;
            foreach (string pinName in this.VerifyDataStructure.MaskPins)
            {
                if (!Services.PinService.Exists(pinName))
                {
                    throw new TestMethodException($"Mask pin=[{pinName}] does not exist.\n");
                }
            }
        }

        private void ClearVerifyRuntimeData()
        {
            this.isDefaultFuncTestUsed = false;
            this.functionalTestForShmooExecute = null;
        }

        private void ClearExecuteRuntimeData()
        {
            this.VerifyDataStructure.PlotLegend.Reset();
            this.functionalTestForShmooExecute.ApplyTestConditions();
        }

        // function used internally to generate points sequence.
        private List<double> GeneratePointsSequence(double start, double resolution, double numberOfPoints)
        {
            List<double> points = new List<double>();
            for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
            {
                points.Add(start + (pointIndex * resolution));
            }

            return points;
        }

        // function used internally to extract tokens from x and y range expressions given in the TM parameters.
        private ExtractedRangeFields ExtractRangeFields(TestMethodsParams.String xAxisRange)
        {
            var r = new Regex(@"(.+):(.+):(.+)", RegexOptions.IgnoreCase);
            var match = r.Match(xAxisRange);
            if (match.Success)
            {
                ExtractedRangeFields fieldsValues = default;
                fieldsValues.Start = match.Groups[1].Value;
                fieldsValues.Resolution = match.Groups[2].Value;
                fieldsValues.NumberOfPoints = match.Groups[3].Value;
                return fieldsValues;
            }

            // ERROR
            return default;
        }

        // function used internally to setup functional test.
        private void SetupFunctionalTest()
        {
            this.functionalTestForShmooExecute =
                    this.TestMethodExtension.GetFunctionalTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.PrePlist);
        }

        private bool IsPointWithinKillLimits(ShmooPoint point)
        {
            var insideLimits = true;
            if (this.isXAxisDefined)
            {
                insideLimits = this.IsValueWithinLimits(
                    point.XValue,
                    this.VerifyDataStructure.RegionalKillLimits.XMin,
                    this.VerifyDataStructure.RegionalKillLimits.XMax);
            }

            if (this.isYAxisDefined)
            {
                insideLimits = insideLimits && this.IsValueWithinLimits(
                    point.YValue,
                    this.VerifyDataStructure.RegionalKillLimits.YMin,
                    this.VerifyDataStructure.RegionalKillLimits.YMax);
            }

            return insideLimits;
        }

        private bool IsValueWithinLimits(double value, double min, double max)
        {
            return !double.IsNaN(value) && (value >= min - ShmooConstants.DoubleTolerance && value <= max + ShmooConstants.DoubleTolerance);
        }

        private KillLimits GetRegionalKillLimits(string regionalKillLimits)
        {
            var limits = default(KillLimits);

            if (!string.IsNullOrEmpty(regionalKillLimits))
            {
                var splitLimits = regionalKillLimits.Split(',');
                if (this.isXAxisDefined && this.isYAxisDefined && splitLimits.Length != 4)
                {
                    throw new TestMethodException("2D shmoo was used, but the number of regionalKills provided wasn't 4. Please provide 4 comma separated doubles in format of: XMin, XMax, YMin, YMax");
                }

                if (this.isXAxisDefined && !this.isYAxisDefined && splitLimits.Length != 2)
                {
                    throw new TestMethodException("1D shmoo was used, but the number of regionalKills provided wasn't 2. Please provide 2 comma separated doubles in format of: XMin, XMax");
                }

                try
                {
                    var limitList = splitLimits.Select(double.Parse).ToList();
                    if (this.isXAxisDefined)
                    {
                        limits.XMin = limitList.ElementAt(0);
                        limits.XMax = limitList.ElementAt(1);
                    }

                    if (this.isYAxisDefined)
                    {
                        limits.YMin = limitList.ElementAt(2);
                        limits.YMax = limitList.ElementAt(3);
                    }

                    limits.AreLimitsUsed = true;
                }
                catch (Exception e)
                {
                    throw new TestMethodException($"Parse of regional kill limits failed with exception {e}.  XMin, XMax for 1D shmoo or XMin, XMax, YMin, YMax for 2D shmoo. ");
                }
            }

            return limits;
        }

        /// <summary>
        /// ExtractedRangedFields.
        /// </summary>
        public struct ExtractedRangeFields
        {
            /// <summary>
            /// Gets or sets Start.
            /// </summary>
            public string Start { get; set; }

            /// <summary>
            /// Gets or sets NumberOfPoints.
            /// </summary>
            public string NumberOfPoints { get; set; }

            /// <summary>
            /// Gets or sets Resolution.
            /// </summary>
            public string Resolution { get; set; }
        }

        /// <summary>
        /// RegionalKillLimits.
        /// </summary>
        [Serializable]
        public struct KillLimits
        {
            /// <summary>
            /// Gets or sets a value indicating whether limits are used.
            /// </summary>
            public bool AreLimitsUsed { get; set; }

            /// <summary>
            /// Gets or sets XMin.
            /// </summary>
            public double XMin { get; set; }

            /// <summary>
            /// Gets or sets XMax.
            /// </summary>
            public double XMax { get; set; }

            /// <summary>
            /// Gets or sets YMin.
            /// </summary>
            public double YMin { get; set; }

            /// <summary>
            /// Gets or sets YMax.
            /// </summary>
            public double YMax { get; set; }
        }
    }
}