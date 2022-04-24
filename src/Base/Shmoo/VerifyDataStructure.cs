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
    using System.Collections.Generic;

    /// <summary>
    /// Centralized data structure to be populated during VerifyOffline() and restored in Init().
    /// </summary>
    [Serializable]
    internal class VerifyDataStructure
    {
        /// <summary>
        /// Gets or sets the list of mask pins.
        /// </summary>
        public List<string> MaskPins { get; set; }

        /// <summary>
        /// Gets or sets plot legend.
        /// </summary>
        public PlotLegendBase PlotLegend { get; set; }

        /// <summary>
        /// Gets or sets Shmoo Plot.
        /// </summary>
        public ShmooPlot ShmooPlot { get; set; }

        /// <summary>
        /// Gets or sets Shmoo Printer.
        /// </summary>
        public ShmooPrinter Printer { get; set; }

        /// <summary>
        /// Gets or sets the X axis points as string.
        /// </summary>
        public List<string> XAxisStringPoints { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the Y axis points as string.
        /// </summary>
        public List<string> YAxisStringPoints { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets regional kill limits.
        /// </summary>
        public PrimeShmooTestMethod.KillLimits RegionalKillLimits { get; set; }
    }
}
