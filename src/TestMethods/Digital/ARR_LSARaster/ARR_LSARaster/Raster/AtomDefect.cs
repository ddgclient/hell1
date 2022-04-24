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
    using System.Text;

    /// <summary>
    /// Class to represent a defect decoded Ctvs captured in Raster.
    /// </summary>
    public class AtomDefect : IDefect, IEquatable<AtomDefect>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomDefect"/> class.
        /// </summary>
        /// <param name="failAddress"> Value of failaddress. </param>
        /// <param name="dword"> Value of dword. </param>
        /// <param name="bank"> Value of bank. </param>
        /// <param name="multiFailIo"> Value of all FailIos.</param>
        public AtomDefect(int dword, int bank, string failAddress, Dictionary<string, string> multiFailIo)
        {
            this.Dword = dword;
            this.Bank = bank;
            this.FailAddress = failAddress;
            this.CoreToFailIO = multiFailIo;
        }

        /// <summary> Gets or sets array name for this defect. </summary>
        public string Array { get; set; }

        /// <summary> Gets or sets slice (or module) for this defect. </summary>
        public string Module { get; set; }

        /// <summary> Gets or sets dword value for this defect. </summary>
        public int Dword { get; set; }

        /// <summary> Gets or sets bank value for this defect. </summary>
        public int Bank { get; set; }

        /// <summary> Gets or sets fail address value for this defect. </summary>
        public string FailAddress { get; set; }

        /// <summary> Gets or sets dict containing multiple failIO; a map of Core -> its failIo. If we're in Module mode, there will be a failIO per core for each module. </summary>
        public Dictionary<string, string> CoreToFailIO { get; set; }

        /// <inheritdoc/>
        public bool SendToRepair { get; set; } = true;

        /// <summary>
        /// Method for instatiating instances of this class <see cref="AtomDefect"/>.
        /// </summary>
        /// <param name="ctvData"> CtvData to parse.</param>
        /// <param name="captureSet"> CaptureSet used to decode ctvData. </param>
        /// <param name="pinMappingSet"> PinMappingSet used for this execution of LSARaster. </param>
        /// <param name="currentPin"> Pin that the ctvData is coming from. </param>
        /// <param name="currentArray"> Array that this ctvData is coming from. </param>
        /// <param name="dwordElement"> Dword element used to collect ctvData. </param>
        /// <returns> List of <see cref="IDefect"/> objects. </returns>
        public static List<IDefect> CreateDefects(List<string> ctvData, MetadataConfig.CaptureSet captureSet, MetadataConfig.PinMappingSet pinMappingSet, string currentPin, string currentArray, RasterConfig.DwordElementContainer dwordElement)
        {
            string module = pinMappingSet.GetModuleFromPin(currentPin);
            List<IDefect> defects = new List<IDefect>();

            // No ctvData to parse; this is a nonValid defect
            if (ctvData.Count == 0)
            {
                Prime.Services.ConsoleService.PrintDebug("No ctvData to parse for this location. Submitting a Non-Valid Defect.");
                int dword = dwordElement.GetDwordValue();
                int bank = dwordElement.GetBankValue();

                AtomDefect invalidDefect = new AtomDefect(dword, bank, "000", new Dictionary<string, string>() { { "#NonValid_0", "00000001" } });
                invalidDefect.Module = pinMappingSet.GetModuleFromPin(currentPin);
                invalidDefect.Array = currentArray;
                invalidDefect.SendToRepair = false;

                defects.Add(invalidDefect);
                return defects;
            }

            foreach (string chunk in ctvData)
            {
                Prime.Services.ConsoleService.PrintDebug($"Decoding section of ctvData: [{chunk}]");
                int dword = -1;
                int bank = -1;
                string failAddress = null;
                Dictionary<string, string> multiFailIo = new Dictionary<string, string>();

                foreach (KeyValuePair<string, MetadataConfig.DecodingElement> decodeInfo in captureSet.DecodingElements)
                {
                    string currentDecodingElement = decodeInfo.Key.ToUpper();
                    int start = decodeInfo.Value.Start;
                    int end = decodeInfo.Value.End;
                    int length = Math.Abs(start - end) + 1;
                    string elementBits = chunk.Substring(Math.Min(start, end), length);

                    if (start < end)
                    {
                        elementBits = SharedFunctions.ReverseString(elementBits);
                    }

                    Prime.Services.ConsoleService.PrintDebug($"Decoding Element: [{currentDecodingElement}]\nStartParam is: [{start}]\nEndParam is: [{end}]\nLength is: [{length}]\nValue to convert: [{elementBits}]");

                    if (currentDecodingElement == "FAILADDRESS")
                    {
                        failAddress = elementBits;
                        Prime.Services.ConsoleService.PrintDebug($"Decoded: [0x{DDG.RadixConversion.BinaryToHex(failAddress)}]\n");
                    }
                    else if (currentDecodingElement.Contains("FAILIO"))
                    {
                        string[] delimitedIO = currentDecodingElement.Split('_');

                        if (delimitedIO.Length != 2)
                        {
                            throw new Prime.Base.Exceptions.TestMethodException($"Currently in ATOM mode, could not determing which core this FAILIO belongs to.\nDecoding Element: [{currentDecodingElement}]");
                        }

                        multiFailIo.Add(delimitedIO[1], elementBits);
                    }
                    else if (currentDecodingElement == "BANK")
                    {
                        bank = DDG.RadixConversion.BinaryToInteger(elementBits);
                        Prime.Services.ConsoleService.PrintDebug($"Decoded: [{bank}]\n");
                    }
                    else if (currentDecodingElement == "DWORD")
                    {
                        dword = DDG.RadixConversion.BinaryToInteger(elementBits);
                        Prime.Services.ConsoleService.PrintDebug($"Decoded: [{dword}]\n");
                    }
                    else
                    {
                        throw new Prime.Base.Exceptions.TestMethodException($"The template does not recognize this decoding element: [{currentDecodingElement}]");
                    }
                }

                StringBuilder infoBuilder = new StringBuilder();
                infoBuilder.Append($"Dword={dword}, Bank={bank}, FailAddress=0x{DDG.RadixConversion.BinaryToHex(failAddress)}, FailIO:\n");
                foreach (var failIo in multiFailIo)
                {
                    infoBuilder.Append($"Core_{failIo.Key}=0x{DDG.RadixConversion.BinaryToHex(failIo.Value)} ");
                }

                Prime.Services.ConsoleService.PrintDebug($"Decoded values for ctvData: {infoBuilder}");

                AtomDefect newDefect = new AtomDefect(dword, bank, failAddress, multiFailIo);
                newDefect.Module = module;
                newDefect.Array = currentArray;
                defects.Add(newDefect);
            }

            return defects;
        }

        /// <inheritdoc/>
        public void AddToInternalDatabase(ref Dictionary<string, List<IDefect>> database)
        {
            string key = $"{this.Array}_{this.Module}";

            if (!database.ContainsKey(key))
            {
                database.Add(key, new List<IDefect>());
            }

            database[key].Add(this);
        }

        /// <inheritdoc/>
        public string CreateTFileHeaderBlock()
        {
            return $"Array: {this.Array}\nModule: {this.Module}\n";
        }

        /// <summary>
        /// Using the info provided in this instance of <see cref="AtomDefect"/>, create a string for the T-File...
        /// </summary>
        /// <returns> A string to be used in the T-File. </returns>
        public string CreateTFileString()
        {
            StringBuilder tFileBuilder = new StringBuilder();
            tFileBuilder.Append($"Array: {this.Array}\nModule: {this.Module}\n");

            foreach (var coreNumToIO in this.CoreToFailIO)
            {
                List<int> indexes = SharedFunctions.ExtractOnesIndexesFromFailIo(coreNumToIO.Value);

                if (indexes.Count > 0)
                {
                    tFileBuilder.Append($"{coreNumToIO.Key},{this.Dword},{this.Bank},{"0x" + DDG.RadixConversion.BinaryToHex(this.FailAddress)},{"0x" + DDG.RadixConversion.BinaryToHex(coreNumToIO.Value)}\n");
                }
            }

            // If the string doesn't contain any actual defect, return Empty string tto make sure Array and Slice aren't spammed.
            if (tFileBuilder.ToString().Contains(","))
            {
                return tFileBuilder.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Check if two defect objects have equivalent member variable values.
        /// </summary>
        /// <param name="defect"> Defect object to compare against. </param>
        /// <returns> A value indicating if the two objects are equal. </returns>
        public bool Equals(AtomDefect defect)
        {
            return this.Array == defect.Array &&
            this.Module == defect.Module &&
            this.Dword == defect.Dword &&
            this.Bank == defect.Bank &&
            this.FailAddress == defect.FailAddress;
        }

        /// <inheritdoc/>
        public string CreateRepairString()
        {
            StringBuilder repairBuilder = new StringBuilder();
            int failAddressAsInt = DDG.RadixConversion.BinaryToInteger(this.FailAddress);
            int numOfDefects = 0;

            foreach (var coreToFailIo in this.CoreToFailIO)
            {
                List<int> indexes = SharedFunctions.ExtractOnesIndexesFromFailIo(coreToFailIo.Value);
                numOfDefects += indexes.Count;

                foreach (var index in indexes)
                {
                    repairBuilder.Append($"{this.Array},{this.Module},{coreToFailIo.Key},{this.Dword},{this.Bank},{failAddressAsInt},{index};");
                }
            }

            if (numOfDefects > 0)
            {
                repairBuilder.Remove(repairBuilder.Length - 1, 1); // remove last delimiter character
            }

            return repairBuilder.ToString();
        }
    }
}
