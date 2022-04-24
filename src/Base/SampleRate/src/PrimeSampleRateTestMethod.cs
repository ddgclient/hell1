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
namespace Prime.TestMethods.SampleRate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.PhAttributes;
    using Prime.Utilities;

    /// <summary>
    /// Indicates whether or not to print the exit port in ituff.
    /// </summary>
    public enum ItuffEnabled
    {
        /// <summary>
        /// Run Verify All.
        /// </summary>
        False,

        /// <summary>
        /// Do not run Verify All.
        /// </summary>
        True,
    }

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class PrimeSampleRateTestMethod : TestMethodBase
    {
        /// <summary>
        /// Object hold the sampleRateFeature information.
        /// </summary>
        private ISampleRate sampleRateFeature;

        /// <summary>
        /// Indicates what wafer sample option to be considered for sampling the wafer.
        /// </summary>
        public enum SampleOptions
        {
            /// <summary>
            /// Sample the wafer based on the wafer sample rate parameter.
            /// </summary>
            WAFER_SAMPLE_RATE,

            /// <summary>
            /// Sample only the wafer listed by the user or station controller variable.
            /// </summary>
            WAFER_LIST,

            /// <summary>
            ///  Only DUT sampling is enabled, No wafer Sampling. User should not select this, do wafer Sampling.
            /// </summary>
            DUT_SAMPLING,
        }

        /// <summary>
        /// Gets or sets the SamplingRateValue, DUT Sampling is based on this number.
        /// </summary>
        public TestMethodsParams.String SamplingRateValue { get; set; }

        /// <summary>
        /// Gets or sets the WaferSampleRateValue, Wafer Sampling is based on this number.
        /// </summary>
        public TestMethodsParams.String WaferSampleRateValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the WaferSampleList, DUT Sampling is based on this wafer list.
        /// </summary>
        public TestMethodsParams.String WaferSampleList { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Wafer Sample Form .
        /// </summary>
        public SampleOptions SampleOption { get; set; } = SampleOptions.DUT_SAMPLING;

        /// <summary>
        /// Gets or sets the PrintItuffExitport, DUT Sampling is based on this number.
        /// </summary>
        public ItuffEnabled PrintItuffExitPort { get; set; } = ItuffEnabled.False;

        /// <summary>
        /// Gets or sets, SkipPort Information, what port the DUT has to exit out , whether to print the ituff information or not.
        /// </summary>
        /// valid.
        public override sealed void Verify()
        {
            this.PopulateCurrentSampleRateFeatureInfo();
            if (!this.sampleRateFeature.Verify())
            {
                    throw new TestMethodException("Failed sampleRateFeature Verify.");
            }
        }

        /// <inheritdoc />
        [Returns(2, PortType.Pass, "Pass!")]
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            try
            {
                // Port 0 ==> Error encountered in sample Rate test instance.
                // Port 1 ==> Sampling for the current unit to be done.
                // Port 2 ==> NO sampling for the current unit to be done.
                if (this.sampleRateFeature.Execute())
                {
                    this.PrintExitPortInfoToItuff(1);
                    Prime.Services.ConsoleService.PrintDebug("Exiting to port 1.\n");
                    return 1;
                }
                else
                {
                    this.PrintExitPortInfoToItuff(2);
                    Prime.Services.ConsoleService.PrintDebug("Exiting to port 2.\n");
                    return 2;
                }
            }
            catch (Base.Exceptions.TestMethodException ex)
            {
                Prime.Services.ConsoleService.PrintError(ex.ToString());
                return 0;
            }
        }

        private void PopulateCurrentSampleRateFeatureInfo()
        {
            if (this.SampleOption == SampleOptions.WAFER_SAMPLE_RATE)
            {
                DutSampling dutsampling = new DutSampling(this.SamplingRateValue);
                this.sampleRateFeature = new WaferRateSampling(this.WaferSampleRateValue, dutsampling);
                Prime.Services.ConsoleService.PrintDebug("WAFER SAMPLE RATE option is specified.\n");
            }
            else if (this.SampleOption == SampleOptions.WAFER_LIST)
            {
                Prime.Services.ConsoleService.PrintDebug("WAFER LIST option is specified.\n");
                DutSampling dutsampling = new DutSampling(this.SamplingRateValue);
                this.sampleRateFeature = new WaferListSampling(this.WaferSampleList, dutsampling);
            }
            else
            {
                Prime.Services.ConsoleService.PrintDebug("DUT sample option is specified.\n");
                this.sampleRateFeature = new DutSampling(this.SamplingRateValue);
            }
        }

        /// <summary>
        /// Log the port information in the output.
        /// </summary>
        private void PrintExitPortInfoToItuff(double port)
        {
            if (this.PrintItuffExitPort == ItuffEnabled.True)
            {
                var mrsltWritter = Prime.Services.DatalogService.GetItuffMrsltWriter();
                mrsltWritter.SetPrecision(0);
                mrsltWritter.SetTnamePostfix("_EDC");
                mrsltWritter.SetData(port);
                Prime.Services.DatalogService.WriteToItuff(mrsltWritter);
            }
        }
    }
}