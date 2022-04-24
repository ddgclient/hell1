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

    /// <inheritdoc/>
    public class AtomArray : FailedArray
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomArray"/> class.
        /// </summary>
        /// <param name="arrayName"> Name of the failing array. </param>
        public AtomArray(string arrayName)
            : base(arrayName)
        {
        }

        /// <summary>
        /// Gets or sets the module(s) for this Atom array failure.
        /// </summary>
        public HashSet<string> Module { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets the core for this Atom array failure.
        /// </summary>
        public string Core { get; set; }

        /// <inheritdoc/>
        public override void ExtractValuesFromLabel(ILabel label, MetadataConfig.PinMappingSet pinMappingSet)
        {
            // NOTES: when examining the groups within a regex, the first group (index 0) will be the entire captured value. The groups following will be the individual MBD values.
            // If CORE# or SLICEID is present, there will be 5 total values present, the 0th index being the entire capture, the others being mapped by the LabelRegexTokens
            var labelRegexTokens = pinMappingSet.GetLabelRegexTokens();
            var prescreenLabelRegex = pinMappingSet.GetPrescreenLabelRegex();

            string labelContent = label.GetName();
            var mbdMatches = prescreenLabelRegex.Matches(labelContent);

            if (mbdMatches.Count < 1)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"RasterMode is supported for this PinMappingsSet, but the prescreenLabelRegex could not match to the given label \"{label.GetName()}\" at address \"{label.GetAddress()}\".");
            }
            else if (mbdMatches.Count > 1)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"The prescreenLabelRegex matched to more than one group in the given label \"{label.GetName()}\" at address \"{label.GetAddress()}\". Invalid state.");
            }

            // Initialize to invalid values
            string core = string.Empty;
            int multiport = int.MinValue;
            int bank = int.MinValue;
            int dword = int.MinValue;

            for (int i = 0; i < labelRegexTokens.Count; i++)
            {
                switch (labelRegexTokens[i])
                {
                    case MetadataConfig.MBD.MULTIPORT:
                        multiport = int.Parse(mbdMatches[0].Groups[i + 1].ToString());
                        break;
                    case MetadataConfig.MBD.BANK:
                        bank = int.Parse(mbdMatches[0].Groups[i + 1].ToString());
                        break;
                    case MetadataConfig.MBD.DWORD:
                        dword = int.Parse(mbdMatches[0].Groups[i + 1].ToString());
                        break;
                    case MetadataConfig.MBD.SLICE:
                        core = mbdMatches[0].Groups[i + 1].ToString();
                        break;
                    default:
                        throw new Prime.Base.Exceptions.TestMethodException($"Unhandled state. Could not map label regex token [{labelRegexTokens[i]}] to Multiport, Bank, Dword, or Slice");
                }
            }

            if (multiport == int.MinValue || bank == int.MinValue || dword == int.MinValue)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"Could not get MBD address from current failing label [{labelContent}] at address [{label.GetAddress()}]");
            }
            else
            {
                this.MBDAddress = new Tuple<int, int, int>(multiport, bank, dword);
            }

            if (core != string.Empty)
            {
                this.Core = core;
            }
            else
            {
                throw new Prime.Base.Exceptions.TestMethodException("Parsing label for Atom defect; cannot find Core within the label.");
            }

            Prime.Services.ConsoleService.PrintDebug($"Label values retrieved from label [{labelContent}]\nMultiport = {this.MBDAddress.Item1}\nBank = {this.MBDAddress.Item2}\nDword = {this.MBDAddress.Item3}\nCore = {this.Core}");
        }

        /// <inheritdoc/>
        public override void ExtractValuesFromPins(List<string> failedPins, MetadataConfig.PinMappingSet pinMappingSet)
        {
            foreach (var pin in failedPins)
            {
                string module = pinMappingSet.GetModuleFromPin(pin);
                this.Module.Add(module);
            }
        }

        /// <inheritdoc/>
        public override void MapToInternalDB(ref DBContainer database)
        {
            foreach (var module in this.Module)
            {
                database.AddNewEntry(this, module, "Module");
            }
        }
    }
}