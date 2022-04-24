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
    public class VFCurves_UnitTest
    {
        /// <summary>
        /// AddVF() fails when duplicate inst provided.
        /// </summary>
        [TestMethod]
        public void VFCurves_AddVFDuplicateKey_False()
        {
            // arrange
            string domain = "SA";
            string name = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1100;
            int maxF = 2100;
            float incF = 50.0f;
            float freqGB = 0.1f;
            var pf = new List<int> { 1100, 1500, 2100 };
            var pv = new List<float> { 0.5f, 0.85f, 1.2f };

            var curves = new VFCurves(domain);

            bool status = true;

            // act
            status = curves.AddVF(domain, name, minV, maxV, minF, maxF, incF, freqGB, pf, pv);
            status = curves.AddVF(domain, name, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            // assert
            Assert.IsFalse(status);
        }

        /// <summary>
        /// AddVF() fails when duplicate inst provided.
        /// </summary>
        [TestMethod]
        public void VFCurves_AddVF_True()
        {
            // arrange
            string domain = "SA";
            string name1 = "VPU";
            string name2 = "NoC";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1100;
            int maxF = 2100;
            float incF = 50.0f;
            float freqGB = 0.1f;
            var pf = new List<int> { 1100, 1500, 2100 };
            var pv = new List<float> { 0.5f, 0.85f, 1.2f };

            var curves = new VFCurves(domain);

            bool status = true;

            // act
            status = curves.AddVF(domain, name1, minV, maxV, minF, maxF, incF, freqGB, pf, pv);
            if (status)
            {
                status = curves.AddVF(domain, name2, minV, maxV, minF, maxF, incF, freqGB, pf, pv);
            }

            // assert
            Assert.IsTrue(status);
        }

        /// <summary>
        /// UpdateVF() passes.
        /// </summary>
        [TestMethod]
        public void VFCurves_UpdateVFUnequalListSize_False()
        {
            // arrange
            string domain = "SA";
            string name1 = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1100;
            int maxF = 2100;
            float incF = 50.0f;
            float freqGB = 0.1f;
            var pf = new List<int> { 1100, 1500, 2100 };
            var pv = new List<float> { 0.5f, 0.85f, 1.2f };

            var curves = new VFCurves(domain);

            bool addStatus = true;
            bool updateStatus = true;
            addStatus = curves.AddVF(domain, name1, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            var pf2 = new List<int> { 1600, 2400, 3600 };
            var pv2 = new List<float> { 0.65f, 0.95f };

            // act
            updateStatus = curves.UpdateVF(pf2, pv2);

            // assert
            Assert.IsTrue(addStatus);
            Assert.IsFalse(updateStatus);
        }

        /// <summary>
        /// UpdateVF() passes.
        /// </summary>
        [TestMethod]
        public void VFCurves_UpdateVF_True()
        {
            // arrange
            string domain = "SA";
            string name1 = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1100;
            int maxF = 2100;
            float incF = 50.0f;
            float freqGB = 0.1f;
            var pf = new List<int> { 1100, 1500, 2100 };
            var pv = new List<float> { 0.5f, 0.85f, 1.2f };

            var curves = new VFCurves(domain);

            bool addStatus = true;
            bool updateStatus = true;
            addStatus = curves.AddVF(domain, name1, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            var pf2 = new List<int> { 1600, 2400, 3600 };
            var pv2 = new List<float> { 0.65f, 0.95f, 1.1f };

            // act
            updateStatus = curves.UpdateVF(pf2, pv2);

            // assert
            Assert.IsTrue(addStatus);
            Assert.IsTrue(updateStatus);
        }
    }
}
