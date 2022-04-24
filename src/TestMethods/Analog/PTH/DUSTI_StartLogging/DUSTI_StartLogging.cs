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

namespace DUSTI_StartLogging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Prime;
    using Prime.ConsoleService;
    using Prime.PhAttributes;
    using Prime.TesterService;

    /// <summary>
    /// Test method used to control the DUSTI hardware on HDMT testers.  This allows for read out of DTS measurements over the TDO.  The DUSTI is controlled using the I2C bus availalbe on hte HDMT tester.
    /// </summary>
    [PrimeTestMethod]

    public class DUSTI_StartLogging : Prime.TestMethods.TestMethodBase
    {
        private static Mutex mut = new Mutex();
        private int preTime;

        /// <summary>
        /// Gets or Sets Levels Load option.  Not required.
        /// </summary>
        public Prime.TestMethods.TestMethodsParams.String LevelsOption { get; set; }

        /// <summary>
        /// Gets or Sets Levels Load option.  Not required.
        /// </summary>
        public Prime.TestMethods.TestMethodsParams.String PinName { get; set; }

        /// <summary>
        /// Gets or sets  the flow to pass, no failures.
        /// </summary>
        public Prime.TestMethods.TestMethodsParams.String ForceFlow { get; set; }

        /// <summary>
        /// Gets or sets  the count of attempts we try to execute the I2C command.
        /// </summary>
        public Prime.TestMethods.TestMethodsParams.String AttemptCount { get; set; }

        /// <summary>
        /// Gets or sets  the wait time between writing an I2C command to reading a completion value.
        /// </summary>
        public Prime.TestMethods.TestMethodsParams.String AckWaitTime { get; set; }

        /// <summary>
        /// Gets or sets  the wait time to get the FPGA value.
        /// </summary>
        public Prime.TestMethods.TestMethodsParams.String FpgaWaitTime { get; set; }

        /// <inheritdoc />
        public override void Verify()
        {
            if (this.LevelsOption == string.Empty)
            {
                Prime.Services.ConsoleService.PrintDebug("Levels is Empty. \n");
                throw new ArgumentException("There are no Levels specified.\n");
            }
            else if (this.PinName == string.Empty)
            {
                Prime.Services.ConsoleService.PrintDebug("Pin Name is Empty. \n");
                throw new ArgumentException("There is no Pin Name specified.\n");
            }
            else if (this.ForceFlow != "False" && this.ForceFlow != "True")
            {
                Prime.Services.ConsoleService.PrintDebug("Force Flow is Needs to be True or False. \n");
                throw new ArgumentException("There is no Force Flow specified.\n");
            }
            else
            {
                Prime.Services.ConsoleService.PrintDebug("Great Success!!!. \n");
            }
        }

        /// <summary>
        /// Sets the ack time and the frequency for the I2C bus.
        /// </summary>
        /// <returns>
        /// True when complete.
        /// </returns>

        /// <summary>
        /// Checks to see if DUSTI/Snoopy responds to basic communication.
        /// </summary>
        /// <returns>
        /// True when present, false when exception occurs..
        /// </returns>
        public bool VerifyI2CPresent()
        {
            byte readAddress = 0x2C << 1 | 1;

            try
            {
                List<byte> readDataAtte = Prime.Services.TesterService.ReadI2cData(this.PinName, readAddress, 2);
                Prime.Services.ConsoleService.PrintDebug("I2C Connection Validation");
            }
            catch (Prime.Base.Exceptions.FatalException)
            {
                Prime.Services.ConsoleService.PrintDebug("Exception Caught");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Passes ULT to the MCU and starts the logging of data.
        /// </summary>
        /// <returns>
        /// See above.
        /// </returns>
        public bool StartLogging()
        {
            List<byte> ultValue = new List<byte>();
            int i;
            int localLen = 0;
            int start_time, elapsed_time;

            // Initialize value.
            start_time = DateTime.Now.Millisecond;
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Start Logging " + start_time);
            for (i = 0; i < 52; i++)
            {
                ultValue.Insert(i, 0x00);
            }

            // ultValue.Insert(0, 0x00); // Read Buffer for MAX3170
            ultValue.Insert(0, 0x00);
            ultValue.Insert(1, 0x15);
            ultValue.Insert(2, 0x00);
            ultValue.Insert(3, 0x34);

            var lotName = Prime.Services.UserVarService.GetStringValue("SCVars", "SC_LOTNAME");
            var partWafer = Prime.Services.UserVarService.GetStringValue("SCVars", "SC_PARTIALWAFID");
            var waferX = Prime.Services.UserVarService.GetStringValue("SCVars", "SC_WAFERX");
            var waferY = Prime.Services.UserVarService.GetStringValue("SCVars", "SC_WAFERY");
            var cX = Prime.Services.UserVarService.GetStringValue("SCVars", "SC_TRAYX");
            var cY = Prime.Services.UserVarService.GetStringValue("SCVars", "SC_TRAYY");
            var site = Prime.Services.UserVarService.GetStringValue("SCVars", "SC_SITEID");
            var cell = Prime.Services.UserVarService.GetStringValue("SCVars", "SC_CELLID");
            var location = Prime.Services.UserVarService.GetStringValue("SCVars", "SC_LOCN");
            var chuck = Prime.Services.UserVarService.GetStringValue("SCVars", "SC_CHUCKID");
            var tray = Prime.Services.UserVarService.GetStringValue("SCVars", "SC_TRAYID");
            var testCount = Prime.Services.UserVarService.GetStringValue("SCVars", "SC_TEST_COUNT");
            byte[] singleByte = new byte[1];

            // Begin Lot Name Sequencing
            localLen = lotName.Length;
            Prime.Services.ConsoleService.PrintDebug("Lot Number = " + lotName);
            for (i = 0; i < localLen; i++)
            {
                singleByte = Encoding.ASCII.GetBytes(lotName.Substring(i, 1));
                ultValue.Insert(4 + i, singleByte[0]);
            }

            // Begin Wafer Num Name Sequencing
            localLen = partWafer.Length;
            Prime.Services.ConsoleService.PrintDebug("Wafer Number = " + partWafer);
            for (i = 0; i < localLen; i++)
            {
                singleByte = Encoding.ASCII.GetBytes(partWafer.Substring(i, 1));
                ultValue.Insert(14 + i, singleByte[0]);
            }

            // Begin Wafer X Name Sequencing
            localLen = waferX.Length;
            Prime.Services.ConsoleService.PrintDebug("WafX Number = " + waferX);
            for (i = 0; i < localLen; i++)
            {
                singleByte = Encoding.ASCII.GetBytes(waferX.Substring(i, 1));
                ultValue.Insert(20 + i, singleByte[0]);
            }

            // Begin Wafer Y Name Sequencing
            localLen = waferY.Length;
            Prime.Services.ConsoleService.PrintDebug("WafY Number = " + waferY);
            for (i = 0; i < localLen; i++)
            {
                singleByte = Encoding.ASCII.GetBytes(waferY.Substring(i, 1));
                ultValue.Insert(23 + i, singleByte[0]);
            }

            // Begin Tray X Name Sequencing
            localLen = cX.Length;
            Prime.Services.ConsoleService.PrintDebug("Cx Number = " + cX);
            for (i = 0; i < localLen; i++)
            {
                singleByte = Encoding.ASCII.GetBytes(cX.Substring(i, 1));
                ultValue.Insert(26 + i, singleByte[0]);
            }

            // Begin Tray Y Name Sequencing
            localLen = cY.Length;
            Prime.Services.ConsoleService.PrintDebug("Cy Number = " + cY);
            for (i = 0; i < localLen; i++)
            {
                singleByte = Encoding.ASCII.GetBytes(cY.Substring(i, 1));
                ultValue.Insert(28 + i, singleByte[0]);
            }

            // Begin Site ID Name Sequencing
            localLen = site.Length;
            Prime.Services.ConsoleService.PrintDebug("Site Number = " + site);
            for (i = 0; i < localLen; i++)
            {
                singleByte = Encoding.ASCII.GetBytes(site.Substring(i, 1));
                ultValue.Insert(30 + i, singleByte[0]);
            }

            // Begin Cell ID Name Sequencing
            localLen = cell.Length;
            Prime.Services.ConsoleService.PrintDebug("Cell Number = " + cell);
            for (i = 0; i < localLen; i++)
            {
                singleByte = Encoding.ASCII.GetBytes(cell.Substring(i, 1));
                ultValue.Insert(34 + i, singleByte[0]);
            }

            // Begin Location Name Sequencing
            localLen = location.Length;
            Prime.Services.ConsoleService.PrintDebug("Location Number = " + location);
            for (i = 0; i < localLen; i++)
            {
                singleByte = Encoding.ASCII.GetBytes(location.Substring(i, 1));
                ultValue.Insert(41 + i, singleByte[0]);
            }

            // Chuck ID
            if (chuck.Length > 0)
            {
                Prime.Services.ConsoleService.PrintDebug("Chuck Number = " + chuck);
                singleByte = Encoding.ASCII.GetBytes(chuck.Substring(0, 1));
                ultValue.Insert(48, singleByte[0]);
            }

            // Tray Seq
            if (tray.Length > 0)
            {
                Prime.Services.ConsoleService.PrintDebug("Tray Number = " + tray);
                singleByte = Encoding.ASCII.GetBytes(tray.Substring(0, 1));
                ultValue.Insert(49, singleByte[0]);
            }

            // Test Count
            if (testCount.Length > 0)
            {
                Prime.Services.ConsoleService.PrintDebug("Test Count = " + testCount);
                singleByte = Encoding.ASCII.GetBytes(testCount.Substring(0, 1));
                ultValue.Insert(50, singleByte[0]);
            }

            int datCount = ultValue.Count;
            Prime.Services.ConsoleService.PrintDebug("Content Count " + datCount);
            int stripCount = 0;
            stripCount = datCount - 52;
            ultValue.RemoveRange(51, stripCount);
            datCount = ultValue.Count;
            Prime.Services.ConsoleService.PrintDebug("Total Count " + datCount);
            int sumTotal = ultValue.Sum(x => Convert.ToInt32(x));
            string checkSumHex = sumTotal.ToString("X");
            int checkSumLen = checkSumHex.Length;
            string checkSumStrip = " ";
            switch (checkSumLen)
            {
                case 1:
                    checkSumStrip = checkSumHex.Substring(0, 1);
                    break;
                case 2:
                    checkSumStrip = checkSumHex.Substring(0, 2);
                    break;
                case 3:
                    checkSumStrip = checkSumHex.Substring(1, 2);
                    break;
                case 4:
                    checkSumStrip = checkSumHex.Substring(2, 2);
                    break;
            }

            Prime.Services.ConsoleService.PrintDebug("Hex Sum  " + checkSumHex);
            Prime.Services.ConsoleService.PrintDebug("Hex Sum Strip " + checkSumStrip);
            Prime.Services.ConsoleService.PrintDebug("Sum of Bytes " + sumTotal);
            int divisor = sumTotal / 255;
            Prime.Services.ConsoleService.PrintDebug("Divisor " + divisor);
            int checkSum = sumTotal - (255 * divisor);
            checkSum = Convert.ToInt32(checkSumStrip, 16);
            Prime.Services.ConsoleService.PrintDebug("CheckSum " + checkSum);
            ultValue.Add(Convert.ToByte(checkSum));
            datCount = ultValue.Count;
            byte[] readWriteValue = new byte[datCount];
            byte deviceAddressWrite = 0x2C << 1;
            byte deviceAddressRead = 0x2C << 1 | 1;
            readWriteValue = ultValue.ToArray();
            for (i = 0; i < datCount; i++)
            {
                Prime.Services.ConsoleService.PrintDebug("Content Dump " + i + " Value " + readWriteValue[i].ToString());
            }

            bool stop = true;
            int attemptLoop = 0;
            int waitingTime = 3;
            int fPGAWait = 3;
            if (this.AttemptCount == string.Empty)
            {
                attemptLoop = 10;
            }
            else
            {
                attemptLoop = Convert.ToInt32(this.AttemptCount);
            }

            if (this.AckWaitTime == string.Empty)
            {
                waitingTime = 3;
            }
            else
            {
                waitingTime = Convert.ToInt32(this.AckWaitTime);
                if (waitingTime < 3)
                {
                    waitingTime = 3;
                }
            }

            if (this.FpgaWaitTime == string.Empty)
            {
                fPGAWait = 3;
            }
            else
            {
                fPGAWait = Convert.ToInt32(this.FpgaWaitTime);
                if (fPGAWait < 3)
                {
                    fPGAWait = 3;
                }
            }

            for (i = 0; i < attemptLoop; i++)
            {
                Prime.Services.TesterService.WriteI2cData(this.PinName, deviceAddressWrite, ultValue, stop);
                Thread.Sleep(waitingTime);
                int readCount = 1;
                List<byte> readData = Prime.Services.TesterService.ReadI2cData(this.PinName, deviceAddressRead, readCount);
                Prime.Services.ConsoleService.PrintDebug("Read Value " + readData.Sum(x => Convert.ToInt32(x)));
                if (readData.Sum(x => Convert.ToInt32(x)) == 6)
                {
                    Prime.Services.ConsoleService.PrintDebug("FPGA ACK was successful");
                    break;
                }
                else
                {
                    Prime.Services.ConsoleService.PrintDebug("FPGA ACK was not successful");
                    if (attemptLoop - 1 == i)
                    {
                        return false;
                    }
                }
            }

            List<byte> fpgaStat = new List<byte>();
            fpgaStat.Add(0x00);
            fpgaStat.Add(0x17);
            uint dutID = Prime.Services.TestProgramService.GetCurrentDutIndex();
            if (dutID == 0)
            {
                fpgaStat.Add(0x0);
            }
            else if (dutID == 1)
            {
                fpgaStat.Add(0x1);
            }

            for (i = 0; i < attemptLoop; i++)
            {
                Prime.Services.TesterService.WriteI2cData(this.PinName, deviceAddressWrite, fpgaStat, stop);
                Thread.Sleep(fPGAWait);
                int readCount = 2;
                List<byte> readFpgaData = Prime.Services.TesterService.ReadI2cData(this.PinName, deviceAddressRead, readCount);
                Prime.Services.ConsoleService.PrintDebug("FPGA Data Bytes " + readFpgaData.ElementAt(1) + " And " + readFpgaData.ElementAt(0));
                Prime.Services.ConsoleService.PrintDebug("FPGA Data " + readFpgaData.Sum(x => Convert.ToInt32(x)));
                if (readFpgaData.Sum(x => Convert.ToInt32(x)) == 102)
                {
                    Prime.Services.ConsoleService.PrintDebug("Start Logging Transaction Successful");

                    // ITUFF Logging
                    string ituffInput1 = "2_tname_testtime_" + this.InstanceName + "\n";
                    /*Prime.Services.DatalogService.WriteToItuff(ituffInput1);
                    Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName + "\n");*/
                    Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
                    elapsed_time = DateTime.Now.Millisecond - start_time;
                    /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS\n");*/
                    Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");
                    return true;
                }
                else
                {
                    Prime.Services.ConsoleService.PrintDebug("Start Logging Transaction Unsuccessful");
                    if (i == attemptLoop - 1)
                    {
                        // ITUFF Logging
                        string ituffInput2 = "2_tname_testtime_" + this.InstanceName + "\n";
                        /*Prime.Services.DatalogService.WriteToItuff(ituffInput2);
                        Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName + "\n");*/
                        Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
                        elapsed_time = DateTime.Now.Millisecond - start_time;
                        Prime.Services.ConsoleService.PrintDebug("Time Stamp Elapsed Start Logging " + elapsed_time);
                        Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");
                        /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS\n");*/
                        break;
                    }
                }
            }

            // ITUFF Logging
            string ituffInput = "2_tname_testtime_" + this.InstanceName + "\n";
            /*Prime.Services.DatalogService.WriteToItuff(ituffInput);
            Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName + "\n");*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
            elapsed_time = DateTime.Now.Millisecond - start_time;
            /* Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS\n");*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");
            return false;
        }

        /// <inheritdoc />
        ///
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        [Returns(2, PortType.Pass, "Pass!")]
        [Returns(3, PortType.Pass, "Pass!")]
        [Returns(4, PortType.Pass, "Pass!")]

        public override int Execute()
        {
            int attemptLoop = 0;
            int waitingTime = 0;
            int fPGAWait = 0;
            int start_time;
            start_time = DateTime.Now.Millisecond;
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Execution Begin " + start_time);

            if (this.VerifyI2CPresent())
            {
                if (this.AttemptCount == string.Empty)
                {
                    attemptLoop = 10;
                }
                else
                {
                    attemptLoop = Convert.ToInt32(this.AttemptCount);
                }

                if (this.AckWaitTime == string.Empty)
                {
                    waitingTime = 1;
                }
                else
                {
                    waitingTime = Convert.ToInt32(this.AckWaitTime);
                }

                if (this.FpgaWaitTime == string.Empty)
                {
                    fPGAWait = 1;
                }
                else
                {
                    fPGAWait = Convert.ToInt32(this.FpgaWaitTime);
                }

                uint dutID = Prime.Services.TestProgramService.GetCurrentDutIndex();
                Prime.Services.ConsoleService.PrintDebug("Dut Index " + dutID);
                Prime.Services.ConsoleService.PrintDebug("Begin Execution");
                Dictionary<string, string> timeOutSetting = new Dictionary<string, string>();
                timeOutSetting.Add("TimeOut", "10s");
                Prime.Services.PinService.SetPinAttributeValues(this.PinName, timeOutSetting);

                // Start Logging.
                mut.WaitOne(); // Set mutex to execute one site at a time.
                this.preTime = DateTime.Now.Millisecond - start_time;
                Prime.Services.ConsoleService.PrintDebug("Time Stamp Start Logging " + this.preTime);
                if (this.ForceFlow == "False")
                {
                    if (this.StartLogging())
                    {
                        mut.ReleaseMutex(); // Release Mutex.
                        return 1;
                    }
                    else
                    {
                        mut.ReleaseMutex(); // Release Mutex.
                        return 0;
                    }
                }
                else if (this.ForceFlow == "True")
                {
                    if (this.StartLogging())
                    {
                        mut.ReleaseMutex(); // Release Mutex.
                        return 1;
                    }
                    else
                    {
                        mut.ReleaseMutex(); // Release Mutex.
                        return 2;
                    }
                }
            }

            int elapsed_time = 0;
            this.preTime = DateTime.Now.Millisecond - start_time;
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Init Max " + this.preTime);

            // ITUFF Logging
            /*Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName);*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Elapsed " + elapsed_time);
            /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");
            Prime.Services.ConsoleService.PrintDebug("Flow Port 4");
            return 4;
        }
    }
}