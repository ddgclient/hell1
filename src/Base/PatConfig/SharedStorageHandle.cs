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
    using Prime.Base.Exceptions;
    using Prime.PatConfigService;

    /// <summary>
    /// This class represents handle containing dynamic value of type SharedStorage.
    /// </summary>
    public class SharedStorageHandle : IDynamicDataHandle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SharedStorageHandle"/> class.
        /// </summary>
        /// <param name="data">Data to be set on the handler.</param>
        /// <param name="handle">handler to set data.</param>
        /// <param name="subConfigName">a specific configuration element inside the handle to apply data.</param>
        public SharedStorageHandle(string data, IPatConfigHandle handle, string subConfigName)
        {
            this.SubConfigName = subConfigName;
            var dynamicData = data.Split('.');
            if (dynamicData.Length != 2)
            {
                throw new TestMethodException(
                    "Invalid value on data=[" + data + "], SharedStorage data source format is 'Context.SharedStorageString'.\n");
            }

            this.Handle = handle;
            switch (dynamicData[0])
            {
                case "DUT":
                    this.Context = SharedStorageService.Context.DUT;
                    break;
                case "IP":
                    this.Context = SharedStorageService.Context.IP;
                    break;
                case "LOT":
                    this.Context = SharedStorageService.Context.LOT;
                    break;
                default:
                    throw new TestMethodException(
                        "Invalid value on data=[" + data + "], SharedStorage context must be valid value (DUT,IP,LOT).\n");
            }

            this.DynamicValue = dynamicData[1];
            this.PatConfigHelper = new PatConfigHelper();
        }

        /// <summary>
        /// Gets or Sets SubConfigName.
        /// </summary>
        public string SubConfigName { get; set; }

        /// <inheritdoc/>
        public IPatConfigHandle Handle { get; private set; }

        /// <inheritdoc/>
        public string DynamicValue { get; private set; }

        /// <summary>
        /// Gets context name for the shared storage.
        /// </summary>
        private SharedStorageService.Context Context { get; }

        /// <summary>
        /// Gets PatConfigHelper to use helper methods.
        /// </summary>
        private PatConfigHelper PatConfigHelper { get; }

        /// <inheritdoc/>
        public void SetData()
        {
            var data = Services.SharedStorageService.GetStringRowFromTable(this.DynamicValue, this.Context);
            this.PatConfigHelper.SetUpHandlerData(data, this.Handle, this.SubConfigName);
        }
    }
}
