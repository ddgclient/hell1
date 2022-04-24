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

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DTSBase.UnitTest")]

namespace DTSBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DDG;
    using Prime.ConsoleService;
    using Prime.SharedStorageService;

    /// <summary>
    /// VoltageConverterImpl runs different pin attributes, timings and pattern modifications based on voltageObject settings.
    /// </summary>
    internal class DTSImpl : IDTS
    {
        private readonly Configuration settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="DTSImpl"/> class.
        /// </summary>
        /// <param name="configurationName">Configuration.</param>
        public DTSImpl(string configurationName)
        {
            if (!Prime.Services.SharedStorageService.KeyExistsInObjectTable(DDG.DTS.DTSConfigurationTable, Context.DUT))
            {
                throw new Exception($"{DDG.DTS.DTSConfigurationTable} does not exist in SharedStorage.");
            }

            var configurations = Prime.Services.SharedStorageService.GetRowFromTable(DDG.DTS.DTSConfigurationTable, typeof(List<Configuration>), Context.DUT) as List<Configuration>;
            if (configurations == null || configurations.Count == 0)
            {
                throw new Exception($"{DDG.DTS.DTSConfigurationTable} is invalid.");
            }

            this.settings = configurations.Find(o => o.Name == configurationName);
            this.Console = Prime.Services.TestProgramService.GetCurrentLogLevel() != "DISABLED" ? Prime.Services.ConsoleService : null;
        }

        /// <summary>
        /// Gets a variable holding Prime.Services.ConsoleService or null depending on the current instances LogLevel.
        /// </summary>
        protected IConsoleService Console { get; }

        /// <inheritdoc/>
        public Configuration GetSettings()
        {
            return this.settings;
        }

        /// <inheritdoc/>
        public bool ProcessCapturedData(string ctv)
        {
            var values = new Dictionary<string, List<double>>();
            this.GetValues(ctv, ref values);
            this.PrintToDatalog(values);
            return this.EvaluateLimits(values);
        }

        /// <inheritdoc/>
        public bool EvaluateLimits(Dictionary<string, List<double>> values)
        {
            var settings = this.GetSettings();
            if (values == null ||
                values.Count == 0 ||
                !(!string.IsNullOrEmpty(settings.SetPoint) &
                                                     (!string.IsNullOrEmpty(settings.UpperTolerance) ||
                                                      !string.IsNullOrEmpty(settings.LowerTolerance))))
            {
                return true;
            }

            var passing = true;
            var upperTolerance = string.IsNullOrEmpty(settings.UpperTolerance)
                ? double.NaN
                : settings.UpperTolerance.ToDouble(true);
            var lowerTolerance = string.IsNullOrEmpty(settings.LowerTolerance)
                ? double.NaN
                : settings.LowerTolerance.ToDouble(true);
            var setPoint = settings.SetPoint.ToDouble(true);

            foreach (var sensor in values.Where(sensor => !settings.IgnoredSensorsList.Contains(sensor.Key)))
            {
                var minValue = this.settings.LastPattern ? sensor.Value.Last() : sensor.Value.Min();
                var maxValue = this.settings.LastPattern ? sensor.Value.Last() : sensor.Value.Max();
                if (!double.IsNaN(upperTolerance) && maxValue - setPoint > upperTolerance)
                {
                    this.Console?.PrintDebug($"DTS monitor: Failed {sensor.Key} Max={maxValue} Upper Tolerance Limit={upperTolerance}");
                    passing = false;
                }

                if (!double.IsNaN(lowerTolerance) && setPoint - minValue > lowerTolerance)
                {
                    this.Console?.PrintDebug($"DTS monitor: Failed {sensor.Key} Min={minValue} Lower Tolerance Limit={lowerTolerance}");
                    passing = false;
                }
            }

            return passing;
        }

        /// <inheritdoc/>
        public void PrintToDatalog(Dictionary<string, List<double>> values)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            if (this.settings.DatalogValues)
            {
                var strgValWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();
                strgValWriter.SetTnamePostfix("_DTS");

                var data = values.Aggregate(string.Empty, (current, sensor) =>
                {
                    if (this.settings.LastPattern)
                    {
                        return current + $"{sensor.Key}:{sensor.Value.Last():F}|";
                    }

                    if (sensor.Value.Count == 1)
                    {
                        return current + $"{sensor.Key}:{sensor.Value.Max():F}|";
                    }

                    return current + $"{sensor.Key}:{sensor.Value.Min():F},{sensor.Value.Average():F},{sensor.Value.Max():F}|";
                });
                data = data.Remove(data.Length - 1, 1);
                strgValWriter.SetData(data);
                Prime.Services.DatalogService.WriteToItuff(strgValWriter);
            }

            if (this.settings.CompressedDatalog && !this.settings.LastPattern)
            {
                var strgValWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();
                strgValWriter.SetTnamePostfix("_COMPRESSED_DTS");
                strgValWriter.SetDelimiterCharacterForWrap('%');
                var data = string.Empty;
                foreach (var sensor in values)
                {
                    data += sensor.Key;
                    data += ":";
                    var stringValues = sensor.Value.Select(x => x.ToString("F"));
                    data += string.Join(",", stringValues);
                    data += "|";
                }

                data = data.Remove(data.Length - 1, 1);
                strgValWriter.SetData(Prime.Base.Utilities.StringUtilities.DeflateCompress32(data));
                this.Console?.PrintDebug($"DTS data: {data}");
                Prime.Services.DatalogService.WriteToItuff(strgValWriter);
            }
        }

        /// <inheritdoc/>
        public void GetValues(string ctv, ref Dictionary<string, List<double>> values)
        {
            if (this.settings == null || !this.settings.IsEnabled || ctv.Length / (this.settings.SensorsList.Count * this.settings.RegisterSize) < 1)
            {
                this.Console?.PrintDebug("DTS capture mode is disabled, has not been initialized or there is no captured data. Process will be skipped.");
                return;
            }

            if (values == null)
            {
                values = new Dictionary<string, List<double>>();
            }

            var sensorsCount = this.settings.SensorsList.Count;
            var registerSize = this.settings.RegisterSize;
            var patternsCount = this.settings.LastPattern
                ? 1
                : ctv.Length / (sensorsCount * registerSize);
            for (var i = 0; i < patternsCount; i++)
            {
                for (var j = 0; j < sensorsCount; j++)
                {
                    var bits = this.settings.LastPattern ?
                        ctv.Substring(ctv.Length - ((i + 1) * sensorsCount * registerSize) + (j * registerSize), registerSize).Reverse() :
                        ctv.Substring((patternsCount * sensorsCount * registerSize) - ((i + 1) * sensorsCount * registerSize) + (j * registerSize), registerSize).Reverse();

                    var value = (bits.BinaryToInteger() * this.settings.Slope) + this.settings.Offset;
                    if (values.ContainsKey(this.settings.SensorsList[j]))
                    {
                        values[this.settings.SensorsList[j]].Add(value);
                    }
                    else
                    {
                        var sensorValues = new List<double> { value };
                        values.Add(this.settings.SensorsList[j], sensorValues);
                    }
                }
            }
        }
    }
}