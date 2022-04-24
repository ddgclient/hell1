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

namespace VminAggregatorTC
{
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using DDG;
    using Newtonsoft.Json;
    using Prime.Base.Utilities;
    using Prime.PhAttributes;
    using Prime.TestMethods;

    /// <summary>
    /// Defined UserCodeTC.
    /// </summary>
    [PrimeTestMethod]
    public class VminAggregatorTC : TestMethodBase
    {
        private string localFile;

        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        public TestMethodsParams.File InputFile { get; set; }

        /// <summary>
        /// Gets or sets the file system interface.
        /// </summary>
        protected IFileSystem FileSystem_ { get; set; } = new FileSystem();

        /// <summary>
        /// Gets or sets the configuration data from input file.
        /// </summary>
        protected List<Configuration> ConfigurationData_ { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            this.localFile = DDG.FileUtilities.GetFile(this.InputFile);
            this.ConfigurationData_ = JsonConvert.DeserializeObject<List<Configuration>>(this.FileSystem_.File.ReadAllText(this.localFile));
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            var writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            foreach (var configuration in this.ConfigurationData_)
            {
                var vmins = new List<double>();
                foreach (var core in configuration.VminExpressions)
                {
                    var finalVmin = -8888D;
                    foreach (var vmin in core
                                 .Select(vminExpression =>
                                 {
                                     try
                                     {
                                         return (double)vminExpression.Evaluate();
                                     }
                                     catch
                                     {
                                         return -8888D;
                                     }
                                 })
                                 .Where(vmin => !vmin.Equals(-8888D, 3) && (vmin.Equals(-9999D, 3) || vmin > finalVmin)))
                    {
                        finalVmin = vmin;
                    }

                    vmins.Add(finalVmin);
                }

                var concatenated = string.Join("|", vmins.Select(o => o.Equals(-9999D, 3) ? "-9999" : o.Equals(-8888D, 3) ? "-8888" : $"{o:N3}"));
                var frequencyString = (string)configuration.Frequency.Evaluate();
                var frequency = frequencyString.StringWithUnitsToDouble() / 1E09;
                var dataString = $"{frequency:N3}@{concatenated}";
                writer.SetTnamePostfix($"|{configuration.Domain}@{configuration.Corner}");
                writer.SetData(dataString);
                Prime.Services.DatalogService.WriteToItuff(writer);

                if (!string.IsNullOrWhiteSpace(configuration.DffToken))
                {
                    Prime.Services.DffService.SetDff(configuration.DffToken, dataString);
                }
            }

            return 1;
        }
    }
}