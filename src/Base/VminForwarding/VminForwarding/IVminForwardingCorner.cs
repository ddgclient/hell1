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
    using System.Collections.Generic;

    /// <summary>
    /// Interface for the VminForwardingCorner object.
    /// </summary>
    public interface IVminForwardingCorner
    {
        /// <summary>
        /// Gets the starting voltage for this corner.
        /// This will be the maximum of:
        ///   the passed in value startVoltagesFromParameter,
        ///   FastInfra.xml gsds_vmin_source values,
        ///   FastInfra.xml interdomaincorner_vmin_source values.
        /// </summary>
        /// <param name="startVoltagesFromParameter">The test instances starting value.</param>
        /// <returns>The starting voltage to use for the search.</returns>
        List<double> GetStartingVoltage(List<double> startVoltagesFromParameter);

        /// <summary>
        /// Gets the starting voltage for this corner.
        /// This will be the maximum of:
        ///   the passed in value startVoltagesFromParameter,
        ///   FastInfra.xml gsds_vmin_source values,
        ///   FastInfra.xml interdomaincorner_vmin_source values.
        /// </summary>
        /// <param name="startVoltagesFromParameter">The test instances starting value.</param>
        /// <returns>The starting voltage to use for the search.</returns>
        double GetStartingVoltage(double startVoltagesFromParameter);

        /// <summary>
        /// Updates the vmin forwarding value for this corner.
        /// Will also update values associated with the FastInfra.xml
        /// store_results_in_gsds and tokens_for_downstream_sockets attributes.
        /// </summary>
        /// <param name="vmins">Final vmin result.</param>
        /// <returns>true if the vmin data was stored correctly.</returns>
        bool StoreVminResult(List<double> vmins);

        /// <summary>
        /// Updates the vmin forwarding value for this corner.
        /// Will also update values associated with the FastInfra.xml
        /// store_results_in_gsds and tokens_for_downstream_sockets attributes.
        /// </summary>
        /// <param name="vmin">Final vmin result.</param>
        /// <returns>true if the vmin data was stored correctly.</returns>
        bool StoreVminResult(double vmin);
    }
}
