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
    using Prime.VminForwardingService;

    /// <summary>
    /// Defines the <see cref="IVminForwardingFactory" />.
    /// </summary>
    public interface IVminForwardingFactory
    {
        /// <summary>
        /// Factory method.
        /// </summary>
        /// <param name="name">Configuration name.</param>
        /// <param name="flow">Flow index.</param>
        /// <returns>Die Recovery interface.</returns>
        IVminForwardingCorner Get(string name, int flow);

        /// <summary>
        /// Factory method for creating a IVminForwardingCorner=<see cref="IVminForwardingCorner"/>.
        /// </summary>
        /// <param name="domain">Domain name (either a single name or a comma separated list).</param>
        /// <param name="freqCorner">Frequency corner name (F1, F2, F3, ...).</param>
        /// <param name="flow">Flow index.</param>
        /// <returns>Die Recovery interface.</returns>
        IVminForwardingCorner Get(string domain, string freqCorner, int flow);

        /// <summary>
        /// Gets a list of all domain names from Primes VminForwarding structures.
        /// </summary>
        /// <returns>List of Domain names as strings.</returns>
        List<string> GetAllDomainNames();

        /// <summary>
        /// Gets the list of Instance names for the given Domain name. (ie. returns [CR0, CR1, CR2, CR3] when given CR).
        /// </summary>
        /// <param name="domainName">Name of Domain.</param>
        /// <returns>List of instance names as strings.</returns>
        List<string> GetInstanceNamesForDomain(string domainName);

        /// <summary>
        /// Gets the list of valid frequency corner names for this instance. This is the full DomainInstance@Corner name.
        /// </summary>
        /// <param name="instanceName">Name of the domain instance instance.</param>
        /// <returns>List of valid corners as a string.</returns>
        List<string> GetCornerNamesForDomainInstance(string instanceName);

        /// <summary>
        /// Gets the frequency constant for the given corner.
        /// </summary>
        /// <param name="fullCornerName">Full VminForwarding Corner name (ie CR0@F1, CLR@F6).</param>
        /// <param name="flow">Flow ID (needed if this corner uses a binmatrix constant for its frequency).</param>
        /// <returns>frequency for the corner.</returns>
        double GetFrequency(string fullCornerName, int flow);

        /// <summary>
        /// Copies all the VminForwarding records for the given list of domains to a snapshot/temporary location.
        /// </summary>
        /// <param name="domainNames">Name of all the domain names to save. This is the top-level domain names, not the instance names (ie CR not CR0,CR1,CR2).</param>
        void SaveVminForwardingSnapshot(List<string> domainNames);

        /// <summary>
        /// Retrieves the saved snapshot data for this corner.
        /// </summary>
        /// <param name="cornerName">Full Domain + FrequencyCorner name. ie CLR@F3.</param>
        /// <returns>Saved voltage vmin record.</returns>
        VminForwardingCornerData GetVminForwardingSnapshot(string cornerName);

        /// <summary>
        /// Retrieves the Domain/Corner to Dff token mapping.
        /// </summary>
        /// <returns>Dff Token map as a Dictionary with Keys=DFF Token and Value=SharedStorage Use dBy VminForwarding.</returns>
        Dictionary<string, string> GetDffTokenMap();

        /// <summary>
        /// Retrieves the IsSinglePointMode token from shared storage.
        /// </summary>
        /// <returns>true if Vmin searches are disabled and should only run a single point.</returns>
        bool IsSinglePointMode();

        /// <summary>
        /// Retrieves the SearchGuardbandEnable token from shared storage.
        /// </summary>
        /// <returns>true if VminTC should enable its SearchGuardband parameter.</returns>
        bool IsSearchGuardbandEnabled();
    }
}
