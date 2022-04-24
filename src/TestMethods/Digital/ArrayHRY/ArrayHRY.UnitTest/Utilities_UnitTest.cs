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

namespace ArrayHRY.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.PinService;
    using Prime.TestProgramService;
    using Prime.UserVarService;

    /// <summary>
    /// Defines the <see cref="Utilities_UnitTest" />.
    /// </summary>
    [TestClass]
    public class Utilities_UnitTest
    {
        /// <summary>
        /// Initialize method to setup all common mocks.
        /// </summary>
        [TestInitialize]
        public void TestSetup()
        {
            // Ignore any print messages.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(p => p.PrintDebug(It.IsAny<string>())).Callback((string msg) => Console.WriteLine(msg));
            consoleServiceMock.Setup(p => p.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int i, string s1, string s2) => Console.WriteLine("ERROR:" + msg));
            Prime.Services.ConsoleService = consoleServiceMock.Object;
        }

        /// <summary>
        /// Test out the reverse method in Utilities.
        /// </summary>
        [TestMethod]
        public void UtilitiesReverse()
        {
            Assert.AreEqual("a", Utilities.StringReverse("a"));
            Assert.AreEqual("dcba", Utilities.StringReverse("abcd"));
            Assert.AreEqual(string.Empty, Utilities.StringReverse(string.Empty));
        }

        /// <summary>
        /// Test the exceptions thrown by Utilities.RangeToStartAndLengthTuple().
        /// </summary>
        [TestMethod]
        public void RangeToStartAndLengthTuple_Exception()
        {
            var ex = Assert.ThrowsException<ArgumentException>(() => Utilities.RangeToStartAndLengthTuple(null));
            Assert.AreEqual("Range cannot be null or empty.", ex.Message);

            var ex1 = Assert.ThrowsException<ArgumentException>(() => Utilities.RangeToStartAndLengthTuple("a-5"));
            Assert.AreEqual("Cannot convert R0=[a] and R1=[5] into unsigned integers from range=[a-5]", ex1.Message);

            var ex2 = Assert.ThrowsException<ArgumentException>(() => Utilities.RangeToStartAndLengthTuple("5-x"));
            Assert.AreEqual("Cannot convert R0=[5] and R1=[x] into unsigned integers from range=[5-x]", ex2.Message);

            var ex3 = Assert.ThrowsException<ArgumentException>(() => Utilities.RangeToStartAndLengthTuple("q"));
            Assert.AreEqual("Cannot convert range=[q] into an unsigned integer.", ex3.Message);
        }

        /// <summary>
        /// Test the passing cases of Utilities.RangeToStartAndLengthTuple().
        /// </summary>
        [TestMethod]
        public void RangeToStartAndLengthTuple_Pass()
        {
            Assert.AreEqual(new Tuple<uint, int>(5, 1), Utilities.RangeToStartAndLengthTuple("5"));
            Assert.AreEqual(new Tuple<uint, int>(5, 1), Utilities.RangeToStartAndLengthTuple("5-5"));
            Assert.AreEqual(new Tuple<uint, int>(1, 11), Utilities.RangeToStartAndLengthTuple("1-11"));
            Assert.AreEqual(new Tuple<uint, int>(0, -8), Utilities.RangeToStartAndLengthTuple("7-0"));
        }

        /// <summary>
        /// Test the passing cases of Utilities.GetSubStringWithStartAndLengthTuple().
        /// </summary>
        [TestMethod]
        public void GetSubStringWithStartAndLengthTuple_Pass()
        {
            Assert.AreEqual("A", Utilities.GetSubStringWithStartAndLengthTuple("ABCDEFG", new Tuple<uint, int>(0, 1)));
            Assert.AreEqual("B", Utilities.GetSubStringWithStartAndLengthTuple("ABCDEFG", new Tuple<uint, int>(1, 1)));
            Assert.AreEqual("CDEF", Utilities.GetSubStringWithStartAndLengthTuple("ABCDEFG", new Tuple<uint, int>(2, 4)));
            Assert.AreEqual("FEDC", Utilities.GetSubStringWithStartAndLengthTuple("ABCDEFG", new Tuple<uint, int>(2, -4)));
        }

        /// <summary>
        /// Test the case where it is not a pin.
        /// </summary>
        [TestMethod]
        public void IsPinNotGroup_NotPin_False()
        {
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(o => o.Exists("FakePin")).Returns(false);
            Prime.Services.PinService = pinServiceMock.Object;

            Assert.IsFalse(Utilities.IsPinNotGroup("FakePin"));
            pinServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the case where it is a group.
        /// </summary>
        [TestMethod]
        public void IsPinNotGroup_IsGroup_False()
        {
            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(o => o.IsGroup()).Returns(true);

            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(o => o.Exists("SomeGroup")).Returns(true);
            pinServiceMock.Setup(o => o.Get("SomeGroup")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            Assert.IsFalse(Utilities.IsPinNotGroup("SomeGroup"));
            pinServiceMock.VerifyAll();
            pinMock.VerifyAll();
        }

        /// <summary>
        /// Test the case where it is a pin.
        /// </summary>
        [TestMethod]
        public void IsPinNotGroup_IsPin_True()
        {
            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(o => o.IsGroup()).Returns(false);

            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(o => o.Exists("SomePin")).Returns(true);
            pinServiceMock.Setup(o => o.Get("SomePin")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            Assert.IsTrue(Utilities.IsPinNotGroup("SomePin"));
            pinServiceMock.VerifyAll();
            pinMock.VerifyAll();
        }

        /// <summary>
        /// Test the case where it is in the current scope.
        /// </summary>
        [TestMethod]
        public void ResolvePinScope_CurrentScope_True()
        {
            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(o => o.IsGroup()).Returns(false);

            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(o => o.Exists("SomePin")).Returns(true);
            pinServiceMock.Setup(o => o.Get("SomePin")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var pinName = "SomePin";
            Assert.IsTrue(Utilities.ResolvePinScope("IP_CPU", ref pinName));
            Assert.AreEqual("SomePin", pinName);
            pinServiceMock.VerifyAll();
            pinMock.VerifyAll();
        }

        /// <summary>
        /// Test the case where it is in the IP scope.
        /// </summary>
        [TestMethod]
        public void ResolvePinScope_IpScope_True()
        {
            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(o => o.IsGroup()).Returns(false);

            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(o => o.Exists("SomePin")).Returns(false);
            pinServiceMock.Setup(o => o.Exists("IP_CPU::SomePin")).Returns(true);
            pinServiceMock.Setup(o => o.Get("IP_CPU::SomePin")).Returns(pinMock.Object);
            Prime.Services.PinService = pinServiceMock.Object;

            var pinName = "SomePin";
            Assert.IsTrue(Utilities.ResolvePinScope("IP_CPU", ref pinName));
            Assert.AreEqual("IP_CPU::SomePin", pinName);
            pinServiceMock.VerifyAll();
            pinMock.VerifyAll();
        }

        /// <summary>
        /// Test the case where the pin is not found.
        /// </summary>
        [TestMethod]
        public void ResolvePinScope_NotFound_False()
        {
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            pinServiceMock.Setup(o => o.Exists("SomePin")).Returns(false);
            pinServiceMock.Setup(o => o.Exists("IP_CPU::SomePin")).Returns(false);
            Prime.Services.PinService = pinServiceMock.Object;

            var pinName = "SomePin";
            Assert.IsFalse(Utilities.ResolvePinScope("IP_CPU", ref pinName));
            pinServiceMock.VerifyAll();
        }

        /// <summary>
        /// Check the ResolveUserVar case where the uservar exists as-is.
        /// </summary>
        [TestMethod]
        public void ResolveUserVar_AsIs_True()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("collection.uservar1")).Returns(true);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var userVar = "collection.uservar1";
            Assert.IsTrue(Utilities.ResolveUserVar("FakeIP", "FakeModule", ref userVar));
            Assert.AreEqual("collection.uservar1", userVar);
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Check the ResolveUserVar case where the uservar exists in IP scope.
        /// </summary>
        [TestMethod]
        public void ResolveUserVar_InIP_True()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("collection.uservar1")).Returns(false);
            userVarServiceMock.Setup(o => o.Exists("FakeIP::FakeModule::collection.uservar1")).Returns(true);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var userVar = "collection.uservar1";
            Assert.IsTrue(Utilities.ResolveUserVar("FakeIP", "FakeModule", ref userVar));
            Assert.AreEqual("FakeIP::FakeModule::collection.uservar1", userVar);
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Check the ResolveUserVar case where the uservar exists in IP Scope.
        /// </summary>
        [TestMethod]
        public void ResolveUserVar_InIPAlreadyHasModule_True()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("FakeModule::collection.uservar1")).Returns(false);
            userVarServiceMock.Setup(o => o.Exists("FakeIP::FakeModule::collection.uservar1")).Returns(true);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var userVar = "FakeModule::collection.uservar1";
            Assert.IsTrue(Utilities.ResolveUserVar("FakeIP", "FakeModule", ref userVar));
            Assert.AreEqual("FakeIP::FakeModule::collection.uservar1", userVar);
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Check the ResolveUserVar case where the uservar exists in IP Scope with the collection as module name.
        /// </summary>
        [TestMethod]
        public void ResolveUserVar_InIPCollectionAsModule_True()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("collection.uservar1")).Returns(false);
            userVarServiceMock.Setup(o => o.Exists("FakeIP::FakeModule::collection.uservar1")).Returns(false);
            userVarServiceMock.Setup(o => o.Exists("FakeIP::collection::collection.uservar1")).Returns(true);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var userVar = "collection.uservar1";
            Assert.IsTrue(Utilities.ResolveUserVar("FakeIP", "FakeModule", ref userVar));
            Assert.AreEqual("FakeIP::collection::collection.uservar1", userVar);
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Check the ResolveUserVar case where the uservar is found in PKG scope.
        /// </summary>
        [TestMethod]
        public void ResolveUserVar_InPkgCollectionAsModule_False()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("collection.uservar1")).Returns(false);
            userVarServiceMock.Setup(o => o.Exists("FakeIP::FakeModule::collection.uservar1")).Returns(false);
            userVarServiceMock.Setup(o => o.Exists("FakeIP::collection::collection.uservar1")).Returns(false);
            userVarServiceMock.Setup(o => o.Exists("__main__::collection::collection.uservar1")).Returns(true);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var userVar = "collection.uservar1";
            Assert.IsTrue(Utilities.ResolveUserVar("FakeIP", "FakeModule", ref userVar));
            Assert.AreEqual("__main__::collection::collection.uservar1", userVar);
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Check the ResolveUserVar case where the uservar isn't found.
        /// </summary>
        [TestMethod]
        public void ResolveUserVar_NotFound_False()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("collection.uservar1")).Returns(false);
            userVarServiceMock.Setup(o => o.Exists("FakeIP::FakeModule::collection.uservar1")).Returns(false);
            userVarServiceMock.Setup(o => o.Exists("FakeIP::collection::collection.uservar1")).Returns(false);
            userVarServiceMock.Setup(o => o.Exists("__main__::collection::collection.uservar1")).Returns(false);
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var userVar = "collection.uservar1";
            Assert.IsFalse(Utilities.ResolveUserVar("FakeIP", "FakeModule", ref userVar));
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Check the GetModuleAndIp functionality.
        /// </summary>
        [TestMethod]
        public void GetModuleAndIp_NoScope()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceName()).Returns("SomeTest");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            Utilities.GetModuleAndIp(out var ipName, out var moduleName);
            Assert.AreEqual(string.Empty, ipName);
            Assert.AreEqual(string.Empty, moduleName);
        }

        /// <summary>
        /// Check the GetModuleAndIp functionality.
        /// </summary>
        [TestMethod]
        public void GetModuleAndIp_PkgScope()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceName()).Returns("SomeModule::SomeTest");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            Utilities.GetModuleAndIp(out var ipName, out var moduleName);
            Assert.AreEqual(string.Empty, ipName);
            Assert.AreEqual("SomeModule", moduleName);
        }

        /// <summary>
        /// Check the GetModuleAndIp functionality.
        /// </summary>
        [TestMethod]
        public void GetModuleAndIp_IpScope()
        {
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceName()).Returns("SomeIP::SomeModule::SomeTest");
            Prime.Services.TestProgramService = testProgramServiceMock.Object;

            Utilities.GetModuleAndIp(out var ipName, out var moduleName);
            Assert.AreEqual("SomeIP", ipName);
            Assert.AreEqual("SomeModule", moduleName);
        }

        /// <summary>
        /// Test the DatalogHryStrgval where no data wrapping is required.
        /// </summary>
        [TestMethod]
        public void DatalogHryStrgval_NoWrapNoAlgName()
        {
            var hryData = "000000000001";
            var hryListData = new List<char>(hryData);

            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock.Setup(o => o.SetTnamePostfix("_HRY_RAWSTR_1"));
            writerMock.Setup(o => o.SetData(hryData));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            Utilities.DatalogHryStrgval(string.Empty, hryListData, 2000);
            datalogServiceMock.VerifyAll();
            writerMock.VerifyAll();
        }

        /// <summary>
        /// Test the DatalogHryStrgval where wrapping the data is required..
        /// </summary>
        [TestMethod]
        public void DatalogHryStrgval_WithWrapWithAlgName()
        {
            var hryData1 = "0000000001";
            var hryData2 = "0000100000";
            var hryData3 = "1111110000";
            var hryListData = new List<char>(hryData1 + hryData2 + hryData3);

            var writerMock1 = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock1.Setup(o => o.SetTnamePostfix("_AlgA_HRY_RAWSTR_1"));
            writerMock1.Setup(o => o.SetData(hryData1));

            var writerMock2 = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock2.Setup(o => o.SetTnamePostfix("_AlgA_HRY_RAWSTR_2"));
            writerMock2.Setup(o => o.SetData(hryData2));

            var writerMock3 = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            writerMock3.Setup(o => o.SetTnamePostfix("_AlgA_HRY_RAWSTR_3"));
            writerMock3.Setup(o => o.SetData(hryData3));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogServiceMock.SetupSequence(o => o.GetItuffStrgvalWriter())
                .Returns(writerMock1.Object)
                .Returns(writerMock2.Object)
                .Returns(writerMock3.Object);

            datalogServiceMock.Setup(o => o.WriteToItuff(writerMock1.Object));
            datalogServiceMock.Setup(o => o.WriteToItuff(writerMock2.Object));
            datalogServiceMock.Setup(o => o.WriteToItuff(writerMock3.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            Utilities.DatalogHryStrgval("AlgA", hryListData, 10);
            datalogServiceMock.VerifyAll();
            writerMock1.VerifyAll();
            writerMock2.VerifyAll();
            writerMock3.VerifyAll();
        }
    }
}
