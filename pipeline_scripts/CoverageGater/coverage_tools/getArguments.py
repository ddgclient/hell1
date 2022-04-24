"""Responsible for reading arguments and detecting if they're valid"""

import sys
import getopt
from coverage_tools import jsonTextParser

argumentNumberExceptionMessage = "No arguments given. Make sure they are formatted correctly or that they exist."
argumentEmptyExceptionMessage = "One or both arguments found to be empty when executing."

class ArgumentsEmptyException(Exception):
    pass

class Arguments:
    def __init__(self, coverageReportPath, coverageTarget, passOverride, failOverride):
        self.coverageReportPath = coverageReportPath
        self.coverageTarget = coverageTarget
        self.passOverride = passOverride
        self.failOverride = failOverride

def getArguments() -> Arguments:
    """Same functionality as base version, but allows for the use of
    a JSON configuration file for passing in all arguments."""

    coverageJSONPath = ''
    coverageTarget = -1
    passOverride = None
    failOverride = None

    argumentList = sys.argv[1:]

    if len(argumentList) < 1:
        raise ArgumentsEmptyException(argumentNumberExceptionMessage, sys.argv)

    options = "r:t:c:h"
    long_options = ["Report =", "Target =", "Config =", "Help"]
    arguments, values = getopt.getopt(argumentList, options, long_options)

    for arg, currentVal in arguments:
        currentArg = arg.strip()
        if currentArg == "-h" or currentArg == "--Help":
            print("Displaying help...")
        elif currentArg == "-r" or currentArg == "--Report":
            coverageJSONPath = currentVal
        elif currentArg == "-t" or currentArg == "--Target":
            coverageTarget = float(currentVal)
        elif currentArg == "-c" or  currentArg == "--Config":
            configObj = parseConfigFile(currentVal)
            if "CoverageReport" in configObj.keys():
                coverageJSONPath = configObj["CoverageReport"]
            if "CoverageTarget" in configObj.keys():
                coverageTarget = configObj["CoverageTarget"]
            if "PassOverride" in configObj.keys():
                passOverride = configObj["PassOverride"]
            if "FailOverride" in configObj.keys():
                failOverride = configObj["FailOverride"]

    argumentsNotEmpty = bool(coverageJSONPath) and coverageTarget is not None
    argumentReporter = ''

    if(not coverageJSONPath):
        argumentReporter += "No path was given to JSON coverage report.\n"
    if(not coverageTarget):
        argumentReporter += "No value was given for coverage target (or the given value was zero).\n"
    if(not argumentsNotEmpty):
        raise ArgumentsEmptyException(argumentEmptyExceptionMessage, sys.argv)

    return Arguments(coverageJSONPath, coverageTarget, passOverride, failOverride)

def parseConfigFile(configPath) -> dict:
    return jsonTextParser.createJSONObject(configPath)