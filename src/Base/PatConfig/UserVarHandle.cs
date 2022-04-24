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
    /// This class represents handle containing dynamic value of type UserVar.
    /// </summary>
    public class UserVarHandle : IDynamicDataHandle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserVarHandle"/> class.
        /// </summary>
        /// <param name="data">Data to be set on the handler.</param>
        /// <param name="handle">handler to set data.</param>
        /// <param name="subConfigName">a specific configuration element inside the handle to apply data.</param>
        public UserVarHandle(string data, IPatConfigHandle handle, string subConfigName)
        {
            this.SubConfigName = subConfigName;
            var dynamicData = data.Split('.');
            if (dynamicData.Length != 2)
            {
                throw new TestMethodException(
                    "Invalid value on data=[" + data + "], UserVar data source format is 'Collection.UserVarName'.\n");
            }

            this.Collection = dynamicData[0];
            this.DynamicValue = dynamicData[1];

            var userVarExists = Services.UserVarService.Exists(this.Collection, this.DynamicValue);

            if (!userVarExists)
            {
                throw new TestMethodException(
                    "UserVar : Collection=[" + this.Collection + "] VarName=[" + this.DynamicValue + "] doesn't exists.\n");
            }

            this.Handle = handle;
            this.PatConfigHelper = new PatConfigHelper();
        }

        /// <summary>
        /// Gets or Sets SubConfigName name .
        /// </summary>
        public string SubConfigName { get; set; }

        /// <inheritdoc/>
        public IPatConfigHandle Handle { get; }

        /// <inheritdoc/>
        public string DynamicValue { get; }

        /// <summary>
        /// Gets collection name for the user var.
        /// </summary>
        private string Collection { get; }

        /// <summary>
        /// Gets PatConfigHelper to use helper methods.
        /// </summary>
        private PatConfigHelper PatConfigHelper { get; }

        /// <inheritdoc/>
        public void SetData()
        {
            var data = Services.UserVarService.GetStringValue(this.Collection, this.DynamicValue);
            this.PatConfigHelper.SetUpHandlerData(data, this.Handle, this.SubConfigName);
        }
    }
}