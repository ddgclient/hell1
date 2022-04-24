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

namespace InterleavePatModShmoo
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Prime.Base.Exceptions;

    /// <summary>
    /// This clas will manage shmoo plot legend characters.
    /// </summary>
    public class PlotLegend : IEnumerable<KeyValuePair<char, string>>
    {
        private Dictionary<char, string> forwardMapping = new Dictionary<char, string>();
        private Dictionary<string, char> reverseMapping = new Dictionary<string, char>();
        private Queue<char> availableLegendChars = new Queue<char>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PlotLegend"/> class.
        /// </summary>
        public PlotLegend()
        {
            Enumerable.Range((int)'a', 26).Select(x => (char)x).ToList().ForEach(c => this.availableLegendChars.Enqueue(c));
            Enumerable.Range((int)'0', 10).Select(x => (char)x).ToList().ForEach(c => this.availableLegendChars.Enqueue(c));
        }

        /// <summary>
        /// Reserved symbols.
        /// </summary>
        public enum ReservedSymbols
        {
            /// <summary>
            /// pass.
            /// </summary>
            PASS = '*',

            /// <summary>
            /// skip.
            /// </summary>
            SKIP = '#',

            /// <summary>
            /// excessive fail. This should be used when no more characters left to be used.
            /// </summary>
            EXCESSIVE_FAIL = '@',
        }

        /// <summary>
        /// Adds a new fail info and returns the assigned chracter for it.
        /// Note: method will take care of the duplicated fail info.
        /// </summary>
        /// <param name="info">info.</param>
        /// <returns>return.</returns>
        public char AddFailInfoAndGetLegendSymbol(string info)
        {
            if (this.reverseMapping.TryGetValue(info, out var existingSymbol) == true)
            {
                return existingSymbol;
            }
            else if (this.availableLegendChars.Count == 0)
            {
                return (char)ReservedSymbols.EXCESSIVE_FAIL;
            }

            char nextAvailableLegendChar = this.availableLegendChars.Dequeue();
            this.forwardMapping.Add(nextAvailableLegendChar, info);
            this.reverseMapping.Add(info, nextAvailableLegendChar);
            return nextAvailableLegendChar;
        }

        /// <summary>
        /// Used to loop over legend map.
        /// </summary>
        /// <returns>enumerator for the loop.</returns>
        public IEnumerator<KeyValuePair<char, string>> GetEnumerator()
        {
            return this.forwardMapping.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
