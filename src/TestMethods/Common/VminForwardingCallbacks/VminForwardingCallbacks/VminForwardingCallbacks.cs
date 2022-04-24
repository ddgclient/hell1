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

namespace VminForwardingCallbacks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommandLine;
    using DDG;
    using Prime.Base.Exceptions;
    using VminForwardingBase;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    public class VminForwardingCallbacks
    {
        /// <summary>
        /// Stores a snapshot of the current vminforwarding table.
        /// eg. VminSearchStore(G.U.I.DDGVminForwardPassingFlow + CR,CRF,CRX2,CRX3).
        /// </summary>
        /// <param name="args">GSDS containing current flow + Domain groups as a comma sepearated list to store or nothing for all groups.</param>
        public static void VminSearchStore(string args)
        {
            try
            {
                var parserResult = Parser.Default.ParseArguments<SearchStoreOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                parserResult.WithParsed(options =>
                {
                    List<string> domainNames;
                    if (string.IsNullOrWhiteSpace(options.Domains))
                    {
                        Prime.Services.ConsoleService.PrintDebug("VminSearchStore: [Domains] options is empty, getting the full list of all domain names.");
                        domainNames = DDG.VminForwarding.Service.GetAllDomainNames();
                    }
                    else
                    {
                        domainNames = options.Domains.Split(',').Select(o => o.Trim()).ToList();
                    }

                    DDG.VminForwarding.Service.SaveVminForwardingSnapshot(domainNames);
                }).
                    WithNotParsed(e => throw new ArgumentException($"VminSearchStore: failed parsing arguments. {string.Join("\n", e)}"));
            }
            catch (Exception e)
            {
                // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                Prime.Services.ConsoleService.PrintError($"Exception in VminSearchStore - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Perform the vmin interpolation based on the current vminforwarding table and the saved vmin table.
        /// </summary>
        /// <param name="args">Should be ituff if logging is requested, or empty if no logging is required.</param>
        public static void VminInterpolation(string args)
        {
            try
            {
                var parserResult = Parser.Default.ParseArguments<StcInterpolationOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                parserResult.WithParsed(options =>
                {
                    var domainNames = options.Domains.Split(',').Select(o => o.Trim()).ToList();
                    var cornerNames = options.Corners.Split(',').Select(o => o.Trim()).ToList();
                    var flowId = Convert.ToInt32(DDG.Gsds.ReadToken(options.FlowGsds));

                    VminForwardingPrediction.PrimeSTCInterpolation(domainNames, cornerNames, flowId);
                }).
                    WithNotParsed(e => throw new ArgumentException($"VminInterpolation: failed parsing arguments. {string.Join("\n", e)}"));
            }
            catch (Exception e)
            {
                // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                Prime.Services.ConsoleService.PrintError($"Exception in VminInterpolation - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Loads the Vmin Data from DFF into shared storage.
        /// </summary>
        /// <param name="args">none.</param>
        public static void LoadVminFromDFF(string args)
        {
            try
            {
                var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                var cornerMap = DDG.VminForwarding.Service.GetDffTokenMap();
                var tokenMap = MapVminCornerToSharedStorage(cornerMap, console);
                foreach (var item in tokenMap)
                {
                    console?.PrintDebug($"\t{item.Key} => {item.Value}");
                    var dffTokenList = item.Key.Split(':').ToList();
                    string dffValue;
                    switch (dffTokenList.Count)
                    {
                        case 1:
                            console?.PrintDebug($"\t\tDffService.GetDff({dffTokenList[0]})");
                            dffValue = Prime.Services.DffService.GetDff(dffTokenList[0]);
                            break;
                        case 2:
                            console?.PrintDebug($"\t\tDffService.GetDffByOpType({dffTokenList[1]}, {dffTokenList[0]})");
                            dffValue = Prime.Services.DffService.GetDffByOpType(dffTokenList[1], dffTokenList[0]);
                            break;
                        case 3:
                            console?.PrintDebug($"\t\tDffService.GetDff({dffTokenList[2]}, {dffTokenList[1]}, {dffTokenList[0]})");
                            dffValue = Prime.Services.DffService.GetDff(dffTokenList[2], dffTokenList[1], dffTokenList[0]);
                            break;
                        default:
                            throw new TestMethodException($"Bad Format for DFF Token [{item.Key}]. Too many '.' to parse correctly.");
                    }

                    var formattedValue = dffValue.Replace("v", ",");

                    // Prime fails to parse the data if it starts with |
                    if (formattedValue.StartsWith("|"))
                    {
                        formattedValue = "-9999" + formattedValue;
                    }

                    console?.PrintDebug($"\t\tsaving [{formattedValue}] to shared storage.");
                    Prime.Services.SharedStorageService.InsertRowAtTable(item.Value, formattedValue, Prime.SharedStorageService.Context.DUT);
                }
            }
            catch (Exception e)
            {
                // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                Prime.Services.ConsoleService.PrintError($"Exception in LoadVminFromDFF - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        // TODO: this was moved from VminForwardingBase due to changes in Prime5.02 ... would like to move it back...
        private static Dictionary<string, string> MapVminCornerToSharedStorage(Dictionary<string, string> cornerToDffMap, Prime.ConsoleService.IConsoleService console)
        {
            var domains = DDG.VminForwarding.Service.GetAllDomainNames();
            var config = Prime.Services.VminForwardingService.CreateConfigurationHandler();
            var tokenMap = new Dictionary<string, string>();
            var errors = false;

            console?.PrintDebug("Creating DFF Token to SharedStorage Mapping.");
            foreach (var domain in domains)
            {
                var instances = DDG.VminForwarding.Service.GetInstanceNamesForDomain(domain);
                var corners = DDG.VminForwarding.Service.GetCornerNamesForDomainInstance(instances.First());
                foreach (var fullDomainCornerName in corners)
                {
                    var corner = fullDomainCornerName.Split('@').Last();
                    var cornerName = $"{domain}@{corner}";

                    if (!cornerToDffMap.ContainsKey(cornerName))
                    {
                        Prime.Services.ConsoleService.PrintError($"Corner=[{cornerName}] not found in DffMappingFile.");
                        errors = true;
                    }
                    else
                    {
                        var dffToken = cornerToDffMap[cornerName];
                        var sharedStorage = config.GetSharedStorageLimitCheck(cornerName);
                        if (string.IsNullOrWhiteSpace(sharedStorage))
                        {
                            Prime.Services.ConsoleService.PrintError($"Corner=[{cornerName}] does not have a SharedStorageLimitCheck field in the prime vminforwarding configuration file.");
                            errors = true;
                        }
                        else
                        {
                            tokenMap[dffToken] = sharedStorage;
                            console?.PrintDebug($"\t{cornerName}:{dffToken} => {tokenMap[dffToken]}");
                        }
                    }
                }
            }

            if (errors)
            {
                throw new Prime.Base.Exceptions.TestMethodException("Errors mapping VminCorners to DFF/GSDS using ConfigFile.");
            }

            return tokenMap;
        }

        private class SearchStoreOptions
        {
            [Option("domains", Required = false, HelpText = "A comma separated list of domain names. The current VMin data for these domains will be stored.")]
            public string Domains { get; set; }
        }

        private class StcInterpolationOptions
        {
            [Option("domains", Required = true, HelpText = "A comma separated list of domain names to run interpolation on.")]
            public string Domains { get; set; }

            [Option("check_corners", Required = true, HelpText = "A comma separated list of corner names with 'check' data. (these corners will be used to run interpolation on the other corners).")]
            public string Corners { get; set; }

            [Option("flow", Required = true, HelpText = "GSDS token (of the form G.U.I.tokenname) containing the current/passing flow number.")]
            public string FlowGsds { get; set; }
        }
    }
}