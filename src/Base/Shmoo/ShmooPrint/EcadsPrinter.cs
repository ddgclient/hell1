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

namespace Prime.TestMethods.Shmoo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Prime.Base.Exceptions;
    using Prime.FunctionalService;
    using Prime.TestConditionService;

    /// <summary>
    /// Print class for ecads format.
    /// </summary>
    internal class EcadsPrinter : ShmooPrinter
    {
        private readonly string instanceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EcadsPrinter"/> class.
        /// </summary>
        /// <param name="testInstanceName">Name of current test instance to be used during print.</param>
        public EcadsPrinter(string testInstanceName)
        {
            this.instanceName = testInstanceName;
        }

        /// <inheritdoc/>
        public override void PrintPlotToItuff(PlotLegendBase plotLegend, ShmooPlot shmooPlot)
        {
            var shmooLoopPoints = shmooPlot.ShmooLoopPoints;
            var writter = Prime.Services.DatalogService.GetItuffComntWriter();
            writter.IncludeTnameInPrint(true);

            var xString = this.GetEcadsPrint("X", shmooPlot.XAxisData);
            var yString = this.GetEcadsPrint("Y", shmooPlot.YAxisData);
            for (int i = 0; i < xString.Count; i++)
            {
                string x = xString[i];
                string y = yString.ElementAtOrDefault(i);

                writter.AddData(x);
                if (!string.IsNullOrEmpty(y))
                {
                    writter.AddData(y);
                }
            }

            StringBuilder line = new StringBuilder();
            for (var pointIndex = 0; pointIndex < shmooLoopPoints.Count; pointIndex++)
            {
                var point = shmooLoopPoints[pointIndex];
                line.Append(plotLegend.GetPointSymbol(point));

                if (pointIndex == (shmooLoopPoints.Count - 1) || Math.Abs(point.YValue - shmooLoopPoints[pointIndex + 1].YValue) > ShmooConstants.DoubleTolerance)
                {
                    writter.AddData($"P3Data_{line}");
                    line.Clear();
                }
            }

            foreach (var legend in plotLegend.GetPlotLegend().OrderBy(i => i.Key))
            {
                writter.AddData($"P3Legend_{legend.Key}_{legend.Value}");
            }

            writter.AddData($"Plot3End_{this.instanceName}");

            Prime.Services.DatalogService.WriteToItuff(writter);
        }

        private List<string> GetEcadsPrint(string axisTag, ShmooPlot.AxisData axisData)
        {
            var builder = new List<string>();
            if (!(axisData.Axis is EmptyShmooAxis))
            {
                this.PopulateShmooSummary(axisData, out string first, out string last, out string resolution);
                builder.Add("PLOT_P" + axisTag + "Name," + axisData.Axis.AxisNameForDatalog);
                builder.Add("PLOT_P" + axisTag + "Start," + first);
                builder.Add("PLOT_P" + axisTag + "Stop," + last);
                builder.Add("PLOT_P" + axisTag + "Step," + resolution);

                var isDouble = double.TryParse(axisData.Axis.OriginalValue, out var originalValue);
                var scalar = this.GetPrefixValue(axisData.Axis.UnitPrefixForDatalog);

                var value = isDouble ? (originalValue / scalar).ToString() : axisData.Axis.OriginalValue;

                builder.Add("PLOT_P" + axisTag + "Value," + value);
            }

            return builder;
        }
    }
}
