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

namespace SimpleCtvTC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CommandLine;
    using DDG;
    using Prime.ConsoleService;
    using Prime.PhAttributes;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;

    /// <summary>
    /// This class is intended to overwrite the test method PrimeFuncCaptureCtvTestMethod.
    /// </summary>
    [PrimeTestMethod]
    public class SimpleCtvTC : PrimeFunctionalTestMethod, IFunctionalExtensions
    {
        private List<Register> registersCollection;
        private ushort exitPort = 0;

        /// <summary>
        /// Gets or sets registers to decode from CTV. Format: --registers reg1:11-0 reg2:13,12.
        /// </summary>
        public TestMethodsParams.String Registers { get; set; }

        /// <summary>
        /// Gets or sets registers limits for selected registers. Format: --high reg1:11 --low reg2:1.
        /// </summary>
        public TestMethodsParams.String Limits { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets registers to print to ituff. Format: --registers reg1 reg2.
        /// </summary>
        public TestMethodsParams.String Print { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; set; }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Passed.")]
        [Returns(0, PortType.Fail, "Failed plist execution..")]
        [Returns(2, PortType.Fail, "Failed high limit.")]
        [Returns(3, PortType.Fail, "Failed low limit.")]
        [Returns(4, PortType.Fail, "Failed high and low limits, nor not equal limit.")]
        public override int Execute()
        {
            base.Execute();
            return this.exitPort;
        }

        /// <inheritdoc />
        public override void CustomVerify()
        {
            this.Console = this.LogLevel != PrimeLogLevel.DISABLED ? Prime.Services.ConsoleService : null;
            this.registersCollection = new List<Register>();
            var parserRegistersResult = Parser.Default.ParseArguments<RegistersOptions>(this.Registers.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            parserRegistersResult.WithParsed(options =>
                {
                    foreach (var register in options.Registers)
                    {
                        var item = new Register(register);
                        this.registersCollection.Add(item);
                    }
                })
                .WithNotParsed(e =>
                    throw new ArgumentException(
                        $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: failed parsing arguments. {string.Join("\n", e)}"));
            if (!string.IsNullOrEmpty(this.Limits))
            {
                var parserLimitsResult = Parser.Default.ParseArguments<LimitsOptions>(this.Limits.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                parserLimitsResult.WithParsed(options =>
                    {
                        foreach (var highLimit in options.HighLimits)
                        {
                            var limit = highLimit.Split(':');
                            if (limit.Length != 2)
                            {
                                throw new ArgumentException(
                                    $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: invalid {nameof(this.Limits)} = {highLimit}.");
                            }

                            this.registersCollection.Find(x => x.Name == limit[0]).HighLimit = limit[1].ToInt();
                        }

                        foreach (var lowLimit in options.LowLimits)
                        {
                            var limit = lowLimit.Split(':');
                            if (limit.Length != 2)
                            {
                                throw new ArgumentException(
                                    $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: invalid {nameof(this.Limits)} = {lowLimit}.");
                            }

                            this.registersCollection.Find(x => x.Name == limit[0]).LowLimit = limit[1].ToInt();
                        }

                        foreach (var nowEqualLimit in options.NotEqualLimits)
                        {
                            var limit = nowEqualLimit.Split(':');
                            if (limit.Length != 2)
                            {
                                throw new ArgumentException(
                                    $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: invalid {nameof(this.Limits)} = {nowEqualLimit}.");
                            }

                            this.registersCollection.Find(x => x.Name == limit[0]).NotEqualLimit = limit[1].ToInt();
                        }
                    })
                    .WithNotParsed(e =>
                        throw new ArgumentException(
                            $"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: failed parsing arguments. {string.Join("\n", e)}"));
            }

            if (!string.IsNullOrEmpty(this.Print))
            {
                var parserPrintResult = Parser.Default.ParseArguments<RegistersOptions>(this.Print.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                parserPrintResult.WithParsed(options =>
                {
                    foreach (var register in options.Registers)
                    {
                        this.registersCollection.Find(x => x.Name == register).Print = true;
                    }
                })
                .WithNotParsed(e =>
                    throw new ArgumentException($"{MethodBase.GetCurrentMethod().Module}.{MethodBase.GetCurrentMethod().Name}: failed parsing arguments. {string.Join("\n", e)}"));
            }
        }

        /// <inheritdoc/>
        bool IFunctionalExtensions.ProcessCtvPerPin(Dictionary<string, string> ctvData)
        {
            this.exitPort = 0;
            var data = this.ExtractData(ctvData);
            if (!string.IsNullOrEmpty(data))
            {
                var failedNotEqual = false;
                var failedLow = false;
                var failedHigh = false;
                foreach (var register in this.registersCollection)
                {
                    var binRegister = register.Indexes.Aggregate(string.Empty, (current, index) => current + data[index]);

                    var value = binRegister.BinaryToInteger();
                    this.WriteToItuff(register, value);
                    this.EvaluateLimits(register, value, ref failedNotEqual, ref failedLow, ref failedHigh);
                }

                if (failedNotEqual || (failedHigh && failedLow))
                {
                    this.exitPort = 4;
                }
                else if (failedLow)
                {
                    this.exitPort = 3;
                }
                else
                {
                    this.exitPort = failedHigh ? (ushort)2 : (ushort)1;
                }
            }

            return true;
        }

        private void EvaluateLimits(Register register, int value, ref bool failedNotEqual, ref bool failedLow, ref bool failedHigh)
        {
            if (register.NotEqualLimit >= 0 && value != register.NotEqualLimit)
            {
                this.Console?.PrintDebug($"Failed Register=[{register.Name}] Value=[{value}] NotEqual=[{register.NotEqualLimit}]\n");
                {
                    failedNotEqual = true;
                }
            }

            if (register.HighLimit >= 0 && value > register.HighLimit)
            {
                this.Console?.PrintDebug($"Failed Register=[{register.Name}] Value=[{value}] HighLimit=[{register.HighLimit}]\n");
                {
                    failedHigh = true;
                }
            }

            if (register.LowLimit >= 0 && value < register.LowLimit)
            {
                this.Console?.PrintDebug($"Failed Register=[{register.Name}] Value=[{value}] LowLimit=[{register.LowLimit}]\n");
                {
                    failedLow = true;
                }
            }
        }

        private void WriteToItuff(Register register, int value)
        {
            if (register.Print)
            {
                var ituffWriter = Prime.Services.DatalogService.GetItuffMrsltWriter();
                ituffWriter.SetTnamePostfix($"_{register.Name}");
                if (register.NotEqualLimit != -1)
                {
                    ituffWriter.SetData(value, register.NotEqualLimit, register.NotEqualLimit);
                }
                else
                {
                    ituffWriter.SetData(value, register.LowLimit, register.HighLimit);
                }

                Prime.Services.DatalogService.WriteToItuff(ituffWriter);
            }
        }

        private string ExtractData(Dictionary<string, string> ctvData)
        {
            var data = string.Empty;
            if (ctvData.Count > 0)
            {
                var ctvs = ctvData.Values.ToList();
                var numberOfVectors = ctvs.First().Length;
                var numberOfPins = ctvs.Count;
                if (ctvData.Values.All(x => x.Length == numberOfVectors))
                {
                    for (var i = 0; i < numberOfVectors; i++)
                    {
                        for (var j = 0; j < numberOfPins; j++)
                        {
                            data += ctvs[j][i];
                        }
                    }
                }

                this.Console?.PrintDebug($"Binary String MSB-LSB=[{string.Concat(data.ToCharArray().Reverse())}]\n");
            }

            return data;
        }
    }
}