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

namespace FIVRTrimTC
{
    using Prime.PhAttributes;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class FIVRTrimTC : TestMethodBase
    {
        /// <summary>Enum to hold bool value for Parameters.</summary>
        public enum MyBool
        {
            /// <summary>true.</summary>
            True,

            /// <summary>false.</summary>
            False,
        }

        /// <summary>Gets or sets whether to enable Sort/Class Delta calculations.</summary>
        public MyBool EnableSortClassDelta { get; set; } = MyBool.False;

        /// <summary>Gets or sets whether results should be logged to DFF or not.</summary>
        public MyBool EnableDFF { get; set; } = MyBool.True;

        /// <summary>Gets or sets whether this is a sort test socket.</summary>
        public MyBool IsSort { get; set; } = MyBool.False;

        /// <summary>Gets or sets whether results should be written to GSDS tokens.</summary>
        public MyBool EnableGSDS { get; set; } = MyBool.True;

        /// <summary>Gets or sets the alternative tag used for Ituff Logging.</summary>
        public TestMethodsParams.String AltTagID { get; set; } = string.Empty;

        /// <summary>Gets or sets whether failures should result in a failing exit port.</summary>
        public MyBool EnableTrimKill { get; set; } = MyBool.True;

        private FivrTrim FIVRObj { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            Prime.Services.ConsoleService.PrintDebug($"Instance=[{this.InstanceName}] Now reading parameters.");
            var scDeltaEn = this.EnableSortClassDelta == MyBool.True;
            var dffEn = this.EnableDFF == MyBool.True;
            var isSort = this.IsSort == MyBool.True;
            var gsdsEn = this.EnableGSDS == MyBool.True;
            var trimKill = this.EnableTrimKill == MyBool.True;
            var debug = this.LogLevel != PrimeLogLevel.DISABLED;

            Prime.Services.ConsoleService.PrintDebug($"Instance=[{this.InstanceName}] Now initializing FivrTrim Object.");
            this.FIVRObj = new FivrTrim(scDeltaEn, dffEn, isSort, gsdsEn, this.AltTagID.ToString(), trimKill, debug);

            Prime.Services.ConsoleService.PrintDebug($"Instance=[{this.InstanceName}] Done with Verify.");
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        [Returns(2, PortType.Fail, "Fail BG Trim")]
        [Returns(3, PortType.Fail, "Fail VCOR Trim")]
        [Returns(4, PortType.Fail, "Fail VCO Trim")]
        [Returns(5, PortType.Fail, "Fail CPS Trim")]
        [Returns(6, PortType.Fail, "Fail PWM Trim")]
        [Returns(7, PortType.Fail, "Fail VTG Trim")]
        [Returns(8, PortType.Fail, "Fail DAC Trim")]
        [Returns(9, PortType.Fail, "Fail CSR Trim")]
        public override int Execute()
        {
            Prime.Services.ConsoleService.PrintDebug($"Instance=[{this.InstanceName}] Now running Trim Calculations.");
            var exitPort = this.FIVRObj.FIVRTrimCalc();

            Prime.Services.ConsoleService.PrintDebug($"Instance=[{this.InstanceName}] Trim Calculation returned exitPort=[{exitPort}].");

            return exitPort;
        }
    }
}