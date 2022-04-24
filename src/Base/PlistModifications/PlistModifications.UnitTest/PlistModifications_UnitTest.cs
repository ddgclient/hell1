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
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.PerformanceService;
    using Prime.PlistService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PlistModifications_UnitTest
    {
        private Mock<IPlistService> plistService;
        private Mock<IPlistObject> plistObject;
        private Mock<IPlistContent> patternContent;
        private Mock<IPlistContent> plistContent;

        /// <summary>
        /// Initializes mocks and other initial values.
        /// </summary>
        /// <exception cref="FatalException">Prime Exception.</exception>
        [TestInitialize]
        public void Initialize()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
            {
                System.Console.WriteLine($"DEBUG: {msg}");
            });

            Prime.Services.ConsoleService = consoleServiceMock.Object;

            DDG.PlistModifications.Service.CleanTree(string.Empty);
            this.plistService = new Mock<IPlistService>(MockBehavior.Strict);
            Prime.Services.PlistService = this.plistService.Object;
            this.plistObject = new Mock<IPlistObject>(MockBehavior.Strict);
            this.patternContent = new Mock<IPlistContent>(MockBehavior.Strict);
            this.patternContent.Setup(o => o.IsPattern()).Returns(true);
            this.plistContent = new Mock<IPlistContent>(MockBehavior.Strict);
            this.plistContent.Setup(o => o.IsPattern()).Returns(false);
            this.plistContent.SetupSequence(o => o.GetPlistItemName())
                .Returns("subplist1")
                .Returns("subplist2");
            this.plistObject.SetupSequence(o => o.GetPatternsAndIndexes(false))
                .Returns(new List<IPlistContent> { this.patternContent.Object, this.plistContent.Object })
                .Returns(new List<IPlistContent> { this.patternContent.Object, this.plistContent.Object })
                .Returns(new List<IPlistContent> { this.patternContent.Object });
            this.plistService.Setup(o => o.GetPlistObject(It.IsAny<string>())).Returns(this.plistObject.Object);

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void RestoreTree_EmptyParameter_Pass()
        {
            this.plistContent.Setup(o => o.GetPatternIndex()).Returns(2);
            this.patternContent.Setup(o => o.GetPatternIndex()).Returns(1);
            this.plistObject.Setup(o => o.Resolve());

            var tree = DDG.PlistModifications.Service.BuildPlistTree("SomePatlist");
            DDG.PlistModifications.Service.RestoreTree(string.Empty);
            this.plistContent.VerifyAll();
            this.patternContent.VerifyAll();
            this.plistObject.VerifyAll();
            this.plistService.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void RestoreTree_PassingParameter_Pass()
        {
            this.plistContent.Setup(o => o.GetPatternIndex()).Returns(2);
            this.patternContent.Setup(o => o.GetPatternIndex()).Returns(1);
            this.plistObject.Setup(o => o.Resolve());

            var tree = DDG.PlistModifications.Service.BuildPlistTree("SomePatlist");
            DDG.PlistModifications.Service.RestoreTree("SomePatlist");
            this.plistContent.VerifyAll();
            this.patternContent.VerifyAll();
            this.plistObject.VerifyAll();
            this.plistService.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void RestoreTree_SubPlist_Pass()
        {
            this.plistContent.Setup(o => o.GetPatternIndex()).Returns(2);
            this.patternContent.Setup(o => o.GetPatternIndex()).Returns(1);
            this.plistObject.Setup(o => o.Resolve());

            var tree = DDG.PlistModifications.Service.BuildPlistTree("SomePatlist");
            DDG.PlistModifications.Service.RestoreTree("subplist2");
            this.plistContent.VerifyAll();
            this.patternContent.VerifyAll();
            this.plistObject.VerifyAll();
            this.plistService.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void CleanTree_EmptyParameter_Pass()
        {
            this.plistContent.Setup(o => o.GetPatternIndex()).Returns(2);

            var tree = DDG.PlistModifications.Service.BuildPlistTree("SomePatlist");
            DDG.PlistModifications.Service.CleanTree(string.Empty);
            this.plistContent.VerifyAll();
            this.patternContent.VerifyAll();
            this.plistObject.VerifyAll();
            this.plistService.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void CleanTree_PassingParameter_Pass()
        {
            this.plistContent.Setup(o => o.GetPatternIndex()).Returns(2);

            var tree = DDG.PlistModifications.Service.BuildPlistTree("SomePatlist");
            DDG.PlistModifications.Service.CleanTree("SomePatlist");
            this.plistContent.VerifyAll();
            this.patternContent.VerifyAll();
            this.plistObject.VerifyAll();
            this.plistService.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Find_Subplist_Pass()
        {
            this.plistContent.Setup(o => o.GetPatternIndex()).Returns(2);

            var tree = DDG.PlistModifications.Service.BuildPlistTree("SomePatlist");
            var result = tree.Find("subplist2");
            Assert.AreEqual("subplist2", result.PlistName);
            this.plistContent.VerifyAll();
            this.patternContent.VerifyAll();
            this.plistObject.VerifyAll();
            this.plistService.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Find_SubPlistNoFound_Null()
        {
            this.plistContent.Setup(o => o.GetPatternIndex()).Returns(2);

            var tree = DDG.PlistModifications.Service.BuildPlistTree("SomePatlist");
            var result = tree.Find("invalid");
            Assert.IsNull(result);
            this.plistContent.VerifyAll();
            this.patternContent.VerifyAll();
            this.plistObject.VerifyAll();
            this.plistService.VerifyAll();
        }
    }
}
