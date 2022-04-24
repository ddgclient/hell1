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

namespace VoltageConverterCallbacks
{
    using DDG;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    public class VoltageConverterCallbacks
    {
        /// <summary>
        /// Callback function to apply voltage converter.
        /// Function should run before plist execution.
        /// </summary>
        /// <param name="args">Argument String.</param>
        public static void VoltageConverter(string args)
        {
            var levelsTc = Prime.Services.TestProgramService.GetCurrentLevels();

            if (args.Contains("fivrcondition"))
            {
                var patlists = Prime.Services.TestProgramService.GetCurrentPatternLists();
                foreach (var patlist in patlists)
                {
                    var voltageObject = DDG.VoltageHandler.GetVoltageObject(null, levelsTc, patlist, null, null, args, out var options);
                    var overrides = DDG.VoltageHandler.GetVoltageOverrides(options);
                    DDG.VoltageHandler.ApplyInitialVoltage(voltageObject, levelsTc, overrides);
                }
            }
            else
            {
                var voltageObject = DDG.VoltageHandler.GetVoltageObject(null, levelsTc, null, null, null, args, out var options);
                var overrides = DDG.VoltageHandler.GetVoltageOverrides(options);
                DDG.VoltageHandler.ApplyInitialVoltage(voltageObject, levelsTc, overrides);
            }
        }
    }
}
