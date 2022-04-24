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
    /// EmptyShmooAxis axis used when no parameter is provided.
    /// </summary>
    [Serializable]
    internal class EmptyShmooAxis : IShmooAxis
    {
        /// <inheritdoc/>
        public string OriginalValue => string.Empty;

        /// <inheritdoc/>
        public string AxisName => string.Empty;

        /// <inheritdoc/>
        public string AxisNameForDatalog => string.Empty;

        /// <inheritdoc/>
        public PrimeShmooTestMethod.UnitPrefixForDatalog UnitPrefixForDatalog => PrimeShmooTestMethod.UnitPrefixForDatalog.Base;

        /// <inheritdoc/>
        public double PreviousValue => double.NaN;

        /// <inheritdoc/>
        public bool Verify()
        {
            return true;
        }

        /// <inheritdoc/>
        public bool PrePointExecute(string axisValue, IFunctionalTest functionalTest)
        {
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
            // Empty shmoo should do nothing here.
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
