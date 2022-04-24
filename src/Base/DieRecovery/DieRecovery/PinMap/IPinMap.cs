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
    using System.Collections;
    using System.Collections.Generic;
    using Prime.Base.Exceptions;
    using Prime.FunctionalService;

    /// <summary>
    /// Base definition of the Recovery PinMapBase Interface.
    /// </summary>
    public interface IPinMap
    {
        /// <summary>
        /// Updates the pin masking for the given IFunctionalTestForSearch object using
        /// the BitArray representing a Tracking structure.
        /// </summary>
        /// <param name="mask">Bit array containing the mask to apply.</param>
        /// <param name="plist">PList object to apply the mask to.</param>
        /// <param name="maskedPins">Additional masked pins. They will be added to the resulting list.</param>
        void MaskPins(BitArray mask, ref IFunctionalTest plist, List<string> maskedPins);

        /// <summary>
        /// Gets the Pins to mask from the BitArray representing a Tracking structure.
        /// </summary>
        /// <param name="mask">Bit array containing the mask to apply.</param>
        /// <param name="maskedPins">Additional masked pins. They will be added to the resulting list.</param>
        /// <returns>List of pins to mask.</returns>
        List<string> GetMaskPins(BitArray mask, List<string> maskedPins);

        /// <summary>
        /// Updates the pin masking for the given IFunctionalTestForSearch object using
        /// the BitArray representing a Tracking structure.
        /// </summary>
        /// <param name="mask">Bit array containing the mask to apply.</param>
        /// <param name="plist">PList object to apply the mask to.</param>
        void ModifyPlist(BitArray mask, ref IFunctionalTest plist);

        /// <summary>
        /// Runs a restore routine. Used for restoring patterns and/or plist to their original/default states.
        /// </summary>
        void Restore();

        /// <summary>
        /// Verify routine to be called during Init..
        /// </summary>
        /// /// <param name="plist">PList object.</param>
        void Verify(ref IFunctionalTest plist);

        /// <summary>
        /// Decodes the failure for the given IFunctionalTest based on the pin mapping.
        /// Implemented decoder will need to cast object to the specific IFunctionalTest implementation.
        /// Returns a BitArray with each element representing pass (false) or fail (true) for each element in the Tracking structure.
        /// </summary>
        /// <param name="functionalTest">IFunctionalTest=<see cref="IFunctionalTest"/> generic functional test. </param>
        /// <param name="currentSlice">(optional) Current Slice/Core/Subdomain if required.</param>
        /// <returns>BitArray representing the failures.</returns>
        BitArray DecodeFailure(IFunctionalTest functionalTest, int? currentSlice = null);

        /// <summary>
        /// Converts the FailTracker BitVector to a BitVector representing Failing Voltage Domains.
        /// </summary>
        /// <param name="trackerBitArray">BitArray output of DecodeFailure.</param>
        /// <returns>BitArray with true for each failing voltage domain.</returns>
        BitArray FailTrackerToFailVoltageDomains(BitArray trackerBitArray);

        /// <summary>
        /// Expands a BitArray from 1 bit per voltage domain to 1 bit per fail tracker bit.
        /// This is the opposite of FailTrackerToFailVoltageDomains().
        /// </summary>
        /// <param name="voltageDomainBitArray">BitArray with 1 bit per voltage target/domain.</param>
        /// <returns>BitArray with 1 Bit per fail tracker.</returns>
        BitArray VoltageDomainsToFailTracker(BitArray voltageDomainBitArray);

        /// <summary>
        /// Applies the IP Enable/Disable pattern modify associated with this PinMapBase object using the supplied BitArray.
        /// </summary>
        /// <param name="iPConfigBits">BitArray representing enable/disable for this IP (true == disable, false == enable).</param>
        /// <param name="plist">Specific PList to apply the pattern modify to.</param>
        /// <exception cref="TestMethodException"> thrown if there's a problem with the patmodify.</exception>
        void ApplyPatConfig(BitArray iPConfigBits, string plist);

        /// <summary>
        /// Applies the IP Enable/Disable pattern modify associated with this PinMapBase object using the supplied BitArray.
        /// </summary>
        /// <param name="iPConfigBits">BitArray representing enable/disable for this IP (true == disable, false == enable).</param>
        /// <exception cref="TestMethodException"> thrown if there's a problem with the patmodify.</exception>
        void ApplyPatConfig(BitArray iPConfigBits);

        /// <summary>
        /// Returns list of pinmaps and expected size.
        /// </summary>
        /// <returns>List of pinmaps.</returns>
        IReadOnlyList<IPinMapDecoder> GetConfiguration();
    }
}
