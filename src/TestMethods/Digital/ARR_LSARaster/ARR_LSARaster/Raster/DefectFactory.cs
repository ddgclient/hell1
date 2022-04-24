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
    /// Class dedicated to creating objects that implement <see cref="IDefect"/> interface.
    /// </summary>
    public class DefectFactory
    {
        /// <summary>
        /// Take in ctvData and decode them into <see cref="IDefect"/> objects.
        /// </summary>
        /// <param name="ctvData"> CtvData to decode into defects. </param>
        /// <param name="pinMappingSet"> PinMappingSet containing metadata about defects.</param>
        /// <param name="captureSet"> Capture set that specifies how to decode the ctvData. </param>
        /// <param name="currentArray"> Current array to assign defects to. </param>
        /// <param name="dwordElement"> Dword element used for patMod before ctv capture. </param>
        /// <param name="currentPin"> Pin that the defect failed on. </param>
        /// <returns> List of <see cref="IDefect"/>. </returns>
        public static List<IDefect> DecodeDefects(List<string> ctvData, MetadataConfig.PinMappingSet pinMappingSet, MetadataConfig.CaptureSet captureSet, string currentArray, RasterConfig.DwordElementContainer dwordElement, string currentPin)
        {
            MetadataConfig.ArrayType arrayType = pinMappingSet.GetArrayType();

            switch (arrayType)
            {
                case MetadataConfig.ArrayType.ATOM:
                    return AtomDefect.CreateDefects(ctvData, captureSet, pinMappingSet, currentPin, currentArray, dwordElement);

                case MetadataConfig.ArrayType.BIGCORE:
                    return BigCoreDefect.CreateDefects(ctvData, captureSet, pinMappingSet, currentPin, currentArray, dwordElement);

                default:
                    throw new Prime.Base.Exceptions.FatalException("Cannot decode current array type into defects; encoutered a type that hasn't been implemented yet.");
            }
        }
    }
}
