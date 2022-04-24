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

namespace MEMDECODE_CLK_FLL_ALL
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using DDG;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;

    /// <summary>
    /// Contains the CMEM Decode functions for the MEMDECODE_CLK_FLL_ALL module.
    /// </summary>
    public class Lock_tgl42uy : PrimeFunctionalTestMethod, IFunctionalExtensions
    /* public static class MEMDECODE_CLK_FLL_ALL */
    {
        /*
        /// <summary>
        /// Initializes a new instance of the <see cref="Lock_tgl42uy"/> class.
        /// </summary>
        public Lock_tgl42uy()
        {
            Prime.Services.ConsoleService.PrintDebug($"Executing Lock_tgl42uy Constructor.");
        } */

        private static int TAP_FAIL_PORT { get; } = 2;

        private static int RESET_FAIL_PORT { get; } = 3;

        private static int PASS_PORT { get; } = 1;

        private static OrderedDictionary FLL_STATUS_REGISTER { get; } = new OrderedDictionary
        {
            // .     Field                                       Number of Bits
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
#pragma warning disable SA1001 // Commas should be spaced correctly
            { "fll_extip_stat_reg_fll_enable"                   ,  "1" },
            { "fll_extip_stat_reg_dist_reset_b"                 ,  "1" },
            { "fll_extip_stat_reg_fll_early_lock"               ,  "1" },
            { "fll_extip_stat_reg_fll_final_lock"               ,  "1" },
            { "fll_extip_stat_reg_ramp_done"                    ,  "1" },
            { "fll_extip_stat_reg_freq_change_req"              ,  "1" },
            { "fll_extip_stat_reg_freq_change_done"             ,  "1" },
            { "fll_extip_stat_reg_reg_writes_done"              ,  "1" },
            { "fll_extip_stat_reg_vbias_code_2fllcore"          , "16" },
            { "fll_extip_stat_reg_fll_ratio"                    ,  "8" },
            { "fll_diag_stat_reg_fll_refclk_req"                ,  "1" },
            { "fll_diag_stat_reg_fll_refclk_ack"                ,  "1" },
            { "fll_diag_stat_reg_fll_outclk_req"                ,  "1" },
            { "fll_diag_stat_reg_fll_outclk_ack"                ,  "1" },
            { "fll_diag_stat_reg_fll_counter_err"               , "12" },
            { "fll_diag_stat_reg_fll_counter_sum"               , "12" },
            { "fll_diag_stat_reg_hv_pwr_good"                   ,  "1" },
            { "fll_diag_stat_reg_ldoen_dly_cntr_done"           ,  "1" },
            { "fll_diag_stat_reg_fastrampdone_cntr_done"        ,  "1" },
            { "fll_diag_stat_reg_ldo_ramp_timer_done"           ,  "1" },
            { "fll_diag_stat_1_reg_fll_counter_ovf"             , "10" },
            { "fll_diag_stat_1_reg_counter_overflow_sticky"     ,  "1" },
            { "fll_diag_stat_1_reg_measurement_enable_dly_ver"  ,  "1" },
            { "fll_diag_stat_1_reg_reserved_diag_sts_1"         , "20" },
            { "fll_diag_stat_2_reg_reserved_diag_sts_2"         , "32" },
#pragma warning restore SA1001 // Commas should be spaced correctly
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row
        };

        /// <inheritdoc/>
        bool IFunctionalExtensions.ProcessCtvPerPin(Dictionary<string, string> ctvData)
        {
            Prime.Services.ConsoleService.PrintDebug($"Executing Lock_tgl42uy.ProcessCtvPerPin");

            // only expecting 1 pin to be captured.
            var keys = ctvData.Keys.ToList();

            Prime.Services.ConsoleService.PrintDebug($"[Lock_tgl42uy.ProcessCtvPerPin] Capture CTV Data for Pins=[{string.Join(", ", keys)}].");
            var binaryData = ctvData[keys[0]];
            Prime.Services.ConsoleService.PrintDebug($"[Lock_tgl42uy.ProcessCtvPerPin] Capture CTV Data=[{binaryData}].");
            if (string.IsNullOrEmpty(binaryData))
            {
                // FIXME - HACK to do offline validation
                binaryData = new string('0', 128 * (18 + 23 + 18 + 23));
                Prime.Services.ConsoleService.PrintDebug($"[Lock_tgl42uy.ProcessCtvPerPin] VALIDATION HACK, replaced Capture CTV Data=[{binaryData}].");
            }

            var fll133_start = 0;
            var fll133_size = 18 * 128;
            var fll100_start = fll133_start + fll133_size;
            var fll100_size = 23 * 128;
            var fll066_start = fll100_start + fll100_size;
            var fll066_size = 18 * 128;
            var fll050_start = fll066_start + fll066_size;
            var fll050_size = 23 * 128;

            if (binaryData.Length < (fll050_start + fll050_size))
            {
                Prime.Services.ConsoleService.PrintError($"[Lock_tgl42uy.ProcessCtvPerPin] Not enough data.  Got [{binaryData.Length}] bits, expecting [{fll050_start + fll050_size}] bits.");
                return false;
            }

            var fll133_exit = LockWrapper(binaryData.Substring(fll133_start, fll133_size), clock: "FLL133", fail_port: 4, freq_count: 18, l_lim: 08, u_lim: 25, first_ratio: 08, ratio_incr: 1);
            var fll100_exit = LockWrapper(binaryData.Substring(fll100_start, fll100_size), clock: "FLL100", fail_port: 4, freq_count: 23, l_lim: 11, u_lim: 33, first_ratio: 11, ratio_incr: 1);
            var fll066_exit = LockWrapper(binaryData.Substring(fll066_start, fll066_size), clock: "FLL066", fail_port: 4, freq_count: 18, l_lim: 08, u_lim: 25, first_ratio: 08, ratio_incr: 1);
            var fll050_exit = LockWrapper(binaryData.Substring(fll050_start, fll050_size), clock: "FLL050", fail_port: 4, freq_count: 23, l_lim: 11, u_lim: 33, first_ratio: 11, ratio_incr: 1);

            // FIXME - not sure the correct way to resolve this.
            // var exit_port = new List<int> { fll133_exit, fll100_exit, fll066_exit, fll050_exit }.Max();
            return true;
        }

        private static int LockWrapper(string binaryData, string clock, int fail_port, int freq_count, int l_lim, int u_lim, int first_ratio, int ratio_incr)
        {
            var vbiasAll = new List<int>();
            var counterrAll = new List<int>();
            var countsumAll = new List<int>();

            // FIXME the extra -1 is there to match the evg implentation, but it seems unnecessary.
            vbiasAll.Add(-1);
            counterrAll.Add(-1);
            countsumAll.Add(-1);

            // set to true if this dut violates any of the limits.
            var killDut = false;
            var exitPort = PASS_PORT;

            for (var index = 0; index < freq_count; index++)
            {
                var offset = index * 128;

                // This equation will be positive if you're in the kill range, otherwise it will be negative
                var kill_range = (u_lim - ((index * ratio_incr) + first_ratio)) * (((index * ratio_incr) + first_ratio) - l_lim);
                Prime.Services.ConsoleService.PrintDebug($"[Lock_tgl42uy.LockWrapper] Clock=[{clock}] Index=[{index}] KillRange=[{kill_range}].");

                // save the vbias, counterr and countsum values.
                var vbias = binaryData.ExtractBits("15-0", 8 + offset).BinaryToInteger();
                var counterrBase = binaryData.ExtractBits("11-0", 36 + offset);
                int counterr2sComp = counterrBase.BinaryToInteger(twosComp: true);
                int counterrVal = (int)((Math.Abs(counterr2sComp) - 2) / ((index * ratio_incr) + first_ratio) * 2.5);
                var countsum = binaryData.ExtractBits("11-0", 48 + offset).BinaryToInteger();

                Prime.Services.ConsoleService.PrintDebug($"[Lock_tgl42uy.LockWrapper] Clock=[{clock}] Index=[{index}] Vbias=[{vbias}].");
                Prime.Services.ConsoleService.PrintDebug($"[Lock_tgl42uy.LockWrapper] Clock=[{clock}] Index=[{index}] CountErr=[{counterr2sComp}].");
                Prime.Services.ConsoleService.PrintDebug($"[Lock_tgl42uy.LockWrapper] Clock=[{clock}] Index=[{index}] CountSum=[{countsum}].");

                vbiasAll.Add(vbias);
                counterrAll.Add(counterr2sComp);
                countsumAll.Add(countsum);

                // check the kill range.
                if (kill_range >= 0)
                {
                    var enable = binaryData.ExtractBits("0", 0 + offset);

                    // FIXME - probably need to prioritize the exit ports.
                    if (vbias < 1 || vbias > 2046)
                    {
                        Prime.Services.ConsoleService.PrintError($"[lock_wrapper] Clock=[{clock}] index=[{index}] Failed Vbias=[{vbias}]");
                        exitPort = fail_port;
                        killDut = true;
                    }

                    if (counterrVal < -2 || counterrVal > 1)
                    {
                        Prime.Services.ConsoleService.PrintError($"[lock_wrapper] Clock=[{clock}] index=[{index}] Failed CountErr=[{counterrVal}] (Bits=[{counterrBase}])");
                        exitPort = fail_port;
                        killDut = true;
                    }

                    if (enable != "1")
                    {
                        Prime.Services.ConsoleService.PrintError($"[lock_wrapper] Clock=[{clock}] index=[{index}] Failed Enable=[{enable}]");
                        exitPort = RESET_FAIL_PORT;
                        killDut = true;
                    }

                    if (binaryData.Substring(offset, 128).Count(o => o == '1') > 127)
                    {
                        Prime.Services.ConsoleService.PrintError($"[lock_wrapper] Clock=[{clock}] index=[{index}] Failed Tap=[{binaryData.Substring(offset, 128)}]");
                        exitPort = TAP_FAIL_PORT;
                        killDut = true;
                    }

                    if (killDut)
                    {
                        LogFullFllStatus(binaryData, offset, $"{clock}_{index}_CRISTATUS");
                    }
                }
            }

            // log the results.
            MemDecode.WriteStrgvalToItuff($"{clock}_VBIAS", string.Join("_", vbiasAll));
            MemDecode.WriteStrgvalToItuff($"{clock}_COUNTERR", string.Join("_", counterrAll));
            MemDecode.WriteStrgvalToItuff($"{clock}_COUNTSUM", string.Join("_", countsumAll));

            return exitPort;
        }

        private static void LogFullFllStatus(string binaryData, int offset, string name)
        {
            var data = new List<string>();
            var index = 0;
            for (var i = 0; i < FLL_STATUS_REGISTER.Count; i++)
            {
                var size = int.Parse((string)FLL_STATUS_REGISTER[i]);
                var range = size == 1 ? $"0" : $"{size - 1}-0";
                data.Add(binaryData.ExtractBits(range, index + offset).BinaryToHex());
                index += size;
            }

            MemDecode.WriteStrgvalToItuff(name, string.Join("_", data));
            /*
            data["fll_extip_stat_reg_fll_enable"                 ] = binaryData.ExtractBits("0", 0 + offset);
            data["fll_extip_stat_reg_dist_reset_b"               ] = binaryData.ExtractBits("0", 1 + offset);
            data["fll_extip_stat_reg_fll_early_lock"             ] = binaryData.ExtractBits("0", 2 + offset);
            data["fll_extip_stat_reg_fll_final_lock"             ] = binaryData.ExtractBits("0", 3 + offset);
            data["fll_extip_stat_reg_ramp_done"                  ] = binaryData.ExtractBits("0", 4 + offset);
            data["fll_extip_stat_reg_freq_change_req"            ] = binaryData.ExtractBits("0", 5 + offset);
            data["fll_extip_stat_reg_freq_change_done"           ] = binaryData.ExtractBits("0", 6 + offset);
            data["fll_extip_stat_reg_reg_writes_done"            ] = binaryData.ExtractBits("0", 7 + offset);
            data["fll_extip_stat_reg_vbias_code_2fllcore"        ] = binaryData.ExtractBits("15-0", 8 + offset);
            data["fll_extip_stat_reg_fll_ratio"                  ] = binaryData.ExtractBits("7-0", 24 + offset);
            data["fll_diag_stat_reg_fll_refclk_req"              ] = binaryData.ExtractBits("0", 32 + offset);
            data["fll_diag_stat_reg_fll_refclk_ack"              ] = binaryData.ExtractBits("0", 33 + offset);
            data["fll_diag_stat_reg_fll_outclk_req"              ] = binaryData.ExtractBits("0", 34 + offset);
            data["fll_diag_stat_reg_fll_outclk_ack"              ] = binaryData.ExtractBits("0", 35 + offset);
            data["fll_diag_stat_reg_fll_counter_err"             ] = binaryData.ExtractBits("11-0", 36 + offset);
            data["fll_diag_stat_reg_fll_counter_sum"             ] = binaryData.ExtractBits("11-0", 48 + offset);
            data["fll_diag_stat_reg_hv_pwr_good"                 ] = binaryData.ExtractBits("0", 60 + offset);
            data["fll_diag_stat_reg_ldoen_dly_cntr_done"         ] = binaryData.ExtractBits("0", 61 + offset);
            data["fll_diag_stat_reg_fastrampdone_cntr_done"      ] = binaryData.ExtractBits("0", 62 + offset);
            data["fll_diag_stat_reg_ldo_ramp_timer_done"         ] = binaryData.ExtractBits("0", 63 + offset);
            data["fll_diag_stat_1_reg_fll_counter_ovf"           ] = binaryData.ExtractBits("9-0", 64 + offset);
            data["fll_diag_stat_1_reg_counter_overflow_sticky"   ] = binaryData.ExtractBits("0", 74 + offset);
            data["fll_diag_stat_1_reg_measurement_enable_dly_ver"] = binaryData.ExtractBits("0", 75 + offset);
            data["fll_diag_stat_1_reg_reserved_diag_sts_1"       ] = binaryData.ExtractBits("19-0", 76 + offset);
            data["fll_diag_stat_2_reg_reserved_diag_sts_2"       ] = binaryData.ExtractBits("31-0", 96 + offset); */
        }
    }
}
