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
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Prime.Base.Exceptions;
    using Prime.FunctionalService;
    using Prime.TestConditionService;

    /// <summary>
    /// Print class for Aries format.
    /// </summary>
    internal class AriesPrinter : ShmooPrinter
    {
        /// <inheritdoc/>
        public override void PrintPlotToItuff(PlotLegendBase plotLegend, ShmooPlot shmooPlot)
        {
            this.PrintSSTP(shmooPlot);

            var shmooLoopPoints = shmooPlot.ShmooLoopPoints;

            var xTname = this.GetAxisTname(shmooPlot.XAxisData);
            var yTname = this.GetAxisTname(shmooPlot.YAxisData);

            var writter = Prime.Services.DatalogService.GetItuffRawbinaryWriter();
            writter.SetTnamePostfix(xTname + yTname);

            var line = new StringBuilder();
            for (var pointIndex = 0; pointIndex < shmooLoopPoints.Count; pointIndex++)
            {
                var point = shmooLoopPoints[pointIndex];
                line.Append(plotLegend.GetPointSymbol(point));

                if (pointIndex == (shmooLoopPoints.Count - 1) || Math.Abs(point.YValue - shmooLoopPoints[pointIndex + 1].YValue) > ShmooConstants.DoubleTolerance)
                {
                    var toWrite = this.GetBinaryString(line);
                    writter.AddData($"{toWrite}", true);
                    line.Clear();
                }
            }

            Prime.Services.DatalogService.WriteToItuff(writter);
        }

        private object GetBinaryString(StringBuilder line)
        {
            var binaryLine = line.ToString();
            binaryLine = Regex.Replace(binaryLine, "[A-Za-z]", "0");
            binaryLine = binaryLine.Replace("*", "1");
            return binaryLine;
        }

        private string GetAxisTname(ShmooPlot.AxisData axisData)
        {
            var tname = string.Empty;

            if (!(axisData.Axis is EmptyShmooAxis))
            {
                this.PopulateShmooSummary(axisData, out string first, out string last, out string resolution);

                tname += $"_" + axisData.Axis.AxisNameForDatalog + "_" + first + "_"
                         + last + "_" + resolution;
            }

            return tname;
        }
    }
}
