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

    /// <summary>
    /// This defines the base class for plot legend.
    /// </summary>
    [Serializable]
    public abstract class PlotLegendBase : IEnumerable<KeyValuePair<string, string>>
    {
        /// <summary>
        /// Gets or sets pointToSymbolMap.
        /// This map holds the shmoo map that will be printed to ituff/console.
        /// This will hold the symbol per point. (not the legend. The legend has a separate class).
        /// </summary>
        protected Dictionary<ShmooPoint, string> PointToSymbolMap { get; set; } = new Dictionary<ShmooPoint, string>();

        /// <summary>
        /// This should return symbol for the given point.
        /// </summary>
        /// <param name="point">point.</param>
        /// <returns>symbol.</returns>
        public abstract string GetPointSymbol(ShmooPoint point);

        /// <summary>
        /// This should return the plot legend data.
        /// </summary>
        /// <returns>Legend.</returns>
        public abstract Dictionary<string, string> GetPlotLegend();

        /// <summary>
        /// Resets legend data.
        /// </summary>
        public abstract void ResetLegend();

        /// <summary>
        /// Used to loop over legend map.
        /// </summary>
        /// <returns>enumerator for the loop.</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.GetPlotLegend().GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Resets data.
        /// </summary>
        public void Reset()
        {
            this.PointToSymbolMap.Clear();
            this.ResetLegend();
        }

        /// <summary>
        /// Sets symbol. It can be used only be inheriting classes.
        /// </summary>
        /// <param name="point">point.</param>
        /// <param name="symbol">symbol.</param>
        protected void SetPointSymbol(ShmooPoint point, string symbol)
        {
            this.PointToSymbolMap[point] = symbol;
        }
    }
}