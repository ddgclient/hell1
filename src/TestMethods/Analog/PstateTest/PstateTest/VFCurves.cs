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
    /// class to contain collection of VF curves.
    /// </summary>
    public class VFCurves
    {
        private Dictionary<string, VF> curves = new Dictionary<string, VF> { };
        private List<string> keyOrder = new List<string> { };
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="VFCurves"/> class.
        /// </summary>
        /// <param name="n">name of VF instance.</param>
        public VFCurves(string n)
        {
            this.name = n;
        }

        /// <summary>
        /// Gets next curve in curves dictionary.
        /// </summary>
        public IEnumerable<KeyValuePair<string, VF>> Curves
        {
            get
            {
                foreach (var key in this.keyOrder)
                {
                    yield return new KeyValuePair<string, VF>(key, this.curves[key]);
                }
            }
        }

        /// <summary>
        /// Gets VF based on key.
        /// </summary>
        /// <param name="key">name of VF to return.</param>
        /// <returns>VF from private dict.</returns>
        public VF this[string key]
        {
            get
            {
                return this.curves[key];
            }
        }

        /// <summary>
        /// method to add VF curve to dict.
        /// </summary>
        /// <param name="d">name of VF domain.</param>
        /// <param name="n">name of VF instance.</param>
        /// <param name="minV">min voltage.</param>
        /// <param name="maxV">max voltage.</param>
        /// <param name="minF">min freq.</param>
        /// <param name="maxF">max freq.</param>
        /// <param name="incF">freq bins.</param>
        /// <param name="freqGB">freq guardband.</param>
        /// <param name="pf">List of int.</param>
        /// <param name="pv">List of float.</param>
        /// <returns>book indicating successful execution.</returns>
        public bool AddVF(string d, string n, float minV, float maxV, int minF, int maxF, float incF, float freqGB, List<int> pf, List<float> pv)
        {
            try
            {
                var vf = new VF(d, n, minV, maxV, minF, maxF, incF, freqGB, pf, pv);
                this.curves.Add(n, vf);
                this.keyOrder.Add(n);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// method to update VF curves based on test points.
        /// </summary>
        /// <param name="pf">List of int.</param>
        /// <param name="pv">List of float.</param>
        /// <returns>bool indicating successful execution.</returns>
        public bool UpdateVF(List<int> pf, List<float> pv)
        {
            try
            {
                foreach (var key in this.curves.Keys)
                {
                    this.curves[key].Update(pf, pv);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if key exists in private dict.
        /// </summary>
        /// <param name="name">string name of key to check.</param>
        /// <returns>bool indicating if key exists.</returns>
        public bool VFExists(string name)
        {
            if (this.curves.ContainsKey(name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}