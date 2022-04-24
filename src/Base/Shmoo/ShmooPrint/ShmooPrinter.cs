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
    using System.Globalization;
    using System.Linq;
    using Prime.FunctionalService;

    /// <summary>
    /// Base class used for ituff prints.
    /// </summary>
    [Serializable]
    internal abstract class ShmooPrinter
    {
        /// <summary>
        /// Prints plot to ituff. Prints to console at the same time.
        /// </summary>
        /// <param name="plotLegend">legend for plot.</param>
        /// <param name="shmooPlot">shmoo plot to print.</param>
        public abstract void PrintPlotToItuff(PlotLegendBase plotLegend, ShmooPlot shmooPlot);

        /// <summary>
        /// Returns failure data string ot be used by Plot legend.
        /// </summary>
        /// <param name="fails">list of failures for functional test.</param>
        /// <returns>Formatted failure string.</returns>
        public string GetFailString(List<IFailureData> fails)
        {
            IFailureData firstFail = fails.First();
            string plist = firstFail.GetParentPlistName();
            string pattern = firstFail.GetPatternName();
            string domain = firstFail.GetDomainName();
            int cycle = Convert.ToInt32(firstFail.GetCycle());
            int mainrma = Convert.ToInt32(firstFail.GetVectorAddress());
            List<string> failingPins = firstFail.GetFailingPinNames();
            string failingPinsItuff = failingPins.Count > 0 ? string.Join(",", failingPins) : failingPins.First();
            int subrma = -1;
            int scanrma = -1;
            string failString = $"{pattern}:{plist}:{domain}({cycle},{mainrma},{subrma},{scanrma}):{failingPinsItuff}";
            return failString;
        }

        /// <summary>
        /// Returns resolution of shmoo point list.
        /// </summary>
        /// <param name="shmooPoints">list of shmoo points being run.</param>
        /// <returns>Resolution based on spacing between points.</returns>
        public double GetResolution(List<double> shmooPoints)
        {
            double resolution;
            if (shmooPoints.Count == 0)
            {
                resolution = double.NaN;
            }
            else if (shmooPoints.Count == 1)
            {
                resolution = 0;
            }
            else
            {
                var firstPoint = shmooPoints.FirstOrDefault();
                var secondPoint = shmooPoints.ElementAtOrDefault(1);

                // formatter goes out to 12 digits to support up to 0.01 ns increments and to prevent floating point issues.
                resolution = Math.Round(secondPoint - firstPoint, 12);
            }

            return resolution;
        }

        /// <summary>
        /// Gets the string value to print for scalar prefixes.
        /// </summary>
        /// <param name="axisScale">Enum for scalar of datalog.</param>
        /// <returns>unit prefix based on scale.</returns>
        protected string GetPrefixString(PrimeShmooTestMethod.UnitPrefixForDatalog axisScale)
        {
            switch (axisScale)
            {
                case PrimeShmooTestMethod.UnitPrefixForDatalog.Nano:
                    return "n";
                case PrimeShmooTestMethod.UnitPrefixForDatalog.Micro:
                    return "u";
                case PrimeShmooTestMethod.UnitPrefixForDatalog.Milli:
                    return "m";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Gets value for prefix string.
        /// </summary>
        /// <param name="axisScale">Enum for scalar of datalog.</param>
        /// <returns>multiplier to be used to scale based on units.</returns>
        protected double GetPrefixValue(PrimeShmooTestMethod.UnitPrefixForDatalog axisScale)
        {
            switch (axisScale)
            {
                case PrimeShmooTestMethod.UnitPrefixForDatalog.Nano:
                    return 1E-9;
                case PrimeShmooTestMethod.UnitPrefixForDatalog.Micro:
                    return 1E-6;
                case PrimeShmooTestMethod.UnitPrefixForDatalog.Milli:
                    return 1E-3;
                default:
                    return 1;
            }
        }

        /// <summary>
        /// Generates summary (first, last, resolution) for shmoo plot print.
        /// </summary>
        /// <param name="axisData">Axis data being summarized.</param>
        /// <param name="first">first shmoo point value.</param>
        /// <param name="last">last shmoo point value.</param>
        /// <param name="resolution">resolution between shmoo points.</param>
        protected void PopulateShmooSummary(ShmooPlot.AxisData axisData, out string first, out string last, out string resolution)
        {
            if (axisData.StringPoints.Count == 0)
            {
                var firstValue = axisData.Points.FirstOrDefault();
                var lastValue = axisData.Points.LastOrDefault();
                var resolutionValue = this.GetResolution(axisData.Points);

                // Apply scalar after points are calculated to prevent floating point issues
                double scale = this.GetPrefixValue(axisData.Axis.UnitPrefixForDatalog);
                firstValue /= scale;
                lastValue /= scale;
                resolutionValue /= scale;

                first = firstValue.ToString(CultureInfo.InvariantCulture);
                last = lastValue.ToString(CultureInfo.InvariantCulture);
                resolution = resolutionValue.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                first = axisData.StringPoints.FirstOrDefault();
                last = axisData.StringPoints.LastOrDefault();
                resolution = "NaN";
            }
        }

        /// <summary>
        /// Generates SSTP summary for shmoo plot print. prints in the format: 2_strgval_X_{originalXValue}_Y_4{originalYValue} Used by Aries and ShmooHub formats.
        /// </summary>
        /// <param name="shmooPlot">Shmoo plot to print SSTP for.</param>
        // print SSTP to ituff.
        protected void PrintSSTP(ShmooPlot shmooPlot)
        {
            var writter = Services.DatalogService.GetItuffStrgvalWriter();
            writter.SetTnamePostfix("_SSTP");

            var xItuff = this.GetAxisSSTP(shmooPlot.XAxisData.Axis, "X");
            var yItuff = this.GetAxisSSTP(shmooPlot.YAxisData.Axis, "Y");

            var strgval = string.Join("_", new[] { xItuff, yItuff }.Where(c => !string.IsNullOrEmpty(c)));

            if (!string.IsNullOrEmpty(strgval))
            {
                writter.SetData(strgval);
                Services.DatalogService.WriteToItuff(writter);
            }
        }

        private string GetAxisSSTP(IShmooAxis shmooAxis, string tag)
        {
            var sstp = string.Empty;
            if (!(shmooAxis is EmptyShmooAxis))
            {
                sstp = tag;
                var value = shmooAxis.OriginalValue;
                var unit = shmooAxis.GetUnit();

                if (!string.IsNullOrEmpty(value))
                {
                    sstp += "_" + value + unit;
                }
            }

            return sstp;
        }
    }
}
