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

namespace PstateTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Prime;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.Utilities;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class PstateTest : TestMethodBase
    {
        private ICaptureFailureAndCtvPerPinTest funcTest;

        /// <summary>
        /// Gets or sets the LevelsTc name.
        /// </summary>
        public TestMethodsParams.LevelsCondition LevelsTc { get; set; }

        /// <summary>
        /// Gets or sets the TimingsTc name.
        /// </summary>
        public TestMethodsParams.TimingCondition TimingsTc { get; set; }

        /// <summary>
        /// Gets or sets the Patlist name.
        /// </summary>
        public TestMethodsParams.Plist Patlist { get; set; }

        /// <summary>
        /// Gets or sets the VidDomain (this comment will be used on the pre-header file).
        /// </summary>
        public TestMethodsParams.String VidDomain { get; set; }

        /// <summary>
        /// Gets or sets the InputFile (this comment will be used on the pre-header file).
        /// </summary>
        public TestMethodsParams.String InputFile { get; set; }

        /// <summary>
        /// Gets or sets the CapData input file (this comment will be used on the pre-header file).
        /// </summary>
        public TestMethodsParams.String CapDataDef { get; set; }

        /// <summary>
        /// Gets or sets the FuncVminGsdsToken name.
        /// </summary>
        public TestMethodsParams.String FuncVminGsdsToken { get; set; }

        /// <summary>
        /// Gets or sets the FailCount number.
        /// </summary>
        public TestMethodsParams.Integer FailCount { get; set; }

        /// <summary>
        /// Gets or sets comma-separated list of pins to capture.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString CapturePins { get; set; }

        /// <summary>
        /// Gets or sets comma-separated list of pins to mask.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString MaskPins { get; set; }

        /// <summary>
        /// Gets or sets pre plist.
        /// </summary>
        public TestMethodsParams.String PrePlist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets FailCountUlong.
        /// </summary>
        public ulong FailCountUlong { get; set; }

        /// <summary>
        /// Gets or sets inputFile path in SC.
        /// </summary>
        protected string InputFilePath { get; set; }

        /// <summary>
        /// Gets or sets CapData file path in SC.
        /// </summary>
        protected string CapDataFilePath { get; set; }

        /// <summary>
        /// Gets or sets gsds data from Json file.
        /// </summary>
        protected JsonData FileData { get; set; }

        /// <summary>
        /// Gets or sets local CapData file.
        /// </summary>
        protected CapData CapData { get; set; }

        /// <summary>
        /// Gets or sets Curves object.
        /// </summary>
        protected VFCurves Curves { get; set; }

        /// <summary>
        /// Gets or sets DataTokens object.
        /// </summary>
        protected DataTokens Tokens { get; set; }

        /// <summary>
        /// Gets or sets GsdsData dict.
        /// </summary>
        protected Dictionary<string, string> GsdsData { get; set; }

        /// <summary>
        /// Gets or sets FirstPin.
        /// </summary>
        protected string FirstPin { get; set; }

        /// <summary>
        /// Gets or sets DatalogStrings.
        /// </summary>
        protected List<string> DatalogStrings { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            // check for empty string
            if (this.LevelsTc == string.Empty)
            {
                throw new ArgumentException("LevelsTc must be a valid string.");
            }

            // check for empty string
            if (this.TimingsTc == string.Empty)
            {
                throw new ArgumentException("TimingsTc must be a valid string.");
            }

            // check for empty string
            if (this.Patlist == string.Empty)
            {
                throw new ArgumentException("Patlist must be a valid string.");
            }

            // check for invalid integer
            if (this.FailCount == string.Empty)
            {
                throw new ArgumentException("FailCount must be a valid string.");
            }
            else
            {
                if ((int)this.FailCount < 0)
                {
                    throw new ArgumentException("FailCount must be >= 0. Disabled: 0. Enabled: 1.");
                }
                else
                {
                    this.FailCountUlong = Convert.ToUInt64((int)this.FailCount);
                }
            }

            // check for empty string
            if (this.VidDomain == string.Empty)
            {
                throw new ArgumentException("VidDomain must be a valid string.");
            }

            // check for empty string
            if (this.FuncVminGsdsToken == string.Empty)
            {
                throw new ArgumentException("FuncVminGsdsToken must be a valid string.");
            }

            // MaskPins can be empty, but CapturePins can't
            if (this.CapturePins == string.Empty)
            {
                throw new ArgumentException("CapturePins must be a valid comma-separated string.");
            }
            else
            {
                // check that pins are valid
                foreach (var pin in this.CapturePins.ToList())
                {
                    if (!Prime.Services.PinService.Exists(pin))
                    {
                        throw new ArgumentException($"CapturePins: {pin} is not a valid pin.");
                    }
                }

                // TODO: support multiple pins
                this.FirstPin = this.CapturePins.ToList().First<string>();
            }

            // check for empty string
            if (this.InputFile == string.Empty)
            {
                throw new ArgumentException("InputFile must be a valid JSON file.");
            }

            // check for empty string
            if (this.CapDataDef == string.Empty)
            {
                throw new ArgumentException("CapDataDef must be a valid JSON file.");
            }

            // TODO: refactor the below
            this.InputFilePath = Prime.Services.FileService.GetFile(this.InputFile);
            this.ReadInputFile();
            this.ProcessFileData();

            this.CapDataFilePath = Prime.Services.FileService.GetFile(this.CapDataDef);
            this.ReadCapDataFile();
            this.ProcessCapData();

            // ensure each curve inst has token data
            // TODO: add unit test
            foreach (var keyValuePair in this.Curves.Curves)
            {
                // look for token
                if (!this.Tokens.TokenExists(keyValuePair.Key))
                {
                    throw new ArgumentException($"No DataSpec for instance {keyValuePair.Key}!");
                }
            }

            this.DatalogStrings = new List<string> { };

            // set up func test: always allow at least 1 fail
            ulong tempFailCount = this.FailCountUlong == 0 ? 1 : this.FailCountUlong;
            this.funcTest = Prime.Services.FunctionalService.CreateCaptureFailureAndCtvPerPinTest(this.Patlist, this.LevelsTc, this.TimingsTc, this.CapturePins.ToList(), tempFailCount, this.PrePlist);
        }

        /// <inheritdoc />
        [Returns(3, PortType.Fail, "Capture Fail!")]
        [Returns(2, PortType.Fail, "Pattern Fail!")]
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            int retval = 1;
            string funcVminToken;
            List<IFailureData> failData = new List<IFailureData>();
            Dictionary<string, string> captureData;
            this.DatalogStrings.Clear();

            // read GSDS token
            try
            {
                funcVminToken = Prime.Services.SharedStorageService.GetStringRowFromTable(this.FuncVminGsdsToken, Prime.SharedStorageService.Context.DUT);
            }
            catch (Exception e)
            {
                // throw exception or return -1?
                Prime.Services.ConsoleService.PrintError(e.Message);

                // throw new ArgumentException($"{this.FuncVminGsdsToken} wasn't successfully read.");
                return -1;
            }

            // decode voltages
            // check for single domain string match
            List<string> domainVFStrings = new List<string>();
            List<int> freqPoints = new List<int>();
            List<float> voltPoints = new List<float>();

            if (!funcVminToken.Contains("_" + this.VidDomain + @":"))
            {
                // throw exception or return -1?
                Prime.Services.ConsoleService.PrintError($"{this.FuncVminGsdsToken} doesn't contain domain {this.VidDomain}.");
                return -1;
            }
            else
            {
                if (!this.ParseTestPoints(funcVminToken, ref freqPoints, ref voltPoints))
                {
                    return -1;
                }
            }

            // update VF curves
            if (!this.Curves.UpdateVF(freqPoints, voltPoints))
            {
                return -1;
            }

            // TODO: skip pats based on freq range
            // this.UpdatePList();

            // TODO: pat mod voltages
            // this.UpdateTargetVoltages();

            // TODO: use of retval is messy. Maybe should throw exception. It also makes debug easier.
            // exec burst
            if (!string.IsNullOrEmpty(this.MaskPins))
            {
                this.funcTest.SetPinMask(this.MaskPins.ToList());
            }

            var status = this.funcTest.Execute();
            if (!status)
            {
                this.funcTest.DatalogFailure(1);
                failData = this.funcTest.GetPerCycleFailures();
            }

            captureData = this.funcTest.GetCtvData();

            // check status
            // check if capture data is valid
            // TODO: add unit test here?
            if (!this.IsCapDataValid(ref retval, ref captureData))
            {
                // if decode() returns false, exit test immediately
                return -1;
            }

            // decode cap mem - this would be specific to instance...
            if (!this.DecodeCaptureData(ref retval, ref captureData))
            {
                // if decode() returns false, exit test immediately
                return -1;
            }

            // check fail data for pat fails
            this.CheckFailData(ref retval, failData);

            // datalog results
            if (!this.DatalogResults())
            {
                return -1;
            }

            // make sure exit port is consistent w/ status var

            // assign exit port
            return retval;
        }

        private bool DatalogResults()
        {
            if (this.DatalogStrings.Count == 0)
            {
                return false;
            }
            else
            {
                int i = 0;
                var strgvalWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();

                foreach (var vf in this.Curves.Curves)
                {
                    int j = 0;
                    foreach (var test in vf.Value.TestFreqs)
                    {
                        strgvalWriter.SetData(this.DatalogStrings[i++]);
                        strgvalWriter.SetTnamePostfix($"{vf.Key}_{j}");
                        Prime.Services.DatalogService.WriteToItuff(strgvalWriter);
                    }
                }
            }

            return true;
        }

        // check if capture data is valid
        private bool IsCapDataValid(ref int retval, ref Dictionary<string, string> capdata)
        {
            // if no data, set exit port to -1
            if (capdata.Count == 0)
            {
                Prime.Services.ConsoleService.PrintError($"Capture data is empty.");
                retval = -1;
                return false;
            }
            else if (!capdata.ContainsKey(this.FirstPin))
            {
                Prime.Services.ConsoleService.PrintError($"No capture data for pin {this.FirstPin}.");
                retval = -1;
                return false;
            }

            int bitCount = 0;

            // if incorrect # of bits, set exit port to -1
            foreach (var vf in this.Curves.Curves)
            {
                // multiply # of freqs by # bits captured
                bitCount += vf.Value.TestFreqs.Count * this.Tokens.Length(vf.Key);
            }

            if (capdata[this.FirstPin].Length != bitCount)
            {
                Prime.Services.ConsoleService.PrintError($"Expected: {bitCount} bits of capture data, but received: {capdata[this.FirstPin].Length}.");
                retval = -1;
                return false;
            }

            return true;
        }

        // decode according to input file, set exit port
        private bool DecodeCaptureData(ref int retval, ref Dictionary<string, string> capdata)
        {
            // TODO: how to handle multiple pins?

            // decode cap data
            int start = 0;
            var instPassFailVec = new List<bool> { };
            foreach (var inst in this.Curves.Curves)
            {
                // TODO: below assumes only capture fields to be decoded
                // substring based on # of freqs and length of token
                int bitLen = inst.Value.TestFreqs.Count * this.Tokens.Length(inst.Key);
                string data = capdata[this.FirstPin].Substring(start, bitLen);

                // provide inst name and ref to capdata to DataTokens, get pass/fail status and iTuFF tokens
                var passFailVector = new List<bool> { };
                var datalogStrings = new List<string> { };
                if (!this.Tokens.DecodeCapData(inst.Key, start, bitLen, ref data, out passFailVector, out datalogStrings))
                {
                    Prime.Services.ConsoleService.PrintError($"Could not decode capture data for instance: {inst}");
                    retval = -1;
                    return false;
                }

                if (passFailVector.Contains(false))
                {
                    instPassFailVec.Add(false);
                }
                else
                {
                    instPassFailVec.Add(true);
                }

                // print pass/ fail results
                if (this.LogLevel >= TestMethodBase.PrimeLogLevel.PRIME_DEBUG)
                {
                    int i = 0;
                    var sbDebug = new StringBuilder();
                    MethodBase m = MethodBase.GetCurrentMethod();
                    sbDebug.AppendLine($"{m.Name}: Pass/ Fail status for instance: {inst.Key}:");
                    foreach (var freq in inst.Value.TestFreqs)
                    {
                        sbDebug.AppendLine($"\tfreq: {freq} status: {passFailVector[i]}");
                        sbDebug.AppendLine($"\tfreq: {freq} datalog: {datalogStrings[i]}");
                        i++;
                    }

                    Prime.Services.ConsoleService.PrintDebug(sbDebug.ToString());
                }

                // update datalogging strings
                this.DatalogStrings.AddRange(datalogStrings);

                start += bitLen;
            }

            // if failing cap data, set exit port to 3
            if (instPassFailVec.Contains(false))
            {
                retval = 3;
            }

            // otherwise, just return
            return true;
        }

        // trivial implementation: check fail count to set exit port
        private void CheckFailData(ref int retval, List<IFailureData> failData)
        {
            // check for failures
            if (this.FailCountUlong > 0 && failData.Count > 0)
            {
                retval = 2;
            }

            // any other processing?

            // if failcount is 0, then ignore failures, otherwise set failing exit port
            return;
        }

        private void UpdateTargetVoltages()
        {
            throw new NotImplementedException();
        }

        private void UpdatePList()
        {
            // iterate thru VF curves
            /*foreach (var pair in this.Curves.Curves)
            {
                // get nested Patlist based on naming convention

                // TODO: skip pats up to Fmin? not sure if valid

                // skip pats above Fmax + freq GB
                int freqGB = pair.Value.FmaxGB;
            }*/

            throw new NotImplementedException();
        }

        private bool ParseTestPoints(string funcVminToken, ref List<int> freqPoints, ref List<float> voltPoints)
        {
            // sUPSVF = evg.GetGSDSData(sUPSGSDS,"string","UNT",-99,0) #FAST Standard Format >> domain1:freq1^vmin1%freq2^vmin2_domain2:freq3^vmin3 ***Frequency in GHz
            // CR:4.800^1.169v1.148v1.171v1.140v-9999v-9999v-9999v-9999%4.200^1.013v1.008v1.017v1.003v-9999v-9999v-9999v-9999%3.400^0.846v0.835v0.839v0.834v-9999v-9999v-9999v-9999%2.200^0.655v0.650v0.648v0.655v-9999v-9999v-9999v-9999%1.200^0.590v0.580v0.590v0.590v-9999v-9999v-9999v-9999%0.400^0.560v0.540v0.550v0.570v-9999v-9999v-9999v-9999
            // SAQ:2.700^0.785%2.200^0.685%1.100^0.555
            List<string> domainVFStrings;
            try
            {
                domainVFStrings = funcVminToken.Split('_').ToList<string>().Find(x => x.StartsWith(this.VidDomain + @":")).Split(':').ToList<string>()[1].Split('%').ToList<string>();
                foreach (var item in domainVFStrings)
                {
                    var results = item.Split('^');
                    float freq;
                    if (!float.TryParse(results[0], out freq))
                    {
                        Prime.Services.ConsoleService.PrintError($"Can't convert {results[0]} to float.");
                        throw new ArgumentException($"Can't convert {results[0]} to float.");
                    }
                    else
                    {
                        freq *= 1000f;
                        freqPoints.Add((int)Math.Round(freq));
                    }

                    float volt;
                    if (!float.TryParse(results[1], out volt))
                    {
                        Prime.Services.ConsoleService.PrintError($"Can't convert {results[1]} to float.");
                        throw new ArgumentException($"Can't convert {results[1]} to float.");
                    }
                    else
                    {
                        voltPoints.Add(volt);
                    }
                }
            }
            catch (Exception)
            {
                Prime.Services.ConsoleService.PrintError($"Can't split {this.FuncVminGsdsToken} into valid VF strings for domain {this.VidDomain}.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// method creates dict of freq, volt pairs, based on JSON data.
        /// </summary>
        /// <param name="s">CSV string contiaining list of freq test points.</param>
        /// <param name="maxV">float containing max voltage to test.</param>
        /// <param name="minV">float containing min voltage to test.</param>
        /// <param name="freqs">output List of int.</param>
        /// <param name="volts">output list of float.</param>
        private void Interpolate(string s, float maxV, float minV, out List<int> freqs, out List<float> volts)
        {
            Dictionary<int, float> points = new Dictionary<int, float> { };

            // maxV > minV
            if (minV > maxV)
            {
                throw new ArgumentException($"minV can't be larger than maxV: {minV} > {maxV}.");
            }

            // divide volt range by # of freqs
            freqs = s.Split(',').Select(int.Parse).ToList();
            float vBins = (maxV - minV) / (freqs.Count - 1);
            volts = this.Range(minV, maxV, vBins).ToList();

            /*// split minV, maxV range into equally spaced bins https://stackoverflow.com/questions/2434593/create-a-dictionary-using-2-lists-using-linq
            points = freqs.Zip(volts, (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);
            return points;*/
        }

        // https://stackoverflow.com/questions/7552839/generate-sequence-with-step-value
        private IEnumerable<float> Range(float min, float max, float step)
        {
            float i;
            for (i = min; i <= max; i += step)
            {
                yield return i;
            }

            if (i != max + step)
            {
                yield return max;
            }
        }

        /// <summary>
        /// Reads InputFile and stuffs GSDS table.
        /// </summary>
        private void ReadInputFile()
        {
            try
            {
                this.FileData = JsonConvert.DeserializeObject<JsonData>(File.ReadAllText(this.InputFilePath));
                if (this.FileData == null)
                {
                    throw new ArgumentException("No data was parsed.");
                }
            }
            catch (JsonException e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <summary>
        /// Reads CapData file.
        /// </summary>
        private void ReadCapDataFile()
        {
            try
            {
                this.CapData = JsonConvert.DeserializeObject<CapData>(File.ReadAllText(this.CapDataFilePath));
                if (this.CapData == null)
                {
                    throw new ArgumentException("No capture data syntax was parsed.");
                }
            }
            catch (JsonException e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <summary>
        /// Process FileData, create VFCurves.
        /// </summary>
        private void ProcessFileData()
        {
            // check if domain exists in dictionary keys
            if (!this.FileData.Domains.ContainsKey(this.VidDomain))
            {
                throw new ArgumentException($"InputFile Domain dict must contain key {this.VidDomain}.");
            }

            // check if single domain exists in dictionary keys
            if (this.FileData.Domains.Keys.Count != 1)
            {
                throw new ArgumentException($"InputFile Domain dict must contain only 1 domain: {this.VidDomain}.");
            }

            // create dict of test points tokens TODO: unit tests here
            // TODO: remove this logic and provide single GSDS?
            this.GsdsData = new Dictionary<string, string>();
            foreach (var key in this.FileData.Domains[this.VidDomain]["corners"].Keys)
            {
                if (key == string.Empty)
                {
                    throw new ArgumentException($"corners sub-dict can't contain empty strings as keys.");
                }

                if (!this.FileData.Domains[this.VidDomain]["corners"][key].ContainsKey("gsds"))
                {
                    throw new ArgumentException($"Corner {key} must have corresponding GSDS token.");
                }
                else if (this.FileData.Domains[this.VidDomain]["corners"][key]["gsds"] == string.Empty)
                {
                    throw new ArgumentException($"Corner {key} must have a non-empty GSDS token name.");
                }

                try
                {
                    this.GsdsData.Add(this.FileData.Domains[this.VidDomain]["corners"][key]["gsds"], null);
                }
                catch (Exception e)
                {
                    throw new ArgumentException(e.Message);
                }
            }

            // create instance of VFCurves to hold VFs
            this.Curves = new VFCurves(this.FileData.Domains[this.VidDomain] + "_Curves");

            // iterate thru instances, creating VF curves
            // TODO: use Parallel.Foreach?
            foreach (string inst in this.FileData.Domains[this.VidDomain]["instances"].Keys)
            {
                // get instance-specific VF Curve data
                float minV;
                float maxV;
                int minF;
                int maxF;
                float incF;
                float freqGB;

                if (!float.TryParse(this.FileData.Domains[this.VidDomain]["instances"][inst]["minV"], out minV))
                {
                    throw new ArgumentException($"Can't convert {this.FileData.Domains[this.VidDomain]["instances"][inst]["minV"]} to float.");
                }

                if (!float.TryParse(this.FileData.Domains[this.VidDomain]["instances"][inst]["maxV"], out maxV))
                {
                    throw new ArgumentException($"Can't convert {this.FileData.Domains[this.VidDomain]["instances"][inst]["maxV"]} to float.");
                }

                if (minV > maxV)
                {
                    throw new ArgumentException($"minV can't be larger than maxV: {minV} > {maxV}.");
                }

                if (!float.TryParse(this.FileData.Domains[this.VidDomain]["instances"][inst]["freqGB"], out freqGB))
                {
                    throw new ArgumentException($"Can't convert {this.FileData.Domains[this.VidDomain]["instances"][inst]["freqGB"]} to float.");
                }

                if (!int.TryParse(this.FileData.Domains[this.VidDomain]["instances"][inst]["minF"], out minF))
                {
                    throw new ArgumentException($"Can't convert {this.FileData.Domains[this.VidDomain]["instances"][inst]["minF"]} to int.");
                }

                if (!int.TryParse(this.FileData.Domains[this.VidDomain]["instances"][inst]["maxF"], out maxF))
                {
                    throw new ArgumentException($"Can't convert {this.FileData.Domains[this.VidDomain]["instances"][inst]["maxF"]} to int.");
                }

                if (!float.TryParse(this.FileData.Domains[this.VidDomain]["instances"][inst]["incF"], out incF))
                {
                    throw new ArgumentException($"Can't convert {this.FileData.Domains[this.VidDomain]["instances"][inst]["incF"]} to float.");
                }

                // get initial interpolated values based on minV, max V
                // TODO: check freqPoints vs min max
                // Dictionary<int, float> initialPoints = this.Interpolate(this.FileData.Domains[this.VidDomain]["instances"][inst]["freqPoints"].ToString(), maxV, minV);
                List<int> freqPoints = new List<int>();
                List<float> voltPoints = new List<float>();
                this.Interpolate(this.FileData.Domains[this.VidDomain]["instances"][inst]["freqPoints"].ToString(), maxV, minV, out freqPoints, out voltPoints);

                // check keys collection is same count TODO: unit test here
                if (freqPoints.Count != this.GsdsData.Count)
                {
                    throw new ArgumentException($"Instance {inst} must have same number of freq points {freqPoints.Count} as domain corners {this.GsdsData.Count}.");
                }

                // TODO: check # of freqs vs # of pats in Patlist

                // add VF table w/ initial voltages based on minV, maxV
                if (!this.Curves.AddVF(this.VidDomain, inst, minV, maxV, minF, maxF, incF, freqGB, freqPoints, voltPoints))
                {
                    throw new ArgumentException($"Could not add VF object for domain: {this.VidDomain} and inst: {inst}.");
                }
            }
        }

        /// <summary>
        /// Process CapData var, create DataTokens.
        /// </summary>
        private void ProcessCapData()
        {
            // check if domain exists in dictionary keys
            if (!this.CapData.Domains.ContainsKey(this.VidDomain))
            {
                throw new ArgumentException($"InputFile Domain dict must contain key {this.VidDomain}.");
            }

            // process CapData
            if (this.CapData.Domains.Keys.Count != 1)
            {
                throw new ArgumentException($"InputFile Domain dict must contain only 1 domain: {this.VidDomain}.");
            }

            // create instance of DataTokens to hold datalogs
            this.Tokens = new DataTokens(this.CapData.Domains[this.VidDomain] + "_Tokens");

            // iterate thru instances, creating DatalogTokens
            // TODO: use Parallel.Foreach ?
            foreach (string inst in this.CapData.Domains[this.VidDomain]["instances"].Keys)
            {
                // check if instance exists in VFCurves
                if (!this.Curves.VFExists(inst))
                {
                    throw new ArgumentException($"No curve found for {inst}.");
                }

                // handle _register metadata
                int regLen = 0;
                if (!this.CapData.Domains[this.VidDomain]["instances"][inst].ContainsKey("_register"))
                {
                    throw new ArgumentException($"No _register metadata found for {inst}.");
                }
                else
                {
                    string regLenStr = this.CapData.Domains[this.VidDomain]["instances"][inst]["_register"]["length"];
                    if (!int.TryParse(regLenStr, out regLen))
                    {
                        throw new ArgumentException($"No _register length metadata found for {inst}.");
                    }
                    else
                    {
                        if (!this.Tokens.AddCapLen(inst, regLen))
                        {
                            throw new ArgumentException($"Could not set _register length metadata for {inst}.");
                        }
                    }
                }

                foreach (var token in this.CapData.Domains[this.VidDomain]["instances"][inst].Keys)
                {
                    if (token.StartsWith("_"))
                    {
                        continue;
                    }

                    List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();

                    // check that field is non-empty
                    if (string.IsNullOrEmpty(token))
                    {
                        throw new ArgumentException($"Field: {token} for {inst} must be valid string.");
                    }

                    // validate positions
                    string posString = this.CapData.Domains[this.VidDomain]["instances"][inst][token]["positions"];
                    if (string.IsNullOrEmpty(posString))
                    {
                        throw new ArgumentException($"Field: {token} for {inst} must have bit positions.");
                    }
                    else
                    {
                        // split string by comma
                        List<string> ranges = posString.Split(',').ToList<string>();
                        int msb = -1;
                        int lsb = -1;

                        // split substrings by colon
                        foreach (var item in ranges)
                        {
                            List<string> msbLsb = item.Split(':').ToList<string>();

                            if (msbLsb.Count == 0)
                            {
                                throw new ArgumentException($"Field: {token} for {inst} must have bit positions.");
                            }
                            else if (msbLsb.Count == 1)
                            {
                                if (!int.TryParse(msbLsb[0], out msb))
                                {
                                    throw new ArgumentException($"Field: {token} for {inst} must have valid bit positions.");
                                }
                                else
                                {
                                    lsb = -1;
                                }
                            }
                            else if (msbLsb.Count == 2)
                            {
                                if (!int.TryParse(msbLsb[0], out msb) || !int.TryParse(msbLsb[1], out lsb))
                                {
                                    throw new ArgumentException($"Field: {token} for {inst} must have valid bit positions.");
                                }
                            }
                            else
                            {
                                throw new ArgumentException($"Field: {token} for {inst} must have valid bit positions.");
                            }

                            tuples.Add(new Tuple<int, int>(msb, lsb));
                        }
                    }

                    // validate limits
                    string limsString = this.CapData.Domains[this.VidDomain]["instances"][inst][token]["limits"];
                    List<Tuple<string, int>> lims = new List<Tuple<string, int>>();
                    if (!string.IsNullOrEmpty(limsString))
                    {
                        // split string by comma
                        int num;
                        string op;
                        List<string> limits = limsString.Split(',').ToList<string>();
                        foreach (var item in limits)
                        {
                            // expect "<operator><integer>"
                            Regex rx = new Regex(@"^([><=]+)(\d+)$");
                            if (!rx.IsMatch(item))
                            {
                                throw new ArgumentException($"Field: {token} for {inst} must use valid format for limits.");
                            }
                            else
                            {
                                var matches = rx.Match(item);

                                // convert string to int
                                if (!int.TryParse(matches.Groups[2].Value, out num))
                                {
                                    throw new ArgumentException($"Field: {token} for {inst} must use valid format for limits.");
                                }

                                op = matches.Groups[1].Value;
                            }

                            lims.Add(new Tuple<string, int>(op, num));
                        }
                    }

                    // validate datalog index
                    int index;
                    if (!int.TryParse(this.CapData.Domains[this.VidDomain]["instances"][inst][token]["datalog"], out index))
                    {
                        throw new ArgumentException($"Field: {token} for {inst} must have a valid datalog index.");
                    }

                    // add to DataTokens
                    if (!this.Tokens.AddDataSpec(
                    inst,
                    token,
                    tuples,
                    this.CapData.Domains[this.VidDomain]["instances"][inst][token]["expression"],
                    lims,
                    index))
                    {
                        throw new ArgumentException($"Couldn't add new datalog token: {token} for inst: {inst}.");
                    }
                }
            }
        }
    }
}