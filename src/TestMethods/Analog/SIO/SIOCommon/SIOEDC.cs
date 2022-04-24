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

namespace SIO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Main SIO EDC class.
    /// </summary>
    public class SIOEDC
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SIOEDC"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="instance">Test Instance name.</param>
        /// <param name="debug">Whether debug mode is enabled or not.</param>
        public SIOEDC(string instance, bool debug)
        {
            this.SetupComplete = false;

            this.TestInstanceName = instance;
            this.DebugMode = debug;

            this.SioLib = new SIOLib(debug);
            this.SioEdcUtils = new SIOEDC_Util(debug);
        }

        /// <summary>Gets or Sets the SIOLib object.</summary>
        public SIOLib SioLib { get; set; }

        /// <summary>Gets or sets the UserFileData.</summary>
        public UserFile UserFileData { get; set; }

        // Private data for the class
        private string TestInstanceName { get; set; }

        private bool DebugMode { get; set; }

        private SIOEDC_Util SioEdcUtils { get; set; }

        // Data for the EDCLog functions
        private SIOEDC_Util.SIOFormatFile FormatFileData { get; set; }

        private Dictionary<string, List<SIOEDC_Util.SIOSequence>> SequenceFileData { get; set; }

        private string UserToken { get; set; } = string.Empty;

        private bool SetupComplete { get; set; }

        // Data for the EDCMain functions
        private bool EDCShmooEnabled { get; set; }

        private bool EDCLogEnabled { get; set; }

        /// <summary>
        /// Perform one-time setup and checks for the EDCLog functionality.
        /// Meant to be called from the templates "Verify" function.
        /// </summary>
        /// <param name="userFile">Name of the User File to parse.</param>
        /// <param name="token">User File Token to use for this test.</param>
        /// <param name="allowMissingFiles">If false (the default) will error out if the format/sequence file is not found or invalid.</param>
        public void SetupEDCLog(string userFile, string token, bool allowMissingFiles = false)
        {
            this.UserToken = token;

            // Load the User File.
            this.UserFileData = new UserFile(userFile);
            if (!this.UserFileData.Valid)
            {
                throw new Exception($"SIOEDC.SetupEDCLog failed to construct UserFileData.");
            }

            // Get the User data from the User File + Token provided.
            if (!this.UserFileData.TokenBlocks.ContainsKey(this.UserToken))
            {
                throw new Exception($"SIOEDC.Setup failed, token=[{this.UserToken}] does not exist in UserFileData.");
            }

            var userData = this.UserFileData.TokenBlocks[this.UserToken];

            // Load the format file specified from the FORMAT_FILE User Data
            this.FormatFileData = this.SioEdcUtils.LoadFormatFile(userData.FormatFile);
            if (!this.FormatFileData.valid && !allowMissingFiles)
            {
                throw new Exception($"SIOEDC.Setup failed to construct FormatFileData.");
            }

            // Load the sequence file specified from the SEQ_FILE User Data
            this.SequenceFileData = this.SioEdcUtils.LoadSequenceFile(userData.SeqFile);
            if (this.SequenceFileData.Count == 0)
            {
                if (allowMissingFiles)
                {
                    this.SequenceFileData[userData.SeqId] = new List<SIOEDC_Util.SIOSequence>();
                }
                else
                {
                    throw new Exception($"SIOEDC.Setup failed to construct SequenceFileData.");
                }
            }

            if (!this.SequenceFileData.ContainsKey(userData.SeqId))
            {
                throw new Exception($"SIOEDC.Setup failed, seqid=[{userData.SeqId}] does not exist in SequenceFileData.");
            }

            this.SetupComplete = true;
        }

        /// <summary>
        /// Main function for executing EDCLog.
        /// </summary>
        /// <param name="ctvData">Dictionary with Key=Pin Value=CTV Data.</param>
        /// <param name="plist">Name of the PatternList that was executed.</param>
        /// <param name="pin">Name of the Pin to use as a key to ctvData.</param>
        /// <returns>Retuns the exit port.</returns>
        public ushort RunEDCLog(Dictionary<string, string> ctvData, string plist, string pin)
        {
            // make sure setup has been run.
            if (!this.SetupComplete)
            {
                this.SioLib.MsgToConsole(MsgEnum.SIO_ERROR, $"Error in RunEDCLog ... SetupEDCLog has not been run.");
                throw new ArgumentException("Error in RunEDCLog ... SetupEDCLog has not been run");
            }

            if (!ctvData.ContainsKey(pin))
            {
                this.SioLib.MsgToConsole(MsgEnum.SIO_ERROR, $"CTV data does not contain data for pin=[{pin}]");
                throw new ArgumentException($"CTV data does not contain data for pin=[{pin}]");
            }

            // Convert the binary data to Base32 and compress it.
            SIOLib.CompressedData data = this.SioLib.BinToBase32(ctvData[pin]);
            if (string.IsNullOrWhiteSpace(data.UncompressedString) || string.IsNullOrWhiteSpace(data.CompressedString))
            {
                // this.SioLib.MsgToConsole(MsgEnum.SIO_ERROR, $"Failed to compress ctv data=[{ctvData[pin]}]");
                // throw new ArgumentException($"Failed to compress ctv data=[{ctvData[pin]}]");
                this.SioLib.MsgToConsole(MsgEnum.SIO_ERROR, $"Failed to compress ctv data");
                throw new ArgumentException($"Failed to compress ctv data");
            }

            this.SioLib.MsgToConsole(MsgEnum.SIO_DEBUG, $"sBinDmemData={ctvData[pin].Length} Bits long");
            this.SioLib.MsgToConsole(MsgEnum.SIO_DEBUG, $"sBase32DmemData={data.CompressedString}");

            // Log the compressed data and translation key to ituff.
            var userData = this.UserFileData.TokenBlocks[this.UserToken];  // checked in setup
            var testName = this.TestInstanceName;
            if (!string.IsNullOrWhiteSpace(userData.DlogName))
            {
                testName = userData.DlogName;
            }

            var dlogTestName = $"{testName}_EDC_CAPTURE_L0_P";
            var dlogTestData = $"TOKEN=RUN:0!Plist={plist}!RUN=1!CmpName=NA!TestType=CAPTURE!DataStart#{data.CompressedString}#DataEnd!KEY={data.TranslationTable}";
            if (!this.SioLib.ResultStrgValToDatalog(dlogTestName, dlogTestData))
            {
                this.SioLib.MsgToConsole(MsgEnum.SIO_ERROR, $"Datalogging failed");
                return 0;
            }

            // If requested, parse the data based on the format/sequence files to determine
            // pass/fail and print out the data.
            //  FIXME - this is a bit if a mess.
            if (userData.LogType.ToUpper() != "CAPTURE" || this.DebugMode)
            {
                // pull the data for each field from the raw capture data.
                var seqList = this.SequenceFileData[userData.SeqId];  // checked in setup
                SIOEDC_Util.HashedData dataHash = this.SioEdcUtils.HashBitStream(seqList, ctvData[pin]);

                if (dataHash == null || dataHash.Count() == 0)
                {
                    this.SioLib.MsgToConsole(MsgEnum.SIO_ERROR, $"Failed to assign data from raw ctv to struct.");
                    return 0;
                }

                // build the output, this also sets the pass/fail results.
                List<string> outputList;
                int localExit = 0;
                bool outputOk = this.SioEdcUtils.GenerateOutput(this.FormatFileData, dataHash, userData.RegDef, out outputList, out localExit);

                if (!outputOk)
                {
                    this.SioLib.MsgToConsole(MsgEnum.SIO_ERROR, $"Failed to generate output correctly.");
                    return 0;
                }
                else
                {
                    if (this.DebugMode)
                    {
                        // this should only display in OBNOXIOUS mode...not sure how to do that in prime
                        this.SioEdcUtils.DisplaySequenceData(seqList, userData.SeqId, dataHash);
                    }

                    if (this.DebugMode)
                    {
                        this.SioEdcUtils.DisplayOutput(this.FormatFileData.header, outputList);
                    }
                }

                // FIXME. should do some checking on this cast.
                return (ushort)localExit;
            }
            else
            {
                // capture only mode
                return 1;
            }
        }

        /// <summary>
        /// Perform one-time setup and checks for the EDCMain functionality.
        /// Meant to be called from the templates "Verify" function.
        /// </summary>
        /// <param name="userFile">Name of the User File to parse.</param>
        /// <param name="token">User File Token to use for this test.</param>
        /// <param name="shmooEnabled">Indicates if Shmoo is enabled.</param>
        /// <param name="logEnabled">Indicates if Logging is enabled.</param>
        /// <returns>True on success, False if any failures.</returns>
        public bool SetupEDCMain(string userFile, string token, bool shmooEnabled, bool logEnabled)
        {
            this.EDCShmooEnabled = shmooEnabled;
            this.EDCLogEnabled = logEnabled;

            this.SetupEDCLog(userFile, token, allowMissingFiles: true);
            if (!this.SioLib.ShmooTestSetup(this.UserFileData, this.UserToken))
            {
                return false;
            }

            this.UserFileData.TokenBlocks[this.UserToken].TestInstanceIsShmoo = false; // EDCMain doesn't support this functionality.
            this.UserFileData.TokenBlocks[this.UserToken].EDCShmooEnabled = this.EDCShmooEnabled;

            // update the callback function for the shmoo to use the EDC function.
            this.UserFileData.TokenBlocks[this.UserToken].ShmooSingleTestPointFunc = this.RunShmooSinglePointEDC;

            return true;
        }

        /// <summary>
        /// Main function for executing EDCMain.
        /// </summary>
        /// <returns>Retuns the number of shmoo errors.</returns>
        public int RunEDCMain()
        {
            var userData = this.UserFileData.TokenBlocks[this.UserToken];
            var numErrors = this.SioLib.RunShmoo(userData);
            return numErrors;
        }

        /// <summary>
        /// Runs each point in a shmoo.
        /// </summary>
        /// <param name="userData">UserFile.UserData struct with the shmoo.</param>
        /// <param name="currentState">Dictionary with the current state of the shmoo.</param>
        public void RunShmooSinglePointEDC(UserFile.UserData userData, Dictionary<string, string> currentState)
        {
            this.SioLib.MsgToConsole(MsgEnum.SIO_DEBUG, $"RunShmooSinglePointEDC: CurrentState[RUN]=[{currentState["RUN"]}] Executing Test=[{userData.ExecuteTest}]");
            var testRslt = Prime.Services.TestProgramService.ExecuteTestInstance(userData.ExecuteTest);
            if (testRslt < 0)
            {
                testRslt = 0;
                this.SioLib.ShmooSetupErros++;
            }

            // Read the capture data from GSDS token.
            var captureData = Prime.Services.SharedStorageService.GetStringRowFromTable(SIOLib.SIOGSDSBINDMEMDATA, Prime.SharedStorageService.Context.DUT);
            if (captureData.Length == 0)
            {
                this.SioLib.MsgToConsole(MsgEnum.SIO_ERROR, $"EDC: Captured 0 bits during testpoint=[{string.Join(", ", currentState.Select(i => $"{i.Key}: {i.Value}"))}].");
            }

            var compressData = this.SioLib.BinToBase32(captureData);
            this.SioLib.MsgToConsole(MsgEnum.SIO_DEBUG, $"EDC: nBinDmemCount = {captureData.Length}");
            this.SioLib.MsgToConsole(MsgEnum.SIO_DEBUG, $"EDC: sBinDmemData = {captureData}");
            this.SioLib.MsgToConsole(MsgEnum.SIO_DEBUG, $"EDC: sBase32DmemData = {compressData.CompressedString}");

            // this matches EmbPython but no idea why its used - FIXME
            if (currentState.ContainsKey("RUN") && currentState["RUN"] != string.Empty)
            {
                currentState["RUN"] = (int.Parse(currentState["RUN"]) + 1).ToString();
            }

            var dlogTestName = $"{userData.DlogName}_EDC_{userData.TestType}_L{userData.PreEDCSetupIteration++}_P";
            var dlogTestData = $"TOKEN={this.SioLib.ShmooStateToToken(currentState, false)}!Plist={userData.Plist}!RUN={userData.NumberOfRuns}!CmpName=NA!TestType=CAPTURE!DataStart#{compressData.CompressedString}#DataEnd!KEY={compressData.TranslationTable}";

            if (userData.EDCLogEnabled)
            {
                if (!this.SioLib.ResultStrgValToDatalog(dlogTestName, dlogTestData))
                {
                    this.SioLib.MsgToConsole(MsgEnum.SIO_ERROR, $"EDC: Failed to datalog.");
                }
            }
            else
            {
                this.SioLib.MsgToConsole(MsgEnum.SIO_DEBUG, dlogTestName);
                this.SioLib.MsgToConsole(MsgEnum.SIO_DEBUG, dlogTestData);
            }

            if (userData.LogType.ToUpper() != "CAPTURE" || this.DebugMode)
            {
                // pull the data for each field from the raw capture data.
                var seqList = this.SequenceFileData[userData.SeqId];  // checked in setup
                SIOEDC_Util.HashedData dataHash = this.SioEdcUtils.HashBitStream(seqList, captureData);

                if (dataHash == null || dataHash.Count() == 0)
                {
                    this.SioLib.MsgToConsole(MsgEnum.SIO_ERROR, $"Failed to assign data from raw ctv to struct.");
                }
                else
                {
                    List<string> outputList;
                    int localExit = 0;
                    bool outputOk = this.SioEdcUtils.GenerateOutput(this.FormatFileData, dataHash, userData.RegDef, out outputList, out localExit);
                    if (!outputOk)
                    {
                        this.SioLib.MsgToConsole(MsgEnum.SIO_ERROR, $"Failed to generate output correctly.");
                    }
                    else if (this.DebugMode)
                    {
                        this.SioEdcUtils.DisplaySequenceData(seqList, userData.SeqId, dataHash);
                        this.SioEdcUtils.DisplayOutput(this.FormatFileData.header, outputList);
                    }
                }
            }

            if (this.EDCShmooEnabled)
            {
                userData.TestResults[0].PassCount[0] += testRslt;
            }
        }
    }
}