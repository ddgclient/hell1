// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// INTEL CONFIDENTIAL
// Copyright (2019) (2020) Intel Corporation
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

// https://dejanstojanovic.net/aspnet/2020/january/mocking-systemio-filesystem-in-unit-tests-in-aspnet-core/

namespace PrimeValTool_unitTest
{
    using System.Collections.Generic;
    using PrimeValTool;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="TestProgramResults"/> class.
    /// </summary>
    [TestClass]
    public class TestProgramResults_UnitTest
    {
        /// <summary>
        /// Dummy description.
        /// </summary>
        [TestMethod]
        public void GenerateTpResultStruct_NotEmptyListPortSame_Pass()
        {
            List<TestProgramResults> testProgramResults = new List<TestProgramResults>();
            string automationDirectory = @"C:\DummyFolder";

            var listItems = new TestProgramResults
            {
                TestInstanceName = "DC1::SimplaInstace_F1",
                TesterPort = "1",
                TestInstanceStatus = "bypass"
            };

            var localList = new Dictionary<string, TestProgramResults>
            {
                { listItems.TestInstanceName, listItems }
            };

            TestProgramResults.GenerateTpResultStruct(localList, automationDirectory, @"dummySubFolder", ref testProgramResults);
            Assert.AreEqual(localList.Count, testProgramResults.Count);

            foreach (var items in testProgramResults)
            {
                Assert.AreEqual("DC1", items.TpModuleName);
                Assert.AreEqual("SimplaInstace_F1", items.TestInstanceName);
                Assert.AreEqual("1", items.TesterPort);
                Assert.AreEqual("1", items.ExpectedPort[0]);
                Assert.AreEqual("PASS", items.PassFailStatus);
            }
        }

        /// <summary>
        /// Dummy description.
        /// </summary>
        [TestMethod]
        public void GenerateTpResultStruct_NotEmptyListPortDifferent_Pass()
        {
            List<TestProgramResults> testProgramResults = new List<TestProgramResults>();
            string automationDirectory = @"C:\DummyFolder";

            var listItems = new TestProgramResults
            {
                TestInstanceName = "DC1::SimplaInstace_F1",
                TesterPort = "0",
                TestInstanceStatus = "bypass"
            };

            var localList = new Dictionary<string, TestProgramResults> { { listItems.TestInstanceName, listItems } };
            TestProgramResults.GenerateTpResultStruct(localList, automationDirectory, @"dummySubFolder", ref testProgramResults);
            Assert.AreEqual(localList.Count, testProgramResults.Count);

            foreach (var items in testProgramResults)
            {
                Assert.AreEqual("DC1", items.TpModuleName);
                Assert.AreEqual("SimplaInstace_F1", items.TestInstanceName);
                Assert.AreEqual("0", items.TesterPort);
                Assert.AreEqual("1", items.ExpectedPort[0]);
                Assert.AreEqual("FAIL", items.PassFailStatus);
            }
        }

        /// <summary>
        /// Dummy description.
        /// </summary>
        [TestMethod]
        public void GenerateTpResultStruct_InstanceNameMissingSeparator_Pass()
        {
            List<TestProgramResults> testProgramResults = new List<TestProgramResults>();
            string automationDirectory = @"C:\DummyFolder";

            var listItems = new TestProgramResults
            {
                TestInstanceName = "DC1SimplaInstace_F1",
                TesterPort = "0",
                TestInstanceStatus = "bypass"
            };
            var localList = new Dictionary<string, TestProgramResults> { { listItems.TestInstanceName, listItems } };
            TestProgramResults.GenerateTpResultStruct(localList, automationDirectory, @"dummySubFolder", ref testProgramResults);
            Assert.AreEqual(localList.Count, testProgramResults.Count);
            foreach (var items in testProgramResults)
            {
                Assert.AreEqual("N/A", items.TpModuleName);
                Assert.AreEqual("DC1SimplaInstace_F1", items.TestInstanceName);
                Assert.AreEqual("0", items.TesterPort);
                Assert.AreEqual("1", items.ExpectedPort[0]);
                Assert.AreEqual("FAIL", items.PassFailStatus);
            }
        }

        /// <summary>
        /// Dummy description.
        /// </summary>
        [TestMethod]
        public void GenerateTpResultStruct_Port99_Pass()
        {
            List<TestProgramResults> testProgramResults = new List<TestProgramResults>();
            string automationDirectory = @"C:\DummyFolder";

            var listItems = new TestProgramResults
            {
                TestInstanceName = "DC1::SimplaInstace_F9",
                TesterPort = "1",
                TestInstanceStatus = "bypass"
            };

            var localList = new Dictionary<string, TestProgramResults> { { listItems.TestInstanceName, listItems } };
            TestProgramResults.GenerateTpResultStruct(localList, automationDirectory, @"dummySubFolder", ref testProgramResults);
            Assert.AreEqual(localList.Count, testProgramResults.Count);

            foreach (var items in testProgramResults)
            {
                Assert.AreEqual("DC1", items.TpModuleName);
                Assert.AreEqual("SimplaInstace_F9", items.TestInstanceName);
                Assert.AreEqual("1", items.TesterPort);
                Assert.AreEqual("9", items.ExpectedPort[0]);
                Assert.AreEqual("FAIL", items.PassFailStatus);
            }
        }

        /// <summary>
        /// Verify functionalities in IdentifyPortsInTestName method.
        /// </summary>
        [TestMethod]
        public void IdentifyPortsInName()
        {
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void ParseConsoleLog()
        {
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void GenerateCsvFile()
        {
            Assert.IsTrue(false);
        }

        public void CompareItuffResult()
        {
            Assert.IsTrue(false);
        }
    }
}