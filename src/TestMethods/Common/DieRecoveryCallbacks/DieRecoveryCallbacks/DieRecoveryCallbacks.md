# DDG Prime Die Recovery Callbacks
### 3/2/2022

[ConfigureIpForRecovery](#configureipforrecovery)

[MaskIP](#maskip)

[DisableIP](#disableip)

[WriteTracker](#writetracker)

[CopyTracker](#copytracker)

[CloneTracker](#clonetracker)

[LoadPinMapFile](#loadpinmapfile)

[RunRule](#runrule)

##   

## Enabling the Callbacks
Add an instance of CallbacksManager in your INIT flow. No parameters are necessary.

## Using callbacks
Callbacks can be executed from CommonParameters in Prime test instances and from anywhere an Evergreen UserFunction
can be executed. See the prime wiki for more details.

[Prime WIKI - Calling Cs from Evg](https://dev.azure.com/mit-us/PRIME/_wiki/wikis/PRIME.wiki/3019/Calling-Prime-C-Sharp-Code-from-EVG-UF)

[Prime WIKI - Common Parameters](https://dev.azure.com/mit-us/PRIME/_wiki/wikis/PRIME.wiki/3020/Common-Parameters)

#### Simple Example from ICUserFuncTest

<span style="font-family:monospace; font-size:8; color:blue">
Test iCUserFuncTest LoadIaCoreNoaDieRecoveryPinMaps<br>
{<br>
&nbsp;&nbsp;&nbsp;&nbsp;function_name = "CPD_DEBUG!ExecPrimeCsCallback";<br>
&nbsp;&nbsp;&nbsp;&nbsp;function_parameter = "CallbackName(--arg1 val1 --arg2 val2)";<br>
}<br>
</span>

If the function_paramter contains an equals sign, then you must assign the result to a GSDS variable.
Unless specified this GSDS value will be empty, its a Prime/Evergreen handshaking thing.
The "--arg1=val1" syntax is functionally equivalent to "--arg1 val1".  

<span style="font-family:monospace; font-size:8; color:blue">
Test iCUserFuncTest LoadIaCoreNoaDieRecoveryPinMaps<br>
{<br>
&nbsp;&nbsp;&nbsp;&nbsp;function_name = "CPD_DEBUG!ExecPrimeCsCallback";<br>
&nbsp;&nbsp;&nbsp;&nbsp;function_parameter = "G.U.S.DummyValue=CallbackName(--arg1=val1 --arg2=val2)";<br>
}<br>
</span>


## Available Functions

----

### ConfigureIpForRecovery   
This function applies the PatModify associated with the given pinmap to the patterns on the tester.
Data is supplied by a DieRecovery Tracker name or a binary string.

#### Usages
ConfigureIpForRecovery(--pinmap *DieRecoveryPinMapName* --tracker *DieRecoveryTrackerName* --patlist *PatListName*)   
ConfigureIpForRecovery(--pinmap *DieRecoveryPinMapName* --value *BinaryValue* --patlist *PatListName*)   

#### Arguments   

  --pinmap:     [required] The DieRecovery PinMap name to use.

  --tracker:    The DieRecovery Tracker holding the data to use.

  --value:      The binary data to use instead os in additional to the tracker data.
                If both tracker and value are used the function will run a OR bitwise operation.

  --patlist:    [optional] The PatternList to modify (default will modify only plists used in the current test instance). Use "--patlist global" to modify all plists.  

----

### MaskIP  
This function runs the IPinMap.GetMaskPins() function for the given tracker and returns the list of pins to mask.
Return value is a comma-separate list of pins, suitable to be passed to an EVG template as a mask_pins parameter.
It might result in PatConfig depending on which PinMapDecoder is used.  

#### Usages  
MaskIP(--tracker CORE0,CORE1,CORE2,CORE3 --pinmap CORE0_NOA,CORE1_NOA,CORE2_NOA,CORE3_NOA --gsds G.U.S.PinsToMask)   
G.U.S.PinsToMask = MaskIP(--tracker=TRACKER --pinmap=PINMAP)   
MaskIP(--value 00000 --pinmap MyPinMap)   

#### Arguments  
  --pinmap:     [required] The DieRecovery PinMap name to use.

  --tracker:    The DieRecovery Tracker holding the data to use.

  --value:      The binary data to use instead os in additional to the tracker data.
                If both tracker and value are used the function will run a OR bitwise operation.

  --gsds:       The name of the GSDS token to write the results into. (of the form
               G.U.S.TokenName).  

  --maskpins:  Additional mask pins to include to the overall mask.  

#### Examples
```perl
Test SomeEvgTemplate SomeIAInstance
{
    preinstance = "CPD_DEBUG!ExecPrimeCsCallback MaskIP(--tracker CORE0,CORE1,CORE2,CORE3 --pinmap CORE0_NOA,CORE1_NOA,CORE2_NOA,CORE3_NOA --gsds G.U.S.PinsToMask)";
    mask_pins = "G.U.S.PinsToMask";
    ...
}

Test SomeEvgTemplate SomeIAInstanceAlternateFormat
{
    preinstance = "CPD_DEBUG!ExecPrimeCsCallback G.U.S.PinsToMask=MaskIP(--tracker CORE0,CORE1,CORE2,CORE3 --pinmap CORE0_NOA,CORE1_NOA,CORE2_NOA,CORE3_NOA)";
    mask_pins = "G.U.S.PinsToMask";
    ...
}

Test SomeEvgTemplate SomeIAInstanceAlternateFormat
{
    preinstance = "CPD_DEBUG!ExecPrimeCsCallback G.U.S.PinsToMask=MaskIP(--value 0011 --maskpins TDO --tracker CORE0,CORE1,CORE2,CORE3 --pinmap CORE0_NOA,CORE1_NOA,CORE2_NOA,CORE3_NOA)";
    mask_pins = "G.U.S.PinsToMask";
    ...
}

```

----

### DisableIP  
Same as ConfigureIpForRecovery, just aliased to match the new naming conventions.   

----

### WriteTracker   
This function writes a value to a DieRecovery tracker. The data can be specified with a binary string, or 
from a GSDS token, DFF Token, Hdmt UserVariable or a different DieRecovery tracker.

The default will overwrite the tracker, use --merge to do a bitwise-or between the source and destination.  

#### Usages
WriteTracker(--tracker *DieRecoveryTrackerName* --value *literalvalue*)   
WriteTracker(--tracker *DieRecoveryTrackerName* --gsds *gsdstoken*)   
WriteTracker(--tracker *DieRecoveryTrackerName* --dff *dfftoken*)   
WriteTracker(--tracker *DieRecoveryTrackerName* --uservar *uservar*)   
WriteTracker(--tracker *DieRecoveryTrackerName* --src_tracker *DieRecoveryTrackerName*)   
WriteTracker(--tracker *DieRecoveryTrackerName* --reset)   

#### Arguments   

  --tracker:    [Required] The DieRecovery Tracker to write data to.

  --merge:      [Optional] Merge the source into the destination (bitwise-or) instead of overwriting it.

  --noprint:    [Optional] Nothing will be printed to ituff if this option is supplied.  

One of these sources is required:  
  --gsds:       The GSDS token to get the write data from (of the form
               G.U.S.TokenName).

  --dff:        The DFF token to get the write data from (of the form
               OpType:DffTokenName or DieID:OpType:DffTokenName).

  --uservar:    The Hdmt UserVariable to get the write data from (of the form
               Collection.UserVar).

  --value:      The raw binary data to write. (ie. 1001100)

  --src_tracker : A DieRecovery tracker name to get the source data from

  --reset:      Get the value from the InitialValue setting in the DieRecovery Tracker input file.

#### Ituff Printing  

Ituff printing for Writetracker matches the normal ituff printing for DieRecovery Token updates, unless the --noprint option is supplied.  


```
Example Test:  
Test RunCallback Test1_P1
{
    Callback = "WriteTracker";
    Parameters = "--tracker T0,T1 --value 10";
}

Example Ituff:  
2_tname_PVAL_CALLBACKS::Test1_P1::T0|T1
2_strgval_TestResult:b10|Incoming:b00|Outgoing:b10
```
Explanation:   
Incoming = The current value of the --tracker argument.  
TestResult = The value to be written to the tracker, from the --value/--gsds/--dff/--uservar/--src_tracker argument.  
Outgoing = The final updated value of the --tracker argument.  

_
----

### CopyTracker   
This function copies the value of a tracker to a GSDS token, DFF token or an Hdmt UserVariable.

#### Usages
CopyTracker(--tracker *DieRecoveryTrackerName* --gsds *gsdstoken*)   
CopyTracker(--tracker *DieRecoveryTrackerName* --dff *dfftoken*)   
CopyTracker(--tracker *DieRecoveryTrackerName* --uservar *uservar*)   

#### Arguments   
  --tracker:    [Required] The DieRecovery Tracker to get the data from.

  --gsds:       The GSDS token to write data to (of the form G.U.S.TokenName).

  --dff:        The DFF token to write data (of the form DffTokenName or
               DieID:DffTokenName).

  --uservar:    The Hdmt UserVariable to write data to (of the form
               Collection.UserVar).   

----

### CloneTracker   
This function clones an existing traker definition and data into a new tracker.

#### Usages
CloneTracker(--existing_tracker trackerA --new_tracker trackerB)   

#### Arguments   
  --existing__tracker:    [Required] Existing tracker name to be cloned.
  --new__tracker:         [Required] New tracker name.

----

### LoadPinMapFile   
Loads PinMapDecoder file. See the DDG_Prime_Die_Recovery documentation for details on what types are available.

#### Usages
LoadPinMapFile(--decoder *pinmapdecoder* --file *file.json*)

#### Arguments   

  --decoder:    [Required] The PinMapDecoder type to load.

  --file:       [Required] The JSON file to load. This file should contain the json representation of a list of *pinmapdecoder* objects.

----

### RunRule   
This function runs the given die recovery rule using the named tracker and returns either the first passing NAME, SIZE or BITVECTOR in the given GSDS token.

#### Usages

RunRule(--tracker *DieRecoveryTrackerName* --rule *rulename* --gsds *gsdstoken*)   
RunRule(--tracker *DieRecoveryTrackerName* --rule *rulename* --gsds *gsdstoken* --store_value NAME)   
RunRule(--tracker *DieRecoveryTrackerName* --rule *rulename* --gsds *gsdstoken* --store_value SIZE)   
RunRule(--tracker *DieRecoveryTrackerName* --rule *rulename* --gsds *gsdstoken* --store_value BITVECTOR)   

#### Arguments   
  --tracker:    [Required] The DieRecovery Tracker to use.

  --rule:       [Required] The DieRecovery Rule to execute.

  --gsds  [Required] The GSDS Token to store the result in. Of the form G.[UL].[SI].TokenName.

  --store_value [Optional, Default=NAME] The value to store. Either NAME (stores the name of the first passing rule), SIZE (stores the size of the first passing rule), or BITVECTOR (stores the full bitvector of the first passing rule).

#### Example (exit port 1 if 4C, port 2 if 2C, port 0 otherwise)

<span style="font-family:monospace; font-size:8; color:blue">
Test iCAuxiliaryTest ValidateIaCoreRecovery  <br>
{  <br>
&nbsp;&nbsp;&nbsp;&nbsp;preinstance = "CPD_DEBUG!ExecPrimeCsCallback RunRule(--tracker CORE0,CORE1,CORE2,CORE3,CORE4,CORE5,CORE6,CORE7 --rule CoreDefeaturingVector --gsds G.U.S.PassingRuleName)";  <br>
&nbsp;&nbsp;&nbsp;&nbsp;data_type = "STRING";  <br>
&nbsp;&nbsp;&nbsp;&nbsp;expression = "G.U.S.PassingRuleName";  <br>
&nbsp;&nbsp;&nbsp;&nbsp;result_port = "[R]=='4C'|1^[R]=='2C'|2^0";  <br>
}  <br>
</span>


<br><br><br>
