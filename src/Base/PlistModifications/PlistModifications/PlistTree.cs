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
    using System.Linq;
    using Prime.ConsoleService;
    using Prime.PlistService;

    /// <summary>
    /// Defines a plist tree.
    /// </summary>
    public class PlistTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlistTree"/> class.
        /// </summary>
        /// <param name="parentPlist">Parent plist name.</param>
        public PlistTree(string parentPlist)
        {
            this.Console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
            this.PlistName = parentPlist;
            this.PlistObject = Prime.Services.PlistService.GetPlistObject(parentPlist);
            this.Contents = this.PlistObject.GetPatternsAndIndexes(false);
            this.RestoreElementsOptions = new Dictionary<uint, Dictionary<string, string>>();
            this.CurrentElementsOptions = new Dictionary<uint, Dictionary<string, string>>();
            this.Children = new Dictionary<uint, PlistTree>();
            foreach (var item in this.Contents.Where(item => !item.IsPattern()))
            {
                var plistName = item.GetPlistItemName();
                var childTree = DDG.PlistModifications.Service.BuildPlistTree(plistName);
                this.Children.Add(item.GetPatternIndex(), childTree);
            }
        }

        /// <summary>
        /// Gets the parent PlistName.
        /// </summary>
        public string PlistName { get; }

        /// <summary>
        /// Gets the plist children objects.
        /// </summary>
        public Dictionary<uint, PlistTree> Children { get; }

        /// <summary>
        /// Gets or sets a value indicating whether plist is dirty.
        /// </summary>
        public bool DirtyFlag { get; set; } = false;

        /// <summary>
        /// Gets the Plist object reference.
        /// </summary>
        public IPlistObject PlistObject { get; }

        /// <summary>
        /// Gets the plist contents.
        /// </summary>
        public List<IPlistContent> Contents { get; }

        /// <summary>
        /// Gets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; }

        /// <summary>
        /// Gets or sets current PreBurstPlist name.
        /// </summary>
        private string CurrentPreBurstPlist { get; set; }

        /// <summary>
        /// Gets or sets restore PreBurstPlist name.
        /// </summary>
        private string RestorePreBurstPlist { get; set; }

        /// <summary>
        /// Gets or sets the element options to restore.
        /// </summary>
        private Dictionary<uint, Dictionary<string, string>> RestoreElementsOptions { get; set; }

        /// <summary>
        /// Gets or sets the current element options.
        /// </summary>
        private Dictionary<uint, Dictionary<string, string>> CurrentElementsOptions { get; set; }

        /// <summary>
        /// Recursive find for PlistTree.
        /// </summary>
        /// <param name="plistName">Plist name.</param>
        /// <returns>Node.</returns>
        public PlistTree Find(string plistName)
        {
            if (string.IsNullOrEmpty(plistName) || this.PlistName == plistName)
            {
                return this;
            }

            var found = this.Children.Select(o => o.Value).FirstOrDefault(o => o.PlistName == plistName);
            if (found != null)
            {
                return found;
            }

            foreach (var child in this.Children)
            {
                return child.Value.Find(plistName);
            }

            return null;
        }

        /// <summary>
        /// Updates single plist element option.
        /// </summary>
        /// <param name="index">Plist element index.</param>
        /// <param name="option">Plist element option pair.</param>
        public void UpdateSinglePlistElementOption(uint index, KeyValuePair<string, string> option)
        {
            if (!this.RestoreElementsOptions.ContainsKey(index))
            {
                this.RestoreElementsOptions.Add(index, new Dictionary<string, string>());
                this.CurrentElementsOptions.Add(index, new Dictionary<string, string>());
            }

            if (!this.RestoreElementsOptions[index].ContainsKey(option.Key))
            {
                var originalValue = string.Empty;
                try
                {
                    this.PlistObject.GetElementOption(index, option.Key);
                }
                catch
                {
                    // ignored
                }

                this.RestoreElementsOptions[index][option.Key] = originalValue;
                this.CurrentElementsOptions[index][option.Key] = originalValue;
            }

            if (this.CurrentElementsOptions[index][option.Key] == option.Value)
            {
                this.Console?.PrintDebug($"CCR: Skipping setting Plist=[{this.PlistName}] Index=[{index}] Option=[{option.Key}] Value=[{option.Value}] since it was last value set.");
            }
            else
            {
                this.Console?.PrintDebug($"CCR: Setting Plist=[{this.PlistName}] Index=[{index}] Option=[{option.Key}] Value=[{option.Value}].");
                if (string.IsNullOrEmpty(option.Value))
                {
                    this.PlistObject.RemoveElementOption(index, option.Key);
                }
                else
                {
                    this.PlistObject.SetElementOption(index, option.Key, option.Value);
                }

                this.CurrentElementsOptions[index][option.Key] = option.Value;
                this.DirtyFlag = true;
            }
        }

        /// <summary>
        /// Restore plist contents to its original values.
        /// </summary>
        public void RestorePlist()
        {
            if (this.DirtyFlag && this.RestorePreBurstPlist != null)
            {
                this.Console?.PrintDebug($"CCR: Restoring Plist=[{this.PlistName}] PreBurstPlist=[{this.RestorePreBurstPlist}].");
                this.UpdatePreBurstPList(this.RestorePreBurstPlist);
            }

            foreach (var item in this.Contents)
            {
                var index = item.GetPatternIndex();
                if (this.DirtyFlag && item.IsPattern())
                {
                    if (!this.RestoreElementsOptions.ContainsKey(index))
                    {
                        continue;
                    }

                    foreach (var option in this.RestoreElementsOptions[index])
                    {
                        this.UpdateSinglePlistElementOption(index, option);
                    }
                }
                else if (!item.IsPattern())
                {
                    this.Children[index].RestorePlist();
                }
            }

            this.DirtyFlag = false;
        }

        /// <summary>
        /// Updates pre burst plist option.
        /// </summary>
        /// <param name="preBurstPlist">Value.</param>
        public void UpdatePreBurstPList(string preBurstPlist)
        {
            if (this.RestorePreBurstPlist == null)
            {
                try
                {
                    var optionValue = this.PlistObject.GetOption("PreBurstPList");
                    this.RestorePreBurstPlist = optionValue;
                    this.CurrentPreBurstPlist = optionValue;
                }
                catch
                {
                    this.RestorePreBurstPlist = string.Empty;
                    this.CurrentPreBurstPlist = string.Empty;
                }
            }

            if (this.CurrentPreBurstPlist == preBurstPlist)
            {
                this.Console?.PrintDebug($"CCR: Skipping plist=[{this.PlistName}] PreBurstPList=[{preBurstPlist}] since it matches current value.");
                return;
            }

            if (string.IsNullOrEmpty(preBurstPlist))
            {
                this.Console?.PrintDebug($"CCR: Updating plist=[{this.PlistName}] removing PreBurstPList.");
                this.PlistObject.RemoveOptions(new List<string> { "PreBurstPList" });
                this.CurrentPreBurstPlist = string.Empty;
            }
            else
            {
                this.Console?.PrintDebug($"CCR: Updating plist=[{this.PlistName}] with PreBurstPList=[{preBurstPlist}].");
                this.PlistObject.SetOption("PreBurstPList", preBurstPlist);
                this.CurrentPreBurstPlist = preBurstPlist;
            }

            this.DirtyFlag = true;
        }
    }
}
