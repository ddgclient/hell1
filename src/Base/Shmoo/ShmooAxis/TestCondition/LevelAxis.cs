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
    using Prime.Base.Exceptions;
    using Prime.FunctionalService;
    using Prime.TestConditionService;

    /// <summary>
    /// LevelsTestCondition version of axis.
    /// </summary>
    [Serializable]
    internal class LevelAxis : TestConditionAxis
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LevelAxis"/> class.
        /// </summary>
        /// <param name="axisParameter">name of parameter that axis is made of.</param>
        /// <param name="testConditionName">input levels test condition.</param>
        /// <param name="unitPrefixForDatalog">scale for the axis.</param>
        /// <param name="nameForDatalog">name of the axis during datalog prints.</param>
        public LevelAxis(string axisParameter, string testConditionName, PrimeShmooTestMethod.UnitPrefixForDatalog unitPrefixForDatalog, string nameForDatalog)
            : base(axisParameter, testConditionName, unitPrefixForDatalog, nameForDatalog)
        {
        }

        /// <inheritdoc/>
        public override void ApplyTestCondition(IFunctionalTest functionalTest)
        {
            functionalTest.ApplyLevelTestCondition();
        }

        /// <inheritdoc/>
        public override string GetUnit()
        {
            return "V";
        }
    }
}
