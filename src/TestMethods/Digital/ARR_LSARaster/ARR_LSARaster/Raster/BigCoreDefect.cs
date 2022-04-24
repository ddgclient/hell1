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
    public class BigCoreDefect : IDefect, IEquatable<BigCoreDefect>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BigCoreDefect"/> class.
        /// </summary>
        /// <param name="failAddress"> Value of failaddress. </param>
        /// <param name="dword"> Value of dword. </param>
        /// <param name="bank"> Value of bank. </param>
        /// <param name="failIo"> Value of failIo. </param>
        public BigCoreDefect(int dword, int bank, string failAddress, string failIo)
        {
            this.Dword = dword;
            this.Bank = bank;
            this.FailAddress = failAddress;
            this.FailIO = failIo;
        }

        /// <summary> Gets or sets array name for this defect. </summary>
        public string Array { get; set; } = string.Empty;

        /// <summary> Gets or sets slice (or module) for this defect. </summary>
        public string Slice { get; set; } = string.Empty;

        /// <summary> Gets or sets dword value for this defect. </summary>
        public int Dword { get; set; }

        /// <summary> Gets or sets bank value for this defect. </summary>
        public int Bank { get; set; }

        /// <summary> Gets or sets fail address value for this defect. </summary>
        public string FailAddress { get; set; }

        /// <summary> Gets or sets failIo value for this defect. </summary>
        public string FailIO { get; set; }

        /// <inheritdoc/>
        public bool SendToRepair { get; set; } = true;

        /// <summary>
        /// Method for instatiating instances of this class <see cref="BigCoreDefect"/>.
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
            List<IDefect> defects = new List<IDefect>();
            List<string> sliceIds = pinMappingSet.GetSliceIdFromPin(currentPin, out _);

            // No ctvData to parse; this is a nonValid defect
            if (ctvData.Count == 0)
            {
                Prime.Services.ConsoleService.PrintDebug("No ctvData to parse for this location. Submitting a Non-Valid Defect.");
                int dword = dwordElement.GetDwordValue();
                int bank = dwordElement.GetBankValue();

                BigCoreDefect invalidDefect = new BigCoreDefect(dword, bank, "000000000000", "00000000000000000000000000000000");
                invalidDefect.Array = currentArray;
                invalidDefect.SendToRepair = false;

                foreach (var slice in sliceIds)
                {
                    invalidDefect.Slice = slice;
                    defects.Add(invalidDefect.Clone());
                }

                return defects;
            }

            foreach (string chunk in ctvData)
            {
                Prime.Services.ConsoleService.PrintDebug($"Decoding section of ctvData: [{chunk}]");

                int dword = -1;
                int bank = -1;
                string failAddress = null;
                string failIO = null;

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
                    else if (currentDecodingElement == "FAILIO")
                    {
                        failIO = elementBits;
                        Prime.Services.ConsoleService.PrintDebug($"Decoded: [0x{DDG.RadixConversion.BinaryToHex(failIO)}]\n");
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

                BigCoreDefect newDefect = new BigCoreDefect(dword, bank, failAddress, failIO);
                newDefect.Array = currentArray;
                Prime.Services.ConsoleService.PrintDebug($"Decoded values for ctvData: Dword={dword}, Bank={bank}, FailAddress={DDG.RadixConversion.BinaryToHex(failAddress)}, FailIO={DDG.RadixConversion.BinaryToHex(failIO)}");

                foreach (var slice in sliceIds)
                {
                    newDefect.Slice = slice;
                    defects.Add(newDefect.Clone());
                }
            }

            return defects;
        }

        /// <summary>
        /// Adds defect to DB.
        /// </summary>
        /// <param name="database"> DB to add to. </param>
        public void AddToInternalDatabase(ref Dictionary<string, List<IDefect>> database)
        {
            string key = $"{this.Array}";

            if (!database.ContainsKey(key))
            {
                database.Add(key, new List<IDefect>());
            }

            database[key].Add(this);
        }

        /// <summary>
        /// Using the info provided in this instance of <see cref="BigCoreDefect"/>, create a string for the T-File...
        /// </summary>
        /// <returns> A string to be used in the T-File. </returns>
        public string CreateTFileString()
        {
            string nonValidPrefix = this.SendToRepair ? string.Empty : $"#NonValid,";
            Prime.Services.ConsoleService.PrintDebug($"{nonValidPrefix}Array: {this.Array}\nSlice: {this.Slice}\n{this.Dword},{this.Bank},{"0x" + DDG.RadixConversion.BinaryToHex(this.FailAddress)},{"0x" + DDG.RadixConversion.BinaryToHex(this.FailIO)}\n");
            return $"{nonValidPrefix}Array: {this.Array}\nSlice: {this.Slice}\n{this.Dword},{this.Bank},{"0x" + DDG.RadixConversion.BinaryToHex(this.FailAddress)},{"0x" + DDG.RadixConversion.BinaryToHex(this.FailIO)}\n";
        }

        /// <inheritdoc/>
        public string CreateTFileHeaderBlock() // FIXME: Is this still needed? Was removed from main logic
        {
            return $"Array: {this.Array}\n";
        }

        /// <summary>
        /// Check if two defect objects have equivalent member variable values.
        /// </summary>
        /// <param name="defect"> Defect object to compare against. </param>
        /// <returns> A value indicating if the two objects are equal. </returns>
        public bool Equals(BigCoreDefect defect)
        {
            return this.Array == defect.Array &&
            this.Slice == defect.Slice &&
            this.Dword == defect.Dword &&
            this.Bank == defect.Bank &&
            this.FailAddress == defect.FailAddress &&
            this.FailIO == defect.FailIO;
        }

        /// <summary>
        /// Method for cloning this object.
        /// </summary>
        /// <returns> Copy of this object. </returns>
        public BigCoreDefect Clone()
        {
            BigCoreDefect newDefect = new BigCoreDefect(this.Dword, this.Bank, this.FailAddress, this.FailIO);
            newDefect.Array = this.Array;
            newDefect.Slice = this.Slice;
            newDefect.SendToRepair = this.SendToRepair;
            return newDefect;
        }

        /// <inheritdoc/>
        public string CreateRepairString()
        {
            StringBuilder repairBuilder = new StringBuilder();
            List<int> indexes = SharedFunctions.ExtractOnesIndexesFromFailIo(this.FailIO);
            int failAddressAsInt = DDG.RadixConversion.BinaryToInteger(this.FailAddress);

            foreach (var index in indexes)
            {
                string temp = $"{this.Array},{this.Slice},{this.Dword},{this.Bank},{failAddressAsInt},{index};";
                if (temp != ";")
                {
                    Prime.Services.ConsoleService.PrintDebug($"Raster Appending {temp}\n");
                    repairBuilder.Append(temp);
                }
            }

            if (repairBuilder.ToString().EndsWith(";"))
            {
                repairBuilder.Remove(repairBuilder.Length - 1, 1); // remove last delimiter character
            }

            return repairBuilder.ToString();
        }
    }
}
