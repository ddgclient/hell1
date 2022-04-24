// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// INTEL CONFIDENTIAL
// Copyright (2019) (2020) Intel Corporation
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

namespace PrimeValTool_unitTest
{
    using Wrapper;
    using System.Collections.Generic;
    using PrimeValTool;
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Unit tests for the <see cref="RuntimeCollaterals"/> class.
    /// </summary>
    [TestClass]
    public class RuntimeSettings_UnitTest
    {
        Mock<IEnvironmentVariableHandler> f_envHandler = new Mock<IEnvironmentVariableHandler>();
        Mock<IDirectoryHandler> f_dirHandler = new Mock<IDirectoryHandler>();

        /// <summary>
        /// Returns the directory that the automation directory should be set to. Only works if the solution is the sub-directory of a "pVal" folder.
        /// </summary>
        /// <returns> The location for the automation directory for pVal execution. </returns>
        public string ReturnRequiredAutomationDirectory() => Directory.GetCurrentDirectory();

        [TestInitialize]
        public void Init()
        {
            f_envHandler
                .Setup(x => x.RetrievePathFromEnvironmentVariable(It.IsAny<string>()))
                .Returns("C//:FakeDir");
            Handlers.EnvironmentVariableHandler = f_envHandler.Object;

            f_dirHandler
                .Setup(x => x.ReadTextFromPath(It.IsAny<string>()))
                .Returns((string path) =>
                    {
                        return System.IO.File.ReadAllText(path);
                    });
            Handlers.DirectoryHandler = f_dirHandler.Object;
        }

        [TestCleanup]
        public void Cleanup()
        {
            f_dirHandler
                .Setup(x => x.ReadTextFromPath(It.IsAny<string>()))
                .Returns((string path) =>
                {
                    return System.IO.File.ReadAllText(path);
                });
            Handlers.DirectoryHandler = f_dirHandler.Object;
        }

        /// <summary>
        /// Dummy summary.
        /// </summary>
        [TestMethod]
        public void InitializeLogDirectories_Success() //FIXME: Test dependent on ENV vars on sys. Check to see if we can mock env vars
        {
            var collaterals = RuntimeCollaterals.InitializeToolCollaterals();
            bool logsPathCorrect =
                collaterals.TOSLogsPath == Path.GetFullPath(".\\test_collaterals\\fakeTP\\logs");
            bool mainLogsPathCorrect =
                collaterals.FinalReportLogsPath == Path.GetFullPath(".\\test_collaterals\\fakeTP\\logs");
            Assert.IsTrue(logsPathCorrect && mainLogsPathCorrect);
        }

        /// <summary>
        /// Dummy description.
        /// </summary>
        [TestMethod]
        public void ConvertToAbsolute_Empty_Fail()
        {
            string injectionText = File.ReadAllText(
                ".\\test_collaterals\\test_pval_configs\\pValRunList_empty_params.json");

            f_dirHandler
                .Setup(x => x.ReadTextFromPath(It.IsAny<string>()))
                .Returns(injectionText);

            try
            {
                RuntimeCollaterals.InitializeToolCollaterals();
            }
            catch(Exception ex)
            {
                Assert.AreEqual("User input in configuration was empty when it was required for execution.\n" +
                        "Required elements are: Tpl, Stpl, Soc, Plist, Env(Release or Debug). Please check these for proper values.", ex.Message);
            }
        }

        /// <summary>
        /// Dummy description.
        /// </summary>
        [TestMethod]
        public void ConvertToAbsolute_stplFileEmpty_Fail()
        {
            string injectionText = File.ReadAllText(
                ".\\test_collaterals\\test_pval_configs\\pValRunList_fake_stpl.json");
            string expectedErrorPath = Path.GetFullPath(".\\test_collaterals\\DoesNotExist\\SubTestPlan.stpl");

            f_dirHandler
                .Setup(x => x.ReadTextFromPath(It.IsAny<string>()))
                .Returns(injectionText);

            try
            {
                var tools = RuntimeCollaterals.InitializeToolCollaterals();
            }
            catch (Exception e)
            {
                Assert.AreEqual($"File [{expectedErrorPath}] defined in pValRunList.json was not found.", e.Message);
                return;
            }

            Assert.IsTrue(false, "No exception raised for fake STPL file path");
        }

        /// <summary>
        /// Looks like to be a test that tries to find the given tosName within the json file we create...
        /// not sure why this is just checking if a JSON file exists in the auto directory
        /// </summary>
        [TestMethod]
        public void ParsePlanListFile_TosNameNotMatching_Fail()
        {
            var tools = RuntimeCollaterals.InitializeToolCollaterals();
            string tosName = "9999";
            try
            {
                tools.SettingsFile.ReturnTestPlanMatchingWithTOSName(tosName);
            }
            catch (Exception e)
            {
                Assert.AreEqual($"No found test plans found in user defined .json file for TOS version [{tosName}]", e.Message);
            }
        }

        /// <summary>
        /// Looks like to be a test that tries to find the given tosName within the json file we create...
        /// not sure why this is just checking if a JSON file exists in the auto directory
        /// </summary>
        [TestMethod]
        public void ParsePlanListFile_TosNameMatching()
        {
            var tools = RuntimeCollaterals.InitializeToolCollaterals();
            string tosName = "TOS3900";
            var testPlanList = tools.SettingsFile.ReturnTestPlanMatchingWithTOSName(tosName);
            Assert.AreEqual(testPlanList[0].TestPlanName, "FakeTestPlan");
        }
    }
}