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

namespace ConcurrentPlistTracer
{
    using Prime.FunctionalService;
    using Prime.PatConfigService;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;
    using VminTC;

    /// <summary>
    /// This class is intended to overwrite the test method PrimeFunctionalTestMethod.
    /// </summary>
    [PrimeTestMethod]
    public class ConcurrentPlistTracer : PrimeFunctionalTestMethod, IFunctionalExtensions
    {
        /// <summary>
        /// Gets or sets the name of the Prime PatConfig used to add/remove CTV data for tracing.
        /// </summary>
        public TestMethodsParams.String PatConfigForCtv { get; set; }

        private IPatConfigHandle PatConfigAddCTV { get; set; }

        private IPatConfigHandle PatConfigRemoveCTV { get; set; }

        /// <inheritdoc />
        public override void CustomVerify()
        {
            // create the patconfig handles.
            this.PatConfigAddCTV = Prime.Services.PatConfigService.GetPatConfigHandleWithPlist(this.PatConfigForCtv, this.Patlist);
            this.PatConfigAddCTV.FillData(PatternSymbol.ONE);
            this.PatConfigRemoveCTV = Prime.Services.PatConfigService.GetPatConfigHandleWithPlist(this.PatConfigForCtv, this.Patlist);
            this.PatConfigRemoveCTV.FillData(PatternSymbol.ZERO);
        }

        /// <inheritdoc />
        [Returns(2, PortType.Fail, "FAIL PORT")]
        [Returns(1, PortType.Pass, "PASS PORT")]
        [Returns(0, PortType.Fail, "FAIL PORT")]
        public override int Execute()
        {
            Prime.Services.ConsoleService.PrintDebug($"Applying PatConfig=[{this.PatConfigForCtv}] with Data=[1+]");
            Prime.Services.PatConfigService.Apply(this.PatConfigAddCTV);

            var exitPort = base.Execute();

            Prime.Services.ConsoleService.PrintDebug($"Applying PatConfig=[{this.PatConfigForCtv}] with Data=[0+]");
            Prime.Services.PatConfigService.Apply(this.PatConfigRemoveCTV);

            return exitPort;
        }

        /// <inheritdoc />
        bool IFunctionalExtensions.ProcessCtvPerCycle(ICaptureCtvPerCycleTest captureCtvPerCycleTest)
        {
            return VminUtilities.TraceCTVs(captureCtvPerCycleTest);
        }
    }
}
