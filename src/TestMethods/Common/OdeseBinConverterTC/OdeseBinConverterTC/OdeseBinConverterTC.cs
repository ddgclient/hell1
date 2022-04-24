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

namespace OdeseBinConverterTC
{
    using System.Linq;
    using DDG;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.PhAttributes;
    using Prime.TestMethods.OdeseBinConverter;

    /// <summary>
    /// Test method responsible for executing different variations of functional test.
    /// </summary>
    [PrimeTestMethod]
    public class OdeseBinConverterTC : PrimeOdeseBinConverterTestMethod, IOdeseBinConverterExtensions
    {
        /// <summary>
        /// Gets or sets interface to skip console printing when LogLevel is not enabled.
        /// </summary>
        public IConsoleService Console { get; set; } = null;

        /// <inheritdoc />
        uint IOdeseBinConverterExtensions.ConvertSoftBin(uint currentSoftbin)
        {
            var currentBin = currentSoftbin.ToString();
            if (currentBin.Length <= 4)
            {
                return currentSoftbin;
            }

            var simplifiedBinningIndexesForPassingBin = Prime.Services.UserVarService.GetStringValue("RunTimeLibraryVars", "iCGL_PrimeSimplifiedBinningIndexesForPassingBin").Split(',');
            var simplifiedBinningIndexesForFailingBin = Prime.Services.UserVarService.GetStringValue("RunTimeLibraryVars", "iCGL_PrimeSimplifiedBinningIndexesForFailingBin").Split(',');

            var newBin = currentBin[0] == '9' ?
                simplifiedBinningIndexesForFailingBin.Aggregate(string.Empty, (current, c) => current + currentBin[c.ToInt()]) :
                simplifiedBinningIndexesForPassingBin.Aggregate(string.Empty, (current, c) => current + currentBin[c.ToInt()]);

            var uIntBin = (uint)newBin.ToInt();
            this.Console?.PrintDebug($"Converting Softbin=[{currentSoftbin}] to New Softbin=[{uIntBin}]");
            return uIntBin;
        }
    }
}