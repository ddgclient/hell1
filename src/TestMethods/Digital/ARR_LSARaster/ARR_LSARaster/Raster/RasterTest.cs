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
    using Prime.VoltageService;

    /// <summary>
    /// Class representing functionality of RasterMode for LSARaster test class.
    /// </summary>
    public class RasterTest
    {
        /// <summary>
        /// Identifier for when a label is considered complete.
        /// </summary>
        public const string CompleteLabelIdentifier = "Complete";

        /// <summary>
        /// Name to use for sliceId when executing a test in parallel mode.
        /// </summary>
        public const string ParallelSlice = "PARALLEL";

        private Dictionary<string, List<IDefect>> defectDB = new Dictionary<string, List<IDefect>>();
        private MetadataConfig.PinMappingSet pinMappingSet;
        private MetadataConfig.ArrayType arrayType;

        // Map of ArrayName + Slice -> Defect objects

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterTest"/> class.
        /// </summary>
        /// <param name="deserializedMetadata"> Instance of <see cref="MetadataConfig"/>, must be derived from the same JSON used in prescreen. </param>
        /// <param name="deserializedRaster"> Instance of <see cref="RasterConfig"/>. </param>
        /// <param name="levelsName"> Name of levels file to use for plist execution.</param>
        /// <param name="timingsName"> Name of timings  file to use for plist execution.</param>
        /// <param name="pinMappingSetName"> Name of pinMappingSet to use for this instance of Raster.</param>
        /// <param name="fivrCondition"> Fivr domain to use when executing plist. Does not need to be used at sort. </param>.
        /// <param name="reductionConfigSetName"> Name of ReductionConfigSet to use when reducting internal DB. </param>
        /// <param name="prescreenMapName"> Key to use when fetching internal DB from SharedStorage. </param>
        /// <param name="simulationString"> String used to simulate rasterMap for testing. </param>
        /// <param name="tfileRasterPrint"> Should tfile be printed to by raster. </param>
        public RasterTest(MetadataConfig deserializedMetadata, RasterConfig deserializedRaster, string levelsName, string timingsName, string pinMappingSetName, string fivrCondition, string reductionConfigSetName, string prescreenMapName, string tfileRasterPrint = "true", string simulationString = "")
        {
            this.DeserializedMetadata = deserializedMetadata;
            this.DeserializedRaster = deserializedRaster;
            this.LevelsName = levelsName;
            this.TimingsName = timingsName;
            this.PinMappingSetName = pinMappingSetName;
            this.FivrCondition = fivrCondition;
            this.ReductionConfigSetName = reductionConfigSetName;
            this.PrescreenMapName = prescreenMapName;
            this.SimulationString = simulationString;
            this.TfileRasterPrint = tfileRasterPrint;
        }

        /// <summary>
        /// Values representing the final status of algorithm execution.
        /// </summary>
        public enum ExecutionStates
        {
            /// <summary>
            /// Failed cycle on an amble pattern.
            /// </summary>
            RasterTimeout = -1,

            /// <summary>
            /// Failed cycle on an amble pattern.
            /// </summary>
            FailOnAmble = 0,

            /// <summary>
            /// HRY sent us here but rasterconfig doesn't have array defined.
            /// </summary>
            RasterConfigUndefined = 0,

            /// <summary>
            /// Fails have been raster/algorithm works as expected.
            /// </summary>
            Success = 1,

            /// <summary>
            /// Fail on cycle containing label other than complete label.
            /// </summary>
            FailOnNonCompleteLabel = 2,
        }

        /// <summary> Gets or sets an instance of <see cref="MetadataConfig"/>, must be derived from the same JSON used in prescreen. </summary>
        public MetadataConfig DeserializedMetadata { get; set; }

        /// <summary> Gets or sets an instance of <see cref="RasterConfig"/>. </summary>
        public RasterConfig DeserializedRaster { get; set; }

        /// <summary>
        /// Gets database containing defects after performing <see cref="RasterTest.Execute"/>.
        /// </summary>
        public Dictionary<string, List<IDefect>> DefectDatabase
        {
            get
            {
                return this.defectDB;
            }

            private set
            {
                this.defectDB = value;
            }
        }

        /// <summary> Gets exit port for this test instance. </summary>
        public int ExitPort { get; private set; }

        /// <summary> Gets or sets the T-File info for this test instance. </summary>
        public string TFile { get; set; }

        /// <summary> Gets or sets name of levels file to use for plist execution. </summary>
        public string LevelsName { get; set; }

        /// <summary> Gets or sets name of timings file to use for plist execution. </summary>
        public string TimingsName { get; set; }

        /// <summary> Gets or sets name of pinMappingSet to use for this instance of Raster. </summary>
        public string PinMappingSetName { get; set; }

        /// <summary> Gets or sets name of reductionConfigSet to use for this instance of Raster. </summary>
        public string ReductionConfigSetName { get; set; }

        /// <summary> Gets or sets the map name to retrieve the internal DB from SharedStorage.. </summary>
        public string PrescreenMapName { get; set; }

        /// <summary> Gets or sets FivrCondition to use for plist execution. </summary>
        public string FivrCondition { get; set; }

        /// <summary> Gets or sets string used for simulating internal DB for debugging purposes. </summary>
        public string SimulationString { get; set; }

        /// <summary> Gets or sets whether to print Tfile raster results. </summary>
        public string TfileRasterPrint { get; set; }

        /// <summary>Gets or sets the uservar collection name for the X,Y identifiers for this DUT (for the TFile).</summary>
        public string DutCollection { get; set; } = "SCVars"; // FIXME - this only works with sort I think. need to make this more robust

        /// <summary>Gets or sets the uservar variable name for the X identifiers for this DUT (for the TFile).</summary>
        public string DutXGlobal { get; set; } = "SC_WAFERX"; // FIXME - this only works with sort I think. need to make this more robust

        /// <summary>Gets or sets the uservar variable name for the Y identifiers for this DUT (for the TFile).</summary>
        public string DutYGlobal { get; set; } = "SC_WAFERY"; // FIXME - this only works with sort I think. need to make this more robust

        /// <summary>
        /// Returns a mapping of each pin's ctvData to its decoded information based on the captureSet provided.
        /// </summary>
        /// <param name="ctvData"> Dictionary of each pin mapped to its returned ctvData. </param>
        /// <param name="pinMappingSet"> PinMappingSet containing required metadata for ctv decode. </param>
        /// <param name="captureSet"> CaptureSet used to decode the ctvData. </param>
        /// <param name="currentArray"> Array name to use for defect data. </param>
        /// <param name="dwordElement"> <see cref="RasterConfig.DwordElementContainer"/> used for the plist execution when gathering the ctvs for the specified array/dword address combo. </param>
        /// <returns> Mapping of pin to decoded ctvData. Map(Pin name -> List(Map(Dword element -> value))). </returns>
        /// <remarks> Would probably be better to convert the List of mappings to a list of objects for better clarity. </remarks>
        public static List<IDefect> DecodeCtvData(Dictionary<string, string> ctvData, MetadataConfig.PinMappingSet pinMappingSet, MetadataConfig.CaptureSet captureSet, string currentArray, RasterConfig.DwordElementContainer dwordElement)
        {
            List<IDefect> defects = new List<IDefect>();

            foreach (KeyValuePair<string, string> pinToCtv in ctvData)
            {
                string currentPin = pinToCtv.Key;
                string ctvBits = pinToCtv.Value;
                Prime.Services.ConsoleService.PrintDebug($"Current Pin: [{currentPin}]\nDecoding ctvBits: [{ctvBits}]");

                // Length of ctv must be a multiple of defined length by user. If ctvBits are length == 0, this is fine, each defect class handles this case in its own way.
                if ((ctvBits.Length % captureSet.Length) != 0)
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"CtvData from pin {pinToCtv.Key} is not a multiple of {captureSet.Length}\nBits found: {ctvBits.Length}");
                }

                List<string> ctvChunks = SeperateStringToChunks(ctvBits, (uint)captureSet.Length);
                List<IDefect> decodedCtvData = DefectFactory.DecodeDefects(ctvChunks, pinMappingSet, captureSet, currentArray, dwordElement, currentPin);
                defects.AddRange(decodedCtvData);
            }

            return defects;
        }

        /// <summary>
        /// Execution of test on a particular MBD address when targeting an array.
        /// </summary>
        /// <param name="pinsForCtvCapture"> List of pins to capture ctvs from. </param>
        /// <param name="pinsToMask"> Pins to mask during test execution. </param>
        /// <param name="handles"> <see cref="Prime.PatConfigService.IPatConfigHandle"/> to apply before execution. </param>
        /// <param name="pinMappingSet"> PinMappingSet to use to determine configurations to apply before test. </param>
        /// <param name="plistToExecute"> Plist name to execute. </param>
        /// <param name="currentSliceId"> SliceId used to determine settings for execution. </param>
        /// <param name="fivrCondition"> Fivr condition to use for execution. </param>
        /// <param name="levelsName"> Name of levels to apply for execution. </param>
        /// <param name="timingsName"> Name of timings to apply for execution. </param>
        /// <param name="faildata"> List of <see cref="IFailureData"/> retrieved during execution. </param>
        /// <returns> CTV Data from test execution. </returns>
        public static Dictionary<string, string> ExecuteTestOnMBDAddress(
            List<string> pinsForCtvCapture,
            List<string> pinsToMask,
            List<Prime.PatConfigService.IPatConfigHandle> handles,
            MetadataConfig.PinMappingSet pinMappingSet,
            string plistToExecute,
            string currentSliceId,
            string fivrCondition,
            string levelsName,
            string timingsName,
            out List<IFailureData> faildata)
        {
            Prime.Services.ConsoleService.PrintDebug($"Executing PLIST [{plistToExecute}] for array [{plistToExecute}]");

            IFivrCondition voltageApply = null;
            Prime.PlistService.IPlistObject plistObj = null;

            // Pattern per sliceId will have multiple pattern defined per PLIST
            // I'm not sure if this behavior should be allowed w/ Multicore as well. I'll assume the PDEs know/will understand the debug log if both are enabled and that's the incorrect behavior.
            if (pinMappingSet.HasPatternPerSliceId)
            {
                Prime.Services.ConsoleService.PrintDebug($"Array has pattern per sliceId, disabling all other patterns not mapped to current slice [{currentSliceId}]");

                if (currentSliceId == ParallelSlice)
                {
                    throw new Prime.Base.Exceptions.TestMethodException("Cannot use PatternPerSliceId if template is set to parallel raster");
                }

                var patternToExecute = pinMappingSet.GetPatternMappedToSlice(currentSliceId);
                Prime.Services.ConsoleService.PrintDebug($"Pattern [{patternToExecute}] mapped to sliceId [{currentSliceId}]. Disabling all other patterns in current Plist");
                plistObj = Prime.Services.PlistService.GetPlistObject(plistToExecute);
                plistObj.EnableGivenPatternsDisableRest(new HashSet<string>() { patternToExecute });
            }

            // If we're in class socket, we need to apply Fivr conditions along with other patMods
            if (fivrCondition != string.Empty)
            {
                voltageApply = Prime.Services.VoltageService.CreateFivrForCondition(fivrCondition, plistToExecute);
            }

            Prime.Services.PatConfigService.Apply(handles);

            try
            {
                voltageApply?.ApplyCondition();

                // TODO: this test method does not support preplist.
                var test = Prime.Services.FunctionalService.CreateCaptureFailureAndCtvPerPinTest(plistToExecute, levelsName, timingsName, pinsForCtvCapture, 1, string.Empty);
                test.SetPinMask(pinsToMask);
                test.ApplyTestConditions();
                test.Execute();
                var ctvData = test.GetCtvData();
                faildata = test.GetPerCycleFailures();

                PrescreenTest.PrintCtvsToConsole(ctvData);

                test.Reset();
                return ctvData;
            }
            catch (Exception ex)
            {
                Prime.Services.ConsoleService.PrintError("Error during functional test execution.");
                throw ex;
            }
            finally
            {
                plistObj?.EnableAllPatterns();
            }
        }

        /// <summary>
        /// Convert a mapping of sliceId to set of MBD addresses to a mapping of MBD addresses to unique slices.
        /// </summary>
        /// <param name="sliceToAddresses"> Mapping of slice to mbd addresses. </param>
        /// <returns> A dict using tuples as keys with a set of unique slices attached. </returns>
        public static Dictionary<Tuple<int, int, int>, HashSet<string>> GetSlicesForMBDAddress(Dictionary<string, List<Tuple<int, int, int>>> sliceToAddresses)
        {
            Dictionary<Tuple<int, int, int>, HashSet<string>> mbdToSlices = new Dictionary<Tuple<int, int, int>, HashSet<string>>();

            foreach (var pair in sliceToAddresses)
            {
                string currentSlice = pair.Key;

                foreach (var mbdAddress in pair.Value)
                {
                    if (!mbdToSlices.ContainsKey(mbdAddress))
                    {
                        mbdToSlices.Add(mbdAddress, new HashSet<string>());
                    }

                    mbdToSlices[mbdAddress].Add(currentSlice);
                }
            }

            return mbdToSlices;
        }

        /// <summary>
        /// LogDatabase.
        /// </summary>
        /// <param name="database">.</param>
        public static void LogDatabaseReductions(DBContainer database)
        {
            var reductionsWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();

            if (database.DoMaxCoreReductionsExist())
            {
                reductionsWriter.SetTnamePostfix("CORES_MAX_COUNT_REDUCTION");
                reductionsWriter.SetData(database.SummarizeMaxCoresCount());
                Prime.Services.DatalogService.WriteToItuff(reductionsWriter);
                Prime.Services.ConsoleService.PrintDebug(reductionsWriter.ToString());
            }

            if (database.DoMaxArrayReductionsExist())
            {
                reductionsWriter.SetTnamePostfix("ARRAY_MAX_COUNT_REDUCTION");
                reductionsWriter.SetData(database.SummarizeArrayMaxCount());
                Prime.Services.DatalogService.WriteToItuff(reductionsWriter);
                Prime.Services.ConsoleService.PrintDebug(reductionsWriter.ToString());
            }

            if (database.DoMafReductionsExist())
            {
                reductionsWriter.SetTnamePostfix("ARRAY_MAF_MAX_REDUCTION");
                reductionsWriter.SetData(database.SummarizeMAFMaxCount());
                Prime.Services.DatalogService.WriteToItuff(reductionsWriter);
                Prime.Services.ConsoleService.PrintDebug(reductionsWriter.ToString());
            }

            if (database.DoMBDReductionsExist())
            {
                reductionsWriter.SetTnamePostfix("MBDS_REDUCTION");
                reductionsWriter.SetData(database.SummarizeMBDSReduction());
                Prime.Services.DatalogService.WriteToItuff(reductionsWriter);
                Prime.Services.ConsoleService.PrintDebug(reductionsWriter.ToString());
            }
        }

        /// <summary>
        /// Return the plist to execute given an LDAT array and the slice we're executing on.
        /// </summary>
        /// <param name="arrayConfig"> The configuration of the LDAT array we'll execute on. </param>
        /// <param name="pinMappingSet"> PinMappingSet used to determine the plist to execute. </param>
        /// <param name="currentSliceId"> SliceID we're targeting for the given LDAT array. </param>
        /// <returns> Name of the plist to execute to target this array. </returns>
        public static string GetPlistToExecute(RasterConfig.LdatArray arrayConfig, MetadataConfig.PinMappingSet pinMappingSet, string currentSliceId)
        {
            // Multicore runs will have multiple PLIST defined for each array
            if (pinMappingSet.MulticorePatternEnabled)
            {
                Prime.Services.ConsoleService.PrintDebug($"PinMappingSet MulticorePatternEnabled is set to true. Attempting to find a PLIST matched to the current sliceId [{currentSliceId}]");
                return GetMulticorePlistToExecute(pinMappingSet, arrayConfig, currentSliceId);
            }
            else
            {
                // Should only be one plist within config if not in Multicore...
                try
                {
                    return arrayConfig.PlistName[0];
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Prime.Services.ConsoleService.PrintError("No plists were defined for the given array. Please check the configuration for the current LDAT array for proper plist definition.");
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Plist to Execute.
        /// </summary>
        /// <param name="pmSet"> PinMappingSet used for this Raster execution.</param>
        /// <param name="arrayConfig"> Array configuration for LdatArray currently being rastered.</param>
        /// <param name="currentSliceId"> Current slice we're rastering.</param>
        /// <returns>plist.</returns>
        public static string GetMulticorePlistToExecute(MetadataConfig.PinMappingSet pmSet, RasterConfig.LdatArray arrayConfig, string currentSliceId)
        {
            var slices = pmSet.GetSliceList();

            if (slices.Count != arrayConfig.PlistName.Count)
            {
                throw new Prime.Base.Exceptions.TestMethodException("Number of slices in PinMappingSet does not match the number of plists defined in the RasterConfig file");
            }

            string hryName = $"_{pmSet.GetHryNameFromSliceId(currentSliceId).ToLower()}_";

            foreach (var plist in arrayConfig.PlistName)
            {
                if (plist.ToLower().Contains(hryName))
                {
                    return plist;
                }
            }

            throw new Prime.Base.Exceptions.TestMethodException($"PinMappingSet is in Multicore mode, but could not find a plist matching [{hryName}] for current array");
        }

        /// <summary>
        /// SeparateStringToChunks.
        /// </summary>
        /// <param name="ctvBits">.</param>
        /// <param name="length">1.</param>
        /// <returns>chunks.</returns>
        public static List<string> SeperateStringToChunks(string ctvBits, uint length)
        {
            List<string> chunks = new List<string>();
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (char character in ctvBits)
            {
                sb.Append(character);
                i++;

                if (i == length)
                {
                    i = 0;
                    chunks.Add(sb.ToString());
                    sb.Clear();
                }
            }

            return chunks;
        }

        /// <summary>
        /// CheckDFMFails.
        /// </summary>
        /// <param name="failData">.</param>
        /// <param name="parentPlist"> Plist that cycle fail belongs to.</param>
        /// <returns>true/false.</returns>
        public static ExecutionStates CheckDFMFails(List<IFailureData> failData, string parentPlist)
        {
            if (failData.Count > 0)
            {
                // We've entered a failing state in the algorithm; determine the cause.
                if (!SharedFunctions.CheckAllLabelsContain(failData, out var fail, CompleteLabelIdentifier))
                {
                    var label = Prime.Services.PatternService.GetLabelFromAddress(fail.GetPatternName(), fail.GetDomainName(), (uint)fail.GetVectorAddress(), false);
                    var plistObj = Prime.Services.PlistService.GetPlistObject(parentPlist);
                    if (plistObj.IsPatternAnAmble(fail.GetPatternName()))
                    {
                        Prime.Services.ConsoleService.PrintError($"Unit failed on amble pattern. Pattern\"{fail.GetPatternName()}\" at address \"{fail.GetVectorAddress()}\".\nFailing label: [{label.GetName()}]");
                        return ExecutionStates.FailOnAmble;
                    }
                    else
                    {
                        Prime.Services.ConsoleService.PrintError($"Pattern\"{fail.GetPatternName()}\" at address \"{fail.GetVectorAddress()}\" ended on non-complete label. Pbist engine error.\nFailing label: [{label.GetName()}]");
                        return ExecutionStates.FailOnNonCompleteLabel;
                    }
                }
            }

            return ExecutionStates.Success;
        }

        /// <summary>
        /// PrintRasterMap.
        /// </summary>
        /// <param name="rasterMap"> Defects.</param>
        /// <param name="keyName"> Name of key to use when printing out array defects.</param>
        public static void PrintRasterMap(Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>> rasterMap, string keyName)
        {
            StringBuilder mapPrinter = new StringBuilder();
            Prime.Services.ConsoleService.PrintDebug("Performing raster on following arrays at specified locations:\n");

            foreach (var arrayNameToSliceOrModuleMap in rasterMap)
            {
                string currentArrayName = arrayNameToSliceOrModuleMap.Key;

                foreach (var sliceOrModuleToMBDValues in arrayNameToSliceOrModuleMap.Value)
                {
                    string currentKey = sliceOrModuleToMBDValues.Key;
                    mapPrinter.Append($"Array: {currentArrayName} {keyName}: {currentKey} MBD: ");
                    foreach (var mbdValue in sliceOrModuleToMBDValues.Value)
                    {
                        mapPrinter.Append($"[{mbdValue.Item1}{mbdValue.Item2}{mbdValue.Item3}]");
                    }

                    mapPrinter.Append("\n");
                }
            }

            Prime.Services.ConsoleService.PrintDebug(mapPrinter.ToString());
        }

        /// <summary>
        /// Execute method for RasterMode. Entry point for LSARasterTC when set to Raster.
        /// </summary>
        public void Execute()
        {
            Prime.Services.ConsoleService.PrintDebug("Beginning start of raster execute...");
            Prime.Services.ConsoleService.PrintDebug($"Using PinMappingSet defined by user: {this.PinMappingSetName}");

            this.pinMappingSet = this.DeserializedMetadata.GetPinMappingSet(this.PinMappingSetName);
            this.arrayType = this.pinMappingSet.GetArrayType();

            RasterConfig.ReductionConfigSet reductionConfigSet = null;
            DBContainer database;
            Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>> rasterMap;

            if (this.ReductionConfigSetName != string.Empty)
            {
                Prime.Services.ConsoleService.PrintDebug($"Getting reduction map defined by user in raster config [{this.ReductionConfigSetName}]");
                reductionConfigSet = this.DeserializedRaster.GetReductionConfigSet(this.ReductionConfigSetName);
            }

            // User want's to fake the DB if this value is populated. Ignore SharedStorage
            if (!string.IsNullOrWhiteSpace(this.SimulationString))
            {
                database = DBContainer.CreateDBFromString(this.SimulationString, this.arrayType, reductionConfigSet);
            }
            else
            {
                database = DBContainer.GetDBFromStorage(this.PrescreenMapName, reductionConfigSet);
            }

            rasterMap = database.CreateRasterMap(this.arrayType);
            PrintRasterMap(rasterMap, MetadataConfig.ConvertArrayTypeToDBKeyName(this.arrayType));

            ExecutionStates finalStatus = ExecutionStates.Success;

            if (this.DeserializedRaster.IsRasterInParallel())
            {
                Prime.Services.ConsoleService.PrintDebug("Rastering in parallel...");
                finalStatus = this.RasterDatabaseInParallel(rasterMap);
            }
            else
            {
                Prime.Services.ConsoleService.PrintDebug("Rastering serially...");
                finalStatus = this.RasterDatabaseSerially(rasterMap);
            }

            this.ExitPort = (int)finalStatus;

            // -1 currently comes from raster timing out > 30 seconds.
            if (this.ExitPort == 1 || this.ExitPort == -1)
            {
                this.TFile = this.GenerateTFileStrings(this.DefectDatabase); // FIXME: Let's make this a public method that's called by LSARasterTC class so we can unit test this...
                LogDatabaseReductions(database); // FIXME: Same here... make this public

                // If we have some sort of actual defect (indicated by a 0x hex value) then we want to print to the TFile.  Otherwise, do not.
                if (this.TFile.Contains("0x") && !this.TfileRasterPrint.ToLower().Contains("false"))
                {
                    Prime.Services.DatalogService.WriteToTFile(this.TFile);
                }
                else
                {
                    Prime.Services.ConsoleService.PrintDebug($"Skipping TFile writing, because no defects were found.  Or TfileRasterPrint param is set to 'false'. Looking for '0x' in tfile string.  This is what you're missing:\n {this.TFile.ToString()}");
                }
            }
        }

        /// <summary>
        /// GenerateTfileStrings.
        /// </summary>
        /// <param name="defectDB">Defect Database.</param>
        /// <returns>String.</returns>
        public string GenerateTFileStrings(Dictionary<string, List<IDefect>> defectDB)
        {
            if (defectDB.Count == 0)
            {
                Prime.Services.ConsoleService.PrintDebug($"defectDB is empty, returning from GenerateTFileStrings because nothing to do.");
                return string.Empty;
            }

            // FIXME: Change it so we refer to the db using .this
            var dutX = "0";
            var dutY = "0";
            StringBuilder tFileBuilder = new StringBuilder();

            if (Prime.Services.UserVarService.Exists(this.DutCollection, this.DutXGlobal) &&
                   Prime.Services.UserVarService.Exists(this.DutCollection, this.DutYGlobal))
            {
                dutX = Prime.Services.UserVarService.GetStringValue(this.DutCollection, this.DutXGlobal);
                dutY = Prime.Services.UserVarService.GetStringValue(this.DutCollection, this.DutYGlobal);
                Prime.Services.ConsoleService.PrintDebug($"Assigned DUT X/Y from {this.DutCollection}.{this.DutXGlobal} and {this.DutCollection}.{this.DutYGlobal}");
            }

            string testName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
            tFileBuilder.Append($"DUT {dutX}, {dutY}\n");
            tFileBuilder.Append($"Test: {testName}\n");

            List<string> defects_list = new List<string>();

            foreach (var keyToDefectInfo in defectDB)
            {
                // tFileBuilder.Append(keyToDefectInfo.Value[0].CreateTFileHeaderBlock());
                foreach (var defect in keyToDefectInfo.Value)
                {
                    // Instead of appending directly to tfile string, add to dictionary so we can remove duplicate printing of array/slice below.
                    string tFile_string = defect.CreateTFileString();

                    if (!string.IsNullOrEmpty(tFile_string))
                    {
                        defects_list.Add(tFile_string);
                    }

                    // tFileBuilder.Append(defect.CreateTFileString());
                }
            }

            defects_list.Sort();
            defects_list = defects_list.Distinct().ToList();

            var last_array = "none";
            var last_slice = "none";

            foreach (var defect in defects_list)
            {
                var defect_no_array_no_slice = Regex.Replace(defect, @"Array: (.*?)\n(Slice|Module): (.*?)\n", string.Empty);

                var defect_no_array = Regex.Replace(defect, @"Array: (.*?)\n", string.Empty);

                Match match = Regex.Match(defect, @"Array: (.*?)\n(?:Slice|Module): (.*?)\n");
                var current_array = match.Groups[1].Value;
                var current_slice = match.Groups[2].Value;

                var defect_new = defect;

                Prime.Services.ConsoleService.PrintDebug($"Defect RAW: {Regex.Replace(defect, "\n", "\\n")}\n\nCurrent Array={current_array}, Last Array={last_array}, Current Slice={current_slice}, Last Slice={last_slice}\n");

                if (last_array == current_array)
                {
                    defect_new = defect_no_array;

                    if (last_slice == current_slice && last_array == current_array)
                    {
                        defect_new = defect_no_array_no_slice;
                    }
                }

                last_array = current_array;
                last_slice = current_slice;

                Prime.Services.ConsoleService.PrintDebug($"Defect RAW Returned: {Regex.Replace(defect_new, "\n", "\\n")}\n");
                tFileBuilder.Append(defect_new);
            }

            return tFileBuilder.ToString();
        }

        /// <summary>
        /// Iterate over every MBD address that failed for a given array, but only enable capture for one rootKey at a time. For an MBD address, multiple executions may be required if that failing MBD for a given array has failed more than one rootKey.
        /// </summary>
        /// <remarks> RootKey changes depending on product. Ex. BigCore uses SliceId for rootKey, ATOM uses module for rootKey. </remarks>
        /// <param name="rasterMap"> Mapping of Map(ArrayName -> Map(RootKey -> List of MBD)). </param>
        /// <returns> An enum representing the status of the algorithm execution. </returns>
        public ExecutionStates RasterDatabaseSerially(Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>> rasterMap)
        {
            Stopwatch stop = Stopwatch.StartNew();
            Prime.Services.ConsoleService.PrintDebug("Beginning serial raster algorithm...");
            bool setRasterFailFlag = false;

            // Iterate over each failing array
            foreach (var arrayToSlices in rasterMap)
            {
                string currentArrayName = arrayToSlices.Key;
                Prime.Services.ConsoleService.PrintDebug($"Attempting raster on array [{currentArrayName}]");

                if (!this.DeserializedRaster.CheckIfLdatArrayHasRasterEnabled(currentArrayName))
                {
                    Prime.Services.ConsoleService.PrintDebug("Raster is not enabled for this array (RasterExists=False in RasterConfig JSON)");
                    continue;
                }

                var arrayConfig = this.DeserializedRaster.GetLdatArray(currentArrayName);
                if (arrayConfig == null)
                {
                    Prime.Services.ConsoleService.PrintError($"Array isn't found in raster config.  Skipping to next.  (Do you mean to set RasterExists='false' in RasterConfig.json?)");
                    setRasterFailFlag = true;
                    continue;
                }

                var captureSet = this.DeserializedMetadata.GetCaptureSet(arrayConfig.CaptureSetName);
                var mbdToSlice = GetSlicesForMBDAddress(arrayToSlices.Value);
                Prime.Services.ConsoleService.PrintDebug("Iterating over all unique addresses...");

                // Iterate over each failing MBD
                foreach (var pair in mbdToSlice)
                {
                    var currentMBDAddress = pair.Key;
                    var currentMBDAddressAsString = $"{currentMBDAddress.Item1},{currentMBDAddress.Item2},{currentMBDAddress.Item3}";
                    var slices = pair.Value.ToList();
                    Prime.Services.ConsoleService.PrintDebug("Determining pins for ctv capture...");
                    List<string> pinForCurrentSlice = this.pinMappingSet.GetPinMappedToRootKey(slices);
                    List<string> pinsToMask = this.pinMappingSet.GetPinsToMask(slices);

                    // Iterate over each failing Slice/Module(rootKey)
                    foreach (var slice in slices)
                    {
                        // 45 second timeout trying to loop through raster, give up otherwise.
                        if (stop.Elapsed.TotalSeconds > 45)
                        {
                            Prime.Services.ConsoleService.PrintError("Raster timed out > 45 seconds! returning port 0.  Do you need rasterreduction config?");
                            stop.Reset();
                            return ExecutionStates.RasterTimeout;
                        }

                        string pinForCtvCapture = this.pinMappingSet.GetPinMappedToRootKey(slice);
                        Prime.Services.ConsoleService.PrintDebug($"Pin for ctv capture: {pinForCtvCapture}\n");
                        Prime.Services.ConsoleService.PrintDebug($"Performing raster on current MBD address [{currentMBDAddressAsString}] for slice [{slice}]. Begin matching to labels defined in raster config...");

                        bool didMatch = arrayConfig.MatchAddressToDwordElement(currentMBDAddress, out var dwordElement);

                        if (didMatch)
                        {
                            Prime.Services.ConsoleService.PrintDebug("MBD address matched to DwordElement in raster config. Performing plist execution...");
                            string plistToExecute = GetPlistToExecute(arrayConfig, this.pinMappingSet, slice);
                            var handles = arrayConfig.CreatePatConfigHandles(dwordElement, plistToExecute);
                            var ctvData = ExecuteTestOnMBDAddress(
                                new List<string>() { pinForCtvCapture },
                                pinsToMask,
                                handles,
                                this.pinMappingSet,
                                plistToExecute,
                                slice,
                                this.FivrCondition,
                                this.LevelsName,
                                this.TimingsName,
                                out var failData);

                            var status = CheckDFMFails(failData, plistToExecute);

                            switch (status)
                            {
                                case ExecutionStates.FailOnAmble:
                                    return status;
                                case ExecutionStates.FailOnNonCompleteLabel:
                                    return status;
                                default:
                                    break;
                            }

                            Prime.Services.ConsoleService.PrintDebug("Performing ctv decode...");

                            // FIXME: Get Dword info from failIo if specified in configurations
                            var decodedCtvData = DecodeCtvData(ctvData, this.pinMappingSet, captureSet, currentArrayName, dwordElement);
                            this.StoreDefects(decodedCtvData);
                        }
                        else
                        {
                            throw new Prime.Base.Exceptions.TestMethodException($"Could not match MBD address [{currentMBDAddressAsString}] for array [{currentArrayName}] to any MBD address defined in RasterConfig.");
                        }
                    } // End of each failing slice/module
                } // End of each failing MBD
            } // End of each failing array

            Prime.Services.ConsoleService.PrintDebug($"Raster Execute finished in {stop.Elapsed.TotalSeconds}");
            stop.Reset();

            if (setRasterFailFlag)
            {
                return ExecutionStates.RasterConfigUndefined;
            }

            return ExecutionStates.Success;
        }

        /// <summary>
        /// Iterate over every MBD address that failed for a given array, but enable capture on all failing rootKeys (capture ctvs on pins mapped to those rootKeys).
        /// </summary>
        /// <remarks> RootKey changes depending on product. Ex. BigCore uses SliceId for rootKey, ATOM uses module for rootKey. </remarks>
        /// <param name="rasterMap"> Mapping of Map(ArrayName -> Map(RootKey -> List of MBD)). </param>
        /// <returns> An enum representing the status of the algorithm execution. </returns>
        public ExecutionStates RasterDatabaseInParallel(Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>> rasterMap)
        {
            Stopwatch stop = Stopwatch.StartNew();

            Prime.Services.ConsoleService.PrintDebug("Beginning parallel raster algorithm...");
            bool setRasterFailFlag = false;

            // Iterate over each failing array
            foreach (var arrayToSlices in rasterMap)
            {
                string currentArrayName = arrayToSlices.Key;
                Prime.Services.ConsoleService.PrintDebug($"Attempting raster on array [{currentArrayName}]");

                if (!this.DeserializedRaster.CheckIfLdatArrayHasRasterEnabled(currentArrayName))
                {
                    Prime.Services.ConsoleService.PrintDebug("Raster is not enabled for this array (RasterExists=False in RasterConfig JSON)");
                    continue;
                }

                var arrayConfig = this.DeserializedRaster.GetLdatArray(currentArrayName);
                if (arrayConfig == null)
                {
                    Prime.Services.ConsoleService.PrintError($"Array isn't found in raster config.  Skipping to next.  (Do you mean to set RasterExists='false' in RasterConfig.json?)");
                    setRasterFailFlag = true;
                    continue;
                }

                var captureSet = this.DeserializedMetadata.GetCaptureSet(arrayConfig.CaptureSetName);
                var mbdToSlice = GetSlicesForMBDAddress(arrayToSlices.Value);

                Prime.Services.ConsoleService.PrintDebug("Iterating over all unique addresses...");

                // Iterating over each failing MBD
                foreach (var pair in mbdToSlice)
                {
                    // 45 second timeout trying to loop through raster, give up otherwise.
                    if (stop.Elapsed.TotalSeconds > 45)
                    {
                        stop.Reset();
                        Prime.Services.ConsoleService.PrintError("Raster timed out > 45 seconds! returning port 0.  Do you need rasterreduction config?");
                        return ExecutionStates.RasterTimeout;
                    }

                    var currentMBDAddress = pair.Key;
                    var currentMBDAddressAsString = $"{currentMBDAddress.Item1},{currentMBDAddress.Item2},{currentMBDAddress.Item3}";
                    var slices = pair.Value.ToList();
                    Prime.Services.ConsoleService.PrintDebug("Determining pins for ctv capture...");
                    List<string> pinsForCtvCapture = this.pinMappingSet.GetPinMappedToRootKey(slices);
                    List<string> pinsToMask = this.pinMappingSet.GetPinsToMask(slices);

                    Prime.Services.ConsoleService.PrintDebug($"Pins for ctv capture: {pinsForCtvCapture.Aggregate((i, j) => i + ", " + j)}\n");
                    Prime.Services.ConsoleService.PrintDebug($"Performing raster on current MBD address [{pair.Key}]. Begin matching to labels defined in raster config...");

                    bool didMatch = arrayConfig.MatchAddressToDwordElement(currentMBDAddress, out var dwordElement);

                    if (didMatch)
                    {
                        Prime.Services.ConsoleService.PrintDebug("MBD address matched to DwordElement in raster config. Performing plist execution...");

                        // Pass in all pins that are mapped to slices that failed for specified MBD address for parallel capture.
                        string plistToExecute = GetPlistToExecute(arrayConfig, this.pinMappingSet, string.Empty);
                        var handles = arrayConfig.CreatePatConfigHandles(dwordElement, plistToExecute);
                        var ctvData = ExecuteTestOnMBDAddress(
                            pinsForCtvCapture,
                            pinsToMask,
                            handles,
                            this.pinMappingSet,
                            plistToExecute,
                            ParallelSlice,
                            this.FivrCondition,
                            this.LevelsName,
                            this.TimingsName,
                            out var failData);

                        var status = CheckDFMFails(failData, plistToExecute);

                        switch (status)
                        {
                            case ExecutionStates.FailOnAmble:
                                return status;
                            case ExecutionStates.FailOnNonCompleteLabel:
                                return status;
                            default:
                                break;
                        }

                        Prime.Services.ConsoleService.PrintDebug("Performing ctv decode...");

                        var defects = DecodeCtvData(ctvData, this.pinMappingSet, captureSet, currentArrayName, dwordElement);
                        this.StoreDefects(defects);
                    }
                    else
                    {
                        throw new Prime.Base.Exceptions.TestMethodException($"Could not match MBD address [{currentMBDAddressAsString}] for array [{currentArrayName}] to any MBD address defined in raster config.");
                    }
                } // End of each failing MBD
            } // End of each failing array

            Prime.Services.ConsoleService.PrintDebug($"Raster Execute finished in {stop.Elapsed.TotalSeconds}");
            stop.Reset();

            if (setRasterFailFlag)
            {
                return ExecutionStates.RasterConfigUndefined;
            }

            return ExecutionStates.Success;
        }

        /// <summary>
        /// Stores defects detected into internal database.
        /// </summary>
        /// <param name="defects"> Defects to store in internal database. </param>
        public void StoreDefects(List<IDefect> defects)
        {
            foreach (var defect in defects)
            {
                defect.AddToInternalDatabase(ref this.defectDB);
            }
        }
    }
}