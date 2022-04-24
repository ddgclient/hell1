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

namespace SimpleCtvTC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Register container.
    /// </summary>
    internal class Register
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Register"/> class.
        /// </summary>
        /// <param name="value">Options.</param>
        public Register(string value)
        {
            var elements = value.Split(':');
            if (elements.Length != 2)
            {
                throw new ArgumentException($"Invalid syntax for for register={value}");
            }

            this.Name = elements[0];

            this.Indexes = elements[1].Split(',')
                .Select(x => x.Split('-'))
                .Select(p => new { First = int.Parse(p.First()), Last = int.Parse(p.Last()) })
                .SelectMany(x => x.First < x.Last ?
                    Enumerable.Range(x.First, x.Last - x.First + 1) :
                    Enumerable.Range(x.Last, x.First - x.Last + 1).Reverse());
        }

        /// <summary>
        /// Gets register name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets register indexes.
        /// </summary>
        public IEnumerable<int> Indexes { get; }

        /// <summary>
        /// Gets or sets high limit.
        /// </summary>
        public int HighLimit { get; set; } = -1;

        /// <summary>
        /// Gets or sets low limit.
        /// </summary>
        public int LowLimit { get; set; } = -1;

        /// <summary>
        /// Gets or sets not equal limit.
        /// </summary>
        public int NotEqualLimit { get; set; } = -1;

        /// <summary>
        /// Gets or sets a value indicating whether the register must be printed to ituff.
        /// </summary>
        public bool Print { get; set; } = false;
    }
}
