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
namespace VF.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PstateTest;

    /// <summary>
    /// Unit test collection to test VF test class.
    /// </summary>
    [TestClass]
    public class VF_UnitTest
    {
        /// <summary>
        /// Constructor executes w/ valid args.
        /// </summary>
        [TestMethod]
        public void VF_ValidArgs_True()
        {
            // arrange
            string d = "SA";
            string n = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1050;
            int maxF = 2100;
            float incF = 50f;
            float freqGB = 0.1f;
            int midF = (int)Math.Round((Math.Round((maxF - minF) / incF / 2.0f) * incF) + minF);
            List<int> pf = new List<int> { minF, midF, maxF };
            List<float> pv = new List<float> { minV, minV + ((maxV - minV) / 2), maxV };

            // act
            var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            // assert
            Assert.AreEqual(26, vf.TestFreqs.Count);
            Assert.AreEqual(2300, vf.TestFreqs.Last());
            Assert.AreEqual(0.5f, vf.TestVolts[0]);
            Assert.AreEqual(1.2f, vf.TestVolts.Last());
            Assert.AreEqual(1.2f, vf.TestVolts[vf.TestFreqs.IndexOf(maxF)]);
        }

        /// <summary>
        /// Constructor executes w/ too few test points.
        /// </summary>
        [TestMethod]
        public void VF_OnlyOnePoint_False()
        {
            // arrange
            string d = "SA";
            string n = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1050;
            int maxF = 2100;
            float incF = 50f;
            float freqGB = 0.1f;
            List<int> pf = new List<int> { minF };
            List<float> pv = new List<float> { minV };
            string message = string.Empty;

            // act
            try
            {
                var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv);
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            // assert
            Assert.AreEqual("There must be at least 2 points to fit a line!", message);
        }

        /// <summary>
        /// Constructor executes w/ valid args.
        /// </summary>
        [TestMethod]
        public void VF_ValidArgsTwoPoints_True()
        {
            // arrange
            string d = "SA";
            string n = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1050;
            int maxF = 2100;
            float incF = 50f;
            float freqGB = 0.1f;
            List<int> pf = new List<int> { minF, maxF };
            List<float> pv = new List<float> { minV, maxV };

            // act
            var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            // assert
            Assert.AreEqual(26, vf.TestFreqs.Count);
            Assert.AreEqual(2300, vf.TestFreqs.Last());
            Assert.AreEqual(0.5f, vf.TestVolts[0]);
            Assert.AreEqual(1.2f, vf.TestVolts.Last());
            Assert.AreEqual(1.2f, vf.TestVolts[vf.TestFreqs.IndexOf(maxF)]);
        }

        /// <summary>
        /// Constructor executes w/ equal voltages (flat line).
        /// </summary>
        [TestMethod]
        public void VF_ValidArgsFlatLine_True()
        {
            // arrange
            string d = "SA";
            string n = "VPU";
            float minV = 0.75f;
            float maxV = 0.75f;
            int minF = 1050;
            int maxF = 2100;
            float incF = 50f;
            float freqGB = 0.1f;
            int midF = (int)Math.Round((Math.Round((maxF - minF) / incF / 2.0f) * incF) + minF);
            List<int> pf = new List<int> { minF, midF, maxF };
            List<float> pv = new List<float> { minV, minV + ((maxV - minV) / 2), maxV };

            // act
            var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            // assert
            Assert.AreEqual(26, vf.TestFreqs.Count);
            Assert.AreEqual(2300, vf.TestFreqs.Last());
            Assert.AreEqual(0.75f, vf.TestVolts[0]);
            Assert.AreEqual(0.75f, vf.TestVolts.Last());
            Assert.AreEqual(0.75f, vf.TestVolts[vf.TestFreqs.IndexOf(maxF)]);
        }

        /// <summary>
        /// Constructor executes w/ voltages below/ above min/ max.
        /// </summary>
        [TestMethod]
        public void VF_ValidArgsVoltageOutOfRange_True()
        {
            // arrange
            string d = "SA";
            string n = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1050;
            int maxF = 2100;
            float incF = 50f;
            float freqGB = 0.1f;
            int midF = (int)Math.Round((Math.Round((maxF - minF) / incF / 2.0f) * incF) + minF);
            List<int> pf = new List<int> { minF, midF, maxF };
            List<float> pv = new List<float> { minV - 0.05f, minV + ((maxV - minV) / 2), maxV + 0.05f };

            // act
            var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            // assert
            Assert.AreEqual(26, vf.TestFreqs.Count);
            Assert.AreEqual(2300, vf.TestFreqs.Last());
            Assert.AreEqual(0.5f, vf.TestVolts[0]);
            Assert.AreEqual(1.2f, vf.TestVolts.Last());
            Assert.AreEqual(1.2f, vf.TestVolts[vf.TestFreqs.IndexOf(maxF)]);
        }

        /// <summary>
        /// Constructor executes w/ Qclk gear 2 freqs.
        /// </summary>
        [TestMethod]
        public void VF_ValidArgsQclkGear2Subset_True()
        {
            // arrange
            string d = "SA";
            string n = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1600;
            int maxF = 2700;
            float incF = 33.33f;
            float freqGB = 0.1f;
            int midF = (int)Math.Round((Math.Round((maxF - minF) / incF / 2.0f) * incF) + minF);
            List<int> pf = new List<int> { minF, midF, maxF };
            List<float> pv = new List<float> { minV, minV + ((maxV - minV) / 2), maxV };

            // act
            var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            // assert
            Assert.AreEqual(42, vf.TestFreqs.Count);
            Assert.AreEqual(2967, vf.TestFreqs.Last());
            Assert.AreEqual(0.5f, vf.TestVolts[0]);
            Assert.AreEqual(1.2f, vf.TestVolts.Last());
            Assert.AreEqual(1.2f, vf.TestVolts[vf.TestFreqs.IndexOf(maxF)]);
        }

        /// <summary>
        /// Constructor executes w/ Qclk gear 2 freqs.
        /// </summary>
        [TestMethod]
        public void VF_ValidArgsQclkGear2Full_True()
        {
            // arrange
            string d = "SA";
            string n = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1600;
            int maxF = 3133;
            float incF = 33.33f;
            float freqGB = 0.1f;
            int midF = (int)Math.Round((Math.Round((maxF - minF) / incF / 2.0f) * incF) + minF);
            List<int> pf = new List<int> { minF, midF, maxF };
            List<float> pv = new List<float> { minV, minV + ((maxV - minV) / 2), maxV };
            List<int> tf = new List<int>();

            // create contiguous subset of freqs
            IEnumerable<int> testFreqs = Enumerable.Range(0, (int)Math.Round((2700 - minF) / incF) + 1).Select(x => minF + (int)Math.Round(x * incF));
            tf.AddRange(testFreqs.ToList());
            tf.AddRange(new List<int> { 2733, 2800, 2867, 2933, 3000, 3067 });
            float maxFGB = maxF * (1.0f + freqGB);
            int newMaxF = (int)Math.Round((maxFGB / incF) * incF);
            testFreqs = Enumerable.Range(0, (int)Math.Round((newMaxF - maxF) / incF) + 1).Select(x => maxF + (int)Math.Round(x * incF));
            tf.AddRange(testFreqs.ToList());

            // act
            var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv, tf);

            // assert
            Assert.AreEqual(50, vf.TestFreqs.Count);
            Assert.AreEqual(3433, vf.TestFreqs.Last());
            Assert.AreEqual(0.5f, vf.TestVolts[0]);
            Assert.AreEqual(1.2f, vf.TestVolts.Last());
            Assert.AreEqual(1.2f, vf.TestVolts[vf.TestFreqs.IndexOf(maxF)]);
            Assert.AreEqual(0.85f, vf.TestVolts[vf.TestFreqs.IndexOf(midF)]);
        }

        /// <summary>
        /// Constructor executes w/ Qclk gear 2 freqs.
        /// </summary>
        [TestMethod]
        public void VF_ValidArgsQclkGear4_True()
        {
            // arrange
            string d = "SA";
            string n = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1600;
            int maxF = 2700;
            float incF = 33.33f;
            float freqGB = 0.1f;
            int midF = (int)Math.Round((Math.Round((maxF - minF) / incF / 2.0f) * incF) + minF);
            List<int> pf = new List<int> { minF, midF, maxF };
            List<float> pv = new List<float> { minV, minV + ((maxV - minV) / 2), maxV };

            // act
            var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            // assert
            Assert.AreEqual(42, vf.TestFreqs.Count);
            Assert.AreEqual(2967, vf.TestFreqs.Last());
            Assert.AreEqual(0.5f, vf.TestVolts[0]);
            Assert.AreEqual(1.2f, vf.TestVolts.Last());
            Assert.AreEqual(1.2f, vf.TestVolts[vf.TestFreqs.IndexOf(maxF)]);
        }

        /// <summary>
        /// Constructor executes w/ test freqs are below test points.
        /// </summary>
        [TestMethod]
        public void VF_ValidArgsFreqsBelowTestPoints_True()
        {
            // arrange
            string message = string.Empty;
            string d = "SA";
            string n = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1050;
            int maxF = 2100;
            float incF = 50f;
            float freqGB = 0.1f;
            int midF = (int)Math.Round((Math.Round((maxF - minF) / incF / 2.0f) * incF) + minF);
            List<int> pf = new List<int> { 2500, 2700, 2900 };
            List<float> pv = new List<float> { 0.75f, 0.85f, 1.0f };

            // act
            var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            // assert
            Assert.AreEqual(26, vf.TestFreqs.Count);
            Assert.AreEqual(2300, vf.TestFreqs.Last());
            Assert.AreEqual(0.5f, vf.TestVolts[0]);
            Assert.AreEqual(0.5f, vf.TestVolts[vf.TestFreqs.IndexOf(1200)]);
            Assert.AreEqual(0.65f, vf.TestVolts.Last());
            Assert.AreEqual(0.65f, vf.TestVolts[vf.TestFreqs.IndexOf(2300)]);
        }

        /// <summary>
        /// Constructor executes w/ test freqs are above test points.
        /// </summary>
        [TestMethod]
        public void VF_ValidArgsFreqsAboveTestPoints_True()
        {
            // arrange
            string message = string.Empty;
            string d = "SA";
            string n = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1050;
            int maxF = 2100;
            float incF = 50f;
            float freqGB = 0.1f;
            List<int> pf = new List<int> { 600, 800, 1000 };
            List<float> pv = new List<float> { 0.055f, 0.6375f, 0.9f };

            // act
            var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            // assert
            // Assert.AreEqual("There must be at least 2 points to fit a line!", message);
            Assert.AreEqual(26, vf.TestFreqs.Count);
            Assert.AreEqual(2300, vf.TestFreqs.Last());
            Assert.AreEqual(0.966f, vf.TestVolts[0]);
            Assert.AreEqual(1.162f, vf.TestVolts[vf.TestFreqs.IndexOf(1200)]);
            Assert.AreEqual(1.2f, vf.TestVolts.Last());
            Assert.AreEqual(1.2f, vf.TestVolts[vf.TestFreqs.IndexOf(maxF)]);
        }

        /// <summary>
        /// flat voltage curve.
        /// </summary>
        [TestMethod]
        public void VF_ValidArgsFlatVFSegment_True()
        {
            // arrange
            string d = "SA";
            string n = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1600;
            int maxF = 2700;
            float incF = 33.33f;
            float freqGB = 0.1f;
            int midF = (int)Math.Round((Math.Round((maxF - minF) / incF / 2.0f) * incF) + minF);
            List<int> pf = new List<int> { minF, midF, maxF };
            List<float> pv = new List<float> { minV + 0.1f, minV + 0.1f, maxV };

            // act
            var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            // assert
            Assert.AreEqual(42, vf.TestFreqs.Count);
            Assert.AreEqual(2967, vf.TestFreqs.Last());
            Assert.AreEqual(0.6f, vf.TestVolts[0]);
            Assert.AreEqual(1.2f, vf.TestVolts.Last());
            Assert.AreEqual(1.2f, vf.TestVolts[vf.TestFreqs.IndexOf(maxF)]);
        }

        /// <summary>
        /// non-monotonic test voltages.
        /// </summary>
        [TestMethod]
        public void VF_ValidArgsNonMonotonicVF_True()
        {
            // arrange
            string d = "SA";
            string n = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1600;
            int maxF = 2700;
            float incF = 33.33f;
            float freqGB = 0.1f;
            int midF = (int)Math.Round((Math.Round((maxF - minF) / incF / 2.0f) * incF) + minF);
            List<int> pf = new List<int> { minF, midF, maxF };
            List<float> pv = new List<float> { minV, maxV, minV };

            // act
            var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            // assert
            Assert.AreEqual(42, vf.TestFreqs.Count);
            Assert.AreEqual(2967, vf.TestFreqs.Last());
            Assert.AreEqual(0.5f, vf.TestVolts[0]);
            Assert.AreEqual(0.5f, vf.TestVolts.Last());
            Assert.AreEqual(0.5f, vf.TestVolts[vf.TestFreqs.IndexOf(maxF)]);
        }

        /// <summary>
        /// Fn+1 == Fn.
        /// </summary>
        [TestMethod]
        public void VF_ValidArgsEqualFreqs_True()
        {
            // arrange
            string d = "SA";
            string n = "VPU";
            float minV = 0.5f;
            float maxV = 1.2f;
            int minF = 1600;
            int maxF = 2700;
            float incF = 33.33f;
            float freqGB = 0.1f;
            int midF = (int)Math.Round((Math.Round((maxF - minF) / incF / 2.0f) * incF) + minF);
            List<int> pf = new List<int> { minF, minF, maxF };
            List<float> pv = new List<float> { minV, minV, maxV };

            // act
            var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv);

            // assert
            Assert.AreEqual(42, vf.TestFreqs.Count);
            Assert.AreEqual(2967, vf.TestFreqs.Last());
            Assert.AreEqual(0.5f, vf.TestVolts[0]);
            Assert.AreEqual(1.2f, vf.TestVolts.Last());
            Assert.AreEqual(1.2f, vf.TestVolts[vf.TestFreqs.IndexOf(maxF)]);
        }

        // test point voltage above maxV

        // test point voltage below minV

        // decreasing voltage

        // UPS token reversed
    }
}