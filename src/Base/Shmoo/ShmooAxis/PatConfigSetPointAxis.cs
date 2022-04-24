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
    using Prime.PatConfigService;

    /// <summary>
    /// PatConfigSetPointAxis axis.
    /// </summary>
    internal class PatConfigSetPointAxis : IShmooAxis
    {
        private readonly List<IPatConfigSetPointHandle> patConfigSetPointHandles = new List<IPatConfigSetPointHandle>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PatConfigSetPointAxis"/> class.
        /// </summary>
        /// <param name="patConfigPairs">list of pat config names.</param>
        /// <param name="plist">plist to apply.</param>
        /// <param name="axisNameForDatalog">Axis name used for prints.</param>
        public PatConfigSetPointAxis(string patConfigPairs, string plist, string axisNameForDatalog)
        {
            this.AxisName = patConfigPairs;
            this.AxisNameForDatalog = axisNameForDatalog;
            foreach (var patConfigPair in patConfigPairs.Split(','))
            {
                var setPointPair = patConfigPair.Split(':');
                if (setPointPair.Length != 2)
                {
                    throw new TestMethodException("PatConfigSetPoint Axis value: " + patConfigPair + " was not in the format: \"module:group\"");
                }

                this.patConfigSetPointHandles.Add(Prime.Services.PatConfigService.GetSetPointHandle(setPointPair[0], setPointPair[1], plist));
            }
        }

        /// <inheritdoc/>
        public string OriginalValue { get; } = "NaN";

        /// <inheritdoc/>
        public string AxisName { get; }

        /// <inheritdoc/>
        public string AxisNameForDatalog { get; }

        /// <inheritdoc/>
        public PrimeShmooTestMethod.UnitPrefixForDatalog UnitPrefixForDatalog { get; }

        /// <inheritdoc/>
        public double PreviousValue { get; }

        /// <inheritdoc/>
        public bool Verify()
        {
            return true;
        }

        /// <summary>
        /// validates points.
        /// </summary>
        /// <param name="points">points to val.</param>
        public void ValidatePoints(List<string> points)
        {
            // String value's format needs to be determined.
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <param name="axisValue">axis val.</param>
        /// <param name="functionalTest">func test.</param>
        /// <returns>true pass false fail.</returns>
        public bool PrePointExecute(string axisValue, IFunctionalTest functionalTest)
        {
            this.patConfigSetPointHandles.ForEach(o => o.ApplySetPoint(axisValue));
            return true;
        }

        /// <inheritdoc/>
        public bool PostPointExecute(ShmooPoint point, IFunctionalTest functionalTest)
        {
            return true;
        }

        /// <inheritdoc/>
        public void PostExecute()
        {
            // Nothing by default.
        }

        /// <inheritdoc/>
        public bool PreExecute()
        {
            return true;
        }

        /// <inheritdoc/>
        public string GetUnit()
        {
            return string.Empty;
        }

        /// <inheritdoc/>
        public bool Apply(double axisValue, IFunctionalTest functionalTest)
        {
            return true;
        }
    }
}