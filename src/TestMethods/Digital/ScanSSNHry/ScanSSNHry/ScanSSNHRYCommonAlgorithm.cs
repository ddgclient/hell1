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

namespace ScanSSNHry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Prime.Base.Exceptions;
    using Prime.ConsoleService;
    using Prime.FunctionalService;

    /// <summary>
    /// Common algorithm used to find the scan partitionUnderDebug failing using ssn multipartition with rotation.
    /// </summary>
    public class ScanSSNHRYCommonAlgorithm
    {
        private const char HRYFail = '0';
        private const char HRYPass = '1';
        private const char HRYUntested = '8';
        private const char HRYUnassigned = '9';

        /// <summary>
        /// This function proccessed the fails and mapped these according to the json inputs file to print the hry RAWString to ituff.
        /// </summary>
        /// <param name="hryInputProcessed">Class where the json input files where proccessing.</param>
        /// <param name="scanFails">List of fails from the plist execution.</param>
        /// <param name="perPatMaxCapture">Parameter define on the instance (maximum captures per pattern).</param>
        /// <param name="instanceName">instance name for the ituff print.</param>
        /// <param name="partitionsFailing">list of debugPartitions failing.</param>
        /// <param name="partitionsUnderDebugList">list of debugPartitions under debug to be mask under hry rawstring.</param>
        /// <param name="rawStringToPrint"> String to be print on the ituff according to the fails captured.</param>
        public virtual void GenerateHRY(InputsProcessed hryInputProcessed, List<IFailureData> scanFails, ulong perPatMaxCapture, string instanceName, ref List<string> partitionsFailing, List<string> partitionsUnderDebugList, ref List<char> rawStringToPrint)
        {
            ulong pinNumbers = hryInputProcessed.PinMappingProcessed.PinNumbers;

            List<char> rawPartitionsHRY = this.GetInitialHRYString(hryInputProcessed, scanFails, perPatMaxCapture, partitionsUnderDebugList);
            List<string> failingPartitionsPrint = new List<string>();

            // Proccessing each fail.
            foreach (var scanFail in scanFails)
            {
                HRYTemplateInput.Pattern patternObjectFailing =
                    this.GetMatchingObject(hryInputProcessed.HryInputDataProcessed.Patterns, scanFail.GetPatternName());

                foreach (var pinName in scanFail.GetFailingPinNames())
                {
                    string ssnBitFailing = "000";

                    // Processing fails on scan patterns using the SSN rotation method, the reset fails will be proccessed directly.
                    if (!patternObjectFailing.ContentType.Equals("preamble"))
                    {
                        ulong starterBit = ((scanFail.GetCycle() - patternObjectFailing.OutputPacketOffset) * pinNumbers) % patternObjectFailing.PacketSize;
                        ulong maxCounter = patternObjectFailing.PacketSize - 1;
                        ulong[] virtualPacket = CreateVirtualPacket(maxCounter, starterBit, pinNumbers);

                        PinMappingInput.PinMapping pinObjectFailing =
                            this.GetMatchingObject(hryInputProcessed.PinMappingProcessed.PinsMapping, pinName);

                        int positionPinFail = pinObjectFailing.Ssn_datapth - 1;
                        ssnBitFailing = virtualPacket[positionPinFail].ToString().PadLeft(3, '0');
                    }

                    HRYTemplateInput.Pattern.Packet ssnPacketFailing =
                           this.GetMatchingObject(patternObjectFailing.Packets, ssnBitFailing);

                    // HRY ituff data print based on 'HRYPrint' field in the Partitions object.
                    // Add the new partition failing and also check if the partition is on the list of partition under debug.
                    if (!failingPartitionsPrint.Contains(ssnPacketFailing.Partitions[0].HRYPrint))
                    {
                        rawPartitionsHRY[ssnPacketFailing.Partitions[0].HRYIndex] = HRYFail;
                        failingPartitionsPrint.Add(ssnPacketFailing.Partitions[0].HRYPrint);

                        // Proccessing the under debug partitionUnderDebug to print "9" value on the HRY RAWSTRING.
                        if (partitionsUnderDebugList.Contains(ssnPacketFailing.Partitions[0].HRYPrint))
                        {
                                rawPartitionsHRY[ssnPacketFailing.Partitions[0].HRYIndex] = HRYUnassigned;
                        }
                    }
                }
            }

            // Removing the partition under debug from the list of partition failing to print on the ituff
            foreach (string partitionUnderDebug in partitionsUnderDebugList)
            {
                if (failingPartitionsPrint.Contains(partitionUnderDebug))
                {
                    failingPartitionsPrint.Remove(partitionUnderDebug);
                }
            }

            partitionsFailing = failingPartitionsPrint;
            rawStringToPrint = rawPartitionsHRY;

            this.WriteItuffData(rawPartitionsHRY, failingPartitionsPrint, instanceName);
        }

        /// <summary>
        /// Function that create a virtiual ssnPacketFailing per cycle according to the cylce failing, number of pines and packetSize.
        /// </summary>
        /// <param name="maxCounter"> Max values for the virtual ssnPacketFailing defined by packetSize.</param>
        /// <param name="starterBit"> Is the first value for the virtual ssnPacketFailing according to the cycle.</param>
        /// <param name="pinNumbers"> the number of pines that are testing per cycle, used to create the virtual ssnPacketFailing in the specific cycle.</param>
        /// <returns>Array of the virtual ssnPacketFailing.</returns>
        private static ulong[] CreateVirtualPacket(ulong maxCounter, ulong starterBit, ulong pinNumbers)
        {
            ulong[] virtualPacket = new ulong[pinNumbers];
            for (ulong i = 0; i < pinNumbers; i++)
            {
                virtualPacket[i] = starterBit;
                starterBit += 1;
                if (starterBit > maxCounter)
                {
                    starterBit = 0;
                }
            }

            return virtualPacket;
        }

        /// <summary>
        /// Function used to create the string to print on the ituff.
        /// </summary>
        private void WriteItuffData(List<char> rawHRY, List<string> failingPartitionList, string instanceName)
        {
            Prime.Services.DatalogService.SetAltInstanceName(instanceName);

            // adding raw HRY string. Example:
            // 2_tname_PrimeScanHRY _HRY_RAWSTR
            // 2_strgval_0111
            string rawHRYStr = new string(rawHRY.ToArray());
            var writer = Prime.Services.DatalogService.GetItuffStrgvalWriter();
            writer.SetTnamePostfix("HRY_RAWSTR");
            writer.SetData(rawHRYStr);
            Prime.Services.DatalogService.WriteToItuff(writer);

            // adding hry prints for each failing partitionUnderDebug. Example:
            // 2_tname_HRY_PrimeScanHRY_GLM_M0_C0_IEC
            // 2_strgval_0
            failingPartitionList.ForEach(failingPartition =>
            {
                writer.SetTnamePostfix(failingPartition);
                writer.SetData($"{HRYFail}");
                Prime.Services.DatalogService.WriteToItuff(writer);
            });
        }

        /// <summary>
        /// This will return the initial HRY string. Taking into account the HRY string length and unassigned debugPartitions.
        /// </summary>
        /// <param name="hryInputProcessed">processed input file.</param>
        /// <param name="scanFails">scan failures to process.</param>
        /// <param name="perPatMaxCapture">maximum captures per patternGroupFailing.</param>
        /// <param name="partitionUnderDebug">list of debugPartitions under debug.</param>
        /// <returns>String to print to ituff.</returns>
        private List<char> GetInitialHRYString(InputsProcessed hryInputProcessed, List<IFailureData> scanFails, ulong perPatMaxCapture, List<string> partitionUnderDebug)
        {
            List<char> hryString = new List<char>(hryInputProcessed.HryInputDataProcessed.HryLength);

            char hryDefaultSymbol = HRYPass;
            if (!partitionUnderDebug.Any() & this.CaptureLimitsReached(scanFails, perPatMaxCapture))
            {
                hryDefaultSymbol = HRYUntested;
            }

            hryString.AddRange(Enumerable.Repeat(hryDefaultSymbol, hryInputProcessed.HryInputDataProcessed.HryLength));
            hryInputProcessed.UnassignedPartitions.ToList<int>().ForEach(x => hryString[x] = HRYUnassigned);
            return hryString;
        }

        /// <summary>
        /// This function will return true if capture limits are reached.
        /// This is needed to know if the untested debugPartitions are passing, or simply not tested due to capture limits.
        /// </summary>
        /// <param name="scanFails">scan failures to process.</param>
        /// <param name="perPatMaxCapture">maximum captures per patternGroupFailing.</param>
        /// <returns>true if capture limits reached, false otherwise.</returns>
        private bool CaptureLimitsReached(List<IFailureData> scanFails, ulong perPatMaxCapture)
        {
            Dictionary<string, ulong> failPerPat = new Dictionary<string, ulong>();
            scanFails.ForEach(x => failPerPat[x.GetPatternName()] = 0);
            foreach (var scanFail in scanFails)
            {
                if (++failPerPat[scanFail.GetPatternName()] >= perPatMaxCapture)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Function used to get the match value in the list for de pinMapping or HRYTemplate and the value to look.
        /// </summary>
        /// <param name="regularExpressions">incoming rgular expression to look.</param>
        /// <param name="stringToLookFor">incoming string to match with the regular expresion.</param>
        /// <returns>return the regular extensions.</returns>
        private T GetMatchingObject<T>(List<T> regularExpressions, string stringToLookFor)
        {
            foreach (var regularExpression in regularExpressions)
            {
                Regex regex = new Regex(regularExpression.ToString());
                Match match = regex.Match(stringToLookFor);
                if (match.Success)
                {
                    return regularExpression;
                }
            }

            throw new TestMethodException($"Failed to find any matching object for regex=[{stringToLookFor}].");
        }
    }
}