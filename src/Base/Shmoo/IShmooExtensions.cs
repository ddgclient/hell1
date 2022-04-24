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
    using System.Collections;
    using System.Collections.Generic;
    using Prime.FunctionalService;
    using Prime.VoltageService;

    /// <summary>
    /// Class to define extendable methods.
    /// </summary>
    public interface IShmooExtensions
    {
        /// <summary>
        /// Method to be implemented by user to return all the points in the x-axis range that TestMethod should iterate over.
        /// </summary>
        /// <param name="pointValue">string from test method parameter "XAxisRange". It usually contains the needed information
        /// so user can generate the list of double points for this axis. User is responsible for string format.</param>
        /// <returns>List of double points values for x-axis to iterate over.</returns>
        List<double> GetXAxisPoints(string pointValue);

        /// <summary>
        /// Method to be implemented by user to return all the points in the y-axis range that TestMethod should iterate over.
        /// </summary>
        /// <param name="pointValue">string from test method parameter "YAxisRange". It usually contains the needed information
        /// so user can generate the list of double points for this axis. User is responsible for string format.</param>
        /// <returns>List of double points values for y-axis to iterate over.</returns>
        List<double> GetYAxisPoints(string pointValue);

        /// <summary>
        /// Extension to allow modifying of plist execution settings, for example set capture pins or disable incremental mode (start from last failing pattern).
        /// </summary>
        /// <param name="patlist">The plist name to build the IFunctionalTest.</param>
        /// <param name="levelsTc">The levels test condition name to build the IFunctionalTest.</param>
        /// <param name="timingsTc">The timings test conditions name name to build the IFunctionalTest.</param>
        /// <param name="prePlist">The callback to run before the plist execution.</param>
        /// <returns>The IFunctionalTest object build as per user requirements.</returns>
        IFunctionalTest GetFunctionalTest(string patlist, string levelsTc, string timingsTc, string prePlist);

        /// <summary>
        /// Extension to allow user to run some code prior to plist execution on specific shmoo point. The point is passed as a function parameter.
        /// </summary>
        /// <param name="point">Shmoo point to apply.</param>
        /// <param name="functionalTest">Func test object that contains execution data.</param>
        /// <returns>true if apply succeeded and this point is relevant for the test, and false if this point need to be skipped.</returns>
        bool PrePointExecute(ShmooPoint point, IFunctionalTest functionalTest);

        /// <summary>
        /// Extension to allow user to run some code after plist execution on specific shmoo point. The point is passed as a function parameter.
        /// </summary>
        /// /// <param name="point">Current shmoo point.</param>
        /// <param name="functionalTest">Func test object that contains execution data.</param>
        void PostPointExecute(ShmooPoint point, IFunctionalTest functionalTest);

        /// <summary>
        ///  Extension allow user to override the code that runs on a specific shmoo point. By default, the provided plist will be executed.
        ///   The return value of this function will impact the exit port of the instance. If the user returns 'false'
        ///  value on at least one shmoo point, then the instance will exit from port 0. Otherwise, the template will exit from port 1.
        /// </summary>
        /// <param name="point">Current shmoo point.</param>
        /// <returns>True if the execution pass, otherwise false.</returns>
        bool PointExecute(ShmooPoint point);

        /// <summary>
        /// Extension to allow user to run some code after all shmoo points have executed.
        /// </summary>
        /// <param name="functionalTest">Func test object that contains execution data.</param>
        void PostExecute(IFunctionalTest functionalTest);

        /// <summary>
        /// Extension to allow user to run some code before any shmoo points have executed.
        /// </summary>
        /// <param name="functionalTest">Func test object that contains execution data.</param>
        void PreExecute(IFunctionalTest functionalTest);

        /// <summary>
        /// Gets the list of pins to mask execution after execution. The test method will merge this list with the ones from the test instance parameter.
        /// </summary>
        /// <returns>The list of mask pins.</returns>
        List<string> GetDynamicPinMask();
    }
}
