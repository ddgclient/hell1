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
    using System.Text.RegularExpressions;

    /// <summary> This class represents the deserialized version of the user Metadata configuration. Contains helper methods to help retrieve data without diving into its structure. </summary>
    public class MetadataConfig
    {
        private static Dictionary<string, MBD> regexTokenToEnumHelper = new Dictionary<string, MBD>()
        {
            { "SLICE", MBD.SLICE },
            { "MULTIPORT", MBD.MULTIPORT },
            { "BANK", MBD.BANK },
            { "DWORD", MBD.DWORD },
        };

        private static Dictionary<ArrayType, string> arrayTypeToKeyNameHelper = new Dictionary<ArrayType, string>()
        {
            { ArrayType.ATOM, "Module" },
            { ArrayType.BIGCORE, "Slice" },
        };

        /// <summary>
        /// Enum names for MBD to map to their values. Prevents dictonary from containing values other than ones specified here.
        /// </summary>
        public enum MBD
        {
            /// <summary> Enum to represent Slice. </summary>
            SLICE,

            /// <summary> Enum to represent Multiport </summary>
            MULTIPORT,

            /// <summary> Enum to represent Bank </summary>
            BANK,

            /// <summary> Enum to represent Dword </summary>
            DWORD,
        }

        /// <summary>
        /// This ENUM represents the type of array we're dealing with during execution.
        /// </summary>
        public enum ArrayType
        {
            /// <summary>
            /// Enum indicating that the MetadataConfig is currently supporting ATOM products.
            /// </summary>
            ATOM,

            /// <summary>
            /// Enum indicating that the MetadataConfig is currently supporting BigCore products.
            /// </summary>
            BIGCORE,
        }

        /// <summary>
        /// Gets or sets root for this JSON object.
        /// </summary>
        public Root Setup { get; set; }

        /// <summary>
        /// Converts a label regex token to its MBD enum equivalent.
        /// </summary>
        /// <param name="token"> String representing the label regex token. </param>
        /// <returns> MBD value corresponding to the regex token. </returns>
        public static MBD ConvertLabelRegexTokenToEnum(string token)
        {
            try
            {
                return regexTokenToEnumHelper[token];
            }
            catch (KeyNotFoundException ex)
            {
                Prime.Services.ConsoleService.PrintError($"LabelRegexToken [{token}] is not valid. Valid tokens are: SLICE, MULTIPORT, BANK, DWORD");
                throw ex;
            }
        }

        /// <summary>
        /// Converts array type to the root key used to store failInfo in the internalDB. Used mainly for printing to debug log.
        /// </summary>
        /// <param name="type"> Type of array stored in the DB. </param>
        /// <returns> A string containing the name of the root key. </returns>
        public static string ConvertArrayTypeToDBKeyName(ArrayType type)
        {
            try
            {
                return arrayTypeToKeyNameHelper[type];
            }
            catch (KeyNotFoundException ex)
            {
                Prime.Services.ConsoleService.PrintError($"Array type [{type}] is not recognized");
                throw ex;
            }
        }

        /// <summary>
        /// Try and fetch a PinMappingSet corresponding to the given pinMappingSetKey.
        /// </summary>
        /// <param name="pinMappingSetKey"> String representing the string key that matches to a pinMappingSet obj. </param>
        /// <returns> A <see cref="PinMappingSet"/> object. </returns>
        public PinMappingSet GetPinMappingSet(string pinMappingSetKey)
        {
            PinMappingSet pinMappingSet = null;
            try
            {
                pinMappingSet = this.Setup.SlicePinMapping[pinMappingSetKey];
            }
            catch (KeyNotFoundException ex)
            {
                Prime.Services.ConsoleService.PrintError($"Could not find PinMappingSet [{pinMappingSetKey}] in current MetadataConfig");
                throw ex;
            }

            return pinMappingSet;
        }

        /// <summary>
        /// Try and fetch a CaptureSet corresponding to the given CaptureSet name.
        /// </summary>
        /// <param name="captureSetKey"> String representing the string key that matches to a captureSet obj. </param>
        /// <returns> <see cref="CaptureSet"/> instance. </returns>
        public CaptureSet GetCaptureSet(string captureSetKey)
        {
            CaptureSet captureSet = null;
            try
            {
                captureSet = this.Setup.CaptureConfigSets[captureSetKey];
            }
            catch (KeyNotFoundException ex)
            {
                Prime.Services.ConsoleService.PrintError($"Could not find CaptureSet \"{captureSetKey}\" in current MetadataConfig");
                throw ex;
            }

            return captureSet;
        }

        /// <summary>
        /// Root object for this JSON object.
        /// </summary>
        public class Root
        {
            /// <summary>
            /// Gets or sets PatModConfigSet for this object instance.
            /// </summary>
            public Dictionary<string, List<string>> PatModConfigSets { get; set; }

            /// <summary>
            /// Gets or sets CaptureConfigSets for this JSON object instance.
            /// </summary>
            public Dictionary<string, CaptureSet> CaptureConfigSets { get; set; }

            /// <summary>
            /// Gets or sets SlicePinMapping for this JSON object instance.
            /// </summary>
            public Dictionary<string, PinMappingSet> SlicePinMapping { get; set; }
        }

        /// <summary> Json property containing info for CaptureSet. </summary>
        public class CaptureSet
        {
            /// <summary> Gets or sets Length for this CaptureSet. </summary>
            public int Length { get; set; }

            /// <summary> Gets or sets mapping of DecodingElements to their respective name. </summary>
            public Dictionary<string, DecodingElement> DecodingElements { get; set; }
        }

        /// <summary> Json property containing info for DecodingElement. </summary>
        public class DecodingElement
        {
            /// <summary> Gets or sets Start for this DecodingElement. </summary>
            public int Start { get; set; }

            /// <summary> Gets or sets End for this Decoding Element. </summary>
            public int End { get; set; }
        }

        /// <summary> Class representing deserialized version of the PinMappingSet. Contains helper methods to access info inside the config without need of diving into its structure. </summary>
        public class PinMappingSet
        {
            private List<MBD> convertedRegexTokens;
            private Regex arrayNameRegex;
            private Regex prescreenLabelRegex;

            /// <summary> Gets or sets a value indicating whether multicore patterns are enabled for this PinMappingSet. </summary>
            public bool MulticorePatternEnabled { get; set; }

            /// <summary> Gets or sets a value indicating whether patterns per slice are enabled for this PinMappingSet. </summary>
            public bool HasPatternPerSliceId { get; set; } = false;

            /// <summary> Gets or sets Configurations for this PinMappingSet. </summary>
            public Configurations Configurations { get; set; }

            /// <summary> Gets or sets a list of PinMapping for this PinMappingSet. </summary>
            public List<PinMapping> PinMappings { get; set; }

            /// <summary>
            /// Method for validating and converting config properties to objects for execution.
            /// </summary>
            /// <returns> A value representing whether this PinMappingSet is valid and successfuly converted into the proper objects. </returns>
            public bool ValidateAndSetupItems()
            {
                int patNameDetected;
                bool initialValidation = this.ValidatePinMappings(out patNameDetected) & this.ValidateConfigurations() & this.ValidateMulticoreRequirements();

                // Conversion of configuration to member objects for testtime saving
                if (initialValidation)
                {
                    this.ConvertLabelRegexTokensToEnums();
                    this.ConvertRegexStringsToRegexObjects();
                    this.HasPatternPerSliceId = patNameDetected > 0;
                }

                return initialValidation;
            }

            /// <summary>
            /// Determine what product is supported by the current PinMappingSet.
            /// </summary>
            /// <returns> An enum representing the product that this PinMappingSet supports. </returns>
            public ArrayType GetArrayType()
            {
                if (!string.IsNullOrEmpty(this.PinMappings[0].Module))
                {
                    return ArrayType.ATOM;
                }
                else if (!string.IsNullOrEmpty(this.PinMappings[0].SliceId))
                {
                    return ArrayType.BIGCORE;
                }
                else
                {
                    throw new Prime.Base.Exceptions.TestMethodException("Cannot determine the current ArrayType for this execution of the TC.");
                }
            }

            /// <summary>
            /// Given a list of slices, convert them into pins for ctv capture.
            /// </summary>
            /// <param name="keys"> List of strings containing slices. </param>
            /// <returns> List of pins for ctv captures. </returns>
            public List<string> GetPinMappedToRootKey(List<string> keys)
            {
                List<string> pins = new List<string>();

                switch (this.GetArrayType())
                {
                    case ArrayType.ATOM:

                        foreach (var map in this.PinMappings)
                        {
                            if (keys.Contains(map.Module))
                            {
                                pins.Add(map.PinName);
                            }
                        }

                        if (pins.Count != keys.Count)
                        {
                            throw new Prime.Base.Exceptions.TestMethodException($"Could not map modules to pins for current PinMappingSet");
                        }

                        break;

                    case ArrayType.BIGCORE:

                        foreach (var map in this.PinMappings)
                        {
                            if (keys.Contains(map.SliceId))
                            {
                                pins.Add(map.PinName);
                            }
                        }

                        if (pins.Count != keys.Count)
                        {
                            throw new Prime.Base.Exceptions.TestMethodException($"Could not map slices to pins for current PinMappingSet");
                        }

                        break;

                    default:
                        throw new Prime.Base.Exceptions.TestMethodException($"Cannot determine if product is mapped to Module or Slice for current product [{this.GetArrayType()}]");
                }

                return pins;
            }

            /// <summary>
            /// Given a slice, convert into pin for ctv capture.
            /// </summary>
            /// <param name="key"> String containing slice. </param>
            /// <returns> Pin name for ctv captures. </returns>
            public string GetPinMappedToRootKey(string key)
            {
                switch (this.GetArrayType())
                {
                    case ArrayType.BIGCORE:

                        foreach (var map in this.PinMappings)
                        {
                            if (map.SliceId == key)
                            {
                                return map.PinName;
                            }
                        }

                        throw new Prime.Base.Exceptions.TestMethodException($"Could not map slice to any pin for current PinMappingSet");
                    case ArrayType.ATOM:

                        foreach (var map in this.PinMappings)
                        {
                            if (map.Module == key)
                            {
                                return map.PinName;
                            }
                        }

                        throw new Prime.Base.Exceptions.TestMethodException($"Could not map slice to any pin for current PinMappingSet");

                    default:
                        throw new Prime.Base.Exceptions.TestMethodException($"Could not determine type of info to retrieve for pin for given product [{this.GetArrayType()}]");
                }
            }

            /// <summary>
            /// Fetch ArrayNameRegex from this PinMappingSet.
            /// </summary>
            /// <returns> A string containing the ArrayNameRegex pattern. </returns>
            public Regex GetArrayNameRegex()
            {
                return this.arrayNameRegex;
            }

            /// <summary>
            /// Fetch PrescreenLabelRegex from this PinMappingSet.
            /// </summary>
            /// <returns> A string containing the PrescreenLabelRegex pattern. </returns>
            public Regex GetPrescreenLabelRegex()
            {
                return this.prescreenLabelRegex;
            }

            /// <summary>
            /// Fetch LabelRegexTokens from this PinMappingSet.
            /// </summary>
            /// <returns> A list of strings containing label regex tokens. </returns>
            public List<MBD> GetLabelRegexTokens()
            {
                return this.convertedRegexTokens;
            }

            /// <summary>
            /// Fetch IsRasterModeSupported from this PinMappingSet.
            /// </summary>
            /// <returns> A value indicating whether Raster is supported for a given PinMappingSet. </returns>
            public bool IsRasterModeSupported()
            {
                return this.Configurations.IsRasterModeSupported;
            }

            /// <summary>
            /// Fetch GetDwordFromFailIO value from this PinMappingSet.
            /// </summary>
            /// <returns> A value indicating whether to use FailIo as DWORD value for a given defect. </returns>
            public bool IsGetDwordFromFailIO()
            {
                return this.Configurations.IsGetDwordFromFailIoIndex;
            }

            /// <summary>
            /// Given a pin name, find the SliceId mapped to that pin.
            /// </summary>
            /// <param name="pin"> Name of the pin which is mapped to a SliceId. </param>
            /// <param name="hryIdentifier"> HryIdentifier from the pin mapping used to map the given pin. </param>
            /// <returns> SliceId mapped to the given pin. </returns>
            public List<string> GetSliceIdFromPin(string pin, out string hryIdentifier)
            {
                List<string> slices = new List<string>();
                hryIdentifier = string.Empty;

                foreach (var pinMapping in this.PinMappings)
                {
                    if (string.Equals(pinMapping.PinName, pin))
                    {
                        var sliceRange = SharedFunctions.ParseRange(pinMapping.SliceId);

                        foreach (int slice in sliceRange)
                        {
                            slices.Add(slice.ToString());
                        }

                        if (slices.Count > 1)
                        {
                            hryIdentifier = pinMapping.HryPrefix;
                        }
                        else
                        {
                            hryIdentifier = pinMapping.HryName;
                        }
                    }
                }

                return slices;
            }

            /// <summary>
            /// Fetch module given the name of a pin.
            /// </summary>
            /// <param name="pin"> The name of the pin that is mapped to a module. </param>
            /// <returns> The module that the pin is mapped to. </returns>
            public string GetModuleFromPin(string pin)
            {
                foreach (var pinMapping in this.PinMappings)
                {
                    if (string.Equals(pinMapping.PinName, pin))
                    {
                        return pinMapping.Module;
                    }
                }

                throw new Prime.Base.Exceptions.TestMethodException($"Could not find module for given pin [{pin}]");
            }

            /// <summary>
            /// Get HryName mapped to the given sliceId.
            /// </summary>
            /// <param name="sliceId"> SliceId that is mapped to an HryName. </param>
            /// <returns> HryName of the given sliceId. </returns>
            public string GetHryNameFromSliceId(string sliceId)
            {
                foreach (var pinMapping in this.PinMappings)
                {
                    if (pinMapping.SliceId == sliceId)
                    {
                        return pinMapping.HryName;
                    }
                }

                return null;
            }

            /// <summary>
            /// Method to get all pins to mask given slice(s) we're currently iterating over.
            /// </summary>
            /// <param name="slices"> All slices we're currently interested in. </param>
            /// <returns> A list of pins that aren't mapped to the list of slices. </returns>
            public List<string> GetPinsToMask(List<string> slices)
            {
                List<string> maskPins = new List<string>();
                string pinMappingSlice = string.Empty;

                foreach (var pinMapping in this.PinMappings)
                {
                    if (!string.IsNullOrEmpty(pinMapping.Module))
                    {
                        pinMappingSlice = pinMapping.Module;
                    }
                    else
                    {
                        pinMappingSlice = pinMapping.SliceId;
                    }

                    string pinMappingPin = pinMapping.PinName;
                    Prime.Services.ConsoleService.PrintDebug($"pinMappingSlice: [{pinMappingSlice}], pinMappingPin: [{pinMappingPin}]");

                    if (!slices.Contains(pinMappingSlice))
                    {
                        Prime.Services.ConsoleService.PrintDebug($"Masking {pinMappingPin}, it is not the sliceid/modle that we are looking at");
                        maskPins.Add(pinMappingPin);
                    }
                }

                return maskPins;
            }

            /// <summary>
            /// Get PatName mapped to the given sliceId.
            /// </summary>
            /// <param name="sliceId"> SliceId that is mapped to an PatName. </param>
            /// <returns> HryName of the given sliceId. </returns>
            public string GetPatternMappedToSlice(string sliceId)
            {
                foreach (var pinMapping in this.PinMappings)
                {
                    if (pinMapping.SliceId == sliceId)
                    {
                        return pinMapping.PatName;
                    }
                }

                return null;
            }

            /// <summary>
            /// Method for determining if we should get slice from label when performing prescreen algorithm.
            /// </summary>
            /// <returns> A value indicating whether to use label as a source for SliceId. </returns>
            public bool IsGetSliceFromLabel()
            {
                return this.convertedRegexTokens.Contains(MBD.SLICE);
            }

            /// <summary>
            /// Method to return all available slices in this PinMappingSet.
            /// </summary>
            /// <returns> List of slices contained in this pinMappingSet. </returns>
            public List<string> GetSliceList()
            {
                List<string> slices = new List<string>();

                foreach (var pinMapping in this.PinMappings)
                {
                    List<int> parsedSlices = SharedFunctions.ParseRange(pinMapping.SliceId);

                    foreach (var slice in parsedSlices)
                    {
                        slices.Add(slice.ToString());
                    }
                }

                return slices;
            }

            private bool ValidateMulticoreRequirements()
            {
                if (this.MulticorePatternEnabled && !this.IsGetSliceFromLabel())
                {
                    Prime.Services.ConsoleService.PrintError("If MulticorePatternEnabled is set to true, SLICE must be present in the LabelRegexTokens for this pinMappingSet.");
                    return false;
                }

                return true;
            }

            private bool ValidateConfigurations()
            {
                bool isValid = true;

                if (string.IsNullOrWhiteSpace(this.Configurations.ArrayNameRegex))
                {
                    Prime.Services.ConsoleService.PrintError("Current PinMappingSet is missing ArrayNameRegex");
                    isValid = false;
                }

                if (string.IsNullOrWhiteSpace(this.Configurations.PrescreenLabelRegex))
                {
                    Prime.Services.ConsoleService.PrintError("Current PinMappingSet is missing PrescreenLabelRegex");
                    isValid = false;
                }

                if (SharedFunctions.IsPropertyNull(this.Configurations.LabelRegExTokens, "LabelRegExTokens"))
                {
                    isValid = false;
                }
                else
                {
                    HashSet<string> tempChecker = new HashSet<string>();

                    foreach (var token in this.Configurations.LabelRegExTokens)
                    {
                        if (tempChecker.Contains(token))
                        {
                            Prime.Services.ConsoleService.PrintError($"LabelRegexTokens contains more than one instance of the token [{token}]");
                            isValid = false;
                        }

                        tempChecker.Add(token);
                    }
                }

                return isValid;
            }

            private void ConvertRegexStringsToRegexObjects()
            {
                this.arrayNameRegex = new Regex(this.Configurations.ArrayNameRegex);
                this.prescreenLabelRegex = new Regex(this.Configurations.PrescreenLabelRegex);
            }

            private void ConvertLabelRegexTokensToEnums()
            {
                var tokens = new List<MBD>();

                foreach (var token in this.Configurations.LabelRegExTokens)
                {
                    var mbdValue = ConvertLabelRegexTokenToEnum(token);
                    tokens.Add(mbdValue);
                }

                this.convertedRegexTokens = tokens;
            }

            private bool ValidatePinMappings(out int patNameDetected)
            {
                bool isValid = true;
                patNameDetected = 0;
                int currentPinMapping = 0;
                foreach (var pinMapping in this.PinMappings)
                {
                    if (string.IsNullOrWhiteSpace(pinMapping.SliceId) && string.IsNullOrWhiteSpace(pinMapping.Module))
                    {
                        Prime.Services.ConsoleService.PrintError($"PinMapping index [{currentPinMapping}] for this PinMappingSet is missing SliceId and Module. Define one of these to make this PinMappingSet valid.");
                        isValid = false;
                    }

                    if (!string.IsNullOrWhiteSpace(pinMapping.SliceId) && !string.IsNullOrWhiteSpace(pinMapping.Module))
                    {
                        Prime.Services.ConsoleService.PrintError($"PinMapping index [{currentPinMapping}] for this PinMappingSet is has both SliceId and Module. Only one of these can be defined, not both.");
                        isValid = false;
                    }

                    if (string.IsNullOrWhiteSpace(pinMapping.PinName))
                    {
                        Prime.Services.ConsoleService.PrintError($"PinMapping index [{currentPinMapping}] for this PinMappingSet is missing PinName to be valid.");
                        isValid = false;
                    }

                    if (string.IsNullOrWhiteSpace(pinMapping.HryName) && string.IsNullOrWhiteSpace(pinMapping.HryPrefix))
                    {
                        Prime.Services.ConsoleService.PrintError($"PinMapping index [{currentPinMapping}] for this PinMappingSet is missing both HryName or HryPrefix. Define one of these to make this PinMappingSet valid.");
                        isValid = false;
                    }

                    if (!string.IsNullOrWhiteSpace(pinMapping.HryName) && !string.IsNullOrWhiteSpace(pinMapping.HryPrefix))
                    {
                        Prime.Services.ConsoleService.PrintError($"PinMapping index [{currentPinMapping}] for this PinMappingSet has both HryName or HryPrefix. Only one of these can be defined, not both.");
                        isValid = false;
                    }

                    if (!string.IsNullOrWhiteSpace(pinMapping.PatName))
                    {
                        patNameDetected++;

                        if (this.MulticorePatternEnabled)
                        {
                            Prime.Services.ConsoleService.PrintError("PatName is defined for a PinMapping in this PinMappingSet, but MulticorePatternEnabled is true.");
                            isValid = false;
                        }
                    }

                    currentPinMapping++;
                }

                if (patNameDetected != this.PinMappings.Count && patNameDetected != 0)
                {
                    Prime.Services.ConsoleService.PrintError($"PatName detected for {patNameDetected} PinMapping(s), but detected {this.PinMappings.Count - patNameDetected} PinMapping(s) without a PatName present. PatName must be defined for all or none of the PinMappings.");
                    isValid = false;
                }

                return isValid;
            }
        }

        /// <summary> Json property containing info for Configurations. </summary>
        public class Configurations
        {
            /// <summary> Gets or sets ArrayNameRegex for this Configurations. </summary>
            public string ArrayNameRegex { get; set; }

            /// <summary> Gets or sets a value indicating whether raster mode is supported for this PinMappingSet. </summary>
            public bool IsRasterModeSupported { get; set; } = true;

            /// <summary> Gets or sets a value indicating whether getting Dword from FailIo index is supported. </summary>
            public bool IsGetDwordFromFailIoIndex { get; set; } = false;

            /// <summary> Gets or sets PreScreenLabelRegex for this Configurations. </summary>
            public string PrescreenLabelRegex { get; set; }

            /// <summary> Gets or sets a list of strings representing LabelRegExTokens. </summary>
            public List<string> LabelRegExTokens { get; set; }
        }

        /// <summary> Json property containing info for PinMapping. </summary>
        public class PinMapping
        {
            /// <summary> Gets or sets SliceId for this PinMapping. </summary>
            public string SliceId { get; set; }

            /// <summary> Gets or sets PinName for this PinMapping.. </summary>
            public string PinName { get; set; } = string.Empty;

            /// <summary> Gets or sets PatName for this PinMapping. </summary>
            public string PatName { get; set; } = string.Empty;

            /// <summary> Gets or sets Module for this PinMapping. </summary>
            public string Module { get; set; } = string.Empty;

            /// <summary> Gets or sets HryName for this PinMapping. </summary>
            public string HryName { get; set; }

            /// <summary> Gets or sets HryPrefix for this PinMapping. </summary>
            public string HryPrefix { get; set; }
        }
    }
}