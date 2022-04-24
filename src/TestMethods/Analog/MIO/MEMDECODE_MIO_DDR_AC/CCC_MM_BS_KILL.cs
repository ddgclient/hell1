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

namespace MEMDECODE_MIO_DDR_AC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DDG;
    using Prime.Base.Exceptions;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;

    /// <summary>
    /// This class is intended to overwrite the members of the IFunctionalExtensions interfaces to extend the test method PrimeFuncCaptureCtvTestMethod.
    /// </summary>
    public class CCC_MM_BS_KILL : PrimeFunctionalTestMethod, IFunctionalExtensions
    {
        /// <inheritdoc/>
        bool IFunctionalExtensions.ProcessCtvPerPin(Dictionary<string, string> ctvData)
        {
            // only expecting 1 pin to be captured.
            var keys = ctvData.Keys.ToList();
            Prime.Services.ConsoleService.PrintDebug($"[CCC_MM_BS_KILL.ProcessCtvPerPin] Capture CTV Data for Pins=[{string.Join(", ", keys)}].");
            var binData = ctvData[keys[0]];
            Prime.Services.ConsoleService.PrintDebug($"[CCC_MM_BS_KILL.ProcessCtvPerPin] Capture CTV Data=[{binData}].");

            // Check the length
            if (binData.Length != 9216)
            {
                throw new TestMethodException($"CCC_MM_BS_KILL expecting 9216 capture bits, got {binData.Length} bits on {keys[0]}.");
            }

            // Decode all the data.
            var allPass = true;
            allPass &= CCC_MM(binData, name: "pi3", offset: 0, tc: 1, checkLimits: true);
            allPass &= CCC_MM(binData, name: "pi0", offset: 1536, tc: 2, checkLimits: true);
            allPass &= CCC_MM(binData, name: "pi1", offset: 3072, tc: 3, checkLimits: true);
            allPass &= CCC_MM(binData, name: "pi2", offset: 4608, tc: 4, checkLimits: true);
            allPass &= CCC_MM(binData, name: "pi4", offset: 6144, tc: 5, checkLimits: true);
            CCC_MM(binData, name: "rxvref", offset: 7680, tc: 0, checkLimits: false);

            return allPass;
        }

        private static bool CCC_MM(string binaryData, string name, int offset, int tc, bool checkLimits = true)
        {
            List<int> minVals = new List<int>();
            List<int> maxVals = new List<int>();

            // Extract all the results.
            for (var ccc = 0; ccc < 8; ccc++)
            {
                minVals.Add(binaryData.ExtractBits("10-4", offset + (ccc * 192)).BinaryToInteger());
                maxVals.Add(binaryData.ExtractBits("17-11", offset + (ccc * 192)).BinaryToInteger());
            }

            // Log the results to Ituff.
            MemDecode.WriteStrgvalToItuff($"MaxVal_TC{tc}", string.Join("|", maxVals));
            MemDecode.WriteStrgvalToItuff($"MminVal_TC{tc}", string.Join("|", minVals));

            // Calculate pass/fail based on the extracted results.
            var retval = true;
            if (checkLimits && maxVals.Min() < 1)
            {
                Prime.Services.ConsoleService.PrintError($"CCC_MM: Name={name} Offset={offset} TC={tc} MaxVal violates minimum_limit of 1 [{string.Join(",", maxVals)}]");
                retval = false;
            }

            return retval;
        }
    }
}
