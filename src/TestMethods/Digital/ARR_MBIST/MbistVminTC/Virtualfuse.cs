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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Prime;
    using Prime.SharedStorageService;

    /// <summary>
    /// Recovery Class.
    /// </summary>
    public class Virtualfuse
    {
        /// <summary> VFDM LookupTable Shared storage name.</summary>
        private static string vfdmLookuptableName = "MBISTVFDMLookupTable";

        /// <summary>Gets or sets vfdm lookup table. </summary>
        public VirtFuseColOnlyJsonParser VfdmLookupTable { get; set; }

        /// <summary>Gets or sets virtualfusedata. </summary>
        public ConcurrentDictionary<string, Virtfuseblock> Virtualfusedata { get; set; }

        /// <summary> Reverses string.</summary>
        /// <param name = "s" > String to reverse.</param>
        /// <returns> Reversed string.</returns>
        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary> Load VFDMLookup.</summary>
        /// <param name = "forceConfigFileParseState" > Whether to force load the files.</param>
        /// <param name = "vfdmconfig" > The file name of the VFDM to load.</param>
        /// <param name = "hryreference" > This is the lookup of string to list.</param>
        public virtual void LoadVFDMlookup(MbistVminTC.EnableStates forceConfigFileParseState, string vfdmconfig, List<string> hryreference)
        {
            this.VfdmLookupTable = new VirtFuseColOnlyJsonParser();
            if (forceConfigFileParseState == MbistVminTC.EnableStates.Enabled)
            {
                Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Force pull VFDM config JSON from RecoveryFile {vfdmconfig}");
                this.VfdmLookupTable = this.VirtualFuse(Prime.Services.FileService.GetFile(vfdmconfig));
                if (this.VfdmLookupTable != null)
                {
                    this.VfdmLookupTable.Buildfuse(hryreference);
                    Prime.Services.SharedStorageService.InsertRowAtTable(vfdmLookuptableName, this.VfdmLookupTable, Context.LOT);
                }
            }
            else
            {
                try
                {
                    Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Pulling VFDM config JSON from Shared storage");
                    this.VfdmLookupTable = (VirtFuseColOnlyJsonParser)Prime.Services.SharedStorageService.GetRowFromTable(vfdmLookuptableName, typeof(VirtFuseColOnlyJsonParser), Context.LOT);
                }
                catch
                {
                    Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Failed to pull VFDM config JSON from Shared storage");
                    this.VfdmLookupTable = this.VirtualFuse(Prime.Services.FileService.GetFile(vfdmconfig));
                    if (this.VfdmLookupTable == null)
                    {
                        Services.ConsoleService.PrintError($"[{MethodBase.GetCurrentMethod().Name}] During parsing of the VFDM config file {vfdmconfig} an issue was found and was unable to parse");
                    }
                    else
                    {
                        this.VfdmLookupTable.Buildfuse(hryreference);
                        Prime.Services.SharedStorageService.InsertRowAtTable(vfdmLookuptableName, this.VfdmLookupTable, Context.LOT);
                    }
                }
            }
        }

        /// <summary>Will be called in the.</summary>
        /// <param name = "jsonfile" > Name of the JSON file to lao.</param>
        /// <returns>recoveryJsonParser class or null on fail.</returns>
        public virtual VirtFuseColOnlyJsonParser VirtualFuse(string jsonfile)
        {
            string localFilePath = Prime.Services.FileService.GetFile(jsonfile);
            if (string.IsNullOrEmpty(localFilePath))
            {
                Prime.Services.ConsoleService.PrintError($"[VirtualFuse] Error, prime GetFile({jsonfile}) returned empty string, file probably doesn't exist.");
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<VirtFuseColOnlyJsonParser>(File.ReadAllText(localFilePath));
            }
            catch (JsonException ex)
            {
                Prime.Services.ConsoleService.PrintError($"[VirtualFuse] Error, failed to load hry file=[{jsonfile}]. Exception=[{ex.Message}].");
                return null;
            }
        }

        /// <summary> Read all shared storage to internal variable. </summary>
        public void ReadAllSharedStorage()
        {
            foreach (var fuse in this.Virtualfusedata)
            {
                fuse.Value.VFDMReadSharedStorage(this.VfdmLookupTable);
            }
        }

        /// <summary> Read all shared storage to internal variable. </summary>
        public void WriteAllSharedStorage()
        {
            foreach (var fuse in this.Virtualfusedata)
            {
                fuse.Value.VFDMWriteSharedStorage();
            }
        }

        /// <summary> Creates virtualfuse data. </summary>
        public virtual void CreateVirtualfuse()
        {
            this.Virtualfusedata = new ConcurrentDictionary<string, Virtfuseblock>();
            foreach (string name in this.VfdmLookupTable.GSDSNAMEs.Keys)
            {
                this.Virtualfusedata.TryAdd(name, new Virtfuseblock(name, this.VfdmLookupTable));
                Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Initialized VFDM name {name}.");
            }
        }

        /// <summary> Block that will store one GSDS worth of data.</summary>
        public class Virtfuseblock
        {
            /// <summary> Initializes a new instance of the <see cref="Virtfuseblock"/> class. </summary>
            /// <param name="name">Name of GSDS Token.</param>
            /// <param name="virtualconfig">Virtual fuse config file.</param>
            public Virtfuseblock(string name, VirtFuseColOnlyJsonParser virtualconfig)
            {
                this.GsdsName = name;
                this.Initialize(virtualconfig);
                this.Buildfromrawgsds(virtualconfig);
            }

            /// <summary> Gets or sets List of fuses for each global.</summary>
            public virtual List<Fuse> Fuses { get; set; }

            /// <summary> Gets or sets a name of vfdm fuse.</summary>
            public virtual string GsdsName { get; set; }

            /// <summary> Gets or sets a value indicating whether fuse is available.</summary>
            public virtual string Gsdsstring { get; set; } = string.Empty;

            /// <summary> Intialize the dictionary to all x's.</summary>
            /// <param name = "virtualconfig" > Virtual fuse configuration.</param>
            public void Initialize(VirtFuseColOnlyJsonParser virtualconfig)
            {
                this.Gsdsstring = string.Empty;
                foreach (string fuse in virtualconfig.GrabVFDMblock(this.GsdsName).FusePositions)
                {
                    this.Gsdsstring += this.Buildvalue(fuse, 0, true);
                }
            }

            /// <summary> Buidsvalue for GSDS string.</summary>
            /// <param name = "location" > Startlocation for string.</param>
            /// <param name = "value" > Value of String to reverse.</param>
            /// <param name = "initialize" > If you need to intialize value or use String to reverse.</param>
            /// <returns> Reversed string.</returns>
            public string Buildvalue(string location, int value, bool initialize)
            {
                var stfin = location.Split('-').Select(int.Parse).ToList();
                int length = Math.Abs(stfin[0] - stfin[1]) + 1;
                List<dynamic> result = new List<dynamic>();
                bool reverse = false;

                if (stfin[0] > stfin[1])
                {
                    reverse = true;
                }

                if (initialize == true)
                {
                    return new string('X', length);
                }
                else
                {
                    if (reverse == true)
                    {
                        return Reverse(Convert.ToString(value, 2).PadLeft(length, '0'));
                    }
                    else
                    {
                        return Convert.ToString(value, 2).PadLeft(length, '0');
                    }
                }
            }

            /// <summary> Buidsvalue for GSDS string.</summary>
            /// <param name = "location" > Startlocation for string.</param>
            /// <param name = "value" > Value String to parse for use.</param>
            /// <returns> Reversed string.</returns>
            public string Extractvalue(string location, string value)
            {
                var stfin = location.Split('-').Select(int.Parse).ToList();
                int length = Math.Abs(stfin[0] - stfin[1]) + 1;
                bool reverse = false;

                if (stfin[0] > stfin[1])
                {
                    reverse = true;
                }

                if (reverse == true)
                {
                    return Reverse(value.Substring(stfin[1], length));
                }
                else
                {
                    return value.Substring(stfin[0], length);
                }
            }

            /// <summary>Writes shared Storage of value.</summary>
            public void VFDMWriteSharedStorage()
            {
                this.Buildrawgsds();
                Prime.Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] : {this.GsdsName} : [{this.Gsdsstring}]");
                Prime.Services.SharedStorageService.InsertRowAtTable(this.GsdsName, this.Gsdsstring, Context.DUT);
            }

            /// <summary>Writes shared Storage of value.</summary>
            /// <param name = "virtualconfig" > Virtual fuse configuration.</param>
            public void VFDMReadSharedStorage(VirtFuseColOnlyJsonParser virtualconfig)
            {
                this.Gsdsstring = Prime.Services.SharedStorageService.GetStringRowFromTable(this.GsdsName, Context.DUT);
                this.Buildfromrawgsds(virtualconfig);
                Prime.Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] {this.GsdsName} : [{this.Gsdsstring}]");
            }

            /// <summary>Builds from raw GSDS the fuse values and whether it is valid.</summary>
            /// <param name = "virtualconfig" > Virtual fuse configuration.</param>
            public void Buildfromrawgsds(VirtFuseColOnlyJsonParser virtualconfig)
            {
                this.Fuses = new List<Fuse>();
                foreach (string fuse in virtualconfig.GrabVFDMblock(this.GsdsName).FusePositions)
                {
                    string rawvalue = this.Extractvalue(fuse, this.Gsdsstring);
                    bool available = false;
                    if (rawvalue.Contains('X'))
                    {
                        available = true;
                    }

                    this.Fuses.Add(new Fuse(rawvalue, available));
                }
            }

            /// <summary> Builds Raw gsds used when needing to write back to shared storage.</summary>
            public void Buildrawgsds()
            {
                this.Gsdsstring = string.Empty;
                foreach (Fuse fuse in this.Fuses)
                {
                    this.Gsdsstring += fuse.FuseValue;
                }
            }

            /// <summary> Virtual fuse class for each fuse To maintain state.</summary>
            public class Fuse
            {
                /// <summary> Initializes a new instance of the <see cref="Fuse"/> class. </summary>
                /// <param name="rawvalue">RawValue for this fuse.</param>
                /// <param name="available">Whether resources are avai.</param>
                public Fuse(string rawvalue, bool available)
                {
                    this.FuseValue = rawvalue;
                    this.Available = available;
                }

                /// <summary> Gets or sets a value indicating whether fuse is available.</summary>
                public virtual bool Available { get; set; } = true;

                /// <summary> Gets or sets the value in virtualfuse.</summary>
                public virtual string FuseValue { get; set; }
            }
        }
    }
}
