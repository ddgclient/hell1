"""
pll_vid_walk.py

This is a python script that handles finding vmins along multiple curves for multiple PLLs
It then handles flow control to run those PLLs at their various ratios and vmins.

!! Create documentation for validation requirements
!! Figure out how MV should work for VID WALK - figure out a way to ensure that production mode can't get the default values. -- not done.
!! For server, there are cases where VMIN that is not stored in FAST due to one failing core. There might need some enhancement to calculate the maximum for the cores. -- not done.
!! Include more than just Locktime check for pass/fail. -- not done.
!! sItuffName should reflect the naming convention, should use the instance name. -- done. put the sItuffName in datalogResults only, and pulled the current test name
!! Default Frequency - should run all ratios at nom -- done. Changed generate VMIN to reflect this.
!! Print out ratios on every execution -- done. Added datalogResults that prints all ratios for each PLL ('_' delimited)
!! Force executing all ratios, but only killing on certain ratios -- done. Added logic to execute every voltage while printing a different HRY result based on whether in or out of VID+GB 
!! Add a check for the FAST domain to exit port 0 if the domain doesn't exist in the FAST Infra file. -- not done.
"""

"""Imports"""
import sys
import evg
#from evg import evg

"""Constants that we will use to manage the loops"""
bDebugEnabled = False
sItuffHeader1 = '0_tname_'											#MIDAS only datalog (not applicable to sort TP)
sItuffHeader2 = '0_strgval_'										#Using strgval for ituff
sItuffHeader3 = '0_lsep\n'											#technically not needed, but whatever.
fDefaultFrequency = 1.0												#Default Frequency = ratio 10 (in GHz) !!
fDefaultVoltage = 1.0												#Default Voltage = VNOM
sVminGSDS = "VID_WALK_VMIN"											#This is GSDS which contains VMIN value for CMEM decode
sUPSGSDS = "FAST_UPSVFPASSFLOW"										#This is EVG FAST template GSDS which contains UPS values
sFIVRregex = "tgl_pre"												#This matches the FuseConfig regex used by FIVR to apply VDAC  <pattern_partial_name>tgl_pre</pattern_partial_name>
sUserVarExecuteAllPoints = "CLK_ADPLL_ALL.VIDWK_EXECUTE_ALL_POINTS" #Might need to delete this...
fFASTFrequencyModifier = 1000000000									#EVG FAST template stores it's frequencies in GHz
sGBVarsPHMSwitch = "GBVars.PHM_GB_switch"

"""Initialization of Primary Dictionary"""
dPLLInfo = {}						 

"""Class structure that is used to maintain all info"""
class pllClass():
	def __init__(cSelf,lFields):	#Put input file fields into correct slot & initialize datalog structures
		cSelf.sPLLName			= str(lFields[0])
		cSelf.lRatioList		= lFields[1].split('|')
		cSelf.lRatios			= [int(sRatio) for sRatio in cSelf.lRatioList]
		cSelf.sVIDCorner		= str(lFields[2])
		cSelf.lTestname			= lFields[3].split('?')
		cSelf.iBCLKRef			= int(lFields[4])
		cSelf.fMaxVoltage		= float(lFields[5])
		cSelf.fMinVoltage		= float(lFields[6])
		cSelf.fMaxFrequency		= float(lFields[7])
		cSelf.fMinFrequency		= float(lFields[8])
		cSelf.lSubPLL			= lFields[9].split('|')
		cSelf.sGSDS				= str(lFields[10])
		cSelf.sIPGBVar			= str(lFields[11])
		cSelf.sAppType			= str(lFields[12])	#There are 2 possible values for Application Type: "FIVR" or "DIRECT_HW"
		if cSelf.sAppType == "FIVR":				   #fivr complicates things
			cSelf.sPList		= str(lFields[13])     #string matching for patmodding vidwalk plists 
			cSelf.dSubDomain = {}
			for subdomain in lFields[14].split('|'):   #There might be multiple fuseconfig setpoints to write to... 
				dTemp = {}
				lTemp = subdomain.split('^')		   #Split the setpoint from the fuse(s) that are needed
				sKey = lTemp[0]						   #The key matches the FuseConfig Setpoint defined by FIVR team to control VDAC  <fuse_register_name>VDAC</fuse_register_name>
				dTemp['expr'] = int(lTemp[1])          #This matches FIVR analog->digital conversion expression  <fivr_initial_voltage_expression>VINPUT[]*256</fivr_initial_voltage_expression>
				dTemp['fuses'] = lTemp[2].split('%')   #This will be a list of all fuses to write to if any
				cSelf.dSubDomain[sKey] = dTemp
		
		#Initialize resultant lists/dicts used later on
		cSelf.lVmins			= list()
		cSelf.lResult			= list()
		cSelf.sUPSVF			= str()
		cSelf.dLocktime			= dict((sSubPLL,list()) for sSubPLL in cSelf.lSubPLL)

	def printDebug(cSelf):			  #Print to console all file inputs
		evg.PrintToConsole("PLLName = "		 + str(cSelf.sPLLName)		+ "\n")
		evg.PrintToConsole("Ratios = "		 + str(cSelf.lRatios)		+ "\n")
		evg.PrintToConsole("VIDCorner = "	 + str(cSelf.sVIDCorner)	+ "\n")
		evg.PrintToConsole("Testname = "	 + str(cSelf.lTestname)		+ "\n")
		evg.PrintToConsole("BCLKRef = "		 + str(cSelf.iBCLKRef)		+ "\n")
		evg.PrintToConsole("MaxVoltage = "	 + str(cSelf.fMaxVoltage)	+ "\n")
		evg.PrintToConsole("MinVoltage = "	 + str(cSelf.fMinVoltage)	+ "\n")
		evg.PrintToConsole("MaxFrequency = " + str(cSelf.fMaxFrequency) + "\n")
		evg.PrintToConsole("MinFrequency = " + str(cSelf.fMinFrequency) + "\n")
		evg.PrintToConsole("SubPLL = "		 + str(cSelf.lSubPLL)		+ "\n")
		evg.PrintToConsole("GSDS = "		 + str(cSelf.sGSDS)			+ "\n")		   
		evg.PrintToConsole("IP GBVar = "	 + str(cSelf.sIPGBVar)		+ "\n")		   
		evg.PrintToConsole("AppType = "		 + str(cSelf.sAppType)		+ "\n")				
		if cSelf.sAppType == "FIVR":
			evg.PrintToConsole("PList = "		  + str(cSelf.sPList)		+ "\n")		   
#			evg.PrintToConsole("FIVRexpression = "	  + str(cSelf.iFIVRexpression)	+ "\n")		   
			for setpoint in cSelf.dSubDomain:
				evg.PrintToConsole("Setpoint "+str(setpoint)+" = "+str(cSelf.dSubDomain[setpoint])+"\n")
		evg.PrintToConsole("\n"											+ "\n")		   

def getDebugMode():

	try:		
		if (evg.GetTestParam(evg.GetInstanceName(),"debug_mode") == "DISABLED"): return False 
		else: return True
	except:
		evg.PrintToConsole("Could not get debug_mode from instance parameters, defaulting to DISABLED.\n")		  
		return False		

"""INIT: pass in input file location, and init() will parse the file and create the corresponding data structures"""
def init():

	global dPLLInfo											#Need to save to the dictionary
	bDebugEnabled = getDebugMode()							#boolean value to create debug_mode flag
	sArgs = evg.GetUFArgument()								   
	lArgs = sArgs.split()
	sFile = lArgs[1] 
	pFile = open(evg.GetFile(sFile), 'r')					 
	for sLine in pFile.readlines():							#read each line in input file
		lFields = sLine.strip().split(',')					#strip each line of whitespace characters and split by comma (CSV)
		lFields = [sField.strip() for sField in lFields]	#make sure that each item in list doesn't have whitespace characters
		if(lFields[0] and lFields[0][0] != "#"):			#Do not iterate over lines that start with #
			cNewPLL = pllClass(lFields)						#push new list into the class
			dPLLInfo[cNewPLL.sPLLName] = cNewPLL			#add new class to dictionary with the key of the PLLName
			
	for sPLL in dPLLInfo:
		if bDebugEnabled: dPLLInfo[sPLL].printDebug()

"""VIDWALK: pass in the corresponding key for data structure, and script will generate functional vmins, execute the corresponding instances, and datalog the results"""
def vidWalk():

	evg.SetSitePort(0,0)									#we default the site port to 0, since executeVID should reset this to 1 or 2 if executed correctly. Port 0 = bin90
	global bDebugEnabled									#we need to assign to both these globals
	global dPLLInfo											   
	bDebugEnabled = getDebugMode()							#vidWalk will be in a different instance than init, so read it's debugMode as well
	
	sArgs = evg.GetUFArgument()
	lArgs = sArgs.split()
	sPLL = lArgs[1]											#2nd Argument must match one of the PLLNames from the input file. We only execute this flow per PLL, not all at once.
#	 sPLL = "GTPLL"

	if bDebugEnabled: evg.PrintToConsole("ARGS="+str(lArgs)+"\n")

	dPLLInfo[sPLL].fMaxVoltage	   = float(lArgs[2].strip('V').strip('v')) #3rd Argument must define the Max voltage per the Bin/Flow Matrix
	dPLLInfo[sPLL].fMinVoltage	   = float(lArgs[3].strip('V').strip('v')) #4th Argument must define the Max voltage per the Bin/Flow Matrix

	if bDebugEnabled: dPLLInfo[sPLL].printDebug()
	
	dPLLInfo[sPLL].sUPSVF	 = str()
	dPLLInfo[sPLL].lVmins	 = list()						#clear out our lists for VMINs, Results, UPSVF, and Locktime
	dPLLInfo[sPLL].lResult	 = list()
	dPLLInfo[sPLL].dLocktime = dict((sSubPLL,list()) for sSubPLL in dPLLInfo[sPLL].lSubPLL)
	
	iUPSMinRatio,iUPSMaxRatio = generateVmins(sPLL)			#1st we generate the Functional VMIN's according to the VID values
	iExitPort = executeVID(iUPSMinRatio,iUPSMaxRatio,sPLL)	#2nd we execute the PLL's Locktime instances at the correlating VMIN value per ratio
	datalogResults(iExitPort,sPLL)							#3rd we datalog to ituff all pertinent data per ratio, but all on 1 line (the VMIN's we generated, the raw pass/fail, and the locktime from the CMEM decode instance)

	evg.SetSitePort(0,iExitPort)							#this overwrites the "0" that was assigned earlier
	
def generateVmins(sPLL):

	lPoints = []
	lResults = []
	iUPSMinRatio = int(dPLLInfo[sPLL].fMaxFrequency)/dPLLInfo[sPLL].iBCLKRef #we snap the MIN to the highest possible value so that we can compare and get the real minimum point
	iUPSMaxRatio = int(dPLLInfo[sPLL].fMinFrequency)/dPLLInfo[sPLL].iBCLKRef #same as above but for MAX
	
	try:																				  #pull the FAST UPSVF GSDS 
		sUPSVF = evg.GetGSDSData(sUPSGSDS,"string","UNT",-99,0)							  #FAST Standard Format >> domain1:freq1^vmin1%freq2^vmin2_domain2:freq3^vmin3 ***Frequency in GHz
	except:																				  #use a default if GSDS exception
		sItuffName = evg.GetInstanceName()												  #This will be ITUFF token used to datalog
		evg.PrintToItuff( sItuffHeader1 + sItuffName + '_' + sPLL	+ '_UPSVF'	  + "\n" + sItuffHeader2 + "-9999" + "\n" + sItuffHeader3)
		sUPSVF = dPLLInfo[sPLL].sVIDCorner + ":" + str(0.4) + "^" + str(fDefaultVoltage) + "%" + str(3.7) + "^" + str(fDefaultVoltage)	  #set the point to the default value if FAST wasn't executed
#		sUPSVF = "CR:4.900^1.356V1.399V1.286V1.365V-9999V-9999V-9999V-9999%4.200^1.036V1.075V1.034V1.059V-9999V-9999V-9999V-9999%3.400^0.843V0.866V0.827V0.835V-9999V-9999V-9999V-9999%2.200^0.643V0.658V0.636V0.636V-9999V-9999V-9999V-9999%1.200^0.510V0.530V0.510V0.510V-9999V-9999V-9999V-9999%0.400^0.300V0.300V0.300V0.300V-9999V-9999V-9999V-9999_CLR:4.300^1.115%3.600^0.900%3.000^0.809%1.800^0.625%0.800^0.514%0.400^0.470_CRX2:4.900^1.286V1.312V1.272V1.291V-9999V-9999V-9999V-9999%4.200^1.012V1.040V1.005V1.015V-9999V-9999V-9999V-9999%3.400^0.824V0.889V0.814V0.814V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_CRX3:4.800^1.262V1.296V1.240V1.288V-9999V-9999V-9999V-9999%4.200^1.038V1.070V1.022V1.032V-9999V-9999V-9999V-9999%3.400^0.834V0.889V0.824V0.824V-9999V-9999V-9999V-9999%2.200^0.648V0.693V0.648V0.648V-9999V-9999V-9999V-9999%1.200^0.527V0.558V0.521V0.521V-9999V-9999V-9999V-9999%0.400^0.430V0.450V0.420V0.420V-9999V-9999V-9999V-9999_GTS:1.350^0.911%1.100^0.771%0.900^0.700%0.600^0.630%0.300^0.560_SAQ:3.000^0.840%2.200^0.670%1.100^0.590_CRSSA:4.900^1.113V1.176V1.100V1.132V-9999V-9999V-9999V-9999%4.200^0.926V0.976V0.915V0.928V-9999V-9999V-9999V-9999%3.400^0.781V0.804V0.772V0.774V-9999V-9999V-9999V-9999%2.200^0.638V0.636V0.603V0.603V-9999V-9999V-9999V-9999%1.200^0.536V0.560V0.515V0.525V-9999V-9999V-9999V-9999%0.400^0.470V0.500V0.460V0.470V-9999V-9999V-9999V-9999_CLRSA:4.300^1.025%3.600^0.857%3.000^0.758%1.800^0.600%0.800^0.510%0.400^0.490_SAPS:1.000^0.750%0.200^0.580_SAIS:0.533^0.613%0.400^0.600%0.200^0.580_SAF:0.800^0.670%0.533^0.590_SACD:0.662^0.850%0.562^0.700%0.312^0.580_CDSSA:0.662^0.684%0.562^0.640%0.312^0.580_GTSM:1.350^1.158%1.100^0.922%0.900^0.790%0.600^0.660%0.300^0.540"
	if bDebugEnabled: evg.PrintToConsole("FASTVF="+sUPSVF+"\n")
	
	if sUPSVF.find(dPLLInfo[sPLL].sVIDCorner) < 0:										  #Error checking to make sure that we have a valid domain in the UPSVF string.
		sItuffName = evg.GetInstanceName()												  #This will be ITUFF token used to datalog
		evg.PrintToItuff( sItuffHeader1 + sItuffName + '_' + sPLL	+ '_UPSVF'	  + "\n" + sItuffHeader2 + "-9999" + "\n" + sItuffHeader3)
		sUPSVF = dPLLInfo[sPLL].sVIDCorner + ":" + str(0.4) + "^" + str(fDefaultVoltage) + "%" + str(3.7) + "^" + str(fDefaultVoltage)	  #set the point to the default value if FAST wasn't executed		

	for lDomains in sUPSVF.split('_'):													  #we need to split the FAST string by domain
		lDomains = lDomains.strip().split(':')											  #then split the domain from the data
		if dPLLInfo[sPLL].sVIDCorner == lDomains[0]: 
			dPLLInfo[sPLL].sUPSVF = lDomains[1]											  #milespac 04/25/2018 - optimization to print UPSVF if the script fails
			lPoints = lDomains[1].split('%')											  #then split the data by data point	
			
	lPoints = [sPoint.split('^') for sPoint in lPoints]									  #now split the frequency from the vmin
	for lPoint in lPoints:																  #milespac => 03/19/2019 adding support for per-core VMIN
		if 'v' in lPoint[1].lower():													  #<contains "v">:
			lTemp = lPoint[1].lower().split('v')										  #<split lPoint[1] like line 154>
			lTemp = [sTemp for sTemp in lTemp if float(sTemp) <= dPLLInfo[sPLL].fMaxVoltage and float(sTemp) >= dPLLInfo[sPLL].fMinVoltage] #<remove all items in list that arent inside limits>
			lPoint[1] = max(lTemp)														  #<save only the maximum voltage>
		
	lPoints = [[float(lPoint[0])*fFASTFrequencyModifier,float(lPoint[1])] for lPoint in lPoints]	#reformat the frequency and vmin into floats
	if bDebugEnabled: evg.PrintToConsole("Points="+str(lPoints)+"\n")
	lPoints.sort()																					  #sort the data points to guarantee that they are lowest->highest frequency
	if len(lPoints) > 1:	#calculate the slope and intercept between two data points. We only do the calculation if there are more than 1 data points, otherwise this code would exception.
		for index in range(len(lPoints)-1):
			fFrequencyLower,fVoltageLower = lPoints[index]							#since we ordered the points, the lower index has the lower frequency
			fFrequencyUpper,fVoltageUpper = lPoints[index+1]						#we need two points to do generate slope & intercept
			fRatioLower = fFrequencyLower/float(dPLLInfo[sPLL].iBCLKRef)			#we do this math with ratio instead of frequency since the input file uses ratio
			fRatioUpper = fFrequencyUpper/float(dPLLInfo[sPLL].iBCLKRef)			
			if (fRatioUpper - fRatioLower) != 0:									#milespac 04/25/2018 - bugfix for UPSVF strings with multiple points that have the same Ratio, i.e. F5 = F6 = 4.3GHz
				fSlope = (fVoltageUpper - fVoltageLower)/(fRatioUpper - fRatioLower)
				fIntercept = fVoltageUpper - (fSlope*fRatioUpper)
				dResult = {"Slope":fSlope,"Intercept":fIntercept,"RatioLower":int(round(fRatioLower)),"RatioUpper":int(round(fRatioUpper))}	   #we store the dResults in a dictionary to retrieve later
				lResults.append(dResult)												 #lResults is a list of all slope/intercept lines for a given ratio range. 
			else: 
				if bDebugEnabled: evg.PrintToConsole("Upper point "+str(lPoints[index+1])+" has the same ratio as the lower point "+str(lPoints[index]) +" \n")
				
	else:
		iLowerRatio = min(dPLLInfo[sPLL].lRatios)																	#we find the 1st frequency in the points list, and calculate the ratio
		iUpperRatio = max(dPLLInfo[sPLL].lRatios)																	#we find the 1st frequency in the points list, and calculate the ratio
		fSingleVoltage = float(lPoints[0][1])																		#we use the voltage as the intercept point (x axis = ratio, y axis = voltage)
		lResults = [{"Slope":0.0,"Intercept":fSingleVoltage,"RatioLower":iLowerRatio,"RatioUpper":iUpperRatio}]		#for 1 ratio, the slope is 0 and the intercept is equal to the voltage

	for dResult in lResults:																						#Compare the points for this result to the global min/max
		if iUPSMinRatio > dResult["RatioLower"]: iUPSMinRatio = dResult["RatioLower"]								#UPSMinRatio is the lowest supported frequency point (F1 for instance)
		if iUPSMaxRatio < dResult["RatioUpper"]: iUPSMaxRatio = dResult["RatioUpper"]								#UPSMaxRatio is the highest supported frequency point (F5 for instance)

	if bDebugEnabled: evg.PrintToConsole(sPLL+" Results:" + str(lResults) + "\n")	  

	dGBModifier = 0.0
#	 dGBModifier = (evg.GetTpGlobalValue(dPLLInfo[sPLL].sIPGBVar,"double")*evg.GetTpGlobalValue(sGBVarsPHMSwitch,"integer"))
	
	for ratio in dPLLInfo[sPLL].lRatios:	#use the slope & intercept to generate functional vmins only if a ratio is between two points
		fCalcVmin = 0																			 #initialize to 0 to wipe previous value
		for dResult in lResults:																 #if any of these are true, then we will use this slope/intercept line to calculate the vmin #milespac -> Added support for GBVars on every point!
			if (ratio >= dResult["RatioLower"] and ratio <= dResult["RatioUpper"]) or (ratio <= iUPSMinRatio and iUPSMinRatio == dResult["RatioLower"]) or (ratio >= iUPSMaxRatio and iUPSMaxRatio == dResult["RatioUpper"]): fCalcVmin = (dResult["Slope"]*ratio) + dResult["Intercept"] + dGBModifier
		if (fCalcVmin > dPLLInfo[sPLL].fMaxVoltage): fCalcVmin = dPLLInfo[sPLL].fMaxVoltage		 #if we are outside of the limits, snap to them
		if (fCalcVmin < dPLLInfo[sPLL].fMinVoltage): fCalcVmin = dPLLInfo[sPLL].fMinVoltage 
		dPLLInfo[sPLL].lVmins.append('%.3f' % fCalcVmin)										 #this is where we append the new calculated vmin to the datalogging structure.

	if bDebugEnabled: evg.PrintToConsole("VMIN's Generated for all ratios in " + sPLL+ "\n") 
	return iUPSMinRatio,iUPSMaxRatio															 #executeVID function needs both of these values for finding the range

def executeVID(iUPSMinRatio,iUPSMaxRatio,sPLL):

	iExitPort = 1	  #we initialize to exit port 1
 
#	 bFlag = 1
	bFlag = evg.GetTpGlobalValue(sUserVarExecuteAllPoints,"integer")
	for index in range(len(dPLLInfo[sPLL].lRatios)):														#loop through all ratios and only execute if the ratio is inside the range
		sInstanceName = (dPLLInfo[sPLL].lTestname[0] + sPLL + dPLLInfo[sPLL].lTestname[1] + "R" + str(dPLLInfo[sPLL].lRatios[index]).zfill(2) + dPLLInfo[sPLL].lTestname[2])			#This needs to match the locktime testnames in the TP
		if dPLLInfo[sPLL].lRatios[index] in range(iUPSMinRatio,int(float(iUPSMaxRatio * 1.08))+1):			#find if we are in the range of valid ratios
			if dPLLInfo[sPLL].sAppType == "DIRECT_HW":
				evg.SetGSDSDblData(sVminGSDS,float(dPLLInfo[sPLL].lVmins[index]),"UNT",-99,0,1)					   #this value is the vmin for this ratio
			elif dPLLInfo[sPLL].sAppType == "FIVR":
				for setpoint in dPLLInfo[sPLL].dSubDomain:																					 #we can have multiple fuseconfig setpoints which get the same voltage
					iVDACVmin = int(float(dPLLInfo[sPLL].lVmins[index])*(dPLLInfo[sPLL].dSubDomain[setpoint]['expr']))                       #converts vmin into VDAC code
					sFunctionArg = ''.join([setpoint," "])																					 #creates the fuseconfig setpoint as dictionary key
					sFunctionArg += ','.join((str(subdomain + ":0d" + str(iVDACVmin)) for subdomain in dPLLInfo[sPLL].dSubDomain[setpoint]['fuses'])) #creates the fuse value from the subdomain dictionary
					sFunctionArg += ''.join([" ",dPLLInfo[sPLL].sPList,str(dPLLInfo[sPLL].lRatioList[index]),"_vidwk_list"," ",sFIVRregex])  #creates the plist name and regex to fuseconfig
					iResult = evg.ExecuteFunction("FUSE_CONFIG_CALLBACKS!setFuse",sFunctionArg)
					if bDebugEnabled: evg.PrintToConsole("My sFunctionArg = " + sFunctionArg + "\n")
					if iResult != 1: iExitPort = 0 #Error occured during FuseConfig Callbacks - exit port 0 and fix your script/input. 
			else: iExitPort = 0 #Error occured in Input File or Script Logic!
			iResult = evg.ExecuteTest(sInstanceName)														#ExecuteTest will return a 1 if the test exits port 1, and a 0 for all other cases
			iExitPort = int(min(float(iExitPort),float(iResult)))											#This should make an exit port of 0 "sticky". Min() only working for floats is idiotic.
		elif (dPLLInfo[sPLL].lRatios[index] not in range(iUPSMinRatio,int(float(iUPSMaxRatio * 1.08))+1)) and (bFlag == 1):
			if dPLLInfo[sPLL].sAppType == "DIRECT_HW":
				evg.SetGSDSDblData(sVminGSDS,float(dPLLInfo[sPLL].lVmins[index]),"UNT",-99,0,1)					   #this value is the vmin for this ratio
			elif dPLLInfo[sPLL].sAppType == "FIVR":
				for setpoint in dPLLInfo[sPLL].dSubDomain:																					 #we can have multiple fuseconfig setpoints which get the same voltage
					iVDACVmin = int(float(dPLLInfo[sPLL].lVmins[index])*(dPLLInfo[sPLL].dSubDomain[setpoint]['expr']))                       #converts vmin into VDAC code
					sFunctionArg = ''.join([setpoint," "])																					 #creates the fuseconfig setpoint as dictionary key
					sFunctionArg += ','.join((str(subdomain + ":0d" + str(iVDACVmin)) for subdomain in dPLLInfo[sPLL].dSubDomain[setpoint]['fuses'])) #creates the fuse value from the subdomain dictionary
					sFunctionArg += ''.join([" ",dPLLInfo[sPLL].sPList,str(dPLLInfo[sPLL].lRatioList[index]),"_vidwk_list"," ",sFIVRregex])  #creates the plist name and regex to fuseconfig
					iResult = evg.ExecuteFunction("FUSE_CONFIG_CALLBACKS!setFuse",sFunctionArg)
					if bDebugEnabled: evg.PrintToConsole("My sFunctionArg = " + sFunctionArg + "\n")
					if iResult != 1: iExitPort = 0 #Error occured during FuseConfig Callbacks - exit port 0 and fix your script/input. 
			else: iExitPort = 0 #Error occured in Input File or Script Logic!
			iResult = evg.ExecuteTest(sInstanceName) + 8
		else:
			iResult = 5																						#Set the result at the "Not Executed" 
			[evg.SetGSDSDblData((dPLLInfo[sPLL].sGSDS+"_"+subpll),99.0,"UNT",-99,0,1) for subpll in dPLLInfo[sPLL].lSubPLL]	   #Set the GSDS at an invalid value for data extraction
		dPLLInfo[sPLL].lResult.append(str(iResult))															#HRY gets either 1 or 0 depending on the test exit port
		[dPLLInfo[sPLL].dLocktime[subpll].append('%.3f' % evg.GetGSDSData((dPLLInfo[sPLL].sGSDS+"_"+subpll),"double","UNT",-99,0)) for subpll in dPLLInfo[sPLL].lSubPLL]	#this loop will append the locktime for subpll's in the PLL
		if bDebugEnabled: [evg.PrintToConsole(subpll+" Ratio "+str(dPLLInfo[sPLL].lRatios[index])+" executed with locktime "+str(dPLLInfo[sPLL].dLocktime[subpll][index])+"\n") for subpll in dPLLInfo[sPLL].lSubPLL]
	if bDebugEnabled: evg.PrintToConsole("Locktime Instances executed for ratios in " + sPLL+ "\n")
	
	if iExitPort == 0: iExitPort = 2																		#Exit Port 0 of this UF is reserved for bin90, port 2 is the "fail" port
	return iExitPort

def datalogResults(iExitPort,sPLL): #Datalog the Calculated VMINs, the HRY string, and the locktime per subpll

	sItuffName = evg.GetInstanceName()	   #This will be ITUFF token used to datalog
	evg.PrintToItuff( sItuffHeader1 + sItuffName + '_' + sPLL	+ '_RATIOS'	   + "\n" + sItuffHeader2 + str("_".join(dPLLInfo[sPLL].lRatioList))		+ "\n" + sItuffHeader3)
	evg.PrintToItuff( sItuffHeader1 + sItuffName + '_' + sPLL	+ '_HRY'	   + "\n" + sItuffHeader2 + str("_".join(dPLLInfo[sPLL].lResult))			+ "\n" + sItuffHeader3)
	evg.PrintToItuff( sItuffHeader1 + sItuffName + '_' + sPLL	+ '_VMINS'	   + "\n" + sItuffHeader2 + str("_".join(dPLLInfo[sPLL].lVmins))			+ "\n" + sItuffHeader3)
	[evg.PrintToItuff(sItuffHeader1 + sItuffName + '_' + subpll + '_LOCKTIME'  + "\n" + sItuffHeader2 + str("_".join(dPLLInfo[sPLL].dLocktime[subpll])) + "\n" + sItuffHeader3)		for subpll in dPLLInfo[sPLL].lSubPLL]
	if iExitPort != 1: evg.PrintToItuff( sItuffHeader1 + sItuffName + '_' + sPLL   + '_UPSVF'	 + "\n" + sItuffHeader2 + dPLLInfo[sPLL].sUPSVF			+ "\n" + sItuffHeader3)

