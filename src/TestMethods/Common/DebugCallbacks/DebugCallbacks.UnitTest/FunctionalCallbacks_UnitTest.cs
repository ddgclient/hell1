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

namespace DebugCallbacks.UnitTest
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.FunctionalService;
    using Prime.PerformanceService;
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="FunctionalCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class FunctionalCallbacks_UnitTest
    {
        private Mock<ITestProgramService> testProgramServiceMock;
        private Mock<IFunctionalService> functionalServiceMock;

        /// <summary>
        /// Set up the common mocks for testing.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            this.testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            Prime.Services.TestProgramService = this.testProgramServiceMock.Object;
            var parameters = new Dictionary<string, string> { { "LevelsTc", "SomeLevels" }, { "TimingsTc", "SomeTimings" } };
            this.testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(parameters);

            var noCaptureTest = new Mock<INoCaptureTest>(MockBehavior.Strict);
            noCaptureTest.Setup(o => o.Execute()).Returns(true);
            this.functionalServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            Prime.Services.FunctionalService = this.functionalServiceMock.Object;
            this.functionalServiceMock.Setup(o => o.CreateNoCaptureTest("SomePlist", "SomeLevels", "SomeTimings", string.Empty))
                .Returns(noCaptureTest.Object);

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Refer to method name.
        /// </summary>
        [TestMethod]
        public void ExecuteNoCapturePlist_Pass()
        {
            var result = Functional.ExecuteNoCapturePlist("SomePlist");
            Assert.AreEqual("1", result);
        }
    }
}
