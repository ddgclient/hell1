import gitlab
import re
import argparse
import sys
import os
import subprocess
from distutils.dir_util import copy_tree
import shutil
import stat
from collections import namedtuple
import time
from git import Repo
import json

# shutil.rmtree can take an error-handling function that will be called when it has problem removing a file. 
# can use this function to force the removal of the problematic file(s).
def remove_readonly(func, path, excinfo):
    os.chmod(path, stat.S_IWRITE)
    func(path)

def getArgs():
    parser = argparse.ArgumentParser(description='This script is used during release process to create wiki pages.',
        formatter_class=argparse.ArgumentDefaultsHelpFormatter)
    parser.add_argument( "-authkey", type=str, default='', help="Personal access token for authentication with GitLab.")
    parser.add_argument( "-project", type=str, default="97806", help="Path to GitLab project in the form <namespace>/<project>")
    parser.add_argument( "-url", default='https://gitlab.devtools.intel.com', help="Gitlab URL.")
    parser.add_argument( "-commit_ref", type=str, help="Path to GitLab project in the form <namespace>/<project>")
    parser.add_argument( "-wiki_url", type=str, default="https://gitlab.devtools.intel.com/ddg-client-prime-code-base-tgl/tgl_poc_code.wiki.git", help="WIKI repo path")
    return parser, parser.parse_args()

class GitlabRunner():
    def __init__(self, url, wikiUrl, authkey, project, commit_ref):
        # Parse command line arguments
        self.url = url
        self.wikiUrl = wikiUrl
        #self.authkey = authkey
        self.authkey = os.environ.get("authkey")
        self.project_id = project
        #self.commit_ref = commit_ref #release name
        self.commit_ref = os.environ.get("CI_COMMIT_REF_NAME")
        #self.commit_ref = commit_ref
        self.mrs = None  # list of all merge requests
        # Create python-gitlab server instance
        print("Inside Runner", flush=True)
        try:
            server = gitlab.Gitlab(self.url, self.authkey, api_version=4, ssl_verify=True)
            print("Initialized gitlab object.")
            # Get an instance of the project and store it off
            self.project = server.projects.get(self.project_id)
            print(self.project, flush=True)
        except Exception as e:
            print("%s"%e, flush=True)
            
        print("After Runner initialization", flush=True)
     
    def getIDSID(self):
        print("Running getIDSID()", flush=True)
        users = self.project.users.list(all=True)
        print(users)
        try:
            userNames = [x.username for x in users]
            print(",".join(userNames))
            return ",".join(userNames)
        except Exception as e:
            print("%s"%e, flush=True)

    def uploadDocsToWiki(self):
        try:
            wikiDir = os.path.join(os.getcwd(), os.path.basename(self.wikiUrl))
            print("WikiDir is %s"%wikiDir, flush=True)
            
            # clone the WIKI git repo
            repo = Repo.clone_from(self.wikiUrl, wikiDir)

            # cd into the WIKI repo
            os.chdir(wikiDir)
            # create a new directory based on release name, copy the releaseName/documentation content into new directory and push the dir to WIKI master repo
            newWikiDir = os.path.join(os.getcwd(), self.commit_ref)
            os.makedirs(newWikiDir)
            print("NewWikiDir is %s"%newWikiDir, flush=True)
            copy_tree(os.path.join("./../", "documentation"), newWikiDir)
            
            repo.index.add(newWikiDir)
            repo.index.commit("Adding release %s to WIKI"%self.commit_ref)
            repo.remotes.origin.push()
            os.chdir("../")
            
        except Exception as e:
            print("ERROR: %s"%e, flush=True)
            raise("Couldn't complete uploadDocsToWiki() call")
            
    def addIssues(self, content, issues):
        print("Running addIssues()", flush=True)
        #print(self.project.attributes) # displays all the attributes
        #git_url = self.project.http_url_to_repo
    
        content += "\n# Resolved Issues\n"
        content += "| Title | Type | Link | Owner |\n"
        content += "| --- | --- | --- | --- |\n"
        for issue in issues:
            #print(issue.attributes)
            # create link to each issue
            if not issue.assignee:
                name = ""
            else:
                name = issue.assignee['name']
                
            if not issue.labels:
                label = ""
            else:
                label = issue.labels[0]
                
            content += "| " + "%s"%issue.title + "| " + label + " | " + "%s"%(self.project.web_url + "/issues/" + '%s'%issue.iid) + " |" + name + " |\n"
            #content +=  "\n" + ''.join(map(str, issue.title)) + " " + '%s'%issue.labels + " : " + webUrl + "/issues/" + '%s'%issue.iid + "\n"   
            
            #content += "[%s] "%issue.title + "(%s)"%(self.project.web_url + "/issues/" + '%s'%issue.iid)
        return content
   
    def addDocs(self, content):
        print("Running addDocs()", flush=True)
        files = os.listdir(os.path.join("./", "documentation"))
        content += "\n# Documentations\n"
        for file in files:
            if file.endswith(".html"):
                fileName = os.path.splitext(os.path.basename(file))[0]
                content += "[%s]"%fileName + "(%s)"%(self.commit_ref + "/" + fileName) + "\n\n"
        return content
   
    def addWikiPage(self, pageSlug, content):
        print("Running addWikiPage()", flush=True)
        page = self.project.wikis.create({'title': pageSlug, 'content': content})
        page.save()

    def deleteWikiPage(self):
        pass
        #page = self.project.wikis.get('prime_v4.01.00 evg5040302 tos36110 ddg011')
        #page.delete()

    # get issues from a milestone
    def getIssues(self, msTitle):    
        print("Running getIssues()", flush=True)
        milestones = self.project.milestones.list()
        id = -1
        for milestone in milestones:
            if milestone.title == msTitle:
                id = milestone.id
                break
        try:
            milestone = self.project.milestones.get(id)
            issues = milestone.issues()
        except:
            print("ERROR: milestone id: %s isn't valid"%id, flush=True)
            issues = []
 
        return issues

    def createRelease(self):
        pass

    def getReleaseNote(self):
        print("Running getReleaseNote()", flush=True)
        try:
            ref = self.project.tags.get(self.commit_ref)
            #releaseNote = ref.release["description"]
            return ref.release["description"]
        except Exception as e:
            print("ERROR: %s"%e, flush=True)
            print("Couldn't complete getReleaseNote() call, releaseNote is empty", flush=True)
            return ""

    def createReleaseConfig(self, note):
        print("Running createReleaseConfig()", flush=True)
        tos = self.parseReleaseForTOS(note)
        evg = self.parseReleaseForEVG(note)
        prime = self.parseReleaseForPrime(note)
        #IDSIDs = "jhanbaba"
        #self.getIDSID()
        IDSIDs = self.getIDSID() + ",dtduong,jacksonl,srdugre,schoi,mmcruz,cceisenm,jthwing,lguo6,jurtecho,akshigoe,lrpaloma,mruanto,nyiin,tsinha,dmarmstr,hmmanuel,apereir,hdnguye7,jqdelosr,lsurabat,mmaroon,jsgarcia,tvongnal,gfhay,pamfulto,brownmat,shirleyt,yingjin,vrchandr,mtignaci,tbgriffi,sruiwale,swjohnso,cdjones,sravindr,mwarren,srameshc,abalian"

        #with open("mail.txt", 'r') as file:
        #    data = file.read()
        
        dictionary = {
            "TOS" : tos,
            "EVG" : evg,
            "Prime" : prime,
            "LibPath" : "\\\\amr.corp.intel.com\\ec\\proj\\mdl\\jf\\intel\\tpapps\\userlibs\\mtl\\{0}".format(self.commit_ref),
            "ReleaseDoc": "\\\\amr.corp.intel.com\\ec\\proj\\mdl\\jf\\intel\\tpapps\\userlibs\\mtl\\{0}\\documentation\\Documentation.html".format(self.commit_ref),
            "ReleaseWIKI" : "{0}/wikis/{1}".format(self.project.web_url, self.commit_ref),
            "ProjectWIKI" : "{0}/wikis/home".format(self.project.web_url),
            "CodeCoverage" : "\\\\amr.corp.intel.com\\ec\\proj\\mdl\\jf\\intel\\tpapps\\userlibs\\mtl\\{0}\\unittestCoverage\\coverLogComplete.html".format(self.commit_ref),
            "Subject" : self.commit_ref + " Release Notification",
            "IDSIDs" : IDSIDs,
            "Message" : "<h5> Greetings! <br> DDG Client MTL Prime WG is proud to announce the release of Prime user code runtime library: <span style=\"color:red\">%s</span></h5>"%self.commit_ref,
            "DDDGPrimePortal": "http://ddgclient.intel.com/",
            "TMMPrimePortal": "https://dev.azure.com/mit-us/PRIME/_wiki/wikis/PRIME.wiki/150/Release-Notes"
            #"Data" : data
        }
        json_object = json.dumps(dictionary, indent = 4) 
        
        with open((os.path.join(os.getcwd(), "releaseConfig.json")), "w") as outfile:  
            outfile.write(json_object)

    def parseReleaseForTOS(self, note):   
        print("Running parseReleaseForTOS()", flush=True)    
        match = re.search(r'^.*TOS.*?:\s*([\w\d_\-\.]+)\s*.*$', note, re.DOTALL)
        if match:
            #print(match.group(1))
            return match.group(1)
        else:
            return ""

    def parseReleaseForEVG(self, note):     
        print("Running parseReleaseForEVG()", flush=True)        
        match = re.search(r'^.*EVG.*?:\s*([\w\d_\-\.]+)\s*.*$', note, re.DOTALL)
        if match:
            #print(match.group(1))
            return match.group(1)
        else:
            return ""

    def parseReleaseForPrime(self, note):    
        print("Running parseReleaseForPrime()", flush=True)     
        match = re.search(r'^.*Prime.*?:\s*([\w\d_\-\.]+)\s*.*$', note, re.DOTALL)
        if match:
            #print(match.group(1))
            return match.group(1)
        else:
            return ""

    def parseReleaseForMilestone(self, note):        
        print("Running parseReleaseForMilestone()", flush=True)
        match = re.search(r'^.*Milestone.*?:\s*([\w\d_\-\.]+)\s*.*$', note, re.DOTALL)
        if match:
            #print(match.group(1))
            return match.group(1)
        else:
            return ""
        
    def addReleaseToHome(self):
        print("Running addReleaseToHome()", flush=True)
        page = self.project.wikis.get("home")
        match = re.search(r'(.*)(# Releases)(.*)$', page.content, re.DOTALL)
        if match:
            pre = match.group(1)
            post = match.group(3)
            mid = "# Releases\n"
            newLine = "[{0}]({1})\n".format(self.commit_ref,self.commit_ref)
            page.content = pre + mid + newLine + post
            page.save()

    def run(self):
        self.uploadDocsToWiki()
        self.addReleaseToHome()
        releaseNote = self.getReleaseNote()
        self.createReleaseConfig(releaseNote)
        ms = self.parseReleaseForMilestone(releaseNote)
        issues = self.getIssues(ms)
        content = self.addIssues(releaseNote, issues)
        content = self.addDocs(content)
        self.addWikiPage(self.commit_ref, content)
        #self.deleteWikiPage()

if __name__ == '__main__':
    original_stdout = sys.stdout
    with open('log.txt', 'w') as f:
        sys.stdout = f # Change the standard output to the file we created.
        myParser, myargs = getArgs()
        #print("hello after pararugement", flush=True)
        try:
            GitlabRunner(url=myargs.url, wikiUrl=myargs.wiki_url, authkey=myargs.authkey, project=myargs.project, commit_ref=myargs.commit_ref).run()
        except Exception as ex:
            print("ERROR running the code", flush=True)
            raise ex
        finally:
            sys.stdout = original_stdout # Reset the standard output to its original value