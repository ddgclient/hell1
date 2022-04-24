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

namespace MEMDECODE_MIO_DDR_AC
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Prime.ConsoleService;
    using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
    using Prime.DffService;
    using Prime.FileService;
    using Prime.TestMethods;
    using Prime.TestMethods.Functional;
    using Prime.TestProgramService;
    using Prime.TpSettingsService;
    using Prime.UserVarService;

    /// <summary>
    /// Dummy description of this test method's unit test.
    /// </summary>
    [TestClass]
    public class MEMDECODE_MIO_DDR_AC_UnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MEMDECODE_MIO_DDR_AC_UnitTest"/> class.
        /// </summary>
        public MEMDECODE_MIO_DDR_AC_UnitTest()
        {
            this.ErrorOutput = new List<string>();
            this.ConsoleOutput = new List<string>();

            this.ConsoleServiceMock = new Mock<IConsoleService>(MockBehavior.Strict);
            this.ConsoleServiceMock.Setup(o => o.PrintDebug(It.IsAny<string>())).Callback((string s) =>
            {
                Console.WriteLine(s);
                this.ConsoleOutput.Add(s);
            });
            this.ConsoleServiceMock.Setup(o => o.PrintError(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<string, int, string, string>((string msg, int line, string n, string src) =>
                {
                    Console.WriteLine($"ERROR: {msg}");
                    this.ErrorOutput.Add(msg);
                });
            Prime.Services.ConsoleService = this.ConsoleServiceMock.Object;

            this.StrgvalFormatMock = new Mock<IStrgvalFormat>(MockBehavior.Strict);
            this.StrgvalFormatMock.Setup(o => o.SetData(It.IsAny<string>())).Callback((string s) =>
            {
                this.CurrentStrgvalData = s;
            });
            this.StrgvalFormatMock.Setup(o => o.SetTnamePostfix(It.IsAny<string>())).Callback((string s) =>
            {
                this.CurrentStrgvalName = s;
            });

            this.DatalogServiceMock = new Mock<IDatalogService>(MockBehavior.Strict);
            this.DatalogServiceMock.Setup(o => o.WriteToItuff(It.IsAny<IItuffFormat>())).Callback((IItuffFormat s) =>
            {
                var txt = "0_tname_TESTNAME";
                if (!string.IsNullOrEmpty(this.CurrentStrgvalName))
                {
                    txt += "_" + this.CurrentStrgvalName;
                    this.CurrentStrgvalName = string.Empty;
                }

                txt += $"\n0_strgval_{this.CurrentStrgvalData}";
                this.CurrentStrgvalData = string.Empty;

                Console.WriteLine($"[ITUFF]{txt.Replace("\n", "\n[ITUFF]")}");
            });

            this.DatalogServiceMock.Setup(o => o.GetItuffStrgvalWriter()).Returns(this.StrgvalFormatMock.Object);
            Prime.Services.DatalogService = this.DatalogServiceMock.Object;

            Console.WriteLine("Done with constructor");
        }

        private Mock<IConsoleService> ConsoleServiceMock { get; set; }

        private Mock<IStrgvalFormat> StrgvalFormatMock { get; set; }

        private Mock<IDatalogService> DatalogServiceMock { get; set; }

        private List<string> ErrorOutput { get; set; }

        private List<string> ConsoleOutput { get; set; }

        private string CurrentStrgvalData { get; set; }

        private string CurrentStrgvalName { get; set; }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CCC_MM_BS_KILL_Pass_All1()
        {
            Dictionary<string, string> ctvData = new Dictionary<string, string>();
            ctvData.Add("TDO", new string('1', 9216));

            var underTest = new CCC_MM_BS_KILL();
            ((IFunctionalExtensions)underTest).ProcessCtvPerPin(ctvData);
            Assert.IsTrue(true); /* at this point just making sure there are no exceptions */
            Assert.AreEqual(0, this.ErrorOutput.Count, $"Errors detected\n{string.Join("\n", this.ErrorOutput)}"); /* FIXME - cannot set error port yet, check that no error messages were printed out. */
        }

        /// <summary>
        /// Dummy description of this unit test.
        /// </summary>
        [TestMethod]
        public void CCC_MM_BS_KILL_Fail_All0()
        {
            Dictionary<string, string> ctvData = new Dictionary<string, string>();
            ctvData.Add("TDO", new string('0', 9216));

            var underTest = new CCC_MM_BS_KILL();
            ((IFunctionalExtensions)underTest).ProcessCtvPerPin(ctvData);
            Assert.IsTrue(true); /* at this point just making sure there are no exceptions */
            Assert.AreEqual(5, this.ErrorOutput.Count); /* FIXME - cannot set error port yet, check that the correct number of errors was logged. */
        }
    }
}
