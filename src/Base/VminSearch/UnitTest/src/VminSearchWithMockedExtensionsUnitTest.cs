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

namespace Prime.TestMethods.VminSearch.UnitTest
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.FunctionalService;
    using Prime.TestMethods.VminSearch;
    using Prime.TestMethods.VminSearch.Helpers;
    using Prime.VoltageService;

    /// <summary>
    /// UnitTest class.
    /// </summary>
    [TestClass]
    public class VminSearchWithMockedExtensionsUnitTest
    {
        private Mock<IFunctionalService> funcServiceMock;
        private Mock<ICaptureCtvPerPinTest> funcTestMock;
        private Mock<IVoltageService> voltageServiceMock;
        private Mock<IVForcePinAttribute> dpsVoltageMock;
        private Mock<IVminSearchExtensions> extensionsMock;

        /// <summary>
        /// Setups the mocks.
        /// </summary>
        [TestInitialize]
        public void SetupMockingVerify()
        {
            // Mocking for IFunctionalService
            this.funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            this.funcTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            this.funcTestMock.Setup(func => func.ApplyTestConditions());
            this.funcTestMock.Setup(func => func.ResolvePlist("test")).Returns("plist");
            Services.FunctionalService = this.funcServiceMock.Object;

            // Mocking for IVoltageService
            this.voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            this.dpsVoltageMock = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            this.dpsVoltageMock.Setup(voltage => voltage.Reset());
            Services.VoltageService = this.voltageServiceMock.Object;

            // Mocking for IVminSearchExtensions
            this.extensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            this.extensionsMock.Setup(x => x.GetSearchVoltageObject(new List<string>() { "A", "B", "C", "D" }, "plist")).Returns(this.dpsVoltageMock.Object);
            this.extensionsMock.Setup(x => x.GetFunctionalTest("plist", "level", "timing", string.Empty)).Returns(this.funcTestMock.Object);
            this.extensionsMock.Setup(x => x.GetBypassPort()).Returns(-1);
            this.extensionsMock.Setup(x => x.ApplyPreExecuteSetup("plist"));
            this.extensionsMock.Setup(x => x.ApplyInitialVoltage(null));
            this.extensionsMock.Setup(x => x.ApplyPreSearchSetup("plist"));
        }

        /// <summary>
        /// Search execute throws due to invalid start voltage (wrong count) returned from GetStartVoltageValues extension.
        /// </summary>
        [TestMethod]
        public void Execute_GetStartVoltageValuesReturnsWrongCount_Throw()
        {
            // Mock setup
            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(true);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.4", "0.4", "0.4", "0.4" })).Returns(new List<double>() { 0.400, 0.400, 0.400 });

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.4,0.4,0.4,0.4",
                EndVoltageLimits = "0.6,0.6,0.6,0.6",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.ThrowsException<Base.Exceptions.TestMethodException>(() => search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Search execute throws due to invalid end voltage limit (wrong count) returned from GetEndVoltageLimitValues extension.
        /// </summary>
        [TestMethod]
        public void Execute_GetEndVoltageLimitValuesReturnsWrongCount_Throw()
        {
            // Mock setup
            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(true);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.4", "0.4", "0.4", "0.4" })).Returns(new List<double>() { 0.400, 0.400, 0.400, 0.400 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "0.6", "0.6", "0.6", "0.6" })).Returns(new List<double>() { 0.600, 0.600, 0.600, 0.600, 0.600 });

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.4,0.4,0.4,0.4",
                EndVoltageLimits = "0.6,0.6,0.6,0.6",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.ThrowsException<Base.Exceptions.TestMethodException>(() => search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Search is not executed due to invalid start voltage (all negative) returned from GetStartVoltageValues extension.
        /// </summary>
        [TestMethod]
        public void Execute_SearchNotExecutedDueToInvalidNegativeStartVoltages_Return0()
        {
            // Mock setup
            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(true);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.4", "0.4", "0.4", "0.4" })).Returns(new List<double>() { -9999, -8888, -9999, -8888 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "0.6", "0.6", "0.6", "0.6" })).Returns(new List<double>() { 0.600, 0.600, 0.600, 0.600 });
            this.extensionsMock.Setup(x => x.HasToContinueToNextSearch(It.IsAny<List<SearchResultData>>(), this.funcTestMock.Object)).Returns(false);
            this.extensionsMock.Setup(x => x.PostProcessSearchResults(It.Is<List<SearchResultData>>(y => y.TrueForAll(z =>
                z.IsPass.Equals(false)
                && z.TnamePostfix.Equals(string.Empty)
                && z.MultiPassCount.Equals(1U)
                && z.RepetitionCount.Equals(1U)
                && z.Voltages.SequenceEqual(new List<double>())
                && z.StartVoltages.SequenceEqual(new List<double>() { -9999, -8888, -9999, -8888 })
                && z.EndVoltageLimits.SequenceEqual(new List<double>() { 0.600, 0.600, 0.600, 0.600 })
                && z.ExecutionCount.Equals(0)
                && z.PerTargetIncrements.SequenceEqual(new List<uint> { 0, 0, 0, 0 })
                && z.VoltageLimitingPatterns.SequenceEqual(new List<string>()))))).Returns(1);

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.4,0.4,0.4,0.4",
                EndVoltageLimits = "0.6,0.6,0.6,0.6",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(0, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Execute simple case failing plist and decoding all targets failing at all search points.
        /// Additionally cover case of wrong initial mask from GetInitialMaskBits, size does not match number of targets.
        /// </summary>
        [TestMethod]
        public void Execute_FailAllTargetsForAllPoint_Return0()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.Execute()).Returns(false);

            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(true);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.4", "0.4", "0.4", "0.4" })).Returns(new List<double>() { 0.400, 0.400, 0.400, 0.400 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "0.6", "0.6", "0.6", "0.6" })).Returns(new List<double>() { 0.600, 0.600, 0.600, 0.600 });
            this.extensionsMock.Setup(x => x.GetInitialMaskBits()).Returns(new BitArray(3, false));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.400, 0.400, 0.400, 0.400 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.500, 0.500, 0.500, 0.500 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.600, 0.600, 0.600, 0.600 }));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(4, false), this.funcTestMock.Object));
            this.extensionsMock.Setup(x => x.ProcessPlistResults(false, this.funcTestMock.Object)).Returns(new BitArray(4, true));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard(string.Empty, false));
            this.extensionsMock.Setup(x => x.HasToRepeatSearch(It.IsAny<List<SearchResultData>>())).Returns(false);
            this.extensionsMock.Setup(x => x.HasToContinueToNextSearch(It.IsAny<List<SearchResultData>>(), this.funcTestMock.Object)).Returns(false);
            this.extensionsMock.Setup(x => x.PostProcessSearchResults(It.Is<List<SearchResultData>>(y => y.TrueForAll(z =>
                z.IsPass.Equals(false)
                && z.TnamePostfix.Equals(string.Empty)
                && z.MultiPassCount.Equals(1U)
                && z.RepetitionCount.Equals(1U)
                && z.Voltages.SequenceEqual(new List<double>() { -9999, -9999, -9999, -9999 })
                && z.StartVoltages.SequenceEqual(new List<double>() { 0.400, 0.400, 0.400, 0.400 })
                && z.EndVoltageLimits.SequenceEqual(new List<double>() { 0.600, 0.600, 0.600, 0.600 })
                && z.ExecutionCount.Equals(3)
                && z.PerTargetIncrements.SequenceEqual(new List<uint>() { 3, 3, 3, 3 })
                && z.VoltageLimitingPatterns.SequenceEqual(new List<string>() { "na", "na", "na", "na" }))))).Returns(1);

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.4,0.4,0.4,0.4",
                EndVoltageLimits = "0.6,0.6,0.6,0.6",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(0, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Execute simple case failing plist and decoding all targets failing at two search points but passing at third point.
        /// Additionally cover case of wrong initial mask from GetInitialMaskBits, all bits in given mask are true at first pass.
        /// </summary>
        [TestMethod]
        public void Execute_FailAllTargetsTwoPointsPassingThirdPoint_Return1()
        {
            // Mock setup
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(false).Returns(false).Returns(true);

            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(true);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.4", "0.4", "0.4", "0.4" })).Returns(new List<double>() { 0.400, 0.400, 0.400, 0.400 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "0.6", "0.6", "0.6", "0.6" })).Returns(new List<double>() { 0.600, 0.600, 0.600, 0.600 });
            this.extensionsMock.SetupSequence(x => x.GetInitialMaskBits()).Returns(new BitArray(4, true)).Returns(new BitArray(4, false));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.400, 0.400, 0.400, 0.400 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.500, 0.500, 0.500, 0.500 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.600, 0.600, 0.600, 0.600 }));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(4, false), this.funcTestMock.Object));
            this.extensionsMock.Setup(x => x.ProcessPlistResults(false, this.funcTestMock.Object)).Returns(new BitArray(4, true));
            this.extensionsMock.Setup(x => x.ProcessPlistResults(true, this.funcTestMock.Object)).Returns(new BitArray(4, false));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard("M2", true));
            this.extensionsMock.Setup(x => x.HasToRepeatSearch(It.IsAny<List<SearchResultData>>())).Returns(false);
            this.extensionsMock.Setup(x => x.HasToContinueToNextSearch(It.IsAny<List<SearchResultData>>(), this.funcTestMock.Object)).Returns(false);
            this.extensionsMock.Setup(x => x.PostProcessSearchResults(It.Is<List<SearchResultData>>(y => y.Exists(z =>
                z.IsPass.Equals(true)
                && z.TnamePostfix.Equals("M2")
                && z.MultiPassCount.Equals(2U)
                && z.RepetitionCount.Equals(1U)
                && z.Voltages.SequenceEqual(new List<double>() { 0.600, 0.600, 0.600, 0.600 })
                && z.StartVoltages.SequenceEqual(new List<double>() { 0.400, 0.400, 0.400, 0.400 })
                && z.EndVoltageLimits.SequenceEqual(new List<double>() { 0.600, 0.600, 0.600, 0.600 })
                && z.ExecutionCount.Equals(3)
                && z.PerTargetIncrements.SequenceEqual(new List<uint>() { 2, 2, 2, 2 })
                && z.VoltageLimitingPatterns.SequenceEqual(new List<string>() { "na", "na", "na", "na" }))))).Returns(1);

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.4,0.4,0.4,0.4",
                EndVoltageLimits = "0.6,0.6,0.6,0.6",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
                MultiPassMasks = "1111,0000",
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Execute simple case with different setups per target and different failing targets per iteration.
        /// </summary>
        [TestMethod]
        public void Execute_PassingSearchDifferentSetupsPerTarget_Return1()
        {
            // Mock setup
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(false).Returns(false).Returns(false).Returns(true);

            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(true);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.4", "0.5", "0.4", "0.5" })).Returns(new List<double>() { 0.400, 0.500, 0.600, 0.700 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "0.7", "1.0", "0.7", "1.0" })).Returns(new List<double>() { 0.600, 0.800, 1.000, 1.200 });
            this.extensionsMock.Setup(x => x.GetInitialMaskBits()).Returns(new BitArray(new[] { false, false, true, false }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.400, 0.500, -8888, 0.700 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.500, 0.600, -8888, 0.800 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.600, 0.600, -8888, 0.800 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { -9999, 0.600, -8888, 0.900 }));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { false, false, true, false }), this.funcTestMock.Object));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { true, false, true, false }), this.funcTestMock.Object));
            this.extensionsMock.SetupSequence(x => x.ProcessPlistResults(It.IsAny<bool>(), this.funcTestMock.Object))
                .Returns(new BitArray(new[] { true, true, false, true }))
                .Returns(new BitArray(new[] { true, false, false, false }))
                .Returns(new BitArray(new[] { true, false, false, true }))
                .Returns(new BitArray(new[] { false, false, false, false }));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard(string.Empty, true));
            this.extensionsMock.Setup(x => x.HasToRepeatSearch(It.IsAny<List<SearchResultData>>())).Returns(false);
            this.extensionsMock.Setup(x => x.HasToContinueToNextSearch(It.IsAny<List<SearchResultData>>(), this.funcTestMock.Object)).Returns(false);
            this.extensionsMock.Setup(x => x.PostProcessSearchResults(It.Is<List<SearchResultData>>(y => y.TrueForAll(z =>
                z.IsPass.Equals(true)
                && z.TnamePostfix.Equals(string.Empty)
                && z.MultiPassCount.Equals(1U)
                && z.RepetitionCount.Equals(1U)
                && z.Voltages.SequenceEqual(new List<double>() { -9999, 0.600, -8888, 0.900 })
                && z.StartVoltages.SequenceEqual(new List<double>() { 0.400, 0.500, 0.600, 0.700 })
                && z.EndVoltageLimits.SequenceEqual(new List<double>() { 0.600, 0.800, 1.000, 1.200 })
                && z.MaskBits.SequenceEqual(new BitArray(new BitArray(new[] { true, false, true, false })))
                && z.ExecutionCount.Equals(4)
                && z.PerTargetIncrements.SequenceEqual(new List<uint>() { 3, 1, 0, 2 })
                && z.VoltageLimitingPatterns.SequenceEqual(new List<string>() { "na", "na", "na", "na" }))))).Returns(1);

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.4,0.5,0.4,0.5",
                EndVoltageLimits = "0.7,1.0,0.7,1.0",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                MultiPassMasks = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Execute simple case with different setups per target with setting for single point execution.
        /// </summary>
        [TestMethod]
        public void Execute_DifferentSetupsPerTargetForceSinglePointFailApplyVoltage_Return0()
        {
            // Mock setup
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(false).Returns(false).Returns(false).Returns(true);

            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(true);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(true);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.4", "0.5", "0.4", "0.5" })).Returns(new List<double>() { 0.400, 0.500, 0.600, 0.700 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "0.7", "1.0", "0.7", "1.0" })).Returns(new List<double>() { 0.600, 0.800, 1.000, 1.200 });
            this.extensionsMock.Setup(x => x.GetInitialMaskBits()).Returns(new BitArray(new[] { false, false, true, false }));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { false, false, true, false }), this.funcTestMock.Object));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.400, 0.500, -8888, 0.700 }));
            this.extensionsMock.Setup(x => x.ProcessPlistResults(It.IsAny<bool>(), this.funcTestMock.Object)).Returns(new BitArray(new[] { true, true, false, true }));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard(string.Empty, false));
            this.extensionsMock.Setup(x => x.HasToRepeatSearch(It.IsAny<List<SearchResultData>>())).Returns(false);
            this.extensionsMock.Setup(x => x.HasToContinueToNextSearch(It.IsAny<List<SearchResultData>>(), this.funcTestMock.Object)).Returns(false);
            this.extensionsMock.Setup(x => x.PostProcessSearchResults(It.Is<List<SearchResultData>>(y => y.TrueForAll(z =>
                z.IsPass.Equals(false)
                && z.TnamePostfix.Equals(string.Empty)
                && z.MultiPassCount.Equals(1U)
                && z.RepetitionCount.Equals(1U)
                && z.Voltages.SequenceEqual(new List<double>() { -9999, -9999, -8888, -9999 })
                && z.StartVoltages.SequenceEqual(new List<double>() { 0.400, 0.500, 0.600, 0.700 })
                && z.EndVoltageLimits.SequenceEqual(new List<double>() { 0.600, 0.800, 1.000, 1.200 })
                && z.MaskBits.SequenceEqual(new BitArray(new BitArray(new[] { true, true, true, true })))
                && z.ExecutionCount.Equals(1)
                && z.PerTargetIncrements.SequenceEqual(new List<uint>() { 1, 1, 0, 1 })
                && z.VoltageLimitingPatterns.SequenceEqual(new List<string>() { "na", "na", "na", "na" }))))).Returns(1);

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.4,0.5,0.4,0.5",
                EndVoltageLimits = "0.7,1.0,0.7,1.0",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(0, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Execute case for failing of masking check (result bits with fail in masked position) initial and after reach search limit.
        /// </summary>
        [TestMethod]
        public void Execute_FailProcessResultMaskingCheck_Return0()
        {
            // Mock setup
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(false).Returns(false);

            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(true);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.4", "0.5", "0.4", "0.5" })).Returns(new List<double>() { 0.400, 0.500, 0.600, 0.700 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "0.7", "1.0", "0.7", "1.0" })).Returns(new List<double>() { 0.450, 0.600, 0.650, 0.800 });
            this.extensionsMock.Setup(x => x.GetInitialMaskBits()).Returns(new BitArray(new[] { false, false, true, false }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.400, 0.500, -8888, 0.700 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { -9999, 0.600, -8888, 0.800 }));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { false, false, true, false }), this.funcTestMock.Object));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { true, false, true, false }), this.funcTestMock.Object));
            this.extensionsMock.SetupSequence(x => x.ProcessPlistResults(It.IsAny<bool>(), this.funcTestMock.Object))
                .Returns(new BitArray(new[] { false, false, true, false }))
                .Returns(new BitArray(new[] { true, false, false, false }));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard(string.Empty, false));
            this.extensionsMock.Setup(x => x.HasToRepeatSearch(It.IsAny<List<SearchResultData>>())).Returns(false);
            this.extensionsMock.Setup(x => x.HasToContinueToNextSearch(It.IsAny<List<SearchResultData>>(), this.funcTestMock.Object)).Returns(false);
            this.extensionsMock.Setup(x => x.PostProcessSearchResults(It.Is<List<SearchResultData>>(y => y.TrueForAll(z =>
                z.IsPass.Equals(false)
                && z.TnamePostfix.Equals(string.Empty)
                && z.MultiPassCount.Equals(1U)
                && z.RepetitionCount.Equals(1U)
                && z.Voltages.SequenceEqual(new List<double>() { -9999, -9999, -8888, -9999 })
                && z.StartVoltages.SequenceEqual(new List<double>() { 0.400, 0.500, 0.600, 0.700 })
                && z.EndVoltageLimits.SequenceEqual(new List<double>() { 0.450, 0.600, 0.650, 0.800 })
                && z.MaskBits.SequenceEqual(new BitArray(new BitArray(new[] { true, true, true, true })))
                && z.ExecutionCount.Equals(2)
                && z.PerTargetIncrements.SequenceEqual(new List<uint>() { 1, 2, 0, 2 })
                && z.VoltageLimitingPatterns.SequenceEqual(new List<string>() { "na", "na", "na", "na" }))))).Returns(1);

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.4,0.5,0.4,0.5",
                EndVoltageLimits = "0.7,1.0,0.7,1.0",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(0, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Execute for failing of plist result check (plist fail, but bit result all pass) at first search point.
        /// </summary>
        [TestMethod]
        public void Execute_ProcessResultFailPlistCheckAtFirstSearchPoint_Return1()
        {
            // Mock setup
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(false).Returns(true);

            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(true);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.4", "0.5", "0.4", "0.5" })).Returns(new List<double>() { 0.400, 0.500, 0.600, 0.700 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "0.7", "1.0", "0.7", "1.0" })).Returns(new List<double>() { 0.450, 0.600, 0.650, 0.800 });
            this.extensionsMock.Setup(x => x.GetInitialMaskBits()).Returns(new BitArray(new[] { false, false, true, false }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.400, 0.500, -8888, 0.700 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { -9999, 0.600, -8888, 0.800 }));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { false, false, true, false }), this.funcTestMock.Object));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { true, false, true, false }), this.funcTestMock.Object));
            this.extensionsMock.SetupSequence(x => x.ProcessPlistResults(It.IsAny<bool>(), this.funcTestMock.Object))
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard(string.Empty, true));
            this.extensionsMock.Setup(x => x.HasToRepeatSearch(It.IsAny<List<SearchResultData>>())).Returns(false);
            this.extensionsMock.Setup(x => x.HasToContinueToNextSearch(It.IsAny<List<SearchResultData>>(), this.funcTestMock.Object)).Returns(false);
            this.extensionsMock.Setup(x => x.PostProcessSearchResults(It.Is<List<SearchResultData>>(y => y.TrueForAll(z =>
                z.IsPass.Equals(true)
                && z.TnamePostfix.Equals(string.Empty)
                && z.MultiPassCount.Equals(1U)
                && z.RepetitionCount.Equals(1U)
                && z.Voltages.SequenceEqual(new List<double>() { -9999, 0.600, -8888, 0.800 })
                && z.StartVoltages.SequenceEqual(new List<double>() { 0.400, 0.500, 0.600, 0.700 })
                && z.EndVoltageLimits.SequenceEqual(new List<double>() { 0.450, 0.600, 0.650, 0.800 })
                && z.MaskBits.SequenceEqual(new BitArray(new BitArray(new[] { true, false, true, false })))
                && z.ExecutionCount.Equals(2)
                && z.PerTargetIncrements.SequenceEqual(new List<uint>() { 1, 1, 0, 1 })
                && z.VoltageLimitingPatterns.SequenceEqual(new List<string>() { "na", "na", "na", "na" }))))).Returns(1);

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.4,0.5,0.4,0.5",
                EndVoltageLimits = "0.7,1.0,0.7,1.0",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            Assert.AreEqual(2, search.PointData.Count);
            Assert.IsFalse(search.SearchMask.Xor(new BitArray(new[] { true, false, true, true })).AndAll());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Execute skipping result bits checks and passing at first point even with a plist fail result due to setting to ignore result bits checks.
        /// </summary>
        [TestMethod]
        public void Execute_SkipResultBitsCheckingPlistCheckConditionHappensAtFirstPoint_Return1()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.Execute()).Returns(false);

            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(false);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.4", "0.5", "0.4", "0.5" })).Returns(new List<double>() { 0.400, 0.500, 0.600, 0.700 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "0.7", "1.0", "0.7", "1.0" })).Returns(new List<double>() { 0.450, 0.600, 0.650, 0.800 });
            this.extensionsMock.Setup(x => x.GetInitialMaskBits()).Returns(new BitArray(new[] { false, false, true, false }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.400, 0.500, -8888, 0.700 }));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { false, false, true, false }), this.funcTestMock.Object));
            this.extensionsMock.Setup(x => x.ProcessPlistResults(It.IsAny<bool>(), this.funcTestMock.Object)).Returns(new BitArray(new[] { false, false, false, false }));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard(string.Empty, true));
            this.extensionsMock.Setup(x => x.HasToRepeatSearch(It.IsAny<List<SearchResultData>>())).Returns(false);
            this.extensionsMock.Setup(x => x.HasToContinueToNextSearch(It.IsAny<List<SearchResultData>>(), this.funcTestMock.Object)).Returns(false);
            this.extensionsMock.Setup(x => x.PostProcessSearchResults(It.Is<List<SearchResultData>>(y => y.TrueForAll(z =>
                z.IsPass.Equals(true)
                && z.TnamePostfix.Equals(string.Empty)
                && z.MultiPassCount.Equals(1U)
                && z.RepetitionCount.Equals(1U)
                && z.Voltages.SequenceEqual(new List<double>() { 0.400, 0.500, -8888, 0.700 })
                && z.StartVoltages.SequenceEqual(new List<double>() { 0.400, 0.500, 0.600, 0.700 })
                && z.EndVoltageLimits.SequenceEqual(new List<double>() { 0.450, 0.600, 0.650, 0.800 })
                && z.MaskBits.SequenceEqual(new BitArray(new BitArray(new[] { false, false, true, false })))
                && z.ExecutionCount.Equals(1)
                && z.PerTargetIncrements.SequenceEqual(new List<uint>() { 0, 0, 0, 0 })
                && z.VoltageLimitingPatterns.SequenceEqual(new List<string>() { "na", "na", "na", "na" }))))).Returns(1);

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.4,0.5,0.4,0.5",
                EndVoltageLimits = "0.7,1.0,0.7,1.0",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Execute skipping result bits checks, matching masking fail condition but ignoring it due to setting to ignore result bits checks,
        /// but also matching check in DefineVoltagesForNextSearchPoint to avoid infinite loops if none voltage target gets updated.
        /// </summary>
        [TestMethod]
        public void Execute_SkipResultBitsCheckingMaskCheckConditionFailCheckInDefiningVoltagesForNextPoint_Return0()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.Execute()).Returns(false);

            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(false);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.4", "0.5", "0.4", "0.5" })).Returns(new List<double>() { 0.400, 0.500, 0.600, 0.700 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "0.7", "1.0", "0.7", "1.0" })).Returns(new List<double>() { 0.450, 0.600, 0.650, 0.800 });
            this.extensionsMock.Setup(x => x.GetInitialMaskBits()).Returns(new BitArray(new[] { false, false, true, false }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.400, 0.500, -8888, 0.700 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { -9999, 0.600, -8888, 0.800 }));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { false, false, true, false }), this.funcTestMock.Object));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { true, false, true, false }), this.funcTestMock.Object));
            this.extensionsMock.Setup(x => x.ProcessPlistResults(It.IsAny<bool>(), this.funcTestMock.Object)).Returns(new BitArray(new[] { false, false, true, false }));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard(string.Empty, false));
            this.extensionsMock.Setup(x => x.HasToRepeatSearch(It.IsAny<List<SearchResultData>>())).Returns(false);
            this.extensionsMock.Setup(x => x.HasToContinueToNextSearch(It.IsAny<List<SearchResultData>>(), this.funcTestMock.Object)).Returns(false);
            this.extensionsMock.Setup(x => x.PostProcessSearchResults(It.Is<List<SearchResultData>>(y => y.TrueForAll(z =>
                z.IsPass.Equals(false)
                && z.TnamePostfix.Equals(string.Empty)
                && z.MultiPassCount.Equals(1U)
                && z.RepetitionCount.Equals(1U)
                && z.Voltages.SequenceEqual(new List<double>() { -9999, -9999, -8888, -9999 })
                && z.StartVoltages.SequenceEqual(new List<double>() { 0.400, 0.500, 0.600, 0.700 })
                && z.EndVoltageLimits.SequenceEqual(new List<double>() { 0.450, 0.600, 0.650, 0.800 })
                && z.MaskBits.SequenceEqual(new BitArray(new BitArray(new[] { true, true, true, true })))
                && z.ExecutionCount.Equals(2)
                && z.PerTargetIncrements.SequenceEqual(new List<uint>() { 1, 2, 0, 2 })
                && z.VoltageLimitingPatterns.SequenceEqual(new List<string>() { "na", "na", "na", "na" }))))).Returns(1);

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.4,0.5,0.4,0.5",
                EndVoltageLimits = "0.7,1.0,0.7,1.0",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(0, search.Execute());
            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Execute overshoot capability when search pass at first try.
        /// </summary>
        [TestMethod]
        public void Execute_OvershootCapability_Return1()
        {
            // Mock Setup
            this.funcTestMock.Setup(func => func.Execute()).Returns(true);

            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(false);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.5", "0.5", "0.5", "0.5" })).Returns(new List<double>() { 0.500, 0.500, 0.500, 0.500 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "0.7", "0.7", "0.7", "0.7" })).Returns(new List<double>() { 0.700, 0.700, 0.700, 0.700 });
            this.extensionsMock.Setup(x => x.GetLowerStartVoltageValues(new List<string>() { "0.3", "0.3", "0.3", "0.3" })).Returns(new List<double>() { 0.300, 0.300, 0.300, 0.300 });
            this.extensionsMock.Setup(x => x.GetInitialMaskBits()).Returns(new BitArray(new[] { false, false, false, false }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.500, 0.500, 0.500, 0.500 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.300, 0.300, 0.300, 0.300 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.300, 0.300, 0.400, 0.300 }));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { false, false, false, false }), this.funcTestMock.Object));
            this.extensionsMock.SetupSequence(x => x.ProcessPlistResults(It.IsAny<bool>(), this.funcTestMock.Object))
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, true, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard(string.Empty, true));
            this.extensionsMock.Setup(x => x.HasToRepeatSearch(It.IsAny<List<SearchResultData>>())).Returns(false);
            this.extensionsMock.Setup(x => x.HasToContinueToNextSearch(It.IsAny<List<SearchResultData>>(), this.funcTestMock.Object)).Returns(false);
            this.extensionsMock.Setup(x => x.PostProcessSearchResults(It.Is<List<SearchResultData>>(y => y.TrueForAll(z =>
                z.IsPass.Equals(true)
                && z.TnamePostfix.Equals(string.Empty)
                && z.MultiPassCount.Equals(1U)
                && z.RepetitionCount.Equals(1U)
                && z.Voltages.SequenceEqual(new List<double>() { 0.300, 0.300, 0.400, 0.300 })
                && z.StartVoltages.SequenceEqual(new List<double>() { 0.300, 0.300, 0.300, 0.300 })
                && z.EndVoltageLimits.SequenceEqual(new List<double>() { 0.700, 0.700, 0.700, 0.700 })
                && z.MaskBits.SequenceEqual(new BitArray(new BitArray(4)))
                && z.ExecutionCount.Equals(3)
                && z.PerTargetIncrements.SequenceEqual(new List<uint>() { 0, 0, 1, 0 })
                && z.VoltageLimitingPatterns.SequenceEqual(new List<string>() { "na", "na", "na", "na" }))))).Returns(1);

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                PrePlist = string.Empty,
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.5,0.5,0.5,0.5",
                EndVoltageLimits = "0.7,0.7,0.7,0.7",
                StartVoltagesForRetry = "0.3,0.3,0.3,0.3",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());
            this.dpsVoltageMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Executes repetition capability for one search.
        /// </summary>
        [TestMethod]
        public void Execute_OneSearchWithRepetition_Return1()
        {
            // Mock setup
            this.funcTestMock.SetupSequence(func => func.Execute()).Returns(false).Returns(true).Returns(false).Returns(false).Returns(true);

            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(true);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.5", "0.5", "0.5", "0.5" })).Returns(new List<double>() { 0.500, 0.500, 0.500, 0.500 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "1.0", "1.0", "1.0", "1.0" })).Returns(new List<double>() { 1.000, 1.000, 1.000, 1.000 });
            this.extensionsMock.Setup(x => x.GetInitialMaskBits()).Returns(new BitArray(new[] { false, false, false, false }));

            // First repetition
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.500, 0.500, 0.500, 0.500 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.500, 0.600, 0.600, 0.500 }));

            // Second repetition
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.500, 0.600, 0.600, 0.600 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.500, 0.700, 0.600, 0.600 }));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(4), this.funcTestMock.Object));
            this.extensionsMock.SetupSequence(x => x.ProcessPlistResults(It.IsAny<bool>(), this.funcTestMock.Object))
                .Returns(new BitArray(new[] { false, true, true, false }))
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, true, true, true }))
                .Returns(new BitArray(new[] { false, true, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard("R1", true));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard("R2", true));
            this.extensionsMock.Setup(x => x.HasToRepeatSearch(It.IsAny<List<SearchResultData>>())).Returns(true);
            this.extensionsMock.Setup(x => x.HasToContinueToNextSearch(It.IsAny<List<SearchResultData>>(), this.funcTestMock.Object)).Returns(true);
            this.extensionsMock.Setup(x => x.PostProcessSearchResults(It.IsAny<List<SearchResultData>>())).Returns(1);

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.5,0.5,0.5,0.5",
                EndVoltageLimits = "1.0,1.0,1.0,1.0",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
                MaxRepetitionCount = 2,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());

            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Execute multi pass, overshoot and repetition in the same search.
        /// </summary>
        [TestMethod]
        public void Execute_MultiPassOvershootRepetition_Return1()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.Execute()).Returns(true);

            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(true);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.5", "0.5", "0.5", "0.5" })).Returns(new List<double>() { 0.500, 0.500, 0.500, 0.500 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "0.7", "0.7", "0.7", "0.7" })).Returns(new List<double>() { 0.700, 0.700, 0.700, 0.700 });
            this.extensionsMock.Setup(x => x.GetLowerStartVoltageValues(new List<string>() { "0.3", "0.3", "0.3", "0.3" })).Returns(new List<double>() { 0.300, 0.300, 0.300, 0.300 });
            this.extensionsMock.SetupSequence(x => x.GetInitialMaskBits())
                .Returns(new BitArray(new[] { false, false, true, true }))
                .Returns(new BitArray(new[] { false, false, true, true }))
                .Returns(new BitArray(new[] { false, true, false, true }))
                .Returns(new BitArray(new[] { true, true, false, false }))
                .Returns(new BitArray(new[] { true, true, false, false }))
                .Returns(new BitArray(new[] { true, true, false, false }))
                .Returns(new BitArray(new[] { true, false, true, false }))
                .Returns(new BitArray(new[] { true, false, true, false }));

            // First multi-pass and overshoot
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.500, 0.500, -8888, -8888 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.300, 0.300, -8888, -8888 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.400, 0.300, -8888, -8888 }));

            // Second multi-pass and repetition
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.500, -8888, 0.500, -8888 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.600, -8888, 0.600, -8888 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.700, -8888, 0.600, -8888 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { -9999, -8888, 0.600, -8888 }));

            // Third multi-pass, overshoot and repetition
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { -8888, -8888, 0.500, 0.500 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { -8888, -8888, 0.300, 0.300 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { -8888, -8888, 0.300, 0.400 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { -8888, -8888, 0.600, 0.500 }));

            // Fourth multi-pass, overshoot and repetition
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { -8888, 0.500, -8888, 0.500 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { -8888, 0.300, -8888, 0.300 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { -8888, 0.400, -8888, 0.300 }));

            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { false, false, true, true }), this.funcTestMock.Object));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { false, true, false, true }), this.funcTestMock.Object));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { true, true, false, true }), this.funcTestMock.Object));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { true, true, false, false }), this.funcTestMock.Object));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(new[] { true, false, true, false }), this.funcTestMock.Object));
            this.extensionsMock.SetupSequence(x => x.ProcessPlistResults(It.IsAny<bool>(), this.funcTestMock.Object))
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { true, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }))

                .Returns(new BitArray(new[] { true, false, true, false }))
                .Returns(new BitArray(new[] { true, false, false, false }))
                .Returns(new BitArray(new[] { true, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }))

                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, true }))
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, true, false }))
                .Returns(new BitArray(new[] { false, false, false, false }))

                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, true, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));

            this.extensionsMock.Setup(x => x.ExecuteScoreboard("M1R1", true));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard("M2R1", true));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard("M3R1", true));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard("M3R2", true));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard("M4R1", true));
            this.extensionsMock.SetupSequence(x => x.HasToRepeatSearch(It.IsAny<List<SearchResultData>>())).Returns(false).Returns(false).Returns(true).Returns(false);
            this.extensionsMock.Setup(x => x.HasToContinueToNextSearch(It.IsAny<List<SearchResultData>>(), this.funcTestMock.Object)).Returns(true);
            this.extensionsMock.Setup(x => x.PostProcessSearchResults(It.IsAny<List<SearchResultData>>())).Returns(1);

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.5,0.5,0.5,0.5",
                EndVoltageLimits = "0.7,0.7,0.7,0.7",
                StartVoltagesForRetry = "0.3,0.3,0.3,0.3",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                MultiPassMasks = "0011,0101,1100,1010",
                MaxRepetitionCount = 2,
                TestMethodExtension = this.extensionsMock.Object,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());

            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Executes multiple repetition with overshoot.
        /// </summary>
        [TestMethod]
        public void Execute_RepetitionWithOvershoot_Return1()
        {
            // Mock setup
            this.funcTestMock.Setup(func => func.Execute()).Returns(true);

            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(true);
            this.extensionsMock.Setup(x => x.GetStartVoltageValues(new List<string>() { "0.5", "0.5", "0.5", "0.5" })).Returns(new List<double>() { 0.500, 0.500, 0.500, 0.500 });
            this.extensionsMock.Setup(x => x.GetEndVoltageLimitValues(new List<string>() { "0.7", "0.7", "0.7", "0.7" })).Returns(new List<double>() { 0.700, 0.700, 0.700, 0.700 });
            this.extensionsMock.Setup(x => x.GetLowerStartVoltageValues(new List<string>() { "0.3", "0.3", "0.3", "0.3" })).Returns(new List<double>() { 0.300, 0.300, 0.300, 0.300 });
            this.extensionsMock.Setup(x => x.GetInitialMaskBits()).Returns(new BitArray(4));
            this.extensionsMock.Setup(x => x.ApplyMask(new BitArray(4), this.funcTestMock.Object));

            this.extensionsMock.SetupSequence(x => x.ProcessPlistResults(It.IsAny<bool>(), this.funcTestMock.Object))
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, true, false }))
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }))
                .Returns(new BitArray(new[] { true, false, false, false }))
                .Returns(new BitArray(new[] { false, false, false, false }));

            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.500, 0.500, 0.500, 0.500 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.300, 0.300, 0.300, 0.300 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.300, 0.300, 0.400, 0.300 }));
            this.extensionsMock.Setup(x => x.ApplySearchVoltage(this.dpsVoltageMock.Object, new List<double>() { 0.400, 0.300, 0.300, 0.300 }));

            this.extensionsMock.Setup(x => x.ExecuteScoreboard("R1", true));
            this.extensionsMock.Setup(x => x.ExecuteScoreboard("R2", true));
            this.extensionsMock.SetupSequence(x => x.HasToRepeatSearch(It.IsAny<List<SearchResultData>>())).Returns(true).Returns(true);
            this.extensionsMock.Setup(x => x.HasToContinueToNextSearch(It.IsAny<List<SearchResultData>>(), this.funcTestMock.Object)).Returns(true);
            this.extensionsMock.Setup(x => x.PostProcessSearchResults(It.IsAny<List<SearchResultData>>())).Returns(1);

            this.dpsVoltageMock.Setup(voltage => voltage.Restore());

            // Test setup
            var search = new PrimeVminSearchTestMethod()
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.5,0.5,0.5,0.5",
                EndVoltageLimits = "0.7,0.7,0.7,0.7",
                StartVoltagesForRetry = "0.3,0.3,0.3,0.3",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                MaxRepetitionCount = 2,
                TestMethodExtension = this.extensionsMock.Object,
            };
            search.InstanceName = "test";

            // Test
            search.Verify();
            Assert.AreEqual(1, search.Execute());

            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }

        /// <summary>
        /// Executes multiple repetition with overshoot.
        /// </summary>
        [TestMethod]
        public void Execute_InstanceBypass_Return1()
        {
            // Mock setup
            this.funcTestMock = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            this.dpsVoltageMock = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            this.funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);

            this.extensionsMock = new Mock<IVminSearchExtensions>(MockBehavior.Strict);
            this.extensionsMock.Setup(x => x.GetSearchVoltageObject(new List<string>() { "A", "B", "C", "D" }, "plist")).Returns(this.dpsVoltageMock.Object);
            this.extensionsMock.Setup(x => x.GetFunctionalTest("plist", "level", "timing", string.Empty)).Returns(this.funcTestMock.Object);
            this.extensionsMock.Setup(x => x.GetBypassPort()).Returns(3);
            this.extensionsMock.Setup(x => x.IsSinglePointMode()).Returns(false);
            this.extensionsMock.Setup(x => x.IsCheckOfResultBitsEnabled()).Returns(true);

            // Test setup
            var search = new PrimeVminSearchTestMethod
            {
                LevelsTc = "level",
                TimingsTc = "timing",
                Patlist = "plist",
                VoltageTargets = "A,B,C,D",
                StartVoltages = "0.5,0.5,0.5,0.5",
                EndVoltageLimits = "1.0,1.0,1.0,1.0",
                StepSize = 0.1,
                FeatureSwitchSettings = string.Empty,
                IfeObject = string.Empty,
                FivrCondition = string.Empty,
                TestMethodExtension = this.extensionsMock.Object,
                MaxRepetitionCount = 2,
            };

            // Test
            search.Verify();
            Assert.AreEqual(3, search.Execute());

            this.voltageServiceMock.VerifyAll();
            this.dpsVoltageMock.VerifyAll();
            this.funcServiceMock.VerifyAll();
            this.funcTestMock.VerifyAll();
            this.extensionsMock.VerifyAll();
        }
    }
}
