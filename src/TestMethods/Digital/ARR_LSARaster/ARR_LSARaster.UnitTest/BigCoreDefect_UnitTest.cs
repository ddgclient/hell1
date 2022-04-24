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

    /// <summary>
    /// Unit test class for BigCoreDefect class.
    /// </summary>
    [TestClass]
    public class BigCoreDefect_UnitTest : BaseTest
    {
        private JsonInput serializedMetadata;
        private MetadataConfig deserializedMetadata;

        private JsonInput serializedRaster;
        private RasterConfig deserializedRaster;

        /// <summary>
        /// Initialize method for this test class.
        /// </summary>
        [TestInitialize]
        public override void Init()
        {
            base.Init();

            this.serializedMetadata = new JsonInput(File.ReadAllText(@"./TestInput/Metadata.json"));
            this.deserializedMetadata = this.serializedMetadata.DeserializeInput<MetadataConfig>();

            this.serializedRaster = new JsonInput(File.ReadAllText(@"./TestInput/RasterConfig.json"));
            this.deserializedRaster = this.serializedRaster.DeserializeInput<RasterConfig>();
        }

        /// <summary>
        /// Check the logic of CreateDefects when no ctvData is detected.
        /// </summary>
        [TestMethod]
        public void CreateDefects_NoCtvData()
        {
            this.deserializedRaster.GetLdatArray("bpu_bme").MatchAddressToDwordElement(new Tuple<int, int, int>(0, 0, 0), out var container);
            List<IDefect> defects = BigCoreDefect.CreateDefects(new List<string>(), this.deserializedMetadata.GetCaptureSet("LSA_RASTER_CAPTURE_DECODING_SET_CORE"), this.deserializedMetadata.GetPinMappingSet("4_CORES"), "NOAB_00", "fake", container);
            Assert.IsTrue(defects[0].SendToRepair == false);
        }

        /// <summary>
        /// Check the logic of CreateDefects when no ctvData is detected.
        /// </summary>
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void CreateDefects_NotRecognizedDecodingElement()
        {
            Assert.IsTrue(false);
        }

        /// <summary>
        /// Ensure <see cref="BigCoreDefect"/> adds itself properly to generic data structure.
        /// </summary>
        [TestMethod]
        public void AddToInternalDatabase()
        {
            Dictionary<string, List<IDefect>> db = new Dictionary<string, List<IDefect>>();
            BigCoreDefect defect = new BigCoreDefect(0, 0, "00000000000", "000000000000");
            defect.Array = "fake";
            defect.AddToInternalDatabase(ref db);
            Assert.IsTrue(db["fake"].Count == 1);
        }

        /// <summary>
        /// Assert that we can check for equality.
        /// </summary>
        [TestMethod]
        public void Equals()
        {
            BigCoreDefect defect0 = new BigCoreDefect(0, 0, "00000000000", "000000000000");
            BigCoreDefect defect1 = new BigCoreDefect(0, 0, "00000000000", "000000000000");
            Assert.IsTrue(defect0.Equals(defect1));
        }

        /// <summary>
        /// Ensure we properly create expected header block.
        /// </summary>
        [TestMethod]
        public void CreateTFileHeaderBlock()
        {
            BigCoreDefect defect0 = new BigCoreDefect(0, 0, "00000000000", "000000000000");
            defect0.Array = "fake";
            string testBlock = defect0.CreateTFileHeaderBlock();
            string compareBlock = $"Array: fake\n";
            Assert.AreEqual(testBlock, compareBlock);
        }

        /// <summary>
        /// Ensure we properly create a the expected repair string.
        /// </summary>
        [TestMethod]
        public void CreateRepairString()
        {
            BigCoreDefect defect0 = new BigCoreDefect(0, 0, "00000000001", "000000000001");
            defect0.Array = "fake";
            defect0.Slice = "2";
            string testBlock = defect0.CreateRepairString();
            string compareBlock = $"fake,2,0,0,1,0";
            Assert.AreEqual(compareBlock, testBlock);
        }
    }
}
