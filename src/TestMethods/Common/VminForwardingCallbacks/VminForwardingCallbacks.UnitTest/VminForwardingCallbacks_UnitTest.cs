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

namespace VminForwardingCallbacks.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.DffService;
    using Prime.SharedStorageService;
    using Prime.VminForwardingService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class VminForwardingCallbacks_UnitTest
    {
        /// <summary>
        /// Setup any common mocks for all the tests.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            // mock the console service.
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;
        }

        /// <summary>
        /// Cleanup any common mocks for all the tests.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            DDG.VminForwarding.Service = new VminForwardingBase.VminForwardingFactory();
        }

        /// <summary>
        /// test the fail conditions for VminSearchStore.
        /// </summary>
        [TestMethod]
        public void VminSearchStore_Fail()
        {
            Assert.ThrowsException<ArgumentException>(() => VminForwardingCallbacks.VminSearchStore("--not_an_argument"));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void VminSearchStore_Pass()
        {
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;
            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            vminFactoryMock.Setup(o => o.GetAllDomainNames()).Returns(new List<string> { "CLR", "GTS" });
            vminFactoryMock.Setup(o => o.SaveVminForwardingSnapshot(new List<string> { "CLR" }));
            vminFactoryMock.Setup(o => o.SaveVminForwardingSnapshot(new List<string> { "CLR", "GTS" }));
            DDG.VminForwarding.Service = vminFactoryMock.Object;

            // run the test with an argument.
            VminForwardingCallbacks.VminSearchStore("--domains CLR");
            vminFactoryMock.Verify(o => o.GetAllDomainNames(), Times.Never);
            vminFactoryMock.Verify(o => o.SaveVminForwardingSnapshot(new List<string> { "CLR" }), Times.Once);

            // run the test with no arguments (all domains)
            VminForwardingCallbacks.VminSearchStore(string.Empty);
            vminFactoryMock.Verify(o => o.GetAllDomainNames(), Times.Once);
            vminFactoryMock.Verify(o => o.SaveVminForwardingSnapshot(new List<string> { "CLR", "GTS" }), Times.Once);
        }

        /// <summary>
        /// test the fail conditions for VminSearchStore.
        /// </summary>
        [TestMethod]
        public void VminInterpolation_Fail()
        {
            Assert.ThrowsException<ArgumentException>(() => VminForwardingCallbacks.VminInterpolation(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => VminForwardingCallbacks.VminInterpolation("--domains blah"));
            Assert.ThrowsException<ArgumentException>(() => VminForwardingCallbacks.VminInterpolation("--not_an_argument"));
            Assert.ThrowsException<ArgumentException>(() => VminForwardingCallbacks.VminInterpolation("--domains CLR"));
            Assert.ThrowsException<ArgumentException>(() => VminForwardingCallbacks.VminInterpolation("--check_corners F1,F3,F6"));
            Assert.ThrowsException<ArgumentException>(() => VminForwardingCallbacks.VminInterpolation("--domains CLR --check_corners F1,F3,F6"));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void VminInterpolation_Pass()
        {
            // needed mocks
            //   var allInstanceNames = DDG.VminForwarding.Service.GetInstanceNamesForDomain(domain);
            //   var allCorners = DDG.VminForwarding.Service.GetCornerNamesForDomainInstance(instance).OrderBy(j => j).ToList();
            var exportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            exportHandlerMock.Setup(o => o.GetProcessedCornersData()).Returns(new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>());

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(exportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            vminFactoryMock.Setup(o => o.GetInstanceNamesForDomain("CLR")).Returns(new List<string> { "CLR" });
            vminFactoryMock.Setup(o => o.GetInstanceNamesForDomain("CLR2")).Returns(new List<string> { "CLR2" });
            vminFactoryMock.Setup(o => o.GetCornerNamesForDomainInstance("CLR")).Returns(new List<string> { "CLR@F6", "CLR@F5", "CLR@F4", "CLR@F3", "CLR@F2", "CLR@F1" });
            vminFactoryMock.Setup(o => o.GetCornerNamesForDomainInstance("CLR2")).Returns(new List<string> { "CLR2@F6", "CLR2@F5", "CLR2@F4", "CLR2@F3", "CLR2@F2", "CLR2@F1" });
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot(It.IsAny<string>())).Returns((VminForwardingCornerData)null);
            DDG.VminForwarding.Service = vminFactoryMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInIntegerTable("FlowVar", Context.DUT)).Returns(true);
            sharedStorageMock.Setup(o => o.GetIntegerRowFromTable("FlowVar", Context.DUT)).Returns(1);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            var strValWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Loose);
            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(strValWriterMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(strValWriterMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;

            // TODO: Make the prediction code mockable...

            // run the test.
            VminForwardingCallbacks.VminInterpolation("--domains CLR --check_corners F1 --flow G.U.I.FlowVar"); // only one corner, won't do anything

            VminForwardingCallbacks.VminInterpolation("--domains CLR --check_corners F1,F6 --flow G.U.I.FlowVar"); // two corners, one domain == 1 call

            VminForwardingCallbacks.VminInterpolation("--domains CLR --check_corners F1,F3,F6 --flow G.U.I.FlowVar"); // 3 corners, one domain == 2 calls

            VminForwardingCallbacks.VminInterpolation("--domains CLR,CLR2 --check_corners F1,F3,F6 --flow G.U.I.FlowVar"); // 3 corners, 2 domain == 4 calls

            exportHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
            vminFactoryMock.VerifyAll();
            vminFactoryMock.VerifyAll();
        }

        /// <summary>
        /// Test failing cases of LoadVminFromDFF.
        /// </summary>
        [TestMethod]
        public void LoadVminFromDFF_BadFormat_Fail()
        {
            var vminConfigHandlerMock = new Mock<IVminForwardingConfigurationHandler>(MockBehavior.Strict);
            vminConfigHandlerMock.Setup(o => o.GetDomainNames()).Returns(new List<string> { "CR" });
            vminConfigHandlerMock.Setup(o => o.GetInstanceNames("CR")).Returns(new List<string> { "CR0" });
            vminConfigHandlerMock.Setup(o => o.GetsCornerNames("CR0")).Returns(new List<string> { "CR0@F1" });
            vminConfigHandlerMock.Setup(o => o.GetSharedStorageLimitCheck("CR@F1")).Returns("LimitCheck_CR");

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateConfigurationHandler()).Returns(vminConfigHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var cornerMap = new Dictionary<string, string>
            {
                { "CR@F1", "Dummy:DieId3:OpType3:DffToken3" },
            };

            // Mock the SharedStorge writes.
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.GetRowFromTable(DDG.VminForwarding.Globals.VminForwardingDffMap, typeof(Dictionary<string, string>), DDG.VminForwarding.Globals.VminForwardingDffMapContext))
                .Returns(cornerMap);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // run the test.
            Assert.ThrowsException<TestMethodException>(() => VminForwardingCallbacks.LoadVminFromDFF(string.Empty));

            // verify the mocks.
            vminConfigHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Test passing cases of LoadVminFromDFF.
        /// </summary>
        [TestMethod]
        public void LoadVminFromDFF_BadFile_Fail()
        {
            var vminConfigHandlerMock = new Mock<IVminForwardingConfigurationHandler>(MockBehavior.Strict);
            vminConfigHandlerMock.Setup(o => o.GetDomainNames()).Returns(new List<string> { "CR", "CLR", "GTS" });
            vminConfigHandlerMock.Setup(o => o.GetInstanceNames("CR")).Returns(new List<string> { "CR0", "CR1" });
            vminConfigHandlerMock.Setup(o => o.GetInstanceNames("CLR")).Returns(new List<string> { "CLR" });
            vminConfigHandlerMock.Setup(o => o.GetInstanceNames("GTS")).Returns(new List<string> { "GTS" });
            vminConfigHandlerMock.Setup(o => o.GetsCornerNames("CR0")).Returns(new List<string> { "CR0@F1" });
            vminConfigHandlerMock.Setup(o => o.GetsCornerNames("CLR")).Returns(new List<string> { "CLR@F2" });
            vminConfigHandlerMock.Setup(o => o.GetsCornerNames("GTS")).Returns(new List<string> { "GTS@F3" });
            vminConfigHandlerMock.Setup(o => o.GetSharedStorageLimitCheck("CLR@F2")).Returns("LimitCheck_CLR");
            vminConfigHandlerMock.Setup(o => o.GetSharedStorageLimitCheck("GTS@F3")).Returns(string.Empty);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateConfigurationHandler()).Returns(vminConfigHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var cornerMap = new Dictionary<string, string>
            {
                { "CLR@F2", "OpType2.DffToken2" },
                { "GTS@F3", "DieId3:OpType3:DffToken3" },
            };

            // Mock the SharedStorge writes.
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.GetRowFromTable(DDG.VminForwarding.Globals.VminForwardingDffMap, typeof(Dictionary<string, string>), DDG.VminForwarding.Globals.VminForwardingDffMapContext))
                .Returns(cornerMap);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // run the test.
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => VminForwardingCallbacks.LoadVminFromDFF(string.Empty));
            Assert.AreEqual("Errors mapping VminCorners to DFF/GSDS using ConfigFile.", ex.Message);

            // verify the mocks.
            vminConfigHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Test passing cases of LoadVminFromDFF.
        /// </summary>
        [TestMethod]
        public void LoadVminFromDFF_Pass()
        {
            var vminConfigHandlerMock = new Mock<IVminForwardingConfigurationHandler>(MockBehavior.Strict);
            vminConfigHandlerMock.Setup(o => o.GetDomainNames()).Returns(new List<string> { "CR", "CLR", "GTS" });
            vminConfigHandlerMock.Setup(o => o.GetInstanceNames("CR")).Returns(new List<string> { "CR0", "CR1" });
            vminConfigHandlerMock.Setup(o => o.GetInstanceNames("CLR")).Returns(new List<string> { "CLR" });
            vminConfigHandlerMock.Setup(o => o.GetInstanceNames("GTS")).Returns(new List<string> { "GTS" });
            vminConfigHandlerMock.Setup(o => o.GetsCornerNames("CR0")).Returns(new List<string> { "CR0@F1" });
            vminConfigHandlerMock.Setup(o => o.GetsCornerNames("CLR")).Returns(new List<string> { "CLR@F2" });
            vminConfigHandlerMock.Setup(o => o.GetsCornerNames("GTS")).Returns(new List<string> { "GTS@F3" });
            vminConfigHandlerMock.Setup(o => o.GetSharedStorageLimitCheck("CR@F1")).Returns("LimitCheck_CR");
            vminConfigHandlerMock.Setup(o => o.GetSharedStorageLimitCheck("CLR@F2")).Returns("LimitCheck_CLR");
            vminConfigHandlerMock.Setup(o => o.GetSharedStorageLimitCheck("GTS@F3")).Returns("LimitCheck_GTS");

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateConfigurationHandler()).Returns(vminConfigHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var cornerMap = new Dictionary<string, string>
            {
                { "CR@F1", "DffToken1" },
                { "CLR@F2", "OpType2:DffToken2" },
                { "GTS@F3", "DieId3:OpType3:DffToken3" },
            };

            // Mock DFF values.
            var dffMock = new Mock<IDffService>(MockBehavior.Strict);
            dffMock.Setup(o => o.GetDff("DffToken1", true)).Returns("|0.530|0.530|0.530");
            dffMock.Setup(o => o.GetDffByOpType("DffToken2", "OpType2", true)).Returns("||0.654v0.656v0.649v0.654v-9999v-9999v-9999v-9999|0.654v0.656v0.649v0.654v-9999v-9999v-9999v-9999");
            dffMock.Setup(o => o.GetDff("DffToken3", "OpType3", "DieId3", true)).Returns("1.30");
            Prime.Services.DffService = dffMock.Object;

            // Mock the SharedStorge writes.
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.InsertRowAtTable("LimitCheck_CR", "-9999|0.530|0.530|0.530", Context.DUT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("LimitCheck_CLR", "-9999||0.654,0.656,0.649,0.654,-9999,-9999,-9999,-9999|0.654,0.656,0.649,0.654,-9999,-9999,-9999,-9999", Context.DUT));
            sharedStorageMock.Setup(o => o.InsertRowAtTable("LimitCheck_GTS", "1.30", Context.DUT));

            sharedStorageMock.Setup(o => o.GetRowFromTable(DDG.VminForwarding.Globals.VminForwardingDffMap, typeof(Dictionary<string, string>), DDG.VminForwarding.Globals.VminForwardingDffMapContext))
                .Returns(cornerMap);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // run the test.
            VminForwardingCallbacks.LoadVminFromDFF(string.Empty);

            // verify the mocks.
            vminConfigHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
            dffMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }
    }
}
