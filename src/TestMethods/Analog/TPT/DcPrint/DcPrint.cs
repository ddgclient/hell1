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

namespace DcPrint
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Prime;
    using Prime.ConsoleService;
    using Prime.DcService;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.TestMethods.Dc;
    using Prime.Utilities;

    /// <summary>
    /// This class is intended to overwrite the test method PrimeDcTestMethod.
    /// </summary>
    [PrimeTestMethod]
    public class DcPrint : PrimeDcTestMethod, IDcExtensions
    {
        /// <summary>
        /// Gets or sets the PerPinDcSetup. Hack until the base class makes checking the results available.
        /// </summary>
        public DcSetup PerPinDcSetup { get; set; } = new DcSetup();

        /// <inheritdoc />
        public override void CustomVerify()
        {
            // This is all copied from Base.Verify() ... its only here so we can create perPinDcSetup to
            // call AreAllDcResultsWithinLimits in CustomPostProcessResults.
            List<int> samplingCount = new List<int>();
            var pinNames = this.Pins.ToList();
            var measurementTypes = new List<MeasurementType>();
            DcCommon.VerifyMeasurementType(this.MeasurementTypes.ToList(), pinNames.Count, ref measurementTypes);
            DcCommon.VerifyPinsDuplications(pinNames, this.SamplingCount, ref samplingCount);
            DcCommon.ResolveLimits(measurementTypes, this.LowLimits.LimitsToList(), this.HighLimits.LimitsToList(), out var lowLimits, out var highLimits);
            DcCommon.PreparePerPinLimits(pinNames, ref lowLimits, ref highLimits);
            this.PerPinDcSetup = DcCommon.PreparePerPinDcSetup(pinNames, ref lowLimits, ref highLimits, samplingCount, measurementTypes);
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass")]
        [Returns(0, PortType.Fail, "Fail")]
        public override int Execute()
        {
            return base.Execute();
        }

        /// <summary>
        /// Writes the calculated values to ITUFF.
        /// </summary>
        /// <param name="results">DC Data returned from testing.</param>
        void IDcExtensions.CustomPostProcessResults(IDcResults results)
        {
            if (results == null)
            {
                Prime.Services.ConsoleService.PrintDebug("CustomPostProcessResults: [results] is null, setting exit port to 0.");
                this.ExitPort = 0;
                return; // check if null
            }

            // Get all of the pin groups
            List<IPinGroupDcResults> dcGroupResults = results.GetAllPinGroupsDcResults();

            // check if empty
            if (dcGroupResults == null || dcGroupResults.Count == 0)
            {
                Prime.Services.ConsoleService.PrintDebug("CustomPostProcessResults: GetAllPinGroupsDcResults returned null or .Count==0. Setting exit port to 0.");
                this.ExitPort = 0;
                return; // is empty
            }

            // setup iTUFF writer and set precision to 8 (10 n<unit> measurement)
            var ituffWriter = Prime.Services.DatalogService.GetItuffMrsltWriter();
            ituffWriter.SetPrecision(8);
            var failure = false;

            // loop over pin results, get a group and then loop again
            foreach (IPinGroupDcResults pinDcResults in dcGroupResults)
            {
                List<IPinDcResults> pinDc = pinDcResults.GetAllPinsDcResults();
                foreach (IPinDcResults pins in pinDc)
                {
                    // get results and get an average if multiple pins measured
                    List<double> pinVals = pins.GetPinDcResults();
                    double outputVal = -9999.0;

                    if (pinVals != null && pinVals.Count >= 1)
                    {
                        outputVal = pinVals.Average();
                    }
                    else
                    {
                        Prime.Services.ConsoleService.PrintDebug("No values found");
                        failure = true;
                    }

                    // Write to iTUFF the test name and the pin name
                    ituffWriter.SetTnamePostfix("_" + pins.GetPinName());
                    ituffWriter.SetData(outputVal);
                    Prime.Services.DatalogService.WriteToItuff(ituffWriter);
                }
            }

            // copied from base implementation.
            if (!failure)
            {
                bool areDcResultsWithinLimits = results.AreAllDcResultsWithinLimits(this.PerPinDcSetup);
                this.ExitPort = areDcResultsWithinLimits ? (ushort)1 : (ushort)0;
            }

            return;
        }
    }
}