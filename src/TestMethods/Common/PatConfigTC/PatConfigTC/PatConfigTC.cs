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

namespace PatConfigTC
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using DDG;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.PatConfigService;
    using Prime.PhAttributes;
    using Prime.TestMethods;

    /// <summary>
    /// Test method responsible for executing different variations of functional test.
    /// </summary>
    [PrimeTestMethod]
    public class PatConfigTC : TestMethodBase
    {
        private List<(string tag, string configuration, HdmtExpression value, IPatConfigHandle handle)> entries;
        private List<HdmtExpression> tags;
        private string localFile;

        /// <summary>
        /// Gets or sets a IFileSystem for Mocking.
        /// </summary>
        public IFileSystem FileWrapper { get; set; } = new FileSystem();

        /// <summary>
        /// Gets or sets the input file with all tags.
        /// </summary>
        public TestMethodsParams.File InputFile { get; set; }

        /// <summary>
        /// Gets or sets the optional plist regular expression.
        /// </summary>
        public TestMethodsParams.String PlistRegEx { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of tags to apply during pattern(s) modifications.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString Tags { get; set; }

        /// <summary>
        /// Gets or sets interface to skip console printing when LogLevel is not enabled.
        /// </summary>
        protected IConsoleService Console_ { get; set; }

        /// <summary>
        /// Gets or sets configuration data.
        /// </summary>
        protected List<Configuration> ConfigurationData_ { get; set; }

        /// <inheritdoc/>
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            var handles = new List<IPatConfigHandle>();
            foreach (var entry in this.entries)
            {
                foreach (var tag in this.tags)
                {
                    var tagValue = Convert.ToString(tag.Evaluate());

                    if (tagValue != entry.tag)
                    {
                        continue;
                    }

                    var value = Convert.ToString(entry.value.Evaluate());
                    var expectedSize = entry.handle.GetExpectedDataSize();
                    if (expectedSize < (ulong)value.Length)
                    {
                        throw new Exception($"Invalid size. Data=[{value}] is not matching expected size=[{expectedSize}] for configuration=[{entry.handle.GetConfigurationName()}].");
                    }

                    if (expectedSize > (ulong)value.Length)
                    {
                        value = value.PadLeft((int)expectedSize, '0');
                    }

                    this.Console_?.PrintDebug($"Processing tag=[{entry.tag}], configuration=[{entry.handle.GetConfigurationName()}, value=[{value}].");

                    entry.handle.SetData(value);
                    handles.Add(entry.handle);
                }
            }

            Prime.Services.PatConfigService.Apply(handles);
            return 1;
        }

        /// <inheritdoc/>
        public override void Verify()
        {
            this.Console_ = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;
            var tagsList = this.Tags.ToList();
            this.tags = new List<HdmtExpression>();
            foreach (var tag in tagsList)
            {
                this.tags.Add(new HdmtExpression(tag));
            }

            this.entries = new List<(string tag, string configuration, HdmtExpression value, IPatConfigHandle handle)>();
            this.localFile = DDG.FileUtilities.GetFile(this.InputFile);
            this.ConfigurationData_ = JsonConvert.DeserializeObject<List<Configuration>>(this.FileWrapper.File.ReadAllText(this.localFile));

            foreach (var configuration in this.ConfigurationData_)
            {
                this.Console_?.PrintDebug($"Adding tag=[{configuration.Tag}], PatConfig=[{configuration.PatConfig}], value=[{configuration.Data}]");
                var handle = string.IsNullOrEmpty(this.PlistRegEx)
                    ? Prime.Services.PatConfigService.GetPatConfigHandle(configuration.PatConfig)
                    : Prime.Services.PatConfigService.GetPatConfigHandle(configuration.PatConfig, this.PlistRegEx);
                this.entries.Add((configuration.Tag, configuration.PatConfig, new HdmtExpression(configuration.Data), handle));
            }
        }
    }
}