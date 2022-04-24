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

namespace DieRecoveryCallbacks
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CommandLine;
    using DDG;
    using Newtonsoft.Json;
    using Prime.Base.Exceptions;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    public class DieRecoveryCallbacks
    {
        /// <summary>
        /// Callback function to Configure an IP for Recovery (wrapper around ConfigureIpForRecovery).
        /// </summary>
        /// <param name="args">Argument String.</param>
        public static void DisableIP(string args)
        {
            ConfigureIpForRecovery(args);
        }

        /// <summary>
        /// Callback function to Configure an IP for Recovery.
        /// </summary>
        /// <param name="args">Argument String.</param>
        public static void ConfigureIpForRecovery(string args)
        {
            try
            {
                var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                var parserResult = Parser.Default.ParseArguments<ConfigureOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                parserResult.WithParsed(options =>
                {
                    if (string.IsNullOrWhiteSpace(options.Tracker) && string.IsNullOrWhiteSpace(options.Value))
                    {
                        throw new ArgumentException("One of [--value, --tracker] is required for ConfigureIpForRecovery.");
                    }

                    BitArray maskBits = null;
                    if (!string.IsNullOrWhiteSpace(options.Value))
                    {
                        maskBits = options.Value.ToBitArray();
                    }

                    if (!string.IsNullOrWhiteSpace(options.Tracker))
                    {
                        console?.PrintDebug($"[{instanceName}] Reading value from Tracker=[{options.Tracker}].");
                        var recovery = DDG.DieRecovery.Service.Get(options.Tracker);
                        var trackerBits = recovery.GetMaskBits();
                        maskBits = maskBits == null ? trackerBits : maskBits.Or(trackerBits);
                    }

                    if (string.IsNullOrWhiteSpace(options.PatList) || options.PatList.ToUpper() == "LOCAL")
                    {
                        options.PatList = string.Join(",", Prime.Services.TestProgramService.GetCurrentPatternLists());
                    }

                    console?.PrintDebug($"[{instanceName}] PinMap=[{options.PinMap}].");
                    console?.PrintDebug($"[{instanceName}] Value=[{options.Value}].");
                    console?.PrintDebug($"[{instanceName}] PatList=[{options.PatList}].");

                    var pinMap = DDG.PinMap.Service.Get(options.PinMap);
                    if (options.PatList.ToUpper() == "GLOBAL" || string.IsNullOrEmpty(options.PatList))
                    {
                        pinMap.ApplyPatConfig(maskBits);
                    }
                    else
                    {
                        foreach (var patlist in options.PatList.Split(','))
                        {
                            pinMap.ApplyPatConfig(maskBits, patlist);
                        }
                    }
                }).
                    WithNotParsed(e => throw new ArgumentException($"DieRecoveryCallbacks: failed parsing arguments. {string.Join("\n", e)}"));
            }
            catch (Exception e)
            {
                // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                Prime.Services.ConsoleService.PrintError($"Exception in ConfigureIpForRecovery - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Callback function to Mask an IP for Recovery.
        /// </summary>
        /// <param name="args">Argument String.</param>
        /// <returns>String containing the comma separated pins to mask.</returns>
        public static string MaskIP(string args)
        {
            try
            {
                var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                var parserResult = Parser.Default.ParseArguments<MaskOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                var pinsToMaskRetVal = string.Empty;

                parserResult.WithParsed(options =>
                {
                    if (string.IsNullOrWhiteSpace(options.Tracker) && string.IsNullOrWhiteSpace(options.Value))
                    {
                        throw new ArgumentException("One of [--value, --tracker] is required for MaskIP.");
                    }

                    BitArray maskBits = null;
                    if (!string.IsNullOrWhiteSpace(options.Value))
                    {
                        maskBits = options.Value.ToBitArray();
                    }

                    if (!string.IsNullOrWhiteSpace(options.Tracker))
                    {
                        console?.PrintDebug($"[{instanceName}] Reading value from Tracker=[{options.Tracker}].");
                        var recovery = DDG.DieRecovery.Service.Get(options.Tracker);
                        var trackerBits = recovery.GetMaskBits();
                        maskBits = maskBits == null ? trackerBits : maskBits.Or(trackerBits);
                    }

                    console?.PrintDebug($"[{instanceName}] PinMap=[{options.PinMap}].");
                    console?.PrintDebug($"[{instanceName}] Value=[{maskBits.ToBinaryString()}].");

                    List<string> additionalPins = null;
                    if (!string.IsNullOrWhiteSpace(options.MaskPins))
                    {
                        additionalPins = options.MaskPins.Split(',').ToList();
                    }

                    var pinMap = DDG.PinMap.Service.Get(options.PinMap);
                    var pinsToMask = pinMap.GetMaskPins(maskBits, additionalPins);
                    pinsToMaskRetVal = string.Join(",", pinsToMask);
                    console?.PrintDebug($"[{instanceName}] PinMap Returned PinsToMask=[{pinsToMaskRetVal}].");

                    if (!string.IsNullOrWhiteSpace(options.GsdsToken))
                    {
                        DDG.Gsds.WriteToken(options.GsdsToken, pinsToMaskRetVal);
                    }
                }).
                    WithNotParsed(e => throw new ArgumentException($"MaskIP: failed parsing arguments. {string.Join("\n", e)}"));
                return pinsToMaskRetVal;
            }
            catch (Exception e)
            {
                // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                Prime.Services.ConsoleService.PrintError($"Exception in MaskIP - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Callback function for writing a value to a tracker.
        /// </summary>
        /// <param name="args">argument string.</param>
        public static void WriteTracker(string args)
        {
            try
            {
                var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                var parserResult = Parser.Default.ParseArguments<WriteTrackerOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                parserResult.WithParsed(options =>
                {
                    var recoveryTracker = DDG.DieRecovery.Service.Get(options.Tracker);

                    if (!string.IsNullOrWhiteSpace(options.GsdsToken))
                    {
                        console?.PrintDebug($"[{instanceName}] Getting Value from GsdsToken=[{options.GsdsToken}].");
                        options.Value = Convert.ToString(DDG.Gsds.ReadToken(options.GsdsToken));
                    }
                    else if (!string.IsNullOrWhiteSpace(options.SrcTracker))
                    {
                        console?.PrintDebug($"[{instanceName}] Getting Value from Tracker=[{options.SrcTracker}].");
                        options.Value = DDG.DieRecovery.Service.Get(options.SrcTracker).GetMaskBits().ToBinaryString();
                    }
                    else if (!string.IsNullOrWhiteSpace(options.UserVar))
                    {
                        console?.PrintDebug($"[{instanceName}] Getting Value from UserVar=[{options.UserVar}].");
                        options.Value = Prime.Services.UserVarService.GetStringValue(options.UserVar);
                    }
                    else if (!string.IsNullOrWhiteSpace(options.DffToken))
                    {
                        console?.PrintDebug($"[{instanceName}] Getting Value from DffToken=[{options.DffToken}].");
                        var numDots = options.DffToken.Count(c => c == ':');
                        if (numDots < 1)
                        {
                            throw new ArgumentException($"WriteTracker callback is expecting DFF Token to be [<dieid>:<optype>:<variable>] or [<optype>:<variable>], not [{options.DffToken}].");
                        }

                        if (numDots == 1)
                        {
                            var dff = options.DffToken.Split(new[] { ':' }, 2);
                            options.Value = Prime.Services.DffService.GetDffByOpType(dff[1], dff[0]);
                        }
                        else
                        {
                            var dff = options.DffToken.Split(new[] { ':' }, 3);
                            options.Value = Prime.Services.DffService.GetDff(dff[2], dff[1], dff[0]);
                        }
                    }
                    else if (options.Reset)
                    {
                        options.Value = recoveryTracker.ResetValue;
                    }
                    else if (string.IsNullOrWhiteSpace(options.Value))
                    {
                        throw new ArgumentException("One of [--value, --gsds, --uservar, --dff, --reset] is required for WriteTracker.");
                    }

                    var updateMode = options.MergeMode ? UpdateMode.Merge : UpdateMode.OverWrite;

                    console?.PrintDebug($"[{instanceName}] Tracker=[{options.Tracker}].");
                    console?.PrintDebug($"[{instanceName}] Value=[{options.Value}].");
                    console?.PrintDebug($"[{instanceName}] UpdateMode=[{updateMode}].");

                    if (!options.Value.All(bit => bit == '0' || bit == '1'))
                    {
                        throw new ArgumentException($"Value=[{options.Value}] is not a valid binary string for WriteTracker.");
                    }

                    // update the tracker.
                    if (!recoveryTracker.UpdateTrackingStructure(options.Value.ToBitArray(), mode: updateMode, log: !options.NoPrintToItuff))
                    {
                        throw new TestMethodException($"DieRecovery.UpdateTrackingStructure returned fail for Tracker=[{options.Tracker}] Data=[{options.Value}].");
                    }
                }).
                    WithNotParsed(e => throw new ArgumentException($"WriteTracker: failed parsing arguments. {string.Join("\n", e)}"));
            }
            catch (Exception e)
            {
                // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                Prime.Services.ConsoleService.PrintError($"Exception in WriteTracker - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Callback function for writing a value to a tracker.
        /// </summary>
        /// <param name="args">String argument.</param>
        public static void CopyTracker(string args)
        {
            try
            {
                var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                var parserResult = Parser.Default.ParseArguments<CopyTrackerOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                parserResult.WithParsed(options =>
                {
                    console?.PrintDebug($"[{instanceName}] Tracker=[{options.Tracker}].");
                    if (string.IsNullOrWhiteSpace(options.GsdsToken) && string.IsNullOrWhiteSpace(options.UserVar) && string.IsNullOrWhiteSpace(options.DffToken))
                    {
                        throw new ArgumentException("At least one of [GsdsToken, UserVar, DffToken] is required for the CopyTracker callback.");
                    }

                    var recoveryTracker = DDG.DieRecovery.Service.Get(options.Tracker);
                    var value = recoveryTracker.GetMaskBits().ToBinaryString();

                    if (!string.IsNullOrWhiteSpace(options.GsdsToken))
                    {
                        console?.PrintDebug($"[{instanceName}] Writing Value=[{value}] to GSDS=[{options.GsdsToken}].");
                        DDG.Gsds.WriteToken(options.GsdsToken, value);
                    }

                    if (!string.IsNullOrWhiteSpace(options.UserVar))
                    {
                        console?.PrintDebug($"[{instanceName}] Writing Value=[{value}] to UserVar=[{options.UserVar}].");
                        Prime.Services.UserVarService.SetValue(options.UserVar, value);
                    }

                    if (!string.IsNullOrWhiteSpace(options.DffToken))
                    {
                        console?.PrintDebug($"[{instanceName}] Writing Value=[{value}] to DFF=[{options.DffToken}].");
                        if (options.DffToken.Contains(":"))
                        {
                            var dffTokenParts = options.DffToken.Split(new[] { ':' }, 2);
                            Prime.Services.DffService.SetDff(dffTokenParts[1], value, dffTokenParts[0]);
                        }
                        else
                        {
                            Prime.Services.DffService.SetDff(options.DffToken, value);
                        }
                    }
                }).
                    WithNotParsed(e => throw new ArgumentException($"CopyTracker: failed parsing arguments. {string.Join("\n", e)}"));
            }
            catch (Exception e)
            {
                // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                Prime.Services.ConsoleService.PrintError($"Exception in CopyTracker - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Clones an existing tracker definition and data into a new tracker.
        /// </summary>
        /// <param name="args">Arguments.</param>
        public static void CloneTracker(string args)
        {
            try
            {
                var parserResult = Parser.Default.ParseArguments<CloneTrackerOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                parserResult.WithParsed(options =>
                    {
                        var existingTrackers = options.ExistingTracker.Split(',');
                        var newTrackers = options.NewTracker.Split(',');
                        if (existingTrackers.Length != newTrackers.Length)
                        {
                            throw new ArgumentException($"CloneTracker: number of existing and new trackers must much.", args);
                        }

                        for (int i = 0; i < existingTrackers.Length; i++)
                        {
                            DDG.DieRecovery.Service.CloneTracker(existingTrackers[i], newTrackers[i]);
                        }
                    }).
                    WithNotParsed(e => throw new ArgumentException($"CloneTracker: failed parsing arguments. {string.Join("\n", e)}", args));
            }
            catch (Exception e)
            {
                // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                Prime.Services.ConsoleService.PrintError($"Exception in CloneTracker - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Callback function to load a file containing one or more pinmaps.
        /// Argument format is LoadPinMap( PinMapDecoderType, FileToLoad ).
        /// </summary>
        /// <param name="args">Arguments, format is "PinMapDecoderType, FileToLoad".</param>
        public static void LoadPinMapFile(string args)
        {
            try
            {
                var console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
                var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                var parserResult = Parser.Default.ParseArguments<LoadPinMapOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                parserResult.WithParsed(options =>
                {
                    console?.PrintDebug($"[{instanceName}] LoadPinMap Type=[{options.DecoderType}] File=[{options.FileToLoad}].");

                    var baseType = typeof(DieRecoveryBase.PinMapDecoderBase);
                    var decoderBaseClass = baseType.Name;
                    var decoderBaseNamespace = baseType.FullName.Split('.').First();
                    console?.PrintDebug($"[{instanceName}] LoadPinMap using BaseNamespace=[{decoderBaseNamespace}] and BaseType=[{decoderBaseClass}].");

                    var asm = baseType.Assembly;
                    var pinMapType = asm.GetType(decoderBaseNamespace + "." + options.DecoderType);

                    if (pinMapType == null)
                    {
                        var validTypes = asm.GetTypes()
                            .Where(t => baseType.IsAssignableFrom(t) && t.IsClass && t != baseType)
                            .Select(n => n.ToString().Replace(decoderBaseNamespace + ".", string.Empty))
                            .ToList();
                        validTypes.Sort();

                        throw new TestMethodException($"Invalid Decoder=[{options.DecoderType}]. Valid Decoders are [{string.Join(", ", validTypes)}].");
                    }

                    var pinMapListType = typeof(IEnumerable<>).MakeGenericType(pinMapType);

                    var fileContents = File.ReadAllText(DDG.FileUtilities.GetFile(options.FileToLoad));

                    IEnumerable<DieRecoveryBase.PinMapDecoderBase> decoders;
                    try
                    {
                        decoders = (IEnumerable<DieRecoveryBase.PinMapDecoderBase>)JsonConvert.DeserializeObject(fileContents, pinMapListType);
                    }
                    catch (JsonException e)
                    {
                        Prime.Services.ConsoleService.PrintError($"[{instanceName}] Json error while reading file=[{options.FileToLoad}] Error={e.Message}");
                        throw;
                    }

                    foreach (var decoder in decoders)
                    {
                        console?.PrintDebug($"Saving PinMap=[{decoder.Name}].");
                        DDG.DieRecovery.Utilities.StorePinMapDecoder(decoder);
                    }
                }).
                    WithNotParsed(e => throw new ArgumentException($"LoadPinMapFile: failed parsing arguments. {string.Join("\n", e)}"));
            }
            catch (Exception e)
            {
                // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                Prime.Services.ConsoleService.PrintError($"Exception in LoadPinMapFile - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Callback function to Run a DieRecovery Rule.
        /// </summary>
        /// <param name="args">Arguments, format is tbd.</param>
        public static void RunRule(string args)
        {
            try
            {
                var instanceName = Prime.Services.TestProgramService.GetCurrentTestInstanceName();
                var parserResult = Parser.Default.ParseArguments<RunRulesOptions>(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                parserResult.WithParsed(options =>
                {
                    Prime.Services.ConsoleService.PrintDebug($"[{instanceName}] Tracker=[{options.Tracker}] Rule=[{options.Rule}] GsdsForResult=[{options.GsdsForResult}] ValueForResult=[{options.ValueToStore}]");
                    var recoveryTracker = DDG.DieRecovery.Service.Get(options.Tracker);
                    var rslt = recoveryTracker.RunRule(options.Rule);
                    if (rslt.Count == 0 && options.ValueToStore.ToUpper() == "SIZE")
                    {
                        DDG.Gsds.WriteToken(options.GsdsForResult, "0");
                    }
                    else if (rslt.Count == 0)
                    {
                        DDG.Gsds.WriteToken(options.GsdsForResult, string.Empty);
                    }
                    else if (options.ValueToStore.ToUpper() == "NAME")
                    {
                        DDG.Gsds.WriteToken(options.GsdsForResult, rslt[0].Name);
                    }
                    else if (options.ValueToStore.ToUpper() == "SIZE")
                    {
                        DDG.Gsds.WriteToken(options.GsdsForResult, rslt[0].Size.ToString());
                    }
                    else if (options.ValueToStore.ToUpper() == "BITVECTOR")
                    {
                        DDG.Gsds.WriteToken(options.GsdsForResult, rslt[0].BitVector);
                    }
                    else
                    {
                        throw new ArgumentException($"RunRule: Invalid store_value option [{options.ValueToStore.ToUpper()}]. Expecting NAME, SIZE or BITVECTOR.");
                    }
                }).
                    WithNotParsed(e => throw new ArgumentException($"RunRule: failed parsing arguments. {string.Join("\n", e)}"));
            }
            catch (Exception e)
            {
                // Uncaught exceptions don't get printed correctly when called from EVG code, so catch everything and print it here.
                Prime.Services.ConsoleService.PrintError($"Exception in RunRule - [{e.GetType()}] {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        private class ConfigureOptions
        {
            [Option("pinmap", Required = true, HelpText = "The DieRecovery PinMap name to use.")]
            public string PinMap { get; set; }

            [Option("tracker", Required = false, HelpText = "The DieRecovery Tracker holding the data to use.")]
            public string Tracker { get; set; } = string.Empty;

            [Option("value", Required = false, HelpText = "The BitMask data to use instead of the tracker data.")]
            public string Value { get; set; } = string.Empty;

            [Option("patlist", Required = false, HelpText = "The PatternList to modify (default will modify all plists).")]
            public string PatList { get; set; } = string.Empty;
        }

        private class MaskOptions
        {
            [Option("pinmap", Required = true, HelpText = "The DieRecovery PinMap name to use.")]
            public string PinMap { get; set; }

            [Option("tracker", Required = false, HelpText = "The DieRecovery Tracker holding the data to use.")]
            public string Tracker { get; set; } = string.Empty;

            [Option("value", Required = false, HelpText = "The BitMask data to use instead of the tracker data.")]
            public string Value { get; set; } = string.Empty;

            [Option("gsds", Required = false, HelpText = "The pins to mask will be writing to this token as a comma separated string. (of the form G.U.S.TokenName).")]
            public string GsdsToken { get; set; } = string.Empty;

            [Option("maskpins", Required = false, HelpText = "Additional mask pins.")]
            public string MaskPins { get; set; } = string.Empty;
        }

        private class WriteTrackerOptions
        {
            [Option("tracker", Required = true, HelpText = "The DieRecovery Tracker to write data to.")]
            public string Tracker { get; set; } = string.Empty;

            [Option("gsds", Required = false, HelpText = "The GSDS token to get the write data from (of the form G.U.S.TokenName).")]
            public string GsdsToken { get; set; } = string.Empty;

            [Option("dff", Required = false, HelpText = "The DFF token to get the write data from (of the form OpType.DffTokenName or DieID.OpType.DffTokenName).")]
            public string DffToken { get; set; } = string.Empty;

            [Option("uservar", Required = false, HelpText = "The Hdmt UserVariable to get the write data from (of the form Collection.UserVar).")]
            public string UserVar { get; set; } = string.Empty;

            [Option("value", Required = false, HelpText = "The raw binary data to write.")]
            public string Value { get; set; } = string.Empty;

            [Option("src_tracker", Required = false, HelpText = "A tracker to get the source data from.")]
            public string SrcTracker { get; set; } = string.Empty;

            [Option("reset", Required = false, HelpText = "Writes the Reset/Initial value to the Tracker.")]
            public bool Reset { get; set; } = false;

            [Option("merge", Required = false, HelpText = "Merge the data with the tracker (using bitwise-Or) instead of overwriting the data.")]
            public bool MergeMode { get; set; } = false;

            [Option("noprint", Required = false, HelpText = "Will not print the Tracker upate to ituff.")]
            public bool NoPrintToItuff { get; set; } = false;
        }

        private class CopyTrackerOptions
        {
            [Option("tracker", Required = true, HelpText = "The DieRecovery Tracker to get the data from.")]
            public string Tracker { get; set; } = string.Empty;

            [Option("gsds", Required = false, HelpText = "The GSDS token to write data to (of the form G.U.S.TokenName).")]
            public string GsdsToken { get; set; } = string.Empty;

            [Option("dff", Required = false, HelpText = "The DFF token to write data (of the form DffTokenName or DieID:DffTokenName).")]
            public string DffToken { get; set; } = string.Empty;

            [Option("uservar", Required = false, HelpText = "The Hdmt UserVariable to write data to (of the form Collection.UserVar).")]
            public string UserVar { get; set; } = string.Empty;
        }

        private class CloneTrackerOptions
        {
            [Option("existing_tracker", Required = true, HelpText = "The DieRecovery Tracker to to be cloned.")]
            public string ExistingTracker { get; set; } = string.Empty;

            [Option("new_tracker", Required = true, HelpText = "The NEW DieRecovery Tracker name.")]
            public string NewTracker { get; set; } = string.Empty;
        }

        private class LoadPinMapOptions
        {
            [Option("decoder", Required = true, HelpText = "The PinMapDecoder type to load.")]
            public string DecoderType { get; set; }

            [Option("file", Required = true, HelpText = "The JSON file to load.")]
            public string FileToLoad { get; set; }
        }

        private class RunRulesOptions
        {
            [Option("tracker", Required = true, HelpText = "The DieRecovery Tracker to use.")]
            public string Tracker { get; set; }

            [Option("rule", Required = true, HelpText = "The DieRecovery Rule to execute.")]
            public string Rule { get; set; }

            [Option("gsds", Required = true, HelpText = "The GSDS to store the result in. Of the form G.[UL].[SI].TokenName.")]
            public string GsdsForResult { get; set; }

            [Option("store_value", Required = false, HelpText = "The value to store. Either NAME (stores the name of the first passing rule), SIZE (stores the size of the first passing rule), or BITVECTOR (stores the full bitvector of the first passing rule).")]
            public string ValueToStore { get; set; } = "NAME";
        }
    }
}
