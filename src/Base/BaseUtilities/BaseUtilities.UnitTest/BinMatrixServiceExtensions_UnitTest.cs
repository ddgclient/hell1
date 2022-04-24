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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.BinMatrixService;
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="BinMatrixServiceExtensions_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class BinMatrixServiceExtensions_UnitTest
    {
        /// <summary>
        /// No spec to replace. Should return same input.
        /// </summary>
        [TestMethod]
        public void EvaluateString_NoBrackets_pass()
        {
            var result = Prime.Services.BinMatrixService.EvaluateString("hello world");
            Assert.AreEqual("hello world", result);
        }

        /// <summary>
        /// Passing flowNumber as argument.
        /// </summary>
        [TestMethod]
        public void EvaluateString_UsingFlowAsArgument_pass()
        {
            var flowNumber = 1;
            var specName = "there";
            var specValue = "world";
            var specUnits = "1";

            var binMatrixMock = new Mock<IBinMatrixService>(MockBehavior.Strict);
            var specSetInfoMock = new Mock<ISpecInfo>(MockBehavior.Strict);
            specSetInfoMock.Setup(s => s.GetData()).Returns(specValue);
            specSetInfoMock.Setup(s => s.GetUnit()).Returns(specUnits);
            binMatrixMock.Setup(b => b.GetSpecInfo(flowNumber, specName)).Returns(specSetInfoMock.Object);
            Prime.Services.BinMatrixService = binMatrixMock.Object;

            var result = Prime.Services.BinMatrixService.EvaluateString("hello {there}", flowNumber);
            Assert.AreEqual($"hello {specValue}{specUnits}", result);
            binMatrixMock.VerifyAll();
            specSetInfoMock.VerifyAll();
        }

        /// <summary>
        /// Auto detect flowNumber.
        /// </summary>
        [TestMethod]
        public void EvaluateString_AutoDetectFlowNumber_pass()
        {
            var flowNumber = 1;
            var specName = "there";
            var specValue = "world";
            var specUnits = "1";
            var parameters = new Dictionary<string, string>
            {
                { "timing", "FakeTiming" },
                { "levels", "FakeLevels" },
                { "FlowNumber", "1" },
                { "otherparam", "somevalue" },
                { "Patlist", "PLIST12" },
            };

            var binMatrixMock = new Mock<IBinMatrixService>(MockBehavior.Strict);
            var specSetInfoMock = new Mock<ISpecInfo>(MockBehavior.Strict);
            specSetInfoMock.Setup(s => s.GetData()).Returns(specValue);
            specSetInfoMock.Setup(s => s.GetUnit()).Returns(specUnits);
            binMatrixMock.Setup(b => b.GetSpecInfo(flowNumber, specName)).Returns(specSetInfoMock.Object);
            Prime.Services.BinMatrixService = binMatrixMock.Object;
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(parameters);
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            var result = Prime.Services.BinMatrixService.EvaluateString("hello {there}");
            Assert.AreEqual($"hello {specValue}{specUnits}", result);
            binMatrixMock.VerifyAll();
            specSetInfoMock.VerifyAll();
        }
    }
}
