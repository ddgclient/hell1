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
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Class to hold datalog tokens.
    /// </summary>
    public class DataTokens
    {
        private Dictionary<string, List<DataSpec>> tokens;
        private List<string> keyOrder;
        private Dictionary<string, int> captureLengths;
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTokens"/> class.
        /// </summary>
        /// <param name="s">string name of object.</param>
        public DataTokens(string s)
        {
            this.name = s;
            this.tokens = new Dictionary<string, List<DataSpec>> { };
            this.captureLengths = new Dictionary<string, int> { };
        }

        /// <summary>
        /// Gets next DataSpec in tokens dictionary.
        /// </summary>
        public IEnumerable<KeyValuePair<string, List<DataSpec>>> Tokens
        {
            get
            {
                foreach (var key in this.keyOrder)
                {
                    yield return new KeyValuePair<string, List<DataSpec>>(key, this.tokens[key]);
                }
            }
        }

        /// <summary>
        /// Gets DataSpec based on key.
        /// </summary>
        /// <param name="key">name of VF to return.</param>
        /// <returns>VF from private dict.</returns>
        public List<DataSpec> this[string key]
        {
            get
            {
                return this.tokens[key];
            }
        }

        /// <summary>
        /// Adds new DataSpec to tokens.
        /// </summary>
        /// <param name="inst">string name of instance.</param>
        /// <param name="field">string name of data field.</param>
        /// <param name="pos">list of bit positions.</param>
        /// <param name="form">string of transformation expression.</param>
        /// <param name="lims">list of limits.</param>
        /// <param name="index">int datalog index.</param>
        /// <returns>bool pass/ fail status.</returns>
        public bool AddDataSpec(string inst, string field, List<Tuple<int, int>> pos, string form, List<Tuple<string, int>> lims, int index)
        {
            // create new DataSpec
            var ds = new DataSpec(field, pos, form, lims, index);

            // insert it into inst's list, this could result in duplicates
            if (!this.tokens.ContainsKey(inst))
            {
                this.tokens[inst] = new List<DataSpec> { };
                this.keyOrder = new List<string> { };
            }

            this.tokens[inst].Add(ds);
            this.keyOrder.Add(inst);

            return true;
        }

        /// <summary>
        /// returns length of datalog token for given inst.
        /// </summary>
        /// <param name="inst">string of datalog token.</param>
        /// <returns>int length.</returns>
        public int Length(string inst)
        {
            int len = -1;
            if (this.tokens.ContainsKey(inst))
            {
                /*len = 0;
                foreach (var field in this.tokens[inst])
                {
                    len += field.Length();
                }*/

                return this.captureLengths[inst];
            }
            else
            {
                return len;
            }
        }

        /// <summary>
        /// Override ToString() method.
        /// </summary>
        /// <returns>string containing VF curves.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Datalog tokens for {this.name}");

            foreach (var inst in this.tokens.Keys)
            {
                sb.AppendLine($"\t{inst}");
                foreach (var item in this.tokens[inst])
                {
                    sb.AppendLine(item.ToString());
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Decodes capture data for instance.
        /// </summary>
        /// <param name="inst">string name of instance.</param>
        /// <param name="start">int starting index of capdata to decode.</param>
        /// <param name="len">int length of capdata to decode.</param>
        /// <param name="capdata">string of binary data.</param>
        /// <param name="passFailVector">List of int pass/fail status.</param>
        /// <param name="datalogStrings">List of string datalogging.</param>
        /// <returns>bool execution successful.</returns>
        public bool DecodeCapData(string inst, int start, int len, ref string capdata, out List<bool> passFailVector, out List<string> datalogStrings)
        {
            int tests = len / this.Length(inst);
            List<string> decValues = Enumerable.Repeat<string>(string.Empty, this.tokens[inst].Count).ToList();
            List<bool> passVec = new List<bool> { };
            List<string> dataStrList = Enumerable.Repeat<string>(string.Empty, this.tokens[inst].Count).ToList();
            string dataStrDelimited = string.Empty;
            List<string> dataStr = new List<string> { };

            for (int i = 0; i < tests; i++)
            {
                // extract single test CTV data
                string testData = capdata.Substring(i * this.Length(inst), this.Length(inst));

                // get decoded string
                var fields = new List<string> { };
                bool testPf = true;
                dataStrList = Enumerable.Repeat<string>(string.Empty, this.tokens[inst].Count).ToList();

                foreach (var field in this.tokens[inst])
                {
                    string bits = string.Empty;
                    string dec = string.Empty;
                    bool pf = false;
                    if (!field.ExtractAssemble(testData, out bits))
                    {
                        passFailVector = passVec;
                        datalogStrings = dataStr;
                        return false;
                    }

                    if (!field.TransformBits(bits, out dec))
                    {
                        passFailVector = passVec;
                        datalogStrings = dataStr;
                        return false;
                    }

                    fields.Add(dec);
                    dataStrList.Insert(field.DatalogIndex, dec);

                    if (!field.ApplyLimits(dec, out pf))
                    {
                        passFailVector = passVec;
                        datalogStrings = dataStr;
                        return false;
                    }

                    testPf = pf ? testPf : false;
                }

                // remove empty strings, combine in delimited string
                dataStrList = dataStrList.Where(x => !string.IsNullOrEmpty(x)).ToList();
                dataStrDelimited = string.Join("_", dataStrList);

                dataStr.Add(dataStrDelimited);
                passVec.Add(testPf);
            }

            passFailVector = passVec;
            datalogStrings = dataStr;
            return true;
        }

        /// <summary>
        /// add capture length for instance.
        /// </summary>
        /// <param name="inst">string indicating instance name.</param>
        /// <param name="regLen">int length of capture string.</param>
        /// <returns>bool indicating successful execution.</returns>
        public bool AddCapLen(string inst, int regLen)
        {
            if (regLen < 1)
            {
                return false;
            }
            else
            {
                this.captureLengths[inst] = regLen;
                return true;
            }
        }

        /// <summary>
        /// Check if key exists in private dict.
        /// </summary>
        /// <param name="name">string name of key to check.</param>
        /// <returns>bool indicating if key exists.</returns>
        public bool TokenExists(string name)
        {
            if (this.tokens.ContainsKey(name))
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
