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

namespace VminForwardingBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DDG;
    using Prime.ConsoleService;
    using Prime.VminForwardingService;

    /// <summary>
    /// Main VminFowardingTable class.
    /// </summary>
    // TODO: add the vmin interpolation functions here or somewhere...
    public class VminForwardingTable : IVminForwardingCorner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VminForwardingTable"/> class.
        /// </summary>
        /// <param name="vminCorners">List of Domain Instance @ Corner names (ie CR0@F1, CR1@F3).</param>
        /// <param name="flow">Flow Number.</param>
        /// <inheritdoc cref="VminForwardingTable"/>
        public VminForwardingTable(List<string> vminCorners, int flow)
        {
            this.UpdatePrimeVminConfiguration();
            this.CornerNames = vminCorners;
            this.FlowID = flow;
            this.VminHandler = Prime.Services.VminForwardingService.CreateHandler(this.CornerNames, this.FlowID);
            this.Console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VminForwardingTable"/> class.
        /// </summary>
        /// <param name="vminCorners">String containing comma separated list of Domain Instance @ Corner names (ie CR0@F1, CR1@F3).</param>
        /// <param name="flow">Flow Number.</param>
        /// <inheritdoc cref="VminForwardingTable"/>
        public VminForwardingTable(string vminCorners, int flow)
            : this(vminCorners.Split(',').Select(o => o.Trim()).ToList(), flow)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VminForwardingTable"/> class.
        /// </summary>
        /// <param name="vminDomain">String containing comma seperated list of Domain Instance names.</param>
        /// <param name="frequencyCorner">String containing a single Frequency Corner.</param>
        /// <param name="flow">Flow Number.</param>
        public VminForwardingTable(string vminDomain, string frequencyCorner, int flow)
            : this(vminDomain.Split(',').Select(o => $"{o.Trim()}@{frequencyCorner}").ToList(), flow)
        {
        }

        /// <summary>
        /// Gets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; }

        /// <summary>
        /// Gets or sets the List of Vmin Corners associated with this object.
        /// </summary>
        protected List<string> CornerNames { get; set; }

        /// <summary>
        /// Gets or sets the Flow associated with this object.
        /// </summary>
        protected int FlowID { get; set; }

        /// <summary>
        /// Gets or sets the Vmin corner object from Prime.
        /// </summary>
        protected IVminForwardingHandler VminHandler { get; set; }

        /// <summary>
        /// Gets a value indicating whether Prime VminForwarding UseLimitCheckAsSource is true or false.
        /// </summary>
        protected bool UseLimitCheckAsSource { get; private set; }

        /// <summary>
        /// Gets a value indicating whether Prime VminForwarding UseVoltagesSources is true or false.
        /// </summary>
        protected bool UseVoltagesSources { get; private set; }

        /// <summary>
        /// Gets a value indicating whether Prime VminForwarding UseLimitCheck is true or false.
        /// </summary>
        protected bool UseLimitCheck { get; private set; }

        /// <summary>
        /// Gets a value indicating whether Prime VminForwarding StoreVoltages is true or false.
        /// </summary>
        protected bool StoreVoltages { get; private set; }

        /// <inheritdoc/>
        public double GetStartingVoltage(double startVoltageFromParameter)
        {
            return this.GetStartingVoltage(Enumerable.Repeat(startVoltageFromParameter, this.CornerNames.Count).ToList()).Max();
        }

        /// <inheritdoc/>
        public List<double> GetStartingVoltage(List<double> startVoltagesFromParameter)
        {
            if (startVoltagesFromParameter.Count != this.CornerNames.Count)
            {
                if (startVoltagesFromParameter.Count == 1)
                {
                    var tmp = startVoltagesFromParameter[0];
                    for (var i = 1; i < this.CornerNames.Count; i++)
                    {
                        startVoltagesFromParameter.Add(tmp);
                    }
                }
                else
                {
                    throw new ArgumentException($"Number of voltages [{startVoltagesFromParameter.Count}] does not match the number of Corners [{this.CornerNames.Count}].", nameof(startVoltagesFromParameter));
                }
            }

            return this.VminHandler.GetSourceVoltages(startVoltagesFromParameter).ToList();
        }

        /// <inheritdoc/>
        public bool StoreVminResult(double vmin)
        {
            return this.StoreVminResult(Enumerable.Repeat(vmin, this.CornerNames.Count).ToList());
        }

        /// <inheritdoc/>
        public bool StoreVminResult(List<double> vmins)
        {
            if (!this.StoreVoltages)
            {
                this.Console?.PrintDebug($"DDGVminForwarding.StoreVminResult: StoreVoltages=False ... not storing vmin results for corner=[{string.Join(", ", this.CornerNames)}].");
                return true;
            }

            // verify the correct number of values has been passed.
            if (vmins.Count != this.CornerNames.Count)
            {
                throw new ArgumentException($"Number of vmins [{vmins.Count}] does not match the number of Corners [{this.CornerNames.Count}].", nameof(vmins));
            }

            var result = this.VminHandler.StoreVoltages(vmins);
            if (!result)
            {
                Prime.Services.ConsoleService.PrintError($"DDGVminForwarding.StoreVminResult: Failed to store vmin=[{string.Join(",", vmins)}] for corner=[{string.Join(",", this.CornerNames)}].");
            }

            return result;
        }

        /// <summary>
        /// Set all the Prime VminForwarding flags based on shared storage we saved during VminForwardingBase instance.
        /// Ideally we'd do this once and be done, but it needs to happen after Prime sets up the VminForwarding but
        /// before VminTC builds its IVminForwardingHandler objects and since both of those happen in PrimeInitTestMethod this is
        /// my best solution. This forces the flags to be correct whenever we create an IVminForwardingHandler object.
        /// </summary>
        private void UpdatePrimeVminConfiguration()
        {
            // get all the values from shared storage.
            try
            {
                this.UseLimitCheckAsSource = this.GetPrimeConfigurationFlagFromSharedStorage(DDG.VminForwarding.Globals.VminForwardingUseLimitCheckAsSourceEnable);
                this.UseVoltagesSources = this.GetPrimeConfigurationFlagFromSharedStorage(DDG.VminForwarding.Globals.VminForwardingUseVoltagesSourcesEnable);
                this.UseLimitCheck = this.GetPrimeConfigurationFlagFromSharedStorage(DDG.VminForwarding.Globals.VminForwardingUseLimitCheckEnable);
                this.StoreVoltages = this.GetPrimeConfigurationFlagFromSharedStorage(DDG.VminForwarding.Globals.VminForwardingStoreVoltagesEnable);
            }
            catch (Exception)
            {
                Prime.Services.ConsoleService.PrintError($"Failed to read VminForwarding Flags from SharedStorage. VminForwardingBase probably didn't run.");
                throw;
            }

            // update the operation flags for prime.
            Prime.Services.VminForwardingService.SetOperationModeFlag(OperationMode.UseLimitCheckAsSource, this.UseLimitCheckAsSource);
            Prime.Services.VminForwardingService.SetOperationModeFlag(OperationMode.UseVoltagesSources, this.UseVoltagesSources);
            Prime.Services.VminForwardingService.SetOperationModeFlag(OperationMode.UseLimitCheck, this.UseLimitCheck);
            Prime.Services.VminForwardingService.SetOperationModeFlag(OperationMode.StoreVoltages, this.StoreVoltages);
        }

        private bool GetPrimeConfigurationFlagFromSharedStorage(string flag)
        {
            var intValue = Prime.Services.SharedStorageService.GetIntegerRowFromTable(flag, VminForwarding.Globals.VminForwardingFlagContext);
            return intValue == 1;
        }
    }
}
