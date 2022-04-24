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

namespace ScanSSNHry
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Prime.Base.Exceptions;
    using Prime.DatalogService;
    using Prime.FunctionalService;
    using Prime.PlistService;

    /// <summary>
    ///  Function to process the input files.
    /// </summary>
    public class InputsProcessed
    {
        /// <summary>
        /// Gets or sets HryInputData.
        /// This is the parsed data from .json file.
        /// </summary>
        public HRYTemplateInput HryInputDataProcessed { get; set; } = new HRYTemplateInput();

        /// <summary>
        /// Gets or sets pin mapping data.
        /// This is the parsed data from .json file.
        /// </summary>
        public PinMappingInput PinMappingProcessed { get; set; } = new PinMappingInput();

        /// <summary>
        /// Gets or sets UnassignedPartitions.
        /// This will hold the partition indexes that is not covered by any patterns/group/pin/range combination.
        /// </summary>
        public HashSet<int> UnassignedPartitions { get; set; }

        /// <summary>
        /// This function is used to read the json files and create the list according to each input file.
        /// </summary>
        /// <param name="jsonTemplatepath">string with the path for the jsonTemplate file.</param>
        /// <param name="jsonPinMappingpath">string with the path for the jsonPinMapping file.</param>
        public virtual void ProcessData(string jsonTemplatepath, string jsonPinMappingpath)
        {
            string localTemplateFilePath = Prime.Services.FileService.GetFile(jsonTemplatepath);
            string localPinFilePath = Prime.Services.FileService.GetFile(jsonPinMappingpath);
            this.HryInputDataProcessed = JsonConvert.DeserializeObject<HRYTemplateInput>(File.ReadAllText(localTemplateFilePath));
            this.PinMappingProcessed = JsonConvert.DeserializeObject<PinMappingInput>(File.ReadAllText(localPinFilePath));
            this.PopulateUnassignedPartitions();
        }

        /// <summary>
        /// This function veriify if hte patterns on the plist are mapped on the json file.
        /// Function added by Evaristo.
        /// </summary>
        /// <param name="patlist">Name of plist that is used to evaluate the patterns on the json. </param>
        public virtual void PatternsInJson(string patlist)
        {
            IPlistObject plistObj = Prime.Services.PlistService.GetPlistObject(patlist);
            HashSet<string> patternsInPlist = plistObj.GetUniquePatternNames();
            foreach (var pattern in patternsInPlist)
            {
                bool pattern_inJson = false;
                foreach (var regularExpression in this.HryInputDataProcessed.Patterns)
                {
                    Regex regex = new Regex(regularExpression.ToString());
                    Match match = regex.Match(pattern);
                    if (match.Success)
                    {
                        pattern_inJson = true;
                        Console.WriteLine($"Pattern= [{pattern}] is defined on JSON file.\n");
                    }
                }

                if (!pattern_inJson)
                {
                    throw new TestMethodException($"Pattern= [{pattern}] is not defined in the json file.\n");
                }
            }
        }

        /// <summary>
        /// This private function finds the debugPartitions indexes that are not covere in by any patterns/group/pin/range combination.
        /// The superset of the partition indexes is actually all number between 0 and HryLength.
        /// HryLength is a field specified by user in .json input file.
        /// </summary>
        private void PopulateUnassignedPartitions()
        {
            this.UnassignedPartitions = new HashSet<int>(Enumerable.Range(0, this.HryInputDataProcessed.HryLength).ToArray());

            foreach (var patternData in this.HryInputDataProcessed.Patterns)
            {
                foreach (var locationData in patternData.Packets)
                {
                    foreach (var instanceData in locationData.Partitions)
                    {
                        this.UnassignedPartitions.Remove(instanceData.HRYIndex);
                    }
                }
            }
        }
    }
}
