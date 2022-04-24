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

namespace VminForwardingSaveFakeDataTC
{
    using System;
    using Prime.PhAttributes;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class VminForwardingSaveFakeDataTC : TestMethodBase
    {
        /// <summary>
        /// Gets or sets the List of Domains.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString Domains { get; set; }

        /// <summary>
        /// Gets or sets the Frequency Corner.
        /// </summary>
        public TestMethodsParams.String FrequencyCorner { get; set; }

        /// <summary>
        /// Gets or sets the Flow ID.
        /// </summary>
        public TestMethodsParams.Integer FlowId { get; set; }

        /// <summary>
        /// Gets or sets the List of Vmin Results.
        /// </summary>
        public TestMethodsParams.CommaSeparatedDouble VminResults { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            var failures = 0;
            if (this.Domains == null || string.IsNullOrEmpty(this.Domains))
            {
                Prime.Services.ConsoleService.PrintError("Parameter=[Domains] is required.");
                failures++;
            }

            if (this.VminResults == null || string.IsNullOrEmpty(this.VminResults))
            {
                Prime.Services.ConsoleService.PrintError("Parameter=[VminResults] is required.");
                failures++;
            }

            if (this.FrequencyCorner == null || string.IsNullOrEmpty(this.FrequencyCorner))
            {
                Prime.Services.ConsoleService.PrintError("Parameter=[FrequencyCorner] is required.");
                failures++;
            }

            if (this.FlowId == null || this.FlowId <= 0)
            {
                Prime.Services.ConsoleService.PrintError("Parameter=[FlowId] is required to be > 0.");
                failures++;
            }

            if (failures > 0)
            {
                throw new Exception($"{this.InstanceName} failed Verify.");
            }
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            var corner = DDG.VminForwarding.Service.Get(this.Domains.ToString(), this.FrequencyCorner.ToString(), this.FlowId);
            corner.StoreVminResult(this.VminResults.ToList());
            return 1;
        }
    }
}
