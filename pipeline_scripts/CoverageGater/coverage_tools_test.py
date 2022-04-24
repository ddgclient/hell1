import unittest
from unittest.mock import patch
import sys
import json
from coverage_tools import *

class Test_getArguments(unittest.TestCase):
    '''Unit tests for getArguments.py'''

    def test_noArgsPassed(self):
        # arg injection setup
        testargs = ["scriptPath"]
        with patch.object(sys, 'argv', testargs):
            try:
                getArguments.getArguments()
            except Exception as ex:
                self.assertTrue(ex.args[0] == getArguments.argumentNumberExceptionMessage, "Exception raised does not match exception expected")
                return
        
        self.assertTrue(False, "No exceptions were raised during testing; one was expected.")

    def test_getArguments_configArgSetup(self):
        testargs = ["scriptPath", 
                    "--Config", ".\\test_collaterals\\testConfigs.json"]
        with patch.object(sys, 'argv', testargs):
            argObj = getArguments.getArguments()
            isValid = (
                argObj.coverageReportPath == "C:\\someDirectory\\sampleRepo\\coverageReport.json" and
                argObj.coverageTarget == 90 and
                argObj.passOverride == [ "Test1", "Test2" ] and
                argObj.failOverride == [ "Test3", "Test4" ])
            self.assertTrue(isValid, "JSON arg file was not decoded correctly.")
            return

    def test_getArguments_oneArgEmpty(self):
        # arg injection setup
        testargs = ["scriptPath", r".\example.json", ""] # FIXME: fix all tests, sys.argv first element should always be path to script...
        with patch.object(sys, 'argv', testargs):
            try:
                getArguments.getArguments()
            except Exception as ex:
                self.assertTrue(ex.args[0] == getArguments.argumentEmptyExceptionMessage, "Exception raised does not match exception expected")
                return

        self.assertTrue(False, "No exceptions were raised during testing; one was expected.")

    def test_getArguments_mixedArgs(self):
        # arg injection setup
        testargs = ["scriptPath", 
                    "--Config", ".\\test_collaterals\\testConfigs_missing.json",
                   "--Report", "C:\\someDirectory\\sampleRepo\\coverageReport.json",
                   "--Target", "90"]

        with patch.object(sys, 'argv', testargs):
            argObj = getArguments.getArguments()
            isValid = (
                argObj.coverageReportPath == "C:\\someDirectory\\sampleRepo\\coverageReport.json" and
                argObj.coverageTarget == 90 and                argObj.passOverride == [ "Test1", "Test2" ] and
                argObj.failOverride == [ "Test3", "Test4" ])
            self.assertTrue(isValid, "JSON arg file was not parsed correctly.")
            return

    def test_getArguments_noConfig(self):
        testargs = ["scriptPath", 
                   "--Report", "C:\\someDirectory\\sampleRepo\\coverageReport.json",
                   "--Target", "90"]

        with patch.object(sys, 'argv', testargs):
            argObj = getArguments.getArguments()
            isValid = (
                argObj.coverageReportPath == "C:\\someDirectory\\sampleRepo\\coverageReport.json" and
                argObj.coverageTarget == 90 and
                argObj.passOverride == None and
                argObj.failOverride == None)
            self.assertTrue(isValid, "Args were not parsed correctly.")
            return

    def test_getArguments_noConfig_coverageTargetZero(self):
        testargs = ["scriptPath", 
                   "--Report", "C:\\someDirectory\\sampleRepo\\coverageReport.json",
                   "--Target", "0"]

        with patch.object(sys, 'argv', testargs):
            argObj = getArguments.getArguments()
            isValid = (
                argObj.coverageReportPath == "C:\\someDirectory\\sampleRepo\\coverageReport.json" and
                argObj.coverageTarget == 0 and
                argObj.passOverride == None and
                argObj.failOverride == None)
            self.assertTrue(isValid, "Args were not parsed correctly.")
            return

class Test_jsonTextParser(unittest.TestCase):
    '''Unit tests for jsonTextParser.py'''

    def test_correctJSONText_returnNoChanges(self):
        validJSON = '{\n"Hello":"World"\n}'
        self.assertTrue(jsonTextParser.correctJSONText(validJSON) == validJSON, "JSON text was modified when it shouldn't have")

    def test_correctJSONText_returnProcessedJSON(self):
        validJSON = '{\n"Hello":"World"\n}'
        testJSON = '???123{\n"Hello":"World"\n}'
        self.assertTrue(jsonTextParser.correctJSONText(testJSON) == validJSON, "JSON was not modified to match corrected version")

class Test_jsonObjectParser(unittest.TestCase):
    '''Unit tests for jsonObjectParser.py'''

    def test_returnAllAssemblies_returnTwoAssemblies(self):
        testJSON = ''
        with open(r".\test_collaterals\exampleReport.json") as reader:
            testJSONText = reader.read()

        testJSONObject = json.loads(testJSONText)
        assemblies = jsonObjectParser.returnAllAssemblies(testJSONObject)

        isAmountValid = (len(assemblies) == 2)
        nameOne = assemblies[0]["Name"]
        nameTwo = assemblies[1]["Name"]
        isNamesValid = ((nameOne == 'BaseUtilities') and (nameTwo == 'CallbacksManager'))
        self.assertTrue(isAmountValid and isNamesValid, "Returned objects do not behave as expected")

    def test_returnAssembliesDict_returnTwoStats(self):
        testJSON = ''
        with open(r".\test_collaterals\exampleReport.json") as reader:
            testJSONText = reader.read()

        testJSONObject = json.loads(testJSONText)
        assemblies = jsonObjectParser.returnAllAssemblies(testJSONObject)
        assembliesDict = jsonObjectParser.returnCoverageDict(assemblies)
        
        isAmountValid = (len(assembliesDict) == 2)
        isCoverageValid = (assembliesDict["BaseUtilities"] == 96 and assembliesDict["CallbacksManager"] == 100)

        self.assertTrue(isCoverageValid, "Returned objects do not behave as expected.")
    
    def test_returnAssembliesDict_repeatAssemblyName(self):
        testJSON = ''
        with open(r".\test_collaterals\exampleReport_repeatName.json") as reader:
            testJSONText = reader.read()

        testJSONObject = json.loads(testJSONText)
        assemblies = jsonObjectParser.returnAllAssemblies(testJSONObject)

        try:
            assembliesDict = jsonObjectParser.returnCoverageDict(assemblies)
        except Exception as ex:
            self.assertTrue(
                type(ex) == jsonObjectParser.RepeatAssemblyException, 
                "Exception raised was not the one expected.")
            return

        self.assertTrue(False, "No exception was raised when one was expected.")

class Test_gater(unittest.TestCase):
    '''Unit tests for gater.py'''
    commonArguments = getArguments.Arguments(None, 90, None, None)
    passOverriderArguments = getArguments.Arguments(None, 90, ["Test2"], None)
    failOverriderArguments = getArguments.Arguments(None, 90, None, ["Test1"])
    zeroOverriderArguments = getArguments.Arguments(None, 0, None, None)

    def test_determineGatingAssemblies_printOneFailing(self):
        coverDict = {}
        coverDict['Test1'] = 100
        coverDict['Test2'] = 50
        gatingAssemblies = gater.determineGatingAssemblies(coverDict, self.commonArguments)

        self.assertTrue('Test2' in gatingAssemblies, "Method did not detect the gating assembly.")

    def test_determineGatingAssemblies_printNoneFailing(self):
        coverDict = {}
        coverDict['Test1'] = 100
        coverDict['Test2'] = 90
        gatingAssemblies = gater.determineGatingAssemblies(coverDict, self.commonArguments)

        self.assertTrue(len(gatingAssemblies) == 0, "Method detected gating assembly when there were none.")

    def test_determineGatingAssemblies_overrideAssemblyToFail(self):
        coverDict = {}
        coverDict['Test1'] = 100
        coverDict['Test2'] = 50
        gatingAssemblies = gater.determineGatingAssemblies(coverDict, self.failOverriderArguments)

        self.assertTrue('Test1' in gatingAssemblies, "Method did not detect the gating assembly.")

    def test_determineGaitingAssemblies_overrideAssemblyToPass(self):
        coverDict = {}
        coverDict['Test1'] = 100
        coverDict['Test2'] = 50
        gatingAssemblies = gater.determineGatingAssemblies(coverDict, self.passOverriderArguments)

        self.assertTrue(len(gatingAssemblies) == 0, "Method detected gating assembly when one should have been overriden.")
    
    def test_determineGatingAssemblies_coverageTargetZero(self):
         coverDict = {}
         coverDict['Test1'] = 100
         coverDict['Test2'] = 50
         gatingAssemblies = gater.determineGatingAssemblies(coverDict, self.zeroOverriderArguments)

         self.assertTrue(len(gatingAssemblies) == 0, "Method detected gating assemblies when there should have been none")

if __name__ == '__main__':
    unittest.main()