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

namespace PstateTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// class to contain a single VF curve for a given domain and IP.
    /// </summary>
    public class VF
    {
        private string domain;
        private string name;
        private float minV;
        private float maxV;
        private int minF;
        private int maxF;
        private float incF;
        private float freqGB;
        private List<int> pointFreqs;
        private List<float> pointVolts;
        private List<int> testFreqs;
        private List<float> testVolts;

        /// <summary>
        /// Initializes a new instance of the <see cref="VF"/> class.
        /// </summary>
        /// <param name="d">domain name.</param>
        /// <param name="n">instance name.</param>
        /// <param name="minV">min voltage.</param>
        /// <param name="maxV">max voltage.</param>
        /// <param name="minF">min freq.</param>
        /// <param name="maxF">max freq.</param>
        /// <param name="incF">freq bins.</param>
        /// <param name="freqGB">freq guardband.</param>
        /// <param name="pf">list of int.</param>
        /// <param name="pv">list of float.</param>
        public VF(string d, string n, float minV, float maxV, int minF, int maxF, float incF, float freqGB, List<int> pf, List<float> pv)
        {
            this.domain = d;
            this.name = n;
            this.minV = minV;
            this.maxV = maxV;
            this.minF = minF;
            this.maxF = maxF;
            this.incF = incF;
            this.freqGB = freqGB;
            this.pointFreqs = pf;
            this.pointVolts = pv;
            this.testFreqs = new List<int>();
            this.testVolts = new List<float>();

            // set superset of freqs
            float maxFGB = maxF * (1.0f + freqGB);
            int newMaxF = (int)Math.Round((maxFGB / incF) * incF);
            IEnumerable<int> testFreqs = Enumerable.Range(0, (int)Math.Round((newMaxF - minF) / incF) + 1).Select(x => minF + (int)Math.Round(x * incF));
            this.testFreqs = testFreqs.ToList();

            // update VF curve
            this.Update(this.pointFreqs, this.pointVolts);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VF"/> class.
        /// </summary>
        /// <param name="d">domain name.</param>
        /// <param name="n">instance name.</param>
        /// <param name="minV">min voltage.</param>
        /// <param name="maxV">max voltage.</param>
        /// <param name="minF">min freq.</param>
        /// <param name="maxF">max freq.</param>
        /// <param name="incF">freq bins.</param>
        /// <param name="freqGB">freq guardband.</param>
        /// <param name="pf">list of int.</param>
        /// <param name="pv">list of float.</param>
        /// <param name="tf">list of test freqs.</param>
        public VF(string d, string n, float minV, float maxV, int minF, int maxF, float incF, float freqGB, List<int> pf, List<float> pv, List<int> tf)
        {
            this.domain = d;
            this.name = n;
            this.minV = minV;
            this.maxV = maxV;
            this.minF = minF;
            this.maxF = maxF;
            this.incF = incF;
            this.freqGB = freqGB;
            this.pointFreqs = pf;
            this.pointVolts = pv;
            this.testFreqs = tf;
            this.testVolts = new List<float>();

            // update VF curve
            this.Update(this.pointFreqs, this.pointVolts);
        }

        /// <summary>
        /// Gets list of integers containing freq's to test.
        /// </summary>
        public List<int> TestFreqs
        {
            get
            {
                return this.testFreqs;
            }
        }

        /// <summary>
        /// Gets fmax based on current max UPS VF point and freq guardband.
        /// </summary>
        public int FmaxGB
        {
            get
            {
                // get max pointFreqs
                float maxFGB = this.pointFreqs.Max() * (1.0f + this.freqGB);
                return (int)Math.Round((maxFGB / this.incF) * this.incF);
            }
        }

        /// <summary>
        /// Gets list of floats containing voltages to test.
        /// </summary>
        public List<float> TestVolts
        {
            get
            {
                return this.testVolts;
            }
        }

        /// <summary>
        /// Update VF curve with new freq, volt pairs.
        /// </summary>
        /// <param name="pf">list of int.</param>
        /// <param name="pv">list of float.</param>
        public void Update(List<int> pf, List<float> pv)
        {
            // check that list is size 2+
            if (pf.Count != pv.Count || pf.Count < 2)
            {
                throw new ArgumentException("There must be at least 2 points to fit a line!");
            }

            // TODO: add algorithm to interpolate/ extrapolate here
            List<int> pfSorted = new List<int>();
            List<float> pvSorted = new List<float>();
            pfSorted.AddRange(pf);
            pvSorted.AddRange(pv);

            // if last element smaller than first, reverse
            if (pfSorted.Last<int>() < pfSorted.First<int>())
            {
                pfSorted.Reverse();
                pvSorted.Reverse();
            }

            this.pointFreqs = pfSorted;
            this.pointVolts = pvSorted;
            List<float> testVoltsTmp = new List<float>();

            // drop all points below first freq
            int fi = 0;
            /*while (pfSorted.Any() && (pfSorted.First<int>() < this.testFreqs.First<int>()))
            {
                pfSorted.RemoveAt(0);
                pvSorted.RemoveAt(0);
            }*/

            // ensure there's something to interpolate/ extrapolate
            // if ((pfSorted.Count - fi) < 2)
            if (pfSorted.Count < 2)
            {
                throw new ArgumentException("There must be at least 2 points to fit a line!");
            }

            // ensure at least 1 freq overlaps w/ test points
            /*if ((this.testFreqs.Min() > pfSorted.Max()) || (this.testFreqs.Max() < pfSorted.Min()))
            {
                throw new ArgumentException("There must be at least 1 test freq overlapping with UPS test points!");
            }*/

            // loop thru freqs
            foreach (var freq in this.testFreqs)
            {
                float volt = this.minV;
                if (pfSorted.Contains(freq))
                {
                    // trivial case: freq is test point
                    volt = (float)Math.Round(pvSorted[pfSorted.IndexOf(freq)] > this.maxV ? this.maxV : (pvSorted[pfSorted.IndexOf(freq)] < this.minV ? this.minV : pvSorted[pfSorted.IndexOf(freq)]), 3);

                    // if freq is not lowest freq test point, increment index
                    if (freq != pfSorted.First<int>())
                    {
                        fi++;
                    }
                }
                else if (freq < pfSorted.First<int>())
                {
                    // extrapolate on lower end
                    volt = pvSorted[0] + ((freq - pfSorted[0]) * ((pvSorted[1] - pvSorted[0]) / ((float)pfSorted[1] - (float)pfSorted[0])));
                }
                else if (freq > pfSorted.Last<int>())
                {
                    // extrapolate on higher end
                    volt = pvSorted[pfSorted.Count - 1] + ((freq - pfSorted[pfSorted.Count - 1]) * ((pvSorted[pvSorted.Count - 1] - pvSorted[pvSorted.Count - 2]) / ((float)pfSorted[pfSorted.Count - 1] - (float)pfSorted[pfSorted.Count - 2])));
                }
                else
                {
                    // interpolate
                    volt = pvSorted[fi] + ((freq - pfSorted[fi]) * ((pvSorted[fi + 1] - pvSorted[fi]) / ((float)pfSorted[fi + 1] - (float)pfSorted[fi])));
                }

                testVoltsTmp.Add((float)Math.Round(volt > this.maxV ? this.maxV : (volt < this.minV ? this.minV : volt), 3));
            }

            this.testVolts = testVoltsTmp;
        }

        /// <summary>
        /// Override ToString() method.
        /// </summary>
        /// <returns>string containing VF curves.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"VF curve for domain {this.domain} instance {this.name}:");

            for (int i = 0; i < this.testFreqs.Count; i++)
            {
                sb.AppendLine($"Freq: {this.testFreqs[i]}\tVolt: {this.testVolts[i]}");
            }

            sb.AppendLine(string.Empty);

            return sb.ToString();
        }
    }
}