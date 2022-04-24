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

namespace PlistModificationsBase.UnitTest
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.Base.Exceptions;
    using Prime.PerformanceService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PlistModificationsBase_UnitTest : PlistModificationsBase
    {
        /// <summary>
        /// Initializes mocks and other initial values.
        /// </summary>
        /// <exception cref="FatalException">Prime Exception.</exception>
        [TestInitialize]
        public void Initialize()
        {
            DDG.PlistModifications.Service.CleanTree(string.Empty);
            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Verify_Pass()
        {
            this.Verify();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_EmptyRestore_Pass()
        {
            this.OperationMode = Mode.Restore;
            this.Verify();
            this.Execute();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_RestorePlist_Pass()
        {
            this.OperationMode = Mode.Restore;
            this.Patlists = "Patlist";
            this.Verify();
            this.Execute();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_EmptyClean_Pass()
        {
            this.OperationMode = Mode.Clean;
            this.Verify();
            this.Execute();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_CleanPlist_Pass()
        {
            this.OperationMode = Mode.Clean;
            this.Patlists = "Patlist";
            this.Verify();
            this.Execute();
        }
    }
}
