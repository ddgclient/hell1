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

namespace DDG.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime;
    using Prime.ConsoleService;
    using Prime.PatConfigService;
    using Prime.PerformanceService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PatternModifications_UnitTest
    {
        /// <summary>
        /// Mocks setup.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Passing sized binary.
        /// </summary>
        [TestMethod]
        public void GetPatSymbolString_PassingBinaryWithSize_Pass()
        {
            var expected = "10";
            var result = "0b10".GetPatSymbolString(2);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Passing decimal value.
        /// </summary>
        [TestMethod]
        public void GetPatSymbolString_PassingDecimal_Pass()
        {
            var expected = "00010110";
            var result = "0d22".GetPatSymbolString(8);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Passing hex value.
        /// </summary>
        [TestMethod]
        public void GetPatSymbolString_PassingHex_Pass()
        {
            var expected = "1011100001101010";
            var result = "0xB86a".GetPatSymbolString(16);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Passing hex value with padding.
        /// </summary>
        [TestMethod]
        public void GetPatSymbolString_PassingHexPadding_Pass()
        {
            var expected = "0000000001101010";
            var result = "0x6a".GetPatSymbolString(16);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Passing hex value.
        /// </summary>
        [TestMethod]
        public void GetPatSymbolString_PassingHexReverse_Pass()
        {
            var expected = "0101011000011101";
            var result = "0xB86a'r".GetPatSymbolString(16);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Empty input.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetPatSymbolString_EmptyInput_Fail()
        {
            var result = string.Empty.GetPatSymbolString(1);
        }

        /// <summary>
        /// GetPatternConfigHandles: Restore.
        /// </summary>
        [TestMethod]
        public void GetPatternConfigHandles_Restore_Pass()
        {
            var input = "Config1:LLHH,Config2:0x7F";
            var patList = "somePlist";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            Services.ConsoleService = consoleServiceMock.Object;
            var patConfigHandleMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock
                .Setup(patternConfig => patternConfig.GetPatConfigHandleWithPlist("Config1", patList))
                .Returns(patConfigHandleMock.Object);
            patConfigServiceMock
                .Setup(patternConfig => patternConfig.GetPatConfigHandleWithPlist("Config2", patList))
                .Returns(patConfigHandleMock.Object);
            patConfigHandleMock.SetupSequence(o => o.GetExpectedDataSize())
                .Returns(4)
                .Returns(8);
            patConfigHandleMock.Setup(o => o.SetData("LLHH"));
            patConfigHandleMock.Setup(o => o.SetData("01111111"));
            Services.PatConfigService = patConfigServiceMock.Object;
            var target = DDG.PatternModifications.Service;
            var result = target.GetPatternConfigHandles(input, patList);
            patConfigHandleMock.VerifyAll();
            patConfigServiceMock.VerifyAll();
        }

        /// <summary>
        /// GetPatternConfigHandles: Restore.
        /// </summary>
        [TestMethod]
        public void GetPatternConfigHandles_NullInput_Fail()
        {
            var patList = "somePlist";
            var target = DDG.PatternModifications.Service;
            Assert.IsNull(target.GetPatternConfigHandles(null, patList));
        }

        /// <summary>
        /// GetPatternConfigHandles: Restore.
        /// </summary>
        [TestMethod]
        public void GetPatternConfigHandles_InvalidpatConfigPair_Fail()
        {
            var input = "Config1:LLHH:blah,Config2:8'x7F";
            var patList = "somePlist";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            Services.ConsoleService = consoleServiceMock.Object;

            var target = DDG.PatternModifications.Service;
            var ex = Assert.ThrowsException<ArgumentException>(() => target.GetPatternConfigHandles(input, patList));
            Assert.AreEqual("PatternModificationsBase.dll.GetPatternConfigHandles: Invalid PatConfig input=[Config1:LLHH:blah,Config2:8'x7F]", ex.Message);
        }

        /// <summary>
        /// GetPatternConfigHandles: Setup.
        /// </summary>
        [TestMethod]
        public void GetPatternConfigHandles_Setup_Pass()
        {
            var json = "Config1:HHLL'r,Config2:0x7F";
            var patList = "somePlist";
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            Services.ConsoleService = consoleServiceMock.Object;
            var patConfigHandleMock = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            patConfigHandleMock.SetupSequence(o => o.GetExpectedDataSize())
                .Returns(4)
                .Returns(8);
            patConfigHandleMock.Setup(o => o.SetData("LLHH"));
            patConfigHandleMock.Setup(o => o.SetData("01111111"));
            var patConfigServiceMock = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigServiceMock
                .Setup(patternConfig => patternConfig.GetPatConfigHandleWithPlist("Config1", patList))
                .Returns(patConfigHandleMock.Object);
            patConfigServiceMock
                .Setup(patternConfig => patternConfig.GetPatConfigHandleWithPlist("Config2", patList))
                .Returns(patConfigHandleMock.Object);
            Services.PatConfigService = patConfigServiceMock.Object;
            var target = DDG.PatternModifications.Service;
            var result = target.GetPatternConfigHandles(json, patList);
            patConfigHandleMock.VerifyAll();
            patConfigServiceMock.VerifyAll();
        }

        /// <summary>
        /// ApplyPatternConfigHandles: Empty.
        /// </summary>
        [TestMethod]
        public void ApplyPatternConfigHandles_Empty_Skip()
        {
            var handles = new List<IPatConfigHandle>();
            var target = DDG.PatternModifications.Service;
            target.ApplyPatternConfigHandles(handles);
        }

        /// <summary>
        /// ApplyPatternConfigHandles: Some handles.
        /// </summary>
        [TestMethod]
        public void ApplyPatternConfigHandles_SomeHandles_Pass()
        {
            var handle = new Mock<IPatConfigHandle>(MockBehavior.Strict);
            var handles = new List<IPatConfigHandle> { handle.Object };
            var patConfigService = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigService.Setup(p => p.Apply(handles));
            Prime.Services.PatConfigService = patConfigService.Object;

            var target = DDG.PatternModifications.Service;
            target.ApplyPatternConfigHandles(handles);
            patConfigService.VerifyAll();
        }
    }
}
