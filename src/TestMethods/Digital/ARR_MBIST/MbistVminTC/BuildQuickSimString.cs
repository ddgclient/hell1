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

// ---------------------------------------------------------------
// Created By Tim Kirkham
// ---------------------------------------------------------------
namespace MbistVminTC
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>Build Hry Data Class.</summary>
    [ExcludeFromCodeCoverage]
    public class BuildQuickSimString
    {
        private string plist = "arr_mbist_x_x_tap_all_hry_all_all_parallelallsteps_ks_list";
        private string filepath = "C:\\PRIME\\GITLAB\\tgl_poc_code_4_13\\src\\TestMethods\\Digital\\ARR_MBIST\\MbistVminTC\\InputFiles\\MBIST_HRY_50x.json";
        private Hry hryclass;

       /// <summary>Main.</summary>
        public void Main()
        {
            // Display the number of command line arguments.
            var temp = this.Buildstring(this.plist, this.filepath);
            Console.WriteLine(temp);
        }

       /// <summary>Build Hry String.</summary>
        /// <param name="plist" > Plist to execute.</param>
        /// <param name="filepath" > Filepath for HRY file.</param>
        /// <returns>String to use for quicksim.</returns>
        public string Buildstring(string plist, string filepath)
        {
            this.hryclass = new Hry();
            var lookup = JsonConvert.DeserializeObject<HryJsonParser>(File.ReadAllText(filepath));

            var patterns = lookup.Plists[plist];
            var ctvstring = string.Empty;

            foreach (var pattern in patterns)
            {
                var numzeros = lookup.Patterns[pattern].CaptureCount;
                ctvstring += new string('L', numzeros);
            }

            Console.WriteLine(ctvstring);
            return ctvstring;
        }
    }
}
