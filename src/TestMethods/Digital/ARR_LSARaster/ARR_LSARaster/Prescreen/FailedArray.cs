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
    using System;
    using System.Collections.Generic;
    using Prime.PatternService;

    /// <summary>
    /// Fail detected during prescreen to be submitted to the internal DB. Classes that extend FailedArray are created by the <see cref="FailedArrayFactory"/> during
    /// prescreen execution.
    /// </summary>
    // In raster we implement IDefect, in prescreen we inherit FailedArray. The idea behind supporting mutiple products is the same,
    // but with two different approaches. This isn't clean and will cause confusion
    public abstract class FailedArray
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailedArray"/> class.
        /// </summary>
        /// <param name="arrayName"> Name of the failed array. </param>
        public FailedArray(string arrayName)
        {
            this.ArrayName = arrayName;
        }

        /// <summary>
        /// Gets or sets name of the array that failed.
        /// </summary>
        public string ArrayName { get; set; }

        /// <summary>
        /// Gets or sets the address of the array failure.
        /// </summary>
        public Tuple<int, int, int> MBDAddress { get; set; }

        /// <summary>
        /// Maps the given fail to the internal database.
        /// </summary>
        /// <param name="database"> Internal database to submit the defect to. </param>
        public abstract void MapToInternalDB(ref DBContainer database);

        /// <summary>
        /// Using an <see cref="ILabel"/> object, populate values for this <see cref="FailedArray"/>.
        /// </summary>
        /// <param name="label"> <see cref="ILabel"/> object to extract values from. </param>
        /// <param name="pinMappingSet"> Instance of <see cref="MetadataConfig.PinMappingSet"/> to provide metadata on extracting label info. </param>
        public abstract void ExtractValuesFromLabel(ILabel label, MetadataConfig.PinMappingSet pinMappingSet);

        /// <summary>
        /// From a list containing all failed pins for a given cycle, populate values for this <see cref="FailedArray"/>.
        /// </summary>
        /// <param name="failedPins"> List of all pins that failed for a given cycle. </param>
        /// <param name="pinMappingSet"> Instance of <see cref="MetadataConfig.PinMappingSet"/> to provide metadata on extracting info from pins. </param>
        public abstract void ExtractValuesFromPins(List<string> failedPins, MetadataConfig.PinMappingSet pinMappingSet);
    }
}
