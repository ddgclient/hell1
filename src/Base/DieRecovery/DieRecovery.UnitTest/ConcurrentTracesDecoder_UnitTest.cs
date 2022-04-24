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

namespace DieRecoveryBase.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DDG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Prime.ConsoleService;
    using Prime.FunctionalService;
    using Prime.PerformanceService;
    using Prime.PlistService;
    using Prime.SharedStorageService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConcurrentTracesDecoder_UnitTest
    {
        private string configurationJson;
        private Mock<IPlistObject> plistObjectMock;
        private Mock<IPlistService> plistServiceMock;
        private Mock<ICaptureFailureTest> captureFailureTestMock;
        private IPinMapDecoder decoder;
        private Mock<IPlistObject> subPlistObjectMock;

        private Dictionary<string, string> SharedStorageValues { get; set; }

        private Mock<ISharedStorageService> SharedStorageMock { get; set; }

        /// <summary>
        /// Initializes all tests.
        /// </summary>
        [TestInitialize]
        public void InitializingTestMethod()
        {
            DDG.PlistModifications.Service.CleanTree(string.Empty);
            var consoleServiceMock = new Mock<IConsoleService>(MockBehavior.Loose);
            consoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string msg) =>
            {
                System.Console.WriteLine($"DEBUG: {msg}");
            });

            Prime.Services.ConsoleService = consoleServiceMock.Object;

            // Default Mock for Shared service.
            this.SharedStorageValues = new Dictionary<string, string>();
            this.SharedStorageMock = new Mock<ISharedStorageService>(MockBehavior.Strict);
            this.SharedStorageMock.Setup(o =>
                    o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Context>()))
                .Callback((string key, object obj, Context context) =>
                {
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.SharedStorageMock.Setup(o =>
                    o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, string obj, Context context) =>
                {
                    this.SharedStorageValues[key] = obj;
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.SharedStorageMock.Setup(o =>
                    o.InsertRowAtTable(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<Context>()))
                .Callback((string key, double obj, Context context) =>
                {
                    this.SharedStorageValues[key] = JsonConvert.SerializeObject(obj);
                    Console.WriteLine($"Saving SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                });
            this.SharedStorageMock
                .Setup(o => o.GetRowFromTable(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Context>()))
                .Callback((string key, Type obj, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Type obj, Context context) =>
                    JsonConvert.DeserializeObject(this.SharedStorageValues[key], obj));
            this.SharedStorageMock.Setup(o => o.GetDoubleRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => double.Parse(this.SharedStorageValues[key]));
            this.SharedStorageMock.Setup(o => o.GetStringRowFromTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Callback((string key, Context context) =>
                {
                    Console.WriteLine($"Extracting SharedStorage Key={key} Value={this.SharedStorageValues[key]}");
                    if (!this.SharedStorageValues.ContainsKey(key))
                    {
                        throw new Prime.Base.Exceptions.FatalException($"[{key}] not found in shared storage.");
                    }
                })
                .Returns((string key, Context context) => this.SharedStorageValues[key]);
            this.SharedStorageMock.Setup(o => o.KeyExistsInObjectTable(It.IsAny<string>(), It.IsAny<Context>()))
                .Returns((string key, Context c) => this.SharedStorageValues.ContainsKey(key));
            Prime.Services.SharedStorageService = this.SharedStorageMock.Object;

            var performanceServiceMock = new Mock<IPerformanceService>(MockBehavior.Loose);
            Prime.Services.PerformanceService = performanceServiceMock.Object;

            this.plistObjectMock = new Mock<IPlistObject>(MockBehavior.Strict);
            this.subPlistObjectMock = new Mock<IPlistObject>(MockBehavior.Strict);
            this.plistServiceMock = new Mock<IPlistService>(MockBehavior.Strict);
            this.plistServiceMock.Setup(o => o.GetPlistObject("patlist0")).Returns(this.plistObjectMock.Object);
            this.plistServiceMock.Setup(o => o.GetPlistObject("SUBPLIST")).Returns(this.subPlistObjectMock.Object);
            Prime.Services.PlistService = this.plistServiceMock.Object;

            this.captureFailureTestMock = new Mock<ICaptureFailureTest>(MockBehavior.Strict);
            this.captureFailureTestMock.As<IFunctionalTest>().Setup(o => o.GetPlistName()).Returns("patlist0");

            this.configurationJson =
@"
{
    'Name': 'CCR_map',
    'Size': 7,
    'Description': 'this is a comment and will be ignored. ',
    'MaskConfigurations': [
        {
            'Comment': 'CCF. TargetPositions: C0,C1,C2,C3,CCF,MBIST,GT',
            'TargetPositions': [4],
            'Options':
            {
                'Mask': 'NOAB_08,NOAB_09,NOAB_10,NOAB_11'
            }
        },
        {
            'Comment': 'GT0. TargetPositions: C0,C1,C2,C3,CCF,MBIST,GT',
            'TargetPositions': [6],
            'PatternNames': [
                '.*FUN_GT.*'
            ],
            'Options':
            {
                'Mask': 'all_leg_pins,all_ddr_pins',
                'DisableCapture': 'LEG:0',
                'DisableCompare': 'LEG:0'
            }
        },
        {
            'Comment': 'C0. TargetPositions: C0,C1,C2,C3,CCF,MBIST,GT',
            'TargetPositions': [0],
            'PatternNames': [
                '.*CORE.*'
            ],
            'Options':
            {
                'Mask': 'NOAB_00'
            }
        },
        {
            'Comment': 'C1. TargetPositions: C0,C1,C2,C3,CCF,MBIST,GT',
            'TargetPositions': [1],
            'PatternNames': [
                '.*CORE.*'
            ],
            'Options':
            {
                'Mask': 'NOAB_01'
            }
        },
        {
            'Comment': 'C2. TargetPositions: C0,C1,C2,C3,CCF,MBIST,GT',
            'TargetPositions': [2],
            'PatternNames': [
                '.*CORE.*'
            ],
            'Options':
            {
                'Mask': 'NOAB_02'
            }
        },
        {
            'Comment': 'C3. TargetPositions: C0,C1,C2,C3,CCF,MBIST,GT',
            'TargetPositions': [3],
            'PatternNames': [
                '.*CORE.*'
            ],
            'Options':
            {
                'Mask': 'NOAB_03'
            }
        }
    ],
    'Entries': [
        {
            'Comment': 'Failing Mbist. TargetPositions: C0,C1,C2,C3,CCF,MBIST,GT',
            'FailFilters': {
                'Burst': 1,
                'PatternName': 'MBIST_CHECK',
                'PatternOccurrence': 1,
                'FailingPins': [
                    'TDO'
                ],
                'TargetPositions': [5]
            },
            'StartPattern': {
                'Burst': 1,
                'PatternName': 'MBIST_PROGRAM',
                'PatternOccurrence': 1,
            },
            'PreBurstPList': {
                'PreBurstPList': 'mbist_reset_list',
            },
            'PlistElementOptions': [
                {
                    'Index': [6],
                    'Options':
                    {
                        'Mask': 'all_leg_pins,all_ddr_pins',
                        'DisableCapture': 'LEG:0'
                    }
                },
                {
                    'Index': [15],
                    'Options':
                    {
                        'Mask': 'all_leg_pins,all_ddr_pins'
                    }
                }
            ]
        },
        {
            'Comment': 'Failing CORE. TargetPositions: C0,C1,C2,C3,CCF,MBIST,GT',
            'FailFilters': {
                'Burst': 2,
                'PatternName': 'CORE_CHECK',
                'PatternOccurrence': 1,
                'FailingPins': [
                    'NOAB_00,NOAB_08',
                    'NOAB_01,NOAB_09',
                    'NOAB_02,NOAB_10',
                    'NOAB_03,NOAB_11'
                ],
                'TargetPositions': [
                    0,
                    1,
                    2,
                    3
                ]
            },
            'StartPattern': {
                'Burst': 2,
                'PatternName': 'CCF_PROGRAM',
                'PatternOccurrence': 1,
            },
            'PreBurstPList': {
                'Patlist': 'patlist0',
                'PreBurstPList': 'ccf_reset_list'
            }
        },
        {
            'Comment': 'Failing GT. TargetPositions: C0,C1,C2,C3,CCF,MBIST,GT',
            'FailFilters': {
                'Burst': 1,
                'PatternName': 'GT_CHECK',
                'PatternOccurrence': 1,
                'TargetPositions': [6]
            }
        },
        {
            'Comment': 'Failing GT. TargetPositions: C0,C1,C2,C3,CCF,MBIST,GT',
            'FailFilters': {
                'Burst': 1,
                'PatternName': 'FUN_GT1',
                'PatternOccurrence': 1,
                'TargetPositions': [6]
            },
            'PreBurstPList': {
                'Patlist': 'patlist0',
                'PreBurstPList': 'ccf_reset_list'
            },
            'PlistElementOptions': [
                {
                    'Patlist': 'SUBPLIST',
                    'Index': [1],
                    'Options':
                    {
                        'Mask': 'all_leg_pins,all_ddr_pins',
                        'DisableCapture': 'LEG:0'
                    }
                }
            ]
        },
        {
            'Comment': 'Failing GT. TargetPositions: C0,C1,C2,C3,CCF,MBIST,GT',
            'FailFilters': {
                'Burst': 1,
                'PatternName': 'FUN_GT2',
                'PatternOccurrence': 1,
                'TargetPositions': [6]
            },
            'PlistElementOptions': [
                {
                    'Patlist': 'SUBPLIST',
                    'Index': [1],
                    'Options':
                    {
                        'Mask': 'all_leg_pins,all_ddr_pins',
                        'DisableCapture': 'LEG:0'
                    }
                }
            ]
        }
    ]
}
";
        }

        /// <summary>
        /// Mock plist contents.
        /// </summary>
        public void MockPlistContentsIndex()
        {
            var tuple0 = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple0.Setup(o => o.IsPattern()).Returns(true);
            tuple0.Setup(o => o.GetPlistItemName()).Returns("CCF_PROGRAM");
            tuple0.Setup(o => o.GetPatternIndex()).Returns(0);
            var tuple1 = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple1.Setup(o => o.IsPattern()).Returns(true);
            tuple1.Setup(o => o.GetPlistItemName()).Returns("CORE_PROGRAM");
            tuple1.Setup(o => o.GetPatternIndex()).Returns(1);
            var tuple2 = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple2.Setup(o => o.IsPattern()).Returns(true);
            tuple2.Setup(o => o.GetPlistItemName()).Returns("MBIST_PROGRAM");
            tuple2.Setup(o => o.GetPatternIndex()).Returns(2);
            var tuple3 = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple3.Setup(o => o.IsPattern()).Returns(true);
            tuple3.Setup(o => o.GetPlistItemName()).Returns("FUN_GT1");
            tuple3.Setup(o => o.GetPatternIndex()).Returns(3);
            var tuple4 = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple4.Setup(o => o.IsPattern()).Returns(true);
            tuple4.Setup(o => o.GetPlistItemName()).Returns("FUN_GT2");
            tuple4.Setup(o => o.GetPatternIndex()).Returns(4);
            var tuple5 = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple5.Setup(o => o.IsPattern()).Returns(true);
            tuple5.Setup(o => o.GetPlistItemName()).Returns("CORE_CHECK");
            tuple5.Setup(o => o.GetPatternIndex()).Returns(5);
            var tuple6 = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple6.Setup(o => o.IsPattern()).Returns(true);
            tuple6.Setup(o => o.GetPlistItemName()).Returns("CCF_CHECK");
            tuple6.Setup(o => o.GetPatternIndex()).Returns(6);
            var tuple7 = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple7.Setup(o => o.IsPattern()).Returns(true);
            tuple7.Setup(o => o.GetPlistItemName()).Returns("MBIST_CHECK");
            tuple7.Setup(o => o.GetPatternIndex()).Returns(7);
            var subPlist8 = new Mock<IPlistContent>(MockBehavior.Strict);
            subPlist8.Setup(o => o.IsPattern()).Returns(false);
            subPlist8.Setup(o => o.GetPlistItemName()).Returns("SUBPLIST");
            subPlist8.Setup(o => o.GetPatternIndex()).Returns(8);
            var tuple0A = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple0A.Setup(o => o.IsPattern()).Returns(true);
            tuple0A.Setup(o => o.GetPlistItemName()).Returns("CCF_PROGRAM");
            tuple0A.Setup(o => o.GetPatternIndex()).Returns(0);
            var tuple1A = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple1A.Setup(o => o.IsPattern()).Returns(true);
            tuple1A.Setup(o => o.GetPlistItemName()).Returns("CORE_PROGRAM");
            tuple1A.Setup(o => o.GetPatternIndex()).Returns(1);
            var tuple2A = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple2A.Setup(o => o.IsPattern()).Returns(true);
            tuple2A.Setup(o => o.GetPlistItemName()).Returns("MBIST_PROGRAM");
            tuple2A.Setup(o => o.GetPatternIndex()).Returns(2);
            var tuple3A = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple3A.Setup(o => o.IsPattern()).Returns(true);
            tuple3A.Setup(o => o.GetPlistItemName()).Returns("FUN_GT1");
            tuple3A.Setup(o => o.GetPatternIndex()).Returns(3);
            var tuple4A = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple4A.Setup(o => o.IsPattern()).Returns(true);
            tuple4A.Setup(o => o.GetPlistItemName()).Returns("FUN_GT2");
            tuple4A.Setup(o => o.GetPatternIndex()).Returns(4);
            var tuple5A = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple5A.Setup(o => o.IsPattern()).Returns(true);
            tuple5A.Setup(o => o.GetPlistItemName()).Returns("CORE_CHECK");
            tuple5A.Setup(o => o.GetPatternIndex()).Returns(5);
            var tuple6A = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple6A.Setup(o => o.IsPattern()).Returns(true);
            tuple6A.Setup(o => o.GetPlistItemName()).Returns("CCF_CHECK");
            tuple6A.Setup(o => o.GetPatternIndex()).Returns(6);
            var tuple7A = new Mock<IPlistContent>(MockBehavior.Strict);
            tuple7A.Setup(o => o.IsPattern()).Returns(true);
            tuple7A.Setup(o => o.GetPlistItemName()).Returns("MBIST_CHECK");
            tuple7A.Setup(o => o.GetPatternIndex()).Returns(7);

            var topIndex = new List<IPlistContent> { tuple0.Object, tuple1.Object, tuple2.Object, tuple3.Object, tuple4.Object, tuple5.Object, tuple6.Object, tuple7.Object, subPlist8.Object };
            var subIndex = new List<IPlistContent> { tuple0A.Object, tuple1A.Object, tuple2A.Object, tuple3A.Object, tuple4A.Object, tuple5A.Object, tuple6A.Object, tuple7A.Object };
            this.plistObjectMock.Setup(o => o.GetPatternsAndIndexes(false))
                .Returns(topIndex);
            this.subPlistObjectMock.Setup(o => o.GetPatternsAndIndexes(false))
                .Returns(subIndex);
        }

        /// <summary>
        /// Mock plist contents.
        /// </summary>
        public void MockEmptyPrePlist()
        {
            this.MockPlistContentsIndex();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        public void ReadInputFile()
        {
            this.decoder = (IPinMapDecoder)JsonConvert.DeserializeObject(this.configurationJson, typeof(ConcurrentTracesDecoder));
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ApplyPlistSettings_AllZeros_Pass()
        {
            this.MockPlistContentsIndex();
            this.captureFailureTestMock.Setup(o => o.Reset());
            this.captureFailureTestMock.Setup(o => o.HasStartPattern()).Returns(false);
            this.plistObjectMock.Setup(o => o.Resolve());

            this.ReadInputFile();
            var functionalTest = this.captureFailureTestMock.As<IFunctionalTest>().Object;

            this.decoder.MaskPlistFromTracker(new BitArray(7, false), ref functionalTest);
            this.decoder.ApplyPlistSettings(new BitArray(7, false), ref functionalTest);
            this.decoder.Restore();
            this.captureFailureTestMock.VerifyAll();
            this.plistObjectMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ApplyPlistSettings_Matching_Pass()
        {
            this.MockPlistContentsIndex();
            this.plistObjectMock.Setup(o => o.SetElementOption(6, "Mask", "all_leg_pins,all_ddr_pins"));
            this.plistObjectMock.Setup(o => o.SetElementOption(6, "DisableCapture", "LEG:0"));
            this.plistObjectMock.Setup(o => o.SetElementOption(15, "Mask", "all_leg_pins,all_ddr_pins"));
            this.plistObjectMock.Setup(o => o.SetOption("PreBurstPList", "first_reset"));
            this.plistObjectMock.Setup(o => o.SetOption("PreBurstPList", "mbist_reset_list"));
            this.plistObjectMock.Setup(o => o.GetOption("PreBurstPList")).Returns("first_reset");
            this.plistObjectMock.Setup(o => o.RemoveElementOption(6, "Mask"));
            this.plistObjectMock.Setup(o => o.RemoveElementOption(6, "DisableCapture"));
            this.plistObjectMock.Setup(o => o.Resolve());

            this.ReadInputFile();
            this.captureFailureTestMock.Setup(o => o.Reset());
            this.captureFailureTestMock.Setup(o => o.HasStartPattern()).Returns(true);
            this.captureFailureTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("MBIST_CHECK", 1, 1));
            this.captureFailureTestMock.Setup(o => o.SetStartPattern("MBIST_PROGRAM", 1, 1));
            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("MBIST_CHECK");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "TDO" });
            this.captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failDataMock.Object });

            var functionalTest = this.captureFailureTestMock.As<IFunctionalTest>().Object;
            var bits = this.decoder.GetFailTrackerFromPlistResults(functionalTest);
            Assert.AreEqual("0000010", bits.ToBinaryString());
            this.decoder.MaskPlistFromTracker("0000011".ToBitArray(), ref functionalTest);
            this.decoder.ApplyPlistSettings("0000011".ToBitArray(), ref functionalTest);
            this.decoder.Restore();
            this.captureFailureTestMock.VerifyAll();
            this.plistObjectMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ApplyPlistSettings_GetOptionFails_Pass()
        {
            this.MockPlistContentsIndex();
            this.subPlistObjectMock.Setup(o => o.SetElementOption(1, "Mask", "all_leg_pins,all_ddr_pins"));
            this.subPlistObjectMock.Setup(o => o.SetElementOption(1, "DisableCapture", "LEG:0"));
            this.plistObjectMock.Setup(o => o.SetOption("PreBurstPList", "ccf_reset_list"));
            this.plistObjectMock.Setup(o => o.GetOption("PreBurstPList")).Throws(new Exception());
            this.plistObjectMock.Setup(o => o.RemoveOptions(new List<string> { "PreBurstPList" }));
            this.subPlistObjectMock.Setup(o => o.RemoveElementOption(1, "Mask"));
            this.subPlistObjectMock.Setup(o => o.RemoveElementOption(1, "DisableCapture"));
            this.plistObjectMock.Setup(o => o.Resolve());

            this.ReadInputFile();
            this.captureFailureTestMock.Setup(o => o.Reset());
            this.captureFailureTestMock.Setup(o => o.HasStartPattern()).Returns(true);
            this.captureFailureTestMock.Setup(o => o.GetStartPattern()).Returns(new Tuple<string, uint, uint>("FUN_GT1", 1, 1));
            this.captureFailureTestMock.Setup(o => o.SetStartPattern("FUN_GT1", 1, 1));
            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("FUN_GT2");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "TDO" });
            this.captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failDataMock.Object });

            var functionalTest = this.captureFailureTestMock.As<IFunctionalTest>().Object;
            var bits = this.decoder.GetFailTrackerFromPlistResults(functionalTest);
            Assert.AreEqual("0000001", bits.ToBinaryString());
            this.decoder.MaskPlistFromTracker("0000011".ToBitArray(), ref functionalTest);
            this.decoder.ApplyPlistSettings("0000011".ToBitArray(), ref functionalTest);
            this.decoder.Restore();
            this.captureFailureTestMock.VerifyAll();
            this.plistObjectMock.VerifyAll();
            this.subPlistObjectMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void ApplyPlistSettings_NoFails_Pass()
        {
            this.MockEmptyPrePlist();
            this.ReadInputFile();
            this.captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData>());
            this.captureFailureTestMock.Setup(o => o.HasStartPattern()).Returns(false);
            this.plistObjectMock.Setup(o => o.Resolve());

            var functionalTest = this.captureFailureTestMock.As<IFunctionalTest>().Object;
            var result = this.decoder.GetFailTrackerFromPlistResults(functionalTest);
            Assert.AreEqual("0000000", result.ToBinaryString());
            this.decoder.MaskPlistFromTracker("0000000".ToBitArray(), ref functionalTest);
            this.decoder.ApplyPlistSettings("0000000".ToBitArray(), ref functionalTest);
            this.captureFailureTestMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_AllFilters_Pass()
        {
            this.ReadInputFile();
            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("MBIST_CHECK");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "TDO" });
            this.captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failDataMock.Object });
            var functionalTest = this.captureFailureTestMock.As<IFunctionalTest>().Object;
            var result = this.decoder.GetFailTrackerFromPlistResults(functionalTest);
            Assert.AreEqual("0000010", result.ToBinaryString());
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_WrongFunctionalTestType_Fail()
        {
            this.ReadInputFile();
            var invalid = new Mock<ICaptureCtvPerPinTest>(MockBehavior.Strict);
            var functionalTest = invalid.As<IFunctionalTest>().Object;
            var result = Assert.ThrowsException<ArgumentException>(() => this.decoder.GetFailTrackerFromPlistResults(functionalTest));
            Assert.AreEqual("DieRecoveryBase.dll.GetFailTrackerFromPlistResults: unable to cast IFunctionalTest into ICaptureFailureTest object. Using incorrect input type for this decoder.", result.Message);
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_MultiCore_Pass()
        {
            this.ReadInputFile();
            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("CORE_CHECK");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(2);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "NOAB_09", "NOAB_03" });
            this.captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failDataMock.Object });
            var functionalTest = this.captureFailureTestMock.As<IFunctionalTest>().Object;
            var result = this.decoder.GetFailTrackerFromPlistResults(functionalTest);
            Assert.AreEqual("0101000", result.ToBinaryString());
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_NoFailPins_Pass()
        {
            this.ReadInputFile();
            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("GT_CHECK");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(1);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "DON'T_CARE" });
            this.captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failDataMock.Object });
            var functionalTest = this.captureFailureTestMock.As<IFunctionalTest>().Object;
            var result = this.decoder.GetFailTrackerFromPlistResults(functionalTest);
            Assert.AreEqual("0000001", result.ToBinaryString());
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetFailTrackerFromPlistResults_NoMatch_Exception()
        {
            this.ReadInputFile();
            var failDataMock = new Mock<IFailureData>(MockBehavior.Strict);
            failDataMock.Setup(o => o.GetPatternName()).Returns("OTHER_PATTERN");
            failDataMock.Setup(o => o.GetBurstIndex()).Returns(2);
            failDataMock.Setup(o => o.GetPatternInstanceId()).Returns(1);
            failDataMock.Setup(o => o.GetFailingPinNames()).Returns(new List<string> { "NOAB_09", "NOAB_03" });
            this.captureFailureTestMock.Setup(o => o.GetPerCycleFailures()).Returns(new List<IFailureData> { failDataMock.Object });
            var functionalTest = this.captureFailureTestMock.As<IFunctionalTest>().Object;
            var ex = Assert.ThrowsException<Exception>(() => this.decoder.GetFailTrackerFromPlistResults(functionalTest));
            Assert.AreEqual("CCR: PinMapDecoder=[CCR_map] did not find a matching entry.", ex.Message);
            failDataMock.VerifyAll();
        }

        /// <summary>
        /// Refer to test name.
        /// </summary>
        [TestMethod]
        public void GetDecoderType_Pass()
        {
            this.ReadInputFile();
            var result = this.decoder.GetDecoderType();
            Assert.AreEqual("ConcurrentTracesDecoder", result);
        }
    }
}
