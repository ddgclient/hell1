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
    /// Print class for shmoohub.
    /// </summary>
    [Serializable]
    internal class ShmooHubPrinter : ShmooPrinter
    {
        // This is required by TRACE tool to asocitate an ituff print with shmoo
        private const string ShmooHubIdentifier = "ShmooHub";

        private StringBuilder shmooHubItuffData;
        private StringBuilder shmooConsoleData;

        /// <inheritdoc/>
        public override void PrintPlotToItuff(PlotLegendBase plotLegend, ShmooPlot shmooPlot)
        {
            this.SetupPlotStrings(plotLegend, shmooPlot);
            this.PrintSSTP(shmooPlot);
            this.PrintSpecSetAndShmooItuff(shmooPlot);
            this.PrintLegendAndFailPinsItuff(plotLegend);
        }

        private void SetupPlotStrings(PlotLegendBase plotLegend, ShmooPlot shmooPlot)
        {
            var shmooLoopPoints = shmooPlot.ShmooLoopPoints;

            var distinctXAxis = shmooLoopPoints.Select(point => point.XValue).Distinct().ToList();

            List<ShmooPoint> sortYAxis = shmooLoopPoints.OrderBy(point => point.YValue).ToList();

            this.shmooConsoleData = new StringBuilder();
            this.shmooHubItuffData = new StringBuilder();

            this.shmooConsoleData.Append($"{"Y\\X",5} "); // Print all the distinct xValue on top of shmoo.
            foreach (var xValue in distinctXAxis)
            {
                var printValue = this.GetPointString(shmooPlot.XAxisData.StringPoints, xValue);
                this.shmooConsoleData.Append($"| {printValue,5} ");
            }

            this.shmooConsoleData.Append("\n");

            for (int i = 0; i < sortYAxis.Count; i++)
            {
                if (i == 0)
                {
                    var printValue = this.GetPointString(shmooPlot.YAxisData.StringPoints, sortYAxis[i].YValue);
                    this.shmooConsoleData.Append($"{printValue,5} |");
                }

                if (i > 0 &&
                    !sortYAxis[i].YValue.Equals(double.NaN) &&
                    sortYAxis[i].YValue != sortYAxis[i - 1].YValue)
                {
                    this.shmooConsoleData.Append("\n");

                    var printValue = this.GetPointString(shmooPlot.YAxisData.StringPoints, sortYAxis[i].YValue);
                    this.shmooConsoleData.Append($"{printValue,5} |");

                    this.shmooHubItuffData.Append('_');
                }

                string symbol = plotLegend.GetPointSymbol(sortYAxis[i]);
                this.shmooConsoleData.Append($" {symbol,5}  ");
                this.shmooHubItuffData.Append(symbol);
            }
        }

        // TODO: This function should be removed once string format Shmoo Points are added.
        private string GetPointString(List<string> stringPoints, double value)
        {
            if (stringPoints.Count == 0)
            {
                return value.ToString();
            }
            else
            {
                return stringPoints.ElementAtOrDefault((int)value);
            }
        }

        // Print specSet, specSet value and series of legend symbol
        private void PrintSpecSetAndShmooItuff(ShmooPlot shmooPlot)
        {
            var writter = Prime.Services.DatalogService.GetItuffStrgvalWriter();

            string tname = string.Empty;
            tname += this.GetAxisTname(shmooPlot.XAxisData);
            tname += this.GetAxisTname(shmooPlot.YAxisData);

            // add TRACE identifier for ShmooHub print
            tname += $"{(string.IsNullOrEmpty(tname) ? string.Empty : "_")}{ShmooHubIdentifier}";

            writter.SetTnamePostfix(tname);
            writter.SetData(this.shmooHubItuffData.ToString());
            Prime.Services.DatalogService.WriteToItuff(writter);
        }

        private string GetAxisTname(ShmooPlot.AxisData axisData)
        {
            var tname = string.Empty;

            if (!(axisData.Axis is EmptyShmooAxis))
            {
                this.PopulateShmooSummary(axisData, out string first, out string last, out string resolution);

                tname += $"^" + axisData.Axis.AxisNameForDatalog + "^" + first + "^"
                         + last + "^" + resolution;
            }

            return tname;
        }

        // Print the legend and failing cycle to ituff in these format.
        // 2_tname_<ModuleName:TestName>^LEGEND^a
        // 2_strgval_<patternname>:LEG(41038,36033,-1,-1):failing pins
        private void PrintLegendAndFailPinsItuff(PlotLegendBase plotLegend)
        {
            Prime.Services.ConsoleService.PrintDebug(this.shmooConsoleData.ToString());
            var ituffWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();

            foreach (var legend in plotLegend)
            {
                Prime.Services.ConsoleService.PrintDebug($"Legend : [{legend.Key}] | Failure information: {legend.Value}");
                ituffWriter.SetTnamePostfix($"^LEGEND^{legend.Key}");
                ituffWriter.SetData(legend.Value);
                Prime.Services.DatalogService.WriteToItuff(ituffWriter);
            }
        }
    }
}