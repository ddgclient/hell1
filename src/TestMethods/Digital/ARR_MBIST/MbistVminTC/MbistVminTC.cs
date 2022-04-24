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

// ---------------------------------------------------------------
// Created By Tim Kirkham
// ---------------------------------------------------------------
namespace MbistVminTC
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using DDG;
    using Prime;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PhAttributes;
    using Prime.PlistService;
    using Prime.ScoreboardService;
    using Prime.SharedStorageService;
    using Prime.TestMethods;
    using Prime.TestMethods.VminSearch;
    using Prime.VoltageService;

    /// <summary>
    /// This class is intended to overwrite the test method PrimeVminSearchTestMethod.
    /// </summary>
    [PrimeTestMethod]
    public class MbistVminTC : PrimeVminSearchTestMethod, IVminSearchExtensions
    {
        private INoCaptureTest postPlist;
        private ICaptureFailureTest captureFailureTest;
        private IScoreboardLogger logger;
        private List<string> voltageTargets;
        private List<int> flows;
        private int port;
        private bool failureOutsideCTVs;
        private bool resetfailure;
        private string modifiedpatlist;
        private int executionCount = 0;

        private Mapping map;

        private ConcurrentDictionary<string, DieData> diedataset;
        private string prevstartpattern;
        private string startpattern;
        private List<double> voltageStart;
        private List<double> voltageEnd;
        private List<double> voltageInc;
        private IPlistObject plistObject;

        /// <summary>
        /// Enumerate possible test modes.
        /// </summary>
        public enum TestModes
        {
            /// <summary>
            /// MultiVmin search mode.
            /// </summary>
            MultiVmin,

            /// <summary>
            /// SingleVmin search mode.
            /// </summary>
            SingleVmin,

            /// <summary>
            /// Functional mode. It will run single point test.
            /// </summary>
            Functional,

            /// <summary>
            /// Scoreboard mode. It will run single point test.
            /// </summary>
            Scoreboard,
        }

        /// <summary>
        /// Enumerate possible forwarding modes.
        /// </summary>
        public enum ForwardingModes
        {
            /// <summary>
            /// Using start value from VminForwarding but does not update the table.
            /// </summary>
            Input,

            /// <summary>
            /// Ignoring VminForwarding for start value but it updates the final vmin. It can only go higher.
            /// </summary>
            Output,

            /// <summary>
            /// Using VminForwarding for start value and updated final vmin.
            /// </summary>
            InputOutput,

            /// <summary>
            /// No forwarding is enabled.
            /// </summary>
            None,
        }

        /// <summary>
        /// Mode used for BISR to choose what is stored in shared storage.
        /// </summary>
        public enum BisrModes
        {
            /// <summary> Compressed String. </summary>
            Compressed,

            /// <summary> Compressed String skip patmod. </summary>
            Compressed_skippatmod,

            /// <summary> Store Raw BISR chains.  Used when you have no SOFT fusing at fusebox level. </summary>
            Bisr,

            /// <summary> Raw Bisr skip patmod. </summary>
            Bisr_skippatmod,

            /// <summary> Raw Bisr skip patmod. </summary>
            Off,
        }

        /// <summary>
        /// Test class modes.
        /// </summary>
        public enum MbistTestModes
        {
            /// <summary> Default Meaning you are running PRE/Raster/Post for any repair and only supports single VMIN testpoint. </summary>
            HRY,

            /// <summary> Default Meaning you are KS mode not updating HRY tokens. </summary>
            KS,

            /// <summary> Retest logic - separate instance that runs on all memories previously recorded as "repairable" or "Y". </summary>
            PostRepair,
        }

        /// <summary> PrintItuff modes. </summary>
        public enum ItuffPrint
        {
            /// <summary> Print ITUFF for HRY. </summary>
            Hry,

            /// <summary> Hry and VminPerMem. </summary>
            Hry_VminPerMem,

            /// <summary> Hry_VminPerDomain </summary>
            Hry_VminPerDomain,

            /// <summary> VminPerDomain </summary>
            VminPerDomain,

            /// <summary> VminPerMem. </summary>
            VminPerMem,

            /// <summary> Disabled. </summary>
            Disabled,
        }

        /// <summary> PrintItuff modes. </summary>
        public enum DFFOperation
        {
            /// <summary> Disabled. </summary>
            Disabled,

            /// <summary> Write BISR. </summary>
            Write_BISR,

            /// <summary> Write BISR and Recovery. </summary>
            Write_BISR_REC,

            /// <summary> Write Recovery. </summary>
            Write_REC,

            /// <summary> Read BISR. </summary>
            Read_BISR,

            /// <summary> Read BISR and Recovery. </summary>
            Read_BISR_REC,

            /// <summary> Read Recovery. </summary>
            Read_REC,
        }

        /// <summary>
        /// Recovery Modes.
        /// </summary>
        public enum EnableStates
        {
            /// <summary>
            /// Disabled.
            /// </summary>
            Disabled,

            /// <summary>
            /// Collect recoverable IPs.
            /// </summary>
            Enabled,
        }

        /// <summary> Gets or sets enables default or post repair flows. </summary>
        public TestModes TestMode { get; set; } = TestModes.SingleVmin;

        /// <summary>
        /// Gets or sets comma separated corner identifiers.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString CornerIdentifiers { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets forwarding mode.
        /// </summary>
        public ForwardingModes ForwardingMode { get; set; } = ForwardingModes.None;

        /// <summary>
        /// Gets or sets forwarding mode.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public TestMethodsParams.String InitialMaskBits { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a list of voltage overrides from VminForwarding.
        /// </summary>
        public TestMethodsParams.String VoltageConverter { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an offset to applied voltage.
        /// </summary>
        public TestMethodsParams.String VoltagesOffset { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets PostPlist execution.
        /// </summary>
        public TestMethodsParams.Plist PostPlist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets FailCaptureCount. Default 1 will set stop-on-first-fail.
        /// Any value greater than 1 will run full plist unless used in combination with ReturnOn plist options.
        /// </summary>
        public TestMethodsParams.UnsignedInteger FailCaptureCount { get; set; } = int.MaxValue;

        /// <summary>
        /// Gets or sets CTV capture pins.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString CtvPins { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets mask pins.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString MaskPins { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets shared storage key for a pre-populated list of PatConfigSetPoints by CornerIdentification.
        /// </summary>
        public TestMethodsParams.String CornerPatConfigSetPoints { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets vmin result. Stores value in SharedStorage using comma-separated key names with Context.DUT.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public TestMethodsParams.CommaSeparatedString VminResult { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets trigger map.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public TestMethodsParams.String TriggerMap { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets trigger levels test condition name.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public TestMethodsParams.LevelsCondition TriggerLevelsCondition { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets DTS configuration name. Empty configuration name means DTS capture move is disabled.
        /// </summary>
        public TestMethodsParams.String DtsConfiguration { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the state to force parsing of a new config file for debug scenarios.
        /// </summary>
        public EnableStates ForceConfigFileParseState { get; set; } = EnableStates.Disabled;

        /// <summary>
        /// Gets or sets whether to collect per pattern debug.  Per controller per pattern result information.
        /// </summary>
        public EnableStates AdvancedDebug { get; set; } = EnableStates.Disabled;

        /// <summary>
        /// Gets or sets mode for printing hry string to the ituff.
        /// </summary>
        public ItuffPrint PrintToItuff { get; set; } = ItuffPrint.Disabled;

        /// <summary>
        /// Gets or sets mode for clearing or not global HRY string.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString ClearVariables { get; set; } = string.Empty;

        /// <summary> Gets or sets set the recovery mode of the instancMbistTestModee. </summary>
        public EnableStates RecoveryModeDownbin { get; set; } = EnableStates.Disabled;

        /// <summary> Gets or sets  Mode used to store BISR data in TP. </summary>
        public BisrModes BisrMode { get; set; } = BisrModes.Bisr;

        /// <summary> Gets or sets  Mode used to store BISR data in TP. </summary>
        public DFFOperation DffOperation { get; set; } = DFFOperation.Disabled;

        /// <summary> Gets or sets  VFDM config. </summary>
        public TestMethodsParams.String VFDMconfig { get; set; } = string.Empty;

        /// <summary> Gets or sets  Mapping Config. </summary>
        public TestMethodsParams.String MappingConfig { get; set; } = string.Empty;

        /// <summary> Gets or sets  Threads. </summary>
        public TestMethodsParams.Integer Threads { get; set; } = 0;

        /// <summary> Gets or sets enables default or post repair flows. </summary>
        public MbistTestModes MbistTestMode { get; set; } = MbistTestModes.HRY;

        /// <summary> Gets or sets base# used only by MBIST tools since no otherway to bypass flow. </summary>
        public TestMethodsParams.Integer ScoreboardBaseNumberMbist { get; set; } = 0;

        /// <summary> Gets or sets For running multiple fuse values with different ITUFF prints.</summary>
        public TestMethodsParams.String ItuffNameExtenstion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets flow number.
        /// </summary>
        public TestMethodsParams.CommaSeparatedInteger FlowNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets LookupTableConfigurationFile.
        /// </summary>
        public TestMethodsParams.String LookupTableConfigurationFile { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets RecoveryConfigurationFile.
        /// </summary>
        public TestMethodsParams.String RecoveryConfigurationFile { get; set; } = string.Empty;

        /// <summary> Gets or sets to ignore state.</summary>
        public EnableStates IgnorePrePstFail { get; set; } = EnableStates.Disabled;

        /// <summary>
        /// Gets or sets interface to VminForwarding_. Stores tuple with corner name, flow and interface object.
        /// </summary>
        protected List<Tuple<string, int, IVminForwardingCorner>> VminForwarding_ { get; set; }

        /// <summary>
        /// Gets a wrapper for DTS service.
        /// </summary>
        protected DTSHandler DTSHandler_ { get; } = new DTSHandler();

        /// <summary>
        /// Gets or sets a value indicating whether incremental search mode is enabled.
        /// </summary>
        protected bool IsStartOnFirstFail_ { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether stop on fail pattern is enabled.
        /// </summary>
        protected bool ReturnOnGlobalStickyError_ { get; set; } = true;

        /// <summary>
        /// Gets or sets initial mask array for each search iteration.
        /// </summary>
        [ExcludeFromCodeCoverage]
        protected BitArray InitialSearchMask_ { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether per target increments should be printed to ituff.
        /// </summary>
        protected bool PrintPerTargetIncrements_ { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether apply search voltage should be skipped. Used for functional modes.
        /// </summary>
        protected bool SkipApplySearchVoltage_ { get; set; }

        /// <summary>
        /// Gets or sets incoming mask.
        /// </summary>
        protected BitArray IncomingMask_ { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether decoded results for masked elements should be ignored.
        /// </summary>
        protected bool IgnoreMaskedResults_ { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether FivrVoltageMode is enabled.
        /// </summary>
        protected bool IsFivrVoltageMode_ { get; set; } = false;

        /// <summary>
        /// Gets or sets voltage converter options from commandline.
        /// </summary>
        protected VoltageConverterOptions VoltageConverterOptions_ { get; set; }

        /// <summary>
        /// Writes the given data to Ituff using STRGVAL format with the given tname as the TestName.
        /// </summary>
        /// <param name="tname">TestName to use. Overrides the default tname with SetCustomTname() api.</param>
        /// <param name="data">strgval data to log.</param>
        public static void WriteStrgvalToItuff(string tname, string data)
        {
            var writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            writer.SetCustomTname(tname);
            writer.SetData(data);
            Prime.Services.DatalogService.WriteToItuff(writer);
        }

        /// <inheritdoc />
        [Returns(0, PortType.Fail, "Failed")]
        [Returns(1, PortType.Pass, "Passed")]
        [Returns(2, PortType.Pass, "Repairable Arrays")]
        [Returns(3, PortType.Pass, "Recovery")]
        [Returns(4, PortType.Pass, "Repair and Recovery")]
        [Returns(5, PortType.Error, "Config Error")]
        [Returns(6, PortType.Error, "Missing Corner Case")]
        [Returns(7, PortType.Fail, "Failed DTS Monitor")]
        [Returns(8, PortType.Fail, "Reset failure")]

        public override int Execute()
        {
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Executing MbistVminTC instance");

            if (this.ClearVariables.ToList().Count > 0)
            {
                foreach (var pin in this.CtvPins.ToList())
                {
                    this.diedataset[pin].ClearVariable(this.ClearVariables.ToList());
                }

                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Clear All Globals enabled, shared storage was successfully cleared.");
            }

            foreach (var pin in this.CtvPins.ToList())
            {
                this.diedataset[pin].Pulldatassddf(this.ClearVariables.ToList());
                this.diedataset[pin].VoltageLevelExecutionResultsList = new List<VoltageLevelExecutionResult>();
            }

            this.startpattern = "UNKNOWN";
            this.prevstartpattern = "UNKNOWN";

            int result = base.Execute();

            // this.Console?.PrintDebug($"Execution ended at {string.Join(string.Empty, this.currentVoltageValues)}");
            this.ExecutePostInstance();

            this.port = this.MbistPostInstancePerDie();

            if (this.failureOutsideCTVs == true && this.IgnorePrePstFail == EnableStates.Disabled)
            {
                this.port = (int)VoltageLevelExecutionResult.FlowFlags.INITVAL;
            }

            if (!this.DTSHandler_.EvaluateLimits())
            {
                this.port = 7;
            }

            return this.port;
        }

        /// <inheritdoc />
        public override void CustomVerify()
        {
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] begin Custom Verify Flow.");
            this.InitializeVoltageTargets();
            this.VerifyCreateLookupTable();
            this.VerifyFeatureSwitchSettings();
            this.VerifyForwarding();
            this.VerifySingleVminMode();
            this.VerifyMultiVminMode();
            this.VerifySinglePointMode();
            this.VerifyPostPlist();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        BitArray IVminSearchExtensions.GetInitialMaskBits()
        {
            if (this.InitialSearchMask_.Count == this.voltageTargets.Count)
            {
                return new BitArray(this.InitialSearchMask_);
            }

            return new BitArray(this.voltageTargets.Count, false);
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        IFunctionalTest IVminSearchExtensions.GetFunctionalTest(string patlist, string levelsTc, string timingsTc, string prePlist)
        {
            this.VerifyDts();
            this.plistObject = Prime.Services.PlistService.GetPlistObject(this.Patlist);
            var currentReturnOnValue = string.Empty;
            try
            {
                currentReturnOnValue = this.plistObject.GetOption("ReturnOn");
            }
            catch
            {
                // ignore
            }

            if (this.TestMode != TestModes.Scoreboard && this.ReturnOnGlobalStickyError_ == true && this.MbistTestMode == MbistTestModes.KS)
            {
                if (currentReturnOnValue != "GlobalStickyError")
                {
                    this.plistObject.SetOption("ReturnOn", "GlobalStickyError");
                    this.plistObject.Resolve();
                }
            }
            else if (this.TestMode == TestModes.Scoreboard || this.ReturnOnGlobalStickyError_ == false)
            {
                if (!string.IsNullOrEmpty(currentReturnOnValue))
                {
                    this.plistObject.RemoveOptions(new List<string> { "ReturnOn" });
                    this.plistObject.Resolve();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(currentReturnOnValue))
                {
                    this.plistObject.RemoveOptions(new List<string> { "ReturnOn" });
                    this.plistObject.Resolve();
                }
            }

            var ctvPins = this.CtvPins.ToList();
            if (this.DTSHandler_.IsDtsEnabled() && !ctvPins.Contains(this.DTSHandler_.GetCtvPinName()))
            {
                ctvPins.Add(this.DTSHandler_.GetCtvPinName());
            }

            this.captureFailureTest = this.TestMode == TestModes.Scoreboard ?
                Prime.Services.FunctionalService.CreateCaptureFailureAndCtvPerPinTest(patlist, levelsTc, timingsTc, this.CtvPins.ToList(), this.FailCaptureCount, 1, prePlist) :
                Prime.Services.FunctionalService.CreateCaptureFailureAndCtvPerPinTest(patlist, levelsTc, timingsTc, this.CtvPins.ToList(), this.FailCaptureCount, prePlist);

            if (!string.IsNullOrEmpty(this.MaskPins))
            {
                this.captureFailureTest.SetPinMask(this.MaskPins.ToList());
            }

            if (this.IsStartOnFirstFail_)
            {
                this.captureFailureTest.EnableStartPatternOnFirstFail();
            }
            else
            {
                this.captureFailureTest.DisableStartPattern();
            }

            if (!string.IsNullOrEmpty(this.TriggerMap))
            {
                this.captureFailureTest.SetTriggerMap(this.TriggerMap);
            }

            return this.captureFailureTest;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        void IVminSearchExtensions.ApplyMask(BitArray maskBits, IFunctionalTest functionalTest)
        {
            functionalTest.SetPinMask(this.MaskPins.ToList());
        }

        /// <inheritdoc/>
        void IVminSearchExtensions.ApplyPreExecuteSetup(string plistName)
        {
            this.DTSHandler_.Reset();
            this.SetIncomingMask();
        }

        /// <inheritdoc/>
        void IVminSearchExtensions.ApplyInitialVoltage(IVoltage voltageObject)
        {
            var overrides = DDG.VoltageHandler.GetVoltageOverrides(this.VoltageConverterOptions_);
            DDG.VoltageHandler.ApplyInitialVoltage(voltageObject, this.LevelsTc, overrides);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        void IVminSearchExtensions.ApplyPreSearchSetup(string plistName)
        {
            this.SetInitialMask();
        }

        /// <inheritdoc />
        void IVminSearchExtensions.ApplySearchVoltage(IVoltage voltageObject, List<double> voltageValues)
        {
            if (this.SkipApplySearchVoltage_)
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: skipping voltage setup. Using initial voltage.");
                return;
            }

            DDG.VoltageHandler.ApplySearchVoltage(voltageObject, voltageValues, this.VoltagesOffset);
        }

        /// <inheritdoc />
        BitArray IVminSearchExtensions.ProcessPlistResults(bool plistExecuteResult, IFunctionalTest functionalTest)
        {
            ConcurrentDictionary<string, string> ctvcaptures = new ConcurrentDictionary<string, string>();
            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Started ---- ");
            this.failureOutsideCTVs = false;
            this.resetfailure = false;
            var failoutofrange = false;
            var currentrun = new Dictionary<string, CurrentRunStateTracking>();
            foreach (var pin in this.CtvPins.ToList())
            {
                currentrun[pin] = new CurrentRunStateTracking(this.VoltageTargets.ToList(), this.Console);
                this.diedataset[pin].Hryclass.ClearCurHryString();
                var dtsctvs = string.Empty;
                var bisrctvs = new Dictionary<string, string>();
                this.diedataset[pin].Vminclass.VoltageStateReset();
                this.diedataset[pin].Voltagevalues = this.VoltageValues;
            }

            this.Console?.PrintDebug($"\n\n[{MethodBase.GetCurrentMethod().Name}]--------------------------------------------------------");
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Running FULL CTV capture parsing.");
            var captureFailure = functionalTest as ICaptureFailureAndCtvPerPinTest;
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Start pattern [{this.startpattern}])");
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] plist execution pass: [{plistExecuteResult}])");
            if (captureFailure != null && plistExecuteResult == false)
            {
                // this.PrintCaptureFailures(captureFailure);
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Grabbing failure data.");

                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Captured failure capture.");
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Getting ctvs failure capture.");
                List<string> failingPinNames = captureFailure.GetFailingPinNames();
                IFailureData failpattern = captureFailure.GetPerCycleFailures().First();

                this.resetfailure = this.plistObject.IsPatternAnAmble(failpattern.GetPatternName());

                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Fail Pins[{string.Join(",", failingPinNames)}])");
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] plist from config : {this.modifiedpatlist}");
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] CapturePins : {this.CtvPins}");

                if (failingPinNames.Count == 0)
                {
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Fullpassnofailpins)");
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] No failures captured shouldn't get here.");

                    foreach (var pin in this.CtvPins.ToList())
                    {
                        this.diedataset[pin].SetFullPassState(true);
                    }
                }
                else
                {
                    foreach (var pin in failingPinNames)
                    {
                        if (this.CtvPins.ToList().Contains(pin))
                        {
                            string ctvResults = captureFailure.GetCtvData(pin);
                            ctvcaptures[pin] = ctvResults;

                            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Amount of CTVs captured to prime string: {ctvResults.Length}.");
                            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Captured CTVs into string : {ctvResults}.");
                        }
                    }

                    foreach (var pin in this.CtvPins.ToList())
                    {
                        if (ctvcaptures.ContainsKey(pin))
                        {
                            if (this.diedataset[pin].Hryclass.Contains1(ctvcaptures[pin]) == false && this.resetfailure == false)
                            {
                                this.diedataset[pin].SetFullPassState(true);
                            }
                            else
                            {
                                var resultstring = this.diedataset[pin].Hryclass.RunAllCTVPerPlist(this.modifiedpatlist, ctvcaptures[pin], this.diedataset[pin].Virtualfuseclass, this.startpattern);
                                var bisrctvs = resultstring.Item2;
                                failoutofrange = resultstring.Item3;

                                if (this.LogLevel == PrimeLogLevel.PRIME_DEBUG)
                                {
                                    foreach (KeyValuePair<long, List<string>> thread in this.diedataset[pin].Hryclass.DebugCaptures)
                                    {
                                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.diedataset[pin].Diename}: [Pattern/Thread]: {thread.Key}");
                                        foreach (var line in thread.Value)
                                        {
                                            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.diedataset[pin].Diename}: \t{line}");
                                        }
                                    }
                                }

                                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.diedataset[pin].Diename}: Successfully parsed all CTVS Index/Value : {string.Join(string.Empty, this.diedataset[pin].Hryclass.CurrentHryString)}");
                                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.diedataset[pin].Diename}: Now Checking priority updates");
                                this.diedataset[pin].BisrRecoveryFlows(bisrctvs, ref currentrun, pin);

                                if (this.resetfailure)
                                {
                                    this.diedataset[pin].VoltageLevelExecutionResultsList.Last().FlowFlag = VoltageLevelExecutionResult.FlowFlags.RESET;
                                }
                            }
                        }
                        else
                        {
                            this.diedataset[pin].SetFullPassState(true);
                        }
                    }
                }
            }
            else
            {
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Fullpassnofailpins)");
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] No failures captured shouldn't get here.");

                foreach (var pin in this.CtvPins.ToList())
                {
                    this.diedataset[pin].SetFullPassState(true);
                }
            }

            try
            {
                if (this.TestMode != TestModes.Functional && this.TestMode != TestModes.Scoreboard)
                {
                    var startpatterntuple = captureFailure.GetStartPattern();
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Start Pattern captured from function {startpatterntuple.Item1}.");
                    this.startpattern = startpatterntuple.Item1;
                    this.prevstartpattern = startpatterntuple.Item1;
                }
                else
                {
                    this.startpattern = "UNKNOWN";
                }
            }
            catch
            {
                this.startpattern = "UNKNOWN";
            }

            if (this.AdvancedDebug == EnableStates.Enabled)
            {
                foreach (var pin in ctvcaptures.Keys)
                {
                    this.ConcurrentDebugPrint(this.diedataset[pin].Hryclass.ConcurrentFails);
                }
            }

            var idx = 0;
            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- PrintExecutions ---- ");
            foreach (var pin in this.CtvPins.ToList())
            {
                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- {this.diedataset[pin].Diename} Results---- ");
                foreach (VoltageLevelExecutionResult entry in this.diedataset[pin].VoltageLevelExecutionResultsList)
                {
                    this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] Stored Voltage Results: {idx}");
                    entry.ExecutionPrint(this.diedataset[pin].Diename);
                    idx += 1;
                }
            }

            this.executionCount += 1;
            if (this.resetfailure == true || (failoutofrange == true && this.ReturnOnGlobalStickyError_ == false))
            {
                if (failoutofrange == true && this.ReturnOnGlobalStickyError_ == false)
                {
                    this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] The failcounter was not adequate to collect all CTVs please increase");
                }

                if (this.ReturnOnGlobalStickyError_ == true)
                {
                    this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] Stop on fail pattern is enabled.");
                }

                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Stopped ---- ");
                return new BitArray(this.VoltageTargets.ToList().Count, false);
            }
            else
            {
                if (this.Combinedieresults() == true)
                {
                    if (this.TestMode == TestModes.Functional || this.TestMode == TestModes.Scoreboard)
                    {
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Exiting execution since you are in Scoreboard/Functional Mode.  Test has passed.  Check levels for voltage setpoint.");
                    }
                    else
                    {
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Exiting execution at {string.Join(string.Empty, this.VoltageValues)}V.");
                    }

                    this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] FULL_PASS<- Stopped ---- ");
                    return new BitArray(this.VoltageTargets.ToList().Count, false);
                }
                else
                {
                    if (this.TestMode == TestModes.Functional || this.TestMode == TestModes.Scoreboard)
                    {
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Exiting execution since you are in Scoreboard/Functional Mode.  Test has failed.  Check levels for voltage setpoint.");
                    }
                    else
                    {
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Need to run next voltage level {string.Join(",", this.VoltageValues)}V.");
                    }

                    foreach (var pin in this.CtvPins.ToList())
                    {
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] From running {this.diedataset[pin].Diename} the flow it was found to be: {this.diedataset[pin].VoltageLevelExecutionResultsList[this.diedataset[pin].VoltageLevelExecutionResultsList.Count - 1].FlowFlag} Continuing search");
                    }

                    if (this.CheckPerVminEnabledAcrossDies() == true)
                    {
                        this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Stopped ---- ");
                        return new BitArray(this.ReturnpervoltagePassAcrossDies());
                    }
                    else
                    {
                        this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Stopped ---- ");
                        return new BitArray(this.VoltageTargets.ToList().Count, false);
                    }
                }
            }
        }

        /// <summary> Runs all new flows after HRY data is collected. Bisr/Recovery/PerArrayVMIN. </summary>
        /// <returns>List of bool pass or fail per voltage domain.</returns>
        public bool Combinedieresults()
        {
            var pass = true;
            foreach (var pin in this.CtvPins.ToList())
            {
                if (this.diedataset[pin]
                        .VoltageLevelExecutionResultsList[
                            this.diedataset[pin].VoltageLevelExecutionResultsList.Count - 1].FlowFlag !=
                    VoltageLevelExecutionResult.FlowFlags.FULL_PASS
                    && this.diedataset[pin]
                        .VoltageLevelExecutionResultsList[
                            this.diedataset[pin].VoltageLevelExecutionResultsList.Count - 1].FlowFlag !=
                    VoltageLevelExecutionResult.FlowFlags.REPAIRABLE
                    && this.diedataset[pin]
                        .VoltageLevelExecutionResultsList[
                            this.diedataset[pin].VoltageLevelExecutionResultsList.Count - 1].FlowFlag !=
                    VoltageLevelExecutionResult.FlowFlags.RECOVERY)
                {
                    pass = false;
                }
            }

            return pass;
        }

        /// <summary> Runs Check of permin being enabled and aggregates the data. </summary>
        /// <returns>List of bool pass or fail for whether pervmin was turned on.</returns>
        public bool CheckPerVminEnabledAcrossDies()
        {
            var pass = true;
            foreach (var pin in this.CtvPins.ToList())
            {
                if (this.diedataset[pin].Vminclass.Pervminenabled() == true)
                {
                    pass = false;
                }
            }

            return pass;
        }

        /// <summary> Runs Check of permin being enabled and aggregates the data. </summary>
        /// <returns>List of bool pass or fail for whether pervmin was turned on.</returns>
        public int MbistPostInstancePerDie()
        {
            List<int> priority = new List<int>() { 5, 6, 8, 7, 0, 4, 3, 2, 1 };
            var result = 8;
            var dieResults = new Dictionary<string, int>();
            foreach (var pin in this.CtvPins.ToList())
            {
                dieResults[this.diedataset[pin].Diename] = this.diedataset[pin].MbistPostInstance();
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] {this.diedataset[pin].Diename} expected port: {dieResults[this.diedataset[pin].Diename]}");
                if (dieResults[this.diedataset[pin].Diename] < result)
                {
                    result = dieResults[this.diedataset[pin].Diename];
                }
            }

            return result;
        }

        /// <summary> Accumulates voltage data to a single string. </summary>
        /// <returns>List of bool pass or fail for whether pervmin was turned on.</returns>
        public List<double> PerDieVoltageAccumulation()
        {
            var dieResults = new Dictionary<string, int>();
            double[] newvmin = new double[this.VoltageTargets.ToList().Count];

            var idx = 0;
            foreach (var item in this.VoltageTargets.ToList())
            {
                newvmin[idx] = -5555;
                idx++;
            }

            foreach (var pin in this.CtvPins.ToList())
            {
                var diepass = true;
                int recIndexchoice = this.diedataset[pin].ChooseRecoveryOption();
                if ((int)this.diedataset[pin].VoltageLevelExecutionResultsList[recIndexchoice].FlowFlag == 0
                    || (int)this.diedataset[pin].VoltageLevelExecutionResultsList[recIndexchoice].FlowFlag > 4)
                {
                    diepass = false;
                }

                this.Console?.PrintDebug($"[testpass]: {diepass}, flowflag: {(int)this.diedataset[pin].VoltageLevelExecutionResultsList[recIndexchoice].FlowFlag}, .");

                idx = 0;

                foreach (var voltage in this.diedataset[pin].VoltageLevelExecutionResultsList[recIndexchoice].Voltagestested)
                {
                    if (voltage == false)
                    {
                        newvmin[idx] = -9999;
                    }
                    else
                    {
                        if (this.diedataset[pin].VoltageLevelExecutionResultsList[recIndexchoice].Vmin[idx] > newvmin[idx])
                        {
                            newvmin[idx] = this.diedataset[pin].VoltageLevelExecutionResultsList[recIndexchoice].Vmin[idx];
                        }
                    }

                    idx++;
                }
            }

            return newvmin.ToList();
        }

        /// <summary> Returns per voltage by aggregating across all Dies. </summary>
        /// <returns>List of bool pass or fail for whether another execution has to happen.</returns>
        public bool[] ReturnpervoltagePassAcrossDies()
        {
            var status = new bool[this.voltageTargets.Count];
            var idx = 0;
            foreach (var loc in status)
            {
                status[idx] = true;
            }

            foreach (var pin in this.CtvPins.ToList())
            {
                var currentstatus = this.diedataset[pin].Vminclass.VoltagePassRead();
                idx = 0;
                foreach (var stat in currentstatus)
                {
                    if (stat == false)
                    {
                        status[idx] = false;
                    }
                }
            }

            return status;
        }

        /// <summary> Failing pattern capture per die. </summary>
        /// <returns>List of string of failures.</returns>
        public List<string> FailingPatternsPerDie()
        {
            var failingpats = new List<string>();
            foreach (var pin in this.CtvPins.ToList())
            {
                int recIndexchoice = this.diedataset[pin].ChooseRecoveryOption();
                failingpats.AddRange(this.diedataset[pin].VoltageLevelExecutionResultsList[recIndexchoice].ScoreboardFails.Keys);
            }

            return this.diedataset[this.CtvPins.ToList()[0]].Hryclass
                .FindlimitingPattern(failingpats.Distinct().ToList(), this.modifiedpatlist);
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        List<double> IVminSearchExtensions.GetStartVoltageValues(List<string> startVoltagesKeys)
        {
            var startVoltages = this.CalculateStartVoltageValues(startVoltagesKeys);
            if (startVoltages.Count > 1 && this.voltageTargets.Count == 1)
            {
                startVoltages = new List<double> { this.CalculateStartVoltageValues(startVoltagesKeys).Max() };
            }

            this.voltageInc = new List<double>();
            this.voltageStart = startVoltages;
            foreach (var loc in this.voltageStart)
            {
                this.voltageInc.Add(this.StepSize);
            }

            return startVoltages;
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        List<double> IVminSearchExtensions.GetEndVoltageLimitValues(List<string> endVoltageLimitKeys)
        {
            return this.voltageEnd = this.CalculateVoltageLimits(endVoltageLimitKeys);
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        List<double> IVminSearchExtensions.GetLowerStartVoltageValues(List<string> lowerStartVoltageKeys)
        {
            return this.CalculateVoltageLimits(lowerStartVoltageKeys);
        }

        /// <inheritdoc/>
        bool IVminSearchExtensions.IsSinglePointMode() => this.TestMode == TestModes.Functional
                                                          || this.TestMode == TestModes.Scoreboard
                                                          || (VminForwarding.Service.IsSinglePointMode() && this.ForwardingMode != ForwardingModes.None);

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        IVoltage IVminSearchExtensions.GetSearchVoltageObject(List<string> targets, string plistName)
        {
            this.IsFivrVoltageMode_ = this.FeatureSwitchSettings.ToList().Contains("fivr_mode_on");
            if (this.IsFivrVoltageMode_)
            {
                if (string.IsNullOrEmpty(this.FivrCondition))
                {
                    throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: fivr_mode_on requires to to {nameof(this.FivrCondition)} .");
                }

                var fivrObject = DDG.VoltageHandler.GetVoltageObject(targets, this.LevelsTc, this.Patlist, this.FivrCondition, null, this.VoltageConverter, out var fivrOptions);
                this.VoltageConverterOptions_ = fivrOptions;
                return fivrObject;
            }

            var dpsObject = DDG.VoltageHandler.GetVoltageObject(targets, this.LevelsTc, this.Patlist, null, this.TriggerLevelsCondition, this.VoltageConverter.ToString(), out var dpsOptions);
            this.VoltageConverterOptions_ = dpsOptions;
            return dpsObject;
        }

        /// <inheritdoc/>  //TODO:TIM UPDATE for LOGGING DATA
        int IVminSearchExtensions.PostProcessSearchResults(List<SearchResultData> searchResults)
        {
            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Started ---- ");
            var resultVoltages = this.PerDieVoltageAccumulation();

            var perdomainticks = this.GrabVoltageTicksPerDomain(resultVoltages);

            List<string> limitingPattern = new List<string>();
            if (this.prevstartpattern != "UNKNOWN")
            {
                limitingPattern.Add(this.prevstartpattern);
            }

            if ((this.TestMode != TestModes.Functional && this.TestMode != TestModes.Scoreboard) || this.VminForwarding_ != null)
            {
                this.PrintSearchResults(resultVoltages, limitingPattern, perdomainticks, perdomainticks.Max());
                ExtendedDataLogger.LogVminConfiguration(this.VminForwarding_);
            }

            this.LogSinglePointModeScoreboard();
            this.UpdateVminResult(resultVoltages);
            this.UpdateVminForwarding(resultVoltages);
            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Stopped ---- ");
            return 1;
        }

        /// <summary>
        /// Logs the joined resulting voltages from the multi-pass search as well as the start and end voltage values for all search targets.
        /// Logs the voltage limiting patterns when the patternNameMap is not empty.
        /// Logs the voltage increments per target when the option is enabled through the FeatureSwitchSettings parameter.
        /// </summary>
        /// <param name="resultVoltages">List of result voltages.</param>
        /// <param name="limitingPatterns">List of limiting patterns.</param>
        /// <param name="perTargetIncrements">List of per domain increments.</param>
        /// <param name="executionCount">Total or max number of executions.</param>
        public void PrintSearchResults(List<double> resultVoltages, List<string> limitingPatterns, List<uint> perTargetIncrements, uint executionCount)
        {
            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Started ---- ");
            if (resultVoltages is null || limitingPatterns is null || perTargetIncrements is null)
            {
                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Stopped ---- ");
                return;
            }

            var targetValueSeparator = "_";
            var tokenValueSeparator = "|";
            var outputSearchVoltages = string.Join(targetValueSeparator, resultVoltages.Select(i => i < 0 ? $"{i:F0}" : $"{i:F3}"));
            var outputStartVoltages = string.Join(targetValueSeparator, this.voltageStart.Select(i => $"{i:F3}"));
            var outputEndVoltages = string.Join(targetValueSeparator, this.voltageEnd.Select(i => $"{i:F3}"));
            var outputVoltages = outputSearchVoltages + tokenValueSeparator + outputStartVoltages + tokenValueSeparator + outputEndVoltages + tokenValueSeparator + executionCount;

            var strgvalWriter = Services.DatalogService.GetItuffStrgvalWriter();
            strgvalWriter.SetData(outputVoltages);
            Prime.Services.DatalogService.WriteToItuff(strgvalWriter);

            if (!string.IsNullOrEmpty(this.PatternNameMap))
            {
                this.WriteLimitingPatternsToItuff(limitingPatterns, this.PatternNameMap);
            }

            this.WritePerTargetIncrementsToItuff(perTargetIncrements);
            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Stopped ---- ");
        }

        /// <summary> Runs all new flows after HRY data is collected. Bisr/Recovery/PerArrayVMIN. </summary>
        /// <param name="vmin"> List of voltages.</param>
        /// <returns>List of ticks per domain.</returns>
        public List<uint> GrabVoltageTicksPerDomain(List<double> vmin)
        {
            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Started ---- ");
            var ticksPertarget = new List<uint>();
            var i = 0;
            foreach (var voltage in vmin)
            {
                if (voltage == -9999)
                {
                    var curstep = Math.Ceiling(((this.voltageEnd[i] - this.voltageStart[i]) / this.voltageInc[i]) + 1);
                    ticksPertarget.Add((uint)curstep);
                }
                else
                {
                    if (this.voltageEnd[i] - this.voltageStart[i] == 0)
                    {
                        ticksPertarget.Add(1);
                    }
                    else
                    {
                        var curstep = Math.Ceiling(((vmin[i] - this.voltageStart[i]) / this.voltageInc[i]) + 1);
                        ticksPertarget.Add((uint)curstep);
                    }
                }

                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Stopped ---- ");
            }

            return ticksPertarget;
        }

        /// <summary>
        /// Updates VminResult. If one target was already tested it will take fail result or max voltage.
        /// </summary>
        /// <param name="voltageResults">accumulated voltage results.</param>
        protected internal void UpdateVminResult(List<double> voltageResults)
        {
            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Started ---- ");
            if (string.IsNullOrEmpty(this.VminResult) || voltageResults == null)
            {
                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Stopped ---- ");
                return;
            }

            var vminResultTokens = this.VminResult.ToList();
            if (vminResultTokens.First().StartsWith("D."))
            {
                var concatenated = voltageResults.Aggregate(string.Empty, (current, result) => current + $"{result:N3}v");
                concatenated.Remove(concatenated.Length - 1);
                var key = vminResultTokens.First().Substring(2);
                Prime.Services.DffService.SetDff(key, concatenated);
            }
            else if (vminResultTokens.Count == 1)
            {
                var key = vminResultTokens.First();
                var value = voltageResults.Min().Equals(-9999, 3) ? -9999 : voltageResults.Max();
                Prime.Services.SharedStorageService.InsertRowAtTable(key, value, Context.DUT);
            }
            else if (voltageResults.Count == vminResultTokens.Count)
            {
                for (var i = 0; i < voltageResults.Count; i++)
                {
                    Prime.Services.SharedStorageService.InsertRowAtTable(vminResultTokens[i], voltageResults[i], Context.DUT);
                }
            }
            else
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: number of SharedStorage keys in {nameof(this.VminResult)} must match number of {nameof(this.VoltageTargets)}.");
            }

            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Stopped ---- ");
        }

        /// <summary>
        /// Calculates start voltage using incoming parameter and vmin forwarding.
        /// </summary>
        /// <param name="startVoltagesKeys">Starting voltage from parameter.</param>
        /// <returns>List of starting voltages.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Invalid mode.</exception>
        [ExcludeFromCodeCoverage]
        protected internal List<double> CalculateStartVoltageValues(List<string> startVoltagesKeys)
        {
            var startVoltagesValues = this.ExpandVoltageKeys(startVoltagesKeys);
            if (string.IsNullOrEmpty(this.CornerIdentifiers)
                || (this.ForwardingMode != ForwardingModes.Input && this.ForwardingMode != ForwardingModes.InputOutput))
            {
                return new List<double>(startVoltagesValues);
            }

            for (var i = 0; i < startVoltagesValues.Count; i++)
            {
                var vminForwardingVoltage = this.VminForwarding_[i].Item3.GetStartingVoltage(startVoltagesValues[i]);
                if (vminForwardingVoltage > startVoltagesValues[i])
                {
                    startVoltagesValues[i] = vminForwardingVoltage;
                }
            }

            return new List<double>(startVoltagesValues);
        }

        /// <summary>
        /// Calculates voltage limits from keys.
        /// </summary>
        /// <param name="voltageKeys">Voltage limit keys. It can take DFF, SharedStorage or a list of values.</param>
        /// <returns>Evaluated doubles.</returns>
        [ExcludeFromCodeCoverage]
        protected internal List<double> CalculateVoltageLimits(List<string> voltageKeys)
        {
            var voltages = this.ExpandVoltageKeys(voltageKeys);
            return this.TestMode == TestModes.SingleVmin ? new List<double> { voltages.Max() } : voltages;
        }

        /// <summary>
        /// Sets Initial Mask bits using bit set OR operation for InitialMaskBits and DieRecovery_.
        /// </summary>
        [ExcludeFromCodeCoverage]
        protected internal void SetInitialMask()
        {
            var result = new BitArray(this.IncomingMask_);
            if (!string.IsNullOrEmpty(this.MultiPassMasks))
            {
                result = result.Or(this.CurrentMultiPassMask);
            }

            this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Name} --Initial Search Mask Bits:{result.ToBinaryString()}");
            this.InitialSearchMask_ = result;
        }

        /// <summary>
        /// Sets incoming mask from parameter or DieRecovery tracking.
        /// </summary>
        [ExcludeFromCodeCoverage]
        protected internal void SetIncomingMask()
        {
            this.IncomingMask_ = !string.IsNullOrEmpty(this.InitialMaskBits) ? this.InitialMaskBits.ToString().ToBitArray() : new BitArray(this.voltageTargets.Count, false);
            this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Name} --Incoming Mask Bits:{this.IncomingMask_.ToBinaryString()}");
        }

        /// <summary>
        /// Verification of SingleVmin Mode parameters.
        /// </summary>
        /// <exception cref="ArgumentException">Invalid configuration.</exception>
        [ExcludeFromCodeCoverage]
        protected internal void VerifySingleVminMode()
        {
            if (this.TestMode != TestModes.SingleVmin)
            {
                return;
            }

            if (this.voltageTargets.Count != 1)
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: supports a single {nameof(this.VoltageTargets)}");
            }

            if (!string.IsNullOrEmpty(this.MultiPassMasks))
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: does not support {nameof(this.MultiPassMasks)}");
            }
        }

        /// <summary>
        /// /// Verification of MultiVmin Mode parameters.
        /// </summary>
        /// <exception cref="ArgumentException">Invalid configuration.</exception>
        [ExcludeFromCodeCoverage]
        protected internal void VerifyMultiVminMode()
        {
            if (this.TestMode != TestModes.MultiVmin)
            {
                return;
            }

            var numberOfTargets = this.voltageTargets.Count;
            var numberOfCorners = this.CornerIdentifiers.ToList().Count;
            if (numberOfTargets < 2)
            {
                throw new ArgumentException(
                    $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: only supports multiple {nameof(this.VoltageTargets)}.");
            }

            if (!string.IsNullOrEmpty(this.CornerIdentifiers) && numberOfTargets != numberOfCorners)
            {
                throw new ArgumentException(
                    $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: number of {nameof(this.VoltageTargets)} must match number of {nameof(this.CornerIdentifiers)}.");
            }
        }

        /// <summary>
        /// Initializes voltages targets and initial mask using defaults.
        /// </summary>
        protected virtual void InitializeVoltageTargets()
        {
            this.voltageTargets = this.VoltageTargets.ToList();
            this.InitialSearchMask_ = new BitArray(this.voltageTargets.Count, false);
        }

        /// <summary>
        /// Evaluate comma-separated FeatureSwitchSettings.
        /// </summary>
        [ExcludeFromCodeCoverage]
        protected virtual void VerifyFeatureSwitchSettings()
        {
            if (string.IsNullOrEmpty(this.FeatureSwitchSettings))
            {
                return;
            }

            var featureList = this.FeatureSwitchSettings.ToList();
            this.IsStartOnFirstFail_ = !featureList.Contains("start_on_first_fail_off");
            this.ReturnOnGlobalStickyError_ = !featureList.Contains("return_on_global_sticky_error_off");
            this.IgnoreMaskedResults_ = featureList.Contains("ignore_masked_results");
            this.PrintPerTargetIncrements_ = featureList.Contains("print_per_target_increments");

            if (this.LogLevel != PrimeLogLevel.PRIME_DEBUG)
            {
                return;
            }

            var message = "--FeatureSwitchSettings start_on_first_fail:";
            message += this.IsStartOnFirstFail_ ? "ON" : "OFF";
            message = "--FeatureSwitchSettings return_on_global_sticky_error:";
            message += this.ReturnOnGlobalStickyError_ ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings ignore_masked_results:";
            message += this.IgnoreMaskedResults_ ? "ON" : "OFF";
            message += "\n--FeatureSwitchSettings print_per_target_increments:";
            message += this.PrintPerTargetIncrements_ ? "ON" : "OFF";
            this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Name}:\n" + message);
        }

        /// <summary>
        /// Sets VminForwarding_ reference.
        /// </summary>
        /// <exception cref="ArgumentException">CornerIdentifiers is required.</exception>
        [ExcludeFromCodeCoverage]
        protected virtual void VerifyForwarding()
        {
            this.VminForwarding_ = null;
            this.flows = new List<int>();

            if (this.LogLevel == PrimeLogLevel.PRIME_DEBUG)
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Name}:\n --ForwardingMode:{this.ForwardingMode}");
            }

            if (this.ForwardingMode == ForwardingModes.None)
            {
                return;
            }

            if (this.LogLevel == PrimeLogLevel.PRIME_DEBUG)
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Name}: --CornerIdentifiers:{this.CornerIdentifiers}");
            }

            if (string.IsNullOrEmpty(this.CornerIdentifiers))
            {
                return;
            }

            var flowParameter = Prime.Services.TestProgramService.GetCurrentTestInstanceParameter("FlowIndex").Split(',');
            this.flows = flowParameter.Select(o => o.ToInt()).ToList();
            var corners = this.CornerIdentifiers.ToList();
            if (this.flows.Count == 1 && this.flows.Count != corners.Count)
            {
                for (var i = 1; i < corners.Count; i++)
                {
                    this.flows.Add(this.flows[0]);
                }
            }

            if (this.flows.Count != corners.Count)
            {
                throw new ArgumentException($"ForwardingMode requires single FlowIndex or a number matching the number of {nameof(this.CornerIdentifiers)}.");
            }

            this.VminForwarding_ = new List<Tuple<string, int, IVminForwardingCorner>>();
            for (var i = 0; i < corners.Count; i++)
            {
                this.VminForwarding_.Add(new Tuple<string, int, IVminForwardingCorner>(corners[i], this.flows[i], VminForwarding.Service.Get(corners[i], this.flows[i])));
            }
        }

        /// <summary>
        /// Verifies DTS configuration.
        /// </summary>
        protected virtual void VerifyDts()
        {
            this.DTSHandler_.SetConfiguration(this.DtsConfiguration);
        }

        /// <summary>
        /// Post instance routine.
        /// </summary>
        [ExcludeFromCodeCoverage]
        protected virtual void ExecutePostInstance()
        {
            if (this.postPlist != null)
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod().Name}: executing {nameof(this.PostPlist)}={this.PostPlist}.");
                this.postPlist.Execute();
            }
        }

        /// <summary>
        /// Updates Vmin Forwarding table using results and mode.
        /// </summary>
        /// <param name="voltageResults">accumulated voltage results.</param>
        [ExcludeFromCodeCoverage]
        protected virtual void UpdateVminForwarding(List<double> voltageResults)
        {
            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Started ---- ");

            // If search fails but rules pass: Skip VminForwarding, update DieRecovery. Exit Port 3.
            if (this.TestMode == TestModes.Functional || this.TestMode == TestModes.Scoreboard || string.IsNullOrEmpty(this.CornerIdentifiers) || (this.port == 3))
            {
                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Stopped ---- ");
                return;
            }

            switch (this.ForwardingMode)
            {
                case ForwardingModes.Output:
                    for (var i = 0; i < voltageResults.Count; i++)
                    {
                        this.VminForwarding_[i].Item3.StoreVminResult(voltageResults[i]);
                    }

                    break;
                case ForwardingModes.InputOutput:
                    for (var i = 0; i < voltageResults.Count; i++)
                    {
                        var vminForwardingVoltage = this.VminForwarding_[i].Item3.GetStartingVoltage(-9999);
                        this.VminForwarding_[i].Item3.StoreVminResult(voltageResults[i].Equals(-9999, 3) ? voltageResults[i] : Math.Max(voltageResults[i], vminForwardingVoltage));
                    }

                    break;
                case ForwardingModes.Input:
                case ForwardingModes.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Stopped ---- ");
        }

        private void VerifyPostPlist()
        {
            this.postPlist = !string.IsNullOrEmpty(this.PostPlist) ? Prime.Services.FunctionalService.CreateNoCaptureTest(this.PostPlist, this.LevelsTc, this.TimingsTc, string.Empty) : null;
        }

        private void VerifySinglePointMode()
        {
            if ((this.TestMode == TestModes.Functional || this.TestMode == TestModes.Scoreboard) &&
                (string.IsNullOrEmpty(this.CornerIdentifiers) || this.ForwardingMode == ForwardingModes.None ||
                 this.ForwardingMode == ForwardingModes.Output))
            {
                this.SkipApplySearchVoltage_ = true;
            }
            else
            {
                this.SkipApplySearchVoltage_ = false;
            }

            this.logger = null;
            if (((IVminSearchExtensions)this).IsSinglePointMode() && this.TestMode == TestModes.Scoreboard)
            {
                // Prime.Services.ScoreBoardService.CreateLogger(this.ScoreboardBaseNumberMbist, this.PatternNameMap, this.ScoreboardMaxFails);
                this.logger = Prime.Services.ScoreBoardService.CreateLogger(this.ScoreboardBaseNumberMbist, this.PatternNameMap, this.ScoreboardMaxFails);
            }
        }

        private void ConcurrentDebugPrint(ConcurrentDictionary<string, ConcurrentDictionary<string, Hry.ResultNameChar>> concurrentfailures)
        {
            this.Console?.PrintDebug($"\n\n[{MethodBase.GetCurrentMethod().Name}]--------------------------------------------------------");
            foreach (var fail in concurrentfailures)
            {
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]\tPattern Fail: {fail.Key}");
                foreach (var cont in fail.Value)
                {
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]\t\t\tController: {cont.Key}:   Failure: {cont.Value}");
                }
            }

            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]--------------------------------------------------------\n\n");
        }

        private void LogSinglePointModeScoreboard()
        {
            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Started ---- ");
            if (!((IVminSearchExtensions)this).IsSinglePointMode() || this.logger == null)
            {
                return;
            }

            var failingPatterns = new HashSet<string>();
            foreach (var pin in this.CtvPins.ToList())
            {
                foreach (var pat in this.diedataset[pin].Hryclass.HryLookupTable.Plists[this.modifiedpatlist])
                {
                    if (!this.diedataset[pin].Hryclass.HryLookupTable.Bisrpats.Contains(pat))
                    {
                        if (this.diedataset[pin].Hryclass.ScoreboardFails.ContainsKey(pat))
                        {
                            failingPatterns.Add(pat);
                        }
                    }
                }
            }

            this.logger.ProcessFailData(failingPatterns.ToList());
            this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}] <- Stopped ---- ");
        }

        [ExcludeFromCodeCoverage]
        private List<double> ExpandVoltageKeys(List<string> voltageKeys)
        {
            if (voltageKeys.First().StartsWith("D."))
            {
                var key = voltageKeys.First().Substring(2);
                var value = Prime.Services.DffService.GetDff(key);
                voltageKeys = value.ToLower().Split('v').ToList();
            }

            if (voltageKeys.Count.Equals(1) && this.voltageTargets.Count > 1)
            {
                voltageKeys = new List<string>(Enumerable.Repeat(voltageKeys.ElementAt(0), this.voltageTargets.Count));
            }

            return voltageKeys.Select(o => o.ToDouble(true)).ToList();
        }

        /// <summary>
        /// Creates and stores, or retrieves, the Master MbistLookTable containing all plists. If the shared storage
        /// value does not exist the config file gets parsed into an MbistLookupTable object and store in shared storage
        /// as a reference for other test instances/die. If it does exist, the master MbistlookupTable is referenced as
        /// a lookup to find the respective selected plist's table for the test instance.
        /// </summary>
        private void VerifyCreateLookupTable()
        {
            if (this.Patlist.ToString().Contains("::"))
            {
                var temp = this.Patlist.ToString().Split(':');
                this.modifiedpatlist = temp[temp.Length - 1];
            }
            else
            {
                this.modifiedpatlist = this.Patlist;
            }

            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Beginning Method.");
            this.map = new Mapping();
            if (this.MappingConfig != string.Empty)
            {
                this.map.LoadMappingConfig(this.ForceConfigFileParseState, this.MappingConfig);
            }

            this.diedataset = new ConcurrentDictionary<string, DieData>();

            if (this.map.Map.DieToPin.Count > 0)
            {
                foreach (var pin in this.map.Map.DieToPin)
                {
                    var tempituff = this.map.Hryname(this.MbistTestMode, MappingJsonParser.TPTypes.ITUFF);
                    if (tempituff.Length > 0)
                    {
                        List<string> tempname = new List<string>() { pin.Value.Shortname, tempituff, this.ItuffNameExtenstion };
                        tempituff = string.Join("_", tempname.Where(s => !string.IsNullOrEmpty(s)));
                    }
                    else
                    {
                        tempituff = "HRY_RAWSTR_MBIST";
                    }

                    var tempss = this.map.Hryname(this.MbistTestMode, MappingJsonParser.TPTypes.SharedStorage);
                    if (tempss.Length > 0)
                    {
                        List<string> tempname = new List<string>() { pin.Value.Shortname, tempituff, this.ItuffNameExtenstion };
                        tempss = string.Join("_", tempname.Where(s => !string.IsNullOrEmpty(s)));
                    }
                    else
                    {
                        tempss = "HRY_RAWSTR_MBIST";
                    }

                    this.diedataset[pin.Key] = new DieData(
                                            this.LogLevel,
                                            this.modifiedpatlist,
                                            this.TestMode,
                                            this.DffOperation,
                                            this.BisrMode,
                                            this.MbistTestMode,
                                            this.AdvancedDebug,
                                            this.RecoveryModeDownbin,
                                            this.voltageTargets.ToList(),
                                            this.PrintToItuff);

                    this.diedataset[pin.Key].Inithry(
                        tempituff,
                        tempss,
                        this.Threads,
                        this.ForceConfigFileParseState,
                        this.ScoreboardBaseNumberMbist,
                        this.LookupTableConfigurationFile);

                    tempituff = this.map.Getname(MappingJsonParser.Fields.Vmin, MappingJsonParser.TPTypes.ITUFF);
                    if (tempituff.Length > 0)
                    {
                        List<string> tempname = new List<string>() { pin.Value.Shortname, tempituff, this.ItuffNameExtenstion };
                        tempituff = string.Join("_", tempname.Where(s => !string.IsNullOrEmpty(s)));
                    }

                    tempss = this.map.Getname(MappingJsonParser.Fields.Vmin, MappingJsonParser.TPTypes.SharedStorage);
                    if (tempss.Length > 0)
                    {
                        List<string> tempname = new List<string>() { pin.Value.Shortname, tempss, this.ItuffNameExtenstion };
                        tempss = string.Join("_", tempname.Where(s => !string.IsNullOrEmpty(s)));
                    }

                    this.diedataset[pin.Key].Initvmin(this.voltageTargets.ToList(), tempituff, tempss, pin.Value.Voltages);

                    if (this.RecoveryConfigurationFile != string.Empty)
                    {
                        var tempdff = this.map.Getname(MappingJsonParser.Fields.Recovery, MappingJsonParser.TPTypes.DFF);
                        if (tempdff.Length > 0)
                        {
                            List<string> tempname = new List<string>() { pin.Value.Shortname, tempdff, this.ItuffNameExtenstion };
                            tempdff = string.Join("_", tempname.Where(s => !string.IsNullOrEmpty(s)));
                        }

                        tempss = this.map.Getname(MappingJsonParser.Fields.Recovery, MappingJsonParser.TPTypes.SharedStorage);
                        if (tempss.Length > 0)
                        {
                            List<string> tempname = new List<string>() { pin.Value.Shortname, tempss, this.ItuffNameExtenstion };
                            tempss = string.Join("_", tempname.Where(s => !string.IsNullOrEmpty(s)));
                        }

                        this.diedataset[pin.Key].Initrecovery(
                                                tempdff,
                                                tempss,
                                                this.ForceConfigFileParseState,
                                                this.RecoveryConfigurationFile);
                    }

                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Parsed HRY Parsing of {this.LookupTableConfigurationFile} for die {pin.Value}");

                    this.diedataset[pin.Key].Initbisr(this.map, pin.Value.Shortname);

                    if (this.VFDMconfig != string.Empty)
                    {
                        this.diedataset[pin.Key].Initvirtualfuse(this.ForceConfigFileParseState, this.VFDMconfig);
                        this.diedataset[pin.Key].Virtualfuseclass.CreateVirtualfuse();
                    }

                    this.diedataset[pin.Key].VoltageLevelExecutionResultsList = new List<VoltageLevelExecutionResult>();

                    this.CheckClearOptions(this.ClearVariables.ToList());
                }
            }
        }

        private void CheckClearOptions(List<string> options)
        {
            List<string> allowedOptions = new List<string>() { "hry", "bisr", "vmin", "vfdm", "recovery", "all" };
            var incorrectopt = options.Except(allowedOptions);
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Allowed options are: {string.Join(",", allowedOptions)}");
            if (incorrectopt.Count() > 0)
            {
                Services.ConsoleService.PrintError($"[{MethodBase.GetCurrentMethod().Name}] Incorrect options given: {string.Join(",", allowedOptions)}");
            }
        }

        private void WriteLimitingPatternsToItuff(List<string> limitingPatterns, string patternNameMap)
        {
            var targetValueSeparator = "_";
            const string noLimitingPatternToken = "na";
            var printLimitingPatterns = false;
            var limitingPatternsString = Enumerable.Repeat(noLimitingPatternToken, limitingPatterns.Count).ToArray();
            for (var targetIndex = 0; targetIndex < limitingPatterns.Count; targetIndex++)
            {
                if (limitingPatterns[targetIndex] != noLimitingPatternToken)
                {
                    printLimitingPatterns = true;
                    limitingPatternsString[targetIndex] = ExtendedDataLogger.GetMappedString(limitingPatterns[targetIndex], patternNameMap);
                }
            }

            if (printLimitingPatterns)
            {
                var outputLimitingPatterns = string.Join(targetValueSeparator, limitingPatternsString);
                var strgvalWriter = Services.DatalogService.GetItuffStrgvalWriter();
                var tnamePostfix = targetValueSeparator + "lp";
                strgvalWriter.SetTnamePostfix(tnamePostfix);
                strgvalWriter.SetData(outputLimitingPatterns);
                Services.DatalogService.WriteToItuff(strgvalWriter);
            }
        }

        [ExcludeFromCodeCoverage]
        private void WritePerTargetIncrementsToItuff(List<uint> perTargetIncrements)
        {
            if (!this.PrintPerTargetIncrements_)
            {
                return;
            }

            var targetValueSeparator = "_";
            var outPerTargetIncrements = string.Join(targetValueSeparator, perTargetIncrements);
            var strgvalWriter = Services.DatalogService.GetItuffStrgvalWriter();
            var tnamePostfix = targetValueSeparator + "it";
            strgvalWriter.SetTnamePostfix(tnamePostfix);
            strgvalWriter.SetData(outPerTargetIncrements);
            Services.DatalogService.WriteToItuff(strgvalWriter);
        }

        /// <summary>State tracking for current run.</summary>
        public class CurrentRunStateTracking
        {
            /// <summary> Initializes a new instance of the <see cref="CurrentRunStateTracking"/> class.</summary>
            /// <param name = "voltagenames" > current value set.</param>
            /// <param name="console">Prime.Services.ConsoleService or null depending on the current instances LogLevel.</param>
            public CurrentRunStateTracking(List<string> voltagenames, IConsoleService console)
            {
                this.Console = console;
                this.VoltagePass = new bool[voltagenames.Count];
                var i = 0;
                foreach (var voltname in voltagenames)
                {
                    this.VoltagePass[i] = true;
                    i++;
                }

                this.BisrFailComp = new List<string>();
            }

            /// <summary>Gets or sets a value indicating whether skip recovery flow.</summary>
            public bool Skiprecovery { get; set; } = false;

            /// <summary> Gets or sets a value indicating whether if recovery ips pass.</summary>
            public bool Recoveryipspass { get; set; } = true;

            /// <summary> Gets or sets a value indicating whether if nonrecovery ips pass.</summary>
            public bool Nonrecoveryipspass { get; set; } = true;

            /// <summary> Gets or sets a value indicating whether if repairable ips pass.</summary>
            public bool Repairablefound { get; set; } = false;

            /// <summary> Gets or sets a value indicating whether global failures are found.</summary>
            public bool GlobalFailures { get; set; } = false;

            /// <summary> Gets or sets voltage per power pass results.</summary>
            public bool[] VoltagePass { get; set; }

            /// <summary> Gets or sets list of unrepairable memories.</summary>
            public List<int> Unrepairablemems { get; set; } = new List<int>();

            /// <summary> Gets or sets list of Bisr Compressions that fail.</summary>
            public List<string> BisrFailComp { get; set; }

            /// <summary>
            /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
            /// </summary>
            protected IConsoleService Console { get; set; }

            /// <summary> Prints the data stored in this class.</summary>
            /// <param name="diename">Die name.</param>
            [ExcludeFromCodeCoverage]
            public void PrintRunstate(string diename)
            {
                this.Console?.PrintDebug($"\n\n[{MethodBase.GetCurrentMethod().Name}]--------------------------{diename}------------------------------");
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] SkipRecovery: {this.Skiprecovery}");
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Recoveryipspass: {this.Recoveryipspass}");
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Nonrecoveryipspass: {this.Nonrecoveryipspass}");
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Repairablefound: {this.Repairablefound}");
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] GlobalFailures: {this.GlobalFailures}");
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] VoltagePass: {string.Join(", ", this.VoltagePass)}");

                if (this.Unrepairablemems.Count > 0)
                {
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] UnrepairableMemories: {string.Join(", ", this.Unrepairablemems)}");
                }

                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]--------------------------------------------------------\n\n");
            }
        }

        /// <summary> Per die class for multi die at class. </summary>
        public class DieData
        {
            /// <summary> Initializes a new instance of the <see cref="DieData"/> class.</summary>
            /// <param name="loglevel">Log level of template.</param>
            /// <param name="patlist">Patlist executed.</param>
            /// <param name="testmodes">Test mode of template.</param>
            /// <param name="dffoperation">Dff Operation.</param>
            /// <param name="bisrmodes">BISR mode.</param>
            /// <param name="mbisttestmode">MBIST Modes.</param>
            /// <param name="advanceddebug">Advanced debug.</param>
            /// <param name="recoverydownbin">RecoveryDownBin allowed.</param>
            /// <param name="voltageTargets">Voltage targets.</param>
            /// <param name="printtoituff">Print to ituff enabled.</param>
            public DieData(MbistVminTC.PrimeLogLevel loglevel, string patlist, MbistVminTC.TestModes testmodes, MbistVminTC.DFFOperation dffoperation, MbistVminTC.BisrModes bisrmodes, MbistVminTC.MbistTestModes mbisttestmode, MbistVminTC.EnableStates advanceddebug, MbistVminTC.EnableStates recoverydownbin, List<string> voltageTargets, MbistVminTC.ItuffPrint printtoituff)
            {
                this.Loglevel = loglevel;
                this.Console = this.Loglevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;
                this.Patlist = patlist;
                this.TestMode = testmodes;
                this.Dffoperation = dffoperation;
                this.BisrMode = bisrmodes;
                this.AdvancedDebug = advanceddebug;
                this.MbistTestmode = mbisttestmode;
                this.RecoveryModeDownbin = recoverydownbin;
                this.VoltageTargets = voltageTargets;
                this.Printtoituff = printtoituff;
            }

            /// <summary> Gets or sets Die Name.</summary>
            public string Diename { get; set; }

            /// <summary> Gets or sets Mapping File.</summary>
            public Mapping Map { get; set; }

            /// <summary> Gets or sets Execution results.</summary>
            public List<VoltageLevelExecutionResult> VoltageLevelExecutionResultsList { get; set; }

            /// <summary> Gets or sets Log level.</summary>
            public MbistVminTC.PrimeLogLevel Loglevel { get; set; }

            /// <summary> Gets or sets Patlist executing.</summary>
            public string Patlist { get; set; }

            /// <summary> Gets or sets Hry class.</summary>
            public Hry Hryclass { get; set; }

            /// <summary> Gets or sets Vmin class.</summary>
            public Vmin Vminclass { get; set; }

            /// <summary> Gets or sets Recovery class.</summary>
            public RecoveryJsonParser RecoveryLookupTable { get; set; }

            /// <summary> Gets or sets Recovery class.</summary>
            public Recovery Recoveryclass { get; set; }

            /// <summary> Gets or sets Currentbisr class.</summary>
            public List<BisrChainResult> Currentbisr { get; set; }

            /// <summary> Gets or sets BisrCompression class.</summary>
            public Dictionary<string, BisrCompress> BisrCompressions { get; set; }

            /// <summary> Gets or sets Bisrpatmod handler.</summary>
            public Dictionary<string, IPatConfigHandle> BisrsetPointHandler { get; set; }

            /// <summary> Gets or sets Virtual fuse class.</summary>
            public Virtualfuse Virtualfuseclass { get; set; }

            /// <summary> Gets or sets GlobalHry.</summary>
            public List<char> Globalhrystring { get; set; }

            /// <summary> Gets or sets Recovery data string.</summary>
            public List<char> Recoverydata { get; set; }

            /// <summary> Gets or sets Voltage values.</summary>
            public List<double> Voltagevalues { get; set; }

            /// <summary> Gets or sets Voltage targets.</summary>
            public List<string> VoltageTargets { get; set; }

            /// <summary>
            /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
            /// </summary>
            protected IConsoleService Console { get; set; }

            /// <summary> Gets or sets Recovery Downbin.</summary>
            private MbistVminTC.EnableStates RecoveryModeDownbin { get; set; } = EnableStates.Disabled;

            /// <summary> Gets or sets MbistTestmodes.</summary>
            private MbistVminTC.EnableStates AdvancedDebug { get; set; }

            /// <summary> Gets or sets MbistTestmodes.</summary>
            private MbistVminTC.MbistTestModes MbistTestmode { get; set; }

            /// <summary> Gets or sets Testmodes.</summary>
            private MbistVminTC.TestModes TestMode { get; set; }

            /// <summary> Gets or sets DffOperation.</summary>
            private MbistVminTC.DFFOperation Dffoperation { get; set; }

            /// <summary> Gets or sets BisrModes.</summary>
            private MbistVminTC.BisrModes BisrMode { get; set; }

            /// <summary> Gets or sets Ituff mode.</summary>
            private MbistVminTC.ItuffPrint Printtoituff { get; set; } = ItuffPrint.Disabled;

            /// <summary> Takes HRY from current run and agragates it to global by prioriy. </summary>
            public void CheckHryStringPriority()
            {
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Original sharedStorage value : [{string.Join(string.Empty, this.Globalhrystring)}]");
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Current run locations : [{string.Join(string.Empty, this.Hryclass.CurrentHryString)}]");
                foreach (KeyValuePair<int, char> updatehry in this.Hryclass.CurrentHryString)
                {
                    switch (this.MbistTestmode)
                    {
                        case MbistTestModes.PostRepair:

                            // post repair update flow
                            if (this.Globalhrystring[updatehry.Key] == (char)Hry.ResultNameChar.Repairable)
                            {
                                if (updatehry.Value == (char)Hry.ResultNameChar.Fail)
                                {
                                    this.Globalhrystring[updatehry.Key] = (char)Hry.ResultNameChar.Fail_retest;
                                }
                                else if (updatehry.Value == (char)Hry.ResultNameChar.Pass)
                                {
                                    this.Globalhrystring[updatehry.Key] = (char)Hry.ResultNameChar.Pass_retest;
                                }
                                else
                                {
                                    this.Globalhrystring[updatehry.Key] = updatehry.Value;
                                }
                            }
                            else
                            {
                                if (this.Globalhrystring[updatehry.Key] == (char)Hry.ResultNameChar.Pass && updatehry.Value == (char)Hry.ResultNameChar.Fail)
                                {
                                    this.Globalhrystring[updatehry.Key] = (char)Hry.ResultNameChar.Inconsist_pst_fail;
                                }
                                else
                                {
                                    this.Globalhrystring[updatehry.Key] = this.Hryclass.ChoosePriorityResult(this.Globalhrystring[updatehry.Key], this.Hryclass.CurrentHryString[updatehry.Key]);
                                }
                            }

                            break;
                        default:
                            this.Globalhrystring[updatehry.Key] = this.Hryclass.ChoosePriorityResult(this.Globalhrystring[updatehry.Key], this.Hryclass.CurrentHryString[updatehry.Key]);
                            break;
                    }
                }

                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Updated sharedStorage value  : [{string.Join(string.Empty, this.Globalhrystring)}]");
            }

            /// <summary>
            /// Logic to choose the proper recovery option.
            /// </summary>
            /// <returns>The index to use for recovery.</returns>
            public int ChooseRecoveryOption()
            {
                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: <- Started ---- ");
                int recoveryip = 100;
                if (this.RecoveryLookupTable != null)
                {
                    recoveryip = this.VoltageLevelExecutionResultsList.Count() - 1;
                }
                else
                {
                    for (int idx = this.VoltageLevelExecutionResultsList.Count() - 1; idx >= 0; idx--)
                    {
                        switch (this.VoltageLevelExecutionResultsList[idx].FlowFlag)
                        {
                            case VoltageLevelExecutionResult.FlowFlags.FULL_PASS:
                                return idx;
                            case VoltageLevelExecutionResult.FlowFlags.ERROR:
                                Services.ConsoleService.PrintError($"{this.Diename}: There is an issue with something setting an error most likely incorrect recovery defintion");
                                break;
                            case VoltageLevelExecutionResult.FlowFlags.INITVAL:
                                Services.ConsoleService.PrintError($"{this.Diename}: Flow never set a valid FlowFlag for in list at {idx}");
                                break;
                            case VoltageLevelExecutionResult.FlowFlags.FAIL:
                                break;
                            case VoltageLevelExecutionResult.FlowFlags.RESET:
                                break;
                            default:
                                if (this.VoltageLevelExecutionResultsList[idx].Priority < recoveryip)
                                {
                                    recoveryip = idx;
                                }

                                break;
                        }
                    }

                    if (recoveryip == 100)
                    {
                        recoveryip = this.VoltageLevelExecutionResultsList.Count() - 1;
                    }
                }

                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: <- Stopped ---- ");
                return recoveryip;
            }

            /// <summary> Post MBIST functions. </summary>
            /// <returns>Int for the exit port. </returns>
            public int MbistPostInstance()
            {
                var idx = 0;
                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: <- Started ---- ");
                foreach (VoltageLevelExecutionResult entry in this.VoltageLevelExecutionResultsList)
                {
                    this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Stored Voltage Results: {idx}");
                    entry.ExecutionPrint(this.Diename);
                    idx += 1;
                }

                int recIndexchoice = this.ChooseRecoveryOption();

                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Chosen Result: {recIndexchoice}");
                this.VoltageLevelExecutionResultsList[recIndexchoice].ExecutionPrint(this.Diename);

                this.Hryclass.CurrentHryString = this.VoltageLevelExecutionResultsList[recIndexchoice].Hry;

                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Calling CheckHryStringPriority method.");
                this.CheckHryStringPriority();

                this.Hryclass.HryWriteSharedStorage(this.Globalhrystring);

                if (this.Vminclass.Pervminenabled() == true)
                {
                    this.Vminclass.VminWriteSharedStorage();
                }

                if (this.BisrMode != BisrModes.Off)
                {
                    // this.Console?.PrintDebug("[EXECUTE] Calling StoreGlobalValueInSharedStorage method.");
                    foreach (var bisrcont in this.VoltageLevelExecutionResultsList[recIndexchoice].Bisr)
                    {
                        if (this.Dffoperation == DFFOperation.Write_BISR_REC ||
                            this.Dffoperation == DFFOperation.Write_BISR)
                        {
                            bisrcont.WriteData(this.BisrMode, true);
                        }
                        else
                        {
                            bisrcont.WriteData(this.BisrMode);
                        }
                    }
                }

                // Write back Recovery data to shared storage.
                if (this.Recoveryclass != null)
                {
                    if (this.Dffoperation == DFFOperation.Write_BISR_REC || this.Dffoperation == DFFOperation.Write_REC)
                    {
                        this.Recoveryclass.WriteData(this.VoltageLevelExecutionResultsList[recIndexchoice].Recovery, true);
                    }
                    else
                    {
                        this.Recoveryclass.WriteData(this.VoltageLevelExecutionResultsList[recIndexchoice].Recovery);
                    }
                }

                // Write Virtual fuse data back
                if (this.Virtualfuseclass != null)
                {
                    this.Virtualfuseclass.WriteAllSharedStorage();
                }

                if (this.Printtoituff == ItuffPrint.Hry_VminPerDomain || this.Printtoituff == ItuffPrint.Hry || this.Printtoituff == ItuffPrint.Hry_VminPerMem)
                {
                    this.Hryclass.HryPrintDataToItuff(string.Join(string.Empty, this.Globalhrystring));
                }

                if (this.Printtoituff == ItuffPrint.Hry_VminPerDomain || this.Printtoituff == ItuffPrint.VminPerDomain)
                {
                    if (this.Vminclass.Pervminenabled() == true)
                    {
                        this.Vminclass.PrintDataToItuffPerDomain();
                    }
                    else
                    {
                        Services.ConsoleService.PrintError($"{this.Diename}: Can't print out PerDomain voltage data since you are running in Scoreboard or Functional modes.");
                    }
                }

                if (this.Printtoituff == ItuffPrint.Hry_VminPerMem || this.Printtoituff == ItuffPrint.VminPerMem)
                {
                    if (this.Vminclass.Pervminenabled() == true)
                    {
                        this.Vminclass.PrintDataToItuffPerArrayVmin();
                    }
                    else
                    {
                        Services.ConsoleService.PrintError($"{this.Diename}: Can't print out PerArrayVmin voltage data since you are running in Scoreboard or Functional modes.");
                    }
                }

                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: <- Stopped ---- ");
                return (int)this.VoltageLevelExecutionResultsList[recIndexchoice].FlowFlag;
            }

            /// <summary>Initializes HRY blocks.</summary>
            /// <param name="ituffname">Ituff name.</param>
            /// <param name="sharedstorage">sharedstorage name.</param>
            /// <param name="threads">Threads needed to parse data.</param>
            /// <param name="forceConfigFileParseState">Force the config filed to be pulled from file. </param>
            /// <param name="scoreboardBaseNumberMbist">Scoreboard # to check if scoreboarding needs to be enabled.</param>
            /// <param name="lookupTableConfigurationFile">File name of configuration file including lookup table.</param>
            public void Inithry(string ituffname, string sharedstorage, int threads, MbistVminTC.EnableStates forceConfigFileParseState, int scoreboardBaseNumberMbist, string lookupTableConfigurationFile)
            {
                this.Hryclass = new Hry();
                if (ituffname != string.Empty)
                {
                    this.Hryclass.HrystringNameSS = sharedstorage;
                }

                if (sharedstorage != string.Empty)
                {
                    this.Hryclass.HrystringNameITUFF = ituffname;
                }

                this.Hryclass.Threads = threads;
                this.Hryclass.AdvanceDebug = this.AdvancedDebug;
                this.Hryclass.Loglevel = this.Loglevel;
                this.Hryclass.Console = this.Console;
                this.Hryclass.HryfileParse(forceConfigFileParseState, lookupTableConfigurationFile, this.Patlist);
                if ((scoreboardBaseNumberMbist > 0 && this.TestMode == TestModes.Scoreboard) || this.TestMode == TestModes.MultiVmin || this.TestMode == TestModes.SingleVmin)
                {
                    this.Hryclass.ScoreboardEnabled = true;
                }
            }

            /// <summary>Initializes VMIN block.</summary>
            /// <param name="voltages">Voltage names.</param>
            /// <param name="tempituff">ItuffName.</param>
            /// <param name="tempss">SharedStorage name.</param>
            /// <param name="voltageupdate">Voltage name updates for that die.</param>
            public void Initvmin(List<string> voltages, string tempituff, string tempss, Dictionary<string, string> voltageupdate)
            {
                var newvoltageref = new List<string>(this.Hryclass.HryLookupTable.VoltageStringRef);
                foreach (var voltagelookup in voltageupdate)
                {
                    var indexupdate = new List<int>();
                    var idx = 0;
                    foreach (var voltagename in newvoltageref)
                    {
                        if (voltagename == voltagelookup.Key)
                        {
                            indexupdate.Add(idx);
                        }

                        idx++;
                    }

                    foreach (var index in indexupdate)
                    {
                        newvoltageref[index] = voltagelookup.Value;
                    }
                }

                this.Vminclass = new Vmin(
                    this.Hryclass.HryLookupTable.HryStringRef,
                    newvoltageref,
                    voltages);

                this.Hryclass.HryLookupTable.VoltageStringRef = newvoltageref;
                this.Vminclass.PerMemVoltageNameITUFF = tempituff;
                this.Vminclass.PerMemVoltageNameSS = tempss;
            }

            /// <summary>Initializes Recovery Class.</summary>
            /// <param name="tempdff">ItuffName.</param>
            /// <param name="tempss">SharedStorage name.</param>
            /// <param name="forceConfigFileParseState">Force loadconfig file.</param>
            /// <param name="recoveryConfigurationFile">Recovery Config file name.</param>
            public void Initrecovery(string tempdff, string tempss, MbistVminTC.EnableStates forceConfigFileParseState, string recoveryConfigurationFile)
            {
                this.Recoveryclass = new Recovery();
                this.RecoveryLookupTable = this.Recoveryclass.RecoveryfileParse(forceConfigFileParseState, recoveryConfigurationFile, this.Hryclass.HryLookupTable.HryStringRef);
                this.Recoveryclass.DFFRecoveryName = tempdff;
                this.Recoveryclass.SSRecoveryName = tempss;
            }

            /// <summary>Initializes Recovery Class.</summary>
            /// <param name="map">Mapping file.</param>
            /// <param name="die">Die name.</param>
            public void Initbisr(Mapping map, string die)
            {
                this.Diename = die;
                this.Currentbisr = new List<BisrChainResult>();
                this.BisrCompressions = new Dictionary<string, BisrCompress>();
                this.BisrsetPointHandler = new Dictionary<string, IPatConfigHandle>();

                // this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] {this.hry.HryLookupTable.Bisrexists}");
                if (this.Hryclass.HryLookupTable.Bisrdata.Count > 0)
                {
                    foreach (KeyValuePair<string, HryJsonParser.BisrClasses> bisr in this.Hryclass.HryLookupTable.Bisrdata)
                    {
                        this.BisrCompressions[bisr.Key] = new BisrCompress()
                        {
                            BufferSize = this.Hryclass.HryLookupTable.Bisrdata[bisr.Key].Buffer_size,
                            FuseboxAddress = this.Hryclass.HryLookupTable.Bisrdata[bisr.Key].FuseboxAddress,
                            FuseboxSize = this.Hryclass.HryLookupTable.Bisrdata[bisr.Key].FuseBoxSize,
                            Chains = this.Hryclass.HryLookupTable.Bisrdata[bisr.Key].Chains,
                            Totallength = this.Hryclass.HryLookupTable.Bisrdata[bisr.Key].Totallength,
                            ZeroSize = this.Hryclass.HryLookupTable.Bisrdata[bisr.Key].ZeroCountBits,
                            MaxSessions = this.Hryclass.HryLookupTable.Bisrdata[bisr.Key].MaxSessions,
                            Console = this.Console,
                        };

                        if (this.BisrMode == BisrModes.Bisr ||
                            this.BisrMode == BisrModes.Compressed)
                        {
                            this.BisrsetPointHandler.Add(bisr.Key, Prime.Services.PatConfigService.GetPatConfigHandle(bisr.Key));
                        }

                        var ituffname = map.Bisrname(bisr.Key, MappingJsonParser.TPTypes.ITUFF);
                        if (ituffname == string.Empty)
                        {
                            ituffname = $"{die}_{bisr.Key}";
                        }

                        var dffname = map.Bisrname(bisr.Key, MappingJsonParser.TPTypes.DFF);
                        if (dffname == string.Empty)
                        {
                            dffname = $"{die}{bisr.Key}";
                        }

                        var sharedStoragename = map.Bisrname(bisr.Key, MappingJsonParser.TPTypes.SharedStorage);
                        if (sharedStoragename == string.Empty)
                        {
                            sharedStoragename = $"{die}_{bisr.Key}";
                        }

                        var bisrresult = new BisrChainResult(bisr.Key, string.Empty, false, string.Empty, string.Empty, new List<string>(), ituffname, dffname, sharedStoragename, this.Console);
                        this.Currentbisr.Add(bisrresult);
                    }
                }
            }

            /// <summary>Initializes Recovery Class.</summary>
            /// <param name="forceConfigFileParseState">Force config load.</param>
            /// <param name="vfdmconfig">Force VFDMconfig.</param>
            public void Initvirtualfuse(MbistVminTC.EnableStates forceConfigFileParseState, string vfdmconfig)
            {
                this.Virtualfuseclass = new Virtualfuse();
                this.Virtualfuseclass.LoadVFDMlookup(forceConfigFileParseState, vfdmconfig, this.Hryclass.HryLookupTable.HryStringRef);
            }

            /// <summary> Clears all varables this would be done and is required on first instance to clear all variables. </summary>
            /// /// <param name="options"> What fields to clear.</param>
            public void ClearVariable(List<string> options)
            {
                // Initialize HRY string
                if (options.Contains("all") || options.Contains("hry"))
                {
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Initializing Local HRY string storage {this.Diename}.");
                    this.Globalhrystring = new List<char>();
                    this.Globalhrystring = this.Hryclass.HryLookupTable.HryStringRef.Select(u => 'U').ToList<char>();
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Setting {this.Diename} this.globalHrystring: [{string.Join(string.Empty, this.Globalhrystring)}]");
                }

                if (options.Contains("all") || options.Contains("vmin"))
                {
                    if (this.Vminclass.Pervminenabled() == true)
                    {
                        // Initialize VMIN HRY STRING
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Initializing {this.Diename} Local Voltage HRY storage .");
                        this.Vminclass.ClearVoltageString(this.Hryclass.HryLookupTable.HryStringRef);
                        this.Vminclass.VminWriteSharedStorage();
                    }
                }

                if (options.Contains("all") || options.Contains("recovery"))
                {
                    if (this.Recoveryclass != null)
                    {
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Initializing {this.Diename} Recovery String.");
                        this.Recoverydata = this.RecoveryLookupTable.InitializeRecovery();
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Clearing {this.Diename} Recovery String in shared storage.\n[{string.Join(string.Empty, this.Recoverydata)}]");
                    }
                }

                if (this.BisrMode != BisrModes.Off)
                {
                    if (options.Contains("all") || options.Contains("bisr"))
                    {
                        this.Console?.PrintDebug(
                            $"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: BISR COUNT. {this.Currentbisr.Count}");
                        if (this.Currentbisr.Count > 0)
                        {
                            this.Console?.PrintDebug(
                                $"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Initializing BISR.");
                            foreach (BisrChainResult bisr in this.Currentbisr)
                            {
                                bisr.InitializeBisrString(
                                    this.BisrMode,
                                    this.Hryclass.HryLookupTable.Bisrdata[bisr.BisrControllerName].Totallength,
                                    this.Hryclass.HryLookupTable.Bisrdata[bisr.BisrControllerName].FuseBoxSize);
                            }
                        }
                    }
                }

                if (options.Contains("all") || options.Contains("vfdm"))
                {
                    if (this.Virtualfuseclass != null)
                    {
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Initializing VFDM.");
                        this.Virtualfuseclass.CreateVirtualfuse();
                    }
                }
            }

            /// <summary> Runs all new flows after HRY data is collected. Bisr/Recovery/PerArrayVMIN. </summary>
            /// <param name="ctvResults"> This contains the HRY string.</param>
            /// <param name="currentrun"> If set later skips recovery.</param>
            /// <param name="pin"> Pin name to go thru.</param>
            public void BisrRecoveryFlows(Dictionary<string, string> ctvResults, ref Dictionary<string, CurrentRunStateTracking> currentrun, string pin)
            {
                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: <- Started ---- ");
                if (this.Loglevel == PrimeLogLevel.PRIME_DEBUG)
                {
                    currentrun[pin].PrintRunstate(this.Diename);
                }

                VoltageLevelExecutionResult currentresults;
                List<char> trecovery;
                if (this.Recoveryclass == null)
                {
                    trecovery = new List<char>();
                }
                else
                {
                    trecovery = new List<char>(this.Recoverydata);
                }

                currentresults = new VoltageLevelExecutionResult(new List<BisrChainResult>(this.Currentbisr), this.Hryclass.CurrentHryString, trecovery, this.Voltagevalues, this.Hryclass.ScoreboardFails, this.Console);

                this.HryResultFlowControl(ref currentrun, pin);

                if (currentrun[pin].GlobalFailures == true)
                {
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename} Global Failure detected.");
                    this.Setflowflag(ref currentresults, VoltageLevelExecutionResult.FlowFlags.FAIL);
                    if (this.ChecktoAddRecovery(currentresults) == true)
                    {
                        for (var i = 0; i < currentrun[pin].VoltagePass.Length; i++)
                        {
                            currentresults.Voltagestested[i] = currentrun[pin].VoltagePass[i];
                        }

                        this.VoltageLevelExecutionResultsList.Add(currentresults);
                    }
                }
                else
                {
                    if (this.Hryclass.HryLookupTable.Bisrexists == true && currentrun[pin].Repairablefound == true)
                    {
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}:  BISR Exist.");
                        foreach (string pattern in this.Hryclass.HryLookupTable.Bisrpats)
                        {
                            foreach (KeyValuePair<string, HryJsonParser.PatternBlocks.ControllersBlock> bisrCtrlr in this.Hryclass.HryLookupTable.Patterns[pattern].Controllers)
                            {
                                List<string> bisrcomments = bisrCtrlr.Value.CaptureComments.Split(',').ToList<string>();
                                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}:  Attempting to run compression on controller {bisrCtrlr.Key.ToString()}");

                                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}:  Capttured BISR CTVS for {pattern} = [{ctvResults[pattern]}].");
                                var tempcompresult = this.BisrCompressions[bisrCtrlr.Key].CompressChains(ctvResults[pattern], string.Empty);

                                var idx = this.Currentbisr.FindIndex(item => item.BisrControllerName == bisrCtrlr.Key);
                                currentresults.Bisr[idx] = new BisrChainResult(bisrCtrlr.Key, this.Currentbisr[idx].RawData, tempcompresult["Compress"], tempcompresult["FuseToApply"], tempcompresult["FuseAfterBurn"], bisrcomments, this.Currentbisr[idx].ITuffName, this.Currentbisr[idx].DffName, this.Currentbisr[idx].BisrControllerSS, this.Console, tempcompresult["AvailableFuse"]);
                                currentresults.Bisr[idx].AggregateBisrChainData(ctvResults[pattern].ToList(), this.Hryclass.HryLookupTable.Indexperplist[this.Patlist], this.Hryclass.HryLookupTable.HryStringRef);

                                if (currentresults.Bisr[idx].CompressionStatus == false)
                                {
                                    currentrun[pin].BisrFailComp.Add(bisrCtrlr.Key);
                                }

                                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}:  Successfully attempted compression on controller {bisrCtrlr.Key}");
                            }
                        }

                        if (currentrun[pin].Unrepairablemems.Count == 0 && currentrun[pin].Recoveryipspass == true && currentrun[pin].Nonrecoveryipspass == true)
                        {
                            currentrun[pin].Skiprecovery = true;
                            this.Setflowflag(ref currentresults, VoltageLevelExecutionResult.FlowFlags.REPAIRABLE);
                            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Skipping recovery.");
                            if (this.ChecktoAddRecovery(currentresults) == true)
                            {
                                this.VoltageLevelExecutionResultsList.Add(currentresults);
                            }
                        }
                    }
                    else if (currentrun[pin].Repairablefound == true && this.Hryclass.HryLookupTable.Bisrexists == false)
                    {
                        if (currentrun[pin].Unrepairablemems.Count == 0 && currentrun[pin].Recoveryipspass == true && currentrun[pin].Nonrecoveryipspass == true)
                        {
                            currentrun[pin].Skiprecovery = true;
                            this.Setflowflag(ref currentresults, VoltageLevelExecutionResult.FlowFlags.REPAIRABLE);
                            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Skipping recovery VFDM repair found.");
                            if (this.ChecktoAddRecovery(currentresults) == true)
                            {
                                this.VoltageLevelExecutionResultsList.Add(currentresults);
                            }
                        }
                    }

                    if (currentrun[pin].Skiprecovery == false && this.RecoveryLookupTable != null)
                    {
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Running Recovery");
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Check if recovery is needed.");
                        if (currentrun[pin].Nonrecoveryipspass == true && currentrun[pin].Recoveryipspass == false)
                        {
                            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Running Recovery.");
                            var recoveryresult = this.Recoveryclass.ParseResults(this.Hryclass.CurrentHryString, this.RecoveryLookupTable, this.Recoverydata, this.RecoveryModeDownbin);
                            currentresults.Recovery = recoveryresult.RecoveryString;
                            currentresults.Priority = recoveryresult.Priority;

                            if (recoveryresult.ValidRecovery == true)
                            {
                                if (this.ChecktoAddRecovery(currentresults) == true)
                                {
                                    if (this.ClearUnrepairableFuses(ref currentresults, recoveryresult.Remove_Mems, currentrun[pin].BisrFailComp) == true)
                                    {
                                        if (currentrun[pin].Repairablefound == true)
                                        {
                                            this.Setflowflag(ref currentresults, VoltageLevelExecutionResult.FlowFlags.REPAIR_RECOVERY);
                                        }
                                        else
                                        {
                                            this.Setflowflag(ref currentresults, VoltageLevelExecutionResult.FlowFlags.RECOVERY);
                                        }

                                        this.VoltageLevelExecutionResultsList.Add(currentresults);
                                    }
                                }
                            }
                            else
                            {
                                if (this.ChecktoAddRecovery(currentresults) == true)
                                {
                                    this.Setflowflag(ref currentresults, VoltageLevelExecutionResult.FlowFlags.FAIL);

                                    for (var i = 0; i < currentrun[pin].VoltagePass.Length; i++)
                                    {
                                        currentresults.Voltagestested[i] = currentrun[pin].VoltagePass[i];
                                    }

                                    this.VoltageLevelExecutionResultsList.Add(currentresults);
                                }
                            }
                        }
                        else
                        {
                            if (this.ChecktoAddRecovery(currentresults) == true)
                            {
                                this.Setflowflag(ref currentresults, VoltageLevelExecutionResult.FlowFlags.FAIL);
                                this.VoltageLevelExecutionResultsList.Add(currentresults);
                            }
                        }
                    }

                    if (currentrun[pin].Nonrecoveryipspass == false)
                    {
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: NON RECOVERABLE IPS FAIL");
                        this.Setflowflag(ref currentresults, VoltageLevelExecutionResult.FlowFlags.FAIL);
                        this.VoltageLevelExecutionResultsList.Add(currentresults);
                        for (var i = 0; i < currentrun[pin].VoltagePass.Length; i++)
                        {
                            currentresults.Voltagestested[i] = currentrun[pin].VoltagePass[i];
                        }
                    }
                }

                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: <- Stopped ---- ");
            }

            /// <summary> When no failures detected, this method set all the plist hry values to 1's. </summary>
            /// <param name="setfullpass"> If set later skips recovery.</param>
            public void SetFullPassState(bool setfullpass)
            {
                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: <- Started ---- ");
                if (setfullpass == true)
                {
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Pass condition for HRY.");
                }
                else
                {
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Initialize running controllers to pass condition.");
                }

                foreach (int index in this.Hryclass.HryLookupTable.Indexperplist[this.Patlist])
                {
                    this.Hryclass.CurrentHryString.AddOrUpdate(index, (char)Hry.ResultNameChar.Pass, (key, existingVal) => this.Hryclass.ChoosePriorityResult(existingVal, Hry.ResultNameChar.Pass));
                    if (this.Vminclass.Pervminenabled() == true && setfullpass == true)
                    {
                        this.PerCharVoltageChkUpdate((char)Hry.ResultNameChar.Pass, index);
                    }
                }

                if (setfullpass == true)
                {
                    this.VoltageLevelExecutionResultsList.Add(new VoltageLevelExecutionResult(this.Currentbisr, this.Hryclass.CurrentHryString, this.Recoverydata, new List<double>(this.Voltagevalues), new ConcurrentDictionary<string, HashSet<int>>(), this.Console));
                    this.VoltageLevelExecutionResultsList[this.VoltageLevelExecutionResultsList.Count - 1].FlowFlag = VoltageLevelExecutionResult.FlowFlags.FULL_PASS;
                }
                else
                {
                    this.VoltageLevelExecutionResultsList.Add(new VoltageLevelExecutionResult(this.Currentbisr, this.Hryclass.CurrentHryString, this.Recoverydata, new List<double>(this.Voltagevalues), new ConcurrentDictionary<string, HashSet<int>>(), this.Console));
                    this.VoltageLevelExecutionResultsList[this.VoltageLevelExecutionResultsList.Count - 1].FlowFlag = VoltageLevelExecutionResult.FlowFlags.RESET;
                }

                this.Console?.PrintDebug($"\n[{MethodBase.GetCurrentMethod().Name}]{this.Diename} <- Stopped ---- ");
            }

            /// <summary> Gets required Information from HRY whether to run BISR/Recovery and sets VMINS. </summary>
            /// <param name="currentrun"> If set later skips recovery.</param>
            /// <param name="pin"> pin name running.</param>
            public void HryResultFlowControl(ref Dictionary<string, CurrentRunStateTracking> currentrun, string pin)
            {
                foreach (KeyValuePair<int, char> hrychar in this.Hryclass.CurrentHryString)
                {
                    if (hrychar.Value == (char)Hry.ResultNameChar.Fail ||
                        hrychar.Value == (char)Hry.ResultNameChar.Unrepairable ||
                        hrychar.Value == (char)Hry.ResultNameChar.Fail_retest)
                    {
                        if (this.RecoveryLookupTable != null)
                        {
                            if (this.RecoveryLookupTable.TotalForAllmodesbyIndex.Contains(hrychar.Key))
                            {
                                currentrun[pin].Recoveryipspass = false;
                                currentrun[pin].Unrepairablemems.Add(hrychar.Key);
                            }
                            else
                            {
                                currentrun[pin].Nonrecoveryipspass = false;
                                currentrun[pin].Skiprecovery = true;
                            }
                        }
                        else
                        {
                            currentrun[pin].Nonrecoveryipspass = false;
                        }
                    }
                    else if (hrychar.Value == (char)Hry.ResultNameChar.Repairable ||
                             hrychar.Value == (char)Hry.ResultNameChar.Pass_retest)
                    {
                        currentrun[pin].Repairablefound = true;
                    }

                    if (hrychar.Value == (char)Hry.ResultNameChar.Inconsist_fs_pg ||
                        hrychar.Value == (char)Hry.ResultNameChar.Cont_fail ||
                        hrychar.Value == (char)Hry.ResultNameChar.Inconsist_ps_fg ||
                        hrychar.Value == (char)Hry.ResultNameChar.Pattern_bad_wait ||
                        hrychar.Value == (char)Hry.ResultNameChar.Pattern_misprogram)
                    {
                        currentrun[pin].GlobalFailures = true;
                    }

                    if (this.Vminclass.Pervminenabled() == true && this.TestMode != TestModes.Functional && this.TestMode != TestModes.Scoreboard)
                    {
                        var result = this.PerCharVoltageChkUpdate(hrychar.Value, hrychar.Key);
                        if (result.Item2 == false)
                        {
                            currentrun[pin].VoltagePass[result.Item1] = false;
                        }
                    }
                }
            }

            /// <summary> Grabs current hry value and index to set voltages in perarrayvmin. </summary>
            /// <param name="hrychar"> If set later skips recovery.</param>
            /// <param name="index"> Index of HRY char.</param>
            /// <returns>Tuple of voltage index and where it failed.</returns>
            public Tuple<int, bool> PerCharVoltageChkUpdate(char hrychar, int index)
            {
                var pass = true;
                if (hrychar == (char)Hry.ResultNameChar.Pass ||
                    hrychar == (char)Hry.ResultNameChar.Repairable ||
                    hrychar == (char)Hry.ResultNameChar.Pass_retest)
                {
                    if (this.TestMode != TestModes.Functional && this.TestMode != TestModes.Scoreboard)
                    {
                        var tempvmin = this.Voltagevalues[
                            this.VoltageTargets.ToList().IndexOf(this.Hryclass.HryLookupTable.VoltageStringRef[index])];

                        if ((this.Vminclass.Grabvmin(index) < tempvmin && this.Vminclass.Grabvmin(index) > 0) || this.Vminclass.Grabvmin(index) == -5555 ||
                            this.Vminclass.Grabvmin(index) == -9999)
                        {
                            this.Vminclass.Setvmin(index, tempvmin);
                            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Voltages for Memory {this.Hryclass.HryLookupTable.HryStringRef[index]} was updated to {tempvmin}\n");
                        }
                    }
                }
                else if (hrychar != (char)Hry.ResultNameChar.Untested)
                {
                    if (this.TestMode != TestModes.Functional && this.TestMode != TestModes.Scoreboard)
                    {
                        if (this.Vminclass.Grabvmin(index) < 7777 && this.Vminclass.Grabvmin(index) > 0)
                        {
                            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: You may have low range speckles on rail: {this.Hryclass.HryLookupTable.VoltageStringRef[index]} on memory {this.Hryclass.HryLookupTable.HryStringRef[index]}");
                        }
                    }

                    this.Vminclass.VoltageWriteFail(this.VoltageTargets.ToList().IndexOf(this.Hryclass.HryLookupTable.VoltageStringRef[index]));
                    this.Vminclass.Setvmin(index, -9999);
                    pass = false;
                }

                return Tuple.Create(this.VoltageTargets.ToList().IndexOf(this.Hryclass.HryLookupTable.VoltageStringRef[index]), pass);
            }

            /// <summary>Function pulls DFF/SS values depending on settings.</summary>
            /// <param name="clearedvariables"> what variables to clear. </param>
            public void Pulldatassddf(List<string> clearedvariables)
            {
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Started");
                if (!clearedvariables.Contains("all") && !clearedvariables.Contains("hry"))
                {
                    this.Globalhrystring = this.Hryclass.HryReadSharedStorage();
                }

                if (this.Recoveryclass != null && (!clearedvariables.Contains("all") && !clearedvariables.Contains("recovery")))
                {
                    if (this.Dffoperation == DFFOperation.Read_REC || this.Dffoperation == DFFOperation.Read_BISR_REC)
                    {
                        this.Recoverydata = this.Recoveryclass.ReadData(true);
                    }
                    else
                    {
                        this.Recoverydata = this.Recoveryclass.ReadData();
                    }
                }

                if (this.Virtualfuseclass != null && (!clearedvariables.Contains("all") && !clearedvariables.Contains("vfdm")))
                {
                    this.Virtualfuseclass.ReadAllSharedStorage();
                }

                if (this.BisrMode != BisrModes.Off)
                {
                    if (this.Currentbisr.Count > 0)
                    {
                        this.Console?.PrintDebug(
                            $"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Made it to function {this.Currentbisr}");
                        foreach (var bisr in this.Currentbisr)
                        {
                            this.Console?.PrintDebug(
                                $"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Running: {bisr.BisrControllerName}");
                            bisr.InitializeBisrString(this.BisrMode, this.Hryclass.HryLookupTable.Bisrdata[bisr.BisrControllerName].Totallength, this.Hryclass.HryLookupTable.Bisrdata[bisr.BisrControllerName].FuseBoxSize);

                            if (!clearedvariables.Contains("all") && !clearedvariables.Contains("bisr"))
                            {
                                if (this.Dffoperation == DFFOperation.Read_BISR ||
                                    this.Dffoperation == DFFOperation.Read_BISR_REC)
                                {
                                    bisr.ReadData(this.BisrMode, true);
                                }
                                else
                                {
                                    bisr.ReadData(this.BisrMode);
                                }
                            }

                            if (this.BisrsetPointHandler[bisr.BisrControllerName] != null &&
                                (this.BisrMode == BisrModes.Bisr || this.BisrMode == BisrModes.Compressed))
                            {
                                this.BisrsetPointHandler[bisr.BisrControllerName]
                                    .SetData(bisr.Fusepatmod(this.BisrMode));
                                Prime.Services.PatConfigService.Apply(
                                    this.BisrsetPointHandler[bisr.BisrControllerName]);
                                this.Console?.PrintDebug(
                                    $"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Just Patmoded: {bisr.BisrControllerName} with value {bisr.Fusepatmod(this.BisrMode)}");
                            }
                        }
                    }
                }

                if (this.Vminclass.Pervminenabled() == true && (!clearedvariables.Contains("all") && !clearedvariables.Contains("vmin")))
                {
                    this.Vminclass.VminReadSharedStorage();
                }
            }

            private void Setflowflag(ref VoltageLevelExecutionResult currentresult, VoltageLevelExecutionResult.FlowFlags running)
            {
                // TODO May need to work on this to make sure its setting all flags correctly.  Have not thought thru all corner cases.
                if (running == VoltageLevelExecutionResult.FlowFlags.REPAIRABLE)
                {
                    if (currentresult.FlowFlag == VoltageLevelExecutionResult.FlowFlags.INITVAL)
                    {
                        currentresult.FlowFlag = running;
                    }
                }
                else if (running == VoltageLevelExecutionResult.FlowFlags.RECOVERY)
                {
                    if (currentresult.FlowFlag == VoltageLevelExecutionResult.FlowFlags.INITVAL)
                    {
                        currentresult.FlowFlag = running;
                    }
                    else if (currentresult.FlowFlag == VoltageLevelExecutionResult.FlowFlags.REPAIRABLE)
                    {
                        currentresult.FlowFlag = VoltageLevelExecutionResult.FlowFlags.REPAIR_RECOVERY;
                    }
                }
                else if (running == VoltageLevelExecutionResult.FlowFlags.REPAIR_RECOVERY)
                {
                    if (currentresult.FlowFlag == VoltageLevelExecutionResult.FlowFlags.INITVAL)
                    {
                        currentresult.FlowFlag = running;
                    }
                }
                else if (running == VoltageLevelExecutionResult.FlowFlags.FAIL)
                {
                    currentresult.FlowFlag = VoltageLevelExecutionResult.FlowFlags.FAIL;
                }
            }

            private bool ChecktoAddRecovery(VoltageLevelExecutionResult currenttest)
            {
                bool returnvalue = false;
                if (this.VoltageLevelExecutionResultsList.Count > 0)
                {
                    // Checking for duplicate results of the previous voltage level execution, to avoid adding duplicate results.
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Comparison of Recovery {this.VoltageLevelExecutionResultsList[this.VoltageLevelExecutionResultsList.Count - 1].Recovery.SequenceEqual(currenttest.Recovery)}");
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Comparison of hry {this.VoltageLevelExecutionResultsList[this.VoltageLevelExecutionResultsList.Count - 1].Hry.SequenceEqual(currenttest.Hry)}");
                    this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Comparison of BISR {this.VoltageLevelExecutionResultsList[this.VoltageLevelExecutionResultsList.Count - 1].Bisr.SequenceEqual(currenttest.Bisr)}");

                    if (this.VoltageLevelExecutionResultsList[this.VoltageLevelExecutionResultsList.Count - 1].Recovery.SequenceEqual(currenttest.Recovery) &&
                        this.VoltageLevelExecutionResultsList[this.VoltageLevelExecutionResultsList.Count - 1].Bisr.SequenceEqual(currenttest.Bisr) &&
                        this.VoltageLevelExecutionResultsList[this.VoltageLevelExecutionResultsList.Count - 1].Hry.SequenceEqual(currenttest.Hry))
                    {
                        returnvalue = false;
                        this.VoltageLevelExecutionResultsList[this.VoltageLevelExecutionResultsList.Count - 1].Repeated += 1;
                    }
                    else
                    {
                        returnvalue = true;
                    }
                }
                else
                {
                    returnvalue = true;
                }

                return returnvalue;
            }

            /// <summary>Compression of Data section.</summary>
            /// <param name = "currentexec" > Class current execution results.</param>
            /// <param name= "failmems" > Index of failing memories. </param>
            /// <param name= "failbisrs" > Names of the failing BISRs to retry compression. </param>
            /// <returns>String of current PD data.</returns>
            private bool ClearUnrepairableFuses(ref VoltageLevelExecutionResult currentexec, Dictionary<string, List<string>> failmems, List<string> failbisrs)
            {
                // TODO add code to do repair compression by removing possible failing fuses
                if (failbisrs.Count > 0)
                {
                    foreach (BisrChainResult bisrchain in currentexec.Bisr)
                    {
                        if (failbisrs.Contains(bisrchain.BisrControllerName))
                        {
                            var clearidxs = new List<int>();
                            foreach (KeyValuePair<string, List<string>> mem_names in failmems)
                            {
                                foreach (var mem in mem_names.Value)
                                {
                                    var idx = 0;
                                    foreach (var location in bisrchain.BisrComments)
                                    {
                                        if (Regex.IsMatch(location, mem))
                                        {
                                            if (this.Loglevel == PrimeLogLevel.PRIME_DEBUG)
                                            {
                                                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Removing Memorie {mem} from BISR Repairs");
                                            }

                                            clearidxs.Add(idx);
                                        }

                                        idx += 1;
                                    }
                                }
                            }

                            var tempbisr = bisrchain.RawData.ToList();
                            foreach (var index in clearidxs)
                            {
                                tempbisr[index] = '0';
                            }

                            if (this.Loglevel == PrimeLogLevel.PRIME_DEBUG)
                            {
                                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Removing failing Memories to allow BISR compress to be optimized");
                                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Original BISR: {bisrchain.RawData}");
                                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]{this.Diename}: Updated BISR: {tempbisr.ToString()}");
                            }

                            bisrchain.RawData = tempbisr.ToString();
                        }
                    }

                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary> Containter class to temporarily store all execution results for the current test instance.</summary>
        public class VoltageLevelExecutionResult
        {
            /// <summary> Initializes a new instance of the <see cref="VoltageLevelExecutionResult"/> class..</summary>
            /// <param name = "bisr" > Bisr results.</param>
            /// <param name= "hry" > Index of failing memories. </param>
            /// <param name= "recovery" > Recovery results. </param>
            /// <param name= "vmin" > Vmin results. </param>
            /// <param name= "scoreboardFails" > Scoreboard fialures. </param>
            /// <param name="console">Prime.Services.ConsoleService or null depending on the current instances LogLevel.</param>
            public VoltageLevelExecutionResult(List<BisrChainResult> bisr, ConcurrentDictionary<int, char> hry, List<char> recovery, List<double> vmin, ConcurrentDictionary<string, HashSet<int>> scoreboardFails, IConsoleService console)
            {
                this.Console = console;
                this.Bisr = bisr;
                this.Hry = new ConcurrentDictionary<int, char>(hry);
                this.Recovery = recovery;
                this.Vmin = new List<double>(vmin);
                this.Repeated = 1;
                this.ScoreboardFails = new ConcurrentDictionary<string, HashSet<int>>(scoreboardFails);
                this.Voltagestested = new bool[vmin.Count];
                for (int i = 0; i < vmin.Count; i++)
                {
                    this.Voltagestested[i] = true;
                }
            }

            /// <summary> Containter class to temporarily store all execution results for the current test instance.</summary>
            public enum FlowFlags
            {
                /// <summary> Full Pass.</summary>
                FULL_PASS = 1,

                /// <summary> Repairable.</summary>
                REPAIRABLE = 2,

                /// <summary> Recovery.</summary>
                RECOVERY = 3,

                /// <summary> Repair_Recovery.</summary>
                REPAIR_RECOVERY = 4,

                /// <summary> Fail.</summary>
                FAIL = 0,

                /// <summary> Error.</summary>
                ERROR = 5,

                /// <summary> InitVal.</summary>
                INITVAL = 6,

                /// <summary> Reset.</summary>
                RESET = 8,
            }

            /// <summary> Gets or Sets BISR chains.</summary>
            public List<BisrChainResult> Bisr { get; set; }

            /// <summary> Gets or Sets HRY.</summary>
            public ConcurrentDictionary<int, char> Hry { get; set; }

            /// <summary> Gets or Sets Scoreboard Fails.</summary>
            public ConcurrentDictionary<string, HashSet<int>> ScoreboardFails { get; set; }

            /// <summary> Gets or Sets Vmins.</summary>
            public List<double> Vmin { get; set; }

            /// <summary> Gets or Sets Recovery.</summary>
            public List<char> Recovery { get; set; }

            /// <summary> Gets or Sets Priority.</summary>
            public int Priority { get; set; }

            /// <summary> Gets or Sets Repeated.</summary>
            public int Repeated { get; set; }

            /// <summary> Gets or Sets Voltage Rails Tested and Whether they pass.</summary>
            public bool[] Voltagestested { get; set; }

            /// <summary> Gets or Sets FlowFlag.</summary>
            public FlowFlags FlowFlag { get; set; } = FlowFlags.INITVAL;

            /// <summary>
            /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
            /// </summary>
            protected IConsoleService Console { get; set; }

            /// <summary> Prints execution data.</summary>
            /// <param name = "diename" > Dienameesults.</param>
            public void ExecutionPrint(string diename)
            {
                this.Console?.PrintDebug($"\t{diename} = HRY: {string.Join(string.Empty, this.Hry)}");

                if (this.Bisr.Count > 0)
                {
                    this.Console?.PrintDebug($"\t{diename} = BISR:");
                    foreach (var bisr in this.Bisr)
                    {
                        bisr.PrintBisr();
                    }
                }

                if (this.Recovery != null)
                {
                    this.Console?.PrintDebug($"\t{diename} = Recovery: {string.Join(string.Empty, this.Recovery)}");
                }

                this.Console?.PrintDebug($"\t{diename} = Priority: {this.Priority}");
                this.Console?.PrintDebug($"\t{diename} = FlowFlag: {this.FlowFlag.ToString()}");
                if (this.Vmin.Count > 0)
                {
                    this.Console?.PrintDebug($"\t{diename} = Vmin: {string.Join(string.Empty, this.Vmin)}");
                }

                if (this.Voltagestested.Length > 0)
                {
                    this.Console?.PrintDebug($"\t{diename} = Pass: {string.Join(string.Empty, this.Voltagestested)}");
                }

                this.Console?.PrintDebug($"\t{diename} = Repeated: {this.Repeated}");

                if (this.ScoreboardFails.Count > 0)
                {
                    this.Console?.PrintDebug($"\t{diename} = Scoreboard Information:");
                    foreach (var score in this.ScoreboardFails)
                    {
                        this.Console?.PrintDebug($"\t{diename} = Pattern: {score.Key} Failing controllers Indexs: {string.Join(string.Empty, score.Value.ToList())}");
                    }
                }
            }
        }
    }
}
