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

namespace DUSTI_Configure
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

    public class DUSTI_Configure : Prime.TestMethods.TestMethodBase
    {
        private static Mutex mut = new Mutex();
        private int preTime;
        private int xmlFlag;
        private int calibrateFlag;
        private int mcuResetFlag;
        private List<byte> writeContent = new List<byte>();

        /// <summary>
        /// Gets or sets the XML Input file.
        /// </summary>
        public Prime.TestMethods.TestMethodsParams.String XMLInputFile { get; set; }

        /// <summary>
        /// Gets or Sets PLIST Load option.  Not required.
        /// </summary>
        public Prime.TestMethods.TestMethodsParams.String PlistOption { get; set; }

        /// <summary>
        /// Gets or Sets Levels Load option.  Not required.
        /// </summary>
        public Prime.TestMethods.TestMethodsParams.String LevelsOption { get; set; }

        /// <summary>
        /// Gets or Sets Timings Load option.  Not required.
        /// </summary>
        public Prime.TestMethods.TestMethodsParams.String TimingsOption { get; set; }

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

        /// <summary>
        /// Gets or sets  Reset of the MCU.  True or False.
        /// </summary>
        public Prime.TestMethods.TestMethodsParams.String McuReset { get; set; }

        /// <summary>
        /// Gets or sets Sleep service.
        /// </summary>
        public ISleepService ISleep { get; set; } = new SleepService();

        /// <inheritdoc />
        public override void Verify()
        {
            if (this.XMLInputFile == string.Empty)
            {
                Prime.Services.ConsoleService.PrintDebug("No XML Input. \n");
                throw new ArgumentException("There is no XML specified.\n");
            }
            else if (this.PlistOption == string.Empty)
            {
                Prime.Services.ConsoleService.PrintDebug("PList is Empty. \n");
                throw new ArgumentException("There is no PList specified.\n");
            }
            else if (this.LevelsOption == string.Empty)
            {
                Prime.Services.ConsoleService.PrintDebug("Levels is Empty. \n");
                throw new ArgumentException("There are no Levels specified.\n");
            }
            else if (this.TimingsOption == string.Empty)
            {
                Prime.Services.ConsoleService.PrintDebug("Timings is Empty. \n");
                throw new ArgumentException("There are no Timings specified.\n");
            }
            else if (this.PinName == string.Empty)
            {
                Prime.Services.ConsoleService.PrintDebug("Pin Name is Empty. \n");
                throw new ArgumentException("There is no Pin Name specified.\n");
            }
            else if (this.ForceFlow != "False" && this.ForceFlow != "True")
            {
                Prime.Services.ConsoleService.PrintDebug("Force Flow is not set to True or False. \n");
                throw new ArgumentException("Force Flow is not set to True or False.\n");
            }
            else if (this.McuReset != "False" && this.McuReset != "True")
            {
                Prime.Services.ConsoleService.PrintDebug("Reset is not set to True or False. \n");
                throw new ArgumentException("Reset is not set to True or False.\n");
            }
            else
            {
                if (this.ParseXML())
                {
                    Prime.Services.ConsoleService.PrintDebug("Great Success!!!. \n");
                }
            }
        }

        /// <summary>
        /// Sets the ack time and the frequency for the I2C bus.
        /// </summary>
        /// <returns>
        /// True when complete.
        /// </returns>
        public bool SetTimeAndDelay()
        {
            Dictionary<string, string> pinFrequency = new Dictionary<string, string>();
            Dictionary<string, string> pinDelay = new Dictionary<string, string>();
            pinFrequency.Add("ClockRate", "100KHz");
            pinDelay.Add("TimeOut", "10");
            Prime.Services.PinService.SetPinAttributeValues(this.PinName, pinFrequency);
            Prime.Services.PinService.SetPinAttributeValues(this.PinName, pinDelay);
            return true;
        }

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
                int summation = readDataAtte.Sum(x => Convert.ToInt32(x));
                Prime.Services.ConsoleService.PrintDebug("Read Value Verify I2C " + summation);
            }
            catch (Prime.Base.Exceptions.FatalException)
            {
                Prime.Services.ConsoleService.PrintDebug("Exception Caught");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Lookup proper site and return the associated value.
        /// </summary>
        /// <returns>
        /// The value for the IP address.
        /// </returns>
        public bool ResetMCU()
        {
            int i;
            byte deviceAddressWrite = 0x2C << 1;
            List<byte> resetValue = new List<byte>();
            bool stop = true;
            int attemptLoop = 0;
            int waitingTime = 3;
            int fPGAWait = 3;
            int start_time, elapsed_time;

            start_time = DateTime.Now.Millisecond;
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Reset MCU " + start_time);
            this.calibrateFlag = 0;
            this.mcuResetFlag = 0;
            this.xmlFlag = 0;

            this.ISleep.Sleep(300);
            resetValue.Insert(0, 0x00);
            resetValue.Insert(1, 0x18);
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
                Prime.Services.TesterService.WriteI2cData(this.PinName, deviceAddressWrite, resetValue, stop);
                this.ISleep.Sleep(waitingTime);
            }

            this.ISleep.Sleep(15000);

            // ITUFF Logging - not currently working.
            /*Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName);*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF INFO 2_tname_testtime_" + this.InstanceName);
            elapsed_time = DateTime.Now.Millisecond - start_time;
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Elapsed Reset MCU " + elapsed_time);
            /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF INFO 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");
            return true;
        }

        /// <summary>
        /// Lookup proper site and return the associated value.
        /// </summary>
        /// <returns>
        /// The value for the IP address.
        /// </returns>
        public string SiteLookup()
        {
            string retVal;
            var site = Prime.Services.UserVarService.GetStringValue("SCVars", "SC_SITEID");
            if (site == "A101")
            {
                retVal = "21";
            }
            else if (site == "A201")
            {
                retVal = "22";
            }
            else if (site == "A301")
            {
                retVal = "23";
            }
            else if (site == "A401")
            {
                retVal = "24";
            }
            else if (site == "B101")
            {
                retVal = "25";
            }
            else if (site == "B201")
            {
                retVal = "26";
            }
            else if (site == "B301")
            {
                retVal = "27";
            }
            else if (site == "B401")
            {
                retVal = "28";
            }
            else if (site == "C101")
            {
                retVal = "29";
            }
            else if (site == "C201")
            {
                retVal = "30";
            }
            else if (site == "C301")
            {
                retVal = "31";
            }
            else if (site == "C401")
            {
                retVal = "32";
            }
            else if (site == "D101")
            {
                retVal = "33";
            }
            else if (site == "D201")
            {
                retVal = "34";
            }
            else if (site == "D301")
            {
                retVal = "35";
            }
            else if (site == "D401")
            {
                retVal = "36";
            }
            else if (site == "E101")
            {
                retVal = "37";
            }
            else if (site == "E201")
            {
                retVal = "38";
            }
            else if (site == "E301")
            {
                retVal = "39";
            }
            else if (site == "E401")
            {
                retVal = "40";
            }
            else
            {
                retVal = "00";
            }

            return retVal;
        }

        /// <summary>
        /// Convert Value converst the string collected into two bytes.
        /// </summary>
        /// /// <param name="collected">Raw data passed in to convert.</param>
        /// <returns>
        /// See above.
        /// </returns>
        public byte[] ConvertValue(string collected)
        {
            Prime.Services.ConsoleService.PrintDebug("Converting Values \n");
            int inputValue = Convert.ToInt16(collected);
            Prime.Services.ConsoleService.PrintDebug("Integer Value " + inputValue);
            string hexValue = inputValue.ToString("X");
            Prime.Services.ConsoleService.PrintDebug("Hex Value " + hexValue);
            byte[] lengthVal = new byte[2];
            int localLen = hexValue.Length;
            Prime.Services.ConsoleService.PrintDebug("Length  " + localLen);

            switch (localLen)
            {
                case 1:
                    lengthVal[0] = Convert.ToByte(hexValue, 16);
                    lengthVal[1] = 0X00;
                    break;
                case 2:
                    lengthVal[0] = Convert.ToByte(hexValue, 16);
                    lengthVal[1] = 0X00;
                    break;
                case 3:
                    lengthVal[0] = Convert.ToByte(hexValue.Substring(1, 2), 16);
                    lengthVal[1] = Convert.ToByte(hexValue.Substring(0, 1), 16);
                    break;
                case 4:
                    lengthVal[0] = Convert.ToByte(hexValue.Substring(2, 2), 16);
                    lengthVal[1] = Convert.ToByte(hexValue.Substring(0, 2), 16);
                    break;
            }

            Prime.Services.ConsoleService.PrintDebug("Finished");
            return lengthVal;
        }

        /// <summary>
        /// Intiilizes the MAX3107.
        /// </summary>
        /// /// <returns>
        /// Return true if passes.
        /// </returns>
        public bool InitMax()
        {
            // Initialize variable to all 0x00 for all bytes .
            List<byte> maxReg = new List<byte>();
            List<byte> lcrReg = new List<byte>();
            List<byte> gpioReg = new List<byte>();
            List<byte> gpioData = new List<byte>();
            List<byte> pllReg = new List<byte>();
            List<byte> brgReg = new List<byte>();
            List<byte> divLsbReg = new List<byte>();
            List<byte> divMsbReg = new List<byte>();
            List<byte> clkSrcReg = new List<byte>();
            List<byte> mode2Reg = new List<byte>();
            List<byte> mode1Reg = new List<byte>();
            List<byte> resetReg = new List<byte>();
            List<byte> iRQtReg = new List<byte>();
            List<byte> lsrIntReg = new List<byte>();
            List<byte> spclChrReg = new List<byte>();
            List<byte> stsIntReg = new List<byte>();
            byte writeAddress = 0x2C << 1;
            byte readAddress = 0x2C << 1 | 1;
            int start_time, elapsed_time;

            start_time = DateTime.Now.Millisecond;
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Init Max " + start_time);

            List<byte> readData = Prime.Services.TesterService.ReadI2cData(this.PinName, readAddress, 2);
            int summation = readData.Sum(x => Convert.ToInt32(x));
            Prime.Services.ConsoleService.PrintDebug("Read Value " + summation);

            // Initialize the registers to write to the MAX3107
            lcrReg.Add(0x0B); // LCR Adddress.
            lcrReg.Add(0x03); // LCR Data.
            gpioReg.Add(0x18); // GPIO Config Address.
            gpioReg.Add(0x00); // GPIO Config Data;
            gpioData.Add(0x19); // GPIO Data Address.
            gpioData.Add(0xFC); // GPIO Data Data;
            pllReg.Add(0x1A); // PLL Address.
            pllReg.Add(0x44); // PLL Data.
            brgReg.Add(0x1B); // BRG Address.
            brgReg.Add(0x00); // BRG Data.
            divLsbReg.Add(0x1C); // DIVLSB Address.
            divLsbReg.Add(0x03); // DIVLSB Data.
            divMsbReg.Add(0x1D); // DIVMSB Address.
            divMsbReg.Add(0x00); // DIVMSB Data.
            clkSrcReg.Add(0x1E); // CLKSRC Address.
            clkSrcReg.Add(0x14); // CLKSRC Data.
            mode1Reg.Add(0x09); // Mode2 Address.
            mode1Reg.Add(0x00); // Mode2 Data.
            iRQtReg.Add(0x01); // IRQ Address.
            iRQtReg.Add(0x00); // IRQ Data.
            lsrIntReg.Add(0x03); // LSR Address.
            lsrIntReg.Add(0x00); // LSR Data.
            spclChrReg.Add(0x05); // Special Character Address.
            spclChrReg.Add(0x00); // Special Character Data.
            stsIntReg.Add(0x07);  // Sts Int Enable Address.
            stsIntReg.Add(0x00); // Sts Int Enable Data.

            Prime.Services.ConsoleService.PrintDebug("Initializing MAX3107");
            mode2Reg.Add(0x0A); // Mode2 Address.
            mode2Reg.Add(0x01); // Mode2 Data.
            Prime.Services.TesterService.WriteI2cData(this.PinName, writeAddress, mode2Reg, true); // Reset the MAX3107
            Prime.Services.ConsoleService.PrintDebug("ReadValue");
            maxReg.Add(0x1F); // Revision Address.
            Prime.Services.TesterService.WriteI2cData(this.PinName, writeAddress, maxReg, true); // Set read address for the revision.
            resetReg.Add(0x0A); // Mode2 Address.
            resetReg.Add(0x00); // Mode2 Data.
            Prime.Services.TesterService.WriteI2cData(this.PinName, writeAddress, resetReg, true); // Out of reset the MAX3107

            Prime.Services.TesterService.WriteI2cData(this.PinName, writeAddress, lcrReg, true); // Program LCR
            Prime.Services.TesterService.WriteI2cData(this.PinName, writeAddress, pllReg, true); // Program PLL Config
            Prime.Services.TesterService.WriteI2cData(this.PinName, writeAddress, brgReg, true); // Program BRG Config
            Prime.Services.TesterService.WriteI2cData(this.PinName, writeAddress, divLsbReg, true); // Program Div LSB
            Prime.Services.TesterService.WriteI2cData(this.PinName, writeAddress, divMsbReg, true); // Program Div MSB
            Prime.Services.TesterService.WriteI2cData(this.PinName, writeAddress, clkSrcReg, true); // Program CLKSRC
            Prime.Services.TesterService.WriteI2cData(this.PinName, writeAddress, gpioReg, true); // Program GPIO Config
            Prime.Services.TesterService.WriteI2cData(this.PinName, writeAddress, gpioData, true); // Program GPIO Data

            // ITUFF Logging
            Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
            elapsed_time = DateTime.Now.Millisecond - start_time;
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Elapsed Init Max " + elapsed_time);
            /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");

            return true;
        }

        /// <summary>
        /// Calibration Routine.
        /// </summary>
        /// /// <returns>
        /// Calibrates the MCU.
        /// </returns>
        public bool CalibrateMCU()
        {
            int i;
            byte[] result = new byte[1];
            byte deviceAddressWrite = 0x2C << 1; // Using device address 0xA with first bit set to write (0)
            byte deviceAddressRead = 0x2C << 1 | 1; // Using device address 0xA with first bit set to write (0)
            bool stop = true;
            int attemptLoop = 0;
            int waitingTime = 3;
            int fPGAWait = 3;
            int start_time, elapsed_time;
            uint dutID = Prime.Services.TestProgramService.GetCurrentDutIndex();

            start_time = DateTime.Now.Millisecond;
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Calibrate MCU Begin " + start_time);

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

            List<byte> writeData = new List<byte>();
            writeData.Add(0x00);
            writeData.Add(0x14);
            if (dutID == 0)
            {
                writeData.Add(0x0);
            }
            else if (dutID == 1)
            {
                writeData.Add(0x1);
            }

            Prime.Services.TesterService.WriteI2cData(this.PinName, deviceAddressWrite, writeData, stop);
            this.ISleep.Sleep(waitingTime);
            if (this.PlistOption != string.Empty && this.LevelsOption != string.Empty && this.TimingsOption != string.Empty)
            {
                Prime.Services.FunctionalService.CreateNoCaptureTest(this.PlistOption, this.LevelsOption, this.TimingsOption, string.Empty); // PRIME 5 and Up*/
                /*Prime.Services.FunctionalService.CreateNoCaptureTest(this.PlistOption, this.LevelsOption, this.TimingsOption); // PRIME 4 and lower.*/
            }

            this.ISleep.Sleep(waitingTime);
            Prime.Services.ConsoleService.PrintDebug("Begin MCU Training.");
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

            Prime.Services.ConsoleService.PrintDebug("FPGA Status Command " + fpgaStat.ElementAt(2) + " And " + fpgaStat.ElementAt(1) + " And " + fpgaStat.ElementAt(0));
            for (i = 0; i < attemptLoop; i++)
            {
                Prime.Services.TesterService.WriteI2cData(this.PinName, deviceAddressWrite, fpgaStat, stop);
                this.ISleep.Sleep(fPGAWait);
                int readCount = 2;
                List<byte> readFpgaData = Prime.Services.TesterService.ReadI2cData(this.PinName, deviceAddressRead, readCount);
                Prime.Services.ConsoleService.PrintDebug("Calibrate MCU Data " + readFpgaData.Sum(x => Convert.ToInt32(x)));
                if ((readFpgaData.Sum(x => Convert.ToInt32(x)) == 85) || (readFpgaData.Sum(x => Convert.ToInt32(x)) == 101))
                {
                    Prime.Services.ConsoleService.PrintDebug("Training In Progress, Check Loop " + i);
                    elapsed_time = DateTime.Now.Millisecond - start_time;
                    /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
                    Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");
                    return true;
                }
                else if (readFpgaData.Sum(x => Convert.ToInt32(x)) == 99)
                {
                    Prime.Services.ConsoleService.PrintDebug("Training Failed, Check Loop " + i);

                    // ITUFF Logging
                    /*Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName);*/
                    Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
                    elapsed_time = DateTime.Now.Millisecond - start_time;
                    /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
                    Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");

                    return false;
                }

                Prime.Services.ConsoleService.PrintDebug("FPGA Transaction Unsuccessful");
                if (i == attemptLoop - 1)
                {
                    // ITUFF Logging
                    /*Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName);*/
                    Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
                    elapsed_time = DateTime.Now.Millisecond - start_time;
                    /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
                    Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");

                    return false;
                }
            }

            // ITUFF Logging
            /*Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName);*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
            elapsed_time = DateTime.Now.Millisecond - start_time;
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Elapsed Calibrate MCU " + elapsed_time);
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
            int start_time;
            start_time = DateTime.Now.Millisecond;
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Execution Begin " + start_time);

            if (this.VerifyI2CPresent())
            {
                Prime.Services.ConsoleService.PrintDebug("Pre Execution XML Flag " + this.xmlFlag);
                Prime.Services.ConsoleService.PrintDebug("Pre Execution Calibrate Flag " + this.calibrateFlag);
                Prime.Services.ConsoleService.PrintDebug("Pre Execution MCU Flag " + this.mcuResetFlag);

                this.SetTimeAndDelay();

                uint dutID = Prime.Services.TestProgramService.GetCurrentDutIndex();
                List<uint> idS = Prime.Services.TestProgramService.GetDefinedDutsIndex();
                Prime.Services.ConsoleService.PrintDebug("Dut Index " + dutID);
                Prime.Services.ConsoleService.PrintDebug("Begin Execution");
                if (this.McuReset == "True")
                {
                    this.mcuResetFlag = 0;
                }

                mut.WaitOne(); // Run one site at at time in order to set flags and run only once.
                if (this.McuReset == "True" && this.mcuResetFlag == 0)
                {
                    if (this.InitMax())
                    {
                        this.ResetMCU();
                        this.mcuResetFlag = 1; // Set so reset only runs once.
                        Prime.Services.ConsoleService.PrintDebug("Reset of the MAX3107 was successful.");
                        this.ParseXML();
                        Prime.Services.ConsoleService.PrintDebug("XML Parse completed after Reset.");
                    }
                    else
                    {
                        Prime.Services.ConsoleService.PrintDebug("Reset of the MAX3107 was unsuccessful.");
                        return 0;
                    }
                }

                mut.ReleaseMutex(); // Release Mutex.
                this.preTime = DateTime.Now.Millisecond - start_time;
                /*this.xmlFlag = 0;*/
                Prime.Services.ConsoleService.PrintDebug("Time Stamp Program MCU " + this.preTime);
                Prime.Services.ConsoleService.PrintDebug("XML Flag " + this.xmlFlag);

                if (this.ForceFlow == "False")
                {
                    mut.WaitOne(); // XML parsing only needs to occur once.
                    if (this.xmlFlag == 0)
                    {
                        Prime.Services.ConsoleService.PrintDebug("Sending XML data to MCU");
                        if (this.SendCommand())
                        {
                            Prime.Services.ConsoleService.PrintDebug("MCU Programming Successful, FPGA in correct state.");
                            this.xmlFlag = 1; // Set to disable the xml being parsed more than once.
                            Prime.Services.ConsoleService.PrintDebug("Calibrating MCU on DUT 1");
                            if (this.CalibrateMCU())
                            {
                                Prime.Services.ConsoleService.PrintDebug("Calibration successful");
                                this.calibrateFlag = 1;
                                mut.ReleaseMutex(); // Release Mutex.
                                this.xmlFlag = 1; // Set to disable the xml being parsed more than once.
                                return 1;
                            }
                            else
                            {
                                Prime.Services.ConsoleService.PrintDebug("Calibration unsuccessful");
                                mut.ReleaseMutex(); // Release Mutex.
                                return 0;
                            }
                        }
                        else
                        {
                            Prime.Services.ConsoleService.PrintDebug("MCU Programing unsuccessful, FPGA may not be in correct state.");
                            this.xmlFlag = 0; // Set to disable the xml being parsed more than once.
                            mut.ReleaseMutex(); // Release Mutex.
                            return 0;
                        }
                    }
                    else if (this.xmlFlag == 1)
                    {
                        Prime.Services.ConsoleService.PrintDebug("Not programming MCU, already programmed prior");
                        mut.ReleaseMutex(); // Release Mutex.
                        return 3;
                    }
                    else
                    {
                        Prime.Services.ConsoleService.PrintDebug("Error, XML data not present");
                        mut.ReleaseMutex(); // Release Mutex.
                        return 0;
                    }
                }
                else if (this.ForceFlow == "True")
                {
                    mut.WaitOne(); // XML parsing only needs to occur once.
                    if (this.xmlFlag == 0)
                    {
                        Prime.Services.ConsoleService.PrintDebug("Sending XML data to MCU");
                        if (this.SendCommand())
                        {
                            Prime.Services.ConsoleService.PrintDebug("MCU Programing Successful, FPGA in correct state.");
                            this.xmlFlag = 1; // Set to disable the xml being parsed more than once.
                            Prime.Services.ConsoleService.PrintDebug("Calibrating MCU on DUT 1");
                            if (this.CalibrateMCU())
                            {
                                Prime.Services.ConsoleService.PrintDebug("Calibration successful");
                                this.calibrateFlag = 1;
                                mut.ReleaseMutex(); // Release Mutex.
                                this.xmlFlag = 1; // Set to disable the xml being parsed more than once.
                                return 1;
                            }
                            else
                            {
                                Prime.Services.ConsoleService.PrintDebug("Calibration unsuccessful");
                                mut.ReleaseMutex(); // Release Mutex.
                                return 0;
                            }
                        }
                        else
                        {
                            Prime.Services.ConsoleService.PrintDebug("MCU Programing  unsuccessful, FPGA may not be in correct state.");
                            this.xmlFlag = 0; // Set to disable the xml being parsed more than once.
                            mut.ReleaseMutex(); // Release Mutex.
                            return 0;
                        }
                    }
                    else if (this.xmlFlag == 1)
                    {
                        Prime.Services.ConsoleService.PrintDebug("Not Programing MCU , already programed prior");
                        mut.ReleaseMutex(); // Release Mutex.
                        return 3;
                    }
                    else
                    {
                        Prime.Services.ConsoleService.PrintDebug("Error, XML data not present");
                        mut.ReleaseMutex(); // Release Mutex.
                        return 0;
                    }
                }
                else
                {
                    mut.ReleaseMutex();
                    return 0;
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

        /// <summary>
        /// Paresing the XML.
        /// </summary>
        /// /// <returns>
        /// Returns true if the paring is executed correctly.
        /// </returns>
        public bool ParseXML()
        {
            // Variable declarations
            /*List<byte> writeContent = new List<byte>();*/
            string checkSumStrip = " ";
            bool xmlDebugFlag = true;

            var start_time = DateTime.Now.Millisecond;
            var xmlPath = Prime.Services.FileService.GetFile(this.XMLInputFile);
            XmlDocument xMlDoc = new XmlDocument();
            /*if (!Prime.Services.FileService.FileExists(this.XMLInputFile))
            {
                throw new ArgumentException($"{nameof(this.XMLInputFile)} =[{this.XMLInputFile}] does not exist.");
            }*/

            /*string filePath = Prime.Services.FileService.GetFile(this.XMLInputFile);*/
            xMlDoc.Load(xmlPath);
            uint dutID = Prime.Services.TestProgramService.GetCurrentDutIndex();

            XmlNodeList nodeCount = xMlDoc.GetElementsByTagName("Dts");
            string patternMatchModeSwitch = string.Empty;

            foreach (XmlNode node in xMlDoc.DocumentElement)
            {
                int i;
                switch (node.Name)
                {
                    case "Product":
                        break;

                    case "PatternMatchMode":
                        patternMatchModeSwitch = node.InnerText;

                        if (node.InnerText == "1")
                        {
                            // Initialize variable to all 0x00 for all bytes .
                            for (i = 0; i < 627; i++)
                            {
                                this.writeContent.Insert(i, 0x00);

                                // this.writeContent.Add(0x00);
                            }

                            this.writeContent.Insert(0, 0x00); // Read Buffer for MAX3170
                            this.writeContent.Insert(1, 0x11); // I2C Command header
                            this.writeContent.Insert(2, Convert.ToByte(dutID)); // DUT ID
                            this.writeContent.Insert(3, 0x02); // Total Number Of Bytes, Upper Byte
                            this.writeContent.Insert(4, 0x74); // Total Number Of Bytes, Lower Byte
                            this.writeContent.Insert(5, 0x00); // Package #
                            this.writeContent.Insert(6, 0x01); // Pattern Match Mode
                        }
                        else if (node.InnerText == "2")
                        {
                            // Initialize variable to all 0x00 for all bytes .
                            for (i = 0; i < 758; i++)
                            {
                                this.writeContent.Insert(i, 0x00);

                                // this.writeContent.Add(0x00);
                            }

                            this.writeContent.Insert(0, 0x00); // Read Buffer for MAX3170
                            this.writeContent.Insert(1, 0x12); // I2C Command header
                            this.writeContent.Insert(2, Convert.ToByte(dutID)); // DUT ID
                            this.writeContent.Insert(3, 0x02); // Total Number Of Bytes, Upper Byte
                            this.writeContent.Insert(4, 0xF7); // Total Number Of Bytes, Lower Byte
                            this.writeContent.Insert(5, 0x00); // Package #
                            this.writeContent.Insert(6, 0x02); // Pattern Match Mode
                        }
                        else if (node.InnerText == "3")
                        {
                            // Initialize variable to all 0x00 for all bytes .
                            for (i = 0; i < 693; i++)
                            {
                                this.writeContent.Insert(i, 0x00);

                                // this.writeContent.Add(0x00);
                            }

                            this.writeContent.Insert(0, 0x00); // Read Buffer for MAX3170
                            this.writeContent.Insert(1, 0x13); // I2C Command header
                            this.writeContent.Insert(2, Convert.ToByte(dutID)); // DUT ID
                            this.writeContent.Insert(3, 0x02); // Total Number Of Bytes, Upper Byte
                            this.writeContent.Insert(4, 0xB6); // Total Number Of Bytes, Lower Byte
                            this.writeContent.Insert(5, 0x00); // Package #
                            this.writeContent.Insert(6, 0x03); // Pattern Match Mode
                        }

                        break;

                    // Setup Parallelism and Reserved Bits.
                    case "CellParallelism":
                        if (node.Attributes[0].InnerText == "hex")
                        {
                            int parallel = Convert.ToInt32(node.InnerText, 16);
                            if (xmlDebugFlag)
                            {
                                Prime.Services.ConsoleService.PrintDebug("Parallelism Decimal Value " + parallel);
                            }

                            this.writeContent.Insert(7, Convert.ToByte(parallel));
                        }
                        else
                        {
                            int parallel = Convert.ToInt16(node.InnerText);

                            this.writeContent.Insert(7, Convert.ToByte(parallel));
                        }

                        break;
                    case "Reserved0": // For Future Use
                        if (node.Attributes[0].InnerText == "hex")
                        {
                            int reserved = Convert.ToInt32(node.InnerText, 16);
                            if (xmlDebugFlag)
                            {
                                Prime.Services.ConsoleService.PrintDebug("Parallelism Decimal Value " + reserved);
                            }

                            this.writeContent.Insert(8, Convert.ToByte(reserved));
                        }
                        else
                        {
                            int reserved = Convert.ToInt16(node.InnerText);

                            this.writeContent.Insert(8, Convert.ToByte(reserved));
                        }

                        break;
                    case "Reserved1": // For Future Use
                        if (node.Attributes[0].InnerText == "hex")
                        {
                            int reserved = Convert.ToInt32(node.InnerText, 16);
                            if (xmlDebugFlag)
                            {
                                Prime.Services.ConsoleService.PrintDebug("Parallelism Decimal Value " + reserved);
                            }

                            this.writeContent.Insert(9, Convert.ToByte(reserved));
                        }
                        else
                        {
                            int reserved = Convert.ToInt16(node.InnerText);

                            this.writeContent.Insert(9, Convert.ToByte(reserved));
                        }

                        break;
                    case "Reserved2": // For Future Use
                        if (node.Attributes[0].InnerText == "hex")
                        {
                            int reserved = Convert.ToInt32(node.InnerText, 16);
                            if (xmlDebugFlag)
                            {
                                Prime.Services.ConsoleService.PrintDebug("Parallelism Decimal Value " + reserved);
                            }

                            this.writeContent.Insert(10, Convert.ToByte(reserved));
                        }
                        else
                        {
                            int reserved = Convert.ToInt16(node.InnerText);

                            this.writeContent.Insert(10, Convert.ToByte(reserved));
                        }

                        break;
                    case "Reserved3": // For Future Use
                        if (node.Attributes[0].InnerText == "hex")
                        {
                            int reserved = Convert.ToInt32(node.InnerText, 16);
                            if (xmlDebugFlag)
                            {
                                Prime.Services.ConsoleService.PrintDebug("Parallelism Decimal Value " + reserved);
                            }

                            this.writeContent.Insert(11, Convert.ToByte(reserved));
                        }
                        else
                        {
                            int reserved = Convert.ToInt16(node.InnerText);

                            this.writeContent.Insert(11, Convert.ToByte(reserved));
                        }

                        break;

                    // Setting up for TEL subnet.
                    case "Networking":
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            int j;
                            if (child.Name == "IpConfig")
                            {
                                if (xmlDebugFlag)
                                {
                                    Prime.Services.ConsoleService.PrintDebug(child.Attributes[0].InnerText);
                                    Prime.Services.ConsoleService.PrintDebug(child.Attributes[1].InnerText);
                                }

                                string subnet = child.Attributes[0].InnerText;
                                string iPAdd = child.Attributes[1].InnerText;

                                var temp = subnet.Split(new[] { '.' }, StringSplitOptions.None);
                                int[] subnetMask = new int[4];
                                for (j = 0; j < 4; j++)
                                {// Writing Subnet Mask to list
                                    subnetMask[j] = Convert.ToInt16(temp[j]);

                                    string tempHexString = subnetMask[j].ToString("X");
                                    if (xmlDebugFlag)
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Subnet " + tempHexString);
                                    }

                                    byte tempByte = Convert.ToByte(tempHexString, 16);

                                    this.writeContent.Insert(j + 12, tempByte); // Pattern Match Mode
                                }

                                int[] iPAddy = new int[4];
                                temp = iPAdd.Split(new[] { '.' }, StringSplitOptions.None);
                                for (j = 0; j < 4; j++)
                                { // Writing IP address to list.
                                    if (j == 3)
                                    {
                                        iPAddy[j] = Convert.ToInt16(this.SiteLookup());
                                    }
                                    else
                                    {
                                        iPAddy[j] = Convert.ToInt16(temp[j]);
                                    }

                                    string tempHexString = iPAddy[j].ToString("X");
                                    if (xmlDebugFlag)
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("IP " + tempHexString);
                                    }

                                    byte tempByte = Convert.ToByte(tempHexString, 16);
                                    this.writeContent.Insert(j + 16, tempByte); // Pattern Match Mode
                                }
                            }
                            else if (child.Name == "TidiConnection")
                            {
                                string server = child.Attributes[0].InnerText;
                                string port = child.Attributes[1].InnerText;

                                int[] serverAddy = new int[4];
                                var temp = server.Split(new[] { '.' }, StringSplitOptions.None);
                                for (j = 0; j < 4; j++)
                                {// Writing IP address to list.
                                    serverAddy[j] = Convert.ToInt16(temp[j]);
                                    string tempHexString = serverAddy[j].ToString("X");
                                    if (xmlDebugFlag)
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Server " + tempHexString);
                                    }

                                    byte tempByte = Convert.ToByte(tempHexString, 16);
                                    this.writeContent.Insert(j + 20, tempByte); // Pattern Match Mode
                                }

                                // Writing the port Data.
                                int[] portNum = new int[2];
                                int len = port.Length;

                                string portBit1 = port.Substring(0, 2);
                                string portBit2 = port.Substring(2, 2);

                                this.writeContent.Insert(24, Convert.ToByte(portBit1, 16));
                                this.writeContent.Insert(25, Convert.ToByte(portBit2, 16));
                            }
                        }

                        break;

                    case "SnoopyDigitalPotentiometers":

                        foreach (XmlNode child in node.ChildNodes)
                        {
                            if (child.Name == "Dpot")
                            {
                                string dutValue = "0";
                                int pot_Num = XmlConvert.ToInt16(child.Attributes[0].Value);
                                string potValue = child.Attributes[2].Value;
                                string potValue1 = potValue.TrimStart('0', 'x');
                                string dutValueTemp = child.Attributes[1].Value;
                                if (dutValueTemp.Length > 4)
                                {
                                    int strLn = dutValueTemp.Length - 4;
                                    dutValue = dutValueTemp.Remove(4, strLn);
                                }

                                int localLen = potValue1.Length;
                                if (xmlDebugFlag)
                                {
                                    Prime.Services.ConsoleService.PrintDebug("Pot Number " + pot_Num + " Value " + potValue1);
                                    Prime.Services.ConsoleService.PrintDebug("Pot Number " + pot_Num + " Data Length " + localLen);
                                    Prime.Services.ConsoleService.PrintDebug("DUT Number " + dutValue);
                                }

                                /* int dutInt = 0;
                                 switch (dutValue)
                                 {
                                     case "DUT1":
                                         dutInt = 0;
                                         break;
                                     case "DUT2":
                                         dutInt = 12;
                                         break;
                                 }*/

                                string potVal1 = string.Empty;
                                string potVal2 = string.Empty;
                                if (xmlDebugFlag)
                                {
                                    Prime.Services.ConsoleService.PrintDebug(child.Attributes[2].InnerText);
                                }

                                switch (localLen)
                                {
                                    case 0:
                                        if (xmlDebugFlag)
                                        {
                                            Prime.Services.ConsoleService.PrintDebug("Default Value in Use");
                                        }

                                        break;
                                    case 1:
                                        if (xmlDebugFlag)
                                        {
                                            Prime.Services.ConsoleService.PrintDebug("SingleChar");
                                        }

                                        potVal1 = potValue1.Substring(0, 1);
                                        int decVal = Convert.ToInt32(potVal1, 16);
                                        if (xmlDebugFlag)
                                        {
                                            Prime.Services.ConsoleService.PrintDebug("FirstTapIRInstruction Decimal Value " + decVal);
                                        }

                                        this.writeContent.Insert(27 + (2 * (pot_Num - 1)), Convert.ToByte(decVal));
                                        break;
                                    case 2:
                                        if (xmlDebugFlag)
                                        {
                                            Prime.Services.ConsoleService.PrintDebug("TwoChar");
                                        }

                                        potVal1 = potValue1.Substring(0, 2);
                                        decVal = Convert.ToInt32(potVal1, 16);
                                        if (xmlDebugFlag)
                                        {
                                            Prime.Services.ConsoleService.PrintDebug("FirstTapIRInstruction Decimal Value " + decVal);
                                        }

                                        this.writeContent.Insert(27 + (2 * (pot_Num - 1)), Convert.ToByte(decVal));
                                        break;
                                    case 3:
                                        if (xmlDebugFlag)
                                        {
                                            Prime.Services.ConsoleService.PrintDebug("ThreeChar");
                                        }

                                        potVal1 = potValue1.Substring(0, 1);
                                        potVal2 = potValue1.Substring(1, 2);
                                        decVal = Convert.ToInt32(potVal1, 16);
                                        int decVal2 = Convert.ToInt32(potVal2, 16);
                                        if (xmlDebugFlag)
                                        {
                                            Prime.Services.ConsoleService.PrintDebug("FirstTapIRInstruction Decimal Value " + decVal + "_" + decVal2);
                                        }

                                        this.writeContent.Insert(26 + (2 * (pot_Num - 1)), Convert.ToByte(decVal));
                                        this.writeContent.Insert(27 + (2 * (pot_Num - 1)), Convert.ToByte(decVal2));
                                        break;
                                    case 4:
                                        if (xmlDebugFlag)
                                        {
                                            Prime.Services.ConsoleService.PrintDebug("FourChar");
                                        }

                                        potVal1 = potValue1.Substring(0, 2);
                                        potVal2 = potValue1.Substring(2, 2);
                                        decVal = Convert.ToInt32(potVal1, 16);
                                        decVal2 = Convert.ToInt32(potVal2, 16);
                                        if (xmlDebugFlag)
                                        {
                                            Prime.Services.ConsoleService.PrintDebug("FirstTapIRInstruction Decimal Value " + decVal + "_" + decVal2);
                                        }

                                        this.writeContent.Insert(26 + (2 * (pot_Num - 1)), Convert.ToByte(decVal));
                                        this.writeContent.Insert(27 + (2 * (pot_Num - 1)), Convert.ToByte(decVal2));
                                        break;
                                }
                            }
                            else if (child.Name == "TrainingStateDpots")
                            {
                                string trainingType = child.Attributes[0].Value;
                                if (child.Attributes[0].Value == "Static")
                                {
                                    this.writeContent.Insert(50, 0x00);
                                }
                                else if (child.Attributes[0].Value == "Dynamic")
                                {
                                    this.writeContent.Insert(50, 0x01);
                                }
                            }
                        }

                        break;

                    case "Mode":
                        int temporaryValue = 0;
                        var checkSumHex = " ";
                        var checkSumLen = 0;
                        if (patternMatchModeSwitch == "1" && node.Attributes[0].InnerText == "1")
                        {
                            if (xmlDebugFlag)
                            {
                                Prime.Services.ConsoleService.PrintDebug("Mode 1");
                                Prime.Services.ConsoleService.PrintDebug("Name " + node.Attributes[1].InnerText);
                            }

                            // Parse Through The Data for Write Prepparation
                            foreach (XmlNode child in node.ChildNodes)
                            {
                                if (child.Name == "TapIRInstructionLength")
                                {
                                    if (child.InnerText == string.Empty)
                                    {
                                        if (xmlDebugFlag)
                                        {
                                            Prime.Services.ConsoleService.PrintDebug("Using Default Values");
                                        }
                                    }
                                    else
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Programmed Value = " + child.InnerText);
                                        temporaryValue = Convert.ToInt16(child.InnerText);
                                        this.writeContent.Insert(51, Convert.ToByte(temporaryValue));
                                    }
                                }
                                else if (child.Name == "DtsDataSize")
                                {
                                    if (child.InnerText == string.Empty)
                                    {
                                        if (xmlDebugFlag)
                                        {
                                            Prime.Services.ConsoleService.PrintDebug("Using Default Values");
                                        }
                                    }
                                    else
                                    {
                                        if (xmlDebugFlag)
                                        {
                                            Prime.Services.ConsoleService.PrintDebug("Using Programmed Value = " + child.InnerText);
                                        }

                                        temporaryValue = Convert.ToInt16(child.InnerText);

                                        this.writeContent.Insert(52, Convert.ToByte(temporaryValue));
                                    }
                                }
                                else if (child.Name == "Dts")
                                {
                                    if (xmlDebugFlag)
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("DTS");
                                    }

                                    int dTS_Num = XmlConvert.ToInt16(child.Attributes[0].InnerText);
                                    if (xmlDebugFlag)
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("DTS Number " + dTS_Num);
                                    }

                                    foreach (XmlNode child1 in child.ChildNodes)
                                    {
                                        if (child1.Name == "TapIRInstruction")
                                        {
                                            if (child.InnerText == string.Empty)
                                            {
                                                if (xmlDebugFlag)
                                                {
                                                    Prime.Services.ConsoleService.PrintDebug("Using Default Values");
                                                }
                                            }
                                            else
                                            {
                                                int tapIRValue = Convert.ToInt16(child1.InnerText);
                                                if (xmlDebugFlag)
                                                {
                                                    Prime.Services.ConsoleService.PrintDebug("TapIRInstruction Decimal Value " + tapIRValue);
                                                }

                                                byte[] result = new byte[1];
                                                result = this.ConvertValue(child1.InnerText);
                                                this.writeContent.Insert(53 + (19 * (dTS_Num - 1)), result[1]);
                                                this.writeContent.Insert(54 + (19 * (dTS_Num - 1)), result[0]);
                                            }
                                        }
                                        else if (child1.Name == "Diode" && child1.Attributes[2].InnerText != string.Empty)
                                        {
                                            Prime.Services.ConsoleService.PrintDebug(child1.Attributes[2].InnerText);
                                            int diode_Num = XmlConvert.ToInt16(child1.Attributes[0].InnerText);
                                            int diodeLenVal = Convert.ToInt16(child1.Attributes[2].InnerText);
                                            Prime.Services.ConsoleService.PrintDebug("Diode" + child1.Attributes[2].InnerText + "Decimal Value " + diodeLenVal);
                                            byte[] result = new byte[1];
                                            result = this.ConvertValue(child1.Attributes[2].InnerText);
                                            this.writeContent.Insert((55 + (2 * (diode_Num - 1))) + (19 * (dTS_Num - 1)), result[1]);
                                            this.writeContent.Insert((56 + (2 * (diode_Num - 1))) + (19 * (dTS_Num - 1)), result[0]);
                                        }
                                    }
                                }
                            }

                            Prime.Services.ConsoleService.PrintDebug("Begin Parsing!!!");
                            int datCount = 0;
                            datCount = this.writeContent.Count;
                            Prime.Services.ConsoleService.PrintDebug("Content Count " + datCount);
                            int stripCount = 0;
                            stripCount = datCount - 627;
                            this.writeContent.RemoveRange(626, stripCount);
                            datCount = this.writeContent.Count;
                            Prime.Services.ConsoleService.PrintDebug("Total Count " + datCount);
                            int sumTotal = this.writeContent.Sum(x => Convert.ToInt32(x));
                            checkSumHex = sumTotal.ToString("X");
                            checkSumLen = checkSumHex.Length;
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
                            this.writeContent.Add(Convert.ToByte(checkSum));
                            datCount = this.writeContent.Count;
                            byte[] readWriteValue = new byte[datCount];
                            readWriteValue = this.writeContent.ToArray();
                            for (i = 0; i < datCount; i++)
                            {
                                Prime.Services.ConsoleService.PrintDebug("Content Dump " + i + " Value " + readWriteValue[i].ToString());
                            }
                        }
                        else if (patternMatchModeSwitch == "2" && node.Attributes[0].InnerText == "2")
                        {
                            Prime.Services.ConsoleService.PrintDebug("Mode 2");
                            Prime.Services.ConsoleService.PrintDebug("Name " + node.Attributes[1].InnerText);

                            // Parse Through The Data for Write Prepparation
                            foreach (XmlNode child in node.ChildNodes)
                            {
                                if (child.Name == "FirstTapIRInstructionLength")
                                {
                                    if (child.InnerText == string.Empty)
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Default Values");
                                    }
                                    else
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Programmed Value  IR LENGTH= " + child.InnerText);
                                        if (child.Attributes[0].InnerText == "hex")
                                        {
                                            int decVal = Convert.ToInt32(child.InnerText, 16);
                                            Prime.Services.ConsoleService.PrintDebug("FirstTapIRInstructionLength Decimal Value " + decVal);
                                            this.writeContent.Insert(51, Convert.ToByte(decVal));
                                        }
                                        else
                                        {
                                            temporaryValue = Convert.ToInt16(child.InnerText);

                                            this.writeContent.Insert(51, Convert.ToByte(temporaryValue));
                                        }
                                    }
                                }
                                else if (child.Name == "FirstTapDRLength")
                                {
                                    if (child.InnerText == string.Empty)
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Default Values");
                                    }
                                    else
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Programmed Value DR Length = " + child.InnerText);
                                        if (child.Attributes[0].InnerText == "hex")
                                        {
                                            int decVal = Convert.ToInt32(child.InnerText, 16);
                                            Prime.Services.ConsoleService.PrintDebug("FirstTapDRLength Decimal Value " + decVal);
                                            this.writeContent.Insert(52, Convert.ToByte(decVal));
                                        }
                                        else
                                        {
                                            temporaryValue = Convert.ToInt16(child.InnerText);

                                            this.writeContent.Insert(52, Convert.ToByte(temporaryValue));
                                        }
                                    }
                                }
                                else if (child.Name == "SecondTapIRInstructionLength")
                                {
                                    if (child.InnerText == string.Empty)
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Default Values");
                                    }
                                    else
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Programmed Value DR Length = " + child.InnerText);
                                        if (child.Attributes[0].InnerText == "hex")
                                        {
                                            int decVal = Convert.ToInt32(child.InnerText, 16);
                                            Prime.Services.ConsoleService.PrintDebug("SecondTapIRInstructionLength Decimal Value " + decVal);
                                            this.writeContent.Insert(53, Convert.ToByte(decVal));
                                        }
                                        else
                                        {
                                            temporaryValue = Convert.ToInt16(child.InnerText);

                                            this.writeContent.Insert(53, Convert.ToByte(temporaryValue));
                                        }
                                    }
                                }
                                else if (child.Name == "DtsDataSize")
                                {
                                    if (child.InnerText == string.Empty)
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Default Values");
                                    }
                                    else
                                    {
                                        if (child.Attributes[0].InnerText == "hex")
                                        {
                                            int decVal = Convert.ToInt32(child.InnerText, 16);
                                            Prime.Services.ConsoleService.PrintDebug("SecondTapIRInstructionLength Decimal Value " + decVal);
                                            this.writeContent.Insert(54, Convert.ToByte(decVal));
                                        }
                                        else
                                        {
                                            Prime.Services.ConsoleService.PrintDebug("Using Programmed Value DTS SIZE = " + child.InnerText);
                                            temporaryValue = Convert.ToInt16(child.InnerText);

                                            this.writeContent.Insert(54, Convert.ToByte(temporaryValue));
                                        }
                                    }
                                }
                                else if (child.Name == "Dts")
                                {
                                    Prime.Services.ConsoleService.PrintDebug("DTSSSSSSS");

                                    int dTS_Num = XmlConvert.ToInt16(child.Attributes[0].InnerText);
                                    Prime.Services.ConsoleService.PrintDebug("DTS Number " + dTS_Num);
                                    foreach (XmlNode child1 in child.ChildNodes)
                                    {
                                        if (child1.Name == "FirstTapIRInstruction")
                                        {
                                            int localLen = child1.InnerText.Length;
                                            string tapIRLength = child1.InnerText;
                                            string tapIRB1 = string.Empty;
                                            string tapIRB2 = string.Empty;
                                            Prime.Services.ConsoleService.PrintDebug(child1.InnerText);

                                            if (child1.Attributes[0].InnerText == "hex")
                                            {
                                                switch (localLen)
                                                {
                                                    case 0:
                                                        Prime.Services.ConsoleService.PrintDebug("Default Value in Use");
                                                        break;
                                                    case 1:
                                                        Prime.Services.ConsoleService.PrintDebug("SingleChar");
                                                        tapIRB1 = tapIRLength.Substring(0, 1);
                                                        int decVal = Convert.ToInt32(tapIRB1, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("FirstTapIRInstruction Decimal Value " + decVal);
                                                        this.writeContent.Insert(56 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal));
                                                        break;
                                                    case 2:
                                                        Prime.Services.ConsoleService.PrintDebug("TwoChar");
                                                        tapIRB1 = tapIRLength.Substring(0, 2);
                                                        decVal = Convert.ToInt32(tapIRB1, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("FirstTapIRInstruction Decimal Value " + decVal);
                                                        this.writeContent.Insert(56 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal));
                                                        break;
                                                    case 3:
                                                        Prime.Services.ConsoleService.PrintDebug("ThreeChar");
                                                        tapIRB1 = tapIRLength.Substring(0, 1);
                                                        tapIRB2 = tapIRLength.Substring(1, 2);
                                                        decVal = Convert.ToInt32(tapIRB1, 16);
                                                        int decVal2 = Convert.ToInt32(tapIRB2, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("FirstTapIRInstruction Decimal Value " + decVal + "_" + decVal2);
                                                        this.writeContent.Insert(55 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal));
                                                        this.writeContent.Insert(56 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal2));
                                                        break;
                                                    case 4:
                                                        Prime.Services.ConsoleService.PrintDebug("FourChar");
                                                        tapIRB1 = tapIRLength.Substring(0, 2);
                                                        tapIRB2 = tapIRLength.Substring(2, 2);
                                                        decVal = Convert.ToInt32(tapIRB1, 16);
                                                        decVal2 = Convert.ToInt32(tapIRB2, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("FirstTapIRInstruction Decimal Value " + decVal + "_" + decVal2);
                                                        this.writeContent.Insert(55 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal));
                                                        this.writeContent.Insert(56 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal2));
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                int localDecValue = 0;
                                                localDecValue = Convert.ToInt16(child1.InnerText);
                                                Prime.Services.ConsoleService.PrintDebug("FirstTapIRInstruction Decimal Value " + localDecValue);
                                                byte[] result = new byte[1];
                                                result = this.ConvertValue(child1.InnerText);
                                                this.writeContent.Insert(55 + (22 * (dTS_Num - 1)), result[0]);
                                                this.writeContent.Insert(56 + (22 * (dTS_Num - 1)), result[1]);
                                            }
                                        }
                                        else if (child1.Name == "FirstTapDRInstruction")
                                        {
                                            int localLen = child1.InnerText.Length;
                                            string tapDRLength = child1.InnerText;
                                            string tapDRB1 = string.Empty;
                                            string tapDRB2 = string.Empty;
                                            Prime.Services.ConsoleService.PrintDebug(child1.InnerText);

                                            if (child1.Attributes[0].InnerText == "hex")
                                            {
                                                switch (localLen)
                                                {
                                                    case 0:
                                                        Prime.Services.ConsoleService.PrintDebug("Default Value in Use");
                                                        break;
                                                    case 1:
                                                        Prime.Services.ConsoleService.PrintDebug("SingleChar");
                                                        tapDRB1 = tapDRLength.Substring(0, 1);
                                                        int decVal = Convert.ToInt32(tapDRB1, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("FirstTapDRInstruction Decimal Value " + decVal);
                                                        this.writeContent.Insert(58 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal));
                                                        break;
                                                    case 2:
                                                        Prime.Services.ConsoleService.PrintDebug("TwoChar");
                                                        tapDRB1 = tapDRLength.Substring(0, 2);
                                                        decVal = Convert.ToInt32(tapDRB1, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("FirstTapDRInstruction Decimal Value " + decVal);
                                                        this.writeContent.Insert(58 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal));
                                                        break;
                                                    case 3:
                                                        Prime.Services.ConsoleService.PrintDebug("ThreeChar");
                                                        tapDRB1 = tapDRLength.Substring(0, 1);
                                                        tapDRB2 = tapDRLength.Substring(1, 2);
                                                        decVal = Convert.ToInt32(tapDRB1, 16);
                                                        int decVal2 = Convert.ToInt32(tapDRB2, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("FirstTapDRInstruction Decimal Value " + decVal + "_" + decVal2);
                                                        this.writeContent.Insert(57 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal));
                                                        this.writeContent.Insert(58 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal2));
                                                        break;
                                                    case 4:
                                                        Prime.Services.ConsoleService.PrintDebug("FourChar");
                                                        tapDRB1 = tapDRLength.Substring(0, 2);
                                                        tapDRB2 = tapDRLength.Substring(2, 2);
                                                        decVal = Convert.ToInt32(tapDRB1, 16);
                                                        decVal2 = Convert.ToInt32(tapDRB2, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("FirstTapDRInstruction Decimal Value " + decVal + "_" + decVal2);
                                                        this.writeContent.Insert(57 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal));
                                                        this.writeContent.Insert(58 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal2));
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                int localDecValue = 0;
                                                localDecValue = Convert.ToInt16(child1.InnerText);
                                                Prime.Services.ConsoleService.PrintDebug("FirstTapDRInstruction Decimal Value " + localDecValue);
                                                byte[] result = new byte[1];
                                                result = this.ConvertValue(child1.InnerText);
                                                this.writeContent.Insert(57 + (22 * (dTS_Num - 1)), result[1]);
                                                this.writeContent.Insert(58 + (22 * (dTS_Num - 1)), result[0]);
                                            }
                                        }
                                        else if (child1.Name == "SecondTapIRInstruction")
                                        {
                                            int localLen = child1.InnerText.Length;
                                            string tapIRLength = child1.InnerText;
                                            string tapIRB1 = string.Empty;
                                            string tapIRB2 = string.Empty;
                                            Prime.Services.ConsoleService.PrintDebug(child1.InnerText);

                                            if (child1.Attributes[0].InnerText == "hex")
                                            {
                                                switch (localLen)
                                                {
                                                    case 0:
                                                        Prime.Services.ConsoleService.PrintDebug("Default Value in Use");
                                                        break;
                                                    case 1:
                                                        Prime.Services.ConsoleService.PrintDebug("SingleChar");
                                                        tapIRB1 = tapIRLength.Substring(0, 1);
                                                        int decVal = Convert.ToInt32(tapIRB1, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("SecondTapIRInstruction Decimal Value " + decVal);
                                                        this.writeContent.Insert(60 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal));
                                                        break;
                                                    case 2:
                                                        Prime.Services.ConsoleService.PrintDebug("TwoChar");
                                                        tapIRB1 = tapIRLength.Substring(0, 2);
                                                        decVal = Convert.ToInt32(tapIRB1, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("SecondTapIRInstruction Decimal Value " + decVal);
                                                        this.writeContent.Insert(60 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal));
                                                        break;
                                                    case 3:
                                                        Prime.Services.ConsoleService.PrintDebug("ThreeChar");
                                                        tapIRB1 = tapIRLength.Substring(0, 1);
                                                        tapIRB2 = tapIRLength.Substring(1, 2);
                                                        decVal = Convert.ToInt32(tapIRB1, 16);
                                                        int decVal2 = Convert.ToInt32(tapIRB2, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("SecondTapIRInstruction Decimal Value " + decVal + "_" + decVal2);
                                                        this.writeContent.Insert(59 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal));
                                                        this.writeContent.Insert(60 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal2));
                                                        break;
                                                    case 4:
                                                        Prime.Services.ConsoleService.PrintDebug("FourChar");
                                                        tapIRB1 = tapIRLength.Substring(0, 2);
                                                        tapIRB2 = tapIRLength.Substring(2, 2);
                                                        decVal = Convert.ToInt32(tapIRB1, 16);
                                                        decVal2 = Convert.ToInt32(tapIRB2, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("SecondTapIRInstruction Decimal Value " + decVal + "_" + decVal2);
                                                        this.writeContent.Insert(59 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal));
                                                        this.writeContent.Insert(60 + (22 * (dTS_Num - 1)), Convert.ToByte(decVal2));
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                int localDecValue = 0;
                                                localDecValue = Convert.ToInt16(child1.InnerText);
                                                Prime.Services.ConsoleService.PrintDebug("SecondTapIRInstruction Decimal Value " + localDecValue);
                                                byte[] result = new byte[1];
                                                result = this.ConvertValue(child1.InnerText);
                                                this.writeContent.Insert(59 + (22 * (dTS_Num - 1)), result[1]);
                                                this.writeContent.Insert(60 + (22 * (dTS_Num - 1)), result[0]);
                                            }
                                        }
                                        else if (child1.Name == "Diode" && (child1.Attributes[2].InnerText != string.Empty))
                                        {
                                            Prime.Services.ConsoleService.PrintDebug(child1.Attributes[2].InnerText);
                                            int diode_Num = XmlConvert.ToInt16(child1.Attributes[0].InnerText);
                                            Prime.Services.ConsoleService.PrintDebug("Diode Number " + diode_Num);
                                            int localLen = child1.Attributes[2].InnerText.Length;
                                            string diodeLength = child1.Attributes[2].InnerText;
                                            int diodeValue = Convert.ToInt16(diodeLength);
                                            Prime.Services.ConsoleService.PrintDebug(" TAP DR Offset " + diode_Num + "_" + diodeValue);
                                            string diodeB1 = string.Empty;
                                            string diodeB2 = string.Empty;

                                            byte[] result = new byte[1];
                                            result = this.ConvertValue(child1.Attributes[2].InnerText);
                                            this.writeContent.Insert(61 + (22 * (dTS_Num - 1)) + (2 * (diode_Num - 1)), result[1]);
                                            this.writeContent.Insert(62 + (22 * (dTS_Num - 1)) + (2 * (diode_Num - 1)), result[0]);
                                        }
                                    }
                                }
                            }

                            // 741 total bytes to be sent, need to trim the excess.
                            Prime.Services.ConsoleService.PrintDebug("Begin Parsing!!!");
                            int datCount = 0;
                            datCount = this.writeContent.Count;
                            Prime.Services.ConsoleService.PrintDebug("Content Count " + datCount);
                            int stripCount = 0;
                            stripCount = datCount - 759;
                            this.writeContent.RemoveRange(758, stripCount);
                            datCount = this.writeContent.Count;
                            Prime.Services.ConsoleService.PrintDebug("Total Count " + datCount);
                            int sumTotal = this.writeContent.Sum(x => Convert.ToInt32(x));
                            checkSumHex = sumTotal.ToString("X");
                            checkSumLen = checkSumHex.Length;
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
                            this.writeContent.Add(Convert.ToByte(checkSum));
                            datCount = this.writeContent.Count;
                            byte[] readWriteValue = new byte[datCount];
                            readWriteValue = this.writeContent.ToArray();
                            for (i = 0; i < datCount; i++)
                            {
                                Prime.Services.ConsoleService.PrintDebug("Content Dump " + i + " Value " + readWriteValue[i].ToString());
                            }
                        }
                        else if (patternMatchModeSwitch == "3" && node.Attributes[0].InnerText == "3")
                        {
                            Prime.Services.ConsoleService.PrintDebug("Mode 3");
                            Prime.Services.ConsoleService.PrintDebug("Name " + node.Attributes[1].InnerText);

                            // Parse Through The Data for Write Prepparation
                            foreach (XmlNode child in node.ChildNodes)
                            {
                                if (child.Name == "TapHeaderLength")
                                {
                                    if (child.InnerText == string.Empty)
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Default Values");
                                    }
                                    else
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Programmed Value  IR LENGTH= " + child.InnerText);

                                        temporaryValue = Convert.ToInt16(child.InnerText);

                                        this.writeContent.Insert(51, Convert.ToByte(temporaryValue));
                                    }
                                }
                                else if (child.Name == "DtsIdLength")
                                {
                                    if (child.InnerText == string.Empty)
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Default Values");
                                    }
                                    else
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Programmed Value DR Length = " + child.InnerText);

                                        temporaryValue = Convert.ToInt16(child.InnerText);

                                        this.writeContent.Insert(52, Convert.ToByte(temporaryValue));
                                    }
                                }
                                else if (child.Name == "DtsDataSize")
                                {
                                    if (child.InnerText == string.Empty)
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Default Values");
                                    }
                                    else
                                    {
                                        Prime.Services.ConsoleService.PrintDebug("Using Programmed Value DTS SIZE = " + child.InnerText);
                                        temporaryValue = Convert.ToInt16(child.InnerText);

                                        this.writeContent.Insert(53, Convert.ToByte(temporaryValue));
                                    }
                                }
                                else if (child.Name == "Packet")
                                {
                                    Prime.Services.ConsoleService.PrintDebug("Packetssssssssss");

                                    int packetNum = XmlConvert.ToInt16(child.Attributes[0].InnerText);
                                    foreach (XmlNode child1 in child.ChildNodes)
                                    {
                                        if (child1.Name == "HeaderId")
                                        {
                                            int localLen = child1.InnerText.Length;
                                            string headerLength = child1.InnerText;
                                            string headerB1 = string.Empty;
                                            string headerB2 = string.Empty;
                                            Prime.Services.ConsoleService.PrintDebug(child1.InnerText);

                                            if (child1.Attributes[0].InnerText == "hex")
                                            {
                                                switch (localLen)
                                                {
                                                    case 0:
                                                        Prime.Services.ConsoleService.PrintDebug("Default Value in Use");
                                                        break;
                                                    case 1:
                                                        Prime.Services.ConsoleService.PrintDebug("SingleChar");
                                                        headerB1 = headerLength.Substring(0, 1);
                                                        int decVal = Convert.ToInt32(headerB1, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("HeaderId Decimal Value " + decVal);
                                                        this.writeContent.Insert(55 + (21 * (packetNum - 1)), Convert.ToByte(decVal));
                                                        break;
                                                    case 2:
                                                        Prime.Services.ConsoleService.PrintDebug("TwoChar");
                                                        headerB1 = headerLength.Substring(0, 2);
                                                        decVal = Convert.ToInt32(headerB1, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("HeaderId Decimal Value " + decVal);
                                                        this.writeContent.Insert(55 + (21 * (packetNum - 1)), Convert.ToByte(decVal));
                                                        break;
                                                    case 3:
                                                        Prime.Services.ConsoleService.PrintDebug("ThreeChar");
                                                        headerB1 = headerLength.Substring(0, 1);
                                                        headerB2 = headerLength.Substring(1, 2);
                                                        decVal = Convert.ToInt32(headerB1, 16);
                                                        int decVal2 = Convert.ToInt32(headerB2, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("HeaderId Decimal Value " + decVal + "_" + decVal2);
                                                        this.writeContent.Insert(54 + (21 * (packetNum - 1)), Convert.ToByte(decVal));
                                                        this.writeContent.Insert(55 + (21 * (packetNum - 1)), Convert.ToByte(decVal2));
                                                        break;
                                                    case 4:
                                                        Prime.Services.ConsoleService.PrintDebug("FourChar");
                                                        headerB1 = headerLength.Substring(0, 2);
                                                        headerB2 = headerLength.Substring(2, 2);
                                                        decVal = Convert.ToInt32(headerB1, 16);
                                                        decVal2 = Convert.ToInt32(headerB2, 16);
                                                        Prime.Services.ConsoleService.PrintDebug("HeaderId Decimal Value " + decVal + "_" + decVal2);
                                                        this.writeContent.Insert(54 + (21 * (packetNum - 1)), Convert.ToByte(decVal));
                                                        this.writeContent.Insert(55 + (21 * (packetNum - 1)), Convert.ToByte(decVal2));
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                int localDecValue = 0;
                                                localDecValue = Convert.ToInt16(child1.InnerText);
                                                Prime.Services.ConsoleService.PrintDebug("HeaderId Decimal Value " + localDecValue);
                                                byte[] result = new byte[1];
                                                result = this.ConvertValue(child1.InnerText);
                                                this.writeContent.Insert(54 + (21 * (packetNum - 1)), result[1]);
                                                this.writeContent.Insert(55 + (21 * (packetNum - 1)), result[0]);
                                            }
                                        }
                                        else if (child1.Name == "DtsIdName")
                                        {
                                            int localLen = child1.InnerText.Length;
                                            string dtsNameLength = child1.InnerText;
                                            string dtsNmB1 = string.Empty;
                                            string dtsNmB2 = string.Empty;
                                            Prime.Services.ConsoleService.PrintDebug(child1.InnerText);

                                            if (child1.Attributes.Count != 0)
                                            {
                                                if (child1.Attributes[0].InnerText == "hex")
                                                {
                                                    switch (localLen)
                                                    {
                                                        case 0:
                                                            Prime.Services.ConsoleService.PrintDebug("Default Value in Use");
                                                            break;
                                                        case 1:
                                                            Prime.Services.ConsoleService.PrintDebug("SingleChar");
                                                            dtsNmB1 = dtsNameLength.Substring(0, 1);
                                                            int decVal = Convert.ToInt32(dtsNmB1, 16);
                                                            Prime.Services.ConsoleService.PrintDebug("HeaderId Decimal Value " + decVal);
                                                            this.writeContent.Insert(57 + (21 * (packetNum - 1)), Convert.ToByte(decVal));
                                                            break;
                                                        case 2:
                                                            Prime.Services.ConsoleService.PrintDebug("TwoChar");
                                                            dtsNmB1 = dtsNameLength.Substring(0, 2);
                                                            decVal = Convert.ToInt32(dtsNmB1, 16);
                                                            Prime.Services.ConsoleService.PrintDebug("HeaderId Decimal Value " + decVal);
                                                            this.writeContent.Insert(57 + (21 * (packetNum - 1)), Convert.ToByte(decVal));
                                                            break;
                                                        case 3:
                                                            Prime.Services.ConsoleService.PrintDebug("ThreeChar");
                                                            dtsNmB1 = dtsNameLength.Substring(0, 1);
                                                            dtsNmB2 = dtsNameLength.Substring(1, 2);
                                                            decVal = Convert.ToInt32(dtsNmB1, 16);
                                                            int decVal2 = Convert.ToInt32(dtsNmB2, 16);
                                                            Prime.Services.ConsoleService.PrintDebug("HeaderId Decimal Value " + decVal + "_" + decVal2);
                                                            this.writeContent.Insert(56 + (21 * (packetNum - 1)), Convert.ToByte(decVal));
                                                            this.writeContent.Insert(57 + (21 * (packetNum - 1)), Convert.ToByte(decVal2));
                                                            break;
                                                        case 4:
                                                            Prime.Services.ConsoleService.PrintDebug("FourChar");
                                                            dtsNmB1 = dtsNameLength.Substring(0, 2);
                                                            dtsNmB2 = dtsNameLength.Substring(2, 2);
                                                            decVal = Convert.ToInt32(dtsNmB1, 16);
                                                            decVal2 = Convert.ToInt32(dtsNmB2, 16);
                                                            Prime.Services.ConsoleService.PrintDebug("HeaderId Decimal Value " + decVal + "_" + decVal2);
                                                            this.writeContent.Insert(56 + (21 * (packetNum - 1)), Convert.ToByte(decVal));
                                                            this.writeContent.Insert(57 + (21 * (packetNum - 1)), Convert.ToByte(decVal2));
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    int localDecValue = 0;
                                                    localDecValue = Convert.ToInt16(child1.InnerText);
                                                    Prime.Services.ConsoleService.PrintDebug("HeaderId Decimal Value " + localDecValue);
                                                    byte[] result = new byte[1];
                                                    result = this.ConvertValue(child1.InnerText);
                                                    this.writeContent.Insert(56 + (21 * (packetNum - 1)), result[1]);
                                                    this.writeContent.Insert(57 + (21 * (packetNum - 1)), result[0]);
                                                }
                                            }
                                        }
                                        else if (child1.Name == "Diode" && (child1.Attributes[2].InnerText != string.Empty))
                                        {
                                            byte[] result = new byte[1];
                                            result = this.ConvertValue(child1.Attributes[2].InnerText);
                                            this.writeContent.Insert(58 + (21 * (packetNum - 1)), result[1]);
                                            this.writeContent.Insert(59 + (21 * (packetNum - 1)), result[0]);
                                        }
                                    }
                                }
                            }

                            int datCount = 0;
                            datCount = this.writeContent.Count;
                            Prime.Services.ConsoleService.PrintDebug("Content Count " + datCount);
                            int stripCount = 0;
                            stripCount = datCount - 693;
                            this.writeContent.RemoveRange(692, stripCount);
                            datCount = this.writeContent.Count;
                            Prime.Services.ConsoleService.PrintDebug("Total Count " + datCount);
                            int sumTotal = this.writeContent.Sum(x => Convert.ToInt32(x));
                            checkSumHex = sumTotal.ToString("X");
                            checkSumLen = checkSumHex.Length;
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
                            this.writeContent.Add(Convert.ToByte(checkSum));
                            datCount = this.writeContent.Count;
                            datCount = this.writeContent.Count;
                            byte[] readWriteValue = new byte[datCount];
                            readWriteValue = this.writeContent.ToArray();
                            for (i = 0; i < datCount; i++)
                            {
                                Prime.Services.ConsoleService.PrintDebug("Content Dump " + i + " Value " + readWriteValue[i].ToString());
                            }
                        }

                        break;
                }
            }

            // ITUFF Logging
            /*Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName);*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
            var elapsed_time = DateTime.Now.Millisecond - start_time;
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Elapsed Program MCU " + elapsed_time);
            /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");
            return true;
        }

        /// <summary>
        /// Sends the command to the MCU.
        /// </summary>
        /// /// <returns>
        /// Returns true if executed correctly.
        /// </returns>
        public bool SendCommand()
        {
            int i;
            int start_time, elapsed_time;
            byte deviceAddressWrite = 0x2C << 1; // Using device address 0xA with first bit set to write (0)
            byte deviceAddressRead = 0x2C << 1 | 1; // Using device address 0xA with first bit set to write (0)
            bool stop = true;
            int attemptLoop = 0;
            int waitingTime = 3;
            int fPGAWait = 3;
            start_time = DateTime.Now.Millisecond;
            uint dutID = Prime.Services.TestProgramService.GetCurrentDutIndex();
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Program MCU " + start_time);

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
                Prime.Services.TesterService.WriteI2cData(this.PinName, deviceAddressWrite, this.writeContent, stop);
                this.ISleep.Sleep(waitingTime);
                int readCount = 1;
                List<byte> readData = Prime.Services.TesterService.ReadI2cData(this.PinName, deviceAddressRead, readCount);
                int summation = readData.Sum(x => Convert.ToInt32(x));
                Prime.Services.ConsoleService.PrintDebug("Read Value " + summation);
                if (readData.Sum(x => Convert.ToInt32(x)) == 6)
                {
                    Prime.Services.ConsoleService.PrintDebug("Mode programming was successful");
                    goto Next;
                }
                else
                {
                    Prime.Services.ConsoleService.PrintDebug("Mode programming was not successful");
                    if (i == attemptLoop)
                    {
                        return false;
                    }
                }
            }

            goto Next;
        Next:
            this.ISleep.Sleep(fPGAWait);
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
                this.ISleep.Sleep(fPGAWait);
                int readCount = 2;
                List<byte> readFpgaData = Prime.Services.TesterService.ReadI2cData(this.PinName, deviceAddressRead, readCount);
                Prime.Services.ConsoleService.PrintDebug("FPGA Data Send Command " + readFpgaData.ElementAt(0) + " And " + readFpgaData.ElementAt(1));
                Prime.Services.ConsoleService.PrintDebug("FPGA Data Send Command " + readFpgaData.Sum(x => Convert.ToInt32(x)));
                if (readFpgaData.Sum(x => Convert.ToInt32(x)) == 84)
                {
                    Prime.Services.ConsoleService.PrintDebug("FPGA Transaction Successful");

                    // ITUFF Logging
                    /*Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName);*/
                    Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
                    elapsed_time = DateTime.Now.Millisecond - start_time;
                    Prime.Services.ConsoleService.PrintDebug("Time Stamp Elapsed Program MCU " + elapsed_time);
                    /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
                    Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");
                    return true;
                }
                else
                {
                    Prime.Services.ConsoleService.PrintDebug("FPGA Transaction Unsuccessful");
                    if (i == attemptLoop - 1)
                    {
                        return false;
                    }
                }
            }

            // ITUFF Logging
            /*Prime.Services.DatalogService.WriteToItuff("2_tname_testtime_" + this.InstanceName);*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_tname_testtime_" + this.InstanceName);
            elapsed_time = DateTime.Now.Millisecond - start_time;
            Prime.Services.ConsoleService.PrintDebug("Time Stamp Elapsed Program MCU " + elapsed_time);
            /*Prime.Services.DatalogService.WriteToItuff("2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");*/
            Prime.Services.ConsoleService.PrintDebug("ITUFF DEBUG 2_strgval_pre_" + this.preTime + "mS_main_" + elapsed_time + "mS");
            return false;
        }
    }
}