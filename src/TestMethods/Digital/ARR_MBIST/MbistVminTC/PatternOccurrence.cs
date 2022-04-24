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

namespace MbistVminTC
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Tuple with pattern occurrence information.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PatternOccurrence
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatternOccurrence"/> class.
        /// </summary>
        /// <param name="burst">Burst index.</param>
        /// <param name="name">Pattern name.</param>
        /// <param name="occurrence">Pattern occurrence.</param>
        public PatternOccurrence(ulong burst, string name, ulong occurrence)
        {
            this.Burst = burst;
            this.PatternName = name;
            this.Occurrence = occurrence;
        }

        /// <summary>
        /// Gets the burst index.
        /// </summary>
        public ulong Burst { get; }

        /// <summary>
        /// Gets the pattern name.
        /// </summary>
        public string PatternName { get; }

        /// <summary>
        /// Gets the pattern occurrence.
        /// </summary>
        public ulong Occurrence { get; }

        /// <summary>
        /// Compares current and other object.
        /// </summary>
        /// <param name="other">Other object.</param>
        /// <returns>true when equal.</returns>
        public bool Equals(PatternOccurrence other)
        {
            return this.Burst == other.Burst && this.PatternName == other.PatternName && this.Occurrence == other.Occurrence;
        }
    }
}
