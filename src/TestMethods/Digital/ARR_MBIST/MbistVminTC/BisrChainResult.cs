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

// ---------------------------------------------------------------
// Created By Tim Kirkham
// ---------------------------------------------------------------
namespace MbistVminTC
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Prime;
    using Prime.ConsoleService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Compression BISR Return class.
    /// </summary>
    public class BisrChainResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BisrChainResult"/> class.</summary>
        /// <param name = "bisrControllerName" > Bisr Controller to be used for Shared Storage.</param>
        /// <param name = "rawData" > Bisr String Captured from tester.</param>
        /// <param name = "compressionStatus" > True faulse whether BISR chain compressed into fuse box.</param>
        /// <param name = "fusetoapply" > Value gotton from runnning software compression.  Value to be burned.</param>
        /// <param name = "fusetotal" > Fuse value total if allwoing multipass repair like foudnd in TSMC fuses.</param>
        /// <param name = "bisrcomments" > List of BISR comments used to clear unrepairable arrays.</param>
        /// <param name = "ituff" > ituff Name.</param>
        /// <param name = "dff" > Dff name.</param>
        /// <param name = "bisrss" > SharedStorage name.</param>
        /// <param name = "console">Prime.Services.ConsoleService or null depending on the current instances LogLevel.</param>
        /// <param name = "availableFuse" > Fuse available To burn.</param>
        public BisrChainResult(string bisrControllerName, string rawData, bool compressionStatus, string fusetoapply, string fusetotal, List<string> bisrcomments, string ituff, string dff, string bisrss, IConsoleService console, bool availableFuse = true)
        {
            this.Console = console;
            this.BisrControllerName = bisrControllerName;
            this.RawData = rawData;
            this.CompressionStatus = compressionStatus;
            this.FuseToApply = fusetoapply;
            this.FuseAfterBurn = fusetotal;
            this.BisrComments = bisrcomments;
            this.ITuffName = ituff;
            this.DffName = dff;
            this.BisrControllerSS = bisrss;
        }

        /// <summary> Gets or sets the bisr controller name.</summary>
        public string BisrControllerName { get; set; }

        /// <summary> Gets or sets the bisr controller name.</summary>
        public string BisrControllerSS { get; set; }

        /// <summary> Gets or sets the raw bisr chain.</summary>
        public string RawData { get; set; }

        /// <summary> Gets or sets a value indicating whether gets or sets whether there is a fuse available to burn.</summary>
        public bool AvailableFuse { get; set; } = true;

        /// <summary> Gets or sets a value indicating whether gets or sets whether compression fits in the fusebox.</summary>
        public bool CompressionStatus { get; set; } = false;

        /// <summary> Gets or sets a value indicating whether gets or sets the fuse to apply if you want the repair to happen.</summary>
        public string FuseToApply { get; set; }

        /// <summary> Gets or sets a value indicating whether gets or sets expected fuse after burn occurs.</summary>
        public string FuseAfterBurn { get; set; }

        /// <summary> Gets or sets a value indicating whether gets or sets expected fuse after burn occurs.</summary>
        public List<string> BisrComments { get; set; }

        /// <summary> Gets or sets name used if ITUFF is written.</summary>
        public string ITuffName { get; set; }

        /// <summary> Gets or sets a value indicating the Dff name to be printed.</summary>
        public string DffName { get; set; }

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        /// <summary> Print BISR info.</summary>
        public void PrintBisr()
        {
            this.Console?.PrintDebug($"\t\t[{MethodBase.GetCurrentMethod().Name}] BISRNAME: {this.BisrControllerName}]\n\t\t\tBisrChain: {this.RawData}\n\t\t\tFuseToApply: {this.FuseToApply}\n\t\t\tFuseAfterBurn: {this.FuseAfterBurn}\n");
        }

        /// <summary> Update BISR Chain with original BISR chain for controllers untested in the array.</summary>
        /// <param name = "currentBisr" > Current BISR applied patmod string.</param>
        /// <param name = "run_index" > Running indexs for controllers.</param>
        /// <param name = "reference" > Reference names for controllers.</param>
        public void AggregateBisrChainData(List<char> currentBisr, HashSet<int> run_index, List<string> reference)
        {
            HashSet<int> runindexs = new HashSet<int>();
            foreach (var location in run_index)
            {
                var index = 0;
                foreach (var bisrname in this.BisrComments)
                {
                    if (Regex.IsMatch(bisrname, reference[location]))
                    {
                        this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Moving Data from Memory {reference[location]} to the original BISR Chain");
                        runindexs.Add(index);
                    }

                    index += 1;
                }
            }

            var updatedBisr = this.RawData.ToList();
            foreach (var location in runindexs)
            {
                updatedBisr[location] = currentBisr[location];
            }

            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Removing failing Memories to allow BISR compress to be optimized");
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Orig    BISR: {this.RawData}");
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Updated BISR: {string.Join(string.Empty, updatedBisr)}");

            this.RawData = string.Join(string.Empty, updatedBisr);
        }

        /// <summary> Writes sharedstorage for this bisr.</summary>
        /// <param name = "mode" > Bisr Controller Name.</param>
        public void WriteSharedStorage(MbistVminTC.BisrModes mode)
        {
            Services.SharedStorageService.InsertRowAtTable($"{this.BisrControllerSS}_RAW", this.RawData, Context.DUT);
            if (mode == MbistVminTC.BisrModes.Compressed || mode == MbistVminTC.BisrModes.Compressed_skippatmod)
            {
                Services.SharedStorageService.InsertRowAtTable($"{this.BisrControllerSS}_FUSE", this.FuseToApply, Context.DUT);
            }
        }

        /// <summary> Write DFF.</summary>
        /// <param name = "mode" > Bisr Controller Name.</param>
        public void WriteDff(MbistVminTC.BisrModes mode)
        {
            if (mode == MbistVminTC.BisrModes.Bisr || mode == MbistVminTC.BisrModes.Bisr_skippatmod)
            {
                Services.DffService.SetDff(this.DffName, this.RawData);
            }
            else
            {
                Services.DffService.SetDff(this.DffName, this.FuseToApply);
            }
        }

        /// <summary> GetBisrData.</summary>
        /// <param name = "mode" > Bisr Controller Name.</param>
        /// <param name = "dff" > Whether to read from DFF or Shared Storage.</param>
        public void ReadData(MbistVminTC.BisrModes mode, bool dff = false)
        {
            if (dff == true)
            {
                this.BisrReadDff(mode);
            }
            else
            {
                this.BisrReadSharedStorage(mode);
            }
        }

        /// <summary> WriteBisrData.</summary>
        /// <param name = "mode" > Bisr Controller Name.</param>
        /// <param name="dff"> Whether to read from DFF or Shared Storage.</param>
        public void WriteData(MbistVminTC.BisrModes mode, bool dff = false)
        {
            this.WriteSharedStorage(mode);
            if (dff == true)
            {
                this.WriteDff(mode);
            }
        }

        /// <summary> ReadDFF.</summary>
        /// <param name = "mode" > Bisr Controller Name.</param>
        public void BisrReadDff(MbistVminTC.BisrModes mode)
        {
            if (mode == MbistVminTC.BisrModes.Bisr || mode == MbistVminTC.BisrModes.Bisr_skippatmod)
            {
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Retrieving from shared for key : [{this.BisrControllerName}].");
                this.RawData = (string)Services.DffService.GetDff(this.DffName);
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Retrieving value from shared storage as BISR: [{this.RawData}].");
            }
            else
            {
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Retrieving from shared for key : [{this.BisrControllerName}].");
                this.FuseToApply = (string)Services.DffService.GetDff(this.DffName);
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Retrieving value from shared storage as BISR: [{this.RawData}].");
            }
        }

        /// <summary> ReadShared Storage for BISR.</summary>
        /// <param name = "mode" > BISR Controller Name.</param>
        public void BisrReadSharedStorage(MbistVminTC.BisrModes mode)
        {
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Retrieving from shared for key : [{this.BisrControllerSS}_RAW].");
            this.RawData = (string)Prime.Services.SharedStorageService.GetStringRowFromTable($"{this.BisrControllerSS}_RAW", Context.DUT);
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Retrieving value from shared storage as BISR: [{this.RawData}].");
            if (mode == MbistVminTC.BisrModes.Compressed || mode == MbistVminTC.BisrModes.Compressed_skippatmod)
            {
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Retrieving from shared for key : [{this.BisrControllerSS}_FUSE].");
                this.FuseToApply = (string)Prime.Services.SharedStorageService.GetStringRowFromTable($"{this.BisrControllerSS}_FUSE", Context.DUT);
                this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Retrieving value from shared storage as Compressed fuse: [{this.FuseToApply}].");
            }
        }

        /// <summary> Returns value to be applied to reset.</summary>
        /// <param name = "mode" > Bisr Controller Name.</param>
        /// <returns>string to apply to patmod locaiton.</returns>
        public string Fusepatmod(MbistVminTC.BisrModes mode)
        {
            if (mode == MbistVminTC.BisrModes.Bisr || mode == MbistVminTC.BisrModes.Bisr_skippatmod)
            {
                return this.RawData;
            }
            else
            {
                return this.FuseToApply;
            }
        }

        /// <summary>Return initialization String for BISR.</summary>
        /// <param name = "mode" > Mode to return the proper initilization value.</param>
        /// <param name = "totalbisrchain" > Length of BISR chain.</param>
        /// <param name = "totalfusebox" > Length of Fusebox.</param>
        public void InitializeBisrString(MbistVminTC.BisrModes mode, int totalbisrchain, int totalfusebox)
        {
            this.CompressionStatus = false;
            this.FuseAfterBurn = string.Empty;

            this.RawData = new string('0', totalbisrchain);
            this.FuseToApply = new string('0', totalfusebox);
            this.Console?.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Initializing bisr name [{this.BisrControllerName}] to : [{this.RawData}]");
        }
    }
}
