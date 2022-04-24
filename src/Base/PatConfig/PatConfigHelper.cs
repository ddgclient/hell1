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
    /// This class represents a helper for PatConfig test method.
    /// </summary>
    public class PatConfigHelper
    {
        /// <summary>
        /// Checks if need to set or fill data on handler and apply it.
        /// </summary>
        /// <param name="data">Data to be applied.</param>
        /// <param name="handle">Handler to apply data.</param>
        /// <param name="subConfigName">a specific configuration element inside the handle to apply data.</param>
        public void SetUpHandlerData(string data, IPatConfigHandle handle, string subConfigName)
        {
            if (data.Contains("+"))
            {
                if (data.Length != 2)
                {
                    throw new TestMethodException(
                        "Invalid value on data=[" + data + "], fill mode '+' only allows 1 character.\n");
                }

                PatternSymbol symbol = this.ConvertDataToSymbol(data[0]);
                if (subConfigName == string.Empty)
                {
                    handle.FillData(symbol);
                }
                else
                {
                    handle.FillData(symbol, subConfigName);
                }
            }
            else
            {
                if (subConfigName == string.Empty)
                {
                    handle.SetData(data);
                }
                else
                {
                    handle.SetData(data, subConfigName);
                }
            }
        }

        /// <summary>
        /// Converts data to PatConfig symbol.
        /// </summary>
        /// <returns>PatConfig symbol to be used on fill data.</returns>
        private PatternSymbol ConvertDataToSymbol(char data)
        {
            switch (data)
            {
                case 'H':
                    return PatternSymbol.H;
                case 'L':
                    return PatternSymbol.L;
                case '1':
                    return PatternSymbol.ONE;
                case 'X':
                    return PatternSymbol.X;
                case 'Z':
                    return PatternSymbol.Z;
                case '0':
                    return PatternSymbol.ZERO;
                case 'R':
                    return PatternSymbol.R;
                case 'E':
                    return PatternSymbol.E;
                case 'T':
                    return PatternSymbol.T;
                case 'K':
                    return PatternSymbol.K;
                case 'C':
                    return PatternSymbol.C;
                case 'S':
                    return PatternSymbol.S;
                default:
                    throw new TestMethodException("Undefined Symbol [" + data + "] on fill data.\n");
            }
        }
    }
}
