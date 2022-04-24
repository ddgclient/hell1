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

namespace DDG.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.SharedStorageService;

    /// <summary>
    /// Unit test 1 to set value to shared storage and read back.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PocRecoveryBase_UnitTest
    {
        private Dictionary<string, string> SharedStorageValues { get; set; }

        private Dictionary<string, int> SharedStorageValuesInt { get; set; }

        private Mock<ISharedStorageService> SharedStorageMock { get; set; }

        private Mock<IDatalogService> DatalogServiceMock { get; set; }

        private Mock<IStrgvalFormat> DatalogMock { get; set; }

        private List<string> ItuffOutput { get; set; }

        private string CurrentStrgvalData { get; set; }

        private double CurrentMrsltvalData { get; set; }

        private string CurrentStrgvalName { get; set; }

        private string CurrentMrsltvalName { get; set; }

        private Mock<IStrgvalFormat> StrgvalFormatMock { get; set; }

        private Mock<IMrsltFormat> MrsltFormatMock { get; set; }

        /// <summary>
        /// Initializing unit test mock services.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.SharedStorageValues = new Dictionary<string, string>();
            this.SharedStorageValuesInt = new Dictionary<string, int>();
            this.ItuffOutput = new List<string>();

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(
                o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });

            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // Default Mock for Shared service.
            this.SharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>()))
                .Callback((string key, object obj, Context context) =>
                {
                    Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                });
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Context>()))
                .Callback((string key, int obj, Context context) =>
                {
                    Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.SharedStorageValuesInt[key] = obj;
                });
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, string obj, Context context) =>
                {
                    Console.WriteLine($"Saving SharedStorage Key={key}");
                    this.SharedStorageValues[key] = obj;
                });
            this.SharedStorageMock.Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>()))
                .Callback((string key, Type obj, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
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
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.SharedStorageValuesInt.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => this.SharedStorageValuesInt[key]);
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

        /// <summary>
        /// Method Under Test: SetTracking().
        /// Expected Result: Pass.
        /// Description: Set a single integer value into the shared storage database. Verify by reading back function.
        /// </summary>
        [TestMethod]
        public void SetTracking_SetSingleValue_Pass()
        {
            /* Setup variables for test execution */
            int value_storage;          // Value to hold read back value.
            int value_setting = 99;     // Try to set token "dummy" to 99.

            /* Execute test */
            Console.WriteLine($"Initial Value is \n{value_setting}");
            PocRecovery.Service.SetTracking("dummy", value_setting);
            value_storage = PocRecovery.Service.GetTracking("dummy");
            Assert.AreEqual(value_storage, value_setting);  // Confirm read vlue matches write value.
            value_setting = 1;
            PocRecovery.Service.SetTracking("dummy", value_setting);
            value_storage = PocRecovery.Service.GetTracking("dummy");
            Assert.AreEqual(value_storage, value_setting);  // Confirm read vlue matches write value.
            value_setting = 2;
            PocRecovery.Service.SetTracking("dummy", value_setting);
            value_storage = PocRecovery.Service.GetTracking("dummy");

            /*Verify that the mock set dummy to final expected value */
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("dummy", 2, Context.DUT));
        }

        /// <summary>
        /// Method Under Test: SerialSetTracking().
        /// Expected Result: Pass.
        /// Description: Set multiple characters and read back.
        /// </summary>
        [TestMethod]
        public void SerialSetTracking_SetValues_Pass()
        {
            /* Setup variables for test execution */
            List<char> value_storage;          // Value to hold read back value.
            List<char> value_setting = new List<char>(4);     // Try to set token "dummy" to 99.
            string test;
            value_setting.Add('1');
            value_setting.Add('0');
            value_setting.Add('1');
            value_setting.Add('2');
            test = string.Join(string.Empty, value_setting);
            /* Execute test */
            Console.WriteLine($"Initial Value is \n{value_setting}");
            PocRecovery.Service.SerialSetTracking("SOCRecoveryTokenString", value_setting);
            value_storage = PocRecovery.Service.ReadSerialSharedStorage("SOCRecoveryTokenString");
            Assert.AreEqual(value_storage[0], value_setting[0]);  // Confirm read vlue matches write value.
            Assert.AreEqual(value_storage[1], value_setting[1]);  // Confirm read vlue matches write value.
            Assert.AreEqual(value_storage[2], value_setting[2]);  // Confirm read vlue matches write value.
            Assert.AreEqual(value_storage[3], value_setting[3]);  // Confirm read vlue matches write value.
            Console.WriteLine($"{value_setting} compared to {value_storage}");
            /*Verify that the mock set dummy to final expected value */
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("SOCRecoveryTokenString", test, Context.DUT));
        }

        /// <summary>
        /// Method Under Test: SetTracking().
        /// Expected Result: Pass.
        /// Description: Set a single integer value into the shared storage database. Verify by reading back function.
        /// </summary>
        [TestMethod]
        public void SetTracking_SetSingleValue_Fail()
        {
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("dummy", 99, Context.DUT)).Throws(new Prime.Base.Exceptions.FatalException("Failed to write shared storage"));
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintError("TokenName=[dummy]Value failed to be written to share storage database", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.FatalException>(() => PocRecovery.Service.SetTracking("dummy", 99));
            Assert.AreEqual("Failed to write shared storage", ex.Message);
            consoleServiceMock.VerifyAll();
        }

        /// <summary>
        /// Method Under Test: SetTracking().
        /// Expected Result: Fail.
        /// Description: Try to set a null token name value to validate test method exception will be thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void SetTracking_SetNullToken_Fail()
        {
            /* Setup variables for test execution */
            int value_storage;       // Value to hold read back value.
            int value_setting = 99;  // Value to write to specified token.
            /* Execute test */
            Console.WriteLine($"Initial Value is \n{value_setting}");
            PocRecovery.Service.SetTracking(" ", value_setting);        // Expected to throw test method exception on execution.
            value_storage = PocRecovery.Service.GetTracking(" ");
            /*Verify results */
            Assert.AreEqual(value_storage, value_setting);
        }

        /// <summary>
        /// Method Under Test: GetTracking().
        /// Expected Result: Fail.
        /// Description: Try to read back a token which does not exist. Test Method exception expected on readback.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void GetTracking_GetNullToken_Fail()
        {
            /* Setup variables for test execution */
            int value_setting = 99;
            int value_storage;
            /* Execute test */
            value_storage = PocRecovery.Service.GetTracking(" "); // Expect test method exception on failure of this test.
            /* Verify results */
            Assert.AreEqual(value_storage, value_setting);
            this.SharedStorageMock.VerifyAll();
        }

        /// <summary>
        /// Method Under Test: SetTrackingList().
        /// Expected Result: Pass.
        /// Description: Try to set a list of tokens to known valid values.
        /// </summary>
        [TestMethod]
        public void SetTrackingList_SetTokensAsList_Pass()
        {
            /* Initialise test variables */
            string storage_list = "Bobby|Is|Really|Cool";
            int[] value_array = { 1, 2, 3, 4 };
            string value_setting = "1|2|3|4";
            int value_storage;
            /* Execute unit test */
            Console.WriteLine($"Initial token names to set are \n{storage_list}");
            Console.WriteLine($"Initial values to set are \n{value_setting}");
            PocRecovery.Service.SetTrackingList(storage_list, value_setting);
            value_storage = PocRecovery.Service.GetTracking("Bobby");
            Assert.AreEqual(value_storage, value_array[0]);             // Quick check to verify read value matches write value.
            value_storage = PocRecovery.Service.GetTracking("Is");
            Assert.AreEqual(value_storage, value_array[1]);             // Quick check to verify read value matches write value.
            value_storage = PocRecovery.Service.GetTracking("Really");
            Assert.AreEqual(value_storage, value_array[2]);             // Quick check to verify read value matches write value.
            value_storage = PocRecovery.Service.GetTracking("Cool");
            Assert.AreEqual(value_storage, value_array[3]);             // Quick check to verify read value matches write value.
            /* Verify */
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Bobby", 1, Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Is", 2, Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Really", 3, Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Cool", 4, Context.DUT));
        }

        /// <summary>
        /// Method Under Test: SetTrackingList().
        /// Expected Result: Pass.
        /// Description: Check function can handle if only one token is entered without any delimiter .
        /// </summary>
        [TestMethod]
        public void SetTrackingList_WriteOneValue_Pass()
        {
            /* Test setup */
            string storage_list = "Bobby";
            string value_setting = "1";
            /* Test Execution */
            Console.WriteLine($"Initial token names to set are \n{storage_list}");
            Console.WriteLine($"Initial values to set are \n{value_setting}");
            PocRecovery.Service.SetTrackingList(storage_list, value_setting);
            /* Verify */
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Bobby", 1, Context.DUT));
        }

        /// <summary>
        /// Method Under Test: SetTrackingList().
        /// Expected Result: Fail.
        /// Description: Check function will trigger test method exception if a token list and value list entered have different lenths.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void SetTrackingList_differentstringlengths_fail()
        {
            /* Setup variables */
            string storage_list = "Bobby|Is|Really|Cool";
            string value_setting = "1|2|3";
            /* Test execution */
            Console.WriteLine($"Initial token names to set are \n{storage_list}");
            Console.WriteLine($"Initial values to set are \n{value_setting}");
            PocRecovery.Service.SetTrackingList(storage_list, value_setting);       // Expecting test exception since token names and values are different lengths.
            /* Verify */
        }

        /// <summary>
        /// Method Under Test: SetTrackingList().
        /// Expected Result: Fail.
        /// Description: Check function will trigger format exception if integer values are not specified.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SetTrackingList_incorrecttypeinput_fail()
        {
            /* Initialise test variables */
            string storage_list = "Bobby|Is|Really";
            string value_setting = "1|2|3.0";
            /*Execute test */
            Console.WriteLine($"Initial token names to set are \n{storage_list}");
            Console.WriteLine($"Initial values to set are \n{value_setting}");
            PocRecovery.Service.SetTrackingList(storage_list, value_setting);
            /* Verify */
        }

        /// <summary>
        /// Method Under Test: SetTrackingList().
        /// Expected Result: Fail.
        /// Description: Check function will trigger test method exception if error check mode is set True and invalid "1" value is set.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void SetTrackingList_errorcheckzero_fail()
        {
            /* Initialise test setup */
            string storage_list = "Bobby|Is|Really";
            string value_setting = "1|2|0";
            int return_value = 99;
            /* Test method exception */
            Console.WriteLine($"Initial token names to set are \n{storage_list}");
            Console.WriteLine($"Initial values to set are \n{value_setting}");
            return_value = PocRecovery.Service.SetTrackingList(storage_list, value_setting, true); // Expecting test method exception with invalid value set.
            /* Verify */
            Assert.AreEqual(return_value, 0);
            Console.WriteLine($"Return value was \n{return_value}");
        }

        /// <summary>
        /// Method Under Test: SetTrackingList().
        /// Expected Result: Pass.
        /// Description: Check passing when error check mode is enabled bu all values are valid.
        /// </summary>
        [TestMethod]
        public void SetTrackingList_errorcheckone_pass()
        {
            /* Initialise */
            string storage_list = "Bobby|Is|Really";
            string value_setting = "0|2|3";
            int return_value;
            /*TestCategoryAttribute Execution */
            Console.WriteLine($"Initial token names to set are \n{storage_list}");
            Console.WriteLine($"Initial values to set are \n{value_setting}");
            return_value = PocRecovery.Service.SetTrackingList(storage_list, value_setting, true);
            /* Verify */
            Assert.AreEqual(return_value, 1);
            Console.WriteLine($"Return value was \n{return_value}");
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Bobby", 0, Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Is", 2, Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Really", 3, Context.DUT));
        }

        /// <summary>
        /// Method Under Test: GetTrackingList().
        /// Expected Result: Pass.
        /// Description: Check passing valid values written into shared storage database can be read back in list.
        /// </summary>
        [TestMethod]
        public void GetTrackingList_readvalidvalues_pass()
        {
            /* Intialise */
            string storage_list = "Bobby|Is|Really";
            string value_setting = "0|2|3";
            List<int> return_value = new List<int>();
            /* Test Execution */
            Console.WriteLine($"Initial token names to read are \n{storage_list}");
            Console.WriteLine($"Initial values expected are \n{value_setting}");
            PocRecovery.Service.SetTrackingList(storage_list, value_setting, true);
            return_value = PocRecovery.Service.GetTrackingList(storage_list);
            Console.WriteLine($"Return value was \n{return_value}");
            /* Verify */
            Assert.AreEqual(return_value[0], 0);
            Assert.AreEqual(return_value[1], 2);
            Assert.AreEqual(return_value[2], 3);
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Bobby", 0, Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Is", 2, Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Really", 3, Context.DUT));
        }

        /// <summary>
        /// Method Under Test: GetTrackingList().
        /// Expected Result: Fail.
        /// Description: Expect a fatal exception if trying to read from the Prime shared storage database a token which does nt exist.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.FatalException))]
        public void GetTrackingList_readnullvalue_fail()
        {
            /* Initialise */
            string storage_list = "Bobby|Is";
            string value_setting = "0|2";
            List<int> return_value = new List<int>();
            /* Test Execute */
            Console.WriteLine($"Initial token names to read are \n{storage_list}");
            Console.WriteLine($"Initial values expected are \n{value_setting}");
            PocRecovery.Service.SetTrackingList(storage_list, value_setting, true);
            storage_list = "Bobby|Is|Cool";
            return_value = PocRecovery.Service.GetTrackingList(storage_list); // Expecting exception on read back of cool token which has not been set.
            Console.WriteLine($"Return value was \n{return_value}");
            /* Verify */
        }

        /// <summary>
        /// Method Under Test: GetTrackingList().
        /// Expected Result: Fail.
        /// Description: Expect a fatal exception reading back non exist token from list method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.FatalException))]
        public void GetTrackingList_readzerovalue_fail()
        {
            /* Initialise Tokens */
            string storage_list = "Bobby|Is";
            string value_setting = "1|0";
            List<int> return_value = new List<int>();
            /*Test Execution */
            Console.WriteLine($"Initial token names to read are \n{storage_list}");
            Console.WriteLine($"Initial values expected are \n{value_setting}");
            PocRecovery.Service.SetTrackingList(storage_list, value_setting, false); // Error check mode is disabled to ensure that settracking list will not fail.
            storage_list = "Bobby|Is|Cool";
            return_value = PocRecovery.Service.GetTrackingList(storage_list);   // Expected exception on readback for null value.
            Console.WriteLine($"Return value was \n{return_value}");
            /*Verify*/
        }

        /// <summary>
        /// Method Under Test: SetTrackingList().
        /// Expected Result: Fail.
        /// Description: Expect a fatal exception setting a value of "1" indicating invalid fail case.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void SetTrackingList_setonevalue_fail()
        {
            /*Initialise */
            string storage_list = "Bobby|Is";
            string value_setting = "1|0";
            List<int> return_value = new List<int>();
            /* Test Execute */
            Console.WriteLine($"Initial token names to read are \n{storage_list}");
            Console.WriteLine($"Initial values expected are \n{value_setting}");
            PocRecovery.Service.SetTrackingList(storage_list, value_setting, true);
            /*Verify */
        }

        /// <summary>
        /// Method Under Test: PrintStringToItuff().
        /// Expected Result: pass.
        /// Description: Print a valid value to ituff.
        /// </summary>
        [TestMethod]
        public void PrintStringToItuff_datavalid_pass()
        {
            /*Initialise */
            string test_name = "RecoveryString";
            string binary_string = "101011";
            List<string> expected_output = new List<string>();
            expected_output.Add("0_tname_" + test_name);
            expected_output.Add("0_strgval_" + binary_string);
            /* Test Execute */
            /*Datalog Mock */
            PocRecovery.Service.PrintStringToItuff(test_name, binary_string);

            /*Verify */
            Assert.AreEqual(expected_output.ToString(), this.ItuffOutput.ToString());
        }

        /// <summary>
        /// Method Under Test: PrintIntToItuff().
        /// Expected Result: pass.
        /// Description: Print a token value to ituff.
        /// </summary>
        [TestMethod]
        public void PrintValToItuff_datavalid_pass()
        {
            /*Initialise */
            string test_name = "RecoveryString_1";
            int value = 2;
            List<string> expected_output = new List<string>();
            expected_output.Add("0_tname_" + test_name);
            expected_output.Add("0_mrslt_" + value.ToString());
            /* Test Execute */
            /*Datalog Mock */
            PocRecovery.Service.PrintValToItuff(test_name, (double)value);

            /*Verify */
            Assert.AreEqual(expected_output.ToString(), this.ItuffOutput.ToString());
        }
    }
}
