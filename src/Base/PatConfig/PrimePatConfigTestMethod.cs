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

namespace Prime.TestMethods.PatConfig
{
    using System;
    using global::PatConfig.Wrappers;
    using Newtonsoft.Json;
    using Prime.Base.Exceptions;
    using Prime.PhAttributes;

    /// <summary>
    /// Allow users to apply pattern modifies and fuse configs using PatConfigService.
    /// </summary>
    [PrimeTestMethod]
    public class PrimePatConfigTestMethod : TestMethodBase
    {
        /// <summary>
        /// fileWrapper for File static class mocking capabilities.
        /// </summary>
        private readonly IFileWrapper fileWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimePatConfigTestMethod"/> class.
        /// </summary>
        public PrimePatConfigTestMethod()
        {
            this.fileWrapper = new FileWrapper();
            this.PatConfigData = new PatConfig();
            this.PatConfigJson = new PatConfigJsonFile();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimePatConfigTestMethod"/> class.
        /// Receives an IFileWrapper mock object from the unit test.
        /// </summary>
        /// <param name="fileWrapper">IFileWrapper mock object.</param>
        public PrimePatConfigTestMethod(IFileWrapper fileWrapper)
        {
            this.fileWrapper = fileWrapper;
            this.PatConfigData = new PatConfig();
            this.PatConfigJson = new PatConfigJsonFile();
        }

        /// <summary>
        /// Gets or sets ConfigurationFile name.
        /// </summary>
        public TestMethodsParams.File ConfigurationFile { get; set; }

        /// <summary>
        /// Gets or sets SetPoint name.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString SetPoint { get; set; }

        /// <summary>
        /// Gets or sets Pattern list name.
        /// </summary>
        public TestMethodsParams.Plist Plist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets Pattern list name.
        /// </summary>
        public TestMethodsParams.Plist RegEx { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the PatConfigJsonFile file data contents.
        /// </summary>
        private PatConfigJsonFile PatConfigJson { get; set; }

        /// <summary>
        /// Gets or sets PatConfigData object, contains the logic to set and apply data.
        /// </summary>
        private PatConfig PatConfigData { get; set; }

        /// <inheritdoc />
        public override sealed void Verify()
        {
            // replace the IOfflineReady calls
            this.ParseJsonFile();
            /* end IOfflineReady calls */

            this.PopulateHandles();
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "PASS PORT")]
        [Returns(0, PortType.Fail, "FAIL PORT")]
        public override int Execute()
        {
            var areConfigurationsApplied = this.PatConfigData.ApplyHandles();
            if (areConfigurationsApplied)
            {
                Services.ConsoleService.PrintDebug("All configurations from SetPoint=[" + this.SetPoint + "] were applied correctly.\n");
                return 1;
            }

            Services.ConsoleService.PrintError("One or more configurations from SetPoint=[" + this.SetPoint + "]  could not be applied correctly.\n");
            return 0;
        }

        /* /// <inheritdoc />
        void IOfflineReady.VerifyOffline(ref object instanceInitDataStructure)
        {
            // offline environment (TORCH etc.)
            // or online when there is no offline datastructure to import
            this.ParseJsonFile();
            instanceInitDataStructure = this.PatConfigJson;
        }

        /// <inheritdoc />
        void IOfflineReady.Init(object instanceDataStructure)
        {
            // Online on tester
            this.PatConfigJson = (PatConfigJsonFile)instanceDataStructure;
        } */

        /// <summary>
        /// This function parses the PinData json file and stores its data into the
        /// PinDataJsonFile member.
        /// </summary>
        private void ParseJsonFile()
        {
            this.VerifyConfigurationFile();
            var localJsonPath = Prime.Services.FileService.GetFile(this.ConfigurationFile);
            try
            {
                this.PatConfigJson = JsonConvert.DeserializeObject<PatConfigJsonFile>(this.fileWrapper.ReadAllText(localJsonPath));
            }
            catch (JsonException ex)
            {
                throw new TestMethodException(
                    "Parsing of Configuration File=[" + localJsonPath + "] has failed with this error message=["
                    + ex.Message + "].\n");
            }
        }

        /// <summary>
        /// This private function verifies that the configuration file parameter is not empty.
        /// </summary>
        private void VerifyConfigurationFile()
        {
            if (this.ConfigurationFile == string.Empty)
            {
                throw new ArgumentException("ConfigurationFile parameter can't be empty.\n");
            }
        }

        /// <summary>
        /// This private function verifies that the configuration file data is valid.
        /// </summary>
        private void PopulateHandles()
        {
            this.PatConfigData.ClearHandles();
            foreach (var spt in this.SetPoint.ToList())
            {
                var toApplySetPoint = this.PatConfigJson.SetPoints.Find(sp => sp.Name == spt);

                if (toApplySetPoint == null)
                {
                    throw new ArgumentException(
                        "SetPoint=[" + spt + "] doesn't exist on ConfigurationFile.\n");
                }

                foreach (var configuration in toApplySetPoint.Configurations)
                {
                    this.PatConfigData.AddHandler(configuration, this.Plist, this.RegEx);
                }
            }
        }
    }
}