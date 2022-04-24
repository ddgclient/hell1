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

namespace SocRecovery.UnitTest
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

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class SocRecovery_UnitTest : SocRecovery
    {
        private Dictionary<string, string> SharedStorageValues { get; set; }

        private List<int> POCRecovery_input { get; set; }

        private int POCRecovery_output { get; set; }

        private Dictionary<string, int> SharedStorageValuesInt { get; set; }

        private Mock<ISharedStorageService> SharedStorageMock { get; set; }

        private Mock<DDG.IPocRecovery> POCRecoveryMock { get; set; }

        private List<string> ItuffOutput { get; set; }

        private string CurrentStrgvalData { get; set; }

        private double CurrentMrsltvalData { get; set; }

        private string CurrentStrgvalName { get; set; }

        private string CurrentMrsltvalName { get; set; }

        private Mock<IStrgvalFormat> StrgvalFormatMock { get; set; }

        private Mock<IMrsltFormat> MrsltFormatMock { get; set; }

        private Mock<IDatalogService> DatalogServiceMock { get; set; }

        /// <summary>
        /// Default initialization.
        /// </summary>
        [TestInitialize]
        public void Initialization()
        {
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(
                    o => o.PrintDebug(It.IsAny<string>())).
                Callback((string msg) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });
            Prime.Services.ConsoleService = consoleServiceMock.Object;
            this.ItuffOutput = new List<string>();

            // Default Mock for POC Recovery
            this.POCRecoveryMock = new Mock<DDG.IPocRecovery>(MockBehavior.Strict);
            this.POCRecoveryMock.Setup(o => o.GetTrackingList(It.IsAny<string>()))
                .Returns((string token_names) => this.POCRecovery_input);
            this.POCRecoveryMock.Setup(o => o.SetTrackingList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback((string token_list, string value_list, bool error) =>
                {
                Console.WriteLine("Saving SharedStorage key");
                })
                .Returns((string token_names, string value_list, bool error) => this.POCRecovery_output);

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
            this.SharedStorageMock.Setup(o => o.DumpAllTablesToConsole());

            Prime.Services.SharedStorageService = this.SharedStorageMock.Object;

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
            this.DatalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            this.DatalogServiceMock.Setup(o => o.WriteToItuff(It.IsAny<IMrsltFormat>())).Callback((IItuffFormat s) =>
            {
                var txt = "0_tname";
                if (!string.IsNullOrEmpty(this.CurrentMrsltvalName))
                {
                    txt += this.CurrentMrsltvalName;
                    this.CurrentMrsltvalName = string.Empty;
                }

                txt += "\n0_mrslt_" + this.CurrentMrsltvalData;
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
        /// Method Under Test: Verify().
        /// Expected Result: Pass.
        /// Description: Run verify in SerialMode_init mode with valid token and length entered.
        /// </summary>
        [TestMethod]
        public void Verify_Verify_SerialMode_Init_pass()
        {
            /* Initialise Tokens */
            /*Execute test */
            this.RecoveryMode = Modes.SerialMode_Init;
            this.TokenNames = "SocRecovery";
            this.SerialModeLength = 4; // A four bit wide variable containing zero characters.
            this.Verify();
            this.Verify();
        }

        /// <summary>
        /// Method Under Test: Verify().
        /// Expected Result: Pass.
        /// Description: Run verify in SerialMode_init mode with override mode active.
        /// </summary>
        [TestMethod]
        public void Verify_Override_SerialMode_Init_pass()
        {
            /* Initialise Tokens */
            /*Execute test */
            this.RecoveryMode = Modes.SerialMode_Init;
            this.TokenNames = "SocRecovery";
            this.SerialModeLength = -1; // A four bit wide variable containing zero characters.
            this.ValueList = "0101"; // A four bit wide variable containing zero characters.
            this.Verify();
            this.Verify();
        }

        /// <summary>
        /// Method Under Test: Verify().
        /// Expected Result: Fail.
        /// Description: Run verify in SerialMode_init mode with override mode active and invalid values.
        /// </summary>
        [TestMethod]
        public void Verify_Override_SerialMode_Init_fail()
        {
            /* Initialise Tokens */
            /*Execute test */
            this.RecoveryMode = Modes.SerialMode_Init;
            this.TokenNames = "SocRecovery";
            this.SerialModeLength = -1; // -1 enters override mode.
            this.ValueList = string.Empty;
            Assert.ThrowsException<Exception>(this.Verify);
        }

        /// <summary>
        /// Method Under Test: Verify().
        /// Expected Result: Pass.
        /// Description: Run verify in SerialMode_init mode with valid token and no length entered.
        /// </summary>
        [TestMethod]
        public void Verify_Verify_SerialModeInit_nolength_pass()
        {
            /* Initialise Tokens */
            /*Execute test */
            this.RecoveryMode = Modes.SerialMode_Init;
            this.TokenNames = "SocRecovery";
            Assert.ThrowsException<Exception>(this.Verify);
            /*Verify */
        }

        /// <summary>
        /// Method Under Test: Verify().
        /// Expected Result: Pass.
        /// Description: Run verify in init mode with valid token names entered.
        /// </summary>
        [TestMethod]
        public void Verify_VerifyInit_pass()
        {
            /* Initialise Tokens */
            /*Execute test */
            this.RecoveryMode = Modes.Init;
            this.TokenNames = "Token_1|Token_2";
            this.Verify();
            this.Verify();
            /*Verify */
        }

        /// <summary>
        /// Method Under Test: Verify().
        /// Expected Result: fail.
        /// Description: Run verify in init mode with null token names entered.
        /// </summary>
        [TestMethod]
        public void Verify_VerifyInitNullTokens_fail()
        {
            /* Initialise Tokens */
            /* Execute test */
            this.RecoveryMode = Modes.Init;
            this.TokenNames = string.Empty;
            Assert.ThrowsException<Exception>(this.Verify);
            /*Verify */
        }

        /// <summary>
        /// Method Under Test: Verify().
        /// Expected Result: fail.
        /// Description: Run verify in set token mode with null token names and null value list entered.
        /// </summary>
        [TestMethod]
        public void Verify_SetTokenModeEmpty_fail()
        {
            /*Test execution */
            this.RecoveryMode = Modes.SetToken;
            this.TokenNames = string.Empty;
            this.ValueList = string.Empty;
            Assert.ThrowsException<Exception>(this.Verify);
        }

        /// <summary>
        /// Method Under Test: Verify().
        /// Expected Result: fail.
        /// Description: Run verify in set token mode with valid token name and value list entered.
        /// </summary>
        [TestMethod]
        public void Verify_SetTokenModeValid_pass()
        {
            /*Test execution */
            this.RecoveryMode = Modes.SetToken;
            this.TokenNames = "Token1|Token2";
            this.ValueList = "1|2";
            this.Verify();
        }

        /// <summary>
        /// Method Under Test: Verify().
        /// Expected Result: pass.
        /// Description: Run verify in serial set token mode with valid token name and value list entered.
        /// </summary>
        [TestMethod]
        public void Verify_SerialSetTokenModeValid_pass()
        {
            /*Test execution */
            this.RecoveryMode = Modes.SerialMode_SetToken;
            this.TokenNames = "SocRecoveryString";
            this.ValueList = "1|2";
            this.Verify();
        }

        /// <summary>
        /// Method Under Test: Verify().
        /// Expected Result: fail.
        /// Description: Run verify in serial set token mode with invalid value list entered.
        /// </summary>
        [TestMethod]
        public void Verify_SerialSetTokenModeValid_fail()
        {
            /*Test execution */
            this.RecoveryMode = Modes.SerialMode_SetToken;
            this.TokenNames = "SocRecoveryString";
            this.ValueList = string.Empty;
            Assert.ThrowsException<Exception>(this.Verify);
        }

        /// <summary>
        /// Method Under Test: Verify().
        /// Expected Result: fail.
        /// Description: Run verify in set token mode with invalid token name and valid value list entered.
        /// </summary>
        [TestMethod]
        public void Verify_SetTokenModeValid_fail()
        {
            /*Test execution */
            this.RecoveryMode = Modes.SetToken;
            this.TokenNames = string.Empty;
            this.ValueList = "1|2";
            Assert.ThrowsException<Exception>(this.Verify);
        }

        /// <summary>
        /// Method Under Test: Verify().
        /// Expected Result: fail.
        /// Description: Run verify in set token mode with different token name count and valid value count entered.
        /// </summary>
        [TestMethod]
        public void Verify_SetTokenModeDifferenlengths_fail()
        {
            /*Test execution */
            this.RecoveryMode = Modes.SetToken;
            this.TokenNames = "Token1|Token2|Token3";
            this.ValueList = "1|2";
            Assert.ThrowsException<Exception>(this.Verify);
        }

        /// <summary>
        /// Method Under Test: Verify().
        /// Expected Result: fail.
        /// Description: Run verify in binary mode to test error check for invalid input is working.
        /// </summary>
        [TestMethod]
        public void Verify_binaryconversion_fail()
        {
            /*Test execution */
            this.RecoveryMode = Modes.BinaryConversion;
            this.TokenNames = "Token1";
            this.ValueList = "TEST1_1|TEST2_2|TEST3_4";
            Assert.ThrowsException<Exception>(this.Verify);
        }

        /// <summary>
        /// Method Under Test: Verify().
        /// Expected Result: fail.
        /// Description: Run verify in binary mode with valid input to ensure pass.
        /// </summary>
        [TestMethod]
        public void Verify_binaryconversion_pass()
        {
            /*Test execution */
            this.RecoveryMode = Modes.BinaryConversion;
            this.TokenNames = "Token1";
            this.ValueList = "TEST1_1|TEST2_2|TEST3_3";
            this.Verify();
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Run SerialMode_init mode for valid token.
        /// </summary>
        [TestMethod]
        public void Execute_SerialModeInit_validinputs_pass()
        {
            /*initilise valid inputs */
            this.RecoveryMode = Modes.SerialMode_Init;
            this.TokenNames = "SOCRecoveryToken";
            this.SerialModeLength = 4;
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(this.TokenNames, "0000", Prime.SharedStorageService.Context.DUT));
            /* Test flow executions */
            this.Execute();
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable(this.TokenNames, "0000", Prime.SharedStorageService.Context.DUT));
            /* Check results */
            Assert.AreEqual(this.Execute(), 1);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Run SerialMode_init using overide mode to set initial value.
        /// </summary>
        [TestMethod]
        public void Execute_SerialModeInit_overridevalid_pass()
        {
            /*initilise valid inputs */
            this.RecoveryMode = Modes.SerialMode_Init;
            this.TokenNames = "SOCRecoveryToken";
            this.SerialModeLength = -1;
            this.ValueList = "0101";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(this.TokenNames, "0101", Prime.SharedStorageService.Context.DUT));
            /* Test flow executions */
            this.Execute();
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable(this.TokenNames, "0101", Prime.SharedStorageService.Context.DUT));
            /* Check results */
            Assert.AreEqual(this.Execute(), 1);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Run init mode for valid entered tokens.
        /// </summary>
        [TestMethod]
        public void Execute_InitModevalidlist_pass()
        {
            /*initilise valid inputs */
            this.RecoveryMode = Modes.Init;
            this.TokenNames = "Token_1|Token_2";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token_1", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token_2", 0, Prime.SharedStorageService.Context.DUT));
            /* Test flow executions */
            this.Execute();
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Token_1", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Token_2", 0, Prime.SharedStorageService.Context.DUT));
            /* Check results */
            Assert.AreEqual(this.Execute(), 1);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Run init mode for invalid entered tokens.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void Execute_InitModeinvalidlist_fail()
        {
            /*initilise valid inputs */
            this.RecoveryMode = Modes.Init;
            this.TokenNames = string.Empty;
            /* Test flow executions */
            Assert.AreEqual(this.Execute(), 0);
            /* Check results */
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Run set token mode for valid entered tokens.
        /// </summary>
        [TestMethod]
        public void Execute_SetTokenModeseValid_pass()
        {
            this.RecoveryMode = Modes.SetToken;
            this.TokenNames = "Token1|Token2";
            this.ValueList = "3|4";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 4, Prime.SharedStorageService.Context.DUT));
            Assert.AreEqual(this.Execute(), 1);
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Token1", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Token2", 4, Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Run serial mode set token mode for valid values.
        /// </summary>
        [TestMethod]
        public void Execute_SetSerialTokenModevalid_pass()
        {
            this.RecoveryMode = Modes.SerialMode_SetToken;
            this.TokenNames = "SOCRecovery";
            this.ValueList = "3|4|X|1";
            var test = "3401";
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable(this.TokenNames, Prime.SharedStorageService.Context.DUT)).Returns("0000");
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(this.TokenNames, test, Prime.SharedStorageService.Context.DUT));
            Assert.AreEqual(this.Execute(), 1);
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable(this.TokenNames, test, Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Run serial mode set token error check mode for valid values.
        /// Description: Token value in '0000' state, error should not trigger.
        /// </summary>
        [TestMethod]
        public void Execute_SetSerialTokenModeErrorvalid_pass()
        {
            this.RecoveryMode = Modes.SerialMode_SetToken_ErrorCheck;
            this.TokenNames = "SOCRecovery";
            this.ValueList = "3|4|X|1";
            var test = "3401";
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable(this.TokenNames, Prime.SharedStorageService.Context.DUT)).Returns("0000");
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(this.TokenNames, test, Prime.SharedStorageService.Context.DUT));
            Assert.AreEqual(this.Execute(), 1);
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable(this.TokenNames, test, Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Run serial mode set token error check mode where user is trying to set 0 to token where recovery has been triggered.
        /// Description: Test exercises some of the error checks and value setting logic for set token error check mode.
        /// </summary>
        [TestMethod]
        public void Execute_SetSerialTokenModeErrorInvalidrequest_pass()
        {
            this.RecoveryMode = Modes.SerialMode_SetToken_ErrorCheck;
            this.TokenNames = "SOCRecovery";
            this.ValueList = "0|3|X|1";
            var test = "1301";
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable(this.TokenNames, Prime.SharedStorageService.Context.DUT)).Returns("1000");
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(this.TokenNames, test, Prime.SharedStorageService.Context.DUT));
            Assert.AreEqual(this.Execute(), 1);
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable(this.TokenNames, test, Prime.SharedStorageService.Context.DUT));

            this.ValueList = "2|2|X|0";
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable(this.TokenNames, Prime.SharedStorageService.Context.DUT)).Returns(test);
            test = "1101";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(this.TokenNames, test, Prime.SharedStorageService.Context.DUT));
            Assert.AreEqual(this.Execute(), 1);
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable(this.TokenNames, test, Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: fail.
        /// Description: Run serial mode set token mode for invalid length input values.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Prime.Base.Exceptions.TestMethodException))]
        public void Execute_SetSerialTokenModeseInValidlenght_fail()
        {
            this.RecoveryMode = Modes.SerialMode_SetToken;
            this.TokenNames = "SOCRecovery";
            this.ValueList = "3|4|X|1";
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable(this.TokenNames, Prime.SharedStorageService.Context.DUT)).Returns("000");
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(this.TokenNames, "3401", Prime.SharedStorageService.Context.DUT));
            Assert.AreEqual(this.Execute(), 1);
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable(this.TokenNames, "3401", Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Run set token error check mode for valid entered tokens. Valid condition should be triggered.
        /// </summary>
        [TestMethod]
        public void Execute_SetTokenErrorCheck_ValidRecovery_pass()
        {
            /*Setup prime shared storage database */
            this.RecoveryMode = Modes.SetToken;
            this.TokenNames = "Token1|Token2";
            this.ValueList = "3|3";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 3, Prime.SharedStorageService.Context.DUT));
            this.Execute();
            /* Change mode for execute funciton */
            this.RecoveryMode = Modes.SetToken_ErrorCheck;
            this.ValueList = "3|3";
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token1", Prime.SharedStorageService.Context.DUT)).Returns(3);
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token2", Prime.SharedStorageService.Context.DUT)).Returns(3);
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 3, Prime.SharedStorageService.Context.DUT));

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // Execute the test. Should pass trying as token values will not be updated form current recovery state
            Assert.AreEqual(this.Execute(), 1);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: fail.
        /// Description: Run set token error check mode for invalid entered tokens. InValid condition should be triggered. User tries to switch from one recovery scnario to another. 3 to 4.
        /// </summary>
        [TestMethod]
        public void Execute_SetTokenErrorCheck_InvalidRecoverySwitch_fail()
        {
            /*Setup prime shared storage database */
            this.RecoveryMode = Modes.SetToken;
            this.TokenNames = "Token1|Token2";
            this.ValueList = "3|3";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 3, Prime.SharedStorageService.Context.DUT));
            this.Execute();
            /* Change mode for execute funciton */
            this.RecoveryMode = Modes.SetToken_ErrorCheck;
            this.ValueList = "3|4";
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token1", Prime.SharedStorageService.Context.DUT)).Returns(3);
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token2", Prime.SharedStorageService.Context.DUT)).Returns(3);
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 4, Prime.SharedStorageService.Context.DUT));

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // Execute the test. Should fail trying to set token2 from 3 to 4.
            Assert.AreEqual(this.Execute(), 0);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: fail.
        /// Description: Run set token error check mode for invalid entered tokens. Case where user attempts to move from recovery setting to fully functional.3 to 0.
        /// </summary>
        [TestMethod]
        public void Execute_SetTokenErrorCheck_RecoveryToPass_fail()
        {
            /*Setup prime shared storage database */
            this.RecoveryMode = Modes.SetToken;
            this.TokenNames = "Token1|Token2";
            this.ValueList = "3|3";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 3, Prime.SharedStorageService.Context.DUT));
            this.Execute();
            /* Change mode for execute function */
            this.RecoveryMode = Modes.SetToken_ErrorCheck;
            this.ValueList = "0|3";
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token1", Prime.SharedStorageService.Context.DUT)).Returns(3);
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token2", Prime.SharedStorageService.Context.DUT)).Returns(3);
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 3, Prime.SharedStorageService.Context.DUT));

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // Execute the test. Should fail trying to set token2 from 3 (Recovery state) to 0 (Pass state).
            Assert.AreEqual(this.Execute(), 0);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: fail.
        /// Description: User attempts to set token from fully functions (0) to 3 (recovery scenario) .
        /// </summary>
        [TestMethod]
        public void Execute_SetTokenErrorCheck_GoodtoBad_pass()
        {
            /*Setup prime shared storage database */
            this.RecoveryMode = Modes.SetToken;
            this.TokenNames = "Token1|Token2";
            this.ValueList = "0|3";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 3, Prime.SharedStorageService.Context.DUT));
            this.Execute();
            /* Change mode for execute funciton */
            this.RecoveryMode = Modes.SetToken_ErrorCheck;
            this.ValueList = "3|3";
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token1", Prime.SharedStorageService.Context.DUT)).Returns(0);
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token2", Prime.SharedStorageService.Context.DUT)).Returns(3);
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 3, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 3, Prime.SharedStorageService.Context.DUT));

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // Execute the test. Should pass trying to set token1 from 0 (pass state) to 3 (recovery state).
            Assert.AreEqual(this.Execute(), 1);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: All tokens are at default flow control mode should route to port 1, expect to pass out port 1 .
        /// </summary>
        [TestMethod]
        public void Execute_FlowControl_Norecoverypass_pass()
        {
            /*Setup prime shared storage database to 0s */
            this.RecoveryMode = Modes.Init;
            this.TokenNames = "Token1|Token2";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 0, Prime.SharedStorageService.Context.DUT));
            this.Execute();
            /* Change mode for execute function */
            this.RecoveryMode = Modes.FlowControl;
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token1", Prime.SharedStorageService.Context.DUT)).Returns(0);
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token2", Prime.SharedStorageService.Context.DUT)).Returns(0);

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // Execute the test. All tokens should be at 0 and exit port 1.
            Assert.AreEqual(this.Execute(), 1);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: All bits are at 0 serial flow control mode should route to port 1, expect to pass out port 1 .
        /// </summary>
        [TestMethod]
        public void Execute_SerialFlowControl_Norecoverypass_pass()
        {
            /*Setup prime shared storage database to 0s */
            this.RecoveryMode = Modes.SerialMode_Init;
            this.SerialModeLength = 2;
            this.TokenNames = "RecoveryString";
            new List<char> { '0', '0' };
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("RecoveryString", "00", Prime.SharedStorageService.Context.DUT));
            this.Execute();
            /* Change mode for execute function */
            this.RecoveryMode = Modes.SerialMode_FlowControl;
            this.ValueList = "1|1";
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT)).Returns("00");

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // Execute the test. All tokens should be at 0 and exit port 1.
            Assert.AreEqual(this.Execute(), 1);
            this.SharedStorageMock.Verify(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: All bits are at 0 serial flow control mode should route to port 1, expect to pass out port 1 .
        /// Test skipping mode by entering X in one bit position of value list.
        /// </summary>
        [TestMethod]
        public void Execute_SerialFlowControl_OneBitNorecoverypass_pass()
        {
            /*Setup prime shared storage database to 0s */
            this.RecoveryMode = Modes.SerialMode_Init;
            this.SerialModeLength = 2;
            this.TokenNames = "RecoveryString";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("RecoveryString", "00", Prime.SharedStorageService.Context.DUT));
            this.Execute();
            /* Change mode for execute function */
            this.RecoveryMode = Modes.SerialMode_FlowControl;
            this.ValueList = "X|1";
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT)).Returns("00");

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // Execute the test. All tokens should be at 0 and exit port 1.
            Assert.AreEqual(this.Execute(), 1);
            this.SharedStorageMock.Verify(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: One bit is at 1 serial flow control mode should route to port 0, expect to pass out port 1 .
        /// Test skipping mode by entering X in one bit position of value list.
        /// </summary>
        [TestMethod]
        public void Execute_SerialFlowControl_bitisone_pass()
        {
            this.TokenNames = "RecoveryString";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("RecoveryString", "01", Prime.SharedStorageService.Context.DUT));
            /* Change mode for execute function */
            this.RecoveryMode = Modes.SerialMode_FlowControl;
            this.ValueList = "X|1";
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT)).Returns("01");

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // Execute the test. One token is at 1, exit port 0.
            Assert.AreEqual(this.Execute(), 0);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Testing skip bit works, skipping bit two set to 1. Expect to exit out port 1 indicating only bit set to zero is checked .
        /// Test skipping mode by entering X in one bit position of value list.
        /// </summary>
        [TestMethod]
        public void Execute_SerialFlowControl_skipbitisone_pass()
        {
            this.TokenNames = "RecoveryString";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("RecoveryString", "01", Prime.SharedStorageService.Context.DUT));
            /* Change mode for execute function */
            this.RecoveryMode = Modes.SerialMode_FlowControl;
            this.ValueList = "1|X";
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT)).Returns("01");

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // Execute the test. One token is at 1, exit port 0.
            Assert.AreEqual(this.Execute(), 1);
            this.SharedStorageMock.Verify(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Setting fail condition where ValueList is different length to string read back from PSSDB.
        /// </summary>
        [TestMethod]
        public void Execute_SerialFlowControl_invaliduserinput_pass()
        {
            this.TokenNames = "RecoveryString";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("RecoveryString", "011", Prime.SharedStorageService.Context.DUT));
            /* Change mode for execute function */
            this.RecoveryMode = Modes.SerialMode_FlowControl;
            this.ValueList = "1|X";
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT)).Returns("011");

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // user input value list and reecovery variable differ in length . expecting exit out port -1.
            Assert.AreEqual(this.Execute(), -1);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Setting fail condition where flow value is not X or 0 -9.
        /// </summary>
        [TestMethod]
        public void Execute_SerialFlowControl_flowoutrange_pass()
        {
            this.TokenNames = "RecoveryString";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("RecoveryString", "011", Prime.SharedStorageService.Context.DUT));
            /* Change mode for execute function */
            this.RecoveryMode = Modes.SerialMode_FlowControl;
            this.ValueList = "1|Y";
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT)).Returns("011");

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // user input value list and reecovery variable differ in length . expecting exit out port -1.
            Assert.AreEqual(this.Execute(), -1);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Setting fail condition where ValueList is set to skip all bits in recovery string.
        /// </summary>
        [TestMethod]
        public void Execute_SerialFlowControl_valuelistallxbits_pass()
        {
            this.TokenNames = "RecoveryString";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("RecoveryString", "01", Prime.SharedStorageService.Context.DUT));
            /* Change mode for execute function */
            this.RecoveryMode = Modes.SerialMode_FlowControl;
            this.ValueList = "X|X";
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT)).Returns("01");

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // expecting to fail out port - 1 for setting value list to all 'X' skip bits.
            Assert.AreEqual(this.Execute(), -1);
            this.SharedStorageMock.Verify(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: One token is set to recovery value, 2 expect to pass out port 2.
        /// </summary>
        [TestMethod]
        public void Execute_FlowControl_RecoveryScenario_pass()
        {
            /*Setup prime shared storage database */
            this.RecoveryMode = Modes.SetToken;
            this.TokenNames = "Token1|Token2";
            this.ValueList = "0|2";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 2, Prime.SharedStorageService.Context.DUT));
            this.Execute();
            /* Change mode for execute function */
            this.RecoveryMode = Modes.FlowControl;
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token1", Prime.SharedStorageService.Context.DUT)).Returns(0);
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token2", Prime.SharedStorageService.Context.DUT)).Returns(2);

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // Execute the test. token should be set to 2 and exit port 2.
            Assert.AreEqual(this.Execute(), 2);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Two token is set to recovery value, checking exit port is max value set.
        /// </summary>
        [TestMethod]
        public void Execute_FlowControl_RecoveryScenarioMaxValue_pass()
        {
            /*Setup prime shared storage database */
            this.RecoveryMode = Modes.SetToken;
            this.TokenNames = "Token1|Token2";
            this.ValueList = "2|6";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 2, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 6, Prime.SharedStorageService.Context.DUT));
            this.Execute();
            /* Change mode for execute function */
            this.RecoveryMode = Modes.FlowControl;
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token1", Prime.SharedStorageService.Context.DUT)).Returns(2);
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token2", Prime.SharedStorageService.Context.DUT)).Returns(6);

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // Execute the test. token should be set to 2 and exit port 2.
            Assert.AreEqual(this.Execute(), 6);

            /*Reverse token order and run again to ensure same result */
            this.TokenNames = "Token1|Token2";
            this.ValueList = "6|2";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 6, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 2, Prime.SharedStorageService.Context.DUT));
            this.Execute();

            /* Change mode for execute function */
            this.RecoveryMode = Modes.FlowControl;
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token1", Prime.SharedStorageService.Context.DUT)).Returns(6);
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token2", Prime.SharedStorageService.Context.DUT)).Returns(2);

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // Execute the test. token should be set to 2 and exit port 2.
            Assert.AreEqual(this.Execute(), 6);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Token is set to 1. expect to exit out port 0.
        /// </summary>
        [TestMethod]
        public void Execute_FlowControl_zerovalue_pass()
        {
            this.TokenNames = "Token1";
            /*Setup prime shared storage database */
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 1, Prime.SharedStorageService.Context.DUT));
            /* Change mode for execute function */
            this.RecoveryMode = Modes.FlowControl;
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetIntegerRowFromTable("Token1", Prime.SharedStorageService.Context.DUT)).Returns(1);

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // Execute the test. token should be set to 1 and exit port 0.
            Assert.AreEqual(this.Execute(), 0);
        }

        /// <summary>
        /// exec mode execution: unit test checks if token list can be set to set to specified values.
        /// </summary>
        [TestMethod]
        public void Execute_SetTokenMode_pass()
        {
            this.RecoveryMode = Modes.SetToken;
            this.TokenNames = "Token1|Token2";
            this.ValueList = "0|2";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 2, Prime.SharedStorageService.Context.DUT));
            this.Execute();
        }

        /// <summary>
        /// exec mode execution: unit test checks if token list can be set to set to specified values.
        /// </summary>
        [TestMethod]
        public void Execute_invalidvalues_fail()
        {
            this.RecoveryMode = Modes.SetToken;
            this.TokenNames = "Token1|Token2";
            this.ValueList = "0|14";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token1", 0, Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Token2", 14, Prime.SharedStorageService.Context.DUT));
            this.Execute();
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Set to BinaryConversion Mode in order to test basic functionality.
        /// </summary>
        [TestMethod]
        public void Execute_BinaryConversion_ValidInput_pass()
        {
            /*Setup prime shared storage database */
            this.RecoveryMode = Modes.SerialMode_SetToken;
            this.TokenNames = "SocRecovery";
            this.ValueList = "0|1|2|3|4|7|1|1";
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable(this.TokenNames, Prime.SharedStorageService.Context.DUT)).Returns("00000000");
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(this.TokenNames, "01234711", Prime.SharedStorageService.Context.DUT));
            this.Execute();
            /* Change mode for execute function */
            this.RecoveryMode = Modes.BinaryConversion;
            this.ValueList = "TEST0_1|TEST1_1|TEST2_2|TEST3_2|TEST4_3|TEST5_3|TEST6_3|TEST7_2";
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable("SocRecovery", Prime.SharedStorageService.Context.DUT)).Returns("01234711");
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST0", "0", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST1", "1", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST2", "10", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST3", "01", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST4", "100", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST5", "001", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST6", "111", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST7", "11", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("Binary_" + this.TokenNames, "01100110000111111", Prime.SharedStorageService.Context.DUT));

            // Execute the test. Return value should be 1 for success.
            Assert.AreEqual(this.Execute(), 1);
            this.SharedStorageMock.Verify(o => o.GetStringRowFromTable("SocRecovery", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST0", "0", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST1", "1", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST2", "10", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST3", "01", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST4", "100", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST5", "001", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST6", "111", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Binary_" + this.TokenNames + "_TEST7", "11", Prime.SharedStorageService.Context.DUT));
            this.SharedStorageMock.Verify(o => o.InsertRowAtTable("Binary_" + this.TokenNames, "01100110000111111", Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: fail.
        /// Description: Set to BinaryConversion Mode and test function to ensure user input length matches token length.
        /// </summary>
        [TestMethod]
        public void Execute_BinaryConversion_InValidInput_fail()
        {
            /*Setup prime shared storage database */
            this.RecoveryMode = Modes.SerialMode_SetToken;
            this.TokenNames = "SocRecovery";
            this.ValueList = "0|1|2|3|4";
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable(this.TokenNames, Prime.SharedStorageService.Context.DUT)).Returns("00000");
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable(this.TokenNames, "01234", Prime.SharedStorageService.Context.DUT));
            this.Execute();
            /* Change mode for execute function */
            this.RecoveryMode = Modes.BinaryConversion;
            this.ValueList = "CNVI_0|IPU_1|MEDIA_2|VPU_3|TEST_4";
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable("SocRecovery", Prime.SharedStorageService.Context.DUT)).Returns("01234");

            // Execute the test. Return value should be 1 for success.
            Assert.AreEqual(this.Execute(), -1);
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Setting fail condition where ValueList is set to skip all bits in recovery string.
        /// </summary>
        [TestMethod]
        public void Execute_TokenPrint_print_pass()
        {
            this.TokenNames = "RecoveryString";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("RecoveryString", "01", Prime.SharedStorageService.Context.DUT));

            /* Change mode for execute function */
            this.RecoveryMode = Modes.Token_Print;
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT)).Returns("01");

            // Assert.AreEqual(expected_output.ToString(), this.ItuffOutput.ToString());

            // this.POCRecoveryMock.Setup(o => o.GetTrackingList(this.TokenNames)).Returns(returnList);
            // expecting to fail out port - 1 for setting value list to all 'X' skip bits.
            Assert.AreEqual(this.Execute(), 1);
            this.SharedStorageMock.Verify(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT));
        }

        /// <summary>
        /// Method Under Test: Execute().
        /// Expected Result: pass.
        /// Description: Setting token overrid condition where ValueList is set to override token print with specific entry.
        /// </summary>
        [TestMethod]
        public void Execute_TokenPrintoverride_print_pass()
        {
            var expected_output = new List<string>();
            expected_output.Add("[ITUFF]0_tname_cnvi");
            expected_output.Add("[ITUFF]0_mrslt_0.0");
            expected_output.Add("[ITUFF]0_tname_media");
            expected_output.Add("[ITUFF]0_mrslt_1.0");

            this.TokenNames = "RecoveryString";
            this.ValueList = "cnvi|media";
            this.SharedStorageMock.Setup(o => o.InsertRowAtTable("RecoveryString", "01", Prime.SharedStorageService.Context.DUT));

            /* Change mode for execute function */
            this.RecoveryMode = Modes.Token_Print;
            /* Invoke the set error mode with get row and set row - TODO replace with POC recovery mock */
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT)).Returns("01");

            Assert.AreEqual(expected_output.ToString(), this.ItuffOutput.ToString());

            Assert.AreEqual(this.Execute(), 1);
            this.SharedStorageMock.Verify(o => o.GetStringRowFromTable("RecoveryString", Prime.SharedStorageService.Context.DUT));
        }
    }
}
