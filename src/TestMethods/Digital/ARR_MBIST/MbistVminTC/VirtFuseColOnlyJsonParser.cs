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
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Recovery JSON Parser.
    /// </summary>
    public class VirtFuseColOnlyJsonParser
    {
        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("VERSION")]
        public double Version { get; set; }

        /// <summary>Gets or sets JSON Element.</summary>
        [JsonProperty("VFDM_REPAIR")]
        public Dictionary<string, VFDMBLOCKS> GSDSNAMEs { get; set; }

        /// <summary> Builds Name list for all gsds tokens.</summary>
        /// <param name="hryreference"> hry reference. </param>
        public void Buildfuse(List<string> hryreference)
        {
            foreach (KeyValuePair<string, VFDMBLOCKS> vfdmblock in this.GSDSNAMEs)
            {
                vfdmblock.Value.Buildfuse(hryreference);
            }
        }

        /// <summary> Returns specific VFDM data.</summary>
        /// <param name="name"> Name of GSDS to return. </param>
        /// <returns>Returns a specific VFDMbock value.</returns>
        public VFDMBLOCKS GrabVFDMblock(string name)
        {
            return this.GSDSNAMEs[name];
        }

        /// <summary>Gets or sets JSON Element.</summary>
        public class VFDMBLOCKS
        {
            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("FUSE_POS")]
            public List<string> FusePositions { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty("CONTROLLERS")]
            public Dictionary<string, Controllers> Controller { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty(Required = Required.DisallowNull)]
            public List<List<string>> FullFusemap { get; set; }

            /// <summary>Gets or sets JSON Element.</summary>
            [JsonProperty(Required = Required.DisallowNull)]
            public List<List<int>> FullFusemapIdx { get; set; }

            /// <summary> Builds Name list for given fuse position.</summary>
            /// <param name="hryreference"> hry reference. </param>
            public void Buildfuse(List<string> hryreference)
            {
                this.FullFusemap = new List<List<string>>();
                this.FullFusemapIdx = new List<List<int>>();

                foreach (var init in this.FusePositions)
                {
                    this.FullFusemap.Add(new List<string>());
                    this.FullFusemapIdx.Add(new List<int>());
                }

                foreach (KeyValuePair<string, Controllers> cont in this.Controller)
                {
                    foreach (KeyValuePair<string, string> fuse in cont.Value.Fusememmap)
                    {
                        var listofmems = fuse.Value.Split(',');
                        foreach (var memgrp in listofmems)
                        {
                            if (!memgrp.Contains('-'))
                            {
                                this.FullFusemap[int.Parse(fuse.Key)].Add(cont.Key + "_MEM" + memgrp);
                                this.FullFusemapIdx[int.Parse(fuse.Key)].Add(hryreference.IndexOf(cont.Key.ToUpper() + "_MEM" + memgrp));
                            }
                            else
                            {
                                var memsf = memgrp.Split('-').Select(int.Parse).ToList();
                                if (memsf[0] < memsf[1])
                                {
                                    for (var i = memsf[0]; i <= memsf[1]; i++)
                                    {
                                        this.FullFusemap[int.Parse(fuse.Key)].Add($"{cont.Key}_MEM{i}");
                                        this.FullFusemapIdx[int.Parse(fuse.Key)].Add(hryreference.IndexOf(cont.Key.ToUpper() + "_MEM" + i));
                                    }
                                }
                                else
                                {
                                    for (var i = memsf[1]; i <= memsf[0]; i++)
                                    {
                                        this.FullFusemap[int.Parse(fuse.Key)].Add($"{cont.Key}_MEM{i}");
                                        this.FullFusemapIdx[int.Parse(fuse.Key)].Add(hryreference.IndexOf(cont.Key.ToUpper() + "_MEM" + i));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>Gets or sets JSON Element.</summary>
            public class Controllers
            {
                /// <summary>Gets or sets JSON Element.</summary>
                [JsonProperty("FUSEMEMMAP")]
                public Dictionary<string, string> Fusememmap { get; set; }
            }
        }
    }
}
