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

namespace VminForwardingSaveAsUpsGsdsTC.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.Base.Exceptions;
    using Prime.BinMatrixService;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.SharedStorageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class VminForwardingSaveAsUpsGsdsTC_UnitTest
    {
        private static double InvalidDataThrowException { get; } = -7777f;

        private Dictionary<string, List<string>> FrequencyMap { get; set; }

        /// <summary>
        /// Setup the generic mocks for the test.
        /// </summary>
        [TestInitialize]
        public void SetupMocks()
        {
            // Mock debug and error messages just to print to the console.
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) => Console.WriteLine(s));
            consoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback((string msg, int line, string n, string src) => Console.WriteLine($"ERROR: {msg}"));
            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // mock the full bin matrix frequency table.
            this.FrequencyMap = new Dictionary<string, List<string>>
            {
                { "CLR_F1_FREQ", new List<string> { "0.4", "0.4", "0.4", "0.4" } },
                { "CLR_F2_FREQ", new List<string> { "0.8", "0.8", "0.8", "0.8" } },
                { "CLR_F3_FREQ", new List<string> { "1.8", "1.8", "1.8", "1.8" } },
                { "CLR_F4_FREQ", new List<string> { "3", "3", "3", "3" } },
                { "CLR_F5_FREQ", new List<string> { "3.6", "3.6", "3.6", "3.6" } },
                { "CLR_F6_FREQ", new List<string> { "4", "4", "4", "4" } },
                { "CR_F1_FREQ", new List<string> { "0.4", "0.4", "0.4", "0.4" } },
                { "CR_F2_FREQ", new List<string> { "1.2", "1.2", "1.2", "1.2" } },
                { "CR_F3_FREQ", new List<string> { "2.2", "2.2", "2.2", "2.2" } },
                { "CR_F4_FREQ", new List<string> { "3.4", "3.4", "3.4", "3.4" } },
                { "CR_F5_FREQ", new List<string> { "4.2", "4.2", "4.2", "4.2" } },
                { "CR_F6_FREQ", new List<string> { "5", "4.8", "4.7", "4.4" } },
                { "CR_F7_FREQ", new List<string> { "5.5", "99", "99", "99" } },
                { "CRF_F1_FREQ", new List<string> { "0.4", "0.4", "0.4", "0.4" } },
                { "CRF_F2_FREQ", new List<string> { "1.2", "1.2", "1.2", "1.2" } },
                { "CRF_F3_FREQ", new List<string> { "2.2", "2.2", "2.2", "2.2" } },
                { "CRF_F4_FREQ", new List<string> { "3.4", "3.4", "3.4", "3.4" } },
                { "CRF_F5_FREQ", new List<string> { "4.2", "4.2", "4.2", "4.2" } },
                { "CRF_F6_FREQ", new List<string> { "5.05", "4.85", "4.7", "4.4" } },
                { "CRX2_F1_FREQ", new List<string> { "0.4", "0.4", "0.4", "0.4" } },
                { "CRX2_F2_FREQ", new List<string> { "1.2", "1.2", "1.2", "1.2" } },
                { "CRX2_F3_FREQ", new List<string> { "2.2", "2.2", "2.2", "2.2" } },
                { "CRX2_F4_FREQ", new List<string> { "3.4", "3.4", "3.4", "3.4" } },
                { "CRX2_F5_FREQ", new List<string> { "4.2", "4.2", "4.2", "4.2" } },
                { "CRX2_F6_FREQ", new List<string> { "4.9", "4.7", "4.6", "4.3" } },
                { "CRX3_F1_FREQ", new List<string> { "0.4", "0.4", "0.4", "0.4" } },
                { "CRX3_F2_FREQ", new List<string> { "1.2", "1.2", "1.2", "1.2" } },
                { "CRX3_F3_FREQ", new List<string> { "2.2", "2.2", "2.2", "2.2" } },
                { "CRX3_F4_FREQ", new List<string> { "3.4", "3.4", "3.4", "3.4" } },
                { "CRX3_F5_FREQ", new List<string> { "4.2", "4.2", "4.2", "4.2" } },
                { "CRX3_F6_FREQ", new List<string> { "4.8", "4.6", "4.5", "4.3" } },
                { "GTS_F1_FREQ", new List<string> { "0.3", "0.3", "0.3", "0.3" } },
                { "GTS_F2_FREQ", new List<string> { "0.6", "0.6", "0.6", "0.6" } },
                { "GTS_F3_FREQ", new List<string> { "0.9", "0.9", "0.9", "0.9" } },
                { "GTS_F4_FREQ", new List<string> { "1.1", "1.1", "1.1", "1.1" } },
                { "GTS_F5_FREQ", new List<string> { "1.35", "1.35", "1.3", "1.3" } },
                { "GTSM_F5_FREQ", new List<string> { "1.1", "1.1", "1.1", "1.1" } },
                { "GTSM_F3_FREQ", new List<string> { "0.9", "0.9", "0.9", "0.9" } },
                { "GTSM_F2_FREQ", new List<string> { "0.6", "0.6", "0.6", "0.6" } },
                { "GTSM_F1_FREQ", new List<string> { "0.3", "0.3", "0.3", "0.3" } },
                { "SACD_F1_FREQ", new List<string> { ".312", ".312", ".312", ".312" } },
                { "SACD_F2_FREQ", new List<string> { ".562", ".562", ".562", ".562" } },
                { "SACD_F4_FREQ", new List<string> { ".662", ".662", ".662", ".662" } },
                { "SAF_F1_FREQ", new List<string> { ".533", ".533", ".533", ".533" } },
                { "SAF_F5_FREQ", new List<string> { ".800", ".800", ".800", ".800" } },
                { "SAIS_F1_FREQ", new List<string> { ".200", ".200", ".200", ".200" } },
                { "SAIS_F5_FREQ", new List<string> { ".533", ".533", ".533", ".533" } },
                { "SAPS_F1_FREQ", new List<string> { ".200", ".200", ".200", ".200" } },
                { "SAPS_F3_FREQ", new List<string> { ".400", ".400", ".400", ".400" } },
                { "SAPS_F5_FREQ", new List<string> { "1.000", "1.000", "1.000", "1.000" } },
                { "SAQ_F1_FREQ", new List<string> { "1.100", "1.100", "1.100", "1.100" } },
                { "SAQ_F4_FREQ", new List<string> { "2.200", "2.200", "2.200", "2.200" } },
                { "SAQ_F6_FREQ", new List<string> { "2.700", "2.700", "2.700", "2.700" } },
            };

            // convert the frequency map into Hz... too lazy to rebuild the map.
            foreach (var constant in this.FrequencyMap.Keys)
            {
                for (var flow = 0; flow < 4; flow++)
                {
                    this.FrequencyMap[constant][flow] = (double.Parse(this.FrequencyMap[constant][flow]) * 1e9).ToString();
                }
            }

            var binMatrixMock = new Mock<IBinMatrixService>(MockBehavior.Strict);
            binMatrixMock.Setup(o => o.GetNumberOfFlows()).Returns(4);
            /*
            foreach (var freqConst in this.FrequencyMap.Keys)
            {
                for (var i = 0; i < 4; i++)
                {
                    var specInfo = new Mock<ISpecInfo>(MockBehavior.Strict);
                    specInfo.Setup(o => o.GetData()).Returns(this.FrequencyMap[freqConst][i]);
                    specInfo.Setup(o => o.GetMultiplier()).Returns("1000000000");

                    binMatrixMock.Setup(o => o.GetSpecInfo(i + 1, freqConst)).Returns(specInfo.Object);
                }
            } */
            Prime.Services.BinMatrixService = binMatrixMock.Object;
        }

        /// <summary>
        /// Simple testcase to check an exception in GetFrequencyForCorner.
        /// </summary>
        [TestMethod]
        public void GetFrequency_Exception()
        {
            var token_FAST_STC_V = "CLR=F1:|||0.500";

            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(o => o.GetStringRowFromTable("FAST_STC_V", Context.DUT)).Returns(token_FAST_STC_V);
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            // Mock in Prime Data.
            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            vminFactoryMock.Setup(o => o.GetInstanceNamesForDomain("CLR")).Throws(new FatalException("No domain called [CLR]."));
            DDG.VminForwarding.Service = vminFactoryMock.Object;

            Assert.ThrowsException<FatalException>(() => VminForwardingSaveAsUpsGsdsTC.UPS_Data.BuildFromEvgTokens(4));
            sharedServiceMock.VerifyAll();
            vminFactoryMock.VerifyAll();
        }

        /// <summary>
        /// Validate the UPS_Data class and its methods when there is no EVG data..
        /// </summary>
        [TestMethod]
        public void UPS_Data_Build_Fail()
        {
            var token_FAST_CORNERS = "x";
            var token_FAST_STC_V = "x";
            var token_FAST_UPSVF = "x";
            var token_FAST_UPSVFPASSFLOW = "x";
            /* var tokenMerged_FAST_CORNERS = "CR=206:|||_CR=205:|||_CR=204:|||_CR=203:|||_CR=202:|||_CR=201:|||,CRF=226:|||1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999_CRF=225:|||_CRF=224:|||_CRF=223:|||_CRF=222:|||_CRF=221:|||0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999,CLR=216:|||_CLR=215:|||1.200_CLR=214:|||_CLR=213:|||_CLR=212:|||_CLR=211:|||0.400,CRX2=256:|||_CRX2=255:|||_CRX2=254:|||_CRX2=253:|||_CRX2=252:|||_CRX2=251:|||,CRX3=266:|||_CRX3=265:|||_CRX3=264:|||_CRX3=263:|||_CRX3=262:|||_CRX3=261:|||,GTS=305:|||_GTS=304:|||_GTS=303:|||_GTS=302:|||_GTS=301:|||,SAQ=406:|||_SAQ=404:|||_SAQ=401:|||,SAPS=515:|||_SAPS=513:|||_SAPS=511:|||,SAIS=525:|||_SAIS=521:|||,SAF=535:|||_SAF=531:|||,SACD=504:|||_SACD=502:|||_SACD=501:|||,GTSM=325:|||_GTSM=323:|||_GTSM=322:|||_GTSM=321:|||"; */
            var tokenMerged_FAST_CORNERS = "CR=999:|||_CR=999:|||_CR=999:|||_CR=999:|||_CR=999:|||_CR=999:|||,CRF=999:|||1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999_CRF=999:|||_CRF=999:|||_CRF=999:|||_CRF=999:|||_CRF=999:|||0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999,CLR=999:|||_CLR=999:|||1.200_CLR=999:|||_CLR=999:|||_CLR=999:|||_CLR=999:|||0.400,CRX2=999:|||_CRX2=999:|||_CRX2=999:|||_CRX2=999:|||_CRX2=999:|||_CRX2=999:|||,CRX3=999:|||_CRX3=999:|||_CRX3=999:|||_CRX3=999:|||_CRX3=999:|||_CRX3=999:|||,GTS=999:|||_GTS=999:|||_GTS=999:|||_GTS=999:|||_GTS=999:|||,SAQ=999:|||_SAQ=999:|||_SAQ=999:|||,SAPS=999:|||_SAPS=999:|||_SAPS=999:|||,SAIS=999:|||_SAIS=999:|||,SAF=999:|||_SAF=999:|||,SACD=999:|||_SACD=999:|||_SACD=999:|||,GTSM=999:|||_GTSM=999:|||_GTSM=999:|||_GTSM=999:|||";
            var tokenMerged_FAST_STC_V = "CR=F6:|||_CR=F5:|||_CR=F4:|||_CR=F3:|||_CR=F2:|||_CR=F1:|||,CRF=F6:|||1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999_CRF=F5:|||_CRF=F4:|||_CRF=F3:|||_CRF=F2:|||_CRF=F1:|||0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999,CLR=F6:|||_CLR=F5:|||1.200_CLR=F4:|||_CLR=F3:|||_CLR=F2:|||_CLR=F1:|||0.400,CRX2=F6:|||_CRX2=F5:|||_CRX2=F4:|||_CRX2=F3:|||_CRX2=F2:|||_CRX2=F1:|||,CRX3=F6:|||_CRX3=F5:|||_CRX3=F4:|||_CRX3=F3:|||_CRX3=F2:|||_CRX3=F1:|||,GTS=F5:|||_GTS=F4:|||_GTS=F3:|||_GTS=F2:|||_GTS=F1:|||,SAQ=F6:|||_SAQ=F4:|||_SAQ=F1:|||,SAPS=F5:|||_SAPS=F3:|||_SAPS=F1:|||,SAIS=F5:|||_SAIS=F1:|||,SAF=F5:|||_SAF=F1:|||,SACD=F4:|||_SACD=F2:|||_SACD=F1:|||,GTSM=F5:|||_GTSM=F3:|||_GTSM=F2:|||_GTSM=F1:|||";
            var tokenMerged_FAST_UPSVF = "CRF:5.050^1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999_CLR:3.600^1.200%0.400^0.400";
            var tokenMerged_FAST_UPSVFPASSFLOW = "CRF:4.400^1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999_CLR:3.600^1.200%0.400^0.400";
            MockDatalog(new List<string> { token_FAST_CORNERS, token_FAST_STC_V, token_FAST_UPSVF, token_FAST_UPSVFPASSFLOW, tokenMerged_FAST_CORNERS, tokenMerged_FAST_STC_V, tokenMerged_FAST_UPSVF, tokenMerged_FAST_UPSVFPASSFLOW });

            // Create Prime Vmin Data.
            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            AddDataToFactoryMock(ref vminFactoryMock, new Dictionary<string, double>
                {
                    { "CRF0@F6", 1.0 },
                    { "CRF1@F6", 1.1 },
                    { "CRF0@F1", 0.7 },
                    { "CRF1@F1", 0.6 },
                    { "CLR@F5", 1.2 },
                    { "CLR@F1", 0.4 },
                });
            this.AddFullConfigToFactoryMock(ref vminFactoryMock);
            DDG.VminForwarding.Service = vminFactoryMock.Object;

            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(o => o.GetStringRowFromTable("FAST_STC_V", Context.DUT)).Throws(new FatalException("Prime Error"));
            sharedServiceMock.Setup(o => o.GetIntegerRowFromTable("FakeFlow", Context.DUT)).Returns(4);
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_UPSVF", tokenMerged_FAST_UPSVF, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_UPSVFPASSFLOW", tokenMerged_FAST_UPSVFPASSFLOW, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_CORNERS", tokenMerged_FAST_CORNERS, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_STC_V", tokenMerged_FAST_STC_V, Context.DUT));

            // Add in the PrimeVminForwarding tables.
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            var underTest = new VminForwardingSaveAsUpsGsdsTC { PassingFlowInputGsds = "G.U.I.FakeFlow", MergeWithEvgData = VminForwardingSaveAsUpsGsdsTC.MyBool.True };
            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_UPSVF", tokenMerged_FAST_UPSVF, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_UPSVFPASSFLOW", tokenMerged_FAST_UPSVFPASSFLOW, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_CORNERS", tokenMerged_FAST_CORNERS, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_STC_V", tokenMerged_FAST_STC_V, Context.DUT), Times.Once);
        }

        /// <summary>
        /// Validate the UPS_Data class and its methods.
        /// </summary>
        [TestMethod]
        public void UPS_Data_Build_Pass()
        {
            // var token_FAST_CORNERS = "CR=206:|||0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999_CR=205:|||0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999_CR=204:|||0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999_CR=203:|||0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999_CR=202:|||0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999_CR=201:|||0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999,CRF=226:|||_CRF=225:|||_CRF=224:|||_CRF=223:|||_CRF=222:|||_CRF=221:|||,CLR=216:|||0.913_CLR=215:|||0.835_CLR=214:|||0.734_CLR=213:|||0.590_CLR=212:|||0.526_CLR=211:|||0.500,CRX2=256:|||_CRX2=255:|||_CRX2=254:|||_CRX2=253:|||_CRX2=252:|||_CRX2=251:|||,CRX3=266:|||_CRX3=265:|||_CRX3=264:|||_CRX3=263:|||_CRX3=262:|||_CRX3=261:|||,GTS=305:|||0.877_GTS=304:|||0.773_GTS=303:|||0.680_GTS=302:|||0.600_GTS=301:|||0.540,SAQ=406:|||0.740_SAQ=404:|||0.640_SAQ=401:|||0.560,SAPS=515:|||0.830_SAPS=513:|||0.600_SAPS=511:|||0.530,SAIS=525:|||0.690_SAIS=521:|||0.530,SAF=535:|||0.640_SAF=531:|||0.560,SACD=504:|||0.746_SACD=502:|||0.670_SACD=501:|||0.570,GTSM=325:|||0.880_GTSM=323:|||0.760_GTSM=322:|||0.640_GTSM=321:|||0.530";
            var token_FAST_CORNERS = "CR=999:|||0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999_CR=999:|||0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999_CR=999:|||0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999_CR=999:|||0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999_CR=999:|||0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999_CR=999:|||0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999,CRF=999:|||_CRF=999:|||_CRF=999:|||_CRF=999:|||_CRF=999:|||_CRF=999:|||,CLR=999:|||0.913_CLR=999:|||0.835_CLR=999:|||0.734_CLR=999:|||0.590_CLR=999:|||0.526_CLR=999:|||0.500,CRX2=999:|||_CRX2=999:|||_CRX2=999:|||_CRX2=999:|||_CRX2=999:|||_CRX2=999:|||,CRX3=999:|||_CRX3=999:|||_CRX3=999:|||_CRX3=999:|||_CRX3=999:|||_CRX3=999:|||,GTS=999:|||0.877_GTS=999:|||0.773_GTS=999:|||0.680_GTS=999:|||0.600_GTS=999:|||0.540,SAQ=999:|||0.740_SAQ=999:|||0.640_SAQ=999:|||0.560,SAPS=999:|||0.830_SAPS=999:|||0.600_SAPS=999:|||0.530,SAIS=999:|||0.690_SAIS=999:|||0.530,SAF=999:|||0.640_SAF=999:|||0.560,SACD=999:|||0.746_SACD=999:|||0.670_SACD=999:|||0.570,GTSM=999:|||0.880_GTSM=999:|||0.760_GTSM=999:|||0.640_GTSM=999:|||0.530";
            var token_FAST_STC_V = "CR=F6:|||0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999_CR=F5:|||0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999_CR=F4:|||0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999_CR=F3:|||0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999_CR=F2:|||0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999_CR=F1:|||0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999,CRF=F6:|||_CRF=F5:|||_CRF=F4:|||_CRF=F3:|||_CRF=F2:|||_CRF=F1:|||,CLR=F6:|||0.913_CLR=F5:|||0.835_CLR=F4:|||0.734_CLR=F3:|||0.590_CLR=F2:|||0.526_CLR=F1:|||0.500,CRX2=F6:|||_CRX2=F5:|||_CRX2=F4:|||_CRX2=F3:|||_CRX2=F2:|||_CRX2=F1:|||,CRX3=F6:|||_CRX3=F5:|||_CRX3=F4:|||_CRX3=F3:|||_CRX3=F2:|||_CRX3=F1:|||,GTS=F5:|||0.877_GTS=F4:|||0.773_GTS=F3:|||0.680_GTS=F2:|||0.600_GTS=F1:|||0.540,SAQ=F6:|||0.740_SAQ=F4:|||0.640_SAQ=F1:|||0.560,SAPS=F5:|||0.830_SAPS=F3:|||0.600_SAPS=F1:|||0.530,SAIS=F5:|||0.690_SAIS=F1:|||0.530,SAF=F5:|||0.640_SAF=F1:|||0.560,SACD=F4:|||0.746_SACD=F2:|||0.670_SACD=F1:|||0.570,GTSM=F5:|||0.880_GTSM=F3:|||0.760_GTSM=F2:|||0.640_GTSM=F1:|||0.530";
            var token_FAST_UPSVF = "CR:4.400^0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999%4.200^0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999%3.400^0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999%2.200^0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999%1.200^0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999_CLR:4.000^0.913%3.600^0.835%3.000^0.734%1.800^0.590%0.800^0.526%0.400^0.500_GTS:1.300^0.877%1.100^0.773%0.900^0.680%0.600^0.600%0.300^0.540_SAQ:2.700^0.740%2.200^0.640%1.100^0.560_SAPS:1.000^0.830%0.400^0.600%0.200^0.530_SAIS:0.533^0.690%0.200^0.530_SAF:0.800^0.640%0.533^0.560_SACD:0.662^0.746%0.562^0.670%0.312^0.570_GTSM:1.100^0.880%0.900^0.760%0.600^0.640%0.300^0.530";
            var token_FAST_UPSVFPASSFLOW = "CR:4.400^0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999%4.200^0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999%3.400^0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999%2.200^0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999%1.200^0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999_CLR:4.000^0.913%3.600^0.835%3.000^0.734%1.800^0.590%0.800^0.526%0.400^0.500_GTS:1.300^0.877%1.100^0.773%0.900^0.680%0.600^0.600%0.300^0.540_SAQ:2.700^0.740%2.200^0.640%1.100^0.560_SAPS:1.000^0.830%0.400^0.600%0.200^0.530_SAIS:0.533^0.690%0.200^0.530_SAF:0.800^0.640%0.533^0.560_SACD:0.662^0.746%0.562^0.670%0.312^0.570_GTSM:1.100^0.880%0.900^0.760%0.600^0.640%0.300^0.530";

            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(o => o.GetStringRowFromTable("FAST_STC_V", Context.DUT)).Returns(token_FAST_STC_V);
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            // Mock in Prime Data.
            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            AddDataToFactoryMock(ref vminFactoryMock, new Dictionary<string, double>
                {
                    { "CRF0@F6", 1.0 },
                    { "CRF1@F6", 1.1 },
                    { "CRF0@F1", 0.7 },
                    { "CRF1@F1", 0.6 },
                    { "CLR@F5", 1.2 },
                    { "CLR@F1", 0.4 },
                });
            this.AddFullConfigToFactoryMock(ref vminFactoryMock);
            DDG.VminForwarding.Service = vminFactoryMock.Object;

            var evgData = VminForwardingSaveAsUpsGsdsTC.UPS_Data.BuildFromEvgTokens(4);
            var tokens = evgData.ToGsdsTokens(4);
            Assert.AreEqual(token_FAST_CORNERS, tokens["FAST_CORNERS"], "Failed compare for 'FAST_CORNERS'.");
            Assert.AreEqual(token_FAST_STC_V, tokens["FAST_STC_V"], "Failed compare for 'FAST_STC_V'.");
            Assert.AreEqual(token_FAST_UPSVF, tokens["FAST_UPSVF"], "Failed compare for 'FAST_UPSVF'.");
            Assert.AreEqual(token_FAST_UPSVFPASSFLOW, tokens["FAST_UPSVFPASSFLOW"], "Failed compare for 'FAST_UPSVFPASSFLOW'.");

            // var tokenMerged_FAST_CORNERS = "CR=206:|||0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999_CR=205:|||0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999_CR=204:|||0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999_CR=203:|||0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999_CR=202:|||0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999_CR=201:|||0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999,CRF=226:|||1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999_CRF=225:|||_CRF=224:|||_CRF=223:|||_CRF=222:|||_CRF=221:|||0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999,CLR=216:|||0.913_CLR=215:|||1.200_CLR=214:|||0.734_CLR=213:|||0.590_CLR=212:|||0.526_CLR=211:|||0.500,CRX2=256:|||_CRX2=255:|||_CRX2=254:|||_CRX2=253:|||_CRX2=252:|||_CRX2=251:|||,CRX3=266:|||_CRX3=265:|||_CRX3=264:|||_CRX3=263:|||_CRX3=262:|||_CRX3=261:|||,GTS=305:|||0.877_GTS=304:|||0.773_GTS=303:|||0.680_GTS=302:|||0.600_GTS=301:|||0.540,SAQ=406:|||0.740_SAQ=404:|||0.640_SAQ=401:|||0.560,SAPS=515:|||0.830_SAPS=513:|||0.600_SAPS=511:|||0.530,SAIS=525:|||0.690_SAIS=521:|||0.530,SAF=535:|||0.640_SAF=531:|||0.560,SACD=504:|||0.746_SACD=502:|||0.670_SACD=501:|||0.570,GTSM=325:|||0.880_GTSM=323:|||0.760_GTSM=322:|||0.640_GTSM=321:|||0.530";
            var tokenMerged_FAST_CORNERS = "CR=999:|||0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999_CR=999:|||0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999_CR=999:|||0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999_CR=999:|||0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999_CR=999:|||0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999_CR=999:|||0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999,CRF=999:|||1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999_CRF=999:|||_CRF=999:|||_CRF=999:|||_CRF=999:|||_CRF=999:|||0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999,CLR=999:|||0.913_CLR=999:|||1.200_CLR=999:|||0.734_CLR=999:|||0.590_CLR=999:|||0.526_CLR=999:|||0.500,CRX2=999:|||_CRX2=999:|||_CRX2=999:|||_CRX2=999:|||_CRX2=999:|||_CRX2=999:|||,CRX3=999:|||_CRX3=999:|||_CRX3=999:|||_CRX3=999:|||_CRX3=999:|||_CRX3=999:|||,GTS=999:|||0.877_GTS=999:|||0.773_GTS=999:|||0.680_GTS=999:|||0.600_GTS=999:|||0.540,SAQ=999:|||0.740_SAQ=999:|||0.640_SAQ=999:|||0.560,SAPS=999:|||0.830_SAPS=999:|||0.600_SAPS=999:|||0.530,SAIS=999:|||0.690_SAIS=999:|||0.530,SAF=999:|||0.640_SAF=999:|||0.560,SACD=999:|||0.746_SACD=999:|||0.670_SACD=999:|||0.570,GTSM=999:|||0.880_GTSM=999:|||0.760_GTSM=999:|||0.640_GTSM=999:|||0.530";
            var tokenMerged_FAST_STC_V = "CR=F6:|||0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999_CR=F5:|||0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999_CR=F4:|||0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999_CR=F3:|||0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999_CR=F2:|||0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999_CR=F1:|||0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999,CRF=F6:|||1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999_CRF=F5:|||_CRF=F4:|||_CRF=F3:|||_CRF=F2:|||_CRF=F1:|||0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999,CLR=F6:|||0.913_CLR=F5:|||1.200_CLR=F4:|||0.734_CLR=F3:|||0.590_CLR=F2:|||0.526_CLR=F1:|||0.500,CRX2=F6:|||_CRX2=F5:|||_CRX2=F4:|||_CRX2=F3:|||_CRX2=F2:|||_CRX2=F1:|||,CRX3=F6:|||_CRX3=F5:|||_CRX3=F4:|||_CRX3=F3:|||_CRX3=F2:|||_CRX3=F1:|||,GTS=F5:|||0.877_GTS=F4:|||0.773_GTS=F3:|||0.680_GTS=F2:|||0.600_GTS=F1:|||0.540,SAQ=F6:|||0.740_SAQ=F4:|||0.640_SAQ=F1:|||0.560,SAPS=F5:|||0.830_SAPS=F3:|||0.600_SAPS=F1:|||0.530,SAIS=F5:|||0.690_SAIS=F1:|||0.530,SAF=F5:|||0.640_SAF=F1:|||0.560,SACD=F4:|||0.746_SACD=F2:|||0.670_SACD=F1:|||0.570,GTSM=F5:|||0.880_GTSM=F3:|||0.760_GTSM=F2:|||0.640_GTSM=F1:|||0.530";
            var tokenMerged_FAST_UPSVF = "CR:4.400^0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999%4.200^0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999%3.400^0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999%2.200^0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999%1.200^0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999_CRF:5.050^1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999_CLR:4.000^0.913%3.600^1.200%3.000^0.734%1.800^0.590%0.800^0.526%0.400^0.400_GTS:1.300^0.877%1.100^0.773%0.900^0.680%0.600^0.600%0.300^0.540_SAQ:2.700^0.740%2.200^0.640%1.100^0.560_SAPS:1.000^0.830%0.400^0.600%0.200^0.530_SAIS:0.533^0.690%0.200^0.530_SAF:0.800^0.640%0.533^0.560_SACD:0.662^0.746%0.562^0.670%0.312^0.570_GTSM:1.100^0.880%0.900^0.760%0.600^0.640%0.300^0.530";
            var tokenMerged_FAST_UPSVFPASSFLOW = "CR:4.400^0.970v0.970v-9999v-9999v-9999v-9999v-9999v-9999%4.200^0.901v0.902v-9999v-9999v-9999v-9999v-9999v-9999%3.400^0.746v0.749v-9999v-9999v-9999v-9999v-9999v-9999%2.200^0.605v0.595v-9999v-9999v-9999v-9999v-9999v-9999%1.200^0.528v0.533v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.490v0.500v-9999v-9999v-9999v-9999v-9999v-9999_CRF:4.400^1.000v1.100v-9999v-9999v-9999v-9999v-9999v-9999%0.400^0.700v0.600v-9999v-9999v-9999v-9999v-9999v-9999_CLR:4.000^0.913%3.600^1.200%3.000^0.734%1.800^0.590%0.800^0.526%0.400^0.500_GTS:1.300^0.877%1.100^0.773%0.900^0.680%0.600^0.600%0.300^0.540_SAQ:2.700^0.740%2.200^0.640%1.100^0.560_SAPS:1.000^0.830%0.400^0.600%0.200^0.530_SAIS:0.533^0.690%0.200^0.530_SAF:0.800^0.640%0.533^0.560_SACD:0.662^0.746%0.562^0.670%0.312^0.570_GTSM:1.100^0.880%0.900^0.760%0.600^0.640%0.300^0.530";
            MockDatalog(new List<string> { token_FAST_CORNERS, token_FAST_STC_V, token_FAST_UPSVF, token_FAST_UPSVFPASSFLOW, tokenMerged_FAST_CORNERS, tokenMerged_FAST_STC_V, tokenMerged_FAST_UPSVF, tokenMerged_FAST_UPSVFPASSFLOW });

            sharedServiceMock.Setup(o => o.GetIntegerRowFromTable("FakeFlow", Context.DUT)).Returns(4);
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_UPSVF", tokenMerged_FAST_UPSVF, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_UPSVFPASSFLOW", tokenMerged_FAST_UPSVFPASSFLOW, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_CORNERS", tokenMerged_FAST_CORNERS, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_STC_V", tokenMerged_FAST_STC_V, Context.DUT));
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            // run the test.
            var underTest = new VminForwardingSaveAsUpsGsdsTC { PassingFlowInputGsds = "G.U.I.FakeFlow", MergeWithEvgData = VminForwardingSaveAsUpsGsdsTC.MyBool.True };
            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_UPSVF", tokenMerged_FAST_UPSVF, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_UPSVFPASSFLOW", tokenMerged_FAST_UPSVFPASSFLOW, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_CORNERS", tokenMerged_FAST_CORNERS, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_STC_V", tokenMerged_FAST_STC_V, Context.DUT), Times.Once);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_CheckParams_AllDefaults()
        {
            var underTest = new VminForwardingSaveAsUpsGsdsTC();
            underTest.Verify();
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Verify_CheckParams()
        {
            this.RunVerify("VAL1", "VAL2", "VAL3", "VAL4", "VAL5", VminForwardingSaveAsUpsGsdsTC.MyBool.False);
            Assert.ThrowsException<Exception>(() => this.RunVerify(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, VminForwardingSaveAsUpsGsdsTC.MyBool.False));
            Assert.ThrowsException<Exception>(() => this.RunVerify(string.Empty, "VAL2", "VAL3", "VAL4", "VAL5", VminForwardingSaveAsUpsGsdsTC.MyBool.False));
            Assert.ThrowsException<Exception>(() => this.RunVerify("VAL1", string.Empty, "VAL3", "VAL4", "VAL5", VminForwardingSaveAsUpsGsdsTC.MyBool.False));
            Assert.ThrowsException<Exception>(() => this.RunVerify("VAL1", "VAL2", string.Empty, "VAL4", "VAL5", VminForwardingSaveAsUpsGsdsTC.MyBool.False));
            Assert.ThrowsException<Exception>(() => this.RunVerify("VAL1", "VAL2", "VAL3", string.Empty, "VAL5", VminForwardingSaveAsUpsGsdsTC.MyBool.False));
            Assert.ThrowsException<Exception>(() => this.RunVerify("VAL1", "VAL2", "VAL3", "VAL4", string.Empty, VminForwardingSaveAsUpsGsdsTC.MyBool.False));
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_FlowInvalid_Fail()
        {
            var underTest = new VminForwardingSaveAsUpsGsdsTC { PassingFlowInputGsds = "G.U.S.FakeFlow" };
            underTest.Verify();

            var gsdsMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            gsdsMock.Setup(o => o.GetStringRowFromTable("FakeFlow", Context.DUT)).Returns("not_a_flow");
            Prime.Services.SharedStorageService = gsdsMock.Object;

            Assert.ThrowsException<TestMethodException>(() => underTest.Execute());
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_NoCore1VminDataFlow1Passing_Pass()
        {
            /*var expectedUpsVfGsds = "CR:5.100^1.000v1.100%0.800^0.700v0.600_CLR:0.600^1.200%0.700^0.800";
            var expectedUpsVfPassinFlowGsds = "CR:5.100^1.000v1.100%0.800^0.700v0.600_CLR:0.600^1.200%0.700^0.800";
            var expectedFastCornersGsds = "CR=206:1.000v1.100|1.000v1.100|1.000v1.100|1.000v1.100_CR=201:0.700v0.600|0.700v0.600|0.700v0.600|0.700v0.600,CLR=215:1.200|1.200|1.200|1.200_CLR=211:0.800|0.800|0.800|0.800"; */
            var expectedUpsVfGsds = "CR:5.000^1.000v-9999%0.400^0.700v-9999_CLR:3.600^1.200%0.400^0.800";
            var expectedUpsVfPassinFlowGsds = "CR:5.000^1.000v-9999%0.400^0.700v-9999_CLR:3.600^1.200%0.400^0.800";
            /* var expectedFastCornersGsds = "CR=206:1.000v-9999|1.000v-9999|1.000v-9999|1.000v-9999_CR=201:0.700v-9999|0.700v-9999|0.700v-9999|0.700v-9999,CLR=215:1.200|1.200|1.200|1.200_CLR=211:0.800|0.800|0.800|0.800"; */
            var expectedFastCornersGsds = "CR=999:1.000v-9999|1.000v-9999|1.000v-9999|1.000v-9999_CR=999:0.700v-9999|0.700v-9999|0.700v-9999|0.700v-9999,CLR=999:1.200|1.200|1.200|1.200_CLR=999:0.800|0.800|0.800|0.800";
            var expectedFastStcGsds = "CR=F6:1.000v-9999|1.000v-9999|1.000v-9999|1.000v-9999_CR=F1:0.700v-9999|0.700v-9999|0.700v-9999|0.700v-9999,CLR=F5:1.200|1.200|1.200|1.200_CLR=F1:0.800|0.800|0.800|0.800";
            MockDatalog(new List<string> { expectedUpsVfGsds, expectedUpsVfPassinFlowGsds, expectedFastCornersGsds, expectedFastStcGsds });

            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(o => o.GetIntegerRowFromTable("FakeFlow", Context.DUT)).Returns(1);
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_UPSVF", expectedUpsVfGsds, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_UPSVFPASSFLOW", expectedUpsVfPassinFlowGsds, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_CORNERS", expectedFastCornersGsds, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_STC_V", expectedFastStcGsds, Context.DUT));
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            AddDataToFactoryMock(ref vminFactoryMock, new Dictionary<string, double>
                {
                    { "CR0@F6", 1.0 },
                    { "CR1@F6", -9999f },
                    { "CR0@F1", 0.7 },
                    { "CR1@F1", -9999f },
                    { "CLR@F5", 1.2 },
                    { "CLR@F1", 0.8 },
                });
            this.AddSmallConfigToFactoryMock(ref vminFactoryMock);
            DDG.VminForwarding.Service = vminFactoryMock.Object;

            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var underTest = new VminForwardingSaveAsUpsGsdsTC { PassingFlowInputGsds = "G.U.I.FakeFlow" };
            underTest.Verify();

            // Assert.ThrowsException<TestMethodException>(() => underTest.Execute());
            Assert.AreEqual(1, underTest.Execute());

            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_UPSVF", expectedUpsVfGsds, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_UPSVFPASSFLOW", expectedUpsVfPassinFlowGsds, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_CORNERS", expectedFastCornersGsds, Context.DUT), Times.Once);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_NoF1DataNoCore0DataFlow3Passing_Pass()
        {
            var expectedUpsVfGsds = "CR:5.000^1.000v-9999_CLR:3.600^1.200";
            var expectedUpsVfPassinFlowGsds = "CR:4.700^1.000v-9999_CLR:3.600^1.200";
            /* var expectedFastCornersGsds = "CR=206:||1.000v-9999|1.000v-9999_CR=201:|||,CLR=215:||1.200|1.200_CLR=211:|||"; */
            var expectedFastCornersGsds = "CR=999:||1.000v-9999|1.000v-9999_CR=999:|||,CLR=999:||1.200|1.200_CLR=999:|||";
            var expectedFastStcGsds = "CR=F6:||1.000v-9999|1.000v-9999_CR=F1:|||,CLR=F5:||1.200|1.200_CLR=F1:|||";
            MockDatalog(new List<string> { expectedUpsVfGsds, expectedUpsVfPassinFlowGsds, expectedFastCornersGsds, expectedFastStcGsds });

            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(o => o.GetIntegerRowFromTable("FakeFlow", Context.DUT)).Returns(3);
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_UPSVF", expectedUpsVfGsds, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_UPSVFPASSFLOW", expectedUpsVfPassinFlowGsds, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_CORNERS", expectedFastCornersGsds, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_STC_V", expectedFastStcGsds, Context.DUT));
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            AddDataToFactoryMock(ref vminFactoryMock, new Dictionary<string, double>
                {
                    { "CR0@F6", 1.0 },
                    { "CR1@F6", InvalidDataThrowException },
                    { "CR0@F1", -9999f },
                    { "CR1@F1", -9999f },
                    { "CLR@F5", 1.2 },
                    { "CLR@F1", -9999f },
                });
            this.AddSmallConfigToFactoryMock(ref vminFactoryMock);
            DDG.VminForwarding.Service = vminFactoryMock.Object;

            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var underTest = new VminForwardingSaveAsUpsGsdsTC { PassingFlowInputGsds = "G.U.I.FakeFlow" };
            underTest.Verify();

            // Assert.ThrowsException<TestMethodException>(() => underTest.Execute());
            Assert.AreEqual(1, underTest.Execute());

            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_UPSVF", expectedUpsVfGsds, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_UPSVFPASSFLOW", expectedUpsVfPassinFlowGsds, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_CORNERS", expectedFastCornersGsds, Context.DUT), Times.Once);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_Flow1Passing_Pass()
        {
            var expectedUpsVfGsds = "CR:5.000^1.000v1.100%0.400^0.700v0.600_CLR:3.600^1.200%0.400^0.800";
            var expectedUpsVfPassinFlowGsds = "CR:5.000^1.000v1.100%0.400^0.700v0.600_CLR:3.600^1.200%0.400^0.800";
            /* var expectedFastCornersGsds = "CR=206:1.000v1.100|1.000v1.100|1.000v1.100|1.000v1.100_CR=201:0.700v0.600|0.700v0.600|0.700v0.600|0.700v0.600,CLR=215:1.200|1.200|1.200|1.200_CLR=211:0.800|0.800|0.800|0.800"; */
            var expectedFastCornersGsds = "CR=999:1.000v1.100|1.000v1.100|1.000v1.100|1.000v1.100_CR=999:0.700v0.600|0.700v0.600|0.700v0.600|0.700v0.600,CLR=999:1.200|1.200|1.200|1.200_CLR=999:0.800|0.800|0.800|0.800";
            var expectedFastStcGsds = "CR=F6:1.000v1.100|1.000v1.100|1.000v1.100|1.000v1.100_CR=F1:0.700v0.600|0.700v0.600|0.700v0.600|0.700v0.600,CLR=F5:1.200|1.200|1.200|1.200_CLR=F1:0.800|0.800|0.800|0.800";
            MockDatalog(new List<string> { expectedUpsVfGsds, expectedUpsVfPassinFlowGsds, expectedFastCornersGsds, expectedFastStcGsds });

            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(o => o.GetIntegerRowFromTable("FakeFlow", Context.DUT)).Returns(1);
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_UPSVF", expectedUpsVfGsds, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_UPSVFPASSFLOW", expectedUpsVfPassinFlowGsds, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_CORNERS", expectedFastCornersGsds, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_STC_V", expectedFastStcGsds, Context.DUT));
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            AddDataToFactoryMock(ref vminFactoryMock, new Dictionary<string, double>
                {
                    { "CR0@F6", 1.0 },
                    { "CR1@F6", 1.1 },
                    { "CR0@F1", 0.7 },
                    { "CR1@F1", 0.6 },
                    { "CLR@F5", 1.2 },
                    { "CLR@F1", 0.8 },
                });
            this.AddSmallConfigToFactoryMock(ref vminFactoryMock);
            DDG.VminForwarding.Service = vminFactoryMock.Object;

            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var underTest = new VminForwardingSaveAsUpsGsdsTC { PassingFlowInputGsds = "G.U.I.FakeFlow" };
            underTest.Verify();
            Assert.AreEqual(1, underTest.Execute());

            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_UPSVF", expectedUpsVfGsds, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_UPSVFPASSFLOW", expectedUpsVfPassinFlowGsds, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_CORNERS", expectedFastCornersGsds, Context.DUT), Times.Once);
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void Execute_Flow4PassingF7InvalidFreq_Pass()
        {
            var expectedUpsVfGsds = "CR:5.500^1.000v1.100%0.400^0.700v0.600_CLR:3.600^1.200%0.400^0.800";
            var expectedUpsVfPassinFlowGsds = "CR:0.400^0.700v0.600_CLR:3.600^1.200%0.400^0.800";
            var expectedFastCornersGsds = "CR=999:|||_CR=999:|||0.700v0.600,CLR=999:|||1.200_CLR=999:|||0.800";
            var expectedFastStcGsds = "CR=F7:|||_CR=F1:|||0.700v0.600,CLR=F5:|||1.200_CLR=F1:|||0.800";
            MockDatalog(new List<string> { expectedUpsVfGsds, expectedUpsVfPassinFlowGsds, expectedFastCornersGsds, expectedFastStcGsds });

            var sharedServiceMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            sharedServiceMock.Setup(o => o.GetIntegerRowFromTable("FakeFlow", Context.DUT)).Returns(4);
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_UPSVF", expectedUpsVfGsds, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_UPSVFPASSFLOW", expectedUpsVfPassinFlowGsds, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_CORNERS", expectedFastCornersGsds, Context.DUT));
            sharedServiceMock.Setup(o => o.InsertRowAtTable("FAST_STC_V", expectedFastStcGsds, Context.DUT));
            Prime.Services.SharedStorageService = sharedServiceMock.Object;

            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            AddDataToFactoryMock(ref vminFactoryMock, new Dictionary<string, double>
                {
                    { "CR0@F7", 1.0 },
                    { "CR1@F7", 1.1 },
                    { "CR0@F1", 0.7 },
                    { "CR1@F1", 0.6 },
                    { "CLR@F5", 1.2 },
                    { "CLR@F1", 0.8 },
                });
            this.AddSmallF7ConfigToFactoryMock(ref vminFactoryMock);
            DDG.VminForwarding.Service = vminFactoryMock.Object;

            // [1] Setup the unit test scenario.
            // This includes the mocking of any prime service with its expected behavior for the given unit test scenario.
            var underTest = new VminForwardingSaveAsUpsGsdsTC { PassingFlowInputGsds = "G.U.I.FakeFlow" };
            underTest.Verify();

            // Assert.ThrowsException<TestMethodException>(() => underTest.Execute());
            Assert.AreEqual(1, underTest.Execute());

            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_UPSVF", expectedUpsVfGsds, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_UPSVFPASSFLOW", expectedUpsVfPassinFlowGsds, Context.DUT), Times.Once);
            sharedServiceMock.Verify(o => o.InsertRowAtTable("FAST_CORNERS", expectedFastCornersGsds, Context.DUT), Times.Once);
        }

        private static void AddDataToFactoryMock(ref Mock<DDG.IVminForwardingFactory> vminFactoryMock, Dictionary<string, double> primeVminData)
        {
            // create a default behavior equivalent to no data saved.
            var vminForwardingDefaultMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminForwardingDefaultMock.Setup(o => o.GetStartingVoltage(-9999)).Returns(-9999);
            vminFactoryMock.Setup(o => o.Get(It.IsAny<string>(), It.IsAny<int>())).Returns(vminForwardingDefaultMock.Object);

            // mock some corners with real data.
            foreach (var cornerName in primeVminData.Keys)
            {
                var vminForwardingMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
                if (Math.Abs(primeVminData[cornerName] - InvalidDataThrowException) < double.Epsilon * 2)
                {
                    vminForwardingMock.Setup(o => o.GetStartingVoltage(-9999)).Throws(new System.FormatException("Input string was not in a correct format."));
                }
                else
                {
                    vminForwardingMock.Setup(o => o.GetStartingVoltage(-9999)).Returns(primeVminData[cornerName]);
                }

                for (var i = 1; i <= 4; i++)
                {
                    vminFactoryMock.Setup(o => o.Get(cornerName, i)).Returns(vminForwardingMock.Object);
                }
            }
        }

        private static void MockDatalog(List<string> valuesToMock)
        {
            var writerMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            foreach (var value in valuesToMock)
            {
                writerMock.Setup(o => o.SetData(value));
            }

            writerMock.Setup(o => o.SetTnamePostfix("::EVG::FAST_UPSVF"));
            writerMock.Setup(o => o.SetTnamePostfix("::EVG::FAST_UPSVFPASSFLOW"));
            writerMock.Setup(o => o.SetTnamePostfix("::EVG::FAST_CORNERS"));
            writerMock.Setup(o => o.SetTnamePostfix("::EVG::FAST_STC_V"));
            writerMock.Setup(o => o.SetTnamePostfix("::PRIME::FAST_UPSVF"));
            writerMock.Setup(o => o.SetTnamePostfix("::PRIME::FAST_UPSVFPASSFLOW"));
            writerMock.Setup(o => o.SetTnamePostfix("::PRIME::FAST_CORNERS"));
            writerMock.Setup(o => o.SetTnamePostfix("::PRIME::FAST_STC_V"));

            var ituffMock = new Mock<IDatalogService>(MockBehavior.Strict);
            ituffMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(writerMock.Object);
            ituffMock.Setup(o => o.WriteToItuff(writerMock.Object));
            Prime.Services.DatalogService = ituffMock.Object;
        }

        private void AddFullConfigToFactoryMock(ref Mock<DDG.IVminForwardingFactory> mock)
        {
            mock.Setup(o => o.GetAllDomainNames()).Returns(new List<string> { "CR", "CRF", "CLR", "CRX2", "CRX3", "GTS", "SAQ", "SAPS", "SAIS", "SAF", "SACD", "GTSM" });
            mock.Setup(o => o.GetInstanceNamesForDomain("CR")).Returns(new List<string> { "CR0", "CR1", "CR2", "CR3", "CR4", "CR5", "CR6", "CR7" });
            mock.Setup(o => o.GetInstanceNamesForDomain("CRF")).Returns(new List<string> { "CRF0", "CRF1", "CRF2", "CRF3", "CRF4", "CRF5", "CRF6", "CRF7" });
            mock.Setup(o => o.GetInstanceNamesForDomain("CLR")).Returns(new List<string> { "CLR" });
            mock.Setup(o => o.GetInstanceNamesForDomain("CRX2")).Returns(new List<string> { "CRX20", "CRX21", "CRX22", "CRX23", "CRX24", "CRX25", "CRX26", "CRX27" });
            mock.Setup(o => o.GetInstanceNamesForDomain("CRX3")).Returns(new List<string> { "CRX30", "CRX31", "CRX32", "CRX33", "CRX34", "CRX35", "CRX36", "CRX37" });
            mock.Setup(o => o.GetInstanceNamesForDomain("GTS")).Returns(new List<string> { "GTS" });
            mock.Setup(o => o.GetInstanceNamesForDomain("SAQ")).Returns(new List<string> { "SAQ" });
            mock.Setup(o => o.GetInstanceNamesForDomain("SAPS")).Returns(new List<string> { "SAPS" });
            mock.Setup(o => o.GetInstanceNamesForDomain("SAIS")).Returns(new List<string> { "SAIS" });
            mock.Setup(o => o.GetInstanceNamesForDomain("SAF")).Returns(new List<string> { "SAF" });
            mock.Setup(o => o.GetInstanceNamesForDomain("SACD")).Returns(new List<string> { "SACD" });
            mock.Setup(o => o.GetInstanceNamesForDomain("GTSM")).Returns(new List<string> { "GTSM" });

            mock.Setup(o => o.GetCornerNamesForDomainInstance("CR0")).Returns(new List<string> { "CR0@F6", "CR0@F5", "CR0@F4", "CR0@F3", "CR0@F2", "CR0@F1" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("CRF0")).Returns(new List<string> { "CRF0@F6", "CRF0@F5", "CRF0@F4", "CRF0@F3", "CRF0@F2", "CRF0@F1" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("CLR")).Returns(new List<string> { "CLR@F6", "CLR@F5", "CLR@F4", "CLR@F3", "CLR@F2", "CLR@F1" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("CRX20")).Returns(new List<string> { "CRX20@F6", "CRX20@F5", "CRX20@F4", "CRX20@F3", "CRX20@F2", "CRX20@F1" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("CRX30")).Returns(new List<string> { "CRX30@F6", "CRX30@F5", "CRX30@F4", "CRX30@F3", "CRX30@F2", "CRX30@F1" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("GTS")).Returns(new List<string> { "GTS@F5", "GTS@F4", "GTS@F3", "GTS@F2", "GTS@F1" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("SAQ")).Returns(new List<string> { "SAQ@F6", "SAQ@F4", "SAQ@F1" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("SAPS")).Returns(new List<string> { "SAPS@F5", "SAPS@F3", "SAPS@F1" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("SAIS")).Returns(new List<string> { "SAIS@F5", "SAIS@F1" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("SAF")).Returns(new List<string> { "SAF@F5", "SAF@F1" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("SACD")).Returns(new List<string> { "SACD@F4", "SACD@F2", "SACD@F1" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("GTSM")).Returns(new List<string> { "GTSM@F5", "GTSM@F3", "GTSM@F2", "GTSM@F1" });

            var instanceToDomainMap = new Dictionary<string, string>
            {
                { "CLR", "CLR" },
                { "SAQ", "SAQ" },
                { "SAPS", "SAPS" },
                { "SAIS", "SAIS" },
                { "SAF", "SAF" },
                { "SACD", "SACD" },
                { "GTS", "GTS" },
                { "GTSM", "GTSM" },
            };

            for (var core = 0; core < 8; core++)
            {
                instanceToDomainMap[$"CR{core}"] = "CR";
                instanceToDomainMap[$"CRF{core}"] = "CRF";
                instanceToDomainMap[$"CRX2{core}"] = "CRX2";
                instanceToDomainMap[$"CRX3{core}"] = "CRX3";
            }

            mock.Setup(o => o.GetFrequency(It.IsAny<string>(), It.IsAny<int>())).Returns((string name, int flow)
                => double.Parse(this.FrequencyMap[$"{instanceToDomainMap[name.Split('@').First()]}_{name.Split('@').Last()}_FREQ"][flow - 1]));
        }

        private void AddSmallConfigToFactoryMock(ref Mock<DDG.IVminForwardingFactory> mock)
        {
            mock.Setup(o => o.GetAllDomainNames()).Returns(new List<string> { "CR", "CLR" });
            mock.Setup(o => o.GetInstanceNamesForDomain("CR")).Returns(new List<string> { "CR0", "CR1" });
            mock.Setup(o => o.GetInstanceNamesForDomain("CLR")).Returns(new List<string> { "CLR" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("CR0")).Returns(new List<string> { "CR0@F6", "CR0@F1" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("CLR")).Returns(new List<string> { "CLR@F5", "CLR@F1" });
            mock.Setup(o => o.GetFrequency("CR0@F6", It.IsAny<int>())).Returns((string name, int flow) => double.Parse(this.FrequencyMap["CR_F6_FREQ"][flow - 1]));
            mock.Setup(o => o.GetFrequency("CR0@F1", It.IsAny<int>())).Returns((string name, int flow) => double.Parse(this.FrequencyMap["CR_F1_FREQ"][flow - 1]));
            mock.Setup(o => o.GetFrequency("CR1@F6", It.IsAny<int>())).Returns((string name, int flow) => double.Parse(this.FrequencyMap["CR_F6_FREQ"][flow - 1]));
            mock.Setup(o => o.GetFrequency("CR1@F1", It.IsAny<int>())).Returns((string name, int flow) => double.Parse(this.FrequencyMap["CR_F1_FREQ"][flow - 1]));
            mock.Setup(o => o.GetFrequency("CLR@F5", It.IsAny<int>())).Returns((string name, int flow) => double.Parse(this.FrequencyMap["CLR_F5_FREQ"][flow - 1]));
            mock.Setup(o => o.GetFrequency("CLR@F1", It.IsAny<int>())).Returns((string name, int flow) => double.Parse(this.FrequencyMap["CLR_F1_FREQ"][flow - 1]));
        }

        private void AddSmallF7ConfigToFactoryMock(ref Mock<DDG.IVminForwardingFactory> mock)
        {
            mock.Setup(o => o.GetAllDomainNames()).Returns(new List<string> { "CR", "CLR" });
            mock.Setup(o => o.GetInstanceNamesForDomain("CR")).Returns(new List<string> { "CR0", "CR1" });
            mock.Setup(o => o.GetInstanceNamesForDomain("CLR")).Returns(new List<string> { "CLR" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("CR0")).Returns(new List<string> { "CR0@F7", "CR0@F1" });
            mock.Setup(o => o.GetCornerNamesForDomainInstance("CLR")).Returns(new List<string> { "CLR@F5", "CLR@F1" });
            mock.Setup(o => o.GetFrequency("CR0@F7", It.IsAny<int>())).Returns((string name, int flow) => double.Parse(this.FrequencyMap["CR_F7_FREQ"][flow - 1]));
            mock.Setup(o => o.GetFrequency("CR0@F1", It.IsAny<int>())).Returns((string name, int flow) => double.Parse(this.FrequencyMap["CR_F1_FREQ"][flow - 1]));
            mock.Setup(o => o.GetFrequency("CR1@F7", It.IsAny<int>())).Returns((string name, int flow) => double.Parse(this.FrequencyMap["CR_F7_FREQ"][flow - 1]));
            mock.Setup(o => o.GetFrequency("CR1@F1", It.IsAny<int>())).Returns((string name, int flow) => double.Parse(this.FrequencyMap["CR_F1_FREQ"][flow - 1]));
            mock.Setup(o => o.GetFrequency("CLR@F5", It.IsAny<int>())).Returns((string name, int flow) => double.Parse(this.FrequencyMap["CLR_F5_FREQ"][flow - 1]));
            mock.Setup(o => o.GetFrequency("CLR@F1", It.IsAny<int>())).Returns((string name, int flow) => double.Parse(this.FrequencyMap["CLR_F1_FREQ"][flow - 1]));
        }

        private void RunVerify(string upsvf, string upsvfpassing, string fastcorners, string faststc, string passingflow, VminForwardingSaveAsUpsGsdsTC.MyBool merge)
        {
            var underTest = new VminForwardingSaveAsUpsGsdsTC
            {
                UpsVfGsds = upsvf,
                UpsVfPassinFlowGsds = upsvfpassing,
                FastCornersGsds = fastcorners,
                FastStcGsds = faststc,
                PassingFlowInputGsds = passingflow,
                MergeWithEvgData = merge,
            };

            underTest.Verify();
        }
    }
}
