import gitlab
from github import Github
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
import github3

# shutil.rmtree can take an error-handling function that will be called when it has problem removing a file. 
# can use this function to force the removal of the problematic file(s).
def remove_readonly(func, path, excinfo):
    os.chmod(path, stat.S_IWRITE)
    func(path)

def getArgs():
    parser = argparse.ArgumentParser(description='This script is used during release process to create wiki pages.',
        formatter_class=argparse.ArgumentDefaultsHelpFormatter)
    parser.add_argument( "-authkey", type=str, default='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', help="Personal access token for authentication with GitLab.")
    parser.add_argument( "-repoPath", type=str, default="ddgclient/hell1", help="Path to GitHub repo in the form <ddgclient>/<repo>")
    #parser.add_argument( "-url", default='https://gitlab.devtools.intel.com', help="Gitlab URL.")
    parser.add_argument( "-commit_ref", type=str, default = "FirstRelease",help="Path to GitLab repo in the form <namespace>/<repo>")
    parser.add_argument( "-wiki_url", type=str, default="https://github.com/ddgclient/hell1.wiki.git", help="WIKI repo path")
    return parser, parser.parse_args()

class GitlabRunner():
    def __init__(self, wikiUrl, authkey, repoPath, commit_ref):
        # Parse command line arguments
        #self.url = url
        self.wikiUrl = wikiUrl
        self.authkey = authkey
        #self.authkey = os.environ.get("authkey")
        repo = repoPath.split("/")
        self.repoOwner = repo[0]
        self.repoName = repo[1]
        self.commit_ref = commit_ref #release name
        #self.commit_ref = os.environ.get("GITHUB_BASE_REF")
        self.mrs = None  # list of all merge requests
        # Create python-gitlab server instance
        print("Inside Runner", flush=True)
        try:
            server = github3.login(token=self.authkey)
            print("Initialized github object.")
            # Get an instance of the repo and store it off
            self.repo = server.repository(owner=self.repoOwner, repository=self.repoName)
            print(self.repo, flush=True)
        except Exception as e:
            print("%s"%e, flush=True)
            
        print("After Runner initialization", flush=True)
     
    def getIDSID(self):
        print("Running getIDSID()", flush=True)
        try:
            users = self.repo.collaborators()
            userNames = [str(x) for x in list(users)]
            print(",".join(userNames))
            return ",".join(userNames)
        except Exception as e:
            print("%s"%e, flush=True)

    def downloadWiki(self):
        try:
            wikiDir = os.path.join(os.getcwd(), os.path.basename(self.wikiUrl))
            print("WikiDir is %s"%wikiDir, flush=True)
            if os.path.isdir(wikiDir):
                os.chmod(wikiDir, 777)
                os.remove(wikiDir)
            
            # clone the WIKI git repo
            self.repoGit = Repo.clone_from(self.wikiUrl, wikiDir)
  
            # cd into the WIKI repo
            os.chdir(wikiDir)
            
            # create a new directory based on release name, copy the releaseName/documentation content into new directory and push the dir to WIKI master repo
            newWikiDir = os.path.join(os.getcwd(), self.commit_ref)
            #os.makedirs(newWikiDir)
            print("NewWikiDir is %s"%newWikiDir, flush=True)
            copy_tree(os.path.join("./../", "documentation"), newWikiDir)
            
            self.repoGit.index.add(newWikiDir)            
            
            self.repoGit.index.commit("Adding wiki folder %s to WIKI"%self.commit_ref)
            self.repoGit.remotes.origin.push()
            os.chdir("../")
            
        except Exception as e:
            print("ERROR: %s"%e, flush=True)
            raise("Couldn't complete downloadWiki() call")
        
    def uploadWiki(self, content):
        print("Running uploadWiki()", flush=True)
        
        wikiDir = os.path.join(os.getcwd(), os.path.basename(self.wikiUrl))
        print("WikiDir is %s"%wikiDir, flush=True)
        os.chdir(wikiDir)
        
        print(os.getcwd())
    
        #open home in write mode
        wikihome = open(wikiDir + "\\" + self.commit_ref + ".md", "w")
        #read whole file to a string
        wikihome.write(content)
        #close file
        wikihome.close()
      
        self.repoGit.index.add(wikiDir + "\\" + self.commit_ref + ".md")            
        self.repoGit.index.commit("Adding release wiki %s to WIKI"%self.commit_ref)
        self.repoGit.remotes.origin.push()
        os.chdir("../")    
        
    def addIssues(self, content, issues):
        print("Running addIssues()", flush=True)
        #print(self.repo.attributes) # displays all the attributes
        #git_url = self.repo.http_url_to_repo
    
        content += "\n# Resolved Issues\n"
        content += "| Title | Type | Link | Owner |\n"
        content += "| --- | --- | --- | --- |\n"
        for issue in issues:
            #print(issue.attributes)
            # create link to each issue
            if not issue.assignee:
                name = ""
            else:
                name = issue.assignee.login
              
            #print(name)
            
            # turn iterator to a list first, then use lambda function to get individual label name
            labels = ", ".join(map(lambda x: x.name, list(issue.labels())))
            #for label in issue.labels():
            #    labels += label.name
            content += "| " + "%s"%issue.title + "| " + labels + " | " + issue.html_url + " |" + name + " |\n"
            #print(content)
        return content
   
    def addDocs(self, content):
        print("Running addDocs()", flush=True)
        files = os.listdir(os.path.join("./", "documentation"))
        content += "\n# Documentations\n"
        for file in files:
            if file.endswith(".html"):
                fileName = os.path.splitext(os.path.basename(file))[0]
                #content += "[%s]"%fileName + "(%s)"%(self.commit_ref + "/" + fileName) + "\n\n"
                content += "[%s]"%fileName + "(%s)"%(fileName) + "\n\n"
        print(content)
        return content
   
    def addWikiPage(self, pageSlug, content):
        print("Running addWikiPage()", flush=True)
        page = self.repo.wikis.create({'title': pageSlug, 'content': content})
        page.save()

    def deleteWikiPage(self):
        pass
        #page = self.repo.wikis.get('prime_v4.01.00 evg5040302 tos36110 ddg011')
        #page.delete()

    # get issues from a milestone
    def getIssues(self, msTitle):    
        print("Running getIssues()", flush=True)
        title = ""
        for milestone in self.repo.milestones():
            if milestone.title == msTitle:
                title = milestone.title
                break
        try:
            #for issue in self.repo.issues(milestone=title):
            #    print(issue.assignee)
            issues = self.repo.issues(milestone=title)
        except:
            print("ERROR: milestone title: %s isn't valid"%title, flush=True)
            issues = []
 
        return issues

    def createRelease(self):
        pass

    def getReleaseNote(self):
        print("Running getReleaseNote()", flush=True)     
        try:
            #login = github3.login(token='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')
            #self.repo = login.repository(owner='ddgclient', repository='hell1')
            print(self.repo.release_from_tag("V1").name)
            print(self.repo.release_from_tag("V1").body)
       
            #releaseNote = self.repo.release_from_tag("V1").body
            return self.repo.release_from_tag("V1").body
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
        #IDSIDs = self.getIDSID() + ",dtduong,jacksonl,srdugre,schoi,mmcruz,cceisenm,jthwing,lguo6,jurtecho,akshigoe,lrpaloma,mruanto,nyiin,tsinha,dmarmstr,hmmanuel,apereir,hdnguye7,jqdelosr,lsurabat,mmaroon,jsgarcia,tvongnal,gfhay,pamfulto,brownmat,shirleyt,yingjin,vrchandr,mtignaci,tbgriffi,sruiwale,swjohnso,cdjones,sravindr,mwarren,srameshc,abalian"
        IDSIDs = self.getIDSID()
        print(IDSIDs)
        
        #with open("mail.txt", 'r') as file:
        #    data = file.read()
        
        dictionary = {
            "TOS" : tos,
            "EVG" : evg,
            "Prime" : prime,
            "LibPath" : "\\\\amr.corp.intel.com\\ec\\proj\\mdl\\jf\\intel\\tpapps\\userlibs\\mtl\\{0}".format(self.commit_ref),
            "ReleaseDoc": "\\\\amr.corp.intel.com\\ec\\proj\\mdl\\jf\\intel\\tpapps\\userlibs\\mtl\\{0}\\documentation\\Documentation.html".format(self.commit_ref),
            "ReleaseWIKI" : "{0}/wiki/{1}".format(self.repo.html_url, self.commit_ref),
            "ProjectWIKI" : "{0}/wiki/home".format(self.repo.html_url),
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
            print(match.group(1))
            return match.group(1)
        else:
            return ""
        
    def addReleaseToHome(self):
        print("Running addReleaseToHome()", flush=True)
        
        wikiDir = os.path.join(os.getcwd(), os.path.basename(self.wikiUrl))
        print("WikiDir is %s"%wikiDir, flush=True)
        os.chdir(wikiDir)
        
        print(os.getcwd())
        #open home in read mode
        wikihome = open(wikiDir+"\Home.md", "r")
        #read whole file to a string
        page = wikihome.read()
        #close file
        wikihome.close()
    
        match = re.search(r'(.*)(# Releases)(.*)$', page, re.DOTALL)
        if match:
            pre = match.group(1)
            post = match.group(3)
            mid = "# Releases\n"
            newLine = "[{0}]({1})\n".format(self.commit_ref,self.commit_ref)
            page = pre + mid + newLine + post
    
        #open home in write mode
        wikihome = open(wikiDir+"\Home.md", "w")
        #read whole file to a string
        wikihome.write(page)
        #close file
        wikihome.close()
      
        self.repoGit.index.add(wikiDir+"\Home.md")            
        self.repoGit.index.commit("Adding release %s to WIKI"%self.commit_ref)
        self.repoGit.remotes.origin.push()
        os.chdir("../")     

    def run(self):
        self.downloadWiki()
        self.addReleaseToHome()
        releaseNote = self.getReleaseNote()
        self.createReleaseConfig(releaseNote)
        ms = self.parseReleaseForMilestone(releaseNote)
        issues = self.getIssues(ms)
        content = self.addIssues(releaseNote, issues)
        content = self.addDocs(content)
        #self.addWikiPage(self.commit_ref, content)
        self.uploadWiki(content)
         #self.addWikiPage(self.commit_ref, "hello")
#        #self.deleteWikiPage()

if __name__ == '__main__':
    original_stdout = sys.stdout
    #GitlabRunner(authkey="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", repoPath="ddgclient/hell1", commit_ref="FirstRelease", wikiUrl="https://github.com/ddgclient/hell1.wiki.git").run()
    
    with open('log.txt', 'w') as f:
        sys.stdout = f # Change the standard output   to the file we created.
        myParser, myargs = getArgs()
        print("hello after pararugement", flush=True)
        try:
            GitlabRunner(wikiUrl=myargs.wiki_url, authkey=myargs.authkey, repoPath=myargs.repoPath, commit_ref=myargs.commit_ref).run()
        except Exception as ex:
            print("ERROR running the code", flush=True)
            raise ex
        finally:
            sys.stdout = original_stdout # Reset the standard output to its original value