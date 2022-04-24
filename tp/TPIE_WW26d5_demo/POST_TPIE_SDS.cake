<Cake OriginalMD5="2002f4ab557eb22c80151d64949758d5" CurrentMD5="0ba7f86d9972bd8abfbce661f9d746e6" SaveUser="jthwing" LastSaveTime="5/6/2021 3:20:05 PM">
<Settings>
<ScriptOwner>Unknown</ScriptOwner>
</Settings>
<GlobalConfigs>
  <Config>
    <Key>MinVersion</Key>
    <Value>1.3.4.396 </Value>
  </Config>
  <Config>
    <Key>Socket</Key>
    <Value>SDx</Value>
  </Config>
  <Config>
    <Key>Product</Key>
    <Value>MTL</Value>
  </Config>
  <Config>
    <Key>STPLFILE</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>TPLFILE</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>ENVFILE</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>SOCKET</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>TOSVERSION</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>Subfamily</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>BOMGROUP</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>PROGRAMTYPE</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>BASETPNAME</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>DEVICE</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>FuseRootDir</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>PLISTFILE</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>DEDC</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>SOCKETFILE</Key>
    <Value>Default</Value>
  </Config>
  <Config>
    <Key>USRVFILE</Key>
    <Value>UservarDefinitions.usrv</Value>
  </Config>
  <ScriptObject>
    <Order>-999</Order>
    <ScriptDisabled>0</ScriptDisabled>
    <Script>//css_dir C:\Program Files ((x86))\Cake;  /* Added automatically (please do not modify) */
//css_dir C:\Program Files\Cake;  /* Added automatically (please do not modify) */
using System;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Cake.Scripting;
using Cake.Scripting.OTPL;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Scripting
{
    //DO NOT CHANGE SCRIPTNAME GlobalConfigScript
    public class GlobalConfigScript : ScriptGlobalsMethodBase
    {

        [STAThread]
        static public void Main(string[] args)
        {
            //When Doing Debug Set to a location containing what you are debugging
            currentFolder = @"";

                //WARNING: Do Not Edit the following line or add additional lines before the closing line (They will be removed), change the Global Variables and Edit the script again to update
                GlobalConfigs GlobalConfigDebug = new GlobalConfigs()  {{@"MinVersion", @"1.3.4.396 "}, {@"Socket", @"SDx"}, {@"Product", @"MTL"}, {@"STPLFILE", @"Default"}, {@"TPLFILE", @"Default"}, {@"ENVFILE", @"Default"}, {@"SOCKET", @"Default"}, {@"TOSVERSION", @"Default"}, {@"Subfamily", @"Default"}, {@"BOMGROUP", @"Default"}, {@"PROGRAMTYPE", @"Default"}, {@"BASETPNAME", @"Default"}, {@"DEVICE", @"Default"}, {@"FuseRootDir", @"Default"}, {@"PLISTFILE", @"Default"}, {@"DEDC", @"Default"}, {@"SOCKETFILE", @"Default"}, {@"USRVFILE", @"UservarDefinitions.usrv"}};

            //Main is called during debug in visual studio as the entry point to the script, it is not required in the final Script
            ScriptReply reply = new GlobalConfigScript
            { }.ExecuteGlobalScript(ref GlobalConfigDebug, true); //Call Script Verify and then Execute via the .Run Extension Method
        }

        //When the Hosting script pulls the description this is what it is pulling. This is required by the Scripting Interface ScriptMethodBase
        public override string Description { get { return "GlobalConfigScript used as a starting point only"; } }

        //Get the Executing Folder from the Environment. This should be the Test Program Path and used in ExecuteScript and ExecuteVerify
        static string currentFolder = Environment.CurrentDirectory;
        private const string Owner = @"conor.p.boland@intel.com";
        private const string updateHTTP = @"http://cake.intel.com/installer/beta/Cake.msi";
        private const string updateGUID = "{6B9168C7-000C-466F-9161-231615846F3C}";
        private const string APP_NAME = "CAKE";
        private static readonly string TEMP_MSI_FOLDER = @"C:\Temp\" + APP_NAME + "Upgrade";
        private static readonly string UPGRADE_BATCH_FILE = Path.Combine(TEMP_MSI_FOLDER, "Upgrade.bat");
        private static readonly string BACKOUT_BATCH_FILE = Path.Combine(TEMP_MSI_FOLDER, "Backout.bat");

        public override Cake.Scripting.ScriptReply ExecuteGlobalUpdate(ScriptReply reply, ref GlobalConfigs GlobalConfig)
        {
            //The Try/Catch/Finally blocks wraps your content, while it's not required as CAKE will handle all errors, it's your opportunity 
            //to Catch and handle errors in your script
            try
            {
                //Update to the Min Versio if Required
                Update(ref reply, GlobalConfig.GetValuebyKey("MinVersion"), updateHTTP, "");

                reply.LogMessage("TP BASE Load", LogLevel.Info);

                //Use CAKE Built in Method to read Report File if it Exists
                string TPIEReportFile = Path.Combine(currentFolder, "Reports", "Integration_Report.txt");
                if (!File.Exists(TPIEReportFile))
                {
                    throw new Exception(string.Format("The TPIE REPORT FILE does NOT exist - {0}"));
                }

                //Open the TPIE Report file and Gather some Data for the Remaining Scripts
                string socket = string.Empty;
                string product = string.Empty;
                string family = string.Empty;
                string subfamily = string.Empty;
                string group = string.Empty;
                string alttp1 = string.Empty;
                string alttp2 = string.Empty;
                using (TPIE_Report tpr = new TPIE_Report(ref reply, TPIEReportFile))
                {
                    //Check for the DEDC Module
                    if ((tpr.ReturnFlow("FINAL").Contains("TPI_DEDC")) || (tpr.ReturnFlow("DEDC").Count &gt; 0))
                        GlobalConfig.UpdateOrAdd(GlobalSuite.Globals.Dedc, "1");

                    //Check that the Program used is a Valid Program in the Test RoadMap
                    DateTime releaseDate;
					CultureInfo enUS = new CultureInfo("en-US"); 
                    if (DateTime.TryParseExact(tpr.ProgramReleaseDate, "MM/dd/yy", new CultureInfo("en-US"), DateTimeStyles.None, out releaseDate))
                    {
                        DateTime dateValue;
                        dateValue = DateTime.Today;
                        if (dateValue.Date &gt; releaseDate.Date)
                        {
                            // **** START TODO: Choose functionality for your product
                            reply.LogMessage(string.Format("The BASE program {0}::{1}::{2} release date {3} is in the past, This is not a valid Base Program. Contact the Test Program Team.", tpr.ProgramFamily, tpr.ProgramSubFamily, tpr.ProgramBaseTP, tpr.ProgramReleaseDate), LogLevel.Caution);
                            //return reply;
                            // **** END TODO
                        }
                    }
                    else
                    {
                        //Log it and just stop right here
                        reply.LogMessage(string.Format("Failed to parse DateTime -{0}- from TPIE Report File, unable to determine if program is a valid program", tpr.ProgramReleaseDate), LogLevel.Abort);
                        return reply;
                    }

                    //Set the Globals Subfamilt Config name
                    GlobalConfig.UpdateOrAdd(GlobalSuite.Globals.Subfamily, tpr.ProgramSubFamily);


                    // **** START TODO: Product Specific Configuration's //ex. MTL CPU28 SDx
                    var split = tpr.ProgramSubFamily.Split(new Char[] { ' ' });
                    if (split.Length &gt;= 3)
                    {
                        socket = split[2].ToString();
                        product = split[1].ToString();
                        family = split[0].ToString();
                        group = string.Join("_", split);
                    }
                    else
                    {
                        socket = "NA";
                        product = "NA";
                        family = "NA";
                        group = tpr.ProgramSubFamily;
                    }

                    GlobalConfig.Configurations[GlobalSuite.Globals.Socket] = socket;
                    GlobalConfig.Configurations[GlobalSuite.Globals.Product] = product;
                    GlobalConfig.Configurations[GlobalSuite.Globals.Bomgroup] = group;
                    GlobalConfig.Configurations[GlobalSuite.Globals.ProgramType] = tpr.ProgramType;
                    GlobalConfig.Configurations[GlobalSuite.Globals.BaseTPName] = tpr.ProgramBaseTP;
                    // **** END TODO: Product Specific Configuration's

                    //If defined grab the ALTTPNAME
                    alttp1 = tpr.SelectSection("Program Identification").GetValueByName("&lt;Alt TP Name 1&gt;");

                }

                //Read the Current FUSE ROOT DIR
                string envFile = string.Empty;
                if (File.Exists(Path.Combine(currentFolder, "EnvironmentFile_!ENG!.env")))
                {
                    envFile = Path.Combine(currentFolder, "EnvironmentFile_!ENG!.env");
                }
                else if (File.Exists(Path.Combine(currentFolder, "EnvironmentFile.env")))
                {
                    envFile = Path.Combine(currentFolder, "EnvironmentFile.env");
                }
                else
                {
                    envFile = FileUtilities.AskUserAboutFiles(ref reply, currentFolder, @"EnvironmentFile.*[.]env$", "EnvironmentFile_!ENG!.env", SearchOption.TopDirectoryOnly, false, true);
                }
                string userProgram = string.Empty;
                using (ENVIRONMENTFILE_ORDERED env = new ENVIRONMENTFILE_ORDERED(ref reply, envFile))
                {
                    //Set the Fuse Root Dir from the ENV File it's used later in the UserVars
                    GlobalConfig.Configurations[GlobalSuite.Globals.FuseRootDir] = new DirectoryInfo(env.GetVariableValue("FUSE_ROOT_DIR", new List&lt;string&gt;())[0]).Name;

                    EnvVariables userProgENV;
                    if (!env.ContainsVariable("TP_NAME", out userProgENV))
                    {
                        if (env.ContainsVariable("MODULAR_TP_PROGRAM_NAME", out userProgENV))
                        {
                            userProgram = userProgENV.Values[0];
                        }
                        else if (!string.IsNullOrEmpty(alttp1))
                        {
                            userProgram = alttp1;
                        }
                        else
                        {
                            reply.LogMessage("The ENV File does not have the Variable TP_NAME or MODULAR_TP_PROGRAM_NAME, this is the BASE TP Name for the Program", LogLevel.Error);
                            return reply;
                        }
                    }
                    else
                    {
                        userProgram = userProgENV.Values[0];
                    }

                    if (string.IsNullOrEmpty(userProgram))
                    {
                        reply.LogMessage("The Program does not define the base TP Name either in ENV(TP_NAME) or ALTTPNamne in TPIE unable to proceed for HDMT Program", LogLevel.Error);
                    }

                    reply.LogMessage("Found BASETP NAME: " + userProgram, LogLevel.Info);
                    GlobalConfig.Configurations[GlobalSuite.Globals.BaseTPName] = userProgram;

                    //Check for the other Required Variable what TOS Version
                    EnvVariables userEnvHolder;
                    if (!env.ContainsVariable("TP_TOS", out userEnvHolder))
                    {
                        reply.LogMessage("The ENV File does not have the Variable TP_TOS, this is Required to be set in the BASE TP", LogLevel.Error);
                    }
                    else
                    {
                        GlobalConfig.Configurations[GlobalSuite.Globals.TosVersion] = userEnvHolder.Values[0];
                    }

                    //Check for the other Required Variable what PLIST Name
                    if (!env.ContainsVariable("TP_PLIST", out userEnvHolder))
                    {
                        reply.LogMessage("The ENV File does not have the Variable TP_PLIST, this is the BASE TP Name for the Program", LogLevel.Error);
                    }
                    else
                    {
                        GlobalConfig.Configurations[GlobalSuite.Globals.PLISTFILE] = userEnvHolder.Values[0];
                    }

                }

                GlobalConfig.UpdateOrAdd(GlobalSuite.Globals.ENVFILE, Path.GetFileName(envFile));
                ConsoleUtilities.ConsoleWriteLineColor(ConsoleColor.White, "Using env file:  " + GlobalConfig.Configurations[GlobalSuite.Globals.ENVFILE]);

                List&lt;string&gt; validSTPLNames = new List&lt;string&gt;(); // = new List&lt;string&gt; { Path.Combine(currentFolder, "SubTestPlan_MTL_GCD64_SDS.stpl") };
                List&lt;string&gt; validSOCFiles = new List&lt;string&gt;(); // = new List&lt;string&gt; { Path.Combine(currentFolder, "SDX_MTL_GCD64_X1.soc") };
                string validSOC = string.Empty;
                List&lt;string&gt; invalidFiles = new List&lt;string&gt;();
                string useSocket = string.Empty;
                string useProduct = string.Empty;

                // **** START TODO: Product Specific Configuration's
                if (GlobalConfig.Configurations[GlobalSuite.Globals.Socket].ToUpper() == "SDX")
                {
                    useSocket = "SDS";
                }
                else if (GlobalConfig.Configurations[GlobalSuite.Globals.Socket].ToUpper() == "SDT")
                {
                    useSocket = "SDT";
                }
                if (GlobalConfig.Configurations[GlobalSuite.Globals.Product].ToUpper() == "CPU28")
                {
                    useProduct = "C28";
                    //ClampKitMap set by Product
                    //GlobalConfig.Configurations[GlobalSuite.Globals.Device] = "8PIA";
                }
                else if (GlobalConfig.Configurations[GlobalSuite.Globals.Product].ToUpper() == "CPU68")
                {
                    useProduct = "C68";
                }

                //find the files - first STPL, then SOC file
                if (!string.IsNullOrEmpty(useSocket))
                {
                    validSTPLNames = new List&lt;string&gt; { Path.Combine(currentFolder, "SubTestPlan_" + family + "_" + product + "_" + useSocket + ".stpl") };
                    if (!File.Exists(validSTPLNames[0]))
                    {
                        validSTPLNames.Clear();
                    }
                }
                if (validSTPLNames.Count == 0)
                {
                    validSTPLNames = FileUtilities.FindFiles(ref reply, currentFolder, @"^.*[.]stpl$", SearchOption.TopDirectoryOnly);
                }

                if (!string.IsNullOrEmpty(useSocket))
                {
                    validSOC = Path.Combine(currentFolder, "SDX_" + useProduct + "_X2_12.soc");
                }
                //if string is empty or file doesn't exist, find a new file
                if (string.IsNullOrEmpty(validSOC) || !File.Exists(validSOC))
                {
                    //validSOCFiles = FileUtilities.FindFiles(ref reply, currentFolder, @"^SD.*[.]soc$", SearchOption.TopDirectoryOnly);
                    validSOC = FileUtilities.AskUserAboutFiles(ref reply, currentFolder, @"^SD.*[.]soc$", "SDX_" + useProduct + "_X2_12.soc", SearchOption.TopDirectoryOnly, false, true);
                }

                //invalidFiles = new List&lt;string&gt; { Path.Combine(currentFolder, "LocationsSets.txt") };
                invalidFiles = invalidFiles.Concat(FileUtilities.FindFiles(ref reply, currentFolder, @"^.*[.]stpl$", SearchOption.TopDirectoryOnly)).ToList();
                if (invalidFiles.Contains(validSTPLNames[0]))
                {
                    invalidFiles.Remove(validSTPLNames[0]);
                }


                invalidFiles = invalidFiles.Concat(FileUtilities.FindFiles(ref reply, currentFolder, @"^SD.*[.]soc$", SearchOption.TopDirectoryOnly)).ToList();
                if (invalidFiles.Contains(validSOC))
                {
                    invalidFiles.Remove(validSOC);
                }
                if (invalidFiles.Contains(Path.Combine(currentFolder, "SDX_" + useProduct + "_X1_1.soc")))
                {
                    invalidFiles.Remove(Path.Combine(currentFolder, "SDX_" + useProduct + "_X1_1.soc"));
                }
                // **** END TODO: Product Specific Configuration's

                //Rename any Extra files not needed as defined in the Product setup above
                foreach (var badFile in invalidFiles)
                {
                    try
                    {
                        File.Move(Path.Combine(currentFolder, badFile), Path.Combine(currentFolder, badFile + "DONOTUSE"));
                        reply.LogMessage("Moving file " + badFile + " to " + badFile + "DONOTUSE", LogLevel.Info);
                    }
                    catch (Exception exD)
                    {
                        reply.LogMessage(string.Format("Unable to Remove Unused File - {0} with message - {1}", badFile, exD.Message), LogLevel.Error);
                    }
                }

                GlobalSuite.moveUnusedFilesAndSetGlobal(ref reply, ref GlobalConfig, currentFolder, @"[.]stpl$", GlobalSuite.Globals.STPLFILE, validSTPLNames);
                GlobalSuite.moveUnusedFilesAndSetGlobal(ref reply, ref GlobalConfig, currentFolder, @"[.]tpl$", GlobalSuite.Globals.TPLFILE);
                //GlobalSuite.moveUnusedFilesAndSetGlobal(ref reply, ref GlobalConfig, currentFolder, @"[.]soc$", GlobalSuite.Globals.SOCKETFILE, validSOCFiles);
                GlobalConfig.UpdateOrAdd(GlobalSuite.Globals.SOCKETFILE, Path.GetFileName(validSOC));
                
                ConsoleUtilities.ConsoleWriteLineColor(ConsoleColor.White, "Using stpl file: " + GlobalConfig.Configurations[GlobalSuite.Globals.STPLFILE]);
                ConsoleUtilities.ConsoleWriteLineColor(ConsoleColor.White, "Using tpl file:  " + GlobalConfig.Configurations[GlobalSuite.Globals.TPLFILE]);
                ConsoleUtilities.ConsoleWriteLineColor(ConsoleColor.White, "Using soc file:  " + GlobalConfig.Configurations[GlobalSuite.Globals.SOCKETFILE]);

                //ConsoleUtilities.ConsoleWriteLineColor(ConsoleColor.White, "Done");


            }
            catch (Exception verifyEX)
            {
                //Log any exceptions and Stop returning that log to the calling function Do not remove
                reply.LogException(verifyEX, new StackFrame(1).GetMethod().ReflectedType.Name);
            }
            //Return Data to calling Script Engine Do not Remove
            return reply;
        }

        /// &lt;summary&gt;
        /// Update to the Minimum version if a Min Version was selected 
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;Normal user Reply&lt;/param&gt;
        /// &lt;param name="minVersion"&gt;The Min version of Cake Required to run this script in the format 1.3.4.251&lt;/param&gt;
        /// &lt;param name="msiPath"&gt;HTTP path to the Beta MSI&lt;/param&gt;
        /// &lt;param name="executingScript"&gt;If the script is passed here after the install it will try to run this script&lt;/param&gt;
        private static void Update(ref ScriptReply reply, string minVersion, string msiPath, string executingScript)
        {
            if (string.IsNullOrEmpty(minVersion))
            {
                reply.LogMessage(@"No MinVersion Found in the Script", LogLevel.ScriptIssue);
                return; //No Min version selected
            }

            Version assemblyVersion = typeof(ScriptReply).Assembly.GetName().Version;
            //The Try/Catch/Finally blocks wraps your content, while it's not required as CAKE will handle all errors, it's your opportunity 
            //to Catch and handle errors in your script
            try
            {

                Cake.Scripting.ScriptingUtilities.ValidateMinimumVersion(minVersion);
                reply.LogMessage(@"Version was ok no need to update", LogLevel.Info);
                //Version is ok just return
                return;
            }
            catch
            {
                reply.LogMessage(@"Unable to get Version from Assembly", LogLevel.ScriptIssue);
                //force an Update to the Beta Version by continuing this
            }

            reply.LogMessage(@"Starting Update Logic", LogLevel.Info);
            Console.Write("A higher version of Cake is required to run this Script. Would you like to update to Cake Beta (Y/N)? : ");
            string userInput = Console.ReadLine().ToUpper();

            while (!userInput.Equals("Y") &amp;&amp; !userInput.Equals("N"))
            {
                Console.Write("\nPlease enter a valid response: ");
                userInput = Console.ReadLine().ToUpper();
            }
            if (userInput.Equals("Y"))
            {
                //Force an Update to the Beta Version of CAKE
                reply.LogMessage("Aborting Script Run for a forced Beta Upgrade", LogLevel.Abort);
                Console.WriteLine("Aborting Script Run for a forced Beta Upgrade");
            }
            else if (userInput.Equals("N"))
            {
                reply.LogMessage("The Minimum Version Requirement was not met. Could not run Script", LogLevel.Abort);
                throw new Exception("Minimum version required does not exist");
            }

            string currentInstallDir = Path.GetDirectoryName(Application.ExecutablePath);
            string installPath = string.Empty;
            if (currentInstallDir.Length &gt; 0)
            {
                installPath = @" TARGETDIR=""" + currentInstallDir + @""" ";
            }

            //Log("Updating package with [" + msiPath + "] released on " + lastModifiedDateTime.ToString() + ".");
            string UpgradeDir = TEMP_MSI_FOLDER;
            if (!Directory.Exists(UpgradeDir))
            {
                Directory.CreateDirectory(UpgradeDir);
            }

            //Set the download path
            string newMsiPath = Path.Combine(UpgradeDir, Path.GetFileNameWithoutExtension(msiPath) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".msi");

            //Check if this is a WEB Location or File Location
            if (msiPath.ToLower().Contains("http"))
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(msiPath, newMsiPath);
                    }
                }
                catch (Exception)
                {
                    //Failed to get Data from Web
                    Cake.Scripting.ScriptingUtilities.Mail("smtp.intel.com", "Cake@intel.com", Owner, string.Format("Cake Web MSI Pull Failed: User: {0}, Assembly: [{1}], MSI: {2}", System.Security.Principal.WindowsIdentity.GetCurrent().Name, "Unknown", msiPath));

                    ShowMessageBox(string.Format("Failed to download MSI Package from {0}. You may need to update Manually, Contact {1} for help.", msiPath, Owner), "UPDATE FAILED", MessageBoxButtons.OK, MessageBoxIcon.Stop);

                    //Kill this Process as we do not continue on outdated version
                    Thread.Sleep(500);
                    Process.GetCurrentProcess().Kill();
                }

            }
            else
            {
                if (File.Exists(msiPath))
                {

                    File.Copy(msiPath, newMsiPath, true);
                    msiPath = newMsiPath;
                }
            }

            string batchContent = "rem @sleep 3" + Environment.NewLine;
            batchContent += "@msiexec /uninstall " + updateGUID + " REBOOT=ReallySuppress /qr /quiet /passive" + Environment.NewLine;
            batchContent += @"@START /WAIT msiexec /i """ + msiPath + @""" " + installPath
                + "REBOOT=ReallySuppress /qr /quiet /passive" + Environment.NewLine;
            if ((executingScript != string.Empty) || (executingScript.Length &gt; 0))
            {
                batchContent += "@\"" + currentInstallDir + "\\cake.host.exe\" " + executingScript + Environment.NewLine;
            }
            string batchFile = UPGRADE_BATCH_FILE;
            if (File.Exists(batchFile))
            {
                try
                {
                    FileCopy(batchFile, BACKOUT_BATCH_FILE, true);
                }
                catch { }
            }
            File.WriteAllText(batchFile, batchContent);
            RunApp(batchFile, "", false);
            Thread.Sleep(500);
            Process.GetCurrentProcess().Kill();
        }

        private static bool RunApp(string app, string arguments, bool waitForProcessToComplete)
        {
            if (app.Length == 0)
            {
                return false;
            }

            using (Process process = new Process())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = true;
                startInfo.FileName = app;
                startInfo.Arguments = arguments;
                startInfo.UseShellExecute = false;
                process.StartInfo = startInfo;

                process.Start();

                if (waitForProcessToComplete)
                {
                    process.WaitForExit();
                }
            }

            return true;
        }

        private static DialogResult ShowMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(message, caption, buttons, icon);
        }

        public static bool FileCopy(string sourceFile, string targetFile, bool overwrite)
        {
            try
            {
                if (overwrite &amp;&amp; File.Exists(targetFile))
                {
                    FileInfo fi = new FileInfo(targetFile);
                    if (fi.IsReadOnly)
                    {
                        fi.IsReadOnly = false;
                    }
                    File.Delete(targetFile);
                }
                File.Copy(sourceFile, targetFile, overwrite);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
</Script>
  </ScriptObject>
</GlobalConfigs>
<ScriptObject>
  <Order>0</Order>
  <ScriptDisabled>0</ScriptDisabled>
  <Script>//css_dir C:\Program Files ((x86))\Cake;  /* Added automatically (please do not modify) */
//css_dir C:\Program Files\Cake;  /* Added automatically (please do not modify) */
using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using Cake.Scripting;
using Cake.Scripting.OTPL;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Scripting
{
    //Update the ClassName to your specific Script Name
    public class POWERTRC: ScriptMethodBase
    {

        [STAThread]
        static public void Main(string[] args)
        {
            //When Doing Debug Set to a location containing what you are debugging
            currentFolder = @"I:\program\1272\eng\hdmtprogs\rkl_sds\cpboland\RKL81S_Arr_Superceed_A";

            //Main is called during debug in visual studio as the entry point to the script, it is not required in the final Script
            ScriptReply reply = new POWERTRC //Update this to Match Script Name above for debug in VS
            {
                //Define any GlobalConfigurations using collection initializer here when running the script from Visual Studio
                //These Globals can be accessed in Veriy or execute by calling GlobalConfig.GetValuebyKey("KEY","DefaultValue");
                //WARNING: Do Not Edit the following line or add additional lines before the closing line (They will be removed), change the Global Variables and Edit the script again to update
                GlobalConfig = new GlobalConfigs() {{@"MinVersion", @"1.3.4.396 "}, {@"Socket", @"SDx"}, {@"Product", @"MTL"}, {@"STPLFILE", @"Default"}, {@"TPLFILE", @"Default"}, {@"ENVFILE", @"Default"}, {@"SOCKET", @"Default"}, {@"TOSVERSION", @"Default"}, {@"Subfamily", @"Default"}, {@"BOMGROUP", @"Default"}, {@"PROGRAMTYPE", @"Default"}, {@"BASETPNAME", @"Default"}, {@"DEVICE", @"Default"}, {@"FuseRootDir", @"Default"}, {@"PLISTFILE", @"Default"}, {@"DEDC", @"Default"}, {@"SOCKETFILE", @"Default"}, {@"USRVFILE", @"UservarDefinitions.usrv"}}
            }.Run(true); //Call Script Verify and then Execute via the .Run Extension Method
        }

        //When the Hosting script pulls the description this is what it is pulling. This is required by the Scripting Interface ScriptMethodBase
        public override string Description { get { return "BlankScript used as a starting point only"; } }

        //Get the Executing Folder from the Environment. This should be the Test Program Path and used in ExecuteScript and ExecuteVerify
        static string currentFolder = Environment.CurrentDirectory;

        /// &lt;summary&gt;
        /// Verify is called first, for all scripts in the CAKE file. You should add all checks that you want this script to perform here
        /// take special care to catch errors and continue the checks so you return all of the errors here in verify.
        /// Logging of any Errors, Aborts, or Exceptions will cause the script host not to call the ExecuteScript Method
        /// This is required by the Scripting Interface ScriptMethodBase
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        /// &lt;returns&gt;Standard Cake Script Reply, containing logs and other data to the script engine. This will be returned to the CAKE Host and added to the other Script Replies&lt;/returns&gt;
        public override ScriptReply ExecuteVerify(ScriptReply reply)
        {
            //The Try/Catch/Finally blocks wraps your content, while it's not required as CAKE will handle all errors, it's your opportunity 
            //to Catch and handle errors in your script

            //GlobalConfig = new GlobalConfigs() { { @"MinVersion", @"1.3.4.287" }, { @"Socket", @"SDS" }, { @"Product", @"DG1" }, { @"STPLFILE", @"SubTestPlan_DG1_A0_ES0.stpl" }, { @"TPLFILE", @"BaseTestPlan.tpl" }, { @"ENVFILE", @"EnvironmentFile_!ENG!.env" }, { @"SOCKET", @"Default" }, { @"TOSVERSION", @"Default" }, { @"Subfamily", @"Default" }, { @"BOMGROUP", @"Default" }, { @"PROGRAMTYPE", @"Default" }, { @"BASETPNAME", @"Default" }, { @"DEVICE", @"Default" }, { @"FuseRootDir", @"Default" }, { @"PLISTFILE", @"PLIST_ALL.xml" }, { @"DEDC", @"Default" }, { @"SOCKETFILE", @"SDX_DG1_X1_1.soc" } }

            try
            {
                //This can be an Info Message, it's warning by default for new scripts
                string tpl = Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("TPLFILE"));
                string stpl = Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("STPLFILE"));
                string env = Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("ENVFILE"));
                string plist = Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("PLISTFILE"));
                string soc = Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("SOCKETFILE"));

                //Be sure to pass in the full path to the file in the LoadTP Call
                GlobalSuite.LoadTP(ref reply, tpl, stpl, env, plist, soc);

                // TRC/Report 1-6
                #region TRCs 1-6
                TRCSuite.TRCTPIEErrorFile();
                TRCSuite.TRCFindLoadAllPLISTs();
                TRCSuite.TRCFlowOrder(Path.Combine(currentFolder, "TRC_Flow_SDS.config"));
                TRCSuite.TRCNaming(Path.Combine(currentFolder, "TRCInstanceName.xml"));
                TRCSuite.TRCPortScreenFuseFile();
                ReportSuite.ReportPAS(new ReportSuiteConfiguration());
                //{
                //    CounterduplicateLogLevel = LogLevel.Caution,
                //    CounteremptyLogLevel = LogLevel.Caution
                //});
                #endregion

                // TRC 7: Make custom to your product
                #region TRC 7: Port TRC
                List&lt;int&gt; skipPorts = new List&lt;int&gt; () { -1, -2 };
                List&lt;string&gt; testsToIgnore = new List&lt;string&gt;() { "AD_DE_SHMOO_E_BEGIN_X_X_X_X_X" };
                TRCSuite.TRCPortChecks(skipPorts, new List&lt;string&gt;(), testsToIgnore);
                #endregion

                // TRC 8: Make custom to your product
                #region TRC 8: Scoreboard TRC
                Cake.Scripting.TRC.ScoreBoardLimitConfig SBConfig = new Cake.Scripting.TRC.ScoreBoardLimitConfig(ref reply);
                SBConfig.addLimitConfig("FUN_(CORE|SALOGIC)", new List&lt;int[]&gt; { new int[] { 1000, 1599 } });
                SBConfig.addLimitConfig("ARR_(CCF|CORE|MBIST|DV)_*(HVQK|KS|DPRR|IDLSAPRR)*", new List&lt;int[]&gt; { new int[] { 1600, 2199 } });
                SBConfig.addLimitConfig("SCN_(SOC|CCF|CORE)", new List&lt;int[]&gt; { new int[] { 2200, 2799 } });
                SBConfig.addLimitConfig("(CLK|MIO|SIO|PTH)_(.*)", new List&lt;int[]&gt; { new int[] { 7000, 7999 } });
                SBConfig.addLimitConfig("SCN_(GT|DE|IPU)", new List&lt;int[]&gt; { new int[] { 2200, 2799 } });
                SBConfig.addLimitConfig("FUN_(GT|DE)_*(GMD|GLG)*", new List&lt;int[]&gt; { new int[] { 1000, 1599 } });
                SBConfig.addLimitConfig("ARR_(GT|DE|IPU)", new List&lt;int[]&gt; { new int[] { 1600, 2199 } });
                SBConfig.addLimitConfig("DRV_(RESET|RESET_BASE)", new List&lt;int[]&gt; { new int[] { 4800, 4999 } });
                TRCSuite.TRCScoreboard(SBConfig);
                #endregion

                // TRC 9: Make custom to your product
                #region PLT TRC
                //string goldPins = Path.Combine(currentFolder, "TRC_gold_pins.txt");
                // exceptions for the levels TRCs
                Regex ignoreLevelsBlockInPowerPinCheck = new Regex("zzzzzzzzzzzzzz", RegexOptions.Compiled);
                Regex ignoreLevelsBlockInDigitalPinOverwriteCheck = new Regex("zzzzzzzzzzzzzz", RegexOptions.Compiled);
                //Regex ignoreLevelsBlockInLevelsPinCheck = new Regex("^power_dwn|^SHOPS$|^SHOPS_PWRDWN|^VCCONT$|^prd$|pwr(dn|up)_lvl|^SHOPS$|^SHOPS_PWRDWN|surge_[hl]c_lvl|^VCCONT$|^prd$|SICC|VBUMP|DICC|PGT_MEASURE|SURGE|PWRDWN|VCCONT|_MEAS$|IDV|pgtctrl|Relay_Control", RegexOptions.Compiled);
                Regex ignoreLevelsBlockInLevelsPinCheck = new Regex("(^iccmeas_.*|^trig(_ir500m|)|^pwr(up|dn|dntrue)|^surge_[hl]c|^vbump.*|^vmeas.*|^vcc_cont_ir500m.*|^dps_on_0V)_lvl", RegexOptions.Compiled);
                Regex ignorePingroupInDigitalPinOverwriteCheck = new Regex("zzzzzzzzzzzzzz", RegexOptions.Compiled);
                Regex ignoreLevelsBlockInVoltageChecks = new Regex("zzzzzzzzzzzzzz", RegexOptions.Compiled);
                Regex ignoreLevelsBlocksInDpinOverwriteCheck = new Regex("zzzzzzzzzzzzzz", RegexOptions.Compiled); // Regex to ignore levels blocks with one of the keywords inside

                // this section should contain all enableclock pins defined in the dpins section of the soc file
                //cbList&lt;string&gt; ignoreEnableClockPinsFromDomainCheck = new List&lt;string&gt;() { "DDRDQ_IL17_NIL17_LP71_7_EC" };
                
                // NOTE: This is case sensitive.  All pin names (first column) must be uppercase and all paramater names (second column) must be lowercase
                List&lt;Cake.Scripting.TRC.ForceValueSettings&gt; expectedForceValuesForSupplies = new List&lt;Cake.Scripting.TRC.ForceValueSettings&gt;(){
                                    //HCDPS
                                    new Cake.Scripting.TRC.ForceValueSettings("HC1_VCCCORE0",       "c_vcccore0_prog",         0.9),
                                    new Cake.Scripting.TRC.ForceValueSettings("HC2_VCCCORE1",       "c_vcccore1_prog",         0.9),
                                    new Cake.Scripting.TRC.ForceValueSettings("HC3_VCCATOM0",       "c_vccatom0_prog",         0.9),
                                    new Cake.Scripting.TRC.ForceValueSettings("HC4_VCCATOM1",       "c_vccatom1_prog",         0.9),
                                    new Cake.Scripting.TRC.ForceValueSettings("HC5_VCCR",           "c_vccr_prog",             0.9),
                                    new Cake.Scripting.TRC.ForceValueSettings("HC6_VCCIA",          "c_vccia_prog",            1.4),
                                    
                                    //HVDPS
                                    new Cake.Scripting.TRC.ForceValueSettings("HV1_VNNAON",         "c_vnnaon_prog",           0.77),
                                    
                                    //LCDPS
                                    new Cake.Scripting.TRC.ForceValueSettings("LC1_V1P8A",          "c_v1p8a_prog",            1.8),
                                    //new Cake.Scripting.TRC.ForceValueSettings("LC2_VCCVINFGT0",     "c_vccfpgm0_prog",         0.77),
                                    //new Cake.Scripting.TRC.ForceValueSettings("LC3_VCCVINFGT1",     "c_vccfpgm1_prog",         0.77),
                                    new Cake.Scripting.TRC.ForceValueSettings("LC4_VCCFPGM0",       "c_vccfpgm0_prog",         1.0),
                                    new Cake.Scripting.TRC.ForceValueSettings("LC5_VCCFPGM1",       "c_vccfpgm1_prog",         1.0),
                                    new Cake.Scripting.TRC.ForceValueSettings("LC6_EXBGREF",        "c_extbgref_prog",         0.8),
                                    new Cake.Scripting.TRC.ForceValueSettings("LC7_VCCIASENSE",     "c_vcciasense_prog",       1.4),
                                    
                                    //VLCDPS
                                    //new Cake.Scripting.TRC.ForceValueSettings("VCC0_VLC",           "0V",                       0),
                                    //new Cake.Scripting.TRC.ForceValueSettings("VCC1_VLC",           "0V",                       0),
                                    //new Cake.Scripting.TRC.ForceValueSettings("VCC2_VLC",           "0V",                       0),
                                    //new Cake.Scripting.TRC.ForceValueSettings("VCC3_VLC",           "0V",                       0),
                                    //new Cake.Scripting.TRC.ForceValueSettings("VCC4_VLC",           "0V",                       0),
                                    //new Cake.Scripting.TRC.ForceValueSettings("VCC5_VLC",           "0V",                       0),
                                    //new Cake.Scripting.TRC.ForceValueSettings("VCC6_VLC",           "0V",                       0),
                                    //new Cake.Scripting.TRC.ForceValueSettings("VCC7_VLC",           "0V",                       0),
                };
                
                Dictionary&lt;string, Cake.Scripting.TRC.PowerPinSettings&gt; expectedPowerPinValues = new Dictionary&lt;string, Cake.Scripting.TRC.PowerPinSettings&gt;() {
                    //    { "IRange", new PowerPinSettings(
                    //        hcdps:  new List&lt;string&gt;(){ "IR24A"},
                    //        lcdps:  new List&lt;string&gt;(){ "IR1_2A"},
                    //        vlcdps: new List&lt;string&gt;(){ "IR256mA"} )
                    //    },
                    //    { "FreeDriveTime", new PowerPinSettings(
                    //        hcdps:  new List&lt;string&gt;(){"500uS", ".5mS", "0.5mS"},
                    //        lcdps:  new List&lt;string&gt;(){"500uS", ".5mS", "0.5mS"},
                    //        vlcdps: new List&lt;string&gt;(){"500uS", ".5mS", "0.5mS"} )
                    //    },
                    //    { "VSlewStepRatio", new PowerPinSettings(
                    //        hcdps:  new List&lt;string&gt;(){"120"},
                    //        lcdps:  null,
                    //        vlcdps: null )
                    //    },
                };

                //TRCSuite.TRCPLT_HDMT(goldPins, ignoreEnableClockPinsFromDomainCheck, ignoreLevelsBlockInPowerPinCheck, ignoreLevelsBlocksInDpinOverwriteCheck,
                //   ignoreLevelsBlockInLevelsPinCheck, ignorePingroupInDigitalPinOverwriteCheck, ignoreLevelsBlockInVoltageChecks,
                //   ignoreLevelsBlocksInDpinOverwriteCheck, expectedForceValuesForSupplies, expectedPowerPinValues);
                #endregion

                // TRC 10: Make custom to your product
                #region Continuity TRC
                // If you need exclude certain pins from continuity tests, then add them the following List.
                List&lt;string&gt; contCoverageExcludePins = new List&lt;string&gt; { "LC6_EXTBGREF", "LC2_VCCVINFGT0", "LC3_VCCVINFGT1", "LC4_VCCFPGM0", "LC5_VCCFPGM1", "FIVR_VTARGET_VDDQ" };

                // If you need exclude certain pins from continuity tests, then add them the following List.
                List&lt;string&gt; surgeExcludePins = new List&lt;string&gt;() { "LC6_EXTBGREF", "LC2_VCCVINFGT0", "LC3_VCCVINFGT1", "LC4_VCCFPGM0", "LC5_VCCFPGM1", "FIVR_VTARGET_VDDQ" };

                // Use regex to define patterns in the following dictionary.
                // Keys are used to search for matching flows.
                // Values are used to define allowed level types.
                Dictionary&lt;string, List&lt;string&gt;&gt; checkSet = new Dictionary&lt;string, List&lt;string&gt;&gt;()
                {
                    {@"_*SOC_SEGMENTED", new List&lt;string&gt; { "BASE::power_dwn_xxx_pwrd_zerzer", "BASE::SBF_GToff_COREoff_*" }},
                };

                List&lt;string&gt; relayStatesExcludePins = new List&lt;string&gt;() { "PHOTO_DETECT_LC" };

                List&lt;Cake.Scripting.TRC.CurrentClampSettings&gt; settings = new List&lt;Cake.Scripting.TRC.CurrentClampSettings&gt;()
                {
                    //new  Cake.Scripting.TRC.CurrentClampSettings("SURGE", "iCDCParametricTest", 1.0, new string[] { "clamphi", "clamplo" },new string[] { "IClampHi", "IClampLo" }),
                    //new  Cake.Scripting.TRC.CurrentClampSettings("SURGE", "iCPowerDownTest", 1.0, new string[] { },new string[] { "IClampHi", "IClampLo" }),
                    new  Cake.Scripting.TRC.CurrentClampSettings("CONT", "iCVccContinuityCPGTest", 0.5, new string[] { "clamp_high", "clamp_low" },new string[]{ "IClampHi", "IClampLo"})
                };

                // limits format: IClamp +/- (A), VForce (V)
                // ex ("VCC0_HC", 0.5, 1.1) means +/- 0.5 Amps and 1.1 Volts
                List&lt;Cake.Scripting.TRC.LimitsSettings&gt; limits = new List&lt;Cake.Scripting.TRC.LimitsSettings&gt;() {
                    new  Cake.Scripting.TRC.LimitsSettings("HV1_VNNAON", 1,2),
                    new  Cake.Scripting.TRC.LimitsSettings("HC1_VCCCORE0", 1,2),
                    new  Cake.Scripting.TRC.LimitsSettings("HC2_VCCCORE1", 1,2),
                    new  Cake.Scripting.TRC.LimitsSettings("HC3_VCCATOM0", 1,2),
                    new  Cake.Scripting.TRC.LimitsSettings("HC4_VCCATOM1", 1,2),
                    new  Cake.Scripting.TRC.LimitsSettings("HC5_VCCR", 1,2),
                    new  Cake.Scripting.TRC.LimitsSettings("HC6_VCCIA", 1,2),
                    new  Cake.Scripting.TRC.LimitsSettings("LC1_V1P8A", 1,2),
                    new  Cake.Scripting.TRC.LimitsSettings("LC7_VCCIASENSE", 1,2)
                };
                Regex levelsExclude = new Regex("nom_lvl_PGT_(SA|CORE[0123])_DC_K_STRESS_X_X_X_X_|(Core[0123]|GT|SA)_1[123456]00mV|dlcpll_m(ax|in)_lvl");

                // List of pins names.
                List&lt;string&gt; pinsExclude = new List&lt;string&gt;() { "PHOTO_DETECT_LC" };

                //TRCSuite.TRCContinuity(contCoverageExcludePins, surgeExcludePins, checkSet, relayStatesExcludePins, settings, limits, levelsExclude, pinsExclude);
                #endregion

                // TRC 11-15: General, Python, HVQK, VIPR, VADTL checks
                #region General, Python, HVQK, VIPR checks
                TRCSuite.TRCGeneral();
                TRCSuite.TRCPython();
                TRCSuite.TRCHVQK();
                TRCSuite.TRCVipr(Path.Combine(currentFolder, @"Modules\TPI_VIPR\InputFiles\vipr.setup"), LogLevel.Error);
                TRCSuite.TRCVADTL();
                #endregion
            }
            catch (Exception verifyEX)
            {
                //Log any exceptions and Stop returning that log to the calling function Do not remove
                reply.LogException(verifyEX, new StackFrame(1).GetMethod().ReflectedType.Name);
            }
            //Return Data to calling Script Engine Do not Remove
            return reply;
        }

        /// &lt;summary&gt;
        /// Execute is called for scripts after Verify has passed, here you should do actual modifications. 
        /// Only a log level of Abort will cause the remaining scripts not to be run.
        /// This is required by the Scripting Interface ScriptMethodBase.
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        /// &lt;returns&gt;Standard Cake Script Reply, containing logs and other data to the script engine. This will be returned to the executing user.&lt;/returns&gt;
        public override ScriptReply ExecuteScript(ScriptReply reply)
        {
            //The Try/Catch/Finally blocks wraps your content, while it's not required as CAKE will handle all errors, it's your opportunity 
            //to Catch and handle errors in your script
            try
            {
                //This can be an Info Message, it's warning by default for new scripts
              //  reply.LogMessage("This script does not implement Execute", LogLevel.Warn);
            }
            catch (Exception verifyEX)
            {
                //Log any exceptions and Stop returning that log to the calling function Do not remove
                reply.LogException(verifyEX, new StackFrame(1).GetMethod().ReflectedType.Name);
            }
            //Return Data to calling Script Engine Do not Remove
            return reply;
        }
    }
}
</Script>
</ScriptObject>
<ScriptObject>
  <Order>1</Order>
  <ScriptDisabled>0</ScriptDisabled>
  <Script>//css_dir C:\Program Files ((x86))\Cake;  /* Added automatically (please do not modify) */
//css_dir C:\Program Files\Cake;  /* Added automatically (please do not modify) */
using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using Cake.Scripting;
using Cake.Scripting.OTPL;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Scripting
{
    //Update the ClassName to your specific Script Name
    public class SupersedeFilesFromInputs : ScriptMethodBase
    {

        [STAThread]
        static public void Main(string[] args)
        {
            //When Doing Debug Set to a location containing what you are debugging
            currentFolder = @"\\alpfile3.al.intel.com\sdx_ODS\program\1274\eng\hdmtprogs\dg1_sds\rmbrimey\INTEG\DG1_A0\2019_07_11_DG1_A0_1929_BASE1";

            //Main is called during debug in visual studio as the entry point to the script, it is not required in the final Script
            ScriptReply reply = new SupersedeFilesFromInputs //Update this to Match Script Name above for debug in VS
            {
                //Define any GlobalConfigurations using collection initializer here when running the script from Visual Studio
                //These Globals can be accessed in Veriy or execute by calling GlobalConfig.GetValuebyKey("KEY","DefaultValue");
                //WARNING: Do Not Edit the following line or add additional lines before the closing line (They will be removed), change the Global Variables and Edit the script again to update
                GlobalConfig = new GlobalConfigs() {{@"MinVersion", @"1.3.4.338"}, {@"Socket", @"SDS"}, {@"Product", @"MTL_GCD64"}, {@"STPLFILE", @"SubTestPlan_MTL_GCD64_SDS.stpl"}, {@"TPLFILE", @"BaseTestPlan.tpl"}, {@"ENVFILE", @"EnvironmentFile_!ENG!.env"}, {@"SOCKET", @"Default"}, {@"TOSVERSION", @"Default"}, {@"Subfamily", @"Default"}, {@"BOMGROUP", @"Default"}, {@"PROGRAMTYPE", @"Default"}, {@"BASETPNAME", @"Default"}, {@"DEVICE", @"Default"}, {@"FuseRootDir", @"Default"}, {@"PLISTFILE", @"PLIST_ALL.xml"}, {@"DEDC", @"Default"}, {@"SOCKETFILE", @"SDX_MTL_GCD64_X1.soc"}}
            }.Run(true); //Call Script Verify and then Execute via the .Run Extension Method
        }

        //When the Hosting script pulls the description this is what it is pulling. This is required by the Scripting Interface ScriptMethodBase
            public override string Description { get { return "Superscede_Patterns_From_Inputfiles pull patterns and Plists from input files adding them to the ENV file as needed"; } }

            //Get the Executing Folder from the Environment. This should be the Test Program Path and used in ExecuteScript and ExecuteVerify
            static string currentFolder = Environment.CurrentDirectory;
            /// &lt;summary&gt;
            /// Verify is called first, for all scripts in the CAKE file. You should add all checks that you want this script to perform here
            /// take special care to catch errors and continue the checks so you return all of the errors here in verify.
            /// Logging of any Errors, Aborts, or Exceptions will cause the script host not to call the ExecuteScript Method
            /// This is required by the Scripting Interface ScriptMethodBase
            /// &lt;/summary&gt;
            /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
            /// &lt;returns&gt;Standard Cake Script Reply, containing logs and other data to the script engine. This will be returned to the CAKE Host and added to the other Script Replies&lt;/returns&gt;
            public override ScriptReply ExecuteVerify(ScriptReply reply)
            {
                //The Try/Catch/Finally blocks wraps your content, while it's not required as CAKE will handle all errors, it's your opportunity 
                //to Catch and handle errors in your script
                try
                {
                    superscedeFilesFromInputsFolder(ref reply);
                }
                catch (Exception verifyEX)
                {
                    //Log any exceptions and Stop returning that log to the calling function Do not remove
                    reply.LogException(verifyEX, new StackFrame(1).GetMethod().ReflectedType.Name);
                }
                //Return Data to calling Script Engine Do not Remove
                return reply;
            }

            /// &lt;summary&gt;
            /// Execute is called for scripts after Verify has passed, here you should do actual modifications. 
            /// Only a log level of Abort will cause the remaining scripts not to be run.
            /// This is required by the Scripting Interface ScriptMethodBase.
            /// &lt;/summary&gt;
            /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
            /// &lt;returns&gt;Standard Cake Script Reply, containing logs and other data to the script engine. This will be returned to the executing user.&lt;/returns&gt;
            public override ScriptReply ExecuteScript(ScriptReply reply)
            {
                return reply;
            }

            public void superscedeFilesFromInputsFolder(ref ScriptReply reply)
            {
                // find any files with extensions below and add to the superscedes folder
                //List&lt;string&gt; codeExtensions = new List&lt;string&gt;() { ".ph", ".dll " };
                Dictionary&lt;string, List&lt;string&gt;&gt; patternsExtensions = new Dictionary&lt;string, List&lt;string&gt;&gt;() {
                { ".plist", new List&lt;string&gt;(){"HDST_PLIST_PATH" , @"~HDMT_TPL_DIR\Supersedes\patterns"}},
                { ".pinobj", new List&lt;string&gt;(){"HDST_PAT_PATH"  , @"~HDMT_TPL_DIR\Supersedes\patterns"}},
                { ".pxr", new List&lt;string&gt;(){"HDST_PAT_PATH"  , @"~HDMT_TPL_DIR\Supersedes\patterns"}},
                { ".tmap", new List&lt;string&gt;(){"HDST_PAT_PATH"  , @"~HDMT_TPL_DIR\Supersedes\patterns"}},
            };
                Dictionary&lt;string, List&lt;string&gt;&gt; patternsFiles = new Dictionary&lt;string, List&lt;string&gt;&gt;();

                foreach (var mFolder in Directory.EnumerateDirectories(Path.Combine(currentFolder, "Modules")))
                {
                    if (Directory.Exists(Path.Combine(mFolder, "InputFiles")))
                    {
                        var filesinDir = Directory.GetFiles(Path.Combine(mFolder, "InputFiles"));
                        foreach (string ext in patternsExtensions.Keys)
                        {
                            //reply.LogMessage("Searching for " + ext + "files.", LogLevel.Info);
                            if (!patternsFiles.ContainsKey(ext))
                                patternsFiles[ext] = new List&lt;string&gt;();
                            patternsFiles[ext].AddRange(filesinDir.Where(i =&gt; Regex.IsMatch(i, ext.Replace(".", "[.]"), RegexOptions.IgnoreCase)));            // NOTE: in here * == the typical meaning of .*
                        }
                    }
                }

                //Create the supers to Add to the ENV Files
                List&lt;string&gt; newPaths = new List&lt;string&gt;();
                Dictionary&lt;string, Dictionary&lt;string, int&gt;&gt; countOfFiles = new Dictionary&lt;string, Dictionary&lt;string, int&gt;&gt;();
                foreach (string extension in patternsExtensions.Keys)
                {
                    foreach (string file in patternsFiles[extension])
                    {
                        // error and skip file types tmap and pxr
                        if (extension == ".tmap" || extension == ".pxr")
                        {
                            reply.LogMessage("Found file " + file + " but tmap and pxr files cannot be superseded in the InputFiles.  Please supersede it in the Supersede folder", LogLevel.Error);
                            continue;
                        }

                        string filepath = Path.GetDirectoryName(file);
                        filepath = filepath.Replace(currentFolder, "~HDMT_TPL_DIR");       // make paths relative to the 

                        //int position = patPath.IndexOf(patternsExtensions[extension][1]);           // we want to insert right after this value
                        if (!newPaths.Contains(filepath))
                            newPaths.Add(filepath);

                        // add module to hash and increment count
                        Match m = Regex.Match(filepath, @"~HDMT_TPL_DIR\\Modules\\(.*)\\InputFiles");
                        string module = m.Groups[1].ToString();

                        if (countOfFiles.ContainsKey(module) &amp;&amp; countOfFiles[module].ContainsKey(extension))
                            countOfFiles[module][extension]++;
                        else if (countOfFiles.ContainsKey(module))
                            countOfFiles[module].Add(extension, 1);
                        else
                            countOfFiles.Add(module, new Dictionary&lt;string, int&gt;() { { extension, 1 } });
                    }
                }

                //print caution message here
                foreach (string module in countOfFiles.Keys.OrderBy(key =&gt; key))
                {
                    string info = "";
                    foreach (var item in countOfFiles[module].OrderBy(i =&gt; i.Key))
                    {
                        info += item.Value + " " + item.Key + " file(s), ";
                    }
                    reply.LogMessage(String.Format("Module {0} has {1}", module, info), LogLevel.Caution);
                }

                string[] environmentFileLoc = Directory.GetFiles(currentFolder, "EnvironmentFile*.env"); // An array that holds all the environment file location in the test program folder
                foreach (var envFileRef in environmentFileLoc)
                {
                    //Loop through all of the Valid ENV Files which may include a ENG and PROD Version
                    using (ENVIRONMENTFILE_ORDERED env = new ENVIRONMENTFILE_ORDERED(ref reply, envFileRef))
                    {
                        //var patPath = env.GetVariableValue(patternsExtensions[extension][0], new List&lt;string&gt;());
                        foreach (var newPath in newPaths)
                        {
                            env.AddOrCreateVariableTop("HDST_PLIST_PATH", newPath); //.AddOrCreateVariableAtPosition(patternsExtensions[extension][0], filepath, position + 1);
                            env.AddOrCreateVariableTop("HDST_PAT_PATH", newPath);
                            //
                        }

                        env.Save(true); // Save the ENV File. True indicates a backup of the original will be made in the Pantry this save is not Done until the end and all changes are made
                    }
                }
            }
        }
    }
</Script>
</ScriptObject>
<ScriptObject>
  <Order>2</Order>
  <ScriptDisabled>0</ScriptDisabled>
  <Script>//css_dir C:\Program Files ((x86))\Cake;  /* Added automatically (please do not modify) */
//css_dir C:\Program Files\Cake;  /* Added automatically (please do not modify) */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using Cake.Scripting;
using Cake.ScriptRunner;
using Cake.Scripting.OTPL;
using System.Diagnostics;
using System.IO;

namespace Scripting
{
    //Update the ClassName to your specific Script Name
    public class Sprinkles : ScriptMethodBase
    {

        [STAThread]
        static public void Main(string[] args)
        {
            //When Doing Debug Set to a location containing what you are debugging
            currentFolder = @"\\alpfile3.al.intel.com\sdx\program\1272\eng\hdmtprogs\kbl_sds\sdpierce\FUTP_666";

            //Main is called during debug in visual studio as the entry point to the script, it is not required in the final Script
            ScriptReply reply = new Sprinkles //Update this to Match Script Name above for debug in VS
            {
                //Define any GlobalConfigurations using collection initializer here when running the script from Visual Studio
                //These Globals can be accessed in Veriy or execute by calling GlobalConfig.GetValuebyKey("KEY","DefaultValue");
                //WARNING: Do Not Edit the following line or add additional lines before the closing line (They will be removed), change the Global Variables and Edit the script again to update
                GlobalConfig = new GlobalConfigs() {{@"MinVersion", @"1.3.4.294"}, {@"Socket", @"SDS"}, {@"Product", @"MTL_GCD64"}, {@"STPLFILE", @"Default"}, {@"TPLFILE", @"Default"}, {@"ENVFILE", @"Default"}, {@"SOCKET", @"Default"}, {@"TOSVERSION", @"Default"}, {@"Subfamily", @"Default"}, {@"BOMGROUP", @"Default"}, {@"PROGRAMTYPE", @"Default"}, {@"BASETPNAME", @"Default"}, {@"DEVICE", @"Default"}, {@"FuseRootDir", @"Default"}, {@"PLISTFILE", @"Default"}, {@"DEDC", @"Default"}, {@"SOCKETFILE", @"Default"}}
            }.Run(true); //Call Script Verify and then Execute via the .Run Extension Method
        }

        //When the Hosting script pulls the description this is what it is pulling. This is required by the Scripting Interface ScriptMethodBase
        public override string Description { get { return "Sprinkles used as a starting point only"; } }

        //Get the Executing Folder from the Environment. This should be the Test Program Path and used in ExecuteScript and ExecuteVerify
        static string currentFolder = Environment.CurrentDirectory;

        /// &lt;summary&gt;
        /// Verify is called first, for all scripts in the CAKE file. You should add all checks that you want this script to perform here
        /// take special care to catch errors and continue the checks so you return all of the errors here in verify.
        /// Logging of any Errors, Aborts, or Exceptions will cause the script host not to call the ExecuteScript Method
        /// This is required by the Scripting Interface ScriptMethodBase
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        /// &lt;returns&gt;Standard Cake Script Reply, containing logs and other data to the script engine. This will be returned to the CAKE Host and added to the other Script Replies&lt;/returns&gt;
        public override ScriptReply ExecuteVerify(ScriptReply reply)
        {
            //Return Data to calling Script Engine Do not Remove
            return reply;
        }

        /// &lt;summary&gt;
        /// Execute is called for scripts after Verify has passed, here you should do actual modifications. 
        /// Only a log level of Abort will cause the remaining scripts not to be run.
        /// This is required by the Scripting Interface ScriptMethodBase.
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        /// &lt;returns&gt;Standard Cake Script Reply, containing logs and other data to the script engine. This will be returned to the executing user.&lt;/returns&gt;
        public override ScriptReply ExecuteScript(ScriptReply reply)
        {
            //The Try/Catch/Finally blocks wraps your content, while it's not required as CAKE will handle all errors, it's your opportunity 
            //to Catch and handle errors in your script
            try
            {
                //Find all Sprinkles
                List&lt;string&gt; sprinkles = new List&lt;string&gt;();
                //Find by Regex all of the Sprinkles.Cake files under Modules for this program
                sprinkles = FileUtilities.FindFiles(ref reply, Path.Combine(currentFolder, "Modules"), @"^Sprinkles[.]Cake$", SearchOption.AllDirectories);

                //Load the Scripts into the Engine and Do basic Verify for all Scripts 
                using (Cake.ScriptRunner.Sprinkles sprinklesObject = new Cake.ScriptRunner.Sprinkles(ref reply, sprinkles, GlobalConfig))
                {
                    ScriptReplyResult eResult = sprinklesObject.Run();
                }

                //Execute all Sprinkles, Handle Aborts, Exceptions, etc etc
                //Return all Logs
            }
            catch (Exception verifyEX)
            {
                //Log any exceptions and Stop returning that log to the calling function Do not remove
                reply.LogException(verifyEX, new StackFrame(1).GetMethod().ReflectedType.Name);
            }
            //Return Data to calling Script Engine Do not Remove
            return reply;
        }
    }
}</Script>
</ScriptObject>
<ScriptObject>
  <Order>3</Order>
  <ScriptDisabled>0</ScriptDisabled>
  <Script>//css_dir C:\Program Files ((x86))\Cake;  /* Added automatically (please do not modify) */
//css_dir C:\Program Files\Cake;  /* Added automatically (please do not modify) */
using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using Cake.Scripting;
using Cake.Scripting.OTPL;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace Scripting
{
    //Update the ClassName to your specific Script Name
    public class ForceFlow_TRC : ScriptMethodBase
    {

        [STAThread]
         static public void Main(string[] args)
        {
            //When Doing Debug Set to a location containing what you are debugging            
            currentFolder = @"I:\program\1274\eng\hdmtprogs\dg1_sds\sejaz\DG1_S908_Rev0";

            //Main is called during debug in visual studio as the entry point to the script, it is not required in the final Script
            ScriptReply reply = new ForceFlow_TRC //Update this to Match Script Name above for debug in VS
            {
                //Define any GlobalConfigurations using collection initializer here when running the script from Visual Studio
                //These Globals can be accessed in Veriy or execute by calling GlobalConfig.GetValuebyKey("KEY","DefaultValue");
                //WARNING: Do Not Edit the following line or add additional lines before the closing line (They will be removed), change the Global Variables and Edit the script again to update
                GlobalConfig = new GlobalConfigs() {{@"MinVersion", @"1.3.4.340 "}, {@"Socket", @"SDS"}, {@"Product", @"MTL_GCD64"}, {@"STPLFILE", @"Default"}, {@"TPLFILE", @"Default"}, {@"ENVFILE", @"Default"}, {@"SOCKET", @"Default"}, {@"TOSVERSION", @"Default"}, {@"Subfamily", @"Default"}, {@"BOMGROUP", @"Default"}, {@"PROGRAMTYPE", @"Default"}, {@"BASETPNAME", @"Default"}, {@"DEVICE", @"Default"}, {@"FuseRootDir", @"Default"}, {@"PLISTFILE", @"PLIST_ALL.xml"}, {@"DEDC", @"Default"}, {@"SOCKETFILE", @"SDX_MTL_GCD64_X1.soc"}}
            }.Run(true); //Call Script Verify and then Execute via the .Run Extension Method
        }

        //When the Hosting script pulls the description this is what it is pulling. This is required by the Scripting Interface ScriptMethodBase
        public override string Description { get { return "FullFlow_TRC"; } }

        //Get the Executing Folder from the Environment. This should be the Test Program Path and used in ExecuteScript and ExecuteVerify
        static string currentFolder = Environment.CurrentDirectory;

        /// &lt;summary&gt;
        /// Verify is called first, for all scripts in the CAKE file. You should add all checks that you want this script to perform here
        /// take special care to catch errors and continue the checks so you return all of the errors here in verify.
        /// Logging of any Errors, Aborts, or Exceptions will cause the script host not to call the ExecuteScript Method
        /// This is required by the Scripting Interface ScriptMethodBase
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        /// &lt;returns&gt;Standard Cake Script Reply, containing logs and other data to the script engine. This will be returned to the CAKE Host and added to the other Script Replies&lt;/returns&gt;
        public override ScriptReply ExecuteVerify(ScriptReply reply)
        {
            //The Try/Catch/Finally blocks wraps your content, while it's not required as CAKE will handle all errors, it's your opportunity 
            //to Catch and handle errors in your script
            try
            {
                //if (GlobalConfig.GetValuebyKey("Socket").ToUpper().Equals("SORT"))
                //{
                    // 1) Checks for hanging ports in all the modules.  Excludes isolated and/or bypassed instances.
                    List&lt;string&gt; hangingPortsExceptionsList = new List&lt;string&gt;() { "MODULE_NAME::TEST_INSTANCE_NAME" };
                    checkForHangingPorts(ref reply, hangingPortsExceptionsList);

                    // 2) Trace return 0 instance chains.
                    //traceReturn0InstanceChain(ref reply);

                    // 3) Detects GLXpress instances in main flow.
                    checkForGlXpressInMainFlow(ref reply);

                    // 4) Checks for port0 code failures
                    //List&lt;string&gt; checkFlowList = new List&lt;string&gt;() { "INIT", "LOTSTARTFLOW", "LOTENDFLOW", "TESTPLANENDFLOW", "TESTPLANTSTARTFLOW" };
                    //List&lt;string&gt; checkTestClassList = new List&lt;string&gt;() { "iCPowerDownTest", "iCPatternModifyTest", "iCFuseConfigExeTest", "iCFuseConfigInitTest", "iCScreenTest", "iCUserFuncTest", "iCGlXpressTest", "iCSampleRateTest", "iCSDRWTest", "iCAuxiliaryTest", };
                    //checkForPort0CodeFailures(ref reply, checkFlowList, checkTestClassList);
                //}
                //else reply.LogMessage(string.Format("Skipping {0} since this is not a SORT socket.", this.Description), LogLevel.Info);
            }
            catch (Exception verifyEX)
            {
                //Log any exceptions and Stop returning that log to the calling function Do not remove
                reply.LogException(verifyEX, new StackFrame(1).GetMethod().ReflectedType.Name);
            }
            //Return Data to calling Script Engine Do not Remove
            return reply;
        }

        /// &lt;summary&gt;
        /// Looks for instances that are in Monitor (as specified by the ffc_kill_list for that module) 
        /// on port 0 for the flows specified in the flow list and the test classes specified in the testclass list.
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        /// &lt;param name="flowList"&gt;List of the flows we want to exclude from checking&lt;/param&gt;
        /// &lt;param name="testClassList"&gt;List of test classes we want to include from checking&lt;/param&gt;
        private void checkForPort0CodeFailures(ref ScriptReply reply, List&lt;string&gt; flowList, List&lt;string&gt; testClassList)
        {
            string stplFileLoc = Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("STPLFILE"));

            /* DEBUG */
            /*int counter = 0;
            string fileName = String.Empty;

            do
            {
                fileName = currentFolder + "\\debug\\debug_" + counter + ".txt";
                counter++;
            } while (File.Exists(fileName));

            StreamWriter outputFile = new StreamWriter(fileName);*/
            /* END DEBUG */

            using (STPL stpl = new STPL(ref reply, stplFileLoc))
            {
                foreach (string tplFileLoc in stpl.tplList)
                {
                    using (TPL tpl = new TPL(ref reply, tplFileLoc))
                    {
                        // Store ffc file in local variable
                        string ffcFileLocation = Path.GetDirectoryName(tplFileLoc) + "\\InputFiles\\ffc_kill_list.dat";
                        List&lt;string&gt; ffcList = new List&lt;string&gt;();

                        if (File.Exists(ffcFileLocation))
                            ffcList = loadFFCFile(ffcFileLocation);
                        else continue; // there is no ffc file for this tpl, skip?

                        foreach (KeyValuePair&lt;string, List&lt;Instance&gt;&gt; flowHierarchyKVP in tpl.GetFlowHierarchy(ref reply))
                        {
                            foreach (Test test in tpl.m_tests)
                            {
                                // 1: Am I an instance that uses one of these Test Classes?
                                if (testClassList.Contains(test.template))
                                {
                                    foreach (Instance inst in flowHierarchyKVP.Value)
                                    {
                                        string[] splitModuleName = flowHierarchyKVP.Key.Split('_');

                                        // Match tests to instances
                                        if (inst.InstanceName.Equals(test.name))
                                        {
                                            // Only run checks for instances that aren't in the INIT flow, LOTSTART,LOTEND,TESTPLANENDFLOW, and TESTPLANSTARTFLOW
                                            if (flowHierarchyKVP.Key.StartsWith(tpl.ModuleName) &amp;&amp; !flowList.Contains(splitModuleName[splitModuleName.Length - 1]))
                                            {
                                                // 2: Does Port0 have a set bin assigned in TPL?
                                                foreach (Port port in inst.ports)
                                                {
                                                    //outputFile.WriteLine(test.template + ":" + flowHierarchyKVP.Key);
                                                    // Only look at port 0
                                                    if (port.number == 0)
                                                    {
                                                        //outputFile.WriteLine(test.template + ":" + flowHierarchyKVP.Key + ":" + inst.InstanceName);
                                                        // Check if there exists a bin
                                                        if (port.softbin == null)
                                                        {
                                                            reply.LogMessage(string.Format("Port 0 of {0} in {1} has no bin set", flowHierarchyKVP.Key, inst.InstanceName), LogLevel.Warn);
                                                        }
                                                        else
                                                        {
                                                            // 3: Does FFC have this port0 on this instance set to kill?
                                                            foreach (string ffcItem in ffcList)
                                                            {
                                                                string[] ffcItems = ffcItem.Split(',');
                                                                if (ffcItems[2].Contains(inst.InstanceName + "_0"))
                                                                {
                                                                    if (ffcItems[1].Equals("Monitor"))
                                                                        //outputFile.WriteLine("Error! " + ffcItem);
                                                                        reply.LogMessage(string.Format("{0} is in Monitor. This should be in Kill", ffcItems[2]), LogLevel.Warn);
                                                                    else;//outputFile.WriteLine("Good " + ffcItem);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //outputFile.Close();
        }

        /// &lt;summary&gt;
        /// Scans through all Test instances in every TPL's MAIN flow and checks if there are any iCGlXpressTest type instances.  \
        /// If there are, then it will report an error saying that is not cool.
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        private void checkForGlXpressInMainFlow(ref ScriptReply reply)
        {
            string stplFileLoc = Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("STPLFILE"));

            using (STPL stpl = new STPL(ref reply, stplFileLoc))
            {
                foreach (string tplFileLoc in stpl.tplList)
                {
                    using (TPL tpl = new TPL(ref reply, tplFileLoc))
                    {
                        foreach (KeyValuePair&lt;string, List&lt;Instance&gt;&gt; flowHeirarchyKVP in tpl.GetFlowHierarchy(ref reply))
                        {
                            // For everything that is in the MAIN flow..
                            if (flowHeirarchyKVP.Key.StartsWith(tpl.ModuleName) &amp;&amp; flowHeirarchyKVP.Key.ToUpper().Contains("MAIN"))
                            {
                                foreach (Instance inst in flowHeirarchyKVP.Value.FindAll(x =&gt; (x.isStart || // This module is a start in this flow/dutflow
                                    flowHeirarchyKVP.Value.Exists(y =&gt; y.ports.Exists(p1 =&gt; p1.goTo != null &amp;&amp; p1.goTo.Contains(x.InstanceName)))) &amp;&amp; // This module is not isolated (being referenced by something)
                                    !x.flowItemName.Equals("DUTFlowItem") &amp;&amp; !x.isComposite))
                                {
                                    //tpl.m_tests.Find(t =&gt; t.name.Equals(x.InstanceName)).template.Equals("iCGlXpressTest")
                                    Test t = tpl.GetTestByName(ref reply, inst.InstanceName);
                                    if (t == null)
                                        reply.LogMessage(string.Format("In TPL:{0}, couldn't find a Test with matching name for Instance:{1}.",
                                            Path.GetFileName(tpl.FileName), inst.InstanceName), LogLevel.Error);
                                    else if (t.template != null &amp;&amp; t.template.Equals("iCGlXpressTest"))
                                        reply.LogMessage(string.Format("In TPL:{0}, found Test Instance:{1} in the MAIN flow which is using a iCGlXpressTest template.  Use of iCGlXpressTest template is forbidden inside the MAIN flow.",
                                            Path.GetFileName(tpl.FileName), inst.InstanceName), LogLevel.Error);
                                    else if (t.template == null)
                                        reply.LogMessage(string.Format("In TPL:{0}, Test:{1} has a NULL template.",
                                            Path.GetFileName(tpl.FileName), inst.InstanceName), LogLevel.Error);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// &lt;summary&gt;
        /// This TRC scans through all instances in non-INIT flows which return 0 for:  
        /// 1. Which setbin is used.  
        /// 2. If no setbin is used, then trace back in the program flow until a setbin is used.  
        /// Finally, it will report a trace of the TPL, instance/instance trace, and the resolved setbin assigned.
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        private void traceReturn0InstanceChain(ref ScriptReply reply)
        {
            string stplFileLoc = Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("STPLFILE"));

            using (STPL stpl = new STPL(ref reply, stplFileLoc))
            using (StreamWriter sw = new StreamWriter(Path.Combine(currentFolder, @"Reports/Return0BinTraces.txt")))
            {
                foreach (string tplFileLoc in stpl.tplList)
                {
                    using (TPL tpl = new TPL(ref reply, tplFileLoc))
                    {
                        sw.Write("\n" + Path.GetFileName(tplFileLoc) + ":");
                        foreach (KeyValuePair&lt;string, List&lt;Instance&gt;&gt; flowHeirarchyKVP in tpl.GetFlowHierarchy(ref reply))
                        {
                            // For each flow that is not an INIT flow..
                            if (flowHeirarchyKVP.Key.StartsWith(tpl.ModuleName) &amp;&amp; !flowHeirarchyKVP.Key.ToUpper().Contains("INIT"))
                            {
                                foreach (Instance inst in flowHeirarchyKVP.Value.FindAll(x =&gt; (x.isStart || // This module is a start in this flow/dutflow
                                flowHeirarchyKVP.Value.Exists(y =&gt; y.ports.Exists(p1 =&gt; p1.goTo != null &amp;&amp; p1.goTo.Contains(x.InstanceName)))) &amp;&amp; // This module is not isolated (being referenced by something)
                                x.ports.Exists(p2 =&gt; p2.returnPort != null &amp;&amp; p2.returnPort.Equals("0")))) // Contains a port(s) which returns 0.
                                {
                                    foreach (Port port in inst.ports.FindAll(p =&gt; p.returnPort != null &amp;&amp; p.returnPort.Equals("0")))
                                    {
                                        string res = recursivelyTraceSetBinFor(ref reply, tpl, inst, port, "\t");
                                        if (res != string.Empty)
                                            sw.Write(res);
                                        else sw.Write("\tNone found.");
                                    }
                                }
                                sw.Flush();
                            }
                        }
                    }
                }
            }
        }

        /// &lt;summary&gt;
        /// Parses FFC file into a list of strings. Each line of content (non-comment) is one element within the return list
        /// &lt;/summary&gt;
        /// &lt;param name="path"&gt;Path to the ffc kill list&lt;/param&gt;
        /// &lt;returns&gt;&lt;/returns&gt;
        private List&lt;string&gt; loadFFCFile(string path)
        {
            List&lt;string&gt; ffcList = new List&lt;string&gt;();
            using (StreamReader sr = new StreamReader(path))
            {
                string line = String.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!line.StartsWith("#"))
                    {
                        ffcList.Add(line);
                    }
                }
            }
            return ffcList;
        }

        private string recursivelyTraceSetBinFor(ref ScriptReply reply, TPL tpl, Instance inst, Port port, string level)
        {
            if (port.softbin != null)
                return string.Format("\n{0}{1}({2}-{3}) = {4}", level, inst.InstanceName, port.number, port.passfail, port.softbin);
            //else if(inst.isStart)
            //    return string.Format("\n{0}Dead End: {1}({2}-{3}) = {4}", level, inst.InstanceName, port.number, port.passfail, port.softbin);
            //else if (port.incCounter != null)
            //    return "\n" + level + "---";
            //if (port.passfail!=null &amp;&amp; port.passfail.Equals("Pass"))
            //return string.Format("\n{0}Possible Force-Flow: {1}({2}-{3})", level, inst.InstanceName, port.number, port.passfail);
            else
            {
                Dictionary&lt;Instance, List&lt;Port&gt;&gt; refs = getListOfInstancesWhichCallThisInstance(ref reply, tpl, inst);
                string res = "";
                if (refs.Count &gt; 0)
                {
                    foreach (KeyValuePair&lt;Instance, List&lt;Port&gt;&gt; kvp in refs)
                    {
                        foreach (Port p in kvp.Value)
                        {
                            if (p.passfail != null &amp;&amp; p.passfail.Equals("Pass") &amp;&amp; p.softbin == null)
                                res += string.Format("\n{0}* Passing port, stopping trace: {1}({2}-{3})", level + "\t", kvp.Key.InstanceName, p.number, p.passfail);
                            else
                            res += level + recursivelyTraceSetBinFor(ref reply, tpl, kvp.Key, p, level + "\t");
                        }
                    }
                }
                return string.Format("\n{0}{1}({2}-{3}):{4}", level, inst.InstanceName, port.number, port.passfail, res);
            }
        }

        /// &lt;summary&gt;
        /// Provides a list of instances which are routed to the instance passed as a parameter.  This will resolve changes in 
        /// flow/DUTflow/etc.
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;&lt;/param&gt;
        /// &lt;param name="tpl"&gt;&lt;/param&gt;
        /// &lt;param name="inst"&gt;&lt;/param&gt;
        /// &lt;returns&gt;&lt;/returns&gt;
        private Dictionary&lt;Instance, List&lt;Port&gt;&gt; getListOfInstancesWhichCallThisInstance(ref ScriptReply reply, TPL tpl, Instance inst)
        {
            Dictionary&lt;Instance, List&lt;Port&gt;&gt; references = new Dictionary&lt;Instance, List&lt;Port&gt;&gt;();

            if (!inst.isStart &amp; !inst.isComposite)
            {
                foreach (Instance tmpInst in tpl.m_instances.FindAll(i =&gt; i.ports.Exists(p =&gt; p.goTo != null &amp;&amp; p.goTo.Equals(inst.InstanceName))))
                {
                    foreach (Port port in tmpInst.ports.FindAll(p =&gt; p.goTo != null &amp;&amp; p.goTo.Equals(inst.InstanceName)))
                    {
                        if (references.ContainsKey(tmpInst))
                            references[tmpInst].Add(port);
                        else references.Add(tmpInst, new List&lt;Port&gt;() { port });
                    }
                }
            }
            else if(inst.isStart &amp;&amp; !inst.isComposite) // If this is a start and not a composite, then..
            {
                // Look up a level and find all instances calling the dutflow.
                foreach (Instance tmpInst in tpl.m_instances.FindAll(i =&gt; i.ports.Exists(p =&gt; p.goTo != null &amp;&amp; p.goTo.Equals(inst.dutflow))))
                {
                    foreach (Port port in tmpInst.ports.FindAll(p =&gt; p.goTo != null &amp;&amp; p.goTo.Equals(inst.dutflow)))
                    {
                        if (references.ContainsKey(tmpInst))
                            references[tmpInst].Add(port);
                        else references.Add(tmpInst, new List&lt;Port&gt;() { port });
                    }
                }
            }
            else if(inst.isComposite)
            {
                // Find all instances that within this composite which return 0.
                foreach (Instance tmpInst in tpl.GetFlowItemsbyFlow(ref reply, inst.InstanceName).FindAll(x =&gt; x.ports.Exists(p =&gt; p.returnPort != null &amp;&amp; p.returnPort.Equals("0"))))
                {
                    foreach (Port port in tmpInst.ports.FindAll(p =&gt; p.returnPort != null &amp;&amp; p.returnPort.Equals("0")))
                    {
                        if (references.ContainsKey(tmpInst))
                            references[tmpInst].Add(port);
                        else references.Add(tmpInst, new List&lt;Port&gt;() { port });
                    }
                }

                // Finds all instances which GoTo this composite.
                foreach (Instance tmpInst in tpl.m_instances.FindAll(i =&gt; i.ports.Exists(p =&gt; p.goTo != null &amp;&amp; p.goTo.Equals(inst.InstanceName))))
                {
                    foreach (Port port in tmpInst.ports.FindAll(p =&gt; p.goTo != null &amp;&amp; p.goTo.Equals(inst.InstanceName)))
                    {
                        if (references.ContainsKey(tmpInst))
                            references[tmpInst].Add(port);
                        else references.Add(tmpInst, new List&lt;Port&gt;() { port });
                    }
                }
            }
            else
            {
                reply.LogMessage("WAT? " + Path.GetFileName(tpl.FileName) + ":" + inst.InstanceName, LogLevel.Error);
            }

            return references;
        }

        /// &lt;summary&gt;
        /// Will check that the only instance level ports that have exit -1 are either -2 or -1 ports.
        /// This method will also handle exceptions sent to it, and normally excludes all isolated and/or bypassed instances.
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        /// &lt;param name="exceptions"&gt;List of strings which contain instances to exclude from this search.&lt;/param&gt;
        private void checkForHangingPorts(ref ScriptReply reply, List&lt;string&gt; exceptions)
        {
            using (STPL stpl = new STPL(ref reply, Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("STPLFILE"))))
            {
                foreach(string tplLoc in stpl.tplList)
                {
                    using (TPL tpl = new TPL(ref reply, tplLoc))
                    {
                        #region extra tests disabled
                        /*/ Record all instances with debug_mode != "DISABLED";
                        foreach (Test t in tpl.m_tests.FindAll(x =&gt; x.parameters.ContainsKey("debug_mode") &amp;&amp; !x.parameters["debug_mode"].Equals("\"DISABLED\"")))
                            reply.LogMessage(string.Format("debug_mode not DISABLED, instead found found debug_mode = {0} in TPL:{1}, Test:{2}.",
                                t.parameters["debug_mode"], Path.GetFileName(tplLoc), t.name), LogLevel.Error);*/
                        
                        /* Record all instances with bypass_global = "1";
                        foreach (Test t in tpl.m_tests.FindAll(x =&gt; x.parameters.ContainsKey("bypass_global") &amp;&amp; x.parameters["bypass_global"].Equals("\"1\"")))
                            reply.LogMessage(string.Format("In TPL:{0}, Test:{1} has bypass_global = \"1\".",
                                Path.GetFileName(tplLoc), t.name), LogLevel.Info);*/

                        /*foreach(Instance i in tpl.m_instances)
                        {
                            foreach (Port p in i.ports.FindAll(x =&gt; x.goTo == null &amp;&amp; (x.returnPort == null || x.returnPort.Equals(string.Empty))))
                                reply.LogMessage(string.Format("Unconnected ports found in TPL:{0}, Instance:{1}, Port:{2}.",
                                    Path.GetFileName(tplLoc), i.InstanceName, p.number), LogLevel.Caution);
                        }*/
                        #endregion

                        // Only for instances not on the exceptions list..
                        foreach (Instance inst in tpl.m_instances.FindAll(x =&gt; !exceptions.Contains(x.name)))
                        {
                            // Skip this instance if the test has been bypassed in the TP.
                            Test t = tpl.GetTestByName(ref reply, inst.InstanceName);
                            if (t != null &amp;&amp; t.parameters.ContainsKey("bypass_value") &amp;&amp; t.parameters["bypass_value"].Equals("\"1\""))
                                continue;

                            // If this is a starting instance, or..
                            // if this instance is being referrenced by another instance in this TPL.
                            // Basically, this instance should be part of the flow and not an isolated instance.
                            if (inst.isStart || tpl.m_instances.Exists(x =&gt; x.ports.Exists(y =&gt; y.goTo != null &amp;&amp; y.goTo.Equals(inst.InstanceName))))
                            {
                                // Look for ports that are returning -1 with an exit value of NOT -1 or -2.  Only -1 and -2 value ports should be
                                // returning a value of -1.
                                foreach (Port p in inst.ports.FindAll(p =&gt; p.goTo == null &amp;&amp; p.returnPort != null &amp;&amp; p.returnPort.Equals("-1") &amp;&amp; p.number != -1 &amp;&amp; p.number != -2))
                                    reply.LogMessage(string.Format("Hanging port found in TPL:{0}, Instance:{1}, Result:{2}, Return:{3}.  Cannot return -1 if function doesn't result with -1 or -2.",
                                        Path.GetFileName(tplLoc), inst.InstanceName, p.number, p.returnPort), LogLevel.Caution);

                                #region extra tests disabled
                                // Look for ports that are returning &gt; 0 if they're resulting with &lt;= 0.
                                /*foreach (Port p in inst.ports.FindAll(p =&gt; p.goTo == null &amp;&amp; p.returnPort != null &amp;&amp; int.Parse(p.returnPort) &gt; 0 &amp;&amp; p.number &lt;= 0))
                                    reply.LogMessage(string.Format("Bad module exits as good in TPL:{0}, Instance:{1}, Result:{2}, Return:{3}.",
                                        Path.GetFileName(tplLoc), inst.InstanceName, p.number, p.returnPort), LogLevel.Caution);

                                // Look for ports that are returning &lt;= 0 if they're resulting with &gt; 0.
                                foreach (Port p in inst.ports.FindAll(p =&gt; p.goTo == null &amp;&amp; p.returnPort != null &amp;&amp; int.Parse(p.returnPort) &lt;= 0 &amp;&amp; p.number &gt; 0))
                                    reply.LogMessage(string.Format("Good module exits as bad in TPL:{0}, Instance:{1}, Result:{2}, Return:{3}.",
                                        Path.GetFileName(tplLoc), inst.InstanceName, p.number, p.returnPort), LogLevel.Info);*/
                                #endregion
                            }
                            #region extra tests disabled
                            // Report an isolated instance for record-keeping.
                            //else reply.LogMessage(string.Format("Isolated instance found in TPL:{0}, Instance:{1}.",
                            //    Path.GetFileName(tplLoc), inst.InstanceName), LogLevel.Info);
                            #endregion
                        }
                    }
                }
            }
        }

        /// &lt;summary&gt;
        /// Execute is called for scripts after Verify has passed, here you should do actual modifications. 
        /// Only a log level of Abort will cause the remaining scripts not to be run.
        /// This is required by the Scripting Interface ScriptMethodBase.
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        /// &lt;returns&gt;Standard Cake Script Reply, containing logs and other data to the script engine. This will be returned to the executing user.&lt;/returns&gt;
        public override ScriptReply ExecuteScript(ScriptReply reply)
        {
            //The Try/Catch/Finally blocks wraps your content, while it's not required as CAKE will handle all errors, it's your opportunity 
            //to Catch and handle errors in your script
            try
            {
            }
            catch (Exception verifyEX)
            {
                //Log any exceptions and Stop returning that log to the calling function Do not remove
                reply.LogException(verifyEX, new StackFrame(1).GetMethod().ReflectedType.Name);
            }
            //Return Data to calling Script Engine Do not Remove
            return reply;
        }
    }
}

</Script>
</ScriptObject>
<ScriptObject>
  <Order>4</Order>
  <ScriptDisabled>1</ScriptDisabled>
  <Script>//css_dir C:\Program Files ((x86))\Cake;  /* Added automatically (please do not modify) */
//css_dir C:\Program Files\Cake;  /* Added automatically (please do not modify) */
using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using Cake.Scripting;
using Cake.Scripting.OTPL;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Scripting
{
    //Update the ClassName to your specific Script Name
    public class FuseConfigTRC : ScriptMethodBase
    {

        [STAThread]
        static public void Main(string[] args)
        {
            //When Doing Debug Set to a location containing what you are debugging
            currentFolder = @"I:\program\1274\eng\hdmtprogs\dg1_sds\kkutty\FullTP\7";

            //Main is called during debug in visual studio as the entry point to the script, it is not required in the final Script
            ScriptReply reply = new FuseConfigTRC //Update this to Match Script Name above for debug in VS
            {
                //Define any GlobalConfigurations using collection initializer here when running the script from Visual Studio
                //These Globals can be accessed in Veriy or execute by calling GlobalConfig.GetValuebyKey("KEY","DefaultValue");
                //WARNING: Do Not Edit the following line or add additional lines before the closing line (They will be removed), change the Global Variables and Edit the script again to update
                GlobalConfig = new GlobalConfigs() {{@"MinVersion", @"1.3.4.338"}, {@"Socket", @"SDS"}, {@"Product", @"MTL_GCD64"}, {@"STPLFILE", @"SubTestPlan_MTL_GCD64_SDS.stpl"}, {@"TPLFILE", @"BaseTestPlan.tpl"}, {@"ENVFILE", @"EnvironmentFile_!ENG!.env"}, {@"SOCKET", @"Default"}, {@"TOSVERSION", @"Default"}, {@"Subfamily", @"Default"}, {@"BOMGROUP", @"Default"}, {@"PROGRAMTYPE", @"Default"}, {@"BASETPNAME", @"Default"}, {@"DEVICE", @"Default"}, {@"FuseRootDir", @"Default"}, {@"PLISTFILE", @"PLIST_ALL.xml"}, {@"DEDC", @"Default"}, {@"SOCKETFILE", @"SDX_MTL_GCD64_X1.soc"}}
            }.Run(true); //Call Script Verify and then Execute via the .Run Extension Method
        }

        //When the Hosting script pulls the description this is what it is pulling. This is required by the Scripting Interface ScriptMethodBase
        public override string Description { get { return "BlankScript used as a starting point only"; } }

        //Get the Executing Folder from the Environment. This should be the Test Program Path and used in ExecuteScript and ExecuteVerify
        static string currentFolder = Environment.CurrentDirectory;

        /// &lt;summary&gt;
        /// Verify is called first, for all scripts in the CAKE file. You should add all checks that you want this script to perform here
        /// take special care to catch errors and continue the checks so you return all of the errors here in verify.
        /// Logging of any Errors, Aborts, or Exceptions will cause the script host not to call the ExecuteScript Method
        /// This is required by the Scripting Interface ScriptMethodBase
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        /// &lt;returns&gt;Standard Cake Script Reply, containing logs and other data to the script engine. This will be returned to the CAKE Host and added to the other Script Replies&lt;/returns&gt;
        public override ScriptReply ExecuteVerify(ScriptReply reply)
        {
            //The Try/Catch/Finally blocks wraps your content, while it's not required as CAKE will handle all errors, it's your opportunity 
            //to Catch and handle errors in your script
            try
            {
                //GlobalConfig = new GlobalConfigs() { { @"MinVersion", @"1.3.4.294" }, { @"Socket", @"SDS" }, { @"Product", @"DG1" }, { @"STPLFILE", @"SubTestPlan_DG1_A0_ES0.stpl" }, { @"TPLFILE", @"BaseTestPlan.tpl" }, { @"ENVFILE", @"EnvironmentFile_!ENG!.env" }, { @"SOCKET", @"SDX_DG1_X1_1.soc" }, { @"TOSVERSION", @"Default" }, { @"Subfamily", @"Default" }, { @"BOMGROUP", @"Default" }, { @"PROGRAMTYPE", @"Default" }, { @"BASETPNAME", @"Default" }, { @"DEVICE", @"Default" }, { @"FuseRootDir", @"Default" }, { @"PLISTFILE", @"PLIST_ALL.xml" }, { @"DEDC", @"Default" }, { @"SOCKETFILE", @"SDX_DG1_X1_1.soc" } }
                string tpl = Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("TPLFILE"));
                string stpl = Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("STPLFILE"));
                string env = Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("ENVFILE"));
                string plist = Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("PLISTFILE"));
                string soc = Path.Combine(currentFolder, GlobalConfig.GetValuebyKey("SOCKETFILE"));

                //Be sure to pass in the full path to the file in the LoadTP Call
                GlobalSuite.LoadTP(ref reply, tpl, stpl, env, plist, soc);
                //CheckFuseConfigSyntax(ref reply);
                CheckForRatio4Usage(ref reply);
            }
            catch (Exception verifyEX)
            {
                //Log any exceptions and Stop returning that log to the calling function Do not remove
                reply.LogException(verifyEX, new StackFrame(1).GetMethod().ReflectedType.Name);
            }
            //Return Data to calling Script Engine Do not Remove
            return reply;
        }

        private static void CheckFuseConfigSyntax(ref ScriptReply reply)
        {
            // For each of the modules in the TP..
            foreach (string tplFile in Pantry.getSTPL().tplList)
            {
                TPL mtpl = Pantry.getTPL(tplFile);

                // For each of iCFuseConfigExeTest instance found in the module..
                foreach (Test test in mtpl.m_tests.FindAll(x =&gt; x.template.Equals("iCFuseConfigExeTest")))
                {
                    // If the instance contains patlist_to_modify parameter..
                    if (test.ContainsParameter("patlist_to_modify"))
                    {
                        string value = test.parameters["patlist_to_modify"];
                        // Remove the " padding
                        if (value.StartsWith("\""))
                            value = value.Substring(1, value.Length - 1);
                        if (value.EndsWith("\""))
                            value = value.Substring(0, value.Length - 1);

                        // Split the string into individual fuseconfig regex arguments.
                        string[] tokens = value.Split('#');
                        for(int i = 0;i&lt;tokens.Length;i++)
                        {
                            // If this isn't the first fuseconfig regex argument, then it should start with a space.
                            if(i!=0 &amp;&amp; !tokens[i].StartsWith(" "))
                            {
                                reply.LogMessage("In Module: " + mtpl.ModuleName + ", FuseConfigExeTest Intance: " + test.name + ", Value: " + value + " missing a space prior to config regex: " + tokens[i], LogLevel.Error);
                            }

                            // If any of the fuseconfig regex arguments doesn't contain a space between open-close brackets, then error.  This is a requirement.
                            if(tokens[i].Contains("]["))
                                reply.LogMessage("In Module: " + mtpl.ModuleName + ", FuseConfigExeTest Intance: " + test.name + ", Value: " + value + " you cannot have '][' without a space in between.", LogLevel.Error);
                        }
                    }
                    // If the instance does not contain a patlist_to_modify, wth!?  No soup for you!
                    else reply.LogMessage("FuseConfigExeTest needs a \"patlist_to_modify\" parameter in test: " + test.name + ", TPL: " + tplFile, LogLevel.Error);
                }
            }
        }

        private static void CheckForRatio4Usage(ref ScriptReply reply)
        {
            // For each of the modules in the TP..
            foreach (string tplFile in Pantry.getSTPL().tplList)
            {
                TPL mtpl = Pantry.getTPL(tplFile);

                // For each of iCFuseConfigExeTest instance found in the module..
                foreach (Test test in mtpl.m_tests.FindAll(x =&gt; x.template.Equals("iCFuseConfigExeTest")))
                {
                    if (test.ContainsParameter("bypass_global") &amp;&amp; test.parameters["bypass_global"].Equals("\"1\""))
                        continue;
                    // If the instance contains patlist_to_modify parameter..
                    else if (test.ContainsParameter("patlist_to_modify"))
                    {
                        // Ensure that the parameter is set to RATIO:4, or else error.
                        string value = test.parameters["patlist_to_modify"];
                        if (value.StartsWith("\""))
                            value = value.Substring(1, value.Length - 1);
                        if (value.EndsWith("\""))
                            value = value.Substring(0, value.Length - 1);
                        string[] tokens = value.Split('#');
                        foreach (string str in tokens)
                        {
                            if (!str.EndsWith(" [RATIO:4]"))
                                reply.LogMessage("Missing \" [RATIO:4]\" declaration in some or all fuseconfig strings in Module: " + mtpl.ModuleName + ", FuseConfigExeTest Intance: " + test.name + ", Value: " + value, LogLevel.Error);
                        }
                    }
                    // If the instance does not contain a patlist_to_modify, wth!?  No soup for you!
                    else reply.LogMessage("FuseConfigExeTest needs a \"patlist_to_modify\" parameter in test: " + test.name + ", TPL: " + tplFile, LogLevel.Error);
                }
            }
        }

        /// &lt;summary&gt;
        /// Execute is called for scripts after Verify has passed, here you should do actual modifications. 
        /// Only a log level of Abort will cause the remaining scripts not to be run.
        /// This is required by the Scripting Interface ScriptMethodBase.
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        /// &lt;returns&gt;Standard Cake Script Reply, containing logs and other data to the script engine. This will be returned to the executing user.&lt;/returns&gt;
        public override ScriptReply ExecuteScript(ScriptReply reply)
        {
            //The Try/Catch/Finally blocks wraps your content, while it's not required as CAKE will handle all errors, it's your opportunity 
            //to Catch and handle errors in your script
            try
            {
                //This can be an Info Message, it's warning by default for new scripts
                reply.LogMessage("This script does not implement Execute", LogLevel.Warn);
            }
            catch (Exception verifyEX)
            {
                //Log any exceptions and Stop returning that log to the calling function Do not remove
                reply.LogException(verifyEX, new StackFrame(1).GetMethod().ReflectedType.Name);
            }
            //Return Data to calling Script Engine Do not Remove
            return reply;
        }
    }
}
</Script>
</ScriptObject>
<ScriptObject>
  <Order>5</Order>
  <ScriptDisabled>0</ScriptDisabled>
  <Script>//css_dir C:\Program Files ((x86))\Cake;  /* Added automatically (please do not modify) */
//css_dir C:\Program Files\Cake;  /* Added automatically (please do not modify) */
using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using Cake.Scripting;
using Cake.Scripting.OTPL;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Scripting
{
    //Update the ClassName to your specific Script Name
    public class HDMT_Pindef_Update: ScriptMethodBase
    {

        [STAThread]
        static public void Main(string[] args)
        {
            //When Doing Debug Set to a location containing what you are debugging
            currentFolder = @"c:\INT3_Cake";

            //Main is called during debug in visual studio as the entry point to the script, it is not required in the final Script
            ScriptReply reply = new HDMT_Pindef_Update //Update this to Match Script Name above for debug in VS
            {
                //Define any GlobalConfigurations using collection initializer here when running the script from Visual Studio
                //These Globals can be accessed in Veriy or execute by calling GlobalConfig.GetValuebyKey("KEY","DefaultValue");
                //WARNING: Do Not Edit the following line or add additional lines before the closing line (They will be removed), change the Global Variables and Edit the script again to update
                GlobalConfig = new GlobalConfigs() {{@"Socket", @"SDS"}, {@"Product", @"MTL_GCD64"}, {@"STPLFILE", @"Default"}, {@"TPLFILE", @"Default"}, {@"ENVFILE", @"Default"}, {@"TosVersion", @"Default"}, {@"Subfamily", @"Default"}, {@"Bomgroup", @"Default"}, {@"ProgramType", @"Default"}, {@"BaseTPName", @"Default"}, {@"Device", @"Default"}, {@"FuseRootDir", @"Default"}, {@"PLISTFILE", @"Default"}, {@"MinVersion", @"1.3.4.298"}, {@"Dedc", @"Default"}, {@"SOCKETFILE", @"Default"}}
            }.Run(true); //Call Script Verify and then Execute via the .Run Extension Method
        }

        //When the Hosting script pulls the description this is what it is pulling. This is required by the Scripting Interface ScriptMethodBase
        public override string Description { get { return "BlankScript used as a starting point only"; } }

        //Get the Executing Folder from the Environment. This should be the Test Program Path and used in ExecuteScript and ExecuteVerify
        static string currentFolder = Environment.CurrentDirectory;

        /// &lt;summary&gt;
        /// Verify is called first, for all scripts in the CAKE file. You should add all checks that you want this script to perform here
        /// take special care to catch errors and continue the checks so you return all of the errors here in verify.
        /// Logging of any Errors, Aborts, or Exceptions will cause the script host not to call the ExecuteScript Method
        /// This is required by the Scripting Interface ScriptMethodBase
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        /// &lt;returns&gt;Standard Cake Script Reply, containing logs and other data to the script engine. This will be returned to the CAKE Host and added to the other Script Replies&lt;/returns&gt;

        public override ScriptReply ExecuteScript(ScriptReply reply)
        {
            //The Try/Catch/Finally blocks wraps your content, while it's not required as CAKE will handle all errors, it's your opportunity 
            //to Catch and handle errors in your script
            try
            {
                //string socfile = string.Empty;
                //string[] socfiles = Directory.GetFiles(currentFolder, "*.soc");
                //string[] pinfiles = Directory.GetFiles(currentFolder, "*.pin");
                //string[] soclines;
                //string[] pinlines;
				
				//string line = null;
				//int line_number = 0;
				//int line_to_delete = 18;

				//using (StreamReader reader = new StreamReader(currentFolder + "\\BinMatrix.xml")) 
				//{
				//	using (StreamWriter writer = new StreamWriter(currentFolder + "\\BinMatrix_new.xml")) {
				//		while ((line = reader.ReadLine()) != null) {
				//			line_number++;

				//			if (line_number == line_to_delete)
				//				continue;

				//			writer.WriteLine(line);
				//		}
				//	}
				//}
				
				//if (File.Exists(currentFolder + "\\BinMatrix_new.xml"))
				//{
				//	File.Delete(currentFolder + "\\BinMatrix.xml");
				//	File.Move(currentFolder + "\\BinMatrix_new.xml", currentFolder + "\\BinMatrix.xml");
				//	reply.LogMessage("BinMatrix Updated ", LogLevel.Info);
				//}
				
                //This can be an Info Message, it's warning by default for new scripts
                if (File.Exists(currentFolder + "\\SubTestPlan_MTL_GCD64_SDS.stpl"))
                {
                    File.Move(currentFolder + "\\SubTestPlan_MTL_GCD64_SDS.stpl", currentFolder + "\\SubTestPlan_MTL_GCD64_SDS.stpl");
                    reply.LogMessage("STPL remamed to SubTestPlan_MTL_GCD64_SDS.stpl ", LogLevel.Info);
                }

            }
            catch (Exception verifyEX)
            {
                //Log any exceptions and Stop returning that log to the calling function Do not remove
                reply.LogException(verifyEX, new StackFrame(1).GetMethod().ReflectedType.Name);
            }
            //Return Data to calling Script Engine Do not Remove
            return reply;
        }
    }
}
</Script>
</ScriptObject>
<ScriptObject>
  <Order>6</Order>
  <ScriptDisabled>0</ScriptDisabled>
  <Script>//css_dir C:\Program Files\Cake;
//css_dir C:\Program Files (x86)\Cake;
using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using Cake.Scripting;
using Cake.Scripting.OTPL;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;


namespace Scripting
{
    //Update the ClassName to your specific Script Name
    public class Update_BDEFS : ScriptMethodBase
    {

        [STAThread]
        static public void Main(string[] args)
        {
            //When Doing Debug Set to a location containing what you are debugging
            currentFolder = @"c:\INT3_Cake";

            //Main is called during debug in visual studio as the entry point to the script, it is not required in the final Script
            ScriptReply reply = new Update_BDEFS //Update this to Match Script Name above for debug in VS
            {
                //Define any GlobalConfigurations using collection initializer here when running the script from Visual Studio
                //These Globals can be accessed in Veriy or execute by calling GlobalConfig.GetValuebyKey("KEY","DefaultValue");
                //WARNING: Do Not Edit the following line or add additional lines before the closing line (They will be removed), change the Global Variables and Edit the script again to update
                GlobalConfig = new GlobalConfigs() {{@"Socket", @"SDS"}, {@"Product", @"ADL"}, {@"STPLFILE", @"Default"}, {@"TPLFILE", @"Default"}, {@"ENVFILE", @"Default"}, {@"TosVersion", @"Default"}, {@"Subfamily", @"Default"}, {@"Bomgroup", @"Default"}, {@"ProgramType", @"Default"}, {@"BaseTPName", @"Default"}, {@"Device", @"Default"}, {@"FuseRootDir", @"Default"}, {@"PLISTFILE", @"Default"}, {@"MinVersion", @"1.3.4.298"}, {@"Dedc", @"Default"}, {@"SOCKETFILE", @"Default"}}
            }.Run(true); //Call Script Verify and then Execute via the .Run Extension Method
        }

        //When the Hosting script pulls the description this is what it is pulling. This is required by the Scripting Interface ScriptMethodBase
        public override string Description { get { return "Update_BDEFS used as a starting point only"; } }

        //Get the Executing Folder from the Environment. This should be the Test Program Path and used in ExecuteScript and ExecuteVerify
        static string currentFolder = Environment.CurrentDirectory;

        /// &lt;summary&gt;
        /// Regex that identified a blank line
        /// &lt;/summary&gt;
        protected static readonly Regex regBlankLine = new Regex(@"^\s*$", RegexOptions.Compiled);

        /// &lt;summary&gt;
        /// Regex that identifies lines that are commented out
        /// &lt;/summary&gt;
        protected static readonly Regex regIsComment = new Regex(@"^\s*#", RegexOptions.Compiled);

        /// /// &lt;summary&gt;
        /// Regex that identifies Header Line 
        /// Fbin,IAConfig,GTConfig,EURecovery,DefectRepair,VminRepair,ECC
        /// &lt;/summary&gt;
        protected static readonly Regex regIsheader = new Regex(@"^Fbin,", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// &lt;summary&gt;
        /// Verify is called first, for all scripts in the CAKE file. You should add all checks that you want this script to perform here
        /// take special care to catch errors and continue the checks so you return all of the errors here in verify.
        /// Logging of any Errors, Aborts, or Exceptions will cause the script host not to call the ExecuteScript Method
        /// This is required by the Scripting Interface ScriptMethodBase
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        /// &lt;returns&gt;Standard Cake Script Reply, containing logs and other data to the script engine. This will be returned to the CAKE Host and added to the other Script Replies&lt;/returns&gt;
        public override ScriptReply ExecuteVerify(ScriptReply reply)
        {

            //The Try/Catch/Finally blocks wraps your content, while it's not required as CAKE will handle all errors, it's your opportunity 
            //to Catch and handle errors in your script
            try
            {
                string bdefsFile = FileUtilities.AskUserAboutFiles(ref reply, currentFolder, @"[.]bdefs$", "", SearchOption.TopDirectoryOnly, false);// Path.Combine(currentFolder, "BinDefinitions.bdefs");

                // Read in .bdefs file and Edit as Necessary
                using (var bdefs = new Cake.Scripting.OTPL.BDEFS(ref reply, bdefsFile))
                {

                    //Fix GoodDie
   
                    // Scrub the Softbins Foreach softbin create a 4 digit Bin 
                    // and add if it does not exist. The Binner is going to require 
                    // all of these to exist as we will be resetting to a 4 Digit bin not a
                    // 8 digit bin
                    foreach (var key in bdefs.CurrentSoftBins())
                    {
                        //Ignore if SoftBin is not 8 digits
                        if (key &lt; 10000) continue;

                        var bin = bdefs.GetSoftBin(key); //Note: Softbin will exist in this syntax

                        //Skip Pass Bins
                        if (bin.Bin &lt; 90000000) continue;

                        //Get the new 4 digit and create it if it does not already exist
                        var softbin = int.Parse(bin.Bin.ToString().Substring(4));
                        var name = Regex.Replace(bin.Name, "^b(....)(....)(.*)", "b" + softbin.ToString() + "$3");

                        bdefs.AddSoftBins(softbin, name, name);
                    }
  
                    //Change SpeedBins to Fail
                    var speedbins = GetSpeedBins(Path.Combine(currentFolder, "BinMatrix.xml"));
                    foreach (var spBin in speedbins)
                    {
                        BDEFSBin hb = bdefs.GetHardBin(int.Parse(spBin));
                        if (hb.Bin == -999) continue;
                        string hbName = hb.Name;
                        hb.passfail = BinPassFail.Fail;
                        //Update the SoftBin that refers to this hardBin
                        bdefs.UpdateSoftBinReferencetoHardBin(hbName, hb.Name);
                    }

                    //Generate bins needed for BMFC good die binning
                    for (int ib = 1; ib &lt; 7; ib++)
                    {
                        for (int i = 0; i &lt; 100; i++)
                        {
                            int sb = 10000000 + ib * 10000 + i*100;
                            string name = "b" + sb.ToString();
                            AddSoftBins(bdefs, ib, sb, name, "BMFC good die placeholder");
                        }
                    }


                    // Write new .bdefs file
                    bdefs.Save(true);
                }
            }
            catch (Exception verifyEX)
            {
                //Log any exceptions and Stop returning that log to the calling function Do not remove
                reply.LogException(verifyEX, new StackFrame(1).GetMethod().ReflectedType.Name);
            }
            //Return Data to calling Script Engine Do not Remove
            return reply;
        }

        public void AddSoftBins(BDEFS bdef, int hardbin, int softbin, string name, string desc)
        {
            if (!bdef.SoftBins.ContainsKey(softbin))
            {
                var newBin = new BDEFSBin(name, softbin, desc, bdef.HardBins[hardbin].Name, BinPassFail.Pass);
                bdef.SoftBins.Add(softbin, newBin);
            }
        }

        private static void ManuallyAddB26_SoftBin(ScriptReply reply, string name, int newSBNumber, string hardBin, BDEFS bdefs)
        {
            BDEFSBin newSB = new BDEFSBin(name, newSBNumber, name, hardBin, BinPassFail.Undefined);

            if (!bdefs.SoftBinExists(newSBNumber))
            {
                bdefs.SoftBins.Add(newSBNumber, newSB);
            }
            else
            {
                reply.LogMessage(String.Format("Attempted to add a SoftBin that already exists - {0}", newSBNumber),
                    LogLevel.Info);
            }
        }

        private static List&lt;string&gt; GetSpeedBins(string binmatrixFile)
        {
            List&lt;string&gt; speedbins = new List&lt;string&gt;();

            //Update BDEFS File, get the Bin's from the Bin Matrix XML File
            XDocument binmatrix = null;
            try
            {
                binmatrix = XDocument.Load(Path.Combine(currentFolder, binmatrixFile));
            }
            catch (Exception xex)
            {
                //Bail out here as we must/expect a BinMatrix File
                throw new Exception(string.Format("Unable to load BinMatrix in file {0} with error - {1}", binmatrixFile,
                    xex.Message));
            }

            //SORT_SKL22
            if ((binmatrix != null) &amp;&amp; (binmatrix.Elements().Count() &gt; 0))
            {
                //Get the flow elements from the SpeedBins from the XML Xpath
                var speedflowitems = binmatrix.GetXPathElements("/BinMatrix/BOMGroupTable/BOMGroup/ActiveFlowList/Flow");
                if ((speedflowitems != null) &amp;&amp; (speedflowitems.Count() &gt; 0))
                {
                    speedbins.AddRange(speedflowitems.Select(entity =&gt; entity.Attribute("bin").Value));
                }
            }
            else
            {
                throw new Exception(string.Format("BinMatrix was empty in file - {0}", binmatrixFile));
            }

            return speedbins;
        }

        /// &lt;summary&gt;
        /// Execute is called for scripts after Verify has passed, here you should do actual modifications. 
        /// Only a log level of Abort will cause the remaining scripts not to be run.
        /// This is required by the Scripting Interface ScriptMethodBase.
        /// &lt;/summary&gt;
        /// &lt;param name="reply"&gt;ScriptHost will pass in a reply object containing information/logs about the script user,version, etc etc&lt;/param&gt;
        /// &lt;returns&gt;Standard Cake Script Reply, containing logs and other data to the script engine. This will be returned to the executing user.&lt;/returns&gt;
        public override ScriptReply ExecuteScript(ScriptReply reply)
        {
            //The Try/Catch/Finally blocks wraps your content, while it's not required as CAKE will handle all errors, it's your opportunity 
            //to Catch and handle errors in your script
            try
            {
                //This can be an Info Message, it's warning by default for new scripts
                //reply.LogMessage("This script does not implement Execute", LogLevel.Warn);
            }
            catch (Exception verifyEX)
            {
                //Log any exceptions and Stop returning that log to the calling function Do not remove
                reply.LogException(verifyEX, new StackFrame(1).GetMethod().ReflectedType.Name);
            }
            //Return Data to calling Script Engine Do not Remove
            return reply;
        }
    }
}
</Script>
</ScriptObject>
</Cake>
