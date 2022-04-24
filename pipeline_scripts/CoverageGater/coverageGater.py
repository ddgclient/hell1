from coverage_tools import *
import json
import io
import sys

def parseAssemblyCoverage(jsonObject):
    assemblies = jsonObjectParser.returnAllAssemblies(jsonObject)
    assemblyCoverage = jsonObjectParser.returnCoverageDict(assemblies)
    return assemblyCoverage



#########################################
############ Start of main ##############
#########################################

if __name__ != "__main__":
    exit()

scriptArgs = getArguments.getArguments()
jsonObject = jsonTextParser.createJSONObject(scriptArgs.coverageReportPath)
assemblyCoverage = parseAssemblyCoverage(jsonObject)
gatingAssemblies = gater.determineGatingAssemblies(assemblyCoverage, scriptArgs)

if len(gatingAssemblies) == 0:
    print("All gating assemblies PASS")
    sys.exit(0) # passing exit code for C programs
else:
    print("{} gating assemblies FAILED - {}".format(len(gatingAssemblies), gatingAssemblies))
    sys.exit(1) # failing exit code for C programs