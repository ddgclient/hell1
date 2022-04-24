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

namespace ExitPortFromGsds
{
    using System;
    using System.CodeDom;
    using System.IO;
    using System.Reflection;
    using Newtonsoft.Json;
    using Prime;
    using Prime.ConsoleService;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.UserVarService;
    using Prime.Utilities;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class ExitPortFromGsds : TestMethodBase
    {
        /// <summary>
        /// Gets or sets the InputFile (this comment will be used on the pre-header file).
        /// </summary>
        public TestMethodsParams.String InputFile { get; set; }

        /// <summary>
        /// Gets or sets inputFile path in SC.
        /// </summary>
        protected string InputFilePath { get; set; }

        /// <summary>
        /// Gets or sets gsds data from Json file.
        /// </summary>
        protected JsonData FileData { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            // check for empty string
            if (this.InputFile == string.Empty)
            {
                throw new ArgumentException("InputFile must be a valid JSON file.");
            }

            this.InputFilePath = Prime.Services.FileService.GetFile(this.InputFile);
            this.ReadInputFile();

            // check that either GsdsName or UserVarName is non-empty
            if (this.FileData.GsdsName == null && this.FileData.UserVarName == null)
            {
                throw new ArgumentException("GsdsName and UserVarName can't both be null!");
            }

            // check that only one of GsdsName or UserVarName is non-empty
            if (this.FileData.GsdsName != null && this.FileData.UserVarName != null)
            {
                // throw new ArgumentException("GsdsName and UserVarName can't both be used!");
                throw new ArgumentException($"{nameof(this.FileData.GsdsName)} and {nameof(this.FileData.UserVarName)} can't both be used!");
            }

            // if UserVarName not null, then check to see if UserVar exists
            if (this.FileData.UserVarName != null)
            {
                if (!Prime.Services.UserVarService.Exists(this.FileData.UserVarName))
                {
                    throw new ArgumentException($"{nameof(this.FileData.UserVarName)} doesn't exist!");
                }
            }
        }

        /// <inheritdoc />
        [Returns(5, PortType.Pass, "Pass port 5!")]
        [Returns(4, PortType.Pass, "Pass port 4!")]
        [Returns(3, PortType.Pass, "Pass port 3!")]
        [Returns(2, PortType.Pass, "Pass port 2!")]
        [Returns(1, PortType.Pass, "Pass port 1!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            string moduleFlow;

            // read GSDS
            // this.ReadGsds();
            if (this.FileData.UserVarName != null)
            {
                moduleFlow = Prime.Services.UserVarService.GetStringValue(this.FileData.UserVarName);
            }
            else
            {
                moduleFlow = Prime.Services.SharedStorageService.GetStringRowFromTable(this.FileData.GsdsName, Prime.SharedStorageService.Context.DUT);
            }

            // check if key exists in FileData.ExitPorts dict
            if (!this.FileData.ExitPorts.ContainsKey(moduleFlow))
            {
                Prime.Services.ConsoleService.PrintError($"Module flow {moduleFlow} doesn't exist in ExitPorts dict!");
                return 0;
            }
            else
            {
                // set exit port
                return this.FileData.ExitPorts[moduleFlow];
            }
        }

        /// <summary>
        /// Reads InputFile and stuffs GSDS table.
        /// </summary>
        private void ReadInputFile()
        {
            try
            {
                this.FileData = JsonConvert.DeserializeObject<JsonData>(File.ReadAllText(this.InputFilePath));
                if (this.FileData == null)
                {
                    throw new ArgumentException("No data was parsed.");
                }
            }
            catch (JsonException e)
            {
                throw new ArgumentException(e.Message);
            }
        }
    }
}