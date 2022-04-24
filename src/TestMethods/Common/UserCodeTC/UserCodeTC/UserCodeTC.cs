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

namespace UserCodeTC
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Reflection;
    using DDG;
    using IronPython.Runtime.Types;
    using Microsoft.Scripting.Hosting;
    using Prime.PhAttributes;
    using Prime.SharedStorageService;
    using Prime.TestMethods;
    using UserCodeCallbacks;

    /// <summary>
    /// Defined UserCodeTC.
    /// </summary>
    [PrimeTestMethod]
    public class UserCodeTC : TestMethodBase
    {
        private string localFile;
        private object assemblyObject;
        private MethodInfo methodInfo;
        private ScriptSource pythonScript;
        private ScriptScope scope;

        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        public TestMethodsParams.File InputFile { get; set; }

        /// <summary>
        /// Gets or sets the namespace.class name where the method to run is.
        /// </summary>
        public TestMethodsParams.String NamespaceClass { get; set; }

        /// <summary>
        /// Gets or sets the method name to run.
        /// </summary>
        public TestMethodsParams.String Method { get; set; }

        /// <summary>
        /// Gets or sets the file system interface.
        /// </summary>
        protected IFileSystem FileSystem_ { get; set; } = new FileSystem();

        /// <inheritdoc />
        public override void Verify()
        {
            this.localFile = DDG.FileUtilities.GetFile(this.InputFile);
            if (this.localFile.EndsWith(".py"))
            {
                var pythonEngine = IronPython.Hosting.Python.CreateEngine();

                var servicesType = typeof(Prime.Services);
                var members = servicesType.GetProperties();
                var services = new Dictionary<string, object>();
                foreach (var module in members)
                {
                    services[module.Name] = module.GetValue(servicesType, null);
                }

                this.scope = pythonEngine.CreateScope(services);
                this.scope.SetVariable("Context", DynamicHelpers.GetPythonTypeFromType(typeof(Context)));
                this.pythonScript = pythonEngine.CreateScriptSourceFromString(this.FileSystem_.File.ReadAllText(this.localFile));
            }
            else if (this.localFile.EndsWith(".cs"))
            {
                var fileContents = this.FileSystem_.File.ReadAllText(this.localFile);

                var results = UserCodeCallbacks.CompileCode(fileContents);

                this.assemblyObject = results.CompiledAssembly.CreateInstance(this.NamespaceClass);
                if (this.assemblyObject == null)
                {
                    throw new Exception($"Failed compiling code {this.InputFile}.");
                }

                this.methodInfo = this.assemblyObject.GetType().GetMethod(this.Method);
            }
            else
            {
                throw new ArgumentException($"File={this.localFile} using incorrect extension.");
            }
        }

        /// <inheritdoc />
        [Returns(10, PortType.Pass, "Pass!")]
        [Returns(9, PortType.Pass, "Pass!")]
        [Returns(8, PortType.Pass, "Pass!")]
        [Returns(7, PortType.Pass, "Pass!")]
        [Returns(6, PortType.Pass, "Pass!")]
        [Returns(5, PortType.Pass, "Pass!")]
        [Returns(4, PortType.Pass, "Pass!")]
        [Returns(3, PortType.Pass, "Pass!")]
        [Returns(2, PortType.Pass, "Pass!")]
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            if (this.localFile.EndsWith(".py"))
            {
                this.scope.SetVariable("ExitPort", -1);
                this.pythonScript.Execute(this.scope);
                return (int)this.scope.GetVariable("ExitPort");
            }

            Prime.Services.ConsoleService.PrintDebug($"Invoking {this.Method} in {this.NamespaceClass} from file {this.InputFile}.");
            var result = this.methodInfo.Invoke(this.assemblyObject, null) as string;
            return result.ToInt();
        }
    }
}