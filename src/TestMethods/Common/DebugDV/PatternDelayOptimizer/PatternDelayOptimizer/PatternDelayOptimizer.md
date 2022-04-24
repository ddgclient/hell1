# PatternDelayOptimizer template

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

This is a debug/dv test template designed to find the minimum wait time for test execution in an SBFT test. It runs a per-pattern binary search on the wait time to determine the lowest passing value.    

The wait time can be done with a "RPT" or "MOV" instruction.   

----  

## 2. Test Instance Parameters   
  
| Parameter Name | Required?    | Type             | Description  |
| :-----------   | :----------- | :-----------     | :----------- |
| Patlist        | Yes          | Plist            | Plist name to be executed  |
| TimingsTc      | Yes          | TimingCondition  | Timing test condition required for plist execution  |
| LevelsTc       | Yes          | LevelsCondition  | Levels test condition required for plist execution  |
| MaskPins       | No           | String           | Comma separated list of pins to mask before executing Plist  |
| PrePlist       | No           | String           | Prime Callback to execute before each execution of the plist.  |
| [PatmodConfig](#patmodinputfilepatmodconfig)      | Yes | String | Name of the Prime PatConfig to execute to update the pattern wait time.  |
| [PatmodInputFile](#patmodinputfilepatmodconfig)   | Yes | File   | Prime Pattern Modify .patmod.json file containing the PatConfig named in the PatmodConfig parameter.  |
| [PatmodOutputFile](#patmodoutputfile)             | Yes | File   | Output file in Prime Pattern Modify .patmod.json format containing the PatConfig needed to update all the patterns with their optimal wait time.  |
| [SummaryOutputFile](#summaryoutputfile)           | Yes | File   | Output file in .json format containing a summary of the results including the Invalid/Failing patterns.  |
| PerRunPatternLimit                                | No  | Integer| Sets a limit on how many patterns to execute at one time. If the plist contains 10 patterns and this value is 5, then the search will be run with the first 5 patterns enabled and then run again with the remaining 5 patterns. A value of 1 would run each pattern separately. Default of 0 will execute all patterns. This is only meant to help mitigate die heating and plist execution timeouts, it will not affect the results in any way. |
| SearchValueMin                                    | Yes | Integer| Set the minimum wait time to check. |
| SearchValueMax                                    | No  | Integer| If supplied and greater than 0 this sets the maximum wait time to check. Otherwise the existing wait time in the pattern is used.  |
| SearchValueResolution                             | Yes | Integer| Sets the search resolution. If the current testpoint and the nextpoint are closer than this value, then the search will end and report the lowest passing wait time.  |
| MaxTestpoints                                     | Yes | Integer| Sets the maximum number of testpoints to check. Once this limit is reached it will return the lowest passing wait time. |
| GuardbandMultiplier                               | No  | Double | Guardband value. The final wait time will be the lowest passing wait time * (1 + GuardbandMultiplier)  |
| SearchMethod                                      | No  | enum | Determines the type of search run: Binary, LinearLowToHigh (starts at minimum value, stops at first passing point), LinearHighToLow (starts at maximum values, stops at first failing point) |
| RestorePatterns                                   | No  | True/False | If True the patterns original wait times will be restored after the test has finished. If False the patterns will be left with the optimal wait times.  |
| ReloadPatConfig                                   | No  | True/False | If True the PatmodInputFile will be loaded/reloaded when Verify is run on the instance. There is no need for PatmodInputFile to be in the env ALEPH_FILES definition in this case. It can also be used to override the definition of an existing ALEPH file that the user does not have write permissions for. **This might fail due to Prime ticket 23312 and require reloading the testprogram.**  |  


----  

### 2.1. Parameter Details   

#### PatmodInputFile/PatmodConfig  

PatmodInputFile is the Prime Pattern Modify file which defines the PatConfig (referenced
by the PatmodConfig parameter) to use to update the wait times.
It must be registered in the .env file in the ALEPH_FILES section.  

The PatternRegEx can be generic, the code will restrict it to modify one pattern at a time.
(but making it more specific will speed up the ALEPH initialization done by PrimeInitTestMethod)

The StartAddress/Offset should point to the Instruction vector controlling the main wait time to be searched.  

The file is used as a parameter so the template can read the default value of the wait instruction.
Because of this the order of the ConfigurationElements is important. The 1st Domain listed will be used as the base.
The wait times for all other domains must be an integer multiplier of the base domains wait time.
So that means the 1st domain should be the one with the lowest frequency.

Example PatmodInputFile: (PatmodConfig = "ImpactStudiesBaseWaits")
```json
{
	"Configurations": [
		{
			"Name": "ImpactStudiesBaseWaits",
			"ConfigurationElement": [
				{
					"Type": "INSTRUCTION",
					"Domain": "IP_CPU::LEG",
					"StartAddress": "HVM_TEST_WAIT__BRK__ph0__HVMExecWait1",
					"StartAddressOffset": 84,
					"EndAddress": "HVM_TEST_WAIT__BRK__ph0__HVMExecWait1",
					"EndAddressOffset": 84,
					"PatternsRegEx": ["^[gds].*"],
					"ValidationMode" : "ALLOW_LABEL_NO_MATCHING"
				},
				{
					"Type": "INSTRUCTION",
					"Domain": "IP_CPU::DDR",
					"StartAddress": "HVM_TEST_WAIT",
					"StartAddressOffset": 174,
					"EndAddress": "HVM_TEST_WAIT",
					"EndAddressOffset": 174,
					"PatternsRegEx": ["^[gds].*"],
					"ValidationMode" : "ALLOW_LABEL_NO_MATCHING"
				}
			]
		}
	]
}
```

----  

#### PatmodOutputFile  

This is the main output of the template. Its a Prime .patmod.json file which contains all the
Configurations and Data needed to update the patterns with their optimal wait times.
It will only contain the passing patterns, any patterns which failed will not be listed here.

The Configuration name will alway be "ImpactStudiesOptimumWaits"

Example:
```json
{
  "Configurations": [
    {
      "Name": "ImpactStudiesOptimumWaits",
      "ConfigurationElement": [
        {
          "Type": "INSTRUCTION",
          "Domain": "IP_CPU::LEG",
          "StartAddress": "HVM_TEST_WAIT__BRK__ph0__HVMExecWait1",
          "StartAddressOffset": 84,
          "EndAddress": "HVM_TEST_WAIT__BRK__ph0__HVMExecWait1",
          "EndAddressOffset": 84,
          "PatternsRegEx": [
            "^g1153694F1905750I_MM_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Twiddle_3Y_880023AF$"
          ],
          "ValidationMode": "ALLOW_LABEL_NO_MATCHING",
          "Data": "MOV 1232, R7"
        },
        {
          "Type": "INSTRUCTION",
          "Domain": "IP_CPU::DDR",
          "StartAddress": "HVM_TEST_WAIT",
          "StartAddressOffset": 174,
          "EndAddress": "HVM_TEST_WAIT",
          "EndAddressOffset": 174,
          "PatternsRegEx": [
            "^g1153694F1905750I_MM_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Twiddle_3Y_880023AF$"
          ],
          "ValidationMode": "ALLOW_LABEL_NO_MATCHING",
          "Data": "MOV 2464, R7"
        },
        {
          "Type": "INSTRUCTION",
          "Domain": "IP_CPU::LEG",
          "StartAddress": "HVM_TEST_WAIT__BRK__ph0__HVMExecWait1",
          "StartAddressOffset": 84,
          "EndAddress": "HVM_TEST_WAIT__BRK__ph0__HVMExecWait1",
          "EndAddressOffset": 84,
          "PatternsRegEx": [
            "^g1153697F1905744I_Ma_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_88001FDE$"
          ],
          "ValidationMode": "ALLOW_LABEL_NO_MATCHING",
          "Data": "MOV 2096, R7"
        },
        {
          "Type": "INSTRUCTION",
          "Domain": "IP_CPU::DDR",
          "StartAddress": "HVM_TEST_WAIT",
          "StartAddressOffset": 174,
          "EndAddress": "HVM_TEST_WAIT",
          "EndAddressOffset": 174,
          "PatternsRegEx": [
            "^g1153697F1905744I_Ma_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_88001FDE$"
          ],
          "ValidationMode": "ALLOW_LABEL_NO_MATCHING",
          "Data": "MOV 4192, R7"
        },
        {
          "Type": "INSTRUCTION",
          "Domain": "IP_CPU::LEG",
          "StartAddress": "HVM_TEST_WAIT__BRK__ph0__HVMExecWait1",
          "StartAddressOffset": 84,
          "EndAddress": "HVM_TEST_WAIT__BRK__ph0__HVMExecWait1",
          "EndAddressOffset": 84,
          "PatternsRegEx": [
            "^g1153682F1905720I_Mg_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Frenzy_Y_880012CC$"
          ],
          "ValidationMode": "ALLOW_LABEL_NO_MATCHING",
          "Data": "MOV 2240, R7"
        },
        {
          "Type": "INSTRUCTION",
          "Domain": "IP_CPU::DDR",
          "StartAddress": "HVM_TEST_WAIT",
          "StartAddressOffset": 174,
          "EndAddress": "HVM_TEST_WAIT",
          "EndAddressOffset": 174,
          "PatternsRegEx": [
            "^g1153682F1905720I_Mg_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Frenzy_Y_880012CC$"
          ],
          "ValidationMode": "ALLOW_LABEL_NO_MATCHING",
          "Data": "MOV 4480, R7"
        },
        {
          "Type": "INSTRUCTION",
          "Domain": "IP_CPU::LEG",
          "StartAddress": "HVM_TEST_WAIT__BRK__ph0__HVMExecWait1",
          "StartAddressOffset": 84,
          "EndAddress": "HVM_TEST_WAIT__BRK__ph0__HVMExecWait1",
          "EndAddressOffset": 84,
          "PatternsRegEx": [
            "^g1153668F1905746I_Mc_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_880020CE$",
            "^g1153748F1905747I_Md_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_880020FE$",
            "^g1153776F1905743I_Mc_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_88001F8E$",
            "^g1153787F1905742I_Md_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_88001E7E$"
          ],
          "ValidationMode": "ALLOW_LABEL_NO_MATCHING",
          "Data": "MOV 440, R7"
        },
        {
          "Type": "INSTRUCTION",
          "Domain": "IP_CPU::DDR",
          "StartAddress": "HVM_TEST_WAIT",
          "StartAddressOffset": 174,
          "EndAddress": "HVM_TEST_WAIT",
          "EndAddressOffset": 174,
          "PatternsRegEx": [
            "^g1153668F1905746I_Mc_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_880020CE$",
            "^g1153748F1905747I_Md_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_880020FE$",
            "^g1153776F1905743I_Mc_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_88001F8E$",
            "^g1153787F1905742I_Md_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_88001E7E$"
          ],
          "ValidationMode": "ALLOW_LABEL_NO_MATCHING",
          "Data": "MOV 880, R7"
        }
      ]
    }
  ]
}
```



----  

#### SummaryOutputFile  

This is a secondary output of the template. It's a .json file containing a summary of the results.  

The "ConfigName" string is the name of the PatMod Configuration used to generate the results.
It will be the PatmodConfig parameter value.  

The section called "ValidResults" is a dictionary where the Keys are the PatMod Data
(domains are separated by the pipe symbol) and the Value is the list of patterns which use that wait time.  

The last section called "InvalidPatterns" is the list of patterns which fail the Maximum/default
wait time and should be considered as bad/failing patterns.

```json
{
  "ConfigName": "ImpactStudiesBaseWaits",
  "ValidResults": {
    "MOV 1232, R7|MOV 2464, R7": [
      "g1153694F1905750I_MM_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Twiddle_3Y_880023AF"
    ],
    "MOV 2096, R7|MOV 4192, R7": [
      "g1153697F1905744I_Ma_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_88001FDE"
    ],
    "MOV 2240, R7|MOV 4480, R7": [
      "g1153682F1905720I_Mg_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Frenzy_Y_880012CC"
    ],
    "MOV 440, R7|MOV 880, R7": [
      "g1153668F1905746I_Mc_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_880020CE",
      "g1153748F1905747I_Md_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_880020FE",
      "g1153776F1905743I_Mc_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_88001F8E",
      "g1153787F1905742I_Md_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Scylla_SIJY_88001E7E"
    ]
  },
  "InvalidPatterns": [
    "g1153699F1905722I_Mh_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_LeekSpin_Y_880019E0",
    "g1153739F1905748I_MM_VTB044T_Finm042h2ibn_a121216xx00022xbx1xxxalb_TB1PThTC003J1y3_LJx0A42x3nxx0000_DS02_Twiddle_3Y_880022EF"
  ]
}
```

----  

## 3. Datalog output  

None. There is no data logged to ituff.

###  

----  

## 4. TPL Samples  

###  

```csharp
Test PatternDelayOptimizer ImpactStudiesTest1
{
   LevelsTc = "IP_CPU::IP_CPU_BASE::DDR_univ_lvl_nom_lvl";
   TimingsTc = "IP_CPU::IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100";
   Patlist = "IP_CPU::fun_core_sbft_vcc_f5_srhcrx2f5_mci_nc_drg_list";
   
   PerRunPatternLimit = 0;
   SearchValueMin = 400,
   SearchValueResolution = 100;
   MaxTestpoints = 100;
   GuardbandMultiplier = 0.1;
   
   PatmodConfig = "ImpactStudiesBaseWaits";
   PatmodInputFile = "./Modules/IMPACT_STUDIES/InputFiles/impactstudies.patmod.json";
   PatmodOutputFile = "./Modules/IMPACT_STUDIES/InputFiles/is_final.patmod.json";
   SummaryOutputFile = "./Modules/IMPACT_STUDIES/InputFiles/is_final.summary.json";
   
   LogLevel = "PRIME_DEBUG";
}

```


----  

## 5. Exit Ports  

| Exit Port       | Condition   | Description |   
| -----------     | ----------- | ----------- |    
| -2  | Alarm | Any HW alarm condition |    
| -1  | Error | Any software error |   
| 0   | Fail  | not used. |   
| 1   | Pass  | normal exit port if the search ran. |   

###

----  
