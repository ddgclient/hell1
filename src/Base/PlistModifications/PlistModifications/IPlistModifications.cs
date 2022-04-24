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
    /// <summary>
    /// Interface for pattern modification. Helper for Prime PatConfig.
    /// </summary>
    public interface IPlistModifications
    {
        /// <summary>
        /// Builds/Fetches plist tree in concurrent data structure.
        /// </summary>
        /// <param name="plist">Plist name.</param>
        /// <returns>Plist tree object.</returns>
        PlistTree BuildPlistTree(string plist);

        /// <summary>
        /// Removes tree node from concurrent data structure.
        /// </summary>
        /// <param name="plist">Plist name. If empty it will delete the entire tree.</param>
        void CleanTree(string plist);

        /// <summary>
        /// Restores tree node from concurrent data structure.
        /// </summary>
        /// <param name="plist">Plist name. If empty it will delete the entire tree.</param>
        void RestoreTree(string plist);
    }
}
