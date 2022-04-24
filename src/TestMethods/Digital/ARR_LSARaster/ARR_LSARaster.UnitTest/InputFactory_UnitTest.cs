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
    using global::LSARasterTC;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.FileService;

    /// <summary>
    /// Unit testing methods in the InputFactory class.
    /// </summary>
    [TestClass]
    public class InputFactory_UnitTest
    {
        private Mock<IFileService> mockFile = new Mock<IFileService>();

        /// <summary>
        /// Initialize method for this test class.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            this.mockFile.Setup(x => x.FileExists(It.IsAny<string>()))
                .Returns((string filepath) => { return System.IO.File.Exists(filepath); });
            this.mockFile.Setup(x => x.GetFile(It.IsAny<string>()))
                .Returns((string filepath) => { return filepath; });

            Prime.Services.FileService = this.mockFile.Object;
        }

        /// <summary>
        /// When method detects a XML, create a XMLInput object.
        /// </summary>
        [TestMethod]
        public void CreateConfigHandler_CreateXMLConfig()
        {
            var handler = InputFactory.CreateConfigHandler(@"./TestInput/empty.xml");
            Assert.IsInstanceOfType(handler, typeof(XMLInput));
        }

        /// <summary>
        /// When method detects a JSON, create a JsonInput object.
        /// </summary>
        [TestMethod]
        public void CreateConfigHandler_CreateJSONConfig()
        {
            var handler = InputFactory.CreateConfigHandler(@"./TestInput/empty.json");
            Assert.IsInstanceOfType(handler, typeof(JsonInput));
        }

        /// <summary>
        /// When a invalid filetype is detected, throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateConfigHandler_InvalidConfigType()
        {
            InputFactory.CreateConfigHandler(@"./TestInput/empty.nonvalid");
        }
    }
}
