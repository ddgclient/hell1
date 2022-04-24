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

namespace LSARasterTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using global::LSARasterTC;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;

    /// <summary> Validating ConditionsChecker class used in CTV_MODE. </summary>
    [TestClass]
    public class ConditionsChecker_UnitTest
    {
        /// <summary>
        /// Assert that condition passes if vector meets criteria.
        /// </summary>
        [TestMethod]
        public void CheckIfConditionPassed_Success()
        {
            string conditions = "P002,0-1,00|P002,3,1";
            string conditions2 = "P002,0-1,00|P002,3,1|NOA1,0-6,01110000";

            // X's in string work as "don't cares"
            Dictionary<string, string> ctvData = new Dictionary<string, string>()
            {
                { "P002", "00X100X1" },
                { "NOA1", "01110000" },
            };

            var checker = new HryConditionsChecker(ctvData, conditions);
            var checker2 = new HryConditionsChecker(ctvData, conditions2);

            Assert.IsTrue(checker.CheckIfConditionPassed());
            Assert.IsTrue(checker.CheckIfConditionPassed());
        }

        /// <summary>
        /// Assert that condition fails if vector doesn't criteria.
        /// </summary>
        [TestMethod]
        public void CheckIfConditionPassed_Fails()
        {
            string conditions = "P002,0-1,00|P002,3,1";
            string conditions2 = "P002,0-1,00|P002,3,1|NOA1,0-6,01110000";

            // X's in string work as "don't cares"
            Dictionary<string, string> ctvData = new Dictionary<string, string>()
            {
                { "P002", "XXXXXXXX" },
                { "NOA1", "XXXXXXXX" },
            };

            var checker = new HryConditionsChecker(ctvData, conditions);
            var checker2 = new HryConditionsChecker(ctvData, conditions2);

            Assert.IsFalse(checker.CheckIfConditionPassed());
            Assert.IsFalse(checker.CheckIfConditionPassed());
        }

        /// <summary>
        /// Make sure that an Argument Exception is thrown if there are less than 3 arguments in a condition.
        /// </summary>
        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void CheckIfConditionPassed_LessThanThreeArgs()
        {
            string conditions = "P002,0-1";

            // X's in string work as "don't cares"
            Dictionary<string, string> ctvData = new Dictionary<string, string>()
            {
                { "P002", "00X100X1" },
                { "NOA1", "01110000" },
            };

            var checker = new HryConditionsChecker(ctvData, conditions);
            checker.CheckIfConditionPassed();
        }

        /// <summary>
        /// Make sure that an Argument Exception is thrown if there are more than 3 arguments in a condition.
        /// </summary>
        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void CheckIfConditionPassed_MoreThanThreeArgs()
        {
            string conditions = "P002,0-1,00,ExtraArg";

            // X's in string work as "don't cares"
            Dictionary<string, string> ctvData = new Dictionary<string, string>()
            {
                { "P002", "00X100X1" },
                { "NOA1", "01110000" },
            };

            var checker = new HryConditionsChecker(ctvData, conditions);
            checker.CheckIfConditionPassed();
        }

        /// <summary>
        /// Verify return indexes succeeds when given a valid string.
        /// </summary>
        [TestMethod]
        public void ReturnIndexes_Success()
        {
            string range1 = "10-15";
            List<int> range1List = SharedFunctions.ParseRange(range1);
            List<int> range1Validation = new List<int>() { 10, 11, 12, 13, 14, 15 };

            string range2 = "11-9:20-21";
            List<int> range2List = SharedFunctions.ParseRange(range2);
            List<int> range2Validation = new List<int>() { 11, 10, 9, 20, 21 };

            string range3 = "45";
            List<int> range3List = SharedFunctions.ParseRange(range3);
            List<int> range3Validation = new List<int>() { 45 };

            for (int i = 0; i < range1Validation.Count; i++)
            {
                Assert.AreEqual(range1Validation[i], range1List[i]);
            }

            for (int i = 0; i < range2Validation.Count; i++)
            {
                Assert.AreEqual(range2Validation[i], range2List[i]);
            }

            for (int i = 0; i < range3Validation.Count; i++)
            {
                Assert.AreEqual(range3Validation[i], range3List[i]);
            }
        }

        /// <summary>
        /// Verify return indexes fails when given an invalid string.
        /// </summary>
        [ExpectedException(typeof(FormatException))]
        [TestMethod]
        public void ReturnIndexes_FormatException()
        {
            string range1 = "hello :)";
            SharedFunctions.ParseRange(range1);
        }

        /// <summary>
        /// Verify return indexes fails when given a null string.
        /// </summary>
        [ExpectedException(typeof(OverflowException))]
        [TestMethod]
        public void ReturnIndexes_OverflowException()
        {
            string range1 = "9999999999999999999999999";
            SharedFunctions.ParseRange(range1);
        }

        /// <summary>
        /// Assert that condition passes if vector meets criteria.
        /// </summary>
        [TestMethod]
        public void ConditionCheckerHrydatapassed_returnpinlist_Success()
        {
            string validHryTableInputXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?> <HSR_HRY_config>  <ReverseCtvCaptureData>true</ReverseCtvCaptureData>  <CtvToHryMapping> 	<Map ctv_data=\"0\" hry_data=\"0\" /> 	<Map ctv_data=\"1\" hry_data=\"1\" /> </CtvToHryMapping>  <Criterias> 	<Criteria hry_index=\"0\"  pin=\"P001\" ctv_index_range=\"2\"  condition=\"P002,0-1,00|P002,3,1\"    hry_output_on_condition_fail=\"8\" bypass_global=\"HSR.HRY_Global_1\" /> 	<Criteria hry_index=\"1\"  pin=\"P001\" ctv_index_range=\"6\"  condition=\"P002,4-5,00|P002,7,1\"    hry_output_on_condition_fail=\"8\" bypass_global=\"HSR.HRY_Global_1\" /> </Criterias>  <Algorithms> 	<Algorithm index=\"0\" name=\"SCAN\"    pat_modify_label=\"\" ctv_size=\"36\" /> 	<Algorithm index=\"1\" name=\"PMOVI\"   pat_modify_label=\"\" ctv_size=\"36\" /> 	<Algorithm index=\"2\" name=\"March-C\" pat_modify_label=\"\" ctv_size=\"36\" /> </Algorithms>  </HSR_HRY_config>";
            var input = InputFactory.CreateConfigHandler(validHryTableInputXML, InputFactory.FileType.XML);
            var deserializedHryTable = input.DeserializeInput<HryTableConfigXml>();

            List<HryTableConfigXml.CriteriaObject> criterias = deserializedHryTable.GetCriterias();
            var checker = new HryConditionsChecker(criterias);
            var pinHash = checker.GetListofPinsToMonitor();
            var pinList = pinHash.ToList();

            // Output of the function call should be a list of pins
            var expectedOutput = new List<string>()
                    {
                        "P001",
                        "P002",
                    };

            for (int i = 0; i < expectedOutput.Count; i++)
            {
                Assert.AreEqual(expectedOutput[i], pinList[i]);
            }
        }
    }
}
