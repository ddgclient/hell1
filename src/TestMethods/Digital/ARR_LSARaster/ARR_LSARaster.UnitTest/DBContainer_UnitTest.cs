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
namespace LSARasterTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Unit test class for <see cref="DBContainer"/>.
    /// </summary>
    [TestClass]
    public class DBContainer_UnitTest
    {
        private Mock<IConsoleService> mockConsole = new Mock<IConsoleService>();

        private RasterConfig deserializedRasterRC;
        private string validInputWithReductionConfig = File.ReadAllText(".\\TestInput\\RasterConfig_ReductionConfigSet.json");

        /// <summary>
        /// Init for this test class.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            this.mockConsole.Setup(x => x.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, int, string, string>((string msg, int line, string n, string src) => { Console.WriteLine($"ERROR: {msg}"); });
            this.mockConsole
                .Setup(x => x.PrintDebug(It.IsAny<string>()))
                .Callback<string>((string msg) => { Console.WriteLine(msg); });
            Prime.Services.ConsoleService = this.mockConsole.Object;

            var serializedRasterRC = new JsonInput(this.validInputWithReductionConfig);
            this.deserializedRasterRC = serializedRasterRC.DeserializeInput<RasterConfig>();
        }

        /// <summary>
        /// Testing for if we're able to retrieve a database created by a previous prescreen instance.
        /// </summary>
        [TestMethod]
        public void GetDBFromStorage_SuccessfulReturn()
        {
            Dictionary<string, Dictionary<string, List<string>>> sharedStorageDB = new Dictionary<string, Dictionary<string, List<string>>>();
            sharedStorageDB.Add("1", new Dictionary<string, List<string>>()
            {
                { "bpu_bme", new List<string>() { "0,0,0", "1,0,0" } },
                { "bpu_trel", new List<string>() { "0,0,0" } },
            });

            Mock<ISharedStorageService> mockSharedStorage = new Mock<ISharedStorageService>();
            mockSharedStorage
                .Setup(x => x.GetRowFromTable("fakeMap", typeof(Dictionary<string, Dictionary<string, List<string>>>), Context.DUT))
                .Returns(sharedStorageDB);
            Prime.Services.SharedStorageService = mockSharedStorage.Object;
            var rasterContainer = DBContainer.GetDBFromStorage("fakeMap", null);
            var convertedDB = rasterContainer.RasterDatabase;
            Assert.IsTrue(convertedDB["1"]["bpu_bme"].Contains(new Tuple<int, int, int>(0, 0, 0))
                && convertedDB["1"]["bpu_bme"].Contains(new Tuple<int, int, int>(1, 0, 0))
                && convertedDB["1"]["bpu_trel"].Contains(new Tuple<int, int, int>(0, 0, 0)));
        }

        /// <summary>
        /// Fails when we use an invalid/missing key from the database.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(System.Exception))] // FIXME: need more specific exception...
        public void GetDBFromStorage_InvalidKey()
        {
            Mock<ISharedStorageService> mockSharedStorage = new Mock<ISharedStorageService>();
            mockSharedStorage
                .Setup(x => x.GetRowFromTable("fakeMap", typeof(Dictionary<string, Dictionary<string, List<string>>>), Context.DUT))
                .Throws(new Exception());
            Prime.Services.SharedStorageService = mockSharedStorage.Object;
            var container = DBContainer.GetDBFromStorage("fakeMap", null);
        }

        /// <summary>
        /// Fails when we use an invalid/missing key from the database.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))] // FIXME: need more specific exception...
        public void GetDBFromStorage_EmptyDatabase()
        {
            Mock<ISharedStorageService> mockSharedStorage = new Mock<ISharedStorageService>();
            mockSharedStorage
                .Setup(x => x.GetRowFromTable("fakeMap", typeof(Dictionary<string, Dictionary<string, List<string>>>), Context.DUT))
                .Returns(new Dictionary<string, Dictionary<string, List<string>>>());
            Prime.Services.SharedStorageService = mockSharedStorage.Object;
            var container = DBContainer.GetDBFromStorage("fakeMap", null);
        }

        /// <summary>
        /// Confirm we can retrieve a database stored from prescreen for raster.
        /// </summary>
        [TestMethod]
        public void StoreDBInStorage()
        {
            object fakeStorage = null;

            Mock<ISharedStorageService> mockSharedStorage = new Mock<ISharedStorageService>();

            mockSharedStorage
                .Setup(x => x.InsertRowAtTable("fakeMap", It.IsAny<object>(), Context.DUT))
                .Callback<string, object, Context>((string key, object data, Context context) => { fakeStorage = data; });
            Prime.Services.SharedStorageService = mockSharedStorage.Object;

            Dictionary<string, Dictionary<string, HashSet<Tuple<int, int, int>>>> prescreenDB = new Dictionary<string, Dictionary<string, HashSet<Tuple<int, int, int>>>>();
            prescreenDB.Add("1", new Dictionary<string, HashSet<Tuple<int, int, int>>>()
            {
                { "bpu_bme", new HashSet<Tuple<int, int, int>>() { new Tuple<int, int, int>(0, 0, 0), new Tuple<int, int, int>(1, 0, 0) } },
                { "bpu_trel", new HashSet<Tuple<int, int, int>>() { new Tuple<int, int, int>(0, 0, 0) } },
            });
            DBContainer container = new DBContainer();
            container.PrescreenDatabase = prescreenDB;
            container.StoreDBInStorage("fakeMap");

            mockSharedStorage
                .Setup(x => x.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>()))
                .Returns(fakeStorage);
            Prime.Services.SharedStorageService = mockSharedStorage.Object;

            var rasterContainer = DBContainer.GetDBFromStorage("fakeMap", null);
            var convertedDB = rasterContainer.RasterDatabase;
            Assert.IsTrue(convertedDB["1"]["bpu_bme"].Contains(new Tuple<int, int, int>(0, 0, 0))
                && convertedDB["1"]["bpu_bme"].Contains(new Tuple<int, int, int>(1, 0, 0))
                && convertedDB["1"]["bpu_trel"].Contains(new Tuple<int, int, int>(0, 0, 0)));
        }

        /// <summary>
        /// Confirm we can reduce the # of failng array per slice in the case of MAF(Mass Array Failure).
        /// </summary>
        [TestMethod]
        public void ReduceArraysPerSlice()
        {
            string simulationDB =
                "bpu_trel,1,0|1|0;bpu_bme,1,0|2|0;bpu_bme,1,0|0|0;bpu_bme,1,0|2|0;bpu_bme,1,1|0|0;bpu_bme,1,1|0|0;bpu_trol,1,0|0|0";
            DBContainer container = DBContainer.CreateDBFromString(
                simulationDB,
                MetadataConfig.ArrayType.BIGCORE,
                this.deserializedRasterRC.GetReductionConfigSet("Example_Set"));

            container.CreateRasterMap(MetadataConfig.ArrayType.BIGCORE);

            Assert.IsTrue(container.DoMaxArrayReductionsExist());
        }

        /// <summary>
        /// Confirm we can reduce the # of failng slices per array in the case of MAF(Mass Array Failure).
        /// </summary>
        public void ReduceSlicesPerArray()
        {
            Assert.IsTrue(false);
        }
    }
}
