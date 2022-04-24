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

namespace VminTC
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using DDG;
    using Prime;
    using Prime.FunctionalService;
    using Prime.PhAttributes;
    using Prime.PlistService;
    using Prime.SharedStorageService;
    using Prime.TestMethods;
    using Prime.TestMethods.VminSearch;
    using Prime.VoltageService;

    /// <summary>
    /// This class is intended to overwrite the test method PrimeVminSearchTestMethod.
    /// </summary>
    [PrimeTestMethod]
    public class VminTC : PrimeVminSearchTestMethod, IVminSearchExtensions
    {
        /// <summary>
        /// Fail port.
        /// </summary>
        public const int FailPort = 0;

        /// <summary>
        /// Pass port.
        /// </summary>
        public const int PassPort = 1;

        /// <summary>
        /// Fail rules port.
        /// </summary>
        public const int FailRulesPort = 2;

        /// <summary>
        /// Fail recovery port.
        /// </summary>
        public const int FailRecoveryPort = 3;

        /// <summary>
        /// Fail thermals port.
        /// </summary>
        public const int FailThermalsPort = 4;

        /// <summary>
        /// Fail amble port.
        /// </summary>
        public const int FailAmblePort = 5;

        private INoCaptureTest postPlist;
        private ICaptureFailureTest captureFailureTest;
        private List<string> voltageTargets;
        private List<int> flows;
        private SearchResults currentSearchResults;
        private int port;
        private List<PatternOccurrence> plistContentsIndex;
        private Dictionary<PatternOccurrence, List<double>> perPatternVoltage;
        private PatternOccurrence startPattern;
        private bool failedUpdateDieRecoveryTracking;
        private IPlistObject plistObject;
        private List<double> startVoltages;

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
        /// Enumerate possible recovery modes.
        /// </summary>
        public enum RecoveryModes
        {
            /// <summary>
            /// If search and rules pass: Update VminForwarding and DieRecovery. Exit Port 1.
            /// If search and rules fail: Update VminForwarding, skip DieRecovery. Exit Port 0.
            /// If search passes but fail rules: Update VminForwarding, skip DieRecovery. Exit Port 2.
            /// If search fails but rules pass: Update VminForwarding, skip DieRecovery. Exit Port 0.
            /// </summary>
            Default,

            /// <summary>
            /// If search and rules pass: Update VminForwarding and DieRecovery. Exit Port 1.
            /// If search and rules fail: Update VminForwarding, skip DieRecovery. Exit Port 0.
            /// If search passes but fail rules: Update VminForwarding, skip DieRecovery. Exit Port 2.
            /// If search fails but rules pass: Skip VminForwarding, update DieRecovery. Exit Port 3.
            /// </summary>
            RecoveryPort,

            /// <summary>
            /// If search and rules pass: Update VminForwarding and DieRecovery. Exit Port 1.
            /// If search fails, rules fail and loop has reached MaxRepetitionCount: Update VminForwarding, skip DieRecovery. Exit Port 0.
            /// If search passes, rules fail: Update VminForwarding, skip DieRecovery. Exit Port 2.
            /// If search fails, rules pass and loop has not reached MaxRepetitionCount: Update VminForwarding, update DieRecovery and repeat search.
            /// If search fails, rules pass and loop has reached MaxRepetitionCount: Update VminForwarding, skip DieRecovery. Exit Port 3.
            /// </summary>
            RecoveryLoop,

            /// <summary>
            /// If search and rules pass: Update VminForwarding and DieRecovery. Exit Port 1.
            /// If search fails, rules fail and loop has reached MaxRepetitionCount: Update VminForwarding, skip DieRecovery. Exit Port 0.
            /// If search passes, rules fail and loop has not reached MaxRepetitionCount: Update mask to skip passing targets and repeat search (running fail only).
            /// If search passes, rules fail and loop has reached MaxRepetitionCount: Update VminForwarding, skip DieRecovery. Exit Port 2.
            /// If search fails, rules pass and loop has not reached MaxRepetitionCount: Update VminForwarding, update DieRecovery and repeat search.
            /// If search fails, rules pass and loop has reached MaxRepetitionCount: Update VminForwarding, skip DieRecovery. Exit Port 3.
            /// </summary>
            RecoveryFailRetest,

            /// <summary>
            /// If search and rules pass: Update VminForwarding. Exit Port 1.
            /// If search or rules fail: Update VminForwarding. Exit Port 0.
            /// </summary>
            NoRecovery,
        }

        /// <summary>
        /// Gets or sets test mode.
        /// </summary>
        public TestModes TestMode { get; set; }

        /// <summary>
        /// Gets or sets comma separated corner identifiers.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString CornerIdentifiers { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets CTV capture pins.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString CtvPins { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets shared storage key for a pre-populated list of PatConfigSetPoints by CornerIdentification.
        /// </summary>
        public TestMethodsParams.String CornerPatConfigSetPoints { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets DTS configuration name. Empty configuration name means DTS capture move is disabled.
        /// </summary>
        public TestMethodsParams.String DtsConfiguration { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets FailCaptureCount. Default 1 will set stop-on-first-fail.
        /// Any value greater than 1 will run full plist unless used in combination with ReturnOn plist options.
        /// </summary>
        public TestMethodsParams.UnsignedInteger FailCaptureCount { get; set; } = 1;

        /// <summary>
        /// Gets or sets forwarding mode.
        /// </summary>
        public ForwardingModes ForwardingMode { get; set; } = ForwardingModes.None;

        /// <summary>
        /// Gets or sets forwarding mode.
        /// </summary>
        public TestMethodsParams.String InitialMaskBits { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets LimitGuardband to be used with VminForwarding SearchGuardbandEnabled option.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString LimitGuardband { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets mask pins.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString MaskPins { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets recovery map name. User can enter Json stream or input file.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString PinMap { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets PostPlist execution.
        /// </summary>
        public TestMethodsParams.Plist PostPlist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets DieRecovery update mode.
        /// </summary>
        public RecoveryModes RecoveryMode { get; set; } = RecoveryModes.Default;

        /// <summary>
        /// Gets or sets DieRecoveryOutgoing_ rule.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString RecoveryOptions { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets recovery tracking name to be updated.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString RecoveryTrackingOutgoing { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an recovery tracking name to be sourced.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString RecoveryTrackingIncoming { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets scoreboard per pattern capture fails limit. Default is 1.
        /// </summary>
        public TestMethodsParams.UnsignedInteger ScoreboardPerPatternFails { get; set; } = 1;

        /// <summary>
        /// Gets or sets trigger levels test condition name.
        /// </summary>
        public TestMethodsParams.LevelsCondition TriggerLevelsCondition { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets trigger map.
        /// </summary>
        public TestMethodsParams.String TriggerMap { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets vmin result. Stores value in SharedStorage using comma-separated key names with Context.DUT.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString VminResult { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a list of voltage overrides from VminForwarding.
        /// </summary>
        public TestMethodsParams.String VoltageConverter { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an offset to applied voltage.
        /// </summary>
        public TestMethodsParams.String VoltagesOffset { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets interface to PinMapBase. Reference gets updated during CustomVerify.
        /// </summary>
        protected IPinMap PinMap_ { get; set; }

        /// <summary>
        /// Gets or sets interface to DieRecoveryBase. Reference gets updated during CustomVerify.
        /// </summary>
        protected IDieRecovery DieRecoveryOutgoing_ { get; set; }

        /// <summary>
        /// Gets or sets interface to DieRecoveryBase. Reference gets updated during CustomVerify.
        /// </summary>
        protected IDieRecovery DieRecoveryIncoming_ { get; set; }

        /// <summary>
        /// Gets or sets interface to RecoveryMode.
        /// </summary>
        protected IRecoveryMode RecoveryMode_ { get; set; }

        /// <summary>
        /// Gets or sets interface to VminForwarding_. Stores tuple with corner name, flow and interface object.
        /// </summary>
        protected List<Tuple<string, int, IVminForwardingCorner>> VminForwarding_ { get; set; }

        /// <summary>
        /// Gets or sets a wrapper for DTS service.
        /// </summary>
        protected DTSHandler DTSHandler_ { get; set; } = new DTSHandler();

        /// <summary>
        /// Gets or sets a wrapper for CornerPatConfigSetPoints.
        /// </summary>
        protected CornerPatConfigSetPointsHandler CornerPatConfigSetPointsHandler_ { get; set; }

        /// <summary>
        /// Gets or sets initial mask array for each search iteration.
        /// </summary>
        protected BitArray InitialSearchMask_ { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether apply search voltage should be skipped. Used for functional modes.
        /// </summary>
        protected bool SkipApplySearchVoltage_ { get; set; }

        /// <summary>
        /// Gets or sets incoming mask.
        /// </summary>
        protected BitArray IncomingMask_ { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether FivrVoltageMode is enabled.
        /// </summary>
        protected bool IsFivrVoltageMode_ { get; set; } = false;

        /// <summary>
        /// Gets or sets voltage converter options from commandline.
        /// </summary>
        protected VoltageConverterOptions VoltageConverterOptions_ { get; set; }

        /// <summary>
        /// Gets or sets different feature switches.
        /// </summary>
        protected FeatureSwitches Switches_ { get; set; }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Passed.")]
        [Returns(0, PortType.Fail, "Failed search and failed recovery rules.")]
        [Returns(2, PortType.Fail, "Passed search but failed recovery rules.")]
        [Returns(3, PortType.Fail, "Failed search and passed recovery rules.")]
        [Returns(4, PortType.Fail, "Failed DTS monitor.")]
        [Returns(5, PortType.Fail, "Failed pattern amble.")]
        public override int Execute()
        {
            if (this.LogLevel != PrimeLogLevel.DISABLED)
            {
                var parameters = Prime.Services.TestProgramService.GetCurrentTestInstanceParameters();
                var message = parameters.Aggregate("\nCurrent Test Instance Parameters:", (current, parameter) => current + $"\n--Parameter=[{parameter.Key}] Value=[{parameter.Value}]");
                this.Console?.PrintDebug(message);
            }

            base.Execute();
            this.ExecutePostInstance();
            return this.port;
        }

        /// <inheritdoc />
        public override void CustomVerify()
        {
            this.InitializeVoltageTargets();
            this.VerifyForwarding();
            this.VerifyCornerSetPointsHandles();
            this.VerifyPinMap();
            this.VerifyDieRecovery();
            this.VerifySingleVminMode();
            this.VerifyMultiVminMode();
            this.VerifyRecoveryMode();
            this.VerifySinglePointMode();
            this.VerifyPostPlist();
            this.VerifyGetPlistContentsIndex();
            this.VerifyPerPatternPrinting();
        }

        /// <inheritdoc />
        BitArray IVminSearchExtensions.GetInitialMaskBits()
        {
            using (var sw = Prime.Services.PerformanceService?.GetStopWatch(string.Empty))
            {
                if (this.voltageTargets.Count == 1)
                {
                    return !this.InitialSearchMask_.Cast<bool>().Contains(false)
                        ? new BitArray(this.voltageTargets.Count, true)
                        : new BitArray(this.voltageTargets.Count, false);
                }

                if (this.InitialSearchMask_.Count == this.voltageTargets.Count)
                {
                    return new BitArray(this.InitialSearchMask_);
                }

                var configurations = this.PinMap_.GetConfiguration();

                var index = 0;
                var voltageInitialMask = string.Empty;
                foreach (var configuration in configurations)
                {
                    var value = "1";
                    var size = configuration.NumberOfTrackerElements;
                    for (var i = 0; i < size; i++)
                    {
                        if (this.InitialSearchMask_[index + i])
                        {
                            continue;
                        }

                        value = "0";
                        break;
                    }

                    voltageInitialMask += value;
                    index += size;
                }

                return voltageInitialMask.ToBitArray();
            }
        }

        /// <inheritdoc/>
        IFunctionalTest IVminSearchExtensions.GetFunctionalTest(string patlist, string levelsTc, string timingsTc, string prePlist)
        {
            this.VerifyDts();
            this.VerifyFeatureSwitchSettings();
            this.VerifyUpdatePlistObject();
            this.SetReturnOnGlobalStickyError();

            var ctvPins = this.CtvPins.ToList();
            if (this.DTSHandler_.IsDtsEnabled() && !ctvPins.Contains(this.DTSHandler_.GetCtvPinName()))
            {
                ctvPins.Add(this.DTSHandler_.GetCtvPinName());
            }

            if (ctvPins.Count == 0)
            {
                this.captureFailureTest = this.TestMode == TestModes.Scoreboard ?
                    Prime.Services.FunctionalService.CreateCaptureFailureTest(patlist, levelsTc, timingsTc, this.FailCaptureCount, this.ScoreboardPerPatternFails, prePlist) :
                    Prime.Services.FunctionalService.CreateCaptureFailureTest(patlist, levelsTc, timingsTc, this.FailCaptureCount, prePlist);
            }
            else if (this.Switches_.TraceCtv || this.Switches_.CtvPerCycle)
            {
                this.captureFailureTest = this.TestMode == TestModes.Scoreboard ?
                    Prime.Services.FunctionalService.CreateCaptureFailureAndCtvPerCycleTest(patlist, levelsTc, timingsTc, ctvPins.ToList(), this.FailCaptureCount, this.ScoreboardPerPatternFails, prePlist) :
                    Prime.Services.FunctionalService.CreateCaptureFailureAndCtvPerCycleTest(patlist, levelsTc, timingsTc, ctvPins.ToList(), this.FailCaptureCount, prePlist);
            }
            else
            {
                this.captureFailureTest = this.TestMode == TestModes.Scoreboard ?
                    Prime.Services.FunctionalService.CreateCaptureFailureAndCtvPerPinTest(patlist, levelsTc, timingsTc, ctvPins.ToList(), this.FailCaptureCount, this.ScoreboardPerPatternFails, prePlist) :
                    Prime.Services.FunctionalService.CreateCaptureFailureAndCtvPerPinTest(patlist, levelsTc, timingsTc, ctvPins.ToList(), this.FailCaptureCount, prePlist);
            }

            if (!string.IsNullOrEmpty(this.MaskPins))
            {
                this.captureFailureTest.SetPinMask(this.MaskPins.ToList());
            }

            if (!string.IsNullOrEmpty(this.TriggerMap))
            {
                this.captureFailureTest.SetTriggerMap(this.TriggerMap);
            }

            if (this.Switches_.IsStartOnFirstFail)
            {
                this.captureFailureTest.EnableStartPatternOnFirstFail();
            }
            else
            {
                this.captureFailureTest.DisableStartPattern();
            }

            return this.captureFailureTest;
        }

        /// <inheritdoc />
        void IVminSearchExtensions.ApplyMask(BitArray maskBits, IFunctionalTest functionalTest)
        {
            using (var sw = Prime.Services.PerformanceService?.GetStopWatch(string.Empty))
            {
                if (this.PinMap_ != null)
                {
                    var mask = this.TestMode == TestModes.MultiVmin
                        ? this.CombineMask(maskBits)
                        : this.InitialSearchMask_;
                    this.Console?.PrintDebug(
                        $"{MethodBase.GetCurrentMethod()?.Name} --Applying Mask Bits:{mask.ToBinaryString()}");
                    this.PinMap_.MaskPins(mask, ref functionalTest, this.MaskPins.ToList());
                    this.PinMap_.ModifyPlist(mask, ref functionalTest);
                }
                else
                {
                    functionalTest.SetPinMask(this.MaskPins.ToList());
                }

                this.StoreStartPatternForPerPatternVmin();
            }
        }

        /// <inheritdoc/>
        void IVminSearchExtensions.ApplyPreExecuteSetup(string plistName)
        {
            using (var sw = Prime.Services.PerformanceService?.GetStopWatch(string.Empty))
            {
                this.port = FailPort;
                this.DTSHandler_.Reset();
                this.CornerPatConfigSetPointsHandler_.Run();
            }
        }

        /// <inheritdoc/>
        void IVminSearchExtensions.ApplyInitialVoltage(IVoltage voltageObject)
        {
            using (var sw = Prime.Services.PerformanceService?.GetStopWatch(string.Empty))
            {
                var overrides = DDG.VoltageHandler.GetVoltageOverrides(this.VoltageConverterOptions_);
                DDG.VoltageHandler.ApplyInitialVoltage(voltageObject, this.LevelsTc, overrides);
            }
        }

        /// <inheritdoc />
        void IVminSearchExtensions.ApplyPreSearchSetup(string plistName)
        {
            using (var sw = Prime.Services.PerformanceService?.GetStopWatch(string.Empty))
            {
                this.currentSearchResults.DecodedResult = null;
                this.currentSearchResults.SearchFlowStates = this.SearchStates;

                this.SetInitialMask();
                if (this.Switches_.DisableMaskedTargets && this.InitialSearchMask_ != null)
                {
                    this.PinMap_.ApplyPatConfig(this.InitialSearchMask_, this.Patlist);
                }

                if (this.Switches_.IsPerPatternPrintingEnabled)
                {
                    this.startPattern = this.plistContentsIndex.First();
                    this.perPatternVoltage = new Dictionary<PatternOccurrence, List<double>>();
                }
                else
                {
                    this.startPattern = null;
                    this.perPatternVoltage = null;
                }
            }
        }

        /// <inheritdoc />
        void IVminSearchExtensions.ApplySearchVoltage(IVoltage voltageObject, List<double> voltageValues)
        {
            using (var sw = Prime.Services.PerformanceService?.GetStopWatch(string.Empty))
            {
                if (this.SkipApplySearchVoltage_)
                {
                    this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: skipping voltage setup. Using initial voltage.");
                    return;
                }

                DDG.VoltageHandler.ApplySearchVoltage(voltageObject, voltageValues, this.VoltagesOffset);
            }
        }

        /// <inheritdoc />
        BitArray IVminSearchExtensions.ProcessPlistResults(bool plistExecuteResult, IFunctionalTest functionalTest)
        {
            using (var sw = Prime.Services.PerformanceService?.GetStopWatch(string.Empty))
            {
                if (this.Switches_.TraceCtv && functionalTest is ICaptureCtvPerCycleTest captureCtvPerCycleTest)
                {
                    VminUtilities.TraceCTVs(captureCtvPerCycleTest);
                }
                else if (functionalTest is ICaptureFailureAndCtvPerPinTest ctvTest)
                {
                    try
                    {
                        var ctvData = ctvTest.GetCtvData(this.DTSHandler_.GetCtvPinName());
                        this.DTSHandler_.ProcessPlistDts(ctvData);
                    }
                    catch
                    {
                        // ignored DTS processing when there is no CTV data.
                    }
                }

                if (functionalTest is ICaptureFailureTest test)
                {
                    this.ProcessCaptureFailures(plistExecuteResult, test);
                }

                var defaultResult = new BitArray(this.voltageTargets.Count, !plistExecuteResult);
                if (this.PinMap_ == null)
                {
                    return this.currentSearchResults.DecodedResult = defaultResult;
                }

                this.currentSearchResults.DecodedResult = this.PinMap_.DecodeFailure(functionalTest);
                if (this.Switches_.IgnoreMaskedResults)
                {
                    this.currentSearchResults.DecodedResult =
                        this.currentSearchResults.DecodedResult.And(new BitArray(this.InitialSearchMask_).Not());
                }

                switch (this.TestMode)
                {
                    case TestModes.MultiVmin
                        when this.currentSearchResults.DecodedResult.Count == this.voltageTargets.Count:
                        return this.currentSearchResults.DecodedResult;
                    case TestModes.MultiVmin
                        when this.currentSearchResults.DecodedResult.Count != this.voltageTargets.Count:
                        return this.PinMap_.FailTrackerToFailVoltageDomains(this.currentSearchResults.DecodedResult);
                    default:
                        return defaultResult;
                }
            }
        }

        /// <inheritdoc/>
        List<double> IVminSearchExtensions.GetStartVoltageValues(List<string> startVoltagesKeys)
        {
            return this.TestMode == TestModes.SingleVmin ? new List<double> { this.startVoltages.Max() } : this.startVoltages;
        }

        /// <inheritdoc/>
        List<double> IVminSearchExtensions.GetEndVoltageLimitValues(List<string> endVoltageLimitKeys)
        {
            return this.CalculateVoltageLimits(endVoltageLimitKeys);
        }

        /// <inheritdoc />
        List<double> IVminSearchExtensions.GetLowerStartVoltageValues(List<string> lowerStartVoltageKeys)
        {
            return this.CalculateVoltageLimits(lowerStartVoltageKeys);
        }

        /// <inheritdoc/>
        bool IVminSearchExtensions.IsSinglePointMode() => this.TestMode == TestModes.Functional
                                                          || this.TestMode == TestModes.Scoreboard
                                                          || (this.ForwardingMode != ForwardingModes.None && VminForwarding.Service != null && VminForwarding.Service.IsSinglePointMode());

        /// <inheritdoc/>
        IVoltage IVminSearchExtensions.GetSearchVoltageObject(List<string> targets, string plistName)
        {
            this.IsFivrVoltageMode_ = this.FeatureSwitchSettings.ToList().Contains("fivr_mode_on");
            if (this.IsFivrVoltageMode_)
            {
                if (string.IsNullOrEmpty(this.FivrCondition))
                {
                    throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: fivr_mode_on requires to to {nameof(this.FivrCondition)} .");
                }

                var fivrObject = DDG.VoltageHandler.GetVoltageObject(targets, this.LevelsTc, this.Patlist, this.FivrCondition, null, this.VoltageConverter, out var fivrOptions);
                this.VoltageConverterOptions_ = fivrOptions;
                return fivrObject;
            }

            if (!string.IsNullOrWhiteSpace(this.TriggerLevelsCondition) && string.IsNullOrWhiteSpace(this.TriggerMap))
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: use of {nameof(this.TriggerLevelsCondition)} requires to use {nameof(this.TriggerMap)}");
            }

            var dpsObject = DDG.VoltageHandler.GetVoltageObject(targets, this.LevelsTc, this.Patlist, null, this.TriggerLevelsCondition, this.VoltageConverter.ToString(), out var dpsOptions);
            this.VoltageConverterOptions_ = dpsOptions;
            return dpsObject;
        }

        /// <inheritdoc/>
        int IVminSearchExtensions.PostProcessSearchResults(List<SearchResultData> searchResults)
        {
            using (var sw = Prime.Services.PerformanceService?.GetStopWatch(string.Empty))
            {
                this.currentSearchResults.SearchResultData = this.currentSearchResults.SearchResultData ?? searchResults;
                this.CustomPrintToItuff(searchResults);
                this.EvaluateAmbleFails(searchResults);
                this.UpdateDieRecoveryTracking();
                this.UpdatePort();
                this.ProcessVminResults();
                return this.port;
            }
        }

        /// <inheritdoc/>
        bool IVminSearchExtensions.HasToContinueToNextSearch(List<SearchResultData> searchResults, IFunctionalTest functionalTest)
        {
            if (!this.Switches_.ForceRecoveryLoop && this.currentSearchResults.FailedSearch && this.currentSearchResults.FailedRules)
            {
                this.Console?.PrintDebug("Search failed with no option for recovery.");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        bool IVminSearchExtensions.HasToRepeatSearch(List<SearchResultData> searchResults)
        {
            this.currentSearchResults.SearchFlowStates = this.SearchStates;
            this.currentSearchResults.SearchResultData = searchResults;
            var hasToRepeat = this.RecoveryMode_.HasToRepeatSearch(ref this.currentSearchResults, this.PinMap_, this.DieRecoveryOutgoing_, this.RecoveryOptions, this.TestMode == TestModes.MultiVmin);
            return (this.Switches_.ForceRecoveryLoop && !searchResults.Last().IsPass) || hasToRepeat;
        }

        /// <inheritdoc />
        int IVminSearchExtensions.GetBypassPort()
        {
            if (this.Switches_.ResetPointers)
            {
                this.PinMap_ = null;
                this.DieRecoveryOutgoing_ = null;
                this.DieRecoveryIncoming_ = null;
                this.VminForwarding_ = null;
                this.CornerPatConfigSetPointsHandler_ = null;
                this.DTSHandler_ = new DTSHandler();
                this.plistContentsIndex = null;
                this.CustomVerify();
            }

            this.SetDieRecoveryTrackers();
            this.port = this.SetIncomingMask();

            if (this.port == PassPort)
            {
                this.DieRecoveryOutgoing_?.LogTrackingStructure(this.IncomingMask_, new BitArray(this.IncomingMask_.Length, false));
            }

            return this.port;
        }

        /// <summary>
        /// Updates VminResult. If one target was already tested it will take fail result or max voltage.
        /// </summary>
        /// <param name="voltageResults">Aggregated voltage results.</param>
        protected internal void UpdateVminResult(List<double> voltageResults)
        {
            using (var sw = Prime.Services.PerformanceService?.GetStopWatch(string.Empty))
            {
                if (string.IsNullOrEmpty(this.VminResult))
                {
                    return;
                }

                if (voltageResults == null)
                {
                    voltageResults = new List<double>(Enumerable
                        .Repeat(VminUtilities.VoltageMaskValue, this.voltageTargets.Count()).ToArray());
                }

                var vminResultTokens = this.VminResult.ToList();
                if (vminResultTokens.First().StartsWith("D."))
                {
                    var concatenated = string.Join("v", voltageResults.Select(o =>
                            o.Equals(VminUtilities.VoltageFailValue, 3) ? "-9999" :
                            o.Equals(VminUtilities.VoltageMaskValue, 3) ? "-8888" : $"{o:N3}"));
                    var key = vminResultTokens.First().Substring(2);
                    Prime.Services.DffService.SetDff(key, concatenated);
                }
                else if (vminResultTokens.Count == 1)
                {
                    var key = vminResultTokens.First();
                    var value = voltageResults.Min() < 0 ? voltageResults.Min() : voltageResults.Max();
                    Prime.Services.SharedStorageService.InsertRowAtTable(key, value, Context.DUT);
                }
                else
                {
                    for (var i = 0; i < voltageResults.Count; i++)
                    {
                        Prime.Services.SharedStorageService.InsertRowAtTable(vminResultTokens[i], voltageResults[i], Context.DUT);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates start voltage using incoming parameter and vmin forwarding.
        /// </summary>
        /// <param name="startVoltagesKeys">Starting voltage from parameter.</param>
        /// <returns>List of starting voltages.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Invalid mode.</exception>
        protected internal List<double> CalculateStartVoltageValues(List<string> startVoltagesKeys)
        {
            var startVoltagesValues = this.ExpandVoltageKeys(startVoltagesKeys);
            if (!string.IsNullOrEmpty(this.CornerIdentifiers) && (this.ForwardingMode == ForwardingModes.Input ||
                                                                  this.ForwardingMode == ForwardingModes.InputOutput))
            {
                for (var i = 0; i < startVoltagesValues.Count; i++)
                {
                    var vminForwardingVoltage = this.VminForwarding_[i].Item3.GetStartingVoltage(startVoltagesValues[i]);
                    if (vminForwardingVoltage > startVoltagesValues[i])
                    {
                        startVoltagesValues[i] = vminForwardingVoltage;
                    }
                }
            }

            if (this.voltageTargets.Count == 1 && startVoltagesValues.Count > 1)
            {
                return new List<double> { startVoltagesValues.Max() };
            }

            return new List<double>(startVoltagesValues);
        }

        /// <summary>
        /// Calculates voltage limits from keys.
        /// </summary>
        /// <param name="voltageKeys">Voltage limit keys. It can take DFF, SharedStorage or a list of values.</param>
        /// <returns>Evaluated doubles.</returns>
        protected internal List<double> CalculateVoltageLimits(List<string> voltageKeys)
        {
            var voltages = this.ExpandVoltageKeys(voltageKeys);
            return this.TestMode == TestModes.SingleVmin ? new List<double> { voltages.Max() } : voltages;
        }

        /// <summary>
        /// Sets Initial Mask bits using bit set OR operation for InitialMaskBits and DieRecoveryIncoming_.
        /// </summary>
        protected internal void SetInitialMask()
        {
            var result = this.RecoveryMode_.GetMaskBits(this.currentSearchResults, !this.Switches_.ForceRecoveryLoop);
            if (!string.IsNullOrEmpty(this.MultiPassMasks))
            {
                result.Or(this.CurrentMultiPassMask);
            }

            result = this.ProcessCoreGroups(result);
            this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Name} --Initial Search Mask Bits:{result.ToBinaryString()}");
            this.InitialSearchMask_ = result;
        }

        /// <summary>
        /// Sets incoming mask from parameter or DieRecovery tracking.
        /// </summary>
        /// <returns>Bypass port result; 1 for bypass and -1 to continue testing.</returns>
        protected internal int SetIncomingMask()
        {
            BitArray result;
            if (!string.IsNullOrEmpty(this.InitialMaskBits))
            {
                result = this.InitialMaskBits.ToString().ToBitArray();
                if (this.DieRecoveryIncoming_ != null && (this.ForwardingMode == ForwardingModes.InputOutput || this.ForwardingMode == ForwardingModes.Input))
                {
                    var trackingStructureMaskBits = this.DieRecoveryIncoming_.GetMaskBits();
                    result = result.Or(trackingStructureMaskBits);
                }
            }
            else if (this.DieRecoveryIncoming_ != null && (this.ForwardingMode == ForwardingModes.InputOutput || this.ForwardingMode == ForwardingModes.Input))
            {
                result = this.DieRecoveryIncoming_.GetMaskBits();
            }
            else if (this.PinMap_ != null)
            {
                var configurations = this.PinMap_.GetConfiguration();
                var size = configurations.Sum(decoder => decoder.NumberOfTrackerElements);
                result = new BitArray(size, false);
            }
            else
            {
                result = new BitArray(this.voltageTargets.Count, false);
            }

            if (result.Cast<bool>().Any(o => !o))
            {
                this.startVoltages = this.CalculateStartVoltageValues(this.StartVoltages);
                if (this.TestMode == TestModes.MultiVmin && this.DieRecoveryIncoming_ == null)
                {
                    var maskFromVoltages = this.GetMaskBitsFromVoltages(this.startVoltages, result.Count);
                    result.Or(maskFromVoltages);
                }
                else if (this.startVoltages.All(o => o < 0) && !this.SkipApplySearchVoltage_)
                {
                    result.SetAll(true);
                }
            }

            this.IncomingMask_ = new BitArray(result);
            this.currentSearchResults = new SearchResults
            {
                TestResultsBits = new List<BitArray>(),
                RulesResultsBits = new BitArray(this.IncomingMask_.Count, false),
                IncomingMask = new BitArray(this.IncomingMask_),
                MaxRepetitionCount = this.MaxRepetitionCount,
            };

            this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Name} --Incoming Mask Bits:{result.ToBinaryString()}");
            var bypass = result.Cast<bool>().Contains(false) ? -1 : 1;
            if (bypass == 1)
            {
                this.UpdateVminResult(null);
            }

            return bypass;
        }

        /// <summary>
        /// Process capture failure test data to console.
        /// </summary>
        /// <param name="plistExecuteResult">Plist result.</param>
        /// <param name="plist">Capture failure test object.</param>
        protected internal void ProcessCaptureFailures(bool plistExecuteResult, ICaptureFailureTest plist)
        {
            var fails = this.UpdatePatternFailTable(plistExecuteResult, plist);
            if (this.LogLevel == PrimeLogLevel.DISABLED || plistExecuteResult)
            {
                return;
            }

            this.PrintFailData(plist, fails);
        }

        /// <summary>
        /// Determine port based on search results.
        /// </summary>
        protected internal void UpdatePort()
        {
            this.port = this.RecoveryMode_.GetPort(this.currentSearchResults);
            if (this.currentSearchResults.FailedSearch && this.currentSearchResults.FailedAmble)
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Name} --Search fail due to amble pattern. Routing to port={FailAmblePort}.");
                this.port = FailAmblePort;
                return;
            }

            if (this.failedUpdateDieRecoveryTracking)
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Name} --DieRecovery tracking update failed. Routing to port={FailPort}.");
                this.port = FailPort;
                return;
            }

            var passedDts = this.DTSHandler_.EvaluateLimits();
            if (!passedDts && !this.currentSearchResults.FailedSearch)
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Name} --DTS monitor failed. Routing to port={FailThermalsPort}.");
                this.port = FailThermalsPort;
            }
        }

        /// <summary>
        /// Verification of SingleVmin Mode parameters.
        /// </summary>
        protected internal void VerifySingleVminMode()
        {
            if (this.TestMode != TestModes.SingleVmin)
            {
                return;
            }

            if (this.voltageTargets.Count != 1)
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: supports a single {nameof(this.VoltageTargets)}");
            }

            if (!string.IsNullOrEmpty(this.MultiPassMasks))
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: does not support {nameof(this.MultiPassMasks)}");
            }
        }

        /// <summary>
        /// Verification of Recovery modes.
        /// </summary>
        protected internal void VerifyRecoveryMode()
        {
            if (this.MaxRepetitionCount > 1 && !(this.RecoveryMode == RecoveryModes.RecoveryLoop || this.RecoveryMode == RecoveryModes.RecoveryFailRetest))
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: use of {nameof(this.MaxRepetitionCount)} greater than 1 requires to set {RecoveryModes.RecoveryLoop} or {RecoveryModes.RecoveryFailRetest}.");
            }

            switch (this.RecoveryMode)
            {
                case RecoveryModes.Default:
                    this.RecoveryMode_ = new RecoveryDefaultMode(this.Console);
                    break;
                case RecoveryModes.RecoveryPort:
                    if (string.IsNullOrEmpty(this.PinMap))
                    {
                        throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: use of {nameof(RecoveryModes.RecoveryPort)} requires to set {nameof(this.PinMap)}.");
                    }

                    this.RecoveryMode_ = new RecoveryPortMode(this.Console);
                    break;
                case RecoveryModes.RecoveryLoop:
                    if (string.IsNullOrEmpty(this.PinMap))
                    {
                        throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: use of {nameof(RecoveryModes.RecoveryLoop)} requires to set {nameof(this.PinMap)}.");
                    }

                    this.RecoveryMode_ = new RecoveryLoopMode(this.Console);
                    break;

                case RecoveryModes.RecoveryFailRetest:
                    if (string.IsNullOrEmpty(this.PinMap))
                    {
                        throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: use of {nameof(RecoveryModes.RecoveryFailRetest)} requires to set {nameof(this.PinMap)}.");
                    }

                    if (this.Switches_.ForceRecoveryLoop)
                    {
                        throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: use of {nameof(RecoveryModes.RecoveryFailRetest)} does not support {nameof(this.Switches_.ForceRecoveryLoop)}.");
                    }

                    this.RecoveryMode_ = new RecoveryFailRetestMode(this.Console);
                    break;

                case RecoveryModes.NoRecovery:
                    this.RecoveryMode_ = new NoRecoveryMode(this.Console);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// /// Verification of MultiVmin Mode parameters.
        /// </summary>
        /// <exception cref="ArgumentException">Invalid configuration.</exception>
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
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: only supports multiple {nameof(this.VoltageTargets)}.");
            }

            if (!string.IsNullOrEmpty(this.CornerIdentifiers) && numberOfTargets != numberOfCorners)
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: number of {nameof(this.VoltageTargets)} must match number of {nameof(this.CornerIdentifiers)}.");
            }

            if ((this.ForwardingMode == ForwardingModes.Output || this.ForwardingMode == ForwardingModes.InputOutput) && string.IsNullOrEmpty(this.RecoveryTrackingOutgoing))
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: requires use of {nameof(this.RecoveryTrackingOutgoing)}.");
            }

            if ((this.ForwardingMode == ForwardingModes.Input || this.ForwardingMode == ForwardingModes.InputOutput) && string.IsNullOrEmpty(this.RecoveryTrackingIncoming))
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: requires use of {nameof(this.RecoveryTrackingIncoming)}.");
            }
        }

        /// <summary>
        /// Initializes voltages targets and initial mask using defaults.
        /// </summary>
        protected virtual void InitializeVoltageTargets()
        {
            this.voltageTargets = this.VoltageTargets.ToList();
            this.InitialSearchMask_ = new BitArray(this.voltageTargets.Count, false);
            if (!string.IsNullOrWhiteSpace(this.VminResult))
            {
                var count = this.VminResult.ToList().Count;
                if (count != 1 && count != this.voltageTargets.Count)
                {
                    throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: Number of {nameof(this.VminResult)} must be 1 or match number of {nameof(this.VoltageTargets)}.");
                }
            }
        }

        /// <summary>
        /// Evaluate comma-separated FeatureSwitchSettings.
        /// </summary>
        protected virtual void VerifyFeatureSwitchSettings()
        {
            this.Switches_ = new FeatureSwitches(this.FeatureSwitchSettings);
            if (this.Switches_.TraceCtv && string.IsNullOrWhiteSpace(this.CtvPins))
            {
                throw new Prime.Base.Exceptions.TestMethodException("Parameter=[CtvPins] must be specified when FeatureSwitchSettings=[trace_ctv_on].");
            }
        }

        /// <summary>
        /// Updates Prime Plist object.
        /// </summary>
        protected virtual void VerifyUpdatePlistObject()
        {
            this.plistObject = Prime.Services.PlistService.GetPlistObject(this.Patlist);
        }

        /// <summary>
        /// Modifies plist and capture settings options to run using ReturnOn = GlobalStickyError.
        /// </summary>
        protected virtual void SetReturnOnGlobalStickyError()
        {
            if (this.Switches_.ReturnOnGlobalStickyError)
            {
                if (this.FailCaptureCount == 1)
                {
                    this.FailCaptureCount = 9999;
                }

                var returnOnOption = string.Empty;
                try
                {
                    returnOnOption = this.plistObject.GetOption("ReturnOn");
                }
                catch
                {
                    // ignored.
                }

                if (string.IsNullOrEmpty(returnOnOption) || returnOnOption != "GlobalStickyError")
                {
                    this.plistObject.SetOption("ReturnOn", "GlobalStickyError");
                    this.plistObject.Resolve();
                }
            }
        }

        /// <summary>
        /// Sets PinMap_ reference.
        /// </summary>
        protected virtual void VerifyPinMap()
        {
            this.PinMap_ = string.IsNullOrEmpty(this.PinMap) ? null : DDG.PinMap.Service.Get(this.PinMap);
            var functionalTest = this.captureFailureTest as IFunctionalTest;
            this.PinMap_?.Verify(ref functionalTest);
        }

        /// <summary>
        /// Sets VminForwarding_ reference.
        /// </summary>
        /// <exception cref="ArgumentException">CornerIdentifiers is required.</exception>
        protected virtual void VerifyForwarding()
        {
            this.VminForwarding_ = null;
            this.flows = new List<int>();

            if (this.LogLevel == PrimeLogLevel.TEST_METHOD)
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Name}:\n --ForwardingMode:{this.ForwardingMode}");
            }

            if (this.ForwardingMode == ForwardingModes.None)
            {
                return;
            }

            if (this.LogLevel == PrimeLogLevel.TEST_METHOD)
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Name}: --CornerIdentifiers:{this.CornerIdentifiers}");
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
        /// Initializes CornerPatConfigSetPoints handles.
        /// </summary>
        /// <exception cref="ArgumentException">Key is not in shared storage.</exception>
        protected virtual void VerifyCornerSetPointsHandles()
        {
            this.CornerPatConfigSetPointsHandler_ = new CornerPatConfigSetPointsHandler(this.Patlist, this.CornerPatConfigSetPoints, this.VminForwarding_);
        }

        /// <summary>
        /// Sets DieRecoveryOutgoing_ reference.
        /// </summary>
        /// <exception cref="ArgumentException">RecoveryTracking and/or PinMap are required.</exception>
        protected virtual void VerifyDieRecovery()
        {
            this.DieRecoveryIncoming_ = null;
            this.DieRecoveryOutgoing_ = null;

            if (!string.IsNullOrEmpty(this.InitialMaskBits))
            {
                if (this.PinMap_ == null)
                {
                    throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Name}: use of {nameof(this.InitialMaskBits)} requires to specify a valid {nameof(this.PinMap)}.");
                }
            }

            if (string.IsNullOrEmpty(this.RecoveryOptions))
            {
                return;
            }

            var recoveryOptionsListRegx = new Regex(@"[01]+(,[01]+)*");
            if (!recoveryOptionsListRegx.IsMatch(this.RecoveryOptions.ToString()) && string.IsNullOrEmpty(this.RecoveryTrackingOutgoing))
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: use of {nameof(this.RecoveryOptions)} using DieRecovery requires to set {nameof(this.RecoveryTrackingOutgoing)}.");
            }
        }

        /// <summary>
        /// Creates DieRecoveryTrackers first time execute is run after Verify.
        /// </summary>
        protected virtual void SetDieRecoveryTrackers()
        {
            if (!string.IsNullOrEmpty(this.RecoveryTrackingOutgoing) && this.DieRecoveryOutgoing_ == null)
            {
                if (this.PinMap_ == null)
                {
                    throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Name}: use of {nameof(this.RecoveryTrackingOutgoing)} requires to specify a valid {nameof(this.PinMap)}.");
                }

                this.DieRecoveryOutgoing_ = DDG.DieRecovery.Service.Get(this.RecoveryTrackingOutgoing);
            }

            if (!string.IsNullOrEmpty(this.RecoveryTrackingIncoming) && this.DieRecoveryIncoming_ == null)
            {
                if (this.PinMap_ == null)
                {
                    throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Name}: use of {nameof(this.RecoveryTrackingIncoming)} requires to specify a valid {nameof(this.PinMap)}.");
                }

                this.DieRecoveryIncoming_ = DDG.DieRecovery.Service.Get(this.RecoveryTrackingIncoming);
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
        protected virtual void ExecutePostInstance()
        {
            if (this.postPlist != null)
            {
                using (var sw = Prime.Services.PerformanceService?.GetStopWatch(string.Empty))
                {
                    this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Name}: executing {nameof(this.PostPlist)}={this.PostPlist}.");
                    this.postPlist.Execute();
                }
            }
        }

        /// <summary>
        /// Processing vmin results. Updates VminResult and VminForwarding.
        /// </summary>
        protected virtual void ProcessVminResults()
        {
            var voltageResults = this.currentSearchResults.AggregateVoltages();
            this.UpdateVminResult(voltageResults);
            this.UpdateVminForwarding(voltageResults);
        }

        /// <summary>
        /// Updates Vmin Forwarding table using results and mode.
        /// </summary>
        /// <param name="voltageResults">Aggregated voltage results.</param>
        protected virtual void UpdateVminForwarding(List<double> voltageResults)
        {
            var failed = false;

            List<double> limitGuardbands = null;
            if (!string.IsNullOrEmpty(this.LimitGuardband))
            {
                limitGuardbands = this.ExpandVoltageKeys(this.LimitGuardband);
            }

            if (this.IsVminForwardingDisabled())
            {
                if (limitGuardbands != null)
                {
                    for (var i = 0; i < voltageResults.Count; i++)
                    {
                        failed |= this.FailedLimitGuardband(true, this.currentSearchResults.SearchResultData.Last().StartVoltages[i], voltageResults[i], limitGuardbands[i]);
                    }
                }
            }
            else
            {
                if (voltageResults.Count == 1 && this.VminForwarding_.Count > 1)
                {
                    voltageResults =
                        new List<double>(Enumerable.Repeat(voltageResults.ElementAt(0), this.VminForwarding_.Count));
                }

                for (var i = 0; i < voltageResults.Count; i++)
                {
                    var vminForwardingVoltage = this.VminForwarding_[i].Item3.GetStartingVoltage(VminUtilities.VoltageFailValue);
                    failed |= limitGuardbands != null && this.FailedLimitGuardband(DDG.VminForwarding.Service.IsSearchGuardbandEnabled(), vminForwardingVoltage, voltageResults[i], limitGuardbands[i]);
                    if (!this.Switches_.VminUpdateOnPassOnly || voltageResults[i] > 0.0)
                    {
                        failed |= !this.VminForwarding_[i].Item3.StoreVminResult(voltageResults[i].Equals(VminUtilities.VoltageFailValue, 3)
                            ? voltageResults[i]
                            : Math.Max(voltageResults[i], vminForwardingVoltage));
                    }
                }
            }

            if (failed && this.port == PassPort)
            {
                this.port = FailPort;
            }
        }

        /// <summary>
        /// Update DieRecovery tracking table using accumulated BitArray.
        /// </summary>
        protected virtual void UpdateDieRecoveryTracking()
        {
            this.failedUpdateDieRecoveryTracking = false;
            if (this.DieRecoveryOutgoing_ == null ||
                this.ForwardingMode == ForwardingModes.None ||
                this.ForwardingMode == ForwardingModes.Input ||
                this.currentSearchResults.TestResultsBits.Count <= 0)
            {
                return;
            }

            this.currentSearchResults.RulesResultsBits = this.ProcessCoreGroups(this.currentSearchResults.RulesResultsBits);
            this.failedUpdateDieRecoveryTracking = !this.RecoveryMode_.UpdateRecoveryTrackers(this.currentSearchResults, this.DieRecoveryOutgoing_, this.Switches_.RecoveryUpdateAlways);
        }

        private bool FailedLimitGuardband(bool enabled, double start, double result, double guardband)
        {
            if (enabled && start > 0 && (result.Equals(VminUtilities.VoltageFailValue, 3) || (result - start) > guardband))
            {
                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: Voltage Result=[{result}] - Start=[{start}] has exceeded LimitGuardband=[{guardband}].");
                return true;
            }

            return false;
        }

        private bool IsVminForwardingDisabled()
        {
            if (this.TestMode == TestModes.Functional ||
                this.TestMode == TestModes.Scoreboard ||
                string.IsNullOrEmpty(this.CornerIdentifiers) ||
                (this.port == FailRecoveryPort) ||
                (this.ForwardingMode != ForwardingModes.Output && this.ForwardingMode != ForwardingModes.InputOutput))
            {
                return true;
            }

            return false;
        }

        private void EvaluateAmbleFails(List<SearchResultData> searchResults)
        {
            this.currentSearchResults.FailedAmble = VminUtilities.FailedAmble(searchResults, this.plistObject);
        }

        private void CustomPrintToItuff(List<SearchResultData> searchResults)
        {
            if ((this.TestMode == TestModes.Functional || this.TestMode == TestModes.Scoreboard) && this.VminForwarding_ == null)
            {
                return;
            }

            if (this.Switches_.PrintIndependentSearchResults)
            {
                Prime.TestMethods.VminSearch.DataLogger.PrintResultsForAllSearches(searchResults, this.PatternNameMap, this.Switches_.PrintPerTargetIncrements);
            }
            else if (searchResults.Count != 0)
            {
                Prime.TestMethods.VminSearch.DataLogger.PrintMergedSearchResults(searchResults, this.PatternNameMap, this.Switches_.PrintPerTargetIncrements);
            }

            ExtendedDataLogger.LogVminConfiguration(this.VminForwarding_);
            this.PrintPerPatternVmin();
        }

        private BitArray ProcessCoreGroups(BitArray initial)
        {
            int size = 1;
            if (this.Switches_.DisablePairs)
            {
                size = 2;
            }

            if (this.Switches_.DisableQuadruplets)
            {
                size = 4;
            }

            if (size == 1)
            {
                return initial;
            }

            var result = new BitArray(initial);
            for (var i = 0; i < result.Length / size; i++)
            {
                if (result.Slice(i * size, size).Cast<bool>().Contains(true))
                {
                    for (int j = 0; j < size; j++)
                    {
                        result[(i * size) + j] = true;
                    }
                }
            }

            return result;
        }

        private void VerifyPostPlist()
        {
            this.postPlist = !string.IsNullOrEmpty(this.PostPlist) ? Prime.Services.FunctionalService.CreateNoCaptureTest(this.PostPlist, this.LevelsTc, this.TimingsTc, string.Empty) : null;
        }

        private void VerifyGetPlistContentsIndex()
        {
            this.plistContentsIndex = null;
            if (this.Switches_.IsPerPatternPrintingEnabled)
            {
                this.plistContentsIndex = PlistUtilities.GetPlistContentsIndex(this.Patlist);
            }
        }

        private void VerifyPerPatternPrinting()
        {
            if (string.IsNullOrEmpty(this.PatternNameMap) && this.Switches_.IsPerPatternPrintingEnabled)
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Module}.{MethodBase.GetCurrentMethod()?.Name}: use of per_pattern_printing feature requires to set {nameof(this.PatternNameMap)}");
            }
        }

        private BitArray GetMaskBitsFromVoltages(List<double> voltages, int maskSize)
        {
            var maskBits = new BitArray(voltages.Count, false);
            for (var i = 0; i < voltages.Count; i++)
            {
                if (voltages[i] < 0)
                {
                    maskBits.Set(i, true);
                }
            }

            if (voltages.Count != maskSize)
            {
                var result = new BitArray(this.PinMap_.VoltageDomainsToFailTracker(maskBits));
                return result;
            }

            return maskBits;
        }

        private BitArray CombineMask(BitArray maskBits)
        {
            BitArray mask;
            if (this.InitialSearchMask_.Count != maskBits.Count)
            {
                mask = new BitArray(this.PinMap_.VoltageDomainsToFailTracker(maskBits));
                mask.Or(this.InitialSearchMask_);
            }
            else
            {
                mask = new BitArray(maskBits);
                mask.Or(this.InitialSearchMask_);
            }

            return mask;
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
        }

        private List<IFailureData> UpdatePatternFailTable(bool plistExecuteResult, ICaptureFailureTest plist)
        {
            if (!this.Switches_.IsPerPatternPrintingEnabled)
            {
                return null;
            }

            List<IFailureData> fails = null;

            var findStartPattern = this.plistContentsIndex.FindIndex(o => o.Equals(this.startPattern));
            if (findStartPattern < 0)
            {
                throw new Exception($"Unable to find start pattern=[{this.startPattern.PatternName}] in patlist=[{this.Patlist}].");
            }

            if (plistExecuteResult)
            {
                for (var i = findStartPattern; i < this.plistContentsIndex.Count; i++)
                {
                    if (this.perPatternVoltage.ContainsKey(this.plistContentsIndex[i]))
                    {
                        this.Console?.PrintDebug($"--ignoring burst=[{this.plistContentsIndex[i].Burst}] pattern=[{this.plistContentsIndex[i].PatternName}] voltage=[{string.Join(",", this.VoltageValues.ToArray())}] since same pattern entry was added to per-pattern voltage table.");
                        continue;
                    }

                    this.perPatternVoltage[this.plistContentsIndex[i]] = new List<double>(this.VoltageValues);
                    this.Console?.PrintDebug($"--adding burst=[{this.plistContentsIndex[i].Burst}] pattern=[{this.plistContentsIndex[i].PatternName}] voltage=[{string.Join(",", this.VoltageValues.ToArray())}] to per-pattern voltage table.");
                }
            }
            else
            {
                fails = plist.GetPerCycleFailures();
                var cycleData = fails.First();
                var firstFail = new PatternOccurrence(cycleData.GetBurstIndex(), cycleData.GetPatternName(), cycleData.GetPatternInstanceId(), ulong.MinValue);
                var findFirstFail = this.plistContentsIndex.FindIndex(o => o.Equals(firstFail));
                if (findFirstFail < 0)
                {
                    throw new Exception($"Unable to find failing pattern=[{firstFail.PatternName}] burst=[{firstFail.Burst}] occurrence=[{firstFail.Occurrence}] in patlist=[{this.Patlist}].");
                }

                for (var i = findStartPattern; i < findFirstFail; i++)
                {
                    if (this.perPatternVoltage.ContainsKey(this.plistContentsIndex[i]))
                    {
                        this.Console?.PrintDebug($"--ignoring burst=[{this.plistContentsIndex[i].Burst}] pattern=[{this.plistContentsIndex[i].PatternName}] voltage=[{string.Join(",", this.VoltageValues.ToArray())}] since same pattern entry was added to per-pattern voltage table.");
                        continue;
                    }

                    this.perPatternVoltage[this.plistContentsIndex[i]] = new List<double>(this.VoltageValues);
                    this.Console?.PrintDebug($"--adding burst=[{this.plistContentsIndex[i].Burst}] pattern=[{this.plistContentsIndex[i].PatternName}] voltage=[{string.Join(",", this.VoltageValues.ToArray())}] to per-pattern voltage table.");
                }
            }

            return fails;
        }

        private void PrintFailData(ICaptureFailureTest plist, List<IFailureData> fails)
        {
            if (fails == null)
            {
                fails = plist.GetPerCycleFailures();
            }

            foreach (var fail in fails)
            {
                string previousLabel;
                try
                {
                    previousLabel = fail.GetPreviousLabel();
                }
                catch
                {
                    previousLabel = "<no_label_found>";
                }

                this.Console?.PrintDebug($"{MethodBase.GetCurrentMethod()?.Name} --Fail data: " +
                                                   $" Plist={fail.GetParentPlistName()}" +
                                                   $" Pattern={fail.GetPatternName()}" +
                                                   $" Label={previousLabel}" +
                                                   $" PinNames={string.Join(",", fail.GetFailingPinNames())}" +
                                                   $" Channels={string.Join(",", fail.GetFailingPinChannels().ConvertAll(Convert.ToString))}" +
                                                   $" Vector={Convert.ToString(fail.GetVectorAddress())}.");
            }
        }

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

            return voltageKeys.Select(o => o.EvaluateExpression()).ToList();
        }

        private void PrintPerPatternVmin()
        {
            if (!this.Switches_.IsPerPatternPrintingEnabled || this.perPatternVoltage.Count <= 0)
            {
                return;
            }

            var perPatternVmin = string.Empty;
            foreach (var pattern in this.perPatternVoltage)
            {
                if (this.plistObject.IsPatternAnAmble(pattern.Key.PatternName))
                {
                    continue;
                }

                var id = ExtendedDataLogger.GetMappedString(pattern.Key.PatternName, this.PatternNameMap);
                var result = string.Join("_", pattern.Value);
                perPatternVmin += $"{id}:{result}|";
            }

            if (string.IsNullOrEmpty(perPatternVmin))
            {
                return;
            }

            perPatternVmin = perPatternVmin.Remove(perPatternVmin.Length - 1);
            var writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            writer.SetTnamePostfix("_pp");
            writer.SetDelimiterCharacterForWrap('^');
            writer.SetData(perPatternVmin);
            Prime.Services.DatalogService.WriteToItuff(writer);
            Prime.Services.SharedStorageService.InsertRowAtTable(this.InstanceName + "_pp", perPatternVmin, Context.DUT);
        }

        private void StoreStartPatternForPerPatternVmin()
        {
            if (!this.Switches_.IsPerPatternPrintingEnabled || !this.captureFailureTest.HasStartPattern())
            {
                return;
            }

            var tuples = this.captureFailureTest.GetStartPattern();
            this.startPattern = new PatternOccurrence(tuples.Item2, tuples.Item1, tuples.Item3, 0);
        }
    }
}