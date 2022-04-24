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

namespace PTHBgTrimTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.FunctionalService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class PTHBgTrimTC_UnitTest
    {
        private Mock<ISharedStorageService> gsdsServiceMock;
        private Mock<IConsoleService> consoleServiceMock;
        private Mock<IStrgvalFormat> datalogMock;
        private Mock<IDatalogService> datalogServiceMock;
        private Mock<IFunctionalService> funcServiceMock;
        private Mock<ICaptureFailureAndCtvPerCycleTest> ctvPerCycleTestMock;

        /// <summary>
        /// Sets empty params.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.gsdsServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            Prime.Services.SharedStorageService = this.gsdsServiceMock.Object;

            this.funcServiceMock = new Mock<IFunctionalService>(MockBehavior.Strict);
            this.ctvPerCycleTestMock = new Mock<ICaptureFailureAndCtvPerCycleTest>(MockBehavior.Strict);
            Prime.Services.FunctionalService = this.funcServiceMock.Object;

            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            this.datalogMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            this.datalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            Prime.Services.DatalogService = this.datalogServiceMock.Object;
        }

        /// <summary>
        /// Verify missing required parameter throws Exception. False.
        /// </summary>
        [TestMethod]
        public void Verify_Exception_False()
        {
            // [1] Setup the unit test scenario.
            PTHBgTrimTC underTest = new PTHBgTrimTC();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.ThrowsException<NullReferenceException>(() => underTest.Verify());
        }

        /// <summary>
        /// Verify wrong number of CTV pin - only 1 pin being processed. False.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_WrongNumberCTVPin_False()
        {
            // [1] Setup the unit test scenario.
            var plistName = "bgr_trim_plist";
            var lvlName = "univ_lvl_nom";
            var timName = "func_sdr";

            PTHBgTrimTC underTest = new PTHBgTrimTC()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                CtvCapturePins = "TDO,VIEWPIN",
                PatternName = "g1050023F0844370I_QK_VTB044T_Phnm0k3c00da_n040816xx00044xxx1xxxalb_TB0PTfTC002J2ga_LJx0A42x0nxx0000_fivr_bg_trim_tap",
            };

            // [2] Call the method under test.
            this.consoleServiceMock.Setup(console => console.PrintDebug("Expecting exactly one capture pin - TDO pin with correct name.\n"));
            underTest.Verify();
        }

        /// <summary>
        /// Verify empty pattern name. False.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_EmptyPattern_False()
        {
            // [1] Setup the unit test scenario.
            var plistName = "bgr_trim_plist";
            var lvlName = "univ_lvl_nom";
            var timName = "func_sdr";

            PTHBgTrimTC underTest = new PTHBgTrimTC()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                CtvCapturePins = "TDO,",
                PatternName = string.Empty,
                GSDSAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg",
            };

            // [2] Call the method under test.
            this.consoleServiceMock.Setup(console => console.PrintDebug("PatternName / GSDSAvgName cannot be empty.\n"));
            underTest.Verify();
        }

        /// <summary>
        /// Verify missing parameter. False.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_MissingParam_False()
        {
            // [1] Setup the unit test scenario.
            var plistName = "bgr_trim_plist";
            var lvlName = "univ_lvl_nom";
            var timName = "func_sdr";

            PTHBgTrimTC underTest = new PTHBgTrimTC()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                CtvCapturePins = "TDO,",
                PatternName = "g1050023F0844370I_QK_VTB044T_Phnm0k3c00da_n040816xx00044xxx1xxxalb_TB0PTfTC002J2ga_LJx0A42x0nxx0000_fivr_bg_trim_tap",
                GSDSAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg",
                CodeLimitMax = null,
                CodeLimitMin = null,
                CodeLimitRange = null,
            };

            // [2] Call the method under test.
            this.consoleServiceMock.Setup(console => console.PrintDebug("Parameters CodeLimitMax, CodeLimitMin, RangeLimit are all required. They cannot be empty.\n"));
            underTest.Verify();
        }

        /// <summary>
        /// Verify max limit is lower than min limit. False.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Verify_MaxLimitLowerMinLimit_False()
        {
            // [1] Setup the unit test scenario.
            var plistName = "bgr_trim_plist";
            var lvlName = "univ_lvl_nom";
            var timName = "func_sdr";

            PTHBgTrimTC underTest = new PTHBgTrimTC()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                CtvCapturePins = "TDO,",
                PatternName = "g1050023F0844370I_QK_VTB044T_Phnm0k3c00da_n040816xx00044xxx1xxxalb_TB0PTfTC002J2ga_LJx0A42x0nxx0000_fivr_bg_trim_tap",
                GSDSAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg",
                CodeLimitMax = 100,
                CodeLimitMin = 110,
                CodeLimitRange = 10,
            };

            // [2] Call the method under test.
            this.consoleServiceMock.Setup(console => console.PrintDebug("CodeLimitMax cannot be less than CodeLimitMin.\n"));
            underTest.Verify();
        }

        /// <summary>
        /// Execute passes. Port 1.
        /// </summary>
        [TestMethod]
        public void Execute_Pass_Port1()
        {
            // [1] Setup the unit test scenario.
            var plistName = "bgr_trim_plist";
            var lvlName = "univ_lvl_nom";
            var timName = "func_sdr";
            var ctvPinList = new List<string> { "TDO" };
            var patternName = "g1050023F0844370I_QK_VTB044T_Phnm0k3c00da_n040816xx00044xxx1xxxalb_TB0PTfTC002J2ga_LJx0A42x0nxx0000_fivr_bg_trim_tap";
            var gsdsAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg";
            string expectedData;

            PTHBgTrimTC underTest = new PTHBgTrimTC()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                CtvCapturePins = "TDO",
                PatternName = patternName,
                GSDSAvgName = gsdsAvgName,
                CodeLimitMax = 140,
                CodeLimitMin = 80,
                CodeLimitRange = 10,
            };

            this.funcServiceMock.Setup(funcService => funcService.CreateCaptureFailureAndCtvPerCycleTest(plistName, lvlName, timName, ctvPinList, 1000, 1000, It.IsAny<string>())).Returns(this.ctvPerCycleTestMock.Object);
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.ctvPerCycleTestMock.Setup(test => test.ApplyTestConditions());
            this.ctvPerCycleTestMock.Setup(test => test.Execute()).Returns(true);

            // Create sample data.
            List<ICtvPerPattern> ctvDataList = new List<ICtvPerPattern>();
            var dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0111011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            this.ctvPerCycleTestMock.Setup(test => test.GetCtvPerPattern(patternName)).Returns(ctvDataList);

            // Create Ituff matching sample data.
            this.datalogServiceMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(this.datalogMock.Object);
            this.datalogMock.Setup(strVal => strVal.SetTnamePostfix(It.IsAny<string>()));
            expectedData = "0|1|122";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            expectedData = "0|1|123";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            this.datalogServiceMock.Setup(datalog => datalog.WriteToItuff(this.datalogMock.Object));

            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 0, Done: 1, TrimCode: 122.\n"));
            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 0, Done: 1, TrimCode: 123.\n"));

            // Set GSDS.
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.InsertRowAtTable("G.U.I.BGR1_BG_0_0_trim_result_avg", 122, Context.DUT));

            // Create Ituff matching avg, min, max data.
            // Avg and Min values
            expectedData = "122";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Max value
            expectedData = "123";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Range value
            expectedData = "1";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(1, executeResult);
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
        /// Execute passes with stop watch enabled. Port 1.
        /// </summary>
        [TestMethod]
        public void Execute_PassWithStopWatch_Port1()
        {
            // [1] Setup the unit test scenario.
            var plistName = "bgr_trim_plist";
            var lvlName = "univ_lvl_nom";
            var timName = "func_sdr";
            var ctvPinList = new List<string> { "TDO" };
            var patternName = "g1050023F0844370I_QK_VTB044T_Phnm0k3c00da_n040816xx00044xxx1xxxalb_TB0PTfTC002J2ga_LJx0A42x0nxx0000_fivr_bg_trim_tap";
            var gsdsAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg";
            string expectedData;

            PTHBgTrimTC underTest = new PTHBgTrimTC()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                CtvCapturePins = "TDO",
                PatternName = patternName,
                GSDSAvgName = gsdsAvgName,
                CodeLimitMax = 140,
                CodeLimitMin = 80,
                CodeLimitRange = 10,
                EnableStopwatch = 1,
            };

            this.funcServiceMock.Setup(funcService => funcService.CreateCaptureFailureAndCtvPerCycleTest(plistName, lvlName, timName, ctvPinList, 1000, 1000, It.IsAny<string>())).Returns(this.ctvPerCycleTestMock.Object);
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.ctvPerCycleTestMock.Setup(test => test.ApplyTestConditions());
            this.ctvPerCycleTestMock.Setup(test => test.Execute()).Returns(true);

            // Create sample data.
            List<ICtvPerPattern> ctvDataList = new List<ICtvPerPattern>();
            var dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0111011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            this.ctvPerCycleTestMock.Setup(test => test.GetCtvPerPattern(patternName)).Returns(ctvDataList);

            // Create Ituff matching sample data.
            this.datalogServiceMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(this.datalogMock.Object);
            this.datalogMock.Setup(strVal => strVal.SetTnamePostfix(It.IsAny<string>()));
            expectedData = "0|1|122";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            expectedData = "0|1|123";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            this.datalogServiceMock.Setup(datalog => datalog.WriteToItuff(this.datalogMock.Object));

            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 0, Done: 1, TrimCode: 122.\n"));
            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 0, Done: 1, TrimCode: 123.\n"));

            // Set GSDS.
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.InsertRowAtTable("G.U.I.BGR1_BG_0_0_trim_result_avg", 122, Context.DUT));

            // Create Ituff matching avg, min, max data.
            // Avg and Min values.
            expectedData = "122";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Max value.
            expectedData = "123";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Range value.
            expectedData = "1";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Print stopwatch data.
            this.consoleServiceMock.Setup(console => console.PrintDebug(It.Is<string>(x => this.PrintConsole(x) & x.StartsWith("Stopwatch elapsed time: "))));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(1, executeResult);
        }

        /// <summary>
        /// Execute fails: no data captured. Port 3.
        /// </summary>
        [TestMethod]
        public void Execute_Fail_Port3()
        {
            // [1] Setup the unit test scenario.
            var plistName = "bgr_trim_plist";
            var lvlName = "univ_lvl_nom";
            var timName = "func_sdr";
            var ctvPinList = new List<string> { "TDO" };
            var patternName = "g1050023F0844370I_QK_VTB044T_Phnm0k3c00da_n040816xx00044xxx1xxxalb_TB0PTfTC002J2ga_LJx0A42x0nxx0000_fivr_bg_trim_tap";
            var gsdsAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg";

            PTHBgTrimTC underTest = new PTHBgTrimTC()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                CtvCapturePins = "TDO",
                PatternName = patternName,
                GSDSAvgName = gsdsAvgName,
                CodeLimitMax = 140,
                CodeLimitMin = 80,
                CodeLimitRange = 10,
            };

            this.funcServiceMock.Setup(funcService => funcService.CreateCaptureFailureAndCtvPerCycleTest(plistName, lvlName, timName, ctvPinList, 1000, 1000, It.IsAny<string>())).Returns(this.ctvPerCycleTestMock.Object);
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            List<ICtvPerPattern> ctvDataList = new List<ICtvPerPattern>();
            this.ctvPerCycleTestMock.Setup(test => test.ApplyTestConditions());
            this.ctvPerCycleTestMock.Setup(test => test.Execute()).Returns(true);
            this.ctvPerCycleTestMock.Setup(test => test.GetCtvPerPattern(patternName)).Returns(ctvDataList);

            this.datalogMock.Setup(strVal => strVal.SetTnamePostfix(It.IsAny<string>()));
            this.datalogMock.Setup(strVal => strVal.SetData(It.IsAny<string>()));
            this.datalogServiceMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(this.datalogMock.Object);
            this.datalogServiceMock.Setup(datalog => datalog.WriteToItuff(this.datalogMock.Object));

            this.consoleServiceMock.Setup(console => console.PrintDebug("No data captured.\n"));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(3, executeResult);
        }

        /// <summary>
        /// Execute fails: data captured does not match expected length. Port 4.
        /// </summary>
        [TestMethod]
        public void Execute_Fail_Port4()
        {
            // [1] Setup the unit test scenario.
            var plistName = "bgr_trim_plist";
            var lvlName = "univ_lvl_nom";
            var timName = "func_sdr";
            var ctvPinList = new List<string> { "TDO" };
            var patternName = "g1050023F0844370I_QK_VTB044T_Phnm0k3c00da_n040816xx00044xxx1xxxalb_TB0PTfTC002J2ga_LJx0A42x0nxx0000_fivr_bg_trim_tap";
            var gsdsAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg";

            PTHBgTrimTC underTest = new PTHBgTrimTC()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                CtvCapturePins = "TDO",
                PatternName = patternName,
                GSDSAvgName = gsdsAvgName,
                CodeLimitMax = 140,
                CodeLimitMin = 80,
                CodeLimitRange = 10,
            };

            this.funcServiceMock.Setup(funcService => funcService.CreateCaptureFailureAndCtvPerCycleTest(plistName, lvlName, timName, ctvPinList, 1000, 1000, It.IsAny<string>())).Returns(this.ctvPerCycleTestMock.Object);
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.ctvPerCycleTestMock.Setup(test => test.ApplyTestConditions());
            this.ctvPerCycleTestMock.Setup(test => test.Execute()).Returns(true);

            // Create sample data
            List<ICtvPerPattern> ctvDataList = new List<ICtvPerPattern>();
            var dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("101011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0111011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            this.ctvPerCycleTestMock.Setup(test => test.GetCtvPerPattern(patternName)).Returns(ctvDataList);

            this.consoleServiceMock.Setup(console => console.PrintDebug(
                "Expected length of CTV data for g1050023F0844370I_QK_VTB044T_Phnm0k3c00da_n040816xx00044xxx1xxxalb_TB0PTfTC002J2ga_LJx0A42x0nxx0000_fivr_bg_trim_tap to be 10, captured 9.\n"));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(4, executeResult);
        }

        /// <summary>
        /// Execute fails: error bit is 1. Port 5.
        /// </summary>
        [TestMethod]
        public void Execute_FailErrorBit_Port5()
        {
            // [1] Setup the unit test scenario.
            var plistName = "bgr_trim_plist";
            var lvlName = "univ_lvl_nom";
            var timName = "func_sdr";
            var ctvPinList = new List<string> { "TDO" };
            var patternName = "g1050023F0844370I_QK_VTB044T_Phnm0k3c00da_n040816xx00044xxx1xxxalb_TB0PTfTC002J2ga_LJx0A42x0nxx0000_fivr_bg_trim_tap";
            var gsdsAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg";
            string expectedData;

            PTHBgTrimTC underTest = new PTHBgTrimTC()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                CtvCapturePins = "TDO",
                PatternName = patternName,
                GSDSAvgName = gsdsAvgName,
                CodeLimitMax = 140,
                CodeLimitMin = 80,
                CodeLimitRange = 10,
            };

            this.funcServiceMock.Setup(funcService => funcService.CreateCaptureFailureAndCtvPerCycleTest(plistName, lvlName, timName, ctvPinList, 1000, 1000, It.IsAny<string>())).Returns(this.ctvPerCycleTestMock.Object);
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.ctvPerCycleTestMock.Setup(test => test.ApplyTestConditions());
            this.ctvPerCycleTestMock.Setup(test => test.Execute()).Returns(true);

            // Create sample data
            List<ICtvPerPattern> ctvDataList = new List<ICtvPerPattern>();
            var dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("1111011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            this.ctvPerCycleTestMock.Setup(test => test.GetCtvPerPattern(patternName)).Returns(ctvDataList);

            // Create Ituff matching sample data
            this.datalogServiceMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(this.datalogMock.Object);
            this.datalogMock.Setup(strVal => strVal.SetTnamePostfix(It.IsAny<string>()));
            expectedData = "0|1|122";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            expectedData = "1|1|123";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            this.datalogServiceMock.Setup(datalog => datalog.WriteToItuff(this.datalogMock.Object));

            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 0, Done: 1, TrimCode: 122.\n"));
            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 1, Done: 1, TrimCode: 123.\n"));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(5, executeResult);
        }

        /// <summary>
        /// Execute fails: done bit is 0. Port 5.
        /// </summary>
        [TestMethod]
        public void Execute_FailDoneBit_Port5()
        {
            // [1] Setup the unit test scenario.
            var plistName = "bgr_trim_plist";
            var lvlName = "univ_lvl_nom";
            var timName = "func_sdr";
            var ctvPinList = new List<string> { "TDO" };
            var patternName = "g1050023F0844370I_QK_VTB044T_Phnm0k3c00da_n040816xx00044xxx1xxxalb_TB0PTfTC002J2ga_LJx0A42x0nxx0000_fivr_bg_trim_tap";
            var gsdsAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg";
            string expectedData;

            PTHBgTrimTC underTest = new PTHBgTrimTC()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                CtvCapturePins = "TDO",
                PatternName = patternName,
                GSDSAvgName = gsdsAvgName,
                CodeLimitMax = 140,
                CodeLimitMin = 80,
                CodeLimitRange = 10,
            };

            this.funcServiceMock.Setup(funcService => funcService.CreateCaptureFailureAndCtvPerCycleTest(plistName, lvlName, timName, ctvPinList, 1000, 1000, It.IsAny<string>())).Returns(this.ctvPerCycleTestMock.Object);
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.ctvPerCycleTestMock.Setup(test => test.ApplyTestConditions());
            this.ctvPerCycleTestMock.Setup(test => test.Execute()).Returns(true);

            // Create sample data
            List<ICtvPerPattern> ctvDataList = new List<ICtvPerPattern>();
            var dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0011011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            this.ctvPerCycleTestMock.Setup(test => test.GetCtvPerPattern(patternName)).Returns(ctvDataList);

            // Create Ituff matching sample data
            this.datalogServiceMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(this.datalogMock.Object);
            this.datalogMock.Setup(strVal => strVal.SetTnamePostfix(It.IsAny<string>()));
            expectedData = "0|1|122";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            expectedData = "0|0|123";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            this.datalogServiceMock.Setup(datalog => datalog.WriteToItuff(this.datalogMock.Object));

            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 0, Done: 1, TrimCode: 122.\n"));
            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 0, Done: 0, TrimCode: 123.\n"));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(5, executeResult);
        }

        /// <summary>
        /// Execute fails: max limit violated. Port 6.
        /// </summary>
        [TestMethod]
        public void Execute_FailMaxLimit_Port6()
        {
            // [1] Setup the unit test scenario.
            var plistName = "bgr_trim_plist";
            var lvlName = "univ_lvl_nom";
            var timName = "func_sdr";
            var ctvPinList = new List<string> { "TDO" };
            var patternName = "g1050023F0844370I_QK_VTB044T_Phnm0k3c00da_n040816xx00044xxx1xxxalb_TB0PTfTC002J2ga_LJx0A42x0nxx0000_fivr_bg_trim_tap";
            var gsdsAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg";
            string expectedData;

            PTHBgTrimTC underTest = new PTHBgTrimTC()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                CtvCapturePins = "TDO",
                PatternName = patternName,
                GSDSAvgName = gsdsAvgName,
                CodeLimitMax = 120,
                CodeLimitMin = 80,
                CodeLimitRange = 10,
            };

            this.funcServiceMock.Setup(funcService => funcService.CreateCaptureFailureAndCtvPerCycleTest(plistName, lvlName, timName, ctvPinList, 1000, 1000, It.IsAny<string>())).Returns(this.ctvPerCycleTestMock.Object);
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.ctvPerCycleTestMock.Setup(test => test.ApplyTestConditions());
            this.ctvPerCycleTestMock.Setup(test => test.Execute()).Returns(true);

            // Create sample data
            List<ICtvPerPattern> ctvDataList = new List<ICtvPerPattern>();
            var dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0111011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            this.ctvPerCycleTestMock.Setup(test => test.GetCtvPerPattern(patternName)).Returns(ctvDataList);

            // Create Ituff matching sample data
            this.datalogServiceMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(this.datalogMock.Object);
            this.datalogMock.Setup(strVal => strVal.SetTnamePostfix(It.IsAny<string>()));
            expectedData = "0|1|122";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            expectedData = "0|1|123";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            this.datalogServiceMock.Setup(datalog => datalog.WriteToItuff(this.datalogMock.Object));

            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 0, Done: 1, TrimCode: 122.\n"));
            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 0, Done: 1, TrimCode: 123.\n"));

            // Set GSDS.
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.InsertRowAtTable("G.U.I.BGR1_BG_0_0_trim_result_avg", 122, Context.DUT));

            // Create Ituff matching avg, min, max data.
            // Avg and Min values.
            expectedData = "122";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Max value.
            expectedData = "123";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Range value.
            expectedData = "1";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Debug message for failure.
            this.consoleServiceMock.Setup(console => console.PrintDebug("Failing max limit. 122 > 120.\n"));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(6, executeResult);
        }

        /// <summary>
        /// Execute fails: min limit violated. Port 7.
        /// </summary>
        [TestMethod]
        public void Execute_FailMinLimit_Port7()
        {
            // [1] Setup the unit test scenario.
            var plistName = "bgr_trim_plist";
            var lvlName = "univ_lvl_nom";
            var timName = "func_sdr";
            var ctvPinList = new List<string> { "TDO" };
            var patternName = "g1050023F0844370I_QK_VTB044T_Phnm0k3c00da_n040816xx00044xxx1xxxalb_TB0PTfTC002J2ga_LJx0A42x0nxx0000_fivr_bg_trim_tap";
            var gsdsAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg";
            string expectedData;

            PTHBgTrimTC underTest = new PTHBgTrimTC()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                CtvCapturePins = "TDO",
                PatternName = patternName,
                GSDSAvgName = gsdsAvgName,
                CodeLimitMax = 130,
                CodeLimitMin = 125,
                CodeLimitRange = 10,
            };

            this.funcServiceMock.Setup(funcService => funcService.CreateCaptureFailureAndCtvPerCycleTest(plistName, lvlName, timName, ctvPinList, 1000, 1000, It.IsAny<string>())).Returns(this.ctvPerCycleTestMock.Object);
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.ctvPerCycleTestMock.Setup(test => test.ApplyTestConditions());
            this.ctvPerCycleTestMock.Setup(test => test.Execute()).Returns(true);

            // Create sample data
            List<ICtvPerPattern> ctvDataList = new List<ICtvPerPattern>();
            var dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0111011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            this.ctvPerCycleTestMock.Setup(test => test.GetCtvPerPattern(patternName)).Returns(ctvDataList);

            // Create Ituff matching sample data
            this.datalogServiceMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(this.datalogMock.Object);
            this.datalogMock.Setup(strVal => strVal.SetTnamePostfix(It.IsAny<string>()));
            expectedData = "0|1|122";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            expectedData = "0|1|123";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            this.datalogServiceMock.Setup(datalog => datalog.WriteToItuff(this.datalogMock.Object));

            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 0, Done: 1, TrimCode: 122.\n"));
            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 0, Done: 1, TrimCode: 123.\n"));

            // Set GSDS.
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.InsertRowAtTable("G.U.I.BGR1_BG_0_0_trim_result_avg", 122, Context.DUT));

            // Create Ituff matching avg, min, max data.
            // Avg and Min values.
            expectedData = "122";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Max value.
            expectedData = "123";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Range value.
            expectedData = "1";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Debug message for failure.
            this.consoleServiceMock.Setup(console => console.PrintDebug("Failing min limit. 122 < 125.\n"));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(7, executeResult);
        }

        /// <summary>
        /// Execute fails: range limit violated. Port 8.
        /// </summary>
        [TestMethod]
        public void Execute_FailRangeLimit_Port8()
        {
            // [1] Setup the unit test scenario.
            var plistName = "bgr_trim_plist";
            var lvlName = "univ_lvl_nom";
            var timName = "func_sdr";
            var ctvPinList = new List<string> { "TDO" };
            var patternName = "g1050023F0844370I_QK_VTB044T_Phnm0k3c00da_n040816xx00044xxx1xxxalb_TB0PTfTC002J2ga_LJx0A42x0nxx0000_fivr_bg_trim_tap";
            var gsdsAvgName = "G.U.I.BGR1_BG_0_0_trim_result_avg";
            string expectedData;

            PTHBgTrimTC underTest = new PTHBgTrimTC()
            {
                Patlist = plistName,
                LevelsTc = lvlName,
                TimingsTc = timName,
                CtvCapturePins = "TDO",
                PatternName = patternName,
                GSDSAvgName = gsdsAvgName,
                CodeLimitMax = 130,
                CodeLimitMin = 120,
                CodeLimitRange = 0,
            };

            this.funcServiceMock.Setup(funcService => funcService.CreateCaptureFailureAndCtvPerCycleTest(plistName, lvlName, timName, ctvPinList, 1000, 1000, It.IsAny<string>())).Returns(this.ctvPerCycleTestMock.Object);
            underTest.Verify();

            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            this.ctvPerCycleTestMock.Setup(test => test.ApplyTestConditions());
            this.ctvPerCycleTestMock.Setup(test => test.Execute()).Returns(true);

            // Create sample data
            List<ICtvPerPattern> ctvDataList = new List<ICtvPerPattern>();
            var dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0111011110");
            ctvDataList.Add(dataPattern0.Object);
            dataPattern0 = new Mock<ICtvPerPattern>();
            dataPattern0.Setup(x => x.GetCtvData("TDO")).Returns("0101011110");
            ctvDataList.Add(dataPattern0.Object);
            this.ctvPerCycleTestMock.Setup(test => test.GetCtvPerPattern(patternName)).Returns(ctvDataList);

            // Create Ituff matching sample data
            this.datalogServiceMock.Setup(datalog => datalog.GetItuffStrgvalWriter()).Returns(this.datalogMock.Object);
            this.datalogMock.Setup(strVal => strVal.SetTnamePostfix(It.IsAny<string>()));
            expectedData = "0|1|122";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            expectedData = "0|1|123";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));
            this.datalogServiceMock.Setup(datalog => datalog.WriteToItuff(this.datalogMock.Object));

            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 0, Done: 1, TrimCode: 122.\n"));
            this.consoleServiceMock.Setup(console => console.PrintDebug("Error: 0, Done: 1, TrimCode: 123.\n"));

            // Set GSDS.
            this.gsdsServiceMock.Setup(gsdsService => gsdsService.InsertRowAtTable("G.U.I.BGR1_BG_0_0_trim_result_avg", 122, Context.DUT));

            // Create Ituff matching avg, min, max data.
            // Avg and Min values.
            expectedData = "122";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Max value.
            expectedData = "123";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Range value.
            expectedData = "1";
            this.datalogMock.Setup(strVal => strVal.SetData(expectedData));

            // Debug message for failure.
            this.consoleServiceMock.Setup(console => console.PrintDebug("Failing range limit. 1 > 0.\n"));

            // [2] Call the method under test.
            int executeResult = underTest.Execute();

            // [3] Verifies the results of the Method under test, and that any mock setup on [1].
            Assert.AreEqual(8, executeResult);
        }
    }
}
