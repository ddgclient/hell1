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

namespace FIVRTrimTC
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
    public class FivrTrimTC_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FivrTrimTC_UnitTest"/> class.
        /// </summary>
        public FivrTrimTC_UnitTest()
        {
            this.DFFValues = new Dictionary<string, string>();
            this.GSDSValues = new Dictionary<string, int>();

            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => { Console.WriteLine(s); });
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) => { Console.WriteLine($"ERROR: {msg}"); });
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;

            var ituffStrgvalMock = new Mock<IStrgaltFormat>(MockBehavior.Strict);
            ituffStrgvalMock.Setup(o => o.SetCustomTname(It.IsAny<string>()));
            ituffStrgvalMock.Setup(o => o.SetData("ssp", It.IsAny<string>()));

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
            this.SharedServiceMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<int>(), Context.DUT)).Callback((string key, int value, Context context) =>
            {
                Console.WriteLine($"[GSDS] {key} = {value}");
                this.GSDSValues[key] = value;
            });
            this.SharedServiceMock.Setup(o => o.GetIntegerRowFromTable(It.IsAny<string>(), Context.DUT))
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

        private Dictionary<string, int> GSDSValues { get; set; }

        private Dictionary<string, string> DFFValues { get; set; }

        /// <summary>
        /// populate gsds tokens (use default values from the FivrTrim Data).
        /// </summary>
        /// <param name="obj">FivrTrim object.</param>
        public void PopulateDefaultGSDS(FivrTrim obj)
        {
            foreach (var domain in obj.FivrData.Keys)
            {
                foreach (var trimType in obj.FivrData[domain].Trims.Keys)
                {
                    var trimObj = obj.FivrData[domain].Trims[trimType];
                    if (trimObj.HasPhases)
                    {
                        for (var phaseNum = 0; phaseNum < trimObj.NumPhases; phaseNum++)
                        {
                            for (var sampleNum = 0; sampleNum < 4; sampleNum++)
                            {
                                this.GSDSValues[$"PCH_{domain}_{trimType}_{phaseNum}_trim_result_{sampleNum}"] = trimObj.DefaultVal;
                                this.GSDSValues[$"PCH_{domain}_{trimType}_trim_done_{sampleNum}"] = 1;
                                this.GSDSValues[$"PCH_{domain}_{trimType}_{phaseNum}_trim_error_{sampleNum}"] = 0;
                            }
                        }
                    }
                    else
                    {
                        for (var sampleNum = 0; sampleNum < 4; sampleNum++)
                        {
                            this.GSDSValues[$"PCH_{domain}_{trimType}_trim_result_{sampleNum}"] = trimObj.DefaultVal;
                            this.GSDSValues[$"PCH_{domain}_{trimType}_trim_done_{sampleNum}"] = 1;
                            this.GSDSValues[$"PCH_{domain}_{trimType}_trim_error_{sampleNum}"] = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_ParamEmpty_True()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            FIVRTrimTC underTest = new FIVRTrimTC { };

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
            FIVRTrimTC underTest = new FIVRTrimTC
            {
                EnableDFF = FIVRTrimTC.MyBool.False,
                IsSort = FIVRTrimTC.MyBool.True,
                EnableGSDS = FIVRTrimTC.MyBool.False,
                AltTagID = "SOMESTRING",
                EnableTrimKill = FIVRTrimTC.MyBool.False,
            };

            // [2] Call the method under test.
            underTest.Verify();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void FIVRTrimCalc_GSDS_Exception()
        {
            var obj = new FivrTrim(scDeltaEn: false, dffEn: false, isSort: false, gsdsEn: true, altTagID: string.Empty, trimKill: true, debug: true);

            try
            {
                // since no gsds is populated, this should throw an e
                var rslt = obj.FIVRTrimCalc();
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
        public void FIVRTrimCalc_NoSCDelta_Pass()
        {
            var obj = new FivrTrim(scDeltaEn: false, dffEn: true, isSort: false, gsdsEn: true, altTagID: string.Empty, trimKill: true, debug: true);
            this.PopulateDefaultGSDS(obj);

            var rslt = obj.FIVRTrimCalc();
            Assert.AreEqual(1, rslt);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void FIVRTrimCalc_NoSCDelta_Fail_BGRange()
        {
            var obj = new FivrTrim(scDeltaEn: false, dffEn: true, isSort: false, gsdsEn: true, altTagID: string.Empty, trimKill: true, debug: true);
            this.PopulateDefaultGSDS(obj);

            // change two BG samples so the range is out of bounds
            this.GSDSValues[$"PCH_BGR_BG_trim_result_0"] = 10;
            this.GSDSValues[$"PCH_BGR_BG_trim_result_0"] = 200;

            var rslt = obj.FIVRTrimCalc();
            Assert.AreEqual(2, rslt);
        }
    }
}
