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
    using Prime.Base.Exceptions;
    using Prime.FunctionalService;
    using Prime.TestConditionService;

    /// <summary>
    /// TestConditionAxis version of axis.
    /// </summary>
    [Serializable]
    internal abstract class TestConditionAxis : IShmooAxis
    {
        private ITestCondition parameterTestCondition;
        private string testConditionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestConditionAxis"/> class.
        /// </summary>
        /// <param name="specSetName">name of specset that axis is made of.</param>
        /// <param name="testConditionName">input test condition name.</param>
        /// <param name="unitPrefixForDatalog">scale for the axis.</param>
        /// <param name="nameForDatalog">name of the axis during datalog prints.</param>
        protected TestConditionAxis(string specSetName, string testConditionName, PrimeShmooTestMethod.UnitPrefixForDatalog unitPrefixForDatalog, string nameForDatalog)
        {
            this.AxisName = specSetName;
            this.UnitPrefixForDatalog = unitPrefixForDatalog;
            this.AxisNameForDatalog = nameForDatalog;
            this.testConditionName = testConditionName;
        }

        /// <inheritdoc/>
        public string OriginalValue { get; private set; }

        /// <inheritdoc/>
        public string AxisName { get; }

        /// <inheritdoc/>
        public string AxisNameForDatalog { get; }

        /// <inheritdoc/>
        public PrimeShmooTestMethod.UnitPrefixForDatalog UnitPrefixForDatalog { get; }

        /// <inheritdoc/>
        public double PreviousValue { get; internal set; } = double.NaN;

        /// <inheritdoc/>
        public bool Verify()
        {
            this.parameterTestCondition = this.GetTestCondition(this.testConditionName);
            this.OriginalValue = this.parameterTestCondition.GetSpecSetValue(this.AxisName);

            // GetTestCondition() & GetSpecSetValue() will throw in case of error
            return true;
        }

        /// <inheritdoc/>
        public bool Apply(double axisValue, IFunctionalTest functionalTest)
        {
            if (this.parameterTestCondition != null && Math.Abs(axisValue - this.PreviousValue) > ShmooConstants.DoubleTolerance)
            {
                // only apply when axis value changes
                this.ApplyTestCondition(functionalTest);
            }

            this.PreviousValue = axisValue;
            return true;
        }

        /// <summary>
        /// Applies test condition if the axis value has changed.
        /// </summary>
        /// <param name="functionalTest">functional test to apply test condition onto.</param>
        public abstract void ApplyTestCondition(IFunctionalTest functionalTest);

        /// <inheritdoc/>
        public bool PrePointExecute(string axisValue, IFunctionalTest functionalTest)
        {
            this.parameterTestCondition.SetSpecSetValue(this.AxisName, axisValue);
            this.Apply(double.Parse(axisValue), functionalTest);
            return true;
        }

        /// <inheritdoc/>
        public void PostExecute()
        {
            this.parameterTestCondition.SetSpecSetValue(this.AxisName, this.OriginalValue);
        }

        /// <inheritdoc/>
        public bool PostPointExecute(ShmooPoint point, IFunctionalTest functionalTest)
        {
            return true;
        }

        /// <inheritdoc/>
        public bool PreExecute()
        {
            this.PreviousValue = double.NaN;
            return true;
        }

        /// <inheritdoc/>
        public abstract string GetUnit();

        private ITestCondition GetTestCondition(string testConditionName)
        {
            return Prime.Services.TestConditionService.GetTestCondition(testConditionName);
        }
    }
}
