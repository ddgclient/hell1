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

namespace VminTC.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using DDG;
    using DieRecoveryBase;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.DffService;
    using Prime.FunctionalService;
    using Prime.PlistService;
    using Prime.SharedStorageService;
    using Prime.TestMethods.VminSearch;
    using Prime.TestProgramService;

    /// <summary>
    /// MultiVminTc unit tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MultiVmin_UnitTest : VminTC
    {
        private Mock<IPinMap> pinMapMock;
        private Mock<IDieRecovery> dieRecoveryMock;
        private Mock<IVminForwardingCorner> vminForwardingCornerMock;
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<ITestProgramService> testProgramServiceMock;
        private Mock<IDieRecoveryFactory> dieRecoveryFactoryMock;
        private Mock<IDatalogService> datalogServiceMock;
        private Mock<IStrgvalFormat> strgvalFormatMock;
        private Mock<IVminForwardingFactory> vminForwardingFactoryMock;

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.FeatureSwitchSettings = string.Empty;
            this.PinMap = string.Empty;
            this.CornerIdentifiers = string.Empty;
            this.InitialMaskBits = string.Empty;
            this.Patlist = string.Empty;
            this.LevelsTc = "SomeLevels";
            this.VoltageTargets = "PinName";
            this.TestMode = TestModes.MultiVmin;

            this.vminForwardingCornerMock = new Mock<IVminForwardingCorner>(MockBehavior.Default);
            this.vminForwardingFactoryMock = new Mock<IVminForwardingFactory>(MockBehavior.Strict);
            this.vminForwardingFactoryMock.Setup(f => f.Get(It.IsAny<string>(), It.IsAny<int>())).Returns(this.vminForwardingCornerMock.Object);
            this.vminForwardingFactoryMock.Setup(f => f.IsSinglePointMode()).Returns(false);
            this.pinMapMock = new Mock<IPinMap>(MockBehavior.Strict);
            this.pinMapMock.Setup(o => o.Verify(ref It.Ref<IFunctionalTest>.IsAny));
            var pinMapFactoryMock = new Mock<IPinMapFactory>(MockBehavior.Strict);
            pinMapFactoryMock.Setup(p => p.Get(It.IsAny<string>())).Returns(this.pinMapMock.Object);
            this.dieRecoveryMock = new Mock<IDieRecovery>(MockBehavior.Strict);
            this.dieRecoveryFactoryMock = new Mock<IDieRecoveryFactory>(MockBehavior.Strict);
            this.dieRecoveryFactoryMock.Setup(d => d.Get(It.IsAny<string>())).Returns(this.dieRecoveryMock.Object);

            DDG.VminForwarding.Service = this.vminForwardingFactoryMock.Object;
            DDG.PinMap.Service = pinMapFactoryMock.Object;
            DDG.DieRecovery.Service = this.dieRecoveryFactoryMock.Object;

            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(
                    o => o.PrintDebug(It.IsAny<string>())).
                Callback((string msg) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            this.consoleServiceMock.Setup(
                    o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string member, string path) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;
            this.testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("FlowIndex")).Returns("1");
            Prime.Services.TestProgramService = this.testProgramServiceMock.Object;

            this.datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            this.strgvalFormatMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            this.datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.strgvalFormatMock.Object);
            Prime.Services.DatalogService = this.datalogServiceMock.Object;

            this.VerifyDts();
        }

        /// <summary>
        /// Direct call to DieRecoveryOutgoing_ PinMap_.
        /// </summary>
        [TestMethod]
        public void ProcessPlistResults_Pass()
        {
            this.VoltageTargets = "1,2,3";
            this.PinMap = "P1,P2,P3";
            this.TestMode = TestModes.MultiVmin;
            var plistMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var expected = new BitArray(new[] { true, false, true });
            this.pinMapMock.Setup(p => p.DecodeFailure(It.IsAny<IFunctionalTest>(), null))
                .Returns(expected);
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.PinMap_ = this.pinMapMock.Object;

            var temp = (IVminSearchExtensions)this;
            var result = temp.ProcessPlistResults(false, plistMock.Object);
            Assert.IsTrue(expected.Xor(result).OfType<bool>().All(e => !e));
            this.pinMapMock.VerifyAll();
        }

        /// <summary>
        /// MaskPins passing.
        /// </summary>
        [TestMethod]
        public void ApplyMask_Pass()
        {
            this.InitialMaskBits = "1000";
            this.VoltageTargets = "1,2,3,4";
            this.PinMap = "P0,P1,P2,P3";
            this.StartVoltages = "0.5";

            var maskedPins = new List<string> { "NOA0", "NOA2" };
            var plistMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            var maskBits = "1010".ToBitArray();
            this.pinMapMock.Setup(o => o.MaskPins(new BitArray(new[] { true, false, true, false }), ref It.Ref<IFunctionalTest>.IsAny, new List<string>()));
            this.pinMapMock.Setup(o => o.ModifyPlist(new BitArray(new[] { true, false, true, false }), ref It.Ref<IFunctionalTest>.IsAny));
            this.PinMap_ = this.pinMapMock.Object;
            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetIncomingMask();
            this.SetInitialMask();

            var temp = (IVminSearchExtensions)this;
            temp.ApplyMask(maskBits, plistMock.Object);
            this.pinMapMock.VerifyAll();
            this.dieRecoveryMock.VerifyAll();
            plistMock.VerifyAll();
        }

        /// <summary>
        /// Simple case.
        /// </summary>
        [TestMethod]
        public void VerifyMode_TwoDomains_Pass()
        {
            this.CornerIdentifiers = "Corner1,Corner2";
            this.VoltageTargets = "Core0,Core1";
            this.PinMap = "Core0Map,Core1Map";

            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            this.PinMap_ = this.pinMapMock.Object;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.dieRecoveryMock.VerifyAll();
            this.pinMapMock.VerifyAll();
        }

        /// <summary>
        /// Invalid DieRecovery.
        /// </summary>
        [TestMethod]
        public void VerifyMode_MissingRecoveryTrackingOutgoing_Fail()
        {
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.CornerIdentifiers = "Corner1,Corner2";
            this.VoltageTargets = "Core0,Core1";
            this.PinMap = "Core0Map,Core1Map";
            this.RecoveryTrackingIncoming = "T0,T1";
            this.RecoveryTrackingOutgoing = string.Empty;
            this.TestMode = TestModes.MultiVmin;
            this.PinMap_ = this.pinMapMock.Object;

            var result = Assert.ThrowsException<ArgumentException>(this.CustomVerify);
            Assert.AreEqual("VminTC.dll.VerifyMultiVminMode: requires use of RecoveryTrackingOutgoing.", result.Message);
        }

        /// <summary>
        /// Invalid DieRecovery.
        /// </summary>
        [TestMethod]
        public void VerifyMode_MissingRecoveryTrackingIncoming_Fail()
        {
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.CornerIdentifiers = "Corner1,Corner2";
            this.VoltageTargets = "Core0,Core1";
            this.PinMap = "Core0Map,Core1Map";
            this.RecoveryTrackingIncoming = string.Empty;
            this.RecoveryTrackingOutgoing = "T0,T1";
            this.TestMode = TestModes.MultiVmin;
            this.PinMap_ = this.pinMapMock.Object;

            var result = Assert.ThrowsException<ArgumentException>(this.CustomVerify);
            Assert.AreEqual("VminTC.dll.VerifyMultiVminMode: requires use of RecoveryTrackingIncoming.", result.Message);
        }

        /// <summary>
        /// Number of Corner Identifiers is not matching number of voltage targets.
        /// </summary>
        [TestMethod]
        public void VerifyMode_CornersNotMatchingTargets_Fail()
        {
            this.CornerIdentifiers = "Corner1";
            this.VoltageTargets = "Core0,Core1";
            this.PinMap = "Core0Map,Core1Map";
            this.TestMode = TestModes.MultiVmin;

            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            this.PinMap_ = this.pinMapMock.Object;

            var result = Assert.ThrowsException<ArgumentException>(this.CustomVerify);
            Assert.AreEqual("VminTC.dll.VerifyMultiVminMode: number of VoltageTargets must match number of CornerIdentifiers.", result.Message);
        }

        /// <summary>
        /// MultiVminTc does not support single domain.
        /// </summary>
        [TestMethod]
        public void VerifyMode_SingleDomain_Fail()
        {
            this.CornerIdentifiers = "Corner1";
            this.VoltageTargets = "Core0";
            this.PinMap = "Core0Map";
            this.TestMode = TestModes.MultiVmin;

            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            this.PinMap_ = this.pinMapMock.Object;

            var result = Assert.ThrowsException<ArgumentException>(this.CustomVerify);
            Assert.AreEqual("VminTC.dll.VerifyMultiVminMode: only supports multiple VoltageTargets.", result.Message);
        }

        /// <summary>
        /// Skip Vmin and DieRecovery updates.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_ForwardingInput_Pass()
        {
            this.ForwardingMode = ForwardingModes.Input;
            this.StartVoltages = "0.5,0.5,0.5,0.5";
            this.EndVoltageLimits = "1.5,1.5,1.5,1.5";
            this.VoltageTargets = "T0,T1,T2,T3";
            this.PinMap = "P0,P1,P2,P3";
            this.RecoveryTrackingIncoming = "R0,R1,R2,R3";
            this.RecoveryTrackingOutgoing = "R0,R1,R2,R3";
            this.TestMode = TestModes.MultiVmin;
            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, false));
            this.pinMapMock.Setup(o => o.Restore());
            this.PinMap_ = this.pinMapMock.Object;
            this.vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            var voltages = new List<double> { 0.5, 0.6, 0.7, 0.8 };
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());
            this.SetDieRecoveryTrackers();
            this.SetIncomingMask();
            var temp = (IVminSearchExtensions)this;
            var searchPointData = new SearchPointData(voltages, new SearchPointData.PatternData("p1", 1, 1));
            var searchStateValues = new SearchStateValues()
            {
                Voltages = voltages,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(4, false),
                PerTargetIncrements = new List<uint> { 0, 1, 2, 3 },
                PerPointData = new List<SearchPointData> { searchPointData },
            };
            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchResultData = new SearchResultData(searchStateValues, true, searchIdentifiers);
            var searchPoints = new List<SearchResultData> { searchResultData };
            var repeat = temp.HasToRepeatSearch(searchPoints);
            Assert.IsFalse(repeat);
            var port = temp.PostProcessSearchResults(searchPoints);
            Assert.AreEqual(1, port);
            this.strgvalFormatMock.VerifyAll();
        }

        /// <summary>
        /// Passing search in Output mode.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_ForwardingOutput_Pass()
        {
            this.ForwardingMode = ForwardingModes.Output;
            this.VoltageTargets = "C0,C1,C2,C3";
            this.CornerIdentifiers = "C0F1,C1F1,C2F1,C3F1";
            this.PinMap = "C0_MAP,C1_MAP,C2_MAP,C3_MAP";
            this.StartVoltages = "0.5,0.5,0.5,0.5";
            this.EndVoltageLimits = "1.5,1.5,1.5,1.5";
            this.TestMode = TestModes.MultiVmin;
            this.RecoveryTrackingOutgoing = "T0,T1,T2,T3";

            var voltages = new List<double> { 0.5, 0.6, 0.7, 0.8 };
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());
            var searchPointData = new SearchPointData(voltages, new SearchPointData.PatternData("p1", 1, 1));
            var searchStateValues = new SearchStateValues()
            {
                Voltages = voltages,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(4, false),
                PerTargetIncrements = new List<uint> { 0, 1, 2, 3 },
                PerPointData = new List<SearchPointData> { searchPointData },
            };
            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchResultData = new SearchResultData(searchStateValues, true, searchIdentifiers);
            var searchPoints = new List<SearchResultData> { searchResultData };
            this.dieRecoveryMock.Setup(d => d.UpdateTrackingStructure(new BitArray(4, false), new BitArray(4, false), new BitArray(4, false), UpdateMode.Merge, true)).Returns(true);
            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            var decoder = new Mock<IPinMapDecoder>(MockBehavior.Strict);
            decoder.Setup(o => o.NumberOfTrackerElements).Returns(4);
            this.pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { decoder.Object });
            this.pinMapMock.Setup(o => o.Restore());
            this.PinMap_ = this.pinMapMock.Object;
            var vminMock = new Mock<IVminForwardingCorner>(MockBehavior.Strict);
            this.vminForwardingCornerMock.Setup(v => v.StoreVminResult(It.IsAny<double>())).Returns(true);
            this.vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);
            this.strgvalFormatMock.Setup(o => o.SetData("C0F1:1:1.500_C1F1:1:1.500_C2F1:1:1.500_C3F1:1:1.500"));
            this.strgvalFormatMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgvalFormatMock.Object));

            var temp = (IVminSearchExtensions)this;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetDieRecoveryTrackers();
            this.SetIncomingMask();
            var repeat = temp.HasToRepeatSearch(searchPoints);
            Assert.IsFalse(repeat);
            var port = temp.PostProcessSearchResults(searchPoints);
            Assert.AreEqual(1, port);
            this.dieRecoveryMock.VerifyAll();
            this.pinMapMock.VerifyAll();
            vminMock.VerifyAll();
            this.strgvalFormatMock.VerifyAll();
        }

        /// <summary>
        /// Passing search in Output mode.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_LimitGuardbandNoForwarding_Fail()
        {
            this.ForwardingMode = ForwardingModes.None;
            this.VoltageTargets = "C0,C1,C2,C3";
            this.PinMap = "C0_MAP,C1_MAP,C2_MAP,C3_MAP";
            this.StartVoltages = "0.5,0.5,0.5,0.5";
            this.EndVoltageLimits = "1.5,1.5,1.5,1.5";
            this.TestMode = TestModes.MultiVmin;
            this.LimitGuardband = "0.1";

            var voltages = new List<double> { 0.5, 0.6, 0.7, 0.8 };
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());
            var searchPointData = new SearchPointData(voltages, new SearchPointData.PatternData("p1", 1, 1));
            var searchStateValues = new SearchStateValues()
            {
                Voltages = voltages,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(4, false),
                PerTargetIncrements = new List<uint> { 0, 1, 2, 3 },
                PerPointData = new List<SearchPointData> { searchPointData },
            };
            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchResultData = new SearchResultData(searchStateValues, true, searchIdentifiers);
            var searchPoints = new List<SearchResultData> { searchResultData };
            var decoder = new Mock<IPinMapDecoder>(MockBehavior.Strict);
            decoder.Setup(o => o.NumberOfTrackerElements).Returns(4);
            this.pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { decoder.Object });
            this.pinMapMock.Setup(o => o.Restore());
            this.PinMap_ = this.pinMapMock.Object;
            var vminMock = new Mock<IVminForwardingCorner>(MockBehavior.Strict);

            var temp = (IVminSearchExtensions)this;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetDieRecoveryTrackers();
            this.SetIncomingMask();
            var repeat = temp.HasToRepeatSearch(searchPoints);
            Assert.IsFalse(repeat);
            var port = temp.PostProcessSearchResults(searchPoints);
            Assert.AreEqual(0, port);
            this.dieRecoveryMock.VerifyAll();
            this.pinMapMock.VerifyAll();
            vminMock.VerifyAll();
            this.strgvalFormatMock.VerifyAll();
        }

        /// <summary>
        /// Passing search in Output mode.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_FailTrackingUpdate_Fail()
        {
            this.ForwardingMode = ForwardingModes.Output;
            this.VoltageTargets = "C0,C1,C2,C3";
            this.CornerIdentifiers = "C0F1,C1F1,C2F1,C3F1";
            this.PinMap = "C0_MAP,C1_MAP,C2_MAP,C3_MAP";
            this.StartVoltages = "0.5,0.5,0.5,0.5";
            this.EndVoltageLimits = "1.5,1.5,1.5,1.5";
            this.TestMode = TestModes.MultiVmin;
            this.RecoveryTrackingOutgoing = "T0,T1,T2,T3";

            var voltages = new List<double> { 0.5, 0.6, 0.7, 0.8 };
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());
            var searchPointData = new SearchPointData(voltages, new SearchPointData.PatternData("p1", 1, 1));
            var searchStateValues = new SearchStateValues()
            {
                Voltages = voltages,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(4, false),
                PerTargetIncrements = new List<uint> { 0, 1, 2, 3 },
                PerPointData = new List<SearchPointData> { searchPointData },
            };
            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchResultData = new SearchResultData(searchStateValues, true, searchIdentifiers);
            var searchPoints = new List<SearchResultData> { searchResultData };
            this.dieRecoveryMock.Setup(d => d.UpdateTrackingStructure(new BitArray(4, false), new BitArray(4, false), new BitArray(4, false), UpdateMode.Merge, true)).Returns(false);
            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            var decoder = new Mock<IPinMapDecoder>(MockBehavior.Strict);
            decoder.Setup(o => o.NumberOfTrackerElements).Returns(4);
            this.pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { decoder.Object });
            this.pinMapMock.Setup(o => o.Restore());
            this.PinMap_ = this.pinMapMock.Object;
            var vminMock = new Mock<IVminForwardingCorner>(MockBehavior.Strict);
            this.vminForwardingCornerMock.Setup(v => v.StoreVminResult(It.IsAny<double>())).Returns(true);
            this.vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);
            this.strgvalFormatMock.Setup(o => o.SetData("C0F1:1:1.500_C1F1:1:1.500_C2F1:1:1.500_C3F1:1:1.500"));
            this.strgvalFormatMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgvalFormatMock.Object));

            var temp = (IVminSearchExtensions)this;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetDieRecoveryTrackers();
            this.SetIncomingMask();
            var repeat = temp.HasToRepeatSearch(searchPoints);
            Assert.IsFalse(repeat);
            var port = temp.PostProcessSearchResults(searchPoints);
            Assert.AreEqual(0, port);
            this.dieRecoveryMock.VerifyAll();
            this.pinMapMock.VerifyAll();
            vminMock.VerifyAll();
            this.strgvalFormatMock.VerifyAll();
        }

        /// <summary>
        /// Passing search in Merge mode.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_ForwardingInputOutput_Pass()
        {
            this.VminResult = "C0Result,C1Result,C2Result,C3Result";
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.VoltageTargets = "C0,C1,C2,C3";
            this.CornerIdentifiers = "C0F1,C1F1,C2F1,C3F1";
            this.PinMap = "C0_MAP,C1_MAP,C2_MAP,C3_MAP";
            this.StartVoltages = "0.5,0.5,0.5,0.5";
            this.EndVoltageLimits = "1.5,1.5,1.5,1.5";
            this.TestMode = TestModes.MultiVmin;
            this.RecoveryTrackingIncoming = "T0,T1,T2,T3";
            this.RecoveryTrackingOutgoing = "P0,P1,P2,P3";
            this.TestMode = TestModes.MultiVmin;

            var voltages = new List<double> { 0.5, 0.6, 0.7, 0.8 };
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());
            var searchPointData = new SearchPointData(voltages, new SearchPointData.PatternData("p1", 1, 1));
            var searchStateValues = new SearchStateValues()
            {
                Voltages = voltages,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(4, false),
                PerTargetIncrements = new List<uint> { 0, 1, 2, 3 },
                PerPointData = new List<SearchPointData> { searchPointData },
            };
            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchResultData = new SearchResultData(searchStateValues, true, searchIdentifiers);
            var searchPoints = new List<SearchResultData> { searchResultData };
            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, false));
            this.dieRecoveryMock.Setup(d => d.UpdateTrackingStructure(new BitArray(4, false), new BitArray(4, false), new BitArray(4, false), UpdateMode.Merge, true)).Returns(true);
            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            this.pinMapMock.Setup(o => o.Restore());
            this.PinMap_ = this.pinMapMock.Object;
            this.vminForwardingCornerMock.Setup(v => v.GetStartingVoltage(-9999)).Returns(0.6);
            this.vminForwardingCornerMock.Setup(v => v.StoreVminResult(It.IsAny<double>())).Returns(true);

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(s => s.InsertRowAtTable("C0Result", voltages[0], Context.DUT));
            sharedStorageServiceMock.Setup(s => s.InsertRowAtTable("C1Result", voltages[1], Context.DUT));
            sharedStorageServiceMock.Setup(s => s.InsertRowAtTable("C2Result", voltages[2], Context.DUT));
            sharedStorageServiceMock.Setup(s => s.InsertRowAtTable("C3Result", voltages[3], Context.DUT));
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            this.vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);
            this.strgvalFormatMock.Setup(o => o.SetData("C0F1:1:1.500_C1F1:1:1.500_C2F1:1:1.500_C3F1:1:1.500"));
            this.strgvalFormatMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgvalFormatMock.Object));

            var temp = (IVminSearchExtensions)this;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetDieRecoveryTrackers();
            this.SetIncomingMask();
            var repeat = temp.HasToRepeatSearch(searchPoints);
            Assert.IsFalse(repeat);
            var port = temp.PostProcessSearchResults(searchPoints);
            Assert.AreEqual(1, port);
            this.dieRecoveryMock.VerifyAll();
            this.pinMapMock.VerifyAll();
            this.vminForwardingCornerMock.VerifyAll();
            sharedStorageServiceMock.VerifyAll();
            this.strgvalFormatMock.VerifyAll();
        }

        /// <summary>
        /// Passing search but failing recovery options.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_FailRecoveryOptions_Port2()
        {
            this.VminResult = "D.TOKEN";
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.VoltageTargets = "C0,C1,C2,C3";
            this.CornerIdentifiers = "C0F1,C1F1,C2F1,C3F1";
            this.PinMap = "C0_MAP,C1_MAP,C2_MAP,C3_MAP";
            this.RecoveryOptions = "0000";
            this.StartVoltages = "0.5,0.5,0.5,0.5";
            this.EndVoltageLimits = "1.5,1.5,1.5,1.5";
            this.RecoveryTrackingIncoming = "C0,C1,C2,C3";
            this.RecoveryTrackingOutgoing = "C0,C1,C2,C3";
            this.TestMode = TestModes.MultiVmin;

            var voltages = new List<double> { 0.5, 0.6, 0.7, -9999 };
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());

            var searchPointData = new SearchPointData(voltages, new SearchPointData.PatternData("p1", 1, 1));
            var searchStateValues = new SearchStateValues()
            {
                Voltages = voltages,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(4, false),
                PerTargetIncrements = new List<uint> { 0, 1, 2, 3 },
                PerPointData = new List<SearchPointData> { searchPointData },
            };
            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchResultData = new SearchResultData(searchStateValues, true, searchIdentifiers);
            var searchPoints = new List<SearchResultData> { searchResultData };
            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, false));
            this.dieRecoveryMock.Setup(o => o.LogTrackingStructure(new BitArray(new[] { false, false, false, false }), new BitArray(new[] { false, false, false, true })));
            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            this.pinMapMock.Setup(o => o.Restore());
            this.PinMap_ = this.pinMapMock.Object;
            this.vminForwardingCornerMock.Setup(v => v.GetStartingVoltage(-9999)).Returns(0.6);
            this.vminForwardingCornerMock.Setup(v => v.StoreVminResult(0.6));
            this.vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);
            this.strgvalFormatMock.Setup(o => o.SetData("C0F1:1:1.500_C1F1:1:1.500_C2F1:1:1.500_C3F1:1:1.500"));
            this.strgvalFormatMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgvalFormatMock.Object));
            var dffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            Prime.Services.DffService = dffServiceMock.Object;
            dffServiceMock.Setup(o => o.SetDff("TOKEN", "0.500v0.600v0.700v-9999"));
            var temp = (IVminSearchExtensions)this;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            temp.GetBypassPort();
            temp.ApplyPreExecuteSetup(this.Patlist);
            var repeat = temp.HasToRepeatSearch(searchPoints);
            Assert.IsFalse(repeat);
            var port = temp.PostProcessSearchResults(searchPoints);
            Assert.AreEqual(2, port);
            this.dieRecoveryMock.VerifyAll();
            this.pinMapMock.VerifyAll();
            this.vminForwardingCornerMock.VerifyAll();
            this.strgvalFormatMock.VerifyAll();
        }

        /// <summary>
        /// Failing search. Should update Vmin Forwarding.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_FailSearch_Pass()
        {
            this.ForwardingMode = ForwardingModes.InputOutput;
            this.VoltageTargets = "C0,C1,C2,C3";
            this.CornerIdentifiers = "C0F1,C1F1,C2F1,C3F1";
            this.PinMap = "C0_MAP,C1_MAP,C2_MAP,C3_MAP";
            this.RecoveryOptions = "0000";
            this.StartVoltages = "0.5,0.5,0.5,0.5";
            this.EndVoltageLimits = "1.5,1.5,1.5,1.5";
            this.RecoveryTrackingIncoming = "C0,C1,C2,C3";
            this.RecoveryTrackingOutgoing = "C0,C1,C2,C3";
            this.TestMode = TestModes.MultiVmin;
            this.Patlist = "SomePatlist";
            this.LevelsTc = "SomeLevels";
            this.TimingsTc = "SomeTimings";
            this.PrePlist = string.Empty;

            var voltages = new List<double> { 0.5, 0.6, 0.7, -9999 };
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());
            var searchPointData = new SearchPointData(voltages, new SearchPointData.PatternData("p1", 1, 1));
            var searchStateValues = new SearchStateValues()
            {
                Voltages = voltages,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(4, false),
                PerTargetIncrements = new List<uint> { 0, 1, 2, 3 },
                PerPointData = new List<SearchPointData> { searchPointData },
            };
            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchResultData = new SearchResultData(searchStateValues, false, searchIdentifiers);
            var searchPoints = new List<SearchResultData> { searchResultData };
            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, false));
            this.dieRecoveryMock.Setup(o => o.LogTrackingStructure(new BitArray(new[] { false, false, false, false }), new BitArray(new[] { false, false, false, true })));
            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            this.pinMapMock.Setup(o => o.Restore());
            this.PinMap_ = this.pinMapMock.Object;
            this.vminForwardingCornerMock.Setup(v => v.GetStartingVoltage(-9999)).Returns(0.6);
            this.vminForwardingCornerMock.Setup(v => v.StoreVminResult(0.6)); // coming from forwarding table.
            this.vminForwardingCornerMock.Setup(v => v.StoreVminResult(voltages[1]));
            this.vminForwardingCornerMock.Setup(v => v.StoreVminResult(voltages[2]));
            this.vminForwardingCornerMock.Setup(v => v.StoreVminResult(voltages[3]));
            var tuple0 = new Tuple<string, int, IVminForwardingCorner>("C0", 1, this.vminForwardingCornerMock.Object);
            var tuple1 = new Tuple<string, int, IVminForwardingCorner>("C1", 1, this.vminForwardingCornerMock.Object);
            var tuple2 = new Tuple<string, int, IVminForwardingCorner>("C2", 1, this.vminForwardingCornerMock.Object);
            var tuple3 = new Tuple<string, int, IVminForwardingCorner>("C3", 1, this.vminForwardingCornerMock.Object);
            this.VminForwarding_ = new List<Tuple<string, int, IVminForwardingCorner>> { tuple0, tuple1, tuple2, tuple3 };

            this.vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);
            this.strgvalFormatMock.Setup(o => o.SetData("C0F1:1:1.500_C1F1:1:1.500_C2F1:1:1.500_C3F1:1:1.500"));
            this.strgvalFormatMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgvalFormatMock.Object));

            var plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            var plistObjectMock = new Mock<IPlistObject>(MockBehavior.Strict);
            plistObjectMock.Setup(o => o.IsPatternAnAmble(It.IsAny<string>())).Returns(false);
            plistServiceMock.Setup(o => o.GetPlistObject(this.Patlist)).Returns(plistObjectMock.Object);
            Prime.Services.PlistService = plistServiceMock.Object;
            var functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            var captureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            captureTestMock.Setup(o => o.EnableStartPatternOnFirstFail());
            functionalServiceMock
                .Setup(o => o.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 1, string.Empty))
                .Returns(captureTestMock.Object);
            Prime.Services.FunctionalService = functionalServiceMock.Object;

            var temp = (IVminSearchExtensions)this;
            temp.GetFunctionalTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.PrePlist);
            this.CustomVerify();
            this.SetDieRecoveryTrackers();
            this.SetIncomingMask();
            var repeat = temp.HasToRepeatSearch(searchPoints);
            Assert.IsFalse(repeat);
            var port = temp.PostProcessSearchResults(searchPoints);
            Assert.AreEqual(0, port);
            this.dieRecoveryMock.VerifyAll();
            this.pinMapMock.VerifyAll();
            this.vminForwardingCornerMock.VerifyAll();
            this.strgvalFormatMock.VerifyAll();
        }

        /// <summary>
        /// Failing search. Input should not update table.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_Input_Pass()
        {
            this.ForwardingMode = ForwardingModes.Input;
            this.VoltageTargets = "C0,C1,C2,C3";
            this.CornerIdentifiers = "C0F1,C1F1,C2F1,C3F1";
            this.PinMap = "C0_MAP,C1_MAP,C2_MAP,C3_MAP";
            this.RecoveryOptions = "0000";
            this.StartVoltages = "0.5,0.5,0.5,0.5";
            this.EndVoltageLimits = "1.5,1.5,1.5,1.5";
            this.RecoveryTrackingIncoming = "C0,C1,C2,C3";
            this.RecoveryTrackingOutgoing = "C0,C1,C2,C3";
            this.TestMode = TestModes.MultiVmin;

            var voltages = new List<double> { 0.5, 0.6, 0.7, 0.8 };
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());
            var searchPointData = new SearchPointData(voltages, new SearchPointData.PatternData("p1", 1, 1));
            var searchStateValues = new SearchStateValues()
            {
                Voltages = voltages,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(4, false),
                PerTargetIncrements = new List<uint> { 0, 1, 2, 3 },
                PerPointData = new List<SearchPointData> { searchPointData },
            };
            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchResultData = new SearchResultData(searchStateValues, true, searchIdentifiers);
            var searchPoints = new List<SearchResultData> { searchResultData };
            this.dieRecoveryMock.Setup(o => o.GetMaskBits()).Returns(new BitArray(4, false));
            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            this.pinMapMock.Setup(o => o.Restore());
            this.PinMap_ = this.pinMapMock.Object;
            this.vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);
            this.strgvalFormatMock.Setup(o => o.SetData("C0F1:1:1.500_C1F1:1:1.500_C2F1:1:1.500_C3F1:1:1.500"));
            this.strgvalFormatMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgvalFormatMock.Object));

            var temp = (IVminSearchExtensions)this;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetDieRecoveryTrackers();
            this.SetIncomingMask();
            var repeat = temp.HasToRepeatSearch(searchPoints);
            Assert.IsFalse(repeat);
            var port = temp.PostProcessSearchResults(searchPoints);
            Assert.AreEqual(1, port);
            this.dieRecoveryMock.VerifyAll();
            this.pinMapMock.VerifyAll();
            this.vminForwardingCornerMock.VerifyAll();
            this.strgvalFormatMock.VerifyAll();
        }

        /// <summary>
        /// Failing search. None should not update table.
        /// </summary>
        [TestMethod]
        public void PostProcessSearchResults_None_Pass()
        {
            this.ForwardingMode = ForwardingModes.None;
            this.VoltageTargets = "C0,C1,C2,C3";
            this.CornerIdentifiers = "C0F1,C1F1,C2F1,C3F1";
            this.PinMap = "C0_MAP,C1_MAP,C2_MAP,C3_MAP";
            this.RecoveryOptions = "0000";
            this.StartVoltages = "0.5,0.5,0.5,0.5";
            this.EndVoltageLimits = "1.5,1.5,1.5,1.5";
            this.TestMode = TestModes.MultiVmin;

            var voltages = new List<double> { 0.5, 0.6, 0.7, 0.8 };
            var start = this.StartVoltages.ToList().ConvertAll(v => v.ToDouble());
            var end = this.EndVoltageLimits.ToList().ConvertAll(v => v.ToDouble());
            var searchPointData = new SearchPointData(voltages, new SearchPointData.PatternData("p1", 1, 1));
            var searchStateValues = new SearchStateValues()
            {
                Voltages = voltages,
                StartVoltages = start,
                EndVoltageLimits = end,
                MaskBits = new BitArray(4, false),
                PerTargetIncrements = new List<uint> { 0, 1, 2, 3 },
                PerPointData = new List<SearchPointData> { searchPointData },
            };
            var searchIdentifiers = new SearchIdentifiers(string.Empty, 0, 0);
            var searchResultData = new SearchResultData(searchStateValues, true, searchIdentifiers);
            var searchPoints = new List<SearchResultData> { searchResultData };
            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;
            var decoder = new Mock<IPinMapDecoder>(MockBehavior.Strict);
            decoder.Setup(o => o.NumberOfTrackerElements).Returns(4);
            this.pinMapMock.Setup(o => o.GetConfiguration()).Returns(new List<IPinMapDecoder> { decoder.Object });
            this.pinMapMock.Setup(o => o.Restore());
            this.PinMap_ = this.pinMapMock.Object;
            this.vminForwardingFactoryMock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns(1.5e9);
            this.strgvalFormatMock.Setup(o => o.SetData("C0F1:1:1.500_C1F1:1:1.500_C2F1:1:1.500_C3F1:1:1.500"));
            this.strgvalFormatMock.Setup(o => o.SetTnamePostfix("_vminFwCfg"));
            this.datalogServiceMock.Setup(o => o.WriteToItuff(this.strgvalFormatMock.Object));

            var temp = (IVminSearchExtensions)this;
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetIncomingMask();
            var repeat = temp.HasToRepeatSearch(searchPoints);
            Assert.IsFalse(repeat);
            var port = temp.PostProcessSearchResults(searchPoints);
            Assert.AreEqual(1, port);
            this.dieRecoveryMock.VerifyAll();
            this.pinMapMock.VerifyAll();
            this.vminForwardingCornerMock.VerifyAll();
        }

        /// <summary>
        /// Returns InitialMaskArray.
        /// </summary>
        [TestMethod]
        public void GetInitialMaskBits_AllZeros_Pass()
        {
            this.VoltageTargets = "T0,T1,T2,T3";
            this.PinMap = "P0,P1,P2,P3";
            this.InitialSearchMask_ = new BitArray(4, false);
            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            var result = ((IVminSearchExtensions)this).GetInitialMaskBits();
            Assert.IsTrue(result.Xor(this.InitialSearchMask_).OfType<bool>().All(e => !e));
        }

        /// <summary>
        /// RunRules using DieRecovery rules. Pass.
        /// </summary>
        [TestMethod]
        public void RunRules_DieRecovery_Pass()
        {
            this.RecoveryOptions = "SomeGroup , 2";
            this.VoltageTargets = "C0,C1,C2,C3";
            var incomingBitArray = new BitArray(new[] { true, false, false, false });
            var defeatureRules = new List<DefeatureRule.Rule>();
            var rule1 = new DefeatureRule.Rule("2C", "1100", 2, DefeatureRule.RuleType.Recovery, DefeatureRule.RuleMode.ValidCombinations);
            defeatureRules.Add(rule1);
            this.dieRecoveryMock.Setup(d => d.RunRule(new BitArray(new[] { true, true, false, false }), "SomeGroup")).Returns(defeatureRules);
            this.dieRecoveryMock.Setup(d => d.GetMaskBits()).Returns(new BitArray(new[] { false, true, false, false }));
            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;

            var searchResults = default(SearchResults);
            searchResults.TestResultsBits = new List<BitArray> { incomingBitArray };

            searchResults.RulesResultsBits = searchResults.RunRules(this.RecoveryOptions, this.DieRecoveryOutgoing_);
            Assert.IsFalse(searchResults.FailedRules);
            Assert.AreEqual("1100", searchResults.RulesResultsBits.ToBinaryString());
            this.dieRecoveryMock.VerifyAll();
        }

        /// <summary>
        /// RunRules using DieRecovery rules. Fail.
        /// </summary>
        [TestMethod]
        public void RunRules_DieRecovery_Fail()
        {
            this.RecoveryOptions = "SomeGroup , 4";
            this.VoltageTargets = "C0,C1,C2,C3";
            this.RecoveryTrackingIncoming = "T0,T1,T2,T3";
            this.RecoveryTrackingOutgoing = "T0,T1,T2,T3";
            this.PinMap = "P0,P1,P2,P3";
            var incomingBitArray = new BitArray(new[] { true, false, false, false });
            var defeatureRules = new List<DefeatureRule.Rule>();
            var rule1 = new DefeatureRule.Rule("2C", "1100", 2, DefeatureRule.RuleType.Recovery, DefeatureRule.RuleMode.ValidCombinations);
            defeatureRules.Add(rule1);
            this.dieRecoveryMock.Setup(d => d.RunRule(new BitArray(new[] { true, true, false, false }), "SomeGroup")).Returns(defeatureRules);
            this.dieRecoveryMock.Setup(d => d.GetMaskBits()).Returns(new BitArray(new[] { false, true, false, false }));
            this.DieRecoveryOutgoing_ = this.dieRecoveryMock.Object;

            this.VerifyFeatureSwitchSettings();
            this.CustomVerify();
            this.SetDieRecoveryTrackers();
            var searchResults = default(SearchResults);
            searchResults.TestResultsBits = new List<BitArray> { incomingBitArray };

            searchResults.RulesResultsBits = searchResults.RunRules(this.RecoveryOptions, this.DieRecoveryOutgoing_);
            Assert.IsTrue(searchResults.FailedRules);
            this.dieRecoveryMock.VerifyAll();
        }
    }
}
