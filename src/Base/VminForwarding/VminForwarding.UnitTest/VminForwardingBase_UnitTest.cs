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
    using System.IO;
    using System.Runtime.CompilerServices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.FileService;
    using Prime.SharedStorageService;
    using Prime.VminForwardingService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class VminForwardingBase_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VminForwardingBase_UnitTest"/> class.
        /// </summary>
        public VminForwardingBase_UnitTest()
        {
            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => Console.WriteLine(s));
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;
        }

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Verify_ConfigMode_False()
        {
            this.TestVerifyErrors(
                new VminForwardingBase
                {
                    Mode = VminForwardingBase.ExecuteMode.Configure,
                    UseDffAsSource = VminForwardingBase.MyBool.True,
                    UseLimitCheck = VminForwardingBase.MyBool.False,
                    DffMappingFile = "SomeFile",
                    DffMappingSet = "SomeSet",
                },
                "Parameters [DffMappingFile, DffMappingSet, DffMappingOptype] are required when Mode=[Configure] and UseDffAsSource=[True] or UseLimitCheck=[True].");

            this.TestVerifyErrors(
                new VminForwardingBase
                {
                    Mode = VminForwardingBase.ExecuteMode.Configure,
                    UseDffAsSource = VminForwardingBase.MyBool.False,
                    UseLimitCheck = VminForwardingBase.MyBool.True,
                    DffMappingFile = "SomeFile",
                    DffMappingOptype = "PBIC_DAB",
                },
                "Parameters [DffMappingFile, DffMappingSet, DffMappingOptype] are required when Mode=[Configure] and UseDffAsSource=[True] or UseLimitCheck=[True].");

            this.TestVerifyErrors(
                new VminForwardingBase
                {
                    Mode = VminForwardingBase.ExecuteMode.Configure,
                    UseDffAsSource = VminForwardingBase.MyBool.False,
                    UseLimitCheck = VminForwardingBase.MyBool.True,
                    DffMappingSet = "SomeSet",
                    DffMappingOptype = "PBIC_DAB",
                },
                "Parameters [DffMappingFile, DffMappingSet, DffMappingOptype] are required when Mode=[Configure] and UseDffAsSource=[True] or UseLimitCheck=[True].");

            this.TestVerifyErrors(
                new VminForwardingBase
                {
                    Mode = VminForwardingBase.ExecuteMode.Configure,
                    UseDffAsSource = VminForwardingBase.MyBool.True,
                    UseVoltagesSources = VminForwardingBase.MyBool.True,
                    DffMappingFile = "SomeFile",
                    DffMappingSet = "SomeSet",
                },
                "Parameters [UseDffAsSource] and [UseVoltagesSources] cannot both be True.");
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_ConfigNoDff_Pass()
        {
            // setup the mocks
            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseLimitCheckAsSource, false));
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseVoltagesSources, true));
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseLimitCheck, false));
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.StoreVoltages, true));
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.MockFlagSetAndGet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingSinglePointMode, 0);
            this.MockFlagSetAndGet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingSearchGuardbandEnable, 0);

            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingUseLimitCheckAsSourceEnable, 0);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingUseVoltagesSourcesEnable, 1);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingUseLimitCheckEnable, 0);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingStoreVoltagesEnable, 1);

            var tokenMap = new Dictionary<string, string>();
            sharedStorageMock.Setup(o => o.InsertRowAtTable(DDG.VminForwarding.Globals.VminForwardingDffMap, tokenMap, DDG.VminForwarding.Globals.VminForwardingDffMapContext));
            sharedStorageMock.Setup(o => o.OverrideObjectRowResetPolicy(DDG.VminForwarding.Globals.VminForwardingDffMap, ResetPolicy.NEVER_RESET, DDG.VminForwarding.Globals.VminForwardingDffMapContext));

            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // Create the template instance
            VminForwardingBase underTest = new VminForwardingBase
            {
                Mode = VminForwardingBase.ExecuteMode.Configure,
            };

            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            // check downstream socket mode and enable search buardband is false.
            Assert.IsFalse(DDG.VminForwarding.Service.IsSinglePointMode(), "IsSinglePointMode check failed");
            Assert.IsFalse(DDG.VminForwarding.Service.IsSearchGuardbandEnabled(), "IsSearchGuardbandEnabled check failed");

            // verify all mocks.
            vminForwardingServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_ConfigWithDff_Pass()
        {
            // setup the mocks
            var fileMock = new Mock<IFileService>(MockBehavior.Strict);
            fileMock.Setup(o => o.FileExists("DffMapping.json")).Returns(true);
            fileMock.Setup(o => o.GetFile("DffMapping.json")).Returns(GetPathToFiles() + "DffMapping.json");
            Prime.Services.FileService = fileMock.Object;

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseLimitCheckAsSource, true));
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseVoltagesSources, false));
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseLimitCheck, false));
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.StoreVoltages, false));
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.MockFlagSetAndGet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingSinglePointMode, 1);
            this.MockFlagSetAndGet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingSearchGuardbandEnable, 1);

            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingUseLimitCheckAsSourceEnable, 1);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingUseVoltagesSourcesEnable, 0);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingUseLimitCheckEnable, 0);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingStoreVoltagesEnable, 0);

            var expectedMap = new Dictionary<string, string>()
            {
                { "CR@F1", "PBIC_DAB:HF1CR" },
                { "CR@F2", "PBIC_DAB:HF2CR" },
                { "CR@F3", "PBIC_DAB:HF3CR" },
                { "CR@F4", "PBIC_DAB:HF4CR" },
                { "CR@F5", "PBIC_DAB:HF5CR" },
                { "CR@F6", "PBIC_DAB:HF6CR" },
                { "CR@F7", "PBIC_DAB:HF7CR" },
                { "CLR@F1", "PBIC_DAB:HF1CLR" },
                { "CLR@F2", "PBIC_DAB:HF2CLR" },
                { "CLR@F3", "PBIC_DAB:HF3CLR" },
                { "CLR@F4", "PBIC_DAB:HF4CLR" },
                { "CLR@F5", "PBIC_DAB:HF5CLR" },
                { "CLR@F6", "PBIC_DAB:HF6CLR" },
            };
            sharedStorageMock.Setup(o => o.InsertRowAtTable(DDG.VminForwarding.Globals.VminForwardingDffMap, expectedMap, DDG.VminForwarding.Globals.VminForwardingDffMapContext));
            sharedStorageMock.Setup(o => o.OverrideObjectRowResetPolicy(DDG.VminForwarding.Globals.VminForwardingDffMap, ResetPolicy.NEVER_RESET, DDG.VminForwarding.Globals.VminForwardingDffMapContext));
            sharedStorageMock.Setup(o => o.GetRowFromTable(DDG.VminForwarding.Globals.VminForwardingDffMap, typeof(Dictionary<string, string>), DDG.VminForwarding.Globals.VminForwardingDffMapContext))
                .Returns(expectedMap);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // Create the template instance
            VminForwardingBase underTest = new VminForwardingBase
            {
                Mode = VminForwardingBase.ExecuteMode.Configure,
                VminSinglePointMode = VminForwardingBase.MyBool.True,
                SearchGuardbandEnable = VminForwardingBase.MyBool.True,
                UseDffAsSource = VminForwardingBase.MyBool.True,
                UseVoltagesSources = VminForwardingBase.MyBool.False,
                StoreVoltages = VminForwardingBase.MyBool.False,
                DffMappingFile = "DffMapping.json",
                DffMappingSet = "vmin_dff_token",
                DffMappingOptype = "PBIC_DAB",
            };

            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            // check the DFF token map is correct.
            var map = DDG.VminForwarding.Service.GetDffTokenMap();
            Assert.AreEqual(expectedMap.Count, map.Count, "TokenMap has the wrong number of elements.");
            foreach (var item in expectedMap)
            {
                Assert.IsTrue(map.ContainsKey(item.Key), $"TokenMap does not contain [{item.Key}].");
                Assert.AreEqual(item.Value, map[item.Key], $"TokenMap Value does not match for key=[{item.Key}].");
            }

            // check downstream socket and SearchGuardband modes are true.
            Assert.IsTrue(DDG.VminForwarding.Service.IsSinglePointMode(), "IsSinglePointMode check failed");
            Assert.IsTrue(DDG.VminForwarding.Service.IsSearchGuardbandEnabled(), "IsSearchGuardbandEnabled check failed");

            // verify all mocks.
            vminForwardingServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            fileMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_ConfigWithDff_FailBadFile()
        {
            // setup the mocks
            var fileMock = new Mock<IFileService>(MockBehavior.Strict);
            fileMock.Setup(o => o.FileExists("DffMappingEmpty.json")).Returns(true);
            fileMock.Setup(o => o.GetFile("DffMappingEmpty.json")).Returns(GetPathToFiles() + "DffMappingEmpty.json");
            Prime.Services.FileService = fileMock.Object;

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseLimitCheckAsSource, true));
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseVoltagesSources, false));
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseLimitCheck, false));
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.StoreVoltages, true));
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingSinglePointMode, 1);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingSearchGuardbandEnable, 0);

            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingUseLimitCheckAsSourceEnable, 1);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingUseVoltagesSourcesEnable, 0);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingUseLimitCheckEnable, 0);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingStoreVoltagesEnable, 1);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // Create the template instance
            VminForwardingBase underTest = new VminForwardingBase
            {
                Mode = VminForwardingBase.ExecuteMode.Configure,
                VminSinglePointMode = VminForwardingBase.MyBool.True,
                SearchGuardbandEnable = VminForwardingBase.MyBool.False,
                UseDffAsSource = VminForwardingBase.MyBool.True,
                UseVoltagesSources = VminForwardingBase.MyBool.False,
                StoreVoltages = VminForwardingBase.MyBool.True,
                DffMappingFile = "DffMappingEmpty.json",
                DffMappingSet = "vmin_dff_token",
                DffMappingOptype = "PBIC_DAB",
            };

            underTest.Verify();
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.Execute());
            Assert.AreEqual($"DFF Mapping file=[DffMappingEmpty.json] is missing the Top-Level Token=[UpsDffMap].", ex.Message);

            // verify all mocks.
            vminForwardingServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            fileMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_ConfigWithDff_FailBadSet()
        {
            // setup the mocks
            var fileMock = new Mock<IFileService>(MockBehavior.Strict);
            fileMock.Setup(o => o.FileExists("DffMapping.json")).Returns(true);
            fileMock.Setup(o => o.GetFile("DffMapping.json")).Returns(GetPathToFiles() + "DffMapping.json");
            Prime.Services.FileService = fileMock.Object;

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseLimitCheckAsSource, true));
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseVoltagesSources, false));
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.UseLimitCheck, false));
            vminForwardingServiceMock.Setup(o => o.SetOperationModeFlag(OperationMode.StoreVoltages, true));
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingSinglePointMode, 1);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingSearchGuardbandEnable, 0);

            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingUseLimitCheckAsSourceEnable, 1);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingUseVoltagesSourcesEnable, 0);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingUseLimitCheckEnable, 0);
            this.MockFlagSet(ref sharedStorageMock, DDG.VminForwarding.Globals.VminForwardingStoreVoltagesEnable, 1);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            // Create the template instance
            VminForwardingBase underTest = new VminForwardingBase
            {
                Mode = VminForwardingBase.ExecuteMode.Configure,
                VminSinglePointMode = VminForwardingBase.MyBool.True,
                UseDffAsSource = VminForwardingBase.MyBool.True,
                UseVoltagesSources = VminForwardingBase.MyBool.False,
                DffMappingFile = "DffMapping.json",
                DffMappingSet = "badset",
                DffMappingOptype = "PBIC_DAB",
            };

            underTest.Verify();
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => underTest.Execute());
            Assert.AreEqual($"DFF Mapping file=[DffMapping.json] does not contain Set=[badset].", ex.Message);

            // verify all mocks.
            vminForwardingServiceMock.VerifyAll();
            sharedStorageMock.VerifyAll();
            fileMock.VerifyAll();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_DumpTables()
        {
            var clrF1ActiveData = CreateActiveVminRecord("CLR@F1", 1, 1000, 0.6);
            var clrF2ActiveData = CreateActiveVminRecord("CLR@F2", 2, 2000, 1.0);
            var clrF3ActiveData = CreateActiveVminRecord("CLR@F3", -1, -1, -9999);

            var data = new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>
            {
                { "CLR", new Dictionary<string, List<VminForwardingCornerRecord>> { { "CLR", new List<VminForwardingCornerRecord> { clrF1ActiveData, clrF2ActiveData, clrF3ActiveData } } } },
            };

            var exportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            exportHandlerMock.Setup(o => o.GetProcessedCornersData()).Returns(data);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(exportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var consoleMock = new Mock<IConsoleService>(MockBehavior.Loose);
            Prime.Services.ConsoleService = consoleMock.Object;

            VminForwardingBase underTest = new VminForwardingBase { Mode = VminForwardingBase.ExecuteMode.DumpTables };
            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            consoleMock.Verify(o => o.PrintDebug("\t\tRecord=CLR@F1 Active Data: Freq=[1000] Voltage=[0.6] Flow=[1]"), Times.Once);
            consoleMock.Verify(o => o.PrintDebug("\t\tRecord=CLR@F2 Active Data: Freq=[2000] Voltage=[1] Flow=[2]"), Times.Once);
            consoleMock.Verify(o => o.PrintDebug("\t\tRecord=CLR@F3 Active Data: Freq=[] Voltage=[] Flow=[]"), Times.Once);

            exportHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        private static string GetPathToFiles([CallerFilePath] string srcPath = "")
        {
            return Path.GetDirectoryName(srcPath) + "\\InputFiles\\";
        }

        private static VminForwardingCornerRecord CreateActiveVminRecord(string name, int flow, double freq, double voltage)
        {
            var activeData = new VminForwardingCornerRecord();
            activeData.Key = name;
            if (voltage > 0)
            {
                activeData.ActiveCornerData = new VminForwardingCornerData();
                activeData.ActiveCornerData.Flow = flow;
                activeData.ActiveCornerData.Frequency = freq;
                activeData.ActiveCornerData.Voltage = voltage;
            }
            else
            {
                activeData.ActiveCornerData = null;
            }

            return activeData;
        }

        private void TestVerifyErrors(VminForwardingBase underTest, string error)
        {
            underTest.Verify();
            this.ConsoleServiceMock.Verify(o => o.PrintError(error, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        private void MockFlagSetAndGet(ref Mock<ISharedStorageService> mock, string flag, int value)
        {
            this.MockFlagSet(ref mock, flag, value);
            mock.Setup(o => o.KeyExistsInIntegerTable(flag, DDG.VminForwarding.Globals.VminForwardingFlagContext)).Returns(true);
            mock.Setup(o => o.GetIntegerRowFromTable(flag, DDG.VminForwarding.Globals.VminForwardingFlagContext)).Returns(value);
        }

        private void MockFlagSet(ref Mock<ISharedStorageService> mock, string flag, int value)
        {
            mock.Setup(o => o.InsertRowAtTable(flag, value, DDG.VminForwarding.Globals.VminForwardingFlagContext));
            mock.Setup(o => o.OverrideIntegerRowResetPolicy(flag, ResetPolicy.NEVER_RESET, DDG.VminForwarding.Globals.VminForwardingFlagContext));
        }
    }
}
