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
    using Prime.PatConfigService;

    /// <summary>
    /// PatConfigAxis version of axis.
    /// </summary>
    internal class PatConfigAxis : IShmooAxis
    {
        private readonly List<IPatConfigHandle> patConfigs = new List<IPatConfigHandle>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PatConfigAxis"/> class.
        /// </summary>
        /// <param name="patConfigNames">list of pat config names.</param>
        /// <param name="plist">plist to apply.</param>
        /// <param name="axisNameForDatalog">Axis name used for prints.</param>
        public PatConfigAxis(string patConfigNames, string plist, string axisNameForDatalog)
        {
            this.AxisName = patConfigNames;
            this.AxisNameForDatalog = axisNameForDatalog;
            foreach (var patConfigName in patConfigNames.Split(','))
            {
                this.patConfigs.Add(Prime.Services.PatConfigService.GetPatConfigHandleWithPlist(patConfigName, plist));
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

        /// <inheritdoc/>
        public bool PrePointExecute(string axisValue, IFunctionalTest functionalTest)
        {
            int.TryParse(axisValue, out int value);

            var data = this.patConfigs.Select(o => IntegerToBinary(value, (int)o.GetExpectedDataSize())).ToList();

            for (var i = 0; i < data.Count; i++)
            {
                this.patConfigs[i].SetData(data[i]);
            }

            Prime.Services.PatConfigService.Apply(this.patConfigs);
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
            foreach (var patConfig in this.patConfigs)
            {
                patConfig.ResetToDefault();
            }
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

        /// <summary>
        /// validates points.
        /// </summary>
        /// <param name="points">points to val.</param>
        public void ValidatePoints(List<string> points)
        {
            foreach (var axisValue in points)
            {
                var intParsed = int.TryParse(axisValue, out var axisValueToApply);
                if (!intParsed)
                {
                    throw new TestMethodException("PatConfig Axis value was not in an integer format");
                }
            }
        }

        /// <inheritdoc/>
        public bool Apply(double axisValue, IFunctionalTest functionalTest)
        {
            return true;
        }

        private static string ResizeBinary(string data, int size)
        {
            if (size <= 0 || size == data.Length)
            {
                return data;
            }

            if (size > data.Length)
            {
                return data.PadLeft(size, '0');
            }

            return data.Substring(data.Length - size);
        }

        private static string IntegerToBinary(int data, int size = 0)
        {
            return ResizeBinary(Convert.ToString(data, 2), size);
        }
    }
}