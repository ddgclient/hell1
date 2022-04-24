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

namespace LSARasterTC.UnitTest.MockObjects
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;

    /// <summary>
    /// Represents a PlistHandle that Prime services would return.
    /// </summary>
    public class FakeStrgvalFormat : IStrgvalFormat
    {
        /// <summary>
        /// Gets a string representing this IStrgval data.
        /// </summary>
        public string Data { get; private set; }

        /// <summary>
        /// Gets a string representing this IStrgval postfix.
        /// </summary>
        public string PostFix { get; private set; }

        /// <inheritdoc/>
        public void ApplyFormatting()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void ForceAries()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void ForceItuffLevel(string level)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public StringBuilder GetStream()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetCustomTname(string tname)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetData(string strgvalValue)
        {
            this.Data = strgvalValue;
        }

        /// <inheritdoc/>
        public void SetDelimiterCharacterForWrap(char delimiter)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetMaxLineLength(uint length)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetMsUnitAttributes(Unit unit)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetTnamePostfix(string postfix)
        {
            this.PostFix = postfix;
        }

        /// <inheritdoc/>
        public void SetTnamePrefix(string prefix)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetTssidAttributes(string data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public string ToFormattedString()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.Data}_{this.PostFix}";
        }
    }
}
