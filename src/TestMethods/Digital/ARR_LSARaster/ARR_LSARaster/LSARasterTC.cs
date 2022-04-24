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
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Prime.FunctionalService;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;
    using Prime.VoltageService;

    /// <summary>
    /// This class is intended to overwrite the test method PrimeFuncCaptureFailuresTestMethod.
    /// </summary>
    [PrimeTestMethod]
    public class LSARasterTC : TestMethodBase
    {
        /// <summary> Default key to use when storing internal DB to sharedstorage. Just in case the user does not specify one. </summary>
        public const string DefaultPrescreenMapName = "Default_DB_Name";

        /// <summary> Prefix to append to PrescreenMapName to prevent access of unintended items in shared storage. </summary>
        public const string PrescreenMapNamePrefix = "LSARasterTC_";

        // This is used as regular expression to check failing pattern names in Prescreen Execute().
        private readonly Regex resetRegex = new Regex("reset", RegexOptions.IgnoreCase);

        private IInputFile serializedMetadata;
        private MetadataConfig deserializedMetadata;

        private IInputFile serializedHryTable;
        private HryTableConfigXml deserializedHryTable;

        private IInputFile serializedRaster;
        private RasterConfig deserializedRaster;

        private MetadataConfig.PinMappingSet pinMappingSet;

        private IFivrCondition voltageApply;

        /// <summary> List of pins to monitor when preforming prescreen test with CTVMODE enabled. </summary>
        private List<string> ctvModePinList;

        private bool isFivrMode;

        /// <summary>
        /// Enum representing which mode to use for this TC execution.
        /// </summary>
        public enum TestInstanceMode
        {
            /// <summary> Use Prescreen mode for this TC instance. </summary>
            PRESCREEN = 1,

            /// <summary> Use Raster mode for this TC instance. </summary>
            RASTER = 2,
        }

        /// <summary> Enum representing which print mode to use for Prescreen; mainly affects prints to ituff. </summary>
        public enum PrintMode
        {
            /// <summary> Set PreScreenMode to PASSMODE, ignore CTV capture, only report a pass or fail result. </summary>
            PASSMODE,

            /// <summary> Set PreScreenMode to FAILMODE, ignore CTV capture, report pass/fail along with array info. </summary>
            FAILMODE,

            /// <summary> Set PreScreenMode to CTVMODE, decode CTV bits, report pass/fail along with array info and decoded ctvData info.</summary>
            CTVMODE,
        }

        /// <summary>
        /// Gets or sets Patlist to execute.
        /// </summary>
        public TestMethodsParams.Plist Patlist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets TimingsTc for plist execution.
        /// </summary>
        public TestMethodsParams.TimingCondition TimingsTc { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets LevelsTc for plist execution.
        /// </summary>
        public TestMethodsParams.LevelsCondition LevelsTc { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the PrePlist callback to plist execution.
        /// </summary>
        public TestMethodsParams.String PrePlist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets  comma separated pins for mask.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString MaskPins { get; set; } = string.Empty;

        /*CTV Releated Parameters*/

        /// <summary>
        /// Gets or sets comma separated pins for CTV capture.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString CtvCapturePins { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets int for the number of global failures to capture. If 0 (default), it means failure capture is disabled.
        /// </summary>
        public TestMethodsParams.UnsignedInteger FailuresToCaptureTotal { get; set; } = 0;

        /// <summary>
        /// Gets or sets int for number of failures per pattern to capture. If 0 (default), per pattern capture is disabled.
        /// </summary>
        public TestMethodsParams.UnsignedInteger FailuresToCapturePerPattern { get; set; } = 0;

        /// <summary>
        /// Gets or sets int for the number of global failures to print to ituff. If 0 (default), it means failure prints are disabled.
        /// </summary>
        public TestMethodsParams.UnsignedInteger MaxFailuresToItuff { get; set; } = 0;

        /// <summary>
        /// Gets or sets int for number of failures per pattern to print to ituff. If 0 (default), per pattern failure count defaults to MaxFailuresToItuff.
        /// </summary>
        public TestMethodsParams.UnsignedInteger MaxFailuresPerPatternToItuff { get; set; } = 0;

        /// <summary>
        /// Gets or sets the configuration in case DTS processing is wanted.
        /// </summary>
        public TestMethodsParams.String DtsConfigurationName { get; set; } = string.Empty;

        /// <summary> Gets or sets which mode to use for this instance of of LSARaster. </summary>
        public TestInstanceMode ExecutionMode { get; set; } = TestInstanceMode.PRESCREEN;

        /// <summary> Gets or sets whether to print to raster tfile. </summary>
        public TestMethodsParams.String TfileRasterPrint { get; set; } = "True";

        /// <summary> Gets or sets filepath to MetadataConfig. Required for all TC instances. </summary>
        public TestMethodsParams.String MetadataConfigPath { get; set; }

        /// <summary> Gets or sets filepath to MetadataConfig's schema. Deprecated parameter. Not required. </summary>
        public TestMethodsParams.String MetadataConfigSchemaPath { get; set; }

        /// <summary> Gets or sets filepath to HryMap. Required when in Prescreen and PrescreenPrintMode is set to CTV_Mode. </summary>
        public TestMethodsParams.String HryMapPath { get; set; } = string.Empty;

        /// <summary> Gets or sets filepath to HryMap's schema. Deprecated parameter. Not required. </summary>
        public TestMethodsParams.String HryMapSchemaPath { get; set; } = string.Empty;

        /// <summary> Gets or sets filepath to RasterConfig. Required for Raster. </summary>
        public TestMethodsParams.String RasterConfigPath { get; set; } = string.Empty;

        /// <summary> Gets or sets filepath to RasterConfig's schema. Deprecated parameter. Not required. </summary>
        public TestMethodsParams.String RasterConfigSchemaPath { get; set; } = string.Empty;

        /// <summary> Gets or sets PinMappingSet to use during execution; this is a name defined in MetadataConfig. </summary>
        public TestMethodsParams.String PinMappingSetName { get; set; }

        /// <summary> Gets or sets ReductionConfigSet to use during Raster execution; optional if user needs to reduce internalDB. </summary>
        public TestMethodsParams.String ReductionConfigSetName { get; set; } = string.Empty;

        /// <summary> Gets or sets key used to store failing arrays in shared storage to be accessed later. Prescreen instance submits DB to this key, Raster instance uses this as the key to fetch from DB. </summary>
        public TestMethodsParams.String PrescreenMapName { get; set; } = DefaultPrescreenMapName;

        /// <summary> Gets or sets print mode for Prescreen. By default this is set to FAILMODE. </summary>
        public PrintMode PrescreenPrintMode { get; set; } = PrintMode.FAILMODE;

        /// <summary> Gets or sets HRY flow token for Prescreen. Used for ituff printing. </summary>
        public TestMethodsParams.String PrescreenHryFlowToken { get; set; }

        /// <summary> Gets or sets HRY frequency token for Prescreen. Used for ituff printing. </summary>
        public TestMethodsParams.String PrescreenHryFreqToken { get; set; }

        /// <summary> Gets or sets maximum number of failing arrays to print to Ituff. By default, this is set to 0, or unlimited. </summary>
        public TestMethodsParams.UnsignedInteger PrescreenMAFLimit { get; set; } = 0;

        /// <summary> Gets or sets tag to output info for repair during Rastermode. Required for Raster is passing defects to iCRepair. </summary>
        public TestMethodsParams.String OutputTag { get; set; } = string.Empty;

        /// <summary> Gets or sets a string representing the Fivr condition to use for this TC instance. Required if we're on a class socket. </summary>
        public TestMethodsParams.String FivrCondition { get; set; } = string.Empty;

        /// <summary> Gets or sets a string which will simulate the internal RasterMap for this TC instance. Eliminates the need to run Prescreen by faking locations which need to be rastered. </summary>
        public TestMethodsParams.String RasterMapSimulation { get; set; } = string.Empty;

        /// <summary>
        /// Submits all defects found during execution to iCRepair.
        /// </summary>
        /// <param name="outputTag"> Tag to submit defects to. </param>
        /// <param name="db"> Database containing defects that are being exported to iCRepair. </param>
        public static void ExportDefectsToRepair(TestMethodsParams.String outputTag, Dictionary<string, List<IDefect>> db)
        {
            StringBuilder repairBuilder = new StringBuilder();

            foreach (var keyToDefects in db)
            {
                foreach (var defect in keyToDefects.Value)
                {
                    if (defect.SendToRepair)
                    {
                        var temp = defect.CreateRepairString();

                        // We dont want to include blanks or duplicates (edit: removed because it adds a lot of TT)
                        if (!string.IsNullOrEmpty(temp))
                        {
                            repairBuilder.Append($"{temp};");
                            Prime.Services.ConsoleService.PrintDebug($"Appending defect to send to iCRepair: {temp}\n");
                        }
                    }
                }
            }

            if (repairBuilder.ToString().EndsWith(";"))
            {
                repairBuilder.Remove(repairBuilder.Length - 1, 1); // remove last delimiter character
            }

            /* Prime.Services.EvergreenService.SetGsdsUnit(outputTag, repairBuilder.ToString()); */
            Prime.Services.SharedStorageService.InsertRowAtTable(outputTag, repairBuilder.ToString(), Prime.SharedStorageService.Context.DUT);
            Prime.Services.ConsoleService.PrintDebug($"Raster Final Append {repairBuilder.ToString()}\n");
        }

        /// <summary>
        /// Verify method for this TC.
        /// </summary>
        public override void Verify()
        {
            this.isFivrMode = Prime.Services.TestProgramService.IsClassTestSocket();
            this.PrescreenMapName = PrescreenMapNamePrefix + this.PrescreenMapName;

            this.CheckAllRequiredParams();
            this.CreateConfigHandlers();
            this.DeserializeConfigs();
            this.ValidateDeserializedConfigs();

            // FOR OPCODE patMod request:
            // If you decide to settle on a GlobalMaxDefectsCount parameter, you have to create the patConfigHandles for maxDefectsCount, and set its data here to prevent massive testtime usage during execution...
            // All of the configuration files are initialized and checked at this point, you should have the info you need to begin the creation of the maxDefectCount patConfigHandles
        }

        /// <summary> Main entry point for the TC. </summary>
        /// <returns> Exit port for the TC. </returns>
        [Returns(3, PortType.Pass, "Port for at least one failure in memory in Prescreen.")]
        [Returns(2, PortType.Fail, "Port for label does not match MBD convention in Prescreen. Different failures while decoding defects in Raster. ")]
        [Returns(1, PortType.Pass, "No defects found in Prescreen. Test passed in Raster.")]
        [Returns(0, PortType.Fail, "Any fail condition.  (preamble fail, rasterconfig missing definition for array, or exception for domain with no labels.  Raster timeout.)")]
        public override int Execute()
        {
            int port;
            var timeTracker = Stopwatch.StartNew();

            if (this.ExecutionMode == TestInstanceMode.PRESCREEN)
            {
                Prime.Services.ConsoleService.PrintDebug("LSARasterTC instance set to Prescreen. Executing Prescreen...");
                var prescreenResult = this.ExecutePrescreenMode();
                port = prescreenResult.ExitPort;

                if (port == 3 && this.pinMappingSet.IsRasterModeSupported())
                {
                    prescreenResult.PrintInternalDbToConsole();
                    Prime.Services.ConsoleService.PrintDebug("Raster mode supported, submitting internal DB to SharedStorage.");
                    prescreenResult.SubmitDBToSharedStorage(this.PrescreenMapName);
                }
            }
            else if (this.ExecutionMode == TestInstanceMode.RASTER)
            {
                Prime.Services.ConsoleService.PrintDebug("LSARasterTC instance set to Raster. Executing Raster...");

                var rasterResult = this.ExecuteRasterMode();
                port = rasterResult.ExitPort;

                if (port == 1 || port == -1)
                {
                    if (this.OutputTag != string.Empty)
                    {
                        Prime.Services.ConsoleService.PrintDebug($"Submitting defects for repair at OutputTag [{this.OutputTag}]");
                        ExportDefectsToRepair(this.OutputTag, rasterResult.DefectDatabase);
                    }
                }
            }
            else
            {
                throw new Prime.Base.Exceptions.TestMethodException($"TestInstanceMode for this TC is not understood by the template\nCurrent instance mode: [{this.ExecutionMode}]");
            }

            timeTracker.Stop();
            Prime.Services.ConsoleService.PrintDebug($"Time to execute the TC: [{timeTracker.ElapsedMilliseconds} ms]");
            if (port == -1)
            {
                // Don't return port -1 from code.  This is only for a raster timeout currently.
                port = 0;
            }

            return port;
        }

        /// <summary>
        /// Validate serialized configurations through the use of their respective schema.
        /// </summary>
        /// <remarks> We can no longer use the Newtonsoft json validator nuget as it has a 1000hr time limit. This code is essentially useless until we find another solution. </remarks>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void ConfigSchemaCheck()
        {
            bool isRasterValid = true;

            if (this.ExecutionMode == TestInstanceMode.RASTER)
            {
                string rasterSchemaText = SharedFunctions.RetrieveTextFromFile(this.RasterConfigSchemaPath);
                isRasterValid = this.serializedRaster.Validate(rasterSchemaText);

                if (!isRasterValid)
                {
                    Prime.Services.ConsoleService.PrintError("RasterConfig invalidated by the schema.");
                }
            }

            string metadataSchemaText = SharedFunctions.RetrieveTextFromFile(this.MetadataConfigSchemaPath);
            bool isMetadataValid = this.serializedMetadata.Validate(metadataSchemaText);

            if (!isMetadataValid)
            {
                Prime.Services.ConsoleService.PrintError("Metadata file invalidated by the given schema.");
            }

            if (!(isRasterValid && isMetadataValid))
            {
                throw new Prime.Base.Exceptions.TestMethodException("One or more configuration files were invalidated by their respective schema");
            }
        }

        /// <summary>
        /// Perform internal check on each object required for execution of the TC. Depending on object, also initializes values needed for execution.
        /// </summary>
        public void ValidateDeserializedConfigs()
        {
            bool dwordsValid = true;
            bool pinMappingSetValid = true;

            this.pinMappingSet = this.deserializedMetadata.GetPinMappingSet(this.PinMappingSetName);
            pinMappingSetValid = this.pinMappingSet.ValidateAndSetupItems();

            if (this.ExecutionMode == TestInstanceMode.RASTER)
            {
                if (!this.pinMappingSet.IsRasterModeSupported())
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"Currently in Raster mode, but PinMappingSet [{this.PinMappingSetName}] does not support Raster; IsRasterModeSupported is set to false");
                }

                dwordsValid = this.deserializedRaster.Verify();
            }

            if (!dwordsValid || !pinMappingSetValid)
            {
                throw new Prime.Base.Exceptions.TestMethodException("At least one configuration file is not valid. Either malformed of missing critical element");
            }
        }

        /// <summary>
        /// Check all required params for this instance of LSARaster. Params required are determined by current ExecutionMode and other params set by the user.
        /// </summary>
        /// <remarks> TOS does mark which params are optional and which ones aren't, but since this TC is a combination of two modes, depending on what the user sets, the param requirements change. </remarks>
        public void CheckAllRequiredParams()
        {
            // required for both prescreen and raster
            Dictionary<string, object> defaultParams = new Dictionary<string, object>()
            {
                { "TimingsTC", this.TimingsTc },
                { "LevelsTC", this.LevelsTc },
                { "PinMappingSetName", this.PinMappingSetName },
                { "MetadataConfigPath", this.MetadataConfigPath },
                { "PrescreenMapName", this.PrescreenMapName },
            };

            Dictionary<string, object> prescreenParams = new Dictionary<string, object>()
            {
                { "Patlist", this.Patlist },
                { "PrescreenHryFlowToken", this.PrescreenHryFlowToken },
                { "PrescreenHryFreqToken", this.PrescreenHryFreqToken },
            };

            // when prescreen is in ctvMode
            Dictionary<string, object> prescreenCtvParams = new Dictionary<string, object>()
            {
                { "HryMapPath", this.HryMapPath },
            };

            Dictionary<string, object> rasterParams = new Dictionary<string, object>()
            {
                { "RasterConfigPath", this.RasterConfigPath },
            };

            List<string> missingParams = new List<string>();
            Dictionary<string, object> paramsToCheck;

            switch (this.ExecutionMode)
            {
                case TestInstanceMode.PRESCREEN:

                    if (this.PrescreenPrintMode == PrintMode.CTVMODE)
                    {
                        paramsToCheck = prescreenParams.Union(prescreenCtvParams).ToDictionary(k => k.Key, v => v.Value);
                    }
                    else
                    {
                        paramsToCheck = prescreenParams;
                    }

                    break;

                case TestInstanceMode.RASTER:
                    paramsToCheck = rasterParams;
                    break;

                default:
                    throw new Prime.Base.Exceptions.TestMethodException("TC instance did not declare a valid ExecutionMode");
            }

            foreach (KeyValuePair<string, object> pair in paramsToCheck)
            {
                if (pair.Value == null)
                {
                    missingParams.Add(pair.Key);
                }
            }

            foreach (KeyValuePair<string, object> pair in defaultParams)
            {
                if (pair.Value == null)
                {
                    missingParams.Add(pair.Key);
                }
            }

            if (missingParams.Count != 0)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"Missing parameters for this TC instance. Missing parameters are: {missingParams.Aggregate((i, j) => i + ", " + j)}");
            }
        }

        /// <summary>
        /// Deserialize the user configurations into objects needed for TC execution.
        /// </summary>
        public void DeserializeConfigs()
        {
            if (this.ExecutionMode == TestInstanceMode.PRESCREEN && this.PrescreenPrintMode == PrintMode.CTVMODE)
            {
                this.deserializedHryTable = this.serializedHryTable.DeserializeInput<HryTableConfigXml>();

                // Need to parse the HryTableConfig for all pins that we need to collect ctvData from when we execute the functional test
                HryConditionsChecker pinFinder = new HryConditionsChecker(this.deserializedHryTable.GetCriterias());
                var pinSet = pinFinder.GetListofPinsToMonitor();
                this.ctvModePinList = pinSet.ToList();
            }
            else if (this.ExecutionMode == TestInstanceMode.RASTER)
            {
                this.deserializedRaster = this.serializedRaster.DeserializeInput<RasterConfig>();
            }

            // MetadataConfig always needed
            this.deserializedMetadata = this.serializedMetadata.DeserializeInput<MetadataConfig>();
        }

        /// <summary>
        /// This method handles the creation of IInputFile objects for deserialization of user configs. IInputFile objects will handle how to deserialize and verify an object depending on its filetype.
        /// </summary>
        public void CreateConfigHandlers()
        {
            if (this.ExecutionMode == TestInstanceMode.PRESCREEN && this.PrescreenPrintMode == PrintMode.CTVMODE)
            {
                this.serializedHryTable = InputFactory.CreateConfigHandler(this.HryMapPath);
            }

            if (this.ExecutionMode == TestInstanceMode.RASTER)
            {
                this.serializedRaster = InputFactory.CreateConfigHandler(this.RasterConfigPath);
            }

            this.serializedMetadata = InputFactory.CreateConfigHandler(this.MetadataConfigPath);
        }

        /// <summary>
        /// Performs a functional test using the PLIST defined in the TC params, then passes fail info to the main Prescreen algorithm.
        /// </summary>
        /// <returns> A <see cref="PrescreenTest"/> object containing execution results. </returns>
        private PrescreenTest ExecutePrescreenMode()
        {
            // Need to apply FIVR condition if we're in class socket
            if (this.isFivrMode)
            {
                this.voltageApply = Prime.Services.VoltageService.CreateFivrForCondition(this.FivrCondition, this.Patlist);
            }
            else
            {
                this.voltageApply = null;
            }

            bool testPass;
            List<IFailureData> failData;
            Dictionary<string, string> ctvData;

            var prescreen = new PrescreenTest(this.pinMappingSet, this.PrescreenHryFlowToken, this.PrescreenHryFreqToken, this.PrescreenMAFLimit, this.PrescreenMapName, this.PrescreenPrintMode, this.Patlist);

            try
            {
                if (this.PrescreenPrintMode == PrintMode.CTVMODE)
                {
                    // Value of 32000; hardcoded to what capture count was in EVG
                    var test = Prime.Services.FunctionalService.CreateCaptureFailureAndCtvPerPinTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.ctvModePinList, 32000, this.PrePlist);
                    test.ApplyTestConditions();
                    this.voltageApply?.ApplyCondition();

                    testPass = test.Execute();
                    failData = test.GetPerCycleFailures();
                    ctvData = test.GetCtvData();

                    PrescreenTest.PrintCtvsToConsole(ctvData);

                    foreach (var fail in failData)
                    {
                        Match resetMatch = this.resetRegex.Match(fail.GetPatternName());
                        if (Prime.Services.PlistService.GetPlistObject(this.Patlist).IsPatternAnAmble(fail.GetPatternName()) || resetMatch.Success)
                        {
                            prescreen.ExitPort = 0;
                            Prime.Services.ConsoleService.PrintError("Unit failed during an amble pattern. Main prescreen algorithm will not be executed.");
                            return prescreen;
                        }
                    }

                    test.Reset();
                    prescreen.Execute(testPass, failData, ctvData, this.deserializedHryTable);
                }
                else
                {
                    // Value of 32000; hardcoded to what capture count was in EVG
                    var test = Prime.Services.FunctionalService.CreateCaptureFailureTest(this.Patlist, this.LevelsTc, this.TimingsTc, 32000, this.PrePlist);
                    test.ApplyTestConditions();
                    this.voltageApply?.ApplyCondition();

                    testPass = test.Execute();
                    failData = test.GetPerCycleFailures();

                    foreach (var fail in failData)
                    {
                        Match resetMatch = this.resetRegex.Match(fail.GetPatternName());
                        if (Prime.Services.PlistService.GetPlistObject(this.Patlist).IsPatternAnAmble(fail.GetPatternName()) || resetMatch.Success)
                        {
                            prescreen.ExitPort = 0;
                            Prime.Services.ConsoleService.PrintError("Unit failed during an amble pattern. Main prescreen algorithm will not be executed.");
                            return prescreen;
                        }
                    }

                    prescreen.Execute(testPass, failData);
                }
            }
            catch (Exception ex)
            {
                Prime.Services.ConsoleService.PrintError("Error during functional test execution.");
                throw ex;
            }

            return prescreen;
        }

        private RasterTest ExecuteRasterMode()
        {
            var rasterTest = new RasterTest(this.deserializedMetadata, this.deserializedRaster, this.LevelsTc, this.TimingsTc, this.PinMappingSetName, this.FivrCondition, this.ReductionConfigSetName, this.PrescreenMapName, this.TfileRasterPrint, this.RasterMapSimulation);
            rasterTest.Execute();
            return rasterTest;
        }
    }
}