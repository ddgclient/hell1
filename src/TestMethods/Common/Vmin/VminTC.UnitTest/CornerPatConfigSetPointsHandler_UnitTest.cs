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

namespace VminUtilities.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.PatConfigService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CornerPatConfigSetPointsHandler_UnitTest
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<ISharedStorageService> sharedStorageMock;

        /// <summary>
        /// Gets or sets mock shared storage.
        /// </summary>
        public Dictionary<string, string> SharedStorageValues { get; set; }

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(
                o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
            {
                System.Console.WriteLine($"DEBUG: {msg}");
            });

            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            // Default Mock for Shared service.
            this.SharedStorageValues = new Dictionary<string, string>();
            this.sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>()))
                .Callback((string key, object obj, Context context) =>
                {
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, string obj, Context context) =>
                {
                    this.SharedStorageValues[key] = obj;
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.sharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<Context>()))
                .Callback((string key, double obj, Context context) =>
                {
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.sharedStorageMock.Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>()))
                .Callback((string key, Type obj, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Type obj, Context context) => JsonConvert.DeserializeObject(this.SharedStorageValues[key], obj));
            this.sharedStorageMock.Setup(o => o.GetDoubleRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => double.Parse(this.SharedStorageValues[key]));
            this.sharedStorageMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => this.SharedStorageValues[key]);
            Prime.Services.SharedStorageService = this.sharedStorageMock.Object;
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void Constructor_invalidsetpoint_Fail()
        {
            this.sharedStorageMock.Setup(o => o.KeyExistsInObjectTable("cornerSetpoint", Context.DUT)).Returns(false);

            var ex = Assert.ThrowsException<ArgumentException>(() => new CornerPatConfigSetPointsHandler("patlist", "cornerSetpoint", new List<Tuple<string, int, IVminForwardingCorner>>()));
            Assert.AreEqual("Unable to find CornerPatConfigSetPoints=cornerSetpoint in SharedStorage.", ex.Message);
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void Run_Pass()
        {
            var vminForwardingFactoryMock = new Mock<IVminForwardingFactory>(MockBehavior.Strict);
            var vminForwardingMock = new Mock<IVminForwardingCorner>(MockBehavior.Strict);
            var tuple0 = new Tuple<string, int, IVminForwardingCorner>("C0", 1, vminForwardingMock.Object);
            var tuple1 = new Tuple<string, int, IVminForwardingCorner>("C1", 1, vminForwardingMock.Object);
            var tuple2 = new Tuple<string, int, IVminForwardingCorner>("C2", 1, vminForwardingMock.Object);
            var tuple3 = new Tuple<string, int, IVminForwardingCorner>("C3", 1, vminForwardingMock.Object);
            var vminForwarding = new List<Tuple<string, int, IVminForwardingCorner>> { tuple0, tuple1, tuple2, tuple3 };
            DDG.VminForwarding.Service = vminForwardingFactoryMock.Object;

            var patConfigSetPoint = new Mock<IPatConfigSetPointHandle>(MockBehavior.Strict);
            patConfigSetPoint.Setup(o => o.ApplySetPoint("SP1"));
            var patConfigService = new Mock<IPatConfigService>(MockBehavior.Strict);
            patConfigService.Setup(o => o.GetSetPointHandle("M0", "G0", "SomePatlist")).Returns(patConfigSetPoint.Object);
            Prime.Services.PatConfigService = patConfigService.Object;
            var setPoint1 = new PatConfigSetPoint()
            {
                Module = "M0",
                Group = "G0",
                SetPoint = "SP1",
                Condition = "'IamAlive' == 'IamAlive'",
            };

            var setPoint2 = new PatConfigSetPoint()
            {
                Module = "M0",
                Group = "G0",
                SetPoint = "SP2",
                Condition = "1 == 0",
            };

            this.sharedStorageMock.Setup(o => o.KeyExistsInObjectTable("SetPoints", Context.DUT)).Returns(true);
            var map = new FreqSetPointMap()
            {
                CornerIdentifiers = new Dictionary<string, List<PatConfigSetPoint>>
                {
                    { "C0", new List<PatConfigSetPoint> { setPoint1, setPoint2 } },
                    { "C3", new List<PatConfigSetPoint> { setPoint2 } },
                },
            };
            var serializedObject = JsonConvert.SerializeObject(map);
            this.SharedStorageValues.Add("SetPoints", serializedObject);

            var target = new CornerPatConfigSetPointsHandler("SomePatlist", "SetPoints", vminForwarding);
            target.Run();
            patConfigService.VerifyAll();
            patConfigSetPoint.VerifyAll();
            vminForwardingFactoryMock.VerifyAll();
            vminForwardingMock.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void Run_Skip()
        {
            var vminForwardingFactoryMock = new Mock<IVminForwardingFactory>(MockBehavior.Strict);
            var vminForwardingMock = new Mock<IVminForwardingCorner>(MockBehavior.Strict);
            var tuple0 = new Tuple<string, int, IVminForwardingCorner>("C0", 1, vminForwardingMock.Object);
            var tuple1 = new Tuple<string, int, IVminForwardingCorner>("C1", 1, vminForwardingMock.Object);
            var tuple2 = new Tuple<string, int, IVminForwardingCorner>("C2", 1, vminForwardingMock.Object);
            var tuple3 = new Tuple<string, int, IVminForwardingCorner>("C3", 1, vminForwardingMock.Object);
            var vminForwarding = new List<Tuple<string, int, IVminForwardingCorner>> { tuple0, tuple1, tuple2, tuple3 };
            DDG.VminForwarding.Service = vminForwardingFactoryMock.Object;

            var setPoint = new PatConfigSetPoint()
            {
                Module = "M0",
                Group = "G0",
                SetPoint = "SP1",
            };

            this.sharedStorageMock.Setup(o => o.KeyExistsInObjectTable("SetPoints", Context.DUT)).Returns(true);
            var map = new FreqSetPointMap()
            {
                CornerIdentifiers = new Dictionary<string, List<PatConfigSetPoint>>(),
            };
            var serializedObject = JsonConvert.SerializeObject(map);
            this.SharedStorageValues.Add("SetPoints", serializedObject);

            var target = new CornerPatConfigSetPointsHandler("SomePatlist", "SetPoints", vminForwarding);
            target.Run();
            vminForwardingFactoryMock.VerifyAll();
            vminForwardingMock.VerifyAll();
        }

        /// <summary>
        /// Refer to TestMethod name.
        /// </summary>
        [TestMethod]
        public void Run_NoVminForwarding()
        {
            var setPoint = new PatConfigSetPoint()
            {
                Module = "M0",
                Group = "G0",
                SetPoint = "SP1",
            };

            this.sharedStorageMock.Setup(o => o.KeyExistsInObjectTable("SetPoints", Context.DUT)).Returns(true);
            var map = new FreqSetPointMap()
            {
                CornerIdentifiers = new Dictionary<string, List<PatConfigSetPoint>>(),
            };
            var serializedObject = JsonConvert.SerializeObject(map);
            this.SharedStorageValues.Add("SetPoints", serializedObject);

            var target = new CornerPatConfigSetPointsHandler("SomePatlist", "SetPoints", null);
            target.Run();
        }
    }
}
