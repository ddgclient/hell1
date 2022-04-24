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

namespace DieRecoveryBase
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;
    using DDG;
    using Newtonsoft.Json;
    using Prime;

    /// <summary>
    /// Defines the <see cref="DieRecoveryFactory" />.
    /// </summary>
    public class DieRecoveryFactory : IDieRecoveryFactory
    {
        /// <inheritdoc/>
        public IDieRecovery Get(string name)
        {
            return new DieRecoveryTracker(name);
        }

        /// <inheritdoc/>
        public bool AreTrackerChangesAllowed()
        {
            var name = DieRecovery.Globals.DieRecoveryTrackerDownBinsAllowed;
            var context = DieRecovery.Globals.DieRecoveryTrackerGlobalContext;
            if (Services.SharedStorageService.KeyExistsInIntegerTable(name, context))
            {
                return Services.SharedStorageService.GetIntegerRowFromTable(name, context) == 1;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool LoadTrackerFile(string trackerFile)
        {
            Services.ConsoleService.PrintDebug($"Loading Tracker Configuration file {trackerFile}.");
            var localTrackerFile = FileUtilities.GetFile(trackerFile);
            var table = JsonConvert.DeserializeObject<TrackerTable>(File.ReadAllText(localTrackerFile));

            foreach (var tracker in table.Trackers)
            {
                Services.ConsoleService.PrintDebug($"Saving definition for Tracker=[{tracker.Name}] Size=[{tracker.Size}] InitialValue=[{tracker.InitialValue}] LinkDisable=[{string.Join(",", tracker.LinkOnDisable)}].");
                DieRecovery.Utilities.StoreTrackerDefinition(tracker);
            }

            // verify that all linked trackers exist.
            var allTrackers = DieRecovery.Utilities.GetAllTrackerNames();
            var trackerNotFound = false;
            foreach (var tracker in table.Trackers)
            {
                foreach (var linkedTrackerName in tracker.LinkOnDisable)
                {
                    if (!allTrackers.Contains(linkedTrackerName))
                    {
                        Services.ConsoleService.PrintError($"Tracker=[{linkedTrackerName}] Linked from Tracker=[{tracker.Name}] does not exist.");
                        trackerNotFound = true;
                    }
                }
            }

            return !trackerNotFound;
        }

        /// <inheritdoc/>
        public void CloneTracker(string existingTrackerName, string newTrackerName)
        {
            DieRecovery.Utilities.CloneTracker(existingTrackerName, newTrackerName);
        }

        /// <inheritdoc/>
        public bool LoadRulesFile(string rulesFile)
        {
            Services.ConsoleService.PrintDebug($"Loading Rules file {rulesFile}.");

            var readerSettings = new XmlReaderSettings { IgnoreComments = true };
            DefeatureRulesFile rulesFileObj;
            using (var inputFile = XmlReader.Create(FileUtilities.GetFile(rulesFile), readerSettings))
            {
                XmlSerializer x = new XmlSerializer(typeof(DefeatureRulesFile));
                rulesFileObj = (DefeatureRulesFile)x.Deserialize(inputFile);
            }

            List<DefeatureRule> allRules = new List<DefeatureRule>();
            foreach (var wrapper in rulesFileObj.DefeatureRules)
            {
                foreach (var rule in wrapper.Rules)
                {
                    DefeatureRule coreDefeatureRule = new DefeatureRule(rule.Name, rule.Index.RangeToList());
                    foreach (var ruleCondition in rule.Rules)
                    {
                        if (ruleCondition.Mode == "ValidCombinations")
                        {
                            var type = DefeatureRule.RuleType.FullyFeatured;
                            if (ruleCondition.Type.ToLower() == "recovery")
                            {
                                type = DefeatureRule.RuleType.Recovery;
                            }

                            coreDefeatureRule.Add(DefeatureRule.RuleMode.ValidCombinations, ruleCondition.Name, int.Parse(ruleCondition.Size), type, ruleCondition.BitVectors.Select(o => o.Value.ToBitArray()).ToList());
                        }
                        else
                        {
                            Services.ConsoleService.PrintError($"Unknown Mode=[{ruleCondition.Mode}] for Rule=[{rule.Name}.{ruleCondition.Name}]");
                            return false;
                        }
                    }

                    allRules.Add(coreDefeatureRule);
                }
            }

            foreach (var rule in allRules)
            {
                Services.ConsoleService.PrintDebug($"Saving definition for Rule=[{rule.Name}].");
                DieRecovery.Utilities.StoreRule(rule);
            }

            return true;
        }
    }
}
