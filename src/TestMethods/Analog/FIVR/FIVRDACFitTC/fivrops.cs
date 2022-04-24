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
    using System.Collections.Generic;
    using Prime.Base.Exceptions;

    /// <summary>
    /// Main Class for PPTH_FIVR_OPS Functions.
    /// </summary>
    public class FivrOps
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FivrOps"/> class.
        /// </summary>
        /// <param name="locnName">Indicator of if we are at a "HOT" test location.</param>
        /// <param name="opTypeName">Is used when we are at a not 'HOT' location.  The function uses this to determine what DFF location it should pull the boot domain VID codes from. This means that this variable must be set to 'PBIC_S1' for non-hot HVM locations, and PBIC_S2 for engineering non-hot locations.</param>
        /// <param name="ssid">Ube optype where the dff tokens need to be populated to (WFR=-98 for sort, else PKG=-99).</param>
        /// <param name="useAMMeas">AnalogMeasure(true)/CMEM(false) measurements selector.</param>
        public FivrOps(string locnName = "HOT", string opTypeName = "PBIC_DAB", int ssid = -99, bool useAMMeas = true)
        {
            this.Location = locnName;
            this.Optype = opTypeName;
            this.UseAMMeas = useAMMeas;
            this.LocnSSID = ssid;

            // Create the GSDSKeys
            this.GSDSKeys = new Dictionary<string, GSDSKeyStruct>();
            this.GSDSKeys["VNN"] = new GSDSKeyStruct(
                pAMVolts: new List<string> { "VDAC_A_FIVR_PCHVNN_VL_LC_0", "VDAC_B_FIVR_PCHVNN_VL_LC_0", "VDAC_C_FIVR_PCHVNN_VL_LC_0", "VDAC_D_FIVR_PCHVNN_VL_LC_0", "VDAC_E_FIVR_PCHVNN_VL_LC_0", "VDAC_F_FIVR_PCHVNN_VL_LC_0", "VDAC_G_FIVR_PCHVNN_VL_LC_0", "VDAC_H_FIVR_PCHVNN_VL_LC_0", "VDAC_I_FIVR_PCHVNN_VL_LC_0" },
                pCMEMExpVolts: new List<string> { "DACCALC_ANC_TARGET_VLOADR_LC_0", "DACCALC_ANC_TARGET_VLOADR_LC_1", "DACCALC_ANC_TARGET_VLOADR_LC_2", "DACCALC_ANC_TARGET_VLOADR_LC_3", "DACCALC_AND_TARGET_VLOADR_LC_0", "DACCALC_AND_TARGET_VLOADR_LC_1", "DACCALC_AND_TARGET_VLOADR_LC_2", "DACCALC_AND_TARGET_VLOADR_LC_3" },
                pCMEMVidCodes: new List<string> { "VDAC_POINT_A_FIVR_PCHVNN_VL_LC_0", "VDAC_POINT_B_FIVR_PCHVNN_VL_LC_0", "VDAC_POINT_C_FIVR_PCHVNN_VL_LC_0", "VDAC_POINT_D_FIVR_PCHVNN_VL_LC_0", "VDAC_POINT_E_FIVR_PCHVNN_VL_LC_0", "VDAC_POINT_F_FIVR_PCHVNN_VL_LC_0", "VDAC_POINT_G_FIVR_PCHVNN_VL_LC_0", "VDAC_POINT_H_FIVR_PCHVNN_VL_LC_0", "VDAC_POINT_I_FIVR_PCHVNN_VL_LC_0" },
                pTargetVoltageToTestMap: 2,
                pOperatingVoltRanges: new Dictionary<string, int> { { "MIN", 160 }, { "MAX", 512 } },
                pValidVidCodeRange: new Dictionary<string, int> { { "MIN", 5 }, { "MAX", 800 } });

            this.GSDSKeys["V1P05"] = new GSDSKeyStruct(
                pAMVolts: new List<string> { "VDAC_A_FIVR_PCH1P05_VL_LC_0", "VDAC_B_FIVR_PCH1P05_VL_LC_0", "VDAC_C_FIVR_PCH1P05_VL_LC_0", "VDAC_D_FIVR_PCH1P05_VL_LC_0", "VDAC_E_FIVR_PCH1P05_VL_LC_0", "VDAC_F_FIVR_PCH1P05_VL_LC_0", "VDAC_G_FIVR_PCH1P05_VL_LC_0", "VDAC_H_FIVR_PCH1P05_VL_LC_0", "VDAC_I_FIVR_PCH1P05_VL_LC_0" },
                pCMEMExpVolts: new List<string> { "DACCALC_ANC_TARGET_VLOAD0_LC_0", "DACCALC_ANC_TARGET_VLOAD0_LC_1", "DACCALC_ANC_TARGET_VLOAD0_LC_2", "DACCALC_ANC_TARGET_VLOAD0_LC_3", "DACCALC_AND_TARGET_VLOAD0_LC_0", "DACCALC_AND_TARGET_VLOAD0_LC_1", "DACCALC_AND_TARGET_VLOAD0_LC_2", "DACCALC_AND_TARGET_VLOAD0_LC_3" },
                pCMEMVidCodes: new List<string> { "VDAC_POINT_A_FIVR_PCH1P05_VL_LC_0", "VDAC_POINT_B_FIVR_PCH1P05_VL_LC_0", "VDAC_POINT_C_FIVR_PCH1P05_VL_LC_0", "VDAC_POINT_D_FIVR_PCH1P05_VL_LC_0", "VDAC_POINT_E_FIVR_PCH1P05_VL_LC_0", "VDAC_POINT_F_FIVR_PCH1P05_VL_LC_0", "VDAC_POINT_G_FIVR_PCH1P05_VL_LC_0", "VDAC_POINT_H_FIVR_PCH1P05_VL_LC_0", "VDAC_POINT_I_FIVR_PCH1P05_VL_LC_0" },
                pTargetVoltageToTestMap: 2,
                pOperatingVoltRanges: new Dictionary<string, int> { { "MIN", 160 }, { "MAX", 512 } },
                pValidVidCodeRange: new Dictionary<string, int> { { "MIN", 5 }, { "MAX", 800 } });

            this.AllDomains = new List<string> { "VNN", "V1P05" }; // cannot use dict.keys here because order matters.
        }

        /// <summary>
        /// Gets the GSDSKeys Struct.
        /// </summary>
        public Dictionary<string, GSDSKeyStruct> GSDSKeys { get; private set; }

        /// <summary>
        /// Gets the full list of domains (in order) that need to be populated in the dff tokens.
        /// </summary>
        public List<string> AllDomains { get; private set; }

        // Location is simply an indicator of if we are at a "hot" test location. If so,
        // this variable should be set to 'HOT' to tell the function that it needs to compute
        // a full temperature based offset for domains indicated in g_lDomainsToApplyPerUnitOffset
        // When not at a hot location, this can be any string except 'HOT'.  This is handled
        // in the wrapper functions at the very end of this file.
        private string Location { get; }

        // ube optype where the dff tokens need to be populated to (WFR=-98 for sort, else PKG=-99)
        private int LocnSSID { get; }

        // Optype is used when we are at a not 'HOT' location.  The function uses this
        // to determine what DFF location it should pull the boot domain VID codes from.
        // This means that this variable must be set to 'PBIC_S1' for non-hot HVM locations,
        // and PBIC_S2 for engineering non-hot locations.  This is handled in the wrapper
        // functions at the very end of this file.
        private string Optype { get; }

        // AnalogMeasure(true)/CMEM(false) measurements selector
        private bool UseAMMeas { get; } = true;

        // offset/slope and R2 Limits
        private Dictionary<string, int> LimitsSlope { get; } = new Dictionary<string, int> { { "MIN", -1000 }, { "MAX", 1000 } };

        private Dictionary<string, int> LimitsOffset { get; } = new Dictionary<string, int> { { "MIN", -1000 }, { "MAX", 1000 } };

        private Dictionary<string, int> LimitsR2 { get; } = new Dictionary<string, int> { { "MIN", -1000 }, { "MAX", 1000 } };

        // DFF token names with pipe-separated values per voltage domain
        private string SlopeDFFToken { get; } = "PFDACSL";

        private string OffsetDFFToken { get; } = "PFDACOF";

        private string SignDFFToken { get; } = "PFDACSG";

        // VRCI LSB resolution
        private double VoltResolution { get; } = 0.0025;

        /// <summary>
        /// Sets Default DFF values.
        /// </summary>
        public void ForceDefault_On_Exception()
        {
            LogStrgaltSspDataWithSeparator("PPTH_FIVR_OPS::DACCALC_VDAC_ERROR_FORCE", "1|1|1");
            /* Prime.Services.DatalogService.WriteToItuff("2_tname_PPTH_FIVR_OPS::DACCALC_VDAC_ERROR_FORCE\n2_strgalt_ssp_1|1|1\n2_lsep\n"); */
            Prime.Services.ConsoleService.PrintDebug("Overriding with defaults: SL 22|1|18|1 , OF 7|0|6|0 , SIG 1|1");
            var sSlopeDFFStrConverted = "22|1|18|1";
            var sOffsetDFFStrConverted = "7|0|6|0";
            var sSignDFFStrConverted = "1|1";

            Prime.Services.DffService.SetDff(this.SlopeDFFToken, sSlopeDFFStrConverted); // evg.SetDFFValue(g_nLocnSSID, g_sSlopeDFFToken, sSlopeDFFStrConverted)
            Prime.Services.DffService.SetDff(this.OffsetDFFToken, sOffsetDFFStrConverted); // evg.SetDFFValue(g_nLocnSSID, g_sOffsetDFFToken, sOffsetDFFStrConverted)
            Prime.Services.DffService.SetDff(this.SignDFFToken, sSignDFFStrConverted); // evg.SetDFFValue(g_nLocnSSID, g_sSignDFFToken, sSignDFFStrConverted)
        }

        /// <summary>
        /// Main function for performing the VDAC fit calculations.
        /// </summary>
        /// <param name="printDFF">If true, results will be logged a DFF tokens.</param>
        /// <returns>true on success.</returns>
        public bool FIVRDACFitCalculate(bool printDFF)
        {
            // Containers to hold the dff/ituff values
            List<string> slopeDFFStr = new List<string>();
            List<string> slopeDFFStrVRCI = new List<string>();
            List<string> offsetDFFStr = new List<string>();
            List<string> offsetDFFStrVRCI = new List<string>();
            List<string> slopeDFFStrConverted = new List<string>();
            List<string> offsetDFFStrConverted = new List<string>();
            List<string> signDFFStrConverted = new List<string>();
            List<string> r2ItuffStr = new List<string>();
            List<string> useAMMeasStr = new List<string>();

            // Fail Control
            var linearFitErr = false;
            var voltageRangeErr = false;
            var dacLimitErr = false;

            foreach (var domain in this.AllDomains)
            {
                // Get the r2 limit for the current domain.  This filters through global defined values via the domain dictionary.
                var useAMMeas = this.UseAMMeas;

                List<double> amVoltageMeasVals = new List<double>();
                foreach (var gsdskey in this.GSDSKeys[domain].AMVolts)
                {
                    var tmpVal = this.GetSharedData(gsdskey);
                    amVoltageMeasVals.Add(tmpVal);  // ACTUAL MEASURED VOLTS IN VLOAD PIN
                }

                List<double> cMEMExpectedVoltsVals = new List<double>();
                /* This code was commented out in the EmbPython implementation.
                foreach (var gsdskey in this.GSDSKeys[domain].CMEMExpVolts)
                {
                    var tmpVal = this.GetSharedData(gsdskey);
                    cMEMExpectedVoltsVals.Add(tmpVal / this.VoltResolution);
                } */

                List<double> cMEMVidCodeVals = new List<double>();
                foreach (var gsdskey in this.GSDSKeys[domain].CMEMVidCodes)
                {
                    var tmpVal = this.GetSharedData(gsdskey);
                    cMEMVidCodeVals.Add(tmpVal * this.VoltResolution); // VID code converted to volts equivalent
                }

                LinearFitStruct lFit;
                if (useAMMeas)
                {
                    lFit = this.CalculateLinearFit(amVoltageMeasVals, cMEMVidCodeVals);
                }
                else
                {
                    // linear fit of CMEM Expected voltage measurements, CMEMVidCode values
                    // FIXME: this isn't valid since the cMEMExpectedVoltsVals code was commented out.
                    lFit = this.CalculateLinearFit(cMEMExpectedVoltsVals, cMEMVidCodeVals);
                }

                if (!lFit.Status)
                {
                    throw new TestMethodException($"LinearFit failed for Domain={domain} UseAMMeas={useAMMeas}.");
                }

                // ensure that r-squared value is above the minimum
                if (this.LimitsR2["MIN"] > lFit.R2Value)
                {
                    Prime.Services.ConsoleService.PrintError($"Actual r-squared=[{lFit.R2Value}] is less than the minimum allowed of [{this.LimitsR2["MIN"]}] for Domain=[{domain}].");
                    linearFitErr = true;
                }

                // verify the vid code for the min operating voltage
                if (!this.VerifyOperVoltForDomain(domain, lFit.Slope, lFit.Offset))
                {
                    voltageRangeErr = true;
                }

                // now check that the slope/offset are within our limits
                if (lFit.Slope < this.LimitsSlope["MIN"] || lFit.Slope > this.LimitsSlope["MAX"])
                {
                    Prime.Services.ConsoleService.PrintError($"Actual slope=[{lFit.Slope}] is not in valid range [{this.LimitsSlope["MIN"]}]-[{this.LimitsSlope["MAX"]}] for Domain=[{domain}].");
                    dacLimitErr = true;
                }

                if (lFit.Offset < this.LimitsOffset["MIN"] || lFit.Offset > this.LimitsOffset["MAX"])
                {
                    Prime.Services.ConsoleService.PrintError($"Actual offset=[{lFit.Offset}] is not in valid range [{this.LimitsOffset["MIN"]}]-[{this.LimitsOffset["MAX"]}] for Domain=[{domain}].");
                    dacLimitErr = true;
                }

                // set slope/ offset to gsds for AVID use.
                // split slope/offset values into msb/lsb values for DFF tokens
                this.SetSharedData($"FIVR_{domain}_DAC_SLOPE_DEC", (double)lFit.SlopeConverted);
                this.SetSharedData($"FIVR_{domain}_DAC_OFFSET_DEC", (double)lFit.OffsetConverted);
                this.SetSharedData($"FIVR_{domain}_DAC_RSQUARE_DEC", (double)lFit.R2Value);

                if (lFit.SlopeConverted > 255)
                {
                    this.SetSharedData($"FIVR_{domain}_DAC_SLOPE_LSB_DEC", lFit.SlopeConverted - 256);
                    this.SetSharedData($"FIVR_{domain}_DAC_SLOPE_MSB_DEC", 1);
                    slopeDFFStrConverted.Add((lFit.SlopeConverted - 256).ToString());
                    slopeDFFStrConverted.Add("1");
                }
                else
                {
                    this.SetSharedData($"FIVR_{domain}_DAC_SLOPE_LSB_DEC", lFit.SlopeConverted);
                    this.SetSharedData($"FIVR_{domain}_DAC_SLOPE_MSB_DEC", 0);
                    slopeDFFStrConverted.Add(lFit.SlopeConverted.ToString());
                    slopeDFFStrConverted.Add("0");
                }

                var offset = Math.Abs(lFit.OffsetConverted);
                if (offset > 127)
                {
                    this.SetSharedData($"FIVR_{domain}_DAC_OFF_LSB_DEC", offset - 127);
                    this.SetSharedData($"FIVR_{domain}_DAC_OFF_MSB_DEC", 1);
                    offsetDFFStrConverted.Add((offset - 127).ToString());
                    offsetDFFStrConverted.Add("1");
                }
                else
                {
                    this.SetSharedData($"FIVR_{domain}_DAC_OFF_LSB_DEC", offset);
                    this.SetSharedData($"FIVR_{domain}_DAC_OFF_MSB_DEC", 0);
                    offsetDFFStrConverted.Add(offset.ToString());
                    offsetDFFStrConverted.Add("0");
                }

                this.SetSharedData($"FIVR_{domain}_DAC_SIGN_DEC", lFit.SignConverted);

                slopeDFFStr.Add(lFit.Slope.ToString("N3"));
                slopeDFFStrVRCI.Add((400.0 * lFit.Slope).ToString("N3"));

                offsetDFFStr.Add(lFit.Offset.ToString("N3"));
                offsetDFFStrVRCI.Add(lFit.Offset.ToString("N3"));

                signDFFStrConverted.Add(lFit.SignConverted.ToString());

                r2ItuffStr.Add(lFit.R2Value.ToString("N5"));

                useAMMeasStr.Add(useAMMeas.ToString());
            }

            // Write the results to ITuff
            /* FIXME Python had DACCALC_SLOPE_RAW & DACCALC_OFFSET_RAW_VRCI values swapped, matching that here */
            LogStrgaltSspDataWithSeparator("PPTH_FIVR_OPS::DACCALC_SLOPE_RAW", string.Join("|", slopeDFFStrVRCI));
            LogStrgaltSspDataWithSeparator("PPTH_FIVR_OPS::DACCALC_OFFSET_RAW", string.Join("|", offsetDFFStr));
            LogStrgaltSspDataWithSeparator("PPTH_FIVR_OPS::DACCALC_SLOPE_RAW_VRCI", string.Join("|", slopeDFFStr));
            LogStrgaltSspDataWithSeparator("PPTH_FIVR_OPS::DACCALC_OFFSET_RAW_VRCI", string.Join("|", offsetDFFStrVRCI));
            LogStrgaltSspDataWithSeparator("PPTH_FIVR_OPS::DACCALC_R2", string.Join("|", r2ItuffStr));
            LogStrgaltSspDataWithSeparator("PPTH_FIVR_OPS::DACCALC_SLOPE_FINAL", string.Join("|", slopeDFFStrConverted));
            LogStrgaltSspDataWithSeparator("PPTH_FIVR_OPS::DACCALC_OFFSET_FINAL", string.Join("|", offsetDFFStrConverted));
            LogStrgaltSspDataWithSeparator("PPTH_FIVR_OPS::DACCALC_SIGN_FINAL", string.Join("|", signDFFStrConverted));
            /*
            Prime.Services.DatalogService.WriteToItuff("2_tname_PPTH_FIVR_OPS::DACCALC_SLOPE_RAW\n2_strgalt_ssp_" + string.Join("|", slopeDFFStrVRCI) + "\n2_lsep\n"); // EmbPython had this swapped with VRCI version
            Prime.Services.DatalogService.WriteToItuff("2_tname_PPTH_FIVR_OPS::DACCALC_OFFSET_RAW\n2_strgalt_ssp_" + string.Join("|", offsetDFFStr) + "\n2_lsep\n");
            Prime.Services.DatalogService.WriteToItuff("2_tname_PPTH_FIVR_OPS::DACCALC_SLOPE_RAW_VRCI\n2_strgalt_ssp_" + string.Join("|", slopeDFFStr) + "\n2_lsep\n"); // EmbPython had this swapped with raw version
            Prime.Services.DatalogService.WriteToItuff("2_tname_PPTH_FIVR_OPS::DACCALC_OFFSET_RAW_VRCI\n2_strgalt_ssp_" + string.Join("|", offsetDFFStrVRCI) + "\n2_lsep\n");
            Prime.Services.DatalogService.WriteToItuff("2_tname_PPTH_FIVR_OPS::DACCALC_R2\n2_strgalt_ssp_" + string.Join("|", r2ItuffStr) + "\n2_lsep\n");
            Prime.Services.DatalogService.WriteToItuff("2_tname_PPTH_FIVR_OPS::DACCALC_SLOPE_FINAL\n2_strgalt_ssp_" + string.Join("|", slopeDFFStrConverted) + "\n2_lsep\n");
            Prime.Services.DatalogService.WriteToItuff("2_tname_PPTH_FIVR_OPS::DACCALC_OFFSET_FINAL\n2_strgalt_ssp_" + string.Join("|", offsetDFFStrConverted) + "\n2_lsep\n");
            Prime.Services.DatalogService.WriteToItuff("2_tname_PPTH_FIVR_OPS::DACCALC_SIGN_FINAL\n2_strgalt_ssp_" + string.Join("|", signDFFStrConverted) + "\n2_lsep\n"); */

            // print a tname to indicate if we enabled calculations via Vout (measured voltage) or Vref (target voltage)
            LogStrgaltSspDataWithSeparator("PPTH_FIVR_OPS::DACCALC_USEMEASUREDVOLTS", string.Join("|", useAMMeasStr));
            /* Prime.Services.DatalogService.WriteToItuff("2_tname_PPTH_FIVR_OPS::DACCALC_USEMEASUREDVOLTS\n2_strgalt_ssp_" + string.Join("|", useAMMeasStr) + "\n2_lsep\n"); */

            if (printDFF && !(linearFitErr || voltageRangeErr || dacLimitErr))
            {
                Prime.Services.DffService.SetDff(this.SlopeDFFToken, string.Join("|", slopeDFFStrConverted)); // Location = this.LocnSSID
                Prime.Services.DffService.SetDff(this.OffsetDFFToken, string.Join("|", offsetDFFStrConverted)); // Location = this.LocnSSID
                Prime.Services.DffService.SetDff(this.SignDFFToken, string.Join("|", signDFFStrConverted)); // Location = this.LocnSSID
            }

            // Log any errors to ITuff
            LogStrgaltSspDataWithSeparator("PPTH_FIVR_OPS::DACCALC_ERR", $"{Convert.ToInt32(linearFitErr)}|{Convert.ToInt32(voltageRangeErr)}|{Convert.ToInt32(dacLimitErr)}");
            /* Prime.Services.DatalogService.WriteToItuff($"2_tname_PPTH_FIVR_OPS::DACCALC_ERR\n2_strgalt_ssp_{Convert.ToInt32(linearFitErr)}|{Convert.ToInt32(voltageRangeErr)}|{Convert.ToInt32(dacLimitErr)}\n2_lsep\n"); */

            // Error out if we got an error
            if (linearFitErr || voltageRangeErr || dacLimitErr)
            {
                Prime.Services.ConsoleService.PrintError($"Error in FIVRDACFitCalculate: LinearFitErr=[{linearFitErr}] VoltageRangeErr=[{voltageRangeErr}] DACLimitErr=[{dacLimitErr}]");
                this.ForceDefault_On_Exception();
                return false;
            }

            return true;
        }

        private static void LogStrgaltSspDataWithSeparator(string tname, string sspData)
        {
            var writer = Prime.Services.DatalogService.GetItuffStrgaltWriter();
            writer.SetCustomTname(tname);
            writer.SetData("ssp", sspData);
            Prime.Services.DatalogService.WriteToItuff(writer);

            var separator = Prime.Services.DatalogService.GetItuffSeparatorFormatWriter();
            Prime.Services.DatalogService.WriteToItuff(separator);
        }

        /// <summary>
        /// The CalculateLinearFit.
        /// </summary>
        /// <param name="xValues">List of X values.</param>
        /// <param name="yValues">List of Y values.</param>
        /// <returns>The <see cref="LinearFitStruct"/>.</returns>
        private LinearFitStruct CalculateLinearFit(List<double> xValues, List<double> yValues)
        {
            LinearFitStruct retval = new LinearFitStruct();
            if (xValues.Count != yValues.Count)
            {
                Prime.Services.ConsoleService.PrintError($"X and Y lists passed to g_lLinearFit function are unequal - {xValues.Count} vs {yValues.Count}. X=[{xValues}] Y=[{yValues}].");
            }
            else
            {
                var count = xValues.Count;
                var sx = 0.0;
                var sy = 0.0;
                var sxx = 0.0;
                var syy = 0.0;
                var sxy = 0.0;
                for (var i = 0; i < count; i++)
                {
                    sx += xValues[i];
                    sy += yValues[i];
                    sxx += xValues[i] * xValues[i];
                    syy += yValues[i] * yValues[i];
                    sxy += xValues[i] * yValues[i];
                }

                var det = (sxx * count) - (sx * sx);
                if (det == 0)
                {
                    throw new TestMethodException($"Problem calculating LinearFit, divisor [det] is 0. XValues=[{string.Join(",", xValues)}] YValues=[{string.Join(", ", xValues)}]");
                }

                retval.Slope = Math.Round(((sxy * count) - (sy * sx)) / det, 3);
                retval.Offset = Math.Round(((sxx * sy) - (sx * sxy)) / det, 3);

                retval.SlopeConverted = (int)Math.Ceiling(retval.Slope * 256);
                retval.OffsetConverted = (int)Math.Ceiling(retval.Offset * 200);

                if (retval.Offset < 0)
                {
                    retval.SignConverted = 1;
                }

                var meanError = 0.0;
                var residual = 0.0;
                for (var i = 0; i < count; i++)
                {
                    meanError += Math.Pow(yValues[i] - (sy / count), 2);
                    residual += Math.Pow(yValues[i] - (retval.Slope * xValues[i]) - retval.Offset, 2);
                }

                if (meanError == 0)
                {
                    retval.R2Value = Math.Round(1.00000, 5);
                }
                else
                {
                    retval.R2Value = Math.Round(1 - (residual / meanError), 5);
                }

                Prime.Services.ConsoleService.PrintDebug($"LinearFit: X=[{string.Join(", ", xValues)}].");
                Prime.Services.ConsoleService.PrintDebug($"LinearFit: Y=[{string.Join(", ", yValues)}].");
                Prime.Services.ConsoleService.PrintDebug($"LinearFit: Slope=[{retval.Slope}] Offset=[{retval.Offset}] SlopeConverted=[{retval.SlopeConverted}] OffsetConverted=[{retval.OffsetConverted}] SignConverted=[{retval.SignConverted}] meanErr=[{meanError}] residual=[{residual}] R2Value=[{retval.R2Value}].");

                retval.Status = true;
                return retval;
            }

            return retval;
        }

        private bool VerifyOperVoltForDomain(string domain, double slope, double offset)
        {
            var minOperVolt = this.GSDSKeys[domain].OperatingVoltRanges["MIN"];
            var maxOperVolt = this.GSDSKeys[domain].OperatingVoltRanges["MAX"];

            var calcVidCodeMin = (slope * minOperVolt) + offset;
            if (this.GSDSKeys[domain].ValidVidCodeRange["MIN"] > (int)calcVidCodeMin)
            {
                Prime.Services.ConsoleService.PrintError($"Calculated VIDCode {calcVidCodeMin} for min operating voltage ({minOperVolt}) of domain ({domain}) is less than the valid vid code range start ({this.GSDSKeys[domain].ValidVidCodeRange["MIN"]})!");
                return false;
            }

            var calcVidCodeMax = (slope * maxOperVolt) + offset;
            if (this.GSDSKeys[domain].ValidVidCodeRange["MAX"] < (int)calcVidCodeMax)
            {
                Prime.Services.ConsoleService.PrintError($"Calculated VIDCode {calcVidCodeMax} for max operating voltage ({maxOperVolt}) of domain ({domain}) is greater than the valid vid code range start ({this.GSDSKeys[domain].ValidVidCodeRange["MAX"]})!");
                return false;
            }

            return true;
        }

        private double GetSharedData(string key)
        {
            return Prime.Services.SharedStorageService.GetDoubleRowFromTable(key, Prime.SharedStorageService.Context.DUT);
        }

        private void SetSharedData(string key, int value)
        {
            Prime.Services.SharedStorageService.InsertRowAtTable(key, value, Prime.SharedStorageService.Context.DUT);
        }

        private void SetSharedData(string key, double value)
        {
            Prime.Services.SharedStorageService.InsertRowAtTable(key, value, Prime.SharedStorageService.Context.DUT);
        }

        /// <summary>
        /// Defines the <see cref="LinearFitStruct" />.
        /// </summary>
        public class LinearFitStruct
        {
            /// <summary>
            /// Gets or sets a value indicating whether Status.
            /// </summary>
            public bool Status { get; set; } = false;

            /// <summary>
            /// Gets or sets the Slope.
            /// </summary>
            public double Slope { get; set; } = -999;

            /// <summary>
            /// Gets or sets the Offset.
            /// </summary>
            public double Offset { get; set; } = -999;

            /// <summary>
            /// Gets or sets the R2Value.
            /// </summary>
            public double R2Value { get; set; } = -999;

            /// <summary>
            /// Gets or sets the SlopeConverted.
            /// </summary>
            public int SlopeConverted { get; set; } = -999;

            /// <summary>
            /// Gets or sets the OffsetConverted.
            /// </summary>
            public int OffsetConverted { get; set; } = -999;

            /// <summary>
            /// Gets or sets the SignConverted.
            /// </summary>
            public int SignConverted { get; set; } = 0;
        }

        /// <summary>
        /// Defines the <see cref="GSDSKeyStruct" />.
        /// </summary>
        public class GSDSKeyStruct
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GSDSKeyStruct"/> class.
            /// </summary>
            /// <param name="pAMVolts">AMVolts List of string.</param>
            /// <param name="pCMEMExpVolts">CMEMExpVolts List of string.</param>
            /// <param name="pCMEMVidCodes">CMEMVidCodes List of string.</param>
            /// <param name="pTargetVoltageToTestMap">TargetVoltageToTestMap integer.</param>
            /// <param name="pOperatingVoltRanges">OperatingVoltRanges Dictionary, Keys=MIN/MAX Values=int.</param>
            /// <param name="pValidVidCodeRange">ValidVidCodeRange Dictionary, Keys=MIN/MAX Values=int..</param>
            public GSDSKeyStruct(List<string> pAMVolts, List<string> pCMEMExpVolts, List<string> pCMEMVidCodes, int pTargetVoltageToTestMap, Dictionary<string, int> pOperatingVoltRanges, Dictionary<string, int> pValidVidCodeRange)
            {
                this.AMVolts = pAMVolts;
                this.CMEMExpVolts = pCMEMExpVolts;
                this.CMEMVidCodes = pCMEMVidCodes;
                this.TargetVoltageToTestMap = pTargetVoltageToTestMap;
                this.OperatingVoltRanges = pOperatingVoltRanges;
                this.ValidVidCodeRange = pValidVidCodeRange;
            }

            /// <summary>
            /// Gets the AMVolts.
            /// </summary>
            public List<string> AMVolts { get; private set; }

            /// <summary>
            /// Gets the CMEMExpVolts.
            /// </summary>
            public List<string> CMEMExpVolts { get; private set; }

            /// <summary>
            /// Gets the CMEMVidCodes.
            /// </summary>
            public List<string> CMEMVidCodes { get; private set; }

            /// <summary>
            /// Gets the TargetVoltageToTestMap.
            /// </summary>
            public int TargetVoltageToTestMap { get; private set; }

            /// <summary>
            /// Gets the OperatingVoltRanges.
            /// </summary>
            public Dictionary<string, int> OperatingVoltRanges { get; private set; }

            /// <summary>
            /// Gets the ValidVidCodeRange.
            /// </summary>
            public Dictionary<string, int> ValidVidCodeRange { get; private set; }
        }
    }
}
