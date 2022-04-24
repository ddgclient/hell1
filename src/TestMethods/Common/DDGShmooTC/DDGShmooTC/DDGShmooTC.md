# DDG Shmoo template

### Rev 0

| Contents      |    
| :----------- |  
| 1. [Introduction](#introduction)      |   
| 2. [Test Instance Parameters](#test-instance-parameters)   |   
| 3. [Datalog output](#datalog-output)   |   
| 4. [TPL Samples](#tpl-samples)   |   
| 5. [Exit Ports](#exit-ports)   |  

###

----  
## 1. Introduction  

This TestMethod extends the default Prime Shmoo testclass with support for different axis types. See the [Prime Wiki](https://dev.azure.com/mit-us/PrimeWiki/_wiki/wikis/PrimeWiki.wiki/33613/Readme) for details on the base shmoo class.  

----  
## 2. Test Instance Parameters  

| Parameter Name       | Required? | Type | Values |  Comments |  
| :-----------         | :----------- | :----------- | :----------- | :----------- |   
| Patlist              | Yes | Plist           | Plist name to be executed |  |  
| TimingsTc            | Yes | TimingCondition | Timing test condition required for plist execution |  |  
| LevelsTc             | Yes | LevelsCondition | Levels test condition required for plist execution |  |  
| MaskPins             | No  | CommaSeparatedString | Comma separated list of pins to mask before executing Plist |  |  
| XAxisType            | No  | String               | [Axis](#axistype)            | X-Axis Mode |  
| XAxisParam           | Yes | CommaSeparatedString | [Parameter](#parametertype)  | X-Axis parameter to shmoo |  
| XAxisRange           | Yes | String               | [Range](#rangetype)          | X-Axis values to shmoo |  
| YAxisType            | No  | String               | [Axis](#axistype)            | Y-Axis Mode |  
| YAxisParam           | Yes | CommaSeparatedString | [Parameter](#parametertype)  | Y-Axis parameter to shmoo |  
| YAxisRange           | Yes | String               | [Range](#rangetype)          | Y-Axis values to shmoo |  
| VoltageConverter     | Yes | String               |           | See VoltageConverter Callbacks for details  |  
| DataLogType          | Yes | String               | ECADS, SHMOO_HUB | defaults to SHMOO_HUB |  
| PrePointExecMode     | No  | String               | See [PrePointExec](#prepointexec) |  
| PrePointExecTest     | No  | String               | See [PrePointExec](#prepointexec) |  
| IfeObject            | Yes | String               |  ""  | Useless inherited parameter, always set to "". |  
| XAxisParamType       | No  | String  | UserDefined/None | Set to None if XAxisType is None, othwise leave it a UserDefined. |
| YAxisParamType       | No  | String  | UserDefined/None | Set to None if YAxisType is None, othwise leave it a UserDefined. |


----  
## 2.1. Parameter Types  

#### AxisType  
**None**: Axis is disabled.  
**UserVarTiming**: Axis is an Hdmt User Variable used in timing equations. Parameter should be *collection*.*variable*  
**UserVarLevels**: Axis is an Hdmt User Variable used in levels equations.  Parameter should be *collection*.*variable*  
**SpecSetVariable**: Axis is a TestCondition/SpecSet variable.  
**FIVR**: Axis is a FIVR domain. Parameter must be defined in a Prime ALEPH .fivrDomain.json file.  
**DLVR**: Axis is a DLVR domain. Parameter must be defined in a Prime ALEPH .fivrDomain.json file and the VoltageConverter parameter must be specified with the DLVR configurations/settings.  
**PatConfig**: Axis is a Prime PatConfig Handle. Paramter must be defined in a Prime ALEPH .patmod.json file.  
**PatConfigSetPoint**: Axis is a Prime PatConfig SetPoint. Parameter must be defined in a Prime ALEPH .PatConfigSetpoints.json file.  

#### ParameterType  
The name of the parameter to shmoo. 
See [AxisType](#axistype) section for details on what parameter types are allowed. 
The SpecSetVariable type must be a single parameter, all the others can be a comma separated list of parameters. 
(in which case all will get the same value, so if the cores are all on different FIVR rails you can still shmoo them all at the same time) 

Both X and Y parameters are required but you can set the YAxisParam & YAxisRange to "" if you only want to do a 1-Dimension shmoo.  

#### RangeType  

Two formats are supported.  
**Format1**: ```StartValue:StepSize:NumberOfSteps```   
All values should be numbers (double type). For some Axis types the value will be converted to the appropriate format (ie for PatConfig it will be converted to a binary string of the proper length)     

**Format2**: Comma-Separated-String  
In this format, all the values to shmoo must be listed out separately, deliminated by commas. In this mode the values will not be altered before being written to the parameter, so its up to the user to use the correct format/length for each value.  

#### PrePointExec  

PrePointExecMode/PrePointExecTest parameters provides a mechanism for running a separate test instance before execution of a shmoo testpoint.
If the test fails (exits other than port 1) then the shmoo test point will be marked as SKIPPED ("#").  

The test will be run BEFORE the shmoo parameter is updated. After the test is executed the shmoo parameters will be updated and BOTH shmoo timings/levels will be re-applied to the hardware (regardless of which one was changed).  

**PrePointExecTest** should be the name of the test instance to run (fully scoped if this is an intradut/modular testprogram).
Also note that for intradut the test must be in the same scope as the shmoo. A shmoo in IP1 cannot run a test from IP2 or from the package.  


**PrePointExecMode** can be one of:
 -  Never : feature is disabled.  
 -  OnXChange : Run PrePointExecTest when the X Parameter changes.  
 -  OnYChange : Run PrePointExecTest when the Y Parameter changes.  
 -  OnAnyChange : Run PrePointExecTest when either Axis parameter changes.  

Example:
```
PrePointExecMode = "OnYChange";  
PrePointExecTest = "DRV_RESET_BASE::RESET_X_FUNC_K_START_X_XXXXXX_X_X_FIVR_PWRON";  
```   


----  
## 3. Datalog output  
Supports SHMOO_HUB and ECADS formats.
The base Prime Shmoo will always print the SHMOO_HUB format if either axis is SpecSetVariable.
Ticket filed with MIT but for now selecting ECADS format will cause both formats to be logged.    

The "current" X/Y value is only correct for SpecSetVariable types.  

If the failing pattern is not the first instance of the pattern then it will be displayed as "patternname|instanceid".  

Sample SHMOO_HUB format.
```   
0_tname_TPI_BASE_PRIME::TestDDGFivrShmoo_SSTP
0_strgval_X_1E-08_Y_-1

0_tname_TPI_BASE_PRIME::TestDDGFivrShmoo^p_bclkper_spec^8E-09^1.1E-08^1E-09_CORE0,CORE1,CORE2,CORE3^0.75^0.95^0.05
0_strgval_aaaa_bbbb_cc**_****_****

0_tname_TPI_BASE_PRIME::TestDDGFivrShmoo^LEGEND^a
0_strgval_tgl_pre_F9999991G_040416xxx1a040x22xxalb_T0xx2i_4l00_Mdrv_0_vrevTB1P_hdmt2mcpi_flat_hdmt2_CXJ_cf2kg_0:myplist:LEG(0,557,-1,-1):IP_CPU::TDO

0_tname_TPI_BASE_PRIME::TestDDGFivrShmoo^LEGEND^b
0_strgval_tgl_pre_F9999991G_040416xxx1a040x22xxalb_T0xx2i_4l00_Mdrv_0_vrevTB1P_hdmt2mcpi_flat_hdmt2_CXJ_cf2kg_0:myplist:LEG(0,561,-1,-1):IP_CPU::TDO

0_tname_TPI_BASE_PRIME::TestDDGFivrShmoo^LEGEND^c
0_strgval_tgl_pre_F9999991G_040416xxx1a040x22xxalb_T0xx2i_4l00_Mdrv_0_vrevTB1P_hdmt2mcpi_flat_hdmt2_CXJ_cf2kg_0:myplist:LEG(0,581,-1,-1):IP_CPU::TDO
```   

Sample ECADS format.
```
0_tname_TPI_BASE_PRIME::TestDDGFivrShmoo
0_comnt_PLOT_PXName,p_bclkper_spec
0_comnt_PLOT_PYName,CORE0,CORE1,CORE2,CORE3
0_comnt_PLOT_PXStart,8E-09
0_comnt_PLOT_PYStart,0.75
0_comnt_PLOT_PXStop,1.1E-08
0_comnt_PLOT_PYStop,0.95
0_comnt_PLOT_PXStep,4
0_comnt_PLOT_PYStep,5
0_comnt_PLOT_PXValue,1E-08
0_comnt_PLOT_PYValue,-1
0_comnt_P3Data_aaaa
0_comnt_P3Data_bbbb
0_comnt_P3Data_cc**
0_comnt_P3Data_****
0_comnt_P3Data_****
0_comnt_P3Legend_a_tgl_pre_F9999991G_040416xxx1a040x22xxalb_T0xx2i_4l00_Mdrv_0_vrevTB1P_hdmt2mcpi_flat_hdmt2_CXJ_cf2kg_0:myplist:LEG(0,557,-1,-1):IP_CPU::TDO
0_comnt_P3Legend_b_tgl_pre_F9999991G_040416xxx1a040x22xxalb_T0xx2i_4l00_Mdrv_0_vrevTB1P_hdmt2mcpi_flat_hdmt2_CXJ_cf2kg_0:myplist:LEG(0,561,-1,-1):IP_CPU::TDO
0_comnt_P3Legend_c_tgl_pre_F9999991G_040416xxx1a040x22xxalb_T0xx2i_4l00_Mdrv_0_vrevTB1P_hdmt2mcpi_flat_hdmt2_CXJ_cf2kg_0:myplist:LEG(0,581,-1,-1):IP_CPU::TDO
```   


###  

----  
## 4. TPL Samples  

###  

MTL DLVR (class) Example:   
```perl
Test DDGShmooTC DIAG_CORE_SHMOO_E_BEGCPU_X_VCORE_F1_X_STF200
{
	LevelsTc = "IP_CPU_BASE::cpu_all_bf_x_x_ipcpu_lvl_min_lvl";
	TimingsTc = "SCN_CORE_C68::cpu_stf_timing_tclk100_sclk200_cclk400";
	Patlist = "scn_core_c68_vccc_f1_begcpu_sNs200_mix_core0_atpg_classhvm_list";
	SetPointsPlistParamName = "Patlist";
	SetPointsPreInstance = "MaaaCdrv:atomfreq:0.8GHz,MaaaCdrv:ringfreq:1.3GHz,MaaaCdrv:corefreq:1.3GHz";
	XAxisType = "SpecSetVariable";
	XAxisParam = "p_mtd_per";
	XAxisDatalogPrefix = "Nano";
	XAxisRange = "8e-9:1e-9:4";
	YAxisType = "FIVR";
	YAxisParam = "CORE5,CORE4,CORE3,CORE2,CORE1,CORE0";
	YAxisRange = "0.5:0.02:41";
	VoltageConverter = "--dlvrpins IP_CPU::VCCIA_HC --fivrcondition NOM";
}

```



TGL FIVR Example:
```perl
Test DDGShmooTC TestDDGFivrShmoo
{
    Patlist = "list_drv_mcp_tgl_pre_F9999991G_040416xxx1a040x22xxalb_T0xx2i_4l00_Mdrv_0_vrevTB1P_hdmt2mcpi_flat_hdmt2_CXJ_cf2kg_0";
    TimingsTc = "__main__::mcp_func_univ_pkg_mcp_univ_univ_b100_t100_d100_p4ns";
    LevelsTc = "__main__::DDR_univ_lvl_pkg_nom_lvl";
    XAxisType = "SpecSetVariable";
    XAxisParam = "p_bclkper_spec";
    XAxisRange = "8e-9:1e-9:4";
    YAxisType = "FIVR";
    YAxisParam = "CORE0,CORE1,CORE2,CORE3";
    YAxisRange = "0.75:0.05:5";
    VoltageConverter = "--fivrcondition=NOM";
    IfeObject = "";
    LogLevel = "PRIME_DEBUG";
}
```


----  
## 5. Exit Ports  

| Exit Port       | Condition   | Description |   
| -----------     | ----------- | ----------- |    
| -2  | Alarm | Any HW alarm condition |    
| -1  | Error | Any software error |   
| 0   | Fail  | At least one point failed in the shmoo. |   
| 1   | Pass  | All points passed in the shmoo. |   

###

----  
