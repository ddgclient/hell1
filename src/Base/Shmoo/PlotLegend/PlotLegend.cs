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

namespace Prime.TestMethods.Shmoo
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Prime.Base.Exceptions;

    /// <summary>
    /// This clas will manage shmoo plot legend characters.
    /// </summary>
    [Serializable]
    public class PlotLegend : PlotLegendBase
    {
        private readonly Queue<char> allLegendChars = new Queue<char>();
        private readonly Dictionary<string, string> forwardMapping = new Dictionary<string, string>();
        private readonly Dictionary<string, string> reverseMapping = new Dictionary<string, string>();
        private Queue<char> availableLegendChars;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlotLegend"/> class.
        /// </summary>
        public PlotLegend()
        {
            Enumerable.Range((int)'A', 26).Select(x => (char)x).ToList().ForEach(c => this.allLegendChars.Enqueue(c));
            Enumerable.Range((int)'a', 26).Select(x => (char)x).ToList().ForEach(c => this.allLegendChars.Enqueue(c));
            Enumerable.Range((int)'0', 10).Select(x => (char)x).ToList().ForEach(c => this.allLegendChars.Enqueue(c));
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
        /// <param name="point">point.</param>
        /// <param name="info">info.</param>
        public void AddData(ShmooPoint point, string info)
        {
            if (this.reverseMapping.TryGetValue(info, out var existingSymbol))
            {
                this.SetPointSymbol(point, existingSymbol);
                return;
            }
            else if (this.availableLegendChars.Count == 0)
            {
                this.SetPointSymbol(point, ((char)ReservedSymbols.EXCESSIVE_FAIL).ToString());
                return;
            }

            char nextAvailableLegendChar = this.availableLegendChars.Dequeue();
            this.SetPointSymbol(point, nextAvailableLegendChar.ToString());
            this.forwardMapping.Add(nextAvailableLegendChar.ToString(), info);
            this.reverseMapping.Add(info, nextAvailableLegendChar.ToString());
        }

        /// <summary>
        /// Flag a point as skipped. There's a reserved symbol for skipped point.
        /// </summary>
        /// <param name="point">point to flag as skipped.</param>
        public void FlagSkippedPoint(ShmooPoint point)
        {
            this.PointToSymbolMap[point] = ((char)ReservedSymbols.SKIP).ToString();
        }

        /// <summary>
        /// This should return symbol for the given point. If this point doesn't have entry, we assume it's passing.
        /// </summary>
        /// <param name="point">point.</param>
        /// <returns>symbol.</returns>
        public override string GetPointSymbol(ShmooPoint point)
        {
            return this.PointToSymbolMap.ContainsKey(point) ? this.PointToSymbolMap[point] : ((char)ReservedSymbols.PASS).ToString();
        }

        /// <summary>
        /// Returns the plot legend mapping from key to value.
        /// </summary>
        /// <returns>Legend map.</returns>
        public override Dictionary<string, string> GetPlotLegend()
        {
            return this.forwardMapping;
        }

        /// <summary>
        /// Resets data.
        /// </summary>
        public override void ResetLegend()
        {
            this.forwardMapping.Clear();
            this.reverseMapping.Clear();
            this.availableLegendChars = new Queue<char>(this.allLegendChars);
        }
    }
}
