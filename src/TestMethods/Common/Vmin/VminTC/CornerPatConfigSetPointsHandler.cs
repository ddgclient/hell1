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

namespace DDG
{
    using System;
    using System.Collections.Generic;
    using Prime.PatConfigService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Handler for Corner specific PatConfigSetPoints.
    /// </summary>
    public class CornerPatConfigSetPointsHandler
    {
        private readonly List<Tuple<IPatConfigSetPointHandle, string, int>> cornerPatConfigSetPointsHandles;

        /// <summary>
        /// Initializes a new instance of the <see cref="CornerPatConfigSetPointsHandler"/> class.
        /// </summary>
        /// <param name="patlist">Patlist parameter.</param>
        /// <param name="cornerPatConfigSetPoints">Instance parameter value.</param>
        /// <param name="vminForwarding">VminForwarding corners information.</param>
        public CornerPatConfigSetPointsHandler(string patlist, string cornerPatConfigSetPoints, List<Tuple<string, int, IVminForwardingCorner>> vminForwarding)
        {
            this.cornerPatConfigSetPointsHandles = null;
            if (vminForwarding == null || string.IsNullOrEmpty(cornerPatConfigSetPoints))
            {
                return;
            }

            if (!Prime.Services.SharedStorageService.KeyExistsInObjectTable(cornerPatConfigSetPoints, Context.DUT))
            {
                throw new ArgumentException($"Unable to find CornerPatConfigSetPoints={cornerPatConfigSetPoints} in SharedStorage.");
            }

            var freqSetPointMap = Prime.Services.SharedStorageService.GetRowFromTable(cornerPatConfigSetPoints, typeof(FreqSetPointMap), Context.DUT) as FreqSetPointMap;
            this.cornerPatConfigSetPointsHandles = new List<Tuple<IPatConfigSetPointHandle, string, int>>();
            foreach (var corner in vminForwarding)
            {
                if (freqSetPointMap == null || !freqSetPointMap.CornerIdentifiers.ContainsKey(corner.Item1))
                {
                    continue;
                }

                foreach (var setPoint in freqSetPointMap.CornerIdentifiers[corner.Item1])
                {
                    if (!string.IsNullOrEmpty(setPoint.Condition))
                    {
                        var expression = new HdmtExpression(setPoint.Condition);
                        if (!(bool)expression.Evaluate())
                        {
                            continue;
                        }
                    }

                    var handle = Prime.Services.PatConfigService.GetSetPointHandle(setPoint.Module, setPoint.Group, patlist);
                    this.cornerPatConfigSetPointsHandles.Add(new Tuple<IPatConfigSetPointHandle, string, int>(handle, setPoint.SetPoint, corner.Item2));
                }
            }
        }

        /// <summary>
        /// Run Corner specific PatConfigSetPoints.
        /// </summary>
        public void Run()
        {
            if (this.cornerPatConfigSetPointsHandles == null || this.cornerPatConfigSetPointsHandles.Count <= 0)
            {
                return;
            }

            foreach (var setPoint in this.cornerPatConfigSetPointsHandles)
            {
                var setPointName = Prime.Services.BinMatrixService.EvaluateString(setPoint.Item2, setPoint.Item3);
                setPoint.Item1.ApplySetPoint(setPointName);
            }
        }
    }
}
