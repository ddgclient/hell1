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
namespace BisrRepairCheckTC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using MbistVminTC;
    using Newtonsoft.Json;
    using Prime;
    using Prime.FunctionalService;
    using Prime.PhAttributes;
    using Prime.SharedStorageService;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;

    /// <summary>
    /// Test method responsible for executing different variations of functional test.
    /// </summary>
    [PrimeTestMethod]
    public class BisrRepairCheckTC : PrimeFunctionalTestMethod, IFunctionalExtensions
    {
        private HryJsonParser hryJson;
        private Mapping map;

        /// <summary>
        /// Recovery Modes.
        /// </summary>
        public enum EnableState
        {
            /// <summary>
            /// Disabled.
            /// </summary>
            Disabled,

            /// <summary>
            /// Collect recoverable IPs.
            /// </summary>
            Enabled,
        }

        /// <summary> Gets or sets LookupTableConfigurationFile. </summary>
        public TestMethodsParams.String LookupTableConfigurationFile { get; set; }

        /// <summary> Gets or sets enables Refusing of parts. </summary>
        public MbistVminTC.EnableStates AllowRefuse { get; set; } = MbistVminTC.EnableStates.Disabled;

        /// <summary> Gets or sets  Mapping Config. </summary>
        public TestMethodsParams.String MappingConfig { get; set; }

        /// <summary> Gets or sets  Mapping Config. </summary>
        public TestMethodsParams.CommaSeparatedString RepairCheckBisrName { get; set; } = string.Empty;

        /// <inheritdoc />
        public override void CustomVerify()
        {
            this.hryJson = this.HryJsonParser(Prime.Services.FileService.GetFile(this.LookupTableConfigurationFile));
            this.map = new Mapping();
            if (this.MappingConfig != string.Empty)
            {
                this.map.LoadMappingConfig(MbistVminTC.EnableStates.Enabled, this.MappingConfig);
            }
        }

        /// <summary>Will be called in the.</summary>
        /// <param name = "jsonfile" > Name of the JSON file to lao.</param>
        /// <returns>bool for whether file was found or errored.</returns>
        public virtual HryJsonParser HryJsonParser(string jsonfile)
        {
            if (string.IsNullOrEmpty(jsonfile))
            {
                Prime.Services.ConsoleService.PrintError($"[HryJsonParser] Error, prime GetFile({jsonfile}) returned empty string, file probably doesn't exist.");
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<HryJsonParser>(File.ReadAllText(jsonfile));
            }
            catch (JsonException ex)
            {
                Prime.Services.ConsoleService.PrintError($"[HryJsonParser] Error, failed to load hry file=[{jsonfile}]. Exception=[{ex.Message}].");
                return null;
            }
        }

        /// <summary> Contains1 function Since it should be faster then contains. </summary>
        /// <param name="searchstring"> String to search in.</param>
        /// <returns> True if value is contained.</returns>
        public virtual bool Contains1(string searchstring)
        {
            HashSet<char> compress = new HashSet<char>();
            foreach (var character in searchstring)
            {
                compress.Add(character);
            }

            if (compress.Count == 1 && !compress.Contains('1'))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <inheritdoc />
        [Returns(0, PortType.Fail, "Refusing Not Allow")]
        [Returns(1, PortType.Pass, "Refusing")]
        [Returns(2, PortType.Pass, "No Fusing Required BIRA and FUSE Match")]
        [Returns(3, PortType.Pass, "No Fusing No repair")]
        [Returns(4, PortType.Pass, "Fusing Required but no burnt fuses prior")]

        bool IFunctionalExtensions.ProcessCtvPerPin(Dictionary<string, string> ctvData)
        {
            var bisrchanged = false;
            var bisrfused = false;
            Prime.Services.ConsoleService.PrintDebug("Processing CTV data.");
            int location = 0;
            foreach (var pin in this.CtvCapturePins.ToList())
            {
                var addname = this.map.Map.DieToPin[pin].Shortname;
                foreach (string patternname in this.hryJson.Plists[this.Patlist])
                {
                    var subctvstring = ctvData[pin].Substring(location, this.hryJson.Patterns[patternname].CaptureCount);
                    location += this.hryJson.Patterns[patternname].CaptureCount;

                    List<string> items = new List<string>();
                    if (this.RepairCheckBisrName != string.Empty)
                    {
                        items = this.RepairCheckBisrName.ToList();
                    }
                    else
                    {
                        items = this.hryJson.Patterns[patternname].Controllers.Keys.ToList();
                    }

                    foreach (var bisr in items)
                    {
                        var names = new string[] { addname, bisr, "RAW" };
                        var bisrname = string.Join("_", names.Where(s => !string.IsNullOrEmpty(s)));

                        var currentbisr = (string)Prime.Services.SharedStorageService.GetStringRowFromTable(bisrname, Context.DUT);

                        if (this.Contains1(subctvstring) == true)
                        {
                            bisrfused = true;
                            if (currentbisr == subctvstring)
                            {
                                Prime.Services.ConsoleService.PrintDebug($"{bisrname} Matched fused BISR value.");
                            }
                            else
                            {
                                bisrchanged = true;
                                Prime.Services.ConsoleService.PrintDebug($"{bisrname} Was previously fused and doesn't match.");
                                Prime.Services.ConsoleService.PrintDebug($"Old BISR:{currentbisr} Ned BISR:{subctvstring} .");
                            }
                        }
                        else
                        {
                            if (this.Contains1(currentbisr) == true)
                            {
                                bisrchanged = true;
                            }

                            Prime.Services.ConsoleService.PrintDebug($"{bisrname} Not fused yet.");
                        }
                    }
                }
            }

            if (bisrfused == false && bisrchanged == true)
            {
                this.ExitPort = 4;
            }
            else if (bisrfused == false && bisrchanged == false)
            {
                this.ExitPort = 3;
            }
            else if (bisrchanged == true && this.AllowRefuse == MbistVminTC.EnableStates.Enabled)
            {
                this.ExitPort = 1;
            }
            else if (bisrchanged == true && this.AllowRefuse == MbistVminTC.EnableStates.Disabled)
            {
                this.ExitPort = 0;
            }
            else if (bisrchanged == false)
            {
                this.ExitPort = 2;
            }

            return true;
        }

        /*/// <inheritdoc />
        bool IFunctionalExtensions.ProcessFailures(ICaptureFailureTest captureFailureTest)
        {
            Prime.Services.ConsoleService.PrintDebug("Processing CaptureFailure data.");
            this.ExitPort = 1;
            return true;
        }*/
    }
}