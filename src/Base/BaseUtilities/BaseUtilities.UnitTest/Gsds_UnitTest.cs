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
    using Prime.SharedStorageService;

    /// <summary>
    /// Defines the <see cref="Gsds_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Gsds_UnitTest
    {
        /// <summary>
        /// test the IsTokenFormat method.
        /// </summary>
        [TestMethod]
        public void GSDS_IsTokenFormat()
        {
            Assert.IsTrue(DDG.Gsds.IsTokenFormat("G.U.D.blah"));
            Assert.IsTrue(DDG.Gsds.IsTokenFormat("G.L.D.blah"));
            Assert.IsTrue(DDG.Gsds.IsTokenFormat("G.I.D.blah"));

            Assert.IsTrue(DDG.Gsds.IsTokenFormat("G.U.I.blah"));
            Assert.IsTrue(DDG.Gsds.IsTokenFormat("G.L.I.blah"));
            Assert.IsTrue(DDG.Gsds.IsTokenFormat("G.I.I.blah"));

            Assert.IsTrue(DDG.Gsds.IsTokenFormat("G.U.O.blah"));
            Assert.IsTrue(DDG.Gsds.IsTokenFormat("G.L.O.blah"));
            Assert.IsTrue(DDG.Gsds.IsTokenFormat("G.I.O.blah"));

            Assert.IsFalse(DDG.Gsds.IsTokenFormat("blah"));
            Assert.IsFalse(DDG.Gsds.IsTokenFormat("X.L.O.blah"));
            Assert.IsFalse(DDG.Gsds.IsTokenFormat("G.X.O.blah"));
            Assert.IsFalse(DDG.Gsds.IsTokenFormat("G.L.X.blah"));
        }

        /// <summary>
        /// Test the GSDS functionality, all passing cases.
        /// </summary>
        [TestMethod]
        public void GSDS_ReadWrite_Pass()
        {
            var storageContainer = new Dictionary<string, string>();

            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Loose);
            gsdsMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Context>())).
                Callback((string key, string value, Context context) => storageContainer[GenerateKey("string", context, key)] = value);
            gsdsMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Context>())).
                Callback((string key, int value, Context context) => storageContainer[GenerateKey("int", context, key)] = value.ToString());
            gsdsMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<Context>())).
                Callback((string key, double value, Context context) => storageContainer[GenerateKey("double", context, key)] = value.ToString());
            /*gsdsMock.Setup(o => o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>())).
             Callback((string key, object value, Context context) => storageContainer[GenerateKey("object", context, key)] = JsonConvert.SerializeObject((string)value)); */

            gsdsMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), It.IsAny<Context>())).
                Returns((string key, Context context) => storageContainer[GenerateKey("string", context, key)]);
            gsdsMock.Setup(o => o.GetDoubleRowFromTable(It.IsAny<string>(), It.IsAny<Context>())).
                Returns((string key, Context context) => storageContainer[GenerateKey("double", context, key)].ToDouble());
            gsdsMock.Setup(o => o.GetIntegerRowFromTable(It.IsAny<string>(), It.IsAny<Context>())).
                Returns((string key, Context context) => storageContainer[GenerateKey("int", context, key)].ToInt());
            gsdsMock.Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>())).
                Returns((string key, Type type, Context context) => storageContainer[GenerateKey("object", context, key)]);

            gsdsMock.Setup(o => o.KeyExistsInStringTable(It.IsAny<string>(), It.IsAny<Context>())).
                Returns((string key, Context context) => storageContainer.ContainsKey(GenerateKey("string", context, key)));
            gsdsMock.Setup(o => o.KeyExistsInDoubleTable(It.IsAny<string>(), It.IsAny<Context>())).
                Returns((string key, Context context) => storageContainer.ContainsKey(GenerateKey("double", context, key)));
            gsdsMock.Setup(o => o.KeyExistsInIntegerTable(It.IsAny<string>(), It.IsAny<Context>())).
                Returns((string key, Context context) => storageContainer.ContainsKey(GenerateKey("int", context, key)));
            gsdsMock.Setup(o => o.KeyExistsInObjectTable(It.IsAny<string>(), It.IsAny<Context>())).
                Returns((string key, Context context) => storageContainer.ContainsKey(GenerateKey("object", context, key)));

            Prime.Services.SharedStorageService = gsdsMock.Object;

            string token = "SomeGsdsTokenName";
            string stringValUnit = "StringValUnit";
            string stringValLot = "StringValLot";
            string stringValIp = "StringValIp";

            double doubleValUnit = 5.892;
            double doubleValLot = 100.45;
            double doubleValIp = 23.9;

            int intValUnit = 5;
            int intValLot = 10;
            int intValIp = -47;

            TestClassForStorage objectValUnit = new TestClassForStorage("Unit");
            TestClassForStorage objectValLot = new TestClassForStorage("Lot");
            TestClassForStorage objectValIp = new TestClassForStorage("Ip");

            // Verify none of the keys exist.
            Assert.IsFalse(DDG.Gsds.TokenExists("G.U.S." + token));
            Assert.IsFalse(DDG.Gsds.TokenExists("G.L.S." + token));
            Assert.IsFalse(DDG.Gsds.TokenExists("G.I.S." + token));

            Assert.IsFalse(DDG.Gsds.TokenExists("G.U.D." + token));
            Assert.IsFalse(DDG.Gsds.TokenExists("G.L.D." + token));
            Assert.IsFalse(DDG.Gsds.TokenExists("G.I.D." + token));

            Assert.IsFalse(DDG.Gsds.TokenExists("G.U.I." + token));
            Assert.IsFalse(DDG.Gsds.TokenExists("G.L.I." + token));
            Assert.IsFalse(DDG.Gsds.TokenExists("G.I.I." + token));

            Assert.IsFalse(DDG.Gsds.TokenExists("G.U.O." + token));
            Assert.IsFalse(DDG.Gsds.TokenExists("G.L.O." + token));
            Assert.IsFalse(DDG.Gsds.TokenExists("G.I.O." + token));

            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.U.S." + token));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.L.S." + token));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.I.S." + token));

            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.U.D." + token));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.L.D." + token));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.I.D." + token));

            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.U.I." + token));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.L.I." + token));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.I.I." + token));

            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.U.O." + token));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.L.O." + token));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.I.O." + token));

            // Write all the gsds values.
            DDG.Gsds.WriteToken("G.U.S." + token, stringValUnit);
            DDG.Gsds.WriteToken("G.L.S." + token, stringValLot);
            DDG.Gsds.WriteToken("G.I.S." + token, stringValIp);

            DDG.Gsds.WriteToken("G.U.D." + token, doubleValUnit.ToString());
            DDG.Gsds.WriteToken("G.L.D." + token, doubleValLot.ToString());
            DDG.Gsds.WriteToken("G.I.D." + token, doubleValIp.ToString());

            DDG.Gsds.WriteToken("G.U.I." + token, intValUnit.ToString());
            DDG.Gsds.WriteToken("G.L.I." + token, intValLot.ToString());
            DDG.Gsds.WriteToken("G.I.I." + token, intValIp.ToString());

            // disabled write with objects, just force values to read.
            /*DDG.Gsds.WriteToken("G.U.O." + token, JsonConvert.SerializeObject(objectValUnit));
            DDG.Gsds.WriteToken("G.L.O." + token, JsonConvert.SerializeObject(objectValLot));
            DDG.Gsds.WriteToken("G.I.O." + token, JsonConvert.SerializeObject(objectValIp)); */
            storageContainer[GenerateKey("object", Context.DUT, token)] = JsonConvert.SerializeObject(objectValUnit);
            storageContainer[GenerateKey("object", Context.LOT, token)] = JsonConvert.SerializeObject(objectValLot);
            storageContainer[GenerateKey("object", Context.IP, token)] = JsonConvert.SerializeObject(objectValIp);

            // Verify all the keys exist.
            Assert.IsTrue(DDG.Gsds.TokenExists("G.U.S." + token));
            Assert.IsTrue(DDG.Gsds.TokenExists("G.L.S." + token));
            Assert.IsTrue(DDG.Gsds.TokenExists("G.I.S." + token));

            Assert.IsTrue(DDG.Gsds.TokenExists("G.U.D." + token));
            Assert.IsTrue(DDG.Gsds.TokenExists("G.L.D." + token));
            Assert.IsTrue(DDG.Gsds.TokenExists("G.I.D." + token));

            Assert.IsTrue(DDG.Gsds.TokenExists("G.U.I." + token));
            Assert.IsTrue(DDG.Gsds.TokenExists("G.L.I." + token));
            Assert.IsTrue(DDG.Gsds.TokenExists("G.I.I." + token));

            Assert.IsTrue(DDG.Gsds.TokenExists("G.U.O." + token));
            Assert.IsTrue(DDG.Gsds.TokenExists("G.L.O." + token));
            Assert.IsTrue(DDG.Gsds.TokenExists("G.I.O." + token));

            Assert.IsTrue(DDG.Gsds.IsTokenFormatAndExists("G.U.S." + token));
            Assert.IsTrue(DDG.Gsds.IsTokenFormatAndExists("G.L.S." + token));
            Assert.IsTrue(DDG.Gsds.IsTokenFormatAndExists("G.I.S." + token));

            Assert.IsTrue(DDG.Gsds.IsTokenFormatAndExists("G.U.D." + token));
            Assert.IsTrue(DDG.Gsds.IsTokenFormatAndExists("G.L.D." + token));
            Assert.IsTrue(DDG.Gsds.IsTokenFormatAndExists("G.I.D." + token));

            Assert.IsTrue(DDG.Gsds.IsTokenFormatAndExists("G.U.I." + token));
            Assert.IsTrue(DDG.Gsds.IsTokenFormatAndExists("G.L.I." + token));
            Assert.IsTrue(DDG.Gsds.IsTokenFormatAndExists("G.I.I." + token));

            Assert.IsTrue(DDG.Gsds.IsTokenFormatAndExists("G.U.O." + token));
            Assert.IsTrue(DDG.Gsds.IsTokenFormatAndExists("G.L.O." + token));
            Assert.IsTrue(DDG.Gsds.IsTokenFormatAndExists("G.I.O." + token));

            // Read all the gsds values back.
            Assert.AreEqual(stringValUnit, (string)DDG.Gsds.ReadToken("G.U.S." + token));
            Assert.AreEqual(stringValLot, (string)DDG.Gsds.ReadToken("G.L.S." + token));
            Assert.AreEqual(stringValIp, (string)DDG.Gsds.ReadToken("G.I.S." + token));

            Assert.AreEqual(doubleValUnit, (double)DDG.Gsds.ReadToken("G.U.D." + token));
            Assert.AreEqual(doubleValLot, (double)DDG.Gsds.ReadToken("G.L.D." + token));
            Assert.AreEqual(doubleValIp, (double)DDG.Gsds.ReadToken("G.I.D." + token));

            Assert.AreEqual(intValUnit, (int)DDG.Gsds.ReadToken("G.U.I." + token));
            Assert.AreEqual(intValLot, (int)DDG.Gsds.ReadToken("G.L.I." + token));
            Assert.AreEqual(intValIp, (int)DDG.Gsds.ReadToken("G.I.I." + token));

            Assert.AreEqual(JsonConvert.SerializeObject(objectValUnit), (string)DDG.Gsds.ReadToken("G.U.O." + token));
            Assert.AreEqual(JsonConvert.SerializeObject(objectValLot), (string)DDG.Gsds.ReadToken("G.L.O." + token));
            Assert.AreEqual(JsonConvert.SerializeObject(objectValIp), (string)DDG.Gsds.ReadToken("G.I.O." + token));

            gsdsMock.VerifyAll();
        }

        /// <summary>
        /// Test the GSDS functionality, all failing cases (except for Key not found since that's a Prime generated error that isn't caught).
        /// </summary>
        [TestMethod]
        public void GSDS_ReadWrite_Fail()
        {
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.WriteToken("WrongFormat", "value"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.WriteToken("L.S.WrongFormat", "value"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.WriteToken("G.U.X.WrongFormat", "value"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.WriteToken("G.L.X.WrongFormat", "value"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.WriteToken("G.I.X.WrongFormat", "value"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.WriteToken("G.X.S.WrongFormat", "value"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.WriteToken("G.X.D.WrongFormat", "value"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.WriteToken("G.X.I.WrongFormat", "value"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.WriteToken("X.U.S.WrongFormat", "value"));

            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.ReadToken("WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.ReadToken("L.S.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.ReadToken("G.U.X.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.ReadToken("G.L.X.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.ReadToken("G.I.X.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.ReadToken("G.X.S.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.ReadToken("G.X.D.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.ReadToken("G.X.I.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.ReadToken("X.U.S.WrongFormat"));

            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.TokenExists("WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.TokenExists("L.S.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.TokenExists("G.U.X.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.TokenExists("G.L.X.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.TokenExists("G.I.X.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.TokenExists("G.X.S.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.TokenExists("G.X.D.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.TokenExists("G.X.I.WrongFormat"));
            Assert.ThrowsException<ArgumentException>(() => DDG.Gsds.TokenExists("X.U.S.WrongFormat"));

            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("WrongFormat"));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("L.S.WrongFormat"));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.U.X.WrongFormat"));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.L.X.WrongFormat"));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.I.X.WrongFormat"));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.X.S.WrongFormat"));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.X.D.WrongFormat"));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("G.X.I.WrongFormat"));
            Assert.IsFalse(DDG.Gsds.IsTokenFormatAndExists("X.U.S.WrongFormat"));
        }

        private static string GenerateKey(string table, Context context, string token)
        {
            return table + "!" + context.ToString() + "!" + token;
        }

        /// <summary>
        /// Dummy Type to check out object storage.
        /// </summary>
        public class TestClassForStorage
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestClassForStorage"/> class.
            /// </summary>
            /// <param name="key">TypeDesignation.</param>
            public TestClassForStorage(string key)
            {
                this.TypeDesignation = key;
            }

            /// <summary>
            /// Gets or sets the TypeDesignation string.
            /// </summary>
            public string TypeDesignation { get; set; }
        }
    }
}
