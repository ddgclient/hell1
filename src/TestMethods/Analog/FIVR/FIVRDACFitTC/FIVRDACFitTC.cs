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

namespace FIVRDACFitTC
{
    using System;
    using Prime.PhAttributes;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class FIVRDACFitTC : TestMethodBase
    {
        /// <summary>Enum to hold the temperature information.</summary>
        public enum Temperature
        {
            /// <summary>HOT Location.</summary>
            HOT,

            /// <summary>NOT_HOT Location.</summary>
            NOT_HOT,
        }

        /// <summary>Enum to hold the Location to populate DFF.</summary>
        public enum DFFLocn
        {
            /// <summary>WFR Location.</summary>
            WFR = -98,

            /// <summary>PKG Location.</summary>
            PKG = -99,
        }

        /// <summary>Enum to hold bool value for Parameters.</summary>
        public enum MyBool
        {
            /// <summary>true.</summary>
            True,

            /// <summary>false.</summary>
            False,
        }

        /// <summary>Gets or sets Location Type (currently not used).</summary>
        public Temperature Location { get; set; } = Temperature.HOT;

        /// <summary>Gets or sets OpType for getting DFF data (current not used).</summary>
        public TestMethodsParams.String OpType { get; set; } = "PBIC_DAB";

        /// <summary>Gets or sets Location for writing DFF (current not used).</summary>
        public DFFLocn DFFWriteLocn { get; set; } = DFFLocn.PKG;

        /// <summary>Gets or sets AnalogMeasure(true)/CMEM(false) measurements selector.</summary>
        public MyBool UseAMMeas { get; set; } = MyBool.True;

        /// <summary>Gets or sets whether results should be logged to DFF or not.</summary>
        public MyBool LogDFF { get; set; } = MyBool.True;

        /// <summary>Gets or sets whether default DFF data should be logged on an error.</summary>
        public MyBool SetDefaultDFFOnError { get; set; } = MyBool.True;

        private FivrOps FIVRObj { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            // verify all parameters have values.  Since all but OpType are enums that's the only one to check.
            if (this.OpType == string.Empty && this.Location == Temperature.NOT_HOT)
            {
                throw new ArgumentException($"Paramter Optype must be set when Location is NOT_HOT.");
            }

            this.FIVRObj = new FivrOps(this.Location.ToString(), this.OpType.ToString(), (int)this.DFFWriteLocn, this.UseAMMeas == MyBool.True);
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            try
            {
                if (this.FIVRObj.FIVRDACFitCalculate(this.LogDFF == MyBool.True))
                {
                    Prime.Services.ConsoleService.PrintDebug("FIVRDACFitCalculate ran successfully.");
                    return 1;
                }
                else
                {
                    Prime.Services.ConsoleService.PrintDebug("FIVRDACFitCalculate failed.");
                    return 0;
                }
            }
            catch (Exception e)
            {
                Prime.Services.ConsoleService.PrintError($"Instance=[{this.InstanceName}] Caught an exception - {e.Message}");
                if (this.SetDefaultDFFOnError == MyBool.True)
                {
                    this.FIVRObj.ForceDefault_On_Exception();
                }

                return 0;
            }
        }
    }
}