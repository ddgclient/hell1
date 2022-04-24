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

namespace LSARasterTC
{
    using System.Collections.Generic;

    /// <summary>
    /// This class is used to represent the different type of arrays we'll be seeing when performing raster. Each Defect will have its own method of deserializing into.
    /// </summary>
    public interface IDefect
    {
        /// <summary>
        /// Gets or sets a value indicating whether to send this object to repair.
        /// </summary>
        bool SendToRepair { get; set; }

        /// <summary>
        /// Method for adding this object into the internalDB.
        /// </summary>
        /// <param name="database"> Database to load the defect into. </param>
        void AddToInternalDatabase(ref Dictionary<string, List<IDefect>> database);

        /// <summary>
        /// Create a string used to log this Defect to the TFile.
        /// </summary>
        /// <returns> A string formatted for TFile. </returns>
        string CreateTFileString();

        /// <summary>
        /// Create a string that can be used as the header for a TFile block. The header will contain info that's common between all defects categorized under the same key.
        /// </summary>
        /// <returns> A string value. </returns>
        string CreateTFileHeaderBlock();

        /// <summary>
        /// Creates a string to be submitted to the iCRepair supersede.
        /// </summary>
        /// <returns> A string value. </returns>
        string CreateRepairString();
    }
}
