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

namespace DUSTI_StopLogging
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

    public class DUSTI_StopLogging : Prime.TestMethods.TestMethodBase
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
                Prime.Services.ConsoleService.PrintDebug("Force Flow is Empty. \n");
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
        /// Stops the data logging.
        /// </summary>
        /// /// <returns>
        /// Stops the MCU from streaming data.
        /// </returns>
        public bool StopLogging()
        {
            int i;
            byte deviceAddressWrite = 0x2C << 1; // Using device address 0xA with first bit set to write (0)
            byte deviceAddressRead = 0x2C << 1 | 1; // Using device address 0xA with first bit set to write (0)
            bool stop = true;
            int attemptLoop = 0;
            int start_time, elapsed_time;
            uint dutID = Prime.Services.TestProgramService.GetCurrentDutIndex();

            start_time = DateTime.Now.Millisecond;
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Stop Logging " + start_time);

            if (this.AttemptCount == string.Empty)
            {
                attemptLoop = 10;
            }
            else
            {
                attemptLoop = Convert.ToInt32(this.AttemptCount);
            }

            List<byte> writeData = new List<byte>();
            writeData.Add(0x00);
            writeData.Add(0x16);
            if (dutID == 0)
            {
                writeData.Add(0x0);
            }
            else if (dutID == 1)
            {
                writeData.Add(0x1);
            }

            Prime.Services.TesterService.WriteI2cData(this.PinName, deviceAddressWrite, writeData, stop);
            Prime.Services.ConsoleService.PrintDebug("Stop Logging command sent");

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

            List<byte> fpgaStat = new List<byte>();

            fpgaStat.Add(0x00);
            fpgaStat.Add(0x17);
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
                Prime.Services.ConsoleService.PrintDebug("FPGA Data " + readFpgaData.Sum(x => Convert.ToInt32(x)));
                if (readFpgaData.Sum(x => Convert.ToInt32(x)) == 102)
                {
                    Prime.Services.ConsoleService.PrintDebug("Successful tansition to SNOOP_A");
                    readFpgaData.RemoveRange(0, 2);
                    Prime.Services.TesterService.WriteI2cData(this.PinName, deviceAddressWrite, fpgaStat, stop);
                    Thread.Sleep(fPGAWait);
                    readFpgaData = Prime.Services.TesterService.ReadI2cData(this.PinName, deviceAddressRead, readCount);
                    Prime.Services.ConsoleService.PrintDebug("FPGA Data " + readFpgaData.Sum(x => Convert.ToInt32(x)));
                }

                if (readFpgaData.Sum(x => Convert.ToInt32(x)) == 136)
                {
                    Prime.Services.ConsoleService.PrintDebug("Stop Logging Transaction Successful on Site 1");

                    // ITUFF Logging
                    /*Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName);*/
                    Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
                    elapsed_time = DateTime.Now.Millisecond - start_time;
                    /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
                    Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");

                    return true;
                }
                else if (readFpgaData.Sum(x => Convert.ToInt32(x)) == 152)
                {
                    Prime.Services.ConsoleService.PrintDebug("Stop Logging Transaction Successful on Site 2");

                    // ITUFF Logging
                    /*Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName);*/
                    Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
                    elapsed_time = DateTime.Now.Millisecond - start_time;
                    /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
                    Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");

                    return true;
                }
                else
                {
                    Prime.Services.ConsoleService.PrintDebug("Stop Logging Transaction Unsuccessful");
                    if (i == attemptLoop - 1)
                    {
                        // ITUFF Logging
                        /*Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName);*/
                        Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
                        elapsed_time = DateTime.Now.Millisecond - start_time;
                        Prime.Services.ConsoleService.PrintDebug("Time Stamp Elapsed Stop Logging " + elapsed_time);
                        /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
                        Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");

                        return false;
                    }
                }
            }

            // ITUFF Logging
            /*Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName);*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
            elapsed_time = DateTime.Now.Millisecond - start_time;
            /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
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
                List<uint> idS = Prime.Services.TestProgramService.GetDefinedDutsIndex();
                Prime.Services.ConsoleService.PrintDebug("Dut Index " + dutID);
                Prime.Services.ConsoleService.PrintDebug("Begin Execution");
                Dictionary<string, string> timeOutSetting = new Dictionary<string, string>();
                timeOutSetting.Add("TimeOut", "10s");
                Prime.Services.PinService.SetPinAttributeValues(this.PinName, timeOutSetting);

                mut.WaitOne(); // Set mutex to execute one site at a time.
                this.preTime = DateTime.Now.Millisecond - start_time;
                Prime.Services.ConsoleService.PrintDebug("Time Stamp Stop Logging " + this.preTime);
                if (this.ForceFlow == "False")
                {
                    if (this.StopLogging())
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
                    if (this.StopLogging())
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
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Elapsed Program MCU " + elapsed_time);
            /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");
            Prime.Services.ConsoleService.PrintDebug("Flow Port 4");
            return 4;
        }
    }
}