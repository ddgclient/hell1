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
    using System.IO;
    using System.Linq;
    using Prime;
    using Prime.PhAttributes;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class PllVidWalkTC : TestMethodBase
    {
        /// <summary>Gets or sets the Input file name.</summary>
        public TestMethodsParams.File InputFile { get; set; }

        /// <summary>Gets or sets the PLL name.</summary>
        public TestMethodsParams.String PllName { get; set; }

        /// <summary>Gets or sets the Maximum Voltage.</summary>
        public TestMethodsParams.Double MaxVoltage { get; set; }

        /// <summary>Gets or sets the Minimum Voltage.</summary>
        public TestMethodsParams.Double MinVoltage { get; set; }

        /// <summary>Gets or sets the Default Value if FAST wasn't run.</summary>
        public TestMethodsParams.Double DefaultVoltage { get; set; } = 1.0;

        /// <summary>Gets or sets the Scaling factor for the lower range of valid Ratio.</summary>
        public TestMethodsParams.Double MinRatioMult { get; set; } = 1.00;

        /// <summary>Gets or sets the Scaling factor for the upper range of valid Ratio.</summary>
        public TestMethodsParams.Double MaxRatioMult { get; set; } = 1.08;

        /// <summary>Gets or sets the name of the EVG UF test to use to set the fuses.</summary>
        public TestMethodsParams.String FuseCfgTest { get; set; } = string.Empty;

        private Dictionary<string, ClkPll> PLLs { get; set; }

        private string ModuleName { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            // get the module name from the test instance name, this might include IP information.
            Prime.Services.ConsoleService.PrintDebug($"Parsing module name from instance=[{this.InstanceName}].");
            string[] nameDelim = { "::" };
            var nameLst = new List<string>(this.InstanceName.Split(nameDelim, StringSplitOptions.RemoveEmptyEntries));
            if (nameLst.Count < 2)
            {
                throw new Exception($"Cannot get Module name from InstanceName=[{this.InstanceName}].");
            }

            this.ModuleName = string.Join("::", nameLst.GetRange(0, nameLst.Count - 1));
            Prime.Services.ConsoleService.PrintDebug($"\t[{this.InstanceName}] ModuleName = [{this.ModuleName}].");

            // Read the required arguments
            Prime.Services.ConsoleService.PrintDebug($"Reading the required arguments.");
            if (this.InputFile == null || this.InputFile == string.Empty)
            {
                throw new Exception($"[{this.InstanceName}] InputFile parameter cannot be empty.");
            }

            if (this.PllName == null || this.PllName == string.Empty)
            {
                throw new Exception($"[{this.InstanceName}] PllName parameter cannot be empty.");
            }

            if (this.MinVoltage == null || this.MinVoltage <= 0)
            {
                throw new Exception($"[{this.InstanceName}] MinVoltage parameter must be greater-than 0.");
            }

            if (this.MaxVoltage == null || this.MaxVoltage <= 0)
            {
                throw new Exception($"[{this.InstanceName}] MaxVoltage parameter must be greater-than 0.");
            }

            if (this.FuseCfgTest == string.Empty)
            {
                throw new Exception($"[{this.InstanceName}] FuseCfgTest is a required parameter.");
            }

            if (!Prime.Services.TestProgramService.VerifyTestInstance(this.FuseCfgTest))
            {
                throw new Exception($"[{this.InstanceName}] FuseCfgTest=[{this.FuseCfgTest}] Verify returned false.  It might not be a valid testname.");
            }

            Prime.Services.ConsoleService.PrintDebug($"\t[{this.InstanceName}] InputFile  = [{this.InputFile}].");
            Prime.Services.ConsoleService.PrintDebug($"\t[{this.InstanceName}] PllName    = [{this.PllName}].");
            Prime.Services.ConsoleService.PrintDebug($"\t[{this.InstanceName}] MinVoltage = [{this.MinVoltage}].");
            Prime.Services.ConsoleService.PrintDebug($"\t[{this.InstanceName}] MaxVoltage = [{this.MaxVoltage}].");
            Prime.Services.ConsoleService.PrintDebug($"\t[{this.InstanceName}] FuseCfgTest= [{this.FuseCfgTest}].");

            // Read the other optional arguments.
            Prime.Services.ConsoleService.PrintDebug($"Reading the optional arguments.");
            Prime.Services.ConsoleService.PrintDebug($"\t[{this.InstanceName}] DefaultVoltage = [{this.DefaultVoltage}].");
            Prime.Services.ConsoleService.PrintDebug($"\t[{this.InstanceName}] MinRatioMult   = [{this.MinRatioMult}].");
            Prime.Services.ConsoleService.PrintDebug($"\t[{this.InstanceName}] MaxRatioMult   = [{this.MaxRatioMult}].");

            // Read the input file/create the PLL objects.
            Prime.Services.ConsoleService.PrintDebug($"Parsing input file.");
            if (!this.CreatePllStructs(this.InputFile, this.ModuleName, this.DefaultVoltage, this.FuseCfgTest))
            {
                throw new Exception($"Failed to create PLL Structs from File=[{this.InputFile}].");
            }

            // Now make sure the requested PLL was in the input file.
            if (!this.PLLs.ContainsKey(this.PllName))
            {
                throw new Exception($"[{this.InstanceName}] Unable to find PLL={this.PllName} in input file [{this.InputFile}].");
            }
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        [Returns(2, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            var pll = this.PLLs[this.PllName];

            // clear any previous data.
            pll.Clear();

            // update the voltages based on the parameters. FIXME (this really could be done in Verify).
            pll.UpdateMinMaxVoltage(this.MinVoltage, this.MaxVoltage);
            if (this.LogLevel != PrimeLogLevel.DISABLED)
            {
                pll.PrintDebug();
            }

            var upsRatios = pll.GenerateVmins(this.InstanceName); // 1st we generate the Functional VMIN's according to the VID values
            var exitPort = pll.ExecuteVID(upsRatios.Item1, upsRatios.Item2, this.MinRatioMult, this.MaxRatioMult); // 2nd we execute the PLL's Locktime instances at the correlating VMIN value per ratio
            pll.DatalogResults(exitPort, this.InstanceName); // 3rd we datalog to ituff all pertinent data per ratio, but all on 1 line (the VMIN's we generated, the raw pass/fail, and the locktime from the CMEM decode instance)

            return exitPort;
        }

        private bool CreatePllStructs(string inputFile, string module, double defaultVcc, string fuseCfgTest)
        {
            Prime.Services.ConsoleService.PrintDebug($"CreatePllStructs({inputFile}, {module}, {defaultVcc}) now running.");

            var localFileName = Prime.Services.FileService.GetFile(inputFile);
            if (string.IsNullOrEmpty(localFileName))
            {
                Prime.Services.ConsoleService.PrintError($"Prime.GetFile({inputFile}) returned an empty string.");
                return false;
            }

            this.PLLs = new Dictionary<string, ClkPll>();
            using (StreamReader sr = new StreamReader(localFileName))
            {
                string line;
                int lineNumber = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    lineNumber++;
                    line = line.Trim();

                    // Remove any comments and skip blank lines.
                    if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var fields = line.Split(',').Select(t => t.Trim()).ToList();
                    this.PLLs[fields[0]] = new ClkPll(fields, fuseCfgTest, module, defaultVcc);
                    if (this.LogLevel != PrimeLogLevel.DISABLED)
                    {
                        this.PLLs[fields[0]].PrintDebug();
                    }
                }
            }

            return true;
        }
    }
}