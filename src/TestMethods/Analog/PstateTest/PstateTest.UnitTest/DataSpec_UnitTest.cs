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

namespace DataSpec.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PstateTest;

    /// <summary>
    /// Unit test collection to test DataSpec test class.
    /// </summary>
    [TestClass]
    public class DataSpec_UnitTest
    {
        /// <summary>
        /// Constructor executes w/ valid args.
        /// </summary>
        [TestMethod]
        public void DataSpec_ValidArgs_True()
        {
            // arrange
            string name = "lock_time";
            List<Tuple<int, int>> positions = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(9, 1),
            };
            string transform = @"Convert.ToInt32(x,2)";
            List<Tuple<string, int>> limits = new List<Tuple<string, int>>
            {
                new Tuple<string, int>(">", 0),
                new Tuple<string, int>("<", 100),
            };
            int datalogIndex = 0;

            // act
            var lockTime = new DataSpec(name, positions, transform, limits, datalogIndex);

            // assert
            Assert.AreEqual(10, lockTime.Length());
        }

        /// <summary>
        /// ExtractAssemble with invalid capture data.
        /// </summary>
        [TestMethod]
        public void DataSpec_ExtractAssembleInvalidArgs_False()
        {
            // arrange
            string capData = string.Empty;
            string fieldData = string.Empty;
            string name = "lock_time";
            List<Tuple<int, int>> positions = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(9, 1),
            };
            string transform = @"Convert.ToInt32(x,2)";
            List<Tuple<string, int>> limits = new List<Tuple<string, int>>
            {
                new Tuple<string, int>(">", 0),
                new Tuple<string, int>("<", 100),
            };
            int datalogIndex = 0;
            var lockTime = new DataSpec(name, positions, transform, limits, datalogIndex);

            // act
            var result = lockTime.ExtractAssemble(capData, out fieldData);

            // assert
            Assert.IsFalse(result);
            Assert.AreEqual(string.Empty, fieldData);
        }

        /// <summary>
        /// ExtractAssemble with short capture data.
        /// </summary>
        [TestMethod]
        public void DataSpec_ExtractAssembleShortCapData_False()
        {
            // arrange
            string capData = "01110010";
            string fieldData = string.Empty;
            string name = "lock_time";
            List<Tuple<int, int>> positions = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(9, 1),
            };
            string transform = @"Convert.ToInt32(x,2)";
            List<Tuple<string, int>> limits = new List<Tuple<string, int>>
            {
                new Tuple<string, int>(">", 0),
                new Tuple<string, int>("<", 100),
            };
            int datalogIndex = 0;
            var lockTime = new DataSpec(name, positions, transform, limits, datalogIndex);

            // act
            var result = lockTime.ExtractAssemble(capData, out fieldData);

            // assert
            Assert.IsFalse(result);
            Assert.AreEqual(string.Empty, fieldData);
        }

        /// <summary>
        /// ExtractAssemble with valid capture data.
        /// </summary>
        [TestMethod]
        public void DataSpec_ExtractAssembleValidArgs_True()
        {
            // arrange
            string capData = "0111001000";
            string fieldData = string.Empty;
            string name = "lock_time";
            List<Tuple<int, int>> positions = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(9, 1),
            };
            string transform = @"Convert.ToInt32(x,2)";
            List<Tuple<string, int>> limits = new List<Tuple<string, int>>
            {
                new Tuple<string, int>(">", 0),
                new Tuple<string, int>("<", 100),
            };
            int datalogIndex = 0;
            var lockTime = new DataSpec(name, positions, transform, limits, datalogIndex);

            // act
            var result = lockTime.ExtractAssemble(capData, out fieldData);

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("0000100111", fieldData);
        }

        /// <summary>
        /// TransformBits with invalid bit string.
        /// </summary>
        [TestMethod]
        public void DataSpec_TransformInvalidArgs_False()
        {
            // arrange
            string bitString = string.Empty;
            string fieldData = string.Empty;
            string name = "lock_time";
            List<Tuple<int, int>> positions = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(9, 1),
            };
            string transform = @"Convert.ToInt32(x,2)";
            List<Tuple<string, int>> limits = new List<Tuple<string, int>>
            {
                new Tuple<string, int>(">", 0),
                new Tuple<string, int>("<", 100),
            };
            int datalogIndex = 0;
            var lockTime = new DataSpec(name, positions, transform, limits, datalogIndex);

            // act
            var result = lockTime.TransformBits(bitString, out fieldData);

            // assert
            Assert.IsFalse(result);
            Assert.AreEqual(string.Empty, fieldData);
        }

        /// <summary>
        /// TransformBits with valid bit string.
        /// </summary>
        [TestMethod]
        public void DataSpec_TransformValidArgs_True()
        {
            // arrange
            string bitString = "0000100111";
            string fieldData = string.Empty;
            string name = "lock_time";
            List<Tuple<int, int>> positions = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(9, 1),
            };
            string transform = @"Convert.ToInt32(x,2)";
            List<Tuple<string, int>> limits = new List<Tuple<string, int>>
            {
                new Tuple<string, int>(">", 0),
                new Tuple<string, int>("<", 100),
            };
            int datalogIndex = 0;
            var lockTime = new DataSpec(name, positions, transform, limits, datalogIndex);

            // act
            var result = lockTime.TransformBits(bitString, out fieldData);

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("39", fieldData);
            Assert.AreEqual(39, Convert.ToInt32(fieldData));
        }

        /// <summary>
        /// ApplyLimits with valid bit string.
        /// </summary>
        [TestMethod]
        public void DataSpec_ApplyLimitsEmptyValue_False()
        {
            // arrange
            string fieldValue = string.Empty;
            bool comparison = false;
            string name = "lock_time";
            List<Tuple<int, int>> positions = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(9, 1),
            };
            string transform = @"Convert.ToInt32(x,2)";
            List<Tuple<string, int>> limits = new List<Tuple<string, int>>
            {
                new Tuple<string, int>(">", 0),
                new Tuple<string, int>("<", 100),
            };
            int datalogIndex = 0;
            var lockTime = new DataSpec(name, positions, transform, limits, datalogIndex);

            // var result = lockTime.TransformBits(bitString, out fieldData);

            // act
            var result = lockTime.ApplyLimits(fieldValue, out comparison);

            // assert
            Assert.IsFalse(result);
            Assert.IsFalse(comparison);
        }

        /// <summary>
        /// ApplyLimits with valid field value and pass limits.
        /// </summary>
        [TestMethod]
        public void DataSpec_ApplyLimitsPassLimits_True()
        {
            // arrange
            string fieldValue = "39";
            bool comparison = false;
            string name = "lock_time";
            List<Tuple<int, int>> positions = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(9, 1),
            };
            string transform = @"Convert.ToInt32(x,2)";
            List<Tuple<string, int>> limits = new List<Tuple<string, int>>
            {
                new Tuple<string, int>(">", 0),
                new Tuple<string, int>("<", 100),
            };
            int datalogIndex = 0;
            var lockTime = new DataSpec(name, positions, transform, limits, datalogIndex);

            // var result = lockTime.TransformBits(bitString, out fieldData);

            // act
            var result = lockTime.ApplyLimits(fieldValue, out comparison);

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(comparison);
        }

        /// <summary>
        /// ApplyLimits with valid field value but fail limits.
        /// </summary>
        [TestMethod]
        public void DataSpec_ApplyLimitsFailLimits_True()
        {
            // arrange
            string fieldValue = "139";
            bool comparison = false;
            string name = "lock_time";
            List<Tuple<int, int>> positions = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(9, 1),
            };
            string transform = @"Convert.ToInt32(x,2)";
            List<Tuple<string, int>> limits = new List<Tuple<string, int>>
            {
                new Tuple<string, int>(">", 0),
                new Tuple<string, int>("<", 100),
            };
            int datalogIndex = 0;
            var lockTime = new DataSpec(name, positions, transform, limits, datalogIndex);

            // var result = lockTime.TransformBits(bitString, out fieldData);

            // act
            var result = lockTime.ApplyLimits(fieldValue, out comparison);

            // assert
            Assert.IsTrue(result);
            Assert.IsFalse(comparison);
        }

        /// <summary>
        /// ApplyLimits with valid field value but fail limits.
        /// </summary>
        [TestMethod]
        public void DataSpec_ApplyLimitsInvalidValue_False()
        {
            // arrange
            string fieldValue = "philip";
            bool comparison = false;
            string name = "lock_time";
            List<Tuple<int, int>> positions = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(9, 1),
            };
            string transform = @"Convert.ToInt32(x,2)";
            List<Tuple<string, int>> limits = new List<Tuple<string, int>>
            {
                new Tuple<string, int>(">", 0),
                new Tuple<string, int>("<", 100),
            };
            int datalogIndex = 0;
            var lockTime = new DataSpec(name, positions, transform, limits, datalogIndex);

            // var result = lockTime.TransformBits(bitString, out fieldData);

            // act
            var result = lockTime.ApplyLimits(fieldValue, out comparison);

            // assert
            Assert.IsFalse(result);
            Assert.IsFalse(comparison);
        }
    }
}
