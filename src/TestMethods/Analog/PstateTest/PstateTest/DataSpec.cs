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
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.SqlServer.Server;

    /// <summary>
    /// Class that specifies a range of capture memory data.
    /// </summary>
    public class DataSpec
    {
        private string name;
        private List<Tuple<int, int>> positions;
        private string transform;
        private List<Tuple<string, int>> limits;
        private int datalogIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSpec"/> class.
        /// </summary>
        /// <param name="name">string representing field token.</param>
        /// <param name="datalogIndex">zero-based index of datalog token position.</param>
        public DataSpec(string name, int datalogIndex)
        {
            this.name = name;
            this.datalogIndex = datalogIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSpec"/> class.
        /// </summary>
        /// <param name="name">string representing field token.</param>
        /// <param name="pos">list of integer bit positions.</param>
        /// <param name="form">string indicating transformation of binary string.</param>
        /// <param name="lims">list of strings indicating limits to apply to transformed value.</param>
        /// <param name="index">zero-based index of datalog token position.</param>
        public DataSpec(string name, List<Tuple<int, int>> pos, string form, List<Tuple<string, int>> lims, int index)
        {
            this.name = name;
            this.positions = pos;
            this.transform = form;
            this.limits = lims;
            this.datalogIndex = index;
        }

        /// <summary>
        /// Gets name of data token.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Gets name of data token.
        /// </summary>
        public List<Tuple<int, int>> Positions
        {
            get
            {
                return this.positions;
            }
        }

        /// <summary>
        /// Gets transform of data token.
        /// </summary>
        public string Transform
        {
            get
            {
                return this.transform;
            }
        }

        /// <summary>
        /// Gets list of limits.
        /// </summary>
        public List<Tuple<string, int>> Limits
        {
            get
            {
                return this.limits;
            }
        }

        /// <summary>
        /// Gets datalog index of data token.
        /// </summary>
        public int DatalogIndex
        {
            get
            {
                return this.datalogIndex;
            }
        }

        /// <summary>
        /// return total number of bits in token.
        /// </summary>
        /// <returns>int total # of bits in token.</returns>
        public int Length()
        {
            int len = 0;
            foreach (var pair in this.positions)
            {
                if (pair.Item2 == -1)
                {
                    len++;
                }
                else
                {
                    len += (pair.Item1 - pair.Item2) + 1;
                }
            }

            return len;
        }

        /// <summary>
        /// Given a string of bits, extracts bits according to positions.
        /// </summary>
        /// <param name="capBits">string of capture data, LSB:MSB.</param>
        /// <param name="fieldBits">string of field bits, MSB:LSB.</param>
        /// <returns>bool indicating success.</returns>
        public bool ExtractAssemble(string capBits, out string fieldBits)
        {
            var field = new StringBuilder();

            // quick check of input string
            var onlyBits = new Regex($"^[01]+$");
            if (capBits.Length < this.Length())
            {
                fieldBits = string.Empty;
                return false;
            }
            else if (!onlyBits.IsMatch(capBits))
            {
                fieldBits = string.Empty;
                return false;
            }
            else
            {
                foreach (var pos in this.positions)
                {
                    if (capBits.Length < pos.Item1)
                    {
                        fieldBits = string.Empty;
                        return false;
                    }
                    else
                    {
                        if (pos.Item2 == -1)
                        {
                            field.Append(capBits.Substring(pos.Item1, 1));
                        }
                        else
                        {
                            field.Append(capBits.Substring(pos.Item2, (pos.Item1 - pos.Item2) + 1).ReverseString());
                        }
                    }
                }

                fieldBits = field.ToString();
                return true;
            }
        }

        /// <summary>
        /// Transforms bit string into int value.
        /// </summary>
        /// <param name="bits">string of bits, MSB:LSB.</param>
        /// <param name="value">transformed value of bit string.</param>
        /// <returns>bool indicating success.</returns>
        public bool TransformBits(string bits, out string value)
        {
            // always convert to int
            int iVal;
            try
            {
                iVal = Convert.ToInt32(bits, 2);
            }
            catch (Exception)
            {
                value = string.Empty;
                return false;
            }

            value = iVal.ToString();
            return true;
        }

        /// <summary>
        /// apply limits to the transformed value and indicate pass/ fail.
        /// </summary>
        /// <param name="value">value to assess.</param>
        /// <param name="pass">bool indicating pass/ fail according to limits.</param>
        /// <returns>bool indicating success.</returns>
        public bool ApplyLimits(string value, out bool pass)
        {
            bool retval = true;
            if (value == string.Empty)
            {
                pass = false;
                return false;
            }
            else
            {
                int iVal;
                try
                {
                    iVal = Convert.ToInt32(value);
                    bool comparison = true;
                    foreach (var lim in this.limits)
                    {
                        if (!lim.Item1.Operator(iVal, lim.Item2, out comparison))
                        {
                            pass = false;
                            return false;
                        }
                        else
                        {
                            if (!comparison)
                            {
                                retval = false;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    pass = false;
                    return false;
                }
            }

            pass = retval;
            return true;
        }

        /// <summary>
        /// Override ToString() method.
        /// </summary>
        /// <returns>string containing VF curves.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"  {this.name}:\t");

            for (int i = 0; i < this.positions.Count; i++)
            {
                if (this.positions[i].Item2 == -1)
                {
                    sb.Append($"{this.positions[i].Item1}");
                }
                else
                {
                    sb.Append($"{this.positions[i].Item1}:{this.positions[i].Item2}");
                }
            }

            sb.AppendLine(string.Empty);

            return sb.ToString();
        }
    }
}
