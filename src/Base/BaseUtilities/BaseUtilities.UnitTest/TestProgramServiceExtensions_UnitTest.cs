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
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="TestProgramServiceExtensions_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TestProgramServiceExtensions_UnitTest
    {
        /// <summary>
        /// Test the passing cases for TestProgramServiceExtensions.GetCurrentPatternLists.
        /// </summary>
        [TestMethod]
        public void GetCurrentPatternLists_Pass()
        {
            Dictionary<string, string> plistParameters = new Dictionary<string, string>()
            {
                { "patlist", "PLIST1" },
                { "reset_patlist", "PLIST2" },
                { "retest_patlist", "PLIST3" },
                { "arbiter_patlist", "PLIST4" },
                { "primary_patlist", "PLIST5" },
                { "secondary_patlist", "PLIST6" },
                { "leak_high_patlist", "PLIST7" },
                { "leak_low_patlist", "PLIST8" },
                { "prescreen_patlist", "PLIST9" },
                { "second_patlist", "PLIST10" },
                { "search_reset_patlist", "PLIST11" },
                { "Patlist", "PLIST12" },
            };

            Dictionary<string, string> allParameters = new Dictionary<string, string>(plistParameters);
            allParameters["timing"] = "FakeTiming";
            allParameters["levels"] = "FakeLevels";
            allParameters["otherparam"] = "somevalue";

            var serviceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            serviceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(allParameters);

            var expected = plistParameters.Values.OrderBy(s => s).ToList();
            var actual = DDG.TestProgramServiceExtensions.GetCurrentPatternLists(serviceMock.Object).OrderBy(s => s).ToList();
            CollectionAssert.AreEqual(expected, actual);
            serviceMock.VerifyAll();
        }

        /// <summary>
        /// Test the passing cases for TestProgramServiceExtensions.GetCurrentPatternLists.
        /// </summary>
        [TestMethod]
        public void GetCurrentTimings_Pass()
        {
            Dictionary<string, string> allParameters = new Dictionary<string, string>()
            {
                { "timing", "FakeTiming" },
                { "levels", "FakeLevels" },
                { "patlist", "PLIST1" },
                { "otherparam", "somevalue" },
            };

            var serviceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            serviceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(allParameters);

            Assert.AreEqual("FakeTiming", DDG.TestProgramServiceExtensions.GetCurrentTimings(serviceMock.Object));
            serviceMock.VerifyAll();
        }

        /// <summary>
        /// Test the passing cases for TestProgramServiceExtensions.GetCurrentPatternLists.
        /// </summary>
        [TestMethod]
        public void GetCurrentLevels_Pass()
        {
            Dictionary<string, string> allParameters = new Dictionary<string, string>()
            {
                { "timing", "FakeTiming" },
                { "levels", "FakeLevels" },
                { "patlist", "PLIST1" },
                { "otherparam", "somevalue" },
            };

            var serviceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            serviceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(allParameters);

            Assert.AreEqual("FakeLevels", DDG.TestProgramServiceExtensions.GetCurrentLevels(serviceMock.Object));
            serviceMock.VerifyAll();
        }

        /// <summary>
        /// Test the passing cases for TestProgramServiceExtensions.GetCurrentPatternLists.
        /// </summary>
        [TestMethod]
        public void GetCurrentFlowNumber_Pass()
        {
            this.TestGetCurrentFlowNumber(
                1,
                new Dictionary<string, string>
                {
                    { "timing", "FakeTiming" },
                    { "levels", "FakeLevels" },
                    { "FlowNumber", "1" },
                    { "otherparam", "somevalue" },
                    { "Patlist", "PLIST12" },
                });

            this.TestGetCurrentFlowNumber(
                4,
                new Dictionary<string, string>
                {
                    { "timing", "FakeTiming" },
                    { "levels", "FakeLevels" },
                    { "setting_values", "a,b,flow:4" },
                    { "otherparam", "somevalue" },
                    { "Patlist", "PLIST12" },
                });

            this.TestGetCurrentFlowNumber(-1, new Dictionary<string, string> { { "setting_values", "a,b,other:blah" } });
            this.TestGetCurrentFlowNumber(-1, new Dictionary<string, string> { { "setting_values", "a,b,other:blah1:blah2" } });
            this.TestGetCurrentFlowNumber(-1, new Dictionary<string, string> { { "setting_values", string.Empty } });

            this.TestGetCurrentFlowNumber(
                -1,
                new Dictionary<string, string>
                {
                    { "timing", "FakeTiming" },
                    { "levels", "FakeLevels" },
                    { "otherparam", "somevalue" },
                    { "Patlist", "PLIST12" },
                });
        }

        /// <summary>
        /// Test GetCurrentLogLevel as called from a Prime instance.
        /// </summary>
        [TestMethod]
        public void GetCurrentLogLevel_Prime_Pass()
        {
            var serviceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            serviceMock.Setup(o => o.GetCurrentTestInstanceParameter("LogLevel")).Returns("TEST_METHOD");
            Assert.AreEqual("TEST_METHOD", DDG.TestProgramServiceExtensions.GetCurrentLogLevel(serviceMock.Object));
            serviceMock.VerifyAll();
        }

        /// <summary>
        /// Test GetCurrentLogLevel as called from an Evergreen instance.
        /// </summary>
        [TestMethod]
        public void GetCurrentLogLevel_Evg_Pass()
        {
            var serviceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            serviceMock.Setup(o => o.GetCurrentTestInstanceParameter("LogLevel")).Throws(new Prime.Base.Exceptions.FatalException("blah"));
            Assert.AreEqual("DISABLED", DDG.TestProgramServiceExtensions.GetCurrentLogLevel(serviceMock.Object));
            serviceMock.VerifyAll();
        }

        private void TestGetCurrentFlowNumber(int expected, Dictionary<string, string> parameters)
        {
            var serviceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            serviceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(parameters);
            Assert.AreEqual(expected, DDG.TestProgramServiceExtensions.GetCurrentFlowNumber(serviceMock.Object));
            serviceMock.VerifyAll();
        }
    }
}