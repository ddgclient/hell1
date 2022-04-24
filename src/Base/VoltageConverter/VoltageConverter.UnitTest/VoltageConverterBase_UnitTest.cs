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

namespace VoltageConverterBase.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.PerformanceService;
    using Prime.PinService;
    using Prime.SharedStorageService;
    using Prime.TestConditionService;
    using Prime.TestProgramService;
    using Prime.UserVarService;
    using Prime.VoltageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class VoltageConverterBase_UnitTest
    {
        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(
                o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
            {
                System.Console.WriteLine($"DEBUG: {msg}");
            });

            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetRailConfigurations_Pass()
        {
            var result = VoltageHandler.GetRailConfigurations("--railconfigurations conf1 conf2");
            Assert.AreEqual("conf1", result[0]);
            Assert.AreEqual("conf2", result[1]);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetRailConfigurations_Invalid_Fail()
        {
            Assert.ThrowsException<ArgumentException>(() => VoltageHandler.GetRailConfigurations("--invalid"));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetRailConfigurations_Empty_Null()
        {
            var result = VoltageHandler.GetRailConfigurations(string.Empty);
            Assert.IsNull(result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ParseCommandLine_Invalid_Fail()
        {
            Assert.ThrowsException<ArgumentException>(() => VoltageHandler.ParseCommandLine("NOM", "--invalid"));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ParseCommandLine_Empty_Null()
        {
            var result = VoltageHandler.ParseCommandLine(string.Empty, string.Empty);
            Assert.IsNull(result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ParseCommandLine_Null_Null()
        {
            var result = VoltageHandler.ParseCommandLine(string.Empty, null);
            Assert.IsNull(result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ParseCommandLine_InvalidFivrConditionOverride_Exception()
        {
            var ex = Assert.ThrowsException<ArgumentException>(() => VoltageHandler.ParseCommandLine("Condition1", "--fivrcondition=Condition2"));
            Assert.AreEqual("VoltageConverterBase.dll.<ParseCommandLine>b__0: fivrcondition cannot be overwritten using command line. Use instance parameter. --fivrcondition=Condition2", ex.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ParseCommandLine_InvalidSwitch_Exception()
        {
            Assert.ThrowsException<ArgumentException>(() => VoltageHandler.ParseCommandLine("Condition1", "--someinvalidswitch"));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageOverrides_Null_Null()
        {
            var result = VoltageHandler.GetVoltageOverrides(null);
            Assert.IsNull(result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageOverrides_Invalid_Exception()
        {
            var options = new VoltageConverterOptions
            {
                Overrides = "s|u|invalid",
            };

            Assert.ThrowsException<ArgumentException>(() => VoltageHandler.GetVoltageOverrides(options));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageOverrides_Literal_Pass()
        {
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            var fivrCondition = new Mock<IFivrCondition>();
            voltageServiceMock.Setup(o => o.CreateFivrForCondition("NOM", "SomePatlist")).Returns(fivrCondition.Object);
            var voltageObject = VoltageHandler.GetVoltageObject(null, "SomeLevels", "SomePatlist", "NOM", string.Empty, "--overrides=SA:0.8", out var options);
            var overrides = VoltageHandler.GetVoltageOverrides(options);
            Assert.IsInstanceOfType(voltageObject, typeof(IFivrCondition));
            Assert.AreEqual(0.8, overrides["SA"]);

            voltageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageOverrides_SharedStorage_Pass()
        {
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(o => o.KeyExistsInDoubleTable("TOKEN", Context.DUT)).Returns(true);
            sharedStorageServiceMock.Setup(o => o.GetDoubleRowFromTable("TOKEN", Context.DUT)).Returns(0.8);
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            var fivrCondition = new Mock<IFivrCondition>();
            voltageServiceMock.Setup(o => o.CreateFivrForCondition("NOM", "SomePatlist")).Returns(fivrCondition.Object);
            var voltageObject = VoltageHandler.GetVoltageObject(null, "SomeLevels", "SomePatlist", "NOM", string.Empty, "--overrides=SA:TOKEN", out var options);
            var overrides = VoltageHandler.GetVoltageOverrides(options);
            Assert.IsInstanceOfType(voltageObject, typeof(IFivrCondition));
            Assert.AreEqual(0.8, overrides["SA"]);

            sharedStorageServiceMock.VerifyAll();
            voltageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageOverrides_UserVar_Pass()
        {
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(o => o.KeyExistsInDoubleTable("TOKEN", Context.DUT)).Returns(false);
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            Prime.Services.UserVarService = userVarServiceMock.Object;
            userVarServiceMock.Setup(o => o.Exists("TOKEN")).Returns(true);
            userVarServiceMock.Setup(o => o.GetDoubleValue("TOKEN")).Returns(0.8);
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            var fivrCondition = new Mock<IFivrCondition>();
            voltageServiceMock.Setup(o => o.CreateFivrForCondition("NOM", "SomePatlist")).Returns(fivrCondition.Object);
            var voltageObject = VoltageHandler.GetVoltageObject(null, "SomeLevels", "SomePatlist", "NOM", string.Empty, "--overrides=SA:TOKEN", out var options);
            var overrides = VoltageHandler.GetVoltageOverrides(options);
            Assert.IsInstanceOfType(voltageObject, typeof(IFivrCondition));
            Assert.AreEqual(0.8, overrides["SA"]);

            sharedStorageServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            voltageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageOverrides_VminForwarding_Pass()
        {
            var sharedStorageServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            sharedStorageServiceMock.Setup(o => o.KeyExistsInDoubleTable("SA@F1", Context.DUT)).Returns(false);
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            Prime.Services.UserVarService = userVarServiceMock.Object;
            userVarServiceMock.Setup(o => o.Exists("SA@F1")).Returns(false);
            var testProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            Prime.Services.TestProgramService = testProgramServiceMock.Object;
            testProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameters()).Returns(new Dictionary<string, string> { { "FlowIndex", "1" } });
            var vminForwardingFactoryMock = new Mock<IVminForwardingFactory>(MockBehavior.Strict);
            DDG.VminForwarding.Service = vminForwardingFactoryMock.Object;
            var vminCorner = new Mock<IVminForwardingCorner>(MockBehavior.Strict);
            vminCorner.Setup(o => o.GetStartingVoltage(0)).Returns(0.8);
            vminForwardingFactoryMock.Setup(o => o.Get("SA@F1", 1)).Returns(vminCorner.Object);

            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            var fivrCondition = new Mock<IFivrCondition>();
            voltageServiceMock.Setup(o => o.CreateFivrForCondition("NOM", "SomePatlist")).Returns(fivrCondition.Object);
            var voltageObject = VoltageHandler.GetVoltageObject(null, "SomeLevels", "SomePatlist", "NOM", string.Empty, "--overrides=SA:SA@F1", out var options);
            var overrides = VoltageHandler.GetVoltageOverrides(options);
            Assert.IsInstanceOfType(voltageObject, typeof(IFivrCondition));
            Assert.AreEqual(0.8, overrides["SA"]);

            sharedStorageServiceMock.VerifyAll();
            userVarServiceMock.VerifyAll();
            voltageServiceMock.VerifyAll();
            vminCorner.VerifyAll();
            vminForwardingFactoryMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ApplySearchVoltage_FivrDomain_Pass()
        {
            var fivrDomain = new Mock<IFivrDomainsAndConditionWithRails>(MockBehavior.Strict);
            fivrDomain.Setup(o => o.Apply(new List<double> { 1 }));
            DDG.VoltageHandler.ApplySearchVoltage(fivrDomain.Object, new List<double> { 1.0 }, string.Empty);
            fivrDomain.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ApplySearchVoltage_Dps_Pass()
        {
            var dps = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            dps.Setup(o => o.Apply(new List<double> { 1 }));
            DDG.VoltageHandler.ApplySearchVoltage(dps.Object, new List<double> { 1.0 }, string.Empty);
            dps.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ApplySearchVoltage_DpsWithOffset_Pass()
        {
            var dps = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            dps.Setup(o => o.Apply(new List<double> { 1.1 }));
            DDG.VoltageHandler.ApplySearchVoltage(dps.Object, new List<double> { 1.0 }, "0.1");
            dps.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ApplyInitialVoltage_DpsNoOverrides_Pass()
        {
            var dps = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            DDG.VoltageHandler.ApplyInitialVoltage(dps.Object, "SomeLevels", null);
            dps.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ApplyInitialVoltage_DpsOverrides_Pass()
        {
            var dps = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            dps.Setup(o => o.Apply(new List<double> { 0.5 }));
            DDG.VoltageHandler.ApplyInitialVoltage(dps.Object, "SomeLevels", new Dictionary<string, double> { { "PinA", 0.5 } });
            dps.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ApplyInitialVoltage_FivrNoOverrides_Pass()
        {
            var fivr = new Mock<IFivrDomainsAndCondition>(MockBehavior.Strict);
            fivr.Setup(o => o.ApplyCondition());
            DDG.VoltageHandler.ApplyInitialVoltage(fivr.Object, "SomeLevels", null);
            fivr.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ApplyInitialVoltage_FivrOverrides_Pass()
        {
            var fivr = new Mock<IFivrDomainsAndCondition>(MockBehavior.Strict);
            fivr.Setup(o => o.ApplyConditionWithOverride(new Dictionary<string, double> { { "PinA", 0.5 } }));
            DDG.VoltageHandler.ApplyInitialVoltage(fivr.Object, "SomeLevels", new Dictionary<string, double> { { "PinA", 0.5 } });
            fivr.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVForceAttributesFromLevel_Pass()
        {
            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(o => o.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            Prime.Services.PinService = pinServiceMock.Object;
            pinServiceMock.Setup(o => o.Get("PinA")).Returns(pinMock.Object);
            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetPinAttributeValue("PinA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;
            testConditionServiceMock.Setup(o => o.GetTestCondition("SomeLevels")).Returns(testConditionMock.Object);
            var result = DDG.VoltageHandler.GetVForceAttributesFromLevel(new List<string> { "PinA" }, "SomeLevels");
            Assert.AreEqual("1mS", result["PinA"]["FreeDriveTime"]);

            pinMock.VerifyAll();
            pinServiceMock.VerifyAll();
            testConditionMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageObject_VForcePinAttribute()
        {
            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(o => o.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            Prime.Services.PinService = pinServiceMock.Object;
            pinServiceMock.Setup(o => o.Get("PinA")).Returns(pinMock.Object);
            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetPinAttributeValue("PinA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;
            testConditionServiceMock.Setup(o => o.GetTestCondition("SomeLevels")).Returns(testConditionMock.Object);
            var dps = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            voltageServiceMock.Setup(o => o.CreateVForceForPinAttribute(new List<string> { "PinA", "PinA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(dps.Object);
            var result = DDG.VoltageHandler.GetVoltageObject(new List<string> { "PinA", "PinA" }, "SomeLevels", "SomePatlist", null, null, null, out var options);
            Assert.IsInstanceOfType(result, typeof(IVForcePinAttribute));

            pinMock.VerifyAll();
            pinServiceMock.VerifyAll();
            testConditionMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageObject_VForcePinAttribute_Overrides()
        {
            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(o => o.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            Prime.Services.PinService = pinServiceMock.Object;
            pinServiceMock.Setup(o => o.Get("PinA")).Returns(pinMock.Object);
            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetPinAttributeValue("PinA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;
            testConditionServiceMock.Setup(o => o.GetTestCondition("SomeLevels")).Returns(testConditionMock.Object);
            var dps = new Mock<IVForcePinAttribute>(MockBehavior.Strict);
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            voltageServiceMock.Setup(o => o.CreateVForceForPinAttribute(new List<string> { "PinA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(dps.Object);
            var result = DDG.VoltageHandler.GetVoltageObject(null, "SomeLevels", "SomePatlist", null, null, "--overrides=PinA:1.0", out var options);
            Assert.IsInstanceOfType(result, typeof(IVForcePinAttribute));

            pinMock.VerifyAll();
            pinServiceMock.VerifyAll();
            testConditionMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageObject_VForcePinAttributeWithRails()
        {
            var pinMock = new Mock<IPin>(MockBehavior.Strict);
            pinMock.Setup(o => o.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinServiceMock = new Mock<IPinService>(MockBehavior.Strict);
            Prime.Services.PinService = pinServiceMock.Object;
            pinServiceMock.Setup(o => o.Get("PinA")).Returns(pinMock.Object);
            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetPinAttributeValue("PinA", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;
            testConditionServiceMock.Setup(o => o.GetTestCondition("SomeLevels")).Returns(testConditionMock.Object);
            var dps = new Mock<IVForcePinAttributeWithRails>(MockBehavior.Strict);
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            voltageServiceMock.Setup(o => o.CreateVForceForPinAttributeWithRails(new List<string> { "PinA" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>(), new List<string> { "powermux" })).Returns(dps.Object);
            var result = DDG.VoltageHandler.GetVoltageObject(new List<string> { "PinA" }, "SomeLevels", "SomePatlist", null, null, "--railconfigurations=powermux", out var options);
            Assert.IsInstanceOfType(result, typeof(IVForcePinAttributeWithRails));

            pinMock.VerifyAll();
            pinServiceMock.VerifyAll();
            testConditionMock.VerifyAll();
            testConditionServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageObject_FivrCondition()
        {
            var fivr = new Mock<IFivrCondition>(MockBehavior.Strict);
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            voltageServiceMock.Setup(o => o.CreateFivrForCondition("NOM", "SomePatlist")).Returns(fivr.Object);
            var result = DDG.VoltageHandler.GetVoltageObject(null, "SomeLevels", "SomePatlist", "NOM", null, null, out var options);
            Assert.IsInstanceOfType(result, typeof(IFivrCondition));

            voltageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageObject_FivrDomainsAndCondition()
        {
            var fivr = new Mock<IFivrDomainsAndCondition>(MockBehavior.Strict);
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            voltageServiceMock.Setup(o => o.CreateFivrForDomainsAndCondition(new List<string> { "DomainA" }, "NOM", "SomePatlist")).Returns(fivr.Object);
            var result = DDG.VoltageHandler.GetVoltageObject(new List<string> { "DomainA" }, "SomeLevels", "SomePatlist", "NOM", null, null, out var options);
            Assert.IsInstanceOfType(result, typeof(IFivrDomainsAndCondition));

            voltageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageObject_FivrDomainsAndConditionWithRails()
        {
            var fivr = new Mock<IFivrDomainsAndConditionWithRails>(MockBehavior.Strict);
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;
            voltageServiceMock.Setup(o => o.CreateFivrDomainsAndConditionWithRails(new List<string> { "DomainA" }, "NOM", "SomePatlist", new List<string> { "powermux" })).Returns(fivr.Object);
            var result = DDG.VoltageHandler.GetVoltageObject(new List<string> { "DomainA" }, "SomeLevels", "SomePatlist", "NOM", null, "--railconfigurations=powermux", out var options);
            Assert.IsInstanceOfType(result, typeof(IFivrDomainsAndConditionWithRails));

            voltageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageObject_FivrDomainsAndConditionWithRails_DLVR()
        {
            var fivr = new Mock<IFivrDomainsAndConditionWithRails>(MockBehavior.Strict);
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var pin = new Mock<IPin>(MockBehavior.Strict);
            pin.Setup(o => o.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinService = new Mock<IPinService>(MockBehavior.Strict);
            pinService.Setup(o => o.Get("dlvr")).Returns(pin.Object);
            Prime.Services.PinService = pinService.Object;

            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetPinAttributeValue("dlvr", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(o => o.GetTestCondition("SomeLevels")).Returns(testConditionMock.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            voltageServiceMock.Setup(o => o.CreateFivrDomainsAndConditionWithRails(new List<string> { "DomainA" }, "NOM", "SomePatlist", new List<string> { "powermux", "dlvr" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(fivr.Object);
            var result = DDG.VoltageHandler.GetVoltageObject(new List<string> { "DomainA" }, "SomeLevels", "SomePatlist", "NOM", null, "--railconfigurations=powermux --dlvrpins=dlvr", out var options);
            Assert.IsInstanceOfType(result, typeof(IFivrDomainsAndConditionWithRails));

            voltageServiceMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageObject_FivrDomainsAndConditionWithRails_DLVR_OverrideExpression()
        {
            var fivr = new Mock<IFivrDomainsAndConditionWithRails>(MockBehavior.Strict);
            fivr.Setup(o => o.OverrideExpression(RailHandlerType.DLVR, "new_expression", "dlvr"));
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var pin = new Mock<IPin>(MockBehavior.Strict);
            pin.Setup(o => o.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinService = new Mock<IPinService>(MockBehavior.Strict);
            pinService.Setup(o => o.Get("dlvr")).Returns(pin.Object);
            Prime.Services.PinService = pinService.Object;

            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetPinAttributeValue("dlvr", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(o => o.GetTestCondition("SomeLevels")).Returns(testConditionMock.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            voltageServiceMock.Setup(o => o.CreateFivrDomainsAndConditionWithRails(new List<string> { "DomainA" }, "NOM", "SomePatlist", new List<string> { "powermux", "dlvr" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(fivr.Object);
            var result = DDG.VoltageHandler.GetVoltageObject(new List<string> { "DomainA" }, "SomeLevels", "SomePatlist", "NOM", null, "--railconfigurations=powermux --dlvrpins=dlvr --expressions=new_expression", out var options);
            Assert.IsInstanceOfType(result, typeof(IFivrDomainsAndConditionWithRails));

            voltageServiceMock.VerifyAll();
            fivr.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetVoltageObject_FivrDomainsAndConditionWithRails_DLVR_InvalidOverrideExpression()
        {
            var fivr = new Mock<IFivrDomainsAndConditionWithRails>(MockBehavior.Strict);
            var voltageServiceMock = new Mock<IVoltageService>(MockBehavior.Strict);
            Prime.Services.VoltageService = voltageServiceMock.Object;

            var pin = new Mock<IPin>(MockBehavior.Strict);
            pin.Setup(o => o.GetVforceMandatoryAttributes()).Returns(new List<string> { "FreeDriveTime" });
            var pinService = new Mock<IPinService>(MockBehavior.Strict);
            pinService.Setup(o => o.Get("dlvr")).Returns(pin.Object);
            Prime.Services.PinService = pinService.Object;

            var testConditionMock = new Mock<ITestCondition>(MockBehavior.Strict);
            testConditionMock.Setup(o => o.GetPinAttributeValue("dlvr", "FreeDriveTime")).Returns("1mS");
            var testConditionServiceMock = new Mock<ITestConditionService>(MockBehavior.Strict);
            testConditionServiceMock.Setup(o => o.GetTestCondition("SomeLevels")).Returns(testConditionMock.Object);
            Prime.Services.TestConditionService = testConditionServiceMock.Object;

            voltageServiceMock.Setup(o => o.CreateFivrDomainsAndConditionWithRails(new List<string> { "DomainA" }, "NOM", "SomePatlist", new List<string> { "powermux", "dlvr" }, It.IsAny<Dictionary<string, Dictionary<string, string>>>())).Returns(fivr.Object);

            var ex = Assert.ThrowsException<ArgumentException>(() => DDG.VoltageHandler.GetVoltageObject(new List<string> { "DomainA" }, "SomeLevels", "SomePatlist", "NOM", null, "--railconfigurations=powermux --dlvrpins=dlvr --expressions=e1 e2", out var options));
            Assert.AreEqual("VoltageConverterBase.dll.CreateFivr: number of DlvrPins must match the number of OverrideExpressions.", ex.Message);

            voltageServiceMock.VerifyAll();
            fivr.VerifyAll();
        }
    }
}
