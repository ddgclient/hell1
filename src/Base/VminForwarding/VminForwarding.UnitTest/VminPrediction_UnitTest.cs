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

namespace VminForwardingBase.UnitTest
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.VminForwardingService;

    /// <summary>
    /// Defines the <see cref="VminPrediction_UnitTest" />.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class VminPrediction_UnitTest
    {
        /// <summary>
        /// initialize all common mocks.
        /// </summary>
        [TestInitialize]
        public void AddCommonMocks()
        {
            // don't care about print messages.
            Prime.Services.ConsoleService = new Mock<IConsoleService>(MockBehavior.Loose).Object;
        }

        /// <summary>
        /// Test the STCInterpolation with the prime vminforwarding table.
        /// </summary>
        [TestMethod]
        public void ApplySTCInterpolationToPrime_Pass()
        {
            // define the frequencies.
            var freqFake = 3000000000d;
            var freqF1 = 2000000000d;
            var freqF2 = 2500000000d;
            var freqF3 = 3000000000d;

            // Mock the Prime Vmin Configuration tables.
            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            vminFactoryMock.Setup(o => o.GetInstanceNamesForDomain("CR")).Returns(new List<string> { "CR0", "CR1" });
            vminFactoryMock.Setup(o => o.GetCornerNamesForDomainInstance("CR0")).Returns(new List<string> { "CR0@F6", "CR0@F5", "CR0@F4", "CR0@F3", "CR0@F2", "CR0@F1" });
            vminFactoryMock.Setup(o => o.GetCornerNamesForDomainInstance("CR1")).Returns(new List<string> { "CR1@F6", "CR1@F5", "CR1@F4", "CR1@F3", "CR1@F2", "CR1@F1" });

            // mock the snapshot data
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CR0@F1")).Returns(CreateVminData(1, freqF1, 1.0));
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CR0@F3")).Returns(CreateVminData(1, freqF3, 1.0));
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CR0@F6")).Returns(CreateVminData(1, freqFake, 1.0));
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CR1@F1")).Returns(CreateVminData(1, freqF1, 1.0));
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CR1@F3")).Returns(CreateVminData(1, freqF3, 1.0));
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CR1@F6")).Returns(CreateVminData(1, freqFake, 1.0));

            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot("CR0@F2")).Returns(CreateVminData(1, freqF2, 1.0));
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot("CR0@F4")).Returns(CreateVminData(1, freqFake, 1.0));
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot("CR0@F5")).Returns(CreateVminData(1, freqFake, 1.0));
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot("CR1@F2")).Returns(CreateVminData(1, freqF2, 1.0));
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot("CR1@F4")).Returns(CreateVminData(1, freqFake, 1.0));
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot("CR1@F5")).Returns(CreateVminData(1, freqFake, 1.0));

            // mock the expected results.
            var expectedVmins = new List<double> { 3.0 };
            var expectedVminsF2 = new List<double> { 2.5 };

            var vminForwardingUpdatedDataMock = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminForwardingUpdatedDataMock.Setup(o => o.StoreVminResult(expectedVmins)).Returns(true);

            var vminForwardingUpdatedDataMockF2 = new Mock<DDG.IVminForwardingCorner>(MockBehavior.Strict);
            vminForwardingUpdatedDataMockF2.Setup(o => o.StoreVminResult(expectedVminsF2)).Returns(true);

            vminFactoryMock.Setup(o => o.Get("CR0@F2", 1)).Returns(vminForwardingUpdatedDataMockF2.Object);
            vminFactoryMock.Setup(o => o.Get("CR0@F4", 1)).Returns(vminForwardingUpdatedDataMock.Object);
            vminFactoryMock.Setup(o => o.Get("CR0@F5", 1)).Returns(vminForwardingUpdatedDataMock.Object);
            vminFactoryMock.Setup(o => o.Get("CR1@F2", 1)).Returns(vminForwardingUpdatedDataMockF2.Object);
            vminFactoryMock.Setup(o => o.Get("CR1@F4", 1)).Returns(vminForwardingUpdatedDataMock.Object);
            vminFactoryMock.Setup(o => o.Get("CR1@F5", 1)).Returns(vminForwardingUpdatedDataMock.Object);

            DDG.VminForwarding.Service = vminFactoryMock.Object;

            // mock the updated check data
            var checkData = new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>
            {
                {
                    "CR", new Dictionary<string, List<VminForwardingCornerRecord>>
                    {
                        {
                            "CR0", new List<VminForwardingCornerRecord>
                            {
                                CreateActiveVminRecord("CR0@F1", 1, freqF1, 2.0),
                                CreateActiveVminRecord("CR0@F3", 1, freqF3, 3.0),
                                CreateActiveVminRecord("CR0@F6", 1, freqFake, 3.0),

                                CreateActiveVminRecord("CR0@F2", 1, freqF2, 1.0),
                                CreateActiveVminRecord("CR0@F4", 1, freqFake, 1.0),
                                CreateActiveVminRecord("CR0@F5", 1, freqFake, 1.0),
                            }
                        },
                        {
                            "CR1", new List<VminForwardingCornerRecord>
                            {
                                CreateActiveVminRecord("CR1@F1", 1, freqF1, 2.0),
                                CreateActiveVminRecord("CR1@F3", 1, freqF3, 3.0),
                                CreateActiveVminRecord("CR1@F6", 1, freqFake, 3.0),

                                CreateActiveVminRecord("CR1@F2", 1, freqF2, 1.0),
                                CreateActiveVminRecord("CR1@F4", 1, freqFake, 1.0),
                                CreateActiveVminRecord("CR1@F5", 1, freqFake, 1.0),
                            }
                        },
                    }
                },
            };

            var interpData = new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>
            {
                {
                    "CR", new Dictionary<string, List<VminForwardingCornerRecord>>
                    {
                        {
                            "CR0", new List<VminForwardingCornerRecord>
                            {
                                CreateActiveVminRecord("CR0@F1", 1, freqF1, 2.0),
                                CreateActiveVminRecord("CR0@F3", 1, freqF3, 3.0),
                                CreateActiveVminRecord("CR0@F6", 1, freqFake, 3.0),

                                CreateActiveVminRecord("CR0@F2", 1, freqF2, 2.5),
                                CreateActiveVminRecord("CR0@F4", 1, freqFake, 3.0),
                                CreateActiveVminRecord("CR0@F5", 1, freqFake, 3.0),
                            }
                        },
                        {
                            "CR1", new List<VminForwardingCornerRecord>
                            {
                                CreateActiveVminRecord("CR1@F1", 1, freqF1, 2.0),
                                CreateActiveVminRecord("CR1@F3", 1, freqF3, 3.0),
                                CreateActiveVminRecord("CR1@F6", 1, freqFake, 3.0),

                                CreateActiveVminRecord("CR1@F2", 1, freqF2, 2.5),
                                CreateActiveVminRecord("CR1@F4", 1, freqFake, 3.0),
                                CreateActiveVminRecord("CR1@F5", 1, freqFake, 3.0),
                            }
                        },
                    }
                },
            };

            var exportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            exportHandlerMock.SetupSequence(o => o.GetProcessedCornersData())
                .Returns(checkData)
                .Returns(interpData);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(exportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            // mock the ituff datalogger
            var ituffWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            ituffWriterMock.Setup(o => o.SetTnamePostfix("_search_results"));
            ituffWriterMock.Setup(o => o.SetData("CR:3.000^1v1%3.000^1v1%3.000^1v1%3.000^1v1%2.500^1v1%2.000^1v1"));
            ituffWriterMock.Setup(o => o.SetTnamePostfix("_check_results"));
            ituffWriterMock.Setup(o => o.SetData("CR:3.000^3v3%3.000^1v1%3.000^1v1%3.000^3v3%2.500^1v1%2.000^2v2"));
            ituffWriterMock.Setup(o => o.SetTnamePostfix("_interpolation_results"));
            ituffWriterMock.Setup(o => o.SetData("CR:3.000^3v3%3.000^3v3%3.000^3v3%3.000^3v3%2.500^2.5v2.5%2.000^2v2"));

            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(ituffWriterMock.Object);
            datalogMock.Setup(o => o.WriteToItuff(ituffWriterMock.Object));
            Prime.Services.DatalogService = datalogMock.Object;

            // run the test.
            var domains = new List<string> { "CR" };
            var cornersToInterpolateFrom = new List<string> { "F1", "F3", "F6" };
            VminForwardingPrediction.PrimeSTCInterpolation(domains, cornersToInterpolateFrom, 1);

            // check the results.
            vminForwardingUpdatedDataMock.Verify(o => o.StoreVminResult(expectedVmins), Times.Exactly(4)); // 2 cores * 2 corners
            vminForwardingUpdatedDataMockF2.Verify(o => o.StoreVminResult(expectedVminsF2), Times.Exactly(2)); // 2 cores * 1 corners
            datalogMock.Verify(o => o.WriteToItuff(ituffWriterMock.Object), Times.Exactly(3));
            ituffWriterMock.VerifyAll();
            vminFactoryMock.VerifyAll();
            exportHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the STCInterpolation with the prime vminforwarding table.
        /// </summary>
        [TestMethod]
        public void ApplySTCInterpolationToPrime_MissingSnapshotData_Fail()
        {
            // dummy mock the ituff datalogger
            var ituffWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Loose);
            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(ituffWriterMock.Object);
            datalogMock.Setup(o => o.WriteToItuff(ituffWriterMock.Object));
            Prime.Services.DatalogService = datalogMock.Object;

            var activeDataF1Only = new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>
            {
                {
                    "CLR", new Dictionary<string, List<VminForwardingCornerRecord>>
                    {
                        {
                            "CLR", new List<VminForwardingCornerRecord>
                            {
                                CreateActiveVminRecord("CLR@F1", 1, 1000000000, 1.0),
                            }
                        },
                    }
                },
            };

            var exportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            exportHandlerMock.Setup(o => o.GetProcessedCornersData()).Returns(activeDataF1Only);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(exportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            // Mock the Prime Vmin Configuration tables.
            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            vminFactoryMock.Setup(o => o.GetInstanceNamesForDomain("CLR")).Returns(new List<string> { "CLR" });
            vminFactoryMock.Setup(o => o.GetCornerNamesForDomainInstance("CLR")).Returns(new List<string> { "CLR@F3", "CLR@F2", "CLR@F1" });

            // Mock snapshot data for one of the corners.
            // vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot(It.IsAny<string>(), 1)).Returns(-9999);
            // vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CLR@F1", 1)).Returns(1.0);
            VminForwardingCornerData nullCornerData = null;
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CLR@F3")).Returns(nullCornerData);
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CLR@F2")).Returns(nullCornerData);
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CLR@F1")).Returns(CreateVminData(1, 1000000000, 1.0));

            // Save the factory mock
            DDG.VminForwarding.Service = vminFactoryMock.Object;

            // Run test.
            VminForwardingPrediction.PrimeSTCInterpolation(new List<string> { "CLR" }, new List<string> { "F1", "F3" }, 1);
            vminFactoryMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the STCInterpolation with the prime vminforwarding table.
        /// </summary>
        [TestMethod]
        public void ApplySTCInterpolationToPrime_NoCheckData_Fail()
        {
            // dummy mock the ituff datalogger
            var ituffWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Loose);
            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(ituffWriterMock.Object);
            datalogMock.Setup(o => o.WriteToItuff(ituffWriterMock.Object));
            Prime.Services.DatalogService = datalogMock.Object;

            var activeDataF1Only = new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>
            {
                {
                    "CLR", new Dictionary<string, List<VminForwardingCornerRecord>>
                    {
                        {
                            "CLR", new List<VminForwardingCornerRecord>
                            {
                                CreateActiveVminRecord("CLR@F1", 1, 1000000000, 1.0),
                            }
                        },
                    }
                },
            };

            var exportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            exportHandlerMock.Setup(o => o.GetProcessedCornersData()).Returns(activeDataF1Only);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(exportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            // Mock the Prime Vmin Configuration tables.
            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            vminFactoryMock.Setup(o => o.GetInstanceNamesForDomain("CLR")).Returns(new List<string> { "CLR" });
            vminFactoryMock.Setup(o => o.GetCornerNamesForDomainInstance("CLR")).Returns(new List<string> { "CLR@F3", "CLR@F2", "CLR@F1" });

            // Mock snapshot data for all of the corners.
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CLR@F3")).Returns(CreateVminData(1, 1000000000, 1.0));
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CLR@F2")).Returns(CreateVminData(1, 1000000000, 1.0));
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CLR@F1")).Returns(CreateVminData(1, 1000000000, 1.0));

            // Save the factory mock
            DDG.VminForwarding.Service = vminFactoryMock.Object;

            // Run test.
            VminForwardingPrediction.PrimeSTCInterpolation(new List<string> { "CLR" }, new List<string> { "F1", "F3" }, 1);
            vminFactoryMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Test the STCInterpolation with the prime vminforwarding table.
        /// </summary>
        [TestMethod]
        public void ApplySTCInterpolationToPrime_NoSourceData_Fail()
        {
            // dummy mock the ituff datalogger
            var ituffWriterMock = new Mock<IStrgvalFormat>(MockBehavior.Loose);
            var datalogMock = new Mock<IDatalogService>(MockBehavior.Strict);
            datalogMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(ituffWriterMock.Object);
            datalogMock.Setup(o => o.WriteToItuff(ituffWriterMock.Object));
            Prime.Services.DatalogService = datalogMock.Object;

            var activeDataF1Only = new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>
            {
                {
                    "CLR", new Dictionary<string, List<VminForwardingCornerRecord>>
                    {
                        {
                            "CLR", new List<VminForwardingCornerRecord>
                            {
                                CreateActiveVminRecord("CLR@F1", 1, 1000000000, 1.0),
                                CreateActiveVminRecord("CLR@F3", 1, 1000000000, 1.0),
                            }
                        },
                    }
                },
            };

            var exportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            exportHandlerMock.Setup(o => o.GetProcessedCornersData()).Returns(activeDataF1Only);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(exportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            // Mock the Prime Vmin Configuration tables.
            var vminFactoryMock = new Mock<DDG.IVminForwardingFactory>(MockBehavior.Strict);
            vminFactoryMock.Setup(o => o.GetInstanceNamesForDomain("CLR")).Returns(new List<string> { "CLR" });
            vminFactoryMock.Setup(o => o.GetCornerNamesForDomainInstance("CLR")).Returns(new List<string> { "CLR@F3", "CLR@F2", "CLR@F1" });

            // Mock snapshot data for all of the corners.
            VminForwardingCornerData nullCornerData = null;
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CLR@F3")).Returns(CreateVminData(1, 1000000000, 1.0));
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CLR@F2")).Returns(nullCornerData);
            vminFactoryMock.Setup(o => o.GetVminForwardingSnapshot($"CLR@F1")).Returns(CreateVminData(1, 1000000000, 1.0));

            // Save the factory mock
            DDG.VminForwarding.Service = vminFactoryMock.Object;

            // Run test.
            VminForwardingPrediction.PrimeSTCInterpolation(new List<string> { "CLR" }, new List<string> { "F1", "F3" }, 1);
            vminFactoryMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Get the VMinPrediction.GetVoltage().
        /// </summary>
        [TestMethod]
        public void GetVoltage_Interpolation_Pass()
        {
            var vminData = CreateSimpleVminTable();
            var exportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            exportHandlerMock.SetupSequence(o => o.GetProcessedCornersData()).Returns(vminData);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(exportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var predictor = new VminForwardingPrediction();
            Assert.AreEqual(5.0, predictor.GetVoltage("CR0", 5.0));
            exportHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Get the VMinPrediction.GetVoltage().
        /// </summary>
        [TestMethod]
        public void GetVoltage_MatchExistingFreq_Pass()
        {
            var vminData = CreateSimpleVminTable();
            var exportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            exportHandlerMock.SetupSequence(o => o.GetProcessedCornersData()).Returns(vminData);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(exportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var predictor = new VminForwardingPrediction();
            Assert.AreEqual(2.0, predictor.GetVoltage("CR0", 2.0));
            exportHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Get the VMinPrediction.GetVoltage().
        /// </summary>
        [TestMethod]
        public void GetVoltage_NoActiveData_Exception()
        {
            var vminData = new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>();
            var exportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            exportHandlerMock.SetupSequence(o => o.GetProcessedCornersData()).Returns(vminData);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(exportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var predictor = new VminForwardingPrediction();
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => predictor.GetVoltage("CR0", 1.0));
            Assert.AreEqual("VMinForwarding table is empty, cannot get Voltage for DomainInstance=[CR0].", ex.Message);

            exportHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Get the VMinPrediction.GetVoltage().
        /// </summary>
        [TestMethod]
        public void GetVoltage_InvalidInstance_Exception()
        {
            var vminData = CreateSimpleVminTable();
            var exportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            exportHandlerMock.SetupSequence(o => o.GetProcessedCornersData()).Returns(vminData);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(exportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var predictor = new VminForwardingPrediction();
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => predictor.GetVoltage("invalidinstance", 1.0));
            Assert.AreEqual("DomainInstance=[invalidinstance] does not exist in VMinForwarding table.", ex.Message);
            exportHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Get the VMinPrediction.GetVoltage().
        /// </summary>
        [TestMethod]
        public void GetVoltage_NoActiveDataForInstance_Exception()
        {
            var vminData = new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>();
            vminData["CR"] = new Dictionary<string, List<VminForwardingCornerRecord>>();
            vminData["CR"]["CR0"] = new List<VminForwardingCornerRecord> { CreateActiveVminRecord("CR0@F1", 3, 99000000000d, 22.0) }; // 99Ghz should be stripped out as invalid.
            var exportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            exportHandlerMock.SetupSequence(o => o.GetProcessedCornersData()).Returns(vminData);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(exportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var predictor = new VminForwardingPrediction();
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => predictor.GetVoltage("CR0", 1.0));
            Assert.AreEqual("DomainInstance=[CR0] does not have any active data in the VMinForwarding table.", ex.Message);
            exportHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Get the VMinPrediction.GetVoltage().
        /// </summary>
        [TestMethod]
        public void GetVoltage_OnlyOneActiveDataForInstance_Exception()
        {
            var vminData = new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>();
            vminData["CR"] = new Dictionary<string, List<VminForwardingCornerRecord>>();
            vminData["CR"]["CR0"] = new List<VminForwardingCornerRecord> { CreateActiveVminRecord("CR0@F1", 1, 2000000000d, 2.0) };

            var exportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            exportHandlerMock.SetupSequence(o => o.GetProcessedCornersData()).Returns(vminData);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(exportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var predictor = new VminForwardingPrediction();
            var ex = Assert.ThrowsException<Prime.Base.Exceptions.TestMethodException>(() => predictor.GetVoltage("CR0", 1.0));
            Assert.AreEqual("DomainInstance=[CR0] only has one active vmin at [2.000Ghz], Cannot interpolate to [1.000Ghz].", ex.Message);
            exportHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        /// <summary>
        /// Get the VMinPrediction.GetVoltage().
        /// </summary>
        [TestMethod]
        public void GetVoltage_OnlyOneActiveDataForInstance_Pass()
        {
            var vminData = new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>();
            vminData["CR"] = new Dictionary<string, List<VminForwardingCornerRecord>>();
            vminData["CR"]["CR0"] = new List<VminForwardingCornerRecord> { CreateActiveVminRecord("CR0@F1", 1, 2000000000d, 2.0) };
            var exportHandlerMock = new Mock<IVminForwardingExportHandler>(MockBehavior.Strict);
            exportHandlerMock.SetupSequence(o => o.GetProcessedCornersData()).Returns(vminData);

            var vminForwardingServiceMock = new Mock<IVminForwardingService>(MockBehavior.Strict);
            vminForwardingServiceMock.Setup(o => o.CreateExportHandler()).Returns(exportHandlerMock.Object);
            Prime.Services.VminForwardingService = vminForwardingServiceMock.Object;

            var predictor = new VminForwardingPrediction();
            Assert.AreEqual(2.0, predictor.GetVoltage("CR0", 2.0));
            exportHandlerMock.VerifyAll();
            vminForwardingServiceMock.VerifyAll();
        }

        private static Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>> CreateSimpleVminTable()
        {
            var vminData = new Dictionary<string, Dictionary<string, List<VminForwardingCornerRecord>>>
            {
                {
                    "CR", new Dictionary<string, List<VminForwardingCornerRecord>>
                    {
                        {
                            "CR0", new List<VminForwardingCornerRecord>
                            {
                                CreateActiveVminRecord("CR0@F1", 1, 1000000000d, 1.0),
                                CreateActiveVminRecord("CR0@F3", 1, 2000000000d, 2.0),
                                CreateActiveVminRecord("CR0@F6", 1, 3000000000d, 3.0),
                            }
                        },
                    }
                },
            };

            return vminData;
        }

        private static VminForwardingCornerRecord CreateActiveVminRecord(string name, int flow, double freq, double voltage)
        {
            var activeData = new VminForwardingCornerRecord();
            activeData.Key = name;
            if (voltage > 0)
            {
                activeData.ActiveCornerData = new VminForwardingCornerData();
                activeData.ActiveCornerData.Flow = flow;
                activeData.ActiveCornerData.Frequency = freq;
                activeData.ActiveCornerData.Voltage = voltage;
            }
            else
            {
                activeData.ActiveCornerData = null;
            }

            return activeData;
        }

        private static VminForwardingCornerData CreateVminData(int flow, double freq, double voltage)
        {
            var data = new VminForwardingCornerData();
            data.Flow = flow;
            data.Frequency = freq;
            data.Voltage = voltage;

            return data;
        }
    }
}
