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

namespace SocRecoveryCallbacks.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.SharedStorageService;
    using Prime.TestProgramService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class SocRecoveryCallbacks_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocRecoveryCallbacks_UnitTest"/> class.
        /// </summary>
        public SocRecoveryCallbacks_UnitTest()
        {
            this.SharedStorageValues = new Dictionary<string, string>();
            this.ItuffOutput = new List<string>();

            // Default Mock for console service.
            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                Console.WriteLine(s);
            });
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string n, string src) =>
                {
                    Console.WriteLine($"ERROR: {msg}");
                });
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;

            // Default Mock for Callback service.
            this.TestProgramServiceMock = new Mock<ITestProgramService>(MockBehavior.Strict);
            this.TestProgramServiceMock.Setup(o => o.GetCurrentTestInstanceName()).Returns(TestInstanceName);
            this.TestProgramServiceMock.Setup(o => o.GetCurrentTestInstanceParameter("LogLevel")).Returns("DISABLED");
            Prime.Services.TestProgramService = this.TestProgramServiceMock.Object;

            // Default Mock for Shared service.
            this.SharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), Prime.SharedStorageService.Context.DUT))
                .Callback((string key, object obj, Context context) =>
                {
                    Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                });
            this.SharedStorageMock.Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), Prime.SharedStorageService.Context.DUT))
                .Callback((string key, Type obj, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                })
                .Returns((string key, Type obj, Context context) => JsonConvert.DeserializeObject(this.SharedStorageValues[key], obj));
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => this.SharedStorageValues[key]);
            Prime.Services.SharedStorageService = this.SharedStorageMock.Object;

            // Mrslt Ituff format mock
            this.MrsltFormatMock = new Mock<IMrsltFormat>(MockBehavior.Strict);
            this.MrsltFormatMock.Setup(o => o.SetData(It.IsAny<double>())).Callback((double s) =>
            {
                this.CurrentMrsltvalData = s;
            });
            this.MrsltFormatMock.Setup(o => o.SetTnamePrefix(It.IsAny<string>())).Callback((string s) =>
            {
                this.CurrentMrsltvalName = s;
            });
            this.MrsltFormatMock.Setup(o => o.SetTnamePostfix(It.IsAny<string>())).Callback((string s) =>
            {
                this.CurrentMrsltvalName = s;
            });
            this.MrsltFormatMock.Setup(o => o.SetPrecision(It.IsAny<uint>()));

            // Strgval mock ituff format writer
            this.StrgvalFormatMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            this.StrgvalFormatMock.Setup(o => o.SetData(It.IsAny<string>())).Callback((string s) =>
            {
                this.CurrentStrgvalData = s;
            });
            this.StrgvalFormatMock.Setup(o => o.SetTnamePostfix(It.IsAny<string>())).Callback((string s) =>
            {
                this.CurrentStrgvalName = s;
            });
            this.StrgvalFormatMock.Setup(o => o.SetTnamePrefix(It.IsAny<string>())).Callback((string s) =>
            {
                this.CurrentStrgvalName = s;
            });

            this.DatalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            this.DatalogServiceMock.Setup(o => o.WriteToItuff(It.IsAny<IMrsltFormat>())).Callback((IItuffFormat s) =>
            {
                var txt = "0_tname_";
                if (!string.IsNullOrEmpty(this.CurrentMrsltvalName))
                {
                    txt += this.CurrentMrsltvalName;
                    this.CurrentMrsltvalName = string.Empty;
                }

                txt += $"\n0_mrslt_{this.CurrentMrsltvalData}";
                this.CurrentMrsltvalData = double.NaN;

                Console.WriteLine($"[ITUFF]{txt.Replace("\n", "\n[ITUFF]")}");
                this.ItuffOutput.Add(txt);
            });
            this.DatalogServiceMock.Setup(o => o.WriteToItuff(It.IsAny<IStrgvalFormat>())).Callback((IItuffFormat s) =>
            {
                var txt = "0_tname_";
                if (!string.IsNullOrEmpty(this.CurrentStrgvalName))
                {
                    txt += "_" + this.CurrentStrgvalName;
                    this.CurrentStrgvalName = string.Empty;
                }

                txt += $"\n0_strgval_{this.CurrentStrgvalData}";
                this.CurrentStrgvalData = string.Empty;

                Console.WriteLine($"[ITUFF]{txt.Replace("\n", "\n[ITUFF]")}");
                this.ItuffOutput.Add(txt);
            });
            this.DatalogServiceMock.Setup(o => o.GetItuffMrsltWriter()).Returns(this.MrsltFormatMock.Object);
            this.DatalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.StrgvalFormatMock.Object);
            Prime.Services.DatalogService = this.DatalogServiceMock.Object;
        }

        private static string TestInstanceName { get; } = "FakeModule::FakeTest";

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private Mock<ITestProgramService> TestProgramServiceMock { get; set; }

        private Dictionary<string, string> SharedStorageValues { get; set; }

        private Mock<ISharedStorageService> SharedStorageMock { get; set; }

        private Mock<IDatalogService> DatalogServiceMock { get; set; }

        private List<string> ItuffOutput { get; set; }

        private string CurrentStrgvalData { get; set; }

        private double CurrentMrsltvalData { get; set; }

        private string CurrentStrgvalName { get; set; }

        private string CurrentMrsltvalName { get; set; }

        private Mock<IStrgvalFormat> StrgvalFormatMock { get; set; }

        private Mock<IMrsltFormat> MrsltFormatMock { get; set; }

        /// <summary>
        /// Method Under Test: InitialiseSOCRecovery().
        /// Expected Result: fail.
        /// Description: input arguments are not entered and trigger a fail.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void InitialiseSOCRecovery_NoArgs_Fail()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // [2] Call the method under test.
            SocRecoveryCallbacks.InitialiseSOCRecovery(string.Empty);
        }

        /// <summary>
        /// Method Under Test: InitialiseSOCRecovery().
        /// Expected Result: fail.
        /// Description: valid arguments expected all to be set to 0 and trigger a pass.
        /// </summary>
        [TestMethod]
        public void InitialiseSOCRecovery_ValidArgs_Pass()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // [2] Call the method under test.
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Hello", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("How", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Are", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("You", 0, Prime.SharedStorageService.Context.DUT));
            SocRecoveryCallbacks.InitialiseSOCRecovery("Hello How Are You");
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Hello", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("How", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Are", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("You", 0, Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: SetSOCRecoveryToken().
        /// Expected Result: fail.
        /// Description: valid arguments expected all to be set to 3 and trigger a pass.
        /// </summary>
        [TestMethod]
        public void SetSOCRecoveryToken_ValidArgs_Pass()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // [2] Call the method under test.
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Hello", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("How", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Are", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("You", 3, Prime.SharedStorageService.Context.DUT));
            SocRecoveryCallbacks.SetSOCRecoveryToken("Hello=3 How=3 Are=3 You=3");
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Hello", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("How", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Are", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("You", 3, Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: SetSOCRecoveryToken().
        /// Expected Result: fail.
        /// Description: missing arguments expected to fail.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void SetSOCRecoveryToken_InValidArgs_fail()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // [2] Call the method under test.
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Hello", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("How", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Are", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("You", 3, Prime.SharedStorageService.Context.DUT));
            SocRecoveryCallbacks.SetSOCRecoveryToken("Hello=3 How Are=3 You=3");
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Hello", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("How", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Are", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("You", 3, Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: PrintTokenToItuffDLCP().
        /// Expected Result: fail.
        /// Description: missing arguments expected to fail.
        /// </summary>
        [TestMethod]
        public void PrintTokenToItuffDLCP_InValidArgs_fail()
        {
            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            // [2] Call the method under test.
            var expected_output = new List<string>();
            expected_output.Add("[ITUFF]0_tname_SOC_Recovery_2");
            expected_output.Add("[ITUFF]0_mrslt_51");
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable("SOC_Recovery", Prime.SharedStorageService.Context.DUT)).Returns("123");
            SocRecoveryCallbacks.PrintTokenToItuffDLCP("SOC_Recovery");
            Assert.AreEqual(expected_output.ToString(), this.ItuffOutput.ToString());
        }
    }
}
