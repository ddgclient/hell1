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

namespace Prime.TestMethods.PatConfig
{
    using System;
    using System.Collections.Generic;
    using Prime.Base.Exceptions;
    using Prime.PatConfigService;

    /// <summary>
    /// This class contains the logic on how to set and apply data on PatConfig handles.
    /// </summary>
    public class PatConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatConfig"/> class.
        /// </summary>
        public PatConfig()
        {
            this.DynamicDataToApply = new List<IDynamicDataHandle>();
            this.Handles = new List<IPatConfigHandle>();
            this.PatConfigHelper = new PatConfigHelper();
            this.HandlesByName = new Dictionary<string, IPatConfigHandle>();
        }

        /// <summary>
        /// Enumerated type for selecting data source.
        /// </summary>
        public enum DataSource
        {
            /// <summary>
            /// Raw data source.
            /// </summary>
            Raw,

            /// <summary>
            /// UserVar data source.
            /// </summary>
            UserVar,

            /// <summary>
            /// SharedStorage data source.
            /// </summary>
            SharedStorage,

            /// <summary>
            /// SharedStorage data source.
            /// </summary>
            DFF,
        }

        /// <summary>
        /// Gets handles that needs to set data during execute.
        /// </summary>
        private List<IDynamicDataHandle> DynamicDataToApply { get; }

        /// <summary>
        /// Gets handles list to apply data.
        /// </summary>
        private List<IPatConfigHandle> Handles { get; }

        /// <summary>
        /// Gets or Sets HandlesByName.
        /// </summary>
        private Dictionary<string, IPatConfigHandle> HandlesByName { get; set; }

        /// <summary>
        /// Gets Handles list.
        /// </summary>
        private PatConfigHelper PatConfigHelper { get; }

        /// <summary>
        /// Gets handle and add to list checking if needs to use plist or not.
        /// </summary>
        /// <param name="configuration">Configuration containing name of the handle to get.</param>
        /// <param name="patList">Pattern list name used to get handle.</param>
        /// <param name="regEx"> Regular expression to reduce patterns to apply data on.</param>
        public void AddHandler(
            PatConfigJsonFile.SetPoint.Configuration configuration,
            string patList = "",
            string regEx = "")
        {
            AdvanceGetHandleOptions advanceOptions = new AdvanceGetHandleOptions();
            advanceOptions.InPKGApplyToPKGOnly = configuration.InPkgApplyToPkgOnly;
            advanceOptions.ToBeStored = configuration.ToBeStored;
            IPatConfigHandle currentHandle = this.GetPatConfigHandle(configuration.Name, patList, regEx, advanceOptions);

            if (configuration.Source == DataSource.Raw)
            {
                if (string.IsNullOrWhiteSpace(configuration.Data))
                {
                    if (!currentHandle.IsDataSet())
                    {
                        throw new TestMethodException("Configuration=[" + configuration.Name + "] does not contain any default data, please set data on ConfigurationFile.\n");
                    }
                }
                else
                {
                    this.PatConfigHelper.SetUpHandlerData(configuration.Data, currentHandle, configuration.SubConfigElement);
                }

                this.Handles.Add(currentHandle);
            }
            else
            {
                var handle = this.BuildDynamicHandle(configuration, currentHandle);
                this.Handles.Add(currentHandle);
                this.DynamicDataToApply.Add(handle);
            }
        }

        /// <summary>
        /// Applies handles checking if there is handles with dynamic data that needs to be set.
        /// </summary>
        /// <returns>True if all handles were applied correctly, else return false.</returns>
        public bool ApplyHandles()
        {
            try
            {
                this.SetDataToHandlesWithDynamicSource();
                Services.PatConfigService.Apply(this.Handles);
                return true;
            }
            catch (FatalException e)
            {
                Services.ConsoleService.PrintError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Clears lists of handles for prevent cumulative handles each time verify is run.
        /// </summary>
        public void ClearHandles()
        {
            this.HandlesByName.Clear();
            this.Handles.Clear();
            this.DynamicDataToApply.Clear();
        }

        /// <summary>
        /// Gets handle checking if needs to use plist or not.
        /// </summary>
        /// <returns>IPatConfigHandle to be added to list.</returns>
        private IPatConfigHandle GetPatConfigHandle(string handleName, string patList, string regEx, AdvanceGetHandleOptions advanceOptions)
        {
            if (!this.HandlesByName.ContainsKey(handleName))
            {
                IPatConfigHandle currentHandle =
                    Services.PatConfigService.GetPatConfigHandleWithPlist(handleName, patList, regEx, advanceOptions);
                this.HandlesByName.Add(handleName, currentHandle);
                return currentHandle;
            }

            return this.HandlesByName[handleName];
        }

        /// <summary>
        /// Constructs handle with dynamic data depending on Data source.
        /// </summary>
        /// <returns>Handle object with dynamic data.</returns>
        private IDynamicDataHandle BuildDynamicHandle(PatConfigJsonFile.SetPoint.Configuration configuration, IPatConfigHandle currentHandle)
        {
            IDynamicDataHandle handle;
            switch (configuration.Source)
            {
                case DataSource.UserVar:
                    handle = new UserVarHandle(configuration.Data, currentHandle, configuration.SubConfigElement);
                    break;
                case DataSource.SharedStorage:
                    handle = new SharedStorageHandle(configuration.Data, currentHandle, configuration.SubConfigElement);
                    break;
                case DataSource.DFF:
                    handle = new DffHandle(configuration.Data, currentHandle, configuration.SubConfigElement);
                    break;
                default:
                    throw new TestMethodException("Undefined Source Type\n");
            }

            return handle;
        }

        /// <summary>
        /// Iterates over handles with dynamic data and sets data on each one.
        /// </summary>
        private void SetDataToHandlesWithDynamicSource()
        {
            foreach (var handleToApply in this.DynamicDataToApply)
            {
                handleToApply.SetData();
            }
        }
    }
}
