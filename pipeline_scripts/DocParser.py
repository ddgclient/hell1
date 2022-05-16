import os
import re
import shutil
import sys
from distutils.dir_util import copy_tree
import traceback

def createCentralHtml():
    print("Starting createCentralHtml()", flush=True)
    header = """
<!DOCTYPE html>
<html>
<head>
<meta name="viewport" content="width=device-width, initial-scale=1">
<style>
table {
	font-family: arial, sans-serif;
	border-collapse: collapse;
	width: 50%;
}

table.center {
    margin-left: auto;
    margin-right: auto;
}

td, th {
  border: 1px solid #dddddd;
  text-align: left;
  padding: 8px;
}

tr:nth-child(even) {
  background-color: #dddddd;
}
</style>
</head>
<table class =\"center\">
<body>"""

    commitRefName = os.environ.get("GITHUB_REF_NAME")
    #commitRefName = "fakeRef"
    #ciProjectUrl = os.environ.get("CI_PROJECT_URL")
    ciProjectUrl = os.environ.get("GITHUB_REPOSITORY")
    #ciProjectUrl = "fakeUrl"

    wikiPrefix = ciProjectUrl + "/-/wikis/" + commitRefName + "/"
    links = []
    print("Creating central HTML for TestMethod documentations...")
    files = os.listdir(os.path.join("./", "documentation"))

    for file in files:
        if re.match("([A-Z a-z])\w+([.])(html)", file): # not needed anymore since we added regex during copy over, but it works so whatever
            fileName = os.path.splitext(os.path.basename(file))[0]
            linkToAdd = "<tr><td style=\"text-align:center\"><a href=\"" + file +"\">" + fileName + "</a></td></tr>"
            print("Adding link: " + linkToAdd)
            links.append(linkToAdd)

    with open("./documentation/Documentation.html", "w+") as f:
        f.writelines(header)
        f.writelines("<h2 style=\"text-align: center;\">" + commitRefName + "</h2>\n")
        f.writelines(links)
        f.writelines("\n</table></body>\n</html>")

    print("Done with createCentralHtml()\n", flush=True)

def copyOverCoverage():
    print("Starting with copyOverCoverage()\n", flush=True)
    try:
        if os.path.exists(r'.\logs\dotCover\coverLogComplete.html'):
            shutil.copy(r'.\logs\dotCover\coverLogComplete.html', r".\documentation")
            copy_tree(r'.\logs\dotCover\coverLogComplete', r".\documentation")
    except Exception as ex:
        print("Unexpected error raised during copy of coverlogs")
        print(type(ex))
        
    print("Done with copyOverCoverage()\n", flush=True)

def renameFile(filepath):
    print("Starting renameFile()", flush=True)

    print(filepath)
    fileName = os.path.basename(filepath)    
    fileNameNoExtension = os.path.splitext(os.path.basename(filepath))[0]
    fileExtension = os.path.splitext(os.path.basename(filepath))[1]
    dirName = os.path.dirname(os.path.realpath(filepath))
    print("fileName is {0}".format(fileName))
    print("fileNameNoExtension is {0}".format(fileNameNoExtension))
    print("fileExtension is {0}".format(fileExtension))
    print("dirName is {0}".format(dirName))
    newFileName = os.path.join(dirName, fileNameNoExtension + "_" + os.environ.get("GITHUB_REF_NAME") + fileExtension)
    print("newFileName is {0}".format(newFileName))
    oldFileName = os.path.join(dirName, fileName)
    print("oldFileName is {0}".format(oldFileName))
    try:
        os.rename(oldFileName, newFileName)
    except Exception as e:
        print(traceback.format_exc())
        
    print("Done with renameFile()\n", flush=True)
    return newFileName

def removeImageFolder(filepath):
    print("Starting removeImageFolder()", flush=True)
    tempFile = os.path.join(os.getcwd(),"temp.txt")
    with open(tempFile, 'w', , encoding="cp437", errors='ignore') as fileOut:
        with open(filepath, "r", encoding="cp437", errors='ignore') as fileIn:
            for line in fileIn:
                rslt = re.search(r'(.*?<img\s*src=\")images\/(.*)', line)
                if rslt:
                    line = re.sub(r'(.*?<img\s*src=\")images\/(.*)', r'\1\2', line)
                    print(line, flush=True)
                fileOut.write(line)   
    
    os.remove(filepath)
    os.rename(tempFile, filepath)
    print("Done with removeImageFolder()\n", flush=True)
            
def copyTheFiles():
    print("Starting copyTheFiles()", flush=True)
    path = os.path.join(os.getcwd(), 'src')
    for subdir, dirs, files in os.walk(r'.'):
        for filename in files:
            filepath = subdir + os.sep + filename
            
            #if filepath.endswith(".html"): #this line covers copies over too many unnecessary .html files
            if re.match("([A-Z])\w+([.])(html)", filename): #this only copies over .html files that begin with a capital letter, so templates must begin with a capital letter
                if not os.path.exists(os.path.join(r".\documentation", filename)): 
                    #print (filepath)                
                    shutil.copy(filepath, r".\documentation")
                    
                    # rename after copying
                    newFilePath = os.path.join(r".\documentation", os.path.basename(filepath))
                    newFilePath = renameFile(newFilePath)
                    # remove the reference /image in the file as GitHub wiki has no folder structure, all files store at the same wiki level
                    removeImageFolder(newFilePath)

            if filepath.endswith(".md"):
                if not os.path.exists(os.path.join(r".\documentation", filename)): 
                    shutil.copy(filepath, r".\documentation")
                    
                    # rename after copying
                    newFilePath = os.path.join(r".\documentation", os.path.basename(filepath))
                    newFilePath = renameFile(newFilePath)
                    # remove the reference /image in the file as GitHub wiki has no folder structure, all files store at the same wiki level
                    removeImageFolder(newFilePath)

        if os.path.isdir(subdir + os.sep + "images"):
            copy_tree(subdir + os.sep + "images", r".\documentation\images")
            for filename in os.listdir(os.path.join(r".\documentation", "images")):
               
                # rename after copying
                newFilePath = os.path.join(r".\documentation\images", filename)
                #print(os.path.join(r".\documentation\images", filename))
                renameFile(os.path.join(r".\documentation\images", filename))
    print("Done with copyTheFiles()\n", flush=True)

if __name__ == '__main__':
    original_stdout = sys.stdout
    
    with open('logDocarser.txt', 'w') as f:
        sys.stdout = f # Change the standard output   to the file we created.
        try:
            if not os.path.exists(r'.\documentation'):
                os.makedirs(r'.\documentation')
                os.makedirs(r'.\documentation\images')
            
            copyTheFiles()
            createCentralHtml()
            
        except Exception:
            print("ERROR running DocParser.py", flush=True)
            print(traceback.format_exc())
        finally:
            sys.stdout = original_stdout # Reset the standard output to its original value
