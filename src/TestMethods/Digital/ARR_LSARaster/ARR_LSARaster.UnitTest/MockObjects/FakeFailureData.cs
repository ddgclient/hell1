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
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Prime.FunctionalService;

    /// <summary>
    /// Object that implements <see cref="IFailureData"/> for use during unit tests.
    /// </summary>
    public class FakeFailureData : IFailureData
    {
        private string patternName;
        private string domainName;
        private uint vectorAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeFailureData"/> class.
        /// </summary>
        /// <param name="patternName"> Name of pattern for this fail. </param>
        /// <param name="domainName"> Name of domain for this fail. </param>
        /// <param name="vectorAddress"> Vector that this fail is located. </param>
        public FakeFailureData(string patternName, string domainName, uint vectorAddress)
        {
            this.patternName = patternName;
            this.domainName = domainName;
            this.vectorAddress = vectorAddress;
        }

        /// <summary>
        /// Gets or sets name of plist this fail belongs to.
        /// </summary>
        public string PlistName { get; set; }

        /// <inheritdoc/>
        public ulong GetBurstIndex()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ulong GetCycle()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ulong GetCyclesFromPreviousLabel()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public string GetDomainName()
        {
            return this.domainName;
        }

        /// <inheritdoc/>
        public List<uint> GetFailingPinChannels()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public List<string> GetFailingPinNames()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public string GetParentPlistName()
        {
            return this.PlistName;
        }

        /// <inheritdoc/>
        public ulong GetPatternInstanceId()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public string GetPatternName()
        {
            return this.patternName;
        }

        /// <inheritdoc/>
        public string GetPreviousLabel()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public string GetSubroutinePattern()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ulong GetVectorAddress()
        {
            return this.vectorAddress;
        }

        /// <inheritdoc/>
        public bool IsSubroutine()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        ulong IPerCycleData.GetBurstCycle()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        ulong IPerCycleData.GetTraceLogCycle()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        ulong IPerCycleData.GetTraceLogRegister1()
        {
            throw new NotImplementedException();
        }
    }
}
