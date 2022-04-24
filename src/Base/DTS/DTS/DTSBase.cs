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

namespace DTSBase
{
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using DDG;
    using Newtonsoft.Json;
    using Prime.PhAttributes;
    using Prime.SharedStorageService;
    using Prime.TestMethods;

    /// <summary>
    /// Voltage Converter Test Class. Run during INIT to read configuration.
    /// </summary>
    [PrimeTestMethod]
    public class DTSBase : TestMethodBase
    {
        private string localFileName;

        /// <summary>
        /// Gets or sets the voltage converter ActiveConfiguration file.
        /// </summary>
        public TestMethodsParams.File ConfigurationFile { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file system interface.
        /// </summary>
        protected IFileSystem FileSystem_ { get; set; } = new FileSystem();

        /// <inheritdoc />
        public override void Verify()
        {
            this.localFileName = FileUtilities.GetFile(this.ConfigurationFile);
            var fileContents = this.FileSystem_.File.ReadAllText(this.localFileName);
            Prime.Services.ConsoleService.PrintDebug($"File Contents=[{fileContents}]");
            var settings = JsonConvert.DeserializeObject<List<Configuration>>(fileContents);
            Prime.Services.SharedStorageService.InsertRowAtTable(DDG.DTS.DTSConfigurationTable, settings, Context.DUT);
            Prime.Services.SharedStorageService.OverrideObjectRowResetPolicy(DDG.DTS.DTSConfigurationTable, ResetPolicy.NEVER_RESET, Context.DUT);
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            return 1;
        }
    }
}