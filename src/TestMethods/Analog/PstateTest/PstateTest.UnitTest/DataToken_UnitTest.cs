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

namespace PstateTest.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit test collection to test VFCurves class.
    /// </summary>
    [TestClass]
    public class DataToken_UnitTest
    {
        /// <summary>
        /// AddDataSpec() passes.
        /// </summary>
        [TestMethod]
        public void DataToken_AddDataSpec_True()
        {
            // arrange
            string name = "VPU";
            var dt = new DataTokens(name);

            string field = "lock_time";
            var positions = new List<Tuple<int, int>> { new Tuple<int, int>(13, 4) };
            string form = string.Empty;
            List<Tuple<string, int>> lims = new List<Tuple<string, int>>
            {
                new Tuple<string, int>(">", 0),
                new Tuple<string, int>("<", 512),
            };

            int index = 0;

            // act
            bool status = dt.AddDataSpec(name, field, positions, form, lims, index);

            // assert
            Assert.IsTrue(status);
        }

        /// <summary>
        /// DecodeCapData() passes.
        /// </summary>
        [TestMethod]
        public void DataToken_DecodeCapDataPassLimits_True()
        {
            // arrange
            string name = "VPU";
            string field = "lock_time";
            var positions = new List<Tuple<int, int>> { new Tuple<int, int>(13, 4) };
            string form = string.Empty;
            List<Tuple<string, int>> lims = new List<Tuple<string, int>>
            {
                new Tuple<string, int>(">", 0),
                new Tuple<string, int>("<", 512),
            };

            int index = 0;
            var dt = new DataTokens(name);
            bool addStatus = dt.AddDataSpec(name, field, positions, form, lims, index + 2);
            addStatus = dt.AddDataSpec(
                name,
                "raw_lock",
                new List<Tuple<int, int>> { new Tuple<int, int>(2, -1) },
                form,
                new List<Tuple<string, int>> { new Tuple<string, int>("==", 1) },
                index + 1);
            addStatus = dt.AddDataSpec(
                name,
                "pll_enable",
                new List<Tuple<int, int>> { new Tuple<int, int>(0, -1) },
                form,
                new List<Tuple<string, int>> { new Tuple<string, int>("==", 1) },
                index);

            string capData = "10101111000000000000000000000000";
            dt.AddCapLen(name, capData.Length);
            var passFailVec = new List<bool>();
            var datalogStrings = new List<string>();

            // act
            bool status = dt.DecodeCapData(name, 0, 32, ref capData, out passFailVec, out datalogStrings);

            // assert
            Assert.IsTrue(addStatus);
            Assert.IsTrue(status);
            Assert.AreEqual(1, passFailVec.Count);
            Assert.AreEqual(1, datalogStrings.Count);
            Assert.IsFalse(passFailVec.Contains(false));
        }

        /// <summary>
        /// DecodeCapData() passes.
        /// </summary>
        [TestMethod]
        public void DataToken_DecodeCapDataFailsLimit_True()
        {
            // arrange
            string name = "VPU";
            string field = "lock_time";
            var positions = new List<Tuple<int, int>> { new Tuple<int, int>(13, 4) };
            string form = string.Empty;
            List<Tuple<string, int>> lims = new List<Tuple<string, int>>
            {
                new Tuple<string, int>(">", 0),
                new Tuple<string, int>("<", 15),
            };

            int index = 0;
            var dt = new DataTokens(name);
            bool addStatus = dt.AddDataSpec(name, field, positions, form, lims, index + 2);
            addStatus = dt.AddDataSpec(
                name,
                "raw_lock",
                new List<Tuple<int, int>> { new Tuple<int, int>(2, -1) },
                form,
                new List<Tuple<string, int>> { new Tuple<string, int>("==", 1) },
                index + 1);
            addStatus = dt.AddDataSpec(
                name,
                "pll_enable",
                new List<Tuple<int, int>> { new Tuple<int, int>(0, -1) },
                form,
                new List<Tuple<string, int>> { new Tuple<string, int>("==", 1) },
                index);

            string capData = "1010111000000000000000000000000010101111000000000000000000000000";
            dt.AddCapLen(name, 32);
            var passFailVec = new List<bool>();
            var datalogStrings = new List<string>();

            // act
            bool status = dt.DecodeCapData(name, 0, capData.Length, ref capData, out passFailVec, out datalogStrings);

            // assert
            Assert.IsTrue(addStatus);
            Assert.IsTrue(status);
            Assert.AreEqual(2, passFailVec.Count);
            Assert.AreEqual(2, datalogStrings.Count);
            Assert.IsTrue(passFailVec.Contains(false));
        }
    }
}
