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

namespace AuxiliaryTC
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Prime.ConsoleService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.PhAttributes;
    using Prime.TestMethods;

    /// <summary>
    /// Dummy description of this test method.
    /// </summary>
    [PrimeTestMethod]
    public class AuxiliaryTC : TestMethodBase
    {
        // todo: centralize this.
        private static readonly Dictionary<ValueType, DDG.UserVar.ValidTypes> UserVarTypeMap = new Dictionary<ValueType, DDG.UserVar.ValidTypes>
        {
            { ValueType.Integer, DDG.UserVar.ValidTypes.INTEGER },
            { ValueType.Double, DDG.UserVar.ValidTypes.DOUBLE },
            { ValueType.String, DDG.UserVar.ValidTypes.STRING },
        };

        private DDG.HdmtExpression expressionObj;
        private DDG.HdmtExpression portExpressionObj;

        /// <summary>
        /// Defines the data types.
        /// </summary>
        public enum ValueType
        {
            /// <summary>
            /// Integer.
            /// </summary>
            Integer,

            /// <summary>
            /// Double.
            /// </summary>
            Double,

            /// <summary>
            /// String.
            /// </summary>
            String,
        }

        /// <summary>
        /// Defines the different storage types.
        /// </summary>
        public enum StorageType
        {
            /// <summary>
            /// UserVar.
            /// </summary>
            UserVar,

            /// <summary>
            /// SharedStorage
            /// </summary>
            SharedStorage,

            /// <summary>
            /// DFF.
            /// </summary>
            DFF,
        }

        /// <summary>
        /// Enable type.
        /// </summary>
        public enum EnableType
        {
            /// <summary>
            /// Enabled.
            /// </summary>
            Enabled,

            /// <summary>
            /// Disabled.
            /// </summary>
            Disabled,
        }

        /// <summary>
        /// Gets or sets the name of the callback to execute.
        /// </summary>
        public TestMethodsParams.String Expression { get; set; }

        /// <summary>
        /// Gets or sets the parameters to send to the callback.
        /// </summary>
        public ValueType DataType { get; set; } = ValueType.Integer;

        /// <summary>
        /// Gets or sets the ResultToken storage type.
        /// </summary>
        public StorageType Storage { get; set; } = StorageType.SharedStorage;

        /// <summary>
        /// Gets or sets datalog enable type.
        /// </summary>
        public EnableType Datalog { get; set; } = EnableType.Disabled;

        /// <summary>
        /// Gets or sets the names of token where the evaluated result will be set.
        /// </summary>
        public TestMethodsParams.String ResultToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an expression to set exit port based on result.
        /// </summary>
        public TestMethodsParams.String ResultPort { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;
            if (string.IsNullOrEmpty(this.ResultPort) && string.IsNullOrEmpty(this.ResultToken))
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: user must specify at least one option {nameof(this.ResultToken)} and/or {nameof(this.ResultPort)}.");
            }

            this.expressionObj = new DDG.HdmtExpression(this.Expression);
            this.portExpressionObj = string.IsNullOrEmpty(this.ResultPort) ? null : new DDG.HdmtExpression(this.ResultPort);
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(2, PortType.Pass, "Pass!")]
        [Returns(3, PortType.Pass, "Pass!")]
        [Returns(4, PortType.Pass, "Pass!")]
        [Returns(5, PortType.Pass, "Pass!")]
        [Returns(6, PortType.Pass, "Pass!")]
        [Returns(7, PortType.Pass, "Pass!")]
        [Returns(8, PortType.Pass, "Pass!")]
        [Returns(9, PortType.Pass, "Pass!")]
        [Returns(10, PortType.Pass, "Pass!")]
        [Returns(11, PortType.Pass, "Pass!")]
        [Returns(12, PortType.Pass, "Pass!")]
        [Returns(13, PortType.Pass, "Pass!")]
        [Returns(14, PortType.Pass, "Pass!")]
        [Returns(15, PortType.Pass, "Pass!")]
        [Returns(16, PortType.Pass, "Pass!")]
        [Returns(17, PortType.Pass, "Pass!")]
        [Returns(18, PortType.Pass, "Pass!")]
        [Returns(19, PortType.Pass, "Pass!")]
        [Returns(20, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            this.Console?.PrintDebug($"Evaluating {nameof(this.Expression)}=[{this.Expression}])");
            try
            {
                var result = this.expressionObj.Evaluate();
                this.Console?.PrintDebug($"--Evaluated Result=[{result}].");

                if (!string.IsNullOrEmpty(this.ResultToken))
                {
                    this.SaveResult(result);
                }

                if (this.Datalog == EnableType.Enabled)
                {
                    this.DatalogResult(result);
                }

                if (string.IsNullOrEmpty(this.ResultPort))
                {
                    return 1;
                }

                var resultToken = new Dictionary<string, object>()
                {
                    { "R", result },
                };

                this.portExpressionObj.Parameters = resultToken;
                {
                    return (int)this.portExpressionObj.Evaluate();
                }
            }
            catch (Exception ex)
            {
                Prime.Services.ConsoleService.PrintError($"{ex.Message}\n{ex.StackTrace}");
                return 0;
            }
        }

        private void DatalogResult(object result)
        {
            IItuffFormat writer;
            if (this.DataType == ValueType.Double)
            {
                writer = Prime.Services.DatalogService.GetItuffMrsltWriter();
                (writer as IMrsltFormat).SetData(Convert.ToDouble(result));
            }
            else
            {
                writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
                (writer as IStrgvalFormat).SetData(Convert.ToString(result));
            }

            Prime.Services.DatalogService.WriteToItuff(writer);
        }

        private void SaveResult(object result)
        {
            switch (this.Storage)
            {
                case StorageType.UserVar:
                    DDG.UserVar.Write(this.ResultToken, UserVarTypeMap[this.DataType], result);
                    break;
                case StorageType.DFF:
                    Prime.Services.DffService.SetDff(this.ResultToken, Convert.ToString(result));
                    break;
                case StorageType.SharedStorage:
                default:
                    DDG.Gsds.WriteToken(this.ResultToken, result);
                    break;
            }
        }
    }
}