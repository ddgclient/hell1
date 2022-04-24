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
    using System.Linq;
    using System.Text;
    using Prime.TestMethods;

    /// <summary>
    /// A class to act as a container for all FailArray info. Contains helper methods to assist in DB management.
    /// </summary>
    public class DBContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DBContainer"/> class.
        /// </summary>
        public DBContainer()
        {
        }

        /// <summary> Gets or sets generic structure used to store all fail array info for prescreen. </summary>
        /// <remarks> Mapping of Map(SliceId/Module -> Map(ArrayName -> FailInfo)). </remarks>
        public Dictionary<string, Dictionary<string, HashSet<Tuple<int, int, int>>>> PrescreenDatabase { get; set; } = new Dictionary<string, Dictionary<string, HashSet<Tuple<int, int, int>>>>();

        /// <summary> Gets or sets a rearranged version of the "Database" member variable to handle during raster. </summary>
        /// <remarks> Mapping of Map(ArrayName -> Map(SliceId/Module -> MBDs)). </remarks>
        public Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>> RasterDatabase { get; set; } = new Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>>();

        /// <summary> Gets or sets datalog of reductions when reducing cores for this Raster execution. Tuple represents array name, sliceId, number of failing MBDs. </summary>
        public List<Tuple<string, string, int>> CoreReductionsDatalog { get; set; } = new List<Tuple<string, string, int>>();

        /// <summary> Gets or sets datalog of reductions when removing slices for arrays that have reached the MAF limit. Tuple represents array name, sliceId, number of failing MBDs. </summary>
        public List<Tuple<string, string, int>> MafReductionsDatalog { get; set; } = new List<Tuple<string, string, int>>();

        /// <summary> Gets or sets datalog of reductions when removing arrays for slices that have exceeded MaxArrayCount, but less than MAF limit. Tuple represents array name, sliceId, number of failing MBDs. </summary>
        public List<Tuple<string, string, int>> MaxArrayReductionsDatalog { get; set; } = new List<Tuple<string, string, int>>();

        /// <summary> Gets or sets datalog of reductions when removing MBDS that exceeded MaxMBDCountPerArrayPerSlice. Tuple represents array name, slice, defect address (MBD values concatenated). </summary>
        public List<Tuple<string, string, string>> MaxMBDCountReductionsDatalog { get; set; } = new List<Tuple<string, string, string>>();

        /// <summary> Gets or sets Reduction config set containing params for reduction. </summary>
        public RasterConfig.ReductionConfigSet ReductionParameters { get; set; } = null;

        /// <summary>
        /// Method for getting internal DB from storage.
        /// </summary>
        /// <param name="prescreenMapName"> Map name/key to use when fetching internalDB from SharedStorage. </param>
        /// <param name="reductionConfigSet"> Reduction configurations to use when reducing internal DB. </param>
        /// <returns> InternalDB from SharedStorage. </returns>
        public static DBContainer GetDBFromStorage(string prescreenMapName, RasterConfig.ReductionConfigSet reductionConfigSet)
        {
            Prime.Services.ConsoleService.PrintDebug($"Fetching internal DB from previous prescreen instance with key: {prescreenMapName}");
            object internalDB;

            try
            {
                internalDB = Prime.Services.SharedStorageService.GetRowFromTable(prescreenMapName, typeof(Dictionary<string, Dictionary<string, List<string>>>), Prime.SharedStorageService.Context.DUT);
            }
            catch (Exception ex)
            {
                Prime.Services.ConsoleService.PrintError($"Failed to retrieve internalDB using key: {prescreenMapName}");
                throw ex;
            }

            var sharedStorageDB = internalDB as Dictionary<string, Dictionary<string, List<string>>>;

            if (sharedStorageDB.Count <= 0)
            {
                throw new Prime.Base.Exceptions.TestMethodException($"InternalDB retrieved with key [{prescreenMapName}] was null or empty");
            }

            DBContainer database = new DBContainer();
            database.RasterDatabase = ConvertSharedStorageDBForRaster(sharedStorageDB);
            database.ReductionParameters = reductionConfigSet;
            return database;
        }

        /// <summary>
        /// Creates a DBContainer by faking a rasterMap from a string.
        /// </summary>
        /// <param name="simulationDB"> String containing simulated raster map. </param>
        /// <param name="arrayType"> Type of array this DBContainer is dealing with. </param>
        /// <param name="reductionConfigSet"> Reduction params to use on the simulated map. </param>
        /// <returns> Instance of <see cref="DBContainer"/> with a simulated DB. </returns>
        public static DBContainer CreateDBFromString(string simulationDB, MetadataConfig.ArrayType arrayType, RasterConfig.ReductionConfigSet reductionConfigSet = null)
        {
            Prime.Services.ConsoleService.PrintDebug("Simulation string detected, faking database...");
            DBContainer database = new DBContainer();
            database.RasterDatabase = CreateRasterMapFromString(simulationDB, arrayType);
            database.ReductionParameters = reductionConfigSet;
            return database;
        }

        /// <summary>
        /// Method to submit prescreen DB to SharedStorage. Since Tuples can't be stored in SharedStorage, has extra logic for converting a list of tuples into a list of strings.
        /// </summary>
        /// <remarks> This method has extra logic to convert the internal structure to something that can be stored in SharedStorage. </remarks>
        /// <param name="prescreenMapKey"> Key used to submit the internal DB to SharedStorage. </param>
        public void StoreDBInStorage(string prescreenMapKey)
        {
            var sharedStorageDB = new Dictionary<string, Dictionary<string, List<string>>>();

            foreach (var rootKeyToArrayDB in this.PrescreenDatabase)
            {
                string rootKey = rootKeyToArrayDB.Key;
                if (!sharedStorageDB.ContainsKey(rootKey))
                {
                    sharedStorageDB.Add(rootKey, new Dictionary<string, List<string>>());
                }

                foreach (var arrayNameToMBDAddress in rootKeyToArrayDB.Value)
                {
                    string arrayName = arrayNameToMBDAddress.Key;
                    if (!sharedStorageDB[rootKey].ContainsKey(arrayName))
                    {
                        sharedStorageDB[rootKey].Add(arrayName, new List<string>());
                    }

                    foreach (var mbdAddress in arrayNameToMBDAddress.Value)
                    {
                        sharedStorageDB[rootKey][arrayName].Add($"{mbdAddress.Item1},{mbdAddress.Item2},{mbdAddress.Item3}");
                    }
                }
            }

            Prime.Services.SharedStorageService.InsertRowAtTable(prescreenMapKey, sharedStorageDB, Prime.SharedStorageService.Context.DUT);
        }

        /// <summary>
        /// Method to add new MBD address at a designated location within the database.
        /// </summary>
        /// <param name="failArray"> Fail array we're mapping to specified sliceId. </param>
        /// <param name="keyToUse"> Value of the root key for the DB. </param>
        /// <param name="keyName"> Name of the root key. </param>
        public void AddNewEntry(FailedArray failArray, string keyToUse, string keyName)
        {
            Prime.Services.ConsoleService.PrintDebug($"Mapping fail array to internal DB using {keyName}: [{keyToUse}]");

            if (this.PrescreenDatabase.ContainsKey(keyToUse))
            {
                if (this.PrescreenDatabase[keyToUse].ContainsKey(failArray.ArrayName))
                {
                    this.PrescreenDatabase[keyToUse][failArray.ArrayName].Add(failArray.MBDAddress);
                }
                else
                {
                    this.PrescreenDatabase[keyToUse].Add(failArray.ArrayName, new HashSet<Tuple<int, int, int>> { failArray.MBDAddress });
                }
            }
            else
            {
                this.PrescreenDatabase.Add(keyToUse, new Dictionary<string, HashSet<Tuple<int, int, int>>>());
                this.PrescreenDatabase[keyToUse].Add(failArray.ArrayName, new HashSet<Tuple<int, int, int>>() { failArray.MBDAddress });
            }
        }

        /// <summary>
        /// Method to print prescreen internal DB to console.
        /// </summary>
        /// <param name="arrayType"> Type of array this DB contains. </param>
        public void PrintInternalDbToConsole(MetadataConfig.ArrayType arrayType)
        {
            string rootKeyName = MetadataConfig.ConvertArrayTypeToDBKeyName(arrayType);

            StringBuilder dbInfoBuilder = new StringBuilder();
            foreach (var keyToArrayMap in this.PrescreenDatabase)
            {
                string rootKey = keyToArrayMap.Key;
                dbInfoBuilder.Append($"{rootKeyName}: {rootKey}\n");
                foreach (var arrayToMBDs in keyToArrayMap.Value)
                {
                    string arrayName = arrayToMBDs.Key;
                    dbInfoBuilder.Append($"Array Name: {arrayName}\n");
                    foreach (var mbd in arrayToMBDs.Value)
                    {
                        dbInfoBuilder.Append($"Multiport: {mbd.Item1} Bank: {mbd.Item2} Dword: {mbd.Item3}\n");
                    }
                }

                Prime.Services.ConsoleService.PrintDebug(dbInfoBuilder.ToString());
                dbInfoBuilder.Clear();
            }
        }

        /// <summary>
        /// Reduces internal datalog of reductions for ituff printing.
        /// </summary>
        /// <returns> A string representing datalog reductions. </returns>
        public string SummarizeMaxCoresCount()
        {
            var reductionDatalog = this.CoreReductionsDatalog;
            Dictionary<string, Dictionary<string, int>> reductionSummary = new Dictionary<string, Dictionary<string, int>>();

            // Creation of summary
            foreach (var reduction in reductionDatalog)
            {
                string arrayName = reduction.Item1;
                string sliceId = reduction.Item2;

                if (!reductionSummary.ContainsKey(arrayName))
                {
                    reductionSummary.Add(arrayName, new Dictionary<string, int>());
                }

                if (!reductionSummary[arrayName].ContainsKey(sliceId))
                {
                    reductionSummary[arrayName].Add(sliceId, 0);
                }

                reductionSummary[arrayName][sliceId]++;
            }

            StringBuilder reductionBuilder = new StringBuilder();

            foreach (var arrayNameToSlice in reductionSummary)
            {
                reductionBuilder.Append($"array_{arrayNameToSlice.Key}_");

                foreach (var sliceToMBDCount in arrayNameToSlice.Value)
                {
                    string slice = sliceToMBDCount.Key;
                    int mbdCount = sliceToMBDCount.Value;
                    reductionBuilder.Append($"slice_{slice}_{mbdCount}_");
                }
            }

            // Remove last underscore
            reductionBuilder.Remove(reductionBuilder.Length - 1, 1);

            return reductionBuilder.ToString();
        }

        /// <summary>
        /// Method indicating whether reductions exist for this property.
        /// </summary>
        /// <returns> A boolean value. </returns>
        public bool DoMafReductionsExist()
        {
            return this.MafReductionsDatalog.Count > 0;
        }

        /// <summary>
        /// Method indicating whether reductions exist for this property.
        /// </summary>
        /// <returns> A boolean value. </returns>
        public bool DoMBDReductionsExist()
        {
            return this.MaxMBDCountReductionsDatalog.Count > 0;
        }

        /// <summary>
        /// Method indicating whether reductions exist for this property.
        /// </summary>
        /// <returns> A boolean value. </returns>
        public bool DoMaxArrayReductionsExist()
        {
            return this.MaxArrayReductionsDatalog.Count > 0;
        }

        /// <summary>
        /// Method indicating whether reductions exist for this property.
        /// </summary>
        /// <returns> A boolean value. </returns>
        public bool DoMaxCoreReductionsExist()
        {
            return this.CoreReductionsDatalog.Count > 0;
        }

        /// <summary>
        /// Reduces internal datalog of reductions for ituff printing.
        /// </summary>
        /// <returns> A string representing datalog reductions. </returns>
        public string SummarizeMBDSReduction()
        {
            var reductionDatalog = this.MaxMBDCountReductionsDatalog;
            Dictionary<string, Dictionary<string, HashSet<string>>> reductionSummary = new Dictionary<string, Dictionary<string, HashSet<string>>>();

            // Creation of summary
            foreach (var reduction in reductionDatalog)
            {
                string arrayName = reduction.Item1;
                string sliceId = reduction.Item2;
                string mbdAddress = reduction.Item3;

                if (!reductionSummary.ContainsKey(arrayName))
                {
                    reductionSummary.Add(arrayName, new Dictionary<string, HashSet<string>>());
                }

                if (!reductionSummary[arrayName].ContainsKey(sliceId))
                {
                    reductionSummary[arrayName].Add(sliceId, new HashSet<string>());
                }

                reductionSummary[arrayName][sliceId].Add(mbdAddress);
            }

            StringBuilder reductionBuilder = new StringBuilder();

            foreach (var arrayNameToSlice in reductionSummary)
            {
                reductionBuilder.Append($"array_{arrayNameToSlice.Key}_");

                foreach (var sliceToMBDs in arrayNameToSlice.Value)
                {
                    reductionBuilder.Append($"slice_{sliceToMBDs.Key}_");
                    foreach (var mbd in sliceToMBDs.Value)
                    {
                        reductionBuilder.Append($"mbd_{mbd}_");
                    }
                }
            }

            // Remove last underscore
            reductionBuilder.Remove(reductionBuilder.Length - 1, 1);

            return reductionBuilder.ToString();
        }

        /// <summary>
        /// Reduces internal datalog of reductions for ituff printing.
        /// </summary>
        /// <returns> A string representing datalog reductions. </returns>
        public string SummarizeMAFMaxCount()
        {
            var reductionDatalog = this.MafReductionsDatalog;
            Dictionary<string, Dictionary<string, int>> reductionSummary = new Dictionary<string, Dictionary<string, int>>();

            // Creation of summary
            foreach (var reduction in reductionDatalog)
            {
                string arrayName = reduction.Item1;
                string sliceId = reduction.Item2;
                int removedMBDs = reduction.Item3;

                if (!reductionSummary.ContainsKey(sliceId))
                {
                    reductionSummary.Add(sliceId, new Dictionary<string, int>());
                }

                if (!reductionSummary[sliceId].ContainsKey(arrayName))
                {
                    reductionSummary[sliceId].Add(arrayName, 0);
                }

                reductionSummary[sliceId][arrayName] += removedMBDs;
            }

            StringBuilder reductionBuilder = new StringBuilder();

            foreach (var sliceToArrayDB in reductionSummary)
            {
                reductionBuilder.Append($"slice_{sliceToArrayDB.Key}_");

                foreach (var arrayToMBDCount in sliceToArrayDB.Value)
                {
                    reductionBuilder.Append($"array_{arrayToMBDCount.Key}_mbds_{arrayToMBDCount.Value}_");
                }
            }

            // Remove last underscore
            reductionBuilder.Remove(reductionBuilder.Length - 1, 1);

            return reductionBuilder.ToString();
        }

        /// <summary>
        /// Reduces internal datalog of reductions for ituff printing.
        /// </summary>
        /// <returns> A string representing datalog reductions. </returns>
        public string SummarizeArrayMaxCount()
        {
            var reductionDatalog = this.MaxArrayReductionsDatalog;
            Dictionary<string, Dictionary<string, int>> reductionSummary = new Dictionary<string, Dictionary<string, int>>();

            // Creation of summary
            foreach (var reduction in reductionDatalog)
            {
                string arrayName = reduction.Item1;
                string sliceId = reduction.Item2;
                int removedMBDs = reduction.Item3;

                if (!reductionSummary.ContainsKey(sliceId))
                {
                    reductionSummary.Add(sliceId, new Dictionary<string, int>());
                }

                if (!reductionSummary[sliceId].ContainsKey(arrayName))
                {
                    reductionSummary[sliceId].Add(arrayName, 0);
                }

                reductionSummary[sliceId][arrayName] += removedMBDs;
            }

            StringBuilder reductionBuilder = new StringBuilder();

            foreach (var sliceToArrayName in reductionSummary)
            {
                reductionBuilder.Append($"array_{sliceToArrayName.Key}_");

                foreach (var arrayToMBDCount in sliceToArrayName.Value)
                {
                    reductionBuilder.Append($"slice_{arrayToMBDCount.Key}_{arrayToMBDCount.Value}_");
                }
            }

            // Remove last underscore
            reductionBuilder.Remove(reductionBuilder.Length - 1, 1);

            return reductionBuilder.ToString();
        }

        /// <summary> Method for reducing internal DB + storing reductions for datalogging. </summary>
        /// <param name="typeOfArray"> Type of array this database is holding. </param>
        /// <returns> Reduced raster map containing all Arrays mapped to slices, and MBDs mapped to the slices. </returns>
        public Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>> CreateRasterMap(MetadataConfig.ArrayType typeOfArray)
        {
            this.TranslateIntoRasterMap();

            // This algorithm would technically work with ATOM as well, but it would reduce MBDs per module/Arrays per module/modules per array instead of core/slice.
            // Also, no need for this for ATOM since # of arrays is minimal from what I was told
            // If (this.ReductionParameters != null && typeOfArray == MetadataConfig.ArrayType.BIGCORE)
            if (this.ReductionParameters != null)
            {
                Prime.Services.ConsoleService.PrintDebug("Reduction configuration defined, reducing internal DB using reduction parameters");
                this.ReduceSlicesPerArray();
                this.ReduceArraysPerSlice();
                this.ReduceMBDsPerArrayPerSlice();
            }

            return this.RasterDatabase;
        }

        /// <summary>
        /// Converts the DB in shared storage back into a DB that's useable for the main algorithm.
        /// </summary>
        /// <param name="sharedStorageDB"> The generic structure that was stored in sharedStorage. </param>
        /// <returns> A dictionary that can be used for the raster algorithm. </returns>
        private static Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>> ConvertSharedStorageDBForRaster(Dictionary<string, Dictionary<string, List<string>>> sharedStorageDB)
        {
            Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>> rasterDB = new Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>>();

            foreach (var rootKeyToArrayDB in sharedStorageDB)
            {
                string rootKey = rootKeyToArrayDB.Key;
                if (!rasterDB.ContainsKey(rootKey))
                {
                    rasterDB.Add(rootKey, new Dictionary<string, List<Tuple<int, int, int>>>());
                }

                foreach (var arrayNameToMBDAddress in rootKeyToArrayDB.Value)
                {
                    string arrayName = arrayNameToMBDAddress.Key;
                    if (!rasterDB[rootKey].ContainsKey(arrayName))
                    {
                        rasterDB[rootKey].Add(arrayName, new List<Tuple<int, int, int>>());
                    }

                    foreach (var mbdAddress in arrayNameToMBDAddress.Value)
                    {
                        string[] fields = mbdAddress.Split(',');

                        if (fields.Length != 3)
                        {
                            throw new Prime.Base.Exceptions.TestMethodException($"String submitted to internalDB was malformed; could not parse MBD address {mbdAddress}");
                        }

                        rasterDB[rootKey][arrayName].Add(new Tuple<int, int, int>(int.Parse(fields[0]), int.Parse(fields[1]), int.Parse(fields[2])));
                    }
                }
            }

            return rasterDB;
        }

        /// <summary>
        /// When in simulation mode, create an internal DB when given a string.
        /// </summary>
        /// <param name="simulationDB"> Formatted string containing info for creating RasterMap. </param>
        /// <param name="arrayType"> Type of array that this database will hold. </param>
        private static Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>> CreateRasterMapFromString(string simulationDB, MetadataConfig.ArrayType arrayType)
        {
            Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>> rasterMap = new Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>>();
            var seperateDefects = simulationDB.Split(';');
            string rootKeyName = MetadataConfig.ConvertArrayTypeToDBKeyName(arrayType);

            foreach (var defect in seperateDefects)
            {
                var fields = defect.Split(',');

                if (fields.Length != 3)
                {
                    throw new Prime.Base.Exceptions.TestMethodException($"Simulation string subsection {defect} could not properly be parsed. Does not contain three different fields.");
                }

                var mbdValues = fields[2].Split('|');
                int multiport = int.Parse(mbdValues[0]);
                int bank = int.Parse(mbdValues[1]);
                int dword = int.Parse(mbdValues[2]);
                string arrayName = fields[0];
                string rootKey = fields[1];
                Prime.Services.ConsoleService.PrintDebug($"Creating rasterMap with defect pointing at location:\nArray={arrayName} {rootKeyName}={rootKey} MBD={multiport}{bank}{dword}");

                if (!rasterMap.ContainsKey(rootKey))
                {
                    rasterMap.Add(rootKey, new Dictionary<string, List<Tuple<int, int, int>>>());
                }

                if (!rasterMap[rootKey].ContainsKey(arrayName))
                {
                    rasterMap[rootKey].Add(arrayName, new List<Tuple<int, int, int>>());
                }

                rasterMap[rootKey][arrayName].Add(new Tuple<int, int, int>(multiport, bank, dword));
            }

            return rasterMap;
        }

        /// <summary>
        /// Reduces the # of MBD per array per slice within the RasterMap; records reductions for later logging.
        /// </summary>
        private void ReduceMBDsPerArrayPerSlice()
        {
            Dictionary<string, int> maxMBDPerArray = this.GetMaxMBDPerArray();

            foreach (var arrayNameToSliceMap in this.RasterDatabase)
            {
                string currentArray = arrayNameToSliceMap.Key;
                if (maxMBDPerArray.TryGetValue(currentArray, out var maxMBDForCurrentArray))
                {
                    foreach (var sliceToMBDs in arrayNameToSliceMap.Value)
                    {
                        string currentSlice = sliceToMBDs.Key;
                        int mbdCountForCurrentSlice = sliceToMBDs.Value.Count;

                        if (mbdCountForCurrentSlice > maxMBDForCurrentArray)
                        {
                            int mbdsToRemove = mbdCountForCurrentSlice - maxMBDForCurrentArray;
                            Prime.Services.ConsoleService.PrintDebug($"Array [{currentArray}] for Slice [{currentSlice}] is exceeding MBDMaxCount by [{mbdsToRemove}]. Beginning reduction.");

                            for (int i = 0; i < mbdsToRemove; i++)
                            {
                                var mbd = sliceToMBDs.Value[0];
                                sliceToMBDs.Value.RemoveAt(0);
                                Tuple<string, string, string> reductionEntry = new Tuple<string, string, string>(currentArray, currentSlice, $"{mbd.Item1}_{mbd.Item2}_{mbd.Item3}");
                                this.MaxMBDCountReductionsDatalog.Add(reductionEntry);
                            }
                        }
                    }
                }
            }
        }

        private Dictionary<string, int> GetMaxMBDPerArray()
        {
            return this.ReductionParameters.MaxMBDsCount;
        }

        /// <summary>
        /// Reduces the # of arrays per slice within the RasterMap; records reductions for later logging.
        /// </summary>
        private void ReduceArraysPerSlice()
        {
            int mafLimit = this.GetMafLimit();
            int maxArrayCount = this.GetArrayMaxCount();
            List<string> arrayPriority = this.GetArrayPriority();

            Dictionary<string, int> sliceToArrayCount = new Dictionary<string, int>();

            foreach (var arrayNameToSliceMap in this.RasterDatabase)
            {
                foreach (var sliceToMBDs in arrayNameToSliceMap.Value)
                {
                    string currentSlice = sliceToMBDs.Key;

                    if (!sliceToArrayCount.ContainsKey(currentSlice))
                    {
                        sliceToArrayCount.Add(currentSlice, 0);
                    }

                    sliceToArrayCount[currentSlice]++;
                }
            }

            foreach (var pair in sliceToArrayCount)
            {
                string currentSlice = pair.Key;
                int numOfArray = pair.Value;

                // If slice exceeds maf limit, datalog reduction and remove it from raster map
                if (numOfArray > mafLimit)
                {
                    foreach (var arrayNameToSliceMap in this.RasterDatabase)
                    {
                        string currentArray = arrayNameToSliceMap.Key;

                        if (this.RasterDatabase[currentArray].ContainsKey(currentSlice))
                        {
                            int mbdCount = this.RasterDatabase[currentArray][currentSlice].Count;
                            Tuple<string, string, int> reductionEntry = new Tuple<string, string, int>(currentArray, currentSlice, mbdCount);
                            this.MafReductionsDatalog.Add(reductionEntry);
                            this.RasterDatabase[currentArray].Remove(currentSlice);
                        }
                    }
                }
                else if (numOfArray > maxArrayCount)
                {
                    int arraysToRemove = numOfArray - maxArrayCount;
                    int arraysRemoved = 0;

                    foreach (var arrayNameToSliceMap in this.RasterDatabase)
                    {
                        string currentArray = arrayNameToSliceMap.Key;

                        if (!arrayPriority.Contains(currentArray) && arraysRemoved < arraysToRemove)
                        {
                            int mbdCount = this.RasterDatabase[currentArray][currentSlice].Count;
                            Tuple<string, string, int> reductionEntry = new Tuple<string, string, int>(currentArray, currentSlice, mbdCount);
                            this.MaxArrayReductionsDatalog.Add(reductionEntry);
                            this.RasterDatabase[currentArray].Remove(currentSlice);
                            arraysRemoved++;
                        }
                    }
                }
            }
        }

        private List<string> GetArrayPriority()
        {
            return this.ReductionParameters.ArrayPriority;
        }

        private int GetArrayMaxCount()
        {
            return this.ReductionParameters.ArrayMaxCount;
        }

        private int GetMafLimit()
        {
            return this.ReductionParameters.ArrayMAFMax;
        }

        /// <summary>
        /// Reduces the # of slices per array within the RasterMap; records reductions for later logging.
        /// </summary>
        private void ReduceSlicesPerArray()
        {
            int maxCoresCount = this.GetMaxCoresCount();

            foreach (var arrayNameToSliceMap in this.RasterDatabase)
            {
                Dictionary<string, int> sliceToTotalMBDs = new Dictionary<string, int>();
                string currentArrayName = arrayNameToSliceMap.Key;

                // Sort slices by number of failures
                foreach (var sliceIdToMBDs in arrayNameToSliceMap.Value)
                {
                    string sliceId = sliceIdToMBDs.Key;
                    int numOfFails = sliceIdToMBDs.Value.Count;

                    if (!sliceToTotalMBDs.ContainsKey(sliceId))
                    {
                        sliceToTotalMBDs.Add(sliceId, 0);
                    }

                    sliceToTotalMBDs[sliceId] += numOfFails;
                }

                // sort dict based on values
                var sortedByMBDs = from entry in sliceToTotalMBDs orderby entry.Value ascending select entry;
                int totalSlices = sliceToTotalMBDs.Keys.Count;
                int neededSlices = Math.Min(maxCoresCount, totalSlices);

                if (neededSlices == totalSlices)
                {
                    Prime.Services.ConsoleService.PrintDebug($"No core/slice reductions needed for [{currentArrayName}], moving onto next array...");
                    continue;
                }
                else
                {
                    int slicesToRemove = totalSlices - maxCoresCount;
                    int removedSlices = 0;
                    Prime.Services.ConsoleService.PrintDebug($"Max cores/slices for array [{currentArrayName}] exceeded by [{slicesToRemove}] cores. Beginning reduction...");

                    // Due to sorting earlier, we remove slices that have least num of arrays first.
                    foreach (var sliceToMBDCount in sortedByMBDs)
                    {
                        if (removedSlices < slicesToRemove)
                        {
                            string sliceId = sliceToMBDCount.Key;
                            int numOfFailingMBD = this.RasterDatabase[currentArrayName][sliceId].Count;
                            Tuple<string, string, int> reductionEntry = new Tuple<string, string, int>(currentArrayName, sliceId, numOfFailingMBD);
                            this.CoreReductionsDatalog.Add(reductionEntry);
                            this.RasterDatabase[currentArrayName].Remove(sliceId);

                            removedSlices++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        private int GetMaxCoresCount()
        {
            return this.ReductionParameters.MaxCoresCount;
        }

        private void TranslateIntoRasterMap()
        {
            // Translation of Map(RootKey ->Map(ArrayName -> set of MBD values)) to Mapping of Map(Array Name -> Map(RootKey -> set of MBD values)). Makes it easier for Parallel algorithm to do this
            // What the rootKey is depends on product ex. BigCore uses sliceId, ATOM uses module
            var newRasterMap = new Dictionary<string, Dictionary<string, List<Tuple<int, int, int>>>>();

            foreach (var rootKeyToArrayDB in this.RasterDatabase)
            {
                string rootKey = rootKeyToArrayDB.Key;

                foreach (var arrayNameToMBDAddressDB in rootKeyToArrayDB.Value)
                {
                    string arrayName = arrayNameToMBDAddressDB.Key;

                    if (!newRasterMap.ContainsKey(arrayName))
                    {
                        newRasterMap.Add(arrayName, new Dictionary<string, List<Tuple<int, int, int>>>());
                    }

                    if (!newRasterMap[arrayName].ContainsKey(rootKey))
                    {
                        newRasterMap[arrayName].Add(rootKey, new List<Tuple<int, int, int>>());
                    }

                    foreach (var fail in arrayNameToMBDAddressDB.Value)
                    {
                        newRasterMap[arrayName][rootKey].Add(fail);
                    }
                }
            }

            // Overwrite with new format
            this.RasterDatabase = newRasterMap;
        }
    }
}
