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
    using System.IO;
    using System.Reflection;
    using Newtonsoft.Json;
    using Prime;
    using Prime.SharedStorageService;

    /// <summary>
    /// Recovery Class.
    /// </summary>
    public class Mapping
    {
        /// <summary>Gets or sets mapping config. </summary>
        public MappingJsonParser Map { get; set; }

        /// <summary> Load Mapping Config.</summary>
        /// <param name = "forceConfigFileParseState" > Whether to force load the files.</param>
        /// <param name = "mappingconfig" > The file name of the VFDM to load.</param>
        public virtual void LoadMappingConfig(MbistVminTC.EnableStates forceConfigFileParseState, string mappingconfig)
        {
            this.Map = new MappingJsonParser();
            if (forceConfigFileParseState == MbistVminTC.EnableStates.Enabled)
            {
                Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Force pull Mapping config JSON from RecoveryFile {mappingconfig}");
                this.Map = this.MapConfig(mappingconfig);
                if (this.Map != null)
                {
                    Prime.Services.SharedStorageService.InsertRowAtTable("MBISTMapping", this.Map, Context.LOT);
                }
            }
            else
            {
                try
                {
                    Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Pulling Mapping config JSON from Shared storage");
                    this.Map = (MappingJsonParser)Prime.Services.SharedStorageService.GetRowFromTable("MBISTMapping", typeof(MappingJsonParser), Context.LOT);
                }
                catch
                {
                    Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}]] Failed to pull Mapping config JSON from Shared storage");
                    this.Map = this.MapConfig(mappingconfig);
                    if (this.Map == null)
                    {
                        Services.ConsoleService.PrintError($"[{MethodBase.GetCurrentMethod().Name}] During parsing of the Mapping config file {mappingconfig} an issue was found and was unable to parse");
                    }
                }
            }
        }

        /// <summary>Will be called in the.</summary>
        /// <param name = "jsonfile" > Name of the JSON file to lao.</param>
        /// <returns>recoveryJsonParser class or null on fail.</returns>
        public virtual MappingJsonParser MapConfig(string jsonfile)
        {
            string localFilePath = Prime.Services.FileService.GetFile(jsonfile);
            if (string.IsNullOrEmpty(localFilePath))
            {
                Prime.Services.ConsoleService.PrintError($"[{MethodBase.GetCurrentMethod().Name}] Error, prime GetFile({jsonfile}) returned empty string, file probably doesn't exist.");
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<MappingJsonParser>(File.ReadAllText(localFilePath));
            }
            catch (JsonException ex)
            {
                Prime.Services.ConsoleService.PrintError($"[{MethodBase.GetCurrentMethod().Name}] Error, failed to load hry file=[{jsonfile}]. Exception=[{ex.Message}].");
                return null;
            }
        }

        /// <summary>Returns Name of HRY.</summary>
        /// <param name = "ksmode" > Wether HRY is KS mode or HRY.</param>
        /// <param name = "tpname" > name either shared storage or DFF.</param>
        /// <returns>recoveryJsonParser class or null on fail.</returns>
        public string Hryname(MbistVminTC.MbistTestModes ksmode, MappingJsonParser.TPTypes tpname)
        {
            if (ksmode == MbistVminTC.MbistTestModes.KS)
            {
                return this.Map.TokenHRYName(tpname, "KS");
            }
            else
            {
                return this.Map.TokenHRYName(tpname, "HRY");
            }
        }

        /// <summary>Returns Name of HRY.</summary>
        /// <param name = "name" > Field to grab.</param>
        /// <param name = "tpname" > name either shared storage or DFF.</param>
        /// <returns>recoveryJsonParser class or null on fail.</returns>
        public string Getname(MappingJsonParser.Fields name, MappingJsonParser.TPTypes tpname)
        {
            if (name == MappingJsonParser.Fields.Recovery)
            {
                return this.Map.Recovery.GetTPValue(tpname);
            }
            else if (name == MappingJsonParser.Fields.Vmin)
            {
                return this.Map.Vmin.GetTPValue(tpname);
            }

            return string.Empty;
        }

        /// <summary>Returns Name of BISR.</summary>
        /// <param name = "bisrname" > Wether HRY is KS mode or HRY.</param>
        /// <param name = "tpname" > name either shared storage or DFF.</param>
        /// <returns>recoveryJsonParser class or null on fail.</returns>
        public string Bisrname(string bisrname, MappingJsonParser.TPTypes tpname)
        {
            return this.Map.TokenBISRName(tpname, bisrname);
        }
    }
}
