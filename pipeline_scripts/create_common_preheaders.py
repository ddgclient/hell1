import sys
import os
import glob
import errno
import xml.etree.ElementTree as ET
import re

class PreHeader:
    def __findImport(self, import_file):
        for directory in self.SearchPath:
            for file in glob.glob("{}/*.xml".format(directory)):
                if import_file == os.path.basename(file):
                    return file
        raise FileNotFoundError(errno.ENOENT, os.strerror(errno.ENOENT) + " SearchPath=[{}]".format(self.SearchPath), import_file)

    def __createDummyCommonParamsFile(self, common_params_file):
        root = ET.Element("TestLibraryInterfaces")
        root.set("xmlns:xsd", "http://www.w3.org/2001/XMLSchema")
        root.set("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance")
        root.set("xsi:schemaLocation", "http://vtsm.intel.com/2009/TestLibraryInterfaces file:///C:/Intel/hdmt/hdmtOS_3.9.0.1_Release/TOSRelease/bin/release/TestLibraryInterfaces.xsd")
        root.set("xmlns", "http://vtsm.intel.com/2009/TestLibraryInterfaces")
        root.tail = "\n"
        root.text = "\n  "

        testlibrary = ET.Element("TestLibrary")
        testlibrary.set("name", "PrimeTestInstance")
        testlibrary.tail = "\n"
        testlibrary.text = "\n    "

        testclass = ET.Element("TestClass")
        testclass.set("name", os.path.splitext(os.path.basename(common_params_file))[0])
        testclass.tail = "\n    "

        imports = ET.Element("Imports")
        imports.tail = "\n    "
        
        bases = ET.Element("PublicBases")
        bases.tail = "\n    "
        
        parameters = ET.Element("Parameters")
        parameters.tail = "\n    "
        
        exitports = ET.Element("ExitPorts")
        exitports.tail = "\n  "
        
        testlibrary.append(testclass)
        testlibrary.append(imports)
        testlibrary.append(bases)
        testlibrary.append(parameters)
        testlibrary.append(exitports)
        root.append(testlibrary)

        tree = ET.ElementTree(root)
        return tree
        
    def __findOrCreateDummyCommonParamsFile(self, import_file):
        try:
            importFullPath = self.__findImport(import_file)
            return importFullPath
        except FileNotFoundError:
            xmlTree = self.__createDummyCommonParamsFile(import_file)
            importFullPath = os.path.join(self.CommonParamOutputPath, import_file)
            with open(importFullPath, "wb") as fh:
                xmlTree.write(fh, encoding='utf-8', xml_declaration=True)               
            return importFullPath
                                          
    def __getCommonParamImports(self, preheader_file):
        tree = ET.parse(preheader_file)
        root = tree.getroot()
        imports = [x.text for x in root.findall("./{*}TestLibrary/{*}Imports/{*}FileName") if x.text.endswith("CommonParams.xml")]
        return imports
    
    def __getParamsRecursive(self, preheader_file):
        tree = ET.parse(preheader_file)
        root = tree.getroot()

        parameters = [x.attrib["name"] for x in root.findall("./{*}TestLibrary/{*}Parameters/{*}Parameter")]
        imports = [x.text for x in root.findall("./{*}TestLibrary/{*}Imports/{*}FileName")]
        for xml in imports:
            xml_full_path = self.__findImport(xml)
            parameters.extend(self.__getParamsRecursive(xml_full_path))

        return parameters

    def __init__(self, preheader_file, search_path, common_param_path):
        self.FileName = preheader_file
        self.SearchPath = search_path
        self.CommonParamOutputPath = common_param_path
        self.CommonParamsFile = None
        self.IncludedCommonParams = []
        
        tree = ET.parse(preheader_file)
        root = tree.getroot()

        testclass_elem = root.find("./{*}TestLibrary/{*}TestClass")
        if testclass_elem is None:
            print("\n***ERROR*** Parsing Preheaders\nFile=[{}] is not a valid preheader. Does not contain a TestLibrary/TestClass element.\n".format(preheader_file))
            sys.exit(1)
        testclass = testclass_elem.attrib["name"]
        parameters = [x.attrib["name"] for x in root.findall("./{*}TestLibrary/{*}Parameters/{*}Parameter")]
        imports = [x.text for x in root.findall("./{*}TestLibrary/{*}Imports/{*}FileName")]
        for importFile in imports:
            if not importFile.endswith("CommonParams.xml"):
                importFullPath = self.__findImport(importFile)
                parameters.extend(self.__getParamsRecursive(importFullPath))
            else:
                self.CommonParamsFile = self.__findOrCreateDummyCommonParamsFile(importFile)
                self.IncludedCommonParams = self.__getCommonParamImports(self.CommonParamsFile)
                
        bases = [x.text for x in root.findall("./{*}TestLibrary/{*}PublicBases/{*}BaseName")]
        self.TestClass = testclass
        self.Parameters = parameters
        #print("\tName={}".format(self.TestClass))
        #print("\tParameters={}".format(self.Parameters))


    def AddCommonParametersAndWriteFile(self, new_imports):
        if self.CommonParamsFile is None:
            print("Template=[{}] does not include a common parameter file, cannot add parameters".format(self.TestClass))
            return 0

        ET.register_namespace("", "http://vtsm.intel.com/2009/TestLibraryInterfaces")
        #print("\nUpdating {} with CommonParams={}".format(os.path.basename(self.CommonParamsFile), new_imports))
        tree = ET.parse(self.CommonParamsFile)
        root = tree.getroot()

        import_tag = root.find("./{*}TestLibrary/{*}Imports")
        existing_imports = [x.text for x in root.findall("./{*}TestLibrary/{*}Imports/{*}FileName")]
        #print("\tExisting={}".format(existing_imports))
        imports_to_add = [x for x in new_imports if x not in existing_imports]
        #print("\tToAdd={}".format(imports_to_add))
        if not imports_to_add:
            return 0
        
        if not existing_imports:
            import_tag.text = "\n      "
        else:
            root.findall("./{*}TestLibrary/{*}Imports/{*}FileName")[-1].tail += "  "
            
        for i,f in enumerate(imports_to_add):
            fileTag = ET.Element("FileName")
            fileTag.text = f
            fileTag.tail = "\n    " if i == len(imports_to_add) - 1 else "\n      "
            import_tag.append(fileTag)

        #print("\tWriting to {}".format(self.CommonParamOutputPath))
        print("Adding Params={} to [{}]".format(imports_to_add, os.path.basename(self.CommonParamsFile)))
        importFullPath = os.path.join(self.CommonParamOutputPath, os.path.basename(self.CommonParamsFile))

        # just wrting the xml to file messes up the namespace ordering...
        # so write to a string first, then fix the TestLibraryInterfaces line, then write out to a file
        xmlstr = ET.tostring(root, encoding='unicode', method='xml')
        search_string = "TestLibraryInterfaces [^\>]+"
        replace_string = "TestLibraryInterfaces xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://vtsm.intel.com/2009/TestLibraryInterfaces file:///C:/Intel/hdmt/hdmtOS_3.9.0.1_Release/TOSRelease/bin/release/TestLibraryInterfaces.xsd\" xmlns=\"http://vtsm.intel.com/2009/TestLibraryInterfaces\""
        xmlstr = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + re.sub(search_string, replace_string, xmlstr, count=1)
        with open(importFullPath, "w") as fh:
            fh.write(xmlstr)
            
        # this switches the order of the tags in the TestLibraryInterfaces line which causes issues for TPIE
        #with open(importFullPath, "wb") as fh:
        #    tree.write(fh, encoding='utf-8', xml_declaration=True)               
        return 1
    
    def IsMatch(self, match_rule):
        if "ExceptIf" in match_rule:
            if self.TestClass in match_rule["ExceptIf"] or next((x for x in self.Parameters if x in match_rule["ExceptIf"]), None):
                return False
            
        if "Name" in match_rule:
            for regex in match_rule["Name"]:
                if re.search(regex, self.TestClass):
                    return True
            return False

        if "HasParam" in match_rule:
            for param in match_rule["HasParam"]:
                if param in self.Parameters:
                    return True
            return False
        return False
            

def GetAllPreheaders(search_path, common_param_path):
    all_preheaders = {}
    for directory in search_path:
        for file in glob.glob("{}/*.xml".format(directory)):
            name = os.path.splitext(os.path.basename(file))[0]
            if name not in all_preheaders and not name.endswith("CommonParams") and not name.endswith("CommonParam") and name != "TestMethodBase":
                #print("\nReading File:[{}]".format(name))
                all_preheaders[name] = PreHeader(file, search_path, common_param_path)
    return all_preheaders

def GetPrimePath(props_file):
    tree = ET.parse(props_file)
    root = tree.getroot()
    prime_node = root.find("./{*}PropertyGroup/{*}PrimePath")
    if prime_node is None:
        raise ValueError("Unable to find [PropertyGroup/PrimePath] in .props file=[{}]".format(props_file))
    
    return prime_node.text

if __name__ == '__main__':
    print()
    print("PYTHON: {}".format(sys.version))
    #print("ARGS: {}".format(sys.argv))
    cwd = os.getcwd()
    #print("CWD: {}".format(cwd))
    scriptPath, scriptName = os.path.split(os.path.realpath(__file__) )
    print("SCRIPT NAME: {}".format(scriptName))
    #print("SCRIPT PATH: {}".format(scriptPath))
    print()

    #########################################################
    # Expect 1 argument to be the path to the current code repository.
    # If there's no args use one directory above the scripts path.
    if len(sys.argv) > 1:
        repo_path = sys.argv[1]
        print("Using CommandLineArgument=[{}] as the code path.".format(repo_path))
    else:
        repo_path = os.path.join(scriptPath, "..")
        print("Using the ScriptsLocation=[{}] as the code path.".format(repo_path))

    prime_path = GetPrimePath(os.path.join(repo_path, "src/Prime.Cs.Default.props"))
    print("Using the PrimePath=[{}] from [src/Prime.Cs.Default.props] as the Prime location.".format(prime_path))

    #########################################################
    # Constants
    SearchPath = [
        os.path.join(repo_path, "preheaders"),
        os.path.join(repo_path, "preheaders/CommonParams"),
        os.path.join(prime_path, "resources/preheaders"),
        os.path.join(prime_path, "resources/preheaders/TestMethodsDefaultCommonParams"),
        os.path.join(prime_path, "resources/preheaders/CommonParams"),
    ]
    CommonParamDestPath = os.path.join(repo_path, "preheaders/CommonParams")

    PreHeaderRules = [
        { "MatchIf": { "Name":[".*"] }                                       , "AddParams":["PreInstanceCommonParam.xml", "PostInstanceCommonParam.xml"] },
        { "MatchIf": { "HasParam":["Patlist"] }                              , "AddParams":["PatConfigSetPointsCommonParam.xml"] },
        { "MatchIf": { "HasParam":["Patlist"], "ExceptIf":["FivrCondition"] }, "AddParams":["FivrConditionCommonParam.xml"] },
        { "MatchIf": { "HasParam":["Patlist"], "ExceptIf":["DfxTimingTuner"]}, "AddParams":["SoftwareTriggerCommonParam.xml"] },
        { "MatchIf": { "HasParam":["LevelsTc"] }                             , "AddParams":["RelayTestConditionCommonParam.xml"] },
    ]

    #########################################################
    # Get all the preheaders
    allPreheaders = GetAllPreheaders(SearchPath, CommonParamDestPath)
    print("Found {} preheaders to check for common parameters.".format(len(allPreheaders)))
    
    # modify the preheaders
    total_updated = 0
    for preheader in allPreheaders.values():
        params_to_add = []
        for rule in PreHeaderRules:
            if preheader.IsMatch(rule["MatchIf"]):
                params_to_add.extend(rule["AddParams"])
        if params_to_add:
            total_updated += preheader.AddCommonParametersAndWriteFile(params_to_add)

    print("SCRIPT={} has completed. {} preheaders needed updates.\n\n".format(scriptName, total_updated))
    sys.exit(0)
    

