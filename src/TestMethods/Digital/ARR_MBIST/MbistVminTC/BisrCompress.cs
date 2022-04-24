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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Prime.ConsoleService;

    /// <summary>
    /// Compression BISR Class.
    /// </summary>
    // [ExcludeFromCodeCoverage]
    public class BisrCompress
    {
        /// <summary> Type of print to do. </summary>
        public enum PrintType
        {
            /// <summary> Prints to Prime for prime solution.</summary>
            Prime,

            /// <summary> Prints to Console. </summary>
            Console,
        }

        /// <summary> Gets or sets Return Data type.</summary>
        public PrintType PrintMode { get; set; } = PrintType.Prime;

        /// <summary> Gets or sets Buffer Size for data on BISR Chain.</summary>
        public int BufferSize { get; set; }

        /// <summary> Gets or sets Zero Size in BISR Chain.</summary>
        public int ZeroSize { get; set; }

        /// <summary> Gets or sets Fusebox Size.</summary>
        public int FuseboxSize { get; set; }

        /// <summary> Gets or sets Total size of BISR Chain.</summary>
        public int Totallength { get; set; }

        /// <summary> Gets or sets Max Sessions.</summary>
        public int MaxSessions { get; set; }

        /// <summary> Gets or sets Zero Size in BISR Chain.</summary>
        public int FuseboxAddress { get; set; }

        /// <summary> Gets or sets the chains in this bisr chain.</summary>
        public List<int> Chains { get; set; }

        /// <summary> Gets or sets a value indicating whether indicating autonomous mode fusing(if not used it will optimize PDs).</summary>
        public bool AutonomousModeBurn { get; set; } = true;

        /// <summary> Gets or sets the chains in this bisr chain.</summary>
        public List<int> OrigSessionPointers { get; set; } = new List<int>();

        /// <summary> Gets or sets PD Pointers found in current fuse.</summary>
        public List<List<int>> OrigPDPointers { get; set; } = new List<List<int>>();

        /// <summary> Gets or sets List of data packets already burnt in fuses.</summary>
        public List<List<string>> OrigPowerDomainData { get; set; } = new List<List<string>>();

        /// <summary> Gets or sets List of Sessions used.</summary>
        public string OrigSessionEnable { get; set; } = "000";

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        public IConsoleService Console { get; set; } = null;

        /// <summary> Gets or sets Current Session # to fuse.</summary>
        private int CurrentSession { get; set; } = 0;

        /// <summary> Gets or sets Current Session Pointer value to start fuse.</summary>
        private int CurrentSessionPoint { get; set; }

        /*/// <summary>Main Function.</summary>
        public static void Main()
        {
            var compresscapture = string.Concat(Enumerable.Repeat("0", 41));
            var temp = string.Concat(Enumerable.Repeat("0", 233)) + "101" + string.Concat(Enumerable.Repeat("0", 65));
            compresscapture += temp;
            temp = string.Concat(Enumerable.Repeat("0", 61));
            compresscapture += temp;
            var bisr1 = new BisrCompress
            {
                BufferSize = 7,
                ZeroSize = 17,
                FuseboxSize = 256,
                Totallength = 403,
                FuseboxAddress = 10,
                MaxSessions = 3,
                Chains = new List<int> { 41, 301, 61 },
                AutonomousModeBurn = false,
            };

            // var oldfuseval = String.Empty;
            var oldfuseval = "00100000000000000000000000011110100011010000000000000001010010000000000111010011010000000000000000111101000000000000111101";
            var compressed = bisr1.CompressChains(compresscapture, oldfuseval);

            if (compressed == true)
            {
                Console.Write("Compress fit\n");
            }
            else
            {
                Console.Write("Compress doesn't fit\n");
            }
        }*/

        /// <summary>Compression Algorithm.</summary>
        /// <param name = "capture" > String of what is captured by tester.</param>
        /// <param name = "origfuse" > String of what was burned in last time.</param>
        /// <returns>Boolean of whether compression fits in fuses.</returns>
        public dynamic CompressChains(string capture, string origfuse)
        {
            var bisrposition = 0;
            var pdstartfinish = new List<Tuple<int, int>>();
            var pdchains = new List<string>();
            var sessionpointer = string.Empty;
            var pdpoint = new List<string>();

            var notfusable = this.BreakdownOldFuse(origfuse);
            if (notfusable != true)
            {
                this.PrintFunction($"\n[{MethodBase.GetCurrentMethod().Name}] ----------------------- BISR CHAIN BREAKDOWN -----------------------", this.PrintMode);
                foreach (int pdlength in this.Chains)
                {
                    pdstartfinish.Add(new Tuple<int, int>(bisrposition, pdlength));
                    bisrposition += pdlength;
                }

                var count = 0;
                foreach (var chaintuple in pdstartfinish)
                {
                    var pdchain = capture.Substring(chaintuple.Item1, chaintuple.Item2);
                    this.PrintFunction($"\n[{MethodBase.GetCurrentMethod().Name}] Captured BISR String for PD_" + count + ": " + pdchain.ToString(), this.PrintMode);
                    var subchaintotal = this.CompressPD(pdchain, count);
                    pdchains.Add(subchaintotal);
                    count++;
                }

                string[] treusedPDs = Enumerable.Repeat(string.Empty, this.Chains.Count).ToArray();
                List<string> reusedPDs = treusedPDs.ToList();
                if ((this.AutonomousModeBurn == false) && this.CurrentSession != 0)
                {
                    reusedPDs = this.ReusePDs(ref pdchains, ref reusedPDs);
                }

                if (pdchains.Count > 1)
                {
                    pdpoint = this.CreatePDPointer(pdchains, reusedPDs);
                }

                if (this.MaxSessions > 1)
                {
                    sessionpointer = this.CreateSessionPointer(pdchains);
                }

                var fuse = string.Empty;
                var fusetotal = origfuse;
                var additionalfuse = string.Empty;
                if (this.CurrentSession != 0)
                {
                    fusetotal = this.UpdateSespd(0, sessionpointer, fusetotal);
                }
                else
                {
                    fusetotal += sessionpointer;
                }

                fuse += sessionpointer;
                if (this.CurrentSessionPoint != 0)
                {
                    fuse += new string('0', this.CurrentSessionPoint - fuse.Length);
                }

                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] ----------------------- BISR CHAIN BREAKDOWN -----------------------\n", this.PrintMode);

                foreach (var point in pdpoint)
                {
                    fusetotal += point;
                    fuse += point;
                    additionalfuse += point;
                }

                var j = 0;
                foreach (var data in pdchains)
                {
                    this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] PD" + j.ToString() + " DATA:  " + data + " : " + data.Length, this.PrintMode);
                    j++;
                    fuse += data;
                    fusetotal += data;
                    additionalfuse += data;
                }

                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] ----------------------- SUMMARY -----------------------", this.PrintMode);
                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] MaxFuseboxSize: " + this.FuseboxSize.ToString(), this.PrintMode);
                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] Prior Fuse: " + origfuse.Length, this.PrintMode);
                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] Total Fuse after Burn:  " + fusetotal.Length, this.PrintMode);

                if (this.CurrentSession != 0)
                {
                    this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] Additional Fuses (Exclude: Session/Enables): " + additionalfuse.Length, this.PrintMode);
                    this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] Additional Fuse (includes Data/PD Pointer): " + additionalfuse, this.PrintMode);
                }

                this.PrintFunction($"\n[{MethodBase.GetCurrentMethod().Name}] ----------------------- Expected Fuse After Burn -----------------------", this.PrintMode);
                this.PrintFunction(fusetotal, this.PrintMode);
                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] ------------------------------------------------------------------------", this.PrintMode);

                this.PrintFunction($"\n[{MethodBase.GetCurrentMethod().Name}] ----------------------- Fuse Value to Apply ---------------------------", this.PrintMode);
                this.PrintFunction(fuse, this.PrintMode);
                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] -----------------------------------------------------------------------", this.PrintMode);
                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] ----------------------- SUMMARY -----------------------\n", this.PrintMode);

                var fusefit = false;
                if (this.FuseboxSize >= fusetotal.Length)
                {
                    fusefit = true;
                }

                var returndict = new Dictionary<string, dynamic>()
                {
                    { "Compress", fusefit },
                    { "FuseToApply", fuse },
                    { "FuseAfterBurn", fusetotal },
                    { "AvailableFuse", true },
                };

                return returndict;
            }
            else
            {
                var returndict = new Dictionary<string, dynamic>()
                {
                    { "Compress", false },
                    { "FuseToApply", string.Empty },
                    { "FuseAfterBurn", origfuse },
                    { "AvailableFuse", false },
                };
                return returndict;
            }
        }

        /// <summary>Prints to either Prime or to console depending on value.</summary>
        /// <param name = "value" > String to print to console or Prime consoles.</param>
        /// <param name = "printto" > Chooses Console or prime print.</param>
        public void PrintFunction(string value, PrintType printto)
        {
            if (printto == PrintType.Console)
            {
                System.Console.WriteLine(value);
            }
            else
            {
                this.Console?.PrintDebug(value);
            }
        }

        /// <summary>Checks if Pointers to Data are reused.</summary>
        /// <param name = "pdchains" > Passed by reference the data chains to be removed.</param>
        /// <param name = "reusedPDs" > Reused pds updated if reused version found.</param>
        /// <returns>Returns a list of reused PDs.</returns>
        public List<string> ReusePDs(ref List<string> pdchains, ref List<string> reusedPDs)
        {
            for (var ch = pdchains.Count - 1; ch >= 1; ch--)
            {
                for (var ses = 0; ses < this.CurrentSession; ses++)
                {
                    if (pdchains[ch] == this.OrigPowerDomainData[ch][ses])
                    {
                        pdchains[ch] = string.Empty;
                        reusedPDs[ch] = Convert.ToString(this.OrigPDPointers[ch][ses], 2).PadLeft(this.FuseboxAddress, '0');
                    }
                }
            }

            return reusedPDs;
        }

        /// <summary>Updates Session Pointer.</summary>
        /// <param name = "start" > Start of where to check for updates.</param>
        /// <param name = "newstring" > New Session Data.</param>
        /// <param name = "original" > Original Fuse value string.</param>
        /// <returns>String of compressed session pointers.</returns>
        public string UpdateSespd(int start, string newstring, string original)
        {
            var newchararray = newstring.ToCharArray();
            var orignalarray = original.ToCharArray();

            for (var i = start; i < start + newstring.Length; i++)
            {
                if (newchararray[i].Equals('1'))
                {
                    orignalarray[i] = '1';
                }
            }

            original = new string(orignalarray);
            return original;
        }

        /// <summary>Breaks down old fuse chain.</summary>
        /// <param name = "oldfuse" > List of PD Chain lengths.</param>
        /// <returns>String of compressed session pointers.</returns>
        public bool BreakdownOldFuse(string oldfuse)
        {
            this.PrintFunction($"\n[{MethodBase.GetCurrentMethod().Name}] ----------------------- Start: Breakdown original Fuse -----------------------", this.PrintMode);
            if (oldfuse != string.Empty)
            {
                this.OrigPDPointers.Clear();
                this.OrigPowerDomainData.Clear();
                this.OrigSessionPointers.Clear();
                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] Old Fuse value: " + oldfuse, this.PrintMode);
                this.OrigSessionEnable = oldfuse.Substring(0, this.MaxSessions);
                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] Previous Session Enable: " + this.OrigSessionEnable, this.PrintMode);
                var reversedOrigSessionEnable = string.Join(string.Empty, this.OrigSessionEnable.Reverse());

                // Console.WriteLine("Previous Session Enable(Reversed): " + reversedOrigSessionEnable);
                this.CurrentSession = this.OrigSessionEnable.Count(f => f == '1');
                if (this.OrigSessionEnable[0] == '1')
                {
                    this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] ----------------------- Finish: No Burn Sessions Available -----------------------\n", this.PrintMode);
                    return true;
                }
                else if (this.OrigSessionEnable != string.Concat(Enumerable.Repeat("0", this.MaxSessions)))
                {
                    for (var i = 0; i < this.CurrentSession; i++)
                    {
                        var start = 0;
                        var data = this.MaxSessions + ((this.MaxSessions - 1) * this.FuseboxAddress);
                        var capture = string.Empty;

                        if (i == 0)
                        {
                            this.OrigSessionPointers.Add(data);
                            this.CurrentSessionPoint = data;
                        }
                        else
                        {
                            start = this.MaxSessions + ((i - 1) * this.FuseboxAddress);
                            capture = oldfuse.Substring(start, this.FuseboxAddress);
                            data = Convert.ToInt32(capture, 2);
                            this.OrigSessionPointers.Add(data);
                        }

                        this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] Previous Session: " + i + ",  Data Captured: " + capture + ",  Value: " + data, this.PrintMode);
                    }

                    this.CurrentSessionPoint = oldfuse.Length;
                    var j = 0;
                    foreach (var enable in reversedOrigSessionEnable)
                    {
                        if (enable == '1')
                        {
                            var start = 0;
                            var data = this.OrigSessionPointers[j] + (this.FuseboxAddress * (this.Chains.Count - 1));
                            var capture = string.Empty;

                            for (var k = 0; k < this.Chains.Count; k++)
                            {
                                if (k == 0)
                                {
                                    if (j == 0)
                                    {
                                        this.OrigPDPointers.Add(new List<int> { data });
                                    }
                                    else
                                    {
                                        this.OrigPDPointers[k].Add(data);
                                    }
                                }
                                else
                                {
                                    start = this.OrigSessionPointers[j] + ((k - 1) * this.FuseboxAddress);
                                    capture = oldfuse.Substring(start, this.FuseboxAddress);
                                    data = Convert.ToInt32(capture, 2);

                                    if (j == 0)
                                    {
                                        this.OrigPDPointers.Add(new List<int> { data });
                                    }
                                    else
                                    {
                                        this.OrigPDPointers[k].Add(data);
                                    }
                                }

                                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] Session: " + j + ",  Previous PD_" + k + ": Value: " + data + ", Data Captured(if exists): " + capture, this.PrintMode);
                            }

                            var l = 0;
                            foreach (var temp in this.OrigPDPointers)
                            {
                                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] Session: " + j + " Pointer PD" + l, this.PrintMode);
                                foreach (var temp2 in temp)
                                {
                                    this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] \t" + temp2.ToString(), this.PrintMode);
                                }

                                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] \n", this.PrintMode);
                                l++;
                            }
                        }

                        j++;
                    }

                    for (var s = 0; s < this.OrigSessionPointers.Count; s++)
                    {
                        if (this.OrigSessionPointers[s] != 0)
                        {
                            for (var t = 0; t < this.OrigPDPointers.Count - 1; t++)
                            {
                                var start = this.OrigPDPointers[t][s];
                                var capture = oldfuse.Substring(start, this.OrigPDPointers[t + 1][s] - start);

                                if (s == 0)
                                {
                                    this.OrigPowerDomainData.Add(new List<string> { capture });
                                }
                                else
                                {
                                    this.OrigPowerDomainData[t].Add(capture);
                                }

                                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] Session: " + s + ",  PD_" + t + ":  Data Captured: " + capture, this.PrintMode);
                            }

                            var lengcap = oldfuse.Length - this.OrigPDPointers[this.OrigPDPointers.Count - 1][s];
                            if (s + 1 < this.OrigSessionPointers.Count)
                            {
                                if (this.OrigSessionPointers[s + 1] != 0)
                                {
                                    lengcap = this.OrigSessionPointers[s + 1] - this.OrigPDPointers[this.OrigPDPointers.Count - 1][s];
                                }
                            }

                            var lastcapture = oldfuse.Substring(this.OrigPDPointers[this.OrigPDPointers.Count - 1][s], lengcap);
                            if (s == 0)
                            {
                                this.OrigPowerDomainData.Add(new List<string> { lastcapture });
                            }
                            else
                            {
                                this.OrigPowerDomainData[this.OrigPDPointers.Count - 1].Add(lastcapture);
                            }

                            this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] Session: " + s + ",  PD_" + (this.OrigPDPointers.Count - 1).ToString() + ":  Data Captured: " + lastcapture, this.PrintMode);
                        }
                    }
                }
            }
            else
            {
                var total = this.MaxSessions + ((this.MaxSessions - 1) * this.FuseboxAddress);
                this.OrigSessionPointers.Add(total);
                this.OrigPDPointers.Add(new List<int> { ((this.MaxSessions - 1) * this.FuseboxAddress * 2) + this.MaxSessions });
            }

            this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] ----------------------- Finish: Breakdown original Fuse -----------------------\n", this.PrintMode);
            return false;
        }

        /// <summary>Create Pointers to Data Per PD.</summary>
        /// <param name = "chains" > List of PD Chain lengths.</param>
        /// <param name = "reusedpds" > Reused PDs if found.</param>
        /// <returns>List<String></String> of compressed session pointers.</returns>
        public List<string> CreatePDPointer(List<string> chains, List<string> reusedpds)
        {
            var pdinfo = new List<string>();
            var padding = 0;

            if (this.CurrentSessionPoint == 0 && this.MaxSessions > 1)
            {
                padding += ((this.MaxSessions - 1) * this.FuseboxAddress * 2) + this.MaxSessions;
            }
            else if (this.CurrentSessionPoint == 0 && this.MaxSessions == 1)
            {
                padding += (this.MaxSessions - 1) * this.FuseboxAddress * 2;
            }
            else
            {
                padding = this.CurrentSessionPoint;
                padding += (this.MaxSessions - 1) * this.FuseboxAddress;
            }

            this.PrintFunction("\n", this.PrintMode);
            for (var i = 0; i < chains.Count; i++)
            {
                if (i == 0)
                {
                    padding += chains[i].Length;
                }
                else if (reusedpds[i] == string.Empty)
                {
                    var value = Convert.ToString(padding, 2).PadLeft(this.FuseboxAddress, '0');
                    this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] PD Chain: " + i.ToString() + " Pointer:  " + value, this.PrintMode);
                    pdinfo.Add(value);
                    padding += chains[i].Length;
                }
                else
                {
                    pdinfo.Add(reusedpds[i]);
                    this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] PD Chain: " + i.ToString() + " Reused Pointer:  " + reusedpds[i], this.PrintMode);
                }
            }

            return pdinfo;
        }

        /// <summary>Create Session Pointers from current BISR.</summary>
        /// <param name = "chains" > List of PD Chain lengths.</param>
        /// <returns>String of compressed session pointers data.</returns>
        public string CreateSessionPointer(List<string> chains)
        {
            var sessiondata = string.Empty;
            var sessionselect = string.Empty;

            // Session Enables
            for (var i = this.MaxSessions - 1; i >= 0; i--)
            {
                if (this.CurrentSession == i)
                {
                    sessionselect += "1";
                }
                else
                {
                    sessionselect += "0";
                }
            }

            this.PrintFunction($"\n[{MethodBase.GetCurrentMethod().Name}] Session Burn: " + sessionselect, this.PrintMode);
            sessiondata += sessionselect;

            if (this.MaxSessions > 1)
            {
                var sessionaddpad = string.Empty;

                // Session Address
                for (var j = 1; j < this.MaxSessions; j++)
                {
                    if (j == this.CurrentSession)
                    {
                        sessionaddpad = Convert.ToString(this.CurrentSessionPoint, 2).PadLeft(this.FuseboxAddress, '0');
                    }
                    else
                    {
                        sessionaddpad = string.Concat(Enumerable.Repeat("0", this.FuseboxAddress));
                    }

                    sessiondata += sessionaddpad;
                    this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] Session " + j + " Address: " + sessionaddpad + " : " + sessionaddpad.Length.ToString(), this.PrintMode);
                }

                this.PrintFunction($"[{MethodBase.GetCurrentMethod().Name}] Combined Session Data: " + sessiondata + " : " + sessiondata.Length.ToString() + "\n", this.PrintMode);
            }

            return sessiondata;
        }

        /// <summary>Compression of Data section.</summary>
        /// <param name = "capture" > String of what is captured by tester.</param>
        /// <param name = "pd" > Pointer Data # running.</param>
        /// <returns>String of current PD data.</returns>
        public string CompressPD(string capture, int pd)
        {
            var zerocount = 0;
            var datacap = 0;
            var compressedstr = new List<Tuple<string, string>>();
            var datasection = string.Empty;

            foreach (var character in capture)
            {
                if (datacap != 0)
                {
                    datacap -= 1;
                    datasection += character;
                }
                else
                {
                    if (datasection.Length == this.BufferSize)
                    {
                        compressedstr.Add(new Tuple<string, string>($"\t[{MethodBase.GetCurrentMethod().Name}] PD_" + pd + ", Data: ", datasection));
                        datasection = string.Empty;
                    }

                    if (character == '1')
                    {
                        datacap = this.BufferSize - 1;
                        if (zerocount > 0)
                        {
                            compressedstr.Add(new Tuple<string, string>($"\t[{MethodBase.GetCurrentMethod().Name}] PD_" + pd + ", Zero: ", this.Zerocountconv(zerocount)));
                        }

                        datasection += character;
                        zerocount = 0;
                    }
                    else
                    {
                        zerocount += 1;

                        if (zerocount == Math.Pow(2, this.ZeroSize))
                        {
                            compressedstr.Add(new Tuple<string, string>($"\t[{MethodBase.GetCurrentMethod().Name}] PD_" + pd + ", Zero: ", this.Zerocountconv(zerocount)));
                            zerocount = 0;
                        }
                    }
                }
            }

            if (zerocount > 0)
            {
                compressedstr.Add(new Tuple<string, string>($"\t[{MethodBase.GetCurrentMethod().Name}] PD_" + pd + ", Zero: ", this.Zerocountconv(zerocount)));
            }

            // TODO: VERIFY HOW THIS WORKS WITH 1 at end.
            else if (datasection.Length > 0)
            {
                var addZero = this.BufferSize - datasection.Length;
                if (addZero > 0)
                {
                    compressedstr.Add(new Tuple<string, string>($"\t[{MethodBase.GetCurrentMethod().Name}] PD_" + pd + ", Data: ", datasection + string.Concat(Enumerable.Repeat("0", addZero))));
                }
                else
                {
                    compressedstr.Add(new Tuple<string, string>($"\t[{MethodBase.GetCurrentMethod().Name}] PD_" + pd + ", Data:", datasection));
                }
            }

            foreach (var value in compressedstr)
            {
                this.PrintFunction(value.Item1 + ", " + value.Item2, this.PrintMode);
            }

            var comppd = string.Empty;
            foreach (var compsect in compressedstr)
            {
                if (compsect.Item1 == "zero")
                {
                    comppd += "0" + compsect.Item2;
                }
                else
                {
                    comppd = comppd + compsect.Item2;
                }
            }

            return comppd;
        }

        /// <summary>Returns the zero count section padded to correct size.</summary>
        /// <param name = "zerocount" > int count of zeros found.</param>
        /// <returns>String for Zero compress value.</returns>
        public string Zerocountconv(int zerocount)
        {
            return '0' + Convert.ToString(zerocount, 2).PadLeft(this.ZeroSize, '0');
        }
    }
}
