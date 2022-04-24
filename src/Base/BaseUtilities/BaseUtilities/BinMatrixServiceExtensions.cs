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
    using System.Linq;
    using System.Text.RegularExpressions;
    using Prime.BinMatrixService;

    /// <summary>
    /// Defines the <see cref="BinMatrixServiceExtensions" />.
    /// </summary>
    public static class BinMatrixServiceExtensions
    {
        /// <summary>
        /// Evaluates BinMatrix spec values for tokens encapsulated with {}.
        /// </summary>
        /// <param name="service">Service interface.</param>
        /// <param name="input">String to be evaluated..</param>
        /// <param name="flowNumber">Flow number. When is not specified the method will try to get current flow number from test instance parameter.</param>
        /// <returns>Evaluated string.</returns>
        public static string EvaluateString(this IBinMatrixService service, string input, int flowNumber = 0)
        {
            Regex rgx = new Regex(@"\{\S+\}");
            if (!rgx.IsMatch(input))
            {
                return input;
            }

            if (flowNumber < 1)
            {
                flowNumber = Prime.Services.TestProgramService.GetCurrentFlowNumber();
            }

            var specList = rgx.Matches(input).Cast<Match>().Select(m => m.Value).ToList();
            foreach (var spec in specList)
            {
                var specName = spec.TrimStart('{').TrimEnd('}');
                var specInfo = Prime.Services.BinMatrixService.GetSpecInfo(flowNumber, specName);
                var value = specInfo.GetData() + specInfo.GetUnit();
                input = input.Replace(spec, value);
            }

            return input;
        }
    }
}