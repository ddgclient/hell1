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
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.UserVarService;

    /// <summary>
    /// Defines the <see cref="UserVar_UnitTest" />.
    /// </summary>
    [TestClass]
    public class UserVar_UnitTest
    {
        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_Exists_Pass()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.Exists("Collection.Name1")).Returns(true);
            userVarServiceMock.Setup(o => o.Exists("Collection.Name2")).Returns(false);
            userVarServiceMock.Setup(o => o.Exists("Collection.Name3")).Throws(new Prime.Base.Exceptions.FatalException("invalid format"));
            Prime.Services.UserVarService = userVarServiceMock.Object;

            Assert.IsTrue(DDG.UserVar.Exists("Collection.Name1"));
            Assert.IsFalse(DDG.UserVar.Exists("Collection.Name2"));
            Assert.IsFalse(DDG.UserVar.Exists("Collection.Name3"));
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_WriteBoolean_Pass()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.SetValue("Collection.Name1", true));
            Prime.Services.UserVarService = userVarServiceMock.Object;

            DDG.UserVar.Write("Collection.Name1", UserVar.ValidTypes.BOOLEAN, "true");
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_WriteDouble_Pass()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.SetValue("Collection.Name1", 3.14159));
            Prime.Services.UserVarService = userVarServiceMock.Object;

            DDG.UserVar.Write("Collection.Name1", UserVar.ValidTypes.DOUBLE, "3.14159");
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_WriteInteger_Pass()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.SetValue("Collection.Name1", 42));
            Prime.Services.UserVarService = userVarServiceMock.Object;

            DDG.UserVar.Write("Collection.Name1", UserVar.ValidTypes.INTEGER, "42");
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_WriteString_Pass()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.SetValue("Collection.Name1", "blah"));
            Prime.Services.UserVarService = userVarServiceMock.Object;

            DDG.UserVar.Write("Collection.Name1", UserVar.ValidTypes.STRING, "blah");
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_WriteArrayBoolean_Pass()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.SetValue("Collection.Name1", new List<bool> { true, true, false, false }));
            Prime.Services.UserVarService = userVarServiceMock.Object;

            DDG.UserVar.Write("Collection.Name1", UserVar.ValidTypes.ARRAYBOOLEAN, "true,true,false,false");
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_WriteArrayDouble_Pass()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.SetValue("Collection.Name1", new List<double> { 1.1, 2.2, 3.3, 4.4 }));
            Prime.Services.UserVarService = userVarServiceMock.Object;

            DDG.UserVar.Write("Collection.Name1", UserVar.ValidTypes.ARRAYDOUBLE, "1.1,2.2,3.3,4.4");
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_WriteArrayInteger_Pass()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.SetValue("Collection.Name1", new List<int> { 1, 1, 2, 3, 5, 8 }));
            Prime.Services.UserVarService = userVarServiceMock.Object;

            DDG.UserVar.Write("Collection.Name1", UserVar.ValidTypes.ARRAYINTEGER, "1,1,2,3,5,8");
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_WriteArrayString_Pass()
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.SetValue("Collection.Name1", new List<string> { "A", "B", "C" }));
            Prime.Services.UserVarService = userVarServiceMock.Object;

            DDG.UserVar.Write("Collection.Name1", UserVar.ValidTypes.ARRAYSTRING, "A,B,C");
            userVarServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_ReadBool_Pass()
        {
            var userVarServiceMock = this.MockUserVarServiceReadUnique(UserVar.ValidTypes.BOOLEAN, "Collection.NameX", "true");
            Assert.AreEqual(true, Convert.ToBoolean(DDG.UserVar.ReadAndGetType("Collection.NameX", out var actualType)));
            Assert.AreEqual(UserVar.ValidTypes.BOOLEAN, actualType);
            userVarServiceMock.Verify(o => o.GetBoolValue("Collection.NameX"));
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_ReadDouble_Pass()
        {
            var userVarServiceMock = this.MockUserVarServiceReadUnique(UserVar.ValidTypes.DOUBLE, "Collection.NameX", "3.14");
            Assert.AreEqual(3.14, Convert.ToDouble(DDG.UserVar.ReadAndGetType("Collection.NameX", out var actualType)));
            Assert.AreEqual(UserVar.ValidTypes.DOUBLE, actualType);
            userVarServiceMock.Verify(o => o.GetDoubleValue("Collection.NameX"));
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_ReadInt_Pass()
        {
            var userVarServiceMock = this.MockUserVarServiceReadUnique(UserVar.ValidTypes.INTEGER, "Collection.NameX", "37");
            Assert.AreEqual(37, Convert.ToInt32(DDG.UserVar.ReadAndGetType("Collection.NameX", out var actualType)));
            Assert.AreEqual(UserVar.ValidTypes.INTEGER, actualType);
            userVarServiceMock.Verify(o => o.GetIntValue("Collection.NameX"));
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_ReadString_Pass()
        {
            var userVarServiceMock = this.MockUserVarServiceReadUnique(UserVar.ValidTypes.STRING, "Collection.NameX", "myvalue");
            Assert.AreEqual("myvalue", DDG.UserVar.ReadAndGetType("Collection.NameX", out var actualType));
            Assert.AreEqual(UserVar.ValidTypes.STRING, actualType);
            userVarServiceMock.Verify(o => o.GetStringValue("Collection.NameX"));
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_ReadArrayBool_Pass()
        {
            var userVarServiceMock = this.MockUserVarServiceReadUnique(UserVar.ValidTypes.ARRAYBOOLEAN, "Collection.NameX", "true,true,false,false");
            Assert.AreEqual("True,True,False,False", DDG.UserVar.ReadAndGetType("Collection.NameX", out var actualType));
            Assert.AreEqual(UserVar.ValidTypes.ARRAYBOOLEAN, actualType);
            userVarServiceMock.Verify(o => o.GetArrayBoolValue("Collection.NameX"));
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_ReadArrayDouble_Pass()
        {
            var userVarServiceMock = this.MockUserVarServiceReadUnique(UserVar.ValidTypes.ARRAYDOUBLE, "Collection.NameX", "9.1,9.2,9.3,9.4");
            Assert.AreEqual("9.1,9.2,9.3,9.4", DDG.UserVar.ReadAndGetType("Collection.NameX", out var actualType));
            Assert.AreEqual(UserVar.ValidTypes.ARRAYDOUBLE, actualType);
            userVarServiceMock.Verify(o => o.GetArrayDoubleValue("Collection.NameX"));
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_ReadArrayInt_Pass()
        {
            var userVarServiceMock = this.MockUserVarServiceReadUnique(UserVar.ValidTypes.ARRAYINTEGER, "Collection.NameX", "10,11,12,13,14");
            Assert.AreEqual("10,11,12,13,14", DDG.UserVar.ReadAndGetType("Collection.NameX", out var actualType));
            Assert.AreEqual(UserVar.ValidTypes.ARRAYINTEGER, actualType);
            userVarServiceMock.Verify(o => o.GetArrayIntValue("Collection.NameX"));
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_ReadArrayString_Pass()
        {
            var userVarServiceMock = this.MockUserVarServiceReadUnique(UserVar.ValidTypes.ARRAYSTRING, "Collection.NameX", "A,B,C,D,E,F");
            Assert.AreEqual("A,B,C,D,E,F", DDG.UserVar.ReadAndGetType("Collection.NameX", out var actualType));
            Assert.AreEqual(UserVar.ValidTypes.ARRAYSTRING, actualType);
            userVarServiceMock.Verify(o => o.GetArrayStringValue("Collection.NameX"));
        }

        /// <summary>
        /// Test the UserVar functions.
        /// </summary>
        [TestMethod]
        public void UserVar_ReadInvalidTypeException()
        {
            var name = "SomeCollection.SomeVar";
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.GetBoolValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetDoubleValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetIntValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetStringValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetArrayBoolValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetArrayDoubleValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetArrayIntValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetArrayStringValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            Prime.Services.UserVarService = userVarServiceMock.Object;

            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintError("Failed to read UserVar=[SomeCollection.SomeVar]: not a valid user var.", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => DDG.UserVar.ReadAndGetType("SomeCollection.SomeVar", out var actualType));
            Assert.AreEqual("Failed to read UserVar=[SomeCollection.SomeVar]: not a valid user var.", ex.Message);
            userVarServiceMock.VerifyAll();
            consoleServiceMock.VerifyAll();
        }

        private Mock<IUserVarService> MockUserVarServiceReadUnique(UserVar.ValidTypes typeToMock, string name, string value)
        {
            var userVarServiceMock = new Mock<IUserVarService>(MockBehavior.Strict);
            userVarServiceMock.Setup(o => o.GetBoolValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetDoubleValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetIntValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetStringValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetArrayBoolValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetArrayDoubleValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetArrayIntValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));
            userVarServiceMock.Setup(o => o.GetArrayStringValue(name)).Throws(new Prime.Base.Exceptions.FatalException("not a valid user var."));

            switch (typeToMock)
            {
                case UserVar.ValidTypes.BOOLEAN:
                    userVarServiceMock.Setup(o => o.GetBoolValue(name)).Returns(Convert.ToBoolean(value));
                    break;
                case UserVar.ValidTypes.DOUBLE:
                    userVarServiceMock.Setup(o => o.GetDoubleValue(name)).Returns(Convert.ToDouble(value));
                    break;
                case UserVar.ValidTypes.INTEGER:
                    userVarServiceMock.Setup(o => o.GetIntValue(name)).Returns(Convert.ToInt32(value));
                    break;
                case UserVar.ValidTypes.STRING:
                    userVarServiceMock.Setup(o => o.GetStringValue(name)).Returns(value);
                    break;
                case UserVar.ValidTypes.ARRAYBOOLEAN:
                    userVarServiceMock.Setup(o => o.GetArrayBoolValue(name)).Returns(value.Split(',').Select(it => Convert.ToBoolean(it)).ToList());
                    break;
                case UserVar.ValidTypes.ARRAYDOUBLE:
                    userVarServiceMock.Setup(o => o.GetArrayDoubleValue(name)).Returns(value.Split(',').Select(it => Convert.ToDouble(it)).ToList());
                    break;
                case UserVar.ValidTypes.ARRAYINTEGER:
                    userVarServiceMock.Setup(o => o.GetArrayIntValue(name)).Returns(value.Split(',').Select(it => Convert.ToInt32(it)).ToList());
                    break;
                default: /* ValidTypes.ARRAYSTRING: */
                    userVarServiceMock.Setup(o => o.GetArrayStringValue(name)).Returns(value.Split(',').ToList());
                    break;
            }

            Prime.Services.UserVarService = userVarServiceMock.Object;
            return userVarServiceMock;
        }
    }
}
