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

namespace MbistRasterRepairTC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using ARR_MBIST;
    using Prime.Base.Exceptions;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;

    /// <summary>
    /// MBIST Raster/Repair Test Template.
    /// </summary>
    [PrimeTestMethod]
    public class MbistRasterRepairTC : PrimeFunctionalTestMethod, IFunctionalExtensions
    {
        private ushort exitPort = 0;

        /// <summary>Simple enum so parameters can be True/False.</summary>
        public enum MyBoolean
        {
            /// <summary>True.</summary>
            TRUE,

            /// <summary>False.</summary>
            FALSE,
        }

        /// <summary>Gets or sets the name of the Raster/Repair JSON configuration file.</summary>
        public TestMethodsParams.String RasterInputFile { get; set; }

        /// <summary>Gets or sets EnableRepair flag.</summary>
        public MyBoolean EnableRepair { get; set; } = MyBoolean.FALSE;

        /// <summary>Gets or sets EnableFAFI flag.</summary>
        public MyBoolean EnableFAFI { get; set; } = MyBoolean.TRUE;

        private MbistRasterAlgorithm RasterAlgo { get; set; }

        private bool RepairMode { get; set; }

        private bool FAFIMode { get; set; }

        private int HryLength { get; set; }

        private List<string> CapturePinsList { get; set; }

        private MbistRasterInput.PList RasterPList { get; set; }

        private bool DebugMode { get; set; }

        /// <inheritdoc/>
        bool IFunctionalExtensions.ProcessCtvPerPin(Dictionary<string, string> ctvData)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            this.exitPort = 0;
            this.RasterAlgo.Initialize();  // clean up any data from previous runs
            Prime.Services.ConsoleService.PrintDebug($"Running ProcessCtvPerPin (RepairMode={this.RepairMode}).");

            // serialize the capture data
            var serializedData = Mbist.SerializeCaptureData(ctvData, this.CapturePinsList, this.RasterPList.CaptureInterLeaveMode);
            if (this.DebugMode && serializedData.Length < 10000)
            {
                // this is just too slow with very large capture amounts.
                Prime.Services.ConsoleService.PrintDebug($"SerializedCaptureData=[{serializedData}].");
            }

            if (serializedData.Length == 0)
            {
                throw new TestMethodException($"Capture data is empty for pins=[{string.Join(", ", this.CapturePinsList)}] in [{this.InstanceName}]");
            }

            // Run the raster/repair algorithm
            var captCnt = this.RasterAlgo.DecodeAllCaptures(this.RasterPList, serializedData);

            // FIXME error check captCnt...

            // write out the raw raster results
            Prime.Services.ConsoleService.PrintDebug($"Writing Data to Raster Logfile.");
            if (!this.RasterAlgo.WriteRasterLog(this.InstanceName))
            {
                Prime.Services.ConsoleService.PrintError("Failed to write to Raster Logfile.");
                this.exitPort = 0;
                return false;
            }

            var needsRepair = this.RasterAlgo.FailDatabase.RepairsNeeded();
            var repaired = false;
            Prime.Services.ConsoleService.PrintDebug($"Checking if Repair is required RepairMode=[{this.RepairMode}] DefectsFound=[{needsRepair}].");
            if (needsRepair && this.RepairMode)
            {
                Prime.Services.ConsoleService.PrintDebug($"Trying to do Repair.");
                repaired = this.RasterAlgo.PerformRepair();
            }

            // get the existing hry data
            string currentHRY = Mbist.GetMbistHRYData();
            if (currentHRY == string.Empty)
            {
                // FIXME - this should probably be an error.
                Prime.Services.ConsoleService.PrintDebug($"Current HRY is empty, intializing to {this.HryLength} U's.");
                currentHRY = new string('U', this.HryLength);
            }

            Prime.Services.ConsoleService.PrintDebug($"Current HRY HRY_RAWSTR_MBIST=[{currentHRY}].");

            // generate the final/merged HRY string
            var finalHRY = this.RasterAlgo.UpdateHRY(this.RasterPList, currentHRY);
            Prime.Services.ConsoleService.PrintDebug($"Updated HRY HRY_RAWSTR_MBIST=[{finalHRY}].");

            // save the new HRY
            Mbist.SetMbistHRYData(finalHRY);

            // Calculate the Exit port
            //   FIXME - probably put this in the RasterAlgorithm code
            if (this.RasterAlgo.MafFlag || this.RasterAlgo.ControllerStatusFailure)
            {
                this.exitPort = 3; // UNREPAIRABLE
            }
            else if (this.RasterAlgo.FailDatabase.Count == 0)
            {
                this.exitPort = 4; // NO_FAILURES
            }
            else if (this.RepairMode)
            {
                if (repaired)
                {
                    this.exitPort = 1; // REPAIRED
                }
                else
                {
                    this.exitPort = 3; // UNREPAIRABLE
                }
            }
            else if (this.RasterAlgo.FailDatabase.Count > 0)
            {
                this.exitPort = 2; // RASTERED
            }
            else
            {
                // unknown
                this.exitPort = 0;
            }

            if (Prime.Services.UserVarService.Exists("TestTimeLog", "PrimeUserCode"))
            {
                Prime.Services.UserVarService.SetValue("TestTimeLog", "PrimeUserCode", stopWatch.ElapsedMilliseconds);
            }

            return true;
        }

        /// <inheritdoc/>
        [Returns(4, PortType.Fail, "No Failures Detected")]
        [Returns(3, PortType.Fail, "Unrepairable")]
        [Returns(2, PortType.Fail, "Raster Completed")]
        [Returns(1, PortType.Pass, "Repaired")]
        [Returns(0, PortType.Fail, "FAIL PORT")]
        public override int Execute()
        {
            base.Execute();
            return this.exitPort;
        }

        /// <inheritdoc/>
        public override void CustomVerify()
        {
            Prime.Services.ConsoleService.PrintDebug("Getting Mode parameters");
            this.FAFIMode = this.EnableFAFI == MyBoolean.TRUE;
            this.RepairMode = this.EnableRepair == MyBoolean.TRUE;
            Prime.Services.ConsoleService.PrintDebug($"Param EnableRepair==[{this.EnableRepair}] Setting RepairMode to [{this.RepairMode}]");
            Prime.Services.ConsoleService.PrintDebug($"Param EnableFAFI==[{this.EnableFAFI}] Setting FAFIMode to [{this.FAFIMode}]");

            Prime.Services.ConsoleService.PrintDebug("Setting Debug Mode");
            this.DebugMode = this.LogLevel != PrimeLogLevel.DISABLED;

            if (this.FAFIMode && !this.DebugMode)
            {
                Prime.Services.ConsoleService.PrintDebug($"DebugMode is Disabled, disabling FAFIMode as well.");
                this.FAFIMode = false;
            }

            Prime.Services.ConsoleService.PrintDebug("Creating new Algorithm object");
            this.RasterAlgo = new MbistRasterAlgorithm(fafiMode: this.FAFIMode, repairMode: this.RepairMode);

            // ------ Load the Raster input file
            Prime.Services.ConsoleService.PrintDebug($"Loading JSON input file {this.RasterInputFile}");
            var rasterInputData = this.RasterAlgo.LoadInputFile(this.RasterInputFile);
            if (rasterInputData == null)
            {
                throw new FileLoadException($"Error, Could not load Raster/Repair JSON file [{this.RasterInputFile}] correctly.");
            }

            Prime.Services.ConsoleService.PrintDebug($"Getting Raster PList struct for {this.Patlist}");
            this.RasterPList = rasterInputData.GetPListStruct(this.Patlist);
            if (this.RasterPList == null)
            {
                throw new ArgumentException($"Error, PatList=[{this.Patlist}] is not found in RasterInput=[{this.RasterInputFile}]");
            }

            // fixme - get rid of this stuff.
            Prime.Services.ConsoleService.PrintDebug("Getting CapturePinsList");
            this.CapturePinsList = Mbist.ResolveCapturePins(this.CtvCapturePins.ToList(), this.RasterPList.CapturePins.Split(',').ToList());
            if (this.CapturePinsList.Count == 0)
            {
                throw new ArgumentException($"Mismatch between Raster/Repair CapturePins and CtvCapturePins parameter.");
            }

            Prime.Services.ConsoleService.PrintDebug("Getting HRY Length");
            this.HryLength = rasterInputData.HryLength;

            Prime.Services.ConsoleService.PrintDebug("Done with CustomVerify.");
        }
    }
}
