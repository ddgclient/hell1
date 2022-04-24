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
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Prime;
    using Prime.SharedStorageService;

    /// <summary>Class containing all required voltage functions. </summary>
    public class Vmin
    {
        private List<double> permemVoltageLevels;

        private List<string> vminlookuptable;

        private bool pervminenabled = false;

        private bool[] voltagepass;

        /// <summary> Initializes a new instance of the <see cref="Vmin"/> class.</summary>
        /// <param name = "hryref" > Length for reference.</param>
        /// <param name = "vminlookup" > List of VMINs per HRY location.</param>
        /// <param name = "vmintargets" > Voltage rails being searched.</param>
        public Vmin(List<string> hryref, List<string> vminlookup, List<string> vmintargets)
        {
            this.voltagepass = new bool[vmintargets.Count];
            this.vminlookuptable = vminlookup;
            if (hryref.Count == vminlookup.Count)
            {
                var foundcount = 0;
                var idx = 0;
                foreach (var vminname in vmintargets)
                {
                    this.voltagepass[idx] = true;
                    if (vminlookup.Contains(vminname))
                    {
                        foundcount += 1;
                    }

                    if (foundcount == vmintargets.Count)
                    {
                        this.pervminenabled = true;
                        this.ClearVoltageString(hryref);
                        break;
                    }

                    idx += 1;
                }
            }
            else
            {
                this.pervminenabled = false;
/*                var idx = 0;
                foreach (var vminname in vmintargets)
                {
                    this.voltagepass[idx] = false;
                    idx += 1;
                }
*/
            }
        }

        /// <summary>Gets or sets name of GSDS written to. </summary>
        public string PerMemVoltageNameITUFF { get; set; } = "MbistVminPerMem";

        /// <summary>Gets or sets name of GSDS written to. </summary>
        public string PerMemVoltageNameSS { get; set; } = "MbistVminPerMem";

        /// <summary> Grab if pervmin is enabled.</summary>
        /// <returns>Whether pervmin is enabled. </returns>
        public bool Pervminenabled()
        {
            return this.pervminenabled;
        }

        /// <summary> Return Voltage State.</summary>
        /// <returns> returns array of bools of passing voltages. </returns>
        public bool[] VoltagePassRead()
        {
            return this.voltagepass;
        }

        /// <summary> Writes fail to voltage tracking.</summary>
        /// <param name = "index" > Index of voltage to return.</param>
        public void VoltageWriteFail(int index)
        {
            this.voltagepass[index] = false;
        }

        /// <summary> Set run state back to pass.</summary>
        public void VoltageStateReset()
        {
            this.voltagepass = Enumerable.Repeat(true, this.voltagepass.Length).ToArray();
        }

        /// <summary> Grab value from per mem vmin string.</summary>
        /// <param name = "index" > Index of voltage to return.</param>
        /// <returns>Voltage found at that index. </returns>
        public double Grabvmin(int index)
        {
            return this.permemVoltageLevels[index];
        }

        /// <summary> Set value per mem vmin string.</summary>
        /// <param name = "index" > Index of voltage to return.</param>
        /// <param name = "value" > Set Index of voltage to return.</param>
        public void Setvmin(int index, double value)
        {
            this.permemVoltageLevels[index] = value;
        }

        /// <summary> Return Voltages Per Domain.</summary>
        public void PrintDataToItuffPerDomain()
        {
            Dictionary<string, double> voltagePerDomain = new Dictionary<string, double>();
            int i = 0;
            var founduntested = false;
            foreach (var voltage in this.vminlookuptable)
            {
                if (voltagePerDomain.ContainsKey(voltage))
                {
                    if (voltagePerDomain[voltage] == -9999 || this.permemVoltageLevels[i] == -9999)
                    {
                        voltagePerDomain[voltage] = -9999; // Do Nothing
                    }
                    else if (this.permemVoltageLevels[i] == -5555)
                    {
                        voltagePerDomain[voltage] = -5555;
                        founduntested = true;
                    }
                    else if (voltagePerDomain[voltage] == -5555 && founduntested == true)
                    {
                        // Do nothing
                    }
                    else if (this.permemVoltageLevels[i] > voltagePerDomain[voltage] && voltagePerDomain[voltage] != -9999)
                    {
                        voltagePerDomain[voltage] = this.permemVoltageLevels[i];
                    }
                }
                else if (this.permemVoltageLevels[i] == -5555)
                {
                    voltagePerDomain.Add(voltage, this.permemVoltageLevels[i]);
                    founduntested = true;
                }
                else
                {
                    voltagePerDomain.Add(voltage, this.permemVoltageLevels[i]);
                }

                i++;
            }

            var tempstring = string.Empty;
            foreach (KeyValuePair<string, double> voltage in voltagePerDomain)
            {
                tempstring += $"{voltage.Key}:{voltage.Value},";
            }

            tempstring = tempstring.Substring(0, tempstring.Length - 1);

            /* Prime.Services.DatalogService.WriteToItuff($"2_tname_{this.PerMemVoltageNameITUFF}\n2_strgval_{string.Join(",", tempstring)}\n"); */
            MbistVminTC.WriteStrgvalToItuff(this.PerMemVoltageNameITUFF, string.Join(",", tempstring));
        }

        /// <summary> Prints specified data to ituff. </summary>
        public void PrintDataToItuffPerArrayVmin()
        {
        /* Prime.Services.DatalogService.WriteToItuff($"2_tname_{this.PerMemVoltageNameITUFF}\n2_strgval_{string.Join(",", this.permemVoltageLevels)}\n"); */
        MbistVminTC.WriteStrgvalToItuff(this.PerMemVoltageNameITUFF, string.Join(",", this.permemVoltageLevels));
        }

        /// <summary> Clears the Voltage list to all untested.</summary>
        /// <param name = "hryref" > List of voltages.</param>
        public void ClearVoltageString(List<string> hryref)
        {
            if (hryref.Count == this.vminlookuptable.Count)
            {
                List<double> voltages = new List<double>();
                foreach (var hryloc in hryref)
                {
                    voltages.Add(-5555);
                }

                this.permemVoltageLevels = voltages;
                Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] PermemVoltageLevels: [{string.Join(",", this.permemVoltageLevels)}]");
            }
        }

        /// <summary> Writes sharedstorage for this bisr.</summary>
        public void VminWriteSharedStorage()
        {
            var number = NumberFormatInfo.CurrentInfo;

            // number.NumberDecimalSeparator = ".";
            var voltagestring = string.Join(",", this.permemVoltageLevels.Select(_ => _.ToString(number)));
            Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Writing to shared storage token [{this.PerMemVoltageNameSS}].");
            Services.SharedStorageService.InsertRowAtTable(this.PerMemVoltageNameSS, voltagestring, Context.DUT);
            Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Value writen: [{voltagestring}].");
        }

        /// <summary> ReadShared Storagefor vmin.</summary>
        public void VminReadSharedStorage()
        {
            Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Retrieving from shared for key : [{this.PerMemVoltageNameSS}].");
            var voltagestring = (string)Prime.Services.SharedStorageService.GetStringRowFromTable(this.PerMemVoltageNameSS, Context.DUT);
            Services.ConsoleService.PrintDebug($"[{MethodBase.GetCurrentMethod().Name}] Retrieved Value : [{voltagestring}].");
            this.permemVoltageLevels = voltagestring.Split(',').Select(x => double.Parse(x)).ToList();
        }
    }
}
