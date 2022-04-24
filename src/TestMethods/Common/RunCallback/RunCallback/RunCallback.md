# RunCallback Template

### Rev 0

| Contents      |    
| :----------- |  
| 1. [Introduction](#introduction)      |   
| 2. [Test Instance Parameters](#test-instance-parameters)   |   
| 3. [TPL Samples](#tpl-samples)   |   
| 4. [Exit Ports](#exit-ports)   |  

###

----  
## 1. Introduction  
This template can be used to execute any Prime Callback function.
It works the same as an iCUserFuncTest instance, but the callback will print debug messages correctly based on the LogLevel parameter. 
However, this function cannot call evergreen UF code.

----  
## 2. Test Instance Parameters  

| Parameter Name       | Required? | Type | Values |  Comments |  
| :-----------         | :----------- | :----------- | :----------- | :----------- |   
| Callback             | Yes | String |  | Name of the callback to execute. |  
| Parameters           | No  | String |  | Parameters to send to the callback. |  
| ResultToken          | No  | String |  | GSDS (of the form G.[ULI].S.TokenName) to hold the return value from the callback. |  
| ResultPort           | No  | String |  | Expression to determine exit port. Expected to be solved as int. Uses [R] as the return value from the callback. See AuxiliaryTC for expression examples.|  

----  
## 3. TPL Samples  

###  

```perl
Test RunCallback BASEPRIM_X_FUNC_K_BEGIN_X_X_X_X_CORETOTRACKER_ATOM  
{
    Callback = "WriteTracker";  
    Parameters = "--tracker ATOM_M0,ATOM_M1 --value 00000000";  
}  

Test RunCallback SOMETESTNAME  
{  
    Callback = "ExecuteInstance";  
    Parameters = "--test SOMEOTHERTESTINSTANCE --save_exit_port G.U.S.ExitPort";  
    ResultPort = "ToInt32([G.U.S.ExitPort])==1?1:0"    
}  

```


----  
## 4. Exit Ports  

| Exit Port       | Condition   | Description |   
| -----------     | ----------- | ----------- |    
| -2  | Alarm | Any HW alarm condition |    
| -1  | Error | Any software error |   
| 0   | Fail  | nothing causes this today. |   
| 1   | Pass  | If the callback was successfully executed. |   

###

----  
