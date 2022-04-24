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
    using System;
    using System.Collections.Generic;
    using Prime.Base.Exceptions;

    /// <summary>blah.</summary>
    public class FivrTrim
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FivrTrim"/> class.
        /// </summary>
        /// <param name="scDeltaEn">Enable Sort/Class Delta calculations.</param>
        /// <param name="dffEn">Enable writing DFF data.</param>
        /// <param name="isSort">True if Sort.</param>
        /// <param name="gsdsEn">Enable writing GSDS.</param>
        /// <param name="altTagID">Alternative Tag ID for Ituff logging.</param>
        /// <param name="trimKill">True if Kill is enabled.</param>
        /// <param name="debug">True for debug mode.</param>
        /// <param name="locnSSID">SSID Location for DFF (not used).</param>
        /// <param name="moduleName">Module Name (not used).</param>
        public FivrTrim(bool scDeltaEn, bool dffEn, bool isSort, bool gsdsEn, string altTagID, bool trimKill, bool debug, int locnSSID = -99, string moduleName = "FIVRTRIM")
        {
            this.SCDeltaEnableMaster = scDeltaEn;
            this.DFFEnable = dffEn;
            this.IsSort = isSort;
            this.GSDSEnable = gsdsEn;
            this.AltTagID = altTagID;
            this.TrimKill = trimKill;
            this.DebugMode = debug;

            // Build the FIVR Data struct (equivalent to the embpython g_dData
            this.FivrData = new Dictionary<string, FIVRDomain>();
            this.FivrData["BGR"] = new FIVRDomain(new Dictionary<string, FIVRDomain.Trim> { { "BG", new FIVRDomain.Trim(dffToken: "PFVBG", dffPos: 0, cat: "BG", defaultVal: 130, defaultStr: "BG") } });
            this.FivrData["FFC"] = new FIVRDomain(new Dictionary<string, FIVRDomain.Trim> { { "VCO", new FIVRDomain.Trim(dffToken: "PFVVCO", dffPos: 0, cat: "VCO", defaultVal: 70, defaultStr: "FFCVCO") } });
            this.FivrData["VNN"] = new FIVRDomain(new Dictionary<string, FIVRDomain.Trim>
            {
                { "VCOR_PROP", new FIVRDomain.Trim(dffToken: "PFVCOR", dffPos: 0, cat: "VCOR", defaultVal: 33, defaultStr: "PWMVCO") },
                { "VCOR_INTEG", new FIVRDomain.Trim(dffToken: "PFVCOR", dffPos: 2, cat: "VCOR", defaultVal: 33, defaultStr: "PWMVCO") },
                { "VCO", new FIVRDomain.Trim(dffToken: "PFVCO", dffPos: 1, cat: "VCO", defaultVal: 33, defaultStr: "PWMVCO") },
                { "CPS", new FIVRDomain.Trim(dffToken: "PFCPS", dffPos: 0, cat: "CPS", defaultVal: 8, defaultStr: "CPS") },
                { "PWMCOMP", new FIVRDomain.Trim(dffToken: "PFPWM", dffPos: 0, numPhases: this.IONumPhases, cat: "PWM", defaultVal: 8, defaultStr: "PWMCOMP") },
            });
            this.FivrData["V1P05"] = new FIVRDomain(new Dictionary<string, FIVRDomain.Trim>
            {
                { "VCOR_PROP", new FIVRDomain.Trim(dffToken: "PFVCOR", dffPos: 1, cat: "VCOR", defaultVal: 33, defaultStr: "PWMVCO") },
                { "VCOR_INTEG", new FIVRDomain.Trim(dffToken: "PFVCOR", dffPos: 3, cat: "VCOR", defaultVal: 33, defaultStr: "PWMVCO") },
                { "VCO", new FIVRDomain.Trim(dffToken: "PFVCO", dffPos: 2, cat: "VCO", defaultVal: 33, defaultStr: "PWMVCO") },
                { "CPS", new FIVRDomain.Trim(dffToken: "PFCPS", dffPos: 1, cat: "CPS", defaultVal: 8, defaultStr: "CPS") },
                { "PWMCOMP", new FIVRDomain.Trim(dffToken: "PFPWM", dffPos: 0, numPhases: this.IONumPhases, cat: "PWM", defaultVal: 8, defaultStr: "PWMCOMP") },
            });

            // Read MIDAS global flag to determine if we should log data in MIDAS format or not
            /* if (!this.IsSort && Prime.Services.TpSettingsService.IsTpFeatureEnabled(Prime.TpSettingsService.Feature.Midas))
            {
                this.ItuffHeader1 = "0_tname_";
                this.ItuffHeader2 = "0_strgalt_";
            }
            else
            {
                this.ItuffHeader1 = "2_tname_";
                this.ItuffHeader2 = "2_strgalt_";
            } */

            // setup the error struct, this was an Exception in EmbPython.
            this.AllErrors = new List<ErrorContainer>();

            this.VCOR_Temp = string.Empty;
        }

        /// <summary>Gets or sets the Main FIVR Data Struct.</summary>
        public Dictionary<string, FIVRDomain> FivrData { get; set; }

        private bool DebugMode { get; set; } = false;

        /* TEST LIMITS */
        /* raw value limits inclusive. */
        private Dictionary<string, Dictionary<string, int>> RawLimits { get; } = new Dictionary<string, Dictionary<string, int>>
        {
            { "BG", new Dictionary<string, int> { { "MIN", 0 }, { "MAX", 255 } } },
            { "VCO", new Dictionary<string, int> { { "MIN", 1 }, { "MAX", 127 } } },
            { "VCOR", new Dictionary<string, int> { { "MIN", 0 }, { "MAX", 127 } } },
            { "CPS", new Dictionary<string, int> { { "MIN",  1 }, { "MAX", 127 } } },
            { "PWM", new Dictionary<string, int> { { "MIN",  1 }, { "MAX", 15 } } },
            { "VTG", new Dictionary<string, int> { { "MIN",  1 }, { "MAX", 15 } } },
            { "DAC", new Dictionary<string, int> { { "MIN",  1 }, { "MAX", 15 } } },
            { "CSR", new Dictionary<string, int> { { "MIN",  1 }, { "MAX", 15 } } },
        };

        /* Max range limits for each trim type, inclusive */
        private Dictionary<string, int> RangeLimits { get; } = new Dictionary<string, int>
        {
            { "BG", 15 },
            { "VCO", 127 },
            { "VCOR", 127 },
            { "CPS", 15 },
            { "PWM", 15 },
            { "VTG", 15 },
            { "DAC", 15 },
            { "CSR", 15 },
        };

        /* SCDelta limits, inclusive */
        private Dictionary<string, int> SCDeltaLimits { get; } = new Dictionary<string, int>
        {
            { "BG", 15 },
            { "VCO", 15 },
            { "VCOR", 15 },
            { "CPS", 15 },
            { "PWM", 15 },
            { "VTG", 15 },
        };

        /* These are legacy fail port definitions.
         * They are now datalogged as fail codes to identify the type of failure that occured.
         */
        private Dictionary<string, Dictionary<string, int>> FailPorts { get; } = new Dictionary<string, Dictionary<string, int>>
        {
            {
                "RAW", new Dictionary<string, int>
                {
                    { "BG", 2 },
                    { "VCOR", 3 },
                    { "VCO", 4 },
                    { "CPS", 5 },
                    { "PWM", 6 },
                    { "VTG", 7 },
                    { "DAC", 8 },
                    { "CSR", 9 },
                }
            },
            {
                "RANGE", new Dictionary<string, int>
                {
                    { "BG", 10 },
                    { "VCOR", 11 },
                    { "VCO", 12 },
                    { "CPS", 13 },
                    { "PWM", 14 },
                    { "VTG", 15 },
                    { "DAC", 16 },
                    { "CSR", 17 },
                }
            },
            {
                "DELTA", new Dictionary<string, int>
                {
                    { "BG", 18 },
                    { "VCOR", 19 },
                    { "VCO", 20 },
                    { "CPS", 21 },
                    { "PWM", 22 },
                    { "VTG", 23 },
                    { "DAC", 24 },
                    { "CSR", 25 },
                }
            },
            {
                "FINAL", new Dictionary<string, int> /* These are the actual/final exit ports */
                {
                    { "BG", 2 },
                    { "VCOR", 3 },
                    { "VCO", 4 },
                    { "CPS", 5 },
                    { "PWM", 6 },
                    { "VTG", 7 },
                    { "DAC", 8 },
                    { "CSR", 9 },
                }
            },
        };

        /* Hooks for controlling DONE/ERROR bit checking for each trim type */
        private Dictionary<string, Dictionary<string, bool>> IgnoreBits { get; } = new Dictionary<string, Dictionary<string, bool>>
        {
            { "BG", new Dictionary<string, bool> { { "DONE", false }, { "ERROR", false } } },
            { "VCOR", new Dictionary<string, bool> { { "DONE", false }, { "ERROR", false } } },
            { "VCO", new Dictionary<string, bool> { { "DONE", false }, { "ERROR", false } } },
            { "CPS", new Dictionary<string, bool> { { "DONE", false }, { "ERROR", false } } },
            { "PWM", new Dictionary<string, bool> { { "DONE", false }, { "ERROR", false } } },
            { "VTG", new Dictionary<string, bool> { { "DONE", false }, { "ERROR", false } } },
            { "DAC", new Dictionary<string, bool> { { "DONE", false }, { "ERROR", false } } },
            { "CSR", new Dictionary<string, bool> { { "DONE", false }, { "ERROR", false } } },
        };

        // sort/class delta control
        private bool SCDeltaEnableMaster { get; set; } = false;

        /*
         * These are per-trim hooks to control SCDelta check.  For a specific trims SCDelta to be enabled,
         * the global switch must be set (via function that is called), and the trmi specific switch must be
         * set as well.  The per-trim switches are meant to act as a per-trim override of the global value, so
         * we default them to '1 (which allows the global value to control everything), unless we want to force
         * a specific trim to have SCDelta check DISABLED, in which case we put the trim specific setting to '0
         * 121116 - ultC0 first silicon - disable PWM sort/class delta only.
         * 130111 - disable VCO sort/class delta until sort is in sync, due to VCO recipe change
         */
        private Dictionary<string, bool> SCDeltaEnable { get; set; } = new Dictionary<string, bool>
        {
            { "BG", true },
            { "VCOR", true },
            { "VCO", true },
            { "CPS", true },
            { "PWM", true },
            { "VTG", true },
            { "DAC", true },
            { "CSR", true },
        };

        /*
         * g_fTrimRecipeVersion is a unique identifier that is linked to trim settings used during test (those that are programmed
         * either as fuse settings, or as direct settings in the trim pattern), example: coarse/fine frequency control, mixer resistor
         * RFI vco reference, etc.  g_fTrimRecipeVersion is the "current" recipe that is running.  g_fSortTrimRecipeVersion is intended
         * for use at class test, so we can compare the sort recipe vs. the class recipe.  At sort we set g_fSortTrimRecipeVersion equal
         * to g_fTrimRecipeVersion inside of the getSortTrimRecipeVersion() function.  Note that getSortTrimRecipeVersion() MUST execute
         * before any SCdelta functions or datalogging functions inside of main()
         * */
        private double TrimRecipeVersion { get; set; } = 1.0;

        private double SortTrimRecipeVersion { get; set; } = -999;

        /* Trim recipe version tracking.  These dictionaries track what recipe versions are valid to enable sort/class
         * delta check on, for the corresponding trim types.  Initial condition on 130213 is that we updated VCO trim setings
         * mid-stepping, resulting in mismatch of VCO trim recipe between sort and class (due to time lag).  Thus, unversioned
         * trim recipe (which means an either no value, or an incoming value of -999 for the FVVD0 token) vs. trim recipe v1.0
         * should have VCO sort/class delta check disabled.  If both # sort and class show trim recipe v1.0, VCO sort/class deltas
         * can be enabled. BG, CPS, PWM and NLC can have sort/class delta enabled for both unversioned vs. 1.0 and 1.0 vs. 1.0.
         * In the future, if trim recipe ever changes again, the dictionaries below would need to be updated to include an entry for
         * that new trim, and what recipes were valid for enabling sort/class delta with it.
         * dictionary format is:  <recipe version>: [<list of recipes that are valid for enabling sort/class delta]
         * g_bSCDeltaEnable still acts as a global kill switch for sort/class delta!  If that is 0, then the value of the individual
         * trim type enable bit will not matter, and will be forced to 0 inside of getTrimSpecificSCDeltaEn()
         *
         * Recipe Definition Table
         * Version   Description
         * =======   ===========
         * -999      Unversioned trim data from sort.  May or may not be equivalent to v1.0
         * v1.0      Initial version of tracked trim recipe.  FCM VCOs use COARSE=0x7, MULT=0x1, FFINE=0x38.  FFC uses COARSE=0x7, 10 bit FFC FINE = 0x190, MULT=0x1.  ULT sets mixer resistor to 0 for GTs.
         */
        private Dictionary<string, Dictionary<double, List<double>>> ValidTrimRecipesForSCDelta { get; set; } = new Dictionary<string, Dictionary<double, List<double>>>
        {
            { "BG", new Dictionary<double, List<double>> { { 1.0, new List<double> { -999, 1.0 } } } },
            { "VCOR", new Dictionary<double, List<double>> { { 1.0, new List<double> { -999, 1.0 } } } },
            { "VCO", new Dictionary<double, List<double>> { { 1.0, new List<double> { -999, 1.0 } } } },
            { "CPS", new Dictionary<double, List<double>> { { 1.0, new List<double> { -999, 1.0 } } } },
            { "PWM", new Dictionary<double, List<double>> { { 1.0, new List<double> { -999, 1.0 } } } },
            { "VTG", new Dictionary<double, List<double>> { { 1.0, new List<double> { -999, 1.0 } } } },
            { "DAC", new Dictionary<double, List<double>> { { 1.0, new List<double> { -999, 1.0 } } } },
            { "CSR", new Dictionary<double, List<double>> { { 1.0, new List<double> { -999, 1.0 } } } },
        };

        // This variable is used as temporary storage for VCOR.
        private string VCOR_Temp { get; set; } = string.Empty;

        private List<ErrorContainer> AllErrors { get; set; }

        /* Global Inputs - these may be overridden in the wrapper functions defined at bottom of script (EmbPython implementation) */
        private bool DFFEnable { get; set; } = false;

        private bool GSDSEnable { get; set; } = false;

        private bool TrimKill { get; set; } = true;

        private string AltTagID { get; set; } = string.Empty;

        private bool IsSort { get; set; } = false;

        /* Define the actual number of phases per domain for this product */
        private int IONumPhases { get; } = 4;

        private int NumSamples { get; } = 3;

        /* Datalog order structure - this is used to ensure we always datalog values in correct order, or put empty pipes for domains that may not exist.
         * this should ALWAYS have the superset of FIVR domains as defined in the MTL
         */
        private List<string> LoggingBGTrim { get; } = new List<string> { "BGR" };

        private List<string> LoggingVCOR_PROP_Trim { get; } = new List<string> { "VNN", "V1P05" };

        private List<string> LoggingVCOR_INTEG_Trim { get; } = new List<string> { "VNN", "V1P05" };

        private List<string> LoggingVCOTrim { get; } = new List<string> { "FFC", "VNN", "V1P05" };

        private List<string> LoggingCPSTrim { get; } = new List<string> { "VNN", "V1P05" };

        private List<string> LoggingPWMTrimGB { get; } = new List<string> { "VNN", "V1P05" };

        private List<string> LoggingVTGTrimGB { get; } = new List<string> { "VNN", "V1P05" };

        private List<string> LoggingDACTrimGB { get; } = new List<string> { "VNN", "V1P05" };

        private List<string> LoggingCSRTrimGB { get; } = new List<string> { "VNN", "V1P05" };

        private List<string> LoggingPWM { get; } = new List<string> { "VNN", "V1P05" };

        /// <summary>
        /// Main function for executing FIVR Trim Calculations.
        /// </summary>
        /// <returns>Exit Port.</returns>
        public int FIVRTrimCalc()
        {
            this.Init();

            this.GetSamples();
            this.GetAveragesAndRanges();

            /* this was all comments in EmbPython
             *    #computePWMGb()  // didn't implement this.
             *    # we must run the getSortTrimRecipeVersion() function before doing anything
             *    # related to sort/class deltas or datalogging!  This ensures the sort recipe
             *    # is populated with proper value.
             *    #GetSortTrimRecipeVersion()
             */

            // we qualify the SCdelta check with the per-trim SCDelta enables inside
            if (this.SCDeltaEnableMaster)
            {
                this.GetSCDelta();
            }

            this.LogData();
            /* LogTrimVersion() // this was commented out in EmbPython */

            return this.GetExitPort();
        }

        private static void LogStrgaltSspData(string tname, string sspData)
        {
            var writer = Prime.Services.DatalogService.GetItuffStrgaltWriter();
            writer.SetCustomTname(tname);
            writer.SetData("ssp", sspData);
            Prime.Services.DatalogService.WriteToItuff(writer);
        }

        private void Init()
        {
            // clear any stale data.
            this.AllErrors.Clear();
            this.VCOR_Temp = string.Empty;
            foreach (var fivr in this.FivrData.Values)
            {
                fivr.Clear();
            }
        }

        /// <summary>
        /// Function to check if there were any errors and assign appropriate exit port if there were.
        /// </summary>
        /// <returns>exit port.</returns>
        private int GetExitPort()
        {
            Prime.Services.ConsoleService.PrintDebug("Running GetExitPort()");
            var exitPort = 0;
            var failInfo = string.Empty;

            /* check if the Exception object contains any errors.
             * If it does, exit on the port defined for the first failure in the list
             * if there are no failures, exit on port 1.
             */
            if (this.AllErrors.Count > 0)
            {
                var error = this.AllErrors[0];
                failInfo = $"{error.Error}|{error.Domain}|{error.TrimType}|{error.ItuffMsg}";
                exitPort = error.FinalFailPort;

                if (this.DebugMode)
                {
                    for (var i = 0; i < this.AllErrors.Count; i++)
                    {
                        Prime.Services.ConsoleService.PrintDebug($"-e- {this.AllErrors[i].DebugMsg} {this.AllErrors[i].Error}");
                    }
                }
            }
            else
            {
                failInfo = "PASS";
                exitPort = 1;
            }

            // Override port assignment to 1 if kill is disabled.  Used at sort.
            if (!this.TrimKill)
            {
                failInfo = "FORCEPASSEN|" + failInfo;
                exitPort = 1;
            }

            // log fail info to ituff
            LogStrgaltSspData("FIVR_TRIM_AVG_FAIL_INFO", failInfo);
            /* Prime.Services.DatalogService.WriteToItuff($"{this.ItuffHeader1}FIVR_TRIM_AVG_FAIL_INFO\n{this.ItuffHeader2}ssp_{failInfo}\n"); */
            return exitPort;
        }

        private int GetSharedInt(string key)
        {
            return Prime.Services.SharedStorageService.GetIntegerRowFromTable(key, Prime.SharedStorageService.Context.DUT);
        }

        private void SetSharedData(string key, int value)
        {
            Prime.Services.SharedStorageService.InsertRowAtTable(key, value, Prime.SharedStorageService.Context.DUT);
        }

        /* Read BG, VCO and CPS samples and status bits from GSDS (equates to TRIM_CAPTURE structure)
         * Error Checks: NONE
         */
        private SampleContainer ReadSample(string domain, string trimType, int sampleNum)
        {
            Prime.Services.ConsoleService.PrintDebug($"Running ReadSample() for DOMAIN: {domain} TRIMTYPE: {trimType} SAMPLE: {sampleNum}");
            var sample = new SampleContainer();

            var keyResult = $"PCH_{domain}_{trimType}_trim_result{this.AltTagID}_{sampleNum}";
            Prime.Services.ConsoleService.PrintDebug($"ReadSample(): Getting Data for [{keyResult}]");
            sample.Value = this.GetSharedInt(keyResult);

            if (trimType != "VCOR_INTEG" && trimType != "VCOR_PROP")
            {
                var keyDone = $"PCH_{domain}_{trimType}_trim_done{this.AltTagID}_{sampleNum}";
                Prime.Services.ConsoleService.PrintDebug($"ReadSample(): Getting Data for [{keyDone}]");
                sample.Done = this.GetSharedInt(keyDone);

                var keyError = $"PCH_{domain}_{trimType}_trim_error{this.AltTagID}_{sampleNum}";
                Prime.Services.ConsoleService.PrintDebug($"ReadSample(): Getting Data for [{keyError}]");
                sample.Error = this.GetSharedInt(keyError);
            }
            else
            {
                sample.Done = 1;
                sample.Error = 0;
            }

            return sample;
        }

        /* Read PWM samples and status bits from GSDS
         * Error Checks: NONE
         */
        private SampleContainer ReadPWMSample(string domain, string fivrType, string trimType, int phaseNum, int sampleNum)
        {
            Prime.Services.ConsoleService.PrintDebug($"Running ReadPWMSample() for DOMAIN: {domain} FIVRTYPE: {fivrType} TRIMTYPE: {trimType} PHASE: {phaseNum} SAMPLE: {sampleNum}");
            var sample = new SampleContainer();

            var keyResult = $"PCH_{domain}_{trimType}_{phaseNum}{this.AltTagID}_trim_result_{sampleNum}";
            Prime.Services.ConsoleService.PrintDebug($"ReadPWMSample(): Getting Data for [{keyResult}]");
            sample.Value = this.GetSharedInt(keyResult);

            var keyDone = $"PCH_{fivrType}_{trimType}_trim_done{this.AltTagID}_{sampleNum}";
            Prime.Services.ConsoleService.PrintDebug($"ReadSample(): Getting Data for [{keyDone}]");
            sample.Done = this.GetSharedInt(keyDone);

            var keyError = $"PCH_{fivrType}_{trimType}_{phaseNum}_trim_error{this.AltTagID}_{sampleNum}";
            Prime.Services.ConsoleService.PrintDebug($"ReadSample(): Getting Data for [{keyError}]");
            sample.Error = this.GetSharedInt(keyError);

            /*var keyGB = $"PCH_{fivrType}_{trimType}_IMAXGB{this.AltTagID}_{sampleNum}";
            Prime.Services.ConsoleService.PrintDebug($"ReadSample(): Getting Data for [{keyGB}]");
            sample.GB = this.GetSharedInt(keyGB);*/
            sample.GB = 8; // the code to read this was commented out in EmbPython
            return sample;
        }

        /* Determine what samples are valid.  A sample is valid if:
         *  a. The value of the sample is within the defined min/max limits
         *  b. The done bit is valid and not ignored, or, ignored
         *  c. The error bit is valid and not ignored, or, ignored
         * Return: bSampleValid
         */
        private bool IsSampleValid(SampleContainer sample, string category)
        {
            bool trimValid = false;
            bool doneValid = false;
            bool errorValid = false;
            bool sampleValid = false;

            if (!this.RawLimits.ContainsKey(category))
            {
                throw new ArgumentException($"No Raw Limits found for Trim=[{category}].", category);
            }

            if (!this.IgnoreBits.ContainsKey(category))
            {
                throw new ArgumentException($"No Ignore Settings found for Trim=[{category}].", category);
            }

            // verify that the value is between the min/max limits.
            if (sample.Value <= this.RawLimits[category]["MAX"] && sample.Value >= this.RawLimits[category]["MIN"])
            {
                trimValid = true;
            }

            // now check if the iDn/iErr bits are valid. If we are ignoring iDn/iErr bit values
            // the associated valid bit is always forced to '1
            doneValid = this.IgnoreBits[category]["DONE"] || sample.Done == 1;
            errorValid = this.IgnoreBits[category]["ERROR"] || sample.Error == 0;

            // the sample is only valid if all the bits are valid.
            sampleValid = trimValid && doneValid && errorValid;
            Prime.Services.ConsoleService.PrintDebug($"IsSampleValid({category}): TrimValid={trimValid} SampleValue={sample.Value} MinLimit={this.RawLimits[category]["MIN"]} MaxLimit={this.RawLimits[category]["MAX"]}");
            Prime.Services.ConsoleService.PrintDebug($"IsSampleValid({category}): DoneValid={doneValid} IgnoreDone={this.IgnoreBits[category]["DONE"]} SampleDone={sample.Done}");
            Prime.Services.ConsoleService.PrintDebug($"IsSampleValid({category}): ErrorValid={errorValid} IgnoreError={this.IgnoreBits[category]["ERROR"]} SampleError={sample.Error}");
            Prime.Services.ConsoleService.PrintDebug($"IsSampleValid({category}): SampleValid={sampleValid}");
            return sampleValid;
        }

        /* Compute good samples (equates to SINGLE_TRIM_SAMPLE_VALID and TOTAL_TRIM_SAMPLES_VALID)
         * Reads a sample and checks if it is GOOD (done/error/value limit).  Only put GOOD samples into SAMPLES list.
         * Error Checks: NONE
         */
        private bool GetSamples()
        {
            Prime.Services.ConsoleService.PrintDebug("Running GetSamples()");

            foreach (var domain in this.FivrData.Keys)
            {
                foreach (var trimType in this.FivrData[domain].Trims.Keys)
                {
                    Prime.Services.ConsoleService.PrintDebug($"Getting Samples for DOMAIN:{domain} TRIMTYPE:{trimType}");
                    var fivrObj = this.FivrData[domain].Trims[trimType];

                    /* instead of hardcoding trimtype, could check if numPhases > 1 */
                    if (trimType == "PWMCOMP")
                    {
                        var fivrType = domain;
                        for (var phase = 0; phase < fivrObj.NumPhases; phase++)
                        {
                            List<int> sampleValues = new List<int>();
                            for (var i = 0; i < this.NumSamples; i++)
                            {
                                var sample = this.ReadPWMSample(domain, fivrType, trimType, phase, i);
                                Prime.Services.ConsoleService.PrintDebug($"Domain:{domain}, Trim:{trimType}, Fivr:{fivrType}, Phase:{phase}, Sample:{i}, Value:{sample.Value}, Done:{sample.Done}, Error:{sample.Error}, GB:{sample.GB}");

                                if (this.IsSampleValid(sample, fivrObj.Category))
                                {
                                    sampleValues.Add(sample.Value);
                                }
                                else
                                {
                                    sampleValues.Add(fivrObj.DefaultVal);
                                    Prime.Services.ConsoleService.PrintDebug($"Setting default val {fivrObj.DefaultVal} for {trimType}");
                                    LogStrgaltSspData("FIVR_TRIM_AVG_DEFAULT_INFO", $"{fivrObj.DefaultVal}|{fivrObj.DefaultStr}");
                                    /* Prime.Services.DatalogService.WriteToItuff($"{this.ItuffHeader1}FIVR_TRIM_AVG_DEFAULT_INFO\n{this.ItuffHeader2}ssp_{fivrObj.DefaultVal}|{fivrObj.DefaultStr}\n"); */
                                }
                            } // end of foreach sample

                            // save the samples.
                            Prime.Services.ConsoleService.PrintDebug($"Domain:{domain}, Trim:{trimType}, Fivr:{fivrType}, Phase:{phase}, Values:[{string.Join(",", sampleValues)}]");
                            fivrObj.SetSamples(sampleValues, phase);
                        } // end of foreach phase
                    } // end of trimType == "PWMCOMP"
                    else
                    {
                        List<int> sampleValues = new List<int>();
                        for (var i = 0; i < this.NumSamples; i++)
                        {
                            var sample = this.ReadSample(domain, trimType, i);
                            Prime.Services.ConsoleService.PrintDebug($"Domain:{domain}, Trim:{trimType}, Sample:{i}, Value:{sample.Value}, Done:{sample.Done}, Error:{sample.Error}");

                            if (this.IsSampleValid(sample, fivrObj.Category))
                            {
                                sampleValues.Add(sample.Value);
                            }
                            else
                            {
                                sampleValues.Add(fivrObj.DefaultVal);
                                Prime.Services.ConsoleService.PrintDebug($"Setting default val {fivrObj.DefaultVal} for {trimType}");
                                LogStrgaltSspData("FIVR_TRIM_AVG_DEFAULT_INFO", $"{fivrObj.DefaultVal}|{fivrObj.DefaultStr}");
                                /* Prime.Services.DatalogService.WriteToItuff($"{this.ItuffHeader1}FIVR_TRIM_AVG_DEFAULT_INFO\n{this.ItuffHeader2}ssp_{fivrObj.DefaultVal}|{fivrObj.DefaultStr}\n"); */
                            }
                        } // end of foreach sample

                        // save the samples.
                        Prime.Services.ConsoleService.PrintDebug($"Domain:{domain}, Trim:{trimType}, Values:[{string.Join(",", sampleValues)}]");
                        fivrObj.SetSamples(sampleValues);
                    } // end of trimType != "PWMCOMP"
                } // end of foreach trimtype
            } // end of foreach domain

            return true;
        }

        /* compute the average, range and error values for a given set of samples
         * Error checks:
         *   b. check that max range is within defined limits
         *   c. check that final average is within defined limits
         */
        private bool ComputeAvgAndRange(string domain, string trimType, int phase)
        {
            Prime.Services.ConsoleService.PrintDebug($"Running computeAvgAndRange() for DOMAIN:{domain} TRIMTYPE:{trimType} PHASE:{phase}");
            var fivrObj = this.FivrData[domain].Trims[trimType];
            var category = fivrObj.Category;
            var sampleErr = false;
            var rangeErr = false;
            string phaseAsStr = phase.ToString();

            if (!fivrObj.HasPhases)
            {
                // force phase to be 0 if not PWM, since the others don't have phases.
                phase = 0;
                phaseAsStr = string.Empty;
            }

            var sum = 0;
            var max = -9999;
            var min = 9999;
            var allSamples = fivrObj.GetSamples(phase);
            Prime.Services.ConsoleService.PrintDebug($"Samples:[{string.Join(", ", allSamples)}].");

            foreach (var value in allSamples)
            {
                // Last sample value will be passed to DFF because average might yield wrong frequency due to non isotonic trim response. yarojass
                fivrObj.SetLastSample(value, phase);
                sum += value;
                if (value > max)
                {
                    max = value;
                }

                if (value < min)
                {
                    min = value;
                }
            }

            // EmbPython code: int(math.floor(float(iAvg) / len(g_dData[sDomain][sTrimType][sPhase]['SAMPLES'])))
            // c# throws away the remainder when doing integer division, so this is equavalent to a doing a "floor" calculation post-divide.
            var average = sum / allSamples.Count;
            var range = Math.Abs(max - min);

            Prime.Services.ConsoleService.PrintDebug($"Sum={sum} Count={allSamples.Count} Average={average}");
            Prime.Services.ConsoleService.PrintDebug($"Min={min} Max={max} Range={range}");

            if (average <= this.RawLimits[category]["MAX"] && average >= this.RawLimits[category]["MIN"])
            {
                fivrObj.SetAverage(average, phase);
            }
            else if (this.IsSort)
            {
                fivrObj.SetAverage(0, phase);
                sampleErr = true;
            }
            else
            {
                fivrObj.SetAverage(-999, phase);
                sampleErr = true;
            }

            if (range <= this.RangeLimits[category])
            {
                fivrObj.SetRange(range, phase);
            }
            else
            {
                fivrObj.SetRange(range, phase);
                rangeErr = true;
            }

            Prime.Services.ConsoleService.PrintDebug($"LimitMax={this.RawLimits[category]["MAX"]} LimitMin={this.RawLimits[category]["MIN"]} SampleError={sampleErr}");
            Prime.Services.ConsoleService.PrintDebug($"RangeLimit={this.RangeLimits[category]} RangeError={rangeErr}");

            if (sampleErr)
            {
                this.AllErrors.Add(new ErrorContainer(domain, trimType, phaseAsStr, this.FailPorts["RAW"][category], this.FailPorts["FINAL"][category], "error calculating average", "LIMIT"));
            }

            if (rangeErr)
            {
                this.AllErrors.Add(new ErrorContainer(domain, trimType, phaseAsStr, this.FailPorts["RANGE"][category], this.FailPorts["FINAL"][category], "error calculating range", "RANGE"));
            }

            return true;
        }

        /* Compute average of good samples
         * Error Checks:
         *    a. check that sample list is not empty (ensure we have at least 1 good sample) prior to computing average and range
         */
        private bool GetAveragesAndRanges()
        {
            Prime.Services.ConsoleService.PrintDebug("Running GetAveragesAndRanges()");
            foreach (var domain in this.FivrData.Keys)
            {
                foreach (var trimType in this.FivrData[domain].Trims.Keys)
                {
                    Prime.Services.ConsoleService.PrintDebug($"Executing on DOMAIN:{domain} TRIMTYPE:{trimType}");
                    var fivrObj = this.FivrData[domain].Trims[trimType];
                    var category = fivrObj.Category;
                    var numPhases = fivrObj.HasPhases ? fivrObj.NumPhases : 1;
                    for (var phase = 0; phase < numPhases; phase++)
                    {
                        var phaseAsStr = fivrObj.HasPhases ? phase.ToString() : string.Empty;
                        var allSamples = fivrObj.GetSamples(phase);
                        if (allSamples.Count == 0)
                        {
                            if (this.IsSort)
                            {
                                fivrObj.SetAverage(0, phase);
                            }
                            else
                            {
                                fivrObj.SetAverage(-999, phase);
                            }

                            this.AllErrors.Add(new ErrorContainer(domain, trimType, phaseAsStr, this.FailPorts["RAW"][category], this.FailPorts["FINAL"][category], "sample list is empty during getAveragesAndRanges", "LIMIT"));
                            this.AllErrors.Add(new ErrorContainer(domain, trimType, phaseAsStr, this.FailPorts["RANGE"][category], this.FailPorts["FINAL"][category], "sample list is empty during getAveragesAndRanges", "RANGE"));
                        }
                        else
                        {
                            this.ComputeAvgAndRange(domain, trimType, phase);
                        }
                    } // end foreach phase
                } // end foreach trim
            } // end foreach domain

            return true;
        }

        /*Compute sort/class delta of average
         * Error Checks:
         *    a. Check that SCDelta is within defined limits
         * Read DFF data from SORT for use in SCDelta check
         * Error Checks: NONE
         */
        private bool ComputeSCDelta(string domain, string trimType, int phase)
        {
            Prime.Services.ConsoleService.PrintDebug($"Running computeSCDelta() for DOMAIN:{domain} TRIMTYPE:{trimType} PHASE:{phase}");
            var fivrObj = this.FivrData[domain].Trims[trimType];
            var category = fivrObj.Category;
            string phaseAsStr = phase.ToString();

            if (!fivrObj.HasPhases)
            {
                // force phase to be 0 if not PWM, since the others don't have phases.
                phase = 0;
                phaseAsStr = string.Empty;
            }

            // first check that our class data is valid.
            var average = fivrObj.GetAverage(phase);
            if (average == -999)
            {
                fivrObj.SetSCDelta(-999, phase);
                this.AllErrors.Add(new ErrorContainer(domain, trimType, phaseAsStr, this.FailPorts["DELTA"][category], this.FailPorts["FINAL"][category], "average is -999 during SCDelta", "SCDELTA"));
            }
            else
            {
                // get sort dff token value
                var dffVal = Prime.Services.DffService.GetDffByOpType(fivrObj.DffToken, "SORT");
                var dffLst = dffVal.Split('|');
                var dffPos = fivrObj.HasPhases ? phase : fivrObj.DffPos;
                if (dffPos >= dffLst.Length)
                {
                    throw new TestMethodException($"Problem when reading Sort DFF Token=[{fivrObj.DffToken}] Value=[{dffVal}]. Expecting at least [{dffPos}] positions for domain=[{domain}] trimType=[{trimType}] phase=[{phase}]");
                }

                if (!int.TryParse(dffLst[dffPos], out var sortVal))
                {
                    throw new TestMethodException($"Problem when reading Sort DFF Token=[{fivrObj.DffToken}] Value=[{dffVal}]. Position [{dffPos}] was not an ineger for domain=[{domain}] trimType=[{trimType}] phase=[{phase}]");
                }

                var scDelta = sortVal - average;
                if (scDelta <= this.SCDeltaLimits[category] && scDelta >= (-1 * this.SCDeltaLimits[category]))
                {
                    fivrObj.SetSCDelta(scDelta, phase);
                }
                else
                {
                    fivrObj.SetSCDelta(-999, phase);
                    this.AllErrors.Add(new ErrorContainer(domain, trimType, phaseAsStr, this.FailPorts["DELTA"][category], this.FailPorts["FINAL"][category], "SCDelta outside of limit", "SCDELTA"));
                }
            }

            return true;
        }

        /*function used to grab the sort trim recipe version from DFF. It runs immediately prior to
          getSCDelta(), ensuring that g_fSortTrimRecipeVersion is up to date before we try to use it
          g_fSortTrimRecipeVersion is populated with -999 by default, or if it fails to read the FVVD0 token.
          At sort we just copy g_fTrimRecipeVersion value into g_fSortTrimRecipeVersion
         */
        private bool GetSortTrimRecipeVersion()
        {
            if (!this.IsSort)
            {
                try
                {
                    this.SortTrimRecipeVersion = double.Parse(Prime.Services.DffService.GetDffByOpType("FVREC", "SORT"));
                }
                catch
                {
                    this.SortTrimRecipeVersion = -999;
                }
            }
            else
            {
                this.SortTrimRecipeVersion = this.TrimRecipeVersion;
            }

            return true;
        }

        /* function used to return trim specific SCdelta enable - used inside subroutines
         * that rely on SCdelta settings it will return 0 if it fails to match the incoming
         * trim type
         */
        private bool GetTrimSpecificSCDeltaEn(string trimType)
        {
            // short-circut everthing if the Master enable is false.
            if (!this.SCDeltaEnableMaster)
            {
                return false;
            }

            // verify the structs all contain trimType
            if (!this.ValidTrimRecipesForSCDelta.ContainsKey(trimType))
            {
                throw new TestMethodException($"ValidTrimRecipesForSCDelta does not contain an entry for TrimType=[{trimType}].  Valid Entryes=[{string.Join(", ", this.ValidTrimRecipesForSCDelta.Keys)}].");
            }

            if (this.SCDeltaEnable.ContainsKey(trimType))
            {
                // FIXME is this an exception or just return false?
                return false;
            }

            // make sure the Version info is in the struct.
            if (!this.ValidTrimRecipesForSCDelta[trimType].ContainsKey(this.TrimRecipeVersion))
            {
                // FIXME is this an exception or just return false?
                return false;
            }

            return this.ValidTrimRecipesForSCDelta[trimType][this.TrimRecipeVersion].Contains(this.SortTrimRecipeVersion) && this.SCDeltaEnable[trimType];
        }

        private bool GetSCDelta()
        {
            Prime.Services.ConsoleService.PrintDebug("Running GetSCDelta()");
            foreach (var domain in this.FivrData.Keys)
            {
                foreach (var trimType in this.FivrData[domain].Trims.Keys)
                {
                    Prime.Services.ConsoleService.PrintDebug($"Executing on DOMAIN:{domain} TRIMTYPE:{trimType}");
                    if (this.GetTrimSpecificSCDeltaEn(trimType))
                    {
                        var fivrObj = this.FivrData[domain].Trims[trimType];
                        var numPhases = fivrObj.HasPhases ? fivrObj.NumPhases : 1;
                        for (var phase = 0; phase < numPhases; phase++)
                        {
                            this.ComputeSCDelta(domain, trimType, phase);
                        }
                    }
                    else
                    {
                        Prime.Services.ConsoleService.PrintDebug($"GetTrimSpecificSCDeltaEn({trimType}) returned false, not computing Sort/Class delta.");
                    }
                }
            }

            return true;
        }

        private bool BuildDatalogStrings(List<string> domainsToLog, string trimType)
        {
            Prime.Services.ConsoleService.PrintDebug($"Running BuildDataLogStrings() for Domains=[{string.Join(", ", domainsToLog)}] and Trim type={trimType}");
            List<string> averageValues = new List<string>();
            List<string> rangeValues = new List<string>();
            List<string> scDeltaValues = new List<string>();
            List<string> lastSampleValues = new List<string>();

            // save all the data
            foreach (var domain in domainsToLog)
            {
                Prime.Services.ConsoleService.PrintDebug($"Building Datalog for Domain : {domain}");
                if (this.FivrData.ContainsKey(domain))
                {
                    if (this.FivrData[domain].Trims.ContainsKey(trimType))
                    {
                        var fivrObj = this.FivrData[domain].Trims[trimType];
                        var numPhases = fivrObj.HasPhases ? fivrObj.NumPhases : 1;
                        for (var phase = 0; phase < numPhases; phase++)
                        {
                            Prime.Services.ConsoleService.PrintDebug($"Building Datalog for Domain={domain} TrimType={trimType} Phase={phase}");
                            averageValues.Add(fivrObj.GetAverage(phase).ToString());
                            rangeValues.Add(fivrObj.GetRange(phase).ToString());
                            scDeltaValues.Add(fivrObj.GetSCDelta(phase).ToString());
                            lastSampleValues.Add(fivrObj.GetLastSample(phase).ToString());

                            // FIXME: I don't understand how this worked for PWM in EmbPython, it doesn't take phase into account.
                            if (this.GSDSEnable)
                            {
                                var token = $"{domain}_{trimType}{this.AltTagID}";
                                var value = fivrObj.GetAverage(phase);
                                Prime.Services.ConsoleService.PrintDebug($"Saving GSDS Token=[{token}] Value=[{value}]");
                                this.SetSharedData(token, value);
                            }
                        }
                    }
                    else
                    {
                        Prime.Services.ConsoleService.PrintDebug($"Domain={domain} does contain a TrimType={trimType}.");
                        averageValues.Add(string.Empty);
                        rangeValues.Add(string.Empty);
                        scDeltaValues.Add(string.Empty);
                        lastSampleValues.Add(string.Empty);
                    }
                }
                else
                {
                    Prime.Services.ConsoleService.PrintDebug($"Domain={domain} does not exist in FivrData struct.");
                    averageValues.Add(string.Empty);
                    rangeValues.Add(string.Empty);
                    scDeltaValues.Add(string.Empty);
                    lastSampleValues.Add(string.Empty);
                }
            }

            // log the data to ituff
            LogStrgaltSspData($"FIVR_TRIM{this.AltTagID}_AVG_{trimType}", string.Join("|", averageValues));
            /* var ituffAverage = $"{this.ItuffHeader1}FIVR_TRIM{this.AltTagID}_AVG_{trimType}\n{this.ItuffHeader2}ssp_{string.Join("|", averageValues)}\n";
            Prime.Services.ConsoleService.PrintDebug($"Writing Averages to Ituff:{ituffAverage}");
            Prime.Services.DatalogService.WriteToItuff(ituffAverage); */

            LogStrgaltSspData($"FIVR_TRIM{this.AltTagID}_AVG_{trimType}_MAXRANGE", string.Join("|", rangeValues));
            /* var ituffRange = $"{this.ItuffHeader1}FIVR_TRIM{this.AltTagID}_AVG_{trimType}_MAXRANGE\n{this.ItuffHeader2}ssp_{string.Join("|", rangeValues)}\n";
            Prime.Services.ConsoleService.PrintDebug($"Writing Ranges to Ituff:{ituffRange}");
            Prime.Services.DatalogService.WriteToItuff(ituffRange); */

            var scDeltaEnableLocal = this.GetTrimSpecificSCDeltaEn(trimType);
            if (this.SCDeltaEnableMaster && scDeltaEnableLocal)
            {
                LogStrgaltSspData($"FIVR_TRIM{this.AltTagID}_AVG_{trimType}_SCDELTA", string.Join("|", scDeltaValues));
                /* var ituffDelta = $"{this.ItuffHeader1}FIVR_TRIM{this.AltTagID}_AVG_{trimType}_SCDELTA\n{this.ItuffHeader2}ssp_{string.Join("|", scDeltaValues)}\n";
                Prime.Services.ConsoleService.PrintDebug($"Writing Sort/Class Deltas to Ituff:{ituffDelta}");
                Prime.Services.DatalogService.WriteToItuff(ituffDelta); */
            }

            // based on trimtype, store the appropriate data to GSDS
            // FIXME: shouldn't use hardcoded token names.
            if (this.DFFEnable)
            {
                if (trimType == "BG")
                {
                    // Save the Average Value.
                    Prime.Services.ConsoleService.PrintDebug($"DFF average value for TrimType={trimType} Token=PFVBG");
                    Prime.Services.DffService.SetDff("PFVBG", string.Join("|", averageValues));
                }
                else if (trimType == "VCO")
                {
                    // Save the Last Sample Value.
                    Prime.Services.ConsoleService.PrintDebug($"DFF Last Sample value for TrimType={trimType} Token=PFVCO");
                    Prime.Services.DffService.SetDff("PFVCO", string.Join("|", lastSampleValues));
                }
                else if (trimType == "VCOR_PROP")
                {
                    // Temporarily save the Last Sample Value to combine it with trimType=VCOR_INTEG
                    Prime.Services.ConsoleService.PrintDebug($"Using PFVCOR STATIC VALUES for TrimType={trimType}");
                    this.VCOR_Temp = string.Join("|", lastSampleValues);
                }
                else if (trimType == "VCOR_INTEG")
                {
                    // Save the Last Sample Value + the PFCOR temp.
                    var value = $"{this.VCOR_Temp}|{string.Join("|", lastSampleValues)}";
                    Prime.Services.ConsoleService.PrintDebug($"DFF Last Sample value + PFVCOR Static for TrimType={trimType} Token=PFVCOR");
                    Prime.Services.DffService.SetDff("PFVCOR", value);
                }
                else if (trimType == "CPS")
                {
                    // Save the Average Value.
                    Prime.Services.ConsoleService.PrintDebug($"DFF average value for TrimType={trimType} Token=PFCPS");
                    Prime.Services.DffService.SetDff("PFCPS", string.Join("|", averageValues));
                }
                else if (trimType == "PWMCOMP")
                {
                    // Save the Average Value.
                    Prime.Services.ConsoleService.PrintDebug($"DFF average value for TrimType={trimType} Token=PFPWM");
                    Prime.Services.DffService.SetDff("PFPWM", string.Join("|", averageValues));
                }
            }

            return true;
        }

        private bool LogData()
        {
            Prime.Services.ConsoleService.PrintDebug("Running LogData()");
            this.BuildDatalogStrings(this.LoggingBGTrim, "BG");
            this.BuildDatalogStrings(this.LoggingVCOR_PROP_Trim, "VCOR_PROP");
            this.BuildDatalogStrings(this.LoggingVCOR_INTEG_Trim, "VCOR_INTEG");
            this.BuildDatalogStrings(this.LoggingVCOTrim, "VCO");
            this.BuildDatalogStrings(this.LoggingCPSTrim, "CPS");
            this.BuildDatalogStrings(this.LoggingPWM, "PWMCOMP");
            return true;
        }

        /* This function will log the trim version configured in the script, as well
         * as the global resulting per-trim sort/class delta check enable bits.  At sort
         * we also log the trim version to DFF, so we can read it at class.
         */
        private bool LogTrimVersion()
        {
            var scDeltaStr = $"{this.SCDeltaEnableMaster}|{this.SCDeltaEnable["BG"]}|{this.SCDeltaEnable["VCO"]}|{this.SCDeltaEnable["CPS"]}|{this.SCDeltaEnable["PWM"]}";
            var ituffStr = $"{this.TrimRecipeVersion}|{this.SortTrimRecipeVersion}|{scDeltaStr}";
            LogStrgaltSspData("FIVR_TRIM_AVG_VERSION", ituffStr);
            /* Prime.Services.DatalogService.WriteToItuff($"{this.ItuffHeader1}FIVR_TRIM_AVG_VERSION\n{this.ItuffHeader2}ssp_{ituffStr}\n"); */

            // at sort we log the trim version to DFF so we can read it at class
            if (this.DFFEnable && this.IsSort)
            {
                Prime.Services.DffService.SetDff("FVREC", this.TrimRecipeVersion.ToString());
            }

            return true;
        }

        /// <summary>Container for Sample Data.</summary>
        public class SampleContainer
        {
            /// <summary>Initializes a new instance of the <see cref="SampleContainer"/> class.</summary>
            public SampleContainer()
            {
                this.Value = 0;
                this.Done = 0;
                this.Error = 0;
                this.GB = 0;
            }

            /// <summary>Gets or sets sample Value.</summary>
            public int Value { get; set; }

            /// <summary>Gets or sets sample Done.</summary>
            public int Done { get; set; }

            /// <summary>Gets or sets sample Error.</summary>
            public int Error { get; set; }

            /// <summary>Gets or sets sample GB.</summary>
            public int GB { get; set; }
        }

        /// <summary>Main container for FIVR data.</summary>
        public class FIVRDomain
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FIVRDomain"/> class.
            /// </summary>
            /// <param name="trims">Dictionary object of the FIVR Trims.</param>
            public FIVRDomain(Dictionary<string, Trim> trims)
            {
                this.Trims = trims;
            }

            /// <summary>Gets the Trim Values.</summary>
            public Dictionary<string, Trim> Trims { get; private set; }

            /// <summary>
            /// clears the structure.
            /// </summary>
            public void Clear()
            {
                foreach (var trim in this.Trims.Values)
                {
                    trim.Clear();
                }
            }

            /// <summary>Sub-container for FIVR Trim data.</summary>
            public class Trim
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="Trim"/> class.
                /// </summary>
                /// <param name="dffToken">Name of DFF Token.</param>
                /// <param name="dffPos">Position in the DFF Token.</param>
                /// <param name="cat">Key to use for the Limits/Ignore/FailPorts structures.</param>
                /// <param name="defaultStr">Name to log to ituff when a default value is used.</param>
                /// <param name="defaultVal">Default value to use if the sample is invalid.</param>
                /// <param name="numPhases">Number of Phases in this object (if applicable).</param>
                public Trim(string dffToken, int dffPos, string cat, string defaultStr, int defaultVal, int numPhases = -1)
                {
                    this.DffToken = dffToken;
                    this.DffPos = dffPos;
                    this.Category = cat;
                    this.DefaultStr = defaultStr;
                    this.DefaultVal = defaultVal;

                    if (numPhases < 0)
                    {
                        this.HasPhases = false;
                        numPhases = 1;
                    }
                    else
                    {
                        this.HasPhases = true;
                    }

                    this.NumPhases = numPhases;

                    this.Samples = new Dictionary<int, List<int>>();
                    this.LastSample = new Dictionary<int, int>();
                    this.Average = new Dictionary<int, int>();
                    this.Range = new Dictionary<int, int>();
                    this.SCDelta = new Dictionary<int, int>();

                    for (var i = 0; i < this.NumPhases; i++)
                    {
                        this.Samples[i] = new List<int>();
                        this.LastSample[i] = -999;
                        this.Average[i] = -999;
                        this.Range[i] = -999;
                        this.SCDelta[i] = -999;
                    }
                }

                /// <summary>Gets a value indicating whether this Trim has Phases.</summary>
                public bool HasPhases { get; private set; }

                /// <summary>Gets the DFF Token Name.</summary>
                public string DffToken { get; private set; }

                /// <summary>Gets the Position within the DFF Token.</summary>
                public int DffPos { get; private set; }

                /// <summary>Gets the Number of Phases.</summary>
                public int NumPhases { get; private set; }

                /// <summary>
                /// Gets the Category string, this is the key to use
                /// for the Limits/Ignore/FailPorts structs.
                /// </summary>
                public string Category { get; private set; }

                /// <summary>Gets the name to log to ituff when a default value is used.</summary>
                public string DefaultStr { get; private set; }

                /// <summary>Gets the default value to use if the sample value is invalid.</summary>
                public int DefaultVal { get; private set; }

                /// <summary>Gets or sets the Last Sample value, used for DFF.</summary>
                private Dictionary<int, int> LastSample { get; set; }

                /// <summary>Gets or sets the Average value.</summary>
                private Dictionary<int, int> Average { get; set; }

                /// <summary>Gets or sets the Range value.</summary>
                private Dictionary<int, int> Range { get; set; }

                /// <summary>Gets or sets the Sort/Class Dela value.</summary>
                private Dictionary<int, int> SCDelta { get; set; }

                private Dictionary<int, List<int>> Samples { get; set; }

                /// <summary>
                /// Saves the sample values.
                /// </summary>
                /// <param name="samples">List of sample values.</param>
                /// <param name="phase">Phase number if that is applicable to this fivr type.</param>
                public void SetSamples(List<int> samples, int phase = 0)
                {
                    this.CheckIfPhaseValid(phase);
                    this.Samples[phase] = samples;
                }

                /// <summary>
                /// Gets the current sample values.
                /// </summary>
                /// <param name="phase">Phase number if that is applicable to this FIVR Trim Type.</param>
                /// <returns>List of sample values.</returns>
                public List<int> GetSamples(int phase = 0)
                {
                    this.CheckIfPhaseValid(phase);
                    return this.Samples[phase];
                }

                /// <summary>
                /// Sets the Last Sample value for this phase.
                /// </summary>
                /// <param name="value">Sample value to save.</param>
                /// <param name="phase">Phase Number if applicable.</param>
                public void SetLastSample(int value, int phase = 0)
                {
                    this.CheckIfPhaseValid(phase);
                    this.LastSample[phase] = value;
                }

                /// <summary>
                /// Sets the Average value for this phase.
                /// </summary>
                /// <param name="value">Sample value to save.</param>
                /// <param name="phase">Phase Number if applicable.</param>
                public void SetAverage(int value, int phase = 0)
                {
                    this.CheckIfPhaseValid(phase);
                    this.Average[phase] = value;
                }

                /// <summary>
                /// Sets the Range value for this phase.
                /// </summary>
                /// <param name="value">Sample value to save.</param>
                /// <param name="phase">Phase Number if applicable.</param>
                public void SetRange(int value, int phase = 0)
                {
                    this.CheckIfPhaseValid(phase);
                    this.Range[phase] = value;
                }

                /// <summary>
                /// Sets the Sort/Class Delta value for this phase.
                /// </summary>
                /// <param name="value">Sample value to save.</param>
                /// <param name="phase">Phase Number if applicable.</param>
                public void SetSCDelta(int value, int phase = 0)
                {
                    this.CheckIfPhaseValid(phase);
                    this.SCDelta[phase] = value;
                }

                /// <summary>
                /// Gets the Last Sample Value.
                /// </summary>
                /// <param name="phase">Phase number if that is applicable to this FIVR Trim Type.</param>
                /// <returns>int values.</returns>
                public int GetLastSample(int phase = 0)
                {
                    this.CheckIfPhaseValid(phase);
                    return this.LastSample[phase];
                }

                /// <summary>
                /// Gets the Average Value.
                /// </summary>
                /// <param name="phase">Phase number if that is applicable to this FIVR Trim Type.</param>
                /// <returns>int values.</returns>
                public int GetAverage(int phase = 0)
                {
                    this.CheckIfPhaseValid(phase);
                    return this.Average[phase];
                }

                /// <summary>
                /// Gets the Range Value.
                /// </summary>
                /// <param name="phase">Phase number if that is applicable to this FIVR Trim Type.</param>
                /// <returns>int values.</returns>
                public int GetRange(int phase = 0)
                {
                    this.CheckIfPhaseValid(phase);
                    return this.Range[phase];
                }

                /// <summary>
                /// Gets the Range Value.
                /// </summary>
                /// <param name="phase">Phase number if that is applicable to this FIVR Trim Type.</param>
                /// <returns>int values.</returns>
                public int GetSCDelta(int phase = 0)
                {
                    this.CheckIfPhaseValid(phase);
                    return this.SCDelta[phase];
                }

                /// <summary>
                /// Clears the struct.
                /// </summary>
                public void Clear()
                {
                    for (var i = 0; i < this.NumPhases; i++)
                    {
                        this.Samples[i] = new List<int>();
                        this.LastSample[i] = -999;
                        this.Average[i] = -999;
                        this.Range[i] = -999;
                        this.SCDelta[i] = -999;
                    }
                }

                private void CheckIfPhaseValid(int phase)
                {
                    if (!this.Samples.ContainsKey(phase))
                    {
                        throw new ArgumentException($"Phase=[{phase}] is invalid, Valid phases are [{string.Join(",", this.Samples.Keys)}]", "phase");
                    }
                }
            }
        }

        /// <summary>Main container for Error data.</summary>
        public class ErrorContainer
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ErrorContainer"/> class.
            /// </summary>
            /// <param name="domain">Domain Name.</param>
            /// <param name="trimType">Trim Type.</param>
            /// <param name="phase">Phase or empty string.</param>
            /// <param name="errPort">Error Port.</param>
            /// <param name="failPort">Final Fail Port.</param>
            /// <param name="msg">Debug Message.</param>
            /// <param name="ituff">Ituff Token.</param>
            public ErrorContainer(string domain, string trimType, string phase, int errPort, int failPort, string msg, string ituff)
            {
                this.Error = errPort;
                this.DebugMsg = $"{trimType} {msg} for {domain} {phase}";
                this.Domain = domain;
                this.TrimType = trimType;
                this.FinalFailPort = failPort;
                this.ItuffMsg = ituff;
            }

            /// <summary>Gets or sets the Domain name.</summary>
            public string Domain { get; set; }

            /// <summary>Gets or sets the Trim Type.</summary>
            public string TrimType { get; set; }

            /// <summary>Gets or sets the ErrorPort.</summary>
            public int Error { get; set; }

            /// <summary>Gets or sets the Debug Message.</summary>
            public string DebugMsg { get; set; }

            /// <summary>Gets or sets the Final ExitPort.</summary>
            public int FinalFailPort { get; set; }

            /// <summary>Gets or sets the Ituff Message.</summary>
            public string ItuffMsg { get; set; }
        }
    }
}
