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

using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VminForwardingSaveAsUpsGsdsTC.UnitTest")]

namespace VminForwardingSaveAsUpsGsdsTC
{
    using System.Collections.Generic;
    using System.Linq;
    using DDG;
    using Prime.Base.Exceptions;
    using Prime.PhAttributes;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class VminForwardingSaveAsUpsGsdsTC : TestMethodBase
    {
        /// <summary>
        /// Simple enum to hold True/False parameter values.
        /// </summary>
        public enum MyBool
        {
            /// <summary>
            /// Enum for True.
            /// </summary>
            True,

            /// <summary>
            /// Enum For false.
            /// </summary>
            False,
        }

        /// <summary>
        /// Gets or sets the name of the GSDS token to hold the vmin results for the 1st tested flow.
        /// </summary>
        public TestMethodsParams.String UpsVfGsds { get; set; } = "G.U.S.FAST_UPSVF";

        /// <summary>
        /// Gets or sets the name of the GSDS token to hold the vmin results for the passing flow.
        /// </summary>
        public TestMethodsParams.String UpsVfPassinFlowGsds { get; set; } = "G.U.S.FAST_UPSVFPASSFLOW";

        /// <summary>
        /// Gets or sets the name of the GSDS token to hold the full vmin results of all FAST corners.
        /// </summary>
        public TestMethodsParams.String FastCornersGsds { get; set; } = "G.U.S.FAST_CORNERS";

        /// <summary>
        /// Gets or sets the name of the GSDS token to hold the full vmin results of all FAST corners (FAST_STC_V).
        /// </summary>
        public TestMethodsParams.String FastStcGsds { get; set; } = "G.U.S.FAST_STC_V";

        /// <summary>
        /// Gets or sets the GSDS Token containing the current/passing flow.
        /// </summary>
        public TestMethodsParams.String PassingFlowInputGsds { get; set; } = "G.U.I.DDGVminForwardPassingFlow";

        /// <summary>
        /// Gets or sets the Mode. True means to read the EVG FAST tokens and modify them. False means to ignore any existing EVG data.
        /// </summary>
        public MyBool MergeWithEvgData { get; set; } = MyBool.False;

        /// <inheritdoc />
        public override void Verify()
        {
            var failures = 0;
            if (this.UpsVfGsds == string.Empty)
            {
                Prime.Services.ConsoleService.PrintError("Parameter=[UpsVfGsds] is required.");
                failures++;
            }

            if (this.UpsVfPassinFlowGsds == string.Empty)
            {
                Prime.Services.ConsoleService.PrintError("Parameter=[UpsVfPassingFlowGsds] is required.");
                failures++;
            }

            if (this.FastCornersGsds == string.Empty)
            {
                Prime.Services.ConsoleService.PrintError("Parameter=[FastCornersGsds] is required.");
                failures++;
            }

            if (this.FastStcGsds == string.Empty)
            {
                Prime.Services.ConsoleService.PrintError("Parameter=[FastStcGsds] is required.");
                failures++;
            }

            if (string.IsNullOrEmpty(this.PassingFlowInputGsds))
            {
                Prime.Services.ConsoleService.PrintError("Parameter=[PassingFlowInputGsds] is required to be a GSDS token name.");
                failures++;
            }

            if (failures > 0)
            {
                throw new Exception($"{this.InstanceName} failed Verify.");
            }
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            this.SaveGsdsTokens(this.MergeWithEvgData == MyBool.True);
            return 1;
        }

        private static double GetFrequencyForCorner(string domainGroupName, string frequencyCorner, int flowId)
        {
            try
            {
                var instances = DDG.VminForwarding.Service.GetInstanceNamesForDomain(domainGroupName);
                var freq = DDG.VminForwarding.Service.GetFrequency($"{instances.First()}@{frequencyCorner}", flowId);
                return freq;
            }
            catch (System.Exception e)
            {
                Prime.Services.ConsoleService.PrintError($"Failed to get Frequency for Domain=[{domainGroupName}] FreqCorner=[{frequencyCorner}] Flow=[{flowId}]: {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        private void DataLogToken(string token, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                value = "x"; // TODO: Can you datalog an empty string?
            }

            Prime.Services.ConsoleService.PrintDebug($"Writing Token=[{token}] as Ituff Data=[{value}].");
            var writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            writer.SetTnamePostfix($"::{token}");
            writer.SetData(value);
            Prime.Services.DatalogService.WriteToItuff(writer);
        }

        private void SaveGsdsTokens(bool mergeWithEVg = false)
        {
            var passingFlowAsString = Convert.ToString(DDG.Gsds.ReadToken(this.PassingFlowInputGsds));
            if (!int.TryParse(passingFlowAsString, out var passingFlow))
            {
                throw new TestMethodException($"Error: GsdsToken for the passing flow [{this.PassingFlowInputGsds}] does not contain a valid integer. Value=[{passingFlowAsString}].");
            }

            var finalUpsData = new UPS_Data();
            if (mergeWithEVg)
            {
                finalUpsData = VminForwardingSaveAsUpsGsdsTC.UPS_Data.BuildFromEvgTokens(passingFlow);

                var evgtokens = finalUpsData.ToGsdsTokens(passingFlow);
                this.DataLogToken("EVG::FAST_UPSVF", evgtokens["FAST_UPSVF"]);
                this.DataLogToken("EVG::FAST_UPSVFPASSFLOW", evgtokens["FAST_UPSVFPASSFLOW"]);
                this.DataLogToken("EVG::FAST_CORNERS", evgtokens["FAST_CORNERS"]);
                this.DataLogToken("EVG::FAST_STC_V", evgtokens["FAST_STC_V"]);
            }

            var numberOfFlows = (int)Prime.Services.BinMatrixService.GetNumberOfFlows();
            var allDomainNames = DDG.VminForwarding.Service.GetAllDomainNames();
            foreach (var domainName in allDomainNames)
            {
                var instanceNames = DDG.VminForwarding.Service.GetInstanceNamesForDomain(domainName);
                var cornerNames = DDG.VminForwarding.Service.GetCornerNamesForDomainInstance(instanceNames.First()).Select(c => c.Split('@').Last());

                foreach (var freqCornerName in cornerNames)
                {
                    // TODO: does UPS really need the level_select id for the corner?
                    var freqCornerId = 999;

                    for (var flowId = 1; flowId <= numberOfFlows; flowId++)
                    {
                        for (var subDomainIndex = 0; subDomainIndex < instanceNames.Count; subDomainIndex++)
                        {
                            var cornerName = $"{instanceNames[subDomainIndex]}@{freqCornerName}";
                            double currentVcc;
                            try
                            {
                                currentVcc = DDG.VminForwarding.Service.Get(cornerName, flowId).GetStartingVoltage(-9999);
                            }
                            catch
                            {
                                // prime doesn't like it when shared storage used for dff doesn't have data for a flow, but its ok, just use -9999;
                                currentVcc = -9999d;
                            }

                            var frequencyInGhz = DDG.VminForwarding.Service.GetFrequency(cornerName, flowId) / 1e9;
                            finalUpsData.InsertValue(currentVcc, domainName, subDomainIndex, freqCornerName, freqCornerId, frequencyInGhz, flowId - 1);
                        }
                    }
                }
            }

            var tokens = finalUpsData.ToGsdsTokens(passingFlow);
            this.DataLogToken("PRIME::FAST_UPSVF", tokens["FAST_UPSVF"]);
            this.DataLogToken("PRIME::FAST_UPSVFPASSFLOW", tokens["FAST_UPSVFPASSFLOW"]);
            this.DataLogToken("PRIME::FAST_CORNERS", tokens["FAST_CORNERS"]);
            this.DataLogToken("PRIME::FAST_STC_V", tokens["FAST_STC_V"]);

            DDG.Gsds.WriteToken(this.UpsVfGsds, tokens["FAST_UPSVF"]);
            DDG.Gsds.WriteToken(this.UpsVfPassinFlowGsds, tokens["FAST_UPSVFPASSFLOW"]);
            DDG.Gsds.WriteToken(this.FastCornersGsds, tokens["FAST_CORNERS"]);
            DDG.Gsds.WriteToken(this.FastStcGsds, tokens["FAST_STC_V"]);
        }

        /// <summary>
        /// Structure to hold UPS data for Gsds tokens.
        /// </summary>
        internal class UPS_Data
        {
            /// <summary>
            /// Gets or sets the list of Domain Data.
            /// </summary>
            internal List<UPS_DomainData> AllDomainData { get; set; } = new List<UPS_DomainData>();

            /// <summary>
            /// Reconstruct the EVG VMin Forwarding able by reading the G.U.S.FAST_STC_V token.
            /// </summary>
            /// <param name="currentFlow">The current/passing flow.</param>
            /// <returns>UPS_Data.</returns>
            internal static UPS_Data BuildFromEvgTokens(int currentFlow)
            {
                var allDataObj = new UPS_Data();
                string fastCorners;
                try
                {
                    fastCorners = Convert.ToString(DDG.Gsds.ReadToken("G.U.S.FAST_STC_V"));
                }
                catch (FatalException e)
                {
                    Prime.Services.ConsoleService.PrintError($"Failed to read G.U.S.FAST_STC_V. Error={e.Message}.");
                    return allDataObj;
                }

                allDataObj.AllDomainData = new List<UPS_DomainData>();

                foreach (var domainData in fastCorners.Split(','))
                {
                    var domainDataObj = new UPS_DomainData();
                    domainDataObj.AllCornerData = new List<UPS_CornerData>();

                    foreach (var cornerDataWithName in domainData.Split('_'))
                    {
                        var splitCornerDataWithName = cornerDataWithName.Split(new[] { '=' }, 2);
                        var domainGroupName = splitCornerDataWithName[0];
                        domainDataObj.DomainName = domainGroupName;

                        var cornerDataWithFreqName = splitCornerDataWithName[1];
                        var splitCornerDataWithFreqName = cornerDataWithFreqName.Split(new[] { ':' }, 2);
                        var cornerFreqName = splitCornerDataWithFreqName[0];
                        var allCornerData = splitCornerDataWithFreqName[1];

                        var cornerDataObj = new UPS_CornerData();
                        cornerDataObj.CurrentFlow = currentFlow;
                        cornerDataObj.FirstFlow = currentFlow;
                        cornerDataObj.AllFlowData = new List<UPS_CornerDataOneFlow>();

                        cornerDataObj.CornerId = 999;
                        cornerDataObj.CornerName = cornerFreqName;

                        var cornerDataForEachFlow = allCornerData.Split('|');
                        for (var flowIndex = 0; flowIndex < cornerDataForEachFlow.Length; flowIndex++)
                        {
                            var cornerData = cornerDataForEachFlow[flowIndex];
                            var obj = new UPS_CornerDataOneFlow();

                            obj.FrequencyInGhz = GetFrequencyForCorner(domainGroupName, cornerFreqName, flowIndex + 1) / 1e9;
                            obj.VminData = string.IsNullOrEmpty(cornerData) ? new List<double>() : cornerData.Split('v').ToList().Select(o => o.ToDouble()).ToList();

                            cornerDataObj.AllFlowData.Add(obj);
                        }

                        domainDataObj.AllCornerData.Add(cornerDataObj);
                    }

                    allDataObj.AllDomainData.Add(domainDataObj);
                }

                return allDataObj;
            }

            /// <summary>
            /// Inserts the data into the UPS structure, either creating a new record or merging with an existing record.
            /// </summary>
            /// <param name="inputValue">Input value to add.</param>
            /// <param name="domainGroupName">The DomainGroup name of the stored data (ie CLR, CR, CRF).</param>
            /// <param name="subDomainIndex">The index into the domain group (ie the core number).</param>
            /// <param name="freqCornerName">The name of the frequency corner (F1, F2, ...).</param>
            /// <param name="freqCornerId">The level_select value for this corner.</param>
            /// <param name="freqValue">The frequency in Ghz of this data.</param>
            /// <param name="flowIndex">The index of the FlowID (not the actual flow number).</param>
            internal void InsertValue(double inputValue, string domainGroupName, int subDomainIndex, string freqCornerName, int freqCornerId, double freqValue, int flowIndex)
            {
                var createNewDomainRecord = false;
                var createNewCornerRecord = false;

                var matchingDomainData = this.AllDomainData.Find(o => o.DomainName == domainGroupName);
                if (matchingDomainData == null)
                {
                    matchingDomainData = new UPS_DomainData();
                    matchingDomainData.AllCornerData = new List<UPS_CornerData>();
                    matchingDomainData.DomainName = domainGroupName;

                    createNewDomainRecord = true;
                }

                var matchingCornerData = matchingDomainData.AllCornerData.Find(o => o.CornerName == freqCornerName);
                if (matchingCornerData == null)
                {
                    matchingCornerData = new UPS_CornerData();
                    matchingCornerData.AllFlowData = Enumerable.Repeat(new UPS_CornerDataOneFlow(), flowIndex + 1).ToList();
                    matchingCornerData.CornerId = freqCornerId;
                    matchingCornerData.CornerName = freqCornerName;
                    matchingCornerData.CurrentFlow = flowIndex + 1;
                    matchingCornerData.FirstFlow = flowIndex + 1;

                    createNewCornerRecord = true;
                }

                if (matchingCornerData.AllFlowData == null)
                {
                    matchingCornerData.AllFlowData = new List<UPS_CornerDataOneFlow>();
                }

                if (createNewCornerRecord || matchingCornerData.AllFlowData.Count <= flowIndex)
                {
                    while (matchingCornerData.AllFlowData.Count <= flowIndex)
                    {
                        matchingCornerData.AllFlowData.Add(new UPS_CornerDataOneFlow());
                    }

                    var matchingFlowData = matchingCornerData.AllFlowData[flowIndex];
                    matchingFlowData.FrequencyInGhz = freqValue;
                    matchingFlowData.VminData = Enumerable.Repeat(-9999d, subDomainIndex + 1).ToList();
                    matchingFlowData.VminData[subDomainIndex] = inputValue;
                }
                else if (matchingCornerData.AllFlowData[flowIndex].VminData.Count <= subDomainIndex)
                {
                    while (matchingCornerData.AllFlowData[flowIndex].VminData.Count <= subDomainIndex)
                    {
                        matchingCornerData.AllFlowData[flowIndex].VminData.Add(-9999d);
                    }

                    matchingCornerData.AllFlowData[flowIndex].VminData[subDomainIndex] = inputValue;
                }
                else if (matchingCornerData.AllFlowData[flowIndex].VminData[subDomainIndex] < inputValue)
                {
                    matchingCornerData.AllFlowData[flowIndex].VminData[subDomainIndex] = inputValue;
                }

                if (createNewCornerRecord)
                {
                    matchingDomainData.AllCornerData.Add(matchingCornerData);
                }

                if (createNewDomainRecord)
                {
                    this.AllDomainData.Add(matchingDomainData);
                }
            }

            /// <summary>
            /// Convert the object into GSDS tokens.
            /// </summary>
            /// <param name="passingFlow">The current/passing flow.</param>
            /// <returns>Dictionary, Keys=Token Names, Values=token values.</returns>
            internal Dictionary<string, string> ToGsdsTokens(int passingFlow)
            {
                var tokens = new Dictionary<string, string>();

                var allVminsByDomainById = new List<string>();
                var allVminsByDomainByName = new List<string>();
                var allVminsByDomainForPassingFlow = new List<string>();
                var allVminsByDomainForFirstFlow = new List<string>();

                foreach (var domainData in this.AllDomainData)
                {
                    var allVminsForOneDomainAllCornersAndFlowsById = new List<string>();
                    var allVminsForOneDomainAllCornersAndFlowsByName = new List<string>();
                    var allVminsForOneDomainAllCornersPassingFlow = new List<string>();
                    var allVminsForOneDomainAllCornersFirstFlow = new List<string>();

                    foreach (var cornerData in domainData.AllCornerData)
                    {
                        var allVminsForOneCornerAllFlows = new List<string>();
                        var measuredFlow = false;

                        for (var flowIndex = 0; flowIndex < cornerData.AllFlowData.Count; flowIndex++)
                        {
                            var flowData = cornerData.AllFlowData[flowIndex];
                            var vminAsString = flowData.GetVminAsString("v");
                            var validVmins = vminAsString != string.Empty;
                            allVminsForOneCornerAllFlows.Add(flowIndex + 1 >= passingFlow
                                ? vminAsString
                                : string.Empty);

                            if ((flowIndex + 1) == passingFlow && validVmins)
                            {
                                allVminsForOneDomainAllCornersPassingFlow.Add($"{flowData.FrequencyInGhz:0.000}^{vminAsString}");
                            }

                            if (!measuredFlow && validVmins)
                            {
                                allVminsForOneDomainAllCornersFirstFlow.Add($"{flowData.FrequencyInGhz:0.000}^{vminAsString}");
                                measuredFlow = true;
                            }
                        }

                        if (allVminsForOneCornerAllFlows.Count > 0)
                        {
                            allVminsForOneDomainAllCornersAndFlowsById.Add($"{domainData.DomainName}={cornerData.CornerId}:{string.Join("|", allVminsForOneCornerAllFlows)}");
                            allVminsForOneDomainAllCornersAndFlowsByName.Add($"{domainData.DomainName}={cornerData.CornerName}:{string.Join("|", allVminsForOneCornerAllFlows)}");
                        }
                    }

                    allVminsByDomainById.Add(string.Join("_", allVminsForOneDomainAllCornersAndFlowsById));
                    allVminsByDomainByName.Add(string.Join("_", allVminsForOneDomainAllCornersAndFlowsByName));

                    if (allVminsForOneDomainAllCornersPassingFlow.Count > 0)
                    {
                        allVminsByDomainForPassingFlow.Add($"{domainData.DomainName}:{string.Join("%", allVminsForOneDomainAllCornersPassingFlow)}");
                    }

                    if (allVminsForOneDomainAllCornersFirstFlow.Count > 0)
                    {
                        allVminsByDomainForFirstFlow.Add($"{domainData.DomainName}:{string.Join("%", allVminsForOneDomainAllCornersFirstFlow)}");
                    }
                }

                tokens["FAST_CORNERS"] = string.Join(",", allVminsByDomainById);
                tokens["FAST_STC_V"] = string.Join(",", allVminsByDomainByName);
                tokens["FAST_UPSVF"] = string.Join("_", allVminsByDomainForFirstFlow);
                tokens["FAST_UPSVFPASSFLOW"] = string.Join("_", allVminsByDomainForPassingFlow);

                return tokens;
            }

            /// <summary>
            /// Structure to hold UPS data for Gsds tokens.
            /// </summary>
            internal class UPS_DomainData
            {
                /// <summary>
                ///  gets or sets the domain name.
                /// </summary>
                internal string DomainName { get; set; }

                /// <summary>
                ///  gets or sets the list of corner data.
                /// </summary>
                internal List<UPS_CornerData> AllCornerData { get; set; }
            }

            /// <summary>
            /// Structure to hold UPS data for Gsds tokens.
            /// </summary>
            internal class UPS_CornerData
            {
                /// <summary>
                /// Gets or sets the CornerID or level_selects value.
                /// </summary>
                internal int CornerId { get; set; }

                /// <summary>
                /// Gets or sets the corner name.
                /// </summary>
                internal string CornerName { get; set; } = string.Empty;

                /// <summary>
                /// Gets or sets the list of data per flow.
                /// </summary>
                internal List<UPS_CornerDataOneFlow> AllFlowData { get; set; } = null;

                /// <summary>
                ///  Gets or sets the Current Flow.
                /// </summary>
                internal int CurrentFlow { get; set; }

                /// <summary>
                /// Gets or sets the First flow with data.
                /// </summary>
                internal int FirstFlow { get; set; }
            }

            /// <summary>
            /// Structure to hold UPS data for Gsds tokens.
            /// </summary>
            internal class UPS_CornerDataOneFlow
            {
                /// <summary>
                /// Gets or sets the frequency (In Ghz).
                /// </summary>
                internal double FrequencyInGhz { get; set; } = 0;

                /// <summary>
                /// Gets or sets list of Vmin Data.
                /// </summary>
                internal List<double> VminData { get; set; } = new List<double>();

                /// <summary>
                /// Joins the VMinData and returns it as a string if any of the data is valid.
                /// </summary>
                /// <param name="delim">Character to use as a separator.</param>
                /// <returns>joined string.</returns>
                internal string GetVminAsString(string delim)
                {
                    // If the frequency is 99Ghz or there is no valid data, return an empty string.
                    // otherwise return all the vmins.
                    if (this.FrequencyInGhz >= 99 || this.VminData.Count(v => v > 0) == 0)
                    {
                        return string.Empty;
                    }

                    return string.Join(delim, this.VminData.Select(vmin => vmin > 0 ? string.Format("{0:0.000}", vmin) : $"{vmin}"));
                }
            }
        }
    }
}
