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
    using System.Text;
    using System.Threading.Tasks;
    using Prime.Base.Exceptions;
    using Prime.FunctionalService;

    /// <summary>
    /// Holds the shmoo axis and shmoo points.
    /// </summary>
    [Serializable]
    internal class ShmooPlot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShmooPlot"/> class.
        /// </summary>
        /// <param name="xAxisData">XAxis for shmoo plot.</param>
        /// <param name="yAxisData">YAxis for shmoo plot.</param>
        public ShmooPlot(AxisData xAxisData, AxisData yAxisData)
        {
            this.XAxisData = xAxisData;
            this.YAxisData = yAxisData;

            this.CreatePointsLoopList();
        }

        /// <summary>
        /// Gets all shmoo points arranged in one list. At execute we iterate over this list to create shmoo.
        /// </summary>
        public List<ShmooPoint> ShmooLoopPoints { get; } = new List<ShmooPoint>();

        /// <summary>
        /// Gets data for x axis.
        /// </summary>
        public AxisData XAxisData { get; }

        /// <summary>
        /// Gets data for y axis.
        /// </summary>
        public AxisData YAxisData { get; }

        /// <summary>
        /// Runs before each point is executed. Sets the TC spec value of X and Y if they are defined.
        /// </summary>
        /// <param name="point">current point.</param>
        /// <param name="functionalTest">funcTest of the executed plist.</param>
        /// <returns>false if this point should be skipped, true otherwise.</returns>
        public bool PrePointExecute(ShmooPoint point, IFunctionalTest functionalTest)
        {
            // TODO: Currently Double values are being used to index string list for axis types with string inputs (PatConfig and PatConfigSetpoint)
            // Need to convert ShmooPoints to be strings instead of doubles for Prime v9 release. Saving this for major release to prevent breaking user code.
            var yValue = this.YAxisData.StringPoints.Count > 0 ? this.YAxisData.StringPoints.ElementAtOrDefault((int)point.YValue) : point.YValue.ToString();
            var xValue = this.XAxisData.StringPoints.Count > 0 ? this.XAxisData.StringPoints.ElementAtOrDefault((int)point.XValue) : point.XValue.ToString();

            return this.XAxisData.Axis.PrePointExecute(xValue, functionalTest) && this.YAxisData.Axis.PrePointExecute(yValue, functionalTest);
        }

        /// <summary>
        /// Runs during verify.
        /// </summary>
        /// <returns>false if this point should be skipped, true otherwise.</returns>
        public bool Verify()
        {
            return this.XAxisData.Axis.Verify() && this.YAxisData.Axis.Verify();
        }

        /// <summary>
        /// Runs after execute for cleanup/print functions.
        /// </summary>
        public void PostExecute()
        {
            this.XAxisData.Axis.PostExecute();
            this.YAxisData.Axis.PostExecute();
        }

        /// <summary>
        /// Runs before all shmoo points have been executed.
        /// </summary>
        /// <returns>True if successfully executed, false otherwise.</returns>
        public bool PreExecute()
        {
            return this.XAxisData.Axis.PreExecute() && this.YAxisData.Axis.PreExecute();
        }

        // function used internally to create one list of doubles as the final points list for performing shmoo in Execute().
        private void CreatePointsLoopList()
        {
            var xPointsNum = this.XAxisData.Points.Count;
            var yPointsNum = this.YAxisData.Points.Count;

            if (xPointsNum > 0 && yPointsNum > 0)
            {
                foreach (var yPoint in this.YAxisData.Points)
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
            this.XAxisData.Points.ForEach(x => this.ShmooLoopPoints.Add(new ShmooPoint(x, yPoint)));
        }

        /// <summary>
        /// Wrapper for all data for a specific cardinal axis (x or y).
        /// </summary>
        [Serializable]
        public struct AxisData
        {
            /// <summary>
            /// Shmoo Axis for points to run over.
            /// </summary>
            public IShmooAxis Axis;

            /// <summary>
            /// List of points to run over shmoo axis.
            /// </summary>
            public List<double> Points;

            /// <summary>
            /// List of points as strings to run over shmoo axis.
            /// </summary>
            public List<string> StringPoints;

            /// <summary>
            /// Initializes a new instance of the <see cref="AxisData"/> struct.
            /// </summary>
            /// <param name="axis">Unique shmoo axis.</param>
            /// <param name="points">Points to be executed on axis.</param>
            /// <param name="stringPoints">string Points to be executed on axis.</param>
            public AxisData(IShmooAxis axis, List<double> points, List<string> stringPoints)
            {
                this.Axis = axis;
                this.Points = points;
                this.StringPoints = stringPoints;
            }
        }
    }
}
