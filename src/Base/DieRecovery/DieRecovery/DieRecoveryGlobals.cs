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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DieRecoveryBase;
using Prime;
using Prime.SharedStorageService;

[assembly: InternalsVisibleTo("DieRecoveryBase.UnitTest")]
[assembly: InternalsVisibleTo("DieRecoveryCallbacks")]

namespace DDG
{
    /// <summary>
    /// Defines the <see cref="DieRecovery" />.
    /// </summary>
    public static partial class DieRecovery
    {
        /// <summary>
        /// Defines the <see cref="Globals" />.
        /// </summary>
        public static class Globals
        {
            /// <summary>
            /// Gets the name of the DieRecovery SharedStorage Object which contains all the defeature rules.
            /// </summary>
            internal static readonly string DefeatureRulesTableName = "__DDG_DieRecoveryDefeatureRuleTable__";

            /// <summary>
            /// Gets the context of the DieRecovery SharedStorage Object which contains all the defeature rules.
            /// </summary>
            internal static readonly Context DefeatureRulesTableContext = Context.DUT;

            /// <summary>
            /// Gets the name of the DieRecovery SharedStorage Object which contains all the PinMapBase Decoders.
            /// </summary>
            internal static readonly string DieRecoveryPinMapTableName = "__DDG_DieRecoveryPinMapTable__";

            /// <summary>
            /// Gets the context of the DieRecovery SharedStorage Object which contains all the PinMapBase Decoders.
            /// </summary>
            internal static readonly Context DieRecoveryPinMapTableContext = Context.DUT;

            /// <summary>
            /// Gets the name of the DieRecovery SharedStorage Object which contains all the PinMapBase Decoders Types.
            /// </summary>
            internal static readonly string DieRecoveryTypesTableName = "__DDG_DieRecoveryPinMapTypeTable__";

            /// <summary>
            /// Gets the context of the DieRecovery SharedStorage Object which contains all the PinMapBase Decoders Types.
            /// </summary>
            internal static readonly Context DieRecoveryTypesTableContext = Context.DUT;

            /// <summary>
            /// Gets the name of the DieRecovery SharedStorage Object which contains all the Tracker data.
            /// </summary>
            internal static readonly string DieRecoveryTrackerTableName = "__DDG_DieRecoveryTrackerTable__";

            /// <summary>
            /// Gets the context of the DieRecovery SharedStorage Object which contains all the Tracker Definitions.
            /// </summary>
            internal static readonly Context DieRecoveryTrackerTableContext = Context.DUT;

            /// <summary>
            /// Gets the string used as a separator in shared storage naming.
            /// </summary>
            internal static readonly string NameSeparator = "!";

            /// <summary>
            /// Gets the name of the DieRecovery SharedStorage Object which contains all the Tracker data.
            /// </summary>
            internal static readonly string DieRecoveryTrackerDataName = "__DDG_DieRecoveryTrackerData__";

            /// <summary>
            /// Gets the context of the DieRecovery SharedStorage Object which contains all the Current Tracker data.
            /// </summary>
            internal static readonly Context DieRecoveryTrackerDataContext = Context.DUT;

            /// <summary>
            /// Gets the name of the DieRecovery SharedStorage Integer which indicates whether DownBins are allowed (1=allowed, 0=not allowed).
            /// </summary>
            internal static readonly string DieRecoveryTrackerDownBinsAllowed = "__DDG_DieRecoveryGlobals__!DownBinsAllowed";

            /// <summary>
            /// Gets the context of the Global DieRecovery SharedStorage Controls.
            /// </summary>
            internal static readonly Context DieRecoveryTrackerGlobalContext = Context.DUT;
        }

        /*
        /// <summary>
        /// Gets or sets Pin map creator.
        /// </summary>
        public static IPinMapFactory PinMapCreator { get; set; } = DDG.PinMap.Service;

        /// <summary>
        /// Gets or sets Die Recovery creator.
        /// </summary>
        public static IDieRecoveryFactory DieRecoveryCreator { get; set; } = DDG.DieRecovery.Service; */

        /// <summary>
        /// Defines the <see cref="Utilities" />.
        /// </summary>
        public static class Utilities
        {
            /// <summary>
            /// Adds a new PinMapDecoder to the full PinMapDecoder Table.
            /// </summary>
            /// <param name="pinMapDecoder">A list of <see cref="IPinMapDecoder"/> containing all the data to store.</param>
            internal static void StorePinMapDecoder(PinMapDecoderBase pinMapDecoder)
            {
                // Save the decoder.
                var name = Globals.DieRecoveryPinMapTableName + Globals.NameSeparator + pinMapDecoder.Name;
                var context = Globals.DieRecoveryPinMapTableContext;
                Services.SharedStorageService.InsertRowAtTable(name, pinMapDecoder, context);
                Services.SharedStorageService.OverrideObjectRowResetPolicy(name, ResetPolicy.NEVER_RESET, context);

                // Update the types table.
                name = Globals.DieRecoveryTypesTableName + Globals.NameSeparator + pinMapDecoder.Name;
                Services.SharedStorageService.InsertRowAtTable(name, pinMapDecoder.GetType().FullName, context);
                Services.SharedStorageService.OverrideStringRowResetPolicy(name, ResetPolicy.NEVER_RESET, context);

                // Update the full list of pinmaps.
                var pinMapNames = GetAllPinMapNames();
                if (!pinMapNames.Contains(pinMapDecoder.Name))
                {
                    pinMapNames.Add(pinMapDecoder.Name);
                    Services.SharedStorageService.InsertRowAtTable(Globals.DieRecoveryPinMapTableName, string.Join(",", pinMapNames), context);
                    Services.SharedStorageService.OverrideStringRowResetPolicy(Globals.DieRecoveryPinMapTableName, ResetPolicy.NEVER_RESET, context);
                }
            }

            /// <summary>
            /// Reads the full DieRecovery PinMapBase table from shared storage.
            /// </summary>
            /// <param name="decoderName">Name of the decoder.</param>
            /// <returns>A list of <see cref="IPinMapDecoder"/>.</returns>
            /// <exception cref="Prime.Base.Exceptions">Thrown when the table doesn't exist.</exception>
            internal static IPinMapDecoder RetrievePinMapDecoder(string decoderName)
            {
                var context = Globals.DieRecoveryPinMapTableContext;
                var type = Services.SharedStorageService.GetStringRowFromTable(Globals.DieRecoveryTypesTableName + Globals.NameSeparator + decoderName, context);

                var pinMap = (IPinMapDecoder)Services.SharedStorageService.GetRowFromTable(Globals.DieRecoveryPinMapTableName + Globals.NameSeparator + decoderName, Type.GetType(type), context);
                return pinMap;
            }

            /// <summary>
            /// Gets the list of all existing PinMaps.
            /// </summary>
            /// <returns>list of strings.</returns>
            internal static List<string> GetAllPinMapNames()
            {
                var name = Globals.DieRecoveryPinMapTableName;
                var context = Globals.DieRecoveryPinMapTableContext;
                if (Services.SharedStorageService.KeyExistsInStringTable(name, context))
                {
                    var data = Services.SharedStorageService.GetStringRowFromTable(name, context);
                    return data.Split(',').ToList();
                }

                return new List<string>();
            }

            /// <summary>
            /// Checks if the tracker has stored data.
            /// </summary>
            /// <param name="trackerName">Name of the Tracker.</param>
            /// <returns>Data in binary string format.</returns>
            internal static bool HasTrackerData(string trackerName)
            {
                var name = Globals.DieRecoveryTrackerDataName + Globals.NameSeparator + trackerName;
                var context = Globals.DieRecoveryTrackerDataContext;
                return Services.SharedStorageService.KeyExistsInStringTable(name, context) &&
                       !string.IsNullOrEmpty(Services.SharedStorageService.GetStringRowFromTable(name, context));
            }

            /// <summary>
            /// Gets the tracker data from shared storage.
            /// </summary>
            /// <param name="trackerName">Name of the Tracker.</param>
            /// <returns>Data in binary string format.</returns>
            internal static string RetrieveTrackerData(string trackerName)
            {
                var name = Globals.DieRecoveryTrackerDataName + Globals.NameSeparator + trackerName;
                var context = Globals.DieRecoveryTrackerDataContext;
                return Services.SharedStorageService.GetStringRowFromTable(name, context);
            }

            /// <summary>
            /// Stores data for the given tracker.
            /// </summary>
            /// <param name="trackerName">Name of the tracker.</param>
            /// <param name="data">Data to store.</param>
            internal static void StoreTrackerData(string trackerName, string data)
            {
                var name = Globals.DieRecoveryTrackerDataName + Globals.NameSeparator + trackerName;
                var context = Globals.DieRecoveryTrackerDataContext;
                Services.SharedStorageService.InsertRowAtTable(name, data, context);
            }

            /// <summary>
            /// Gets the tracker definition (Not the data) from shared storage.
            /// </summary>
            /// <param name="trackerName">Name of the tracker.</param>
            /// <returns>Tracker Definition <see cref="Tracker"/>.</returns>
            internal static Tracker RetrieveTrackerDefinition(string trackerName)
            {
                var name = Globals.DieRecoveryTrackerTableName + Globals.NameSeparator + trackerName;
                var context = Globals.DieRecoveryTrackerTableContext;
                return (Tracker)Services.SharedStorageService.GetRowFromTable(name, typeof(Tracker), context);
            }

            /// <summary>
            /// Stores the full DieRecovery Tracker table in shared storage.
            /// This is the read-only container used to store the original tracker file.
            /// </summary>
            /// <param name="tracker">Full table <see cref="Tracker"/> containing all the tracker data to store.</param>
            internal static void StoreTrackerDefinition(Tracker tracker)
            {
                var name = Globals.DieRecoveryTrackerTableName + Globals.NameSeparator + tracker.Name;
                var context = Globals.DieRecoveryTrackerTableContext;
                Services.ConsoleService.PrintDebug($"<DieRecoveryGlobals.StoreTrackerDefinition> Attempting to store data to shared storage [{context}.{name}]");
                Services.SharedStorageService.InsertRowAtTable(name, tracker, context);
                Services.SharedStorageService.OverrideObjectRowResetPolicy(name, ResetPolicy.NEVER_RESET, context);

                var trackerNames = GetAllTrackerNames();
                if (!trackerNames.Contains(tracker.Name))
                {
                    trackerNames.Add(tracker.Name);
                    Services.SharedStorageService.InsertRowAtTable(Globals.DieRecoveryTrackerTableName, string.Join(",", trackerNames), context);
                    Services.SharedStorageService.OverrideStringRowResetPolicy(Globals.DieRecoveryTrackerTableName, ResetPolicy.NEVER_RESET, context);
                }
            }

            /// <summary>
            /// Gets the list of all existing trackers.
            /// </summary>
            /// <returns>list of strings.</returns>
            internal static List<string> GetAllTrackerNames()
            {
                var name = Globals.DieRecoveryTrackerTableName;
                var context = Globals.DieRecoveryTrackerTableContext;
                if (Services.SharedStorageService.KeyExistsInStringTable(name, context))
                {
                    var data = Services.SharedStorageService.GetStringRowFromTable(name, context);
                    return data.Split(',').ToList();
                }

                return new List<string>();
            }

            /// <summary>
            /// Retrieves an existing tracker and clone all information and data.
            /// </summary>
            /// <param name="existingTrackerName">Reference tracker name.</param>
            /// <param name="newTrackerName">New tracker name.</param>
            internal static void CloneTracker(string existingTrackerName, string newTrackerName)
            {
                // clone tracker definition
                Services.ConsoleService.PrintDebug($"Cloning Tracker=[{existingTrackerName} with new name=[{newTrackerName}]");
                var existingTracker = RetrieveTrackerDefinition(existingTrackerName);
                var newTracker = existingTracker.Clone(newTrackerName);
                StoreTrackerDefinition(newTracker);

                // clone tracker data
                string currentValue = string.Empty;
                if (HasTrackerData(existingTrackerName))
                {
                    currentValue = RetrieveTrackerData(existingTrackerName);
                }

                if (!HasTrackerData(newTrackerName) && string.IsNullOrEmpty(currentValue))
                {
                    return;
                }

                Services.ConsoleService.PrintDebug($"Storing Tracker=[{newTrackerName} Value=[{currentValue}]");
                StoreTrackerData(newTrackerName, new string(currentValue.ToCharArray()));
            }

            /// <summary>
            /// Retrieves the rule from shared storage.
            /// </summary>
            /// <param name="ruleName">Name of rule.</param>
            /// <returns>Rule <see cref="DefeatureRule"/>.</returns>
            internal static DefeatureRule RetrieveRule(string ruleName)
            {
                var name = Globals.DefeatureRulesTableName + Globals.NameSeparator + ruleName;
                var context = Globals.DefeatureRulesTableContext;
                var rule = (DefeatureRule)Services.SharedStorageService.GetRowFromTable(name, typeof(DefeatureRule), context);
                return rule;
            }

            /// <summary>
            /// Stores the full DieRecovery Rules table in shared storage.
            /// </summary>
            /// <param name="data">Dictionary Keys=string (rule name), Values=<see cref="DefeatureRule"/> containing all the data to store.</param>
            internal static void StoreRule(DefeatureRule data)
            {
                var name = Globals.DefeatureRulesTableName + Globals.NameSeparator + data.Name;
                var context = Globals.DefeatureRulesTableContext;
                Services.ConsoleService.PrintDebug($"<DieRecoveryGlobals.StoreRule> Attempting to store data to shared storage [{context}.{name}]");
                Services.SharedStorageService.InsertRowAtTable(name, data, context);
                Services.SharedStorageService.OverrideObjectRowResetPolicy(name, ResetPolicy.NEVER_RESET, context);

                var ruleNames = GetAllRuleNames();
                if (!ruleNames.Contains(data.Name))
                {
                    ruleNames.Add(data.Name);
                    Services.SharedStorageService.InsertRowAtTable(Globals.DefeatureRulesTableName, string.Join(",", ruleNames), context);
                    Services.SharedStorageService.OverrideStringRowResetPolicy(Globals.DefeatureRulesTableName, ResetPolicy.NEVER_RESET, context);
                }
            }

            /// <summary>
            /// Gets the list of all existing Rules.
            /// </summary>
            /// <returns>list of strings.</returns>
            internal static List<string> GetAllRuleNames()
            {
                var name = Globals.DefeatureRulesTableName;
                var context = Globals.DefeatureRulesTableContext;
                if (Services.SharedStorageService.KeyExistsInStringTable(name, context))
                {
                    var data = Services.SharedStorageService.GetStringRowFromTable(name, context);
                    return data.Split(',').ToList();
                }

                return new List<string>();
            }
        }
    }
}
