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

namespace DDGTriggeredDC
{
    using System;
    using System.Collections.Generic;
    using Prime.Base.Exceptions;
    using Prime.DcService;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.TestMethods.TriggeredDc;

    /// <summary>
    /// This class is intended to overwrite the test method PrimeTriggeredDcTestMethod.
    /// </summary>
    [PrimeTestMethod]
    public class DDGTriggeredDC : PrimeTriggeredDcTestMethod, ITriggeredDcExtensions
    {
        /// <summary>
        /// Gets or sets a list of GSDS/SharedStorage to save the results too.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString SaveResults { get; set; } = string.Empty;

        /// <inheritdoc />
        void ITriggeredDcExtensions.CustomPostProcessResults(IDcResults results)
        {
            // this function is in the base class.
            this.PrintToDatalog(results);

            // save results to gsds.
            if (!string.IsNullOrWhiteSpace(this.SaveResults))
            {
                this.SaveResultsToGsds(results, this.SaveResults);
            }
        }

        private void SaveResultsToGsds(IDcResults results, List<string> gsdsList)
        {
            var resultsPerPin = new Dictionary<string, List<double>>();
            var totalResults = 0;
            foreach (var singlePinGroupResult in results.GetAllPinGroupsDcResults())
            {
                foreach (var singlePinResult in singlePinGroupResult.GetAllPinsDcResults())
                {
                    var pin = singlePinResult.GetPinName();
                    resultsPerPin[pin] = singlePinResult.GetPinDcResults();
                    totalResults += resultsPerPin[pin].Count;
                }
            }

            var pinList = this.Pins.ToList();
            if (gsdsList.Count == totalResults)
            {
                var gsdsIndex = 0;
                foreach (var pin in pinList)
                {
                    if (!resultsPerPin.ContainsKey(pin))
                    {
                        throw new TestMethodException($"Pin from Pins Parameter=[{pin}] does not have any DC results.");
                    }

                    foreach (var result in resultsPerPin[pin])
                    {
                        var roundedResult = Math.Round(result, 6); // to match the rounding used when logging to ituff.
                        Prime.Services.ConsoleService.PrintDebug($"Saving Pin=[{pin}] Result=[{roundedResult}] to Gsds=[{gsdsList[gsdsIndex]}].");
                        DDG.Gsds.WriteToken(gsdsList[gsdsIndex], roundedResult);
                        gsdsIndex++;
                    }
                }
            }
            else
            {
                throw new TestMethodException($"{gsdsList.Count} GSDS tokens supplied, but {totalResults} DC results measured. There must be exactly one GSDS listed for each result.");
            }
        }
    }
}
