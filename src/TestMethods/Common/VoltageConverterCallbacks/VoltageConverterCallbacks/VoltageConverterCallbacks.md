# DDG Prime Voltage Converter Callbacks  

## Introduction
The main purpose of this callback is to provide a common methodology for all EVG and Prime test clases to apply different options from DDG Prime Voltage Converter service.<br>
Please also use Prime VoltageService documentation as reference:<br>
https://dev.azure.com/mit-us/PrimeWiki/_wiki/wikis/PrimeWiki.wiki/42240/Readme

## Enabling the Callback
Add CallbacksManager in INIT flow.

## Using callback in Prime PrePlist Common Parameter
Once your callback is registered you can call it as common parameter callback at your test instance. Since this callback is intended to modify the voltage condition is recommended to run as **PrePlist** <br>

`
PrePlist = "VoltageConverter(--railconfigurations pinattributes powermux --dlvrpins pinName --fivrcondition NOM --overrides CORE0:0.4)";
`


## Using callback in Evg using CPD_DEBUG!ExecPrimeCsCallback 
CPD_DEBUG!ExecPrimeCsCallback  is able to call any registered Prime C# callback as regular UserFunction. Same as in Prime the recommeded use is **preplist**

`
preplist = "CPD_DEBUG!PrimeCsCallbacks G.U.S.CallbackResult=VoltageConverter(--railconfigurations pinattributes powermux --dlvrpins pinName --fivrcondition NOM --overrides CORE0:0.4)";
`

## Using callback from VminTC
VminTC has built-in voltage converter capability which means this callback is not required as PrePlist. User should use VoltageConverter parameter instead.

**Important: VminTC FivrCondition must be specified using FivrCondition parameter instead of command line option.**

`
VoltageConverter = "--railconfigurations pinattributes powermux --dlvrpins VCCIA --expresssions CCF_ONLY --overrides CORE0:0.4)";
`

## CommandLine Options and Switches

### --railconfigurations=[configuration1 ... configurationN]
Sets list of rail configurations to be applied when voltage gets applied. Refer to Prime Voltage Service documentation for configuration.
### --dlvrpins=[pin1 ... pinM]
Sets list of dlvr pins to set based on *.voltageDomains.json* ALEPH configuration file.
### --expressions=[expr1 ... exprN]
Sets list of override expressions for dlvr pins. Number of expressions have to match the number of dlvrpins.<br>
If parameter is not being set. The first expression on the list will be set as default.
### --fivrcondition=[name]
FivrCondition to be applied. Configuration is in ALEPH input files. Option is not allowed from VminTC.
### --overrides=[domain1:value1,...,domainN:valueN]
Comma-separated list of domain:value to be overrided from test condition.  
Supported values for values, evaluated in this order:  
1.  Literal double value: --overrides=CORE0:1.2  
2.  SharedStorage token: --overrides=CORE0:TokenName  
3.  UserVar: --overrides=CORE0:Collection.UserVar  
3.  VminForwardingCorner using FlowDomain: --overrides=CORE0:FlowDomain^CR0@F1  
4.  VminForwardingCorner using FlowIndex parameter: --overrides=CORE0:CR0@F1  
5.  VminForwardingCorner Interpolation using Frequency (in GHz): --overrides=CORE0:CR0@2.75  

