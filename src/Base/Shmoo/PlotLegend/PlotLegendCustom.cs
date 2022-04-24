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
    /// This class representss custom plot legend.
    /// User of Shmoo test method, can decide to use 'CUSTOM' plot mode through the parameter.
    /// In this custom mode, user will have control over the symbol to write for each shmoo point,
    /// and over the legend keys and values. Note that in custom mode, there's no direct connection between
    /// symbol and legend key; unlike the default plotting mode.
    /// </summary>
    [Serializable]
    public class PlotLegendCustom : PlotLegendBase
    {
        private readonly Dictionary<string, string> legendKeyToValueMap = new Dictionary<string, string>();

        /// <summary>
        /// Populates the shmoo map with the following information per a given point:
        /// [1] symbol: the symbol of this point to show when plotting the map to ituff/console.
        /// [2] legend data: the key and values to show when printing the legend following the plotting map.
        ///      Note: from user's perpective, there's no restriction or any connection between key and symbol.
        ///            but at the end, the final legend key will appear on ituff/console as follow:
        ///
        ///            E.g. point=(4, 7);
        ///                 legendKeyAndValue=(keys: MySpecialPoint, HelloABC | values=FailInfo1, FailInfo2)
        ///
        ///                 plotting:
        ///
        ///                 2_tname_INSTANCE_NAME^LEGEND^[4,7,MySpecialPoint]
        ///                 2_strgval_fail_FailInfo1
        ///                 2_tname_INSTANCE_NAME^LEGEND^[4,7,HelloABC]
        ///                 2_strgval_fail_FailInfo2.
        /// </summary>
        /// <param name="point">point under test.</param>
        /// <param name="symbol">symbol for this point, when printing the shmoo map.</param>
        /// <param name="legendKeyAndValue">legend info to add. See example.</param>
        public void AddData(ShmooPoint point, string symbol, Dictionary<string, string> legendKeyAndValue)
        {
            this.SetPointSymbol(point, symbol);

            foreach (var keyValue in legendKeyAndValue)
            {
                string key = this.GenerateKey(point, keyValue.Key);

                if (this.legendKeyToValueMap.ContainsKey(key))
                {
                    throw new TestMethodException($"There's a key already exists in custom legend map for key=[{key}].");
                }

                this.legendKeyToValueMap[key] = keyValue.Value;
            }
        }

        /// <summary>
        /// Returns the plot legend mapping from key to value.
        /// </summary>
        /// <returns>Legend map.</returns>
        public override Dictionary<string, string> GetPlotLegend()
        {
            return this.legendKeyToValueMap;
        }

        /// <summary>
        /// This should return symbol for the given point. If this point doesn't have entry, we throw an exception;
        /// since in custom plot legend mode, user need to fill the symbol for each possible point.
        /// </summary>
        /// <param name="point">point.</param>
        /// <returns>symbol.</returns>
        public override string GetPointSymbol(ShmooPoint point)
        {
            if (!this.PointToSymbolMap.ContainsKey(point))
            {
                throw new TestMethodException("In custom plot legend mode, every point need to have a symbol in the legend.");
            }

            return this.PointToSymbolMap[point];
        }

        /// <summary>
        /// Resets legend data.
        /// </summary>
        public override void ResetLegend()
        {
            this.legendKeyToValueMap.Clear();
        }

        /// <summary>
        /// This function should be used (internally by Prime developes) to create the key for the legend.
        /// </summary>
        /// <param name="point">point.</param>
        /// <param name="key">requested key for this point.</param>
        /// <returns>the final key to be used with the shmoo legend.</returns>
        private string GenerateKey(ShmooPoint point, string key)
        {
            char[] notAllowedItuffChars = new char[] { '\n' };

            if (key.IndexOfAny(notAllowedItuffChars) != -1)
            {
                throw new TestMethodException($"User not allowed to set custom legend key=[{key}] with one of the following chars: {notAllowedItuffChars.ToString()}.");
            }

            if (key.Any(char.IsWhiteSpace))
            {
                throw new TestMethodException($"User not allowed to set custom legend key=[{key}] with whitespaces.");
            }

            return $"[{point.XValue}^{point.YValue}^{key}";
        }
    }
}
