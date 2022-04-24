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
    using Prime.PatConfigService;

    /// <summary>
    /// Represents a PlistHandle that Prime services would return.
    /// </summary>
    public class FakePlistHandle : IPatConfigHandle
    {
        private ulong expectedDataSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakePlistHandle"/> class.
        /// </summary>
        /// <param name="expectedDataSize"> Sets the expected data size for this <see cref="IPatConfigHandle"/>. </param>
        public FakePlistHandle(ulong expectedDataSize)
        {
            this.expectedDataSize = expectedDataSize;
        }

        /// <summary>
        /// Gets data for this PatConfigHandle.
        /// </summary>
        public string Data { get; private set; }

        /// <inheritdoc/>
        public void FillData(PatternSymbol symbol)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void FillData(PatternSymbol symbol, ulong subConfigIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void FillData(PatternSymbol symbol, string subConfigName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public string GetConfigurationName()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ulong GetExpectedDataSize()
        {
            return this.expectedDataSize;
        }

        /// <inheritdoc/>
        public ulong GetExpectedDataSize(ulong subConfigIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ulong GetExpectedDataSize(string subConfigName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ulong GetNumberOfConfigurationElements()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public HashSet<string> GetPatternsName()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool IsDataSet()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void ResetToDefault()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void ResetToDefault(ulong subConfigIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void ResetToDefault(string subConfigName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetData(string data)
        {
            this.Data = data;
        }

        /// <inheritdoc/>
        public void SetData(string data, ulong subConfigIndex)
        {
            this.Data = data;
        }

        /// <inheritdoc/>
        public void SetData(string data, string subConfigName)
        {
            throw new NotImplementedException();
        }
    }
}
