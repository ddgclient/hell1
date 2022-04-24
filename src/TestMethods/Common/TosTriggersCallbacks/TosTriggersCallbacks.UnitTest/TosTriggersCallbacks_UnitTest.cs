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

namespace TosTriggersCallbacks.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.PinService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class TosTriggersCallbacks_UnitTest
    {
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<ISharedStorageService> sharedStorageService;
        private Dictionary<string, string> sharedStorageValues;

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(
                    o => o.PrintDebug(It.IsAny<string>())).
                Callback((string msg) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            this.consoleServiceMock.Setup(
                    o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string member, string path) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            // Default Mock for Shared service.
            this.sharedStorageService = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.sharedStorageValues = new Dictionary<string, string>();
            this.sharedStorageService.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), Prime.SharedStorageService.Context.IP))
                .Callback((string key, object obj, Context context) =>
                {
                    Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.sharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                });
            this.sharedStorageService.Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), Prime.SharedStorageService.Context.IP))
                .Callback((string key, Type obj, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                })
                .Returns((string key, Type obj, Context context) => JsonConvert.DeserializeObject(this.sharedStorageValues[key], obj));
            this.sharedStorageService.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.sharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => this.sharedStorageValues[key]);
            Prime.Services.SharedStorageService = this.sharedStorageService.Object;
        }

        /// <summary>
        /// SetupExecute_SinglePin_Pass.
        /// </summary>
        [TestMethod]
        public void SetupExecute_SinglePin_Pass()
        {
            var pinService = new Mock<IPinService>(MockBehavior.Strict);
            pinService.Setup(o => o.SetPinAttributeValues("PinA", new Dictionary<string, string> { { "VForce", "1.1" } }));
            Prime.Services.PinService = pinService.Object;
            TosTriggersCallbacks.TosTriggersCallbackSetup("--type=setpinattributes --prepause=1 --postpause=2 --settings=1:PinA:VForce:1.1");
            TosTriggersCallbacks.TosTriggersCallbackExecute("1");
        }

        /// <summary>
        /// SetupExecute_SinglePin_Pass.
        /// </summary>
        [TestMethod]
        public void SetupExecute_SinglePinTwoSetups_Pass()
        {
            var pinService = new Mock<IPinService>(MockBehavior.Strict);
            pinService.Setup(o => o.SetPinAttributeValues("PinA", new Dictionary<string, string> { { "VForce", "1.1" }, { "FreeDriveTime", "0.05" } }));
            Prime.Services.PinService = pinService.Object;
            TosTriggersCallbacks.TosTriggersCallbackSetup("--type=setpinattributes --prepause=1 --postpause=2 --settings 1:PinA:VForce:1.1 1:PinA:FreeDriveTime:0.05");
            TosTriggersCallbacks.TosTriggersCallbackExecute("1");
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Setup_InvalidType_Fail()
        {
            var pinService = new Mock<IPinService>(MockBehavior.Strict);
            pinService.Setup(o => o.SetPinAttributeValues("PinA", new Dictionary<string, string> { { "VForce", "1.1" } }));
            Prime.Services.PinService = pinService.Object;
            var ex = Assert.ThrowsException<ArgumentException>(() => TosTriggersCallbacks.TosTriggersCallbackSetup("--type=invalidtype --prepause=1 --postpause=2 --settings=1:PinA:VForce:1.1"));
            Assert.AreEqual("Unsupported callback type=invalidtype", ex.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Setup_FormatError_Fail()
        {
            var pinService = new Mock<IPinService>(MockBehavior.Strict);
            pinService.Setup(o => o.SetPinAttributeValues("PinA", new Dictionary<string, string> { { "VForce", "1.1" } }));
            Prime.Services.PinService = pinService.Object;
            Assert.ThrowsException<ArgumentException>(() => TosTriggersCallbacks.TosTriggersCallbackSetup("-$type=invalidtype --prepause=1 --postpause=2 --settings=1:PinA:VForce:1.1"));
        }

        /// <summary>
        /// SetupExecute_MultiPin_Pass.
        /// </summary>
        [TestMethod]
        public void SetupExecute_MultiPin_Pass()
        {
            var pinService = new Mock<IPinService>(MockBehavior.Strict);
            pinService.Setup(o => o.SetPinAttributeValues("PinA", new Dictionary<string, string> { { "VForce", "1.1" } }));
            pinService.Setup(o => o.SetPinAttributeValues("PinB", new Dictionary<string, string> { { "VForce", "1.2" } }));
            Prime.Services.PinService = pinService.Object;
            TosTriggersCallbacks.TosTriggersCallbackSetup("--type setpinattributes --settings 1:PinA:VForce:1.1 1:PinB:VForce:1.2");
            TosTriggersCallbacks.TosTriggersCallbackExecute("1");
        }

        /// <summary>
        /// SetupExecute_InvalidNumberOfTokens_Fail.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetupExecute_InvalidNumberOfTokens_Fail()
        {
            var pinService = new Mock<IPinService>(MockBehavior.Strict);
            pinService.Setup(o => o.SetPinAttributeValues("PinA", new Dictionary<string, string> { { "VForce", "1.1" } }));
            Prime.Services.PinService = pinService.Object;
            TosTriggersCallbacks.TosTriggersCallbackSetup("--type=setpinattributes --prepause=1 --postpause=2 --settings=1:PinA:VForce:1.1:3");
            TosTriggersCallbacks.TosTriggersCallbackExecute("1");
        }
    }
}
