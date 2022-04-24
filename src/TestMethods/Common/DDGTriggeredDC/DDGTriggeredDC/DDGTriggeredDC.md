# DDGTriggeredDC Template


| Contents      |    
| :----------- |  
| 1. [Introduction](#introduction)      |   
| 2. [Test Instance Parameters](#test-instance-parameters)   |   
| 3. [TPL Samples](#tpl-samples)   |   
| 4. [Exit Ports](#exit-ports)   |  

###

----  
## 1. Introduction  
This is a simple extension to the PrimeTriggeredDcTestMethod which provides the ability to save the measured results to GSDS/SharedStorage.  

----  
## 2. Test Instance Parameters  

#### 2.1 PrimeTriggeredDCTestMethod Test Instance Parameters  
See the [Prime Wiki](https://dev.azure.com/mit-us/PrimeWiki/_wiki/wikis/PrimeWiki.wiki/33623/Readme) for details on the base parameters.  

#### 2.1 Additional DDGTriggeredDC Test Instance Parameters  

| Parameter Name       | Required? | Type | Comments |  
| :-----------         | :----------- | :----------- | :----------- |   
| SaveResults          | Yes | CommaSeparatedString | List of GSDS tokens to save the results to. Needs to be one GSDS per measurement. Use the Evergreen 'G.U.[DS].Token' format. Can be saved to Double or String types. For multiple measurements it will be Pin first, eg Pin1Result1,Pin1Result2,...,Pin1ResultN,Pin2Result1,...,Pin2ResultN |  

----  
## 3. TPL Samples  

###  

Example of taking 2 measurements on 3 pins.
```csharp
Test PrimeTriggeredDcTestMethod ANALOGMEASURE_ExamplePrime_onemeasurement
{
    LevelsTc = "IP_CPU::IP_CPU_BASE::DDR_FIVR_IMON_lvl_nom_lvl";
    Patlist = "IP_CPU::avppn_chk_gt0_nmosoff_plist";
    TimingsTc = "IP_CPU::IP_CPU_BASE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d100";
    Pins = "VCC_IN_HC,FIVR_PROBE_ANA_0,FIVR_PROBE_ANA_1";
    MeasurementTypes = "Current,Voltage,Voltage";
    LowLimits = "-4A,-0.1V,-0.1V";
    HighLimits = "20A,1.3V,1.3V";
    TriggerMapName = "FIVR_IMON_TriggerMap";
    LogLevel = "PRIME_DEBUG";
    DatalogLevel = "All";
    SaveResults = "G.U.D.Icc0,G.U.D.Icc1,G.U.D.VAVPP0,G.U.D.VAVPP1,G.U.D.VAVPN0,G.U.D.VAVPN1"
}
```


----  
## 4. Exit Ports  

| Exit Port       | Condition   | Description |   
| -----------     | ----------- | ----------- |    
| -2  | Alarm | Any HW alarm condition |    
| -1  | Error | Any software error |   
| 0   | Fail  | No change from base PrimeTriggeredDCTestMethod. |   
| 1   | Pass  | No change from base PrimeTriggeredDCTestMethod. |   

###

----  
