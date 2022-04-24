import os
import re
import shutil
from distutils.dir_util import copy_tree

def createCentralHtml():
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

    commitRefName = os.environ.get("CI_COMMIT_REF_NAME")
    #commitRefName = "fakeRef"
    ciProjectUrl = os.environ.get("CI_PROJECT_URL")
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

def copyOverCoverage():
    try:
        if os.path.exists(r'.\logs\dotCover\coverLogComplete.html'):
            shutil.copy(r'.\logs\dotCover\coverLogComplete.html', r".\documentation")
            copy_tree(r'.\logs\dotCover\coverLogComplete', r".\documentation")
    except Exception as ex:
        print("Unexpected error raised during copy of coverlogs")
        print(type(ex))

###############################
###### START OF MAIN ##########
###############################
if not os.path.exists(r'.\documentation'):
    os.makedirs(r'.\documentation')
    os.makedirs(r'.\documentation\images')

for subdir, dirs, files in os.walk(r'.'):
    for filename in files:
        filepath = subdir + os.sep + filename

        #if filepath.endswith(".html"): #this line covers copies over too many unnecessary .html files
        if re.match("([A-Z])\w+([.])(html)", filename): #this only copies over .html files that begin with a capital letter, so templates must begin with a capital letter
            if not os.path.exists(os.path.join(r".\documentation", filename)): 
                shutil.copy(filepath, r".\documentation")
                print (filepath)

        if filepath.endswith(".md"):
            if not os.path.exists(os.path.join(r".\documentation", filename)): 
                shutil.copy(filepath, r".\documentation")
                print (filepath)
            
    if os.path.isdir(subdir + os.sep + "images"):
        copy_tree(subdir + os.sep + "images", r".\documentation\images")

createCentralHtml()