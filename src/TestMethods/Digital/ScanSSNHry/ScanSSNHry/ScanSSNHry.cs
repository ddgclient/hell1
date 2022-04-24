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

namespace ScanSSNHry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Prime;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.Kernel.TestMethodsExtension;
    using Prime.PhAttributes;
    using Prime.PlistService;
    using Prime.TestMethods;

    /// <summary>
    /// This class is intended to extraxt the HRY data for scan using SSN pattern multipartition with rotation.
    /// </summary>
    [PrimeTestMethod]
    public class ScanSSNHry : TestMethodBase
    {
        private const int TestMethodFailPort = 0;
        private const int TestMethodPassPort = 1;
        private const int TestMethodFailsCaptured = 2;
        private const int TestMethodFailReset = 4;

        private List<string> partitionsFailing = new List<string>();
        private List<char> rawStringToPrint = new List<char>();
        private string printResultsToItuff = string.Empty;

        private bool isPlistExecutionPassed = true;
        private ICaptureFailureTest funcTest;
        private InputsProcessed hryInputDataProcessed = new InputsProcessed();
        private ScanSSNHRYCommonAlgorithm hryCommonAlgorithm = new ScanSSNHRYCommonAlgorithm();
        private List<string> maskPins;
        private List<string> debugPartitions;

        /// <summary>
        /// Gets or sets Patlist to execute.
        /// </summary>
        public TestMethodsParams.Plist Patlist { get; set; }

        /// <summary>
        /// Gets or sets TimingsTc for plist execution.
        /// </summary>
        public TestMethodsParams.TimingCondition TimingsTc { get; set; }

        /// <summary>
        /// Gets or sets LevelsTc to plist execution.
        /// </summary>
        public TestMethodsParams.LevelsCondition LevelsTc { get; set; }

        /// <summary>
        /// Gets or sets the PrePlist callback to plist execution.
        /// </summary>
        public TestMethodsParams.String PrePlist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets comma separated pins for mask.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString MaskPins { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets number of Patlist execution failures to capture per pattern. The minimal number is 1.
        /// </summary>
        public TestMethodsParams.UnsignedInteger PerPatFailCaptureCount { get; set; } = 1;

        /// <summary>
        /// Gets or sets HRY generation rules input file.
        /// </summary>
        public TestMethodsParams.File HRYInputFile { get; set; }

        /// <summary>
        /// Gets or sets pin mapping rules input file.
        /// </summary>
        public TestMethodsParams.File PinMappingInputFile { get; set; }

        /// <summary>
        /// Gets or sets PartitionsUnderDebug. When one of the failing partition, matching one of the regexes, test method will print 9 on the HRY string.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString PartitionsUnderDebug { get; set; } = string.Empty;

        /// <inheritdoc />
        public override void Verify()
        {
            this.VerifyMaskPins();
            this.funcTest = Prime.Services.FunctionalService.CreateCaptureFailureTest(
                this.Patlist,
                this.LevelsTc,
                this.TimingsTc,
                ulong.MaxValue,
                this.PerPatFailCaptureCount,
                this.PrePlist);
            this.VerifyIfFilesExist(this.HRYInputFile);
            this.VerifyIfFilesExist(this.PinMappingInputFile);
            this.ReadInputFile();
            this.VerifyIfPatternsInJson();
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "PASS PORT - NO FAILURES CAPTURED")]
        [Returns(0, PortType.Fail, "FAIL PORT")]
        [Returns(4, PortType.Fail, "FAIL PORT - RESET FAILING")]
        [Returns(2, PortType.Fail, "FAIL PORT - FAILURE CAPTURED AND PROCESSED SUCCESSFULLY")]

        // Main function where data is extracted.
        public override int Execute()
        {
            try
            {
                this.funcTest.ApplyTestConditions();
                this.SetPinMask();
                this.isPlistExecutionPassed = this.funcTest.Execute();
                List<IFailureData> scanFailData = this.funcTest.GetPerCycleFailures();
                this.GenerateHRY(scanFailData);
                return this.CalculateExitPort();
            }
            catch (TestMethodException exception)
            {
                Prime.Services.ConsoleService.PrintError($"Test method has failed with this error message=[{exception.Message}].");
                return TestMethodFailPort;
            }
        }

        /// <summary>
        /// This function creates HRY string to be printed to ituff.
        /// </summary>
        /// <param name="scanFails">scan failures to process.</param>
        public virtual void GenerateHRY(List<IFailureData> scanFails)
        {
            this.debugPartitions = this.PartitionsUnderDebug;
            this.hryCommonAlgorithm.GenerateHRY(this.hryInputDataProcessed, scanFails, this.PerPatFailCaptureCount, this.InstanceName, ref this.partitionsFailing, this.debugPartitions, ref this.rawStringToPrint);
        }

        /// <summary>
        /// This public function reads hry .json input file, and prepares data to be used in run-time.
        /// </summary>
        public virtual void ReadInputFile()
        {
            try
            {
                this.hryInputDataProcessed.ProcessData(this.HRYInputFile, this.PinMappingInputFile);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                throw new TestMethodException(
                    "Reading HRYInputsFile=[" + this.HRYInputFile + "] or [" + this.PinMappingInputFile + "]  has failed with this error message=["
                    + ex.Message + "].");
            }
        }

        /// <summary>
        /// This function verfy if patterns are mapped on the Jason file.
        /// </summary>
        public virtual void VerifyIfPatternsInJson()
        {
            try
            {
                this.hryInputDataProcessed.PatternsInJson(this.Patlist);
            }
            catch (TestMethodException ex)
            {
                Console.WriteLine("Reading VerifyExistsPatterns has failed with this error message= \n" + ex);
                throw new TestMethodException(
                    "Verify has failed with this error message= " + ex);
            }
        }

        /// <summary>
        /// Verify if file exist and is define as input parameter.
        /// </summary>
        /// <param name="file">File to verify.</param>
        public virtual void VerifyIfFilesExist(string file)
        {
            if (file == string.Empty)
            {
                throw new TestMethodException("Need to define all the input file.");
            }

            if (!Prime.Services.FileService.FileExists(file))
            {
                throw new TestMethodException($"File [{file}] does not exist.");
            }
        }

        // Function to mask the pins provided by the user.
        private void SetPinMask()
        {
            List<string> pinMasks = new List<string>();
            if (this.maskPins != null && this.maskPins.Count != 0)
            {
                pinMasks = pinMasks.Union(this.maskPins).ToList();
            }

            this.funcTest.SetPinMask(pinMasks);
        }

        /// <summary>
        /// This function will calculate the final exit port of the test method.
        /// Please see internal functions that it calls documentation for more details.
        /// </summary>
        /// <returns>Final exit port.</returns>
        private int CalculateExitPort()
        {
            ///////////////////
            // plist pass case
            ///////////////////
            if (this.isPlistExecutionPassed)
            {
                return 1;
            }

            ///////////////////
            // plist fail case
            ///////////////////
            int exitPort = TestMethodFailPort;

            try
            {
                this.CheckPartitionsFailing(ref exitPort);
            }
            catch (TestMethodException e)
            {
                Prime.Services.ConsoleService.PrintError(e.Message);
            }

            return exitPort;
        }

        private void CheckPartitionsFailing(ref int exitPort)
        {
            exitPort = TestMethodPassPort;
            if (this.rawStringToPrint.Contains('0'))
            {
                exitPort = TestMethodFailsCaptured;
            }

            foreach (var failingPat in this.partitionsFailing)
            {
                    Regex regex = new Regex("Reset");
                    Match match = regex.Match(failingPat);
                    if (match.Success)
                    {
                        exitPort = TestMethodFailReset;
                    }
            }
        }

        // Verifies mask pins
        private void VerifyMaskPins()
        {
            this.maskPins = this.MaskPins;
            foreach (string pinName in this.maskPins)
            {
                if (!Services.PinService.Exists(pinName))
                {
                    throw new TestMethodException($"Mask pin=[{pinName}] does not exist.\n");
                }
            }
        }
    }
}