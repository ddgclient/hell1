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
    using Prime.Base.Exceptions;
    using Prime.PatConfigService;

    /// <summary>
    /// This class represents handle containing dynamic value of type DFF.
    /// </summary>
    public class DffHandle : IDynamicDataHandle
    {
        private const string CURRENTVALUE = "CURRENT";

        /// <summary>
        /// Initializes a new instance of the <see cref="DffHandle"/> class.
        /// </summary>
        /// <param name="data">Data to be set on the handler.</param>
        /// <param name="handle">handler to set data.</param>
        /// <param name="subConfigName">a specific configuration element inside the handle to apply data.</param>
        public DffHandle(string data, IPatConfigHandle handle, string subConfigName)
        {
            this.SubConfigName = subConfigName;

            var dynamicData = data.Split('.');

            if (dynamicData.Length != 3 && dynamicData.Length != 4)
            {
                throw new TestMethodException(
                    "Invalid value on data=[" + data +
                    "], DFF data source format is '<DieId>.<OPTYPE>.<TokenName>.<Field (Optional)>'.\n");
            }

            this.DieId = dynamicData[0].Trim();
            if (this.DieId.ToUpper() == CURRENTVALUE)
            {
                this.DieId = string.Empty;
            }

            this.OPTYPE = dynamicData[1].Trim();
            if (this.OPTYPE.ToUpper() == CURRENTVALUE)
            {
                this.OPTYPE = string.Empty;
            }

            this.TokenName = dynamicData[2].Trim();
            if (string.IsNullOrEmpty(this.TokenName))
            {
                throw new TestMethodException(
                    "Token name is empty, value on data=[" + data + "], DFF data source format is '<TokenName>.<Field (optional)>|<DieId>/<SSID>|<OPTYPE>'.\n");
            }

            if (dynamicData.Length == 4)
            {
                this.TokenName += "." + dynamicData[3].Trim();
            }

            this.Handle = handle;
            this.PatConfigHelper = new PatConfigHelper();
        }

        /// <summary>
        /// Gets or Sets SubConfigName name .
        /// </summary>
        public string SubConfigName { get; set; }

        /// <summary>
        /// Gets or Sets DieId.
        /// </summary>
        public string DieId { get; set; }

        /// <summary>
        /// Gets or Sets OPTYPE.
        /// </summary>
        public string OPTYPE { get; set; }

        /// <summary>
        /// Gets or Sets TokenName.
        /// </summary>
        public string TokenName { get; set; }

        /// <inheritdoc/>
        public IPatConfigHandle Handle { get; }

        /// <inheritdoc/>
        public string DynamicValue { get; }

        /// <summary>
        /// Gets PatConfigHelper to use helper methods.
        /// </summary>
        private PatConfigHelper PatConfigHelper { get; }

        /// <inheritdoc/>
        public void SetData()
        {
            string dffValue = string.Empty;
            if (string.IsNullOrEmpty(this.OPTYPE) && string.IsNullOrEmpty(this.DieId))
            {
                dffValue = Prime.Services.DffService.GetDff(this.TokenName);
            }
            else if (!string.IsNullOrEmpty(this.OPTYPE) && !string.IsNullOrEmpty(this.DieId))
            {
                dffValue = Prime.Services.DffService.GetDff(this.TokenName, this.OPTYPE, this.DieId);
            }
            else if (string.IsNullOrEmpty(this.OPTYPE) && !string.IsNullOrEmpty(this.DieId))
            {
                dffValue = Prime.Services.DffService.GetDffByDieId(this.TokenName, this.DieId);
            }
            else
            {
                dffValue = Prime.Services.DffService.GetDffByOpType(this.TokenName, this.OPTYPE);
            }

            this.PatConfigHelper.SetUpHandlerData(dffValue, this.Handle, this.SubConfigName);
        }
    }
}
