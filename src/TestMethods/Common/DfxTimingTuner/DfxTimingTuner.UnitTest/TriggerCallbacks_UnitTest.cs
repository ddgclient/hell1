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

namespace DfxTimingTuner.UnitTest
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.PinService;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;

    /// <summary>
    /// Defines the <see cref="TriggerCallbacks_UnitTest" />.
    /// </summary>
    [TestClass]
    public class TriggerCallbacks_UnitTest
    {
        /// <summary>
        /// Setup testprogram mocks common to all tests.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            var testprogramServiceMocks = new Mock<ITestProgramService>(MockBehavior.Strict);
            testprogramServiceMocks.Setup(o => o.GetCurrentDutId()).Returns("1");
            testprogramServiceMocks.Setup(o => o.GetCurrentIpName()).Returns(string.Empty);
            Prime.Services.TestProgramService = testprogramServiceMocks.Object;
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void GetCallCount_NoSetup_Exception()
        {
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(o => o.KeyExistsInIntegerTable(TriggerCallbacks.CallbackCountStorageName, Context.IP)).Returns(false);
            Prime.Services.SharedStorageService = sharedStorageMock.Object;

            /* TriggerCallbacks.ResetStorageForCallbacks(); */
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => TriggerCallbacks.GetCallCount());
            Assert.IsTrue(ex.Message.Contains("TriggerCallbacks.SetupStorageForCallbacks() was probably not called"));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void IncrementCompareEdge_Pass()
        {
            var searchPins = new List<string> { "Pin1", "Pin2", "Pin3" };
            double stepSize = 0.1;
            int numSteps = 10;

            // setup the mocks.
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;
            var sharedStorageMock = this.MockSharedStorageForTriggers(searchPins, stepSize, numSteps);

            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            foreach (var pin in searchPins)
            {
                pinServiceMock.Setup(p => p.SetCompareTiming(pin, stepSize.ToString()));
            }

            Prime.Services.PinService = pinServiceMock.Object;

            // Call the function.
            TriggerCallbacks.SetupStorageForCallbacks(stepSize, searchPins);
            for (var step = 0; step < numSteps; step++)
            {
                TriggerCallbacks.IncrementCompareEdge(string.Empty);
            }

            // check all the mocks.
            sharedStorageMock.VerifyAll();
            foreach (var pin in searchPins)
            {
                pinServiceMock.Verify(p => p.SetCompareTiming(pin, stepSize.ToString()), Times.Exactly(numSteps));
            }
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void IncrementDriveEdge_Pass()
        {
            var searchPins = new List<string> { "Pin1", "Pin2", "Pin3" };
            double stepSize = 0.1;
            int numSteps = 2;

            // setup the mocks.
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;
            var sharedStorageMock = this.MockSharedStorageForTriggers(searchPins, stepSize, numSteps);

            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            foreach (var pin in searchPins)
            {
                pinServiceMock.Setup(p => p.SetDriveTiming(pin, stepSize.ToString()));
            }

            Prime.Services.PinService = pinServiceMock.Object;

            // Call the function.
            TriggerCallbacks.SetupStorageForCallbacks(stepSize, searchPins);
            for (var step = 0; step < numSteps; step++)
            {
                TriggerCallbacks.IncrementDriveEdge(string.Empty);
            }

            // check all the mocks.
            sharedStorageMock.VerifyAll();
            foreach (var pin in searchPins)
            {
                pinServiceMock.Verify(p => p.SetDriveTiming(pin, stepSize.ToString()), Times.Exactly(numSteps));
            }
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void IncrementDriveEdge_Fail()
        {
            var searchPins = new List<string> { "Pin1", "Pin2", "Pin3" };
            double stepSize = 0.1;
            int numSteps = 0;

            // setup the mocks.
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;
            var sharedStorageMock = this.MockSharedStorageForTriggers(searchPins, stepSize, numSteps);
            sharedStorageMock.Setup(s => s.InsertRowAtTable(TriggerCallbacks.FailStorageName, "ERROR", Context.IP));
            sharedStorageMock.Setup(s => s.GetStringRowFromTable(TriggerCallbacks.FailStorageName, Context.IP)).Returns("ERROR");

            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(p => p.SetDriveTiming(searchPins.First(), stepSize.ToString())).Throws(new Prime.Base.Exceptions.FatalException("ERROR"));
            Prime.Services.PinService = pinServiceMock.Object;

            // Call the function.
            TriggerCallbacks.SetupStorageForCallbacks(stepSize, searchPins);
            TriggerCallbacks.IncrementDriveEdge(string.Empty);

            // check all the mocks.
            var actualError = TriggerCallbacks.GetFailureStatus(out var message);
            Assert.IsTrue(actualError);
            Assert.AreEqual("ERROR", message);

            sharedStorageMock.VerifyAll();
            pinServiceMock.VerifyAll();
        }

        private Mock<ISharedStorageService> MockSharedStorageForTriggers(List<string> pins, double stepSize, int numSteps)
        {
            var sharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedStorageMock.Setup(s => s.InsertRowAtTable(TriggerCallbacks.IncrementStorageName, stepSize.ToString(), Context.IP));
            sharedStorageMock.Setup(s => s.GetStringRowFromTable(TriggerCallbacks.IncrementStorageName, Context.IP)).Returns(stepSize.ToString());

            sharedStorageMock.Setup(s => s.InsertRowAtTable(TriggerCallbacks.PinListStorageName, pins, Context.IP));
            sharedStorageMock.Setup(s => s.GetRowFromTable(TriggerCallbacks.PinListStorageName, typeof(List<string>), Context.IP)).Returns(pins);

            sharedStorageMock.Setup(s => s.InsertRowAtTable(TriggerCallbacks.FailStorageName, string.Empty, Context.IP));

            sharedStorageMock.Setup(s => s.InsertRowAtTable(TriggerCallbacks.CallbackCountStorageName, 0, Context.IP));
            Queue<int> counts = new Queue<int>(numSteps);
            for (var i = 0; i < numSteps; i++)
            {
                sharedStorageMock.Setup(s => s.InsertRowAtTable(TriggerCallbacks.CallbackCountStorageName, i + 1, Context.IP));
                counts.Enqueue(i);
            }

            if (numSteps > 0)
            {
                sharedStorageMock.Setup(s => s.GetIntegerRowFromTable(TriggerCallbacks.CallbackCountStorageName, Context.IP))
                    .Returns(() => { return counts.Dequeue(); });
            }

            Prime.Services.SharedStorageService = sharedStorageMock.Object;
            return sharedStorageMock;
        }
    }
}
