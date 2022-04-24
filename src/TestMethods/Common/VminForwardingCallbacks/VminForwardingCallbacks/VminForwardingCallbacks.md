# DDG Prime VminForwarding Callbacks
### Rev 0

[VminSearchStore](#vminsearchstore)

[VminInterpolation](#vmininterpolation)

[LoadVminFromDFF](#loadvminfromdff)

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
Test iCUserFuncTest ExampleCallbackUF<br>
{<br>
&nbsp;&nbsp;&nbsp;&nbsp;function_name = "CPD_DEBUG!ExecPrimeCsCallback";<br>
&nbsp;&nbsp;&nbsp;&nbsp;function_parameter = "CallbackName(--arg1 val1 --arg2 val2)";<br>
}<br>
</span>


## Available Functions

----

### VminSearchStore   
This function saves the current vmins for the given domains (or all domains if none are specified).
It is meant to save a snapshot of the vmins after the "Search" flows are run but before the "Check" flows.

#### Usages   
VminSearchStore(--domains *ListOfDomains*)   
VminSearchStore()   

#### Arguments   

  --domains:     [optional] A comma separated list of Domain names. These domains will have all their current vmin data stored to a "snapshot". This should be the main domain names, not the instance names (ie CR, CRF ... not CR0, CR1...)

###

----

### VminInterpolation   
This function runs the Search-To-Check interpolation on the given vmin domains.
The corners supplied are used to run interpolation on the other corners.
It is meant to update the vmin forwarding values for corners that do not run "Check" flows by using data for the corners that did.
It requires "VminSearchStore" to have been run.

#### Usages
VminInterpolation(--domains *ListOfDomains* --corners *ListOfCorners* --flow *GSDSFlowToken*)   

#### Arguments   

   --domains:     [required] A comma separated list of Domain names to run interpolation on. This should be the main domain names, not the instance names (ie CR, CRF ... not CR0, CR1...)   

   --check_corners:     [required] A comma separate list of Corner names with 'check' data. (ie this is the list of corners that did NOT have -stc_interpolation=true in the FastInfra.xml file and will be used to run interpolation on the other corners.).   

   --flow:        [required] GSDS token (of the form G.U.I.tokenname or G.U.S.tokenname) containing the current/passing flow number. It must be an integer but it can be stored in a String GSDS token.  

----

#### Examples
<span style="font-family:monospace; font-size:8; color:blue">
Test iCUserFuncTest IaCoresVminSnapshot<br>
{<br>
&nbsp;&nbsp;&nbsp;&nbsp;function_name = "CPD_DEBUG!ExecPrimeCsCallback";<br>
&nbsp;&nbsp;&nbsp;&nbsp;function_parameter = "VminSearchStore(--domains CR,CRF,CRX2,CRX3)";<br>
}<br>
</span>
<br><br>
<span style="font-family:monospace; font-size:8; color:blue">
Test iCUserFuncTest IaCoresStcInterpolation<br>
{<br>
&nbsp;&nbsp;&nbsp;&nbsp;function_name = "CPD_DEBUG!ExecPrimeCsCallback";<br>
&nbsp;&nbsp;&nbsp;&nbsp;function_parameter = "VminInterpolation(--domains CR,CRF,CRX2,CRX3 --check_corners F1,F3,F6 --flow G.U.I.PassingFlow)";<br>
}<br>
</span>

### LoadVminFromDFF   
This function loads DFF values for VminData into SharedStorage tokens to be used by the VminForwarding infrastructure. 
The names of the DFF and sharedstorage tokens must be setup by an instance of the VminForwardingBase testclass. 
See the documentation for DDG_Prime_Vmin_Forwarding for more details.

#### Usages   
LoadVminFromDFF()   

#### Arguments   

None.

###

