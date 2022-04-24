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

namespace OdeseBinConverterTC.UnitTest
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.TestMethods.OdeseBinConverter;
    using Prime.UserVarService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class OdeseBinConverterTC_UnitTest : OdeseBinConverterTC
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IUserVarService> userVarServiceMock;

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(
                o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
            {
                System.Console.WriteLine($"DEBUG: {msg}");
            });
            this.consoleServiceMock.Setup(o =>
                    o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string msg, int line, string member, string path) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            this.userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            Prime.Services.UserVarService = this.userVarServiceMock.Object;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Passing_Test()
        {
            this.userVarServiceMock.Setup(o => o.GetStringValue("RunTimeLibraryVars", "iCGL_PrimeSimplifiedBinningIndexesForPassingBin")).Returns("2,3,4,5");
            this.userVarServiceMock.Setup(o => o.GetStringValue("RunTimeLibraryVars", "iCGL_PrimeSimplifiedBinningIndexesForFailingBin")).Returns("4,5,6,7");

            var result = ((IOdeseBinConverterExtensions)this).ConvertSoftBin(1234);
            Assert.AreEqual(1234, (int)result);
            result = ((IOdeseBinConverterExtensions)this).ConvertSoftBin(90191904);
            Assert.AreEqual(1904, (int)result);
            result = ((IOdeseBinConverterExtensions)this).ConvertSoftBin(10011234);
            Assert.AreEqual(112, (int)result);
        }
    }
}