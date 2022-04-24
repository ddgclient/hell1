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

namespace FunctionalShopsTC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Prime.FunctionalService;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class FunctionalShopsTC : PrimeFunctionalTestMethod
    {
        private Dictionary<string, Dictionary<string, string>> pinAttributes;
        private Dictionary<string, Dictionary<string, string>> pinAttributesOrig;
        private bool lastPass = true;

        /// <summary>
        /// Enumerate possible test modes.
        /// </summary>
        public enum TestModes
        {
            /// <summary>
            /// Production mode.
            /// </summary>
            Production,

            /// <summary>
            /// Characterization mode.
            /// </summary>
            Characterization,
        }

        /// <summary>
        /// Enumerate possible Vox options.
        /// </summary>
        public enum VOXOptions
        {
            /// <summary>
            /// Voltage Output High.
            /// </summary>
            VOH,

            /// <summary>
            /// Voltage Output Low.
            /// </summary>
            VOL,
        }

        /// <summary>
        /// Gets or Sets the PinConfig file name.
        /// </summary>
        public TestMethodsParams.File PinConfigFile { get; set; }

        /// <summary>
        /// Gets or Sets the Schema file name.
        /// </summary>
        public TestMethodsParams.File SchemaFile { get; set; }

        /// <summary>
        /// Gets or sets test mode. Default mode is 'Production'.
        /// </summary>
        public TestModes TestMode { get; set; } = TestModes.Production;

        /// <summary>
        /// Gets or sets test mode. Default mode is 'Production'.
        /// </summary>
        public VOXOptions VOXOption { get; set; } = VOXOptions.VOL;

        /// <summary>
        /// Gets or Sets PinConfig dictionary.
        /// </summary>
        private PinConfig PinData { get; set; }

        /// <summary>
        /// Gets or Sets PinConfig dictionary.
        /// </summary>
        private List<string> FailedPins { get; set; }

        /// <inheritdoc />
        [Returns(2, PortType.Fail, "FAIL PORT. Fail preamble.")]
        [Returns(1, PortType.Pass, "PASS PORT")]
        [Returns(0, PortType.Fail, "FAIL PORT. Fail pattern.")]
        public override int Execute()
        {
            var port = this.TestMode == TestModes.Production ? this.Production() : this.Characterization();

            return port;
        }

        /// <inheritdoc />
        public override void CustomVerify()
        {
            this.ReadPinConfig();

            // add verify pins exist.
            this.ArePinsValid();
            if (this.TestMode == TestModes.Characterization)
            {
                this.PrepareRequiredPinAttributes();
            }
        }

        /// <summary>
        /// Verify pins are valid dpins.
        /// </summary>
        public void ArePinsValid()
        {
            foreach (var pinConfig in this.PinData.PinConfigs)
            {
                if (!Prime.Services.PinService.Exists(pinConfig.PinName))
                {
                    throw new ArgumentException($"{pinConfig.PinName} is not a valid pin.");
                }
            }
        }

        /// <summary>
        /// Test condtion apply.
        /// </summary>
        public void ApplyTestConditions()
        {
            Prime.Services.ConsoleService.PrintDebug("Start of ApplyTestConditions");
            this.FunctionalTest.ApplyTestConditions();

            // adding a mode (Production/Characterization/leaerningMode, add a accumulator mode in charactrization mode, get some average/sigma for a range of units
            var pinMasks = this.TestMethodExtension.GetDynamicPinMask();

            // Combine user defined mask with instance level mask
            // try a private mmember for MasskPins
            if (this.MaskPins.ToList().Count != 0)
            {
                pinMasks = pinMasks.Union(this.MaskPins.ToList()).ToList();
            }

            this.FunctionalTest.SetPinMask(pinMasks);
        }

        /// <summary>
        /// Prodution code.
        /// </summary>
        /// <returns>port number.</returns>
        public int Production()
        {
            Prime.Services.ConsoleService.PrintDebug("Beginning start of produciton mode...");
            this.PrepareRequiredPinAttributes();
            this.ApplyTestConditions();
            this.StorePinAttributes();
            this.LoadVOX();
            this.ApplyVOX();

            int port;
            this.lastPass = true;

            if (!this.FunctionalTest.Execute())
            {
                this.ProcessFails();
                port = 0;
            }
            else
            {
                Prime.Services.ConsoleService.PrintDebug("Production mode execution passed");
                port = 1;
            }

            this.Log();
            this.RestorePinAttributes();

            // at FailedPins to null at the end of test so it doesn't affect logic for consecutive runs
            this.FailedPins = null;

            // this.TestMethodExtension.PostProcessResults(this.FailData);
            return port;
        }

        /// <summary>
        /// Prodution code.
        /// </summary>
        public void LoadVOX()
        {
            Prime.Services.ConsoleService.PrintDebug("LoadVOX");

            // set starting VOX value for all pins
            foreach (var pinConfig in this.PinData.PinConfigs)
            {
                // updateing both pinAttributes dictionary
                this.pinAttributes[pinConfig.PinName]["VOX"] = pinConfig.VOX.ToString();
            }
        }

        /// <summary>
        /// Prodution code.
        /// </summary>
        public void Log()
        {
            foreach (var pinConfig in this.PinData.PinConfigs)
            {
                Prime.Services.ConsoleService.PrintDebug($"Pin: {pinConfig.PinName}, VOX: {this.pinAttributes[pinConfig.PinName]["VOX"]}");
            }
        }

        /// <summary>
        /// Process list of failing pins.
        /// </summary>
        public void ProcessFails()
        {
            var captureFailureTest = this.FunctionalTest as ICaptureFailureTest;
            if (this.lastPass)
            {
                captureFailureTest.DatalogFailure(0);
            }

            this.FailedPins = captureFailureTest.GetFailingPinNames();
            Prime.Services.ConsoleService.PrintDebug($"Failing pins: {string.Join(" ", this.FailedPins)}");
        }

        /// <summary>
        /// Update initial VOX values for all pins.
        /// </summary>
        public void UpdateVOXInitial()
        {
            Prime.Services.ConsoleService.PrintDebug("This is the first pass, so set the initial VOX to either VOX_LL or VOX_UL");
            Prime.Services.ConsoleService.PrintDebug(this.VOXOption == VOXOptions.VOL
                ? "VOXOptions.VOL, setting VOX_LL"
                : "VOXOptions.VOL, setting VOX_UL");

            var value = this.VOXOption == VOXOptions.VOL ? this.PinData.VOX_LL : this.PinData.VOX_UL;

            // set starting VOX value for all pins
            foreach (var pinConfig in this.PinData.PinConfigs)
            {
                // updating both pinAttributes dictionary & pinConfig object
                pinConfig.VOX = value;
                this.pinAttributes[pinConfig.PinName]["VOX"] = value.ToString();
                Prime.Services.ConsoleService.PrintDebug($"pinConfig.VOX is {value.ToString()}, {pinConfig.PinName} VOX value is {this.pinAttributes[pinConfig.PinName]["VOX"]}");
            }
        }

        /// <summary>
        /// Update initial VOX values for all pins.
        /// </summary>
        /// <param name="vox">new VOX value.</param>
        public void UpdateVOXSubsequent(double vox)
        {
            Prime.Services.ConsoleService.PrintDebug("This is not the first pass, set the VOX to each search point");

            // only update the vox value of failed pins
            foreach (var pin in this.FailedPins)
            {
                var pinConfig = this.PinData.PinConfigs.Find(o => o.PinName == pin);
                if (pinConfig != null)
                {
                    // updating both pinAttributes dictionary & PinData object
                    pinConfig.VOX = vox;
                    this.pinAttributes[pinConfig.PinName]["VOX"] = pinConfig.VOX.ToString();
                    Prime.Services.ConsoleService.PrintDebug($"pinConfig.VOX is {vox.ToString()}, {pinConfig.PinName} VOX value is {this.pinAttributes[pinConfig.PinName]["VOX"]}");
                }
            }
        }

        /// <summary>
        /// Update VOX values for failed pins.
        /// </summary>
        /// <param name="vox">new VOX value.</param>
        public void UpdateVOX(double vox)
        {
            Prime.Services.ConsoleService.PrintDebug("Start of UpdateVOX");

            // if there's no fails, it means this is the first pass, so set the initial VOX to either VOX_LL or VOX_UL
            if (this.FailedPins == null)
            {
                this.UpdateVOXInitial();
            }

            // if there's fails, need to update VOX for next search iteration
            else
            {
                this.UpdateVOXSubsequent(vox);
            }
        }

        /// <summary>
        /// Code to apply current pin attribute values.
        /// </summary>
        public void ApplyVOX()
        {
            Prime.Services.ConsoleService.PrintDebug("Running SetPintAttributes() ...");

            // Should probably only update the pins that failed for TT consideration
            // currently updating pinAttributes for all pins, regardless whether they failed or not in previous execution
            foreach (var pinConfig in this.PinData.PinConfigs)
            {
                Prime.Services.ConsoleService.PrintDebug($"Applying PinAttributes Pin=[{pinConfig.PinName}] Values=[{string.Join(",", this.pinAttributes[pinConfig.PinName].Select(kvp => kvp.Key + ":" + kvp.Value).ToArray())}]");
                Prime.Services.PinService.SetPinAttributeValues(pinConfig.PinName, this.pinAttributes[pinConfig.PinName]);
            }
        }

        /// <summary>
        /// Characterization code. add the production.
        /// </summary>
        public void PrepareRequiredPinAttributes()
        {
            this.pinAttributes = new Dictionary<string, Dictionary<string, string>>();
            this.pinAttributesOrig = new Dictionary<string, Dictionary<string, string>>();
            var attributes = new Dictionary<string, string>
            {
                ["PinModeSel"] = string.Empty,
                ["FixedDriveState"] = string.Empty,
                ["TermMode"] = string.Empty,
                ["TermVRef"] = string.Empty,
                ["VCH"] = string.Empty,
                ["VCL"] = string.Empty,
                ["IOL"] = string.Empty,
                ["IOH"] = string.Empty,
                ["VIH"] = string.Empty,
                ["VIL"] = string.Empty,
                ["VOX"] = string.Empty,
                ["VRef"] = string.Empty,
            };

            foreach (var pinConfig in this.PinData.PinConfigs)
            {
                // need a copy of attributes, not the object, otherwise all pins will have the same "attributes" reference
                this.pinAttributes[pinConfig.PinName] = new Dictionary<string, string>(attributes);
                this.pinAttributesOrig[pinConfig.PinName] = new Dictionary<string, string>(attributes);
            }
        }

        /// <summary>
        /// Store original pin attributes.
        /// </summary>
        public void StorePinAttributes()
        {
            // add decorator later
            Prime.Services.ConsoleService.PrintDebug("Storing original PinAttributes...");

            // store original attributes & set the starting point for pinAttributes
            var testCondition = Prime.Services.TestConditionService.GetTestCondition(this.LevelsTc);
            foreach (var pinConfig in this.PinData.PinConfigs)
            {
                foreach (var attributeName in this.pinAttributes[pinConfig.PinName].Keys.ToList())
                {
                    var attributeValue = string.Empty;
                    try
                    {
                        attributeValue = testCondition.GetPinAttributeValue(pinConfig.PinName, attributeName);
                        this.pinAttributes[pinConfig.PinName][attributeName] = attributeValue;
                        this.pinAttributesOrig[pinConfig.PinName][attributeName] = attributeValue;
                    }
                    catch
                    {
                        Prime.Services.ConsoleService.PrintDebug($"Ignoring pin attribute=[{attributeName}] for pin=[{pinConfig.PinName}]");
                    }

                    if (string.IsNullOrEmpty(attributeValue))
                    {
                        this.pinAttributes[pinConfig.PinName].Remove(attributeName);
                        this.pinAttributesOrig[pinConfig.PinName].Remove(attributeName);
                    }
                }

                Prime.Services.ConsoleService.PrintDebug($"PinAttributes for pin after reading from hardware Pin=[{pinConfig.PinName}] Values=[{string.Join(",", this.pinAttributes[pinConfig.PinName].Select(kvp => kvp.Key + ":" + kvp.Value).ToArray())}]");
            }
        }

        /// <summary>
        /// Store original pin attributes.
        /// </summary>
        public void RestorePinAttributes()
        {
            // add decorator later
            Prime.Services.ConsoleService.PrintDebug("Restoring original PinAttributes...");

            // restore original attributes
            foreach (var pinConfig in this.PinData.PinConfigs)
            {
                Prime.Services.PinService.SetPinAttributeValues(pinConfig.PinName, this.pinAttributesOrig[pinConfig.PinName]);
            }
        }

        /// <summary>
        /// Characterization code.
        /// </summary>
        /// <returns>port number.</returns>
        public int Characterization()
        {
            Prime.Services.ConsoleService.PrintDebug("Start of characterization mode...");

            // may need to re-apply mask every time as it is not sticky
            this.ApplyTestConditions();

            this.StorePinAttributes();

            this.lastPass = false;

            var searchPassed = this.VOXOption == VOXOptions.VOL ? this.SearchVOL() : this.SearchVOH();

            this.Log();

            this.RestorePinAttributes();

            // at FailedPins to null at the end of test so it doesn't affect logic for consecutive runs
            this.FailedPins = null;

            return searchPassed ? 1 : 0;
        }

        /// <summary>
        /// Search for VOL.
        /// </summary>
        /// <returns> return whether seasrch passed. </returns>
        public bool SearchVOL()
        {
            Prime.Services.ConsoleService.PrintDebug("Start of SearchVOL");
            var searchPassed = false;

            // this.pinAttributes[pinConfig.PinName]["VOX"] = vox.ToString(), VOX is going from fail to pass assumption, strobe for L basically
            for (var vox = this.PinData.VOX_LL; vox <= this.PinData.VOX_UL; vox += this.PinData.Resolution)
            {
                if (vox + this.PinData.Resolution > this.PinData.VOX_UL)
                {
                    this.lastPass = true;
                }

                // if method return true, it means all pins passed
                if (this.SearchPoint(vox))
                {
                    searchPassed = true;
                    break;
                }
            }

            return searchPassed;
        }

        /// <summary>
        /// Search for VOH.
        /// </summary>
        /// <returns> return whether seasrch passed. </returns>
        public bool SearchVOH()
        {
            Prime.Services.ConsoleService.PrintDebug("Start of SearchVOH");
            var searchPassed = false;

            // this.pinAttributes[pinConfig.PinName]["VOX"] = vox.ToString(), VOX is going from fail to pass assumption, strobe for L basically
            for (var vox = this.PinData.VOX_UL; vox >= this.PinData.VOX_LL; vox -= this.PinData.Resolution)
            {
                if (vox - this.PinData.Resolution < this.PinData.VOX_UL)
                {
                    this.lastPass = true;
                }

                // if method return true, it means all pins passed
                if (this.SearchPoint(vox))
                {
                    searchPassed = true;
                    break;
                }
            }

            return searchPassed;
        }

        /// <summary>
        /// Search for VOH.
        /// </summary>
        /// <param name="vox">new VOX value.</param>
        /// <returns> return whether seasrch passed. </returns>
        public bool SearchPoint(double vox)
        {
            Prime.Services.ConsoleService.PrintDebug("Start of SearchPoint");

            this.UpdateVOX(vox);
            this.ApplyVOX();

            if (!this.FunctionalTest.Execute())
            {
                this.ProcessFails();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check the existence of PinConfigFile.
        /// </summary>
        public void ReadPinConfig()
        {
            var pinConfigFile = this.GetFileName(this.PinConfigFile);
            this.PinData = JsonConvert.DeserializeObject<PinConfig>(File.ReadAllText(pinConfigFile));
        }

        /// <summary>
        /// Return file name.
        /// </summary>
        /// <param name="filePath">The first name to parse.</param>
        /// <returns>file name.</returns>
        public string GetFileName(string filePath)
        {
            var file = DDG.FileUtilities.GetFile(filePath);
            Prime.Services.ConsoleService.PrintDebug($"file = {Convert.ToString(file)}\n");
            return file;
        }

        /// <summary>
        /// Check the existence of PinConfigFile.
        /// </summary>
        /// <param name="pinConfigFile">The pinConfigFile name to parse.</param>
        /// <param name="jsonSchema"> The JsonSchema name to parse.</param>
        /// <returns>boolean.</returns>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public bool IsValidPinConfig(string pinConfigFile, string jsonSchema)
        {
            // var schemaText = File.ReadAllText(jsonSchema);
            // JSchema schema = JSchema.Parse(schemaText);
            // var pinConfigText = File.ReadAllText(pinConfigFile);
            // JObject jsonFile = JObject.Parse(pinConfigText);
            // return jsonFile.IsValid(schema);
            return true;
        }
    }
}