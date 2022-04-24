"""Takes in objects representing all assemblies, determines which ones to gate"""
from coverage_tools import getArguments

def determineGatingAssemblies(assembliesDict: dict, scriptArgs: getArguments.Arguments):
    '''Determine if any assemblies do not meet the coverage requirement'''

    print("Checking if assemblies meet requirement...\n")
    gatingAssemblies = []
    overriddenToPass = []
    overriddenToFail = []

    if scriptArgs.passOverride is not None:
        print("Pass override assemblies detected, following assemblies will always pass...")
        print(str(scriptArgs.passOverride))
    else:
        print("No pass overrides detected...")
        scriptArgs.passOverride = []

    if scriptArgs.failOverride is not None:
        print("Fail override assemblies detected, following assemblies will always fail...")
        print(str(scriptArgs.failOverride))
    else:
        print("No fail overrides detected...")
        scriptArgs.failOverride = []

    print()

    for name, coverage in assembliesDict.items():
        print("Assembly=[{}] Coverage=[{}] Target=[{}]".format(name, coverage, scriptArgs.coverageTarget))
        if coverage < scriptArgs.coverageTarget:
            if name in scriptArgs.passOverride:
                overriddenToPass.append(name)
            else:
                gatingAssemblies.append(name)
        else:
            if name in scriptArgs.failOverride:
                overriddenToFail.append(name)

    for assembly in gatingAssemblies:
        print('Assembly [{}] coverage value [{}] does not meet target.'.format(assembly, str(scriptArgs.coverageTarget)))
    print()

    for assembly in overriddenToFail:
        print('Assembly [{}] met coverage target [{}], but was overridden to fail.'.format(assembly, str(scriptArgs.coverageTarget)))
    print()

    for assembly in overriddenToPass:
        print('Assembly [{}] did not meet the coverage target [{}], but was overridden to pass.'.format(assembly, str(scriptArgs.coverageTarget)))
    print()

    return gatingAssemblies + overriddenToFail