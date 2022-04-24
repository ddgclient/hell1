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

namespace UserCodeTC.UnitTest
{
    using System;
    using System.Collections.Concurrent;
    using System.IO.Abstractions.TestingHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.FileService;
    using Prime.SharedStorageService;
    using UserCodeCallbacks;

    /// <summary>
    /// PowerSequenceHandler_UnitTest.
    /// </summary>
    [TestClass]
    public class UserCodeTC_UnitTest : UserCodeTC
    {
        private Mock<IConsoleService> consoleServiceMock;

        /// <summary>
        /// TestInitialize.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            this.consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            this.consoleServiceMock.Setup(
                    o => o.PrintDebug(It.IsAny<string>())).
                Callback((string msg) =>
                {
                    System.Console.WriteLine($"DEBUG: {msg}");
                });

            Prime.Services.ConsoleService = this.consoleServiceMock.Object;

            UserCodeCallbacks.CompiledObjects_ = new ConcurrentDictionary<Tuple<string, string>, object>();
        }

        /// <summary>
        /// Execute_Basic_Pass.
        /// </summary>
        [TestMethod]
        public void Execute_Basic_Pass()
        {
            this.InputFile = "SomeFile.cs";
            this.NamespaceClass = "SomeNamespace.SomeClass";
            this.Method = "HelloWorld";
            const string source =
@"
namespace SomeNamespace
{
    using Prime.ConsoleService;
    public class SomeClass
    {
        public string HelloWorld()
        {
            Prime.Services.ConsoleService.PrintDebug(""Hello world!"");
            return ""1"";    
        }
    }
}
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile.cs", mockFile);
            this.FileSystem_ = fileSystemMock;
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists(this.InputFile.ToString())).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile.cs");
            Prime.Services.FileService = fileService.Object;

            this.Verify();
            var executeResult = this.Execute();
            Assert.AreEqual(1, executeResult);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Execute_Python_Pass()
        {
            var sharedStorageServiceMock = new Mock<ISharedStorageService>();
            sharedStorageServiceMock.Setup(o => o.InsertRowAtTable("Key", -7, Context.DUT));
            sharedStorageServiceMock.Setup(o => o.GetIntegerRowFromTable("Key", Context.DUT)).Returns(-7);
            Prime.Services.SharedStorageService = sharedStorageServiceMock.Object;
            this.InputFile = "SomeFile.py";
            const string source =
@"
SharedStorageService.InsertRowAtTable('Key', -7, Context.DUT)
value = SharedStorageService.GetIntegerRowFromTable('Key', Context.DUT)
ConsoleService.PrintDebug('value=' + str(value))
ExitPort = 1
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile.py", mockFile);
            this.FileSystem_ = fileSystemMock;
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists(this.InputFile.ToString())).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile.py");
            Prime.Services.FileService = fileService.Object;

            this.Verify();
            var executeResult = this.Execute();
            Assert.AreEqual(1, executeResult);
        }

        /// <summary>
        /// Execute_Basic_Pass.
        /// </summary>
        [TestMethod]
        public void Verify_MissingType_Fail()
        {
            this.InputFile = "SomeFile.cs";
            this.NamespaceClass = "MissingNamespace"; // Invalid, CompiledAssembly.CreateInstance will return null;
            this.Method = "HelloWorld";
            const string source =
@"
namespace SomeNamespace
{
    using Prime.ConsoleService;
    public class SomeClass
    {
        public string HelloWorld()
        {
            Prime.Services.ConsoleService.PrintDebug(""Hello world!"");
            return ""1"";    
        }
    }
}
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile.cs", mockFile);
            this.FileSystem_ = fileSystemMock;
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists(this.InputFile.ToString())).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile.cs");
            Prime.Services.FileService = fileService.Object;

            var ex = Assert.ThrowsException<Exception>(() => this.Verify());
            Assert.AreEqual("Failed compiling code SomeFile.cs.", ex.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void Verify_FailedCompile_Fail()
        {
            this.InputFile = "SomeFile";
            this.NamespaceClass = "SomeNamespace.SomeClass";
            this.Method = "HelloWorld";
            const string source =
@"
namespace SomeNamespace
{
    using Prime.ConsoleService;
    public class SomeClass
    {
        public string HelloWorld(
        {
            Prime.Services.ConsoleService.PrintDebug(""Hello world!"");
            return ""1"";    
        }
    }
}
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile", mockFile);
            this.FileSystem_ = fileSystemMock;
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists(this.InputFile.ToString())).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile");
            Prime.Services.FileService = fileService.Object;

            Assert.ThrowsException<ArgumentException>(this.Verify);
        }

        /// <summary>
        /// Execute_Basic_Pass.
        /// </summary>
        [TestMethod]
        public void CallbacksCompileAndRun_Pass()
        {
            var args = "--file SomeFile --namespace.class SomeNamespace.SomeClass --method HelloWorld";
            var source =
                @"
namespace SomeNamespace
{
    using Prime.ConsoleService;
    public class SomeClass
    {
        public string HelloWorld()
        {
            Prime.Services.ConsoleService.PrintDebug(""Hello world!"");
            return ""1"";    
        }
    }
}
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile", mockFile);
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists("SomeFile")).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile");
            Prime.Services.FileService = fileService.Object;
            UserCodeCallbacks.FileSystem_ = fileSystemMock;

            var result = UserCodeCallbacks.CompileUserCode(args);
            Assert.AreEqual("pass", result);
            result = UserCodeCallbacks.RunUserCode(args);
            Assert.AreEqual("1", result);
        }

        /// <summary>
        /// Execute_Basic_Pass.
        /// </summary>
        [TestMethod]
        public void CompileUserCode_MissingType_Fail()
        {
            var args = "--file SomeFile --namespace.class SomeNamespace.WrongClass --method HelloWorld";
            var source =
                @"
namespace SomeNamespace
{
    using Prime.ConsoleService;
    public class SomeClass
    {
        public string HelloWorld()
        {
            Prime.Services.ConsoleService.PrintDebug(""Hello world!"");
            return ""1"";    
        }
    }
}
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile", mockFile);
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists("SomeFile")).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile");
            Prime.Services.FileService = fileService.Object;
            UserCodeCallbacks.FileSystem_ = fileSystemMock;

            var ex = Assert.ThrowsException<ArgumentException>(() => UserCodeCallbacks.CompileUserCode(args));
            Assert.IsTrue(ex.Message.EndsWith("Unable to create instance for SomeNamespace.WrongClass."));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void RunUserCode_ForceCompile_Pass()
        {
            var args = "--file SomeFile --namespace.class SomeNamespace.SomeClass --method HelloWorld";
            var source =
                @"
namespace SomeNamespace
{
    using Prime.ConsoleService;
    public class SomeClass
    {
        public string HelloWorld()
        {
            Prime.Services.ConsoleService.PrintDebug(""Hello world!"");
            return ""1"";    
        }
    }
}
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile", mockFile);
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists("SomeFile")).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile");
            Prime.Services.FileService = fileService.Object;
            UserCodeCallbacks.FileSystem_ = fileSystemMock;

            var result = UserCodeCallbacks.RunUserCode(args);
            Assert.AreEqual("1", result);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void RunUserCode_MissingType_Fail()
        {
            var args = "--file SomeFile --namespace.class InvalidNamespace --method HelloWorld";
            var source =
                @"
namespace SomeNamespace
{
    using Prime.ConsoleService;
    public class SomeClass
    {
        public string HelloWorld()
        {
            Prime.Services.ConsoleService.PrintDebug(""Hello world!"");
            return ""1"";    
        }
    }
}
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile", mockFile);
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists("SomeFile")).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile");
            Prime.Services.FileService = fileService.Object;
            UserCodeCallbacks.FileSystem_ = fileSystemMock;

            var ex = Assert.ThrowsException<ArgumentException>(() => UserCodeCallbacks.RunUserCode(args));
            Assert.IsTrue(ex.Message.EndsWith("Unable to create instance for InvalidNamespace."));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void RunUserCode_WrongMethodName_Fail()
        {
            var args = "--file SomeFile --namespace.class SomeNamespace.SomeClass --method WrongMethodName";
            var source =
                @"
namespace SomeNamespace
{
    using Prime.ConsoleService;
    public class SomeClass
    {
        public string HelloWorld()
        {
            Prime.Services.ConsoleService.PrintDebug(""Hello world!"");
            return ""1"";    
        }
    }
}
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile", mockFile);
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists("SomeFile")).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile");
            Prime.Services.FileService = fileService.Object;
            UserCodeCallbacks.FileSystem_ = fileSystemMock;

            var ex = Assert.ThrowsException<ArgumentException>(() => UserCodeCallbacks.RunUserCode(args));
            Assert.IsTrue(ex.Message.EndsWith("Unable to create instance for SomeNamespace.SomeClass method WrongMethodName."));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void RunUserCode_InvalidArgs_Pass()
        {
            var args = "--file SomeFile --invalid SomeNamespace.SomeClass --method HelloWorld";
            var source =
                @"
namespace SomeNamespace
{
    using Prime.ConsoleService;
    public class SomeClass
    {
        public string HelloWorld()
        {
            Prime.Services.ConsoleService.PrintDebug(""Hello world!"");
            return ""1"";    
        }
    }
}
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile", mockFile);
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists("SomeFile")).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile");
            Prime.Services.FileService = fileService.Object;
            UserCodeCallbacks.FileSystem_ = fileSystemMock;

            Assert.ThrowsException<ArgumentException>(() => UserCodeCallbacks.RunUserCode(args));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void CompileUserCode_ErrorInArgs_Fail()
        {
            var args = "--file SomeFile --invalid SomeNamespace.SomeClass --method HelloWorld";
            var source =
                @"
namespace SomeNamespace
{
    using Prime.ConsoleService;
    public class SomeClass
    {
        public string HelloWorld()
        {
            Prime.Services.ConsoleService.PrintDebug(""Hello world!"");
            return ""1"";    
        }
    }
}
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile", mockFile);
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists("SomeFile")).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile");
            Prime.Services.FileService = fileService.Object;
            UserCodeCallbacks.FileSystem_ = fileSystemMock;

            Assert.ThrowsException<ArgumentException>(() => UserCodeCallbacks.CompileUserCode(args));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void CompileUserCode_ErrorInSource_Fail()
        {
            var args = "--file SomeFile --namespace.class SomeNamespace.SomeClass --method HelloWorld";
            var source =
                @"
namespace SomeNamespace
{
    using Prime.ConsoleService;
    public class SomeClass
    {
        public string HelloWorld()
        {
            Prime.Services.ConsoleService.PrintDebug(""Hello world!"");
            return ""1""    
        }
    }
}
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile", mockFile);
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists("SomeFile")).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile");
            Prime.Services.FileService = fileService.Object;
            UserCodeCallbacks.FileSystem_ = fileSystemMock;

            Assert.ThrowsException<ArgumentException>(() => UserCodeCallbacks.CompileUserCode(args));
        }

        /// <summary>
        /// Execute_Basic_Pass.
        /// </summary>
        [TestMethod]
        public void CallbacksRun_Pass()
        {
            var args = "--file SomeFile --namespace.class SomeNamespace.SomeClass --method HelloWorld";
            var source =
                @"
namespace SomeNamespace
{
    using Prime.ConsoleService;
    public class SomeClass
    {
        public string HelloWorld()
        {
            Prime.Services.ConsoleService.PrintDebug(""Hello world!"");
            return ""1"";    
        }
    }
}
";

            var fileSystemMock = new MockFileSystem();
            var mockFile = new MockFileData(source);
            fileSystemMock.AddFile("SomeFile", mockFile);
            var fileService = new Mock<IFileService>(MockBehavior.Strict);
            fileService.Setup(o => o.FileExists("SomeFile")).Returns(true);
            fileService.Setup(o => o.GetFile(It.IsAny<string>())).Returns("SomeFile");
            Prime.Services.FileService = fileService.Object;
            UserCodeCallbacks.FileSystem_ = fileSystemMock;

            var result = UserCodeCallbacks.RunUserCode(args);
            Assert.AreEqual("1", result);
        }
    }
}
