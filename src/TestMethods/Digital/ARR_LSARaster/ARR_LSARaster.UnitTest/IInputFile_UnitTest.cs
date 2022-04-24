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
    using global::LSARasterTC;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary> Dummy description of this test method's unit test.</summary>
    [TestClass]
    public class IInputFile_UnitTest
    {
        private Mock<IInputFile> userInput;

        /// <summary> Dummy description of this test method's unit test.</summary>
        [TestInitialize]
        public void Init()
        {
            this.userInput = new Mock<IInputFile>();
            this.userInput.Setup(
                ui => ui.DeserializeInput<MetadataConfig>())
                .Returns(
                new Mock<MetadataConfig>().Object);
        }

        /// <summary> Dummy description of this test method's unit test.</summary>
        [TestMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:Parameters should be on same line or separate lines", Justification = "String interpolation")]
        public void DeserializeToGivenType()
        {
            var deserializedInput = this.userInput.Object.DeserializeInput<MetadataConfig>();
            Assert.IsInstanceOfType(deserializedInput, typeof(MetadataConfig),
                string.Format("Was of type {0}, was supposed to be {1}", deserializedInput.GetType(), typeof(MetadataConfig)));
        }

        /// <summary>
        /// Assert that our interface can deserialize to many given types.
        /// </summary>
        [TestMethod]
        public void DeserializeToManyTypes()
        {
            var iInterfaceFile = new Mock<IInputFile>();
            iInterfaceFile.Setup(ui => ui.DeserializeInput<IInputFile>())
                .Returns(
                new Mock<IInputFile>().Object);

            var metaFile = new Mock<IInputFile>();
            metaFile.Setup(
                ui => ui.DeserializeInput<MetadataConfig>())
                .Returns(
                new Mock<MetadataConfig>().Object);

            var rasterFile = new Mock<IInputFile>();
            rasterFile.Setup(
                ui => ui.DeserializeInput<RasterConfig>())
                .Returns(
                new Mock<RasterConfig>().Object);

            Assert.IsInstanceOfType(iInterfaceFile.Object.DeserializeInput<IInputFile>(), typeof(IInputFile));
            Assert.IsInstanceOfType(metaFile.Object.DeserializeInput<MetadataConfig>(), typeof(MetadataConfig));
            Assert.IsInstanceOfType(rasterFile.Object.DeserializeInput<RasterConfig>(), typeof(RasterConfig));
        }
    }
}
