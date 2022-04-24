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
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.TestMethods.Shmoo;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class DDGShmooTC : PrimeShmooTestMethod, IShmooExtensions
    {
        /// <summary>
        /// Enum for the Datalogging mode.
        /// </summary>
        public enum ItuffMode
        {
            /// <summary>
            /// SHMOO_HUB format.
            /// </summary>
            SHMOO_HUB,

            /// <summary>
            /// ECADS format.
            /// </summary>
            ECADS,
        }

        /// <summary>
        /// Enum containing the Axis ID (X or Y).
        /// </summary>
        public enum AxisID
        {
            /// <summary>
            /// X Axis.
            /// </summary>
            X,

            /// <summary>
            /// Y Axis.
            /// </summary>
            Y,
        }

        /// <summary>
        /// Enum for X/Y Axis Type parameters.
        /// </summary>
        public enum AxisType
        {
            /// <summary>
            /// Axis is unused.
            /// </summary>
            None,

            /// <summary>
            /// Axis is a UserVar used in the Timing TestCondition.
            /// </summary>
            UserVarTiming,

            /// <summary>
            /// Axis is a UserVar used in the Levles TestCondition.
            /// </summary>
            UserVarLevels,

            /// <summary>
            /// Axis is a TestCondition/SpecSet Variables Type.
            /// </summary>
            SpecSetVariable,

            /// <summary>
            /// Axis is a FIVR Voltage Target Type.
            /// </summary>
            FIVR,

            /// <summary>
            /// Axis is a PatConfig Type.
            /// </summary>
            PatConfig,

            /// <summary>
            /// Axis is a Prime PatConfig SetPoint Type.
            /// </summary>
            PatConfigSetPoint,
        }

        /// <summary>
        /// Enum specifying the PrePointExecMode.
        /// </summary>
        public enum PrePointExecType
        {
            /// <summary>PrePointExec mode is disabled.</summary>
            Never,

            /// <summary>PrePointExecTest is run whenever the X Parameter changes.</summary>
            OnXChange,

            /// <summary>PrePointExecTest is run whenever the Y Parameter changes.</summary>
            OnYChange,

            /// <summary>PrePointExecTest is run on every testpoint.</summary>
            OnAnyChange,
        }

        /// <summary>
        /// Gets or sets the XAxisType parameter.
        /// </summary>
        public AxisType XAxisType { get; set; } = AxisType.SpecSetVariable;

        /// <summary>
        /// Gets or sets the YAxisType parameter.
        /// </summary>
        public AxisType YAxisType { get; set; } = AxisType.SpecSetVariable;

        /// <summary>
        /// Gets or sets a list of voltage overrides.
        /// </summary>
        public TestMethodsParams.String VoltageConverter { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Ituff Formatting to use.
        /// </summary>
        public ItuffMode DataLogType { get; set; } = ItuffMode.SHMOO_HUB;

        /// <summary>
        /// Gets or sets the PrePointExec Mode.
        /// </summary>
        public PrePointExecType PrePointExecMode { get; set; } = PrePointExecType.Never;

        /// <summary>
        /// Gets or sets the test to run when PrePointExec mode is enabled.
        /// </summary>
        public TestMethodsParams.String PrePointExecTest { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        private ShmooPoint LastPointExecuted { get; set; } = null;

        private List<ShmooPoint> AllShmooPoints { get; set; }

        private IFunctionalTest FuncTest { get; set; }

        private Dictionary<AxisID, ShmooAxis> AxisDetails { get; set; } = new Dictionary<AxisID, ShmooAxis>(2);

        /// <inheritdoc />
        public override void CustomVerify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;

            /* this.AxisDetails = new Dictionary<AxisID, ShmooAxis>(2);
            this.AxisDetails[AxisID.X] = new ShmooAxis(AxisID.X, this.XAxisType, this.XAxisParam, this.TimingsTc, this.LevelsTc, this.Patlist, this.VoltageConverter);
            this.AxisDetails[AxisID.Y] = new ShmooAxis(AxisID.Y, this.YAxisType, this.YAxisParam, this.TimingsTc, this.LevelsTc, this.Patlist, this.VoltageConverter); */

            // Verify the PrePointExec arguments.
            if (this.PrePointExecMode != PrePointExecType.Never)
            {
                if (this.PrePointExecTest == null || string.IsNullOrWhiteSpace(this.PrePointExecTest))
                {
                    throw new Prime.Base.Exceptions.TestMethodException("Parameter=[PrePointExecTest] must be specified when Parameter=[PrePointExecMode] is not [Never].");
                }

                var allTests = Prime.Services.TestProgramService.GetAllTestInstanceNames();
                if (!allTests.Contains(this.PrePointExecTest))
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"Parameter=[PrePointExecTest] is invalid.  Test=[{this.PrePointExecTest}] does not exist.");
                }
            }
        }

        /// <inheritdoc />
        void IShmooExtensions.PreExecute(IFunctionalTest funcTest)
        {
            this.LastPointExecuted = null;
            this.AllShmooPoints = new List<ShmooPoint>();
        }

        /// <inheritdoc />
        void IShmooExtensions.PostExecute(IFunctionalTest funcTest)
        {
            if (this.DataLogType == ItuffMode.ECADS)
            {
                this.PrintItuffEcadsFormat();
            }

            // reset all axis back to their original values.
            this.AxisDetails.Values.ToList().ForEach(axis => axis.ResetOriginalValue());
        }

        /// <inheritdoc />
        IFunctionalTest IShmooExtensions.GetFunctionalTest(string patlist, string levelsTc, string timingsTc, string prePlist)
        {
            this.FuncTest = Prime.Services.FunctionalService.CreateCaptureFailureTest(patlist, levelsTc, timingsTc, 1, prePlist);
            return this.FuncTest;
        }

        /// <inheritdoc />
        List<double> IShmooExtensions.GetXAxisPoints(string pointsRangeExpression)
        {
            this.AxisDetails[AxisID.X] = new ShmooAxis(AxisID.X, this.XAxisType, this.XAxisParam, this.TimingsTc, this.LevelsTc, this.Patlist, this.VoltageConverter, this.Console);
            return this.AxisDetails[AxisID.X].DecodeRange(pointsRangeExpression);
        }

        /// <inheritdoc />
        List<double> IShmooExtensions.GetYAxisPoints(string pointsRangeExpression)
        {
            this.AxisDetails[AxisID.Y] = new ShmooAxis(AxisID.Y, this.YAxisType, this.YAxisParam, this.TimingsTc, this.LevelsTc, this.Patlist, this.VoltageConverter, this.Console);
            return this.AxisDetails[AxisID.Y].DecodeRange(pointsRangeExpression);
        }

        /// <inheritdoc />
        bool IShmooExtensions.PrePointExecute(ShmooPoint point, IFunctionalTest funcTest)
        {
            var xChanged = this.LastPointExecuted == null || this.PowerDownBetweenPoints == PowerDownBetweenPointsSettings.ENABLED || Math.Abs(this.LastPointExecuted.XValue - point.XValue) > double.Epsilon * 2;
            var yChanged = this.LastPointExecuted == null || this.PowerDownBetweenPoints == PowerDownBetweenPointsSettings.ENABLED || Math.Abs(this.LastPointExecuted.YValue - point.YValue) > double.Epsilon * 2;
            this.AllShmooPoints.Add(point);

            // Order of operations
            // 1. Run Prepoint test.
            // 2. Update SpecSet/Testcondition variables.
            // 3. Apply TestConditions to hardware.
            // 4. Update fivr/dlvr settings directly to hardware.
            if (this.PrePointExecMode == PrePointExecType.OnAnyChange ||
                (xChanged && this.PrePointExecMode == PrePointExecType.OnXChange) ||
                (yChanged && this.PrePointExecMode == PrePointExecType.OnYChange))
            {
                // mark everything so levels/timings are re-applied.
                xChanged = true;
                yChanged = true;
                this.LastPointExecuted = null;

                var rslt = Prime.Services.TestProgramService.ExecuteTestInstance(this.PrePointExecTest);
                this.Console?.PrintDebug($"Executed PrePointTest=[{this.PrePointExecTest}] with result=[{rslt}].");
                if (rslt != 1)
                {
                    return false; // this testpoint will be skipped.
                }
            }

            // If this is the first testpoint then we need to apply the base test conditions.
            // todo: this could  be optimized and combined with the UpdateAndApply if x/y is a testcondition.
            // todo: the base class now calls this.functionalTest.ApplyTestConditions() at the beginning for Execute().
            if (this.LastPointExecuted == null)
            {
                funcTest.ApplyTestConditions();
            }

            // Handle X/Y Parameters if they are test condition types.
            if (xChanged && this.AxisDetails[AxisID.X].IsTestConditionType)
            {
                this.AxisDetails[AxisID.X].UpdateAndApply(point.XValue, funcTest);
            }

            if (yChanged && this.AxisDetails[AxisID.Y].IsTestConditionType)
            {
                this.AxisDetails[AxisID.Y].UpdateAndApply(point.YValue, funcTest);
            }

            // Handle X/Y Parameters if they are NOT test condition types.
            if (xChanged && !this.AxisDetails[AxisID.X].IsTestConditionType)
            {
                this.AxisDetails[AxisID.X].UpdateAndApply(point.XValue);
            }

            if (yChanged && !this.AxisDetails[AxisID.Y].IsTestConditionType)
            {
                this.AxisDetails[AxisID.Y].UpdateAndApply(point.YValue);
            }

            this.LastPointExecuted = point;
            return true;
        }

        /// <inheritdoc />
        void IShmooExtensions.PostPointExecute(ShmooPoint point, IFunctionalTest funcTest)
        {
            var captureTest = (ICaptureFailureTest)funcTest;
            var fails = captureTest.GetPerCycleFailures();

            if (fails.Count != 0)
            {
                var firstFail = fails[0];
                var plist = firstFail.GetParentPlistName();
                var pattern = firstFail.GetPatternName();
                var patternId = firstFail.GetPatternInstanceId();
                var domain = firstFail.GetDomainName();
                var cycle = Convert.ToInt32(firstFail.GetCycle());
                var failingPins = firstFail.GetFailingPinNames();
                var failingPinsItuff = failingPins.Count > 0 ? string.Join(",", failingPins) : "unknown";
                if (patternId > 1)
                {
                    pattern += $"|{patternId}"; // add the patternid if its not the first pattern. could do this all the time.
                }

                // TODO: how to get the real mainrma if this is a subr failure?
                var mainrma = Convert.ToInt32(firstFail.GetVectorAddress());
                var subrma = firstFail.IsSubroutine() ? mainrma : -1;
                var scanrma = -1;
                var failInfo = $"{pattern}:{plist}:{domain}({cycle},{mainrma},{subrma},{scanrma}):{failingPinsItuff}";

                // failure found, add the fail string.
                this.GetPlotLegend().AddData(point, failInfo);
            }
        }

        private void PrintItuffEcadsFormat()
        {
            var writer = Prime.Services.DatalogService.GetItuffComntWriter();
            writer.ClearData();
            writer.IncludeTnameInPrint(false);
            writer.AddData($"Plot3Start_{this.InstanceName}");

            var axis = new List<ShmooAxis>(2);
            if (this.AxisDetails[AxisID.X].IsValid)
            {
                axis.Add(this.AxisDetails[AxisID.X]);
            }

            if (this.AxisDetails[AxisID.Y].IsValid)
            {
                axis.Add(this.AxisDetails[AxisID.Y]);
            }

            axis.ForEach(a => writer.AddData($"PLOT_P{a.ID}Name,{a.Parameter}"));
            axis.ForEach(a => writer.AddData($"PLOT_P{a.ID}Start,{a.TestPoints.First()}"));
            axis.ForEach(a => writer.AddData($"PLOT_P{a.ID}Stop,{a.TestPoints.Last()}"));
            axis.ForEach(a =>
            {
                writer.AddData($"PLOT_P{a.ID}Step,{a.TestPoints.Count}"); /* Changed from StepSize to StepCount */
            });
            axis.ForEach(a => writer.AddData($"PLOT_P{a.ID}Value,{string.Join("|", a.OriginalValue)}"));

            string line = string.Empty;
            for (var pointIndex = 0; pointIndex < this.AllShmooPoints.Count; pointIndex++)
            {
                var point = this.AllShmooPoints[pointIndex];
                line += this.GetPlotLegend().GetPointSymbol(point);

                if (pointIndex == (this.AllShmooPoints.Count - 1) || point.YValue != this.AllShmooPoints[pointIndex + 1].YValue)
                {
                    writer.AddData($"P3Data_{line}");
                    line = string.Empty;
                }
            }

            foreach (var legend in this.GetPlotLegend().GetPlotLegend().OrderBy(i => i.Key))
            {
                writer.AddData($"P3Legend_{legend.Key}_{legend.Value}");
            }

            writer.AddData($"Plot3End_{this.InstanceName}");
            Prime.Services.DatalogService.WriteToItuff(writer);
        }
    }
}