# DFX Timing Tuner  

### Rev 0  

| Contents      |    
| :----------- |  
| 1. [Introduction](#introduction)      |   
| 2. [External Dependencies](#external-dependencies)   |     
| 3. [Test Instance Parameters](#test-instance-parameters)   |   
| 4. [Search Range and Adaptive Mode](#search-range-and-adaptive-mode)  
| 5. [Configuration File Format](#config-file)   |   
| 6. [Datalog output](#datalog-output)   |   
| 7. [TPL Samples](#tpl-samples)   |   
| 8. [Exit Ports](#exit-ports)   |  

###

----  
## 1. Introduction  
This test method performs per-pin timing searches on Drive or Compare edges in order to calculate the optimal settings and then saves the results to Hdmt UserVariables.

### 1.1. Methodology  
1. First test instance to set the Input (Drive) edge placement.      
   1. Pattern is a Parallel-In/Serial-Out loopback.  
   1. Perform a search on the Data pins Drive Edge.  
   1. Read serial output data (with CTV) to determine per-pin/per-searchpoint pass/fail results and calculate the optimal settings for each pin.  
   1. Update timings with the optimal Drive Edge values.  
1. Second test instance to set the Output (Compare) edge placement.  
   1. Pattern is a Parallel-In/Parallel-Out loopback.  
   1. Performa a search on the Data pins Compare Edge.  
   1. Read parallel output data (with CTV) to determine per-pin/per-searchpoint pass/fail results and calculate the optimal settings for each pin.  
   1. Update timings with the optimal Compare Edge values.  

----  
## 2. External Dependencies  

 - Requires Per-Pin Timing support using Hdmt UserVariables for the Per-Pin Timing equations.  
   - UserVars should be defined as type "Time".  
 - Requires patterns using TosTrigger inside loops to perform the search.  
 - For IntraDut testprograms TOS requires the Pattern scope (IP or Package) to be the same as the Test Instance scope for TosTrigger to function correctly.  
 - Requires a single instance of "CallbacksManager" to be in the testprogram.  It doesn't need to be in the flow but it needs to exist to register the TOSTrigger callbacks.  
 - See the section [TPL Samples](#tpl-samples) for examples.  

----  
## 3. Test Instance Parameters  

| Parameter Name       | Required? | Type | Values |  Comments |  
| :-----------         | :----------- | :----------- | :----------- | :----------- |   
| Patlist              | Yes | Plist           | Plist name to be executed |  |  
| TimingsTc            | Yes | TimingCondition | Timing test condition required for plist execution |  |  
| LevelsTc             | Yes | LevelsCondition | Levels test condition required for plist execution |  |  
| MaskPins             | No  | CommaSeparatedString | Comma separated list of pins to mask before executing Plist |  |  
| ConfigFile           | Yes | File            | Configuration File | see [Configuration File Format](#config-file) for details |  
| ConfigSet            | Yes | string          | Configuration Set  | see [Configuration File Format](#config-file) for details |  
| SearchStart          | Yes | string          | Time Value | See [Search Range And Adaptive Mode](#search-range-and-adaptive-mode) |  
| SearchResolution     | Yes | string          | Time Value | See [Search Range And Adaptive Mode](#search-range-and-adaptive-mode) |  
| SearchEnd            | Yes | string          | Time Value | See [Search Range And Adaptive Mode](#search-range-and-adaptive-mode) |  
| AdaptiveStart          | Yes | string          | Time Value | See [Search Range And Adaptive Mode](#search-range-and-adaptive-mode) |  
| AdaptiveResolution     | Yes | string          | Time Value | See [Search Range And Adaptive Mode](#search-range-and-adaptive-mode) |  
| AdaptiveEnd            | Yes | string          | Time Value | See [Search Range And Adaptive Mode](#search-range-and-adaptive-mode) |  
| UpdateTC             | No  | string          | None, Current, All | On a successfull search, this controls which Timing TestConditions are updated with the new values. |  
| Datalog              | No  | boolean         | True/False | If True, results are datalogged whether the instance passes or fails, if false results are only datalogged when the instance fails |  

----  
## 4. Search Range and Adaptive Mode   

There are two different sets of search ranges.
The first specified as Search* is the initial search run after Verify.
In normal use this is only the first die or if Adaptive mode fails.
The second is specified as Adaptive*. These are used after the first die passes.
The AdaptiveStart and AdaptiveEnd values are used as offsets from the current value.

The search range is specified with 3 parameters - *Start, *End and *Resolution.
All can be Engineering format with units (ie -2ns, 2ns, 100ps, etc ...).
*End must be greater than *Start.
*Resolution must be positive and less than 2ns.

Standard example:  All die search from 0nS to 10nS.
```perl
	SearchStart = "0ns";
	SearchResolution = "100ps";
	SearchEnd = "10ns";
```

Standard + Adaptive mode:  
1st die searches from -4nS to 6nS.  
2nd+ die searches from <current_value>-2nS to <current_value>+2nS  
```perl
	SearchStart = "-4ns";        
	SearchResolution = "25ps";   
	SearchEnd = "6ns";            
	AdaptiveStart = "-2ns";        
	AdaptiveResolution = "50ps";   
	AdaptiveEnd = "2ns";            
```

**Important Note** The HDMT is limited to a 4ns search range per pattern burst. The software will automatically run multiple bursts if the total search range exceeds 4ns, but the user should be aware that test time for search range of 4.1ns will be significantly higher than if the range was 4ns, even if the number of steps was the same.  

###  

----  
## 5. Config File   

Config File Format:  
```xml
<?xml version="1.0" encoding="utf-8"?>
<!-- TopLevel Element, standard, don not change --> 
<DfxTimingTuner>

    <!-- "pingroup" element used to define a group of pins. -->
    <!-- Occurrence: 1 or more -->
    <!-- Attribute: "name" defines the name of this pingroup. Must be unique in this file. -->
    <pingroup name="PinGroupName">

        <!-- "pin" element used to specify each pin in a group. -->
        <!-- Occurrence: 1 or more -->
        <!-- Attribute: "alias" (optional) Defines an alias (Alternative pin name) which -->
        <!--                    can be used to generate the UserVar name associated with -->
        <!--                    the pin.                                                 -->
        <!-- TextValue: Name of the pin -->
        <pin alias="alias1">pin1name</pin>
        <pin alias="alias2">pin2name</pin>
    </pingroup>

    <!-- "set" element used to define a set of configuration parameters. This will be referenced from the -->
    <!--       test instance in the ConfigSet parameter.                                                  -->
    <!-- Occurrence: 1 or more -->
    <!-- Attribute: "name" defines the name of this set. Must be unique in this file. -->
    <set name="SetName">

        <!-- "search_mode" defines whether this test is for "Drive" (input) mode or "Compare" (output) mode. -->
        <!-- Occurrence: 1 -->
        <!-- TextValue: "Drive" or "Compare" to define which edge to search. -->
        <search_mode>Drive|Compare</search_mode>  

        <!-- "search_pins" is the list of pins that will be searched. (ie these pins drive/compare edges   -->
        <!--               will be changed)                                                                -->
        <!-- Occurrence: 1 -->
        <!-- TextValue: Must reference the name of a "pingroup" element defined in this file. -->
        <search_pins>pingroup name</search_pins>

        <!-- "capture_pins" is the list of pins which will be captured as CTV data. -->
        <!-- Occurrence: 1 -->
        <!-- TextValue: Must reference the name of a "pingroup" element defined in this file. -->
        <capture_pins>pingroup name</capture_pins>

        <!-- "capture_ctvorder" is for "Drive" mode where all the data is on a single pin. The order of    -->
        <!--                    this group defines how to assign the capture data to pins (ie the first    -->
        <!--                    ctv bit is assigned to the first pin in the group, the 2nd ctv data to the -->
        <!--                    2nd pin in the group, etc ...)                                             -->
        <!--                    It will "wrap" if there are 20 CTVs but only 10 pins, the 11th CTV will be -->
        <!--                    assigned as the 2nd bit to the 1st pin.                                    -->
        <!-- Occurrence: 0 if search_mode==Compare, 1 if search_mode==Drive -->
        <!-- TextValue: Must reference the name of a "pingroup" element defined in this file. -->
        <capture_ctvorder>pingroup name</capture_ctvorder>

        <!-- "uservar" contains the expression used to generate name of the UserVar to store the per-pin   --> 
        <!--           results. It uses the pingroup defined by "ctv_order" (for drive mode) or            -->
        <!--           "capture_pins" (for compare mode). The variables %ALIAS% and %PIN% can be used to   -->
        <!--           refer to the pins alias or name.                                                    -->
        <!-- Occurrence: 1 -->
        <uservar>Collection.UserVarRegEx</uservar>

        <!-- "loop_size" contains the information to modify the number of TOSTrigger loops in the pattern. -->
        <!-- Occurrence: 1 -->
        <!-- Attribute: "config" (required) The Prime PatConfig Name used to update the pattern. Must be   -->
        <!--            loaded in the tester using a Prime PatConfig ALEPH file.                           -->
        <!-- TextValue: The Data to write using PatConfig. Use %SIZE% to refer to the loop counter         -->
        <!--            (auto-generated by the template based on the search range). The data must contain  -->
        <!--            the pattern Instruction Opcode and all Operands (ie "MOV %SIZE%, R1")              -->
        <loop_size config="PatConfig">LoopControlRegEx</loop_size>

        <!-- "pingroup_for_adjust" Optional for test time reduction                                           -->
        <!--                       This is the Hdmt Pingroup that holds all the pins in the search_pins list  -->
        <!-- Occurrence: 0-1 -->
        <!-- TextValue: Must reference the name of a Hdmt PinGroup which contains all the pins in the search_pins list -->
        <pingroup_for_adjust>HdmtPinGroup</pingroup_for_adjust>

    </set>
</DfxTimingTuner>
```

----  
### Example Config File  
```xml
<?xml version="1.0" encoding="utf-8"?>  
<DfxTimingTuner>  
  <pingroup name="MCI_Pins">  
	<pin alias="MCI003">IP_CPU::DDRDQ_IL05_NIL11_LP41_3</pin>
	<pin alias="MCI002">IP_CPU::DDRDQ_IL05_NIL11_LP41_2</pin>
	<pin alias="MCI001">IP_CPU::DDRDQ_IL05_NIL11_LP41_1</pin>
	<pin alias="MCI000">IP_CPU::DDRDQ_IL05_NIL11_LP41_0</pin>
  </pingroup>  
  <pingroup name="TDO">  
    <pin>DPIN_9_000</pin>  
  </pingroup>  
  <set name="SampleDriveMode">  
    <search_mode>Drive</search_mode>  
    <search_pins>MCI_Pins</search_pins>  
    <capture_pins>TDO</capture_pins>  
    <capture_ctvorder>MCI_Pins</capture_ctvorder>  
    <uservar>TimingCollection1.%ALIAS%_drv_offset</uservar>  
    <loop_size config="DfxTuneLoopSize">MOV %SIZE%, R1</loop_size>  
    <pingroup_for_adjust>mci_in</pingroup_for_adjust>
  </set>  
  <set name="SampleCompareMode">  
    <search_mode>Compare</search_mode>  
    <search_pins>MCI_Pins</search_pins>  
    <capture_pins>MCI_Pins</capture_pins>  
    <uservar>TimingCollection1.%ALIAS%_stb_offset</uservar>  
    <loop_size config="DfxTuneLoopSize">MOV %SIZE%, R1</loop_size>  
    <pingroup_for_adjust>mci_out</pingroup_for_adjust>
  </set>  
</DfxTimingTuner>  
```

----  
## 5.1. Prime PatConfig Details  
The config file requires setting up a Prime PatternConfig through ALEPH files. Details about that can be found on the Prime Wiki at [PatConfig Service](https://dev.azure.com/mit-us/PRIME/_wiki/wikis/PRIME.wiki/4650/PatConfig-Service(PatModify-FuseConfig))

Example ALEPH file for the required patmod:  
```json
{
	"Configurations": [
		{
			"Name": "DfxTuneLooopSize",
			"ConfigurationElement": [
				{
					"Type": "INSTRUCTION",
					"Domain": ["IP_CPU::LEG","IP_CPU::DDR"],
					"StartAddress": "LOOP_INIT_LABEL",
					"StartAddressOffset": 0,
					"EndAddress": "LOOP_INIT_LABEL",
					"EndAddressOffset": 0,
					"PatternsRegEx": [".*_MCI_BABYSTEP_trgsrch_.*"]
				}
			]
		}
	]
}
```

----  
## 6. Datalog output
Datalogging is done per-pin. There will be one tname/strgval per pin. The tname will be the "testname::UserVarCategory.UserVar"

If the Datalog parameter is false, results will only be logged if the test fails (but all pins will be logged not just the ones that failed). Failing pins will have a value of -9999 (no units).   

If there is some hardware or software error that prevents the search from completing, the error will be logged under "tname::ERROR"  

Example Ituff logging (failure on DPIN_9_007 pin):  
```perl
2_tname_TGLPRIME::DfxTimingTuner_Test1::PPTimingOffsets_MCI100.DPIN_9_003_stb_offset  
2_strgval_4.200ns  
2_tname_TGLPRIME::DfxTimingTuner_Test1::PPTimingOffsets_MCI100.DPIN_9_005_stb_offset  
2_strgval_7.800ns  
2_tname_TGLPRIME::DfxTimingTuner_Test1::PPTimingOffsets_MCI100.DPIN_9_007_stb_offset  
2_strgval_-9999  
2_tname_TGLPRIME::DfxTimingTuner_Test1::PPTimingOffsets_MCI100.DPIN_9_011_stb_offset  
2_strgval_7.000ns  
2_tname_TGLPRIME::DfxTimingTuner_Test1::PPTimingOffsets_MCI100.DPIN_9_013_stb_offset  
2_strgval_5.800n  
```

Example Ituff logging (error while executing):  
```perl
2_tname_TGLPRIME::DfxTimingTuner_Test1::ERROR  
2_strgval_this_will_be_the_error_message_here  
```

###  

----  
## 7. TPL Samples  

### 7.1. Example Flow/Instances   

**Example Flow:**  
![SimpleFlow](images/dfxtuner_tpflow2.png)

**Example Instances**:
```perl
Test DfxTimingTuner TuneSTF100_Drive
{
    Patlist = "STF100_DriveTrain_Plist";
    LevelsTc = "__main__::basic_func_lvl_mid";
    TimingsTc = "__main__::stf100_timing_100MHz";
    
    ConfigFile = "./Modules/TGLPRIME/InputFiles/DfxTunerConfigSTF.xml";
    ConfigSet = "STF100DriveSet";
    
    SearchStart = "-4";
    SearchResolution = "100ps";
    SearchEnd = "4ns";

    Datalog = "False";
    UpdateTC = "Current";
}

Test DfxTimingTuner TuneSTF100_Compare
{
    Patlist = "STF100_CompareTrain_Plist";
    LevelsTc = "__main__::basic_func_lvl_mid";
    TimingsTc = "__main__::stf100_timing_100MHz";
    
    ConfigFile = "./Modules/TGLPRIME/InputFiles/DfxTunerConfigSTF.xml";
    ConfigSet = "STF100CompareSet";
    
    SearchStart = "-4";
    SearchResolution = "100ps";
    SearchEnd = "4ns";

    Datalog = "False";
    UpdateTC = "Current";
}

Test DfxTimingTuner TuneSTF400_Drive
{
    Patlist = "STF400_DriveTrain_Plist";
    LevelsTc = "__main__::basic_func_lvl_mid";
    TimingsTc = "__main__::stf400_timing_100MHz";
    
    ConfigFile = "./Modules/TGLPRIME/InputFiles/DfxTunerConfigSTF.xml";
    ConfigSet = "STF400DriveSet";
    
    SearchStart = "-2";
    SearchResolution = "20ps";
    SearchEnd = "2ns";

    Datalog = "True";
    UpdateTC = "Current";
}

Test DfxTimingTuner TuneSTF400_Compare
{
    Patlist = "STF400_CompareTrain_Plist";
    LevelsTc = "__main__::basic_func_lvl_mid";
    TimingsTc = "__main__::stf400_timing_100MHz";
    
    ConfigFile = "./Modules/TGLPRIME/InputFiles/DfxTunerConfigSTF.xml";
    ConfigSet = "STF400CompareSet";
    
    SearchStart = "-2";
    SearchResolution = "20ps";
    SearchEnd = "2ns";

    Datalog = "True";
    UpdateTC = "All"; # final test should upate apply all changes to all testconditions.
}

# the picture is old, there should be an instance of CallbacksManager in the 
# TPI_BASE_PRIME flow, so this isn't needed anymore.
Test DfxTimingTunerCallbacks SetupDfxTimingTuningCallbacks
{
    LogLevel = "PRIME_DEBUG";
}

```

----  
### 7.2. Example Pattern      

Below shows an example pattern structure. The important parts are:  
- Label where the loop is initialized (LOOP_INIT_LABEL) so it can be changed by code.  
- The Register used to control the loop doesn't matter, but must match the one used in the configuration file.  
- TOSTrigger at the END of the loop.  
- See HDMT UserGuide for details on how to use ClearSoftwareTrigger/TOSTrigger/JNSTR with multi-domains.   

**Example Pattern Structure:**   
```perl
# setup/configuration/reset vectors
    { V { all_leg=EE0X0111... } }
    ...
# Initialize the search loop counter
LOOP_INIT_LABEL:
MOV 50, R9

# Label for start of the search loop
LOOP_START_LABEL:

# Training vectors, with CTV to capture data.
    { V { all_leg=EE0X0111... } }
    ...
CTV { V { all_leg=EE0X0111... } }
    ...
    { V { all_leg=EE0X0111... } }
    ...

# TOSTrigger to update the timings for the next search point
# The ClearSoftwareTrigger/TOSTrigger should only be in one
# domain. The JNSTR loop needs to be in all domains.
ClearSoftwareTrigger
TOSTrigger 1
SWTrigger_LoopStart:

# dummy vectors/loop to wait until TOSTrigger is done
    { V { all_leg=EE0X0111... } }
    ...
JNSTR SWTrigger_LoopStart

# End of search loop
DEC R9
JNZ LOOP_START_LABEL

# final cleanup/shutdown vectors.
    { V { all_leg=EE0X0111... } }
    ...
RET
```

----  
### 7.3. Example Timings (.tim, .tcg, .usrv)   
TODO: get some MTL examples for timing.

**Example UserVars for per-pin offset:**  
```perl
UserVars TimingOffsets_MCI100
{
    Time mci0_drv_offset = 4ns;
    Time mci1_drv_offset = 4ns;
    Time mci2_drv_offset = 4ns;
    
    Time mci0_stb_offset = 10ns;
    Time mci1_stb_offset = 10ns;
    Time mci2_stb_offset = 10ns;
}

```

**Example TestCondition using per-pin offsets:**  
```perl
SpecificationSet sampleSpecset (cat1)
{
   ...
    # base edge placements with group offsets
    Time p_mci_drv_offset = 0nS;
    Time p_mci_strb_offset = 2nS;
    Time c_mci_drv = c_bclk_drv_e1+(p_mci_drv_mul*c_bclk_per)+p_mci_drv_offset;
    Time c_mci_strb = c_bclk_drv_e1+(p_mci_strb_mul*c_bclk_per)+p_mci_strb_offset;
    
    # final drive/strobe locations with per-pin offsets
    Time c_mci0_drv = c_mci_drv + mci0_drv_offset;
    Time c_mci0_strb = c_mci_strb + mci0_stb_offset;
    Time c_mci1_drv = c_mci_drv + mci1_drv_offset;
    Time c_mci1_strb = c_mci_strb + mci1_stb_offset;
    Time c_mci2_drv = c_mci_drv + mci2_drv_offset;
    Time c_mci2_strb = c_mci_strb + mci2_stb_offset;
   ...
}
TestConditionGroup sample_TCG
{
    SpecificationSet = sampleSpecset;
    Timing = __main__::sampleTimingSequence;
}
TestCondition sample_perpin_testcondition
{
    TestConditionGroup  = sample_TCG;
    Selector = cat1;
}
```

**Example Timings using per-pin offsets:**  
```perl
Timing sampleTimingSequence
{
    Domain Default
    {
        PeriodTable
        {
            Period  = tper;
        }
		
        MCI0_FULLPINNAME
        {
            drive = c_mci0_drv;
            compare =  c_mci0_strb;
        }
        MCI1_FULLPINNAME
        {
            drive = c_mci1_drv;
            compare =  c_mci1_strb;
        }
        MCI2_FULLPINNAME
        {
            drive = c_mci2_drv;
            compare =  c_mci2_strb;
        }

        ...
				
    }  # end of Domain
    
} # end-of timing type
```

###  

----  
## 8. Exit Ports  

| Exit Port       | Condition   | Description |   
| -----------     | ----------- | ----------- |    
| -2  | Alarm | Any HW alarm condition |    
| -1  | Error | Any software error |   
| 0   | Fail  | Failing condition. |   
| 1   | Pass  | Passing condition. |   

###

----  
