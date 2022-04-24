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
    using Prime.FunctionalService;

    /// <summary>
    /// IShmooAxis class is used to determine what actions are taken during execution.
    /// </summary>
    public interface IShmooAxis
    {
        /// <summary>
        /// Gets original value for the axis parameter as a string.
        /// </summary>
        string OriginalValue { get; }

        /// <summary>
        /// Gets name of the axis. Usage differs depending on axis type.
        /// </summary>
        string AxisName { get; }

        /// <summary>
        /// Gets name of the axis used for ituff prints. Used when lines are too long for tname.
        /// </summary>
        string AxisNameForDatalog { get; }

        /// <summary>
        /// Gets the scale used for prints for the axis.
        /// </summary>
        PrimeShmooTestMethod.UnitPrefixForDatalog UnitPrefixForDatalog { get; }

        /// <summary>
        /// Gets previous value run on the axis.
        /// </summary>
        double PreviousValue { get; }

        /// <summary>
        /// Runs during verify.
        /// </summary>
        /// <returns>false if this point should be skipped, true otherwise.</returns>
        bool Verify();

        /// <summary>
        /// Runs before each point is executed. Sets the TC spec value of X and Y if they are defined.
        /// </summary>
        /// <param name="axisValue">current point.</param>
        /// <param name="functionalTest">functionalTest of the executed plist.</param>
        /// <returns>false if this point should be skipped, true otherwise.</returns>
        bool PrePointExecute(string axisValue, IFunctionalTest functionalTest);

        /// <summary>
        /// Runs after each point is executed. returns false if multiple defects captured for point. true otherwise.
        /// </summary>
        /// <returns>false if multiple defects captured for point. true otherwise.</returns>
        /// <param name="point">current point.</param>
        /// <param name="functionalTest">funcTest of the executed plist.</param>
        bool PostPointExecute(ShmooPoint point, IFunctionalTest functionalTest);

        /// <summary>
        /// Runs after execute for cleanup/print functions.
        /// </summary>
        void PostExecute();

        /// <summary>
        /// Runs before all shmoo points have been executed.
        /// </summary>
        /// <returns>True if successfully executed, false otherwise.</returns>
        bool PreExecute();

        /// <summary>
        /// Returns units for type of test condition.
        /// </summary>
        /// <returns>Unit to be printed to ituff.</returns>
        string GetUnit();

        /// <summary>
        /// Applies the given axis value to the axis.
        /// </summary>
        /// <param name="axisValue">Value to be applied.</param>
        /// <param name="functionalTest">functionalTest to be applied.</param>
        /// <returns>True if successfully executed, false otherwise.</returns>
        bool Apply(double axisValue, IFunctionalTest functionalTest);
    }
}
