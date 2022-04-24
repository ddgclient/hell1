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
    using Prime.PatConfigService;

    /// <summary>
    /// Class that holds deserialized version of Ldat config for Raster mode.
    /// </summary>
    public class RasterConfig
    {
        private const string MaxDefectsCountOpCode = "MOV ";
        private const string RegisterToModify = "R0";

        private static Dictionary<string, PatTargets> patTargetHelper = new Dictionary<string, PatTargets>()
        {
            { "MULTIPORT", PatTargets.MULTIPORT },
            { "BANK", PatTargets.BANK },
            { "DWORD", PatTargets.DWORD },
            { "MAXDEFECTSCOUNT", PatTargets.MAXDEFECTSCOUNT },
        };

        private static List<PatTargets> requiredPatTargets = new List<PatTargets>
        {
            PatTargets.MULTIPORT,
            PatTargets.BANK,
            PatTargets.DWORD,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterConfig"/> class.
        /// </summary>
        public RasterConfig()
        {
        }

        /// <summary>
        /// Enum to represent all possible patTargets when performing raster.
        /// </summary>
        public enum PatTargets
        {
            // If a new patTarget is added, need to update helper method ConvertStringToPatTarget() and helper object patTargetHelper to reflect this.

            /// <summary> Enum representing a pat target. </summary>
            MULTIPORT,

            /// <summary> Enum representing a pat target. </summary>
            BANK,

            /// <summary> Enum representing a pat target. </summary>
            DWORD,

            /// <summary> Enum representing a pat target. </summary>
            MAXDEFECTSCOUNT,

            /// <summary> Enum representing a pat target. </summary>
            IOMASK,
        }

        /// <summary> Gets or sets name of the Plist for this array. </summary>
        public Root Setup { get; set; }

        /// <summary> Helper method to convert a string to a <see cref="PatTargets"/>. </summary>
        /// <param name="patTarget"> String to convert to a <see cref="PatTargets"/>. </param>
        /// <returns> Value of <see cref="PatTargets"/>. </returns>
        public static PatTargets ConvertStringToPatTarget(PatModifyElement patTarget)
        {
            string expectedValue = patTarget.PatTarget.ToUpper();
            if (patTargetHelper.ContainsKey(expectedValue))
            {
                return patTargetHelper[expectedValue];
            }
            else if (expectedValue.Contains("IOMASK"))
            {
                return PatTargets.IOMASK;
            }
            else
            {
                throw new Prime.Base.Exceptions.TestMethodException($"Could not recognize PatTarget [{patTarget}] as a valid PatTarget.");
            }
        }

        /// <summary>
        /// Verify that all Ldat arrays within this config contain the proper elements within their DwordElements.
        /// </summary>
        /// <returns> A value indicating whether all ldat arrays in this config contain the required targets in their DwordElements. </returns>
        /// <remarks> This is the only check needed to perform after deserialization. The JSON schema takes care of the rest.</remarks>
        public bool Verify()
        {
            return this.CheckIfDwordsAreValid();
        }

        /// <summary>
        /// Get desired LdatArray from internal dictionary.
        /// </summary>
        /// <param name="arrayName"> Name of the array to retrieve. </param>
        /// <returns> <see cref="LdatArray"/> instance linked to passed array name. </returns>
        public LdatArray GetLdatArray(string arrayName)
        {
            try
            {
                return this.Setup.LdatArrays[arrayName];
            }
            catch (KeyNotFoundException)
            {
                Prime.Services.ConsoleService.PrintError($"Could not find Ldat array \"{arrayName}\" in the RasterConfig JSON file.  Raster can't run without this array defined.");
                return null;

                // throw ex;
            }
        }

        /// <summary>
        /// This checks the RasterConfig JSON file to see if RasterExists is true or false.  Default assumes true if not defined.
        /// </summary>
        /// <param name="arrayName">Name of the array.</param>
        /// <returns>true or false which matches the json definition.</returns>
        public bool CheckIfLdatArrayHasRasterEnabled(string arrayName)
        {
            try
            {
                return bool.Parse(this.Setup.LdatArrays[arrayName].RasterExists);
            }
            catch (Exception)
            {
                Prime.Services.ConsoleService.PrintDebug($"\"{arrayName}\" in the RasterConfig JSON file didn't have RasterExists defined.  Assuming raster is supposed to run.");
                return true;
            }
        }

        /// <summary>
        /// Get desired LdatArray from internal dictionary.
        /// </summary>
        /// <param name="name"> Name of the config set to retrieve. </param>
        /// <returns> <see cref="ReductionConfigSet"/> linked to passed name. </returns>
        public ReductionConfigSet GetReductionConfigSet(string name)
        {
            try
            {
                return this.Setup.ReductionConfigSets[name];
            }
            catch (KeyNotFoundException ex)
            {
                Prime.Services.ConsoleService.PrintError($"Could not find the ReductionConfigSet with the name \"{name}\" in the RasterConfig JSON file.");
                throw ex;
            }
        }

        /// <summary>
        /// Method for returning the RasterInParallel configuration option.
        /// </summary>
        /// <returns> A value indicating whether to raster serially or in parallel. </returns>
        public bool IsRasterInParallel()
        {
            return this.Setup.RasterInParallel;
        }

        private bool CheckIfDwordsAreValid()
        {
            // Check all Dword elements if they contain all needed elements.
            foreach (var ldatArray in this.Setup.LdatArrays.Values)
            {
                foreach (var dwordElement in ldatArray.DwordElement)
                {
                    bool multiportExists = false;
                    bool bankExists = false;
                    bool dwordExists = false;

                    foreach (var patModElement in dwordElement.PatModifyElement)
                    {
                        var patTarget = ConvertStringToPatTarget(patModElement);

                        if (patTarget == PatTargets.MULTIPORT)
                        {
                            multiportExists = true;
                        }
                        else if (patTarget == PatTargets.BANK)
                        {
                            bankExists = true;
                        }
                        else if (patTarget == PatTargets.DWORD)
                        {
                            dwordExists = true;
                        }
                    }

                    if (!(multiportExists && bankExists && dwordExists))
                    {
                        return false; // Shouldn't be hit due to schema validation, but good to have in case...
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Root of this configuration object.
        /// </summary>
        public class Root
        {
            /// <summary> Gets or sets a value indicating whether to raster the arrays in the DB in parallel or serially.  </summary>
            public bool RasterInParallel { get; set; } = false;

            /// <summary> Gets or sets mapping of array name to their configurations.  </summary>
            public Dictionary<string, LdatArray> LdatArrays { get; set; }

            /// <summary>
            /// Gets or sets mapping of reduction config set name to the container object.
            /// </summary>
            public Dictionary<string, ReductionConfigSet> ReductionConfigSets { get; set; }
        }

        /// <summary>
        /// Class representing a reductionConfigSet.
        /// </summary>
        public class ReductionConfigSet
        {
            /// <summary>
            /// Gets or sets maximum cores to raster during execution.
            /// </summary>
            public int MaxCoresCount { get; set; }

            /// <summary>
            /// Gets or sets maximum arrays to raster during execution.
            /// </summary>
            public int ArrayMaxCount { get; set; }

            /// <summary>
            /// Gets or sets limit of arrays that can fail per slice before that specific slice is removed.
            /// </summary>
            public int ArrayMAFMax { get; set; }

            /// <summary>
            /// Gets or sets the priority of arrays to keep when considering reductions.
            /// </summary>
            public List<string> ArrayPriority { get; set; }

            /// <summary>
            /// Gets or sets the maximum number of MBDs per array, per slice.
            /// </summary>
            public Dictionary<string, int> MaxMBDsCount { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether to print reductions to Ituff.
            /// </summary>
            public bool ItuffPrint { get; set; }
        }

        /// <summary> Class holding information on a given LdatArray. </summary>
        public class LdatArray
        {
            /// <summary> Gets or sets if raster valid for this array (true false). </summary>
            public string RasterExists { get; set; }

            /// <summary> Gets or sets name(s) of the Plist for this array. </summary>
            public List<string> PlistName { get; set; }

            /// <summary> Gets or sets name of the PatMod for this array. </summary>
            public string PatModConfigSetName { get; set; }

            /// <summary> Gets or sets name of the CaptureSet for this array. </summary>
            public string CaptureSetName { get; set;  }

            /// <summary> Gets or sets name of the Plist for this array. </summary>
            public List<DwordElementContainer> DwordElement { get; set; }

            /// <summary>
            /// Method for converting a <see cref="DwordElementContainer"/> into a list of <see cref="IPatConfigHandle"/>.
            /// </summary>
            /// <param name="dwordElement"> An instance of <see cref="DwordElementContainer"/> to convert. </param>
            /// <param name="plistName"> Name of the plist to apply handles to. </param>
            /// <returns> A list of <see cref="IPatConfigHandle"/>. </returns>
            public List<IPatConfigHandle> CreatePatConfigHandles(DwordElementContainer dwordElement, string plistName)
            {
                List<IPatConfigHandle> handles = new List<IPatConfigHandle>();

                foreach (var element in dwordElement.PatModifyElement)
                {
                    PatTargets patTarget = ConvertStringToPatTarget(element);
                    var plistHandle = Prime.Services.PatConfigService.GetPatConfigHandleWithPlist(this.PatModConfigSetName + "_" + element.PatTarget, plistName);
                    var rawHexValue = element.Value.Substring(2);
                    string binaryPatModValue = DDG.RadixConversion.HexToBinary(rawHexValue);

                    // MaxDefectsCount modifies an OpCode
                    if (patTarget == PatTargets.MAXDEFECTSCOUNT)
                    {
                        // NEED TO DO PATTERN READING TO FIGURE OUT THE REGISTER WE'RE MODIFYING
                        var intPatModValue = DDG.RadixConversion.BinaryToInteger(binaryPatModValue);
                        string modValue_orig = MaxDefectsCountOpCode + $"{intPatModValue}, {RegisterToModify}";
                        string modValue = modValue_orig;
                        var expected_size = plistHandle.GetExpectedDataSize();
                        Prime.Services.ConsoleService.PrintDebug($"Creating PatConfigHandle [{this.PatModConfigSetName + "_" + element.PatTarget}] with value [{modValue}], expected size [{expected_size}]");

                        while (expected_size > 1)
                        {
                            // For instructions | is the separator if multiple are expected in the same configurationset.  This can happen based on multiple domains having the same LABEL
                            modValue = modValue + "|" + modValue_orig;
                            expected_size--;
                        }

                        Prime.Services.ConsoleService.PrintDebug($"POST_PADDED PatConfigHandle [{this.PatModConfigSetName + "_" + element.PatTarget}] with value [{modValue}], expected size [{expected_size}]");
                        plistHandle.SetData(modValue);
                    }
                    else
                    {
                        string binaryPatModValue_orig = SharedFunctions.ReverseString(binaryPatModValue);
                        binaryPatModValue = binaryPatModValue_orig;

                        // Calculate the number of duplicates needed based on the size defined and the size needed.  EG if 20 bits and we're given 4, it needs to be repeated 5 times.  This happens if the label shows up multiple times.
                        var expected_size = plistHandle.GetExpectedDataSize() / (ulong)binaryPatModValue.Length;
                        Prime.Services.ConsoleService.PrintDebug($"Creating PatConfigHandle [{this.PatModConfigSetName + "_" + element.PatTarget}] with value [{binaryPatModValue}], expected size [{expected_size}]");

                        while (expected_size > 1)
                        {
                            binaryPatModValue = binaryPatModValue + binaryPatModValue_orig;
                            expected_size--;
                        }

                        Prime.Services.ConsoleService.PrintDebug($"POST-PADDED PatConfigHandle [{this.PatModConfigSetName + "_" + element.PatTarget}] with value [{binaryPatModValue}], expected size [{expected_size}]");
                        plistHandle.SetData(binaryPatModValue);
                    }

                    handles.Add(plistHandle);
                }

                return handles;
            }

            /// <summary>
            /// Check of a label matches against given PatModElement values.
            /// </summary>
            /// <param name="mbdAddress"> MBD address being checked for validity. </param>
            /// <param name="dwordElement"> <see cref="DwordElementContainer"/> containing pat modify info for raster. </param>
            /// <returns> A value indicating whether the given label passes the check against the given patModElements. </returns>
            public bool MatchAddressToDwordElement(Tuple<int, int, int> mbdAddress, out DwordElementContainer dwordElement)
            {
                dwordElement = null;

                foreach (var element in this.DwordElement)
                {
                    int multiportValue = element.GetMultiportValue();
                    int bankValue = element.GetBankValue();
                    int dwordValue = element.GetDwordValue();

                    bool addressMatched = mbdAddress.Item1 == multiportValue &&
                        mbdAddress.Item2 == bankValue &&
                        mbdAddress.Item3 == dwordValue;

                    if (addressMatched)
                    {
                        dwordElement = element;
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary> Class holding information on PatModifyElements. </summary>
        public class DwordElementContainer
        {
            /// <summary> Gets or sets list of PatModifys. </summary>
            public List<PatModifyElement> PatModifyElement { get; set; }

            /// <summary>
            /// Method for getting multiport value for this <see cref="DwordElementContainer"/>.
            /// </summary>
            /// <returns> Multiport value. </returns>
            internal int GetMultiportValue()
            {
                foreach (var element in this.PatModifyElement)
                {
                    var mbdValue = ConvertStringToPatTarget(element);

                    if (mbdValue == PatTargets.MULTIPORT)
                    {
                        return SharedFunctions.ExtractHexValue(element);
                    }
                }

                throw new Prime.Base.Exceptions.TestMethodException("Could not find multiport value for current DwordElement");
            }

            /// <summary>
            /// Method for getting bank value for this <see cref="DwordElementContainer"/>.
            /// </summary>
            /// <returns> Bank value. </returns>
            internal int GetBankValue()
            {
                foreach (var element in this.PatModifyElement)
                {
                    var mbdValue = ConvertStringToPatTarget(element);

                    if (mbdValue == PatTargets.BANK)
                    {
                        return SharedFunctions.ExtractHexValue(element);
                    }
                }

                throw new Prime.Base.Exceptions.TestMethodException("Could not find bank value for current DwordElement");
            }

            /// <summary>
            /// Method for getting dword value for this <see cref="DwordElementContainer"/>.
            /// </summary>
            /// <returns> Dword value. </returns>
            internal int GetDwordValue()
            {
                foreach (var element in this.PatModifyElement)
                {
                    var mbdValue = ConvertStringToPatTarget(element);

                    if (mbdValue == PatTargets.DWORD)
                    {
                        return SharedFunctions.ExtractHexValue(element);
                    }
                }

                throw new Prime.Base.Exceptions.TestMethodException("Could not find dword value for current DwordElement");
            }
        }

        /// <summary> Class holding information on PatModifys. </summary>
        public class PatModifyElement
        {
            /// <summary> Gets or sets PatTarget for this PatModify. </summary>
            public string PatTarget { get; set; }

            /// <summary> Gets or sets value for this PatModify. </summary>
            public string Value { get; set; }
        }
    }
}
