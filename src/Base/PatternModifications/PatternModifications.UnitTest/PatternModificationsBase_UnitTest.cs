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

namespace PatternModificationsBase.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.Base.Exceptions;
    using Prime.FileService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PatternModificationsBase_UnitTest : PatternModificationsBase
    {
        private Mock<ISharedStorageService> SharedStorageMock { get; set; }

        private Mock<IFileService> FileServiceMock { get; set; }

        private Mock<IFileSystem> FileSystemMock { get; set; }

        private Dictionary<string, string> SharedStorageValues { get; set; }

        /// <summary>
        /// Initializes mocks and other initial values.
        /// </summary>
        /// <exception cref="FatalException">Prime Exception.</exception>
        [TestInitialize]
        public void Initialize()
        {
            // Default Mock for Shared service.
            this.SharedStorageValues = new Dictionary<string, string>();
            this.SharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>()))
                .Callback((string key, object obj, Context context) =>
                {
                    Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                });
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, string obj, Context context) =>
                {
                    Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.SharedStorageValues[key] = obj;
                });
            this.SharedStorageMock.Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>()))
                .Callback((string key, Type obj, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Type obj, Context context) => JsonConvert.DeserializeObject(this.SharedStorageValues[key], obj));
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => this.SharedStorageValues[key]);
            this.SharedStorageMock.Setup(o => o.KeyExistsInObjectTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    if (this.SharedStorageValues.ContainsKey(key))
                    {
                        Console.WriteLine($"SharedStorage Key={key} exists in table.");
                    }
                    else
                    {
                        Console.WriteLine($"SharedStorage Key={key} exists in table.");
                    }
                })
                .Returns((string key, Context context) => this.SharedStorageValues.ContainsKey(key));
            Prime.Services.SharedStorageService = this.SharedStorageMock.Object;

            this.FileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
            Prime.Services.FileService = this.FileServiceMock.Object;
            this.FileSystemMock = new Mock<IFileSystem>();
            this.FileSystemAbstraction = this.FileSystemMock.Object;
        }

        /// <summary>
        /// Single corner, single configuration.
        /// </summary>
        [TestMethod]
        public void Verify_SimpleCase_Pass()
        {
            this.SharedStorageKey = "SomeKey";
            this.ConfigurationFile = "SomeFile";
            var fileContents = @"{'CornerIdentifiers':{'CLRF1':[{'Module':'Func','Group':'core_freq','SetPoint':'{BM_core_freq}'}]}}";
            this.FileSystemMock.Setup(f => f.File.ReadAllText(It.IsAny<string>())).Returns(fileContents);
            this.FileServiceMock.Setup(f => f.FileExists(this.ConfigurationFile.ToString())).Returns(true);
            this.FileServiceMock.Setup(f => f.GetFile(this.ConfigurationFile.ToString())).Returns(this.ConfigurationFile);

            this.Verify();
            var executeResult = this.Execute();
            Assert.AreEqual(1, executeResult);

            this.SharedStorageMock.Verify(o => o.KeyExistsInObjectTable(this.SharedStorageKey, Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable(this.SharedStorageKey, It.IsAny<object>(), Context.DUT));
            this.FileSystemMock.Verify();
            this.FileServiceMock.Verify();
        }

        /// <summary>
        /// Invalid Syntax. Missing Module.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Newtonsoft.Json.JsonSerializationException))]
        public void Verify_InvalidSyntax_Fail()
        {
            this.SharedStorageKey = "SomeKey";
            this.ConfigurationFile = "SomeFile";
            var fileContents = @"{'CornerIdentifiers':{'CLRF1':[{'Group':'core_freq','SetPoint':'{BM_core_freq}'}]}}";
            this.FileSystemMock.Setup(f => f.File.ReadAllText(It.IsAny<string>())).Returns(fileContents);
            this.FileServiceMock.Setup(f => f.FileExists(this.ConfigurationFile.ToString())).Returns(true);
            this.FileServiceMock.Setup(f => f.GetFile(this.ConfigurationFile.ToString())).Returns(this.ConfigurationFile);
            this.Verify();
        }
    }
}
