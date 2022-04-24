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

namespace SIO
{
    using System;
    using System.Collections.Generic;

    /// <inheritdoc/>
    public class RegDef_DP : IRegDef
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegDef_DP"/> class.
        /// </summary>
        /// <param name="utils">SIOEDC_Util object.</param>
        public RegDef_DP(SIOEDC_Util utils)
        {
            this.Utils = utils;
            this.FuncMap = new Dictionary<string, Func<SIOEDC_Util.HashedData, string, string, string>>
            {
                { "DP_LCE_TEST_STATUS", this.Dp_lce_test_status },
                { "DP_LCE_TEST_STATUS_PERLANE", this.Dp_lce_test_status_perlane },
            };
        }

        private Dictionary<string, Func<SIOEDC_Util.HashedData, string, string, string>> FuncMap { get; set; }

        private SIOEDC_Util Utils { get; set; }

        /// <inheritdoc/>
        bool IRegDef.Contains(string port, string lane, string field)
        {
            return this.FuncMap.ContainsKey(field.ToUpper());
        }

        /// <inheritdoc/>
        string IRegDef.GetData(SIOEDC_Util.HashedData dataHash, string port, string lane, string field)
        {
            // FIXME: detect error condition
            var retval = this.FuncMap[field.ToUpper()](dataHash, port, lane);
            return retval;
        }

        private string Dp_lce_test_status(SIOEDC_Util.HashedData dataHash, string port, string lane)
        {
            var output = "P";
            return output;
        }

        private string Dp_lce_test_status_perlane(SIOEDC_Util.HashedData dataHash, string port, string lane)
        {
            var lceStatus_lsb = 9;
            var lceStatus_msb = 13;
            var lceErr_lsb = 17;
            var lceErr_msb = 32;
            var lceStatus_pass = "01110"; // "0xe";
            var lceErr_pass = "0000000000000000"; // "0x0";

            var lcecap = dataHash.Get(port, lane, "LCECAP");
            var lceStatus_dataStr = this.Utils.Get_bit_field(lcecap, lceStatus_lsb, lceStatus_msb);
            var lceErr_dataStr = this.Utils.Get_bit_field(lcecap, lceErr_lsb, lceErr_msb);

            var output = "F";
            if (lceStatus_dataStr == lceStatus_pass && lceErr_dataStr == lceErr_pass)
            {
                output = "P";
            }

            return output;

            // python code
            // LceStatus_data = hex(bitfield(datahash[port][lane]['LCECAP'], LceStatus_lsb, LceStatus_msb))
            // LceErr_data = hex(bitfield(datahash[port][lane]['LCECAP'], LceErr_lsb, LceErr_msb))
            // if LceStatus_data == LceStatus_pass and LceErr_data == LceErr_pass:
            //     output = "P"
            // else:
            //     output = "F"
        }
    }
}
