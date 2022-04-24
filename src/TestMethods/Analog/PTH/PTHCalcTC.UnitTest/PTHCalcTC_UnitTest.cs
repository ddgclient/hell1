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

namespace PTHCalcTC.UnitTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.SharedStorageService;

    /// <summary>
    /// Description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class PTHCalcTC_UnitTest
    {
        private Mock<ISharedStorageService> gsdsServiceMock;
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IStrgvalFormat> datalogMock;
        private Mock<IDatalogService> datalogServiceMock;

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.gsdsServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = this.gsdsServiceMock.Object;

            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            this.datalogMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            this.datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            Prime.Services.DatalogService = this.datalogServiceMock.Object;
        }

        /// <summary>
        /// Verify missing required parameter failed. False.
        /// </summary>
        [TestMethod]
        public void Verify_Fail_False()
        {
            // [1] Setup the unit test scenario.
            PTHCalcTC underTest = new PTHCalcTC()
            {
                CodeLimitMin = 0,
                CodeLimitMax = 255,
                RangeLimit = 10,
            };

            // [2] Call the method under test.
            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.ThrowsException<Prime.Base.Exceptions.FatalException>(() => underTest.Verify());
        }

        /// <summary>
        /// Function to print to console and return true.
        /// Created to be used in lambda function for validating result of mocking print to console.
        /// </summary>
        /// <param name="s">String to print to console.</param>
        /// <returns>true.</returns>
        public bool PrintConsole(string s)
        {
            Console.WriteLine(s);
            return true;
        }

        /// <summary>
        /// Execute passes. Port 1.
        /// </summary>
        [TestMethod]
        public void Execute_Pass_Port1()
        {
            // [1] Setup the unit test scenario.
            PTHCalcTC underTest = new PTHCalcTC()
            {
                ListGSDSInputNames = "G.U.I.BGR1_BG_0_0_trim_result_0,G.U.I.BGR1_BG_0_0_trim_result_1,G.U.I.BGR1_BG_0_0_trim_result_2",
                GSDSAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg",
                CodeLimitMin = 0,
                CodeLimitMax = 255,
                RangeLimit = 10,
            };
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_0", Context.DUT)).Returns(128);
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_1", Context.DUT)).Returns(127);
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_2", Context.DUT)).Returns(128);

            // Mock output GSDS and debug message
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.InsertRowAtTable("G.U.I.BGR1_BG_0_0_trim_result_avg", 128, Context.DUT));
            this.consoleServiceMock.Setup(console => console.PrintDebug(It.Is<string>(x => this.PrintConsole(x) & x.StartsWith("Stopwatch elapsed time: "))));

            this.datalogMock.Setup(strVal => strVal.SetTnamePostfix(It.IsAny<string>()));
            this.datalogMock.Setup(strVal => strVal.SetData(It.IsAny<string>()));
            this.datalogServiceMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(this.datalogMock.Object);
            this.datalogServiceMock.Setup(datalog => datalog.WriteToItuff(this.datalogMock.Object));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(1, executeResult);
        }

        /// <summary>
        /// Execute fails because input value is bad. Port 0.
        /// </summary>
        [TestMethod]
        public void Execute_FailInvalidInput_Port0()
        {
            // [1] Setup the unit test scenario.
            PTHCalcTC underTest = new PTHCalcTC()
            {
                ListGSDSInputNames = "G.U.I.BGR1_BG_0_0_trim_result_0,G.U.I.BGR1_BG_0_0_trim_result_1,G.U.I.BGR1_BG_0_0_trim_result_2",
                GSDSAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg",
                CodeLimitMin = 0,
                CodeLimitMax = 255,
                RangeLimit = 10,
            };
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_0", Context.DUT)).Returns(0);
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_1", Context.DUT)).Returns(127);
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_2", Context.DUT)).Returns(128);

            // Mock output GSDS and debug message
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.InsertRowAtTable("G.U.I.BGR1_BG_0_0_trim_result_avg", -999, Context.DUT));
            this.consoleServiceMock.Setup(console => console.PrintDebug("Issues when processing GSDS input: G.U.I.BGR1_BG_0_0_trim_result_0.\n"));
            this.consoleServiceMock.Setup(console => console.PrintDebug(It.Is<string>(x => this.PrintConsole(x) & x.StartsWith("Stopwatch elapsed time: "))));

            this.datalogMock.Setup(strVal => strVal.SetData(It.IsAny<string>()));
            this.datalogMock.Setup(strVal => strVal.SetTnamePostfix(It.IsAny<string>()));
            this.datalogServiceMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(this.datalogMock.Object);
            this.datalogServiceMock.Setup(datalog => datalog.WriteToItuff(this.datalogMock.Object));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(0, executeResult);
        }

        /// <summary>
        /// Execute fails because over max limit. Port 3.
        /// </summary>
        [TestMethod]
        public void Execute_FailInvalidInput_Port3()
        {
            // [1] Setup the unit test scenario.
            PTHCalcTC underTest = new PTHCalcTC()
            {
                ListGSDSInputNames = "G.U.I.BGR1_BG_0_0_trim_result_0,G.U.I.BGR1_BG_0_0_trim_result_1,G.U.I.BGR1_BG_0_0_trim_result_2",
                GSDSAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg",
                CodeLimitMin = 0,
                CodeLimitMax = 120,
                RangeLimit = 10,
            };
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_0", Context.DUT)).Returns(128);
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_1", Context.DUT)).Returns(127);
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_2", Context.DUT)).Returns(128);

            // Mock output GSDS and debug message
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.InsertRowAtTable("G.U.I.BGR1_BG_0_0_trim_result_avg", 128, Context.DUT));
            this.consoleServiceMock.Setup(console => console.PrintDebug("Failing max limit. 128 > 120.\n"));
            this.consoleServiceMock.Setup(console => console.PrintDebug(It.Is<string>(x => this.PrintConsole(x) & x.StartsWith("Stopwatch elapsed time: "))));

            this.datalogMock.Setup(strVal => strVal.SetData(It.IsAny<string>()));
            this.datalogMock.Setup(strVal => strVal.SetTnamePostfix(It.IsAny<string>()));
            this.datalogServiceMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(this.datalogMock.Object);
            this.datalogServiceMock.Setup(datalog => datalog.WriteToItuff(this.datalogMock.Object));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(3, executeResult);
        }

        /// <summary>
        /// Execute fails because over min limit. Port 4.
        /// </summary>
        [TestMethod]
        public void Execute_FailInvalidInput_Port4()
        {
            // [1] Setup the unit test scenario.
            PTHCalcTC underTest = new PTHCalcTC()
            {
                ListGSDSInputNames = "G.U.I.BGR1_BG_0_0_trim_result_0,G.U.I.BGR1_BG_0_0_trim_result_1,G.U.I.BGR1_BG_0_0_trim_result_2",
                GSDSAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg",
                CodeLimitMin = 125,
                CodeLimitMax = 128,
                RangeLimit = 10,
            };
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_0", Context.DUT)).Returns(123);
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_1", Context.DUT)).Returns(124);
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_2", Context.DUT)).Returns(124);

            // Mock output GSDS and debug message
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.InsertRowAtTable("G.U.I.BGR1_BG_0_0_trim_result_avg", 124, Context.DUT));
            this.consoleServiceMock.Setup(console => console.PrintDebug("Failing min limit. 124 < 125.\n"));
            this.consoleServiceMock.Setup(console => console.PrintDebug(It.Is<string>(x => this.PrintConsole(x) & x.StartsWith("Stopwatch elapsed time: "))));

            this.datalogMock.Setup(strVal => strVal.SetData(It.IsAny<string>()));
            this.datalogMock.Setup(strVal => strVal.SetTnamePostfix(It.IsAny<string>()));
            this.datalogServiceMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(this.datalogMock.Object);
            this.datalogServiceMock.Setup(datalog => datalog.WriteToItuff(this.datalogMock.Object));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(4, executeResult);
        }

        /// <summary>
        /// Execute fails because range over limit. Port 5.
        /// </summary>
        [TestMethod]
        public void Execute_FailInvalidInput_Port5()
        {
            // [1] Setup the unit test scenario.
            PTHCalcTC underTest = new PTHCalcTC()
            {
                ListGSDSInputNames = "G.U.I.BGR1_BG_0_0_trim_result_0,G.U.I.BGR1_BG_0_0_trim_result_1,G.U.I.BGR1_BG_0_0_trim_result_2",
                GSDSAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg",
                CodeLimitMin = 100,
                CodeLimitMax = 128,
                RangeLimit = 10,
            };
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_0", Context.DUT)).Returns(113);
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_1", Context.DUT)).Returns(124);
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.GetIntegerRowFromTable("G.U.I.BGR1_BG_0_0_trim_result_2", Context.DUT)).Returns(124);

            // Mock output GSDS and debug message
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.InsertRowAtTable("G.U.I.BGR1_BG_0_0_trim_result_avg", 120, Context.DUT));
            this.consoleServiceMock.Setup(console => console.PrintDebug("Failing range limit. 11 > 10.\n"));
            this.consoleServiceMock.Setup(console => console.PrintDebug(It.Is<string>(x => this.PrintConsole(x) & x.StartsWith("Stopwatch elapsed time: "))));

            this.datalogMock.Setup(strVal => strVal.SetData(It.IsAny<string>()));
            this.datalogMock.Setup(strVal => strVal.SetTnamePostfix(It.IsAny<string>()));
            this.datalogServiceMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(this.datalogMock.Object);
            this.datalogServiceMock.Setup(datalog => datalog.WriteToItuff(this.datalogMock.Object));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(5, executeResult);
        }
    }
}
