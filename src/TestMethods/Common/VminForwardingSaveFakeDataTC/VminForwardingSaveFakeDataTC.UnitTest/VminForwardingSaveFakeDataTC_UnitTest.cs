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

namespace VminForwardingSaveFakeDataTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class VminForwardingSaveFakeDataTC_UnitTest
    {
        /// <summary>
        /// Initialize any common mocks.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_CheckParams()
        {
            Assert.ThrowsException<Exception>(() => this.RunVerify(string.Empty, string.Empty, 0, string.Empty));
            this.RunVerify("CLR", "F1", 2, "0.81");
            Assert.ThrowsException<Exception>(() => this.RunVerify(string.Empty, "F1", 2, "0.81"));
            Assert.ThrowsException<Exception>(() => this.RunVerify("CLR", string.Empty, 2, "0.81"));
            Assert.ThrowsException<Exception>(() => this.RunVerify("CLR", "F1", 0, "0.81"));
            Assert.ThrowsException<Exception>(() => this.RunVerify("CLR", "F1", 2, string.Empty));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_Pass()
        {
            var vminList = new List<double> { 0.8, 1.1 };
            const string domains = "CR0,CLR";
            const string corner = "F1";
            const int flow = 2;

            var vminHandlerMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminHandlerMock.Setup(o => o.StoreVminResult(vminList)).Returns(true);

            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            vminFactoryMock.Setup(o => o.Get(domains, corner, flow)).Returns(vminHandlerMock.Object);
            DDG.VminForwarding.Service = vminFactoryMock.Object;

            var underTest = new VminForwardingSaveFakeDataTC
            {
                Domains = domains,
                FrequencyCorner = corner,
                FlowId = flow,
                VminResults = string.Join(",", vminList),
            };
            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());
            vminHandlerMock.Verify(o => o.StoreVminResult(vminList), Times.Once);
        }

        private void RunVerify(string domains, string corners, int flow, string vmins)
        {
            var underTest = new VminForwardingSaveFakeDataTC
            {
                Domains = domains,
                FrequencyCorner = corners,
                FlowId = flow,
                VminResults = vmins,
            };

            underTest.Verify();
        }
    }
}
