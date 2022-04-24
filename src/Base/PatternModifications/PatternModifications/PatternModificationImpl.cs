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

namespace DDG
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Prime;
    using Prime.PatConfigService;

    /// <inheritdoc />
    internal class PatternModificationImpl : IPatternModification
    {
        /// <inheritdoc />
        public List<IPatConfigHandle> GetPatternConfigHandles(string input, string patList)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                if (string.IsNullOrEmpty(input))
                {
                    return null;
                }

                var result = new List<IPatConfigHandle>();
                var patConfigPairs = input.Split(',').ToList();
                foreach (var patConfigPair in patConfigPairs)
                {
                    var configurationTuple = patConfigPair.Trim().Split(':');
                    if (configurationTuple.Length != 2)
                    {
                        throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: Invalid PatConfig input=[{input}]");
                    }

                    var handle = this.GetPatternConfigHandle(configurationTuple[0], configurationTuple[1], patList);
                    result.Add(handle);
                }

                return result;
            }
        }

        /// <inheritdoc />
        public void ApplyPatternConfigHandles(List<IPatConfigHandle> handles)
        {
            using (var unused = Prime.Services.PerformanceService.GetStopWatch(string.Empty))
            {
                if (handles != null && handles.Count > 0)
                {
                    Services.PatConfigService.Apply(handles);
                }
            }
        }

        private IPatConfigHandle GetPatternConfigHandle(string configuration, string data, string patList)
        {
            var handle = string.IsNullOrEmpty(patList) ? Services.PatConfigService.GetPatConfigHandle(configuration) : Services.PatConfigService.GetPatConfigHandleWithPlist(configuration, patList);
            var expectedDataSize = handle.GetExpectedDataSize();
            data = data.GetPatSymbolString((int)expectedDataSize);
            handle.SetData(data);
            return handle;
        }
    }
}
