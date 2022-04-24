# DDG Prime Debug Callbacks
### Rev 0

[ApplyEndSequence](#applyendsequence)

[ApplyPatternTriggerMap](#applypatterntriggermap)

[BitVectorPatConfigSetPoint](#bitvectorpatconfigsetpoint)

[DisableSmartTC](#disablesmarttc)

[EnableSmartTC](#enablesmarttc)

[ExecuteInstance](#executeinstance)

[ExecuteNoCapturePlist](#executenocaptureplist)

[ExecutePatConfig](#executepatconfig)

[ExecutePatConfigSetPoint](#executepatconfigsetpoint)

[EvaluateExpression](#evaluateexpression)

[FlushAllSmartTCCategories](#flushallsmarttccategories)

[FlushSmartTCCategory](#flushsmarttccategory)

[MirrorDff](#mirrordff)

[PrintDff](#printdff)

[PrintSharedStorage](#printsharedstorage)

[PrintToItuff](#printtoituff)

[ProcessAlephFiles](#processalephfiles)

[Sleep](#sleep)

[SetAltInstanceName](#setaltinstanceName)

[SetCurrentDieId](#setcurrentdieid)

[SetPinAttributes](#setpinattributes)

[SetPowerUpTCName](#setpoweruptcname)

[ValidatePatternTriggerMap](#validatepatterntriggermap)

[WriteDff](#writedff)

[WriteSharedStorage](#writesharedstorage)

[WriteUserVar](#writeuservar)
##   

----

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

### WriteSharedStorage   
This function writes the given value to the sharedstorage or gsds token.
The value can be from a literal value for from another GSDS Token or a UserVar.  

#### Usages
WriteSharedStorage(--token *sharedstoragename* --value *value*)   

#### Arguments   

  --token:     [required] The SharedStorage or GSDS token to write to, in GSDS format (see [GSDS Token Format](#gsds-token-format)).

  --value:     [required] The value to write. Can be a GSDS token (see [GSDS Token Format](#gsds-token-format)) or an Hdmt UserVariable (of the form Collection.UserVar) or a literal value.  

#### Examples
WriteSharedStorage(--token G.U.S.DestToken --value G.L.I.SourceToken) -> G.L.I.SourceToken will be read and its value stored in G.U.S.DestToken   
WriteSharedStorage(--token G.U.S.DestToken --value FUSEUFGL.UFGL_Resort) -> The UserVar GFUSEUFGL.UFGL_Resort will be read and its value stored in G.U.S.DestToken   
WriteSharedStorage(--token G.L.I.DestToken --value 45) -> The value 45 will be written to G.L.I.DestToken   

Note: If the UserVar/Gsds Token doesn't exist then it will be treated like any other literal data.  
WriteSharedStorage(--token G.U.S.DestToken --value G.L.S.NotaToken)
-> if G.L.S.NotaToken doesn't exist then G.U.S.DestToken will be written to "G.L.S.NotaToken".  

#### GSDS Token Format

Format=**G.*scope*.*type*.*tokenname***

*scope* =  **U** for unit/dut, **L** for lot, or **I** for Ip. (I is only valid for sharedstorage)

*type* = **S** for string, **D** for double, or **I** for integer.  

Examples= G.U.S.SliceTracking

----

### PrintSharedStorage   
This function reads the given sharedstorage or gsds token and prints its value to the console. The token can be a comma separated list of token names. If no token is given, all shared storage values will be printed.

#### Usages
PrintSharedStorage()   
PrintSharedStorage(--token *sharedstoragename1*)   
PrintSharedStorage(--token *sharedstoragename1*,*sharedstoragename2*,*sharedstoragename3*)   

#### Arguments   

  --token:     [optional] The SharedStorage or GSDS token (or comma separate list of tokens), in GSDS format (see [GSDS Token Format](#gsds-token-format)).

----

### EnableSmartTC   
This callback enables SmartTC.

#### Usages
EnableSmartTC()   

----

### DisableSmartTC   
This callback disables SmartTC.

#### Usages
DisableSmartTC()   

----

### ApplyEndSequence   
This callback applies EndSequence as defined in tpl.

#### Usages
ApplyEndSequence()   

----

### SetPowerUpTCName   
This callback sets a new PowerUpTC name. 
#### Arguments   
Test condition name.
#### Usages
SetPowerUpTCName(*testcondition*)   

----

### FlushAllSmartTCCategories   
This callback flushes **ALL** SmartTC categories.

#### Usages
FlushAllSmartTCCategories()   

----

### FlushSmartTCCategory   
This callback enables SmartTC.

#### Arguments   
Test condition category. LEVELS_SETUP, LEVELS_POWER_ON, LEVELS_POWER_DOWN, TIMING, RELAY and THERMAL
#### Usages
FlushSmartTCCategory(*category*)   

----  

### ExecuteInstance   
This callback executes a comma-separated list of test instances and returns their exit ports
(as a comma-separated string).

#### Usages  
ExecuteInstance(--test PkgTest1,PkgTest2 --save_exit_port G.U.S.ExitPorts)   
ExecuteInstance(--test IP_CPU::IPTest1 --exception_on_fail)   

#### Arguments   

  --test:                [required] Comma-separated list of tests to run.   

  --save_exit_port:      [optional] GSDS token to save the exit ports of the tests (comma-separated for multiple tets).  

  --exception_on_fail:   [optional] If any of the executed tests exits through a port < 1, an exception is thrown which will cause the calling instance to exit port -1.   

----   
### ExecuteNoCapturePlist   
Executes a plist without loading test conditions or setting any capture mode.

#### Usages  
ExecuteNoCapturePlist(plist_name)   

#### Arguments   

  plist_name:            [required] plist name. <br>
**Important**: needs to be compatible with current instance levels and timings (same timing and power domains).

----   

### ExecutePatConfig   
This callback executes list of Prime PatConfig configurations for local plist only.

#### Usages  
ExecutePatConfig(Configuration1:Data1,...,ConfigurationN:DataN)   

#### Data Format   
Hex: 0xFF  
Decimal: 0d8  
Binary: 0b1011, 1011  
Reverse data: 0x0F'r  

#### Example
ExecutePatConfig(Config1:LLHH,Config2:0x7F)

----   

### ExecutePatConfigSetPoint   
This callback executes list of Prime PatConfig SetPoints.

#### Usages  
ExecutePatConfigSetPoint(Module1:Group1:SetPoint1,...,ModuleN:GroupN:SetPointN)   --> Local plist only.
ExecutePatConfigSetPoint(Module1:Group1:SetPoint1,...,ModuleN:GroupN:SetPointN:Global)   --> Apply to all matching patterns

#### Example
ExecutePatConfigSetPoint(MGFun:GTRatio:1.5GHz,MScn:SomeFuse:Enabled)

----   

### BitVectorPatConfigSetPoint   
This callback executes a different Prime PatConfig SetPoint for each bit in a binary string.
The name of the SetPoint will be generated from a user supplied template which can include the index and value of the bit.  

Usage: BitVectorPatConfigSetPoint(--bitvector _token_ --setpoint _module_:_group_:_setpoint_ [--msb_first] [--value_map _map_] [--index_width _size_])  

#### --bitvector _token_   
Specifies the binary string to use.
_token_ is a comma-separate list of literal binary strings and/or GSDS tokens containing binary strings.  

#### --setpoint _module_:_group_:_setpoint_   
#### --setpoint _module_:_group_  
Specifies the PatConfigSetPoint to be executed for each bit in the bitvector.   
   * _module_ is the SetPoint module.   
   * _group_ is the SetPoint group.  
   * _setpoint_ is the SetPoint to execute. If it is omitted the default will be used.   

** Any of the above can contain the variables %INDEX% (will be replaced with the index of the current bit) and/or %VALUE% (will be replaced with the value of the current bit)  

#### --value_map 0:valueA,1:valueB [optional]  
This provides alternative data to use in the %VALUE% replacement. valueA will be used when the bit is 0, valueB will be used when the bit is 1  

#### --msb_first [optional]  
By default the left-most binary bit is considered bit 0 for the %INDEX% replacement. With this option, the right-most binary bit will be bit 0.  

#### --index_width size [optional]  
Sets the minimum width for %INDEX% replacements (left-padded with 0). By default %INDEX% will be replaced with 0,1,2,3, but with --index_width 3 it would be 000, 001, 002, 003,    

#### Example  

BitVectorPatConfigSetPoint(--bitvector G.U.S.Tracker1 --setpoint FUNC:CoreStatus:Core%INDEX%_%VALUE%  --value_map 0:STROBE,1:MASK)  
* If G.U.S.Tracker1="1011" then the following setpoints are executed:  
   * FUNC:CoreStatus:Core0_MASK   
   * FUNC:CoreStatus:Core1_STROBE   
   * FUNC:CoreStatus:Core2_MASK   
   * FUNC:CoreStatus:Core3_MASK   


----

### Sleep   
This callback executes thread sleep for the specified number of mS.

#### Usages  
Sleep(time)

#### Example
Sleep(10)

----   

### SetPinAttributes
Executes SetPinAttributeCallback. User should follow TOS rules.
#### --prepause [time in mS]
Pause before callbacks execution.
#### --postpause [time in mS]
Pause after callbacks execution.
#### --settings [pin1:attribute1:value1 pinN:attributeN:valueN]
List of settings to apply.
#### Example:
           SetPinAttributes("--prepause=1 --postpause=2 --settings=PinA:VForce:1.1");

----   
### ApplyPatternTriggerMap
This callback applies a pre-validated triggermap for a given plist.

#### Usages  
ApplyPatternTriggerMap(TriggerName,PlistName)

----   

### ValidatePatternTriggerMap
This callback applies a validates triggermap for a given plist.

#### Usages  
ValidatePatternTriggerMap(TriggerName,PlistName)

----   

### ProcessAlephFiles
This callback processes a list of ALEPH files separated by semicolon (;) regardless if they were defined in ENV.

#### Usages  
ProcessAlephFiles(FileName)

----   

### MirrorDff
Mirrors a list of DFF tokens into ShareStorage using MD_token_optype_targetdie as key.

#### Usages  
MirrorDff(--tokens=token1,token2 --optype=optype --targetdie=dieId),
targetdie and optype are optional fields.

----   

### PrintDff
Prints a list of DFF tokens to console while log level is set.

#### Usages  
PrintDff(--tokens=token1,token2 --optype=optype --targetdie=dieId),
targetdie and optype are optional fields.

----   

### WriteDff
Writes a DFF token.

#### Usages  
WriteDff(--token=token --value=value --targetdie=dieId), targetdie is an optional field.

----   

### SetAltInstanceName
Sets the alternative test instance name. Equivalent to Prime.Services.DatalogService.SetAltInstanceName(NAME)  

#### Usages  
SetAltInstanceName(NAME)  

----   

### SetCurrentDieId
Sets the current DieID for DFF operations. Equivalent to Prime.Services.DffService.SetCurrentDieId(dieId)  

#### Usages  
SetCurrentDieId(dieId)  

----   


### WriteUserVar   
This function writes the given value to the Hdmt UserVariable.
The value can be from a literal value, a GSDS Token or another UserVar.  

#### Usages
WriteUserVar(--uservar *uservariable* --value *value* --type *type_of_uservar*)   

#### Arguments   

  --uservar:   [required] The UserVariable to write to, in the format _collection_._varname_

  --value:     [required] The value to write. Can be a GSDS token (see [GSDS Token Format](#gsds-token-format)) or an Hdmt UserVariable (of the form _collection_._varname_) or a literal value.  

  --type:      [required] The Hdmt Type of the destination UserVar (applies to the --value if that is a UserVar as well). One of Boolean, Double, Integer, String, ArrayBoolean, ArrayDouble, ArrayInteger, or ArrayString. When writing to an Array type, input should be either a UserVar of the same type or a comma separated string.   

#### Examples
WriteUserVar(--uservar RecoveryVars.IsDownBin --value False --type Boolean)   
WriteUserVar(--uservar ARR_COMMON_CXX.ARR_ARIQD0_SCBD_0_0_0_1 --value 1 --type Integer)   
WriteUserVar(--uservar SCVars.SC_WAFERID --value G.U.I.Wafer --type String) --> If the source is not a UserVar, then its type does not have to match the destination type, as long its valid (eg can't store NOTANUMBER as a Double). In this case, the Integer stored in the GSDS will be stored as a String UserVar.   

### PrintToItuff   
This function writes the given values to the Ituff datalog.  

#### Usages
PrintToItuff(--body_type *type* --body_data *data* [--tname_suf *tnamepostfix*])   

#### Arguments   

  --body_type:   [required] The Ituff format. Must be one of mrslt, strgval or rawbinary_msbF.

  --body_data:   [required] The value to write. Can be a GSDS token (see [GSDS Token Format](#gsds-token-format)) or an Hdmt UserVariable (of the form _collection_._varname_) or a literal value. Multiple comma separated values will be concatenated together (or added in the case of mrslt format)  

  --tname_suf:   [optional] A string to add to the end of the testname on the tname line.     

#### Examples

Example Input for a single token:  
```perl
PrintToItuff(--body_type strgval --body_data G.U.S.Token1 --tname_suf _SomePostFix)  
```
Output:
```perl
2_tname_MyTestName_SomePostFix
2_strgval_DataThatWasInToken1
```

Example Input for multiple tokens:  
```perl
PrintToItuff(--body_type strgval --body_data G.U.S.Token1,|,G.U.S.Token2,|,G.U.S.Token3 --tname_suf _SomePostFix)  
```
Output:
```perl
2_tname_MyTestName_SomePostFix
2_strgval_DataThatWasInToken1|DataThatWasInToken2|DataThatWasInToken3
```


### EvaluateExpression   
This function evaluates a numerical expression and saves the result to a GSDS, UserVar or DFF token.  
See the AuxiliaryTC testclass for details about the Expression engine and available operations.  

#### Usages
EvaluateExpression(--result _resultToken_ --datatype _datatype_ --storagetype _storagetype_ --expression -- _expression_)   

#### Arguments   

  --result:      [required] The token to write with the result. Can be a GSDS token (see [GSDS Token Format](#gsds-token-format)) or an Hdmt UserVariable (of the form _collection_._varname_), or a DFF token (just the token name, will be written using the current optype and die_id).  

  --storagetype: [required] The type of the result token. Must be GSDS, UserVar or DFF.       

  --datatype:    [required] The data type of the result. Must be String, Integer or Double.  

  --expression:  [required] The mathematical expression to evaluate. Can contain GSDS, UserVar or DFF tokens of the form [token] to be evaluated. If the expression contains a minus (-) sign, then it must be the last argument, and it must be of the form "--expression -- _expression_". Otherwise it can be anywhere and doesn't require the extra --  

#### Examples

* Subtract a DFF and UserVar and store the result in a GSDS. Since the operation includes subtracting, the expression must be at the end and include --expression --   
EvaluateExpression(--result G.U.D.OffsetValue --storagetype gsds --datatype double --expression -- [DffVMinToken] - [GBVars.VminGuardband]")

* Add a literal value to a GSDS and update the same token. Since the operation is +, the --expression can be anywere.  
EvaluateExpression(--result G.U.D.Token --expression G.U.D.Token + 0.1 --storeagetype gsds --datatype double)  


----
