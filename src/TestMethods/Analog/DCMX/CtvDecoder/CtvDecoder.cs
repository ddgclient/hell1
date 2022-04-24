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

namespace CtvDecoder
{
    using System.Collections.Generic;
    using global::CtvServices;
    using Prime.Base.Exceptions;
    using Prime.FunctionalService;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;

    /// <summary>
    /// This class is intended to overwrite the members of the IFunctionalExtensions interfaces to extend the test method PrimeFuncCaptureCtvTestMethod.
    /// </summary>
    [PrimeTestMethod]
    public class CtvDecoder : PrimeFunctionalTestMethod, IFunctionalExtensions
    {
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> ituffResults = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> ituffFailFields = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        private ushort exitPort;
        private Dictionary<string, dynamic> dataStructure;
        private ICaptureCtvPerPinTest funcTest;
        private List<string> capturePins;
        private bool funcTestStatus;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtvDecoder"/> class.
        /// </summary>
        public CtvDecoder()
        {
            this.CtvServices = new CtvServices();
        }

        /// <summary>
        /// List of available EnabledDisabled states.
        /// </summary>
        public enum EnabledDisabled
        {
            /// <summary>
            /// ENABLED.
            /// </summary>
            ENABLED,

            /// <summary>
            /// DISABLED.
            /// </summary>
            DISABLED,
        }

        /// <summary>
        /// Gets or sets CSV file with dataStructure parameters.
        /// </summary>
        public TestMethodsParams.File ConfigurationFile { get; set; }

        /// <summary>
        /// Gets or sets the TssidRename.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString TssidRename { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the CsvDelimiter.
        /// </summary>
        public TestMethodsParams.String CsvDelimiter { get; set; } = ",";

        /// <summary>
        /// Gets or sets the CtvServices.
        /// </summary>
        public CtvServices CtvServices { get; set; }

        /// <inheritdoc />
        public override void CustomVerify()
        {
            /* Moved from Verify */
            this.capturePins = this.CtvCapturePins;
            this.funcTest = Prime.Services.FunctionalService.CreateCaptureCtvPerPinTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.capturePins, this.PrePlist);
            if (this.capturePins.Count == 0)
            {
                throw new TestMethodException("No capture pins were provided - expecting at least one capture pin to be specified.");
            } /* End noved from Verify */

            // Initializes the dictionary with the file content.
            this.dataStructure = this.CtvServices.CtvStructureInit(
                this.ConfigurationFile,
                this.CtvCapturePins,
                this.TssidRename,
                this.CsvDelimiter);
        }

        /// <inheritdoc />
        bool IFunctionalExtensions.ProcessCtvPerPin(Dictionary<string, string> ctvData)
        {
            // Execute CtvStructureProcessing
            this.CtvServices.CtvStructureProcessing(
                this.CtvCapturePins,
                ctvData,
                ref this.exitPort,
                this.dataStructure,
                this.ituffResults,
                this.ituffFailFields);

            // Ituff printing
            ItuffFormat.PrintToItuff(this.ituffResults);
            ItuffFormat.PrintToItuff(this.ituffFailFields);
            return this.exitPort == 1;
        }

        /// <inheritdoc />
        [Returns(6, PortType.Fail, "FAIL PORT")]
        [Returns(5, PortType.Fail, "FAIL PORT")]
        [Returns(4, PortType.Fail, "FAIL PORT")]
        [Returns(3, PortType.Fail, "FAIL PORT")]
        [Returns(2, PortType.Fail, "FAIL PORT")]
        [Returns(1, PortType.Pass, "PASS PORT")]
        [Returns(0, PortType.Fail, "FAIL PORT")]
        public override int Execute()
        {
            this.funcTest.ApplyTestConditions();
            this.funcTestStatus = this.funcTest.Execute();
            if (!this.funcTestStatus)
            {
                Prime.Services.ConsoleService.PrintDebug("Functional test execution failed");
            }

            Dictionary<string, string> ctvData = this.funcTest.GetCtvData();

            Prime.Services.ConsoleService.PrintDebug(ctvData.CtvDataToString());
            this.TestMethodExtension.ProcessCtvPerPin(ctvData);
            return this.exitPort;
        }
    }
}
