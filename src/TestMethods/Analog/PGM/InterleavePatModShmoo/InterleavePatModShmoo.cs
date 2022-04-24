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

namespace InterleavePatModShmoo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PhAttributes;
    using Prime.TestConditionService;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class InterleavePatModShmoo : TestMethodBase
    {
        // functional test object to be used in run-time to execute plist
        private IFunctionalTest functionalTest = null;

        // x and y parameter test condition to be used in run-time
        private ITestCondition xParamTC = null;
        private ITestCondition yParamTC = null;

        // x and y parameter tc type
        private TestConditionType xParamTcType = TestConditionType.ALL;
        private TestConditionType yParamTcType = TestConditionType.ALL;

        // x and y original specSet value
        private string originalXAxisParamValue = null;
        private string originalYAxisParamValue = null;

        // x and y points after calculation - ready to be consumed
        private List<double> xPoints = new List<double>();
        private List<double> yPoints = new List<double>();

        // All Shmoo in one ShmooHub format.
        private string shmooHubItuffData = string.Empty;

        // all shmoo points arranged in one list. At execute we iterate over this list to create shmoo.
        private List<ShmooPoint> shmooLoopPoints = new List<ShmooPoint>();

        // Each 'char' value should have a matchig entry on "plotLegend". Otherwise fail on fatal eror.
        private Dictionary<ShmooPoint, char> plotData = new Dictionary<ShmooPoint, char>();

        // Plots legend
        private PlotLegend plotLegend = new PlotLegend();

        // Test method default behavior expects user to provide x/y parameters of TestCondition type.
        // sooi19: Remove this usage to only allow TCParam to be used
        // private bool isTCParamsOnlyUsed = true;

        // Test method default post point processing can be used only if default functional test used.
        private bool isDefaultFuncTestUsed = false;

        // Store module:group specified in ConfigList Parameter
        private List<string> moduleGroupList = new List<string>();

        // sooi19: Determine order of y axis(ascend or descend mode) for correct console/ituff printing
        private bool yaxisAscendOrder = true;

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
        /// Gets or sets the XAxisParameter. By default only TC specs are supported (levels/timings).
        /// </summary>
        public TestMethodsParams.String XAxisParam { get; set; }

        /// <summary>
        /// Gets or sets the XAxisRange. Default format: "Start: Resolution: NumberOfPoints".
        /// </summary>
        public TestMethodsParams.String XAxisRange { get; set; }

        /// <summary>
        /// Gets or sets the YAxisParameter. By default only TC specs are supported (levels/timings).
        /// </summary>
        public TestMethodsParams.String YAxisParam { get; set; }

        /// <summary>
        /// Gets or sets the YAxisRange. Default format: "Start: Resolution: NumberOfPoints".
        /// </summary>
        public TestMethodsParams.String YAxisRange { get; set; }

        /// <summary>
        /// Gets or sets ExtendedFunctions.
        /// </summary>
        public TestMethodsParams.String IfeObject { get; set; }

        /// <summary>
        /// Gets or Sets ConfigList in format of Module:Group.
        /// </summary>
        public TestMethodsParams.String ConfigList { get; set; }

        /// <summary>
        /// Gets or Sets ConfigSetPoints to execute in format setpoint1,setpoint2,setpoint n...
        /// </summary>
        public TestMethodsParams.CommaSeparatedString ConfigSetPoints { get; set; }

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;
            this.ClearRuntimeData();
            this.SetupFunctionalTest();
            this.GetTcParams();
            this.SaveOriginalTcValue();
            this.CalculateAxisPoints();
            this.CreatePointsLoopList();
            this.VerifyConfigList(this.ConfigList);
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "PASS PORT")]
        [Returns(0, PortType.Fail, "FAIL PORT")]
        public override int Execute()
        {
            // the .json file of pat config on Shmoo Module inputs folder.
            IPatConfigSetPointHandle configSetPointHandle = Prime.Services.PatConfigService.GetSetPointHandle(this.moduleGroupList[0], this.moduleGroupList[1]); // get the pat config handle.
            foreach (string configSetPoint in this.ConfigSetPoints.ToList())
            {
                this.plotData.Clear();
                this.plotLegend = new PlotLegend();
                configSetPointHandle.ApplySetPoint(configSetPoint); // write the data to pattern.
                this.Console?.PrintDebug("Applying setpoint=[" + configSetPoint + "]\n");

                string tmpPointX = string.Empty;
                string tmpPointY = string.Empty;
                foreach (var point in this.shmooLoopPoints)
                {
                    if (this.PrePointExecute(point) == false)
                    {
                        this.plotData.Add(point, (char)PlotLegend.ReservedSymbols.SKIP);
                        continue;
                    }

                    if (tmpPointX != point.XValue.ToString())
                    {
                        this.xParamTC.ForceApply();
                        this.Console?.PrintDebug("Shmoo setting x-axis spec value=[" + point.XValue + "]");
                    }

                    if (tmpPointY != point.YValue.ToString())
                    {
                        this.yParamTC.ForceApply();
                        this.Console?.PrintDebug("Shmoo setting y-axis spec value=[" + point.YValue + "]");
                    }

                    // this.functionalTest.ApplyTestConditions(); // in case test conditions changed by PrePointExecute().
                    this.functionalTest.Execute();
                    tmpPointX = point.XValue.ToString();
                    tmpPointY = point.YValue.ToString();

                    if (this.PostPointExecute(point, this.functionalTest, out var failData))
                    {
                        this.plotData.Add(point, (char)PlotLegend.ReservedSymbols.PASS);
                    }
                    else
                    {
                        this.plotData.Add(point, this.plotLegend.AddFailInfoAndGetLegendSymbol(failData));
                    }
                }

                // clear shmoohubItuffData before every repetitive shmoo
                this.shmooHubItuffData = string.Empty;
                this.PrintPlotToConsole();
                this.PrintPlotToItuff(configSetPoint);
            }

            // patmod to default state for HVM execution
            configSetPointHandle.ApplySetPointDefault();
            this.Console?.PrintDebug("Successfully apply default set point.\n");

            // restore original x and y tcparam value
            this.RestoreDefaultTcValue();
            return 1;
        }

        /// <summary>
        /// This is a default implementation of the function. User can choose to override it.
        /// Expected format-> startPoint:resolution:numOfPoints. Each value is a double.
        /// </summary>
        /// <param name="pointsRangeExpression">String in this format: startPoint:resolution:numOfPoints. </param>
        /// <returns>List of double for X points.</returns>
        private List<double> GetXAxisPoints(string pointsRangeExpression)
        {
            return this.ParsePointsRange(pointsRangeExpression);
        }

        /// <summary>
        /// This is a default implementation of the function. User can choose to override it.
        /// Expected format-> startPoint:resolution:numOfPoints. Each value is a double.
        /// </summary>
        /// <param name="pointsRangeExpression">String in this format: startPoint:resolution:numOfPoints. </param>
        /// <returns>List of double for Y points.</returns>
        private List<double> GetYAxisPoints(string pointsRangeExpression)
        {
            return this.ParsePointsRange(pointsRangeExpression);
        }

        /// <summary>
        /// This is a default implementation of the function. User can choose to override it.
        /// In this implementation DFM + first fail per plist settings is created.
        /// </summary>
        /// <param name="patlist">plist.</param>
        /// <param name="levelsTc">levels.</param>
        /// <param name="timingsTc">timings.</param>
        /// <param name="prePlist">prePlist.</param>
        /// <returns>The created IFunctionalTest.</returns>
        private IFunctionalTest GetFunctionalTest(string patlist, string levelsTc, string timingsTc, string prePlist)
        {
            return Prime.Services.FunctionalService.CreateCaptureFailureTest(patlist, levelsTc, timingsTc, 1, prePlist);
        }

        /// <summary>
        /// This is a default implementation of the function. User can choose to override it.
        /// In this default implementation, we set the TC spec value of X and Y if they are defined.
        /// </summary>
        /// <param name="point">current point.</param>
        /// <returns>false if this point should be skipped, true otherwise.</returns>
        private bool PrePointExecute(ShmooPoint point)
        {
            if (this.xParamTC != null)
            {
                this.xParamTC.SetSpecSetValue(this.XAxisParam, point.XValue.ToString());
            }

            if (this.yParamTC != null)
            {
                this.yParamTC.SetSpecSetValue(this.YAxisParam, point.YValue.ToString());
            }

            return true;
        }

        /// <summary>
        /// This is a default implementation of the function. User can choose to override it.
        /// It returns false if multiple defects captured for point. true otherwise.
        /// </summary>
        /// <param name="point">current point.</param>
        /// <param name="funcTest">funcTest of the executed plist.</param>
        /// <param name="failInfo">fail info to return.</param>
        /// <returns>false if this point should be skipped, true otherwise.</returns>
        private bool PostPointExecute(ShmooPoint point, IFunctionalTest funcTest, out string failInfo)
        {
            if (!this.isDefaultFuncTestUsed)
            {
                throw new TestMethodException($"User should implement extension method PostPointExecute() when GetFunctionalTest() is extended.");
            }

            ICaptureFailureTest dfmFuncTest = (ICaptureFailureTest)funcTest;
            List<IFailureData> fails = dfmFuncTest.GetPerCycleFailures();

            if (fails.Count != 0)
            {
                //////////////////////////////////////
                // the standard shmoo_hub fail format
                //////////////////////////////////////

                IFailureData firstFail = fails.First();
                string pattern = firstFail.GetPatternName();
                string domain = firstFail.GetDomainName();
                int cycle = Convert.ToInt32(firstFail.GetCycle());
                int mainrma = Convert.ToInt32(firstFail.GetVectorAddress());
                List<string> failingPins = firstFail.GetFailingPinNames();
                string failingPinsItuff = failingPins.Count > 0 ? string.Join(", ", failingPins) : failingPins.First();
                int subrma = -1;
                int scanrma = -1;
                failInfo = $"<{pattern}>:{domain}({cycle},{mainrma},{subrma},{scanrma}):{failingPinsItuff}";

                // failure found.
                return false;
            }
            else
            {
                // pass case.
                failInfo = "Pass";
                return true;
            }
        }

        private List<double> ParsePointsRange(string pointsRangeExpression)
        {
            // Tokenize the points range as "START_POINT:RESOLUTION:NUMBER_OF_POINTS".
            ExtractedRangeFields rangeFields = this.ExtractRangeFields(pointsRangeExpression);

            if (Convert.ToDouble(rangeFields.Resolution) < 0)
            {
                this.yaxisAscendOrder = false;
            }

            // conversion to double guaranteed by verify
            return this.GeneratePointsSequence(
                Convert.ToDouble(rangeFields.Start),
                Convert.ToDouble(rangeFields.Resolution),
                Convert.ToDouble(rangeFields.NumberOfPoints));
        }

        private void ClearRuntimeData()
        {
            this.xParamTC = null;
            this.yParamTC = null;
            this.xParamTcType = TestConditionType.ALL;
            this.yParamTcType = TestConditionType.ALL;
            this.isDefaultFuncTestUsed = false;
            this.originalXAxisParamValue = null;
            this.originalYAxisParamValue = null;
            this.shmooHubItuffData = string.Empty;
            this.functionalTest = null;
            this.xPoints.Clear();
            this.yPoints.Clear();
            this.shmooLoopPoints.Clear();
            this.plotData.Clear();
        }

        // function used internally for getting x and y params test conditions and updating member variables.
        private void GetTcParams()
        {
            if (!string.IsNullOrEmpty(this.XAxisParam))
            {
                this.xParamTC = this.RetrieveTcParam(this.XAxisParam, out this.xParamTcType);
                if (this.xParamTC == null)
                {
                    throw new TestMethodException($"XAxisParameter value=[{this.XAxisParam}] is not a valid TC.\n");
                }
            }

            if (!string.IsNullOrEmpty(this.YAxisParam))
            {
                this.yParamTC = this.RetrieveTcParam(this.YAxisParam, out this.yParamTcType);
                if (this.yParamTC == null)
                {
                    throw new TestMethodException($"YAxisParameter value=[{this.YAxisParam}] is not a valid TC.\n");
                }
            }
        }

        // function used internally for getting x and y params test conditions and updating member variables.
        private ITestCondition RetrieveTcParam(string specParam, out TestConditionType paramTcType)
        {
            ITestCondition lvlTC = Prime.Services.TestConditionService?.GetTestCondition(this.LevelsTc) ??
                                   throw new TestMethodException(
                                       "Prime.Services.TestConditionService.GetTestCondition(this.LevelsTc)");

            ITestCondition timTC = Prime.Services.TestConditionService?.GetTestCondition(this.TimingsTc) ??
                                   throw new TestMethodException(
                                       "Prime.Services.TestConditionService.GetTestCondition(this.TimingsTc)");

            try
            {
                lvlTC.GetSpecSetValue(specParam);
                paramTcType = TestConditionType.LEVELS;
                return lvlTC;
            }
            catch (FatalException)
            {
            }

            try
            {
                timTC.GetSpecSetValue(specParam);
                paramTcType = TestConditionType.TIMING;
                return timTC;
            }
            catch (FatalException)
            {
            }

            paramTcType = TestConditionType.ALL;
            return null;
        }

        // Save original value of X and Y axis parameter for ituff.
        private void SaveOriginalTcValue()
        {
            if (this.xParamTC != null)
            {
                this.originalXAxisParamValue = this.xParamTC.GetSpecSetValue(this.XAxisParam);
                this.Console?.PrintDebug("OriSpecSet for X=[" + this.originalXAxisParamValue + "]\n");
            }

            if (this.yParamTC != null)
            {
                this.originalYAxisParamValue = this.yParamTC.GetSpecSetValue(this.YAxisParam);
                this.Console?.PrintDebug("OriSpecSet for Y=[" + this.originalYAxisParamValue + "]\n");
            }
        }

        private void RestoreDefaultTcValue()
        {
            if (this.xParamTC != null)
            {
                this.xParamTC.SetSpecSetValue(this.XAxisParam, this.originalXAxisParamValue);
            }

            if (this.yParamTC != null)
            {
                this.yParamTC.SetSpecSetValue(this.YAxisParam, this.originalYAxisParamValue);
            }
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

        // function used internally to print shmoo plotting.
        private void PrintPlotToConsole()
        {
            List<double> distinctXAxis = this.shmooLoopPoints.Select(point => point.XValue).Distinct().ToList();

            List<ShmooPoint> sortYAxis;
            if (this.yaxisAscendOrder)
            {
                sortYAxis = this.shmooLoopPoints.OrderBy(point => point.YValue).ToList();
            }
            else
            {
                sortYAxis = this.shmooLoopPoints.OrderByDescending(point => point.YValue).ToList();
            }

            string plotting = string.Empty;
            plotting += $"{"Y\\X",5} "; // Print all the distinct xValue on top of shmoo.
            foreach (var xValue in distinctXAxis)
            {
                plotting += $"| {xValue,5} ";
            }

            plotting += "\n";

            for (int i = 0; i < sortYAxis.Count; i++)
            {
                if (i == 0)
                {
                    plotting += $"{sortYAxis[i].YValue,5} |";
                }

                if (i > 0 &&
                    !sortYAxis[i].YValue.Equals(double.NaN) &&
                    sortYAxis[i].YValue != sortYAxis[i - 1].YValue)
                {
                    plotting += "\n" + $"{sortYAxis[i].YValue,5} |"; // Print to next line if current yValue is different from previous index.
                    this.shmooHubItuffData += '_';
                }

                bool checkKeyExist = this.plotData.TryGetValue(sortYAxis[i], out char symbol);
                if (checkKeyExist)
                {
                    plotting += $" {symbol,5}  ";
                    this.shmooHubItuffData += symbol;
                }
                else
                {
                    throw new TestMethodException($"No plotting data for point=[{sortYAxis[i]}].");
                }
            }

            this.Console?.PrintDebug(plotting);

            foreach (var legendEntry in this.plotLegend)
            {
                this.Console?.PrintDebug($"Legend : [{legendEntry.Key}] | Failure information: {legendEntry.Value}");
            }
        }

        private void PrintPlotToItuff(string configSetPoint)
        {
            this.PrintSSTP(configSetPoint);
            this.PrintSpecSetAndShmooItuff(configSetPoint);
            this.PrintLegendAndFailPinsItuff(configSetPoint);
        }

        // print SSTP to ituff.
        private void PrintSSTP(string configSetPoint)
        {
            var writter = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            writter.SetTnamePostfix("_IPMShmoo_" + configSetPoint + "_SSTP");
            string strgval = null;

            if (this.xParamTC != null)
            {
                strgval = $"X_{this.originalXAxisParamValue}";
                bool isLvlTc = this.xParamTcType == TestConditionType.LEVELS;
                string unit = isLvlTc ? "V" : "nS";
                strgval += unit;
            }

            if (this.yParamTC != null)
            {
                strgval += $"_Y_{this.originalYAxisParamValue}";
                bool isLvlTc = this.yParamTcType == TestConditionType.LEVELS;
                string unit = isLvlTc ? "V" : "nS";
                strgval += unit;
            }

            writter.SetData(strgval);

            Prime.Services.DatalogService.WriteToItuff(writter);
        }

        // Print specSet, specSet value and series of legend symbol
        private void PrintSpecSetAndShmooItuff(string configSetPoint)
        {
            var writter = Prime.Services.DatalogService.GetItuffStrgvalWriter();

            string tname = null;
            if (!string.IsNullOrEmpty(this.XAxisParam))
            {
                double firstXPoint = this.xPoints.First();
                double lastXPoint = this.xPoints.Last();
                double resolution = this.xPoints[1] - firstXPoint;
                tname = $"_IPMShmoo_" + configSetPoint + "^" + this.XAxisParam + "^" + firstXPoint + "^"
                    + lastXPoint + "^" + resolution;
            }

            if (!string.IsNullOrEmpty(this.YAxisParam))
            {
                double firstYPoint = this.yPoints.First();
                double lastYPoint = this.yPoints.Last();
                double resolution = this.yPoints[1] - firstYPoint;
                tname += $"_" + this.YAxisParam + "^" + firstYPoint + "^"
                    + lastYPoint + "^" + resolution;
            }

            writter.SetTnamePostfix(tname);
            writter.SetData(this.shmooHubItuffData.ToString());
            Prime.Services.DatalogService.WriteToItuff(writter);
        }

        // Print the legend and failing cycle to ituff in these format.
        // 2_tname_<ModuleName:TestName>^LEGEND^a
        // 2_strgval_<patternname>:LEG(41038,36033,-1,-1):failing pins
        private void PrintLegendAndFailPinsItuff(string configSetPoint)
        {
            var ituffWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();

            foreach (var legend in this.plotLegend)
            {
                ituffWriter.SetTnamePostfix("_IPMShmoo_" + configSetPoint + $"^LEGEND^{legend.Key}");
                ituffWriter.SetData(legend.Value);
                Prime.Services.DatalogService.WriteToItuff(ituffWriter);
            }
        }

        // function used internally to parse x and y shmoo range expressions (given in TM params) into test methods members.
        private void CalculateAxisPoints()
        {
            this.xPoints = this.GetXAxisPoints(this.XAxisRange);
            this.yPoints = this.GetYAxisPoints(this.YAxisRange);
        }

        // function used internally to extract tokens from x and y range expressions given in the TM parameters.
        private ExtractedRangeFields ExtractRangeFields(TestMethodsParams.String xAxisRange)
        {
            var r = new Regex(@"(.+):(.+):(.+)", RegexOptions.IgnoreCase);
            var match = r.Match(xAxisRange);
            if (match.Success)
            {
                ExtractedRangeFields fieldsValues;
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
            this.functionalTest =
                    this.GetFunctionalTest(this.Patlist, this.LevelsTc, this.TimingsTc, string.Empty) as IFunctionalTest;

            this.isDefaultFuncTestUsed = true;
        }

        // function used internally to create one list of doubles as the final points list for performing shmoo in Execute().
        private void CreatePointsLoopList()
        {
            var xPointsNum = this.xPoints.Count;
            var yPointsNum = this.yPoints.Count;

            this.shmooLoopPoints.Clear();

            if (xPointsNum > 0 && yPointsNum > 0)
            {
                foreach (var yPoint in this.yPoints)
                {
                    this.Create1DShmooLoop(yPoint);
                }
            }
            else if (xPointsNum > 0)
            {
                this.Create1DShmooLoop(double.NaN);
            }
            else
            {
                // ERROR
                throw new TestMethodException("No axis is being defined or incorrect format.");
            }
        }

        // function used internally to create 1D shmoo line of fixed y-point and running x-point.
        private void Create1DShmooLoop(double yPoint)
        {
            this.xPoints.ForEach(x => this.shmooLoopPoints.Add(new ShmooPoint(x, yPoint)));
        }

        // function to delimit configList to obtain module and group name
        private void VerifyConfigList(string configList)
        {
            this.moduleGroupList = configList.Split(':').ToList();
            if (this.moduleGroupList.Count != 2)
            {
                throw new TestMethodException(
                    "Incorrect format for ConfigList. Format should be <Module>:<Group> as specified in PatConfigSetPoint.json file.");
            }
        }

        /// <summary>
        /// ExtractedRangedFields.
        /// </summary>
        public struct ExtractedRangeFields
        {
            /// <summary>
            /// Start.
            /// </summary>
            public string Start;

            /// <summary>
            /// NumberOfPoints.
            /// </summary>
            public string NumberOfPoints;

            /// <summary>
            /// Resolution.
            /// </summary>
            public string Resolution;
        }
    }
}