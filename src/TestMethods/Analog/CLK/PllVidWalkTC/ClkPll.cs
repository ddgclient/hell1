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

namespace PllVidWalkTC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Prime.TpSettingsService;

    /// <summary>Defines the <see cref="ClkPll" />.</summary>
    public class ClkPll
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClkPll"/> class.
        /// </summary>
        /// <param name="fields">List of values from a Clk Input file.</param>
        /// <param name="fuseCfgTest">Name of the Evg UF test to run for fuse config (Temporary until Prime handles this natively).</param>
        /// <param name="module">Optional argument to set the default module name for UserVars.</param>
        /// <param name="defaultVoltage">Optional argument to set the default voltage if FAST wasn't executed.</param>
        public ClkPll(List<string> fields, string fuseCfgTest, string module = "", double defaultVoltage = -1)
        {
            // first update constants/globals
            if (module != string.Empty)
            {
                // strip out any IP scoping, we just want the module name for the user var collection.
                var l = module.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                this.UserVarExecuteAllPointsCollection = $"{l[l.Length - 1]}::{l[l.Length - 1]}";
            }

            if (defaultVoltage > 0)
            {
                this.DefaultVoltage = defaultVoltage;
            }

            this.FuseCfgTest = fuseCfgTest;

            // intialize the PLL struct from the fields.
            if (fields.Count < 13)
            {
                throw new ArgumentException($"ClkPll Constructor expecting at least 13 elements, got {fields.Count}.", "fields");
            }

            this.Name = fields[0];
            this.FormattedRatios = new List<string>(fields[1].Split('|'));
            this.Ratios = new List<int>();
            int tempInt;
            foreach (var ratio in this.FormattedRatios)
            {
                if (int.TryParse(ratio, out tempInt))
                {
                    this.Ratios.Add(tempInt);
                }
                else
                {
                    throw new ArgumentException($"ClkPll Constructor failed to convert ratio [{ratio}] to an integer.", "fields[1]");
                }
            }

            this.VidCorner = fields[2];
            this.TestName = new List<string>(fields[3].Split('?'));
            if (this.TestName.Count != 3)
            {
                throw new ArgumentException($"ClkPll Constructor expecting TestName to have 3 parts separated by '?', found [{this.TestName.Count}] for '{fields[3]}'.", "fields[3]");
            }

            if (int.TryParse(fields[4], out tempInt))
            {
                this.BclkRef = tempInt;
            }
            else
            {
                throw new ArgumentException($"ClkPll Constructor failed to convert BclkReference [{fields[4]}] to an integer.", "fields[4]");
            }

            double tempDouble;
            if (double.TryParse(fields[5], out tempDouble))
            {
                this.VoltageMax = tempDouble;
            }
            else
            {
                throw new ArgumentException($"ClkPll Constructor failed to convert MaxVoltage [{fields[5]}] to an double.", "fields[5]");
            }

            if (double.TryParse(fields[6], out tempDouble))
            {
                this.VoltageMin = tempDouble;
            }
            else
            {
                throw new ArgumentException($"ClkPll Constructor failed to convert MinVoltage [{fields[6]}] to an double.", "fields[6]");
            }

            if (double.TryParse(fields[7], out tempDouble))
            {
                this.FrequencyMax = tempDouble;
            }
            else
            {
                throw new ArgumentException($"ClkPll Constructor failed to convert MaxFrequency [{fields[7]}] to an double.", "fields[7]");
            }

            if (double.TryParse(fields[8], out tempDouble))
            {
                this.FrequencyMin = tempDouble;
            }
            else
            {
                throw new ArgumentException($"ClkPll Constructor failed to convert MinFrequency [{fields[8]}] to an double.", "fields[8]");
            }

            this.SubPLLs = new List<string>(fields[9].Split('|'));
            this.GSDSToken = fields[10];
            this.IPGBVar = fields[11];
            this.AppType = fields[12].ToUpper();
            if (this.AppType == "FIVR")
            {
                /* There are 2 possible values for Application Type: "FIVR" or "DIRECT_HW".
                 * Fivr has additional requirements.
                */
                if (fields.Count < 15)
                {
                    throw new ArgumentException($"ClkPll Constructor ApplicationType=FIVR, needs 15 elements got {fields.Count}.", "fields");
                }

                this.PList = fields[13]; /* string matching for patmodding vidwalk plists */
                this.SubDomains = new Dictionary<string, SubDomainStruct>();
                /* There might be multiple fuseconfig setpoints to write to... */
                foreach (var block in fields[14].Split('|'))
                {
                    var elems = block.Split('^'); /* Split the setpoint from the fuse(s) that are needed */
                    if (elems.Length != 3)
                    {
                        throw new ArgumentException($"ClkPll Constructor ApplicationType=FIVR, expecting subdomain fields to have 3 '^' separated elements, found {elems.Length} for '{block}'.", "fields[14]");
                    }

                    var key = elems[0]; /* The key matches the FuseConfig Setpoint defined by FIVR team to control VDAC  <fuse_register_name>VDAC</fuse_register_name> */
                    /* 1st element matches FIVR analog->digital conversion expression  <fivr_initial_voltage_expression>VINPUT[]*256</fivr_initial_voltage_expression>*/
                    /* 2nd element will be a list of all fuses to write to if any*/
                    if (!int.TryParse(elems[1], out tempInt))
                    {
                        throw new ArgumentException($"ClkPll Constructor failed to convert Conversion Expression [{elems[1]}] to an int in subdomain [{block}].", "fields[14]");
                    }

                    this.SubDomains[key] = new SubDomainStruct(key, tempInt, new List<string>(elems[2].Split('%')));
                }
            }
            else if (this.AppType != "DIRECT_HW")
            {
                throw new ArgumentException($"ClkPll Constructor ApplicationType should be FIVR or DIRECT_HW, not {this.AppType}.", "fields[12]");
            }

            // Initialize resultant lists/dicts used later on
            this.VMins = new List<double>();
            this.Results = new List<int>();
            this.UPSVF = string.Empty;
            this.LockTimes = new Dictionary<string, List<double>>();
            foreach (var subPll in this.SubPLLs)
            {
                this.LockTimes[subPll] = new List<double>();
            }
        }

        private string Name { get; set; }

        private List<string> FormattedRatios { get; set; }

        private List<int> Ratios { get; set; }

        private string VidCorner { get; set; }

        private List<string> TestName { get; set; }

        private int BclkRef { get; set; }

        private double VoltageMax { get; set; }

        private double VoltageMin { get; set; }

        private double FrequencyMax { get; set; }

        private double FrequencyMin { get; set; }

        private List<string> SubPLLs { get; set; }

        private string GSDSToken { get; set; }

        private string IPGBVar { get; set; }

        private string AppType { get; set; }

        private string PList { get; set; }

        private Dictionary<string, SubDomainStruct> SubDomains { get; set; }

        private List<double> VMins { get; set; }

        private List<int> Results { get; set; }

        private string UPSVF { get; set; }

        private Dictionary<string, List<double>> LockTimes { get; set; }

        private string FuseCfgTest { get; }

        // Constants -- FIXME -- move these to a separate class.
        private double DefaultFrequency { get; } = 1.0;

        private double DefaultVoltage { get; } = 1.0;

        private string VminGSDS { get; } = "VID_WALK_VMIN";

        private string UPSGSDS { get; } = "FAST_UPSVFPASSFLOW";

        private string FIVRRegex { get; } = "tgl_pre";

        private string UserVarExecuteAllPointsCollection { get; } = "CLK_ADPLL_ALL";

        private string UserVarExecuteAllPointsVariable { get; } = "VIDWK_EXECUTE_ALL_POINTS";

        private int FASTFrequencyModifier { get; } = 1000000000;

        private string GBVarsPHMSwitchCollection { get; } = "GBVars";

        private string GBVarsPHMSwitchVariable { get; } = "PHM_GB_switch";

        /// <summary>
        /// Updates the min/max voltages.
        /// </summary>
        /// <param name="minVoltage">Minimum Voltage per the Bin/Flow Matrix.</param>
        /// <param name="maxVoltage">Maximum Voltage per the Bin/Flow Matrix.</param>
        public void UpdateMinMaxVoltage(double minVoltage, double maxVoltage)
        {
            this.VoltageMin = minVoltage;
            this.VoltageMax = maxVoltage;
        }

        /// <summary>
        /// Print the contents of the PLL Object.
        /// </summary>
        public void PrintDebug()
        {
            Prime.Services.ConsoleService.PrintDebug($"PLLName = {this.Name}");
            Prime.Services.ConsoleService.PrintDebug($"Ratios = {string.Join(", ", this.Ratios)}");
            Prime.Services.ConsoleService.PrintDebug($"VIDCorner = {this.VidCorner}");
            Prime.Services.ConsoleService.PrintDebug($"Testname = {string.Join("/", this.TestName)}");
            Prime.Services.ConsoleService.PrintDebug($"BCLKRef = {this.BclkRef}");
            Prime.Services.ConsoleService.PrintDebug($"MaxVoltage = {this.VoltageMax}");
            Prime.Services.ConsoleService.PrintDebug($"MinVoltage = {this.VoltageMin}");
            Prime.Services.ConsoleService.PrintDebug($"MaxFrequency = {this.FrequencyMax}");
            Prime.Services.ConsoleService.PrintDebug($"MinFrequency = {this.FrequencyMin}");
            Prime.Services.ConsoleService.PrintDebug($"SubPLL = {string.Join(", ", this.SubPLLs)}");
            Prime.Services.ConsoleService.PrintDebug($"GSDS = {this.GSDSToken}");
            Prime.Services.ConsoleService.PrintDebug($"IP GBVar = {this.IPGBVar}");
            Prime.Services.ConsoleService.PrintDebug($"AppType = {this.AppType}");

            if (this.AppType == "FIVR")
            {
                Prime.Services.ConsoleService.PrintDebug($"PList = {this.PList}");
                foreach (var setpoint in this.SubDomains.Keys)
                {
                    Prime.Services.ConsoleService.PrintDebug($"Setpoint {setpoint} = {this.SubDomains[setpoint].Expression}:{string.Join(", ", this.SubDomains[setpoint].Fuses)}");
                }
            }

            Prime.Services.ConsoleService.PrintDebug("\n");
        }

        /// <summary>
        /// Resets all the results and calculated values.
        /// </summary>
        public void Clear()
        {
            this.VMins.Clear();
            this.Results.Clear();
            this.UPSVF = string.Empty;
            foreach (var subPll in this.LockTimes.Values)
            {
                subPll.Clear();
            }
        }

        /// <summary>
        /// Generates the Functional VMINs according to the VID values.
        /// </summary>
        /// <param name="instanceName">Testinstance name to use for Ituff Logging.</param>
        /// <returns>Tuple containing the UPS Min Ratio (item1) and UPS Max Ratio (item2).</returns>
        public Tuple<int, int> GenerateVmins(string instanceName)
        {
            string upsVF = string.Empty;
            try
            {
                // pull the FAST UPSVF GSDS
                // FAST Standard Format >> domain1:freq1^vmin1%freq2^vmin2_domain2:freq3^vmin3 ***Frequency in GHz
                upsVF = this.GetSharedString(this.UPSGSDS);
            }
            catch
            {
                Prime.Services.ConsoleService.PrintError($"Failed to read GSDS Token=[{this.UPSGSDS}], using default.");
                LogStrgvalWithSeparator($"_{this.Name}_UPSVF", "-9999");
                /* Prime.Services.DatalogService.WriteToItuff($"{ItuffHeader1()}{instanceName}_{this.Name}_UPSVF\n{ItuffHeader2()}-9999\n{ItuffHeader3()}"); */
                upsVF = $"{this.VidCorner}:0.4^{this.DefaultVoltage}%3.7^{this.DefaultVoltage}"; // set the point to the default value if FAST wasn't executed
            }

            Prime.Services.ConsoleService.PrintDebug($"FASTVF = {upsVF}\n");

            // Error checking to make sure that we have a valid domain in the UPSVF string.
            if (upsVF.IndexOf($"{this.VidCorner}:") < 0)
            {
                Prime.Services.ConsoleService.PrintError($"Failed to find domain=[{this.VidCorner}] in Token=[{this.UPSGSDS}], Value=[{upsVF}], using default.");
                LogStrgvalWithSeparator($"_{this.Name}_UPSVF", "-9999");
                /* Prime.Services.DatalogService.WriteToItuff($"{ItuffHeader1()}{instanceName}_{this.Name}_UPSVF\n{ItuffHeader2()}-9999\n{ItuffHeader3()}"); */
                upsVF = $"{this.VidCorner}:0.4^{this.DefaultVoltage}%3.7^{this.DefaultVoltage}"; // set the point to the default value if FAST wasn't executed
            }

            // we need to split the FAST string by domain
            foreach (var domainStr in upsVF.Trim().Split('_'))
            {
                // then split the domain from the data
                var domainVals = domainStr.Trim().Split(':');
                if (domainVals[0] == this.VidCorner)
                {
                    this.UPSVF = domainVals[1].Trim();
                }
            }

            // now separate out the freq/voltage pairs for this domain.
            List<Tuple<double, double>> freqVoltPairs = new List<Tuple<double, double>>();
            foreach (var freqVoltStr in this.UPSVF.Split('%'))
            {
                var tmp = freqVoltStr.Split('^');
                double freq = 0;
                double volt = -9999.0;
                if (!double.TryParse(tmp[0], out freq))
                {
                    throw new ArgumentException($"Could not convert Frequency Field=[{tmp[0]}] of GSDS=[{freqVoltStr}] into a double.", this.UPSGSDS);
                }

                foreach (var voltStr in tmp[1].Split('v'))
                {
                    var tmpVolt = -9999.0;
                    if (!double.TryParse(voltStr, out tmpVolt))
                    {
                        throw new ArgumentException($"Could not convert Voltage Field=[{voltStr}] of GSDS=[{freqVoltStr}] into a double.", this.UPSGSDS);
                    }

                    if (tmpVolt > volt && tmpVolt <= this.VoltageMax && tmpVolt >= this.VoltageMin)
                    {
                        volt = tmpVolt;
                    }
                }

                freqVoltPairs.Add(new Tuple<double, double>(freq * this.FASTFrequencyModifier, volt));
            }

            // sort by frequency (lowest to highest)
            freqVoltPairs.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            Prime.Services.ConsoleService.PrintDebug($"Points={string.Join(", ", freqVoltPairs)}");

            List<LinearFitStruct> results = new List<LinearFitStruct>();
            if (freqVoltPairs.Count > 1)
            {
                // calculate the slope and intercept between two data points.
                // We only do the calculation if there are more than 1 data points, otherwise this code would exception.
                for (var i = 0; i < freqVoltPairs.Count - 1; i++)
                {
                    var lowerPoint = freqVoltPairs[i];                // since we ordered the points, the lower index has the lower frequency
                    var upperPoint = freqVoltPairs[i + 1];            // we need two points to do generate slope & intercept
                    var ratioLower = lowerPoint.Item1 / this.BclkRef; // we do this math with ratio instead of frequency since the input file uses ratio
                    var ratioUpper = upperPoint.Item1 / this.BclkRef;
                    if (ratioUpper - ratioLower != 0)
                    {
                        var slope = (upperPoint.Item2 - lowerPoint.Item2) / (ratioUpper - ratioLower);
                        var intercept = upperPoint.Item2 - (slope * ratioUpper);
                        results.Add(new LinearFitStruct(slope, intercept, (int)Math.Round(ratioLower), (int)Math.Round(ratioUpper)));
                    }
                    else
                    {
                        // milespac 04 / 25 / 2018 - bugfix for UPSVF strings with multiple points that have the same Ratio, i.e.F5 = F6 = 4.3GHz
                        Prime.Services.ConsoleService.PrintDebug($"Upper point {freqVoltPairs[i + 1]} has the same ratio as the lower point {freqVoltPairs[i]}");
                    }
                }
            }

            if (freqVoltPairs.Count == 1 || results.Count == 0)
            {
                var ratioLower = this.Ratios.Min();           // we find the 1st frequency in the points list, and calculate the ratio
                var ratioUpper = this.Ratios.Max();           // we find the 1st frequency in the points list, and calculate the ratio
                var singleVoltage = freqVoltPairs[0].Item2;   // we use the voltage as the intercept point (x axis = ratio, y axis = voltage)

                // for 1 ratio, the slope is 0 and the intercept is equal to the voltage
                results.Add(new LinearFitStruct(slope: 0, intercept: singleVoltage, lowerRatio: ratioLower, upperRatio: ratioUpper));
            }

            Prime.Services.ConsoleService.PrintDebug($"{this.Name} Results: {string.Join(", ", results)}");

            // Compare the points for this result to the global min/max
            var minResultsLowerRatio = results.Min(x => x.RatioLower);
            var maxPossibleLowerRatio = (int)Math.Truncate(this.FrequencyMax / this.BclkRef); // we snap the MIN to the highest possible value so that we can compare and get the real minimum point
            int upsMinRatio = Math.Min(maxPossibleLowerRatio, minResultsLowerRatio); // UPSMinRatio is the lowest supported frequency point (F1 for instance)

            var maxResultsUpperRatio = results.Max(x => x.RatioUpper);
            var minPossibleUpperRatio = (int)Math.Truncate(this.FrequencyMin / this.BclkRef); // same as above but for MAX
            int upsMaxRatio = Math.Max(minPossibleUpperRatio, maxResultsUpperRatio); // UPSMinRatio is the lowest supported frequency point (F1 for instance)

            // now calculate the vmins
            var gbModifier = 0.0;
            /* code commented out in EmbPython implemenation
            dGBModifier = (evg.GetTpGlobalValue(dPLLInfo[sPLL].sIPGBVar, "double") * evg.GetTpGlobalValue(sGBVarsPHMSwitch, "integer")) */

            // use the slope & intercept to generate functional vmins only if a ratio is between two points
            foreach (var ratio in this.Ratios)
            {
                var calcVmin = 0.0;                         // initialize to 0 to wipe previous value
                foreach (var result in results)
                {
                    if ((ratio >= result.RatioLower && ratio <= result.RatioUpper) ||
                        (ratio <= upsMinRatio && upsMinRatio == result.RatioLower) ||
                        (ratio >= upsMaxRatio && upsMaxRatio == result.RatioUpper))
                    {
                        calcVmin = (result.Slope * ratio) + result.Intercept + gbModifier;
                    }

                    // if we are outside of the limits, snap to them
                    if (calcVmin > this.VoltageMax)
                    {
                        calcVmin = this.VoltageMax;
                    }

                    if (calcVmin < this.VoltageMin)
                    {
                        calcVmin = this.VoltageMin;
                    }
                }

                this.VMins.Add(Math.Round(calcVmin, 3)); // his is where we append the new calculated vmin to the datalogging structure.
            }

            Prime.Services.ConsoleService.PrintDebug($"VMIN's Generated for all ratios in {this.Name}={string.Join(", ", this.VMins)}");
            return new Tuple<int, int>(upsMinRatio, upsMaxRatio);
        }

        /// <summary>
        /// Executes the LockTime test for each Vcc.
        /// </summary>
        /// <param name="upsMinRatio">Minimum viable Ratio.</param>
        /// <param name="upsMaxRatio">Maximum viable Ratio.</param>
        /// <param name="minRatioMult">Optional scaling multiplier for checking the bounds against the lower ratio.</param>
        /// <param name="maxRatioMult">Optional scaling multiplier for checking the bounds against the upper ratio.</param>
        /// <returns>Exit Port.</returns>
        public int ExecuteVID(int upsMinRatio, int upsMaxRatio, double minRatioMult = 1.0, double maxRatioMult = 1.08)
        {
            var exitPort = 1;
            var eapFlag = Prime.Services.UserVarService.GetIntValue(this.UserVarExecuteAllPointsCollection, this.UserVarExecuteAllPointsVariable);
            var minRatio = (int)(upsMinRatio * minRatioMult);
            var maxRatio = (int)(upsMaxRatio * maxRatioMult);

            // loop through all ratios and only execute if the ratio is inside the range
            for (var index = 0; index < this.Ratios.Count; index++)
            {
                var ratio = this.Ratios[index];
                var vmin = this.VMins[index];
                var instanceName = $"{this.TestName[0]}{this.Name}{this.TestName[1]}R{this.FormattedRatios[index]}{this.TestName[2]}"; // This needs to match the locktime testnames in the TP
                var instanceRslt = 5; // default, not in range, not ExecuteAllPoints

                // find if we are in the range of valid ratios
                var inRange = ratio >= minRatio && ratio <= maxRatio;
                if (inRange || eapFlag == 1)
                {
                    if (this.AppType == "DIRECT_HW")
                    {
                        this.SetSharedData(this.VminGSDS, vmin); // this value is the vmin for this ratio
                    }
                    else if (this.AppType == "FIVR")
                    {
                        // we can have multiple fuseconfig setpoints which get the same voltage
                        foreach (var setpoint in this.SubDomains.Values)
                        {
                            var vdacVmin = (int)Math.Truncate(vmin * setpoint.Expression); // converts vmin into VDAC code
                            List<string> fuses = new List<string>();
                            foreach (var fuse in setpoint.Fuses)
                            {
                                fuses.Add($"{fuse}:0d{vdacVmin}"); // creates the fuse value from the subdomain dictionary
                            }

                            var functionArg = $"{setpoint.Name} {string.Join(",", fuses)} {this.PList}{this.FormattedRatios[index]}_vidwk_list {this.FIVRRegex}";
                            Prime.Services.ConsoleService.PrintDebug($"Setting Fuses = [{functionArg}]");
                            /* var funcRslt = Prime.Services.TestProgramService.ExecuteFunction("FUSE_CONFIG_CALLBACKS!setFuse", functionArg); */
                            Prime.Services.TestProgramService.SetTestInstanceParameter(this.FuseCfgTest, "function_parameter", functionArg);
                            var funcRslt = Prime.Services.TestProgramService.ExecuteTestInstance(this.FuseCfgTest);
                            Prime.Services.ConsoleService.PrintDebug($"Test [{this.FuseCfgTest}] returned {funcRslt}");
                            if (funcRslt != 1)
                            {
                                Prime.Services.ConsoleService.PrintError($"Test [{this.FuseCfgTest}] returned {funcRslt}");
                                exitPort = 0;
                            }
                        }
                    }
                    else
                    {
                        Prime.Services.ConsoleService.PrintError($"Application type must be DIRECT_HW or FIVR, not [{this.AppType}].");
                        exitPort = 0;
                    }

                    Prime.Services.ConsoleService.PrintDebug($"Executing {instanceName}");
                    instanceRslt = Prime.Services.TestProgramService.ExecuteTestInstance(instanceName);
                    if (!inRange)
                    {
                        instanceRslt += 8;
                    }

                    Prime.Services.ConsoleService.PrintDebug($"Instance {instanceName} returned {instanceRslt}");
                    if (inRange)
                    {
                        // only update the exit port if this was a valid Vmin
                        exitPort = Math.Min(exitPort, instanceRslt);
                    }
                }
                else
                {
                    instanceRslt = 5;
                    this.SetSharedData(this.VminGSDS, 99.0);
                }

                // save the results
                this.Results.Add(instanceRslt); // HRY gets either 1 or 0 depending on the test exit port

                // this loop will append the locktime for subpll's in the PLL
                foreach (var subPll in this.SubPLLs)
                {
                    var value = this.GetShareDouble($"{this.GSDSToken}_{subPll}");
                    this.LockTimes[subPll].Add(Math.Round(value, 3));
                    Prime.Services.ConsoleService.PrintDebug($"{subPll} Ratio={ratio} executed with locktime {this.LockTimes[subPll][index]}");
                }
            }

            if (exitPort == 0)
            {
                exitPort = 2; // Exit Port 0 of this UF is reserved for bin90, port 2 is the "fail" port
            }

            return exitPort;
        }

        /// <summary>
        /// Writes all the results to Ituff.
        /// </summary>
        /// <param name="exitport">Test instance exit port.</param>
        /// <param name="instanceName">Testinstance name to use for Ituff Logging.</param>
        public void DatalogResults(int exitport, string instanceName)
        {
            LogStrgvalWithSeparator($"_{this.Name}_RATIOS", string.Join("_", this.FormattedRatios));
            LogStrgvalWithSeparator($"_{this.Name}_HRY", string.Join("_", this.Results));
            LogStrgvalWithSeparator($"_{this.Name}_VMINS", string.Join("_", this.VMins));
            /* Prime.Services.DatalogService.WriteToItuff($"{ItuffHeader1()}{instanceName}_{this.Name}_RATIOS\n{ItuffHeader2()}{string.Join("_", this.FormattedRatios)}\n{ItuffHeader3()}");
            Prime.Services.DatalogService.WriteToItuff($"{ItuffHeader1()}{instanceName}_{this.Name}_HRY\n{ItuffHeader2()}{string.Join("_", this.Results)}\n{ItuffHeader3()}");
            Prime.Services.DatalogService.WriteToItuff($"{ItuffHeader1()}{instanceName}_{this.Name}_VMINS\n{ItuffHeader2()}{string.Join("_", this.VMins)}\n{ItuffHeader3()}"); */
            foreach (var subPll in this.SubPLLs)
            {
                LogStrgvalWithSeparator($"_{subPll}_LOCKTIME", string.Join("_", this.LockTimes[subPll]));
                /* Prime.Services.DatalogService.WriteToItuff($"{ItuffHeader1()}{instanceName}_{subPll}_LOCKTIME\n{ItuffHeader2()}{string.Join("_", this.LockTimes[subPll])}\n{ItuffHeader3()}"); */
            }

            if (exitport != 1)
            {
                LogStrgvalWithSeparator($"_{this.Name}_UPSVF", string.Join("_", this.UPSVF));
                /* Prime.Services.DatalogService.WriteToItuff($"{ItuffHeader1()}{instanceName}_{this.Name}_UPSVF\n{ItuffHeader2()}{this.UPSVF}\n{ItuffHeader3()}"); */
            }
        }

        private static void LogStrgvalWithSeparator(string tnamePostFix, string data)
        {
            var writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            writer.SetTnamePostfix(tnamePostFix);
            writer.SetData(data);
            Prime.Services.DatalogService.WriteToItuff(writer);

            var separator = Prime.Services.DatalogService.GetItuffSeparatorFormatWriter();
            Prime.Services.DatalogService.WriteToItuff(separator);
        }

        private string GetSharedString(string key)
        {
            return Prime.Services.SharedStorageService.GetStringRowFromTable(key, Prime.SharedStorageService.Context.DUT);
        }

        private double GetShareDouble(string key)
        {
            return Prime.Services.SharedStorageService.GetDoubleRowFromTable(key, Prime.SharedStorageService.Context.DUT);
        }

        private void SetSharedData(string key, double value)
        {
            Prime.Services.SharedStorageService.InsertRowAtTable(key, value, Prime.SharedStorageService.Context.DUT);
        }

        /// <summary>Defines the <see cref="SubDomainStruct" />.</summary>
        public class SubDomainStruct
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SubDomainStruct"/> class.
            /// </summary>
            /// <param name="setpoint">Setpoint Name.</param>
            /// <param name="expr">Int Expression.</param>
            /// <param name="fuses">Fuse Names, List of string.</param>
            public SubDomainStruct(string setpoint, int expr, List<string> fuses)
            {
                this.Name = setpoint;
                this.Expression = expr;
                this.Fuses = fuses;
            }

            /// <summary>
            /// Gets the setpoint name.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Gets the value which matches FIVR analog->digital conversion expression <fivr_initial_voltage_expression>VINPUT[]*256</fivr_initial_voltage_expression>.
            /// </summary>
            public int Expression { get; private set; }

            /// <summary>
            /// Gets the list of all fuses to write to if any.
            /// </summary>
            public List<string> Fuses { get; private set; }
        }

        /// <summary>Defines the <see cref="LinearFitStruct" />.</summary>
        public class LinearFitStruct
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LinearFitStruct"/> class.
            /// </summary>
            /// <param name="slope">Slope Value.</param>
            /// <param name="intercept">Intercept Value.</param>
            /// <param name="lowerRatio">Lower Ratio Value.</param>
            /// <param name="upperRatio">Upper Ratio Value.</param>
            public LinearFitStruct(double slope, double intercept, int lowerRatio, int upperRatio)
            {
                this.Slope = slope;
                this.Intercept = intercept;
                this.RatioLower = lowerRatio;
                this.RatioUpper = upperRatio;
            }

            /// <summary>Gets the Slope Value.</summary>
            public double Slope { get; private set; }

            /// <summary>Gets the Intercept Value.</summary>
            public double Intercept { get; private set; }

            /// <summary>Gets the Lower Ratio Value.</summary>
            public int RatioLower { get; private set; }

            /// <summary>Gets the Upper Ratio Value.</summary>
            public int RatioUpper { get; private set; }

            /// <summary>
            /// Implent string conversion for this struct.
            /// </summary>
            /// <returns>string representation of this object.</returns>
            public override string ToString()
            {
                return $"{{Slope:{this.Slope},Intercept:{this.Intercept},Ratio:({this.RatioLower},{this.RatioUpper})}}";
            }
        }
    }
}
