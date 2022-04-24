# ImpactStudiesVmin template

## DEBUG ONLY -- NOT FOR PRODUCTION TEST

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

This is a debug/dv test template designed to run a series of Vmin searches in order to find the best patterns to use.  
The user supplies a base set of VminTC parameters and a list of patternlists. The template will run a Vmin search on each 
patternlist (forwarding the vmin from one to the next) and run a custom per-IP scoreboard on each.

----  

## 2. Test Instance Parameters  

| Parameter Name       | Required? | Type | Description |  
| :-----------         | :----------- | :----------- | :----------- |      
| ConfigurationFile    | Yes | File   | Configuration file to use. See details below.  |  


----  

### 2.1. Parameter Details   

#### ConfigurationFile  

The configuration file contains 2 main sections:
1. VminParameters  
   1. This is a dictionary containing the base VminTC parameters to use for each search (eg TimingsTc, LevelsTc, PinMap, ...)  
2. Tests
   1. This is the list of Vmin searches to run. It contains 3 elements.  
      1. Name: This is the name to use when logging information about this search. It can be anything, but should be unique.  
      2. Patlist: This is the plist to run the search on.  
      3. Overrides: [Optional] This is a dictionary containing VminTC parameters to use for this test. They can be in addtion to the ones under the VminParametes section, or can overwrite them.  

The first Test in the Tests section will have its StartVoltages parameter set to the StartVoltages in the VminParameters section. 
All subsequent tests will use the vmin result from the previous test.   

```json
{
  "VminParameters": 
  {
    "ParameterA": "ValueA",
    "ParameterB": "ValueB"
  },
  "Tests": 
  [
    { 
      "Name": "BaselineTest",
      "Patlist": "BaselinePlist",
      "Overrides": { "ParameterC": "ValueC" }
    },
    { 
      "Name": "SearchTest1",
      "Patlist": "Plist1",
    },
    { 
      "Name": "SearchTest2",
      "Patlist": "Plist1",
      "Overrides": { "ParameterC": "ValueC", "ParameterA": "ValueD" }
    }
  ]
}
```

----  

## 3. Datalog output  

This information is logged for each "Test" listed in the Configuration File. 
For the examples below, the test instance was called "IP_CPU\::IMPACT_STUDIES\::TestImpactStudiesVmin" and the 
test from the ConfigFile was called "baseline".  

The first thing logged is the standard Vmin results.  
tname is TestInstanceName|TestNameFromConfigFile
strgval is FinalVoltageValues|StartingVoltageValues|UpperLimitVoltageValues|NumberOfExecutions  
```
0_tname_IP_CPU::IMPACT_STUDIES::TestImpactStudiesVmin|baseline
0_strgval_0.510_0.490_0.520_0.530|0.400_0.400_0.400_0.400|1.200_1.200_1.200_1.200|14
```

The next thing is the scoreboard test details.
tname is TestInstanceName|TestNameFromConfigFile|scb
strgval is TargetVoltageValues|StartingPatternName|StartingPatternBurst|StartingPatternIndex|NumberOfFailuresLogged
```
0_tname_IP_CPU::IMPACT_STUDIES::TestImpactStudiesVmin|baseline|scb
0_strgval_0.49_0.47_0.5_0.51|g1153682F1905720I_Mg_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Frenzy_Y_880012CC|0|1|6
```

The next thing logged (if the scoreboard test had failures) will be the per-IP failures.
tname is TestInstanceName|TestNameFromConfigFile|PinMap
(if the failure cannot be mapped to a PinMap, then the failing pin will be used instead)
strgval is the list of ScoreboardBaseNum + ScoreBoardNum
(ScoreBoardNum is generated using the PatternNameMap parameter)
```
0_tname_IP_CPU::IMPACT_STUDIES::TestImpactStudiesVmin|baseline|CORE0_NOA
0_strgval_00011153682|00011153739

0_tname_IP_CPU::IMPACT_STUDIES::TestImpactStudiesVmin|baseline|CORE1_NOA
0_strgval_00011153682|00011153739

0_tname_IP_CPU::IMPACT_STUDIES::TestImpactStudiesVmin|baseline|CORE2_NOA
0_strgval_00011153682|00011153739

0_tname_IP_CPU::IMPACT_STUDIES::TestImpactStudiesVmin|baseline|CORE3_NOA
0_strgval_00011153697
```

If the failure was due to an AMBLE pattern (preburst/prepattern/postburst/postpattern)
it will be logged as a AMBLE IP failure with the full pattern name (not a scoreboard number)  
```
0_tname_IP_CPU::IMPACT_STUDIES::TestImpactStudiesVmin|baseline|AMBLE
0_strgval_tgl_pre_F9999991G_040416xxx10040x44xxalb_T0ax2i_0439_Mdrv_0_vrevTB1P_hdmt2_hvm_hdmt2_CXJ_cf1y2_0
```


The last information logged comes from the normal VminTC ituff information. 
This includes the search results and the limiting patterns. 
This information will not include the TestName from the Configuration File.  
```
0_tname_IP_CPU::IMPACT_STUDIES::TestImpactStudiesVmin
0_strgval_0.510_0.490_0.520_0.530|0.400_0.400_0.400_0.400|1.200_1.200_1.200_1.200|14

0_tname_IP_CPU::IMPACT_STUDIES::TestImpactStudiesVmin_lp
0_strgval_1153787|1153682|1153697|1153739
```

###  

----  

## 4. TPL Samples  

###  

```csharp
Test ImpactStudiesVmin TestImpactStudiesVmin
{
    ConfigurationFile = "./Modules/IMPACT_STUDIES/InputFiles/impactstudies.vminconfig.json";
    LogLevel = "PRIME_DEBUG";
}
```

Example impactstudies.vminconfig.json
```json
{
  "VminParameters": 
  {
    "TimingsTc": "IP_CPU::IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100",
    "LevelsTc": "IP_CPU::IP_CPU_BASE::DDR_univ_lvl_nom_lvl",
    "PinMap": "CORE0_NOA,CORE1_NOA,CORE2_NOA,CORE3_NOA",
    "StartVoltages": "0.4,0.4,0.4,0.4",
    "EndVoltageLimits": "1.2,1.2,1.2,1.2",
    "StepSize": "0.01",
    "VoltageTargets": "CORE0,CORE1,CORE2,CORE3",
    "TestMode": "MultiVmin",
    "PatternNameMap": "1,2,3,4,5,6,7",
    "ScoreboardBaseNumber": "0000",
    "ScoreboardEdgeTicks": 2,
    "FeatureSwitchSettings": "fivr_mode_on",
    "FivrCondition": "NOM",
    "PreInstance": "WriteSharedStorage(--token G.U.S.Dummy1 --value A)"
  },
  "Tests": 
  [
    { 
      "Name": "baseline",
      "Patlist": "IP_CPU::fun_core_sbft_vcc_f5_srhcrx2f5_mci_nc_drg_list",
      "Overrides": { "ScoreboardBaseNumber": "0001" }
    },
    { 
      "Name": "search1",
      "Patlist": "IP_CPU::experimental_list",
      "Overrides": { "ScoreboardBaseNumber": "0002" }
    }
  ]
}
```


----  

## 5. Exit Ports  

| Exit Port       | Condition   | Description |   
| -----------     | ----------- | ----------- |    
| -2  | Alarm | Any HW alarm condition |    
| -1  | Error | Any software error |   
| 0   | Fail  | not used. |   
| 1   | Pass  | normal exit port if the searches ran. |   

###

----  
