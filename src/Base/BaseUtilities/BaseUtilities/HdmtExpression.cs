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

namespace DDG
{
    using System;
    using NCalc;
    using Prime.Base.Utilities;
    using Prime.ConsoleService;

    /// <summary>
    /// Defines the <see cref="HdmtExpression" />.
    /// </summary>
    public class HdmtExpression : NCalc.Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HdmtExpression"/> class.
        /// </summary>
        /// <param name="expression">String expression.</param>
        /// <param name="options">EvaluateOptions options.</param>
        public HdmtExpression(string expression, EvaluateOptions options)
            : base(expression, options)
        {
            this.EvaluateFunction += this.NCalcExtensionFunctions;
            this.EvaluateParameter += this.EvaluateHdmtToken;
            this.Console = Prime.Services.TestProgramService.GetCurrentLogLevel() == "DISABLED" ? null : Prime.Services.ConsoleService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HdmtExpression"/> class.
        /// </summary>
        /// <param name="expression">String expression.</param>
        public HdmtExpression(string expression)
            : this(expression, EvaluateOptions.None)
        {
        }

        private IConsoleService Console { get; } = null;

        /// <summary>
        /// Defines implicit operator for string to Expression conversion.
        /// </summary>
        /// <param name="o">string value.</param>
        public static implicit operator HdmtExpression(string o)
        {
            return new HdmtExpression(o);
        }

        // TODO: Make GetValueFromDff its own service, make it more robust/generic.
        private static bool GetValueFromDff(string parameter, out string dffValue)
        {
            try
            {
                dffValue = Prime.Services.DffService.GetDff(parameter);
                return true;
            }
            catch
            {
                dffValue = null;
                return false;
            }
        }

        private void NCalcExtensionFunctions(string name, FunctionArgs functionArgs)
        {
            if (name == "ToInt32")
            {
                var param = functionArgs.Parameters[0].Evaluate();
                this.Console?.PrintDebug($"Executing ToInt32({param})");
                functionArgs.Result = System.Convert.ToInt32(param);
                return;
            }

            if (name == "ToDouble")
            {
                var param = functionArgs.Parameters[0].Evaluate();
                this.Console?.PrintDebug($"Executing ToDouble({param})");
                functionArgs.Result = param is string s ? s.StringWithUnitsToDouble() : System.Convert.ToDouble(param);
                return;
            }

            if (name == "Random")
            {
                var r = new Random();
                functionArgs.Result = r.NextDouble();
                this.Console?.PrintDebug($"Evaluated Random()=[{functionArgs.Result}]");
                return;
            }

            if (name == "Substring")
            {
                var param = Convert.ToString(functionArgs.Parameters[0].Evaluate());
                int start = (int)functionArgs.Parameters[1].Evaluate();
                int length = (int)functionArgs.Parameters[2].Evaluate();
                this.Console?.PrintDebug($"Executing Substring(String={param}, Start={start}, Length={length})");
                functionArgs.Result = param.Substring(start, length);
                return;
            }

            if (name == "Bin2Dec")
            {
                var binaryNumber = Convert.ToString(functionArgs.Parameters[0].Evaluate());
                this.Console?.PrintDebug($"Executing Bin2Dec({binaryNumber})");
                try
                {
                    functionArgs.Result = System.Convert.ToInt32(binaryNumber, 2);
                }
                catch (FormatException e)
                {
                    Prime.Services.ConsoleService.PrintError($"DDG Expression Engine Bin2Dec({binaryNumber}) failed. Argument must be a valid binary number <32 bits.");
                    e.Data["Bin2Dec.AdditionalInfo"] = $"Bin2Dec({binaryNumber}) failed. Argument must be a valid binary number <32 bits.";
                    throw;
                }

                return;
            }

            if (name == "Dec2Bin")
            {
                var number = functionArgs.Parameters[0].Evaluate();
                var bits = (int)functionArgs.Parameters[1].Evaluate();
                this.Console?.PrintDebug($"Executing Dec2Bin(Number={number}, Bits={bits})");
                functionArgs.Result = System.Convert.ToString(Convert.ToInt32(number), 2).PadLeft(bits, '0');
                return;
            }

            if (name == "Hex2Bin")
            {
                var number = functionArgs.Parameters[0].Evaluate();
                var bits = (int)functionArgs.Parameters[1].Evaluate();
                this.Console?.PrintDebug($"Executing Dec2Bin(Number={number}, Bits={bits})");
                functionArgs.Result = System.Convert.ToString(Convert.ToInt32(number), 2).PadLeft(bits, '0');
                return;
            }

            if (name == "Reverse")
            {
                var param = Convert.ToString(functionArgs.Parameters[0].Evaluate());
                this.Console?.PrintDebug($"Executing Reverse({param})");
                char[] charArray = param.ToCharArray();
                Array.Reverse(charArray);
                functionArgs.Result = new string(charArray);
                return;
            }

            if (name == "GetPatSymbolString")
            {
                var value = Convert.ToString(functionArgs.Parameters[0].Evaluate());
                var size = Convert.ToInt32(functionArgs.Parameters[1].Evaluate());
                this.Console?.PrintDebug($"Executing GetPatSymbolString({value},{size})");
                functionArgs.Result = value.GetPatSymbolString(size);
                return;
            }
        }

        private void EvaluateHdmtToken(string parameter, ParameterArgs args)
        {
            if (DDG.Gsds.IsTokenFormatAndExists(parameter))
            {
                args.Result = DDG.Gsds.ReadToken(parameter);
                this.Console?.PrintDebug($"Evaluated GSDS Token [{parameter}]=[{args.Result}]");
                return;
            }

            if (DDG.UserVar.Exists(parameter))
            {
                args.Result = DDG.UserVar.ReadAndGetType(parameter, out var userVarType);
                this.Console?.PrintDebug($"Evaluated UserVar Token [{parameter}]=[{args.Result}]");
                return;
            }

            var specSet = DDG.SpecSet.ReadToken(parameter);
            if (!string.IsNullOrEmpty(specSet))
            {
                args.Result = specSet;
                this.Console?.PrintDebug($"Evaluated SpecSet Token [{parameter}]=[{args.Result}]");
            }

            if (GetValueFromDff(parameter, out var dffValue))
            {
                args.Result = dffValue;
                this.Console?.PrintDebug($"Evaluated DFF Token [{parameter}]=[{args.Result}]");
                return;
            }
        }
    }
}
