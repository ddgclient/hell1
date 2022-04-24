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

namespace VminForwardingBase.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.SharedStorageService;
    using Prime.VminForwardingService;

    /// <summary>
    /// Defines the <see cref="VminForwardingFactory_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class VminForwardingFactory_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VminForwardingFactory_UnitTest"/> class.
        /// </summary>
        public VminForwardingFactory_UnitTest()
        {
            // Default Mock for Console service.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) => Console.WriteLine(msg));
            consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string d, string file) => Console.WriteLine("ERROR:" + msg));
            Prime.Services.ConsoleService = consoleServiceMock.Object;
        }

        /// <summary>
        /// Test the ability to do snapshot and extract the data..
        /// </summary>
        [TestMethod]
        public void VminSnapshot_Pass()
        {
            var recordData = new VminForwardingCornerData();
            recordData.Flow = 1;
            recordData.Frequency = 100;
            recordData.Voltage = 1.0;

            Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>> processedData = new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>
            {
                { "CR", new Dictionary<string, List<VminForwardingCornerRecord>>() },
            };

            foreach (var corner in new List<string> { "CR0@F1", "CR1@F1", "CR2@F1", "CR3@F1" })
            {
                var record = new VminForwardingCornerRecord();
                record.Key = corner;
                record.ActiveCornerData = recordData;
                record.CornerData = new List<VminForwardingCornerData> { recordData };
                processedData["CR"][corner.Split('@').First()] = new List<VminForwardingCornerRecord> { record };
            }

            var vminForwardingExportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            vminForwardingExportHandlerMock.Setup(o => o.GetProcessedCornersData()).Returns(processedData);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(vminForwardingExportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            foreach (var corner in new List<string> { "CR0@F1", "CR1@F1", "CR2@F1", "CR3@F1" })
            {
                var name = DDG.VminForwarding.Globals.VminForwardingSnapshot + DDG.VminForwarding.Globals.NameSeparator + corner;
                var context = DDG.VminForwarding.Globals.VminForwardingSnapshotContext;
                sharedStorageServiceMock.Setup(o => o.InsertRowAtTable(name, It.Is<VminForwardingCornerData>(actual => actual.Flow == 1 && actual.Frequency == 100 && actual.Voltage == 1), context));
            }

            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // Run the test...
            DDG.VminForwarding.Service.SaveVminForwardingSnapshot(new List<string> { "CR" });

            // verify mocks
            sharedStorageServiceMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
            vminForwardingExportHandlerMock.VerifyAll();
        }

        /// <summary>
        /// Test the ability to do snapshot and extract the data..
        /// </summary>
        [TestMethod]
        public void VminSnapshot_Fail()
        {
            Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>> processedData = new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>
            {
                { "CR", new Dictionary<string, List<VminForwardingCornerRecord>>() },
                { "CLR", new Dictionary<string, List<VminForwardingCornerRecord>>() },
            };

            var vminForwardingExportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            vminForwardingExportHandlerMock.Setup(o => o.GetProcessedCornersData()).Returns(processedData);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(vminForwardingExportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            // Run the test...
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => DDG.VminForwarding.Service.SaveVminForwardingSnapshot(new List<string> { "GTS" }));
            Assert.AreEqual("No Domain=[GTS] found in Prime ExportHandler. Contents=[CLR,CR]", ex.Message);

            // verify mocks
            vminForwardingServiceMock.VerifyAll();
            vminForwardingExportHandlerMock.VerifyAll();
        }

        /// <summary>
        /// Test the ability to do snapshot and extract the data..
        /// </summary>
        [TestMethod]
        public void GetVminSnapshot_Pass()
        {
            var name = DDG.VminForwarding.Globals.VminForwardingSnapshot + DDG.VminForwarding.Globals.NameSeparator + "CR2@F5";
            var context = DDG.VminForwarding.Globals.VminForwardingSnapshotContext;

            var data = new VminForwardingCornerData();
            data.Flow = 1;
            data.Frequency = 1;
            data.Voltage = 0.95;

            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInObjectTable(name, context)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.GetRowFromTable(name, typeof(VminForwardingCornerData), context)).Returns(data);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // Run the test...
            Assert.AreEqual(0.95, DDG.VminForwarding.Service.GetVminForwardingSnapshot("CR2@F5").Voltage);

            // verify mocks
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the ability to do snapshot and extract the data..
        /// </summary>
        [TestMethod]
        public void GetVminSnapshot_Fail()
        {
            var name = DDG.VminForwarding.Globals.VminForwardingSnapshot + DDG.VminForwarding.Globals.NameSeparator + "CR2@F5";
            var context = DDG.VminForwarding.Globals.VminForwardingSnapshotContext;
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageServiceMock.Setup(o => o.KeyExistsInObjectTable(name, context)).Returns(false);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;

            // Run the test...
            Assert.IsNull(DDG.VminForwarding.Service.GetVminForwardingSnapshot("CR2@F5"));

            // verify mocks
            sharedStorageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the ability to do basic storing and extracting of vmin data.
        /// </summary>
        [TestMethod]
        public void StoreAndExtractData_Pass()
        {
            // setup the mocks
            var vminHandlerF1Mock = new Mock<IVminForwardingHandler>(MockBehavior.Strict);
            vminHandlerF1Mock.Setup(o => o.StoreVoltages(new List<double> { 0.81, 0.82, 0.83, 0.84 })).Returns(true);
            vminHandlerF1Mock.Setup(o => o.GetSourceVoltages(new List<double> { 0.5, 0.5, 1.0, 1.01 })).Returns(new List<double> { 0.81, 0.82, 1.0, 1.01 });

            var vminHandlerF2Mock = new Mock<IVminForwardingHandler>(MockBehavior.Strict);
            vminHandlerF2Mock.Setup(o => o.GetSourceVoltages(new List<double> { 1.0, 1.01, 0.5, 0.5 })).Returns(new List<double> { 1.0, 1.01, 0.83, 0.84 });
            vminHandlerF2Mock.Setup(o => o.GetSourceVoltages(new List<double> { 1.01, 1.01, 1.01, 1.01 })).Returns(new List<double> { 1.01, 1.01, 1.01, 1.01 });

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateHandler(new List<string> { "CR0@F1", "CR1@F1", "CR2@F1", "CR3@F1" }, 1, false)).Returns(vminHandlerF1Mock.Object);
            vminForwardingServiceMock.Setup(o => o.CreateHandler(new List<string> { "CR0@F2", "CR1@F2", "CR2@F2", "CR3@F2" }, 1, false)).Returns(vminHandlerF2Mock.Object);
            var sharedStorageMock = this.MockFlags(ref vminForwardingServiceMock, useLimitCheckAsSource: false, useVoltageSources: true, useLimitCheck: false, storeVoltages: true);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            // run the test
            var f1CornerForCores = DDG.VminForwarding.Service.Get("CR0@F1,CR1@F1,CR2@F1,CR3@F1", 1);
            f1CornerForCores.StoreVminResult(new List<double> { 0.81, 0.82, 0.83, 0.84 });
            CollectionAssert.AreEqual(new List<double> { 0.81, 0.82, 1.0, 1.01 }, f1CornerForCores.GetStartingVoltage(new List<double> { 0.5, 0.5, 1.0, 1.01 }), "Mismatch on F1 Starting voltages");

            var f2CornerForCores = DDG.VminForwarding.Service.Get("CR0,CR1,CR2,CR3", "F2", 1);
            CollectionAssert.AreEqual(new List<double> { 1.0, 1.01, 0.83, 0.84 }, f2CornerForCores.GetStartingVoltage(new List<double> { 1.0, 1.01, 0.5, 0.5 }), "Mismatch on F2 Starting voltages");
            Assert.AreEqual(1.01, f2CornerForCores.GetStartingVoltage(1.01), "Mismatch on F2 Starting voltages (single element)");
            CollectionAssert.AreEqual(new List<double> { 1.01, 1.01, 1.01, 1.01 }, f2CornerForCores.GetStartingVoltage(new List<double> { 1.01 }), "Mismatch on F2 Starting voltages (list one element)");

            // verify mocks
            vminForwardingServiceMock.VerifyAll();
            vminHandlerF1Mock.VerifyAll();
            vminHandlerF2Mock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Test wrapper function.
        /// </summary>
        [TestMethod]
        public void GetAllDomainNames_Pass()
        {
            var domains = new List<string> { "CR", "CLR", "GTS" };

            var vminConfigHandlerMock = new Mock<IVminForwardingConfigurationHandler>(MockBehavior.Strict);
            vminConfigHandlerMock.Setup(o => o.GetDomainNames()).Returns(domains);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateConfigurationHandler()).Returns(vminConfigHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            Assert.AreEqual(domains, DDG.VminForwarding.Service.GetAllDomainNames());
            vminConfigHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test wrapper function.
        /// </summary>
        [TestMethod]
        public void GetInstanceNamesForDomain_Pass()
        {
            var instances = new List<string> { "CR0", "CR1", "CR2", "CR3" };

            var vminConfigHandlerMock = new Mock<IVminForwardingConfigurationHandler>(MockBehavior.Strict);
            vminConfigHandlerMock.Setup(o => o.GetInstanceNames("CR")).Returns(instances);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateConfigurationHandler()).Returns(vminConfigHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            Assert.AreEqual(instances, DDG.VminForwarding.Service.GetInstanceNamesForDomain("CR"));
            vminConfigHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test wrapper function.
        /// </summary>
        [TestMethod]
        public void GetCornerNamesForDomainInstance_Pass()
        {
            var corners = new List<string> { "CR0@F1", "CR0@F2", "CR0@F3", "CR0@F5" };

            var vminConfigHandlerMock = new Mock<IVminForwardingConfigurationHandler>(MockBehavior.Strict);
            vminConfigHandlerMock.Setup(o => o.GetsCornerNames("CR0")).Returns(corners);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateConfigurationHandler()).Returns(vminConfigHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            Assert.AreEqual(corners, DDG.VminForwarding.Service.GetCornerNamesForDomainInstance("CR0"));
            vminConfigHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test wrapper function.
        /// </summary>
        [TestMethod]
        public void GetFrequency_Pass()
        {
            var vminHandlerMock = new Mock<IVminForwardingHandler>(MockBehavior.Strict);
            vminHandlerMock.Setup(o => o.GetFrequencySourceValue("CR0@F1")).Returns(200);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateHandler(new List<string> { "CR0@F1" }, 3, false)).Returns(vminHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            Assert.AreEqual(200, DDG.VminForwarding.Service.GetFrequency("CR0@F1", 3));
            vminHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test wrapper function.
        /// </summary>
        [TestMethod]
        public void StoreVminResult_Pass()
        {
            var vminHandlerMock = new Mock<IVminForwardingHandler>(MockBehavior.Strict);
            vminHandlerMock.Setup(o => o.StoreVoltages(new List<double> { 1.4 })).Returns(true);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateHandler(new List<string> { "CR0@F1" }, 3, false)).Returns(vminHandlerMock.Object);
            var sharedStorageMock = this.MockFlags(ref vminForwardingServiceMock, useLimitCheckAsSource: false, useVoltageSources: true, useLimitCheck: false, storeVoltages: true);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var vminHandler = DDG.VminForwarding.Service.Get("CR0@F1", 3);
            Assert.IsTrue(vminHandler.StoreVminResult(1.4));

            vminHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Test wrapper function.
        /// </summary>
        [TestMethod]
        public void StoreVminResult_StoreVoltagesFalse_Pass()
        {
            var vminHandlerMock = new Mock<IVminForwardingHandler>(MockBehavior.Strict);
            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateHandler(new List<string> { "CR0@F1" }, 3, false)).Returns(vminHandlerMock.Object);
            var sharedStorageMock = this.MockFlags(ref vminForwardingServiceMock, useLimitCheckAsSource: true, useVoltageSources: false, useLimitCheck: true, storeVoltages: false);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var vminHandler = DDG.VminForwarding.Service.Get("CR0@F1", 3);
            Assert.IsTrue(vminHandler.StoreVminResult(1.4));

            vminHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Test wrapper function.
        /// </summary>
        [TestMethod]
        public void StoreVminResult_StoreVoltagesTrue_Pass()
        {
            var vminHandlerMock = new Mock<IVminForwardingHandler>(MockBehavior.Strict);
            vminHandlerMock.Setup(o => o.StoreVoltages(new List<double> { 1.4 })).Returns(true);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateHandler(new List<string> { "CR0@F1" }, 3, false)).Returns(vminHandlerMock.Object);
            var sharedStorageMock = this.MockFlags(ref vminForwardingServiceMock, useLimitCheckAsSource: false, useVoltageSources: true, useLimitCheck: false, storeVoltages: true);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var vminHandler = DDG.VminForwarding.Service.Get("CR0@F1", 3);
            Assert.IsTrue(vminHandler.StoreVminResult(1.4));

            vminHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Test wrapper function.
        /// </summary>
        [TestMethod]
        public void StoreVminResult_Fail_OutofRange()
        {
            var vminHandlerMock = new Mock<IVminForwardingHandler>(MockBehavior.Strict);
            vminHandlerMock.Setup(o => o.StoreVoltages(new List<double> { 1.4 })).Returns(false);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateHandler(new List<string> { "CR0@F1" }, 3, false)).Returns(vminHandlerMock.Object);
            var sharedStorageMock = this.MockFlags(ref vminForwardingServiceMock, useLimitCheckAsSource: false, useVoltageSources: true, useLimitCheck: false, storeVoltages: true);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var vminHandler = DDG.VminForwarding.Service.Get("CR0@F1", 3);
            Assert.IsFalse(vminHandler.StoreVminResult(1.4));

            vminHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Test wrapper function.
        /// </summary>
        [TestMethod]
        public void StoreVminResult_Fail()
        {
            var vminHandlerMock = new Mock<IVminForwardingHandler>(MockBehavior.Strict);
            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateHandler(new List<string> { "CR0@F1", "CR1@F1" }, 3, false)).Returns(vminHandlerMock.Object);
            var sharedStorageMock = this.MockFlags(ref vminForwardingServiceMock, useLimitCheckAsSource: false, useVoltageSources: true, useLimitCheck: false, storeVoltages: true);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var vminHandler = DDG.VminForwarding.Service.Get("CR0@F1, CR1@F1", 3);
            var ex = Assert.ThrowsException<ArgumentException>(() => vminHandler.StoreVminResult(new List<double> { 1.0, 1.1, 1.2 }));
            Assert.AreEqual("Number of vmins [3] does not match the number of Corners [2].\r\nParameter name: vmins", ex.Message);

            vminHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Test wrapper function.
        /// </summary>
        [TestMethod]
        public void GetStartingVoltage_Fail()
        {
            var vminHandlerMock = new Mock<IVminForwardingHandler>(MockBehavior.Strict);
            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateHandler(new List<string> { "CR0@F1", "CR1@F1" }, 3, false)).Returns(vminHandlerMock.Object);
            var sharedStorageMock = this.MockFlags(ref vminForwardingServiceMock, useLimitCheckAsSource: false, useVoltageSources: true, useLimitCheck: false, storeVoltages: true);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var vminHandler = DDG.VminForwarding.Service.Get("CR0@F1, CR1@F1", 3);
            var ex = Assert.ThrowsException<ArgumentException>(() => vminHandler.GetStartingVoltage(new List<double> { 1.0, 1.1, 1.2 }));
            Assert.AreEqual("Number of voltages [3] does not match the number of Corners [2].\r\nParameter name: startVoltagesFromParameter", ex.Message);

            vminHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Test wrapper function.
        /// </summary>
        [TestMethod]
        public void Constructor_BaseNotRun_Exception()
        {
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.GetIntegerRowFromTable(VminForwarding.Globals.VminForwardingUseLimitCheckAsSourceEnable, VminForwarding.Globals.VminForwardingFlagContext))
                .Throws(new Prime.Base.Exceptions.FatalException("Object not found"));
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.FatalException>(() => DDG.VminForwarding.Service.Get("CR0@F1, CR1@F1", 3));
            Assert.AreEqual("Object not found", ex.Message);

            vminForwardingServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        private Mock<ISharedStorageService> MockFlags(ref Mock<IVminForwardingService> vminServiceMock, bool useLimitCheckAsSource, bool useVoltageSources, bool useLimitCheck, bool storeVoltages)
        {
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.GetIntegerRowFromTable(VminForwarding.Globals.VminForwardingUseLimitCheckAsSourceEnable, VminForwarding.Globals.VminForwardingFlagContext))
                .Returns(useLimitCheckAsSource ? 1 : 0);
            sharedStorageMock.Setup(o => o.GetIntegerRowFromTable(VminForwarding.Globals.VminForwardingUseVoltagesSourcesEnable, DDG.VminForwarding.Globals.VminForwardingFlagContext))
                .Returns(useVoltageSources ? 1 : 0);
            sharedStorageMock.Setup(o => o.GetIntegerRowFromTable(VminForwarding.Globals.VminForwardingUseLimitCheckEnable, DDG.VminForwarding.Globals.VminForwardingFlagContext))
                .Returns(useLimitCheck ? 1 : 0);
            sharedStorageMock.Setup(o => o.GetIntegerRowFromTable(VminForwarding.Globals.VminForwardingStoreVoltagesEnable, DDG.VminForwarding.Globals.VminForwardingFlagContext))
                .Returns(storeVoltages ? 1 : 0);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            vminServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseLimitCheckAsSource, useLimitCheckAsSource));
            vminServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseVoltagesSources, useVoltageSources));
            vminServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseLimitCheck, useLimitCheck));
            vminServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.StoreVoltages, storeVoltages));

            return sharedStorageMock;
        }
    }
}
