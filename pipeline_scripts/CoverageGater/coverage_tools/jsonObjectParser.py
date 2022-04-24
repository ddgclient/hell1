"""Parses the JSON object for info on code coverage."""

from coverage_tools import getArguments

repeatAssemblyExceptionMessage = """Found more than one assembly with 
the same name in the coverage report."""

InvalidConfigurationExceptionMessage = """Required argument not present in
json configuration file."""

class RepeatAssemblyException(Exception):
    pass

class InvalidConfigurationException(Exception):
    pass

def returnAllAssemblies(jsonObject: dict) -> dict:
    '''Returns all objects representing assemblies from the JSON.'''

    return jsonObject["Children"]

def returnCoverageDict(assemblies: dict) -> dict:
    '''Return a simple dict only containing the names of assemblies and
   their coverage.'''

    assembliesDict = {}

    for assembly in assemblies:
        if assembly["Name"] in assembliesDict.keys():
            raise RepeatAssemblyException(repeatAssemblyExceptionMessage)
        
        assembliesDict[assembly["Name"]] = assembly["CoveragePercent"]
    
    return assembliesDict