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

namespace MbistHRYTC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ARR_MBIST;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;

    /// <summary>
    /// MBIST HRY Test Template.
    /// </summary>
    [PrimeTestMethod]
    public class MbistHRYTC : PrimeFunctionalTestMethod, IFunctionalExtensions
    {
        private ushort exitPort = 0;

        /// <summary>Simple enum so parameters can be True/False.</summary>
        public enum MyBoolean
        {
            /// <summary>True.</summary>
            TRUE,

            /// <summary>False.</summary>
            FALSE,
        }

        /// <summary>Gets or sets the name of the HRY JSON configuration file.</summary>
        public TestMethodsParams.String HRYInputFile { get; set; }

        /// <summary>Gets or sets Retest Mode.</summary>
        public MyBoolean RetestMode { get; set; } = MyBoolean.FALSE;

        /// <summary>Gets or sets Log To Ituff Mode.</summary>
        public MyBoolean LogToItuff { get; set; } = MyBoolean.TRUE;

        private MbistHRYAlgorithm HryAlgorithm { get; set; } = new MbistHRYAlgorithm();

        private MbistHRYInput.MbistLookupTable LookupTable { get; set; }

        private int HryLength { get; set; }

        private List<string> CapturePinsList { get; set; }

        /// <inheritdoc/>
        bool IFunctionalExtensions.ProcessCtvPerPin(Dictionary<string, string> ctvData)
        {
            Prime.Services.ConsoleService.PrintDebug($"Running ProcessCtvPerPin with RetestMode={this.RetestMode} LogToItuff={this.LogToItuff}.");

            var serializedData = Mbist.SerializeCaptureData(ctvData, this.CapturePinsList, this.LookupTable.CaptureInterLeaveMode);
            Prime.Services.ConsoleService.PrintDebug($"SerializedCaptureData=[{serializedData}].");

            var hryDataStr = this.HryAlgorithm.GenerateHRY(this.LookupTable, serializedData, this.HryLength, this.RetestMode == MyBoolean.TRUE);
            Prime.Services.ConsoleService.PrintDebug($"CurrentHRY=[{hryDataStr}].");

            // calculate the exit port.
            this.exitPort = (ushort)this.HryAlgorithm.CalculateExitPort(hryDataStr, this.RetestMode == MyBoolean.TRUE);
            Prime.Services.ConsoleService.PrintDebug($"ExitPort=[{this.exitPort}].");

            // get the existing hry data
            string originalHryStr = Mbist.GetMbistHRYData();
            Prime.Services.ConsoleService.PrintDebug($"Previous HRY_RAWSTR_MBIST=[{originalHryStr}].");

            // merge the old HRY_RAWSTR_MBIST with the new hry string
            var finalHryStr = hryDataStr;
            if (originalHryStr.Length == hryDataStr.Length)
            {
                finalHryStr = this.HryAlgorithm.MergeHry(originalHryStr, hryDataStr, this.RetestMode == MyBoolean.TRUE);
            }
            else
            {
                Prime.Services.ConsoleService.PrintDebug($"Current HRY and HRY_RAWSTR_MBIST are different lengths ({hryDataStr.Length} vs {originalHryStr.Length}), using Current.");
            }

            Prime.Services.ConsoleService.PrintDebug($"Saving new HRY_RAWSTR_MBIST=[{hryDataStr}].");

            // and finally upload the new HRY_RAWSTR_MBIST
            Mbist.SetMbistHRYData(finalHryStr);

            // log to ituff if requested, keep max char per line under 4000
            if (this.LogToItuff == MyBoolean.TRUE)
            {
                Prime.Services.ConsoleService.PrintDebug($"Attempting to write final hry to Ituff.");
                /* var ituffStr = this.HryAlgorithm.GenerateItuffData(finalHryStr, 4000);
                Prime.Services.ConsoleService.PrintDebug($"[ITUFF]{ituffStr.Replace("\n", "\n[ITUFF]")}");
                Prime.Services.DatalogService.WriteToItuff(ituffStr); */
                var writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
                writer.SetData(finalHryStr);
                Prime.Services.DatalogService.WriteToItuff(writer);
            }

            return true;
        }

        /// <inheritdoc/>
        [Returns(3, PortType.Fail, "Pattern Issue")]
        [Returns(2, PortType.Fail, "Unrepairable")]
        [Returns(1, PortType.Pass, "Repaired")]
        [Returns(0, PortType.Fail, "FAIL PORT")]
        public override int Execute()
        {
            base.Execute();
            return this.exitPort;
        }

        /// <summary>CustomVerify.</summary>
        public override void CustomVerify()
        {
            // ------ Load the HRY input file
            var hryInputData = this.HryAlgorithm.LoadInputFile(this.HRYInputFile);
            if (hryInputData == null)
            {
                throw new FileLoadException($"Error, Could not load HRY JSON file [{this.HRYInputFile}] correctly.");
            }

            // ------ Make sure the Patlist has an HRY table and save it.
            this.LookupTable = hryInputData.GetLookupTable(this.Patlist);
            if (this.LookupTable == null)
            {
                throw new ArgumentException($"Error, PatList=[{this.Patlist}] is not found in HryInput=[{this.HRYInputFile}]");
            }

            this.HryLength = hryInputData.HryLength;

            // ------ Check the Capture pins in the HryTable
            this.CapturePinsList = Mbist.ResolveCapturePins(this.CtvCapturePins.ToList(), this.LookupTable.CapturePins.Split(',').ToList());
            if (this.CapturePinsList.Count == 0)
            {
                throw new ArgumentException($"Mismatch between HRY Table CapturePins and CtvCapturePins parameter.");
            }
        }
    }
}
