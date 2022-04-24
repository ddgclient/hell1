# DDGCapturePacketsTC Test Class  


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

This testclass is meant to replace the Evergreen iCCapturePacketsTest template.
It executes a pattern list, captures CTV data, processes the data and saves it to a GSDS token 
where it can be used by the iCRepair test template.  

To use a GSDS as input for the iCRepair tempate, set "mode_of_execution" = "simulation" and "input_test" to the gsds token.  

iCCapturePacketsTest supports 4 modes - PER_PIN, PER_VECTOR, PACKETS, and PACKETS_DFM.  
**This template only supports PER_PIN mode.**  
If you require any other mode, submit an [issue](https://gitlab.devtools.intel.com/ddg-client-prime-code-base-tgl/tgl_poc_code/-/issues) to request it.

----  

## 2. Test Instance Parameters  

| Parameter Name       | Required? | Description |  
| :-----------         | :----------- | :----------- |      
| Patlist       | Yes | PatternList to run.  |  
| TimingsTc     | Yes | Timings TestCondition to use.  |  
| LevelsTc      | Yes | Levels TestCondition to use.  |  
| MaskPins      | No  | Comma separated list of pins to mask.  |  
| ExecutionMode | Yes | The Templates mode. Currently only PER_PIN is supported. See [Execution Modes](#execution-modes) for details  |  
| DataPins      | Yes | Comma separated list of pins to capture data from.  |  
| OutputGsds    | Yes | GSDS token to save the captured data to. Format=G.[LUI].S._tokenname_ -- where [LUI] specifies the context, L for Lot, U for DUT/Unit or I for IP (Prime only).  |  
| TotalSize     | No  | Expected size of the final captured data. If this is specified and it mismatches, the template exits port 0. If its not specified, no check is done. |  


----  

### 2.1. Execution Modes  

#### 2.1.1 PER_PIN    

This mode simply concatenates all the CTV data from the DataPins into a single string. 
All the data for the first DataPin is first, then all the data for the 2nd DataPin, etc...  

_Example_:  
DataPins = "P002,P004,P006";  

CapturedData:   

| Vector | P002 | P004 | P006 |
| :---   | :--- | :--- | :--- |
|  45    |   1  |   0  |   0  |
|  46    |   1  |   0  |   1  |
|  47    |   1  |   0  |   1  |
|  48    |   1  |   0  |   0  |
|  49    |   1  |   0  |   1  |

Result: 111110000001101 (P002 + P004 + P006)  
In this case, if TotalSize was specified it should be 15 (3 Pins * 5 vectors)  

----  

## 3. Datalog output  

The first failure will be logged using the standard format.  
No other information is saved to ituff.  

```
2_tname_PVAL_COMMON::DDGCapturePackets_NonAmbleFail_P1
2_category_1
2_fdpmv_571
2_fcpmv_-1
2_fsdmv_-1
2_pttrn_CPU_TAP_ALL:g0019815W4480630A_92_VC28xCA066_Rm000000xxx0v_nxxx080808xxxxxxxxxxx_sC28A6PxxBTC002J052_x07_A07_CDU_CDIE_TAP_TAPLINKCFG_DR_OVRSHIFT:drv_cdie_maintap_bbs_list
2_vcont_10571
2_faildata_4024

```

###  

----  

## 4. TPL Samples  

###  

```csharp
Test DDGCapturePacketsTC DDGCapturePackets_MultiPin_P1
{
    LevelsTc = "__main__::bf_lvl_nom_lvl";
    Patlist = "drv_cdie_maintap_bbs_list";
    TimingsTc = "__main__::cpu_stf_timing_tclk100_sclk100_cclk100";
    ExecutionMode = "PER_PIN";
    DataPins = "YY_TEST_PORT_OUT_C2S_00,YY_TEST_PORT_OUT_C2S_01";
    OutputGsds = "G.U.S.CP1";
    TotalSize = 4;
}

```


----  

## 5. Exit Ports  

| Exit Port       | Condition   | Description |   
| -----------     | ----------- | ----------- |    
| -2  | Alarm | Any HW alarm condition |    
| -1  | Error | Any software error |   
| 0   | Fail  | The total bits captured is 0 or does not match the TotalSize parameter. |   
| 1   | Pass  | Captured data has at least a one "1" bit, total bits captured matches the TotalSize parameter and no Amble patterns failed. Its possible a main pattern failed (would be logged to ituff)  |   
| 2   | Fail  | All bits captured are "0" or an Amble pattern failed (preamble, postamble, midamble). The GSDS token is still written in this case. |   

###

----  
