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

namespace UserCodeCallbacks
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Reflection;
    using CommandLine;
    using Microsoft.CSharp;

    /// <summary>
    /// Defines the <see cref="UserCodeCallbacks" />.
    /// </summary>
    public static class UserCodeCallbacks
    {
        /// <summary>
        /// Gets or sets bag with all compiled objects for future reference.
        /// </summary>
        public static ConcurrentDictionary<Tuple<string, string>, object> CompiledObjects_ { get; set; } = new ConcurrentDictionary<Tuple<string, string>, object>();

        /// <summary>
        /// Gets or sets file system interface.
        /// </summary>
        public static IFileSystem FileSystem_ { get; set; } = new FileSystem();

        /// <summary>
        /// Callback method to compile code as callback.
        /// </summary>
        /// <param name="args">Commandline parameters.</param>
        /// <returns>Pass or Fail.</returns>
        public static string CompileUserCode(string args)
        {
            var result = "Fail";
            var parserResult = Parser.Default.ParseArguments<UserCodeCallbacksOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            parserResult.WithParsed(options =>
                {
                    var localFile = DDG.FileUtilities.GetFile(options.File);
                    var key = new Tuple<string, string>(localFile, options.NamespaceClass);
                    CompiledObjects_.TryRemove(key, out _);
                    var fileContents = FileSystem_.File.ReadAllText(localFile);
                    var compilerResult = CompileCode(fileContents);
                    var assemblyObject = compilerResult.CompiledAssembly.CreateInstance(options.NamespaceClass);
                    if (assemblyObject == null)
                    {
                        throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Unable to create instance for {options.NamespaceClass}.");
                    }

                    if (!CompiledObjects_.TryAdd(key, assemblyObject))
                    {
                        throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Unable to add object in ConcurrentDictionary.");
                    }

                    result = "pass";
                }).
                WithNotParsed(e => throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: failed parsing arguments. {string.Join("\n", e)}"));

            return result;
        }

        /// <summary>
        /// Callback method to run code as callback.
        /// </summary>
        /// <param name="args">Commandline parameters.</param>
        /// <returns>Pass or Fail.</returns>
        public static string RunUserCode(string args)
        {
            var result = "fail";
            var parserResult = Parser.Default.ParseArguments<UserCodeCallbacksOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            parserResult.WithParsed(options =>
                {
                    var localFile = DDG.FileUtilities.GetFile(options.File);
                    var key = new Tuple<string, string>(localFile, options.NamespaceClass);
                    if (!CompiledObjects_.TryGetValue(key, out var assemblyObject))
                    {
                        var fileContents = FileSystem_.File.ReadAllText(localFile);
                        var compilerResult = CompileCode(fileContents);
                        assemblyObject = compilerResult.CompiledAssembly.CreateInstance(options.NamespaceClass);
                        if (assemblyObject == null)
                        {
                            throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Unable to create instance for {options.NamespaceClass}.");
                        }

                        if (!CompiledObjects_.TryAdd(key, assemblyObject))
                        {
                            throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Unable to add object in ConcurrentDictionary.");
                        }
                    }

                    var methodInfo = assemblyObject.GetType().GetMethod(options.Method);
                    if (methodInfo == null)
                    {
                        throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: Unable to create instance for {options.NamespaceClass} method {options.Method}.");
                    }

                    Prime.Services.ConsoleService.PrintDebug($"Invoking {options.Method} in {options.NamespaceClass} from file {options.File}.");
                    result = methodInfo.Invoke(assemblyObject, null) as string;
                }).
                WithNotParsed(e => throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: failed parsing arguments. {string.Join("\n", e)}"));

            return result;
        }

        /// <summary>
        /// Compile code using object reference to get pointer to assembly directory.
        /// </summary>
        /// <param name="fileContents">Code to compile.</param>
        /// <returns>CompilerResults.</returns>
        public static CompilerResults CompileCode(string fileContents)
        {
            var providerOptions = new Dictionary<string, string>();
            var provider = new CSharpCodeProvider(providerOptions);
            var compilerParams = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false,
                TempFiles = new TempFileCollection("c:/temp", false),
            };

            var servicesType = typeof(Prime.Services);
            var members = servicesType.GetProperties();
            var modules = new List<string> { servicesType.Assembly.Location };
            modules.AddRange(members.Select(member => member.PropertyType.Assembly.Location));

            modules = modules.Distinct().ToList();
            foreach (var module in modules)
            {
                compilerParams.ReferencedAssemblies.Add(module);
            }

            var results = provider.CompileAssemblyFromSource(compilerParams, fileContents);

            if (results.Errors.Count != 0)
            {
                var errorDetail = results.Errors.Cast<CompilerError>().Aggregate(string.Empty, (current, error) =>
                    current + ("Line number " + error.Line + ", Error Number: " + error.ErrorNumber + ", '" + error.ErrorText +
                               ";" + Environment.NewLine));

                throw new ArgumentException($"Failed compiling assembly. {errorDetail}");
            }

            return results;
        }
    }
}
