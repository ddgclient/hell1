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

namespace FIVRDACFitTC
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.DffService;
    using Prime.SharedStorageService;
    using Prime.TpSettingsService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class FIVRDACFitTC_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FIVRDACFitTC_UnitTest"/> class.
        /// </summary>
        public FIVRDACFitTC_UnitTest()
        {
            this.DFFValues = new Dictionary<string, string>();
            this.GSDSValues = new Dictionary<string, double>();

            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => { Console.WriteLine(s); });
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => { Console.WriteLine($"ERROR: {msg}"); });
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;

            var ituffStrgvalMock = new Mock<IStrgaltFormat>(MockBehavior.Strict);
            ituffStrgvalMock.Setup(o => o.SetCustomTname(It.IsAny<string>()));
            ituffStrgvalMock.Setup(o => o.SetData(It.IsAny<string>(), It.IsAny<string>()));

            var datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            var ituffSepMock = new Mock<ISeparatorFormat>(MockBehavior.Strict);
            datalogServiceMock.Setup(o => o.GetItuffSeparatorFormatWriter()).Returns(ituffSepMock.Object);
            datalogServiceMock.Setup(o => o.GetItuffStrgaltWriter()).Returns(ituffStrgvalMock.Object);
            datalogServiceMock.Setup(o => o.WriteToItuff(ituffSepMock.Object));
            datalogServiceMock.Setup(o => o.WriteToItuff(ituffStrgvalMock.Object));
            Prime.Services.DatalogService = datalogServiceMock.Object;

            this.DffServiceMock = new Mock<IDffService>(MockBehavior.Strict);
            this.DffServiceMock.Setup(o => o.SetDff(It.IsAny<string>(), It.IsAny<string>())).Callback((string key, string value) =>
            {
                Console.WriteLine($"[DFF] {key} = {value}");
                this.DFFValues[key] = value;
            });
            this.DffServiceMock.Setup(o => o.GetDffByOpType(It.IsAny<string>(), It.IsAny<string>(), true))
                .Returns((string key, string opType, bool log) => this.DFFValues[key])
                .Callback((string key, string opType, bool log) =>
                {
                    Console.WriteLine($"[DFF] Reading {key} from {opType}");
                });
            Prime.Services.DffService = this.DffServiceMock.Object;

            this.SharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.SharedServiceMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<double>(), Context.DUT)).Callback((string key, double value, Context context) =>
            {
                Console.WriteLine($"[GSDS] {key} = {value}");
                this.GSDSValues[key] = value;
            });
            this.SharedServiceMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<int>(), Context.DUT)).Callback((string key, int value, Context context) =>
            {
                Console.WriteLine($"[GSDS] {key} = {value}");
                this.GSDSValues[key] = value;
            });
            this.SharedServiceMock.Setup(o => o.GetDoubleRowFromTable(It.IsAny<string>(), Context.DUT))
                .Returns((string key, Context context) => this.GSDSValues[key])
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"[GSDS] Reading {key}.");
                });
            Prime.Services.SharedStorageService = this.SharedServiceMock.Object;

            Console.WriteLine("Done with constructor");
        }

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private Mock<IDffService> DffServiceMock { get; set; }

        private Mock<ISharedStorageService> SharedServiceMock { get; set; }

        private Dictionary<string, double> GSDSValues { get; set; }

        private Dictionary<string, string> DFFValues { get; set; }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_ParamEmpty_False()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => { Console.WriteLine($"ERROR: {msg}"); });
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            FIVRDACFitTC underTest = new FIVRDACFitTC { OpType = string.Empty, Location = FIVRDACFitTC.Temperature.NOT_HOT };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamNoEmpty_True()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            FIVRDACFitTC underTest = new FIVRDACFitTC { Location = FIVRDACFitTC.Temperature.HOT };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void FIVRDACFitCalculate_Exception()
        {
            var obj = new FivrOps();

            try
            {
                // since no gsds is populated, this should throw an exception.
                var rslt = obj.FIVRDACFitCalculate(printDFF: true);
                Assert.Fail("Exception not thown.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                Assert.IsTrue(true, "Correctly threw an assertion.");
            }
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void FIVRDACFitCalculate_Pass()
        {
            var obj = new FivrOps();

            // put dummy data in the GSDS tokens so we can get a pass result.
            foreach (var domain in obj.AllDomains)
            {
                var x = 1;
                foreach (var gsdskey in obj.GSDSKeys[domain].AMVolts)
                {
                    this.GSDSValues[gsdskey] = x++;
                }

                var y = 2;
                foreach (var gsdskey in obj.GSDSKeys[domain].CMEMVidCodes)
                {
                    this.GSDSValues[gsdskey] = 15 * y++;
                }
            }

            var rslt = obj.FIVRDACFitCalculate(printDFF: true);
            Assert.IsTrue(rslt);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void FIVRDACFitCalculate_Fail()
        {
            var obj = new FivrOps();

            // put dummy data in the GSDS tokens so we can get a pass result.
            foreach (var domain in obj.AllDomains)
            {
                var x = 1;
                foreach (var gsdskey in obj.GSDSKeys[domain].AMVolts)
                {
                    this.GSDSValues[gsdskey] = x++;
                }

                var y = 1;
                foreach (var gsdskey in obj.GSDSKeys[domain].CMEMVidCodes)
                {
                    this.GSDSValues[gsdskey] = y++;
                }
            }

            var rslt = obj.FIVRDACFitCalculate(printDFF: true);
            Assert.IsFalse(rslt);
        }
    }
}
